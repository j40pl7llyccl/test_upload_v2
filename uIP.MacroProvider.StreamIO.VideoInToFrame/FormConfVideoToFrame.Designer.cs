using System;
using System.Windows.Forms;

namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    partial class FormConfVideoToFrame : Form
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.selectVideoButton = new System.Windows.Forms.Button();
            this.intervalLabel = new System.Windows.Forms.Label();
            this.intervalTextBox = new System.Windows.Forms.TextBox();
            this.btnExtract = new System.Windows.Forms.Button();
            this.textBoxPickedDir = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label_progress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // selectVideoButton
            // 
            this.selectVideoButton.Location = new System.Drawing.Point(22, 20);
            this.selectVideoButton.Name = "selectVideoButton";
            this.selectVideoButton.Size = new System.Drawing.Size(98, 32);
            this.selectVideoButton.TabIndex = 0;
            this.selectVideoButton.Text = "SelectVideoPath";
            this.selectVideoButton.Click += new System.EventHandler(this.selectVideoButton_Click);
            // 
            // intervalLabel
            // 
            this.intervalLabel.Location = new System.Drawing.Point(20, 70);
            this.intervalLabel.Name = "intervalLabel";
            this.intervalLabel.Size = new System.Drawing.Size(100, 20);
            this.intervalLabel.TabIndex = 2;
            this.intervalLabel.Text = "切割間隔(秒):";
            this.intervalLabel.Click += new System.EventHandler(this.intervalLabel_Click);
            // 
            // intervalTextBox
            // 
            this.intervalTextBox.Location = new System.Drawing.Point(145, 70);
            this.intervalTextBox.Name = "intervalTextBox";
            this.intervalTextBox.Size = new System.Drawing.Size(134, 22);
            this.intervalTextBox.TabIndex = 3;
            this.intervalTextBox.TextChanged += new System.EventHandler(this.intervalTextBox_TextChanged);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(145, 113);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(75, 23);
            this.btnExtract.TabIndex = 4;
            this.btnExtract.Text = "extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // textBoxPickedDir
            // 
            this.textBoxPickedDir.Location = new System.Drawing.Point(145, 27);
            this.textBoxPickedDir.Name = "textBoxPickedDir";
            this.textBoxPickedDir.Size = new System.Drawing.Size(283, 22);
            this.textBoxPickedDir.TabIndex = 5;
            this.textBoxPickedDir.TextChanged += new System.EventHandler(this.textBoxPickedDir_TextChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 0;
            // 
            // label_progress
            // 
            this.label_progress.AllowDrop = true;
            this.label_progress.AutoEllipsis = true;
            this.label_progress.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_progress.Location = new System.Drawing.Point(142, 154);
            this.label_progress.Name = "label_progress";
            this.label_progress.Size = new System.Drawing.Size(347, 20);
            this.label_progress.TabIndex = 6;
            // 
            // FormConfVideoToFrame
            // 
            this.ClientSize = new System.Drawing.Size(501, 192);
            this.Controls.Add(this.label_progress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxPickedDir);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.selectVideoButton);
            this.Controls.Add(this.intervalLabel);
            this.Controls.Add(this.intervalTextBox);
            this.Name = "FormConfVideoToFrame";
            this.Text = "Video cutting tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button button_pickFolder;
        private Label label1;
        private TextBox textBox_intervalSecond;
        private Button Extract;
        private TextBox textBox_pickedDir;
        private Button button1;
        private TextBox textBox1;
        private Label label2;
        private TextBox textBox2;
        private Button button_extract;
        private Button selectVideoButton;
        private Label intervalLabel;
        private TextBox intervalTextBox;
        private Button btnExtract;
        private TextBox textBoxPickedDir;
        private Label label3;
        private Label label_progress;
    }
}

