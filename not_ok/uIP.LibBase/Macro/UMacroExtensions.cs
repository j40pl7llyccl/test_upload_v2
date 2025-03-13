using System;
using System.Text;
using System.Threading;
using System.Xml;

namespace uIP.LibBase.Macro
{
    public class UMacroExtensions : UMacro
    {
        // mean call the macro's open method with type "fpMacroExecHandler", the 1st parameter id
        // will be replace to the manager reference
        // 1. access to Lua instance
        // 2. access to AppDomain

        // mark the macro have the ability to call other script
        // 1. when implement the UMacroMethodProviderPlugin, it must have the ability to delay loading in parameter reading
        //   - thru macro GET/ SET interface
        protected bool m_bCapableOfExecScript = false;

        // mark this macro will connect previous and behind ones' carrying data structure converting
        protected bool m_bCapableOfLinkBtnMacros = false;

        public bool CapableOfExecScript
        {
            get { return m_bCapableOfExecScript; }
            set { Monitor.Enter( m_hSync ); try { m_bCapableOfExecScript = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public bool CapableOfLinkBtnMacros
        {
            get { return m_bCapableOfLinkBtnMacros; }
            set { Monitor.Enter( m_hSync ); try { m_bCapableOfLinkBtnMacros = value; } finally { Monitor.Exit( m_hSync ); } }
        }

        public UMacroExtensions( UMacroMethodProviderPlugin owner, string nameOfOwnerSharpClass, 
                                 string methodName, fpMacroExecHandler methodHandler,
                                 UDataCarrierTypeDescription[] typeDescOfImmuParam, UDataCarrierTypeDescription[] typeDescOfVarParam,
                                 UDataCarrierTypeDescription[] typeDescOfPrevPropag, UDataCarrierTypeDescription[] typeDescOfRtnPropag )
            : base( owner, nameOfOwnerSharpClass, methodName, methodHandler, typeDescOfImmuParam, typeDescOfVarParam, typeDescOfPrevPropag, typeDescOfRtnPropag )
        {
        }

        public UMacroExtensions( UMacroMethodProviderPlugin owner, string nameOfOwnerSharpClass, 
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
