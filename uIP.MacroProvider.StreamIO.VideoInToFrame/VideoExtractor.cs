using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;
using uIP.MacroProvider.StreamIO.VideoInToFrame;

namespace VideoFrameExtractor
{
    public class VideoExtractor : UMacroMethodProviderPlugin
    {
        const string VideoToFrameMethodName = "VideoDev_VideosToFrame";

        // 使用者設定參數
        private string _videoFolderPath = string.Empty;
        private int _intervalSeconds;

        public enum eCallReturn
        {
            NONE,   // No error
            ERROR   // Error occurred
                    // ...other possible return values...
        }
        public VideoExtractor() : base()
        {
            m_strInternalGivenName = "VideoExtractor";
        }
        public override bool Initialize(UDataCarrier[] param)
        {
            /*--
                follow the example of uIP.MacroProvider.StreamIO.ImageFileLoader
            --*/
            var macro = new UMacro(null, m_strInternalGivenName, VideoToFrameMethodName, OpenVideoFile,
                                    null, //immutable
                                    null, //variable
                                    new UDataCarrierTypeDescription[] {
                                    new UDataCarrierTypeDescription(typeof(string), "Video file path")
                                    },
                                    new UDataCarrierTypeDescription[]{
                                        new UDataCarrierTypeDescription(typeof(string), "Image file path")
                                    } // return
                        );

            m_UserQueryOpenedMethods.Add(macro);

            m_createMacroDoneFromMethod.Add(VideoToFrameMethodName, MacroShellDoneCall_VideoFile);

            // config macro GET/SET 
            m_MacroControls.Add("LoadingVideoDir", new UScriptControlCarrierMacro("LoadingVideoDir", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Loading dir") },
                new fpGetMacroScriptControlCarrier((UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus) => IoctrlGet_LoadingDir(whichMacro.MethodName, whichMacro)),
                new fpSetMacroScriptControlCarrier((UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data) => IoctrlSet_LoadingDir(whichMacro.MethodName, whichMacro, data))));

            // popup UI to config
            m_macroMethodConfigPopup.Add(VideoToFrameMethodName, PopupConf_VideoFile);
            m_bOpened = true;
            return true;
        }
        //focus on SET about loading video dir
        private bool IoctrlSet_LoadingDir(string callMethodName, UMacro instance, UDataCarrier[] data)
        {
            if (instance == null || data == null || data.Length == 0) return false;
            if (instance.MutableInitialData == null)
                instance.MutableInitialData = new UDataCarrier(data[0].Data, data[0].Tp);
            else
                instance.MutableInitialData.Data = data[0].Data;
            instance.MutableInitialData.Tp = data[0].Tp;
            return true;
        }

        //focus on GET about loading video dir
        private UDataCarrier[] IoctrlGet_LoadingDir(string callMethodName, UMacro instance)
        {
            if (instance == null || instance.MutableInitialData == null) return null;
            return new UDataCarrier[] { new UDataCarrier(instance.MutableInitialData.Data, instance.MutableInitialData.Tp) };
        }

        //focus on popup UI to config
        private Form PopupConf_VideoFile(string callMethodName, UMacro macroToConf)
        {
            if (callMethodName == VideoToFrameMethodName)
            {
                return new FormConfVideoToFrame() { MacroInstance = macroToConf };
            }
            return null;
        }
        private bool MacroShellDoneCall_VideoFile(string callMethodName, UMacro instance)
        {
            return true;
        }
        private UDataCarrier[] OpenVideoFile(UMacro MacroInstance,
                                      UDataCarrier[] PrevPropagationCarrier, //上一個人傳入的資料
                                      List<UMacroProduceCarrierResult> historyResultCarriers,
                                      List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
                                      List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
                                      List<UScriptHistoryCarrier> historyCarrier,
                                  ref bool bStatusCode, ref string strStatusMessage,
                                  ref UDataCarrier[] CurrPropagationCarrier, //下一個傳出去的資料
                                  ref UDrawingCarriers CurrDrawingCarriers,
                                  ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
                                  ref fpUDataCarrierSetResHandler ResultCarrierHandler)
        {
            // 1. 若上一個 Macro 有傳入資料，可嘗試從 PrevPropagationCarrier 取出 intervalSeconds
            if (!UDataCarrier.GetByIndex(PrevPropagationCarrier, 0, "", out var inputFilepath) || !Directory.Exists(inputFilepath))
            {
                strStatusMessage = "no file path error";
                return null;
            }

            if (MacroInstance.MutableInitialData == null)
            {
                bStatusCode = false;
                strStatusMessage = "not config init data";
                return null;
            }
            if (!(MacroInstance.MutableInitialData.Data is UDataCarrier[] data) || data == null)
            {
                bStatusCode = false;
                strStatusMessage = "init data invalid";
                return null;
            }
            try
            {

                // 1. 設定輸出資料夾
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFilepath), Path.GetFileNameWithoutExtension(inputFilepath));
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                // 2. 處理 ProcessVideos()，並取得所有影片的影格對應資料 (for ini)
                ProcessVideos(inputFilepath, outputFolder, _intervalSeconds, MacroInstance);

                // 3. 下一個傳送出去的資料
                CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray(outputFolder);

                // 4. 假設成功處理，設置狀態碼和狀態訊息
                bStatusCode = true;
                strStatusMessage = "Success";

                return new UDataCarrier[] {
                    new UDataCarrier(outputFolder, typeof(string))
                };
            }
            catch (Exception e)
            {
                bStatusCode = false;
                strStatusMessage = $"Exception:{e.Message}";
                return null;
            }
        }

        public int GetVideoDuration(string videoPath)
        {
            try
            {
                using (VideoCapture cap = new VideoCapture(videoPath))
                {
                    if (!cap.IsOpened())
                    {
                        Console.WriteLine($"無法打開影片: {videoPath}");
                        return 0;
                    }

                    double fps = cap.Fps;
                    double frameCount = cap.FrameCount;
                    if (fps <= 0)
                    {
                        Console.WriteLine($"影片的 {videoPath} fps 為 0 或無效");
                        return 0;
                    }

                    int duration = (int)(frameCount / fps);
                    return duration;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得影片長度失敗: {videoPath}，錯誤: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 修改後：每隔 intervalSeconds 秒（從第一個間隔開始）擷取影片中的一幀，
        /// 儲存至 outputFolder 並以原始檔名_序號.jpg 命名，同時回傳一個 mapping (key= "30s"、"60s" 等)
        /// </summary>
        public Dictionary<string, string> SplitVideoIntoFrames(string videoPath, string outputFolder, double intervalSeconds, UMacro macro)
        {
            var mapping = new Dictionary<string, string>();
            try
            {
                // 若輸出資料夾不存在，則建立
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                using (VideoCapture cap = new VideoCapture(videoPath))
                {
                    if (!cap.IsOpened())
                    {
                        Console.WriteLine($"無法打開影片: {videoPath}");
                        return mapping;
                    }

                    double fps = cap.Fps;
                    if (fps <= 0)
                    {
                        Console.WriteLine($"影片 {videoPath} 的 FPS 無效。");
                        return mapping;
                    }

                    if (intervalSeconds <= 0)
                    {
                        Console.WriteLine("切割秒數必須大於 0");
                        return mapping;
                    }

                    // 計算 frameInterval
                    int frameInterval = (int)Math.Round(fps * intervalSeconds);
                    frameInterval = Math.Max(frameInterval, 1);

                    // 修改：從第一個間隔開始擷取，而非從 0 秒開始
                    int currentFramePosition = frameInterval;
                    int photoCount = 1; // 從 1 開始
                    // 取得影片原始檔名（不含副檔名）
                    string originalFileName = Path.GetFileNameWithoutExtension(videoPath);

                    using (Mat frame = new Mat())
                    {
                        while (true)
                        {
                            if (macro != null && macro.CancelExec)
                            {
                                Console.WriteLine("偵測到 Script 已取消，停止擷取影格");
                                break;
                            }
                            cap.Set(VideoCaptureProperties.PosFrames, currentFramePosition);
                            bool ret = cap.Read(frame);
                            if (!ret || frame.Empty())
                            {
                                break;
                            }

                            // 設定輸出檔名，依照範例格式：原始檔名_序號.jpg
                            string imageFileName = $"{originalFileName}_{photoCount}.jpg";
                            string photoPath = Path.Combine(outputFolder, imageFileName);
                            bool saveResult = Cv2.ImWrite(photoPath, frame);
                            if (saveResult)
                            {
                                Console.WriteLine($"儲存圖片: {photoPath}");
                                // 計算該張影格所代表的秒數（例如：30秒、60秒）
                                int secondsMark = (int)(photoCount * intervalSeconds);
                                string key = $"{secondsMark}s";
                                mapping[key] = imageFileName;
                                photoCount++;
                            }
                            else
                            {
                                Console.WriteLine($"儲存圖片失敗: {photoPath}");
                            }

                            currentFramePosition += frameInterval;
                            if (currentFramePosition >= cap.FrameCount)
                            {
                                break;
                            }
                        }
                    }

                    Console.WriteLine($"影片 {Path.GetFileName(videoPath)} 總共擷取 {photoCount - 1} 張圖片。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"影片 {videoPath} 切割影格時發生錯誤: {ex.Message}");
            }
            return mapping;
        }

        /// <summary>
        /// 處理指定資料夾下的所有影片檔，並將每部影片擷取的影格存到對應的輸出子資料夾中，
        /// 同時收集 ini 檔需用的資訊，最後寫出 config.ini
        /// </summary>
        public void ProcessVideos(string videoFolder, string outputFolder, double intervalSeconds, UMacro macro)
        {
            try
            {
                if (!Directory.Exists(videoFolder))
                {
                    Console.WriteLine($"影片目錄不存在: {videoFolder}");
                    return;
                }

                // 篩選影片檔案
                var videoFiles = Directory.GetFiles(videoFolder)
                    .Where(f => new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv" }
                                .Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();

                if (videoFiles.Length == 0)
                {
                    Console.WriteLine("找不到任何符合條件的影片檔。");
                    return;
                }

                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                // 收集 ini 資料的集合 (key: 原始檔名, value: 對應秒數及檔名 mapping)
                var iniMappings = new Dictionary<string, Dictionary<string, string>>();
                object iniLock = new object(); // 執行緒鎖

                // 使用平行處理
                Parallel.ForEach(videoFiles, video =>
                {
                    try
                    {
                        Console.WriteLine($"開始處理影片: {video}");
                        int duration = GetVideoDuration(video);
                        Console.WriteLine($"影片 {Path.GetFileName(video)} 總長度: {duration} 秒");

                        // 為每部影片建立以影片檔名命名的子資料夾
                        string videoOutputFolder = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(video));
                        if (!Directory.Exists(videoOutputFolder))
                            Directory.CreateDirectory(videoOutputFolder);

                        // 呼叫修改後的 SplitVideoIntoFrames()，取得 mapping 資料
                        var mapping = SplitVideoIntoFrames(video, videoOutputFolder, intervalSeconds, macro);

                        // 將 mapping 資料存入全域 iniMappings (以影片原始檔名為 key)
                        string originalName = Path.GetFileNameWithoutExtension(video);
                        lock (iniLock)
                        {
                            iniMappings[originalName] = mapping;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"處理影片 {video} 時發生錯誤: {ex.Message}");
                    }
                });

                Console.WriteLine("\n所有影片處理完成！");

                // 所有影片處理完畢後，寫出 config.ini 到 outputFolder
                WriteIniFile(outputFolder, intervalSeconds, iniMappings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"處理影片資料夾時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 寫出 ini 檔，格式如下：
        /// [System Section]
        /// intervalSeconds=設定的秒數
        ///
        /// [原始檔名1]
        /// 30s=原始檔名1_1.jpg
        /// 60s=原始檔名1_2.jpg
        ///
        /// [原始檔名2]
        /// 30s=原始檔名2_1.jpg
        /// 60s=原始檔名2_2.jpg
        /// 
        /// 如果intervalSeconds<1秒,比如0.5秒
        /// 
        /// [原始檔名1]
        /// 0.5s=原始檔名1_1.jpg
        /// 1.0s=原始檔名1_2.jpg
        /// 1.5s=原始檔名1_3.jpg
        /// </summary>
        private void WriteIniFile(string outputFolder, double intervalSeconds, Dictionary<string, Dictionary<string, string>> mappings)
        {
            try
            {
                //string iniFilePath = Path.Combine(outputFolder, "config.ini");
                string iniFilePath = Path.Combine(Application.StartupPath, "config_video.ini");
                using (StreamWriter sw = new StreamWriter(iniFilePath))
                {
                    // 寫入 System Section
                    sw.WriteLine("[System Section]");
                    sw.WriteLine($"intervalSeconds={intervalSeconds}");
                    sw.WriteLine();

                    // 寫入每個影片的 mapping 資訊
                    foreach (var videoMapping in mappings)
                    {
                        sw.WriteLine($"[{videoMapping.Key}]"); // 原始檔名作為區段名稱

                        // 如果設定的秒數小於 1 秒，則依序產生時間鍵
                        if (intervalSeconds < 1)
                        {
                            int counter = 1;
                            foreach (var kvp in videoMapping.Value)
                            {
                                // 每一筆的時間依 counter * intervalSeconds 計算，並格式化為一位小數
                                double time = intervalSeconds * counter;
                                sw.WriteLine($"{time:0.##}s={kvp.Value}");
                                counter++;
                            }
                        }
                        else
                        {
                            // 其他情況則採用原有的鍵值
                            int counter = 1;
                            foreach (var kvp in videoMapping.Value)
                            {
                                int time = (int)intervalSeconds * counter;
                                sw.WriteLine($"{time}s={kvp.Value}");
                            }
                        }
                        sw.WriteLine();
                    }
                }
                Console.WriteLine($"成功寫出 ini 檔: {iniFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"寫出 ini 檔時發生錯誤: {ex.Message}");
            }
        }


        public override void Close()
        {
            base.Close();
        }
    }
}




