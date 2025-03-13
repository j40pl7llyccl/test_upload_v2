using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.YZW.SOP.Inference
{
    public class ModelData : IDisposable
    {
        public ModelData() { }
        public void Dispose()
        {
#if !NOT_IMPORT_DLL
            // free instance resource
            if ( Handle != IntPtr.Zero )
            {
                ImportDllOpenedFunctions.destroy_model( Handle );
                ImportDllOpenedFunctions.model_inference_release( Handle );
            }
#endif
            Handle = IntPtr.Zero;

            // delete path
            try
            {
                if ( Directory.Exists( TemporalDir ) )
                {
                    Directory.Delete( TemporalDir, true );
                }
            }
            catch { }
        }

        internal string TemporalDir { get; set; } = "";
        internal IntPtr Handle { get; set; } = IntPtr.Zero;
        internal bool IsModelTaught { get; set; } = false;

        internal string EncryptModelFilepath { get; set; }

        internal int InputImgResolutionW { get; set; } = 640;
        internal int InputImgResolutionH { get; set; } = 640;
        internal int InputImgChannels { get; set; } = 3;

        internal int FrameResolutionW { get; set; } = 1920;
        internal int FrameResolutionH { get; set; } = 1080;
        internal int FrameImgChannels { get; set; } = 3;

        internal int OutputBatchNo { get; set; } = 1;
        internal int OutputBlockSize { get; set; } = 25200;
        internal int OutputBlockNo { get; set; } = 10;

        internal string NetworkInput { get; set; } = "images";
        internal string NetworkOutput { get; set; } = "output0";

        internal double PostProcBoxConfidence { get; set; } = 0.5;
        internal double PostProcNMSThreshold { get; set; } = 0.5;
    }
}
