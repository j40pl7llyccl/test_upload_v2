using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class uMProvidImageLoader : UMacroMethodProviderPlugin
    {

        const string OpenImageFileMethodName01 = "ImageDev_OpenImageFile";
        const string OpenImageFileWithDisplayInfoMethodName02 = "ImageDev_OpenImageFileWithDisplayInfo";


        public uMProvidImageLoader() : base()
        {
            m_strInternalGivenName = "ImageLoader";
        }

        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened ) return true;

            // config the macro
            // fill opened method(s) inside macro to m_UserQueryOpenedMethods
            // + MUST config fields
            //   - MethodName: given name
            //   - PrevPropagationParamTypeDesc: describe requirement of the prev. macro output
            //   - RetPropagationParamTypeDesc: describe the macro output for next step
            //   - RetResultTypeDesc: describe the macro result
            m_UserQueryOpenedMethods.Add(
                new UMacro( null, m_strCSharpDefClassName, OpenImageFileMethodName01, OpenImageFile,
                            null, // immutable
                            null, // variable
                            null, // prev
                            new UDataCarrierTypeDescription[]{
                                new UDataCarrierTypeDescription(typeof(IntPtr), "Image buffer pointer"),
                                new UDataCarrierTypeDescription(typeof(int), "Image width"),
                                new UDataCarrierTypeDescription(typeof(int), "Image height"),
                                new UDataCarrierTypeDescription(typeof(int), "Image pixel bits"),
                                new UDataCarrierTypeDescription(typeof(string), "Image file path")
                            }// return
                )
            );
            m_createMacroDoneFromMethod.Add( OpenImageFileMethodName01, MacroShellDoneCall );

            m_UserQueryOpenedMethods.Add(
                new UMacro( null, m_strCSharpDefClassName, OpenImageFileWithDisplayInfoMethodName02, OpenImageFileAndDisplay,
                    null,
                    null,
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription( typeof(Control), "Control to display"),
                        new UDataCarrierTypeDescription( typeof(Control), "...")
                    },
                    new UDataCarrierTypeDescription[] {
                        new UDataCarrierTypeDescription( typeof(Control), "Control to display"),
                        new UDataCarrierTypeDescription( typeof(Control), "..."),
                        new UDataCarrierTypeDescription( typeof(string), "File path"),
                        //new UDataCarrierTypeDescription( typeof(Bitmap), "Loaded image")
                    }
                )
            );
            // config variable

            // config the macro GET/SET
            // fill macro get/set
            // - list all available names
            // - if multiple methods use same name, check by UMacro MethodName
            // - if own by one method, not check the MethodName
            m_MacroControls.Add( "LoadingImageDir", new UScriptControlCarrierMacro( "LoadingImageDir", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( string ), "Loading dir" ) },
                IoctrlGet_LoadingDir, IoctrlSet_LoadingDir ) );

            m_MacroControls.Add( OpenImageKey02.Param02_EnableCycleRun.ToString(),
                new UScriptControlCarrierMacro(
                    OpenImageKey02.Param02_EnableCycleRun.ToString(), true, true, true,
                    new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Enabled cycle run") },
                    IoctrlGet2_EnableCycRun, IoctrlSet2_EnableCycRun
                )
            );

            m_MacroControls.Add( OpenImageKey02.Param02_SearchPattern.ToString(),
                new UScriptControlCarrierMacro(
                    OpenImageKey02.Param02_SearchPattern.ToString(), true, true, true,
                    new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "search file pattern")},
                    IoctrlGet2_SearchPattern, IoctrlSet2_SearchPattern
                )
            );

            m_MacroControls.Add( OpenImageKey02.Param02_LoadingPath.ToString(),
                new UScriptControlCarrierMacro(
                    OpenImageKey02.Param02_LoadingPath.ToString(), true, true, true,
                    new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "loading folder path")},
                    IoctrlGet2_LoadingPath, IoctrlSet2_LoadingPath
                )
            );

            // popup UI to config
            m_macroMethodConfigPopup.Add( OpenImageFileMethodName01, PopupConf_OpenImageFile );
            m_macroMethodConfigPopup.Add( OpenImageFileWithDisplayInfoMethodName02, PopupConf_OpenImageFile );

            // create macro done call
            m_createMacroDoneFromMethod.Add( OpenImageFileWithDisplayInfoMethodName02, MacroShellDoneCall );

            // default parameter get set

            // config class GET/SET if necessary

            // [0]: Assembly RW path
            // [1]: Working dir

            InitImageFrom();

            m_bOpened = true;
            return true;
        }

        protected override UMacro NewMacroInstance( UMacro reference )
        {
            // create jump macro
            if ( reference.MethodName == ImageFromBufferMethodName || reference.MethodName == ImageFromFolderMethodName )
            {
                return new UMacroCapableOfCtrlFlow(
                    this, string.Copy( GetType().FullName ?? string.Empty ), string.Copy( reference.MethodName ), reference.fpHandler,
                    reference.ImmutableParamTypeDesc, reference.VariableParamTypeDesc,
                    reference.PrevPropagationParamTypeDesc, reference.RetPropagationParamTypeDesc,
                    reference.RetResultTypeDesc );
            }

            return base.NewMacroInstance( reference );
        }

        private bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == OpenImageFileWithDisplayInfoMethodName02 )
            {
                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>(){
                    { OpenImageKey02.Param02_LoadingPath.ToString(), UDataCarrier.MakeOne("") },
                    { OpenImageKey02.Param02_SearchPattern.ToString(), UDataCarrier.MakeOne("*.*") },
                    { OpenImageKey02.Param02_EnableCycleRun.ToString(), UDataCarrier.MakeOne(false) },
                    { OpenImageKey02.UserControl.ToString(), UDataCarrier.MakeOne( new UserControlDisplayLoadingInfo(){ WorkWith = instance }, true ) },
                    { OpenImageKey02.IncIndex.ToString(), UDataCarrier.MakeOne( true) },
                    { OpenImageKey02.NextIndex.ToString(), UDataCarrier.MakeOne(0) },
                }, true );

                instance.AbilityToInteractWithUI = true;
            }
            return true;
        }

        #region LoadingImageDir parameter GET/SET
        internal static UDataCarrier MakeOpenImageInitData( string path )
        {
            if ( !string.IsNullOrEmpty( path ) && Directory.Exists( path ) )
            {
                string[] found = new string[ 0 ];
                try { found = Directory.GetFiles( path, "*.*", SearchOption.TopDirectoryOnly ); } catch { }

                var ret = UDataCarrier.MakeVariableItemsArray( path, found, ( int )0, new UImageComBuffer() );
                // config to handleable resource
                ret[ ( int )OpenImageIndex01.Instance ].Handleable = true;
                return UDataCarrier.MakeOne( ret, true ); // mark as handleable
            }
            return null;
        }

        private bool IoctrlSet_LoadingDir( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( whichMacro.MethodName == OpenImageFileMethodName01 )
            {
                var path = UDataCarrier.GetItem( data, 0, "", out var status );
                whichMacro.MutableInitialData = MakeOpenImageInitData( path );
                return true;
            }

            return false;
        }

        private UDataCarrier[] IoctrlGet_LoadingDir( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            if (whichMacro.MethodName == OpenImageFileMethodName01 )
            {
                UDataCarrier[] ret = null;
                UDataCarrier[] innerD = whichMacro.MutableInitialData?.Data as UDataCarrier[] ?? null;
                if ( innerD != null && innerD.Length > ( ( int )OpenImageIndex01.SearchDir ) )
                {
                    var path = UDataCarrier.GetItem( innerD, ( int )OpenImageIndex01.SearchDir, "", out var dummy );
                    ret = UDataCarrier.MakeOneItemArray( path );
                    bRetStatus = true;
                }
                return ret;
            }

            bRetStatus = true;
            return null;
        }

        private Form PopupConf_OpenImageFile( string callMethodName, UMacro macroToConf )
        {
            if (callMethodName == OpenImageFileMethodName01)
                return new FormConfOpenImageDirectory() { MacroInstance = macroToConf };
            else if (callMethodName == OpenImageFileWithDisplayInfoMethodName02)
            {
                var form = new FormSetupOpenDirFiles() { RunWith = macroToConf };
                form.ReloadInfo();
                return form;
            }
            return null;
        }
        #endregion

        private UDataCarrier[] OpenImageFile( UMacro MacroInstance,
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
            if ( MacroInstance.MutableInitialData == null )
            {
                bStatusCode = false;
                strStatusMessage = "not config init data";
                return null;
            }
            if ( !( MacroInstance.MutableInitialData.Data is UDataCarrier[] data ) || data == null )
            {
                bStatusCode = false;
                strStatusMessage = "init data invalid";
                return null;
            }
            try
            {
                // get current index
                string[] founds = data[ ( int )OpenImageIndex01.FoundFiles ].Data as string[];
                int currindex = ( int )data[ ( int )OpenImageIndex01.CurrentIndex ].Data;
                UImageComBuffer buff = data[ ( int )OpenImageIndex01.Instance ].Data as UImageComBuffer;
                if ( founds == null || founds.Length == 0 )
                {
                    bStatusCode = false;
                    strStatusMessage = "not found any file to load";
                    return null;
                }
                if ( currindex >= founds.Length )
                {
                    bStatusCode = false;
                    strStatusMessage = "index out-of-range";
                    return null;
                }
                if ( buff == null )
                {
                    bStatusCode = false;
                    strStatusMessage = "image buffer not ready for image";
                    return null;
                }
                // get current file path
                string filepath = founds[ currindex ];
                // inc to next index
                data[ ( int )OpenImageIndex01.CurrentIndex ].Build( ++currindex >= founds.Length ? 0 : currindex );
                // load image file
                var gotBmp = buff.LoadBmp2Return( filepath, out var status );
                if ( !status )
                {
                    gotBmp?.Dispose();
                    bStatusCode = false;
                    strStatusMessage = $"load fail of image file {filepath}";
                    return null;
                }
                // create a bitmap for drawing
                if ( CurrDrawingCarriers == null )
                    CurrDrawingCarriers = new UDrawingCarriers( buff.Width, buff.Height );
                UDrawingCarrierBitmaps bmpsC = new UDrawingCarrierBitmaps();
                bmpsC.AddBmp( 0, 0, gotBmp, true );
                if ( !CurrDrawingCarriers.Add( bmpsC ) )
                {
                    bmpsC.Dispose();
                    bStatusCode = false;
                    strStatusMessage = $"cannot add drawing result";
                    return null;
                }
                // success
                bStatusCode = true;
                // gen propagate
                CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray(
                    buff.Buffer, // image buffer pointer
                    buff.Width,  // image width
                    buff.Height, // image height
                    buff.Bits,   // image format in bits
                    filepath     // image file location
                );
                // no result
                return null;
            }
            catch ( Exception e )
            {
                bStatusCode = false;
                strStatusMessage = $"Exception: {e}";
                return null;
            }
        }

    }

}