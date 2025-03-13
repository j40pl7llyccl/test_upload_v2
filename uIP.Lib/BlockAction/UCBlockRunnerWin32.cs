using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO;

using uIP.Lib.DataCarrier;
using uIP.Lib.InterPC;
using uIP.Lib.Utility;

namespace uIP.Lib.BlockAction
{
    /// <summary>
    /// List layer
    /// - 1st: flatten work with one-by-one exec
    /// - 2nd: marged in a single pack and reload one-by-one to flatten; multiple works in a pack will treat as group
    /// 
    /// Suggest
    /// - add to 2nd to make packed work(s) sequence
    /// - Infinite work reload directly to 1st and create one runner to exec.
    /// 
    /// Remark
    /// - Grouping works will release final data of block at ERROR and last one to prevent memory pressure
    /// </summary>
    public class UCBlockRunnerWin32 : IDisposable, IPollingRunner
    {
        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;
        public bool IsDispose {  get { return m_bDisposing || m_bDisposed; } }

        protected string _strGivenName = null;
        protected UCBlockManager _pBlockManager = null;
        protected fpLogMessage _fpLog = null;

        protected ManualResetEvent _hEvent;
        protected object _hSyncMethod;
        protected object _hSyncRunList;
        protected UInt32 _nWorkingSN = 1;
        protected UInt32 _nGroupSN = 1;
        protected List<WorkOfBlock> _WorkList = new List<WorkOfBlock>();

        protected object _hSync2ndList = new object();
        protected List<WorkOfBlock[]> _2ndWorkList = new List<WorkOfBlock[]>();

        public string GivenName {  get { return _strGivenName; } }

        internal UCBlockManager BlockManager {  get { return _pBlockManager; } }

        protected fpUCBlockRunnerNotify _fpBlockRunStateChangeNotify = null;
        public fpUCBlockRunnerNotify BlockRunStateChangeCall {  get { return _fpBlockRunStateChangeNotify; } set { _fpBlockRunStateChangeNotify = value; } }

        public UCBlockRunnerWin32()
        {
            _pBlockManager = new UCBlockManager();
            InitRes();
        }
        public UCBlockRunnerWin32(
            string givenNm,
            fpLogMessage log,
            IPipeClientComm pipeClient, 
            UCWin32SharedMemFormating formatSharedMem,
            UCDataSyncW32<Int32> i32ShMem,
            UCDataSyncW32<Int64> i64ShMem,
            UCDataSyncW32<double> dfShMem,
            UCDataSyncW32<Int32> i32ShMemPermanent,
            UCDataSyncW32<Int64> i64ShMemPermanent,
            UCDataSyncW32<double> dfShMemPermanent )
        {
            _strGivenName = String.IsNullOrEmpty( givenNm ) ? "" : String.Copy( givenNm );
            _pBlockManager = new UCBlockManager( log, pipeClient, formatSharedMem, i32ShMem, i64ShMem, dfShMem, i32ShMemPermanent, i64ShMemPermanent, dfShMemPermanent );
            _fpLog = log;

            InitRes();
        }
        private void InitRes()
        {
            _hEvent = new ManualResetEvent( false );
            _hSyncMethod = new object();
            _hSyncRunList = new object();
            _hPauseEvent = new AutoResetEvent( false );
            _hProceedEvent = new AutoResetEvent( false );
            _hStopEvent = new AutoResetEvent( false );

            _RunningThread = new Thread( new ThreadStart( Run ) );
            _RunningThread.Priority = ThreadPriority.BelowNormal;
            _RunningThread.Start();

            _PollingThread = new Thread( new ThreadStart( Polling ) );
            _PollingThread.Start();
        }
        public void Dispose()
        {
            if ( IsDispose ) return;
            m_bDisposing = true;

            Dispose( false );

            m_bDisposed = true;
            m_bDisposing = false;
        }
        protected virtual void Dispose(bool disposing)
        {
            // terminate polling thread & clear polling work
            _PollingThread.Join();
            _PollingThread = null;
            ClearPollingWorks();

            // terminate running thread & clear work
            _hEvent.Set();
            _RunningThread.Join();
            _RunningThread = null;
            ClearWorkList( ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_PROG_END );

            Clear2ndList( ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_PROG_END );

            // free block manager
            _pBlockManager.Dispose();
            _pBlockManager = null;

            //
            // free resources
            //
            _hEvent.Dispose();
            _hEvent = null;

            _hPauseEvent.Dispose();
            _hPauseEvent = null;

            _hProceedEvent.Dispose();
            _hProceedEvent = null;

            _hStopEvent.Dispose();
            _hStopEvent = null;
        }

        virtual public string AddBlock(Assembly dll, string typeName, string createdGivenName = null)
        {
            if ( IsDispose ) return null;
            if ( dll == null ) return null;
            UCBlockBase block = null;
            Type[] types = dll.GetTypes();
            Type ctp = null;
            if ( types == null ) return null;
            for(int i=0; i < types.Length; i++ ) {
                if (types[i].FullName.LastIndexOf(typeName) >= 0) {
                    if (types[i].IsSubclassOf(typeof(UCBlockBase))) {
                        try { block = Activator.CreateInstance( types[ i ] ) as UCBlockBase; }
                        catch { block = null; }
                        ctp = types[ i ];
                        break;
                    }
                }
            }
            if ( block == null ) return null;
            block.ID = String.IsNullOrEmpty( createdGivenName ) ? String.Copy( ctp.FullName ) : String.Copy( createdGivenName );

            Monitor.Enter( _hSyncMethod );
            try {
                if ( !_pBlockManager.AddBlock( block ) ) {
                    block.Dispose();
                    return null;
                }
                return block.ID;
            } finally {
                Monitor.Exit( _hSyncMethod );
            }
        }
        virtual public bool AddBlock(UCBlockBase block, bool bHandleByThisClass = true)
        {
            if ( IsDispose || block == null ) return false;

            Monitor.Enter( _hSyncMethod );
            try {
                if ( !_pBlockManager.AddBlock( block ) ) {
                    if ( bHandleByThisClass ) {
                        block.Dispose();
                    }
                    return false;
                }
                return true;
            }finally { Monitor.Exit( _hSyncMethod ); }
        }
        virtual public bool RemoveBlock(string id)
        {
            if ( IsDispose ) return false;
            Monitor.Enter( _hSyncMethod );
            try { _pBlockManager.RemoveBlock( id ); } finally { Monitor.Exit( _hSyncMethod ); }
            return true;
        }
        virtual public bool SetBlock( string blockId, string nameOfSet, UDataCarrier dat )
        {
            if ( IsDispose ) return false;
            if ( nameOfSet == UCBlockBase.strUCB_LOG && dat != null && dat.Data != null ) {
                fpLogMessage log = dat.Data as fpLogMessage;
                if ( log != null ) _fpLog = log;
            }
            bool ret = false;
            Monitor.Enter( _hSyncMethod );
            try { ret = _pBlockManager.BlockSet( blockId, nameOfSet, dat ); } finally { Monitor.Exit( _hSyncMethod ); }
            return ret;
        }
        virtual public bool GetBlock(string blockId, string nameOfGet, out UDataCarrier dat)
        {
            dat = null;
            if ( IsDispose ) return false;
            bool ret = false;
            Monitor.Enter( _hSyncMethod );
            try { ret = _pBlockManager.BlockGet( blockId, nameOfGet, out dat ); } finally { Monitor.Exit( _hSyncMethod ); }
            return ret;
        }
        virtual public bool WriteSettings(string folder)
        {
            if ( IsDispose || !Directory.Exists(folder) ) return false;
            Monitor.Enter( _hSyncMethod );
            try {
                foreach(KeyValuePair<string, UCBlockItem> kv in _pBlockManager.Blocks) {
                    if ( String.IsNullOrEmpty( kv.Key ) || kv.Value == null || kv.Value._Block == null )
                        continue;
                    string path = Path.Combine( folder, kv.Key );
                    if ( !CommonUtilities.RCreateDir( path ) )
                        continue;

                    string filenm = String.Format( "{0}.xml", kv.Value._Block.GetType().Name );
                    if ( !kv.Value._Block.WriteData( path, filenm ) && _fpLog != null )
                        _fpLog( eLogMessageType.NORMAL, 0, string.Format( "[UCBlockRunnerWin32::WriteSettings] call block {0} write error!", kv.Key ) );
                }
            } finally { Monitor.Exit( _hSyncMethod ); }
            return true;
        }
        virtual public bool ReadSettings(string folder)
        {
            if ( IsDispose || !Directory.Exists( folder ) ) return false;
            Monitor.Enter( _hSyncMethod );
            try {
                foreach(KeyValuePair<string, UCBlockItem>kv in _pBlockManager.Blocks) {
                    if ( String.IsNullOrEmpty( kv.Key ) || kv.Value == null || kv.Value._Block == null )
                        continue;
                    string path = Path.Combine( folder, kv.Key );
                    if (!Directory.Exists(path)) {
                        if ( _fpLog != null )
                            _fpLog( eLogMessageType.NORMAL, 0, String.Format( "[ UCBlockRunnerWin32::WriteSettings ] call block {0} not find paht {1}", kv.Key, path) );
                        continue;
                    }

                    string filenm = String.Format( "{0}.xml", kv.Value._Block.GetType().Name );
                    if (!kv.Value._Block.ReadData(folder, filenm) && _fpLog != null )
                        _fpLog( eLogMessageType.NORMAL, 0, string.Format( "[UCBlockRunnerWin32::ReadSettings] call block {0} read error!", kv.Key ) );
                }
            } finally { Monitor.Exit( _hSyncMethod ); }
            return true;
        }

        virtual public UInt32 AddWork(string blockId, UDataCarrierSet param, fpUCBlockHandleDatCarrierSet fpHandParam,
            fpBlockRunWorkDoneCallback fpNotifyBeg, object contextOfBeg,
            fpBlockRunWorkDoneCallback fpNotifyEnd, object contextOfEnd,
            bool bHandleOnError = true)
        {
            if ( IsDispose || String.IsNullOrEmpty(blockId) ) {
                if ( fpNotifyBeg != null ) fpNotifyBeg( 0, contextOfBeg, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_PARAMERR );
                if (bHandleOnError) {
                    if ( fpHandParam != null ) fpHandParam( param );
                }
                return 0;
            }

            // add work
            UInt32 nCurrSN = 0;
            Monitor.Enter( _hSyncRunList );
            try {
                nCurrSN = _nWorkingSN; // assign SN
                _nWorkingSN = _nWorkingSN == UInt32.MaxValue ? 1 : _nWorkingSN + 1; // inc sn
                WorkOfBlock pWork = new WorkOfBlock( nCurrSN, param, fpHandParam, blockId, fpNotifyBeg, contextOfBeg, fpNotifyEnd, contextOfEnd );
                _WorkList.Add( pWork );
            }finally {
                Monitor.Exit( _hSyncRunList );
            }

            // trigger event
            _hEvent.Set();
            return nCurrSN;
        }

        virtual public UInt32 AddWork2nd( string blockId, UDataCarrierSet param, fpUCBlockHandleDatCarrierSet fpHandParam,
            fpBlockRunWorkDoneCallback fpNotifyBeg, object contextOfBeg,
            fpBlockRunWorkDoneCallback fpNotifyEnd, object contextOfEnd,
            bool bHandleOnError = true )
        {
            if ( IsDispose || String.IsNullOrEmpty( blockId ) ) {
                if ( fpNotifyBeg != null ) fpNotifyBeg( 0, contextOfBeg, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_PARAMERR );
                if ( bHandleOnError ) {
                    if ( fpHandParam != null ) fpHandParam( param );
                }
                return 0;
            }

            // add work
            UInt32 nCurrSN = 0;
            Monitor.Enter( _hSync2ndList );
            try {
                nCurrSN = _nWorkingSN; // assign SN
                _nWorkingSN = _nWorkingSN == UInt32.MaxValue ? 1 : _nWorkingSN + 1; // inc sn
                WorkOfBlock pWork = new WorkOfBlock( nCurrSN, param, fpHandParam, blockId, fpNotifyBeg, contextOfBeg, fpNotifyEnd, contextOfEnd );
                _2ndWorkList.Add(new WorkOfBlock[] { pWork });
            } finally {
                Monitor.Exit( _hSync2ndList );
            }

            // trigger event
            if (_WorkList.Count <= 0 ) _hEvent.Set();
            return nCurrSN;
        }

        private List<UCBlockBase> GetBlockByIDs(string[] blocks, out bool status)
        {
            status = false;
            List<UCBlockBase> used = new List<UCBlockBase>();
            foreach ( var block in blocks )
            {
                var b = _pBlockManager.GetBlock( block );
                if ( b != null )
                    return used;
                if ( !used.Contains( b ) )
                    used.Add( b );
            }
            status = true;
            return used;
        }

        virtual public bool AddWorkList( string[] blocks, UDataCarrierSet[] runParams, fpUCBlockHandleDatCarrierSet[] fpHandRunParams,
            fpBlockRunWorkDoneCallback[] fpNotifyBegs, object[] contextOfBegs,
            fpBlockRunWorkDoneCallback[] fpNotifyEnds, object[] contextOfEnds,
            fpBlockRunWorkGroupDoneCallback fpGroupDoneCall, object contextOfGCall,
            out UInt32[] Sns, out UInt32 groupSN )
        {
            Sns = null;
            groupSN = 0;
            if ( IsDispose ) return false;
            if ( blocks == null || runParams == null || fpHandRunParams == null || fpNotifyBegs == null || contextOfBegs == null || fpNotifyEnds == null || contextOfEnds == null )
                return false;
            if ( blocks.Length != runParams.Length || runParams.Length != fpHandRunParams.Length || fpHandRunParams.Length != fpNotifyBegs.Length || fpNotifyBegs.Length != contextOfBegs.Length ||
                contextOfBegs.Length != fpNotifyEnds.Length || fpNotifyEnds.Length != contextOfEnds.Length )
                return false;
            List<UInt32> ret = new List<UInt32>();
            List<UCBlockBase> used = GetBlockByIDs(blocks, out var allFound);
            if ( !allFound )
                return false;

            Monitor.Enter( _hSyncRunList );
            try {
                UInt32 gsn = _nGroupSN;
                _nGroupSN = UInt32.MaxValue == _nGroupSN ? 1 : _nGroupSN + 1; // inc group SN
                for ( int i = 0; i < blocks.Length; i++ ) {
                    UInt32 nCurrSN = _nWorkingSN; // assign current SN
                    _nWorkingSN = _nWorkingSN == UInt32.MaxValue ? 1 : _nWorkingSN + 1; // inc current SN
                    WorkOfBlock pWork = new WorkOfBlock( nCurrSN, runParams[ i ], fpHandRunParams[ i ], blocks[ i ], fpNotifyBegs[ i ], contextOfBegs[ i ], fpNotifyEnds[ i ], contextOfEnds[ i ] );
                    pWork._ContextOfRunGroupEnd = contextOfGCall;
                    pWork._fpRunGroupEnd = fpGroupDoneCall;
                    pWork._nGroup = gsn;
                    pWork._GroupUsedBlocks = used;
                    if ( i == 0 ) pWork._bGroupBeg = true;
                    else if ( i == ( blocks.Length - 1 ) ) pWork._bGroupEnd = true;
                    _WorkList.Add( pWork );
                    ret.Add( nCurrSN );
                }
            } finally { Monitor.Exit( _hSyncRunList ); }

            Sns = ret.ToArray();
            _hEvent.Set();
            return true;
        }

        virtual public bool AddWorkList2nd( string[] blocks, UDataCarrierSet[] runParams, fpUCBlockHandleDatCarrierSet[] fpHandRunParams,
            fpBlockRunWorkDoneCallback[] fpNotifyBegs, object[] contextOfBegs,
            fpBlockRunWorkDoneCallback[] fpNotifyEnds, object[] contextOfEnds, 
            fpBlockRunWorkGroupDoneCallback fpGroupDoneCall, object contextOfGCall,
            out UInt32[] Sns, out UInt32 groupSN )
        {
            Sns = null;
            groupSN = 0;
            if ( IsDispose ) return false;
            if ( blocks == null || runParams == null || fpHandRunParams == null || fpNotifyBegs == null || contextOfBegs == null || fpNotifyEnds == null || contextOfEnds == null )
                return false;
            if ( blocks.Length != runParams.Length || runParams.Length != fpHandRunParams.Length || fpHandRunParams.Length != fpNotifyBegs.Length || fpNotifyBegs.Length != contextOfBegs.Length ||
                contextOfBegs.Length != fpNotifyEnds.Length || fpNotifyEnds.Length != contextOfEnds.Length )
                return false;
            List<UInt32> ret = new List<UInt32>();
            List<WorkOfBlock> blks = new List<WorkOfBlock>();
            List<UCBlockBase> used = GetBlockByIDs( blocks, out var allFound );
            if ( !allFound )
                return false;

            Monitor.Enter( _hSync2ndList );
            try {
                UInt32 gsn = _nGroupSN;
                groupSN = gsn;
                _nGroupSN = UInt32.MaxValue == _nGroupSN ? 1 : _nGroupSN + 1; // inc group SN
                for ( int i = 0; i < blocks.Length; i++ ) {
                    UInt32 nCurrSN = _nWorkingSN; // assign current SN
                    _nWorkingSN = _nWorkingSN == UInt32.MaxValue ? 1 : _nWorkingSN + 1; // inc current SN
                    WorkOfBlock pWork = new WorkOfBlock( nCurrSN, runParams[ i ], fpHandRunParams[ i ], blocks[ i ], fpNotifyBegs[ i ], contextOfBegs[ i ], fpNotifyEnds[ i ], contextOfEnds[ i ] );
                    pWork._ContextOfRunGroupEnd = contextOfGCall;
                    pWork._fpRunGroupEnd = fpGroupDoneCall;
                    pWork._nGroup = gsn;
                    pWork._GroupUsedBlocks = used;
                    if ( i == 0 ) pWork._bGroupBeg = true;
                    else if ( i == ( blocks.Length - 1 ) ) pWork._bGroupEnd = true;
                    blks.Add( pWork );
                    ret.Add( nCurrSN );
                }

                _2ndWorkList.Add( blks.ToArray() );
            } finally { Monitor.Exit( _hSync2ndList ); }

            Sns = ret.ToArray();
            if (_WorkList.Count <= 0) _hEvent.Set();
            return true;
        }

        virtual public bool RemoveWork(UInt32 sn)
        {
            if (sn == 0||IsDispose) {
                return false;
            }

            // remove 2nd
            Remove2ndListWorkSn( sn, true );
            // remove 1st
            Remove1stListWorkSn( sn, true );

            return true;
        }

        private void Remove1stListWorkSn(UInt32 sn, bool bSync)
        {
            WorkOfBlock pWork = null;
            if ( bSync ) Monitor.Enter( _hSyncRunList );
            try {
                for ( int i = 0; i < _WorkList.Count; i++ ) {
                    if ( _WorkList[ i ] == null ) continue;
                    if ( _WorkList[ i ]._nSN == sn ) {
                        pWork = _WorkList[ i ];
                        _WorkList.RemoveAt( i );
                        break;
                    }
                }
            } finally {
                if ( bSync ) Monitor.Exit( _hSyncRunList );
            }

            if (pWork != null) {
                if ( pWork._fpNotifyBeg != null )
                    pWork._fpNotifyBeg( pWork._nSN, pWork._pNotifyBegContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
                pWork.Dispose();
                pWork = null;
            }
        }
        private void Remove2ndListWorkSn(UInt32 sn, bool bSync)
        {
            WorkOfBlock[] pWork = null;
            if ( bSync ) Monitor.Enter( _hSync2ndList );
            try {
                for(int i = 0; i< _2ndWorkList.Count; i++ ) {
                    WorkOfBlock[] blks = _2ndWorkList[ i ];
                    if ( blks == null || blks.Length <= 0 ) continue;

                    for(int x = 0; x < blks.Length; x++ ) {
                        if ( blks[ x ] == null ) continue;
                        if (blks[x]._nSN == sn) {
                            pWork = blks; break;
                        }
                    }
                    if (pWork != null) {
                        _2ndWorkList.RemoveAt( i );
                        break;
                    }

                }
            }finally {
                if ( bSync ) Monitor.Exit( _hSync2ndList );
            }

            if (pWork != null && pWork.Length > 0) {
                for(int i = 0; i < pWork.Length; i++ ) {
                    if ( pWork[ i ] == null ) continue;
                    if ( pWork[ i ]._fpNotifyEnd != null )
                        pWork[ i ]._fpNotifyEnd( pWork[ i ]._nSN, pWork[ i ]._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
                    pWork[ i ].Dispose();
                    pWork[ i ] = null;
                }
                pWork = null;
            }
        }

        virtual public bool RemoveGroup(UInt32 gsn)
        {
            if ( gsn == 0 || IsDispose ) return false;

            // remove 2nd
            Remove2ndGroup( gsn, true );
            // remove 1st
            Remove1stGroup( gsn, true );

            return true;
        }
        private void Remove2ndGroup(UInt32 gsn, bool bSync)
        {
            WorkOfBlock[] blks = null;
            if ( bSync ) Monitor.Enter( _hSync2ndList );
            try {
                for(int i = 0; i < _2ndWorkList.Count; i++ ) {
                    if ( _2ndWorkList[ i ] == null || _2ndWorkList[ i ].Length <= 0 ) continue;
                    if ( _2ndWorkList[ i ][ 0 ] == null ) continue;
                    if (_2ndWorkList[i][0]._nGroup == gsn) {
                        blks = _2ndWorkList[ i ];
                        _2ndWorkList.RemoveAt( i );
                        break;
                    }
                }
            }finally { if ( bSync ) Monitor.Exit( _hSync2ndList ); }

            if (blks != null && blks.Length > 0) {
                for(int i = 0; i < blks.Length; i++ ) {
                    if ( blks[ i ] == null ) continue;
                    if ( blks[ i ]._fpNotifyEnd != null )
                        blks[ i ]._fpNotifyEnd( blks[ i ]._nSN, blks[ i ]._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
                    blks[ i ].Dispose();
                    blks[ i ] = null;
                }
                blks = null;
            }
        }
        private void Remove1stGroup( UInt32 gsn, bool bSync )
        {
            List<WorkOfBlock> blks = new List<WorkOfBlock>();
            if ( bSync ) Monitor.Enter( _hSyncRunList );
            try {
                bool exit = true;
                while ( true ) {
                    exit = true;
                    for ( int i = 0; i < _WorkList.Count; i++ ) {
                        if ( _WorkList[ i ] == null ) continue;
                        if ( _WorkList[ i ]._nGroup == gsn ) {
                            exit = false;
                            blks.Add( _WorkList[ i ] );
                            _WorkList.RemoveAt( i );
                            break;
                        }
                    }
                    if ( exit )
                        break;
                }
            } finally { if ( bSync ) Monitor.Exit( _hSyncRunList ); }

            if ( blks.Count > 0 ) {
                for ( int i = 0; i < blks.Count; i++ ) {
                    if ( blks[ i ] == null ) continue;
                    if ( blks[ i ]._fpNotifyEnd != null )
                        blks[ i ]._fpNotifyEnd( blks[ i ]._nSN, blks[ i ]._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
                    blks[ i ].Dispose();
                    blks[ i ] = null;
                }
                blks.Clear();
            }
        }

        //virtual public void ClearWorks()
        //{
        //    if ( IsDispose ) return;
        //    Monitor.Enter( _hSyncRunList );
        //    try {
        //        for(int i = 0; i < _WorkList.Count; i++ ) {
        //            if ( _WorkList[ i ] == null ) continue;
        //            if ( _WorkList[ i ]._fpNotifyEnd != null )
        //                _WorkList[ i ]._fpNotifyEnd( _WorkList[ i ]._nSN, _WorkList[ i ]._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
        //            _WorkList[ i ].Dispose();
        //            _WorkList[ i ] = null;
        //        }
        //        _WorkList.Clear();
        //    } finally { Monitor.Exit( _hSyncRunList ); }
        //}
        virtual public void Pause()
        {
            if ( IsDispose ) return;
            if ( _nAWorkStateOfBlock == ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PAUSING )
                return;
            _hPauseEvent.Set();
        }
        virtual public bool Proceed()
        {
            if ( IsDispose ) return false;
            if ( _nAWorkStateOfBlock != ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT_PROCEEDING )
                return false;

            _hProceedEvent.Set();
            return true;
        }
        // Clear all work in list
        virtual public void Stop()
        {
            if ( IsDispose ) return;
            if ( _nAWorkStateOfBlock == ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP )
                return;
            _hStopEvent.Set();
        }

        protected Int32 _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_NA;
        protected UCBlockBase _pCurrRunningBlock = null;
        protected AutoResetEvent _hPauseEvent = null;
        protected AutoResetEvent _hProceedEvent = null;
        protected AutoResetEvent _hStopEvent = null;
        protected Thread _RunningThread = null;
        // run block state transition
        private void Run()
        {
            WorkOfBlock pCurrWork = null;
            Int32 nRetCode = 0;
            Int32 nRunBlockErrorState = 0;
            List<UDataCarrierSet> RunGroupHistDat = new List<UDataCarrierSet>();
            UDataCarrierSet RunGroupPrevDat = null;

            while(!IsDispose) {
                if(_hStopEvent.WaitOne(0)) {
                    // get stop event
                    if (_pCurrRunningBlock != null) {
                        _pCurrRunningBlock.Stop();
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP;
                    }
                }

                switch(_nAWorkStateOfBlock) {
                #region Wait
                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT:
                    // wait a work in
                    if ( _WorkList.Count > 0 ) {
                        if ( _fpLog != null && pCurrWork != null ) {
                            _fpLog( eLogMessageType.WARNING, 0, "[UCBlockRunnerWin32::Run()] pCurrWork not null in state BLOCKRUNNING_STATE_WAIT!" );
                        }
                        // work come in
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_GETWORK;
                        _pCurrRunningBlock = null;
                        pCurrWork = null;
                        // reset event if trigger
                        if ( _hEvent.WaitOne( 0 ) )
                            _hEvent.Reset();
                    } else {
                        ReloadOneFrom2ndList();
                        if ( _WorkList.Count > 0 ) {
                            if ( !IsDispose ) break;
                            _hEvent.Reset();
                            break;
                        }
                        if ( _hEvent.WaitOne( 1000 ) )
                            _hEvent.Reset();
                        if ( IsDispose )
                            break;
                    }
                    break;
                #endregion

                #region Get Work
                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_GETWORK:
                    Monitor.Enter( _hSyncRunList );
                    try {
                        if (_fpLog != null && pCurrWork != null) {
                            _fpLog( eLogMessageType.WARNING, 0, "[UCBlockRunnerWin32::Run()] pCurrWork not null in state BLOCKRUNNING_STATE_GETWORK!" );
                        }
                        if ( _WorkList.Count > 0 ) {
                            pCurrWork = _WorkList[ 0 ];
                            // clear call info
                            UCBlockBase pb = null;
                            if (pCurrWork != null && pCurrWork._nGroup != 0 && pCurrWork._bGroupBeg) {
                                for ( int i = 0; i < _WorkList.Count; i++ ) {
                                    if ( _WorkList[ i ] == null ) break;
                                    if ( _WorkList[ i ]._bGroupEnd ) {
                                        pb = _pBlockManager.GetBlock( _WorkList[ i ]._strBlockId );
                                        if ( pb != null ) {
                                            pb.PrevBlockRunDat = null;
                                            pb.BlocksRunHistoryDat = null;
                                        }
                                        break;
                                    }
                                    pb = _pBlockManager.GetBlock( _WorkList[ i ]._strBlockId );
                                    if ( pb != null ) {
                                        pb.PrevBlockRunDat = null;
                                        pb.BlocksRunHistoryDat = null;
                                    }
                                }
                            } else if(pCurrWork != null && pCurrWork._nGroup == 0) {
                                pb = _pBlockManager.GetBlock( pCurrWork._strBlockId );
                                if ( pb != null ) {
                                    pb.PrevBlockRunDat = null;
                                    pb.BlocksRunHistoryDat = null;
                                }
                            }
                            _WorkList.RemoveAt( 0 );
                        } else
                            _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT;
                    } finally { Monitor.Exit( _hSyncRunList ); }

                    if (pCurrWork != null) {
                        _pCurrRunningBlock = _pBlockManager.GetBlock( pCurrWork._strBlockId );
                        if (_pCurrRunningBlock == null) {
                            if ( pCurrWork._fpNotifyBeg != null )
                                pCurrWork._fpNotifyBeg( pCurrWork._nSN, pCurrWork._pNotifyBegContext, (int)BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_BLOCKNOTFOUNF );
                            // not found the block, goto BLOCKRUNNING_STATE_RECYCLE_WORK
                            _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RECYCLE_WORK;
                            break;
                        }

                        // enable group mode
                        if (pCurrWork._nGroup != 0) {
                            if (pCurrWork._bGroupBeg) {
                                RunGroupHistDat.Clear();
                                RunGroupPrevDat = null;
                            }
                            // set prev final dat and history final dat
                            _pCurrRunningBlock.BlocksRunHistoryDat = RunGroupHistDat;
                            _pCurrRunningBlock.PrevBlockRunDat = RunGroupPrevDat;
                        }

                        // notify beg
                        if ( pCurrWork._fpNotifyBeg != null )
                            pCurrWork._fpNotifyBeg( pCurrWork._nSN, pCurrWork._pNotifyBegContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_NO_ERROR );
                        // work ready to run
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNNING;
                        if ( _fpBlockRunStateChangeNotify != null ) _fpBlockRunStateChangeNotify( String.IsNullOrEmpty( _pCurrRunningBlock.DisplayName ) ? _pCurrRunningBlock.ID : _pCurrRunningBlock.DisplayName, BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNNING );
                        _pCurrRunningBlock.ResetToRun(); // reset state and flags
                        // reset all event
                        _hPauseEvent.Reset();
                        _hProceedEvent.Reset();
                    }
                    break;
                #endregion

                #region Running
                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNNING:
                    // check pause event coming
                    if(_hPauseEvent.WaitOne(0)) {
                        if(_pCurrRunningBlock.State != (int) UCBlockStateReserved.UCBLOCK_STATE_PAUSING ) {
                            _pCurrRunningBlock.Pause();
                            _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PAUSING;
                            if ( _fpBlockRunStateChangeNotify != null ) _fpBlockRunStateChangeNotify( String.IsNullOrEmpty( _pCurrRunningBlock.DisplayName ) ? _pCurrRunningBlock.ID : _pCurrRunningBlock.DisplayName, BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PAUSING );
                            break;
                        }
                        if ( _fpLog != null ) _fpLog( eLogMessageType.WARNING, 0, "[UCBlockRunnerWin32::Run()] STATE_RUNNING NOT running block state == pause" );
                    }
                    RunWorkOfBlock( _pCurrRunningBlock, pCurrWork, ref _nAWorkStateOfBlock, ref nRetCode );
                    if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_NG ) {
                        //if (pCurrWork._nGroup != 0) { // one work get error
                        //    // remove the following work as same group
                        //    RemoveSameGroupWorks( pCurrWork._nGroup );
                        //}
                        nRunBlockErrorState = _nAWorkStateOfBlock;
                        // free grouped work on NG
                        if (pCurrWork != null && pCurrWork._nGroup != 0)
                        {
                            WorkOfBlock.FreeFinalData( pCurrWork._GroupUsedBlocks );
                        }
                    }
                    if (nRetCode == (int)UCBlockBaseRet.RET_STATUS_OK) {
                        if (pCurrWork._nGroup != 0) // enable the group
                            RunGroupPrevDat = _pCurrRunningBlock.BlockRunDat; // get the current run dat as prev
                        // free grouped work when end
                        if (pCurrWork != null && pCurrWork._nGroup != 0 && pCurrWork._bGroupEnd)
                            WorkOfBlock.FreeFinalData( pCurrWork._GroupUsedBlocks );
                    }
                    break;
                #endregion

                #region Pause condition
                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PAUSING:
                    RunWorkOfBlock( _pCurrRunningBlock, pCurrWork, ref _nAWorkStateOfBlock, ref nRetCode );
                    if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_NG ) {
                        //if ( pCurrWork._nGroup != 0 ) { // one work get error
                        //    // remove the following work as same group
                        //    RemoveSameGroupWorks( pCurrWork._nGroup );
                        //}
                        nRunBlockErrorState = _nAWorkStateOfBlock; // record current state
                    } else if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_PAUSED )
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT_PROCEEDING;
                    break;

                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT_PROCEEDING:
                    // check proceed event coming
                    if (_hProceedEvent.WaitOne(0)) {
                        _pCurrRunningBlock.Proceed();
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_PROCEEDING;
                        break;
                    }

                    // running SM
                    RunWorkOfBlock( _pCurrRunningBlock, pCurrWork, ref _nAWorkStateOfBlock, ref nRetCode );
                    if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_NG ) {
                        //if ( pCurrWork._nGroup != 0 ) { // one work get error
                        //    // remove the following work as same group
                        //    RemoveSameGroupWorks( pCurrWork._nGroup );
                        //}
                        nRunBlockErrorState = _nAWorkStateOfBlock;
                    } else if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_PAUSED )
                        Thread.Sleep( 10 );
                    break;

                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_PROCEEDING:
                    RunWorkOfBlock( _pCurrRunningBlock, pCurrWork, ref _nAWorkStateOfBlock, ref nRetCode );
                    if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_NG ) {
                        //if ( pCurrWork._nGroup != 0 ) { // one work get error
                        //    // remove the following work as same group
                        //    RemoveSameGroupWorks( pCurrWork._nGroup );
                        //}
                        nRunBlockErrorState = _nAWorkStateOfBlock;
                    } else if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_PAUSE_RELEASED ) {
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNNING;
                        if ( _fpBlockRunStateChangeNotify != null ) _fpBlockRunStateChangeNotify( String.IsNullOrEmpty( _pCurrRunningBlock.DisplayName ) ? _pCurrRunningBlock.ID : _pCurrRunningBlock.DisplayName, BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNNING );
                    }
                    break;
                #endregion

                #region Stop condition
                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP:
                    // if have SM running, wait to end
                    RunWorkOfBlock( _pCurrRunningBlock, pCurrWork, ref _nAWorkStateOfBlock, ref nRetCode );
                    if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_NG ) {
                        //if ( pCurrWork._nGroup != 0 ) { // one work get error
                        //    // remove the following work as same group
                        //    RemoveSameGroupWorks( pCurrWork._nGroup );
                        //}
                        nRunBlockErrorState = _nAWorkStateOfBlock;
                    } else if ( nRetCode == ( int ) UCBlockBaseRet.RET_STATUS_OK ) {
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP_DONE;
                        if ( _fpBlockRunStateChangeNotify != null ) _fpBlockRunStateChangeNotify( String.IsNullOrEmpty( _pCurrRunningBlock.DisplayName ) ? _pCurrRunningBlock.ID : _pCurrRunningBlock.DisplayName, BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP_DONE );
                    }
                    break;

                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP_DONE:
                    // clear work list
                    ClearWorkList( ( int )BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_STOP );
                    // change recycle work
                    _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RECYCLE_WORK;
                    break;
                #endregion

                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RECYCLE_WORK:
                    if ( _fpBlockRunStateChangeNotify != null ) _fpBlockRunStateChangeNotify( String.IsNullOrEmpty( _pCurrRunningBlock.DisplayName ) ? _pCurrRunningBlock.ID : _pCurrRunningBlock.DisplayName, BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RECYCLE_WORK );
                    if (pCurrWork._nGroup != 0 && _pCurrRunningBlock != null) {
                        RunGroupHistDat.Add( _pCurrRunningBlock.BlockRunDat );
                    }
                    else if (_pCurrRunningBlock != null)
                        _pCurrRunningBlock.ClearFinalData(); // single work: clear final data at end

                    if ( pCurrWork != null ) {
                        pCurrWork.Dispose();
                    }
                    pCurrWork = null;
                    _pCurrRunningBlock = null;
                    _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT;
                    break;

                case ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNBLOCK_ERROR:
                    // clear all works in list
                    ClearWorkList( ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_STATEERR );
                    // log message
                    if ( _fpLog != null )
                        _fpLog( eLogMessageType.NORMAL, 0, String.Format( "[UCBlockRunnerWin32::Run()]-Error- run block({0}) with error in state {1} and following works will be removed!", pCurrWork._strBlockId, StateName(nRunBlockErrorState ) ));
                    // recycle work memory
                    if ( pCurrWork != null ) {
                        // free grouped work final data
                        if ( pCurrWork._nGroup != 0 )
                            WorkOfBlock.FreeFinalData( pCurrWork._GroupUsedBlocks );
                        // current work
                        pCurrWork.Dispose();
                    }
                    pCurrWork = null;
                    _pCurrRunningBlock = null;
                    // switch to wait state
                    _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT;
                    break;

                default:
                    _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT;
                    break;

                } // end switch-_nAWorkStateOfBlock
                // process receive stop event
                if (_hStopEvent.WaitOne(0)) {
                    if ( _pCurrRunningBlock != null ) {
                        _pCurrRunningBlock.Stop();
                        _nAWorkStateOfBlock = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP;
                    }
                }
            } // end while-IsDispose

            // recycle work
            if (pCurrWork != null) {
                if ( pCurrWork._fpNotifyEnd != null )
                    pCurrWork._fpNotifyEnd( pCurrWork._nSN, pCurrWork._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_PROG_END );
                if ( pCurrWork._fpRunGroupEnd != null )
                    pCurrWork._fpRunGroupEnd( pCurrWork._nGroup, pCurrWork._ContextOfRunGroupEnd, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_PROG_END );
                pCurrWork.Dispose();
                pCurrWork = null;
            }
        }
        //private void RemoveSameGroupWorks(UInt32 gsn)
        //{
        //    Monitor.Enter( _hSyncRunList );
        //    try {
        //        while (_WorkList.Count > 0) {
        //            if ( _WorkList[ 0 ] == null ) break;
        //            if ( _WorkList[ 0 ]._nGroup != gsn ) break;

        //            if ( _WorkList[ 0 ]._fpNotifyEnd != null )
        //                _WorkList[ 0 ]._fpNotifyEnd( _WorkList[ 0 ]._nSN, _WorkList[ 0 ]._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
        //            if ( _WorkList[ 0 ]._fpRunGroupEnd != null  )
        //                _WorkList[ 0 ]._fpRunGroupEnd( _WorkList[ 0 ]._nGroup, _WorkList[ 0 ]._ContextOfRunGroupEnd, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_REMOVESN );
        //            _WorkList[ 0 ].Dispose();
        //            _WorkList[ 0 ] = null;
        //            _WorkList.RemoveAt( 0 );
        //        }
        //    } finally { Monitor.Exit( _hSyncRunList ); }
        //}
        private void RunWorkOfBlock(UCBlockBase pBlock, WorkOfBlock pWork, ref Int32 nextState, ref Int32 retOfBlock)
        {
            retOfBlock = pBlock.Run( pWork._pParam._pData );

            switch(retOfBlock) {
            case ( int ) UCBlockBaseRet.RET_STATUS_NG: // trigger clear all work in list
                if ( pWork._fpNotifyEnd != null )
                    pWork._fpNotifyEnd( pWork._nSN, pWork._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_BLOCKEXECERR );
                if ( pWork._fpRunGroupEnd != null && pWork._bGroupEnd )
                    pWork._fpRunGroupEnd( pWork._nGroup, pWork._ContextOfRunGroupEnd, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_BLOCKEXECERR );
                nextState = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNBLOCK_ERROR;
                if ( _fpBlockRunStateChangeNotify != null ) _fpBlockRunStateChangeNotify( String.IsNullOrEmpty( pBlock.DisplayName ) ? pBlock.ID : pBlock.DisplayName, BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNBLOCK_ERROR );
                break;

            case ( int ) UCBlockBaseRet.RET_STATUS_OK:
                if ( pWork._fpNotifyEnd != null )
                    pWork._fpNotifyEnd( pWork._nSN, pWork._pNotifyEndContext, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_NO_ERROR );
                if ( pWork._fpRunGroupEnd != null && pWork._bGroupEnd )
                    pWork._fpRunGroupEnd( pWork._nGroup, pWork._ContextOfRunGroupEnd, ( int ) BLOCKWORK_RETURNCODE.BLOCKWORK_RETURNCODE_NO_ERROR );
                nextState = ( int ) BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RECYCLE_WORK;
                break;
            }
        }
        private void ClearWorkList(Int32 returnCode)
        {
            Monitor.Enter( _hSyncRunList );
            try {
                while(_WorkList.Count > 0) {
                    WorkOfBlock pWork = _WorkList[ 0 ];
                    _WorkList.RemoveAt( 0 );

                    if ( pWork == null ) continue;
                    if ( pWork._fpNotifyBeg != null ) pWork._fpNotifyBeg( pWork._nSN, pWork._pNotifyBegContext, returnCode );
                    if ( pWork._fpNotifyEnd != null ) pWork._fpNotifyEnd( pWork._nSN, pWork._pNotifyEndContext, returnCode );
                    if ( pWork._fpRunGroupEnd != null && pWork._bGroupEnd )
                        pWork._fpRunGroupEnd( pWork._nGroup, pWork._ContextOfRunGroupEnd, returnCode );
                    pWork.Dispose();
                    pWork = null;
                }
            }finally { Monitor.Exit( _hSyncRunList ); }
        }
        private void ReloadOneFrom2ndList()
        {
            if ( IsDispose ) return;
            WorkOfBlock[] one = null;
            Monitor.Enter( _hSync2ndList );
            try { if ( _2ndWorkList.Count > 0 ) { one = _2ndWorkList[ 0 ]; _2ndWorkList.RemoveAt( 0 ); } } finally { Monitor.Exit( _hSync2ndList ); }

            if ( one == null || one.Length <= 0 ) return;

            Monitor.Enter( _hSyncRunList );
            try {
                for(int i = 0; i < one.Length; i++ ) {
                    _WorkList.Add( one[ i ] );
                }
            }finally { Monitor.Exit( _hSyncRunList ); }
            one = null;
        }
        private void Clear2ndList(Int32 returnCode)
        {
            Monitor.Enter( _hSync2ndList );
            try {
                for(int i = 0; i < _2ndWorkList.Count; i++ ) {
                    if ( _2ndWorkList[ i ] == null ) continue;
                    for(int x = 0; x < _2ndWorkList[i].Length; x++ ) {
                        WorkOfBlock pWork = _2ndWorkList[ i ][ x ];
                        if ( pWork == null ) continue;
                        if ( pWork._fpNotifyBeg != null ) pWork._fpNotifyBeg( pWork._nSN, pWork._pNotifyBegContext, returnCode );
                        if ( pWork._fpNotifyEnd != null ) pWork._fpNotifyEnd( pWork._nSN, pWork._pNotifyEndContext, returnCode );
                        if ( pWork._fpRunGroupEnd != null && pWork._bGroupEnd )
                            pWork._fpRunGroupEnd( pWork._nGroup, pWork._ContextOfRunGroupEnd, returnCode );
                        pWork.Dispose();
                        pWork = null;
                    }
                }
                _2ndWorkList.Clear();
            } finally { Monitor.Exit( _hSync2ndList ); }
        }
        protected string StateName(int code)
        {
            if ( !Enum.IsDefined( typeof( BLOCKRUNNING_STATE ), code ) ) return "Undefined";
            BLOCKRUNNING_STATE state = (BLOCKRUNNING_STATE) Enum.Parse( typeof( BLOCKRUNNING_STATE ), Enum.GetName( typeof( BLOCKRUNNING_STATE ), code ) );
            switch(state) {
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_NA: return "[NA]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_GETWORK: return "[GetDicKeyStrOne Work]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PAUSING: return "[Pausing]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_PROCEEDING: return "[Process Proceeding]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP: return "[Process Stop]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_PROC_STOP_DONE: return "[Process Stop Done]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RECYCLE_WORK: return "[Recycle Work]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNBLOCK_ERROR: return "[Run BlockAction Error]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_RUNNING: return "[Running]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT: return "[Wait]";
            case BLOCKRUNNING_STATE.BLOCKRUNNING_STATE_WAIT_PROCEEDING: return "[Wait Proceeding]";
            }
            return "undefined";
        }

        private object _hSyncPollWork = new object();
        private List<BlockPollingWork> _PollingWorks = new List<BlockPollingWork>();
        private Thread _PollingThread = null;
        private void ClearPollingWorks()
        {
            Monitor.Enter( _hSyncPollWork );
            try {
                for(int i = 0;i < _PollingWorks.Count; i++ ) {
                    if ( _PollingWorks[ i ] == null ) continue;
                    _PollingWorks[ i ].Dispose();
                    _PollingWorks[ i ] = null;
                }
                _PollingWorks.Clear();
            } finally { Monitor.Exit( _hSyncPollWork ); }
        }
        private void Polling()
        {
            List<BlockPollingWork> toRmv = new List<BlockPollingWork>();
            int nCurWork = 0;
            while(!IsDispose) {
                Monitor.Enter( _hSyncPollWork );
                try {
                    DateTime cur = DateTime.Now;
                    for(int i = 0; i < _PollingWorks.Count; i++ ) {
                        if ( _PollingWorks[ i ] == null ) continue;
                        BlockPollingWork w = _PollingWorks[ i ];

                        TimeSpan diff = cur - w._tmLast;
                        // is timeout ?
                        if (diff.TotalMilliseconds >= w._nMsTimeout) {
                            // callback
                            w._fpTimeoutCall( w._Context );
                            // dec trg time
                            w._nNumOfTrigger = w._nNumOfTrigger < 0 ? w._nNumOfTrigger : w._nNumOfTrigger - 1;
                            // update current time
                            w._tmLast = DateTime.Now;
                        }

                        // trg time exceed
                        if ( w._nNumOfTrigger == 0 ) toRmv.Add( w );
                    }

                    // remove exceeding trg time
                    while(toRmv.Count > 0) {
                        _PollingWorks.Remove( toRmv[ 0 ] );
                        toRmv[ 0 ].Dispose();
                        toRmv.RemoveAt( 0 );
                    }

                    nCurWork = _PollingWorks.Count;
                } finally { Monitor.Exit( _hSyncPollWork ); }

                if ( nCurWork > 0 )
                    Thread.Sleep( 1 );
                else
                    Thread.Sleep( 10 );
            }
        }
        public object AddPolling(UDataCarrier context, fpUCBlockHandleDatCarrier fpHandContext, 
            fpBlockPollingTimeoutCallback fpCallback, int nInterval, int nTrg = -1)
        {
            if ( IsDispose || fpCallback == null ) return null;

            BlockPollingWork item = new BlockPollingWork( context, fpHandContext, fpCallback, nInterval, nTrg );
            Monitor.Enter( _hSyncPollWork );
            try {
                _PollingWorks.Add( item );
            } finally { Monitor.Exit( _hSyncPollWork ); }

            return item as object;
        }
        public void RemovePolling(object item)
        {
            if ( IsDispose || item == null ) return;
            Monitor.Enter( _hSyncPollWork );
            try {
                BlockPollingWork w = item as BlockPollingWork;
                if ( w != null ) {
                    if ( _PollingWorks.Contains( w ) ) {
                        _PollingWorks.Remove( w );
                        w.Dispose();
                        w = null;
                    }
                }
            } finally { Monitor.Exit( _hSyncPollWork ); }
        }

        private static UInt32 _BlkSerialCount = 0;
        private static object _hSyncBlkSerialCount = new object();
        private static UInt32 GetBlockRunCtxCount()
        {
            UInt32 next = 0;
            Monitor.Enter(_hSyncBlkSerialCount );
            try
            {
                next = _BlkSerialCount == UInt32.MaxValue ? 1 : ++_BlkSerialCount;
                _BlkSerialCount = next;
            } finally { Monitor.Exit( _hSyncBlkSerialCount ); }
            return next;
        }

        /// <summary>
        /// Allocate run context for call run block
        /// </summary>
        /// <param name="blocks">runtime new and managed by BlockCallToRunCtx</param>
        /// <param name="inputs">runtime new and managed by BlockCallToRunCtx</param>
        /// <returns></returns>
        public static BlockCallToRunCtx GenBlockToRun( UCBlockBase [] blocks, UDataCarrierSet [] inputs)
        {
            if ( blocks == null || inputs == null || blocks.Length != inputs.Length )
                return null;
            return new BlockCallToRunCtx(GetBlockRunCtxCount()) {
                BlockInstances = blocks.ToList(),
                BlockInputs = inputs.ToList()
            };
        }
    }
}
