using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace uIP.Lib.Multithreading
{
    public delegate void ProcWorkExecDoneHandler();

    public delegate bool WorkerExecJobHandler( string threadNm, string jobNm, object context );
    public delegate void WorkerJobDoneHandler( object context );
    public delegate void WorkerJobContHandler( object context );

    public delegate ProcessingJobGroup WorkerNextJobGroupHandler( ProcessingJobGroup curr );

    public enum WorkerState : int
    {
        Create = 0,
        EnteringExecMethod,
        Idle,
        ExecutingJob,
        ExitExecMethod,
    }

    public enum WorkerJobState : int
    {
        NA = 0,
        Init,
        Exec,
        Done,
        ErrDone,
    }

    public enum WorkerAddJobRetCode : int
    {
        OK = 0,
        ResErr,
        Full,
        Timeout,
        JobErr,
    }

    public delegate void WorkerRmJobFromJobGroupHandler( ProcessingJob job );

    public delegate void WorkerRegJobGroupHandler( ProcessingJobGroup jg );
    public delegate void WorkerRmRegJobGroupHandler( ProcessingJobGroup jg );

    public delegate void WorkerRegJobHandler( ProcessingJob job );
    public delegate void WorkerRmRegJobHandler( ProcessingJob job );

    public delegate void WorkerLogNorHandler( string msg );
    public delegate void WorkerLogWrnHandler( Int32 lvl, string msg );

    public delegate bool WorkerRepoJobsDoneHandler();
    public interface IWorkerPoolAllDone
    {
        UInt32 AddRepoDelegate( WorkerRepoJobsDoneHandler fp );
        void RemoveRepoDelegate( UInt32 id );
        void ClearRepoDelegates();
        bool IsAllDone();
    }
    public class WorkerRepoJobsDoneHandlerMgr
    {
        internal class StoreHandlerInfo
        {
            internal UInt32 _nSN = 0;
            internal WorkerRepoJobsDoneHandler _fp = null;

            internal StoreHandlerInfo() { }
            internal StoreHandlerInfo( UInt32 sn, WorkerRepoJobsDoneHandler fp ) { _nSN = sn; _fp = fp; }
        }

        private object m_sync = new object();
        private UInt32 m_nSN = 1;
        private List<StoreHandlerInfo> m_List = new List<StoreHandlerInfo>();

        private UInt32 GetSn()
        {
            UInt32 ret = m_nSN;
            m_nSN = m_nSN == UInt32.MaxValue ? 1 : ++m_nSN;
            return ret;
        }

        public WorkerRepoJobsDoneHandlerMgr() { }

        public UInt32 Add( WorkerRepoJobsDoneHandler fp, bool bsync )
        {
            if ( m_List == null || fp == null ) return 0;

            bool bSynced = false;
            UInt32 ret = 0;

            if ( bsync )
            {
                Monitor.Enter( m_sync );
                bSynced = true;
            }

            try
            {
                bool got = false;
                for ( int i = 0 ; i < m_List.Count ; i++ )
                {
                    if ( m_List[ i ] == null ) continue;
                    if ( m_List[ i ]._fp == fp )
                    {
                        ret = m_List[ i ]._nSN; got = true; break;
                    }
                }

                if ( !got )
                {
                    m_List.Add( new StoreHandlerInfo( GetSn(), fp ) );
                }
            }
            finally
            {
                if ( bSynced ) Monitor.Exit( m_sync );
            }

            return ret;
        }

        public void Remove( UInt32 sn, bool bsync )
        {
            if ( m_List == null || m_List.Count <= 0 ) return;

            bool synced = false;

            if ( bsync )
            {
                Monitor.Enter( m_sync );
                synced = true;
            }

            try
            {
                for ( int i = 0 ; i < m_List.Count ; i++ )
                {
                    if ( m_List[ i ] == null ) continue;
                    if ( m_List[ i ]._nSN == sn )
                    {
                        m_List.RemoveAt( i ); break;
                    }
                }
            }
            finally
            {
                if ( synced ) Monitor.Exit( m_sync );
            }
        }

        public void Remove( WorkerRepoJobsDoneHandler fp, bool bsync )
        {
            if ( m_List == null || m_List.Count <= 0 ) return;

            bool synced = false;

            if ( bsync )
            {
                Monitor.Enter( m_sync );
                synced = true;
            }

            try
            {
                for ( int i = 0 ; i < m_List.Count ; i++ )
                {
                    if ( m_List[ i ] == null ) continue;
                    if ( m_List[ i ]._fp == fp )
                    {
                        m_List.RemoveAt( i ); break;
                    }
                }
            }
            finally
            {
                if ( synced ) Monitor.Exit( m_sync );
            }
        }

        public void Reset( bool bsync )
        {
            if ( m_List == null || m_List.Count <= 0 ) return;
            
            bool synced = false;

            if ( bsync )
            {
                Monitor.Enter( m_sync );
                synced = true;
            }

            try
            {
                for ( int i = 0 ; i < m_List.Count ; i++ )
                {
                    if ( m_List[ i ] == null ) continue;
                    m_List[ i ]._fp = null;
                    m_List[ i ] = null;
                }
                m_List.Clear();
            }
            finally
            {
                if ( synced ) Monitor.Exit( m_sync );
            }
        }

        public bool IsAllDone()
        {
            if ( m_List == null || m_List.Count <= 0 ) return true;

            for ( int i = 0 ; i < m_List.Count ; i++ )
            {
                if ( m_List[ i ] == null )
                    return false;

                if ( m_List[ i ]._fp == null )
                    continue;
                if ( !m_List[ i ]._fp() )
                    return false;
            }

            return true;
        }
    }
}
