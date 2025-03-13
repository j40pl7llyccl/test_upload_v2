namespace uIP.MacroProvider.Layout.GenContainer
{
    partial class FormLayout3
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
            this.splitContainer_vertical = new System.Windows.Forms.SplitContainer();
            this.splitContainer_horizontal = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_vertical)).BeginInit();
            this.splitContainer_vertical.Panel1.SuspendLayout();
            this.splitContainer_vertical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_horizontal)).BeginInit();
            this.splitContainer_horizontal.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_vertical
            // 
            this.splitContainer_vertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_vertical.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_vertical.Name = "splitContainer_vertical";
            // 
            // splitContainer_vertical.Panel1
            // 
            this.splitContainer_vertical.Panel1.Controls.Add(this.splitContainer_horizontal);
            this.splitContainer_vertical.Size = new System.Drawing.Size(800, 450);
            this.splitContainer_vertical.SplitterDistance = 266;
            this.splitContainer_vertical.TabIndex = 0;
            // 
            // splitContainer_horizontal
            // 
            this.splitContainer_horizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_horizontal.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_horizontal.Name = "splitContainer_horizontal";
            this.splitContainer_horizontal.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer_horizontal.Size = new System.Drawing.Size(266, 450);
            this.splitContainer_horizontal.SplitterDistance = 88;
            this.splitContainer_horizontal.TabIndex = 0;
            // 
            // FormLayout3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer_vertical);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "FormLayout3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormLayout3";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLayout3_FormClosing);
            this.splitContainer_vertical.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_vertical)).EndInit();
            this.splitContainer_vertical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_horizontal)).EndInit();
            this.splitContainer_horizontal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_vertical;
        private System.Windows.Forms.SplitContainer splitContainer_horizontal;
    }
}