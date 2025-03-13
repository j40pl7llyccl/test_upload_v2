using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public delegate void HandleScriptRunnerManagerCall( UScriptRunner context );
    public delegate void HandleScriptDoneCallType1(object context, ScriptExecReturnCode retCode, List<UMacroProduceCarrierResult> results, List<UMacroProduceCarrierDrawingResult> drawResults);
    public delegate void HandleScriptDoneCallType2(object context, ScriptExecReturnCode retCode, byte[] results);
    public delegate void HandleScriptDoneCallType3( object context, ScriptExecReturnCode retCode, List< UScriptHistoryCarrier > results );
    public delegate void HandleScriptDoneCallType4( object context, ScriptExecReturnCode retCode, UDataCarrier[] results );

    /// <summary>
    /// Create a thread to run number of MaxExecCount
    /// </summary>
    public class UScriptRunner : IDisposable
    {
        private bool m_bDisposing = false;
        private bool m_bDiposed = false;
        private object m_sync = new object();
        private CancelExecScript m_syncCancel = null;

        private Thread m_Thread = null;
        private bool m_bRunning = false;
        private UScript m_refScript = null;
        private bool m_bFreeMacroPropagationAtTheEnd = false;

        private object m_callbackContext = null;
        private HandleScriptDoneCallType1 m_callback = null;
        private HandleScriptDoneCallType2 m_callback2 = null;
        private HandleScriptDoneCallType3 m_callback3 = null;
        private HandleScriptDoneCallType4 m_callback4 = null;
        private List< UScript > m_scriptSet = null;

        private int m_nType = 0;
        private Stream m_stream = null;
        private string m_filePath = "";
        private int m_nWaitTimeout = 1000 * 60;

        private HandleScriptRunnerManagerCall m_callback2Manager = null;

        internal string strID = "";
        internal bool EndOnError { get; set; } = false;
        internal Action<object> LoopEndCall { get; set; } = null;
        internal IAsyncResult KeepBeginInvoke { get; set; } = null;

        internal HandleScriptDoneCallType4 Callback4
        {
            get => m_callback4;
            set => m_callback4 = value;
        }

        internal UDataCarrier[] BegPrepropagation { get; set; } = null;
        internal fpUDataCarrierSetResHandler BegPrepropagationHandler { get; set; } = null;

        public int ThreadId => m_bDisposing || m_bDiposed ? -1 : m_Thread?.ManagedThreadId ?? -1;

        private UInt64 ExecCount { get; set; }
        public UInt64 MaxExecCount { get; internal set; } = 1;
        public UScript RunningScript => m_refScript;

        /// <summary>
        /// 執行一般 script
        /// </summary>
        /// <param name="contextForFp">context for delegating exec script done</param>
        /// <param name="fp">delegate exec script done</param>
        /// <param name="scriptToRun">script instance to run</param>
        /// <param name="nWaitTimeout">sync timeout for the semaphore in script</param>
        /// <param name="callbackMgr">delegate to manager</param>
        /// <param name="bFreeMacroPropagationAtEnd">free script resources after calling</param>
        internal UScriptRunner( object contextForFp, HandleScriptDoneCallType1 fp, UScript scriptToRun, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bFreeMacroPropagationAtEnd = false )
        {
            m_callback2Manager = callbackMgr;

            m_callbackContext = contextForFp;
            m_callback = fp;
            m_refScript = scriptToRun;
            m_nWaitTimeout = nWaitTimeout <= 0 ? m_nWaitTimeout : nWaitTimeout;
            m_bFreeMacroPropagationAtTheEnd = bFreeMacroPropagationAtEnd;

            //GenThread2Run();
        }

        /// <summary>
        /// 執行 script 後, 將結果透過 Stream s 以 XML 格式寫出.
        /// 讀取資料請用 UDataCarrier.ReadXml
        /// </summary>
        /// <param name="callbackMgr">delegate to manager</param>
        /// <param name="contextForFp">context for delegating exec script done</param>
        /// <param name="fp">delegate exec script done call</param>
        /// <param name="scriptToRun">script instance to run</param>
        /// <param name="s">stream to write results</param>
        /// <param name="nWaitTimeout">sync timeout for the semaphore in script</param>
        /// <param name="bFreeMacroPropagationAtEnd">free script resources after calling</param>
        internal UScriptRunner( object contextForFp, HandleScriptDoneCallType1 fp, UScript scriptToRun, Stream s, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bFreeMacroPropagationAtEnd = false )
        {
            m_callback2Manager = callbackMgr;

            m_callbackContext = contextForFp;
            m_callback = fp;
            m_refScript = scriptToRun;
            m_nWaitTimeout = nWaitTimeout <= 0 ? m_nWaitTimeout : nWaitTimeout;
            m_bFreeMacroPropagationAtTheEnd = bFreeMacroPropagationAtEnd;

            m_nType = 1;
            m_stream = s;

            //GenThread2Run();
        }

        /// <summary>
        /// 執行 script 後, 將結果寫至檔案
        /// 讀取資料請用 UDataCarrier.ReadXml
        /// </summary>
        /// <param name="callbackMgr">delegate to manager</param>
        /// <param name="contextForFp">context for delegating exec script done</param>
        /// <param name="fp">delegate exec script done call</param>
        /// <param name="scriptToRun">script instance to run</param>
        /// <param name="wrFilePath">file path for write</param>
        /// <param name="nWaitTimeout">sync timeout for the semaphore in script</param>
        /// <param name="bFreeMacroPropagationAtEnd">free script resources after calling</param>
        internal UScriptRunner(object contextForFp, HandleScriptDoneCallType1 fp, UScript scriptToRun,
            string wrFilePath, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bFreeMacroPropagationAtEnd = false )
        {
            m_callback2Manager = callbackMgr;

            m_callbackContext = contextForFp;
            m_callback = fp;
            m_refScript = scriptToRun;
            m_nWaitTimeout = nWaitTimeout <= 0 ? m_nWaitTimeout : nWaitTimeout;
            m_bFreeMacroPropagationAtTheEnd = bFreeMacroPropagationAtEnd;

            m_nType = 2;
            m_filePath = wrFilePath;

            //GenThread2Run();
        }

        /// <summary>
        /// 執行 script 後, 將會把結果打包成 buffer
        /// Remark:
        /// 1. 解開 buffer 資料可用 MacroResultsToConcatBlockBuffer.Unpack
        /// 2. 要輸出到 buffer 的 Macro 要設定 ExtractingResultToPack
        /// </summary>
        /// <param name="callbackMgr">delegate to manager</param>
        /// <param name="contextForFp">context for delegating exec script done</param>
        /// <param name="fp">delegate exec script done call</param>
        /// <param name="scriptToRun">script instance to run</param>
        /// <param name="nWaitTimeout">sync timeout for the semaphore in script</param>
        /// <param name="bFreeMacroPropagationAtEnd">free script resources after calling</param>
        internal UScriptRunner( object contextForFp, HandleScriptDoneCallType2 fp, UScript scriptToRun, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bFreeMacroPropagationAtEnd = false )
        {
            m_callback2Manager = callbackMgr;

            m_callbackContext = contextForFp;
            m_callback2 = fp;
            m_refScript = scriptToRun;
            m_nWaitTimeout = nWaitTimeout <= 0 ? m_nWaitTimeout : nWaitTimeout;
            m_bFreeMacroPropagationAtTheEnd = bFreeMacroPropagationAtEnd;

            m_nType = 3;

            //GenThread2Run();
        }

        /// <summary>
        /// 跨 script 之間執行
        /// </summary>
        /// <param name="callbackMgr">delegate to manager</param>
        /// <param name="contextForFp">context for delegating exec script done</param>
        /// <param name="fp">delegate exec script done call</param>
        /// <param name="scriptToRun">script instance to run</param>
        /// <param name="scriptSet">a set of scripts</param>
        /// <param name="nWaitTimeout">sync timeout for the semaphore in script</param>
        /// <param name="bFreeMacroPropagationAtEnd">free script resources after calling</param>
        internal UScriptRunner( object contextForFp, HandleScriptDoneCallType3 fp, UScript scriptToRun, List<UScript> scriptSet, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bFreeMacroPropagationAtEnd = false )
        {
            m_callback2Manager = callbackMgr;

            m_callbackContext = contextForFp;
            m_callback3 = fp;
            m_refScript = scriptToRun;
            m_scriptSet = scriptSet;
            m_nWaitTimeout = nWaitTimeout <= 0 ? m_nWaitTimeout : nWaitTimeout;
            m_bFreeMacroPropagationAtTheEnd = bFreeMacroPropagationAtEnd;

            m_nType = 4;
            m_syncCancel = new CancelExecScript( null, null );

            //GenThread2Run();
        }

        public void Dispose()
        {
            Monitor.Enter( m_sync );
            try
            {
                if ( !m_bDisposing && !m_bDiposed )
                {
                    m_bDisposing = true;

                    // thread running and call to cancel
                    if ( m_bRunning ) CancelPriv();

                    // wait done
                    m_Thread?.Join();
                    m_Thread = null;

                    m_bDiposed = true;
                    m_bDisposing = false;
                }
            } finally{ Monitor.Exit( m_sync ); }
        }

        private void CancelPriv(object context = null, HandleCancelScriptCallback call = null)
        {
            MaxExecCount = 0;
            if ( m_nType == 4 )
            {
                m_syncCancel.Flag = true;
            }
            else
            {
                m_refScript?.CancelRunning(call, context);
            }
        }

        public void Cancel(HandleCancelScriptCallback call = null, object context = null)
        {
            Monitor.Enter( m_sync );
            try
            {
                if ( !m_bDisposing && !m_bDiposed )
                {
                    CancelPriv( context, call );
                }
            } finally { Monitor.Exit( m_sync ); }
        }

        private void Running()
        {
            //
            // suppose counting always less than UInt64.MaxValue
            //
            do
            {
                ScriptExecReturnCode code = ScriptExecReturnCode.NA;
                byte[] buffResults = null;
                if ( m_nType != 4 )
                {
                    if ( m_refScript.SynchronizedObject != null && m_refScript.SynchronizedObject.WaitOne( m_nWaitTimeout ) )
                    {
                        switch ( m_nType )
                        {
                            case 0:
                                code = m_refScript.Running( false, prevPropagation: BegPrepropagation, fpHandlePrevPropagation: BegPrepropagationHandler, bHandleMacroPropagation: m_bFreeMacroPropagationAtTheEnd );
                                break;
                            case 1:
                                code = m_refScript.RunningRepo2Stream( false, -1, m_stream, prevPropagation: BegPrepropagation, fpHandlePrevPropagation: BegPrepropagationHandler, bHandleMacroPropagation: m_bFreeMacroPropagationAtTheEnd );
                                break;
                            case 2:
                                code = m_refScript.RunningRepo2File( false, -1, m_filePath, prevPropagation: BegPrepropagation, fpHandlePrevPropagation: BegPrepropagationHandler, bHandleMacroPropagation: m_bFreeMacroPropagationAtTheEnd );
                                break;
                            case 3:
                                code = m_refScript.RunningRepoBuff( false, -1, out buffResults, prevPropagation: BegPrepropagation, fpHandlePrevPropagation: BegPrepropagationHandler, bHandleMacroPropagation: m_bFreeMacroPropagationAtTheEnd );
                                break;
                        }

                        // callback
                        m_callback?.Invoke( m_callbackContext, code, m_refScript.ResultCarriers, m_refScript.DrawingResultCarriers );
                        m_callback2?.Invoke( m_callbackContext, code, buffResults );
                        m_callback4?.Invoke( m_callbackContext, code, UDataCarrier.MakeVariableItemsArray( m_refScript.ResultCarriers, m_refScript.DrawingResultCarriers ) );

                        m_refScript.SynchronizedObject.Release();
                    }
                    else
                    {
                        code = ScriptExecReturnCode.SyncTimeout;
                        m_callback?.Invoke( m_callbackContext, code, null, null );
                        m_callback2?.Invoke( m_callbackContext, code, null );
                        m_callback4?.Invoke( m_callbackContext, code, null );
                    }

                    if ( EndOnError && code != ScriptExecReturnCode.OK )
                    {
                        m_refScript.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"Script runner exec {( string.IsNullOrEmpty( m_refScript.NameOfId ) ? "" : m_refScript.NameOfId )} end with error break. [{m_refScript.OnErrorIndex}:{m_refScript.OnErrorMethod}]={m_refScript.StatusMessage}" );
                        break;
                    }
                }
                else
                {
                    if ( UScript.RunningControlFlow( true, m_syncCancel, m_scriptSet, m_refScript, true, m_nWaitTimeout,
                        true, out var results, BegPrepropagation, BegPrepropagationHandler ) )
                    {
                        m_callback3?.Invoke( m_callbackContext, ScriptExecReturnCode.OK, results );
                        m_callback4?.Invoke( m_callbackContext, ScriptExecReturnCode.OK, UDataCarrier.MakeOneItemArray( results ) );
                    }
                    else
                    {
                        m_callback3?.Invoke( m_callbackContext, ScriptExecReturnCode.NA, results );
                        m_callback4?.Invoke( m_callbackContext, ScriptExecReturnCode.NA, null );
                        if (EndOnError)
                        {
                            m_refScript.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"Script runner: ControlFlow exec {m_refScript.NameOfId} fail to break." );
                            break;
                        }
                    }
                    UScriptHistoryCarrier.Free( results ); // free resource after invoking to method
                }
                // dispose() called
                if ( m_bDisposing || m_bDiposed || !ResourceManager.SystemAvaliable )
                    break;
            } while ( ++ExecCount < MaxExecCount );

            // disable eval exec time
            if ( ResourceManager.SystemAvaliable )
                m_refScript.EnableAllMacroEvalExecTime( false );

            m_bRunning = false;
            // callback to manager
            m_callback2Manager?.Invoke( this );

            // loop end called after manger design callback
            LoopEndCall?.Invoke( m_callbackContext );
        }

        internal void GenThread2Run(UInt64 nExec = 1)
        {
            if ( m_Thread != null ) return;
            MaxExecCount = nExec;
            ExecCount = 0;
            m_bRunning = true;
            m_Thread = new Thread( new ParameterizedThreadStart( Call2Run ) );
            m_Thread.Start(this);
        }

        internal void InvokeMain2Run( UInt64 nMaxExecTimes = 1 )
        {
            if ( m_bRunning )
                return;

            m_bRunning = true;
            MaxExecCount = nMaxExecTimes;
            ExecCount = 0;

            KeepBeginInvoke = ResourceManager.BeginInvokeMainThread( new Action( Running ) );
        }

        private static void Call2Run( object instance )
        {
            if (instance is UScriptRunner runner) runner?.Running();
        }
    }

    public class UScriptRunnerFactory : IDisposable
    {
        private static UScriptRunnerFactory _singleton = null; 
        public static UScriptRunnerFactory Singleton
        {
            get
            {
                if ( _singleton == null )
                {
                    _singleton = new UScriptRunnerFactory();
                    _singleton.GenThread();
                }

                return _singleton;
            }
        }
        public static UInt64 InfiniteExecCount { get; private set; } = UInt64.MaxValue;

        /// <summary>
        /// New an instance of script runner with thread inside
        /// </summary>
        /// <param name="contextForFp">context of fp</param>
        /// <param name="fp">exec delegate when done call</param>
        /// <param name="scriptToRun">a script instance to run</param>
        /// <param name="bAutoRun">if create script runner and run; false must call GenThread2Run()</param>
        /// <param name="nWaitTimeout">number of milisecond to wait sync begin running a script</param>
        /// <param name="callbackMgr">delegate callback to back manager</param>
        /// <param name="bHandleMacroPropagationAtTheEnd">handle all propagations when end exec script</param>
        /// <param name="nExecTimes">number of times to exec script</param>
        /// <returns>an instance of script runner</returns>
        public static UScriptRunner NewScriptRunner1( object contextForFp, HandleScriptDoneCallType1 fp,
            UScript scriptToRun, bool bAutoRun = true, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bHandleMacroPropagationAtTheEnd = true, UInt64 nExecTimes = 1 )
        {
            UScriptRunner r = new UScriptRunner( contextForFp, fp, scriptToRun, nWaitTimeout, callbackMgr, bHandleMacroPropagationAtTheEnd );
            if ( bAutoRun ) r.GenThread2Run( nExecTimes );
            return r;
        }

        /// <summary>
        /// New a instance of script runner with thread inside and write results to stream in form of XML
        /// </summary>
        /// <param name="contextForFp">context of fp</param>
        /// <param name="fp">exec delegate when done call</param>
        /// <param name="scriptToRun">a script instance to run</param>
        /// <param name="s">stream to write results when done exec script</param>
        /// <param name="bAutoRun">if create script runner and run; false must call GenThread2Run()</param>
        /// <param name="nWaitTimeout">number of milisecond to wait sync begin running a script</param>
        /// <param name="callbackMgr">delegate callback to back manager</param>
        /// <param name="bHandleMacroPropagationAtTheEnd">handle all propagations when end exec script</param>
        /// <returns>an instance of script runner</returns>
        public static UScriptRunner NewScriptRunner2( object contextForFp, HandleScriptDoneCallType1 fp,
            UScript scriptToRun, Stream s, bool bAutoRun = true, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bHandleMacroPropagationAtTheEnd = true )
        {
            UScriptRunner r = new UScriptRunner( contextForFp, fp, scriptToRun, s, nWaitTimeout, callbackMgr, bHandleMacroPropagationAtTheEnd );
            if (bAutoRun) r.GenThread2Run();
            return r;
        }

        /// <summary>
        /// New a instance of script runner with thread inside and write results to file in form of XML
        /// </summary>
        /// <param name="contextForFp">context of fp</param>
        /// <param name="fp">exec delegate when done call</param>
        /// <param name="scriptToRun">a script instance to run</param>
        /// <param name="wrFilePath">file path to write results when done exec script</param>
        /// <param name="bAutoRun">if create script runner and run; false must call GenThread2Run()</param>
        /// <param name="nWaitTimeout">number of milisecond to wait sync begin running a script</param>
        /// <param name="callbackMgr">delegate callback to back manager</param>
        /// <param name="bHandleMacroPropagationAtTheEnd">handle all propagations when end exec script</param>
        /// <returns>an instance of script runner</returns>
        public static UScriptRunner NewScriptRunner3( object contextForFp, HandleScriptDoneCallType1 fp,
            UScript scriptToRun, string wrFilePath, bool bAutoRun = true, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bHandleMacroPropagationAtTheEnd = true )
        {
            UScriptRunner r = new UScriptRunner( contextForFp, fp, scriptToRun, wrFilePath, nWaitTimeout, callbackMgr, bHandleMacroPropagationAtTheEnd );
            if (bAutoRun) r.GenThread2Run();
            return r;
        }

        /// <summary>
        /// New a instance of script runner with thread inside and write results thru fp, so fp must be well prepared
        /// </summary>
        /// <param name="contextForFp">context of fp</param>
        /// <param name="fp">exec delegate when done call</param>
        /// <param name="scriptToRun">a script instance to run</param>
        /// <param name="bAutoRun">if create script runner and run; false must call GenThread2Run()</param>
        /// <param name="nWaitTimeout">number of milisecond to wait sync begin running a script</param>
        /// <param name="callbackMgr">delegate callback to back manager</param>
        /// <param name="bHandleMacroPropagationAtTheEnd">handle all propagations when end exec script</param>
        /// <returns>an instance of script runner</returns>
        public static UScriptRunner NewScriptRunner4( object contextForFp, HandleScriptDoneCallType2 fp,
            UScript scriptToRun, bool bAutoRun = true, int nWaitTimeout = 0, HandleScriptRunnerManagerCall callbackMgr = null, bool bHandleMacroPropagationAtTheEnd = true )
        {
            UScriptRunner r = new UScriptRunner( contextForFp, fp, scriptToRun, nWaitTimeout, callbackMgr, bHandleMacroPropagationAtTheEnd );
            if (bAutoRun) r.GenThread2Run();
            return r;
        }

        /// <summary>
        /// New a instance of script runner with thread inside and the script is able to call another script
        /// </summary>
        /// <param name="contextForFp">context of fp</param>
        /// <param name="fp"cal delegate when end of exec script></param>
        /// <param name="scriptToRun">script instance to run</param>
        /// <param name="scriptSet">all scripts of current environment</param>
        /// <param name="bAutoRun">if create script runner and run; false must call GenThread2Run()</param>
        /// <param name="nWaitTimeout">number of milisecond to wait sync begin running a script</param>
        /// <param name="callbackMgr">delegate callback to back manager</param>
        /// <param name="bHandleMacroPropagationAtTheEnd">handle all propagations when end exec script</param>
        /// <returns>an instance of script runner</returns>
        public static UScriptRunner NewScriptRunner5( object contextForFp, HandleScriptDoneCallType3 fp,
            UScript scriptToRun, List< UScript > scriptSet, bool bAutoRun = true, int nWaitTimeout = 0,
            HandleScriptRunnerManagerCall callbackMgr = null, bool bHandleMacroPropagationAtTheEnd = true )
        {
            UScriptRunner r = new UScriptRunner( contextForFp, fp, scriptToRun, scriptSet, nWaitTimeout, callbackMgr, bHandleMacroPropagationAtTheEnd );
            if (bAutoRun) r.GenThread2Run();
            return r;
        }

        private bool m_bDisposing = false;
        private bool m_bDisposed = false;
        private object m_sync = new object();
        private DateTime m_timeStamp = DateTime.Now;
        private int m_nSN = 0;

        private Thread m_threadRecycle = null;
        private AutoResetEvent m_evtWakeUpRecyc = null;
        private List<UScriptRunner> m_listRecycleRunners = new List< UScriptRunner >();
        private object m_syncRecyList = new object();

        private List<UScriptRunner> m_listRunning = new List< UScriptRunner >();
        private Dictionary<string, UScriptRunner> m_mapRunning = new Dictionary< string, UScriptRunner >();

        public string[] RunningScriptIDs
        {
            get
            {
                string[] repo = null;
                Monitor.Enter( m_sync );
                try
                {
                    repo = m_mapRunning.Keys.ToArray();
                }finally{Monitor.Exit( m_sync );}

                return repo;
            }
        }

        public string[] InvokeMainThreadRunningScripts
        {
            get
            {
                List<string> ret = new List<string>();
                Monitor.Enter( m_sync );
                try
                {
                    foreach(var kv in m_mapRunning)
                    {
                        if (kv.Value.KeepBeginInvoke != null)
                        {
                            ret.Add( kv.Key );
                        }
                    }
                }
                finally { Monitor.Exit( m_sync ); }

                return ret.ToArray();
            }
        }

        private UScriptRunnerFactory()
        {
            m_evtWakeUpRecyc = new AutoResetEvent( false );
        }

        private void CallMgrToHandle( UScriptRunner r )
        {
            if ( r == null || m_bDisposing || m_bDisposed )
                return;

            Monitor.Enter( m_sync );
            try
            {
                if ( !m_bDisposing && !m_bDisposed )
                {
                    // remove from source
                    m_mapRunning.Remove( r.strID );
                    m_listRunning.Remove( r );

                    // add to recycle list
                    Monitor.Enter( m_syncRecyList );
                    m_listRecycleRunners.Add( r );
                    Monitor.Exit( m_syncRecyList );

                    // trigger process
                    m_evtWakeUpRecyc.Set();
                }
            } finally { Monitor.Exit( m_sync ); }
        }

        public void Dispose()
        {
            Monitor.Enter( m_sync );
            try
            {
                if ( !m_bDisposing && !m_bDisposed )
                {
                    m_bDisposing = true;

                    //
                    // call to stop thread
                    //
                    m_evtWakeUpRecyc.Set();
                    m_threadRecycle?.Join();

                    //
                    // free resource
                    //
                    m_mapRunning.Clear();

                    while ( m_listRecycleRunners.Count > 0 )
                    {
                        UScriptRunner r = m_listRecycleRunners[ 0 ];
                        m_listRecycleRunners.RemoveAt( 0 );
                        r?.Dispose();
                    }

                    while ( m_listRunning.Count > 0 )
                    {
                        UScriptRunner r = m_listRunning[ 0 ];
                        m_listRunning.RemoveAt( 0 );
                        r?.Dispose();
                    }

                    m_evtWakeUpRecyc.Dispose();

                    m_bDisposed = true;
                    m_bDisposing = false;
                }
            } finally{ Monitor.Exit( m_sync ); }
        }

        private string GetSN()
        {
            int sn = 0;
            DateTime n = DateTime.Now;
            if ( n.Year != m_timeStamp.Year || n.Month != m_timeStamp.Month || n.Day != m_timeStamp.Day ||
                 n.Hour != m_timeStamp.Hour || n.Minute != m_timeStamp.Minute || n.Second != m_timeStamp.Second )
            {
                sn = m_nSN = 0;
                m_timeStamp = n;
            }
            else
            {
                sn = m_nSN;
                m_nSN = m_nSN == int.MaxValue ? 0 : ++m_nSN;
            }

            return
                $"{m_timeStamp.Year:0000}{m_timeStamp.Month:00}{m_timeStamp.Day:00}{m_timeStamp.Hour:00}{m_timeStamp.Minute:00}{m_timeStamp.Second:00}_{sn}";
        }

        private static void SetVarParams( UScript s, Dictionary< int, UDataCarrier[] > varParams )
        {
            if ( s == null || varParams == null ) return;
            // config variable param to run
            foreach ( var vp in varParams )
            {
                if ( vp.Key >= 0 && vp.Key < s.MacroSet.Count )
                {
                    if ( s.MacroSet[ vp.Key ].VariableParamTypeDesc != null )
                    {
                        if ( UDataCarrier.TypesCheck( vp.Value,
                            s.MacroSet[ vp.Key ].VariableParamTypeDesc ) )
                            s.MacroSet[ vp.Key ].ParameterCarrierVariable = vp.Value;
                    }
                    else s.MacroSet[ vp.Key ].ParameterCarrierVariable = vp.Value;
                }
            }
        }

        private bool ScriptReadyToGo(UScript instance)
        {
            if ( instance == null ) return false;
            if ( m_bDisposing || m_bDisposed || !ResourceManager.SystemAvaliable )
                return false;
            try
            {
                // check if inside running list
                foreach(var inst in m_listRunning)
                {
                    if ( inst == null) continue;
                    if ( inst.RunningScript == instance )
                    {
                        return false;
                    }
                }
                // check if inside recycle list
                foreach ( var inst in m_listRecycleRunners )
                {
                    if ( inst == null ) continue;
                    if ( inst.RunningScript == instance )
                    {
                        return false;
                    }
                }
                return true;
            }
            catch { return false; }
        }

        private bool SyncMutex(object sync, int ms = 0)
        {
            bool bInvokeMain = ResourceManager.IsMainThreadRunning();
            if ( bInvokeMain )
            {
                var prevT = DateTime.Now;
                while ( true )
                {
                    if ( Monitor.TryEnter( sync, 0 ) )
                        return true;

                    ResourceManager.ThreadPoolingWaiting();
                    if ( m_bDisposed || m_bDisposing )
                        break;
                    var diff = DateTime.Now - prevT;
                    if ( diff.TotalMilliseconds > ms )
                        break;
                }

                return false;
            }
            
            return Monitor.TryEnter(sync, ms);
        }

        public string NewRunner0( object contextForFp, HandleScriptDoneCallType1 fp,
            UScript scriptToRun, Dictionary<int, UDataCarrier[]> varParams = null, bool bFreeMacroPropagationAtEnd = true, 
            int nWaitTimeout = 0, UInt64 nExecTimes = 1, bool bEnableOnErrorEnd = false, Action<object> CallbackLoopEnd = null, bool bEnableEvalTime = false )
        {
            if ( scriptToRun == null || !ResourceManager.SystemAvaliable ) return null;
            string ret = "";

            if ( SyncMutex( m_sync, 50 ) )
            {
                try
                {
                    if ( ScriptReadyToGo( scriptToRun ) )
                    {
                        SetVarParams( scriptToRun, varParams );

                        // create manual start
                        UScriptRunner r = NewScriptRunner1( contextForFp, fp, scriptToRun, false, nWaitTimeout,
                            CallMgrToHandle, bFreeMacroPropagationAtEnd, nExecTimes );
                        r.strID = GetSN();
                        r.EndOnError = bEnableOnErrorEnd;
                        r.LoopEndCall = CallbackLoopEnd;

                        // time eval
                        if ( !bEnableEvalTime )
                            scriptToRun.EnableAllMacroEvalExecTime( false ); // just reset flag
                        else
                            scriptToRun.ResetAllMacroExecTime(); // reset all and enable

                        m_mapRunning.Add( r.strID, r );
                        m_listRunning.Add( r );

                        r.GenThread2Run( nExecTimes );
                        ret = r.strID;
                    }
                } finally { Monitor.Exit( m_sync ); }
            }

            return ret;
        }

        public string NewRunnerRepoStream( object contextForFp, HandleScriptDoneCallType1 fp,
            UScript scriptToRun, Stream s, Dictionary<int, UDataCarrier[]> varParams = null, bool bFreeMacroPropagationAtEnd = true, 
            int nWaitTimeout = 0, UInt64 nExecTimes = 1, bool bEnableOnErrorEnd = false, Action<object> CallbackLoopEnd = null, bool bEnableEvalTime = false )
        {
            if ( scriptToRun == null || !ResourceManager.SystemAvaliable ) return null;
            string ret = "";

            if ( SyncMutex( m_sync, 50 ) )
            {
                try
                {
                    if ( ScriptReadyToGo(scriptToRun) )
                    {
                        SetVarParams( scriptToRun, varParams );

                        // create manual start
                        UScriptRunner r = NewScriptRunner2( contextForFp, fp, scriptToRun, s, false, nWaitTimeout,
                            CallMgrToHandle, bFreeMacroPropagationAtEnd );
                        r.strID = GetSN();
                        r.EndOnError = bEnableOnErrorEnd;
                        r.LoopEndCall = CallbackLoopEnd;

                        // time eval
                        if ( !bEnableEvalTime )
                            scriptToRun.EnableAllMacroEvalExecTime( false ); // just reset flag
                        else
                            scriptToRun.ResetAllMacroExecTime(); // reset all and enable

                        m_mapRunning.Add( r.strID, r );
                        m_listRunning.Add( r );

                        r.GenThread2Run( nExecTimes );
                        ret = r.strID;
                    }
                } finally { Monitor.Exit( m_sync ); }
            }

            return ret;
        }

        public string NewRunnerRepoFile( object contextForFp, HandleScriptDoneCallType1 fp,
            UScript scriptToRun, string wrFilePath, Dictionary<int, UDataCarrier[]> varParams = null, bool bFreeMacroPropagationAtEnd = true,
            int nWaitTimeout = 0, UInt64 nExecTimes = 1, bool bEnableOnErrorEnd = false, Action<object> CallbackLoopEnd = null, bool bEnableEvalTime = false )
        {
            if ( scriptToRun == null ) return null;
            string ret = "";

            if ( SyncMutex( m_sync, 50 ) )
            {
                try
                {
                    if ( ScriptReadyToGo(scriptToRun) )
                    {
                        SetVarParams( scriptToRun, varParams );

                        // create manual start
                        UScriptRunner r = NewScriptRunner3( contextForFp, fp, scriptToRun, wrFilePath, false,
                            nWaitTimeout, CallMgrToHandle, bFreeMacroPropagationAtEnd );
                        r.strID = GetSN();
                        r.EndOnError = bEnableOnErrorEnd;
                        r.LoopEndCall = CallbackLoopEnd;

                        // time eval
                        if ( !bEnableEvalTime )
                            scriptToRun.EnableAllMacroEvalExecTime( false ); // just reset flag
                        else
                            scriptToRun.ResetAllMacroExecTime(); // reset all and enable

                        m_mapRunning.Add( r.strID, r );
                        m_listRunning.Add( r );

                        r.GenThread2Run( nExecTimes );
                        ret = r.strID;
                    }
                } finally { Monitor.Exit( m_sync ); }
            }

            return ret;
        }

        public string NewRunnerRepoByte( object contextForFp, HandleScriptDoneCallType2 fp,
            UScript scriptToRun, Dictionary<int, UDataCarrier[]> varParams = null, bool bFreeMacroPropagationAtEnd = true, 
            int nWaitTimeout = 0, UInt64 nExecTimes = 1, bool bEnableOnErrorEnd = false, Action<object> CallbackLoopEnd = null, bool bEnableEvalTime = false )
        {
            if ( scriptToRun == null ) return null;
            string ret = "";

            if ( Monitor.TryEnter( m_sync, 50 ) )
            {
                try
                {
                    if ( ScriptReadyToGo( scriptToRun ) )
                    {
                        SetVarParams( scriptToRun, varParams );

                        // create manual start
                        UScriptRunner r = NewScriptRunner4( contextForFp, fp, scriptToRun, false, nWaitTimeout,
                            CallMgrToHandle, bFreeMacroPropagationAtEnd );
                        r.strID = GetSN();
                        r.EndOnError = bEnableOnErrorEnd;
                        r.LoopEndCall = CallbackLoopEnd;

                        // time eval
                        if ( !bEnableEvalTime )
                            scriptToRun.EnableAllMacroEvalExecTime( false ); // just reset flag
                        else
                            scriptToRun.ResetAllMacroExecTime(); // reset all and enable

                        m_mapRunning.Add( r.strID, r );
                        m_listRunning.Add( r );

                        r.GenThread2Run( nExecTimes );
                        ret = r.strID;
                    }
                } finally { Monitor.Exit( m_sync ); }
            }

            return ret;
        }

        public string NewRunnerFlowCtrl( object contextForFp, HandleScriptDoneCallType3 fp,
            UScript scriptToRun, List< UScript > scriptSet, Dictionary<int, UDataCarrier[]> varParams = null, 
            int nWaitTimeout = 0, UInt64 nExecTimes = 1, bool bEnableOnErrorEnd = false, Action<object> CallbackLoopEnd = null, bool bEnableEvalTime = false )
        {
            if ( scriptToRun == null ) return null;
            string ret = "";

            if ( SyncMutex( m_sync, 50 ) )
            {
                try
                {
                    if ( ScriptReadyToGo( scriptToRun ) )
                    {
                        SetVarParams( scriptToRun, varParams );

                        // create manual start
                        UScriptRunner r = NewScriptRunner5( contextForFp, fp, scriptToRun, scriptSet, false,
                            nWaitTimeout, CallMgrToHandle );
                        r.strID = GetSN();
                        r.EndOnError = bEnableOnErrorEnd;
                        r.LoopEndCall = CallbackLoopEnd;

                        // time eval
                        if ( !bEnableEvalTime )
                            scriptToRun.EnableAllMacroEvalExecTime( false ); // just reset flag
                        else
                            scriptToRun.ResetAllMacroExecTime(); // reset all and enable

                        m_mapRunning.Add( r.strID, r );
                        m_listRunning.Add( r );

                        r.GenThread2Run( nExecTimes );
                        ret = r.strID;
                    }
                } finally { Monitor.Exit( m_sync ); }
            }

            return ret;
        }

        private bool ScriptInsideRecycle( UScript s )
        {
            bool bInvokeMain = ResourceManager.IsMainThreadRunning();
            if ( bInvokeMain )
            {
                while ( true )
                {
                    if ( Monitor.TryEnter( m_syncRecyList, 0 ) )
                        break;
                    ResourceManager.ThreadPoolingWaiting();
                    if ( m_bDisposed || m_bDisposing )
                        return true;
                }
            }
            else
                Monitor.Enter( m_syncRecyList );

            bool insdie = false;
            try
            {
                foreach ( var recycle in m_listRecycleRunners )
                {
                    if ( recycle.RunningScript == s )
                    {
                        insdie = true;
                        break;
                    }
                }
            }
            finally { Monitor.Exit( m_syncRecyList ); }

            return insdie;
        }


        public bool RunScript(
            object ctxForFp, HandleScriptDoneCallType4 fp,
            string scriptName, out string runnerIdOrMsg,
            Dictionary<int, UDataCarrier[]> variableParams = null,
            UDataCarrier[] input = null, fpUDataCarrierSetResHandler inputHandler = null,
            bool chkScriptRunning = true,
            int nWaitTimeout = 0,
            UInt64 nMaxExec = 1 )
        {
            runnerIdOrMsg = "";
            if ( m_bDisposing || m_bDisposed )
            {
                runnerIdOrMsg = "disposed";
                return false;
            }

            UScript s = ULibAgent.Singleton.Scripts?.GetScript( scriptName ) ?? null;
            if ( s == null )
            {
                runnerIdOrMsg = $"not find script({scriptName})";
                return false;
            }

            if ( ScriptInsideRecycle( s ) )
            {
                runnerIdOrMsg = $"script({scriptName}) still in recycle";
                return false;
            }

            var runWithMain = false;
            var runWithAnotherScript = false;
            foreach ( var m in s.MacroSet )
            {
                if ( !runWithMain && m.AbilityToInteractWithUI )
                    runWithMain = true;
                if ( !runWithAnotherScript && m.AbilityToJumpAnotherScript )
                    runWithAnotherScript = true;
            }

            UScriptRunner r = null;
            if ( !s.UnderRunning || !( s.UnderRunning && chkScriptRunning ) )
            {
                if ( runWithAnotherScript )
                    r = NewScriptRunner5( ctxForFp, null, s, ULibAgent.Singleton.Scripts.Scripts, false, callbackMgr: CallMgrToHandle );
                else
                    r = NewScriptRunner1( ctxForFp, null, s, false, callbackMgr: CallMgrToHandle, nExecTimes: nMaxExec );
            }
            else
            {
                runnerIdOrMsg = $"script({scriptName}) is running";
                return false;
            }

            SetVarParams( s, variableParams );
            r.BegPrepropagation = input;
            r.BegPrepropagationHandler = inputHandler;

            r.Callback4 = fp;
            r.strID = GetSN();
            m_mapRunning.Add( r.strID, r );
            m_listRunning.Add( r );

            if ( runWithMain )
                r.InvokeMain2Run();
            else
                r.GenThread2Run();
            runnerIdOrMsg = r.strID;

            return true;
        }

        public UScriptRunner GetRunner( string id )
        {
            if ( m_bDisposing || m_bDisposed ) return null;
            UScriptRunner repo = null;
            if ( SyncMutex( m_sync, 100 ) )
            {
                try
                {
                    repo = m_mapRunning.ContainsKey( id ) ? m_mapRunning[ id ] : null;
                } finally{Monitor.Exit( m_sync );}
            }

            return repo;
        }

        public bool Cancel( string id, HandleCancelScriptCallback callback = null, object ctx = null )
        {
            bool ret = false;
            if ( SyncMutex( m_sync, 50 ) )
            {
                try
                {
                    if ( !m_bDisposed && !m_bDisposing )
                    {
                        if ( m_mapRunning.ContainsKey( id ) )
                        {
                            m_mapRunning[id]?.Cancel(callback, ctx);
                            ret = true;
                        }
                    }
                } finally { Monitor.Exit( m_sync ); }
            }

            return ret;
        }

        public void Cancel( string[] runnings )
        {
            if ( runnings == null || runnings.Length == 0 )
                return;

            if ( SyncMutex( m_sync, 50 ) )
            {
                try
                {
                    if ( !m_bDisposed && !m_bDisposing )
                    {
                        foreach(var r in runnings)
                        {
                            if ( m_mapRunning.ContainsKey( r ) )
                            {
                                m_mapRunning[ r ]?.Cancel();
                            }
                        }
                    }
                }
                finally { Monitor.Exit( m_sync ); }
            }
        }

        private UScriptRunner GetRecycleItem()
        {
            if ( m_bDisposing || m_bDisposed ) return null;
            UScriptRunner repo = null;
            if ( m_listRecycleRunners.Count > 0 )
            {
                repo = m_listRecycleRunners[ 0 ];
                m_listRecycleRunners.RemoveAt( 0 );
            }

            return repo;
        }

        private void DoRecycle()
        {
            while ( !m_bDisposing && !m_bDisposed )
            {
                if ( m_evtWakeUpRecyc.WaitOne( -1 ) )
                {
                    for ( ;; )
                    {
                        UScriptRunner sr = null;
                        Monitor.Enter( m_syncRecyList );
                        try
                        {
                            sr = GetRecycleItem();
                            sr?.Dispose();
                        }
                        catch { }
                        finally { Monitor.Exit( m_syncRecyList ); }
                        if ( sr == null || m_bDisposing || m_bDisposed ) break;
                    }
                }
            }
        }

        private void GenThread()
        {
            if ( m_threadRecycle != null ) return;
            m_threadRecycle = new Thread( new ParameterizedThreadStart(ExecRecycle) );
            m_threadRecycle.Start(this);
        }

        private static void ExecRecycle( object instance )
        {
            (instance as UScriptRunnerFactory)?.DoRecycle();
        }
    }
}
