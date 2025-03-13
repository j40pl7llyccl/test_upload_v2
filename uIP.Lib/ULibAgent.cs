using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.BlockAction;
using uIP.Lib.DataCarrier;
using uIP.Lib.Service;
using uIP.Lib.Script;
using uIP.Lib.UsrControl;
using uIP.Lib.Utility;

namespace uIP.Lib
{
    public class ULibAgent : IDisposable
    {
        private static ULibAgent _singleton = null;
        private const string RegName = "uIPLibs";
        public const string AssemblyIniFilename = "loading_assemblies.ini";
        public const string SystemIniFilename = "system.ini";
        public const string TemporalShmI32Name = "tmp_sh_i32";
        public const string TemporalShmI64Name = "tmp_sh_i64";
        public const string TemporalShmDfName = "tmp_sh_double";
        public const string ImmutableShmI32Name = "immu_sh_i32";
        public const string ImmutableShmI64Name = "immu_sh_i64";
        public const string ImmutableShmDfName = "immu_sh_double";
        public const string ImmutableShmI32Filename = "immu_i32.ini";
        public const string ImmutableShmI64Filename = "immu_i64.ini";
        public const string ImmutableShmDfFilename = "immu_double.ini";
        public const string IniFolderName = "inis";
        public const string TempFolderName = "tmp";
        public const string EnvTempFolderName = "env_tmp";
        internal const string EnvIOSettingsFilename = "env_settings.zip";

        public static ULibAgent Singleton
        {
            get
            {
                if (_singleton == null) _singleton = new ULibAgent();
                return _singleton;
            }
        }

        public static void Start(string workingDir, Form main, bool bEnableAction = false )
        {
            if ( _singleton != null )
                return;

            _singleton = new ULibAgent();
            _singleton.InitResources( workingDir, main, bEnableAction );
            ResourceManager.SystemUpCall();

            // reload io
            var envSettingsPath = Path.Combine( _singleton.IniRootPath, EnvIOSettingsFilename );
            if ( File.Exists( envSettingsPath ) )
            {
                if (_singleton.EnvSettings?.Read( envSettingsPath )??false)
                    _singleton.ScriptEditor?.ReloadScripts();
            }
        }

        public static void Close()
        {
            if ( _singleton != null )
            {
                var envSettingsPath = Path.Combine( _singleton.IniRootPath, EnvIOSettingsFilename );
                _singleton.EnvSettings?.Write( envSettingsPath );
                ResourceManager.SystemDownCall();
                _singleton.Dispose();
                _singleton = null;
            }
        }

        public static bool CheckAbleToEnd(bool withUi = false)
        {
            if ( _singleton == null )
                return true;

            var rs = _singleton.ScriptRunner.InvokeMainThreadRunningScripts;
            if ( rs == null || rs.Length == 0 )
                return true;

            if ( withUi )
            {
                if (MessageBox.Show("Still have UI script running, Cancel them?", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Question ) == DialogResult.OK )
                {
                    _singleton.ScriptRunner.Cancel( rs );
                }
            }
            return false;
        }

        internal bool m_bDisposing = false;
        internal bool m_bDisposed = false;
        internal bool m_bReady = false;
        public bool Available => !( m_bDisposing || m_bDisposed );

        // plugin assembly service
        protected UPluginAssemblyService m_PluginService = null;
        public UPluginAssemblyService AssemblyPlugins
        {
            get { return m_PluginService; }
        }

        // script service
        protected UScriptService m_ScriptService = null;
        public UScriptService Scripts
        {
            get { return m_ScriptService; }
        }

        // script runner
        protected UScriptRunnerFactory m_scriptRunners = null;

        public UScriptRunnerFactory ScriptRunner
        {
            get { return m_scriptRunners; }
        }

        // action service (state machine)
        protected ActionAgent m_ActionService = null;
        public ActionAgent Actions
        {
            get { return m_ActionService; }
        }

        protected string m_strFilePathNameOfIniPlugin = null;
        protected string m_strDirPathOfRootWorkingDir = null;

        // file log
        protected LogLevelList m_logLevelChecks = null;
        protected LogStringToFile m_fileLogs = null;

        public Action<string> LogNormal { get; private set; } = null;
        public Action<string> LogWarning { get; private set; } = null;
        public Action<string> LogError { get; private set; } = null;

        // user manager
        protected UserDataManager m_userManager = null;
        public UserDataManager UserManager
        {
            get { return m_userManager; }
        }

        // multi-language
        protected UserMultilanguage m_multilang = null;
        public UserMultilanguage Multilanguage
        {
            get { return m_multilang; }
        }

        // gui acl
        protected GUIAccessManager m_guiAcl = null;
        public GUIAccessManager GuiAcl
        {
            get { return m_guiAcl; }
        }

        // dirs
        private string m_tmpRootPath = null;
        private string m_iniRootPath = null;
        internal string IniRootPath => m_iniRootPath;

        // state machine: action/ block
        private UCWin32SharedMemFormating m_formatShm = null;
        private UCDataSyncW32< Int32 > m_tmpShmI32 = null;
        private UCDataSyncW32< Int64 > m_tmpShmI64 = null;
        private UCDataSyncW32< double > m_tmpShmDf = null;
        private UCDataSyncW32<Int32> m_immuShmI32 = null;
        private UCDataSyncW32<Int64> m_immuShmI64 = null;
        private UCDataSyncW32<double> m_immuShmDf = null;

        // script editor
        private FrmScriptEditor m_scriptEditor = null;
        public FrmScriptEditor ScriptEditor
        {
            get { return m_scriptEditor; }
        }

        // env auto load/ save
        private SettingsFileIO m_envSettings = null;
        internal SettingsFileIO EnvSettings => m_envSettings;

        private ULibAgent(UPluginAssemblyService plugins, UScriptService scripts, ActionAgent actions)
        {
            /* 呼叫載入 plugin assembly 需要有以下基礎資訊
             * - 工作目錄: 有放置各 plugin 分類子資料夾或 plugin assembly的路徑
             * - 暫時讀寫檔案的目錄: 提供一個可以讓載入的 plugin assembly 可以暫時寫入與讀出的工作目錄
             * - ini 的資料夾路徑
             * - ini 描述的檔案名稱
             */
            m_PluginService = plugins;
            m_ScriptService = scripts;
            m_ActionService = actions;
        }

        private ULibAgent()
        {

        }

        public void SetServices( UPluginAssemblyService plugins, UScriptService scripts, ActionAgent actions )
        {
            m_PluginService = plugins;
            m_ScriptService = scripts;
            m_ActionService = actions;
        }

        private void RegResources()
        {
            if (m_PluginService != null)
                ResourceManager.Reg( ResourceManager.PluginServiceName, m_PluginService );
            if ( m_ScriptService != null )
                ResourceManager.Reg( ResourceManager.ScriptService, m_ScriptService );
            if (m_scriptRunners != null)
                ResourceManager.Reg( ResourceManager.ScriptRunnerFactory, m_scriptRunners );
            if (m_ActionService != null)
                ResourceManager.Reg( ResourceManager.ActionService, m_ActionService );
            ResourceManager.Reg( ResourceManager.LogDelegate, new fpLogMessage( LogMessage2File ) );
            ResourceManager.Reg( ResourceManager.LibAgent, this );
            if ( m_scriptEditor != null )
                ResourceManager.Reg( ResourceManager.ScriptEditor, m_scriptEditor );
        }

        public void InitResources( string workingPath, Form mainForm, bool bEnableAction = false )
        {
            if ( m_bReady )
                return;

            ResourceManager.Reg( ResourceManager.MainWindow, mainForm );
            ResourceManager.Reg( ResourceManager.LibAgent, this );

            // root dir must exist
            if ( !Directory.Exists( workingPath ) ) return;
            m_bReady = true;


            // create logs
            // 1. runtime folder
            // 2. backup folder -> clean regular
            string logRuntimePath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, "logs", "runtime" ) );
            string logBackupPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, "logs", "backup" ) );
            if (m_logLevelChecks == null) m_logLevelChecks = new LogLevelList();
            if ( !String.IsNullOrEmpty( logRuntimePath ) && !String.IsNullOrEmpty( logBackupPath ) &&
                 m_fileLogs == null )
            {
                // per file char count: 10MB
                // runtime path file count: 1,000,000
                m_fileLogs = new LogStringToFile( 10*1024*1024, logRuntimePath, 1000000, logBackupPath );
                LogNormal = new Action<string>( m_fileLogs.MessageLog );
                LogWarning = new Action<string>( m_fileLogs.WarningLog );
                LogError = new Action<string>( m_fileLogs.ErrorLog );
            }

            // create for user manager
            string usrMgrPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, "users_data" ) );
            if ( m_userManager == null )
            {
                m_userManager = new UserDataManager( LogMessage2File, usrMgrPath );
            }

            // create multi-language
            string mlPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, "multilanguage" ) );
            if ( m_multilang == null )
            {
                m_multilang = new UserMultilanguage(LogMessage2File, mainForm, mlPath );
                if ( m_userManager != null ) m_userManager.MultiLang = m_multilang;
            }

            // create acl
            string aclPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, "gui_acl" ) );
            if ( m_guiAcl == null )
            {
                m_guiAcl = new GUIAccessManager(LogMessage2File, mainForm, aclPath, m_multilang);
                if ( m_userManager != null ) m_userManager.GuiAcl = m_guiAcl.GuiAcl;
            }

            // prepare dirs
            //string assemblyRootPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, "plugins" ) );
            string iniPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, IniFolderName ) );
            m_iniRootPath = iniPath;
            m_tmpRootPath = CommonUtilities.RCreateDir2( Path.Combine( workingPath, TempFolderName ) );
            string assemblyRwPath = CommonUtilities.RCreateDir2( Path.Combine( m_tmpRootPath, "assembly_tmp" ) );
            string sysIniFilePath = Path.Combine( iniPath, SystemIniFilename );
            IniReaderUtility sysIni = new IniReaderUtility();
            sysIni.Parsing( sysIniFilePath );

            // create file io
            m_envSettings = new SettingsFileIO( Path.Combine( m_tmpRootPath, EnvTempFolderName ), LogMessage2File );

            // plugin service
            if ( m_PluginService == null )
            {
                m_PluginService = new UPluginAssemblyService( workingPath, workingPath, iniPath, AssemblyIniFilename, assemblyRwPath, LogMessage2File );
                m_PluginService.Open();

                // install multilanguage delegate fcuntion for plugin
                m_multilang?.InstallLanguageSwitchCallback( 
                    new fpMultilanguageSwitch( 
                        ( im, lang ) => {
                            foreach ( var plug in m_PluginService.PluginAssemblies )
                                plug?.ChangeLanguage( lang );
                        }
                    )
                );

                // install io to manager
                m_envSettings.Add( m_PluginService );
            }

            // script service
            string scriptRwPath = CommonUtilities.RCreateDir2( Path.Combine( m_tmpRootPath, "script_tmp" ) );
            if ( m_ScriptService == null )
            {
                m_ScriptService = new UScriptService( m_PluginService, scriptRwPath );
                sysIni.Get( "ScriptService", out var kvs, "RelativePathDefaultSettings" );
                if (kvs != null && kvs.TryGetValue("RelativePathDefaultSettings", out var got))
                {
                    // use
                    if ( got != null && got.Length > 0 )
                    {
                        string confPath = Path.Combine( workingPath, got[ 0 ] );
                        string dummy = null;
                        if ( File.Exists( confPath ) ) m_ScriptService.ReadSettings( confPath, ref dummy );
                    }
                }

                var dicStartup = ResourceManager.Get( "startup" ) as Dictionary<string, string[]>;
                if (dicStartup != null && 
                    dicStartup.TryGetValue( "auto_save_script", out var datAutoSaveScript ) && 
                    datAutoSaveScript != null && datAutoSaveScript.Length > 0 )
                {
                    var strAutoSaveScript = datAutoSaveScript[0];

                    // install io to manager
                    if ( bool.TryParse( strAutoSaveScript, out var autoSaveScript ) &&
                        autoSaveScript )
                    {
                        m_envSettings.Add( m_ScriptService );
                    }
                }
            }

            // action service
            if ( bEnableAction && m_ActionService == null )
            {
                Dictionary< string, string[] > actionIniDat = null;
                sysIni.Get( "ActionService", out actionIniDat, "Enable", "FormatedSHM_Name", "FormatedSHM_ItemSz",
                    "FormatedSHM_ItemCnt", "TemporalSHM_I32Cnt", "TemporalSHM_I64Cnt", "TemporalSHM_DfCnt",
                    "ImmuSHM_I32Cnt", "ImmuSHM_I64Cnt", "ImmuSHM_DfCnt" );
                if ( actionIniDat != null && actionIniDat.Count > 0 )
                {
                    try
                    {
                        if ( Convert.ToBoolean( actionIniDat[ "Enable" ][ 0 ] ) )
                        {
                            string formatShmName = actionIniDat[ "FormatedSHM_Name" ][ 0 ];
                            int formatShmItemSz = Convert.ToInt32( actionIniDat[ "FormatedSHM_ItemSz" ][ 0 ] );
                            int formatShmItemCnt = Convert.ToInt32( actionIniDat[ "FormatedSHM_ItemCnt" ][ 0 ] );
                            int tmpShmI32Cnt = Convert.ToInt32( actionIniDat[ "TemporalSHM_I32Cnt" ][ 0 ] );
                            int tmpShmI64Cnt = Convert.ToInt32( actionIniDat[ "TemporalSHM_I64Cnt" ][ 0 ] );
                            int tmpShmDfCnt = Convert.ToInt32( actionIniDat[ "TemporalSHM_DfCnt" ][ 0 ] );
                            int immShmI32Cnt = Convert.ToInt32( actionIniDat[ "ImmuSHM_I32Cnt" ][ 0 ] );
                            int immShmI64Cnt = Convert.ToInt32( actionIniDat[ "ImmuSHM_I64Cnt" ][ 0 ] );
                            int immShmDfCnt = Convert.ToInt32( actionIniDat[ "ImmuSHM_DfCnt" ][ 0 ] );
                            if ( !String.IsNullOrEmpty( formatShmName ) && formatShmItemSz > 0 && formatShmItemCnt > 0 )
                            {
                                m_formatShm = new UCWin32SharedMemFormating();
                                m_formatShm.Initialize( formatShmName, formatShmItemSz, formatShmItemCnt );
                            }

                            if ( tmpShmI32Cnt > 0 )
                            {
                                m_tmpShmI32 = new UCDataSyncW32<Int32>();
                                m_tmpShmI32.Initialize( TemporalShmI32Name,
                                    String.Format( "{0}_mux", TemporalShmI32Name ), tmpShmI32Cnt );
                            }

                            if ( tmpShmI64Cnt > 0 )
                            {
                                m_tmpShmI64 = new UCDataSyncW32< Int64 >();
                                m_tmpShmI64.Initialize( TemporalShmI64Name,
                                    String.Format( "{0}_mux", TemporalShmI64Name ), tmpShmI64Cnt );
                            }

                            if ( tmpShmDfCnt > 0 )
                            {
                                m_tmpShmDf = new UCDataSyncW32< double >();
                                m_tmpShmDf.Initialize( TemporalShmDfName, String.Format( "{0}_mix", TemporalShmDfName ),
                                    tmpShmDfCnt );
                            }

                            string immuI32FilePath = Path.Combine( iniPath, ImmutableShmI32Filename );
                            if ( File.Exists( immuI32FilePath ) )
                            {
                                UCDataSyncW32<Int32> tmp = new UCDataSyncW32<Int32>();
                                if ( UCDataSyncW32Utils.ReadIniI32( tmp, immuI32FilePath ) ) m_immuShmI32 = tmp;
                            }

                            if ( m_immuShmI32 == null && immShmI32Cnt > 0 )
                            {
                                m_immuShmI32 = new UCDataSyncW32< Int32 >();
                                m_immuShmI32.Initialize( ImmutableShmI32Name,
                                    String.Format( "{0}_mux", ImmutableShmI32Name ), immShmI32Cnt );
                            }

                            string immuI64FilePath = Path.Combine( iniPath, ImmutableShmI64Filename );
                            if ( File.Exists( immuI64FilePath ) )
                            {
                                UCDataSyncW32< Int64 > tmp = new UCDataSyncW32< Int64 >();
                                if ( UCDataSyncW32Utils.ReadIniI64( tmp, immuI64FilePath ) ) m_immuShmI64 = tmp;
                            }

                            if ( m_immuShmI64 == null && immShmI64Cnt > 0 )
                            {
                                m_immuShmI64 = new UCDataSyncW32< Int64 >();
                                m_immuShmI64.Initialize( ImmutableShmI64Name,
                                    String.Format( "{0}_mux", ImmutableShmI64Name ), immShmI64Cnt );
                            }

                            string immuDfFilePath = Path.Combine( iniPath, ImmutableShmDfFilename );
                            if ( File.Exists( immuDfFilePath ) )
                            {
                                UCDataSyncW32< double > tmp = new UCDataSyncW32< double >();
                                if ( UCDataSyncW32Utils.ReadIniDouble( tmp, immuDfFilePath ) ) m_immuShmDf = tmp;
                            }

                            if ( m_immuShmDf == null && immShmDfCnt > 0 )
                            {
                                m_immuShmDf = new UCDataSyncW32< double >();
                                m_immuShmDf.Initialize( ImmutableShmDfName,
                                    String.Format( "{0}_mux", ImmutableShmDfName ), immShmDfCnt );
                            }
                            string actionTmDir = CommonUtilities.RCreateDir2( Path.Combine( m_tmpRootPath, "action_tmp" ) );
                            m_ActionService = new ActionAgent( "action_agent", actionTmDir, LogMessage2File, null, m_formatShm,
                                m_tmpShmI32, m_tmpShmI64, m_tmpShmDf,
                                m_immuShmI32, m_immuShmI64, m_immuShmDf, m_guiAcl);
                            m_ActionService.ReadAssemblies( workingPath, Path.Combine( iniPath, "loading_blocks.ini" ) );

                            // install io to manager
                            m_envSettings.Add( m_ActionService.ImmutableAM );
                            m_envSettings.Add( m_ActionService.VariableAM );
                        }
                    } catch { }
                }
            }

            if ( m_scriptRunners == null )
            {
                m_scriptRunners = UScriptRunnerFactory.Singleton;
            }

            RegResources();

            if ( m_scriptEditor == null )
            {
                m_scriptEditor = new FrmScriptEditor();
                ResourceManager.Reg( ResourceManager.ScriptEditor, m_scriptEditor );
            }

            ResourceManager.Reg( ResourceManager.SystemUp, true );

            // broadcast that system is up
            m_PluginService.SetPluginClassesControl( ResourceManager.SystemUp, null );
        }

        private void LogMessage2File( eLogMessageType type, Int32 id, String message )
        {
            if ( m_bDisposing || m_bDisposed ) return;

            bool accept = m_logLevelChecks == null || m_logLevelChecks.Accept( id );
            if ( !accept ) return;

            switch ( type )
            {
                case eLogMessageType.NORMAL: m_fileLogs?.MessageLog( message ); break;
                case eLogMessageType.WARNING: m_fileLogs?.WarningLog( message ); break;
                case eLogMessageType.ERROR: m_fileLogs?.ErrorLog( message ); break;
            }
        }

        public void Dispose()
        {
            if ( m_bDisposing )
                return;

            m_bReady = false;
            m_bDisposing = true;
            ResourceManager.Reg( ResourceManager.SystemUp, false );

            //
            // any running thread must stop first
            //
            m_ActionService?.Dispose();
            m_ActionService = null;

            m_scriptRunners?.Dispose();
            m_scriptRunners = null;

            // begin free resources
            ResourceManager.Unreg( RegName );

            m_scriptEditor?.Dispose();
            m_scriptEditor = null;

            m_ScriptService?.Dispose();
            m_ScriptService = null;

            m_PluginService?.Dispose();
            m_PluginService = null;


            m_tmpShmI32?.Dispose();
            m_tmpShmI64?.Dispose();
            m_tmpShmDf?.Dispose();
            if ( m_immuShmI32 != null )
            {
                if (!String.IsNullOrEmpty( m_iniRootPath ))
                    UCDataSyncW32Utils.WriteIniI32( m_immuShmI32, Path.Combine( m_iniRootPath, ImmutableShmI32Filename ) );
                m_immuShmI32.Dispose();
            }

            if ( m_immuShmI64 != null )
            {
                if ( !String.IsNullOrEmpty( m_iniRootPath ) )
                    UCDataSyncW32Utils.WriteIniI64( m_immuShmI64,
                        Path.Combine( m_iniRootPath, ImmutableShmI64Filename ) );
                m_immuShmI64.Dispose();
            }

            if ( m_immuShmDf != null )
            {
                if ( !String.IsNullOrEmpty( m_iniRootPath ) )
                    UCDataSyncW32Utils.WriteIniDouble( m_immuShmDf,
                        Path.Combine( m_iniRootPath, ImmutableShmDfFilename ) );
                m_immuShmDf.Dispose();
            }

            m_formatShm?.Dispose();

            m_guiAcl?.Dispose();
            m_userManager?.Dispose();
            m_multilang?.Dispose();

            if ( Directory.Exists( m_tmpRootPath ) )
            {
                try { Directory.Delete( m_tmpRootPath, true ); } catch { }
            }

            m_bDisposed = true;
            m_bDisposing = false;
        }

        #region [Control: GET/SET parameters]

        #region -- Plugin class --
        public UDataCarrier[] GetPluginClassControlByGivenName( string classGivenName, string cmdName, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( m_bDisposing || m_bDisposed ) return null;
            if ( m_PluginService == null ) return null;

            return m_PluginService.GetPluginClassControl( classGivenName, cmdName, out bRetStatus );
        }
        public UDataCarrier[] GetPlugingClassControlByCSharpClassName( string classCSharpTypeFullName, string cmd, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( m_bDisposing || m_bDisposed ) return null;
            if ( m_PluginService == null ) return null;

            UMacroMethodProviderPlugin assembly = m_PluginService.GetPluginInstanceByClassCSharpTypeName( classCSharpTypeFullName );
            if ( assembly == null ) return null;

            return assembly.GetClassControl( UDataCarrier.MakeOne<string>( cmd ), ref bRetStatus );
        }
        public bool SetPluginClassControlByGivenName( string classGivenName, string cmdName, UDataCarrier[] data )
        {
            bool ret = false;
            if ( m_bDisposing || m_bDisposed ) return ret;
            if ( m_PluginService == null ) return ret;

            return m_PluginService.SetPluginClassControl( classGivenName, cmdName, data );
        }
        public bool SetPluginClassControlByCSharpClassName( string classCSharpTypeFullName, string cmdName, UDataCarrier[] data )
        {
            bool ret = false;
            if ( m_bDisposing || m_bDisposed ) return ret;
            if ( m_PluginService == null ) return ret;

            UMacroMethodProviderPlugin assembly = m_PluginService.GetPluginInstanceByClassCSharpTypeName( classCSharpTypeFullName );
            if ( assembly == null ) return ret;

            return assembly.SetClassControl( UDataCarrier.MakeOne<string>( cmdName ), data );
        }
        public bool SetPluginClassesControl( string cmd, UDataCarrier[] data )
        {
            bool ret = false;
            if ( m_bDisposing || m_bDisposed ) return ret;
            if ( m_PluginService == null ) return ret;

            return m_PluginService.SetPluginClassesControl( cmd, data );
        }
        public UPluginAssemblyCmdList[] GetPluginClassesCmds()
        {
            if ( m_bDisposing || m_bDisposed ) return null;
            if ( m_PluginService == null ) return null;
            UPluginAssemblyCmdList[] ret;
            m_PluginService.GetPluginClassesCmdTypeDescs( out ret );
            return ret;
        }
        #endregion

        #region -- Macro/ ScriptService --
        public UDataCarrier[] GetMacroControl( string nameOfScript, int indexOfMacro, string nameOfCmd, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( m_bDisposing || m_bDisposed ) return null;
            if ( m_ScriptService == null ) return null;
            return m_ScriptService.GetMacroControl( nameOfScript, indexOfMacro, nameOfCmd, out bRetStatus );
        }
        public bool SetMacroControl( string nameOfScript, int indexOfMacro, string nameOfCmd, UDataCarrier[] data )
        {
            if ( m_bDisposing || m_bDisposed ) return false;
            if ( m_ScriptService == null ) return false;
            return m_ScriptService.SetMacroControl( nameOfScript, indexOfMacro, nameOfCmd, data );
        }
        public UDataCarrier[] GetReusableMacroControl( string givenNameOfMacro, string nameOfCmd, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( m_bDisposing || m_bDisposed ) return null;
            if ( m_ScriptService == null ) return null;
            return m_ScriptService.GetReusableMacroControl( givenNameOfMacro, nameOfCmd, out bRetStatus );
        }
        public bool SetReusableMacroControl( string givenNameOfMacro, string nameOfCmd, UDataCarrier[] data )
        {
            if ( m_bDisposing || m_bDisposed ) return false;
            if ( m_ScriptService == null ) return false;
            return m_ScriptService.SetReusableMacroControl( givenNameOfMacro, nameOfCmd, data );
        }
        public UPluginAssemblyCmdList[] GetMacrosCmds()
        {
            if ( m_bDisposing || m_bDisposed )
                return null;
            UPluginAssemblyCmdList[] ret;
            m_ScriptService.GetMacroCmdTypeDescs( out ret );
            return ret;
        }
        #endregion

        #endregion

        #region [ Script/ Macro management ]

        // New script
        public UScript NewScript( string nameOfScript )
        {
            if ( m_bDisposing || m_bDisposed || m_ScriptService == null )
                return null;

            // new or check
            if ( !m_ScriptService.NewScript( nameOfScript ) )
                return null;
            // get the instance
            return m_ScriptService.GetScript( nameOfScript );
        }
        // New Macro
        // - [1.] setup immutable parameter, plugin name default with C# class type name
        public UDataCarrier[] NewMacroImmutableCarrier( string nameOfPlugin, string nameOfMacroMethod, out string message, bool bPluginNameIsGivenName = false )
        {
            message = null;
            if ( m_bDisposing || m_bDisposed || m_PluginService == null )
            {
                message = "Agent unavailable!";
                return null;
            }

            UMacroMethodProviderPlugin plugin = bPluginNameIsGivenName ? m_PluginService.GetPluginInstanceFromGivenName( nameOfPlugin ) : m_PluginService.GetPluginInstanceByClassCSharpTypeName( nameOfPlugin );
            if ( plugin == null )
            {
                message = String.Format( "Not found plugin class named by \"{0}\"!", nameOfPlugin );
                return null;
            }

            if ( !plugin.ContainMethod( nameOfMacroMethod ) )
            {
                message = String.Format( "Method \"{0}\" not found!", nameOfMacroMethod );
                return null;
            }

            return plugin.SetupMacroImmutableOnes( nameOfMacroMethod );
        }
        // - [2.] setup variable parameter, plugin name default with C# class type name
        public UDataCarrier[] NewMacroVariableCarrier( string nameOfScript, string nameOfPlugin, string nameOfMethod, out string message, bool bPluginNameIsGivenName = false )
        {
            message = null;
            if ( m_bDisposing || m_bDisposed || m_PluginService == null || m_ScriptService == null )
            {
                message = "Agent unavailable!";
                return null;
            }
            UScript script = m_ScriptService.GetScript( nameOfScript );
            if ( script == null )
            {
                message = String.Format( "Not found script\"{0}\"!", nameOfScript );
                return null;
            }
            UMacroMethodProviderPlugin plugin = bPluginNameIsGivenName ? m_PluginService.GetPluginInstanceFromGivenName( nameOfPlugin ) : m_PluginService.GetPluginInstanceByClassCSharpTypeName( nameOfPlugin );
            if ( plugin == null )
            {
                message = String.Format( "Not found plugin class named by \"{0}\"!", nameOfPlugin );
                return null;
            }
            if ( !plugin.ContainMethod( nameOfMethod ) )
            {
                message = String.Format( "Method \"{0}\" not found!", nameOfMethod );
                return null;
            }
            bool bStatus = false;
            UDataCarrier[] ret = plugin.SetupMacroVariables( nameOfMethod, script.MacroSet, ref bStatus );
            if ( !bStatus )
            {
                message = String.Format( "Setup variable carrier in Method \"{0}\" of script \"{1}\" error!", nameOfMethod, nameOfScript );
                return null;
            }
            return ret;
        }
        public UDataCarrier[] NewMacroVariableCarrier( UScript script, string nameOfPlugin, string nameOfMethod, out string message, bool bPluginNameIsGivenName = false )
        {
            message = null;
            if ( m_bDisposing || m_bDisposed || m_PluginService == null || m_ScriptService == null )
            {
                message = "Agent unavailable!";
                return null;
            }
            if ( script == null )
            {
                message = "Invalid script!";
                return null;
            }
            UMacroMethodProviderPlugin plugin = bPluginNameIsGivenName ? m_PluginService.GetPluginInstanceFromGivenName( nameOfPlugin ) : m_PluginService.GetPluginInstanceByClassCSharpTypeName( nameOfPlugin );
            if ( plugin == null )
            {
                message = String.Format( "Not found plugin class named by \"{0}\"!", nameOfPlugin );
                return null;
            }
            if ( !plugin.ContainMethod( nameOfMethod ) )
            {
                message = String.Format( "Method \"{0}\" not found!", nameOfMethod );
                return null;
            }
            bool bStatus = false;
            UDataCarrier[] ret = plugin.SetupMacroVariables( nameOfMethod, script.MacroSet, ref bStatus );
            if ( !bStatus )
            {
                message = String.Format( "Setup variable carrier in Method \"{0}\" of script \"{1}\" error!", nameOfMethod, script.NameOfId );
                return null;
            }
            return ret;
        }
        // - [3.] create macro instance
        public UMacro NewMacroInstance( string nameOfScript, string nameOfPlugin, string nameOfMethod, UDataCarrier[] immutable, UDataCarrier[] variable, out string message, bool bPluginNameIsGivenName = false )
        {
            return NewMacroInstance( nameOfScript, nameOfPlugin, UDataCarrier.MakeOneItemArray<string>( nameOfMethod ), immutable, variable, out message, bPluginNameIsGivenName );
        }
        public UMacro NewMacroInstance( UScript script, string nameOfPlugin, string nameOfMethod, UDataCarrier[] immutable, UDataCarrier[] variable, out string message, bool bPluginNameIsGivenName = false )
        {
            return NewMacroInstance( script, nameOfPlugin, UDataCarrier.MakeOneItemArray<string>( nameOfMethod ), immutable, variable, out message, bPluginNameIsGivenName );
        }
        public UMacro NewMacroInstance( string nameOfScript, string nameOfPlugin, UDataCarrier[] parameters, UDataCarrier[] immutable, UDataCarrier[] variable, out string message, bool bPluginNameIsGivenName = false )
        {
            message = null;
            if ( m_bDisposing || m_bDisposed || m_PluginService == null || m_ScriptService == null )
            {
                message = "Agent unavailable!";
                return null;
            }
            UScript script = m_ScriptService.GetScript( nameOfScript );
            if ( script == null )
            {
                message = String.Format( "Not found script\"{0}\"!", nameOfScript );
                return null;
            }
            UMacroMethodProviderPlugin plugin = bPluginNameIsGivenName ? m_PluginService.GetPluginInstanceFromGivenName( nameOfPlugin ) : m_PluginService.GetPluginInstanceByClassCSharpTypeName( nameOfPlugin );
            if ( plugin == null )
            {
                message = String.Format( "Not found plugin class named by \"{0}\"!", nameOfPlugin );
                return null;
            }
            string nameOfMethod = parameters == null || parameters.Length <= 0 || parameters[ 0 ] == null || parameters[ 0 ].Data == null ?
                null : parameters[ 0 ].Data as string;
            if ( !plugin.ContainMethod( nameOfMethod ) )
            {
                message = String.Format( "Method \"{0}\" not found!", nameOfMethod );
                return null;
            }

            UMacro ret = plugin.CreateMacroInstance( parameters, immutable, variable );
            if ( ret == null || !script.Add( ret ) )
            {
                if ( ret != null ) ret.Recycle();
                message = "New macro fail!";
                return null;
            }

            return ret;
        }
        public UMacro NewMacroInstance( UScript script, string nameOfPlugin, UDataCarrier[] parameters, UDataCarrier[] immutable, UDataCarrier[] variable, out string message, bool bPluginNameIsGivenName = false )
        {
            message = null;
            if ( m_bDisposing || m_bDisposed || m_PluginService == null || m_ScriptService == null )
            {
                message = "Agent unavailable!";
                return null;
            }
            if ( script == null )
            {
                message = "Invalid script!";
                return null;
            }
            UMacroMethodProviderPlugin plugin = bPluginNameIsGivenName ? m_PluginService.GetPluginInstanceFromGivenName( nameOfPlugin ) : m_PluginService.GetPluginInstanceByClassCSharpTypeName( nameOfPlugin );
            if ( plugin == null )
            {
                message = String.Format( "Not found plugin class named by \"{0}\"!", nameOfPlugin );
                return null;
            }

            string nameOfMethod = parameters == null || parameters.Length <= 0 || parameters[ 0 ] == null || parameters[ 0 ].Data == null ?
                null : parameters[ 0 ].Data as string;
            if ( !plugin.ContainMethod( nameOfMethod ) )
            {
                message = String.Format( "Method \"{0}\" not found!", nameOfMethod );
                return null;
            }

            UMacro ret = plugin.CreateMacroInstance( parameters, immutable, variable );
            if ( ret == null || !script.Add( ret ) )
            {
                if ( ret != null ) ret.Recycle();
                message = "New macro fail!";
                return null;
            }

            return ret;
        }
        // --- New end ---

        public Int32 GetMacroIndexOfScript( UMacro macro, UScript script )
        {
            if ( m_bDisposing || m_bDisposed || macro == null || script == null || script.MacroSet == null )
                return -1;
            for ( int i = 0 ; i < script.MacroSet.Count ; i++ )
            {
                if ( script.MacroSet[ i ] == null ) continue;
                if ( script.MacroSet[ i ] == macro )
                    return i;
            }
            return -1;
        }
        public Int32 GetMacroIndexOfScript( UMacro macro, string nameOfScript )
        {
            if ( m_bDisposing || m_bDisposed || macro == null )
                return -1;
            UScript script = m_ScriptService.GetScript( nameOfScript );
            return GetMacroIndexOfScript( macro, script );
        }
        public void ModifyMacroVariableCarrier( UMacro macro )
        {
            if ( m_bDisposing || m_bDisposed || macro == null )
                return;
            bool bStatus = false;
            UDataCarrier[] var = macro.OwnerOfPluginClass.SetupMacroVariables( macro.MethodName, macro.OwnerMacrosList, ref bStatus );
            if ( bStatus ) macro.ParameterCarrierVariable = var;
        }
        public void ModifyMacroVariableCarrier( UScript scrip, int macroIndexOfScript )
        {
            if ( m_bDisposing || m_bDisposed || scrip == null || scrip.MacroSet == null )
                return;
            if ( macroIndexOfScript < 0 || macroIndexOfScript >= scrip.MacroSet.Count )
                return;
            ModifyMacroVariableCarrier( scrip.MacroSet[ macroIndexOfScript ] );
        }
        public void ModifyMacroVariableCarrier( string nameOfScript, int macroIndexOfScript )
        {
            if ( m_bDisposing || m_bDisposed || m_ScriptService == null )
                return;
            ModifyMacroVariableCarrier( m_ScriptService.GetScript( nameOfScript ), macroIndexOfScript );
        }
        // Manage Script/ Macro
        public void DeleteScript( string nameOfScript )
        {
            if ( m_bDisposing || m_bDisposed || m_ScriptService == null )
                return;

            m_ScriptService.DeleteScript( nameOfScript );
        }
        public void DeleteScript( ref UScript script )
        {
            if ( m_bDisposing || m_bDisposed || m_ScriptService == null )
                return;
            if ( script == null )
                return;
            UScript tmp = m_ScriptService.GetScript( script.NameOfId );
            if ( tmp == null )
                return;
            m_ScriptService.DeleteScript( script.NameOfId );
            script = null;
        }
        public bool DeleteMacro( UScript script, int index)
        {
            if ( m_bDisposing || m_bDisposed || script == null )
                return false;

            return script.RemoveFrom( index );
        }

        public static UDataCarrier CallPluginClassOpenedFuncRetValue( string strClassFullname, string strFuncName, out bool bCallStatus, params UDataCarrier[] args )
        {
            bCallStatus = false;
            if ( _singleton == null || _singleton.m_bDisposing || _singleton.m_bDisposed || _singleton.AssemblyPlugins == null )
                return null;

            var plugin = _singleton.AssemblyPlugins.GetPluginInstanceByClassCSharpTypeName( strClassFullname );
            if ( plugin == null )
                return null;

            bCallStatus = plugin.CallClassProvideFunc( strFuncName, out var ret, args );
            return ret;
        }

        public static bool CallPluginClassOpenedFuncRetStatus( string strClassFullname, string strFuncName, out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            if ( _singleton == null || _singleton.m_bDisposing || _singleton.m_bDisposed || _singleton.AssemblyPlugins == null )
            return false;

            var plugin = _singleton.AssemblyPlugins.GetPluginInstanceByClassCSharpTypeName( strClassFullname );
            if ( plugin == null )
                return false;

            return plugin.CallClassProvideFunc( strFuncName, out ret, args );
        }
        #endregion
    }
}
