using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.IO;
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
    public class uProviderSopCommunication : UMacroMethodProviderPlugin
    {
        bool _bTerminated = false;
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
        int RemainSize { get;set; } = 0;

        string RxBufferCall { get; set; } = "";
        string RxVideoCall { get; set; } = "";
        string RxFolderCall { get; set; } = "";
        string RunningScript { get; set; } = "";

        PipeCmdComm PipeSrv { get; set; } = null;

        Thread tCommunication { get; set; }

        internal object SyncEvtShmemCH { get; set; } = new object();
        internal Dictionary<string, EvtShmemChannel> EvtShmemCHs { get; set; } = new Dictionary<string, EvtShmemChannel>();

        private static unsafe IntPtr CreateEvent(string name)
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
                sa.bInheritHandle = Convert.ToInt32(false);
                sa.lpSecurityDescriptor = memSD;
                Marshal.StructureToPtr(sa, memSA, false );

                return EventWinSdkFunctions.CreateEvent( ( SECURITY_ATTRIBUTES* )memSA.ToPointer(), Convert.ToInt32( false ), Convert.ToInt32( false ), name );
            }
            catch (Exception e)
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

        public uProviderSopCommunication() { }
        public override bool Initialize( UDataCarrier[] param )
        {
            // check working dir exists
            if ( !UDataCarrier.GetByIndex( param, 1, "", out var workingDir ) || !Directory.Exists( workingDir ) )
                return false;

            if ( m_bOpened )
                return true;

            var sysIni = Path.Combine( workingDir, ULibAgent.IniFolderName, ULibAgent.SystemIniFilename );
            if ( !File.Exists( sysIni ) )
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: system ini file {sysIni} not exist!" );
                return false;
            }

            var iniInst = new IniReaderUtility();
            if (!iniInst.Parsing(sysIni))
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: system ini file {sysIni} parse fail!" );
                return false;
            }

            // any new item needs finding add here
            if (!iniInst.Get( "sop_communication", out var kvs, 
                "c2h_evt_name",
                "h2c_evt_name",
                "share_mem_name",
                "share_mem_size",
                "common_sync_mux",
                "common_size",
                "c2h_size",
                "h2c_size",
                "rx_buffer_call",
                "rx_video_call",
                "rx_folder_call",
                "cmd_pipe_name" ) )
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: communication info error" );
                return false;
            }

            var nameDic = kvs.ToDictionary( kv => kv.Key, kv => kv.Value == null || kv.Value.Length <= 0 ? "" : kv.Value[ 0 ] );
            
            foreach(var kv in nameDic )
            {
                if (string.IsNullOrEmpty(kv.Value))
                {
                    ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: event {kv.Key} is empty" );
                    return false;
                }
            }
            if ( !int.TryParse( nameDic[ "share_mem_size" ], out var shmemsz ))
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: share mem size error" );
                return false;
            }

            hC2HEvt = CreateEvent( "Global\\" + nameDic[ "c2h_evt_name" ] );
            if (hC2HEvt == IntPtr.Zero)
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: create c->h event fail" );
                return false;
            }

            hH2CEvt = CreateEvent( "Global\\" + nameDic[ "h2c_evt_name" ] );
            if (hH2CEvt == IntPtr.Zero)
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: create h->c event fail" );
                return false;
            }

            hShMem = MemWinSdkFunctions.CreateFileMapping( ( uint )shmemsz, nameDic[ "share_mem_name" ] );
            if (hShMem == IntPtr.Zero)
            {
                var errorCode = CommonWinSdkFunctions.GetLastError();
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: create share mem fail last error code={errorCode}" );
                return false;
            }

            pAddr = MemWinSdkFunctions.MapViewOfFile( hShMem, ( uint )shmemsz );
            if ( pAddr == IntPtr.Zero)
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: map share mem fail" );
                return false;
            }

            // predefined operating call script name
            if ( nameDic.TryGetValue( "rx_buffer_call", out var buffC ) && !string.IsNullOrEmpty( buffC ) )
                RxBufferCall = string.Copy( buffC );
            if (nameDic.TryGetValue( "rx_video_call", out var videoC ) && !string.IsNullOrEmpty( videoC ) )
                RxVideoCall = string.Copy( videoC );
            if (nameDic.TryGetValue( "rx_folder_call", out var folderC) && !string.IsNullOrEmpty(folderC))
                RxFolderCall = string.Copy( folderC );

            int szCommon = 0;
            int szC2H = 0;
            int szH2C = 0;
            string synNm = "";
            if ( nameDic.TryGetValue( "common_size", out var commonSzS ) && int.TryParse( commonSzS, out szCommon ) ) { }
            if ( nameDic.TryGetValue( "c2h_size", out var c2h ) && int.TryParse( c2h, out szC2H ) ) { }
            if ( nameDic.TryGetValue( "h2c_size", out var h2c ) && int.TryParse( h2c, out szH2C ) ) { }

            if ( szCommon > 0 && ( !nameDic.TryGetValue( "common_sync_mux", out synNm ) || string.IsNullOrEmpty( synNm ) ) )
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: common size > 0 without sync mux name" );
                return false;
            }

            if ( !string.IsNullOrEmpty( synNm ) )
                SyncCommon = new Mutex( false, "Global\\" + synNm );

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

            if (szC2H <= 0 || szH2C <= 0)
            {
                ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: size({shmemsz}) not enough; common={szCommon}, C2H={szC2H}, H2C={szH2C}" );
                return false;
            }

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
                if (szCommon > 0)
                    pCommAddr = new IntPtr( p8 );
                pC2HAddr = new IntPtr( p8 + szCommon );
                pH2CAddr = new IntPtr( p8 + szCommon + C2HSize );
                pRemainAddr = new IntPtr( p8 + szCommon + C2HSize + H2CSize );
            }

            tCommunication = new Thread( Communicate );
            tCommunication.Start();

            ResourceManager.AddSystemUpCalls( SystemupCalled );

            if ( nameDic.TryGetValue( "cmd_pipe_name", out var pipeName ) && !string.IsNullOrEmpty( pipeName ) )
            {
                PipeSrv = new PipeCmdComm( pipeName, ULibAgent.Singleton.LogNormal ) { Owner = this };
            }

            m_bOpened = true;
            return true;
        }

        private static void SystemupCalled()
        {
            if ( !ResourceManager.Get( ResourceManager.ProgGuid, "", out var guid ) || string.IsNullOrEmpty( guid ) )
                return;

            try
            {
                var hEvt = EventWinSdkFunctions.OpenEvent( ( uint )EVT_ACC_RIGHT.ALL_ACCESS, Convert.ToInt32( false ), $"Global\\sysup_{guid}" );
                if ( hEvt != IntPtr.Zero )
                {
                    EventWinSdkFunctions.SetEvent( hEvt );
                    EventWinSdkFunctions.Close(hEvt);
                }
            }
            catch { }
        }

        public override void Close()
        {
            PipeSrv?.Dispose();
            PipeSrv = null;

            foreach(var kv in EvtShmemCHs)
            {
                kv.Value.Dispose();
            }

            _bTerminated = true;
            if ( tCommunication != null )
            {
                EventWinSdkFunctions.Set( hC2HEvt );
                tCommunication.Join();
            }

            base.Close();
            SyncCommon?.Dispose();
            if ( hC2HEvt != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( hC2HEvt );
            if ( hH2CEvt != IntPtr.Zero ) CommonWinSdkFunctions.CloseHandle( hH2CEvt );
            MemWinSdkFunctions.CloseFileMapping( hShMem, pAddr );
            hC2HEvt = IntPtr.Zero;
            hH2CEvt = IntPtr.Zero;
            hShMem = IntPtr.Zero;
            pAddr = IntPtr.Zero;
        }

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
        }

        /// <summary>
        /// Thread exec
        /// </summary>
        unsafe void Communicate()
        {
            while ( !_bTerminated )
            {
                if (WaitWinSdkFunctions.WaitForSingleObject(hC2HEvt, (uint)WAIT.INFINITE) == (uint)WAIT_STATUS.OBJECT_0)
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


                    if (*pI32CommID == (int)CommID.CommConnected)
                    {
                        if ( pCommAddr != IntPtr.Zero && CommonSize > sizeof(int))
                        {
                            SyncCommon.WaitOne();
                            int* pConnCnt = ( int* )pCommAddr.ToPointer();
                            (*pConnCnt)++;
                            code = 1;
                            msg = $"conn count={*pConnCnt}";
                            SyncCommon.ReleaseMutex();
                        }
                    }
                    else if ( *pI32CommID == (int)CommID.SOP)
                    {
                        int* pOpCode = ++pI32CommID;
                        ProcessSopReq( *pOpCode, (byte*)(++pOpCode), dataL - sizeof(int) * 2, ref rspT, ref code, ref msg );
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
            *pInt = sizeof( int ) + sizeof(int);
            *( ++pInt ) = ( int )RspTypeID.IntCode;
            *( ++pInt ) = code;
            return true;
        }

        unsafe bool ResponseIntCodeWithString( int code, string message )
        {
            byte[] wr = string.IsNullOrEmpty( message ) ? new byte[ 0 ] : Encoding.UTF8.GetBytes( message );
            // totalSize:int, rsp type code:int, code:int, byte with zero end
            int sz = sizeof( int ) + sizeof(int) + wr.Length;
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

        void SopCallScript(string scriptName, UDataCarrier[] input, ref int rspCode, ref string rspMsg )
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
            if ( !script.UnderRunning )
            {
                rspCode = ( int )SopRspCode.ScriptUnderRunning;
                rspMsg = $"{scriptName} is running";
                return;
            }
            // run script
            RunningScript = scriptName;
            var rc = script.Running( true, prevPropagation: input );
            if ( rc != ScriptExecReturnCode.OK )
            {
                rspCode = ( int )SopRspCode.ScriptExecWithError;
                rspMsg = $"{scriptName} exec script with error={rc}";
                RunningScript = "";
                return;
            }
            // get last macro as result
            var finalRC = script.ResultCarriers.Last();
            if ( !UDataCarrier.GetByIndex( finalRC.ResultSet, 0, false, out var callStatus ) )
            {
                rspCode = ( int )SopRspCode.ScriptResultInvalid;
                rspMsg = $"{scriptName} result index 0 not contain status";
                RunningScript = "";
                return;
            }

            // call script done with OK
            if (callStatus)
            {
                rspCode = ( int )SopRspCode.OK;
                rspMsg = "";
                RunningScript = "";
                return;
            }

            // call script done with error
            if (!UDataCarrier.GetByIndex(finalRC.ResultSet, 1, "", out var reason))
            {
                rspCode = ( int )SopRspCode.ScriptResultInvalid;
                rspMsg = $"{scriptName} result index 1 not contain reason";
                RunningScript = "";
                return;
            }

            RunningScript = "";
            rspCode = ( int )SopRspCode.NG;
            rspMsg = reason;
        }

        unsafe void ProcessSopReq( int opcode, byte* pData, int dataLen, ref RspTypeID rspID, ref int rspCode, ref string rspMsg)
        {
            if (opcode == (int)CommSop.PinPon)
            {
                rspID = RspTypeID.IntCode;
                rspCode = *( ( int* )pData );
                return;
            }

            if ( opcode == ( int )CommSop.ByBuffOp )
            {

                int* pImageInfo = ( int* )pData;
                int imgW = pImageInfo[ 0 ];
                int imgH = pImageInfo[ 2 ];
                int imgPixBitSize = pImageInfo[ 3 ];
                int imgStride = pImageInfo[ 4 ];

                SopCallScript( RxBufferCall, UDataCarrier.MakeVariableItemsArray( imgW, imgH, imgPixBitSize, imgStride, pRemainAddr ), ref rspCode, ref rspMsg );
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
                if (strlen == 0)
                {
                    rspCode = ( int )SopRspCode.InputParameterError;
                    rspMsg = $"{CommSop.ByVideoOp} had no file path";
                    return;
                }

                SopCallScript(RxVideoCall, UDataCarrier.MakeOneItemArray(Encoding.UTF8.GetString(pData, strlen)), ref rspCode, ref rspMsg );
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

                SopCallScript( RxVideoCall, UDataCarrier.MakeOneItemArray( Encoding.UTF8.GetString( pData, strlen ) ), ref rspCode, ref rspMsg );
            }
        }

    }
}
