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
using static uIP.MacroProvider.StreamIO.ImageFileLoader.uMProvidImageLoader;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class UserControlDisplayLoadingInfo : UserControl
    {
        internal UMacro WorkWith { get; set; } = null;
        public UserControlDisplayLoadingInfo()
        {
            InitializeComponent();
        }

        /*
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            if ( ( keyData == ( Keys.Control | Keys.Up ) ) || ( keyData == ( Keys.Control | Keys.Down ) ) ||
                 ( keyData == ( Keys.Control | Keys.W ) ) || ( keyData == ( Keys.Control | Keys.S ) ) )
            {
                bool bInc = ( keyData == ( Keys.Control | Keys.Up ) ) || ( keyData == ( Keys.Control | Keys.W ) );

                if ( WorkWith != null && UDataCarrier.GetDicKeyStrOne<Dictionary<string, UDataCarrier>>( WorkWith.MutableInitialData, null, out var md ) && md != null &&
                    md.TryGetValue( OpenImageKey02.LoadedPaths.ToString(), out var pathsC ) &&
                    md.TryGetValue(OpenImageKey02.NextIndex.ToString(), out var nextC ) &&
                    md.TryGetValue(OpenImageKey02.Param02_EnableCycleRun.ToString(), out var cycC) )
                {
                    if ( UDataCarrier.GetDicKeyStrOne(pathsC, new string[0], out var paths ) && UDataCarrier.GetDicKeyStrOne(nextC, -1, out var nextIndex) && UDataCarrier.GetDicKeyStrOne(cycC, false, out var enCyc) )
                    {
                        if ( paths != null && paths.Length > 0 && nextIndex >= 0 && nextIndex < paths.Length)
                        {
                            if ( bInc )
                                nextIndex = nextIndex + 1 >= paths.Length ? ( enCyc ? 0 : nextIndex ) : nextIndex + 1;
                            else
                                nextIndex = nextIndex - 1 < 0 ? ( enCyc ? paths.Length - 1 : nextIndex ) : nextIndex - 1;

                            md[ OpenImageKey02.NextIndex.ToString() ].Data = nextIndex;
                        }
                    }
                }
            }
            return base.ProcessCmdKey( ref msg, keyData );
        }
        */

        internal void RxKey( Keys keyData )
        {
            if ( ( keyData == ( Keys.Control | Keys.Up ) ) || ( keyData == ( Keys.Control | Keys.Down ) ) ||
                 ( keyData == ( Keys.Control | Keys.W ) ) || ( keyData == ( Keys.Control | Keys.S ) ) )
            {
                bool bInc = ( keyData == ( Keys.Control | Keys.Up ) ) || ( keyData == ( Keys.Control | Keys.W ) );

                if ( WorkWith != null && UDataCarrier.Get<Dictionary<string, UDataCarrier>>( WorkWith.MutableInitialData, null, out var md ) && md != null )
                {
                    if ( md.TryGetValue( OpenImageKey02.IncIndex.ToString(), out var incC ) )
                    {
                        var prev = ( bool )incC.Data;
                        if ( prev != bInc &&
                             md.TryGetValue( OpenImageKey02.LoadedPaths.ToString(), out var filepathsC ) &&
                             UDataCarrier.Get<string[]>( filepathsC, null, out var filepaths ) &&
                             md.TryGetValue( OpenImageKey02.Param02_EnableCycleRun.ToString(), out var cycRunCarr ) &&
                             UDataCarrier.Get( cycRunCarr, true, out var cycRun ) &&
                             md.TryGetValue( OpenImageKey02.NextIndex.ToString(), out var indexCarr ) &&
                             UDataCarrier.Get( indexCarr, 0, out var currIndex ) )
                        {
                            int nextIndex = 0;
                            if ( cycRun )
                            {
                                if ( bInc )
                                    nextIndex = currIndex + 1 >= filepaths.Length ? 0 : currIndex + 1;
                                else
                                    nextIndex = currIndex - 1 < 0 ? filepaths.Length - 1 : currIndex - 1;
                            }
                            else
                            {
                                if ( bInc )
                                    nextIndex = currIndex + 1 >= filepaths.Length ? currIndex : currIndex + 1;
                                else
                                    nextIndex = currIndex - 1 < 0 ? currIndex : currIndex - 1;
                            }

                            if (md.TryGetValue(OpenImageKey02.NextIndex.ToString(), out var nextIC))
                            {
                                nextIC.Data = nextIndex;
                            }
                        }

                        incC.Data = bInc;
                    }
                    else md.Add( OpenImageKey02.IncIndex.ToString(), UDataCarrier.MakeOne( bInc ) );
                }
            }
        }

        internal void SetInfo(string filename, int currIndex, int totalCount)
        {
            if (textBox_displayFilename.InvokeRequired)
            {
                BeginInvoke(new Action<string, int, int>(SetInfo), filename, currIndex, totalCount);
                return;
            }

            textBox_displayFilename.Text = filename;
            numericUpDown_currIndex.Value = currIndex;
            numericUpDown_totalFileCount.Value = totalCount;
            if ( currIndex + 1 == totalCount )
                button_resetCount.BackColor = Color.LightPink;
            else if ( button_resetCount.BackColor != SystemColors.Control )
                button_resetCount.BackColor = SystemColors.Control;
        }

        private void button_resetCount_Click( object sender, EventArgs e )
        {
            if ( WorkWith == null )
                return;
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( WorkWith.MutableInitialData, null, out var mic ) || mic == null )
                return;

            if ( !mic.TryGetValue( OpenImageKey02.NextIndex.ToString(), out var nextIndexC ) )
                return;

            nextIndexC.Data = 0;
            numericUpDown_currIndex.Value = 0;
            button_resetCount.BackColor = SystemColors.Control;
        }
    }
}
