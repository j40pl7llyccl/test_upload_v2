using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using uIP.Lib;

namespace uIP.Lib.UsrControl
{
    /// <summary>
    /// File base users and group manager
    /// - Must supply the folder to RW
    /// - Check on loading the users and groups file to do
    /// - Provide a simple UI to edit the users
    ///
    /// Group file format
    /// [available_groups]
    /// (group ID: int)= (given name: string), (enable right: int), (visible right: int)
    /// ...
    /// [default_group]
    /// (group ID: int)= "name of group" // a user not have the group
    /// [right_to_new_group]
    /// (group ID: int)= "name of group" // authorized group right to create a new group, only can create higher right than group
    /// [right_to_new_user]
    /// (group ID: int) = "name of group" // authorized group right to create a new user, only can create higher right than user 
    /// 
    /// Two kinds of ini user file formats
    /// - user description
    ///   - [users]
    ///     "name of user(unique)"="pwd", (group:int), (inher Enable from which group: int), (inher Visible from which group: int)
    ///     ...
    ///
    /// - bulk user add/ remove
    ///   - [add]
    ///     "name of user"="pwd", (group: int)
    ///     ...
    ///   - [remove]
    ///     "name of user"= remove
    ///
    /// </summary>
    public class UserDataManager : IUserManagement, IDisposable
    {
        protected const string _strGroupFileName = "GroupDescriptions.ini";
        protected const string _strUsersFileName = "Users.ini";

        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;
        protected string m_strUserDatIoPath = null;
        protected string m_strWorkingGroupFilePath = null;
        protected string m_strWorkingUsersFilePath = null;
        protected fpLogMessage m_fpLog = null;
        protected object m_hSync = new object();
        protected UsersGuiControl _GuiAcl = null;
        protected IMultilanguageManager _MultiLang = null;

        public UsersGuiControl GuiAcl {  get { return _GuiAcl; } set { _GuiAcl = value; } }
        public IMultilanguageManager MultiLang {  get { return _MultiLang; } set { _MultiLang = value; } }

        public UserDataManager( fpLogMessage log, string path )
        {
            if ( !Directory.Exists( path ) ) {
                return;
            }

            m_fpLog = log;
            m_strUserDatIoPath = String.Copy( Path.GetFullPath( path ) );
            m_strWorkingGroupFilePath = Path.Combine( m_strUserDatIoPath, _strGroupFileName );
            m_strWorkingUsersFilePath = Path.Combine( m_strUserDatIoPath, _strUsersFileName );
        }

        public void Dispose()
        {
            if ( m_bDisposing || m_bDisposed ) return;
            m_bDisposing = true;

            _GuiAcl = null;

            m_LoginCallbacks.Clear();

            m_bDisposed = true;
            m_bDisposing = false;
        }

        public void OpenEditor()
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            UsersData dat = new UsersData( m_fpLog, m_strWorkingGroupFilePath, m_strWorkingUsersFilePath );
            frmEditUser editor = new frmEditUser( dat )
            {
                AsDiaolg = true,
                DefaultUserFilePath = m_strWorkingUsersFilePath
            };

            editor.ShowDialog();
            editor.Dispose();
            editor = null;

            dat = null;
        }

        // With UI
        public bool UserLogin( out UserBasicInfo currUser, out int nCurrEnabledLevel, out int nCurrVisibleLevel , out Dictionary<int, string> groups )
        {
            currUser = null;
            nCurrEnabledLevel = int.MaxValue;
            nCurrVisibleLevel = int.MaxValue;
            groups = null;

            if ( m_bDisposing || m_bDisposed ) return false;

            bool status = false;
            UsersData dat = new UsersData( m_fpLog, m_strWorkingGroupFilePath, m_strWorkingUsersFilePath );
            frmUserLogin login = new frmUserLogin( dat.Users );
            if (login.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (login.IsLogin) {
                    status = true;
                    currUser = new UserBasicInfo( login.User.Name, login.User.Pwd, login.User.Group );
                    nCurrEnabledLevel = login.User.EnabledLevel;
                    nCurrVisibleLevel = login.User.VisibleLevel;
                    groups = new Dictionary<int, string>();
                    foreach(KeyValuePair<int, UserGroup>kv in dat.Groups) {
                        groups.Add( kv.Key, String.IsNullOrEmpty( kv.Value._strName ) ? "" : String.Copy( kv.Value._strName ) );
                    }
                }
            }

            login.Dispose();
            login = null;

            if ( status ) {
                Monitor.Enter( m_hSync );
                try {
                    // config login user
                    m_nCurrEnabledLevel = nCurrEnabledLevel;
                    m_nCurrVisibleLevel = nCurrVisibleLevel;
                    m_LoginUser = currUser;
                    // callback to notify
                    for(int i= 0;i < m_LoginCallbacks.Count;  i++) {
                        m_LoginCallbacks[ i ]( true, m_LoginUser, m_nCurrEnabledLevel, m_nCurrVisibleLevel );
                    }
                } finally {
                    Monitor.Exit( m_hSync );
                }
            }

            return status;
        }

        // No UI
        public bool UserLogin( string userName, string pwd, out int group, out int nCurrEnabledLevel, out int nCurrVisibleLevel, out Dictionary<int, string> groups )
        {
            group = -1;
            nCurrEnabledLevel = int.MaxValue;
            nCurrVisibleLevel = int.MaxValue;
            groups = null;
            if ( m_bDisposing || m_bDisposed )
                return false;

            bool ret = false;
            Monitor.Enter( m_hSync );
            try { ret = PrivUserLogin( userName, pwd, out group, out nCurrEnabledLevel, out nCurrVisibleLevel, out groups ); }
            finally { Monitor.Exit( m_hSync ); }
            return ret;

        }
        private bool PrivUserLogin(string userName, string pwd, out int group, out int nCurrEnabledLevel, out int nCurrVisibleLevel, out Dictionary<int, string> groups )
        {
            group = -1;
            nCurrEnabledLevel = int.MaxValue;
            nCurrVisibleLevel = int.MaxValue;
            groups = null;

            if ( m_bDisposing || m_bDisposed ) return false;

            UsersData dat = new UsersData( m_fpLog, m_strWorkingGroupFilePath, m_strWorkingUsersFilePath );

            if ( !dat.Users.ContainsKey( userName ) )
                return false;
            if ( dat.Users[ userName ].Pwd == pwd )
                return false;

            group = dat.Users[ userName ].Group;
            nCurrEnabledLevel = dat.Users[ userName ].EnabledLevel;
            nCurrVisibleLevel = dat.Users[ userName ].VisibleLevel;
            groups = new Dictionary<int, string>();
            foreach ( KeyValuePair<int, UserGroup> kv in dat.Groups ) {
                groups.Add( kv.Key, String.IsNullOrEmpty( kv.Value._strName ) ? "" : String.Copy( kv.Value._strName ) );
            }

            // config the login user data
            m_nCurrEnabledLevel = nCurrEnabledLevel;
            m_nCurrVisibleLevel = nCurrVisibleLevel;
            m_LoginUser = new UserBasicInfo(String.Copy(userName), String.Copy(pwd), group);
            // callback to reg delegate
            for(int i = 0; i < m_LoginCallbacks.Count; i++ ) {
                m_LoginCallbacks[ i ]( true, m_LoginUser, m_nCurrEnabledLevel, m_nCurrVisibleLevel );
            }

            return true;
        }

        protected UserBasicInfo m_LoginUser = null;
        protected int m_nCurrEnabledLevel = -1;
        protected int m_nCurrVisibleLevel = -1;
        protected List<fpUserLoginCall> m_LoginCallbacks = new List<fpUserLoginCall>();

        public UserBasicInfo LoginedUser { get { return m_LoginUser; } }
        public int CurrentEnabledLevel { get { return m_nCurrEnabledLevel; } }
        public int CurrentVisibleLevel { get { return m_nCurrVisibleLevel; } }
        public bool Login( string userName, string pwd )
        {
            if ( m_bDisposing || m_bDisposed )
                return false;

            bool ret = false;

            Monitor.Enter( m_hSync );
            try {
                int group , curEnabled, curVisible;
                Dictionary<int, string> gps;
                ret = PrivUserLogin( userName, pwd, out group, out curEnabled, out curVisible, out gps );
            } finally {
                Monitor.Exit( m_hSync );
            }
            return ret;
        }
        public bool LoginWithUI()
        {
            if ( m_bDisposing || m_bDisposed )
                return false;

            UserControlData usr;
            UsersData dat = new UsersData( m_fpLog, m_strWorkingGroupFilePath, m_strWorkingUsersFilePath );
            dat.GuiAcl = _GuiAcl;
            dat.MultiLang = _MultiLang;
            if ( !dat.PopupLogin( out usr ) )
                return false;

            Monitor.Enter( m_hSync );
            try {

                m_LoginUser = new UserBasicInfo( usr.Name, usr.Pwd, usr.Group );
                m_nCurrEnabledLevel = usr.EnabledLevel;
                m_nCurrVisibleLevel = usr.VisibleLevel;
                // callback to reg delegate
                for ( int i = 0; i < m_LoginCallbacks.Count; i++ ) {
                    m_LoginCallbacks[ i ]( true, m_LoginUser, m_nCurrEnabledLevel, m_nCurrVisibleLevel );
                }
            } finally { Monitor.Exit( m_hSync ); }

            return true;
        }
        public bool ChangePwdWithUI()
        {
            if ( m_bDisposing || m_bDisposed )
                return false;

            if ( m_LoginUser == null || String.IsNullOrEmpty( m_LoginUser.Name ) )
                return false;

            UsersData dat = new UsersData( m_fpLog, m_strWorkingGroupFilePath, m_strWorkingUsersFilePath );
            if ( !dat.Users.ContainsKey( m_LoginUser.Name ) )
                return false;

            frmChangePwd dlg = null;
            if (_GuiAcl != null) {
                dlg = ( frmChangePwd ) _GuiAcl.CreateShowDialog<frmChangePwd>( m_LoginUser.InherEnabledGroup, m_LoginUser.InherVisibleGroup );
                dlg.UserData = dat.Users[ m_LoginUser.Name ];
                dlg.Visible = false;
            } else {
                dlg = new frmChangePwd( dat.Users[ m_LoginUser.Name ], true );
            }
            if (dlg.ShowDialog() == DialogResult.OK) {
                if ( dlg.Succ ) {
                dat.WriteUsers( m_strWorkingUsersFilePath );
                    m_LoginUser.Pwd = dat.Users[ m_LoginUser.Name ].Pwd;
                } else {
                    MessageBox.Show( dlg.ErrorString, "", MessageBoxButtons.OK, MessageBoxIcon.Error );
                }
            }
            dlg.Dispose();
            dlg = null;

            return true;
        }
        public void Logout()
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            // config the login user data
            m_nCurrEnabledLevel = Int32.MaxValue;
            m_nCurrVisibleLevel = Int32.MaxValue;
            m_LoginUser = new UserBasicInfo( "", "", Int32.MaxValue );
            // callback to reg delegate
            for ( int i = 0; i < m_LoginCallbacks.Count; i++ ) {
                m_LoginCallbacks[ i ]( false, m_LoginUser, m_nCurrEnabledLevel, m_nCurrVisibleLevel );
            }
        }
        public void LogoutWithUI()
        {
            if ( m_bDisposing || m_bDisposed )
                return;

            string text = "Logout?";
            string caption = "Warning";
            if (_MultiLang != null) {
                if ( _MultiLang.StringMapping != null && _MultiLang.StringMapping.ContainsKey( "Logout" ) )
                    text = String.Format( "{0}?", _MultiLang.StringMapping[ "Logout" ] );
                if ( _MultiLang.StringMapping != null && _MultiLang.StringMapping.ContainsKey( "Warning" ) )
                    caption = _MultiLang.StringMapping[ "Warning" ];
            }

            if ( MessageBox.Show(text, caption, MessageBoxButtons.OKCancel) == DialogResult.OK) {
                Logout();
            }
        }
        public void InstallCallback( fpUserLoginCall func )
        {
            if ( m_bDisposed || m_bDisposing )
                return;
            Monitor.Enter( m_hSync );
            try {
                if ( !m_LoginCallbacks.Contains( func ) )
                    m_LoginCallbacks.Add( func );
            } finally {
                Monitor.Exit( m_hSync );
            }
        }
        public void RemoveCallback( fpUserLoginCall func )
        {
            if ( m_bDisposed || m_bDisposing )
                return;
            Monitor.Enter( m_hSync );
            try {
                if ( !m_LoginCallbacks.Contains( func ) )
                    m_LoginCallbacks.Remove( func );
            } finally {
                Monitor.Exit( m_hSync );
            }
        }

        public UsersData NewOne()
        {
            if ( m_bDisposing || m_bDisposed )
                return null;
            UsersData dat = new UsersData( m_fpLog, m_strWorkingGroupFilePath, m_strWorkingUsersFilePath );
            return dat;
        }
    }
}