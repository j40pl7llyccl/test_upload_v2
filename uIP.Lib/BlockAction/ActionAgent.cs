using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;

using uIP.Lib.Utility;
using uIP.Lib.InterPC;
using uIP.Lib.DataCarrier;

namespace uIP.Lib.BlockAction
{
    public class ActionAgent : IDisposable
    {
        private bool m_bLoadingAssembly = false;
        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;
        public bool IsDispose { get { return m_bDisposing || m_bDisposed; } }

        protected fpLogMessage _fpLog = null;
        protected Dictionary<string, List<string>> _Actions = new Dictionary<string, List<string>>();
        protected UCBlockRunnerWin32 _Runner = null;
        protected PluginAssemblies _Assemblies = null;
        protected string _strTmpRwPath = null;
        protected string[] _strsAllAvailableTps = null;

        protected ActionManager _AM_Immutable = null;
        protected ActionManager _AM_Variable = null;

        public string[] AllAvailableBlockNames {  get { return _strsAllAvailableTps; } }
        public UCBlockRunnerWin32 Runner { get { return _Runner; } }
        public PluginAssemblies LoadedAssemblies { get { return _Assemblies; } }
        public ActionManager ImmutableAM { get { return _AM_Immutable; } }
        public ActionManager VariableAM {  get { return _AM_Variable; } }

        public const string DESC_OF_INI_SEC_ASSEMBLY_KEY = "assembly";

        public void ReadAssemblies(string assembliesRootPath, string assembliesIniPath)
        {
            if ( m_bLoadingAssembly ) return;
            // [Function of Assemblies]
            // assembly="relative path of root"
            // ...
            // [Function of Assemblies]
            // assembly="relative path of root"
            // ...
            if ( !Directory.Exists( assembliesRootPath ) || !File.Exists( assembliesIniPath ) )
                return;

            IniReaderUtility ini = new IniReaderUtility();
            if ( !ini.Parsing( assembliesIniPath ) )
                return;

            string[] sections = ini.GetSections();
            if ( sections == null || sections.Length <= 0 )
                return;

            m_bLoadingAssembly = true;

            for(int i = 0; i < sections.Length; i++ ) {
                SectionDataOfIni kvs = ini.Get( sections[ i ] );
                if ( kvs == null || kvs.Data == null || kvs.Data.Count <= 0 )
                    continue;
                for(int j = 0; j < kvs.Data.Count; j++ ) {
                    if ( kvs.Data[ j ] == null ) continue;
                    if ( kvs.Data[ j ].Key != DESC_OF_INI_SEC_ASSEMBLY_KEY ) continue;
                    if ( kvs.Data[ j ].Values == null || kvs.Data[ j ].Values.Length <= 0 ) continue;
                    string fn = String.IsNullOrEmpty( kvs.Data[ j ].Values[ 0 ] ) ? null : kvs.Data[ j ].Values[ 0 ];
                    if ( String.IsNullOrEmpty( fn ) ) continue;
                    fn = fn.Trim().Replace( "\"", "" ).Trim();
                    if ( String.IsNullOrEmpty( fn ) ) continue;
                    string path = Path.Combine( assembliesRootPath, fn );

                    if (_Assemblies == null) {
                        _Assemblies = new PluginAssemblies();
                    }

                    // get absolute path
                    string pathOfAssembly = Path.GetFullPath( path );
                    if (_Assemblies.Add( pathOfAssembly ) ) {
                        if ( _fpLog != null ) _fpLog( eLogMessageType.NORMAL, 0, String.Format( "[ActionAgent] load assembly {0}", pathOfAssembly ) );
                    } else {
                        if ( _fpLog != null ) _fpLog( eLogMessageType.NORMAL, 0, String.Format( "[ActionAgent] load assembly {0} fail!", pathOfAssembly ) );
                    }
                }
            }

            Type[] tps = _Assemblies.QueryFromType<UCBlockBase>();
            if (tps != null && tps.Length > 0) {
                List<string> keep = new List<string>();
                for ( int i = 0; i < tps.Length; i++ ) {
                    keep.Add( tps[ i ].FullName );
                    if ( _fpLog != null ) _fpLog( eLogMessageType.NORMAL, 0, String.Format("[ActionAgent] BlockAction Type: {0}", tps[i].FullName) );
                }
                _strsAllAvailableTps = keep.ToArray();
                keep = null;
                tps = null;
            }

            // loading default blocks
            SectionDataOfIni sdat = ini.Get( "DefaultBlocks" );
            if (sdat != null && sdat.Data != null && sdat.Data.Count > 0) {
                for (int i = 0; i < sdat.Data.Count; i++ ) {
                    if ( sdat.Data[ i ] == null ) continue;
                    if ( sdat.Data[i].Key == "block" && sdat.Data[i].Values.Length > 0 && !String.IsNullOrEmpty( sdat.Data[i].Values[0])) {
                        AddBlock( sdat.Data[ i ].Values[ 0 ].Replace( "\"", "" ).Trim() );
                    }
                }
            }
        }

        public ActionAgent( string givenName,
            //string assembliesRootPath, string assembliesIniPath, 
            string pathForTmpParam,
            fpLogMessage log,
            IPipeClientComm pipeClient,
            UCWin32SharedMemFormating formatingSharedMem,
            UCDataSyncW32<Int32> i32SharedMem,
            UCDataSyncW32<Int64> i64SharedMem,
            UCDataSyncW32<double> dfSharedMem,
            UCDataSyncW32<Int32> i32SharedMemPermanent,
            UCDataSyncW32<Int64> i64SharedMemPermanent,
            UCDataSyncW32<double> dfSharedMemPermanent,
            IGuiAclManagement guiAcl )
        {
            _fpLog = log;
            // load assembly
            // ReadAssemblies( assembliesRootPath, assembliesIniPath );
            // check tmp path
            if (!String.IsNullOrEmpty(pathForTmpParam)) {
                if ( CommonUtilities.RCreateDir( pathForTmpParam ) )
                    _strTmpRwPath = String.Copy( pathForTmpParam );
            }
            // create block runner
            _Runner = new UCBlockRunnerWin32( givenName, log, pipeClient, formatingSharedMem, i32SharedMem, i64SharedMem, dfSharedMem, i32SharedMemPermanent, i64SharedMemPermanent, dfSharedMemPermanent );
            _Runner.BlockManager.SetGuiAcl( guiAcl );

            _AM_Immutable = new ActionManager( this );
            _AM_Immutable.FileName = "ImmuActionMgSettings.xml";
            _AM_Immutable.GivenName = "AM_Immutable";
            _AM_Variable = new ActionManager( this );
            _AM_Variable.FileName = "VarActionMgSettings.xml";
            _AM_Variable.GivenName = "AM_Variable";
        }

        public bool AddBlock(string fullTypeName)
        {
            if ( IsDispose ) return false;
            if ( _Runner.BlockManager.Blocks.ContainsKey( fullTypeName ) ) return true;
            Type tp;
            Assembly got = PluginAssemblies.GetCurrentDomainAssemblyByTypeFullName( fullTypeName, out tp );
            if ( got == null ) return false;
            if ( !tp.IsSubclassOf( typeof( UCBlockBase ) ) )
                return false;
            UCBlockBase block = Activator.CreateInstance( tp ) as UCBlockBase;
            block.ID = fullTypeName;
            block.Owner = this;
            if (!_Runner.BlockManager.AddBlock(block)) {
                block.Dispose();
                block = null;
                return false;
            }
            return true;
        }
        public bool CallBlockSet(string fullTypeName, string nameOfSet, UDataCarrier dat)
        {
            if ( IsDispose ) return false;
            if ( !_Runner.BlockManager.Blocks.ContainsKey( fullTypeName ) ) return false;

            return _Runner.SetBlock( fullTypeName, nameOfSet, dat );
        }
        public bool CallBlockGet(string fullTypeName, string nameOfGet, out UDataCarrier dat)
        {
            dat = null;
            if ( IsDispose ) return false;
            if ( !_Runner.BlockManager.Blocks.ContainsKey( fullTypeName ) ) return false;

            return _Runner.GetBlock( fullTypeName, nameOfGet, out dat );
        }

        public void RemoveActionOne(UInt32 sn)
        {
            if ( IsDispose ) return;
            _Runner.RemoveWork( sn );
        }
        public void RemoveActionMultiple(UInt32 sn)
        {
            if ( IsDispose ) return;
            _Runner.RemoveGroup( sn );
        }
        public bool AddActionOne( string blockId, UDataCarrierSet param, fpUCBlockHandleDatCarrierSet fpHandParam,
            fpBlockRunWorkDoneCallback fpNotifyBeg, object contextOfBeg,
            fpBlockRunWorkDoneCallback fpNotifyEnd, object contextOfEnd, out UInt32 sn )
        {
            sn = 0;
            if ( IsDispose ) return false;

            sn = _Runner.AddWork2nd( blockId, param, fpHandParam, fpNotifyBeg, contextOfBeg, fpNotifyEnd, contextOfEnd );

            return sn != 0;
        }
        public bool AddActionMultiple( string[] blocks, UDataCarrierSet[] runParams, fpUCBlockHandleDatCarrierSet[] fpHandRunParams,
            fpBlockRunWorkDoneCallback[] fpNotifyBegs, object[] contextOfBegs,
            fpBlockRunWorkDoneCallback[] fpNotifyEnds, object[] contextOfEnds,
            fpBlockRunWorkGroupDoneCallback fpNotifyGroupEnd, object contextOfGEndCall, out UInt32[] Sns, out UInt32 groupSN )
        {
            Sns = null;
            groupSN = 0;
            if ( IsDispose ) return false;

            return _Runner.AddWorkList2nd( blocks, runParams, fpHandRunParams, fpNotifyBegs, contextOfBegs, fpNotifyEnds, contextOfEnds, fpNotifyGroupEnd, contextOfGEndCall, out Sns, out groupSN );
        }
        private bool RunAM( AnActionInfo a, out UInt32[] Sns, out UInt32 groupSn, 
            fpBlockRunWorkDoneCallback fpBegNotify = null, fpBlockRunWorkDoneCallback fpEndNotify = null, fpBlockRunWorkGroupDoneCallback fpGroupEndNotify = null,
            bool bSync = true )
        {
            Sns = null;
            groupSn = 0;
            if ( a == null )
                return false;

            if (bSync) Monitor.Enter(a._SyncDat);
            try {
                for ( int i = 0; i < a._Blocks.Count; i++ ) {
                // config first
                    if ( a._Blocks[ i ]._BlockSettings != null ) {
                    CallBlockSet( a._Blocks[ i ]._strNameOfBlock, UCBlockBase.strUCB_SETTINGS, a._Blocks[ i ]._BlockSettings );
                }
            }

            string[] blocks = a.BlocksName;
            UDataCarrierSet[] runParams = a.BlocksInputs;
            fpUCBlockHandleDatCarrierSet[] fpHandRunParams = Gen<fpUCBlockHandleDatCarrierSet>( blocks == null ? 0 : blocks.Length, null );
            fpBlockRunWorkDoneCallback[] fpNotifyBegs = Gen<fpBlockRunWorkDoneCallback>( blocks == null ? 0 : blocks.Length, null );
                fpBlockRunWorkDoneCallback[] fpNotifyEnds = Gen<fpBlockRunWorkDoneCallback>( blocks == null ? 0 : blocks.Length, null );
            object[] contextOfBegs = Gen<object>( blocks == null ? 0 : blocks.Length, null );
                if ( fpBegNotify != null ) fpNotifyBegs[ 0 ] = fpBegNotify;
                if ( fpEndNotify != null ) fpNotifyEnds[ fpNotifyEnds.Length - 1 ] = fpEndNotify;
                return AddActionMultiple( blocks, runParams, fpHandRunParams, fpNotifyBegs, contextOfBegs, fpNotifyBegs, contextOfBegs, fpGroupEndNotify, null, out Sns, out groupSn );
            } finally { if ( bSync ) Monitor.Exit( a._SyncDat ); }
        }
        public bool RunAM( string nameOfAction, ActionManager whichAM, out UInt32[] Sns, out UInt32 groupSn, 
            fpBlockRunWorkDoneCallback fpBegNotify = null, fpBlockRunWorkDoneCallback fpEndNotify = null, fpBlockRunWorkGroupDoneCallback fpGroupEndNotify = null )
        {
            Sns = null;
            groupSn = 0;
            if ( whichAM == null )
                return false;

            AnActionInfo a = whichAM.Get( nameOfAction );
            if ( a == null )
                return false;

            return RunAM( a, out Sns, out groupSn, fpBegNotify, fpEndNotify, fpGroupEndNotify );
        }
        public bool RunAM( string nameOfAction, ActionManager whichAM, UDataCarrierSet[] rDat, int[] rIndex, out UInt32[] Sns, out UInt32 groupSn, 
            fpBlockRunWorkDoneCallback fpBegNotify = null, fpBlockRunWorkDoneCallback fpEndNotify = null, fpBlockRunWorkGroupDoneCallback fpGroupEndNotify = null )
        {
            Sns = null;
            groupSn = 0;
            if (whichAM == null)
                return false;

            AnActionInfo a = whichAM.Get(nameOfAction);
            if (a == null)
                return false;

            Monitor.Enter(a._SyncDat);
            try {
                if (rDat == null || rIndex == null)
                    return RunAM( a, out Sns, out groupSn, fpBegNotify, fpEndNotify, fpGroupEndNotify, false );
                else if ( rDat.Length == rIndex.Length ) {
                    if (a.BlocksInputs == null)
                        return false;
                    for ( int i = 0; i < rDat.Length; i++ ) {
                        a.SetInputParam(rDat[i]._Array, rIndex[i]);
                    }
                    return RunAM( a, out Sns, out groupSn, fpBegNotify, fpEndNotify, fpGroupEndNotify, false );
                }
                return false;
            } finally { Monitor.Exit( a._SyncDat ); }
        }

        private static T[] Gen<T>(int num, T val)
        {
            if ( typeof( T ).IsValueType || typeof( T ).IsEnum )
                return null;
            List<T> ret = new List<T>();
            for(int i = 0; i < num; i++ ) {
                ret.Add( val );
            }

            return ret.ToArray();
        }

        public void Dispose()
        {
            if ( IsDispose ) return;
            m_bDisposing = true;

            Dispose( true );

            m_bDisposed = true;
            m_bDisposing = false;
        }
        protected virtual void Dispose( bool disposing )
        {
            if (_AM_Immutable != null) {
                _AM_Immutable.Dispose();
                _AM_Immutable = null;
            }
            if (_AM_Variable != null) {
                _AM_Variable.Dispose();
                _AM_Variable = null;
            }
            _Actions.Clear();

            _Runner.Dispose();
            _Runner = null;

            _Assemblies = null;

            // remove tmp path
            if (Directory.Exists(_strTmpRwPath)) {
                try { Directory.Delete( _strTmpRwPath, true ); } catch { }
            }
            _strTmpRwPath = null;
        }
    }
}
