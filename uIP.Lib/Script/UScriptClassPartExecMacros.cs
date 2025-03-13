using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using uIP.Lib.DataCarrier;
using uIP.Lib.Utility;

namespace uIP.Lib.Script
{
    public partial class UScript
    {
        // flags
        private bool m_bUnderConfiguration = false; // true: in UI configuring
        private bool m_bUnderRunning = false;
        private bool m_bCanLogOut = false;

        // Parameter
        private bool m_bOnErrorGoAnyway = false;
        private bool m_bSyncCall = true;
        private Int32 m_nSyncWaitMS = 0;
        //private bool m_bExecIncludeGotoFunc = false;

        // ResultSet
        private string m_strOErrorMethod = null;
        private string m_strStatusMessage = null;
        private List<UMacroProduceCarrierResult> m_MacroResultCarriers = new List<UMacroProduceCarrierResult>();
        private List<UMacroProduceCarrierPropagation> m_MacroPropagationCarriers = new List<UMacroProduceCarrierPropagation>();
        private List<UMacroProduceCarrierDrawingResult> m_MacroDrawingResultCarriers = new List<UMacroProduceCarrierDrawingResult>();
        protected double m_dfExecTimeSpan = 0.0;
        protected UMacro m_LastExecMacro = null;// latest exec macro

        // looping call
        //private bool m_bLoopingStop = false;
        //private fpScriptLoopingEndCallback m_fpLoopingDoneCall = null;

        private object m_syncCancelCall = new object();
        private bool m_bCancelCalling = false;
        private CancelExecScript m_CancelCallInfo = null;

        #region Properties

        // Parameter
        public bool ContinueOnError 
        {
            get => m_bOnErrorGoAnyway;
            set => m_bOnErrorGoAnyway = value;
        }
        public bool SynchronizedCall
        {
            get => m_bSyncCall;
            set => m_bSyncCall = value;
        }
        public Semaphore SynchronizedObject => m_hBinSem;
        public Int32 SyncWaitTimeout 
        { 
            get => m_nSyncWaitMS;
            set => m_nSyncWaitMS = value < -1 ? -1 : value;
        }
        public bool UnderConfiguration 
        { 
            get => m_bUnderConfiguration;
            set => m_bUnderConfiguration = value;
        }
        public bool UnderRunning => m_bUnderRunning;
        public bool EnableLogOut 
        { 
            get => m_bCanLogOut;
            set => m_bCanLogOut = value;
        }
        //public bool EnableGotoFunc 
        //{
        //    get => m_bExecIncludeGotoFunc;
        //    set => m_bExecIncludeGotoFunc = value;
        //}
        public bool AbilityJumpMacro
        {
            get {
                if ( m_listMacros == null || m_listMacros.Count <= 0 ) return false;
                foreach(var m in m_listMacros)
                {
                    if ( m is UMacroCapableOfCtrlFlow )
                        return true;
                }
                return false;
            }
        }
        public bool AbilitySwitchScript
        {
            get
            {
                if ( m_listMacros == null || m_listMacros.Count <= 0 ) return false;
                foreach(var m in m_listMacros)
                {
                    if (m is UMacroExtensions ext && ext != null && ext.CapableOfExecScript)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool InteractWithUI
        {
            get
            {
                if ( m_listMacros == null || m_listMacros.Count <= 0 ) return false;
                foreach ( var m in m_listMacros )
                {
                    if ( m.AbilityToInteractWithUI )
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // ResultSet
        public int PreviousIndex { get; private set; } = -1;
        public int RunningIndex { get; private set; } = -1;
        public string OnErrorMethod => m_strOErrorMethod;
        public int OnErrorIndex { get; protected set; } = -1;
        public string StatusMessage => m_strStatusMessage;
        public List<UMacroProduceCarrierResult> ResultCarriers
        { 
            get => (m_bDisposed ? null : m_MacroResultCarriers);
            set { if ( m_bDisposed ) return; m_MacroResultCarriers = value; }
        }
        public List<UMacroProduceCarrierPropagation> PropagationCarriers
        {
            get => (m_bDisposed ? null : m_MacroPropagationCarriers);
            set { if ( m_bDisposed ) return; m_MacroPropagationCarriers = value; }
        }
        public List<UMacroProduceCarrierDrawingResult> DrawingResultCarriers
        {
            get => (m_bDisposed ? null : m_MacroDrawingResultCarriers);
            set { if ( m_bDisposed ) return; m_MacroDrawingResultCarriers = value; }
        }

        public double TotalExecTime => m_dfExecTimeSpan;
        public UMacro LastExecMacro => m_LastExecMacro;

        // Looping
        //public bool StopLooping { get { return m_bLoopingStop; } set { m_bLoopingStop = value; } }
        //public fpScriptLoopingEndCallback fpLoopingDoneCall { get { return m_fpLoopingDoneCall; } set { m_fpLoopingDoneCall = value; } }

        #endregion

        protected void FreeResultCarriers()
        {
            foreach ( var t in m_MacroResultCarriers )
            {
                if ( t == null ) continue;
                try { t.Dispose(); }
                catch ( Exception exp )
                {
                    fpLog?.Invoke( eLogMessageType.ERROR, 0,
                        $"[UScript::FreeResultCarriers] call {t.PluginClassName}::{t.MacroMethod} from Dispose() with err {exp.ToString()}" );
                }
            }

            m_MacroResultCarriers.Clear();
        }
        protected void FreePropagationCarriers()
        {
            foreach ( var t in m_MacroPropagationCarriers )
            {
                if ( t == null ) continue;
                try { t.Dispose(); }
                catch ( Exception exp )
                {
                    fpLog?.Invoke( eLogMessageType.ERROR, 0,
                        $"[UScript::FreePropagationCarriers] call {t.PluginClassName}::{t.MacroMethod} from Dispose() with err {exp.ToString()}" );
                }
            }

            m_MacroPropagationCarriers.Clear();
        }
        protected void FreeDrawingResultCarriers()
        {
            foreach ( var t in m_MacroDrawingResultCarriers )
            {
                if ( t == null ) continue;
                try { t.Dispose(); }
                catch ( Exception exp )
                {
                    fpLog?.Invoke( eLogMessageType.ERROR, 0,
                        $"[UScript::FreeDrawingResultCarriers] call {t.PluginClassName}::{t.MacroMethod} from Dispose() with err {exp.ToString()}" );
                }
            }

            m_MacroDrawingResultCarriers.Clear();
        }
        protected static void FreeHistoryCarrierSet( List<UScriptHistoryCarrier> list )
        {
            if ( list == null || list.Count <= 0 ) return;
            foreach ( var t in list )
            {
                if ( t == null ) continue;
                t.Dispose();
            }
            list.Clear();
        }

        private List<UDataCarrierSet> ResultsCarrierToSet( bool bGetGroup )
        {
            List<UDataCarrierSet> wrd = new List<UDataCarrierSet>();
            if ( m_MacroResultCarriers != null && m_MacroResultCarriers.Count > 0 && m_listMacros != null && m_listMacros.Count > 0 )
            {
                List<UMacroProduceCarrierResult> for_repo = new List<UMacroProduceCarrierResult>();
                foreach ( var t in m_MacroResultCarriers )
                {
                    if ( t == null )
                        continue;
                    if ( !m_listMacros[ t.IndexOfScript ].ExtractingResultToPack ) // user must be enable the flag
                        continue;
                    if ( bGetGroup )
                        GetProduceCarrierResult( t.IndexOfScript, m_MacroResultCarriers, m_listMacros, for_repo );
                    else
                    {
                        if ( !for_repo.Contains( t ) )
                            for_repo.Add( t );
                    }
                }
                // pack report format
                foreach ( var t in for_repo )
                {
                    if ( t.ResultSet == null )
                        continue;

                    wrd.Add( new UDataCarrierSet( $"{m_strNameOfId}:{t.IndexOfScript}", t.ResultSet, null ) );
                }
            }
            return wrd;
        }

        public bool CancelRunning(HandleCancelScriptCallback fp = null, Object context = null)
        {
            // check if under running
            if ( !m_bUnderRunning || m_listMacros == null || m_listMacros.Count <= 0 ) return false;
            bool bRet = true;
            // sync
            Monitor.Enter( m_syncCancelCall );
            try
            {
                if ( !m_bCancelCalling )
                {
                    m_bCancelCalling = true;

                    if ( m_CancelCallInfo == null )
                    {
                        m_CancelCallInfo = new CancelExecScript( context, fp ){Flag = true, RunningScript = this};
                        foreach ( var m in m_listMacros )
                        {
                            m.CancelExec = true;
                        }
                    }
                    else
                        bRet = false;

                    m_bCancelCalling = false;
                }
                else bRet = false;
            }finally { Monitor.Exit( m_syncCancelCall ); }

            return bRet;
        }

        public ScriptExecReturnCode Running( bool bSyncCall, bool bResetCancel = true,
                                             int nBegIndex = 0, int nEndIndex = -1,
                                             UDataCarrier[] prevPropagation = null, fpUDataCarrierSetResHandler fpHandlePrevPropagation = null,
                                             List<UScriptHistoryCarrier> prevScriptHistorySet = null, bool bHandlePrevHistorySet = true, 
                                             bool bHandleMacroPropagation = false,
                                             bool bResetBeforeExec = true
            )
        {
            if ( m_bDisposed )
            {
                if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                    $"[UScript::Running] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> disposed." );
                return ScriptExecReturnCode.Disposed;
            }
            if ( m_bUnderConfiguration )
            {
                if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                    $"[UScript::Running] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> in configuration." );
                return ScriptExecReturnCode.InConfiguration;
            }

            // --- Critical section enter ---
            bool bObtainSem = false;
            if ( m_bSyncCall && bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync )
                {
                    if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                    if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                    if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                        $"[UScript::Running] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> sync timeout({m_nSyncWaitMS}ms)." );
                    return ScriptExecReturnCode.SyncTimeout;
                }
                bObtainSem = true;
            }

            if ( bResetCancel )
            {
                // clear cancel call info
                Monitor.Enter( m_syncCancelCall );
                try
                {
                    m_CancelCallInfo = null;
                    // clear cancel state
                    foreach ( var t in m_listMacros )
                        t.CancelExec = false;
                } finally { Monitor.Exit( m_syncCancelCall ); }
            }

            var jumpable = false;
            foreach(var m in  m_listMacros )
            {
                if ( m == null ) continue;

                // check existing jumpable
                if (!jumpable && ( m.AbilityToJumpIndex || m.AbilityToJumpAnotherScript) )
                    jumpable = true;
                // reset flag
                m.IsJumpInto = false;
                // notify begin exec
                if ( bResetBeforeExec )
                    m.OwnerOfPluginClass?.SetMacroControl( m, UDataCarrier.MakeOne( UMacroMethodProviderPlugin.PredefMacroIoctl_ScriptBeginExecNotify ), UDataCarrier.MakeVariableItemsArray( this, nBegIndex ) );
            }

            PreviousIndex = -1;
            RunningIndex = -1;
            m_bUnderRunning = true;

            m_dfExecTimeSpan = 0.0;
            m_LastExecMacro = null;

            m_strOErrorMethod = null;
            OnErrorIndex = -1;
            m_strStatusMessage = null;

            UDataCarrier[] currMacroResult = null;
            MacroExecReturnCode currMacroRetCode = MacroExecReturnCode.NA;
            ScriptExecReturnCode retScriptCode = ScriptExecReturnCode.OK;
            int execIndex = 0;
            double dfTimeSpan = 0.0;


            // Free prev result carriers
            FreeResultCarriers();

            // Free prev propagation carriers
            FreePropagationCarriers();

            // Free prev drawing result carriers
            FreeDrawingResultCarriers();

            try
            {
                // adjust nEndIndex
                nEndIndex = nEndIndex < 0 ? (m_listMacros.Count - 1) : nEndIndex;

                // setup macro begin index & prev propagation carrier
                UDataCarrier[] prevPropagationCarrierSet = null;
                if ( nBegIndex >= 0 && nBegIndex < m_listMacros.Count )
                {
                    prevPropagationCarrierSet = prevPropagation;
                    execIndex = nBegIndex;
                    // record prev propagation
                    if ( prevPropagation != null )
                        m_MacroPropagationCarriers.Add( new UMacroProduceCarrierPropagation( null, null, null, -1, prevPropagation, fpHandlePrevPropagation ) );
                }
                else
                    execIndex = 0;

                // exec all macros
                RunningIndex = execIndex;
                for ( ; execIndex < m_listMacros.Count && execIndex <= nEndIndex ; RunningIndex = execIndex )
                {
                    if ( m_listMacros[ execIndex ] == null ) continue;

                    UDataCarrier[] currRetPropagation = null;
                    fpUDataCarrierSetResHandler currRetPropagationHandler = null;
                    UDrawingCarriers currRetDrawResults = null;
                    fpUDataCarrierSetResHandler currRetMacroResultHandler = null;

                    NotifyMacroRunDelegate?.Invoke( NotifyMacroRunDelegateCtx, this, m_listMacros[ execIndex ], execIndex );

                    m_LastExecMacro = m_listMacros[ execIndex ];
                    currMacroResult = m_listMacros[ execIndex ].Exec( execIndex, prevPropagationCarrierSet, m_MacroResultCarriers, m_MacroPropagationCarriers, m_MacroDrawingResultCarriers, prevScriptHistorySet,
                                                                      ref currMacroRetCode, ref currRetPropagation, ref currRetDrawResults, ref currRetPropagationHandler, ref currRetMacroResultHandler );

                    dfTimeSpan = dfTimeSpan + m_listMacros[ execIndex ].ExecTime;

                    // clear data
                    m_listMacros[ execIndex ].IsJumpInto = false;

                    // check invisible flag
                    if ( !m_listMacros[ execIndex ].Invisible )
                    {
                        /* 檢查是否需要將鄰近的macro變成group
                         * - Yes: prevPropagationCarrierSet(傳入propagation parameter)還是使用第一個macro的prevPropagationCarrierSet
                         * - No: 變更prevPropagationCarrierSet為目前的currRetPropagation
                         */
                        UMacro nextone = (execIndex + 1) >= m_listMacros.Count ? null : m_listMacros[ execIndex + 1 ];
                        if ( nextone != null && !nextone.GatherNearbyMacro )
                            prevPropagationCarrierSet = currRetPropagation;
                        // Update propagation
                        UMacroProduceCarrierPropagation existingPropagation = UMacroProduceCarrierPropagation.GetItem( m_MacroPropagationCarriers, execIndex );
                        if ( existingPropagation == null )
                            m_MacroPropagationCarriers.Add( new UMacroProduceCarrierPropagation( m_strNameOfId,
                                                                                                 m_listMacros[ execIndex ].OwnerClassName,
                                                                                                 m_listMacros[ execIndex ].MethodName,
                                                                                                 execIndex,
                                                                                                 currRetPropagation,
                                                                                                 currRetPropagationHandler ) );
                        else
                            existingPropagation.Reset( currRetPropagation, currRetPropagationHandler );
                    }
                    else
                    {
                        /* 是 invisible 將不會儲存propagation, 並釋放它
                         */
                        currRetPropagationHandler?.Invoke( currRetPropagation );
                    }

                    // Store result
                    UMacroProduceCarrierResult existingResult = UMacroProduceCarrierResult.GetItem( m_MacroResultCarriers, execIndex );
                    if ( existingResult == null )
                        m_MacroResultCarriers.Add( new UMacroProduceCarrierResult( m_strNameOfId,
                                                                              m_listMacros[ execIndex ].OwnerClassName,
                                                                              m_listMacros[ execIndex ].MethodName,
                                                                              execIndex,
                                                                              currMacroResult,
                                                                              currRetMacroResultHandler ) );
                    else
                        existingResult.ResetResult( currMacroResult, currRetMacroResultHandler );
                    // Store drawing
                    UMacroProduceCarrierDrawingResult existingDraw = UMacroProduceCarrierDrawingResult.GetItem( m_MacroDrawingResultCarriers, execIndex );
                    if ( existingDraw == null )
                        m_MacroDrawingResultCarriers.Add( new UMacroProduceCarrierDrawingResult( m_strNameOfId,
                                                                 m_listMacros[ execIndex ].OwnerClassName,
                                                                 m_listMacros[ execIndex ].MethodName,
                                                                 execIndex,
                                                                 currRetDrawResults ) );
                    else
                        existingDraw.ResetDrawing( currRetDrawResults );

                    if ( currMacroRetCode != MacroExecReturnCode.OK )
                    {
                        retScriptCode = ScriptExecReturnCode.MacroExecErr;
                        if ( !m_bOnErrorGoAnyway )
                        {
                            // Error
                            // - log it
                            // - break current script
                            m_strOErrorMethod = m_listMacros[ execIndex ].MethodName;
                            OnErrorIndex = execIndex;
                            m_strStatusMessage = m_listMacros[ execIndex ].ExecStatusMessage;

                            if ( m_bCanLogOut )
                                fpLog?.Invoke( eLogMessageType.ERROR, 0,
                                    $"[UScript::Running] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> call macro({execIndex},{( string.IsNullOrEmpty( m_listMacros[ execIndex ].OwnerClassName ) ? "" : m_listMacros[ execIndex ].OwnerClassName )},{( string.IsNullOrEmpty( m_listMacros[ execIndex ].MethodName ) ? "" : m_listMacros[ execIndex ].MethodName )}) err({currMacroRetCode.ToString()}) with msg({( string.IsNullOrEmpty( m_strStatusMessage ) ? "" : m_strStatusMessage )}).{( string.IsNullOrEmpty( m_listMacros[ execIndex ].ExceptionString ) ? "" : ( "\nException:" + m_listMacros[ execIndex ].ExceptionString ) )}" );
                            break;
                        }
                    }
                    if ( currMacroRetCode == MacroExecReturnCode.MacroBreak )
                    {
                        if ( m_bCanLogOut )
                            fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                                $"[UScript::Running] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> call macro({execIndex},{( string.IsNullOrEmpty( m_listMacros[ execIndex ].OwnerClassName ) ? "" : m_listMacros[ execIndex ].OwnerClassName )},{( string.IsNullOrEmpty( m_listMacros[ execIndex ].MethodName ) ? "" : m_listMacros[ execIndex ].MethodName )}) code = {currMacroRetCode.ToString()}." );
                        break;
                    }

                    // log current
                    if ( m_bCanLogOut )
                        fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                            $"[UScript::Running] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> call macro({execIndex},{( string.IsNullOrEmpty( m_listMacros[ execIndex ].OwnerClassName ) ? "" : m_listMacros[ execIndex ].OwnerClassName )},{( string.IsNullOrEmpty( m_listMacros[ execIndex ].MethodName ) ? "" : m_listMacros[ execIndex ].MethodName )}) code = {currMacroRetCode.ToString()}, time spane = {m_listMacros[ execIndex ].ExecTime:F2} ms" );
                    // control if looping & inc execIndex
                    //if ( m_bExecIncludeGotoFunc )
                    if ( jumpable )
                    {
                        // try to convert jumping macro
                        if ( m_listMacros[ execIndex ] is UMacroCapableOfCtrlFlow jumpingMacro && jumpingMacro.MustJump )
                        {
                            if ( jumpingMacro.Jump2WhichMacro == ( int )MacroGotoFunctions.GOTO_END )
                                break;
                            if ( jumpingMacro.Jump2WhichMacro < 0 || jumpingMacro.Jump2WhichMacro >= m_listMacros.Count )
                            {
                                m_strOErrorMethod = m_listMacros[ execIndex ].MethodName;
                                OnErrorIndex = execIndex;
                                m_strStatusMessage = String.Format( "Goto out-of-range" );
                                break;
                            }
                            if ( execIndex == ( m_listMacros.Count - 1 ) ) // current is last one
                                prevPropagationCarrierSet = currRetPropagation; // update to prev to make sure next calling success
                            PreviousIndex = execIndex;
                            execIndex = jumpingMacro.Jump2WhichMacro; // update exec index
                            m_listMacros[ execIndex ].IsJumpInto = true; // mark as jump into this macro
                        }
                        else
                        {
                            PreviousIndex = execIndex;
                            execIndex++; // inc exec index
                        }
                    }
                    else
                    {
                        PreviousIndex = execIndex;
                        execIndex++;
                    }
                } // end for-execIndex
            }
            catch ( Exception exp )
            {
                if ( fpLog != null )
                {
                    if ( execIndex >= 0 && execIndex < m_listMacros.Count )
                        fpLog( eLogMessageType.ERROR, 0,
                            $"[UScript::Running]Exec {m_strNameOfId} get exception in {m_listMacros[ execIndex ].MethodName}." );
                    fpLog( eLogMessageType.ERROR, 0, $"Exception:\n{exp}\n\nDump:\n{CommonUtilities.Dump01( exp )}" );
                }
                retScriptCode = ScriptExecReturnCode.Exception;
            }
            finally
            {
                m_dfExecTimeSpan = dfTimeSpan;

                // log time span
                if ( m_bCanLogOut )
                    fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                        $"[UScript::Running] exec {m_strNameOfId} time span = {dfTimeSpan:F2} ms" );

                // free prev results
                if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );

                // check to reset info
                if (NotifyMacroRunDelegateAutoReset)
                {
                    NotifyMacroRunDelegate = null;
                    NotifyMacroRunDelegateCtx?.Dispose();
                    NotifyMacroRunDelegateCtx = null;
                    NotifyMacroRunDelegateAutoReset = false;
                }

                m_bUnderRunning = false;

                // check the cancel call info
                Monitor.Enter( m_syncCancelCall );
                try
                {
                    if ( m_CancelCallInfo != null )
                    {
                        try { m_CancelCallInfo.CancelCall(); } 
                        catch ( Exception e )
                        {
                            fpLog?.Invoke( eLogMessageType.WARNING, 0,
                                $"Cancel exec script callback with error: {e.ToString()}" );
                        }

                        m_CancelCallInfo = null;
                    }
                }finally{ Monitor.Exit( m_syncCancelCall ); }

                if ( bHandleMacroPropagation )
                {
                    FreePropagationCarriers();
                    //FreeResultCarriers();
                    //FreeDrawingResultCarriers();
                }

                // --- Critical section leave ---
                if ( bObtainSem ) m_hBinSem.Release();
            }

            return retScriptCode;
        }

        public ScriptExecReturnCode RunningRepoBuff( bool bSyncCall, int nSyncTimeout, out byte[] buffFormatedResults,
                                                     int nBegIndex = 0, int nEndIndex = -1,
                                                     UDataCarrier[] prevPropagation = null, fpUDataCarrierSetResHandler fpHandlePrevPropagation = null,
                                                     List<UScriptHistoryCarrier> prevScriptHistorySet = null, bool bHandlePrevHistorySet = true,
                                                     bool bHandleMacroPropagation = false )
        {
            buffFormatedResults = null;
            if ( m_bDisposed )
            {
                if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                    $"[UScript::RunningRepoBuff] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> disposed." );
                return ScriptExecReturnCode.Disposed;
            }

            bool bCriticalSecEnt = false;
            if ( bSyncCall )
            {
                if ( !m_hBinSem.WaitOne( nSyncTimeout, true ) )
                {
                    if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                    if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                    if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                        $"[UScript::RunningRepoBuff] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> sync timeout({m_nSyncWaitMS}ms)." );
                    return ScriptExecReturnCode.SyncTimeout;
                }
                bCriticalSecEnt = true;
            }

            ScriptExecReturnCode retCode = Running( false, true, nBegIndex, nEndIndex, prevPropagation, fpHandlePrevPropagation, prevScriptHistorySet, bHandlePrevHistorySet, bHandleMacroPropagation );
            if ( retCode == ScriptExecReturnCode.OK )
                buffFormatedResults = ProduceResultCarrierKeeper.ProduceResultCarriersToByteArray( true, m_MacroResultCarriers, m_listMacros );

            if ( bCriticalSecEnt ) m_hBinSem.Release();

            return retCode;
        }

        public ScriptExecReturnCode RunningRepo2File( bool bSyncCall, int nSyncTimeout, string filepath,
                                                      int nBegIndex = 0, int nEndIndex = -1,
                                                      UDataCarrier[] prevPropagation = null, fpUDataCarrierSetResHandler fpHandlePrevPropagation = null,
                                                      List<UScriptHistoryCarrier> prevScriptHistorySet = null, bool bHandlePrevHistorySet = true,
                                                      bool bHandleMacroPropagation = false,
                                                      bool bResetBeforeExec = true
            )
        {
            if ( m_bDisposed )
            {
                if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                    $"[UScript::RunningRepo2File] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> disposed." );
                return ScriptExecReturnCode.Disposed;
            }

            bool bCriticalSecEnt = false;
            if ( bSyncCall )
            {
                if ( !m_hBinSem.WaitOne( nSyncTimeout, true ) )
                {
                    if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                    if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                    if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                        $"[UScript::RunningRepo2File] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> sync timeout({m_nSyncWaitMS}ms)." );
                    return ScriptExecReturnCode.SyncTimeout;
                }
                bCriticalSecEnt = true;
            }

            ScriptExecReturnCode retCode = Running( false, true, nBegIndex, nEndIndex, 
                prevPropagation, fpHandlePrevPropagation, 
                prevScriptHistorySet, bHandlePrevHistorySet, 
                bHandleMacroPropagation, bResetBeforeExec );
            if ( retCode == ScriptExecReturnCode.OK )
                UDataCarrier.WriteXml( ResultsCarrierToSet( true ), filepath );

            if ( bCriticalSecEnt ) m_hBinSem.Release();

            return retCode;
        }

        public ScriptExecReturnCode RunningRepo2Stream( bool bSyncCall, int nSyncTimeout, Stream ws,
                                                      int nBegIndex = 0, int nEndIndex = -1,
                                                      UDataCarrier[] prevPropagation = null, fpUDataCarrierSetResHandler fpHandlePrevPropagation = null,
                                                      List<UScriptHistoryCarrier> prevScriptHistorySet = null, bool bHandlePrevHistorySet = true,
                                                      bool bHandleMacroPropagation = false,
                                                      bool bResetBeforeExec = true
            )
        {
            if ( m_bDisposed )
            {
                if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                    $"[UScript::RunningRepo2File] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> disposed." );
                return ScriptExecReturnCode.Disposed;
            }

            bool bCriticalSecEnt = false;
            if ( bSyncCall )
            {
                if ( !m_hBinSem.WaitOne( nSyncTimeout, true ) )
                {
                    if ( fpHandlePrevPropagation != null && prevPropagation != null ) fpHandlePrevPropagation( prevPropagation );
                    if ( prevScriptHistorySet != null && bHandlePrevHistorySet ) FreeHistoryCarrierSet( prevScriptHistorySet );
                    if ( m_bCanLogOut ) fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                        $"[UScript::RunningRepo2File] <Name={( string.IsNullOrEmpty( m_strNameOfId ) ? "" : m_strNameOfId )}, ID={m_nSnOfId}> sync timeout({m_nSyncWaitMS}ms)." );
                    return ScriptExecReturnCode.SyncTimeout;
                }
                bCriticalSecEnt = true;
            }

            ScriptExecReturnCode retCode = Running( false, true, nBegIndex, nEndIndex, 
                prevPropagation, fpHandlePrevPropagation, 
                prevScriptHistorySet, bHandlePrevHistorySet, 
                bHandleMacroPropagation, bResetBeforeExec );
            if ( retCode == ScriptExecReturnCode.OK )
                UDataCarrier.WriteXml( ResultsCarrierToSet( true ), ws );

            if ( bCriticalSecEnt ) m_hBinSem.Release();

            return retCode;
        }

        public void ResetAllMacroExecTime(bool enabled = true)
        {
            if ( m_bDisposed || m_listMacros == null ) return;
            try
            {
                foreach ( var m in m_listMacros )
                    m?.ResetExecTimeData( enabled );
            }
            catch { }
        }
        public void ResetMacroExecTime( int index0, bool enabled = true )
        {
            if ( m_bDisposed || m_listMacros == null ) return;
            try
            {
                if ( index0 < 0 || index0 >= m_listMacros.Count ) return;
                m_listMacros[ index0 ]?.ResetExecTimeData();
            }
            catch { }
        }
        public void ResetAllAvgTime()
        {
            if ( m_bDisposed || m_listMacros == null ) return;
            try
            {
                foreach ( var m in m_listMacros )
                    m?.ResetAvgExecTimeData();
            } catch { }
        }
        public void ResetMacroAvgTime(int index0)
        {
            if ( m_bDisposed || m_listMacros == null ) return;
            try
            {
                if ( index0 < 0 || index0 >= m_listMacros.Count )
                    return;
                m_listMacros[ index0 ]?.ResetAvgExecTimeData();
            } catch { }
        }
        public void EnableAllMacroEvalExecTime(bool enabled = true)
        {
            if ( m_bDisposed || m_listMacros == null ) return;
            try
            {
                foreach ( var m in m_listMacros )
                {
                    if ( m == null ) continue;
                    m.EnableTimeEval = enabled;
                }
            } catch { }
        }
        public void EnableMacroEvalExecTime(int index0, bool enabled = true)
        {
            if ( m_bDisposed || m_listMacros == null ) return;
            try
            {
                if ( index0 < 0 || index0 >= m_listMacros.Count || m_listMacros[ index0 ] == null ) return;
                m_listMacros[ index0 ].EnableTimeEval = enabled;
            }
            catch { }
        }


        public bool ContainInteractWithUI()
        {
            if ( m_bDisposed || m_listMacros == null )
                return false;

            foreach ( var m in m_listMacros )
            {
                if ( m.AbilityToInteractWithUI )
                    return true;
            }
            return false;
        }

        public bool ContainJumpScript()
        {
            if ( m_bDisposed || m_listMacros == null )
                return false;
            foreach ( var m in m_listMacros )
            {
                if ( m.AbilityToJumpAnotherScript )
                    return true;
            }
            return false;
        }
    }
}
