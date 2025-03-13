using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    internal enum MutableKeys
    {
        Param_SaveImage,
        Param_SaveImageDir,
        Param_SaveImageFormat,
        Param_WriteDbResult,
        Param_LogMsg,
        Param_ContIndex,
        Param_IsFromVideo,
        ImageSetResults,
        ResultImageOutputDir,
    }

    internal enum ResultDescription
    {
        inspect_status,
        inspect_messages,
    }

    internal enum InputSourceDescription
    {
        input_source_from,
        input_source_from_buffer,
        input_source_from_video,
        input_source_from_folder
    }

    internal sealed class Decls
    {
        internal static Dictionary<string, ImageFormat> AcceptableImageFormats = new Dictionary<string, ImageFormat>()
        {
            { "png", ImageFormat.Png },
            { "jpg", ImageFormat.Jpeg },
            { "tiff", ImageFormat.Tiff },
            { "bmp", ImageFormat.Bmp }
        };
    }


}
