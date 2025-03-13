using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uIP.Lib.Multithreading
{
    /// <summary>
    /// 1. Handled by a dispathing thread.
    /// 2. Some variables not protected.
    /// </summary>
    public class ProcessingWorkerItem : IDisposable
    {
        //private bool _bIntoNewJob;
        //private object _SyncCall;
        protected Semaphore _BinSem;
        // Thread control events and flags
        private ManualResetEvent _WakeupEvt;     // Instance : wake up this thread to do a job.
        private ManualResetEvent _NotifyDoneEvt; // Reference: notify caller thread done the job.

        protected bool _bDisposed;                 // This instance no more accessable.
        protected bool _bDisposing;
        protected bool _bTerminating;              // Trigger thread to end.
        protected bool _bEndByException;           // Trigger thread to end with an exception.
        protected bool _bBack2Owner;

        static protected WorkerLogNorHandler _LogNor = null;
        static protected WorkerLogWrnHandler _LogWrn = null;

        // Thread instance and state
        protected Thread _Thread;     // Instance
        protected WorkerState _State; // Thread state
        protected int _nWaitEndTm;    // Caller wait the thread to end.

        protected bool _bBusy;
        protected bool _bEvtSignal;

        protected ProcessingJob _Job;

        protected ProcessingWorkerPool _Owner; // Reference

        public string _strDbgName;
        public string _strDbgExceptionJobName;
        public string _strDbgExceptionStackTrace;

        protected int _nWakeUpTmMS = 2000;

        protected ProcWorkExecDoneHandler _fpExeEndCallback;

        #region >>> Property <<<

        public WorkerState State
        {
            get { return _State; }
        }

        public ManualResetEvent DoneBackEvt
        {
            set { _NotifyDoneEvt = value; }
        }

        public bool IsBusy
        {
            get { return _bBusy; }
        }

        public bool IsSignalJob
        {
            get { return _bEvtSignal; }
        }

        public ProcessingWorkerPool Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        public bool IsBack2Owner
        {
            get { return _bBack2Owner; }
            set { _bBack2Owner = value; }
        }

        public int PoolWakeUpTimeMS
        {
            get { return _nWaitEndTm; }
            set { if ( value < 0 ) return; _nWaitEndTm = value; }
        }

        public ProcWorkExecDoneHandler fpExeEndCallback
        {
            get { return _fpExeEndCallback; }
            set { _fpExeEndCallback = value; }
        }

        #endregion

        public ProcessingWorkerItem()
        {
            //_bIntoNewJob = false;
            //_SyncCall = new object();
            _BinSem = new Semaphore( 1, 1 );

            _WakeupEvt = new ManualResetEvent( false );
            _NotifyDoneEvt = null;
            _bDisposed = false;
            _bDisposing = false;
            _bTerminating = false;
            _bEndByException = false;
            _bBack2Owner = false;

            _Thread = new Thread( new ThreadStart( this.DoingJob ) );
            _State = WorkerState.Create;
            _nWaitEndTm = 5000;

            _bBusy = false;
            _bEvtSignal = false;

            _Job = null;

            _Owner = null;

            _strDbgName = null;
            _strDbgExceptionJobName = null;
            _strDbgExceptionStackTrace = null;

            if ( _Thread != null )
                _Thread.Start();
        }

        public ProcessingWorkerItem( ProcessingWorkerPool owner, ManualResetEvent evt, string dbgNm )
        {
            //_bIntoNewJob = false;
            //_SyncCall = new object();
            _BinSem = new Semaphore( 1, 1 );

            _WakeupEvt = new ManualResetEvent( false );
            _NotifyDoneEvt = evt;
            _bDisposed = false;
            _bDisposing = false;
            _bTerminating = false;
            _bEndByException = false;
            _bBack2Owner = false;

            _Thread = new Thread( new ThreadStart( this.DoingJob ) );
            _State = WorkerState.Create;
            _nWaitEndTm = 5000;

            _bBusy = false;
            _bEvtSignal = false;


            _Job = null;

            _Owner = owner;

            _strDbgName = String.IsNullOrEmpty( dbgNm ) ? null : String.Copy( dbgNm );
            _strDbgExceptionJobName = null;
            _strDbgExceptionStackTrace = null;

            if ( _Thread != null )
                _Thread.Start();
        }

        ~ProcessingWorkerItem()
        {
            Dispose( false );
        }

        protected virtual void DoingJob()
        {
            _State = WorkerState.EnteringExecMethod;

            int nLocWakeUp = _nWaitEndTm < 0 ? 0 : _nWaitEndTm; ;

#if ( CONSOLE_DBG )
            Console.Write( "{0}: Thread begin!\n", _strDbgName );
#endif

            while ( !_bTerminating && !_bEndByException )
            {
                _bEvtSignal = false;

                bool status = _WakeupEvt.WaitOne( nLocWakeUp, true );
                if ( !status )
                {
                    if ( _nWaitEndTm != nLocWakeUp && _nWaitEndTm >= 0 )
                        nLocWakeUp = _nWaitEndTm;
                    continue;
                }
                if ( _bTerminating )
                    break;

                _bEvtSignal = true;
                _bBusy = true;

                //
                // Process assigned job
                //
                bool bFuncStat = false;
                try
                {
                    // Do Job
                    if ( _Job != null )
                    {
                        if ( !_Job.Stop && !_Job.Disposing && _Job.OnExec != null )
                        {
                            // exec job callback function
                            bFuncStat = _Job.OnExec( this._strDbgName, _Job.Name, _Job.Context );
                            if ( !bFuncStat )
                                if( _LogNor != null ) _LogNor( String.Format( "[MTProcessingThreadItem::DoingJob] thread({0}) exec job({1}) NG.", this._strDbgName, _Job.Name ) );
                            // exec job done callback funcation
                            if ( bFuncStat && _Job.OnDone != null )
                                _Job.OnDone( _Job.Context );
                        }
                    }
                }
                catch ( Exception exp )
                {
                    _bEndByException = true;
                    _strDbgExceptionStackTrace = String.IsNullOrEmpty( exp.StackTrace ) ? null : String.Copy( exp.StackTrace );
                    _strDbgExceptionJobName = _Job == null || String.IsNullOrEmpty( _Job.Name ) ? null : String.Copy( _Job.Name );

                    if ( _LogWrn != null ) _LogWrn( 1, String.Format( "MTProcessingThreadItem/DoingJob: Thread ({0}) do job({1}) get exception {2}",
                                                       String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName,
                                                       String.IsNullOrEmpty( _strDbgExceptionJobName ) ? "" : _strDbgExceptionJobName,
                                                       String.IsNullOrEmpty( _strDbgExceptionStackTrace ) ? "" : _strDbgExceptionStackTrace ) );
                    _State = WorkerState.ExitExecMethod;
                }
                finally
                {
                    if ( _Job != null )
                    {
                        _Job.ExecJobDone();
                        // Detach from this job
                        if ( !_Job.Disposing )
                        {
                            _Job.AttachedThread = null;
                        }
                    }
                    // Reset Job
                    _Job = null;
                    // Clear Event
                    _WakeupEvt.Reset();

                    // Before event back, reset the flag. Sync issue.
                    _bBusy = false;

                    if ( !_bTerminating )
                    {
                        // Event back
                        if ( _NotifyDoneEvt != null )
                        {
                            _NotifyDoneEvt.Set();
                            _NotifyDoneEvt = null;
                        }
                        // Finall done callback
                        if ( _fpExeEndCallback != null )
                        {
                            _fpExeEndCallback();
                            _fpExeEndCallback = null;
                        }
                        // Have an owner
                        if ( _Owner != null )
                            _Owner.Enqueue( this );
                    }

                }
            } // end-while

            _State = WorkerState.ExitExecMethod;
            if ( _nWaitEndTm != nLocWakeUp && _nWaitEndTm >= 0 )
                nLocWakeUp = _nWaitEndTm;
        }

        #region >>> Implement IDisposable <<<

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( bool disposing )
        {
            if ( _bDisposing )
                return;

            _bDisposing = true;
            if ( !_bDisposed )
            {
                _bTerminating = true;
                _WakeupEvt.Set();

                if ( !_Thread.Join( _nWaitEndTm ) )
                {
                    if ( _LogNor != null ) _LogNor( String.Format( "[MTProcessingThreadItem::Dispose] thread({0}/{1}) not done timeout({2}).", 
                                                    _Owner != null ? _Owner.IDName : "", 
                                                    String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName,
                                                    _nWaitEndTm ) );
                }

                _BinSem.Close();
                _BinSem = null;
                _WakeupEvt.Close();
                _WakeupEvt = null;
                _Thread = null;
            }
            _bDisposed = true;
            _bDisposing = false;
        }

        #endregion

        public bool ExecJob( ProcessingJob job )
        {
            if ( job == null )
            {
                if ( _LogWrn != null ) _LogWrn( 1, String.Format( "[MTProcessingThreadItem::ExecJob] Thread({0}) -- got null job!", String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName ) );
                return false;
            }

            if ( _bDisposing || _BinSem == null )
            {
                if ( _LogWrn != null ) _LogWrn( 1, String.Format( "[MTProcessingThreadItem::ExecJob] Thread({0}) -- Disposing({1})/ BinSem({2})", String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName, _bDisposing ? "true" : "false", _BinSem == null ? "null" : "ok" ) );
                return false;
            }

            if ( _bTerminating || _bDisposed || _bBusy || _State != WorkerState.EnteringExecMethod )
            {
                if ( _LogWrn != null ) _LogWrn( 1, String.Format( "[MTProcessingThreadItem::ExecJob] Thread({0}) -- Terminating({1})/ Disposed({2})/ Busy({3})/ State({4})",
                                                   String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName,
                                                   _bDisposed ? "true" : "false",
                                                   _bBusy ? "true" : "false",
                                                   _State.ToString() ) );
                return false;
            }

            bool status = _BinSem.WaitOne();
            if ( !status )
            {
                if ( _LogWrn != null ) _LogWrn( 1, String.Format( "[MTProcessingThreadItem::ExecJob] Thread({0}) -- obtain sem error", String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName ) );
                return false;
            }
            if ( _Job != null )
            {
                if ( _LogWrn != null ) _LogWrn( 1, String.Format( "[MTProcessingThreadItem::ExecJob] Thread({0}) already attached a job.", String.IsNullOrEmpty( _strDbgName ) ? "" : _strDbgName ) );
                _BinSem.Release();
                return false;
            }

            try
            {
                job.AttachedThread = this;
                _Job = job;
                _WakeupEvt.Set();
            }
            finally
            {
                _BinSem.Release();
            }

            return true;
        }

    }
}
