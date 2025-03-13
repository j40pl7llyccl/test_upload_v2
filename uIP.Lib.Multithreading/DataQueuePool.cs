using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uIP.Lib.Multithreading
{

    /// <summary>
    /// Remark:
    ///     Cannot using index base for a pool item as quering index, cause an item will auto-adjust
    ///     in intelligent mode.
    /// </summary>
    public class DataQueuePool<T> : IDisposable
    {
        protected List<DataQueueItem<T>> _BackupList;
        protected Queue<DataQueueItem<T>> _PoolQ;
        //protected Object _Sync;
        protected Semaphore _Sync;

        protected bool _bDisposed;
        protected bool _bDisposing;

        static protected WorkerLogNorHandler _LogNor = null;
        static protected WorkerLogWrnHandler _LogWrn = null;

        //
        // Intelligent adjustment size
        //
        // 使用 Thread 來做重新配置
        //protected bool _bThreadingEnd = false;
        //protected Thread _ReallocT;
        //protected ManualResetEvent _NotifyEmptyEvt;
        //protected Int32 _nThreadWakeupTm;
        protected bool _bAutoAdj = false;
        // parameters
        protected Int32 _nLowerBound; // User initial count
        protected Int32 _nUpperBound;
        protected double _dfReallocScale; // 每次增長的倍數
        protected Int32 _nEmptyThreshold; // 單位時間內偵測到 pool 空的水位值, 大於此數值就會增長數量
        protected Int32 _nEmptyDetectionDuration;  // 偵測 pool 變空的單位時間
        protected Int32 _nFullDetectionDuration; // 偵測 pool 一直維持在無人存取的時間
        // runtime update
        protected Int32 _nLastAccessTick;
        protected Int32 _nEmptyBegTick;
        protected Int32 _nEmptyCount;
        protected Int32 _nFullRemainedDecCount;
        //protected bool _bRealloc;

        public string _strGivenName = null;

        #region >>> Property <<<

        public int Size
        {
            get { return ( ( _bDisposed || _bDisposing || _BackupList == null ) ? 0 : _BackupList.Count ); }
        }

        public int FreeCount
        {
            get { return ( ( _bDisposed || _bDisposing || _PoolQ == null ) ? 0 : _PoolQ.Count ); }
        }

        public Int32 UpperBound
        {
            get { return _nUpperBound; }
            set { _nUpperBound = value < _nLowerBound ? ( _nLowerBound * 2 ) : value; if ( _nUpperBound <= 0 ) _nUpperBound = 2; }
        }

        public double Scale
        {
            get { return _dfReallocScale; }
            set { _dfReallocScale = value <= 0 ? 1.0 : value; }
        }

        public Int32 EmptyCountThreshold
        {
            get { return _nEmptyThreshold; }
            set { _nEmptyThreshold = value < 0 ? 0 : value; }
        }

        public Int32 EmptyDetectionTime
        {
            get { return _nEmptyDetectionDuration; }
            set { _nEmptyDetectionDuration = value < 1 ? 1 : value; }
        }

        public Int32 FullDetectionTime
        {
            get { return _nFullDetectionDuration; }
            set { _nFullDetectionDuration = value < 1 ? 1 : value; }
        }

        public List<DataQueueItem<T>> Items { get { return _BackupList; } }

        #endregion

        public DataQueuePool( int count, bool bIntelligentMgr )
        {
            _BackupList = null;
            _PoolQ = null;
            //_Sync = new object();
            _Sync = new Semaphore( 1, 1 );

            _bDisposed = false;
            _bDisposing = false;

            _BackupList = new List<DataQueueItem<T>>();
            _PoolQ = new Queue<DataQueueItem<T>>();

            if ( count > 0 )
            {
                DataQueueItem<T> obj = null;
                for ( int i = 0 ; i < count ; i++ )
                {
                    obj = new DataQueueItem<T>();
                    if ( obj == null )
                        continue;

                    obj.Owner = this;
                    _BackupList.Add( obj );
                    _PoolQ.Enqueue( obj );
                }
            }

            //
            // Intelligent pool manage
            //
            //_ReallocT = new Thread( new ParameterizedThreadStart( PoolControl ) );
            //_NotifyEmptyEvt = new ManualResetEvent( false );
            _bAutoAdj = bIntelligentMgr;
            // parameters
            //_nThreadWakeupTm = 4000; // 4-sec
            _nLowerBound = count <= 0 ? 0 : count; // User initial count
            _nUpperBound = 1000;
            _dfReallocScale = 0.2; // 每次增長的倍數
            _nEmptyThreshold = 10; // 單位時間內偵測到 pool 空的水位值, 大於此數值就會增長數量
            _nEmptyDetectionDuration = 500;  // 偵測 pool 變空的單位時間(ms)
            _nFullDetectionDuration = 30000; // 偵測 pool 一直維持在無人存取的時間
            // runtime update
            _nLastAccessTick = Environment.TickCount & Int32.MaxValue;
            _nEmptyBegTick = -1;
            _nEmptyCount = 0;
            _nFullRemainedDecCount = 0;
            //_bRealloc = false;        
            //_ReallocT.Start(); 

            _strGivenName = String.Format("QueueBasePool_{0}", typeof( T ).ToString());
        }

        ~DataQueuePool()
        {
            Dispose( false );
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
                if ( _PoolQ != null )
                {
                    _PoolQ.Clear();
                    _PoolQ = null;
                }

                if ( _BackupList != null )
                {
                    for ( int i = 0; i < _BackupList.Count; i++ )
                    {
                        if ( _BackupList[ i ] == null )
                            continue;
                        _BackupList[ i ].Dispose();
                        _BackupList[ i ] = null;
                    }
                    _BackupList.Clear();
                    _BackupList = null;
                }

                if ( _Sync != null )
                {
                    _Sync.Close();
                    _Sync = null;
                }
            }
            _bDisposed = true;

            _bDisposing = false;
        }

        #endregion

        #region >>> Pool Accessing Methods <<<

        public virtual int GetFreeCount()
        {
            return ( ( _bDisposed || _bDisposing || _PoolQ == null ) ? 0 : _PoolQ.Count );
        }

        public virtual void Put( DataQueueItem<T> item )
        {
            if ( _bDisposed || _bDisposing || item == null )
                return;

            bool status = _Sync.WaitOne();

            if ( !status )
            {
                if ( !_bDisposing && !_bDisposed )
                    throw new Exception( String.Format( "{0}/DataQueuePool/Put: Semaphore error!", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName ) );
                return;
            }

            try
            {
                if ( !_PoolQ.Contains( item ) )
                    _PoolQ.Enqueue( item );
                else
                {
#if ( CONSOLE_DBG )
                    //Console.Write( "Same item inside Pool Q!\n" );
#endif
                    //_LogNor(String.Format( "{0}/DataQueuePool/Put: Same item inside Pool Q!", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName ) );
                }
                ProcPoolFull();
            }
            finally
            {
                _nLastAccessTick = Environment.TickCount & Int32.MaxValue;
                _Sync.Release();
            }
        }

        public virtual DataQueueItem<T> Get()
        {
            if ( _bDisposed || _bDisposing )
                return null;

            DataQueueItem<T> item = null;

            bool status = _Sync.WaitOne();
            if ( !status )
                return item;

            try
            {
                if ( _PoolQ.Count > 0 )
                {
                    item = _PoolQ.Dequeue();
                    //ProcPoolFull();
                }
                else
                {
                    ProcPoolEmpty();
                    if ( _PoolQ.Count > 0 )
                        item = _PoolQ.Dequeue();
                }
            }
            finally
            {
                _nLastAccessTick = Environment.TickCount & Int32.MaxValue;
                _Sync.Release();
            }

            return item;
        }

        #endregion

        #region >>> Intelligent Pool Manage <<<

        protected virtual bool AllocateItemData( DataQueueItem<T> itemShell )
        {
            throw new Exception( String.Format( "{0}/DataQueuePool/AllocateItemData: cannot call base function!", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName ) );
        }

        protected virtual void DoAfterThinningPool( )
        {
        }

        // Must call inside critical section.
        protected virtual void ProcPoolEmpty()
        {
            if ( _bDisposed || _bDisposing || !_bAutoAdj )
                return;

            if ( _nEmptyBegTick < 0 )
                _nEmptyBegTick = Environment.TickCount & Int32.MaxValue;

            Int32 presentTm = Environment.TickCount & Int32.MaxValue;
            Int32 elapseTm = presentTm < _nEmptyBegTick ?
                ( Int32.MaxValue - _nEmptyBegTick + presentTm ) :
                ( presentTm - _nEmptyBegTick );

            // Elapse time more than detection time --> reset counter
            if ( elapseTm > _nEmptyDetectionDuration )
            {
                _nEmptyCount = 0;
                _nEmptyBegTick = Environment.TickCount & Int32.MaxValue;
            }

            _nEmptyCount++;

            // Inside a period of time, empty count has already more than threshold.
            // Adjust items.
            if ( _nEmptyCount > _nEmptyThreshold )
            {
                // Add new item.
                int allocCount = _BackupList.Count <= 0 ? 10 : Convert.ToInt32( Convert.ToDouble( _BackupList.Count ) * _dfReallocScale );
                allocCount = allocCount <= 0 ? 1 : allocCount;
                int actAllocCnt = 0;

                // Boundary check. Less than upper bound, allocate.
                if ( _BackupList.Count < _nUpperBound )
                {
#if ( CONSOLE_DBG )
                    Console.Write( "Elapse Time {0}: count empty = {1}\n", elapseTm, _nEmptyCount );
                    Console.Write( "Realloc: before count = {0}, want alloc = {1}\n", _BackupList.Count, allocCount );
#endif
                    if ( _LogWrn != null )
                    {
                        _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolEmpty: Elapse Time {1}: count empty = {2}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, elapseTm, _nEmptyCount ) );
                        _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolEmpty: Realloc: before count = {1}, want alloc = {2}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, _BackupList.Count, allocCount ) );
                    }
                    for ( actAllocCnt = 0; actAllocCnt < allocCount && _BackupList.Count < _nUpperBound ; actAllocCnt++ )
                    {
                        DataQueueItem<T> poolItem = new DataQueueItem<T>();
                        if ( poolItem == null )
                            continue;
                        if ( ! AllocateItemData( poolItem ) )
                        {
                            poolItem.Dispose();
                            continue;
                        }
                        poolItem.Owner = this;
                        _BackupList.Add( poolItem );
                        _PoolQ.Enqueue( poolItem );
                    }
                }
                _nEmptyCount = 0;
                _nEmptyBegTick = Environment.TickCount & Int32.MaxValue;

#if ( CONSOLE_DBG )
                Console.Write( "Realloc: want = {0}, alloc = {1}, free count = {2}, pool count = {3}\n", allocCount, actAllocCnt, _PoolQ.Count, _BackupList.Count );
#endif
                if ( _LogWrn != null ) _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolEmpty: Realloc: want = {1}, alloc = {2}, free count = {3}, pool count = {4}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, allocCount, actAllocCnt, _PoolQ.Count, _BackupList.Count ) );
            }
        }

        protected virtual void ProcPoolFull( )
        {
            if ( _bDisposed || _bDisposing || !_bAutoAdj )
                return;

            Int32 presentTm = Environment.TickCount & Int32.MaxValue;
            Int32 elapseTm = presentTm < _nLastAccessTick ?
                ( Int32.MaxValue - _nLastAccessTick + presentTm ) :
                ( presentTm - _nLastAccessTick );

            if ( elapseTm <= _nFullDetectionDuration )
            {
                if ( _nFullRemainedDecCount > 0 && _PoolQ.Count > 0 )
                {
#if ( CONSOLE_DBG )
                    Console.Write( "--1--Free: before -- remained count = {0}, pool free = {1}, pool total = {2}\n", _nFullRemainedDecCount, _PoolQ.Count, _BackupList.Count );
#endif
                    if ( _LogWrn != null ) _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolFull: --1--Free: before -- remained count = {1}, pool free = {2}, pool total = {3}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, _nFullRemainedDecCount, _PoolQ.Count, _BackupList.Count ) );

                    for ( ; ; )
                    {
                        // Get an item from a queue.
                        DataQueueItem<T> poolItem = _PoolQ.Dequeue();
                        // Find the item in a list.
                        if ( !_BackupList.Remove( poolItem ) )
                        {
                            throw new Exception( String.Format( "{0}/DataQueuePool/ProcPoolFull: Lost item sync in List and Queue!", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName ) );
                        }

                        poolItem.Dispose();
                        _nFullRemainedDecCount--;

                        if ( _nFullRemainedDecCount <= 0 || _PoolQ.Count <= 0 )
                        {
                            break;
                        }
                        if ( _BackupList.Count <= _nLowerBound )
                        {
                            _nFullRemainedDecCount = 0;
                            break;
                        }
                    }
#if ( CONSOLE_DBG )
                    Console.Write( "--1--Free: after -- remained count = {0}, pool free = {1}, pool total = {2}\n", _nFullRemainedDecCount, _PoolQ.Count, _BackupList.Count );
#endif
                    if ( _LogWrn != null ) _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolFull: --1--Free: after -- remained count = {1}, pool free = {2}, pool total = {3}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, _nFullRemainedDecCount, _PoolQ.Count, _BackupList.Count ) );
                }
                return;
            }

            if ( _BackupList.Count <= _nLowerBound )
            {
                _nFullRemainedDecCount = 0;
                return;
            }

            // No free item
            if ( _PoolQ.Count <= 0 )
                return;

            // Free half diff count each time.
            int diff = _BackupList.Count - _nLowerBound;
            if ( diff <= 0 )
                return;
            int diffHalf = diff <= 1 ? diff : Convert.ToInt32( Convert.ToDouble( diff ) * 0.5 );
            int actualFreeCount = 0;

#if ( CONSOLE_DBG )
            Console.Write( "--2--Free: before -- want free count = {0}, pool free count = {1}, pool total count = {2}\n", diffHalf, _PoolQ.Count, _BackupList.Count );
#endif
            if ( _LogWrn != null ) _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolFull:--2--Free: before -- want free count = {1}, pool free count = {2}, pool total count = {3}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, diffHalf, _PoolQ.Count, _BackupList.Count ) );
            
            for ( int i = 0; i < diffHalf; i++ )
            {
                // Get an item from a queue.
                DataQueueItem<T> poolItem = _PoolQ.Dequeue();
                // Find the item in a list.
                if ( !_BackupList.Remove( poolItem ) )
                {
                    throw new Exception( String.Format( "{0}/DataQueuePool/ProcPoolFull: Lost item sync in List and Queue!", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName ) );
                }

                poolItem.Dispose();
                actualFreeCount++;

                if ( _PoolQ.Count <= 0 || _BackupList.Count <= _nLowerBound )
                {
                    break;
                }
            }

            _nFullRemainedDecCount = diffHalf - actualFreeCount;
            _nFullRemainedDecCount = _nFullRemainedDecCount < 0 ? 0 : _nFullRemainedDecCount;
            if ( _BackupList.Count <= _nLowerBound )
                _nFullRemainedDecCount = 0;

#if ( CONSOLE_DBG )
            Console.Write( "--2--Free: after -- actaul free count = {0}, remained count = {1}, pool free = {2}, pool total = {3}\n", actualFreeCount, _nFullRemainedDecCount, _PoolQ.Count, _BackupList.Count );
#endif
            if ( _LogWrn != null ) _LogWrn( 2, String.Format( "{0}/DataQueuePool/ProcPoolFull:--2--Free: after -- actaul free count = {1}, remained count = {2}, pool free = {3}, pool total = {4}", String.IsNullOrEmpty( _strGivenName ) ? "" : _strGivenName, actualFreeCount, _nFullRemainedDecCount, _PoolQ.Count, _BackupList.Count ) );
            DoAfterThinningPool();
        }

        #endregion
    }
}
