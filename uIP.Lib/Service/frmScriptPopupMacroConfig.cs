using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public partial class frmScriptPopupMacroConfig : Form
    {
        UScriptService _SS = null;

        public frmScriptPopupMacroConfig()
        {
            InitializeComponent();
        }

        public UScriptService SS {
            get { return _SS; }
            set {
                _SS = value;
                UpdateScripts();
            }
        }

        void UpdateScripts()
        {
            comboBox_scriptList.Items.Clear();
            comboBox_macroList.Items.Clear();
            _nCurSelScriptIndex = -1;
            if ( _SS == null || _SS.Scripts == null || _SS.Scripts.Count <= 0 )
                return;

            for(int i =0; i < _SS.Scripts.Count; i++ ) {
                comboBox_scriptList.Items.Add( _SS.Scripts[ i ].NameOfId );
            }
        }

        void UpdateMacros()
        {
            comboBox_macroList.Items.Clear();
            if ( _SS == null || _SS.Scripts == null || _nCurSelScriptIndex < 0 || _nCurSelScriptIndex >= _SS.Scripts.Count )
                return;

            for(int i = 0; i < _SS.Scripts[ _nCurSelScriptIndex].MacroSet.Count; i++ ) {
                comboBox_macroList.Items.Add( String.Format( "[{0}] {1}", i, _SS.Scripts[ _nCurSelScriptIndex ].MacroSet[ i ].MethodName ) );
            }
        }

        int _nCurSelScriptIndex = -1;

        private void comboBox_scriptList_SelectedIndexChanged( object sender, EventArgs e )
        {
            _nCurSelScriptIndex = -1;
            ComboBox cb = sender as ComboBox;
            if ( cb == null || cb.SelectedIndex < 0 || _SS == null )
                return;

            _nCurSelScriptIndex = cb.SelectedIndex;
            UpdateMacros();
        }

        private void button_popupConf_Click( object sender, EventArgs e )
        {
            if ( comboBox_macroList.SelectedIndex < 0 || _SS == null )
                return;
            if ( _nCurSelScriptIndex < 0 || _nCurSelScriptIndex >= _SS.Scripts.Count )
                return;

            int mIndex = comboBox_macroList.SelectedIndex;
            if ( mIndex < 0 || mIndex >= _SS.Scripts[ _nCurSelScriptIndex ].MacroSet.Count )
                return;

            _SS.SetMacroControl( _SS.Scripts[ _nCurSelScriptIndex ].NameOfId, mIndex, UMacroMethodProviderPlugin.PredefMacroIoctl_SetupMacro, null );
        }
    }
}
