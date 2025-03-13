using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public partial class FormConfRegions : Form
    {
        UMacro ConfMacro { get; set; }
        UsrCtrlRegionEditor Settings { get; set; }

        public FormConfRegions()
        {
            InitializeComponent();
            var dummy = new UsrCtrlRegionEditor();
            dummy.Location = new Point( 0, 0 );
            Controls.Add( dummy );
        }

        public FormConfRegions(UMacro macro)
        {
            InitializeComponent();
            ConfMacro = macro;
            var btnOkRB = new Point( button_ok.Location.X + button_ok.Width, button_ok.Location.Y + button_ok.Height );
            var offset = new Point( btnOkRB.X - ClientSize.Width, btnOkRB.Y - ClientSize.Height );

            if ( UDataCarrier.Get<EvaluationConf>( macro.MutableInitialData, null, out var conf ) && conf != null )
                Settings = new UsrCtrlRegionEditor( conf.Regions, conf.BackgroundImagePath );
            else
                Settings = new UsrCtrlRegionEditor();

            if ( !string.IsNullOrEmpty( macro.OwnerOfScript?.NameOfId ?? "" ) )
                Text += $" of {macro.OwnerOfScript.NameOfId}";

            Settings.Location = new Point( 0, 0 );
            Controls.Add( Settings );

            int clientW = Settings.Width;
            int clientH = Settings.Height;
            clientH += ( 2 * ( offset.Y < 0 ? -offset.Y : offset.Y ) + button_ok.Height );

            ClientSize = new Size( clientW, clientH );

            button_ok.Location = new Point( ClientSize.Width + offset.X - button_ok.Width, ClientSize.Height + offset.Y - button_ok.Height );
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( ConfMacro == null )
                return;
            if ( !( ConfMacro.OwnerOfPluginClass is uMProvidSopDetect d ) || d == null )
                return;

            if (!Directory.Exists(d.DataRWDir))
            {
                MessageBox.Show( $"Dir {d.DataRWDir} not exist!" );
                return;
            }

            if (!UDataCarrier.Get<EvaluationConf>(ConfMacro.MutableInitialData, null, out var conf))
            {
                MessageBox.Show( "Convert type fail!" );
                return;
            }

            if (Settings.BackgroundBmp == null)
            {
                MessageBox.Show( "Background image not ready to config!" );
                return;
            }

            if (!conf.SetBackgroundImage(Settings.BackgroundBmp, d.DataRWDir))
            {
                MessageBox.Show( "Write image fail!" );
            }

            conf.Regions = Settings.Regions;

            Settings.Regions = null;
        }
    }
}
