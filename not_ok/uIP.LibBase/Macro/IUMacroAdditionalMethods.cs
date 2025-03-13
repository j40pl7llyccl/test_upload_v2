using System.Xml;

namespace uIP.LibBase.Macro
{
    public interface IUMacroAdditionalMethods
    {
        // Get Cancel status
        bool CancelState();
        // Reproduce done called
        void ReproduceDoneCall( UMacro source );
        // write parameters for additional functions
        void WriteAdditionalParameters( XmlTextWriter wr );
        // read parameters for additional functions
        void ReadAdditionalParameters( XmlNode rd );
    }
}
