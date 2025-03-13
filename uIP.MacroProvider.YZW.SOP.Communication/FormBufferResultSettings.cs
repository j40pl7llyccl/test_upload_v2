using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    public partial class FormBufferResultSettings : Form
    {
        internal UMacro WorkWith { get; set; } = null;
        public FormBufferResultSettings()
        {
            InitializeComponent();

            foreach ( var kv in Decls.AcceptableImageFormats )
                comboBox_saveImageFormat.Items.Add( kv.Key );

            comboBox_saveImageFormat.SelectedIndex = 0;
        }

        int GetIndex(string s)
        {
            for(int i = 0; i < comboBox_saveImageFormat.Items.Count; i++ )
            {
                if ( s == comboBox_saveImageFormat.Items[ i ].ToString() )
                    return i;
            }
            return 0;
        }

        internal FormBufferResultSettings UpdateToUI()
        {
            if ( WorkWith == null ) return this;

            checkBox_saveResultImage.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImage.ToString(), false );
            textBox_saveImgeRoot.Text = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageDir.ToString(), "" );
            checkBox_writeDB.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_WriteDbResult.ToString(), false );
            if ( UDataCarrier.DicKeyStrOneContain(WorkWith.MutableInitialData, MutableKeys.Param_SaveImageFormat.ToString()))
            {
                comboBox_saveImageFormat.SelectedIndex = GetIndex( UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageFormat.ToString(), "" ) );
            }

            return this;
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( WorkWith == null ) return;

            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImage.ToString(), checkBox_saveResultImage.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageDir.ToString(), textBox_saveImgeRoot.Text );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_WriteDbResult.ToString(), checkBox_writeDB.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, MutableKeys.Param_SaveImageFormat.ToString(), comboBox_saveImageFormat.Items[ comboBox_saveImageFormat.SelectedIndex ].ToString() );
        }

        private void button_pickSaveDir_Click( object sender, EventArgs e )
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if ( !string.IsNullOrEmpty( textBox_saveImgeRoot.Text ) )
                dlg.SelectedPath = textBox_saveImgeRoot.Text;
            if (dlg.ShowDialog() == DialogResult.OK )
            {
                textBox_saveImgeRoot.Text = dlg.SelectedPath;
            }
            dlg.Dispose();
        }
    }
}
