using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Utility;

namespace uIP.Lib.InterPC
{
    internal enum eUSrvProcNewConnState
    {
        NA,
        CreateNewNamePipe,
        WaitConnecting,
        CreateACH,
        ErrHandling,
    }

    public class UPipeCb
    {
        public Thread _threadProc = null;
        public IntPtr _hPipe = IntPtr.Zero;
        public bool _bCHAvailable = false;
        public bool _bPendingIO = false;
        public OVERLAPPED _operlapped = new OVERLAPPED();
        public eUPipeState _State = eUPipeState.NA;
        public UPipeCb() { }
    }

    public class UPipeServer : IDisposable
    {
        private IntPtr INVALID_HANDLE_VALUE = new IntPtr( -1 );
        protected bool _bDisposing = false;
        protected bool _bDisposed = false;

        protected UInt32 _nMaxBuffSize = 0;
        protected string _strGivenName = null;

        private eUSrvProcNewConnState _WaitReqState = eUSrvProcNewConnState.NA;
        private Thread _threadWaitReq = null;
        private Thread _threadRecycle = null;
        private object _hSync = new object();
        private List<UPipeCb> _listConn = new List<UPipeCb>();
        private fpUNamedPipeOpenStatus _fpOpenCallback = null;

        private ManualResetEvent _evtNotifyRecycle = new ManualResetEvent( false );
        private object _hSyncRecycle = new object();
        private List<UPipeCb> _listRecycle = new List<UPipeCb>();

        protected fpUPipeDebug _fpDbg = null;

        public fpUPipeDebug fpDbg { get { return _fpDbg; } set { _fpDbg = value; } }

        public UPipeServer( string pipeName, UInt32 nMaxBuffSize, fpUNamedPipeOpenStatus openStatusReport, fpUPipeDebug dbg )
        {
            _strGivenName = String.IsNullOrEmpty( pipeName ) ? null : String.Copy( pipeName );
            _nMaxBuffSize = nMaxBuffSize <= 0 ? 4096 : nMaxBuffSize;

            _fpOpenCallback = openStatusReport;
            _fpDbg = dbg;

            _threadWaitReq = new Thread( new ThreadStart( WaitConnect ) );
            _threadWaitReq.Start();

            _threadRecycle = new Thread( new ThreadStart( RecycleConn ) );
            _threadRecycle.Start();
        }

        public void Dispose()
        {
            if ( _bDisposing || _bDisposed ) return;
            _bDisposing = true;

            Dispose( true );

            _bDisposed = true;
            _bDisposing = false;
        }

        protected virtual void Dispose( bool disposing )
        {
            // simulate a client connect
            IntPtr hPipe = IntPtr.Zero;
            string fullPipeName = String.Format( @"\\.\pipe\{0}", _strGivenName );
            unsafe
            {
                hPipe = FileIoWinSdkFunctions.CreateFile( fullPipeName,
                                                          ( UInt32 ) EFileAccess.GENERIC_READ |
                                                          ( UInt32 ) EFileAccess.GENERIC_WRITE,
                                                          0,
                                                          null,
                                                          ( UInt32 ) ECreationDisposition.OPEN_EXISTING,
                                                          0,
                                                          IntPtr.Zero );
                if ( hPipe != INVALID_HANDLE_VALUE )
                {
                    MsMethods.WaitFromAppDoEvent( 1 );
                    CommonWinSdkFunctions.CloseHandle( hPipe );
                }
            }

            // close waiting request thread
            if ( _threadWaitReq != null )
            {
                _threadWaitReq.Join();
                _threadWaitReq = null;
            }

            // close recycling thread
            _evtNotifyRecycle.Set();
            if ( _threadRecycle != null )
            {
                _threadRecycle.Join();
                _threadRecycle = null;
            }


            // trigger
            //byte[] dummy = new byte[ 1 ];
            //UInt32 nw = 0;

            //Monitor.Enter( _hSync );
            //try
            //{
            //    // close opened pipe
            //    for ( int i = 0; i < _listConn.Count; i++ )
            //    {
            //        if ( !_listConn[ i ]._bCHAvailable )
            //            continue;
            //        // trigger unblocking
            //        //unsafe
            //        //{
            //        //    fixed ( byte* p8 = dummy )
            //        //    {
            //        //        void* pvoid = ( void* ) p8;
            //        //        FileSystem.WriteFile( _listConn[ i ]._hPipe, new IntPtr( pvoid ), ( UInt32 ) dummy.Length, ref nw, IntPtr.Zero );
            //        //    }
            //        //}
            //    }
            //}
            //finally
            //{
            //    Monitor.Exit( _hSync );
            //}

            MsMethods.WaitFromAppDoEvent( 1 );

            for ( int i = 0; i < _listConn.Count; i++ )
            {
                if ( _listConn[ i ]._threadProc != null )
                {
                    _listConn[ i ]._threadProc.Join();
                    _listConn[ i ]._threadProc = null;
                }
            }
            _listConn.Clear();

            for ( int i = 0; i < _listRecycle.Count; i++ )
            {
                if ( _listRecycle[ i ]._threadProc != null )
                {
                    _listRecycle[ i ]._threadProc.Join();
                    _listRecycle[ i ]._threadProc = null;
                }
            }
            _listRecycle.Clear();

            _evtNotifyRecycle.Close();
            _evtNotifyRecycle = null;

        }

        /// <summary>
        /// This function processes all data from every channel. So, this will be multi-threading call.
        /// Data sync must considerate.
        /// </summary>
        /// <param name="rxDat">buffer to receive</param>
        /// <param name="nRx">actual size fill in buffer</param>
        protected virtual void ProcessRequest( UPipeCb cb, byte[] rxDat, Int32 nRx, out byte[] rspDat, out Int32 nOffsetIdx, out Int32 nDatLen )
        {
            rspDat = null;
            nOffsetIdx = 0;
            nDatLen = 0;

            if ( rxDat != null && rxDat.Length > 0 )
            {
                rspDat = rxDat;
                nOffsetIdx = 0;
                nDatLen = nRx;

                if ( _fpDbg != null )
                {
                    StringBuilder sb = new StringBuilder();
                    List<byte> aline = new List<byte>();
                    bool isClear = false;
                    sb.Append( "--------------------------------------------------------------------------------\n" );
                    sb.AppendFormat( "ThreadID={0}, RxCount={1}\n", cb._threadProc.ManagedThreadId, nRx );
                    for ( int i = 0 ; i < nRx ; i++ )
                    {
                        isClear = false;
                        if ( (i % 16) == 0 && i != 0 )
                        {
                            StringBuilder ff = new StringBuilder();
                            for ( int j = 0 ; j < aline.Count ; j++ )
                            {
                                if ( j == 0 ) ff.AppendFormat( "{0:X2}", aline[ j ] );
                                else ff.AppendFormat( " {0:X2}", aline[ j ] );
                            }
                            ff.Append( "    " );
                            for ( int j = 0 ; j < aline.Count ; j++ ) { ff.AppendFormat( "{0}", aline[ j ] >= ( byte ) (' ') && aline[ j ] <= ( byte ) ('~') ? ( char ) aline[ j ] : '.' ); }
                            sb.AppendFormat( "{0}\n", ff.ToString() );
                            isClear = true;
                        }

                        if ( isClear )
                            aline.Clear();
                        aline.Add( rxDat[ i ] );

                    }
                    if ( aline.Count > 0 )
                    {
                        StringBuilder ff = new StringBuilder();
                        for ( int j = 0 ; j < 16 ; j++ )
                        {
                            if ( j < aline.Count )
                            {
                                if ( j == 0 ) ff.AppendFormat( "{0:X2}", aline[ j ] );
                                else ff.AppendFormat( " {0:X2}", aline[ j ] );
                            }
                            else
                                ff.Append("   ");
                        }
                        ff.Append( "    " );
                        for ( int j = 0 ; j < aline.Count ; j++ )
                        {
                            ff.AppendFormat( "{0}", aline[ j ] >= ( byte ) (' ') && aline[ j ] <= ( byte ) ('~') ? ( char ) aline[ j ] : '.' );
                        }
                        sb.AppendFormat( "{0}\n", ff.ToString() );
                    }
                    sb.Append( "--------------------------------------------------------------------------------" );
                    _fpDbg( sb.ToString(), 0 );
                }
            }
        }

        private void WaitConnect()
        {
            if ( String.IsNullOrEmpty( _strGivenName ) )
                return;
            //IntPtr hPipe = IntPtr.Zero;
            IntPtr hCurrentWaitPipe = IntPtr.Zero;
            string pipeName = String.Format( @"\\.\pipe\{0}", _strGivenName );
            if ( _fpDbg != null ) _fpDbg( String.Format( "Named Pipe({0}) began...", _strGivenName ), 0 );

            bool bReadyCall = false;

            while ( !_bDisposing && !_bDisposed )
            {
                switch ( _WaitReqState )
                {
                    default:
                        _WaitReqState = eUSrvProcNewConnState.CreateNewNamePipe;
                        break;

                    case eUSrvProcNewConnState.CreateNewNamePipe:
                        unsafe
                        {
                            hCurrentWaitPipe = PipeWinSdkFunctions.CreateNamedPipe( pipeName,
                                                                                    ( UInt32 ) CreatePipeOpenModeFlags.PIPE_ACCESS_DUPLEX,
                                                                                    ( UInt32 ) CreatePipeModeFlags.PIPE_TYPE_MESSAGE |
                                                                                    ( UInt32 ) CreatePipeModeFlags.PIPE_READMODE_MESSAGE |
                                                                                    ( UInt32 ) CreatePipeModeFlags.PIPE_NOWAIT,
                                                                                    255,
                                                                                    _nMaxBuffSize,
                                                                                    _nMaxBuffSize,
                                                                                    5000, // 0
                                                                                    null );
                        }
                        if ( hCurrentWaitPipe == INVALID_HANDLE_VALUE )
                        {
                            //if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::WaitConnect] invalid pipe handle." ), 0 );
                            //Thread.Sleep( 500 );
                            break;
                        }
                        // notify out once 
                        if ( !bReadyCall )
                        {
                            if ( _fpOpenCallback != null )
                                _fpOpenCallback( _strGivenName, true );
                            bReadyCall = true;
                        }
                        _WaitReqState = eUSrvProcNewConnState.WaitConnecting;
                        break;

                    case eUSrvProcNewConnState.WaitConnecting:
                        unsafe
                        {
                            if ( PipeWinSdkFunctions.ConnectNamedPipe( hCurrentWaitPipe, null ) )
                            {
                                _WaitReqState = eUSrvProcNewConnState.CreateACH;
                            }
                            else
                            {
                                UInt32 code = CommonWinSdkFunctions.GetLastError();
                                if ( code == ( UInt32 ) WinErrorCode.ERROR_PIPE_CONNECTED )
                                {
                                    // it's ok
                                    _WaitReqState = eUSrvProcNewConnState.CreateACH;
                                }
                                else if ( code == ( UInt32 ) WinErrorCode.ERROR_PIPE_LISTENING )
                                {
                                    Thread.Sleep( 500 );
                                    break; // listening
                                }
                                else
                                {
                                    _WaitReqState = eUSrvProcNewConnState.ErrHandling;
                                }
                            }
                        }
                        break;

                    case eUSrvProcNewConnState.ErrHandling:
                        if ( hCurrentWaitPipe != null )
                        {
                            CommonWinSdkFunctions.CloseHandle( hCurrentWaitPipe );
                            hCurrentWaitPipe = IntPtr.Zero;
                        }
                        _WaitReqState = eUSrvProcNewConnState.CreateNewNamePipe;
                        break;

                    case eUSrvProcNewConnState.CreateACH:
                        {
                            UPipeCb cb = null;
                            Monitor.Enter( _hSync );
                            try
                            {
                                cb = new UPipeCb();
                                cb._hPipe = hCurrentWaitPipe;
                                //cb._operlapped.hEvent = WindowsEvent.Create( true, true, null );
                                cb._bCHAvailable = true;
                                cb._threadProc = new Thread( new ParameterizedThreadStart( ProcRequest ) );
                                //cb._threadProc = new Thread( new ParameterizedThreadStart( ProcessReqOverlapped ) );
                                _listConn.Add( cb );
                                if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::WaitConnect] create a pipe {0} channel.", cb._hPipe ), 0 );
                            }
                            finally
                            {
                                Monitor.Exit( _hSync );
                            }
                            // begin channel rx
                            cb._threadProc.Start( ( object ) cb );

                            hCurrentWaitPipe = IntPtr.Zero;
                            _WaitReqState = eUSrvProcNewConnState.CreateNewNamePipe;
                        }
                        break;
                }
            } // end-while

            if ( hCurrentWaitPipe != IntPtr.Zero )
            {
                CommonWinSdkFunctions.CloseHandle( hCurrentWaitPipe );
                hCurrentWaitPipe = IntPtr.Zero;
            }
        }

        private unsafe bool Connect2NewClient( IntPtr hPipe, OVERLAPPED* po )
        {
            bool bConnected = false, bPendingIO = false;

            // start an overlapped connection for this pipe instance
            bConnected = PipeWinSdkFunctions.ConnectNamedPipe( hPipe, po );

            UInt32 code = CommonWinSdkFunctions.GetLastError();

            // overlapped ConnectNamedPipe should return zero
            if ( bConnected )
            {
                return false;
            }

            switch ( code )
            {
                // the overlapped connection in progress
                case ( UInt32 ) WinErrorCode.ERROR_IO_PENDING:
                    bPendingIO = true;
                    break;

                // client is already connected, so signal an event
                case ( UInt32 ) WinErrorCode.ERROR_PIPE_CONNECTED:
                    EventWinSdkFunctions.Set( po->hEvent );
                    break;

                default:
                    //Console.WriteLine( String.Format( "[PipeOverlapSrv::Connect2NewClient] ConnectNamedPipe fail with code({0}).", code ) );
                    if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::NewClient] ConnectNamedPipe fail with code({0}).", code ), 0 );
                    return false;

            }
            return bPendingIO;
        }

        private void DisconnectAndReconnect( UPipeCb cb )
        {
            if ( cb == null )
                return;

            // Disconnect the pipe instance
            if ( !PipeWinSdkFunctions.DisconnectNamedPipe( cb._hPipe ) )
            {
                //Console.WriteLine( String.Format( "[PipeOverlapSrv::DisconnectAndReconnect] DisconnectNamedPipe with error code({0}).", DebuggingErrorHandling.GetLastError() ) );
                if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::Disconnect] DisconnectNamedPipe with error code({0}).", CommonWinSdkFunctions.GetLastError() ), 0 );
            }

            // Call a subroutine to connect to the new client.
            unsafe
            {
                fixed ( OVERLAPPED* pol = & cb._operlapped )
                {
                    cb._bPendingIO = Connect2NewClient( cb._hPipe, pol );
                }
            }
            cb._State = cb._bPendingIO ? eUPipeState.Connecting : eUPipeState.Reading;
            if ( _fpDbg != null ) _fpDbg( "[UPipeServer::Disconnect] action done!", 0 );
        }


        private void ProcessReqOverlapped( object param )
        {
            if ( param == null )
                return;
            UPipeCb cb = ( UPipeCb ) param;
            //bool bSucc = false;
            byte[] buffer = new byte[ _nMaxBuffSize ];
            UInt32 nRead = 0;
            byte[] rspDat = null;
            Int32 offsetIdx = 0;
            Int32 datLen = 0;

            // change to blocking
            //UInt32[] mode = new UInt32[ 1 ] { ( UInt32 ) CreatePipeModeFlags.PIPE_WAIT };
            //unsafe
            //{
            //    fixed ( UInt32* p32 = mode )
            //    {
            //        void* pvoid = ( void* ) p32;
            //        bSucc = Pipe.SetNamedPipeHandleState( cb._hPipe, new IntPtr( pvoid ), IntPtr.Zero, IntPtr.Zero );
            //    }
            //}
            //if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::ProcessReqOverlapped] switch {0} pipe to wait = {1}.", cb._hPipe, bSucc.ToString() ), 0 );

            // connect to new client
            unsafe
            {
                fixed ( OVERLAPPED* ovlp = &cb._operlapped )
                {
                    cb._bPendingIO = Connect2NewClient( cb._hPipe, ovlp );
                }
            }
            if ( !cb._bPendingIO )
                cb._State = eUPipeState.Reading;

            // start an overlapped tx/ rx
            UInt32 dwWait = 0, dwErr = 0;
            bool bSuccess = false;
            UInt32 cbRet = 0;
            bool bErrBreak = false;
            while ( !_bDisposing && !_bDisposed && !bErrBreak )
            {
                // Wait for the event object to be signaled, indicating 
                // completion of an overlapped read, write, or 
                // connect operation. 
                dwWait = WaitWinSdkFunctions.WaitForSingleObject( cb._operlapped.hEvent,
                                                       1000 );
                if ( _bDisposing || _bDisposed )
                    break;
                if ( dwWait != ( UInt32 ) WAIT_STATUS.OBJECT_0 )
                    continue;

                // GetClassConfiguration the result if the operation was pending.
                if ( cb._bPendingIO )
                {
                    bSuccess = CommonWinSdkFunctions.GetOverlappedResult( cb._hPipe,          // handle to pipe
                                                                ref cb._operlapped, // OVERLAPPED structure
                                                                ref cbRet,          // bytes transferred
                                                                    false );            // do not wait
                    switch ( cb._State )
                    {
                        // Pending connect operation
                        case eUPipeState.Connecting:
                            if ( !bSuccess )
                            {
                                //Console.WriteLine( String.Format( "[PipeOverlapSrv::ProcessRequest] error code({0}).", DebuggingErrorHandling.GetLastError() ) );
                                if ( _fpDbg != null )
                                    _fpDbg( String.Format( "UPipeServer::ProcessReqOverlapped] error code({0}).", CommonWinSdkFunctions.GetLastError() ), 0 );
                                bErrBreak = true;
                                continue;
                            }
                            cb._State = eUPipeState.Reading;
                            break;

                        // Pending read operation
                        case eUPipeState.Reading:
                            if ( !bSuccess )
                            {
                                UInt32 errcode = CommonWinSdkFunctions.GetLastError();
                                if ( errcode != ( UInt32 ) WinErrorCode.ERROR_IO_INCOMPLETE )
                                    DisconnectAndReconnect( cb );
                                continue;
                            }
                            if ( cbRet > 0 )
                                cb._State = eUPipeState.Writing;
                            break;

                        // Pending write operation
                        case eUPipeState.Writing:
                            if ( !bSuccess || cbRet != ( UInt32 ) datLen )
                            {
                                DisconnectAndReconnect( cb );
                                continue;
                            }
                            cb._State = eUPipeState.Reading;
                            break;

                        default:
                            //Console.WriteLine( String.Format( "[PipeOverlapSrv::ProcessRequest] invalid state 1." ) );
                            if ( _fpDbg != null )
                                _fpDbg( "[UPipeServer::ProcessRequest] invalid state 1.", 0 );
                            return;
                    }
                }

                // The pipe state determines which operation to do next.
                switch ( cb._State )
                {
                    // READING_STATE: 
                    // The pipe instance is connected to the client 
                    // and is ready to read a request from the client. 
                    case eUPipeState.Reading:
                        unsafe
                        {
                            fixed ( byte* p8 = buffer )
                            {
                                fixed ( OVERLAPPED* pol = &cb._operlapped )
                                {
                                    void* pvoid1 = ( void* ) p8;
                                    void* pvoid2 = ( void* ) pol;
                                    bSuccess = FileIoWinSdkFunctions.ReadFile( cb._hPipe,
                                                                         new IntPtr( pvoid1 ),
                                                                        ( UInt32 ) buffer.Length,
                                                                    ref nRead,
                                                                        new IntPtr( pvoid2 ) );
                                }
                            }
                        }
                        // The read operation completed successfully.
                        if ( bSuccess && nRead != 0 )
                        {
                            // call function to process
                            ProcessRequest( cb, buffer, ( Int32 ) nRead, out rspDat, out offsetIdx, out datLen );

                            cb._bPendingIO = false;
                            cb._State = rspDat != null && datLen > 0 ? eUPipeState.Writing : eUPipeState.Reading;
                            continue;
                        }
                        // The read operation is still pending.
                        dwErr = CommonWinSdkFunctions.GetLastError();
                        if ( !bSuccess && (dwErr == ( UInt32 ) WinErrorCode.ERROR_IO_PENDING ||
                                            dwErr == ( UInt32 ) WinErrorCode.ERROR_NO_DATA) )

                        {
                            cb._bPendingIO = true;
                            continue;
                        }
                        // An error occurred; disconnect from the client.
                        DisconnectAndReconnect( cb );
                        break;

                    // WRITING_STATE: 
                    // The request was successfully read from the client. 
                    // GetClassConfiguration the reply data and write it to the client.
                    case eUPipeState.Writing:
                        unsafe
                        {
                            fixed ( byte* p8 = rspDat )
                            {
                                fixed ( OVERLAPPED* pol = &cb._operlapped )
                                {
                                    void* pvoid1 = ( void* ) (&p8[ offsetIdx ]);
                                    void* pvoid2 = ( void* ) pol;
                                    bSuccess = FileIoWinSdkFunctions.WriteFile( cb._hPipe,
                                                                          new IntPtr( pvoid1 ),
                                                                         ( UInt32 ) datLen,
                                                                     ref cbRet,
                                                                         new IntPtr( pvoid2 ) );
                                }
                            }
                        }
                        // The write operation completed successfully.
                        if ( bSuccess && cbRet == ( UInt32 ) datLen )
                        {
                            cb._bPendingIO = false;
                            cb._State = eUPipeState.Reading;
                            continue;
                        }
                        // The write operation is still pending.
                        dwErr = CommonWinSdkFunctions.GetLastError();
                        if ( !bSuccess && (dwErr == ( UInt32 ) WinErrorCode.ERROR_IO_PENDING) )
                        {
                            cb._bPendingIO = true;
                            continue;
                        }
                        // An error occurred; disconnect from the client.
                        DisconnectAndReconnect( cb );
                        break;

                    default:
                        //Console.WriteLine( "[UPipeServer::ProcessReqOverlapped] invalid state 2." );
                        if ( _fpDbg != null )
                            _fpDbg( "[UPipeServer::ProcessReqOverlapped] invalid state 2.", 0 );
                        return;
                }
            }

            cb._bCHAvailable = false;
            if ( cb._operlapped.hEvent != IntPtr.Zero )
                CommonWinSdkFunctions.CloseHandle( cb._operlapped.hEvent );
            //FileSystem.FlushFileBuffers( cb._hPipe );
            PipeWinSdkFunctions.DisconnectNamedPipe( cb._hPipe );
            CommonWinSdkFunctions.CloseHandle( cb._hPipe );

            if ( _fpDbg != null )
                _fpDbg( String.Format( "[UPipeServer::ProcessReqOverlapped] close pipe {0}.", cb._hPipe ), 0 );

            if ( !_bDisposing && !_bDisposed )
            {
                // remove from connecting list
                Monitor.Enter( _hSync );
                try
                {
                    if ( _listConn.Contains( cb ) )
                        _listConn.Remove( cb );
                }
                finally
                {
                    Monitor.Exit( _hSync );
                }

                // add to recycling list
                Monitor.Enter( _hSyncRecycle );
                try
                {
                    _listRecycle.Add( cb );
                }
                finally
                {
                    Monitor.Exit( _hSyncRecycle );
                }

                // trigger recycling thread
                try
                {
                    _evtNotifyRecycle.Set();
                }
                catch { }
            }

        }


        private void ProcRequest( object param )
        {
            if ( param == null )
                return;

            UPipeCb cb = ( UPipeCb ) param;

            IntPtr hWorkingPipe = cb._hPipe;
            bool bSucc = false;
            byte[] buffer = new byte[ _nMaxBuffSize ];
            UInt32 nRead = 0;
            byte[] finBuffer = null;
            bool bSuccReading = true;

            byte[] rspDat = null;
            Int32 offsetIdx = 0;
            Int32 datLen = 0;
            UInt32 nWrite = 0;

            if ( buffer == null )
            {
                cb._bCHAvailable = false;
                CommonWinSdkFunctions.CloseHandle( hWorkingPipe ); // close handle
                return;
            }

            // change to blocking
            //UInt32[] mode = new UInt32[ 1 ] { ( UInt32 ) CreatePipeModeFlags.PIPE_WAIT };
            //unsafe
            //{
            //    fixed ( UInt32* p32 = mode )
            //    {
            //        void* pvoid = ( void* ) p32;
            //        bSucc = Pipe.SetNamedPipeHandleState( hWorkingPipe, new IntPtr( pvoid ), IntPtr.Zero, IntPtr.Zero );
            //    }
            //}
            //if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::ProcRequest] switch {0} pipe to wait = {1}.", hWorkingPipe, bSucc.ToString() ), 0 );

            //COMMTIMEOUTS[] timeout = new COMMTIMEOUTS[ 1 ];
            //unsafe
            //{
            //    fixed ( COMMTIMEOUTS* pout = timeout )
            //    {
            //        bSucc = DeviceIO.GetCommTimeouts( hWorkingPipe, pout );
            //        timeout[ 0 ].ReadIntervalTimeout = 100;
            //        timeout[ 0 ].ReadTotalTimeoutConstant = UInt32.MaxValue;
            //        timeout[ 0 ].ReadTotalTimeoutMultiplier = UInt32.MaxValue;
            //        bSucc = DeviceIO.SetCommTimeouts( hWorkingPipe, pout );
            //    }
            //}

            while ( !_bDisposing && !_bDisposed )
            {
                // read first
                unsafe
                {
                    // blocking IO will make server closed normally, so ReadFile() here must be non-blocking.
                    fixed ( byte* p8 = buffer )
                    {
                        void* pvoid = ( void* ) p8;
                        bSucc = FileIoWinSdkFunctions.ReadFile( hWorkingPipe, new IntPtr( pvoid ), _nMaxBuffSize, ref nRead, IntPtr.Zero );
                    }
                }

                if ( _bDisposing || _bDisposed )
                    break;
                if ( !bSucc )
                {
                    UInt32 errcode = CommonWinSdkFunctions.GetLastError();
                    bSuccReading = false;

                    if ( _fpDbg != null && errcode != ( UInt32 ) WinErrorCode.ERROR_NO_DATA ) _fpDbg( String.Format( "[UPipeServer::ProcRequest] {0} error occur on reading with code({1}).", cb._hPipe, errcode ), 0 );
                    if ( errcode == ( UInt32 ) WinErrorCode.ERROR_NO_DATA )
                    {
                        Thread.Sleep( 5 );
                        continue;
                    }
                    else if ( errcode == ( UInt32 ) WinErrorCode.ERROR_MORE_DATA )
                    {
                        UInt32 nTotal = nRead;
                        byte[] firstArr = new byte[ nRead ];
                        List<byte[]> allRx = new List<byte[]>();
                        byte[] tmpBuff = null;
                        UInt32 nTmpRead = 0;
                        // process 1st rx
                        unsafe
                        {
                            fixed ( byte* p8 = firstArr )
                            {
                                void* pvoid = ( void* ) p8;
                                Marshal.Copy( buffer, 0, new IntPtr( pvoid ), ( Int32 ) nRead );
                            }
                        }
                        allRx.Add( firstArr );
                        bSuccReading = true;
                        // continue remaining
                        while ( true )
                        {
                            tmpBuff = new byte[ _nMaxBuffSize ];
                            nTmpRead = 0;
                            unsafe
                            {
                                fixed ( byte* p8 = tmpBuff )
                                {
                                    void* pvoid = ( void* ) p8;
                                    bSucc = FileIoWinSdkFunctions.ReadFile( hWorkingPipe, new IntPtr( pvoid ), _nMaxBuffSize, ref nTmpRead, IntPtr.Zero );
                                }
                            }
                            // add rx buffer
                            nTotal += nTmpRead;
                            allRx.Add( tmpBuff );
                            // get code
                            errcode = CommonWinSdkFunctions.GetLastError();
                            if ( bSucc )
                            {
                                // merge
                                finBuffer = new byte[ nTotal ];
                                nRead = nTotal;
                                unsafe
                                {
                                    fixed( byte* p8 = finBuffer )
                                    {
                                        byte* pCurr = p8;
                                        for( int i = 0 ; i < allRx.Count ; i++, pCurr += _nMaxBuffSize, nTotal -= _nMaxBuffSize )
                                        {
                                            Marshal.Copy( allRx[ i ], 0, new IntPtr( ( void* ) pCurr ), ( Int32 ) (nTotal > _nMaxBuffSize ? _nMaxBuffSize : nTotal) );
                                        }
                                    }
                                }
                                break;
                            }
                            else if ( errcode != ( UInt32 ) WinErrorCode.ERROR_MORE_DATA )
                            {
                                if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::ProcRequest] proc more data with error code {0}", errcode ), 0 );
                                bSuccReading = false;
                                break;
                            }
                        }
                    }
                    // go here
                    // - Accept reading MORE_DATA successfully
                    // - Otherwise as an error, trigger disconnect
                    if ( !bSuccReading ) // trigger error disconnect
                    {
                        cb._bCHAvailable = false;
                        break;
                    }
                }
                else
                    finBuffer = buffer;

                UInt32 code = 0;

                ProcessRequest( cb, finBuffer, ( Int32 ) nRead, out rspDat, out offsetIdx, out datLen );
                finBuffer = null;

                if ( rspDat != null && datLen > 0 )
                {
                    //if ( offsetIdx < 0 || ( offsetIdx + datLen ) >= rspDat.Length )
                    //    continue;

                    unsafe
                    {
                        if ( datLen > _nMaxBuffSize )
                        {
                            // switch to blocking
                            UInt32[] mode = new UInt32[ 1 ] { ( UInt32 ) CreatePipeModeFlags.PIPE_WAIT };
                            fixed ( UInt32* p32 = mode )
                            {
                                void* pvoid = ( void* ) p32;
                                PipeWinSdkFunctions.SetNamedPipeHandleState( hWorkingPipe, new IntPtr( pvoid ), IntPtr.Zero, IntPtr.Zero );
                            }
                        }
                        // write to client
                        fixed ( byte* p8 = rspDat )
                        {
                            void* pvoid = ( void* ) (&p8[ offsetIdx ]);
                            datLen = offsetIdx < 0 || (offsetIdx + datLen) > rspDat.Length ? 0 : datLen; // respond anyway regardless error
                            bSucc = FileIoWinSdkFunctions.WriteFile( hWorkingPipe, new IntPtr( pvoid ), ( UInt32 ) datLen, ref nWrite, IntPtr.Zero );
                        }
                    }
                    code = CommonWinSdkFunctions.GetLastError();
                    if ( !bSucc || nWrite != ( UInt32 ) datLen )
                    {
                        cb._bCHAvailable = false;
                        if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::ProcRequest] {0} error occur on writing with code({1}).", cb._hPipe, code ), 0 );
                        break;
                    }
                    if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::ProcRequest] pipe {0} tx size {1} with code({2}).", cb._hPipe, nWrite, code ), 0 );
                    if ( datLen > _nMaxBuffSize )
                    {
                        // switch back
                        unsafe
                        {
                            // switch to nonblocking
                            UInt32[] mode = new UInt32[ 1 ] { ( UInt32 ) CreatePipeModeFlags.PIPE_NOWAIT };
                            fixed ( UInt32* p32 = mode )
                            {
                                void* pvoid = ( void* ) p32;
                                PipeWinSdkFunctions.SetNamedPipeHandleState( hWorkingPipe, new IntPtr( pvoid ), IntPtr.Zero, IntPtr.Zero );
                            }
                        }
                    }
                }
            }

            if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::ProcRequest] close pipe {0}.", hWorkingPipe ), 0 );

            cb._bCHAvailable = false;
            FileIoWinSdkFunctions.FlushFileBuffers( hWorkingPipe );
            PipeWinSdkFunctions.DisconnectNamedPipe( hWorkingPipe );
            CommonWinSdkFunctions.CloseHandle( hWorkingPipe );

            if ( !_bDisposing && !_bDisposed )
            {
                // remove from connecting list
                Monitor.Enter( _hSync );
                try
                {
                    if ( _listConn.Contains( cb ) )
                        _listConn.Remove( cb );
                }
                finally
                {
                    Monitor.Exit( _hSync );
                }

                // add to recycling list
                Monitor.Enter( _hSyncRecycle );
                try
                {
                    _listRecycle.Add( cb );
                }
                finally
                {
                    Monitor.Exit( _hSyncRecycle );
                }

                // trigger recycling thread
                try
                {
                    _evtNotifyRecycle.Set();
                }
                catch { }
            }
        }

        private void RecycleConn()
        {
            while ( !_bDisposing && !_bDisposed )
            {
                _evtNotifyRecycle.WaitOne();
                Monitor.Enter( _hSyncRecycle );
                try
                {
                    while ( _listRecycle.Count > 0 )
                    {
                        UPipeCb cb = _listRecycle[ 0 ];
                        _listRecycle.RemoveAt( 0 );

                        if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeServer::RecycleConn] recycle pipe {0}.", cb._hPipe ), 0 );

                        cb._threadProc.Join();
                        cb._threadProc = null;
                        cb = null;
                    }
                }
                finally
                {
                    Monitor.Exit( _hSyncRecycle );
                }
                _evtNotifyRecycle.Reset();
            }

            //Monitor.Enter( _hSyncRecycle );
            //try
            //{
            //    for ( int i = 0; i < _listRecycle.Count; i++ )
            //    {
            //        _listRecycle[ i ]._threadProc.Join();
            //        _listRecycle[ i ]._threadProc = null;
            //    }
            //    _listRecycle.Clear();
            //}
            //finally
            //{
            //    Monitor.Exit( _hSyncRecycle );
            //}

        }
    }
}
