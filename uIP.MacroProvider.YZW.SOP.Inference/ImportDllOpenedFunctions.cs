using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.YZW.SOP.Inference
{
    internal static class ImportDllOpenedFunctions
    {
        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "image_data", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int image_data( IntPtr handler, IntPtr raw );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "load_model", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int load_model( IntPtr handler, String modelpath );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "set_network_address", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int set_network_address( IntPtr handler, String input, String output );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "set_model_input_data", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int set_model_input_data( IntPtr handler, int width, int height, int channel );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "set_model_output_data", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int set_model_output_data( IntPtr handler, int batch_num, int block_size, int block_number );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "create_model", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int create_model( IntPtr handler );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "destroy_model", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int destroy_model( IntPtr handler );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "model_inference", CallingConvention = CallingConvention.StdCall )]
        internal static extern int model_inference( IntPtr handler );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "get_output_size", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int get_detection_size( IntPtr handler );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "get_model_output", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int get_model_output( IntPtr handler, float[] buffer, bool show = false );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "set_frame_property", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int set_frame_property( IntPtr handler, int width, int height, int channel );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "set_mean_std", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int set_mean_std( IntPtr handler, bool norm, float[] mean, float[] std );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "set_postprocess_thresholds", CallingConvention = CallingConvention.Cdecl )]
        internal static extern int set_postprocess_thresholds( IntPtr handler, float box_confidence, float nms_threshold );

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "model_inference_create", CallingConvention = CallingConvention.StdCall )]
        internal static extern IntPtr model_inference_create();

        [DllImport( "SOPVisionModelInference.dll", EntryPoint = "model_inference_release", CallingConvention = CallingConvention.Cdecl )]
        internal static extern void model_inference_release( IntPtr handler );
    }
}
