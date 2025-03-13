using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uIP.Lib.Multithreading
{
    /// <summary>
    /// Manage the thread pool
    /// </summary>
    public class ProcessingWorkerPool : IDisposable
    {
        protected bool _bDisposed;
        protected bool _bDisposing;

        //protected object _SyncObj;
        protected Semaphore _BinSem;
        protected int _nAllocatedCount;
        protected List<ProcessingWorkerItem> _BackupList;
        protected List<ProcessingWorkerItem> _DieList;
        protected Queue<ProcessingWorkerItem> _PoolList;
        protected ManualResetEvent _UpdateEvt;

        protected string _strIdName;

        #region >>> Property <<<

        public int AllocCount
        {
            get { return _nAllocatedCount; }
        }

        public ManualResetEvent Event
        {
            get { return _UpdateEvt; }
        }

        public int AvailableCount
        {
            get { return ( _PoolList == null ? 0 : _PoolList.Count ); }
        }

        public string IDName
        {
            get { return _strIdName; }
            set { _strIdName = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); }
        }

        #endregion

        public ProcessingWorkerPool( int count, string idName )
        {
            _bDisposed = false;
            _bDisposing = false;

            //_SyncObj = null;
            _BinSem = null;
            _nAllocatedCount = 0;

            _strIdName = String.IsNullOrEmpty( idName ) ? null : String.Copy( idName );

            if ( count <= 0 )
            {
                _BackupList = null;
                _PoolList = null;
                _UpdateEvt = null;
                _DieList = null;
                return;
            }

            //_SyncObj = new object();
            _BinSem = new Semaphore( 1, 1 );
            _BackupList = new List<ProcessingWorkerItem>( count );
            _DieList = new List<ProcessingWorkerItem>();
            _PoolList = new Queue<ProcessingWorkerItem>( count );
            _UpdateEvt = new ManualResetEvent( false );

            ProcessingWorkerItem thread = null;
            for ( int i = 0; i < count; i++ )
            {
                thread = new ProcessingWorkerItem( this, _UpdateEvt, String.Format( "{0}/{1}", String.IsNullOrEmpty( _strIdName ) ? "" : _strIdName, i + 1 ) );
                if ( thread == null )
                    continue;

                thread.IsBack2Owner = true;
                _BackupList.Add( thread );
                _PoolList.Enqueue( thread );
            }
            _nAllocatedCount = _BackupList.Count;

            bool allDone = false;
            for ( ; ; )
            {
                allDone = true;
                for ( int i = 0; i < _BackupList.Count; i++ )
                {
                    if ( _BackupList[ i ] == null )
                        continue;

                    if ( _BackupList[ i ].State != WorkerState.EnteringExecMethod )
                    {
                        allDone = false;
                        break;
                    }
                }
                if ( allDone )
                    break;

                Thread.Sleep( 0 );
            }

        }

        ~ProcessingWorkerPool()
        {
            Dispose( false );
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
                if ( _DieList != null )
                {
                    _DieList.Clear();
                    _DieList = null;
                }

                if ( _PoolList != null )
                {
                    _PoolList.Clear();
                    _PoolList = null;
                }

                if ( _BackupList != null )
                {
                    for ( int i = 0; i < _BackupList.Count; i++ )
                    {
                        if ( _BackupList[ i ] != null )
                        {
                            _BackupList[ i ].Dispose();
                            _BackupList[ i ] = null;
                        }
                    }

                    _BackupList.Clear();
                    _BackupList = null;
                }

                if ( _BinSem != null )
                {
                    _BinSem.Close();
                    _BinSem = null;
                }
                if ( _UpdateEvt != null )
                {
                    _UpdateEvt.Close();
                    _UpdateEvt = null;
                }
            }
            _bDisposed = true;
            _bDisposing = false;
        }

        #endregion

        public virtual bool IsWholeJobDone()
        {
            if ( _nAllocatedCount <= 0 )
                return true;

            int cnt = _PoolList.Count + _DieList.Count;

            if ( cnt == _BackupList.Count )
                return true;

            return false;
        }

        public virtual ProcessingWorkerItem Dequeue( int waitms )
        {
            //if ( _PoolList == null || _SyncObj == null )
            if ( _PoolList == null || _BinSem == null )
                return null;
            if ( _bDisposing || _bDisposed )
                return null;

            bool status = _BinSem.WaitOne( waitms, true );
            if ( !status )
                return null;

            ProcessingWorkerItem thread = null;
            try
            {
                if ( _PoolList.Count > 0 )
                {
                    thread = _PoolList.Dequeue();
                    if ( thread != null )
                        thread.IsBack2Owner = false;
                }
            }
            finally
            {
                _BinSem.Release();
            }

            return thread;
        }

        public virtual ProcessingWorkerItem Dequeue()
        {
            //if ( _PoolList == null || _SyncObj == null )
            if ( _PoolList == null || _BinSem == null )
                return null;
            if ( _bDisposing || _bDisposed )
                return null;

            bool status = _BinSem.WaitOne();
            if ( !status )
                return null;

            ProcessingWorkerItem thread = null;

            try
            {
                if ( _PoolList.Count > 0 )
                {
                    thread = _PoolList.Dequeue();
                    if ( thread != null )
                        thread.IsBack2Owner = false;
                }
            }
            finally
            {
                _BinSem.Release();
            }

            return thread;
        }


        public virtual void Enqueue( ProcessingWorkerItem thread )
        {
            if ( _bDisposed || _bDisposing )
                return;

            //if ( thread == null || _SyncObj == null )
            if ( thread == null || _BinSem == null )
                return;

            bool status = _BinSem.WaitOne();
            if ( !status )
                return;

            try
            {
                if ( thread.State == WorkerState.ExitExecMethod )
                {
                    if ( !_DieList.Contains( thread ) )
                    {
                        thread.IsBack2Owner = true;
                        _DieList.Add( thread );
                    }
                }
                else
                {
                    if ( !_PoolList.Contains( thread ) )
                    {
                        thread.IsBack2Owner = true;
                        _PoolList.Enqueue( thread );
                        _UpdateEvt.Set();
                    }
                }
            }
            finally
            {
                _BinSem.Release();
            }

        }
    }
}
