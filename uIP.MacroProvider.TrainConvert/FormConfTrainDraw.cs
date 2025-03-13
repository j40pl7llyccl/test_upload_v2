using System;
using System.Diagnostics; // ← 用於呼叫 Python & 讀取輸出
using System.Windows.Forms;
using System.Threading.Tasks;

// 使用 Newtonsoft.Json 解析 Python 輸出的 JSON (記得先安裝 Newtonsoft.Json 套件)
using Newtonsoft.Json.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using uIP.MacroProvider.TrainingConvert;

namespace uIP.MacroProvider.TrainConvert
{
    public partial class FormConfTrainDraw : Form
    {
        private Process trainingProcess; // 用於保存 Python 訓練的 Process 物件

        /// <summary>
        /// 建構子：呼叫 InitializeComponent()，但不包含控制項的手動初始化
        /// 都假設寫在 .Designer.cs 檔裡面
        /// </summary>
        public FormConfTrainDraw()
        {
            InitializeComponent();
            //this.Load += new EventHandler(FormConfTrainDraw_Load); // 註冊 Load 事件
        }

        /// <summary>
        /// 在表單加載時初始化圖表網格
        /// </summary>
        private void FormConfTrainDraw_Load(object sender, EventArgs e)
        {
            InitializeCharts();
        }
        private void InitializeCharts()
        {
            Chart[] charts = { chart_train_box_loss, chart_train_obj_loss, chart_train_cls_loss, chart_val_box_loss, chart_val_obj_loss, chart_val_cls_loss };
            foreach (var chart in charts)
            {
                if (chart.ChartAreas.Count > 0)
                {
                    ChartArea area = chart.ChartAreas[0];

                    // 設定 X 軸的網格線
                    area.AxisX.MajorGrid.Enabled = true;
                    area.AxisX.MajorGrid.LineColor = Color.LightGray;
                    area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                    // 設定 Y 軸的網格線
                    area.AxisY.MajorGrid.Enabled = true;
                    area.AxisY.MajorGrid.LineColor = Color.LightGray;
                    area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                    // 設定 X 軸 & Y 軸範圍，避免圖表完全空白
                    area.AxisX.Minimum = 0;
                    area.AxisX.Maximum = 10;
                    area.AxisX.Interval = 1;

                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = 1;
                    area.AxisY.Interval = 0.1;
                }
            }
        }
        /// <summary>
        /// [Run] 按鈕事件：啟動 Python 並開始讀取訓練過程
        /// </summary>
        private void bt_Run_Click(object sender, EventArgs e)
        {
            // 0) 每次執行前先重置圖表
            ResetGraph();

            // 另外，為避免重複執行，禁用 Run 按鈕
            //bt_Run.Enabled = false;

            // 1) 取得使用者在 WinForm UI 中的參數
            string model = cB_Model.Text;
            string dataset = cB_DataSets.Text;
            string weights = cB_Weights.Text;
            string batchSize = tB_BatchSize.Text;
            string epochs = tB_Epochs.Text;
            string hyps = cB_Hyps.Text;
            Console.WriteLine("model: " + model);
            Console.WriteLine("dataset: " + dataset);
            Console.WriteLine("weights: " + weights);
            Console.WriteLine("batchSize: " + batchSize);
            Console.WriteLine("epochs: " + epochs);
            Console.WriteLine("hyps: " + hyps);
            if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(dataset) || string.IsNullOrEmpty(weights) || string.IsNullOrEmpty(batchSize) || string.IsNullOrEmpty(epochs) )
            {
                MessageBox.Show("請確認所有參數皆已輸入！");
                return;
            }

            // 2)確認是否有安裝相關套件

            string requirementsFile = @"C:\Users\MIP4070\Desktop\yolov5-master\requirements.txt"; //C: \Users\MIP4070\Desktop\yolov5 - master\requirements.txt

            // 組成指令： python -m pip install -r "requirements.txt"
            ProcessStartInfo py_insatll = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = $"-m pip install -r \"{requirementsFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process installProcess = new Process() { StartInfo = py_insatll };

            installProcess.OutputDataReceived += (s, pipArgs) =>
            {
                if (!string.IsNullOrEmpty(pipArgs.Data))
                    Console.WriteLine("[PIP 輸出]: " + pipArgs.Data);
            };

            installProcess.ErrorDataReceived += (s, pipErrArgs) =>
            {
                if (!string.IsNullOrEmpty(pipErrArgs.Data))
                    Console.WriteLine("[PIP 錯誤]: " + pipErrArgs.Data);
            };

            try
            {
                installProcess.Start();
                installProcess.BeginOutputReadLine();
                installProcess.BeginErrorReadLine();
                installProcess.WaitForExit(); // 等待完成
                //MessageBox.Show("套件安裝完成!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法執行 pip install: {ex.Message}");
            }
            /*------------------------------------------------------------------------------------------------------------------------*/
            // 3) 組合成 Python 執行參數 (train.py + 相關參數)
            string pythonFile = @"C:\Users\MIP4070\Desktop\yolov5-master\train.py";
            string args = $"\"{pythonFile}\" --data C:\\Users\\MIP4070\\Desktop\\yolov5-master\\data\\{dataset} " +
                          $"--cfg  C:\\Users\\MIP4070\\Desktop\\yolov5-master\\models\\{model} " +
                          $"--weights  C:\\Users\\MIP4070\\Desktop\\yolov5-master\\weights\\{weights} " +
                          $"--hyp  C:\\Users\\MIP4070\\Desktop\\yolov5-master\\data\\hyps\\{hyps} " +
                          $"--batch-size {batchSize} " +
                          $"--epochs {epochs} " +
                          $"--device cuda:0";

            Console.WriteLine("[python 執行緒]: "+"python " + args);
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process trainingProcess = new Process { StartInfo = psi };
            trainingProcess.OutputDataReceived += Process_OutputDataReceived; 

            trainingProcess.ErrorDataReceived += Process_ErrorDataReceived;  

            /*--trainingProcess.Exited += (procSender, evtArgs) =>
            {
                MessageBox.Show(" 訓練已執行完畢！", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };--*/

            trainingProcess.EnableRaisingEvents = true;

            try
            {
                trainingProcess.Start();
                trainingProcess.BeginOutputReadLine();
                trainingProcess.BeginErrorReadLine();

                // 使用非同步等待，避免UI凍結
                Task.Run(() =>
                {
                    trainingProcess.WaitForExit();
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show(" 訓練程序已完成！", "完成提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法啟動 程序: {ex.Message}");
            }
        }
        /// <summary>
        /// 即時讀取 Python stdout，每有一行輸出便會觸發
        /// </summary>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            string line = e.Data.Trim();
            Debug.WriteLine($"e.Data Type: {e.Data.GetType().Name}, Value: {e.Data}");

            // 1**如果是 JSON 格式才解析**
            if (line.StartsWith("{") && line.EndsWith("}"))
            {
                try
                {
                    JObject dataObj = JObject.Parse(line);
                    int epoch = dataObj["epoch"]?.Value<int>() ?? -1;
                    double train_box_loss = dataObj["train_box_loss"]?.Value<double>() ?? 0;
                    double train_obj_loss = dataObj["train_obj_loss"]?.Value<double>() ?? 0;
                    double train_cls_loss = dataObj["train_cls_loss"]?.Value<double>() ?? 0;
                    double val_box_loss = dataObj["val_box_loss"]?.Value<double>() ?? 0;
                    double val_obj_loss = dataObj["val_obj_loss"]?.Value<double>() ?? 0;
                    double val_cls_loss = dataObj["val_cls_loss"]?.Value<double>() ?? 0;

                    this.Invoke((MethodInvoker)delegate
                    {
                        textBoxOutput.AppendText($"[Parsed Json] {line}" + Environment.NewLine);
                        UpdateChart(epoch, train_box_loss, train_obj_loss, train_cls_loss, val_box_loss, val_obj_loss, val_cls_loss);
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON 解析失敗: {line}, 錯誤: {ex.Message}");
                }
            }
            // 2️ **解析非 JSON 的數據行**
            else if (line.Contains("train/box_loss") || line.Contains("val/box_loss"))
            {
                try
                {
                    // **手動解析 train/box_loss, val/box_loss 數值**
                    var parts = line.Split(':'); // 分割 ":" 前後
                    if (parts.Length == 2)
                    {
                        var values = parts[1].Trim().Split(' '); // 取出數值
                        if (values.Length >= 3)
                        {
                            int epoch = int.Parse(values[0]); 
                            double train_box_loss = double.Parse(values[1]);
                            double train_obj_loss = double.Parse(values[2]);
                            double train_cls_loss = double.Parse(values[3]);
                            double val_box_loss = double.Parse(values[4]);
                            double val_obj_loss = double.Parse(values[5]);
                            double val_cls_loss = double.Parse(values[6]);

                            this.Invoke((MethodInvoker)delegate
                            {
                                textBoxOutput.AppendText($"[Parsed] {line}" + Environment.NewLine);
                                UpdateChart(epoch, train_box_loss, train_obj_loss, train_cls_loss, val_box_loss, val_obj_loss, val_cls_loss);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"數據解析失敗: {line}, 錯誤: {ex.Message}");
                }
            }
            else
            {
                // **一般日誌輸出**
                this.Invoke((MethodInvoker)delegate
                {
                    textBoxOutput.AppendText("[LOG] " + line + Environment.NewLine);
                });
            }
        }


        /// <summary>
        /// 讀取 Python stderr（錯誤輸出）時觸發，可印出 Log 以供除錯
        /// </summary>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                
                this.Invoke(new Action(() =>
                {
                    textBoxOutput.AppendText(e.Data + Environment.NewLine);
                }));

                Debug.WriteLine("[Parsed] " + e.Data);
            }
        }

        /// <summary>
        /// 根據 Python 輸出的結果，更新 6 張 Chart 的折線
        /// </summary>
        private void UpdateChart(int epoch,
                                 double train_box_loss, double train_obj_loss, double train_cls_loss,
                                 double val_box_loss, double val_obj_loss, double val_cls_loss)
        {
            try
            {
                // 確保所有需要的 Series 已存在
                EnsureSeriesExists(chart_train_box_loss, "train/box_loss", Color.Red);
                EnsureSeriesExists(chart_train_obj_loss, "train/obj_loss", Color.Green);
                EnsureSeriesExists(chart_train_cls_loss, "train/cls_loss", Color.Blue);
                EnsureSeriesExists(chart_val_box_loss, "val/box_loss", Color.Orange);
                EnsureSeriesExists(chart_val_obj_loss, "val/obj_loss", Color.Purple);
                EnsureSeriesExists(chart_val_cls_loss, "val/cls_loss", Color.Cyan);

                // 將資料加入對應 Series
                chart_train_box_loss.Series["train/box_loss"].Points.AddXY(epoch, train_box_loss);
                ChartArea area_train_box_loss = chart_train_box_loss.ChartAreas[0];
                area_train_box_loss.AxisX.Title = "Epochs";
                area_train_box_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold); 
                area_train_box_loss.AxisX.TitleForeColor = Color.Black; 

                chart_train_obj_loss.Series["train/obj_loss"].Points.AddXY(epoch, train_obj_loss);
                ChartArea area_train_obj_loss = chart_train_obj_loss.ChartAreas[0];
                area_train_obj_loss.AxisX.Title = "Epochs";
                area_train_obj_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_train_obj_loss.AxisX.TitleForeColor = Color.Black;

                chart_train_cls_loss.Series["train/cls_loss"].Points.AddXY(epoch, train_cls_loss);
                ChartArea area_train_cls_loss = chart_train_cls_loss.ChartAreas[0];
                area_train_cls_loss.AxisX.Title = "Epochs";
                area_train_cls_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_train_cls_loss.AxisX.TitleForeColor = Color.Black;

                chart_val_box_loss.Series["val/box_loss"].Points.AddXY(epoch, val_box_loss);
                ChartArea area_val_box_loss = chart_val_box_loss.ChartAreas[0];
                area_val_box_loss.AxisX.Title = "Epochs";
                area_val_box_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_val_box_loss.AxisX.TitleForeColor = Color.Black;

                chart_val_obj_loss.Series["val/obj_loss"].Points.AddXY(epoch, val_obj_loss);
                ChartArea area_val_obj_loss = chart_val_obj_loss.ChartAreas[0];
                area_val_obj_loss.AxisX.Title = "Epochs";
                area_val_obj_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_val_obj_loss.AxisX.TitleForeColor = Color.Black;

                chart_val_cls_loss.Series["val/cls_loss"].Points.AddXY(epoch, val_cls_loss);
                ChartArea area_val_cls_loss = chart_val_cls_loss.ChartAreas[0];
                area_val_cls_loss.AxisX.Title = "Epochs";
                area_val_cls_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_val_cls_loss.AxisX.TitleForeColor = Color.Black;

                // 更新圖表
                chart_train_box_loss.Update();
                chart_train_obj_loss.Update();
                chart_train_cls_loss.Update();
                chart_val_box_loss.Update();
                chart_val_obj_loss.Update();
                chart_val_cls_loss.Update();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("更新圖表時發生錯誤: " + ex.Message);
            }
        }
		
		private void EnsureSeriesExists(System.Windows.Forms.DataVisualization.Charting.Chart chart, string seriesName, Color lineColor)
		{
            
            /*--
            if (chart.Series.IndexOf(seriesName) >= 0)
                return;

            // 建立一個新的 Series
            var series = new System.Windows.Forms.DataVisualization.Charting.Series(seriesName);
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line; // 可依需求修改為其他圖表類型
            chart.Series.Add(series);
            --*/
            // 嘗試取得已存在的 Series
            var existingSeries = chart.Series.FindByName(seriesName);

            if (existingSeries == null)
            {
                // 若找不到，則建立新的 Series
                existingSeries = new System.Windows.Forms.DataVisualization.Charting.Series(seriesName);
                chart.Series.Add(existingSeries);
            }

            // 設定 Series 類型為折線圖 
            existingSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            // 設定折線顏色
            existingSeries.Color = lineColor;

            // 設定線條寬度
            existingSeries.BorderWidth = 2;

            if (chart.Legends.Count > 0)
            {
                chart.Legends[0].Font = new Font("Arial", 12, FontStyle.Bold); // 設定字體大小
                //chart.Legends[0].ForeColor = Color.Black; // 設定字體顏色
            }
        }
        // 以下是控制項的事件，若無特殊處理需求，可保持空白
        private void cB_Model_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cB_DataSets_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cB_Weights_SelectedIndexChanged(object sender, EventArgs e) { }
        private void tB_BatchSize_TextChanged(object sender, EventArgs e) { }
        private void tB_Epochs_TextChanged(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e){ }

        private void bt_Stop_Click(object sender, EventArgs e)
        {   
            //結束時,需要把畫布給清空
            if (trainingProcess == null)
            {
                MessageBox.Show("沒有正在執行的訓練程序。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (trainingProcess.HasExited)
            {
                MessageBox.Show("訓練程序已經結束。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                trainingProcess = null; // 清除參考
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    trainingProcess.Kill();
                    trainingProcess.WaitForExit();

                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("訓練已中止！", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"無法終止 Python 訓練程序: {ex.Message}");
                    }));
                }
                finally
                {
                    trainingProcess = null; // 確保結束後釋放資源
                }
            });
        }

        private void ResetGraph()
        {
            // 清除圖表上的所有資料點
            chart_train_box_loss.Series["train/box_loss"].Points.Clear();
            chart_train_obj_loss.Series["train/obj_loss"].Points.Clear();
            chart_train_cls_loss.Series["train/cls_loss"].Points.Clear();
            chart_val_box_loss.Series["val/box_loss"].Points.Clear();
            chart_val_obj_loss.Series["val/obj_loss"].Points.Clear();
            chart_val_cls_loss.Series["val/cls_loss"].Points.Clear();
        }


        private void bt_convert_Click(object sender, EventArgs e)
        {
            // 建立另一個視窗實例
            modelConvert mConvert = new modelConvert();

            // 顯示新的視窗
            mConvert.Show();
        }
    }
}
