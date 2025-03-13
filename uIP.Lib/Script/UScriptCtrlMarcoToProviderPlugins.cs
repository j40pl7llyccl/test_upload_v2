using System;
using System.Collections.Generic;

namespace uIP.Lib.Script
{
    public static class UScriptCtrlMarcoToProviderPlugins
    {
        private static string _strNewMacroJoin = "NewMacroJoin";
        /// <summary>
        /// Description:
        /// 1. any macro add into script
        /// 2. broadcast to all plugin assemblies
        /// 
        /// Remark:
        /// cannot callback to script doing macro operating
        /// </summary>
        public static string NewMacroJoinCmd { get { return _strNewMacroJoin; } }
        /// <summary>
        /// Use in Macro provider plugin to install ioctl join macro
        /// </summary>
        /// <param name="fpSet">method in plugin</param>
        /// <returns>descriptor</returns>
        public static UScriptControlCarrierMacro CreateNewMacroJoin( fpSetMacroScriptControlCarrier fpSet )
        {
            UScriptControlCarrierMacro ret = new UScriptControlCarrierMacro( _strNewMacroJoin, false, true, false,
                                                                           new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Script Name"),
                                                                                                               new UDataCarrierTypeDescription(typeof(Int32), "Script ID"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Macros List"),
                                                                                                               new UDataCarrierTypeDescription(typeof(UMacro), "Macro to join")
                                                                           },
                                                                           null,
                                                                           new fpSetMacroScriptControlCarrier( fpSet ) );
            ret.HowToSpread = eUScriptControlCarrierSpreading.Broadcast;

            return ret;
        }

        private static string _strMacroRemove = "MacroRemove";
        /// <summary>
        /// Description:
        /// 1. any macro remove from a script
        /// 2. broadcast to all plugin assemblies
        /// 
        /// Remark:
        /// 1. cannot callback to script doing macro operating
        /// 2. just use the remove list and not do dispose
        /// </summary>
        public static string MacroRemoveCmd { get { return _strMacroRemove; } }
        /// <summary>
        /// Use in Macro provider plugin to install ioctl remove macro(s)
        /// </summary>
        /// <param name="fpSet">method in plugin</param>
        /// <returns>descriptor</returns>
        public static UScriptControlCarrierMacro CreateMacroRemove(fpSetMacroScriptControlCarrier fpSet)
        {
            UScriptControlCarrierMacro ret = new UScriptControlCarrierMacro( _strMacroRemove, false, true, false,
                                                                           new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Script Name"),
                                                                                                               new UDataCarrierTypeDescription(typeof(Int32), "Script ID"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Macros List"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Removed macro list")
                                                                           },
                                                                           null,
                                                                           new fpSetMacroScriptControlCarrier( fpSet ) );
            ret.HowToSpread = eUScriptControlCarrierSpreading.Broadcast;

            return ret;
        }

        /// <summary>
        /// Description:
        /// 1. any macro replace in a script
        /// 2. broadcast to all plugin assemblies
        /// 
        /// Remark:
        /// 1. cannot callback to script doing macro operating
        /// 2. just use and not do dispose
        /// </summary>
        public static string MacroReplaceCmd { get; private set; } = "MacroReplace";
        public static UScriptControlCarrierMacro CreateMacroReplace( fpSetMacroScriptControlCarrier fpSet )
        {
            UScriptControlCarrierMacro ret = new UScriptControlCarrierMacro( MacroReplaceCmd, false, true, false,
                                                                           new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Script Name"),
                                                                                                               new UDataCarrierTypeDescription(typeof(Int32), "Script ID"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Macros List"),
                                                                                                               new UDataCarrierTypeDescription(typeof(UMacro), "Prev macro"),
                                                                                                               new UDataCarrierTypeDescription(typeof(UMacro), "Curr macro")
                                                                           },
                                                                           null,
                                                                           new fpSetMacroScriptControlCarrier( fpSet ) );
            ret.HowToSpread = eUScriptControlCarrierSpreading.Broadcast;

            return ret;
        }

        private static string _strMacroSettingsReadDone1stChance = "MacroSettingsReadDoneCallChance1";
        public static string MacroSettingsReadDone1stChanceCmd { get { return _strMacroSettingsReadDone1stChance; } }
        public static UDataCarrier[] GenMacroSettingsReadDone1stChanceCmd(List<UScript> scriptsInst)
        {
            return UDataCarrier.MakeOneItemArray<List<UScript>>( scriptsInst );
        }
        private static string _strMacroSettingsReadDone2ndtChance = "MacroSettingsReadDoneCallChance2";
        public static string MacroSettingsReadDone2ndChanceCmd { get { return _strMacroSettingsReadDone2ndtChance; } }
        public static UDataCarrier[] GenMacroSettingsReadDone2ndChanceCmd( List<UScript> scriptsInst )
        {
            return UDataCarrier.MakeOneItemArray<List<UScript>>( scriptsInst );
        }
    }
}
