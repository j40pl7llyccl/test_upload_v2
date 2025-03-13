using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;

using uIP.Lib.Utility;
using uIP.Lib.Script;

using Ionic.Zip;

namespace uIP.Lib.Service
{
    internal class UPluginAssemblyInfo
    {
        public UMacroMethodProviderPlugin _OpInstance = null;
        public string _strFilePath = null;
        public bool _bInitStatus = false;

        public UPluginAssemblyInfo() { }
    }

    public class UPluginAssemblyService : IDisposable, IDatIO
    {
        private static string _strMainSectionName = "Assemblies";
        private static string _strMainSectionAssemblyPathKey = "Path";
        #region >>> Variable Decl <<<
        // Flag
        private bool m_bOpened = false;
        private bool m_bReady = false;
        private bool m_bDisposing = false;
        private bool m_bDisposed = false;

        // Parameter
        private string m_strPluginRootPath = null;
        private string m_strWorkingRootPath = null;
        private string m_strIniFileFolderPath = null;
        private string m_strAssemblyIniFilePath = null;

        private string m_strAssemblyRWPath = null;

        // Record all assemblies
        private List<UMacroMethodProviderPlugin> m_listLoadedPluginInst = new List<UMacroMethodProviderPlugin>();
        private List<UPluginAssemblyInfo> m_listLoadingAssembInfo = new List<UPluginAssemblyInfo>();
        private Dictionary<string, UPluginAssemblyInfo> m_refClassFullnameLoadedPlugins = new Dictionary<string, UPluginAssemblyInfo>();
        private List<string> m_AppendedEnvPath = new List<string>();

        // Log handler
        fpLogMessage m_fpLog = null;

        // not create specified class in an assembly
        private static List<string> m_IgnoredAssemblyNames = new List<string>();

        #endregion

        #region [Property]
        public List<UMacroMethodProviderPlugin> PluginAssemblies { get { return m_listLoadedPluginInst; } }
        public static string IgnoredAssemblyName
        {
            set
            {
                if ( String.IsNullOrEmpty( value ) ) return;
                for ( int i = 0 ; i < m_IgnoredAssemblyNames.Count ; i++ )
                {
                    if ( String.IsNullOrEmpty( m_IgnoredAssemblyNames[ i ] ) )
                        continue;
                    if ( m_IgnoredAssemblyNames[ i ].ToLower() == value.ToLower() )
                        return;
                }
                m_IgnoredAssemblyNames.Add( value );
            }
        }
        public fpLogMessage fpLog { get { return m_fpLog; } }

        #endregion

        #region [Provide a way to find the assembly]

        // Also can be found in specific dir by assembly name
        //private static Assembly ResolveCurrDomainAssembly( Object sender, ResolveEventArgs args )
        //{
        //    Assembly[] curr = AppDomain.CurrentDomain.GetAssemblies();
        //    if ( curr == null || curr.Length <= 0 )
        //        return null;

        //    string[] fields = args.Name.Split( ',' );
        //    string assemNm = fields[ 0 ];

        //    for ( int i = 0 ; i < curr.Length ; i++ )
        //    {
        //        if ( curr[ i ] == null || String.IsNullOrEmpty( curr[ i ].FullName ) )
        //            continue;
        //        string[] tmp = curr[ i ].FullName.Split( ',' );
        //        if ( assemNm == tmp[ 0 ] )
        //            return curr[ i ];
        //    }

        //    return null;
        //}

        #endregion

        #region [Constructor]
        public UPluginAssemblyService( string pluginRootPath,
                                      string workingRootPath,
                                      string iniFileFolderPath,
                                      string assemIniFileNm,
                                      string assemblyRWPath,
                                      fpLogMessage fpLog
                                    )
        {
            m_strWorkingRootPath = workingRootPath;

            // Loading assembly/ DLL root reference path
            if ( !String.IsNullOrEmpty( pluginRootPath ) && Directory.Exists( pluginRootPath ) )
                m_strPluginRootPath = String.Copy( pluginRootPath );
            // Init path
            if ( !String.IsNullOrEmpty( iniFileFolderPath ) && Directory.Exists( iniFileFolderPath ) )
            {
                m_strIniFileFolderPath = CommonUtilities.RemoveEndDirSymbol( iniFileFolderPath );

                if ( !String.IsNullOrEmpty( assemIniFileNm ) )
                {
                    string path = Path.Combine( m_strIniFileFolderPath, assemIniFileNm );
                    if ( File.Exists( path ) )
                        m_strAssemblyIniFilePath = path;
                }
            }
            // Assembly RW Path
            if ( !String.IsNullOrEmpty( assemblyRWPath ) ) {
                if (CommonUtilities.RCreateDir(assemblyRWPath))
                    m_strAssemblyRWPath = String.Copy( assemblyRWPath );
            }

            // Create UI instance
            //m_frmUI = new frmLoadedLibInfo( this );

            // Log Handler
            m_fpLog = fpLog;

        }
        #endregion

        #region [Dispose]

        ~UPluginAssemblyService()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
        }

        private void Dispose( bool bDisposing )
        {
            if ( m_bDisposing ) return;
            m_bDisposing = true;

            // clear
            m_refClassFullnameLoadedPlugins = new Dictionary<string, UPluginAssemblyInfo>();

            Close();

            if (Directory.Exists(m_strAssemblyRWPath)) {
                try { Directory.Delete( m_strAssemblyRWPath, true ); } catch { }
            }

            if ( bDisposing )
                GC.SuppressFinalize( this );

            m_bDisposed = true;
        }

        private void Close()
        {
            m_bReady = false;
            m_bOpened = false;
            if ( m_listLoadingAssembInfo != null )
            {
                // Reverse to close
                for ( int i = m_listLoadingAssembInfo.Count - 1 ; i >= 0 ; i-- )
                {
                    if ( m_listLoadingAssembInfo[ i ] == null ) continue;
                    if ( m_listLoadingAssembInfo[ i ]._OpInstance != null )
                        m_listLoadingAssembInfo[ i ]._OpInstance.Close();
                }
                m_listLoadingAssembInfo.Clear();
                m_listLoadingAssembInfo = null;
            }

            if ( m_listLoadedPluginInst != null )
            {
                m_listLoadedPluginInst.Clear();
                m_listLoadedPluginInst = null;
            }

            if ( _controlPluginClassConfig != null) {
                _controlPluginClassConfig.Dispose();
                _controlPluginClassConfig = null;
            }
        }

        #endregion

        #region >>> Open <<<
        public bool Open()
        {
            if ( m_bOpened ) return true;

            // Path not ready
            if ( String.IsNullOrEmpty( m_strPluginRootPath ) )
                return false;
            if ( String.IsNullOrEmpty( m_strAssemblyIniFilePath ) )
                return false;

            // default path for common used
            // <Working Dir>/ThirdPartyLibs
            AddEnvVarPath( m_AppendedEnvPath, CommonUtilities.RCreateDir2( Path.Combine( m_strWorkingRootPath, "ThirdPartyLibs" ) ) );

            // Read ini information
            bool bAssem = LoadAssembly();

            m_bReady = bAssem;
            m_bOpened = true;

            CommonUtilities.AddResolveCurrDomainAssembly();
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler( ResolveCurrDomainAssembly );

            // Call initialize done
            InitializeDoneCall( m_listLoadingAssembInfo );

            // make reference
            m_refClassFullnameLoadedPlugins = new Dictionary<string, UPluginAssemblyInfo>();
            foreach ( var m in m_listLoadingAssembInfo )
            {
                if ( m != null && m._OpInstance != null && !string.IsNullOrEmpty( m._OpInstance.NameOfCSharpDefClass ) )
                {
                    m_refClassFullnameLoadedPlugins.Add( m._OpInstance.NameOfCSharpDefClass, m );
                }
            }

            return true;
        }

        private static bool IsTypeInIgnoreAssemblies( string assemblyNm )
        {
            if ( String.IsNullOrEmpty( assemblyNm ) )
                return false;
            assemblyNm = assemblyNm.ToLower();
            for ( int i = 0 ; i < m_IgnoredAssemblyNames.Count ; i++ )
            {
                if ( String.IsNullOrEmpty( m_IgnoredAssemblyNames[ i ] ) )
                    continue;
                string name = Path.GetFileNameWithoutExtension( m_IgnoredAssemblyNames[ i ] ).ToLower();
                if ( name == assemblyNm )
                    return true;
            }
            return false;
        }
        private static string AssemblyNameOnly( Assembly assembly )
        {
            if ( assembly == null ) return null;
            string[] spl = assembly.FullName.Split( ',' );
            return spl[ 0 ];
        }

        private static void AddEnvVarPath(List<string> lst, string path)
        {
            if ( String.IsNullOrEmpty( path ) || !Directory.Exists( path ) )
                return;

            bool exist = false;
            for(int i = 0 ; i < lst.Count ; i++ )
            {
                if(lst[i].ToLower() == path.ToLower())
                {
                    exist = true; break;
                }
            }

            if ( !exist )
            {
                Environment.SetEnvironmentVariable( "PATH", Environment.GetEnvironmentVariable( "PATH" ) + ";" + path );
                lst.Add( path );
            }
        }

        private static List<UPluginAssemblyInfo> LoadAssembly( string assemblyFilePath, List<string> fullTypNm, fpLogMessage fpLog )
        {
            Assembly a = null;
            if ( String.IsNullOrEmpty( assemblyFilePath ) || !File.Exists( assemblyFilePath ) )
                return null;

            try { a = Assembly.LoadFile( assemblyFilePath ); } catch ( Exception exp ) {
                a = null;
                fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                    $"Load assembly {assemblyFilePath} with error {exp.ToString()}" );
            }
            if ( a == null ) return null;

            List<Type> tmpTps = new List<Type>();
            for(int i = 0 ; i < fullTypNm.Count ; i++ ) {
                Type tp = a.GetType( fullTypNm[ i ] );
                if ( tp == null ) continue;
                tmpTps.Add( tp );
            }
            if ( tmpTps.Count <= 0 )
                return null;

            Type[] tps = tmpTps.ToArray();
            List<UPluginAssemblyInfo> ret = new List<UPluginAssemblyInfo>();
            foreach ( Type tp in tps ) {
                if ( tp.IsClass && !tp.IsAbstract && tp.IsSubclassOf( typeof( UMacroMethodProviderPlugin ) ) ) {
                    //if ( IsTypeInIgnoreAssemblies( AssemblyNameOnly( tp.Assembly ) ) )
                    //    continue; // it is in ignore, so not using this one

                    UPluginAssemblyInfo info = new UPluginAssemblyInfo();
                    if ( info != null ) {
                        try { info._OpInstance = Activator.CreateInstance( tp ) as UMacroMethodProviderPlugin; } catch ( Exception exp ) {
                            info._OpInstance = null;
                            fpLog?.Invoke( eLogMessageType.NORMAL, 0,
                                $"Activate {tp.FullName} with error {exp.ToString()} in file {assemblyFilePath}" );
                        }
                        if ( info._OpInstance != null ) {
                            info._OpInstance.NameOfCSharpDefClass = tp.FullName;
                            //info._strFilePath = String.Copy( sec.Data[ i ].Values[ 0 ] );
                            info._strFilePath = String.Copy( assemblyFilePath );
                            info._OpInstance.OwnerAssembly = a;

                            if ( fpLog != null )
                                info._OpInstance.fpLog = new fpLogMessage( fpLog );

                            ret.Add( info );
                        }
                    }
                }
            }

            return ret;

        }
        private static List<UPluginAssemblyInfo> LoadAssembly(string assemblyFilePath, fpLogMessage fpLog)
        {
            Assembly a = null;
            if ( String.IsNullOrEmpty( assemblyFilePath ) || !File.Exists( assemblyFilePath ) )
                return null;

            try { a = Assembly.LoadFile( assemblyFilePath ); }
            catch ( Exception exp )
            {
                a = null;
                if ( fpLog != null )
                    fpLog( eLogMessageType.NORMAL, 0, String.Format( "Load assembly {0} with error {1}", assemblyFilePath, exp.ToString() ) );
            }
            if ( a == null ) return null;

            Type[] tps = null;
            //try { tps = a.GetTypes(); }
            try { tps = a.GetExportedTypes(); }
            catch ( Exception exp )
            {
                tps = null;
                if ( fpLog != null )
                    fpLog( eLogMessageType.NORMAL, 0, String.Format( "get types error in file {0}: {1}", assemblyFilePath, exp.ToString() ) );
            }
            if ( tps == null ) return null;

            List<UPluginAssemblyInfo> ret = new List<UPluginAssemblyInfo>();
            foreach ( Type tp in tps )
            {
                if ( tp.IsClass && !tp.IsAbstract && tp.IsSubclassOf( typeof( UMacroMethodProviderPlugin ) ) )
                {
                    if ( IsTypeInIgnoreAssemblies( AssemblyNameOnly( tp.Assembly ) ) )
                        continue; // it is in ignore, so not using this one

                    UPluginAssemblyInfo info = new UPluginAssemblyInfo();
                    if ( info != null )
                    {
                        try { info._OpInstance = Activator.CreateInstance( tp ) as UMacroMethodProviderPlugin; }
                        catch ( Exception exp )
                        {
                            info._OpInstance = null;
                            if ( fpLog != null )
                                fpLog( eLogMessageType.NORMAL, 0, String.Format( "Activate {0} with error {1} in file {2}", tp.FullName, exp.ToString(), assemblyFilePath ) );
                        }
                        if ( info._OpInstance != null )
                        {
                            info._OpInstance.NameOfCSharpDefClass = tp.FullName;
                            //info._strFilePath = String.Copy( sec.Data[ i ].Values[ 0 ] );
                            info._strFilePath = String.Copy( assemblyFilePath );
                            info._OpInstance.OwnerAssembly = a;

                            if ( fpLog != null )
                                info._OpInstance.fpLog = new fpLogMessage( fpLog );

                            ret.Add( info );
                        }
                    }
                }
            }

            return ret;
        }

        private void InitializeDoneCall( List<UPluginAssemblyInfo> readyPlugins )
        {
            if ( readyPlugins == null ) return;
            for ( int i = 0 ; i < readyPlugins.Count ; i++ )
            {
                if ( readyPlugins[ i ] == null || readyPlugins[ i ]._OpInstance == null && readyPlugins[ i ]._bInitStatus ) continue;
                try
                {
                    readyPlugins[ i ]._OpInstance.InitializedDone1stChance( m_listLoadedPluginInst );
                }
                catch ( Exception exp )
                {
                    readyPlugins[ i ]._bInitStatus = false;
                    if ( m_fpLog != null )
                        m_fpLog(eLogMessageType.WARNING, 0, String.Format( "Notify {0} initialized done 1st chance fail with error {1}", readyPlugins[ i ]._strFilePath, exp.ToString() ) );
                }
            }
            for(int i = 0 ; i < readyPlugins.Count ; i++ )
            {
                if ( readyPlugins[ i ] == null || readyPlugins[ i ]._OpInstance == null && readyPlugins[ i ]._bInitStatus ) continue;
                try
                {
                    readyPlugins[ i ]._OpInstance.InitializedDone2ndChance();
                }
                catch ( Exception exp )
                {
                    readyPlugins[ i ]._bInitStatus = false;
                    if ( m_fpLog != null )
                        m_fpLog(eLogMessageType.WARNING, 0, String.Format( "Notify {0} initialized done 2nd fail with error {1}", readyPlugins[ i ]._strFilePath, exp.ToString() ) );
                }
            }
            for(int i = 0; i < readyPlugins.Count; i++ ) {
                if ( readyPlugins[ i ] == null || readyPlugins[ i ]._OpInstance == null || !readyPlugins[i]._bInitStatus )
                    continue;
                if ( String.IsNullOrEmpty( readyPlugins[ i ]._OpInstance.GivenName ) )
                    readyPlugins[ i ]._OpInstance.GivenName = readyPlugins[ i ]._OpInstance.GetType().FullName;
                //if ( String.IsNullOrEmpty( readyPlugins[ i ]._OpInstance.NameOfCSharpDefClass ) )
                //    readyPlugins[ i ]._OpInstance.NameOfCSharpDefClass = readyPlugins[ i ]._OpInstance.GetType().FullName;
            }
        }
        private bool LoadAssembly()
        {
            // Ini file content
            // [Assemblies]
            // Path=.\FolderName\aaa.dll <-- Relative folder
            IniReaderUtility ini = new IniReaderUtility();
            if ( !ini.Parsing( m_strAssemblyIniFilePath ) ) return false;
            SectionDataOfIni sec = ini.Get( _strMainSectionName );
            if ( sec == null || sec.Data == null || sec.Data.Count <= 0 ) return false;

            // Store previous path
            string pathPrev = Directory.GetCurrentDirectory();
            // Switch to new one
            if ( pathPrev.ToLower() != m_strPluginRootPath.ToLower() )
                Directory.SetCurrentDirectory( m_strPluginRootPath );

            // Loading Assembly
            for ( int i = 0 ; i < sec.Data.Count ; i++ )
            {
                if ( !(sec.Data[ i ].Key == _strMainSectionAssemblyPathKey &&
                    sec.Data[ i ].Values != null && sec.Data[ i ].Values.Length > 0 &&
                    !String.IsNullOrEmpty( sec.Data[ i ].Values[ 0 ] )) )
                    continue;
                string assemFilePath = Path.GetFullPath( String.Format( @"{0}\{1}", m_strPluginRootPath, sec.Data[ i ].Values[ 0 ].Replace( "\"", "" ).Trim() ) );
                if ( !File.Exists( assemFilePath ) )
                    continue;

                AddEnvVarPath( m_AppendedEnvPath, Path.GetDirectoryName( assemFilePath ) );

                List<string> fullTpNames = new List<string>();
                if (sec.Data[i].Values.Length > 1) {
                    for(int x = 1 ; x < sec.Data[i].Values.Length ; x++ ) {
                        if ( String.IsNullOrEmpty( sec.Data[ i ].Values[ x ] ) )
                            continue;
                        string cur = sec.Data[ i ].Values[ x ];
                        fullTpNames.Add( cur.Replace( "\"", "" ).Trim() );
                    }
                }

                List<UPluginAssemblyInfo> gotPlugins = null;
                if ( fullTpNames.Count > 0 ) gotPlugins = LoadAssembly( assemFilePath, fullTpNames, m_fpLog );
                else gotPlugins = LoadAssembly( assemFilePath, m_fpLog );
                if(gotPlugins != null && gotPlugins.Count > 0)
                {
                    for(int j = 0 ; j < gotPlugins.Count ; j++ )
                    {
                        m_listLoadedPluginInst.Add( gotPlugins[ j ]._OpInstance );
                        m_listLoadingAssembInfo.Add( gotPlugins[ j ] );
                    }
                }
                gotPlugins?.Clear();
            }

            UDataCarrier[] initializeParam = new UDataCarrier[] { new UDataCarrier( m_strAssemblyRWPath, typeof( string ), "Assembly RW path" ),
                                                                  new UDataCarrier( m_strWorkingRootPath, typeof( string), "Env specified working path" ) };

            // Call initialize
            for ( int i = 0 ; i < m_listLoadingAssembInfo.Count ; i++ )
            {
                if ( m_listLoadingAssembInfo[ i ] == null || m_listLoadingAssembInfo[ i ]._OpInstance == null ) continue;
                try
                {
                    m_listLoadingAssembInfo[ i ]._bInitStatus =
                        m_listLoadingAssembInfo[ i ]._OpInstance.Initialize( initializeParam );
                }
                catch ( Exception exp )
                {
                    m_listLoadingAssembInfo[ i ]._bInitStatus = false;
                    if ( m_fpLog != null )
                        m_fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call {0} initialize fail with error {1}", m_listLoadingAssembInfo[ i ]._strFilePath, exp.ToString() ) );
                }
            }

            return true;
        }

        public bool LoadAssembly( string filepath )
        {

            if ( String.IsNullOrEmpty( filepath ) || !File.Exists( filepath ) )
                return false;

            // check current environment
            Assembly[] envAssem = AppDomain.CurrentDomain.GetAssemblies();
            if ( envAssem != null && envAssem.Length > 0 )
            {
                string srcNmOnly = Path.GetFileNameWithoutExtension( filepath );
                srcNmOnly = srcNmOnly.ToLower();
                foreach ( Assembly aaa in envAssem )
                {
                    if ( String.IsNullOrEmpty( aaa.Location ) )
                        continue;
                    string nameonly = Path.GetFileNameWithoutExtension( aaa.Location );
                    if ( String.IsNullOrEmpty( nameonly ) )
                        continue;
                    if ( srcNmOnly == nameonly.ToLower() )
                        return true;
                }
            }

            AddEnvVarPath( m_AppendedEnvPath, Path.GetDirectoryName( filepath ) );
            List<UPluginAssemblyInfo> gotPlugins = LoadAssembly( filepath, m_fpLog );
            if ( gotPlugins != null && gotPlugins.Count > 0 )
            {
                for ( int j = 0 ; j < gotPlugins.Count ; j++ )
                {
                    m_listLoadedPluginInst.Add( gotPlugins[ j ]._OpInstance );
                    m_listLoadingAssembInfo.Add( gotPlugins[ j ] );
                }
                UDataCarrier[] initializeParam = new UDataCarrier[] { new UDataCarrier( m_strAssemblyRWPath, typeof( string ), "Assembly RW path" ),
                                                                      new UDataCarrier( m_strWorkingRootPath, typeof( string), "Env specified working path" ) };

                // Call initialize
                for ( int i = 0 ; i < gotPlugins.Count ; i++ )
                {
                    if ( gotPlugins[ i ] == null || gotPlugins[ i ]._OpInstance == null ) continue;
                    try
                    {
                        gotPlugins[ i ]._bInitStatus = gotPlugins[ i ]._OpInstance.Initialize( initializeParam );
                    }
                    catch ( Exception exp )
                    {
                        gotPlugins[ i ]._bInitStatus = false;
                        if ( m_fpLog != null )
                            m_fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call {0} initialize fail with error {1}", gotPlugins[ i ]._strFilePath, exp.ToString() ) );
                    }
                }
                // initialized done call
                InitializeDoneCall( gotPlugins );

                // make reference
                m_refClassFullnameLoadedPlugins = new Dictionary<string, UPluginAssemblyInfo>();
                foreach ( var m in m_listLoadingAssembInfo )
                {
                    if ( m != null && m._OpInstance != null && !string.IsNullOrEmpty( m._OpInstance.NameOfCSharpDefClass ) )
                    {
                        m_refClassFullnameLoadedPlugins.Add( m._OpInstance.NameOfCSharpDefClass, m );
                    }
                }

            }
            gotPlugins.Clear();
            gotPlugins = null;
            return true;
        }

        #endregion


        #region [ GET/ SET Class defined parameters ) ]

        public UDataCarrier[] GetPluginClassControl( string givenNameOfPluginClass, string cmdName, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( String.IsNullOrEmpty( cmdName ) ) return null;

            UMacroMethodProviderPlugin info = GetPluginInstanceFromGivenName( givenNameOfPluginClass );
            if ( info == null ) return null;

            return info.GetClassControl( UDataCarrier.MakeOne<string>( cmdName ), ref bRetStatus );
        }

        public bool SetPluginClassControl( string givenNameOfPluginClass, string cmdName, UDataCarrier[] data )
        {
            if ( String.IsNullOrEmpty( cmdName ) ) return false;

            UMacroMethodProviderPlugin info = GetPluginInstanceFromGivenName( givenNameOfPluginClass );
            if ( info == null ) return false;

            return (info.SetClassControl( UDataCarrier.MakeOne<string>( cmdName ), data ));
        }

        public bool SetPluginClassesControl( string cmdName, UDataCarrier[] data )
        {
            if ( String.IsNullOrEmpty( cmdName ) ) return false;
            // broadcast to every plugin class
            for ( int i = 0 ; i < m_listLoadingAssembInfo.Count ; i++ )
            {
                if ( m_listLoadingAssembInfo[ i ] != null && m_listLoadingAssembInfo[ i ]._OpInstance != null )
                    m_listLoadingAssembInfo[ i ]._OpInstance.SetClassControl( UDataCarrier.MakeOne<string>( cmdName ), data );
            }

            return true;
        }

        public void GetPluginClassesCmdTypeDescs(out UPluginAssemblyCmdList[] allPluginsClassCmds )
        {
            allPluginsClassCmds = null;
            if ( m_bDisposing || m_bDisposed || !m_bReady || !m_bOpened )
                return;


            int nCount = m_listLoadedPluginInst.Count;
            if ( nCount <= 0 )
                return;

            List<UPluginAssemblyCmdList> lst = new List<UPluginAssemblyCmdList>();
            for ( int i = 0; i < m_listLoadedPluginInst.Count; i++ )
            {
                if ( !m_listLoadedPluginInst[ i ].IsOpened )
                    continue;
                lst.Add( new UPluginAssemblyCmdList( m_listLoadedPluginInst[ i ], true ) );
            }

            allPluginsClassCmds = lst.ToArray();
        }
        public UDataCarrierTypeDescription[] GetPluginClassCmdTypeDescByGivenName( string givenNameOfPluginClass, string cmdName, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( String.IsNullOrEmpty( cmdName ) ) return null;

            UMacroMethodProviderPlugin plugin = GetPluginInstanceFromGivenName( givenNameOfPluginClass );
            if ( plugin == null ) return null;

            return plugin.GetClassControlDescription( UDataCarrier.MakeOne<string>( cmdName ), ref bRetStatus );
        }
        public UDataCarrierTypeDescription[] GetPluginClassCmdTypeDescByClassFullName(string nameOfCSharpClassName, string cmdName, out bool bRetStatus)
        {
            bRetStatus = false;
            if ( String.IsNullOrEmpty( cmdName ) ) return null;

            UMacroMethodProviderPlugin plugin = GetPluginInstanceByClassCSharpTypeName( nameOfCSharpClassName );
            if ( plugin == null ) return null;

            return plugin.GetClassControlDescription( UDataCarrier.MakeOne<string>( cmdName ), ref bRetStatus );
        }

        #region Class Params RW

        private string QueryFirstEleNode( string filePath )
        {
            if ( !File.Exists( filePath ) )
                return null;

            string ret = null;
            using ( Stream rs = File.Open( filePath, FileMode.Open ) )
            {
                if ( rs != null )
                {
                    XmlTextReader tr = new XmlTextReader( rs );
                    bool bExit = false;
                    while ( tr.Read() )
                    {
                        switch ( tr.NodeType )
                        {
                            case XmlNodeType.Element:
                                if ( String.IsNullOrEmpty( ret ) )
                                    ret = String.IsNullOrEmpty( tr.Name ) ? null : String.Copy( tr.Name );
                                bExit = true;
                                break;
                        }
                        if ( bExit )
                            break;
                    }
                    tr.Close();
                }
            }

            return ret;
        }

        private void LoadPluginClassSettings( string dirPath )
        {
            string filepath = String.Format( @"{0}\{1}", dirPath, UMacroMethodProviderPlugin.PluginClassParamDescFileName );
            if ( !File.Exists( filepath ) )
                return;

            string firstEleNode = QueryFirstEleNode( filepath );
            if ( String.IsNullOrEmpty( firstEleNode ) ) return;

            if ( m_listLoadedPluginInst == null ) return;
            for ( int i = 0 ; i < m_listLoadedPluginInst.Count ; i++ )
            {
                // check open status
                if ( !m_listLoadedPluginInst[ i ].IsOpened )
                    continue;
                // find the class
                if ( m_listLoadedPluginInst[ i ].NameOfCSharpDefClass == firstEleNode )
                {
                    // can be loaded?
                    if ( !m_listLoadedPluginInst[ i ].CanReadPluginClassSettings() )
                        break;

                    // begin to load
                    m_listLoadedPluginInst[ i ].ReadPluginClassSettingsBegin();
                    // loading
                    if ( !m_listLoadedPluginInst[ i ].ReadPluginClassSettings( dirPath ) )
                    {
                        if ( m_fpLog != null )
                            m_fpLog(eLogMessageType.NORMAL, 0, String.Format( "[LoadPluginClassSettings] cannot load {0} to class {1}", filepath, m_listLoadedPluginInst[ i ].NameOfCSharpDefClass ) );
                    }
                    // end to load
                    m_listLoadedPluginInst[ i ].ReadPluginClassSettingsEnd();
                    break;
                }
            }
        }

        public bool LoadPluginClassSettings( string zipFilePath, ref string msg )
        {
            msg = "";
            // Check source zip file
            if ( String.IsNullOrEmpty( zipFilePath ) || !File.Exists( zipFilePath ) )
            {
                msg = String.Format( "File {0} not exist.", zipFilePath ); ;
                return false;
            }
            // Check temperal dir
            if ( !Directory.Exists( m_strAssemblyRWPath ) )
            {
                try { Directory.CreateDirectory( m_strAssemblyRWPath ); }
                catch { }
            }
            if ( String.IsNullOrEmpty( m_strAssemblyRWPath ) || !Directory.Exists( m_strAssemblyRWPath ) )
            {
                msg = "Temperal dir not exist.";
                return false;
            }
            // Check if zip file
            if ( !ZipFile.IsZipFile( zipFilePath ) )
            {
                msg = String.Format( "File {0} not a zip file.", zipFilePath );
                return false;
            }
            // Delete folder then create
            string tmpDir = String.Format( @"{0}\{1}", m_strAssemblyRWPath, Path.GetFileNameWithoutExtension( zipFilePath ) );
            if ( Directory.Exists( tmpDir ) )
                Directory.Delete( tmpDir, true );

            Directory.CreateDirectory( tmpDir );
            // unzip file
            using ( ZipFile zip = new ZipFile( zipFilePath ) )
            {
                zip.ZipErrorAction = ZipErrorAction.Skip;
                zip.ExtractAll( tmpDir );
            }

            // load dirs
            string[] toppathes = Directory.GetDirectories( tmpDir, "*", SearchOption.TopDirectoryOnly );
            if ( toppathes != null && toppathes.Length > 0 )
            {
                for ( int i = 0 ; i < toppathes.Length ; i++ )
                {
                    if ( Directory.Exists( toppathes[ i ] ) )
                        LoadPluginClassSettings( toppathes[ i ] );
                }
            }
            else
            {
                // load files
                LoadPluginClassSettings( tmpDir );
            }

            // delete tmperal dir
            try { Directory.Delete( tmpDir, true ); }
            catch { }

            return true;
        }

        private void WritePluginClassSettings( UMacroMethodProviderPlugin instance, string wpathDir )
        {
            if ( instance == null || !Directory.Exists( wpathDir ) )
                return;

            // ask to write
            if ( !instance.CanWritePluginClassSettings() ) return;
            // beg write
            instance.WritePluginClassSettingsBegin();
            if ( !instance.WritePluginClassSettings( wpathDir ) )
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.NORMAL, 0, String.Format( "[WritePluginClassSettings] call class {0} write settings error.", instance.NameOfCSharpDefClass ) );
            // end write
            instance.WritePluginClassSettingsEnd();
        }

        public bool SavePluginClassSettings( string zipFilePath, ref string msg )
        {
            msg = "";
            if ( String.IsNullOrEmpty( zipFilePath ) )
            {
                msg = "zip file path null";
                return false;
            }

            // create a dir path to write
            string strTmpDir = String.Format( @"{0}\{1}", m_strAssemblyRWPath, CommonUtilities.GetCurrentTimeStr() );
            if ( !Directory.Exists( strTmpDir ) )
            {
                try { Directory.CreateDirectory( strTmpDir ); }
                catch { }
            }
            // write settings to dir
            for ( int i = 0 ; i < m_listLoadedPluginInst.Count ; i++ )
            {
                if ( !m_listLoadedPluginInst[ i ].IsOpened ) continue;
                if ( !m_listLoadedPluginInst[ i ].CanWritePluginClassSettings() ) continue;

                string cwpath = string.Format( @"{0}\{1}", strTmpDir, i );
                try { Directory.CreateDirectory( cwpath ); }
                catch { if ( m_fpLog != null ) m_fpLog(eLogMessageType.NORMAL, 0, String.Format( "[SavePluginClassSettings] cannot create path {0} for class {1}", cwpath, m_listLoadedPluginInst[ i ].NameOfCSharpDefClass ) ); continue; }

                WritePluginClassSettings( m_listLoadedPluginInst[ i ], cwpath );
            }
            // zip it
            using ( ZipFile zip = new ZipFile() )
            {
                zip.AddDirectory( strTmpDir );
                zip.Save( zipFilePath );
            }
            // delete tmperal dir
            try { Directory.Delete( strTmpDir, true ); }
            catch { }

            return true;
        }

        public bool SavePluginClassSettings( string zipFilePath, string classFullName, ref string msg )
        {
            msg = "";
            if ( m_bDisposing || m_bDisposed )
            {
                msg = "instance not available";
                return false;
            }
            if ( String.IsNullOrEmpty( zipFilePath ) )
            {
                msg = "zip file path null";
                return false;
            }

            // get class instance
            UMacroMethodProviderPlugin inst = null;
            for ( int i = 0 ; i < m_listLoadedPluginInst.Count ; i++ )
            {
                if ( m_listLoadedPluginInst[ i ] == null || !m_listLoadedPluginInst[i].IsOpened ) continue;
                if ( m_listLoadedPluginInst[ i ].NameOfCSharpDefClass == classFullName )
                {
                    inst = m_listLoadedPluginInst[ i ]; break;
                }
            }
            if ( inst == null )
            {
                msg = String.Format( "Cannot find {0}", classFullName );
                return false;
            }

            // create a dir path to write
            string strTmpDir = String.Format( @"{0}\{1}", m_strAssemblyRWPath, CommonUtilities.GetCurrentTimeStr() );
            if ( !Directory.Exists( strTmpDir ) )
            {
                try { Directory.CreateDirectory( strTmpDir ); }
                catch { }
            }

            WritePluginClassSettings( inst, strTmpDir );
            // zip it
            using ( ZipFile zip = new ZipFile() )
            {
                zip.AddDirectory( strTmpDir );
                zip.Save( zipFilePath );
            }
            // delete tmperal dir
            try { Directory.Delete( strTmpDir, true ); }
            catch { }

            return true;
        }

        #endregion


        public bool CallPluginClassFuncByGivenName(string givenName, string funcName, out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            ret = null;
            if ( string.IsNullOrEmpty( givenName ) )
                return false;
            return GetPluginInstanceFromGivenName( givenName )?.CallClassProvideFunc( funcName, out ret, ctx ) ?? false;
        }
        public bool CallPluginClassFuncByClassFullName(string classFullName, string funcName, out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            ret = null;
            if ( string.IsNullOrEmpty( classFullName ) )
                return false;
            return GetPluginInstanceByClassCSharpTypeName( classFullName )?.CallClassProvideFunc( funcName, out ret, ctx ) ?? false;
        }

        #endregion

        #region [ Create Macro of Script ]

        public UDataCarrier[] SetupImmutableParametersFromGivenName( string givenNameOfPluginClass, string methodName )
        {
            if ( String.IsNullOrEmpty( methodName ) ) return null;
            UMacroMethodProviderPlugin info = GetPluginInstanceFromGivenName( givenNameOfPluginClass );
            if ( info == null ) return null;

            return (info.SetupMacroImmutableOnes( methodName ));
        }

        public UDataCarrier[] SetupVariableParametersFromGivenName( string givenNameOfPluginClass, string methodName, ref bool bRetState )
        {
            if ( String.IsNullOrEmpty( methodName ) ) return null;
            UMacroMethodProviderPlugin info = GetPluginInstanceFromGivenName( givenNameOfPluginClass );
            if ( info == null ) return null;

            return (info.SetupMacroVariables( methodName, null, ref bRetState ));
        }

        public UMacro CreateMacroFromGivenName( string givenNameOfPluginClass, string methodName, UDataCarrier[] immutable, UDataCarrier[] variables )
        {
            if ( String.IsNullOrEmpty( methodName ) ) return null;

            UMacroMethodProviderPlugin info = GetPluginInstanceFromGivenName( givenNameOfPluginClass );
            if ( info == null ) return null;

            return (info.CreateMacroInstance( UDataCarrier.MakeOneItemArray<string>( methodName ), immutable, variables ));
        }

        public UDataCarrier[] SetupImmutableParametersFromSharpClassName( string className, string methodName )
        {
            if ( String.IsNullOrEmpty( methodName ) ) return null;
            UMacroMethodProviderPlugin info = GetPluginInstanceByClassCSharpTypeName( className );
            if ( info == null ) return null;

            return (info.SetupMacroImmutableOnes( methodName ));
        }

        public UDataCarrier[] SetupVariableParametersFromSharpClassName( string className, string methodName, ref bool bRetState )
        {
            if ( String.IsNullOrEmpty( methodName ) ) return null;
            UMacroMethodProviderPlugin info = GetPluginInstanceByClassCSharpTypeName( className );
            if ( info == null ) return null;

            return (info.SetupMacroVariables( methodName, null, ref bRetState ));
        }

        public UMacro CreateMacroFromSharpClassName( string className, string methodName, UDataCarrier[] immutable, UDataCarrier[] variables )
        {
            if ( String.IsNullOrEmpty( methodName ) ) return null;

            UMacroMethodProviderPlugin info = GetPluginInstanceByClassCSharpTypeName( className );
            if ( info == null ) return null;

            return (info.CreateMacroInstance( UDataCarrier.MakeOneItemArray<string>( methodName ), immutable, variables ));
        }

        #endregion

        #region Support IDatIO to write class settings

        public string UniqueName { get { return this.GetType().FullName; } }

        public string IOFileName { get { return "ClassSettings.ini"; } }
        public bool CanWrite { get { return !m_bDisposed && !m_bDisposing && m_bReady; } }
        public bool WriteDat( string folderPath, string fileName )
        {
            string zipFilePath = Path.GetFullPath( Path.Combine( folderPath, String.Format( "AllClasses_{0}.zip", CommonUtilities.GetCurrentTimeStr() ) ) );
            string dummy = null;
            if ( !SavePluginClassSettings( zipFilePath, ref dummy ) ) {
                return false;
            }

            string iniFileNm = Path.GetFullPath( Path.Combine( folderPath, fileName ) );
            try {
                using ( Stream ws = File.Open( iniFileNm, FileMode.Create ) ) {
                    StreamWriter sw = new StreamWriter( ws );
                    sw.WriteLine( "[ZipFile]" );
                    sw.WriteLine( "file=\"{0}\"", Path.GetFileName( zipFilePath ) );
                    sw.Flush();
                    sw.Close();
                }
            } catch { return false; }

            return true;
        }
        public bool CanRead { get { return !m_bDisposed && !m_bDisposing && m_bReady; } }
        public bool ReadDat( string folderPath, string fileName )
        {
            string iniFile = Path.GetFullPath( Path.Combine( folderPath, fileName ) );
            if ( !File.Exists( iniFile ) )
                return false;

            IniReaderUtility ini = new IniReaderUtility();
            if ( !ini.Parsing( iniFile ) )
                return false;

            SectionDataOfIni sec = ini.Get( "ZipFile" );
            if ( sec == null || sec.Data.Count <= 0 )
                return false;

            string zipfileNm = null;
            try {
                zipfileNm = sec.Data[ 0 ].Values[ 0 ].Replace( "\"", "" ).Trim();
            } catch {
                return false;
            }

            string zipFilePath = Path.GetFullPath( Path.Combine( folderPath, zipfileNm ) );
            string dummy = null;
            return LoadPluginClassSettings( zipFilePath, ref dummy );
        }

        public bool PopupGUI()
        {
            frmPopupConfig dlg = new frmPopupConfig();
            dlg.PS = this;
            dlg.ShowDialog();

            return true;
        }
        UserControlPluginClassConfig _controlPluginClassConfig = null;
        public Control GetGUI()
        {
            if ( _controlPluginClassConfig == null) {
                _controlPluginClassConfig = new UserControlPluginClassConfig();
                _controlPluginClassConfig.PS = this;
            }

            return _controlPluginClassConfig.NumberOfConf <= 0 ? null : _controlPluginClassConfig;
        }
        public void SetConfigControlSize(int w, int h)
        {
            if ( w <= 0 || h <= 0 )
                return;
            if ( _controlPluginClassConfig == null )
                return;
            _controlPluginClassConfig.Width = w;
            _controlPluginClassConfig.Height = h;
        }

        #endregion

        public UMacroMethodProviderPlugin GetPluginInstanceFromGivenName( string givenNameOfPluginClass )
        {
            if ( m_listLoadingAssembInfo == null || m_bDisposed || m_bDisposing || !m_bOpened )
                return null;
            if ( String.IsNullOrEmpty( givenNameOfPluginClass ) )
                return null;

            UMacroMethodProviderPlugin plugin = null;
            for ( int i = 0 ; i < m_listLoadingAssembInfo.Count ; i++ )
            {
                if ( m_listLoadingAssembInfo[ i ] == null ) continue;
                if ( m_listLoadingAssembInfo[ i ]._OpInstance == null ||
                     String.IsNullOrEmpty( m_listLoadingAssembInfo[ i ]._OpInstance.GivenName ) )
                    continue;

                if ( m_listLoadingAssembInfo[ i ]._OpInstance.GivenName == givenNameOfPluginClass )
                {
                    plugin = m_listLoadingAssembInfo[ i ]._bInitStatus ?
                        m_listLoadingAssembInfo[ i ]._OpInstance : null;
                    break;
                }
            }

            return plugin;
        }


        public UMacroMethodProviderPlugin GetPluginInstanceByClassCSharpTypeName( string classTypeFullNm )
        {
            if ( m_listLoadingAssembInfo == null || m_bDisposed || m_bDisposing || !m_bOpened )
                return null;
            if ( String.IsNullOrEmpty( classTypeFullNm ) )
                return null;

            if (m_refClassFullnameLoadedPlugins.TryGetValue(classTypeFullNm, out var plugin))
            {
                if ( plugin._OpInstance == null || !plugin._bInitStatus )
                    return null;
                return plugin._OpInstance;
            }

            UMacroMethodProviderPlugin info = null;
            for ( int i = 0 ; i < m_listLoadingAssembInfo.Count ; i++ )
            {
                if ( m_listLoadingAssembInfo[ i ] == null ) continue;
                if ( m_listLoadingAssembInfo[ i ]._OpInstance == null ||
                     String.IsNullOrEmpty( m_listLoadingAssembInfo[ i ]._OpInstance.NameOfCSharpDefClass ) )
                    continue;

                if ( m_listLoadingAssembInfo[ i ]._OpInstance.NameOfCSharpDefClass == classTypeFullNm )
                {
                    info = m_listLoadingAssembInfo[ i ]._bInitStatus ?
                        m_listLoadingAssembInfo[ i ]._OpInstance : null;
                    break;
                }
            }

            return info;
        }

    }
}
