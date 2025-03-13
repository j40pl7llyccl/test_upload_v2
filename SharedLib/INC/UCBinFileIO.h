#pragma once

#include "UCObject.h"
#include <stdio.h>
#include <vector>
#include "UBaseTypeDecl.h"

#define UCBINFILEIO_ID_RESERVED_SIZE    128

typedef void(*fpUCBinFileIOHandleMem)(tYUX_PVOID pMem);
typedef struct tagUCBinFileIOItem
{
    char            _strID[UCBINFILEIO_ID_RESERVED_SIZE];
    tYUX_PVOID      _pBuff;
    tYUX_I64        _nOffset;
    tYUX_I64        _nSize;
    fpUCBinFileIOHandleMem  _fpHandleMem;
}TUCBinFileIOItem;

class UCBinFileIO : public UCObject
{
protected:
    std::vector<TUCBinFileIOItem *>     _vectorDat;

public:
    UCBinFileIO();
    virtual ~UCBinFileIO();


    void Add(TUCBinFileIOItem item);
    TUCBinFileIOItem* GetItem(size_t index_0) const;
    TUCBinFileIOItem* GetItem(char *pID, size_t &index) const;
    size_t NumOfItems();
    void Clear();

    bool Write(char *pPath);
    bool Read(char *pPath);

public:
    static TUCBinFileIOItem* AllocItem(tYUX_PVOID pBuff, tYUX_I64 offset, tYUX_I64 count, fpUCBinFileIOHandleMem fpHandMem, const char *pID = 0);
    static void ConfigItem(TUCBinFileIOItem *p, tYUX_PVOID pBuff, tYUX_I64 offset, tYUX_I64 count, fpUCBinFileIOHandleMem fpHandMem, const char *pID = 0);
    // will free the pItem mem
    static void FreeItem(TUCBinFileIOItem *pItem);
    template<typename T>
    static T* GetItemData(TUCBinFileIOItem *pItem)
    {
        if (!pItem) return 0;
        return static_cast<T*>(pItem->_pBuff);
    }

    static tYUX_PVOID OpenWrite(char *pPath);
    static bool WriteItem(tYUX_PVOID pHandle, TUCBinFileIOItem *pDat2Write, bool bFreeAfterWrite = true);

    static tYUX_PVOID OpenRead(char *pPath);
    static bool ReadItem(tYUX_PVOID pHandle, TUCBinFileIOItem *pReadDat);

    static void CloseRWHandle(tYUX_PVOID pHandle);

    static void CopyString(char *pDst, size_t nMaxDst, const char *pSrc, size_t nLen);
};
