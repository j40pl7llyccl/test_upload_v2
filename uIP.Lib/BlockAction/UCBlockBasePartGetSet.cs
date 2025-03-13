using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using uIP.Lib.InterPC;
using uIP.Lib.DataCarrier;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        protected IGuiAclManagement _GuiAcl = null;
        protected IPipeClientComm _pCommCH = null;
        protected string _strID = null;
        protected fpLogMessage _fpLog = null;
        protected fpUCBlockStateChangedCallback _fpStateChangedCall = null;
        protected fpUCBlockInformOut _fpInformOut = null;
        protected string _strRwDir = "";

        /*
         * Control value規則
         * - 由class內部管理的資源，傳出去(GET)後將Handleable設定成false外部接收到時應該檢查
         *   此類不應由 <T> 來進行存取，因為此類會將Handleable脫鉤，呼叫端會不知道是否可以進行Dispose，因而呼叫Dispose造成錯誤
         * - 由於UDataCarrier有實作destructer，因此如果Data乘載有IDispose物件，GetDicKeyStrOne/ Set時需
         *   要由誰管理資源，以防GC回收造成記憶體錯誤
         *   eg, 外界呼叫Set時，指定Handleable為false，那麼Class內接收後，就應負責管理
         * - 沒有unmanage的資源與沒有IDispose則限制較少
         */
        protected List<UCBlockDataCtrl> _arrayGetSetCtrlList = new List<UCBlockDataCtrl>();

        public const string strUCB_ID = "id";
        public const string strUCB_LOG = "log";
        public const string strUCB_STATE_CHANGE_CALLBACK = "stateChangeCallback";
        public const string strUCB_ASSISTANT_STATE_CHANGE_CALLBACK = "assistantStateChangeCallback";
        public const string strUCB_INFORM_OUT = "informOut";
        public const string strUCB_RW_DIR = "RwDir";
        public const string strUCB_COMMUNICATION_CLIENTPIPE = "communicationPipeClient";
        public const string strUCB_FORMAT_SHARED_MEM = "FormatingSharedMem";
        public const string strUCB_INT32_SHARED_MEM = "Int32SharedMem";
        public const string strUCB_INT64_SHARED_MEM = "Int64SharedMem";
        public const string strUCB_DOUBLE_SHARED_MEM = "DoubleSharedMem";
        public const string strUCB_INT32_SHARED_MEM_PERMANENT = "Int32SharedMemPermanent";
        public const string strUCB_INT64_SHARED_MEM_PERMANENT = "Int64SharedMemPermanent";
        public const string strUCB_DOUBLE_SHARED_MEM_PERMANENT = "DoubleSharedMemPermanent";
        public const string strUCB_POPUP_SETTING = "PopupSettingUI";
        public const string strUCB_SETTINGS = "BlockSettings";
        public const string strUCB_POPUP_PARAMETERUI = "PopupMakeParamUI";
        public const string strUCB_CLONE_PARAMETER = "CloneParameter";
        public const string strUCB_CLONE_SETTINGS = "CloneSettings";
        public const string strUCB_GUI_ACL = "GuiAcl";
        public const string strUCB_INPUT_PARAM_UI_CONTROL = "InputParamUIControl";
        public const string strUCB_BLOCK_SETTINGS_UI_CONTROL = "BlockSettingsUIControl";

        private void InitGetSet()
        {
            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_ID, true, true, false, GetCtrl_Id, SetCtrl_Id,
                new UDataCarrierTypeDescription( typeof( string ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_LOG, false, true, false, null, SetCtrl_Log,
                new UDataCarrierTypeDescription( typeof( fpLogMessage ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_STATE_CHANGE_CALLBACK, false, true, false, null, SetCtrl_StateChangeCallback,
                new UDataCarrierTypeDescription( typeof( fpUCBlockStateChangedCallback ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_INFORM_OUT, false, true, false, null, SetCtrl_InformOut,
                new UDataCarrierTypeDescription( typeof( fpUCBlockInformOut ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_COMMUNICATION_CLIENTPIPE, false, true, false, null, SetCtrl_CommPipeClient,
                new UDataCarrierTypeDescription( typeof( IPipeClientComm ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_FORMAT_SHARED_MEM, false, true, false, null, SetCtrl_FormatSharedMem,
                new UDataCarrierTypeDescription( typeof( UCWin32SharedMemFormating ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_INT32_SHARED_MEM, false, true, false, null, SetCtrl_Int32SharedMem,
                new UDataCarrierTypeDescription( typeof( UCDataSyncW32<Int32> ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_INT64_SHARED_MEM, false, true, false, null, SetCtrl_Int64SharedMem,
                new UDataCarrierTypeDescription( typeof( UCDataSyncW32<Int64> ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_DOUBLE_SHARED_MEM, false, true, false, null, SetCtrl_DoubleSharedMem,
                new UDataCarrierTypeDescription( typeof( UCDataSyncW32<double> ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_INT32_SHARED_MEM_PERMANENT, false, true, false, null, SetCtrl_Int32SharedMemPermanent,
                new UDataCarrierTypeDescription( typeof( UCDataSyncW32<Int32> ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_INT64_SHARED_MEM_PERMANENT, false, true, false, null, SetCtrl_Int64SharedMemPermanent,
                new UDataCarrierTypeDescription( typeof( UCDataSyncW32<Int64> ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_DOUBLE_SHARED_MEM_PERMANENT, false, true, false, null, SetCtrl_DoubleSharedMemPermanent,
                new UDataCarrierTypeDescription( typeof( UCDataSyncW32<double> ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_POPUP_SETTING, false, true, false, null, SetCtrl_PopupSettingUI,
                null ) );
            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_SETTINGS, true, true, true, GetCtrl_Settings, SetCtrl_Settings,
                null ) );


            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_POPUP_PARAMETERUI, true, true, false, GetCtrl_PopupMakeParam, SetCtrl_PopupMakeParam,
                null ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_GUI_ACL, false, true, false, null, SetCtrl_GuiAcl,
                new UDataCarrierTypeDescription( typeof( IGuiAclManagement ) ) ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_INPUT_PARAM_UI_CONTROL, true, false, false, GetCtrl_MakeParamGUI, null,
                null ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_BLOCK_SETTINGS_UI_CONTROL, true, false, false, GetCtrl_BlockSettingGUI, null,
                null ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_CLONE_PARAMETER, true, true, false, GetCtrl_CloneParameter, SetCtrl_CloneParameter, null ) );

            _arrayGetSetCtrlList.Add( new UCBlockDataCtrl(
                strUCB_CLONE_SETTINGS, true, true, false, GetCtrl_CloneSettings, SetCtrl_CloneSettings, null ) );
        }

        #region ID
        private bool SetCtrl_Id(UDataCarrier dat)
        {
            if ( dat == null ) return false;
            if ( dat.Data == null ) return false;

            string conv = dat.Data as String;
            _strID = String.IsNullOrEmpty( conv ) ? "" : String.Copy( conv );
            return true;
        }
        private bool GetCtrl_Id(out UDataCarrier dat )
        {
            dat = UDataCarrier.MakeOne( _strID );
            return true;
        }
        #endregion

        private void SetAssistants(string name, UDataCarrier dat)
        {
            if ( _pAssistants.Count > 0 ) {
                for ( int i = 0; i < _pAssistants.Count; i++ ) {
                    if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                        continue;
                    _pAssistants[ i ]._pAssistant.Set( name, dat );
                }
            }
        }

        #region Log
        private bool SetCtrl_Log(UDataCarrier dat)
        {
            if ( dat == null ) {
                _fpLog = null;
                SetAssistants( strUCB_LOG, null );
                return true;
            }
            if ( dat.Data == null ) return false;

            fpLogMessage log = dat.Data as fpLogMessage;
            if ( log != null ) _fpLog = log;

            SetAssistants( strUCB_LOG, dat ); // also call assistants set log

            return true;
        }
        #endregion

        #region State Change callback
        private bool SetCtrl_StateChangeCallback(UDataCarrier dat)
        {
            if ( dat == null ) {
                _fpStateChangedCall = null;
                return true;
            }
            if ( dat.Data == null ) return false;
            fpUCBlockStateChangedCallback fp = dat.Data as fpUCBlockStateChangedCallback;
            _fpStateChangedCall = fp;
            return true;
        }
        #endregion

        #region inform out
        private bool SetCtrl_InformOut(UDataCarrier dat)
        {
            if ( dat == null ) {
                _fpInformOut = null;
                return true;
            }
            if ( dat.Data == null ) return false;
            fpUCBlockInformOut fp = dat.Data as fpUCBlockInformOut;
            _fpInformOut = fp;
            return true;
        }
        #endregion

        #region communicationPipeClient
        private bool SetCtrl_CommPipeClient(UDataCarrier dat)
        {
            if ( dat == null ) {
                _pCommCH = null;
                SetAssistants( strUCB_COMMUNICATION_CLIENTPIPE, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            IPipeClientComm comm = dat.Data as IPipeClientComm;
            _pCommCH = comm;

            SetAssistants( strUCB_COMMUNICATION_CLIENTPIPE, dat ); // also call assistants set
            return true;
        }
        #endregion

        #region shared mem
        private bool SetCtrl_FormatSharedMem(UDataCarrier  dat)
        {
            if (dat == null) {
                _pSharedMems = null;
                SetAssistants( strUCB_FORMAT_SHARED_MEM, dat );
                return true;
            }
            if ( dat.Data == null ) return false;

            _pSharedMems = dat.Data as UCWin32SharedMemFormating;
            SetAssistants( strUCB_FORMAT_SHARED_MEM, dat );
            return true;
        }
        private bool SetCtrl_Int32SharedMem(UDataCarrier dat)
        {
            if ( dat == null ) {
                _pEnvSharedMemInt32 = null;
                SetAssistants( strUCB_INT32_SHARED_MEM, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            _pEnvSharedMemInt32 = dat.Data as UCDataSyncW32<Int32>;
            SetAssistants( strUCB_INT32_SHARED_MEM, dat ); // also call assistants set
            return true;
        }
        private bool SetCtrl_Int64SharedMem(UDataCarrier dat)
        {
            if ( dat == null ) {
                _pEnvSharedMemInt64 = null;
                SetAssistants( strUCB_INT64_SHARED_MEM, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            _pEnvSharedMemInt64 = dat.Data as UCDataSyncW32<Int64>;
            SetAssistants( strUCB_INT64_SHARED_MEM, dat ); // also call assistants set
            return true;
        }
        private bool SetCtrl_DoubleSharedMem(UDataCarrier dat)
        {
            if (dat == null ) {
                _pEnvSharedMemDouble = null;
                SetAssistants( strUCB_DOUBLE_SHARED_MEM, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            _pEnvSharedMemDouble = dat.Data as UCDataSyncW32<double>;
            SetAssistants( strUCB_DOUBLE_SHARED_MEM, dat ); // also call assistants set
            return true;
        }
        private bool SetCtrl_Int32SharedMemPermanent( UDataCarrier dat)
        {
            if ( dat == null ) {
                _pEnvSharedMemInt32Permanent = null;
                SetAssistants( strUCB_INT32_SHARED_MEM_PERMANENT, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            _pEnvSharedMemInt32Permanent = dat.Data as UCDataSyncW32<Int32>;
            SetAssistants( strUCB_INT32_SHARED_MEM_PERMANENT, dat ); // also call assistants set
            return true;
        }
        private bool SetCtrl_Int64SharedMemPermanent( UDataCarrier dat)
        {
            if ( dat == null ) {
                _pEnvSharedMemInt64Permanent = null;
                SetAssistants( strUCB_INT64_SHARED_MEM_PERMANENT, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            _pEnvSharedMemInt64Permanent = dat.Data as UCDataSyncW32<Int64>;
            SetAssistants( strUCB_INT64_SHARED_MEM_PERMANENT, dat ); // also call assistants set
            return true;
        }
        private bool SetCtrl_DoubleSharedMemPermanent( UDataCarrier dat)
        {
            if (dat == null ) {
                _pEnvSharedMemDoublePermanent = null;
                SetAssistants( strUCB_DOUBLE_SHARED_MEM_PERMANENT, null );
                return true;
            }
            if ( dat.Data == null ) return false;
            _pEnvSharedMemDoublePermanent = dat.Data as UCDataSyncW32<double>;
            SetAssistants( strUCB_DOUBLE_SHARED_MEM_PERMANENT, dat ); // also call assistants set
            return true;
        }
        #endregion

        #region Popup Setting UI

        protected virtual bool SetCtrl_PopupSettingUI(UDataCarrier dat)
        {
            return false;
        }
        protected virtual bool GetCtrl_BlockSettingGUI( out UDataCarrier dat )
        {
            dat = null;
            return false;
        }

        #endregion

        #region Block Settings
        private bool SetCtrl_Settings( UDataCarrier dat )
        {
            return SetBlockSettings( dat );
        }
        protected virtual bool SetBlockSettings( UDataCarrier dat )
        {
            return false;
        }
        private bool GetCtrl_Settings( out UDataCarrier dat )
        {
            return GetBlockSetttings( out dat );
        }
        protected virtual bool GetBlockSetttings( out UDataCarrier dat )
        {
            dat = null;
            return false;
        }

        #endregion

        #region Popup Make Parameter UI
        protected object _TempForMakeParamUI = null;
        //protected virtual bool GetCtrl_PopupMakeParamUI( out UDataCarrier pDat, out fpUCBlockHandleDatCarrier fpHandler )
        private bool GetCtrl_PopupMakeParam( out UDataCarrier pDat )
        {
            return GetCtrl_PopupMakeParamUI( out pDat );
        }
        private bool SetCtrl_PopupMakeParam( UDataCarrier dat )
        {
            return SetCtrl_ConfigMakeParamUI( dat );
        }
        // pDat container type: UDataCarrierSet
        protected virtual bool GetCtrl_PopupMakeParamUI( out UDataCarrier pDat )
        {
            pDat = null;
            //fpHandler = null;
            return false;
        }
        protected virtual bool SetCtrl_ConfigMakeParamUI(UDataCarrier dat)
        {
            _TempForMakeParamUI = dat == null ? null : dat.Data;
            return true;
        }
        private bool GetCtrl_MakeParamGUI( out UDataCarrier pDat )
        {
            return GetCtrl_MakeParamGUIControl( out pDat );
        }
        // pDat container type: inhert from UserControlMakeParamBase
        protected virtual bool GetCtrl_MakeParamGUIControl( out UDataCarrier pDat )
        {
            pDat = null;
            //fpHandler = null;
            return false;
        }
        #endregion

        #region GUI ACL
        private bool SetCtrl_GuiAcl(UDataCarrier dat)
        {
            if (dat == null) {
                _GuiAcl = null;
                SetAssistants( strUCB_GUI_ACL, dat );
                return true;
            }

            if ( dat.Data == null ) return false;
            _GuiAcl = dat.Data as IGuiAclManagement;
            SetAssistants( strUCB_GUI_ACL, dat );
            return true;
        }
        #endregion

        #region Clone Parameter
        protected object _TempStoreParamToClone = null;
        private bool SetCtrl_CloneParameter(UDataCarrier dat)
        {
            return SetCloneParameters( dat );
        }
        private bool GetCtrl_CloneParameter(out UDataCarrier dat)
        {
            return GetCloneParameters( out dat );
        }
        protected virtual bool SetCloneParameters(UDataCarrier dat2Store)
        {
            _TempStoreParamToClone = dat2Store;
            return true;
        }
        protected virtual bool GetCloneParameters(out UDataCarrier dat)
        {
            dat = _TempForMakeParamUI as UDataCarrier;
            return true;
        }
        #endregion

        #region Clone Settings
        protected object _TempStoreSettingsToClone = null;
        private bool SetCtrl_CloneSettings(UDataCarrier dat)
        {
            return SetCloneSettings( dat );
        }
        private bool GetCtrl_CloneSettings( out UDataCarrier dat )
        {
            return GetCloneSettings( out dat );
        }
        protected virtual bool SetCloneSettings(UDataCarrier dat)
        {
            _TempStoreSettingsToClone = dat;
            return true;
        }
        protected virtual bool GetCloneSettings(out UDataCarrier dat)
        {
            dat = _TempStoreSettingsToClone as UDataCarrier;
            return true;
        }
        #endregion

        private static UCBlockDataCtrl RetrieveCtrl(List<UCBlockDataCtrl> lst, string name)
        {
            for(int i =0; i < lst.Count; i++ ) {
                if ( lst[ i ] == null )
                    continue;
                if ( lst[ i ]._strNameOfData == name )
                    return lst[ i ];
            }
            return null;
        }

        // block GET, SET
        virtual public bool Set( string pName, UDataCarrier pData )
        {
            if ( IsDispose ) return false;
            UCBlockDataCtrl ctrl = RetrieveCtrl( _arrayGetSetCtrlList, pName );
            if ( ctrl == null || !ctrl._bCanSet || ctrl._fpSet == null ) return false;

            if ( UDataCarrier.TypesCheck( new UDataCarrier[] { pData }, new UDataCarrierTypeDescription[] { ctrl._DataDescription } ) ) {
                return ctrl._fpSet( pData );
            }
            return false;
        }
        public bool Set(object descName, UDataCarrier dataToSet)
        {
            return Set( descName?.ToString() ?? "", dataToSet );
        }
        public bool SetT<T>( object descName, T dataToSet )
        {
            return Set( descName, UDataCarrier.MakeOne( dataToSet ) );
        }

        virtual public bool Get( string pName, out UDataCarrier pRetData )
        {
            pRetData = null;
            if ( IsDispose ) return false;

            UCBlockDataCtrl ctrl = RetrieveCtrl( _arrayGetSetCtrlList, pName );
            if ( ctrl == null || !ctrl._bCanGet || ctrl._fpGet == null ) return false;

            return ctrl._fpGet( out pRetData );
        }
        public bool Get(object descName, out UDataCarrier retData )
        {
            return Get(descName?.ToString()??"", out retData );
        }
        public T GetT<T>(object descName, T def, bool chkHandleable = true)
        {
            if (!Get(descName?.ToString()??"", out var data))
                return def;
            if ( !data.Handleable && chkHandleable )
                throw new Exception( "cannot get unhandleable throu this method" );

            try
            {
                T r = ( T ) data.Data;
                data.Data = null;
                return r;
            } catch { data?.Dispose(); return def; }
        }

        virtual public bool WriteData(string folderPath, string nameOfFile)
        {
            if ( IsDispose ) return false;

            List<UDataCarrier> datToSave = new List<UDataCarrier>();
            List<string> datNameToSave = new List<string>();
            for(int i = 0; i < _arrayGetSetCtrlList.Count; i++ ) {
                UCBlockDataCtrl ctrl = _arrayGetSetCtrlList[ i ];
                if ( ctrl == null ) continue;
                if (ctrl._bParam && ctrl._bCanGet && ctrl._fpGet != null ) {
                    UDataCarrier dat = null;
                    if (ctrl._fpGet(out dat) && dat != null) {
                        datToSave.Add( dat );
                        datNameToSave.Add( ctrl._strNameOfData );
                    }
                }
            }

            string path = Path.Combine( folderPath, nameOfFile );
            bool status = false;
            try {
                using ( Stream ws = File.Open( path, FileMode.Create ) ) {
                    status = UDataCarrier.WriteXml( datToSave.ToArray(), datNameToSave.ToArray(), ws );
                }
            } catch { }

            UDataCarrier.FreeByIDispose( datToSave.ToArray() );
            datToSave.Clear();
            datNameToSave.Clear();

            return status;
        }
        virtual public bool ReadData(string folderPath, string nameOfFile)
        {
            if ( IsDispose ) return false;

            string path = Path.Combine( folderPath, nameOfFile );
            UDataCarrier[] dat = null;
            string[] names = null;
            if ( !UDataCarrier.ReadXml( path, ref dat, ref names ) )
                return false;

            if (dat!= null && names != null && dat.Length == names.Length) {
                for(int i = 0; i < dat.Length; i++ ) {
                    UCBlockDataCtrl ctrl = RetrieveCtrl( _arrayGetSetCtrlList, names[ i ] );
                    if ( ctrl == null ) continue;

                    if(ctrl._bCanSet && ctrl._fpSet != null) {
                        if ( UDataCarrier.TypesCheck( new UDataCarrier[] { dat[ i ] }, new UDataCarrierTypeDescription[] { ctrl._DataDescription } ) ) {
                            if ( !ctrl._fpSet( dat[ i ] ) && _fpLog != null ) {
                                _fpLog( eLogMessageType.NORMAL, 0, String.Format( "[{0}] reload from file: ctrl name {1} set error!", _strID, ctrl._strNameOfData ) );
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public static Dictionary<string, UDataCarrier> GetCtrlValuesO(UCBlockBase blk, params object [] nameDescs)
        {
            var ret = new Dictionary<string, UDataCarrier>();
            if ( blk == null || nameDescs == null )
                return ret;
            foreach ( var v in nameDescs )
            {
                if ( !blk.Get( v, out var data ) )
                    continue;

                ret.Add( v.ToString(), data );
            }
            return ret;
        }
        public static Dictionary<string, UDataCarrier> GetCtrlValuesS( UCBlockBase blk, params string [] names)
        {
            var ret = new Dictionary<string, UDataCarrier>();
            if ( blk == null || names == null )
                return ret;
            foreach ( var v in names )
            {
                if ( !blk.Get( v, out var data ))
                    continue;

                ret.Add( v.ToString(), data );
            }
            return ret;
        }

        public static Dictionary<string, UDataCarrier> SetCtrlValues(UCBlockBase blk, Dictionary<object, object> toSet)
        {
            var ret = new Dictionary<string, UDataCarrier>();
            if ( blk == null || toSet == null )
                return ret;
            foreach(var kv in toSet )
            {
                if ( kv.Key == null || kv.Value == null )
                    continue;
                bool handleGetData = false;
                blk.Get( kv.Key.ToString(), out var oriData );
                if ( blk.Set( kv.Key, new UDataCarrier( kv.Value, kv.Value.GetType() ) ) && oriData != null)
                    ret.Add( kv.Key.ToString(), oriData );
                else handleGetData = true;
                if ( handleGetData )
                    oriData?.Dispose();
            }
            return ret;
        }
    }
}
