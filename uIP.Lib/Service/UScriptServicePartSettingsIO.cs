using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

using uIP.Lib.Script;
using uIP.Lib.Utility;
using Ionic.Zip;
using uIP.Lib;

namespace uIP.Lib.Service
{
    internal struct TMacroIOTmpStorage
    {
        internal string _strNameOfPluginSharpClassName;
        internal string _strNameOfMacroMethodInPluginClass;
        internal string _strSetupFile;
        internal string _strNameOfMacro;
        internal bool _bReusable;
        internal XmlNode _CurrNod;
    }

    public partial class UScriptService : IDatIO
    {
        private static string _strFilenameOfScript = "a_script_settings.xml";
        private static string _strFilenameOfAllScripts = "all_scripts_settings.xml";
        private static string _strFilenameOfReusables = "reusable_macros_settings.xml";
        private static string _strNameOfAllScriptsXmlRootElement = "AllScriptsSettings";
        private static string _strNameOfAllScriptsXmlScriptElement = "Script";
        private static string _strNameOfAllScriptsXmlScriptElementAttName = "name";
        private static string _strNameOfScriptRootElement = "ScriptSettings";
        private static string _strNameOfScriptGivenNameElement = "ScriptGivenName";
        private static string _strNameOfScriptSnElement = "ScriptSn";
        private static string _strNameOfScriptAnnotation = "ScriptAnnotation";
        private static string _strNameOfScriptNumOfMacrosElement = "NumOfMacros";
        private static string _strNameOfMacroSettingsElement = "MacroSettings";
        private static string _strNameOfMacroMethodInPluginClassSharpNameElement = "MacroMethodBelongWhichPluginClassName";
        private static string _strNameOfMacroMethodNameElement = "MacroMethodName";
        private static string _strNameOfMacroGivenNameElement = "MacroGivenName";
        private static string _strNameOfMacroReusableFlagElement = "MacroReusable";
        private static string _strFilenameOfMacroSettingsElement = "MacroSettingsFilename";
        private static string _strNameOfReusableMacroSettingsElement = "ReusableMacroSettings";
        private static string _strNameOfReusableMacroMethodInfoElement = "ReusableMacroMethodInfo";
        private static string _strDirnameOfReusableSettingsPath = "ReuableMacrosSettings";

        internal static string FilenameOfScriptSettings { get { return _strFilenameOfScript; } }
        internal static string FilenameOfAllScriptsSettings { get { return _strFilenameOfAllScripts; } }
        internal static string FilenameOfReusableSettings { get { return _strFilenameOfReusables; } }
        internal static string NameOfScriptRootElement { get { return _strNameOfScriptRootElement; } }
        internal static string NameOfScriptGivenNameElement { get { return _strNameOfScriptGivenNameElement; } }
        internal static string NameOfScriptSnElement { get { return _strNameOfScriptSnElement; } }
        internal static string NameOfScriptNumOfMacrosElement { get { return _strNameOfScriptNumOfMacrosElement; } }
        internal static string NameOfMacroSettingsElement { get { return _strNameOfMacroSettingsElement; } }
        internal static string NameOfMacroMethodInPluginClassSharpNameElement { get { return _strNameOfMacroMethodInPluginClassSharpNameElement; } }
        internal static string NameOfMacroMethodNameElement { get { return _strNameOfMacroMethodNameElement; } }
        internal static string NameOfMacroGivenNameElement { get { return _strNameOfMacroGivenNameElement; } }
        internal static string NameOfMacroReusableFlagElement { get { return _strNameOfMacroReusableFlagElement; } }
        internal static string FilenameOfMacroSettingsElement { get { return _strFilenameOfMacroSettingsElement; } }
        internal static string NameOfReusableMacroSettingsElement { get { return _strNameOfReusableMacroSettingsElement; } }
        internal static string NameOfReusableMacroMethodInfoElement { get { return _strNameOfReusableMacroMethodInfoElement; } }
        internal static string DirnameOfReusableSettingsPath { get { return _strDirnameOfReusableSettingsPath; } }

        #region Load Settings

        private bool LoadScriptSettings( string settingsDirPath, string settingsFileFullPath, ref string repoMessage )
        {
            if ( m_refAssemblyLoader == null )
            {
                repoMessage = "No PluginService instance to load script settings.";
                return false;
            }

            string givenNameOfScript = null;
            string annotationOfScript = "";
            XmlDocument doc = new XmlDocument();
            doc.Load( settingsFileFullPath );

            // GetDicKeyStrOne name
            XmlNode node = doc.SelectSingleNode( String.Format( "//{0}/{1}", _strNameOfScriptRootElement, _strNameOfScriptGivenNameElement ) );
            if ( node != null && !String.IsNullOrEmpty( node.InnerText ) )
                givenNameOfScript = String.Copy( node.InnerText );

            // GetDicKeyStrOne annotation
            node = doc.SelectSingleNode( $"//{_strNameOfScriptRootElement}/{_strNameOfScriptAnnotation}" );
            if (node != null && !string.IsNullOrEmpty( node.InnerText ) )
                annotationOfScript = String.Copy( node.InnerText );

            // GetDicKeyStrOne Count
            node = doc.SelectSingleNode( String.Format( "//{0}/{1}", _strNameOfScriptRootElement, _strNameOfScriptNumOfMacrosElement ) );
            if ( node == null || String.IsNullOrEmpty( node.InnerText ) )
            {
                repoMessage = "GetDicKeyStrOne number of macros error.";
                return false;
            }

            int cnt = 0;
            try
            {
                cnt = Convert.ToInt32( node.InnerText );
            }
            catch
            {
                cnt = 0;
            }
            if ( cnt <= 0 )
            {
                repoMessage = "Number of macros error.";
                return false;
            }

            // GetDicKeyStrOne macros
            XmlNodeList tmpNodeList = doc.SelectNodes( String.Format( "//{0}/{1}", _strNameOfScriptRootElement, _strNameOfMacroSettingsElement ) );
            if ( tmpNodeList == null || tmpNodeList.Count != cnt )
            {
                repoMessage = "macro count mismatch.";
                return false;
            }

            // Start reloading settings
            List<TMacroIOTmpStorage> macroSet = new List<TMacroIOTmpStorage>();
            foreach ( XmlElement ele in tmpNodeList )
            {
                if ( ele.HasChildNodes )
                {
                    XmlNodeList children = ele.ChildNodes;
                    TMacroIOTmpStorage macroSettings = new TMacroIOTmpStorage();
                    foreach ( XmlElement subele in children )
                    {
                        if ( subele.LocalName == _strNameOfMacroMethodInPluginClassSharpNameElement )
                            macroSettings._strNameOfPluginSharpClassName = String.IsNullOrEmpty( subele.InnerText ) ? null : String.Copy( subele.InnerText );
                        else if ( subele.LocalName == _strNameOfMacroMethodNameElement )
                            macroSettings._strNameOfMacroMethodInPluginClass = String.IsNullOrEmpty( subele.InnerText ) ? null : String.Copy( subele.InnerText );
                        else if ( subele.LocalName == _strFilenameOfMacroSettingsElement )
                            macroSettings._strSetupFile = String.IsNullOrEmpty( subele.InnerText ) ? null : String.Copy( subele.InnerText );
                        else if ( subele.LocalName == _strNameOfMacroGivenNameElement )
                            macroSettings._strNameOfMacro = String.IsNullOrEmpty( subele.InnerText ) ? null : String.Copy( subele.InnerText );
                        else if ( subele.LocalName == _strNameOfMacroReusableFlagElement )
                        {
                            try { macroSettings._bReusable = String.IsNullOrEmpty( subele.InnerText ) ? false : Convert.ToBoolean( subele.InnerText ); }
                            catch { macroSettings._bReusable = false; }
                        }
                    }

                    // record current node result
                    macroSettings._CurrNod = ele;

                    macroSet.Add( macroSettings );
                }
            }

            // Create a new script
            if ( !NewScript( givenNameOfScript ) )
            {
                repoMessage = String.Format( "Create script {0} fail.", givenNameOfScript );
                return false;
            }

            // GetDicKeyStrOne back the class instance
            UScript workingScript = GetScript( givenNameOfScript );
            if ( workingScript == null )
            {
                repoMessage = String.Format( "GetDicKeyStrOne script {0} error.", givenNameOfScript );
                return false;
            }
            // config annotation
            if ( !string.IsNullOrEmpty( annotationOfScript ) )
                workingScript.Annotation = annotationOfScript;

            // Clear old macros
            workingScript.Clear();

            // Reload each macro
            bool bErr = false;
            for ( int i = 0 ; i < macroSet.Count ; i++ )
            {
                UMacro macro = null;
                // reusable, reload from pool
                if ( macroSet[ i ]._bReusable )
                {
                    macro = GetMacroFromReusablePool( macroSet[ i ]._strNameOfMacro );
                    if ( macro == null )
                    {
                        bErr = true; repoMessage = String.Format( "Cannot find({0}) calling method from pool", String.IsNullOrEmpty( macroSet[ i ]._strNameOfMacro ) ? "" : macroSet[ i ]._strNameOfMacro );
                        break;
                    }
                }
                // Not reusable, reload from file
                else
                {
                    UMacroMethodProviderPlugin pluginClass = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( macroSet[ i ]._strNameOfPluginSharpClassName );
                    if ( pluginClass == null )
                    {
                        bErr = true; repoMessage = String.Format( "Cannot find plugin class {0}.", macroSet[ i ]._strNameOfPluginSharpClassName ); break;
                    }

                    if ( !pluginClass.ReadMacroSettings( ref macro, settingsDirPath, macroSet[ i ]._strSetupFile ) || macro == null )
                    {
                        bErr = true; repoMessage = String.Format( "Call plugin class {0} read settings fail.", macroSet[ i ]._strSetupFile ); break;
                    }

                    // any other parameters need to be reloaded?
                    {
                        IUMacroAdditionalMethods addIf = macro as IUMacroAdditionalMethods;
                        if ( addIf != null ) addIf.ReadAdditionalParameters( macroSet[ i ]._CurrNod );
                    }

                }

                workingScript.Add( macro );
            }

            if ( bErr )
                workingScript.Clear();

            return (bErr ? false : true);
        }

        public bool ReadSettings( string zipfileFullPath, ref string repoMessage )
        {
            // Check source zip file
            if ( String.IsNullOrEmpty( zipfileFullPath ) || !File.Exists( zipfileFullPath ) )
            {
                repoMessage = String.Format( "File {0} not exist.", zipfileFullPath ); ;
                return false;
            }
            // Check temperal dir
            if ( !Directory.Exists( m_strTmpParametersRWPath ) )
            {
                try { Directory.CreateDirectory( m_strTmpParametersRWPath ); }
                catch { }
            }
            if ( String.IsNullOrEmpty( m_strTmpParametersRWPath ) || !Directory.Exists( m_strTmpParametersRWPath ) )
            {
                repoMessage = "Temperal dir not exist.";
                return false;
            }
            // Check if zip file
            if ( !ZipFile.IsZipFile( zipfileFullPath ) )
            {
                repoMessage = String.Format( "File {0} not a zip file.", zipfileFullPath );
                return false;
            }
            // Delete folder then create
            string tmpDir = String.Format( @"{0}\{1}", m_strTmpParametersRWPath, Path.GetFileNameWithoutExtension( zipfileFullPath ) );
            if ( Directory.Exists( tmpDir ) )
                Directory.Delete( tmpDir, true );

            Directory.CreateDirectory( tmpDir );
            // unzip file
            using ( ZipFile zip = new ZipFile( zipfileFullPath ) )
            {
                zip.ZipErrorAction = ZipErrorAction.Skip;
                zip.ExtractAll( tmpDir );
            }
            // Check specified xml exist
            string aScriptFileFullPath = String.Format( @"{0}\{1}", tmpDir, _strFilenameOfScript );
            string allScriptsFileFullPath = String.Format( @"{0}\{1}", tmpDir, _strFilenameOfAllScripts );
            string resuableMacrosFilePath = String.Format( @"{0}\{1}\{2}", tmpDir, _strDirnameOfReusableSettingsPath, _strFilenameOfReusables );
            bool bRetStat = false;

            if ( File.Exists( resuableMacrosFilePath ) )
                LoadSettingsToReusablePool( String.Format( @"{0}\{1}", tmpDir, _strDirnameOfReusableSettingsPath ), _strFilenameOfReusables );

            if ( File.Exists( aScriptFileFullPath ) )
            {
                // Do reloading
                try
                {
                    bRetStat = LoadScriptSettings( tmpDir, aScriptFileFullPath, ref repoMessage );
                }
                catch(Exception e)
                {
                    repoMessage = $"Exception: load {aScriptFileFullPath} with exception\n{e}";
                    bRetStat = false;
                }
            }
            else if ( File.Exists( allScriptsFileFullPath ) )
            {
                // clear previous scripts
                //ClearAllScripts();

                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load( allScriptsFileFullPath );

                    XmlNodeList nodeList = doc.SelectNodes( String.Format( "//{0}/{1}", _strNameOfAllScriptsXmlRootElement, _strNameOfAllScriptsXmlScriptElement ) );
                    if ( nodeList != null && nodeList.Count > 0 )
                    {
                        foreach ( XmlElement el in nodeList )
                        {
                            if ( el != null && !String.IsNullOrEmpty( el.InnerText ) )
                            {
                                string scriptSettingsDirPath = String.Format( @"{0}\{1}", tmpDir, el.InnerText );
                                string scriptSettingsFilePath = String.Format( @"{0}\{1}", scriptSettingsDirPath, _strFilenameOfScript );

                                LoadScriptSettings( scriptSettingsDirPath, scriptSettingsFilePath, ref repoMessage );
                            }
                        }
                    }
                    bRetStat = true;
                }
                catch(Exception e)
                {
                    repoMessage = $"Exception: load all with exception {e}";
                    bRetStat = false;
                }
            }
            else
            {
                repoMessage = "Script settings description not exist.";
            }

            if ( bRetStat )
            {
                // broadcast scripts load setting done
                for ( int i = 0 ; i < m_Scripts.Count ; i++ )
                {
                    if ( m_Scripts[ i ] == null ) continue;
                    UScript.SpreadSetCmd( m_Scripts[ i ].MacroSet, UScriptCtrlMarcoToProviderPlugins.MacroSettingsReadDone1stChanceCmd, UScriptCtrlMarcoToProviderPlugins.GenMacroSettingsReadDone1stChanceCmd( m_Scripts ) );
                    UScript.SpreadSetCmd( m_Scripts[ i ].MacroSet, UScriptCtrlMarcoToProviderPlugins.MacroSettingsReadDone2ndChanceCmd, UScriptCtrlMarcoToProviderPlugins.GenMacroSettingsReadDone2ndChanceCmd( m_Scripts ) );
                }
            }

            Directory.Delete( tmpDir, true );
            return bRetStat;
        }

        #endregion

        #region Save Settings

        private bool SaveScriptSettings( UScript script, string dirPathForWriteSettings, ref string repoMessage )
        {
            if ( script.MacroSet == null || script.MacroSet.Count <= 0 )
            {
                repoMessage = "Nothing to save.";
                return true;
            }

            // Write specified xml
            var status = true;
            string xmlFilePath = String.Format( @"{0}\{1}", dirPathForWriteSettings, _strFilenameOfScript );
            using ( Stream ws = new FileStream( xmlFilePath, FileMode.Create ) )
            {
                XmlTextWriter xmlWr = new XmlTextWriter( ws, Encoding.UTF8 );
                xmlWr.Formatting = Formatting.Indented;

                xmlWr.WriteStartDocument();
                xmlWr.WriteStartElement( _strNameOfScriptRootElement );

                // Write given name
                xmlWr.WriteElementString( _strNameOfScriptGivenNameElement, script.NameOfId );

                // Write ID
                xmlWr.WriteElementString( _strNameOfScriptSnElement, script.SnOfId.ToString() );

                // Write annotation
                xmlWr.WriteElementString( _strNameOfScriptAnnotation, script.Annotation );

                // Write count
                xmlWr.WriteElementString( _strNameOfScriptNumOfMacrosElement, script.MacroSet.Count.ToString() );

                // Call to save
                for ( int i = 0 ; i < script.MacroSet.Count ; i++ )
                {
                    UMacroMethodProviderPlugin plugin = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( script.MacroSet[ i ].OwnerClassName );
                    string macroSettingsFilename = null;
                    if ( plugin == null ) continue;
                    if ( script.MacroSet[ i ] == null ) continue;
                    xmlWr.WriteStartElement( _strNameOfMacroSettingsElement );

                    xmlWr.WriteElementString( _strNameOfMacroMethodInPluginClassSharpNameElement, script.MacroSet[ i ].OwnerClassName );
                    xmlWr.WriteElementString( _strNameOfMacroMethodNameElement, script.MacroSet[ i ].MethodName );

                    xmlWr.WriteElementString( _strNameOfMacroGivenNameElement, String.IsNullOrEmpty( script.MacroSet[ i ].GivenName ) ? "" : script.MacroSet[ i ].GivenName );
                    xmlWr.WriteElementString( _strNameOfMacroReusableFlagElement, script.MacroSet[ i ].ReusableMacro.ToString() );

                    IUMacroAdditionalMethods addIf = script.MacroSet[ i ] as IUMacroAdditionalMethods;
                    if ( addIf != null ) addIf.WriteAdditionalParameters( xmlWr );

                    macroSettingsFilename = String.Format( "{0}.xml", i.ToString() );
                    xmlWr.WriteElementString( _strFilenameOfMacroSettingsElement, script.MacroSet[ i ].ReusableMacro ? "" : macroSettingsFilename );

                    xmlWr.WriteEndElement();

                    if ( !script.MacroSet[ i ].ReusableMacro )
                    {
                        if (!plugin.WriteMacroSettings( script.MacroSet[ i ], dirPathForWriteSettings, macroSettingsFilename ))
                        {
                            status = false;
                            break;
                        }
                    }
                }

                xmlWr.WriteEndElement();
                xmlWr.WriteEndDocument();

                xmlWr.Flush();
                xmlWr.Close();
            }

            return status;
        }

        private bool SaveSettings( string nameOfScript, string tmpRootWorkPath, string filePath, ref string retSaveSettingsDirPath, ref string repoMessage )
        {
            retSaveSettingsDirPath = null;

            if ( String.IsNullOrEmpty( filePath ) )
            {
                repoMessage = "File name was null.";
                return false;
            }
            if ( m_refAssemblyLoader == null )
            {
                repoMessage = "No loader information.";
                return false;
            }
            UScript script = GetScript( nameOfScript );
            if ( script == null )
            {
                repoMessage = String.Format( "Cannot find {0}.", nameOfScript );
                return false;
            }
            // Check temperal dir
            if ( !Directory.Exists( tmpRootWorkPath ) )
            {
                try { Directory.CreateDirectory( tmpRootWorkPath ); }
                catch { }
            }
            if ( String.IsNullOrEmpty( tmpRootWorkPath ) || !Directory.Exists( tmpRootWorkPath ) )
            {
                repoMessage = "Temperal dir not exist.";
                return false;
            }
            // Create a folder for zip
            string nm = Path.GetFileNameWithoutExtension( filePath );
            if ( String.IsNullOrEmpty( nm ) )
            {
                repoMessage = "Invalid file.";
                return false;
            }
            string tmpDir = String.Format( @"{0}\{1}", tmpRootWorkPath, nm ); // append the file name without extension as a save settings dir path
            if ( Directory.Exists( tmpDir ) )
                Directory.Delete( tmpDir, true );
            Directory.CreateDirectory( tmpDir );
            // Do save settings
            bool bFinalStat = SaveScriptSettings( script, tmpDir, ref repoMessage );
            retSaveSettingsDirPath = tmpDir;

            return bFinalStat;
        }

        public bool WriteScriptSettings( string nameOfScript, string zipFileFullPath, ref string repoMessage )
        {
            string tmpSettingFolder = null;
            bool bStat = SaveSettings( nameOfScript, m_strTmpParametersRWPath, zipFileFullPath, ref tmpSettingFolder, ref repoMessage );

            if ( bStat && !String.IsNullOrEmpty( tmpSettingFolder ) && Directory.Exists( tmpSettingFolder ) )
            {
                using ( ZipFile zip = new ZipFile() )
                {
                    zip.AddDirectory( tmpSettingFolder );
                    zip.Save( zipFileFullPath );
                }
            }
            if ( Directory.Exists( tmpSettingFolder ) )
                Directory.Delete( tmpSettingFolder, true );

            return bStat;
        }

        public bool WriteAllScriptsSettings( string zipFileFullPath, ref string repoMessage )
        {
            if ( m_bDisposed )
            {
                repoMessage = "An Instance was disposed.";
                return false;
            }
            if ( m_Scripts == null || m_Scripts.Count <= 0 )
            {
                repoMessage = "Nothing to save.";
                return false;
            }

            if ( String.IsNullOrEmpty( m_strTmpParametersRWPath ) || !Directory.Exists( m_strTmpParametersRWPath ) )
            {
                repoMessage = "Temperal path not exist.";
                return false;
            }

            string tmpRwRootWorkDir = String.Format( @"{0}\{1}", m_strTmpParametersRWPath, CommonUtilities.GetCurrentTimeStr() );
            string tmpRwDirForReusables = String.Format( @"{0}\{1}", tmpRwRootWorkDir, _strDirnameOfReusableSettingsPath );
            if ( !Directory.Exists( tmpRwRootWorkDir ) )
                Directory.CreateDirectory( tmpRwRootWorkDir );
            if ( !Directory.Exists( tmpRwDirForReusables ) )
                Directory.CreateDirectory( tmpRwDirForReusables );

            // save reusable macros
            SaveSettingsFromReusablePool( tmpRwDirForReusables, _strFilenameOfReusables );

            bool bErrOccur = false;
            string tmpRepoWriteSettingDirPath = null;
            string[] dirPathesOfScripts = new string[ m_Scripts.Count ];
            string[] namesOfScripts = new string[ m_Scripts.Count ];
            bool[] states = new bool[ m_Scripts.Count ];

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                states[ i ] = SaveSettings( m_Scripts[ i ].NameOfId, tmpRwRootWorkDir, String.Format( "Script{0}.zip", i ), ref tmpRepoWriteSettingDirPath, ref repoMessage );
                dirPathesOfScripts[ i ] = String.IsNullOrEmpty( tmpRepoWriteSettingDirPath ) ? null : String.Copy( tmpRepoWriteSettingDirPath );
                namesOfScripts[ i ] = String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ? "" : String.Copy( m_Scripts[ i ].NameOfId );
                if ( !states[ i ] )
                {
                    bErrOccur = true; break;
                }
            }

            if ( !bErrOccur )
            {
                // Write xml first
                string xmlPath = String.Format( @"{0}\{1}", tmpRwRootWorkDir, _strFilenameOfAllScripts );
                using ( Stream ws = new FileStream( xmlPath, FileMode.Create ) )
                {
                    XmlTextWriter xmlWr = new XmlTextWriter( ws, Encoding.UTF8 );
                    xmlWr.Formatting = Formatting.Indented;

                    xmlWr.WriteStartDocument();
                    xmlWr.WriteStartElement( _strNameOfAllScriptsXmlRootElement );

                    for ( int i = 0 ; i < dirPathesOfScripts.Length ; i++ )
                    {
                        if ( String.IsNullOrEmpty( dirPathesOfScripts[ i ] ) || !Directory.Exists( dirPathesOfScripts[ i ] ) ) continue;
                        xmlWr.WriteStartElement( _strNameOfAllScriptsXmlScriptElement );
                        xmlWr.WriteAttributeString( _strNameOfAllScriptsXmlScriptElementAttName, String.IsNullOrEmpty( namesOfScripts[ i ] ) ? "" : namesOfScripts[ i ] );
                        xmlWr.WriteString( Path.GetFileNameWithoutExtension( dirPathesOfScripts[ i ] ) ); // save the folder name only
                        xmlWr.WriteEndElement();
                    }

                    xmlWr.WriteEndElement();
                    xmlWr.WriteEndDocument();

                    xmlWr.Flush();
                    xmlWr.Close();
                }

                // Zip them
                using ( ZipFile zip = new ZipFile() )
                {
                    zip.AddDirectory( tmpRwRootWorkDir );
                    zip.Save( zipFileFullPath );
                }
            }

            if ( Directory.Exists( tmpRwRootWorkDir ) )
                Directory.Delete( tmpRwRootWorkDir, true );

            return (bErrOccur ? false : true);
        }

        #endregion

        #region Support IDatIO interface

        public string UniqueName { get { return this.GetType().FullName; } }

        public string IOFileName { get { return "Descript.ini"; } }
        public bool CanWrite { get { return !m_bDisposed; } }
        public bool WriteDat( string folderPath, string fileName )
        {
            string zipFilePath = Path.GetFullPath( Path.Combine( folderPath, String.Format("All_{0}.zip", CommonUtilities.GetCurrentTimeStr() ) ) );
            string dummy = null;
            if (!WriteAllScriptsSettings(zipFilePath, ref dummy )) {
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
        public bool CanRead { get { return !m_bDisposed; } }
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
            return ReadSettings( zipFilePath, ref dummy );

        }

        public bool PopupGUI()
        {
            frmScriptPopupMacroConfig dlg = new frmScriptPopupMacroConfig();
            dlg.SS = this;
            dlg.ShowDialog();

            dlg.Dispose();
            dlg = null;
            return true;
        }
        UserControlMacroConfig _controlMacroConfig = null;
        public Control GetGUI()
        {
            if ( _controlMacroConfig == null) {
                _controlMacroConfig = new UserControlMacroConfig();
                _controlMacroConfig.SS = this;
            }

            _controlMacroConfig.UpdateSS();
            return _controlMacroConfig;
        }
        public void SetMacroConfigControlSize(int w, int h)
        {
            if ( w <= 0 || h <= 0 )
                return;
            if ( _controlMacroConfig == null )
                return;
            _controlMacroConfig.Width = w;
            _controlMacroConfig.Height = h;
        }

        #endregion

    }
}
