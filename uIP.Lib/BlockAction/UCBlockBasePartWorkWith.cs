using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uoo.Lib.Block
{
    public partial class UCBlockBase
    {
        protected UInt32 _nRunnerSN = 0;
        public const string strUCB_RUNNER_PARAM = "RunnerParam";
        public const string strUCB_RUNNER_PARAM_POPUI = "PopRunnerParamUi";

        private void InitWorkWith()
        {
            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_RUNNER_PARAM, true, true, true, GetCtrl_RunnerParam, SetCtrl_RunnerParam,
                null ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_RUNNER_PARAM_POPUI, false, true, false, null, SetCtrl_RunnerParamPopUi,
                null ) );
        }

        virtual public bool NewRunnerWork(out WorkOfBlock work)
        {
            work = new WorkOfBlock( 0, null, null, _strID, null, null, null, null );
            return true;
        }
        virtual public void SetRunnerWork(WorkOfBlock work)
        {

        }
        virtual protected void RunnerBegCall( UInt32 sn, object context, Int32 status )
        {

        }
        virtual protected void RunnerEndCall( UInt32 sn, object context, Int32 status )
        {

        }
        virtual protected bool GetCtrl_RunnerParam(out UDataCarrier dat, out fpUCBlockHandleDatCarrier fp)
        {
            dat = null; fp = null; return false;
        }
        virtual protected bool SetCtrl_RunnerParam(UDataCarrier dat)
        {
            return false;
        }
        virtual protected bool SetCtrl_RunnerParamPopUi(UDataCarrier dat)
        {
            return false;
        }
    }
}
