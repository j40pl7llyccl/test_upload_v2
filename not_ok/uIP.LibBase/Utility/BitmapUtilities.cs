using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.Utility
{
    public static class UBitmapUtilities
    {
        private static int CheckRange( int val, int max, int min )
        {
            val = val < min ? min : val;
            val = val > max ? max : val;
            return val;
        }

        public static void LoaBmpBit8( ref Bitmap bmp, string file )
        {
            if ( bmp != null ) { bmp.Dispose(); bmp = null; }
            if ( !System.IO.File.Exists( file ) ) return;

            using ( System.IO.FileStream rs = new System.IO.FileStream( file, System.IO.FileMode.Open ) )
            {
                bmp = new Bitmap( rs );
            }

            if ( bmp != null )
            {
                if ( bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed )
                {
                    bmp.Dispose(); bmp = null;
                }
            }
        }

        public static void LoadBmpBit24( ref Bitmap bmp, string file )
        {
            if ( bmp != null ) { bmp.Dispose(); bmp = null; }
            if ( !System.IO.File.Exists( file ) ) return;

            using ( System.IO.FileStream rs = new System.IO.FileStream( file, System.IO.FileMode.Open ) )
            {
                bmp = new Bitmap( rs );
            }

            if ( bmp != null )
            {
                if ( bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb )
                {
                    bmp.Dispose(); bmp = null;
                }
            }
        }

        public static Bitmap CopyBmpToBit24( Bitmap src )
        {
            if ( src == null || src.Width <= 0 || src.Height <= 0 )
                return null;
            if ( src.PixelFormat != PixelFormat.Format24bppRgb && src.PixelFormat != PixelFormat.Format8bppIndexed )
                return null;

            Bitmap tmpBmp = new Bitmap( src.Width, src.Height, PixelFormat.Format24bppRgb );
            if ( tmpBmp == null )
                return null;

            for ( int y = 0 ; y < src.Height ; y++ )
                for ( int x = 0 ; x < src.Width ; x++ )
                {
                    tmpBmp.SetPixel( x, y, src.GetPixel( x, y ) );
                }
            return tmpBmp;
        }

        public static Bitmap CopyBmp( Bitmap src )
        {
            if ( src == null )
                return null;
            return Trim2Bmp( src, 0, 0, src.Width, src.Height );
        }

        public static Bitmap Trim2Bmp( Bitmap src, int left, int top, int right, int bottom )
        {
            if ( src == null )
                return null;

            int tmp = 0;

            left = CheckRange( left, src.Width, 0 );
            right = CheckRange( right, src.Width, 0 );
            top = CheckRange( top, src.Height, 0 );
            bottom = CheckRange( bottom, src.Height, 0 );

            if ( left > right )
            {
                tmp = right;
                right = left;
                left = tmp;
            }
            if ( top > bottom )
            {
                tmp = bottom;
                bottom = top;
                top = tmp;
            }
            if ( top == bottom || left == right )
                return null;


            PixelFormat format = src.PixelFormat;

            if ( format != PixelFormat.Format8bppIndexed && format != PixelFormat.Format24bppRgb )
                return null;

            Rectangle rect = new Rectangle( left, top, right - left, bottom - top );

            Bitmap rtn = new Bitmap( rect.Width, rect.Height, format );

            int pixszbyte = 0;
            if ( format == PixelFormat.Format8bppIndexed )
            {
                ColorPalette pal = rtn.Palette;
                for ( int i = 0 ; i < pal.Entries.Length ; i++ )
                    pal.Entries[ i ] = Color.FromArgb( i, i, i );
                rtn.Palette = pal;
                pixszbyte = 1;
            }
            else
                pixszbyte = 3;

            int linesz = pixszbyte * rect.Width;
            byte[] intermediateData = new byte[ linesz ];
            BitmapData srcdata = src.LockBits( rect, ImageLockMode.ReadOnly, format );
            BitmapData dstdata = rtn.LockBits( new Rectangle( 0, 0, rect.Width, rect.Height ), ImageLockMode.ReadWrite, format );

            Int64 srcAddr = srcdata.Scan0.ToInt64();
            Int64 dstAddr = dstdata.Scan0.ToInt64();

            ColorPalette palette = src.PixelFormat == PixelFormat.Format8bppIndexed ? src.Palette : null;

            for ( int y = 0 ; y < rect.Height ; y++ )
            {
                if ( palette == null )
                {
                    // Copy from source bitmap
                    Marshal.Copy( new IntPtr( srcAddr ), intermediateData, 0, linesz );
                    // Put into dsetination bitmap
                    Marshal.Copy( intermediateData, 0, new IntPtr( dstAddr ), linesz );
                }
                else
                {
                    unsafe
                    {
                        IntPtr srcIntptr = new IntPtr( srcAddr );
                        IntPtr dstIntptr = new IntPtr( dstAddr );
                        byte* pSrc8 = ( byte* ) srcIntptr.ToPointer();
                        byte* pDst8 = ( byte* ) dstIntptr.ToPointer();
                        for ( int x = 0 ; x < rect.Width ; x++ )
                        {
                            pDst8[ x ] = palette.Entries[ pSrc8[ x ] ].R;
                        }
                    }
                }
                // Move address.
                srcAddr += srcdata.Stride;
                dstAddr += dstdata.Stride;
            }

            if ( palette != null )
                src.Palette = palette;

            src.UnlockBits( srcdata );
            rtn.UnlockBits( dstdata );

            return rtn;
        }

        public static byte[] CopyBmp( Bitmap srcBmp, ref int retW, ref int retH, ref int retF )
        {
            if ( srcBmp == null )
            {
                retW = retH = retF = 0;
                return null;
            }

            retW = retH = retF = 0;
            if ( srcBmp.Width <= 0 || srcBmp.Height <= 0 || srcBmp.PixelFormat != PixelFormat.Format8bppIndexed )
                return null;

            byte[] pixBuf = new byte[ srcBmp.Width * srcBmp.Height ];
            if ( pixBuf == null )
                return null;

            PixelFormat format = PixelFormat.Format8bppIndexed;

            BitmapData bdata = srcBmp.LockBits( new Rectangle( 0, 0, srcBmp.Width, srcBmp.Height ), ImageLockMode.ReadOnly, format );

            ColorPalette palette = srcBmp.PixelFormat == PixelFormat.Format8bppIndexed ? srcBmp.Palette : null;

            int offset = 0;
            long ptr = bdata.Scan0.ToInt64();
            int cnt = 0;
            for ( int y = 0 ; y < srcBmp.Height ; y++ )
            {
                if ( palette != null )
                {
                    unsafe
                    {
                        IntPtr pAddr = new IntPtr( ptr );
                        byte* pSrc = ( byte* ) pAddr.ToPointer();
                        for ( int x = 0 ; x < srcBmp.Width ; x++ )
                        {
                            pixBuf[ cnt++ ] = palette.Entries[ pSrc[ x ] ].R;
                        }
                    }
                }
                else
                    Marshal.Copy( new IntPtr( ptr ), pixBuf, offset, srcBmp.Width );

                offset += srcBmp.Width;
                ptr += bdata.Stride;
            }

            if ( palette != null )
                srcBmp.Palette = palette;

            srcBmp.UnlockBits( bdata );

            retW = srcBmp.Width;
            retH = srcBmp.Height;
            retF = 8;
            return pixBuf;
        }

        public static IntPtr CopyBmpData( Bitmap srcBmp, ref int retW, ref int retH, ref int retF )
        {
            retW = retH = retF = 0;

            if ( srcBmp == null )
                return IntPtr.Zero;

            if ( srcBmp.Width <= 0 || srcBmp.Height <= 0 )
                return IntPtr.Zero;

            if ( srcBmp.PixelFormat != PixelFormat.Format8bppIndexed && srcBmp.PixelFormat != PixelFormat.Format24bppRgb )
                return IntPtr.Zero;

            BitmapData bdata = srcBmp.LockBits( new Rectangle( 0, 0, srcBmp.Width, srcBmp.Height ), ImageLockMode.ReadOnly, srcBmp.PixelFormat );
            if ( bdata == null )
                return IntPtr.Zero;

            ColorPalette palette = srcBmp.PixelFormat == PixelFormat.Format8bppIndexed ? srcBmp.Palette : null;

            int allocSize = srcBmp.Width * srcBmp.Height * (srcBmp.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 1);
            IntPtr allocBuff = Marshal.AllocHGlobal( allocSize );
            if ( allocBuff != IntPtr.Zero )
            {
                unsafe
                {
                    IntPtr srcIntptr = bdata.Scan0;
                    byte* pSrc = ( byte* ) srcIntptr.ToPointer();
                    int nSrcMv = bdata.Stride;

                    byte* pDst = ( byte* ) allocBuff.ToPointer();
                    int nDstMv = srcBmp.Width * (srcBmp.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 1);

                    int cplen = srcBmp.Width * (srcBmp.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 1);

                    for ( int y = 0 ; y < srcBmp.Height ; y++ )
                    {
                        if ( palette != null )
                        {
                            for ( int x = 0 ; x < srcBmp.Width ; x++ )
                                pDst[ x ] = palette.Entries[ pSrc[ x ] ].R;
                        }
                        else
                            MemWinSdkFunctions.NativeMemcpy( pDst, pSrc, new UIntPtr( Convert.ToUInt32( cplen ) ) );

                        pSrc += nSrcMv;
                        pDst += nDstMv;
                    }

                }
            }

            if ( palette != null )
                srcBmp.Palette = palette;

            srcBmp.UnlockBits( bdata );

            if ( allocBuff != IntPtr.Zero )
            {
                retW = srcBmp.Width;
                retH = srcBmp.Height;
                retF = srcBmp.PixelFormat == PixelFormat.Format8bppIndexed ? 8 : 24;
            }

            return allocBuff;
        }

        public static Bitmap NewBmp( byte[] img, int w, int h, int f )
        {
            if ( img == null || w <= 0 || h <= 0 )
                return null;
            if ( f != 8 && f != 24 )
                return null;

            PixelFormat format = f == 8 ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap( w, h, format );

            if ( bmp == null )
                return null;

            if ( f == 8 )
            {
                // Config the palette
                ColorPalette pal = bmp.Palette;
                for ( int i = 0 ; i < pal.Entries.Length ; i++ )
                    pal.Entries[ i ] = Color.FromArgb( i, i, i );
                bmp.Palette = pal;
            }


            BitmapData bmpdata = bmp.LockBits( new Rectangle( 0, 0, w, h ), ImageLockMode.ReadWrite, format );

            long bmpBuff = bmpdata.Scan0.ToInt64();
            long perLine = bmpdata.Stride;
            int srcPos = 0;
            int cpLen = w * (f == 8 ? 1 : 3);


            for ( int y = 0 ; y < h ; y++ )
            {
                Marshal.Copy( img, srcPos, new IntPtr( bmpBuff ), cpLen );
                srcPos += cpLen;
                bmpBuff += perLine;
            }

            bmp.UnlockBits( bmpdata );

            return bmp;
        }

        public static Bitmap NewBmp( IntPtr img, int w, int h, int f )
        {
            if ( img == null || w <= 0 || h <= 0 )
                return null;
            if ( f != 8 && f != 24 )
                return null;

            PixelFormat format = f == 8 ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap( w, h, format );
            if ( bmp == null )
                return null;

            if ( f == 8 )
            {
                ColorPalette pal = bmp.Palette;
                for ( int i = 0 ; i < pal.Entries.Length ; i++ )
                    pal.Entries[ i ] = Color.FromArgb( i, i, i );
                bmp.Palette = pal;
            }

            BitmapData bmpdata = bmp.LockBits( new Rectangle( 0, 0, w, h ), ImageLockMode.ReadWrite, format );

            long bmpBuff = bmpdata.Scan0.ToInt64();
            long srcBuff = img.ToInt64();
            int srcALine = w * (f == 8 ? 1 : 3);
            byte[] indirectCpBuff = new byte[ srcALine ];
            long bmpPerLine = bmpdata.Stride;

            for ( int y = 0 ; y < h ; y++ )
            {
                // Copy from src to byte[]
                Marshal.Copy( new IntPtr( srcBuff ), indirectCpBuff, 0, srcALine );
                // Copy from byte[] to bmp
                Marshal.Copy( indirectCpBuff, 0, new IntPtr( bmpBuff ), srcALine );

                bmpBuff += bmpPerLine;
                srcBuff += srcALine;
            }

            bmp.UnlockBits( bmpdata );

            return bmp;
        }

        public static void ResetBmpBit24( Bitmap bmp, Color col )
        {
            if ( bmp == null || bmp.PixelFormat != PixelFormat.Format24bppRgb ) return;

            Graphics g = Graphics.FromImage( bmp );
            if ( g == null ) return;

            g.FillRectangle( new SolidBrush( col ), new Rectangle( 0, 0, bmp.Width, bmp.Height ) );
            g.Dispose();
            g = null;
        }

        public static void ResetBmpBit8( Bitmap bmp, byte val )
        {
            if ( bmp == null || bmp.PixelFormat != PixelFormat.Format8bppIndexed ) return;

            BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed );

            unsafe
            {
                byte* pLine = ( byte* ) bdata.Scan0;
                for ( int y = 0 ; y < bmp.Height ; y++ )
                {
                    for ( int x = 0 ; x < bmp.Width ; x++ )
                        pLine[ x ] = val;
                    pLine += bdata.Stride;
                }
            }

            bmp.UnlockBits( bdata );
        }

        public static bool ImageBuff2Bitmap( ref Bitmap bmp, IntPtr pBuff, Int32 w, Int32 h, Int32 f, Int32 line )
        {
            bool realloc = false;
            int bitFormat = 0;
            int bmpw = 0;
            int bmph = 0;

            if ( w <= 0 || h <= 0 || ( f != 8 && f != 24 ) || pBuff.ToInt32() == 0 )
                return false;

            if ( bmp != null )
            {
                if ( bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb )
                    bitFormat = 24;
                else if ( bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed )
                    bitFormat = 8;

                bmpw = bmp.Width;
                bmph = bmp.Height;
            }

            if ( bmpw == 0 || bmph == 0 || bitFormat == 0 || bmpw != w || bmph != h || bitFormat != f )
                realloc = true;

            if ( realloc )
            {
                if ( bmp != null ) { bmp.Dispose(); bmp = null; }
                bmp = new Bitmap( w, h, f == 8 ? System.Drawing.Imaging.PixelFormat.Format8bppIndexed : System.Drawing.Imaging.PixelFormat.Format24bppRgb );
            }

            if ( bmp == null )
                return false;

            // Copy
            if ( f == 8 )
            {
                ColorPalette pal = bmp.Palette;
                for ( int i = 0; i < pal.Entries.Length; i++ )
                    pal.Entries[ i ] = Color.FromArgb( i, i, i );
                bmp.Palette = pal;

                BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed );

                long dstAddr = bdata.Scan0.ToInt64();
                long dstMv = bdata.Stride;

                long srcAddr = pBuff.ToInt64();
                long srcMv = line > w ? line : w;
                UInt32 cpLen = Convert.ToUInt32( bmp.Width );

                if ( dstMv == bmp.Width && w == line && dstMv == srcMv )
                {
                    UInt32 totalCp = Convert.ToUInt32( dstMv ) * Convert.ToUInt32( h );
                    MemWinSdkFunctions.NativeMemcpy(new IntPtr( dstAddr ), new IntPtr( srcAddr ), new UIntPtr( totalCp ) );
                }
                else
                {
                    for ( int y = 0; y < bmp.Height; y++, dstAddr += dstMv, srcAddr += srcMv )
                    {
                        MemWinSdkFunctions.NativeMemcpy(new IntPtr(dstAddr), new IntPtr(srcAddr), new UIntPtr(cpLen));
                    }
                }

                bmp.UnlockBits( bdata );
            }
            else
            {
                BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

                long dstAddr = bdata.Scan0.ToInt64();
                long dstMv = bdata.Stride;

                long srcAddr = pBuff.ToInt64();
                long srcMv = line > ( w * 3 ) ? line : ( w * 3 );
                UInt32 cpLen = Convert.ToUInt32( w * 3 );

                if ( ( w * 3 ) == line && dstMv == ( bmp.Width * 3 ) && w == bmp.Width )
                {
                    UInt32 totalCp = Convert.ToUInt32( cpLen ) * Convert.ToUInt32( h );
                    MemWinSdkFunctions.NativeMemcpy(new IntPtr(dstAddr), new IntPtr(srcAddr), new UIntPtr(totalCp));
                }
                else
                {
                    for ( int y = 0; y < bmp.Height; y++, dstAddr += dstMv, srcAddr += srcMv )
                    {
                        MemWinSdkFunctions.NativeMemcpy(new IntPtr(dstAddr), new IntPtr(srcAddr), new UIntPtr(cpLen));
                    }
                }

                bmp.UnlockBits( bdata );
            }

            return true;
        }

    }
}
