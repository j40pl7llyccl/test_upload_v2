namespace uIP.MacroProvider.StreamIO.DividedData
{
    partial class DataSetSplitter
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblFolderPath;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Label lblTrain;
        private System.Windows.Forms.Label lblTest;
        private System.Windows.Forms.Label lblVal;
        private System.Windows.Forms.NumericUpDown numTrain;
        private System.Windows.Forms.NumericUpDown numTest;
        private System.Windows.Forms.NumericUpDown numVal;
        private System.Windows.Forms.Button btnSplit;

        private void InitializeComponent()
        {
            this.lblFolderPath = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.lblTrain = new System.Windows.Forms.Label();
            this.lblTest = new System.Windows.Forms.Label();
            this.lblVal = new System.Windows.Forms.Label();
            this.numTrain = new System.Windows.Forms.NumericUpDown();
            this.numTest = new System.Windows.Forms.NumericUpDown();
            this.numVal = new System.Windows.Forms.NumericUpDown();
            this.btnSplit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numTrain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVal)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFolderPath
            // 
            this.lblFolderPath.AutoSize = true;
            this.lblFolderPath.Location = new System.Drawing.Point(20, 20);
            this.lblFolderPath.Name = "lblFolderPath";
            this.lblFolderPath.Size = new System.Drawing.Size(136, 24);
            this.lblFolderPath.TabIndex = 9;
            this.lblFolderPath.Text = "資料夾路徑:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(158, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(300, 36);
            this.textBox1.TabIndex = 8;
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(464, 20);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(114, 38);
            this.btnSelectFolder.TabIndex = 7;
            this.btnSelectFolder.Text = "選擇...";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // lblTrain
            // 
            this.lblTrain.AutoSize = true;
            this.lblTrain.Location = new System.Drawing.Point(20, 109);
            this.lblTrain.Name = "lblTrain";
            this.lblTrain.Size = new System.Drawing.Size(161, 24);
            this.lblTrain.TabIndex = 6;
            this.lblTrain.Text = "Train資料比例:";
            // 
            // lblTest
            // 
            this.lblTest.AutoSize = true;
            this.lblTest.Location = new System.Drawing.Point(20, 189);
            this.lblTest.Name = "lblTest";
            this.lblTest.Size = new System.Drawing.Size(151, 24);
            this.lblTest.TabIndex = 5;
            this.lblTest.Text = "Test資料比例:";
            // 
            // lblVal
            // 
            this.lblVal.AutoSize = true;
            this.lblVal.Location = new System.Drawing.Point(20, 257);
            this.lblVal.Name = "lblVal";
            this.lblVal.Size = new System.Drawing.Size(144, 24);
            this.lblVal.TabIndex = 4;
            this.lblVal.Text = "Val資料比例:";
            // 
            // numTrain
            // 
            this.numTrain.Location = new System.Drawing.Point(187, 107);
            this.numTrain.Name = "numTrain";
            this.numTrain.Size = new System.Drawing.Size(138, 36);
            this.numTrain.TabIndex = 3;
            this.numTrain.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // numTest
            // 
            this.numTest.Location = new System.Drawing.Point(187, 192);
            this.numTest.Name = "numTest";
            this.numTest.Size = new System.Drawing.Size(138, 36);
            this.numTest.TabIndex = 2;
            this.numTest.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // numVal
            // 
            this.numVal.Location = new System.Drawing.Point(187, 257);
            this.numVal.Name = "numVal";
            this.numVal.Size = new System.Drawing.Size(138, 36);
            this.numVal.TabIndex = 1;
            this.numVal.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // btnSplit
            // 
            this.btnSplit.Location = new System.Drawing.Point(86, 363);
            this.btnSplit.Name = "btnSplit";
            this.btnSplit.Size = new System.Drawing.Size(492, 55);
            this.btnSplit.TabIndex = 0;
            this.btnSplit.Text = "開始分配";
            this.btnSplit.UseVisualStyleBackColor = true;
            this.btnSplit.Click += new System.EventHandler(this.btnSplit_Click);
            // 
            // DataSetSplitter
            // 
            this.ClientSize = new System.Drawing.Size(648, 471);
            this.Controls.Add(this.btnSplit);
            this.Controls.Add(this.numVal);
            this.Controls.Add(this.numTest);
            this.Controls.Add(this.numTrain);
            this.Controls.Add(this.lblVal);
            this.Controls.Add(this.lblTest);
            this.Controls.Add(this.lblTrain);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lblFolderPath);
            this.Name = "DataSetSplitter";
            this.Text = "資料集隨機分配";
            ((System.ComponentModel.ISupportInitialize)(this.numTrain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
