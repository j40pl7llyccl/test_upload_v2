using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.Lib.UsrControl
{
    public partial class frmUserLogin : Form
    {
        private bool m_bLogin = false;
        private Dictionary<string, UserControlData> m_Users = null;
        private UserControlData m_LoginUser = null;
        private string m_ErrMsg = "";
        private IMultilanguageManager m_MultiLang = null;

        public Dictionary<string, UserControlData> UserDat { set { m_Users = value; } }
        public IMultilanguageManager MultiLang {  set { m_MultiLang = value; } }

        public frmUserLogin(Dictionary<string, UserControlData> users)
        {
            InitializeComponent();
            m_Users = users;
        }
        public frmUserLogin()
        {
            InitializeComponent();
        }

        public void ResetData()
        {
            textBox_userName.Text = "";
            textBox_password.Text = "";
            m_LoginUser = null;
            m_bLogin = false;
            m_ErrMsg = "";
        }

        public UserControlData User { get { return m_LoginUser; } }
        public bool IsLogin { get { return m_bLogin; } }
        public string ErrorMsg {  get { return m_ErrMsg; } }

        private void button_ok_Click( object sender, EventArgs e )
        {
            m_bLogin = false;
            m_ErrMsg = "";
            m_LoginUser = null;
            if (String.IsNullOrEmpty(textBox_userName.Text)) {
                m_ErrMsg = "No User Name input!";
                if ( m_MultiLang != null )
                    m_ErrMsg = m_MultiLang.Get(m_ErrMsg);
                return;
            }
            if (String.IsNullOrEmpty(textBox_password.Text)) {
                m_ErrMsg = "No Password input!";
                if ( m_MultiLang != null )
                    m_ErrMsg = m_MultiLang.Get( m_ErrMsg );
                return;
            }

            if (m_Users == null) {
                m_ErrMsg = "No users info!";
                if ( m_MultiLang != null )
                    m_ErrMsg = m_MultiLang.Get( m_ErrMsg );
                return;
            }
            if (!m_Users.ContainsKey(textBox_userName.Text)) {
                string fm = "No user: {0}!";
                if ( m_MultiLang != null )
                    fm = m_MultiLang.Get( fm );
                m_ErrMsg = String.Format( fm, textBox_userName.Text );
                return;
            }
            if (m_Users[textBox_userName.Text].Pwd != textBox_password.Text) {
                string fm = "User {0}: wrong password!";
                if ( m_MultiLang != null )
                    fm = m_MultiLang.Get( fm );
                m_ErrMsg = String.Format( fm, textBox_userName.Text );
                return;
            }

            m_LoginUser = m_Users[ textBox_userName.Text ];
            m_bLogin = true;
        }
    }
}
