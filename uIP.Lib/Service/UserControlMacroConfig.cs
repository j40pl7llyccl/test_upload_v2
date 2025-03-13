using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public partial class UserControlMacroConfig : UserControl
    {
        UScriptService _SS = null;
        public UserControlMacroConfig()
        {
            InitializeComponent();
        }

        internal UScriptService SS {  get { return _SS; } set { _SS = value; } }

        int _nSelScript = -1;
        int _nSelMacro = -1;
        public void UpdateSS()
        {
            _nSelScript = -1;
            _nSelMacro = -1;
            comboBox_scriptList.Items.Clear();
            comboBox_macroList.Items.Clear();
            comboBox_scriptList.Text = "";
            comboBox_macroList.Text = "";

            if ( _SS == null || _SS.Scripts == null )
                return;

            for(int i = 0; i < _SS.Scripts.Count; i++ ) {
                comboBox_scriptList.Items.Add( _SS.Scripts[ i ].NameOfId );
            }
        }

        void UpdateMacros()
        {
            comboBox_macroList.Text = "";
            _nSelMacro = -1;
            comboBox_macroList.Items.Clear();
            if ( _SS == null || _nSelScript < 0 || _nSelScript >= _SS.Scripts.Count || _SS.Scripts[ _nSelScript ].MacroSet == null )
                return;

            for ( int i = 0; i < _SS.Scripts[ _nSelScript ].MacroSet.Count; i++ ) {
                comboBox_macroList.Items.Add( String.Format( "[{0}] {1}", i, _SS.Scripts[ _nSelScript ].MacroSet[ i ].MethodName ) );
            }
        }

        private void comboBox_scriptList_SelectedIndexChanged( object sender, EventArgs e )
        {
            _nSelScript = -1;
            if ( _SS == null || _SS.Scripts == null )
                return;

            ComboBox cb = sender as ComboBox;
            if ( cb == null || cb.SelectedIndex < 0 || cb.SelectedIndex >= _SS.Scripts.Count ) {
                UpdateMacros();
                return;
            }

            _nSelScript = cb.SelectedIndex;
            UpdateMacros();
        }

        private void comboBox_macroList_SelectedIndexChanged( object sender, EventArgs e )
        {
            _nSelMacro = -1;
            if ( _SS == null || _SS.Scripts == null || _nSelScript < 0 || _nSelScript >= _SS.Scripts.Count )
                return;

            ComboBox cb = sender as ComboBox;
            if ( cb == null || cb.SelectedIndex < 0 )
                return;

            if ( _SS.Scripts[ _nSelScript ].MacroSet == null || cb.SelectedIndex >= _SS.Scripts[ _nSelScript ].MacroSet.Count )
                return;

            _nSelMacro = cb.SelectedIndex;

            // clear previous controls
            panel.Controls.Clear();

            // get new one
            bool stat;
            UDataCarrier[] got = _SS.GetMacroControl( _SS.Scripts[ _nSelScript ].NameOfId, _nSelMacro, UMacroMethodProviderPlugin.PredefMacroIoctl_SetupMacro, out stat );
            Control ctr = null;
            if(stat && got != null && got.Length > 0 && got[0] != null) {
                ctr = got[ 0 ].Data as Control;
            }

            if ( ctr != null ) {
                ctr.Location = new Point( 0, 0 );
                panel.Controls.Add( ctr );
            }
        }
    }
}
