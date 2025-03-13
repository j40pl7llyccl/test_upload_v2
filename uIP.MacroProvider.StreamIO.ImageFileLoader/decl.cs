using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    internal enum OpenImageIndex01 : int
    {
        SearchDir = 0,
        FoundFiles,
        CurrentIndex,
        Instance,
        MaxAsCount // should not add below
    }

    internal enum OpenImageKey02 : int
    {
        Param02_LoadingPath,
        Param02_SearchPattern,
        Param02_EnableCycleRun,
        UserControl,
        LoadedPaths,
        IncIndex,
        NextIndex,
        Image,
    }

    internal enum ImageFromMethodMutableDataKey : int
    {
        Param_LoadingPath,
        Param_SearchPattern,
        Param_EnableCycleRun,
        Param_ResultJumpIndex,
        Param_ParsingProvider,
        Param_ParsingProvideFunc,
        Param_ParsingFilename,
        Param_IsIncIndex,
        LoadedFilePaths,
        LoadedFileTimestamps,
        NextIndex,
        ImageInstance,
        ExtraData,
    }

    public class PathContainImagesSettings
    {
        public string FolderPath { get; set; }
        public string SearchPattern { get; set; } = "*.*";
        public bool IsCycleRun { get; set; } = false;
    }
}
