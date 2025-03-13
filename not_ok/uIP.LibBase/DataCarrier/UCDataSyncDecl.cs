using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.LibBase.DataCarrier
{
    public delegate string fpUCDataSync32IniWriteCallback<T>( IntPtr pDatBeg, Int64 nItemOffset, Int64 nCount, Int32 nItemSizeof, string concatStr ); // report all value and in concatStr seprating
    public delegate void fpUCDataSync32IniReadCallback<T>( string pRdDat, IntPtr pCurWrOneDatAddr );

    public static class UCDataSyncDecl
    {
        public const string UCDATASYNCWIN32_SHMEM_PREFIX = "Local\\";
    }

    public class TUCDataItemInfo
    {
        public Int64 _nOffset = 0;
        public Int64 _nSize = 0;
        public TUCDataItemInfo() { }
        public TUCDataItemInfo( Int64 offset, Int64 size ) { _nOffset = offset; _nSize = size; }

        public static TUCDataItemInfo MakeNewOne( TUCDataItemInfo src )
        {
            return new TUCDataItemInfo( src._nOffset, src._nSize );
        }
    }

    // Remark
    //  - Frobid to use memset clearing memory, cause there are object inside
    public class TShMemAccItem
    {
        public object _pItself;
        public object _pUsedInfoAddr;     // point to an item of _vectorShMemUsedRec, more like its owner
        public string _pShMemName;        // GivenName() in UCDataSync
        public object _pShMemObjInstance; // point to an item of _vectorI32ShMems/ _vectorDoubleShMems/ _vectorU8ShMems
        public string _strShMemType;        // I32 = 0, double = 1, U8 = 2
        public int _nSizeofT;          // sizeof item
        public IntPtr _pMuxHandle;        // Mutex handle
        public IntPtr _pBegAddr;          // Access address from offset of array
        public Int64 _nOffset;           // Offset in array
        public Int64 _nSize;             // Requested size

        public string _pUniqueName;
        public string _strId;
        public string _strTypeName;
        public string _strDescription;
        public string _strPurpose;

        public TShMemAccItem()
        {
            _pItself = null;
            _pUsedInfoAddr = null;
            _pShMemName = null;
            _pShMemObjInstance = null;
            _strShMemType = null;
            _nSizeofT = 0;
            _pMuxHandle = IntPtr.Zero;
            _pBegAddr = IntPtr.Zero;
            _nOffset = 0;
            _nSize = 0;
            _pUniqueName = null;
            _strId = null;
            _strTypeName = null;
            _strDescription = null;
            _strPurpose = null;
        }
    }

    public class TShMemUsedInfo
    {
        public object _pItself;
        public string _pShMemName;        // GivenName() in UCDataSync
        public object _pShMemObjInstance; // point to an item of _vectorI32ShMems/ _vectorDoubleShMems/ _vectorU8ShMems
        public string _strShMemType;        // I32 = 0, double = 1, U8 = 2
        public Int64 _nNextBegIndex;     // next index begin
        public Int64 _nAvailableCount;   // remainder count
        public Int32 _nSizeofT;          // sizeof item
        public IntPtr _pMuxHandle;        // mutex handle

        public TShMemUsedInfo()
        {
            _pItself = null;
            _pShMemName = null;
            _pShMemObjInstance = null;
            _strShMemType = null;
            _nNextBegIndex = 0;
            _nSizeofT = 0;
            _pMuxHandle = IntPtr.Zero;
        }
        public TShMemUsedInfo( string pName, string type/*UCSHAREDMEM_TYPE*/)
        {
            _pItself = null;
            _pShMemName = pName;
            _pShMemObjInstance = null;
            _strShMemType = type;
            _nNextBegIndex = 0;
            _nSizeofT = 0;
            _pMuxHandle = IntPtr.Zero;
        }
    }
}
