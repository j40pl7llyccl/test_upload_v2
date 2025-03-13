using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class FormBufferLoaderSetup : Form
    {
        internal UMacroCapableOfCtrlFlow WorkWith { get; set; }
        public FormBufferLoaderSetup()
        {
            InitializeComponent();
        }

        internal FormBufferLoaderSetup UpdateToUI()
        {
            if ( WorkWith == null ) return this;

            UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), -1, out var index );

            numericUpDown_jump.Value = Convert.ToDecimal( index );
            return this;
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( WorkWith == null ) return;

            var index = Convert.ToInt32( numericUpDown_jump.Value );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), index );
            WorkWith.Jump2WhichMacro = index;
            WorkWith.MustJump = index >= 0;
        }
    }
}
