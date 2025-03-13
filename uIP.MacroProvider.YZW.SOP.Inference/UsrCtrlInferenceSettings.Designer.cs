namespace uIP.MacroProvider.YZW.SOP.Inference
{
    partial class UsrCtrlInferenceSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label_loadModelPath = new System.Windows.Forms.Label();
            this.textBox_modePath = new System.Windows.Forms.TextBox();
            this.button_loadMode = new System.Windows.Forms.Button();
            this.label_inputImage = new System.Windows.Forms.Label();
            this.numericUpDown_inputImageW = new System.Windows.Forms.NumericUpDown();
            this.label_inputImage_w = new System.Windows.Forms.Label();
            this.numericUpDown_inputImageH = new System.Windows.Forms.NumericUpDown();
            this.label_inputImage_h = new System.Windows.Forms.Label();
            this.numericUpDown_inputImageCHs = new System.Windows.Forms.NumericUpDown();
            this.label_inputImage_ch = new System.Windows.Forms.Label();
            this.label_inputCaptFrame = new System.Windows.Forms.Label();
            this.numericUpDown_inputCaptFrameW = new System.Windows.Forms.NumericUpDown();
            this.label_inputCaptFrame_w = new System.Windows.Forms.Label();
            this.numericUpDown_inputCaptFrameH = new System.Windows.Forms.NumericUpDown();
            this.label_inputCaptFrame_h = new System.Windows.Forms.Label();
            this.numericUpDown_inputCaptFrameCHs = new System.Windows.Forms.NumericUpDown();
            this.label_inputCaptFrame_ch = new System.Windows.Forms.Label();
            this.label_outputBatchNo = new System.Windows.Forms.Label();
            this.numericUpDown_outputBatchNo = new System.Windows.Forms.NumericUpDown();
            this.label_blockSize = new System.Windows.Forms.Label();
            this.numericUpDown_outputBlockSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_blockNo = new System.Windows.Forms.NumericUpDown();
            this.label_networkInput = new System.Windows.Forms.Label();
            this.textBox_networkInput = new System.Windows.Forms.TextBox();
            this.label_networkOutput = new System.Windows.Forms.Label();
            this.textBox_networkOutput = new System.Windows.Forms.TextBox();
            this.label_postBoxConfidence = new System.Windows.Forms.Label();
            this.numericUpDown_postBoxConfidence = new System.Windows.Forms.NumericUpDown();
            this.label_postNMSThreshold = new System.Windows.Forms.Label();
            this.numericUpDown_postNmsThreshold = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputImageW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputImageH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputImageCHs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputCaptFrameW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputCaptFrameH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputCaptFrameCHs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_outputBatchNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_outputBlockSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_blockNo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_postBoxConfidence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_postNmsThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // label_loadModelPath
            // 
            this.label_loadModelPath.AutoSize = true;
            this.label_loadModelPath.Location = new System.Drawing.Point(3, 6);
            this.label_loadModelPath.Name = "label_loadModelPath";
            this.label_loadModelPath.Size = new System.Drawing.Size(58, 12);
            this.label_loadModelPath.TabIndex = 0;
            this.label_loadModelPath.Text = "Model Path";
            // 
            // textBox_modePath
            // 
            this.textBox_modePath.Location = new System.Drawing.Point(93, 3);
            this.textBox_modePath.Name = "textBox_modePath";
            this.textBox_modePath.ReadOnly = true;
            this.textBox_modePath.Size = new System.Drawing.Size(272, 22);
            this.textBox_modePath.TabIndex = 1;
            // 
            // button_loadMode
            // 
            this.button_loadMode.Location = new System.Drawing.Point(371, 3);
            this.button_loadMode.Name = "button_loadMode";
            this.button_loadMode.Size = new System.Drawing.Size(75, 23);
            this.button_loadMode.TabIndex = 2;
            this.button_loadMode.Text = "Load";
            this.button_loadMode.UseVisualStyleBackColor = true;
            this.button_loadMode.Click += new System.EventHandler(this.button_loadMode_Click);
            // 
            // label_inputImage
            // 
            this.label_inputImage.AutoSize = true;
            this.label_inputImage.Location = new System.Drawing.Point(3, 33);
            this.label_inputImage.Name = "label_inputImage";
            this.label_inputImage.Size = new System.Drawing.Size(63, 12);
            this.label_inputImage.TabIndex = 3;
            this.label_inputImage.Text = "Input Model";
            // 
            // numericUpDown_inputImageW
            // 
            this.numericUpDown_inputImageW.Location = new System.Drawing.Point(131, 31);
            this.numericUpDown_inputImageW.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_inputImageW.Name = "numericUpDown_inputImageW";
            this.numericUpDown_inputImageW.Size = new System.Drawing.Size(64, 22);
            this.numericUpDown_inputImageW.TabIndex = 4;
            // 
            // label_inputImage_w
            // 
            this.label_inputImage_w.AutoSize = true;
            this.label_inputImage_w.Location = new System.Drawing.Point(91, 33);
            this.label_inputImage_w.Name = "label_inputImage_w";
            this.label_inputImage_w.Size = new System.Drawing.Size(34, 12);
            this.label_inputImage_w.TabIndex = 5;
            this.label_inputImage_w.Text = "Width";
            // 
            // numericUpDown_inputImageH
            // 
            this.numericUpDown_inputImageH.Location = new System.Drawing.Point(243, 31);
            this.numericUpDown_inputImageH.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_inputImageH.Name = "numericUpDown_inputImageH";
            this.numericUpDown_inputImageH.Size = new System.Drawing.Size(64, 22);
            this.numericUpDown_inputImageH.TabIndex = 4;
            // 
            // label_inputImage_h
            // 
            this.label_inputImage_h.AutoSize = true;
            this.label_inputImage_h.Location = new System.Drawing.Point(201, 33);
            this.label_inputImage_h.Name = "label_inputImage_h";
            this.label_inputImage_h.Size = new System.Drawing.Size(36, 12);
            this.label_inputImage_h.TabIndex = 5;
            this.label_inputImage_h.Text = "Height";
            // 
            // numericUpDown_inputImageCHs
            // 
            this.numericUpDown_inputImageCHs.Location = new System.Drawing.Point(367, 31);
            this.numericUpDown_inputImageCHs.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown_inputImageCHs.Name = "numericUpDown_inputImageCHs";
            this.numericUpDown_inputImageCHs.Size = new System.Drawing.Size(64, 22);
            this.numericUpDown_inputImageCHs.TabIndex = 4;
            // 
            // label_inputImage_ch
            // 
            this.label_inputImage_ch.AutoSize = true;
            this.label_inputImage_ch.Location = new System.Drawing.Point(313, 33);
            this.label_inputImage_ch.Name = "label_inputImage_ch";
            this.label_inputImage_ch.Size = new System.Drawing.Size(48, 12);
            this.label_inputImage_ch.TabIndex = 5;
            this.label_inputImage_ch.Text = "Channels";
            // 
            // label_inputCaptFrame
            // 
            this.label_inputCaptFrame.AutoSize = true;
            this.label_inputCaptFrame.Location = new System.Drawing.Point(3, 61);
            this.label_inputCaptFrame.Name = "label_inputCaptFrame";
            this.label_inputCaptFrame.Size = new System.Drawing.Size(62, 12);
            this.label_inputCaptFrame.TabIndex = 3;
            this.label_inputCaptFrame.Text = "Input Frame";
            // 
            // numericUpDown_inputCaptFrameW
            // 
            this.numericUpDown_inputCaptFrameW.Location = new System.Drawing.Point(131, 59);
            this.numericUpDown_inputCaptFrameW.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_inputCaptFrameW.Name = "numericUpDown_inputCaptFrameW";
            this.numericUpDown_inputCaptFrameW.Size = new System.Drawing.Size(64, 22);
            this.numericUpDown_inputCaptFrameW.TabIndex = 4;
            // 
            // label_inputCaptFrame_w
            // 
            this.label_inputCaptFrame_w.AutoSize = true;
            this.label_inputCaptFrame_w.Location = new System.Drawing.Point(91, 61);
            this.label_inputCaptFrame_w.Name = "label_inputCaptFrame_w";
            this.label_inputCaptFrame_w.Size = new System.Drawing.Size(34, 12);
            this.label_inputCaptFrame_w.TabIndex = 5;
            this.label_inputCaptFrame_w.Text = "Width";
            // 
            // numericUpDown_inputCaptFrameH
            // 
            this.numericUpDown_inputCaptFrameH.Location = new System.Drawing.Point(243, 59);
            this.numericUpDown_inputCaptFrameH.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown_inputCaptFrameH.Name = "numericUpDown_inputCaptFrameH";
            this.numericUpDown_inputCaptFrameH.Size = new System.Drawing.Size(64, 22);
            this.numericUpDown_inputCaptFrameH.TabIndex = 4;
            // 
            // label_inputCaptFrame_h
            // 
            this.label_inputCaptFrame_h.AutoSize = true;
            this.label_inputCaptFrame_h.Location = new System.Drawing.Point(201, 61);
            this.label_inputCaptFrame_h.Name = "label_inputCaptFrame_h";
            this.label_inputCaptFrame_h.Size = new System.Drawing.Size(36, 12);
            this.label_inputCaptFrame_h.TabIndex = 5;
            this.label_inputCaptFrame_h.Text = "Height";
            // 
            // numericUpDown_inputCaptFrameCHs
            // 
            this.numericUpDown_inputCaptFrameCHs.Location = new System.Drawing.Point(367, 59);
            this.numericUpDown_inputCaptFrameCHs.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDown_inputCaptFrameCHs.Name = "numericUpDown_inputCaptFrameCHs";
            this.numericUpDown_inputCaptFrameCHs.Size = new System.Drawing.Size(64, 22);
            this.numericUpDown_inputCaptFrameCHs.TabIndex = 4;
            // 
            // label_inputCaptFrame_ch
            // 
            this.label_inputCaptFrame_ch.AutoSize = true;
            this.label_inputCaptFrame_ch.Location = new System.Drawing.Point(313, 61);
            this.label_inputCaptFrame_ch.Name = "label_inputCaptFrame_ch";
            this.label_inputCaptFrame_ch.Size = new System.Drawing.Size(48, 12);
            this.label_inputCaptFrame_ch.TabIndex = 5;
            this.label_inputCaptFrame_ch.Text = "Channels";
            // 
            // label_outputBatchNo
            // 
            this.label_outputBatchNo.AutoSize = true;
            this.label_outputBatchNo.Location = new System.Drawing.Point(3, 89);
            this.label_outputBatchNo.Name = "label_outputBatchNo";
            this.label_outputBatchNo.Size = new System.Drawing.Size(52, 12);
            this.label_outputBatchNo.TabIndex = 6;
            this.label_outputBatchNo.Text = "Batch No.";
            // 
            // numericUpDown_outputBatchNo
            // 
            this.numericUpDown_outputBatchNo.Location = new System.Drawing.Point(93, 87);
            this.numericUpDown_outputBatchNo.Name = "numericUpDown_outputBatchNo";
            this.numericUpDown_outputBatchNo.Size = new System.Drawing.Size(102, 22);
            this.numericUpDown_outputBatchNo.TabIndex = 7;
            // 
            // label_blockSize
            // 
            this.label_blockSize.AutoSize = true;
            this.label_blockSize.Location = new System.Drawing.Point(3, 117);
            this.label_blockSize.Name = "label_blockSize";
            this.label_blockSize.Size = new System.Drawing.Size(55, 12);
            this.label_blockSize.TabIndex = 6;
            this.label_blockSize.Text = "Block Size";
            // 
            // numericUpDown_outputBlockSize
            // 
            this.numericUpDown_outputBlockSize.Location = new System.Drawing.Point(93, 115);
            this.numericUpDown_outputBlockSize.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.numericUpDown_outputBlockSize.Name = "numericUpDown_outputBlockSize";
            this.numericUpDown_outputBlockSize.Size = new System.Drawing.Size(102, 22);
            this.numericUpDown_outputBlockSize.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Block No.";
            // 
            // numericUpDown_blockNo
            // 
            this.numericUpDown_blockNo.Location = new System.Drawing.Point(93, 143);
            this.numericUpDown_blockNo.Name = "numericUpDown_blockNo";
            this.numericUpDown_blockNo.Size = new System.Drawing.Size(102, 22);
            this.numericUpDown_blockNo.TabIndex = 7;
            // 
            // label_networkInput
            // 
            this.label_networkInput.AutoSize = true;
            this.label_networkInput.Location = new System.Drawing.Point(3, 174);
            this.label_networkInput.Name = "label_networkInput";
            this.label_networkInput.Size = new System.Drawing.Size(70, 12);
            this.label_networkInput.TabIndex = 8;
            this.label_networkInput.Text = "NetworkInput";
            // 
            // textBox_networkInput
            // 
            this.textBox_networkInput.Location = new System.Drawing.Point(93, 171);
            this.textBox_networkInput.Name = "textBox_networkInput";
            this.textBox_networkInput.Size = new System.Drawing.Size(102, 22);
            this.textBox_networkInput.TabIndex = 9;
            // 
            // label_networkOutput
            // 
            this.label_networkOutput.AutoSize = true;
            this.label_networkOutput.Location = new System.Drawing.Point(201, 174);
            this.label_networkOutput.Name = "label_networkOutput";
            this.label_networkOutput.Size = new System.Drawing.Size(77, 12);
            this.label_networkOutput.TabIndex = 8;
            this.label_networkOutput.Text = "NetworkOutput";
            // 
            // textBox_networkOutput
            // 
            this.textBox_networkOutput.Location = new System.Drawing.Point(291, 171);
            this.textBox_networkOutput.Name = "textBox_networkOutput";
            this.textBox_networkOutput.Size = new System.Drawing.Size(102, 22);
            this.textBox_networkOutput.TabIndex = 9;
            // 
            // label_postBoxConfidence
            // 
            this.label_postBoxConfidence.AutoSize = true;
            this.label_postBoxConfidence.Location = new System.Drawing.Point(3, 201);
            this.label_postBoxConfidence.Name = "label_postBoxConfidence";
            this.label_postBoxConfidence.Size = new System.Drawing.Size(104, 12);
            this.label_postBoxConfidence.TabIndex = 10;
            this.label_postBoxConfidence.Text = "Post Box Confidence";
            // 
            // numericUpDown_postBoxConfidence
            // 
            this.numericUpDown_postBoxConfidence.Location = new System.Drawing.Point(113, 199);
            this.numericUpDown_postBoxConfidence.Name = "numericUpDown_postBoxConfidence";
            this.numericUpDown_postBoxConfidence.Size = new System.Drawing.Size(82, 22);
            this.numericUpDown_postBoxConfidence.TabIndex = 11;
            // 
            // label_postNMSThreshold
            // 
            this.label_postNMSThreshold.AutoSize = true;
            this.label_postNMSThreshold.Location = new System.Drawing.Point(201, 201);
            this.label_postNMSThreshold.Name = "label_postNMSThreshold";
            this.label_postNMSThreshold.Size = new System.Drawing.Size(101, 12);
            this.label_postNMSThreshold.TabIndex = 10;
            this.label_postNMSThreshold.Text = "Post NMS Threshold";
            // 
            // numericUpDown_postNmsThreshold
            // 
            this.numericUpDown_postNmsThreshold.Location = new System.Drawing.Point(311, 199);
            this.numericUpDown_postNmsThreshold.Name = "numericUpDown_postNmsThreshold";
            this.numericUpDown_postNmsThreshold.Size = new System.Drawing.Size(82, 22);
            this.numericUpDown_postNmsThreshold.TabIndex = 11;
            // 
            // UsrCtrlInferenceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numericUpDown_postNmsThreshold);
            this.Controls.Add(this.numericUpDown_postBoxConfidence);
            this.Controls.Add(this.label_postNMSThreshold);
            this.Controls.Add(this.label_postBoxConfidence);
            this.Controls.Add(this.textBox_networkOutput);
            this.Controls.Add(this.textBox_networkInput);
            this.Controls.Add(this.label_networkOutput);
            this.Controls.Add(this.label_networkInput);
            this.Controls.Add(this.numericUpDown_blockNo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown_outputBlockSize);
            this.Controls.Add(this.label_blockSize);
            this.Controls.Add(this.numericUpDown_outputBatchNo);
            this.Controls.Add(this.label_outputBatchNo);
            this.Controls.Add(this.label_inputCaptFrame_ch);
            this.Controls.Add(this.label_inputImage_ch);
            this.Controls.Add(this.numericUpDown_inputCaptFrameCHs);
            this.Controls.Add(this.label_inputCaptFrame_h);
            this.Controls.Add(this.numericUpDown_inputImageCHs);
            this.Controls.Add(this.numericUpDown_inputCaptFrameH);
            this.Controls.Add(this.label_inputImage_h);
            this.Controls.Add(this.label_inputCaptFrame_w);
            this.Controls.Add(this.numericUpDown_inputImageH);
            this.Controls.Add(this.numericUpDown_inputCaptFrameW);
            this.Controls.Add(this.label_inputImage_w);
            this.Controls.Add(this.label_inputCaptFrame);
            this.Controls.Add(this.numericUpDown_inputImageW);
            this.Controls.Add(this.label_inputImage);
            this.Controls.Add(this.button_loadMode);
            this.Controls.Add(this.textBox_modePath);
            this.Controls.Add(this.label_loadModelPath);
            this.Name = "UsrCtrlInferenceSettings";
            this.Size = new System.Drawing.Size(449, 226);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputImageW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputImageH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputImageCHs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputCaptFrameW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputCaptFrameH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_inputCaptFrameCHs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_outputBatchNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_outputBlockSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_blockNo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_postBoxConfidence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_postNmsThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_loadModelPath;
        private System.Windows.Forms.TextBox textBox_modePath;
        private System.Windows.Forms.Button button_loadMode;
        private System.Windows.Forms.Label label_inputImage;
        private System.Windows.Forms.NumericUpDown numericUpDown_inputImageW;
        private System.Windows.Forms.Label label_inputImage_w;
        private System.Windows.Forms.NumericUpDown numericUpDown_inputImageH;
        private System.Windows.Forms.Label label_inputImage_h;
        private System.Windows.Forms.NumericUpDown numericUpDown_inputImageCHs;
        private System.Windows.Forms.Label label_inputImage_ch;
        private System.Windows.Forms.Label label_inputCaptFrame;
        private System.Windows.Forms.NumericUpDown numericUpDown_inputCaptFrameW;
        private System.Windows.Forms.Label label_inputCaptFrame_w;
        private System.Windows.Forms.NumericUpDown numericUpDown_inputCaptFrameH;
        private System.Windows.Forms.Label label_inputCaptFrame_h;
        private System.Windows.Forms.NumericUpDown numericUpDown_inputCaptFrameCHs;
        private System.Windows.Forms.Label label_inputCaptFrame_ch;
        private System.Windows.Forms.Label label_outputBatchNo;
        private System.Windows.Forms.NumericUpDown numericUpDown_outputBatchNo;
        private System.Windows.Forms.Label label_blockSize;
        private System.Windows.Forms.NumericUpDown numericUpDown_outputBlockSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_blockNo;
        private System.Windows.Forms.Label label_networkInput;
        private System.Windows.Forms.TextBox textBox_networkInput;
        private System.Windows.Forms.Label label_networkOutput;
        private System.Windows.Forms.TextBox textBox_networkOutput;
        private System.Windows.Forms.Label label_postBoxConfidence;
        private System.Windows.Forms.NumericUpDown numericUpDown_postBoxConfidence;
        private System.Windows.Forms.Label label_postNMSThreshold;
        private System.Windows.Forms.NumericUpDown numericUpDown_postNmsThreshold;
    }
}
