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

namespace uIP.MacroProvider.Commons.FlowControl
{
    public partial class FormSetupJumpInScript : Form
    {
        internal UMacro RunWith { get; set; }
        internal Dictionary<string, List<string>> OpenedFuncs = new Dictionary<string, List<string>>();
        public FormSetupJumpInScript()
        {
            InitializeComponent();

            var jtypes = Enum.GetNames( typeof( JumpSameScriptParams.TypeOfJump ) );
            foreach ( var t in jtypes )
                comboBox_jumpTypeSelect.Items.Add( t );

        }

        internal FormSetupJumpInScript UpdateToUI()
        {
            if ( RunWith == null )
                return this;

            if ( !UDataCarrier.Get<JumpSameScriptParams>( RunWith.MutableInitialData, null, out var conf ) )
                return this;

            comboBox_jumpTypeSelect.SelectedIndex = ( int ) conf.JumpType;
            numericUpDown_fixJumpIndex.Value = conf.WhichIndex;
            textBox_jumpScript.Text = conf.WhichScriptToJump;

            List<string> available = new List<string>();
            foreach( var a in ULibAgent.Singleton.AssemblyPlugins.PluginAssemblies)
            {
                foreach(var fo in a.PluginClassFunctions )
                {
                    available.Add( fo.Key );
                }

                if ( available.Count > 0 )
                    OpenedFuncs.Add( a.NameOfCSharpDefClass, available );
            }

            comboBox_loadedAssemblies.Items.Clear();
            comboBox_avaliableFunc.Items.Clear();

            int pluginIndex = -1;
            foreach(var kv in OpenedFuncs)
            {
                comboBox_loadedAssemblies.Items.Add( kv.Key );
                if (conf.CallWhichPluginFullName == kv.Key)
                {
                    pluginIndex = comboBox_loadedAssemblies.Items.Count - 1;
                }
            }

            if (pluginIndex >= 0)
            {
                comboBox_loadedAssemblies.SelectedIndex = pluginIndex;
                int funcIndex = -1;
                for ( int i = 0; i < ( comboBox_avaliableFunc.Items?.Count ?? 0 ); i++ )
                {
                    if ( comboBox_avaliableFunc.Items[ i ].ToString() == conf.CallWhichPluginOfFunc )
                    {
                        funcIndex = i;
                        break;
                    }
                }

                if (funcIndex>=0)
                    comboBox_avaliableFunc.SelectedIndex = funcIndex;
            }

            return this;
        }

        private void comboBox_loadedAssemblies_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !(sender is ComboBox cb) || cb.SelectedIndex < 0 )
                return;

            comboBox_avaliableFunc.Items.Clear();
            if ( OpenedFuncs.TryGetValue( cb.Items[cb.SelectedIndex].ToString(), out var funcs))
            {
                foreach ( var f in funcs )
                {
                    comboBox_avaliableFunc.Items.Add( f );
                }
            }
        }

        private void comboBox_avaliableFunc_SelectedIndexChanged( object sender, EventArgs e )
        {
            richTextBox_functionInfo.Clear();

            if ( !( sender is ComboBox cb ) || cb.SelectedIndex < 0 )
            {
                return;
            }

            var plugin = ULibAgent.Singleton.AssemblyPlugins.GetPluginInstanceByClassCSharpTypeName( comboBox_loadedAssemblies.Items[ comboBox_loadedAssemblies.SelectedIndex ].ToString() );
            if ( plugin.PluginClassFunctions.TryGetValue( cb.Items[cb.SelectedIndex].ToString(), out var info))
            {
                richTextBox_functionInfo.AppendText( "Description:\n" );
                richTextBox_functionInfo.AppendText( $"{info.Description}\n" );
                richTextBox_functionInfo.AppendText( "\n" );
                richTextBox_functionInfo.AppendText( "Input:\n" );
                richTextBox_functionInfo.AppendText( $"{( string.Join( "\n", info.ArgsDescription ) )}" );
                richTextBox_functionInfo.AppendText( "\n" );
                richTextBox_functionInfo.AppendText( "Return:\n" );
                richTextBox_functionInfo.AppendText( $"{info.ReturnValueDescription}" );
            }
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( RunWith == null )
                return;

            if ( !UDataCarrier.Get<JumpSameScriptParams>( RunWith.MutableInitialData, null, out var conf ) )
                return;


            conf.JumpType = ( JumpSameScriptParams.TypeOfJump ) Enum.Parse( typeof( JumpSameScriptParams.TypeOfJump ), comboBox_jumpTypeSelect.Items[ comboBox_jumpTypeSelect.SelectedIndex ].ToString() );
            conf.WhichIndex = Convert.ToInt32(numericUpDown_fixJumpIndex.Value );
            conf.WhichScriptToJump = string.IsNullOrEmpty(textBox_jumpScript.Text) ? "" : string.Copy(textBox_jumpScript.Text);

            if (comboBox_avaliableFunc.SelectedIndex >= 0)
            {
                conf.CallWhichPluginFullName = comboBox_loadedAssemblies.Items[ comboBox_loadedAssemblies.SelectedIndex ].ToString();
                conf.CallWhichPluginOfFunc = comboBox_avaliableFunc.Items[ comboBox_avaliableFunc.SelectedIndex ].ToString();
            }

            JumpSameScriptParams.ConfigMacroJump( RunWith as UMacroCapableOfCtrlFlow, conf );
        }
    }
}
