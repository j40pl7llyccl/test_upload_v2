using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace uIP.Lib.MarshalWinSDK
{
    public static class GdiWinSdkFunctions
    {
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern int SetROP2( IntPtr hdc, int fnDrawMode );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern IntPtr CreatePen( int fnPenStyle, int nWidth, Int32 crColor );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern int Rectangle( IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern int DeleteObject( IntPtr hObject );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern int SelectObject( IntPtr hdc, IntPtr hGdiObj );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern int MoveToEx( IntPtr hdc, int X, int Y, ref POINT lpPoint );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        public static extern int LineTo( IntPtr hdc, int nXEnd, int nYEnd );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "User32.dll" )]
        public static extern IntPtr GetDC( IntPtr hWnd );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "User32.dll" )]
        public static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );
        [DllImport( "User32.dll" )]
        public static extern bool ScreenToClient( IntPtr hWnd, ref Point lpPoint );

        public enum ROP2 : int
        {
            R2_BLACK = 1,        /*  0       */
            R2_NOTMERGEPEN = 2,  /* DPon     */
            R2_MASKNOTPEN = 3,   /* DPna     */
            R2_NOTCOPYPEN = 4,   /* PN       */
            R2_MASKPENNOT = 5,   /* PDna     */
            R2_NOT = 6,          /* Dn       */
            R2_XORPEN = 7,       /* DPx      */
            R2_NOTMASKPEN = 8,   /* DPan     */
            R2_MASKPEN = 9,      /* DPa      */
            R2_NOTXORPEN = 10,   /* DPxn     */
            R2_NOP = 11,         /* D        */
            R2_MERGENOTPEN = 12, /* DPno     */
            R2_COPYPEN = 13,     /* P        */
            R2_MERGEPENNOT = 14, /* PDno     */
            R2_MERGEPEN = 15,    /* DPo      */
            R2_WHITE = 16        /*  1       */
        }

        public enum PenStyle : int
        {
            PS_SOLID = 0,
            PS_DASH = 1,       /* -------  */
            PS_DOT = 2,        /* .......  */
            PS_DASHDOT = 3,    /* _._._._  */
            PS_DASHDOTDOT = 4, /* _.._.._  */
            PS_NULL = 5,
            PS_INSIDEFRAME = 6
        }

        /// <summary>
        /// Create color for GDI as COLORREF
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int32 ToRGB(byte r, byte g, byte b)
        {
            return InternalMethods.LittleEndian ? ( b << 16 | g << 8 | r ) : ( r << 16 | g << 8 | b );
        }

        public static Point GetPoint( IntPtr _xy )
        {
            uint xy = unchecked(IntPtr.Size == 8 ? ( uint )_xy.ToInt64() : ( uint )_xy.ToInt32());
            int x = unchecked(( short )xy);
            int y = unchecked(( short )( xy >> 16 ));
            return new Point( x, y );
        }
    }

}
