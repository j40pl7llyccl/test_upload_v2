using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.MacroProvider.Layout.GenContainer
{
    public partial class FormLayout3 : Form
    {
        internal UMacro RunWith { get; set; } = null;
        int VerticalPanel1SizeRatio = 1;
        int VerticalPanel2SizeRatio = 1;
        int HorizontalPanel1SizeRatio = 1;
        int HorizontalPanel2SizeRatio = 1;

        public FormLayout3()
        {
            InitializeComponent();
            splitContainer_vertical.SplitterDistance = splitContainer_vertical.Size.Width / 2;
            splitContainer_horizontal.SplitterDistance = splitContainer_horizontal.Size.Height / 2;
            splitContainer_vertical.IsSplitterFixed = true;
            splitContainer_horizontal.IsSplitterFixed = true;
        }

        internal List<Action<Keys>> KeyPress = new List<Action<Keys>>();

        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            foreach ( var d in KeyPress )
                d.Invoke( keyData );
            return base.ProcessCmdKey( ref msg, keyData );
        }

        private void ConfigSplitterContainer(int vp1, int vp2, int hp1, int hp2)
        {
            splitContainer_vertical.SplitterDistance = splitContainer_vertical.Size.Width / (vp1 + vp2);
            splitContainer_horizontal.SplitterDistance = splitContainer_horizontal.Size.Height / (hp1 + hp2);

            VerticalPanel1SizeRatio = vp1;
            VerticalPanel2SizeRatio = vp2;
            HorizontalPanel1SizeRatio = hp1;
            HorizontalPanel2SizeRatio = hp2;
        }

        public void ChangeSize(int w, int h)
        {
            if (InvokeRequired)
            {
                BeginInvoke( new Action<int, int>( ChangeSize ), w, h );
                return;
            }

            Width = w;
            Height = h;
        }

        public bool LayoutSplitterContainer( int verticalPanel1, int verticalPanel2, int horizontalPanel1, int horizontalPanel2 )
        {
            if ( verticalPanel1 <= 0 || verticalPanel2 <= 0 || horizontalPanel1 <= 0 || horizontalPanel2 <= 0 )
                return false;

            if (splitContainer_vertical.InvokeRequired)
            {
                Invoke(new Action<int, int, int, int>(ConfigSplitterContainer), verticalPanel1, verticalPanel2, horizontalPanel1, horizontalPanel2 );
                return true;
            }

            ConfigSplitterContainer( verticalPanel1, verticalPanel2, horizontalPanel1, horizontalPanel2 );
            return true;
        }

        public Control LeftTopContainer => splitContainer_horizontal.Panel1;
        public Control LeftBottomContainer => splitContainer_horizontal.Panel2;
        public Control RightContainer => splitContainer_vertical.Panel2;

        private void FormLayout3_FormClosing( object sender, FormClosingEventArgs e )
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            if (RunWith?.OwnerOfScript?.UnderRunning ?? false)
            {
                RunWith.OwnerOfScript.CancelRunning();
            }
        }
    }
}
