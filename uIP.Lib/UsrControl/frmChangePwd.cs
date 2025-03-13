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
    public partial class frmChangePwd : Form
    {
        //private bool m_bChanged = false;
        private string m_strMsg = "";
        private UserControlData m_CurrUser = null;
        private bool m_bForce = false;
        private IMultilanguageManager m_MultiLang = null;
        private bool m_bSucc = false;

        public UserControlData UserData {  set { m_CurrUser = value; } }
        public IMultilanguageManager MultiLang {  set { m_MultiLang = value; } }
        public string ErrorString { get { return m_strMsg; } }
        public bool Succ {  get { return m_bSucc; } }

        public frmChangePwd(UserControlData user, bool bForceChange)
        {
            InitializeComponent();

            m_CurrUser = user;
            this.Text += String.Format( " : {0}", user == null ? "" : user.Name );
            if (bForceChange) {
                label_oldPwd.Visible = false;
                textBox_oldPwd.Visible = false;
                m_bForce = bForceChange;
            }
        }
        public frmChangePwd()
        {
            InitializeComponent();
            //m_bForce = true;
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            //m_bChanged = false;
            if (m_CurrUser == null) {
                return;
            }
            if (!m_bForce) {
                if (textBox_oldPwd.Text != m_CurrUser.Pwd) {
                    m_strMsg = "Old Password error!";
                    if ( m_MultiLang != null )
                        m_strMsg = m_MultiLang.Get( m_strMsg );
                    return;
                }
            }
            if( String.IsNullOrEmpty( textBox_newPwd.Text ) || String.IsNullOrEmpty(textBox_confirmNewPwd.Text )) {
                m_strMsg = "Password cannot be empty!";
                if ( m_MultiLang != null )
                    m_strMsg = m_MultiLang.Get( m_strMsg );
                return;
            }
            if ( textBox_newPwd.Text != textBox_confirmNewPwd.Text ) {
                m_strMsg = "New Password mismatched!";
                if ( m_MultiLang != null )
                    m_strMsg = m_MultiLang.Get( m_strMsg );
                return;
            }

            m_CurrUser.Pwd = String.Copy( textBox_newPwd.Text );
            m_bSucc = true;
            //sm_bChanged = true;
        }
    }
}
