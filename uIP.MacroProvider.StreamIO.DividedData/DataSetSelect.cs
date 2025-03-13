using System;
using System.IO;
using System.Windows.Forms;
// 假設這個命名空間下包含 UScriptService, UMacro 等
using uIP.Lib.Script;
// 你的 ProcessPath 可能位於同一專案, 這裡只要保證能找到 UMacro, UScriptService
//using uIP.MacroProvider.StreamIO.DividedData; // 若需要引用
using uIP.Lib.Service;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    public partial class DataSetSelect : Form
    {
        // 這裡示範保留 WinForm 原有 UI(3個textBox + 9個CheckBox)
        // 但最後不再直接呼叫 plugin.DoProcessPath
        // 而是動態建立一個 Macro "DatasetDev_ProcessPath" 來執行

        // 假設你能從外部取得 UScriptService
        // 你可以改成建構子參數、或屬性注入
        private UScriptService scriptService;
        public UMacro MacroInstance { get; set; }
        public ProcessPath Plugin { get; set; }

        public bool M_Flag { get; set; }


        public DataSetSelect(UScriptService scriptSrv = null)
        {
            InitializeComponent();
            scriptService = scriptSrv;
            M_Flag = true;
            Plugin = new ProcessPath();
            Plugin.Initialize(null);
        }

        private void label1_Click(object sender, EventArgs e)
        { /* ... */ }

        private void bt_directory1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fbd.SelectedPath;
                }
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        { /* ... */ }
        private void bt_directory2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }
        private void bt_directory3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox3.Text = fbd.SelectedPath;
                }
            }
        }

        private void HandleCheckBoxChanged(
            CheckBox current,
            CheckBox rowMate1, CheckBox rowMate2,
            CheckBox colMate1, CheckBox colMate2,
            string rowLabel,
            string colLabel
        )
        {
            if (current.Checked)
            {
                // (A) 同行不可多選
                if (rowMate1.Checked || rowMate2.Checked)
                {
                    MessageBox.Show(
                        $"{rowLabel} 已經勾選了其他類型，不能再勾選 {colLabel}！",
                        "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    current.Checked = false;
                    return;
                }
                // (B) 同列不可重複
                if (colMate1.Checked || colMate2.Checked)
                {
                    MessageBox.Show(
                        $"{colLabel} 已經在其他行被使用，無法重複勾選！",
                        "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning
                    );
                    current.Checked = false;
                    return;
                }
            }
        }

        // 第 1 行: Train=checkBox1, Test=checkBox2, Val=checkBox3
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                current: checkBox1,
                rowMate1: checkBox2,
                rowMate2: checkBox3,
                colMate1: checkBox4,
                colMate2: checkBox7,
                rowLabel: "第1行",
                colLabel: "Train"
            );
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox2,
                rowMate1: checkBox1,
                rowMate2: checkBox3,
                colMate1: checkBox5,
                colMate2: checkBox8,
                "第1行",
                "Test"
            );
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox3,
                rowMate1: checkBox1,
                rowMate2: checkBox2,
                colMate1: checkBox6,
                colMate2: checkBox9,
                "第1行",
                "Validation"
            );
        }
        // 第 2 行: Train=checkBox4, Test=checkBox5, Val=checkBox6
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox4,
                rowMate1: checkBox5,
                rowMate2: checkBox6,
                colMate1: checkBox1,
                colMate2: checkBox7,
                "第2行",
                "Train"
            );
        }
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox5,
                rowMate1: checkBox4,
                rowMate2: checkBox6,
                colMate1: checkBox2,
                colMate2: checkBox8,
                "第2行",
                "Test"
            );
        }
        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox6,
                rowMate1: checkBox4,
                rowMate2: checkBox5,
                colMate1: checkBox3,
                colMate2: checkBox9,
                "第2行",
                "Validation"
            );
        }
        // 第 3 行: Train=checkBox7, Test=checkBox8, Val=checkBox9
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox7,
                rowMate1: checkBox8,
                rowMate2: checkBox9,
                colMate1: checkBox1,
                colMate2: checkBox4,
                "第3行",
                "Train"
            );
        }
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox8,
                rowMate1: checkBox7,
                rowMate2: checkBox9,
                colMate1: checkBox2,
                colMate2: checkBox5,
                "第3行",
                "Test"
            );
        }
        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            HandleCheckBoxChanged(
                checkBox9,
                rowMate1: checkBox7,
                rowMate2: checkBox8,
                colMate1: checkBox3,
                colMate2: checkBox6,
                "第3行",
                "Validation"
            );
        }

        /// <summary>
        /// 計算該路徑檔案數(不含子資料夾)
        /// </summary>
        private int GetFileCount(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return 0;
            return Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Length;
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            if (!M_Flag)
            {
                MessageBox.Show("數據定位功能已被停用。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // 此處可以根據 MacroInstance 進行額外判斷或設定（若需要的話）
            if (MacroInstance == null)
            {
                MessageBox.Show("MacroInstance 未設定，無法進行影片提取。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 1) 讀取 CheckBox 狀態
            bool r1Train = checkBox1.Checked, r1Test = checkBox2.Checked, r1Val = checkBox3.Checked;
            bool r2Train = checkBox4.Checked, r2Test = checkBox5.Checked, r2Val = checkBox6.Checked;
            bool r3Train = checkBox7.Checked, r3Test = checkBox8.Checked, r3Val = checkBox9.Checked;

            // 2) 進行檢查(同一行不可多選、同一列不可重複)
            Func<bool, bool, bool, int> countChecks = (a, b, c) => (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0);

            if (countChecks(r1Train, r1Test, r1Val) > 1)
            {
                MessageBox.Show("第1行同時選了多個 Dataset，請修正。", "警告");
                return;
            }
            if (countChecks(r2Train, r2Test, r2Val) > 1)
            {
                MessageBox.Show("第2行同時選了多個 Dataset，請修正。", "警告");
                return;
            }
            if (countChecks(r3Train, r3Test, r3Val) > 1)
            {
                MessageBox.Show("第3行同時選了多個 Dataset，請修正。", "警告");
                return;
            }

            int trainRowCount = (r1Train ? 1 : 0) + (r2Train ? 1 : 0) + (r3Train ? 1 : 0);
            if (trainRowCount > 1)
            {
                MessageBox.Show("Train Dataset 重複使用於多個路徑，請修正。", "警告");
                return;
            }
            int testRowCount = (r1Test ? 1 : 0) + (r2Test ? 1 : 0) + (r3Test ? 1 : 0);
            if (testRowCount > 1)
            {
                MessageBox.Show("Test Dataset 重複使用於多個路徑，請修正。", "警告");
                return;
            }
            int valRowCount = (r1Val ? 1 : 0) + (r2Val ? 1 : 0) + (r3Val ? 1 : 0);
            if (valRowCount > 1)
            {
                MessageBox.Show("Validation Dataset 重複使用於多個路徑，請修正。", "警告");
                return;
            }

            // 3) 先計算三個路徑的檔案數
            int c1 = GetFileCount(textBox1.Text);
            int c2 = GetFileCount(textBox2.Text);
            int c3 = GetFileCount(textBox3.Text);

            int trainCount = 0, testCount = 0, valCount = 0;
            if (r1Train) trainCount += c1; if (r1Test) testCount += c1; if (r1Val) valCount += c1;
            if (r2Train) trainCount += c2; if (r2Test) testCount += c2; if (r2Val) valCount += c2;
            if (r3Train) trainCount += c3; if (r3Test) testCount += c3; if (r3Val) valCount += c3;

            int total = trainCount + testCount + valCount;
            if (total == 0)
            {
                MessageBox.Show("3個路徑的檔案數為 0 (或都沒勾選)。", "提醒");
                return;
            }

            double trainPercent = (trainCount * 100.0) / total;
            double testPercent = (testCount * 100.0) / total;
            double valPercent = (valCount * 100.0) / total;
            labelTrainPercent.Text = $"Train: {trainCount} 檔 (約 {trainPercent:0.0}%)";
            labelTestPercent.Text = $"Test : {testCount} 檔 (約 {testPercent:0.0}%)";
            labelValPercent.Text = $"Val  : {valCount} 檔 (約 {valPercent:0.0}%)";


            // ============================================================================
            // ★★★ Plugin + Macro 作法: 只要設定 MacroInstance 的參數，不在此手動執行
            // ============================================================================
            try
            {
                // 以 "SetMacroControlValue" 的方式呼叫對應 IoctrlSet_Path1, IoctrlSet_R1Train, ...

                Plugin.SetPath1(MacroInstance, textBox1.Text);//textBox1
                Plugin.SetR1Train(MacroInstance, r1Train);
                Plugin.SetR1Test(MacroInstance, r1Test);
                Plugin.SetR1Val(MacroInstance, r1Val);

                Plugin.SetPath2(MacroInstance, textBox2.Text);
                Plugin.SetR2Train(MacroInstance, r2Train);
                Plugin.SetR2Test(MacroInstance, r2Test);
                Plugin.SetR2Val(MacroInstance, r2Val);

                Plugin.SetPath3(MacroInstance, textBox3.Text);
                Plugin.SetR3Train(MacroInstance, r3Train);
                Plugin.SetR3Test(MacroInstance, r3Test);
                Plugin.SetR3Val(MacroInstance, r3Val);

                // 這裡只是在 GUI 中完成 "設定參數" 的工作。
                // 之後真正執行 Macro (呼叫 Macro_ProcessPath) 由 uIP 在腳本流程中進行。


                // (F) 進行搬檔
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    Plugin.DoProcessPath(textBox1.Text, r1Train, r1Test, r1Val);
                }
                if (!string.IsNullOrEmpty(textBox2.Text))
                {
                    Plugin.DoProcessPath(textBox2.Text, r2Train, r2Test, r2Val);
                }
                if (!string.IsNullOrEmpty(textBox3.Text))
                {
                    Plugin.DoProcessPath(textBox3.Text, r3Train, r3Test, r3Val);
                }

                // 提示完成
                MessageBox.Show("資料集分配完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定 Macro 參數時發生錯誤：{ex}", "錯誤");
                return;
            }

            // 關閉表單
            this.DialogResult = DialogResult.OK;
            this.Close();


        }
    }
}
