using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Result
{
    public class uMProviderSopResult : UMacroMethodProviderPlugin
    {
        const string BufferResultMethodName = "SopBufferResultMethod";
        const string ImageSetResultMethodName = "SopImageSetResultMethod";

        public uMProviderSopResult() { }

        public override bool Initialize( UDataCarrier[] param )
        {
            throw new NotImplementedException();
        }

        protected override UMacro NewMacroInstance( UMacro reference )
        {
            // create jump macro
            if ( reference.MethodName == BufferResultMethodName || reference.MethodName == ImageSetResultMethodName )
            {
                return new UMacroCapableOfCtrlFlow(
                    this, string.Copy( GetType().FullName ?? string.Empty ), string.Copy( reference.MethodName ), reference.fpHandler,
                    reference.ImmutableParamTypeDesc, reference.VariableParamTypeDesc,
                    reference.PrevPropagationParamTypeDesc, reference.RetPropagationParamTypeDesc,
                    reference.RetResultTypeDesc );
            }
            return base.NewMacroInstance( reference );
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
            return null;
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
            return null;
        }
    }
}