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
    public partial class UserControlActionsConfig : UserControl
    {
        ActionManager _AM = null;
        int _nActionSelIndex = -1;
        int _nBlockSelIndex = -1;
        UserControlMakeParamBase _Control = null;
        bool _bConfigInputParam = true;

        public UserControlActionsConfig()
        {
            InitializeComponent();
        }

        internal bool IsConfigInputParam {
            get { return _bConfigInputParam; }
            set { _bConfigInputParam = value; }
        }

        internal ActionManager AM {
            get { return _AM; }
            set {
                _AM = value;
            }
        }

        public void UpdateActions()
        {
            if ( _AM == null )
                return;

            _nActionSelIndex = -1;
            _nBlockSelIndex = -1;

            comboBox_actionList.Text = "";
            comboBox_blocksList.Text = "";
            comboBox_actionList.Items.Clear();
            comboBox_blocksList.Items.Clear();

            if ( _AM.Actions == null || _AM.Actions.Count <= 0 )
                return;

            for ( int i = 0; i < _AM.Actions.Count; i++ ) {
                comboBox_actionList.Items.Add( _AM.Actions[ i ]._strNameOfAction );
            }
        }
        void UpdateBlocks()
        {
            _nBlockSelIndex = -1;
            comboBox_blocksList.Text = "";
            comboBox_blocksList.Items.Clear();
            _Control = null;

            if ( _AM == null || _nActionSelIndex < 0 || _nActionSelIndex >= _AM.Actions.Count )
                return;

            for ( int i = 0; i < _AM.Actions[ _nActionSelIndex ]._Blocks.Count; i++ ) {
                comboBox_blocksList.Items.Add( String.Format( "[{0}] {1}", i, _AM.Actions[ _nActionSelIndex ]._Blocks[ i ]._strNameOfBlock ) );
            }
        }

        private void comboBox_actionList_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( _AM == null || _AM.Actions == null )
                return;
            ComboBox cb = sender as ComboBox;
            if ( cb == null || cb.SelectedIndex < 0 || cb.SelectedIndex >= _AM.Actions.Count )
                return;

            _nActionSelIndex = cb.SelectedIndex;
            UpdateBlocks();
        }

        private void comboBox_blocksList_SelectedIndexChanged( object sender, EventArgs e )
        {
            _Control = null;
            _nBlockSelIndex = -1;
            if ( _nActionSelIndex < 0 )
                return;
            ComboBox cb = sender as ComboBox;
            if ( cb == null || cb.SelectedIndex < 0 || _AM.Actions[ _nActionSelIndex ]._Blocks == null || cb.SelectedIndex >= _AM.Actions[ _nActionSelIndex ]._Blocks.Count )
                return;

            _nBlockSelIndex = cb.SelectedIndex;
            
            // reset child control
            panel.Controls.Clear();

            // get control
            UDataCarrier got;
            string cmd = checkBox_confInput.Checked ? UCBlockBase.strUCB_INPUT_PARAM_UI_CONTROL : UCBlockBase.strUCB_BLOCK_SETTINGS_UI_CONTROL;
            if (_AM.AA.CallBlockGet( _AM.Actions[ _nActionSelIndex ]._Blocks[_nBlockSelIndex]._strNameOfBlock, UCBlockBase.strUCB_INPUT_PARAM_UI_CONTROL, out got ) ) {
                if (got != null && got.Data != null && got.Data.GetType().IsSubclassOf(typeof( UserControlMakeParamBase))) {
                    _Control = got.Data as UserControlMakeParamBase;
                }
            }

            if (_Control != null) {
                _Control.Location = new Point( 0, 0 );
                panel.Controls.Add( _Control );
            }
        }

        private void button_update_Click( object sender, EventArgs e )
        {
            if ( _Control == null )
                return;

            if ( _nActionSelIndex < 0 || _nActionSelIndex >= _AM.Actions.Count || _AM.Actions[ _nActionSelIndex ]._Blocks == null ||
                _nBlockSelIndex < 0 || _nBlockSelIndex >= _AM.Actions[ _nActionSelIndex ]._Blocks.Count ) {
                return;
            }

            if ( _bConfigInputParam )
            _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._InputParam = _Control.GetMadeParameters();
            else {
                _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._BlockSettings = _Control.GetSettings();
            }
        }

        private void checkBox_confInput_CheckStateChanged( object sender, EventArgs e )
        {
            if ( _nActionSelIndex < 0 || _nActionSelIndex >= _AM.Actions.Count || _AM.Actions[ _nActionSelIndex ]._Blocks == null ||
                _nBlockSelIndex < 0 || _nBlockSelIndex >= _AM.Actions[ _nActionSelIndex ]._Blocks.Count ) {
                return;
            }

            // reset child control
            panel.Controls.Clear();

            // get control
            UDataCarrier got;
            string cmd = checkBox_confInput.Checked ? UCBlockBase.strUCB_INPUT_PARAM_UI_CONTROL : UCBlockBase.strUCB_BLOCK_SETTINGS_UI_CONTROL;
            if ( _AM.AA.CallBlockGet( _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._strNameOfBlock, UCBlockBase.strUCB_INPUT_PARAM_UI_CONTROL, out got ) ) {
                if ( got != null && got.Data != null && got.Data.GetType().IsSubclassOf( typeof( UserControlMakeParamBase ) ) ) {
                    _Control = got.Data as UserControlMakeParamBase;
                }
            }

            if ( _Control != null ) {
                _Control.Location = new Point( 0, 0 );
                panel.Controls.Add( _Control );
            }
        }
    }
}
