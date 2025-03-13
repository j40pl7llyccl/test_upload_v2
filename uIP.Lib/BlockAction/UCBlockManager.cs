using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uIP.Lib.InterPC;
using uIP.Lib.DataCarrier;

namespace uIP.Lib.BlockAction
{
    public class UCBlockItem  : IDisposable
    {
        public UCBlockBase _Block = null;
        private bool _bHandleBlock = false;
        public UCBlockItem() { }
        public UCBlockItem(UCBlockBase block, bool bHandle)
        {
            _Block = block;
            _bHandleBlock = bHandle;
        }
        public void Dispose()
        {
            if (_bHandleBlock && _Block != null) {
                _Block.Dispose();
            }
            _Block = null;
            _bHandleBlock = false;
        }
    }

    public class UCBlockManager :IDisposable
    {
        protected bool m_bDisposing = false;
        protected bool m_bDisposed = false;
        public bool IsDispose { get { return m_bDisposing || m_bDisposed; } }

        protected Dictionary<string, UCBlockItem> _Blocks = new Dictionary<string, UCBlockItem>();
        protected IPipeClientComm _pClientPipe = null;
        protected UCWin32SharedMemFormating _pFormatSharedMems = null;
        protected UCDataSyncW32<Int32> _pInt32SharedMem = null;
        protected UCDataSyncW32<double> _pDoubleSharedMem = null;
        protected UCDataSyncW32<Int64> _pInt64SharedMem = null;
        protected UCDataSyncW32<Int32> _pInt32SharedMemPermanent = null;
        protected UCDataSyncW32<double> _pDoubleSharedMemPermanent = null;
        protected UCDataSyncW32<Int64> _pInt64SharedMemPermanent = null;
        protected fpLogMessage _fpLog = null;
        protected IGuiAclManagement _GuiAcl = null;

        internal Dictionary<string, UCBlockItem> Blocks {  get { return _Blocks; } }

        public UCBlockManager() { }
        public UCBlockManager(fpLogMessage log, IPipeClientComm pipeClient, UCWin32SharedMemFormating shMemFormat, 
            UCDataSyncW32<Int32> int32ShMem, UCDataSyncW32<Int64> int64ShMem, UCDataSyncW32<double> dfShMem,
            UCDataSyncW32<Int32> int32ShMemPermanent, UCDataSyncW32<Int64> int64ShMemPermanent, UCDataSyncW32<double> dfShMemPermanent )
        {
            _fpLog = log;
            _pClientPipe = pipeClient;
            _pFormatSharedMems = shMemFormat;
            _pInt32SharedMem = int32ShMem;
            _pInt64SharedMem = int64ShMem;
            _pDoubleSharedMem = dfShMem;
            _pInt32SharedMemPermanent = int32ShMemPermanent;
            _pInt64SharedMemPermanent = int64ShMemPermanent;
            _pDoubleSharedMemPermanent = dfShMemPermanent;
        }
        public void Dispose()
        {
            if ( IsDispose ) return;
            m_bDisposing = true;

            Dispose( false );

            m_bDisposed = true;
            m_bDisposing = false;
        }
        protected virtual void Dispose(bool disposing)
        {
            foreach(KeyValuePair<string,UCBlockItem>kv in _Blocks) {
                if ( kv.Value == null ) continue;
                UCBlockItem itm = kv.Value;
                itm.Dispose();
            }
            _Blocks.Clear();
        }

        protected void SetBlock(string name, UDataCarrier dat)
        {
            if ( IsDispose ) return;
            foreach(KeyValuePair<string, UCBlockItem> kv in _Blocks) {
                if ( kv.Value == null || kv.Value._Block == null || kv.Value._Block.IsDispose )
                    continue;

                kv.Value._Block.Set( name, dat );
            }
        }
        public void SetLog(fpLogMessage log)
        {
            _fpLog = log;
            UDataCarrier dat = UDataCarrier.MakeOne<fpLogMessage>( log );
            SetBlock( UCBlockBase.strUCB_LOG, dat );
        }
        public void SetPipeClient(IPipeClientComm pipeClient)
        {
            _pClientPipe = pipeClient;
            UDataCarrier dat = UDataCarrier.MakeOne<IPipeClientComm>( pipeClient );
            SetBlock( UCBlockBase.strUCB_COMMUNICATION_CLIENTPIPE, dat );
        }
        public void SetFormatSharedMem(UCWin32SharedMemFormating inst)
        {
            _pFormatSharedMems = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCWin32SharedMemFormating>( inst );
            dat.Handleable = false;
            SetBlock( UCBlockBase.strUCB_FORMAT_SHARED_MEM, dat );
        }
        public void SetSharedMemInt32(UCDataSyncW32<Int32> inst)
        {
            _pInt32SharedMem = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCDataSyncW32<Int32>>( inst );
            dat.Handleable = false;
            SetBlock( UCBlockBase.strUCB_INT32_SHARED_MEM, dat );
        }
        public void SetSharedMemInt64(UCDataSyncW32<Int64> inst)
        {
            _pInt64SharedMem = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCDataSyncW32<Int64>>( inst );
            SetBlock( UCBlockBase.strUCB_INT64_SHARED_MEM, dat );
        }
        public void SetSharedMemDouble(UCDataSyncW32<double> inst)
        {
            _pDoubleSharedMem = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCDataSyncW32<double>>( inst );
            SetBlock( UCBlockBase.strUCB_DOUBLE_SHARED_MEM, dat );
        }
        public void SetSharedMemInt32Permanent( UCDataSyncW32<Int32> inst)
        {
            _pInt32SharedMem = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCDataSyncW32<Int32>>( inst );
            SetBlock( UCBlockBase.strUCB_INT32_SHARED_MEM_PERMANENT, dat );
        }
        public void SetSharedMemInt64Permanent( UCDataSyncW32<Int64> inst)
        {
            _pInt64SharedMem = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCDataSyncW32<Int64>>( inst );
            SetBlock( UCBlockBase.strUCB_INT64_SHARED_MEM_PERMANENT, dat );
        }
        public void SetSharedMemDoublePermanent( UCDataSyncW32<double> inst)
        {
            _pDoubleSharedMem = inst;
            UDataCarrier dat = UDataCarrier.MakeOne<UCDataSyncW32<double>>( inst );
            SetBlock( UCBlockBase.strUCB_DOUBLE_SHARED_MEM_PERMANENT, dat );
        }
        public void SetGuiAcl( IGuiAclManagement acl)
        {
            _GuiAcl = acl;
            SetBlock( UCBlockBase.strUCB_GUI_ACL, UDataCarrier.MakeOne<IGuiAclManagement>( acl ) );
        }

        virtual protected void ConfigDefault(UCBlockBase block)
        {
            if ( block == null ) return;
            if ( _pClientPipe != null )
                SetClientPipe( block, _pClientPipe );
            if ( _pFormatSharedMems != null )
                SetFormatingSharedMem( block, _pFormatSharedMems );
            if ( _pInt32SharedMem != null )
                SetSharedMemI32( block, _pInt32SharedMem );
            if ( _pInt64SharedMem != null )
                SetSharedMemI64( block, _pInt64SharedMem );
            if ( _pDoubleSharedMem != null )
                SetSharedMemDf( block, _pDoubleSharedMem );
            if ( _pInt32SharedMemPermanent != null )
                SetSharedMemI32Permanent( block, _pInt32SharedMemPermanent );
            if ( _pInt64SharedMemPermanent != null )
                SetSharedMemI64Permanent( block, _pInt64SharedMemPermanent );
            if ( _pDoubleSharedMemPermanent != null )
                SetSharedMemDfPermanent( block, _pDoubleSharedMemPermanent );
            if ( _fpLog != null )
                SetLog( block, _fpLog );
            if ( _GuiAcl != null )
                SetGuiAcl( block, _GuiAcl );
        }

        virtual public bool AddBlock(UCBlockBase block, bool handle = true)
        {
            if ( IsDispose ) return false;
            if ( block == null || String.IsNullOrEmpty( block.ID ) ) return false;
            if ( _Blocks.ContainsKey( block.ID ) ) return false; // already exist

            ConfigDefault( block );
            _Blocks.Add( block.ID, new UCBlockItem( block, handle ) );
            return true;
        }
        virtual public void RemoveBlock(string blockID)
        {
            if ( IsDispose ) return;
            if (_Blocks.ContainsKey(blockID)) {
                UCBlockItem itm = _Blocks[ blockID ];
                _Blocks.Remove( blockID );
                itm.Dispose();
                itm = null;
            }
        }
        virtual public UCBlockBase GetBlock(string blockID)
        {
            if ( IsDispose ) return null;
            if ( !_Blocks.ContainsKey( blockID ) ) return null;

            return _Blocks[ blockID ]._Block;
        }
        public T GetBlockT<T>(string blkId) where T : UCBlockBase
        {
            try
            {
                return (T)GetBlock(blkId);
            } catch { return null; }
        }

        virtual public bool BlockSet(string blockID, string nameOfSet, UDataCarrier dat)
        {
            if ( IsDispose ) return false;
            UCBlockBase b = GetBlock( blockID );
            if ( b == null ) return false;
            return b.Set( nameOfSet, dat );
        }
        public bool BlockSet(string blkId, object descOfSet, UDataCarrier data)
        {
            try
            {
                return BlockSet( blkId, descOfSet?.ToString() ?? "", data );
            } catch { return false; }
        }
        public bool BlockSetT<T>(string blkId, object descOfSet, T data)
        {
            return BlockSet( blkId, descOfSet, UDataCarrier.MakeOne( data ) );
        }
        virtual public bool BlockGet(string blockID, string nameOfGet, out UDataCarrier dat)
        {
            dat = null;
            if ( IsDispose ) return false;
            UCBlockBase b = GetBlock( blockID );
            if ( b == null ) return false;

            return b.Get( nameOfGet, out dat );
        }
        public bool BlockGet(string blkId, object descOfGet, out UDataCarrier data)
        {
            data = null;
            try
            {
                return BlockGet(blkId, descOfGet?.ToString()??"", out data );
            } catch { return false; }
        }
        public T BlockGetT<T>(string blkId, object descOfGet, T def)
        {
            try
            {
                if ( !BlockGet( blkId, descOfGet, out var data ) )
                    return def;
                return (T)data.Data;
            } catch { return def; }
        }
        public T BlockAssistantGetT<T>(string blkId, int assistantIndex, object descOfGet, T def)
        {
            try
            {
                var b = GetBlock( blkId );
                if ( b == null ) return def;
                return b.AssistantGetT(assistantIndex, descOfGet, def);
            } catch { return def; }
        }
        public bool BlockAssistantSetT<T>(string blkId, int assistantIndex, object descOfSet, T v)
        {
            try
            {
                var b = GetBlock(blkId);
                if ( b == null ) return false;
                return b.AssistantSetT(assistantIndex, descOfSet, v);
            } catch { return false; }
        }

        public static bool SetID(UCBlockBase block, string id)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_ID, UDataCarrier.MakeOne<string>( id ) );
        }
        public static bool GetID(UCBlockBase block, out string id)
        {
            id = null;
            if ( block == null ) return false;
            UDataCarrier dat;
            if ( !block.Get( UCBlockBase.strUCB_ID, out dat ) )
                return false;

            id = dat == null || dat.Data == null ? "" : dat.Data as string;
            return true;

        }
        public static bool SetLog( UCBlockBase block, fpLogMessage fp)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_LOG, UDataCarrier.MakeOne<fpLogMessage>( fp ) );
        }
        public static bool SetBlockStateChangeCallback(UCBlockBase block, fpUCBlockStateChangedCallback fp)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_STATE_CHANGE_CALLBACK, UDataCarrier.MakeOne<fpUCBlockStateChangedCallback>( fp ) );
        }
        public static bool SetInfoOutCall(UCBlockBase block, fpUCBlockInformOut fp)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_INFORM_OUT, UDataCarrier.MakeOne<fpUCBlockInformOut>( fp ) );
        }
        public static bool SetClientPipe(UCBlockBase block, IPipeClientComm cp)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_COMMUNICATION_CLIENTPIPE, UDataCarrier.MakeOne<IPipeClientComm>( cp ) );
        }
        public static bool SetFormatingSharedMem(UCBlockBase block, UCWin32SharedMemFormating fsh)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_FORMAT_SHARED_MEM, UDataCarrier.MakeOne<UCWin32SharedMemFormating>( fsh ) );
        }
        public static bool SetSharedMemI32( UCBlockBase block, UCDataSyncW32<Int32> i32 )
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_INT32_SHARED_MEM, UDataCarrier.MakeOne<UCDataSyncW32<Int32>>( i32 ) );
        }
        public static bool SetSharedMemI64(UCBlockBase block, UCDataSyncW32<Int64> i64)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_INT64_SHARED_MEM, UDataCarrier.MakeOne<UCDataSyncW32<Int64>>( i64 ) );
        }
        public static bool SetSharedMemDf(UCBlockBase block, UCDataSyncW32<double> df)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_DOUBLE_SHARED_MEM, UDataCarrier.MakeOne<UCDataSyncW32<double>>( df ) );
        }
        public static bool SetSharedMemI32Permanent( UCBlockBase block, UCDataSyncW32<Int32> i32 )
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_INT32_SHARED_MEM_PERMANENT, UDataCarrier.MakeOne<UCDataSyncW32<Int32>>( i32 ) );
        }
        public static bool SetSharedMemI64Permanent( UCBlockBase block, UCDataSyncW32<Int64> i64)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_INT64_SHARED_MEM_PERMANENT, UDataCarrier.MakeOne<UCDataSyncW32<Int64>>( i64 ) );
        }
        public static bool SetSharedMemDfPermanent( UCBlockBase block, UCDataSyncW32<double> df)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_DOUBLE_SHARED_MEM_PERMANENT, UDataCarrier.MakeOne<UCDataSyncW32<double>>( df ) );
        }
        public static bool PopupUI(UCBlockBase block)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_POPUP_SETTING, null );
        }
        public static bool SetGuiAcl(UCBlockBase block, IGuiAclManagement acl)
        {
            if ( block == null ) return false;
            return block.Set( UCBlockBase.strUCB_GUI_ACL, UDataCarrier.MakeOne<IGuiAclManagement>( acl ) );
        }
    }
}
