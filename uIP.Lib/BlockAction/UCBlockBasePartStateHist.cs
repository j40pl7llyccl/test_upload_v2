using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        /*
         * 最新的在 index 最小
         */
        protected List<Int32> _BlockStateStorage = new List<int>();
        public List<Int32> HistoricalStates { get { return _BlockStateStorage; } }
        public Int32 LastStateNotDefault {
            get {
                var ds = Enum.GetValues(typeof( UCBlockStateReserved ) ).Cast<Int32>().ToArray();
                if ( _BlockStateStorage == null || _BlockStateStorage.Count <= 0 )
                    return int.MinValue;
                for(int i = 0; i < _BlockStateStorage.Count; i++)
                {
                    if ( ds.Contains( _BlockStateStorage [ i ] ) )
                        continue;
                    return _BlockStateStorage [ i ];
                }
                return int.MinValue;
            }
        }

        protected void ClearBlockKeeper()
        {
            _BlockStateStorage.Clear();
        }
        protected void AddBlockState(Int32 state)
        {
            if ( _BlockStateStorage.Count > 0 && state == _BlockStateStorage[ 0 ] ) return;
            _BlockStateStorage.Insert( 0, state );
        }
        /* 在最近的幾個內搜尋 */
        protected bool GetBlockPreviousState( Int32 firstHitState, out Int32 retState, Int32 count )
        {
            retState = ( Int32 ) UCBlockStateReserved.UCBLOCK_STATE_NA;
            if ( _BlockStateStorage.Count <= 0 ) return false;

            bool bHit = false;
            int index = 0;
            for ( int i = 0; i < _BlockStateStorage.Count; i++ ) {
                if ( _BlockStateStorage[ i ] == firstHitState ) {
                    if ( count <= 0 ) {
                        retState = _BlockStateStorage[ i ];
                        return true;
                    }
                    bHit = true;
                    continue;
                }

                if ( bHit ) {
                    if ( ++index == count ) {
                        retState = _BlockStateStorage[ i ];
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
