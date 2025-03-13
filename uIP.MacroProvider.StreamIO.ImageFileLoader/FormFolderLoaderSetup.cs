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

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class FormFolderLoaderSetup : Form
    {
        internal UMacroCapableOfCtrlFlow WorkWith { get; set; }
        Dictionary<int,string> IndexMapping { get; set; } = new Dictionary<int, string>();
        string PluginClassName { get; set; } = "";
        string FuncName { get; set; } = "";
        string[] LoadedFiles { get; set; } = new string[ 0 ];

        public FormFolderLoaderSetup()
        {
            InitializeComponent();

            comboBox_pluginName.Items.Add( "" );

            try
            {
                int i = 1;
                foreach(var plugin in ULibAgent.Singleton.AssemblyPlugins.PluginAssemblies)
                {
                    comboBox_pluginName.Items.Add( plugin.GetType().Name );
                    IndexMapping.Add( i++, plugin.GetType().FullName);
                }
            }
            catch { }
        }

        internal FormFolderLoaderSetup UpdateToUI()
        {
            if ( WorkWith == null ) return this;

            checkBox_incIndex.Checked = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_IsIncIndex.ToString(), true );
            checkBox_cycRun.Checked = UDataCarrier.GetDicKeyStrOne(WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_EnableCycleRun.ToString(), false );
            textBox_folderPath.Text = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_LoadingPath.ToString(), "" );
            numericUpDown_jumpIndex.Value = Convert.ToDecimal( UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), -1 ) );

            textBox_searchPat.Text = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_SearchPattern.ToString(), "*.*" );

            textBox_parseFilename.Text = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingFilename.ToString(), "" );
            PluginClassName = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingProvider.ToString(), "" );
            FuncName = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingProvideFunc.ToString(), "" );

            numericUpDown_currentIndex.Value = Convert.ToDecimal( UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.NextIndex.ToString(), 0 ) );
            LoadedFiles = UDataCarrier.GetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.LoadedFilePaths.ToString(), new string[ 0 ] );
            label_totalFiles.Text = $"/{LoadedFiles.Length}";

            foreach(var kv in IndexMapping)
            {
                if (kv.Value == PluginClassName)
                {
                    comboBox_pluginName.SelectedIndex = kv.Key;
                    break;
                }
            }

            return this;
        }

        private void comboBox_pluginName_SelectedIndexChanged( object sender, EventArgs e )
        {
            comboBox_pluginOpenedFunc.Items.Clear();
            if ( !( sender is ComboBox cb ) )
                return;
            if ( cb.SelectedIndex < 0 )
                return;

            if (IndexMapping.TryGetValue(cb.SelectedIndex, out var plugin))
            {
                var p = ULibAgent.Singleton.AssemblyPlugins.GetPluginInstanceByClassCSharpTypeName( plugin );
                foreach ( var kv in p.PluginClassFunctions )
                    comboBox_pluginOpenedFunc.Items.Add( kv.Key );
            }

            for(int i = 0; i < comboBox_pluginOpenedFunc.Items.Count; i++ )
            {
                if ( comboBox_pluginOpenedFunc.Items[i].ToString() == FuncName)
                {
                    FuncName = ""; // reset
                    comboBox_pluginOpenedFunc.SelectedIndex = i;
                    break;
                }
            }
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( WorkWith == null )
                return;

            // scan to settings
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_IsIncIndex.ToString(), checkBox_incIndex.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_EnableCycleRun.ToString(), checkBox_cycRun.Checked );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_LoadingPath.ToString(), textBox_folderPath.Text );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ResultJumpIndex.ToString(), Convert.ToInt32( numericUpDown_jumpIndex.Value ) );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_SearchPattern.ToString(), textBox_searchPat.Text );
            UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingFilename.ToString(), textBox_parseFilename.Text );

            if (IndexMapping.TryGetValue( comboBox_pluginName.SelectedIndex, out var plugFullName) && comboBox_pluginOpenedFunc.SelectedIndex >= 0)
            {
                UDataCarrier.SetDicKeyStrOne(
                    WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingProvider.ToString(),
                    plugFullName
                );
                UDataCarrier.SetDicKeyStrOne(
                    WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingProvideFunc.ToString(),
                    comboBox_pluginOpenedFunc.Items[ comboBox_pluginOpenedFunc.SelectedIndex ].ToString()
                );
            }
            else
            {
                UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingProvider.ToString(), "" );
                UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.Param_ParsingProvideFunc.ToString(), "" );
            }

            uMProvidImageLoader.ApplyFolderSettings( WorkWith );
        }

        private void button_openDir_Click( object sender, EventArgs e )
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if ( !string.IsNullOrEmpty( textBox_folderPath.Text ) )
                dlg.SelectedPath = textBox_folderPath.Text;
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                textBox_folderPath.Text = dlg.SelectedPath;
            }
            dlg.Dispose();
        }

        private void button_resetIndex_Click( object sender, EventArgs e )
        {
            if ( WorkWith == null )
                return;

            var index = Convert.ToInt32( numericUpDown_currentIndex.Value );
            if (index >= 0 && index < (LoadedFiles?.Length??0))
            {
                UDataCarrier.SetDicKeyStrOne( WorkWith.MutableInitialData, ImageFromMethodMutableDataKey.NextIndex.ToString(), index );
                MessageBox.Show($"Reset index to {index} ok!");
                return;
            }

            MessageBox.Show( $"Reset index fail!" );
        }
    }
}
