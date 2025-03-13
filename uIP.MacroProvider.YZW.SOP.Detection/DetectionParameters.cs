using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public class DetectionParameters
    {
        public int JudgeObjRegionWay { get; set; } = 0; // distribute_method

        public List<object> WorkRegions { get; set; } = new List<object>();
        public List<object[]> IgnoreRegionsInsideWorkRegions { get; set; } = new List<object[]>();
        public List<WorkingRegionParameters> WorkRegionParams { get; set; } = new List<WorkingRegionParameters>();

        public DetectionParameters() { }
        public static DetectionParameters Clone( DetectionParameters src )
        {
            DetectionParameters ret = new DetectionParameters();
            ret.JudgeObjRegionWay = src.JudgeObjRegionWay;
            foreach ( var obj in src.WorkRegions )
            {
                ret.WorkRegions.Add( CloneRegion( obj ) );
            }
            foreach ( var obj in src.IgnoreRegionsInsideWorkRegions )
            {
                if ( obj == null ) ret.IgnoreRegionsInsideWorkRegions.Add( null );
                var oo = new List<object>();
                foreach ( var rgn in obj )
                    oo.Add( CloneRegion( rgn ) );
                ret.IgnoreRegionsInsideWorkRegions.Add( oo.ToArray() );
            }
            foreach ( var obj in src.WorkRegionParams )
                ret.WorkRegionParams.Add( WorkingRegionParameters.Clone( obj ) );
            return ret;
        }

        internal Dictionary<UDataCarrier, UDataCarrier[]> ConvRegion()
        {
            if ( WorkRegions == null || WorkRegions.Count == 0 )
                return new Dictionary<UDataCarrier, UDataCarrier[]>();

            var ret = new Dictionary<UDataCarrier, UDataCarrier[]>();
            for ( int i = 0; i < WorkRegions.Count; i++ )
            {
                var k = UDataCarrier.Make( WorkRegions[ i ] );
                var v = (from vv in IgnoreRegionsInsideWorkRegions[i] select UDataCarrier.Make(vv)).ToArray();
                ret.Add( k, v );
            }
            return ret;
        }

        internal void ToRegion(Dictionary<UDataCarrier, UDataCarrier[]> rgn)
        {
            if ( rgn == null || rgn.Count == 0 )
                return;

            var k = new List<object>();
            var v = new List<object[]>();

            foreach(var kv in rgn)
            {
                if ( kv.Key.Tp == typeof( Rectangle ) )
                    k.Add( UDataCarrier.Get( kv.Key, new Rectangle() ) );
                if ( kv.Value == null || kv.Value.Length == 0 )
                    v.Add( new object[ 0 ] );
                else
                {
                    var tmp = new List<object>();
                    foreach(var vv in kv.Value)
                    {
                        if ( vv.Tp == typeof( Rectangle ) )
                            tmp.Add( UDataCarrier.Get( vv, new Rectangle() ) );
                        else if ( vv.Tp == typeof( Point[] ) )
                            tmp.Add( UDataCarrier.Get( vv, new Point[ 0 ] ) );
                    }
                    v.Add( tmp.ToArray() );
                }
            }

            WorkRegions = k;
            IgnoreRegionsInsideWorkRegions = v;
        }

        internal static object CloneRegion(object src)
        {
            if ( src == null ) return null;
            if ( src is Rectangle rect )
                return new Rectangle( rect.X, rect.Y, rect.Width, rect.Height );
            else if ( src is Point[] pts )
            {
                var ret = new List<Point>();
                foreach ( var pt in pts )
                {
                    ret.Add( new Point( pt.X, pt.Y ) );
                }
                return ret.ToArray();
            }
            return src;
        }
    }
}
