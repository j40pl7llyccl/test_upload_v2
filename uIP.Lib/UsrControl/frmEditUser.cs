using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using uIP.Lib.Utility;

namespace uIP.Lib.UsrControl
{
    public partial class frmEditUser : Form
    {
        private UsersData _AllUserDat = null;
        private UserControlData m_CurrUser = null;

        private List<int> m_AvailableGroups = new List<int>();
        private List<string> m_AvailableGroupsName = new List<string>();

        private string m_strUserDefaultPath = null;
        private string m_strGroupDefaultPath = null;

        public frmEditUser() { InitializeComponent(); }
        public frmEditUser(UsersData users)
        {
            InitializeComponent();
            _AllUserDat = users;

            UpdateGroupInfo();
            UpdateEditableUsers();

            button_ok.Enabled = false;
            button_ok.Visible = false;
        }
        public bool AsDiaolg {
            set {
                button_ok.Enabled = value;
                button_ok.Visible = value;
            }
        }
        public string DefaultUserFilePath {
            set { m_strUserDefaultPath = value; }
        }
        public string DefaultGroupFilePath {
            set { m_strGroupDefaultPath = value; }
        }

        private void UpdateGroupInfo()
        {
            m_AvailableGroups.Clear();
            m_AvailableGroupsName.Clear();

            if ( _AllUserDat == null || _AllUserDat.GroupRights == null )
                return;

            foreach(KeyValuePair<int, UserGroupAccessRight> kv in _AllUserDat.GroupRights) {
                m_AvailableGroups.Add( kv.Key );
                m_AvailableGroupsName.Add( kv.Value._strNameOfGroup );
            }


            // 0: user name
            // 1: group
            for ( int i = 1; i < dataGridView_Users.Columns.Count; i++ ) {
                DataGridViewComboBoxColumn cbox = dataGridView_Users.Columns[ i ] as DataGridViewComboBoxColumn;
                if (cbox != null) {
                    cbox.Items.Clear();
                } else {
                    continue;
                }
                for(int j = 0; j < m_AvailableGroupsName.Count; j++ ) {
                    cbox.Items.Add( m_AvailableGroupsName[ j ] );
                }
            }
        }

        //private void SetAvailableGroup()
        //{
        //    if (m_CurrUser != null) {
        //        List<int> available = new List<int>();
        //        for (int i = 0; i < m_AvailableGroups.Count; i++ ) {
        //            if (m_AvailableGroups[i] >= m_CurrUser.Group) {
        //                available.Add( m_AvailableGroups[ i ] );
        //            }
        //        }
        //        for ( int i = 1; i < dataGridView_Users.Columns.Count; i++ ) {
        //            DataGridViewComboBoxColumn cbox = dataGridView_Users.Columns[ i ] as DataGridViewComboBoxColumn;
        //            if ( cbox != null ) {
        //                cbox.Items.Clear();
        //            } else {
        //                continue;
        //            }
        //            for ( int j = 0; j < available.Count; j++ ) {
        //                cbox.Items.Add( GetGroupName( available[ j ] ) );
        //            }
        //        }
        //    }
        //}

        private string GetGroupName(int group)
        {
            int index = -1;
            for(int i = 0; i < m_AvailableGroups.Count; i++ ) {
                if(m_AvailableGroups[i] == group) {
                    index = i; break;
                }
            }

            return index < 0 ? "NA" : m_AvailableGroupsName[ index ];
        }
        private int GetGroupValue(string val)
        {
            int index = -1;
            for(int i = 0; i < m_AvailableGroupsName.Count; i++ ) {
                if (m_AvailableGroupsName[i] == val) {
                    index = i; break;
                }
            }
            return index < 0 ? -1 : m_AvailableGroups[ index ];
        }

        private DataGridViewComboBoxCell MakeOneCombox(int group, bool bNA = true)
        {
            DataGridViewComboBoxCell cell = new DataGridViewComboBoxCell();
            if (bNA) cell.Items.Add( "NA" );

            if ( m_CurrUser == null ) {
                for ( int i = 0; i < m_AvailableGroupsName.Count; i++ )
                    cell.Items.Add( m_AvailableGroupsName[ i ] );
                int index = -1;
                for ( int i = 0; i < m_AvailableGroups.Count; i++ ) {
                    if ( m_AvailableGroups[ i ] == group ) {
                        index = i; break;
                    }
                }
                cell.Value = index < 0 ? "NA" : m_AvailableGroupsName[ index ];
            } else {
                List<int> avaliable = new List<int>();
                for (int i = 0; i < m_AvailableGroups.Count; i++ ) {
                    if ( m_AvailableGroups[ i ] >= m_CurrUser.Group )
                        avaliable.Add( m_AvailableGroups[ i ] );
                }
                for ( int i = 0; i < avaliable.Count; i++ )
                    cell.Items.Add( GetGroupName( avaliable[ i ] ) );
                cell.Value = GetGroupName( group );
            }
            return cell;
        }
        private DataGridViewTextBoxCell MakeOneTextbox(string str)
        {
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            //cell.ReadOnly = true;
            cell.Value = str;
            return cell;
        }

        private void UpdateEditableUsers()
        {
            for(int y = 0; y < dataGridView_Users.Rows.Count; y++ ) {
                for(int x = 0; x < dataGridView_Users.Rows[y].Cells.Count; x++ ) {
                    dataGridView_Users.Rows[ y ].Cells[ x ].Dispose();
                }
            }
            dataGridView_Users.Rows.Clear();
            if ( _AllUserDat == null || m_CurrUser == null )
                return;

            Dictionary<int, UserGroup> groups = _AllUserDat.Groups;
            if(groups == null) {
                return;
            }

            int[] keys = groups.Keys.ToArray();
            Array.Sort( keys );
            for(int i = 0; i < keys.Length; i++ ) {
                UserGroup ug = groups[ keys[ i ] ];
                foreach(KeyValuePair<string, UserControlData> kv in ug._Users) {
                    if ( kv.Value.Group < m_CurrUser.Group )
                        continue;
                    //dataGridView_Users.Rows.Add( MakeOneTextbox(kv.Value.Name), MakeOneCombox( kv.Value.Group ), MakeOneCombox( kv.Value.InherEnabledGroup ), MakeOneCombox( kv.Value.InherVisibleGroup ) );
                    dataGridView_Users.Rows.Add();
                    dataGridView_Users.Rows[ dataGridView_Users.Rows.Count - 1 ].Cells[ 0 ] = MakeOneTextbox( kv.Value.Name );
                    dataGridView_Users.Rows[ dataGridView_Users.Rows.Count - 1 ].Cells[ 1 ] = MakeOneCombox( kv.Value.Group, false );
                    dataGridView_Users.Rows[ dataGridView_Users.Rows.Count - 1 ].Cells[ 2 ] = MakeOneCombox( kv.Value.InherEnabledGroup );
                    dataGridView_Users.Rows[ dataGridView_Users.Rows.Count - 1 ].Cells[ 3 ] = MakeOneCombox( kv.Value.InherVisibleGroup );
                }
            }
        }

        private void dataGridView_Users_CellDoubleClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( e.ColumnIndex != 0 )
                return;
            DataGridViewCell col = dataGridView_Users.Rows[ e.RowIndex ].Cells[0];
            DataGridViewTextBoxCell tBox = col as DataGridViewTextBoxCell;
            string name = tBox.Value as string;
            if ( tBox == null || !_AllUserDat.Users.ContainsKey(name) ) return;

            UserControlData usr = _AllUserDat.Users[ name ];
            frmChangePwd dlg = new frmChangePwd( usr, true );
            dlg.ShowDialog();

            dlg.Dispose();
            dlg = null;
        }

        private void frmEditUser_FormClosing( object sender, FormClosingEventArgs e )
        {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                if ( this.Visible ) this.Hide();
            }
        }

        private void button_reload_Click( object sender, EventArgs e )
        {
            if ( _AllUserDat == null ) return;
            if ( m_CurrUser == null ) {
                MessageBox.Show( "Need to login!" );
                return;
            }

            UpdateEditableUsers();
        }

        private void UsersDoneModification()
        {
            UpdateEditableUsers();

            if ( checkBox_autoSaveOnSucc.Checked && !String.IsNullOrEmpty( _AllUserDat.LatestUpdatedUsersFilePath ) )
                _AllUserDat.WriteUsers( _AllUserDat.LatestUpdatedUsersFilePath );
            else if ( checkBox_autoSaveOnSucc.Checked ) {
                SaveFileDialog sdlg = new SaveFileDialog();
                sdlg.Filter = "ini(*.ini)|*.ini";
                if ( sdlg.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty( sdlg.FileName ) ) {
                    _AllUserDat.WriteUsers( sdlg.FileName );
                }
                sdlg.Dispose();
                sdlg = null;
            }
        }

        private void button_loadFile_Click( object sender, EventArgs e )
        {
            if ( _AllUserDat == null ) return;
            if ( m_CurrUser == null) {
                MessageBox.Show( "Need to login!" );
                return;
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "ini(*.ini)|*.ini|txt(*.txt)|*.txt|all|*.*";
            string path = null;
            if (dlg.ShowDialog() == DialogResult.OK) {
                path = string.IsNullOrEmpty( dlg.FileName ) ? null : String.Copy( dlg.FileName );
            }
            dlg.Dispose(); dlg = null;
            if ( String.IsNullOrEmpty( path ) )
                return;

            if (_AllUserDat.ManageUserFromFile(m_CurrUser.Name, m_CurrUser.Pwd, path)) {
                UsersDoneModification();
            }
        }

        private void button_deleteSelection_Click( object sender, EventArgs e )
        {
            if ( _AllUserDat == null ) return;
            if ( m_CurrUser == null ) {
                MessageBox.Show( "Need to login!" );
                return;
            }
            if ( dataGridView_Users.SelectedRows.Count <= 0 ) {
                MessageBox.Show( "No selected item!" );
                return;
            }

            Dictionary<string, UserControlData> usrs = _AllUserDat.Users;
            for ( int i = 0; i < dataGridView_Users.SelectedRows.Count; i++ ) {
                string name = dataGridView_Users.SelectedRows[ i ].Cells[ 0 ].Value as string;
                if ( usrs.ContainsKey( name ) && name != m_CurrUser.Name ) {
                    string msg = null;
                    _AllUserDat.DelUser( m_CurrUser.Name, m_CurrUser.Pwd, name, out msg );
                }
            }

            UsersDoneModification();
        }

        private void button_changeUser_Click( object sender, EventArgs e )
        {
            frmUserLogin dlg = new frmUserLogin( _AllUserDat.Users );
            if (dlg.ShowDialog() == DialogResult.OK && dlg.IsLogin) {
                m_CurrUser = dlg.User;
                //SetAvailableGroup();
                UpdateEditableUsers();
                toolStripStatusLabel_userName.Text = m_CurrUser != null ? m_CurrUser.Name : "NA";
                toolStripStatusLabel_group.Text = GetGroupName( m_CurrUser == null ? -1 : m_CurrUser.Group );
            }
            dlg.Dispose();
            dlg = null;
        }

        private void dataGridView_Users_CellEndEdit( object sender, DataGridViewCellEventArgs e )
        {
            string usrName = dataGridView_Users.Rows[ e.RowIndex ].Cells[ 0 ].Value as string;
            if ( String.IsNullOrEmpty( usrName ) )
                return;

            if ( !_AllUserDat.Users.ContainsKey( usrName ) )
                return;
            UserControlData info = _AllUserDat.Users[ usrName ];
            switch ( e.ColumnIndex ) {
            case 1:
                int group = GetGroupValue( dataGridView_Users.Rows[ e.RowIndex ].Cells[ 1 ].Value as string );
                if ( info.Group != group ) {
                    if ( MessageBox.Show( "Apply changes?", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1 ) == DialogResult.OK ) {
                        //info.Group = group;
                        _AllUserDat.ChangeUserGroup( info, group );
                        if (toolStripStatusLabel_userName.Text == info.Name) {
                            toolStripStatusLabel_group.Text = GetGroupName( info.Group );
                        }
                        UpdateEditableUsers();
                    } else {
                        dataGridView_Users.Rows[ e.RowIndex ].Cells[ 1 ].Value = GetGroupName( info.Group );
                    }
                }
                break;

            case 2:
                int inherEn = GetGroupValue( dataGridView_Users.Rows[ e.RowIndex ].Cells[ 2 ].Value as string );
                if ( info.InherEnabledGroup != inherEn ) {
                    if ( MessageBox.Show( "Apply changes?", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1 ) == DialogResult.OK ) {
                        info.InherEnabledGroup = inherEn;
                    } else {
                        dataGridView_Users.Rows[ e.RowIndex ].Cells[ 2 ].Value = GetGroupName( info.InherEnabledGroup );
                    }
                }
                break;

            case 3:
                int inherVs = GetGroupValue( dataGridView_Users.Rows[ e.RowIndex ].Cells[ 3 ].Value as string );
                if ( info.InherVisibleGroup != inherVs ) {
                    if ( MessageBox.Show( "Apply changes?", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1 ) == DialogResult.OK ) {
                        info.InherVisibleGroup = inherVs;
                    } else {
                        dataGridView_Users.Rows[ e.RowIndex ].Cells[ 3 ].Value = GetGroupName( info.InherVisibleGroup );
                    }
                }
                break;
            }
        }

        private void button_SaveToFile_Click( object sender, EventArgs e )
        {
            SaveFileDialog sdlg = new SaveFileDialog();
            sdlg.Filter = "ini(*.ini)|*.ini";
            if ( !String.IsNullOrEmpty( m_strUserDefaultPath ) && System.IO.File.Exists( m_strUserDefaultPath ) )
                sdlg.FileName = m_strUserDefaultPath;
            if ( sdlg.ShowDialog() == DialogResult.OK && !String.IsNullOrEmpty( sdlg.FileName ) ) {
                _AllUserDat.WriteUsers( sdlg.FileName );
            }
            sdlg.Dispose();
            sdlg = null;
        }

        private void button_DECGroup_Click( object sender, EventArgs e )
        {
            string path = null;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "ini(*.ini)|*.ini|txt(*.txt)|*.txt|all files|*.*";
            if (dlg.ShowDialog() == DialogResult.OK) {
                path = string.IsNullOrEmpty( dlg.FileName ) ? null : String.Copy( dlg.FileName );
            }
            dlg.Dispose();
            dlg = null;
            if ( String.IsNullOrEmpty( path ) )
                return;

            if ( !FileEncryptUtility.CheckFile( path ) )
                return;

            FileEncryptUtility.DEC( path );
        }

        private void button_ENCGroup_Click( object sender, EventArgs e )
        {
            string path = null;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "ini(*.ini)|*.ini|txt(*.txt)|*.txt|all files|*.*";
            if ( dlg.ShowDialog() == DialogResult.OK ) {
                path = string.IsNullOrEmpty( dlg.FileName ) ? null : String.Copy( dlg.FileName );
            }
            dlg.Dispose();
            dlg = null;
            if ( String.IsNullOrEmpty( path ) )
                return;

            if ( !FileEncryptUtility.CheckFile( path ) )
                return;

            FileEncryptUtility.ENC( path );
        }

        private void button_new_Click( object sender, EventArgs e )
        {
            if (_AllUserDat == null)
            {
                MessageBox.Show( "no all user data!" );
                return;
            }
            if ( m_CurrUser == null )
            {
                MessageBox.Show( "Need to login!" );
                return;
            }

            Dictionary<string, int> groups = new Dictionary<string, int>();
            for(int i = 0; i < m_AvailableGroups.Count; i++ )
            {
                if (m_CurrUser.EnabledLevel <= m_AvailableGroups[i] )
                {
                    groups.Add( m_AvailableGroupsName[ i ], m_AvailableGroups[ i ] );
                }
            }

            FormNewUser dlg = new FormNewUser();
            dlg.ConfigGroupList( groups );
            if (dlg.ShowDialog() == DialogResult.OK && dlg.Status)
            {
                if (!_AllUserDat.NewUser(m_CurrUser.Name, m_CurrUser.Pwd, dlg.UserName, dlg.Password, dlg.GroupNo, dlg.GroupNo, dlg.GroupNo, out var err))
                {
                    MessageBox.Show( $"New user fail with {err}" );
                }
                else
                {
                    UpdateEditableUsers();
                }
            }
            dlg.Dispose();
        }
    }
}
