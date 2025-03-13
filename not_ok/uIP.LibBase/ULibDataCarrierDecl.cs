using System.Xml;

namespace uIP.LibBase
{
    public delegate void fpUDataCarrierXMLReader( XmlNode nod );
    public delegate void fpUDataCarrierXMLWriter( XmlTextWriter wt );
    public delegate void fpUDataCarrierResHandler( UDataCarrier dat );
    public delegate void fpUDataCarrierSetResHandler( UDataCarrier[] dat );
    interface IUDataCarrierXmlIO
    {
        void WriteDatCarrXml( XmlTextWriter tw );
        void ReadDatCarrXml( XmlNode nod );
    }
}