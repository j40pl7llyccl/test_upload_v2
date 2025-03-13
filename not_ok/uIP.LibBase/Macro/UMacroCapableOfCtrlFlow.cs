using System;
using System.Xml;

namespace uIP.LibBase.Macro
{
    public class UMacroCapableOfCtrlFlow : UMacro
    {
        // runtime change
        protected bool m_bJump = false;
        protected Int32 m_nJump2WhichMacro = ( int ) MacroGotoFunctions.GOTO_INVALID;

        // only available in last macro
        protected MacroJumpingFunctions m_IterationFunc = MacroJumpingFunctions.JUMPING_NA;
        protected AnotherMacroJumpingInfo m_IterationConf = null;

        public bool MustJump { get { return m_bJump; } set { m_bJump = value; } }
        public Int32 Jump2WhichMacro { get { return m_nJump2WhichMacro; } set { m_nJump2WhichMacro = value; } }

        public MacroJumpingFunctions Iteration { get { return m_IterationFunc; } set { m_IterationFunc = value; } }
        public AnotherMacroJumpingInfo IteratorData { get { return m_IterationConf; } set { m_IterationConf = value; } }

        public UMacroCapableOfCtrlFlow( UMacroMethodProviderPlugin owner, string nameOfOwnerSharpClass, 
                                        string methodName, fpMacroExecHandler methodHandler,
                                        UDataCarrierTypeDescription[] typeDescOfImmuParam, UDataCarrierTypeDescription[] typeDescOfVarParam,
                                        UDataCarrierTypeDescription[] typeDescOfPrevPropag, UDataCarrierTypeDescription[] typeDescOfRtnPropag )
            : base( owner, nameOfOwnerSharpClass, methodName, methodHandler, typeDescOfImmuParam, typeDescOfVarParam, typeDescOfPrevPropag, typeDescOfRtnPropag )
        {
        }

        public UMacroCapableOfCtrlFlow( UMacroMethodProviderPlugin owner, string nameOfOwnerSharpClass, 
                                        string methodName, fpMacroExecHandler methodHandler,
                                        UDataCarrierTypeDescription[] typeDescOfImmuParam, UDataCarrierTypeDescription[] typeDescOfVarParam,
                                        UDataCarrierTypeDescription[] typeDescOfPrevPropag, UDataCarrierTypeDescription[] typeDescOfRtnPropag, UDataCarrierTypeDescription[] typeDescOfRtnResult )
            : base( owner, nameOfOwnerSharpClass, methodName, methodHandler, typeDescOfImmuParam, typeDescOfVarParam, typeDescOfPrevPropag, typeDescOfRtnPropag, typeDescOfRtnResult )
        {
        }

        public override void ReproduceDoneCall( UMacro source )
        {
            base.ReproduceDoneCall( source );
            // any other things must be done? Update from here~~~
        }

        public override void WriteAdditionalParameters( XmlTextWriter wr )
        {
            base.WriteAdditionalParameters( wr );
            if ( wr == null ) return;

            // any other parameters must be saved? Write here~~~
        }

        public override void ReadAdditionalParameters( XmlNode rd )
        {
            base.ReadAdditionalParameters( rd );
            if ( rd == null ) return;

            // any other parameters must be read? Write here~~~
        }
    }
}
