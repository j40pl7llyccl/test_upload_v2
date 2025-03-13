using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uIP.LibBase.Utility;
using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.DataCarrier
{
    public class UCWin32SharedMemFormating : IDisposable
    {
        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;

        protected Int32 _nMaxLenOfName = 0;
        protected Int32 _nMaxNumOfItemsInMainSection = 0;
        protected UCDataSyncW32<byte> _pMainSection = null;
        protected IntPtr[] _ppMainSectionFormated = null;
        protected Int32 _nMainSectionUsed = 0;
        protected Int32 _nMainSectionPerItemSize = 0;

        protected List<TShMemUsedInfo> _vectorShMemUsedRec;
        protected List<TShMemAccItem> _vectorShMemItemsRec;
        protected Dictionary<string, UCDataSyncW32<Int32>> _I32ShMems;
        protected Dictionary<string, UCDataSyncW32<double>> _DoubleShMems;
        protected Dictionary<string, UCDataSyncW32<byte>> _U8ShMems;

        public UCWin32SharedMemFormating()
        {
            _I32ShMems = new Dictionary<string, UCDataSyncW32<Int32>>();
            _DoubleShMems = new Dictionary<string, UCDataSyncW32<double>>();
            _U8ShMems = new Dictionary<string, UCDataSyncW32<byte>>();
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

        protected void Dispose(bool disposing)
        {
            if (_pMainSection != null) {
                _pMainSection.Dispose();
                _pMainSection = null;
            }

            foreach(KeyValuePair<string, UCDataSyncW32<Int32>> kv in _I32ShMems) {
                kv.Value.Dispose();
            }
            _I32ShMems.Clear();

            foreach(KeyValuePair<string, UCDataSyncW32<double>> kv in _DoubleShMems) {
                kv.Value.Dispose();
            }
            _DoubleShMems.Clear();

            foreach(KeyValuePair<string, UCDataSyncW32<byte>> kv in _U8ShMems) {
                kv.Value.Dispose();
            }
            _U8ShMems.Clear();
        }

        protected bool CreatingCheck( string pName )
        {
            if ( m_bDisposing || m_bDisposed )
                return false;
            if ( _nMainSectionUsed >= _nMaxNumOfItemsInMainSection ) // max count exceeding
                return false;
            if ( String.IsNullOrEmpty(pName) ) // invalid shared memory name
                return false;
            // check the same shared memory no matter in which type of vector
            for ( int i = 0; i < _vectorShMemUsedRec.Count; i++ ) {
                TShMemUsedInfo p = _vectorShMemUsedRec[ i ];
                if ( p == null || String.IsNullOrEmpty( p._pShMemName ) ) continue;
                if ( p._pShMemName == pName ) return false;

                string[] strs = CommonUtilities.MySplit( p._pShMemName, UCSHAREDMEM_CONCAT_STR );
                if (strs != null && strs.Length > 0 && strs[0] == pName) {
                    return false;
                }
            }

            return true;
        }

        protected TShMemUsedInfo GetAvailableSharedMemory( Int64 nNeed, string type )
        {
            if ( m_bDisposing || m_bDisposed )
                return null;
            if ( nNeed <= 0 )
                return null;

            for ( int i = 0; i < _vectorShMemUsedRec.Count; i++ ) {
                TShMemUsedInfo p = _vectorShMemUsedRec[ i ];
                if ( p._strShMemType == type && p._nAvailableCount >= nNeed ) {
                    return p;
                }
            }

            return null;
        }

        protected TShMemUsedInfo GetShMemInfo( string pName )
        {
            if ( String.IsNullOrEmpty( pName ) )
                return null;
            // check the same shared memory no matter in which type of vector
            for ( int i = 0; i < _vectorShMemUsedRec.Count; i++ ) {
                TShMemUsedInfo p = _vectorShMemUsedRec[ i ];
                if ( p == null || String.IsNullOrEmpty( p._pShMemName ) ) continue;
                if ( p._pShMemName == pName ) return p;

                string[] strs = CommonUtilities.MySplit( p._pShMemName, UCSHAREDMEM_CONCAT_STR );
                if ( strs != null && strs.Length > 0 && strs[ 0 ] == pName ) {
                    return p;
                }
            }

            return null;
        }

        protected bool CheckSameMapFileName( string pName, int allocSz, out bool bSame, out string ppRetMapFileName, out string ppRetMuxName, out string ppRetOriMuxName )
        {
            bSame = false;
            ppRetMapFileName = null;
            ppRetMuxName = null;
            ppRetOriMuxName = null;

            ppRetOriMuxName = String.Format("{0}{1}", pName, pAttachedMuxName );

            string pMapFileName = String.Format( "{0}{1}", UCDataSyncDecl.UCDATASYNCWIN32_SHMEM_PREFIX, pName );
            IntPtr h = MemWinSdkFunctions.OpenFileMapping( pMapFileName );
            bSame = h != IntPtr.Zero;
            if ( h != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( h );
            if (bSame) {
                string curTmStr = CommonUtilities.GetCurrentTimeStr( "" );
                //pMapFileName = String.Format( "{0}{1}{2}{3}", UCDataSyncDecl.UCDATASYNCWIN32_SHMEM_PREFIX, pName, UCSHAREDMEM_CONCAT_STR, curTmStr );

                //// check again
                //h = MemWinSdkFunctions.OpenFileMapping( pMapFileName );
                //if (h != IntPtr.Zero) {
                //    CommonWinSdkFunctions.CloseHandle( h );
                //    return false;
                //}

                ppRetMapFileName = String.Format( "{0}{1}{2}", pName, UCSHAREDMEM_CONCAT_STR, curTmStr );
                ppRetMuxName = String.Format( "{0}{1}", ppRetMapFileName, pAttachedMuxName );
            }
            return true;
        }


        protected const string pAttachedMuxName = "_MUX";
        protected const string UCSHAREDMEM_CONCAT_STR = "$$";

        virtual public bool Initialize( string pMainSectionName, int nNameMax, int nItemCountMax )
        {
            if ( _pMainSection != null ) return true; // already init
            if ( nNameMax <= 0 || nItemCountMax <= 0 ) return false;
            if ( String.IsNullOrEmpty(pMainSectionName) ) return false;

            string pMuxName = String.Format( "{0}{1}", pMainSectionName, pAttachedMuxName );

            int perItemSize = nNameMax + 1 + 16;
            perItemSize += ( ( perItemSize % 8 ) == 0 ? 0 : ( 8 - ( perItemSize % 8 ) ) ); // make 8-alignment
            _pMainSection = new UCDataSyncW32<byte>();
            if ( !_pMainSection.Initialize( pMainSectionName, pMuxName, nItemCountMax * Convert.ToInt64( perItemSize )) ) {
                _pMainSection.Dispose();
                _pMainSection = null;
                return false;
            }

            _ppMainSectionFormated = new IntPtr[ nItemCountMax ];
            unsafe
            {
                byte* p8 = ( byte* ) _pMainSection.DataBeginAddr.ToPointer();
                for (int i = 0; i < nItemCountMax; i++ ) {
                    byte* pTmp = &p8[ i * perItemSize ];
                    _ppMainSectionFormated[ i ] = new IntPtr( ( void* ) pTmp );
                }
            }

            _nMaxLenOfName = nNameMax;
            _nMaxNumOfItemsInMainSection = nItemCountMax;
            _nMainSectionPerItemSize = perItemSize;
            _nMainSectionUsed = 0;

            return true;
        }

        private TShMemUsedInfo CreateShMem<T>( string pName, int nCount )
        {
            if ( m_bDisposing || m_bDisposed )
                return null;
            if ( typeof( T ) != typeof( Int32 ) && typeof( T ) != typeof( double ) && typeof( T ) != typeof( byte ) )
                return null;

            if ( !CreatingCheck( pName ) )
                return null;
            if ( nCount <= 0 )
                return null;

            // create shared memory
            bool bSame;
            string pNewName;
            string pNewMux;
            string pOriMux;
            if ( !CheckSameMapFileName( pName, _nMainSectionPerItemSize, out bSame, out pNewName, out pNewMux, out pOriMux ) ) {
                return null;
            }

            UCDataSyncW32<T> p = new UCDataSyncW32<T>();
            bool status = p.Initialize( !String.IsNullOrEmpty( pNewName ) ? pNewName : pName, !String.IsNullOrEmpty( pNewName ) ? pNewMux : pOriMux, nCount );
            if ( !status ) {
                p.Dispose();
                p = null;
                return null;
            }

            // Recording
            //_ppMainSectionFormated[_nMainSectionUsed++] = (tYUX_U8 *)p->GivenName();
            if ( typeof( T ) == typeof( Int32 ) )
                _I32ShMems.Add( p.SharedMemName, p as UCDataSyncW32<Int32> ); // add the int32 shared mem
            else if ( typeof( T ) == typeof( double ) )
                _DoubleShMems.Add( p.SharedMemName, p as UCDataSyncW32<double> );
            else
                _U8ShMems.Add( p.SharedMemName, p as UCDataSyncW32<byte> );

            unsafe // write shared mem name
            {
                IntPtr pShName = _ppMainSectionFormated[ _nMainSectionUsed ];
                byte* p8 = ( byte* ) pShName.ToPointer();
                byte[] shName = Encoding.ASCII.GetBytes( p.SharedMemName );
                int i = 0;
                for ( i = 0; i < shName.Length && i < ( _nMainSectionPerItemSize - 1 ); i++ )
                    p8[ i ] = shName[ i ];
                p8[ i ] = 0;
            }
            _nMainSectionUsed++;
            //TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), (int)_vectorI32ShMems.size() - 1, UCSHAREDMEM_I32);
            TShMemUsedInfo pUsedInfo = new TShMemUsedInfo( p.SharedMemName, typeof( T ).FullName );
            pUsedInfo._pItself = pUsedInfo;
            pUsedInfo._pShMemObjInstance = p;
            pUsedInfo._nSizeofT = p.SizeOfT;
            pUsedInfo._pMuxHandle = p.Mutex;
            pUsedInfo._nAvailableCount = p.NumOf;

            _vectorShMemUsedRec.Add( pUsedInfo );
            return pUsedInfo;
        }

        public TShMemUsedInfo CreateI32ShMem( string pName, int nCount )
        {
            return CreateShMem<Int32>( pName, nCount );
        }
        public TShMemUsedInfo CreateDoubleShMem(string pName, int nCount )
        {
            return CreateShMem<double>( pName, nCount );
        }
        public TShMemUsedInfo CreateU8ShMem( string pName, int nCount )
        {
            return CreateShMem<byte>( pName, nCount );
        }

        public bool DeleteShMem(string pName)
        {
            if ( m_bDisposing || m_bDisposed )
                return false;
            TShMemUsedInfo p = GetShMemInfo( pName );
            if ( p == null )
                return false;

            // backup names regardless pName
            List<string> tmpNames = new List<string>();
            for(int i = 0; i < _nMainSectionUsed; i++ ) {
                string str = CommonUtilities.IntptrToAsciiString( _ppMainSectionFormated[ i ] );
                if ( str != pName && !String.IsNullOrEmpty(str) )
                    tmpNames.Add( str );
            }

            // remove item record
            bool bEnd = true;
            while ( true ) {
                bEnd = true;
                for ( int i = 0; i < _vectorShMemItemsRec.Count; i++ ) {
                    if (_vectorShMemItemsRec[i]._pUsedInfoAddr == p) {
                        _vectorShMemItemsRec.RemoveAt( i );
                        bEnd = false; break;
                    }
                }
                if ( bEnd )
                    break;
            }

            // remove the shared mem
            if (p._strShMemType == typeof(Int32).FullName) {
                if (_I32ShMems.ContainsKey(pName)) {
                    UCDataSyncW32<Int32> i32 = _I32ShMems[ pName ];
                    _I32ShMems.Remove( pName );
                    i32.Dispose(); i32 = null;
                }
            } else if(p._strShMemType == typeof(double).FullName) {
                if (_DoubleShMems.ContainsKey(pName)) {
                    UCDataSyncW32<double> df = _DoubleShMems[ pName ];
                    _DoubleShMems.Remove( pName );
                    df.Dispose(); df = null;
                }
            } else if (p._strShMemType == typeof(byte).FullName) {
                if (_U8ShMems.ContainsKey(pName)) {
                    UCDataSyncW32<byte> u8 = _U8ShMems[ pName ];
                    _U8ShMems.Remove( pName );
                    u8.Dispose(); u8 = null;
                }
            }

            // remove record
            for(int i = 0; i < _vectorShMemUsedRec.Count; i++ ) {
                if (_vectorShMemUsedRec[i] == p) {
                    _vectorShMemUsedRec.RemoveAt( i );
                    break;
                }
            }

            // arrange the data
            for(int i = 0; i < tmpNames.Count; i++ ) {
                // put it back
                CommonUtilities.StringByteArrToIntptr( Encoding.ASCII.GetBytes( tmpNames[ i ] ), _ppMainSectionFormated[ i ], _nMainSectionPerItemSize );
            }
            // reset
            for(int i = tmpNames.Count; i < _nMaxNumOfItemsInMainSection; i++ ) {
                unsafe
                {
                    byte* p8 = ( byte* ) _ppMainSectionFormated[ i ].ToPointer();
                    if (p8 != null) {
                        p8[ 0 ] = 0;
                    }
                }
            }
            _nMainSectionUsed = tmpNames.Count;
            return true;
        }

        private bool ItemHandleValid( object h )
        {
            if ( h == null ) return false;
            for(int i = 0; i < _vectorShMemItemsRec.Count; i++ ) {
                if ( _vectorShMemItemsRec[ i ]._pItself == h )
                    return true;
            }
            return false;
        }
        private bool ShMemHandleValid( object h )
        {
            if ( h == null ) return false;
            for (int i = 0; i < _vectorShMemUsedRec.Count; i++ ) {
                if ( _vectorShMemUsedRec[ i ]._pItself == h )
                    return true;
            }
            return false;
        }
        private bool AllocItem<T>(int nNeededItems, out TShMemAccItem pItem )
        {
            pItem = null;
            if ( m_bDisposing || m_bDisposed )
                return false;
            if ( typeof( T ) != typeof( Int32 ) && typeof( T ) != typeof( double ) && typeof( T ) != typeof( byte ) )
                return false;

            //if ( !ShMemHandleValid( h ) )
            //    return false;

            // find one to match requirement
            TShMemUsedInfo h = null;
            for(int i = 0; i < _vectorShMemUsedRec.Count; i++ ) {
                if ( _vectorShMemUsedRec[ i ] == null ) continue;
                if (_vectorShMemUsedRec[i]._strShMemType == typeof(T).FullName && 
                    _vectorShMemUsedRec[i]._nAvailableCount > Convert.ToInt64( nNeededItems)) {
                    h = _vectorShMemUsedRec[ i ]; break;
                }
            }
            if ( h == null )
                return false;

            // req size
            UCDataSyncW32<T> sh = h._pShMemObjInstance as UCDataSyncW32<T>;
            Int64 nLastIndex = h._nNextBegIndex + Convert.ToInt64( nNeededItems );
            if ( nLastIndex > sh.NumOf )
                return false;

            IntPtr pShBegDatAddr = sh.DataBeginAddr;
            if ( pShBegDatAddr == IntPtr.Zero )
                return false;

            // fill info
            TShMemAccItem itm = new TShMemAccItem();
            pItem = itm;
            itm._pItself = itm;
            itm._pUsedInfoAddr = h;
            itm._pShMemObjInstance = sh;
            itm._pShMemName = sh.SharedMemName;
            itm._strShMemType = sh.TypeName;
            itm._nSizeofT = h._nSizeofT;
            itm._pMuxHandle = h._pMuxHandle;
            itm._pBegAddr = IntPtr.Add( pShBegDatAddr, Convert.ToInt32( h._nNextBegIndex ) * sh.SizeOfT);
            itm._nOffset = h._nNextBegIndex;
            itm._nSize = nNeededItems;
            itm._strTypeName = sh.TypeName;
            _vectorShMemItemsRec.Add( itm );
            // update next index
            h._nNextBegIndex = h._nNextBegIndex + Convert.ToInt64( nNeededItems );
            h._nAvailableCount = sh.NumOf - h._nNextBegIndex;

            return true;
        }
        private T[] GetItem<T>( TShMemAccItem item )
        {
            if ( m_bDisposing || m_bDisposed )
                return null;
            if ( typeof( T ) != typeof( Int32 ) && typeof( T ) != typeof( double ) && typeof( T ) != typeof( byte ) )
                return null;
            if ( !ItemHandleValid( item ) )
                return null;

            UCDataSyncW32<T> sh = item._pShMemObjInstance as UCDataSyncW32<T>;
            if ( sh == null ) return null;

            T[] ret = new T[ item._nSize ];
            for ( Int64 i = item._nOffset, count = 0; count < item._nSize; count++, i++ ) {
                sh.Get( i, ref ret[ count ] );
            }

            return ret;
        }
        private bool GetItem<T>( TShMemAccItem item, int offset, ref T val )
        {
            if ( m_bDisposing || m_bDisposed )
                return false;
            if ( typeof( T ) != typeof( Int32 ) && typeof( T ) != typeof( double ) && typeof( T ) != typeof( byte ) )
                return false;
            if ( !ItemHandleValid( item ) )
                return false;
            if ( Convert.ToInt64( offset ) >= item._nSize || offset < 0 )
                return false;

            UCDataSyncW32<T> sh = item._pShMemObjInstance as UCDataSyncW32<T>;
            if ( sh == null ) return false;

            Int64 pos = item._nOffset + Convert.ToInt64( offset );
            return sh.Get( pos, ref val );
        }
        private bool SetItem<T>(TShMemAccItem item, T val, int offset )
        {
            if ( m_bDisposing || m_bDisposed )
                return false;
            if ( typeof( T ) != typeof( Int32 ) && typeof( T ) != typeof( double ) && typeof( T ) != typeof( byte ) )
                return false;
            if ( !ItemHandleValid( item ) )
                return false;
            if ( Convert.ToInt64( offset ) >= item._nSize || offset < 0 )
                return false;

            UCDataSyncW32<T> sh = item._pShMemObjInstance as UCDataSyncW32<T>;
            if ( sh == null ) return false;

            Int64 pos = item._nOffset + Convert.ToInt64( offset );
            return sh.Set(pos, val );
        }
        private bool SetItem<T>( TShMemAccItem item, T[] val, int offset )
        {
            if ( m_bDisposing || m_bDisposed || val == null || val.Length <= 0 )
                return false;
            if ( typeof( T ) != typeof( Int32 ) && typeof( T ) != typeof( double ) && typeof( T ) != typeof( byte ) )
                return false;
            if ( !ItemHandleValid( item ) )
                return false;
            if ( Convert.ToInt64( offset ) >= item._nSize || offset < 0 )
                return false;

            UCDataSyncW32<T> sh = item._pShMemObjInstance as UCDataSyncW32<T>;
            if ( sh == null ) return false;

            Int64[] indexes = new Int64[val.Length];
            Int64 pos = item._nOffset + Convert.ToInt64( offset );
            for ( int i = 0; i < indexes.Length; i++ ) indexes[ i ] = pos + Convert.ToInt64( i );

            return sh.Set( indexes, val );
        }

        public bool AllocItemInt32( int nNeededItems, out TShMemAccItem pItem )
        {
            return AllocItem<Int32>( nNeededItems, out pItem );
        }
        public bool AllocItemDouble( int nNeededItems, out TShMemAccItem pItem )
        {
            return AllocItem<double>( nNeededItems, out pItem );
        }
        public bool AllocItemU8( int nNeededItems, out TShMemAccItem pItem )
        {
            return AllocItem<byte>( nNeededItems, out pItem );
        }

        public Int32[] GetItemInt32( TShMemAccItem item )
        {
            return GetItem<Int32>( item );
        }
        public double[] GetItemDouble( TShMemAccItem item )
        {
            return GetItem<double>( item );
        }
        public byte[] GetItemU8( TShMemAccItem item )
        {
            return GetItem<byte>( item );
        }

        public bool GetItemInt32( TShMemAccItem item, int offset, ref Int32 val )
        {
            return GetItem<Int32>( item, offset, ref val );
        }
        public bool GetItemDouble( TShMemAccItem item, int offset, ref double val )
        {
            return GetItem<double>( item, offset, ref val );
        }
        public bool GetItemU8( TShMemAccItem item, int offset, ref byte val )
        {
            return GetItem<byte>( item, offset, ref val );
        }

        public bool SetItemInt32( TShMemAccItem item, Int32 val, int offset )
        {
            return SetItem<Int32>( item, val, offset );
        }
        public bool SetItemDouble( TShMemAccItem item, double val, int offset )
        {
            return SetItem<double>( item, val, offset );
        }
        public bool SetItemU8( TShMemAccItem item, byte val, int offset )
        {
            return SetItem<byte>( item, val, offset );
        }

        public bool SetItemInt32( TShMemAccItem item, Int32[] val, int offset )
        {
            return SetItem<Int32>( item, val, offset );
        }
        public bool SetItemDouble( TShMemAccItem item, double[] val, int offset )
        {
            return SetItem<double>( item, val, offset );
        }
        public bool SetItemU8( TShMemAccItem item, byte[] val, int offset )
        {
            return SetItem<byte>( item, val, offset );
        }
    }
}
