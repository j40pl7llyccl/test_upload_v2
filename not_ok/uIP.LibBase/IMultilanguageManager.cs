using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.LibBase
{
    //
    // Define the multi-language for a component
    //
    public class ComponentMlInfo
    {
        public string _TypeName = null;
        public string _OriLangString = null;
        public string _CurLangString = null;
        public List<string> _ListOriLangStrings = new List<string>();
        public List<string> _ListCurLangStrings = new List<string>();

        public ComponentMlInfo(string typeName, string oriLangStr)
        {
            _TypeName = string.IsNullOrEmpty(typeName) ? "" : string.Copy(typeName);
            _OriLangString = string.IsNullOrEmpty(oriLangStr) ? "" : string.Copy(oriLangStr);
        }
    }

    public class GuiComponentCollection
    {
        public string _strRootControlTypeFullName = null;
        public Dictionary<string, ComponentMlInfo> _References = null;
        public GuiComponentCollection() { }
        public GuiComponentCollection(string typeFullName, Dictionary<string, ComponentMlInfo> dic)
        {
            _strRootControlTypeFullName = typeFullName;
            _References = dic;
        }
    }

    public class ReferenceBook
    {
        public string _strGivenName = null;
        public Dictionary<string, string> _Dic = new Dictionary<string, string>();
        public ReferenceBook() { }
        public ReferenceBook(string givenName)
        {
            _strGivenName = string.IsNullOrEmpty(givenName) ? "" : string.Copy(givenName);
        }
    }

    public delegate void fpMultilanguageSwitch(IMultilanguageManager mlInst, string langCode);
    public interface IMultilanguageManager
    {
        string CurrentLanguageCode { get; }
        List<ReferenceBook> Books { get; }
        List<GuiComponentCollection> GuiBooks { get; }
        Dictionary<string, string> StringMapping { get; }
        void SwitchLanguage(string langCode);
        void InstallLanguageSwitchCallback(fpMultilanguageSwitch call);
        void RemoveLanguageSwitchCallback(fpMultilanguageSwitch call);
        void RegistryControl(Control owner, Control ctrl);
        void UnregistryControl(Control ctrl);
        bool ScanControl(Control owner, Control ctrl);
        string Get(string book, string str);
        string Get(string str);
    }
}
