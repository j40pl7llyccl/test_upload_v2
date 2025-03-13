namespace uIP.MacroProvider.TrainingConvert
{
    partial class modelConvert
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bt_SelectModel = new System.Windows.Forms.Button();
            this.tB_model = new System.Windows.Forms.TextBox();
            this.bt_Run = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bt_SelectModel
            // 
            this.bt_SelectModel.Font = new System.Drawing.Font("新細明體", 12F);
            this.bt_SelectModel.Location = new System.Drawing.Point(10, 69);
            this.bt_SelectModel.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.bt_SelectModel.Name = "bt_SelectModel";
            this.bt_SelectModel.Size = new System.Drawing.Size(90, 23);
            this.bt_SelectModel.TabIndex = 0;
            this.bt_SelectModel.Text = "Select model";
            this.bt_SelectModel.UseVisualStyleBackColor = true;
            this.bt_SelectModel.Click += new System.EventHandler(this.bt_SelectModel_Click);
            // 
            // tB_model
            // 
            this.tB_model.Location = new System.Drawing.Point(130, 69);
            this.tB_model.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.tB_model.Name = "tB_model";
            this.tB_model.Size = new System.Drawing.Size(367, 22);
            this.tB_model.TabIndex = 1;
            // 
            // bt_Run
            // 
            this.bt_Run.Font = new System.Drawing.Font("新細明體", 12F);
            this.bt_Run.Location = new System.Drawing.Point(426, 109);
            this.bt_Run.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.bt_Run.Name = "bt_Run";
            this.bt_Run.Size = new System.Drawing.Size(71, 40);
            this.bt_Run.TabIndex = 4;
            this.bt_Run.Text = "Convert";
            this.bt_Run.UseVisualStyleBackColor = true;
            this.bt_Run.Click += new System.EventHandler(this.bt_Run_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F);
            this.label1.Location = new System.Drawing.Point(22, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "# .pt=>.trt";
            // 
            // modelConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 192);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bt_Run);
            this.Controls.Add(this.tB_model);
            this.Controls.Add(this.bt_SelectModel);
            this.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.Name = "modelConvert";
            this.Text = "modelConvert";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_SelectModel;
        private System.Windows.Forms.TextBox tB_model;
        private System.Windows.Forms.Button bt_Run;
        private System.Windows.Forms.Label label1;
    }
}