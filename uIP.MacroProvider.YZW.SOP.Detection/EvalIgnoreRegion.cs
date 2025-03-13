using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public class EvalIgnoreRegion : IDisposable
    {
        public Bitmap ResultBmp { get; private set; }
        private Region WorkRegion { get; set; }
        private Region IntersectionRegion { get; set; }
        private Region ExcludeRegion { get; set; }

        public Color IntersectionColor { get; set; } = Color.Aqua; // not equ to ExcludeColoe
        public Color ExcludeColor { get; set; } = Color.Yellow; // not equ to IntersectionColor

        public int RefImgW { get; private set; }
        public int RefImgH { get; private set; }
        public Rectangle IgnoreBoundingBox { get; private set; }

        public int IntersectionArea { get; private set; } = 0;
        public int ExcludeArea { get; private set; } = 0;

        public double IntersectionPercentage => ( ( IntersectionArea + ExcludeArea ) == 0 ? 0 : ( double )IntersectionArea / ( double )( IntersectionArea + ExcludeArea ) ) * 100.0;

        public EvalIgnoreRegion(int refW, int refH)
        {
            RefImgW = refW;
            RefImgH = refH;
        }

        void ResetSize(int w, int h)
        {
            RefImgW = w;
            RefImgH = h;
        }

        public void Dispose()
        {
            ResultBmp?.Dispose();
            ResultBmp = null;

            WorkRegion?.Dispose();
            WorkRegion = null;

            IntersectionRegion?.Dispose();
            IntersectionRegion = null;

            ExcludeRegion?.Dispose();
            ExcludeRegion = null;
        }

        public bool ConfWorkRegion(Rectangle r)
        {
            WorkRegion?.Dispose();
            WorkRegion = null;

            try
            {
                WorkRegion = new Region(r);
                return true;
            }
            catch { return false; }
        }
        public bool ConfWorkRegion( Point[] pts )
        {
            WorkRegion?.Dispose();
            WorkRegion = null;

            if ( pts == null || pts.Length == 0 )
                return false;


            try
            {
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon( pts );

                WorkRegion = new Region( path );

                path.Dispose();
                return true;
            }
            catch { return false; }
        }
        public bool ConfWorkRegion(UDataCarrier data)
        {
            if ( UDataCarrier.Get( data, new Rectangle(), out var v ) )
                return ConfWorkRegion( v );
            else if (UDataCarrier.Get(data, new Point[0], out var pts ))
                return ConfWorkRegion( pts );
            return false;
        }

        public bool ConfIgnoreRegion( Rectangle r )
        {
            IntersectionRegion?.Dispose();
            IntersectionRegion = null;

            ExcludeRegion?.Dispose();
            ExcludeRegion = null;

            try
            {
                IntersectionRegion = new Region( r );
                ExcludeRegion = new Region( r );

                IgnoreBoundingBox = r;
                return true;
            }
            catch { return false; }
        }

        public bool ConfIgnoreRegion( Point[] pts)
        {
            IntersectionRegion?.Dispose();
            IntersectionRegion = null;

            ExcludeRegion?.Dispose();
            ExcludeRegion = null;

            try
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddPolygon( pts );

                IntersectionRegion = new Region( gp );
                ExcludeRegion = new Region( gp );

                gp.Dispose();


                int l = int.MaxValue;
                int t = int.MaxValue;
                int r = int.MinValue;
                int b = int.MinValue;
                foreach ( var p in pts )
                {
                    if ( p.X < l ) l = p.X;
                    if ( p.Y < t ) t = p.Y;

                    if ( p.X > r ) r = p.X;
                    if ( p.Y > b ) b = p.Y;
                }
                IgnoreBoundingBox = new Rectangle( l, t, r - l + 1, b - t + 1 );
                return true;
            }
            catch { return false; }
        }

        public bool ConfIgnoreRegion(UDataCarrier data)
        {
            if ( UDataCarrier.Get( data, new Rectangle(), out var r ) )
                return ConfIgnoreRegion( r );
            else if ( UDataCarrier.Get(data, new Point[0], out var pts ))
                return ConfIgnoreRegion( pts );
            return false;
        }

        public bool Save(string filepath, System.Drawing.Imaging.ImageFormat format)
        {
            if ( ResultBmp == null )
                return false;
            try
            {
                using(var fs = File.Open(filepath, FileMode.Create, FileAccess.ReadWrite))
                {
                    ResultBmp.Save( fs, format );
                }
                return true;
            }
            catch { return false; }
        }

        bool CalcArea()
        {
            if ( ResultBmp == null ) return false;

            IntersectionArea = 0;
            ExcludeArea = 0;
            for ( int y = 0; y < IgnoreBoundingBox.Height; y++ )
            {
                var coorY = y + IgnoreBoundingBox.Y;
                if ( coorY >= ResultBmp.Height )
                    break;
                for ( int x = 0; x < IgnoreBoundingBox.Width; x++ )
                {
                    var coorX = x + IgnoreBoundingBox.X;
                    if ( coorX >= ResultBmp.Width )
                        break;

                    var pix = ResultBmp.GetPixel( coorX, coorY );
                    if ( pix.R == IntersectionColor.R && pix.G == IntersectionColor.G && pix.B == IntersectionColor.B )
                        ++IntersectionArea;
                    if ( pix.R == ExcludeColor.R && pix.G == ExcludeColor.G && pix.B == ExcludeColor.B )
                        ++ExcludeArea;
                }
            }

            return true;
        }

        public bool Eval(bool bExecExclude = true)
        {
            IntersectionArea = 0;
            ExcludeArea = 0;
            ResultBmp?.Dispose();
            ResultBmp = null;

            if ( WorkRegion == null || IntersectionRegion == null || ExcludeRegion == null || RefImgW <= 0 || RefImgH <= 0 )
                return false;

            var status = true;
            Bitmap r = null;
            Brush bb = null;
            Brush ib = null;
            Brush eb = null;

            IntersectionRegion.Intersect( WorkRegion );
            if (bExecExclude)
                ExcludeRegion.Exclude( WorkRegion );

            try
            {
                r = new Bitmap( RefImgW, RefImgH, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
                bb = new SolidBrush( Color.Black );
                ib = new SolidBrush( IntersectionColor );
                eb = new SolidBrush( ExcludeColor );

                using ( var g = Graphics.FromImage( r ) )
                {
                    // reset background
                    g.FillRectangle( bb, new Rectangle( 0, 0, RefImgW, RefImgH ) );

                    // draw region
                    g.FillRegion( ib, IntersectionRegion );

                    if (bExecExclude)
                        g.FillRegion( eb, ExcludeRegion );
                }
                ResultBmp = r;
            }
            catch { status = false; }

            bb?.Dispose();
            ib?.Dispose();
            eb?.Dispose();

            if ( !status )
                r?.Dispose();
            else
                status = CalcArea();

            return status;
        }
    }
}
