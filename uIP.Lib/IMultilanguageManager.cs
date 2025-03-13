using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.Lib
{
    //
    // Define the multi-language for a component
    //
    public class ComponentMlInfo
    {
        public string TypeName { get; set; } = null;
        public string OriLangString { get; set; } = null;
        public string CurLangString { get; set; } = null;
        public List<string> ListOriLangStrings { get; set; } = new List<string>();
        public List<string> ListCurLangStrings { get; set; } = new List<string>();

        public ComponentMlInfo( string typeName, string oriLangStr )
        {
            TypeName = String.IsNullOrEmpty( typeName ) ? "" : String.Copy( typeName );
            OriLangString = String.IsNullOrEmpty( oriLangStr ) ? "" : String.Copy( oriLangStr );
        }
    }

    public class GuiComponentCollection
    {
        public string RootControlTypeFullName { get; set; } = null;
        public Dictionary<string, ComponentMlInfo> References { get; set; } = null;
        public GuiComponentCollection() { }
        public GuiComponentCollection( string typeFullName, Dictionary<string, ComponentMlInfo> dic )
        {
            RootControlTypeFullName = typeFullName;
            References = dic;
        }
    }

    public class ReferenceBook
    {
        public string GivenName { get; set; } = null;
        public Dictionary<string, string> Dic { get; set; } = new Dictionary<string, string>();
        public ReferenceBook() { }
        public ReferenceBook( string givenName )
        {
            GivenName = String.IsNullOrEmpty( givenName ) ? "" : String.Copy( givenName );
        }
    }

    public delegate void fpMultilanguageSwitch( IMultilanguageManager mlInst,  string langCode );
    public interface IMultilanguageManager
    {
        string CurrentLanguageCode { get; }
        List<ReferenceBook> Books { get; }
        List<GuiComponentCollection> GuiBooks { get; }
        Dictionary<string, string> StringMapping { get; }
        void SwitchLanguage( string langCode );
        void InstallLanguageSwitchCallback( fpMultilanguageSwitch call );
        void RemoveLanguageSwitchCallback( fpMultilanguageSwitch call );
        void RegistryControl( Control owner, Control ctrl );
        void UnregistryControl( Control ctrl );
        bool ScanControl( Control owner, Control ctrl );
        string Get( string book, string str );
        string Get( string str );
    }
}
