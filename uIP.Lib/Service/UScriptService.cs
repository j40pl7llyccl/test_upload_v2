using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.Lib.Service
{

    // Multi-thread calling this class is denied.
    public partial class UScriptService : IDisposable
    {

        private bool m_bDisposed = false;
        // cannot re-new the this, otherwise the FlowControl's information must replace to this class
        private List<UScript> m_Scripts = new List<UScript>();
        private string m_strTmpParametersRWPath = null;
        private UPluginAssemblyService m_refAssemblyLoader = null;

        public string TmpParamPath
        {
            get { return m_strTmpParametersRWPath; }
            set
            {
                if ( String.IsNullOrEmpty( value ) || !Directory.Exists( value ) )
                {
                    m_strTmpParametersRWPath = null;
                    return;
                }

                m_strTmpParametersRWPath = CommonUtilities.RemoveEndDirSymbol( value );
            }
        }

        public UPluginAssemblyService Loader => m_refAssemblyLoader;
        public List<UScript> Scripts => m_Scripts;
        public List<UMacro> ReusableMacros => m_ReusableMacros;
        public string ClassName => this.GetType().FullName;

        private object m_sync = new object();
        private bool m_bInConf = false;

        public bool IsConfig
        {
            get
            {
                bool repo = false;
                Monitor.Enter( m_sync );
                repo = m_bInConf;
                Monitor.Exit( m_sync );
                return repo;
            }
            set
            {
                Monitor.Enter( m_sync );
                m_bInConf = value;
                Monitor.Exit( m_sync );
            }
        }

        public UScriptService( UPluginAssemblyService loader, string parametersTmpRWFolder )
        {
            m_refAssemblyLoader = loader;

            CommonUtilities.RCreateDir( parametersTmpRWFolder );
            TmpParamPath = parametersTmpRWFolder;

            InitialCmdControl();
        }

        public void Dispose()
        {
            if ( m_bDisposed ) return;
            m_bDisposed = true;

            if ( _controlMacroConfig  != null) {
                _controlMacroConfig.Dispose();
                _controlMacroConfig = null;
            }

            if (Directory.Exists(m_strTmpParametersRWPath)) {
                try { Directory.Delete( m_strTmpParametersRWPath, true ); } catch { }
            }

            // Clear scripts
            for ( int i = 0; i < m_Scripts.Count; i++ )
            {
                m_Scripts[ i ].Dispose();
            }

            // clear reusable pool
            DisposeReusablePool();
        }

        /// <summary>
        /// Create a new script
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool NewScript( string name )
        {
            if ( m_bDisposed ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].NameOfId == name ) return true;
            }

            UScript script = new UScript( name, this );
            if ( script == null ) return false;

            // assign log delegate function
            if ( m_refAssemblyLoader != null ) script.fpLog = m_refAssemblyLoader.fpLog;

            m_Scripts.Add( script );
            return true;
        }

        /// <summary>
        /// Create a new script
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool NewScript( int id )
        {
            if ( m_bDisposed ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].SnOfId == id ) return true;
            }

            UScript script = new UScript( id, this );
            if ( script == null ) return false;

            // assign log delegate function
            if ( m_refAssemblyLoader != null ) script.fpLog = m_refAssemblyLoader.fpLog;

            m_Scripts.Add( script );
            return true;
        }

        /// <summary>
        /// GetDicKeyStrOne script by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UScript GetScript( string name )
        {
            if ( m_bDisposed ) return null;

            UScript ret = null;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ) continue;
                if ( m_Scripts[ i ].NameOfId == name )
                {
                    ret = m_Scripts[ i ]; break;
                }
            }

            return ret;
        }

        public List< UScript > GetScript( string[] including, string[] excluding )
        {
            if (m_bDisposed) return new List< UScript >();
            List<UScript> repo = new List< UScript >();
            Monitor.Enter( m_sync );
            if ( including == null || including.Length <= 0 )
                repo = m_Scripts.ToArray().ToList();
            else
            {
                foreach ( var s in m_Scripts )
                {
                    if (string.IsNullOrEmpty(s.NameOfId)) continue;
                    bool ok = false;
                    foreach ( var inc in including )
                    {
                        if ( s.NameOfId.Contains( inc ) )
                        {
                            ok = true;
                            break;
                        }
                    }
                    if (ok) repo.Add( s );
                }
            }

            if ( excluding != null && excluding.Length > 0 )
            {
                List<UScript> rmv = new List< UScript >();
                foreach ( var r in repo )
                {
                    if (string.IsNullOrEmpty(r.NameOfId)) continue;
                    foreach ( var exc in excluding )
                    {
                        if ( r.NameOfId.Contains( exc ) )
                        {
                            rmv.Add( r );
                            break;
                        }
                    }
                }

                foreach ( var r in rmv )
                {
                    repo.Remove( r );
                }
            }

            Monitor.Exit( m_sync );

            return repo;
        }

        public bool Rename( string src, string dst )
        {
            if ( m_bDisposed || string.IsNullOrEmpty( dst ) ) return false;
            if ( src == dst ) return false;
            if ( GetScript( dst ) != null ) return false; // exist

            UScript toChange = GetScript( src );
            if ( toChange == null ) return false;

            if ( !toChange.SynchronizedObject.WaitOne( 0 ) )
                return false; // in used

            toChange.NameOfId = dst;

            toChange.SynchronizedObject.Release();
            return true;
        }

        public UScript[] GetScript( string partialName, Int32 cmpIndex, char[] splitSymbols )
        {
            if ( m_bDisposed ) return null;
            if ( cmpIndex < 0 ) return null;
            if ( String.IsNullOrEmpty( partialName ) ) return null;

            List<UScript> ret = new List<UScript>();

            splitSymbols = splitSymbols == null || splitSymbols.Length <= 0 ? new char[] { '_', '-' } : splitSymbols;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) break;
                if ( String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ) break;

                string[] nms = m_Scripts[ i ].NameOfId.Split( splitSymbols );

                if ( cmpIndex >= nms.Length ) continue;

                if ( nms[ cmpIndex ] == partialName )
                    ret.Add( m_Scripts[ i ] );
            }

            return ret.Count <= 0 ? null : ret.ToArray();
        }

        /// <summary>
        /// GetDicKeyStrOne script by SN
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UScript GetScript( int id )
        {
            if ( m_bDisposed ) return null;

            UScript ret = null;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].SnOfId == id )
                {
                    ret = m_Scripts[ i ]; break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Clear all scripts
        /// </summary>
        public void ClearAllScripts()
        {
            if ( m_bDisposed ) return;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                m_Scripts[ i ].Dispose();
                m_Scripts[ i ] = null;
            }
            m_Scripts.Clear();
        }

        #region [GET, SET: variable parameters macro of script]

        public UDataCarrier[] GetMacroVariableParameters( string nameOfScrip, int indexOfMacro, out bool bCallStatus )
        {
            bCallStatus = false;

            UScript script = GetScript( nameOfScrip );
            if ( script == null ) return null;

            UMacro macro = script.GetByIndex( indexOfMacro );
            if ( macro == null ) return null;

            bCallStatus = true;

            return macro.ParameterCarrierVariable;
        }

        public bool SetMacroVariableParameters( string nameOfScrip, int indexOfMacro, UDataCarrier[] data )
        {
            UScript script = GetScript( nameOfScrip );
            if ( script == null ) return false;

            UMacro maccro = script.GetByIndex( indexOfMacro );
            if ( maccro == null ) return false;

            if ( !UDataCarrier.TypesCheck( data, maccro.VariableParamTypeDesc ) )
                return false;

            maccro.ParameterCarrierVariable = data;
            return true;
        }

        public UDataCarrierTypeDescription[] GetMacroVariableParametersDesc( string nameOfScrip, int indexOfMacro, out bool bCallStatus )
        {
            bCallStatus = false;

            UScript script = GetScript( nameOfScrip );
            if ( script == null ) return null;

            UMacro macro = script.GetByIndex( indexOfMacro );
            if ( macro == null ) return null;

            bCallStatus = true;

            return macro.VariableParamTypeDesc;
        }

        #endregion

        /// <summary>
        /// Delete a script by name
        /// </summary>
        /// <param name="nameOfScript"></param>
        public void DeleteScript( string nameOfScript )
        {
            if ( m_bDisposed || String.IsNullOrEmpty( nameOfScript ) ) return;

            UScript script = null;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ) continue;
                if ( m_Scripts[ i ].NameOfId == nameOfScript )
                {
                    // Record
                    script = m_Scripts[ i ];
                    // Remove from list
                    m_Scripts.RemoveAt( i );
                    // Dispose it
                    script.Dispose();
                    break;
                }
            }
        }

        /// <summary>
        /// Delete a script by SN
        /// </summary>
        /// <param name="snOfScript"></param>
        public void DeleteScript( int snOfScript )
        {
            if ( m_bDisposed ) return;

            UScript script = null;
            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].SnOfId == snOfScript )
                {
                    // Record
                    script = m_Scripts[ i ];
                    // Remove from list
                    m_Scripts.RemoveAt( i );
                    // Dispose it
                    script.Dispose();
                    break;
                }
            }
        }

        #region [Normal Macro Manage]
        /// <summary>
        /// Add a macro into a script by name of script
        /// </summary>
        /// <param name="nameOfScript"></param>
        /// <param name="macro"></param>
        /// <returns></returns>
        public bool AddMacro( string nameOfScript, UMacro macro )
        {
            if ( m_bDisposed || macro == null ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ) continue;
                if ( m_Scripts[ i ].NameOfId == nameOfScript )
                    return m_Scripts[ i ].Add( macro );
            }

            return false;
        }

        /// <summary>
        /// Add a macro into a script by SN of script
        /// </summary>
        /// <param name="snOfScript"></param>
        /// <param name="macro"></param>
        /// <returns></returns>
        public bool AddMacro( int snOfScript, UMacro macro )
        {
            if ( m_bDisposed || macro == null ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].SnOfId == snOfScript )
                    return m_Scripts[ i ].Add( macro );
            }

            return false;
        }

        /// <summary>
        /// Insert before a macro in a spcified index of script by name of script 
        /// </summary>
        /// <param name="nameOfScript">name of script</param>
        /// <param name="macro">macro to be added</param>
        /// <param name="index_0base">index to be inserted before</param>
        /// <returns>true:ok, false: not insert</returns>
        public bool InsertMacro( string nameOfScript, UMacro macro, int index_0base )
        {
            if ( m_bDisposed || macro == null ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ) continue;
                if ( m_Scripts[ i ].NameOfId == nameOfScript )
                    return m_Scripts[ i ].InsertBeforeIndex( macro, index_0base );
            }

            return false;
        }

        /// <summary>
        /// Insert before a macro in a spcified index of script by SN of script 
        /// </summary>
        /// <param name="snOfScript">name of script</param>
        /// <param name="macro">macro to be added</param>
        /// <param name="index_0base">index to be inserted before</param>
        /// <returns>true:ok, false: not insert</returns>
        public bool InsertMacro( int snOfScript, UMacro macro, int index_0base )
        {
            if ( m_bDisposed || macro == null ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].SnOfId == snOfScript )
                    return m_Scripts[ i ].InsertBeforeIndex( macro, index_0base );
            }

            return false;
        }

        #endregion


        /// <summary>
        /// Remove a macro from a script by name of script
        /// </summary>
        /// <param name="nameOfScript">name of script</param>
        /// <param name="macroIndex0OfScript">index of macro</param>
        /// <returns></returns>
        public bool RemoveMacroFromScript( string nameOfScript, int macroIndex0OfScript )
        {
            if ( m_bDisposed || String.IsNullOrEmpty( nameOfScript ) ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( String.IsNullOrEmpty( m_Scripts[ i ].NameOfId ) ) continue;
                if ( m_Scripts[ i ].NameOfId == nameOfScript )
                {
                    return (m_Scripts[ i ].RemoveFrom( macroIndex0OfScript ));
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a macro from a script by SN of script
        /// </summary>
        /// <param name="snOfScript">SN of script</param>
        /// <param name="macroIndex0OfScript">index of macro</param>
        /// <returns></returns>
        public bool RemoveMacroFromScript( int snOfScript, int macroIndex0OfScript )
        {
            if ( m_bDisposed ) return false;

            for ( int i = 0 ; i < m_Scripts.Count ; i++ )
            {
                if ( m_Scripts[ i ] == null ) continue;
                if ( m_Scripts[ i ].SnOfId == snOfScript )
                {
                    return (m_Scripts[ i ].RemoveFrom( macroIndex0OfScript ));
                }
            }

            return false;
        }

        public bool ReproduceScript( string srcNameOfScript, string newNameOfScript )
        {
            if ( m_bDisposed ) return false;
            if ( String.IsNullOrEmpty( srcNameOfScript ) || String.IsNullOrEmpty( newNameOfScript ) )
                return false;
            // check existing?
            UScript script = GetScript( newNameOfScript );
            if ( script != null ) return false;
            // get current
            script = GetScript( srcNameOfScript );
            if ( script == null ) return false;

            // create a new one
            if ( !NewScript( newNameOfScript ) ) return false;

            // reproduce
            for ( int m = 0 ; m < script.MacroSet.Count ; m++ )
            {
                bool bErr = false;
                UMacro macro = UMacro.Reproduce( script.MacroSet[ m ] );
                if ( macro != null )
                {
                    if ( !AddMacro( newNameOfScript, macro ) )
                    {
                        macro.Recycle();
                        macro = null;
                        bErr = true;
                    }
                }
                else
                    bErr = true;
                if ( bErr )
                {
                    DeleteScript( newNameOfScript );
                    return false;
                }
            }

            return true;
        }


        public Assembly GetOwnerOfMacroMethodOfPluginAssembly( string nameOfScript, int index0OfMacro )
        {
            UScript script = GetScript( nameOfScript );
            if ( script == null ) return null;
            UMacro info = script.GetByIndex( index0OfMacro );
            return (info == null ? null : info.GetAssemblyOwner());
        }

        public Assembly GetOwnerOfMacroMethodOfPluginAssembly( int snOfScript, int index0OfMacro )
        {
            UScript script = GetScript( snOfScript );
            if ( script == null ) return null;
            UMacro info = script.GetByIndex( index0OfMacro );
            return (info == null ? null : info.GetAssemblyOwner());
        }


        #region Exec Flow Control

        /// <summary>
        /// Exec flow control script, goto script in scriptSet
        /// </summary>
        /// <param name="bLogMessage">log message or not</param>
        /// <param name="cancel">instance to cancel running script</param>
        /// <param name="beginScript">from this script to exec</param>
        /// <param name="scriptSet">where the script set be accessed</param>
        /// <param name="nSyncTimeout">Timeout ms to sync</param>
        /// <param name="retResultCarrier">report all executing results and need to be handled by caller.</param>
        /// <returns>true: ok, false: NG</returns>
        public static bool ScriptRunFlowControl( bool bLogMessage, CancelExecScript cancel, UScript beginScript, List<UScript> scriptSet, int nSyncTimeout, out List<UScriptHistoryCarrier> retResultCarrier )
        {
            return UScript.RunningControlFlow( bLogMessage, cancel, scriptSet, beginScript, true, nSyncTimeout, true, out retResultCarrier );
        }

        #endregion
    }
}
