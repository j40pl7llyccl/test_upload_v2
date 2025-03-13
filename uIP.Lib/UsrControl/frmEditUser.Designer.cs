namespace uIP.Lib.UsrControl
{
    partial class frmEditUser
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
            this.dataGridView_Users = new System.Windows.Forms.DataGridView();
            this.Column_userName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_group = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Column_inherEnableGroup = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Column_inherVisibleGroup = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.button_loadFile = new System.Windows.Forms.Button();
            this.button_changeUser = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_userName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_group = new System.Windows.Forms.ToolStripStatusLabel();
            this.button_reload = new System.Windows.Forms.Button();
            this.button_SaveToFile = new System.Windows.Forms.Button();
            this.button_deleteSelection = new System.Windows.Forms.Button();
            this.checkBox_autoSaveOnSucc = new System.Windows.Forms.CheckBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_DECGroup = new System.Windows.Forms.Button();
            this.button_ENCGroup = new System.Windows.Forms.Button();
            this.button_new = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Users)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView_Users
            // 
            this.dataGridView_Users.AllowUserToAddRows = false;
            this.dataGridView_Users.AllowUserToDeleteRows = false;
            this.dataGridView_Users.AllowUserToResizeColumns = false;
            this.dataGridView_Users.AllowUserToResizeRows = false;
            this.dataGridView_Users.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_Users.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_userName,
            this.Column_group,
            this.Column_inherEnableGroup,
            this.Column_inherVisibleGroup});
            this.dataGridView_Users.Location = new System.Drawing.Point(361, 13);
            this.dataGridView_Users.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataGridView_Users.Name = "dataGridView_Users";
            this.dataGridView_Users.RowTemplate.Height = 31;
            this.dataGridView_Users.Size = new System.Drawing.Size(1094, 1096);
            this.dataGridView_Users.TabIndex = 0;
            this.dataGridView_Users.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_Users_CellDoubleClick);
            this.dataGridView_Users.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_Users_CellEndEdit);
            // 
            // Column_userName
            // 
            this.Column_userName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Column_userName.HeaderText = "User Name";
            this.Column_userName.Name = "Column_userName";
            this.Column_userName.ReadOnly = true;
            this.Column_userName.ToolTipText = "aa";
            this.Column_userName.Width = 300;
            // 
            // Column_group
            // 
            this.Column_group.HeaderText = "Group";
            this.Column_group.Name = "Column_group";
            this.Column_group.Width = 250;
            // 
            // Column_inherEnableGroup
            // 
            this.Column_inherEnableGroup.HeaderText = "Enabled Group (Inheritance)";
            this.Column_inherEnableGroup.Name = "Column_inherEnableGroup";
            this.Column_inherEnableGroup.Width = 250;
            // 
            // Column_inherVisibleGroup
            // 
            this.Column_inherVisibleGroup.HeaderText = "Visible Group (Inheritance)";
            this.Column_inherVisibleGroup.Name = "Column_inherVisibleGroup";
            this.Column_inherVisibleGroup.Width = 250;
            // 
            // button_loadFile
            // 
            this.button_loadFile.Location = new System.Drawing.Point(12, 154);
            this.button_loadFile.Name = "button_loadFile";
            this.button_loadFile.Size = new System.Drawing.Size(331, 65);
            this.button_loadFile.TabIndex = 1;
            this.button_loadFile.Text = "Bulk Load";
            this.button_loadFile.UseVisualStyleBackColor = true;
            this.button_loadFile.Click += new System.EventHandler(this.button_loadFile_Click);
            // 
            // button_changeUser
            // 
            this.button_changeUser.Location = new System.Drawing.Point(12, 367);
            this.button_changeUser.Name = "button_changeUser";
            this.button_changeUser.Size = new System.Drawing.Size(331, 65);
            this.button_changeUser.TabIndex = 1;
            this.button_changeUser.Text = "Change User(Login)";
            this.button_changeUser.UseVisualStyleBackColor = true;
            this.button_changeUser.Click += new System.EventHandler(this.button_changeUser_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_userName,
            this.toolStripStatusLabel_group});
            this.statusStrip1.Location = new System.Drawing.Point(0, 621);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1467, 24);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_userName
            // 
            this.toolStripStatusLabel_userName.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel_userName.Name = "toolStripStatusLabel_userName";
            this.toolStripStatusLabel_userName.Size = new System.Drawing.Size(29, 19);
            this.toolStripStatusLabel_userName.Text = "NA";
            // 
            // toolStripStatusLabel_group
            // 
            this.toolStripStatusLabel_group.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel_group.Name = "toolStripStatusLabel_group";
            this.toolStripStatusLabel_group.Size = new System.Drawing.Size(29, 19);
            this.toolStripStatusLabel_group.Text = "NA";
            // 
            // button_reload
            // 
            this.button_reload.Location = new System.Drawing.Point(12, 83);
            this.button_reload.Name = "button_reload";
            this.button_reload.Size = new System.Drawing.Size(331, 65);
            this.button_reload.TabIndex = 1;
            this.button_reload.Text = "Reload";
            this.button_reload.UseVisualStyleBackColor = true;
            this.button_reload.Click += new System.EventHandler(this.button_reload_Click);
            // 
            // button_SaveToFile
            // 
            this.button_SaveToFile.Location = new System.Drawing.Point(12, 225);
            this.button_SaveToFile.Name = "button_SaveToFile";
            this.button_SaveToFile.Size = new System.Drawing.Size(331, 65);
            this.button_SaveToFile.TabIndex = 1;
            this.button_SaveToFile.Text = "Save";
            this.button_SaveToFile.UseVisualStyleBackColor = true;
            this.button_SaveToFile.Click += new System.EventHandler(this.button_SaveToFile_Click);
            // 
            // button_deleteSelection
            // 
            this.button_deleteSelection.Location = new System.Drawing.Point(12, 296);
            this.button_deleteSelection.Name = "button_deleteSelection";
            this.button_deleteSelection.Size = new System.Drawing.Size(331, 65);
            this.button_deleteSelection.TabIndex = 1;
            this.button_deleteSelection.Text = "Remove Selection";
            this.button_deleteSelection.UseVisualStyleBackColor = true;
            this.button_deleteSelection.Click += new System.EventHandler(this.button_deleteSelection_Click);
            // 
            // checkBox_autoSaveOnSucc
            // 
            this.checkBox_autoSaveOnSucc.AutoSize = true;
            this.checkBox_autoSaveOnSucc.Location = new System.Drawing.Point(12, 583);
            this.checkBox_autoSaveOnSucc.Name = "checkBox_autoSaveOnSucc";
            this.checkBox_autoSaveOnSucc.Size = new System.Drawing.Size(81, 18);
            this.checkBox_autoSaveOnSucc.TabIndex = 3;
            this.checkBox_autoSaveOnSucc.Text = "Auto-save";
            this.checkBox_autoSaveOnSucc.UseVisualStyleBackColor = true;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(12, 863);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(331, 65);
            this.button_ok.TabIndex = 4;
            this.button_ok.Text = "OK";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Visible = false;
            // 
            // button_DECGroup
            // 
            this.button_DECGroup.Location = new System.Drawing.Point(12, 438);
            this.button_DECGroup.Name = "button_DECGroup";
            this.button_DECGroup.Size = new System.Drawing.Size(331, 65);
            this.button_DECGroup.TabIndex = 5;
            this.button_DECGroup.Text = "Decrypt Group File";
            this.button_DECGroup.UseVisualStyleBackColor = true;
            this.button_DECGroup.Click += new System.EventHandler(this.button_DECGroup_Click);
            // 
            // button_ENCGroup
            // 
            this.button_ENCGroup.Location = new System.Drawing.Point(12, 509);
            this.button_ENCGroup.Name = "button_ENCGroup";
            this.button_ENCGroup.Size = new System.Drawing.Size(331, 65);
            this.button_ENCGroup.TabIndex = 5;
            this.button_ENCGroup.Text = "Encrypt Group File";
            this.button_ENCGroup.UseVisualStyleBackColor = true;
            this.button_ENCGroup.Click += new System.EventHandler(this.button_ENCGroup_Click);
            // 
            // button_new
            // 
            this.button_new.Location = new System.Drawing.Point(12, 12);
            this.button_new.Name = "button_new";
            this.button_new.Size = new System.Drawing.Size(331, 65);
            this.button_new.TabIndex = 6;
            this.button_new.Text = "New";
            this.button_new.UseVisualStyleBackColor = true;
            this.button_new.Click += new System.EventHandler(this.button_new_Click);
            // 
            // frmEditUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1467, 645);
            this.Controls.Add(this.button_new);
            this.Controls.Add(this.button_ENCGroup);
            this.Controls.Add(this.button_DECGroup);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.checkBox_autoSaveOnSucc);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_changeUser);
            this.Controls.Add(this.button_reload);
            this.Controls.Add(this.button_deleteSelection);
            this.Controls.Add(this.button_SaveToFile);
            this.Controls.Add(this.button_loadFile);
            this.Controls.Add(this.dataGridView_Users);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "frmEditUser";
            this.Text = "Edit User";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmEditUser_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_Users)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_Users;
        private System.Windows.Forms.Button button_loadFile;
        private System.Windows.Forms.Button button_changeUser;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_userName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_group;
        private System.Windows.Forms.Button button_reload;
        private System.Windows.Forms.Button button_SaveToFile;
        private System.Windows.Forms.Button button_deleteSelection;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_userName;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column_group;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column_inherEnableGroup;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column_inherVisibleGroup;
        private System.Windows.Forms.CheckBox checkBox_autoSaveOnSucc;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_DECGroup;
        private System.Windows.Forms.Button button_ENCGroup;
        private System.Windows.Forms.Button button_new;
    }
}