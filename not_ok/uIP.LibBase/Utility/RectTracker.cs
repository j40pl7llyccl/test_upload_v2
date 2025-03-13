using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Security;
using System.Runtime.InteropServices;


namespace uIP.LibBase.Utility
{
    public struct TRectangle
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public TRectangle( int l, int t, int r, int b )
        {
            left = l < r ? l : r;
            right = r > l ? r : l;
            top = t < b ? t : b;
            bottom = b > t ? b : t;
        }
        public int Point5W {
            get { return ( ( left + right ) / 2 ); }
        }
        public int Point5H {
            get { return ( ( top + bottom ) / 2 ); }
        }
        public int Width {
            get { return ( right - left < 0 ? left - right : right - left ); }
        }
        public int Height {
            get { return ( bottom - top < 0 ? top - bottom : bottom - top ); }
        }
        public int Cx {
            get { return ( ( left + right ) / 2 ); }
        }
        public int Cy {
            get { return ( ( top + bottom ) / 2 ); }
        }
    }

    public struct TPoint
    {
        public long x;
        public long y;
    }

    public enum eHit
    {
        None = 0,
        LeftTop,
        LeftBottom,
        RightBottom,
        RightTop,
        Left,
        Right,
        Top,
        Bottom,
        Inside,
    }

    public enum eRestrict
    {
        No = 0,
        ByParent,
        ByRect,
    }

    public class URectTracker : IDisposable
    {
        private bool m_bDisposed;
        private bool m_bDisposing;
        private Control m_ParentCtrl;
        private bool m_bClassReady;

        private TRectangle m_rectConf;
        private Rectangle m_rectBounding;

        // Runtime variables
        private bool m_bTracking;
        private eHit m_RunHit;
        private Point m_ptOnHitOriginal;
        private Point m_ptO;
        private int m_iRunW;
        private int m_iRunH;
        private int m_iRunOriX;
        private int m_iRunOriY;

        // Parameters
        private eRestrict m_ParamRestrict; // The rect can be out-of-range?
        private TRectangle m_rectParamRestrictRect;
        private bool m_bParamEnabled;          // Accept mouse event?
        private bool m_bParamVisible;          // Can be visible?
        private int m_iParamSenseSz;           // Sense size.
        private Color m_ParamSelColor;         // On selecting drawing color. Not used present.
        private Color m_ParamUnselColor;       // Display color.
        private Color m_ParamSenseCornerColor; // 4-corner sense area display color.
        private Color m_ParamSenseEdgeColor;   // 4-edge sense area display color.
        private bool m_bParamDrawing;          // After mouse moving, drawing directly.
        private bool m_bParamClearOnMvDraw;    // Clear whole before moving. Not used present.
        private bool m_bParamLockMv;           // Lock whole rect movement.
        private bool m_bParamLockXY;           // Cannot change width & height.
        private bool m_bParamLockX;            // Cannot change width.
        private bool m_bParamLockY;            // Cannot change height.
        private double m_dParamZoomX;          // Zoom X dir coefficient.
        private double m_dParamZoomY;          // Zoom Y dir coefficient.

        #region >>> Import GDI <<<

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern int SetROP2( IntPtr hdc, int fnDrawMode );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern IntPtr CreatePen( int fnPenStyle, int nWidth, Int32 crColor );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern int Rectangle( IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern int DeleteObject( IntPtr hObject );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern int SelectObject( IntPtr hdc, IntPtr hGdiObj );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern int MoveToEx( IntPtr hdc, int X, int Y, ref TPoint lpPoint );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Gdi32.dll" )]
        private static extern int LineTo( IntPtr hdc, int nXEnd, int nYEnd );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "User32.dll" )]
        private static extern IntPtr GetDC( IntPtr hWnd );
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "User32.dll" )]
        private static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );

        #endregion


        #region >>> Public Member Functions <<<

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ctrl"></param>
        public URectTracker( Control ctrl )
        {
            m_bDisposed = false;
            m_bDisposing = false;

            m_ParentCtrl = ctrl;
            m_bClassReady = false;

            m_ptOnHitOriginal = new Point();
            m_ptO = new Point();

            m_rectConf.left = 5;
            m_rectConf.top = 5;
            m_rectConf.right = 20;
            m_rectConf.bottom = 20;

            m_iParamSenseSz = 3;

            m_bTracking = false;
            m_RunHit = eHit.None;
            m_ptOnHitOriginal.X = 0;
            m_ptOnHitOriginal.Y = 0;

            m_rectBounding = new Rectangle( m_rectConf.left - m_iParamSenseSz - 1, m_rectConf.top - m_iParamSenseSz - 1, m_rectConf.Width + m_iParamSenseSz * 2 + 2, m_rectConf.Height + m_iParamSenseSz * 2 + 2 );

            // Initialize parameters.
            m_ParamRestrict = eRestrict.ByParent;
            m_rectParamRestrictRect = new TRectangle();
            m_rectParamRestrictRect.left = 0;
            m_rectParamRestrictRect.top = 0;
            m_rectParamRestrictRect.right = 150;
            m_rectParamRestrictRect.bottom = 150;
            m_bParamEnabled = true;
            m_bParamVisible = true;
            m_ParamSelColor = Color.FromArgb( 0, 255, 0 );
            m_ParamUnselColor = Color.FromArgb( 255, 0, 0 );
            m_ParamSenseCornerColor = Color.DeepPink;
            m_ParamSenseEdgeColor = Color.DarkOliveGreen;
            m_bParamDrawing = false;
            m_bParamClearOnMvDraw = false;
            m_bParamLockXY = false;
            m_bParamLockX = false;
            m_bParamLockY = false;
            m_dParamZoomX = 1;
            m_dParamZoomY = 1;


            m_iRunW = 0;
            m_iRunH = 0;
            m_iRunOriX = 0;
            m_iRunOriY = 0;

            if ( ctrl == null )
                return;

            m_bClassReady = true;

            ctrl.SuspendLayout();

            ctrl.MouseDown += new MouseEventHandler( this.HandleMouseDown );
            ctrl.MouseMove += new MouseEventHandler( this.HandleMouseMove );
            ctrl.MouseUp += new MouseEventHandler( this.HandleMouseUp );

            ctrl.Paint += new PaintEventHandler( this.HandlePaint );

            ctrl.ResumeLayout();
        }

        ~URectTracker()
        {
            Dispose( false );
            GC.SuppressFinalize( this );
        }

        public void Dispose()
        {
            Dispose( true );
        }

        public void Dispose( bool bDisposing )
        {
            if ( m_bDisposing )
                return;
            m_bClassReady = false;
            m_bDisposing = true;

            if ( !m_bDisposed && m_ParentCtrl != null ) {
                m_ParentCtrl.SuspendLayout();

                m_ParentCtrl.MouseDown -= new MouseEventHandler( this.HandleMouseDown );
                m_ParentCtrl.MouseMove -= new MouseEventHandler( this.HandleMouseMove );
                m_ParentCtrl.MouseUp -= new MouseEventHandler( this.HandleMouseUp );

                m_ParentCtrl.Paint -= new PaintEventHandler( this.HandlePaint );

                m_ParentCtrl.ResumeLayout();

            }
            m_bDisposed = true;
        }

        /// <summary>
        /// Test if hitting in specified region.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public eHit HitTest( Point pt )
        {
            int halfw = ( m_rectConf.left + m_rectConf.right ) / 2;
            int halfh = ( m_rectConf.top + m_rectConf.bottom ) / 2;

            if ( IsInRegion( new Point( m_rectConf.left, m_rectConf.top ), pt ) )
                return ( m_bParamLockXY ? eHit.None : eHit.LeftTop );

            else if ( IsInRegion( new Point( m_rectConf.left, m_rectConf.bottom ), pt ) )
                return ( m_bParamLockXY ? eHit.None : eHit.LeftBottom );

            else if ( IsInRegion( new Point( m_rectConf.right, m_rectConf.bottom ), pt ) )
                return ( m_bParamLockXY ? eHit.None : eHit.RightBottom );

            else if ( IsInRegion( new Point( m_rectConf.right, m_rectConf.top ), pt ) )
                return ( m_bParamLockXY ? eHit.None : eHit.RightTop );

            else if ( IsInRegion( new Point( halfw, m_rectConf.top ), pt ) )
                return ( m_bParamLockY ? eHit.None : eHit.Top );

            else if ( IsInRegion( new Point( halfw, m_rectConf.bottom ), pt ) )
                return ( m_bParamLockY ? eHit.None : eHit.Bottom );

            else if ( IsInRegion( new Point( m_rectConf.left, halfh ), pt ) )
                return ( m_bParamLockX ? eHit.None : eHit.Left );

            else if ( IsInRegion( new Point( m_rectConf.right, halfh ), pt ) )
                return ( m_bParamLockX ? eHit.None : eHit.Right );

            else if ( pt.X >= m_rectConf.left && pt.X < m_rectConf.right && pt.Y >= m_rectConf.top && pt.Y < m_rectConf.bottom )
                return ( m_bParamLockMv ? eHit.None : eHit.Inside );

            return eHit.None;
        }

        #endregion

        #region >>> Properties <<<

        /// <summary>
        /// Restrict tracking inside parent region.
        /// </summary>
        public eRestrict RestrictMode {
            get { return m_ParamRestrict; }
            set { m_ParamRestrict = value; }
        }

        /// <summary>
        /// Configure the restricted rectangle.
        /// </summary>
        public TRectangle RestrictRect {
            get { return m_rectParamRestrictRect; }
            set {
                m_rectParamRestrictRect.left = value.left;
                m_rectParamRestrictRect.top = value.top;
                m_rectParamRestrictRect.right = value.right;
                m_rectParamRestrictRect.bottom = value.bottom;
            }
        }

        /// <summary>
        /// Accept the mouse events.
        /// </summary>
        public bool Enabled {
            get { return m_bParamEnabled; }
            set {
                if ( value )
                    m_bParamVisible = true;
                m_bParamEnabled = value;
                if ( m_ParentCtrl != null )
                    m_ParentCtrl.Invalidate();
            }
        }

        /// <summary>
        /// Visible?
        /// </summary>
        public bool Visible {
            get { return m_bParamVisible; }
            set {
                if ( !value )
                    m_bParamEnabled = false;
                m_bParamVisible = value;
                if ( m_ParentCtrl != null )
                    m_ParentCtrl.Invalidate();
            }
        }

        /// <summary>
        /// Get/ Set the rect which will be calculated by zoom factor.
        /// </summary>
        public TRectangle Positions {
            get {
                TRectangle rtn = new TRectangle();

                rtn.left = ( int ) ( ( double ) m_rectConf.left / m_dParamZoomX );
                rtn.top = ( int ) ( ( double ) m_rectConf.top / m_dParamZoomY );
                rtn.right = ( int ) ( ( double ) m_rectConf.right / m_dParamZoomX );
                rtn.bottom = ( int ) ( ( double ) m_rectConf.bottom / m_dParamZoomY );

                return rtn;
            }
            set {
                m_rectConf.left = ( int ) ( ( double ) value.left * m_dParamZoomX );
                m_rectConf.top = ( int ) ( ( double ) value.top * m_dParamZoomY );
                m_rectConf.right = ( int ) ( ( double ) value.right * m_dParamZoomX );
                m_rectConf.bottom = ( int ) ( ( double ) value.bottom * m_dParamZoomY );
            }
        }

        /// <summary>
        /// Sense size.
        /// </summary>
        public int SenseSize {
            get { return m_iParamSenseSz; }
            set {
                if ( value < 3 )
                    return;
                m_iParamSenseSz = value;
            }
        }

        /// <summary>
        /// Drawing color on selecting.
        /// </summary>
        public Color SelectedRectColor {
            get { return m_ParamSelColor; }
            set { m_ParamSelColor = value; }
        }

        /// <summary>
        /// Drawing color on re-paint.
        /// </summary>
        public Color UnselectRectColor {
            get { return m_ParamUnselColor; }
            set { m_ParamUnselColor = value; }
        }

        /// <summary>
        /// A color of corner sense area.
        /// </summary>
        public Color CornerColor {
            get { return m_ParamSenseCornerColor; }
            set { m_ParamSenseCornerColor = value; }
        }

        /// <summary>
        /// A Color of edge sense area.
        /// </summary>
        public Color EdgeColor {
            get { return m_ParamSenseEdgeColor; }
            set { m_ParamSenseEdgeColor = value; }
        }

        /// <summary>
        /// Draw after mouse moving.
        /// </summary>
        public bool ImmDraw {
            get { return m_bParamDrawing; }
            set { m_bParamDrawing = value; }
        }

        /// <summary>
        /// Clear whole area before moving rectangle.
        /// </summary>
        public bool DrawWithClear {
            get { return m_bParamClearOnMvDraw; }
            set { m_bParamClearOnMvDraw = value; }
        }

        /// <summary>
        /// Lock to move the rect.
        /// </summary>
        public bool LockMv {
            get { return m_bParamLockMv; }
            set { m_bParamLockMv = value; }
        }

        /// <summary>
        /// Cannot configure size of XY.
        /// </summary>
        public bool LockXY {
            get { return m_bParamLockXY; }
            set { m_bParamLockXY = value; }
        }

        /// <summary>
        /// Lock width.
        /// </summary>
        public bool LockX {
            get { return m_bParamLockX; }
            set { m_bParamLockX = value; }
        }

        /// <summary>
        /// Lock height.
        /// </summary>
        public bool LockY {
            get { return m_bParamLockY; }
            set { m_bParamLockY = value; }
        }

        /// <summary>
        /// Zoom X factor.
        /// </summary>
        public double ZoomX {
            get { return m_dParamZoomX; }
            set {
                if ( value <= 0 )
                    return;
                m_dParamZoomX = value;
            }
        }

        /// <summary>
        /// Zoom Y factor.
        /// </summary>
        public double ZoomY {
            get { return m_dParamZoomY; }
            set {
                if ( value <= 0 )
                    return;
                m_dParamZoomY = value;
            }
        }

        #endregion

        #region >>> Locked Member Functions <<<

        private void HandlePaint( object sender, PaintEventArgs e )
        {
            if ( !m_bParamVisible )
                return;

            Graphics g = e.Graphics;

            if ( g == null )
                return;

            if ( m_bTracking )
                DrawSelected( true, e.Graphics );
            else
                DrawUnselected( true, e.Graphics );

        }

        private void HandleMouseDown( object sender, MouseEventArgs e )
        {
            if ( !m_bClassReady || !m_bParamEnabled )
                return;

            Point pt = new Point( e.X, e.Y );

            eHit ht = HitTest( pt );

            if ( ht == eHit.None )
                return;

            m_ParentCtrl.SuspendLayout();
            switch ( ht ) {
            case eHit.LeftTop:
                m_ptO.X = m_rectConf.right;
                m_ptO.Y = m_rectConf.bottom;
                m_ptOnHitOriginal.X = m_rectConf.left;
                m_ptOnHitOriginal.Y = m_rectConf.top;
                break;
            case eHit.LeftBottom:
                m_ptO.X = m_rectConf.right;
                m_ptO.Y = m_rectConf.top;
                m_ptOnHitOriginal.X = m_rectConf.left;
                m_ptOnHitOriginal.Y = m_rectConf.bottom;
                break;
            case eHit.RightBottom:
                m_ptO.X = m_rectConf.left;
                m_ptO.Y = m_rectConf.top;
                m_ptOnHitOriginal.X = m_rectConf.right;
                m_ptOnHitOriginal.Y = m_rectConf.bottom;
                break;
            case eHit.RightTop:
                m_ptO.X = m_rectConf.left;
                m_ptO.Y = m_rectConf.bottom;
                m_ptOnHitOriginal.X = m_rectConf.right;
                m_ptOnHitOriginal.Y = m_rectConf.top;
                break;
            case eHit.Left:
                m_ptO.X = m_rectConf.right;
                m_ptO.Y = ( m_rectConf.top + m_rectConf.bottom ) / 2;
                m_ptOnHitOriginal.X = m_rectConf.left;
                m_ptOnHitOriginal.Y = m_ptO.Y;
                m_iRunH = Math.Abs( m_rectConf.top - m_rectConf.bottom );
                m_iRunOriY = m_rectConf.top;
                break;
            case eHit.Right:
                m_ptO.X = m_rectConf.left;
                m_ptO.Y = ( m_rectConf.top + m_rectConf.bottom ) / 2;
                m_ptOnHitOriginal.X = m_rectConf.right;
                m_ptOnHitOriginal.Y = m_ptO.Y;
                m_iRunH = Math.Abs( m_rectConf.top - m_rectConf.bottom );
                m_iRunOriY = m_rectConf.top;
                break;
            case eHit.Top:
                m_ptO.X = ( m_rectConf.left + m_rectConf.right ) / 2;
                m_ptO.Y = m_rectConf.bottom;
                m_ptOnHitOriginal.X = m_ptO.X;
                m_ptOnHitOriginal.Y = m_rectConf.top;
                m_iRunW = Math.Abs( m_rectConf.left - m_rectConf.right );
                m_iRunOriX = m_rectConf.left;
                break;
            case eHit.Bottom:
                m_ptO.X = ( m_rectConf.left + m_rectConf.right ) / 2;
                m_ptO.Y = m_rectConf.top;
                m_ptOnHitOriginal.X = m_ptO.X;
                m_ptOnHitOriginal.Y = m_rectConf.bottom;
                m_iRunW = Math.Abs( m_rectConf.left - m_rectConf.right );
                m_iRunOriX = m_rectConf.left;
                break;
            case eHit.Inside:
                m_ptO.X = e.X;
                m_ptO.Y = e.Y;
                m_ptOnHitOriginal.X = e.X;
                m_ptOnHitOriginal.Y = e.Y;
                m_iRunW = Math.Abs( m_rectConf.right - m_rectConf.left );
                m_iRunH = Math.Abs( m_rectConf.bottom - m_rectConf.top );
                m_iRunOriX = m_rectConf.left;
                m_iRunOriY = m_rectConf.top;
                break;
            default:
                m_ParentCtrl.ResumeLayout();
                return;
            }
            m_RunHit = ht;
            m_bTracking = true;
            m_ParentCtrl.Invalidate( GetInvalidUnselectedRegion(), false );
        }

        private void HandleMouseMove( object sender, MouseEventArgs e )
        {
            if ( !m_bTracking )
                return;

            Point pt = new Point( e.X, e.Y );
            TPoint dummyPt = new TPoint();


            switch ( m_ParamRestrict ) {
            case eRestrict.ByParent:
                if ( e.X < 0 )
                    pt.X = 0;
                if ( e.X >= m_ParentCtrl.Width )
                    pt.X = m_ParentCtrl.Width;
                if ( e.Y < 0 )
                    pt.Y = 0;
                if ( e.Y >= m_ParentCtrl.Height )
                    pt.Y = m_ParentCtrl.Height;
                break;
            case eRestrict.ByRect:
                if ( e.X < m_rectParamRestrictRect.left )
                    pt.X = m_rectParamRestrictRect.left;
                if ( e.X >= m_rectParamRestrictRect.right )
                    pt.X = m_rectParamRestrictRect.right;
                if ( e.Y < m_rectParamRestrictRect.top )
                    pt.Y = m_rectParamRestrictRect.top;
                if ( e.Y >= m_rectParamRestrictRect.bottom )
                    pt.Y = m_rectParamRestrictRect.bottom;
                break;
            default:
                pt.X = e.X;
                pt.Y = e.Y;
                break;
            }

            if ( pt.X == m_ptOnHitOriginal.X && pt.Y == m_ptOnHitOriginal.Y )
                return;

            IntPtr dc = GetDC( m_ParentCtrl.Handle );

            IntPtr spen = CreatePen( 0, 1, m_ParamSelColor.ToArgb() );
            if ( spen != null ) {
                SelectObject( dc, spen );
                //SetROP2( dc, 2 ); // NOTMERGEPEN
                //SetROP2( dc, 10 ); // NOTXORPEN
                SetROP2( dc, 14 ); // MERGEPENNOT
            }
            Rectangle rect = GetSelectedPosition();
            MoveToEx( dc, rect.Left, rect.Top, ref dummyPt );
            LineTo( dc, rect.Left, rect.Bottom );
            LineTo( dc, rect.Right, rect.Bottom );
            LineTo( dc, rect.Right, rect.Top );
            LineTo( dc, rect.Left, rect.Top );

            int minx = 0, miny = 0, maxx = 0, maxy = 0;

            switch ( m_RunHit ) {
            case eHit.LeftBottom:
            case eHit.LeftTop:
            case eHit.RightBottom:
            case eHit.RightTop:
                m_ptOnHitOriginal.X = pt.X;
                m_ptOnHitOriginal.Y = pt.Y;

                minx = m_ptO.X;
                miny = m_ptO.Y;
                maxx = minx;
                maxy = miny;
                if ( minx > m_ptOnHitOriginal.X )
                    minx = m_ptOnHitOriginal.X;
                if ( miny > m_ptOnHitOriginal.Y )
                    miny = m_ptOnHitOriginal.Y;
                if ( maxx < m_ptOnHitOriginal.X )
                    maxx = m_ptOnHitOriginal.X;
                if ( maxy < m_ptOnHitOriginal.Y )
                    maxy = m_ptOnHitOriginal.Y;
                break;

            case eHit.Left:
            case eHit.Right:
                m_ptOnHitOriginal.X = pt.X;

                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                maxx = minx + Math.Abs( m_ptO.X - m_ptOnHitOriginal.X );
                miny = m_iRunOriY;
                maxy = miny + m_iRunH;
                break;

            case eHit.Top:
            case eHit.Bottom:
                m_ptOnHitOriginal.Y = pt.Y;

                minx = m_iRunOriX;
                maxx = minx + m_iRunW;
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                maxy = miny + Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y );
                break;

            case eHit.Inside:
                m_ptO.X = m_ptOnHitOriginal.X;
                m_ptO.Y = m_ptOnHitOriginal.Y;
                m_ptOnHitOriginal.X = pt.X;
                m_ptOnHitOriginal.Y = pt.Y;
                m_iRunOriX += ( m_ptOnHitOriginal.X - m_ptO.X );
                m_iRunOriY += ( m_ptOnHitOriginal.Y - m_ptO.Y );

                minx = m_iRunOriX;
                miny = m_iRunOriY;
                maxx = minx + m_iRunW;
                maxy = miny + m_iRunH;
                break;
            default:
                ReleaseDC( m_ParentCtrl.Handle, dc );
                DeleteObject( spen );
                return;
            }

            m_rectBounding.X = minx - m_iParamSenseSz - 1;
            m_rectBounding.Y = miny - m_iParamSenseSz - 1;
            m_rectBounding.Width = maxx - minx + m_iParamSenseSz * 2 + 2;
            m_rectBounding.Height = maxy - miny + m_iParamSenseSz * 2 + 2;

            rect = GetSelectedPosition();
            MoveToEx( dc, rect.Left, rect.Top, ref dummyPt );
            LineTo( dc, rect.Left, rect.Bottom );
            LineTo( dc, rect.Right, rect.Bottom );
            LineTo( dc, rect.Right, rect.Top );
            LineTo( dc, rect.Left, rect.Top );

            SetROP2( dc, 13 ); // COPYPEN
            ReleaseDC( m_ParentCtrl.Handle, dc );
            DeleteObject( spen );

        }

        private void HandleMouseUp( object sender, MouseEventArgs e )
        {
            if ( !m_bTracking )
                return;

            int minx = 0, miny = 0, maxx = 0, maxy = 0;

            switch ( m_RunHit ) {
            case eHit.LeftBottom:
            case eHit.LeftTop:
            case eHit.RightBottom:
            case eHit.RightTop:
                minx = m_ptO.X;
                miny = m_ptO.Y;
                maxx = minx;
                maxy = miny;
                if ( minx > m_ptOnHitOriginal.X )
                    minx = m_ptOnHitOriginal.X;
                if ( miny > m_ptOnHitOriginal.Y )
                    miny = m_ptOnHitOriginal.Y;
                if ( maxx < m_ptOnHitOriginal.X )
                    maxx = m_ptOnHitOriginal.X;
                if ( maxy < m_ptOnHitOriginal.Y )
                    maxy = m_ptOnHitOriginal.Y;
                break;
            case eHit.Left:
            case eHit.Right:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                maxx = minx + Math.Abs( m_ptO.X - m_ptOnHitOriginal.X );
                miny = m_iRunOriY;
                maxy = miny + m_iRunH;
                break;
            case eHit.Top:
            case eHit.Bottom:
                minx = m_iRunOriX;
                maxx = minx + m_iRunW;
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                maxy = miny + Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y );
                break;
            case eHit.Inside:
                minx = m_iRunOriX;
                miny = m_iRunOriY;
                maxx = minx + m_iRunW;
                maxy = miny + m_iRunH;
                break;
            default:
                m_RunHit = eHit.None;
                m_bTracking = false;
                m_ParentCtrl.ResumeLayout();
                return;
            }

            m_rectConf.left = minx;
            m_rectConf.top = miny;
            m_rectConf.right = maxx;
            m_rectConf.bottom = maxy;

            m_RunHit = eHit.None;
            m_bTracking = false;
            m_ParentCtrl.ResumeLayout();

            m_ParentCtrl.Invalidate();
        }

        private bool IsInRegion( Point pt, Point testPt )
        {
            TRectangle tsRgn = new TRectangle();

            tsRgn.left = pt.X - m_iParamSenseSz;
            tsRgn.top = pt.Y - m_iParamSenseSz;
            tsRgn.right = pt.X + m_iParamSenseSz;
            tsRgn.bottom = pt.Y + m_iParamSenseSz;

            if ( testPt.X >= tsRgn.left && testPt.X < tsRgn.right && testPt.Y >= tsRgn.top && testPt.Y < tsRgn.bottom )
                return true;
            return false;
        }

        private Region GetInvalidSelectedRegion()
        {
            if ( !m_bTracking )
                return ( new Region() );

            Rectangle rect = new Rectangle();
            int minx = 0, miny = 0;

            switch ( m_RunHit ) {
            case eHit.Inside:
                rect.X = m_iRunOriX - 1;
                rect.Y = m_iRunOriY - 1;
                rect.Width = m_iRunW + 2;
                rect.Height = m_iRunH + 2;
                break;
            case eHit.LeftBottom:
            case eHit.LeftTop:
            case eHit.RightBottom:
            case eHit.RightTop:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                rect.X = minx - 1;
                rect.Y = miny - 1;
                rect.Width = Math.Abs( m_ptO.X - m_ptOnHitOriginal.X ) + 2;
                rect.Height = Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y ) + 2;
                break;
            case eHit.Left:
            case eHit.Right:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                rect.X = minx - 1;
                rect.Y = m_iRunOriY - 1;
                rect.Width = Math.Abs( m_ptO.X - m_ptOnHitOriginal.X ) + 2;
                rect.Height = m_iRunH + 2;
                break;
            case eHit.Top:
            case eHit.Bottom:
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                rect.X = m_iRunOriX - 1;
                rect.Y = miny - 1;
                rect.Width = m_iRunW + 2;
                rect.Height = Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y ) + 2;
                break;
            default:
                return ( new Region() );
            }

            return ( new Region( rect ) );
        }

        private Region GetInvalidUnselectedRegion()
        {
            Rectangle rect = new Rectangle( m_rectConf.left - m_iParamSenseSz - 2,
                                            m_rectConf.top - m_iParamSenseSz - 2,
                                            m_rectConf.right - m_rectConf.left + m_iParamSenseSz * 2 + 4,
                                            m_rectConf.bottom - m_rectConf.top + m_iParamSenseSz * 2 + 4 );
            return ( new Region( rect ) );
        }

        private void DrawSelected( bool haveDraw, Graphics dr )
        {
            if ( !m_bParamVisible )
                return;

            Graphics g = null;
            if ( haveDraw )
                g = dr;
            else
                g = Graphics.FromHwnd( m_ParentCtrl.Handle );

            int minx = 0, miny = 0;

            if ( g == null )
                return;

            if ( m_bParamClearOnMvDraw )
                g.Clear( Color.Black );

            Pen selPen = new Pen( m_ParamSelColor );

            switch ( m_RunHit ) {
            case eHit.Inside:
                g.DrawRectangle( selPen, new Rectangle( m_iRunOriX, m_iRunOriY, m_iRunW, m_iRunH ) );
                break;
            case eHit.LeftBottom:
            case eHit.LeftTop:
            case eHit.RightBottom:
            case eHit.RightTop:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                g.DrawRectangle( selPen, new Rectangle( minx, miny, Math.Abs( m_ptO.X - m_ptOnHitOriginal.X ), Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y ) ) );
                break;
            case eHit.Left:
            case eHit.Right:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                g.DrawRectangle( selPen, new Rectangle( minx, m_iRunOriY, Math.Abs( m_ptO.X - m_ptOnHitOriginal.X ), m_iRunH ) );
                break;
            case eHit.Top:
            case eHit.Bottom:
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                g.DrawRectangle( selPen, new Rectangle( m_iRunOriX, miny, m_iRunW, Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y ) ) );
                break;
            }

            if ( selPen != null ) { selPen.Dispose(); selPen = null; }

            if ( !haveDraw )
                g.Dispose();
        }

        private Rectangle GetSelectedPosition()
        {
            Rectangle rect = new Rectangle();
            int minx = 0, miny = 0;

            switch ( m_RunHit ) {
            case eHit.Inside:
                rect.X = m_iRunOriX;
                rect.Y = m_iRunOriY;
                rect.Width = m_iRunW;
                rect.Height = m_iRunH;
                break;
            case eHit.LeftBottom:
            case eHit.LeftTop:
            case eHit.RightBottom:
            case eHit.RightTop:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                rect.X = minx;
                rect.Y = miny;
                rect.Width = Math.Abs( m_ptO.X - m_ptOnHitOriginal.X );
                rect.Height = Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y );
                break;
            case eHit.Left:
            case eHit.Right:
                minx = m_ptO.X < m_ptOnHitOriginal.X ? m_ptO.X : m_ptOnHitOriginal.X;
                rect.X = minx;
                rect.Y = m_iRunOriY;
                rect.Width = Math.Abs( m_ptO.X - m_ptOnHitOriginal.X );
                rect.Height = m_iRunH;
                break;
            case eHit.Top:
            case eHit.Bottom:
                miny = m_ptO.Y < m_ptOnHitOriginal.Y ? m_ptO.Y : m_ptOnHitOriginal.Y;
                rect.X = m_iRunOriX;
                rect.Y = miny;
                rect.Width = m_iRunW;
                rect.Height = Math.Abs( m_ptO.Y - m_ptOnHitOriginal.Y );
                break;
            }

            return rect;
        }

        private void DrawUnselected( bool haveDraw, Graphics dr )
        {
            if ( !m_bParamVisible )
                return;

            Graphics g = null;

            if ( haveDraw )
                g = dr;
            else
                g = Graphics.FromHwnd( m_ParentCtrl.Handle );

            if ( g == null )
                return;

            if ( m_bParamClearOnMvDraw )
                g.Clear( Color.Black );

            // Shap
            Pen unselPen = new Pen( m_ParamUnselColor );
            g.DrawRectangle( unselPen, new Rectangle( m_rectConf.left, m_rectConf.top, m_rectConf.right - m_rectConf.left, m_rectConf.bottom - m_rectConf.top ) );
            if ( unselPen != null ) { unselPen.Dispose(); unselPen = null; }
            // Corner Sense
            if ( !m_bParamLockXY ) {
                SolidBrush sb = new SolidBrush( m_ParamSenseCornerColor );
                g.FillRectangle( sb, new Rectangle( m_rectConf.left - m_iParamSenseSz, m_rectConf.top - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                g.FillRectangle( sb, new Rectangle( m_rectConf.left - m_iParamSenseSz, m_rectConf.bottom - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                g.FillRectangle( sb, new Rectangle( m_rectConf.right - m_iParamSenseSz, m_rectConf.bottom - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                g.FillRectangle( sb, new Rectangle( m_rectConf.right - m_iParamSenseSz, m_rectConf.top - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                if ( sb != null ) { sb.Dispose(); sb = null; }
            }
            // Edge Sense
            if ( !m_bParamLockY ) {
                SolidBrush sb = new SolidBrush( m_ParamSenseEdgeColor );
                g.FillRectangle( sb, new Rectangle( m_rectConf.Point5W - m_iParamSenseSz, m_rectConf.top - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                g.FillRectangle( sb, new Rectangle( m_rectConf.Point5W - m_iParamSenseSz, m_rectConf.bottom - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                if ( sb != null ) { sb.Dispose(); sb = null; }
            }
            if ( !m_bParamLockX ) {
                SolidBrush sb = new SolidBrush( m_ParamSenseEdgeColor );
                g.FillRectangle( sb, new Rectangle( m_rectConf.left - m_iParamSenseSz, m_rectConf.Point5H - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                g.FillRectangle( sb, new Rectangle( m_rectConf.right - m_iParamSenseSz, m_rectConf.Point5H - m_iParamSenseSz, m_iParamSenseSz * 2 + 1, m_iParamSenseSz * 2 + 1 ) );
                if ( sb != null ) { sb.Dispose(); sb = null; }
            }
            if ( !haveDraw )
                g.Dispose();
        }

        #endregion
    }
}
