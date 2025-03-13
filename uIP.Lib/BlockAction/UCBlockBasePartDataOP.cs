using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        protected UDataCarrierSet _BlockFinalDat = null;
        protected UDataCarrierSet _PrevBlockFinalDat = null;
        protected List<UDataCarrierSet> _BlocksHistoryDat = null;
        protected int _nRunningIndex = -1;

        public UDataCarrierSet BlockRunDat {  get { return _BlockFinalDat; } }
        public UDataCarrierSet PrevBlockRunDat {  set { _PrevBlockFinalDat = value; } }
        public List<UDataCarrierSet> BlocksRunHistoryDat {  set { _BlocksHistoryDat = value; } }
        public int RunningIndex { get { return _nRunningIndex; } set { _nRunningIndex = value; } }

        protected virtual void HandleFinalData()
        {
            _BlockFinalDat?.Dispose();
            _BlockFinalDat = null;
        }

        public void ClearFinalData()
        {
            HandleFinalData();
            AssistantsClearFinalData();
        }
    }
}
