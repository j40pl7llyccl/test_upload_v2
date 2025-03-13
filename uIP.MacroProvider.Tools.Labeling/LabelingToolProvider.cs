using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Script;

namespace uIP.MacroProvider.Tools.Labeling
{
    public class LabelingToolProvider : UMacroMethodProviderPlugin
    {
        const string LabelingMethodName = "LabelingWithGUI";
        public LabelingToolProvider() { }
        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            m_UserQueryOpenedMethods.Add( new UMacro( null, NameOfCSharpDefClass, LabelingMethodName, LabelingWithGUI,
                null, null,
                new UDataCarrierTypeDescription[]
                {
                    new UDataCarrierTypeDescription( typeof(Control), "Control to display"),
                    new UDataCarrierTypeDescription( typeof(Control), "..."),
                    new UDataCarrierTypeDescription( typeof(string), "File path"),
                },
                new UDataCarrierTypeDescription[]
                {
                    new UDataCarrierTypeDescription( typeof(Control), "Control to display"),
                    new UDataCarrierTypeDescription( typeof(Control), "..."),
                } )
            );

            // create macro done call
            m_createMacroDoneFromMethod.Add( LabelingMethodName, MacroShellDoneCall );

            m_bOpened = true;
            return true;
        }

        private bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == LabelingMethodName )
            {
                // create variable to store and mark as handleable
                instance.MutableInitialData = UDataCarrier.MakeOne( new UserControlLabelingEdit(), true );
                instance.AbilityToInteractWithUI = true;
            }
            return true;
        }

        private static UDataCarrier CmpDescEqu(object ctx, UDataCarrier curr)
        {
            if ( ctx == null || curr == null )
                return null;
            if ( ctx.ToString() == curr.Desc )
                return curr;
            return null;
        }

        private UDataCarrier[] LabelingWithGUI( UMacro MacroInstance,
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

            if (!UDataCarrier.GetByCmpOne(PrevPropagationCarrier, "filepath", new Func<object, UDataCarrier, UDataCarrier>( CmpDescEqu), out var filepathC ) )
            {
                strStatusMessage = "not find file path";
                return null;
            }
            if ( UDataCarrier.Get<UserControlLabelingEdit>(MacroInstance.MutableInitialData, null, out var opC) && MacroInstance.MutableInitialData.Handleable )
            {
                if (!MacroInstance.QueryScriptResultOne("R", new Func<object, UDataCarrier, UDataCarrier>( CmpDescEqu ), out var location, out var controlC ) || 
                    !UDataCarrier.Get<Control>(controlC, null, out var rCtrl ))
                {
                    strStatusMessage = "not find right hand side control";
                    return null;
                }

                ResourceManager.InvokeMainThread( new Action( () =>
                {
                    MacroInstance.SetScriptMacroControl( location, "InstallKeyPressCall", UDataCarrier.MakeOne( new Action<Keys>( opC.RxKey ) ) );
                    rCtrl.Controls.Add( opC );
                } ) );

                /*
                if (!UDataCarrier.GetByCmpOne(PrevPropagationCarrier, "R", new Func<object, UDataCarrier, UDataCarrier>(CmpDescEqu), out var controlC ) || 
                    !UDataCarrier.GetDicKeyStrOne<Control>(controlC, null, out var rCtrl))
                {
                    strStatusMessage = "not find right hand side control";
                    return null;
                }

                MacroInstance.OwnerOfScript.MacroSet[ 0 ].OwnerOfPluginClass.SetMacroControl(
                    MacroInstance.OwnerOfScript.MacroSet[ 0 ],
                    UDataCarrier.MakeOne( "InstallKeyPressCall" ),
                    UDataCarrier.MakeOneItemArray( new Action<Keys>( opC.RxKey ) )
                    );

                rCtrl.Controls.Add( opC );
                */
                MacroInstance.MutableInitialData.Handleable = false;
            }

            // check dirty
            if (opC.DirtyOfSaving)
            {
                if ( MessageBox.Show( "Previous changed not save yet! Save them?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
                    opC.SaveChanged();
                else
                    opC.ResetSaveChanged();
            }
            // load image file
            var imagePath = UDataCarrier.Get( filepathC, "" );
            if ( string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                strStatusMessage = $"image file {imagePath} not exist";
                return null;
            }
            // clear region data
            opC.ClearRegionData();
            // load image and load region conf
            opC.LoadImageFile( imagePath ).ReloadLabeledObjects( Path.Combine( Path.GetDirectoryName( imagePath ), $"{Path.GetFileNameWithoutExtension( imagePath )}.txt" ) );
            // check to reload settings
            if ( !opC.HaveSettings )
                opC.HaveSettings = opC.ReloadSettings();
            // invildate
            opC.InvalidDrawing();

            // prepare current propagation
            var propagation = PrevPropagationCarrier.ToList();
            propagation.Remove( filepathC );
            CurrPropagationCarrier = propagation.ToArray();

            // return
            bStatusCode = true;
            return null;
        }
    }
}