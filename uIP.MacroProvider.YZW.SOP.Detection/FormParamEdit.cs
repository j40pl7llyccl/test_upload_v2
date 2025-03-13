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

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public partial class FormParamEdit : Form
    {
        internal UMacro ConfWith { get; set; }
        internal UserControlSopDetectParams ParamControl { get; set; }
        public FormParamEdit()
        {
            InitializeComponent();

            var okBtnOffset = new Point( button_ok.Location.X - ClientSize.Width, button_ok.Location.Y - ClientSize.Height );
            var cancelBtnOffset = new Point(button_cancel.Location.X - ClientSize.Width, button_cancel.Location.Y - ClientSize.Height );

            ParamControl = new UserControlSopDetectParams() { Location = new Point( 3, 3 ) };
            Controls.Add( ParamControl );

            ClientSize = new Size( ParamControl.Location.X + ParamControl.Width + 3, ParamControl.Location.X + ParamControl.Height + 3 + (-okBtnOffset.Y) );
            button_ok.Location = new Point( ClientSize.Width + okBtnOffset.X, ClientSize.Height + okBtnOffset.Y );
            button_cancel.Location = new Point( ClientSize.Width + cancelBtnOffset.X, ClientSize.Height + cancelBtnOffset.Y );
        }

        internal FormParamEdit ReloadParameters(UMacro m)
        {
            if (!UDataCarrier.Get<EvaluationConf>( m.MutableInitialData, null, out var conf ))
            {
                MessageBox.Show( "invalid mutable data!" );
                return this;
            }
            ConfWith = m;
            ParamControl.ReloadSettings( DetectionParameters.Clone( conf.Parameters ) );
            ParamControl.ReferenceImageFilepath = conf.BackgroundImagePath;
            if (m.OwnerOfPluginClass is uMProvidSopDetect owner)
                ParamControl.DataRWDir = owner.DataRWDir;
            return this;
        }

        private void button_ok_Click( object sender, EventArgs e )
        {
            if ( UDataCarrier.Get<EvaluationConf>( ConfWith?.MutableInitialData ?? null, null, out var conf ) )
            {
                ParamControl.UpdateSettings();
                // replace
                conf.Parameters = ParamControl.Settings;
                // remove prev
                if ( File.Exists( conf.BackgroundImagePath ) )
                {
                    try { File.Delete( conf.BackgroundImagePath ); }
                    catch { }
                }
                // config new
                conf.SetBackgroundImage( ParamControl.ReferenceBackground, ParamControl.DataRWDir );
                // config to dll
                conf.ApplyParameters();
            }
        }
    }
}
