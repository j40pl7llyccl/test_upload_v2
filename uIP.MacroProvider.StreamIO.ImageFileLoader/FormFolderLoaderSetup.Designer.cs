namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    partial class FormFolderLoaderSetup
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
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_folderPath = new System.Windows.Forms.Label();
            this.textBox_folderPath = new System.Windows.Forms.TextBox();
            this.button_openDir = new System.Windows.Forms.Button();
            this.checkBox_incIndex = new System.Windows.Forms.CheckBox();
            this.checkBox_cycRun = new System.Windows.Forms.CheckBox();
            this.label_searchFilePat = new System.Windows.Forms.Label();
            this.textBox_searchPat = new System.Windows.Forms.TextBox();
            this.label_currentIndex = new System.Windows.Forms.Label();
            this.numericUpDown_currentIndex = new System.Windows.Forms.NumericUpDown();
            this.label_totalFiles = new System.Windows.Forms.Label();
            this.button_resetIndex = new System.Windows.Forms.Button();
            this.label_jumpIndex = new System.Windows.Forms.Label();
            this.numericUpDown_jumpIndex = new System.Windows.Forms.NumericUpDown();
            this.groupBox_fullSearch = new System.Windows.Forms.GroupBox();
            this.groupBox_searchByParsing = new System.Windows.Forms.GroupBox();
            this.label_fileToParse = new System.Windows.Forms.Label();
            this.textBox_parseFilename = new System.Windows.Forms.TextBox();
            this.comboBox_pluginName = new System.Windows.Forms.ComboBox();
            this.comboBox_pluginOpenedFunc = new System.Windows.Forms.ComboBox();
            this.label_pluginName = new System.Windows.Forms.Label();
            this.label_functionName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_currentIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_jumpIndex)).BeginInit();
            this.groupBox_fullSearch.SuspendLayout();
            this.groupBox_searchByParsing.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(156, 340);
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
            this.button_cancel.Location = new System.Drawing.Point(260, 340);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 41);
            this.button_cancel.TabIndex = 6;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // label_folderPath
            // 
            this.label_folderPath.AutoSize = true;
            this.label_folderPath.Location = new System.Drawing.Point(19, 64);
            this.label_folderPath.Name = "label_folderPath";
            this.label_folderPath.Size = new System.Drawing.Size(58, 12);
            this.label_folderPath.TabIndex = 7;
            this.label_folderPath.Text = "Folder path";
            // 
            // textBox_folderPath
            // 
            this.textBox_folderPath.Location = new System.Drawing.Point(83, 61);
            this.textBox_folderPath.Name = "textBox_folderPath";
            this.textBox_folderPath.ReadOnly = true;
            this.textBox_folderPath.Size = new System.Drawing.Size(194, 22);
            this.textBox_folderPath.TabIndex = 8;
            // 
            // button_openDir
            // 
            this.button_openDir.Location = new System.Drawing.Point(283, 61);
            this.button_openDir.Name = "button_openDir";
            this.button_openDir.Size = new System.Drawing.Size(75, 23);
            this.button_openDir.TabIndex = 9;
            this.button_openDir.Text = "Select";
            this.button_openDir.UseVisualStyleBackColor = true;
            this.button_openDir.Click += new System.EventHandler(this.button_openDir_Click);
            // 
            // checkBox_incIndex
            // 
            this.checkBox_incIndex.AutoSize = true;
            this.checkBox_incIndex.Location = new System.Drawing.Point(21, 12);
            this.checkBox_incIndex.Name = "checkBox_incIndex";
            this.checkBox_incIndex.Size = new System.Drawing.Size(91, 16);
            this.checkBox_incIndex.TabIndex = 10;
            this.checkBox_incIndex.Text = "Increme index";
            this.checkBox_incIndex.UseVisualStyleBackColor = true;
            // 
            // checkBox_cycRun
            // 
            this.checkBox_cycRun.AutoSize = true;
            this.checkBox_cycRun.Location = new System.Drawing.Point(21, 34);
            this.checkBox_cycRun.Name = "checkBox_cycRun";
            this.checkBox_cycRun.Size = new System.Drawing.Size(70, 16);
            this.checkBox_cycRun.TabIndex = 11;
            this.checkBox_cycRun.Text = "Cycle run";
            this.checkBox_cycRun.UseVisualStyleBackColor = true;
            // 
            // label_searchFilePat
            // 
            this.label_searchFilePat.AutoSize = true;
            this.label_searchFilePat.Location = new System.Drawing.Point(15, 24);
            this.label_searchFilePat.Name = "label_searchFilePat";
            this.label_searchFilePat.Size = new System.Drawing.Size(89, 12);
            this.label_searchFilePat.TabIndex = 12;
            this.label_searchFilePat.Text = "Search file pattern";
            // 
            // textBox_searchPat
            // 
            this.textBox_searchPat.Location = new System.Drawing.Point(110, 21);
            this.textBox_searchPat.Name = "textBox_searchPat";
            this.textBox_searchPat.Size = new System.Drawing.Size(163, 22);
            this.textBox_searchPat.TabIndex = 13;
            // 
            // label_currentIndex
            // 
            this.label_currentIndex.AutoSize = true;
            this.label_currentIndex.Location = new System.Drawing.Point(21, 311);
            this.label_currentIndex.Name = "label_currentIndex";
            this.label_currentIndex.Size = new System.Drawing.Size(70, 12);
            this.label_currentIndex.TabIndex = 14;
            this.label_currentIndex.Text = "Current index";
            // 
            // numericUpDown_currentIndex
            // 
            this.numericUpDown_currentIndex.Location = new System.Drawing.Point(114, 309);
            this.numericUpDown_currentIndex.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown_currentIndex.Name = "numericUpDown_currentIndex";
            this.numericUpDown_currentIndex.Size = new System.Drawing.Size(65, 22);
            this.numericUpDown_currentIndex.TabIndex = 15;
            // 
            // label_totalFiles
            // 
            this.label_totalFiles.AutoSize = true;
            this.label_totalFiles.Location = new System.Drawing.Point(185, 314);
            this.label_totalFiles.Name = "label_totalFiles";
            this.label_totalFiles.Size = new System.Drawing.Size(14, 12);
            this.label_totalFiles.TabIndex = 16;
            this.label_totalFiles.Text = "/n";
            // 
            // button_resetIndex
            // 
            this.button_resetIndex.Location = new System.Drawing.Point(283, 311);
            this.button_resetIndex.Name = "button_resetIndex";
            this.button_resetIndex.Size = new System.Drawing.Size(75, 23);
            this.button_resetIndex.TabIndex = 17;
            this.button_resetIndex.Text = "Reset index";
            this.button_resetIndex.UseVisualStyleBackColor = true;
            this.button_resetIndex.Click += new System.EventHandler(this.button_resetIndex_Click);
            // 
            // label_jumpIndex
            // 
            this.label_jumpIndex.AutoSize = true;
            this.label_jumpIndex.Location = new System.Drawing.Point(19, 91);
            this.label_jumpIndex.Name = "label_jumpIndex";
            this.label_jumpIndex.Size = new System.Drawing.Size(59, 12);
            this.label_jumpIndex.TabIndex = 18;
            this.label_jumpIndex.Text = "Jump index";
            // 
            // numericUpDown_jumpIndex
            // 
            this.numericUpDown_jumpIndex.Location = new System.Drawing.Point(114, 89);
            this.numericUpDown_jumpIndex.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_jumpIndex.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_jumpIndex.Name = "numericUpDown_jumpIndex";
            this.numericUpDown_jumpIndex.Size = new System.Drawing.Size(163, 22);
            this.numericUpDown_jumpIndex.TabIndex = 19;
            // 
            // groupBox_fullSearch
            // 
            this.groupBox_fullSearch.Controls.Add(this.textBox_searchPat);
            this.groupBox_fullSearch.Controls.Add(this.label_searchFilePat);
            this.groupBox_fullSearch.Location = new System.Drawing.Point(21, 126);
            this.groupBox_fullSearch.Name = "groupBox_fullSearch";
            this.groupBox_fullSearch.Size = new System.Drawing.Size(279, 56);
            this.groupBox_fullSearch.TabIndex = 20;
            this.groupBox_fullSearch.TabStop = false;
            this.groupBox_fullSearch.Text = "Search files";
            // 
            // groupBox_searchByParsing
            // 
            this.groupBox_searchByParsing.Controls.Add(this.label_functionName);
            this.groupBox_searchByParsing.Controls.Add(this.label_pluginName);
            this.groupBox_searchByParsing.Controls.Add(this.comboBox_pluginOpenedFunc);
            this.groupBox_searchByParsing.Controls.Add(this.comboBox_pluginName);
            this.groupBox_searchByParsing.Controls.Add(this.textBox_parseFilename);
            this.groupBox_searchByParsing.Controls.Add(this.label_fileToParse);
            this.groupBox_searchByParsing.Location = new System.Drawing.Point(21, 188);
            this.groupBox_searchByParsing.Name = "groupBox_searchByParsing";
            this.groupBox_searchByParsing.Size = new System.Drawing.Size(279, 105);
            this.groupBox_searchByParsing.TabIndex = 21;
            this.groupBox_searchByParsing.TabStop = false;
            this.groupBox_searchByParsing.Text = "Parsing to load files";
            // 
            // label_fileToParse
            // 
            this.label_fileToParse.AutoSize = true;
            this.label_fileToParse.Location = new System.Drawing.Point(15, 24);
            this.label_fileToParse.Name = "label_fileToParse";
            this.label_fileToParse.Size = new System.Drawing.Size(75, 12);
            this.label_fileToParse.TabIndex = 0;
            this.label_fileToParse.Text = "Parse file name";
            // 
            // textBox_parseFilename
            // 
            this.textBox_parseFilename.Location = new System.Drawing.Point(110, 21);
            this.textBox_parseFilename.Name = "textBox_parseFilename";
            this.textBox_parseFilename.Size = new System.Drawing.Size(163, 22);
            this.textBox_parseFilename.TabIndex = 1;
            // 
            // comboBox_pluginName
            // 
            this.comboBox_pluginName.FormattingEnabled = true;
            this.comboBox_pluginName.Location = new System.Drawing.Point(112, 49);
            this.comboBox_pluginName.Name = "comboBox_pluginName";
            this.comboBox_pluginName.Size = new System.Drawing.Size(161, 20);
            this.comboBox_pluginName.TabIndex = 2;
            this.comboBox_pluginName.SelectedIndexChanged += new System.EventHandler(this.comboBox_pluginName_SelectedIndexChanged);
            // 
            // comboBox_pluginOpenedFunc
            // 
            this.comboBox_pluginOpenedFunc.FormattingEnabled = true;
            this.comboBox_pluginOpenedFunc.Location = new System.Drawing.Point(112, 75);
            this.comboBox_pluginOpenedFunc.Name = "comboBox_pluginOpenedFunc";
            this.comboBox_pluginOpenedFunc.Size = new System.Drawing.Size(161, 20);
            this.comboBox_pluginOpenedFunc.TabIndex = 2;
            // 
            // label_pluginName
            // 
            this.label_pluginName.AutoSize = true;
            this.label_pluginName.Location = new System.Drawing.Point(15, 52);
            this.label_pluginName.Name = "label_pluginName";
            this.label_pluginName.Size = new System.Drawing.Size(45, 12);
            this.label_pluginName.TabIndex = 3;
            this.label_pluginName.Text = "Provider";
            // 
            // label_functionName
            // 
            this.label_functionName.AutoSize = true;
            this.label_functionName.Location = new System.Drawing.Point(15, 78);
            this.label_functionName.Name = "label_functionName";
            this.label_functionName.Size = new System.Drawing.Size(46, 12);
            this.label_functionName.TabIndex = 4;
            this.label_functionName.Text = "Function";
            // 
            // FormFolderLoaderSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 393);
            this.Controls.Add(this.groupBox_searchByParsing);
            this.Controls.Add(this.groupBox_fullSearch);
            this.Controls.Add(this.numericUpDown_jumpIndex);
            this.Controls.Add(this.label_jumpIndex);
            this.Controls.Add(this.button_resetIndex);
            this.Controls.Add(this.label_totalFiles);
            this.Controls.Add(this.numericUpDown_currentIndex);
            this.Controls.Add(this.label_currentIndex);
            this.Controls.Add(this.checkBox_cycRun);
            this.Controls.Add(this.checkBox_incIndex);
            this.Controls.Add(this.button_openDir);
            this.Controls.Add(this.textBox_folderPath);
            this.Controls.Add(this.label_folderPath);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormFolderLoaderSetup";
            this.Text = "FormFolderLoaderSetup";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_currentIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_jumpIndex)).EndInit();
            this.groupBox_fullSearch.ResumeLayout(false);
            this.groupBox_fullSearch.PerformLayout();
            this.groupBox_searchByParsing.ResumeLayout(false);
            this.groupBox_searchByParsing.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label_folderPath;
        private System.Windows.Forms.TextBox textBox_folderPath;
        private System.Windows.Forms.Button button_openDir;
        private System.Windows.Forms.CheckBox checkBox_incIndex;
        private System.Windows.Forms.CheckBox checkBox_cycRun;
        private System.Windows.Forms.Label label_searchFilePat;
        private System.Windows.Forms.TextBox textBox_searchPat;
        private System.Windows.Forms.Label label_currentIndex;
        private System.Windows.Forms.NumericUpDown numericUpDown_currentIndex;
        private System.Windows.Forms.Label label_totalFiles;
        private System.Windows.Forms.Button button_resetIndex;
        private System.Windows.Forms.Label label_jumpIndex;
        private System.Windows.Forms.NumericUpDown numericUpDown_jumpIndex;
        private System.Windows.Forms.GroupBox groupBox_fullSearch;
        private System.Windows.Forms.GroupBox groupBox_searchByParsing;
        private System.Windows.Forms.TextBox textBox_parseFilename;
        private System.Windows.Forms.Label label_fileToParse;
        private System.Windows.Forms.ComboBox comboBox_pluginOpenedFunc;
        private System.Windows.Forms.ComboBox comboBox_pluginName;
        private System.Windows.Forms.Label label_functionName;
        private System.Windows.Forms.Label label_pluginName;
    }
}