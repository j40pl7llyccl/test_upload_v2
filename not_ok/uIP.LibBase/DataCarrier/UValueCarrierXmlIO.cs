using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;

using uIP.LibBase.Utility;

namespace uIP.LibBase.DataCarrier
{
    public static class UValueCarrierXmlIO
    {
        private static string _strValCarrDatTp = "ValCarrDatType";
        private static string _strValCarrSharpTp = "ValCarrSharpDefTypeName";
        private static string _strValCarrItemSharpTpOfArr = "ValCarrItemSharpTypeNameofArr";
        private static string _strValCarrDatValue = "ValCarrDatValue";
        private static string _strValCarrDefValue = "ValCarrDefValue";
        private static string _strValCarrValMax = "ValCarrValMax";
        private static string _strValCarrValMin = "ValCarrValMin";
        private static string _strValCarrValueScale = "ValCarrValScale";
        private static string _strValCarrValEnumList = "ValCarrValEnumList";
        private static string _strValCarrValArrDim = "ValCarrValArrDim";

        #region Read structure value from XML

        private static object ReadOne( FieldInfo fi, string pn, XmlDocument doc )
        {
            if ( doc == null ) return null;
            string cnod = String.Format( "{0}/{1}", pn, fi.Name );

            XmlNode nod = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrDatTp ) );
            if ( nod == null ) return null;

            if ( !Enum.IsDefined( typeof( eUValueCarrierDatType ), nod.InnerText ) ) return null;

            eUValueCarrierDatType dtp = ( eUValueCarrierDatType ) Enum.Parse( typeof( eUValueCarrierDatType ), nod.InnerText );
            bool berr = false;
            object ret = null;
            switch ( dtp )
            {
                case eUValueCarrierDatType.Enumeration:
                    nod = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrDatValue ) );
                    if ( nod != null )
                    {
                        ret = Enum.Parse( fi.FieldType, nod.InnerText );
                    }
                    else berr = true;
                    break;

                case eUValueCarrierDatType.Array:
                    nod = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrDatValue ) );
                    if ( nod != null && !String.IsNullOrEmpty( nod.InnerText ) )
                    {
                        Array last = null;
                        XmlNode tpn = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrItemSharpTpOfArr ) );
                        XmlNode dimn = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrValArrDim ) );
                        Int32[] dimInfo = null;

                        if ( dimn != null && !String.IsNullOrEmpty( dimn.InnerText ) )
                        {
                            string[] iii = dimn.InnerText.Split( new char[ 1 ] { ',' } );
                            if ( iii != null )
                            {
                                List<Int32> dl = new List<Int32>();
                                try
                                {
                                    for ( int i = 0 ; i < iii.Length ; i++ )
                                        dl.Add( Convert.ToInt32( iii[ i ] ) );
                                }
                                catch { dl.Clear(); }

                                if ( dl.Count > 0 ) dimInfo = dl.ToArray();
                            }
                        }

                        if ( tpn != null )
                        {
                            if ( tpn.InnerText == typeof( Byte ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<Byte>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( SByte ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<SByte>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( Int16 ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<Int16>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( UInt16 ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<UInt16>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( Int32 ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<Int32>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( UInt32 ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<UInt32>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( Int64 ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<Int64>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( UInt64 ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<UInt64>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( Double ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<Double>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                            else if ( tpn.InnerText == typeof( Single ).Name ) last = UDatManBuffArrConverting.ReverseByteBuffer<Single>( Encoding.UTF8.GetBytes( nod.InnerText ), dimInfo );
                        }

                        if ( last != null ) ret = last;
                        else berr = true;
                    }
                    else berr = true;
                    break;

                case eUValueCarrierDatType.Single:
                    nod = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrDatValue ) );
                    XmlNode snod = doc.SelectSingleNode( String.Format( "{0}/{1}", cnod, _strValCarrSharpTp ) );

                    if ( nod != null && snod != null )
                    {
                        try
                        {
                            if ( snod.InnerText == typeof( Byte ).Name ) ret = Convert.ToByte( nod.InnerText );
                            else if ( snod.InnerText == typeof( SByte ).Name ) ret = Convert.ToSByte( nod.InnerText );
                            else if ( snod.InnerText == typeof( Int16 ).Name ) ret = Convert.ToInt16( nod.InnerText );
                            else if ( snod.InnerText == typeof( UInt16 ).Name ) ret = Convert.ToUInt16( nod.InnerText );
                            else if ( snod.InnerText == typeof( Int32 ).Name ) ret = Convert.ToInt32( nod.InnerText );
                            else if ( snod.InnerText == typeof( UInt32 ).Name ) ret = Convert.ToUInt32( nod.InnerText );
                            else if ( snod.InnerText == typeof( Int64 ).Name ) ret = Convert.ToInt64( nod.InnerText );
                            else if ( snod.InnerText == typeof( UInt64 ).Name ) ret = Convert.ToUInt64( nod.InnerText );
                            else if ( snod.InnerText == typeof( Double ).Name ) ret = Convert.ToDouble( nod.InnerText );
                            else if ( snod.InnerText == typeof( Single ).Name ) ret = Convert.ToSingle( nod.InnerText );
                            else if ( snod.InnerText == typeof( string ).Name ) ret = nod.InnerText;
                        }
                        catch { berr = true; }
                    }
                    else berr = true;
                    break;
            }

            if ( berr )
            {
                // TODO: read refault
                berr = false;
            }

            return ret;
        }

        private static void ReadNestOne( ref object val, Type tp, string pn, XmlDocument doc )
        {
            FieldInfo[] fi = tp.GetFields();
            if ( fi == null ) return;

            for ( int i = 0 ; i < fi.Length ; i++ )
            {
                if ( fi[ i ].FieldType.IsNested && !fi[ i ].FieldType.IsEnum )
                {
                    object obj = fi[ i ].GetValue( val );
                    string npn = String.Format( "{0}/{1}", pn, fi[ i ].Name );

                    ReadNestOne( ref obj, fi[ i ].FieldType, npn, doc );
                    fi[ i ].SetValue( val, obj );
                }
                else
                {
                    object rs = ReadOne( fi[ i ], pn, doc );
                    if ( rs != null ) fi[ i ].SetValue( val, rs );
                }
            }
        }

        public static bool ReadTypeStruct<T>( ref object val, Stream rs )
        {
            if ( typeof( T ).IsClass ) return false; // not support class

            FieldInfo[] fi = typeof( T ).GetFields();
            if ( fi == null || fi.Length <= 0 ) return false;

            // load xml
            XmlDocument doc = null;
            bool bXML = false;
            byte[] buf = new byte[ 3 ];
            rs.Read( buf, 0, 3 );
            if ( buf[ 0 ] == ( byte ) 0xEF && buf[ 1 ] == ( byte ) 0xBB && buf[ 2 ] == ( byte ) 0xBF )
                bXML = true;

            if ( bXML )
            {
                doc = new XmlDocument();
                doc.Load( rs );
            }

            for ( int i = 0 ; i < fi.Length ; i++ )
            {
                if ( fi[ i ].FieldType.IsNested && !fi[ i ].FieldType.IsEnum )
                {
                    object obj = fi[ i ].GetValue( val );
                    ReadNestOne( ref obj, fi[ i ].FieldType, String.Format( "//{0}/{1}", typeof( T ).Name, fi[ i ].Name ), doc );
                    fi[ i ].SetValue( val, obj );
                }
                else
                {
                    object result = ReadOne( fi[ i ], String.Format( "//{0}", typeof( T ).Name ), doc );
                    if ( result != null ) fi[ i ].SetValue( val, result );
                }
            }

            return true;
        }
        #endregion

        #region Write structure value to XML

        private static void WriteOne( object val, FieldInfo fi, string pn, XmlTextWriter tw, XmlDocument refDoc )
        {
            if ( tw == null ) return;
            string currnod = String.Format( "{0}/{1}", pn, fi.Name );
            //Console.WriteLine( "{0}/{1} = {2}", pn, fi.Name, fi.GetValue( val ) );
            tw.WriteStartElement( fi.Name );

            if ( fi.FieldType.IsEnum )
            {
                tw.WriteElementString( _strValCarrDatTp, eUValueCarrierDatType.Enumeration.ToString() ); // container class type
                tw.WriteElementString( _strValCarrSharpTp, fi.FieldType.Name ); // system type name
                // get default value
                if ( refDoc != null )
                {
                    XmlNode nod = refDoc.SelectSingleNode( String.Format( "{0}/{1}", currnod, _strValCarrDefValue ) );
                    if ( nod != null ) tw.WriteElementString( _strValCarrDefValue, nod.InnerText );
                }
                // get current value
                tw.WriteElementString( _strValCarrDatValue, fi.GetValue( val ).ToString() );
                // write list
                string[] ls = Enum.GetNames( fi.FieldType );
                if ( ls != null )
                {
                    for ( int i = 0 ; i < ls.Length ; i++ ) tw.WriteElementString( _strValCarrValEnumList, ls[ i ] );
                }
            }
            else if ( fi.FieldType.IsArray )
            {
                string[] btype = fi.FieldType.Name.Split( new char[ 1 ] { '[' } );
                if ( btype != null && btype.Length > 1 )
                {
                    tw.WriteElementString( _strValCarrDatTp, eUValueCarrierDatType.Array.ToString() ); // container class type
                    tw.WriteElementString( _strValCarrSharpTp, fi.FieldType.Name ); // system type name
                    tw.WriteElementString( _strValCarrItemSharpTpOfArr, btype[ 0 ] ); // array unit type
                    // get default value
                    if ( refDoc != null )
                    {
                        XmlNode nod = refDoc.SelectSingleNode( String.Format( "{0}/{1}", currnod, _strValCarrDefValue ) );
                        if ( nod != null ) tw.WriteElementString( _strValCarrDefValue, nod.InnerText );
                    }
                    // get current value
                    string cv = null;
                    Int32[] dimInfo = null;
                    if ( btype[ 0 ] == typeof( Byte ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<Byte>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( SByte ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<SByte>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( Int16 ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<Int16>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( UInt16 ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<UInt16>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( Int32 ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<Int32>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( UInt32 ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<UInt32>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( Int64 ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<Int64>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( UInt64 ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<UInt64>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( Double ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<Double>( fi.GetValue( val ), out dimInfo );
                    else if ( btype[ 0 ] == typeof( Single ).Name ) cv = UDatManBuffArrConverting.ConvertArrayT2String<Single>( fi.GetValue( val ), out dimInfo );
                    if ( !String.IsNullOrEmpty( cv ) && dimInfo != null )
                    {
                        StringBuilder sb = new StringBuilder();
                        for ( int i = 0 ; i < dimInfo.Length ; i++ )
                        {
                            if ( i == 0 ) sb.AppendFormat( "{0}", dimInfo[ i ] );
                            else sb.AppendFormat( ",{0}", dimInfo[ i ] );
                        }
                        tw.WriteElementString( _strValCarrValArrDim, sb.ToString() );
                        tw.WriteElementString( _strValCarrDatValue, cv );
                    }
                }
            }
            else if ( fi.FieldType.IsValueType || fi.FieldType == typeof( string ) )
            {
                tw.WriteElementString( _strValCarrDatTp, eUValueCarrierDatType.Single.ToString() ); // container class type
                tw.WriteElementString( _strValCarrSharpTp, fi.FieldType.Name ); // system type name
                // get default value
                if ( refDoc != null )
                {
                    XmlNode nod = null;
                    nod = refDoc.SelectSingleNode( String.Format( "{0}/{1}", currnod, _strValCarrDefValue ) );
                    if ( nod != null ) tw.WriteElementString( _strValCarrDefValue, nod.InnerText );
                    nod = refDoc.SelectSingleNode( String.Format( "{0}/{1}", currnod, _strValCarrValMin ) );
                    if ( nod != null ) tw.WriteElementString( _strValCarrValMin, nod.InnerText );
                    nod = refDoc.SelectSingleNode( String.Format( "{0}/{1}", currnod, _strValCarrValMax ) );
                    if ( nod != null ) tw.WriteElementString( _strValCarrValMax, nod.InnerText );
                    nod = refDoc.SelectSingleNode( String.Format( "{0}/{1}", currnod, _strValCarrValueScale ) );
                    if ( nod != null ) tw.WriteElementString( _strValCarrValueScale, nod.InnerText );
                }
                // get current value
                if ( fi.FieldType == typeof( string ) )
                    tw.WriteElementString( _strValCarrDatValue, ( string ) fi.GetValue( val ) );
                else
                    tw.WriteElementString( _strValCarrDatValue, fi.GetValue( val ).ToString() );
            }

            tw.WriteEndElement();
        }

        private static void WriteNestOne( object val, FieldInfo cfi, Type tp, string pn, XmlTextWriter tw, XmlDocument refDoc )
        {
            FieldInfo[] fi = tp.GetFields();
            if ( fi == null ) return;

            for ( int i = 0 ; i < fi.Length ; i++ )
            {
                if ( fi[ i ].FieldType.IsNested && !fi[ i ].FieldType.IsEnum )
                {
                    object obj = fi[ i ].GetValue( val );
                    string npn = String.Format( "{0}/{1}", pn, fi[ i ].Name );

                    if ( tw != null )
                        tw.WriteStartElement( fi[ i ].Name );

                    WriteNestOne( obj, cfi, fi[ i ].FieldType, npn, tw, refDoc );

                    if ( tw != null )
                        tw.WriteEndElement();
                }
                else
                {
                    WriteOne( val, fi[ i ], pn, tw, refDoc );
                }
            }
        }

        public static bool WriteTypeStruct<T>( T val, Stream ws, string refInfoPath )
        {
            if ( typeof( T ).IsClass ) return false; // not support class

            FieldInfo[] fi = val.GetType().GetFields();
            if ( fi == null || fi.Length <= 0 ) return false;

            // load xml
            XmlDocument doc = null;
            if ( File.Exists( refInfoPath ) )
            {
                bool bXML = false;
                using ( Stream rs = File.Open( refInfoPath, FileMode.Open ) )
                {
                    byte[] buf = new byte[ 3 ];
                    rs.Read( buf, 0, 3 );
                    if ( buf[ 0 ] == ( byte ) 0xEF && buf[ 1 ] == ( byte ) 0xBB && buf[ 2 ] == ( byte ) 0xBF )
                        bXML = true;
                }

                if ( bXML )
                {
                    doc = new XmlDocument();
                    doc.Load( refInfoPath );
                }
            }

            XmlTextWriter tw = null;
            try
            {
                if ( ws != null )
                {
                    tw = new XmlTextWriter( ws, Encoding.UTF8 );
                    tw.Formatting = Formatting.Indented;

                    tw.WriteStartDocument();
                    tw.WriteStartElement( typeof( T ).Name );
                }

                for ( int i = 0 ; i < fi.Length ; i++ )
                {
                    if ( fi[ i ].FieldType.IsNested && !fi[ i ].FieldType.IsEnum )
                    {
                        object obj = fi[ i ].GetValue( val );

                        if ( tw != null )
                            tw.WriteStartElement( fi[ i ].Name );

                        WriteNestOne( obj, fi[ i ], fi[ i ].FieldType, String.Format( "//{0}/{1}", typeof( T ).Name, fi[ i ].Name ), tw, doc );

                        if ( tw != null )
                            tw.WriteEndElement();
                    }
                    else
                        WriteOne( val, fi[ i ], String.Format( "//{0}", typeof( T ).Name ), tw, doc );
                }
            }
            finally
            {
                if ( tw != null )
                {
                    tw.WriteEndElement();
                    tw.WriteEndDocument();

                    tw.Flush();
                    tw.Close();
                }
            }

            return true;
        }

        #endregion

        #region Write a sample XML of structure item desc

        private static void WriteDescription( Type tp, XmlTextWriter tw )
        {
            if ( tp == null || tw == null ) return;

            string containerTp = null;

            if ( tp.IsEnum ) containerTp = eUValueCarrierDatType.Enumeration.ToString();
            else if ( tp.IsArray ) containerTp = eUValueCarrierDatType.Array.ToString();
            else if ( tp.IsValueType || tp == typeof( string ) ) containerTp = eUValueCarrierDatType.Single.ToString();

            if ( String.IsNullOrEmpty( containerTp ) ) return;

            tw.WriteElementString( _strValCarrDatTp, containerTp );
            tw.WriteElementString( _strValCarrSharpTp, tp.Name );
            if ( tp.IsArray )
            {
                string[] strs = tp.Name.Split( new char[ 1 ] { '[' } );
                if ( strs != null && strs.Length > 0 && !String.IsNullOrEmpty( strs[ 0 ] ) )
                    tw.WriteElementString( _strValCarrItemSharpTpOfArr, strs[ 0 ] );
            }
            tw.WriteElementString( _strValCarrDefValue, "" );
            tw.WriteElementString( _strValCarrValMax, "" );
            tw.WriteElementString( _strValCarrValMin, "" );
            tw.WriteElementString( _strValCarrValueScale, "" );
            if ( tp.IsEnum )
            {
                string[] ls = Enum.GetNames( tp );
                if ( ls != null && ls.Length > 0 )
                {
                    for ( int i = 0 ; i < ls.Length ; i++ ) tw.WriteElementString( _strValCarrValEnumList, ls[ i ] );
                }
            }
            else
                tw.WriteElementString( _strValCarrValEnumList, "" );
        }

        private static void WriteStructXml( Type tp, XmlTextWriter tw )
        {
            if ( tp == null || tw == null ) return;

            FieldInfo[] fi = tp.GetFields();
            if ( fi == null || fi.Length <= 0 ) return;

            for ( int i = 0 ; i < fi.Length ; i++ )
            {
                if ( !fi[ i ].IsPublic ) continue;

                if ( fi[ i ].FieldType.IsNested && !fi[ i ].FieldType.IsEnum )
                {
                    FieldInfo[] tmp = fi[ i ].FieldType.GetFields();
                    if ( tmp != null && tmp.Length > 0 )
                    {
                        tw.WriteStartElement( fi[ i ].Name );
                        WriteStructXml( fi[ i ].FieldType, tw );
                        tw.WriteEndElement();
                    }
                }
                else
                {
                    tw.WriteStartElement( fi[ i ].Name );
                    WriteDescription( fi[ i ].FieldType, tw );
                    tw.WriteEndElement();
                }
            }

        }

        public static bool WriteStructure<T>( string filePath )
        {
            if ( String.IsNullOrEmpty( filePath ) ) return false;

            FieldInfo[] layer0 = typeof( T ).GetFields();
            if ( layer0 == null || layer0.Length <= 0 ) return false;

            using ( Stream ws = File.Open( filePath, FileMode.Create ) )
            {
                XmlTextWriter tw = new XmlTextWriter( ws, Encoding.UTF8 );
                tw.Formatting = Formatting.Indented;
                tw.WriteStartDocument();

                tw.WriteStartElement( typeof( T ).Name );

                for ( int i = 0 ; i < layer0.Length ; i++ )
                {
                    if ( !layer0[ i ].IsPublic ) continue; // only public field can be.

                    if ( layer0[ i ].FieldType.IsNested && !layer0[ i ].FieldType.IsEnum )
                    {
                        FieldInfo[] curr = layer0[ i ].FieldType.GetFields();
                        if ( curr != null && curr.Length > 0 )
                        {
                            tw.WriteStartElement( layer0[ i ].Name );
                            WriteStructXml( layer0[ i ].FieldType, tw );
                            tw.WriteEndElement();
                        }
                    }
                    else
                    {
                        tw.WriteStartElement( layer0[ i ].Name );
                        WriteDescription( layer0[ i ].FieldType, tw );
                        tw.WriteEndElement();
                    }
                }

                tw.WriteEndElement();

                tw.WriteEndDocument();
                tw.Flush();
                tw.Close();
            }

            return true;
        }

        #endregion
    }

}

