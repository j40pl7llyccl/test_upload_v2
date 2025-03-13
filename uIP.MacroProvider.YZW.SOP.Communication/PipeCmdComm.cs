using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using uIP.Lib;
using uIP.Lib.InterPC;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    public class PipeCmdComm : UPipeServer
    {
        internal uProviderSopCommunication Owner { get; set; }
        Action<string> Log { get; set; } = null;
        public PipeCmdComm(string name, Action<string> log) : base(name, 0, null, null)
        {
            Log = log;
            _fpDbg = LogMessage;
        }

        private void LogMessage(string m, int lvl)
        {
            Log?.Invoke( $"{m}" );
        }

        enum Cmd : int
        {
            ResetExec,
            CreateEvtShmemCH,
            RemoveEvtShmemCH,
            ConfigCallingScript,
        }

        protected override void ProcessRequest( UPipeCb cb, byte[] rxDat, int nRx, out byte[] rspDat, out int nOffsetIdx, out int nDatLen )
        {
            var strOK = "OK";
            var strNG = "NG";
            var rx = Encoding.UTF8.GetString( rxDat, 0, nRx );
            if (string.IsNullOrEmpty(rx))
            {
                var rsp = Encoding.UTF8.GetBytes( "NG, data invalid" );
                rspDat = rsp;
                nOffsetIdx = 0;
                nDatLen = rsp.Length;
                return;
            }

            var ss = rx.Split( ',' );
            var cmd = ss[ 0 ].Trim();
            if (cmd == Cmd.ResetExec.ToString())
            {
                try
                {
                    // by share mem name
                    var shmemName = ss[ 1 ].Trim();
                    EvtShmemChannel ch = null;
                    if ( shmemName == Owner.DefaultEvtShmemCH.ShmemName )
                        ch = Owner.DefaultEvtShmemCH;
                    else
                    {
                        Owner.EvtShmemCHs.TryGetValue( shmemName, out ch );
                    }

                    ch?.CancelRun();
                }
                catch { goto ProcessRequest_NG; }
            }
            else if (cmd == Cmd.CreateEvtShmemCH.ToString() )
            {
                try
                {
                    int huge = int.Parse( ss[ 1 ] );
                    int comm = int.Parse( ss[ 2 ] );
                    int c2h = int.Parse( ss[ 3 ] );
                    int h2c = int.Parse( ss[ 4 ] );

                    var ch = new EvtShmemChannel();
                    if (ch.NewChannel(huge, comm, c2h, h2c ))
                    {
                        Monitor.Enter( Owner.SyncEvtShmemCH );

                        Owner.EvtShmemCHs.Add( ch.ShmemName, ch );

                        Monitor.Exit( Owner.SyncEvtShmemCH );

                        var rspS = $"{ch.ShmemName},{ch.C2HEvtName},{ch.H2CEvtName},{ch.CommMuxName}";
                        rspDat = Encoding.UTF8.GetBytes( rspS );
                        nOffsetIdx = 0;
                        nDatLen = rspDat.Length;
                        return;
                    }
                }
                catch { }

                goto ProcessRequest_NG;
            }
            else if (cmd == Cmd.RemoveEvtShmemCH.ToString() )
            {
                Monitor.Enter( Owner.SyncEvtShmemCH );
                try
                {
                    var shmemN = ss[ 1 ].Trim();
                    if ( Owner.EvtShmemCHs.TryGetValue( shmemN, out var ch ) )
                    {
                        Owner.EvtShmemCHs.Remove( shmemN );
                        ch.Dispose();

                        goto ProcessRequest_OK;
                    }
                }
                catch { }
                finally { Monitor.Exit( Owner.SyncEvtShmemCH ); }

                goto ProcessRequest_NG;
            }
            else if (cmd == Cmd.ConfigCallingScript.ToString() )
            {
                // by shmem name
                try
                {
                    var shmemName = ss[ 1 ].Trim();
                    var toWhich = ss[ 2 ].Trim().ToLower();
                    var scriptName = ss[ 3 ].Trim();
                    EvtShmemChannel ch = null;

                    if ( shmemName == Owner.DefaultEvtShmemCH.ShmemName )
                        ch = Owner.DefaultEvtShmemCH;
                    else
                        Owner.EvtShmemCHs.TryGetValue( shmemName, out ch );

                    if (ch != null && ULibAgent.Singleton.Scripts.GetScript(scriptName) != null)
                    {
                        if ( toWhich == "buffer" ) ch.RxBufferCall = scriptName;
                        else if ( toWhich == "video" ) ch.RxVideoCall = scriptName;
                        else if ( toWhich == "folder" ) ch.RxFolderCall = scriptName;
                        else
                            goto ProcessRequest_NG;

                        goto ProcessRequest_OK;
                    }
                }
                catch { }

                goto ProcessRequest_NG;
            }

            Log?.Invoke( $"{rx}" );

ProcessRequest_OK:
            rspDat = Encoding.UTF8.GetBytes( strOK );
            nOffsetIdx = 0;
            nDatLen = rspDat.Length;
            return;

ProcessRequest_NG:
            rspDat = Encoding.UTF8.GetBytes( strNG );
            nOffsetIdx = 0;
            nDatLen = rspDat.Length;
            return;
        }
    }
}
