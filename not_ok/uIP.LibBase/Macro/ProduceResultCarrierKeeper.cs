using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using uIP.LibBase.Utility;

namespace uIP.LibBase.Macro
{
    internal class ProduceResultCarrierKeeper
    {
        internal Int32 _nMacroIndex = -1;
        internal Int32 _nResultCarrierSetIndex = -1;
        internal Type _Tp = null;
        internal object _Data = null;
        internal bool _bArray = false;
        internal Int32 _nArrayIndex = -1;
        internal Int32 _nUnitSz = 0;
        internal Int32 _nTotalSz = 0;

        internal ProduceResultCarrierKeeper() { }
        internal ProduceResultCarrierKeeper( Int32 idxOfScript, Int32 idxOfResultSet, Type tp, object dat, bool arrTp )
        {
            _nMacroIndex = idxOfScript; _Tp = tp; _Data = dat; _bArray = arrTp;
            _nResultCarrierSetIndex = idxOfResultSet;

            _nUnitSz = tp == null ? 0 : Marshal.SizeOf( tp );
            if ( arrTp )
            {
                Array arr = dat as Array;
                _nTotalSz = arr == null ? 0 : arr.Length * _nUnitSz;
            }
            else
                _nTotalSz = _nUnitSz;
        }

        internal ProduceResultCarrierKeeper( Int32 idxOfScript, Int32 idxOfResultSet, object dat, int nArrIndex )
        {
            _nMacroIndex = idxOfScript; _Data = dat;
            _nResultCarrierSetIndex = idxOfResultSet;
            _nArrayIndex = nArrIndex;

            if ( dat == null )
                _Tp = null;
            else
            {
                Type tpOfDat = dat.GetType();
                if ( !tpOfDat.IsArray )
                {
                    _Tp = !tpOfDat.IsValueType ? null : tpOfDat;
                    _nUnitSz = _Tp == null ? 0 : Marshal.SizeOf( _Tp );
                }
                else
                {
                    Array arr = dat as Array;
                    object itm0 = arr.GetValue( 0 );
                    _Tp = itm0 == null || !itm0.GetType().IsValueType ? null : itm0.GetType();
                    _nUnitSz = _Tp == null ? 0 : Marshal.SizeOf( _Tp );

                    if ( _nUnitSz > 0 )
                    {
                        int[] dimLens = new int[ arr.Rank ];
                        Int32 nTotalItems = 1;
                        for ( int i = 0 ; i < dimLens.Length ; i++ )
                        {
                            dimLens[ i ] = arr.GetLength( i );
                            nTotalItems *= dimLens[ i ];
                        }
                        _nTotalSz = _nUnitSz * nTotalItems;
                        _bArray = true;
                    }
                }
            }
        }

        internal static byte[] ProduceResultCarriersToByteArray( bool bGetGroup, List<UMacroProduceCarrierResult> resultCarrierSet, List<UMacro> aScript )
        {
            if ( resultCarrierSet == null || resultCarrierSet.Count <= 0 || aScript == null )
                return null;

            byte[] repoResult = null;
            List<ProduceResultCarrierKeeper> tmp = new List<ProduceResultCarrierKeeper>();
            if ( resultCarrierSet != null )
            {
                //
                // filter the accept data
                //

                // get which results must be processed
                List<UMacroProduceCarrierResult> for_repo = new List<UMacroProduceCarrierResult>();
                for ( int i = 0 ; i < resultCarrierSet.Count ; i++ )
                {
                    if ( resultCarrierSet[ i ] == null )
                        continue;
                    if ( !aScript[ resultCarrierSet[ i ].IndexOfScript ].ExtractingResultToPack ) // user must be enable the flag
                        continue;

                    if ( bGetGroup )
                        UScript.GetProduceCarrierResult( resultCarrierSet[ i ].IndexOfScript, resultCarrierSet, aScript, for_repo );
                    else
                    {
                        if ( !for_repo.Contains( resultCarrierSet[ i ] ) )
                            for_repo.Add( resultCarrierSet[ i ] );
                    }
                }
                // pack report format
                for ( int i = 0 ; i < for_repo.Count ; i++ )
                {
                    if ( for_repo[ i ] == null )
                        continue;
                    if ( for_repo[ i ].ResultSet == null )
                        continue;

                    // processing the result
                    for ( int j = 0 ; j < for_repo[ i ].ResultSet.Length ; j++ )
                    {
                        if ( for_repo[ i ].ResultSet[ j ] == null )
                            continue;
                        if ( for_repo[ i ].ResultSet[ j ].Tp == null ||
                             for_repo[ i ].ResultSet[ j ].Data == null )
                            continue;
                        if ( for_repo[ i ].ResultSet[ j ].Tp.IsValueType ) // single value
                        {
                            tmp.Add( new ProduceResultCarrierKeeper( for_repo[ i ].IndexOfScript, j,
                                                                           for_repo[ i ].ResultSet[ j ].Tp,
                                                                           for_repo[ i ].ResultSet[ j ].Data,
                                                                           false ) );
                        }
                        else if ( for_repo[ i ].ResultSet[ j ].Data is Array && for_repo[ i ].ResultSet[ j ].Tp == typeof( object[] ) )
                        {
                            Array arr = for_repo[ i ].ResultSet[ j ].Data as Array;
                            // only accept one dimension array
                            if ( arr != null && arr.Rank == 1 )
                            {
                                for ( int k = 0 ; k < arr.Length ; k++ )
                                {
                                    tmp.Add( new ProduceResultCarrierKeeper( for_repo[ i ].IndexOfScript, j,
                                                                                   arr.GetValue( k ), k ) );
                                }
                            }
                        }
                        else if ( for_repo[ i ].ResultSet[ j ].Data is Array )
                        {
                            Array arr = for_repo[ i ].ResultSet[ j ].Data as Array;
                            object i0 = arr.GetValue( 0 );
                            Type tt = i0 == null ? null : i0.GetType();
                            if ( tt != null && tt.IsValueType )
                                tmp.Add( new ProduceResultCarrierKeeper( for_repo[ i ].IndexOfScript, j,
                                                                               tt,
                                                                               for_repo[ i ].ResultSet[ j ].Data,
                                                                               true ) );
                        }
                    }
                }

                //
                // calc the total size
                //
                Int32 nTotalCnt = 0;
                Int32 nOkCnt = 0;
                nTotalCnt += (sizeof( Int32 ) * 2); // total count + serial no
                for ( int i = 0 ; i < tmp.Count ; i++ )
                {
                    if ( tmp[ i ] == null ) continue;
                    nTotalCnt += MacroResultsToConcatBlockBuffer.DATADESC_HDR;
                    nTotalCnt += tmp[ i ]._nTotalSz; // data size
                    nOkCnt++;
                }
                //
                // fill report data
                //
                if ( nTotalCnt > 0 )
                {
                    bool bErr = false;
                    repoResult = new byte[ nTotalCnt ];
                    unsafe
                    {
                        fixed ( byte* p8 = repoResult )
                        {
                            void* pVoid = ( void* ) p8;
                            Int32 nCurrWr = 0;
                            Int32 nOffset = 0;
                            // write header -- item count
                            MacroResultsToConcatBlockBuffer.PackHdr( new IntPtr( pVoid ), 0, repoResult.Length, nOkCnt, 0, ref nCurrWr );
                            nOffset += nCurrWr;
                            // write each item
                            for ( int i = 0 ; i < tmp.Count ; i++ )
                            {
                                if ( tmp[ i ] == null )
                                    continue;
                                nCurrWr = 0;
                                if ( !tmp[ i ]._bArray )
                                {
                                    // make a copy
                                    Array arr = Array.CreateInstance( tmp[ i ]._Tp, 1 );
                                    arr.SetValue( tmp[ i ]._Data, 0 );
                                    if ( !MacroResultsToConcatBlockBuffer.PackData( new IntPtr( pVoid ), nOffset, repoResult.Length, tmp[ i ]._Tp, arr, tmp[ i ]._nMacroIndex, tmp[ i ]._nResultCarrierSetIndex, tmp[ i ]._nArrayIndex, ref nCurrWr ) )
                                    {
                                        bErr = true; break;
                                    }
                                }
                                else
                                {
                                    if ( !MacroResultsToConcatBlockBuffer.PackData( new IntPtr( pVoid ), nOffset, repoResult.Length, tmp[ i ]._Tp, tmp[ i ]._Data as Array, tmp[ i ]._nMacroIndex, tmp[ i ]._nResultCarrierSetIndex, tmp[ i ]._nArrayIndex, ref nCurrWr ) )
                                    {
                                        bErr = true; break;
                                    }
                                }
                                nOffset += nCurrWr;
                            }
                        }
                    }
                    if ( bErr )
                        repoResult = null;
                }
            }
            return repoResult;
        }
    }
}
