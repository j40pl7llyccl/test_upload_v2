namespace uIP.MacroProvider.Commons.FlowControl
{
    partial class UserControlJumpEnd
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_cont = new System.Windows.Forms.Button();
            this.button_end = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_cont
            // 
            this.button_cont.Location = new System.Drawing.Point(3, 43);
            this.button_cont.Name = "button_cont";
            this.button_cont.Size = new System.Drawing.Size(102, 48);
            this.button_cont.TabIndex = 0;
            this.button_cont.Text = "Continue";
            this.button_cont.UseVisualStyleBackColor = true;
            this.button_cont.Click += new System.EventHandler(this.button_cont_Click);
            // 
            // button_end
            // 
            this.button_end.Location = new System.Drawing.Point(111, 43);
            this.button_end.Name = "button_end";
            this.button_end.Size = new System.Drawing.Size(102, 48);
            this.button_end.TabIndex = 0;
            this.button_end.Text = "End";
            this.button_end.UseVisualStyleBackColor = true;
            this.button_end.Click += new System.EventHandler(this.button_end_Click);
            // 
            // UserControlJumpEnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button_end);
            this.Controls.Add(this.button_cont);
            this.Name = "UserControlJumpEnd";
            this.Size = new System.Drawing.Size(219, 150);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_cont;
        private System.Windows.Forms.Button button_end;
    }
}
