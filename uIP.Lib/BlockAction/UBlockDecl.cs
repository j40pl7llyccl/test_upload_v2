using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public delegate void fpUCBlockHandleDatCarrier( UDataCarrier pCarrier );
    public delegate void fpUCBlockHandleDatCarrierSet( UDataCarrierSet pCarrierSet );
    public delegate void fpUCBlockRunnerNotify( string blockName, BLOCKRUNNING_STATE state );

    public enum UCBlockBaseRet : int
    {
        RET_STATUS_NG = -1,
        RET_STATUS_OK = 0,
        RET_STATUS_RUNNING,
        RET_STATUS_PAUSED,
        RET_STATUS_PAUSE_RELEASED,
    }

    public enum UCBlockStateReserved : int
    {
        UCBLOCK_STATE_ERROR = -2, // MUST HAVE: error happened
        UCBLOCK_STATE_NA = -1,
        UCBLOCK_STATE_PREPARING = 0, // MUST HAVE: prepare to exec
        UCBLOCK_STATE_FINISH = 1, // MUST HAVE: normal done
        UCBLOCK_STATE_PAUSING = 2, // MUST HAVE: pausing
        UCBLOCK_STATE_PAUSE_RELEASE = 3, // ABANDONED: pause released
        UCBLOCK_STATE_STOP = 4, // MUST HAVE: stop
        UCBLOCK_STATE_DUMMY_RUN = 5, // Alternative
        UCBLOCK_STATE_PROCEEDING = 6,
    }

    public class TUCBlockMutableData : IDisposable
    {
        //public Int32 _nUnit = 0;
        //public Int64 _nSize = 0;
        //public object _pData = null;
        //public UDataCarrier _pData = null;
        //public fpUCBlockHandleDatCarrier _fpMemHandler = null;
        public UDataCarrierSet _pData = null;
        public fpUCBlockHandleDatCarrierSet _fpMemHandler = null;

        public TUCBlockMutableData() {}
        public TUCBlockMutableData(UDataCarrierSet pDat, fpUCBlockHandleDatCarrierSet fp)
        {
            _pData = pDat;
            _fpMemHandler = fp;
        }

        public void Dispose()
        {
            if (_fpMemHandler != null && _pData != null) {
                _fpMemHandler( _pData );
            }
            _pData = null;
            _fpMemHandler = null;
        }
    }

    // return:
    //   - RET_STATUS_RUNNING: not want to change to next state
    //   - RET_STATUS_NG: automatically change to ERROR state
    //   - RET_STATUS_OK: automatically change to END state
    public delegate void fpUCBlockStateCallback( object context, UDataCarrierSet pParam, bool bFirstEnter, Int32 nCurrState, Int32 nCurrPrevState, List<Int32> pStateHistory, ref Int32 nNextState );
    public delegate bool fpUCBlockHandlePrefDefState( object context, bool isFirstTime );
    public delegate void fpUCBlockStateChangedCallback(UCBlockBase pInstance, Int32 nState);
    public delegate void fpUCBlockInformOut(UCBlockBase pInstance, UDataCarrier pOutDat);

    public class TBlockAssistant
    {
        public UCBlockBase _pAssistant = null;
        public bool _bRunningDone = false;
        public bool _bDoneWithError = false;
        public UDataCarrierSet _pParam = null;

        public TBlockAssistant() { }
        public TBlockAssistant(UCBlockBase blk)
        {
            _pAssistant = blk;
        }
    }

    public delegate bool fpUCBlockSet( UDataCarrier pDat );
    //public delegate bool fpUCBlockGet( out UDataCarrier pDat, out fpUCBlockHandleDatCarrier fpHandler );
    public delegate bool fpUCBlockGet( out UDataCarrier pDat );
    public class UCBlockDataCtrl
    {
        public bool _bCanGet = false;
        public bool _bCanSet = false;
        /*
         * 如果要支援, UDataCarrier 中的兩個 delegate 需要提供
         * - fpUDataCarrierXMLReader
         * - fpUDataCarrierXMLWriter
         */        
        public bool _bParam = false;
        public string _strNameOfData = null;
        public UDataCarrierTypeDescription _DataDescription = null;
        public fpUCBlockSet _fpSet = null;
        public fpUCBlockGet _fpGet = null;

        public UCBlockDataCtrl() { }
        public UCBlockDataCtrl(string name, bool bGet, bool bSet, bool bParam, fpUCBlockGet fpGet, fpUCBlockSet fpSet, UDataCarrierTypeDescription desc)
        {
            _strNameOfData = String.IsNullOrEmpty( name ) ? "" : String.Copy( name );
            _bCanGet = bGet;
            _bCanSet = bSet;
            _bParam = bParam;
            _fpGet = fpGet;
            _fpSet = fpSet;
            _DataDescription = desc;
        }
    }

    public delegate void fpBlockRunWorkDoneCallback( UInt32 sn, object context, Int32 status );
    public delegate void fpBlockRunWorkGroupDoneCallback( UInt32 gsn, object context, Int32 status );
    public enum BLOCKRUNNING_STATE : int
    {
        BLOCKRUNNING_STATE_NA = -1,
        BLOCKRUNNING_STATE_WAIT = 0,
        BLOCKRUNNING_STATE_GETWORK,
        BLOCKRUNNING_STATE_RUNNING,
        BLOCKRUNNING_STATE_PAUSING,
        BLOCKRUNNING_STATE_WAIT_PROCEEDING,
        BLOCKRUNNING_STATE_PROC_PROCEEDING,
        BLOCKRUNNING_STATE_PROC_STOP,
        BLOCKRUNNING_STATE_PROC_STOP_DONE,
        BLOCKRUNNING_STATE_RECYCLE_WORK,
        BLOCKRUNNING_STATE_RUNBLOCK_ERROR,
    }
    public enum BLOCKWORK_RETURNCODE : int
    {
        BLOCKWORK_RETURNCODE_NO_ERROR = 0,       // no error
        BLOCKWORK_RETURNCODE_PARAMERR = -1,      // parameter error
        BLOCKWORK_RETURNCODE_NOMEM = -2,         // no memory to alloc
        BLOCKWORK_RETURNCODE_REMOVESN = -3,      // remove sn
        BLOCKWORK_RETURNCODE_BLOCKNOTFOUNF = -4, // BlockAction not found
        BLOCKWORK_RETURNCODE_BLOCKEXECERR = -5,  // BlockAction exec error
        BLOCKWORK_RETURNCODE_STOP = -6,          // cuase by stop
        BLOCKWORK_RETURNCODE_STATEERR = -7,      // cause by one error
        BLOCKWORK_RETURNCODE_PROG_END = -8,      // Program end
    }
    public class WorkOfBlock : IDisposable
    {
        public bool _bGroupBeg = false;
        public UInt32 _nGroup = 0;
        public bool _bGroupEnd = false;
        public List<UCBlockBase> _GroupUsedBlocks = null;

        public UInt32 _nSN = 0;
        public TUCBlockMutableData _pParam = null;
        public string _strBlockId = null;
        public fpBlockRunWorkDoneCallback _fpNotifyBeg = null;
        public object _pNotifyBegContext = null;
        public fpBlockRunWorkDoneCallback _fpNotifyEnd = null;
        public object _pNotifyEndContext = null;

        public object _ContextOfRunGroupEnd = null;
        public fpBlockRunWorkGroupDoneCallback _fpRunGroupEnd = null;

        public WorkOfBlock() { }
        public WorkOfBlock(UInt32 sn, UDataCarrierSet pParam, fpUCBlockHandleDatCarrierSet fpHandParam, string blockId, 
            fpBlockRunWorkDoneCallback fpBegCall, object contextOfBegCall, 
            fpBlockRunWorkDoneCallback fpEndCall, object contextOfEndCall)
        {
            _nSN = sn;
            _pParam = new TUCBlockMutableData( pParam, fpHandParam );
            _strBlockId = blockId;
            _fpNotifyBeg = fpBegCall;
            _pNotifyBegContext = contextOfBegCall;
            _fpNotifyEnd = fpEndCall;
            _pNotifyEndContext = contextOfEndCall;
        }
        public void Dispose()
        {
            if (_pParam != null) {
                _pParam.Dispose();
                _pParam = null;
            }

            _fpNotifyBeg = null;
            _pNotifyBegContext = null;

            _fpNotifyEnd = null;
            _pNotifyEndContext = null;
        }

        public static void FreeFinalData(List<UCBlockBase> blocks)
        {
            if (blocks != null)
            {
                foreach (UCBlockBase block in blocks)
                {
                    block?.ClearFinalData();
                }
            }
        }
    }

    public delegate void fpBlockPollingTimeoutCallback( UDataCarrier context );
    public class BlockPollingWork : IDisposable
    {
        public UDataCarrier _Context = null;
        public fpUCBlockHandleDatCarrier _fpHandleContext = null;
        public fpBlockPollingTimeoutCallback _fpTimeoutCall = null;
        public int _nMsTimeout = 5;
        public DateTime _tmLast = DateTime.Now;
        public int _nNumOfTrigger = -1;

        public BlockPollingWork() { }
        public BlockPollingWork(UDataCarrier context, fpUCBlockHandleDatCarrier fpHandContext, fpBlockPollingTimeoutCallback fpCallback, 
            int nTimeout, int nTrg = -1)
        {
            _Context = context;
            _fpHandleContext = fpHandContext;
            _fpTimeoutCall = fpCallback;
            _nMsTimeout = nTimeout;
            _nNumOfTrigger = nTrg;
        }
        public void Dispose()
        {
            if (_fpHandleContext != null && _Context != null) {
                _fpHandleContext( _Context );
            }
            _fpHandleContext = null;
            _Context = null;
            _fpTimeoutCall = null;
        }
    }

    //public class BlockRunnerParam : IDisposable
    //{
    //    public UDataCarrier _Param = null;
    //    public fpUCBlockHandleDatCarrier _fpHandParam = null;
    //    public string _strBlockId = null;

    //    public BlockRunnerParam() { }
    //    public BlockRunnerParam(UDataCarrier param, fpUCBlockHandleDatCarrier fp, string blockId)
    //    {
    //        _Param = param;
    //        _fpHandParam = fp;
    //        _strBlockId = blockId;
    //    }
    //    public void Dispose()
    //    {
    //        if ( _fpHandParam != null && _Param != null ) _fpHandParam( _Param );
    //        _fpHandParam = null;
    //        _Param = null;
    //        _strBlockId = null;
    //    }
    //}
}
