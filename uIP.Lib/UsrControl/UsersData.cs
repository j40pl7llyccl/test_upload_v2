using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using uIP.Lib.Utility;

namespace uIP.Lib.UsrControl
{
    /// <summary>
    /// Handle user acl by using file; can replace by database
    /// - RW from ini file
    /// - user right create/ remove
    /// - verify user
    /// </summary>
    public class UsersData
    {
        private const string _strSEC_AvailableGroups = "available_groups";
        private const string _strSEC_DefaultUsrGroup = "default_group";
        private const string _strSEC_RightToNewGroup = "right_to_new_group";
        private const string _strSEC_RightToNewUser = "right_to_new_user";
        private const string _strSEC_UserDat = "users";
        private const string _strSEC_MGR_USR_ADD = "add";
        private const string _strSEC_MGR_USR_RMV = "remove";
        // output default group
        protected bool m_bSettingFileMustEnc = false;
        protected int _nDefaultGroup = 0;
        protected int _nDefaultEnabledRight = 0;
        protected int _nDefaultVisibleRight = 0;
        protected int _nAtLeaseRightToCreateUser = 0;
        protected int _nAtLeaseRightToCreateGroup = 0;
        protected Dictionary<int, UserGroupAccessRight> _GroupRights = new Dictionary<int, UserGroupAccessRight>();
        protected Dictionary<int, UserGroup> _Groups = new Dictionary<int, UserGroup>();
        protected Dictionary<string, UserControlData> _Users = new Dictionary<string, UserControlData>();
        protected fpLogMessage m_fpLog = null;
        protected UsersGuiControl _GuiAcl = null;
        protected IMultilanguageManager _MultiLang = null;

        protected string m_strLatestUpdatedGroupFile = null;
        protected string m_strLatestUpdatedUsersFile = null;

        public string LatestUpdatedGroupFilePath {  get { return m_strLatestUpdatedGroupFile; } }
        public string LatestUpdatedUsersFilePath {  get { return m_strLatestUpdatedUsersFile; } }

        public Dictionary<int, UserGroupAccessRight> GroupRights { get { return _GroupRights; } }
        public Dictionary<string, UserControlData> Users { get { return _Users; } }
        public Dictionary<int, UserGroup> Groups {  get { return _Groups; } }
        public UsersGuiControl GuiAcl {  get { return _GuiAcl; } set { _GuiAcl = value; } }
        public IMultilanguageManager MultiLang {  get { return _MultiLang; } set { _MultiLang = value; } }

        private static void DefaultGroup(Dictionary<int, UserGroupAccessRight> g)
        {
            if ( g == null ) return;
            g[ 0 ] = new UserGroupAccessRight( "Sys Admin", 0, 0 );
            g[ 50 ] = new UserGroupAccessRight( "AE", 50, 50 );
            g[ 100 ] = new UserGroupAccessRight( "Engineer", 100, 100 );
            g[ 150 ] = new UserGroupAccessRight( "Foreman", 150, 150 );
            g[ 200 ] = new UserGroupAccessRight( "Operator", 200, 200 );
        }
        private static void DefaultUser( Dictionary<int, UserGroupAccessRight> g, Dictionary<string, UserControlData> u)
        {
            if ( u == null ) return;
            u[ "sys_admin" ] = new UserControlData( g, "sys_admin", "sys_admin", 0 );
        }

        private static void AddUserToGroup( Dictionary<int, UserGroup> ug, UserControlData usr )
        {
            if ( ug == null || usr == null )
                return;

            UserGroup ugroup = null;
            if ( !ug.ContainsKey( usr.Group ) ) {
                UserGroupAccessRight r;
                if ( usr.GetGroup( usr.Group, out r ) ) {
                    ugroup = new UserGroup();
                    ugroup._nGroup = usr.Group;
                    ugroup._strName = r._strNameOfGroup;
                    ug[ usr.Group ] = ugroup;
                }
            } else {
                ugroup = ug[ usr.Group ];
            }
            if ( ugroup == null )
                return;

            if ( !ugroup._Users.ContainsKey( usr.Name ) )
                ugroup._Users[ usr.Name ] = usr;
        }
        private static void RmvUsrFromGroup( Dictionary<int, UserGroup> ug, UserControlData usr )
        {
            if ( ug == null || usr == null ) return;
            if ( !ug.ContainsKey( usr.Group ) ) return;

            UserGroup group = ug[ usr.Group ];
            if ( group == null || group._Users == null ) return;

            if ( !group._Users.ContainsKey( usr.Name ) ) return;
            group._Users.Remove( usr.Name );
        }
        private static void ConfigUserOfGroup( Dictionary<int, UserGroup> ug, Dictionary<string, UserControlData> u )
        {
            if ( ug == null || u == null ) return;
            ug.Clear();
            foreach(KeyValuePair<string, UserControlData> kv in u) {
                AddUserToGroup( ug, kv.Value );
            }
        }

        public UsersData( fpLogMessage log, bool defEncFiles = true )
        {
            m_fpLog = log;
            DefaultGroup( _GroupRights );
            DefaultUser( _GroupRights, _Users );
            _nAtLeaseRightToCreateUser = 150;
            _nAtLeaseRightToCreateGroup = 100;
            _nDefaultGroup = 200;
            _nDefaultEnabledRight = 200;
            _nDefaultVisibleRight = 200;
            // conf group info
            ConfigUserOfGroup( _Groups, _Users );
        }
        public UsersData( fpLogMessage log, string groupFilePath, string userFilePath, bool defEncFiles = true)
        {
            m_bSettingFileMustEnc = defEncFiles;
            m_fpLog = log;
            // load file first
            if ( File.Exists( groupFilePath ) && File.Exists( userFilePath ) ) {
                ReadGroup( groupFilePath );
                ReadUsers( userFilePath );
            }
            bool bWriteGroupBack = false;
            bool bWriteUsersBack = false;
            // no data, use default
            if ( _GroupRights.Count <= 0 ) {
                DefaultGroup( _GroupRights );
                string pathDirOfGroup = Path.GetDirectoryName( groupFilePath );
                if (Directory.Exists( pathDirOfGroup)) {
                    bWriteGroupBack = true;
                }
            }
            if ( _Users.Count <= 0 ) {
                DefaultUser( _GroupRights, _Users );
                string pathDirOfUsers = Path.GetDirectoryName( userFilePath );
                if (Directory.Exists( pathDirOfUsers ) ) {
                    bWriteUsersBack = true;
                }
            }
            // conf group info
            ConfigUserOfGroup( _Groups, _Users );

            // write back
            if ( bWriteGroupBack ) WriteGroup( groupFilePath );
            if ( bWriteUsersBack ) WriteUsers( userFilePath );
        }
        public bool EncFiles { get { return m_bSettingFileMustEnc; } set { m_bSettingFileMustEnc = value; } }

        virtual public bool WriteGroup(Stream ws)
        {
            // [available group]
            // 0 -> sys admin, < 0 -> NA, > 0 -> user defined
            // <group ID: int>=<given name: string>, <enable right: int>, <visible right: int>
            // ...
            // 50 -> AE
            // 100 -> Customer Engineer
            // 150 -> Foreman
            // 200 -> OP
            // [default group]
            // OP
            // [right to new user]
            // Foreman
            // [right to new group]
            // Customer Engineer

            try
            {
                // available group
                byte[] dat = Encoding.UTF8.GetBytes( String.Format( "[{0}]\n", _strSEC_AvailableGroups ) );
                ws.Write( dat, 0, dat.Length );
                foreach ( KeyValuePair<int, UserGroupAccessRight> kv in _GroupRights )
                {
                    dat = Encoding.UTF8.GetBytes( String.Format( "{0}=\"{1}\", {2}, {3}\n", kv.Key, kv.Value._strNameOfGroup, kv.Value._nEnabledLevel, kv.Value._nVisibleLevel ) );
                    ws.Write( dat, 0, dat.Length );
                }
                dat = Encoding.UTF8.GetBytes( "\n" );
                ws.Write( dat, 0, dat.Length );
                // default group
                dat = Encoding.UTF8.GetBytes( String.Format( "[{0}]\n", _strSEC_DefaultUsrGroup ) );
                ws.Write( dat, 0, dat.Length );
                if ( _GroupRights.ContainsKey( _nDefaultGroup ) )
                {
                    dat = Encoding.UTF8.GetBytes( String.Format( "{0}= {1}\n", _nDefaultGroup, _GroupRights[ _nDefaultGroup ]._strNameOfGroup ) );
                    ws.Write( dat, 0, dat.Length );
                }
                dat = Encoding.UTF8.GetBytes( "\n" );
                ws.Write( dat, 0, dat.Length );
                // right to create user
                dat = Encoding.UTF8.GetBytes( String.Format( "[{0}]\n", _strSEC_RightToNewUser ) );
                ws.Write( dat, 0, dat.Length );
                if ( _GroupRights.ContainsKey( _nAtLeaseRightToCreateUser ) )
                {
                    dat = Encoding.UTF8.GetBytes( String.Format( "{0}= {1}\n", _nAtLeaseRightToCreateUser, _GroupRights[ _nAtLeaseRightToCreateUser ]._strNameOfGroup ) );
                    ws.Write( dat, 0, dat.Length );
                }
                // right to create group
                dat = Encoding.UTF8.GetBytes( String.Format( "[{0}]\n", _strSEC_RightToNewGroup ) );
                ws.Write( dat, 0, dat.Length );
                if ( _GroupRights.ContainsKey( _nAtLeaseRightToCreateGroup ) )
                {
                    dat = Encoding.UTF8.GetBytes( String.Format( "{0}= {1}\n", _nAtLeaseRightToCreateGroup, _GroupRights[ _nAtLeaseRightToCreateGroup ]._strNameOfGroup ) );
                    ws.Write( dat, 0, dat.Length );
                }
                dat = Encoding.UTF8.GetBytes( "\n" );
                ws.Write( dat, 0, dat.Length );
            }
            catch ( Exception ex )
            {
                m_fpLog?.Invoke( eLogMessageType.WARNING, 100, $"Write group fail with exception:\n{ex}" );
                return false;
            }
            return true;
        }

        public void WriteGroup( string path )
        {
            var status = true;

            try
            {
                using ( var ws = File.Open( path, FileMode.Create, FileAccess.ReadWrite ) )
                {
                    status = WriteGroup( ws );
                }
            }
            catch
            {
                status = false;
            }

            if (status && m_bSettingFileMustEnc)
                FileEncryptUtility.ENC( path );
        }
        protected static UserGroupAccessRight NewGroup(KeyValues item, out int group)
        {
            group = -1;
            if ( item == null || item.Values.Length < 3 )
                return null;

            int enLvl = -1, vsLvl = -1;
            try { group = Convert.ToInt32( item.Key ); } catch { return null; }
            try { enLvl = Convert.ToInt32( item.Values[ 1 ] ); } catch { return null; }
            try { vsLvl = Convert.ToInt32( item.Values[ 2 ] ); } catch { return null; }
            return new UserGroupAccessRight( item.Values[ 0 ].Trim().Replace( "\"", "" ).Trim(), enLvl, vsLvl );
        }
        virtual public void ReadGroup( string path )
        {
            // check first
            string currWorkingFile = path;
            string tmpRdFile = null;
            if ( m_bSettingFileMustEnc ) {
                if ( FileEncryptUtility.CheckFile( path ) ) {
                    tmpRdFile = Path.Combine( Path.GetDirectoryName( path ), String.Format( "{0}_{1}.ini", Path.GetFileNameWithoutExtension( path ), CommonUtilities.GetCurrentTimeStr() ) );
                    try { File.Copy( path, tmpRdFile, true ); FileEncryptUtility.DEC( tmpRdFile ); currWorkingFile = tmpRdFile; } catch { }
                } else {
                    if ( m_fpLog != null ) m_fpLog(eLogMessageType.ERROR, 100, String.Format( "[{0}::ReadGroup] file({1}) not from enc!", this.GetType().Name, path ) );
                    return;
                }
            }

            IniReaderUtility util = new IniReaderUtility();
            if ( !util.Parsing( currWorkingFile ) ) {
                if ( File.Exists(tmpRdFile)) { try { File.Delete( tmpRdFile ); } catch { } } // remove tmp file
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.WARNING, 100, String.Format( "[{0}::ReadGroup] parse file({1}) error!", this.GetType().Name, path ) );
                return;
            }

            SectionDataOfIni dat = util.Get( _strSEC_AvailableGroups );
            if ( dat == null || dat.Data == null || dat.Data.Count <= 0 ) {
                if ( File.Exists( tmpRdFile ) ) { try { File.Delete( tmpRdFile ); } catch { } } // remove tmp file
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.WARNING, 100, String.Format( "[{0}::ReadGroup] no ini section in file({1}) error!", this.GetType().Name, path ) );
                return;
            }

            // read group right
            _GroupRights.Clear();
            for(int i = 0; i < dat.Data.Count; i++ ) {
                int group = -1;
                UserGroupAccessRight right = NewGroup(dat.Data[i], out group);
                if ( right == null || group < 0 )
                    continue;

                _GroupRights[ group ] = right;
            }
            // read default value
            dat = util.Get( _strSEC_DefaultUsrGroup );
            if (dat == null || dat.Data == null || dat.Data.Count <= 0) {
                _nDefaultGroup = 200;
                _nDefaultEnabledRight = 200;
                _nDefaultVisibleRight = 200;
            } else {
                int group = -1;
                try { group = Convert.ToInt32( dat.Data[ 0 ].Key ); } catch { group = -1; }
                if (group >= 0 && _GroupRights.ContainsKey(group)) {
                    _nDefaultGroup = group;
                    _nDefaultEnabledRight = _GroupRights[ group ]._nEnabledLevel;
                    _nDefaultVisibleRight = _GroupRights[ group ]._nVisibleLevel;
                }
            }

            // read right to create user
            _nAtLeaseRightToCreateUser = 0;
            dat = util.Get( _strSEC_RightToNewUser );
            if (dat != null && dat.Data != null && dat.Data.Count > 0) {
                try { _nAtLeaseRightToCreateUser = Convert.ToInt32( dat.Data[ 0 ].Key ); } catch { _nAtLeaseRightToCreateUser = 0; }
            }

            // read right to create group
            _nAtLeaseRightToCreateGroup = 0;
            dat = util.Get( _strSEC_RightToNewGroup );
            if ( dat != null && dat.Data != null && dat.Data.Count > 0 ) {
                try { _nAtLeaseRightToCreateGroup = Convert.ToInt32( dat.Data[ 0 ].Key ); } catch { _nAtLeaseRightToCreateGroup = 0; }
            }

            if ( File.Exists( tmpRdFile ) ) { try { File.Delete( tmpRdFile ); } catch { } } // remove tmp file
            m_strLatestUpdatedGroupFile = String.Copy( path );
            if ( m_fpLog != null ) m_fpLog(eLogMessageType.NORMAL, 150, String.Format( "[{0}::ReadGroup] read file {1} success!", this.GetType().Name, path ) );
        }

        virtual public bool WriteUsers(Stream ws)
        {
            try
            {
                byte[] dat = Encoding.UTF8.GetBytes( String.Format( "[{0}]\n", _strSEC_UserDat ) );
                ws.Write( dat, 0, dat.Length );

                foreach ( KeyValuePair<string, UserControlData> kv in _Users )
                {
                    string s = kv.Value.GetWriteStr();
                    dat = Encoding.UTF8.GetBytes( s );
                    ws.Write( dat, 0, dat.Length );
                }

                dat = Encoding.UTF8.GetBytes( "\n" );
                ws.Write( dat, 0, dat.Length );

            }
            catch ( Exception ex )
            {
                m_fpLog?.Invoke( eLogMessageType.WARNING, 100, $"Write users with error:\n{ex}" );
                return false;
            }

            return true;
        }

        virtual public void WriteUsers( string path )
        {
            var status = true;
            try {
                using ( Stream ws = File.Open( path, FileMode.Create, FileAccess.ReadWrite ) ) {
                    status = WriteUsers( ws );
                    ws.Flush();
                    ws.Close();
                }
            } catch { status = false; }

            if ( status && m_bSettingFileMustEnc )
                FileEncryptUtility.ENC( path );
        }

        protected static UserControlData NewUser(KeyValues kv, Dictionary<int, UserGroupAccessRight> g)
        {
            if ( kv == null ) return null;

            if ( String.IsNullOrEmpty( kv.Key ) || kv.Values == null || kv.Values.Length < 2 )
                return null;

            string name = kv.Key.Trim().Replace( "\"", "" ).Trim();
            if ( String.IsNullOrEmpty( name ) )
                return null;

            string pwd = String.IsNullOrEmpty( kv.Values[ 0 ] ) ? "" : kv.Values[ 0 ].Trim().Replace( "\"", "" );

            int group = -1;
            int nInhEn = -1;
            int nInhVs = -1;
            try {
                group = Convert.ToInt32( kv.Values[ 1 ] );
                if (kv.Values.Length >= 3)
                    nInhEn = Convert.ToInt32( kv.Values[ 2 ] );
                if ( kv.Values.Length >= 4 )
                    nInhVs = Convert.ToInt32( kv.Values[ 3 ] );
            } catch {
                return null;
            }

            return new UserControlData( g, name, pwd, group, nInhEn, nInhVs );

        }
        virtual public void ReadUsers(string path)
        {
            // check first
            string currWorkingFile = path;
            string tmpRdFile = null;
            if ( m_bSettingFileMustEnc ) {
                if ( FileEncryptUtility.CheckFile( path ) ) {
                    tmpRdFile = Path.Combine( Path.GetDirectoryName( path ), String.Format( "{0}_{1}.ini", Path.GetFileNameWithoutExtension( path ), CommonUtilities.GetCurrentTimeStr() ) );
                    try { File.Copy( path, tmpRdFile, true ); FileEncryptUtility.DEC( tmpRdFile ); currWorkingFile = tmpRdFile; } catch { }
                } else {
                    if ( m_fpLog != null ) m_fpLog(eLogMessageType.ERROR, 100, String.Format( "[{0}::ReadUsers] file({1}) not from enc!", this.GetType().Name, path ) );
                    return;
                }
            }

            IniReaderUtility util = new IniReaderUtility();
            if ( !util.Parsing( currWorkingFile ) ) {
                if ( File.Exists( tmpRdFile ) ) { try { File.Delete( tmpRdFile ); } catch { } } // remove tmp file
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.WARNING, 100, String.Format( "[{0}::ReadUsers] parse file({1}) error!", this.GetType().Name, path ) );
                return;
            }

            SectionDataOfIni dat = util.Get( _strSEC_UserDat );
            if ( dat == null || dat.Data == null || dat.Data.Count <= 0 ) {
                if ( File.Exists( tmpRdFile ) ) { try { File.Delete( tmpRdFile ); } catch { } } // remove tmp file
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.WARNING, 100, String.Format( "[{0}::ReadUsers] no ini section in file({1}) error!", this.GetType().Name, path ) );
                return;
            }

            _Users.Clear();
            for(int i = 0; i < dat.Data.Count; i++ ) {
                UserControlData usr = NewUser( dat.Data[ i ], _GroupRights );
                if (usr != null) {
                    _Users[ usr.Name ] = usr;
                }
            }

            m_strLatestUpdatedUsersFile = String.Copy( path );
            if ( File.Exists( tmpRdFile ) ) { try { File.Delete( tmpRdFile ); } catch { } } // remove tmp file
            if ( m_fpLog != null ) m_fpLog(eLogMessageType.NORMAL, 150, String.Format( "[{0}::ReadUsers] read file {1} success!", this.GetType().Name, path ) );
        }

        virtual public void GetUserRight(string user, out int enableRight, out int visibleRight)
        {
            enableRight = _nDefaultEnabledRight;
            visibleRight = _nDefaultVisibleRight;
            if (!_Users.ContainsKey(user)) {
                return;
            }

            enableRight = _Users[ user ].EnabledLevel;
            visibleRight = _Users[ user ].VisibleLevel;
        }

        protected bool ValidUserRight( string authUser, string authUserPwd, int right, out string msg )
        {
            msg = "";
            if ( !_Users.ContainsKey( authUser ) ) {
                msg = String.Format( "The user name: {0} not exist!", authUser );
                return false;
            }
            if ( _Users[ authUser ].Pwd != authUserPwd ) {
                msg = String.Format( "The user {0}: pwd error!", authUser );
                return false;
            }
            if ( _Users[ authUser ].Group > right ) {
                msg = String.Format( "The user {0}: not auth to create user. Valid Group = {1}, Current Group = {2}\n", authUser, right, _Users[ authUser ].Group );
                return false;
            }
            return true;
        }

        virtual public bool ManageUserFromFile( string authUser, string authUserPwd, string filePath )
        {
            IniReaderUtility util = new IniReaderUtility();
            // [add]
            // <user name>= "pwd", group
            // ...
            // [remove]
            // <user name>= any not empty
            // ...
            string msg;
            if ( !ValidUserRight( authUser, authUserPwd, _nAtLeaseRightToCreateUser, out msg ) ) {
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.WARNING, 100, msg );
                return false;
            }
            if ( !util.Parsing( filePath ) ) {
                if ( m_fpLog != null ) m_fpLog(eLogMessageType.WARNING, 100, String.Format( "[{0}::ManageUserFromFile]" ) );
                return false;
            }

            // add user
            SectionDataOfIni dat = util.Get( _strSEC_MGR_USR_ADD );
            if ( dat != null && dat.Data != null && dat.Data.Count > 0 ) {
                for ( int i = 0; i < dat.Data.Count; i++ ) {
                    UserControlData info = NewUser( dat.Data[ i ], _GroupRights );
                    if ( info != null ) {
                        if ( info.Group >= _Users[ authUser ].Group && info.Name != authUser ) { // check group and self
                            if ( m_fpLog != null ) m_fpLog(eLogMessageType.NORMAL, 100, String.Format( "Add user {0}", info.Name ) );
                            _Users[ info.Name ] = info;
                            AddUserToGroup( _Groups, info );
                        }
                    }
                }
            }

            // remove user
            dat = util.Get( _strSEC_MGR_USR_RMV );
            if ( dat != null && dat.Data != null && dat.Data.Count > 0 ) {
                for ( int i = 0; i < dat.Data.Count; i++ ) {
                    if ( String.IsNullOrEmpty( dat.Data[ i ].Key ) )
                        continue;
                    string usrname = dat.Data[ i ].Key.Trim().Replace( "\"", "" ).Trim();
                    if (_Users.ContainsKey( usrname ) && _Users[ usrname ].Group >= _Users[authUser].Group && usrname != authUser) {
                        if ( m_fpLog != null ) m_fpLog(eLogMessageType.NORMAL, 100, String.Format("Delete user {0}", usrname) );
                        // remove from user
                        UserControlData info = _Users[ usrname ];
                        _Users.Remove( usrname );
                        // remove from group
                        RmvUsrFromGroup( _Groups, info );
                    }
                }
            }

            return true;
        }
        virtual public bool NewUser(string authUser, string authUserPwd, string newUser, string newUserPwd, int group, int inherGroupEnable, int inherGroupVisible, out string msg)
        {
            msg = "";
            if ( !ValidUserRight( authUser, authUserPwd, _nAtLeaseRightToCreateUser, out msg ) )
                return false;
            if (String.IsNullOrEmpty(newUser) || String.IsNullOrEmpty(newUserPwd)) {
                msg = "[ERROR] User name or Pwd can not be empty!";
                return false;
            }
            if (group < _Users[authUser].Group) {
                msg = String.Format( "[ERROR] The user {0}: need higher right to create new user {1}. Want Group = {2}, Current Group = {3}", authUser, newUser, group, _Users[ authUser ].Group );
                return false;
            }
            if ( (inherGroupEnable >= 0 && inherGroupEnable < _Users[authUser].Group) || ( inherGroupVisible >= 0 && inherGroupVisible < _Users[authUser].Group)) {
                msg = String.Format( "[ERROR] The user {0}: need higher right to create new user {1}. Current Group = {2}, Want inher Enable Group = {3}, Want inher Visible Grouup = {4}", authUser, newUser, _Users[ authUser ].Group, inherGroupEnable, inherGroupVisible );
                return false;
            }

            // check done, create
            UserControlData usr = new UserControlData( _GroupRights, newUser, newUserPwd, group, inherGroupEnable, inherGroupVisible );
            _Users[ newUser ] = usr;

            // update to group
            AddUserToGroup( _Groups, usr );
            return true;
        }
        virtual public bool DelUser(string authUser, string authUserPwd, string existUser, out string msg)
        {
            msg = "";
            if ( !ValidUserRight( authUser, authUserPwd, _nAtLeaseRightToCreateUser, out msg ) )
                return false;

            if (!_Users.ContainsKey(existUser)) {
                msg = String.Format( "[ERROR] user {0} not exist!", existUser );
                return false;
            }

            // remove from user
            UserControlData info = _Users[ existUser ];
            _Users.Remove( existUser );
            // remove from group
            RmvUsrFromGroup( _Groups, info );

            return true;
        }
        virtual public bool ChangePwd(string user, string oldPwd, string newPwd, out string msg)
        {
            msg = "";
            if (!_Users.ContainsKey(user)) {
                msg = String.Format( "[ERROR] user {0} not exist!", user );
                return false;
            }
            if (_Users[user].Pwd != oldPwd) {
                msg = String.Format( "[ERROR] user {0} error old pwd!", user );
                return false;
            }
            if (String.IsNullOrEmpty(newPwd)) {
                msg = String.Format( "[ERROR] pwd cannot be empty!" );
                return false;
            }

            _Users[ user ].Pwd = newPwd;
            return true;
        }
        virtual public bool NewGroup(string authUser, string authUserPwd, string newGroupName, int newGroup, int enableLevel, int visibleLevel, out string msg)
        {
            msg = "";
            if ( !ValidUserRight( authUser, authUserPwd, _nAtLeaseRightToCreateGroup, out msg ) )
                return false;
            if (newGroup < _Users[authUser].Group || enableLevel < _Users[authUser].Group || visibleLevel < _Users[authUser].Group || enableLevel < newGroup || visibleLevel < newGroup ) {
                msg = String.Format( "[ERROR] The user {0} not auth to create group. Current Group = {1}, Want Group = {2}, Enable Level = {3}, Visible Level = {4}", authUser, _Users[authUser].Group, newGroup, enableLevel, visibleLevel );
                return false;
            }

            _GroupRights[ newGroup ] = new UserGroupAccessRight( newGroupName, enableLevel, visibleLevel );
            return true;
        }
        virtual public bool DelGroup(string authUser, string authUserPwd, int group, out string msg)
        {
            msg = "";
            if ( !ValidUserRight( authUser, authUserPwd, _nAtLeaseRightToCreateGroup, out msg ) )
                return false;
            if (_Users[authUser].Group > group) {
                msg = String.Format("[ERROR] user {0} not auth to delete group. Current Group = {1}, Want Group = {2}", authUser, _Users[authUser].Group, group);
                return false;
            }
            if (group == 0) {
                msg = String.Format("[ERROR] cannot remove sys admin group");
                return false;
            }

            // remove group
            if (_GroupRights.ContainsKey(group)) {
                _GroupRights.Remove( group );
            }
            // remove users
            List<string> rmv = new List<string>();
            foreach(KeyValuePair<string, UserControlData> kv in _Users) {
                if (kv.Value.Group == group) {
                    rmv.Add( kv.Value.Name );
                }
            }
            for(int i = 0; i < rmv.Count; i++ ) {
                _Users.Remove( rmv[ i ] );
            }
            return true;
        }

        public virtual bool PopupLogin(out UserControlData user)
        {
            user = null;
            bool ret = false;

            frmUserLogin dlg = null;
            if ( _GuiAcl != null ) {
                dlg = (frmUserLogin) _GuiAcl.CreateShowDialog<frmUserLogin>( 0 , 0 );
                dlg.UserDat = _Users;
                dlg.MultiLang = _MultiLang;
                dlg.Visible = false;
            } else {
                dlg = new frmUserLogin( _Users );
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if(!String.IsNullOrEmpty(dlg.ErrorMsg)) {
                    string caption = "Error";
                    if ( _MultiLang != null && _MultiLang.StringMapping.ContainsKey( caption ) )
                        caption = _MultiLang.StringMapping[ caption ];
                    System.Windows.Forms.MessageBox.Show( dlg.ErrorMsg, caption, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error );
                    dlg.Dispose();
                    dlg = null;
                    return false;
                }

                ret = dlg.IsLogin;
                user = dlg.User;
            }
            dlg.Dispose();
            dlg = null;
            return ret;
        }

        public void ChangeUserGroup(UserControlData usr, int newGroup)
        {
            if ( usr == null || !_GroupRights.ContainsKey( newGroup ) )
                return;
            string msg;
            if (!ValidUserRight(usr.Name, usr.Pwd, newGroup, out msg)) {
                return;
            }
            RmvUsrFromGroup( _Groups, usr );
            usr.Group = newGroup;
            AddUserToGroup( _Groups, usr );
        }
    }
}
