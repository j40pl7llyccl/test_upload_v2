namespace uIP.Lib.Service
{
    partial class frmPopupConfig
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
            this.comboBox_pluginClassList = new System.Windows.Forms.ComboBox();
            this.button_popupConfig = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 33);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name Of Plugin Class";
            // 
            // comboBox_pluginClassList
            // 
            this.comboBox_pluginClassList.FormattingEnabled = true;
            this.comboBox_pluginClassList.Location = new System.Drawing.Point(122, 31);
            this.comboBox_pluginClassList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_pluginClassList.Name = "comboBox_pluginClassList";
            this.comboBox_pluginClassList.Size = new System.Drawing.Size(123, 20);
            this.comboBox_pluginClassList.TabIndex = 0;
            // 
            // button_popupConfig
            // 
            this.button_popupConfig.Location = new System.Drawing.Point(11, 52);
            this.button_popupConfig.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_popupConfig.Name = "button_popupConfig";
            this.button_popupConfig.Size = new System.Drawing.Size(233, 35);
            this.button_popupConfig.TabIndex = 1;
            this.button_popupConfig.Text = "Popup Config";
            this.button_popupConfig.UseVisualStyleBackColor = true;
            this.button_popupConfig.Click += new System.EventHandler(this.button_popupConfig_Click);
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(83, 108);
            this.button_ok.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 31);
            this.button_ok.TabIndex = 2;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            // 
            // frmPopupConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 163);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_popupConfig);
            this.Controls.Add(this.comboBox_pluginClassList);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "frmPopupConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugin Class Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_pluginClassList;
        private System.Windows.Forms.Button button_popupConfig;
        private System.Windows.Forms.Button button_ok;
    }
}