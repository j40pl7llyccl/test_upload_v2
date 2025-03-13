using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.Utility;

namespace uIP.Lib.UsrControl
{
    public partial class FormAclEditor : Form
    {
        public FormAclEditor()
        {
            InitializeComponent();
        }

        private void button_genDefaultGroup_Click( object sender, EventArgs e )
        {
            using ( var ms = new MemoryStream() )
            {
                var ud = new UsersData( null, false );
                ud.WriteGroup( ms );

                var gen = ms.GetBuffer();
                var str = Encoding.UTF8.GetString( gen );
                if ( !string.IsNullOrEmpty( str ) )
                    richTextBox_edit.AppendText( str );
            }
        }

        private void button_genDefaultUser_Click( object sender, EventArgs e )
        {
            using ( var ms = new MemoryStream())
            {
                var ud = new UsersData( null, false );
                ud.WriteUsers( ms );

                var gen = ms.GetBuffer();
                var str = Encoding .UTF8.GetString( gen );
                if ( !string.IsNullOrEmpty( str ) )
                    richTextBox_edit.AppendText( str );
            }
        }

        private void button_reload_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            var got = new byte[ 0 ];
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName) )
            {
                if (FileEncryptUtility.Check( dlg.FileName))
                {
                    using(var ms = new MemoryStream() )
                    {
                        if (FileEncryptUtility.DEC( dlg.FileName, ms ))
                        {
                            got = ms.GetBuffer();
                        }
                    }
                }
                else
                {
                    got = File.ReadAllBytes( dlg.FileName );
                }
            }
            dlg.Dispose();

            if ( got == null || got.Length <= 0 )
                return;

            var str = Encoding.UTF8.GetString( got );
            if ( !string.IsNullOrEmpty( str ) )
                richTextBox_edit.AppendText( str );
        }

        private void button_save_Click( object sender, EventArgs e )
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if ( dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty( dlg.FileName ) )
            {
                try
                {
                    // write data out
                    using ( var ws = File.Open( dlg.FileName, FileMode.Create, FileAccess.ReadWrite ) )
                    {
                        var str = string.Join( "\n", richTextBox_edit.Lines );
                        var data = Encoding.UTF8.GetBytes( str );
                        ws.Write( data, 0, data.Length );
                    }

                    // need to enc file?
                    if ( checkBox_encFile.Checked )
                        FileEncryptUtility.ENC( dlg.FileName );
                }
                catch { }
            }
        }
    }
}
