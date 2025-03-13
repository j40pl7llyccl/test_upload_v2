namespace uIP.MacroProvider.YZW.SOP.Detection
{
    partial class UserControlSopDetectParams
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
            this.label_objRgnJudge = new System.Windows.Forms.Label();
            this.comboBox_judgeObjRgn = new System.Windows.Forms.ComboBox();
            this.button_openRgnConfig = new System.Windows.Forms.Button();
            this.label_waferMinArea = new System.Windows.Forms.Label();
            this.numericUpDown_waferMinArea = new System.Windows.Forms.NumericUpDown();
            this.label_waferPenMaxAcceptDist = new System.Windows.Forms.Label();
            this.numericUpDown_waferPenMaxAcceptDist = new System.Windows.Forms.NumericUpDown();
            this.groupBox_triggerCond = new System.Windows.Forms.GroupBox();
            this.button_apply = new System.Windows.Forms.Button();
            this.button_condReplace = new System.Windows.Forms.Button();
            this.button_condAdd = new System.Windows.Forms.Button();
            this.label_condGivenDesc = new System.Windows.Forms.Label();
            this.textBox_condGivenName = new System.Windows.Forms.TextBox();
            this.label_condType = new System.Windows.Forms.Label();
            this.comboBox_condType = new System.Windows.Forms.ComboBox();
            this.label_condCountThresh = new System.Windows.Forms.Label();
            this.numericUpDown_condCountThreshold = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_condTimeInterval = new System.Windows.Forms.NumericUpDown();
            this.label_condTimeInterval = new System.Windows.Forms.Label();
            this.comboBox_condAvailable = new System.Windows.Forms.ComboBox();
            this.label_availableCond = new System.Windows.Forms.Label();
            this.button_removeCond = new System.Windows.Forms.Button();
            this.comboBox_whichWR = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_condList = new System.Windows.Forms.ComboBox();
            this.label_conditionList = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_waferPenIou = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waferMinArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waferPenMaxAcceptDist)).BeginInit();
            this.groupBox_triggerCond.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_condCountThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_condTimeInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waferPenIou)).BeginInit();
            this.SuspendLayout();
            // 
            // label_objRgnJudge
            // 
            this.label_objRgnJudge.AutoSize = true;
            this.label_objRgnJudge.Location = new System.Drawing.Point(13, 6);
            this.label_objRgnJudge.Name = "label_objRgnJudge";
            this.label_objRgnJudge.Size = new System.Drawing.Size(96, 12);
            this.label_objRgnJudge.TabIndex = 0;
            this.label_objRgnJudge.Text = "Judge object region";
            // 
            // comboBox_judgeObjRgn
            // 
            this.comboBox_judgeObjRgn.FormattingEnabled = true;
            this.comboBox_judgeObjRgn.Location = new System.Drawing.Point(132, 3);
            this.comboBox_judgeObjRgn.Name = "comboBox_judgeObjRgn";
            this.comboBox_judgeObjRgn.Size = new System.Drawing.Size(176, 20);
            this.comboBox_judgeObjRgn.TabIndex = 1;
            // 
            // button_openRgnConfig
            // 
            this.button_openRgnConfig.Location = new System.Drawing.Point(15, 34);
            this.button_openRgnConfig.Name = "button_openRgnConfig";
            this.button_openRgnConfig.Size = new System.Drawing.Size(293, 36);
            this.button_openRgnConfig.TabIndex = 2;
            this.button_openRgnConfig.Text = "Open region config";
            this.button_openRgnConfig.UseVisualStyleBackColor = true;
            this.button_openRgnConfig.Click += new System.EventHandler(this.button_openRgnConfig_Click);
            // 
            // label_waferMinArea
            // 
            this.label_waferMinArea.AutoSize = true;
            this.label_waferMinArea.Location = new System.Drawing.Point(11, 51);
            this.label_waferMinArea.Name = "label_waferMinArea";
            this.label_waferMinArea.Size = new System.Drawing.Size(77, 12);
            this.label_waferMinArea.TabIndex = 3;
            this.label_waferMinArea.Text = "Wafer min area";
            // 
            // numericUpDown_waferMinArea
            // 
            this.numericUpDown_waferMinArea.Location = new System.Drawing.Point(172, 49);
            this.numericUpDown_waferMinArea.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_waferMinArea.Name = "numericUpDown_waferMinArea";
            this.numericUpDown_waferMinArea.Size = new System.Drawing.Size(101, 22);
            this.numericUpDown_waferMinArea.TabIndex = 4;
            this.numericUpDown_waferMinArea.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // label_waferPenMaxAcceptDist
            // 
            this.label_waferPenMaxAcceptDist.AutoSize = true;
            this.label_waferPenMaxAcceptDist.Location = new System.Drawing.Point(11, 79);
            this.label_waferPenMaxAcceptDist.Name = "label_waferPenMaxAcceptDist";
            this.label_waferPenMaxAcceptDist.Size = new System.Drawing.Size(136, 12);
            this.label_waferPenMaxAcceptDist.TabIndex = 3;
            this.label_waferPenMaxAcceptDist.Text = "Wafer <-> Pen max distance";
            // 
            // numericUpDown_waferPenMaxAcceptDist
            // 
            this.numericUpDown_waferPenMaxAcceptDist.Location = new System.Drawing.Point(172, 77);
            this.numericUpDown_waferPenMaxAcceptDist.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.numericUpDown_waferPenMaxAcceptDist.Name = "numericUpDown_waferPenMaxAcceptDist";
            this.numericUpDown_waferPenMaxAcceptDist.Size = new System.Drawing.Size(101, 22);
            this.numericUpDown_waferPenMaxAcceptDist.TabIndex = 4;
            this.numericUpDown_waferPenMaxAcceptDist.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // groupBox_triggerCond
            // 
            this.groupBox_triggerCond.Controls.Add(this.button_apply);
            this.groupBox_triggerCond.Controls.Add(this.button_condReplace);
            this.groupBox_triggerCond.Controls.Add(this.numericUpDown_waferPenIou);
            this.groupBox_triggerCond.Controls.Add(this.numericUpDown_waferPenMaxAcceptDist);
            this.groupBox_triggerCond.Controls.Add(this.label2);
            this.groupBox_triggerCond.Controls.Add(this.button_condAdd);
            this.groupBox_triggerCond.Controls.Add(this.label_waferPenMaxAcceptDist);
            this.groupBox_triggerCond.Controls.Add(this.label_condGivenDesc);
            this.groupBox_triggerCond.Controls.Add(this.numericUpDown_waferMinArea);
            this.groupBox_triggerCond.Controls.Add(this.label_waferMinArea);
            this.groupBox_triggerCond.Controls.Add(this.textBox_condGivenName);
            this.groupBox_triggerCond.Controls.Add(this.label_condType);
            this.groupBox_triggerCond.Controls.Add(this.comboBox_condType);
            this.groupBox_triggerCond.Controls.Add(this.label_condCountThresh);
            this.groupBox_triggerCond.Controls.Add(this.numericUpDown_condCountThreshold);
            this.groupBox_triggerCond.Controls.Add(this.numericUpDown_condTimeInterval);
            this.groupBox_triggerCond.Controls.Add(this.label_condTimeInterval);
            this.groupBox_triggerCond.Controls.Add(this.comboBox_condAvailable);
            this.groupBox_triggerCond.Controls.Add(this.label_availableCond);
            this.groupBox_triggerCond.Controls.Add(this.button_removeCond);
            this.groupBox_triggerCond.Controls.Add(this.comboBox_whichWR);
            this.groupBox_triggerCond.Controls.Add(this.label1);
            this.groupBox_triggerCond.Controls.Add(this.comboBox_condList);
            this.groupBox_triggerCond.Controls.Add(this.label_conditionList);
            this.groupBox_triggerCond.Location = new System.Drawing.Point(3, 76);
            this.groupBox_triggerCond.Name = "groupBox_triggerCond";
            this.groupBox_triggerCond.Size = new System.Drawing.Size(308, 270);
            this.groupBox_triggerCond.TabIndex = 5;
            this.groupBox_triggerCond.TabStop = false;
            this.groupBox_triggerCond.Text = "Working Region Config";
            // 
            // button_apply
            // 
            this.button_apply.Location = new System.Drawing.Point(252, 20);
            this.button_apply.Name = "button_apply";
            this.button_apply.Size = new System.Drawing.Size(53, 23);
            this.button_apply.TabIndex = 6;
            this.button_apply.Text = "Apply";
            this.button_apply.UseVisualStyleBackColor = true;
            this.button_apply.Click += new System.EventHandler(this.button_apply_Click);
            // 
            // button_condReplace
            // 
            this.button_condReplace.Location = new System.Drawing.Point(253, 132);
            this.button_condReplace.Name = "button_condReplace";
            this.button_condReplace.Size = new System.Drawing.Size(53, 23);
            this.button_condReplace.TabIndex = 13;
            this.button_condReplace.Text = "Replace";
            this.button_condReplace.UseVisualStyleBackColor = true;
            this.button_condReplace.Click += new System.EventHandler(this.button_condReplace_Click);
            // 
            // button_condAdd
            // 
            this.button_condAdd.Location = new System.Drawing.Point(135, 132);
            this.button_condAdd.Name = "button_condAdd";
            this.button_condAdd.Size = new System.Drawing.Size(53, 23);
            this.button_condAdd.TabIndex = 13;
            this.button_condAdd.Text = "Add";
            this.button_condAdd.UseVisualStyleBackColor = true;
            this.button_condAdd.Click += new System.EventHandler(this.button_condAdd_Click);
            // 
            // label_condGivenDesc
            // 
            this.label_condGivenDesc.AutoSize = true;
            this.label_condGivenDesc.Location = new System.Drawing.Point(8, 245);
            this.label_condGivenDesc.Name = "label_condGivenDesc";
            this.label_condGivenDesc.Size = new System.Drawing.Size(61, 12);
            this.label_condGivenDesc.TabIndex = 12;
            this.label_condGivenDesc.Text = "Given Desc.";
            this.label_condGivenDesc.Visible = false;
            // 
            // textBox_condGivenName
            // 
            this.textBox_condGivenName.Location = new System.Drawing.Point(108, 242);
            this.textBox_condGivenName.Name = "textBox_condGivenName";
            this.textBox_condGivenName.Size = new System.Drawing.Size(165, 22);
            this.textBox_condGivenName.TabIndex = 11;
            this.textBox_condGivenName.Visible = false;
            // 
            // label_condType
            // 
            this.label_condType.AutoSize = true;
            this.label_condType.Location = new System.Drawing.Point(192, 188);
            this.label_condType.Name = "label_condType";
            this.label_condType.Size = new System.Drawing.Size(29, 12);
            this.label_condType.TabIndex = 10;
            this.label_condType.Text = "Type";
            this.label_condType.Visible = false;
            // 
            // comboBox_condType
            // 
            this.comboBox_condType.FormattingEnabled = true;
            this.comboBox_condType.Location = new System.Drawing.Point(227, 185);
            this.comboBox_condType.Name = "comboBox_condType";
            this.comboBox_condType.Size = new System.Drawing.Size(68, 20);
            this.comboBox_condType.TabIndex = 9;
            this.comboBox_condType.Visible = false;
            // 
            // label_condCountThresh
            // 
            this.label_condCountThresh.AutoSize = true;
            this.label_condCountThresh.Location = new System.Drawing.Point(4, 216);
            this.label_condCountThresh.Name = "label_condCountThresh";
            this.label_condCountThresh.Size = new System.Drawing.Size(98, 12);
            this.label_condCountThresh.TabIndex = 8;
            this.label_condCountThresh.Text = "Count Threshold(>)";
            // 
            // numericUpDown_condCountThreshold
            // 
            this.numericUpDown_condCountThreshold.Location = new System.Drawing.Point(108, 214);
            this.numericUpDown_condCountThreshold.Name = "numericUpDown_condCountThreshold";
            this.numericUpDown_condCountThreshold.Size = new System.Drawing.Size(68, 22);
            this.numericUpDown_condCountThreshold.TabIndex = 7;
            // 
            // numericUpDown_condTimeInterval
            // 
            this.numericUpDown_condTimeInterval.DecimalPlaces = 1;
            this.numericUpDown_condTimeInterval.Location = new System.Drawing.Point(108, 186);
            this.numericUpDown_condTimeInterval.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown_condTimeInterval.Name = "numericUpDown_condTimeInterval";
            this.numericUpDown_condTimeInterval.Size = new System.Drawing.Size(68, 22);
            this.numericUpDown_condTimeInterval.TabIndex = 6;
            // 
            // label_condTimeInterval
            // 
            this.label_condTimeInterval.AutoSize = true;
            this.label_condTimeInterval.Location = new System.Drawing.Point(4, 188);
            this.label_condTimeInterval.Name = "label_condTimeInterval";
            this.label_condTimeInterval.Size = new System.Drawing.Size(89, 12);
            this.label_condTimeInterval.TabIndex = 5;
            this.label_condTimeInterval.Text = "Time interval(sec)";
            // 
            // comboBox_condAvailable
            // 
            this.comboBox_condAvailable.FormattingEnabled = true;
            this.comboBox_condAvailable.Location = new System.Drawing.Point(99, 160);
            this.comboBox_condAvailable.Name = "comboBox_condAvailable";
            this.comboBox_condAvailable.Size = new System.Drawing.Size(174, 20);
            this.comboBox_condAvailable.TabIndex = 4;
            // 
            // label_availableCond
            // 
            this.label_availableCond.AutoSize = true;
            this.label_availableCond.Location = new System.Drawing.Point(12, 163);
            this.label_availableCond.Name = "label_availableCond";
            this.label_availableCond.Size = new System.Drawing.Size(81, 12);
            this.label_availableCond.TabIndex = 3;
            this.label_availableCond.Text = "Available Cond.";
            // 
            // button_removeCond
            // 
            this.button_removeCond.Location = new System.Drawing.Point(194, 132);
            this.button_removeCond.Name = "button_removeCond";
            this.button_removeCond.Size = new System.Drawing.Size(53, 23);
            this.button_removeCond.TabIndex = 2;
            this.button_removeCond.Text = "Remove";
            this.button_removeCond.UseVisualStyleBackColor = true;
            this.button_removeCond.Click += new System.EventHandler(this.button_removeCond_Click);
            // 
            // comboBox_whichWR
            // 
            this.comboBox_whichWR.FormattingEnabled = true;
            this.comboBox_whichWR.Location = new System.Drawing.Point(134, 23);
            this.comboBox_whichWR.Name = "comboBox_whichWR";
            this.comboBox_whichWR.Size = new System.Drawing.Size(112, 20);
            this.comboBox_whichWR.TabIndex = 1;
            this.comboBox_whichWR.SelectedIndexChanged += new System.EventHandler(this.comboBox_whichWR_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Config for work region";
            // 
            // comboBox_condList
            // 
            this.comboBox_condList.FormattingEnabled = true;
            this.comboBox_condList.Location = new System.Drawing.Point(71, 134);
            this.comboBox_condList.Name = "comboBox_condList";
            this.comboBox_condList.Size = new System.Drawing.Size(58, 20);
            this.comboBox_condList.TabIndex = 1;
            this.comboBox_condList.SelectedIndexChanged += new System.EventHandler(this.comboBox_condList_SelectedIndexChanged);
            // 
            // label_conditionList
            // 
            this.label_conditionList.AutoSize = true;
            this.label_conditionList.Location = new System.Drawing.Point(12, 137);
            this.label_conditionList.Name = "label_conditionList";
            this.label_conditionList.Size = new System.Drawing.Size(53, 12);
            this.label_conditionList.TabIndex = 0;
            this.label_conditionList.Text = "Conditons";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 107);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(142, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Wafer <-> Pen IOU threshold";
            // 
            // numericUpDown_waferPenIou
            // 
            this.numericUpDown_waferPenIou.Location = new System.Drawing.Point(172, 105);
            this.numericUpDown_waferPenIou.Name = "numericUpDown_waferPenIou";
            this.numericUpDown_waferPenIou.Size = new System.Drawing.Size(101, 22);
            this.numericUpDown_waferPenIou.TabIndex = 4;
            this.numericUpDown_waferPenIou.Value = new decimal(new int[] {
            11,
            0,
            0,
            0});
            // 
            // UserControlSopDetectParams
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox_triggerCond);
            this.Controls.Add(this.button_openRgnConfig);
            this.Controls.Add(this.comboBox_judgeObjRgn);
            this.Controls.Add(this.label_objRgnJudge);
            this.Name = "UserControlSopDetectParams";
            this.Size = new System.Drawing.Size(315, 349);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waferMinArea)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waferPenMaxAcceptDist)).EndInit();
            this.groupBox_triggerCond.ResumeLayout(false);
            this.groupBox_triggerCond.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_condCountThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_condTimeInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_waferPenIou)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_objRgnJudge;
        private System.Windows.Forms.ComboBox comboBox_judgeObjRgn;
        private System.Windows.Forms.Button button_openRgnConfig;
        private System.Windows.Forms.Label label_waferMinArea;
        private System.Windows.Forms.NumericUpDown numericUpDown_waferMinArea;
        private System.Windows.Forms.Label label_waferPenMaxAcceptDist;
        private System.Windows.Forms.NumericUpDown numericUpDown_waferPenMaxAcceptDist;
        private System.Windows.Forms.GroupBox groupBox_triggerCond;
        private System.Windows.Forms.ComboBox comboBox_condList;
        private System.Windows.Forms.Label label_conditionList;
        private System.Windows.Forms.ComboBox comboBox_condAvailable;
        private System.Windows.Forms.Label label_availableCond;
        private System.Windows.Forms.Button button_removeCond;
        private System.Windows.Forms.Label label_condCountThresh;
        private System.Windows.Forms.NumericUpDown numericUpDown_condCountThreshold;
        private System.Windows.Forms.NumericUpDown numericUpDown_condTimeInterval;
        private System.Windows.Forms.Label label_condTimeInterval;
        private System.Windows.Forms.ComboBox comboBox_condType;
        private System.Windows.Forms.Button button_condReplace;
        private System.Windows.Forms.Button button_condAdd;
        private System.Windows.Forms.Label label_condGivenDesc;
        private System.Windows.Forms.TextBox textBox_condGivenName;
        private System.Windows.Forms.Label label_condType;
        private System.Windows.Forms.ComboBox comboBox_whichWR;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_apply;
        private System.Windows.Forms.NumericUpDown numericUpDown_waferPenIou;
        private System.Windows.Forms.Label label2;
    }
}
