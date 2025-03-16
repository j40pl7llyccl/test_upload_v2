using System;
using System.Windows.Forms;
using uIP.MacroProvider.TrainConvert;

namespace uIP.MacroProvider.TrainingConvert
{
    public partial class modelConvert : Form
    {
        // Plugin 實例
        private uMProvidModelConvert _plugin;

        public modelConvert()
        {
            InitializeComponent();

            // 建立並初始化 Plugin 實例
            _plugin = new uMProvidModelConvert();
            _plugin.Initialize(null);

            bt_SelectModel.Click += new EventHandler(bt_SelectModel_Click);
            bt_Run.Click += new EventHandler(bt_Run_Click);
        }

        private void bt_SelectModel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Model files (*.h5;*.pt;*.onnx)|*.h5;*.pt;*.onnx|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tB_model.Text = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// 當使用者點擊 Run 按鈕時，呼叫 Plugin 執行轉檔邏輯
        /// </summary>
        private void bt_Run_Click(object sender, EventArgs e)
        {
            string inputModel = tB_model.Text;
            if (string.IsNullOrEmpty(inputModel))
            {
                MessageBox.Show("請選擇輸入模型!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool ok = _plugin.StartConvertByCode(inputModel, out string errMsg);
            if (ok)
            {
                MessageBox.Show("轉檔完成......");
            }
            else
            {
                MessageBox.Show("轉檔失敗: " + errMsg, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
