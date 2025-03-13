using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class uMProvidImageLoader
    {
        const string ImageFromBufferMethodName = "InputFromBufferCall";
        const string ImageFromFolderMethodName = "InputFromFolderCall";
        UMacro KeepImageFromBufferMethodMacro { get; set; } = null;
        UMacro KeepImageFromFolderMethodMacro { get; set; } = null;

        private void InitImageFrom()
        {

            // config macro
            KeepImageFromBufferMethodMacro =
                new UMacro( null, NameOfCSharpDefClass, ImageFromBufferMethodName, InputFromBufferCall,
                    null, null,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(IntPtr), "image buffer"),
                        new UDataCarrierTypeDescription(typeof(int), "image width"),
                        new UDataCarrierTypeDescription(typeof(int), "image height"),
                        new UDataCarrierTypeDescription(typeof(int), "image pixel in bits"),
                        new UDataCarrierTypeDescription(typeof(int), "image stride"),
                        new UDataCarrierTypeDescription(typeof(DateTime), "time stamp")
                    },
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(IntPtr), "image buffer"),
                        new UDataCarrierTypeDescription(typeof(int), "image width"),
                        new UDataCarrierTypeDescription(typeof(int), "image height"),
                        new UDataCarrierTypeDescription(typeof(int), "image pixel in bits"),
                        new UDataCarrierTypeDescription(typeof(int), "image stride"),
                        new UDataCarrierTypeDescription(typeof(DateTime), "time stamp")
                    }
                );
            m_UserQueryOpenedMethods.Add( KeepImageFromBufferMethodMacro );

            KeepImageFromFolderMethodMacro =
                new UMacro( null, NameOfCSharpDefClass, ImageFromFolderMethodName, InputFromFolderCall,
                    null, null,
                    new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( string ), "folder path" ) },
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(IntPtr), "image buffer"),
                        new UDataCarrierTypeDescription(typeof(int), "image width"),
                        new UDataCarrierTypeDescription(typeof(int), "image height"),
                        new UDataCarrierTypeDescription(typeof(int), "image pixel in bits"),
                        new UDataCarrierTypeDescription(typeof(int), "image stride"),
                        new UDataCarrierTypeDescription(typeof(DateTime), "time stamp")
                    },
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(IntPtr), "image buffer"),
                        new UDataCarrierTypeDescription(typeof(int), "image width"),
                        new UDataCarrierTypeDescription(typeof(int), "image height"),
                        new UDataCarrierTypeDescription(typeof(int), "image pixel in bits"),
                        new UDataCarrierTypeDescription(typeof(int), "image stride"),
                        new UDataCarrierTypeDescription(typeof(DateTime), "time stamp")
                    }
                );
            m_UserQueryOpenedMethods.Add( KeepImageFromFolderMethodMacro );

            // done create macro shell
            m_createMacroDoneFromMethod.Add( ImageFromBufferMethodName, MacroShellDoneCallImageFrom );
            m_createMacroDoneFromMethod.Add( ImageFromFolderMethodName, MacroShellDoneCallImageFrom );

            // parameters file GET/SET
            m_macroMethodSettingsGet.Add( ImageFromFolderMethodName, GetMacroMethodSettingsImageFrom );
            m_macroMethodSettingsSet.Add( ImageFromFolderMethodName, SetMacroMethodSettingsImageFrom );

            // popup
            m_macroMethodConfigPopup.Add( ImageFromBufferMethodName, PopupConf_ImageFrom );
            m_macroMethodConfigPopup.Add( ImageFromFolderMethodName, PopupConf_ImageFrom );
        }

        /// <summary>
        /// macro shell done call
        /// </summary>
        /// <param name="callMethodName"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        private bool MacroShellDoneCallImageFrom( string callMethodName, UMacro instance )
        {
            if ( callMethodName == ImageFromBufferMethodName )
            {
                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                {
                    { ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), UDataCarrier.MakeOne(-1) }, // for jump to an index
                    { ImageFromMethodMutableDataKey.ImageInstance.ToString(), UDataCarrier.MakeOne(new UImageComBuffer(), true) }, // image buffer
                }, true );
            }
            else if ( callMethodName == ImageFromFolderMethodName )
            {
                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                {
                    { ImageFromMethodMutableDataKey.Param_LoadingPath.ToString(), UDataCarrier.MakeOne("") }, // loading folder path
                    { ImageFromMethodMutableDataKey.Param_SearchPattern.ToString(), UDataCarrier.MakeOne("*.*") }, // search pattern
                    { ImageFromMethodMutableDataKey.Param_EnableCycleRun.ToString(), UDataCarrier.MakeOne(false) }, // enable cycle run
                    { ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), UDataCarrier.MakeOne(-1) }, // result jump
                    { ImageFromMethodMutableDataKey.Param_IsIncIndex.ToString(), UDataCarrier.MakeOne(true) }, // increment index or decrement
                    { ImageFromMethodMutableDataKey.NextIndex.ToString(), UDataCarrier.MakeOne(0) }, // next index
                    { ImageFromMethodMutableDataKey.ImageInstance.ToString(), UDataCarrier.MakeOne( new UImageComBuffer(), true) }, // image buffer
                }, true );
            }

            return true;
        }


        /// <summary>
        /// Get settings to store
        /// </summary>
        /// <param name="m"></param>
        /// <param name="settings"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        bool GetMacroMethodSettingsImageFrom( UMacro m, out object settings, out Type t )
        {
            settings = null;
            t = null;

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null, out var dic ) || dic == null || dic.Count <= 0 )
                return true;

            var toRepo = new Dictionary<string, UDataCarrier>();
            foreach(var kv in dic)
            {
                // begin with Param_ as saving item
                if (kv.Key.IndexOf("Param_") == 0)
                    toRepo.Add(kv.Key, kv.Value);
            }

            // to string[]
            if ( !UDataCarrier.SerializeDicKeyString( toRepo, out var strArr ) || strArr == null )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{GetType().FullName} call GetMacroMethodSettingsImageFrom() with {m.MethodName} settings fail" );
                return true;
            }

            settings = strArr;
            t = strArr.GetType();
            return true;
        }

        /// <summary>
        /// Set settings to load
        /// </summary>
        /// <param name="m"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        bool SetMacroMethodSettingsImageFrom( UMacro m, object settings )
        {
            if ( settings == null || !UDataCarrier.Get<Dictionary<string, UDataCarrier>>(m.MutableInitialData, null, out var dic) || dic == null )
                return false;

            // restore from string[]
            if ( !UDataCarrier.DeserializeDicKeyStringValueOne( settings as string[], out var got ) )
                return false;

            // from settings, config to m
            foreach(var kv in got)
            {
                UDataCarrier.Set( dic, kv.Key, kv.Value );
            }

            // by method name, do something
            if (m.MethodName == ImageFromFolderMethodName)
            {
                ApplyFolderSettings(m);
            }
            else
            {
                // config jump index
                if ( UDataCarrier.Get( got, ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), -1, out var jindex ) &&
                    m is UMacroCapableOfCtrlFlow fcm && fcm != null )
                {
                    fcm.MustJump = jindex >= 0;
                    fcm.Jump2WhichMacro = jindex;
                }
            }
            return true;
        }

        internal static void ApplyFolderSettings(UMacro m)
        {
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null, out var dic ) )
                return;
            var folderReady =
                UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_LoadingPath.ToString(), "", out var folderpath ) &&
                !string.IsNullOrEmpty( folderpath ) && Directory.Exists( folderpath );

            // check to call plugin opened function
            if ( UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ParsingProvider.ToString(), "", out var plugin ) &&
                 UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ParsingProvideFunc.ToString(), "", out var pluginFunc ) &&
                 UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ParsingFilename.ToString(), "", out var parseFilename ) &&
                 folderReady &&
                 !string.IsNullOrEmpty(plugin) &&
                 !string.IsNullOrEmpty(pluginFunc) &&
                 !string.IsNullOrEmpty(parseFilename) )
            {
                var callStatus = ULibAgent.Singleton.AssemblyPlugins.CallPluginClassFuncByClassFullName( plugin, pluginFunc, out var funcR,
                    UDataCarrier.MakeOne( dic, "dic to op" ),
                    UDataCarrier.MakeOne( folderpath, "folder path" ),
                    UDataCarrier.MakeOne( Path.Combine( folderpath, parseFilename ), "parsing file full path" ),
                    UDataCarrier.MakeOne( ImageFromMethodMutableDataKey.LoadedFilePaths.ToString(), "key: loaded file path" ),
                    UDataCarrier.MakeOne( ImageFromMethodMutableDataKey.LoadedFileTimestamps.ToString(), "key: file time stamp" ) );
                if ( callStatus )
                {
                    if ( funcR != null ) UDataCarrier.Set( dic, ImageFromMethodMutableDataKey.ExtraData.ToString(), funcR );
                    UDataCarrier.Set( dic, ImageFromMethodMutableDataKey.NextIndex.ToString(), 0 ); // reset to 0
                }
            }
            // folder ready
            else if ( folderReady )
            {
                var srchPat = UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_SearchPattern.ToString(), "*.*" );
                ReloadFolder( dic, folderpath, srchPat, ImageFromMethodMutableDataKey.LoadedFilePaths.ToString(), ImageFromMethodMutableDataKey.NextIndex.ToString() );
            }

            // config jump index
            if ( UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), -1, out var jindex ) &&
                m is UMacroCapableOfCtrlFlow fcm && fcm != null )
            {
                //fcm.MustJump = jindex >= 0;
                fcm.Jump2WhichMacro = jindex;
            }
        }

        /// <summary>
        /// popup form
        /// </summary>
        /// <param name="callMethodName"></param>
        /// <param name="macroToConf"></param>
        /// <returns></returns>
        private Form PopupConf_ImageFrom( string callMethodName, UMacro macroToConf )
        {
            if ( callMethodName == ImageFromBufferMethodName )
            {
                return new FormBufferLoaderSetup() { WorkWith = macroToConf as UMacroCapableOfCtrlFlow, Text = $"{callMethodName} setup" }.UpdateToUI();
            }
            else if (callMethodName == ImageFromFolderMethodName)
            {
                return new FormFolderLoaderSetup() { WorkWith = macroToConf as UMacroCapableOfCtrlFlow, Text = $"{callMethodName} setup" }.UpdateToUI();
            }

            return null;
        }


        private UDataCarrier[] InputFromBufferCall( UMacro MacroInstance,
                                                    UDataCarrier[] PrevPropagationCarrier,
                                                    List<UMacroProduceCarrierResult> historyResultCarriers,
                                                    List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
                                                    List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
                                                    List<UScriptHistoryCarrier> historyCarrier,
                                                ref bool bStatusCode, ref string strStatusMessage,
                                                ref UDataCarrier[] CurrPropagationCarrier,
                                                ref UDrawingCarriers CurrDrawingCarriers,
                                                ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
                                                ref fpUDataCarrierSetResHandler ResultCarrierHandler )
        {
            if (!(MacroInstance is UMacroCapableOfCtrlFlow jpm))
            {
                strStatusMessage = "macro instance type error";
                return null;
            }
            if (!UDataCarrier.Get<Dictionary<string, UDataCarrier>>(MacroInstance.MutableInitialData, null, out var dic))
            {
                strStatusMessage = "macro mutable data error";
                return null;
            }

            // config default not jump
            jpm.MustJump = false;

            if ( !UDataCarrier.TypesCheck( PrevPropagationCarrier, KeepImageFromBufferMethodMacro.PrevPropagationParamTypeDesc ) )
            {
                strStatusMessage = "prev propagation type error";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }

            if ( !UDataCarrier.GetDicKeyStrOne<UImageComBuffer>( MacroInstance.MutableInitialData, ImageFromMethodMutableDataKey.ImageInstance.ToString(), null, out var buff ) ||
                 buff == null )
            {
                strStatusMessage = "buffer instance not ready";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }

            if ( !UDataCarrier.GetByIndex( PrevPropagationCarrier, 0, IntPtr.Zero, out var pAddr ) || pAddr == IntPtr.Zero )
            {
                strStatusMessage = "no input buffer pointer";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }
            if ( !UDataCarrier.GetByIndex( PrevPropagationCarrier, 1, 0, out var imgW ) || imgW <= 0 )
            {
                strStatusMessage = "no buffer width";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }
            if ( !UDataCarrier.GetByIndex( PrevPropagationCarrier, 2, 0, out var imgH ) || imgH <= 0 )
            {
                strStatusMessage = "no buffer height";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }
            if ( !UDataCarrier.GetByIndex( PrevPropagationCarrier, 3, 0, out var pixBits ) || (pixBits % 8) != 0)
            {
                strStatusMessage = $"invalid pixel bits {pixBits}";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }
            UDataCarrier.GetByIndex( PrevPropagationCarrier, 4, 0, out var stride );
            stride = stride == 0 ? imgW * pixBits / 8 : stride;

            if (!buff.Copy( pAddr, imgW, imgH, pixBits, stride ))
            {
                strStatusMessage = "copy buffer fail";
                if ( jpm != null && jpm.Jump2WhichMacro > 0 ) goto InputFromBufferCall_err_goto;
                return null;
            }

            // convert bmp to draw
            if ( CurrDrawingCarriers == null )
                CurrDrawingCarriers = new UDrawingCarriers( buff.Width, buff.Height );
            UDrawingCarrierBitmaps bmpsC = new UDrawingCarrierBitmaps();
            bmpsC.AddBmp( 0, 0, buff.ToBitmap(), true );
            CurrDrawingCarriers.Add( bmpsC );


            bStatusCode = true;
            var propagation = new UDataCarrier[]
            {
                UDataCarrier.MakeOne(buff.Buffer),
                UDataCarrier.MakeOne(buff.Width),
                UDataCarrier.MakeOne(buff.Height),
                UDataCarrier.MakeOne(buff.Bits),
                UDataCarrier.MakeOne(stride),
                UDataCarrier.MakeOne(DateTime.Now, "timestamp")
            };
            CurrPropagationCarrier = propagation;
            return propagation;

        InputFromBufferCall_err_goto:
            var repo = new UDataCarrier[]
            {
                UDataCarrier.MakeOne( false, "status"),
                UDataCarrier.MakeOne(strStatusMessage, "message")
            };
            // enable jump
            jpm.MustJump = true;
            CurrPropagationCarrier = repo;
            bStatusCode = true;
            return repo;
        }

        private UDataCarrier[] InputFromFolderCall( UMacro MacroInstance,
                                                     UDataCarrier[] PrevPropagationCarrier,
                                                     List<UMacroProduceCarrierResult> historyResultCarriers,
                                                     List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
                                                     List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
                                                     List<UScriptHistoryCarrier> historyCarrier,
                                                 ref bool bStatusCode, ref string strStatusMessage,
                                                 ref UDataCarrier[] CurrPropagationCarrier,
                                                 ref UDrawingCarriers CurrDrawingCarriers,
                                                 ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
                                                 ref fpUDataCarrierSetResHandler ResultCarrierHandler )
        {
            if (!(MacroInstance is UMacroCapableOfCtrlFlow jpm))
            {
                strStatusMessage = "invalid macro type";
                return null;
            }
            // make default not jump
            jpm.MustJump = false;

            // get data from mutable internal erro to end
            if (!UDataCarrier.Get<Dictionary<string, UDataCarrier>>(MacroInstance.MutableInitialData, null, out var dic))
            {
                strStatusMessage = "invalid mutable data";
                return null;
            }

            // check prepropagation contain folder path
            if ( UDataCarrier.GetByIndex( PrevPropagationCarrier, 0, "", out var folderpath ) && Directory.Exists( folderpath ) )
            {
                if ( UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ParsingProvider.ToString(), "", out var plugin ) &&
                     UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ParsingProvideFunc.ToString(), "", out var pluginFunc ) &&
                     UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_ParsingFilename.ToString(), "", out var parseFilename ) &&
                     !string.IsNullOrEmpty(plugin) &&
                     !string.IsNullOrEmpty(pluginFunc) )
                {
                    // call plugin opened function to load file(s) from a path
                    // - such as ini file
                    var callStatus = ULibAgent.Singleton.AssemblyPlugins.CallPluginClassFuncByClassFullName( plugin, pluginFunc, out var funcR,
                        UDataCarrier.MakeOne( dic, "dic to op" ), // [0]
                        UDataCarrier.MakeOne( folderpath, "folder path" ), // [1]
                        UDataCarrier.MakeOne( Path.Combine( folderpath, parseFilename ), "parsing file full path" ), // [2]: contain a description file to load file(s) from
                        UDataCarrier.MakeOne( ImageFromMethodMutableDataKey.LoadedFilePaths.ToString(), "key: loaded file path" ), // [3]: which key to store loaded file path
                        UDataCarrier.MakeOne( ImageFromMethodMutableDataKey.LoadedFileTimestamps.ToString(), "key: loaded file timestamp" ) // [4]: which key to store loaded file timestamp Dictionary<string, DateTime>: key file path, value time stamp
                        );
                    if ( callStatus )
                    {
                        if ( funcR != null ) UDataCarrier.Set( dic, ImageFromMethodMutableDataKey.ExtraData.ToString(), funcR );
                        UDataCarrier.Set( dic, ImageFromMethodMutableDataKey.NextIndex.ToString(), 0 ); // reset to 0
                    }
                    else
                    {
                        // call to pluning class opened function failure, should not happen, return fail
                        var strE = $"Call {plugin}::{pluginFunc} to restore settings to {MacroInstance.MethodName} fail";
                        fpLog?.Invoke( eLogMessageType.NORMAL, 0, strE );

                        strStatusMessage = strE;
                        return null;

                        /*
                        // no way to jump
                        if (jpm.Jump2WhichMacro < 0)
                        {
                            strStatusMessage = strE;
                            return null;
                        }

                        // config jump
                        var repo = MakeLoadingFolderReport( false, strE, folderpath );
                        CurrPropagationCarrier = repo;
                        bStatusCode = true;
                        return repo;
                        */
                    }
                }
                else
                {
                    // reload
                    ReloadFolder(
                        dic, folderpath,
                        UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_SearchPattern.ToString(), "*.*" ),
                        ImageFromMethodMutableDataKey.LoadedFilePaths.ToString(),
                        ImageFromMethodMutableDataKey.NextIndex.ToString()
                    );
                }
            }

            // no next index found, internal error to end
            if (!UDataCarrier.Get(dic, ImageFromMethodMutableDataKey.NextIndex.ToString(), -1, out var nextI) || nextI < 0)
            {
                strStatusMessage = "invalid next index";
                return null;
            }

            UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.LoadedFilePaths.ToString(), new string[0], out var filepaths );
            UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_IsIncIndex.ToString(), true, out var bInc );
            UDataCarrier.Get( dic, ImageFromMethodMutableDataKey.Param_EnableCycleRun.ToString(), false, out var bCycRun );
            UDataCarrier.Get<UImageComBuffer>( dic, ImageFromMethodMutableDataKey.ImageInstance.ToString(), null, out var imageBuff );
            UDataCarrier.Get(dic, ImageFromMethodMutableDataKey.LoadedFileTimestamps.ToString(), new Dictionary<string, DateTime>(), out var fileTimestamps );

            // buffer invalid, internal erro to end
            if (imageBuff == null)
            {
                strStatusMessage = "not alloc image buffer";
                return null;
            }

            // index check: check config jump
            if (nextI >= filepaths.Length || nextI < 0)
            {
                if ( jpm.Jump2WhichMacro < 0 )
                {
                    strStatusMessage = $"invalid next index({nextI}) in range 0~{filepaths.Length - 1}";
                    return null;
                }

                // config jump
                var repo = MakeLoadingFolderReport( false, "", fileTimestamps, folderpath, nextI, filepaths.Length, filepaths );
                jpm.MustJump = true;
                CurrPropagationCarrier = repo;
                bStatusCode = true;
                return repo;
            }

            // try to load image
            var currI = nextI;
            Bitmap bmp = null;
            try
            {
                using ( var s = File.Open( filepaths[ currI ], FileMode.Open, FileAccess.Read ) )
                {
                    bmp = new Bitmap( s );
                }
            }
            catch
            {
                bmp?.Dispose();
                bmp = null;
            }

            // invalid file
            var errS = "";
            if ( bmp == null)
            {
                errS = $"Invalid image file; Load {filepaths[ currI ]} fail";
                if (jpm.Jump2WhichMacro < 0)
                {
                    strStatusMessage = errS;
                    return null;
                }
            }

            // config index
            bool reachEnd = false;
            if (bCycRun)
            {
                if ( bInc )
                    nextI = currI + 1 >= filepaths.Length ? 0 : currI + 1;
                else
                    nextI = currI - 1 < 0 ? filepaths.Length - 1 : currI - 1;
            }
            else
            {
                if ( bInc )
                {
                    reachEnd = currI + 1 >= filepaths.Length;
                    nextI = currI + 1 >= filepaths.Length ? currI : currI + 1;
                }
                else
                {
                    reachEnd = currI - 1 < 0;
                    nextI = currI - 1 < 0 ? currI : currI - 1;
                }
            }
            // write back
            dic[ ImageFromMethodMutableDataKey.NextIndex.ToString() ].Data = nextI;

            // config jump
            if (bmp == null)
            {
                var repo = MakeLoadingFolderReport( false, errS, fileTimestamps, folderpath, currI, filepaths.Length, filepaths );
                jpm.MustJump = true;
                CurrPropagationCarrier = repo;
                bStatusCode = true;
                return repo;
            }

            // copy into buffer
            var bCopyStatus = imageBuff.FromBitmap( bmp );
            if ( CurrDrawingCarriers == null )
                CurrDrawingCarriers = new UDrawingCarriers( bmp.Width, bmp.Height );
            UDrawingCarrierBitmaps bmpsC = new UDrawingCarrierBitmaps();
            bmpsC.AddBmp( 0, 0, bmp, true );
            CurrDrawingCarriers.Add( bmpsC );

            // copy fail
            if (!bCopyStatus)
            {
                errS = $"Invalid pixel bits; pixel bits = {filepaths[currI]}";
                if (jpm.Jump2WhichMacro < 0)
                {
                    strStatusMessage = errS;
                    return null;
                }
                // config jump
                var repo = MakeLoadingFolderReport( false, errS, fileTimestamps, folderpath, currI, filepaths.Length, filepaths );
                jpm.MustJump = true;
                CurrPropagationCarrier = repo;
                bStatusCode = true;
                return repo;
            }

            var filepath = filepaths[ currI ];
            var filetimestamp = fileTimestamps.ContainsKey( filepath ) ? fileTimestamps[ filepath ] : File.GetLastWriteTime( filepath );
            var fin = UDataCarrier.MakeVariableItemsArray( imageBuff.Buffer, imageBuff.Width, imageBuff.Height, imageBuff.Bits, imageBuff.Stride, filetimestamp );
            CurrPropagationCarrier = fin;
            bStatusCode = true;
            return new UDataCarrier[]
            {
                UDataCarrier.MakeOne( imageBuff.Buffer, "loaded image buffer pointer"),
                UDataCarrier.MakeOne(imageBuff.Width, "loaded image width"),
                UDataCarrier.MakeOne(imageBuff.Height, "loaded image height"),
                UDataCarrier.MakeOne(imageBuff.Bits, "loaded image pixel bits"),
                UDataCarrier.MakeOne(imageBuff.Stride, "loaded image buffer stride"),
                UDataCarrier.MakeOne(fileTimestamps, "loaded files timestamp"), // must: access info from this desc
                UDataCarrier.MakeOne(filetimestamp, "loaded file timestamp"),
                UDataCarrier.MakeOne(filepath, "loaded file path"),
                UDataCarrier.MakeOne(reachEnd, "loading reach end")
            };
        }

        static UDataCarrier[] MakeLoadingFolderReport(bool status, string msg, Dictionary<string, DateTime> filesTimestamp, string folderpath = "", int currIndex = -1, int totalFiles = 0, string[] filepaths = null)
        {
            return new UDataCarrier[]
            {
                UDataCarrier.MakeOne(status, "status"),
                UDataCarrier.MakeOne(msg, "message"),
                UDataCarrier.MakeOne(filesTimestamp, "loaded files timestamp"), // must: access info from this desc
                UDataCarrier.MakeOne(folderpath, "folder path"),
                UDataCarrier.MakeOne(currIndex, "current index"),
                UDataCarrier.MakeOne(totalFiles, "total files"),
                UDataCarrier.MakeOne(filepaths == null ? new string[0] : filepaths, "file paths")
            };
        }

    }
}
