namespace uIP.Lib.UsrControl
{
    partial class FormListControl
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
            this.button_listEnvAll = new System.Windows.Forms.Button();
            this.richTextBox_edit = new System.Windows.Forms.RichTextBox();
            this.button_writeAllForMultilang = new System.Windows.Forms.Button();
            this.button_dumpAllFroAcl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_listEnvAll
            // 
            this.button_listEnvAll.Location = new System.Drawing.Point(12, 12);
            this.button_listEnvAll.Name = "button_listEnvAll";
            this.button_listEnvAll.Size = new System.Drawing.Size(133, 53);
            this.button_listEnvAll.TabIndex = 0;
            this.button_listEnvAll.Text = "List All";
            this.button_listEnvAll.UseVisualStyleBackColor = true;
            this.button_listEnvAll.Click += new System.EventHandler(this.button_listEnvAll_Click);
            // 
            // richTextBox_edit
            // 
            this.richTextBox_edit.Location = new System.Drawing.Point(188, 12);
            this.richTextBox_edit.Name = "richTextBox_edit";
            this.richTextBox_edit.Size = new System.Drawing.Size(600, 343);
            this.richTextBox_edit.TabIndex = 1;
            this.richTextBox_edit.Text = "";
            this.richTextBox_edit.WordWrap = false;
            // 
            // button_writeAllForMultilang
            // 
            this.button_writeAllForMultilang.Location = new System.Drawing.Point(12, 71);
            this.button_writeAllForMultilang.Name = "button_writeAllForMultilang";
            this.button_writeAllForMultilang.Size = new System.Drawing.Size(133, 53);
            this.button_writeAllForMultilang.TabIndex = 2;
            this.button_writeAllForMultilang.Text = "Dump All for multilanguage";
            this.button_writeAllForMultilang.UseVisualStyleBackColor = true;
            // 
            // button_dumpAllFroAcl
            // 
            this.button_dumpAllFroAcl.Location = new System.Drawing.Point(12, 130);
            this.button_dumpAllFroAcl.Name = "button_dumpAllFroAcl";
            this.button_dumpAllFroAcl.Size = new System.Drawing.Size(133, 53);
            this.button_dumpAllFroAcl.TabIndex = 3;
            this.button_dumpAllFroAcl.Text = "Dump All for ACL";
            this.button_dumpAllFroAcl.UseVisualStyleBackColor = true;
            // 
            // FormListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 371);
            this.Controls.Add(this.button_dumpAllFroAcl);
            this.Controls.Add(this.button_writeAllForMultilang);
            this.Controls.Add(this.richTextBox_edit);
            this.Controls.Add(this.button_listEnvAll);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormListControl";
            this.Text = "List Controls";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_listEnvAll;
        private System.Windows.Forms.RichTextBox richTextBox_edit;
        private System.Windows.Forms.Button button_writeAllForMultilang;
        private System.Windows.Forms.Button button_dumpAllFroAcl;
    }
}