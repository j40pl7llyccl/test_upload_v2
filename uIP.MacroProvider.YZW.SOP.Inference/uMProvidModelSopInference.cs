using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Inference
{
    public class uMProvidModelSopInference : UMacroMethodProviderPlugin
    {
        const string ModePathNodeName = "model_path";

        const string ModelInferenceMethodName = "Inf_ModelInference";
        private UMacro PredefinedOpenedInferenceMethod { get; set; } = null;
        private string PluginRWDir { get; set; }
        internal string DataRWDir { get; set; }

        public uMProvidModelSopInference() : base()
        {
            m_strInternalGivenName = "YZW_SOP_Inference";
        }

        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            // Create a rw path
            PluginRWDir = UDataCarrier.GetItem( param, 0, "", out var dummy );
            if ( Directory.Exists( PluginRWDir ) )
            {
                DataRWDir = CommonUtilities.RCreateDir2( Path.Combine( PluginRWDir, "YZW_SOP_INF" ) );
            }

            // describe macro
            PredefinedOpenedInferenceMethod = new UMacro( null, NameOfCSharpDefClass, ModelInferenceMethodName, ModelInference,
                    null, null,
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription(typeof(IntPtr), "Image buffer pointer"),
                        new UDataCarrierTypeDescription(typeof(int), "Image width"),
                        new UDataCarrierTypeDescription(typeof(int), "Image height"),
                        new UDataCarrierTypeDescription(typeof(int), "Image pixel bits")
                    },
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription(typeof(IntPtr), "Image buffer pointer"),
                        new UDataCarrierTypeDescription(typeof(int), "Image width"),
                        new UDataCarrierTypeDescription(typeof(int), "Image height"),
                        new UDataCarrierTypeDescription(typeof(int), "Image pixel bits"),
                        new UDataCarrierTypeDescription(typeof(int), "Found object count"),
                        new UDataCarrierTypeDescription(typeof(float[]), "Found objects;Arranged items X1,Y1,W1,H1,Confidence1,Label1,X2,Y2,W2,H2,Confidence2,Label2,...")
                    },
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription(typeof(int[]), "ROI X(Left)"),
                        new UDataCarrierTypeDescription(typeof(int[]), "ROI Y(Top)"),
                        new UDataCarrierTypeDescription(typeof(int[]), "ROI Width"),
                        new UDataCarrierTypeDescription(typeof(int[]), "ROI Height"),
                        new UDataCarrierTypeDescription(typeof(float[]), "Confidence"),
                        new UDataCarrierTypeDescription(typeof(int[]), "Label")
                    }
                );

            m_UserQueryOpenedMethods.Add( PredefinedOpenedInferenceMethod );

            // create macro done call
            m_createMacroDoneFromMethod.Add( ModelInferenceMethodName, MacroShellDoneCall );

            // parameter get/ set
            // - 將 model data中欄位可以讀寫與儲存
            // - 注意要將 model 儲存石方入壓縮的資料夾中
            // - 注意教導model時要先解密
            m_MacroControls.Add( "input_image_info",
                new UScriptControlCarrierMacro( "input_image_info", true, true, true,
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription( typeof( int ), "Input image width" ),
                        new UDataCarrierTypeDescription( typeof( int ), "Input image height" ),
                        new UDataCarrierTypeDescription( typeof( int ), "Input image channels" )
                    },
                    IoctrlGet_InputImage, IoctrlSet_InputImage )
            );
            m_MacroControls.Add( "input_frame_info",
                new UScriptControlCarrierMacro( "input_frame_info", true, true, true,
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription( typeof( int ), "Input frame width" ),
                        new UDataCarrierTypeDescription( typeof( int ), "Input frame height" ),
                        new UDataCarrierTypeDescription( typeof( int ), "Input frame channels" )
                    },
                    IoctrlGet_InputFrame, IoctrlSet_InputFrame )
            );
            m_MacroControls.Add( "output_data",
                new UScriptControlCarrierMacro( "output_data", true, true, true,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription( typeof( int ), "Output batch no." ),
                        new UDataCarrierTypeDescription( typeof( int ), "Output block size" ),
                        new UDataCarrierTypeDescription( typeof( int ), "Output block no." )
                    },
                    IoctrlGet_OutputData, IoctrlSet_OutputData )
            );
            m_MacroControls.Add( "network",
                new UScriptControlCarrierMacro( "network", true, true, true,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription( typeof( string ), "Netowrk input"),
                        new UDataCarrierTypeDescription( typeof( string ), "Network output" ),
                    },
                    IoctrlGet_Network, IoctrlSet_Network )
            );
            m_MacroControls.Add( "post_threshold",
                new UScriptControlCarrierMacro( "post_threshold", true, true, true,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription( typeof( double ), "Post box confidence"),
                        new UDataCarrierTypeDescription( typeof( double ), "Post NMS threshold" ),
                    },
                    IoctrlGet_PostThreshold, IoctrlSet_PostThreshold )
            );


            // popup GUI
            m_macroMethodConfigPopup.Add( ModelInferenceMethodName, PopupConf );

            m_bOpened = true;
            return true;

        }

        private bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == ModelInferenceMethodName )
            {
                // create variable to store and mark as handleable
                instance.MutableInitialData = UDataCarrier.MakeOne( new ModelData(), true );
            }
            return true;
        }

        internal static bool SetModelPath(UMacro macro, string srcPath)
        {
            if ( macro.OwnerOfPluginClass == null || !( macro.OwnerOfPluginClass is uMProvidModelSopInference inf ) || inf == null ) return false;
            var ownerS = macro.OwnerOfScript?.NameOfId ?? "";
            if ( string.IsNullOrEmpty( ownerS ) )
                ownerS = macro.OwnerOfScript?.SnOfId.ToString() ?? "";
            if ( macro.MutableInitialData == null )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[SetModelPath] Script-{ownerS} of {macro.MethodName} config without mutable data" );
                return false;
            }
            if ( !( macro.MutableInitialData.Data is ModelData md ) || md == null )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[SetModelPath] Script-{ownerS} of {macro.MethodName} cannot convert mutable data to ModuleData" );
                return false;
            }
            // check source file exist?
            if (!File.Exists(srcPath))
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[SetModelPath] Script-{ownerS} of {macro.MethodName} source model file {srcPath} not exist" );
                return false;
            }
            // check current
            var curRWDir = "";
            if ( !string.IsNullOrEmpty( md.EncryptModelFilepath ) && File.Exists( md.EncryptModelFilepath ) )
            {
                curRWDir = Path.GetDirectoryName( md.EncryptModelFilepath );
            }
            // RW dir not exist create one
            if (string.IsNullOrEmpty(curRWDir))
            {
                curRWDir = Path.Combine( inf.DataRWDir, CommonUtilities.GetCurrentTimeStr("") );
                curRWDir = CommonUtilities.RCreateDir2( curRWDir );
            }
            if (string.IsNullOrEmpty(curRWDir))
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[SetModelPath] Script-{ownerS} of {macro.MethodName} create dir {curRWDir} fail" );
                return false;
            }
            // config to not teach
            md.IsModelTaught = false;
            // copy or encrypt to target
            var prevFilepath = md.EncryptModelFilepath;
            var currFilepath = Path.Combine( curRWDir, $"{Path.GetFileNameWithoutExtension(srcPath)}.bin" );

            if ( FileEncryptUtility.Check( srcPath ) )
            {
                try
                {
                    File.Copy(srcPath, currFilepath, true );
                }
                catch { }
            }
            else
            {
                FileEncryptUtility.ENC( srcPath, currFilepath );
            }
            // remove previous
            if ( File.Exists(prevFilepath) && prevFilepath.ToLower() != currFilepath.ToLower() )
            {
                try
                {
                    File.Delete( prevFilepath );
                }
                catch { }
            }

            // update current model path
            md.EncryptModelFilepath = currFilepath;
            return true;
        }

        internal static bool TeachModel(UMacro macro, out string errReason)
        {
            errReason = "";
            if ( macro.OwnerOfPluginClass == null || !( macro.OwnerOfPluginClass is uMProvidModelSopInference inf ) || inf == null ) return false;
            var ownerS = macro.OwnerOfScript?.NameOfId ?? "";
            if ( string.IsNullOrEmpty( ownerS ) )
                ownerS = macro.OwnerOfScript?.SnOfId.ToString() ?? "";
            if (macro.MutableInitialData == null)
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} config without mutable data" );
                return false;
            }
            if (!(macro.MutableInitialData.Data is ModelData md) || md == null)
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} cannot convert mutable data to ModuleData" );
                return false;
            }
            // return model ready
            if ( md.IsModelTaught )
                return true;
            // check model file exist
            if (!File.Exists(md.EncryptModelFilepath))
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} model file {md.EncryptModelFilepath} not exist" );
                return false;
            }
            // decrypt model file
            var decryptedFilepath = Path.Combine( Path.GetDirectoryName( md.EncryptModelFilepath ), $"{Path.GetFileNameWithoutExtension(md.EncryptModelFilepath)}_{CommonUtilities.GetCurrentTimeStr("")}" );
            if (!FileEncryptUtility.DEC(md.EncryptModelFilepath, decryptedFilepath) )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} model file {md.EncryptModelFilepath} decrypt fail" );
                return false;
            }
#if !NOT_IMPORT_DLL
            // if handle not read, create one
            if ( md.Handle == IntPtr.Zero )
            {
                md.Handle = ImportDllOpenedFunctions.model_inference_create();
                if (md.Handle == IntPtr.Zero)
                {
                    inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call model_inference_create fail" );
                    errReason = $"call model_inference_create fail";
                    goto TeachModel_ERR;
                }
            }
            // call to load
            int retCode = ImportDllOpenedFunctions.load_model( md.Handle, decryptedFilepath );
            if ( retCode != 0 )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call load_model fail with code={retCode}");
                errReason = $"call load_model fail with code={retCode}";
                goto TeachModel_ERR;
            }
            // call to set input data
            retCode = ImportDllOpenedFunctions.set_model_input_data( md.Handle, md.InputImgResolutionW, md.InputImgResolutionH, md.InputImgChannels );
            if (retCode != 0)
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call set_model_input_data fail with code={retCode}" );
                errReason = $"call set_model_input_data fail with code={retCode}";
                goto TeachModel_ERR;
            }
            // call to set frame property
            retCode = ImportDllOpenedFunctions.set_frame_property( md.Handle, md.FrameResolutionW, md.FrameResolutionH, md.FrameImgChannels );
            if ( retCode != 0 )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call set_frame_property fail with code={retCode}" );
                errReason = $"call set_frame_property fail with code={retCode}";
                goto TeachModel_ERR;
            }
            // call to set output data
            retCode = ImportDllOpenedFunctions.set_model_output_data( md.Handle, md.OutputBatchNo, md.OutputBlockSize, md.OutputBlockNo );
            if ( retCode != 0 )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call set_model_output_data fail with code={retCode}" );
                errReason = $"call set_model_output_data fail with code={retCode}";
                goto TeachModel_ERR;
            }
            // call to set network
            retCode = ImportDllOpenedFunctions.set_network_address(md.Handle, md.NetworkInput, md.NetworkOutput );
            if ( retCode != 0 )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call set_network_address fail with code={retCode}" );
                errReason = $"call set_network_address fail with code={retCode}";
                goto TeachModel_ERR;
            }
            // call to set post process
            retCode = ImportDllOpenedFunctions.set_postprocess_thresholds( md.Handle, ( float )md.PostProcBoxConfidence, ( float )md.PostProcNMSThreshold );
            if ( retCode != 0 )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call set_postprocess_thresholds fail with code={retCode}" );
                errReason = $"call set_postprocess_thresholds fail with code={retCode}";
                goto TeachModel_ERR;
            }
            // call to create model
            retCode = ImportDllOpenedFunctions.create_model( md.Handle );
            if ( retCode != 0 )
            {
                inf.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[TeachModel] Script-{ownerS} of {macro.MethodName} call create_model fail with code={retCode}" );
                errReason = $"call create_model fail with code={retCode}";
                goto TeachModel_ERR;
            }
#endif
            // delete decrypt model
            try
            {
                File.Delete( decryptedFilepath );
            }
            catch { }

            // set to taught
            md.IsModelTaught = true;

            return true;

TeachModel_ERR:
            try
            {
                File.Delete( decryptedFilepath );
            }
            catch { }
            return false;
        }

        /// <summary>
        /// all parameters must be ready except intptr and model path
        /// - new handler
        /// - encrypt model file
        /// </summary>
        /// <param name="macro">macro to config</param>
        /// <param name="srcModelPath">model source path</param>
        /// <returns></returns>
        private bool InitModelInference(UMacro macro, string srcModelPath)
        {
            if (macro == null)
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call InitModelInference with macro instance null" );
                return false;
            }
            if (!(macro.MutableInitialData.Data is ModelData md))
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call InitModelInference with mutable data type({macro.MutableInitialData.Tp.FullName}) invalid" );
                return false;
            }
            if ( md == null )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call InitModelInference with mutable data null" );
                return false;
            }

            IntPtr handle = IntPtr.Zero;
            var createdTmpDir = "";
            var encModelPath = "";
            //
            // check
            //
            if (!File.Exists(srcModelPath))
            {
                fpLog?.Invoke( eLogMessageType.ERROR, 0, $"{m_strInternalGivenName} initialize with invalid model path {srcModelPath}" );
                return false;
            }
            if (macro == null)
            {
                fpLog?.Invoke( eLogMessageType.ERROR, 0, $"{m_strInternalGivenName} initialize with null macro instance" );
                return false;
            }

            //
            // create temporal dir
            //
            var tmpDir = Path.Combine( DataRWDir, CommonUtilities.GetCurrentTimeStr( "" ) );
            var newTmpDir = CommonUtilities.RCreateDir2( tmpDir );
            if (string.IsNullOrEmpty( newTmpDir ) )
            {
                fpLog?.Invoke( eLogMessageType.ERROR, 0, $"{m_strInternalGivenName} cannot create temporal path{newTmpDir} in init." );
                return false;
            }
            createdTmpDir = newTmpDir;
            // check and copy model
            var isEncrypted = false;
            var status = true;
            try
            {
                long fsz = new FileInfo(srcModelPath ).Length;
                using ( var fs = File.Open( srcModelPath, FileMode.Open, FileAccess.Read ) )
                {
                    isEncrypted = FileEncryptUtility.Check( fs, fsz );
                }
                encModelPath = Path.Combine( createdTmpDir, $"{Path.GetFileNameWithoutExtension(srcModelPath)}.bin" );
                if ( !isEncrypted )
                    status = FileEncryptUtility.ENC( srcModelPath, encModelPath );
                else
                    File.Copy( srcModelPath, encModelPath );
            }
            catch (Exception e)
            {
                fpLog?.Invoke( eLogMessageType.ERROR, 0, $"{m_strInternalGivenName} init process model file with error {e}" );
                status = false;
            }
            if ( !status )
                goto InitModelInference_Error;

#if !NOT_IMPORT_DLL
            //
            // create a handle
            //
            handle = ImportDllOpenedFunctions.model_inference_create();
            if (handle != IntPtr.Zero)
            {
                // free previous
                if ( md.Handle != IntPtr.Zero ) ImportDllOpenedFunctions.model_inference_release( md.Handle );
                // assign new
                md.Handle = handle;
                // assign enc file path
                md.EncryptModelFilepath = encModelPath;
                // config to not taught
                md.IsModelTaught = false;
                return true;
            }
#else
            // assign enc file path
            md.EncryptModelFilepath = encModelPath;
            // config to not taught
            md.IsModelTaught = false;
            return true;
#endif

        InitModelInference_Error:
#if !NOT_IMPORT_DLL
            if ( handle != IntPtr.Zero )
                ImportDllOpenedFunctions.model_inference_release( handle );
#endif
            try
            {
                if ( Directory.Exists( createdTmpDir ) )
                    Directory.Delete( createdTmpDir, true );
            }
            catch { }
            return false;
        }

        public override bool ReadMacroSettings( ref UMacro macro, string pathOfFolder, string cfgFile )
        {
            //
            // Read setting from xml file (Load settings)
            // 1. call base to read settings
            // 2. read model path info from xml
            // 3. copy and keep encrypted model to tmp folder
            // 4. teach the model
            //
            if ( !base.ReadMacroSettings( ref macro, pathOfFolder, cfgFile ) )
                return false;

            //
            // can use macro method name to distinguish
            //
            // only one opened method no need check by name
            // 

            var confFullPath = Path.Combine( pathOfFolder, cfgFile );
            if ( !ReadFromParameterNode( confFullPath, out var got, ModePathNodeName ) || got.Count <= 0 )
                return false;

            var modpath = Path.Combine( pathOfFolder, got[ ModePathNodeName ] );
            // init model data
            if ( !InitModelInference( macro, modpath ) )
                return false;
            // call to config
            if ( !TeachModel( macro, out var dummy ) )
                return false;

            return true;
        }
        public override bool WriteMacroSettings( UMacro macro, string pathOfFolder, string cfgFile )
        {
            // Write settings (save settings)
            // 1. call base to save settings
            // 2. write model path info to xml
            // 3. copy encrypted model to folder and zip later
            //
            if (macro == null)
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with macro instance null" );
                return false;
            }
            if (macro.MutableInitialData == null)
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with mutable data null" );
                return false;
            }
            if (!(macro.MutableInitialData.Data is ModelData md))
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with mutable data type({macro.MutableInitialData.Tp.FullName}) invalid" );
                return false;
            }
            if (md == null)
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with mutable data null" );
                return false;
            }


            if ( !base.WriteMacroSettings( macro, pathOfFolder, cfgFile ) )
                return false;

            // can use macro method name to distinguish
            var path2Reload = Path.Combine( pathOfFolder, cfgFile );
            var encFilename = Path.GetFileName( md.EncryptModelFilepath );
            var conf = new Dictionary<string, string>
            {
                { ModePathNodeName, encFilename }
            };
            if ( WriteToParameterNode( path2Reload, conf ) )
            {
                try { File.Copy( md.EncryptModelFilepath, Path.Combine( pathOfFolder, encFilename ) ); }
                catch { return false; }
                return true;
            }

            return false;
        }

        static UDrawingCarriers MakeDrawing( int[] x, int[] y, int[] w, int[] h, float[] confidence, int[] label)
        {
            //Random rnd = new Random( Convert.ToInt32( CommonWinSdkFunctions.GetTickCount() ) );
            Random rnd = new Random();
            UDrawingCarriers ret = new UDrawingCarriers();
            Dictionary<int, Color> cm = new Dictionary<int, Color>();
            Dictionary<int, UDrawingCarrierRect2d> rects = new Dictionary<int, UDrawingCarrierRect2d>();
            Dictionary<int, UDrawingCarrierTexts> txts = new Dictionary<int, UDrawingCarrierTexts>();

            bool err = false;

            try
            {
                for(int i = 0; i < x.Length; i++)
                {
                    if ( !cm.TryGetValue( label[i], out var color))
                    {
                        color = CommonUtilities.ColorList[ rnd.Next( 0, CommonUtilities.ColorList.Count - 1 ) ];
                        cm.Add( label[ i ], color );
                        rects.Add( label[ i ], new UDrawingCarrierRect2d() { _BorderColor = new tUDrawingCarrierRGB( color.R, color.G, color.B ), _BorderWidth = 3 } );
                        txts.Add( label[ i ], new UDrawingCarrierTexts() { _FontColor = new tUDrawingCarrierRGB( color.R, color.G, color.B ), _FontSize = 12 } );
                    }

                    var rect = rects[ label[ i ] ];
                    var txt = txts[ label[ i ] ];

                    rect.AddRect( new tUDrawingCarrierRect( x[ i ], y[ i ], x[ i ] + w[ i ], y[ i ] + h[ i ] ) );
                    txt.Add( x[ i ] + 5, y[ i ] + 5, $"{confidence[ i ]:0.00}" );
                }
                foreach ( var kv in rects )
                    ret.Add( kv.Value );
                foreach (var kv in txts )
                    ret.Add( kv.Value );
            }
            catch { err = true; }

            if (err)
            {
                rects.Clear();
                txts.Clear();
                ret.Dispose();
                return null;
            }            

            return ret;
        }

        private UDataCarrier[] ModelInference( UMacro MacroInstance,
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
            //
            // exec model inference
            //
            // describe call which function with error# in return message

            bStatusCode = false;
            // try to convert type
            if ( !UDataCarrier.Get<ModelData>( MacroInstance?.MutableInitialData ?? null, null, out var md ) )
            {
                strStatusMessage = "Mutable data type error";
                return null;
            }
            // check model taught
            if ( !md.IsModelTaught )
            {
                strStatusMessage = "model not teach yet";
                return null;
            }
            // check prev propagation
            if ( !UDataCarrier.TypesCheck( PrevPropagationCarrier, PredefinedOpenedInferenceMethod.PrevPropagationParamTypeDesc ) )
            {
                strStatusMessage = "previous propagation types not match requirement";
                return null;
            }
            // get image
            var frameBuff = UDataCarrier.GetItem( PrevPropagationCarrier, 0, IntPtr.Zero, out var s01 );
            var frameW    = UDataCarrier.GetItem( PrevPropagationCarrier, 1, 0, out var s02 );
            var frameH    = UDataCarrier.GetItem( PrevPropagationCarrier, 2, 0, out var s03 );
            var frameCHs  = UDataCarrier.GetItem( PrevPropagationCarrier, 3, 0, out var s04 ) / 8; // convert to number of bytes
            if ( !s01 || !s02 || !s03 || !s04 )
            {
                strStatusMessage = $"convert previous propagation fail buff({s01}), w({s02}), h({s03}, CHs({s04}))";
                return null;
            }
            if ( frameW != md.FrameResolutionW || frameH != md.FrameResolutionH || frameCHs != md.FrameImgChannels )
            {
                strStatusMessage = $"Run frame not match config; Run({frameW}, {frameH}, {frameCHs}) != Conf({md.FrameResolutionW}, {md.FrameResolutionH}, {md.FrameImgChannels})";
                return null;
            }
            // call to inference
#if !NOT_IMPORT_DLL
            int retCode = 0;
            retCode = ImportDllOpenedFunctions.image_data( md.Handle, frameBuff );
            if (retCode != 0)
            {
                strStatusMessage = $"Call image_data with error code({retCode})";
                return null;
            }

            retCode = retCode = ImportDllOpenedFunctions.model_inference( md.Handle );
            if (retCode != 0)
            {
                strStatusMessage = $"Call model_inference with error code({retCode})";
                return null;
            }

            int unit = 6;
            int count = ImportDllOpenedFunctions.get_detection_size( md.Handle ) * unit;
            float[] store = new float[ count ];
            retCode = ImportDllOpenedFunctions.get_model_output( md.Handle, store );
            if ( retCode != 0)
            {
                strStatusMessage = $"Call get_model_output with error code({retCode})";
                return null;
            }

            // arrange result
            int nResult = count / unit;
            int[] coorX = new int[ nResult ];
            int[] coorY = new int[ nResult ];
            int[] width = new int[ nResult ];
            int[] height = new int[ nResult ];
            float[] confidence = new float[ nResult ];
            int[] label = new int[ nResult ];
            int offset = 0;
            for(int i = 0; i < nResult; i++, offset += unit)
            {
                int inner = 0;
                coorX[ i ] = Convert.ToInt32( store[ offset + inner++ ] );
                coorY[ i ] = Convert.ToInt32( store[ offset + inner++ ] );
                width[ i ] = Convert.ToInt32( store[ offset + inner++ ] );
                height[ i ] = Convert.ToInt32( store[ offset + inner++ ] );
                confidence[ i ] = store[ offset + inner++ ];
                label[ i ] = Convert.ToInt32( store[ offset + inner++ ] );
            }
            var repo = UDataCarrier.MakeVariableItemsArray( coorX, coorY, width, height, confidence, label );
            CurrDrawingCarriers = MakeDrawing( coorX, coorY, width, height, confidence, label );

            // gen for propagation
            CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray(frameBuff, frameW, frameH, frameCHs, nResult, store);

            bStatusCode = true;
            return repo;
#else
            bStatusCode = true;
            return null;
#endif
        }

        #region Parameter GET/ SET

        #region Input Image
        private UDataCarrier[] IoctrlGet_InputImage( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return null;

            bRetStatus = true;
            return UDataCarrier.MakeVariableItemsArray( md.InputImgResolutionW, md.InputImgResolutionH, md.InputImgChannels );
        }
        private bool IoctrlSet_InputImage( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return false;
            if ( !UDataCarrier.TypesCheck( data, carrier.DataTypes ) )
                return false;

            md.InputImgResolutionW = UDataCarrier.GetItem( data, 0, md.InputImgResolutionW, out var s01 );
            md.InputImgResolutionH = UDataCarrier.GetItem( data, 1, md.InputImgResolutionH, out var s02 );
            md.InputImgChannels = UDataCarrier.GetItem(data, 2, md.InputImgChannels, out var s03 );

            return s01 && s02 && s03;
        }

        #endregion

        #region Frame info
        private UDataCarrier[] IoctrlGet_InputFrame( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return null;

            bRetStatus = true;
            return UDataCarrier.MakeVariableItemsArray( md.FrameResolutionW, md.FrameResolutionH, md.FrameImgChannels );
        }
        private bool IoctrlSet_InputFrame( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return false;
            if ( !UDataCarrier.TypesCheck( data, carrier.DataTypes ) )
                return false;

            md.FrameResolutionW = UDataCarrier.GetItem( data, 0, md.FrameResolutionW, out var s01 );
            md.FrameResolutionH = UDataCarrier.GetItem( data, 1, md.FrameResolutionH, out var s02 );
            md.FrameImgChannels = UDataCarrier.GetItem( data, 2, md.FrameImgChannels, out var s03 );

            return s01 && s02 && s03;
        }
        #endregion

        #region Output data
        private UDataCarrier[] IoctrlGet_OutputData( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return null;

            bRetStatus = true;
            return UDataCarrier.MakeVariableItemsArray( md.OutputBatchNo, md.OutputBlockSize, md.OutputBlockNo );
        }
        private bool IoctrlSet_OutputData( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return false;
            if ( !UDataCarrier.TypesCheck( data, carrier.DataTypes ) )
                return false;

            md.OutputBatchNo = UDataCarrier.GetItem( data, 0, md.OutputBatchNo, out var s01 );
            md.OutputBlockSize = UDataCarrier.GetItem( data, 1, md.OutputBlockSize, out var s02 );
            md.OutputBlockNo = UDataCarrier.GetItem( data, 2, md.OutputBlockNo, out var s03 );

            return s01 && s02 && s03;
        }
        #endregion

        #region Network
        private UDataCarrier[] IoctrlGet_Network( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return null;

            bRetStatus = true;
            return UDataCarrier.MakeVariableItemsArray( md.NetworkInput, md.NetworkOutput );
        }
        private bool IoctrlSet_Network( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return false;
            if ( !UDataCarrier.TypesCheck( data, carrier.DataTypes ) )
                return false;

            md.NetworkInput = UDataCarrier.GetItem( data, 0, md.NetworkInput, out var s01 );
            md.NetworkOutput = UDataCarrier.GetItem( data, 1, md.NetworkOutput, out var s02 );

            return s01 && s02;
        }
        #endregion

        #region Post threshold
        private UDataCarrier[] IoctrlGet_PostThreshold( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return null;

            bRetStatus = true;
            return UDataCarrier.MakeVariableItemsArray( md.PostProcBoxConfidence, md.PostProcNMSThreshold );
        }
        private bool IoctrlSet_PostThreshold( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            var md = UDataCarrier.Get<ModelData>( whichMacro.MutableInitialData, null );
            if ( md == null )
                return false;
            if ( !UDataCarrier.TypesCheck( data, carrier.DataTypes ) )
                return false;

            md.PostProcBoxConfidence = UDataCarrier.GetItem( data, 0, md.PostProcBoxConfidence, out var s01 );
            md.PostProcNMSThreshold = UDataCarrier.GetItem( data, 1, md.PostProcNMSThreshold, out var s02 );

            return s01 && s02;
        }
        #endregion

        #endregion

        private Form PopupConf( string callMethodName, UMacro macroToConf )
        {
            Form ret = null;
            if ( callMethodName == ModelInferenceMethodName )
            {
                return new FormConfInference(macroToConf);
            }

            return ret;
        }

    }
}
