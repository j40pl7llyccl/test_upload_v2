using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using uIP.Lib;

namespace uIP.Lib.UsrControl
{
    /// <summary>
    /// Load access right after user login
    /// - Folder base to contain the all forms control
    /// - Naming rule: (name of form/ control full type name).ini
    /// </summary>
    public class GUIAccessManager : IGuiAclManagement, IDisposable
    {
        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;

        protected UsersGuiControl m_GUIControl = null;
        protected string m_strGuiDescFilesFolderPath = null;
        protected fpLogMessage m_Log = null;
        protected Form m_refMain = null;
        protected IMultilanguageManager m_refMM = null;
        protected Dictionary<Control, UsersGuiControl> m_MoreGuiCtrl = new Dictionary<Control, UsersGuiControl>();

        public UsersGuiControl GuiAcl {  get { return m_GUIControl; } }
        public String GuiDescFileFolderPath {  get { return m_strGuiDescFilesFolderPath; } }

        public GUIAccessManager( fpLogMessage log, Form frmMain, string pathContainGuiDescFiles, IMultilanguageManager mm)
        {
            m_Log = log;

            if ( Directory.Exists( pathContainGuiDescFiles ) )
                m_strGuiDescFilesFolderPath = String.Copy( pathContainGuiDescFiles );

            m_GUIControl = new UsersGuiControl(frmMain, mm);
            m_refMM = mm;
            m_refMain = frmMain;
        }

        public void Dispose()
        {
            if ( m_bDisposing || m_bDisposed )
                return;
            m_bDisposing = true;

            ( m_GUIControl as IDisposable )?.Dispose();
            m_GUIControl = null;

            // clear more
            foreach(KeyValuePair<Control, UsersGuiControl> kv in m_MoreGuiCtrl) {
                if ( kv.Value == null ) continue;
                kv.Value.Dispose();
            }

            m_MoreGuiCtrl.Clear();

            m_bDisposed = true;
            m_bDisposing = false;
        }

        private void ConvLog(string str)
        {
            if ( m_Log == null ) return;
            m_Log(eLogMessageType.NORMAL, 0, String.Format( "[GUIAccessManager] {0}", str ) );
        }

        //private delegate Form fpCreateForm<T>( bool bHandle );
        public Form CreateForm<T>( bool bHandle = true )
        {
            if ( m_bDisposing || m_bDisposed )
                return null;

            Form ret = null;
            //if (m_refMainForm != null && m_refMainForm.InvokeRequired) {
            //    ret = m_refMainForm.Invoke( new fpCreateForm<T>( m_GUIControl.CreateForm<T> ), new object[] { bHandle } ) as Form;
            //} else {
                ret = m_GUIControl.CreateForm<T>( bHandle );
            //}

            return ret;
        }

        //private delegate bool fpAddForm( Form frm, bool bHandle );
        public bool AddForm( Form frm, bool bHandle = true )
        {
            if ( m_bDisposing || m_bDisposed )
                return false;

            bool ret = false;
            //if (m_refMainForm != null && m_refMainForm.InvokeRequired) {
            //    object r = m_refMainForm.Invoke( new fpAddForm( m_GUIControl.AddForm ), new object[] { frm, bHandle } );
            //    ret = ( bool ) r;
            //} else {
                ret = m_GUIControl.AddForm( frm, bHandle );
            //}

            return ret;
        }

        //private delegate void fpRmvForm( Form frm, bool bBySettingHandleFormInst );
        public void RmvForm( Form frm, bool bBySettingHandleFormInst = true )
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            //if ( m_refMainForm != null && m_refMainForm.InvokeRequired ) {
            //    m_refMainForm.Invoke( new fpRmvForm( m_GUIControl.RemoveForm ), new object[] { frm, bBySettingHandleFormInst } );
            //} else {
                m_GUIControl.RemoveForm( frm, bBySettingHandleFormInst );
            //}
        }
        public Form CreateShowDialog<T>( int nEnabledLvl, int nVisibleLvl )
        {
            if ( m_bDisposing || m_bDisposed )
                return null;

            return m_GUIControl.CreateShowDialog<T>( nEnabledLvl, nVisibleLvl );
        }

        protected void LoadingSetting()
        {
            if ( !Directory.Exists( m_strGuiDescFilesFolderPath ) ) {
                if ( m_Log != null ) m_Log(eLogMessageType.ERROR, 100, String.Format( "[GUIAccessManager::LoadingSetting] dir {0} not exist!", m_strGuiDescFilesFolderPath ) );
                return;
            }

            // clear first
            m_GUIControl.ClearComponentAccRights();
            // read from files
            string[] files = Directory.GetFiles( m_strGuiDescFilesFolderPath, "*.*", SearchOption.TopDirectoryOnly );
            for (int i = 0; i < files.Length; i++ ) {
                m_GUIControl.ReadUiControl( files[ i ] );
            }
        }

        public void ScanAccessRights(int curEnabledLevel, int curVisibleLevel, bool bReloadSettings = false)
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            if ( bReloadSettings )
                LoadingSetting();

            m_GUIControl.ScanAccRight( curEnabledLevel, curVisibleLevel, ConvLog );

            // check more GUI Ctrl
            foreach(KeyValuePair<Control, UsersGuiControl> kv in m_MoreGuiCtrl) {
                kv.Value.ScanAccRight( curEnabledLevel, curVisibleLevel, ConvLog );
            }
        }

        public void UsrLoginAclScan( UserBasicInfo usr, int curEnabledLevel, int curVisibleLevel)
        {
            ScanAccessRights( curEnabledLevel, curVisibleLevel, true );
        }

        public bool NewMoreGuiCtrl(Control root, out UsersGuiControl guiCtrl)
        {
            guiCtrl = null;
            if ( m_bDisposing || m_bDisposed || root == null )
                return false;
            if ( m_MoreGuiCtrl == null ) return false;

            if ( !m_MoreGuiCtrl.ContainsKey( root ) )
                m_MoreGuiCtrl[ root ] = new UsersGuiControl( m_refMain, m_refMM );

            guiCtrl = m_MoreGuiCtrl[ root ];
            return true;
        }
        public void RmvMoreGuiCtrl(Control root)
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            if (m_MoreGuiCtrl.ContainsKey(root)) {
                UsersGuiControl ctrl = m_MoreGuiCtrl[ root ];
                m_MoreGuiCtrl.Remove( root );
                ctrl.Dispose();
                ctrl = null;
            }
        }
        public UsersGuiControl GetMoreGuiCtrlByCtrlFullTypeName(string name)
        {
            if ( m_bDisposed || m_bDisposing )
                return null;

            foreach(KeyValuePair<Control, UsersGuiControl> kv in m_MoreGuiCtrl) {
                if ( kv.Key == null ) continue;
                if (kv.Key.GetType().FullName == name) {
                    return kv.Value;
                }
            }
            return null;
        }
        public UsersGuiControl GetMoreGuiCtrlByCtrlName(string name)
        {
            if ( m_bDisposed || m_bDisposing )
                return null;

            foreach ( KeyValuePair<Control, UsersGuiControl> kv in m_MoreGuiCtrl ) {
                if ( kv.Key == null ) continue;
                if ( kv.Key.Name == name ) {
                    return kv.Value;
                }
            }
            return null;
        }
    }
}
