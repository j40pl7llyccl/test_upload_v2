using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uIP.Lib;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    public class EvtShmemChannel : IDisposable
    {
        internal const string InputCarrierBuffPtrDesc = "input buffer pointer";
        internal const string InputCarrierBuffWidthDesc = "input buffer width";
        internal const string InputCarrierBuffHeightDesc = "input buffer height";
        internal const string InputCarrierBuffPixelBitsDesc = "input buffer pixel bits";
        internal const string InputCarrirBuffStrideDesc = "input buffer stride";
        internal const string InputCarrierTimestamp = "input timestamp";
        //internal const string InputCarrierVideoDesc = "input video path";
        //internal const string InputCarrierFolderDesc = "input folder path";

        bool _bDisposed = false;
        bool _bTerminated = false;
        public bool Ready { get; private set; } = false;

        public string ExecDoneQueryResultDescStatus { get; set; } = ResultDescription.inspect_status.ToString(); // bool
        public string ExecDoneQueryResultDescData { get; set; } = ResultDescription.inspect_messages.ToString(); // string

        internal CancelExecScript Cancellation { get; set; }
        public string RunningScript { get; private set; }
        public string RxBufferCall { get; set; } = "";
        public string RxVideoCall { get; set; } = "";
        public string RxFolderCall { get; set; } = "";

        public string ShmemName { get; private set; }
        public string C2HEvtName { get; private set; }
        public string H2CEvtName { get; private set; }
        public string CommMuxName { get; private set; }

        IntPtr hC2HEvt { get; set; } = IntPtr.Zero;
        IntPtr hH2CEvt { get; set; } = IntPtr.Zero;
        IntPtr hShMem { get; set; } = IntPtr.Zero;
        IntPtr pAddr { get; set; } = IntPtr.Zero;

        Mutex SyncCommon { get; set; } = null;

        IntPtr pCommAddr { get; set; } = IntPtr.Zero;
        int CommonSize { get; set; } = 0;

        IntPtr pC2HAddr { get; set; } = IntPtr.Zero;
        int C2HSize { get; set; } = 0;

        IntPtr pH2CAddr { get; set; } = IntPtr.Zero;
        int H2CSize { get; set; } = 0;

        IntPtr pRemainAddr { get; set; } = IntPtr.Zero;
        int RemainSize { get; set; } = 0;

        Thread tCommunication { get; set; }

        public EvtShmemChannel() { }
        ~EvtShmemChannel()
        {
            Dispose( false );
        }

        void Dispose(bool disposing)
        {
            if ( _bDisposed )
                return;
            _bDisposed = true;

            _bTerminated = true;
            if ( tCommunication != null )
            {
                EventWinSdkFunctions.Set( hC2HEvt );
                tCommunication.Join();
            }

            SyncCommon?.Dispose();
            if ( hC2HEvt != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( hC2HEvt );
            if ( hH2CEvt != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( hH2CEvt );
            MemWinSdkFunctions.CloseFileMapping( hShMem, pAddr );
            hC2HEvt = IntPtr.Zero;
            hH2CEvt = IntPtr.Zero;
            hShMem = IntPtr.Zero;
            pAddr = IntPtr.Zero;
        }
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private static unsafe IntPtr CreateEvent( string name )
        {
            IntPtr memSD = IntPtr.Zero;
            IntPtr memSA = IntPtr.Zero;

            try
            {
                int szSD = Marshal.SizeOf( typeof( SECURITY_DESCRIPTOR ) );
                memSD = Marshal.AllocCoTaskMem( szSD );
                int szSA = Marshal.SizeOf( typeof( SECURITY_ATTRIBUTES ) );
                memSA = Marshal.AllocCoTaskMem( szSA );
                SECURITY_DESCRIPTOR sd;
                var status = EventWinSdkFunctions.InitializeSecurityDescriptor( out sd, EventWinSdkFunctions.SECURITY_DESCRIPTOR_REVISION );
                if ( !status )
                {
                    ULibAgent.Singleton.LogError?.Invoke( $"[uProviderSopCommunication::CreateEvent]InitializeSecurityDescriptor: last error code={CommonWinSdkFunctions.GetLastError()}" );
                    return IntPtr.Zero;
                }
                status = EventWinSdkFunctions.SetSecurityDescriptorDacl( ref sd, true, IntPtr.Zero, false );
                if ( !status )
                {
                    ULibAgent.Singleton.LogError?.Invoke( $"[uProviderSopCommunication::CreateEvent]SetSecurityDescriptorDacl: last error code={CommonWinSdkFunctions.GetLastError()}" );
                    return IntPtr.Zero;
                }
                Marshal.StructureToPtr( sd, memSD, false );

                SECURITY_ATTRIBUTES sa;
                sa.nLength = ( uint )szSD;
                sa.bInheritHandle = Convert.ToInt32( false );
                sa.lpSecurityDescriptor = memSD;
                Marshal.StructureToPtr( sa, memSA, false );

                return EventWinSdkFunctions.CreateEvent( ( SECURITY_ATTRIBUTES* )memSA.ToPointer(), Convert.ToInt32( false ), Convert.ToInt32( false ), name );
            }
            catch ( Exception e )
            {
                ULibAgent.Singleton.LogError?.Invoke( $"uProviderSopCommunication::CreateEvent ({name}) with error\n{e}" );
                return IntPtr.Zero;
            }
            finally
            {
                if ( memSD != IntPtr.Zero ) Marshal.FreeCoTaskMem( memSD );
                if ( memSA != IntPtr.Zero ) Marshal.FreeCoTaskMem( memSA );
            }
        }

        public bool BuildChannel(string strShmem, string strC2H, string strH2C, int shmemsz, int szCommon, int szC2H, int szH2C, string strCommMux )
        {
            if ( Ready )
                return true;

            if ( string.IsNullOrEmpty( strShmem ) || string.IsNullOrEmpty( strC2H ) || string.IsNullOrEmpty( strH2C ) )
                return false;

            hC2HEvt = CreateEvent( "Global\\" + strC2H );
            if ( hC2HEvt == IntPtr.Zero )
                return false;

            hH2CEvt = CreateEvent( "Global\\" + strH2C );
            if ( hH2CEvt == IntPtr.Zero )
                return false;

            hShMem = MemWinSdkFunctions.CreateFileMapping( ( uint )shmemsz, strShmem );
            if ( hShMem == IntPtr.Zero )
                return false;

            pAddr = MemWinSdkFunctions.MapViewOfFile( hShMem, ( uint )shmemsz );
            if ( pAddr == IntPtr.Zero )
                return false;

            if ( szCommon > 0 && string.IsNullOrEmpty( strCommMux ) )
                return false;
            if ( !string.IsNullOrEmpty( strCommMux ) )
                SyncCommon = new Mutex( false, "Global\\" + strCommMux );

            if ( szC2H > 0 && szH2C > 0 )
            {
                if ( shmemsz < ( szC2H + szH2C + szCommon ) )
                {
                    ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: open share mem size({shmemsz}) less than C2H({szC2H}) + H2C({szH2C}) + Common({szCommon})" );
                    return false;
                }
            }
            else if ( szC2H <= 0 && szH2C <= 0 )
            {
                if ( shmemsz < ( 1024 * 3 + szCommon ) )
                {
                    var remainSz = shmemsz - szCommon;
                    remainSz = remainSz < 0 ? 0 : remainSz;
                    szC2H = remainSz / 3;
                    szH2C = remainSz / 3;
                }
                else
                {
                    szC2H = 1024;
                    szH2C = 1024;
                }
            }
            else if ( szC2H > 0 )
            {
                if ( shmemsz < ( szC2H * 3 + szCommon ) )
                {
                    var remainSz = shmemsz - szCommon;
                    remainSz = remainSz < 0 ? 0 : remainSz;
                    szC2H = remainSz / 3;
                    szH2C = remainSz / 3;
                }
                else
                    szH2C = szC2H;
            }
            else
            {
                if ( shmemsz < ( szH2C * 3 + szCommon ) )
                {
                    var remainSz = shmemsz - szCommon;
                    remainSz = remainSz < 0 ? 0 : remainSz;
                    szC2H = remainSz / 3;
                    szH2C = remainSz / 3;
                }
                else
                    szC2H = szH2C;
            }

            if ( szC2H <= 0 || szH2C <= 0 )
                return false;

            CommonSize = szCommon;
            C2HSize = szC2H;
            H2CSize = szH2C;
            RemainSize = shmemsz - szCommon - szC2H - szH2C;

            unsafe
            {
                // clear all memory
                MemWinSdkFunctions.NativeMemset( pAddr, 0, new UIntPtr( ( uint )shmemsz ) );
                // point to beg addr
                byte* p8 = ( byte* )pAddr.ToPointer();
                // formating
                if ( szCommon > 0 )
                    pCommAddr = new IntPtr( p8 );
                pC2HAddr = new IntPtr( p8 + szCommon );
                pH2CAddr = new IntPtr( p8 + szCommon + C2HSize );
                pRemainAddr = new IntPtr( p8 + szCommon + C2HSize + H2CSize );
            }

            tCommunication = new Thread( Communicate );
            tCommunication.Start();

            ShmemName = strShmem;
            C2HEvtName = strC2H;
            H2CEvtName = strH2C;
            CommMuxName = strCommMux;

            Ready = true;
            return true;
        }

        public bool NewChannel(int nHuge, int nCommon, int nC2H, int nH2C)
        {
            if ( Ready )
                return true;
            if ( nHuge <= 0 || nCommon <= sizeof( int ) || nC2H <= 0 || nH2C <= 0 )
                return false;

            var ts = CommonUtilities.GetCurrentTimeStr( "" );

            C2HEvtName = $"{GetType().Name}_C2HEvt_{ts}";
            hC2HEvt = CreateEvent( "Global\\" + C2HEvtName );
            if ( hC2HEvt == IntPtr.Zero )
                return false;

            H2CEvtName = $"{GetType().Name}_H2CEvt_{ts}";
            hH2CEvt = CreateEvent( "Global\\" + H2CEvtName );
            if ( hH2CEvt == IntPtr.Zero )
                return false;

            var shmemsz = nHuge + nCommon + nC2H + nH2C;
            ShmemName = $"{GetType().Name}_Shmem_{ts}";
            hShMem = MemWinSdkFunctions.CreateFileMapping( ( uint )shmemsz, ShmemName );
            if ( hShMem == IntPtr.Zero )
                return false;

            pAddr = MemWinSdkFunctions.MapViewOfFile( hShMem, ( uint )shmemsz );
            if ( pAddr == IntPtr.Zero )
                return false;

            CommMuxName = $"{GetType().Name}_CommonMux_{ts}";
            SyncCommon = new Mutex( false, "Global\\" + CommMuxName );

            CommonSize = nCommon;
            C2HSize = nC2H;
            H2CSize = nH2C;
            RemainSize = nHuge;

            unsafe
            {
                // clear all memory
                MemWinSdkFunctions.NativeMemset( pAddr, 0, new UIntPtr( ( uint )shmemsz ) );
                // point to beg addr
                byte* p8 = ( byte* )pAddr.ToPointer();
                // formating
                pCommAddr = new IntPtr( p8 );
                pC2HAddr = new IntPtr( p8 + CommonSize );
                pH2CAddr = new IntPtr( p8 + CommonSize + C2HSize );
                pRemainAddr = new IntPtr( p8 + CommonSize + C2HSize + H2CSize );
            }

            // create communication thread
            tCommunication = new Thread( Communicate );
            tCommunication.Start();

            Ready = true;

            return true;
        }

        public void CancelRun()
        {
            if ( string.IsNullOrEmpty( RunningScript ) )
                return;
            if (Cancellation != null)
            {
                Cancellation.Flag = true;
                return;
            }

            var s = ULibAgent.Singleton.Scripts.GetScript( RunningScript );
            if ( s != null && s.UnderRunning )
                s.CancelRunning();

        }

        #region Communication Enum
        enum CommID : int
        {
            CommConnected = 0,
            SOP = 1,
        }

        enum CommSop : int
        {
            PinPon = 0,
            ByBuffOp,
            ByVideoOp,
            ByFolderOp,
        }

        enum RspTypeID : int
        {
            IntCode = 0,
            IntCodeWithString
        }

        enum SopRspCode : int
        {
            OK = 1,
            NG = 0,
            ScriptNotFound = -1,
            ScriptUnderRunning = -2,
            ScriptExecWithError = -3,
            ScriptResultInvalid = -4,
            InputParameterError = -5,
            ScriptWithUiCannotExecRemotely = -6,
        }
        #endregion

        /// <summary>
        /// Thread exec
        /// </summary>
        unsafe void Communicate()
        {
            while ( !_bTerminated )
            {
                if ( WaitWinSdkFunctions.WaitForSingleObject( hC2HEvt, ( uint )WAIT.INFINITE ) == ( uint )WAIT_STATUS.OBJECT_0 )
                {
                    if ( _bTerminated )
                        break;

                    // len:int, commId:int

                    RspTypeID rspT = RspTypeID.IntCodeWithString;
                    int* pI32 = ( int* )pC2HAddr.ToPointer();
                    int dataL = *pI32;
                    int* pI32CommID = ++pI32;
                    int code = -1;
                    string msg = "undefined";


                    if ( *pI32CommID == ( int )CommID.CommConnected )
                    {
                        if ( pCommAddr != IntPtr.Zero && CommonSize > sizeof( int ) )
                        {
                            SyncCommon.WaitOne();
                            int* pConnCnt = ( int* )pCommAddr.ToPointer();
                            ( *pConnCnt )++;
                            code = 1;
                            msg = $"conn count={*pConnCnt}";
                            SyncCommon.ReleaseMutex();
                        }
                    }
                    else if ( *pI32CommID == ( int )CommID.SOP )
                    {
                        int* pOpCode = ++pI32CommID;
                        ProcessSopReq( *pOpCode, ( byte* )( ++pOpCode ), dataL - sizeof( int ) * 2, ref rspT, ref code, ref msg );
                    }

                    if ( rspT == RspTypeID.IntCode )
                        ResponseIntCode( code );
                    else if ( rspT == RspTypeID.IntCodeWithString )
                        ResponseIntCodeWithString( code, msg );

                    EventWinSdkFunctions.Set( hH2CEvt );
                }
            }
        }

        unsafe bool ResponseIntCode( int code )
        {
            if ( H2CSize < sizeof( int ) * 2 )
                return false;

            // total size: int, rsp type code: int, code: int
            int* pInt = ( int* )pH2CAddr.ToPointer();
            *pInt = sizeof( int ) + sizeof( int );
            *( ++pInt ) = ( int )RspTypeID.IntCode;
            *( ++pInt ) = code;
            return true;
        }

        unsafe bool ResponseIntCodeWithString( int code, string message )
        {
            byte[] wr = string.IsNullOrEmpty( message ) ? new byte[ 0 ] : Encoding.UTF8.GetBytes( message );
            // totalSize:int, rsp type code:int, code:int, byte with zero end
            int sz = sizeof( int ) + sizeof( int ) + wr.Length;
            int total = sizeof( int ) + sz;
            if ( total > H2CSize )
                return false;

            byte* p8 = ( byte* )pH2CAddr.ToPointer();

            // fill size
            int* pDataSz = ( int* )p8;
            *pDataSz = sz;
            p8 += sizeof( int );

            // fill rsp type code
            int* pRspT = ( int* )p8;
            *pRspT = ( int )RspTypeID.IntCodeWithString;
            p8 += sizeof( int );

            // fill code
            int* pCode = ( int* )p8;
            *pCode = code;
            p8 += sizeof( int );

            // fill message
            if ( wr.Length > 0 )
            {
                // copy
                Marshal.Copy( wr, 0, new IntPtr( p8 ), wr.Length );
            }

            return true;
        }

        private static UDataCarrier QueryResultDesc(object ctx, UDataCarrier input)
        {
            if ( input == null )
                return null;

            if (ctx is string[] want && want != null && want.Length > 0 )
            {
                if ( want.Contains( input.Desc ) )
                    return input;
            }

            return null;
        }

        private static UDataCarrier QueryResultDescInScript(string scriptName, object ctx, UDataCarrier input)
        {
            if ( input == null )
                return null;

            if ( ctx is string[] want && want != null && want.Length > 0 )
            {
                if ( want.Contains( input.Desc ) )
                    return input;
            }

            return null;
        }

        private static void HandleResultSet(List<UScriptHistoryCarrier> set)
        {
            if ( set == null ) return;
            foreach(var s in set)
            {
                s?.Dispose();
            }
            set.Clear();
        }

        void SopCallScript( string scriptName, UDataCarrier[] input, ref int rspCode, ref string rspMsg )
        {
            // get script instance
            var script = ULibAgent.Singleton.Scripts.GetScript( scriptName );
            if ( script == null )
            {
                rspCode = ( int )SopRspCode.ScriptNotFound;
                rspMsg = $"Cannot find {scriptName} script to exec";
                return;
            }
            // check under running
            if ( script.UnderRunning )
            {
                rspCode = ( int )SopRspCode.ScriptUnderRunning;
                rspMsg = $"{scriptName} is running";
                return;
            }
            // check script with ui interact
            if (script.InteractWithUI)
            {
                rspCode = ( int )SopRspCode.ScriptWithUiCannotExecRemotely;
                rspMsg = $"{scriptName} has UI interacting and cannot exec rermotely";
                return;
            }
            // run script
            Cancellation = null;
            RunningScript = scriptName;
            List<UScriptHistoryCarrier> results = null;
            var result = new Dictionary<string, UDataCarrier>();
            UDataCarrier[] foundR = null;
            if ( script.AbilitySwitchScript )
            {
                // with going to another script
                Cancellation = new CancelExecScript( null, null );
                if (!UScript.RunningControlFlow(false, Cancellation, ULibAgent.Singleton.Scripts.Scripts, script, true, 100, false, out results, input ))
                {
                    rspCode = ( int )SopRspCode.ScriptExecWithError;
                    rspMsg = $"{scriptName} exec script fail";
                    RunningScript = "";
                    Cancellation = null;
                    return;
                }

                // query from result
                // desc contain ExecDoneQueryResultDescStatus
                // desc contain ExecDoneQueryResultDescData
                if ( !UDataCarrier.GetHistoryCmpMany(results, new string[] { ExecDoneQueryResultDescStatus, ExecDoneQueryResultDescData }, QueryResultDescInScript, out foundR ) ||
                    foundR == null || foundR.Length == 0 )
                {
                    rspCode = ( int )SopRspCode.ScriptExecWithError;
                    rspMsg = $"{scriptName} exec script done but not find results";
                    HandleResultSet( results );
                    RunningScript = "";
                    Cancellation = null;
                    return;
                }
            }
            else
            {
                // direct exec script
                var rc = script.Running( true, prevPropagation: input );
                if ( rc != ScriptExecReturnCode.OK )
                {
                    rspCode = ( int )SopRspCode.ScriptExecWithError;
                    rspMsg = $"{scriptName} exec script with error={rc}";
                    RunningScript = "";
                    Cancellation = null;
                    return;
                }

                results = new List<UScriptHistoryCarrier>() { new UScriptHistoryCarrier( script, true, false, false ) };

                // query from result
                // desc contain ExecDoneQueryResultDescStatus
                // desc contain ExecDoneQueryResultDescData
                if ( !UDataCarrier.GetHistoryCmpMany(
                        results,
                        new string[] { ExecDoneQueryResultDescStatus, ExecDoneQueryResultDescData },
                        QueryResultDescInScript,
                        out foundR ) ||
                    foundR == null || foundR.Length == 0)
                {
                    rspCode = ( int )SopRspCode.ScriptExecWithError;
                    rspMsg = $"{scriptName} exec script done but not find results";
                    HandleResultSet( results );
                    RunningScript = "";
                    Cancellation = null;
                    return;
                }
            }

            // split result
            foreach(var i in foundR )
            {
                if (i.Desc == ExecDoneQueryResultDescStatus )
                {
                    if ( result.ContainsKey( i.Desc ) )
                        result[i.Desc] = i;
                    else
                        result.Add( ExecDoneQueryResultDescStatus, i );
                }
                else if (i.Desc == ExecDoneQueryResultDescData )
                {
                    if ( result.ContainsKey( i.Desc ) )
                        result[ i.Desc ] = i;
                    else
                        result.Add( ExecDoneQueryResultDescData, i );
                }
            }

            // get status
            if (!result.TryGetValue( ExecDoneQueryResultDescStatus, out var statC ) || !UDataCarrier.Get( statC, false, out var execStatus ))
            {
                rspCode = ( int )SopRspCode.ScriptExecWithError;
                rspMsg = $"{scriptName} exec script done but not found status";
                HandleResultSet( results );
                RunningScript = "";
                Cancellation = null;
                return;
            }

            // final report
            rspCode = execStatus ? ( int )SopRspCode.OK : ( int )SopRspCode.NG;
            rspMsg = result.TryGetValue( ExecDoneQueryResultDescData, out var msgC ) ? string.Join("\n", UDataCarrier.Get(msgC, new string[0] )) : "";
            HandleResultSet( results );
            RunningScript = "";
            Cancellation = null;
        }

        unsafe void ProcessSopReq( int opcode, byte* pData, int dataLen, ref RspTypeID rspID, ref int rspCode, ref string rspMsg )
        {
            if ( opcode == ( int )CommSop.PinPon )
            {
                rspID = RspTypeID.IntCode;
                rspCode = *( ( int* )pData );
                return;
            }

            if ( opcode == ( int )CommSop.ByBuffOp )
            {

                int* pImageInfo = ( int* )pData;
                int imgW = pImageInfo[ 0 ];
                int imgH = pImageInfo[ 1 ];
                int imgPixBitSize = pImageInfo[ 2 ];
                int imgStride = pImageInfo[ 3 ];

                var inputs = new UDataCarrier[]
                {
                    UDataCarrier.MakeOne(pRemainAddr, InputCarrierBuffPtrDesc),
                    UDataCarrier.MakeOne(imgW, InputCarrierBuffWidthDesc),
                    UDataCarrier.MakeOne(imgH, InputCarrierBuffHeightDesc),
                    UDataCarrier.MakeOne(imgPixBitSize, InputCarrierBuffPixelBitsDesc),
                    UDataCarrier.MakeOne(imgStride, InputCarrirBuffStrideDesc),
                    UDataCarrier.MakeOne(DateTime.Now, InputCarrierTimestamp),
                    UDataCarrier.MakeOne(InputSourceDescription.input_source_from_buffer.ToString(), InputSourceDescription.input_source_from.ToString())
                };

                SopCallScript( RxBufferCall, inputs, ref rspCode, ref rspMsg );
            }
            else if ( opcode == ( int )CommSop.ByVideoOp )
            {
                int strlen = 0;
                for ( int i = 0; i < dataLen; i++, strlen++ )
                {
                    if ( pData[ i ] == 0 || pData[ i ] == '\0' )
                    {
                        //strlen--;
                        break;
                    }
                }
                if ( strlen == 0 )
                {
                    rspCode = ( int )SopRspCode.InputParameterError;
                    rspMsg = $"{CommSop.ByVideoOp} had no file path";
                    return;
                }

                var inputs = new UDataCarrier[]
                {
                    UDataCarrier.MakeOne(Encoding.UTF8.GetString( pData, strlen ), InputSourceDescription.input_source_from_video.ToString()),
                    UDataCarrier.MakeOne(DateTime.Now, InputCarrierTimestamp),
                    UDataCarrier.MakeOne(InputSourceDescription.input_source_from_video.ToString(), InputSourceDescription.input_source_from.ToString())
                };

                SopCallScript( RxVideoCall, inputs, ref rspCode, ref rspMsg );
            }
            else if ( opcode == ( int )CommSop.ByFolderOp )
            {
                int strlen = 0;
                for ( int i = 0; i < dataLen; i++, strlen++ )
                {
                    if ( pData[ i ] == 0 || pData[ i ] == '\0' )
                    {
                        //strlen--;
                        break;
                    }
                }
                if ( strlen == 0 )
                {
                    rspCode = ( int )SopRspCode.InputParameterError;
                    rspMsg = $"{CommSop.ByFolderOp} had no folder path";
                    return;
                }

                var inputs = new UDataCarrier[]
                {
                    UDataCarrier.MakeOne(Encoding.UTF8.GetString( pData, strlen ), InputSourceDescription.input_source_from_folder.ToString()),
                    UDataCarrier.MakeOne(DateTime.Now, InputCarrierTimestamp),
                    UDataCarrier.MakeOne(InputSourceDescription.input_source_from_folder.ToString(), InputSourceDescription.input_source_from.ToString())
                };

                SopCallScript( RxFolderCall, inputs, ref rspCode, ref rspMsg );
            }
        }

    }
}
