using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.DataCarrier;
using uIP.Lib;
using uIP.Lib.Script;
using System.IO;
using System.Drawing;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    public partial class uProviderSopCommunication
    {
        const string BufferResultMethodName = "SopBufferResultMethod";
        const string ImageSetResultMethodName = "SopImageSetResultMethod";


        internal void InitMacro()
        {
            // install macro method
            m_UserQueryOpenedMethods.Add(
                new UMacro( null, NameOfCSharpDefClass, BufferResultMethodName, SopBufferResultMethod,
                    null, null, null,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(bool), "status"),
                        new UDataCarrierTypeDescription(typeof(string[]), "message")
                    } )
            );

            m_UserQueryOpenedMethods.Add(
                new UMacro( null, NameOfCSharpDefClass, ImageSetResultMethodName, SopImageSetResultMethod,
                    null, null, null,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(bool), "final status"),
                        new UDataCarrierTypeDescription(typeof(string[]), "error message")
                    } )
            );

            // create macro shell done call
            m_createMacroDoneFromMethod.Add( BufferResultMethodName, MacroShellDoneCall );
            m_createMacroDoneFromMethod.Add( ImageSetResultMethodName, MacroShellDoneCall );

            // install settings
            m_macroMethodSettingsGet.Add( BufferResultMethodName, GetMacroMethodSettings );
            m_macroMethodSettingsSet.Add( BufferResultMethodName, SetMacroMethodSettings );
            m_macroMethodConfigPopup.Add( BufferResultMethodName, PopupMacroConfigDialog );

            m_macroMethodSettingsGet.Add( ImageSetResultMethodName, GetMacroMethodSettings );
            m_macroMethodSettingsSet.Add( ImageSetResultMethodName, SetMacroMethodSettings );
            m_macroMethodConfigPopup.Add( ImageSetResultMethodName, PopupMacroConfigDialog );

            // install begin exec each time
            m_macroMethodScriptNotifyBeginExec.Add( ImageSetResultMethodName, new Action<UScript, int, UMacro>( ResetImageSetResults ) );

            ResourceManager.AddSystemUpCalls( new Action( SystemUpCallCreateTable ) );
        }

        void SystemUpCallCreateTable()
        {
            var plugin = ULibAgent.Singleton.AssemblyPlugins.GetPluginInstanceFromGivenName( DBProvider );
            IsDbReady = plugin != null;
            DbProviderClassFullname = plugin.NameOfCSharpDefClass;

            // create table
            BufferResultTableReady =
                ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTable.ToString(), out _, GenBufferResultTableField(BufferResultTable) );
            VideoResultTableReady =
                ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTable.ToString(), out _, GenVideoResultTableField(VideoResultTable) );
            FolderResultTableReady =
                ULibAgent.CallPluginClassOpenedFuncRetStatus(DbProviderClassFullname, DBFuncs.CreateTable.ToString(), out _, GenFolderResultTableField(FolderResultTable) );
        }

        protected override UMacro NewMacroInstance( UMacro reference )
        {
            // create jump macro
            if ( reference.MethodName == ImageSetResultMethodName )
            {
                return new UMacroCapableOfCtrlFlow(
                    this, string.Copy( GetType().FullName ?? string.Empty ), string.Copy( reference.MethodName ), reference.fpHandler,
                    reference.ImmutableParamTypeDesc, reference.VariableParamTypeDesc,
                    reference.PrevPropagationParamTypeDesc, reference.RetPropagationParamTypeDesc,
                    reference.RetResultTypeDesc )
                {
                    AbilityToJumpIndex = true
                };
            }
            return base.NewMacroInstance( reference );
        }
        bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            if (callMethodName == BufferResultMethodName)
            {
                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                {
                    { MutableKeys.Param_SaveImage.ToString(), UDataCarrier.MakeOne(false) },
                    { MutableKeys.Param_SaveImageDir.ToString(), UDataCarrier.MakeOne("") },
                    { MutableKeys.Param_WriteDbResult.ToString(), UDataCarrier.MakeOne(true) },
                } );
            }
            else if (callMethodName == ImageSetResultMethodName)
            {
                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                {
                    { MutableKeys.Param_SaveImage.ToString(), UDataCarrier.MakeOne(false) },
                    { MutableKeys.Param_SaveImageDir.ToString(), UDataCarrier.MakeOne("") },
                    { MutableKeys.Param_WriteDbResult.ToString(), UDataCarrier.MakeOne(true) },
                    { MutableKeys.Param_LogMsg.ToString(), UDataCarrier.MakeOne(true) },
                    { MutableKeys.Param_ContIndex.ToString(), UDataCarrier.MakeOne(0) },
                    { MutableKeys.Param_IsFromVideo.ToString(), UDataCarrier.MakeOne(false) },
                    { MutableKeys.ImageSetResults.ToString(), UDataCarrier.MakeOne(new Dictionary<string, dynamic>()) }
                } );
            }
            return true;
        }

        Form PopupMacroConfigDialog( string callMethodName, UMacro macroToConf )
        {
            if ( callMethodName == BufferResultMethodName )
                return new FormBufferResultSettings() { WorkWith = macroToConf, Text = "Config buffer result" }.UpdateToUI();
            else if ( callMethodName == ImageSetResultMethodName )
                return new FormImageSetResultSettings() { WorkWith = macroToConf, Text = "Config image set result" }.UpdateToUI();

            return null;
        }

        bool GetMacroMethodSettings( UMacro m, out object settings, out Type t )
        {
            settings = null;
            t = null;

            if ( UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null, out var dic ) )
            {
                var keep = new Dictionary<string, UDataCarrier>();
                foreach ( var kv in dic )
                {
                    if (kv.Key.IndexOf("Param_") == 0)
                        keep.Add(kv.Key, kv.Value);
                }
                if (keep.Count > 0)
                {
                    if ( UDataCarrier.SerializeDicKeyString( keep, out var store ) )
                    {
                        settings = store;
                        t = store.GetType();
                    }
                }
            }

            return true;
        }

        bool SetMacroMethodSettings( UMacro m, object settings )
        {
            if ( UDataCarrier.DeserializeDicKeyStringValueOne(settings as string[], out var got ))
            {
                foreach(var kv in got)
                {
                    UDataCarrier.SetDicKeyStrOne(m.MutableInitialData, kv.Key, kv.Value);
                }

                if (m is UMacroCapableOfCtrlFlow fm )
                {
                    var index = UDataCarrier.Get( got, MutableKeys.Param_ContIndex.ToString(), -1 );
                    fm.MustJump = index >= 0;
                    fm.Jump2WhichMacro = index;
                }
            }

            return true;
        }

        private static string SaveImageByDay(UMacro m, string filePrefix)
        {
            if ( m == null )
                return "";

            var dic = UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null );
            if ( dic == null )
                return "";

            var savedImgPath = "";
            // get info from mutable data
            // - save image?
            // - save image folder path
            // - save image format
            if ( dic.TryGetValue( MutableKeys.Param_SaveImage.ToString(), out var saveImgCarr ) &&
                 UDataCarrier.Get( saveImgCarr, false, out var bSaveImg ) &&
                 dic.TryGetValue( MutableKeys.Param_SaveImageDir.ToString(), out var saveDirCarr ) &&
                 UDataCarrier.Get( saveDirCarr, "", out var saveDir ) && !string.IsNullOrEmpty( saveDir ) &&
                 Directory.Exists( saveDir ) &&
                 UDataCarrier.Get( dic, MutableKeys.Param_SaveImageFormat.ToString(), "", out var imgFormat ) &&
                 Decls.AcceptableImageFormats.TryGetValue( imgFormat, out var iformat ) )
            {
                // get drawing to save
                // - query from script result in desc field: "drawing result"
                if ( m.QueryScriptResultOne( "drawing result", new Func<object, UDataCarrier, UDataCarrier>( UDataCarrier.CmpPerfectStrInDescLowercase ),
                     out _, out var got ) &&
                     UDataCarrier.Get<Bitmap>( got, null, out var bmp ) && bmp != null )
                {
                    try
                    {
                        var pathDir = Path.Combine( saveDir, $"{DateTime.Now.Year:0000}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}" );
                        var dir = CommonUtilities.RCreateDir2( pathDir );
                        if ( !string.IsNullOrEmpty( dir ) )
                        {
                            var filepath = Path.Combine( dir, string.IsNullOrEmpty( filePrefix ) ? "" : $"{filePrefix}-" + $"{CommonUtilities.GetCurrentTimeStr( "" )}.{imgFormat}" );
                            using ( var fs = File.Open( filepath, FileMode.Create, FileAccess.ReadWrite ) )
                            {
                                bmp.Save( fs, iformat );
                            }
                            savedImgPath = filepath;
                        }
                    }
                    catch { }
                }
            }
            return savedImgPath;
        }

        private static string SaveImageToNew( UMacro m, string filePrefix )
        {
            if ( m == null )
                return "";

            var dic = UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null );
            if ( dic == null )
                return "";

            // existing?
            if (!UDataCarrier.GetDicKeyStrOne(m.MutableInitialData, MutableKeys.ResultImageOutputDir.ToString(), "", out var outputDir) ||
                string.IsNullOrEmpty(outputDir) )
            {
                // root dir not exist
                if ( !dic.TryGetValue( MutableKeys.Param_SaveImageDir.ToString(), out var saveDirCarr ) ||
                     !UDataCarrier.Get( saveDirCarr, "", out var saveDir ) || 
                     string.IsNullOrEmpty( saveDir ) ||
                     !Directory.Exists( saveDir ) )
                {
                    return "";
                }

                outputDir = Path.Combine( saveDir, CommonUtilities.GetCurrentTimeStr() );
                outputDir = CommonUtilities.RCreateDir2( outputDir );

                // keep
                UDataCarrier.SetDicKeyStrOne( m.MutableInitialData, MutableKeys.ResultImageOutputDir.ToString(), outputDir );
            }
            
            var savedImgPath = "";
            // get info from mutable data
            // - save image format
            if ( !string.IsNullOrEmpty(outputDir) &&
                 UDataCarrier.Get( dic, MutableKeys.Param_SaveImageFormat.ToString(), "", out var imgFormat ) &&
                 Decls.AcceptableImageFormats.TryGetValue( imgFormat, out var iformat ) )
            {
                // get drawing to save
                // - query from script result in desc field: "drawing result"
                if ( m.QueryScriptResultOne( "drawing result", new Func<object, UDataCarrier, UDataCarrier>( UDataCarrier.CmpPerfectStrInDescLowercase ),
                     out _, out var got ) &&
                     UDataCarrier.Get<Bitmap>( got, null, out var bmp ) && bmp != null )
                {
                    try
                    {
                        var filepath = Path.Combine( outputDir, string.IsNullOrEmpty( filePrefix ) ? "" : $"{filePrefix}-" + $"{CommonUtilities.GetCurrentTimeStr( "" )}.{imgFormat}" );
                        using ( var fs = File.Open( filepath, FileMode.Create, FileAccess.ReadWrite ) )
                        {
                            bmp.Save( fs, iformat );
                        }
                        savedImgPath = filepath;
                    }
                    catch { }
                }
            }
            return savedImgPath;
        }

        private static bool GetResult(UMacro m, out bool bRetStatus, out string strRetStatus )
        {
            var bfindResult = false;
            bRetStatus = false;
            strRetStatus = "";

            // get all result
            // - query script results in desc field with begin "detect result:"
            if ( m.QueryScriptResultMany( "detect result:", new Func<object, UDataCarrier, UDataCarrier>( UDataCarrier.CmpBeginStrInDesc ), out var founds ) )
            {
                var results = founds.First();
                var findStatus = false;
                var findMessage = false;

                foreach ( var r in results.Value )
                {
                    // from found results, get desc contain "status"; eg, "detect result: status"
                    if ( !findStatus && r.Desc.Contains( "status" ) )
                    {
                        bRetStatus = ( bool )r.Data;
                        findStatus = true;
                    }
                    // from found results, get desc contain "message"; eg, "detect result: message"
                    else if ( !findMessage && r.Desc.Contains( "message" ) )
                    {
                        strRetStatus = string.Join( "\n", ( r.Data as List<string> )?.ToArray() ?? new string[ 0 ] );
                        findMessage = true;
                    }
                }

                bfindResult = findStatus;
            }

            return bfindResult;
        }

        private UDataCarrier[] SopBufferResultMethod( UMacro MacroInstance,
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
            if (!UDataCarrier.Get<Dictionary<string,UDataCarrier>>( MacroInstance.MutableInitialData, null, out var dic))
            {
                strStatusMessage = "invalid mutable data";
                return null;
            }

            // write image out according to settings
            var savedImgPath = "";

            // get all result
            var bfindResult = false;
            var bRetStatus = false;
            var strRetStatus = "";
            bfindResult = GetResult(MacroInstance, out bRetStatus, out strRetStatus);

            if ( !bfindResult )
            {
                bRetStatus = false;
                strRetStatus = "no result";
            }

            if (!bRetStatus)
                savedImgPath = SaveImageByDay( MacroInstance, "Buff" );

            // write to DB?
            if ( UDataCarrier.GetDicKeyStrOne( MacroInstance.MutableInitialData, MutableKeys.Param_WriteDbResult.ToString(), false ) )
            {
                // write data to db
                var s = GenInsert( BufferResultTable, new Dictionary<string, object>()
                {
                    { "data_source" , MacroInstance.OwnerOfScript.NameOfId },
                    { "status", (long)(bRetStatus ? 0 : 1) },
                    { "image_path", savedImgPath },
                    { "results", strRetStatus }
                }
                );
                AddExecString( s );
            }

            // keep result as report
            bStatusCode = true;
            return new UDataCarrier[]
            {
                UDataCarrier.MakeOne( bRetStatus, ResultDescription.inspect_status.ToString() ),
                UDataCarrier.MakeOne( new string[] {strRetStatus }, ResultDescription.inspect_messages.ToString() )
            };
        }

        private static void ResetImageSetResults(UScript s, int begIndex, UMacro m)
        {
            // get dictionary
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m?.MutableInitialData ?? null, null, out var dic ) )
                return;

            // reset results
            UDataCarrier.Set( dic, MutableKeys.ImageSetResults.ToString(), new Dictionary<string, dynamic>() );

            // reset output image path
            UDataCarrier.Set( dic, MutableKeys.ResultImageOutputDir.ToString(), "" );
        }
        private UDataCarrier[] SopImageSetResultMethod( UMacro MacroInstance,
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
            var bFromVideo = false;
            var bRetStatus = false;
            var savedImgPath = "";
            var jm = MacroInstance as UMacroCapableOfCtrlFlow;
            var bSaveImage = UDataCarrier.GetDicKeyStrOne( MacroInstance.MutableInitialData, MutableKeys.Param_SaveImage.ToString(), false );
            //
            // process jump from another macro into
            //
            if ( MacroInstance.IsJumpInto)
            {
                // check pre-propagations
                if ( !UDataCarrier.GetByCmpOne(PrevPropagationCarrier, "current index", UDataCarrier.CmpBeginStrInDescLowercase, out var currIndexCarr ) ||
                     !UDataCarrier.Get(currIndexCarr, -1, out var currI) ||
                     !UDataCarrier.GetByCmpOne(PrevPropagationCarrier, "total files", UDataCarrier.CmpBeginStrInDescLowercase, out var totalFilesCarr ) || 
                     !UDataCarrier.Get(totalFilesCarr, 0, out var totalFiles) )
                {
                    strStatusMessage = "jump into without 'current index' and 'total files' in prepropagation";
                    return null;
                }

                if ( currI >= 0 && totalFiles > 0 && currI == totalFiles - 1)
                {
                    jm.MustJump = true;
                    jm.Jump2WhichMacro = ( int )MacroGotoFunctions.GOTO_END;
                    goto SopImageSetResultMethod_pack_result;
                }

                // continue
                if (!UDataCarrier.GetDicKeyStrOne( MacroInstance.MutableInitialData, MutableKeys.Param_ContIndex.ToString(), -1, out var jindex ) )
                {
                    strStatusMessage = "cannot get jump index in mutable data";
                    return null;
                }
                bStatusCode = true;
                jm.Jump2WhichMacro = jindex;
                return null;
            }

            //
            // normal inspection end with result
            //
            // get file path:
            // - query script results in desc field: "loaded file path"
            if ( !MacroInstance.QueryScriptResultOne( "loaded file path", new Func<object, UDataCarrier, UDataCarrier>( UDataCarrier.CmpPerfectStrInDescLowercase ), out _, out var gotLoadFileCarr, -1 ) ||
                 !UDataCarrier.Get( gotLoadFileCarr, "", out var filepath ) || string.IsNullOrEmpty( filepath ) )
            {
                strStatusMessage = "not get loaded file path info from result list";
                return null;
            }

            // keep results
            // get all result
            var bfindResult = false;
            var strRetStatus = "";
            bfindResult = GetResult( MacroInstance, out bRetStatus, out strRetStatus );

            if ( !bfindResult )
            {
                bRetStatus = false;
                strRetStatus = "no result";
            }

            // get request from:
            // - query from propagations in desc field: InputSourceDescription.input_source_from.ToString()
            bFromVideo = MacroInstance.QueryScriptPropagationOne( InputSourceDescription.input_source_from.ToString(), new Func<object, UDataCarrier, UDataCarrier>( UDataCarrier.CmpPerfectStrInDescLowercase ), out _, out var inputSourceCarr, -1 ) &&
                         UDataCarrier.Get( inputSourceCarr, "", out var inputSource ) &&
                         Enum.TryParse<InputSourceDescription>( inputSource, out var inptSrc ) &&
                         inptSrc == InputSourceDescription.input_source_from_video;
            // only error write image out according to settings
            // check if saving image
            if ( !bRetStatus && bSaveImage)
                savedImgPath = SaveImageToNew( MacroInstance, bFromVideo ? "Video" : "Folder" );

            if (!UDataCarrier.GetDicKeyStrOne<Dictionary<string, dynamic>>(MacroInstance.MutableInitialData, MutableKeys.ImageSetResults.ToString(), null, out var keepResults))
            {
                strStatusMessage = "not get image result keeper in mutable data";
                return null;
            }
            // keep result in mutable data for report
            keepResults.Add( filepath, new { Status = bRetStatus, StatusMessage = strRetStatus, OutputImageFile = Path.GetFileName(savedImgPath) } );

            // get image loading info
            var bLastImage = false;
            if ( MacroInstance.QueryScriptResultOne( "loading reach end", new Func<object, UDataCarrier, UDataCarrier>( UDataCarrier.CmpPerfectStrInDescLowercase ), out _, out var bReachEndCarr, -1 ) &&
                 UDataCarrier.Get( bReachEndCarr, false, out var bReachEnd ) &&
                 bReachEnd )
                bLastImage = true;
            if (!bLastImage) // not all image done yet
            {
                // continue
                if ( !UDataCarrier.GetDicKeyStrOne( MacroInstance.MutableInitialData, MutableKeys.Param_ContIndex.ToString(), -1, out var jindex ) )
                {
                    strStatusMessage = "cannot get jump index in mutable data";
                    return null;
                }
                bStatusCode = true;
                jm.Jump2WhichMacro = jindex;
                return null;
            }

            // final to end
            jm.Jump2WhichMacro = ( int )MacroGotoFunctions.GOTO_END;

SopImageSetResultMethod_pack_result:
            if ( !MacroInstance.QueryScriptResultOne( "loaded files timestamp", UDataCarrier.CmpPerfectStrInDescLowercase, out _, out var filestimestampCarr ) || 
                 !UDataCarrier.Get<Dictionary<string, DateTime>>(filestimestampCarr, null, out var filestimestamp) || 
                 filestimestamp == null )
            {
                strStatusMessage = "cannot find 'loaded files timestamp'";
                return null;
            }
            UDataCarrier[] results = null;
            List<string> NGs = new List<string>();
            List<string> NGImages = new List<string>();
            try
            {
                if (!UDataCarrier.GetDicKeyStrOne<Dictionary<string, dynamic>>(MacroInstance.MutableInitialData, MutableKeys.ImageSetResults.ToString(), null, out var gotSavedResults))
                {
                    fpLog?.Invoke( eLogMessageType.WARNING, 0, $"cannot get image result set in mutable" );
                    return null;
                }

                bool finStatus = true;
                foreach( var kv in gotSavedResults )
                {
                    if (finStatus && !kv.Value.Status )
                        finStatus = false;
                    if (!kv.Value.Status)
                    {
                        if ( bFromVideo )
                        {
                            // notify time offset
                            if ( filestimestamp.TryGetValue( kv.Key, out var ft ) )
                            {
                                var diff = ft - filestimestamp.First().Value;
                                NGs.Add( $"{diff.TotalSeconds:0.00}: {kv.Value.StatusMessage}" );
                                if ( bSaveImage ) NGImages.Add( $"{diff.TotalSeconds:0.00}: {kv.Value.OutputImageFile}" );
                            }
                            else
                            {
                                NGs.Add( $"NF({Path.GetFileName( kv.Key )}): {kv.Value.StatusMessage}" );
                                if ( bSaveImage ) NGImages.Add( $"NF({Path.GetFileName( kv.Key )}): {kv.Value.OutputImageFile}" );
                            }
                        }
                        else
                        {
                            NGs.Add( $"{Path.GetFileName( kv.Key )}: {kv.Value.StatusMessage}" );
                            if ( bSaveImage ) NGImages.Add( $"{Path.GetFileName( kv.Key )}: {kv.Value.OutputImageFile}" );
                        }
                    }
                }
                results = new UDataCarrier[]
                {
                    UDataCarrier.MakeOne( finStatus, ResultDescription.inspect_status.ToString() ),
                    UDataCarrier.MakeOne( NGs.ToArray(), ResultDescription.inspect_messages.ToString() )
                };
            }
            catch(Exception ex)
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"pack result with exception:\n{ex}" );
                return null;
            }

            // write to DB?
            if ( UDataCarrier.GetDicKeyStrOne( MacroInstance.MutableInitialData, MutableKeys.Param_WriteDbResult.ToString(), false ) )
            {
                // write data to db
                var s = "";
                var datS = "";
                var tb = "";
                if ( bFromVideo )
                {
                    tb = VideoResultTable;
                    if ( MacroInstance.QueryScriptPropagationOne( InputSourceDescription.input_source_from_video.ToString(), UDataCarrier.CmpPerfectStrInDesc, out _, out var videoPathCarr, -1 ) )
                        datS = UDataCarrier.Get( videoPathCarr, "" );
                }
                else
                {
                    tb = FolderResultTable;
                    if ( MacroInstance.QueryScriptPropagationOne( InputSourceDescription.input_source_from_folder.ToString(), UDataCarrier.CmpPerfectStrInDesc, out _, out var folderPathCarr, -1 ) )
                        datS = UDataCarrier.Get( folderPathCarr, "" );
                }

                if (bSaveImage)
                {
                    // format to ini
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine( "[root_path]" );
                    sb.AppendLine( $"{UDataCarrier.GetDicKeyStrOne(MacroInstance.MutableInitialData, MutableKeys.ResultImageOutputDir.ToString(), "")}" );
                    sb.AppendLine( "[files]" );
                    sb.AppendLine( string.Join( "\n", NGImages.ToArray() ) );

                    s = GenInsert( tb, new Dictionary<string, object>()
                        {
                            { "data_source", datS },
                            { "description", MacroInstance.OwnerOfScript.NameOfId },
                            { "status", (long)(bRetStatus ? 0 : 1) },
                            { "results", string.Join("\n", NGs.ToArray()) },
                            { "image_path", sb.ToString() }
                        } );
                }
                else
                {
                    s = GenInsert( tb, new Dictionary<string, object>()
                        {
                            { "data_source", datS },
                            { "description", MacroInstance.OwnerOfScript.NameOfId },
                            { "status", (long)(bRetStatus ? 0 : 1) },
                            { "results", string.Join("\n", NGs.ToArray()) }
                        } );
                }
                AddExecString( s );
            }

            bStatusCode = true;
            return results;
        }
    }
}
