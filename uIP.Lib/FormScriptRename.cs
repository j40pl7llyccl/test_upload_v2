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
    public partial class FormScriptRename : Form
    {
        public FormScriptRename()
        {
            InitializeComponent();
        }

        public string OriName
        {
            get => string.IsNullOrEmpty( textBox_oriName.Text ) ? "" : string.Copy( textBox_oriName.Text );
            set => textBox_oriName.Text = value;
        }

        public string NewName
        {
            get => string.IsNullOrEmpty( textBox_newName.Text ) ? "" : string.Copy( textBox_newName.Text );
            set => textBox_newName.Text = value;
        }
    }
}
