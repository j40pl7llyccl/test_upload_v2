using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

using uIP.Lib.MarshalWinSDK;
using System.Windows.Forms;

namespace uIP.Lib.DataCarrier
{
    public unsafe class UImageBuffer : SafeHandle
    {
        //private IntPtr m_UnmanageBuff;
        private int m_nWidth;
        private int m_nHeight;
        private int m_nPixBits;
        private int m_nStride;
        private Int64 m_nBuffSize;
        private int m_nPixBytes;
        private bool m_bDisposed = false;
        private readonly int m_nAlignment = 4;

        public override bool IsInvalid
        {
            get
            {
                if (m_bDisposed)
                    return true;
                if (handle == IntPtr.Zero)
                    return true;
                return false;
            }
        }

        protected override bool ReleaseHandle()
        {
            if (!m_bDisposed)
            {
                m_bDisposed = true;
                //GC.SuppressFinalize( this );
                if (handle != IntPtr.Zero)
                {
                    FreeMem(handle);
                    handle = IntPtr.Zero;
                }

                m_nWidth = 0;
                m_nHeight = 0;
                m_nPixBits = 0;
                m_nStride = 0;
                m_nBuffSize = 0;
                m_nPixBytes = 0;
            }

            return true;
        }

        public IntPtr Buffer
        {
            get
            {
                if (IsInvalid) return IntPtr.Zero;
                return handle;
            }
        }
        public int Width { get { return m_nWidth; } }
        public int Height { get { return m_nHeight; } }
        public int Bits { get { return m_nPixBits; } }
        public int Stride { get { return m_nStride; } }
        public Int64 Size { get { return m_nBuffSize; } }
        public int Alignment { get { return m_nAlignment; } }
        public bool IsAlignment { get { return m_nStride % m_nAlignment == 0; } }

        public UImageBuffer() : base(IntPtr.Zero, true)
        {
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;
            m_nStride = 0;
            m_nBuffSize = 0;
        }

        public UImageBuffer(int alignment) : base(IntPtr.Zero, true)
        {
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;
            m_nStride = 0;
            m_nBuffSize = 0;
            m_nAlignment = (alignment % 4) != 0 || alignment <= 0 ? 4 : alignment;
        }

        public UImageBuffer(int w, int h, int f, bool bPackedBuff = true)
            : base(IntPtr.Zero, true)
        {
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;
            m_nStride = 0;
            m_nBuffSize = 0;

            if (w <= 0 || h <= 0 || (f % 8) != 0)
                return;


            Alloc(w, h, f, bPackedBuff);
        }

        public bool Alloc(int w, int h, int f, bool bAllocPacked = true)
        {
            if (m_bDisposed)
            {
                throw new InvalidOperationException("The instance has been disposed");
            }

            if (w <= 0 || h <= 0 || f <= 0 || (f % 8) != 0)
                return false;

            int stride = w * (f / 8);
            if (!bAllocPacked)
            {
                stride = (stride % m_nAlignment) == 0 ? stride : stride + (m_nAlignment - (stride % m_nAlignment));
            }

            // calc alloc size
            Int64 tmpSz = stride * h;
            if (tmpSz == m_nBuffSize && handle != IntPtr.Zero)
            {
                m_nWidth = w;
                m_nHeight = h;
                m_nPixBits = f;
                m_nPixBytes = (f / 8);
                m_nStride = stride;
                return true;
            }

            // Reset
            if (handle != IntPtr.Zero)
            {
                FreeMem(handle);
            }

            m_nBuffSize = 0;
            handle = IntPtr.Zero;
            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nStride = 0;
            m_nPixBytes = 0;

            // Alloc new one.
            handle = AllocMem(Convert.ToInt64(tmpSz));
            if (handle == IntPtr.Zero)
                return false;

            m_nWidth = w;
            m_nHeight = h;
            m_nPixBits = f;
            m_nPixBytes = f / 8;
            m_nStride = stride;
            m_nBuffSize = tmpSz;

            return true;
        }

        public void Free()
        {
            if (m_bDisposed)
                return;

            m_nWidth = 0;
            m_nHeight = 0;
            m_nPixBits = 0;
            m_nPixBytes = 0;
            m_nStride = 0;

            if (handle != IntPtr.Zero)
            {
                FreeMem(handle);
                handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// use memcopy instead of byte copy
        /// </summary>
        /// <param name="dst">destination begin mem addr</param>
        /// <param name="nDstStride">destination column count</param>
        /// <param name="src">source begin mem addr</param>
        /// <param name="nSrcStride">source column count</param>
        /// <param name="h">number of rows to copy</param>
        /// <param name="nCpSz">number of bytes to copy in a row</param>
        /// <returns>call status</returns>
        private static bool LocCopy(IntPtr dst, int nDstStride, IntPtr src, int nSrcStride, int h, int nCpSz)
        {
            // invalid conditions
            if (dst == IntPtr.Zero || nDstStride == 0 || src == IntPtr.Zero || nSrcStride == 0 || h == 0 || nCpSz == 0)
                return false;
            // copy size check
            if (nDstStride < nCpSz || nSrcStride < nCpSz)
                return false;
            // whole mem copy
            if (nDstStride == nSrcStride)
                MemWinSdkFunctions.NativeMemcpy(dst, src, new UIntPtr((UInt64)nDstStride * (UInt64)h));
            else
            {
                byte* pSrc = (byte*)src.ToPointer();
                byte* pDst = (byte*)dst.ToPointer();
                UIntPtr sz = new UIntPtr((UInt32)nCpSz);
                for (int y = 0; y < h; y++, pSrc += nSrcStride, pDst += nDstStride)
                {
                    MemWinSdkFunctions.NativeMemcpy(pDst, pSrc, sz);
                }
            }
            return true;
        }

        /// <summary>
        /// copy from source to class and do alloc before copy
        /// </summary>
        /// <param name="src">source memeory addr</param>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="f">bits per pixel</param>
        /// <param name="stride">source colume count</param>
        /// <param name="bAllocPacked">class alloc in pack memory or not</param>
        /// <returns>call status</returns>
        public bool Copy(IntPtr src, int w, int h, int f, int stride, bool bAllocPacked = true)
        {
            if ((f % 8) != 0 || m_bDisposed || src == IntPtr.Zero)
                return false;

            if (!Alloc(w, h, f, bAllocPacked))
                return false;

            return LocCopy(handle, m_nStride, src, stride, h, w * (f / 8));
        }

        /// <summary>
        /// copy from managed byte array to class
        /// </summary>
        /// <param name="src">source byte array</param>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        /// <param name="f">source pixel bits</param>
        /// <param name="bAllocPacked">class alloc in pack memory or not</param>
        /// <returns>call status</returns>
        public bool Copy(byte[] src, int w, int h, int f, bool bAllocPacked = true)
        {
            if ((f % 8) != 0 || m_bDisposed || src == null)
                return false;

            UInt32 sz = (UInt32)w * (UInt32)h * (UInt32)(f / 8);
            if (src.Length < sz)
                return false;

            if (!Alloc(w, h, f, bAllocPacked))
                return false;

            var status = false;
            fixed (byte* pSrc = src)
            {
                status = LocCopy(handle, m_nStride, new IntPtr((void*)pSrc), w * (f / 8), h, w * (f / 8));
            }

            return status;
        }

        private static void ConfigPalette(Bitmap b)
        {
            if (b == null)
                return;
            ColorPalette pal = b.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = Color.FromArgb(i, i, i);
            b.Palette = pal;
        }

        public bool SaveBmp(string path, ImageFormat format)
        {
            if (IsInvalid || handle == IntPtr.Zero || m_nBuffSize == 0)
                return false;

            PixelFormat pixformat = PixelFormat.Undefined;
            if (m_nPixBytes == 1)
                pixformat = PixelFormat.Format8bppIndexed;
            else if (m_nPixBytes == 3)
                pixformat = PixelFormat.Format24bppRgb;
            else if (m_nPixBytes == 4)
                pixformat = PixelFormat.Format32bppRgb;
            else
                return false;

            try
            {
                Bitmap b = null;
                if (IsAlignment)
                {
                    // direct use memory
                    b = new Bitmap((int)m_nWidth, (int)m_nHeight, (int)m_nStride, pixformat, handle);
                }
                else
                {
                    // line by line process
                    b = new Bitmap((int)m_nWidth, (int)m_nHeight, pixformat);
                    var bdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, pixformat);
                    var ok = LocCopy(bdata.Scan0, bdata.Stride, handle, m_nStride, m_nHeight, m_nPixBytes * m_nWidth);
                    b.UnlockBits(bdata);
                    if (!ok)
                    {
                        b.Dispose();
                        b = null;
                    }
                }
                // bitmap ready?
                if (b == null)
                    return false;
                // config palette when 8-bit image
                if (pixformat == PixelFormat.Format8bppIndexed)
                    ConfigPalette(b);
                // save to file
                using (var wfs = File.Open(path, FileMode.Create))
                {
                    b.Save(wfs, format);
                }
                // release bitmap
                b.Dispose();
                return true;

            }
            catch { return false; }
        }

        public bool LoadBmp(string path, bool bAllocPacked = true)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            try
            {
                var status = false;
                using (FileStream rs = new FileStream(path, FileMode.Open))
                {
                    // alloc bitmap
                    Bitmap bmp = new Bitmap(rs);
                    status = FromBitmap(bmp);
                    // free bitmap
                    bmp.Dispose();
                }
                return status;
            }
            catch { return false; }
        }
        public Bitmap LoadBmp2Return(string path, out bool status, bool bAllocPacked = true)
        {
            status = false;
            if ( String.IsNullOrEmpty( path ) || !File.Exists( path ) )
                return null;

            Bitmap b = null;
            try
            {
                using ( FileStream rs = new FileStream( path, FileMode.Open ) )
                {
                    // alloc bitmap
                    b = new Bitmap( rs );
                    // copy data from bmp
                    status = FromBitmap( b );
                }
                if ( !status )
                {
                    b?.Dispose();
                    b = null;
                }
                return b;
            }
            catch { b?.Dispose(); return null; }

        }

        public Bitmap ToBitmap()
        {
            if (IsInvalid || handle == IntPtr.Zero || m_nBuffSize == 0)
                return null;

            PixelFormat pixformat = PixelFormat.Undefined;
            if (m_nPixBytes == 1)
                pixformat = PixelFormat.Format8bppIndexed;
            else if (m_nPixBytes == 3)
                pixformat = PixelFormat.Format24bppRgb;
            else if (m_nPixBytes == 4)
                pixformat = PixelFormat.Format32bppRgb;
            else
                return null;

            Bitmap b = null;
            try
            {
                // line by line process
                b = new Bitmap((int)m_nWidth, (int)m_nHeight, pixformat);
                var bdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, pixformat);
                var ok = LocCopy(bdata.Scan0, bdata.Stride, handle, m_nStride, m_nHeight, m_nPixBytes * m_nWidth);
                b.UnlockBits(bdata);
                if (!ok)
                {
                    b.Dispose();
                    b = null;
                }
                // config palette when 8-bit image
                if (pixformat == PixelFormat.Format8bppIndexed)
                    ConfigPalette(b);

                return b;

            }
            catch { b?.Dispose(); return null; }
        }

        public bool FromBitmap(Bitmap bmp, bool bAllocPacked = true)
        {
            if (m_bDisposed || bmp == null)
                return false;

            var status = false;
            int pixbits = 0;
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                pixbits = 8;
            else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                pixbits = 24;
            else if (bmp.PixelFormat == PixelFormat.Format32bppArgb ||
                     bmp.PixelFormat == PixelFormat.Format32bppPArgb ||
                     bmp.PixelFormat == PixelFormat.Format32bppRgb)
                pixbits = 32;
            // alloc and copy
            if (pixbits > 0 && Alloc(bmp.Width, bmp.Height, pixbits, bAllocPacked))
            {
                var bdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                status = LocCopy(handle, m_nStride, bdata.Scan0, bdata.Stride, m_nHeight, m_nWidth * m_nPixBytes);
                bmp.UnlockBits(bdata);
            }

            return status;
        }

        public byte[] ToByteArr(ref int w, ref int h, ref int f)
        {
            w = h = f = 0;
            if (IsInvalid || m_bDisposed || m_nBuffSize == 0)
                return null;

            byte[] retArr = new byte[m_nBuffSize];
            if (retArr == null)
                return null;

            fixed (byte* p8 = retArr)
            {
                if (!LocCopy(new IntPtr((void*)p8), m_nWidth * m_nPixBytes, handle, m_nStride, m_nHeight, m_nWidth * m_nPixBytes))
                    retArr = null;
            }

            w = retArr == null ? 0 : m_nWidth;
            h = retArr == null ? 0 : m_nHeight;
            f = retArr == null ? 0 : m_nPixBits;

            return retArr;
        }

        private IntPtr ToPosition(int x, int y)
        {
            if (IsInvalid || x < 0 || y < 0 || x >= m_nWidth || y >= m_nHeight)
                return IntPtr.Zero;

            // to y
            byte* pAddr = (byte*)handle.ToPointer();
            pAddr += ((Int64)m_nStride * (Int64)y);
            // to x
            pAddr += ((Int64)m_nPixBytes * (Int64)x);

            return new IntPtr((void*)pAddr);
        }

        /// <summary>
        /// get pixel in position
        /// </summary>
        /// <param name="x">coor X</param>
        /// <param name="y">coor Y</param>
        /// <returns>
        /// 8-bit: [0] value
        /// 24-bit:[0]=R/ [1]=G/ [2]=B
        /// 32-bit:[0]=A/ [1]=R/ [2]=G/ [3]=B
        /// </returns>
        public byte[] GetPosition(int x, int y)
        {
            var pos = ToPosition(x, y);
            if (pos == IntPtr.Zero)
                return null;

            // to position
            byte* pAddr = (byte*)pos.ToPointer();
            // alloc for ret
            var ret = new byte[m_nPixBytes];
            if (UDataCarrier.LittleEndian)
            {
                for (int i = ret.Length - 1, d = 0; i >= 0; i--, d++)
                    ret[d] = pAddr[i];
            }
            else
            {
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = pAddr[i];
            }
            return ret;
        }

        public static Bitmap ConvertBitmap(Bitmap b, PixelFormat format, bool bFree = true)
        {
            if (b == null)
                return null;
            if (b.PixelFormat == format)
                return b;
            int pixbits = 0;
            if (format == PixelFormat.Format8bppIndexed)
                pixbits = 8;
            else if (format == PixelFormat.Format24bppRgb)
                pixbits = 24;
            else if (format == PixelFormat.Format32bppArgb ||
                      format == PixelFormat.Format32bppPArgb ||
                      format == PixelFormat.Format32bppRgb)
                pixbits = 32;
            else
            {
                if (bFree)
                    b.Dispose();
                return null;
            }

            try
            {
                Bitmap r = new Bitmap(b.Width, b.Height, format);
                if (pixbits == 8)
                {
                    ConfigPalette(r);
                    var bdata = r.LockBits(new Rectangle(0, 0, r.Width, r.Height), ImageLockMode.ReadWrite, r.PixelFormat);
                    byte* pRow = (byte*)bdata.Scan0.ToPointer();
                    for (int y = 0; y < r.Height; y++, pRow += bdata.Stride)
                        for (int x = 0; x < r.Width; x++)
                        {
                            Color c = b.GetPixel(x, y);
                            float v = (float)c.R + (float)c.G + (float)c.B;
                            pRow[x] = Convert.ToByte(v / 3.0);
                        }
                    r.UnlockBits(bdata);
                }
                else
                {
                    using (var g = Graphics.FromImage(r))
                    {
                        g.DrawImage(b, 0, 0);
                    }
                }

                if (bFree)
                    b.Dispose();
                return r;
            }
            catch { if (bFree) b.Dispose(); return null; }
        }

        public static bool Trim(UImageBuffer dst, UImageBuffer src, Rectangle roi, ref Rectangle retRoi, bool bDstAlloPacked = true)
        {
            if (dst == null || src == null || dst.m_bDisposed || src.m_bDisposed)
                return false;
            if (src.Width <= 0 || src.Height <= 0 || src.Bits <= 0)
                return false;

            // range check
            int adjL = roi.Left < 0 ? 0 : roi.Left;
            int adjT = roi.Top < 0 ? 0 : roi.Top;
            if (adjL >= src.Width || adjT >= src.Height)
                return false;

            int adjR = roi.Left + roi.Width;
            int adjB = roi.Top + roi.Height;
            adjR = adjR > src.Width ? src.Width : adjR;
            adjB = adjB > src.Height ? src.Height : adjB;

            int adjW = adjR - adjL;
            int adjH = adjB - adjT;
            if (adjW <= 0 || adjH <= 0)
                return false;

            // Config returned roi
            retRoi.X = adjL;
            retRoi.Y = adjT;
            retRoi.Width = adjW;
            retRoi.Height = adjH;

            var srcLTAddr = src.ToPosition(retRoi.X, retRoi.Y);
            return dst.Copy(srcLTAddr, retRoi.Width, retRoi.Height, src.Bits, src.Stride, bDstAlloPacked);
        }

        public static Bitmap ToBitmap(IntPtr src, int w, int h, int f, int stride = 0)
        {
            if ( w <= 0 || h <= 0 || ( f % 8 ) != 0 || stride < 0 )
                return null;
            if ( stride != 0 && ( stride < ( w * ( f / 8 ) ) ) )
                return null;
            PixelFormat format = PixelFormat.Undefined;
            if ( f == 8 ) format = PixelFormat.Format8bppIndexed;
            else if ( f == 24 ) format = PixelFormat.Format24bppRgb;
            else if ( f == 32 ) format = PixelFormat.Format32bppArgb;
            else return null;

            stride = stride == 0 ? (w * (f / 8)) : stride;
            Bitmap ret = new Bitmap( w, h, format );
            if ( ret == null )
                return null;
            var bdata = ret.LockBits( new Rectangle( 0, 0, w, h ), ImageLockMode.ReadWrite, format );
            try
            {
                LocCopy( bdata.Scan0, bdata.Stride, src, stride, h, w * ( f / 8 ) );
            }
            finally { ret.UnlockBits( bdata ); }
            return ret;
        }
        public static Bitmap ToBitmap( byte[] src, int w, int h, int f, int stride = 0)
        {
            if ( src == null || (f % 8) != 0 || stride < 0 ) return null;
            if ( stride != 0 && ( stride < ( w * ( f / 8 ) ) ) ) return null;
            long chk = ( long )w * ( long )h * ( long )( f / 8 );
            if ( chk > src.LongLength ) return null;

            PixelFormat format = PixelFormat.Undefined;
            if ( f == 8 ) format = PixelFormat.Format8bppIndexed;
            else if ( f == 24 ) format = PixelFormat.Format24bppRgb;
            else if ( f == 32 ) format = PixelFormat.Format32bppArgb;
            else return null;

            Bitmap ret = new Bitmap( w, h, format );
            if ( ret == null )
                return null;

            fixed ( byte* p8 = src)
            {
                stride = stride == 0 ? ( w * ( f / 8 ) ) : stride;

                var bdata = ret.LockBits( new Rectangle( 0, 0, w, h ), ImageLockMode.ReadWrite, format );
                try
                {
                    LocCopy( bdata.Scan0, bdata.Stride, new IntPtr( p8 ), stride, h, w * ( f / 8 ) );
                }
                finally { ret.UnlockBits( bdata ); }
            }
            return ret;
        }

        protected virtual IntPtr AllocMem(long nSize)
        {
            return Marshal.AllocHGlobal(new IntPtr(nSize));
        }

        protected virtual void FreeMem(IntPtr addr)
        {
            Marshal.FreeHGlobal(addr);
        }
    }
}
