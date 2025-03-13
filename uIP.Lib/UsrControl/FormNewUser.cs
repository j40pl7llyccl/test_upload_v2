using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uIP.Lib.UsrControl
{
    public partial class FormNewUser : Form
    {
        public FormNewUser()
        {
            InitializeComponent();
        }

        public bool Status { get; private set; } = false;
        public string UserName { get; private set; } = "";
        public string Password { get; private set; } = "";
        public int GroupNo { get; private set; } = int.MaxValue;

        private Dictionary<string, int> Groups { get; set; } = null;

        public void ConfigGroupList(Dictionary<string, int> groupList)
        {
            if ( groupList.Count == 0 )
                return;

            comboBox_group.Items.Clear();

            foreach(var kv in groupList)
            {
                comboBox_group.Items.Add( $"{kv.Key}:{kv.Value}" );
            }
            comboBox_group.SelectedIndex = 0;

            Groups = groupList;
        }

        private void textBox_password_Leave( object sender, EventArgs e )
        {
            if ( !string.IsNullOrEmpty(textBox_password.Text) && !string.IsNullOrEmpty(textBox_confirmPwd.Text))
            {
                label_pwdStatus.BackColor = textBox_password.Text == textBox_confirmPwd.Text ? Color.Green : Color.Red;
            }
        }

        private void textBox_confirmPwd_Leave( object sender, EventArgs e )
        {
            if ( !string.IsNullOrEmpty( textBox_password.Text ) && !string.IsNullOrEmpty( textBox_confirmPwd.Text ) )
            {
                label_pwdStatus.BackColor = textBox_password.Text == textBox_confirmPwd.Text ? Color.Green : Color.Red;
            }
        }

        private void textBox_confirmPwd_TextChanged( object sender, EventArgs e )
        {
            label_pwdStatus.BackColor = textBox_password.Text == textBox_confirmPwd.Text ? Color.Green : Color.Red;
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if (string.IsNullOrEmpty(textBox_password.Text))
            {
                MessageBox.Show( "Password empty!" );
                return;
            }
            if (textBox_confirmPwd.Text != textBox_confirmPwd.Text)
            {
                MessageBox.Show( "Password not match!" );
                return;
            }

            if (string.IsNullOrEmpty(textBox_username.Text))
            {
                MessageBox.Show( "user name empty!" );
                return;
            }

            if (comboBox_group.SelectedIndex < 0)
            {
                MessageBox.Show( "" );
                return;
            }

            var str = comboBox_group.Items[ comboBox_group.SelectedIndex ] as string;
            var key = str.Substring( 0, str.IndexOf( ':' ) );

            UserName = string.Copy( textBox_username.Text );
            Password = string.Copy( textBox_password.Text );
            GroupNo = Groups[ key ];
            Status = true;
        }
    }
}
