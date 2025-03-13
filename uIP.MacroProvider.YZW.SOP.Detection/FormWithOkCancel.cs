using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public partial class FormWithOkCancel : Form
    {
        internal Control AddedControl { get; private set; }
        public FormWithOkCancel()
        {
            InitializeComponent();
        }

        internal void AddControl(Control c)
        {
            if ( c == null )
                return;

            var okOffset = new Point( button_ok.Location.X - ClientSize.Width, button_ok.Location.Y - ClientSize.Height );
            var cancelOffset = new Point( button_cancel.Location.X - ClientSize.Width, button_cancel.Location.Y - ClientSize .Height );

            AddedControl?.Dispose();

            c.Location = new Point( 0, 0 );
            Controls.Add(c);
            AddedControl = c;

            ClientSize = new Size( c.Width, c.Height + (-okOffset.Y) );
            button_ok.Location = new Point( okOffset.X + ClientSize.Width, okOffset.Y + ClientSize.Height );
            button_cancel.Location = new Point( cancelOffset.X + ClientSize.Width, cancelOffset.Y + ClientSize.Height );
        }
    }
}
