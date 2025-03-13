using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    public partial class DataSetSplitter : Form
    {
        // 提供給外部或本身 new 出的 Plugin (FileDistributor) 與 MacroInstance
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
            // 1. 只做「設定參數」，不執行 Macro
            if (string.IsNullOrWhiteSpace(textBox1.Text) || !Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("請選擇有效的資料夾！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double trainRatio = (double)numTrain.Value / 100;
            double testRatio = (double)numTest.Value / 100;
            double valRatio = (double)numVal.Value / 100;

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

            // 此處可以根據 MacroInstance 進行額外判斷或設定（若需要的話）
            if (MacroInstance == null)
            {
                MessageBox.Show("MacroInstance 未設定，無法進行數據分配。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. 呼叫 Plugin 的 SetXXX(...) 方法，將參數寫入 MacroInstance
            Plugin.SetFolderPath(MacroInstance, textBox1.Text);
            Plugin.SetTrainRatio(MacroInstance, trainRatio);
            Plugin.SetTestRatio(MacroInstance, testRatio);
            Plugin.SetValRatio(MacroInstance, valRatio);

            // 4. 直接執行搬檔 (DoDistributeFilesDirect)；
            Plugin.DoDistributeFilesDirect(
                folderPath: textBox1.Text,
                trainRatio: trainRatio,
                testRatio: testRatio,
                valRatio: valRatio
            );

            //5 . 直接進入到各自的資料夾下把裡面的所有檔案的路徑寫到一個txt檔案裡面
            string baseDirectory = textBox1.Text;

            string[] subdirectories = { "test", "train", "val" };

            foreach (string subdir in subdirectories)
            {
                string subdirectoryPath = Path.Combine(baseDirectory, subdir);

                if (Directory.Exists(subdirectoryPath))
                {
                    string outputFile = Path.Combine(subdirectoryPath, $"{subdir}.txt");

                    string[] photoFiles = Directory.GetFiles(subdirectoryPath, "*.*")
                                                  .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                                                                  file.EndsWith(".psd", StringComparison.OrdinalIgnoreCase)
                                                                  )
                                                  .ToArray();

                    // Write photo file paths to the output file
                    using (StreamWriter sw = new StreamWriter(outputFile))
                    {
                        foreach (string file in photoFiles)
                        {
                            sw.WriteLine(file);
                        }
                    }

                    // Optional: Notify the user that the file was created
                    MessageBox.Show($"File paths have been written to {outputFile}");
                }
                else
                {
                    // If the subdirectory doesn't exist, log a message
                    MessageBox.Show($"Directory not found: {subdirectoryPath}");
                }
            }

            // 6. 執行完成後提示
            MessageBox.Show(
                "已於本次執行中『直接』搬檔至 train/test/val 資料夾！\n" +
                "（同時也已設定參數到 MacroInstance，供後續流程使用。）",
                "成功(直接搬檔)",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}
