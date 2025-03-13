namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    partial class FormConfOpenImageDirectory
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
            this.textBox_pickedDir = new System.Windows.Forms.TextBox();
            this.button_pickFolder = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_apply = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_pickedDir
            // 
            this.textBox_pickedDir.Location = new System.Drawing.Point(116, 23);
            this.textBox_pickedDir.Name = "textBox_pickedDir";
            this.textBox_pickedDir.Size = new System.Drawing.Size(328, 22);
            this.textBox_pickedDir.TabIndex = 0;
            // 
            // button_pickFolder
            // 
            this.button_pickFolder.Location = new System.Drawing.Point(12, 12);
            this.button_pickFolder.Name = "button_pickFolder";
            this.button_pickFolder.Size = new System.Drawing.Size(98, 41);
            this.button_pickFolder.TabIndex = 1;
            this.button_pickFolder.Text = "Pick Folder";
            this.button_pickFolder.UseVisualStyleBackColor = true;
            this.button_pickFolder.Click += new System.EventHandler(this.button_pickFolder_Click);
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(346, 92);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(98, 41);
            this.button_ok.TabIndex = 1;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            // 
            // button_apply
            // 
            this.button_apply.Location = new System.Drawing.Point(12, 59);
            this.button_apply.Name = "button_apply";
            this.button_apply.Size = new System.Drawing.Size(98, 41);
            this.button_apply.TabIndex = 1;
            this.button_apply.Text = "Apply";
            this.button_apply.UseVisualStyleBackColor = true;
            this.button_apply.Click += new System.EventHandler(this.button_apply_Click);
            // 
            // FormConfOpenImageDirectory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 145);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_apply);
            this.Controls.Add(this.button_pickFolder);
            this.Controls.Add(this.textBox_pickedDir);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormConfOpenImageDirectory";
            this.Text = "Conf Open File Path";
            this.Shown += new System.EventHandler(this.FormConfOpenImageDirectory_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_pickedDir;
        private System.Windows.Forms.Button button_pickFolder;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_apply;
    }
}