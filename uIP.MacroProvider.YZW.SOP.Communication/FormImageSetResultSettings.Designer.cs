namespace uIP.MacroProvider.YZW.SOP.Communication
{
    partial class FormImageSetResultSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBox_writeDB = new System.Windows.Forms.CheckBox();
            this.button_pickSaveDir = new System.Windows.Forms.Button();
            this.textBox_saveImageDir = new System.Windows.Forms.TextBox();
            this.checkBox_saveResultImage = new System.Windows.Forms.CheckBox();
            this.label_contIndex = new System.Windows.Forms.Label();
            this.numericUpDown_contIndex = new System.Windows.Forms.NumericUpDown();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.checkBox_logMsg = new System.Windows.Forms.CheckBox();
            this.comboBox_saveImageFormat = new System.Windows.Forms.ComboBox();
            this.label_saveImageFormat = new System.Windows.Forms.Label();
            this.checkBox_fromVideo = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_contIndex)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBox_writeDB
            // 
            this.checkBox_writeDB.AutoSize = true;
            this.checkBox_writeDB.Location = new System.Drawing.Point(12, 66);
            this.checkBox_writeDB.Name = "checkBox_writeDB";
            this.checkBox_writeDB.Size = new System.Drawing.Size(97, 16);
            this.checkBox_writeDB.TabIndex = 8;
            this.checkBox_writeDB.Text = "Write DB result";
            this.checkBox_writeDB.UseVisualStyleBackColor = true;
            // 
            // button_pickSaveDir
            // 
            this.button_pickSaveDir.Location = new System.Drawing.Point(285, 14);
            this.button_pickSaveDir.Name = "button_pickSaveDir";
            this.button_pickSaveDir.Size = new System.Drawing.Size(75, 23);
            this.button_pickSaveDir.TabIndex = 7;
            this.button_pickSaveDir.Text = "Pick save dir";
            this.button_pickSaveDir.UseVisualStyleBackColor = true;
            this.button_pickSaveDir.Click += new System.EventHandler(this.button_pickSaveDir_Click);
            // 
            // textBox_saveImageDir
            // 
            this.textBox_saveImageDir.Location = new System.Drawing.Point(96, 12);
            this.textBox_saveImageDir.Name = "textBox_saveImageDir";
            this.textBox_saveImageDir.Size = new System.Drawing.Size(183, 22);
            this.textBox_saveImageDir.TabIndex = 6;
            // 
            // checkBox_saveResultImage
            // 
            this.checkBox_saveResultImage.AutoSize = true;
            this.checkBox_saveResultImage.Location = new System.Drawing.Point(12, 14);
            this.checkBox_saveResultImage.Name = "checkBox_saveResultImage";
            this.checkBox_saveResultImage.Size = new System.Drawing.Size(77, 16);
            this.checkBox_saveResultImage.TabIndex = 5;
            this.checkBox_saveResultImage.Text = "Save image";
            this.checkBox_saveResultImage.UseVisualStyleBackColor = true;
            // 
            // label_contIndex
            // 
            this.label_contIndex.AutoSize = true;
            this.label_contIndex.Location = new System.Drawing.Point(10, 90);
            this.label_contIndex.Name = "label_contIndex";
            this.label_contIndex.Size = new System.Drawing.Size(61, 12);
            this.label_contIndex.TabIndex = 9;
            this.label_contIndex.Text = "Cont. Index";
            // 
            // numericUpDown_contIndex
            // 
            this.numericUpDown_contIndex.Location = new System.Drawing.Point(96, 88);
            this.numericUpDown_contIndex.Name = "numericUpDown_contIndex";
            this.numericUpDown_contIndex.Size = new System.Drawing.Size(102, 22);
            this.numericUpDown_contIndex.TabIndex = 10;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(160, 130);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(98, 41);
            this.button_ok.TabIndex = 11;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(264, 130);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 41);
            this.button_cancel.TabIndex = 12;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // checkBox_logMsg
            // 
            this.checkBox_logMsg.AutoSize = true;
            this.checkBox_logMsg.Location = new System.Drawing.Point(12, 118);
            this.checkBox_logMsg.Name = "checkBox_logMsg";
            this.checkBox_logMsg.Size = new System.Drawing.Size(84, 16);
            this.checkBox_logMsg.TabIndex = 13;
            this.checkBox_logMsg.Text = "Log message";
            this.checkBox_logMsg.UseVisualStyleBackColor = true;
            // 
            // comboBox_saveImageFormat
            // 
            this.comboBox_saveImageFormat.FormattingEnabled = true;
            this.comboBox_saveImageFormat.Location = new System.Drawing.Point(110, 40);
            this.comboBox_saveImageFormat.Name = "comboBox_saveImageFormat";
            this.comboBox_saveImageFormat.Size = new System.Drawing.Size(88, 20);
            this.comboBox_saveImageFormat.TabIndex = 15;
            // 
            // label_saveImageFormat
            // 
            this.label_saveImageFormat.AutoSize = true;
            this.label_saveImageFormat.Location = new System.Drawing.Point(12, 43);
            this.label_saveImageFormat.Name = "label_saveImageFormat";
            this.label_saveImageFormat.Size = new System.Drawing.Size(92, 12);
            this.label_saveImageFormat.TabIndex = 14;
            this.label_saveImageFormat.Text = "Save image format";
            // 
            // checkBox_fromVideo
            // 
            this.checkBox_fromVideo.AutoSize = true;
            this.checkBox_fromVideo.Location = new System.Drawing.Point(204, 42);
            this.checkBox_fromVideo.Name = "checkBox_fromVideo";
            this.checkBox_fromVideo.Size = new System.Drawing.Size(108, 16);
            this.checkBox_fromVideo.TabIndex = 16;
            this.checkBox_fromVideo.Text = "Image from video";
            this.checkBox_fromVideo.UseVisualStyleBackColor = true;
            // 
            // FormImageSetResultSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 183);
            this.Controls.Add(this.checkBox_fromVideo);
            this.Controls.Add(this.comboBox_saveImageFormat);
            this.Controls.Add(this.label_saveImageFormat);
            this.Controls.Add(this.checkBox_logMsg);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.numericUpDown_contIndex);
            this.Controls.Add(this.label_contIndex);
            this.Controls.Add(this.checkBox_writeDB);
            this.Controls.Add(this.button_pickSaveDir);
            this.Controls.Add(this.textBox_saveImageDir);
            this.Controls.Add(this.checkBox_saveResultImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormImageSetResultSettings";
            this.Text = "FormImageSetResultSettings";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_contIndex)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_writeDB;
        private System.Windows.Forms.Button button_pickSaveDir;
        private System.Windows.Forms.TextBox textBox_saveImageDir;
        private System.Windows.Forms.CheckBox checkBox_saveResultImage;
        private System.Windows.Forms.Label label_contIndex;
        private System.Windows.Forms.NumericUpDown numericUpDown_contIndex;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.CheckBox checkBox_logMsg;
        private System.Windows.Forms.ComboBox comboBox_saveImageFormat;
        private System.Windows.Forms.Label label_saveImageFormat;
        private System.Windows.Forms.CheckBox checkBox_fromVideo;
    }
}