using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace uIP.Lib.Multithreading
{
    public class UMultithreading : IDisposable, IManageDedicateWorker
    {
        protected bool _bDisposed = false;
        protected bool _bDisposing = false;
        protected bool _bClearGroupJList = false;
        protected bool _bClearJList = false;
		protected bool _bClearRunGroupJList = false;
		protected bool _bTerminating;
		protected bool _bDispatchingTAlive;
        //protected object _hSyncDispo = null;

        protected List<ProcessingWokDispatcher> _Dispatchers = null;
        protected object _hSyncDisp = null;

        protected List<ProcessingJob> _RecJobs = null;
        protected object _hSyncJobs = null;

        protected List<ProcessingJobGroup> _RecGrps = null;
        protected object _hSyncGrps = null;

		protected Queue<ProcessingJobGroup> _RunGrps = null;
		//protected object _Sync;
		protected Semaphore _BinSem;
		protected Thread _DispatchingT; // Dispatching Thread
		protected ManualResetEvent _UpdateEvt;
		protected int _nMaxJobs = 0;
		protected int _nWakeupRunJobGrpTm = 10 * 1000;

        protected Int32 _nWaitTimeout = 2000;
        protected Int32 _nPollingInterval = 100;

        internal static WorkerLogNorHandler _LogNor = null;
        internal static WorkerLogWrnHandler _LogWrn = null;

        private object _hSyncDW = new object();
        private List<DedicatedWorker> DWs = new List<DedicatedWorker>();

        public UMultithreading()
        {
            //_hSyncDispo = new object();

            _Dispatchers = new List<ProcessingWokDispatcher>();
            _hSyncDisp = new object();

            _RecJobs = new List<ProcessingJob>();
            _hSyncJobs = new object();

            _RecGrps = new List<ProcessingJobGroup>();
            _hSyncGrps = new object();

			_RunGrps = new Queue<ProcessingJobGroup>();
			_BinSem = new Semaphore( 1, 1 );
			_DispatchingT = new Thread( new ThreadStart( this.Dispatching ) );
			_bTerminating = false;
			_bDispatchingTAlive = false;
			_UpdateEvt = new ManualResetEvent( false );
			_DispatchingT.Start();
        }

        #region >>> Resource manager <<<

        ~UMultithreading()
        {
            Dispose( false );
        }

        public virtual void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool bDisposing )
        {
            if ( _bDisposing )
                return;

            _bDisposing = true;

            // free dedicated worker resources
            RecycleDWs();

            // Stop all working threads
            for ( int i = 0; i < _Dispatchers.Count; i++ )
            {
                if ( _Dispatchers[ i ] == null )
                    continue;
                _Dispatchers[ i ].Dispose();
                _Dispatchers[ i ] = null;
            }
            _Dispatchers.Clear();
            _Dispatchers = null;

            // Free group job list
            ClearGroupList();

            // Free job list
            ClearJobList();

			_bTerminating = true;
			// Stop dispatching thread
			if ( _bDispatchingTAlive )
			{
				if ( _UpdateEvt != null )
					_UpdateEvt.Set();
				if ( !_DispatchingT.Join( _nWakeupRunJobGrpTm * 2 ) )
				{
					if ( _LogNor != null ) _LogNor( String.Format( "[MTProcessingThreadDispatcher::Dispose] MTProcessing cannot wait({0}ms) dispatching thread done.", _nWakeupRunJobGrpTm * 2 ) );
				}
			}
			if ( _UpdateEvt != null )
			{
				_UpdateEvt.Close();
				_UpdateEvt = null;
			}
			_DispatchingT = null;
			_BinSem.Close();
			_BinSem = null;
            _bDisposed = true;

            _bDisposing = false;
        }

        #endregion

        #region >>> Dispatcher/ Working Instance <<<

        protected ProcessingWokDispatcher GetSameDispatcher( string str )
        {
            if ( _bDisposed || _bDisposing || String.IsNullOrEmpty( str ) )
                return null;

            ProcessingWokDispatcher ret = null;
            for ( int i = 0; i < _Dispatchers.Count; i++ )
            {
                if ( _Dispatchers[ i ] == null )
                    continue;
                if ( String.IsNullOrEmpty( _Dispatchers[ i ].GivenName ) )
                    continue;
                if ( str == _Dispatchers[ i ].GivenName )
                {
                    ret = _Dispatchers[ i ]; break;
                }
            }

            return ret;
        }

        protected ProcessingWokDispatcher GetSameDispatcher( UInt32 id )
        {
            if ( _bDisposed || _bDisposing )
                return null;

            ProcessingWokDispatcher ret = null;
            for ( int i = 0; i < _Dispatchers.Count; i++ )
            {
                if ( _Dispatchers[ i ] == null )
                    continue;
                if ( _Dispatchers[ i ].ID == id )
                {
                    ret = _Dispatchers[ i ]; break;
                }
            }

            return ret;
        }

        public bool AddDispatcher( ProcessingWokDispatcher dp )
        {
            if ( _bDisposed || _bDisposing || dp == null )
                return false;

            Monitor.Enter( _hSyncDisp );
            try
            {
                if ( ! _Dispatchers.Contains( dp ) )
                _Dispatchers.Add( dp );
            }
            finally { Monitor.Exit( _hSyncDisp ); }

            return true;
        }

        public bool NewDispatcher( string strGivenNm, Int32 nWorkingThreads, bool bAutoStart )
        {
            if ( _bDisposed || _bDisposing || String.IsNullOrEmpty( strGivenNm ) )
                return false;

            Monitor.Enter( _hSyncDisp );
            try
            {
                if ( GetSameDispatcher( strGivenNm ) == null )
                    _Dispatchers.Add( new ProcessingWokDispatcher( nWorkingThreads, 0, bAutoStart, strGivenNm ) );
            }
            finally { Monitor.Exit( _hSyncDisp ); }

            return true;
        }

        public bool NewDispatcher( UInt32 id, Int32 nWorkingThreads, bool bAutoStart )
        {
            if ( _bDisposed || _bDisposing )
                return false;

            Monitor.Enter( _hSyncDisp );
            try
            {
                if ( GetSameDispatcher( id ) == null )
                {
                    ProcessingWokDispatcher dp = new ProcessingWokDispatcher( nWorkingThreads, 0, bAutoStart, null );
                    dp.ID = id;

                    _Dispatchers.Add( dp );
                }
            }
            finally { Monitor.Exit( _hSyncDisp ); }

            return true;
        }

        public ProcessingWokDispatcher GetDispatcher( UInt32 id )
        {
            if ( _bDisposed || _bDisposing )
                return null;

            return GetSameDispatcher( id );
        }

        public ProcessingWokDispatcher GetDispatcher( string str )
        {
            if ( _bDisposed || _bDisposing || String.IsNullOrEmpty( str ) )
                return null;

            return GetSameDispatcher( str );
        }

        #endregion

        #region >>> Job/ Job Group <<<

        protected virtual void ClearJobList()
        {
            if ( _bClearJList )
                return;

            List<ProcessingJob> attachedTJobs = new List<ProcessingJob>();

            Monitor.Enter( _hSyncJobs );
            try
            {
                _bClearJList = true;

                // Move the job that attached to a thread
                bool bExit = true;
                while ( true )
                {
                    bExit = true;

                    for ( int i = 0; i < _RecJobs.Count; i++ )
                    {
                        if ( _RecJobs[ i ] == null )
                        {
                            _RecJobs.RemoveAt( i );
                            bExit = false; break;
                        }
                        if ( _RecJobs[ i ].AttachedThread != null )
                        {
                            attachedTJobs.Add( _RecJobs[ i ] );
                            _RecJobs.RemoveAt( i );
                            bExit = false; break;
                        }
                    }

                    if ( bExit )
                        break;
                }

                // remove
                for ( int i = 0; i < _RecJobs.Count; i++ )
                {
                    if ( _RecJobs[ i ] == null )
                        continue;
                    _RecJobs[ i ].Dispose();
                    _RecJobs[ i ] = null;
                }

                // clear
                _RecJobs.Clear();
            }
            finally
            {
                _bClearJList = false;
                Monitor.Exit( _hSyncJobs );
            }

            // 
            DateTime tmBeg = DateTime.Now, tmCurr = DateTime.Now;
            bool bThreadDetach = false;
            for ( int i = 0; i < attachedTJobs.Count; i++ )
            {
                // TODO: check if attached thread
                if ( attachedTJobs[ i ].AttachedThread != null )
                {
                    // Wait a while
                    tmBeg = DateTime.Now;
                    bThreadDetach = false;
                    while ( true )
                    {
                        tmCurr = DateTime.Now;
                        TimeSpan diff = tmCurr - tmBeg;
                        if ( diff.Milliseconds >= _nWaitTimeout )
                        {
                            if ( _LogNor != null ) _LogNor( String.Format( "[MTProcessing::ClearJobList] job({0}) still attach to a thread.", String.IsNullOrEmpty( attachedTJobs[ i ].Name ) ? "" : attachedTJobs[ i ].Name ) );
                            break;
                        }
                        if ( attachedTJobs[ i ].AttachedThread == null )
                        {
                            bThreadDetach = true; break;
                        }
                    }
                    // a job was detached from a thread
                    if ( bThreadDetach )
                    {
                        attachedTJobs[ i ].Dispose();
                        attachedTJobs[ i ] = null;
                    }
                }
            }
            attachedTJobs.Clear();
            attachedTJobs = null;
        }
		 protected virtual void ClearRunGroupList()
		 {
			 if ( _bClearRunGroupJList )
				 return;
			 bool status = _BinSem.WaitOne( _nWakeupRunJobGrpTm, true );
			 if ( status == false )
				 return;

			 try
			 {
				 _bClearRunGroupJList = true;

				 for ( int i = 0; i < _RunGrps.Count; i++ )
				 {
					 ProcessingJobGroup jg = _RunGrps.Dequeue();
					 if ( jg == null )
						 continue;
					 jg.Dispose( false );
					 //_RecGrps[ i ] = null;
				 }

				 _RunGrps.Clear();
			 }
			 finally
			 {
				 _bClearRunGroupJList = false;
				 _BinSem.Release();
			 }
		 }

        protected virtual void ClearGroupList()
        {
            if ( _bClearGroupJList )
                return;

            Monitor.Enter( _hSyncGrps );
            try
            {
                _bClearGroupJList = true;

                for ( int i = 0; i < _RecGrps.Count; i++ )
                {
                    if ( _RecGrps[ i ] == null )
                        continue;
							_RecGrps[i].Dispose( false );
						  //_RecGrps[ i ] = null;
                }

                _RecGrps.Clear();
            }
            finally
            {
                _bClearGroupJList = false;
                Monitor.Exit( _hSyncGrps );
            }
        }

		 protected virtual void Dispatching()
		 {
			 _bDispatchingTAlive = true;
			 while ( _bTerminating == false )
			 {
				 if ( _RunGrps.Count > 0 )
				 {
					 ProcessingJobGroup jg = null;
					 bool status = _BinSem.WaitOne();
					 if ( status == false )
						 continue;

					 try
					 {
						 if ( _RunGrps.Count <= 0 )
							 jg = null;
						 else
							 jg = _RunGrps.Dequeue();
					 }
					 finally
					 {
						 _BinSem.Release();
					 }
					 if ( jg == null )
						 continue;

					 jg.BeginDispatchingJobs();
				 }
				 else
				 {
					 _UpdateEvt.WaitOne();
					 _UpdateEvt.Reset();
				 }
			 }
			 _bDispatchingTAlive = false;
		 }

		 protected virtual void RegRunJobGroup( ProcessingJobGroup jg )
		 {
			 if ( jg == null )
				 return;

			 bool status = _BinSem.WaitOne( _nWakeupRunJobGrpTm, true );
			 if ( status == false )
				 return;

			 if ( _nMaxJobs > 0 && _RunGrps.Count >= _nMaxJobs )
			 {
				 _BinSem.Release();
				 return;
			 }

			 if ( _RunGrps.Contains( jg ) )
			 {
				 _BinSem.Release();
				 return;
			 }

			 try
			 {
				 _RunGrps.Enqueue( jg );
			 }
			 finally
			 {
				 _BinSem.Release();
			 }
			 _UpdateEvt.Set();
			 //Monitor.Enter( _hSyncGrps );
			 //try
			 //{
			 //   if ( _RecGrps != null && !_RecGrps.Contains( jg ) )
			 //   {
			 //      _RecGrps.Add( jg );
			 //   }
			 //}
			 //finally { Monitor.Exit( _hSyncGrps ); }
		 }

		 public void AddRunJobGroup( ProcessingJobGroup jg )
		 {
			 RegRunJobGroup( jg );
		 }

        protected virtual void RegJobGroup( ProcessingJobGroup jg )
        {
            if ( jg == null )
                return;

            Monitor.Enter( _hSyncGrps );
            try
            {
                if ( _RecGrps != null && !_RecGrps.Contains( jg ) )
                {
                    _RecGrps.Add( jg );
                }
            }
            finally { Monitor.Exit( _hSyncGrps ); }
        }

        protected virtual void RmvJobGroup( ProcessingJobGroup jg )
        {
            if ( jg == null )
                return;

            Monitor.Enter( _hSyncGrps );
            try
            {
                if ( _RecGrps != null && _RecGrps.Contains( jg ) )
                {
                    _RecGrps.Remove( jg );
                    jg.Dispose();
                }
            }
            finally { Monitor.Exit( _hSyncGrps ); }
        }

        protected virtual void RegJob( ProcessingJob job )
        {
            if ( job == null )
                return;

            Monitor.Enter( _hSyncJobs );
            try
            {
                if ( _RecJobs != null && !_RecJobs.Contains( job ) )
                    _RecJobs.Add( job );
            }
            finally { Monitor.Exit( _hSyncJobs ); }
        }

        protected virtual void RmvJob( ProcessingJob job )
        {
            if ( job == null )
                return;

            Monitor.Enter( _hSyncJobs );
            try
            {
                if ( _RecJobs != null && _RecJobs.Contains( job ) )
                {
                    _RecJobs.Remove( job );
                    job.Dispose();
                }
            }
            finally { Monitor.Exit( _hSyncJobs ); }
        }

		  public ProcessingJobGroup NewRunJobGroup()
		  {
			  ProcessingJobGroup jg = new ProcessingJobGroup( null, null, new WorkerRegJobGroupHandler( this.RegJobGroup ), new WorkerRmRegJobGroupHandler( this.RmvJobGroup ) );
			  if ( jg == null )
				  return null;

			  RegJobGroup( jg );
			  //RegRunJobGroup( jg );
			  return jg;
		  }

		  public ProcessingJobGroup NewRunJobGroup( string name, WorkerNextJobGroupHandler njgh )
		  {
			  ProcessingJobGroup jg = new ProcessingJobGroup( name, njgh, new WorkerRegJobGroupHandler( this.RegJobGroup ), new WorkerRmRegJobGroupHandler( this.RmvJobGroup ) );
			  if ( jg == null )
				  return null;

			  RegJobGroup( jg );
			  //RegRunJobGroup( jg );
			  return jg;
		  }



        public ProcessingJobGroup NewJobGroup()
        {
            ProcessingJobGroup jg = new ProcessingJobGroup( null, null, new WorkerRegJobGroupHandler( this.RegJobGroup ), new WorkerRmRegJobGroupHandler( this.RmvJobGroup ) );
            if ( jg == null )
                return null;

            RegJobGroup( jg );
            return jg;
        }

        public ProcessingJobGroup NewJobGroup( string name, WorkerNextJobGroupHandler njgh )
        {
            ProcessingJobGroup jg = new ProcessingJobGroup( name, njgh, new WorkerRegJobGroupHandler( this.RegJobGroup ), new WorkerRmRegJobGroupHandler( this.RmvJobGroup ) );
            if ( jg == null )
                return null;

            RegJobGroup( jg );
            return jg;
        }

        public void DelJobGroup( ProcessingJobGroup jg )
        {
            RmvJobGroup( jg );
            jg.Dispose();
        }

        public ProcessingJob NewJob( ProcessingJobGroup jg,
                                       UInt32 nDispatcherID,
                                       string nm,                          // job name
                                       WorkerExecJobHandler fpExec,        // job main function
                                       WorkerJobDoneHandler fpDone,        // job done callback
                                       WorkerJobContHandler fpHndCntxt,    // handle context mem
                                       object context                      // context
                                     )
        {
            if ( jg == null || _RecGrps == null || !_RecGrps.Contains( jg ) )
                return null;
            ProcessingWokDispatcher dp = GetSameDispatcher( nDispatcherID );
            if ( dp == null )
                return null;

            ProcessingJob ajob = jg.NewJob( nm, fpExec, fpDone, fpHndCntxt, context, dp, new WorkerRmRegJobHandler( this.RmvJob ) );
            if ( ajob != null )
                RegJob( ajob );


            return ajob;
        }

        public ProcessingJob NewJob( ProcessingJobGroup jg,
                                       string strDispatcher,
                                       string nm,                          // job name
                                       WorkerExecJobHandler fpExec,        // job main function
                                       WorkerJobDoneHandler fpDone,        // job done callback
                                       WorkerJobContHandler fpHndCntxt,    // handle context mem
                                       object context                      // context
                                     )
        {
            if ( jg == null || _RecGrps == null || !_RecGrps.Contains( jg ) )
                return null;
            ProcessingWokDispatcher dp = GetSameDispatcher( strDispatcher );
            if ( dp == null )
                return null;

            ProcessingJob ajob = jg.NewJob( nm, fpExec, fpDone, fpHndCntxt, context, dp, new WorkerRmRegJobHandler( this.RmvJob ) );
            if ( ajob != null ) RegJob( ajob );

            return ajob;
        }

        public void DelJob( ProcessingJobGroup jg, ProcessingJob job )
        {
            if ( jg != null && job != null )
            {
                jg.RmvJobFromGroup( job );
            }
        }

        #endregion

        #region >>> Dedicated Worker Mgr <<<
        private void RecycleDWs()
        {
            foreach ( var d in DWs ) d.Dispose();
            DWs.Clear();
        }
        public void RemoveDW( object worker )
        {
            if ( _bDisposing || _bDisposed )
                return;

            Monitor.Enter( _hSyncDW );
            try
            {
                DWs.Remove( worker as DedicatedWorker );
            }
            finally { Monitor.Exit( _hSyncDW ); }
        }
        public bool AddDW( object worker )
        {
            if ( _bDisposing || _bDisposed || !( worker is DedicatedWorker dw ) || dw == null )
                return false;
            Monitor.Enter( _hSyncDW );
            try
            {
                DWs.Add( dw );
            }
            finally { Monitor.Exit( _hSyncDW ); }
            return true;
        }
        public IDisposable NewDW(object initCtx, Action<object> initCtxHandler,
            DedicatedWorkBeginHandler beg,
            DedicatedWorkNextHandler next,
            DedicatedWorkExecHandler exec,
            DedicatedWorkStopHandler stop = null)
        {
            if ( _bDisposing || _bDisposed || beg == null || next == null || exec == null )
                return null;
            DedicatedWorker w = new DedicatedWorker( this, initCtx, initCtxHandler )
            {
                fBegin = beg,
                fNext = next,
                fExec = exec,
                fStop = stop
            };
            if ( !w.Start() )
            {
                w.Dispose();
                return null;
            }

            Monitor.Enter( _hSyncDW );
            try { DWs.Add( w ); }
            finally { Monitor.Exit( _hSyncDW ); };

            return w;
        }
        #endregion

        #region >>> Utility <<<

        public static void DelayAWhile( Int32 nTimeout )
        {
            DateTime tmBeg = DateTime.Now;
            while ( true )
            {
                DateTime tmCurr = DateTime.Now;
                TimeSpan diff = tmCurr - tmBeg;
                if ( diff.Milliseconds >= nTimeout )
                    break;

                Application.DoEvents();
            }
        }

        #endregion
    }
}