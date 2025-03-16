using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    /// <summary>
    /// 「DatasetDev_ProcessPath」 Plugin 範例，
    /// 可透過 Macro 方式或 WinForm 直接呼叫執行三條路徑的搬檔作業，
    /// 並根據各路徑勾選的 Train/Test/Val 選項搬移到目標資料夾中。
    /// </summary>
    public class ProcessPath : UMacroMethodProviderPlugin
    {
        private const string MacroMethodName = "DatasetDev_ProcessPath";

        // 預計 12 個參數:
        // Path1=0, R1Train=1, R1Test=2, R1Val=3,
        // Path2=4, R2Train=5, R2Test=6, R2Val=7,
        // Path3=8, R3Train=9, R3Test=10, R3Val=11.
        private const int INDEX_PATH1 = 0;
        private const int INDEX_R1TRAIN = 1;
        private const int INDEX_R1TEST = 2;
        private const int INDEX_R1VAL = 3;

        private const int INDEX_PATH2 = 4;
        private const int INDEX_R2TRAIN = 5;
        private const int INDEX_R2TEST = 6;
        private const int INDEX_R2VAL = 7;

        private const int INDEX_PATH3 = 8;
        private const int INDEX_R3TRAIN = 9;
        private const int INDEX_R3TEST = 10;
        private const int INDEX_R3VAL = 11;

        public ProcessPath() : base()
        {
            m_strInternalGivenName = "ProcessPath";
        }

        public override bool Initialize(UDataCarrier[] param)
        {
            if (m_bOpened) return true;

            // 1) 註冊 Macro 方法 "DatasetDev_ProcessPath"
            m_UserQueryOpenedMethods.Add(
                new UMacro(
                    null,
                    m_strCSharpDefClassName,
                    MacroMethodName,
                    Macro_ProcessPath,
                    null, null, null,
                    new UDataCarrierTypeDescription[]{
                        new UDataCarrierTypeDescription(typeof(int), "MovedCount")
                    }
                )
            );
            m_createMacroDoneFromMethod.Add(MacroMethodName, MacroShellDoneCall_ProcessPath);

            // 2) 註冊 GET/SET 參數：Path1, R1Train, R1Test, R1Val, Path2, R2Train, R2Test, R2Val, Path3, R3Train, R3Test, R3Val
            m_MacroControls.Add("Path1", new UScriptControlCarrierMacro(
                "Path1", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Path1 Folder") },
                IoctrlGet_Path1, IoctrlSet_Path1
            ));
            m_MacroControls.Add("R1Train", new UScriptControlCarrierMacro(
                "R1Train", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row1 Train") },
                IoctrlGet_R1Train, IoctrlSet_R1Train
            ));
            m_MacroControls.Add("R1Test", new UScriptControlCarrierMacro(
                "R1Test", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row1 Test") },
                IoctrlGet_R1Test, IoctrlSet_R1Test
            ));
            m_MacroControls.Add("R1Val", new UScriptControlCarrierMacro(
                "R1Val", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row1 Val") },
                IoctrlGet_R1Val, IoctrlSet_R1Val
            ));

            m_MacroControls.Add("Path2", new UScriptControlCarrierMacro(
                "Path2", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Path2 Folder") },
                IoctrlGet_Path2, IoctrlSet_Path2
            ));
            m_MacroControls.Add("R2Train", new UScriptControlCarrierMacro(
                "R2Train", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row2 Train") },
                IoctrlGet_R2Train, IoctrlSet_R2Train
            ));
            m_MacroControls.Add("R2Test", new UScriptControlCarrierMacro(
                "R2Test", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row2 Test") },
                IoctrlGet_R2Test, IoctrlSet_R2Test
            ));
            m_MacroControls.Add("R2Val", new UScriptControlCarrierMacro(
                "R2Val", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row2 Val") },
                IoctrlGet_R2Val, IoctrlSet_R2Val
            ));

            m_MacroControls.Add("Path3", new UScriptControlCarrierMacro(
                "Path3", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Path3 Folder") },
                IoctrlGet_Path3, IoctrlSet_Path3
            ));
            m_MacroControls.Add("R3Train", new UScriptControlCarrierMacro(
                "R3Train", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row3 Train") },
                IoctrlGet_R3Train, IoctrlSet_R3Train
            ));
            m_MacroControls.Add("R3Test", new UScriptControlCarrierMacro(
                "R3Test", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row3 Test") },
                IoctrlGet_R3Test, IoctrlSet_R3Test
            ));
            m_MacroControls.Add("R3Val", new UScriptControlCarrierMacro(
                "R3Val", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(bool), "Row3 Val") },
                IoctrlGet_R3Val, IoctrlSet_R3Val
            ));

            // 3) 註冊 Popup 視窗 (用於 GUI 設定)
            m_macroMethodConfigPopup.Add(MacroMethodName, PopupConf_ProcessPath);

            m_bOpened = true;
            return true;
        }
        private bool MacroShellDoneCall_ProcessPath(string callMethodName, UMacro instance)
        {
            // 這裡可以加入針對 MacroInstance 的初始化檢查或設定
            // 例如：
            // if(instance.MutableInitialData == null)
            // {
            //     // 初始化預設參數
            //     instance.MutableInitialData = UDataCarrier.MakeOne(new UDataCarrier[12], true);
            // }
            // 其他必要的檢查或設定

            return true; // 返回 true 代表初始化成功
        }
        private Form PopupConf_ProcessPath(string callMethodName, UMacro macroToConf)
        {
            return new DataSetSelect() { MacroInstance = macroToConf };
        }

        // Macro 執行邏輯 (供 Macro 模式時使用)
        private UDataCarrier[] Macro_ProcessPath(
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
                // 檢查並讀取 12 格參數
                if (!(MacroInstance.MutableInitialData.Data is UDataCarrier[] arr) || arr.Length < 12)
                {
                    bStatusCode = false;
                    strStatusMessage = "ProcessPath: need 12 param => 3 paths + 9 boolean.";
                    return null;
                }

                // 讀取 Path1 與 Row1 選項
                string path1 = UDataCarrier.GetItem(arr, INDEX_PATH1, "", out _);
                bool r1Train = UDataCarrier.GetItem(arr, INDEX_R1TRAIN, false, out _);
                bool r1Test = UDataCarrier.GetItem(arr, INDEX_R1TEST, false, out _);
                bool r1Val = UDataCarrier.GetItem(arr, INDEX_R1VAL, false, out _);
                // 讀取 Path2 與 Row2 選項
                string path2 = UDataCarrier.GetItem(arr, INDEX_PATH2, "", out _);
                bool r2Train = UDataCarrier.GetItem(arr, INDEX_R2TRAIN, false, out _);
                bool r2Test = UDataCarrier.GetItem(arr, INDEX_R2TEST, false, out _);
                bool r2Val = UDataCarrier.GetItem(arr, INDEX_R2VAL, false, out _);
                // 讀取 Path3 與 Row3 選項
                string path3 = UDataCarrier.GetItem(arr, INDEX_PATH3, "", out _);
                bool r3Train = UDataCarrier.GetItem(arr, INDEX_R3TRAIN, false, out _);
                bool r3Test = UDataCarrier.GetItem(arr, INDEX_R3TEST, false, out _);
                bool r3Val = UDataCarrier.GetItem(arr, INDEX_R3VAL, false, out _);

                int totalMoved = 0;
                totalMoved += MoveFiles(path1, r1Train, r1Test, r1Val);
                totalMoved += MoveFiles(path2, r2Train, r2Test, r2Val);
                totalMoved += MoveFiles(path3, r3Train, r3Test, r3Val);

                bStatusCode = true;
                strStatusMessage = $"ProcessPath done, total moved: {totalMoved}";
                CurrPropagationCarrier = UDataCarrier.MakeOneItemArray(totalMoved);
                return null;
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = $"Exception in Macro_ProcessPath: {ex}";
                return null;
            }
        }

        // 新增：供 WinForm 直接呼叫的公開方法，統一處理三條路徑搬檔
        public void DoProcessAllPaths(
            string path1, bool r1Train, bool r1Test, bool r1Val,
            string path2, bool r2Train, bool r2Test, bool r2Val,
            string path3, bool r3Train, bool r3Test, bool r3Val)
        {
            int totalMoved = 0;
            if (!string.IsNullOrEmpty(path1))
            {
                totalMoved += MoveFiles(path1, r1Train, r1Test, r1Val);
            }
            if (!string.IsNullOrEmpty(path2))
            {
                totalMoved += MoveFiles(path2, r2Train, r2Test, r2Val);
            }
            if (!string.IsNullOrEmpty(path3))
            {
                totalMoved += MoveFiles(path3, r3Train, r3Test, r3Val);
            }
            MessageBox.Show(
                $"直接呼叫 DoProcessAllPaths:\n搬移總數 = {totalMoved}",
                "直接搬檔結果",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // 實際搬檔邏輯：給定來源路徑與對應的 Train/Test/Val 選項，
        // 搬移檔案到桌面 AI_DataSets 資料夾下的 train/test/val 子資料夾，
        // 若發現副檔名為 .txt，則改名為對應資料夾名稱
        private int MoveFiles(string path, bool isTrain, bool isTest, bool isVal)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return 0;

            string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_DataSets");
            string trainDir = Path.Combine(baseDir, "train");
            string testDir = Path.Combine(baseDir, "test");
            string valDir = Path.Combine(baseDir, "val");
            Directory.CreateDirectory(trainDir);
            Directory.CreateDirectory(testDir);
            Directory.CreateDirectory(valDir);

            var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            int moved = 0;
            foreach (var f in files)
            {
                string fileName = Path.GetFileName(f);
                string targetDir = "";
                string folderName = "";
                if (isTrain)
                {
                    targetDir = trainDir;
                    folderName = "train";
                }
                else if (isTest)
                {
                    targetDir = testDir;
                    folderName = "test";
                }
                else if (isVal)
                {
                    targetDir = valDir;
                    folderName = "val";
                }
                // 若檔案為 txt 副檔名，則改名為 folderName.txt
                if (Path.GetExtension(fileName).ToLower() == ".txt")
                {
                    fileName = folderName + ".txt";
                }
                File.Move(f, Path.Combine(targetDir, fileName));
                moved++;
            }
            return moved;
        }

        // 以下為 GET/SET 參數的內部實作 (略，同原有邏輯)

        private UDataCarrier[] GetArr(UMacro macro, out bool bRet)
        {
            bRet = true;
            var arr = macro.MutableInitialData?.Data as UDataCarrier[];
            if (arr == null || arr.Length < 12)
            {
                arr = new UDataCarrier[12];
                for (int i = 0; i < 12; i++) arr[i] = new UDataCarrier();
                macro.MutableInitialData = UDataCarrier.MakeOne(arr, true);
            }
            return arr;
        }
        private void UpdateParam(UMacro macro, int idx, object val)
        {
            var arr = GetArr(macro, out _);
            arr[idx].Build(val);
        }

        // Path1
        private bool IoctrlSet_Path1(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            string path = UDataCarrier.GetItem(data, 0, "", out _);
            UpdateParam(m, INDEX_PATH1, path);
            return true;
        }
        private UDataCarrier[] IoctrlGet_Path1(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            string path = UDataCarrier.GetItem(arr, INDEX_PATH1, "", out _);
            return UDataCarrier.MakeOneItemArray(path);
        }
        public bool SetPath1(UMacro macro, string path)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(path ?? "");
            return IoctrlSet_Path1(null, macro, dataArr);
        }
        public string GetPath1(UMacro macro)
        {
            if (macro == null) return "";
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_Path1(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, "", out _);
            return "";
        }
        // R1Train
        private bool IoctrlSet_R1Train(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R1TRAIN, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R1Train(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R1TRAIN, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR1Train(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R1Train(null, macro, dataArr);
        }
        public bool GetR1Train(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R1Train(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // R1Test
        private bool IoctrlSet_R1Test(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R1TEST, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R1Test(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R1TEST, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR1Test(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R1Test(null, macro, dataArr);
        }
        public bool GetR1Test(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R1Test(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // R1Val
        private bool IoctrlSet_R1Val(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R1VAL, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R1Val(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R1VAL, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR1Val(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R1Val(null, macro, dataArr);
        }
        public bool GetR1Val(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R1Val(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // Path2
        private bool IoctrlSet_Path2(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            string path = UDataCarrier.GetItem(data, 0, "", out _);
            UpdateParam(m, INDEX_PATH2, path);
            return true;
        }
        private UDataCarrier[] IoctrlGet_Path2(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            string p = UDataCarrier.GetItem(arr, INDEX_PATH2, "", out _);
            return UDataCarrier.MakeOneItemArray(p);
        }
        public bool SetPath2(UMacro macro, string path)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(path ?? "");
            return IoctrlSet_Path2(null, macro, dataArr);
        }
        public string GetPath2(UMacro macro)
        {
            if (macro == null) return "";
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_Path2(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, "", out _);
            return "";
        }
        // R2Train
        private bool IoctrlSet_R2Train(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R2TRAIN, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R2Train(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R2TRAIN, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR2Train(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R2Train(null, macro, dataArr);
        }
        public bool GetR2Train(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R2Train(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // R2Test
        private bool IoctrlSet_R2Test(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R2TEST, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R2Test(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R2TEST, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR2Test(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R2Test(null, macro, dataArr);
        }
        public bool GetR2Test(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R2Test(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // R2Val
        private bool IoctrlSet_R2Val(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R2VAL, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R2Val(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R2VAL, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR2Val(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R2Val(null, macro, dataArr);
        }
        public bool GetR2Val(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R2Val(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // Path3
        private bool IoctrlSet_Path3(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            string path = UDataCarrier.GetItem(data, 0, "", out _);
            UpdateParam(m, INDEX_PATH3, path);
            return true;
        }
        private UDataCarrier[] IoctrlGet_Path3(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            string p = UDataCarrier.GetItem(arr, INDEX_PATH3, "", out _);
            return UDataCarrier.MakeOneItemArray(p);
        }
        public bool SetPath3(UMacro macro, string path)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(path ?? "");
            return IoctrlSet_Path3(null, macro, dataArr);
        }
        public string GetPath3(UMacro macro)
        {
            if (macro == null) return "";
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_Path3(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, "", out _);
            return "";
        }
        // R3Train
        private bool IoctrlSet_R3Train(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R3TRAIN, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R3Train(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R3TRAIN, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR3Train(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R3Train(null, macro, dataArr);
        }
        public bool GetR3Train(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R3Train(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // R3Test
        private bool IoctrlSet_R3Test(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R3TEST, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R3Test(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R3TEST, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR3Test(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R3Test(null, macro, dataArr);
        }
        public bool GetR3Test(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R3Test(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
        // R3Val
        private bool IoctrlSet_R3Val(UScriptControlCarrier c, UMacro m, UDataCarrier[] data)
        {
            bool val = UDataCarrier.GetItem(data, 0, false, out _);
            UpdateParam(m, INDEX_R3VAL, val);
            return true;
        }
        private UDataCarrier[] IoctrlGet_R3Val(UScriptControlCarrier c, UMacro m, ref bool bRet)
        {
            var arr = GetArr(m, out bRet);
            bool v = UDataCarrier.GetItem(arr, INDEX_R3VAL, false, out _);
            return UDataCarrier.MakeOneItemArray(v);
        }
        public bool SetR3Val(UMacro macro, bool val)
        {
            if (macro == null) return false;
            UDataCarrier[] dataArr = UDataCarrier.MakeOneItemArray(val);
            return IoctrlSet_R3Val(null, macro, dataArr);
        }
        public bool GetR3Val(UMacro macro)
        {
            if (macro == null) return false;
            bool dummy = false;
            UDataCarrier[] ret = IoctrlGet_R3Val(null, macro, ref dummy);
            if (ret != null && ret.Length > 0)
                return UDataCarrier.GetItem(ret, 0, false, out _);
            return false;
        }
    }
}




