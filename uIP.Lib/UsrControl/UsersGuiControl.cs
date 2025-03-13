using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

using uIP.Lib;
using uIP.Lib.Utility;

namespace uIP.Lib.UsrControl
{
    /// <summary>
    /// using this class
    /// - static form not like dialog
    ///   - using CreateForm/ AddForm
    /// - dialog
    ///   - create by using CreateShowDialog and handled by caller
    ///   - only using the access right to control components
    ///
    /// Remark
    /// - all items were flatten into ini section by owner type full name
    /// - name of item cannot be the same
    /// </summary>
    public class UsersGuiControl : IDisposable
    {
        private bool m_bDisposing = false;
        private bool m_bDisposed = false;
        private bool _bClosing = false;
        private List<KeepingPageCtrl> _KeepingTabPageCtrl = new List<KeepingPageCtrl>();
        private List<KeepingFormCtrl> _KeepingForms = new List<KeepingFormCtrl>();
        private List<ComponentAccRight> _ComponentsAccRights = new List<ComponentAccRight>();
        private object m_hSyncOp = new object();
        private Form m_refMainForm = null;
        private IMultilanguageManager m_refMM = null;

        public UsersGuiControl( Form frmMain, IMultilanguageManager mm )
        {
            m_refMainForm = frmMain;
            m_refMM = mm;
        }

        public void Close()
        {
            Monitor.Enter( m_hSyncOp );
            try {

                if ( _bClosing ) return;
                _bClosing = true;
                if ( _KeepingTabPageCtrl.Count > 0 ) {
                    for ( int i = 0; i < _KeepingTabPageCtrl.Count; i++ ) {
                        if ( _KeepingTabPageCtrl[ i ]._tbCtrl != null ) {
                            _KeepingTabPageCtrl[ i ]._tbCtrl.Dispose();
                        }
                    }
                    _KeepingTabPageCtrl.Clear();
                }
                if ( _KeepingForms.Count > 0 ) {
                    for ( int i = 0; i < _KeepingForms.Count; i++ ) {
                        if ( _KeepingForms[ i ] != null ) {
                            _KeepingForms[ i ].Close();
                        }
                    }
                    _KeepingForms.Clear();
                }

                _ComponentsAccRights.Clear();
            } finally {
                Monitor.Exit( m_hSyncOp );
            }
        }

        public void Dispose()
        {
            if ( m_bDisposing ) return;
            m_bDisposing = true;
            Close();
            m_bDisposed = true;
            m_bDisposing = false;
        }

        public Form CreateForm<T>(bool bHandle = true)
        {
            if ( m_bDisposing || m_bDisposed )
                return null;
            KeepingFormCtrl got = KeepingFormCtrl.New<T>( bHandle );
            if (got != null) {
                Monitor.Enter( m_hSyncOp );
                try {
                    _KeepingForms.Add( got );
                    if ( m_refMM != null ) m_refMM.RegistryControl( null, got._Form );
                } finally { Monitor.Exit( m_hSyncOp ); }
                return got._Form;
            }

            return null;
        }
        public bool AddForm(Form frm, bool bHandle = true)
        {
            if ( m_bDisposing || m_bDisposed )
                return false;
            if ( frm == null ) return false;

            Monitor.Enter( m_hSyncOp );

            try {
                for ( int i = 0; i < _KeepingForms.Count; i++ ) {
                    if ( _KeepingForms[ i ]._Form == frm ) {
                        _KeepingForms[ i ]._bHandle = bHandle;
                        return true;
                    }
                }
                _KeepingForms.Add( new KeepingFormCtrl( frm, bHandle ) );
                if ( m_refMM != null ) m_refMM.RegistryControl( null, frm );
            } finally {
                Monitor.Exit( m_hSyncOp );
            }
            return true;
        }
        public void RemoveForm(Form frm , bool bBySettingHandleFormInst  = true)
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            if ( frm == null )
                return;

            Monitor.Enter( m_hSyncOp );
            try {
                bool bEnd = true;
                while ( true ) {
                    bEnd = true;
                    for ( int i = 0; i < _KeepingTabPageCtrl.Count; i++ ) {
                        if ( _KeepingTabPageCtrl[ i ]._OwnerForm == ( frm as Control ) ) {
                            // Insert tab page back
                            TabControl tbCtrl = _KeepingTabPageCtrl[ i ]._Parent as TabControl;
                            if ( tbCtrl != null ) {
                                if ( m_refMM != null ) m_refMM.UnregistryControl( tbCtrl );
                                tbCtrl.TabPages.Add( _KeepingTabPageCtrl[ i ]._tbCtrl );
                            }
                            // Remove current
                            _KeepingTabPageCtrl.RemoveAt( i );
                            bEnd = false; break;
                        }
                    }
                    if ( bEnd )
                        break;
                }

                for ( int i = 0; i < _KeepingForms.Count; i++ ) {
                    if ( _KeepingForms[ i ]._Form == frm ) {
                        if ( m_refMM != null ) m_refMM.UnregistryControl( frm );
                        if ( bBySettingHandleFormInst && _KeepingForms[ i ]._bHandle ) { // allow to handle by setting
                            _KeepingForms[ i ].Close();
                        }
                        // remove current index
                        _KeepingForms.RemoveAt( i );
                        break;
                    }
                }
            } finally {
                Monitor.Exit( m_hSyncOp );
            }
        }
        public Form CreateShowDialog<T>( int nEnabledLvl, int nVisibleLvl )
        {
            if ( m_bDisposing || m_bDisposed )
                return null;

            if ( !typeof( T ).IsSubclassOf( typeof( Form ) ) )
                return null;

            Type tp = typeof( T );
            Form frm = Activator.CreateInstance<T>() as Form;
            ScanAccRight( new string[] { tp.FullName }, frm, _ComponentsAccRights, nEnabledLvl, nVisibleLvl, frm );
            if ( m_refMM != null ) m_refMM.ScanControl( null, frm );
            return frm;
        }

        public void AddComponent(string nameOfOwnerForm, string nameOfComponent, int nEnabledLvl, int nVisibleLvl)
        {
            Monitor.Enter( m_hSyncOp );
            try {
                for ( int i = 0; i < _ComponentsAccRights.Count; i++ ) {
                    if ( _ComponentsAccRights[ i ]._strOwner == nameOfOwnerForm && _ComponentsAccRights[ i ]._strName == nameOfComponent ) {
                        _ComponentsAccRights[ i ]._nEnableLevel = nEnabledLvl;
                        _ComponentsAccRights[ i ]._nVisibleLevel = nVisibleLvl;
                        return;
                    }
                }
                _ComponentsAccRights.Add( new ComponentAccRight( nameOfOwnerForm, nameOfComponent, nEnabledLvl, nVisibleLvl ) );
            } finally {
                Monitor.Exit( m_hSyncOp );
            }
        }

        private delegate void fpScanAccRight( int curEnableLvl, int curVisibleLvl, fpUserRightsOutMessage fp, bool defLockedEnable, bool defLockedVisible );
        public void ScanAccRight( int curEnableLvl, int curVisibleLvl, fpUserRightsOutMessage fp  = null, bool defLockedEnable = false, bool defLockedVisible = false )
        {
            if (m_refMainForm != null && m_refMainForm.InvokeRequired) {
                m_refMainForm.Invoke( new fpScanAccRight( ScanAccRight ), new object[] { curEnableLvl, curVisibleLvl, fp, defLockedEnable, defLockedVisible } );
                return;
            }

            Monitor.Enter( m_hSyncOp );
            try {
                for ( int i = 0; i < _KeepingForms.Count; i++ ) {
                    ScanAccRight( new string[] { _KeepingForms[ i ]._Form.Name, _KeepingForms[ i ]._Form.GetType().FullName }, _KeepingForms[ i ]._Form, _ComponentsAccRights, curEnableLvl, curVisibleLvl, _KeepingForms[ i ]._Form, fp, defLockedEnable, defLockedVisible );
                    ScanTabControl( _ComponentsAccRights, curEnableLvl, curVisibleLvl, fp );
                }
            }finally {
                Monitor.Exit( m_hSyncOp );
            }
        }

        public void ListAllFormControls( fpUserRightsOutMessage fp)
        {
            for(int i = 0; i < _KeepingForms.Count; i++ ) {
                ListAllFormControls( _KeepingForms[ i ]._Form, fp );
            }
        }

        public void ListAllFormControls(Control ctrl, fpUserRightsOutMessage fp,  int layerCnt = 0)
        {
            if (ctrl == null) {
                return;
            }
            StringBuilder sb = new StringBuilder();
            if (layerCnt > 0)
                sb.Append( ' ', layerCnt * 3 );
            if (fp != null) {
                fp( String.Format( "{0}Name = {1}, Type = {2}", sb.ToString(), ctrl.Name, ctrl.GetType().ToString() ) );
            }
            if (ctrl.Controls != null) {
                foreach(Control c in ctrl.Controls ) {
                    if (c != null) {
                        ListAllFormControls( c, fp, layerCnt + 1 );
                    }
                }
            }
        }
        private void ScanAccRight( string[] owner, Control rootOwner, List<ComponentAccRight> ctrlCompo, int curEnableLvl, int curVisibleLvl, Control ctrl, fpUserRightsOutMessage fp = null, bool defLockedEnable = false, bool defLockedVisible = false )
        {
            if ( ctrlCompo == null || ctrlCompo.Count <= 0 )
                return;

            if ( ctrl == null || ctrl.IsDisposed || ctrl.Disposing )
                return;

            // process current form control
            bool got = false;
            bool bEnable = true;
            bool bVisible = true;
            for ( int i = 0; i < ctrlCompo.Count; i++ ) {
                if ( ctrlCompo[ i ]._strName == ctrl.Name && owner.Contains(ctrlCompo[ i ]._strOwner) ) {
                    got = true;
                    bVisible = ctrlCompo[ i ]._nVisibleLevel < 0 ? true : curVisibleLvl <= ctrlCompo[ i ]._nVisibleLevel;
                    bEnable = ctrlCompo[ i ]._nEnableLevel < 0 ? true : curEnableLvl <= ctrlCompo[ i ]._nEnableLevel;
                    bEnable = !bVisible ? false : bEnable; // if not visible, disable it
                    //ctrl.Visible = bVisible;
                    //ctrl.Enabled = bEnable;
                }
            }
            if ( !got ) {
                Form frm = ctrl as Form;
                if ( frm != null ) {
                    bVisible = frm.Visible;
                    bEnable = frm.Enabled;
                } else {
                bVisible = defLockedVisible ? false : true;
                bEnable = defLockedVisible ? false : ( defLockedEnable ? false : true );
                }
            }

            if ( !bVisible || !bEnable ) {
                if ( fp != null )
                    fp( String.Format("[{0}] visible = {1}, enabled = {2}", ctrl.Name, bVisible.ToString(), bEnable.ToString()) );
            }

            TabPage tb = ctrl as TabPage;
            if (tb != null) {
                if (!bVisible) {
                    KeepingPageCtrl keep = new KeepingPageCtrl();
                    keep._Parent = tb.Parent;
                    keep._tbCtrl = tb;
                    keep._tbIndex = tb.TabIndex;
                    tb.Parent = null;
                    keep._strOwner = owner;
                    keep._OwnerForm = rootOwner;

                    if ( m_refMM != null ) m_refMM.RegistryControl( rootOwner, tb );

                    _KeepingTabPageCtrl.Add( keep );
                }
            }
            
            ctrl.Visible = bVisible;
            ctrl.Enabled = bEnable;

            // has subcontrols
            if ( ctrl.Controls != null ) {
                foreach ( Control c in ctrl.Controls ) {
                    ScanAccRight( owner, rootOwner, ctrlCompo, curEnableLvl, curVisibleLvl, c, fp, defLockedEnable, defLockedVisible );
                }
            }

            MenuStrip ms = ctrl as MenuStrip;
            if ( ms != null )
                ScanMenuStrip( ms, ctrlCompo, curEnableLvl, curVisibleLvl, fp, defLockedEnable, defLockedVisible );

        }
        private void ScanTabControl(List<ComponentAccRight> ctrlCompo, int curEnableLvl, int curVisibleLvl, fpUserRightsOutMessage fp = null)
        {
            if ( ctrlCompo == null || ctrlCompo.Count <= 0 )
                return;

            for (int j =0; j < ctrlCompo.Count; j++ ) {
                ComponentAccRight compDesc = ctrlCompo[ j ];
                for (int i =0; i < _KeepingTabPageCtrl.Count; i++ ) {
                    KeepingPageCtrl pgCtrl = _KeepingTabPageCtrl[ i ];
                    if ( pgCtrl != null && pgCtrl._strOwner != null && pgCtrl._strOwner.Contains( compDesc._strOwner) && compDesc._strName == pgCtrl._tbCtrl.Name) {
                        bool bEnable, bVisible;
                        bVisible = curVisibleLvl <= compDesc._nVisibleLevel ? true : false;
                        bEnable = curEnableLvl <= compDesc._nEnableLevel ? true : false;
                        bEnable = !bVisible ? false : bEnable;
                        if (bVisible) {
                            TabControl tbCtrl = pgCtrl._Parent as TabControl;
                            if (pgCtrl._tbIndex < tbCtrl.TabPages.Count) {
                                tbCtrl.TabPages.Insert( pgCtrl._tbIndex, pgCtrl._tbCtrl );
                            } else {
                                tbCtrl.TabPages.Add( pgCtrl._tbCtrl );
                            }
                            if(fp != null) {
                                fp( String.Format( "[{0}] put it back!", pgCtrl._tbCtrl.Name ) );
                            }
                            if ( m_refMM != null ) m_refMM.UnregistryControl( tbCtrl );
                            _KeepingTabPageCtrl.RemoveAt( i );
                        }
                        pgCtrl._tbCtrl.Enabled = bEnable;
                        pgCtrl._tbCtrl.Visible = bVisible;
                        break;
                    }
                }
            }
        }
        private void ScanMenuItem( ToolStripItem item, List<ComponentAccRight> ctrlCompo, int curEnableLvl, int curVisibleLvl, fpUserRightsOutMessage fp, bool defLockedEnable, bool defLockedVisible )
        {
            if ( item == null )
                return;

            ToolStripMenuItem mi = item as ToolStripMenuItem;

            bool got = false;
            bool bEnable = true;
            bool bVisible = true;
            for ( int i = 0; i < ctrlCompo.Count; i++ ) {
                if ( ctrlCompo[ i ]._strName == item.Name ) {
                    got = true;
                    bVisible = ctrlCompo[ i ]._nVisibleLevel < 0 ? true : curVisibleLvl <= ctrlCompo[ i ]._nVisibleLevel;
                    bEnable = ctrlCompo[ i ]._nEnableLevel < 0 ? true : curEnableLvl <= ctrlCompo[ i ]._nEnableLevel;
                    bEnable = !bVisible ? false : bEnable; // if not visible, disable it
                }
            }
            if ( !got ) {
                bVisible = defLockedVisible ? false : true;
                bEnable = defLockedVisible ? false : ( defLockedEnable ? false : true );
            }

            if ( !bVisible || !bEnable ) {
                if ( fp != null )
                    fp( String.Format( "[{0}] visible = {1}, enabled = {2}", item.Name, bVisible.ToString(), bEnable.ToString() ) );
            }
            item.Enabled = bEnable;
            item.Visible = bVisible;

            if (mi != null && mi.DropDownItems != null && mi.DropDownItems.Count > 0) {
                for ( int i = 0; i < mi.DropDownItems.Count; i++ )
                    ScanMenuItem( mi.DropDownItems[ i ], ctrlCompo, curEnableLvl, curVisibleLvl, fp, defLockedEnable, defLockedVisible );
            }
        }
        private void ScanMenuStrip( MenuStrip ms, List<ComponentAccRight> ctrlCompo, int curEnableLvl, int curVisibleLvl, fpUserRightsOutMessage fp = null, bool defLockedEnable = false, bool defLockedVisible = false )
        {
            if ( ms == null || ctrlCompo == null )
                return;

            if (ms.Items != null && ms.Items.Count > 0) {
                for ( int i = 0; i < ms.Items.Count; i++ )
                    ScanMenuItem( ms.Items[ i ], ctrlCompo, curEnableLvl, curVisibleLvl, fp, defLockedEnable, defLockedVisible );
            }
        }

        public void ClearComponentAccRights()
        {
            _ComponentsAccRights.Clear();
        }

        public bool ReadUiControl(string path)
        {
            IniReaderUtility util = new IniReaderUtility();
            if ( !util.Parsing( path ) )
                return false;

            string[] sections = util.GetSections(); // as owner
            if ( sections == null || sections.Length <= 0 )
                return false;

            //_ComponentsAccRights.Clear();
            for (int i = 0; i < sections.Length; i++ ) {
                SectionDataOfIni dat = util.Get( sections[ i ] );
                if ( dat == null || dat.Data == null || dat.Data.Count <= 0 )
                    continue;

                for(int j = 0; j < dat.Data.Count; j++ ) {
                    if ( String.IsNullOrEmpty( dat.Data[ j ].Key ) || dat.Data[ j ].Values.Length < 2 )
                        continue;

                    int enlvl = 0, vslvl = 0;
                    try {
                        enlvl = Convert.ToInt32( dat.Data[ j ].Values[ 0 ].Trim() );
                        vslvl = Convert.ToInt32( dat.Data[ j ].Values[ 1 ].Trim() );
                    }catch { continue; }

                    _ComponentsAccRights.Add( new ComponentAccRight( String.Copy( sections[ i ] ), String.Copy( dat.Data[ j ].Key ), enlvl, vslvl ) );
                }
            }

            return true;
        }
        public bool WriteUiControl( string path )
        {
            // owner as form name or typeof full name 
            // [Owner] 
            // name = enable level, visible level
            // ...

            try {
                using ( Stream ws = File.Open( path, FileMode.Create, FileAccess.ReadWrite ) ) {

                    Dictionary<string, List<ComponentAccRight>> mrg = new Dictionary<string, List<ComponentAccRight>>();

                    for(int i = 0; i < _ComponentsAccRights.Count; i++ ) {
                        if ( String.IsNullOrEmpty( _ComponentsAccRights[ i ]._strOwner ) )
                            continue;
                        List<ComponentAccRight> dat = null;
                        if (!mrg.ContainsKey(_ComponentsAccRights[i]._strOwner)) {
                            dat = new List<ComponentAccRight>();
                            mrg[ _ComponentsAccRights[ i ]._strOwner ] = dat;
                        } else {
                            dat = mrg[ _ComponentsAccRights[ i ]._strOwner ];
                        }
                        if ( dat == null )
                            continue;

                        dat.Add( _ComponentsAccRights[ i ] );
                    }

                    foreach(KeyValuePair<string, List<ComponentAccRight>> kv in mrg) {
                        // write section
                        byte[] line = Encoding.UTF8.GetBytes( String.Format( "[{0}]\n", kv.Key ) );
                        ws.Write( line, 0, line.Length );
                        // write each data
                        for (int i = 0; i < kv.Value.Count; i++ ) {
                            line = Encoding.UTF8.GetBytes( String.Format( "{0}= {1}, {2}\n", kv.Value[ i ]._strName, kv.Value[ i ]._nEnableLevel, kv.Value[ i ]._nVisibleLevel ) );
                            ws.Write( line, 0, line.Length );
                        }
                        line = Encoding.UTF8.GetBytes( "\n" );
                        ws.Write( line, 0, line.Length );
                    }

                    ws.Flush();
                    ws.Close();
                }
            } catch { return false; }

            return false;
        }

        private static void WriteIterOfMenuItem( ToolStripItem itm, StreamWriter ws)
        {
            if ( itm == null || ws == null )
                return;
            ToolStripMenuItem mi = itm as ToolStripMenuItem;
            if (!String.IsNullOrEmpty(itm.Name))
                ws.WriteLine( "{0}=Enable Level, Visible Level", itm.Name );

            if (mi != null && mi.DropDownItems != null && mi.DropDownItems.Count > 0) {
                for ( int i = 0; i < mi.DropDownItems.Count; i++ )
                    WriteIterOfMenuItem( mi.DropDownItems[ i ], ws );
            }
        }
        private static void WriteIterOfCtrl(Control ctrl, StreamWriter ws)
        {
            if ( ctrl == null || ws == null )
                return;

            if ( !String.IsNullOrEmpty( ctrl.Name ) )
                ws.WriteLine( "{0}=Enable Level, Visible Level", ctrl.Name );

            if (ctrl.Controls != null && ctrl.Controls.Count > 0) {
                for ( int i = 0; i < ctrl.Controls.Count; i++ )
                    WriteIterOfCtrl( ctrl.Controls[ i ], ws );
            }

            if (ctrl is MenuStrip) {
                MenuStrip ms = ctrl as MenuStrip;
                for ( int i = 0; i < ms.Items.Count; i++ )
                    WriteIterOfMenuItem( ms.Items[ i ], ws );
            }
        }
        public static void WriteAll(Control ctrl, string filePath)
        {
            if ( ctrl == null )
                return;
            try {
                using ( Stream ws = File.Open( filePath, FileMode.Create ) ) {
                    StreamWriter w = new StreamWriter( ws, Encoding.ASCII );

                    w.WriteLine( "[{0}]", ctrl.GetType().FullName );
                    WriteIterOfCtrl( ctrl, w );

                    w.Flush();
                    w.Close();
                }
            } catch {
            }
        }
        public static void WriteAll(string typeFullName, string filePath)
        {
            Type tp = PluginAssemblies.GetTypeByFullName( typeFullName );
            if ( tp == null || String.IsNullOrEmpty( filePath ) )
                return;

            Object o = null;
            try { o = Activator.CreateInstance( tp ); } catch { return; }
            Control c = o as Control;

            if ( c == null ) {
                IDisposable disp = o as IDisposable;
                if ( disp != null ) {
                    disp.Dispose();
                }
                return;
            }

            try {
                using(Stream ws = File.Open(filePath, FileMode.Create)) {
                    StreamWriter w = new StreamWriter( ws, Encoding.ASCII );

                    w.WriteLine( "[{0}]", c.GetType().FullName );
                    WriteIterOfCtrl( c, w );

                    w.Flush();
                    w.Close();
                }
            } catch {
                c.Dispose();
                return;
            }

            c.Dispose();
            c = null;
            o = null;

        }
    }
}
