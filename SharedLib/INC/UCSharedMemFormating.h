#ifndef UCSharedMemFormating_h__
#define UCSharedMemFormating_h__

#include "UCObject.h"
#include "UCSharedMemFormatingDecl.h"
#include "UCDataSync.h"
#include <vector>
#include <string>
#include "UUtilities.h"

class UCSharedMemFormating : public UCObject
{
protected:
    tYUX_I32                _nMaxLenOfName;
    tYUX_I32                _nMaxNumOfItemsInMainSection;
    UCDataSync<tYUX_U8>    *_pMainSection;
    tYUX_U8               **_ppMainSectionFormated;
    tYUX_I32                _nMainSectionUsed;
    tYUX_I32                _nMainSectionPerItemSize; // should not more than UCDATA_SYNC_MAX_NAME_LEN - 1

    std::vector<TShMemUsedInfo*>        _vectorShMemUsedRec; // each shared memory index used
    std::vector<TShMemAccItem*>         _vectorShMemItemsRec; // each item in shared memory
    std::vector<UCDataSync<tYUX_I32>*>  _vectorI32ShMems;
    std::vector<UCDataSync<double>*>    _vectorDoubleShMems;
    std::vector<UCDataSync<tYUX_U8>*>   _vectorU8ShMems;

public:
    UCSharedMemFormating() : UCObject()
    {
        _strClassRttiName = typeid(*this).name();
        _nMaxLenOfName = 0;
        _nMaxNumOfItemsInMainSection = 0;
        _pMainSection = 0;

        _ppMainSectionFormated = 0;
        _nMainSectionUsed = 0;
        _nMainSectionPerItemSize = 0;
    }
    virtual ~UCSharedMemFormating()
    {
        if (_pMainSection) delete _pMainSection;
        _pMainSection = 0;

        if (_ppMainSectionFormated) delete[] _ppMainSectionFormated;
        _ppMainSectionFormated = 0;

        ClearShMemUsedRec(&_vectorShMemUsedRec);
        ClearShMemItemsRec(&_vectorShMemItemsRec);
        ClearI32ShMems(&_vectorI32ShMems);
        ClearDoubleShMems(&_vectorDoubleShMems);
        ClearU8ShMems(&_vectorU8ShMems);
    }

    virtual tYUX_BOOL Initialize(char *pMainSectionName, int nNameMax, int nItemCountMax) = 0;

    virtual tYUX_BOOL CreateI32ShMem(char *pName, int nCount, tYUX_PVOID &ptrHandle) = 0;
    virtual tYUX_BOOL CreateDoubleShMem(char *pName, int nCount, tYUX_PVOID &ptrHandle) = 0;
    virtual tYUX_BOOL CreateU8ShMem(char *pName, tYUX_I64 nCount, tYUX_PVOID &ptrHandle) = 0;
    tYUX_PVOID GetAvailableShMemI32(int nNeed) { return GetAvailableSharedMemory(nNeed, UCSHAREDMEM_I32); }
    tYUX_PVOID GetAvailableShMemDouble(int nNeed){ return GetAvailableSharedMemory(nNeed, UCSHAREDMEM_DOUBLE); }
    tYUX_PVOID GetAvailableShMemU8(tYUX_I64 nNeed){ return GetAvailableSharedMemory(nNeed, UCSHAREDMEM_U8); }

    virtual tYUX_BOOL DeleteShMem(char *pName) = 0;
    virtual void DeleteShMem(tUCSHAREDMEM_TYPE type)
    {
        std::vector<TShMemUsedInfo*>::iterator itOfRec;
        while (true)
        {
            TShMemUsedInfo *p = 0;
            for (itOfRec = _vectorShMemUsedRec.begin(); itOfRec != _vectorShMemUsedRec.end(); itOfRec++)
            {
                TShMemUsedInfo *pRec = (TShMemUsedInfo*)(*itOfRec);
                if (pRec->_nShMemType == type)
                {
                    p = pRec;
                    break;
                }
            }
            if (!p)
                break;

            DeleteShMem(p->_pShMemName); // note: should not access the p, cause it has been free.
        }
    }
    virtual void DeleteShMem()
    {
        DeleteShMem(UCSHAREDMEM_I32);
        DeleteShMem(UCSHAREDMEM_DOUBLE);
        DeleteShMem(UCSHAREDMEM_U8);
    }

    virtual tYUX_BOOL AddI32(tYUX_PVOID hOfCreated, int nNeededItems, tYUX_PVOID &itemHandle) = 0;
    virtual tYUX_I32* GetI32(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux) = 0;
    virtual tYUX_BOOL AccessI32(tYUX_PVOID itemHandle, tYUX_BOOL bRead, tYUX_I32 *pDat, tYUX_I32 nDat, tYUX_BOOL bSync = (tYUX_BOOL)true) = 0;

    virtual tYUX_BOOL AddDouble(tYUX_PVOID hOfCreated, int nNeededItems, tYUX_PVOID &itemHandle) = 0;
    virtual double* GetDouble(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux) = 0;
    virtual tYUX_BOOL AccessDouble(tYUX_PVOID itemHandle, tYUX_BOOL bRead, double *pDat, tYUX_I32 nDat, tYUX_BOOL bSync = (tYUX_BOOL)true) = 0;

    virtual tYUX_BOOL AddU8(tYUX_PVOID hOfCreated, tYUX_I64 nNeededItems, tYUX_PVOID &itemHandle) = 0;
    virtual tYUX_U8* GetU8(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux) = 0;
    virtual tYUX_BOOL AccessU8(tYUX_PVOID itemHandle, tYUX_BOOL bRead, tYUX_U8 *pDat, tYUX_I64 nDat, tYUX_BOOL bSync = (tYUX_BOOL)true) = 0;

    template<typename T>
    T* GetItemData(tYUX_PVOID handle, tYUX_I64 &nCount, tYUX_PVOID &mux)
    {
        nCount = 0;
        mux = 0;
        for (int i = 0; i < (int)_vectorShMemItemsRec.size(); i++)
        {
            TShMemAccItem*p = _vectorShMemItemsRec[i];
            if (p && p->_pItself == handle)
            {
                if (sizeof(T) != p->_nSizeofT)
                    return 0;

                nCount = p->_nSize;
                return static_cast<T*>(p->_pBegAddr);
            }
        }

        return (T*)0;
    }
    TShMemAccItem* ConvertItemHandle(tYUX_PVOID h)
    {
        for (int i = 0; i < (int)_vectorShMemItemsRec.size(); i++)
        {
            TShMemAccItem*p = _vectorShMemItemsRec[i];
            if (p && p->_pItself == h)
            {
                return p;
            }
        }

        return 0;
    }

protected:
    tYUX_BOOL CreatingCheck(char *pName)
    {
        if (_nMainSectionUsed >= _nMaxNumOfItemsInMainSection) // max count exceeding
            return (tYUX_BOOL)false;
        if (!pName || (int)strlen(pName) > _nMaxLenOfName) // invalid shared memory name
            return (tYUX_BOOL)false;
        // check the same shared memory no matter in which type of vector
        for (int i = 0; i < (int)_vectorShMemUsedRec.size(); i++)
        {
            TShMemUsedInfo* p = _vectorShMemUsedRec[i];
            if (!p || !p->_pShMemName) continue;
            if (!strcmp(p->_pShMemName, pName))
                return (tYUX_BOOL)false;

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
                return (tYUX_BOOL)false;
        }

        return (tYUX_BOOL)true;
    }

    tYUX_PVOID GetAvailableSharedMemory(tYUX_I64 nNeed, tYUX_I32 type)
    {
        if (nNeed <= 0)
            return 0;

        vector<TShMemUsedInfo*>::iterator it;
        for (it = _vectorShMemUsedRec.begin(); it != _vectorShMemUsedRec.end(); it++)
        {
            TShMemUsedInfo *pInfo = (TShMemUsedInfo*)(*it);
            if (!pInfo)
                continue;

            if (pInfo->_nShMemType == type && pInfo->_nAvailableCount >= nNeed)
            {
                return (tYUX_PVOID)pInfo;
            }
        }
        return 0; // no available for this type
    }

    static void ClearShMemUsedRec(std::vector<TShMemUsedInfo*> *pVect)
    {
        if (!pVect || pVect->size() <= 0)
            return;

        std::vector<TShMemUsedInfo*>::iterator it;
        while (pVect->size() > 0)
        {
            it = pVect->begin();
            TShMemUsedInfo *p = (TShMemUsedInfo*)(*it);
            if (p) delete p;

            pVect->erase(it);
        }
    }
    static void ClearShMemItemsRec(std::vector<TShMemAccItem*> *pVect)
    {
        if (!pVect || pVect->size() <= 0)
            return;

        std::vector<TShMemAccItem*>::iterator it;
        while (pVect->size() > 0)
        {
            it = pVect->begin();
            TShMemAccItem *p = (TShMemAccItem*)(*it);
            if (p) delete p;

            pVect->erase(it);
        }
    }
    static void ClearI32ShMems(std::vector<UCDataSync<tYUX_I32>*> *pVect)
    {
        if (!pVect || pVect->size() <= 0)
            return;

        std::vector<UCDataSync<tYUX_I32>*>::iterator it;
        while (pVect->size() > 0)
        {
            it = pVect->begin();
            //UCDataSyncW32<tYUX_I32>* p = dynamic_cast<UCDataSyncW32<tYUX_I32>*>(*it);
            UCDataSync<tYUX_I32>* p = (*it);
            if (p) delete p;

            pVect->erase(it);
        }
    }
    static void ClearDoubleShMems(std::vector<UCDataSync<double>*> *pVect)
    {
        if (!pVect || pVect->size() <= 0)
            return;

        std::vector<UCDataSync<double>*>::iterator it;
        while (pVect->size() > 0)
        {
            it = pVect->begin();
            //UCDataSyncW32<double>* p = dynamic_cast<UCDataSyncW32<double>*>(*it);
            UCDataSync<double>* p = (*it);
            if (p) delete p;

            pVect->erase(it);
        }
    }
    static void ClearU8ShMems(std::vector<UCDataSync<tYUX_U8>*> *pVect)
    {
        if (!pVect || pVect->size() <= 0)
            return;

        std::vector<UCDataSync<tYUX_U8>*>::iterator it;
        while (pVect->size() > 0)
        {
            it = pVect->begin();
            //UCDataSyncW32<tYUX_U8>* p = dynamic_cast<UCDataSyncW32<tYUX_U8>*>(*it);
            UCDataSync<tYUX_U8>* p = (*it);
            if (p) delete p;

            pVect->erase(it);
        }
    }

public:
    static char* GetTypeName(tUCSHAREDMEM_TYPE type)
    {
        switch (type)
        {
        case UCSHAREDMEM_I32: return "INT32";
        case UCSHAREDMEM_DOUBLE: return "DOUBLE";
        case UCSHAREDMEM_U8: return "BYTE";
        }
        return "UNKNOWN";
    }
};


#endif
