using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.ComponentModel.Design;
using System.Windows.Forms;
using uIP.Lib.Script;
using System.Security.Cryptography;

namespace uIP.Lib
{
    /// <summary>
    /// The set of UDataCarrier
    /// </summary>
    public class UDataCarrierSet : IDisposable
    {
        public string _strID = null;
        public string[] _AddiInfo = null;
        public UDataCarrier[] _Array = null;

        public UDataCarrierSet() { }
        public UDataCarrierSet( string id, UDataCarrier[] arr, string[] addi ) { _strID = id; _Array = arr; _AddiInfo = addi; }

        ~UDataCarrierSet()
        {
            if ( _Array != null )
            {
                UDataCarrier.FreeByIDispose( _Array );
                _Array = null;
            }
        }

        public void Dispose()
        {
            if (_Array != null)
            {
                UDataCarrier.FreeByIDispose( _Array );
                _Array = null;
            }

            GC.SuppressFinalize( this );
        }
    }

    /// <summary>
    /// A type and description of a data carrier
    /// </summary>
    public class UDataCarrierTypeDescription
    {
        protected Type m_Type;
        protected string m_strDesc;

        public Type Tp
        {
            get => m_Type;
            set => m_Type = value;
        }

        public string Desc
        {
            get => m_strDesc;
            set => m_strDesc = string.IsNullOrEmpty( value ) ? "" : string.Copy( value );
        }

        public UDataCarrierTypeDescription()
        {
            m_Type = null;
            m_strDesc = null;
        }

        public UDataCarrierTypeDescription( Type tp )
        {
            m_Type = tp;
            m_strDesc = null;
        }

        public UDataCarrierTypeDescription( Type tp, string desc )
        {
            m_Type = tp;
            m_strDesc = string.IsNullOrEmpty( desc ) ? null : string.Copy( desc );
        }


        public static bool CmpPrefect( UDataCarrierTypeDescription[] t1, UDataCarrierTypeDescription[] t2 )
        {
            if ( t1 == null && t2 == null ) return true;
            if ( t1 == null || t2 == null ) return false;
            if ( t1.Length != t2.Length ) return false;

            for ( int i = 0 ; i < t1.Length ; i++ )
            {
                if ( t1[ i ] == null && t2[ i ] == null ) continue;
                if ( t1[ i ] == null || t2[ i ] == null ) return false;
                if ( t1[ i ].Tp != t2[ i ].Tp ) return false;
            }

            return true;
        }

        public static bool CmpIgnore2nd( UDataCarrierTypeDescription[] t1, UDataCarrierTypeDescription[] t2 )
        {
            if ( (t1 == null && t2 == null) || (t2 == null) ) return true;
            if ( t1 == null || t2 == null ) return false;
            if ( t1.Length != t2.Length ) return false;

            for ( int i = 0 ; i < t1.Length ; i++ )
            {
                if ( t1[ i ] == null && t2[ i ] == null ) continue;
                if ( t1[ i ] == null || t2[ i ] == null ) return false;
                if ( t1[ i ].Tp != t2[ i ].Tp ) return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Define a carrier to carry data
    /// - not suggest to fill unmanaged data
    /// - unmanaged data processing
    ///   - MUST manage the mem in data
    /// - Handleable default is false and less constraint
    /// </summary>
    public class UDataCarrier : UDataCarrierTypeDescription, IDisposable
    {
        public static bool LittleEndian { get; private set; } = false;
        public static bool ShowDialogDisplayError { get; set; } = false;
        private bool _bDisposed = false;
        static UDataCarrier()
        {
            unsafe
            {
                UInt32 v = 1;
                UInt32* ptr32 = &v;
                byte* ptr8 = ( byte* ) ptr32;
                LittleEndian = ptr8[ 0 ] != 0;
            }
        }

        private object m_Data = null;
        private fpUDataCarrierXMLReader m_fpReadIn = null;
        private fpUDataCarrierXMLWriter m_fpWriteOut = null;
        private Action<object> m_fpDataHandler = null;
        /// <summary>
        /// Handle resource by Dispose()
        /// - false -> more flexible, less constraint
        /// - true -> need the class handling resource by Dispose()
        /// - Handleable type
        ///   - Data implement IDisposable
        ///   - Data is IEnumerable and each item implement IDisposable
        /// </summary>
        public bool Handleable { get; set; } = false; 

        public object Data
        {
            get => m_Data;
            set => m_Data = value;
        }
        public fpUDataCarrierXMLReader fpRead
        {
            get => m_fpReadIn;
            set => m_fpReadIn = value;
        }
        public fpUDataCarrierXMLWriter fpWrite
        {
            get => m_fpWriteOut;
            set => m_fpWriteOut = value;
        }
        public Action<object> fpDataHandler
        {
            get => m_fpDataHandler;
            set => m_fpDataHandler = value;
        }

        void HandleResource(bool disposing)
        {
            if ( _bDisposed ) return;
            _bDisposed = true;

            if ( !Handleable || m_Data == null )
            {
                m_Data = null;
                return;
            }

            if ( m_Data != null )
            {
                if ( fpDataHandler != null )
                {
                    fpDataHandler.Invoke( m_Data );
                    fpDataHandler = null;
                    m_Data = null;
                }

                //if ( disposing && m_Data != null )
                if ( m_Data != null )
                {
                    if ( m_Data is IDisposable d )
                        d?.Dispose();
                    else if (m_Data is IDictionary<string, UDataCarrier> dicC)
                    {
                        foreach ( var kv in dicC )
                            kv.Value?.Dispose();
                    }
                    else if ( m_Data is IDictionary<string, List<UDataCarrier>> dic02 )
                    {
                        foreach ( var kv in dic02 )
                        {
                            if ( kv.Value == null || kv.Value.Count == 0 )
                                continue;
                            foreach ( var v in kv.Value )
                                v?.Dispose();
                        }
                    }
                    else if ( m_Data is IDictionary<string, UDataCarrier[]> dic03 )
                    {
                        foreach ( var kv in dic03 )
                        {
                            if ( kv.Value == null || kv.Value.Length == 0 )
                                continue;
                            foreach ( var v in kv.Value )
                                v?.Dispose();
                        }
                    }
                    else if (m_Data is IDictionary<string, object> dicO)
                    {
                        foreach(var kv in dicO)
                            (kv.Value as IDisposable)?.Dispose();
                    }
                    else if ( m_Data is IEnumerable i ) // dictionary must be call idispose
                    {
                        foreach ( var ii in i )
                            ( ii as IDisposable )?.Dispose();
                    }
                }
            }

            m_Data = null;
        }

        ~UDataCarrier()
        {
            HandleResource(false);
        }

        public UDataCarrier() : base() { }
        public UDataCarrier( object data, Type tp ) : base( tp ) { m_Data = data; }
        public UDataCarrier( object data, Type tp, string desc )
            : base(tp, desc)
        {
            m_Data = data;
        }
        public void Dispose()
        {
            HandleResource(true);
            GC.SuppressFinalize( this );
        }

        public T Get<T>(T def, bool takeAway = true)
        {
            try
            {
                if ( m_Data is T v)
                {
                    T r = (T)v;
                    if ( takeAway )
                        m_Data = null;
                    return r;
                }
                return def;
            } catch { return def; }
        }
        public object Get(bool takeAeay = true)
        {
            object r = Data;
            if ( takeAeay )
            {
                Data = null;
                Handleable = false;
            }
            return r;
        }
        public bool Put<T>(ref T t, T def, bool takeAway = true)
        {
            try
            {
                if ( m_Data is T v)
                {
                    t = v;
                    if ( takeAway )
                        m_Data = null;
                    return true;
                }
                t = def;
                return false;
            } catch { t = def; return false; }
        }

        public void Build<T>( T val )
        {
            if (Handleable) (m_Data as IDisposable)?.Dispose(); // free before replace
            m_Data = ( object ) val;
            m_Type = m_Data == null ? typeof( T ) : val.GetType();
        }

        public void Build<T>( T val, string desc )
        {
            if (Handleable) (m_Data as IDisposable)?.Dispose(); // free before replace
            m_Data = ( object ) val;
            m_Type = m_Data == null ? typeof( T ) : val.GetType();
            m_strDesc = String.IsNullOrEmpty( desc ) ? null : String.Copy( desc );
        }

        public bool IsTypeMatching<T>()
        {
            if ( m_Type == null ) return false;
            return (typeof( T ) == m_Type);
        }

        #region Static Methods

        public static bool CheckType<T>( UDataCarrier item )
        {
            if ( item == null || item.Tp == null ) return false;
            return (typeof( T ) == item.Tp);
        }

        public static UDataCarrier MakeOne<T>( T val, bool handleable = false )
        {
            UDataCarrier item = new UDataCarrier( ( object ) val, typeof( T ) ) {
                Handleable = handleable
            };
            return item;
        }

        public static UDataCarrier MakeOne<T>(T val, string desc, bool handleable = false )
        {
            var item = MakeOne( val );
            item.Desc = desc;
            return item;
        }


        public static UDataCarrier Make(object val, bool handleable = false )
        {
            if ( val == null ) return null;
            return new UDataCarrier( val, val.GetType() ) { Handleable = handleable };
        }

        //public static UDataCarrier MakeOne<T>( T val, string desc )
        //{
        //    UDataCarrier item = new UDataCarrier( ( object ) val, typeof( T ), desc );
        //    return item;
        //}

        public static UDataCarrier[] MakeArray( int count )
        {
            if ( count <= 0 ) return null;
            UDataCarrier[] ret = new UDataCarrier[ count ];
            // Give an instance
            for ( int i = 0 ; i < ret.Length ; i++ )
                ret[ i ] = new UDataCarrier();
            return ret;
        }

        public static UDataCarrier[] MakeOneItemArray( object val, Type t )
        {
            UDataCarrier[] ret = new UDataCarrier[ 1 ];
            ret[ 0 ] = new UDataCarrier( ( object ) val, val?.GetType()??t );
            return ret;
        }

        public static UDataCarrier[] MakeOneItemArray<T>( T val )
        {
            UDataCarrier[] ret = new UDataCarrier[ 1 ];
            ret[ 0 ] = new UDataCarrier( ( object ) val, typeof( T ) );
            return ret;
        }

        public static UDataCarrier[] MakeOneItemArray<T>( T val, string desc )
        {
            UDataCarrier[] ret = new UDataCarrier[ 1 ];
            ret[ 0 ] = new UDataCarrier( ( object ) val, typeof( T ), desc );
            return ret;
        }

        public static UDataCarrier[] MakeVariableItemsArray(params object[] varParameters)
        {
            if ( varParameters == null || varParameters.Length <= 0 )
                return null;
            UDataCarrier[] ret = MakeArray( varParameters.Length );
            for(int i = 0 ; i < varParameters.Length ; i++ )
            {
                if ( varParameters[ i ] == null )
                    continue;
                ret[ i ].Build( varParameters[ i ] );
            }

            return ret;
        }
        public static UDataCarrier[] MakeVariableItemsArray<T>(params T[] varParams)
        {
            if ( varParams == null || varParams.Length <= 0 )
                return null;

            UDataCarrier[] ret = MakeArray( varParams.Length );
            for(int i = 0; i < varParams.Length; i++ ) {
                ret[ i ].Data = varParams[ i ];
                ret[ i ].Tp = typeof( T );
            }

            return ret;
        }

        public static bool SetItemDescription<T>( UDataCarrier[] arr, int index_0base, string desc )
        {
            if ( arr == null || index_0base < 0 || index_0base >= arr.Length )
                return false;
            arr[ index_0base ].Tp = typeof( T );
            arr[ index_0base ].Desc = desc;
            return true;
        }

        public static bool SetItem<T>( UDataCarrier[] arr, int index_0base, T value, bool handeable = false )
        {
            if ( arr == null || index_0base < 0 || index_0base >= arr.Length )
                return false;
            arr[ index_0base ].Build<T>( value );
            arr[ index_0base ].Handleable = handeable;
            return true;
        }

        public static bool SetItem<T>( UDataCarrier[] arr, int index_0base, T value, string desc, bool handleable = false )
        {
            if ( arr == null || index_0base < 0 || index_0base >= arr.Length )
                return false;
            arr[ index_0base ].Build<T>( value, desc );
            arr[ index_0base ].Handleable = handleable;
            return true;
        }
        public static void PutItem<T>( UDataCarrier [] arr, int index_0base, T def, ref T to)
        {
            try
            {
                to = GetItem<T>( arr, index_0base, def, out var status );
            } catch { }
        }

        public static bool GetByIndex<T>( UDataCarrier[] arr, int index, T def, out T ret )
        {
            if ( arr == null )
            {
                ret = def;
                return false;
            }
            if ( index < 0 )
                index = arr.Length + index;
            if ( index < 0 || index >= arr.Length )
            {
                ret = def;
                return false;
            }

            if ( arr[ index ] == null || arr[ index ].Data == null )
            {
                ret = def;
                return false;
            }

            try
            {
                if ( arr[index].Data is T v)
                {
                    ret = v;
                    return true;
                }
                ret = def;
                return false;
            }
            catch
            {
                ret = def;
                return false;
            }
        }

        public static bool GetByIndex<T>( UDataCarrier[] arr, int index, out T ret, T defaultV, object ctx, Func<object, UDataCarrier, UDataCarrier> cmp )
        {
            ret = defaultV;
            if ( arr == null || index < 0 || index >= arr.Length )
                return false;
            if ( cmp == null )
                return false;

            // check by cmp delegate
            var chked = cmp( ctx, arr[ index ] );
            if ( chked == null )
                return false;

            try
            {
                if ( arr[ index ].Data is T v )
                {
                    ret = v;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static T GetItem< T >( UDataCarrier[] arr, int index, T def, out bool status )
        {
            status = true;
            if ( arr == null )
            {
                status = false;
                return def;
            }
            if ( index < 0 )
                index = arr.Length + index;
            if (index < 0 || index >= arr.Length)
            {
                status = false;
                return def;
            }

            if ( arr[ index ] == null || arr[ index ].Data == null )
            {
                status = false;
                return def;
            }

            try
            {
                return ( T ) arr[ index ].Data;
            } catch
            {
                return def;
            }

            //return ( T ) arr[ index ].Data;
        }

        public static bool TypesCheck( UDataCarrier[] trg2chk, UDataCarrier[] pattern )
        {
            if ( pattern == null || pattern.Length <= 0 ) return true;
            if ( trg2chk == null || trg2chk.Length < pattern.Length ) return false;

            for ( int i = 0 ; i < pattern.Length ; i++ )
            {
                if ( pattern[ i ] == null ) continue;
                if ( pattern[ i ].Tp != trg2chk[ i ].Tp ) return false;
            }

            return true;
        }

        public static bool TypesCheck( UDataCarrier[] trg2chk, UDataCarrierTypeDescription[] pattern )
        {
            if ( pattern == null || pattern.Length <= 0 ) return true;
            if ( trg2chk == null || trg2chk.Length < pattern.Length ) return false;

            for ( int i = 0 ; i < pattern.Length ; i++ )
            {
                if ( pattern[ i ] == null ) continue;
                if ( pattern[ i ].Tp != trg2chk[ i ].Tp ) return false;
            }

            return true;
        }

        public static object ToSingleValue<SrcT>( SrcT val, Type dstTp )
        {
            Type srctp = typeof( SrcT );
            if ( !srctp.IsValueType || dstTp == null || !dstTp.IsValueType ) return null;
            if ( srctp.IsEnum || dstTp.IsEnum ) return null;
            if ( Marshal.SizeOf( srctp ) != Marshal.SizeOf( dstTp ) ) return null;
            if ( Marshal.SizeOf( srctp ) <= 0 ) return null;

            int sz = Marshal.SizeOf( srctp );
            if ( sz <= 0 ) return null;

            IntPtr mem = Marshal.AllocHGlobal( sz );
            if ( mem == IntPtr.Zero ) return null;

            Marshal.StructureToPtr( val, mem, false );
            object convVal = Marshal.PtrToStructure( mem, dstTp );
            Marshal.FreeHGlobal( mem );

            return convVal;
        }

        public static object ToArray<SrcT>( SrcT val, Type dstTp )
        {
            object got = ToSingleValue<SrcT>( val, dstTp );
            if ( got == null ) return null;

            Array ret = Array.CreateInstance( dstTp, 1 );

            ret.SetValue( got, 0 );

            return ret;
        }

        public static object ToList<SrcT>( SrcT val, Type dstTp )
        {
            object got = ToSingleValue<SrcT>( val, dstTp );
            if ( got == null ) return null;

            ArrayList list = new ArrayList();

            list.Add( got );
            return list;
        }

        public static object ToValue( Type srcTp, object val, Type dstTp )
        {
            if ( srcTp == null || dstTp == null || !srcTp.IsValueType || !dstTp.IsValueType )
                return null;
            if ( srcTp.IsEnum || dstTp.IsEnum ) return null;
            if ( val == null ) return null;
            if ( Marshal.SizeOf( srcTp ) != Marshal.SizeOf( dstTp ) ) return null;
            if ( Marshal.SizeOf( srcTp ) <= 0 ) return null;

            IntPtr mem = Marshal.AllocHGlobal( Marshal.SizeOf( srcTp ) );
            if ( mem == IntPtr.Zero ) return null;

            Marshal.StructureToPtr( val, mem, false );
            object conv = Marshal.PtrToStructure( mem, dstTp );
            Marshal.FreeHGlobal( mem );

            return conv;
        }

        public static object ToArray( Array src, Type dstTp )
        {
            if ( src == null || src.Length <= 0 ) return null;
            object itm = src.GetValue( 0 );
            if ( itm == null ) return null;

            Type srctp = itm.GetType();

            Array ret = Array.CreateInstance( dstTp, src.Length );
            for ( int i = 0 ; i < src.Length ; i++ )
            {
                object val = ToValue( srctp, src.GetValue( i ), dstTp );
                if ( val == null ) return null;

                ret.SetValue( val, i );
            }

            return ret;
        }

        public static object ToArray( ArrayList src, Type dstTp )
        {
            if ( src == null || src.Count <= 0 ) return null;
            object itm = src[ 0 ];
            if ( itm == null ) return null;

            Type srctp = itm.GetType();

            Array ret = Array.CreateInstance( dstTp, src.Count );
            for ( int i = 0 ; i < src.Count ; i++ )
            {
                object val = ToValue( srctp, src[ i ], dstTp );
                if ( val == null ) return null;

                ret.SetValue( val, i );
            }

            return ret;
        }

        public static object ToList( Array src, Type dstTp )
        {
            if ( src == null || src.Length <= 0 ) return null;
            object itm = src.GetValue( 0 );
            if ( itm == null ) return null;

            Type srctp = itm.GetType();

            ArrayList ret = new ArrayList();
            for ( int i = 0 ; i < src.Length ; i++ )
            {
                object val = ToValue( srctp, src.GetValue( i ), dstTp );
                if ( val == null ) return null;

                ret.Add( val );
            }

            return ret;
        }

        public static object ToList( ArrayList src, Type dstTp )
        {
            if ( src == null || src.Count <= 0 ) return null;
            object itm = src[ 0 ];
            if ( itm == null ) return null;

            Type srctp = itm.GetType();

            ArrayList ret = new ArrayList();
            for ( int i = 0 ; i < src.Count ; i++ )
            {
                object val = ToValue( srctp, src[ i ], dstTp );
                if ( val == null ) return null;

                ret.Add( val );
            }

            return ret;
        }

        public static bool WriteXml( UDataCarrier[] arr, string[] addInfo, Stream writeStream )
        {
            if ( arr == null || writeStream == null ) return false;

            try
            {
                XmlTextWriter tw = new XmlTextWriter( writeStream, Encoding.UTF8 );
                tw.Formatting = Formatting.Indented;

                tw.WriteStartDocument();
                tw.WriteStartElement( "DataCarrier" );

                if ( addInfo != null && addInfo.Length > 0 )
                {
                    for ( int i = 0 ; i < addInfo.Length ; i++ )
                        tw.WriteElementString( "Appendage", addInfo[ i ] );
                }

                for ( int i = 0 ; i < arr.Length ; i++ )
                {
                    tw.WriteStartElement( "Element" );

                    if ( arr[ i ] == null )
                    {
                        tw.WriteElementString( "TypeName", "" );
                        tw.WriteElementString( "Data", "" );
                    }
                    else
                    {
                        // write data type
                        tw.WriteElementString( "TypeName", arr[ i ].Tp.FullName );

                        if ( arr[ i ].Data == null )
                            tw.WriteElementString( "Data", "" );
                        else
                        {
                            // write data
                            MemoryStream ms = new MemoryStream();
                            XmlSerializer xz = new XmlSerializer( arr[ i ].Tp );
                            xz.Serialize( ms, arr[ i ].Data );

                            tw.WriteElementString( "Data", Encoding.UTF8.GetString( ms.ToArray() ) );

                            ms.Dispose();
                            ms = null;
                        }
                    }

                    tw.WriteEndElement();
                }

                tw.WriteEndElement();
                tw.WriteEndDocument();
                tw.Flush();
                tw.Close();
            }
            catch(Exception e)
            {
                if ( ShowDialogDisplayError )
                    MessageBox.Show( $"Exception:\n{DumpException( e )}" );
                return false;
            }

            return true;
        }

        public static bool WriteXml( UDataCarrier[] arr, string filePath, string[] addInfo )
        {
            if ( arr == null || String.IsNullOrEmpty( filePath ) )
                return false;

            bool bRet = false;

            try
            {
                using ( Stream ws = File.Open( filePath, FileMode.Create ) )
                {
                    bRet = WriteXml( arr, addInfo, ws );
                }
            }
            catch
            {
                return false;
            }

            return bRet;
        }

        public static bool WriteXml( List<UDataCarrierSet> lst, Stream writeStream )
        {
            if ( lst == null || writeStream == null ) return false;
            try
            {
                XmlTextWriter tw = new XmlTextWriter( writeStream, Encoding.UTF8 );
                tw.Formatting = Formatting.Indented;

                tw.WriteStartDocument();
                tw.WriteStartElement( "ManyDataCarriers" );

                for ( int x = 0 ; x < lst.Count ; x++ )
                {
                    if ( lst[ x ] == null )
                    {
                        tw.WriteElementString( "OneItem", "" );
                        continue;
                    }

                    tw.WriteStartElement( "OneItem" );

                    // write ID string
                    tw.WriteElementString( "ID", lst[ x ]._strID );
                    // write additional strings
                    if ( lst[ x ]._AddiInfo != null )
                    {
                        for ( int j = 0 ; j < lst[ x ]._AddiInfo.Length ; j++ )
                            tw.WriteElementString( "Appendage", lst[ x ]._AddiInfo[ j ] );
                    }
                    // write DataCarrier
                    if ( lst[ x ]._Array != null && lst[ x ]._Array.Length > 0 )
                    {
                        for ( int i = 0 ; i < lst[ x ]._Array.Length ; i++ )
                        {
                            tw.WriteStartElement( "Element" );

                            if ( lst[ x ]._Array[ i ] == null )
                            {
                                tw.WriteElementString( "TypeName", "" );
                                tw.WriteElementString( "Data", "" );
                            }
                            else
                            {
                                // write data type
                                tw.WriteElementString( "TypeName", lst[ x ]._Array[ i ].Tp.FullName );

                                if ( lst[ x ]._Array[ i ].Data == null )
                                    tw.WriteElementString( "Data", "" );
                                else
                                {
                                    // write data
                                    MemoryStream ms = new MemoryStream();
                                    XmlSerializer xz = new XmlSerializer( lst[ x ]._Array[ i ].Tp );
                                    xz.Serialize( ms, lst[ x ]._Array[ i ].Data );

                                    tw.WriteElementString( "Data", Encoding.UTF8.GetString( ms.ToArray() ) );

                                    ms.Dispose();
                                    ms = null;
                                }
                            }

                            tw.WriteEndElement(); // Element end element
                        }
                    }
                }

                tw.WriteEndElement();
                tw.WriteEndDocument();
                tw.Flush();
                tw.Close();
            }
            catch (Exception e)
            {
                if ( ShowDialogDisplayError )
                    MessageBox.Show( $"Exception:\n{DumpException( e )}" );
                return false;
            }

            return true;
        }

        public static bool WriteXml( List<UDataCarrierSet> lst, string filePath )
        {
            if ( lst == null || String.IsNullOrEmpty( filePath ) )
                return false;

            bool ret = false;
            try
            {
                using ( Stream ws = File.Open( filePath, FileMode.Create ) )
                {
                    ret = WriteXml( lst, ws );
                }
            }
            catch
            {
                return false;
            }

            return ret;
        }

        private static Type GetPerfectMatch(string str, Assembly[] findingArr )
        {
            Type retTp = null;
            //
            // find perfect match
            //
            foreach ( Assembly a in findingArr )
            {
                if ( a == null ) continue;
                foreach ( Type t in a.GetTypes() )
                {
                    if ( str == t.FullName )
                    {
                        retTp = t; break;
                    }
                }
                if ( retTp != null )
                    break;
            }
            return retTp;
        }

        public static Type GetTpFromName( string str, Assembly[] findingArr )
        {
            Type retTp = null;

            //
            // try to test from system default type
            //
            Type tpstr = Type.GetType( str );
            if ( tpstr != null )
                return tpstr;

            if ( findingArr == null || findingArr.Length <= 0 )
                return null;


            int arrPos = str.LastIndexOf( "[]" );
            if (arrPos > 0 && (str.Length - arrPos) == 2)
            {
                retTp = GetPerfectMatch( str.Substring( 0, arrPos ), findingArr );
                if ( retTp != null )
                {
                    Array arrobj = Array.CreateInstance( retTp, 1 );
                    retTp = arrobj.GetType();
                    return retTp;
                }
            }
            else
            {
                retTp = GetPerfectMatch( str, findingArr );
                if ( retTp != null )
                    return retTp;
            }

            //
            // find perfect match
            //
            //foreach ( Assembly a in findingArr )
            //{
            //    if ( a == null ) continue;
            //    foreach ( Type t in a.GetTypes() )
            //    {
            //        if ( str == t.FullName )
            //        {
            //            retTp = t; break;
            //        }
            //    }
            //    if ( retTp != null )
            //        break;
            //}
            //if ( retTp != null ) return retTp;
            //if ( ResourceManager.DataCarrierTypePerfectMatch )
            //    return null;

            //
            // find last match
            //
            //string[] strs = str.Split( new char[ 2 ] { '.', '+' } ); // '+' is in protected area, so there is a risk
            string[] strs = str.Split( new char[ 1 ] { '.' } );
            if ( strs == null ) return null;
            string part = strs[ strs.Length - 1 ];
            if ( String.IsNullOrEmpty( part ) ) return null;

            int arrsymbolidx = part.IndexOf( "[]" );
            bool bIsArray = (arrsymbolidx > 0);
            string exactTpNm = bIsArray ? part.Substring( 0, arrsymbolidx ) : part;

            foreach ( Assembly a in findingArr )
            {
                if ( a == null ) continue;
                foreach ( Type t in a.GetTypes() )
                {
                    //strs = t.FullName.Split( new char[ 2 ] { '.', '+' } );
                    strs = t.FullName.Split( new char[ 1 ] { '.' } );  // '+' is in protected area, so there is a risk
                    if ( strs != null && !String.IsNullOrEmpty( strs[ strs.Length - 1 ] ) && exactTpNm == strs[ strs.Length - 1 ] )
                    {
                        if ( bIsArray )
                        {
                            Array arrobj = Array.CreateInstance( t, 1 );
                            retTp = arrobj.GetType();
                        }
                        else
                            retTp = t;
                        break;
                    }
                }
                if ( retTp != null )
                    break;
            }

            return retTp;
        }

        public static bool ReadXml( string filePath, ref UDataCarrier[] arr, ref string[] addInfo )
        {
            return ReadXml( filePath, AppDomain.CurrentDomain.GetAssemblies(), ref arr, ref addInfo );
        }

        public static bool ReadXml( XmlNodeList nlDat, XmlNodeList addDat, Assembly[] assemArr, ref UDataCarrier[] arr, ref string[] addInfo )
        {
            arr = null;
            addInfo = null;
            if ( nlDat == null )
                return false;

            try
            {
                List<UDataCarrier> list = new List<UDataCarrier>();
                if ( nlDat != null && nlDat.Count > 0 )
                {
                    for ( int i = 0; i < nlDat.Count; i++ )
                    {
                        XmlNode tpn = nlDat[ i ].SelectSingleNode( "TypeName" );
                        XmlNode dat = nlDat[ i ].SelectSingleNode( "Data" );

                        if ( tpn == null || dat == null || String.IsNullOrEmpty( dat.InnerText ) )
                        {
                            list.Add( new UDataCarrier() );
                            continue;
                        }

                        Type tp = GetTpFromName( tpn.InnerText, assemArr );
                        if ( tp == null )
                        {
                            list.Add( new UDataCarrier() );
                            continue;
                        }

                        UDataCarrier itm = new UDataCarrier();
                        itm.Tp = tp;

                        byte[] data = Encoding.UTF8.GetBytes( dat.InnerText );
                        if ( data != null && data.Length > 0 )
                        {
                            MemoryStream ms = new MemoryStream( data );
                            XmlSerializer xz = new XmlSerializer( tp );
                            itm.Data = xz.Deserialize( ms );

                            ms.Dispose();
                            ms = null;
                        }

                        list.Add( itm );
                    }
                }

                arr = list.Count <= 0 ? null : list.ToArray();

                List<string> addInfoList = new List<string>();
                if ( addDat != null && addDat.Count > 0 )
                {
                    for ( int i = 0; i < addDat.Count; i++ )
                        addInfoList.Add( String.IsNullOrEmpty( addDat[ i ].InnerText ) ? null : String.Copy( addDat[ i ].InnerText ) );
                }
                addInfo = addInfoList.Count <= 0 ? null : addInfoList.ToArray();

                return true;
            }
            catch (Exception e)
            {
                if ( ShowDialogDisplayError )
                    MessageBox.Show( $"Exception:\n{DumpException( e )}" );
                return false;
            }
        }

        public static readonly string ARR_DATACARRIER_PATH = "//DataCarrier/Element";
        public static readonly string ARR_DATACARRIER_ADDI_PATH = "//DataCarrier/Appendage";

        public static bool ReadXml( string filePath, Assembly[] assemArr, ref UDataCarrier[] arr, ref string[] addInfo )
        {
            arr = null; addInfo = null;
            if ( String.IsNullOrEmpty( filePath ) || !File.Exists( filePath ) )
                return false;

//#if DOTNET4
            XmlDocument doc = new XmlDocument();
//#else
//            XmlDataDocument doc = new XmlDataDocument();
//#endif
            try
            {
                doc.Load( filePath );
            }
            //catch ( Exception exp )
            catch
            {
                return false;
            }

            List<UDataCarrier> list = new List<UDataCarrier>();

            XmlNodeList nl = doc.SelectNodes( ARR_DATACARRIER_PATH );
            XmlNodeList addi = doc.SelectNodes( ARR_DATACARRIER_ADDI_PATH );

            return ReadXml( nl, addi, assemArr, ref arr, ref addInfo );
        }

        public static bool ReadXml( Stream rs, Assembly[] assemArr, ref UDataCarrier[] arr, ref string[] addInfo )
        {
            arr = null; addInfo = null;
            if ( rs == null ) return false;

//#if DOTNET4
            XmlDocument doc = new XmlDocument();
//#else
//            XmlDataDocument doc = new XmlDataDocument();
//#endif
            try
            {
                doc.Load( rs );
            }
            //catch ( Exception exp )
            catch
            {
                return false;
            }

            List<UDataCarrier> list = new List<UDataCarrier>();

            XmlNodeList nl = doc.SelectNodes( ARR_DATACARRIER_PATH );
            XmlNodeList addi = doc.SelectNodes( ARR_DATACARRIER_ADDI_PATH );

            return ReadXml( nl, addi, assemArr, ref arr, ref addInfo );
        }

        public static bool ReadXml( string filePath, ref List<UDataCarrierSet> retLst )
        {
            return ReadXml( filePath, AppDomain.CurrentDomain.GetAssemblies(), ref retLst );
        }

        public static bool ReadXml( XmlNodeList itms, Assembly[] assemArr, ref List<UDataCarrierSet> retLst )
        {
            retLst = null;
            if ( itms == null ) return false;

            try
            {

                if ( itms != null && itms.Count > 0 )
                {
                    retLst = new List<UDataCarrierSet>();

                    for ( int x = 0; x < itms.Count; x++ )
                    {
                        // read ID element
                        XmlNode idn = itms[ x ].SelectSingleNode( "ID" );
                        // read Appendage elements
                        XmlNodeList addinl = itms[ x ].SelectNodes( "Appendage" );
                        // read DataCarrier elements
                        XmlNodeList nl = itms[ x ].SelectNodes( "Element" );
                        // parsing DataCarrier element data
                        List<UDataCarrier> list = new List<UDataCarrier>();
                        for ( int i = 0; i < nl.Count; i++ )
                        {
                            XmlNode tpn = nl[ i ].SelectSingleNode( "TypeName" );
                            XmlNode dat = nl[ i ].SelectSingleNode( "Data" );

                            if ( tpn == null || dat == null || String.IsNullOrEmpty( dat.InnerText ) )
                            {
                                list.Add( new UDataCarrier() );
                                continue;
                            }

                            Type tp = GetTpFromName( tpn.InnerText, assemArr );
                            if ( tp == null )
                            {
                                list.Add( new UDataCarrier() );
                                continue;
                            }

                            UDataCarrier itm = new UDataCarrier();
                            itm.Tp = tp;

                            byte[] data = Encoding.UTF8.GetBytes( dat.InnerText );
                            if ( data != null && data.Length > 0 )
                            {
                                MemoryStream ms = new MemoryStream( data );
                                XmlSerializer xz = new XmlSerializer( tp );
                                itm.Data = xz.Deserialize( ms );

                                ms.Dispose();
                                ms = null;
                            }

                            list.Add( itm );
                        }

                        string[] additionalStr = ( addinl == null || addinl.Count <= 0 ) ? null : new string[ addinl.Count ];
                        if ( addinl != null && addinl.Count > 0 )
                        {
                            for ( int j = 0; j < addinl.Count; j++ )
                                additionalStr[ j ] = String.IsNullOrEmpty( addinl[ j ].InnerText ) ? null : String.Copy( addinl[ j ].InnerText );
                        }

                        retLst.Add( new UDataCarrierSet( idn == null || String.IsNullOrEmpty( idn.InnerText ) ? null : String.Copy( idn.InnerText ),
                                                              list.ToArray(),
                                                              additionalStr ) );
                    }
                }
            }
            catch(Exception e)
            {
                if ( ShowDialogDisplayError )
                    MessageBox.Show( $"Exception:\n{DumpException( e )}" );
                return false;
            }

            return true;
        }

        public static readonly string LIST_BASICDATAITEM_PATH = "//ManyDataCarriers/OneItem";

        public static bool ReadXml( string filePath, Assembly[] assemArr, ref List<UDataCarrierSet> retLst )
        {
            retLst = null;
            if ( String.IsNullOrEmpty( filePath ) || !File.Exists( filePath ) )
                return false;
//#if DOTNET4
            XmlDocument doc = new XmlDocument();
//#else
//            XmlDataDocument doc = new XmlDataDocument();
//#endif
            try
            {
                doc.Load( filePath );
            }
            //catch ( Exception exp )
            catch
            {
                return false;
            }


            XmlNodeList itms = doc.SelectNodes( LIST_BASICDATAITEM_PATH );
            return ReadXml( itms, assemArr, ref retLst );
        }

        public static bool ReadXml( Stream rs, Assembly[] assemArr, ref List<UDataCarrierSet> retLst )
        {
            retLst = null;
            if ( rs == null ) return false;
//#if DOTNET4
            XmlDocument doc = new XmlDocument();
//#else
//            XmlDataDocument doc = new XmlDataDocument();
//#endif
            try
            {
                doc.Load( rs );
            }
            //catch ( Exception exp )
            catch
            {
                return false;
            }

            XmlNodeList itms = doc.SelectNodes( LIST_BASICDATAITEM_PATH );
            return ReadXml( itms, assemArr, ref retLst );
        }

        public static bool SerializeDic( Dictionary<UDataCarrier, UDataCarrier[]> dic, out string[] ret, bool base64Conver = false )
        {
            ret = new string[ 0 ];
            if ( dic == null || dic.Count == 0 )
                return true;

            ret = new string[ dic.Count + 1 ];
            var keys = new List<UDataCarrier>();
            int index = 1;
            foreach ( var kv in dic )
            {
                keys.Add( kv.Key );
                using ( var ms = new MemoryStream() )
                {
                    if ( !WriteXml( kv.Value, null, ms ) )
                    {
                        ret = new string[ 0 ];
                        return false;
                    }

                    var data = ms.ToArray();
                    var store = base64Conver ? Convert.ToBase64String( data ) : Encoding.UTF8.GetString( data, 0, data.Length );
                    ret[ index++ ] = store;
                }
            }

            using ( var ms = new MemoryStream() )
            {
                if ( !WriteXml( keys.ToArray(), null, ms ) )
                {
                    ret = new string[ 0 ];
                    return false;
                }
                var data = ms.ToArray();
                ret[ 0 ] = base64Conver ? Convert.ToBase64String( data ) : Encoding.UTF8.GetString( data, 0, data.Length );
            }

            return true;
        }

        public static bool DeserializeDic( string[] input, out Dictionary<UDataCarrier, UDataCarrier[]> ret, bool base64Convert = false, Assembly[] referenceAssemblies = null )
        {
            ret = new Dictionary<UDataCarrier, UDataCarrier[]>();
            if ( input == null || input.Length < 2 )
                return false;

            try
            {
                // read key
                var data = base64Convert ? Convert.FromBase64String( input[ 0 ] ) : Encoding.UTF8.GetBytes( input[ 0 ] );
                var keys = new UDataCarrier[ 0 ];
                using ( var ms = new MemoryStream( data ) )
                {
                    var dummy = new string[ 0 ];
                    if ( !ReadXml( ms, referenceAssemblies ?? AppDomain.CurrentDomain.GetAssemblies(), ref keys, ref dummy ) )
                        return false;
                }

                if ( keys == null || ( keys.Length + 1 ) != input.Length )
                    return false;

                for ( int i = 1; i < input.Length; i++ )
                {
                    data = base64Convert ? Convert.FromBase64String( input[ i ] ) : Encoding.UTF8.GetBytes( input[ i ] );
                    var value = new UDataCarrier[ 0 ];
                    using ( var ms = new MemoryStream( data ) )
                    {
                        var dummy = new string[ 0 ];
                        if ( !ReadXml( ms, AppDomain.CurrentDomain.GetAssemblies(), ref value, ref dummy ) )
                            return false;
                    }
                    ret.Add( keys[ i - 1 ], value );
                }

                return true;
            }
            catch { return false; }
        }

        public static bool SerializeDicKeyString( Dictionary<string, UDataCarrier> dic, out string[] ret, bool base64Conver = false )
        {
            var c = ( from kv in dic select new KeyValuePair<UDataCarrier, UDataCarrier[]>( MakeOne( kv.Key ), new UDataCarrier[] { kv.Value } ) )
                .ToDictionary( kv => kv.Key, kv => kv.Value );
            return SerializeDic( c, out ret, base64Conver );
        }
        public static bool DeserializeDicKeyStringValueOne( string[] input, out Dictionary<string, UDataCarrier> ret, bool base64Conver = false, Assembly[] referenceAssemblies = null )
        {
            ret = new Dictionary<string, UDataCarrier>();
            if ( !DeserializeDic( input, out var got, base64Conver, referenceAssemblies ) )
                return false;

            try
            {
                foreach(var kv in got)
                {
                    if (kv.Value == null || kv.Value.Length == 0)
                    {
                        ret.Add( kv.Key.Data.ToString(), null );
                        continue;
                    }
                    ret.Add( kv.Key.Data.ToString(), kv.Value[ 0 ] );
                }
                return true;
            }
            catch { return false; }
        }

        public static bool SerializeDicKeyString( Dictionary<string, UDataCarrier[]> dic, out string[] ret, bool base64Conver = false )
        {
            var c = ( from kv in dic select new KeyValuePair<UDataCarrier, UDataCarrier[]>( UDataCarrier.MakeOne( kv.Key ), kv.Value ) ).ToDictionary( kv => kv.Key, kv => kv.Value );
            return SerializeDic( c, out ret, base64Conver );
        }

        public static bool DeserializeDicKeyStringValueMany( string[] input, out Dictionary<string, UDataCarrier[]> ret, bool base64Conver = false, Assembly[] referenceAssemblies = null )
        {
            ret = new Dictionary<string, UDataCarrier[]>();
            if ( !DeserializeDic( input, out var got, base64Conver, referenceAssemblies ) )
                return false;

            try
            {
                ret = ( from kv in got select new KeyValuePair<string, UDataCarrier[]>( kv.Key.Data.ToString(), kv.Value ) ).ToDictionary( kv => kv.Key, kv => kv.Value );
                return true;
            }
            catch { return false; }
        }

        public static void FreeByIDispose( UDataCarrier[] c )
        {
            if ( c == null ) return;
            foreach ( var v in c )
            {
                v?.Dispose();
            }
        }

        public static void FreeByIDispose( List< UDataCarrier[] > lst )
        {
            if ( lst == null ) return;
            foreach ( var v in lst )
            {
                FreeByIDispose( v );
            }
        }

        public static bool Put<T>(UDataCarrier src, ref T t, T def)
        {
            if (src == null || src.m_Data == null)
            {
                t = def; return false;
            }
            return src.Put( ref t, def );
        }

        public static T Get<T>(UDataCarrier src, T defaultV)
        {
            if ( src == null || src.Data == null ) return defaultV;
            try
            {
                if ( src.Data is T v )
                    return v;
                return defaultV;
            }
            catch { return defaultV; }
        }

        public static bool Get<T>(UDataCarrier src, T defaultV, out T v )
        {
            if (src == null || src.Data == null)
            {
                v = defaultV;
                return false;
            }

            try
            {
                if ( src.Data is T vv)
                {
                    v = vv;
                    return true;
                }

                v = defaultV;
                return false;
            }
            catch
            {
                v = defaultV;
                return false;
            }
        }

        public static T Get<T>( Dictionary<string, UDataCarrier> dic, string k, T defaultV )
        {
            if ( dic == null || string.IsNullOrEmpty( k ) )
                return defaultV;
            try
            {
                if ( dic.TryGetValue( k, out var carr ) && Get( carr, defaultV, out var v ) )
                    return v;
                return defaultV;
            }
            catch { return defaultV; }
        }
        public static bool Get<T>(Dictionary<string,UDataCarrier> dic, string k, T defaultV, out T v)
        {
            v = defaultV;
            if ( dic == null || string.IsNullOrEmpty( k ) )
                return false;
            try
            {
                return dic.TryGetValue( k, out var carr ) && Get( carr, defaultV, out v );
            }
            catch { return false; }
        }
        public static bool Set<T>(Dictionary<string, UDataCarrier> dic, string k, T v, bool handleable = false)
        {
            if ( dic == null )
                return false;

            if (dic.TryGetValue(k, out var carr))
            {
                dic[ k ] = v is UDataCarrier ? v as UDataCarrier : MakeOne( v, handleable );
                carr?.Dispose();
            }
            else dic.Add(k, v is UDataCarrier ? v as UDataCarrier : MakeOne( v, handleable ));
            return true;
        }
        public static bool GetDicKeyStrOne<T>( UDataCarrier carr, string k, T defaultV, out T v )
        {
            v = defaultV;
            if ( !Get<Dictionary<string, UDataCarrier>>( carr, null, out var dic ) )
                return false;

            return Get( dic, k, defaultV, out v );
        }
        public static T GetDicKeyStrOne<T>(UDataCarrier carr, string k, T defaulV)
        {
            if ( !Get<Dictionary<string, UDataCarrier>>( carr, null, out var dic ) || dic == null )
                return defaulV;

            return Get( dic, k, defaulV );
        }
        public static bool SetDicKeyStrOne<T>(UDataCarrier carr, string k, T v, bool handleable = false)
        {
            if ( !Get<Dictionary<string, UDataCarrier>>( carr, null, out var dic ) || dic == null )
                return false;

            return Set( dic, k, v, handleable );
        }
        public static bool DicKeyStrOneContain(UDataCarrier carr, string k)
        {
            if ( !Get<Dictionary<string, UDataCarrier>>( carr, null, out var dic ) || dic == null )
                return false;

            return dic.ContainsKey( k );
        }
        public static void RmvDicKeyStrOne(UDataCarrier carr, string k)
        {
            if ( !Get<Dictionary<string, UDataCarrier>>( carr, null, out var dic ) || dic == null )
                return;
            if (dic.TryGetValue(k, out var v) )
            {
                dic.Remove(k);
                v?.Dispose();
            }
        }


        public static void Free(object ctx)
        {
            if ( ctx == null )
                return;
            if ( ctx is IDisposable d) d?.Dispose();
            else if (ctx is IDictionary<string, UDataCarrier> dic01)
            {
                foreach ( var kv in dic01 )
                    kv.Value?.Dispose();
            }
            else if (ctx is IDictionary<string, List<UDataCarrier>> dic02)
            {
                foreach(var kv in dic02 )
                {
                    if ( kv.Value == null || kv.Value.Count == 0 )
                        continue;
                    foreach ( var v in kv.Value )
                        v?.Dispose();
                }
            }
            else if (ctx is IDictionary<string, UDataCarrier[]> dic03)
            {
                foreach ( var kv in dic03 )
                {
                    if ( kv.Value == null || kv.Value.Length == 0 )
                        continue;
                    foreach ( var v in kv.Value )
                        v?.Dispose();
                }
            }
            else if (ctx is IEnumerable container)
            {
                foreach(var i in container )
                {
                    if ( i is IDisposable di ) di?.Dispose();
                }
            }
        }

        public static UDataCarrier CmpPerfectStrInDesc(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || string.IsNullOrEmpty(input.Desc) ) return null;
            return ctx is string s && s == input.Desc ? input : null;
        }
        public static UDataCarrier CmpPerfectStrInDescLowercase(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || string.IsNullOrEmpty( input.Desc ) ) return null;
            return ctx is string s && s == input.Desc.ToLower() ? input : null;
        }
        public static UDataCarrier CmpContainStrInDesc(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || string.IsNullOrEmpty( input.Desc ) ) return null;
            return ctx is string s && s.Contains( input.Desc ) ? input : null;
        }
        public static UDataCarrier CmpBeginStrInDesc( object ctx, UDataCarrier input )
        {
            if ( input == null || ctx == null || string.IsNullOrEmpty( input.Desc ) ) return null;
            return ctx is string s && ( input.Desc.IndexOf( s ) == 0 ) ? input : null;
        }
        public static UDataCarrier CmpBeginStrInDescLowercase(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || string.IsNullOrEmpty( input.Desc ) ) return null;
            return ctx is string s && (input.Desc.ToLower().IndexOf(s) == 0) ? input : null;
        }
        public static UDataCarrier CmpType(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || input.Tp == null ) return null;
            return ctx is Type t && t == input.Tp ? input : null;
        }
        public static UDataCarrier CmpTypeName(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || input.Tp == null ) return null;
            return ctx is string name && name == input.Tp.Name ? input : null;
        }
        public static UDataCarrier CmpFullTypeName(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || input.Tp == null ) return null;
            return ctx is string name && name == input.Tp.FullName ? input : null;
        }
        public static UDataCarrier CmpTypeIsInherit(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || input.Tp == null ) return null;
            return ctx is Type t && t.IsSubclassOf( input.Tp ) ? input : null;
        }
        public static UDataCarrier CmpTypeIsBase(object ctx, UDataCarrier input)
        {
            if ( input == null || ctx == null || input.Tp == null ) return null;
            return ctx is Type t && input.Tp.IsSubclassOf(t) ? input : null;
        }

        public static UDataCarrier GetByCmpOne(IEnumerable<UDataCarrier> collection, object ctx, Func<object, UDataCarrier, UDataCarrier> cmp)
        {
            if ( collection == null || cmp == null )
                return null;
            foreach(var i in collection)
            {
                if ( i == null ) continue;
                var got = cmp( ctx, i );
                if ( got != null ) return got;
            }
            return null;
        }
        public static bool GetByCmpOne(IEnumerable<UDataCarrier> collection, object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out UDataCarrier ret, UDataCarrier defaultV = null)
        {
            ret = defaultV;
            if ( collection == null || cmp == null )
                return false;
            foreach(var i in collection)
            {
                if ( i == null ) continue;
                var got = cmp( ctx, i );
                if (got != null)
                {
                    ret = got;
                    return true;
                }
            }
            return false;
        }

        public static bool GetByCmpMany(IEnumerable<UDataCarrier> collection, object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out UDataCarrier[] ret)
        {
            ret = null;
            if ( collection == null || cmp == null )
                return false;

            var keep = new List<UDataCarrier>();
            foreach(var i in collection)
            {
                if ( i == null ) continue;
                var got = cmp( ctx, i );
                if (got != null) keep.Add( got );
            }

            ret = keep.ToArray();
            return ret.Length > 0;
        }

        public static bool GetHistoryCmpOne( List<UScriptHistoryCarrier> histo, object ctx, Func<string, object, UDataCarrier, UDataCarrier> cmp, out UDataCarrier ret, UDataCarrier defaultV = null )
        {
            ret = defaultV;
            if ( histo == null || histo.Count == 0 || cmp == null )
                return false;

            bool bok = false;
            bool exit = false;
            foreach(var h in histo)
            {
                if ( h == null || h.ResultsCarriers == null ) continue;
                foreach(var r in h.ResultsCarriers)
                {
                    if ( r == null || r.ResultSet == null || r.ResultSet.Length == 0 )
                        continue;
                    foreach(var i in r.ResultSet)
                    {
                        if ( i == null )
                            continue;
                        var got = cmp( h.Script.NameOfId, ctx, i );
                        if (got != null)
                        {
                            bok = true;
                            ret = got;
                            exit = true;
                            break;
                        }
                    }
                    if ( exit ) break;
                }
                if ( exit ) break;
            }

            return bok;
        }

        public static bool GetHistoryCmpMany( List<UScriptHistoryCarrier> histo, object ctx, Func<string, object, UDataCarrier, UDataCarrier> cmp, out UDataCarrier[] ret )
        {
            ret = null;
            if ( histo == null || histo.Count == 0 || cmp == null )
                return false;

            List<UDataCarrier> tmp = new List<UDataCarrier>();
            foreach ( var h in histo )
            {
                if ( h == null || h.ResultsCarriers == null ) continue;
                foreach ( var r in h.ResultsCarriers )
                {
                    if ( r == null || r.ResultSet == null || r.ResultSet.Length == 0 )
                        continue;
                    foreach ( var i in r.ResultSet )
                    {
                        if ( i == null )
                            continue;
                        var got = cmp( h.Script.NameOfId, ctx, i );
                        if ( got != null )
                            tmp.Add( got );
                    }
                }
            }

            ret = tmp.ToArray();
            return tmp.Count > 0;
        }


        private static string DumpException( Exception e )
        {
            if ( e == null )
                return "";
            var sb = new StringBuilder();
            while ( e != null )
            {
                if ( e.Data != null && e.Data.Count > 0 )
                {
                    sb.AppendLine( "Data:" );
                    foreach ( DictionaryEntry de in e.Data )
                        sb.AppendLine( $"  key={de.Key}  value={de.Value}" );
                }
                sb.AppendLine( $"HResult: {e.HResult}" );
                sb.AppendLine( $"Message: {e.Message}" );
                sb.AppendLine( $"StackTrace: {e.StackTrace}" );

                e = e.InnerException;
            }
            return sb.ToString();
        }
        #endregion

    }
}
