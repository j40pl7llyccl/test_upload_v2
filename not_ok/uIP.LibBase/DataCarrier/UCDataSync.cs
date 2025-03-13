using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.DataCarrier
{
    public abstract class UCDataSync<T> : IDisposable
    {
        private bool m_bDisposing = false;
        private bool m_bDisposed = false;
        public bool IsDispose { get { return m_bDisposing || m_bDisposed; } }

        protected object _hSyncOp = new object();
        //protected string _pStrGivenName = null;
        protected string _strMappingFileName = null;
        protected string _strMutexName = null;

        //protected T[] _pData = null;
        protected Int64 _nData = 0;
        protected Int32 _nSizeofDataType = 0;

        protected Dictionary<string, TUCDataItemInfo> _mapNamedItems = null;

        public string SharedMemName { get { return _strMappingFileName; } }
        public string MuxName { get { return _strMutexName; } }
        public Int64 NumOf { get { return _nData; } }
        public Int32 SizeOfT { get { return _nSizeofDataType; } }
        public Int32 NumofNamedMap { get { return _mapNamedItems == null || m_bDisposing || m_bDisposed ? 0 : _mapNamedItems.Count; } }
        public Dictionary<string, TUCDataItemInfo> NamedItems {  get { return _mapNamedItems; } }
        public string TypeName {  get { return typeof( T ).FullName; } }

        public UCDataSync()
        {
            _nSizeofDataType = Marshal.SizeOf( typeof( T ) );
            _mapNamedItems = new Dictionary<string, TUCDataItemInfo>();
        }

        public void Dispose()
        {
            if ( m_bDisposed || m_bDisposing )
                return;
            m_bDisposing = true;

            Monitor.Enter( _hSyncOp );
            try { Dispose( true ); } finally { Monitor.Exit( _hSyncOp ); }

            m_bDisposed = true;
            m_bDisposing = false;
        }

        protected virtual void Dispose( bool disposing )
        {
        }

        public abstract bool Initialize( string pMapFilename, string pMuxName, Int64 nEleCount );

        public abstract T[] Get( out Int64 nSize );
        public abstract bool Get( Int64 index, ref T val );
        public abstract IntPtr GetAddr( Int64 index );
        public abstract bool Set( Int64 index, T val, UInt32 nTimeout = (UInt32)WAIT.INFINITE );
        public abstract bool Set( Int64[] indexes, T[] vals, UInt32 nTimeout = ( UInt32 ) WAIT.INFINITE );
        public void ClearNamedMap()
        {
            if ( m_bDisposing || m_bDisposed ) return;
            Monitor.Enter( _hSyncOp ); try { _mapNamedItems.Clear(); } finally { Monitor.Exit( _hSyncOp ); }
        }
        public string GetNamedMapItem( UInt32 index_0, out TUCDataItemInfo retInfo )
        {
            retInfo = new TUCDataItemInfo();

            if ( _mapNamedItems == null || m_bDisposing || m_bDisposed )
                return null;

            if ( _mapNamedItems.Count <= 0 || ( int ) index_0 >= _mapNamedItems.Count )
                return null;

            Monitor.Enter( _hSyncOp );
            try {
                int i = 0;
                foreach ( KeyValuePair<string, TUCDataItemInfo> kv in _mapNamedItems ) {
                    if ( i == index_0 ) {
                        retInfo = TUCDataItemInfo.MakeNewOne( kv.Value );
                        return kv.Key;
                    }
                    i++;
                }
                return null;
            } finally { Monitor.Exit( _hSyncOp ); }
        }
        public abstract void RemoveNamedMap( string pName );
        public bool SetNamedMap( string pName, Int64 nOffset, Int64 nSize )
        {
            if ( String.IsNullOrEmpty( pName ) || m_bDisposing || m_bDisposed )
                return false;
            if ( nOffset < 0 || nSize <= 0 )
                return false;

            Int64 tmpMin = nOffset;
            Int64 tmpMax = nOffset + nSize - 1;

            Monitor.Enter( _hSyncOp );
            try {
                foreach ( KeyValuePair<string, TUCDataItemInfo> kv in _mapNamedItems ) {
                    if ( kv.Key == pName )
                        continue;

                    Int64 curMin = kv.Value._nOffset;
                    Int64 curMax = kv.Value._nOffset + kv.Value._nSize - 1;
                    // check if overlapped range
                    if ( ( tmpMin >= curMin && tmpMin <= curMax ) || ( tmpMax >= curMin && tmpMax <= curMax ) ) {
                        return false;
                    }
                }

                TUCDataItemInfo info = new TUCDataItemInfo( nOffset, nSize );
                _mapNamedItems[ pName ] = info;
                if ( _mapNamedItems.ContainsKey( pName ) ) {
                    _mapNamedItems[ pName ] = info;
                } else {
                    _mapNamedItems.Add( pName, info );
                }
                return true;
            } finally { Monitor.Exit( _hSyncOp ); }
        }
        public bool QueryFromNamedMap( string pName, out TUCDataItemInfo retInfo )
        {
            Monitor.Enter( _hSyncOp );
            try {
                retInfo = new TUCDataItemInfo();
                if ( m_bDisposed || m_bDisposing || String.IsNullOrEmpty( pName ) )
                    return false;
                if ( !_mapNamedItems.ContainsKey( pName ) )
                    return false;
                retInfo = TUCDataItemInfo.MakeNewOne( _mapNamedItems[ pName ] );
                return true;
            }finally { Monitor.Exit( _hSyncOp ); }
        }
        public bool CheckIndexInNamedMap( Int64 index )
        {
            Monitor.Enter( _hSyncOp );
            try {
                if ( m_bDisposing || m_bDisposed )
                    return false;
                foreach ( KeyValuePair<string, TUCDataItemInfo> kv in _mapNamedItems ) {
                    if ( index >= kv.Value._nOffset && index < ( kv.Value._nOffset + kv.Value._nSize ) )
                        return true;
                }
                return false;
            }finally {
                Monitor.Exit( _hSyncOp );
            }
        }
        public abstract T[] GetFromNamedMapArray( string pName );
        public abstract bool SetFromNamedMap(string pName, T[] pDat2Put, int dstOffset, bool sync);
    }
}
