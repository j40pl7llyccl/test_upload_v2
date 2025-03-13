using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using uIP.Lib;

namespace uIP.Lib.BlockAction
{
    public partial class UserControlMakeParamBase : UserControl
    {
        public UserControlMakeParamBase()
        {
            InitializeComponent();
        }

        // implement as input parameter control
        public virtual UDataCarrier[] GetMadeParameters()
        {
            return null;
        }
        // implement as block settings control
        public virtual UDataCarrier GetSettings()
        {
            return null;
        }
    }
}
