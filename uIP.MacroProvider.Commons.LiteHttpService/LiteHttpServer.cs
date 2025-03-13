using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using uIP.Lib;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.Commons.LiteHttpService
{
    public class LiteHttpServer : UMacroMethodProviderPlugin
    {
        FormTestLiteHttpServ TestForm { get; set; } = null;
        string WD { get; set; } = "";
        int ServPort { get; set; } = 3971;
        HttpListener Serv { get; set; }
        Thread ServRunner { get; set; }
        private HttpListener Server { get; set; } = null;

        private object SyncHandler { get; set; } = new object();
        private Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>> PostFullControlHandler { get; set; } = new Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>>();
        private Dictionary<string, Func<Uri, string, List<string>>> PostWithReturnJsonHandler { get; set; } = new Dictionary<string, Func<Uri, string, List<string>>>();

        private Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>> GetFullControlHandler { get; set; } = new Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>>();
        private Dictionary<string, Func<Uri, List<object>>> GetFileControlHandler { get; set; } = new Dictionary<string, Func<Uri, List<object>>>();

        internal bool ServiceAlive => Serv != null && Serv.IsListening;

        public LiteHttpServer() { }
        public override bool Initialize( UDataCarrier[] param )
        {
            // check working dir exists
            if ( !UDataCarrier.GetByIndex( param, 1, "", out var workingDir ) || !Directory.Exists( workingDir ) )
                return false;

            if ( m_bOpened )
                return true;

            //
            // start http service
            //
            // read port from ini
            WD = workingDir;
            var iniP = new IniReaderUtility();
            if ( iniP.Parsing( Path.Combine( WD, "inis", "system.ini" ) ) )
            {
                List<string[]> ret = null;
                if ( iniP.Get( "LiteHttpConf", new List<string>() { "Port" }, ref ret ) && ret != null && ret.Count > 0 && int.TryParse( ret[ 0 ][ 0 ], out var defP ) )
                    ServPort = defP;
            }
            // create resources to start service
            Serv = new HttpListener();
            Serv.Prefixes.Add( $"http://*:{ServPort}/" ); // config service port
            ServRunner = new Thread( new ThreadStart( HandleHttpReq ) );
            ServRunner.Start();

            //
            // add class function
            //
            var givenName = "";

            // POST with json
            givenName = "InstallPostReturnJson";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Install POST processing delegate by a given uri /<path>/<to>?q01=1&q02=2 and do not forget begin with /",
                    ArgsDescription = new string[]
                    {
                        "uri path: /<path>/<to>",
                        "function: arg0=Uri, arg1=post data; return=[0]: http status code, [1]: json data"
                    },
                    ReturnValueDescription = "NA",
                    Call = InstallPostReturnJson
                }
            );
            givenName = "RemovePostReturnJson";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Remove POST processing delegate by give uri",
                    ArgsDescription = new string[] { "uri path" },
                    ReturnValueDescription = "NA",
                    Call = RemovePostReturnJson
                }
            );

            // POST full processing
            givenName = "InstallPostFullProcessing";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Install POST processing delegate by given uri /<path>/<to>?q01=1&q02=2 and do not forget begin with /",
                    ArgsDescription = new string[]
                    {
                        "uri path: /<path>/<to>",
                        "function: arg01=HttpListenerRequest, arg02=HttpListenerResponse; delegate must response.Close() to reply"
                    },
                    ReturnValueDescription = "NA",
                    Call = InstallPostFullProcessing
                }
            );
            givenName = "RemovePostFullProcessing";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Remove POST processing delegate by give uri",
                    ArgsDescription = new string[] { "uri path" },
                    ReturnValueDescription = "NA",
                    Call = RemovePostFullProcessing
                }
            );

            // GET file
            givenName = "InstallGetFile";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Install GetDicKeyStrOne file processing delegate by a given uri /<path>/<to>/<file>?q01=1&q02=2 and do not forget begin with /",
                    ArgsDescription = new string[]
                    {
                        "uri path: /<path>/<to>",
                        "function: arg0=Uri; return=[0]: http status code(int), [1]: byte[] or Stream, [2]: file size(int)"
                    },
                    ReturnValueDescription = "NA",
                    Call = InstallGetFile
                }
            );
            givenName = "RemoveGetFile";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Remove POST processing delegate by give uri",
                    ArgsDescription = new string[] { "uri path" },
                    ReturnValueDescription = "NA",
                    Call = RemoveGetFile
                }
            );

            // GET full processing
            givenName = "InstallGetFullProcessing";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Install POST processing delegate by given uri /<path>/<to>?q01=1&q02=2 and do not forget begin with /",
                    ArgsDescription = new string[]
                    {
                        "uri path: /<path>/<to>",
                        "function: arg01=HttpListenerRequest, arg02=HttpListenerResponse; delegate must response.Close() to reply"
                    },
                    ReturnValueDescription = "NA",
                    Call = InstallGetFullProcessing
                }
            );
            givenName = "RemoveGetFullProcessing";
            m_PluginClassProvideFuncs.Add(
                givenName,
                new PluginClassProvideFunc()
                {
                    Description = "Remove POST processing delegate by give uri",
                    ArgsDescription = new string[] { "uri path" },
                    ReturnValueDescription = "NA",
                    Call = RemoveGetFullProcessing
                }
            );

            // class ioctl
            givenName = "TestForm";
            m_PluginClassControls.Add( givenName, new UScriptControlCarrierPluginClass( givenName, false, true, false, null,
                null, OpenTestForm
                )
            );

            m_bOpened = true;
            return true;
        }

        public override void Close()
        {
            // call base to handle resource
            base.Close();
            // handle current resources
            Serv?.Close();
            ServRunner?.Join();
            Serv = null;
            ServRunner = null;
            // dispose test form if need
            TestForm?.Dispose();
            TestForm = null;
            // clear all delegate
            Monitor.Enter( SyncHandler );
            try
            {
                PostFullControlHandler.Clear();
                PostWithReturnJsonHandler.Clear();
                GetFullControlHandler.Clear();
                GetFileControlHandler.Clear();
            }
            catch { }
            finally {  Monitor.Exit( SyncHandler ); }
        }

        bool OpenTestForm( UScriptControlCarrier carrier, UDataCarrier[] data )
        {
            if ( TestForm == null )
            {
                TestForm = new FormTestLiteHttpServ() { RefInstance = this };
                TestForm.InstallTest();
            }

            TestForm?.Show();

            return true;
        }

        internal bool InstallPostReturnJson( out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;
            if ( !UDataCarrier.GetByIndex<Func<Uri, string, List<string>>>( args, 1, null, out var handler ) || handler == null )
                return false;

            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                if ( PostWithReturnJsonHandler.ContainsKey( uri ) )
                    PostWithReturnJsonHandler[ uri ] = handler;
                else
                    PostWithReturnJsonHandler.Add( uri, handler );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }

            return status;
        }
        internal bool RemovePostReturnJson( out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;
            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                PostWithReturnJsonHandler.Remove( uri );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }
            return status;
        }

        internal bool InstallPostFullProcessing(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;
            if ( !UDataCarrier.GetByIndex<Action<HttpListenerRequest, HttpListenerResponse>>( args, 1, null, out var handler ) || handler == null )
                return false;

            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                if ( PostFullControlHandler.ContainsKey( uri ) )
                    PostFullControlHandler[ uri ] = handler;
                else
                    PostFullControlHandler.Add( uri, handler );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }

            return status;
        }
        internal bool RemovePostFullProcessing( out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;

            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                PostFullControlHandler.Remove( uri );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }
            return status;
        }

        internal bool InstallGetFile(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;
            if ( !UDataCarrier.GetByIndex<Func<Uri, List<object>>>( args, 1, null, out var handler ) || handler == null )
                return false;

            var status = true;
            Monitor.Enter(SyncHandler );
            try
            {
                if ( GetFileControlHandler.ContainsKey( uri ) )
                    GetFileControlHandler[ uri ] = handler;
                else
                    GetFileControlHandler.Add( uri, handler );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }

            return status;
        }
        internal bool RemoveGetFile( out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;
            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                GetFileControlHandler.Remove( uri );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }
            return status;
        }

        internal bool InstallGetFullProcessing( out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;
            if ( !UDataCarrier.GetByIndex<Action<HttpListenerRequest, HttpListenerResponse>>( args, 1, null, out var handler ) || handler == null )
                return false;

            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                if ( GetFullControlHandler.ContainsKey( uri ) )
                    GetFullControlHandler[ uri ] = handler;
                else
                    GetFullControlHandler.Add( uri, handler );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }
            return status;
        }
        internal bool RemoveGetFullProcessing( out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var uri ) || string.IsNullOrEmpty( uri ) )
                return false;

            var status = true;
            Monitor.Enter( SyncHandler );
            try
            {
                GetFullControlHandler.Remove( uri );
            }
            catch { status = false; }
            finally { Monitor.Exit( SyncHandler ); }
            return status;
        }

        private static bool GetPathHandler<T>( Dictionary<string, T> dic, string path, T defaultV, out T handler )
        {
            handler = defaultV;
            if ( dic == null )
                return false;
            var status = false;
            foreach ( var kv in dic )
            {
                if ( kv.Key.IndexOf( path, StringComparison.InvariantCultureIgnoreCase ) == 0 )
                {
                    status = true;
                    handler = kv.Value; break;
                }
            }
            return status;
        }

        private static bool GetPathDirHandler<T>(Dictionary<string, T> dic, string path, T defaultV, out T handler)
        {
            handler = defaultV;
            if ( dic == null )
                return false;
            var status = false;
            foreach(var kv in dic)
            {
                var dir = Path.GetDirectoryName( kv.Key ).Replace( '\\', '/' );
                if ( dir.IndexOf(path, StringComparison.InvariantCultureIgnoreCase ) == 0)
                {
                    status = true;
                    handler = kv.Value; break;
                }
            }
            return status;
        }

        /// <summary>
        /// Thread function to process incoming request
        /// - POST method available
        /// - GET method available
        /// </summary>
        private void HandleHttpReq()
        {
            if ( Serv == null )
                return;
            try
            {
                Serv.Start();
            }
            catch ( Exception ex )
            {
                fpLog?.Invoke( eLogMessageType.WARNING, 1000, $"Http error:\n{CommonUtilities.Dump01( ex )}" );
                return;
            }
            while ( Serv.IsListening )
            {
                HttpListenerContext ctx = null;
                try
                {
                    ctx = Serv.GetContext();
                    var req = ctx.Request;
                    var rsp = ctx.Response;
                    // POST method
                    if ( req.HttpMethod.ToLower() == "post" )
                    {
                        Func<Uri, string, List<string>> jsonHandler = null;
                        Action<HttpListenerRequest, HttpListenerResponse> fullHanlder = null;

                        Monitor.Enter( SyncHandler );
                        try
                        {
                            if ( GetPathHandler( PostWithReturnJsonHandler, req.Url.LocalPath, null, out jsonHandler ) ) { }
                            else if ( GetPathHandler( PostFullControlHandler, req.Url.LocalPath, null, out fullHanlder ) ) { }
                        }
                        finally { Monitor.Exit( SyncHandler ); }

                        var status = true;
                        // Func return
                        // - [0]: status code in integer
                        // - [1]: json string for report
                        if ( jsonHandler != null )
                        {
                            string content = "";
                            using ( var rd = new StreamReader( req.InputStream, req.ContentEncoding ) )
                            {
                                content = rd.ReadToEnd();
                            }
                            var got = jsonHandler.Invoke( req.Url, content );
                            rsp.StatusCode = int.Parse( got[ 0 ] );
                            rsp.Headers.Add( "Access-Control-Allow-Orgin", "*" );
                            rsp.ContentType = "application/json";
                            rsp.ContentEncoding = Encoding.UTF8;
                            if ( !string.IsNullOrEmpty( got[ 1 ] ) )
                            {
                                var bytes = Encoding.UTF8.GetBytes( got[ 1 ] );
                                rsp.OutputStream.Write( bytes, 0, bytes.Length );
                                rsp.OutputStream.Close();
                            }
                            rsp.Close();
                        }
                        // Action
                        // - must be handle and close response
                        else if ( fullHanlder != null )
                        {
                            fullHanlder( req, rsp );
                        }
                        else
                            status = false;

                        if ( status )
                            continue;
                    }
                    // GET Method
                    else if ( req.HttpMethod.ToLower() == "get" )
                    {
                        Func<Uri, List<object>> getFileHandler = null;
                        Action<HttpListenerRequest, HttpListenerResponse> fullHandler = null;

                        Monitor.Enter( SyncHandler );
                        try
                        {
                            if ( GetPathDirHandler( GetFileControlHandler, req.Url.LocalPath, null, out getFileHandler ) ) { }
                            else if ( GetPathDirHandler( GetFullControlHandler, req.Url.LocalPath, null, out fullHandler ) ) { }
                        }
                        finally { Monitor.Exit( SyncHandler ); }

                        var status = true;
                        // Return
                        // - [0]: status code
                        // - [1]: byte/ stream
                        // - [2]: size
                        if ( getFileHandler != null )
                        {
                            var valid = false;
                            var ret = getFileHandler.Invoke( req.Url );
                            rsp.StatusCode = ( int )ret[ 0 ];
                            if ( (int)ret[0] == (int)HttpStatusCode.OK)
                            {
                                rsp.ContentType = MimeMapping.GetMimeMapping( Path.GetFileName( req.Url.AbsolutePath ) );
                                rsp.ContentLength64 = ( int )ret[ 2 ];
                                rsp.AddHeader( "Date", DateTime.Now.ToString( "r" ) );
                            }

                            valid = true;
                            if ( valid && ret[ 1 ] is Stream s )
                            {
                                // write data from stream
                                byte[] rdBA = new byte[ 1024 * 16 ];
                                int nRd = 0;
                                while ( ( nRd = s.Read( rdBA, 0, rdBA.Length ) ) > 0 )
                                {
                                    rsp.OutputStream.Write( rdBA, 0, nRd );
                                }
                                // free stream resource
                                s.Close();
                                s.Dispose();
                            }
                            else if ( valid && ret[ 1 ] is byte[] barr )
                            {
                                rsp.OutputStream.Write( barr, 0, barr.Length );
                            }
                            else
                                status = false;

                            if ( status )
                            {
                                rsp.Close();
                                continue;
                            }

                        }
                        else if ( fullHandler != null )
                        {
                            fullHandler.Invoke( req, rsp );
                        }
                        else
                            status = false;

                        if ( status )
                            continue;
                    }

                    rsp.StatusCode = ( int )HttpStatusCode.NotFound;
                    rsp.StatusDescription = "Not found request";
                    rsp.Close();
                }
                catch ( Exception ex )
                {
                    fpLog?.Invoke( eLogMessageType.WARNING, 1000, $"Http error:\n{CommonUtilities.Dump01( ex )}" );
                    if ( !Serv.IsListening )
                        break;
                    if ( ctx != null )
                    {
                        ctx.Response.StatusCode = ( int )HttpStatusCode.BadRequest;
                        ctx.Response.Close();
                    }
                }
            }
        }

    }
}
