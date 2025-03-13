using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib.Script;

namespace uIP.MacroProvider.Commons.FlowControl
{
    public class JumpSameScriptParams
    {
        public enum TypeOfJump : int
        {
            JumpToIndex,
            JumpToScript,
            JumpByCallRetIndex,
            JumpByCallRetScriptName,
        }

        public TypeOfJump JumpType { get; set; } = TypeOfJump.JumpToIndex;
        public int WhichIndex { get; set; } = 0;
        public string WhichScriptToJump { get; set; } = "";
        public string CallWhichPluginFullName { get; set; }
        public string CallWhichPluginOfFunc { get; set; }

        public static void ConfigMacroJump(UMacroCapableOfCtrlFlow m, JumpSameScriptParams p)
        {
            if ( m == null || p == null )
                return;

            m.AbilityToJumpAnotherScript = false;
            m.AbilityToJumpIndex = false;
            if ( p.JumpType == TypeOfJump.JumpToIndex || p.JumpType == TypeOfJump.JumpByCallRetIndex )
            {
                //if (p.JumpType == TypeOfJump.JumpToIndex)
                //{
                //    m.MustJump = p.JumpType == TypeOfJump.JumpToIndex && p.WhichIndex > 0;
                //    m.Jump2WhichMacro = p.WhichIndex;
                //}
                m.AbilityToJumpIndex = true;
            }
            else if ( p.JumpType == TypeOfJump.JumpToScript || p.JumpType == TypeOfJump.JumpByCallRetScriptName )
                m.AbilityToJumpAnotherScript = true;

        }
    }
}
