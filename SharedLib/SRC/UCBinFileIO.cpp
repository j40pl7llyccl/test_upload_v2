#include "stdafx.h"
#include "UCBinFileIO.h"

UCBinFileIO::UCBinFileIO() : UCObject()
{
    _strClassRttiName = typeid(*this).name();
}
UCBinFileIO::~UCBinFileIO() { Clear(); }

void UCBinFileIO::Add(TUCBinFileIOItem item)
{
    TUCBinFileIOItem *pItem = new TUCBinFileIOItem();
    if (!pItem)
        return;

    memcpy(pItem, &item, sizeof(TUCBinFileIOItem));
    _vectorDat.push_back(pItem);
}
TUCBinFileIOItem* UCBinFileIO::GetItem(size_t index_0) const
{
    if (index_0 < 0 || index_0 >= _vectorDat.size())
        return 0;

    TUCBinFileIOItem* pItem = _vectorDat[index_0];
    return pItem;
}
TUCBinFileIOItem* UCBinFileIO::GetItem(char *pID, size_t &index) const
{
    index = 0;
    if (!pID || strlen(pID) <= 0)
        return 0;

    for (size_t i = 0; i < _vectorDat.size(); i++)
    {
        TUCBinFileIOItem* pItem = _vectorDat[i];
        if (!pItem) continue;

        if (!strcmp(pItem->_strID, pID))
        {
            index = i;
            return pItem;
        }
    }

    return 0;
}
size_t UCBinFileIO::NumOfItems()
{
    return _vectorDat.size();
}
void UCBinFileIO::Clear()
{
    std::vector<TUCBinFileIOItem*>::iterator it;
    while (_vectorDat.size() > 0)
    {
        it = _vectorDat.begin();
        TUCBinFileIOItem *p = (TUCBinFileIOItem*)(*it);
        if (p)
        {
            FreeItem(p);
        }
        _vectorDat.erase(it);
    }
    _vectorDat.clear();
}

bool UCBinFileIO::Write(char *pPath)
{
    tYUX_PVOID f = OpenWrite(pPath);
    if (!f)
        return false;

    for (size_t i = 0; i < _vectorDat.size(); i++)
    {
        TUCBinFileIOItem *p = _vectorDat[i];
        if (!p)
            continue;
        WriteItem(f, p, false);
    }

    CloseRWHandle(f);
    return true;
}

static void FreeReadBuff(void *p)
{
    char *p8 = (char *)p;
    if (p8)
    {
        delete[] p8;
    }
}
static tYUX_PVOID AllocReadBuff(tYUX_I64 count)
{
    return (void *)(count == 0 ? 0 : new char[count]);
}

bool UCBinFileIO::Read(char *pPath)
{
    FILE *f = (FILE *)OpenRead(pPath);
    if (!f)
        return false;

    while (!feof(f))
    {
        TUCBinFileIOItem dat;
        if (!ReadItem((tYUX_PVOID)f, &dat))
            break;

        TUCBinFileIOItem *pItem = AllocItem(dat._pBuff, dat._nOffset, dat._nSize, dat._fpHandleMem, dat._strID);
        if (!pItem)
        {
            if (dat._fpHandleMem && dat._pBuff)
                dat._fpHandleMem(dat._pBuff);
            goto Handle_Read_Error;
        }
        _vectorDat.push_back(pItem);
    }

    CloseRWHandle(f);
    return true;

Handle_Read_Error:
    if (f)
        CloseRWHandle(f);
    Clear();
    return false;
}

tYUX_PVOID UCBinFileIO::OpenWrite(char *pPath)
{
    if (!pPath || strlen(pPath) <= 0)
        return 0;

    FILE *f = 0;
    return fopen_s(&f, pPath, "wb") == 0 ? (tYUX_PVOID)f : 0;
}
TUCBinFileIOItem* UCBinFileIO::AllocItem(tYUX_PVOID pBuff, tYUX_I64 offset, tYUX_I64 count, fpUCBinFileIOHandleMem fpHandMem, const char *pID)
{
    TUCBinFileIOItem *p = new TUCBinFileIOItem();
    if (!p) return 0;

    ConfigItem(p, pBuff, offset, count, fpHandMem, pID);

    return p;
}
void UCBinFileIO::ConfigItem(TUCBinFileIOItem *p, tYUX_PVOID pBuff, tYUX_I64 offset, tYUX_I64 count, fpUCBinFileIOHandleMem fpHandMem, const char *pID)
{
    if (!p) return;

    memset(p, 0x0, sizeof(TUCBinFileIOItem));
    p->_pBuff = pBuff;
    p->_nOffset = offset;
    p->_nSize = count;
    p->_fpHandleMem = fpHandMem;

    if (pID && strlen(pID) > 0)
    {
        size_t srcLen = strlen(pID);
        size_t cpLen = srcLen > (UCBINFILEIO_ID_RESERVED_SIZE - 1) ? (UCBINFILEIO_ID_RESERVED_SIZE - 1) : srcLen;
        memcpy(p->_strID, pID, cpLen);
    }
}

void UCBinFileIO::FreeItem(TUCBinFileIOItem *pItem)
{
    if (!pItem)
        return;
    if (pItem->_fpHandleMem && pItem->_pBuff)
        pItem->_fpHandleMem(pItem->_pBuff);
    delete pItem;
}
bool UCBinFileIO::WriteItem(tYUX_PVOID pHandle, TUCBinFileIOItem *pDat2Write, bool bFreeAfterWrite)
{
    if (!pHandle)
    {
        if (bFreeAfterWrite)
            FreeItem(pDat2Write);
        return false;
    }
    if (!pDat2Write)
        return true;

    FILE *f = (FILE *)pHandle;

    // total size
    tYUX_I64 nItemTotalSize = UCBINFILEIO_ID_RESERVED_SIZE + pDat2Write->_nSize;
    fwrite((void *)&nItemTotalSize, sizeof(tYUX_I64), 1, f);
    // id string
    fwrite((void *)&pDat2Write->_strID[0], sizeof(char), UCBINFILEIO_ID_RESERVED_SIZE, f);
    // write data
    char *p8 = (char *)pDat2Write->_pBuff;
    fwrite((void *)&p8[pDat2Write->_nOffset], sizeof(char), pDat2Write->_nSize, f);

    if (bFreeAfterWrite)
        FreeItem(pDat2Write);

    return true;
}

tYUX_PVOID UCBinFileIO::OpenRead(char *pPath)
{
    if (!pPath || strlen(pPath) <= 0)
        return 0;

    FILE *f = 0;
    return fopen_s(&f, pPath, "rb") == 0 ? (tYUX_PVOID)f : 0;
}
bool UCBinFileIO::ReadItem(tYUX_PVOID pHandle, TUCBinFileIOItem *pReadDat)
{
    if (!pHandle || !pReadDat)
        return false;

    memset(pReadDat, 0x0, sizeof(TUCBinFileIOItem));

    FILE *f = (FILE*)pHandle;

    // read total size
    tYUX_I64 nItemTotalSize;
    if (fread((void *)&nItemTotalSize, sizeof(tYUX_I64), 1, f) != 1)
        return false;
    if (ferror(f))
        return false;
    if (nItemTotalSize <= UCBINFILEIO_ID_RESERVED_SIZE)
        return false;

    // read id string
    if (fread((void*)&pReadDat->_strID[0], sizeof(char), UCBINFILEIO_ID_RESERVED_SIZE, f) != UCBINFILEIO_ID_RESERVED_SIZE)
        return false;
    if (ferror(f))
        return false;

    // read data
    tYUX_I64 nItemDataSize = nItemTotalSize - UCBINFILEIO_ID_RESERVED_SIZE;
    tYUX_PVOID p = AllocReadBuff(nItemDataSize);
    if (!p)
        return false;
    if (fread(p, sizeof(char), nItemDataSize, f) != nItemDataSize)
    {
        FreeReadBuff(p);
        return false;
    }
    if (ferror(f))
    {
        FreeReadBuff(p);
        return false;
    }

    // config ret item data
    pReadDat->_fpHandleMem = FreeReadBuff;
    pReadDat->_nOffset = 0;
    pReadDat->_nSize = nItemDataSize;
    pReadDat->_pBuff = p;

    return true;
}

void UCBinFileIO::CloseRWHandle(tYUX_PVOID pHandle)
{
    if (pHandle)
        fclose((FILE*)pHandle);
}

void UCBinFileIO::CopyString(char *pDst, size_t nMaxDst, const char *pSrc, size_t nLen)
{
    if (!pDst || nMaxDst == 0)
        return;
    pDst[0] = 0;

    if (!pSrc || nLen == 0)
        return;

    size_t cpLen = nLen > (nMaxDst - 1) ? (nMaxDst - 1) : nLen;
    memcpy(pDst, pSrc, cpLen);
    pDst[cpLen] = 0;
}