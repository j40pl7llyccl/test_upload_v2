#ifndef UCDATA_SYNC_h__
#define UCDATA_SYNC_h__

#include "UCObject.h"
#include <string>
#include <map>

#define UCDATA_SYNC_MAX_NAME_LEN     128

typedef struct tagUCDataItemInfo
{
    tYUX_I64    _nOffset;
    tYUX_I64    _nSize;
}TUCDataItemInfo;

using namespace std;

// Remark
//  - cannot use this kind of separation mode, so move the code into this .h
//  - Ever met the memory in U8 type calling memcpy(), sprintf_s() to move data into 
//    shared memory, cause memory access violation. So, if any this kind of problem,
//    maybe copy it one-by-one. Such as:
//    for() SharedMemAddr[] = FromCurrentMem[]
template<typename DATATYPE>
class UCDataSync : public UCObject
{
private:

protected:
    char       *_pStrGivenName;
    char        _strMappingFileName[UCDATA_SYNC_MAX_NAME_LEN];
    char        _strMutexName[UCDATA_SYNC_MAX_NAME_LEN];
    DATATYPE   *_pData;
    tYUX_I64    _nData;
    tYUX_I32    _nSizeofDataType;

    map<string, TUCDataItemInfo>    _mapNamedItems;

public:
    //UCDataSync(char *pMapFileName, char *pMuxName);
    UCDataSync() : UCObject()
    {
        memset((PVOID)& _strMappingFileName[0], 0x0, sizeof(_strMappingFileName));
        memset((PVOID)& _strMutexName[0], 0x0, sizeof(_strMutexName));
        _pData = 0;
        _nData = 0;
        _nSizeofDataType = sizeof(DATATYPE);
        _pStrGivenName = 0;
        _strClassRttiName = typeid(*this).name();
    }
    virtual ~UCDataSync()
    {
        if (_pStrGivenName) delete[] _pStrGivenName;
        _pStrGivenName = 0;
    }

    virtual tYUX_BOOL Initialize(char *pMapFilename, char *pMuxName, tYUX_I64 nEleCount) = 0;
    virtual tYUX_BOOL ReadFile(char *pPath) = 0;

    virtual tYUX_BOOL WriteFile(char *pPath) = 0;
    virtual DATATYPE* Get(tYUX_I64 &nSize) = 0;
    virtual tYUX_BOOL Get(tYUX_I64 index, DATATYPE &val) = 0;
    virtual tYUX_BOOL Set(tYUX_I64 index, DATATYPE val, tYUX_U32 nTimeout) = 0;
    virtual tYUX_BOOL Set(tYUX_I64 *pIndexes, DATATYPE *pValues, tYUX_I32 nSize, tYUX_U32 nTimeout) = 0;

    char* SharedMemName() const { return (char *)(&_strMappingFileName[0]); }
    char* MuxName() const { return (char*)(&_strMutexName[0]); }
    char* GivenName() { return _pStrGivenName; }
    tYUX_I64 NumOf() { return _nData; }
    tYUX_I32 SizeofT() { return _nSizeofDataType; }

    void ClearNamedMap()
    {
        _mapNamedItems.clear();
    }
    size_t NumofNamedMap() { return _mapNamedItems.size(); }
    const char* GetNamedMapItem(size_t index_0, TUCDataItemInfo &retInfo)
    {
        retInfo._nOffset = 0;
        retInfo._nSize = 0;
        if (_mapNamedItems.size() <= 0)
            return 0;

        if (index_0 < 0 || index_0 >= _mapNamedItems.size())
            return 0;

        map<string, TUCDataItemInfo>::iterator it;
        size_t i = 0;
        for (it = _mapNamedItems.begin(), i = 0; it != _mapNamedItems.end(); it++, i++)
        {
            if (i == index_0)
            {
                retInfo = it->second;
                return it->first.c_str();
            }
        }

        return 0;
    }
    bool SetNamedMap(const char *pName, tYUX_I64 nOffset, tYUX_I64 nSize)
    {
        if (!_pData)
            return false;
        if (!pName || strlen(pName) <= 0 || nOffset < 0 || nSize <= 0)
            return false;

        tYUX_I64 tmpMin = nOffset;
        tYUX_I64 tmpMax = nOffset + nSize - 1;

        map<string, TUCDataItemInfo>::iterator it;
        for (it = _mapNamedItems.begin(); it != _mapNamedItems.end(); it++)
        {
            if (it->first == pName)
                continue;

            tYUX_I64 curMin = it->second._nOffset;
            tYUX_I64 curMax = it->second._nOffset + it->second._nSize - 1;
            // check if overlapped range
            if ((tmpMin >= curMin && tmpMin <= curMax) || (tmpMax >= curMin && tmpMax <= curMax))
            {
                return false;
            }
        }

        it = _mapNamedItems.find(pName);
        if (it != _mapNamedItems.end())
        {
            it->second._nOffset = nOffset;
            it->second._nSize = nSize;
        }
        else
        {
            TUCDataItemInfo info;
            info._nOffset = nOffset;
            info._nSize = nSize;
            _mapNamedItems.insert(pair<string, TUCDataItemInfo>(pName, info));
        }
        return true;
    }
    bool QueryFromNamedMap(const char *pName, TUCDataItemInfo &retInfo)
    {
        retInfo._nOffset = 0;
        retInfo._nSize = 0;

        if (!_pData)
            return false;

        map<string, TUCDataItemInfo>::iterator it = _mapNamedItems.find(pName);
        if (it == _mapNamedItems.end())
            return false;

        retInfo._nOffset = it->second._nOffset;
        retInfo._nSize = it->second._nSize;
        return true;
    }
    bool CheckIndexInNamedMap(tYUX_I64 index)
    {
        map<string, TUCDataItemInfo>::iterator it;
        for (it = _mapNamedItems.begin(); it != _mapNamedItems.end(); it++)
        {
            if (index >= it->second._nOffset && index < (it->second._nOffset + it->second._nSize))
                return true;
        }

        return false;
    }
    virtual DATATYPE* GetFromNamedMapArray(const char *pName, tYUX_I64 &nSize, tYUX_PVOID &muxObj) = 0;
    virtual bool GetFromNamedMap(const char *pName, DATATYPE *pDat2Fill, tYUX_I64 nOffset, tYUX_I64 nMaxsize, tYUX_BOOL sync) = 0;
    virtual bool SetFromNamedMap(const char *pName, DATATYPE *pDat2Put, tYUX_I64 nOffset, tYUX_I64 nCount, tYUX_BOOL sync) = 0;
};


#endif
