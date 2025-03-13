using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uIP.Lib
{
    public partial class FrmEditScriptName : Form
    {
        public string ScriptName { private set; get; }
        public FrmEditScriptName()
        {
            InitializeComponent();
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            string nm = textBox_scriptName.Text.Trim();
            ScriptName = String.IsNullOrEmpty( nm ) ? "" : String.Copy( nm );
        }
    }
}
