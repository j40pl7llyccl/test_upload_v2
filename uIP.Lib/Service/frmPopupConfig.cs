using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public partial class frmPopupConfig : Form
    {
        UPluginAssemblyService _PS = null;
        internal UPluginAssemblyService PS {  get { return _PS; } set { _PS = value; UpdateAssembliesList(); } }

        public frmPopupConfig()
        {
            InitializeComponent();
        }

        void UpdateAssembliesList()
        {
            if ( _PS == null ) return;
            comboBox_pluginClassList.Items.Clear();

            if ( _PS.PluginAssemblies == null || _PS.PluginAssemblies.Count <= 0 )
                return;

            for ( int i = 0; i < _PS.PluginAssemblies.Count; i++ )
                comboBox_pluginClassList.Items.Add( _PS.PluginAssemblies[ i ].GivenName );
        }

        private void button_popupConfig_Click( object sender, EventArgs e )
        {
            if ( comboBox_pluginClassList.SelectedIndex < 0 )
                return;

            if ( _PS == null )
                return;

            string nm = comboBox_pluginClassList.Items[ comboBox_pluginClassList.SelectedIndex ] as string;
            _PS.SetPluginClassControl( nm, UMacroMethodProviderPlugin.PredefClassIoctl_ParamGUI, null );
        }
    }
}
