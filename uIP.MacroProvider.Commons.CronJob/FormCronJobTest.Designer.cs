namespace uIP.MacroProvider.Commons.CronJob
{
    partial class FormCronJobTest
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
            this.button_addInSeconds = new System.Windows.Forms.Button();
            this.button_rmvInSeconds = new System.Windows.Forms.Button();
            this.button_addInMinute = new System.Windows.Forms.Button();
            this.button_rmvInMinute = new System.Windows.Forms.Button();
            this.button_addLongPeriod = new System.Windows.Forms.Button();
            this.button_rmvLongPeriod = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_addInSeconds
            // 
            this.button_addInSeconds.Location = new System.Drawing.Point(12, 12);
            this.button_addInSeconds.Name = "button_addInSeconds";
            this.button_addInSeconds.Size = new System.Drawing.Size(110, 39);
            this.button_addInSeconds.TabIndex = 0;
            this.button_addInSeconds.Text = "Add In-Seconds";
            this.button_addInSeconds.UseVisualStyleBackColor = true;
            this.button_addInSeconds.Click += new System.EventHandler(this.button_addInSeconds_Click);
            // 
            // button_rmvInSeconds
            // 
            this.button_rmvInSeconds.Location = new System.Drawing.Point(128, 12);
            this.button_rmvInSeconds.Name = "button_rmvInSeconds";
            this.button_rmvInSeconds.Size = new System.Drawing.Size(110, 39);
            this.button_rmvInSeconds.TabIndex = 0;
            this.button_rmvInSeconds.Text = "Remove In-Seconds";
            this.button_rmvInSeconds.UseVisualStyleBackColor = true;
            this.button_rmvInSeconds.Click += new System.EventHandler(this.button_rmvInSeconds_Click);
            // 
            // button_addInMinute
            // 
            this.button_addInMinute.Location = new System.Drawing.Point(12, 57);
            this.button_addInMinute.Name = "button_addInMinute";
            this.button_addInMinute.Size = new System.Drawing.Size(110, 39);
            this.button_addInMinute.TabIndex = 0;
            this.button_addInMinute.Text = "Add In-Minute";
            this.button_addInMinute.UseVisualStyleBackColor = true;
            this.button_addInMinute.Click += new System.EventHandler(this.button_addInMinute_Click);
            // 
            // button_rmvInMinute
            // 
            this.button_rmvInMinute.Location = new System.Drawing.Point(128, 57);
            this.button_rmvInMinute.Name = "button_rmvInMinute";
            this.button_rmvInMinute.Size = new System.Drawing.Size(110, 39);
            this.button_rmvInMinute.TabIndex = 0;
            this.button_rmvInMinute.Text = "Remove In-Minute";
            this.button_rmvInMinute.UseVisualStyleBackColor = true;
            this.button_rmvInMinute.Click += new System.EventHandler(this.button_rmvInMinute_Click);
            // 
            // button_addLongPeriod
            // 
            this.button_addLongPeriod.Location = new System.Drawing.Point(12, 102);
            this.button_addLongPeriod.Name = "button_addLongPeriod";
            this.button_addLongPeriod.Size = new System.Drawing.Size(110, 39);
            this.button_addLongPeriod.TabIndex = 0;
            this.button_addLongPeriod.Text = "Add Long Period";
            this.button_addLongPeriod.UseVisualStyleBackColor = true;
            this.button_addLongPeriod.Click += new System.EventHandler(this.button_addLongPeriod_Click);
            // 
            // button_rmvLongPeriod
            // 
            this.button_rmvLongPeriod.Location = new System.Drawing.Point(128, 102);
            this.button_rmvLongPeriod.Name = "button_rmvLongPeriod";
            this.button_rmvLongPeriod.Size = new System.Drawing.Size(110, 39);
            this.button_rmvLongPeriod.TabIndex = 0;
            this.button_rmvLongPeriod.Text = "Remove Long Period";
            this.button_rmvLongPeriod.UseVisualStyleBackColor = true;
            this.button_rmvLongPeriod.Click += new System.EventHandler(this.button_rmvLongPeriod_Click);
            // 
            // FormCronJobTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_rmvLongPeriod);
            this.Controls.Add(this.button_rmvInMinute);
            this.Controls.Add(this.button_rmvInSeconds);
            this.Controls.Add(this.button_addLongPeriod);
            this.Controls.Add(this.button_addInMinute);
            this.Controls.Add(this.button_addInSeconds);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormCronJobTest";
            this.Text = "Test CronJob";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCronJobTest_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_addInSeconds;
        private System.Windows.Forms.Button button_rmvInSeconds;
        private System.Windows.Forms.Button button_addInMinute;
        private System.Windows.Forms.Button button_rmvInMinute;
        private System.Windows.Forms.Button button_addLongPeriod;
        private System.Windows.Forms.Button button_rmvLongPeriod;
    }
}