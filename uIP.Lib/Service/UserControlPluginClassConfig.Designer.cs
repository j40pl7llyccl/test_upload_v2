namespace uIP.Lib.Service
{
    partial class UserControlPluginClassConfig
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
            this.tabControl_pluginClasses = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabControl_pluginClasses
            // 
            this.tabControl_pluginClasses.Location = new System.Drawing.Point(3, 3);
            this.tabControl_pluginClasses.Name = "tabControl_pluginClasses";
            this.tabControl_pluginClasses.SelectedIndex = 0;
            this.tabControl_pluginClasses.Size = new System.Drawing.Size(794, 594);
            this.tabControl_pluginClasses.TabIndex = 0;
            // 
            // UserControlPluginClassConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl_pluginClasses);
            this.Name = "UserControlPluginClassConfig";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_pluginClasses;
    }
}
