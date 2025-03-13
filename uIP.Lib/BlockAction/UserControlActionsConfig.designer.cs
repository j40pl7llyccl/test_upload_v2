namespace uIP.Lib.BlockAction
{
    partial class UserControlActionsConfig
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
            this.comboBox_blocksList = new System.Windows.Forms.ComboBox();
            this.comboBox_actionList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel = new System.Windows.Forms.Panel();
            this.button_update = new System.Windows.Forms.Button();
            this.checkBox_confInput = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // comboBox_blocksList
            // 
            this.comboBox_blocksList.FormattingEnabled = true;
            this.comboBox_blocksList.Location = new System.Drawing.Point(77, 33);
            this.comboBox_blocksList.Name = "comboBox_blocksList";
            this.comboBox_blocksList.Size = new System.Drawing.Size(235, 26);
            this.comboBox_blocksList.TabIndex = 4;
            this.comboBox_blocksList.SelectedIndexChanged += new System.EventHandler(this.comboBox_blocksList_SelectedIndexChanged);
            // 
            // comboBox_actionList
            // 
            this.comboBox_actionList.FormattingEnabled = true;
            this.comboBox_actionList.Location = new System.Drawing.Point(77, 3);
            this.comboBox_actionList.Name = "comboBox_actionList";
            this.comboBox_actionList.Size = new System.Drawing.Size(235, 26);
            this.comboBox_actionList.TabIndex = 5;
            this.comboBox_actionList.SelectedIndexChanged += new System.EventHandler(this.comboBox_actionList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Blocks";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Acrions";
            // 
            // panel
            // 
            this.panel.AutoScroll = true;
            this.panel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel.Location = new System.Drawing.Point(0, 76);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(800, 524);
            this.panel.TabIndex = 6;
            // 
            // button_update
            // 
            this.button_update.Location = new System.Drawing.Point(339, 3);
            this.button_update.Name = "button_update";
            this.button_update.Size = new System.Drawing.Size(130, 56);
            this.button_update.TabIndex = 7;
            this.button_update.Text = "Update";
            this.button_update.UseVisualStyleBackColor = true;
            this.button_update.Click += new System.EventHandler(this.button_update_Click);
            // 
            // checkBox_confInput
            // 
            this.checkBox_confInput.AutoSize = true;
            this.checkBox_confInput.Checked = true;
            this.checkBox_confInput.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_confInput.Location = new System.Drawing.Point(490, 7);
            this.checkBox_confInput.Name = "checkBox_confInput";
            this.checkBox_confInput.Size = new System.Drawing.Size(169, 22);
            this.checkBox_confInput.TabIndex = 8;
            this.checkBox_confInput.Text = "Config Input Param";
            this.checkBox_confInput.UseVisualStyleBackColor = true;
            this.checkBox_confInput.CheckStateChanged += new System.EventHandler(this.checkBox_confInput_CheckStateChanged);
            // 
            // UserControlActionsConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBox_confInput);
            this.Controls.Add(this.button_update);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.comboBox_blocksList);
            this.Controls.Add(this.comboBox_actionList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "UserControlActionsConfig";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_blocksList;
        private System.Windows.Forms.ComboBox comboBox_actionList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Button button_update;
        private System.Windows.Forms.CheckBox checkBox_confInput;
    }
}
