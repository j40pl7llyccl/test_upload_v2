using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace uIP.Lib
{
    public static class ResourceManager
    {
        public const string LogDelegate = "LogDelegate";
        public const string LibAgent = "LibAgent";
        public const string PluginServiceName = "PluginService";
        public const string ScriptService = "ScriptService";
        public const string ScriptRunnerFactory = "ScriptRunnerFactory";
        public const string ActionService = "ActionService";
        public const string ScriptEditor = "ScriptEditor";
        public const string SystemUp = "SystemUp";
        public const string MainWindow = "MainWindow";
        public const string ProgGuid = "ProgGuid";

        private static object _sync = new object();
        private static Dictionary<string, object> _Resources = new Dictionary<string, object>();
        private static Form _MainWindow = null;

        public static void Reg( string name, object res )
        {
            if ( String.IsNullOrEmpty( name ) )
                return;

            Monitor.Enter( _sync );
            if ( _Resources.ContainsKey( name ) )
                _Resources[ name ] = res;
            else
                _Resources.Add( name, res );

            if ( name == MainWindow )
                _MainWindow = res as Form;

            Monitor.Exit( _sync );
        }
        public static void Unreg( string name )
        {
            Monitor.Enter( _sync );
            if ( _Resources.ContainsKey( name ) )
                _Resources.Remove( name );
            Monitor.Exit( _sync );
        }
        public static void Clear()
        {
            Monitor.Enter( _sync );
            _Resources.Clear();
            Monitor.Exit( _sync );
        }
        public static object Get( string name )
        {
            object repo = null;

            Monitor.Enter( _sync );
            if ( _Resources.ContainsKey( name ) ) { 
                repo = _Resources[name];
            }
            Monitor.Exit( _sync );

            return repo;
        }
        public static bool Get<T>( string name, T defaultV, out T result )
        {
            var status = false;
            result = defaultV;

            Monitor.Enter( _sync );
            if ( _Resources.TryGetValue( name, out var item ) && item != null && ( item is T v ) )
            {
                result = v;
                status = true;
            }
            Monitor.Exit( _sync );

            return status;
        }

        public static bool IsSystemUp()
        {
            bool repo = false;
            Monitor.Enter( _sync );
            if ( _Resources.ContainsKey( SystemUp ) )
                repo = ( bool ) _Resources[ SystemUp ];
            Monitor.Exit( _sync );
            return repo;
        }

        public static bool SystemAvaliable => ULibAgent.Singleton != null && ULibAgent.Singleton.Available;

        /// <summary>
        /// UDataCarrier data from different PC or program using last match can be enable
        /// Remark:
        /// Disable => Some conditions will case type exception such Point in System.Drawing; ReadXml use exactly assembly can avoid this issue
        /// </summary>
        public static bool DataCarrierTypePerfectMatch { get; set; } = true;

        internal class ActionCalls
        {
            internal object Sync { get; set; } = new object();
            internal bool Called { get; set; } = false;
            internal List<Action> Calls { get; set; } = new List<Action>();
            internal ActionCalls() { }

            internal void Add( Action call )
            {
                if ( call == null ) return;
                Monitor.Enter( Sync );
                try
                {
                    if ( !Calls.Contains( call ) ) Calls.Add( call );
                }
                finally { Monitor.Exit( Sync ); }
            }
            internal void ExecOnce()
            {
                if ( Called )
                    return;

                Called = true;

                Monitor.Enter( Sync );
                try
                {
                    foreach ( Action call in Calls )
                    {
                        call?.Invoke();
                    }
                }
                catch { }
                finally { Monitor.Exit( Sync ); }
            }
        }

        private static ActionCalls SystemUpAction { get; set; } = new ActionCalls();
        public static void AddSystemUpCalls( Action call )
        {
            SystemUpAction.Add( call );
        }
        public static void SystemUpCall()
        {
            SystemUpAction.ExecOnce();
        }

        private static ActionCalls SystemDownAction { get; set; } = new ActionCalls();
        public static void AddSystemDownCalls(Action call)
        {
            SystemDownAction.Add( call );
        }
        public static void SystemDownCall()
        {
            SystemDownAction.ExecOnce();
        }

        #region MainThread OP
        public static void InvokeMainThreadFreeResource( object ctx )
        {
            if ( _MainWindow != null && _MainWindow.InvokeRequired )
            {
                _MainWindow.Invoke( new System.Action( () => UDataCarrier.Free( ctx ) ) );
            }
            else
                UDataCarrier.Free( ctx );
        }

        public static object InvokeMainThread( Func<object, object> act, object ctx )
        {
            if ( act == null )
                return null;

            if ( _MainWindow != null && _MainWindow.InvokeRequired )
            {
                return _MainWindow.Invoke( new Func<object, object>( act ), ctx );
            }

            return act.Invoke( ctx );
        }
        public static void InvokeMainThread(Action call)
        {
            if ( call == null )
                return;

            if ( _MainWindow != null && _MainWindow.InvokeRequired )
            {
                _MainWindow.Invoke( new Action(call) );
                return;
            }

            call();
        }
        public static void ForceInvokeMainThread( Action call)
        {
            if ( call == null ) return;
            if ( _MainWindow == null )
            {
                call();
                return;
            }

            _MainWindow.Invoke( new Action( call ) );
        }

        public static IAsyncResult BeginInvokeMainThread( object ctx, Action<object> call )
        {
            if ( call == null )
                return null;

            if ( _MainWindow == null )
            {
                call( ctx );
                return null;
            }

            return _MainWindow.BeginInvoke( new Action<object>( call ), ctx );
        }

        public static IAsyncResult BeginInvokeMainThread( Action call )
        {
            if ( call == null )
                return null;

            if ( _MainWindow == null )
            {
                call();
                return null;
            }

            return _MainWindow.BeginInvoke( new Action( call ) );
        }

        public static object EndInvokeMainThread( IAsyncResult ar )
        {
            if ( ar == null || _MainWindow == null )
                return null;

            return _MainWindow.EndInvoke( ar );
        }

        public static bool IsMainThreadRunning()
        {
            if ( _MainWindow == null )
                return false;

            return !_MainWindow.InvokeRequired;
        }

        public static void ThreadPoolingWaiting()
        {
            if ( _MainWindow != null && _MainWindow.InvokeRequired )
                Application.DoEvents();
            else
                Thread.Sleep( 1 );
        }
        #endregion
    }
}
