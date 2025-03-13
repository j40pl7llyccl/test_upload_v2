using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace uIP.LibBase.Utility
{
    public class ConcatBlockDataSn
    {
        public Type _DatTp = null;
        public int _nDataSn = -1;

        public ConcatBlockDataSn(Type tp, int nDatSn)
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
        public static readonly int HDR_SIZE = sizeof(int) * 2;
        public static bool PackHdr(IntPtr pBuff, int nOffset, int nMaxSz, int nDataCount, int nSn, ref int nWr)
        {
            nWr = 0;
            if (nMaxSz - nOffset < sizeof(int) * 2)
                return false;
            if (pBuff == IntPtr.Zero)
                return false;

            unsafe
            {
                int* p32 = (int*)pBuff.ToPointer();
                *p32 = nDataCount;
                *++p32 = nSn;
            }

            nWr = sizeof(int) * 2;
            return true;

        }

        public static bool FillHdr(IntPtr pBuff, int nOffset, int nMaxSz, int nSn)
        {
            if (nMaxSz - nOffset < sizeof(int) * 2)
                return false;
            if (pBuff == IntPtr.Zero)
                return false;

            unsafe
            {
                int* p32 = (int*)pBuff.ToPointer();
                p32[1] = nSn;
            }

            return true;
        }

        public static readonly int DATADESC_HDR = sizeof(int) + // macro index
                                                    sizeof(int) + // result's array index
                                                    sizeof(int) + // result's array item index, if the data is object[]
                                                    sizeof(int) + // data uint size
                                                    sizeof(int);  // data total size

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
        public static bool PackData<T>(IntPtr pBuff, int nOffset, int nMaxSz, T[] arrDat, int nIndex, int nDatSn, int nDatArrayIndex, ref int nWr)
        {
            nWr = 0;
            if (!typeof(T).IsValueType || pBuff == IntPtr.Zero || nOffset < 0) return false;
            //if ( arrDat == null )
            //    return true;

            //Int32 nTotalNeed = sizeof( Int32 ) // index
            //                   + sizeof( Int32 ) // data serial number
            //                   + sizeof( Int32 ) // data unit
            //                   + sizeof( Int32 ) // array size
            //                   + Marshal.SizeOf( typeof( T ) ) * ( arrDat == null ? 0 : arrDat.Length ); // total payload data size
            int nTotalNeed = DATADESC_HDR
                               + Marshal.SizeOf(typeof(T)) * (arrDat == null ? 0 : arrDat.Length); // total payload data size

            // size check
            if (nMaxSz - nOffset < nTotalNeed)
                return false;

            unsafe
            {
                byte* pByte = (byte*)pBuff.ToPointer();
                byte* pAccBeg = pByte + nOffset;

                // pack header
                int* pInt32 = (int*)pAccBeg;
                *pInt32 = nIndex;                              // fill macro index info
                *++pInt32 = nDatSn;                          // fill result array data serial number
                *++pInt32 = nDatArrayIndex;                  // fill array index of result array item if the item is object[]
                *++pInt32 = Marshal.SizeOf(typeof(T));   // fill data unit size
                *++pInt32 = arrDat == null ? 0 : arrDat.Length; // fill array size

                if (arrDat != null && arrDat.Length > 0)
                {
                    // copy data
                    pAccBeg = (byte*)(++pInt32);
                    int nTpSz = Marshal.SizeOf(typeof(T));
                    for (int i = 0; i < arrDat.Length; i++, pAccBeg += nTpSz)
                    {
                        void* pVoid = pAccBeg;
                        Marshal.StructureToPtr(arrDat[i], new IntPtr(pVoid), false);
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
        public static bool PackData<T>(IntPtr pBuff, int nOffset, int nMaxSz, T dat, int nIndex, int nDatSn, int nDatArrayIndex, ref int nWr)
        {
            nWr = 0;
            if (!typeof(T).IsValueType || pBuff == IntPtr.Zero || nOffset < 0) return false;

            //Int32 nTotalNeed = sizeof( Int32 ) // index
            //                   + sizeof( Int32 ) // data serial number
            //                   + sizeof( Int32 ) // data unit
            //                   + sizeof( Int32 ) // array size
            //                   + Marshal.SizeOf( typeof( T ) ); // total payload data size
            int nTotalNeed = DATADESC_HDR
                               + Marshal.SizeOf(typeof(T)); // total payload data size

            // size check
            if (nMaxSz - nOffset < nTotalNeed)
                return false;

            unsafe
            {
                byte* pByte = (byte*)pBuff.ToPointer();
                byte* pAccBeg = pByte + nOffset;

                // pack header
                int* pInt32 = (int*)pAccBeg;
                *pInt32 = nIndex;                              // fill index value
                *++pInt32 = nDatSn;                        // fill data serial number
                *++pInt32 = nDatArrayIndex;                // filll array index of result array item if the item is object[]
                *++pInt32 = Marshal.SizeOf(typeof(T)); // fill data unit size
                *++pInt32 = 1; // fill array length

                // copy data
                pAccBeg = (byte*)(++pInt32);
                void* pVoid = pAccBeg;
                Marshal.StructureToPtr(dat, new IntPtr(pVoid), false);
            }

            nWr = nTotalNeed;
            return true;
        }

        public static bool PackData(IntPtr pBuff, int nOffset, int nMaxSz, Type tp, Array arrDat, int nIndex, int nDatSn, int nDatArrayIndex, ref int nWr)
        {
            nWr = 0;
            if (tp == null || !tp.IsValueType || pBuff == IntPtr.Zero || nOffset < 0) return false;

            int nTotalNeed = DATADESC_HDR
                               + Marshal.SizeOf(tp) * (arrDat == null ? 0 : arrDat.Length); // total payload data size

            // size check
            if (nMaxSz - nOffset < nTotalNeed)
                return false;

            unsafe
            {
                byte* pByte = (byte*)pBuff.ToPointer();
                byte* pAccBeg = pByte + nOffset;

                // pack header
                int* pInt32 = (int*)pAccBeg;
                *pInt32 = nIndex;                     // fill index value
                *++pInt32 = nDatSn;                 // fill data serial number
                *++pInt32 = nDatArrayIndex;         // fill array index of result array item if the item is object[]
                *++pInt32 = Marshal.SizeOf(tp);   // fill data unit size
                *++pInt32 = arrDat == null || arrDat.Length <= 0 ? 0 : arrDat.Length; // fill array length

                if (arrDat != null && arrDat.Length > 0)
                {
                    // copy data
                    pAccBeg = (byte*)(++pInt32);
                    int nTpSz = Marshal.SizeOf(tp);
                    for (int i = 0; i < arrDat.Length; i++, pAccBeg += nTpSz)
                    {
                        void* pVoid = pAccBeg;
                        Marshal.StructureToPtr(arrDat.GetValue(i), new IntPtr(pVoid), false);
                    }
                }
            }

            nWr = nTotalNeed;
            return true;
        }

        // unpack the data which match acceptTps types in a nWhichIndex of a macro
        public static List<ConcatBlockReloadedData> Unpack(IntPtr pBuff, int nMaxSz, Type[] acceptTps, int nWhichIndex, out int nSn)
        {
            nSn = -1;
            if (pBuff == IntPtr.Zero || nMaxSz <= 0 || acceptTps == null || acceptTps.Length <= 0)
                return null;

            List<ConcatBlockReloadedData> ret = new List<ConcatBlockReloadedData>();
            int[] referenceTpSz = new int[acceptTps.Length];
            for (int i = 0; i < acceptTps.Length; i++)
            {
                if (acceptTps[i] == null || !acceptTps[i].IsValueType)
                {
                    referenceTpSz[i] = 0; continue;
                }
                referenceTpSz[i] = Marshal.SizeOf(acceptTps[i]);
            }

            unsafe
            {
                int* p32 = (int*)pBuff.ToPointer();
                nSn = p32[1]; // get the serial no

                byte* p8 = (byte*)pBuff.ToPointer();
                int nMv = 0, nCurrTotal = 0;

                byte* p8Beg = p8 + HDR_SIZE; // skip the header
                nCurrTotal = HDR_SIZE; // header = data count + serial no
                for (int i = 0; i < *p32; i++)
                {
                    int* pItemBeg32 = (int*)p8Beg;

                    int nIndex = pItemBeg32[0];
                    int nDatSn = pItemBeg32[1];
                    int nArrIndex = pItemBeg32[2];
                    int nUnitSz = pItemBeg32[3];
                    int nArrsz = pItemBeg32[4];
                    nMv = DATADESC_HDR + nUnitSz * nArrsz;

                    int nTpSzMatchIdx = -1;
                    for (int j = 0; j < referenceTpSz.Length; j++)
                    {
                        if (referenceTpSz[j] <= 0) continue;
                        if (referenceTpSz[j] == nUnitSz)
                        {
                            nTpSzMatchIdx = j; break;
                        }
                    }

                    if (nIndex == nWhichIndex &&         // check index
                         nTpSzMatchIdx >= 0 &&            // value type size check
                         nCurrTotal + nMv <= nMaxSz)   // buffer boundary check
                    {

                        // array size check
                        if (nArrsz > 0)
                        {
                            Array arr = Array.CreateInstance(acceptTps[nTpSzMatchIdx], nArrsz);
                            byte* pDataBeg = (byte*)&pItemBeg32[4];
                            for (int j = 0; j < nArrsz; j++)
                            {
                                arr.SetValue(Marshal.PtrToStructure(new IntPtr(pDataBeg), acceptTps[nTpSzMatchIdx]), j);
                                pDataBeg += nUnitSz;
                            }
                            ret.Add(new ConcatBlockReloadedData(nIndex, nDatSn, arr, acceptTps[nTpSzMatchIdx], nTpSzMatchIdx, nArrIndex));
                        }
                    }

                    // check boundary
                    nCurrTotal += nMv;
                    if (nCurrTotal >= nMaxSz)
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
        public static List<ConcatBlockReloadedData> Unpack(IntPtr pBuff, int nMaxSz, ConcatBlockDataSn[] data, int nWhichIndex, out int nSn)
        {
            nSn = -1;
            if (pBuff == IntPtr.Zero || nMaxSz <= 0 || data == null || data.Length <= 0)
                return null;

            List<ConcatBlockReloadedData> ret = new List<ConcatBlockReloadedData>();

            unsafe
            {
                int* p32 = (int*)pBuff.ToPointer();
                nSn = p32[1]; // get the serial no

                byte* p8 = (byte*)pBuff.ToPointer();
                int nMv = 0, nCurrTotal = 0;

                byte* p8Beg = p8 + sizeof(int) * 2; // skip the header
                nCurrTotal = sizeof(int) * 2; // header = data count + serial no
                for (int i = 0; i < *p32; i++)
                {
                    int* pItemBeg32 = (int*)p8Beg;

                    int nIndex = pItemBeg32[0];
                    int nDatSn = pItemBeg32[1];
                    int nArrIndex = pItemBeg32[2];
                    int nUnitSz = pItemBeg32[3];
                    int nArrsz = pItemBeg32[4];
                    //nMv = sizeof( Int32 ) * 4 + nUnitSz * nArrsz;
                    nMv = DATADESC_HDR + nUnitSz * nArrsz;

                    int nDatSnMatchIdx = -1;
                    Type nDatSnMatchType = null;
                    for (int j = 0; j < data.Length; j++)
                    {
                        if (data[j]._nDataSn == nDatSn && Marshal.SizeOf(data[j]._DatTp) == nUnitSz)
                        {
                            nDatSnMatchIdx = data[j]._nDataSn;
                            nDatSnMatchType = data[j]._DatTp;
                            break;
                        }
                    }

                    if (nIndex == nWhichIndex &&         // check index
                         nDatSnMatchIdx >= 0 &&          // data serial no check
                         nDatSnMatchType != null &&      // data type size check
                         nCurrTotal + nMv <= nMaxSz)   // buffer boundary check
                    {

                        // array size check
                        if (nArrsz > 0)
                        {
                            Array arr = Array.CreateInstance(nDatSnMatchType, nArrsz);
                            byte* pDataBeg = (byte*)&pItemBeg32[4];
                            for (int j = 0; j < nArrsz; j++)
                            {
                                arr.SetValue(Marshal.PtrToStructure(new IntPtr(pDataBeg), nDatSnMatchType), j);
                                pDataBeg += nUnitSz;
                            }
                            ret.Add(new ConcatBlockReloadedData(nIndex, nDatSn, arr, nDatSnMatchType, nDatSnMatchIdx, nArrIndex));
                        }
                    }

                    // check boundary
                    nCurrTotal += nMv;
                    if (nCurrTotal >= nMaxSz)
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
        public int _nIndex = -1;
        public int _nDataSn = -1;
        public int _nIndexOfItemIsArr = -1; // the data item is array, this value store the index of a data item
        public Array _Data = null; // Array
        public Type _DatTp = null;
        public int _nAttTpIdx = -1;

        public ConcatBlockReloadedData() { }
        public ConcatBlockReloadedData(int nIdx, int nDatSn, Array dat, Type tp, int nAttTpIdx)
        {
            _nIndex = nIdx;
            _nDataSn = nDatSn;
            _Data = dat;
            _DatTp = tp;
            _nAttTpIdx = nAttTpIdx;
        }
        public ConcatBlockReloadedData(int nIdx, int nDatSn, Array dat, Type tp, int nAttTpIdx, int nIndexOfItemIsArr)
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
            if (_DatTp == null || _Data == null || typeof(T) != _DatTp)
                return null;

            return (T[])_Data;
        }
    }

}
