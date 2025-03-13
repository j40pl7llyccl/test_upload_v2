using System;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using uIP.LibBase.DataCarrier;

namespace uIP.LibBase.Macro
{
    public enum MacroExecReturnCode : int
    {
        NA = -1,
        OK = 0,
        NG,
        MethodDisabled,
        MethodRecycled,
        InvalidHandler,
        NotConfigYet,
        MacroBreak,
        CancelExec
    }

    public enum ScriptExecReturnCode : int
    {
        OK = 0,
        NA,
        MacroExecErr,
        SyncTimeout,
        Disposed,
        Exception,
        InConfiguration,
    }

    public enum ScriptAdditionFunc : int
    {
        Normal = 0,
        Goto,
    }

    public delegate void fpScriptLoopingEndCallback( UScript whichScript, ScriptExecReturnCode code );

    /// <summary>
    /// As an interface of script's macro call. People who want to implement a 
    /// function into a script call must follow this define.
    /// </summary>
    /// <param name="MacroInstance">input macro instance for later used</param>
    /// <param name="PrevPropagationCarrier">previous macro output propagation carrier info</param>
    /// <param name="historyResultCarriers">all results before current macro</param>
    /// <param name="historyPropagationCarriers">all propagation carriers before current macro</param>
    /// <param name="historyDrawingCarriers">all draw result carriers before current macro</param>
    /// <param name="historyCarrier">including propagation carriers, result carriers and drawing carriers</param>
    /// <param name="bStatusCode">exec current macro method status</param>
    /// <param name="strStatusMessage">string to report in error or else</param>
    /// <param name="CurrPropagationCarrier">report current propagation carrier</param>
    /// <param name="CurrDrawingCarriers">report current drawing carriers</param>
    /// <param name="PropagationCarrierHandler">if need handle the propagation carrier, give a delegate funtion</param>
    /// <param name="ResultCarrierHandler">if need handle result carrier, give a delegate function</param>
    /// <returns>report result carrier</returns>
    public delegate UDataCarrier[] fpMacroExecHandler( UMacro MacroInstance,
                                                       UDataCarrier[] PrevPropagationCarrier,
                                                       List<UMacroProduceCarrierResult> historyResultCarriers,
                                                       List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
                                                       List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
                                                       List<UScriptHistoryCarrier> historyCarrier,
                                                   ref bool bStatusCode, ref string strStatusMessage,
                                                   ref UDataCarrier[] CurrPropagationCarrier,
                                                   ref UDrawingCarriers CurrDrawingCarriers,
                                                   ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
                                                   ref fpUDataCarrierSetResHandler ResultCarrierHandler );

    /// <summary>
    /// 用來對已經產生的 Macro 進行額外的設定(會用在剛產生的時候)
    /// </summary>
    /// <param name="callMethodName">plugin 中對應的 method 名稱</param>
    /// <param name="instance">要進行設定的 macro instance</param>
    /// <returns>設定狀態: true->ok, false->fail</returns>
    public delegate bool fpMacroShellDoneCall( string callMethodName, UMacro instance );

    /// <summary>
    /// 用來產生 macro 前, 針對 plugin 中所提供的 method, 要產生 immutable param 的呼叫, 通常會是 popup dialog
    /// </summary>
    /// <param name="callMethodName">plugin 中對應的 method 名稱</param>
    /// <returns>immutable parameters</returns>
    public delegate UDataCarrier[] fpGenMacroImmutableParam( string callMethodName );

    /// <summary>
    /// 用來產生 macro 前, 針對 plugin 中所提供的 method, 要產生 variable param 的呼叫, 通常會是 popup dialog
    /// </summary>
    /// <param name="callMethodName">plugin 中對應的 method 名稱</param>
    /// <param name="scriptMacros">已經產生的 macro</param>
    /// <param name="repoVariableParam">回傳 variable parameter</param>
    /// <returns>call status: true->ok, false->fail</returns>
    public delegate bool fpGenMacroVariableParam( string callMethodName, List< UMacro > scriptMacros,
        out UDataCarrier[] repoVariableParam );

    /// <summary>
    /// 用來設定 macro 中 immutable parameters
    /// </summary>
    /// <param name="macroToSet">要設定的 macro instance</param>
    public delegate void fpConfigMacroImmuParamSet( UMacro macroToSet );

    /// <summary>
    /// 用來設定 macro 中 variable parameters
    /// </summary>
    /// <param name="macroToSet">要設定的 macro instance</param>
    public delegate void fpConfigMacroVarParamSet( UMacro macroToSet );

    /// <summary>
    /// 用來跳出 macro 中對應的 method 設定畫面
    /// </summary>
    /// <param name="callMethodName">plugin 中對應的 method 名稱</param>
    /// <param name="macroToConf">要設定的 macro instance</param>
    /// <returns>回傳一個設定視窗, 呼叫端會以 ShowDialog() 彈出, 並將其 dispose, 產生端可以使用 IGuiAclManagement 進行多語及使用者權限產生新的 form </returns>
    public delegate Form fpPopupMacroConfigDialog( string callMethodName, UMacro macroToConf );

    /// <summary>
    /// 用來取 macro 的設定 Control (GUI)
    /// </summary>
    /// <param name="callMethodName">plugin 中對應的 method 名稱</param>
    /// <param name="macro">要取得設定的 macro instance</param>
    /// <returns>回傳設定資料</returns>
    public delegate Control fpGetMacroConfig( string callMethodName, UMacro macro );

    /// <summary>
    /// 用來取得 macro method 的功能設定
    /// </summary>
    /// <param name="m">在 script 中的 macro</param>
    /// <param name="settings">回傳的設定資料</param>
    /// <returns>call status: true->ok, false->fail</returns>
    public delegate bool fpGetMacroMethodSettings( UMacro m, out object settings, out Type t );

    /// <summary>
    /// 用來寫入 macro method 的功能設定
    /// </summary>
    /// <param name="m">在 script 中的 macro</param>
    /// <param name="settings">要寫入的設定資料</param>
    /// <returns>call status: true->ok, false->fail</returns>
    public delegate bool fpSetMacroMethodSettings( UMacro m, object settings );

    public enum MacroGotoFunctions : int
    {
        GOTO_END = 100,
        GOTO_INVALID,
    }

    public enum MacroJumpingFunctions : int
    {
        JUMPING_NA = 0,
        JUMPING_TO_BEG_AGAIN,
        JUMPING_TO_ANOTHER_SCRIPT,
        JUMPING_TO_ANOTHER_SCRIPT_KEEPING_HISTORY_CARRIERS,
    }

    public class AnotherMacroJumpingInfo
    {
        public string _strNameOfScript;
        public Int32 _nBeginIndexOfTrgMacro;

        public UDataCarrier[] _FeedingPrevPropagationCarrierSet;
        public fpUDataCarrierSetResHandler _PrevPropagationCarrierSetHandler;

        public AnotherMacroJumpingInfo() { }
    }

    public delegate void HandleCancelScriptCallback( Object context );
    public class CancelExecScript
    {
        private Object m_Sync = new object();
        private bool m_bCancel = false;
        private Object m_Context = null;
        private HandleCancelScriptCallback m_Fp = null;

        private UScript m_RunningScript = null;

        public bool Flag
        {
            get => m_bCancel;
            set
            {
                Monitor.Enter( m_Sync );
                try
                {
                    // set to cancel
                    if ( !m_bCancel && value )
                    {
                        m_bCancel = true;
                        if ( m_RunningScript != null )
                        {
                            foreach ( var m in m_RunningScript.MacroSet )
                            {
                                m.CancelExec = true;
                            }
                        }
                    }
                } finally{ Monitor.Exit( m_Sync ); }
            }
        }

        internal UScript RunningScript
        {
            set
            {
                if ( value == null ) return;
                Monitor.Enter( m_Sync );
                try
                {
                    m_RunningScript = value;
                    if ( m_bCancel )
                    {
                        foreach ( var m in m_RunningScript.MacroSet )
                        {
                            m.CancelExec = true;
                        }
                    }
                } finally{ Monitor.Exit( m_Sync ); }
            }
        }

        public CancelExecScript( Object contextForCallback, HandleCancelScriptCallback fp )
        {
            m_Context = contextForCallback;
            m_Fp = fp;
        }

        internal void CancelCall()
        {
            Monitor.Enter( m_Sync );
            try
            {
                if ( m_bCancel )
                    m_Fp?.Invoke( m_Context );
            } finally{ Monitor.Exit( m_Sync ); }
        }
    }
}
