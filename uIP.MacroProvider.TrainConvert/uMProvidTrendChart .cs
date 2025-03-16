using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;
using uIP.Lib;

namespace uIP.MacroProvider.TrainConvert
{
    /// <summary>
    /// 趨勢圖 Plugin – 封裝模型訓練與趨勢圖更新的核心邏輯，
    /// 供 uIP+Macro 呼叫，也供 FormConfTrainDraw 呼叫。
    /// </summary>
    public class uMProvidTrendChart : UMacroMethodProviderPlugin
    {
        private const string MethodName_OpenTrendChart = "TrendChart_OpenForm";
        private const string MethodName_StartTraining = "TrendChart_StartTraining";
        private const string MethodName_StopTraining = "TrendChart_StopTraining";

        private bool _isInitialized = false;

        public uMProvidTrendChart() : base()
        {
            m_strInternalGivenName = "TrendChartPlugin";
        }

        /// <summary>
        /// 初始化 Plugin：註冊 Macro 方法
        /// </summary>
        public override bool Initialize(UDataCarrier[] param)
        {
            if (m_bOpened) return true;

            // 註冊 Macro 方法：開啟趨勢圖 Form (若 Macro 需要呼叫)
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    MethodName_OpenTrendChart,
                    MacroCall_OpenTrendChart,
                    null, null, null, null
                )
            );

            // 註冊 Macro 方法：開始訓練
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    MethodName_StartTraining,
                    MacroCall_StartTraining,
                    null, null, null, null
                )
            );

            // 註冊 Macro 方法：停止訓練
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    MethodName_StopTraining,
                    MacroCall_StopTraining,
                    null, null, null, null
                )
            );

            m_bOpened = true;
            _isInitialized = true;
            return true;
        }

        #region 對外公開的核心方法

        /// <summary>
        /// 封裝核心訓練邏輯
        /// 從傳入的 FormConfTrainDraw 讀取 UI 參數、執行 Python 轉檔流程，並更新圖表。
        /// </summary>
        /// <param name="form">呼叫此方法的 FormConfTrainDraw 實例</param>
        public void StartTrainingByCode(FormConfTrainDraw form)
        {
            // 0) 重置圖表
            form.ResetGraph();
            // 1) 取得UI參數
            string model = form.SelectedModel;
            string dataset = form.SelectedDataSets;
            string weights = form.SelectedWeights;
            string batchSize = form.BatchSize;
            string epochs = form.Epochs;
            string hyps = form.Hyps;

            if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(dataset) ||
                string.IsNullOrEmpty(weights) || string.IsNullOrEmpty(batchSize) ||
                string.IsNullOrEmpty(epochs))
            {
                MessageBox.Show("請確認所有參數皆已輸入！");
                return;
            }

            // 2) 執行 pip install 安裝必要套件
            string requirementsFile = @"C:\Users\MIP4070\Desktop\yolov5-master\requirements.txt";
            ProcessStartInfo pyInstall = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = $"-m pip install -r \"{requirementsFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process installProcess = new Process() { StartInfo = pyInstall })
            {
                try
                {
                    installProcess.Start();
                    installProcess.BeginOutputReadLine();
                    installProcess.BeginErrorReadLine();
                    installProcess.WaitForExit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"無法執行 pip install: {ex.Message}");
                    return;
                }
            }

            // 3) 組合 Python 執行參數 (呼叫 train.py)
            string pythonFile = @"C:\Users\MIP4070\Desktop\yolov5-master\train.py";
            string args = $"\"{pythonFile}\" --data C:\\Users\\MIP4070\\Desktop\\yolov5-master\\data\\{dataset} " +
                          $"--cfg C:\\Users\\MIP4070\\Desktop\\yolov5-master\\models\\{model} " +
                          $"--weights C:\\Users\\MIP4070\\Desktop\\yolov5-master\\weights\\{weights} " +
                          $"--hyp C:\\Users\\MIP4070\\Desktop\\yolov5-master\\data\\hyps\\{hyps} " +
                          $"--batch-size {batchSize} --epochs {epochs} --device cuda:0";

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            form.trainingProcess = new Process { StartInfo = psi };
            form.trainingProcess.OutputDataReceived += form.InvokeOutputDataReceived;
            form.trainingProcess.ErrorDataReceived += form.InvokeErrorDataReceived;
            form.trainingProcess.EnableRaisingEvents = true;

            try
            {
                form.trainingProcess.Start();
                form.trainingProcess.BeginOutputReadLine();
                form.trainingProcess.BeginErrorReadLine();

                // 非同步等待，避免 UI 凍結
                Task.Run(() =>
                {
                    form.trainingProcess.WaitForExit();
                    form.Invoke(new Action(() =>
                    {
                        MessageBox.Show("訓練程序已完成！", "完成提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法啟動程序: {ex.Message}");
            }
        }

        /// <summary>
        /// 封裝停止訓練的邏輯
        /// </summary>
        public void StopTrainingByCode(FormConfTrainDraw form)
        {
            if (form.trainingProcess == null)
            {
                MessageBox.Show("沒有正在執行的訓練程序。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (form.trainingProcess.HasExited)
            {
                MessageBox.Show("訓練程序已經結束。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                form.trainingProcess = null;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    form.trainingProcess.Kill();
                    form.trainingProcess.WaitForExit();
                    form.Invoke(new Action(() =>
                    {
                        MessageBox.Show("訓練已中止！", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                }
                catch (Exception ex)
                {
                    form.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"無法終止 Python 訓練程序: {ex.Message}");
                    }));
                }
                finally
                {
                    form.trainingProcess = null;
                }
            });
        }

        #endregion

        #region Macro 方法 (供 Macro 呼叫)

        private UDataCarrier[] MacroCall_OpenTrendChart(
            UMacro MacroInstance,
            UDataCarrier[] PrevPropagationCarrier,
            List<UMacroProduceCarrierResult> historyResultCarriers,
            List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
            List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
            List<UScriptHistoryCarrier> historyCarrier,
            ref bool bStatusCode,
            ref string strStatusMessage,
            ref UDataCarrier[] CurrPropagationCarrier,
            ref UDrawingCarriers CurrDrawingCarriers,
            ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
            ref fpUDataCarrierSetResHandler ResultCarrierHandler)
        {
            try
            {
                if (FormConfTrainDraw.StaticInstance == null || FormConfTrainDraw.StaticInstance.IsDisposed)
                {
                    // 若有需求可建立單例
                    FormConfTrainDraw.StaticInstance = new FormConfTrainDraw();
                }
                FormConfTrainDraw.StaticInstance.Show();
                bStatusCode = true;
                strStatusMessage = "TrendChart form opened via Macro.";
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = "Failed to open TrendChart form: " + ex.Message;
            }
            return null;
        }

        private UDataCarrier[] MacroCall_StartTraining(
            UMacro MacroInstance,
            UDataCarrier[] PrevPropagationCarrier,
            List<UMacroProduceCarrierResult> historyResultCarriers,
            List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
            List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
            List<UScriptHistoryCarrier> historyCarrier,
            ref bool bStatusCode,
            ref string strStatusMessage,
            ref UDataCarrier[] CurrPropagationCarrier,
            ref UDrawingCarriers CurrDrawingCarriers,
            ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
            ref fpUDataCarrierSetResHandler ResultCarrierHandler)
        {
            try
            {
                if (FormConfTrainDraw.StaticInstance == null || FormConfTrainDraw.StaticInstance.IsDisposed)
                {
                    strStatusMessage = "TrendChart form is not opened.";
                    bStatusCode = false;
                    return null;
                }
                // 呼叫 Plugin 的核心方法，從單例 Form 中執行訓練
                FormConfTrainDraw.StaticInstance.Invoke(new Action(() =>
                {
                    StartTrainingByCode(FormConfTrainDraw.StaticInstance);
                }));
                bStatusCode = true;
                strStatusMessage = "Training started via Macro.";
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = "Failed to start training: " + ex.Message;
            }
            return null;
        }

        private UDataCarrier[] MacroCall_StopTraining(
            UMacro MacroInstance,
            UDataCarrier[] PrevPropagationCarrier,
            List<UMacroProduceCarrierResult> historyResultCarriers,
            List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
            List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
            List<UScriptHistoryCarrier> historyCarrier,
            ref bool bStatusCode,
            ref string strStatusMessage,
            ref UDataCarrier[] CurrPropagationCarrier,
            ref UDrawingCarriers CurrDrawingCarriers,
            ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
            ref fpUDataCarrierSetResHandler ResultCarrierHandler)
        {
            try
            {
                if (FormConfTrainDraw.StaticInstance == null || FormConfTrainDraw.StaticInstance.IsDisposed)
                {
                    strStatusMessage = "TrendChart form is not opened.";
                    bStatusCode = false;
                    return null;
                }
                FormConfTrainDraw.StaticInstance.Invoke(new Action(() =>
                {
                    // 直接呼叫 Plugin 的停止邏輯
                    StopTrainingByCode(FormConfTrainDraw.StaticInstance);
                }));
                bStatusCode = true;
                strStatusMessage = "Training stopped via Macro.";
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = "Failed to stop training: " + ex.Message;
            }
            return null;
        }

        #endregion
    }
}
