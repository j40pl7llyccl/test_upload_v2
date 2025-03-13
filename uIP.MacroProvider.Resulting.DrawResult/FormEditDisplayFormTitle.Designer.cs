namespace uIP.MacroProvider.Resulting.DrawResult
{
    partial class FormEditDisplayFormTitle
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
            this.label_scriptName = new System.Windows.Forms.Label();
            this.label_formTitle = new System.Windows.Forms.Label();
            this.textBox_title = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.checkBox_enableDrawing = new System.Windows.Forms.CheckBox();
            this.checkBox_showResult = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label_scriptName
            // 
            this.label_scriptName.AutoSize = true;
            this.label_scriptName.Location = new System.Drawing.Point(12, 9);
            this.label_scriptName.Name = "label_scriptName";
            this.label_scriptName.Size = new System.Drawing.Size(32, 12);
            this.label_scriptName.TabIndex = 0;
            this.label_scriptName.Text = "Script";
            // 
            // label_formTitle
            // 
            this.label_formTitle.AutoSize = true;
            this.label_formTitle.Location = new System.Drawing.Point(12, 27);
            this.label_formTitle.Name = "label_formTitle";
            this.label_formTitle.Size = new System.Drawing.Size(54, 12);
            this.label_formTitle.TabIndex = 1;
            this.label_formTitle.Text = "Form Title";
            // 
            // textBox_title
            // 
            this.textBox_title.Location = new System.Drawing.Point(72, 24);
            this.textBox_title.Name = "textBox_title";
            this.textBox_title.Size = new System.Drawing.Size(206, 22);
            this.textBox_title.TabIndex = 2;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(125, 85);
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
            this.button_cancel.Location = new System.Drawing.Point(229, 85);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(98, 41);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // checkBox_enableDrawing
            // 
            this.checkBox_enableDrawing.AutoSize = true;
            this.checkBox_enableDrawing.Location = new System.Drawing.Point(14, 52);
            this.checkBox_enableDrawing.Name = "checkBox_enableDrawing";
            this.checkBox_enableDrawing.Size = new System.Drawing.Size(89, 16);
            this.checkBox_enableDrawing.TabIndex = 4;
            this.checkBox_enableDrawing.Text = "Keep drawing";
            this.checkBox_enableDrawing.UseVisualStyleBackColor = true;
            // 
            // checkBox_showResult
            // 
            this.checkBox_showResult.AutoSize = true;
            this.checkBox_showResult.Location = new System.Drawing.Point(125, 52);
            this.checkBox_showResult.Name = "checkBox_showResult";
            this.checkBox_showResult.Size = new System.Drawing.Size(78, 16);
            this.checkBox_showResult.TabIndex = 5;
            this.checkBox_showResult.Text = "Show result";
            this.checkBox_showResult.UseVisualStyleBackColor = true;
            // 
            // FormEditDisplayFormTitle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 138);
            this.Controls.Add(this.checkBox_showResult);
            this.Controls.Add(this.checkBox_enableDrawing);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_title);
            this.Controls.Add(this.label_formTitle);
            this.Controls.Add(this.label_scriptName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormEditDisplayFormTitle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit Title";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_scriptName;
        private System.Windows.Forms.Label label_formTitle;
        private System.Windows.Forms.TextBox textBox_title;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.CheckBox checkBox_enableDrawing;
        private System.Windows.Forms.CheckBox checkBox_showResult;
    }
}