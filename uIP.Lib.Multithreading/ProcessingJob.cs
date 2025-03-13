using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uIP.Lib.Multithreading
{
    // Remark
    //  1. Init by sigle thread
    //  2. Once the job attached to a thread, we must wait till done.
    //  3. If we want to control the job, we must handle from the source ( manager/ dispatcher ).

    public class ProcessingJob : IDisposable
    {
        protected object _hSync;
        protected string _Name;
        protected WorkerExecJobHandler _fpExec;
        protected WorkerJobDoneHandler _fpDone;
        protected WorkerJobContHandler _fpHandleContext;
        protected object _Context;
//        protected WorkerJobState _State;

        protected ProcessingWokDispatcher _MTDispatcherAttached; // attach which multi-thread processing pool
        protected ProcessingJobGroup _JobGroup;                  // belong to which job group

        protected ProcessingWorkerItem _AttachedThread;          // current job exec by which thread

        protected bool _bStop = false;                           // early stop exec
        protected bool _bDisposing = false;
        protected bool _bJobDoneCall = false;

        protected WorkerRmRegJobHandler _fpRmReg = null;
        protected WorkerRmJobFromJobGroupHandler _fpRmFromGroup = null;

        protected bool _bInvalid = false;
//        public object hSync
//        {
//            get { return _hSync; }
//        }

        public string Name
        {
            get { return _Name; }
            set { _Name = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); }
        }

        public WorkerExecJobHandler OnExec
        {
            set { if ( _AttachedThread == null ) _fpExec = value; }
            get { return _fpExec; }
        }

        public WorkerJobDoneHandler OnDone
        {
            get { return _fpDone; }
            set { if ( _AttachedThread == null ) _fpDone = value; }
        }

        public WorkerJobContHandler OnHandleContext
        {
            get { return _fpHandleContext; }
            set { if ( _AttachedThread == null ) _fpHandleContext = value; }
        }

        public object Context
        {
            set { if ( _AttachedThread == null ) _Context = value; }
            get { return _Context; }
        }

//        public WorkerJobState State
//        {
//            get { return _State; }
//            set { _State = value; }
//        }

        public bool IsInvalid
        {
            get { return _bInvalid; }
            set
            {
                Monitor.Enter( _hSync );
                try { _bInvalid = value; }
                finally { Monitor.Exit( _hSync ); }
            }
        }

        public ProcessingWokDispatcher MTDispatcher
        {
            get { return _MTDispatcherAttached; }
            internal set { _MTDispatcherAttached = value; }
        }

        public ProcessingJobGroup JobGroup
        {
            get { return _JobGroup; }
//            set { _JobGroup = value; }
        }

        public ProcessingWorkerItem AttachedThread
        {
            get { return _AttachedThread; }
            set
            {
                Monitor.Enter( _hSync );
                try { _AttachedThread = value; }
                finally { Monitor.Exit( _hSync ); }
            }
        }

        public bool Stop
        {
            get { return _bStop; }
            set { _bStop = value; }
        }

        public bool Disposing
        {
            get
            {
                bool ret = false;
                //Monitor.Exit( _hSync );
                Monitor.Enter( _hSync );
                try
                {
                    ret = _bDisposing;
                }
                finally { Monitor.Exit( _hSync ); }

                return ret;
            }
        }

        public WorkerRmRegJobHandler RmRegHandler
        {
            get { return _fpRmReg; }
            set
            {
                Monitor.Enter( _hSync );
                try { _fpRmReg = value; }
                finally { Monitor.Exit( _hSync ); }
            }
        }

        public WorkerRmJobFromJobGroupHandler RmJobHandler
        {
            get { return _fpRmFromGroup; }
            set
            {
                Monitor.Enter( _hSync );
                try { _fpRmFromGroup = value; }
                finally { Monitor.Exit( _hSync ); }
            }
        }

        public ProcessingJob()
        {
            _hSync = new object();

            _Name = null;
            _fpExec = null;
            _fpDone = null;
            _fpHandleContext = null;
            _Context = null;

//            _State = WorkerJobState.NA;

            _MTDispatcherAttached = null;
            _JobGroup = null;

        }

        public ProcessingJob( ProcessingJobGroup jg )
        {
            _hSync = new object();

            _Name = null;
            _fpExec = null;
            _fpDone = null;
            _fpHandleContext = null;
            _Context = null;

//            _State = WorkerJobState.NA;

            _MTDispatcherAttached = null;
            _JobGroup = jg;
        }

        public ProcessingJob( ProcessingJobGroup jg, WorkerRmRegJobHandler rmv )
        {
            _hSync = new object();

            _Name = null;
            _fpExec = null;
            _fpDone = null;
            _fpHandleContext = null;
            _Context = null;

//            _State = WorkerJobState.NA;

            _MTDispatcherAttached = null;
            _JobGroup = jg;

            _fpRmReg = rmv;
        }

        public ProcessingJob( string nm, 
                                WorkerExecJobHandler delegExec, 
                                WorkerJobDoneHandler delegDone, 
                                WorkerJobContHandler delegCont,
                                object context,
                                ProcessingWokDispatcher whichMT,
                                ProcessingJobGroup jg,
                                WorkerRmRegJobHandler rmReg,
                                WorkerRmJobFromJobGroupHandler rmJob )
        {
            _hSync = new object();

            _Name = nm;
            _fpExec = delegExec;
            _fpDone = delegDone;
            _fpHandleContext = delegCont;
            _Context = context;

//            _State = WorkerJobState.Init;

            _MTDispatcherAttached = whichMT;
            _JobGroup = jg;

            _fpRmReg = rmReg;
            _fpRmFromGroup = rmJob;
        }

        public void Init( string nm,
                          WorkerExecJobHandler delegExec,
                          WorkerJobDoneHandler delegDone,
                          WorkerJobContHandler delegCont,
                          object context,
                          ProcessingWokDispatcher whichMT,
                          ProcessingJobGroup jg,
                          WorkerRmJobFromJobGroupHandler rmJob )
        {
            Monitor.Enter( _hSync );
            try
            {
                _Name = nm;
                _fpExec = delegExec;
                _fpDone = delegDone;
                _fpHandleContext = delegCont;
                _Context = context;

                //_State = WorkerJobState.Init;

                _MTDispatcherAttached = whichMT;
                _JobGroup = jg;

                _fpRmFromGroup = rmJob;
            }
            finally { Monitor.Exit( _hSync ); }
        }

        public virtual void Dispose()
        {
            if ( _bDisposing )
                return;
            if ( _MTDispatcherAttached != null )
                throw new Exception( "ProcessingJob has attached to a dispatcher" );

            object context = null;
            WorkerJobContHandler fp = null;

            Monitor.Enter( _hSync );
            try
            {
                if ( _bDisposing )
                    return;
                _bDisposing = true;
                _bStop = true;

                context = _Context;
                fp = _fpHandleContext;

                _fpHandleContext = null;
                _Context = null;
            }
            finally { Monitor.Exit( _hSync ); }

            // free context
            if ( fp != null )
                fp( context );

            // remove reg info from somewhere
            if ( _fpRmReg != null )
                _fpRmReg( this );
        }

        public virtual void ExecJobDone()
        {
            if ( _bJobDoneCall )
                return;

            object context = null;
            WorkerJobContHandler fp = null;

            Monitor.Enter( _hSync );
            try
            {
                _bJobDoneCall = true;

                context = _Context;
                fp = _fpHandleContext;

                _fpHandleContext = null;
                _Context = null;
            }
            finally { Monitor.Exit( _hSync ); }

            // free context data
            if ( fp != null )
                fp( context );

            // remove job from its owner group
            if ( _fpRmFromGroup != null )
                _fpRmFromGroup( this );

            // remove reg info from somewhere
            if ( _fpRmReg != null )
                _fpRmReg( this );
        }
    }

}
