using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using uIP.Lib;

namespace uIP.MacroProvider.DB.Sqlite
{
    public partial class SqliteOp
    {
        bool EndProcessing { get; set; } = false;
        Thread tExec { get; set; } = null;

        object SyncExecList { get; set; } = new object();
        List<UDataCarrier[]> ExecStrings { get; set; } = new List<UDataCarrier[]>();
        AutoResetEvent NotifyExecEvt { get; set; } = new AutoResetEvent( false );

        void InitAsyncExec()
        {
            if ( tExec != null )
                return;

            tExec = new Thread( ExecQuery );
            tExec.Start();
        }

        void EndAsyncExec()
        {
            EndProcessing = true;
            NotifyExecEvt.Set();

            tExec?.Join();
        }

        internal bool AddQuery(params UDataCarrier[] q)
        {
            if ( EndProcessing || q == null || q.Length == 0 )
                return false;

            var sync = false;
            while(m_bOpened)
            {
                if (Monitor.TryEnter(SyncExecList, 0))
                {
                    sync = true;
                    break;
                }
                ResourceManager.ThreadPoolingWaiting();
            }
            if ( !sync )
                return false;


            try
            {
                ExecStrings.Add( q );
                NotifyExecEvt.Set();
            }
            finally { Monitor.Exit(SyncExecList); }

            return true;
        }

        void ExecQuery()
        {
            while(true)
            {
                if ( ExecStrings.Count <= 0)
                    NotifyExecEvt.WaitOne();

                if (ExecStrings.Count > 0)
                {
                    var one = ExecStrings.First();
                    ExecStrings.RemoveAt(0);

                    if ( !Exec( out _, one ) )
                    {
                        var arr = ( from i in one where i != null && i.Data != null select i.Data.ToString() ).ToArray();
                        fpLog?.Invoke( eLogMessageType.WARNING, 0, $"DB exec fail:\n{string.Join( "\n", arr )}" );
                    }
                }

                if ( ExecStrings.Count == 0 && EndProcessing )
                    break;
            }
        }
    }
}
