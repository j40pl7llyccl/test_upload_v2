using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase : IDisposable
    {
        //private Int32 _nDummyRun = 0;
        //private Int32 _nDummyRunCount = 0;

        protected static Dictionary<int, object> _StateIntDic = null;
        protected static object _syncStateIntDic = new object();
        static UCBlockBase()
        {
            lock(_syncStateIntDic)
            {
                _StateIntDic = GetEnumDic( _StateIntDic, typeof( UCBlockStateReserved ) );
            }
        }

        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;
        public bool IsDispose { get { return ( m_bDisposing || m_bDisposed ); } }

        protected object _Owner;

        protected Int32 _nLastPrevState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA;
        protected Int32 _nPrevState = (int) UCBlockStateReserved.UCBLOCK_STATE_NA;
        protected Int32 _nState = (int) UCBlockStateReserved.UCBLOCK_STATE_PREPARING;
        protected Int32 _nProceedingState = (int) UCBlockStateReserved.UCBLOCK_STATE_NA;
        protected bool _bPause = false;
        protected bool _bProceed = false;
        protected bool _bStop = false;
        protected string _strDisplayName = null;

        public UCBlockBase()
        {
            InitGetSet();
        }

        public void Dispose()
        {
            if ( m_bDisposing || m_bDisposed )
                return;
            m_bDisposing = true;

            Dispose( true );

            m_bDisposed = true;
            m_bDisposing = false;

        }
        protected virtual void Dispose(bool disposing)
        {
            DisposeAssistants();
            HandleFinalData();
        }

        public object Owner {  get { return _Owner; } set { _Owner = value; } }
        public Int32 State {  get { return _nState; } }
        /// <summary>
        /// ID as unique; inherit class must set this
        /// </summary>
        public string ID { get { return _strID; } set { _strID = String.IsNullOrEmpty( value ) ? "" : String.Copy(value); } }
        public fpLogMessage Log {  get { return _fpLog; } set { _fpLog = value; } }
        public string DisplayName {  get { return _strDisplayName; } }

        // state control
        // keep data of current state
        virtual public void Pause() { _bPause = true; }
        // proceed from pause state
        virtual public void Proceed() { AssistantsFirstProceed(); _bProceed = true; }
        // reset condition to the end, also can release the pause status
        // goto end state and UCT to begin
        virtual public void Stop() { _bStop = true; }
        // this function only be executed by single thread, regarding multi-threading
        // thinking and cannot be suspended by sync object otherwise polling design broken.
        virtual public string OutDescriptionOfMutableParameters() { return null; }
        // Remark:
        // - ppRetMsg: must be alloc from AllocBaseDataTypeMem()
        // - currently used WellDefinedRunner_1()
        virtual public Int32 Run( UDataCarrierSet pParam )
        {
            return WellDefinedRunner_1( pParam);
        }
        virtual public void ResetToRun()
        {
            _nLastPrevState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA;
            _nPrevState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA;
            _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_PREPARING;
            _nProceedingState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA;
            _bPause = false;
            _bProceed = false;
            _bStop = false;

            ClearFinalData();
        }

    }
}
