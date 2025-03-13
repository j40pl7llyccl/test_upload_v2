using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib.Script;
using uIP.Lib;
using System.Drawing;
using System.Windows.Forms;
using uIP.Lib.DataCarrier;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class uMProvidImageLoader
    {

        private UDataCarrier[] IoctrlGet2_EnableCycRun( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            // only OpenImageFileWithDisplayInfoMethodName02 available
            if ( whichMacro.MethodName != OpenImageFileWithDisplayInfoMethodName02 )
            {
                bRetStatus = true;
                return null;
            }

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( whichMacro.MutableInitialData, null, out var set ) || set == null )
            {
                bRetStatus = false;
                return null;
            }

            var enabled = false;
            if ( set.TryGetValue( OpenImageKey02.Param02_EnableCycleRun.ToString(), out var cycRunCarr ) && UDataCarrier.Get( cycRunCarr, false, out var cycRun ) )
                enabled = cycRun;

            bRetStatus = true;
            return UDataCarrier.MakeOneItemArray( enabled );
        }
        private bool IoctrlSet2_EnableCycRun( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            // only OpenImageFileWithDisplayInfoMethodName02 available
            if ( whichMacro.MethodName != OpenImageFileWithDisplayInfoMethodName02 )
            {
                return false;
            }

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( whichMacro.MutableInitialData, null, out var set ) || set == null )
                return false;
            if ( !UDataCarrier.GetByIndex<bool>( data, 0, false, out var enabled ) )
                return false;

            if ( set.ContainsKey( OpenImageKey02.Param02_EnableCycleRun.ToString() ) )
                set[ OpenImageKey02.Param02_EnableCycleRun.ToString() ] = UDataCarrier.MakeOne( enabled );
            else
                set.Add( OpenImageKey02.Param02_EnableCycleRun.ToString(), UDataCarrier.MakeOne( enabled ) );
            return true;
        }

        private UDataCarrier[] IoctrlGet2_SearchPattern( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            // only OpenImageFileWithDisplayInfoMethodName02 available
            if ( whichMacro.MethodName != OpenImageFileWithDisplayInfoMethodName02 )
            {
                bRetStatus = true;
                return null;
            }

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( whichMacro.MutableInitialData, null, out var set ) || set == null )
            {
                bRetStatus = false;
                return null;
            }

            var str = "*.*";
            if ( set.TryGetValue( OpenImageKey02.Param02_SearchPattern.ToString(), out var carr ) )
                UDataCarrier.Get( carr, "", out str );

            bRetStatus = true;
            return UDataCarrier.MakeOneItemArray( str );
        }
        private bool IoctrlSet2_SearchPattern( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            // only OpenImageFileWithDisplayInfoMethodName02 available
            if ( whichMacro.MethodName != OpenImageFileWithDisplayInfoMethodName02 )
            {
                return false;
            }

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( whichMacro.MutableInitialData, null, out var set ) || set == null )
                return false;
            if ( !UDataCarrier.GetByIndex( data, 0, "*.*", out var str ) )
                return false;

            if ( set.ContainsKey( OpenImageKey02.Param02_SearchPattern.ToString() ) )
                set[ OpenImageKey02.Param02_SearchPattern.ToString() ] = UDataCarrier.MakeOne( str );
            else
                set.Add( OpenImageKey02.Param02_SearchPattern.ToString(), UDataCarrier.MakeOne( str ) );
            return true;
        }

        private UDataCarrier[] IoctrlGet2_LoadingPath( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            // only OpenImageFileWithDisplayInfoMethodName02 available
            if ( whichMacro.MethodName != OpenImageFileWithDisplayInfoMethodName02 )
            {
                bRetStatus = true;
                return null;
            }

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( whichMacro.MutableInitialData, null, out var set ) || set == null )
            {
                bRetStatus = false;
                return null;
            }

            var str = "";
            if ( set.TryGetValue( OpenImageKey02.Param02_LoadingPath.ToString(), out var carr ) )
                UDataCarrier.Get( carr, "", out str );

            bRetStatus = true;
            return UDataCarrier.MakeOneItemArray( str );
        }
        private bool IoctrlSet2_LoadingPath( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            // only OpenImageFileWithDisplayInfoMethodName02 available
            if ( whichMacro.MethodName != OpenImageFileWithDisplayInfoMethodName02 )
            {
                return false;
            }

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( whichMacro.MutableInitialData, null, out var set ) || set == null )
                return false;
            if ( !UDataCarrier.GetByIndex( data, 0, "", out var str ) )
                return false;

            if ( set.ContainsKey( OpenImageKey02.Param02_LoadingPath.ToString() ) )
                set[ OpenImageKey02.Param02_LoadingPath.ToString() ] = UDataCarrier.MakeOne( str );
            else
                set.Add( OpenImageKey02.Param02_LoadingPath.ToString(), UDataCarrier.MakeOne( str ) );

            if ( !string.IsNullOrEmpty( str ) )
            {
                var searchPatt = "*.*";
                if ( set.TryGetValue( OpenImageKey02.Param02_SearchPattern.ToString(), out var carr ) )
                    UDataCarrier.Get( carr, searchPatt, out searchPatt );
                try
                {
                    var files = Directory.GetFiles( str, searchPatt, SearchOption.TopDirectoryOnly );
                    // reset file paths
                    if ( set.ContainsKey( OpenImageKey02.LoadedPaths.ToString() ) )
                        set[ OpenImageKey02.LoadedPaths.ToString() ] = UDataCarrier.MakeOne( files );
                    else
                        set.Add( OpenImageKey02.LoadedPaths.ToString(), UDataCarrier.MakeOne( files ) );
                    // reset index
                    if ( set.ContainsKey( OpenImageKey02.NextIndex.ToString() ) )
                        set[ OpenImageKey02.NextIndex.ToString() ] = UDataCarrier.MakeOne( 0 );
                    else
                        set.Add( OpenImageKey02.NextIndex.ToString(), UDataCarrier.MakeOne( 0 ) );
                }
                catch ( Exception ex ) { }
            }

            return true;
        }

        private UDataCarrier[] OpenImageFileAndDisplay( UMacro MacroInstance,
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
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( MacroInstance.MutableInitialData, null, out var mic ) || mic == null )
            {
                strStatusMessage = "cannot convert mutable init data";
                return null;
            }

            // get user control from prev propagation
            mic.TryGetValue( OpenImageKey02.UserControl.ToString(), out UDataCarrier ucc );
            UDataCarrier.Get<Control>( ucc, null, out var genCtrl );

            UDataCarrier target = null;
            Control container = null;
            UDataCarrier[] nextPropagation = null;

            if ( ucc != null && ucc.Handleable )
            {
                // check data in propagations
                if ( MacroInstance.QueryScriptPropagationOne( "LT", new Func<object, UDataCarrier, UDataCarrier>( CmpByStringInDesc ), out var location, out target, -1 ) &&
                    UDataCarrier.Get( target, null, out container ) && container != null && genCtrl != null )
                {
                    ResourceManager.InvokeMainThread( new Action( () =>
                    {
                        if ( genCtrl is UserControlDisplayLoadingInfo want )
                            MacroInstance.SetScriptMacroControl( location, "InstallKeyPressCall", UDataCarrier.MakeOne( new Action<Keys>( want.RxKey ) ) );
                        container.Controls.Add( genCtrl );
                    } ) );
                    ucc.Handleable = false; // give the IDispose right to form
                }
            }

            if ( ucc != null && ucc.Handleable )
            {
                // check data in results
                if ( MacroInstance.QueryScriptResultOne( "LT", new Func<object, UDataCarrier, UDataCarrier>( CmpByStringInDesc ), out var location, out target, -1 ) &&
                    UDataCarrier.Get( target, null, out container ) && container != null && genCtrl != null )
                {
                    ResourceManager.InvokeMainThread( new Action( () =>
                    {
                        if ( genCtrl is UserControlDisplayLoadingInfo want )
                            MacroInstance.SetScriptMacroControl( location, "InstallKeyPressCall", UDataCarrier.MakeOne( new Action<Keys>( want.RxKey ) ) );
                        container.Controls.Add( genCtrl );
                    } ) );
                    ucc.Handleable = false; // give the IDispose right to form
                    nextPropagation = historyResultCarriers[ 0 ].ResultSet;
                }
            }

            // get owner carrier
            UDataCarrier.GetByCmpOne( PrevPropagationCarrier == null ? nextPropagation : PrevPropagationCarrier, "Owner", CmpByStringInDesc, out var ownerC );

            // get file from loading in mutable init variable
            if ( !mic.TryGetValue( OpenImageKey02.LoadedPaths.ToString(), out var filepathsC ) ||
                !UDataCarrier.Get<string[]>( filepathsC, null, out var filepaths ) ||
                filepaths == null || filepaths.Length <= 0 )
            {
                strStatusMessage = "file path(s) not ready";
                MessageBox.Show( $"File(s) not found to end!" );
                UDataCarrier.Get<Form>( ownerC, null )?.Hide();
                return null;
            }

            bool bInc = true;
            int currIndex = 0;
            int nextIndex = 0;
            if ( mic.TryGetValue( OpenImageKey02.NextIndex.ToString(), out var indexCarr ) )
            {
                UDataCarrier.Get( indexCarr, 0, out currIndex );
            }
            if ( mic.TryGetValue( OpenImageKey02.IncIndex.ToString(), out var incCarr ) )
            {
                UDataCarrier.Get( incCarr, false, out bInc );
            }

            var cycRun = false;
            // check cycle run
            if ( mic.TryGetValue( OpenImageKey02.Param02_EnableCycleRun.ToString(), out var cycRunCarr ) && UDataCarrier.Get( cycRunCarr, true, out cycRun ) )
            { }

            // update next index
            currIndex = currIndex < 0 ? filepaths.Length - 1 : ( currIndex >= filepaths.Length ? 0 : currIndex );
            if ( cycRun )
            {
                if ( bInc )
                    nextIndex = currIndex + 1 >= filepaths.Length ? 0 : currIndex + 1;
                else
                    nextIndex = currIndex - 1 < 0 ? filepaths.Length - 1 : currIndex - 1;
            }
            else
            {
                if ( bInc )
                    nextIndex = currIndex + 1 >= filepaths.Length ? currIndex : currIndex + 1;
                else
                    nextIndex = currIndex - 1 < 0 ? currIndex : currIndex - 1;
            }

            if ( mic.ContainsKey( OpenImageKey02.NextIndex.ToString() ) )
                mic[ OpenImageKey02.NextIndex.ToString() ].Data = nextIndex;
            else
                mic.Add( OpenImageKey02.NextIndex.ToString(), UDataCarrier.MakeOne( nextIndex ) );

            // load bmp file
            Bitmap loadBmp = null;
            try
            {
                using ( var fs = File.Open( filepaths[ currIndex ], FileMode.Open ) )
                {
                    loadBmp = new Bitmap( fs );
                }
            }
            catch ( Exception e )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 0, $"Loading {filepaths[ currIndex ]} with exception:\n{e}" );
                strStatusMessage = $"Load file {filepaths[ currIndex ]} fail";
                loadBmp?.Dispose();

                MessageBox.Show( $"Load file {filepaths[ currIndex ]} fail to end!" );
                UDataCarrier.Get<Form>( ownerC, null )?.Hide();

                return null;
            }
            if ( loadBmp == null )
            {
                strStatusMessage = $"load file {filepaths[ currIndex ]} to bmp fail";

                MessageBox.Show( $"Bitmpa not read to end!" );
                UDataCarrier.Get<Form>( ownerC, null )?.Hide();

                return null;
            }

            if ( mic.TryGetValue( OpenImageKey02.Image.ToString(), out var imageCarr ) )
            {
                // free prev bitmap
                ( imageCarr.Data as IDisposable )?.Dispose();
                // assign new
                imageCarr.Data = loadBmp;
            }
            else
                mic.Add( OpenImageKey02.Image.ToString(), UDataCarrier.MakeOne( loadBmp, true ) );

            // update info
            if ( genCtrl is UserControlDisplayLoadingInfo dispInfoCtrl && dispInfoCtrl != null )
            {
                dispInfoCtrl.SetInfo( Path.GetFileName( filepaths[ currIndex ] ), currIndex, filepaths.Length );
            }

            // create a bitmap for drawing
            //if ( CurrDrawingCarriers == null )
            //    CurrDrawingCarriers = new UDrawingCarriers( loadBmp.Width, loadBmp.Height );
            //UDrawingCarrierBitmaps bmpsC = new UDrawingCarrierBitmaps();
            //bmpsC.AddBmp( 0, 0, loadBmp, false );
            //if ( !CurrDrawingCarriers.Add( bmpsC ) )
            //{
            //    bmpsC.Dispose();
            //    strStatusMessage = $"cannot add drawing result";
            //    return null;
            //}

            // prepare propagation
            // keep previous propagation
            var propagation = new List<UDataCarrier>(); //PrevPropagationCarrier == null ? ( nextPropagation == null ? new List<UDataCarrier>() : nextPropagation.ToList() ) : PrevPropagationCarrier.ToList();
            // add file path
            propagation.Add( UDataCarrier.MakeOne( filepaths[ currIndex ] ) );
            propagation[ propagation.Count - 1 ].Desc = "filepath";
            // add current bitmap result
            //propagation.Add( UDataCarrier.MakeOne( loadBmp ) );
            CurrPropagationCarrier = propagation.ToArray();

            bStatusCode = true;
            return null;
        }
    }
}
