using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;

using uIP.LibBase.Utility;

namespace uIP.LibBase.DataCarrier
{
    internal static class UDrawingCarrierUtilities
    {
        internal static bool StringToBool( string str, ref bool ret, bool def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret = def; return false;
            }

            bool bok = true, tmp = false;
            try { tmp = Convert.ToBoolean( str ); }
            catch { bok = false; }

            ret = bok ? tmp : def;

            return bok;
        }

        internal static bool StringTo2dCoor( string str, ref tUDrawingCarrier2DCoor ret, tUDrawingCarrier2DCoor def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret.X = def.X; ret.Y = def.Y; return false;
            }

            string[] vals = str.Split( new char[ 1 ] { ',' } );
            if ( vals == null || vals.Length < 2 )
            {
                ret.X = def.X; ret.Y = def.Y; return false;
            }

            tUDrawingCarrier2DCoor pos = new tUDrawingCarrier2DCoor( def.X, def.Y );
            bool bok = true;

            try
            {
                pos.X = Convert.ToInt32( vals[ 0 ] );
                pos.Y = Convert.ToInt32( vals[ 1 ] );
            }
            catch { bok = false; }

            if ( bok ) { ret.X = pos.X; ret.Y = pos.Y; }
            return bok;
        }

        internal static bool StringToRGB( string str, ref tUDrawingCarrierRGB ret, tUDrawingCarrierRGB def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret.R = def.R; ret.G = def.G; ret.B = def.B; return false;
            }

            string[] vals = str.Split( new char[ 1 ] { ',' } );
            if ( vals == null || vals.Length < 3 )
            {
                ret.R = def.R; ret.G = def.G; ret.B = def.B; return false;
            }

            tUDrawingCarrierRGB col = new tUDrawingCarrierRGB( def.R, def.G, def.B );
            bool bok = true;

            try
            {
                col.R = Convert.ToByte( vals[ 0 ] );
                col.G = Convert.ToByte( vals[ 1 ] );
                col.B = Convert.ToByte( vals[ 2 ] );
            }
            catch { bok = false; }

            if ( bok ) { ret.R = col.R; ret.G = col.G; ret.B = col.B; }

            return bok;
        }

        internal static bool StringToInt32( string str, ref Int32 ret, Int32 def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret = def; return false;
            }

            bool bok = true;
            Int32 tmp = def;
            try { tmp = Convert.ToInt32( str ); }
            catch { bok = false; }

            ret = bok ? tmp : def;

            return bok;
        }

        internal static bool StringToEnum<T>( string str, ref T ret, T def )
        {
            if ( !typeof( T ).IsEnum ) return false;
            if ( String.IsNullOrEmpty( str ) ) { ret = def; return false; }

            T tmp = def;
            bool bok = true;

            if ( Enum.IsDefined( typeof( T ), str ) )
                tmp = ( T ) Enum.Parse( typeof( T ), str );
            else
                bok = false;

            //try { tmp = ( T ) Enum.Parse( typeof( T ), str ); }
            //catch { bok = false; }

            ret = bok ? tmp : def;
            return bok;
        }

        internal static bool StringToLine2dCoor( string str, ref UDrawingCarrierLine2d ret, UDrawingCarrierLine2d def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret._EP1.X = def._EP1.X; ret._EP1.Y = def._EP1.Y;
                ret._EP2.X = def._EP2.X; ret._EP2.Y = def._EP2.Y;
                return false;
            }

            string[] vals = str.Split( new char[ 1 ] { ',' } );
            if ( vals == null || vals.Length < 4 )
            {
                ret._EP1.X = def._EP1.X; ret._EP1.Y = def._EP1.Y;
                ret._EP2.X = def._EP2.X; ret._EP2.Y = def._EP2.Y;
                return false;
            }

            UDrawingCarrierLine2d line = new UDrawingCarrierLine2d( def._EP1.X, def._EP1.Y, def._EP2.X, def._EP2.Y );
            bool bok = true;

            try
            {
                line._EP1.X = Convert.ToInt32( vals[ 0 ] );
                line._EP1.Y = Convert.ToInt32( vals[ 1 ] );
                line._EP2.X = Convert.ToInt32( vals[ 2 ] );
                line._EP2.Y = Convert.ToInt32( vals[ 3 ] );
            }
            catch { bok = false; }

            if ( bok )
            {
                ret._EP1.X = line._EP1.X; ret._EP1.Y = line._EP1.Y;
                ret._EP2.X = line._EP2.X; ret._EP2.Y = line._EP2.Y;
            }
            return bok;
        }

        internal static bool StringToRect( string str, ref tUDrawingCarrierRect ret, tUDrawingCarrierRect def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret.Left = def.Left; ret.Top = def.Top;
                ret.Right = def.Right; ret.Bottom = def.Bottom;
                return false;
            }

            string[] vals = str.Split( new char[ 1 ] { ',' } );
            if ( vals == null || vals.Length < 4 )
            {
                ret.Left = def.Left; ret.Top = def.Top;
                ret.Right = def.Right; ret.Bottom = def.Bottom;
                return false;
            }

            tUDrawingCarrierRect rect = new tUDrawingCarrierRect( def.Left, def.Top, def.Right, def.Bottom );
            bool bok = true;

            try
            {
                rect.Left = Convert.ToInt32( vals[ 0 ] );
                rect.Top = Convert.ToInt32( vals[ 1 ] );
                rect.Right = Convert.ToInt32( vals[ 2 ] );
                rect.Bottom = Convert.ToInt32( vals[ 3 ] );
            }
            catch { bok = false; }

            if ( bok )
            {
                ret.Left = rect.Left; ret.Top = rect.Top;
                ret.Right = rect.Right; ret.Bottom = rect.Bottom;
            }
            return bok;
        }

        internal static bool StringToBuffPixColor( string str, ref UDarawingCarrierPixColor ret, UDarawingCarrierPixColor def )
        {
            if ( String.IsNullOrEmpty( str ) )
            {
                ret.Val = def.Val;
                ret.Color.R = def.Color.R;
                ret.Color.G = def.Color.G;
                ret.Color.B = def.Color.B;
                return false;
            }

            string[] vals = str.Split( new char[ 1 ] { ',' } );
            if ( vals == null || vals.Length < 4 )
            {
                ret.Val = def.Val;
                ret.Color.R = def.Color.R;
                ret.Color.G = def.Color.G;
                ret.Color.B = def.Color.B;
                return false;
            }

            UDarawingCarrierPixColor tmp = new UDarawingCarrierPixColor( def.Val, def.Color );
            bool bok = true;

            try
            {
                tmp.Val = Convert.ToByte( vals[ 0 ] );
                tmp.Color.R = Convert.ToByte( vals[ 1 ] );
                tmp.Color.G = Convert.ToByte( vals[ 2 ] );
                tmp.Color.B = Convert.ToByte( vals[ 3 ] );
            }
            catch { bok = false; }

            if ( bok )
            {
                ret.Val = tmp.Val;
                ret.Color.R = tmp.Color.R;
                ret.Color.G = tmp.Color.G;
                ret.Color.B = tmp.Color.B;
            }
            return bok;
        }

        private static byte[] ConvertBmp( Bitmap bmp, ref Int32 bits )
        {
            bits = 0;
            if ( bmp == null ) return null;
            if ( bmp.Width <= 0 || bmp.Height <= 0 ) return null;
            if ( bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed &&
                 bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb )
                return null;

            byte[] ret = null;
            UInt32 nLineCpcnt = 0;
            UInt32 bufsz = 0;
            bool berr = false;

            if ( bmp.PixelFormat == PixelFormat.Format24bppRgb )
            {
                bufsz = ( UInt32 ) bmp.Width * ( UInt32 ) bmp.Height * 3;
                nLineCpcnt = ( UInt32 ) bmp.Width * 3;
                ret = new byte[ bufsz ];
            }
            else
            {
                bufsz = ( UInt32 ) bmp.Width * ( UInt32 ) bmp.Height;
                nLineCpcnt = ( UInt32 ) bmp.Width;
                ret = new byte[ bufsz ];
            }
            if ( ret == null ) return null;

            BitmapData dat = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, bmp.PixelFormat );
            if ( dat == null ) return null;

            unsafe
            {
                fixed ( byte* p8Dst = ret )
                {
                    UInt32 dstcnt = 0;
                    byte* pLineBeg = ( byte* ) (dat.Scan0.ToPointer());
                    for ( int y = 0 ; y < bmp.Height ; y++, pLineBeg += dat.Stride )
                    {
                        for ( UInt32 x = 0 ; x < nLineCpcnt ; x++ )
                        {
                            p8Dst[ dstcnt++ ] = pLineBeg[ x ];
                            if ( dstcnt > bufsz )
                                break;
                        }
                        if ( dstcnt > bufsz )
                        {
                            berr = true; break;
                        }
                    }
                }
            }

            bmp.UnlockBits( dat );

            if ( berr ) return null;

            bits = bmp.PixelFormat == PixelFormat.Format24bppRgb ? 24 : 8;
            return ret;
        }

        private static byte[] ConvertImage( UDrawingCarrierImage img, ref Int32 lineW )
        {
            lineW = 0;
            if ( img == null ) return null;
            if ( img.Width <= 0 || img.Height <= 0 )
                return null;
            if ( img.Buff1 == IntPtr.Zero && img.Buff2 == null )
                return null;
            if ( img.Format != 8 && img.Format != 24 )
                return null;
            if ( img.Buff2 != null ) { lineW = img.Line; return img.Buff2; }

            UInt32 bufsz = 0;
            UInt32 cpW = 0;
            bool berr = false;
            byte[] ret = null;
            UInt32 nDstBufCnt = 0;
            UInt32 nLineMv = 0;

            if ( img.Format == 8 )
            {
                bufsz = ( UInt32 ) img.Width * ( UInt32 ) img.Height;
                nLineMv = ( UInt32 ) (img.Line > img.Width ? img.Line : img.Width);
                cpW = ( UInt32 ) img.Width;
            }
            else
            {
                bufsz = ( UInt32 ) img.Width * ( UInt32 ) img.Height * 3;
                nLineMv = ( UInt32 ) (img.Line > (img.Width * 3) ? img.Line : (img.Width * 3));
                cpW = ( UInt32 ) (img.Width * 3);
            }
            ret = new byte[ bufsz ];

            unsafe
            {
                fixed ( byte* p8Dst = ret )
                {
                    byte* pSrcLineBeg = ( byte* ) (img.Buff1.ToPointer());
                    for ( int y = 0 ; y < img.Height ; y++, pSrcLineBeg += nLineMv )
                    {
                        for ( UInt32 x = 0 ; x < cpW ; x++ )
                        {
                            ret[ nDstBufCnt++ ] = pSrcLineBeg[ x ];
                            if ( nDstBufCnt > bufsz )
                                break;
                        }
                        if ( nDstBufCnt > bufsz )
                        {
                            berr = true; break;
                        }
                    }
                }
            }

            if ( berr ) return null;
            return ret;
        }

        internal static void WriteOutXml( object inst, XmlTextWriter tw )
        {
            if ( inst == null || tw == null ) return;

            // only public field can be read
            FieldInfo[] fields = inst.GetType().GetFields();
            if ( fields == null || fields.Length <= 0 ) return;

            foreach ( FieldInfo info in fields )
            {
                if ( info == null ) continue;
                if ( info.FieldType == typeof( bool ) )
                {
                    bool val = ( bool ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, val.ToString() );
                }
                else if ( info.FieldType == typeof( Int32 ) )
                {
                    Int32 val = ( Int32 ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, val.ToString() );
                }
                else if ( info.FieldType == typeof( string ) )
                {
                    string str = ( string ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, str );
                }
                else if ( info.FieldType == typeof( tUDrawingCarrier2DCoor ) )
                {
                    tUDrawingCarrier2DCoor val = ( tUDrawingCarrier2DCoor ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, String.Format( "{0},{1}", val.X, val.Y ) );
                }
                else if ( info.FieldType == typeof( List<tUDrawingCarrier2DCoor> ) )
                {
                    List<tUDrawingCarrier2DCoor> val = ( List<tUDrawingCarrier2DCoor> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                            tw.WriteElementString( info.Name, String.Format( "{0},{1}", val[ i ].X, val[ i ].Y ) );
                    }
                }
                else if ( info.FieldType == typeof( eUDrawingCarrierIntersectionType ) )
                {
                    eUDrawingCarrierIntersectionType val = ( eUDrawingCarrierIntersectionType ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, val.ToString() );
                }
                else if ( info.FieldType == typeof( tUDrawingCarrierRGB ) )
                {
                    tUDrawingCarrierRGB val = ( tUDrawingCarrierRGB ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, String.Format( "{0},{1},{2}", ( int ) val.R, ( int ) val.G, ( int ) val.B ) );
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierLine2d> ) )
                {
                    List<UDrawingCarrierLine2d> val = ( List<UDrawingCarrierLine2d> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                            tw.WriteElementString( info.Name, String.Format( "{0},{1},{2},{3}", val[ i ]._EP1.X, val[ i ]._EP1.Y, val[ i ]._EP2.X, val[ i ]._EP2.Y ) );
                    }
                }
                else if ( info.FieldType == typeof( eUDrawingCarrierShapeFillType ) )
                {
                    eUDrawingCarrierShapeFillType val = ( eUDrawingCarrierShapeFillType ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    tw.WriteElementString( info.Name, val.ToString() );
                }
                else if ( info.FieldType == typeof( List<tUDrawingCarrierRect> ) )
                {
                    List<tUDrawingCarrierRect> val = ( List<tUDrawingCarrierRect> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                            tw.WriteElementString( info.Name, String.Format( "{0},{1},{2},{3}", val[ i ].Left, val[ i ].Top, val[ i ].Right, val[ i ].Bottom ) );
                    }
                }
                else if ( info.FieldType == typeof( List<tUDrawingCarrier2DCoor[]> ) )
                {
                    List<tUDrawingCarrier2DCoor[]> val = ( List<tUDrawingCarrier2DCoor[]> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                        {
                            if ( val[ i ] != null && val[ i ].Length > 0 )
                            {
                                tw.WriteStartElement( info.Name );
                                for ( int j = 0 ; j < val[ i ].Length ; j++ )
                                    tw.WriteElementString( "pt", String.Format( "{0},{1}", val[ i ][ j ].X, val[ i ][ j ].Y ) );
                                tw.WriteEndElement();
                            }
                        }
                    }
                }
                else if ( info.FieldType == typeof( List<UDarawingCarrierPixColor> ) )
                {
                    List<UDarawingCarrierPixColor> val = ( List<UDarawingCarrierPixColor> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                            tw.WriteElementString( info.Name, String.Format( "{0},{1},{2},{3}", ( int ) val[ i ].Val, ( int ) val[ i ].Color.R, ( int ) val[ i ].Color.G, ( int ) val[ i ].Color.B ) );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierPixBuff> ) )
                {
                    List<UDrawingCarrierPixBuff> val = ( List<UDrawingCarrierPixBuff> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        Int32[] dim = null;
                        for ( int i = 0 ; i < val.Count ; i++ )
                        {
                            if ( val[ i ] == null || val[ i ].Buff == null || val[ i ].Width <= 0 || val[ i ].Height <= 0 ) continue;
                            tw.WriteStartElement( info.Name );

                            tw.WriteElementString( "X", val[ i ].X.ToString() );
                            tw.WriteElementString( "Y", val[ i ].Y.ToString() );
                            tw.WriteElementString( "W", val[ i ].Width.ToString() );
                            tw.WriteElementString( "H", val[ i ].Height.ToString() );
                            tw.WriteElementString( "B", UDatManBuffArrConverting.ConvertArrayT2String<byte>( val[ i ].Buff, out dim ) );

                            tw.WriteEndElement();
                        }
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierHBitmap> ) )
                {
                    List<UDrawingCarrierHBitmap> val = ( List<UDrawingCarrierHBitmap> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        Int32[] dim = null;
                        for ( int i = 0 ; i < val.Count ; i++ )
                        {
                            if ( val[ i ] == null || val[ i ].hBitmap == IntPtr.Zero ) continue;
                            Bitmap bmp = Bitmap.FromHbitmap( val[ i ].hBitmap );
                            if ( bmp == null || bmp.Width <= 0 || bmp.Height <= 0 )
                            {
                                if ( bmp != null ) bmp.Dispose();
                                bmp = null;
                                continue;
                            }

                            int bits = 0;
                            byte[] buffer = ConvertBmp( bmp, ref bits );
                            if ( buffer == null ) continue;

                            tw.WriteStartElement( info.Name );

                            tw.WriteElementString( "X", val[ i ].X.ToString() );
                            tw.WriteElementString( "Y", val[ i ].Y.ToString() );
                            tw.WriteElementString( "W", bmp.Width.ToString() );
                            tw.WriteElementString( "H", bmp.Height.ToString() );
                            tw.WriteElementString( "F", bits.ToString() );
                            tw.WriteElementString( "B", UDatManBuffArrConverting.ConvertArrayT2String<byte>( buffer, out dim ) );

                            tw.WriteEndElement();
                        }
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierBitmap> ) )
                {
                    List<UDrawingCarrierBitmap> val = ( List<UDrawingCarrierBitmap> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        Int32[] dim = null;
                        for ( int i = 0 ; i < val.Count ; i++ )
                        {
                            if ( val[ i ] == null || val[ i ].Bmp == null ) continue;
                            int bits = 0;
                            byte[] buffer = ConvertBmp( val[ i ].Bmp, ref bits );
                            if ( buffer == null ) continue;

                            tw.WriteStartElement( info.Name );

                            tw.WriteElementString( "X", val[ i ].X.ToString() );
                            tw.WriteElementString( "Y", val[ i ].Y.ToString() );
                            tw.WriteElementString( "W", val[ i ].Bmp.Width.ToString() );
                            tw.WriteElementString( "H", val[ i ].Bmp.Height.ToString() );
                            tw.WriteElementString( "F", bits.ToString() );
                            tw.WriteElementString( "B", UDatManBuffArrConverting.ConvertArrayT2String<byte>( buffer, out dim ) );

                            tw.WriteEndElement();
                        }
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierImage> ) )
                {
                    List<UDrawingCarrierImage> val = ( List<UDrawingCarrierImage> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                        {
                            if ( val[ i ] == null || val[ i ].Width <= 0 || val[ i ].Height <= 0 || val[ i ].Format <= 0 )
                                continue;
                            if ( val[ i ].Buff1 == IntPtr.Zero && val[ i ].Buff2 == null )
                                continue;

                            Int32 lineW = 0;
                            Int32[] dim = null;
                            byte[] dat = ConvertImage( val[ i ], ref lineW );
                            if ( dat == null ) continue;

                            tw.WriteStartElement( info.Name );

                            tw.WriteElementString( "X", val[ i ].X.ToString() );
                            tw.WriteElementString( "Y", val[ i ].Y.ToString() );
                            tw.WriteElementString( "W", val[ i ].Width.ToString() );
                            tw.WriteElementString( "H", val[ i ].Height.ToString() );
                            tw.WriteElementString( "F", val[ i ].Format.ToString() );
                            tw.WriteElementString( "L", lineW.ToString() );
                            tw.WriteElementString( "B", UDatManBuffArrConverting.ConvertArrayT2String<byte>( dat, out dim ) );

                            tw.WriteEndElement();
                        }
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierText> ) )
                {
                    List<UDrawingCarrierText> val = ( List<UDrawingCarrierText> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                    if ( val != null )
                    {
                        for ( int i = 0 ; i < val.Count ; i++ )
                        {
                            if ( val[ i ] == null ) continue;
                            tw.WriteStartElement( info.Name );

                            tw.WriteElementString( "X", val[ i ].X.ToString() );
                            tw.WriteElementString( "Y", val[ i ].Y.ToString() );
                            tw.WriteElementString( "T", val[ i ].Txt );

                            tw.WriteEndElement();
                        }
                    }
                }
                else
                    Console.WriteLine( "[UDrawingCarriers::WriteDatCarrXml] get un-process field = {0}.", info.Name );
            }
        }

        internal static void ReadInXml( object inst, XmlNode nod )
        {
            if ( inst == null || nod == null ) return;

            // only public field can be read
            FieldInfo[] fields = inst.GetType().GetFields();
            if ( fields == null || fields.Length <= 0 ) return;

            foreach ( FieldInfo info in fields )
            {
                if ( info == null ) continue;
                if ( info.FieldType == typeof( bool ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        bool curr = ( bool ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( StringToBool( nn.InnerText, ref curr, curr ) )
                            inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { curr } );
                    }
                }
                else if ( info.FieldType == typeof( Int32 ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        Int32 curr = ( Int32 ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( StringToInt32( nn.InnerText, ref curr, curr ) )
                            inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { curr } );
                    }
                }
                else if ( info.FieldType == typeof( string ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { String.IsNullOrEmpty( nn.InnerText ) ? null : String.Copy( nn.InnerText ) } );
                    }
                }
                else if ( info.FieldType == typeof( tUDrawingCarrier2DCoor ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        tUDrawingCarrier2DCoor curr = ( tUDrawingCarrier2DCoor ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( StringTo2dCoor( nn.InnerText, ref curr, curr ) )
                            inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { curr } );
                    }
                }
                else if ( info.FieldType == typeof( List<tUDrawingCarrier2DCoor> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<tUDrawingCarrier2DCoor> val = ( List<tUDrawingCarrier2DCoor> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<tUDrawingCarrier2DCoor>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            tUDrawingCarrier2DCoor curr = new tUDrawingCarrier2DCoor();
                            if ( StringTo2dCoor( nnl[ i ].InnerText, ref curr, curr ) )
                                val.Add( new tUDrawingCarrier2DCoor( curr.X, curr.Y ) );
                        }

                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( eUDrawingCarrierIntersectionType ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        eUDrawingCarrierIntersectionType val = ( eUDrawingCarrierIntersectionType ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( StringToEnum<eUDrawingCarrierIntersectionType>( nn.InnerText, ref val, val ) )
                            inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( tUDrawingCarrierRGB ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        tUDrawingCarrierRGB val = ( tUDrawingCarrierRGB ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( StringToRGB( nn.InnerText, ref val, val ) )
                            inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierLine2d> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDrawingCarrierLine2d> val = ( List<UDrawingCarrierLine2d> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDrawingCarrierLine2d>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            UDrawingCarrierLine2d ln = new UDrawingCarrierLine2d();
                            if ( StringToLine2dCoor( nnl[ i ].InnerText, ref ln, ln ) )
                                val.Add( ln );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( eUDrawingCarrierShapeFillType ) )
                {
                    XmlNode nn = nod.SelectSingleNode( info.Name );
                    if ( nn != null )
                    {
                        eUDrawingCarrierShapeFillType val = ( eUDrawingCarrierShapeFillType ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( StringToEnum<eUDrawingCarrierShapeFillType>( nn.InnerText, ref val, val ) )
                            inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<tUDrawingCarrierRect> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<tUDrawingCarrierRect> val = ( List<tUDrawingCarrierRect> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<tUDrawingCarrierRect>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            tUDrawingCarrierRect ln = new tUDrawingCarrierRect();
                            if ( StringToRect( nnl[ i ].InnerText, ref ln, ln ) )
                                val.Add( new tUDrawingCarrierRect( ln.Left, ln.Top, ln.Right, ln.Bottom ) );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<tUDrawingCarrier2DCoor[]> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<tUDrawingCarrier2DCoor[]> val = ( List<tUDrawingCarrier2DCoor[]> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<tUDrawingCarrier2DCoor[]>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            XmlNodeList nn2 = nnl[ i ].SelectNodes( "pt" );
                            if ( nn2 == null || nn2.Count <= 0 ) continue;

                            List<tUDrawingCarrier2DCoor> ll2 = new List<tUDrawingCarrier2DCoor>();
                            bool berr = false;
                            for ( int j = 0 ; j < nn2.Count ; j++ )
                            {
                                tUDrawingCarrier2DCoor curr = new tUDrawingCarrier2DCoor();
                                if ( StringTo2dCoor( nn2[ j ].InnerText, ref curr, curr ) )
                                    ll2.Add( new tUDrawingCarrier2DCoor( curr.X, curr.Y ) );
                                else
                                {
                                    berr = true; break;
                                }
                            }
                            if ( !berr && ll2.Count > 0 )
                                val.Add( ll2.ToArray() );
                        }

                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDarawingCarrierPixColor> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDarawingCarrierPixColor> val = ( List<UDarawingCarrierPixColor> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDarawingCarrierPixColor>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            UDarawingCarrierPixColor cur = new UDarawingCarrierPixColor();
                            if ( StringToBuffPixColor( nnl[ i ].InnerText, ref cur, cur ) )
                                val.Add( cur );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierPixBuff> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDrawingCarrierPixBuff> val = ( List<UDrawingCarrierPixBuff> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDrawingCarrierPixBuff>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            if ( nnl[ i ] == null ) continue;

                            UDrawingCarrierPixBuff curr = new UDrawingCarrierPixBuff();

                            XmlNode nn2 = nnl[ i ].SelectSingleNode( "X" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.X, curr.X ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "Y" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Y, curr.Y ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "W" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Width, curr.Width ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "H" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Height, curr.Height ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "B" );
                            if ( nn2 == null ) continue;
                            byte[] barr = Encoding.UTF8.GetBytes( nn2.InnerText );
                            if ( barr == null ) continue;
                            curr.Buff = ( byte[] ) UDatManBuffArrConverting.ReverseByteBuffer<byte>( barr, new Int32[ 1 ] { barr.Length / 2 } ); // 2-byte ascii -> 1-byte value
                            if ( curr.Buff == null ) continue;

                            val.Add( curr );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierHBitmap> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDrawingCarrierHBitmap> val = ( List<UDrawingCarrierHBitmap> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDrawingCarrierHBitmap>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            if ( nnl[ i ] == null ) continue;

                            UDrawingCarrierHBitmap curr = new UDrawingCarrierHBitmap();

                            XmlNode nn2 = nnl[ i ].SelectSingleNode( "X" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.X, curr.X ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "Y" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Y, curr.Y ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "W" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Width, curr.Width ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "H" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Height, curr.Height ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "F" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Format, curr.Format ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "B" );
                            if ( nn2 == null ) continue;
                            byte[] barr = Encoding.UTF8.GetBytes( nn2.InnerText );
                            if ( barr == null ) continue;
                            curr.Buffer = ( byte[] ) UDatManBuffArrConverting.ReverseByteBuffer<byte>( barr, new Int32[ 1 ] { barr.Length / 2 } ); // 2-byte ascii -> 1-byte value
                            if ( curr.Buffer == null ) continue;

                            val.Add( curr );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierBitmap> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDrawingCarrierBitmap> val = ( List<UDrawingCarrierBitmap> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDrawingCarrierBitmap>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            if ( nnl[ i ] == null ) continue;

                            UDrawingCarrierBitmap curr = new UDrawingCarrierBitmap();
                            Int32 width = 0, height = 0, format = 0;

                            XmlNode nn2 = nnl[ i ].SelectSingleNode( "X" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.X, curr.X ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "Y" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Y, curr.Y ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "W" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref width, width ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "H" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref height, height ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "F" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref format, format ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "B" );
                            if ( nn2 == null ) continue;
                            byte[] barr = Encoding.UTF8.GetBytes( nn2.InnerText );
                            if ( barr == null ) continue;
                            byte[] buff = ( byte[] ) UDatManBuffArrConverting.ReverseByteBuffer<byte>( barr, new Int32[ 1 ] { barr.Length / 2 } ); // 2-byte ascii -> 1-byte value
                            if ( buff == null ) continue;

                            // process to image
                            curr.Bmp = UBitmapUtilities.NewBmp( buff, width, height, format );
                            curr.bHandle = true;
                            if ( curr.Bmp == null ) continue;

                            val.Add( curr );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierImage> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDrawingCarrierImage> val = ( List<UDrawingCarrierImage> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDrawingCarrierImage>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            if ( nnl[ i ] == null ) continue;

                            UDrawingCarrierImage curr = new UDrawingCarrierImage();

                            XmlNode nn2 = nnl[ i ].SelectSingleNode( "X" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.X, curr.X ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "Y" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Y, curr.Y ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "W" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Width, curr.Width ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "H" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Height, curr.Height ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "F" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Format, curr.Format ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "L" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Line, curr.Line ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "B" );
                            if ( nn2 == null ) continue;
                            byte[] barr = Encoding.UTF8.GetBytes( nn2.InnerText );
                            if ( barr == null ) continue;
                            curr.Buff2 = ( byte[] ) UDatManBuffArrConverting.ReverseByteBuffer<byte>( barr, new Int32[ 1 ] { barr.Length / 2 } ); // 2-byte ascii -> 1-byte value
                            if ( curr.Buff2 == null ) continue;

                            val.Add( curr );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
                else if ( info.FieldType == typeof( List<UDrawingCarrierText> ) )
                {
                    XmlNodeList nnl = nod.SelectNodes( info.Name );
                    if ( nnl != null && nnl.Count > 0 )
                    {
                        List<UDrawingCarrierText> val = ( List<UDrawingCarrierText> ) inst.GetType().InvokeMember( info.Name, BindingFlags.GetField, null, inst, null );
                        if ( val == null ) val = new List<UDrawingCarrierText>();

                        for ( int i = 0 ; i < nnl.Count ; i++ )
                        {
                            if ( nnl[ i ] == null ) continue;

                            UDrawingCarrierText curr = new UDrawingCarrierText();

                            XmlNode nn2 = nnl[ i ].SelectSingleNode( "X" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.X, curr.X ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "Y" );
                            if ( nn2 == null || !StringToInt32( nn2.InnerText, ref curr.Y, curr.Y ) ) continue;

                            nn2 = nnl[ i ].SelectSingleNode( "T" );
                            if ( String.IsNullOrEmpty( nn2.InnerText ) ) continue;
                            curr.Txt = String.Copy( nn2.InnerText );

                            val.Add( curr );
                        }
                        inst.GetType().InvokeMember( info.Name, BindingFlags.SetField, null, inst, new object[ 1 ] { val } );
                    }
                }
            }
        }

    }
}
