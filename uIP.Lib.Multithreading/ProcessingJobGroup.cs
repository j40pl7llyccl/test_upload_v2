using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace uIP.Lib.Multithreading
{

    public class ProcessingJobGroup : IDisposable
    {
        protected string _strName = null;
        protected object _hSync = null;
        protected List<ProcessingJob> _Jobs = null;
        protected WorkerNextJobGroupHandler _fpNext = null;

        protected WorkerRegJobGroupHandler _fpReg = null;
        protected WorkerRmRegJobGroupHandler _fpRmv = null;

        protected bool _bDisposing = false;

        protected Int32 _nDefWaitMs = 10 * 1000;

        protected bool _bBegDispatching = false;

        static protected WorkerLogNorHandler _LogNor = null;
        static protected WorkerLogWrnHandler _LogWrn = null;

        public string GivenName
        {
            get { return _strName; }
        }

        public WorkerNextJobGroupHandler NextHandler
        {
            get { return _fpNext; }
            set
            {
                Monitor.Enter( _hSync );
                try { _fpNext = value; }
                finally { Monitor.Exit( _hSync ); }
            }
        }

        public WorkerRegJobGroupHandler RegHandler
        {
            get { return _fpReg; }
        }

        public List<ProcessingJob> Jobs
        {
            get { return _Jobs; }
        }

        //public bool IsBegDispatching
        //{
        //    get { return _bBegDispatching; }
        //    set
        //    {
        //        Monitor.Enter( _hSync );
        //        try { _bBegDispatching = value; }
        //        finally { Monitor.Exit( _hSync ); }
        //    }
        //}

        public Int32 nWaitDelayMs
        {
            get { return _nDefWaitMs; }
        }

        public ProcessingJobGroup()
        {
            _hSync = new object();
            _Jobs = new List<ProcessingJob>();
        }

        public ProcessingJobGroup( string nm, WorkerNextJobGroupHandler fpNext )
        {
            _strName = nm;
            _hSync = new object();
            _Jobs = new List<ProcessingJob>();
            _fpNext = fpNext;
        }

        public ProcessingJobGroup( string nm, WorkerNextJobGroupHandler fpNext, WorkerRmRegJobGroupHandler rmv )
        {
            _strName = nm;
            _hSync = new object();
            _Jobs = new List<ProcessingJob>();
            _fpNext = fpNext;

            _fpRmv = rmv;
        }

        public ProcessingJobGroup( string nm, WorkerNextJobGroupHandler fpNext, WorkerRegJobGroupHandler reg, WorkerRmRegJobGroupHandler rmv )
        {
            _strName = nm;
            _hSync = new object();
            _Jobs = new List<ProcessingJob>();
            _fpNext = fpNext;

            _fpReg = reg;
            _fpRmv = rmv;
        }

        protected static void RemoveInvalidJob( List<ProcessingJob> jobs )
        {
            if ( jobs == null ) return;
            jobs.RemoveAll( job => job == null || job.MTDispatcher == null || job.IsInvalid );
            /*
            bool bOut = false;
            while( true )
            {
                if ( jobs.Count <= 0 )
                    break;

                bOut = true;
                for ( int i = 0; i < jobs.Count; i++ )
                {
                    if ( jobs[ i ] == null || jobs[ i ].MTDispatcher == null || jobs[ i ].IsInvalid )
                    {
                        if ( jobs[ i ].MTDispatcher == null ) if ( _LogNor != null ) _LogNor( String.Format( "[MTProcessingJobGroup::RemoveInvalidJob] no thread dispatcher attach to job({0}).", jobs[ i ].Name ) );
                        else if ( jobs[ i ].IsInvalid ) if ( _LogNor != null ) _LogNor( String.Format( "[MTProcessingJobGroup::RemoveInvalidJob] job({0}) invalid.", jobs[ i ].Name ) );

                        jobs[ i ].Dispose();

                        jobs.RemoveAt( i ); bOut = false; break;
                    }
                }

                if ( bOut )
                    break;
            }*/
        }

        //protected static void DispatchJobs( MTProcessingJobGroup jg )
        //{
        //    if ( jg == null || jg._Jobs == null || jg.Jobs.Count <= 0 ) return;

        //    RemoveInvalidJob( jg.Jobs );

        //    for ( int i = 0; i < jg.Jobs.Count; i++ )
        //    {
        //        if ( jg.Jobs[ i ] == null || jg.Jobs[ i ].MTDispatcher == null )
        //        {
        //            continue;
        //        }
        //        // add WorkerRmJobFromJobGroupHandler handler
        //        jg.Jobs[ i ].RmJobHandler = new WorkerRmJobFromJobGroupHandler( jg.RmvJobFromGroup );
        //        // attach to a thread dispatcher
        //        if ( jg.Jobs[ i ].MTDispatcher.AddJob( jg.Jobs[ i ], jg._nDefWaitMs ) != WorkerAddJobRetCode.OK )
        //            jg.Jobs[ i ].IsInvalid = true;
        //    }

        //    RemoveInvalidJob( jg.Jobs );
        //}

        public void DispatchingJobs()
        {
            if ( _bDisposing || _Jobs == null || _Jobs.Count <= 0 )
                return;
            if ( _bBegDispatching )
                return;

            Monitor.Enter( _hSync );
            try
            {
                _bBegDispatching = true;

                // remove invalid job
                RemoveInvalidJob( _Jobs );
                // dispatching
                for ( int i = 0; i < _Jobs.Count; i++ )
                {
                    // check null job
                    if ( _Jobs[ i ] == null )
                        continue;
                    // check dispatcher
                    if ( _Jobs[ i ].MTDispatcher == null )
                    {
                        _Jobs[ i ].IsInvalid = true; continue;
                    }
                    //
                    // add
                    //
                    _Jobs[ i ].RmJobHandler = new WorkerRmJobFromJobGroupHandler( this.RmvJobFromGroup );
                    if ( _Jobs[ i ].MTDispatcher.AddJob( _Jobs[ i ], _nDefWaitMs ) != WorkerAddJobRetCode.OK )
                        _Jobs[ i ].IsInvalid = true;
                    else
                        _Jobs[ i ].IsInvalid = false;
                }
                // remove invalid job
                RemoveInvalidJob( _Jobs );
            }
            finally { Monitor.Exit( _hSync ); }
        }

        public virtual void RmvJobFromGroup( ProcessingJob job )
        {
            if ( job == null || _bDisposing )
                return;

            bool bCallDispose = false;
            bool bCallQNext = false;
            WorkerNextJobGroupHandler fpN = null;
            WorkerRmRegJobGroupHandler fpRm = null;

            Monitor.Enter( _hSync );
            try
            {
                // Remove job from the list
                if ( _Jobs.Contains( job ) )
                    _Jobs.Remove( job );

                // Generate next group
                if ( _Jobs.Count <= 0 )
                {
                    bCallDispose = true;
                    fpRm = _fpRmv;
                    _fpRmv = null;

                    bCallQNext = true;
                    fpN = _fpNext;
                    _fpNext = null;
                }
            }
            finally { Monitor.Exit( _hSync ); }

            // Query next group jobs
            if ( _bBegDispatching && bCallQNext && fpN != null )
            {
                ProcessingJobGroup next = fpN( this );
                if ( next != null )
                {
                    // reg job group
                    if ( next.RegHandler != null ) next.RegHandler( next );
                    // dispatching
                    next.DispatchingJobs();
                }
            }

            // Dispose current
            if ( bCallDispose) 
            {
                // dispose
                Dispose();

                if ( fpRm != null )
                    // remove
                    fpRm( this );
            }
        }

        public virtual bool AddJob( ProcessingJob job )
        {
            if ( job == null || _bDisposing )
                return false;

            bool bRet = true;

            Monitor.Enter( _hSync );
            try
            {
                if ( _bBegDispatching )
                    bRet = false;
                else
                {
                    if ( !_Jobs.Contains( job ) )
                    {
                        job.IsInvalid = false;
                        //job.RmJobHandler = new WorkerRmJobFromJobGroupHandler( this.RmvJobFromGroup );
                        _Jobs.Add( job );
                    }
                }
            }
            finally { Monitor.Exit( _hSync ); }

            return bRet;
        }

        public virtual void BeginDispatchingJobs()
        {
				//if ( _bBegDispatching )
				//    return;

				//Monitor.Enter( _hSync );
				//try
				//{
				//    _bBegDispatching = true;
				//}
				//finally { Monitor.Exit( _hSync ); }

            DispatchingJobs();
        }

        public virtual void Dispose()
        {
            if ( _bDisposing )
                return;

            Monitor.Enter( _hSync );
            try
            {
                _bDisposing = true;

                for ( int i = 0; i < _Jobs.Count; i++ )
                {
                    if ( _Jobs[ i ] == null )
                        continue;
                    _Jobs[ i ].Dispose();
                }

                _Jobs.Clear();
            }
            finally { Monitor.Exit( _hSync ); }

            if ( _fpRmv != null )
                _fpRmv( this );
        }

		 /// <summary>
		 /// Avoid Parent's Job Group List Re-entry
		 /// </summary>
		 /// <param name="isCallRmv"></param>
		 public virtual void Dispose( bool isCallRmv )
		 {
			 if ( _bDisposing )
				 return;

			 Monitor.Enter( _hSync );
			 try
			 {
				 _bDisposing = true;

				 for ( int i = 0; i < _Jobs.Count; i++ )
				 {
					 if ( _Jobs[i] == null )
						 continue;
					 _Jobs[i].Dispose();
				 }

				 _Jobs.Clear();
			 }
			 finally { Monitor.Exit( _hSync ); }

			 if ( isCallRmv == true )
			 {
				 if ( _fpRmv != null )
					 _fpRmv( this );
			 }
		 }

        public virtual ProcessingJob NewJob( string nm,                             // given name
                                               WorkerExecJobHandler delegExec,        // job main function
                                               WorkerJobDoneHandler delegDone,        // job done callback
                                               WorkerJobContHandler delegCont,        // handle context mem
                                               object context,                        // context
                                               ProcessingWokDispatcher whichMT,  // exec by a dispatcher
                                               WorkerRmRegJobHandler rmReg            // remove reg info delegate
                                             )
        {
            ProcessingJob job = new ProcessingJob( nm,
                                                       delegExec,
                                                       delegDone,
                                                       delegCont,
                                                       context,
                                                       whichMT,
                                                       this,
                                                       rmReg,
                                                       null );
            if ( job == null )
                return null;

            if ( !AddJob( job ) )
            {
                job.Dispose();
                job = null;
            }

            return job;
        }
    }

}
