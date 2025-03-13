namespace uIP.Lib.Multithreading
{
    partial class Form1
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
			  this.btnTest = new System.Windows.Forms.Button();
			  this.btnInit = new System.Windows.Forms.Button();
			  this.numTotalDispatcher = new System.Windows.Forms.NumericUpDown();
			  this.numJobsInGrp = new System.Windows.Forms.NumericUpDown();
			  this.numTestJobGrps = new System.Windows.Forms.NumericUpDown();
			  this.lblTotalDispatcher = new System.Windows.Forms.Label();
			  this.lblTotalJobGrps = new System.Windows.Forms.Label();
			  this.lblJobsInGrp = new System.Windows.Forms.Label();
			  this.lblThreadsInDispatcher = new System.Windows.Forms.Label();
			  this.numThreadsInDispatcher = new System.Windows.Forms.NumericUpDown();
			  this.btnDoJobGrps = new System.Windows.Forms.Button();
			  this.lstAppStatus = new System.Windows.Forms.ListBox();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numTotalDispatcher ) ).BeginInit();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numJobsInGrp ) ).BeginInit();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numTestJobGrps ) ).BeginInit();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numThreadsInDispatcher ) ).BeginInit();
			  this.SuspendLayout();
			  // 
			  // btnTest
			  // 
			  this.btnTest.Location = new System.Drawing.Point( 390, 10 );
			  this.btnTest.Name = "btnTest";
			  this.btnTest.Size = new System.Drawing.Size( 105, 45 );
			  this.btnTest.TabIndex = 0;
			  this.btnTest.Text = "Test";
			  this.btnTest.UseVisualStyleBackColor = true;
			  this.btnTest.Click += new System.EventHandler( this.btnTest_Click );
			  // 
			  // btnInit
			  // 
			  this.btnInit.Location = new System.Drawing.Point( 210, 10 );
			  this.btnInit.Name = "btnInit";
			  this.btnInit.Size = new System.Drawing.Size( 105, 45 );
			  this.btnInit.TabIndex = 1;
			  this.btnInit.Text = "Init";
			  this.btnInit.UseVisualStyleBackColor = true;
			  this.btnInit.Click += new System.EventHandler( this.btnInit_Click );
			  // 
			  // numTotalDispatcher
			  // 
			  this.numTotalDispatcher.Location = new System.Drawing.Point( 130, 15 );
			  this.numTotalDispatcher.Name = "numTotalDispatcher";
			  this.numTotalDispatcher.Size = new System.Drawing.Size( 70, 22 );
			  this.numTotalDispatcher.TabIndex = 2;
			  // 
			  // numJobsInGrp
			  // 
			  this.numJobsInGrp.Location = new System.Drawing.Point( 130, 105 );
			  this.numJobsInGrp.Name = "numJobsInGrp";
			  this.numJobsInGrp.Size = new System.Drawing.Size( 70, 22 );
			  this.numJobsInGrp.TabIndex = 3;
			  // 
			  // numTestJobGrps
			  // 
			  this.numTestJobGrps.Location = new System.Drawing.Point( 130, 75 );
			  this.numTestJobGrps.Maximum = new decimal( new int[] {
            100000,
            0,
            0,
            0} );
			  this.numTestJobGrps.Name = "numTestJobGrps";
			  this.numTestJobGrps.Size = new System.Drawing.Size( 70, 22 );
			  this.numTestJobGrps.TabIndex = 4;
			  // 
			  // lblTotalDispatcher
			  // 
			  this.lblTotalDispatcher.AutoSize = true;
			  this.lblTotalDispatcher.Font = new System.Drawing.Font( "PMingLiU", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte ) ( 136 ) ) );
			  this.lblTotalDispatcher.Location = new System.Drawing.Point( 15, 20 );
			  this.lblTotalDispatcher.Name = "lblTotalDispatcher";
			  this.lblTotalDispatcher.Size = new System.Drawing.Size( 85, 13 );
			  this.lblTotalDispatcher.TabIndex = 5;
			  this.lblTotalDispatcher.Text = "Total Dispatcher";
			  // 
			  // lblTotalJobGrps
			  // 
			  this.lblTotalJobGrps.AutoSize = true;
			  this.lblTotalJobGrps.Font = new System.Drawing.Font( "PMingLiU", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte ) ( 136 ) ) );
			  this.lblTotalJobGrps.Location = new System.Drawing.Point( 15, 80 );
			  this.lblTotalJobGrps.Name = "lblTotalJobGrps";
			  this.lblTotalJobGrps.Size = new System.Drawing.Size( 90, 13 );
			  this.lblTotalJobGrps.TabIndex = 6;
			  this.lblTotalJobGrps.Text = "Total Job Groups";
			  // 
			  // lblJobsInGrp
			  // 
			  this.lblJobsInGrp.AutoSize = true;
			  this.lblJobsInGrp.Font = new System.Drawing.Font( "PMingLiU", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte ) ( 136 ) ) );
			  this.lblJobsInGrp.Location = new System.Drawing.Point( 15, 110 );
			  this.lblJobsInGrp.Name = "lblJobsInGrp";
			  this.lblJobsInGrp.Size = new System.Drawing.Size( 88, 13 );
			  this.lblJobsInGrp.TabIndex = 7;
			  this.lblJobsInGrp.Text = "Jobs In A Group";
			  // 
			  // lblThreadsInDispatcher
			  // 
			  this.lblThreadsInDispatcher.AutoSize = true;
			  this.lblThreadsInDispatcher.Font = new System.Drawing.Font( "PMingLiU", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( ( byte ) ( 136 ) ) );
			  this.lblThreadsInDispatcher.Location = new System.Drawing.Point( 15, 50 );
			  this.lblThreadsInDispatcher.Name = "lblThreadsInDispatcher";
			  this.lblThreadsInDispatcher.Size = new System.Drawing.Size( 112, 13 );
			  this.lblThreadsInDispatcher.TabIndex = 9;
			  this.lblThreadsInDispatcher.Text = "Threads In Dispatcher";
			  // 
			  // numThreadsInDispatcher
			  // 
			  this.numThreadsInDispatcher.Location = new System.Drawing.Point( 130, 45 );
			  this.numThreadsInDispatcher.Name = "numThreadsInDispatcher";
			  this.numThreadsInDispatcher.Size = new System.Drawing.Size( 70, 22 );
			  this.numThreadsInDispatcher.TabIndex = 8;
			  // 
			  // btnDoJobGrps
			  // 
			  this.btnDoJobGrps.Location = new System.Drawing.Point( 210, 60 );
			  this.btnDoJobGrps.Name = "btnDoJobGrps";
			  this.btnDoJobGrps.Size = new System.Drawing.Size( 105, 45 );
			  this.btnDoJobGrps.TabIndex = 10;
			  this.btnDoJobGrps.Text = "Do Job Groups";
			  this.btnDoJobGrps.UseVisualStyleBackColor = true;
			  this.btnDoJobGrps.Click += new System.EventHandler( this.btnDoJobGrps_Click );
			  // 
			  // lstAppStatus
			  // 
			  this.lstAppStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
			  this.lstAppStatus.FormattingEnabled = true;
			  this.lstAppStatus.HorizontalScrollbar = true;
			  this.lstAppStatus.ItemHeight = 12;
			  this.lstAppStatus.Location = new System.Drawing.Point( 0, 152 );
			  this.lstAppStatus.Name = "lstAppStatus";
			  this.lstAppStatus.ScrollAlwaysVisible = true;
			  this.lstAppStatus.Size = new System.Drawing.Size( 584, 604 );
			  this.lstAppStatus.TabIndex = 11;
			  // 
			  // Form1
			  // 
			  this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
			  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			  this.ClientSize = new System.Drawing.Size( 584, 756 );
			  this.Controls.Add( this.lstAppStatus );
			  this.Controls.Add( this.btnDoJobGrps );
			  this.Controls.Add( this.lblThreadsInDispatcher );
			  this.Controls.Add( this.numThreadsInDispatcher );
			  this.Controls.Add( this.lblJobsInGrp );
			  this.Controls.Add( this.lblTotalJobGrps );
			  this.Controls.Add( this.lblTotalDispatcher );
			  this.Controls.Add( this.numTestJobGrps );
			  this.Controls.Add( this.numJobsInGrp );
			  this.Controls.Add( this.numTotalDispatcher );
			  this.Controls.Add( this.btnInit );
			  this.Controls.Add( this.btnTest );
			  this.Name = "Form1";
			  this.Text = "Form1";
			  this.Load += new System.EventHandler( this.Form1_Load );
			  this.FormClosed += new System.Windows.Forms.FormClosedEventHandler( this.Form1_FormClosed );
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numTotalDispatcher ) ).EndInit();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numJobsInGrp ) ).EndInit();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numTestJobGrps ) ).EndInit();
			  ( ( System.ComponentModel.ISupportInitialize ) ( this.numThreadsInDispatcher ) ).EndInit();
			  this.ResumeLayout( false );
			  this.PerformLayout();

        }

        #endregion

		 private System.Windows.Forms.Button btnTest;
		 private System.Windows.Forms.Button btnInit;
		 private System.Windows.Forms.NumericUpDown numTotalDispatcher;
		 private System.Windows.Forms.NumericUpDown numJobsInGrp;
		 private System.Windows.Forms.NumericUpDown numTestJobGrps;
		 private System.Windows.Forms.Label lblTotalDispatcher;
		 private System.Windows.Forms.Label lblTotalJobGrps;
		 private System.Windows.Forms.Label lblJobsInGrp;
		 private System.Windows.Forms.Label lblThreadsInDispatcher;
		 private System.Windows.Forms.NumericUpDown numThreadsInDispatcher;
		 private System.Windows.Forms.Button btnDoJobGrps;
		 private System.Windows.Forms.ListBox lstAppStatus;
    }
}

