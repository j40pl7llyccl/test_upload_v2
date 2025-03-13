using System;
using System.Threading;
using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.Macro
{
    public partial class UMacro
    {
        /* 讓執行 macro 與外部事件進行同步
         */
        // sync event -- begin
        protected bool m_bWaitBegExecErrorContinue = false;
        protected IntPtr m_pWaitBegExecEvent = IntPtr.Zero;
        protected string m_strWaitBegExecEvent = null;
        protected UInt32 m_nWaitBegExecTimeout = 0;

        // sync event - sig begin
        protected IntPtr m_pTriggerBegExecEvent = IntPtr.Zero;
        protected string m_strTriggerBegExecEvent = null;

        // sync event -- end
        protected IntPtr m_pWaitOnEndExecEvent = IntPtr.Zero;
        protected string m_strWaitOnEndExecEvent = null;
        protected UInt32 m_nWaitOnEndExecTimeout = 0;

        // macro sync event -- sig end
        protected IntPtr m_pTriggerEndExecEvent = IntPtr.Zero;
        protected string m_strTriggerEndExecEvent = null;

        #region [設定觸發事件]

        private static IntPtr OpenEvent( string name )
        {
            if ( String.IsNullOrEmpty( name ) ) return IntPtr.Zero;

            return EventWinSdkFunctions.Open( ( UInt32 ) EVT_ACC_RIGHT.ALL_ACCESS | ( UInt32 ) EVT_ACC_RIGHT.MODIFY_STATE, false, name );
        }

        #region 執行前: 等待事件

        public bool ResetOnBeginWaitEvent( Int32 nFuncSyncWait )
        {
            if ( m_bRecycled ) return false;
            bool stat = Monitor.TryEnter( m_hSync, nFuncSyncWait );
            if ( !stat ) return false;
            try
            {
                m_bWaitBegExecErrorContinue = false;
                m_nWaitBegExecTimeout = 0;
                // close
                if ( m_pWaitBegExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pWaitBegExecEvent );
                // reset
                m_pWaitBegExecEvent = IntPtr.Zero;
                m_strWaitBegExecEvent = null;
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public bool ConfigOnBeginWaitEvent( bool bTimeoutContinueExec, UInt32 timeout, string name, Int32 nFuncSyncWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nFuncSyncWait );
            if ( !stat ) return false;
            try
            {
                m_bWaitBegExecErrorContinue = bTimeoutContinueExec;
                m_nWaitBegExecTimeout = timeout;

                if ( name != m_strWaitBegExecEvent || m_pWaitBegExecEvent == IntPtr.Zero )
                {
                    // close previous one
                    if ( m_pWaitBegExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pWaitBegExecEvent );
                    // open new one
                    m_pWaitBegExecEvent = OpenEvent( name );
                    m_strWaitBegExecEvent = m_pWaitBegExecEvent == IntPtr.Zero ? null : String.Copy( name );
                }
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public string OnBeginWaitEvent { get { return m_strWaitBegExecEvent; } }
        public bool ContinueExecOnBegingWaitTimeout { get { return m_bWaitBegExecErrorContinue; } }
        public UInt32 OnBeginWaitTimeout { get { return m_nWaitBegExecTimeout; } }

        #endregion

        #region 執行前: 觸發事件

        public bool ResetOnBeginTriggerEvent( Int32 nWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nWait );
            if ( !stat ) return false;
            try
            {
                // close event
                if ( m_pTriggerBegExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pTriggerBegExecEvent );
                m_pTriggerBegExecEvent = IntPtr.Zero;
                // reset string
                m_strTriggerBegExecEvent = null;
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public bool ConfigOnBeginTriggerEvent( string name, Int32 nWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nWait );
            if ( !stat ) return false;
            try
            {
                if ( name != m_strTriggerBegExecEvent || m_pTriggerBegExecEvent == IntPtr.Zero )
                {
                    if ( m_pTriggerBegExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pTriggerBegExecEvent );
                    m_pTriggerBegExecEvent = OpenEvent( name );
                    m_strTriggerBegExecEvent = m_pTriggerBegExecEvent == IntPtr.Zero ? null : String.Copy( name );
                }
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public string OnBeginTriggerEvent { get { return m_strTriggerBegExecEvent; } }

        #endregion

        #region 執行後: 等待事件

        public bool ResetOnEndWaitEvent( Int32 nWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nWait );
            if ( !stat ) return false;
            try
            {
                // close event
                if ( m_pWaitOnEndExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pWaitOnEndExecEvent );
                m_pWaitOnEndExecEvent = IntPtr.Zero;
                // reset string
                m_strWaitOnEndExecEvent = null;
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public bool ConfigOnEndWaitEvent( string name, UInt32 timeout, Int32 nWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nWait );
            if ( !stat ) return false;
            try
            {
                if ( name != m_strWaitOnEndExecEvent || m_pWaitOnEndExecEvent == IntPtr.Zero )
                {
                    if ( m_pWaitOnEndExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pWaitOnEndExecEvent );
                    m_pWaitOnEndExecEvent = OpenEvent( name );
                    m_strWaitOnEndExecEvent = m_pWaitOnEndExecEvent == IntPtr.Zero ? null : String.Copy( name );
                }
                m_nWaitOnEndExecTimeout = timeout;
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public string OnEndWaitEvent { get { return m_strWaitOnEndExecEvent; } }
        public UInt32 OnEndWaitEventTimeout { get { return m_nWaitOnEndExecTimeout; } }

        #endregion

        #region 執行後: 觸發事件

        public bool ResetOnEndTriggerEvent( Int32 nWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nWait );
            if ( !stat ) return false;
            try
            {
                // close event
                if ( m_pTriggerEndExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pTriggerEndExecEvent );
                m_pTriggerEndExecEvent = IntPtr.Zero;
                // reset string
                m_strTriggerEndExecEvent = null;
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public bool ConfigOnEndTriggerEvent( string name, Int32 nWait )
        {
            if ( m_bRecycled ) return false;

            bool stat = Monitor.TryEnter( m_hSync, nWait );
            if ( !stat ) return false;
            try
            {
                if ( name != m_strTriggerEndExecEvent || m_pTriggerEndExecEvent == IntPtr.Zero )
                {
                    if ( m_pTriggerEndExecEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( m_pTriggerEndExecEvent );
                    m_pTriggerEndExecEvent = OpenEvent( name );
                    m_strTriggerEndExecEvent = m_pTriggerBegExecEvent == IntPtr.Zero ? null : String.Copy( name );
                }
            }
            finally { Monitor.Exit( m_hSync ); }
            return true;
        }

        public string OnEndTriggerEvent { get { return m_strTriggerEndExecEvent; } }

        #endregion

        #endregion
    }
}
