using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.Script;
using VideoFrameExtractor;


namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    public partial class FormConfVideoToFrame : Form
    {
        //private frmScriptEditor _scriptEditor;
        public Label LabelProgress => label_progress;
        public string LatestBrowerFolder { get; set; } = "";
        public string PickedDir { get { return string.IsNullOrEmpty(textBoxPickedDir.Text) ? "" : string.Copy(textBoxPickedDir.Text); } }
        //private bool m_flag = true;
        // 用來儲存使用者選取的影片資料夾路徑
        private string videoFolderPath = string.Empty;
        // Class-level variables to store m_flag and macro_instance

        //private object macro_instance; // Replace 'object' with the actual type of macro_instance

        // Other existing class-level variables
        private string _currentRunnerId;
        private object _scriptInstance;
        public UScript ScriptInstance { get; set; }

        public bool M_Flag { get; set; }

        public UMacro MacroInstance { get; set; }


        // VideoExtractor plugin 實例
        private VideoExtractor videoExtractor;


        public FormConfVideoToFrame()
        {
            InitializeComponent();
            //_scriptEditor = scriptEditor;



            videoExtractor = new VideoExtractor();
            videoExtractor.Initialize(null);

            M_Flag = true;

        }

        private void intervalTextBox_TextChanged(object sender, EventArgs e)
        {

            string get_intervalTextBoxtext = intervalTextBox.Text;

        }

        private void videoPathLabel_Click(object sender, EventArgs e)
        {

        }


        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (!M_Flag)
            {
                MessageBox.Show("影片提取功能已被停用。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 此處可以根據 MacroInstance 進行額外判斷或設定（若需要的話）
            if (MacroInstance == null)
            {
                MessageBox.Show("MacroInstance 未設定，無法進行影片提取。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 檢查影片資料夾是否正確選擇
            if (string.IsNullOrEmpty(textBoxPickedDir.Text) || !Directory.Exists(textBoxPickedDir.Text))
            {
                MessageBox.Show("請先選擇正確的影片資料夾！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 檢查切割秒數是否有效
            if (!double.TryParse(intervalTextBox.Text, out double intervalSeconds) || intervalSeconds <= 0)
            {
                MessageBox.Show("請輸入有效且大於0的切割秒數！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 建立輸出資料夾
            //string outputFolder = Path.Combine(textBoxPickedDir.Text, "Output");
            string outputFolder = Path.Combine(System.Windows.Forms.Application.StartupPath, "Video_Output");
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // 在背景執行影片處理作業
            Task.Run(() =>
            {
                try
                {
                    // 假設 videoExtractor 已經實例化並具備 ProcessVideos 方法
                    videoExtractor.ProcessVideos(textBoxPickedDir.Text, outputFolder, intervalSeconds, MacroInstance);
                    // 取得指定目錄下所有檔案
                    //string[] videofiles = Directory.GetFiles(textBoxPickedDir.Text);
                    // 呼叫修改後的 writeTimestampToini 方法處理所有影片
                    //videoExtractor.writeTimestampToini(videofiles, outputFolder, intervalSeconds, MacroInstance);

                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("所有影片處理完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"影片處理發生錯誤：{ex.Message}\n詳細資訊：{ex.StackTrace}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });

        }


        private void intervalLabel_Click(object sender, EventArgs e)
        {

        }

        private void textBoxPickedDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void selectVideoButton_Click(object sender, EventArgs e)
        {
            /*--
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(PickedDir) && Directory.Exists(PickedDir))
                dlg.SelectedPath = PickedDir;
            else if (!string.IsNullOrEmpty(LatestBrowerFolder) && Directory.Exists(LatestBrowerFolder))
                dlg.SelectedPath = LatestBrowerFolder;
            
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
            {
                textBoxPickedDir.Text = string.Copy(dlg.SelectedPath);

                LatestBrowerFolder = Path.GetDirectoryName(dlg.SelectedPath);
            }
            dlg.Dispose();--*/
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(PickedDir) && Directory.Exists(PickedDir))
                dlg.SelectedPath = PickedDir;
            else if (!string.IsNullOrEmpty(LatestBrowerFolder) && Directory.Exists(LatestBrowerFolder))
                dlg.SelectedPath = LatestBrowerFolder;

            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
            {
                // 檢查選擇的路徑是資料夾還是檔案
                if (Directory.Exists(dlg.SelectedPath))
                {
                    // 選擇的是資料夾
                    textBoxPickedDir.Text = string.Copy(dlg.SelectedPath);
                    LatestBrowerFolder = Path.GetDirectoryName(dlg.SelectedPath);
                }
                else if (File.Exists(dlg.SelectedPath))
                {
                    // 選擇的是檔案
                    textBoxPickedDir.Text = string.Copy(dlg.SelectedPath);
                    LatestBrowerFolder = Path.GetDirectoryName(dlg.SelectedPath);
                }
                else
                {
                    MessageBox.Show("選擇的路徑無效！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            dlg.Dispose();

        }

    }
}


