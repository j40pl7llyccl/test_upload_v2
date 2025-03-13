using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO;

namespace uIP.MacroProvider.TrainingConvert
{
    public partial class modelConvert : Form
    {
        public modelConvert()
        {
            InitializeComponent();
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

        private  void bt_Run_Click(object sender, EventArgs e)
        {
            try
            {
                string pythonExe = @"python"; // 確保這是正確的 Python 路徑
                string scriptPath = @"C:\Users\MIP4070\Desktop\yolov5-master\export.py";
                string inputModel = tB_model.Text;

                if (string.IsNullOrEmpty(inputModel))
                {
                    MessageBox.Show("請選擇輸入模型並指定輸出模型名稱!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string folderPath = Path.GetDirectoryName(inputModel);
                string fileName = Path.GetFileNameWithoutExtension(inputModel);
                string onnxFilePath = Path.Combine(folderPath, fileName + ".onnx");
                string trtFilePath = Path.Combine(folderPath, fileName + ".trt");

                RunProcess(pythonExe, $"\"{scriptPath}\" --weights \"{inputModel}\" --include onnx --opset 12 --simplify");
                RunProcess(@"C:\Program Files\AIApp\Installer_driver\TensorRT-10.7.0.23.Windows.win10.cuda-12.6\bin\trtexec.exe",
                           $"--onnx=\"{onnxFilePath}\" --saveEngine=\"{trtFilePath}\" --fp16");

                MessageBox.Show("轉檔完成......");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
            }
        }

        private void RunProcess(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine(error);
                }
                else
                {
                    Console.WriteLine(output);
                }
            }
        }
    }
}
