namespace uIP.Lib.UsrControl
{
    partial class FormNewUser
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
            this.label_username = new System.Windows.Forms.Label();
            this.textBox_username = new System.Windows.Forms.TextBox();
            this.label_pwd = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label_confirmPwd = new System.Windows.Forms.Label();
            this.textBox_confirmPwd = new System.Windows.Forms.TextBox();
            this.comboBox_group = new System.Windows.Forms.ComboBox();
            this.label_group = new System.Windows.Forms.Label();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_pwdStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_username
            // 
            this.label_username.AutoSize = true;
            this.label_username.Location = new System.Drawing.Point(29, 15);
            this.label_username.Name = "label_username";
            this.label_username.Size = new System.Drawing.Size(56, 12);
            this.label_username.TabIndex = 0;
            this.label_username.Text = "User Name";
            // 
            // textBox_username
            // 
            this.textBox_username.Location = new System.Drawing.Point(126, 12);
            this.textBox_username.Name = "textBox_username";
            this.textBox_username.Size = new System.Drawing.Size(146, 22);
            this.textBox_username.TabIndex = 1;
            // 
            // label_pwd
            // 
            this.label_pwd.AutoSize = true;
            this.label_pwd.Location = new System.Drawing.Point(29, 43);
            this.label_pwd.Name = "label_pwd";
            this.label_pwd.Size = new System.Drawing.Size(48, 12);
            this.label_pwd.TabIndex = 0;
            this.label_pwd.Text = "Password";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(126, 40);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(146, 22);
            this.textBox_password.TabIndex = 1;
            this.textBox_password.Leave += new System.EventHandler(this.textBox_password_Leave);
            // 
            // label_confirmPwd
            // 
            this.label_confirmPwd.AutoSize = true;
            this.label_confirmPwd.Location = new System.Drawing.Point(29, 71);
            this.label_confirmPwd.Name = "label_confirmPwd";
            this.label_confirmPwd.Size = new System.Drawing.Size(91, 12);
            this.label_confirmPwd.TabIndex = 0;
            this.label_confirmPwd.Text = "Confirm Password";
            // 
            // textBox_confirmPwd
            // 
            this.textBox_confirmPwd.Location = new System.Drawing.Point(126, 68);
            this.textBox_confirmPwd.Name = "textBox_confirmPwd";
            this.textBox_confirmPwd.PasswordChar = '*';
            this.textBox_confirmPwd.Size = new System.Drawing.Size(146, 22);
            this.textBox_confirmPwd.TabIndex = 1;
            this.textBox_confirmPwd.TextChanged += new System.EventHandler(this.textBox_confirmPwd_TextChanged);
            this.textBox_confirmPwd.Leave += new System.EventHandler(this.textBox_confirmPwd_Leave);
            // 
            // comboBox_group
            // 
            this.comboBox_group.FormattingEnabled = true;
            this.comboBox_group.Location = new System.Drawing.Point(126, 96);
            this.comboBox_group.Name = "comboBox_group";
            this.comboBox_group.Size = new System.Drawing.Size(146, 20);
            this.comboBox_group.TabIndex = 2;
            // 
            // label_group
            // 
            this.label_group.AutoSize = true;
            this.label_group.Location = new System.Drawing.Point(29, 99);
            this.label_group.Name = "label_group";
            this.label_group.Size = new System.Drawing.Size(35, 12);
            this.label_group.TabIndex = 3;
            this.label_group.Text = "Group";
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(31, 144);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(98, 39);
            this.button_ok.TabIndex = 4;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(174, 144);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 39);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // label_pwdStatus
            // 
            this.label_pwdStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_pwdStatus.Location = new System.Drawing.Point(278, 72);
            this.label_pwdStatus.Name = "label_pwdStatus";
            this.label_pwdStatus.Size = new System.Drawing.Size(11, 11);
            this.label_pwdStatus.TabIndex = 5;
            // 
            // FormNewUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 195);
            this.Controls.Add(this.label_pwdStatus);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.label_group);
            this.Controls.Add(this.comboBox_group);
            this.Controls.Add(this.textBox_confirmPwd);
            this.Controls.Add(this.label_confirmPwd);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.label_pwd);
            this.Controls.Add(this.textBox_username);
            this.Controls.Add(this.label_username);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormNewUser";
            this.Text = "New User";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_username;
        private System.Windows.Forms.TextBox textBox_username;
        private System.Windows.Forms.Label label_pwd;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label_confirmPwd;
        private System.Windows.Forms.TextBox textBox_confirmPwd;
        private System.Windows.Forms.ComboBox comboBox_group;
        private System.Windows.Forms.Label label_group;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label_pwdStatus;
    }
}