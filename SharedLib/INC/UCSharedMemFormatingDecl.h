#ifndef UCSharedMemFormatingDecl_h__
#define UCSharedMemFormatingDecl_h__

#include "UBaseTypeDecl.h"
#include <string>

#define UCSHAREDMEM_CONCAT_STR     "$$"

enum tUCSHAREDMEM_TYPE
{
    UCSHAREDMEM_I32 = 0,
    UCSHAREDMEM_DOUBLE,
    UCSHAREDMEM_U8,
};

// Remark
//  - Frobid to use memset clearing memory, cause there are object inside
typedef struct tagShMemAccItem
{
    tYUX_PVOID  _pItself;
    tYUX_PVOID  _pUsedInfoAddr;     // point to an item of _vectorShMemUsedRec, more like its owner
    char       *_pShMemName;        // GivenName() in UCDataSync
    tYUX_PVOID  _pShMemObjInstance; // point to an item of _vectorI32ShMems/ _vectorDoubleShMems/ _vectorU8ShMems
    int         _nShMemType;        // I32 = 0, double = 1, U8 = 2
    int         _nSizeofT;          // sizeof item
    tYUX_PVOID  _pMuxHandle;        // Mutex handle
    tYUX_PVOID  _pBegAddr;          // Access address from offset of array
    tYUX_I64    _nOffset;           // Offset in array
    tYUX_I64    _nSize;             // Requested size

    char       *_pUniqueName;
    std::string _strId;
    std::string _strTypeName;
    std::string _strDescription;
    std::string _strPurpose;

    tagShMemAccItem()
    {
        _pItself = 0;
        _pUsedInfoAddr = 0;
        _pShMemName = 0;
        _pShMemObjInstance = 0;
        _nShMemType = -1;
        _nSizeofT = 0;
        _pMuxHandle = 0;
        _pBegAddr = 0;
        _nOffset = 0;
        _nSize = 0;
        _pUniqueName = 0;
    }
}TShMemAccItem;

typedef struct tagShMemUsedInfo
{
    tYUX_PVOID  _pItself;
    char       *_pShMemName;        // GivenName() in UCDataSync
    tYUX_PVOID  _pShMemObjInstance; // point to an item of _vectorI32ShMems/ _vectorDoubleShMems/ _vectorU8ShMems
    int         _nShMemType;        // I32 = 0, double = 1, U8 = 2
    tYUX_I64    _nNextBegIndex;     // next index begin
    tYUX_I64    _nAvailableCount;   // remainder count
    int         _nSizeofT;          // sizeof item
    tYUX_PVOID  _pMuxHandle;        // mutex handle

    tagShMemUsedInfo()
    {
        _pItself = 0;
        _pShMemName = 0;
        _pShMemObjInstance = 0;
        _nShMemType = -1;
        _nNextBegIndex = 0;
        _nSizeofT = 0;
        _pMuxHandle = 0;
    }
    tagShMemUsedInfo(char *pName, int type/*UCSHAREDMEM_TYPE*/)
    {
        _pItself = 0;
        _pShMemName = pName;
        _pShMemObjInstance = 0;
        _nShMemType = type;
        _nNextBegIndex = 0;
        _nSizeofT = 0;
        _pMuxHandle = 0;
    }

}TShMemUsedInfo;


#endif
