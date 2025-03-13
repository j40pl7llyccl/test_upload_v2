using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public class uMProvidSopDetect : UMacroMethodProviderPlugin
    {
        const string BackgroundImageNodeName = "Background";
        internal const string PredefinedMacroMethodNameEvaluation = "Evaluate";
        UMacro PredefinedMacroEvaluation { get; set; } = null;
        internal string PluginRWDir { get; set; }
        internal string DataRWDir { get; set; }

        internal enum ParamIoctl : int
        {
            regions,
        }

        public uMProvidSopDetect() : base()
        {
            m_strInternalGivenName = "YZW_SOP_Detection";
        }

        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            PluginRWDir = UDataCarrier.GetItem( param, 0, "", out var dummy );
            if ( Directory.Exists( PluginRWDir ) )
            {
                DataRWDir = CommonUtilities.RCreateDir2( Path.Combine( PluginRWDir, "YZW_SOP_EVAL" ) );
            }


            // descript macro
            PredefinedMacroEvaluation = new UMacro( null, NameOfCSharpDefClass, PredefinedMacroMethodNameEvaluation, Evaluate,
                null, null,
                // previous propagation
                new UDataCarrierTypeDescription[] {
                    new UDataCarrierTypeDescription(typeof(IntPtr), "Image buffer pointer"),
                    new UDataCarrierTypeDescription(typeof(int), "Image buffer width"),
                    new UDataCarrierTypeDescription(typeof(int), "Image buffer height"),
                    new UDataCarrierTypeDescription(typeof(int), "Image channels in byte count"),
                    new UDataCarrierTypeDescription(typeof(int), "Found object count"),
                    new UDataCarrierTypeDescription(typeof(float[]), "Inference results")
                },
                // current propagation
                new UDataCarrierTypeDescription[]
                {
                    new UDataCarrierTypeDescription(typeof(Dictionary<Rectangle, bool[]>), "Detection raw map: deck_ex, deck_min, pen_ex, pen_mis, pen_on_deck, pen_on_wafer, op_error"),
                    new UDataCarrierTypeDescription(typeof(Dictionary<Rectangle, List<string[]>>), "Detection error: List[0]: errors, List[1]: warnings")
                },
                // result
                new UDataCarrierTypeDescription[]
                {
                    new UDataCarrierTypeDescription(typeof(Dictionary<Rectangle, bool[]>), "Detection raw map: deck_ex, deck_min, pen_ex, pen_mis, pen_on_deck, pen_on_wafer, op_error"),
                    new UDataCarrierTypeDescription(typeof(Dictionary<Rectangle, List<string[]>>), "Detection error: List[0]: errors, List[1]: warnings")
                }
            );
            m_UserQueryOpenedMethods.Add( PredefinedMacroEvaluation );

            // create macro done call
            m_createMacroDoneFromMethod.Add( PredefinedMacroMethodNameEvaluation, MacroShellDoneCall );

            // method ioctl
            m_MacroControls.Add(
                ParamIoctl.regions.ToString(),
                new UScriptControlCarrierMacro( ParamIoctl.regions.ToString(),
                    true, true, true,
                    new UDataCarrierTypeDescription[] {
                        //new UDataCarrierTypeDescription( typeof( Dictionary<UDataCarrier, List<UDataCarrier>> ), "WorkingRegion with multiple ignore regions" )
                        new UDataCarrierTypeDescription(typeof(DetectionParameters), "Detection parameters")
                    },
                    IoctrlGet_DetectionParams,
                    IoctrlSet_DetectionParams )
            );

            // popup GUI
            m_macroMethodConfigPopup.Add( PredefinedMacroMethodNameEvaluation, PopupConf );

            m_bOpened = true;
            return true;
        }

        private bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == PredefinedMacroMethodNameEvaluation )
            {
                // create variable to store and mark as handleable
                var md = new EvaluationConf();
#if !DebugParam
                md.Handler = ImportDllOpenedFunctions.model_evaluation_create();
#endif
                instance.MutableInitialData = UDataCarrier.MakeOne( md, true );
            }
            return true;
        }

        private void ParseResult( EvaluationConf conf,  float[] r, out Dictionary<Rectangle, bool[]> oriMap, out Dictionary<Rectangle, List<string[]>> message, out bool haveError)
        {
            haveError = false;
            oriMap = new Dictionary<Rectangle, bool[]>();
            message = new Dictionary<Rectangle, List<string[]>>();

            try
            {
                int index = 0;
                while ( true )
                {
                    int x = Convert.ToInt32( r[ index ] );
                    int y = Convert.ToInt32( r[ index + 1 ] );
                    int n = Convert.ToInt32( r[ index + 2 ] );

                    var got = ( from rgn in conf.Parameters.WorkRegions where ( rgn is Rectangle rect && rect.Left == x && rect.Top == y ) select ( ( Rectangle )rgn ) ).ToArray();
                    List<bool> statusCodes = new List<bool>();
                    for( int i = index + 3; i < (index + 3 + n); i++ )
                    {
                        statusCodes.Add( r[ i ] >= 1 );
                    }
                    if (got != null && got.Length > 0)
                    {
                        if ( n < 7 )
                        {
                            oriMap.Add( got[ 0 ], statusCodes.ToArray() );
                            message.Add( got[ 0 ], null );
                            continue;
                        }

                        List<string> err = new List<string>();
                        List<string> wrn = new List<string>();
                        if ( statusCodes[ 6 ] && !statusCodes[ 5 ] ) err.Add( "Pen not close to wafer" );
                        if ( statusCodes[ 6 ] && statusCodes[ 4 ] ) err.Add( "Pen not use on wafer" );
                        if ( statusCodes[ 6 ] && statusCodes[ 5 ] ) wrn.Add( "2 wafers: one with pen the other no pen" );

                        oriMap.Add( got[ 0 ], statusCodes.ToArray() );
                        message.Add( got[ 0 ], new List<string[]> { err.ToArray(), wrn.ToArray() } );

                        if ( !haveError && (err.Count > 0 || wrn.Count > 0) )
                            haveError = true;
                    }

                    index += ( 3 + n );
                    if ( index >= r.Length )
                        break;
                }
            }
            catch { }
        }

        private UDataCarrier[] Evaluate( UMacro MacroInstance,
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
            bStatusCode = false;

            if (!UDataCarrier.Get<EvaluationConf>(MacroInstance.MutableInitialData, null, out var conf))
            {
                strStatusMessage = "mutable data invalid";
                return null;
            }
#if !DebugParam
            if (!UDataCarrier.GetByIndex(PrevPropagationCarrier, 4, 0, out var foundCount))
            {
                strStatusMessage = "cannot get inference object count";
                return null;
            }
            if (!UDataCarrier.GetByIndex(PrevPropagationCarrier, 5, new float[0], out var inferenceR ))
            {
                strStatusMessage = "cannot get inference data";
                return null;
            }

            var code = ImportDllOpenedFunctions.evaluate_detection_data( conf.Handler, foundCount, inferenceR );
            if ( code != 0)
            {
                strStatusMessage = $"call to detection with error code={code}";
                return null;
            }

            var retDataSz = ImportDllOpenedFunctions.get_estimation_size( conf.Handler );
            var toAccData = new float[retDataSz];
            code = ImportDllOpenedFunctions.get_estimation_output( conf.Handler, toAccData );
            if (code != 0)
            {
                strStatusMessage = $"call to get output with error code = {code}";
                return null;
            }

            // parse data
            ParseResult( conf, toAccData, out var detectionRaw, out var detectionMessage, out var isErr );

            // draw results
            CurrDrawingCarriers = new UDrawingCarriers();
            int offsetY = 3;
            int fontSz = 12;
            int fontOffset = 15;
            List<Rectangle> okRgn = new List<Rectangle>();
            List<Rectangle> ngRgn = new List<Rectangle>();
            if (isErr)
            {
                var ngR = new UDrawingCarrierRect2d()
                {
                    _BorderColor = new tUDrawingCarrierRGB(Color.Red.R, Color.Red.G, Color.Red.B),
                    _BorderWidth = 3,
                };
                var okR = new UDrawingCarrierRect2d()
                {
                    _BorderColor = new tUDrawingCarrierRGB(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B),
                    _BorderWidth = 3,
                };
                //CurrDrawingCarriers = new UDrawingCarriers();
                foreach (var d in detectionMessage)
                {
                    if (d.Value == null || d.Value.Count < 2)
                        continue;
                    //int offsetY = 3;
                    //int fontSz = 12;
                    //int fontOffset = 15;
                    bool isNG = false;
                    if (d.Value[0].Length > 0)
                    {
                        var errTxt = new UDrawingCarrierTexts() { _FontColor = new tUDrawingCarrierRGB(Color.Red.R, Color.Red.G, Color.Red.B), _FontSize = fontSz };
                        foreach (var e in d.Value[0])
                        {
                            errTxt.Add(d.Key.Left + 3, d.Key.Top + offsetY, e);
                            offsetY += fontOffset;
                        }
                        CurrDrawingCarriers.Add(errTxt);
                        isNG = true;
                    }
                    if (d.Value[1].Length > 0)
                    {
                        var wrnTxt = new UDrawingCarrierTexts() { _FontColor = new tUDrawingCarrierRGB(Color.Pink.R, Color.Pink.G, Color.Pink.B), _FontSize = fontSz };
                        foreach (var w in d.Value[1])
                        {
                            wrnTxt.Add(d.Key.Left + 3, d.Key.Top + offsetY, w);
                            offsetY += fontOffset;
                        }
                        CurrDrawingCarriers.Add(wrnTxt);
                        isNG = true;
                    }

                    if ( isNG )
                    {
                        ngR.AddRect( new tUDrawingCarrierRect( d.Key.Left, d.Key.Top, d.Key.Right, d.Key.Bottom ) );
                        ngRgn.Add( d.Key );
                    }
                    else
                    {
                        okR.AddRect( new tUDrawingCarrierRect( d.Key.Left, d.Key.Top, d.Key.Right, d.Key.Bottom ) );
                        okRgn.Add( d.Key );
                    }
                }
                CurrDrawingCarriers.Add(ngR);
                CurrDrawingCarriers.Add(okR);
            }
            else
            {
                var okR = new UDrawingCarrierRect2d()
                {
                    _BorderColor = new tUDrawingCarrierRGB(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B),
                    _BorderWidth = 3,
                };
                foreach (var d in detectionMessage)
                {
                    var okTxt = new UDrawingCarrierTexts() { _FontColor = new tUDrawingCarrierRGB(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B), _FontSize = fontSz };
                    okTxt.Add(d.Key.Left + 3, d.Key.Top + offsetY, "OK");
                    CurrDrawingCarriers.Add(okTxt);
                    okR.AddRect(new tUDrawingCarrierRect(d.Key.Left, d.Key.Top, d.Key.Right, d.Key.Bottom));
                    okRgn.Add( d.Key );
                }
                CurrDrawingCarriers.Add(okR);
            }

            bStatusCode = true;
            CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray( detectionRaw, detectionMessage );

            List<string> message = new List<string>();
            foreach(var kv in detectionMessage)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append( $"Rect({kv.Key.Left}, {kv.Key.Top}, {kv.Key.Right}, {kv.Key.Bottom})=>" );

                if ( okRgn.Contains( kv.Key ) ) sb.Append( " OK" );
                else
                {
                    if ( kv.Value[ 0 ].Length > 0 )
                        sb.Append( $" Error: {string.Join( "; ", kv.Value[ 0 ] )}" );
                    if ( kv.Value[ 1 ].Length > 0 )
                        sb.Append( $" Warn: {string.Join( "; ", kv.Value[ 1 ] )}" );
                }
            }

            return new UDataCarrier[]
            {
                UDataCarrier.MakeOne(isErr, "detect result: status, bool"),
                UDataCarrier.MakeOne(okRgn, "detect result: ok region, List<Rectangle>"),
                UDataCarrier.MakeOne(ngRgn, "detect result: ng region, List<Rectangle>"),
                UDataCarrier.MakeOne(message, "detect result: message, List<string>"),
                UDataCarrier.MakeOne(detectionRaw, "detect result: raw, bool[]"),
            };

            //var results = UDataCarrier.MakeVariableItemsArray( isErr, detectionRaw, detectionMessage );
            //results[ 1 ].Desc = "SOP Detecting Message";
            //return results;
#else

            bStatusCode = true;
            return null;
#endif
        }

        #region parameter ioctl

        private UDataCarrier[] IoctrlGet_DetectionParams( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            bRetStatus = false;
            if ( !UDataCarrier.Get<EvaluationConf>( whichMacro.MutableInitialData, null, out var conf ) )
                return null;

            bRetStatus = true;

            var regions = conf.Parameters.ConvRegion();

            UDataCarrier.SerializeDic( regions, out var toStore );

            return UDataCarrier.MakeVariableItemsArray(
                conf.Parameters.JudgeObjRegionWay,
                toStore,
                conf.Parameters.WorkRegionParams.ToArray() );
            //return UDataCarrier.MakeOneItemArray( conf.Parameters );
            /*
            if ( conf.Regions == null || conf.Regions.Count <= 0 )
                return null;

            bRetStatus = true;

            var conv = ( from kv in conf.Regions select kv ).ToDictionary( kv => kv.Key, kv => kv.Value.ToArray() );
            if ( !UDataCarrier.SerializeDic( conv, out var toStore ) )
                return null;
            return UDataCarrier.MakeOneItemArray( toStore );
            */
        }
        private bool IoctrlSet_DetectionParams( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( !UDataCarrier.Get<EvaluationConf>( whichMacro.MutableInitialData, null, out var conf ) )
                return false;

            conf.Parameters.JudgeObjRegionWay = UDataCarrier.GetItem( data, 0, 0, out var dummy );
            if ( UDataCarrier.GetByIndex( data, 1, new string[ 0 ], out var convS ) && UDataCarrier.DeserializeDic( convS, out var regions ) )
            {
                conf.Parameters.ToRegion( regions );
            }
            else
                return false;
            if ( !UDataCarrier.GetByIndex( data, 2, new WorkingRegionParameters[ 0 ], out var confP ) )
                return false;

            conf.Parameters.WorkRegionParams = confP.ToList();

            return true;
            /*
            if ( !UDataCarrier.GetByIndex( data, 0, new string[0], out var content ) )
                return false;

            if ( !UDataCarrier.DeserializeDic( content, out var rev ) )
                return false;

            try
            {
                conf.Regions = ( from kv in rev select kv ).ToDictionary( kv => kv.Key, kv => kv.Value.ToList() );
                return true;
            }
            catch { return false; }
            */
        }

        #endregion


        private Form PopupConf( string callMethodName, UMacro macroToConf )
        {
            Form ret = null;
            if ( callMethodName == PredefinedMacroMethodNameEvaluation )
            {
                //return new FormConfRegions( macroToConf );
                return new FormParamEdit().ReloadParameters(macroToConf);
            }

            return ret;
        }

        public override bool ReadMacroSettings( ref UMacro macro, string pathOfFolder, string cfgFile )
        {
            if ( !base.ReadMacroSettings( ref macro, pathOfFolder, cfgFile ) )
                return false;

            if ( !UDataCarrier.Get<EvaluationConf>( macro.MutableInitialData, null, out var conf ) )
                return false;

            // call to config
            conf.ApplyParameters();

            //
            // call extra processing
            //
            // combine full conf path
            var confFullPath = Path.Combine( pathOfFolder, cfgFile );
            // get child node from parameter node
            if ( !ReadFromParameterNode( confFullPath, out var got, BackgroundImageNodeName ) || got.Count <= 0 )
                return false;
            // combine image path
            var backgroundFullPath = Path.Combine( pathOfFolder, got[ BackgroundImageNodeName ] );
            // copy
            return conf.SetBackgroundImagePath( backgroundFullPath, DataRWDir );
        }
        public override bool WriteMacroSettings( UMacro macro, string pathOfFolder, string cfgFile )
        {
            // Write settings (save settings)
            // 1. call base to save settings
            // 2. write model path info to xml
            // 3. copy background image to folder and zip later
            //
            if ( macro == null )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with macro instance null" );
                return false;
            }
            if ( macro.MutableInitialData == null )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with mutable data null" );
                return false;
            }
            if ( !UDataCarrier.Get<EvaluationConf>( macro.MutableInitialData, null, out var conf ) )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with mutable data type({macro.MutableInitialData.Tp.FullName}) invalid" );
                return false;
            }
            if ( conf == null )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"{m_strInternalGivenName} call to write settings with mutable data null" );
                return false;
            }


            if ( !base.WriteMacroSettings( macro, pathOfFolder, cfgFile ) )
                return false;

            // can use macro method name to distinguish
            var path2Reload = Path.Combine( pathOfFolder, cfgFile );
            var filename = Path.GetFileName( conf.BackgroundImagePath );
            var extraNods = new Dictionary<string, string>
            {
                { BackgroundImageNodeName, filename }
            };
            if ( WriteToParameterNode( path2Reload, extraNods ) )
            {
                try { File.Copy( conf.BackgroundImagePath, Path.Combine( pathOfFolder, filename ) ); }
                catch { return false; }
                return true;
            }

            return false;
        }

    }
}
