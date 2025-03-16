using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;
using uIP.Lib;

namespace uIP.MacroProvider.TrainConvert
{
    /// <summary>
    /// 「模型轉檔」 Plugin：將核心轉檔流程封裝在這裡，
    /// 供 uIP+Macro 或 UI Form 調用。
    /// </summary>
    public class uMProvidModelConvert : UMacroMethodProviderPlugin
    {
        // Macro 方法名稱
        private const string MethodName_OpenModelForm = "ModelConvert_OpenForm";
        private const string MethodName_StartConvert = "ModelConvert_StartConvert";

        private bool _isInitialized = false;

        public uMProvidModelConvert() : base()
        {
            m_strInternalGivenName = "ModelConvertPlugin";
        }

        /// <summary>
        /// 初始化 Plugin：註冊 Macro 方法（例如：開啟轉檔視窗、開始轉檔）
        /// </summary>
        /// <param name="param">外部傳入的參數，如無則為 null</param>
        /// <returns>是否初始化成功</returns>
        public override bool Initialize(UDataCarrier[] param)
        {
            if (m_bOpened) return true;

            // 註冊 Macro 方法：打開模型轉檔 Form (若需要)
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    MethodName_OpenModelForm,
                    MacroCall_OpenModelForm,
                    null,
                    null,
                    null,
                    null
                )
            );

            // 註冊 Macro 方法：開始轉檔
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    MethodName_StartConvert,
                    MacroCall_StartConvert,
                    null,
                    null,
                    null,
                    null
                )
            );

            // 可依需求在此註冊 Macro 變數或 Popup UI 設定

            m_bOpened = true;
            _isInitialized = true;
            return true;
        }

        #region 對外公開的核心轉檔邏輯

        /// <summary>
        /// 直接執行模型轉檔的核心流程
        /// </summary>
        /// <param name="modelPath">使用者指定的輸入模型路徑</param>
        /// <param name="errMsg">若有錯誤回傳錯誤訊息</param>
        /// <returns>轉檔是否成功</returns>
        public bool StartConvertByCode(string modelPath, out string errMsg)
        {
            errMsg = null;
            if (!_isInitialized)
            {
                errMsg = "Plugin not initialized.";
                return false;
            }
            try
            {
                if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
                {
                    errMsg = "請選擇有效的輸入模型路徑!";
                    return false;
                }

                // 定義相關路徑 (根據實際需求調整)
                string pythonExe = @"python";
                string scriptPath = @"C:\Users\MIP4070\Desktop\yolov5-master\export.py";
                string trtExe = @"C:\Program Files\AIApp\Installer_driver\TensorRT-10.7.0.23.Windows.win10.cuda-12.6\bin\trtexec.exe";

                string folderPath = Path.GetDirectoryName(modelPath);
                string fileName = Path.GetFileNameWithoutExtension(modelPath);
                string onnxFile = Path.Combine(folderPath, fileName + ".onnx");
                string trtFile = Path.Combine(folderPath, fileName + ".trt");

                // 呼叫 Python 執行 export.py
                RunProcess(pythonExe, $"\"{scriptPath}\" --weights \"{modelPath}\" --include onnx --opset 12 --simplify");
                // 呼叫 TensorRT 執行轉檔
                RunProcess(trtExe, $"--onnx=\"{onnxFile}\" --saveEngine=\"{trtFile}\" --fp16");

                return true;
            }
            catch (Exception ex)
            {
                errMsg = $"發生錯誤: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// 執行外部程式（例如 python, trtexec）
        /// </summary>
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
                    Console.WriteLine($"[RunProcess Error] {error}");
                }
                else
                {
                    Console.WriteLine($"[RunProcess Output] {output}");
                }
            }
        }

        #endregion

        #region Macro 方法（供 Macro 調用）

        /// <summary>
        /// Macro 方法：打開模型轉檔 Form（若需要讓 Macro 直接彈出 UI）
        /// </summary>
        private UDataCarrier[] MacroCall_OpenModelForm(
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
            // 此範例不彈出 UI，僅回傳訊息
            bStatusCode = true;
            strStatusMessage = "Open ModelConvert Form not implemented via Macro.";
            return null;
        }

        /// <summary>
        /// Macro 方法：開始模型轉檔（直接透過 Macro 執行轉檔邏輯）
        /// </summary>
        private UDataCarrier[] MacroCall_StartConvert(
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
            // 例如從 PrevPropagationCarrier 取得模型路徑（此處僅示範，實際可依需求調整）
            string modelPath = @"C:\Users\MIP4070\Desktop\yolov5-master\weights\some_model.pt";
            bool ok = StartConvertByCode(modelPath, out string errMsg);
            if (ok)
            {
                bStatusCode = true;
                strStatusMessage = "Model conversion completed via Macro.";
            }
            else
            {
                bStatusCode = false;
                strStatusMessage = "Model conversion failed: " + errMsg;
            }
            return null;
        }

        #endregion
    }
}
