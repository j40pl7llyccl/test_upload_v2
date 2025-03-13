using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uIP.Lib.Multithreading
{    

    /// <summary>
    /// Mech
    ///        >>> User Add a job   ( ProcessingJob ) --+
    ///     +------------------------+      +-------+   |
    ///  +-*|  Thread Pool ( Queue ) |      | Job Q | *-+
    ///  |  +------------------------+      +-------+ *-+
    ///  |      |        Trigger a event to wake up |   |
    ///  |      |             +---------------------+   |
    ///  |  +--> -------------*--+                      |
    ///  |  | Dispatching Thread | Get a job from Q     |    *-+
    ///  |  +--------------------+ +--------------------+      +->>> Exec the job. Done -> back to pool.
    ///  +-------------------------+ Have available thread ? *-+
    ///
    /// maxJobs: Specify the number of job can be queued. 0 -> no limit.
    /// </summary>
    public class ProcessingWokDispatcher : IDisposable
    {
        protected bool _bDisposing;
        protected bool _bDisposed;

        protected bool _bTerminating;
        protected bool _bDispatchingTAlive;

        static protected WorkerLogNorHandler _LogNor = null;
        static protected WorkerLogWrnHandler _LogWrn = null;

        //protected object _Sync;
        protected Semaphore _BinSem;

        protected Queue<ProcessingJob> _JobQ;
        protected Thread _DispatchingT; // Dispatching Thread
        protected ProcessingWorkerPool _ThreadPool;
        protected int _nMaxJobs;
        protected int _nWakeupPollTm;

        protected UInt32 _nID = 0;

        public int _nDbgCount = 0;

        #region >>> Property <<<

        public int NumOfThreads
        {
            get { return ( _bTerminating || _bDisposing || _bDisposed || _ThreadPool == null ) ? 0 : _ThreadPool.AllocCount; }
        }

        public int NumOfJobs
        {
            get { return ( _bTerminating || _bDisposing || _bDisposed || _JobQ == null ) ? 0 : _JobQ.Count; }
        }

        public int IdleThreads
        {
            get { return ( _bTerminating || _bDisposing || _bDisposed || _ThreadPool == null ) ? 0 : _ThreadPool.AvailableCount; }
        }

        public string GivenName
        {
            get { return ( _ThreadPool == null ? null : _ThreadPool.IDName ); }
            set { if ( _ThreadPool != null ) _ThreadPool.IDName = value; }
        }

        public UInt32 ID
        {
            get { return _nID; }
            set { _nID = value; }
        }

        public static WorkerLogNorHandler LogNormal { get { return _LogNor; } set { _LogNor = value; } }
        public static WorkerLogWrnHandler LogWarning { get { return _LogWrn; } set { _LogWrn = value; } }

        #endregion

        private void PrivateInit( int allocPoolSize, int maxJobs, string givenName )
        {
            _bDisposing = false;
            _bDisposed = false;
            _bTerminating = false;
            _bDispatchingTAlive = false;

            //_Sync = new object();
            _BinSem = new Semaphore( 1, 1 );
            _JobQ = new Queue<ProcessingJob>();
            _DispatchingT = new Thread( new ThreadStart( this.Dispatching ) );
            _ThreadPool = new ProcessingWorkerPool( allocPoolSize, givenName );
            _nMaxJobs = maxJobs <= 0 ? 0 : maxJobs;
            _nWakeupPollTm = 2000;

#if ( CONSOLE_DBG )
            //Console.Write( "MTProcessingThreadDispatcher: Thread Pool count = {0}, Max JobQ = {1}\n", allocPoolSize, maxJobs );
#endif
        }

        public ProcessingWokDispatcher( int allocPoolSize, int maxJobs, bool bImmediateStart, string givenName )
        {
            PrivateInit( allocPoolSize, maxJobs, givenName );

            if ( bImmediateStart )
                _DispatchingT.Start();
        }

        public ProcessingWokDispatcher( int allocPoolSize, int maxJobs, bool bImmediateStart, string givenName, ThreadPriority priority )
        {
            PrivateInit( allocPoolSize, maxJobs, givenName );

            _DispatchingT.Priority = priority;
            if ( bImmediateStart )
                _DispatchingT.Start();
        }

        ~ProcessingWokDispatcher()
        {
            Dispose( false );
        }

        //
        // Dispatching thread: Exec Dispatching
        //
        protected virtual void Dispatching()
        {
            if ( _ThreadPool == null || _ThreadPool.Event == null )
                return;

            _bDispatchingTAlive = true;

#if ( CONSOLE_DBG )
            //Console.Write( "MTProcessingThreadDispatcher: Dispatching Thread begin!\n" );
#endif

            while ( !_bTerminating )
            {
                if ( _JobQ.Count > 0 && _ThreadPool.AvailableCount > 0 )
                {
                    //
                    // Dispatch job
                    //
                    // Get a job from queue.
                    ProcessingJob job = null;
                    bool status = _BinSem.WaitOne();
                    if ( ! status )
                        continue;
                    try
                    {
                        job = ( _JobQ.Count <= 0 ) ? null : _JobQ.Dequeue();
                        _nDbgCount--;
                    }
                    finally
                    {
                        _BinSem.Release();
                    }

                    if ( job == null )
                        continue;

                    // Get a idle thread.
                    ProcessingWorkerItem thread = _ThreadPool.Dequeue();
                    if ( _bTerminating )
                        break;

                    if ( thread == null )
                    {
                        throw new Exception( "MTProcessingThreadDispatcher/Dispatching: no thread to do job!" );
                    }

                    // Trigger the thread to do job.
                    if ( !thread.ExecJob( job ) )
                    {
                        throw new Exception( "MTProcessingThreadDispatcher/Dispatching: a thread cannot accept a new job! " );
                    }

#if ( CONSOLE_DBG )
                    //Console.Write( "Thread({0}) exec job({1})\n", String.IsNullOrEmpty( thread._strDbgName ) ? "" : thread._strDbgName, String.IsNullOrEmpty( job.Name ) ? "" : job.Name );
#endif
                }
                else if ( _JobQ.Count > 0 && _ThreadPool.AvailableCount <= 0 )
                {
                    if ( _ThreadPool.Event.WaitOne( _nWakeupPollTm, true ) ) // still have job, keep polling & wait event
                        _ThreadPool.Event.Reset();
                }
                else if ( _JobQ.Count <= 0 )
                {
                    _ThreadPool.Event.WaitOne(); // no job, wait forever
                    _ThreadPool.Event.Reset();
                }
#if ( CONSOLE_DBG )
                //Console.Write( "Thread resume = {0}, Job Q count = {1}\n", _ThreadPool.AvailableCount, _JobQ.Count );
#endif
            }

            _bDispatchingTAlive = false;
        }

        #region >>> IDisposable <<<

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( _bDisposing )
                return;

            _bDisposing = true;
            if ( !_bDisposed )
            {
                _bTerminating = true;
                // Stop dispatching thread
                if ( _bDispatchingTAlive )
                {
                    if ( _ThreadPool.Event != null )
                        _ThreadPool.Event.Set();
                    if ( !_DispatchingT.Join( _nWakeupPollTm * 2 ) )
                    {
                        if ( _LogNor != null )_LogNor( String.Format( "[MTProcessingThreadDispatcher::Dispose] {0} cannot wait({1}ms) dispatching thread done.", _ThreadPool.IDName, _nWakeupPollTm * 2 ) );
                    }
                }

                _ThreadPool.Dispose();

                _DispatchingT = null;
                _ThreadPool = null;

                Reset(); // free jobs

                _BinSem.Close();
                _BinSem = null;
            }
            _bDisposed = true;
            _bDisposing = false;
        }

        #endregion

        public virtual bool JobAllDone()
        {
            if ( _bDisposed || _bDisposing || _bTerminating ) return true;
            if ( _ThreadPool == null ) return true;

            return ( _ThreadPool.IsWholeJobDone() );
        }

        public virtual WorkerAddJobRetCode AddJob( ProcessingJob job, int waitms )
        {
            if ( _bDisposed || _bDisposing || _bTerminating )
                return WorkerAddJobRetCode.ResErr;
            if ( job == null )
                return WorkerAddJobRetCode.JobErr;

            bool status = _BinSem.WaitOne( waitms, true );
            if ( !status )
                return WorkerAddJobRetCode.Timeout;
            if ( _nMaxJobs > 0 && _JobQ.Count >= _nMaxJobs )
            {
                _BinSem.Release();
                return WorkerAddJobRetCode.Full;
            }
            if ( _JobQ.Contains( job ) ) // not accept same item
            {
                _BinSem.Release();
                return WorkerAddJobRetCode.OK;
            }

            try
            {
                _nDbgCount++;
                _JobQ.Enqueue( job );
            }
            finally
            {
                _BinSem.Release();
            }

            _ThreadPool.Event.Set();

            return WorkerAddJobRetCode.OK;
        }

        public virtual void Reset( )
        {
            if ( _bDisposed || _bDisposing || _bTerminating || _ThreadPool == null )
                return;

            // Empty queue
            ProcessingJob job = null;
            while ( _JobQ.Count > 0 )
            {
                _BinSem.WaitOne();
                job = _JobQ.Dequeue();
                _BinSem.Release();

                if ( job == null )
                    continue;

                // detach dispatcher
                job.MTDispatcher = null;
                // dispose the job
                job.Dispose();
//                if ( job.OnBack != null )
//                    job.OnBack();
            }
        }
    }
}
