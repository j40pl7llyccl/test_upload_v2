namespace uIP.MacroProvider.Commons.LiteHttpService
{
    partial class FormTestLiteHttpServ
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
            this.richTextBox_message = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBox_message
            // 
            this.richTextBox_message.Location = new System.Drawing.Point(12, 12);
            this.richTextBox_message.Name = "richTextBox_message";
            this.richTextBox_message.Size = new System.Drawing.Size(461, 426);
            this.richTextBox_message.TabIndex = 0;
            this.richTextBox_message.Text = "";
            this.richTextBox_message.WordWrap = false;
            // 
            // FormTestLiteHttpServ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 450);
            this.Controls.Add(this.richTextBox_message);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormTestLiteHttpServ";
            this.Text = "Test Http Service";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTestLiteHttpServ_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_message;
    }
}