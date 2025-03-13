using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    internal static class ImportDllOpenedFunctions
    {

        #region Resource manage
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "model_evaluation_create", CallingConvention = CallingConvention.Cdecl )]
        public static extern IntPtr model_evaluation_create();

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "model_evaluation_release", CallingConvention = CallingConvention.Cdecl )]
        public static extern int model_evaluation_release( IntPtr handler );
        #endregion

        #region region config
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "clean_all_work_areas", CallingConvention = CallingConvention.Cdecl )]
        public static extern int clean_all_work_areas( IntPtr handler );
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "clear_all_triggers", CallingConvention = CallingConvention.Cdecl )]
        public static extern int clear_all_triggers( IntPtr handler );
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "delete_work_area", CallingConvention = CallingConvention.Cdecl )]
        public static extern int delete_work_area( IntPtr handler, int workarea_x, int workarea_y );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_work_area", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_work_area( IntPtr handler, int[] points );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "delete_ignored_area", CallingConvention = CallingConvention.Cdecl )]
        public static extern int delete_ignored_area( IntPtr handler, int workarea_x, int workarea_y );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_ignored_area", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_ignored_area( IntPtr handler, int workarea_x, int workarea_y, int[] number, int[] points );
        #endregion

        #region parameters
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_wafer_min_area", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_wafer_min_area( IntPtr handler, int workarea_x, int workarea_y, int min_area );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_wafer_pen_distance", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_wafer_pen_distance( IntPtr handler, int workarea_x, int workarea_y, int distance );
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_wafer_pen_iou_threshold", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_wafer_pen_iou_threshold( IntPtr handler, int workarea_x, int workarea_y, int iou_threshold );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_wafer_trigger", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_wafer_trigger( IntPtr handler, int workarea_x, int workarea_y, String name, float interval, int count );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_distribute_method", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_distribute_method( IntPtr handler, int method );
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "get_wafer_event_list", CallingConvention = CallingConvention.Cdecl )]
        public static extern int get_wafer_event_list( IntPtr handler, int[] event_num, byte[] buf );
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "set_eval_function", CallingConvention = CallingConvention.Cdecl )]
        public static extern int set_eval_function( IntPtr handler, int workarea_x, int workarea_y, String eval_func );
        #endregion

        #region run
        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "get_estimation_output", CallingConvention = CallingConvention.Cdecl )]
        public static extern int get_estimation_output( IntPtr handler, float[] values );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "evaluate_detection_data", CallingConvention = CallingConvention.Cdecl )]
        public static extern int evaluate_detection_data( IntPtr handler, int size, float[] detection );

        [DllImport( "SOPVisionActionEvaluation.dll", EntryPoint = "get_estimation_size", CallingConvention = CallingConvention.Cdecl )]
        public static extern int get_estimation_size( IntPtr handler );
        #endregion
    }
}
