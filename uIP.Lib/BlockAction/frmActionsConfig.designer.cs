﻿namespace uIP.Lib.BlockAction
{
    partial class frmActionsConfig
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
            this.comboBox_actionList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_blocksList = new System.Windows.Forms.ComboBox();
            this.button_popupConfig = new System.Windows.Forms.Button();
            this.button_blockSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Acrions";
            // 
            // comboBox_actionList
            // 
            this.comboBox_actionList.FormattingEnabled = true;
            this.comboBox_actionList.Location = new System.Drawing.Point(99, 12);
            this.comboBox_actionList.Name = "comboBox_actionList";
            this.comboBox_actionList.Size = new System.Drawing.Size(235, 26);
            this.comboBox_actionList.TabIndex = 1;
            this.comboBox_actionList.SelectedIndexChanged += new System.EventHandler(this.comboBox_actionList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Blocks";
            // 
            // comboBox_blocksList
            // 
            this.comboBox_blocksList.FormattingEnabled = true;
            this.comboBox_blocksList.Location = new System.Drawing.Point(99, 42);
            this.comboBox_blocksList.Name = "comboBox_blocksList";
            this.comboBox_blocksList.Size = new System.Drawing.Size(235, 26);
            this.comboBox_blocksList.TabIndex = 1;
            this.comboBox_blocksList.SelectedIndexChanged += new System.EventHandler(this.comboBox_blocksList_SelectedIndexChanged);
            // 
            // button_popupConfig
            // 
            this.button_popupConfig.Location = new System.Drawing.Point(34, 84);
            this.button_popupConfig.Name = "button_popupConfig";
            this.button_popupConfig.Size = new System.Drawing.Size(300, 53);
            this.button_popupConfig.TabIndex = 2;
            this.button_popupConfig.Text = "Config Input Param";
            this.button_popupConfig.UseVisualStyleBackColor = true;
            this.button_popupConfig.Click += new System.EventHandler(this.button_popupConfig_Click);
            // 
            // button_blockSettings
            // 
            this.button_blockSettings.Location = new System.Drawing.Point(34, 143);
            this.button_blockSettings.Name = "button_blockSettings";
            this.button_blockSettings.Size = new System.Drawing.Size(300, 53);
            this.button_blockSettings.TabIndex = 3;
            this.button_blockSettings.Text = "BlockAction Settings";
            this.button_blockSettings.UseVisualStyleBackColor = true;
            this.button_blockSettings.Click += new System.EventHandler(this.button_blockSettings_Click);
            // 
            // frmActionsConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 209);
            this.Controls.Add(this.button_blockSettings);
            this.Controls.Add(this.button_popupConfig);
            this.Controls.Add(this.comboBox_blocksList);
            this.Controls.Add(this.comboBox_actionList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmActionsConfig";
            this.Text = "Actions Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_actionList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_blocksList;
        private System.Windows.Forms.Button button_popupConfig;
        private System.Windows.Forms.Button button_blockSettings;
    }
}