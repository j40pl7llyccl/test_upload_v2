using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib.MarshalWinSDK;

namespace uIP.MacroProvider.Tools.Labeling
{
    public class PolyonSelection : IDisposable
    {
        bool _bDisposed = false;
        bool _bClosed = false;
        Control Container { get; set; }

        public Color ActiveColor { get; set; } = Color.Aqua;
        public Color DoneColor { get; set; } = Color.Orange;
        public float DrawLineWidth { get; set; } = 3;
        public int BeginCrossSize { get; set; } = 15;
        public float Zoom { get; private set; } = 1;

        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool IsFinished => _bClosed;
        public bool IsEmpty => Points == null || Points.Count == 0;

        private List<Point> Points { get; set; } = new List<Point>();

        public List<Point> Coordinates {
            //get => Points;
            get {
                var zf = Zoom <= 0 ? 1.0 : Zoom;
                return ( from i in Points select new Point( ( int ) ( ( float ) i.X / zf ), ( int ) ( ( float ) i.Y / zf ) ) ).ToList();
            }
        }

        public bool IsDisposed => _bDisposed;

        public PolyonSelection() { }
        public PolyonSelection(Control ctrl)
        {
            Container = ctrl;
            Config();
        }

        public void SetControl(Control c)
        {
            if ( Container != null )
                return;

            Container = c;
            Config();
        }

        ~PolyonSelection()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void ResetZoom(float v, bool bCallInvalidate = false)
        {
            if ( _bDisposed || Container == null )
                return;

            v = v <= 0 ? 1 : v;

            if ( v == Zoom )
                return;

            Zoom = v;

            if (Container.InvokeRequired)
            {
                Container.Invoke( new Action( () => {
                    Points = ( from p in Points select ( new Point( ( int ) ( ( float ) p.X * Zoom ), ( int ) ( ( float ) p.Y * Zoom ) ) ) ).ToList();
                } ) );
            }
            else
                Points = ( from p in Points select ( new Point( ( int ) ( ( float ) p.X * Zoom ), ( int ) ( ( float ) p.Y * Zoom ) ) ) ).ToList();

            if ( bCallInvalidate )
                Container.Invalidate();
        }

        public void ResetPoints(bool bCallInvalidate = false)
        {
            if ( _bDisposed || Container == null )
                return;

            if ( Container.InvokeRequired )
            {
                Container.Invoke( new Action( () => {
                    Points.Clear();
                } ) );
            }
            else
                Points.Clear();


            if ( bCallInvalidate )
                Container.Invalidate();
        }

        void Dispose(bool disposing)
        {
            if ( _bDisposed )
                return;


            if ( disposing)
            {
                if (Container != null && !Container.IsDisposed)
                {
                    Container.MouseClick -= MouseClick;
                    Container.MouseDoubleClick -= MouseDoubleClick;
                    Container.Paint -= ContainerOnPaint;
                }

            }
            Container = null;
            _bDisposed = true;
        }

        private void Config()
        {
            if ( Container == null )
                return;

            Container.MouseClick += MouseClick;
            Container.MouseDoubleClick += MouseDoubleClick;
            Container.Paint += ContainerOnPaint;
        }

        void ContainerSzChanged( object sender, EventArgs e )
        {
            if ( _bDisposed )
                return;

        }

        private static Rectangle MakeBoundingBox(List<Point> pts, int expend = 2)
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

            foreach(var pt in pts)
            {
                if (pt.X < minX) minX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            return new Rectangle(minX - expend, minY - expend, maxX - minX + expend, maxY - minY + expend);
        }

        void MouseClick(object sender, MouseEventArgs e)
        {
            if ( _bDisposed || _bClosed || Container.IsDisposed )
                return;

            if ( !Enabled || !Visible )
                return;


            if ( e.Button == MouseButtons.Left )
            {
                var bDiff = Points.Count == 0 ? true : Points.Last().X != e.X || Points.Last().Y != e.Y;
                if ( bDiff )
                {
                    Points.Add( new Point( e.X, e.Y ) );

                    var hdc = GdiWinSdkFunctions.GetDC( Container.Handle );
                    using ( var g = Graphics.FromHdc(hdc) )
                    {
                        if ( Points.Count == 1 )
                            DrawCross( g, Points.First(), ActiveColor, BeginCrossSize, DrawLineWidth );
                        else
                            DrawLine( g, Points[ Points.Count - 2 ], Points.Last(), ActiveColor, DrawLineWidth );
                    }
                    GdiWinSdkFunctions.ReleaseDC( Container.Handle, hdc );
                }
            }
            else if ( e.Button == MouseButtons.Right )
            {
                /*
                var r = new Rectangle();

                if ( Points.Count > 1 )
                    r = MakeBoundingBox( new List<Point> { Points[ Points.Count - 2 ], Points.Last() }, (int) DrawLineWidth + 1 );
                else if ( Points.Count == 1 )
                    r = new Rectangle( 
                        Points[ 0 ].X - BeginCrossSize - (int)DrawLineWidth - 1, 
                        Points[ 0 ].Y - BeginCrossSize - (int) DrawLineWidth - 1, 
                        BeginCrossSize * 2 + 1 + (int) DrawLineWidth + 1, 
                        BeginCrossSize * 2 + 1 + (int) DrawLineWidth + 1 );
                */

                if ( Points.Count > 0 )
                {
                    Points.RemoveAt( Points.Count - 1 );
                    Container.Invalidate();
                }
                //if ( r.Width > 0 && r.Height > 0 )
                //    Container.Invalidate( r );
            }

        }
        void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ( _bDisposed || _bClosed || Container.IsDisposed )
                return;

            if ( !Enabled || !Visible )
                return;

            // cannot done
            if ( Points.Count < 3 )
                return;

            if ( e.Button != MouseButtons.Left )
                return;

            _bClosed = true;
            // close;
            Points.Add( Points.First() );
            Container.Invalidate();            
        }

        private static void DrawCross(Graphics g, Point pt, Color c, int size, float width)
        {
            if ( g == null )
                return;
            using(var p = new Pen(c, width))
            {
                g.DrawLine( p, new Point( pt.X - size, pt.Y - size ), new Point( pt.X + size, pt.Y + size ) );
                g.DrawLine( p, new Point( pt.X + size, pt.Y - size ), new Point( pt.X - size, pt.Y + size ) );
            }
        }
        private static void DrawLine(Graphics g, Point sp, Point ep, Color c, float width)
        {
            if ( g == null )
                return;
            using(var p = new Pen(c, width))
            {
                g.DrawLine( p, sp, ep );
            }
        }


        void ContainerOnPaint(object sender, PaintEventArgs e)
        {
            if ( _bDisposed )
                return;

            if ( !Visible )
                return;

            if ( Points.Count == 0 )
                return;

            if ( !_bClosed )
                DrawCross( e.Graphics, Points[ 0 ], ActiveColor, BeginCrossSize, DrawLineWidth );

            for ( int i = 1; i < Points.Count; i++ )
            {
                DrawLine( e.Graphics, Points[ i - 1 ], Points[ i ], _bClosed ? DoneColor : ActiveColor, DrawLineWidth );
            }
        }
    }
}
