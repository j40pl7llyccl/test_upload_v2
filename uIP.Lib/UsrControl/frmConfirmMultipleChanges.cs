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
    public partial class frmConfirmMultipleChanges : Form
    {
        public frmConfirmMultipleChanges()
        {
            InitializeComponent();
        }
        public void AddMessage(string str)
        {
            richTextBox_view.AppendText( String.Format( "{0}\n", str ) );
        }
    }
}
