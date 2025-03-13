using System.Xml;

namespace uIP.Lib.Script
{
    public interface IUMacroAdditionalMethods
    {
        // GetDicKeyStrOne Cancel status
        bool CancelState();
        // Reproduce done called
        void ReproduceDoneCall( UMacro source );
        // write parameters for additional functions
        void WriteAdditionalParameters( XmlTextWriter wr );
        // read parameters for additional functions
        void ReadAdditionalParameters( XmlNode rd );
    }
}
