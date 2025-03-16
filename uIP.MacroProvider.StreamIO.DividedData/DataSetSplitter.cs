using System;
using System.IO;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    public partial class DataSetSplitter : Form
    {
        // Macro 與 Plugin 供外部或本身 new 出時使用
        public UMacro MacroInstance { get; set; }
        public bool M_Flag { get; set; }
        public FileDistributor Plugin { get; set; }

        public DataSetSplitter()
        {
            InitializeComponent();
            M_Flag = true;
            Plugin = new FileDistributor();
            Plugin.Initialize(null);
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            // 驗證資料夾是否存在
            if (string.IsNullOrWhiteSpace(textBox1.Text) || !Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("請選擇有效的資料夾！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double trainRatio = (double)numTrain.Value / 100;
            double testRatio = (double)numTest.Value / 100;
            double valRatio = (double)numVal.Value / 100;

            // 驗證比例總和是否為 100%
            if (Math.Abs(trainRatio + testRatio + valRatio - 1.0) > 0.0001)
            {
                MessageBox.Show("Train/Test/Val 比例總和必須為 100%！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!M_Flag)
            {
                MessageBox.Show("數據分配功能已被停用。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MacroInstance == null)
            {
                MessageBox.Show("MacroInstance 未設定，無法進行數據分配。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 將參數寫入 MacroInstance (供後續流程使用)
            Plugin.SetFolderPath(MacroInstance, textBox1.Text);
            Plugin.SetTrainRatio(MacroInstance, trainRatio);
            Plugin.SetTestRatio(MacroInstance, testRatio);
            Plugin.SetValRatio(MacroInstance, valRatio);

            // 呼叫 Plugin 的公開方法進行檔案搬移及產生 txt 檔
            Plugin.DoDistributeFilesDirect(
                folderPath: textBox1.Text,
                trainRatio: trainRatio,
                testRatio: testRatio,
                valRatio: valRatio
            );
        }
    }
}

