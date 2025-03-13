using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Inference
{
    public partial class UsrCtrlInferenceSettings : UserControl
    {
        internal string ModelPath
        {
            get => textBox_modePath.Text;
            set => textBox_modePath.Text = value;
        }

        internal int InputImageW
        {
            get => Convert.ToInt32( numericUpDown_inputImageW.Value );
            set => numericUpDown_inputImageW.Value = Convert.ToDecimal( value );
        }

        internal int InputImageH
        {
            get => Convert.ToInt32( numericUpDown_inputImageH.Value );
            set => numericUpDown_inputImageH.Value = Convert.ToDecimal( value );
        }

        internal int InputImageCHs
        {
            get => Convert.ToInt32( numericUpDown_inputImageCHs.Value );
            set => numericUpDown_inputImageCHs.Value = Convert.ToDecimal( value );
        }

        internal int InputCaptFrameW
        {
            get => Convert.ToInt32( numericUpDown_inputCaptFrameW.Value );
            set => numericUpDown_inputCaptFrameW.Value = Convert.ToDecimal( value );
        }

        internal int InputCaptFrameH
        {
            get => Convert.ToInt32( numericUpDown_inputCaptFrameH.Value );
            set => numericUpDown_inputCaptFrameH.Value = Convert.ToDecimal( value );
        }

        internal int InputCaptFrameCHs
        {
            get => Convert.ToInt32( numericUpDown_inputCaptFrameCHs.Value ) ;
            set => numericUpDown_inputCaptFrameCHs.Value = Convert.ToDecimal( value );
        }

        internal int OutputBatchNo
        {
            get => Convert.ToInt32( numericUpDown_outputBatchNo.Value );
            set => numericUpDown_outputBatchNo.Value = Convert.ToDecimal( value );
        }

        internal int OutputBlockSize
        {
            get => Convert.ToInt32( numericUpDown_outputBlockSize.Value );
            set => numericUpDown_outputBlockSize.Value = Convert.ToDecimal( value );
        }

        internal int OutputBlockNo
        {
            get => Convert.ToInt32( numericUpDown_blockNo.Value );
            set => numericUpDown_blockNo.Value = Convert.ToDecimal( value );
        }

        internal string NetworkInput
        {
            get => textBox_networkInput.Text;
            set => textBox_networkInput.Text = value;
        }

        internal string NetworkOutput
        {
            get => textBox_networkOutput.Text;
            set => textBox_networkOutput.Text = value;
        }

        internal float PostBoxConfidence
        {
            get => Convert.ToSingle( numericUpDown_postBoxConfidence.Value ) / Convert.ToSingle( 100 );
            set
            {
                value = value < 0 ? 0 : value * Convert.ToSingle( 100 );
                value = value > 100 ? 100 : value;
                numericUpDown_postBoxConfidence.Value = Convert.ToDecimal( value );
            }
        }

        internal float PostNMSThreshold
        {
            get => Convert.ToSingle( numericUpDown_postNmsThreshold.Value ) / Convert.ToSingle( 100 );
            set
            {
                value *= Convert.ToSingle( 100 );
                value = value < 0 ? 0 : ( value > 100 ? 100 : value );
                numericUpDown_postNmsThreshold.Value = Convert.ToDecimal( value );
            }
        }

        private UMacro m_Macro { get; set; }

        internal UMacro Macro
        {
            set
            {
                if ( value != null && value.MutableInitialData != null && ( value.MutableInitialData.Data is ModelData md ) && md != null )
                {
                    ModelPath = md.EncryptModelFilepath;
                    InputImageW = md.InputImgResolutionW;
                    InputImageH = md.InputImgResolutionH;
                    InputImageCHs = md.InputImgChannels;
                    InputCaptFrameW = md.FrameResolutionW;
                    InputCaptFrameH = md.FrameResolutionH;
                    InputCaptFrameCHs = md.FrameImgChannels;
                    OutputBatchNo = md.OutputBatchNo;
                    OutputBlockSize = md.OutputBlockSize;
                    OutputBlockNo = md.OutputBlockNo;
                    NetworkInput = md.NetworkInput;
                    NetworkOutput = md.NetworkOutput;
                    PostBoxConfidence = Convert.ToSingle( md.PostProcBoxConfidence );
                    PostNMSThreshold = Convert.ToSingle( md.PostProcNMSThreshold );
                    m_Macro = value;
                }
            }
            get => m_Macro;
        }

        public UsrCtrlInferenceSettings()
        {
            InitializeComponent();
        }

        private void button_loadMode_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            var updateTxt = "";
            if ( dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty( dlg.FileName ) )
            {
                // only update
                if ( uMProvidModelSopInference.SetModelPath( m_Macro, dlg.FileName ) )
                    updateTxt = UDataCarrier.Get( m_Macro.MutableInitialData, new ModelData() ).EncryptModelFilepath;
            }
            dlg.Dispose();
            if ( !string.IsNullOrEmpty( updateTxt ) )
                textBox_modePath.Text = updateTxt;
        }
    }
}
