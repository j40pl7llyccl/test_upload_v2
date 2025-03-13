using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public class UPluginAssemblyCmdList
    {
        private readonly string _strGivenNameOfPlugin;
        private readonly string _strClassFullNameOfPluginCSharp;
        private readonly UScriptControlCarrier[] _listCmds;

        public string GivenName {  get { return _strGivenNameOfPlugin; } }
        public string ClassFullName {  get { return _strClassFullNameOfPluginCSharp; } }
        public UScriptControlCarrier[] CmdsList {  get { return _listCmds; } }

        public UPluginAssemblyCmdList( UMacroMethodProviderPlugin plugin, bool bIsClassCmd )
        {
            if ( plugin == null ) return;

            _strGivenNameOfPlugin = String.IsNullOrEmpty( plugin.GivenName ) ? null : String.Copy( plugin.GivenName );
            _strClassFullNameOfPluginCSharp = String.IsNullOrEmpty( plugin.NameOfCSharpDefClass ) ? null : String.Copy( plugin.NameOfCSharpDefClass );
            if ( bIsClassCmd && plugin.PluginClassControlList != null )
                _listCmds = UScriptControlCarrierPluginClass.CloneDescs( plugin.PluginClassControlList );
            else if ( !bIsClassCmd && plugin.MacroControlList != null )
                _listCmds = UScriptControlCarrierMacro.CloneDescs(plugin.MacroControlList);
        }
        public UPluginAssemblyCmdList(string givenName, string classFullName, List<UScriptControlCarrierPluginClass> input)
        {
            if ( input == null ) return;

            _strGivenNameOfPlugin = String.IsNullOrEmpty( givenName ) ? null : String.Copy( givenName );
            _strClassFullNameOfPluginCSharp = String.IsNullOrEmpty( classFullName ) ? null : String.Copy( classFullName );
            _listCmds = UScriptControlCarrierPluginClass.CloneDescs( input );
        }
        public UPluginAssemblyCmdList( string givenName, string classFullName, UScriptControlCarrierPluginClass[] input )
        {
            if ( input == null ) return;

            _strGivenNameOfPlugin = String.IsNullOrEmpty( givenName ) ? null : String.Copy( givenName );
            _strClassFullNameOfPluginCSharp = String.IsNullOrEmpty( classFullName ) ? null : String.Copy( classFullName );
            _listCmds = UScriptControlCarrierPluginClass.CloneDescs( input );
        }
        public UPluginAssemblyCmdList(string givenName, string classFullName, List<UScriptControlCarrierMacro> input)
        {
            if ( input == null ) return;

            _strGivenNameOfPlugin = String.IsNullOrEmpty( givenName ) ? null : String.Copy( givenName );
            _strClassFullNameOfPluginCSharp = String.IsNullOrEmpty( classFullName ) ? null : String.Copy( classFullName );
            _listCmds = UScriptControlCarrierMacro.CloneDescs( input );
        }
        public UPluginAssemblyCmdList( string givenName, string classFullName, UScriptControlCarrierMacro[] input )
        {
            if ( input == null ) return;

            _strGivenNameOfPlugin = String.IsNullOrEmpty( givenName ) ? null : String.Copy( givenName );
            _strClassFullNameOfPluginCSharp = String.IsNullOrEmpty( classFullName ) ? null : String.Copy( classFullName );
            _listCmds = UScriptControlCarrierMacro.CloneDescs( input );
        }
    }
}
