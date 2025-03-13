using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;
using uIP.MacroProvider.TrainConvert;

namespace uIP.MacroProvider.TrainingConvert
{
    public class YoloTrainingPlugin : UMacroMethodProviderPlugin
    {
        private const string TrainYoloMethodName = "TrainYolo_Method";

        // 供 GET/SET 的幾個私有欄位
        private string _pythonPath = "python";
        private string _trainPyFile = "";
        private string _datasetYaml = "";
        private string _modelCfg = "";
        private string _weights = "";
        private int _batchSize = 16;
        private int _epochs = 10;
        private string _device = "cuda:0";

        public YoloTrainingPlugin()
        {
            m_strInternalGivenName = "YoloTrainingPlugin";
        }

        public override bool Initialize(UDataCarrier[] param)
        {
            // 1) 建立Macro
            var macro = new UMacro(
                owner: null,
                m_strInternalGivenName,
                TrainYoloMethodName,
                RunTraining,
                null,
                null,
                null,
                new UDataCarrierTypeDescription[]
                {
                    new UDataCarrierTypeDescription(typeof(string), "Train Output Result")
                }
            );
            m_UserQueryOpenedMethods.Add(macro);

            // 2) 執行完畢的收尾
            m_createMacroDoneFromMethod.Add(TrainYoloMethodName, MacroShellDoneCall);

            // 3) 彈窗 UI
            m_macroMethodConfigPopup.Add(TrainYoloMethodName, PopupConfTrain);

            //-------------------------------------------------------------------------------------------
            // 增添 GET/SET (m_MacroControls) 等方法，讓表單與 Plugin 互相傳遞參數
            //-------------------------------------------------------------------------------------------
            // 這裡示範將所有訓練參數整合成 JSON 字串存取 (也可拆成多個 ControlCarrierMacro)
            m_MacroControls.Add(
                "TrainYoloSetting",
                new UScriptControlCarrierMacro(
                    "TrainYoloSetting",
                    true,
                    true,
                    true,
                    // 說明這個參數的型態是 string (我們打算用 JSON 字串)
                    new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Training config in JSON") },
                    // GET callback
                    new fpGetMacroScriptControlCarrier((UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus) =>
                        IoctrlGet_TrainCfg(whichMacro.MethodName, whichMacro, ref bRetStatus)),
                    // SET callback
                    new fpSetMacroScriptControlCarrier((UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data) =>
                        IoctrlSet_TrainCfg(whichMacro.MethodName, whichMacro, data))
                )
            );

            m_bOpened = true;
            return true;
        }

        private bool MacroShellDoneCall(string callMethodName, UMacro instance)
        {
            // 執行完後的收尾
            return true;
        }

        private Form PopupConfTrain(string callMethodName, UMacro macroToConf)
        {
            if (callMethodName == TrainYoloMethodName)
            {
                var form = new FormConfTrainDraw();
                // 你可以在這裡把 plugin 或 macroToConf 傳給表單
                // form.SetPluginReference(this) 或
                // form.MacroInstance = macroToConf;
                return form;
            }
            return null;
        }

        /// <summary>
        /// 核心：真正執行 Training 的 callback
        /// </summary>
        private UDataCarrier[] RunTraining(
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
            // 執行 Python 訓練
            var psi = new ProcessStartInfo()
            {
                FileName = _pythonPath,
                Arguments = $"{_trainPyFile} --data {_datasetYaml} --cfg {_modelCfg} --weights {_weights} " +
                            $"--batch-size {_batchSize} --epochs {_epochs} --device {_device}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = new Process();
            proc.StartInfo = psi;
            proc.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    try
                    {
                        var obj = JObject.Parse(e.Data);
                        int epoch = obj["epoch"]?.Value<int>() ?? -1;
                        double tbox = obj["train_box_loss"]?.Value<double>() ?? 0;
                        double vbox = obj["val_box_loss"]?.Value<double>() ?? 0;

                        Console.WriteLine($"[Training] epoch={epoch}, tbox={tbox}, vbox={vbox}");
                    }
                    catch { /* 不是JSON就忽略 */ }
                }
            };
            proc.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine("[stderr] " + e.Data);
                }
            };

            try
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                bStatusCode = true;
                strStatusMessage = "Training Completed!";
                CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray("Train Done!");
                return CurrPropagationCarrier;
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = "Training Failed: " + ex.Message;
                return null;
            }
        }

        //-------------------------------------------------------------------------------------------
        // GET/SET 具體實作
        //-------------------------------------------------------------------------------------------
        // 這裡示範將所有參數存成 JSON，或可自行拆成多個 ControlCarrier
        // 例： { \"pythonPath\": \"python\", \"trainPyFile\": \"train.py\", \"batchSize\": 16, ... }
        //-------------------------------------------------------------------------------------------
        private UDataCarrier[] IoctrlGet_TrainCfg(string methodName, UMacro instance, ref bool bRetStatus)
        {
            // 將當前 Plugin 的私有欄位組成 JSON
            var obj = new JObject
            {
                ["pythonPath"] = _pythonPath,
                ["trainPyFile"] = _trainPyFile,
                ["datasetYaml"] = _datasetYaml,
                ["modelCfg"] = _modelCfg,
                ["weights"] = _weights,
                ["batchSize"] = _batchSize,
                ["epochs"] = _epochs,
                ["device"] = _device
            };
            string jsonStr = obj.ToString(); // 序列化

            bRetStatus = true;
            return new UDataCarrier[] { new UDataCarrier(jsonStr, typeof(string)) };
        }

        private bool IoctrlSet_TrainCfg(string methodName, UMacro instance, UDataCarrier[] data)
        {
            // data[0] 裡面是一個 JSON 字串
            if (data == null || data.Length == 0) return false;
            string jsonStr = data[0].Data as string;
            if (string.IsNullOrEmpty(jsonStr)) return false;

            try
            {
                var obj = JObject.Parse(jsonStr);
                _pythonPath = obj["pythonPath"]?.Value<string>() ?? "python";
                _trainPyFile = obj["trainPyFile"]?.Value<string>() ?? "";
                _datasetYaml = obj["datasetYaml"]?.Value<string>() ?? "";
                _modelCfg = obj["modelCfg"]?.Value<string>() ?? "";
                _weights = obj["weights"]?.Value<string>() ?? "";
                _batchSize = obj["batchSize"]?.Value<int>() ?? 16;
                _epochs = obj["epochs"]?.Value<int>() ?? 10;
                _device = obj["device"]?.Value<string>() ?? "cpu";

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Close()
        {
            base.Close();
        }
    }
}



