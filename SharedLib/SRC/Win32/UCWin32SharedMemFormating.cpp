#include "stdafx.h"
#include "UCWin32SharedMemFormating.h"
#include "UCDataSyncW32.h"
#include <string>

//static void ClearShMemUsedRec(std::vector<TShMemUsedInfo*> *pVect);
//static void ClearShMemItemsRec(std::vector<TShMemAccItem*> *pVect);
//static void ClearI32ShMems(std::vector<UCDataSync<tYUX_I32>*> *pVect);
//static void ClearDoubleShMems(std::vector<UCDataSync<double>*> *pVect);
//static void ClearU8ShMems(std::vector<UCDataSync<tYUX_U8>*> *pVect);
static tYUX_BOOL CheckSameMapFileName(char *pName, int allocSz, tYUX_BOOL &bSame, char **ppRetMapFileName, char **ppRetMuxName, char **ppRetOriMuxName);
static tYUX_BOOL StringCmp(char *pShMemName, char *pName, bool &bSame);

template<typename T>
static tYUX_BOOL Access(T *pDst, T *pSrc, tYUX_I64 nAccess, HANDLE mux = 0)
{
    if (nAccess <= 0)
        return (tYUX_BOOL)false;

    if (mux)
    {
        if (::WaitForSingleObject(mux, INFINITE) != WAIT_OBJECT_0)
            return (tYUX_BOOL)false;
    }

    ::memcpy(pDst, pSrc, sizeof(T) * nAccess);

    if (mux)
        ::ReleaseMutex(mux);

    return (tYUX_BOOL)true;
}


UCWin32SharedMemFormating::UCWin32SharedMemFormating()
    : UCSharedMemFormating()
{
    _strClassRttiName = typeid(*this).name();
}
UCWin32SharedMemFormating::~UCWin32SharedMemFormating()
{
}

tYUX_BOOL UCWin32SharedMemFormating::Initialize(char *pMainSectionName, int nNameMax, int nItemCountMax)
{
    if (_pMainSection)
        return (tYUX_BOOL)false;
    if (nNameMax <= 0 || nItemCountMax <= 0)
        return (tYUX_BOOL)false;
    if (!pMainSectionName || strlen(pMainSectionName) <= 0)
        return (tYUX_BOOL)false;

    static char *pAttachedMuxName = "_MUX";
    int nameLen = (int)strlen(pMainSectionName);
    int muxLen = nameLen + (int)strlen(pAttachedMuxName) + (int)strlen(UCSHAREDMEM_CONCAT_STR);
    char *pMuxName = new char[muxLen];
    if (!pMuxName)
        return (tYUX_BOOL)false;
    sprintf_s(pMuxName, muxLen, "%s%s", pMainSectionName, pAttachedMuxName);

    int perItemSize = nNameMax + 1 + 16;
    perItemSize += ((perItemSize % 8) == 0 ? 0 : ( 8 - (perItemSize % 8))); // make 8-alignment

    _pMainSection = new UCDataSyncW32<tYUX_U8>();
    if (!_pMainSection->Initialize(pMainSectionName, pMuxName, nItemCountMax * (tYUX_I64)perItemSize))
    {
        delete _pMainSection;
        return (tYUX_BOOL)false;
    }
    delete[] pMuxName;

    _ppMainSectionFormated = new tYUX_U8*[nItemCountMax];
    if (!_ppMainSectionFormated)
    {
        delete _pMainSection;
        _pMainSection = 0;
        return (tYUX_BOOL)false;
    }

    tYUX_I64 nBuffCount;
    tYUX_U8 *pAddr = _pMainSection->Get(nBuffCount);
    for (int i = 0; i < nItemCountMax; i++)
    {
        _ppMainSectionFormated[i] = &pAddr[(tYUX_I64)i * (tYUX_I64)perItemSize];
        printf("[UCWin32SharedMemFormating::Initialize] addr %d = %p\n", i, (void *)_ppMainSectionFormated[i]);
    }

    _nMaxLenOfName = nNameMax;
    _nMaxNumOfItemsInMainSection = nItemCountMax;
    _nMainSectionPerItemSize = perItemSize;
    _nMainSectionUsed = 0;

    return (tYUX_BOOL)true;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
TShMemUsedInfo* UCWin32SharedMemFormating::GetShMemInfo(char *pName)
{
    if (!pName || strlen(pName) <= 0)
        return NULL;

    // check the same shared memory no matter in which type of vector
    for (int i = 0; i < (int)_vectorShMemUsedRec.size(); i++)
    {
        TShMemUsedInfo* p = _vectorShMemUsedRec[i];
        if (!p || !p->_pShMemName) continue;
        if (!strcmp(p->_pShMemName, pName))
        {
            return p;
        }

        int count = 0;
        char **pp = UUtility_SplitStr(p->_pShMemName, UCSHAREDMEM_CONCAT_STR, count);
        tYUX_BOOL bSameOne = (tYUX_BOOL)false;
        if (pp && count > 0 && pp[0])
        {
            if (!strcmp(pp[0], pName))
            {
                bSameOne = (tYUX_BOOL)true;
                break;
            }
        }
        UUtility_HandleSplitedStrings(pp, count);
        if (bSameOne)
        {
            return p;
        }
    }

    return NULL;
}

tYUX_BOOL UCWin32SharedMemFormating::CreateI32ShMem(char *pName, int nCount, tYUX_PVOID &ptrHandle)
{
    ptrHandle = 0;
    if (!CreatingCheck(pName))
        return (tYUX_BOOL)false;
    if (nCount <= 0)
        return (tYUX_BOOL)false;

    // create shared memory
    tYUX_BOOL bSame;
    char *pNewName;
    char *pNewMux;
    char *pOriMux;
    if (!CheckSameMapFileName(pName, _nMainSectionPerItemSize, bSame, &pNewName, &pNewMux, &pOriMux))
    {
        if (pNewName) delete[] pNewName;
        if (pNewMux) delete[] pNewMux;
        if (pOriMux) delete[] pOriMux;
        return (tYUX_BOOL)false;
    }

    UCDataSyncW32<tYUX_I32>* p = new UCDataSyncW32<tYUX_I32>();
    tYUX_BOOL status = p->Initialize(pNewName ? pNewName : pName, pNewName ? pNewMux: pOriMux, nCount);
    if (pNewName) delete[] pNewName;
    if (pNewMux) delete[] pNewMux;
    if (pOriMux) delete[] pOriMux;
    if (!status)
    {
        delete p;
        return (tYUX_BOOL)false;
    }

    // Recording
    //_ppMainSectionFormated[_nMainSectionUsed++] = (tYUX_U8 *)p->GivenName();
    sprintf_s((char *)_ppMainSectionFormated[_nMainSectionUsed], _nMainSectionPerItemSize - 1, "%s", p->GivenName());
    _nMainSectionUsed++;
    _vectorI32ShMems.push_back(p);
    //TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), (int)_vectorI32ShMems.size() - 1, UCSHAREDMEM_I32);
    TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), UCSHAREDMEM_I32);
    pUsedInfo->_pItself = (tYUX_PVOID)pUsedInfo;
    pUsedInfo->_pShMemObjInstance = (tYUX_PVOID)p;
    pUsedInfo->_nSizeofT = p->SizeofT();
    pUsedInfo->_pMuxHandle = (tYUX_PVOID)p->GetSyncMux();
    pUsedInfo->_nAvailableCount = p->NumOf();
    _vectorShMemUsedRec.push_back( pUsedInfo );
    ptrHandle = (tYUX_PVOID)pUsedInfo;
    return (tYUX_BOOL)true;
}
tYUX_BOOL UCWin32SharedMemFormating::CreateDoubleShMem(char *pName, int nCount, tYUX_PVOID &ptrHandle)
{
    ptrHandle = 0;
    if (!CreatingCheck(pName))
        return (tYUX_BOOL)false;
    if (nCount <= 0)
        return (tYUX_BOOL)false;

    // create shared memory
    tYUX_BOOL bSame;
    char *pNewName;
    char *pNewMux;
    char *pOriMux;
    if (!CheckSameMapFileName(pName, _nMainSectionPerItemSize, bSame, &pNewName, &pNewMux, &pOriMux))
    {
        if (pNewName) delete[] pNewName;
        if (pNewMux) delete[] pNewMux;
        if (pOriMux) delete[] pOriMux;
        return (tYUX_BOOL)false;
    }

    UCDataSyncW32<double>* p = new UCDataSyncW32<double>();
    tYUX_BOOL status = p->Initialize(pNewName ? pNewName : pName, pNewName ? pNewMux : pOriMux, nCount);
    if (pNewName) delete[] pNewName;
    if (pNewMux) delete[] pNewMux;
    if (pOriMux) delete[] pOriMux;
    if (!status)
    {
        delete p;
        return (tYUX_BOOL)false;
    }

    // Recording
    //_ppMainSectionFormated[_nMainSectionUsed++] = (tYUX_U8 *)p->GivenName();
    sprintf_s((char *)_ppMainSectionFormated[_nMainSectionUsed], _nMainSectionPerItemSize - 1, "%s", p->GivenName());
    _nMainSectionUsed++;
    _vectorDoubleShMems.push_back(p);
    //TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), (int)_vectorDoubleShMems.size() - 1, UCSHAREDMEM_DOUBLE);
    TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), UCSHAREDMEM_DOUBLE);
    pUsedInfo->_pItself = (tYUX_PVOID)pUsedInfo;
    pUsedInfo->_pShMemObjInstance = (tYUX_PVOID)p;
    pUsedInfo->_nSizeofT = p->SizeofT();
    pUsedInfo->_pMuxHandle = (tYUX_PVOID)p->GetSyncMux();
    pUsedInfo->_nAvailableCount = p->NumOf();
    _vectorShMemUsedRec.push_back(pUsedInfo);
    ptrHandle = (tYUX_PVOID)pUsedInfo;
    return (tYUX_BOOL)true;
}
tYUX_BOOL UCWin32SharedMemFormating::CreateU8ShMem(char *pName, tYUX_I64 nCount, tYUX_PVOID &ptrHandle)
{
    ptrHandle = 0;
    if (!CreatingCheck(pName))
        return (tYUX_BOOL)false;
    if (nCount <= 0)
        return (tYUX_BOOL)false;

    // create shared memory
    tYUX_BOOL bSame;
    char *pNewName;
    char *pNewMux;
    char *pOriMux;
    if (!CheckSameMapFileName(pName, _nMainSectionPerItemSize, bSame, &pNewName, &pNewMux, &pOriMux))
    {
        if (pNewName) delete[] pNewName;
        if (pNewMux) delete[] pNewMux;
        if (pOriMux) delete[] pOriMux;
        return (tYUX_BOOL)false;
    }

    UCDataSyncW32<tYUX_U8>* p = new UCDataSyncW32<tYUX_U8>();
    tYUX_BOOL status = p->Initialize(pNewName ? pNewName : pName, pNewName ? pNewMux : pOriMux, nCount);
    if (pNewName) delete[] pNewName;
    if (pNewMux) delete[] pNewMux;
    if (pOriMux) delete[] pOriMux;
    if (!status)
    {
        delete p;
        return (tYUX_BOOL)false;
    }

    // Recording
    //_ppMainSectionFormated[_nMainSectionUsed++] = (tYUX_U8 *)p->GivenName();
    sprintf_s((char *)_ppMainSectionFormated[_nMainSectionUsed], _nMainSectionPerItemSize - 1, "%s", p->GivenName());
    _nMainSectionUsed++;
    _vectorU8ShMems.push_back(p);
    //TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), (int)_vectorU8ShMems.size() - 1, UCSHAREDMEM_U8);
    TShMemUsedInfo *pUsedInfo = new TShMemUsedInfo(p->GivenName(), UCSHAREDMEM_U8);
    pUsedInfo->_pItself = (tYUX_PVOID)pUsedInfo;
    pUsedInfo->_pShMemObjInstance = (tYUX_PVOID)p;
    pUsedInfo->_nSizeofT = p->SizeofT();
    pUsedInfo->_pMuxHandle = (tYUX_PVOID)p->GetSyncMux();
    pUsedInfo->_nAvailableCount = p->NumOf();
    _vectorShMemUsedRec.push_back(pUsedInfo);
    ptrHandle = (tYUX_PVOID)pUsedInfo;
    return (tYUX_BOOL)true;
}

tYUX_BOOL UCWin32SharedMemFormating::DeleteShMem(char *pName)
{
    TShMemUsedInfo* p = GetShMemInfo(pName);
    if (!p)
        return FALSE;

    std::vector<std::string> tmpForName;
    for (int i = 0; i < _nMainSectionUsed; i++)
    {
        if (!strcmp(pName, (char *)_ppMainSectionFormated[i]))
            continue;

        tmpForName.push_back((char *)_ppMainSectionFormated[i]);
    }

    // Remove related items of the shared memory
    std::vector<TShMemAccItem*>::iterator it;
    while (TRUE)
    {
        tYUX_BOOL bAgain = FALSE;
        for (it = _vectorShMemItemsRec.begin(); it != _vectorShMemItemsRec.end(); it++)
        {
            TShMemAccItem* pItem = (TShMemAccItem*)(*it);
            if (pItem && pItem->_pUsedInfoAddr == (tYUX_PVOID)p)
            {
                _vectorShMemItemsRec.erase(it);
                delete pItem;
                bAgain = TRUE;
                break;
            }
        }

        if (!bAgain)
            break;
    }

    // Remove the shared memory
    switch (p->_nShMemType)
    {
    case UCSHAREDMEM_I32:
    {
        std::vector<UCDataSync<tYUX_I32>*>::iterator itOfShMem;
        for (itOfShMem = _vectorI32ShMems.begin(); itOfShMem != _vectorI32ShMems.end(); itOfShMem++)
        {
            UCDataSync<tYUX_I32> *pShMem = (UCDataSync<tYUX_I32>*)(*itOfShMem);
            if ((tYUX_PVOID)pShMem == p->_pShMemObjInstance)
            {
                _vectorI32ShMems.erase(itOfShMem);
                delete pShMem;
                break;
            }
        }
    }
        break;

    case UCSHAREDMEM_DOUBLE:
    {
        std::vector<UCDataSync<double>*>::iterator itOfShMem;
        for (itOfShMem = _vectorDoubleShMems.begin(); itOfShMem != _vectorDoubleShMems.end(); itOfShMem++)
        {
            UCDataSync<double> *pShMem = (UCDataSync<double>*)(*itOfShMem);
            if ((tYUX_PVOID)pShMem == p->_pShMemObjInstance)
            {
                _vectorDoubleShMems.erase(itOfShMem);
                delete pShMem;
                break;
            }
        }
    }
        break;

    case UCSHAREDMEM_U8:
    {
        std::vector<UCDataSync<tYUX_U8>*>::iterator itOfShMem;
        for (itOfShMem = _vectorU8ShMems.begin(); itOfShMem != _vectorU8ShMems.end(); itOfShMem++)
        {
            UCDataSync<tYUX_U8> *pShMem = (UCDataSync<tYUX_U8>*)(*itOfShMem);
            if ((tYUX_PVOID)pShMem == p->_pShMemObjInstance)
            {
                _vectorU8ShMems.erase(itOfShMem);
                delete pShMem;
                break;
            }
        }
    }
        break;

    default:
        throw new std::exception("[UCWin32SharedMemFormating::DeleteShMem] type invlaid.");
        break;
    }

    // Remove from _vectorShMemUsedRec
    std::vector<TShMemUsedInfo*>::iterator itOfRec;
    for (itOfRec = _vectorShMemUsedRec.begin(); itOfRec != _vectorShMemUsedRec.end(); itOfRec++)
    {
        TShMemUsedInfo *pRec = (TShMemUsedInfo*)(*itOfRec);
        if (p == pRec)
        {
            _vectorShMemUsedRec.erase(itOfRec);
            delete pRec;
            break;
        }
    }

    // modify the _ppMainSectionFormated
    for (int i = 0; i < (int)tmpForName.size(); i++)
    {
        std::string& str = tmpForName[i];
        sprintf_s((char *)_ppMainSectionFormated[i], _nMainSectionPerItemSize - 1, "%s", str.c_str());
    }
    _nMainSectionUsed = (int)tmpForName.size();
	for (int i = (int)tmpForName.size(); i < _nMaxNumOfItemsInMainSection; i++)
	{
		if (_ppMainSectionFormated[i])
			_ppMainSectionFormated[i][0] = 0;
	}

    return TRUE;
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_BOOL UCWin32SharedMemFormating::ItemHandleValid(tYUX_PVOID h)
{
    if (!h) return (tYUX_BOOL)false;
    for (int i = 0; i < (int)_vectorShMemItemsRec.size(); i++)
    {
        TShMemAccItem* p = _vectorShMemItemsRec[i];
        if (p && p->_pItself == h)
        {
            return (tYUX_BOOL)true;
        }
    }
    return (tYUX_BOOL)false;
}
tYUX_BOOL UCWin32SharedMemFormating::ShMemHandleValid(tYUX_PVOID h)
{
    if (!h) return (tYUX_BOOL)false;
    for (int i = 0; i < (int)_vectorShMemUsedRec.size(); i++)
    {
        TShMemUsedInfo* p = _vectorShMemUsedRec[i];
        if (p && p->_pItself == h)
            return (tYUX_BOOL)true;
    }

    return (tYUX_BOOL)false;
}

//-----------------------------------------------------------------------------
tYUX_BOOL UCWin32SharedMemFormating::AddI32(tYUX_PVOID hOfCreated, int nNeededItems, tYUX_PVOID &itemHandle)
{
    itemHandle = NULL;

    if (!ShMemHandleValid(hOfCreated))
        return (tYUX_BOOL)false;

    TShMemUsedInfo *p = (TShMemUsedInfo *)hOfCreated;
    // request size
    UCDataSyncW32<tYUX_I32> *pShMem = (UCDataSyncW32<tYUX_I32> *)p->_pShMemObjInstance;
    tYUX_I64 nLastIndex = p->_nNextBegIndex + (tYUX_I64)nNeededItems;
    if (nLastIndex > pShMem->NumOf())
    {
#ifdef UBASE_SUPPORT_TYPE_64
        printf("[UCWin32SharedMemFormating::AddI32] request(Beg:%I64d + Need:%d = %I64d) out-of-range(%I64d)\n", p->_nNextBegIndex, nNeededItems, nLastIndex, pShMem->NumOf());
#else
        printf("[UCWin32SharedMemFormating::AddI32] request(Beg:%d + Need:%d = %d) out-of-range(%d)\n", p->_nNextBegIndex, nNeededItems, nLastIndex, pShMem->NumOf());
#endif
        return (tYUX_BOOL)false;
    }

    tYUX_I64 nCount;
    tYUX_I32 *pArr = pShMem->Get(nCount);
    if (!pArr)
    {
        return (tYUX_BOOL)false;
    }

    // fill info
    TShMemAccItem *pItem = new TShMemAccItem();
    if (!pItem)
    {
        return (tYUX_BOOL)false;
    }
    itemHandle = pItem->_pItself = (tYUX_PVOID)pItem;
    pItem->_pUsedInfoAddr = hOfCreated;
    pItem->_pShMemObjInstance = (tYUX_PVOID)pShMem;
    pItem->_pShMemName = pShMem->GivenName();
    pItem->_nShMemType = p->_nShMemType;
    pItem->_nSizeofT = p->_nSizeofT;
    pItem->_pMuxHandle = p->_pMuxHandle;
    pItem->_pBegAddr = &pArr[p->_nNextBegIndex];
    pItem->_nOffset = p->_nNextBegIndex;
    pItem->_nSize = nNeededItems;
    pItem->_strTypeName = GetTypeName(UCSHAREDMEM_I32);
    _vectorShMemItemsRec.push_back(pItem);
    // update next index
    p->_nNextBegIndex = p->_nNextBegIndex + (tYUX_I64)nNeededItems;
    p->_nAvailableCount = pShMem->NumOf() - p->_nNextBegIndex;

    return (tYUX_BOOL)true;
}
tYUX_I32* UCWin32SharedMemFormating::GetI32(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux)
{
    nCount = 0;
    mux = NULL;
    if (!ItemHandleValid(itemHandle))
        return NULL;

    TShMemAccItem *pItem = (TShMemAccItem*)itemHandle;
    if (pItem->_nShMemType != UCSHAREDMEM_I32)
        return NULL;
    UCDataSyncW32<tYUX_I32> *pShMem = (UCDataSyncW32<tYUX_I32> *)pItem->_pShMemObjInstance;
    if (!pItem->_pShMemObjInstance)
        return NULL;

    nCount = (int)pItem->_nSize;
    mux = pShMem->GetSyncMux();
    return (tYUX_I32*)pItem->_pBegAddr;
}
tYUX_BOOL UCWin32SharedMemFormating::AccessI32(tYUX_PVOID itemHandle, tYUX_BOOL bRead, tYUX_I32 *pDat, tYUX_I32 nDat, tYUX_BOOL bSync)
{
    tYUX_PVOID h;
    int count;
    tYUX_I32 *pAcc = GetI32(itemHandle, count, h);
    if (!pAcc)
        return (tYUX_BOOL)false;

    bool status = true;
    bool bSynced = false;
    HANDLE mux = (HANDLE)h;
    if (bSync && mux)
    {
        if (WaitForSingleObject(mux, INFINITE) != WAIT_OBJECT_0)
            return (tYUX_BOOL)false;
        bSynced = true;
    }

    if (bRead)
    {
        if (!pDat || nDat < count)
            status = false;
        else
            memcpy(pDat, pAcc, sizeof(tYUX_I32)*count);
    }
    else
    {
        if (!pDat || nDat > count)
            status = false;
        else
            memcpy(pAcc, pDat, sizeof(tYUX_I32)*count);
    }

    if (bSynced)
        ReleaseMutex(mux);

    return (tYUX_BOOL)status;
}

//-----------------------------------------------------------------------------
tYUX_BOOL UCWin32SharedMemFormating::AddDouble(tYUX_PVOID hOfCreated, int nNeededItems, tYUX_PVOID &itemHandle)
{
    itemHandle = NULL;

    if (!ShMemHandleValid(hOfCreated))
        return (tYUX_BOOL)false;

    TShMemUsedInfo *p = (TShMemUsedInfo *)hOfCreated;
    // request size
    UCDataSyncW32<double> *pShMem = (UCDataSyncW32<double> *)p->_pShMemObjInstance;
    tYUX_I64 nLastIndex = p->_nNextBegIndex + (tYUX_I64)nNeededItems;
    if (nLastIndex > pShMem->NumOf())
    {
#ifdef UBASE_SUPPORT_TYPE_64
        printf("[UCWin32SharedMemFormating::AddDouble] request(Beg:%I64d + Need:%d = %I64d) out-of-range(%I64d)\n", p->_nNextBegIndex, nNeededItems, nLastIndex, pShMem->NumOf());
#else
        printf("[UCWin32SharedMemFormating::AddDouble] request(Beg:%d + Need:%d = %d) out-of-range(%d)\n", p->_nNextBegIndex, nNeededItems, nLastIndex, pShMem->NumOf());
#endif
        return (tYUX_BOOL)false;
    }

    tYUX_I64 nCount;
    double *pArr = pShMem->Get(nCount);
    if (!pArr)
    {
        return (tYUX_BOOL)false;
    }

    // fill info
    TShMemAccItem *pItem = new TShMemAccItem();
    if (!pItem)
    {
        return (tYUX_BOOL)false;
    }
    itemHandle = pItem->_pItself = (tYUX_PVOID)pItem;
    pItem->_pUsedInfoAddr = hOfCreated;
    pItem->_pShMemObjInstance = (tYUX_PVOID)pShMem;
    pItem->_pShMemName = pShMem->GivenName();
    pItem->_nShMemType = p->_nShMemType;
    pItem->_nSizeofT = p->_nSizeofT;
    pItem->_pMuxHandle = p->_pMuxHandle;
    pItem->_pBegAddr = &pArr[p->_nNextBegIndex];
    pItem->_nOffset = p->_nNextBegIndex;
    pItem->_nSize = nNeededItems;
    pItem->_strTypeName = GetTypeName(UCSHAREDMEM_DOUBLE);
    _vectorShMemItemsRec.push_back(pItem);
    // update next index
    p->_nNextBegIndex = p->_nNextBegIndex + (tYUX_I64)nNeededItems;
    p->_nAvailableCount = pShMem->NumOf() - p->_nNextBegIndex;

    return (tYUX_BOOL)true;
}
double* UCWin32SharedMemFormating::GetDouble(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux)
{
    nCount = 0;
    mux = NULL;
    if (!ItemHandleValid(itemHandle))
        return NULL;

    TShMemAccItem *pItem = (TShMemAccItem*)itemHandle;
    if (pItem->_nShMemType != UCSHAREDMEM_DOUBLE)
        return NULL;
    UCDataSyncW32<double> *pShMem = (UCDataSyncW32<double> *)pItem->_pShMemObjInstance;
    if (!pItem->_pShMemObjInstance)
        return NULL;

    nCount = (int)pItem->_nSize;
    mux = pShMem->GetSyncMux();
    return (double*)pItem->_pBegAddr;
}
tYUX_BOOL UCWin32SharedMemFormating::AccessDouble(tYUX_PVOID itemHandle, tYUX_BOOL bRead, double *pDat, tYUX_I32 nDat, tYUX_BOOL bSync)
{
    tYUX_PVOID h;
    int count;
    double *pAcc = GetDouble(itemHandle, count, h);
    if (!pAcc)
        return (tYUX_BOOL)false;

    bool status = true;
    bool bSynced = false;
    HANDLE mux = (HANDLE)h;
    if (bSync && mux)
    {
        if (WaitForSingleObject(mux, INFINITE) != WAIT_OBJECT_0)
            return (tYUX_BOOL)false;
        bSynced = true;
    }

    if (bRead)
    {
        if (!pDat || nDat < count)
            status = false;
        else
            memcpy(pDat, pAcc, sizeof(double)*count);
    }
    else
    {
        if (!pDat || nDat > count)
            status = false;
        else
            memcpy(pAcc, pDat, sizeof(double)*count);
    }

    if (bSynced)
        ReleaseMutex(mux);

    return (tYUX_BOOL)status;
}

//-----------------------------------------------------------------------------
tYUX_BOOL UCWin32SharedMemFormating::AddU8(tYUX_PVOID hOfCreated, tYUX_I64 nNeededItems, tYUX_PVOID &itemHandle)
{
    itemHandle = NULL;

    if (!ShMemHandleValid(hOfCreated))
        return (tYUX_BOOL)false;

    TShMemUsedInfo *p = (TShMemUsedInfo *)hOfCreated;
    // request size
    UCDataSyncW32<tYUX_U8> *pShMem = (UCDataSyncW32<tYUX_U8> *)p->_pShMemObjInstance;
    tYUX_I64 nLastIndex = p->_nNextBegIndex + (tYUX_I64)nNeededItems;
    if (nLastIndex > pShMem->NumOf())
    {
#ifdef UBASE_SUPPORT_TYPE_64
        printf("[UCWin32SharedMemFormating::AddU8] request(Beg:%I64d + Need:%I64d = %I64d) out-of-range(%I64d)\n", p->_nNextBegIndex, nNeededItems, nLastIndex, pShMem->NumOf());
#else
        printf("[UCWin32SharedMemFormating::AddU8] request(Beg:%d + Need:%d = %d) out-of-range(%d)\n", p->_nNextBegIndex, nNeededItems, nLastIndex, pShMem->NumOf());
#endif
        return (tYUX_BOOL)false;
    }

    tYUX_I64 nCount;
    tYUX_U8 *pArr = pShMem->Get(nCount);
    if (!pArr)
    {
        return (tYUX_BOOL)false;
    }

    // fill info
    TShMemAccItem *pItem = new TShMemAccItem();
    if (!pItem)
    {
        return (tYUX_BOOL)false;
    }
    itemHandle = pItem->_pItself = (tYUX_PVOID)pItem;
    pItem->_pUsedInfoAddr = hOfCreated;
    pItem->_pShMemObjInstance = (tYUX_PVOID)pShMem;
    pItem->_pShMemName = pShMem->GivenName();
    pItem->_nShMemType = p->_nShMemType;
    pItem->_nSizeofT = p->_nSizeofT;
    pItem->_pMuxHandle = p->_pMuxHandle;
    pItem->_pBegAddr = &pArr[p->_nNextBegIndex];
    pItem->_nOffset = p->_nNextBegIndex;
    pItem->_nSize = nNeededItems;
    pItem->_strTypeName = GetTypeName(UCSHAREDMEM_U8);
    _vectorShMemItemsRec.push_back(pItem);
    // update next index
    p->_nNextBegIndex = p->_nNextBegIndex + (tYUX_I64)nNeededItems;
    p->_nAvailableCount = pShMem->NumOf() - p->_nNextBegIndex;

    return (tYUX_BOOL)true;
}
tYUX_U8* UCWin32SharedMemFormating::GetU8(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux)
{
    nCount = 0;
    mux = NULL;
    if (!ItemHandleValid(itemHandle))
        return NULL;

    TShMemAccItem *pItem = (TShMemAccItem*)itemHandle;
    if (pItem->_nShMemType != UCSHAREDMEM_U8)
        return NULL;
    UCDataSyncW32<tYUX_U8> *pShMem = (UCDataSyncW32<tYUX_U8> *)pItem->_pShMemObjInstance;
    if (!pItem->_pShMemObjInstance)
        return NULL;

    nCount = (int)pItem->_nSize;
    mux = pShMem->GetSyncMux();
    return (tYUX_U8*)pItem->_pBegAddr;
}
tYUX_BOOL UCWin32SharedMemFormating::AccessU8(tYUX_PVOID itemHandle, tYUX_BOOL bRead, tYUX_U8 *pDat, tYUX_I64 nDat, tYUX_BOOL bSync)
{
    tYUX_PVOID h;
    int count;
    tYUX_U8 *pAcc = GetU8(itemHandle, count, h);
    if (!pAcc)
        return (tYUX_BOOL)false;

    bool status = true;
    bool bSynced = false;
    HANDLE mux = (HANDLE)h;
    if (bSync && mux)
    {
        if (WaitForSingleObject(mux, INFINITE) != WAIT_OBJECT_0)
            return (tYUX_BOOL)false;
        bSynced = true;
    }

    if (bRead)
    {
        if (!pDat || nDat < count)
            status = false;
        else
            memcpy(pDat, pAcc, sizeof(tYUX_U8)*count);
    }
    else
    {
        if (!pDat || nDat > count)
            status = false;
        else
            memcpy(pAcc, pDat, sizeof(tYUX_U8)*count);
    }

    if (bSynced)
        ReleaseMutex(mux);

    return (tYUX_BOOL)status;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

tYUX_BOOL CheckSameMapFileName(char *pName, int allocSz, tYUX_BOOL &bSame, char **ppRetMapFileName, char **ppRetMuxName, char **ppRetOriMuxName)
{
    bSame = (tYUX_BOOL)false;

    if(ppRetMapFileName) *ppRetMapFileName = NULL;
    if(ppRetMuxName) *ppRetMuxName = NULL;
    if (ppRetOriMuxName) *ppRetOriMuxName = NULL;

    if (!ppRetMapFileName) return (tYUX_BOOL)false;
    if(!ppRetMuxName) return (tYUX_BOOL)false;
    if (!ppRetOriMuxName) return (tYUX_BOOL)false;

    tYUX_BOOL bErr = (tYUX_BOOL)false;

    char *pOriMuxName = new char[UCDATA_SYNC_MAX_NAME_LEN];
    if(!pOriMuxName) return (tYUX_BOOL)false;
    try
    {
        sprintf_s(pOriMuxName, UCDATA_SYNC_MAX_NAME_LEN -1, "%s_MUX", pName);
    }
    catch (...)
    {
        DWORD errCode = GetLastError();
        printf("[CheckSameMapFileName] 1. sprintf_s() error code = %d\n", errCode);
        bErr = (tYUX_BOOL)true;
    }
    if (bErr)
    {
        if (pOriMuxName) delete[] pOriMuxName;
        return (tYUX_BOOL)false;
    }
    *ppRetOriMuxName = pOriMuxName;

    char *pMapFileName = new char[allocSz];
    if (!pMapFileName)
        return (tYUX_BOOL)false;

    try
    {
        sprintf_s(pMapFileName, allocSz - 1, "%s%s", UCDATASYNCWIN32_SHMEM_PREFIX, pName);
    }
    catch (...)
    {
        DWORD errCode = GetLastError();
        printf("[CheckSameMapFileName] 2. sprintf_s() error code = %d\n", errCode);
        bErr = (tYUX_BOOL)true;
    }
    if (bErr)
    {
        delete[] pMapFileName;
        return (tYUX_BOOL)false;
    }

    HANDLE h = ::OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, pMapFileName);
    bSame = h ? (tYUX_BOOL)true : (tYUX_BOOL)false;
    if (h) ::CloseHandle(h);

    if (bSame)
    {
        SYSTEMTIME sysTm;
        try
        {
            ::GetLocalTime(&sysTm);
            sprintf_s(pMapFileName, allocSz - 1, "%s%s%s%d%d%d%d%d%d%.2d", UCDATASYNCWIN32_SHMEM_PREFIX, pName, UCSHAREDMEM_CONCAT_STR,
                                                                         sysTm.wYear, sysTm.wMonth, sysTm.wDay, 
                                                                         sysTm.wHour, sysTm.wMinute, sysTm.wSecond, sysTm.wMilliseconds % 100);
        }
        catch (...)
        {
            DWORD errCode = GetLastError();
            printf("[CheckSameMapFileName] 3. sprintf_s() error code = %d\n", errCode);
            bErr = (tYUX_BOOL)true;
        }
        if (bErr)
        {
            delete[] pMapFileName;
            return (tYUX_BOOL)false;
        }

        h = ::OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, pMapFileName);
        if (h)
        {
            ::CloseHandle(h);
            printf("[CheckSameMapFileName] new name %s exist!\n", pMapFileName);
            delete[] pMapFileName;
            return (tYUX_BOOL)false;
        }

        char *pRetNewName = new char[allocSz];
        char *pRetNewMux = new char[UCDATA_SYNC_MAX_NAME_LEN];
        if (!pRetNewName || !pRetNewMux)
        {
            if (pMapFileName) delete[] pMapFileName;
            if (pRetNewMux) delete[] pRetNewMux;
            return (tYUX_BOOL)false;
        }
        try
        {
            sprintf_s(pRetNewName, allocSz - 1, "%s%s%d%d%d%d%d%d%.2d", pName, UCSHAREDMEM_CONCAT_STR,
                sysTm.wYear, sysTm.wMonth, sysTm.wDay,
                sysTm.wHour, sysTm.wMinute, sysTm.wSecond, sysTm.wMilliseconds % 100);
            sprintf_s(pRetNewMux, UCDATA_SYNC_MAX_NAME_LEN - 1, "%s_MUX", pRetNewName);
        }
        catch (...)
        {
            DWORD errCode = GetLastError();
            printf("[CheckSameMapFileName] 4. sprintf_s() error code = %d\n", errCode);
            bErr = (tYUX_BOOL)true;
        }
        if (bErr)
        {
            delete[] pMapFileName;
            delete[] pRetNewMux;
            delete[] pRetNewName;
            return (tYUX_BOOL)false;
        }

        *ppRetMapFileName = pRetNewName;
        *ppRetMuxName = pRetNewMux;
    }

    delete[] pMapFileName;
    return (tYUX_BOOL)true;
}

tYUX_BOOL StringCmp(char *pShMemName, char *pName, bool &bSame)
{
    bSame = false;
    if (!pShMemName || !pName || strlen(pShMemName) <= 0 || strlen(pName) <= 0)
        return (tYUX_BOOL)false;

    int npp;
    char **pp = UUtility_SplitStr(pShMemName, UCSHAREDMEM_CONCAT_STR, npp);
    if (!pp)
        return (tYUX_BOOL)false;

    bSame = !strcmp(pp[0], pName);
    UUtility_HandleSplitedStrings(pp, npp);

    return (tYUX_BOOL)true;
}
