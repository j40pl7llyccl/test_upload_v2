namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    partial class FormSetupOpenDirFiles
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
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_pickFolder = new System.Windows.Forms.Button();
            this.textBox_pickedDir = new System.Windows.Forms.TextBox();
            this.label_loadPat = new System.Windows.Forms.Label();
            this.textBox_searchPat = new System.Windows.Forms.TextBox();
            this.checkBox_enableCycRun = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_resetIndex = new System.Windows.Forms.NumericUpDown();
            this.button_resetIndex = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_resetIndex)).BeginInit();
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(324, 132);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 41);
            this.button_cancel.TabIndex = 2;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(220, 132);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(98, 41);
            this.button_ok.TabIndex = 1;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_pickFolder
            // 
            this.button_pickFolder.Location = new System.Drawing.Point(12, 85);
            this.button_pickFolder.Name = "button_pickFolder";
            this.button_pickFolder.Size = new System.Drawing.Size(87, 41);
            this.button_pickFolder.TabIndex = 0;
            this.button_pickFolder.Text = "Pick Folder";
            this.button_pickFolder.UseVisualStyleBackColor = true;
            this.button_pickFolder.Click += new System.EventHandler(this.button_pickFolder_Click);
            // 
            // textBox_pickedDir
            // 
            this.textBox_pickedDir.Location = new System.Drawing.Point(104, 104);
            this.textBox_pickedDir.Name = "textBox_pickedDir";
            this.textBox_pickedDir.Size = new System.Drawing.Size(317, 22);
            this.textBox_pickedDir.TabIndex = 3;
            // 
            // label_loadPat
            // 
            this.label_loadPat.AutoSize = true;
            this.label_loadPat.Location = new System.Drawing.Point(12, 65);
            this.label_loadPat.Name = "label_loadPat";
            this.label_loadPat.Size = new System.Drawing.Size(71, 12);
            this.label_loadPat.TabIndex = 4;
            this.label_loadPat.Text = "Search pattern";
            // 
            // textBox_searchPat
            // 
            this.textBox_searchPat.Location = new System.Drawing.Point(105, 62);
            this.textBox_searchPat.Name = "textBox_searchPat";
            this.textBox_searchPat.Size = new System.Drawing.Size(100, 22);
            this.textBox_searchPat.TabIndex = 5;
            this.textBox_searchPat.Text = "*.*";
            // 
            // checkBox_enableCycRun
            // 
            this.checkBox_enableCycRun.AutoSize = true;
            this.checkBox_enableCycRun.Location = new System.Drawing.Point(12, 12);
            this.checkBox_enableCycRun.Name = "checkBox_enableCycRun";
            this.checkBox_enableCycRun.Size = new System.Drawing.Size(102, 16);
            this.checkBox_enableCycRun.TabIndex = 6;
            this.checkBox_enableCycRun.Text = "Enable cycle run";
            this.checkBox_enableCycRun.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Reset index";
            // 
            // numericUpDown_resetIndex
            // 
            this.numericUpDown_resetIndex.Location = new System.Drawing.Point(105, 34);
            this.numericUpDown_resetIndex.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_resetIndex.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_resetIndex.Name = "numericUpDown_resetIndex";
            this.numericUpDown_resetIndex.Size = new System.Drawing.Size(100, 22);
            this.numericUpDown_resetIndex.TabIndex = 8;
            this.numericUpDown_resetIndex.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // button_resetIndex
            // 
            this.button_resetIndex.Location = new System.Drawing.Point(14, 132);
            this.button_resetIndex.Name = "button_resetIndex";
            this.button_resetIndex.Size = new System.Drawing.Size(85, 41);
            this.button_resetIndex.TabIndex = 9;
            this.button_resetIndex.Text = "Reset Index";
            this.button_resetIndex.UseVisualStyleBackColor = true;
            this.button_resetIndex.Click += new System.EventHandler(this.button_resetIndex_Click);
            // 
            // FormSetupOpenDirFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 185);
            this.Controls.Add(this.button_resetIndex);
            this.Controls.Add(this.numericUpDown_resetIndex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox_enableCycRun);
            this.Controls.Add(this.textBox_searchPat);
            this.Controls.Add(this.label_loadPat);
            this.Controls.Add(this.button_pickFolder);
            this.Controls.Add(this.textBox_pickedDir);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSetupOpenDirFiles";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pick dir to load file";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_resetIndex)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_pickFolder;
        private System.Windows.Forms.TextBox textBox_pickedDir;
        private System.Windows.Forms.Label label_loadPat;
        private System.Windows.Forms.TextBox textBox_searchPat;
        private System.Windows.Forms.CheckBox checkBox_enableCycRun;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown_resetIndex;
        private System.Windows.Forms.Button button_resetIndex;
    }
}