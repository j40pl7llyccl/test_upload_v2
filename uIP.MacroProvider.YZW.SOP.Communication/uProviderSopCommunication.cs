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
    public partial class uProviderSopCommunication : UMacroMethodProviderPlugin
    {
        internal enum CallScriptList : int
        {
            ByBuffer,
            ByVideo,
            ByFolder
        }

        // pipe server
        PipeCmdComm PipeSrv { get; set; } = null;
        // event share memory channels
        internal object SyncEvtShmemCH { get; set; } = new object();
        internal Dictionary<string, EvtShmemChannel> EvtShmemCHs { get; set; } = new Dictionary<string, EvtShmemChannel>();
        internal Dictionary<string, string[]> EvtShmemChExecScripts { get; set; } = new Dictionary<string, string[]>();
        internal EvtShmemChannel DefaultEvtShmemCH { get; set; } = null;

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

            // empty value check            
            foreach(var kv in nameDic )
            {
                if (string.IsNullOrEmpty(kv.Value))
                {
                    ULibAgent.Singleton.LogWarning?.Invoke( $"{GetType().Name}: event {kv.Key} is empty" );
                    return false;
                }
            }

            // Create Default evt share memory channel
            DefaultEvtShmemCH = new EvtShmemChannel();
            try
            {
                if (!DefaultEvtShmemCH.BuildChannel(
                    nameDic[ "share_mem_name" ],
                    nameDic[ "c2h_evt_name" ],
                    nameDic[ "h2c_evt_name" ],
                    int.Parse( nameDic[ "share_mem_size" ] ),
                    int.Parse( nameDic[ "common_size" ] ),
                    int.Parse( nameDic[ "c2h_size" ] ),
                    int.Parse( nameDic[ "h2c_size" ] ),
                    nameDic[ "common_sync_mux" ]
                    ) )
                {
                    DefaultEvtShmemCH.Dispose();
                    DefaultEvtShmemCH = null;
                }
            }
            catch(Exception ex )
            {
                DefaultEvtShmemCH.Dispose();
                DefaultEvtShmemCH = null;
            }

            if (DefaultEvtShmemCH != null)
            {
                // predefined operating call script name
                if ( nameDic.TryGetValue( "rx_buffer_call", out var buffC ) && !string.IsNullOrEmpty( buffC ) )
                    DefaultEvtShmemCH.RxBufferCall = string.Copy( buffC );
                if ( nameDic.TryGetValue( "rx_video_call", out var videoC ) && !string.IsNullOrEmpty( videoC ) )
                    DefaultEvtShmemCH.RxVideoCall = string.Copy( videoC );
                if ( nameDic.TryGetValue( "rx_folder_call", out var folderC ) && !string.IsNullOrEmpty( folderC ) )
                    DefaultEvtShmemCH.RxFolderCall = string.Copy( folderC );
            }

            // install system up call delegate
            ResourceManager.AddSystemUpCalls( SystemupCalled );

            // create pipe server
            if ( nameDic.TryGetValue( "cmd_pipe_name", out var pipeName ) && !string.IsNullOrEmpty( pipeName ) )
            {
                PipeSrv = new PipeCmdComm( pipeName, ULibAgent.Singleton.LogNormal ) { Owner = this };
            }

            // install macro for results
            InitMacro();

            // install cron job for db
            InitDbOP();

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
            // close pipe server
            PipeSrv?.Dispose();
            PipeSrv = null;

            // close evt share memory channels
            foreach(var kv in EvtShmemCHs)
            {
                kv.Value.Dispose();
            }

            DefaultEvtShmemCH?.Dispose();
            DefaultEvtShmemCH = null;

            base.Close();
        }
    }
}
