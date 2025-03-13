using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.Lib.BlockAction
{
    public partial class frmActionsConfig : Form
    {
        ActionManager _AM = null;
        int _nActionSelIndex = -1;
        int _nBlockSelIndex = -1;

        public frmActionsConfig()
        {
            InitializeComponent();
        }

        internal ActionManager AM {
            get { return _AM; }
            set {
                _AM = value;
                UpdateActions();
            }
        }
        void UpdateActions()
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

            for(int i = 0; i < _AM.Actions.Count; i++ ) {
                comboBox_actionList.Items.Add( _AM.Actions[ i ]._strNameOfAction );
            }
        }
        void UpdateBlocks()
        {
            _nBlockSelIndex = -1;
            comboBox_blocksList.Text = "";
            comboBox_blocksList.Items.Clear();

            if ( _AM == null || _nActionSelIndex < 0 || _nActionSelIndex >= _AM.Actions.Count )
                return;

            for(int i = 0; i < _AM.Actions[_nActionSelIndex]._Blocks.Count; i++ ) {
                comboBox_blocksList.Items.Add( String.Format("[{0}] {1}", i, _AM.Actions[_nActionSelIndex]._Blocks[i]._strNameOfBlock) );
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
            _nBlockSelIndex = -1;
            if ( _nActionSelIndex < 0 )
                return;
            ComboBox cb = sender as ComboBox;
            if ( cb == null || cb.SelectedIndex < 0 || _AM.Actions[_nActionSelIndex]._Blocks == null || cb.SelectedIndex >= _AM.Actions[ _nActionSelIndex ]._Blocks.Count )
                return;

            _nBlockSelIndex = cb.SelectedIndex;
        }

        private void button_popupConfig_Click( object sender, EventArgs e )
        {
            if ( _AM == null || _AM.Actions == null )
                return;
            if (_nActionSelIndex < 0 || _nActionSelIndex >= _AM.Actions.Count || _AM.Actions[ _nActionSelIndex ]._Blocks == null ||
                _nBlockSelIndex < 0 || _nBlockSelIndex >= _AM.Actions[_nActionSelIndex]._Blocks.Count) {
                return;
            }
            UDataCarrier dat;
            if ( _AM.AA.CallBlockGet( _AM.Actions[_nActionSelIndex]._Blocks[ _nBlockSelIndex ]._strNameOfBlock, UCBlockBase.strUCB_POPUP_PARAMETERUI, out dat ) ) {
                if ( dat.Data != null && dat.Data.GetType() == typeof( UDataCarrierSet ) ) {
                    UDataCarrierSet s = dat.Data as UDataCarrierSet;
                    if (s != null && s._Array != null) {
                        _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._InputParam = s._Array;
                    }
                }
            }
        }

        private void button_blockSettings_Click( object sender, EventArgs e )
        {
            if ( _AM == null || _AM.Actions == null )
                return;
            if ( _nActionSelIndex < 0 || _nActionSelIndex >= _AM.Actions.Count || _AM.Actions[ _nActionSelIndex ]._Blocks == null ||
                _nBlockSelIndex < 0 || _nBlockSelIndex >= _AM.Actions[ _nActionSelIndex ]._Blocks.Count ) {
                return;
            }
            if ( _AM.AA.CallBlockSet( _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._strNameOfBlock, UCBlockBase.strUCB_POPUP_SETTING, null ) ) {
                UDataCarrier dat;
                if ( _AM.AA.CallBlockGet( _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._strNameOfBlock, UCBlockBase.strUCB_SETTINGS, out dat ) ) {
                    _AM.Actions[ _nActionSelIndex ]._Blocks[ _nBlockSelIndex ]._BlockSettings = dat;
                }
            }
        }
    }
}
