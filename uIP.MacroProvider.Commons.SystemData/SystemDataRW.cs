using System;
using System.Collections.Generic;
using System.IO;
using uIP.Lib;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.Commons.SystemData
{
    public class SystemDataRW : UMacroMethodProviderPlugin
    {
        const string EnvVariablesFilename = "env_vars.xml";
        const string EnvVariableLangName = "Language";
        const string EnvVariableDataOutputPathName = "OutputDataPath";

        private string WDPath { get; set; } = "";
        private string RWPath { get; set; } = "";
        private string RWFilepath { get; set; } = "";

        private string OutputDataPath { get; set; } = "";

        public SystemDataRW()
        {
        }

        public override bool Initialize(UDataCarrier[] param)
        {
            // check working dir exists
            if (!UDataCarrier.GetByIndex(param, 1, "", out var workingDir) || !Directory.Exists(workingDir))
                return false;

            if (m_bOpened)
                return true;

            WDPath = workingDir;

            // config working dir
            RWPath = Path.Combine(workingDir, ULibAgent.IniFolderName);

            // config to default
            SetEnvDefault();

            // restore info
            RWFilepath = Path.Combine(RWPath, EnvVariablesFilename);
            if (File.Exists(RWFilepath))
            {
                UDataCarrier[] got = null;
                string[] dummy = null;
                if (UDataCarrier.ReadXml(RWFilepath, ref got, ref dummy) &&
                    UDataCarrier.GetByIndex<string[]>(got, 0, null, out var convD) &&
                    UDataCarrier.DeserializeDicKeyStringValueOne(convD, out var vars))
                {
                    RestoreEnv(vars);
                }
            }

            // reg system down callback
            ResourceManager.AddSystemDownCalls(new Action(SystemDownCallSave));

            // process specific field
            var sysIniPath = Path.Combine(WDPath, ULibAgent.IniFolderName, ULibAgent.SystemIniFilename);
            if (File.Exists(sysIniPath))
            {
                var iniR = new IniReaderUtility();
                if (iniR.Parsing(sysIniPath))
                {
                    var sd = iniR.Get("startup");
                    if (sd != null && sd.Data != null)
                    {
                        var dic = new Dictionary<string, string[]>();
                        foreach (var v in sd.Data)
                        {
                            if (string.IsNullOrEmpty(v.Key))
                                continue;
                            if (v.Values == null)
                                continue;
                            if (dic.ContainsKey(v.Key))
                                continue;
                            dic.Add(v.Key, v.Values);
                        }
                        if (dic.Count > 0)
                            ResourceManager.Reg("startup", dic);
                    }
                }
            }

            m_bOpened = true;
            return true;
        }

        private void SetEnvDefault()
        {
            // set language
            ResourceManager.Reg(EnvVariableLangName, "en");
            // set output data path; create, not change, not save
            var fullpath = CommonUtilities.RCreateDir2(Path.Combine(WDPath, "output_data"));
            ResourceManager.Reg(EnvVariableDataOutputPathName, fullpath);
        }
        private static void RestoreEnv(Dictionary<string, UDataCarrier> kv)
        {
            if (kv == null) return;

            if (kv.TryGetValue(EnvVariableLangName, out var v))
                ResourceManager.Reg(EnvVariableLangName, v.Data);
        }

        private void SystemDownCallSave()
        {
            if (string.IsNullOrEmpty(RWFilepath))
                return;

            //
            // collect data and save
            //
            Dictionary<string, UDataCarrier> kv = new Dictionary<string, UDataCarrier>();
            // language
            ResourceManager.Get<string>(EnvVariableLangName, "en", out var lang);
            kv.Add(EnvVariableLangName, UDataCarrier.MakeOne(lang));

            // convert to save data
            UDataCarrier.SerializeDicKeyString(kv, out var serialized);
            if (serialized != null)
                UDataCarrier.WriteXml(new UDataCarrier[] { UDataCarrier.MakeOne(serialized) }, RWFilepath, null);
        }
    }
}
