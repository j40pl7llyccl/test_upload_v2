using System;
using System.Collections.Generic;
using System.Threading;

namespace uIP.Lib.Script
{
    /// <summary>
    ///   - Script name
    ///   - Sync call by a binary semahore
    ///   - A macro created information ( UMacro )
    ///   - Log interfaces
    ///   - Executing the script
    ///   - ResultSet : available till next call
    ///     - Reported results ( UMacroProduceCarrierResult )
    ///     - Internal results ( UMacroProduceCarrierPropagation )
    ///     - Drawing results ( UMacroProduceCarrierDrawingResult )
    /// </summary>
    public partial class UScript : IDisposable
    {
        #region >>> Variables Decl <<<
        // internal variables
        private bool m_bDisposed = false;
        private Semaphore m_hBinSem = new Semaphore( 1, 1 );
        private object m_Owner = null;

        /* 兩種識別
         * - int
         * - string
         */
        private Int32 m_nSnOfId = -1;
        private string m_strNameOfId = null;

        /* Macro 的 instance 是 plugin assembly class 進行管理, 在此只是參照儲存
         * 當 script 要被 dispose 時會透過 owner instance 進行 recycle
         */
        private List<UMacro> m_listMacros = new List<UMacro>();
        // log delegate
        private fpLogMessage m_fpLog = null;

        // kepp extra info
        protected string m_strAnnotation = "";

        #endregion

        #region Property
        // Information
        public bool IsAvailable => !m_bDisposed;
        public object Owner => m_Owner;

        public string NameOfId
        {
            get => m_strNameOfId;
            set => m_strNameOfId = string.IsNullOrEmpty( value ) ? null : string.Copy( value );
        }
        public Int32 SnOfId
        {
            get => m_nSnOfId;
            set => m_nSnOfId = value >= 0 ? value : -1;
        }
        public string Annotation
        {
            get => m_strAnnotation;
            set => m_strAnnotation = value;
        }
        public List<UMacro> MacroSet => m_listMacros;
        public fpLogMessage fpLog
        {
            set => m_fpLog = value;
            get => m_fpLog;
        }

        protected bool NotifyMacroRunDelegateAutoReset { get; set; } = true;
        protected UDataCarrier NotifyMacroRunDelegateCtx { get; set; } = null;
        protected Action<UDataCarrier, UScript, UMacro, int> NotifyMacroRunDelegate { get; set; } = null;

        #endregion

        #region Constructor

        public UScript() { }

        public UScript( string givenName, object owner )
        {
            m_strNameOfId = String.IsNullOrEmpty( givenName ) ? null : String.Copy( givenName );
            m_Owner = owner;
        }

        public UScript( Int32 id, object owner )
        {
            m_nSnOfId = id;
            m_Owner = owner;
        }

        public UScript( string nm, Int32 id, object owner )
        {
            m_strNameOfId = String.IsNullOrEmpty( nm ) ? null : String.Copy( nm );
            m_nSnOfId = id;
            m_Owner = owner;
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            // 若是不處在 Semaphore 保護下呼叫, 可能會產生 Exception
            if ( m_bDisposed ) return;
            m_bDisposed = true;

            // Wait sync semaphore
            m_hBinSem.WaitOne();

            // Clear result carrier
            FreeResultCarriers();

            // Clear propagation carrier
            FreePropagationCarriers();

            // Clear drawing result carrier
            FreeDrawingResultCarriers();

            // Free calling resource
            for ( int i = 0 ; i < m_listMacros.Count ; i++ )
            {
                if ( m_listMacros[ i ] == null ) continue;
                // Not handle if the method is shared.
                if ( m_listMacros[ i ].ReusableMacro ) continue;

                m_listMacros[ i ].Recycle();
            }

            // Free semaphore resource
            m_hBinSem.Release();
            m_hBinSem.Close();
            m_hBinSem = null;
        }

        #endregion

        #region [管理 Macro]

        private void SetBroadcastCmd( string cmd, UDataCarrier[] dat )
        {
            UDataCarrier cmdItem = new UDataCarrier( cmd, typeof( string ) );
            for ( int i = 0 ; i < m_listMacros.Count ; i++ )
            {
                if ( m_listMacros[ i ] == null )
                    continue;
                m_listMacros[ i ].OwnerOfPluginClass.SetMacroControl( m_listMacros[ i ], cmdItem, dat );
            }
        }

        public bool Add( UMacro macro )
        {
            if ( macro == null ) return false;
            if ( m_bDisposed ) return false;
            if ( m_listMacros == null ) return false;

            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync ) return false;
                bObtainSem = true;
            }

            // Add for a macro to know whole script calls
            macro.OwnerMacrosList = m_listMacros;
            macro.OwnerOfScript = this;
            m_listMacros.Add( macro );

            // notify a macro coming in
            SetBroadcastCmd( UScriptCtrlMarcoToProviderPlugins.NewMacroJoinCmd,
                             UDataCarrier.MakeVariableItemsArray( m_strNameOfId, m_nSnOfId, m_listMacros, macro ) );

            if ( bObtainSem ) m_hBinSem.Release();
            return true;
        }

        public bool InsertBeforeIndex( UMacro macro, int index_0base )
        {
            if ( macro == null ) return false;
            if ( m_bDisposed ) return false;
            if ( m_listMacros == null ) return false;

            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync ) return false;
                bObtainSem = true;
            }

            // Add for a macro to know whole script calls
            macro.OwnerMacrosList = m_listMacros;
            macro.OwnerOfScript = this;
            m_listMacros.Insert( index_0base, macro );

            // notify a macro coming in
            SetBroadcastCmd( UScriptCtrlMarcoToProviderPlugins.NewMacroJoinCmd,
                             UDataCarrier.MakeVariableItemsArray( m_strNameOfId, m_nSnOfId, m_listMacros, macro ) );

            if ( bObtainSem ) m_hBinSem.Release();
            return true;
        }

        public bool CopyAppendingFrom( int reference_index_0base )
        {
            if ( m_bDisposed ) return false;
            if ( m_listMacros == null ) return false;
            if ( reference_index_0base < 0 || reference_index_0base >= m_listMacros.Count ) return false;

            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync ) return false;
                bObtainSem = true;
            }

            UMacro from = m_listMacros[ reference_index_0base ];
            if ( from != null )
            {
                UMacro copycat = null;
                if ( from.ConfigDone )
                    copycat = UMacro.Reproduce( from );
                else
                    copycat = from.OwnerOfPluginClass.CreateMacroInstance( UDataCarrier.MakeOneItemArray<string>( from.MethodName ), from.ParameterCarrierImmutable, from.ParameterCarrierVariable );
                // config
                if ( copycat != null )
                {
                    copycat.OwnerMacrosList = m_listMacros;
                    copycat.OwnerOfScript = this;
                    copycat.GatherNearbyMacro = true;
                    // add to list
                    if ( reference_index_0base == (m_listMacros.Count - 1) ) // the last one
                        m_listMacros.Add( copycat );
                    else
                        m_listMacros.Insert( reference_index_0base + 1, copycat ); // insert to reference index after
                }
            }


            if ( bObtainSem ) m_hBinSem.Release();
            return true;
        }

        public UMacro GetByIndex( int index_0base )
        {
            // Cannot get in running
            if ( m_bDisposed || m_listMacros == null || m_bUnderRunning ) return null;
            if ( index_0base < 0 || index_0base >= m_listMacros.Count ) return null;

            return (m_listMacros[ index_0base ]);
        }

        public UMacro[] GetByMethodName( string name )
        {
            // Cannot get in running
            if ( m_bDisposed || m_listMacros == null || m_bUnderRunning ) return null;
            if ( String.IsNullOrEmpty( name ) ) return null;

            List<UMacro> list = new List<UMacro>();
            if ( list == null ) return null;

            for ( int i = 0 ; i < m_listMacros.Count ; i++ )
            {
                if ( m_listMacros[ i ] == null ) continue;
                if ( m_listMacros[ i ].MethodName == name )
                {
                    list.Add( m_listMacros[ i ] );
                }
            }

            UMacro[] ret = list.Count <= 0 ? null : list.ToArray();

            list.Clear();
            list = null;

            return ret;
        }

        public Int32 GetByMacroInstance(UMacro macro)
        {
            // Cannot get in running
            if ( m_bDisposed || m_listMacros == null || m_bUnderRunning ) return -1;
            if ( m_listMacros == null ) return -1;

            for(int i = 0 ; i < m_listMacros.Count ; i++ )
            {
                if ( m_listMacros[ i ] == null ) continue;
                if ( m_listMacros[ i ] == macro )
                    return i;
            }
            return -1;
        }

        public void Clear()
        {
            if ( m_bDisposed || m_listMacros == null ) return;

            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync ) return;
                bObtainSem = true;
            }

            // Clear resources
            if ( m_listMacros != null )
            {
                for ( int i = 0 ; i < m_listMacros.Count ; i++ )
                {
                    if ( m_listMacros[ i ] == null ) continue;
                    // Not handle that the method is shared.
                    if ( m_listMacros[ i ].ReusableMacro ) continue;

                    m_listMacros[ i ].Recycle();
                }
                m_listMacros.Clear();
            }

            if ( bObtainSem ) m_hBinSem.Release();
        }

        public bool RemoveFrom( int index_0base )
        {
            if ( m_bDisposed || m_listMacros == null ) return false;
            if ( index_0base < 0 || index_0base >= m_listMacros.Count ) return false;

            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync ) return false;
                bObtainSem = true;
            }

            List<UMacro> rml = new List<UMacro>();
            rml.Add( m_listMacros[ index_0base ] );

            UMacro curr = m_listMacros[ index_0base ];
            UMacro nextone = index_0base + 1 >= m_listMacros.Count ? null : m_listMacros[ index_0base + 1 ];

            // current macro is belong a set of group, remove all the group items
            if ( curr.GatherNearbyMacro )
            {
                for ( int i = index_0base + 1 ; i < m_listMacros.Count ; i++ )
                {
                    if ( m_listMacros[ i ] == null || !m_listMacros[ i ].GatherNearbyMacro )
                        break;
                    rml.Add( m_listMacros[ i ] );
                }
                for ( int i = index_0base ; i > 0 ; i-- )
                {
                    if ( m_listMacros[ i ] == null )
                        break;
                    rml.Add( m_listMacros[ i - 1 ] );
                    if ( !m_listMacros[ i - 1 ].GatherNearbyMacro )
                        break;
                }
            }
            // the next macro is a set of group, remove next group items
            else if ( nextone != null && nextone.GatherNearbyMacro )
            {
                for ( int i = index_0base + 1 ; i < m_listMacros.Count ; i++ )
                {
                    if ( m_listMacros[ i ] == null || !m_listMacros[ i ].GatherNearbyMacro )
                        break;
                    rml.Add( m_listMacros[ i ] );
                }
            }

            if (rml.Count > 0)
            {
                // remove from script
                foreach(var m in rml)
                {
                    m_listMacros.Remove( m );
                }

                // notify a macro remove
                SetBroadcastCmd( UScriptCtrlMarcoToProviderPlugins.MacroRemoveCmd,
                                 UDataCarrier.MakeVariableItemsArray( m_strNameOfId, m_nSnOfId, m_listMacros, rml ) );

                // handle resource
                foreach(var m in rml)
                {
                    if ( !m.ReusableMacro )
                        m.Recycle();
                }
                rml.Clear();
            }

            if ( bObtainSem ) m_hBinSem.Release();
            return true;
        }

        public bool RemoveFrom( UMacro m )
        {
            return RemoveFrom( GetByMacroInstance( m ) );
        }

        public bool Replace( UMacro newOne, string methodInfoGivenName )
        {
            if ( m_bDisposed || m_listMacros == null ) return false;
            if ( newOne == null || String.IsNullOrEmpty( methodInfoGivenName ) ) return false;

            var status = false;
            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( m_nSyncWaitMS, true );
                if ( !bSync ) return false;
                bObtainSem = true;
            }

            UMacro toReplace = null;
            int index = -1;

            for(int i = 0; i < m_listMacros.Count; i++)
            {
                var m = m_listMacros[ i ];
                if ( m == null )
                    continue;
                if ( string.IsNullOrEmpty( m.GivenName ) )
                    continue;
                if ( m.GivenName == methodInfoGivenName)
                {
                    toReplace = m;
                    index = i;
                    break;
                }
            }
            
            if ( toReplace != null)
            {
                status = true;
                // replace
                newOne.OwnerOfScript = this;
                m_listMacros[ index ] = newOne;

                // notify a macro remove
                SetBroadcastCmd( UScriptCtrlMarcoToProviderPlugins.MacroReplaceCmd,
                                 UDataCarrier.MakeVariableItemsArray( m_strNameOfId, m_nSnOfId, m_listMacros, toReplace, newOne ) );

                // recycle old one
                if ( !toReplace.ReusableMacro )
                    toReplace.Recycle();
            }

            if ( bObtainSem ) m_hBinSem.Release();
            return status;
        }

        #endregion

        public bool InstallNotification(UDataCarrier ctx, Action<UDataCarrier, UScript, UMacro, int> call, bool bAutoReset = true)
        {
            if ( m_bUnderRunning )
                return false;

            bool bObtainSem = false;
            if ( m_bSyncCall )
            {
                bool bSync = m_hBinSem.WaitOne( 0, true );
                if ( !bSync ) return false;
                bObtainSem = true;
            }

            NotifyMacroRunDelegate = call;
            NotifyMacroRunDelegateAutoReset = bAutoReset;
            NotifyMacroRunDelegateCtx = ctx;

            if ( bObtainSem ) m_hBinSem.Release();
            return true;
        }
    }
}
