namespace uIP.MacroProvider.DB.Sqlite
{
    partial class FormTestDB
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
            this.richTextBox_query = new System.Windows.Forms.RichTextBox();
            this.button_exec = new System.Windows.Forms.Button();
            this.richTextBox_message = new System.Windows.Forms.RichTextBox();
            this.button_query = new System.Windows.Forms.Button();
            this.button_tableExist = new System.Windows.Forms.Button();
            this.button_insertTestTb = new System.Windows.Forms.Button();
            this.button_dropTable = new System.Windows.Forms.Button();
            this.button_truncate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox_query
            // 
            this.richTextBox_query.Location = new System.Drawing.Point(114, 12);
            this.richTextBox_query.Name = "richTextBox_query";
            this.richTextBox_query.Size = new System.Drawing.Size(365, 307);
            this.richTextBox_query.TabIndex = 0;
            this.richTextBox_query.Text = "";
            this.richTextBox_query.WordWrap = false;
            // 
            // button_exec
            // 
            this.button_exec.Location = new System.Drawing.Point(12, 12);
            this.button_exec.Name = "button_exec";
            this.button_exec.Size = new System.Drawing.Size(96, 47);
            this.button_exec.TabIndex = 1;
            this.button_exec.Text = "Exec";
            this.button_exec.UseVisualStyleBackColor = true;
            this.button_exec.Click += new System.EventHandler(this.button_exec_Click);
            // 
            // richTextBox_message
            // 
            this.richTextBox_message.Location = new System.Drawing.Point(12, 325);
            this.richTextBox_message.Name = "richTextBox_message";
            this.richTextBox_message.Size = new System.Drawing.Size(467, 73);
            this.richTextBox_message.TabIndex = 2;
            this.richTextBox_message.Text = "";
            this.richTextBox_message.WordWrap = false;
            // 
            // button_query
            // 
            this.button_query.Location = new System.Drawing.Point(12, 65);
            this.button_query.Name = "button_query";
            this.button_query.Size = new System.Drawing.Size(96, 47);
            this.button_query.TabIndex = 3;
            this.button_query.Text = "Query";
            this.button_query.UseVisualStyleBackColor = true;
            this.button_query.Click += new System.EventHandler(this.button_query_Click);
            // 
            // button_tableExist
            // 
            this.button_tableExist.Location = new System.Drawing.Point(12, 118);
            this.button_tableExist.Name = "button_tableExist";
            this.button_tableExist.Size = new System.Drawing.Size(96, 47);
            this.button_tableExist.TabIndex = 4;
            this.button_tableExist.Text = "Table Exist";
            this.button_tableExist.UseVisualStyleBackColor = true;
            this.button_tableExist.Click += new System.EventHandler(this.button_tableExist_Click);
            // 
            // button_insertTestTb
            // 
            this.button_insertTestTb.Location = new System.Drawing.Point(12, 171);
            this.button_insertTestTb.Name = "button_insertTestTb";
            this.button_insertTestTb.Size = new System.Drawing.Size(96, 47);
            this.button_insertTestTb.TabIndex = 5;
            this.button_insertTestTb.Text = "Insert test tb";
            this.button_insertTestTb.UseVisualStyleBackColor = true;
            this.button_insertTestTb.Click += new System.EventHandler(this.button_insertTestTb_Click);
            // 
            // button_dropTable
            // 
            this.button_dropTable.Location = new System.Drawing.Point(12, 224);
            this.button_dropTable.Name = "button_dropTable";
            this.button_dropTable.Size = new System.Drawing.Size(96, 47);
            this.button_dropTable.TabIndex = 6;
            this.button_dropTable.Text = "Drop Table";
            this.button_dropTable.UseVisualStyleBackColor = true;
            this.button_dropTable.Click += new System.EventHandler(this.button_dropTable_Click);
            // 
            // button_truncate
            // 
            this.button_truncate.Location = new System.Drawing.Point(12, 277);
            this.button_truncate.Name = "button_truncate";
            this.button_truncate.Size = new System.Drawing.Size(96, 47);
            this.button_truncate.TabIndex = 6;
            this.button_truncate.Text = "Truncate Table";
            this.button_truncate.UseVisualStyleBackColor = true;
            this.button_truncate.Click += new System.EventHandler(this.button_truncate_Click);
            // 
            // FormTestDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 407);
            this.Controls.Add(this.button_truncate);
            this.Controls.Add(this.button_dropTable);
            this.Controls.Add(this.button_insertTestTb);
            this.Controls.Add(this.button_tableExist);
            this.Controls.Add(this.button_query);
            this.Controls.Add(this.richTextBox_message);
            this.Controls.Add(this.button_exec);
            this.Controls.Add(this.richTextBox_query);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormTestDB";
            this.Text = "Test Sqlite";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTestDB_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox_query;
        private System.Windows.Forms.Button button_exec;
        private System.Windows.Forms.RichTextBox richTextBox_message;
        private System.Windows.Forms.Button button_query;
        private System.Windows.Forms.Button button_tableExist;
        private System.Windows.Forms.Button button_insertTestTb;
        private System.Windows.Forms.Button button_dropTable;
        private System.Windows.Forms.Button button_truncate;
    }
}