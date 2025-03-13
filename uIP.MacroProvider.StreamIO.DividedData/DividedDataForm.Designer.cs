namespace uIP.MacroProvider.StreamIO.DividedData
{
    partial class DividedDataForm
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
            this.bt_Select = new Sunny.UI.UISymbolButton();
            this.bt_Auto = new Sunny.UI.UISymbolButton();
            this.uiSymbolButton3 = new Sunny.UI.UISymbolButton();
            this.SuspendLayout();
            // 
            // bt_Select
            // 
            this.bt_Select.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_Select.FillColor = System.Drawing.Color.Gray;
            this.bt_Select.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.bt_Select.Location = new System.Drawing.Point(413, 161);
            this.bt_Select.MinimumSize = new System.Drawing.Size(1, 1);
            this.bt_Select.Name = "bt_Select";
            this.bt_Select.Radius = 20;
            this.bt_Select.RectColor = System.Drawing.Color.White;
            this.bt_Select.Size = new System.Drawing.Size(114, 55);
            this.bt_Select.Symbol = 558740;
            this.bt_Select.SymbolDisableColor = System.Drawing.Color.White;
            this.bt_Select.TabIndex = 4;
            this.bt_Select.Text = "Categorize";
            this.bt_Select.TipsFont = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.bt_Select.Click += new System.EventHandler(this.bt_Select_Click);
            // 
            // bt_Auto
            // 
            this.bt_Auto.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_Auto.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.bt_Auto.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.bt_Auto.Location = new System.Drawing.Point(127, 161);
            this.bt_Auto.MinimumSize = new System.Drawing.Size(1, 1);
            this.bt_Auto.Name = "bt_Auto";
            this.bt_Auto.Radius = 20;
            this.bt_Auto.RectColor = System.Drawing.Color.White;
            this.bt_Auto.Size = new System.Drawing.Size(114, 55);
            this.bt_Auto.Symbol = 62101;
            this.bt_Auto.TabIndex = 5;
            this.bt_Auto.Text = "Set ration";
            this.bt_Auto.TipsFont = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.bt_Auto.Click += new System.EventHandler(this.bt_Auto_Click);
            // 
            // uiSymbolButton3
            // 
            this.uiSymbolButton3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiSymbolButton3.FillColor = System.Drawing.Color.Violet;
            this.uiSymbolButton3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.uiSymbolButton3.Location = new System.Drawing.Point(127, 308);
            this.uiSymbolButton3.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiSymbolButton3.Name = "uiSymbolButton3";
            this.uiSymbolButton3.Size = new System.Drawing.Size(400, 35);
            this.uiSymbolButton3.Symbol = 561277;
            this.uiSymbolButton3.TabIndex = 6;
            this.uiSymbolButton3.Text = "Next";
            this.uiSymbolButton3.TipsFont = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            // 
            // DividedDataForm
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(648, 377);
            this.Controls.Add(this.uiSymbolButton3);
            this.Controls.Add(this.bt_Auto);
            this.Controls.Add(this.bt_Select);
            this.Name = "DividedDataForm";
            this.Text = "DividedDataForm";
            this.Load += new System.EventHandler(this.DividedDataForm_Load);
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bt_Next;
        private System.Windows.Forms.Label label2;
        private Sunny.UI.UISymbolButton bt_Select;
        private Sunny.UI.UISymbolButton bt_Auto;
        private Sunny.UI.UISymbolButton uiSymbolButton3;
    }
}

