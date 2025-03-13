namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    partial class FormBufferLoaderSetup
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
            this.label_jump = new System.Windows.Forms.Label();
            this.numericUpDown_jump = new System.Windows.Forms.NumericUpDown();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_jump)).BeginInit();
            this.SuspendLayout();
            // 
            // label_jump
            // 
            this.label_jump.AutoSize = true;
            this.label_jump.Location = new System.Drawing.Point(25, 14);
            this.label_jump.Name = "label_jump";
            this.label_jump.Size = new System.Drawing.Size(71, 12);
            this.label_jump.TabIndex = 0;
            this.label_jump.Text = "Jump to index";
            // 
            // numericUpDown_jump
            // 
            this.numericUpDown_jump.Location = new System.Drawing.Point(113, 12);
            this.numericUpDown_jump.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown_jump.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numericUpDown_jump.Name = "numericUpDown_jump";
            this.numericUpDown_jump.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown_jump.TabIndex = 1;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(47, 40);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(98, 41);
            this.button_ok.TabIndex = 3;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(151, 40);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 41);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // FormBufferLoaderSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 91);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.numericUpDown_jump);
            this.Controls.Add(this.label_jump);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormBufferLoaderSetup";
            this.Text = "FormBufferLoaderSetup";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_jump)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_jump;
        private System.Windows.Forms.NumericUpDown numericUpDown_jump;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
    }
}