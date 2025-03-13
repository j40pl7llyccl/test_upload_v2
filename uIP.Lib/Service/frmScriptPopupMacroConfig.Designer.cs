namespace uIP.Lib.Service
{
    partial class frmScriptPopupMacroConfig
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_scriptList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_macroList = new System.Windows.Forms.ComboBox();
            this.button_popupConf = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Scripts";
            // 
            // comboBox_scriptList
            // 
            this.comboBox_scriptList.FormattingEnabled = true;
            this.comboBox_scriptList.Location = new System.Drawing.Point(84, 8);
            this.comboBox_scriptList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_scriptList.Name = "comboBox_scriptList";
            this.comboBox_scriptList.Size = new System.Drawing.Size(131, 20);
            this.comboBox_scriptList.TabIndex = 1;
            this.comboBox_scriptList.SelectedIndexChanged += new System.EventHandler(this.comboBox_scriptList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 31);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Macros";
            // 
            // comboBox_macroList
            // 
            this.comboBox_macroList.FormattingEnabled = true;
            this.comboBox_macroList.Location = new System.Drawing.Point(84, 29);
            this.comboBox_macroList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_macroList.Name = "comboBox_macroList";
            this.comboBox_macroList.Size = new System.Drawing.Size(131, 20);
            this.comboBox_macroList.TabIndex = 1;
            // 
            // button_popupConf
            // 
            this.button_popupConf.Location = new System.Drawing.Point(33, 61);
            this.button_popupConf.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_popupConf.Name = "button_popupConf";
            this.button_popupConf.Size = new System.Drawing.Size(181, 37);
            this.button_popupConf.TabIndex = 3;
            this.button_popupConf.Text = "Popup Config";
            this.button_popupConf.UseVisualStyleBackColor = true;
            this.button_popupConf.Click += new System.EventHandler(this.button_popupConf_Click);
            // 
            // frmScriptPopupMacroConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 111);
            this.Controls.Add(this.button_popupConf);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_macroList);
            this.Controls.Add(this.comboBox_scriptList);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "frmScriptPopupMacroConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Popup Macro Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_scriptList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_macroList;
        private System.Windows.Forms.Button button_popupConf;
    }
}