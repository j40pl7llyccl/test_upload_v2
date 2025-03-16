using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    /// <summary>
    /// FileDistributor Plugin 封裝了檔案隨機分配的核心邏輯，
    /// 並可由 Macro 或 WinForm 直接呼叫。使用者可指定「資料夾路徑」
    /// 以及 Train/Test/Val 三種比例，搬移後同時在各子資料夾產生列出圖檔路徑的 txt 檔。
    /// </summary>
    public class FileDistributor : UMacroMethodProviderPlugin
    {
        private const string FileDistributorMethodName = "DatasetDev_FileDistributor";
        // 參數索引: Folder=0, Train=1, Test=2, Val=3
        private const int INDEX_FOLDER = 0;
        private const int INDEX_TRAIN = 1;
        private const int INDEX_TEST = 2;
        private const int INDEX_VAL = 3;

        public FileDistributor() : base()
        {
            m_strInternalGivenName = "FileDistributor";
        }

        public override bool Initialize(UDataCarrier[] param)
        {
            if (m_bOpened) return true;

            // 1) 註冊 Macro：讓 uIP 知道本 Plugin 可透過此名稱呼叫
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    FileDistributorMethodName,
                    Macro_DistributeFiles,
                    null, null, null,
                    new UDataCarrierTypeDescription[]{
                        new UDataCarrierTypeDescription(typeof(int), "MovedCount")
                    }
                )
            );
            // 註冊 Macro 後的回呼
            m_createMacroDoneFromMethod.Add(FileDistributorMethodName, MacroShellDoneCall_DistributeFiles);
            // 註冊 Popup 視窗（用於 GUI 設定）
            m_macroMethodConfigPopup.Add(FileDistributorMethodName, PopupConf_FileDistributor);

            // 2) 註冊 GET/SET 參數：FolderPath, TrainRatio, TestRatio, ValRatio
            m_MacroControls.Add("FolderPath", new UScriptControlCarrierMacro(
                "FolderPath", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Source Folder") },
                IoctrlGet_FolderPath, IoctrlSet_FolderPath
            ));
            m_MacroControls.Add("TrainRatio", new UScriptControlCarrierMacro(
                "TrainRatio", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(double), "Train Ratio") },
                IoctrlGet_TrainRatio, IoctrlSet_TrainRatio
            ));
            m_MacroControls.Add("TestRatio", new UScriptControlCarrierMacro(
                "TestRatio", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(double), "Test Ratio") },
                IoctrlGet_TestRatio, IoctrlSet_TestRatio
            ));
            m_MacroControls.Add("ValRatio", new UScriptControlCarrierMacro(
                "ValRatio", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(double), "Val Ratio") },
                IoctrlGet_ValRatio, IoctrlSet_ValRatio
            ));
            Console.WriteLine($"FileDistributor: Initialize called, Total Methods: {m_UserQueryOpenedMethods?.Count ?? 0}");

            m_bOpened = true;
            return true;
        }

        // Macro 執行邏輯：由 Macro 呼叫時觸發
        private UDataCarrier[] Macro_DistributeFiles(
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
            ref fpUDataCarrierSetResHandler ResultCarrierHandler
        )
        {
            try
            {
                if (!UDataCarrier.Get<UDataCarrier[]>(MacroInstance.MutableInitialData, null, out var arr))
                {
                    bStatusCode = false;
                    strStatusMessage = "init data invalid";
                    return null;
                }

                // 讀取參數：FolderPath, TrainRatio, TestRatio, ValRatio
                string folderPath = UDataCarrier.GetItem(arr, INDEX_FOLDER, "", out _);
                double trainRatio = UDataCarrier.GetItem(arr, INDEX_TRAIN, 0.8, out _);
                double testRatio = UDataCarrier.GetItem(arr, INDEX_TEST, 0.1, out _);
                double valRatio = UDataCarrier.GetItem(arr, INDEX_VAL, 0.1, out _);

                if (Math.Abs((trainRatio + testRatio + valRatio) - 1.0) > 0.0001)
                {
                    bStatusCode = false;
                    strStatusMessage = $"Train/Test/Val 必須總和=1.0 (當前={trainRatio + testRatio + valRatio}).";
                    return null;
                }

                // 執行檔案分配動作
                int movedCount = DistributeFiles(folderPath, trainRatio, testRatio, valRatio);

                bStatusCode = true;
                strStatusMessage = $"FileDistributor done, total moved: {movedCount}";
                CurrPropagationCarrier = UDataCarrier.MakeOneItemArray(movedCount);
                return null;
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = $"Exception in Macro_DistributeFiles: {ex}";
                return null;
            }
        }

        /// <summary>
        /// 實際進行檔案隨機分配（Train/Test/Val），並搬移至各子資料夾。
        /// </summary>
        private int DistributeFiles(string folderPath, double trainRatio, double testRatio, double valRatio)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return 0;

            // 讀取所有檔案並隨機排序
            string[] files = Directory.GetFiles(folderPath);
            Random random = new Random();
            files = files.OrderBy(f => random.Next()).ToArray();

            int trainCount = (int)(files.Length * trainRatio);
            int testCount = (int)(files.Length * testRatio);
            int valCount = files.Length - trainCount - testCount;

            // 建立 train、test、val 子資料夾
            string trainPath = Path.Combine(folderPath, "train");
            string testPath = Path.Combine(folderPath, "test");
            string valPath = Path.Combine(folderPath, "val");
            Directory.CreateDirectory(trainPath);
            Directory.CreateDirectory(testPath);
            Directory.CreateDirectory(valPath);

            MoveFiles(files.Take(trainCount).ToArray(), trainPath);
            MoveFiles(files.Skip(trainCount).Take(testCount).ToArray(), testPath);
            MoveFiles(files.Skip(trainCount + testCount).ToArray(), valPath);

            return files.Length;
        }

        private void MoveFiles(string[] files, string destination)
        {
            foreach (var file in files)
            {
                string destFile = Path.Combine(destination, Path.GetFileName(file));
                File.Move(file, destFile);
            }
        }

        private bool MacroShellDoneCall_DistributeFiles(string callMethodName, UMacro instance)
        {
            return true;
        }

        private Form PopupConf_FileDistributor(string callMethodName, UMacro macroToConf)
        {
            Console.WriteLine($"PopupConf_FileDistributor called for: {callMethodName}");
            if (macroToConf == null)
            {
                Console.WriteLine("PopupConf_FileDistributor: macroToConf is NULL!");
            }
            else
            {
                Console.WriteLine("PopupConf_FileDistributor: macroToConf is VALID.");
            }
            if (callMethodName == FileDistributorMethodName)
            {
                var splitter = new DataSetSplitter() { MacroInstance = macroToConf };
                Console.WriteLine($"Created DataSetSplitter, MacroInstance: {(splitter.MacroInstance == null ? "NULL" : "Valid")}");
                return splitter;
            }
            return null;
        }

        /// <summary>
        /// 供外部 WinForm 直接呼叫的方法：進行檔案搬移，並於各子資料夾內產生列出圖檔路徑的 txt 檔，
        /// 使 UI 與 Macro 共用相同轉檔邏輯。
        /// </summary>
        public void DoDistributeFilesDirect(string folderPath, double trainRatio, double testRatio, double valRatio)
        {
            int moved = DistributeFiles(folderPath, trainRatio, testRatio, valRatio);

            // 在每個子資料夾內產生 .txt 檔，列出所有圖片檔案路徑
            string[] subdirectories = { "train", "test", "val" };
            foreach (string subdir in subdirectories)
            {
                string subdirectoryPath = Path.Combine(folderPath, subdir);
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
                                                                  file.EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
                                                  .ToArray();

                    using (StreamWriter sw = new StreamWriter(outputFile))
                    {
                        foreach (string file in photoFiles)
                        {
                            sw.WriteLine(file);
                        }
                    }
                }
            }

            MessageBox.Show(
                $"DoDistributeFilesDirect 已完成\n" +
                $"搬移數量 = {moved}\n" +
                $"(來源路徑: {folderPath})\n" +
                $"Train={trainRatio}, Test={testRatio}, Val={valRatio}",
                "完成",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // 以下為 GET/SET 參數的內部實作
        private UDataCarrier[] GetArr(UMacro macro, out bool bRet)
        {
            bRet = true;
            var arr = macro.MutableInitialData?.Data as UDataCarrier[];
            if (arr == null || arr.Length < 4)
            {
                arr = new UDataCarrier[4];
                for (int i = 0; i < 4; i++)
                    arr[i] = new UDataCarrier();
                macro.MutableInitialData = UDataCarrier.MakeOne(arr, true);
            }
            return arr;
        }
        private void UpdateParam(UMacro macro, int idx, object val)
        {
            var arr = GetArr(macro, out _);
            arr[idx].Build(val);
        }

        private bool IoctrlSet_FolderPath(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            string path = UDataCarrier.GetItem(data, 0, "", out _);
            UpdateParam(m, INDEX_FOLDER, path);
            return true;
        }
        private UDataCarrier[] IoctrlGet_FolderPath(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            string path = UDataCarrier.GetItem(arr, INDEX_FOLDER, "", out _);
            return UDataCarrier.MakeOneItemArray(path);
        }

        public bool SetFolderPath(UMacro macro, string path)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(path ?? "");
            return IoctrlSet_FolderPath(null, macro, dataArr);
        }
        public string GetFolderPath(UMacro macro)
        {
            if (macro == null) return "";
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_FolderPath(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, "", out _);
            return "";
        }

        private bool IoctrlSet_TrainRatio(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            double val = UDataCarrier.GetItem(data, 0, 0.8, out _);
            UpdateParam(m, INDEX_TRAIN, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_TrainRatio(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            double v = UDataCarrier.GetItem(arr, INDEX_TRAIN, 0.8, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetTrainRatio(UMacro macro, double val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_TrainRatio(null, macro, dataArr);
        }
        public double GetTrainRatio(UMacro macro)
        {
            if (macro == null) return 0.8;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_TrainRatio(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, 0.8, out _);
            return 0.8;
        }

        private bool IoctrlSet_TestRatio(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            double val = UDataCarrier.GetItem(data, 0, 0.1, out _);
            UpdateParam(m, INDEX_TEST, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_TestRatio(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            double v = UDataCarrier.GetItem(arr, INDEX_TEST, 0.1, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetTestRatio(UMacro macro, double val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_TestRatio(null, macro, dataArr);
        }
        public double GetTestRatio(UMacro macro)
        {
            if (macro == null) return 0.1;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_TestRatio(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, 0.1, out _);
            return 0.1;
        }

        private bool IoctrlSet_ValRatio(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            double val = UDataCarrier.GetItem(data, 0, 0.1, out _);
            UpdateParam(m, INDEX_VAL, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_ValRatio(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            double v = UDataCarrier.GetItem(arr, INDEX_VAL, 0.1, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetValRatio(UMacro macro, double val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_ValRatio(null, macro, dataArr);
        }
        public double GetValRatio(UMacro macro)
        {
            if (macro == null) return 0.1;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_ValRatio(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, 0.1, out _);
            return 0.1;
        }
    }
}
