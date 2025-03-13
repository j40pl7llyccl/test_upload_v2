using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using uIP.Lib;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public class EvaluationConf : IDisposable
    {
        internal string BackgroundImagePath { get; set; }
        internal DetectionParameters Parameters { get; set; } = new DetectionParameters();
        internal IntPtr Handler { get; set; } = IntPtr.Zero;
        //internal Dictionary<UDataCarrier, List<UDataCarrier>> Regions { get; set; }

        public EvaluationConf() { }
        internal bool SetBackgroundImagePath(string path, string dstDir)
        {
            if ( string.IsNullOrEmpty( path ) || !File.Exists( path ) || !Directory.Exists( dstDir ) )
                return false;

            try
            {
                if ( !string.IsNullOrEmpty(BackgroundImagePath) && File.Exists(BackgroundImagePath) )
                    File.Delete(BackgroundImagePath);
            }
            catch { }

            BackgroundImagePath = null;

            var ext = Path.GetExtension( path );
            var dstFullPath = Path.Combine( dstDir, $"{uMProvidSopDetect.PredefinedMacroMethodNameEvaluation}_{CommonUtilities.GetCurrentTimeStr( "" )}{( string.IsNullOrEmpty( ext ) ? "" : ext )}" );
            try
            {
                File.Copy(path, dstFullPath, true );
                BackgroundImagePath = dstFullPath;
                return true;
            }
            catch { return false; }
        }

        internal bool SetBackgroundImage(Bitmap b, string dstDir)
        {
            if ( b == null || !Directory.Exists( dstDir ) )
                return false;

            try
            {
                if ( !string.IsNullOrEmpty( BackgroundImagePath ) && File.Exists( BackgroundImagePath ) )
                    File.Delete( BackgroundImagePath );
            }
            catch { }

            BackgroundImagePath = null;
            var dstFullPath = Path.Combine( dstDir, $"{uMProvidSopDetect.PredefinedMacroMethodNameEvaluation}_{CommonUtilities.GetCurrentTimeStr( "" )}.png" );
            try
            {
                using(var fs = File.Open(dstFullPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    b.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                }
                BackgroundImagePath = dstFullPath;
                return true;
            }
            catch { return false; }
        }

        private static bool ConvertIgnore( object[] regions, out int[] numbers, out int[] pointCoor)
        {
            numbers = new int[ 0 ];
            pointCoor = new int[ 0 ];
            if ( regions == null || regions.Length == 0 )
                return false;

            var numb = new List<int>();
            var coor = new List<int>();
            foreach(var r in regions)
            {
                if ( r is Rectangle rect)
                {
                    numb.Add( 4 );
                    coor.AddRange( new List<int> { rect.Left, rect.Top, rect.Right, rect.Top, rect.Right, rect.Bottom, rect.Left, rect.Bottom } );
                }
                else if ( r is Point[] poly && poly != null && poly.Length > 0)
                {
                    numb.Add( poly.Length );
                    foreach(var pt in poly)
                    {
                        coor.Add( pt.X );
                        coor.Add( pt.Y );
                    }
                }
            }
            numbers = numb.ToArray();
            pointCoor = coor.ToArray();
            return true;
        }

        internal void ApplyParameters()
        {
            if ( Handler == IntPtr.Zero )
                return;

#if !DebugParam
            // set global parameter
            ImportDllOpenedFunctions.set_distribute_method( Handler, Parameters.JudgeObjRegionWay );
            // clear all working area and relate settings
            ImportDllOpenedFunctions.clean_all_work_areas( Handler );
            //ImportDllOpenedFunctions.clear_all_triggers( Handler );
            // config regions
            for(int i = 0; i < Parameters.WorkRegions.Count; i++)
            {
                var wr = (Rectangle)Parameters.WorkRegions[i];
                // 1. config region
                // set working region
                ImportDllOpenedFunctions.set_work_area( Handler, new int[] { wr.Left, wr.Top, wr.Right, wr.Bottom } );
                if (ConvertIgnore( Parameters.IgnoreRegionsInsideWorkRegions[ i ], out var nums, out var coor ))
                {
                    ImportDllOpenedFunctions.set_ignored_area( Handler, wr.X, wr.Y, nums, coor );
                }
                // 2. config fix for SOP
                ImportDllOpenedFunctions.set_eval_function( Handler, wr.X, wr.Y, "wafer_sop" );
                // wafer min area
                ImportDllOpenedFunctions.set_wafer_min_area( Handler, wr.X, wr.Y, Parameters.WorkRegionParams[ i ].WaferMinArea );
                // wafer pen distance
                ImportDllOpenedFunctions.set_wafer_pen_distance( Handler, wr.X, wr.Y, Parameters.WorkRegionParams[ i ].Wafer2PenMaxDist );
                // Trigger event
                // set event trigger
                foreach(var evt in Parameters.WorkRegionParams[i].TriggerConditions)
                {
                    ImportDllOpenedFunctions.set_wafer_trigger( Handler, wr.X, wr.Y, evt.TriggerType, ( float )evt.TimeIntervalSec, evt.EventCountThreshold );
                }
            }
#endif
        }

        public void Dispose()
        {
#if !DebugParam
            if (Handler != IntPtr.Zero)
                ImportDllOpenedFunctions.model_evaluation_release( Handler );
            Handler = IntPtr.Zero;
#endif
        }

    }
}
