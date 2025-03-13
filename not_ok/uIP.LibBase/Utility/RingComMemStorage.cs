using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.Utility
{
    public unsafe class RingComMemStorage : IDisposable
    {
        private bool _bDisposing = false;
        private bool _bDisposed = false;
        private bool _bConsumer = false;
        private bool _bReady = false;

        private string _strSharedMemName = null;
        private IntPtr _hSharedMem = IntPtr.Zero;
        private IntPtr _pSharedMem = IntPtr.Zero;

        private string _strAddEventName = null;
        private IntPtr _hAddEvent = IntPtr.Zero; // auto-reset

        private string _strMuxName = null;
        private IntPtr _hMux = IntPtr.Zero;

        public string SharedMem { get { return _strSharedMemName; } }
        public string NameOfAddEvent { get { return _strAddEventName; } }
        public string NameOfMux { get { return _strMuxName; } }
        public IntPtr hAddEvent { get { return _hAddEvent; } }
        public bool Ready {  get { return _bReady; } }

        private static bool FormatStruct(IntPtr addr, string strAdd, string strMux, int nMax)
        {
            if ( String.IsNullOrEmpty( strAdd ) || String.IsNullOrEmpty( strMux ) || addr == IntPtr.Zero )
                return false;

            TSharedMemLayoutPacked shmem = ( TSharedMemLayoutPacked ) Marshal.PtrToStructure( addr, typeof( TSharedMemLayoutPacked ) );

            byte[] ba = Encoding.ASCII.GetBytes( strAdd );
            for ( int i = 0 ; i < shmem._strNameOfAddEvent.Length && i < ba.Length ; i++ )
                shmem._strNameOfAddEvent[ i ] = ba[ i ];
            ba = Encoding.ASCII.GetBytes( strMux );
            for ( int i = 0 ; i < shmem._strNameOfMux.Length && i < ba.Length ; i++ )
                shmem._strNameOfMux[ i ] = ba[ i ];

            shmem._bAvailable = Convert.ToInt32( true );
            shmem._nMaxCount = nMax;

            Marshal.StructureToPtr( shmem, addr, false );
            return true;
        }

        // consumer
        public RingComMemStorage(string nameOfSharedMem, string nameOfAddEvent, string nameOfMuxName, Int32 nRingSize = 10)
        {
            _bConsumer = true;

            // create event
            if ( !String.IsNullOrEmpty( nameOfAddEvent ) )
            {
                nameOfAddEvent = CommonUtilities.ToAnsiString( nameOfAddEvent, TSharedMemLayoutPacked._nMaxNameSz - 1 );
                if ( (_hAddEvent = EventWinSdkFunctions.Open( ( UInt32 ) EVT_ACC_RIGHT.ALL_ACCESS, false, nameOfAddEvent )) == IntPtr.Zero )
                {
                    _hAddEvent = EventWinSdkFunctions.Create( false, false, nameOfAddEvent );
                    _strAddEventName = _hAddEvent == IntPtr.Zero ? null : String.Copy( nameOfAddEvent );
                }
                else
                    _strAddEventName = String.Copy( nameOfAddEvent );
            }

            // create mux
            if ( !String.IsNullOrEmpty( nameOfMuxName ) )
            {
                nameOfMuxName = CommonUtilities.ToAnsiString( nameOfMuxName, TSharedMemLayoutPacked._nMaxNameSz - 1 );
                if ( (_hMux = SyncWinSdkFunctions.OpenMutex( nameOfMuxName )) == IntPtr.Zero )
                {
                    _hMux = SyncWinSdkFunctions.CreateMutex( nameOfMuxName );
                    _strMuxName = _hMux == IntPtr.Zero ? null : String.Copy( nameOfMuxName );
                }
                else
                    _strMuxName = String.Copy( nameOfMuxName );
            }

            // open shared mem
            if(!String.IsNullOrEmpty(nameOfSharedMem))
            {
                UInt32 bsz = Convert.ToUInt32( Marshal.SizeOf( typeof( TSharedMemLayoutPacked ) ) + sizeof( IntPtr ) * nRingSize );
                if ( (_hSharedMem = MemWinSdkFunctions.OpenFileMapping( nameOfSharedMem )) == IntPtr.Zero )
                    _hSharedMem = MemWinSdkFunctions.CreateFileMapping( bsz, nameOfSharedMem );
                if ( _hSharedMem != IntPtr.Zero )
                    _pSharedMem = MemWinSdkFunctions.MapViewOfFile( _hSharedMem, bsz );

                if ( _pSharedMem != IntPtr.Zero )
                {
                    MemWinSdkFunctions.NativeMemset( _pSharedMem, 0, new UIntPtr( bsz ) );
                    _bReady = FormatStruct( _pSharedMem, _strAddEventName, _strMuxName, nRingSize ); // fill info to shared mem
                    _strSharedMemName = String.Copy( nameOfSharedMem );
                }
            }
        }
        // not consumer
        public RingComMemStorage(string nameOfSharedMem)
        {
            // open shared mem
            UInt32 bsz = Convert.ToUInt32( Marshal.SizeOf( typeof( TSharedMemLayoutPacked ) ) );
            IntPtr hMapFile = IntPtr.Zero;
            IntPtr pShMem = IntPtr.Zero;
            if ( (hMapFile = MemWinSdkFunctions.OpenFileMapping( nameOfSharedMem )) == IntPtr.Zero )
                return; // fail
            if ( (pShMem = MemWinSdkFunctions.MapViewOfFile( hMapFile, bsz )) == IntPtr.Zero )
                return; // fail

            // read the data
            TSharedMemLayoutPacked hdr = ( TSharedMemLayoutPacked ) Marshal.PtrToStructure( pShMem, typeof( TSharedMemLayoutPacked ) );
            _strMuxName = Encoding.ASCII.GetString( hdr._strNameOfMux );
            _strAddEventName = Encoding.ASCII.GetString( hdr._strNameOfAddEvent );

            bsz = bsz + Convert.ToUInt32( sizeof( IntPtr ) * hdr._nMaxCount );
            MemWinSdkFunctions.UnmapViewOfFile( pShMem );
            _hSharedMem = hMapFile;
            _pSharedMem = MemWinSdkFunctions.MapViewOfFile( hMapFile, bsz );

            _hAddEvent = EventWinSdkFunctions.Open( ( UInt32 ) EVT_ACC_RIGHT.ALL_ACCESS, false, _strAddEventName );
            _hMux = SyncWinSdkFunctions.OpenMutex( _strMuxName );

            _bReady = _pSharedMem != IntPtr.Zero && _hAddEvent != IntPtr.Zero && _hMux != IntPtr.Zero;
        }

        private static void* GetFieldAddEvtName(IntPtr pShMem)
        {
            return pShMem.ToPointer();
        }
        private static void* GetFieldMuxName(IntPtr pShMem)
        {
            byte* p = ( byte* ) pShMem.ToPointer();
            return ( void* ) (p + TSharedMemLayoutPacked._nMaxNameSz);
        }
        private static void* GetFieldAvailable(IntPtr pShMem)
        {
            byte* p = ( byte* ) pShMem.ToPointer();
            return ( void* ) (p + TSharedMemLayoutPacked._nMaxNameSz * 2);
        }
        private static void* GetFieldMaxCount(IntPtr pShMem)
        {
            byte* p = ( byte* ) pShMem.ToPointer();
            return ( void* ) (p + TSharedMemLayoutPacked._nMaxNameSz * 2 + sizeof( Int32 ));
        }
        private static void* GetFieldFrontIndex(IntPtr pShMem)
        {
            byte* p = ( byte* ) pShMem.ToPointer();
            return ( void* ) (p + TSharedMemLayoutPacked._nMaxNameSz * 2 + sizeof( Int32 ) * 2);
        }
        private static void* GetFieldRearIndex(IntPtr pShMem)
        {
            byte* p = ( byte* ) pShMem.ToPointer();
            return ( void* ) (p + TSharedMemLayoutPacked._nMaxNameSz * 2 + sizeof( Int32 ) * 3);
        }
        private static void* GetFieldDataIndex(IntPtr pShMem, int index)
        {
            byte* p = ( byte* ) pShMem.ToPointer();
            byte* pDat = p + TSharedMemLayoutPacked._nMaxNameSz * 2 + sizeof( Int32 ) * 4;
            Int32* pMaxCnt = ( Int32* ) GetFieldMaxCount( pShMem );
            if ( index < 0 || index >= *pMaxCnt )
                return IntPtr.Zero.ToPointer();

            return ( void* ) (pDat + sizeof( IntPtr ) * index);
        }

        public bool Add(IntPtr pComBuff, bool bHandleOnErr = true)
        {
            bool retStat = false;
            if ( _bConsumer || _bDisposing || _bDisposed || !_bReady )
            {
                if ( bHandleOnErr && pComBuff != IntPtr.Zero ) Marshal.FreeCoTaskMem( pComBuff );
                return retStat;
            }

            if(WaitWinSdkFunctions.WaitForSingleObject(_hMux, (UInt32)WAIT.INFINITE) == (UInt32)WAIT_STATUS.OBJECT_0)
            {
                Int32* pAvailable = ( Int32* ) GetFieldAvailable( _pSharedMem );
                Int32* pMaxCnt = ( Int32* ) GetFieldMaxCount( _pSharedMem );
                Int32* pFrontIndex = ( Int32* ) GetFieldFrontIndex( _pSharedMem );
                Int32* pRearIndex = ( Int32* ) GetFieldRearIndex( _pSharedMem );

                if ( *pAvailable == Convert.ToInt32( false ) )
                {
                    // not available, handle the mem ?
                    if ( bHandleOnErr && pComBuff != IntPtr.Zero ) Marshal.FreeCoTaskMem( pComBuff );
                }
                else
                {
                    int nextFront = *pFrontIndex + 1 >= *pMaxCnt ? 0 : *pFrontIndex + 1;
                    if ( nextFront == *pRearIndex ) // currently full
                    {
                        // error, handle the mem ?
                        if ( bHandleOnErr && pComBuff != IntPtr.Zero ) Marshal.FreeCoTaskMem( pComBuff );
                    }
                    else
                    {
                        // fill data in curr index
                        IntPtr* pp = ( IntPtr* ) GetFieldDataIndex( _pSharedMem, *pFrontIndex );
                        *pp = pComBuff;
                        // index move to next
                        *pFrontIndex = nextFront;
                        // ret status
                        retStat = true;
                    }
                }
                SyncWinSdkFunctions.ReleaseMutex( _hMux );
            }
            else
            {
                if ( bHandleOnErr && pComBuff != IntPtr.Zero ) Marshal.FreeCoTaskMem( pComBuff );
                return retStat;
            }

            // set the event
            if ( retStat && _hAddEvent != IntPtr.Zero )
                EventWinSdkFunctions.Set( _hAddEvent );

            return retStat;
        }

        public IntPtr Consume()
        {
            if ( !_bConsumer || _bDisposing || _bDisposed || !_bReady )
                return IntPtr.Zero;

            IntPtr ret = IntPtr.Zero;
            if ( WaitWinSdkFunctions.WaitForSingleObject( _hMux, ( UInt32 ) WAIT.INFINITE ) == ( UInt32 ) WAIT_STATUS.OBJECT_0 )
            {
                Int32* pAvailable = ( Int32* ) GetFieldAvailable( _pSharedMem );
                Int32* pMaxCnt = ( Int32* ) GetFieldMaxCount( _pSharedMem );
                Int32* pFrontIndex = ( Int32* ) GetFieldFrontIndex( _pSharedMem );
                Int32* pRearIndex = ( Int32* ) GetFieldRearIndex( _pSharedMem );

                if(*pFrontIndex != *pRearIndex)
                {
                    int nextRear = *pRearIndex + 1 >= *pMaxCnt ? 0 : *pRearIndex + 1;
                    // get current rear index data
                    IntPtr* pp = ( IntPtr* ) GetFieldDataIndex( _pSharedMem, *pRearIndex );
                    ret = *pp;
                    // inc to next
                    *pRearIndex = nextRear;
                }
                SyncWinSdkFunctions.ReleaseMutex( _hMux );
            }

            return ret;
        }

        public void Dispose()
        {
            if ( _bDisposing )
                return;
            _bDisposing = true;

            if ( _pSharedMem != IntPtr.Zero ) MemWinSdkFunctions.UnmapViewOfFile( _pSharedMem );
            _pSharedMem = IntPtr.Zero;
            if ( _hSharedMem != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hSharedMem );
            _hSharedMem = IntPtr.Zero;
            if ( _hMux != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hMux );
            _hMux = IntPtr.Zero;
            if ( _hAddEvent != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( _hAddEvent );
            _hAddEvent = IntPtr.Zero;

            _bDisposed = true;
            _bDisposing = false;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack =1)]
    public struct TSharedMemLayoutPacked
    {
        public const Int32 _nMaxNameSz = 32;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = _nMaxNameSz )]
        public byte[] _strNameOfAddEvent;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = _nMaxNameSz )]
        public byte[] _strNameOfMux;
        public Int32 _bAvailable;
        public Int32 _nMaxCount;
        public Int32 _nFrontIndex;
        public Int32 _nRearIndex;
    }
}
