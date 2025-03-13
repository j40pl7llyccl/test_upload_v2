using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    public partial class FormImageSetResultSettings : Form
    {
        internal UMacro WorkWith { get; set; } = null;
        public FormImageSetResultSettings()
        {
            InitializeComponent();

            foreach ( var kv in Decls.AcceptableImageFormats )
                comboBox_saveImageFormat.Items.Add( kv.Key );

            comboBox_saveImageFormat.SelectedIndex = 0;
        }

        int GetIndex( string s )
        {
            for ( int i = 0; i < comboBox_saveImageFormat.Items.Count; i++ )
            {
                if ( s == comboBox_saveImageFormat.Items[ i ].ToString() )
                    return i;
            }
            return 0;
        }

        internal FormImageSetResultSettings UpdateToUI()
        {
            if ( WorkWith == null ) return this;

            checkBox_saveResultImage.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImage.ToString(), false );
            textBox_saveImageDir.Text = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageDir.ToString(), "" );
            checkBox_writeDB.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_WriteDbResult.ToString(), false );
            checkBox_fromVideo.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_IsFromVideo.ToString(), false );
            numericUpDown_contIndex.Value = Convert.ToDecimal( UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_ContIndex.ToString(), 0 ) );
            checkBox_logMsg.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_LogMsg.ToString(), true );
            if ( UDataCarrier.DicKeyStrOneContain( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageFormat.ToString() ) )
            {
                comboBox_saveImageFormat.SelectedIndex = GetIndex( UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageFormat.ToString(), "" ) );
            }

            return this;
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( WorkWith == null ) return;

            var index = Convert.ToInt32( numericUpDown_contIndex.Value );

            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImage.ToString(), checkBox_saveResultImage.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageDir.ToString(), textBox_saveImageDir.Text );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_WriteDbResult.ToString(), checkBox_writeDB.Checked );
            UDataCarrier.SetDicKeyStrOne(WorkWith.MutableInitialData, MutableKeys.Param_IsFromVideo.ToString(), checkBox_fromVideo.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_ContIndex.ToString(), index );
            UDataCarrier.SetDicKeyStrOne(WorkWith.MutableInitialData, MutableKeys.Param_LogMsg.ToString(), checkBox_logMsg.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageFormat.ToString(), comboBox_saveImageFormat.Items[ comboBox_saveImageFormat.SelectedIndex ].ToString() );

            if ( WorkWith is UMacroCapableOfCtrlFlow fm)
            {
                fm.MustJump = index >= 0;
                fm.Jump2WhichMacro = index;
            }
        }

        private void button_pickSaveDir_Click( object sender, EventArgs e )
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if ( !string.IsNullOrEmpty( textBox_saveImageDir.Text ) )
                dlg.SelectedPath = textBox_saveImageDir.Text;
            if (dlg.ShowDialog() == DialogResult.OK )
            {
                textBox_saveImageDir.Text = dlg.SelectedPath;
            }
            dlg.Dispose();
        }
    }
}
