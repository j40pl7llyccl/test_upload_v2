using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public partial class UsrCtrlRegionEditor : UserControl
    {
        static readonly double IntersectionThreshold = 80;

        internal enum RegionType : int
        {
            NA = 0,
            Working,
            Ignore
        }
        internal enum GUIShapeType : int
        {
            Rectangle,
            Polygon
        }

        internal class RestoreDraw
        {
            internal IntPtr hWnd;
            internal Color DrawColor;
            internal int Width;
            internal Point[] Points;
        }

        double[] ZoomFactors { get; set; }
        double CurrentZoom { get; set; }

        URectTracker UiTracker { get; set; }
        public Bitmap BackgroundBmp { get; private set; }

        private UDataCarrier m_OnConfWorkingRegion = null;
        public Dictionary<UDataCarrier, List<UDataCarrier>> Regions { get; set; } = new Dictionary<UDataCarrier, List<UDataCarrier>>();
        public UDataCarrier NewWorkingRegion
        {
            set
            {
                if ( value == null ) return;
                if ( Regions.ContainsKey( value ) )
                    return;
                Regions.Add( value, new List<UDataCarrier>() );
            }
        }
        public UDataCarrier OnConfWorkingRegion
        {
            get => m_OnConfWorkingRegion;
            set
            {
                if ( value == null ) return;

                // not exist, create one
                if ( !Regions.ContainsKey(value) )
                    Regions.Add(value, new List<UDataCarrier>());

                m_OnConfWorkingRegion = value;
            }
        }
        public UDataCarrier DeleteWorkingRegion
        {
            set
            {
                if ( value == null ) return;
                if ( Regions.ContainsKey( value ) )
                    Regions.Remove( value );
                if ( m_OnConfWorkingRegion == value )
                    m_OnConfWorkingRegion = null;
            }
        }
        public List<UDataCarrier> WorkingRegions => Regions == null ? null : ( from kv in Regions select kv.Key ).ToList();
        public List<UDataCarrier> IgnoreRegions => OnConfWorkingRegion == null || Regions == null || !Regions.ContainsKey(OnConfWorkingRegion) ? null : Regions[OnConfWorkingRegion];

        int LineWidth { get; set; } = 3;

        ComboBox OpCB { get; set; } = null;

        List<Point> OnEditPolygon { get; set; } = new List<Point>();
        bool IsOnEditPolygon
        {
            get
            {
                return radioButton_polygon.Enabled && radioButton_polygon.Checked;
            }
        }
        bool OnEditPolygonClosed { get; set; } = false;
        Color OnEditPolygonColor { get; set; } = Color.Cyan;

        private RestoreDraw PrevRecord { get; set; } = null;

        void InitComponentsData()
        {
            // config zoom
            ZoomFactors = new double[]
            {
                0.1, 0.2, 0.3, 0.4, 0.5, 1, 1.5, 2, 2.5, 3
            };
            trackBar_zoom.Minimum = 0;
            trackBar_zoom.Maximum = ZoomFactors.Length - 1;
            trackBar_zoom.Value = 5;
            CurrentZoom = ZoomFactors[ trackBar_zoom.Value ];

            // config readio
            radioButton_workArea.Tag = RegionType.Working;
            radioButton_ignoreRegion.Tag = RegionType.Ignore;
            radioButton_rectangle.Tag = GUIShapeType.Rectangle;
            radioButton_polygon.Tag = GUIShapeType.Polygon;

            UiTracker = new URectTracker( panel_editor ) { Visible = false };
            UiTracker.Positions = new TRectangle( 0, 0, 50, 50 );

            // config combobox
            comboBox_ignoreRegion.Tag = RegionType.Ignore;
            comboBox_workingRegion.Tag = RegionType.Working;
            comboBox_ignoreRegion.Enter += new EventHandler( ( s, e ) => OpCB = s as ComboBox );
            comboBox_workingRegion.Enter += new EventHandler( ( s, e ) => OpCB = s as ComboBox );
        }

        public UsrCtrlRegionEditor()
        {
            InitializeComponent();

            InitComponentsData();
        }

        public UsrCtrlRegionEditor( Dictionary<UDataCarrier, List<UDataCarrier>> regions, string backgroundPath )
        {
            InitializeComponent();

            Regions = regions == null ? Regions : regions;

            InitComponentsData();

            // update background
            if (File.Exists(backgroundPath))
            {
                Bitmap b = null;
                try
                {
                    using ( var fs = File.OpenRead( backgroundPath ) )
                    {
                        b = new Bitmap( fs );
                    }
                    BackgroundBmp = b;
                    ImageInfoChange( b, CurrentZoom );
                }
                catch { b?.Dispose(); b = null; }
            }
            // update regions
            // update work regions
            if ( Regions.Count > 0)
                ResetComboBoxList( comboBox_workingRegion, ( from kv in Regions select kv.Key ).ToArray().Length );
        }
        public UsrCtrlRegionEditor( Dictionary<UDataCarrier, List<UDataCarrier>> regions, Bitmap bmp )
        {
            InitializeComponent();

            Regions = regions == null ? Regions : regions;

            InitComponentsData();

            // update background
            BackgroundBmp = bmp;
            ImageInfoChange( bmp, CurrentZoom );

            // update regions
            // update work regions
            if ( Regions.Count > 0 )
                ResetComboBoxList( comboBox_workingRegion, ( from kv in Regions select kv.Key ).ToArray().Length );

        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            BackgroundBmp?.Dispose();
            BackgroundBmp = null;

            UiTracker?.Dispose();
            UiTracker = null;

            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        int ToZoom(int v)
        {
            return (int)(v * ( CurrentZoom <= 0 ? 1.0 : CurrentZoom ));
        }
        int RevZoom(int v)
        {
            return ( int )( v / ( CurrentZoom <= 0 ? 1.0 : CurrentZoom ) );
        }
        Rectangle ToZoom(int l, int t, int r, int b)
        {
            int x = l;
            int y = t;
            int w = r - l + 1;
            int h = b - t + 1;
            return ToZoom( new Rectangle( x, y, w, h ) );
        }
        Rectangle RevZoom(int l, int t, int r, int b)
        {
            int x = l;
            int y = t;
            int w = r - l + 1;
            int h = b - t + 1;
            return RevZoom( new Rectangle( x, y, w, h ) );
        }
        Rectangle ToZoom(Rectangle r)
        {
            return new Rectangle(ToZoom(r.X), ToZoom(r.Y), ToZoom(r.Width), ToZoom(r.Height));
        }
        Rectangle RevZoom(Rectangle r)
        {
            return new Rectangle( RevZoom( r.X ), RevZoom( r.Y ), RevZoom( r.Width ), RevZoom( r.Height ) );
        }
        Point[] ToZoom( Point[] pts)
        {
            if ( pts == null || pts.Length == 0 ) return new Point[ 0 ];
            return ( from p in pts select new Point( ToZoom( p.X ), ToZoom( p.Y ) ) ).ToArray();
        }
        Point[] RevZoom( Point[] pts)
        {
            if (pts == null || pts.Length == 0) return new Point[ 0 ];
            return ( from p in pts select new Point( RevZoom( p.X ), RevZoom( p.Y ) ) ).ToArray();
        }

        static bool CheckIntersection(int refW, int refH, UDataCarrier refRegion, UDataCarrier toChk, out bool bIntersection)
        {
            bIntersection = false;
            if ( refW <= 0 || refH <= 0 || refRegion == null || toChk == null ) return false;

            
            var er = new EvalIgnoreRegion(refW, refH);
            if ( !er.ConfWorkRegion( refRegion ) ) goto CheckIntersection_Error;
            if ( !er.ConfIgnoreRegion( toChk ) ) goto CheckIntersection_Error;
            if ( !er.Eval( false ) ) goto CheckIntersection_Error;

            bIntersection = er.IntersectionArea > 0;
            er.Dispose();
            return true;

CheckIntersection_Error:
            er.Dispose();
            return false;
        }

        private void ImageInfoChange( Bitmap b, double zoom, bool invalidate = true )
        {
            CurrentZoom = zoom;
            if ( b != null )
            {
                int w = ( int )( b.Width * zoom );
                int h = ( int )( b.Height * zoom );
                panel_editor.Width = w;
                panel_editor.Height = h;
            }
            if (invalidate)
                panel_editor.Invalidate();
        }

        private void ResetOnEditPolygon()
        {
            OnEditPolygon.Clear();
            OnEditPolygonClosed = false;
        }
        private void ResetComboBoxList(ComboBox cb, int count)
        {
            if ( cb == null ) return;
            cb.Text = "";
            cb.Items.Clear();
            for ( int i = 0; i < count; i++ ) cb.Items.Add( $"{i}" );
        }

        // click kind of region type
        private void radioButton_workArea_Click( object sender, EventArgs e )
        {
            groupBox_switchFunc.Enabled = true;
            RadioButton btn = sender as RadioButton;
            if ( btn == null )
                return;

            if (btn.Tag is RegionType rt)
            {
                radioButton_rectangle.Enabled = true;
                radioButton_polygon.Enabled = rt == RegionType.Ignore;
            }
            else
            {
                radioButton_rectangle.Enabled = false;
                radioButton_polygon.Enabled = false;
            }

            if ( UiTracker != null )
            {
                UiTracker.Visible = radioButton_rectangle.Checked;
                panel_editor.Invalidate();
            }
        }

        private static void DrawShape( Graphics g, Pen p, Rectangle r, double zoom )
        {
            if ( zoom != 1.0 )
                r = new Rectangle( ( int )( r.X * zoom ), ( int )( r.Y * zoom ), ( int )( r.Width * zoom ), ( int )( r.Height * zoom ) );
            g.DrawRectangle( p, r );
        }
        private static void DrawShape( Graphics g, Pen p, Point[] pts, double zoom )
        {
            if ( pts == null || pts.Length < 3 )
                return;
            if ( zoom != 1.0 )
                pts = ( from pt in pts select new Point( ( int )( pt.X * zoom ), ( int )( pt.Y * zoom ) ) ).ToArray();
            g.DrawPolygon( p, pts );
        }
        private static void DrawShape(Graphics g, Pen p, double zoom, UDataCarrier c)
        {
            if ( c == null ) return;
            if ( c.Data is Rectangle r ) DrawShape( g, p, r, zoom );
            else if ( c.Data is Point[] pts ) DrawShape( g, p, pts, zoom );
        }

        // edit area repaint ( reflect invalidate call)
        private void panel_editor_Paint( object sender, PaintEventArgs e )
        {
            if ( BackgroundBmp == null )
                return;
            // draw background
            var rect = new Rectangle( 0, 0, ( int )( BackgroundBmp.Width * CurrentZoom ), ( int )( BackgroundBmp.Height * CurrentZoom ) );
            e.Graphics.DrawImage( BackgroundBmp, rect );
            // draw regions
            Pen wpActive = new Pen( label_createdWorkRgn.BackColor, LineWidth );
            Pen wpInActi = new Pen( label_createdWorkRgn.BackColor, LineWidth );
            wpInActi.DashStyle = DashStyle.Custom;
            wpInActi.DashPattern = new float[] { 3, 1 };
            Pen ip = new Pen( label_ignoreRegion.BackColor, LineWidth );
            foreach(var kv in Regions)
            {
                DrawShape( e.Graphics, kv.Key == OnConfWorkingRegion ? wpActive : wpInActi, CurrentZoom, kv.Key );
                foreach ( var i in kv.Value )
                    DrawShape( e.Graphics, ip, CurrentZoom, i );
            }
            wpActive.Dispose();
            wpInActi.Dispose();
            ip.Dispose();

            if ( IsOnEditPolygon && OnEditPolygon.Count > 1 )
            {
                using ( Pen p = new Pen( OnEditPolygonColor, LineWidth))
                {
                    for(int i = 1; i < OnEditPolygon.Count; i++)
                    {
                        Point sp = OnEditPolygon[ i - 1 ];
                        Point ep = OnEditPolygon[ i ];
                        if ( CurrentZoom != 1)
                        {
                            sp = new Point( ( int )( sp.X * CurrentZoom ), ( int )( sp.Y * CurrentZoom ) );
                            ep = new Point( ( int )( ep.X * CurrentZoom ), ( int )( ep.Y * CurrentZoom ) );
                        }
                        e.Graphics.DrawLine( p, sp, ep );
                    }
                }
            }
        }

        // load a background image
        private void button_loadImage_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            Bitmap bmp = null;
            string path = null;
            if ( dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty( dlg.FileName ) )
            {
                path = string.Copy( dlg.FileName );
            }
            dlg.Dispose();

            try
            {
                using ( var fs = File.Open( path, FileMode.Open ) )
                {
                    bmp = new Bitmap( fs );
                }
            }
            catch { bmp?.Dispose(); bmp = null; }

            if ( bmp != null )
            {
                BackgroundBmp?.Dispose();
                BackgroundBmp = bmp;
                ImageInfoChange( bmp, CurrentZoom );
            }
        }

        // switch GUI config type: Rect or Polygon
        private void radioButton_rectangle_Click( object sender, EventArgs e )
        {
            if ( !( sender is RadioButton rb ) )
                return;
            var isRectUI = ( rb.Tag is GUIShapeType tp && tp == GUIShapeType.Rectangle && rb.Checked );
            if ( UiTracker != null )
            {
                UiTracker.Visible = isRectUI;
                UiTracker.Enabled = isRectUI;
            }
            if ( !IsOnEditPolygon )
                ResetOnEditPolygon();
        }

        // zoom factor changed
        private void trackBar_zoom_ValueChanged( object sender, EventArgs e )
        {
            if ( ZoomFactors == null || ZoomFactors.Length <= 0 || !( sender is TrackBar tb ) )
                return;

            if ( tb.Value < 0 || tb.Value >= ZoomFactors.Length )
                return;

            CurrentZoom = ZoomFactors[ tb.Value ];
            label_zoomV.Text = $"{CurrentZoom:0.0}";
            ImageInfoChange( BackgroundBmp, CurrentZoom );
        }

        private static Point[] GetPoints( UDataCarrier d )
        {
            Point[] ret = null;
            if ( d.Data is Rectangle r )
            {
                ret = new Point[ 5 ];
                ret[ 0 ] = new Point( r.Left, r.Top );
                ret[ 1 ] = new Point( r.Right, r.Top );
                ret[ 2 ] = new Point( r.Right, r.Bottom );
                ret[ 3 ] = new Point( r.Left, r.Bottom );
                ret[ 4 ] = ret[ 0 ];
            }
            else if (d.Data is Point[] pt)
            {
                ret = pt;
            }
            return ret;
        }

        private static void UseDC2DrawLines(IntPtr handle, Color drawColor, int width, Point[] pts, double zoom = 1.0)
        {
            if ( pts == null || pts.Length < 2 )
                return;
            zoom = zoom <= 0 ? 1.0 : zoom;
            IntPtr dc = GdiWinSdkFunctions.GetDC( handle );
            int color = GdiWinSdkFunctions.ToRGB( drawColor.R, drawColor.G, drawColor.B );
            IntPtr pen = GdiWinSdkFunctions.CreatePen( ( int )GdiWinSdkFunctions.PenStyle.PS_SOLID, width, color );
            try
            {
                GdiWinSdkFunctions.SelectObject( dc, pen );

                POINT pt = new POINT();
                if ( pts != null && pts.Length > 0 )
                {
                    GdiWinSdkFunctions.MoveToEx(
                        dc,
                        zoom != 1 ? ( int )( pts[ 0 ].X * zoom ) : pts[ 0 ].X,
                        zoom != 1 ? ( int )( pts[ 0 ].Y * zoom ) : pts[ 0 ].Y,
                        ref pt );
                    for ( int i = 1; i < pts.Length; i++ )
                    {
                        GdiWinSdkFunctions.LineTo( dc, zoom != 1 ? ( int )( pts[ i ].X * zoom ) : pts[ i ].X, zoom != 1 ? ( int )( pts[ i ].Y * zoom ) : pts[ i ].Y );
                    }
                }
            }
            catch { }
            finally
            {
                if ( dc != IntPtr.Zero ) GdiWinSdkFunctions.ReleaseDC( handle, dc );
                if ( pen != IntPtr.Zero ) GdiWinSdkFunctions.DeleteObject( pen );
            }
        }

        private static void UseDC2DrawCross( IntPtr handle, Color drawColor, Point pt, int width, int expend, double zoom = 1.0 )
        {
            Point[] L1 = new Point[]
            {
                new Point( pt.X - expend, pt.Y - expend ),
                new Point( pt.X + expend, pt.Y + expend )
            };
            Point[] L2 = new Point[]
            {
                new Point( pt.X - expend, pt.Y + expend ),
                new Point( pt.X + expend, pt.Y - expend )
            };
            UseDC2DrawLines( handle, drawColor, width, L1, zoom );
            UseDC2DrawLines( handle, drawColor, width, L2, zoom );
        }

        // combo box select index changed
        // - working region
        // - ignore region
        private void comboBox_workingRegion_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !( sender is ComboBox cb ) )
                return;
            if ( cb.SelectedIndex < 0 )
                return;
            if ( !( cb.Tag is RegionType t ) )
                return;

            // config active combo box
            OpCB = cb;

            List<UDataCarrier> sel = t == RegionType.Working ? WorkingRegions : IgnoreRegions;
            if ( sel == null || cb.SelectedIndex >= sel.Count )
                return;

            if ( t == RegionType.Working )
            {
                // reset working region
                OnConfWorkingRegion = sel[ cb.SelectedIndex ];
                // reload ignore region
                ResetComboBoxList( comboBox_ignoreRegion, IgnoreRegions?.Count ?? 0 );
            }

            // restore
            if ( PrevRecord != null )
                UseDC2DrawLines( PrevRecord.hWnd, PrevRecord.DrawColor, PrevRecord.Width, PrevRecord.Points, CurrentZoom );

            // update current
            PrevRecord = new RestoreDraw()
            {
                hWnd = panel_editor.Handle,
                DrawColor = t == RegionType.Working ? label_createdWorkRgn.BackColor : label_ignoreRegion.BackColor,
                Width = LineWidth,
                Points = GetPoints( sel[ cb.SelectedIndex ] )
            };

            // focus
            UseDC2DrawLines( panel_editor.Handle, Color.Red, LineWidth, GetPoints( sel[ cb.SelectedIndex ] ), CurrentZoom );
        }

        // remove selected region to delete
        private void button_remove_Click( object sender, EventArgs e )
        {
            if ( OpCB == null )
                return;
            if ( !( OpCB.Tag is RegionType t ) )
                return;

            List<UDataCarrier> sel = t == RegionType.Working ? WorkingRegions : IgnoreRegions;
            if ( sel == null || OpCB.SelectedIndex < 0 || OpCB.SelectedIndex >= sel.Count )
                return;

            if ( MessageBox.Show( $"Remove {t} region in index {OpCB.SelectedIndex}?", "Confirm", MessageBoxButtons.OKCancel ) != DialogResult.OK )
                return;

            if ( t == RegionType.Working )
            {
                DeleteWorkingRegion = OnConfWorkingRegion;
                sel = WorkingRegions;
                ResetComboBoxList( comboBox_ignoreRegion, 0 );
            }
            else
                sel.RemoveAt( OpCB.SelectedIndex );

            // reset
            OpCB.Text = "";
            OpCB.Items.Clear();
            for ( int i = 0; i < sel.Count; i++ )
                OpCB.Items.Add( $"{i}" );

            panel_editor.Invalidate();
        }

        // polygon region to closed by mouse left double click
        private void panel_editor_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            if (e.Button == MouseButtons.Left)
            {
                if ( IsOnEditPolygon )
                {
                    if ( OnEditPolygon != null && OnEditPolygon.Count > 2 )
                    {
                        OnEditPolygon.Add( OnEditPolygon.First() );
                        OnEditPolygonClosed = true;
                        panel_editor.Invalidate();
                    }
                    else
                        MessageBox.Show( $"Cannot add polygon less than 3!" );
                }
                else
                    ResetOnEditPolygon();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ResetOnEditPolygon();
            }
        }

        // polygon region apex by mouse left click
        private void panel_editor_MouseClick( object sender, MouseEventArgs e )
        {
            if (IsOnEditPolygon && !OnEditPolygonClosed && e.Button == MouseButtons.Left)
            {
                double x = e.X / ( CurrentZoom <= 0 ? 1.0 : CurrentZoom );
                double y = e.Y / ( CurrentZoom <= 0 ? 1.0 : CurrentZoom );
                OnEditPolygon.Add( new Point( ( int )x, ( int )y ) );
                if ( OnEditPolygon.Count == 1 )
                    UseDC2DrawCross( panel_editor.Handle, OnEditPolygonColor, OnEditPolygon.First(), LineWidth, 25, CurrentZoom );
                else
                    UseDC2DrawLines( panel_editor.Handle, OnEditPolygonColor, LineWidth, OnEditPolygon.ToArray(), CurrentZoom );
            }
        }

        private UDataCarrier CheckIgnore(UDataCarrier data)
        {
            if ( BackgroundBmp == null )
                return null;

            EvalIgnoreRegion eva = new EvalIgnoreRegion(BackgroundBmp.Width, BackgroundBmp.Height);
            try
            {
                if (!eva.ConfWorkRegion(OnConfWorkingRegion))
                {
                    MessageBox.Show( "Warning: working region not ready!" );
                    return null;
                }
                if (!eva.ConfIgnoreRegion(data))
                {
                    MessageBox.Show( "Warning: ignore region config fail!" );
                    return null;
                }
                if (!eva.Eval())
                {
                    MessageBox.Show( "Warning: evaluate ignore region fail!" );
                    return null;
                }
                if (eva.IntersectionPercentage < IntersectionThreshold)
                {
                    MessageBox.Show( $"Error: ignore area intersection%({eva.IntersectionPercentage:0.00}) < {IntersectionThreshold}!!" );
                    return null;
                }
                return data;
            }
            catch { return null; }
            finally
            {
                eva.Dispose();
            }
        }

        // add region to work or ignore
        private void button_add_Click( object sender, EventArgs e )
        {
            if ( BackgroundBmp == null )
            {
                MessageBox.Show( "Error: cannot add without open a background image!" );
                return;
            }

            ComboBox updateCB = null;
            List<UDataCarrier> updateRGN = null;
            if ( IsOnEditPolygon )
            {
                if ( !OnEditPolygonClosed )
                {
                    MessageBox.Show( "Polygon not confirm by mouse left double click!" );
                    return;
                }
                if ( radioButton_ignoreRegion.Checked )
                {
                    var curIgnore = CheckIgnore( UDataCarrier.MakeOne( OnEditPolygon.ToArray() ) );
                    if (curIgnore != null)
                    {
                        var irgn = IgnoreRegions;
                        if ( irgn == null )
                        {
                            MessageBox.Show( "Not pick a working region to set ignore region!" );
                            return;
                        }
                        irgn.Add( curIgnore );
                        ResetOnEditPolygon();
                        MessageBox.Show( "Add polygon to ignore region." );
                        updateCB = comboBox_ignoreRegion;
                        updateRGN = irgn;
                        goto AddClk_InvalidEditor;
                    }
                }
            }
            else if ( UiTracker != null && UiTracker.Visible )
            {
                var r = UiTracker.Positions;
                var status = true;
                if ( radioButton_workArea.Checked )
                {
                    //NewWorkingRegion = UDataCarrier.MakeOne( RevZoom( r.left, r.top, r.right, r.bottom ) );
                    var tmp = UDataCarrier.MakeOne( RevZoom( r.left, r.top, r.right, r.bottom ) );

                    // intersection check
                    foreach(var kv in Regions)
                    {
                        if ( !CheckIntersection( BackgroundBmp.Width, BackgroundBmp.Height, kv.Key, tmp, out var isIntersection ) )
                            continue;
                        if (isIntersection)
                        {
                            MessageBox.Show( $"Error: Found intersection with a work region!" );
                            return;
                        }
                    }

                    OnConfWorkingRegion = tmp;
                    updateCB = comboBox_workingRegion;
                    updateRGN = WorkingRegions;
                }
                else
                {
                    var curIgnore = CheckIgnore( UDataCarrier.MakeOne( RevZoom( r.left, r.top, r.right, r.bottom ) ) );
                    if ( curIgnore != null )
                    {
                        var irgn = IgnoreRegions;
                        if ( irgn == null )
                        {
                            MessageBox.Show( "Not pick a working region to set ignore region!" );
                            return;
                        }
                        irgn.Add( curIgnore );
                        updateCB = comboBox_ignoreRegion;
                        updateRGN = irgn;
                    }
                    else
                        status = false;
                }

                if (status)
                {
                    MessageBox.Show( $"Add rectangle to {( radioButton_ignoreRegion.Checked ? RegionType.Ignore : RegionType.Working )} region" );
                    goto AddClk_InvalidEditor;
                }
                else
                {
                    ResetOnEditPolygon();
                    panel_editor.Invalidate();
                    return;
                }
            }
            ResetOnEditPolygon();
            MessageBox.Show("Nothing happened!");

AddClk_InvalidEditor:
            ResetComboBoxList(updateCB, updateRGN?.Count ?? 0);
            panel_editor.Invalidate();
            return;
        }
    }
}
