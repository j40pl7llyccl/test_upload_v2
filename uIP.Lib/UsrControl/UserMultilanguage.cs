using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using uIP.Lib.Utility;

using uIP.Lib;

namespace uIP.Lib.UsrControl
{
    /// <summary>
    /// Multilanguage control
    /// - load from root with specific sub-folder(language code)
    /// - handle switch
    /// </summary>
    public class UserMultilanguage : IMultilanguageManager, IDisposable
    {
        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;
        protected Form m_refMainForm = null;
        protected object m_hSyncOp = new object();
        protected string m_strPathContainLangs = null;
        protected string m_strDefaultLangCode = null;
        protected string[] m_AvailableLangs = null;
        protected fpLogMessage m_Log = null;

        protected static Type[] DefaultIgnoreOutputTypes = new Type[]
        {
            typeof(NumericUpDown),
            typeof(TextBox),
        };

        public string MultilangFolder {  get { return m_strPathContainLangs; } }

        public UserMultilanguage( fpLogMessage log, Form frmMain, string pathContainLangs)
        {
            m_Log = log;
            m_refMainForm = frmMain;
            if (Directory.Exists(pathContainLangs)) {
                m_strPathContainLangs = String.Copy( pathContainLangs );
                m_AvailableLangs = GetAvailableLangs( m_strPathContainLangs );
            }
        }

        public void Dispose()
        {
            if ( m_bDisposing || m_bDisposed )
                return;
            m_bDisposing = true;

            m_SwitchLangCallbacks.Clear();
            m_RegControls.Clear();

            m_bDisposed = true;
            m_bDisposing = false;
        }

        private  static string[] GetAvailableLangs(string rootPath)
        {
            if ( !Directory.Exists( rootPath ) )
                return null;

            string[] dirs = Directory.GetDirectories( rootPath, "*", SearchOption.TopDirectoryOnly );
            List<string> langs = new List<string>();
            for ( int i = 0; i < dirs.Length; i++ ) {
                langs.Add( Path.GetFileName( dirs[ i ] ) );
            }
            return langs.ToArray();
        }

        protected string m_strCurrentLanguageCode = "en";
        protected List<GuiComponentCollection> m_GuiControlCollection = new List<GuiComponentCollection>();
        protected List<ReferenceBook> m_BooksReferences = new List<ReferenceBook>();
        protected Dictionary<string, string> m_StringReferences = new Dictionary<string, string>();
        protected List<fpMultilanguageSwitch> m_SwitchLangCallbacks = new List<fpMultilanguageSwitch>();
        protected List<RegControl> m_RegControls = new List<RegControl>();

        // folder dir contain supported language
        // + en
        //   - Control1.xml
        //   - Control2.xml
        //   - xxx.ini ( dictionary converting string )
        //   - xxx.ini
        // + zh_tc
        //   - Control1.xml
        //   - Control2.xml
        //   - xxx.ini
        //   - xxx.ini
        // + zh_sc
        //   - Control1.xml
        //   - Control2.xml
        //   - xxx.ini
        //   - xxx.ini

        public List<ReferenceBook> Books { get { return m_BooksReferences; } }
        public List<GuiComponentCollection> GuiBooks { get { return m_GuiControlCollection; } }
        public Dictionary<string, string> StringMapping { get { return m_StringReferences; } }

        public string CurrentLanguageCode { get { return m_strCurrentLanguageCode; } }
        public void SwitchLanguage( string langCode )
        {
            // load the files
            if (!Directory.Exists(m_strPathContainLangs)) {
                if ( m_Log != null ) m_Log(eLogMessageType.ERROR, 150, String.Format( "[UserMultilanguage::SwitchLanguage] directory {0} not exist!", m_strPathContainLangs ) );
                return;
            }

            string[] langs = GetAvailableLangs( m_strPathContainLangs );
            if (langs == null || langs.Length <= 0 || !langs.Contains(langCode)) {
                if ( m_Log != null ) m_Log(eLogMessageType.ERROR, 150, String.Format( "[UserMultilanguage::SwitchLanguage] language {0} not exist!", langCode ) );
                return;
            }
            m_AvailableLangs = langs;
            // reload all information
            ReloadLanguage( Path.Combine( m_strPathContainLangs, langCode ) );
            // config the language code
            m_strCurrentLanguageCode = String.Copy( langCode );
            // callback to each reg delegate
            CallNotification();
        }
        private void ReloadLanguage(string path)
        {
            // clear information
            m_GuiControlCollection.Clear();
            m_BooksReferences.Clear();
            m_StringReferences.Clear();

            // load from files
            string[] files = Directory.GetFiles( path, "*.*", SearchOption.TopDirectoryOnly );
            if ( files == null || files.Length <= 0 ) {
                if ( m_Log != null ) m_Log(eLogMessageType.WARNING, 100, String.Format( "[UserMultilanguage::ReloadLanguage] no file found!" ) );
                return;
            }

            for(int i = 0; i < files.Length; i++ ) {
                string ext = Path.GetExtension( files[ i ] ).ToLower();
                // read to string reference
                if (".txt" == ext || ".ini" == ext) {
                    IniReaderUtility r = new IniReaderUtility();
                    r.EncodeContentType = eIniContentEncoding.Utf8;
                    // parse the file
                    if (!r.Parsing(files[i])) {
                        if ( m_Log != null ) m_Log(eLogMessageType.NORMAL, 100, String.Format( "[UserMultilanguage::ReloadLanguage] parsing {0} as format ini fail!", files[ i ] ) );
                        continue;
                    }
                    // get all sections
                    string[] sections = r.GetSections();
                    if (sections == null || sections.Length <= 0) {
                        if ( m_Log != null ) m_Log(eLogMessageType.NORMAL, 100, String.Format( "[UserMultilanguage::ReloadLanguage] parsing {0} no section got!", files[ i ] ) );
                        continue;
                    }
                    // add each section and its dictionary
                    for ( int x = 0; x < sections.Length; x++ ) {
                        // find the existing
                        ReferenceBook book = null;
                        for ( int j = 0; j < m_BooksReferences.Count; j++ ) {
                            if ( m_BooksReferences[ j ].GivenName == sections[ x ] ) {
                                book = m_BooksReferences[ j ];
                                break;
                            }
                        }
                        Dictionary<string, string> dic = null;
                        // not found, create new
                        if (sections[x] == "common") {
                            dic = m_StringReferences;
                        }
                        else if ( book == null ) {
                            book = new ReferenceBook( sections[ x ] );
                            m_BooksReferences.Add( book );
                            dic = book.Dic;
                        } else {
                            dic = book.Dic;
                        }
                        // get the section dictionary
                        // [section]
                        // "key"="value"
                        // ...
                        SectionDataOfIni dat = r.Get( sections[ x ] );
                        if ( dat != null && dat.Data != null ) {
                            for ( int y = 0; y < dat.Data.Count; y++ ) {
                                if ( dat.Data[ y ].Values == null || dat.Data[ y ].Values.Length <= 0 )
                                    continue;
                                if ( String.IsNullOrEmpty( dat.Data[ y ].Key ) || String.IsNullOrEmpty( dat.Data[ y ].Values[ 0 ] ) )
                                    continue;
                                string key = dat.Data[ y ].Key.Trim().Replace("\"", "");
                                string val = dat.Data[ y ].Values[ 0 ].Trim().Replace( "\"", "" );

                                dic[ key ] = val; // add the dictionary
                            }
                        }
                    }
                }
                // read to component
                if (".xml" == ext) {
                    string typeOfRootCtrl = Path.GetFileNameWithoutExtension( files[ i ] );
                    bool bSame = false;
                    for ( int x = 0; x < m_GuiControlCollection.Count; x++ ) {
                        if ( m_GuiControlCollection[ x ].RootControlTypeFullName == typeOfRootCtrl ) {
                            bSame = true; break;
                        }
                    }
                    if (bSame) {
                        if ( m_Log != null ) m_Log(eLogMessageType.NORMAL, 100, String.Format( "[UserMultilanguage::ReloadLanguage] same one {0}!", typeOfRootCtrl ) );
                        continue;
                    }

                    // read from file
                    Dictionary<string, ComponentMlInfo> dic = null;
                    ReadControlXml( files[ i ], out dic );
                    if (dic != null) {
                        GuiComponentCollection c = new GuiComponentCollection( typeOfRootCtrl, dic );
                        m_GuiControlCollection.Add( c );
                    }
                }
            }
        }
        private delegate void fpCallNotification();
        private void CallNotification()
        {
            if (m_refMainForm != null && m_refMainForm.InvokeRequired) {
                m_refMainForm.Invoke( new fpCallNotification( CallNotification ) );
                return;
            }

            Monitor.Enter( m_hSyncOp );
            try {
                // Process registry ctrl
                ProcessRegCtrl();

                // notify reg delegate
                for(int i = 0; i < m_SwitchLangCallbacks.Count; i++ ) {
                    m_SwitchLangCallbacks[ i ]( this as IMultilanguageManager, m_strCurrentLanguageCode );
                }
            }finally {
                Monitor.Exit( m_hSyncOp );
            }
        }
        private void ScanMenuItemToChangeText( ToolStripItem itm, Dictionary<string, ComponentMlInfo> dic )
        {
            if ( itm == null )
                return;

            if ( dic.ContainsKey( itm.Name ) )
                itm.Text = dic[ itm.Name ].CurLangString;

            ToolStripMenuItem mi = itm as ToolStripMenuItem;
            if ( mi != null && mi.DropDownItems != null && mi.DropDownItems.Count > 0) {
                for(int i = 0; i < mi.DropDownItems.Count; i++ ) {
                    ScanMenuItemToChangeText( mi.DropDownItems[ i ], dic );
                }
            }
        }
        private void ScanToChangeText(Control ctrl, Dictionary<string, ComponentMlInfo> dic)
        {
            if (ctrl == null || ctrl.IsDisposed || ctrl.Disposing) {
                return;
            }
            ComboBox cb = ctrl as ComboBox;
            if ( cb != null ) {
                ComponentMlInfo info = dic.ContainsKey( cb.Name ) ? dic[ cb.Name ] : null;
                // find and replace
                if ( cb.Items.Count > 0  && info != null) {
                    for ( int i = 0; i < cb.Items.Count; i++ ) {
                        int index = -1;
                        // get from ori list
                        if (info.ListOriLangStrings != null && info.ListOriLangStrings.Count > 0) {
                            for (int x = 0; x < info.ListOriLangStrings.Count; x++ ) {
                                if (info.ListOriLangStrings[x] == cb.Items[i].ToString()) {
                                    index = x; break;
                                }
                            }
                        }
                        // get from cur lang list and replace it
                        if (index >= 0 && info.ListCurLangStrings != null && index < info.ListCurLangStrings.Count) {
                            cb.Items[ i ] = info.ListCurLangStrings[ index ];
                        }
                    }
                }
            } else {
                // if got, replace it. Otherwise, use the original
                if ( dic.ContainsKey( ctrl.Name ) && !String.IsNullOrEmpty( dic[ ctrl.Name ].CurLangString ) ) {
                    ctrl.Text = dic[ ctrl.Name ].CurLangString;
                } else {
                    // search the common book
                    if ( m_StringReferences != null) {
                        string str = ctrl.Tag as string;
                        if ( !String.IsNullOrEmpty( str ) && m_StringReferences.ContainsKey( str ) )
                            ctrl.Text = m_StringReferences[ str ];
                    }
                }
                //ctrl.Text = dic.ContainsKey( ctrl.Name ) ? ( String.IsNullOrEmpty(dic[ ctrl.Name ].CurLangString) ? ctrl.Text : dic[ctrl.Name].CurLangString ) : ctrl.Text;
            }

            // if has sub-control, recursive call
            if (ctrl.Controls != null && ctrl.Controls.Count > 0) {
                for (int i = 0; i < ctrl.Controls.Count; i++ ) {
                    ScanToChangeText( ctrl.Controls[ i ], dic );
                }
            }
            // MenuStrip type scan its sub-items
            if (ctrl is MenuStrip) {
                MenuStrip ms = ctrl as MenuStrip;
                for(int i = 0; i < ms.Items.Count; i++ ) {
                    ScanMenuItemToChangeText( ms.Items[ i ], dic );
                }
            }
        }
        private void ProcessRegCtrl()
        {
            for(int i =0; i < m_RegControls.Count; i++ ) {
                GuiComponentCollection got = null;
                string typeFullName = null;
                if ( m_RegControls[ i ]._OwnerCtrl != null) {
                    typeFullName = m_RegControls[ i ]._OwnerCtrl.GetType().FullName;
                } else if (m_RegControls[i]._RegCtrl != null) {
                    typeFullName = m_RegControls[ i ]._RegCtrl.GetType().FullName;
                }
                if ( String.IsNullOrEmpty( typeFullName ) )
                    continue;
                // get the control from full type name
                for (int x = 0; x < m_GuiControlCollection.Count; x++ ) {
                    if (m_GuiControlCollection[x].RootControlTypeFullName == typeFullName) {
                        got = m_GuiControlCollection[ x ];
                        break;
                    }
                }
                if ( got == null )
                    continue;
                // update the name
                ScanToChangeText( m_RegControls[ i ]._RegCtrl, got.References );
            }
        }

        public void InstallLanguageSwitchCallback( fpMultilanguageSwitch call )
        {
            if ( m_bDisposing || m_bDisposed )
                return;
            Monitor.Enter( m_hSyncOp );
            try {
                if ( !m_SwitchLangCallbacks.Contains( call ) )
                    m_SwitchLangCallbacks.Add( call );
            } finally {
                Monitor.Exit( m_hSyncOp );
            }
        }
        public void RemoveLanguageSwitchCallback( fpMultilanguageSwitch call )
        {
            if ( m_bDisposing || m_bDisposed )
                return;
            Monitor.Enter( m_hSyncOp );
            try {
                if ( !m_SwitchLangCallbacks.Contains( call ) )
                    m_SwitchLangCallbacks.Remove( call );
            } finally {
                Monitor.Exit( m_hSyncOp );
            }
        }
        public void RegistryControl( Control owner, Control ctrl )
        {
            if ( m_bDisposing || m_bDisposed )
                return;
            if ( ctrl == null )
                return;

            Monitor.Enter( m_hSyncOp );
            try {
                for (int i = 0; i < m_RegControls.Count;  i++) {
                    if (m_RegControls[i]._RegCtrl == ctrl) {
                        m_RegControls[ i ]._OwnerCtrl = owner;
                        return;
                    }
                }

                m_RegControls.Add(new RegControl( owner, ctrl));
            }finally {
                Monitor.Exit( m_hSyncOp );
            }

        }
        public void UnregistryControl( Control ctrl )
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            Monitor.Enter( m_hSyncOp );
            try {
                for ( int i = 0; i < m_RegControls.Count; i++ ) {
                    if (m_RegControls[i]._RegCtrl == ctrl) {
                        m_RegControls.RemoveAt( i );
                        break;
                    }
                }
            }finally {
                Monitor.Exit( m_hSyncOp );
            }
        }
        public bool ScanControl( Control owner, Control ctrl ) // for tab page out of the form or normal form
        {
            if ( m_bDisposing || m_bDisposed )
                return false;
            if ( ctrl == null )
                return false;

            bool ret = false;
            Monitor.Enter( m_hSyncOp );
            try {
                string strSrch = owner == null ? ctrl.GetType().FullName : owner.GetType().FullName;
                for(int i = 0; i < m_GuiControlCollection.Count; i++ ) {
                    if (m_GuiControlCollection[i].RootControlTypeFullName == strSrch) {
                        ScanToChangeText( ctrl, m_GuiControlCollection[ i ].References );
                        ret = true;
                    }
                }
            } finally {
                Monitor.Exit( m_hSyncOp );
            }

            return ret;
        }
        public string Get( string book, string str )
        {
            if ( m_bDisposing || m_bDisposed )
                return str;

            string ret = str;
            Monitor.Enter( m_hSyncOp );
            try {
                bool got = false;
                for (int i = 0; i < m_BooksReferences.Count; i++ ) {
                    if (m_BooksReferences[i].GivenName == book) {
                        if (m_BooksReferences[i].Dic.ContainsKey(str)) {
                            ret = m_BooksReferences[ i ].Dic[ str ];
                            got = true; break;
                        }
                    }
                }
                if (!got) {
                    if ( m_StringReferences.ContainsKey( str ) )
                        ret = m_StringReferences[ str ];
                }
            } finally {
                Monitor.Exit( m_hSyncOp );
            }

            return ret;
        }
        public string Get(string str)
        {
            if ( m_bDisposing || m_bDisposed )
                return str;

            string ret = str;
            Monitor.Enter( m_hSyncOp );
            try {
                if ( m_StringReferences.ContainsKey( str ) )
                    ret = m_StringReferences[ str ];
            } finally {
                Monitor.Exit( m_hSyncOp );
            }

            return ret;
        }

        private static void ReadControlXml(string filepath, out Dictionary<string, ComponentMlInfo> dic)
        {
            dic = null;
            if ( !File.Exists( filepath ) )
                return;

            dic = new Dictionary<string, ComponentMlInfo>();

            try {
                using(Stream rs = File.Open(filepath, FileMode.Open)) {
                    XmlReader r = XmlReader.Create( rs );
                    if (r != null) {
                        bool bIsCombobox = false;
                        string curNodeEleName_ctrlName = null;
                        string comboxName = null;
                        while(r.Read()) {
                            switch( r.NodeType) {
                            case XmlNodeType.Element:
                                if ( r.Name == "ControlDescription" )
                                    break;
                                if ( bIsCombobox || !String.IsNullOrEmpty(comboxName) ) {
                                    if (r.Name == "item") {
                                        if ( dic.ContainsKey( curNodeEleName_ctrlName ) )
                                            dic[ curNodeEleName_ctrlName ].ListOriLangStrings.Add( r.GetAttribute( "lang" ) );
                                    }
                                } else {
                                    curNodeEleName_ctrlName = String.Copy( r.Name );
                                    dic[ curNodeEleName_ctrlName ] = new ComponentMlInfo( r.GetAttribute( "type" ), r.GetAttribute( "lang" ) );
                                    if ( dic[ curNodeEleName_ctrlName ].TypeName == typeof( ComboBox ).Name ) {
                                        comboxName = r.Name;
                                    }
                                }
                                break;

                            case XmlNodeType.Text:
                                if ( bIsCombobox || !String.IsNullOrEmpty( comboxName ) ) {
                                    if ( dic.ContainsKey( curNodeEleName_ctrlName ) )
                                        dic[ curNodeEleName_ctrlName ].ListCurLangStrings.Add( String.IsNullOrEmpty( r.Value ) ? "" : String.Copy( r.Value ) );
                                } else {
                                    if ( !String.IsNullOrEmpty( comboxName ) ) bIsCombobox = true;
                                    if ( dic.ContainsKey( curNodeEleName_ctrlName ) )
                                        dic[ curNodeEleName_ctrlName ].CurLangString = String.IsNullOrEmpty( r.Value ) ? "" : String.Copy( r.Value );
                                }
                                break;

                            case XmlNodeType.EndElement:
                                if (r.Name == comboxName) {
                                    bIsCombobox = false;
                                    comboxName = null;
                                }
                                break;
                            }
                        }
                    }
                }
            } catch{ }
        }

        private static void WriteMenuItemXml(XmlWriter w, ToolStripItem itm)
        {
            if ( w == null || itm == null )
                return;

            w.WriteStartElement( String.Format( "{0}", itm.Name ) );
            w.WriteAttributeString( "type", itm.GetType().Name );
            w.WriteAttributeString( "lang", itm.Text );
            w.WriteString( itm.Text );

            ToolStripMenuItem mi = itm as ToolStripMenuItem;
            if (mi != null && mi.DropDownItems != null && mi.DropDownItems.Count > 0) {
                for (int i = 0; i < mi.DropDownItems.Count; i++ ) {
                    WriteMenuItemXml( w, mi.DropDownItems[ i ] );
                }
            }

            w.WriteEndElement();
        }
        private static void WriteControlXml(XmlWriter w, Control ctrl)
        {
            if ( w == null || ctrl == null || ctrl.IsDisposed || ctrl.Disposing )
                return;
            if ( string.IsNullOrEmpty( ctrl.Name ) )
                return;

            // check if inside ignore list
            foreach(var t in DefaultIgnoreOutputTypes)
            {
                if ( ctrl.GetType() == t )
                    return;
            }

            // Type 1:
            // <(name of control) type = (C# type full name) lang = (Ori text)>
            //   Apply to current language string
            // </(name of control)>
            //
            // Type 2: combobox
            // <(name of control) type = (C# type full name) lang = (Ori text)>
            //   <item lang = (Ori text)> New lang string </item>
            //   <item lang = (Ori text)> New lang string </item>
            //   ...
            // </(name of control)>

            w.WriteStartElement( String.Format( "{0}", ctrl.Name ) );
            w.WriteAttributeString( "type", ctrl.GetType().Name );
            w.WriteAttributeString( "lang", string.IsNullOrEmpty( ctrl.Text ) ? "" : ctrl.Text );

            ComboBox cb = ctrl as ComboBox;
            if (cb != null && cb.Items != null) {
                for (int i = 0; i < cb.Items.Count; i++ ) {
                    w.WriteStartElement( "item" );
                    w.WriteAttributeString( "lang", cb.Items[ i ].ToString() );
                    w.WriteString( cb.Items[ i ].ToString() );
                    w.WriteEndElement();
                }
            } else {
                w.WriteString( string.IsNullOrEmpty(ctrl.Text) ? "" : ctrl.Text );
            }

            if (ctrl.Controls != null) {
                foreach ( Control c in ctrl.Controls ) {
                    WriteControlXml( w, c );
                }
            }
            if (ctrl is MenuStrip) {
                MenuStrip ms = ctrl as MenuStrip;
                for(int i = 0; i < ms.Items.Count; i++ ) {
                    WriteMenuItemXml( w, ms.Items[ i ] );
                }
            }

            w.WriteEndElement();
        }
        public static void WriteControlXml(Control ctrl, string filePath)
        {
            if ( ctrl == null )
                return;
            try {
                XmlWriterSettings s = new XmlWriterSettings();
                s.Indent = true;
                s.NewLineOnAttributes = true;
                using ( Stream ws = File.Open( filePath, FileMode.Create ) ) {
                    XmlWriter w = XmlWriter.Create( ws, s );
                    if ( w != null ) {
                        w.WriteStartDocument();
                        w.WriteStartElement( "ControlDescription" );

                        WriteControlXml( w, ctrl );

                        w.WriteEndElement();
                        w.WriteEndDocument();

                        w.Flush();
                        w.Close();
                    }
                }
            } catch {
            }
        }
        public static void WriteControlXml(string typeFullName, string filePath)
        {
            Type tp = PluginAssemblies.GetTypeByFullName( typeFullName );
            if ( tp == null || String.IsNullOrEmpty( filePath ) )
                return;
            Object o = null;
            try { o = Activator.CreateInstance( tp ); }catch { return; }
            Control c = o as Control;

            if (c == null) {
                IDisposable disp = o as IDisposable;
                if (disp != null) {
                    disp.Dispose();
                }
                return;
            }

            try {
                XmlWriterSettings s = new XmlWriterSettings();
                s.Indent = true;
                s.NewLineOnAttributes = true;
                using ( Stream ws = File.Open(filePath, FileMode.Create)) {
                    XmlWriter w = XmlWriter.Create( ws, s );
                    if ( w != null ) {
                        w.WriteStartDocument();
                        w.WriteStartElement( "ControlDescription" );

                        WriteControlXml( w, c );

                        w.WriteEndElement();
                        w.WriteEndDocument();

                        w.Flush();
                        w.Close();
                    }
                }
            } catch {
                c.Dispose();
                return;
            }

            c.Dispose();
            c = null;
            o = null;
        }
        private delegate void fpWriteControls( string dir );
        // output all controls to file
        public void WriteControls(string dir)
        {
            // usually write en
            if (!Directory.Exists(dir)) {
                return;
            }

            if (m_refMainForm != null) {
                if (m_refMainForm.InvokeRequired) {
                    m_refMainForm.Invoke( new fpWriteControls( WriteControls ), new object[] { dir } );
                    return;
                }
            }

            for (int i = 0; i < m_RegControls.Count; i++ ) {
                if ( m_RegControls[ i ] == null || m_RegControls[i]._RegCtrl == null )
                    continue;
                // create a file to write
                string filePath = Path.Combine( dir, String.Format( "{0}.xml", m_RegControls[ i ]._RegCtrl.GetType().FullName ) );
                XmlWriterSettings s = new XmlWriterSettings();
                s.Indent = true;
                s.NewLineOnAttributes = true;
                try {
                    using ( Stream ws = File.Open( filePath, FileMode.Create ) ) {
                        XmlWriter xw = XmlWriter.Create( ws, s );
                        if (xw != null) {
                            xw.WriteStartDocument();
                            xw.WriteStartElement( "ControlDescription" );
                            if ( m_RegControls[ i ]._OwnerCtrl != null )
                                xw.WriteElementString( "Owner", m_RegControls[ i ]._OwnerCtrl.GetType().FullName );
                            WriteControlXml( xw, m_RegControls[i]._RegCtrl );

                            xw.WriteEndElement();
                            xw.WriteEndDocument();

                            xw.Flush();
                            xw.Close();
                        }
                    }
                }
                catch(Exception exp) {
                    if (m_Log != null) {
                        m_Log(eLogMessageType.WARNING, 50, String.Format( "[UserMultilanguage::WriteControls] to file {0} with error\n{1}", filePath, exp.ToString() ) );
                    }
                }

            }
        }
    }
}
