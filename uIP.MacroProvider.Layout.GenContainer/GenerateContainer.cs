using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.Layout.GenContainer
{
    public class GenerateContainer : UMacroMethodProviderPlugin
    {
        const string Method_CreateFormLayout3 = "CreateFormLayout3";
        const string KeyDisplayForm = "DisplayForm";

        public GenerateContainer() { }

        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            // add opened method
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    NameOfCSharpDefClass,
                    Method_CreateFormLayout3,
                    CreateFormLayout3,
                    null,
                    null,
                    null,
                    new UDataCarrierTypeDescription[] { 
                        new UDataCarrierTypeDescription( typeof(Control), "Control to display"),
                        new UDataCarrierTypeDescription( typeof(Control), "...")
                    },
                    null
                )
                {  Invisible = true }
            );

            // create macro done call
            m_createMacroDoneFromMethod.Add( Method_CreateFormLayout3, MacroShellDoneCall );

            m_MacroControls.Add(
                "InstallKeyPressCall",
                new UScriptControlCarrierMacro(
                    "InstallKeyPressCall", false, true, false,
                    new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( Action<Keys> ), "Install delegate" ) },
                    null, IoctrlSet_InstallKeyPress )
            );

            // create macro ioctl

            m_bOpened = true;
            return true;
        }

        private bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == Method_CreateFormLayout3 )
            {
                var form = ResourceManager.InvokeMainThread( new Func<object, object>( ctx => new FormLayout3() ), null ) as FormLayout3;
                form.RunWith = instance;
                // create variable to store and mark as handleable
                form?.ChangeSize( 1920, 1030 );
                form?.LayoutSplitterContainer( 1, 4, 1, 1 );

                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                {
                    { KeyDisplayForm, UDataCarrier.MakeOne( form, true) }

                }, true );

                instance.AbilityToInteractWithUI = true;
            }
            return true;
        }

        private bool IoctrlSet_InstallKeyPress( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( whichMacro.MethodName == Method_CreateFormLayout3 )
            {
                if ( UDataCarrier.GetByIndex<Action<Keys>>(data, 0, null, out var got) && 
                    UDataCarrier.Get<Dictionary<string, UDataCarrier>>(whichMacro.MutableInitialData, null, out var md) )
                {
                    if (md.TryGetValue( KeyDisplayForm, out var fc ) && UDataCarrier.Get<FormLayout3>( fc, null, out var frm ))
                    {
                        //ResourceManager.InvokeMainThread( new Func<object, object>( ctx =>
                        //{
                            frm.KeyPress.Add( got );
                        //    return null;
                        //} ), null );
                    }
                }
                return true;
            }

            return false;
        }

        private UDataCarrier[] CreateFormLayout3( UMacro MacroInstance,
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
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( MacroInstance?.MutableInitialData ?? null, null, out var conv ) || conv == null )
            {
                bStatusCode = false;
                strStatusMessage = "Mutable data convert fail";
                return null;
            }

            if (!conv.TryGetValue(KeyDisplayForm, out var formC) || !UDataCarrier.Get<FormLayout3>(formC, null, out var form) || form == null)
            {
                bStatusCode = false;
                strStatusMessage = "Mutable data not contain form instance";
                return null;
            }

            if ( !form.Visible )
                form.Show();

            CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray( form.LeftTopContainer, form.LeftBottomContainer, form.RightContainer, form );
            CurrPropagationCarrier[ 0 ].Desc = "LT";
            CurrPropagationCarrier[ 1 ].Desc = "LB";
            CurrPropagationCarrier[ 2 ].Desc = "R";
            CurrPropagationCarrier[ 3 ].Desc = "Owner";


            bStatusCode = true;
            var ret = UDataCarrier.MakeVariableItemsArray( form.LeftTopContainer, form.LeftBottomContainer, form.RightContainer, form );
            ret[ 0 ].Desc = "LT";
            ret[ 1 ].Desc = "LB";
            ret[ 2 ].Desc = "R";
            ret[ 3 ].Desc = "Owner";
            return ret;
        }
    }
}
