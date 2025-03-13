namespace uIP.MacroProvider.YZW.SOP.Detection
{
    partial class UsrCtrlRegionEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_drawContainer = new System.Windows.Forms.Panel();
            this.panel_editor = new System.Windows.Forms.Panel();
            this.groupBox_switchFunc = new System.Windows.Forms.GroupBox();
            this.radioButton_polygon = new System.Windows.Forms.RadioButton();
            this.radioButton_rectangle = new System.Windows.Forms.RadioButton();
            this.groupBox_region = new System.Windows.Forms.GroupBox();
            this.radioButton_ignoreRegion = new System.Windows.Forms.RadioButton();
            this.radioButton_workArea = new System.Windows.Forms.RadioButton();
            this.button_add = new System.Windows.Forms.Button();
            this.comboBox_workingRegion = new System.Windows.Forms.ComboBox();
            this.label_createdWorkRgn = new System.Windows.Forms.Label();
            this.comboBox_ignoreRegion = new System.Windows.Forms.ComboBox();
            this.label_ignoreRegion = new System.Windows.Forms.Label();
            this.button_remove = new System.Windows.Forms.Button();
            this.label_zoom = new System.Windows.Forms.Label();
            this.trackBar_zoom = new System.Windows.Forms.TrackBar();
            this.label_zoomV = new System.Windows.Forms.Label();
            this.button_loadImage = new System.Windows.Forms.Button();
            this.panel_drawContainer.SuspendLayout();
            this.groupBox_switchFunc.SuspendLayout();
            this.groupBox_region.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_zoom)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_drawContainer
            // 
            this.panel_drawContainer.AutoScroll = true;
            this.panel_drawContainer.Controls.Add(this.panel_editor);
            this.panel_drawContainer.Location = new System.Drawing.Point(172, 3);
            this.panel_drawContainer.Name = "panel_drawContainer";
            this.panel_drawContainer.Size = new System.Drawing.Size(980, 560);
            this.panel_drawContainer.TabIndex = 0;
            // 
            // panel_editor
            // 
            this.panel_editor.Location = new System.Drawing.Point(0, 0);
            this.panel_editor.Name = "panel_editor";
            this.panel_editor.Size = new System.Drawing.Size(200, 200);
            this.panel_editor.TabIndex = 0;
            this.panel_editor.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_editor_Paint);
            this.panel_editor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panel_editor_MouseClick);
            this.panel_editor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panel_editor_MouseDoubleClick);
            // 
            // groupBox_switchFunc
            // 
            this.groupBox_switchFunc.Controls.Add(this.radioButton_polygon);
            this.groupBox_switchFunc.Controls.Add(this.radioButton_rectangle);
            this.groupBox_switchFunc.Enabled = false;
            this.groupBox_switchFunc.Location = new System.Drawing.Point(3, 130);
            this.groupBox_switchFunc.Name = "groupBox_switchFunc";
            this.groupBox_switchFunc.Size = new System.Drawing.Size(163, 68);
            this.groupBox_switchFunc.TabIndex = 1;
            this.groupBox_switchFunc.TabStop = false;
            this.groupBox_switchFunc.Text = "Switch Function";
            // 
            // radioButton_polygon
            // 
            this.radioButton_polygon.AutoSize = true;
            this.radioButton_polygon.Location = new System.Drawing.Point(6, 43);
            this.radioButton_polygon.Name = "radioButton_polygon";
            this.radioButton_polygon.Size = new System.Drawing.Size(62, 16);
            this.radioButton_polygon.TabIndex = 1;
            this.radioButton_polygon.TabStop = true;
            this.radioButton_polygon.Tag = "";
            this.radioButton_polygon.Text = "Polygon";
            this.radioButton_polygon.UseVisualStyleBackColor = true;
            this.radioButton_polygon.Click += new System.EventHandler(this.radioButton_rectangle_Click);
            // 
            // radioButton_rectangle
            // 
            this.radioButton_rectangle.AutoSize = true;
            this.radioButton_rectangle.Location = new System.Drawing.Point(6, 21);
            this.radioButton_rectangle.Name = "radioButton_rectangle";
            this.radioButton_rectangle.Size = new System.Drawing.Size(69, 16);
            this.radioButton_rectangle.TabIndex = 1;
            this.radioButton_rectangle.TabStop = true;
            this.radioButton_rectangle.Tag = "";
            this.radioButton_rectangle.Text = "Rectangle";
            this.radioButton_rectangle.UseVisualStyleBackColor = true;
            this.radioButton_rectangle.Click += new System.EventHandler(this.radioButton_rectangle_Click);
            // 
            // groupBox_region
            // 
            this.groupBox_region.Controls.Add(this.radioButton_ignoreRegion);
            this.groupBox_region.Controls.Add(this.radioButton_workArea);
            this.groupBox_region.Location = new System.Drawing.Point(3, 56);
            this.groupBox_region.Name = "groupBox_region";
            this.groupBox_region.Size = new System.Drawing.Size(163, 68);
            this.groupBox_region.TabIndex = 1;
            this.groupBox_region.TabStop = false;
            this.groupBox_region.Text = "Region Type";
            // 
            // radioButton_ignoreRegion
            // 
            this.radioButton_ignoreRegion.AutoSize = true;
            this.radioButton_ignoreRegion.Location = new System.Drawing.Point(6, 43);
            this.radioButton_ignoreRegion.Name = "radioButton_ignoreRegion";
            this.radioButton_ignoreRegion.Size = new System.Drawing.Size(91, 16);
            this.radioButton_ignoreRegion.TabIndex = 1;
            this.radioButton_ignoreRegion.TabStop = true;
            this.radioButton_ignoreRegion.Tag = "";
            this.radioButton_ignoreRegion.Text = "Ignore Region";
            this.radioButton_ignoreRegion.UseVisualStyleBackColor = true;
            this.radioButton_ignoreRegion.Click += new System.EventHandler(this.radioButton_workArea_Click);
            // 
            // radioButton_workArea
            // 
            this.radioButton_workArea.AutoSize = true;
            this.radioButton_workArea.Location = new System.Drawing.Point(6, 21);
            this.radioButton_workArea.Name = "radioButton_workArea";
            this.radioButton_workArea.Size = new System.Drawing.Size(87, 16);
            this.radioButton_workArea.TabIndex = 1;
            this.radioButton_workArea.TabStop = true;
            this.radioButton_workArea.Tag = "";
            this.radioButton_workArea.Text = "Work Region";
            this.radioButton_workArea.UseVisualStyleBackColor = true;
            this.radioButton_workArea.Click += new System.EventHandler(this.radioButton_workArea_Click);
            // 
            // button_add
            // 
            this.button_add.BackColor = System.Drawing.SystemColors.Control;
            this.button_add.Location = new System.Drawing.Point(3, 204);
            this.button_add.Name = "button_add";
            this.button_add.Size = new System.Drawing.Size(163, 40);
            this.button_add.TabIndex = 1;
            this.button_add.Text = "Add";
            this.button_add.UseVisualStyleBackColor = false;
            this.button_add.Click += new System.EventHandler(this.button_add_Click);
            // 
            // comboBox_workingRegion
            // 
            this.comboBox_workingRegion.FormattingEnabled = true;
            this.comboBox_workingRegion.Location = new System.Drawing.Point(93, 259);
            this.comboBox_workingRegion.Name = "comboBox_workingRegion";
            this.comboBox_workingRegion.Size = new System.Drawing.Size(73, 20);
            this.comboBox_workingRegion.TabIndex = 1;
            this.comboBox_workingRegion.SelectedIndexChanged += new System.EventHandler(this.comboBox_workingRegion_SelectedIndexChanged);
            // 
            // label_createdWorkRgn
            // 
            this.label_createdWorkRgn.AutoSize = true;
            this.label_createdWorkRgn.BackColor = System.Drawing.Color.Lime;
            this.label_createdWorkRgn.Location = new System.Drawing.Point(3, 262);
            this.label_createdWorkRgn.Name = "label_createdWorkRgn";
            this.label_createdWorkRgn.Size = new System.Drawing.Size(84, 12);
            this.label_createdWorkRgn.TabIndex = 1;
            this.label_createdWorkRgn.Text = "Working Region";
            // 
            // comboBox_ignoreRegion
            // 
            this.comboBox_ignoreRegion.FormattingEnabled = true;
            this.comboBox_ignoreRegion.Location = new System.Drawing.Point(93, 285);
            this.comboBox_ignoreRegion.Name = "comboBox_ignoreRegion";
            this.comboBox_ignoreRegion.Size = new System.Drawing.Size(73, 20);
            this.comboBox_ignoreRegion.TabIndex = 1;
            this.comboBox_ignoreRegion.SelectedIndexChanged += new System.EventHandler(this.comboBox_workingRegion_SelectedIndexChanged);
            // 
            // label_ignoreRegion
            // 
            this.label_ignoreRegion.AutoSize = true;
            this.label_ignoreRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.label_ignoreRegion.Location = new System.Drawing.Point(3, 288);
            this.label_ignoreRegion.Name = "label_ignoreRegion";
            this.label_ignoreRegion.Size = new System.Drawing.Size(73, 12);
            this.label_ignoreRegion.TabIndex = 1;
            this.label_ignoreRegion.Text = "Ignore Region";
            // 
            // button_remove
            // 
            this.button_remove.Location = new System.Drawing.Point(3, 311);
            this.button_remove.Name = "button_remove";
            this.button_remove.Size = new System.Drawing.Size(163, 40);
            this.button_remove.TabIndex = 1;
            this.button_remove.Text = "Remove Select";
            this.button_remove.UseVisualStyleBackColor = true;
            this.button_remove.Click += new System.EventHandler(this.button_remove_Click);
            // 
            // label_zoom
            // 
            this.label_zoom.AutoSize = true;
            this.label_zoom.Location = new System.Drawing.Point(3, 391);
            this.label_zoom.Name = "label_zoom";
            this.label_zoom.Size = new System.Drawing.Size(33, 12);
            this.label_zoom.TabIndex = 1;
            this.label_zoom.Text = "Zoom";
            // 
            // trackBar_zoom
            // 
            this.trackBar_zoom.Location = new System.Drawing.Point(62, 382);
            this.trackBar_zoom.Maximum = 9;
            this.trackBar_zoom.Name = "trackBar_zoom";
            this.trackBar_zoom.Size = new System.Drawing.Size(104, 45);
            this.trackBar_zoom.TabIndex = 1;
            this.trackBar_zoom.Value = 5;
            this.trackBar_zoom.ValueChanged += new System.EventHandler(this.trackBar_zoom_ValueChanged);
            // 
            // label_zoomV
            // 
            this.label_zoomV.AutoSize = true;
            this.label_zoomV.Location = new System.Drawing.Point(42, 391);
            this.label_zoomV.Name = "label_zoomV";
            this.label_zoomV.Size = new System.Drawing.Size(11, 12);
            this.label_zoomV.TabIndex = 1;
            this.label_zoomV.Text = "1";
            // 
            // button_loadImage
            // 
            this.button_loadImage.Location = new System.Drawing.Point(5, 3);
            this.button_loadImage.Name = "button_loadImage";
            this.button_loadImage.Size = new System.Drawing.Size(163, 40);
            this.button_loadImage.TabIndex = 1;
            this.button_loadImage.Text = "Load Image";
            this.button_loadImage.UseVisualStyleBackColor = true;
            this.button_loadImage.Click += new System.EventHandler(this.button_loadImage_Click);
            // 
            // UsrCtrlRegionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_zoomV);
            this.Controls.Add(this.trackBar_zoom);
            this.Controls.Add(this.label_zoom);
            this.Controls.Add(this.label_ignoreRegion);
            this.Controls.Add(this.label_createdWorkRgn);
            this.Controls.Add(this.comboBox_ignoreRegion);
            this.Controls.Add(this.comboBox_workingRegion);
            this.Controls.Add(this.button_remove);
            this.Controls.Add(this.button_loadImage);
            this.Controls.Add(this.button_add);
            this.Controls.Add(this.groupBox_region);
            this.Controls.Add(this.groupBox_switchFunc);
            this.Controls.Add(this.panel_drawContainer);
            this.Name = "UsrCtrlRegionEditor";
            this.Size = new System.Drawing.Size(1171, 580);
            this.panel_drawContainer.ResumeLayout(false);
            this.groupBox_switchFunc.ResumeLayout(false);
            this.groupBox_switchFunc.PerformLayout();
            this.groupBox_region.ResumeLayout(false);
            this.groupBox_region.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_zoom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel_drawContainer;
        private System.Windows.Forms.Panel panel_editor;
        private System.Windows.Forms.GroupBox groupBox_switchFunc;
        private System.Windows.Forms.RadioButton radioButton_polygon;
        private System.Windows.Forms.RadioButton radioButton_rectangle;
        private System.Windows.Forms.GroupBox groupBox_region;
        private System.Windows.Forms.RadioButton radioButton_ignoreRegion;
        private System.Windows.Forms.RadioButton radioButton_workArea;
        private System.Windows.Forms.Button button_add;
        private System.Windows.Forms.ComboBox comboBox_workingRegion;
        private System.Windows.Forms.Label label_createdWorkRgn;
        private System.Windows.Forms.ComboBox comboBox_ignoreRegion;
        private System.Windows.Forms.Label label_ignoreRegion;
        private System.Windows.Forms.Button button_remove;
        private System.Windows.Forms.Label label_zoom;
        private System.Windows.Forms.TrackBar trackBar_zoom;
        private System.Windows.Forms.Label label_zoomV;
        private System.Windows.Forms.Button button_loadImage;
    }
}
