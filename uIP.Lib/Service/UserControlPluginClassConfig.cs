using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public partial class UserControlPluginClassConfig : UserControl
    {
        UPluginAssemblyService _PS = null;

        internal UPluginAssemblyService PS {  get { return _PS; } set { _PS = value; UpdateTabControl(); } }

        public UserControlPluginClassConfig()
        {
            InitializeComponent();
        }

        public int NumberOfConf {  get { return tabControl_pluginClasses.TabCount; } }

        void UpdateTabControl()
        {
            tabControl_pluginClasses.TabPages.Clear();

            if ( _PS != null && _PS.PluginAssemblies != null && _PS.PluginAssemblies.Count > 0 ) {
                for ( int i = 0; i < _PS.PluginAssemblies.Count; i++ ) {
                    bool stat;
                    UDataCarrier[] got = _PS.GetPluginClassControl( _PS.PluginAssemblies[ i ].GivenName, UMacroMethodProviderPlugin.PredefClassIoctl_ParamGUI, out stat );
                    Control ctrl = null;
                    if (stat && got != null && got.Length > 0 && got[0] != null) {
                        ctrl = got[ 0 ].Data as Control;
                    }
                    if ( ctrl == null )
                        continue;
                    tabControl_pluginClasses.TabPages.Add( _PS.PluginAssemblies[i].NameOfCSharpDefClass, _PS.PluginAssemblies[ i ].GetType().Name);
                    tabControl_pluginClasses.TabPages[ _PS.PluginAssemblies[ i ].NameOfCSharpDefClass ].AutoScroll = true;

                    if (ctrl is Form) {
                        Form frm = ctrl as Form;
                        frm.FormBorderStyle = FormBorderStyle.None;
                        frm.TopLevel = false;
                        frm.Show();
                    }
                    ctrl.Location = new Point( 0, 0 );
                    tabControl_pluginClasses.TabPages[ _PS.PluginAssemblies[ i ].NameOfCSharpDefClass ].Controls.Add( ctrl );
                }
            }
        }

    }
}
