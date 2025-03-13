namespace uIP.Lib.UsrControl
{
    partial class FormAclEditor
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
            this.richTextBox_edit = new System.Windows.Forms.RichTextBox();
            this.button_genDefaultGroup = new System.Windows.Forms.Button();
            this.button_genDefaultUser = new System.Windows.Forms.Button();
            this.button_reload = new System.Windows.Forms.Button();
            this.checkBox_encFile = new System.Windows.Forms.CheckBox();
            this.button_save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox_edit
            // 
            this.richTextBox_edit.Location = new System.Drawing.Point(237, 12);
            this.richTextBox_edit.Name = "richTextBox_edit";
            this.richTextBox_edit.Size = new System.Drawing.Size(551, 278);
            this.richTextBox_edit.TabIndex = 0;
            this.richTextBox_edit.Text = "";
            this.richTextBox_edit.WordWrap = false;
            // 
            // button_genDefaultGroup
            // 
            this.button_genDefaultGroup.Location = new System.Drawing.Point(12, 12);
            this.button_genDefaultGroup.Name = "button_genDefaultGroup";
            this.button_genDefaultGroup.Size = new System.Drawing.Size(99, 45);
            this.button_genDefaultGroup.TabIndex = 1;
            this.button_genDefaultGroup.Text = "Gen Default Group";
            this.button_genDefaultGroup.UseVisualStyleBackColor = true;
            this.button_genDefaultGroup.Click += new System.EventHandler(this.button_genDefaultGroup_Click);
            // 
            // button_genDefaultUser
            // 
            this.button_genDefaultUser.Location = new System.Drawing.Point(117, 12);
            this.button_genDefaultUser.Name = "button_genDefaultUser";
            this.button_genDefaultUser.Size = new System.Drawing.Size(99, 45);
            this.button_genDefaultUser.TabIndex = 1;
            this.button_genDefaultUser.Text = "Gen Default User";
            this.button_genDefaultUser.UseVisualStyleBackColor = true;
            this.button_genDefaultUser.Click += new System.EventHandler(this.button_genDefaultUser_Click);
            // 
            // button_reload
            // 
            this.button_reload.Location = new System.Drawing.Point(12, 63);
            this.button_reload.Name = "button_reload";
            this.button_reload.Size = new System.Drawing.Size(99, 45);
            this.button_reload.TabIndex = 2;
            this.button_reload.Text = "Reload";
            this.button_reload.UseVisualStyleBackColor = true;
            this.button_reload.Click += new System.EventHandler(this.button_reload_Click);
            // 
            // checkBox_encFile
            // 
            this.checkBox_encFile.AutoSize = true;
            this.checkBox_encFile.Location = new System.Drawing.Point(12, 125);
            this.checkBox_encFile.Name = "checkBox_encFile";
            this.checkBox_encFile.Size = new System.Drawing.Size(61, 16);
            this.checkBox_encFile.TabIndex = 3;
            this.checkBox_encFile.Text = "Encrypt";
            this.checkBox_encFile.UseVisualStyleBackColor = true;
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(12, 147);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(99, 45);
            this.button_save.TabIndex = 2;
            this.button_save.Text = "Save";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // FormAclEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 302);
            this.Controls.Add(this.checkBox_encFile);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.button_reload);
            this.Controls.Add(this.button_genDefaultUser);
            this.Controls.Add(this.button_genDefaultGroup);
            this.Controls.Add(this.richTextBox_edit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormAclEditor";
            this.Text = "ACL Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_edit;
        private System.Windows.Forms.Button button_genDefaultGroup;
        private System.Windows.Forms.Button button_genDefaultUser;
        private System.Windows.Forms.Button button_reload;
        private System.Windows.Forms.CheckBox checkBox_encFile;
        private System.Windows.Forms.Button button_save;
    }
}