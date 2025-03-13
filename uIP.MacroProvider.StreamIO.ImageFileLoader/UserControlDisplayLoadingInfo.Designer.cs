namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    partial class UserControlDisplayLoadingInfo
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
            this.label_loadingFileName = new System.Windows.Forms.Label();
            this.textBox_displayFilename = new System.Windows.Forms.TextBox();
            this.label_totalCount = new System.Windows.Forms.Label();
            this.label_currentIndex = new System.Windows.Forms.Label();
            this.numericUpDown_currIndex = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_totalFileCount = new System.Windows.Forms.NumericUpDown();
            this.button_resetCount = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_currIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_totalFileCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label_loadingFileName
            // 
            this.label_loadingFileName.AutoSize = true;
            this.label_loadingFileName.Location = new System.Drawing.Point(3, 15);
            this.label_loadingFileName.Name = "label_loadingFileName";
            this.label_loadingFileName.Size = new System.Drawing.Size(75, 12);
            this.label_loadingFileName.TabIndex = 0;
            this.label_loadingFileName.Text = "Load file name";
            // 
            // textBox_displayFilename
            // 
            this.textBox_displayFilename.Location = new System.Drawing.Point(84, 12);
            this.textBox_displayFilename.Name = "textBox_displayFilename";
            this.textBox_displayFilename.ReadOnly = true;
            this.textBox_displayFilename.Size = new System.Drawing.Size(229, 22);
            this.textBox_displayFilename.TabIndex = 1;
            // 
            // label_totalCount
            // 
            this.label_totalCount.AutoSize = true;
            this.label_totalCount.Location = new System.Drawing.Point(3, 70);
            this.label_totalCount.Name = "label_totalCount";
            this.label_totalCount.Size = new System.Drawing.Size(51, 12);
            this.label_totalCount.TabIndex = 2;
            this.label_totalCount.Text = "Total files";
            // 
            // label_currentIndex
            // 
            this.label_currentIndex.AutoSize = true;
            this.label_currentIndex.Location = new System.Drawing.Point(3, 42);
            this.label_currentIndex.Name = "label_currentIndex";
            this.label_currentIndex.Size = new System.Drawing.Size(71, 12);
            this.label_currentIndex.TabIndex = 3;
            this.label_currentIndex.Text = "Current Index";
            // 
            // numericUpDown_currIndex
            // 
            this.numericUpDown_currIndex.Location = new System.Drawing.Point(84, 40);
            this.numericUpDown_currIndex.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_currIndex.Name = "numericUpDown_currIndex";
            this.numericUpDown_currIndex.ReadOnly = true;
            this.numericUpDown_currIndex.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown_currIndex.TabIndex = 4;
            // 
            // numericUpDown_totalFileCount
            // 
            this.numericUpDown_totalFileCount.Location = new System.Drawing.Point(84, 68);
            this.numericUpDown_totalFileCount.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_totalFileCount.Name = "numericUpDown_totalFileCount";
            this.numericUpDown_totalFileCount.ReadOnly = true;
            this.numericUpDown_totalFileCount.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown_totalFileCount.TabIndex = 5;
            // 
            // button_resetCount
            // 
            this.button_resetCount.Location = new System.Drawing.Point(210, 39);
            this.button_resetCount.Name = "button_resetCount";
            this.button_resetCount.Size = new System.Drawing.Size(103, 23);
            this.button_resetCount.TabIndex = 6;
            this.button_resetCount.Text = "Reset Index";
            this.button_resetCount.UseVisualStyleBackColor = true;
            this.button_resetCount.Click += new System.EventHandler(this.button_resetCount_Click);
            // 
            // UserControlDisplayLoadingInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button_resetCount);
            this.Controls.Add(this.numericUpDown_totalFileCount);
            this.Controls.Add(this.numericUpDown_currIndex);
            this.Controls.Add(this.label_currentIndex);
            this.Controls.Add(this.label_totalCount);
            this.Controls.Add(this.textBox_displayFilename);
            this.Controls.Add(this.label_loadingFileName);
            this.Name = "UserControlDisplayLoadingInfo";
            this.Size = new System.Drawing.Size(316, 99);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_currIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_totalFileCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_loadingFileName;
        private System.Windows.Forms.TextBox textBox_displayFilename;
        private System.Windows.Forms.Label label_totalCount;
        private System.Windows.Forms.Label label_currentIndex;
        private System.Windows.Forms.NumericUpDown numericUpDown_currIndex;
        private System.Windows.Forms.NumericUpDown numericUpDown_totalFileCount;
        private System.Windows.Forms.Button button_resetCount;
    }
}
