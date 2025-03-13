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

using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class FormSetupOpenDirFiles : Form
    {
        private bool _bFolderPathDirty = false;
        internal UMacro RunWith { get; set; } = null;
        public FormSetupOpenDirFiles()
        {
            InitializeComponent();
        }

        internal void ReloadInfo()
        {
            if ( RunWith == null ) return;
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( RunWith.MutableInitialData, null, out var set ) )
                return;

            if ( set.TryGetValue( OpenImageKey02.Param02_LoadingPath.ToString(), out var pathCarr ) && UDataCarrier.Get( pathCarr, "", out var path ) )
                textBox_pickedDir.Text = path;
            if ( set.TryGetValue( OpenImageKey02.Param02_SearchPattern.ToString(), out var searchPatCarr ) && UDataCarrier.Get( searchPatCarr, "", out var searchPat ) )
                textBox_searchPat.Text = searchPat;
            if ( set.TryGetValue( OpenImageKey02.Param02_EnableCycleRun.ToString(), out var cycRunCarr ) && UDataCarrier.Get( cycRunCarr, false, out var cycRun ) )
                checkBox_enableCycRun.Checked = cycRun;
            if ( set.TryGetValue( OpenImageKey02.NextIndex.ToString(), out var nextIndexCarr ) && UDataCarrier.Get( nextIndexCarr, 0, out var nextIndex ) )
                numericUpDown_resetIndex.Value = nextIndex;
        }

        private void button_pickFolder_Click( object sender, EventArgs e )
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if ( !string.IsNullOrEmpty( textBox_pickedDir.Text ) )
                dlg.SelectedPath = string.Copy( textBox_pickedDir.Text );
            if ( dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty( dlg.SelectedPath ) )
            {
                textBox_pickedDir.Text = string.Copy( dlg.SelectedPath );
                _bFolderPathDirty = true;
            }
            dlg.Dispose();
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( RunWith == null )
                return;
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( RunWith.MutableInitialData, null, out var set ) || set == null )
                return;

            var edSearchPat = textBox_searchPat.Text.Trim();
            if ( string.IsNullOrEmpty( edSearchPat ) ) edSearchPat = "*.*";
            var searchPattern = edSearchPat;
            if ( set.TryGetValue( OpenImageKey02.Param02_SearchPattern.ToString(), out var searchPatCarr ) &&
                UDataCarrier.Get( searchPatCarr, "", out var searchPat ) &&
                !string.IsNullOrEmpty( searchPat ) &&
                !_bFolderPathDirty )
            {
                _bFolderPathDirty = searchPat != edSearchPat;
            }
            else
                _bFolderPathDirty = true;
            // reset search pattern
            if ( set.ContainsKey( OpenImageKey02.Param02_SearchPattern.ToString() ) )
                set[ OpenImageKey02.Param02_SearchPattern.ToString() ] = UDataCarrier.MakeOne( searchPattern );
            else
                set.Add( OpenImageKey02.Param02_SearchPattern.ToString(), UDataCarrier.MakeOne( searchPattern ) );


            // search
            if (_bFolderPathDirty)
            {
                try
                {
                    // set file paths
                    var files = Directory.GetFiles( textBox_pickedDir.Text, searchPattern, SearchOption.TopDirectoryOnly );
                    if ( set.ContainsKey( OpenImageKey02.LoadedPaths.ToString() ) )
                        set[ OpenImageKey02.LoadedPaths.ToString() ] = UDataCarrier.MakeOne( files );
                    else
                        set.Add( OpenImageKey02.LoadedPaths.ToString(), UDataCarrier.MakeOne( files ) );

                    // reset to begin index
                    if ( set.ContainsKey( OpenImageKey02.NextIndex.ToString() ) )
                        set[ OpenImageKey02.NextIndex.ToString() ] = UDataCarrier.MakeOne( 0 );
                    else
                        set.Add( OpenImageKey02.NextIndex.ToString(), UDataCarrier.MakeOne( 0 ) );
                }
                catch ( Exception ex ) { }

                // write search path back
                if ( set.ContainsKey( OpenImageKey02.Param02_LoadingPath.ToString() ) )
                    set[ OpenImageKey02.Param02_LoadingPath.ToString() ] = UDataCarrier.MakeOne( string.Copy( textBox_pickedDir.Text ) );
            }


            // reset index
            var resetIndex = Convert.ToInt32( numericUpDown_resetIndex.Value );
            if (resetIndex >= 0 && 
                set.TryGetValue(OpenImageKey02.LoadedPaths.ToString(), out var filepathsCarr ) &&
                UDataCarrier.Get<string[]>(filepathsCarr, null, out var filepaths) && 
                filepaths != null && filepaths.Length > 0 && resetIndex < filepaths.Length)
            {
                if ( set.ContainsKey( OpenImageKey02.NextIndex.ToString() ) )
                    set[ OpenImageKey02.NextIndex.ToString() ] = UDataCarrier.MakeOne( resetIndex );
                else
                    set.Add( OpenImageKey02.NextIndex.ToString(), UDataCarrier.MakeOne( resetIndex ) );
            }

            // enable cyc run
            if ( set.ContainsKey( OpenImageKey02.Param02_EnableCycleRun.ToString() ) )
                set[ OpenImageKey02.Param02_EnableCycleRun.ToString() ] = UDataCarrier.MakeOne( checkBox_enableCycRun.Checked );
            else
                set.Add( OpenImageKey02.Param02_EnableCycleRun.ToString(), UDataCarrier.MakeOne( checkBox_enableCycRun.Checked ) );

        }

        private void button_resetIndex_Click( object sender, EventArgs e )
        {
            if ( RunWith == null )
                return;

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( RunWith.MutableInitialData, null, out var set ) || set == null )
                return;

            if ( set.TryGetValue( OpenImageKey02.NextIndex.ToString(), out var indexC ) )
            {
                indexC.Data = 0;
            }
            else
                set.Add( OpenImageKey02.NextIndex.ToString(), UDataCarrier.MakeOne( 0 ) );

        }
    }
}
