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

namespace VideoFrameExtractor
{
    public class VideoExtractor : UMacroMethodProviderPlugin
    {
        const string VideoToFrameMethodName = "VideoDev_VideosToFrame";

        public VideoExtractor() : base()
        {
            m_strInternalGivenName = "VideoExtractor";
        }

        public override bool Initialize(UDataCarrier[] param)
        {
            // 註冊 Macro 方法：設定 immutable/variable 輸入與輸出資料型態
            var macro = new UMacro(null, m_strInternalGivenName, VideoToFrameMethodName, OpenVideoFile,
                null, // immutable
                null, // variable
                new UDataCarrierTypeDescription[] {
                    new UDataCarrierTypeDescription(typeof(string), "Video file path")
                },
                new UDataCarrierTypeDescription[] {
                    new UDataCarrierTypeDescription(typeof(string), "Image file path")
                }
            );
            m_UserQueryOpenedMethods.Add(macro);

            // 註冊執行完成後的 Callback 方法
            m_createMacroDoneFromMethod.Add(VideoToFrameMethodName, MacroShellDoneCall_VideoFile);

            // 設定 Macro 控制項：GET/SET LoadingVideoDir
            m_MacroControls.Add("LoadingVideoDir", new UScriptControlCarrierMacro("LoadingVideoDir", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Loading dir") },
                new fpGetMacroScriptControlCarrier((UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus) => IoctrlGet_LoadingDir(whichMacro.MethodName, whichMacro)),
                new fpSetMacroScriptControlCarrier((UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data) => IoctrlSet_LoadingDir(whichMacro.MethodName, whichMacro, data))
            ));

            // 註冊 Popup UI 設定介面
            m_macroMethodConfigPopup.Add(VideoToFrameMethodName, PopupConf_VideoFile);
            m_bOpened = true;
            return true;
        }

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

        private UDataCarrier[] IoctrlGet_LoadingDir(string callMethodName, UMacro instance)
        {
            if (instance == null || instance.MutableInitialData == null) return null;
            return new UDataCarrier[] { new UDataCarrier(instance.MutableInitialData.Data, instance.MutableInitialData.Tp) };
        }

        private Form PopupConf_VideoFile(string callMethodName, UMacro macroToConf)
        {
            if (callMethodName == VideoToFrameMethodName)
            {
                // 透過 Popup 彈出 UI 配置介面
                return new uIP.MacroProvider.StreamIO.VideoInToFrame.FormConfVideoToFrame() { MacroInstance = macroToConf };
            }
            return null;
        }

        private bool MacroShellDoneCall_VideoFile(string callMethodName, UMacro instance)
        {
            return true;
        }

        /// <summary>
        /// 供 UI 呼叫的公開方法：依照影片資料夾、切割間隔及 MacroInstance 執行轉檔。
        /// </summary>
        public void StartConvertByCode(string videoFolder, double intervalSeconds, UMacro macroInstance)
        {
            // 這裡設定輸出資料夾，統一放在應用程式啟動目錄下的 Video_Output 子資料夾
            string outputFolder = Path.Combine(Application.StartupPath, "Video_Output");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            Task.Run(() =>
            {
                try
                {
                    ProcessVideos(videoFolder, outputFolder, intervalSeconds, macroInstance);
                    // 轉檔完成後，提示使用者
                    MessageBox.Show("所有影片處理完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"影片處理發生錯誤：{ex.Message}\n詳細資訊：{ex.StackTrace}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
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
                        Console.WriteLine($"影片 {videoPath} 的 FPS 為 0 或無效");
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
        /// 每隔 intervalSeconds 秒（從第一個間隔開始）擷取影片中的一幀，
        /// 將圖片存到 outputFolder，並回傳秒數與檔名的對應 mapping。
        /// </summary>
        public Dictionary<string, string> SplitVideoIntoFrames(string videoPath, string outputFolder, double intervalSeconds, UMacro macro)
        {
            var mapping = new Dictionary<string, string>();
            try
            {
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

                    // 計算每隔幾個 frame 擷取一次
                    int frameInterval = (int)Math.Round(fps * intervalSeconds);
                    frameInterval = Math.Max(frameInterval, 1);

                    int currentFramePosition = frameInterval;
                    int photoCount = 1;
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
                                break;

                            string imageFileName = $"{originalFileName}_{photoCount}.jpg";
                            string photoPath = Path.Combine(outputFolder, imageFileName);
                            bool saveResult = Cv2.ImWrite(photoPath, frame);
                            if (saveResult)
                            {
                                Console.WriteLine($"儲存圖片: {photoPath}");
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
                                break;
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
        /// 處理指定資料夾下的所有影片檔，將每部影片的影格存到對應輸出資料夾，並寫出 config.ini
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

                var iniMappings = new Dictionary<string, Dictionary<string, string>>();
                object iniLock = new object();

                Parallel.ForEach(videoFiles, video =>
                {
                    try
                    {
                        Console.WriteLine($"開始處理影片: {video}");
                        int duration = GetVideoDuration(video);
                        Console.WriteLine($"影片 {Path.GetFileName(video)} 總長度: {duration} 秒");

                        string videoOutputFolder = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(video));
                        if (!Directory.Exists(videoOutputFolder))
                            Directory.CreateDirectory(videoOutputFolder);

                        var mapping = SplitVideoIntoFrames(video, videoOutputFolder, intervalSeconds, macro);
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
        /// [原始檔名]
        /// 30s=原始檔名_1.jpg
        /// 60s=原始檔名_2.jpg
        /// </summary>
        private void WriteIniFile(string outputFolder, double intervalSeconds, Dictionary<string, Dictionary<string, string>> mappings)
        {
            try
            {
                string iniFilePath = Path.Combine(Application.StartupPath, "config_video.ini");
                using (StreamWriter sw = new StreamWriter(iniFilePath))
                {
                    sw.WriteLine("[System Section]");
                    sw.WriteLine($"intervalSeconds={intervalSeconds}");
                    sw.WriteLine();

                    foreach (var videoMapping in mappings)
                    {
                        sw.WriteLine($"[{videoMapping.Key}]");
                        if (intervalSeconds < 1)
                        {
                            int counter = 1;
                            foreach (var kvp in videoMapping.Value)
                            {
                                double time = intervalSeconds * counter;
                                sw.WriteLine($"{time:0.##}s={kvp.Value}");
                                counter++;
                            }
                        }
                        else
                        {
                            int counter = 1;
                            foreach (var kvp in videoMapping.Value)
                            {
                                int time = (int)(intervalSeconds * counter);
                                sw.WriteLine($"{time}s={kvp.Value}");
                                counter++;
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

        /// <summary>
        /// Macro 方法的入口，根據前一個 Macro 輸入的影片路徑處理影片，並回傳輸出路徑。
        /// </summary>
        private UDataCarrier[] OpenVideoFile(UMacro MacroInstance,
            UDataCarrier[] PrevPropagationCarrier,
            List<UMacroProduceCarrierResult> historyResultCarriers,
            List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
            List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
            List<UScriptHistoryCarrier> historyCarrier,
            ref bool bStatusCode, ref string strStatusMessage,
            ref UDataCarrier[] CurrPropagationCarrier,
            ref UDrawingCarriers CurrDrawingCarriers,
            ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
            ref fpUDataCarrierSetResHandler ResultCarrierHandler)
        {
            // 從上一個 Macro 取得影片路徑
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
                // 設定輸出資料夾（以影片檔名作為子資料夾）
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFilepath), Path.GetFileNameWithoutExtension(inputFilepath));
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                ProcessVideos(inputFilepath, outputFolder, 1, MacroInstance);

                CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray(outputFolder);
                bStatusCode = true;
                strStatusMessage = "Success";

                return new UDataCarrier[] { new UDataCarrier(outputFolder, typeof(string)) };
            }
            catch (Exception e)
            {
                bStatusCode = false;
                strStatusMessage = $"Exception:{e.Message}";
                return null;
            }
        }
    }
}



