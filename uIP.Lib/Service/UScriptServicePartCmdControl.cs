 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib.Script;

namespace uIP.Lib.Service
{
    public enum eMacroEvtNameOfEventControl
    {
        All,
        OnBeginWaitEvent,
        OnBeginTrigEvent,
        OnEndWaitEvent,
        OnEndTrigEvent,
    }

    public partial class UScriptService
    {
        private List<UScriptControlCarrierMacro> m_GlobalControls = new List<UScriptControlCarrierMacro>();
        public List<UScriptControlCarrierMacro> ScriptServiceControls { get { return m_GlobalControls; } }

        private UScriptControlCarrierMacro m_GControlResetMacroExecEvent = null;
        private UScriptControlCarrierMacro m_GControlChangeOnConfMacro = null;

        /* 當前所切換到的 script 名稱與macro在script的index
         */
        private string m_strCurrNameOfOnConfScript = null;
        private Int32 m_nCurrIndexOfOnConMacro = -1;

        private void InitialCmdControl()
        {
            // control cmds
            m_GlobalControls.Add( new UScriptControlCarrierMacro( "MacroMethod", true, false, false,
                                                                  new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( string ), "Plugin Class C# Type Name" ),
                                                                                                      new UDataCarrierTypeDescription( typeof( string ), "Plugin Class Method name" ) },
                                                                  GetControl_MacroMethod,
                                                                  null ) );
            m_GlobalControls.Add( new UScriptControlCarrierMacro( "MacroExtractingResultToPack", true, true, false,
                                                                  new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( bool ), "Enable \"ExtractingResultToPack\"" ) },
                                                                  GetControl_MacroExtractingResultToPack,
                                                                  SetControl_MacroExtractingResultToPack ) );
            m_GlobalControls.Add( new UScriptControlCarrierMacro( "RuntimeLoadAssembly", false, true, false,
                                                                  new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( string ), "Assembly absolute path" ) },
                                                                  null,
                                                                  SetControl_RuntimeLoadAssembly ) );
            m_GlobalControls.Add( new UScriptControlCarrierMacro( "EnableScriptLog", false, true, false,
                                                                  new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription( typeof( string ), "Name of Script" ),
                                                                                                      new UDataCarrierTypeDescription( typeof( bool ), "Enable?" ) },
                                                                  null,
                                                                  SetControl_EnableScriptLog ) );

            m_GControlResetMacroExecEvent = new UScriptControlCarrierMacro( "ResetMacroExecEvent", false, true, false,
                                                                            new UDataCarrierTypeDescription[] {
                                                                                new UDataCarrierTypeDescription( typeof( string ), "Name Of Script" ),
                                                                                new UDataCarrierTypeDescription( typeof( Int32 ), "Index Of Macro" ),
                                                                                new UDataCarrierTypeDescription( typeof( string ), "Evt given name: " + String.Join( ",", Enum.GetNames( typeof( eMacroEvtNameOfEventControl ) ))),
                                                                                new UDataCarrierTypeDescription( typeof(int), "Timeout Ms" ) },
                                                                            null,
                                                                            new fpSetMacroScriptControlCarrier( SetControl_ResetMacroExecEvent ) );
            m_GlobalControls.Add( m_GControlResetMacroExecEvent );

            m_GControlChangeOnConfMacro = new UScriptControlCarrierMacro( "ChangeOnConfMacro", false, true, false,
                                                                          new UDataCarrierTypeDescription[] {
                                                                              new UDataCarrierTypeDescription( typeof( string ), "Name Of Script" ),
                                                                              new UDataCarrierTypeDescription( typeof( Int32 ), "Index Of Macro" ) },
                                                                          null,
                                                                          new fpSetMacroScriptControlCarrier( SetControl_ChangeOnConfMacro ) );
            m_GlobalControls.Add( m_GControlChangeOnConfMacro );


        }

        #region [GET, SET script]

        /// <summary>
        /// GetDicKeyStrOne macro control from a macro in a script
        /// </summary>
        /// <param name="nameOfScript">name of script</param>
        /// <param name="indexOfMacro">index of macro</param>
        /// <param name="nameOfCmd">cmd name</param>
        /// <param name="bRetStatus">return the status of this call</param>
        public UDataCarrier[] GetMacroControl( string nameOfScript, int indexOfMacro, string nameOfCmd, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( String.IsNullOrEmpty( nameOfCmd ) )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET script = {0}, index of macro = {1} with empty cmd.", nameOfScript, indexOfMacro ) );
                return null;
            }

            UDataCarrier[] ret = null;

            UScript script = null;
            UMacro macro = null;
            // check internal first
            if ( CheckMacroControlsExist( nameOfCmd, nameOfScript, indexOfMacro, out script, out macro ) )
            {
                ret = GetThisClassControl( macro, UDataCarrier.MakeOne<string>( nameOfCmd ), ref bRetStatus );
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET GLOBAL script = {0}, index of macro = {1}, using cmd name = {2} with status {3}.", nameOfScript, indexOfMacro, nameOfCmd, bRetStatus ) );
                return ret;
            }

            if ( script == null )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find script\".", nameOfScript, indexOfMacro, nameOfCmd ) );
                return null;
            }

            if ( macro == null )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find macro\".", nameOfScript, indexOfMacro, nameOfCmd ) );
                return null;
            }

            // otherwise, do normal get
            UMacroMethodProviderPlugin plugin = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( macro.OwnerClassName );
            if ( plugin == null )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find plugin class name\".", nameOfScript, indexOfMacro, nameOfCmd ) );
                return null;
            }

            try
            {
                ret = plugin.GetMacroControl( macro, UDataCarrier.MakeOne<string>( nameOfCmd ), ref bRetStatus );
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET script = {0}, index of macro = {1}, using cmd name = {2} with status {3}.", nameOfScript, indexOfMacro, nameOfCmd, bRetStatus ) );
            }
            catch ( Exception exp )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call GET script = {0}, index of macro = {1}, using cmd name = {2}\n[exception]\n{3}.", nameOfScript, indexOfMacro, nameOfCmd, exp.ToString() ) );
                ret = null;
            }

            return ret;
        }

        /// <summary>
        /// Call script to ioctl get regardless call fail
        /// </summary>
        /// <param name="nameOfScript">script to operate get</param>
        /// <param name="nameOfCmd">ioctl get command name</param>
        /// <returns>each result of macro</returns>
        public List<UDataCarrier[]> GetMacroControl( string nameOfScript, string nameOfCmd )
        {
            UScript s = GetScript( nameOfScript );
            if (s == null) return new List< UDataCarrier[] >();

            List<UDataCarrier[]> repo = new List< UDataCarrier[] >();
            foreach ( var m in s.MacroSet )
            {
                if ( m == null )
                {
                    repo.Add( null );
                    continue;
                }

                bool callStat = false;
                UDataCarrier[] got = m.OwnerOfPluginClass.GetMacroControl( m,
                    new UDataCarrier() { Data = nameOfCmd, Tp = typeof( string ) }, ref callStat );
                repo.Add( got );
            }

            return repo;
        }

        /// <summary>
        /// Set macro control from a macro in a script
        /// </summary>
        /// <param name="nameOfScript">name of script</param>
        /// <param name="indexOfMacro">index of macro</param>
        /// <param name="nameOfCmd">cmd name</param>
        /// <param name="data"></param>
        public bool SetMacroControl( string nameOfScript, int indexOfMacro, string nameOfCmd, UDataCarrier[] data )
        {
            if ( String.IsNullOrEmpty( nameOfCmd ) )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1} with empty cmd name.", nameOfScript, indexOfMacro ) );
                return false;
            }

            bool ret = false;
            UScript script = null;
            UMacro macro = null;
            // check internal first
            if ( CheckMacroControlsExist( nameOfCmd, nameOfScript, indexOfMacro, out script, out macro ) )
            {
                ret = SetThisClassControl( macro, UDataCarrier.MakeOne<string>( nameOfCmd ), data );
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET GLOBAL script = {0}, index of macro = {1}, using cmd name = {2} with status {3}.", nameOfScript, indexOfMacro, nameOfCmd, ret ) );
                return ret;
            }

            if ( script == null )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find script\".", nameOfScript, indexOfMacro, nameOfCmd ) );
                return false;
            }

            if ( macro == null )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find macro\".", nameOfScript, indexOfMacro, nameOfCmd ) );
                return false;
            }

            // otherwise, do normal set
            UMacroMethodProviderPlugin plugin = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( macro.OwnerClassName );
            if ( plugin == null )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find plugin class name\".", nameOfScript, indexOfMacro, nameOfCmd ) );
                return false;
            }

            try
            {
                ret = plugin.SetMacroControl( macro, UDataCarrier.MakeOne<string>( nameOfCmd ), data );
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2} with status {3}.", nameOfScript, indexOfMacro, nameOfCmd, ret ) );
            }
            catch ( Exception exp )
            {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2}\n[Exception]\n{3}.", nameOfScript, indexOfMacro, nameOfCmd, exp.ToString() ) );
                ret = false;
            }

            return ret;
        }

        public bool SetMacroControl( string nameOfScript, string nameOfCmd, UDataCarrier[] data )
        {
            if ( String.IsNullOrEmpty( nameOfCmd ) ) {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0} with empty cmd name.", nameOfScript ) );
                return false;
            }

            bool ret = false;
            UScript script = null;
            UMacro macro = null;
            //check internal first
            script = GetScript( nameOfScript );
            if (script == null) {
                if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                    m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, using cmd name = {1} fail with reason \"cannot find script\".", nameOfScript, nameOfCmd ) );
                return false;
            }
            // call each macro
            for ( int i = 0; i < script.MacroSet.Count; i++ ) {
                macro = script.MacroSet[ i ];
                if ( macro == null )
                    continue;

                // otherwise, do normal set
                UMacroMethodProviderPlugin plugin = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( macro.OwnerClassName );
                if ( plugin == null ) {
                    if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                        m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2} fail with reason \"cannot find plugin class name\".", nameOfScript, i, nameOfCmd ) );
                    continue;
                }

                try {
                    plugin.SetMacroControl( macro, UDataCarrier.MakeOne<string>( nameOfCmd ), data );
                    if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                        m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2} with status {3}.", nameOfScript, i, nameOfCmd, ret ) );
                } catch ( Exception exp ) {
                    if ( m_refAssemblyLoader != null && m_refAssemblyLoader.fpLog != null )
                        m_refAssemblyLoader.fpLog( eLogMessageType.NORMAL, 0, String.Format( "Call SET script = {0}, index of macro = {1}, using cmd name = {2}\n[Exception]\n{3}.", nameOfScript, i, nameOfCmd, exp.ToString() ) );
                }
            }

            return true;
        }

        public UDataCarrierTypeDescription[] GetMacroControlDesc( string nameOfScript, int indexOfMacro, string cmd, out bool bRetStatus )
        {
            bRetStatus = false;
            if ( String.IsNullOrEmpty( cmd ) ) return null;

            UScript script = GetScript( nameOfScript );
            if ( script == null ) return null;

            UMacro macro = script.GetByIndex( indexOfMacro );
            if ( macro == null ) return null;

            UMacroMethodProviderPlugin plugin = m_refAssemblyLoader.GetPluginInstanceByClassCSharpTypeName( macro.OwnerClassName );
            if ( plugin == null ) return null;

            return plugin.GetMacroControlTypeDescription( UDataCarrier.MakeOne<string>( cmd ), ref bRetStatus );
        }

        public void GetMacroCmdTypeDescs( out UPluginAssemblyCmdList[] allMacrosDescs )
        {
            allMacrosDescs = null;
            if ( m_bDisposed || m_refAssemblyLoader == null || m_refAssemblyLoader.PluginAssemblies == null ) return;

            List<UPluginAssemblyCmdList> lst = new List<UPluginAssemblyCmdList>();
            lst.Add( new UPluginAssemblyCmdList( "[System, ScriptService]", this.GetType().FullName, m_GlobalControls ) );
            for ( int i = 0 ; i < m_refAssemblyLoader.PluginAssemblies.Count ; i++ )
                lst.Add( new UPluginAssemblyCmdList( m_refAssemblyLoader.PluginAssemblies[ i ], false ) );

            allMacrosDescs = lst.ToArray();
        }

        #endregion

        #region [GET, SET: Global Control]

        private bool SetThisClassControl( UMacro dummy, UDataCarrier carrierOfCmd, UDataCarrier[] data )
        {
            if ( carrierOfCmd == null || !carrierOfCmd.IsTypeMatching<string>() )
                return false;

            string strCast = ( string ) carrierOfCmd.Data;
            if ( String.IsNullOrEmpty( strCast ) ) return false;

            bool ret = false;
            for ( int i = 0 ; i < m_GlobalControls.Count ; i++ )
            {
                if ( m_GlobalControls[ i ] == null ) continue;
                if ( !m_GlobalControls[ i ].CanSet || String.IsNullOrEmpty( m_GlobalControls[ i ].Name ) ) continue;

                if ( m_GlobalControls[ i ].Name == strCast )
                {
                    if ( m_GlobalControls[ i ].SetParam != null )
                        ret = m_GlobalControls[ i ].SetParam( m_GlobalControls[ i ], dummy, data );
                    break;
                }
            }

            return ret;
        }
        private UDataCarrier[] GetThisClassControl( UMacro dummy, UDataCarrier carrierOfCmd, ref bool bCallStatus )
        {
            bCallStatus = false;
            if ( carrierOfCmd == null || !carrierOfCmd.IsTypeMatching<string>() )
                return null;

            string strCast = ( string ) carrierOfCmd.Data;
            if ( String.IsNullOrEmpty( strCast ) ) return null;

            UDataCarrier[] ret = null;

            for ( int i = 0 ; i < m_GlobalControls.Count ; i++ )
            {
                if ( m_GlobalControls[ i ] == null ) continue;
                if ( !m_GlobalControls[ i ].CanGet || String.IsNullOrEmpty( m_GlobalControls[ i ].Name ) ) continue;

                if ( m_GlobalControls[ i ].Name == strCast )
                {
                    if ( m_GlobalControls[ i ].GetParam != null )
                        ret = m_GlobalControls[ i ].GetParam( m_GlobalControls[ i ], dummy, ref bCallStatus );
                    break;
                }
            }

            return ret;
        }


        private bool CheckMacroControlsExist( string cmd, string nameOfScript, int index0OfMacro, out UScript script, out UMacro macro )
        {
            script = null;
            macro = null;
            if ( String.IsNullOrEmpty( cmd ) )
                return false;

            bool bGot = false;
            for ( int i = 0; i < m_GlobalControls.Count; i++ ) {
                if ( m_GlobalControls[ i ] == null || String.IsNullOrEmpty( m_GlobalControls[ i ].Name ) ) continue;
                if ( m_GlobalControls[ i ].Name == cmd ) {
                    bGot = true; break;
                }
            }
            if ( !bGot ) {
                script = GetScript( nameOfScript );
                macro = script == null ? null : script.GetByIndex( index0OfMacro );

            }
            return bGot;
        }

        #region MacroMethod control cmd
        private UDataCarrier[] GetControl_MacroMethod( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            bRetStatus = false;
            if ( whichMacro == null ) return null;

            UDataCarrier[] arr = UDataCarrier.MakeArray( 2 );
            if ( arr == null )
                return null;

            UDataCarrier.SetItem<string>( arr, 0, whichMacro.OwnerClassName );
            UDataCarrier.SetItem<string>( arr, 1, whichMacro.MethodName );
            bRetStatus = true;
            return arr;
        }
        #endregion

        #region MacroExtractingResultToPack control cmd
        private UDataCarrier[] GetControl_MacroExtractingResultToPack( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            bRetStatus = false;
            if ( whichMacro == null ) return null;

            UDataCarrier[] repo = UDataCarrier.MakeOneItemArray<bool>( whichMacro.ExtractingResultToPack );
            bRetStatus = true;
            return repo;
        }
        private bool SetControl_MacroExtractingResultToPack( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( whichMacro == null || data == null || data.Length < 1 )
                return false;
            if ( data[ 0 ] == null || !data[ 0 ].IsTypeMatching<bool>() )
                return false;

            whichMacro.ExtractingResultToPack = ( bool ) data[ 0 ].Data;
            return true;
        }
        #endregion

        #region RuntimeLoadAssembly control cmd
        private bool SetControl_RuntimeLoadAssembly( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( m_refAssemblyLoader == null || data == null || data.Length < 1 )
                return false;
            if ( data[ 0 ] == null || !data[ 0 ].IsTypeMatching<string>() )
                return false;

            return m_refAssemblyLoader.LoadAssembly( data[ 0 ].Data as string );
        }
        #endregion

        #region EnableScriptLog control cmd
        private bool SetControl_EnableScriptLog( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( data == null || data.Length < 2 || data[ 0 ] == null || data[ 0 ].Data == null ||
                 data[ 1 ] == null || data[ 1 ].Data == null || data[ 1 ].Tp == null || data[ 1 ].Tp != typeof( bool ) ) return false;

            string nameOfScript = data[ 0 ].Data as string;
            bool enable = ( bool ) data[ 1 ].Data;

            UScript script = GetScript( nameOfScript );
            if ( script == null ) return false;

            script.EnableLogOut = enable;

            return true;
        }
        #endregion

        #region ResetMacroExecEvent control cmd
        private bool SetControl_ResetMacroExecEvent( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( !UDataCarrier.TypesCheck( data, m_GControlResetMacroExecEvent.DataTypes ) )
                return false;

            string nameOfScript = data[ 0 ].Data as string;
            int indexOfMacro = ( Int32 ) data[ 1 ].Data;
            string act = data[ 2 ].Data as string;
            int timeout = ( Int32 ) data[ 3 ].Data;

            eMacroEvtNameOfEventControl en = eMacroEvtNameOfEventControl.All;

            try { en = ( eMacroEvtNameOfEventControl ) Enum.Parse( typeof( eMacroEvtNameOfEventControl ), act ); }
            catch { return false; }

            UScript script = GetScript( nameOfScript );
            if ( script == null || script.MacroSet == null ) return false;
            UMacro macro = script.GetByIndex( indexOfMacro );
            if ( macro == null ) return false;
            if ( en == eMacroEvtNameOfEventControl.OnBeginWaitEvent ) return macro.ResetOnBeginWaitEvent( timeout );
            else if ( en == eMacroEvtNameOfEventControl.OnBeginTrigEvent ) return macro.ResetOnBeginTriggerEvent( timeout );
            else if ( en == eMacroEvtNameOfEventControl.OnEndWaitEvent ) return macro.ResetOnEndWaitEvent( timeout );
            else if ( en == eMacroEvtNameOfEventControl.OnEndTrigEvent ) return macro.ResetOnEndTriggerEvent( timeout );
            else if ( en == eMacroEvtNameOfEventControl.All )
            {
                if ( !macro.ResetOnBeginWaitEvent( timeout ) ) return false;
                if ( !macro.ResetOnBeginTriggerEvent( timeout ) ) return false;
                if ( !macro.ResetOnEndWaitEvent( timeout ) ) return false;
                return macro.ResetOnEndTriggerEvent( timeout );
            }

            return false;
        }
        #endregion

        #region ChangeOnConfMacro control cmd
        private bool SetControl_ChangeOnConfMacro( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( !UDataCarrier.TypesCheck( data, m_GControlChangeOnConfMacro.DataTypes ) )
                return false;

            string nameOfScript = data[ 0 ].Data as string;
            int indexOfMacro = ( Int32 ) data[ 1 ].Data;

            m_strCurrNameOfOnConfScript = String.IsNullOrEmpty( nameOfScript ) ? null : String.Copy( nameOfScript );
            m_nCurrIndexOfOnConMacro = indexOfMacro;
            return true;
        }
        #endregion

        #endregion
    }
}
