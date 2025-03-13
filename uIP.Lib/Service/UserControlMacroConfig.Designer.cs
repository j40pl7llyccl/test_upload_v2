namespace uIP.Lib.Service
{
    partial class UserControlMacroConfig
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_scriptList = new System.Windows.Forms.ComboBox();
            this.panel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_macroList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Scripts";
            // 
            // comboBox_scriptList
            // 
            this.comboBox_scriptList.FormattingEnabled = true;
            this.comboBox_scriptList.Location = new System.Drawing.Point(69, 3);
            this.comboBox_scriptList.Name = "comboBox_scriptList";
            this.comboBox_scriptList.Size = new System.Drawing.Size(337, 26);
            this.comboBox_scriptList.TabIndex = 1;
            this.comboBox_scriptList.SelectedIndexChanged += new System.EventHandler(this.comboBox_scriptList_SelectedIndexChanged);
            // 
            // panel
            // 
            this.panel.AutoScroll = true;
            this.panel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel.Location = new System.Drawing.Point(0, 67);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(800, 533);
            this.panel.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Macros";
            // 
            // comboBox_macroList
            // 
            this.comboBox_macroList.FormattingEnabled = true;
            this.comboBox_macroList.Location = new System.Drawing.Point(69, 35);
            this.comboBox_macroList.Name = "comboBox_macroList";
            this.comboBox_macroList.Size = new System.Drawing.Size(337, 26);
            this.comboBox_macroList.TabIndex = 1;
            this.comboBox_macroList.SelectedIndexChanged += new System.EventHandler(this.comboBox_macroList_SelectedIndexChanged);
            // 
            // UserControlMacroConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Controls.Add(this.comboBox_macroList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_scriptList);
            this.Controls.Add(this.label1);
            this.Name = "UserControlMacroConfig";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_scriptList;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_macroList;
    }
}
