namespace uIP.MacroProvider.YZW.SOP.Communication
{
    partial class FormBufferResultSettings
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
            this.checkBox_saveResultImage = new System.Windows.Forms.CheckBox();
            this.textBox_saveImgeRoot = new System.Windows.Forms.TextBox();
            this.button_pickSaveDir = new System.Windows.Forms.Button();
            this.checkBox_writeDB = new System.Windows.Forms.CheckBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_saveImageFormat = new System.Windows.Forms.Label();
            this.comboBox_saveImageFormat = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // checkBox_saveResultImage
            // 
            this.checkBox_saveResultImage.AutoSize = true;
            this.checkBox_saveResultImage.Location = new System.Drawing.Point(12, 12);
            this.checkBox_saveResultImage.Name = "checkBox_saveResultImage";
            this.checkBox_saveResultImage.Size = new System.Drawing.Size(77, 16);
            this.checkBox_saveResultImage.TabIndex = 0;
            this.checkBox_saveResultImage.Text = "Save image";
            this.checkBox_saveResultImage.UseVisualStyleBackColor = true;
            // 
            // textBox_saveImgeRoot
            // 
            this.textBox_saveImgeRoot.Location = new System.Drawing.Point(96, 10);
            this.textBox_saveImgeRoot.Name = "textBox_saveImgeRoot";
            this.textBox_saveImgeRoot.Size = new System.Drawing.Size(183, 22);
            this.textBox_saveImgeRoot.TabIndex = 2;
            // 
            // button_pickSaveDir
            // 
            this.button_pickSaveDir.Location = new System.Drawing.Point(285, 12);
            this.button_pickSaveDir.Name = "button_pickSaveDir";
            this.button_pickSaveDir.Size = new System.Drawing.Size(75, 23);
            this.button_pickSaveDir.TabIndex = 3;
            this.button_pickSaveDir.Text = "Pick save dir";
            this.button_pickSaveDir.UseVisualStyleBackColor = true;
            this.button_pickSaveDir.Click += new System.EventHandler(this.button_pickSaveDir_Click);
            // 
            // checkBox_writeDB
            // 
            this.checkBox_writeDB.AutoSize = true;
            this.checkBox_writeDB.Location = new System.Drawing.Point(12, 74);
            this.checkBox_writeDB.Name = "checkBox_writeDB";
            this.checkBox_writeDB.Size = new System.Drawing.Size(97, 16);
            this.checkBox_writeDB.TabIndex = 4;
            this.checkBox_writeDB.Text = "Write DB result";
            this.checkBox_writeDB.UseVisualStyleBackColor = true;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(158, 109);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(98, 41);
            this.button_ok.TabIndex = 5;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(262, 109);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 41);
            this.button_cancel.TabIndex = 6;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // label_saveImageFormat
            // 
            this.label_saveImageFormat.AutoSize = true;
            this.label_saveImageFormat.Location = new System.Drawing.Point(12, 41);
            this.label_saveImageFormat.Name = "label_saveImageFormat";
            this.label_saveImageFormat.Size = new System.Drawing.Size(92, 12);
            this.label_saveImageFormat.TabIndex = 7;
            this.label_saveImageFormat.Text = "Save image format";
            // 
            // comboBox_saveImageFormat
            // 
            this.comboBox_saveImageFormat.FormattingEnabled = true;
            this.comboBox_saveImageFormat.Location = new System.Drawing.Point(110, 38);
            this.comboBox_saveImageFormat.Name = "comboBox_saveImageFormat";
            this.comboBox_saveImageFormat.Size = new System.Drawing.Size(88, 20);
            this.comboBox_saveImageFormat.TabIndex = 8;
            // 
            // FormBufferResultSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 162);
            this.Controls.Add(this.comboBox_saveImageFormat);
            this.Controls.Add(this.label_saveImageFormat);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.checkBox_writeDB);
            this.Controls.Add(this.button_pickSaveDir);
            this.Controls.Add(this.textBox_saveImgeRoot);
            this.Controls.Add(this.checkBox_saveResultImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormBufferResultSettings";
            this.Text = "FormBufferResultSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_saveResultImage;
        private System.Windows.Forms.TextBox textBox_saveImgeRoot;
        private System.Windows.Forms.Button button_pickSaveDir;
        private System.Windows.Forms.CheckBox checkBox_writeDB;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label_saveImageFormat;
        private System.Windows.Forms.ComboBox comboBox_saveImageFormat;
    }
}