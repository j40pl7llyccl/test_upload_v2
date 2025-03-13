using System.Collections.Generic;

namespace uIP.Lib.Script
{
    public partial class UScript
    {
        public static void SpreadSetCmd( List<UMacro> script, string cmd, UDataCarrier[] dat )
        {
            if ( script == null )
                return;

            UDataCarrier cmdItem = new UDataCarrier( cmd, typeof( string ) );
            for ( int i = 0 ; i < script.Count ; i++ )
            {
                if ( script[ i ] == null )
                    continue;
                script[ i ].OwnerOfPluginClass.SetMacroControl( script[ i ], cmdItem, dat );
            }
        }

        private static int[] GetMergedNearbyIndexes( int index_0base, List<UMacro> script )
        {
            if ( script == null || index_0base < 0 || index_0base >= script.Count )
                return null;

            List<int> keep = new List<int>();
            UMacro curr = script[ index_0base ];
            UMacro next = index_0base + 1 >= script.Count ? null : script[ index_0base + 1 ];

            keep.Add( index_0base );

            if ( curr.GatherNearbyMacro )
            {
                for ( int i = index_0base + 1 ; i < script.Count ; i++ )
                {
                    if ( script[ i ] == null || !script[ i ].GatherNearbyMacro )
                        break;
                    keep.Add( i );
                }
                for ( int i = index_0base ; i > 0 ; i-- )
                {
                    if ( script[ i ] == null )
                        break;
                    keep.Add( i - 1 );
                    if ( !script[ i - 1 ].GatherNearbyMacro )
                        break;
                }
            }
            else if ( next != null && next.GatherNearbyMacro )
            {
                for ( int i = index_0base + 1 ; i < script.Count ; i++ )
                {
                    if ( script[ i ] == null || !script[ i ].GatherNearbyMacro )
                        break;
                    keep.Add( i );
                }
            }

            keep.Sort();

            return keep.ToArray();
        }

        public static void GetProduceCarrierResult( int index_0base, List<UMacroProduceCarrierResult> srcCarrierSet, List<UMacro> script, List<UMacroProduceCarrierResult> dstCarrierSet )
        {
            int[] indexes = GetMergedNearbyIndexes( index_0base, script );
            if ( indexes == null || indexes.Length <= 0 || dstCarrierSet == null )
                return;

            for ( int i = 0 ; i < indexes.Length ; i++ )
            {
                if ( indexes[ i ] < 0 || indexes[ i ] >= srcCarrierSet.Count )
                    continue;

                if ( !dstCarrierSet.Contains( srcCarrierSet[ indexes[ i ] ] ) )
                    dstCarrierSet.Add( srcCarrierSet[ indexes[ i ] ] );
            }

        }

        public static void GetProducePropagationCarrier( int index_0base, List<UMacroProduceCarrierPropagation> srcCarrierSet, List<UMacro> script, List<UMacroProduceCarrierPropagation> dstCarrierSet )
        {
            int[] indexes = GetMergedNearbyIndexes( index_0base, script );
            if ( indexes == null || indexes.Length <= 0 || dstCarrierSet == null )
                return;

            for ( int i = 0 ; i < indexes.Length ; i++ )
            {
                if ( indexes[ i ] < 0 || indexes[ i ] >= srcCarrierSet.Count )
                    continue;

                if ( !dstCarrierSet.Contains( srcCarrierSet[ indexes[ i ] ] ) )
                    dstCarrierSet.Add( srcCarrierSet[ indexes[ i ] ] );
            }

        }

        public static void GetProduceDrawResultCarrier( int index_0base, List<UMacroProduceCarrierDrawingResult> srcCarrierSet, List<UMacro> script, List<UMacroProduceCarrierDrawingResult> dstCarrierSet )
        {
            int[] indexes = GetMergedNearbyIndexes( index_0base, script );
            if ( indexes == null || indexes.Length <= 0 || dstCarrierSet == null )
                return;

            for ( int i = 0 ; i < indexes.Length ; i++ )
            {
                if ( indexes[ i ] < 0 || indexes[ i ] >= srcCarrierSet.Count )
                    continue;

                if ( !dstCarrierSet.Contains( srcCarrierSet[ indexes[ i ] ] ) )
                    dstCarrierSet.Add( srcCarrierSet[ indexes[ i ] ] );
            }

        }
    }
}
