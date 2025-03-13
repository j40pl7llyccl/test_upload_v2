using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uIP.Lib.BlockAction
{
    internal sealed class SampleBlock : UCBlockBase
    {
        private enum SelfState : int
        {
            DummyBeg = 100,
            DummyS01,
            DummyS02,
            DummyEnd
        }
        static SampleBlock()
        {
            lock(_syncStateIntDic)
            {
                _StateIntDic = GetEnumDic(_StateIntDic, typeof(SelfState));
            }
        }
        internal SampleBlock() : base()
        {

            ID = typeof(SampleBlock).FullName;
#if false
            FirstStateInfo( null, PrepareToRun );
            DesignedStateInfo(null, RunState );
            SetHandleDefStateStop( null, HandleStop );
            SetHandleDefStateError( null, HandleError );
            SetHandleDefStateFinish(null, HandleSuccess );
#else
            ConfigWellDefined1Callback( bHandStop: true, bHandError: true, bHandFinish: true );
#endif
        }

        private void PrepareToRun( object context, UDataCarrierSet pParam, bool bFirstEnter, Int32 nCurrState, Int32 nCurrPrevState, List<Int32> pStateHistory, ref Int32 nNextState )
        {

        }
        private void RunState( object context, UDataCarrierSet pParam, bool bFirstEnter, Int32 nCurrState, Int32 nCurrPrevState, List<Int32> pStateHistory, ref Int32 nNextState )
        {

        }
        private bool HandleStop( object context, bool isFirstTime )
        {
            ClearFinalData();
            AssistantsClearFinalData();
            return true;
        }
        private bool HandleError( object context, bool isFirstTime )
        {
            ClearFinalData();
            AssistantsClearFinalData();
            return true;
        }
        private bool HandleSuccess( object context, bool isFirstTime )
        {
            ClearFinalData();
            AssistantsClearFinalData();
            return true;
        }

        protected override void WellDefined1HandleFirstState( object pContext, UDataCarrierSet pParams, bool bFirstEnter, int nCurrState, int nCurrPrevState, List<int> pStateHistory, ref int nNextState )
        {
            // TODO: write code
        }
        protected override void WellDefined1HandleDesignedState( object pContext, UDataCarrierSet pParams, bool bFirstEnter, int nCurrState, int nCurrPrevState, List<int> pStateHistory, ref int nNextState )
        {
            // TODO: write code
        }
        protected override bool WellDefined1HandleErrorState( object context, bool isFirstTime )
        {
            ClearFinalData();
            AssistantsClearFinalData();
            return true;
        }
        protected override bool WellDefined1HandleStopState( object context, bool isFirstTime )
        {
            ClearFinalData();
            AssistantsClearFinalData();
            return true;
        }
        protected override bool WellDefined1HandleFinishState( object context, bool isFirstTime )
        {
            ClearFinalData();
            AssistantsClearFinalData();
            return true;
        }
    }
}
