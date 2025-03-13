using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Utility;
using System.Windows.Forms;

namespace uIP.MacroProvider.Commons.FlowControl
{
    public class FlowCtrlProvider : UMacroMethodProviderPlugin
    {
        internal const string MethodName_JumpToBeginWithControl = "JumpToBeginWithControl";
        internal const string Key_CreatedUiControl = "CreatedUiControl";
        internal const string Key_EvtTriggerEnd = "TriggerEndEvt";

        internal const string JumpIndexOrScriptMethodName = "JumpIndexOrScript";
        internal const string DummyMethodName = "Dummy";

        public FlowCtrlProvider() { }
        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null, m_strCSharpDefClassName, MethodName_JumpToBeginWithControl, JumpBeginWithUiCtrl,
                    null, null, null, null, null
                )
            );

            // create macro done call
            m_createMacroDoneFromMethod.Add( MethodName_JumpToBeginWithControl, MacroShellDoneCall );

            // create jump index/script
            m_UserQueryOpenedMethods.Add(
                new UMacro( null, NameOfCSharpDefClass, JumpIndexOrScriptMethodName, JumpIndexOrScript,
                null, null, null, null ) );

            // create dummy
            m_UserQueryOpenedMethods.Add(
                new UMacro( null, NameOfCSharpDefClass, DummyMethodName, Dummy,
                null, null, null, null ) );

            m_createMacroDoneFromMethod.Add( JumpIndexOrScriptMethodName, MacroShellDoneCall );
            m_macroMethodConfigPopup.Add( JumpIndexOrScriptMethodName, PopupMacroConfigDialog );
            m_macroMethodSettingsGet.Add( JumpIndexOrScriptMethodName, GetMacroMethodSettings );
            m_macroMethodSettingsSet.Add( JumpIndexOrScriptMethodName, SetMacroMethodSettings );

            //m_MacroControls.Add(
            //    UMacroProviderDefaultSupports.NewMacroJoinCmd,
            //    UMacroProviderDefaultSupports.CreateNewMacroJoin( ProcSetMacroChanged ) );
            //m_MacroControls.Add(
            //    UMacroProviderDefaultSupports.MacroReplaceCmd,
            //    UMacroProviderDefaultSupports.CreateMacroReplace( ProcSetMacroChanged ) );
            //m_MacroControls.Add(
            //    UMacroProviderDefaultSupports.MacroRemoveCmd,
            //    UMacroProviderDefaultSupports.CreateMacroRemove( ProcSetMacroChanged ) );

            m_bOpened = true;
            return true;
        }
        protected override UMacro NewMacroInstance( UMacro reference )
        {
            if ( reference.MethodName == MethodName_JumpToBeginWithControl || reference.MethodName == JumpIndexOrScriptMethodName )
            {
                var ret = new UMacroCapableOfCtrlFlow(
                    this, string.Copy( GetType().FullName ?? string.Empty ), reference.MethodName, reference.fpHandler,
                    reference.ImmutableParamTypeDesc, reference.VariableParamTypeDesc,
                    reference.PrevPropagationParamTypeDesc, reference.RetPropagationParamTypeDesc,
                    reference.RetResultTypeDesc );

                if ( reference.MethodName == MethodName_JumpToBeginWithControl )
                {
                    ret.MustJump = true;
                    ret.Jump2WhichMacro = 0;
                    ret.AbilityToJumpIndex = true;
                }

                return ret;
            }
            return base.NewMacroInstance( reference );
        }

        private bool MacroShellDoneCall( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == MethodName_JumpToBeginWithControl )
            {
                // create User Control
                var usrCtrl = ResourceManager.InvokeMainThread( new Func<object, object>( ctx => new UserControlJumpEnd() { RunWith = ctx as UMacro } ), instance ) as UserControlJumpEnd;

                var manualResetEvt = EventWinSdkFunctions.Create( true, false, $"fc_{CommonUtilities.GetCurrentTimeStr()}" );
                //var manualResetEvt = EventWinSdkFunctions.CreateEventWithConfSecurity( true, false, $"fc_{CommonUtilities.GetCurrentTimeStr()}" );
                var manualEvtCarr = UDataCarrier.MakeOne( manualResetEvt, true );
                manualEvtCarr.fpDataHandler = new Action<object>( CloseEvt );

                instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                {
                    { Key_CreatedUiControl, UDataCarrier.MakeOne( usrCtrl, true) },
                    { Key_EvtTriggerEnd, manualEvtCarr }
                }, true );

                instance.SetOnEndWaitEvent( manualResetEvt, UInt32.MaxValue, 1000 );
                instance.AbilityToInteractWithUI = true;
            }
            else if (callMethodName == JumpIndexOrScriptMethodName )
            {
                instance.MutableInitialData = UDataCarrier.MakeOne( new JumpSameScriptParams() );
            }
            return true;
        }

        private Form PopupMacroConfigDialog( string callMethodName, UMacro macroToConf )
        {
            if ( callMethodName == JumpIndexOrScriptMethodName )
            {
                return new FormSetupJumpInScript() { RunWith = macroToConf }.UpdateToUI();
            }

            return null;
        }

        private bool GetMacroMethodSettings( UMacro m, out object settings, out Type t )
        {
            settings = null;
            t = null;
            if ( m.MethodName == JumpIndexOrScriptMethodName )
            {
                settings = m.MutableInitialData.Data;
                t = m.MutableInitialData.Tp;
                return true;
            }

            return false;
        }

        private bool SetMacroMethodSettings( UMacro m, object settings )
        {
            if ( settings != null && settings.GetType() == typeof( JumpSameScriptParams ) )
            {
                var conf = settings as JumpSameScriptParams;
                m.MutableInitialData = UDataCarrier.MakeOne( conf );
                JumpSameScriptParams.ConfigMacroJump( m as UMacroCapableOfCtrlFlow, conf );
                return true;
            }

            return false;
        }

        /*
        private void CheckHaveGotoMacro( List<UMacro> macros )
        {
            if ( macros == null || macros.Count <= 0 )
                return;

            bool bJumpInScript = false;
            var script = macros[ 0 ].OwnerOfScript;
            if ( script == null )
                return;

            foreach ( var m in macros )
            {
                //if (UDataCarrier.GetDicKeyStrOne<JumpSameScriptParams>(m.MutableInitialData, null, out var conf))
                //{
                //    if (conf.JumpType == JumpSameScriptParams.TypeOfJump.JumpToIndex || conf.JumpType == JumpSameScriptParams.TypeOfJump.JumpByCallRetIndex)
                //    {
                //        bJumpInScript = true;
                //        break;
                //    }
                //}
                if ( m is UMacroCapableOfCtrlFlow )
                {
                    bJumpInScript = true;
                    break;
                }
            }

            //script.EnableGotoFunc = bJumpInScript;
        }

        bool ProcSetMacroChanged( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( !UDataCarrier.GetByIndex<List<UMacro>>( data, 2, null, out var macros ) )
                return false;

            CheckHaveGotoMacro( macros );
            return true;
        }
        */

        static void CloseEvt(object ctx)
        {
            IntPtr ptr = ( IntPtr )ctx;
            EventWinSdkFunctions.Close( ptr );
        }

        static UDataCarrier FindInDesc(object ctx, UDataCarrier current)
        {
            if ( current == null || ctx == null ) return null;
            if ( ctx is string s && s == current.Desc )
                return current;

            return null;
        }

        private UDataCarrier[] JumpBeginWithUiCtrl( UMacro MacroInstance,
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
            // get mutable data
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( MacroInstance.MutableInitialData, null, out var keep ) )
            {
                strStatusMessage = "cannot convert mutable data";
                return null;
            }
            // get user control
            if (keep.TryGetValue(Key_CreatedUiControl, out var uiCtrlCarr) && uiCtrlCarr.Handleable)
            {
                if ( MacroInstance.QueryScriptResultOne("LB", new Func<object, UDataCarrier, UDataCarrier>( FindInDesc ), out var location, out var containerC ) &&
                    UDataCarrier.Get<Control>(containerC, null, out var container) && container != null &&
                    UDataCarrier.Get<UserControlJumpEnd>( uiCtrlCarr, null, out var cc ) && cc != null )
                {
                    ResourceManager.InvokeMainThread( () =>
                    {
                        container.Controls.Add(cc);
                        MacroInstance.SetScriptMacroControl( location, "InstallKeyPressCall", UDataCarrier.MakeOne( new Action<Keys>( cc.RxKey ) ) );
                    } );

                    uiCtrlCarr.Handleable = false;
                }
            }

            // get event and reset
            if (!keep.TryGetValue(Key_EvtTriggerEnd, out var evtCarr))
            {
                strStatusMessage = "not find event carrier";
                return null;
            }
            EventWinSdkFunctions.Reset( ( IntPtr )evtCarr.Data );

            bStatusCode = true;
            return null;
        }

        private UDataCarrier[] JumpIndexOrScript( UMacro MacroInstance,
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
            if ( !UDataCarrier.Get<JumpSameScriptParams>( MacroInstance.MutableInitialData, null, out var conf ) || conf == null )
            {
                strStatusMessage = "invalid mutable variable";
                return null;
            }

            if ( !( MacroInstance is UMacroCapableOfCtrlFlow fcm ) )
            {
                strStatusMessage = "invalid macro type";
                return null;
            }

            fcm.MustJump = false;
            if ( conf.JumpType == JumpSameScriptParams.TypeOfJump.JumpToIndex )
            {
                fcm.MustJump = true;
                fcm.Jump2WhichMacro = conf.WhichIndex;
            }
            else if ( conf.JumpType == JumpSameScriptParams.TypeOfJump.JumpToScript )
            {
                if ( string.IsNullOrEmpty( conf.WhichScriptToJump ) )
                {
                    strStatusMessage = "no script to jump";
                    return null;
                }
                // config end current script
                fcm.MustJump = true;
                fcm.Jump2WhichMacro = ( int )MacroGotoFunctions.GOTO_END;
                // config jump script
                fcm.Iteration = MacroJumpingFunctions.JUMPING_TO_ANOTHER_SCRIPT_KEEPING_HISTORY_CARRIERS;
                fcm.IteratorData = new AnotherMacroJumpingInfo()
                {
                    _FeedingPrevPropagationCarrierSet = PrevPropagationCarrier,
                    _nBeginIndexOfTrgMacro = 0,
                    _strNameOfScript = conf.WhichScriptToJump,
                };
            }
            else if ( conf.JumpType == JumpSameScriptParams.TypeOfJump.JumpByCallRetIndex )
            {
                var ret = ULibAgent.CallPluginClassOpenedFuncRetValue( conf.CallWhichPluginFullName, conf.CallWhichPluginOfFunc, out var callStatus, UDataCarrier.MakeOne( historyResultCarriers ), UDataCarrier.MakeOne( PrevPropagationCarrier ) );
                if ( !callStatus )
                {
                    strStatusMessage = $"Call {conf.CallWhichPluginOfFunc} of {conf.CallWhichPluginFullName} fail";
                    return null;
                }
                if ( !UDataCarrier.Get( ret, 0, out var index ) )
                {
                    strStatusMessage = $"Call {conf.CallWhichPluginOfFunc} of {conf.CallWhichPluginFullName} return type is not int";
                    return null;
                }
                fcm.MustJump = true;
                fcm.Jump2WhichMacro = index;
            }
            else if ( conf.JumpType == JumpSameScriptParams.TypeOfJump.JumpByCallRetScriptName )
            {
                var ret = ULibAgent.CallPluginClassOpenedFuncRetValue( conf.CallWhichPluginFullName, conf.CallWhichPluginOfFunc, out var callStatus, UDataCarrier.MakeOne( historyResultCarriers ), UDataCarrier.MakeOne( PrevPropagationCarrier ) );
                if ( !callStatus )
                {
                    strStatusMessage = $"Call {conf.CallWhichPluginOfFunc} of {conf.CallWhichPluginFullName} fail";
                    return null;
                }
                // return type must UDataCarrier[]
                // [0]: name of script
                // [1]: begin index
                // [2..n]: propagations
                if ( !UDataCarrier.Get<UDataCarrier[]>( ret, null, out var toNext ) )
                {
                    strStatusMessage = $"Call {conf.CallWhichPluginOfFunc} of {conf.CallWhichPluginFullName} return type is not UDataCarrier[]";
                    return null;
                }
                if ( !UDataCarrier.GetByIndex( toNext, 0, "", out var nameOfScript ) || string.IsNullOrEmpty( nameOfScript ) )
                {
                    strStatusMessage = "not get script name";
                    return null;
                }
                if ( !UDataCarrier.GetByIndex( toNext, 1, -1, out var begIdx ) || begIdx < 0 )
                {
                    strStatusMessage = "invalid begin index";
                    return null;
                }

                List<UDataCarrier> prop = new List<UDataCarrier>();
                for ( int i = 2; i < toNext.Length; i++ )
                {
                    prop.Add( toNext[ i ] );
                }

                // conf end current script
                fcm.MustJump = true;
                fcm.Jump2WhichMacro = ( int )MacroGotoFunctions.GOTO_END;
                // config jump script
                fcm.Iteration = MacroJumpingFunctions.JUMPING_TO_ANOTHER_SCRIPT;
                fcm.IteratorData = new AnotherMacroJumpingInfo()
                {
                    _FeedingPrevPropagationCarrierSet = prop.ToArray(),
                    _nBeginIndexOfTrgMacro = begIdx,
                    _strNameOfScript = nameOfScript
                };
            }
            else
            {
                strStatusMessage = $"not implement type {conf.JumpType}";
                return null;
            }

            bStatusCode = true;
            return null;
        }
        private UDataCarrier[] Dummy( UMacro MacroInstance,
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
            fpLog?.Invoke( eLogMessageType.NORMAL, 0, $"{MacroInstance.OwnerOfScript.NameOfId} {GetType().Name}::Dummy exec" );
            bStatusCode = true;
            return null;
        }
    }
}
