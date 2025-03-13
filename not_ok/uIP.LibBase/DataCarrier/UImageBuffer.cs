using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.DataCarrier
{
    // Size is packed. No reserved for alignment.
    public unsafe class UImageBuffer : SafeHandle
    {
        //private IntPtr m_UnmanageBuff;
        private UInt32 m_nWidth;
        private UInt32 m_nHeight;
        private UInt32 m_nPixBits;
        private Int64 m_nBuffSize;
        private UInt32 m_nPixBytes;
        private bool m_bDisposed = false;

        public override bool IsInvalid {
            get {
                if ( m_bDisposed )
                    return true;
                if ( handle == IntPtr.Zero )
                    return true;
                return false;
            }
        }

        protected override bool ReleaseHandle()
        {
            if ( !m_bDisposed ) {
                m_bDisposed = true;
                //GC.SuppressFinalize( this );
                if ( handle != IntPtr.Zero ) {
                    FreeMem( handle );
                    handle = IntPtr.Zero;
                }

                m_nWidth = 0;
                m_nHeight = 0;
                m_nPixBits = 0;
                m_nBuffSize = 0;
                m_nPixBytes = 0;
            }

            return true;
        }

        public IntPtr Buffer {
            get {
                if ( IsInvalid ) return IntPtr.Zero;
                return handle;
            }
        }
        public UInt32 Width { get { return m_nWidth; } }
        public UInt32 Height { get { return m_nHeight; } }
        public UInt32 Bits { get { return m_nPixBits; } }
        public Int64 Size { get { return m_nBuffSize; } }

        public bool SaveBmp( string path )
        {
            if ( IsInvalid || handle == IntPtr.Zero || m_nBuffSize == 0 )
                return false;

            if ( m_nPixBytes == 1 ) {
                if ( ( m_nWidth % 4 ) == 0 ) {
                    Bitmap bmp = new Bitmap(
                                             Convert.ToInt32( m_nWidth ),
                                             Convert.ToInt32( m_nHeight ),
                                             Convert.ToInt32( m_nWidth ), PixelFormat.Format8bppIndexed, handle );
                    if ( bmp != null ) {
                        ColorPalette pal = bmp.Palette;
                        for ( int i = 0; i < 256; i++ ) pal.Entries[ i ] = Color.FromArgb( i, i, i );
                        bmp.Palette = pal;

                        using ( FileStream ws = new FileStream( path, FileMode.Create ) ) {
                            if ( ws != null )
                                bmp.Save( ws, ImageFormat.Bmp );
                        }
                        bmp.Dispose();
                        bmp = null;
                    }
                } else {
                    UInt32 strideBy4 = Convert.ToUInt32( ( m_nWidth + 3 ) & ~3 );
                    Int64 allocSz = strideBy4 * m_nHeight;
                    IntPtr heapMem = AllocMem( allocSz );
                    if ( heapMem == IntPtr.Zero )
                        return false;

                    byte* pSrc8 = ( byte* ) handle.ToPointer();
                    byte* pDst8 = ( byte* ) heapMem.ToPointer();

                    UInt32 srcMv = m_nWidth;
                    UInt32 dstMv = strideBy4;
                    ulong cpsz = Convert.ToUInt64( m_nWidth );

                    for ( int y = 0; y < m_nHeight; y++ ) {
                        MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( cpsz ) );

                        pSrc8 += srcMv;
                        pDst8 += dstMv;
                    }

                    try {
                        Bitmap bmp = new Bitmap(
                                                 Convert.ToInt32( m_nWidth ),
                                                 Convert.ToInt32( m_nHeight ),
                                                 Convert.ToInt32( strideBy4 ),
                                                 PixelFormat.Format8bppIndexed, heapMem );
                        if ( bmp != null ) {
                            ColorPalette pal = bmp.Palette;
                            for ( int i = 0; i < 256; i++ ) pal.Entries[ i ] = Color.FromArgb( i, i, i );
                            bmp.Palette = pal;

                            using ( FileStream ws = new FileStream( path, FileMode.Create ) ) {
                                if ( ws != null )
                                    bmp.Save( ws, ImageFormat.Bmp );
                            }
                            bmp.Dispose();
                            bmp = null;
                        }
                    } finally {
                        FreeMem( heapMem );
                    }
                }
            } else if ( m_nPixBytes == 3 ) {
                UInt32 astride = m_nWidth * 3;

                if ( ( astride % 4 ) == 0 ) {
                    Bitmap bmp = new Bitmap(
                                             Convert.ToInt32( m_nWidth ),
                                             Convert.ToInt32( m_nHeight ),
                                             Convert.ToInt32( astride ),
                                             PixelFormat.Format24bppRgb, handle );
                    if ( bmp != null ) {
                        using ( FileStream ws = new FileStream( path, FileMode.Create ) ) {
                            if ( ws != null )
                                bmp.Save( ws, ImageFormat.Bmp );
                        }
                        bmp.Dispose();
                        bmp = null;
                    }
                } else {
                    UInt32 strideBy4 = Convert.ToUInt32( ( astride + 3 ) & ~3 );
                    Int64 allocSz = strideBy4 * m_nHeight;
                    IntPtr heapMem = AllocMem( allocSz );
                    if ( heapMem == IntPtr.Zero )
                        return false;

                    byte* pSrc8 = ( byte* ) handle.ToPointer();
                    byte* pDst8 = ( byte* ) heapMem.ToPointer();

                    UInt32 srcMv = Convert.ToUInt32( m_nWidth * m_nPixBytes );
                    UInt32 dstMv = strideBy4;
                    ulong cpsz = Convert.ToUInt64( srcMv );

                    for ( int y = 0; y < m_nHeight; y++ ) {
                        MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( cpsz ) );

                        pSrc8 += srcMv;
                        pDst8 += dstMv;
                    }

                    try {
                        Bitmap bmp = new Bitmap(
                                                 Convert.ToInt32( m_nWidth ),
                                                 Convert.ToInt32( m_nHeight ),
                                                 Convert.ToInt32( strideBy4 ),
                                                 PixelFormat.Format24bppRgb, heapMem );
                        if ( bmp != null ) {
                            using ( FileStream ws = new FileStream( path, FileMode.Create ) ) {
                                if ( ws != null )
                                    bmp.Save( ws, ImageFormat.Bmp );
                            }
                            bmp.Dispose();
                            bmp = null;
                        }
                    } finally {
                        FreeMem( heapMem );
                    }

                }
            } else
                return false;

            return true;
        }

        public bool LoadBmp( string path )
        {
            if ( String.IsNullOrEmpty( path ) || !File.Exists( path ) )
                return false;

            using ( FileStream rs = new FileStream( path, FileMode.Open ) ) {
                Bitmap bmp = new Bitmap( rs );
                if ( bmp != null && ( bmp.PixelFormat == PixelFormat.Format24bppRgb || bmp.PixelFormat == PixelFormat.Format8bppIndexed ) ) {
                    if ( Alloc(
                                Convert.ToUInt32( bmp.Width ),
                                Convert.ToUInt32( bmp.Height ),
                                Convert.ToUInt32( bmp.PixelFormat == PixelFormat.Format8bppIndexed ? 8 : 24 ) ) ) {
                        if ( bmp.PixelFormat == PixelFormat.Format24bppRgb ) {
                            BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, bmp.PixelFormat );
                            if ( bdata != null ) {
                                this.Copy( bdata.Scan0, Convert.ToUInt32( bdata.Stride ) );
                                bmp.UnlockBits( bdata );
                            }
                        } else {
                            Copy( bmp );
                        }
                    }
                }
                if ( bmp != null ) {
                    bmp.Dispose();
                    bmp = null;
                }
            }

            return true;
        }

        public UImageBuffer() : base( IntPtr.Zero, true )
        {
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;
            m_nBuffSize = 0;
        }

        public UImageBuffer( UInt32 w, UInt32 h, UInt32 f )
            : base( IntPtr.Zero, true )
        {
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;
            m_nBuffSize = 0;

            if ( w <= 0 || h <= 0 || ( f % 8 ) != 0 )
                return;

            Alloc( w, h, f );
        }

        public bool Alloc( UInt32 w, UInt32 h, UInt32 f )
        {
            if ( m_bDisposed ) {
                throw new InvalidOperationException( "The instance has been disposed" );
            }

            if ( w <= 0 || h <= 0 || f <= 0 || ( f % 8 ) != 0 )
                return false;

            // Check size.
            Int64 tmpSz = Convert.ToInt64( w ) * Convert.ToInt64( h ) * Convert.ToInt64( Convert.ToDouble( f ) / Convert.ToDouble( 8 ) );
            if ( tmpSz == m_nBuffSize && handle != IntPtr.Zero )
                return true;

            // Reset
            if ( handle != IntPtr.Zero ) {
                FreeMem( handle );
            }

            m_nBuffSize = 0;
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;

            // Alloc new one.
            handle = AllocMem( Convert.ToInt64( tmpSz ) );
            if ( handle == IntPtr.Zero )
                return false;

            m_nWidth = w;
            m_nHeight = h;
            m_nPixBits = f;
            m_nPixBytes = Convert.ToUInt32( Convert.ToDouble( f ) / Convert.ToDouble( 8 ) );
            m_nBuffSize = tmpSz;

            return true;
        }

        public void Free()
        {
            if ( m_bDisposed )
                return;

            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;

            if ( handle != IntPtr.Zero ) {
                FreeMem( handle );
                handle = IntPtr.Zero;
            }
        }

        // Alloc buffer before calling
        public void Copy( void* pSrc, UInt32 lineBytes )
        {
            if ( pSrc == null || IsInvalid )
                return;

            bool packed = ( ( m_nWidth * m_nPixBytes ) == lineBytes || lineBytes <= 0 );
            if ( packed ) {
                UInt32 tmpSz = Convert.ToUInt32( m_nWidth ) * Convert.ToUInt32( m_nHeight ) * Convert.ToUInt32( m_nPixBytes );
                if ( tmpSz > m_nBuffSize )
                    return;

                MemWinSdkFunctions.NativeMemcpy( handle.ToPointer(), pSrc, new UIntPtr( tmpSz ) );
                return;
            }

            // Copy from line-by-line.
            UInt32 tmpw = Convert.ToUInt32( m_nWidth * m_nPixBytes );
            if ( tmpw > lineBytes )
                return;

            byte* pSrc8 = ( byte* ) pSrc;
            byte* pDst8 = ( byte* ) handle.ToPointer();
            long srcMv = Convert.ToInt64( lineBytes );
            long dstMv = Convert.ToInt64( m_nWidth * m_nPixBytes );
            ulong cpLine = Convert.ToUInt64( m_nWidth ) * Convert.ToUInt64( m_nPixBytes );

            for ( int y = 0; y < m_nHeight; y++, pSrc8 += srcMv, pDst8 += dstMv )
                MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( cpLine ) );
        }

        public void Copy( IntPtr src, UInt32 lineBytes )
        {
            Copy( src.ToPointer(), lineBytes );
        }

        public void Copy( void* pSrc )
        {
            if ( pSrc == null || IsInvalid )
                return;

            UInt32 total = Convert.ToUInt32( m_nWidth ) * Convert.ToUInt32( m_nHeight ) * Convert.ToUInt32( m_nPixBytes );
            if ( total > m_nBuffSize )
                return;

            MemWinSdkFunctions.NativeMemcpy( handle.ToPointer(), pSrc, new UIntPtr( total ) );
        }

        public void Copy( IntPtr src )
        {
            Copy( src.ToPointer() );
        }

        // Auto-adjust buffer
        public bool Copy( IntPtr src, UInt32 w, UInt32 h, UInt32 lineBytes, UInt32 f )
        {
            if ( ( f % 8 ) != 0 || m_bDisposed || src == IntPtr.Zero )
                return false;

            if ( !Alloc( w, h, f ) )
                return false;

            // Change size
            m_nWidth = w;
            m_nHeight = h;
            m_nPixBits = f;
            m_nPixBytes = f / 8;

            if ( lineBytes > w )
                Copy( src.ToPointer(), lineBytes );
            else
                Copy( src.ToPointer() );

            return true;
        }

        public bool Copy( byte[] src, UInt32 w, UInt32 h, UInt32 f )
        {
            if ( ( f % 8 ) != 0 || m_bDisposed || src == null )
                return false;

            UInt32 sz = w * h * ( f / 8 );
            if ( src.Length < sz )
                return false;

            if ( !Alloc( w, h, f ) )
                return false;

            fixed ( byte* pSrc = src ) {
                Copy( ( void* ) pSrc );
            }

            return true;
        }

        public Bitmap ToBitmap()
        {
            if ( IsInvalid )
                return null;
            if ( m_nWidth <= 0 || m_nHeight <= 0 || ( m_nPixBits != 8 && m_nPixBits != 24 ) ||
                 handle.ToInt32() == 0 )
                return null;

            Bitmap bmp = new Bitmap(
                                     Convert.ToInt32( m_nWidth ),
                                     Convert.ToInt32( m_nHeight ),
                                     m_nPixBits == 8 ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb );
            if ( bmp == null )
                return null;

            // Copy
            if ( m_nPixBits == 8 ) {
                ColorPalette pal = bmp.Palette;
                for ( int i = 0; i < pal.Entries.Length; i++ )
                    pal.Entries[ i ] = Color.FromArgb( i, i, i );
                bmp.Palette = pal;

                BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed );

                byte* pDst8 = ( byte* ) bdata.Scan0.ToPointer();
                long dstMv = bdata.Stride;
                byte* pSrc8 = ( byte* ) handle.ToPointer();
                long srcMv = m_nWidth;
                UInt32 cpLen = Convert.ToUInt32( bmp.Width );

                if ( dstMv == bmp.Width && dstMv == srcMv ) {
                    UInt32 totalCp = Convert.ToUInt32( dstMv ) * Convert.ToUInt32( m_nHeight );
                    MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( totalCp ) );
                } else {
                    for ( int y = 0; y < bmp.Height; y++, pSrc8 += srcMv, pDst8 += dstMv )
                        MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( cpLen ) );
                }

                bmp.UnlockBits( bdata );
            } else {
                BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

                byte* pDst8 = ( byte* ) bdata.Scan0.ToPointer();
                long dstMv = bdata.Stride;
                byte* pSrc8 = ( byte* ) handle.ToPointer();
                long srcMv = m_nWidth * 3;
                UInt32 cpLen = Convert.ToUInt32( m_nWidth * 3 );

                if ( dstMv == ( bmp.Width * 3 ) && m_nWidth == bmp.Width ) {
                    UInt32 totalCp = Convert.ToUInt32( cpLen ) * Convert.ToUInt32( m_nHeight );
                    MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( totalCp ) );
                } else {
                    for ( int y = 0; y < bmp.Height; y++, pSrc8 += srcMv, pDst8 += dstMv )
                        MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( cpLen ) );
                }

                bmp.UnlockBits( bdata );
            }

            return bmp;
        }

        public void Copy( Bitmap bmp )
        {
            if ( bmp == null || ( bmp.PixelFormat != PixelFormat.Format24bppRgb && bmp.PixelFormat != PixelFormat.Format8bppIndexed ) )
                return;

            int format = bmp.PixelFormat == PixelFormat.Format8bppIndexed ? 8 : 24;
            if ( !Alloc( Convert.ToUInt32( bmp.Width ), Convert.ToUInt32( bmp.Height ), Convert.ToUInt32( format ) ) )
                return;

            // Lock bits
            BitmapData bdata = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, bmp.PixelFormat );
            if ( bdata == null )
                return;

            void* pSrc = bdata.Scan0.ToPointer();
            int lineBytes = bdata.Stride;

            // Copy from line-by-line.
            UInt32 tmpw = m_nWidth * m_nPixBytes;
            if ( tmpw > lineBytes ) {
                bmp.UnlockBits( bdata );
                return;
            }

            // prepare memory address information
            byte* pSrc8 = ( byte* ) pSrc;
            byte* pDst8 = ( byte* ) handle.ToPointer();
            long srcMv = Convert.ToInt64( lineBytes );
            long dstMv = Convert.ToInt64( m_nWidth * m_nPixBytes );
            ulong cpLine = Convert.ToUInt64( m_nWidth ) * Convert.ToUInt64( m_nPixBytes );
            // get color palette
            ColorPalette pal = bmp.PixelFormat == PixelFormat.Format8bppIndexed ? bmp.Palette : null;
            // Copy
            for ( int y = 0; y < m_nHeight; y++, pSrc8 += srcMv, pDst8 += dstMv ) {
                if ( pal != null ) {
                    byte* pALineDst = pDst8;
                    byte* pALineSrc = pSrc8;
                    for ( int x = 0; x < m_nWidth; x++ )
                        pALineDst[ x ] = pal.Entries[ pALineSrc[ x ] ].R; // get value from palette
                } else
                    MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrc8, new UIntPtr( cpLine ) );
            }
            // back color palette
            if ( pal != null ) bmp.Palette = pal;

            bmp.UnlockBits( bdata );
            return;
        }

        public byte[] ToByteArr( ref UInt32 w, ref UInt32 h, ref UInt32 f )
        {
            w = h = f = 0;
            if ( IsInvalid || m_bDisposed || m_nBuffSize == 0 )
                return null;

            byte[] retArr = new byte[ m_nBuffSize ];
            if ( retArr == null )
                return null;

            fixed ( byte* p8 = retArr ) {
                MemWinSdkFunctions.NativeMemcpy( ( void* ) p8, handle.ToPointer(), new UIntPtr( Convert.ToUInt64( m_nBuffSize ) ) );
            }

            w = m_nWidth;
            h = m_nHeight;
            f = m_nPixBits;

            return retArr;
        }

        public bool GetPixel(int x, int y, out int val1, out int val2, out int val3)
        {
            val1 = val2 = val3 = -1;
            if ( IsClosed || m_bDisposed ) return false;
            if ( x < 0 || y < 0 || handle == IntPtr.Zero ) return false;

            UInt32 ux = ( UInt32 ) x;
            UInt32 uy = ( UInt32 ) y;
            if ( ux >= m_nWidth || uy >= m_nHeight )
                return false;

            unsafe
            {
                byte* p8 = (byte*) handle.ToPointer();
                p8 += ( ( y * m_nWidth + x ) * m_nPixBytes );

                if (m_nPixBits == 8) {
                    val1 = ( int ) ( *p8 );
                    return true;
                } else if (m_nPixBits == 24) {
                    val1 = ( int ) ( p8[ 0 ] );
                    val2 = ( int ) ( p8[ 1 ] );
                    val3 = ( int ) ( p8[ 2 ] );
                    return true;
                }
            }
            return false;
        }

        public static bool Trim( UImageBuffer dst, UImageBuffer src, Rectangle roi, ref Rectangle retRoi )
        {
            if ( dst == null || src == null || dst.m_bDisposed || src.m_bDisposed )
                return false;
            if ( src.Width <= 0 || src.Height <= 0 || src.Bits <= 0 )
                return false;

            // range check
            UInt32 adjL = Convert.ToUInt32( roi.Left < 0 ? 0 : roi.Left );
            UInt32 adjT = Convert.ToUInt32( roi.Top < 0 ? 0 : roi.Top );
            if ( adjL >= src.Width || adjT >= src.Height )
                return false;

            UInt32 adjR = Convert.ToUInt32( roi.Left + roi.Width );
            UInt32 adjB = Convert.ToUInt32( roi.Top + roi.Height );
            adjR = adjR > src.Width ? src.Width : adjR;
            adjB = adjB > src.Height ? src.Height : adjB;

            long adjW = Convert.ToInt64( ( long ) adjR - ( long ) adjL );
            long adjH = Convert.ToInt64( ( long ) adjB - ( long ) adjT );
            if ( adjW <= 0 || adjH <= 0 )
                return false;

            // Config returned roi
            retRoi.X = Convert.ToInt32( adjL );
            retRoi.Y = Convert.ToInt32( adjT );
            retRoi.Width = Convert.ToInt32( adjW );
            retRoi.Height = Convert.ToInt32( adjH );

            // Re-config src
            if ( !dst.Alloc( Convert.ToUInt32( adjW ), Convert.ToUInt32( adjH ), Convert.ToUInt32( src.Bits ) ) )
                return false;

            // Pointer OP
            uint srcOffsetX = Convert.ToUInt32( adjL * ( src.Bits / 8 ) );
            uint srcPerlineMv = Convert.ToUInt32( src.Width * ( src.Bits / 8 ) );

            uint dstPerlineMv = Convert.ToUInt32( dst.Width * ( dst.Bits / 8 ) );
            ulong cpLen = Convert.ToUInt64( adjW ) * Convert.ToUInt64( dst.Bits / 8 );

            byte* pSrc8 = ( byte* ) src.Buffer.ToPointer();
            byte* pDst8 = ( byte* ) dst.Buffer.ToPointer();

            // config begin address
            pSrc8 += ( srcPerlineMv * adjT );
            for ( int y = 0; y < adjH; y++ ) {
                byte* pSrcCpAddr = pSrc8 + srcOffsetX;
                MemWinSdkFunctions.NativeMemcpy( ( void* ) pDst8, ( void* ) pSrcCpAddr, new UIntPtr( cpLen ) );

                // Move address
                pSrc8 += srcPerlineMv;
                pDst8 += dstPerlineMv;
            }

            return true;
        }


        protected virtual IntPtr AllocMem( long nSize )
        {
            return Marshal.AllocHGlobal( new IntPtr( nSize ) );
        }

        protected virtual void FreeMem( IntPtr addr )
        {
            Marshal.FreeHGlobal( addr );
        }
    }
}
