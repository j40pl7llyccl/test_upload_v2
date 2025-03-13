using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    public enum GdiFillMode : int
    {
        ALTERNATE = 1,
        WINDING = 2,
    }

    public enum CombineRgnMode : int
    {
        RGN_AND = 1,
        RGN_OR = 2,
        RGN_XOR = 3,
        RGN_DIFF = 4,
        RGN_COPY = 5,
    }

    public enum RegionFlags : int
    {
        ERROR = 0,
        NULLREGION = 1,
        SIMPLEREGION = 2,
        COMPLEXREGION = 3,
    }

    public static class GdiRgnWinSdkFunctions
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool DeleteObject( IntPtr ho );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern IntPtr CreatePolygonRgn( IntPtr pptl /* POINT* */, int cPoint, int iMode /* GdiFillMode */ );

        public static IntPtr CreatePolygonRgn(POINT[] pts, GdiFillMode mode)
        {
            if ( pts == null || pts.Length <= 0 )
                return IntPtr.Zero;

            IntPtr ret = IntPtr.Zero;
            unsafe
            {
                int szPOINT = Marshal.SizeOf( typeof( POINT ) );
                IntPtr buf = Marshal.AllocCoTaskMem( szPOINT * pts.Length );

                POINT* pPts = ( POINT* ) buf.ToPointer();
                for(int i = 0; i < pts.Length; i++ ) {
                    pPts[ i ].x = pts[ i ].x;
                    pPts[ i ].y = pts[ i ].y;
                }

                ret = CreatePolygonRgn( buf, pts.Length, ( int ) mode );

                Marshal.FreeCoTaskMem( buf );
            }

            return ret;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern IntPtr CreateRectRgn( int x1, int y1, int x2, int y2 );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern IntPtr CreateEllipticRgn( int x1, int y1, int x2, int y2 );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern IntPtr CreateRoundRectRg( int x1, int y1, int x2, int y2, int w, int h );

        /// <summary>
        /// The CombineRgn function combines two regions and stores the result in a third region. The two regions are combined according to the specified mode.
        /// </summary>
        /// <param name="hrgnDst">A handle to a new region with dimensions defined by combining two other regions. (This region must exist before CombineRgn is called.)</param>
        /// <param name="hrgnSrc1">A handle to the first of two regions to be combined.</param>
        /// <param name="hrgnSrc2">A handle to the second of two regions to be combined.</param>
        /// <param name="iMode">
        /// CombineRgnMode ->
        /// RGN_AND : Creates the intersection of the two combined regions.
        /// RGN_COPY : Creates a copy of the region identified by hrgnSrc1.
        /// RGN_DIFF : Combines the parts of hrgnSrc1 that are not part of hrgnSrc2.
        /// RGN_OR : Creates the union of two combined regions.
        /// RGN_XOR : Creates the union of two combined regions except for any overlapping areas.
        /// </param>
        /// <returns>
        /// RegionFlags ->
        /// NULLREGION : The region is empty.
        /// SIMPLEREGION : The region is a single rectangle.
        /// COMPLEXREGION : The region is more than a single rectangle.
        /// ERROR : No region is created.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern int CombineRgn( IntPtr hrgnDst, IntPtr hrgnSrc1, IntPtr hrgnSrc2, int iMode );

        /// <summary>
        /// The EqualRgn function checks the two specified regions to determine whether they are identical. The function considers two regions identical if they are equal in size and shape.
        /// </summary>
        /// <param name="hrgn1">Handle to a region.</param>
        /// <param name="hrgn2">Handle to a region.</param>
        /// <returns>
        /// If the two regions are equal, the return value is nonzero.
        /// If the two regions are not equal, the return value is zero.A return value of ERROR means at least one of the region handles is invalid.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool EqualRgn( IntPtr hrgn1, IntPtr hrgn2 );

        /// <summary>
        /// The FillRgn function fills a region by using the specified brush.
        /// </summary>
        /// <param name="hdc">Handle to the device context.</param>
        /// <param name="hrgn">Handle to the region to be filled. The region's coordinates are presumed to be in logical units.</param>
        /// <param name="hbr">Handle to the brush to be used to fill the region.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool FillRgn( IntPtr hdc, IntPtr hrgn, IntPtr hbr );

        /// <summary>
        /// The FrameRgn function draws a border around the specified region by using the specified brush.
        /// </summary>
        /// <param name="hdc">Handle to the device context.</param>
        /// <param name="hrgn">Handle to the region to be enclosed in a border. The region's coordinates are presumed to be in logical units.</param>
        /// <param name="hbr">Handle to the brush to be used to draw the border.</param>
        /// <param name="w">Specifies the width, in logical units, of vertical brush strokes.</param>
        /// <param name="h">Specifies the height, in logical units, of horizontal brush strokes.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool FrameRgn( IntPtr hdc, IntPtr hrgn, IntPtr hbr, int w, int h );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern int GetPolyFillMode( IntPtr hdc );

        /// <summary>
        /// The GetRgnBox function retrieves the bounding rectangle of the specified region.
        /// </summary>
        /// <param name="hrgn">A handle to the region.</param>
        /// <param name="lprc">A pointer to a RECT structure that receives the bounding rectangle in logical units.</param>
        /// <returns>
        /// NULLREGION : Region is empty.
        /// SIMPLEREGION : Region is a single rectangle.
        /// COMPLEXREGION : Region is more than a single rectangle.
        /// If the hrgn parameter does not identify a valid region, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern int GetRgnBox( IntPtr hrgn, IntPtr lprc /* RECT* */);

        public static RegionFlags GetRgnBox(IntPtr hrgn, out RECT rect)
        {
            rect = new RECT();

            RegionFlags ret = RegionFlags.ERROR;

            unsafe
            {
                int szRECT = Marshal.SizeOf( typeof( RECT ) );
                IntPtr buf = Marshal.AllocCoTaskMem( szRECT );
                RECT* pRect = ( RECT* ) buf.ToPointer();

                int r = GetRgnBox( hrgn, buf );

                if ( r != ( int ) RegionFlags.ERROR ) {
                    rect.left = pRect->left;
                    rect.top = pRect->top;
                    rect.right = pRect->right;
                    rect.bottom = pRect->bottom;
                }

                Marshal.FreeCoTaskMem( buf );

                if (Enum.IsDefined(typeof(RegionFlags), r)) {
                    ret = ( RegionFlags ) Enum.Parse( typeof( RegionFlags ), Enum.GetName( typeof( RegionFlags ), r ) );
                }

            }
            return ret;
        }

        /// <summary>
        /// The InvertRgn function inverts the colors in the specified region.
        /// </summary>
        /// <param name="hdc">Handle to the device context.</param>
        /// <param name="hrgn">Handle to the region for which colors are inverted. The region's coordinates are presumed to be logical coordinates.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// </returns>
        [ SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool InvertRgn( IntPtr hdc, IntPtr hrgn );

        /// <summary>
        /// The OffsetRgn function moves a region by the specified offsets.
        /// </summary>
        /// <param name="hrgn">Handle to the region to be moved.</param>
        /// <param name="x">Specifies the number of logical units to move left or right.</param>
        /// <param name="y">Specifies the number of logical units to move up or down.</param>
        /// <returns>
        /// NULLREGION : Region is empty.
        /// SIMPLEREGION : Region is a single rectangle.
        /// COMPLEXREGION : Region is more than a single rectangle.
        /// ERROR : An error occurred; region is unaffected.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern int OffsetRgn( IntPtr hrgn, int x, int y );

        /// <summary>
        /// The PaintRgn function paints the specified region by using the brush currently selected into the device context.
        /// </summary>
        /// <param name="hdc">Handle to the device context.</param>
        /// <param name="hrgn">Handle to the region to be filled. The region's coordinates are presumed to be logical coordinates.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool PaintRgn( IntPtr hdc, IntPtr hrgn );

        /// <summary>
        /// The PtInRegion function determines whether the specified point is inside the specified region.
        /// </summary>
        /// <param name="hrgn">Handle to the region to be examined.</param>
        /// <param name="x">Specifies the x-coordinate of the point in logical units.</param>
        /// <param name="y">Specifies the y-coordinate of the point in logical units.</param>
        /// <returns>
        /// If the specified point is in the region, the return value is nonzero.
        /// If the specified point is not in the region, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool PtInRegion( IntPtr hrgn, int x, int y );

        /// <summary>
        /// The RectInRegion function determines whether any part of the specified rectangle is within the boundaries of a region.
        /// </summary>
        /// <param name="hrgn">Handle to the region.</param>
        /// <param name="lprect">Pointer to a RECT structure containing the coordinates of the rectangle in logical units. The lower and right edges of the rectangle are not included.</param>
        /// <returns>
        /// If any part of the specified rectangle lies within the boundaries of the region, the return value is nonzero.
        /// If no part of the specified rectangle lies within the boundaries of the region, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool RectInRegion( IntPtr hrgn, IntPtr lprect );

        public static bool RectInRegion(IntPtr hrgn, RECT rect)
        {
            bool ret = false;

            unsafe
            {
                int szRECT = Marshal.SizeOf( typeof( RECT ) );
                IntPtr buf = Marshal.AllocCoTaskMem( szRECT );
                RECT* pRect = ( RECT* ) buf.ToPointer();

                pRect->left = rect.left;
                pRect->top = rect.top;
                pRect->right = rect.right;
                pRect->bottom = rect.bottom;

                ret = RectInRegion( hrgn, buf );

                Marshal.FreeCoTaskMem( buf );
            }

            return ret;
        }

        /// <summary>
        /// The SetPolyFillMode function sets the polygon fill mode for functions that fill polygons.
        /// </summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <param name="mode">
        /// GdiFillMode
        /// ALTERNATE : Selects alternate mode (fills the area between odd-numbered and even-numbered polygon sides on each scan line).
        /// WINDING : Selects winding mode (fills any region with a nonzero winding value).
        /// </param>
        /// <returns>The return value specifies the previous filling mode. If an error occurs, the return value is zero.</returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        public static extern int SetPolyFillMode( IntPtr hdc, int mode );

        /// <summary>
        /// The SetRectRgn function converts a region into a rectangular region with the specified coordinates.
        /// </summary>
        /// <param name="hrgn">Handle to the region.</param>
        /// <param name="left">Specifies the x-coordinate of the upper-left corner of the rectangular region in logical units.</param>
        /// <param name="top">Specifies the y-coordinate of the upper-left corner of the rectangular region in logical units.</param>
        /// <param name="right">Specifies the x-coordinate of the lower-right corner of the rectangular region in logical units.</param>
        /// <param name="bottom">Specifies the y-coordinate of the lower-right corner of the rectangular region in logical units.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// </returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "Gdi32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetRectRgn( IntPtr hrgn, int left, int top, int right, int bottom );
    }
}
