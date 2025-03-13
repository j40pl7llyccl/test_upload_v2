using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace uIP.Lib.Utility
{
    public class ConcatBlockDataSn
    {
        public Type _DatTp = null;
        public Int32 _nDataSn = -1;

        public ConcatBlockDataSn( Type tp, Int32 nDatSn )
        {
            _DatTp = tp;
            _nDataSn = nDatSn;
        }
    }

    /*
     * 儲存資料結構 UMacroProduceCarrierResult.ResultSet 中的所有元素
     *  -> 一個 value type 的 instance: 轉換成一個元素的一維陣列
     *  -> value type 的陣列 instance: 儲存成 n 個元素的一維陣列
     *  -> 一個 object[] 陣列 instance: 裡面有多個不同 value type 的 一個元素 或 陣列, 會多紀錄在 [ 2 ]裡面
     */

    public static class MacroResultsToConcatBlockBuffer
    {
        public static readonly Int32 HDR_SIZE = sizeof( Int32 ) * 2;
        public static bool PackHdr( IntPtr pBuff, Int32 nOffset, Int32 nMaxSz, Int32 nDataCount, Int32 nSn, ref Int32 nWr )
        {
            nWr = 0;
            if ( (nMaxSz - nOffset) < (sizeof( Int32 ) * 2) )
                return false;
            if ( pBuff == IntPtr.Zero )
                return false;

            unsafe
            {
                Int32* p32 = ( Int32* ) pBuff.ToPointer();
                *p32 = nDataCount;
                *(++p32) = nSn;
            }

            nWr = sizeof( Int32 ) * 2;
            return true;

        }

        public static bool FillHdr( IntPtr pBuff, Int32 nOffset, Int32 nMaxSz, Int32 nSn )
        {
            if ( (nMaxSz - nOffset) < (sizeof( Int32 ) * 2) )
                return false;
            if ( pBuff == IntPtr.Zero )
                return false;

            unsafe
            {
                Int32* p32 = ( Int32* ) pBuff.ToPointer();
                p32[ 1 ] = nSn;
            }

            return true;
        }

        public static readonly Int32 DATADESC_HDR = sizeof( Int32 ) + // macro index
                                                    sizeof( Int32 ) + // result's array index
                                                    sizeof( Int32 ) + // result's array item index, if the data is object[]
                                                    sizeof( Int32 ) + // data uint size
                                                    sizeof( Int32 );  // data total size

        /// <summary>
        /// Remark: Beware of the structure memory must be continue.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="pBuff">buffer to write data</param>
        /// <param name="nOffset">offset from beginning</param>
        /// <param name="nMaxSz">max buffer size</param>
        /// <param name="arrDat">array data to write</param>
        /// <param name="nIndex">index data</param>
        /// <param name="nDatSn">data serial no</param>
        /// <param name="nWr">number of bytes to write</param>
        /// <returns></returns>
        public static bool PackData<T>( IntPtr pBuff, Int32 nOffset, Int32 nMaxSz, T[] arrDat, Int32 nIndex, Int32 nDatSn, Int32 nDatArrayIndex, ref Int32 nWr )
        {
            nWr = 0;
            if ( !typeof( T ).IsValueType || pBuff == IntPtr.Zero || nOffset < 0 ) return false;
            //if ( arrDat == null )
            //    return true;

            //Int32 nTotalNeed = sizeof( Int32 ) // index
            //                   + sizeof( Int32 ) // data serial number
            //                   + sizeof( Int32 ) // data unit
            //                   + sizeof( Int32 ) // array size
            //                   + Marshal.SizeOf( typeof( T ) ) * ( arrDat == null ? 0 : arrDat.Length ); // total payload data size
            Int32 nTotalNeed = DATADESC_HDR
                               + Marshal.SizeOf( typeof( T ) ) * (arrDat == null ? 0 : arrDat.Length); // total payload data size

            // size check
            if ( (nMaxSz - nOffset) < nTotalNeed )
                return false;

            unsafe
            {
                byte* pByte = ( byte* ) pBuff.ToPointer();
                byte* pAccBeg = pByte + nOffset;

                // pack header
                Int32* pInt32 = ( Int32* ) pAccBeg;
                *pInt32 = nIndex;                              // fill macro index info
                *(++pInt32) = nDatSn;                          // fill result array data serial number
                *(++pInt32) = nDatArrayIndex;                  // fill array index of result array item if the item is object[]
                *(++pInt32) = Marshal.SizeOf( typeof( T ) );   // fill data unit size
                *(++pInt32) = (arrDat == null ? 0 : arrDat.Length); // fill array size

                if ( arrDat != null && arrDat.Length > 0 )
                {
                    // copy data
                    pAccBeg = ( byte* ) (++pInt32);
                    Int32 nTpSz = Marshal.SizeOf( typeof( T ) );
                    for ( int i = 0 ; i < arrDat.Length ; i++, pAccBeg += nTpSz )
                    {
                        void* pVoid = ( void* ) pAccBeg;
                        Marshal.StructureToPtr( arrDat[ i ], new IntPtr( pVoid ), false );
                    }
                }
            }

            nWr = nTotalNeed;
            return true;
        }

        /// <summary>
        /// Remark: Beware of the structure memory must be continue.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="pBuff">buffer to write data</param>
        /// <param name="nOffset">offset from beginning<</param>
        /// <param name="nMaxSz">max buffer size</param>
        /// <param name="dat">data to write</param>
        /// <param name="nIndex">index data</param>
        /// <param name="nDatSn">data serial no</param>
        /// <param name="nWr">number of bytes to write</param>
        /// <returns></returns>
        public static bool PackData<T>( IntPtr pBuff, Int32 nOffset, Int32 nMaxSz, T dat, Int32 nIndex, Int32 nDatSn, Int32 nDatArrayIndex, ref Int32 nWr )
        {
            nWr = 0;
            if ( !typeof( T ).IsValueType || pBuff == IntPtr.Zero || nOffset < 0 ) return false;

            //Int32 nTotalNeed = sizeof( Int32 ) // index
            //                   + sizeof( Int32 ) // data serial number
            //                   + sizeof( Int32 ) // data unit
            //                   + sizeof( Int32 ) // array size
            //                   + Marshal.SizeOf( typeof( T ) ); // total payload data size
            Int32 nTotalNeed = DATADESC_HDR
                               + Marshal.SizeOf( typeof( T ) ); // total payload data size

            // size check
            if ( (nMaxSz - nOffset) < nTotalNeed )
                return false;

            unsafe
            {
                byte* pByte = ( byte* ) pBuff.ToPointer();
                byte* pAccBeg = pByte + nOffset;

                // pack header
                Int32* pInt32 = ( Int32* ) pAccBeg;
                *pInt32 = nIndex;                              // fill index value
                *(++pInt32) = nDatSn;                        // fill data serial number
                *(++pInt32) = nDatArrayIndex;                // filll array index of result array item if the item is object[]
                *(++pInt32) = Marshal.SizeOf( typeof( T ) ); // fill data unit size
                *(++pInt32) = 1; // fill array length

                // copy data
                pAccBeg = ( byte* ) (++pInt32);
                void* pVoid = ( void* ) pAccBeg;
                Marshal.StructureToPtr( dat, new IntPtr( pVoid ), false );
            }

            nWr = nTotalNeed;
            return true;
        }

        public static bool PackData( IntPtr pBuff, Int32 nOffset, Int32 nMaxSz, Type tp, Array arrDat, Int32 nIndex, Int32 nDatSn, Int32 nDatArrayIndex, ref Int32 nWr )
        {
            nWr = 0;
            if ( tp == null || !tp.IsValueType || pBuff == IntPtr.Zero || nOffset < 0 ) return false;

            Int32 nTotalNeed = DATADESC_HDR
                               + Marshal.SizeOf( tp ) * (arrDat == null ? 0 : arrDat.Length); // total payload data size

            // size check
            if ( (nMaxSz - nOffset) < nTotalNeed )
                return false;

            unsafe
            {
                byte* pByte = ( byte* ) pBuff.ToPointer();
                byte* pAccBeg = pByte + nOffset;

                // pack header
                Int32* pInt32 = ( Int32* ) pAccBeg;
                *pInt32 = nIndex;                     // fill index value
                *(++pInt32) = nDatSn;                 // fill data serial number
                *(++pInt32) = nDatArrayIndex;         // fill array index of result array item if the item is object[]
                *(++pInt32) = Marshal.SizeOf( tp );   // fill data unit size
                *(++pInt32) = arrDat == null || arrDat.Length <= 0 ? 0 : arrDat.Length; // fill array length

                if ( arrDat != null && arrDat.Length > 0 )
                {
                    // copy data
                    pAccBeg = ( byte* ) (++pInt32);
                    Int32 nTpSz = Marshal.SizeOf( tp );
                    for ( int i = 0 ; i < arrDat.Length ; i++, pAccBeg += nTpSz )
                    {
                        void* pVoid = ( void* ) pAccBeg;
                        Marshal.StructureToPtr( arrDat.GetValue( i ), new IntPtr( pVoid ), false );
                    }
                }
            }

            nWr = nTotalNeed;
            return true;
        }

        // unpack the data which match acceptTps types in a nWhichIndex of a macro
        public static List<ConcatBlockReloadedData> Unpack( IntPtr pBuff, Int32 nMaxSz, Type[] acceptTps, Int32 nWhichIndex, out Int32 nSn )
        {
            nSn = -1;
            if ( pBuff == IntPtr.Zero || nMaxSz <= 0 || acceptTps == null || acceptTps.Length <= 0 )
                return null;

            List<ConcatBlockReloadedData> ret = new List<ConcatBlockReloadedData>();
            Int32[] referenceTpSz = new Int32[ acceptTps.Length ];
            for ( int i = 0 ; i < acceptTps.Length ; i++ )
            {
                if ( acceptTps[ i ] == null || !acceptTps[ i ].IsValueType )
                {
                    referenceTpSz[ i ] = 0; continue;
                }
                referenceTpSz[ i ] = Marshal.SizeOf( acceptTps[ i ] );
            }

            unsafe
            {
                Int32* p32 = ( Int32* ) pBuff.ToPointer();
                nSn = p32[ 1 ]; // get the serial no

                byte* p8 = ( byte* ) pBuff.ToPointer();
                Int32 nMv = 0, nCurrTotal = 0;

                byte* p8Beg = p8 + (HDR_SIZE); // skip the header
                nCurrTotal = HDR_SIZE; // header = data count + serial no
                for ( int i = 0 ; i < *p32 ; i++ )
                {
                    Int32* pItemBeg32 = ( Int32* ) p8Beg;

                    Int32 nIndex = pItemBeg32[ 0 ];
                    Int32 nDatSn = pItemBeg32[ 1 ];
                    Int32 nArrIndex = pItemBeg32[ 2 ];
                    Int32 nUnitSz = pItemBeg32[ 3 ];
                    Int32 nArrsz = pItemBeg32[ 4 ];
                    nMv = DATADESC_HDR + nUnitSz * nArrsz;

                    Int32 nTpSzMatchIdx = -1;
                    for ( int j = 0 ; j < referenceTpSz.Length ; j++ )
                    {
                        if ( referenceTpSz[ j ] <= 0 ) continue;
                        if ( referenceTpSz[ j ] == nUnitSz )
                        {
                            nTpSzMatchIdx = j; break;
                        }
                    }

                    if ( nIndex == nWhichIndex &&         // check index
                         nTpSzMatchIdx >= 0 &&            // value type size check
                         (nCurrTotal + nMv) <= nMaxSz )   // buffer boundary check
                    {

                        // array size check
                        if ( nArrsz > 0 )
                        {
                            Array arr = Array.CreateInstance( acceptTps[ nTpSzMatchIdx ], nArrsz );
                            byte* pDataBeg = ( byte* ) (&pItemBeg32[ 4 ]);
                            for ( int j = 0 ; j < nArrsz ; j++ )
                            {
                                arr.SetValue( Marshal.PtrToStructure( new IntPtr( ( void* ) pDataBeg ), acceptTps[ nTpSzMatchIdx ] ), j );
                                pDataBeg += nUnitSz;
                            }
                            ret.Add( new ConcatBlockReloadedData( nIndex, nDatSn, arr, acceptTps[ nTpSzMatchIdx ], nTpSzMatchIdx, nArrIndex ) );
                        }
                    }

                    // check boundary
                    nCurrTotal += nMv;
                    if ( nCurrTotal >= nMaxSz )
                        break;
                    // move pointer to next position
                    p8Beg += nMv;
                }
            }

            return ret;
        }

        /*
         * 解出資料
         *  - 由 data 中所儲存的 Type 與 對應到UMacroProduceCarrierResult.ResultSet index資料進行解析
         *  - 符合 macro index ( nWhichIndex )
         */
        public static List<ConcatBlockReloadedData> Unpack( IntPtr pBuff, Int32 nMaxSz, ConcatBlockDataSn[] data, Int32 nWhichIndex, out Int32 nSn )
        {
            nSn = -1;
            if ( pBuff == IntPtr.Zero || nMaxSz <= 0 || data == null || data.Length <= 0 )
                return null;

            List<ConcatBlockReloadedData> ret = new List<ConcatBlockReloadedData>();

            unsafe
            {
                Int32* p32 = ( Int32* ) pBuff.ToPointer();
                nSn = p32[ 1 ]; // get the serial no

                byte* p8 = ( byte* ) pBuff.ToPointer();
                Int32 nMv = 0, nCurrTotal = 0;

                byte* p8Beg = p8 + (sizeof( Int32 ) * 2); // skip the header
                nCurrTotal = sizeof( Int32 ) * 2; // header = data count + serial no
                for ( int i = 0 ; i < *p32 ; i++ )
                {
                    Int32* pItemBeg32 = ( Int32* ) p8Beg;

                    Int32 nIndex = pItemBeg32[ 0 ];
                    Int32 nDatSn = pItemBeg32[ 1 ];
                    Int32 nArrIndex = pItemBeg32[ 2 ];
                    Int32 nUnitSz = pItemBeg32[ 3 ];
                    Int32 nArrsz = pItemBeg32[ 4 ];
                    //nMv = sizeof( Int32 ) * 4 + nUnitSz * nArrsz;
                    nMv = DATADESC_HDR + nUnitSz * nArrsz;

                    Int32 nDatSnMatchIdx = -1;
                    Type nDatSnMatchType = null;
                    for ( int j = 0 ; j < data.Length ; j++ )
                    {
                        if ( data[ j ]._nDataSn == nDatSn && Marshal.SizeOf( data[ j ]._DatTp ) == nUnitSz )
                        {
                            nDatSnMatchIdx = data[ j ]._nDataSn;
                            nDatSnMatchType = data[ j ]._DatTp;
                            break;
                        }
                    }

                    if ( nIndex == nWhichIndex &&         // check index
                         nDatSnMatchIdx >= 0 &&          // data serial no check
                         nDatSnMatchType != null &&      // data type size check
                         (nCurrTotal + nMv) <= nMaxSz )   // buffer boundary check
                    {

                        // array size check
                        if ( nArrsz > 0 )
                        {
                            Array arr = Array.CreateInstance( nDatSnMatchType, nArrsz );
                            byte* pDataBeg = ( byte* ) (&pItemBeg32[ 4 ]);
                            for ( int j = 0 ; j < nArrsz ; j++ )
                            {
                                arr.SetValue( Marshal.PtrToStructure( new IntPtr( ( void* ) pDataBeg ), nDatSnMatchType ), j );
                                pDataBeg += nUnitSz;
                            }
                            ret.Add( new ConcatBlockReloadedData( nIndex, nDatSn, arr, nDatSnMatchType, nDatSnMatchIdx, nArrIndex ) );
                        }
                    }

                    // check boundary
                    nCurrTotal += nMv;
                    if ( nCurrTotal >= nMaxSz )
                        break;
                    // move pointer to next position
                    p8Beg += nMv;
                }
            }

            return ret;
        }
    }

    public class ConcatBlockReloadedData
    {
        public Int32 _nIndex = -1;
        public Int32 _nDataSn = -1;
        public Int32 _nIndexOfItemIsArr = -1; // the data item is array, this value store the index of a data item
        public Array _Data = null; // Array
        public Type _DatTp = null;
        public Int32 _nAttTpIdx = -1;

        public ConcatBlockReloadedData() { }
        public ConcatBlockReloadedData( Int32 nIdx, Int32 nDatSn, Array dat, Type tp, Int32 nAttTpIdx )
        {
            _nIndex = nIdx;
            _nDataSn = nDatSn;
            _Data = dat;
            _DatTp = tp;
            _nAttTpIdx = nAttTpIdx;
        }
        public ConcatBlockReloadedData( Int32 nIdx, Int32 nDatSn, Array dat, Type tp, Int32 nAttTpIdx, Int32 nIndexOfItemIsArr )
        {
            _nIndex = nIdx;
            _nDataSn = nDatSn;
            _nIndexOfItemIsArr = nIndexOfItemIsArr;
            _Data = dat;
            _DatTp = tp;
            _nAttTpIdx = nAttTpIdx;
        }

        public T[] GetData<T>()
        {
            if ( _DatTp == null || _Data == null || typeof( T ) != _DatTp )
                return null;

            return (( T[] ) _Data);
        }
    }

}
