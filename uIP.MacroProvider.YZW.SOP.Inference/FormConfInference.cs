using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Inference
{
    public partial class FormConfInference : Form
    {
        UsrCtrlInferenceSettings Settings { get; set; }
        public FormConfInference()
        {
            InitializeComponent();
            var dummy = new UsrCtrlInferenceSettings();
            dummy.Location = new Point( 0, 0 );
            Controls.Add( dummy );
        }
        public FormConfInference(UMacro m)
        {
            InitializeComponent();

            var btnOkRB = new Point( button_ok.Location.X + button_ok.Width, button_ok.Location.Y + button_ok.Height );
            var offset = new Point( btnOkRB.X - ClientSize.Width, btnOkRB.Y - ClientSize.Height );

            if ( !string.IsNullOrEmpty( m.OwnerOfScript?.NameOfId ?? "" ) )
                Text += $" of {m.OwnerOfScript.NameOfId}";

            Settings = new UsrCtrlInferenceSettings() { Macro = m };
            Settings.Location = new Point(0, 0);
            Controls.Add(Settings);

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
            // get config back and config to inference
            object obj = Settings.Macro?.MutableInitialData?.Data ?? null;
            if ( obj != null && obj is ModelData md )
            {
                md.InputImgResolutionW = Settings.InputImageW;
                md.InputImgResolutionH = Settings.InputImageH;
                md.InputImgChannels = Settings.InputImageCHs;

                md.FrameResolutionW = Settings.InputCaptFrameW;
                md.FrameResolutionH = Settings.InputCaptFrameH;
                md.FrameImgChannels = Settings.InputCaptFrameCHs;

                md.OutputBatchNo = Settings.OutputBatchNo;
                md.OutputBlockSize = Settings.OutputBlockSize;
                md.OutputBlockNo = Settings.OutputBlockNo;

                md.NetworkInput = string.IsNullOrEmpty( Settings.NetworkInput ) ? "images" : string.Copy( Settings.NetworkInput );
                md.NetworkOutput = string.IsNullOrEmpty( Settings.NetworkOutput ) ? "output0" : string.Copy( Settings.NetworkOutput );

                md.PostProcBoxConfidence = Settings.PostBoxConfidence;
                md.PostProcNMSThreshold = Settings.PostNMSThreshold;
            }

            // teach model
            if ( !uMProvidModelSopInference.TeachModel( Settings.Macro, out var errS ) )
                MessageBox.Show( $"Error: {errS}" );
        }
    }
}
