using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uIP.Lib.UsrControl
{
    public partial class FormListControl : Form
    {
        string FolderPath { get; set; } = "";
        public FormListControl()
        {
            InitializeComponent();

            // dump all for multilanguage
            button_writeAllForMultilang.Click += new EventHandler( ( s, e ) =>
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if ( !string.IsNullOrEmpty( FolderPath ) )
                    dlg.SelectedPath = FolderPath;
                if ( dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty( dlg.SelectedPath ) )
                {
                    FolderPath = string.Copy( dlg.SelectedPath );
                    var errL = new List<string>();
                    var got = Search();
                    foreach ( var kv in got )
                    {
                        foreach ( var t in kv.Value )
                        {
                            Control ctrl = null;
                            try
                            {
                                ctrl = Activator.CreateInstance( t ) as Control;
                                UserMultilanguage.WriteControlXml( ctrl, Path.Combine( dlg.SelectedPath, $"{t.FullName}.xml" ) );
                            }
                            catch
                            {
                                errL.Add( t.FullName );
                            }
                            ctrl?.Dispose();
                        }
                    }
                    if ( errL.Count > 0 )
                    {
                        MessageBox.Show( $"Follow type not output\n{string.Join( "\n", errL.ToArray() )}" );
                    }
                    else
                        MessageBox.Show( $"Write all control done to\n{dlg.SelectedPath}" );
                }
                dlg.Dispose();
            } );

            // dump for all for ACL
            button_dumpAllFroAcl.Click += new EventHandler( ( s, e ) =>
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if ( !string.IsNullOrEmpty( FolderPath ) )
                    dlg.SelectedPath = FolderPath;
                if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                {
                    FolderPath = string.Copy( dlg.SelectedPath );
                    var got = Search();
                    var errL = new List<string>();
                    if (got != null)
                    {
                        foreach(var kv in got)
                        {
                            foreach(var t in kv.Value)
                            {
                                Control ctrl = null;
                                try
                                {
                                    ctrl = Activator.CreateInstance( t ) as Control;
                                    UsersGuiControl.WriteAll( ctrl, Path.Combine( FolderPath, $"{t.FullName}.ini" ) );
                                }
                                catch ( Exception exp )
                                {
                                    Console.WriteLine( exp );
                                    errL.Add( t.FullName );
                                }
                                ctrl?.Dispose();
                            }
                        }
                        if ( errL.Count > 0 )
                        {
                            MessageBox.Show( $"Follow type not output\n{string.Join( "\n", errL.ToArray() )}" );
                        }
                        else
                            MessageBox.Show( $"Write all control done to\n{dlg.SelectedPath}" );
                    }

                }
            } );
        }

        private static Dictionary<Assembly, List<Type>> Search()
        {
            var aa = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<Assembly, List<Type>> got = new Dictionary<Assembly, List<Type>>();
            foreach ( var a in aa )
            {
                if ( a.FullName.IndexOf( "uIP", StringComparison.InvariantCultureIgnoreCase ) == 0 )
                {
                    var tps = a.GetTypes();
                    foreach ( var t in tps )
                    {
                        if ( t.IsSubclassOf( typeof( Control ) ) )
                        {
                            if ( !got.TryGetValue( a, out var cc ) )
                            {
                                cc = new List<Type>();
                                got.Add( a, cc );
                            }
                            cc.Add( t );
                        }
                    }
                }
            }
            return got;
        }

        private void button_listEnvAll_Click( object sender, EventArgs e )
        {
            richTextBox_edit.Clear();

            var aa = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<Assembly, List<Type>> got = Search();

            foreach(var kv in got)
            {
                richTextBox_edit.AppendText( $"[{kv.Key.FullName}]\n" );
                foreach ( var t in kv.Value )
                    richTextBox_edit.AppendText( $"   {t.FullName}\n" );

                richTextBox_edit.AppendText( "\n" );
            }
        }
    }
}
