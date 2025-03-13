using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using uIP.LibBase.MarshalWinSDK;
using uIP.LibBase.Utility;

namespace uIP.LibBase.DataCarrier
{
    public class UCDataSyncW32<T> : UCDataSync<T>
    {
        protected const string UCDATASYNCWIN32_FILEPREFIX = "DATASYNC_FILE_PREFIX";
        protected const string UCDATASYNCWIN32_GIVENNAME = "Given Name";
        protected const string UCDATASYNCWIN32_MUX = "Mux Name";
        protected const string UCDATASYNCWIN32_MAPPINGFILE = "Mapping File Name";
        protected const string UCDATASYNCWIN32_SIZEOFTYPE = "Sizeof Data Type";
        protected const string UCDATASYNCWIN32_SHAREDMEM = "Shared Memory Data";

        protected bool _bReadyAccess = false;
        protected IntPtr _hMapFile = IntPtr.Zero;
        protected IntPtr _hMutex = IntPtr.Zero;
        protected IntPtr _pBegMapFile = IntPtr.Zero;
        protected IntPtr _pData = IntPtr.Zero;

        public IntPtr DataBeginAddr {  get { return _pData; } }
        public IntPtr Mutex {  get { return _hMutex; } }

        public UCDataSyncW32() : base() { }
        protected override void Dispose( bool disposing )
        {
            _bReadyAccess = false;
            _pData = IntPtr.Zero;
            base.Dispose( disposing );

            // unmap address
            if (_pBegMapFile != IntPtr.Zero) {
                MemWinSdkFunctions.UnmapViewOfFile( _pBegMapFile );
                _pBegMapFile = IntPtr.Zero;
            }
            // close handle
            if ( _hMapFile != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hMapFile );
            if ( _hMutex != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hMutex );
            _hMapFile = IntPtr.Zero;
            _hMutex = IntPtr.Zero;
        }

        private static string ToMappingFileName(string str)
        {
            return String.Format( "{0}{1}", UCDataSyncDecl.UCDATASYNCWIN32_SHMEM_PREFIX, str );
        }

        private unsafe void Check() // re-mapping address
        {
            if ( _pBegMapFile == IntPtr.Zero )
                return;

            Int64 Len = 0;
            Int64 nAlloc = 0;

#if UBASE_SUPPORT_TYPE_64
            Int64 *pLen = (Int64*) _pBegMapFile;
            Len = *pLen;
            nAlloc = Len * _nSizeofDataType + sizeof( Int64 );
#else
            Int32* pLen = ( Int32* ) _pBegMapFile;
            Len = *pLen;
            nAlloc = Len * _nSizeofDataType + sizeof( Int32 );
#endif
            // check the length in header
            if ( Len == _nData )
                return;

            // re-create
            if ( _hMutex != IntPtr.Zero )
                WaitWinSdkFunctions.WaitForSingleObject( _hMutex, ( UInt32 ) WAIT.INFINITE );

            _bReadyAccess = false;
            _nData = 0;

            MemWinSdkFunctions.UnmapViewOfFile( _pBegMapFile );
            CommonWinSdkFunctions.CloseHandle( _hMapFile );
            _pBegMapFile = IntPtr.Zero;
            _hMapFile = IntPtr.Zero;
            _pData = IntPtr.Zero;

            IntPtr hMap = MemWinSdkFunctions.OpenFileMapping( ToMappingFileName(_strMappingFileName) );
            if ( hMap == IntPtr.Zero )
#if UBASE_SUPPORT_TYPE_64
            {
                UInt32 hi = Convert.ToUInt32( nAlloc >> 32 );
                UInt32 lo = Convert.ToUInt32( nAlloc & 0xffffffff );
                hMap = MemWinSdkFunctions.CreateFileMapping( lo, _strMappingFileName, hi );
            }
#else
                hMap = MemWinSdkFunctions.CreateFileMapping( Convert.ToUInt32( nAlloc ), ToMappingFileName( _strMappingFileName ) );
#endif
            if ( hMap == IntPtr.Zero ) {
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
                Console.WriteLine( "[Check] create mapping file fail!\n" );
                return;
            }

            IntPtr pBuff = MemWinSdkFunctions.MapViewOfFile( _hMapFile, Convert.ToUInt32( nAlloc ) );
            if (pBuff == IntPtr.Zero) {
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
                Console.WriteLine( "[Check] map view of file fail!\n" );
                CommonWinSdkFunctions.CloseHandle( hMap );
                return;
            }

            byte* pDat = ( byte* ) pBuff.ToPointer();

#if UBASE_SUPPORT_TYPE_64
            pDat += sizeof(Int64);
#else
            pDat += sizeof( Int32 );
#endif
            // config the resources to class data
            _hMapFile = hMap;
            _pBegMapFile = pBuff;

            _pData = new IntPtr( ( void* ) pDat );
            _nData = Len;
            _bReadyAccess = true;

            if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );

# if UBASE_SUPPORT_TYPE_64
            Console.WriteLine( "[Check] re-map count={0}", Len );
#else
            Console.WriteLine( "[Check] re-map count={0}", Len );
#endif
        }

        private void FreeResources()
        {
            _bReadyAccess = false;
            if ( _pBegMapFile != IntPtr.Zero ) {
                MemWinSdkFunctions.UnmapViewOfFile( _pBegMapFile );
            }
            _pBegMapFile = IntPtr.Zero;
            _pData = IntPtr.Zero;
            if ( _hMapFile != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hMapFile );
            if ( _hMutex != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hMutex );

            _hMapFile = IntPtr.Zero;
            _hMutex = IntPtr.Zero;
        }
        private bool OpenResources( Int64 count )
        {
            if ( count <= 0 )
                return false;

            bool bEverOpened = false;

            // alloc resources - mutex
            IntPtr hMux = SyncWinSdkFunctions.OpenMutex( _strMutexName );
            if ( hMux == IntPtr.Zero )
                hMux = SyncWinSdkFunctions.CreateMutex( _strMutexName );
            _hMutex = hMux;
            // alloc resource - mapping file
            IntPtr hMap = MemWinSdkFunctions.OpenFileMapping( ToMappingFileName(_strMappingFileName ));
            Int64 nAllocSz = count * Convert.ToInt64( _nSizeofDataType)  +
#if UBASE_SUPPORT_TYPE_64
                sizeof( Int64 );
#else
                sizeof( Int32 );
#endif
            if ( hMap == IntPtr.Zero )
#if UBASE_SUPPORT_TYPE_64
            {
                UInt32 hi = Convert.ToUInt32( nAllocSz >> 32 );
                UInt32 lo = Convert.ToUInt32( nAllocSz & 0xffffffff );
                hMap = MemWinSdkFunctions.CreateFileMapping( lo, _strMappingFileName, hi );
            }
#else
                hMap = MemWinSdkFunctions.CreateFileMapping( Convert.ToUInt32( nAllocSz ), ToMappingFileName( _strMappingFileName ) );
#endif
            else
                bEverOpened = true;

            if ( hMap == IntPtr.Zero ) {
                Console.WriteLine( "[UCDataSyncW32::OpenResources]Call CreateFileMappingA with error code-{0}", CommonWinSdkFunctions.GetLastError() );
                return false;
            }
            IntPtr pBuff = MemWinSdkFunctions.MapViewOfFile( hMap, Convert.ToUInt32( nAllocSz ) );
            if ( pBuff == IntPtr.Zero ) {
                Console.WriteLine( "[UCDataSyncW32::OpenResources]Call MapViewOfFile with error code-{0}", CommonWinSdkFunctions.GetLastError() );
                CommonWinSdkFunctions.CloseHandle( hMap );
                return false;
            }

            // not created, clear all the contents
            if ( !bEverOpened ) {
                if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, ( UInt32 ) WAIT.INFINITE );
                unsafe
                {
                    byte* p8 = ( byte* ) pBuff.ToPointer();
                    for ( Int64 i = 0; i < nAllocSz; i++ ) p8[ i ] = 0;
#if UBASE_SUPPORT_TYPE_64
                    Int64 *p64 = (Int64*)p8;
                    *p64 = count;
#else
                    Int32* p32 = ( Int32* ) p8;
                    *p32 = Convert.ToInt32( count ); // write the size
#endif
                }
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }


            // config pointer
            unsafe
            {
                byte* p8 = ( byte* ) pBuff.ToPointer();
#if UBASE_SUPPORT_TYPE_64
                //Int64 *p64 = (Int64*) p8;
                //*p64 = count;
                p8 += sizeof(Int64);
#else
                //Int32* p32 = ( Int32* ) p8;
                //*p32 = Convert.ToInt32( count );
                p8 += sizeof( Int32 );
#endif
                _pData = new IntPtr( ( void* ) p8 );
            }
            // config the resources to class data
            _hMapFile = hMap;
            _pBegMapFile = pBuff;
            _nData = count;
            _bReadyAccess = true;

            return true;
        }

        public override bool Initialize( string pMapFilename, string pMuxName, Int64 nEleCount )
        {
            //static const char *pPrefix = "Local\\";
            // already created?
            if ( _bReadyAccess )
                return true;
            if ( String.IsNullOrEmpty( pMapFilename ) || String.IsNullOrEmpty( pMuxName ) )
                return false;
            bool bTpOk = typeof( T ).IsValueType && !typeof( T ).IsEnum;
            if ( !bTpOk )
                return false;

            _strMappingFileName = String.Copy( pMapFilename );
            _strMutexName = String.Copy( pMuxName );

            // create mux
            if ( _hMutex == IntPtr.Zero ) {
                IntPtr hMux = SyncWinSdkFunctions.OpenMutex( _strMutexName );
                if ( hMux == IntPtr.Zero )
                    hMux = SyncWinSdkFunctions.CreateMutex( _strMutexName );
                _hMutex = hMux;
            }
            // condition check
            if ( nEleCount < 0 )
                return false;
            // create mapping file handle
            Int64 nAlloc = 0;
            bool bEverOpened = false;

#if UBASE_SUPPORT_TYPE_64
            nAlloc = nEleCount * ( Int64 ) _nSizeofDataType + sizeof( Int64 );
#else
            nAlloc = nEleCount * ( Int64 ) _nSizeofDataType + sizeof( Int32 );
#endif

            IntPtr hMap = MemWinSdkFunctions.OpenFileMapping( ToMappingFileName(_strMappingFileName ));
            if ( hMap == IntPtr.Zero )
#if UBASE_SUPPORT_TYPE_64
            {
                UInt32 hi = Convert.ToUInt32( nAlloc >> 32 );
                UInt32 lo = Convert.ToUInt32( nAlloc & 0xffffffff );
                hMap = MemWinSdkFunctions.CreateFileMapping( lo, _strMappingFileName, hi );
            }
#else
                hMap = MemWinSdkFunctions.CreateFileMapping( Convert.ToUInt32( nAlloc ), ToMappingFileName(_strMappingFileName ) );
#endif
            else
                bEverOpened = true;
            if ( hMap == IntPtr.Zero ) {
                Console.WriteLine( "[UCDataSyncW32::Initialize]Call CreateFileMappingA with error code-{0}", CommonWinSdkFunctions.GetLastError() );
                return false;
            }
            IntPtr pBuff = MemWinSdkFunctions.MapViewOfFile( hMap, Convert.ToUInt32( nAlloc ) );
            if ( pBuff == IntPtr.Zero ) {
                CommonWinSdkFunctions.CloseHandle( hMap );
                return false;
            }

            // not created, clear all the contents
            if ( !bEverOpened ) {
                if (_hMutex != IntPtr.Zero) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, (UInt32)WAIT.INFINITE );
                unsafe
                {
                    byte* p8 = ( byte* ) pBuff.ToPointer();
                    for ( Int64 i = 0; i < nAlloc; i++ ) p8[ i ] = 0;
#if UBASE_SUPPORT_TYPE_64
                    Int64 *p64 = (Int64*)p8;
                    *p64 = nEleCount;
#else
                    Int32* p32 = ( Int32* ) p8;
                    *p32 = Convert.ToInt32( nEleCount ); // write the size
#endif
                }
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }

            // config the resources to class data
            _hMapFile = hMap;
            _pBegMapFile = pBuff;
            unsafe
            {
                byte* p8 = ( byte* ) pBuff.ToPointer();
#if UBASE_SUPPORT_TYPE_64
                p8 += sizeof(Int64);
#else
                p8 += sizeof( Int32 );
#endif
                _pData = new IntPtr( ( void* ) p8 );
            }
            _nData = nEleCount;
            _bReadyAccess = true;

            return true;
        }

        public override T[] Get( out Int64 nSize )
        {
            nSize = 0;
            if ( !_bReadyAccess || _pData == IntPtr.Zero )
                return null;

            Check();

            T[] ret = new T[ _nData ];

            unsafe
            {
                byte* p8 = ( byte* ) _pData.ToPointer();
                for ( Int64 i = 0; i < _nData; i++, p8 += _nSizeofDataType ) {
                    IntPtr addr = Marshal.UnsafeAddrOfPinnedArrayElement( ret as Array, Convert.ToInt32( i ) );
                    byte* pDst = ( byte* ) addr.ToPointer();
                    for ( int x = 0; x < _nSizeofDataType; x++ ) pDst[ x ] = p8[ x ]; // byte to byte copy
                }
            }

            nSize = _nData;
            return ret;
        }
        public override bool Get( Int64 index, ref T val )
        {
            if ( !_bReadyAccess )
                return false;

            Check();
            if ( index < 0 || index >= _nData )
                return false;

            T[] arr = new T[ 1 ];

            unsafe
            {
                byte* p8 = ( byte* ) _pData.ToPointer();
                p8 += ( Convert.ToInt64( _nSizeofDataType ) * index );
                IntPtr addr = Marshal.UnsafeAddrOfPinnedArrayElement( arr as Array, 0 );
                byte* pDst = ( byte* ) addr.ToPointer();
                for ( int x = 0; x < _nSizeofDataType; x++ ) pDst[ x ] = p8[ x ]; // byte to byte copy
            }

            val = arr[ 0 ];
            return true;
        }
        public override IntPtr GetAddr( Int64 index )
        {
            if ( !_bReadyAccess )
                return IntPtr.Zero;

            Check();
            if ( index < 0 || index >= _nData )
                return IntPtr.Zero;

            IntPtr ret = IntPtr.Zero;
            unsafe
            {
                byte* p8 = ( byte* ) _pData.ToPointer();
                p8 += ( Convert.ToInt64( _nSizeofDataType ) * index );

                ret = new IntPtr( ( void* ) p8 );
            }

            return ret;
        }
        public override bool Set( Int64 index, T val, UInt32 nTimeout = UInt32.MaxValue )
        {
            if ( !_bReadyAccess )
                return false;

            Check();
            if ( index < 0 || index >= _nData )
                return false;

            if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, nTimeout );
            try {
                unsafe {
                    byte* p8 = ( byte* ) _pData.ToPointer();
                    p8 += ( index * Convert.ToInt64( _nSizeofDataType ) );
                    Marshal.StructureToPtr( val, new IntPtr( ( void* ) p8 ), false );
                }
            }finally {
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }
            return true;
        }
        public override bool Set( Int64[] indexes, T[] vals, UInt32 nTimeout = UInt32.MaxValue )
        {
            if ( !_bReadyAccess )
                return false;
            if ( indexes.Length != vals.Length )
                return false;

            Check();

            if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, nTimeout );
            try {
                unsafe
                {
                    for ( int i = 0; i < indexes.Length; i++ ) {
                        if ( indexes[ i ] < 0 || indexes[ i ] >= _nData )
                            continue;
                        byte* p8 = ( byte* ) _pData.ToPointer();
                        p8 += ( indexes[ i ] * Convert.ToInt64( _nSizeofDataType ) );
                        Marshal.StructureToPtr( vals[ i ], new IntPtr( ( void* ) p8 ), false );
                    }
                }
            } finally {
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }
            return true;
        }

        public override void RemoveNamedMap( string pName )
        {
            if ( IsDispose ) return;
            TUCDataItemInfo info = null;
            Monitor.Enter( _hSyncOp );
            try {
                if ( _mapNamedItems.ContainsKey( pName ) ) {
                    info = _mapNamedItems[ pName ];
                    _mapNamedItems.Remove( pName );
                }
            } finally { Monitor.Exit( _hSyncOp ); }

            // clear shared memory
            if ( info != null && info._nOffset >= 0 && (info._nOffset + info._nSize) <= _nData ) {

                if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, UInt32.MaxValue );
                try {
                    unsafe
                    {
                        byte* p8 = ( byte* ) _pData.ToPointer();
                        p8 += ( info._nOffset * Convert.ToInt64( _nSizeofDataType ) );
                        Int64 clrSz = info._nSize * Convert.ToInt64( _nSizeofDataType );
                        for ( Int64 i = 0; i < clrSz; i++ ) p8[ i ] = 0;
                    }
                } finally {
                    if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
                }
            }
        }
        public override T[] GetFromNamedMapArray( string pName )
        {
            if ( !_bReadyAccess )
                return null;

            TUCDataItemInfo info;
            if ( !QueryFromNamedMap( pName, out info ) || info == null )
                return null;

            T[] ret = null;

            if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, UInt32.MaxValue );
            try {
                unsafe
                {
                    ret = new T[ info._nSize ];
                    for(Int64 i = info._nOffset, count = 0; count < info._nSize; count++, i++ ) {
                        byte* p8 = (byte*) _pData.ToPointer();
                        p8 += ( Convert.ToInt64( _nSizeofDataType ) * i );
                        IntPtr pAddr = Marshal.UnsafeAddrOfPinnedArrayElement( ret as Array, Convert.ToInt32( count ) );
                        byte* pDst = ( byte* ) pAddr.ToPointer();
                        for ( int x = 0; x < _nSizeofDataType; x++ ) pDst[ x ] = p8[ x ]; // byte to byte copy
                    }
                }
            } finally {
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }

            return ret;
        }
        public override bool SetFromNamedMap( string pName, T[] pDat2Put, int dstOffset, bool sync )
        {
            if ( !_bReadyAccess || dstOffset < 0)
                return false;

            if ( _hMutex != IntPtr.Zero && sync ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, UInt32.MaxValue );
            try {
                TUCDataItemInfo info;
                if ( !QueryFromNamedMap( pName, out info ) || info == null )
                    return false;

                for ( int i = 0; i < pDat2Put.Length && i < Convert.ToInt32( info._nSize ); i++ ) {
                    Int64 currIndex = info._nOffset + Convert.ToInt64( i + dstOffset );
                    if ( currIndex >= _nData || (currIndex + Convert.ToInt64( _nSizeofDataType * pDat2Put.Length ) ) >= _nData )
                        return false;

                    unsafe
                    {
                        byte* p8 = ( byte* ) _pData.ToPointer();
                        p8 += ( Convert.ToInt64( _nSizeofDataType ) * currIndex );
                        IntPtr pAddr = Marshal.UnsafeAddrOfPinnedArrayElement( pDat2Put as Array, i );
                        byte* pSrc = ( byte* ) pAddr.ToPointer();
                        for ( int x = 0; x < _nSizeofDataType; x++ ) p8[ x ] = pSrc[ x ];
                    }

                }
                return true;
            }finally {
                if ( _hMutex != IntPtr.Zero && sync ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }
        }

        private bool _bIniReadFileChanged = false;
        public bool IniReadActionSzChanged { get { return _bIniReadFileChanged; } }
        /// <summary>
        /// --- File Format ---
        /// [DataType]
        /// Type = name of this class
        /// [Resources]
        /// MappingFileName = string
        /// MutexName = string
        /// AllocItemCount = int64
        /// [Items]
        /// NameOfItem = indexBeg, size, value1, value2...
        /// </summary>
        /// <param name="path"></param>
        /// <param name="askWrOneDat"></param>
        /// <returns></returns>
        public bool ReadFileIni( string path, fpUCDataSync32IniReadCallback<T> askWrOneDat, Int64 nForceChangedLen = 0 )
        {
            _bIniReadFileChanged = false;
            //if ( !_bReadyAccess ) return false;
            if ( !File.Exists( path ) || askWrOneDat == null )
                return false;

            IniReaderUtility ini = new IniReaderUtility();
            if ( !ini.Parsing( path ) )
                return false;

            // Type check
            List<string[]> values = new List<string[]>();
            if ( !ini.Get( "DataType", ref values, "Type" ) )
                return false;
            if ( values == null || values.Count <= 0 || values[ 0 ] == null || values[ 0 ].Length <= 0 || String.IsNullOrEmpty( values[ 0 ][ 0 ] ) )
                return false;
            if ( typeof( T ).FullName != values[ 0 ][ 0 ] ) // type check fail
                return false;

            // Resource
            string mappingNm = null;
            string muxNm = null;
            Int64 nAlloc = 0;

            if (!ini.Get("Resources", ref values, "MappingFileName", "MutexName", "AllocItemCount" ) ||
                values.Count < 3 || values[0] == null || values[0].Length <= 0 || String.IsNullOrEmpty(values[0][0]) ||
                values[1] == null || values[1].Length <= 0 || String.IsNullOrEmpty(values[1][0]) ||
                values[2] == null || values[2].Length <= 0 || String.IsNullOrEmpty(values[2][0]) ) {
                return false;
            }

            mappingNm = values[ 0 ][ 0 ].Trim().Replace( "\"", "" );
            muxNm = values[ 1 ][ 0 ].Trim().Replace( "\"", "" );
            try { nAlloc = Convert.ToInt64( values[ 2 ][ 0 ] ); } catch {
                return false;
            }
            if ( String.IsNullOrEmpty( mappingNm ) || String.IsNullOrEmpty( muxNm ) )
                return false;
            if ( nAlloc <= 0 )
                return false;

            bool bSzChanged = false;
            if ( nForceChangedLen > 0 && nAlloc != nForceChangedLen ) {
                nAlloc = nForceChangedLen;
                bSzChanged = true;
            }

            Monitor.Enter( _hSyncOp );
            try {

                // copy need info
                _strMappingFileName = String.Copy( mappingNm );
                _strMutexName = String.Copy( muxNm );

                // free resources
                FreeResources();

                // create resources
                if ( !OpenResources( nAlloc ) )
                    return false;

                // read each data
                SectionDataOfIni dat = ini.Get( "Items" );
                if ( dat.Data.Count <= 0 )
                    return true;

                if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, ( UInt32 ) WAIT.INFINITE );
                try {

                    for ( int i = 0; i < dat.Data.Count; i++ ) {
                        if ( String.IsNullOrEmpty( dat.Data[ i ].Key ) || dat.Data[ i ].Values == null )
                            continue;
                        string k = dat.Data[ i ].Key.Trim().Replace( "\"", "" );
                        if ( k.IndexOf( "UCDataSyncW32" ) == 0 ) {
                            if ( dat.Data[ i ].Values.Length == 3 || !String.IsNullOrEmpty( dat.Data[ i ].Values[ 2 ] ) ) {
                                // array one-by-one
                                Int64 index = -1;
                                try { index = Convert.ToInt64( k.Split( '_' )[ 1 ] ); } catch { continue; }
                                if ( index < 0 || index >= _nData )
                                    continue;
                                Int64 addrOffset = index * Convert.ToInt64( _nSizeofDataType );
                                unsafe
                                {
                                    byte* p8 = ( byte* ) _pData.ToPointer();
                                    p8 += addrOffset;

                                    askWrOneDat( dat.Data[ i ].Values[ 2 ], new IntPtr( ( void* ) p8 ) ); // call to write a data
                                }
                            }
                        } else if ( dat.Data[ i ].Values.Length >= 2 ) {
                            if ( String.IsNullOrEmpty( dat.Data[ i ].Values[ 0 ] ) || String.IsNullOrEmpty( dat.Data[ i ].Values[ 1 ] ) )
                                continue;
                            Int64 index = Convert.ToInt64( dat.Data[ i ].Values[ 0 ] );
                            Int64 size = Convert.ToInt64( dat.Data[ i ].Values[ 1 ] );
                            if ( SetNamedMap( k, index, size ) ) {
                                unsafe
                                {
                                    Int64 addrOffset = index * Convert.ToInt64( _nSizeofDataType );
                                    byte* p8 = ( byte* ) _pData.ToPointer();
                                    p8 += addrOffset;
                                    for ( int x = 2; x < dat.Data[ i ].Values.Length; x++, p8 += _nSizeofDataType ) {
                                        askWrOneDat( dat.Data[ i ].Values[ x ], new IntPtr( ( void* ) p8 ) );
                                    }
                                }
                            }
                        }

                    }

                } finally {
                    if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
                }
            } finally {
                Monitor.Exit( _hSyncOp );
            }

            _bIniReadFileChanged = bSzChanged;
            return true;
        }

        public bool WriteFileIni(string path, fpUCDataSync32IniWriteCallback<T> qConvValsStr)
        {
            if ( !_bReadyAccess || qConvValsStr == null )
                return false;

            if ( _hMutex != IntPtr.Zero ) WaitWinSdkFunctions.WaitForSingleObject( _hMutex, ( UInt32 ) WAIT.INFINITE );
            try {

                using(Stream ws = File.Open(path, FileMode.Create)) {
                    StreamWriter sw = new StreamWriter( ws );

                    sw.WriteLine( "[DataType]" );
                    sw.WriteLine( "Type={0}", typeof( T ).FullName );
                    sw.WriteLine();

                    sw.WriteLine( "[Resources]" );
                    sw.WriteLine( "MappingFileName=\"{0}\"", _strMappingFileName );
                    sw.WriteLine( "MutexName=\"{0}\"", _strMutexName );
                    sw.WriteLine( "AllocItemCount={0}", _nData );
                    sw.WriteLine();

                    sw.WriteLine( "[Items]" );
                    // write each data
                    for(Int64 i = 0; i < _nData; i++ ) {
                        if ( CheckIndexInNamedMap( i ) )
                            continue;
                        string repo = qConvValsStr( _pData, i, 1, _nSizeofDataType, "," );
                        if ( String.IsNullOrEmpty( repo ) )
                            continue;
                        sw.WriteLine( "UCDataSyncW32_{0}=0,1,{1}", i, repo );
                    }
                    sw.WriteLine();
                    // write user def
                    foreach(KeyValuePair<string, TUCDataItemInfo> kv in _mapNamedItems) {
                        if ( kv.Value == null )
                            continue;

                        string repo = qConvValsStr( _pData, kv.Value._nOffset, kv.Value._nSize, _nSizeofDataType, "," );
                        if ( String.IsNullOrEmpty( kv.Key ) || String.IsNullOrEmpty(repo) )
                            continue;

                        sw.WriteLine( "\"{0}\"={1},{2},{3}", kv.Key, kv.Value._nOffset, kv.Value._nSize, repo );
                    }
                    sw.WriteLine();

                    sw.Dispose();
                    sw = null;
                }

            } finally {
                if ( _hMutex != IntPtr.Zero ) SyncWinSdkFunctions.ReleaseMutex( _hMutex );
            }
            return true;
        }
    }

    public static class UCDataSyncW32Utils
    {
        public static string UCDataSync32IniWriteCallbackDouble( IntPtr pDatBeg, Int64 nItemOffset, Int64 nCount, Int32 nItemSizeof, string concatStr )
        {
            if ( pDatBeg == IntPtr.Zero ) return null;
            if ( sizeof( double ) != nItemSizeof ) return null;
            StringBuilder sb = new StringBuilder();
            unsafe
            {
                byte* p8 = ( byte* ) pDatBeg.ToPointer();
                p8 += ( nItemOffset * Convert.ToInt64( nItemSizeof ) );
                double* pDf = ( double* ) p8;
                for ( Int64 i = 0; i < nCount; i++ ) {
                    double df = pDf[ i ];
                    if ( i == 0 ) sb.Append( df.ToString() );
                    else sb.AppendFormat( ",{0}", df.ToString() );
                }
            }
            return sb.ToString();
        }
        public static void UCDataSync32IniReadCallbackDouble( string pRdDat, IntPtr pCurWrOneDatAddr )
        {
            if ( String.IsNullOrEmpty( pRdDat ) ) return;
            string[] strs = pRdDat.Split( ',' );
            if ( strs == null || strs.Length <= 0 ) return;
            unsafe
            {
                double* pDf = ( double* ) pCurWrOneDatAddr;
                for ( int i = 0; i < strs.Length; i++, pDf++ ) {
                    if ( String.IsNullOrEmpty( strs[ i ] ) )
                        continue;
                    try {
                        string val = strs[ i ].Trim();
                        double convDf = Convert.ToDouble( val );
                        *pDf = convDf;
                    } catch { break; }
                }
            }
        }
        public static bool ReadIniDouble( UCDataSyncW32<double> inst, string path )
        {
            if ( inst == null ) return false;
            return inst.ReadFileIni( path, UCDataSync32IniReadCallbackDouble );
        }
        public static bool WriteIniDouble( UCDataSyncW32<double> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.WriteFileIni( path, UCDataSync32IniWriteCallbackDouble );
        }

        public static string UCDataSync32IniWriteCallbackInt64( IntPtr pDatBeg, Int64 nItemOffset, Int64 nCount, Int32 nItemSizeof, string concatStr )
        {
            if ( pDatBeg == IntPtr.Zero ) return null;
            if ( sizeof( Int64 ) != nItemSizeof ) return null;
            StringBuilder sb = new StringBuilder();
            unsafe
            {
                byte* p8 = ( byte* ) pDatBeg.ToPointer();
                p8 += ( nItemOffset * Convert.ToInt64( nItemSizeof ) );
                Int64* pDat = ( Int64* ) p8;
                for ( Int64 i = 0; i < nCount; i++ ) {
                    Int64 val = pDat[ i ];
                    if ( i == 0 ) sb.Append( val.ToString() );
                    else sb.AppendFormat( ",{0}", val.ToString() );
                }
            }
            return sb.ToString();
        }
        public static void UCDataSync32IniReadCallbackInt64( string pRdDat, IntPtr pCurWrOneDatAddr )
        {
            if ( String.IsNullOrEmpty( pRdDat ) ) return;
            string[] strs = pRdDat.Split( ',' );
            if ( strs == null || strs.Length <= 0 ) return;
            unsafe
            {
                Int64* pDat = ( Int64* ) pCurWrOneDatAddr;
                for ( int i = 0; i < strs.Length; i++, pDat++ ) {
                    if ( String.IsNullOrEmpty( strs[ i ] ) )
                        continue;
                    try {
                        string val = strs[ i ].Trim();
                        Int64 conv = Convert.ToInt64( val );
                        *pDat = conv;
                    } catch { break; }
                }
            }
        }
        public static bool ReadIniI64( UCDataSyncW32<Int64> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.ReadFileIni( path, UCDataSync32IniReadCallbackInt64 );
        }
        public static bool WriteIniI64( UCDataSyncW32<Int64> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.WriteFileIni( path, UCDataSync32IniWriteCallbackInt64 );
        }

        public static string UCDataSync32IniWriteCallbackInt32( IntPtr pDatBeg, Int64 nItemOffset, Int64 nCount, Int32 nItemSizeof, string concatStr )
        {
            if ( pDatBeg == IntPtr.Zero ) return null;
            if ( sizeof( Int32 ) != nItemSizeof ) return null;
            StringBuilder sb = new StringBuilder();
            unsafe
            {
                byte* p8 = ( byte* ) pDatBeg.ToPointer();
                p8 += ( nItemOffset * Convert.ToInt64( nItemSizeof ) );
                Int32* pDat = ( Int32* ) p8;
                for ( Int64 i = 0; i < nCount; i++ ) {
                    Int32 val = pDat[ i ];
                    if ( i == 0 ) sb.Append( val.ToString() );
                    else sb.AppendFormat( ",{0}", val.ToString() );
                }
            }
            return sb.ToString();
        }
        public static void UCDataSync32IniReadCallbackInt32( string pRdDat, IntPtr pCurWrOneDatAddr )
        {
            if ( String.IsNullOrEmpty( pRdDat ) ) return;
            string[] strs = pRdDat.Split( ',' );
            if ( strs == null || strs.Length <= 0 ) return;
            unsafe
            {
                Int32* pDat = ( Int32* ) pCurWrOneDatAddr;
                for ( int i = 0; i < strs.Length; i++, pDat++ ) {
                    if ( String.IsNullOrEmpty( strs[ i ] ) )
                        continue;
                    try {
                        string val = strs[ i ].Trim();
                        Int32 conv = Convert.ToInt32( val );
                        *pDat = conv;
                    } catch { break; }
                }
            }
        }
        public static bool ReadIniI32( UCDataSyncW32<Int32> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.ReadFileIni( path, UCDataSync32IniReadCallbackInt32 );
        }
        public static bool WriteIniI32( UCDataSyncW32<Int32>inst, string path)
        {
            if ( inst == null ) return false;
            return inst.WriteFileIni( path, UCDataSync32IniWriteCallbackInt32 );
        }

        public static string UCDataSync32IniWriteCallbackInt16( IntPtr pDatBeg, Int64 nItemOffset, Int64 nCount, Int32 nItemSizeof, string concatStr )
        {
            if ( pDatBeg == IntPtr.Zero ) return null;
            if ( sizeof( Int16 ) != nItemSizeof ) return null;
            StringBuilder sb = new StringBuilder();
            unsafe
            {
                byte* p8 = ( byte* ) pDatBeg.ToPointer();
                p8 += ( nItemOffset * Convert.ToInt64( nItemSizeof ) );
                Int16* pDat = ( Int16* ) p8;
                for ( Int64 i = 0; i < nCount; i++ ) {
                    Int16 val = pDat[ i ];
                    if ( i == 0 ) sb.Append( val.ToString() );
                    else sb.AppendFormat( ",{0}", val.ToString() );
                }
            }
            return sb.ToString();
        }
        public static void UCDataSync32IniReadCallbackInt16( string pRdDat, IntPtr pCurWrOneDatAddr )
        {
            if ( String.IsNullOrEmpty( pRdDat ) ) return;
            string[] strs = pRdDat.Split( ',' );
            if ( strs == null || strs.Length <= 0 ) return;
            unsafe
            {
                Int16* pDat = ( Int16* ) pCurWrOneDatAddr;
                for ( int i = 0; i < strs.Length; i++, pDat++ ) {
                    if ( String.IsNullOrEmpty( strs[ i ] ) )
                        continue;
                    try {
                        string val = strs[ i ].Trim();
                        Int16 conv = Convert.ToInt16( val );
                        *pDat = conv;
                    } catch { break; }
                }
            }
        }
        public static bool ReadIniI16( UCDataSyncW32<Int16> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.ReadFileIni( path, UCDataSync32IniReadCallbackInt16 );
        }
        public static bool WriteIniI16( UCDataSyncW32<Int16> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.WriteFileIni( path, UCDataSync32IniWriteCallbackInt16 );
        }

        public static string UCDataSync32IniWriteCallbackByte( IntPtr pDatBeg, Int64 nItemOffset, Int64 nCount, Int32 nItemSizeof, string concatStr )
        {
            if ( pDatBeg == IntPtr.Zero ) return null;
            if ( sizeof( byte ) != nItemSizeof ) return null;
            StringBuilder sb = new StringBuilder();
            unsafe
            {
                byte* p8 = ( byte* ) pDatBeg.ToPointer();
                p8 += ( nItemOffset * Convert.ToInt64( nItemSizeof ) );
                byte* pDat = ( byte* ) p8;
                for ( Int64 i = 0; i < nCount; i++ ) {
                    byte val = pDat[ i ];
                    if ( i == 0 ) sb.Append( val.ToString() );
                    else sb.AppendFormat( ",{0}", val.ToString() );
                }
            }
            return sb.ToString();
        }
        public static void UCDataSync32IniReadCallbackByte( string pRdDat, IntPtr pCurWrOneDatAddr )
        {
            if ( String.IsNullOrEmpty( pRdDat ) ) return;
            string[] strs = pRdDat.Split( ',' );
            if ( strs == null || strs.Length <= 0 ) return;
            unsafe
            {
                byte* pDat = ( byte* ) pCurWrOneDatAddr;
                for ( int i = 0; i < strs.Length; i++, pDat++ ) {
                    if ( String.IsNullOrEmpty( strs[ i ] ) )
                        continue;
                    try {
                        string val = strs[ i ].Trim();
                        byte conv = Convert.ToByte( val );
                        *pDat = conv;
                    } catch { break; }
                }
            }
        }
        public static bool ReadIniByte( UCDataSyncW32<byte> inst, string path)
        {
            if ( inst == null ) return false;
            return inst.ReadFileIni( path, UCDataSync32IniReadCallbackByte );
        }
        public static bool WriteIniByte( UCDataSyncW32<byte> inst, string path)
        {
            if (inst == null) return false;
            return inst.WriteFileIni( path, UCDataSync32IniWriteCallbackByte );
        }

    }
}
