using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uIP.Lib
{
    public class FuncDelegateCalls
    {
        private object Sync { get; set; } = new object();
        private Dictionary<string, Func<UDataCarrier, Action<int, string>, bool>> CallList { get; set; } = new Dictionary<string, Func<UDataCarrier, Action<int, string>, bool>>();
        public FuncDelegateCalls() { }

        public bool Add( string givenName, Func<UDataCarrier, Action<int, string>, bool> func )
        {
            if ( string.IsNullOrWhiteSpace( givenName ) || func == null )
                return false;

            var status = true;
            Monitor.Enter( Sync );
            try
            {
                if ( CallList.ContainsKey( givenName ) )
                    status = false;
                else
                    CallList.Add( givenName, func );
            }
            finally { Monitor.Exit( Sync ); }

            return status;
        }

        public void Calling( UDataCarrier input, out bool status, Action<int, string> log = null, bool bStopOnErr = true, int nLogLevel = 0)
        {
            status = true;
            var errCall = new List<string>();
            Monitor.Enter( Sync );
            try
            {
                foreach(var kv in CallList)
                {
                    var ret = kv.Value?.Invoke( input, log ) ?? false;
                    if (!ret)
                    {
                        status = false;
                        errCall.Add( kv.Key );
                        if ( bStopOnErr )
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                status = false;
                log?.Invoke( nLogLevel, $"Call {errCall} with error:\n{e}" );
            }
            finally
            {
                if ( !status )
                    log?.Invoke( nLogLevel, $"Call with fail in following: {string.Join( ", ", errCall.ToArray() )}" );
                Monitor.Exit( Sync );
            }
        }
    }
}
