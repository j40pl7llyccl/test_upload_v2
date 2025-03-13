using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uIP.Lib.DataCarrier;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        /*
         * UCWin32SharedMemFormating 各個可用的 shared memory 空間與名稱配置在之前必須先產生
         */
        protected UCWin32SharedMemFormating _pSharedMems = null;
        protected Dictionary<string, TShMemAccItem> _mapAllocFormattedSharedMem = null;
        /*
         * 可以使用的 shared memory 但是需要在之前產生
         */
        protected UCDataSyncW32<Int32> _pEnvSharedMemInt32 = null;
        protected UCDataSyncW32<double> _pEnvSharedMemDouble = null;
        protected UCDataSyncW32<Int64> _pEnvSharedMemInt64 = null;
        protected UCDataSyncW32<Int32> _pEnvSharedMemInt32Permanent = null;
        protected UCDataSyncW32<double> _pEnvSharedMemDoublePermanent = null;
        protected UCDataSyncW32<Int64> _pEnvSharedMemInt64Permanent = null;

        /*
         * 只接收外部指定, 程式內部不會產生
         */
        public UCWin32SharedMemFormating FormattingSharedMemory { get { return _pSharedMems; } set { _pSharedMems = value; } }
        public UCDataSyncW32<Int32> SharedMemoryInt32 {  get { return _pEnvSharedMemInt32; } set { _pEnvSharedMemInt32 = value; } }
        public UCDataSyncW32<double> SharedMemoryDouble {  get { return _pEnvSharedMemDouble; } set { _pEnvSharedMemDouble = value; } }
        public UCDataSyncW32<Int64> SharedMemoryInt64 {  get { return _pEnvSharedMemInt64; } set { _pEnvSharedMemInt64 = value; } }
        public UCDataSyncW32<Int32> SharedMemoryInt32Permanent {  get { return _pEnvSharedMemInt32Permanent; } set { _pEnvSharedMemInt32Permanent = value; } }
        public UCDataSyncW32<double> SharedMemoryDoublePermanent {  get { return _pEnvSharedMemDoublePermanent; } set { _pEnvSharedMemDoublePermanent = value; } }
        public UCDataSyncW32<Int64> SharedMemoryInt64Permanent {  get { return _pEnvSharedMemInt64Permanent; } set { _pEnvSharedMemInt64Permanent = value; } }

        //
        // Formatting shared memory access
        //
        public int GetNumOfFormattingSharedMemory()
        {
            if ( IsDispose ) return -1;
            if ( _mapAllocFormattedSharedMem == null ) return 0;

            return _mapAllocFormattedSharedMem.Count;
        }
        public TShMemAccItem GetFormattingSharedMemoryFromIndex( int index_0 )
        {
            if ( IsDispose ) return null;
            if ( _mapAllocFormattedSharedMem == null ) return null;
            int i = 0;
            foreach(KeyValuePair<string, TShMemAccItem> kv in _mapAllocFormattedSharedMem) {
                if (i == index_0) {
                    return kv.Value;
                }
                i++;
            }
            return null;
        }
        public bool IsItemFormattingSharedMemoryValid( TShMemAccItem h )
        {
            if ( IsDispose ) return false;
            if ( h == null ) return false;
            if ( _mapAllocFormattedSharedMem == null ) return false;
            foreach ( KeyValuePair<string, TShMemAccItem> kv in _mapAllocFormattedSharedMem) {
                if (kv.Value == h) {
                    return true;
                }
            }
            return false;
        }
        public TShMemAccItem GetFormattingSharedMemoryFromIdName( string pIdName )
        {
            if ( IsDispose ) return null;
            if ( _mapAllocFormattedSharedMem == null ) return null;
            if ( !_mapAllocFormattedSharedMem.ContainsKey( pIdName ) ) return null;
            return _mapAllocFormattedSharedMem[ pIdName ];
        }

        protected TShMemAccItem CreateSharedMemoryStorageI32( string pIdName, int nCount )
        {
            if ( IsDispose ) return null;
            if ( string.IsNullOrEmpty( pIdName ) || nCount <= 0 ) return null;
            if ( _pSharedMems == null || _mapAllocFormattedSharedMem == null ) return null;
            if ( _mapAllocFormattedSharedMem.ContainsKey( pIdName ) )
                return _mapAllocFormattedSharedMem[ pIdName ];

            TShMemAccItem itm = null;
            if ( !_pSharedMems.AllocItemInt32( nCount, out itm ) )
                return null;

            _mapAllocFormattedSharedMem[ pIdName ] = itm;
            return itm;
        }
        protected TShMemAccItem CreateSharedMemoryStorageDouble( string pIdName, int nCount )
        {
            if ( IsDispose ) return null;
            if ( string.IsNullOrEmpty( pIdName ) || nCount <= 0 ) return null;
            if ( _pSharedMems == null || _mapAllocFormattedSharedMem == null ) return null;
            if ( _mapAllocFormattedSharedMem.ContainsKey( pIdName ) )
                return _mapAllocFormattedSharedMem[ pIdName ];

            TShMemAccItem itm = null;
            if ( !_pSharedMems.AllocItemDouble( nCount, out itm ) )
                return null;

            _mapAllocFormattedSharedMem[ pIdName ] = itm;
            return itm;
        }
        protected TShMemAccItem CreateSharedMemoryStorageU8( string pIdName, int nCount )
        {
            if ( IsDispose ) return null;
            if ( string.IsNullOrEmpty( pIdName ) || nCount <= 0 ) return null;
            if ( _pSharedMems == null || _mapAllocFormattedSharedMem == null ) return null;
            if ( _mapAllocFormattedSharedMem.ContainsKey( pIdName ) )
                return _mapAllocFormattedSharedMem[ pIdName ];

            TShMemAccItem itm = null;
            if ( !_pSharedMems.AllocItemU8( nCount, out itm ) )
                return null;

            _mapAllocFormattedSharedMem[ pIdName ] = itm;
            return itm;
        }

    }
}
