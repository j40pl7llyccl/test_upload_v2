using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public partial class UScriptService
    {
        /* 用來記錄可重用(在不同的script)的macro
         */
        private object m_hSyncReusablePool = new object();
        private List<UMacro> m_ReusableMacros = new List<UMacro>();

        #region [Reusable Macro Manage]

        private void DisposeReusablePool()
        {
            Monitor.Enter( m_hSyncReusablePool );
            try
            {
                // Clear reusable
                for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
                    m_ReusableMacros[ i ].Recycle();
                m_ReusableMacros.Clear();
            }
            finally { Monitor.Exit( m_hSyncReusablePool ); }
        }

        public UMacro NewMaccroToReusablePool( string nameOfSharpClass, string nameOfMethod, string nameOfMacro, bool isConfigVariableParam )
        {
            if ( m_bDisposed || m_ReusableMacros == null ) return null;
            if ( String.IsNullOrEmpty( nameOfSharpClass ) || String.IsNullOrEmpty( nameOfMethod ) || String.IsNullOrEmpty( nameOfMacro ) )
                return null;

            // Check if there is any same item inside list
            for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
            {
                if ( String.IsNullOrEmpty( m_ReusableMacros[ i ].GivenName ) )
                    continue;
                if ( m_ReusableMacros[ i ].GivenName == nameOfMacro )
                    return m_ReusableMacros[ i ];
            }

            if ( m_refAssemblyLoader == null ) return null;

            UDataCarrier[] immutableParams = m_refAssemblyLoader.SetupImmutableParametersFromSharpClassName( nameOfSharpClass, nameOfMethod );
            UDataCarrier[] variableParams = null;
            if ( isConfigVariableParam )
            {
                bool bRetStat = false;
                variableParams = m_refAssemblyLoader.SetupVariableParametersFromSharpClassName( nameOfSharpClass, nameOfMethod, ref bRetStat );
                if ( !bRetStat ) variableParams = null;
            }

            UMacro macro = m_refAssemblyLoader.CreateMacroFromSharpClassName( nameOfSharpClass, nameOfMethod, immutableParams, variableParams );
            if ( macro != null )
            {
                macro.ReusableMacro = true;
                macro.GivenName = nameOfMacro;
            }


            Monitor.Enter( m_hSyncReusablePool );
            try
            {
                m_ReusableMacros.Add( macro );
            }
            finally { Monitor.Exit( m_hSyncReusablePool ); }

            return macro;
        }

        public UMacro GetMacroFromReusablePool( string nameOfMacro )
        {
            if ( m_bDisposed || m_ReusableMacros == null ) return null;
            if ( String.IsNullOrEmpty( nameOfMacro ) ) return null;

            for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
            {
                if ( String.IsNullOrEmpty( m_ReusableMacros[ i ].GivenName ) ) continue;
                if ( m_ReusableMacros[ i ].GivenName == nameOfMacro )
                    return m_ReusableMacros[ i ];
            }

            return null;
        }

        private void RemoveReusableMacroFromScripts( string nameOfMacro )
        {
            if ( m_bDisposed || m_Scripts == null ) return;

            bool bTryAgain = false;
            int nNextBeg = 0;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null || m_Scripts[ i ].MacroSet == null )
                    continue;

                nNextBeg = 0;
                while ( true )
                {
                    bTryAgain = false;
                    for ( int j = nNextBeg ; j < m_Scripts[ i ].MacroSet.Count ; j++ )
                    {
                        if ( m_Scripts[ i ].MacroSet[ j ] == null )
                            continue;
                        if ( String.IsNullOrEmpty( m_Scripts[ i ].MacroSet[ j ].GivenName ) )
                            continue;
                        if ( m_Scripts[ i ].MacroSet[ j ].GivenName == nameOfMacro )
                        {
                            m_Scripts[ i ].RemoveFrom( j ); nNextBeg = j; bTryAgain = true;
                            break;
                        }
                    }
                    if ( !bTryAgain )
                        break;
                }
            }
        }

        public void ClearReusablePool()
        {
            if ( m_bDisposed || m_ReusableMacros == null ) return;

            Monitor.Enter( m_hSyncReusablePool );
            try
            {
                for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
                {
                    if ( String.IsNullOrEmpty( m_ReusableMacros[ i ].GivenName ) )
                    {
                        m_ReusableMacros[ i ].Recycle();
                        continue;
                    }
                    // Remove
                    RemoveReusableMacroFromScripts( m_ReusableMacros[ i ].GivenName );
                    // Recycle
                    m_ReusableMacros[ i ].Recycle();
                } // end for-i
                // Clear
                m_ReusableMacros.Clear();
            }
            finally { Monitor.Exit( m_hSyncReusablePool ); }
        }

        public void RemoveMacroFromReusablePool( string nameOfMacro )
        {
            if ( m_bDisposed || m_ReusableMacros == null ) return;
            if ( String.IsNullOrEmpty( nameOfMacro ) ) return;

            Monitor.Enter( m_hSyncReusablePool );
            try
            {
                for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
                {
                    if ( m_ReusableMacros[ i ] == null || String.IsNullOrEmpty( m_ReusableMacros[ i ].GivenName ) )
                        continue;
                    if ( m_ReusableMacros[ i ].GivenName == nameOfMacro )
                    {
                        // Remove from scripts too
                        RemoveReusableMacroFromScripts( nameOfMacro );
                        // Recycle
                        m_ReusableMacros[ i ].Recycle();
                        // Remove from list
                        m_ReusableMacros.RemoveAt( i );
                        break;
                    }
                }
            }
            finally { Monitor.Exit( m_hSyncReusablePool ); }
        }

        private bool LoadSettingsToReusablePool( string pathOfFolder, string nameOfSettingFile )
        {
            if ( !Directory.Exists( pathOfFolder ) ) return false;
            if ( m_bDisposed || m_ReusableMacros == null ) return false;

            // Reload from xml
            string filePath = String.Format( @"{0}\{1}", pathOfFolder, nameOfSettingFile );
            if ( !File.Exists( filePath ) ) return false;

            XmlDocument doc = new XmlDocument();
            doc.Load( filePath );

            XmlNodeList methods = doc.SelectNodes( String.Format( "//{0}/{1}", _strNameOfReusableMacroSettingsElement, _strNameOfReusableMacroMethodInfoElement ) );
            if ( methods != null )
            {
                for ( int i = 0 ; i < methods.Count ; i++ )
                {
                    if ( methods[ i ] == null ) continue;

                    string givenNm = null;
                    string classNm = null;
                    string methodNm = null;
                    string xmlNm = null;

                    XmlNode node;
                    // given name
                    node = methods[ i ].SelectSingleNode( _strNameOfMacroGivenNameElement );
                    if ( node != null && !String.IsNullOrEmpty( node.InnerText ) )
                        givenNm = String.Copy( node.InnerText );
                    // class name
                    node = methods[ i ].SelectSingleNode( _strNameOfMacroMethodInPluginClassSharpNameElement );
                    if ( node != null && !String.IsNullOrEmpty( node.InnerText ) )
                        classNm = String.Copy( node.InnerText );
                    // method name
                    node = methods[ i ].SelectSingleNode( _strNameOfMacroMethodNameElement );
                    if ( node != null && !String.IsNullOrEmpty( node.InnerText ) )
                        methodNm = String.Copy( node.InnerText );
                    // xml file
                    node = methods[ i ].SelectSingleNode( _strFilenameOfMacroSettingsElement );
                    if ( node != null && !String.IsNullOrEmpty( node.InnerText ) )
                        xmlNm = String.Copy( node.InnerText );

                    if ( String.IsNullOrEmpty( givenNm ) || String.IsNullOrEmpty( classNm ) ||
                         String.IsNullOrEmpty( methodNm ) || String.IsNullOrEmpty( xmlNm ) )
                        continue;

                    // get class instance
                    UMacroMethodProviderPlugin plugin = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( classNm );
                    if ( plugin == null )
                        continue;

                    // Do reloading
                    UMacro macro = null;
                    if ( !plugin.ReadMacroSettings( ref macro, pathOfFolder, xmlNm ) || macro == null )
                        continue;

                    // Replace
                    for ( int j = 0 ; j < m_Scripts.Count ; j++ )
                    {
                        if ( m_Scripts[ i ] == null ) continue;
                        m_Scripts[ i ].Replace( macro, givenNm );
                    }

                    // Remove old one
                    RemoveMacroFromReusablePool( givenNm );

                    // Add
                    m_ReusableMacros.Add( macro );
                }
            }
            return true;
        }

        private bool SaveSettingsFromReusablePool( string folderPath, string xmlFileNm )
        {
            if ( !Directory.Exists( folderPath ) ) return false;
            if ( m_bDisposed || m_ReusableMacros == null || m_ReusableMacros.Count <= 0 ) return false;

            string wrFilePath = String.Format( @"{0}\{1}", folderPath, xmlFileNm );

            using ( FileStream ws = new FileStream( wrFilePath, FileMode.Create ) )
            {
                XmlTextWriter xmlwr = new XmlTextWriter( ws, Encoding.UTF8 );
                xmlwr.Formatting = Formatting.Indented;

                xmlwr.WriteStartDocument();
                xmlwr.WriteStartElement( _strNameOfReusableMacroSettingsElement );

                for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
                {
                    UMacroMethodProviderPlugin bsc = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( m_ReusableMacros[ i ].OwnerClassName );
                    if ( bsc == null ) continue;

                    xmlwr.WriteStartElement( _strNameOfReusableMacroMethodInfoElement );

                    xmlwr.WriteElementString( _strNameOfMacroGivenNameElement, m_ReusableMacros[ i ].GivenName );
                    xmlwr.WriteElementString( _strNameOfMacroMethodInPluginClassSharpNameElement, m_ReusableMacros[ i ].OwnerClassName );
                    xmlwr.WriteElementString( _strNameOfMacroMethodNameElement, m_ReusableMacros[ i ].MethodName );
                    string fileNm = String.Format( "{0}.xml", i );
                    xmlwr.WriteElementString( _strFilenameOfMacroSettingsElement, fileNm );

                    xmlwr.WriteEndElement();

                    bsc.WriteMacroSettings( m_ReusableMacros[ i ], folderPath, fileNm );
                }

                xmlwr.WriteEndElement();
                xmlwr.WriteEndDocument();
                xmlwr.Flush();
                xmlwr.Close();
            }
            return true;
        }

        public UDataCarrier[] GetReusableMacroControl( string givenNameOfMacro, string cmd, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( m_bDisposed || m_ReusableMacros == null || String.IsNullOrEmpty( givenNameOfMacro ) )
                return null;

            for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
            {
                if ( m_ReusableMacros[ i ] == null || String.IsNullOrEmpty( m_ReusableMacros[ i ].GivenName ) )
                    continue;
                if ( m_ReusableMacros[ i ].GivenName == givenNameOfMacro )
                    return m_ReusableMacros[ i ].OwnerOfPluginClass == null ?
                           null :
                           m_ReusableMacros[ i ].OwnerOfPluginClass.GetMacroControl( m_ReusableMacros[ i ], UDataCarrier.MakeOne<string>( cmd ), ref bRetStatus );
            }

            return null;
        }
        public bool SetReusableMacroControl( string givenNameOfMacro, string cmd, UDataCarrier[] data )
        {
            bool ret = false;
            if ( m_bDisposed || m_ReusableMacros == null || String.IsNullOrEmpty( givenNameOfMacro ) )
                return ret;

            for ( int i = 0 ; i < m_ReusableMacros.Count ; i++ )
            {
                if ( m_ReusableMacros[ i ] == null || String.IsNullOrEmpty( m_ReusableMacros[ i ].GivenName ) )
                    continue;
                if ( m_ReusableMacros[ i ].GivenName == givenNameOfMacro )
                    return m_ReusableMacros[ i ].OwnerOfPluginClass == null ?
                           false :
                           m_ReusableMacros[ i ].OwnerOfPluginClass.SetMacroControl( m_ReusableMacros[ i ], UDataCarrier.MakeOne<string>( cmd ), data );
            }

            return ret;
        }

        #endregion
    }
}
