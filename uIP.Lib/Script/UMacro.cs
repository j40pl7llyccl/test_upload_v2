using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Xml;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.DataCarrier;
using System.Windows.Forms;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace uIP.Lib.Script
{
    public partial class UMacro : IUMacroAdditionalMethods
    {
        protected object m_hSync = new object();
        protected bool m_bRecycled = false;
        protected UMacroMethodProviderPlugin m_PluginOwner;
        protected UScript m_ScriptOwner = null;
        // fixed in constructor
        protected string m_strClassNameOfPluginOwner;
        protected string m_strMethodNameOfPluginClass;
        protected string m_strGivenNameOfPluginOwner;
        protected fpMacroExecHandler m_fpMacroExecMethod;
        protected string m_strGivenName;
        // For shared calling information
        protected bool m_bReusableMacro;
        // able to jump to another script
        protected bool m_bAbleToJumpAnotherScript = false;
        // able to interact with UI
        protected bool m_bAbleToInteractWithUI = false;
        // able to jump to another macro
        protected bool m_bAbleToJumpIndex = false;

        // Modify in runtime
        /* Macro 內部資料參數
         * - 初始化資料
         * - 不可變動參數
         * - 可變動參數
         */
        protected UDataCarrier m_MutableInitialData; // 可以儲存此物件的一些資訊，例如有與DLL有物件相關聯則可以儲存pointer address
        protected UDataCarrier m_ScriptOwnerCarrier = null;
        protected UDataCarrier[] m_ParameterCarrierImmutable; // 初始設定後就不再改變
        protected UDataCarrier[] m_ParameterCarrierVariable; // 執行中可再次被改變
        /* 資料型態描述
         * + 回報結果可以被打包至記憶體需要遵照以下規則
         *   - 型態必須為 value type: 即基本型態或是由基本型態所組成的結構, 不可出現 reference type
         *   - 連續資料必須為 array, 且維度為 1
         */
        protected UDataCarrierTypeDescription[] m_ImmutableParameterTypeDescription;
        protected UDataCarrierTypeDescription[] m_VariableParameterTypeDescription;
        protected UDataCarrierTypeDescription[] m_PreviousPropagationParameterTypeDescription;
        protected UDataCarrierTypeDescription[] m_ReturnPropagationParameterTypeDescription;
        protected UDataCarrierTypeDescription[] m_ReturnResultTypeDescription;

        protected List<UMacro> m_refOwnerScriptInstance = null; // Reference only

        /* 執行中使用的變數
         */
        private Int32 m_nIndexOfScript = -1;
        protected bool m_bCancelExec = false;
        protected bool m_bEnabled;
        protected MacroExecReturnCode m_StatusCode;
        protected string m_strReport;
        protected string m_strException;
        protected double m_dfExecTm = 0.0;
        // control flag
        /* m_bInvisible 為 true 時
         * - PrevPropagation 將會直接傳遞至下一個
         * - ReturnPropagation 將會被忽略
         * 
         * m_bExtractingResultToPack
         * - 對結果做出額外提取的動作，會依據是要寫出至檔案或是記憶體做出額外處理
         * - 打包至記憶體: 需要遵循上述value type, array規則
         * - 檔案則限制性較小，會利用 XML 進行 DataCarrier 的輸出，當然也可以利用 memory stream 進行打包至記憶體
         */
        protected bool m_bInvisible = false;
        protected bool m_bConfigFirst = false;
        protected bool m_bConfigDone = true;
        // mark the macro result to do extra processing
        // 1. pack to xml format: UScript::PackCurrentResultsToFile()
        // 2. pack to binary buffer: UScript::PackCurrentResultsToByte()
        protected bool m_bExtractingResultToPack = false;
        private static string _strExtractingResultToPackEleName = "bExtractingResultToPack";
        // gather nearby macro to be a group
        protected bool m_bGatherNearbyMacro = false;
        private string _strGatherNearbyMacroEleName = "GatherNearbyMacro";

        #region [ Public Property ]

        public UMacroMethodProviderPlugin OwnerOfPluginClass => m_PluginOwner;

        /// <summary>
        /// get/ set owner script instance
        /// Remark:
        /// if macro is reusabe, mean it will shared between scripts and cannot be get or set.
        /// GetDicKeyStrOne will return null and not suit for checking any macro in a script.
        /// </summary>
        public UScript OwnerOfScript
        {            
            get => m_bReusableMacro ? null : m_ScriptOwner;
            set
            {
                if ( m_bReusableMacro )
                    return;

                Monitor.Enter( m_hSync );
                try
                {
                    m_ScriptOwner = value;
                    m_ScriptOwnerCarrier = new UDataCarrier( m_ScriptOwner, typeof( UScript ) );
                }
                finally { Monitor.Exit( m_hSync ); }
            }
        }

        public string OwnerClassName => m_strClassNameOfPluginOwner;

        public string MethodName => m_strMethodNameOfPluginClass;
        public string GivenShortenName { get; set; } = "";

        public fpMacroExecHandler fpHandler => m_fpMacroExecMethod;

        public Int32 IndexOfScript => m_nIndexOfScript;
        public bool IsJumpInto { get; internal set; } = false;

        public bool AbilityToJumpAnotherScript
        {
            get => m_bAbleToJumpAnotherScript;
            set => m_bAbleToJumpAnotherScript = value;
        }

        public bool AbilityToInteractWithUI
        {
            get => m_bAbleToInteractWithUI;
            set => m_bAbleToInteractWithUI = value;
        }

        public bool AbilityToJumpIndex
        {
            get => m_bAbleToJumpIndex;
            set => m_bAbleToJumpIndex = value;
        }

        public Int32 MacroIndexOfDesignTime
        {
            get
            {
                if ( m_ScriptOwner == null || m_ScriptOwner.MacroSet == null ) return -1;
                for ( int i = 0 ; i < m_ScriptOwner.MacroSet.Count ; i++ )
                {
                    if ( m_ScriptOwner.MacroSet[ i ] == this ) return i;
                }

                return -1;
            }
        }

        /// <summary>
        /// can store user data. if need to do mem handling, implement IDispose.
        /// </summary>
        public UDataCarrier MutableInitialData
        {
            get => m_MutableInitialData;
            set
            {
                Monitor.Enter( m_hSync );
                try
                {
                    // handle resource before set new one
                    m_MutableInitialData?.Dispose();
                    // set value
                    m_MutableInitialData = value;
                }
                finally { Monitor.Exit( m_hSync ); }
            }
        }
        /// <summary>
        /// macro immutable carrier, and cannot include MUST handling resource
        /// </summary>
        public UDataCarrier[] ParameterCarrierImmutable
        {
            get => m_ParameterCarrierImmutable;
            set
            {
                Monitor.Enter( m_hSync );
                try
                {
                    UDataCarrier.FreeByIDispose( m_ParameterCarrierImmutable ); // free resource before set
                    m_ParameterCarrierImmutable = value;
                }
                finally { Monitor.Exit( m_hSync ); }
            }
        }
        /// <summary>
        /// macro variable carrier, and cannot include MUST handling resource.
        /// </summary>
        public UDataCarrier[] ParameterCarrierVariable
        {
            get => m_ParameterCarrierVariable;
            set
            {
                Monitor.Enter( m_hSync );
                try
                {
                    UDataCarrier.FreeByIDispose( m_ParameterCarrierVariable ); // free resource before set
                    m_ParameterCarrierVariable = value;
                }
                finally { Monitor.Exit( m_hSync ); }
            }
        }

        public bool Enabled => m_bEnabled;

        public bool CancelExec
        {
            get => m_bCancelExec;
            internal set => m_bCancelExec = value;
        }

        public MacroExecReturnCode ExecStatusCode => m_StatusCode;

        public string ExecStatusMessage => m_strReport;

        public string ExceptionString => m_strException;

        public UDataCarrierTypeDescription[] ImmutableParamTypeDesc
        {
            get => m_ImmutableParameterTypeDescription;
            set { Monitor.Enter( m_hSync ); try { m_ImmutableParameterTypeDescription = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public UDataCarrierTypeDescription[] VariableParamTypeDesc
        {
            get => m_VariableParameterTypeDescription;
            set { Monitor.Enter( m_hSync ); try { m_VariableParameterTypeDescription = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public UDataCarrierTypeDescription[] PrevPropagationParamTypeDesc
        {
            get => m_PreviousPropagationParameterTypeDescription;
            set { Monitor.Enter( m_hSync ); try { m_PreviousPropagationParameterTypeDescription = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public UDataCarrierTypeDescription[] RetPropagationParamTypeDesc
        {
            get => m_ReturnPropagationParameterTypeDescription;
            set { Monitor.Enter( m_hSync ); try { m_ReturnPropagationParameterTypeDescription = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public UDataCarrierTypeDescription[] RetResultTypeDesc
        {
            get => m_ReturnResultTypeDescription;
            set { Monitor.Enter( m_hSync ); try { m_ReturnResultTypeDescription = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        // 儲存這個 Macro 的 method 是存在哪個繼承自 UMacroMethodProviderPlugin class 的設計時的名稱
        public string GivenNameOfOwnerPluginClass
        {
            get => m_strGivenNameOfPluginOwner;
            set { Monitor.Enter( m_hSync ); try { m_strGivenNameOfPluginOwner = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool Invisible
        {
            get => m_bInvisible;
            set { Monitor.Enter( m_hSync ); try { m_bInvisible = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool ConfigFirst
        {
            get => m_bConfigFirst;
            set { Monitor.Enter( m_hSync ); try { m_bConfigFirst = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool ConfigDone
        {
            get => m_bConfigDone;
            set { Monitor.Enter( m_hSync ); try { m_bConfigDone = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool ExtractingResultToPack
        {
            get => m_bExtractingResultToPack;
            set { Monitor.Enter( m_hSync ); try { m_bExtractingResultToPack = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool GatherNearbyMacro
        {
            get => m_bGatherNearbyMacro;
            set { Monitor.Enter( m_hSync ); try { m_bGatherNearbyMacro = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public List<UMacro> OwnerMacrosList
        {
            get => m_refOwnerScriptInstance;
            set { Monitor.Enter( m_hSync ); try { m_refOwnerScriptInstance = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public string GivenName
        {
            get => m_strGivenName;
            set { Monitor.Enter( m_hSync ); try { m_strGivenName = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool ReusableMacro
        {
            get => m_bReusableMacro;
            set { Monitor.Enter( m_hSync ); try { m_bReusableMacro = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public Double ExecTime
        {
            get => m_dfExecTm;
            set => m_dfExecTm = value;
        }

        public bool EnableTimeEval { get; set; } = false;
        public double MinExecTime { get; private set; } = double.MaxValue;
        public double MaxExecTime { get; private set; } = double.MinValue;

        private double ExecTimeTotal { get; set; } = 0;
        private UInt64 ExecTimeCount { get; set; } = 0;
        public double AvgExecTime 
        {
            get
            {
                try
                {
                    return ExecTimeCount == 0 ? 0 : ExecTimeTotal / Convert.ToDouble( ExecTimeCount );
                }
                catch { return 0; }
            }
        }

        #endregion

        public UMacro( UMacroMethodProviderPlugin owner, string nameOfOwnerSharpClass, string methodName,
                       fpMacroExecHandler methodHandler,
                       UDataCarrierTypeDescription[] typeDescOfImmuParam, UDataCarrierTypeDescription[] typeDescOfVarParam,
                       UDataCarrierTypeDescription[] typeDescOfPrevPropag, UDataCarrierTypeDescription[] typeDescOfRtnPropag )
        {
            m_PluginOwner = owner;
            m_strClassNameOfPluginOwner = String.IsNullOrEmpty( nameOfOwnerSharpClass ) ? null : String.Copy( nameOfOwnerSharpClass );
            m_strMethodNameOfPluginClass = String.IsNullOrEmpty( methodName ) ? null : String.Copy( methodName );
            m_fpMacroExecMethod = methodHandler;
            m_strGivenNameOfPluginOwner = null;

            m_MutableInitialData = null;
            m_ParameterCarrierImmutable = null;
            m_ParameterCarrierVariable = null;
            m_bEnabled = true;
            m_StatusCode = MacroExecReturnCode.NA;
            m_strReport = null;
            m_strException = null;
            m_ImmutableParameterTypeDescription = typeDescOfImmuParam;
            m_VariableParameterTypeDescription = typeDescOfVarParam;
            m_PreviousPropagationParameterTypeDescription = typeDescOfPrevPropag;
            m_ReturnPropagationParameterTypeDescription = typeDescOfRtnPropag;
            m_ReturnResultTypeDescription = null;

            m_strGivenName = null;
            m_bReusableMacro = false;
        }

        public UMacro( UMacroMethodProviderPlugin owner, string nameOfOwnerSharpClass, string methodName,
                       fpMacroExecHandler methodHandler,
                       UDataCarrierTypeDescription[] typeDescOfImmuParam, UDataCarrierTypeDescription[] typeDescOfVarParam,
                       UDataCarrierTypeDescription[] typeDescOfPrevPropag, UDataCarrierTypeDescription[] typeDescOfRtnPropag, UDataCarrierTypeDescription[] typeDescOfRtnResult )
        {
            m_PluginOwner = owner;
            m_strClassNameOfPluginOwner = String.IsNullOrEmpty( nameOfOwnerSharpClass ) ? null : String.Copy( nameOfOwnerSharpClass );
            m_strMethodNameOfPluginClass = String.IsNullOrEmpty( methodName ) ? null : String.Copy( methodName );
            m_fpMacroExecMethod = methodHandler;
            m_strGivenNameOfPluginOwner = null;

            m_MutableInitialData = null;
            m_ParameterCarrierImmutable = null;
            m_ParameterCarrierVariable = null;
            m_bEnabled = true;
            m_StatusCode = MacroExecReturnCode.NA;
            m_strReport = null;
            m_strException = null;
            m_ImmutableParameterTypeDescription = typeDescOfImmuParam;
            m_VariableParameterTypeDescription = typeDescOfVarParam;
            m_PreviousPropagationParameterTypeDescription = typeDescOfPrevPropag;
            m_ReturnPropagationParameterTypeDescription = typeDescOfRtnPropag;
            m_ReturnResultTypeDescription = typeDescOfRtnResult;

            m_strGivenName = null;
            m_bReusableMacro = false;
        }

        ~UMacro()
        {
            Recycle(false);
        }

        public virtual void Recycle(bool disposing = true)
        {
            if ( m_bRecycled ) return;

            Monitor.Enter( m_hSync );
            try
            {
                if ( m_pTriggerBegExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pTriggerBegExecEvent );
                m_pTriggerBegExecEvent = IntPtr.Zero;

                if ( m_pTriggerEndExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pTriggerEndExecEvent );
                m_pTriggerEndExecEvent = IntPtr.Zero;

                if ( m_pWaitBegExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pWaitBegExecEvent );
                m_pWaitBegExecEvent = IntPtr.Zero;

                if ( m_pWaitOnEndExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pWaitOnEndExecEvent );
                m_pWaitOnEndExecEvent = IntPtr.Zero;

                //m_PluginOwner?.RecycleMacro( this );
                if ( m_PluginOwner != null ) m_PluginOwner.RecycleMacro( this );

                // reset variables
                m_ScriptOwner = null;
                m_fpMacroExecMethod = null;
                m_ParameterCarrierImmutable = null;
                m_ParameterCarrierVariable = null;
                m_refOwnerScriptInstance = null;

                m_bRecycled = true;
            }
            finally
            {
                Monitor.Exit( m_hSync );
            }

            if (disposing)
                GC.SuppressFinalize( this );
        }

        public void DetachFromOwner()
        {
            Monitor.Enter( m_hSync );
            try { m_PluginOwner = null; }
            finally { Monitor.Exit( m_hSync ); }
        }

        public Assembly GetAssemblyOwner()
        {
            return ((m_bRecycled || m_PluginOwner == null) ? null : m_PluginOwner.OwnerAssembly);
        }

        public void IsAbleToCall( bool bEnabled )
        {
            Monitor.Enter( m_hSync );
            try { m_bEnabled = bEnabled; }
            finally { Monitor.Exit( m_hSync ); }
        }

        public void ResetExecTimeData(bool enabled = true)
        {
            Monitor.Enter( m_hSync );

            EnableTimeEval = enabled;
            MinExecTime = double.MaxValue;
            MaxExecTime = double.MinValue;
            ExecTimeCount = 0;
            ExecTimeTotal = 0;

            Monitor.Exit( m_hSync );
        }

        public void ResetAvgExecTimeData()
        {
            Monitor.Enter( m_hSync );

            ExecTimeCount = 0;
            ExecTimeTotal = 0;

            Monitor.Exit( m_hSync );
        }

        public UDataCarrier[] Exec( Int32 nCurrIndexOfScript,
                                    UDataCarrier[] prevPropagation,
                                    List<UMacroProduceCarrierResult> historyResult,
                                    List<UMacroProduceCarrierPropagation> historyPropagations,
                                    List<UMacroProduceCarrierDrawingResult> historyDrawingResults,
                                    List<UScriptHistoryCarrier> otherScriptsHistoryCarriers,
                                ref MacroExecReturnCode retStatusCode,
                                ref UDataCarrier[] retPropagation,
                                ref UDrawingCarriers retDrawingResult,
                                ref fpUDataCarrierSetResHandler retHandlerOfRetPropagation,
                                ref fpUDataCarrierSetResHandler retHandlerOfResultCarrier )
        {
            m_nIndexOfScript = nCurrIndexOfScript;
            m_dfExecTm = 0.0;

            long begTm = Convert.ToInt64( 0 );
            long endTm = Convert.ToInt64( 0 );

            CommonWinSdkFunctions.QueryPerformanceCounter( ref begTm );

            m_strReport = null;
            m_strException = null;
            //
            // check control flags
            //
            // available ?
            if ( !m_bEnabled ) { retStatusCode = m_StatusCode = MacroExecReturnCode.MethodDisabled; retPropagation = prevPropagation; return null; }
            // maco method null ?
            if ( m_fpMacroExecMethod == null ) { retStatusCode = m_StatusCode = MacroExecReturnCode.InvalidHandler; return null; }
            // recycled ?
            if ( m_bRecycled ) { retStatusCode = m_StatusCode = MacroExecReturnCode.MethodRecycled; return null; }
            // config done ?
            if ( m_bConfigFirst && !m_bConfigDone ) { retStatusCode = m_StatusCode = MacroExecReturnCode.NotConfigYet; return null; }
            // cancel exec ?
            if ( m_bCancelExec )
            {
                retStatusCode = m_StatusCode = MacroExecReturnCode.CancelExec;
                return null;
            }

            UDataCarrier[] ret = null; // result carrier
            bool bCancel = false;

            // --- critical section enter ---
            Monitor.Enter( m_hSync );
            try
            {
                bool bCanExec = true;

                //
                // event sync before exec
                //
                if ( m_pTriggerBegExecEvent != IntPtr.Zero ) EventWinSdkFunctions.Set( m_pTriggerBegExecEvent ); // notify, trigger out
                if ( m_pWaitBegExecEvent != IntPtr.Zero ) // wait event
                {
                    DateTime synPrev = DateTime.Now;
                    bool bSucc = false;

                    for (; ; )
                    {
                        UInt32 wstat = WaitWinSdkFunctions.WaitForSingleObject( m_pWaitBegExecEvent, 0 );
                        if ( wstat == ( UInt32 )WAIT_STATUS.OBJECT_0 )
                        {
                            bSucc = true; break;
                        }
                        TimeSpan diff = DateTime.Now - synPrev;
                        if ( diff.TotalMilliseconds > ( double )m_nWaitBegExecTimeout )
                            break;
                        if ( m_bCancelExec || !ResourceManager.SystemAvaliable )
                        {
                            bCancel = true; break;
                        }
                        System.Windows.Forms.Application.DoEvents(); // process windows event
                                                                     //Thread.Sleep( 1 );
                    }

                    if ( !bSucc )
                    {
                        if ( bCancel )
                        {
                            m_StatusCode = MacroExecReturnCode.CancelExec;
                            m_strReport = "Cancel exec";
                            bCanExec = false;
                        }
                        else if ( !m_bWaitBegExecErrorContinue )
                        {
                            m_StatusCode = MacroExecReturnCode.NG;
                            m_strReport = "Wait begin event timeout";
                            bCanExec = false;
                        }
                        else
                        {
                            if ( m_PluginOwner != null && m_PluginOwner.fpLog != null )
                                m_PluginOwner.fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}-{1}] wait timeout and cont.", m_ScriptOwner == null ? "" : m_ScriptOwner.NameOfId, m_nIndexOfScript ) );
                        }
                    }
                }

                //
                // Exec the macro method
                //
                if ( bCanExec )
                {
                    bool bStat = false;
                    ret = m_fpMacroExecMethod( this, prevPropagation, historyResult, historyPropagations, historyDrawingResults, otherScriptsHistoryCarriers,
                                     ref bStat, ref m_strReport, ref retPropagation, ref retDrawingResult, ref retHandlerOfRetPropagation, ref retHandlerOfResultCarrier );
                    if ( bStat )
                        m_StatusCode = MacroExecReturnCode.OK;
                    else
                    {
                        m_StatusCode = MacroExecReturnCode.NG;
                        ret = null;
                    }

                    retStatusCode = m_StatusCode;
                }

            }
            catch ( Exception exp )
            {
                m_strException = exp.ToString();
                m_StatusCode = MacroExecReturnCode.NG;
                m_strReport = "Exception";
                retStatusCode = m_StatusCode;
            }
            finally
            {
                //
                // event sync after exec
                //
                if ( m_pTriggerEndExecEvent != IntPtr.Zero ) EventWinSdkFunctions.Set( m_pTriggerEndExecEvent ); // notify, trigger out
                if ( m_pWaitOnEndExecEvent != IntPtr.Zero ) // wait event
                {
                    DateTime synPrev = DateTime.Now;
                    for ( ;;)
                    {
                        if ( WaitWinSdkFunctions.WaitForSingleObject( m_pWaitOnEndExecEvent, 0 ) == ( UInt32 ) WAIT_STATUS.OBJECT_0 )
                            break;
                        TimeSpan diff = DateTime.Now - synPrev;
                        if ( diff.TotalMilliseconds >= ( double ) m_nWaitOnEndExecTimeout )
                            break;

                        if ( bCancel || m_bCancelExec || !ResourceManager.SystemAvaliable ) // check the cancel flag
                            break;
                        System.Windows.Forms.Application.DoEvents(); // process windows event
                    }
                }

                CommonWinSdkFunctions.QueryPerformanceCounter( ref endTm );
                m_dfExecTm = Convert.ToDouble( endTm - begTm ) / Convert.ToDouble( CommonWinSdkFunctions.CurrFrequence ) * 1000.0;

                if ( EnableTimeEval && m_StatusCode == MacroExecReturnCode.OK )
                {
                    MinExecTime = m_dfExecTm < MinExecTime ? m_dfExecTm : MinExecTime;
                    MaxExecTime = m_dfExecTm > MaxExecTime ? m_dfExecTm : MaxExecTime;

                    ExecTimeTotal = ExecTimeTotal + m_dfExecTm;
                    ++ExecTimeCount;
                }

                // --- critical section leave ---
                Monitor.Exit( m_hSync );
            }

            return ret;
        }

        public bool CancelState()
        {
            return m_bCancelExec;
        }

        public virtual void ReproduceDoneCall( UMacro source )
        {
            // any other things must be done? Update from here~~~

            if ( source != null )
                m_bGatherNearbyMacro = source.GatherNearbyMacro;
        }

        public virtual void WriteAdditionalParameters( XmlTextWriter wr )
        {
            if ( wr == null ) return;

            // any other parameters must be saved? Write here~~~

            wr.WriteElementString( _strExtractingResultToPackEleName, m_bExtractingResultToPack.ToString() );
            wr.WriteElementString( _strGatherNearbyMacroEleName, m_bGatherNearbyMacro.ToString() );
        }

        public virtual void ReadAdditionalParameters( XmlNode rd )
        {
            if ( rd == null ) return;

            // any other parameters must be read? Write here~~~

            XmlNode nod = null;

            nod = rd.SelectSingleNode( _strExtractingResultToPackEleName );
            if ( nod != null )
            {
                try { m_bExtractingResultToPack = Convert.ToBoolean( nod.InnerText ); }
                catch { m_bExtractingResultToPack = false; }
            }

            nod = rd.SelectSingleNode( _strGatherNearbyMacroEleName );
            if ( nod != null )
            {
                try { m_bGatherNearbyMacro = Convert.ToBoolean( nod.InnerText ); }
                catch { m_bGatherNearbyMacro = false; }
            }
        }

        private static bool SearchPropagations( List<UMacroProduceCarrierPropagation> propags, bool findOne, int currIndex, object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out Dictionary<int, List<UDataCarrier>> found, int way = 0 )
        {
            found = new Dictionary<int, List<UDataCarrier>>();
            if ( propags == null )
                return false;

            foreach ( var propagation in propags )
            {
                if ( propagation == null || propagation.PropagationCarrier == null )
                    continue;

                bool doFinding = false;
                if ( way < 0 && propagation.IndexOfScript < currIndex )
                    doFinding = true;
                else if ( way > 0 && propagation.IndexOfScript > currIndex )
                    doFinding = true;
                else if ( propagation.IndexOfScript != currIndex )
                    doFinding = true;

                if ( doFinding )
                {
                    foreach ( var c in propagation.PropagationCarrier )
                    {
                        try
                        {
                            var got = cmp( ctx, c );
                            if ( got != null )
                            {
                                if ( !found.ContainsKey( propagation.IndexOfScript ) )
                                    found.Add( propagation.IndexOfScript, new List<UDataCarrier>() );
                                var toAdd = found[ propagation.IndexOfScript ];
                                toAdd.Add( c );
                                if ( findOne )
                                    return true;
                                return true;
                            }
                        }
                        catch { }
                    }
                }
            }
            return false;
        }

        private static bool SearchResults( List<UMacroProduceCarrierResult> results, bool findOne, int currIndex, object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out Dictionary<int, List<UDataCarrier>> found, int way = 0 )
        {
            found = new Dictionary<int, List<UDataCarrier>>();
            if ( results == null )
                return false;

            foreach ( var result in results )
            {
                if ( result == null || result.ResultSet == null )
                    continue;

                bool doFinding = false;
                if ( way < 0 && result.IndexOfScript < currIndex )
                    doFinding = true;
                else if ( way > 0 && result.IndexOfScript > currIndex )
                    doFinding = true;
                else if ( result.IndexOfScript != currIndex )
                    doFinding = true;

                if ( doFinding )
                {
                    foreach ( var c in result.ResultSet )
                    {
                        try
                        {
                            var got = cmp( ctx, c );
                            if ( got != null )
                            {
                                if ( !found.ContainsKey( result.IndexOfScript ) )
                                    found.Add( result.IndexOfScript, new List<UDataCarrier>() );
                                var toAdd = found[ result.IndexOfScript ];
                                toAdd.Add( c );
                                if ( findOne )
                                    return true;
                                return true;
                            }
                        }
                        catch { }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// query one owner script propagations data by a deletege cmp
        /// </summary>
        /// <param name="ctx">context for cmp</param>
        /// <param name="cmp">deletgate function</param>
        /// <param name="locateIndex">found in which index of script</param>
        /// <param name="found">found data</param>
        /// <param name="way">-1: find less than index; 0: find all except self; 1: find more than index</param>
        /// <returns>call status</returns>
        public bool QueryScriptPropagationOne( object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out int locateIndex, out UDataCarrier found, int way = 0)
        {
            locateIndex = -1;
            found = null;
            if ( cmp == null || OwnerOfScript == null || OwnerOfScript.PropagationCarriers == null || OwnerOfScript.PropagationCarriers.Count <= 0 )
                return false;

            if ( !SearchPropagations( OwnerOfScript.PropagationCarriers, true, m_nIndexOfScript, ctx, cmp, out var tmp, way ) || tmp.Count <= 0 )
                return false;

            locateIndex = tmp.First().Key;
            found = tmp.First().Value[ 0 ];

            return true;
        }
        /// <summary>
        /// query many owner script propagations data by a delegate cmp
        /// </summary>
        /// <param name="ctx">context for cmp</param>
        /// <param name="cmp">deletgate function</param>
        /// <param name="found">found data</param>
        /// <param name="way">-1: find less than index; 0: find all except self; 1: find more than index</param>
        /// <returns>call status</returns>
        public bool QueryScriptPropagationMany( object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out Dictionary<int, List<UDataCarrier>> found, int way = 0 )
        {
            found = null;
            if ( cmp == null || OwnerOfScript == null || OwnerOfScript.PropagationCarriers == null || OwnerOfScript.PropagationCarriers.Count <= 0 )
                return false;
            return SearchPropagations( OwnerOfScript.PropagationCarriers, true, m_nIndexOfScript, ctx, cmp, out found, way );
        }

        /// <summary>
        /// query one owner script results data by a deletege cmp
        /// </summary>
        /// <param name="ctx">context for cmp</param>
        /// <param name="cmp">deletgate function</param>
        /// <param name="locateIndex">found in which index of script</param>
        /// <param name="found">found data</param>
        /// <param name="way">-1: find less than index; 0: find all except self; 1: find more than index</param>
        /// <returns>call status</returns>
        public bool QueryScriptResultOne( object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out int locateIndex, out UDataCarrier found, int way = 0 )
        {
            locateIndex = -1;
            found = null;
            if ( cmp == null || OwnerOfScript == null || OwnerOfScript.ResultCarriers == null || OwnerOfScript.ResultCarriers.Count <= 0 )
                return false;

            if ( !SearchResults( OwnerOfScript.ResultCarriers, true, m_nIndexOfScript, ctx, cmp, out var tmp, way ) || tmp.Count <= 0 )
                return false;

            locateIndex = tmp.First().Key;
            found = tmp.First().Value[ 0 ];

            return true;
        }
        /// <summary>
        /// query many owner script results data by a delegate cmp
        /// </summary>
        /// <param name="ctx">context for cmp</param>
        /// <param name="cmp">deletgate function</param>
        /// <param name="found">found data</param>
        /// <param name="way">-1: find less than index; 0: find all except self; 1: find more than index</param>
        /// <returns>call status</returns>
        public bool QueryScriptResultMany( object ctx, Func<object, UDataCarrier, UDataCarrier> cmp, out Dictionary<int, List<UDataCarrier>> found, int way = 0 )
        {
            found = null;
            if ( cmp == null || OwnerOfScript == null || OwnerOfScript.ResultCarriers == null || OwnerOfScript.ResultCarriers.Count <= 0 )
                return false;
            return SearchResults( OwnerOfScript.ResultCarriers, true, m_nIndexOfScript, ctx, cmp, out found, way );
        }

        /// <summary>
        /// Query owner script with method name
        /// </summary>
        /// <param name="name">method name of macro</param>
        /// <param name="found">found data: key is index, value is the macro</param>
        /// <returns>call status</returns>
        public bool QueryScriptMacroMethod(string name, out Dictionary<int, UMacro> found)
        {
            found = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || OwnerOfScript.MacroSet.Count <= 0 )
                return false;

            found = new Dictionary<int, UMacro>();
            name = name.ToLower();
            for(int i = 0; i < OwnerOfScript.MacroSet.Count; i++ )
            {
                var m = OwnerOfScript.MacroSet[i];
                if (m.MethodName.ToLower() == name)
                {
                    found.Add( i, m );
                }
            }
            return true;
        }

        /// <summary>
        /// query macro support control
        /// </summary>
        /// <param name="opName">control name</param>
        /// <param name="found">return data</param>
        /// <param name="canGet">can get op</param>
        /// <param name="canSet">can set op</param>
        /// <returns>call status</returns>
        public bool QueryScriptMacroSupportControl( string opName, out Dictionary<int, UMacro> found, bool canGet = true, bool canSet = true )
        {
            found = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || OwnerOfScript.MacroSet.Count <= 0 )
                return false;

            found = new Dictionary<int, UMacro>();
            for ( int i = 0; i < OwnerOfScript.MacroSet.Count; i++ )
            {
                var m = OwnerOfScript.MacroSet[ i ];
                if ( m == this )
                    continue;
                if ( m.OwnerOfPluginClass?.MacroControlList?.TryGetValue( opName, out var op ) ?? false )
                {
                    if ( ( op.CanGet && canGet ) || ( op.CanSet && canSet ) )
                        found.Add( i, m );
                }
            }

            return true;
        }

        /// <summary>
        /// query macro owner plugin class support control
        /// </summary>
        /// <param name="opName">control name</param>
        /// <param name="found">return data</param>
        /// <param name="canGet">can get op</param>
        /// <param name="canSet">can set op</param>
        /// <returns>call status</returns>
        public bool QueryScriptMacroOwnerPluginSupportControl(string opName, out Dictionary<int, UMacro> found, bool canGet = true, bool canSet = true)
        {
            found = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || OwnerOfScript.MacroSet.Count <= 0 )
                return false;

            found = new Dictionary<int, UMacro>();
            for ( int i = 0; i < OwnerOfScript.MacroSet.Count; i++ )
            {
                var m = OwnerOfScript.MacroSet[ i ];
                if ( m == this )
                    continue;
                if ( m.OwnerOfPluginClass?.PluginClassControlList?.TryGetValue( opName, out var op ) ?? false )
                {
                    if ( ( op.CanGet && canGet ) || ( op.CanSet && canSet ) )
                        found.Add( i, m );
                }
            }

            return true;
        }

        /// <summary>
        /// query macro owner plugin class support function
        /// </summary>
        /// <param name="funcName">name of function</param>
        /// <param name="found">return data</param>
        /// <returns>call status</returns>
        public bool QueryScriptMacroOwnerPluginSupportOpenedFunc(string funcName, out Dictionary<int, UMacro> found)
        {
            found = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || OwnerOfScript.MacroSet.Count <= 0 )
                return false;

            found = new Dictionary<int, UMacro>();
            for ( int i = 0; i < OwnerOfScript.MacroSet.Count; i++ )
            {
                var m = OwnerOfScript.MacroSet[ i ];
                if ( m == this )
                    continue;
                if ( m.OwnerOfPluginClass?.PluginClassFunctions?.TryGetValue( funcName, out var op ) ?? false )
                {
                    found.Add( i, m );
                }
            }

            return true;

        }

        /// <summary>
        /// Set macro control inside owner script
        /// </summary>
        /// <param name="index">macro index of script to operate</param>
        /// <param name="opName">name of the control</param>
        /// <param name="data">required data to set</param>
        /// <returns>call status</returns>
        public bool SetScriptMacroControl(int index, string opName, params UDataCarrier[] data)
        {
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || index < 0 || index >= OwnerOfScript.MacroSet.Count )
                return false;

            var m = OwnerOfScript.MacroSet[ index ];
            return m?.OwnerOfPluginClass?.SetMacroControl( m, UDataCarrier.MakeOne( opName ), data ) ?? false;
        }

        /// <summary>
        /// GetDicKeyStrOne macro control inside owner script
        /// </summary>
        /// <param name="index">macro index of script to operate</param>
        /// <param name="opName">name of the control</param>
        /// <param name="data">return data</param>
        /// <returns>call status</returns>
        public bool GetScriptMacroControl(int index, string opName, out UDataCarrier[] data)
        {
            data = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || index < 0 || index >= OwnerOfScript.MacroSet.Count )
                return false;

            var m = OwnerOfScript.MacroSet[ index ];
            var status = false;
            data = m?.OwnerOfPluginClass?.GetMacroControl( m, UDataCarrier.MakeOne( opName ), ref status ) ?? null;
            return status;
        }

        /// <summary>
        /// Set macro owner plugin class control inside owner script
        /// </summary>
        /// <param name="index">macro index of script to operate</param>
        /// <param name="opName">name of the control</param>
        /// <param name="data">data to set</param>
        /// <returns>call status</returns>
        public bool SetScriptMacroOwnerClassControl(int index, string opName, params UDataCarrier[] data)
        {
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || index < 0 || index >= OwnerOfScript.MacroSet.Count )
                return false;

            var m = OwnerOfScript.MacroSet[ index ];
            return m?.OwnerOfPluginClass?.SetClassControl( UDataCarrier.MakeOne( opName ), data ) ?? false;
        }
        /// <summary>
        /// GetDicKeyStrOne macro owner plugin class control inside owner script
        /// </summary>
        /// <param name="index">macro index of script to operate</param>
        /// <param name="opName">name of control</param>
        /// <param name="data">return data</param>
        /// <returns>call status</returns>
        public bool GetScriptMacroOwnerClassControl(int index, string opName, out UDataCarrier[] data)
        {
            data = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || index < 0 || index >= OwnerOfScript.MacroSet.Count )
                return false;

            var m = OwnerOfScript.MacroSet[ index ];
            var status = false;
            data = m?.OwnerOfPluginClass?.GetClassControl( UDataCarrier.MakeOne( opName ), ref status ) ?? null;
            return status;
        }
        /// <summary>
        /// Call macro owner plugin class opened function inside owner script
        /// </summary>
        /// <param name="index">macro index of script to operate</param>
        /// <param name="funcName">function to operate</param>
        /// <param name="result">return data</param>
        /// <param name="args">input argument(s)</param>
        /// <returns>call status</returns>
        public bool CallScriptMacroOwnerClassOpenedFunc(int index, string funcName, out UDataCarrier result, params UDataCarrier[] args)
        {
            result = null;
            if ( OwnerOfScript == null || OwnerOfScript.MacroSet == null || index < 0 || index >= OwnerOfScript.MacroSet.Count )
                return false;

            var m = OwnerOfScript.MacroSet[ index ];
            return m?.OwnerOfPluginClass?.CallClassProvideFunc( funcName, out result, args ) ?? false;
        }

        #region static utility methods

        public static UMacro Reproduce( UMacro src )
        {
            if ( src == null || !src.ConfigDone || src.OwnerOfPluginClass == null )
                return null;

            UMacro ret = src.OwnerOfPluginClass.ReproduceMacro( src );

            if ( ret == null )
                return null;

            IUMacroAdditionalMethods conv = ret as IUMacroAdditionalMethods;
            //conv?.ReproduceDoneCall( src );
            if ( conv != null )
                conv.ReproduceDoneCall( src );
            ret.ConfigDone = true;

            return ret;
        }

        public static UMacro[] GetMacro( List<UMacro> list, string name )
        {
            if ( list == null || list.Count <= 0 || String.IsNullOrEmpty( name ) ) return null;
            List<UMacro> tmp = new List<UMacro>();
            for ( int i = 0 ; i < list.Count ; i++ )
            {
                if ( list[ i ] == null || String.IsNullOrEmpty( list[ i ].MethodName ) ) continue;
                if ( list[ i ].MethodName == name )
                    tmp.Add( list[ i ] );
            }

            return tmp.Count <= 0 ? null : tmp.ToArray();
        }

        public static UMacro[] GetMacro( UMacro[] arr, string name )
        {
            if ( arr == null || arr.Length <= 0 || String.IsNullOrEmpty( name ) ) return null;
            List<UMacro> tmp = new List<UMacro>();
            for ( int i = 0 ; i < arr.Length ; i++ )
            {
                if ( arr[ i ] == null || String.IsNullOrEmpty( arr[ i ].MethodName ) ) continue;
                if ( arr[ i ].MethodName == name )
                    tmp.Add( arr[ i ] );
            }

            return tmp.Count <= 0 ? null : tmp.ToArray();
        }

        #endregion
    }
}
