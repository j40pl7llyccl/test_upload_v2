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
using uIP.Lib.MarshalWinSDK;

namespace uIP.MacroProvider.Commons.FlowControl
{
    public partial class UserControlJumpEnd : UserControl
    {
        internal UMacro RunWith { get; set; } = null;
        internal Form OwnerForm { get; set; } = null;
        public UserControlJumpEnd()
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        /*
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            if ( ( keyData == ( Keys.Control | Keys.Up ) ) || ( keyData == ( Keys.Control | Keys.Down ) ) ||
                 ( keyData == ( Keys.Control | Keys.W ) ) || ( keyData == ( Keys.Control | Keys.S ) ) )
            {
                TriggerEvent();
            }
            return base.ProcessCmdKey( ref msg, keyData );
        }
        */

        internal void RxKey(Keys keyData )
        {
            if ( ( keyData == ( Keys.Control | Keys.Up ) ) || ( keyData == ( Keys.Control | Keys.Down ) ) ||
                 ( keyData == ( Keys.Control | Keys.W ) ) || ( keyData == ( Keys.Control | Keys.S ) ) )
            {
                if ( RunWith is UMacroCapableOfCtrlFlow fc )
                {
                    fc.Jump2WhichMacro = 0;
                    TriggerEvent();
                }
            }
        }

        private void TriggerEvent()
        {
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( RunWith.MutableInitialData, null, out var set ) || set == null )
                return;
            if ( !set.TryGetValue( FlowCtrlProvider.Key_EvtTriggerEnd, out var evtCarr ) || evtCarr == null )
                return;

            EventWinSdkFunctions.Set( ( IntPtr )evtCarr.Data );
        }

        private void button_cont_Click( object sender, EventArgs e )
        {
            if ( RunWith == null ) return;
            if ( RunWith is UMacroCapableOfCtrlFlow fc)
            {
                fc.Jump2WhichMacro = 0;
                TriggerEvent();
            }
        }

        private void button_end_Click( object sender, EventArgs e )
        {
            if ( RunWith == null ) return;
            if (RunWith is UMacroCapableOfCtrlFlow fc)
            {
                fc.Jump2WhichMacro = ( int )MacroGotoFunctions.GOTO_END;
                TriggerEvent();
                OwnerForm?.Hide();
            }
        }
    }
}
