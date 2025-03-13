namespace uIP.MacroProvider.Tools.Labeling
{
    partial class UserControlLabelingEdit
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
            this.panel_container = new System.Windows.Forms.Panel();
            this.panel_drawing = new System.Windows.Forms.Panel();
            this.trackBar_zoom = new System.Windows.Forms.TrackBar();
            this.label_zoom = new System.Windows.Forms.Label();
            this.label_zoomV = new System.Windows.Forms.Label();
            this.label_labelList = new System.Windows.Forms.Label();
            this.groupBox_editLabel = new System.Windows.Forms.GroupBox();
            this.button_lableReplace = new System.Windows.Forms.Button();
            this.button_lablelDelete = new System.Windows.Forms.Button();
            this.button_labelAdd = new System.Windows.Forms.Button();
            this.label_desc = new System.Windows.Forms.Label();
            this.textBox_lableDesc = new System.Windows.Forms.TextBox();
            this.numericUpDown_labelV = new System.Windows.Forms.NumericUpDown();
            this.label_index = new System.Windows.Forms.Label();
            this.comboBox_labelList = new System.Windows.Forms.ComboBox();
            this.groupBox_regionType = new System.Windows.Forms.GroupBox();
            this.radioButton_polygon = new System.Windows.Forms.RadioButton();
            this.radioButton_rect = new System.Windows.Forms.RadioButton();
            this.label_regionList = new System.Windows.Forms.Label();
            this.comboBox_regionList = new System.Windows.Forms.ComboBox();
            this.button_rmvRegion = new System.Windows.Forms.Button();
            this.button_addRgn = new System.Windows.Forms.Button();
            this.button_resetPolygon = new System.Windows.Forms.Button();
            this.groupBox_changeLabel = new System.Windows.Forms.GroupBox();
            this.button_changeLabel = new System.Windows.Forms.Button();
            this.numericUpDown_changeLabelTo = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_changeLabelFrom = new System.Windows.Forms.NumericUpDown();
            this.label_changeLabelFrom2 = new System.Windows.Forms.Label();
            this.button_saveChange = new System.Windows.Forms.Button();
            this.checkBox_showAll = new System.Windows.Forms.CheckBox();
            this.panel_container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_zoom)).BeginInit();
            this.groupBox_editLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_labelV)).BeginInit();
            this.groupBox_regionType.SuspendLayout();
            this.groupBox_changeLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_changeLabelTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_changeLabelFrom)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_container
            // 
            this.panel_container.AutoScroll = true;
            this.panel_container.Controls.Add(this.panel_drawing);
            this.panel_container.Location = new System.Drawing.Point(3, 3);
            this.panel_container.Name = "panel_container";
            this.panel_container.Size = new System.Drawing.Size(960, 540);
            this.panel_container.TabIndex = 1;
            // 
            // panel_drawing
            // 
            this.panel_drawing.Location = new System.Drawing.Point(0, 0);
            this.panel_drawing.Name = "panel_drawing";
            this.panel_drawing.Size = new System.Drawing.Size(200, 100);
            this.panel_drawing.TabIndex = 0;
            this.panel_drawing.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_drawing_Paint);
            // 
            // trackBar_zoom
            // 
            this.trackBar_zoom.Location = new System.Drawing.Point(1064, 13);
            this.trackBar_zoom.Maximum = 4;
            this.trackBar_zoom.Name = "trackBar_zoom";
            this.trackBar_zoom.Size = new System.Drawing.Size(104, 45);
            this.trackBar_zoom.TabIndex = 2;
            this.trackBar_zoom.Value = 2;
            this.trackBar_zoom.ValueChanged += new System.EventHandler(this.trackBar_zoom_ValueChanged);
            // 
            // label_zoom
            // 
            this.label_zoom.AutoSize = true;
            this.label_zoom.Location = new System.Drawing.Point(969, 13);
            this.label_zoom.Name = "label_zoom";
            this.label_zoom.Size = new System.Drawing.Size(33, 12);
            this.label_zoom.TabIndex = 3;
            this.label_zoom.Text = "Zoom";
            // 
            // label_zoomV
            // 
            this.label_zoomV.AutoSize = true;
            this.label_zoomV.Location = new System.Drawing.Point(1031, 13);
            this.label_zoomV.Name = "label_zoomV";
            this.label_zoomV.Size = new System.Drawing.Size(11, 12);
            this.label_zoomV.TabIndex = 4;
            this.label_zoomV.Text = "1";
            // 
            // label_labelList
            // 
            this.label_labelList.AutoSize = true;
            this.label_labelList.Location = new System.Drawing.Point(969, 183);
            this.label_labelList.Name = "label_labelList";
            this.label_labelList.Size = new System.Drawing.Size(51, 12);
            this.label_labelList.TabIndex = 5;
            this.label_labelList.Text = "Label List";
            // 
            // groupBox_editLabel
            // 
            this.groupBox_editLabel.Controls.Add(this.button_lableReplace);
            this.groupBox_editLabel.Controls.Add(this.button_lablelDelete);
            this.groupBox_editLabel.Controls.Add(this.button_labelAdd);
            this.groupBox_editLabel.Controls.Add(this.label_desc);
            this.groupBox_editLabel.Controls.Add(this.textBox_lableDesc);
            this.groupBox_editLabel.Controls.Add(this.numericUpDown_labelV);
            this.groupBox_editLabel.Controls.Add(this.label_index);
            this.groupBox_editLabel.Location = new System.Drawing.Point(969, 74);
            this.groupBox_editLabel.Name = "groupBox_editLabel";
            this.groupBox_editLabel.Size = new System.Drawing.Size(253, 100);
            this.groupBox_editLabel.TabIndex = 6;
            this.groupBox_editLabel.TabStop = false;
            this.groupBox_editLabel.Text = "Edit Label";
            // 
            // button_lableReplace
            // 
            this.button_lableReplace.Location = new System.Drawing.Point(170, 72);
            this.button_lableReplace.Name = "button_lableReplace";
            this.button_lableReplace.Size = new System.Drawing.Size(75, 23);
            this.button_lableReplace.TabIndex = 8;
            this.button_lableReplace.Text = "Replace";
            this.button_lableReplace.UseVisualStyleBackColor = true;
            this.button_lableReplace.Click += new System.EventHandler(this.button_lableReplace_Click);
            // 
            // button_lablelDelete
            // 
            this.button_lablelDelete.Location = new System.Drawing.Point(89, 71);
            this.button_lablelDelete.Name = "button_lablelDelete";
            this.button_lablelDelete.Size = new System.Drawing.Size(75, 23);
            this.button_lablelDelete.TabIndex = 8;
            this.button_lablelDelete.Text = "Delete";
            this.button_lablelDelete.UseVisualStyleBackColor = true;
            this.button_lablelDelete.Click += new System.EventHandler(this.button_lablelDelete_Click);
            // 
            // button_labelAdd
            // 
            this.button_labelAdd.Location = new System.Drawing.Point(8, 72);
            this.button_labelAdd.Name = "button_labelAdd";
            this.button_labelAdd.Size = new System.Drawing.Size(75, 23);
            this.button_labelAdd.TabIndex = 8;
            this.button_labelAdd.Text = "Add";
            this.button_labelAdd.UseVisualStyleBackColor = true;
            this.button_labelAdd.Click += new System.EventHandler(this.button_labelAdd_Click);
            // 
            // label_desc
            // 
            this.label_desc.AutoSize = true;
            this.label_desc.Location = new System.Drawing.Point(6, 47);
            this.label_desc.Name = "label_desc";
            this.label_desc.Size = new System.Drawing.Size(58, 12);
            this.label_desc.TabIndex = 3;
            this.label_desc.Text = "Description";
            // 
            // textBox_lableDesc
            // 
            this.textBox_lableDesc.Location = new System.Drawing.Point(74, 44);
            this.textBox_lableDesc.Name = "textBox_lableDesc";
            this.textBox_lableDesc.Size = new System.Drawing.Size(171, 22);
            this.textBox_lableDesc.TabIndex = 2;
            // 
            // numericUpDown_labelV
            // 
            this.numericUpDown_labelV.Location = new System.Drawing.Point(74, 16);
            this.numericUpDown_labelV.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDown_labelV.Name = "numericUpDown_labelV";
            this.numericUpDown_labelV.Size = new System.Drawing.Size(171, 22);
            this.numericUpDown_labelV.TabIndex = 1;
            // 
            // label_index
            // 
            this.label_index.AutoSize = true;
            this.label_index.Location = new System.Drawing.Point(6, 18);
            this.label_index.Name = "label_index";
            this.label_index.Size = new System.Drawing.Size(32, 12);
            this.label_index.TabIndex = 0;
            this.label_index.Text = "Index";
            // 
            // comboBox_labelList
            // 
            this.comboBox_labelList.FormattingEnabled = true;
            this.comboBox_labelList.Location = new System.Drawing.Point(1033, 180);
            this.comboBox_labelList.Name = "comboBox_labelList";
            this.comboBox_labelList.Size = new System.Drawing.Size(181, 20);
            this.comboBox_labelList.TabIndex = 7;
            this.comboBox_labelList.SelectedIndexChanged += new System.EventHandler(this.comboBox_labelList_SelectedIndexChanged);
            // 
            // groupBox_regionType
            // 
            this.groupBox_regionType.Controls.Add(this.radioButton_polygon);
            this.groupBox_regionType.Controls.Add(this.radioButton_rect);
            this.groupBox_regionType.Location = new System.Drawing.Point(971, 216);
            this.groupBox_regionType.Name = "groupBox_regionType";
            this.groupBox_regionType.Size = new System.Drawing.Size(251, 51);
            this.groupBox_regionType.TabIndex = 8;
            this.groupBox_regionType.TabStop = false;
            this.groupBox_regionType.Text = "Region Type";
            // 
            // radioButton_polygon
            // 
            this.radioButton_polygon.AutoSize = true;
            this.radioButton_polygon.Location = new System.Drawing.Point(140, 21);
            this.radioButton_polygon.Name = "radioButton_polygon";
            this.radioButton_polygon.Size = new System.Drawing.Size(62, 16);
            this.radioButton_polygon.TabIndex = 0;
            this.radioButton_polygon.TabStop = true;
            this.radioButton_polygon.Text = "Polygon";
            this.radioButton_polygon.UseVisualStyleBackColor = true;
            this.radioButton_polygon.Click += new System.EventHandler(this.radioButton_rect_Click);
            // 
            // radioButton_rect
            // 
            this.radioButton_rect.AutoSize = true;
            this.radioButton_rect.Location = new System.Drawing.Point(6, 21);
            this.radioButton_rect.Name = "radioButton_rect";
            this.radioButton_rect.Size = new System.Drawing.Size(69, 16);
            this.radioButton_rect.TabIndex = 0;
            this.radioButton_rect.TabStop = true;
            this.radioButton_rect.Text = "Rectangle";
            this.radioButton_rect.UseVisualStyleBackColor = true;
            this.radioButton_rect.Click += new System.EventHandler(this.radioButton_rect_Click);
            // 
            // label_regionList
            // 
            this.label_regionList.AutoSize = true;
            this.label_regionList.Location = new System.Drawing.Point(979, 276);
            this.label_regionList.Name = "label_regionList";
            this.label_regionList.Size = new System.Drawing.Size(83, 12);
            this.label_regionList.TabIndex = 9;
            this.label_regionList.Text = "Labeled Regions";
            // 
            // comboBox_regionList
            // 
            this.comboBox_regionList.FormattingEnabled = true;
            this.comboBox_regionList.Location = new System.Drawing.Point(1068, 273);
            this.comboBox_regionList.Name = "comboBox_regionList";
            this.comboBox_regionList.Size = new System.Drawing.Size(99, 20);
            this.comboBox_regionList.TabIndex = 10;
            this.comboBox_regionList.SelectedIndexChanged += new System.EventHandler(this.comboBox_regionList_SelectedIndexChanged);
            // 
            // button_rmvRegion
            // 
            this.button_rmvRegion.Location = new System.Drawing.Point(1075, 299);
            this.button_rmvRegion.Name = "button_rmvRegion";
            this.button_rmvRegion.Size = new System.Drawing.Size(92, 23);
            this.button_rmvRegion.TabIndex = 11;
            this.button_rmvRegion.Text = "Delete(Ctrl+D)";
            this.button_rmvRegion.UseVisualStyleBackColor = true;
            this.button_rmvRegion.Click += new System.EventHandler(this.button_rmvRegion_Click);
            // 
            // button_addRgn
            // 
            this.button_addRgn.Location = new System.Drawing.Point(977, 299);
            this.button_addRgn.Name = "button_addRgn";
            this.button_addRgn.Size = new System.Drawing.Size(92, 23);
            this.button_addRgn.TabIndex = 12;
            this.button_addRgn.Text = "New(Ctrl+N)";
            this.button_addRgn.UseVisualStyleBackColor = true;
            this.button_addRgn.Click += new System.EventHandler(this.button_addRgn_Click);
            // 
            // button_resetPolygon
            // 
            this.button_resetPolygon.Enabled = false;
            this.button_resetPolygon.Location = new System.Drawing.Point(977, 328);
            this.button_resetPolygon.Name = "button_resetPolygon";
            this.button_resetPolygon.Size = new System.Drawing.Size(190, 23);
            this.button_resetPolygon.TabIndex = 13;
            this.button_resetPolygon.Text = "Reset polygon(Ctrl+R)";
            this.button_resetPolygon.UseVisualStyleBackColor = true;
            this.button_resetPolygon.Click += new System.EventHandler(this.button_resetPolygon_Click);
            // 
            // groupBox_changeLabel
            // 
            this.groupBox_changeLabel.Controls.Add(this.button_changeLabel);
            this.groupBox_changeLabel.Controls.Add(this.numericUpDown_changeLabelTo);
            this.groupBox_changeLabel.Controls.Add(this.numericUpDown_changeLabelFrom);
            this.groupBox_changeLabel.Controls.Add(this.label_changeLabelFrom2);
            this.groupBox_changeLabel.Location = new System.Drawing.Point(971, 377);
            this.groupBox_changeLabel.Name = "groupBox_changeLabel";
            this.groupBox_changeLabel.Size = new System.Drawing.Size(251, 100);
            this.groupBox_changeLabel.TabIndex = 14;
            this.groupBox_changeLabel.TabStop = false;
            this.groupBox_changeLabel.Text = "Change label";
            // 
            // button_changeLabel
            // 
            this.button_changeLabel.Location = new System.Drawing.Point(6, 60);
            this.button_changeLabel.Name = "button_changeLabel";
            this.button_changeLabel.Size = new System.Drawing.Size(231, 23);
            this.button_changeLabel.TabIndex = 10;
            this.button_changeLabel.Text = "Change";
            this.button_changeLabel.UseVisualStyleBackColor = true;
            this.button_changeLabel.Click += new System.EventHandler(this.button_changeLabel_Click);
            // 
            // numericUpDown_changeLabelTo
            // 
            this.numericUpDown_changeLabelTo.Location = new System.Drawing.Point(168, 29);
            this.numericUpDown_changeLabelTo.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDown_changeLabelTo.Name = "numericUpDown_changeLabelTo";
            this.numericUpDown_changeLabelTo.Size = new System.Drawing.Size(69, 22);
            this.numericUpDown_changeLabelTo.TabIndex = 9;
            // 
            // numericUpDown_changeLabelFrom
            // 
            this.numericUpDown_changeLabelFrom.Location = new System.Drawing.Point(87, 29);
            this.numericUpDown_changeLabelFrom.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDown_changeLabelFrom.Name = "numericUpDown_changeLabelFrom";
            this.numericUpDown_changeLabelFrom.Size = new System.Drawing.Size(69, 22);
            this.numericUpDown_changeLabelFrom.TabIndex = 9;
            // 
            // label_changeLabelFrom2
            // 
            this.label_changeLabelFrom2.AutoSize = true;
            this.label_changeLabelFrom2.Location = new System.Drawing.Point(6, 31);
            this.label_changeLabelFrom2.Name = "label_changeLabelFrom2";
            this.label_changeLabelFrom2.Size = new System.Drawing.Size(75, 12);
            this.label_changeLabelFrom2.TabIndex = 0;
            this.label_changeLabelFrom2.Text = "Label# from to";
            // 
            // button_saveChange
            // 
            this.button_saveChange.Location = new System.Drawing.Point(976, 488);
            this.button_saveChange.Name = "button_saveChange";
            this.button_saveChange.Size = new System.Drawing.Size(245, 54);
            this.button_saveChange.TabIndex = 15;
            this.button_saveChange.Text = "Save change";
            this.button_saveChange.UseVisualStyleBackColor = true;
            this.button_saveChange.Click += new System.EventHandler(this.button_saveChange_Click);
            // 
            // checkBox_showAll
            // 
            this.checkBox_showAll.AutoSize = true;
            this.checkBox_showAll.Location = new System.Drawing.Point(981, 357);
            this.checkBox_showAll.Name = "checkBox_showAll";
            this.checkBox_showAll.Size = new System.Drawing.Size(101, 16);
            this.checkBox_showAll.TabIndex = 16;
            this.checkBox_showAll.Text = "Show all regions";
            this.checkBox_showAll.UseVisualStyleBackColor = true;
            this.checkBox_showAll.Click += new System.EventHandler(this.checkBox_showAll_Click);
            // 
            // UserControlLabelingEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBox_showAll);
            this.Controls.Add(this.button_saveChange);
            this.Controls.Add(this.groupBox_changeLabel);
            this.Controls.Add(this.button_resetPolygon);
            this.Controls.Add(this.button_addRgn);
            this.Controls.Add(this.button_rmvRegion);
            this.Controls.Add(this.comboBox_regionList);
            this.Controls.Add(this.label_regionList);
            this.Controls.Add(this.groupBox_regionType);
            this.Controls.Add(this.comboBox_labelList);
            this.Controls.Add(this.groupBox_editLabel);
            this.Controls.Add(this.label_labelList);
            this.Controls.Add(this.label_zoomV);
            this.Controls.Add(this.label_zoom);
            this.Controls.Add(this.trackBar_zoom);
            this.Controls.Add(this.panel_container);
            this.Name = "UserControlLabelingEdit";
            this.Size = new System.Drawing.Size(1235, 548);
            this.panel_container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_zoom)).EndInit();
            this.groupBox_editLabel.ResumeLayout(false);
            this.groupBox_editLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_labelV)).EndInit();
            this.groupBox_regionType.ResumeLayout(false);
            this.groupBox_regionType.PerformLayout();
            this.groupBox_changeLabel.ResumeLayout(false);
            this.groupBox_changeLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_changeLabelTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_changeLabelFrom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel_container;
        private System.Windows.Forms.Panel panel_drawing;
        private System.Windows.Forms.TrackBar trackBar_zoom;
        private System.Windows.Forms.Label label_zoom;
        private System.Windows.Forms.Label label_zoomV;
        private System.Windows.Forms.Label label_labelList;
        private System.Windows.Forms.GroupBox groupBox_editLabel;
        private System.Windows.Forms.TextBox textBox_lableDesc;
        private System.Windows.Forms.NumericUpDown numericUpDown_labelV;
        private System.Windows.Forms.Label label_index;
        private System.Windows.Forms.ComboBox comboBox_labelList;
        private System.Windows.Forms.Button button_lableReplace;
        private System.Windows.Forms.Button button_lablelDelete;
        private System.Windows.Forms.Button button_labelAdd;
        private System.Windows.Forms.Label label_desc;
        private System.Windows.Forms.GroupBox groupBox_regionType;
        private System.Windows.Forms.RadioButton radioButton_polygon;
        private System.Windows.Forms.RadioButton radioButton_rect;
        private System.Windows.Forms.Label label_regionList;
        private System.Windows.Forms.ComboBox comboBox_regionList;
        private System.Windows.Forms.Button button_rmvRegion;
        private System.Windows.Forms.Button button_addRgn;
        private System.Windows.Forms.Button button_resetPolygon;
        private System.Windows.Forms.GroupBox groupBox_changeLabel;
        private System.Windows.Forms.Button button_changeLabel;
        private System.Windows.Forms.NumericUpDown numericUpDown_changeLabelTo;
        private System.Windows.Forms.NumericUpDown numericUpDown_changeLabelFrom;
        private System.Windows.Forms.Label label_changeLabelFrom2;
        private System.Windows.Forms.Button button_saveChange;
        private System.Windows.Forms.CheckBox checkBox_showAll;
    }
}
