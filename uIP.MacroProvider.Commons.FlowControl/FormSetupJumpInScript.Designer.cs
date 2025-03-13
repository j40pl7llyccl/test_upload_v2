namespace uIP.MacroProvider.Commons.FlowControl
{
    partial class FormSetupJumpInScript
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
            this.label_typeSelect = new System.Windows.Forms.Label();
            this.comboBox_jumpTypeSelect = new System.Windows.Forms.ComboBox();
            this.label_jumpIndex = new System.Windows.Forms.Label();
            this.numericUpDown_fixJumpIndex = new System.Windows.Forms.NumericUpDown();
            this.label_loadedAssemblies = new System.Windows.Forms.Label();
            this.comboBox_loadedAssemblies = new System.Windows.Forms.ComboBox();
            this.label_availableFunctions = new System.Windows.Forms.Label();
            this.comboBox_avaliableFunc = new System.Windows.Forms.ComboBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.richTextBox_functionInfo = new System.Windows.Forms.RichTextBox();
            this.label_jumpScript = new System.Windows.Forms.Label();
            this.textBox_jumpScript = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_fixJumpIndex)).BeginInit();
            this.SuspendLayout();
            // 
            // label_typeSelect
            // 
            this.label_typeSelect.AutoSize = true;
            this.label_typeSelect.Location = new System.Drawing.Point(12, 26);
            this.label_typeSelect.Name = "label_typeSelect";
            this.label_typeSelect.Size = new System.Drawing.Size(82, 12);
            this.label_typeSelect.TabIndex = 0;
            this.label_typeSelect.Text = "Select jump type";
            // 
            // comboBox_jumpTypeSelect
            // 
            this.comboBox_jumpTypeSelect.FormattingEnabled = true;
            this.comboBox_jumpTypeSelect.Location = new System.Drawing.Point(109, 23);
            this.comboBox_jumpTypeSelect.Name = "comboBox_jumpTypeSelect";
            this.comboBox_jumpTypeSelect.Size = new System.Drawing.Size(121, 20);
            this.comboBox_jumpTypeSelect.TabIndex = 1;
            // 
            // label_jumpIndex
            // 
            this.label_jumpIndex.AutoSize = true;
            this.label_jumpIndex.Location = new System.Drawing.Point(12, 51);
            this.label_jumpIndex.Name = "label_jumpIndex";
            this.label_jumpIndex.Size = new System.Drawing.Size(59, 12);
            this.label_jumpIndex.TabIndex = 2;
            this.label_jumpIndex.Text = "Jump index";
            // 
            // numericUpDown_fixJumpIndex
            // 
            this.numericUpDown_fixJumpIndex.Location = new System.Drawing.Point(109, 49);
            this.numericUpDown_fixJumpIndex.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown_fixJumpIndex.Name = "numericUpDown_fixJumpIndex";
            this.numericUpDown_fixJumpIndex.Size = new System.Drawing.Size(61, 22);
            this.numericUpDown_fixJumpIndex.TabIndex = 3;
            // 
            // label_loadedAssemblies
            // 
            this.label_loadedAssemblies.AutoSize = true;
            this.label_loadedAssemblies.Location = new System.Drawing.Point(12, 83);
            this.label_loadedAssemblies.Name = "label_loadedAssemblies";
            this.label_loadedAssemblies.Size = new System.Drawing.Size(91, 12);
            this.label_loadedAssemblies.TabIndex = 4;
            this.label_loadedAssemblies.Text = "Loaded assemblies";
            // 
            // comboBox_loadedAssemblies
            // 
            this.comboBox_loadedAssemblies.FormattingEnabled = true;
            this.comboBox_loadedAssemblies.Location = new System.Drawing.Point(109, 80);
            this.comboBox_loadedAssemblies.Name = "comboBox_loadedAssemblies";
            this.comboBox_loadedAssemblies.Size = new System.Drawing.Size(287, 20);
            this.comboBox_loadedAssemblies.TabIndex = 5;
            this.comboBox_loadedAssemblies.SelectedIndexChanged += new System.EventHandler(this.comboBox_loadedAssemblies_SelectedIndexChanged);
            // 
            // label_availableFunctions
            // 
            this.label_availableFunctions.AutoSize = true;
            this.label_availableFunctions.Location = new System.Drawing.Point(12, 115);
            this.label_availableFunctions.Name = "label_availableFunctions";
            this.label_availableFunctions.Size = new System.Drawing.Size(73, 12);
            this.label_availableFunctions.TabIndex = 6;
            this.label_availableFunctions.Text = "Available func";
            // 
            // comboBox_avaliableFunc
            // 
            this.comboBox_avaliableFunc.FormattingEnabled = true;
            this.comboBox_avaliableFunc.Location = new System.Drawing.Point(109, 112);
            this.comboBox_avaliableFunc.Name = "comboBox_avaliableFunc";
            this.comboBox_avaliableFunc.Size = new System.Drawing.Size(287, 20);
            this.comboBox_avaliableFunc.TabIndex = 7;
            this.comboBox_avaliableFunc.SelectedIndexChanged += new System.EventHandler(this.comboBox_avaliableFunc_SelectedIndexChanged);
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(240, 267);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ok.TabIndex = 8;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(321, 267);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 8;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // richTextBox_functionInfo
            // 
            this.richTextBox_functionInfo.Location = new System.Drawing.Point(23, 138);
            this.richTextBox_functionInfo.Name = "richTextBox_functionInfo";
            this.richTextBox_functionInfo.Size = new System.Drawing.Size(373, 123);
            this.richTextBox_functionInfo.TabIndex = 9;
            this.richTextBox_functionInfo.Text = "";
            this.richTextBox_functionInfo.WordWrap = false;
            // 
            // label_jumpScript
            // 
            this.label_jumpScript.AutoSize = true;
            this.label_jumpScript.Location = new System.Drawing.Point(186, 51);
            this.label_jumpScript.Name = "label_jumpScript";
            this.label_jumpScript.Size = new System.Drawing.Size(58, 12);
            this.label_jumpScript.TabIndex = 2;
            this.label_jumpScript.Text = "Jump script";
            // 
            // textBox_jumpScript
            // 
            this.textBox_jumpScript.Location = new System.Drawing.Point(250, 48);
            this.textBox_jumpScript.Name = "textBox_jumpScript";
            this.textBox_jumpScript.Size = new System.Drawing.Size(146, 22);
            this.textBox_jumpScript.TabIndex = 10;
            // 
            // FormSetupJumpInScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 302);
            this.Controls.Add(this.textBox_jumpScript);
            this.Controls.Add(this.richTextBox_functionInfo);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.comboBox_avaliableFunc);
            this.Controls.Add(this.label_availableFunctions);
            this.Controls.Add(this.comboBox_loadedAssemblies);
            this.Controls.Add(this.label_loadedAssemblies);
            this.Controls.Add(this.numericUpDown_fixJumpIndex);
            this.Controls.Add(this.label_jumpScript);
            this.Controls.Add(this.label_jumpIndex);
            this.Controls.Add(this.comboBox_jumpTypeSelect);
            this.Controls.Add(this.label_typeSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSetupJumpInScript";
            this.Text = "Config Jump";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_fixJumpIndex)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_typeSelect;
        private System.Windows.Forms.ComboBox comboBox_jumpTypeSelect;
        private System.Windows.Forms.Label label_jumpIndex;
        private System.Windows.Forms.NumericUpDown numericUpDown_fixJumpIndex;
        private System.Windows.Forms.Label label_loadedAssemblies;
        private System.Windows.Forms.ComboBox comboBox_loadedAssemblies;
        private System.Windows.Forms.Label label_availableFunctions;
        private System.Windows.Forms.ComboBox comboBox_avaliableFunc;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.RichTextBox richTextBox_functionInfo;
        private System.Windows.Forms.Label label_jumpScript;
        private System.Windows.Forms.TextBox textBox_jumpScript;
    }
}