using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using uIP.Lib.MarshalWinSDK;

namespace uIP.Lib.InterPC
{
    internal class UPipeClientTxReq
    {
        internal byte[] _TxBuff = null;
        internal Int32 _nOffsetIdx = 0;
        internal Int32 _nTxLen = 0;

        internal fpUCltProcPipeRxDat _fpProxRx = null;

        internal UPipeClientTxReq() { }
    }

    public class UPipeClient : IPipeClientComm, IDisposable
    {
        private IntPtr INVALID_HANDLE_VALUE = new IntPtr( -1 );
        protected bool _bReady = false;
        protected bool _bDisposing = false;
        protected bool _bDisposed = false;

        private Thread _threadProc = null;
        private IntPtr _hPipe = IntPtr.Zero;
        private fpUNamedPipeOpenStatus _fpOpenedStatusNotify = null;

        private string _strPipeName = null;
        private byte[] _BufferRx = null;

        private ManualResetEvent _evtNotify = new ManualResetEvent( false );
        private object _hSync = new object();
        private List<UPipeClientTxReq> _listReq = new List<UPipeClientTxReq>();

        protected fpUPipeDebug _fpDbg = null;

        public bool AbleToTxRx { get { return _bReady && !_bDisposing && !_bDisposed; } }
        public bool Ready { get { return AbleToTxRx; } }

        public UPipeClient( string pipeName, Int32 nMaxRx, fpUNamedPipeOpenStatus fpOpenedNotify, fpUPipeDebug fpDbg )
        {
            _strPipeName = String.IsNullOrEmpty( pipeName ) ? null : String.Copy( pipeName );
            _BufferRx = new byte[ nMaxRx <= 0 ? 4096 : nMaxRx ];

            _fpOpenedStatusNotify = fpOpenedNotify;
            _fpDbg = fpDbg;

            _threadProc = new Thread( new ThreadStart( Process ) );
            _threadProc.Start();
        }

        public void Dispose()
        {
            if ( _bDisposing || _bDisposed )
                return;
            _bDisposing = true;

            Dispose( true );

            _bDisposed = true;
            _bDisposing = false;

        }

        public bool Add( byte[] txBuff, Int32 offset, Int32 len, fpUCltProcPipeRxDat fp )
        {
            if ( _bDisposing || _bDisposed )
                return false;

            if ( txBuff == null || len <= 0 ) return false;
            if ( offset < 0 || offset >= txBuff.Length ) return false;
            if ( ( offset + len ) > txBuff.Length ) return false;

            Monitor.Enter( _hSync );
            try
            {
                UPipeClientTxReq req = new UPipeClientTxReq();
                req._TxBuff = txBuff;
                req._nOffsetIdx = offset;
                req._nTxLen = len;
                req._fpProxRx = fp;
                _listReq.Add( req );
            }
            finally
            {
                Monitor.Exit( _hSync );
            }

            _evtNotify.Set();

            return true;
        }

        protected void Dispose( bool bDisposing )
        {
            // stop process
            _evtNotify.Set();
            _threadProc.Join();
            _threadProc = null;

            Monitor.Enter( _hSync );
            try
            {
                // rsp
                for ( int i = 0; i < _listReq.Count; i++ )
                {
                    if ( _listReq[ i ] != null && _listReq[ i ]._fpProxRx != null )
                        _listReq[ i ]._fpProxRx( eUPipeState.NA, null, 0, 0 );
                }
                _listReq.Clear();
            }
            finally { Monitor.Exit( _hSync ); }

            // close event
            _evtNotify.Close();
            _evtNotify = null;

            // close pipe
            CommonWinSdkFunctions.CloseHandle( _hPipe );
            _hPipe = IntPtr.Zero;
        }

        private bool Initialize()
        {
            if ( String.IsNullOrEmpty( _strPipeName ) )
                return false;
            // connect to pipe first
            IntPtr hPipe = IntPtr.Zero;
            string fullPipeName = String.Format( @"\\.\pipe\{0}", _strPipeName );

            while( !_bDisposing && !_bDisposed )
            {
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
                        break;
                }

                // check error code
                if ( CommonWinSdkFunctions.GetLastError() != ( UInt32 ) WinErrorCode.ERROR_PIPE_BUSY )
                {
                    if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeClient::Initialize] could not open pipe with code {0}.", CommonWinSdkFunctions.GetLastError() ), 0 );
                    return false;
                }

                //if ( !PipeWinSdkFunctions.WaitNamedPipe( _strPipeName, 5000 ) )
                if ( !PipeWinSdkFunctions.WaitNamedPipe( fullPipeName, 5000 ) )
                    return false;
            }

            if ( _bDisposing || _bDisposed )
            {
                if ( hPipe != IntPtr.Zero && hPipe != INVALID_HANDLE_VALUE )
                    CommonWinSdkFunctions.CloseHandle( hPipe );
                return false;
            }

            // pipe connected, change to message-read mode
            UInt32[] dwMode = new UInt32[ 1 ] { ( UInt32 ) CreatePipeModeFlags.PIPE_READMODE_MESSAGE };
            bool bSucc = false;
            unsafe
            {
                fixed ( UInt32* p32 = dwMode )
                {
                    void* pvoid = ( void* ) p32;
                    bSucc = PipeWinSdkFunctions.SetNamedPipeHandleState( hPipe,
                                                                         new IntPtr( pvoid ),
                                                                         IntPtr.Zero,
                                                                         IntPtr.Zero );
                }
            }
            if ( !bSucc )
            {
                CommonWinSdkFunctions.CloseHandle( hPipe );
                if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeClient::Initialize] cannot switch mode to PIPE_READMODE_MESSAGE." ), 0 );
                return false;
            }

            _hPipe = hPipe;
            return true;
        }

        private void Process()
        {
            _bReady = Initialize();
            if ( _fpOpenedStatusNotify != null )
                _fpOpenedStatusNotify( _strPipeName, _bReady );
            if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeClient::Process] create pipe {0} with status {1}.", _hPipe, _bReady.ToString() ), 0 );
            if ( !_bReady )
                return;

            Int32 state = 0;
            UPipeClientTxReq curr = null;
            bool bSucc = false;
            UInt32 nTx = 0, nRx = 0;
            List<byte[]> listRx = new List<byte[]>();
            bool bExit = false;
            UInt32 code = 0;

            while ( !_bDisposing && !_bDisposed && !bExit )
            {
                switch ( state )
                {
                    // wait request
                    case 0:
                        if ( _listReq.Count > 0 )
                        {
                            Monitor.Enter( _hSync );
                            try { curr = _listReq[ 0 ]; _listReq.RemoveAt( 0 ); }
                            finally { Monitor.Exit( _hSync ); }
                            state = 1;
                        }
                        else
                        {
                            if ( _evtNotify.WaitOne( 20000, true ) )
                                _evtNotify.Reset();
                        }
                        break;

                    // tx request
                    case 1:
                        if ( curr == null || curr._TxBuff == null || curr._nTxLen <= 0 )
                        {
                            curr = null;
                            state = 0; break;
                        }
                        unsafe
                        {
                            fixed ( byte* p8 = curr._TxBuff )
                            {
                                void* pvoid = ( void* ) ( &p8[ curr._nOffsetIdx ] );
                                // write file
                                bSucc = FileIoWinSdkFunctions.WriteFile( _hPipe, new IntPtr( pvoid ), ( UInt32 ) curr._nTxLen, ref nTx, IntPtr.Zero );
                            }
                        }
                        if ( !bSucc )
                        {
                            UInt32 dwErr = CommonWinSdkFunctions.GetLastError();
                            bExit = true;
                            _bReady = false;

                            if ( _fpDbg != null )
                                _fpDbg( String.Format( "[UPipeClient::Process] tx data error with code({0}) & connection going to failure.", dwErr ), 0 );
                            if ( curr._fpProxRx != null )
                                curr._fpProxRx( eUPipeState.TxError, null, 0, dwErr );
                            continue;
                        }
                        state = 2;
                        listRx.Clear();

                        break;

                    // rx response
                    case 2:
                        unsafe
                        {
                            fixed ( byte* p8 = _BufferRx )
                            {
                                void* pvoid = ( void* ) p8;
                                bSucc = FileIoWinSdkFunctions.ReadFile( _hPipe, new IntPtr( pvoid ), ( UInt32 ) _BufferRx.Length, ref nRx, IntPtr.Zero );
                            }
                        }
                        code = CommonWinSdkFunctions.GetLastError();
                        if ( !bSucc && code != ( UInt32 ) WinErrorCode.ERROR_MORE_DATA )
                        {
                            if ( curr._fpProxRx != null )
                                curr._fpProxRx( eUPipeState.RxError, null, 0, code );
                            if ( _fpDbg != null ) _fpDbg( String.Format( "[UPipeClient::Process] rx error with code({0}).", code ), 0 );
                            curr = null;
                            state = 0;
                            break;
                        }
                        if ( bSucc )
                        {
                            byte[] rspbuff = null;
                            Int32 rspBuffUsed = 0;
                            if ( listRx.Count <= 0 )
                            {
                                rspbuff = _BufferRx;
                                rspBuffUsed = ( Int32 ) nRx;
                            }
                            else
                            {
                                // final ones
                                if(nRx > 0)
                                {
                                    byte[] bk = new byte[ nRx ];
                                    unsafe
                                    {
                                        fixed ( byte* p8 = bk )
                                        {
                                            void* pvoid = ( void* ) p8;
                                            Marshal.Copy( _BufferRx, 0, new IntPtr( pvoid ), ( Int32 ) nRx );
                                        }
                                    }
                                    listRx.Add( bk );
                                }
                                // calc total buffer len
                                Int32 nTotal = 0;
                                for ( int i = 0; i < listRx.Count; i++ )
                                    nTotal += listRx[ i ].Length;
                                rspBuffUsed = nTotal;
                                // alloc buff
                                byte[] alloc = new byte[ nTotal ];
                                nTotal = 0;
                                for ( int i = 0; i < listRx.Count; i++ )
                                {
                                    unsafe
                                    {
                                        fixed ( byte* p8 = alloc )
                                        {
                                            void* pvoid = ( void* ) ( &p8[ nTotal ] );
                                            Marshal.Copy( listRx[ i ], 0, new IntPtr( pvoid ), listRx[ i ].Length );
                                        }
                                    }
                                    nTotal += listRx[ i ].Length;
                                }
                                rspbuff = alloc;
                            }
                            if ( curr._fpProxRx != null )
                                curr._fpProxRx( eUPipeState.DataReady, rspbuff, ( UInt32 ) rspBuffUsed, 0 );

                            curr = null;
                            state = 0;
                        }
                        else
                        {
                            byte[] bk = new byte[ nRx ];
                            unsafe
                            {
                                fixed ( byte* p8 = bk )
                                {
                                    void* pvoid = ( void* ) p8;
                                    Marshal.Copy( _BufferRx, 0, new IntPtr( pvoid ), ( Int32 ) nRx );
                                }
                            }
                            listRx.Add( bk );
                        }
                        break;
                } // end-switch
            } // end-while
        } // end-Process
    }
}
