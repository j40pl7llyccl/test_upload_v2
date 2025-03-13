namespace uIP.Lib.UsrControl
{
    partial class frmChangePwd
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
            if ( disposing && ( components != null ) ) {
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
            this.textBox_confirmNewPwd = new System.Windows.Forms.TextBox();
            this.textBox_newPwd = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label_oldPwd = new System.Windows.Forms.Label();
            this.textBox_oldPwd = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(116, 196);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(347, 69);
            this.button_ok.TabIndex = 3;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // textBox_confirmNewPwd
            // 
            this.textBox_confirmNewPwd.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox_confirmNewPwd.Location = new System.Drawing.Point(219, 134);
            this.textBox_confirmNewPwd.Name = "textBox_confirmNewPwd";
            this.textBox_confirmNewPwd.PasswordChar = '*';
            this.textBox_confirmNewPwd.Size = new System.Drawing.Size(274, 36);
            this.textBox_confirmNewPwd.TabIndex = 2;
            // 
            // textBox_newPwd
            // 
            this.textBox_newPwd.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox_newPwd.Location = new System.Drawing.Point(219, 94);
            this.textBox_newPwd.Name = "textBox_newPwd";
            this.textBox_newPwd.PasswordChar = '*';
            this.textBox_newPwd.Size = new System.Drawing.Size(274, 36);
            this.textBox_newPwd.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(9, 137);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(204, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Confirm Password";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(44, 97);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 29);
            this.label1.TabIndex = 4;
            this.label1.Text = "New Password";
            // 
            // label_oldPwd
            // 
            this.label_oldPwd.AutoSize = true;
            this.label_oldPwd.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_oldPwd.Location = new System.Drawing.Point(56, 55);
            this.label_oldPwd.Name = "label_oldPwd";
            this.label_oldPwd.Size = new System.Drawing.Size(157, 29);
            this.label_oldPwd.TabIndex = 4;
            this.label_oldPwd.Text = "Old Password";
            // 
            // textBox_oldPwd
            // 
            this.textBox_oldPwd.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox_oldPwd.Location = new System.Drawing.Point(219, 52);
            this.textBox_oldPwd.Name = "textBox_oldPwd";
            this.textBox_oldPwd.PasswordChar = '*';
            this.textBox_oldPwd.Size = new System.Drawing.Size(274, 36);
            this.textBox_oldPwd.TabIndex = 0;
            // 
            // frmChangePwd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 303);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_confirmNewPwd);
            this.Controls.Add(this.textBox_oldPwd);
            this.Controls.Add(this.textBox_newPwd);
            this.Controls.Add(this.label_oldPwd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmChangePwd";
            this.Text = "Change Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.TextBox textBox_confirmNewPwd;
        private System.Windows.Forms.TextBox textBox_newPwd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_oldPwd;
        private System.Windows.Forms.TextBox textBox_oldPwd;
    }
}