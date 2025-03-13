using System;
using System.Collections.Generic;

namespace uIP.Lib.Script
{
    public static class UMacroProviderDefaultSupports
    {
        private static string _strNewMacroJoin = "NewMacroJoin";
        /// <summary>
        /// Macro join into a script; forbidden to do script OP(new/ delete/ replace...) cause deadlock
        /// </summary>
        public static string NewMacroJoinCmd { get { return _strNewMacroJoin; } }
        public static UScriptControlCarrierMacro CreateNewMacroJoin( fpSetMacroScriptControlCarrier fpSet )
        {
            UScriptControlCarrierMacro ret = new UScriptControlCarrierMacro( _strNewMacroJoin, false, true, false,
                                                                           new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Script Name"),
                                                                                                               new UDataCarrierTypeDescription(typeof(Int32), "Script ID"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Macros List"),
                                                                                                               new UDataCarrierTypeDescription(typeof(UMacro), "Join Macro")
                                                                           },
                                                                           null,
                                                                           new fpSetMacroScriptControlCarrier( fpSet ) );
            ret.HowToSpread = eUScriptControlCarrierSpreading.Broadcast;

            return ret;
        }

        private static string _strMacroRemove = "MacroRemove";
        /// <summary>
        /// Any macro remove from a script; forbidden to do script op(new/ delete/ replace...) cause deadlock
        /// </summary>
        public static string MacroRemoveCmd { get { return _strMacroRemove; } }
        public static UScriptControlCarrierMacro CreateMacroRemove(fpSetMacroScriptControlCarrier fpSet)
        {
            UScriptControlCarrierMacro ret = new UScriptControlCarrierMacro( _strMacroRemove, false, true, false,
                                                                           new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Script Name"),
                                                                                                               new UDataCarrierTypeDescription(typeof(Int32), "Script ID"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Macros List"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Removed List")
                                                                           },
                                                                           null,
                                                                           new fpSetMacroScriptControlCarrier( fpSet ) );
            ret.HowToSpread = eUScriptControlCarrierSpreading.Broadcast;

            return ret;
        }

        public static string MacroReplaceCmd { get; private set; } = "MacroReplace";
        public static UScriptControlCarrierMacro CreateMacroReplace( fpSetMacroScriptControlCarrier fpSet )
        {
            UScriptControlCarrierMacro ret = new UScriptControlCarrierMacro( _strMacroRemove, false, true, false,
                                                                           new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(string), "Script Name"),
                                                                                                               new UDataCarrierTypeDescription(typeof(Int32), "Script ID"),
                                                                                                               new UDataCarrierTypeDescription(typeof(List<UMacro>), "Macros List"),
                                                                                                               new UDataCarrierTypeDescription(typeof(UMacro), "Previous macro"),
                                                                                                               new UDataCarrierTypeDescription(typeof(UMacro), "Current macro")
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
