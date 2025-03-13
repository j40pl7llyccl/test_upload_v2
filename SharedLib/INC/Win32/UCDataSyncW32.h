#ifndef UCDataSyncW32_h__
#define UCDataSyncW32_h__

#include <stdio.h>
#include <windows.h>
#include <string>
#include <typeinfo>
#include <sstream>
#include <fstream>
#include <vector>
#include "UCDataSync.h"
#include "UCBinFileIO.h"
#include "UUtilities.h"

using namespace std;

#define UCDATASYNCWIN32_SHMEM_PREFIX    "Local\\"
#define UCDATASYNCWIN32_FILEPREFIX      "DATASYNC_FILE_PREFIX"
#define UCDATASYNCWIN32_GIVENNAME       "Given Name"
#define UCDATASYNCWIN32_MUX             "Mux Name"
#define UCDATASYNCWIN32_MAPPINGFILE     "Mapping File Name"
#define UCDATASYNCWIN32_SIZEOFTYPE      "Sizeof Data Type"
#define UCDATASYNCWIN32_SHAREDMEM       "Shared Memory Data"

#ifdef UBASE_SUPPORT_TYPE_64
#define DATASYNC_FILE_PREFIX    "x64"
#else
#define DATASYNC_FILE_PREFIX    "x86"
#endif

typedef string(*fpUCDataSync32IniWriteCallback)(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit);
typedef void(*fpUCDataSync32IniReadCallback)(const char *pRdDat, tYUX_PVOID pCurItem);

template<typename DATATYPE>
class UCDataSyncW32 : public UCDataSync<DATATYPE>
{
private:
    BOOL        _bReadyAccess;
    HANDLE      _hMapFile;
    HANDLE      _hMutex;
    PVOID       _pBegMapFile;


private:
    void Check()
    {
        if (!_pBegMapFile)
            return;
        tYUX_I64 *pLen = (tYUX_I64*)_pBegMapFile;
        tYUX_I64 len = *pLen;
        if (*pLen == _nData)
            return;

        // re-create
        ::WaitForSingleObject(_hMutex, INFINITE);

        _bReadyAccess = FALSE;
        _pData = NULL;
        _nData = 0;

        ::UnmapViewOfFile(_pBegMapFile);
        ::CloseHandle(_hMapFile);

        _pBegMapFile = NULL;
        _hMapFile = NULL;

        tYUX_I64 nAlloc = (tYUX_I64)len *(tYUX_I64)_nSizeofDataType + sizeof(tYUX_I64);
        HANDLE hMap = ::OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, _strMappingFileName);
        if (!hMap)
#ifdef UBASE_SUPPORT_TYPE_64
        {
            LARGE_INTEGER i64Val;
            i64Val.QuadPart = nAlloc;
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, i64Val.HighPart, i64Val.LowPart, _strMappingFileName);
        }
#else
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, nAlloc, _strMappingFileName);
#endif
        if (!hMap)
        {
            ::ReleaseMutex(_hMutex);
            printf("[Check] create mapping file fail!\n");
            return;
        }
        LPVOID pBuff = ::MapViewOfFile(hMap, FILE_MAP_ALL_ACCESS, 0, 0, nAlloc);
        if (!pBuff)
        {
            ::ReleaseMutex(_hMutex);
            printf("[Check] map view of file fail!\n");
            ::CloseHandle(hMap);
            return;
        }

        char *pDat = (char *)pBuff;
        pDat += sizeof(tYUX_I64);
        // config the resources to class data
        _hMapFile = hMap;
        _pBegMapFile = pBuff;
        _pData = (DATATYPE *)pDat;
        _nData = len;
        _bReadyAccess = TRUE;

        ::ReleaseMutex(_hMutex);

#ifdef UBASE_SUPPORT_TYPE_64
        printf("[Check] re-map count=%I64d\n", len);
#else
        printf("[Check] re-map count=%d\n", len);
#endif
    }

public:
    UCDataSyncW32() : UCDataSync<DATATYPE>()
    {
        _strClassRttiName = typeid(*this).name();
        _hMapFile = NULL;
        _hMutex = NULL;
        _bReadyAccess = FALSE;
        _pBegMapFile = NULL;
    }

    virtual ~UCDataSyncW32()
    {
        _bReadyAccess = FALSE;
        if (_pBegMapFile)
        {
            ::UnmapViewOfFile((LPCVOID)_pBegMapFile); 
            _pData = NULL; _pBegMapFile = NULL;
        }
        if (_hMapFile) ::CloseHandle(_hMapFile); _hMapFile = NULL;
        if (_hMutex) ::CloseHandle(_hMutex); _hMutex = NULL;
    }

    virtual tYUX_BOOL Initialize(char *pMapFilename, char *pMuxName, tYUX_I64 nEleCount) // not thread safe
    {
        //static const char *pPrefix = "Local\\";
        // already created?
        if (_bReadyAccess)
            return TRUE;
        // config the string
        int szMap = (int)strlen(pMapFilename) + (int)strlen(UCDATASYNCWIN32_SHMEM_PREFIX);
        int szMux = (int)strlen(pMuxName);
        if (szMap > UCDATA_SYNC_MAX_NAME_LEN - 1)
            throw new std::exception("Map name exceeded...");
        if (szMux > UCDATA_SYNC_MAX_NAME_LEN - 1)
            throw new std::exception("Mux name exceeded...");
        sprintf_s(_strMappingFileName, sizeof(_strMappingFileName) - 1, "%s%s", UCDATASYNCWIN32_SHMEM_PREFIX, pMapFilename);
        sprintf_s(_strMutexName, sizeof(_strMutexName) - 1, "%s", pMuxName);

        // create mux
        if (!_hMutex)
        {
            HANDLE hMux = ::OpenMutexA(MUTEX_ALL_ACCESS, FALSE, _strMutexName);
            if (!hMux)
                hMux = ::CreateMutexA(NULL, FALSE, _strMutexName);
            _hMutex = hMux;
        }
        // condition check
        if (nEleCount < 0)
            return FALSE;
        // create mapping file handle
        tYUX_I64 nAlloc = (tYUX_I64)nEleCount *(tYUX_I64)_nSizeofDataType + sizeof(tYUX_I64);
        BOOL bEverOpened = FALSE;
        HANDLE hMap = ::OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, _strMappingFileName);
        if (!hMap)
#ifdef UBASE_SUPPORT_TYPE_64
        {
            LARGE_INTEGER i64Val;
            i64Val.QuadPart = nAlloc;
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, i64Val.HighPart, i64Val.LowPart, _strMappingFileName);
        }
#else
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, nAlloc, _strMappingFileName);
#endif
        else
            bEverOpened = TRUE;
        if (!hMap)
        {
            printf("[UCDataSyncW32::Initialize]Call CreateFileMappingA with error code-%d\n", ::GetLastError());
            return FALSE;
        }
        LPVOID pBuff = ::MapViewOfFile(hMap, FILE_MAP_ALL_ACCESS, 0, 0, nAlloc);
        if (!pBuff)
        {
            ::CloseHandle(hMap);
            return FALSE;
        }
        // not created, clear all the contents
        if (!bEverOpened)
        {
            ::WaitForSingleObject(_hMutex, INFINITE);
            memset(pBuff, 0x0, nAlloc);
            ::ReleaseMutex(_hMutex);
        }
        tYUX_I64 *pLen = (tYUX_I64 *)pBuff;
        *pLen = nEleCount;
        char *pDat = (char *)pBuff;
        pDat += sizeof(tYUX_I64);
        // config the resources to class data
        _hMapFile = hMap;
        _pBegMapFile = pBuff;
        _pData = (DATATYPE *)pDat;
        _nData = nEleCount;
        _bReadyAccess = TRUE;

        if (_pStrGivenName) delete[] _pStrGivenName;
        _pStrGivenName = new char[strlen(pMapFilename) + 1];
        if (_pStrGivenName)
        {
            memcpy(_pStrGivenName, pMapFilename, strlen(pMapFilename));
            _pStrGivenName[strlen(pMapFilename)] = 0;
        }


        return TRUE;
    }

    virtual tYUX_BOOL ReadFile(char *pPath) // not thread safe, overwrite existing data
    {
        UCBinFileIO rf;
        char tmpStore[UCDATA_SYNC_MAX_NAME_LEN];

        // read from file
        if (!rf.Read(pPath))
            return FALSE;

        // free resources
        FreeResources();

        // open the file
        size_t indexOfItem;
        TUCBinFileIOItem* pItem = rf.GetItem(UCDATASYNCWIN32_FILEPREFIX, indexOfItem);
        if (!pItem)
        {
            printf("[UCDataSyncW32::ReadFile] not found file prefix\n");
            return FALSE;
        }
        char *prefix = UCBinFileIO::GetItemData<char>(pItem);
        UCBinFileIO::CopyString(tmpStore, UCDATA_SYNC_MAX_NAME_LEN, prefix, pItem->_nSize);
        if (strcmp(tmpStore, DATASYNC_FILE_PREFIX))
        {
            printf("[UCDataSyncW32::ReadFile] prefix error want(%s), got(%s)\n", DATASYNC_FILE_PREFIX, prefix);
            return FALSE;
        }

        // get given name
        pItem = rf.GetItem(UCDATASYNCWIN32_GIVENNAME, indexOfItem);
        if (pItem)
        {
            char *pGN = UCBinFileIO::GetItemData<char>(pItem);
            if (pGN && strlen(pGN) > 0)
            {
                if (_pStrGivenName) delete[] _pStrGivenName;
                _pStrGivenName = new char[pItem->_nSize + 1];
                if (_pStrGivenName)
                {
                    memcpy(_pStrGivenName, pGN, pItem->_nSize);
                    _pStrGivenName[pItem->_nSize] = 0;
                }
            }
        }

        // get mux , mapping file & data
        TUCBinFileIOItem* pItemMux = rf.GetItem(UCDATASYNCWIN32_MUX, indexOfItem);
        TUCBinFileIOItem* pItemMap = rf.GetItem(UCDATASYNCWIN32_MAPPINGFILE, indexOfItem);
        TUCBinFileIOItem* pItemSiz = rf.GetItem(UCDATASYNCWIN32_SIZEOFTYPE, indexOfItem);
        TUCBinFileIOItem* pItemMem = rf.GetItem(UCDATASYNCWIN32_SHAREDMEM, indexOfItem);
        size_t nLastDefIndex = indexOfItem;
        if (!pItemMux || !pItemMap || !pItemSiz || !pItemMem)
        {
            printf("[UCDataSyncW32::ReadFile] pItemMux=%p, pItemMap=%p, pItemSiz=%p, pItemMem=%p\n", (void*)pItemMux, (void*)pItemMap, (void*)pItemSiz, (void*)pItemMem);
            return FALSE;
        }
        if (pItemSiz->_nSize != sizeof(tYUX_I32))
        {
#ifdef UBASE_SUPPORT_TYPE_64
            printf("[UCDataSyncW32::ReadFile] sizeof data type _nSize=%I64d not equal sizeof(tYUX_I32)\n", pItemSiz->_nSize);
#else
            printf("[UCDataSyncW32::ReadFile] sizeof data type _nSize=%d not equal sizeof(tYUX_I32)\n", pItemSiz->_nSize);
#endif
            return FALSE;
        }
        tYUX_I32 *pSizeof = UCBinFileIO::GetItemData<tYUX_I32>(pItemSiz);
        if (*pSizeof != _nSizeofDataType)
        {
#ifdef UBASE_SUPPORT_TYPE_64
            printf("[UCDataSyncW32::ReadFile] sizeof(%I64d) data type in file not equal to this class(%d)!\n", pItemSiz->_nSize, _nSizeofDataType);
#else
            printf("[UCDataSyncW32::ReadFile] sizeof(%d) data type in file not equal to this class(%d)!\n", pItemSiz->_nSize, _nSizeofDataType);
#endif
            return FALSE;
        }

        UCBinFileIO::CopyString(_strMutexName, UCDATA_SYNC_MAX_NAME_LEN, UCBinFileIO::GetItemData<char>(pItemMux), pItemMux->_nSize);
        UCBinFileIO::CopyString(_strMappingFileName, UCDATA_SYNC_MAX_NAME_LEN, UCBinFileIO::GetItemData<char>(pItemMap), pItemMap->_nSize);

        // open resource
        if (!OpenResources(pItemMem->_nSize / (tYUX_I64)_nSizeofDataType))
        {
            printf("[UCDataSyncW32::ReadFile] open resource fail!\n");
            return FALSE;
        }
        // copy data
        ::WaitForSingleObject(_hMutex, INFINITE);
        //memcpy(_pData, pItemMem->_pBuff, pItemMem->_nSize);
        tYUX_U8 *pDst = (tYUX_U8 *)_pData;
        tYUX_U8 *pSrc = (tYUX_U8 *)pItemMem->_pBuff;
        for (tYUX_I64 i = 0; i < pItemMem->_nSize; i++) pDst[i] = pSrc[i];
        ::ReleaseMutex(_hMutex);

        // reload named map
        size_t nTotalNamedMap = rf.NumOfItems();
        for (size_t i = nLastDefIndex + 1; i < nTotalNamedMap; i++)
        {
            pItem = rf.GetItem(i);
            if (!pItem)
                continue;

            TUCDataItemInfo* pMap = UCBinFileIO::GetItemData<TUCDataItemInfo>(pItem);
            if (!pMap)
                continue;

            SetNamedMap(pItem->_strID, pMap->_nOffset, pMap->_nSize);
        }

        return TRUE;
    }

    /*
    virtual tYUX_BOOL ReadFile(char *pPath) // not thread safe, overwrite existing data
    {
        // free resources
        _bReadyAccess = FALSE;
        if (_pBegMapFile)
        {
            ::UnmapViewOfFile((LPCVOID)_pBegMapFile);
            _pData = NULL; _pBegMapFile = NULL;
        }
        if (_hMapFile) ::CloseHandle(_hMapFile); _hMapFile = NULL;
        if (_hMutex) ::CloseHandle(_hMutex); _hMutex = NULL;

        // open the file
        FILE *fp = NULL;
        errno_t err = fopen_s(&fp, pPath, "rb");
        if (err)
            return FALSE;

        char prefix[16];
        int nPrefix = (int)strlen(DATASYNC_FILE_PREFIX);
        if (nPrefix > sizeof(prefix) - 1)
            return FALSE;
        fread(prefix, sizeof(char), nPrefix, fp);
        prefix[nPrefix] = 0;
        if (strcmp(prefix, DATASYNC_FILE_PREFIX))
        {
            printf("[UCDataSyncW32::ReadFile] prefix error want(%s), got(%s)\n", DATASYNC_FILE_PREFIX, prefix);
            return FALSE;
        }

        // read first 128 - mutex name
        char tmp[UCDATA_SYNC_MAX_NAME_LEN];
        size_t nRd = fread((void*)&tmp[0], sizeof(char), UCDATA_SYNC_MAX_NAME_LEN, fp);
        if (nRd < UCDATA_SYNC_MAX_NAME_LEN)
        {
            fclose(fp); return FALSE;
        }
        memcpy((PVOID)&_strMutexName[0], (PVOID)&tmp[0], UCDATA_SYNC_MAX_NAME_LEN);
        // read map file name
        nRd = fread((void*)&tmp[0], sizeof(char), UCDATA_SYNC_MAX_NAME_LEN, fp);
        if (nRd < UCDATA_SYNC_MAX_NAME_LEN)
        {
            fclose(fp); return FALSE;
        }
        memcpy((PVOID)&_strMappingFileName[0], (PVOID)&tmp[0], UCDATA_SYNC_MAX_NAME_LEN);
        // read array size
        tYUX_I64 nArrSz = 0;
        UINT32 nEleSz = 0;
        nRd = fread((void*)&nArrSz, sizeof(tYUX_I64), 1, fp);
        if (nRd != 1)
        {
            fclose(fp); return FALSE;
        }
        nRd = fread((void *)&nEleSz, sizeof(UINT32), 1, fp);
        if (nRd != 1)
        {
            fclose(fp); return FALSE;
        }
        if (nArrSz == 0 || nEleSz != _nSizeofDataType)
        {
            fclose(fp); return FALSE;
        }

#ifdef UBASE_SUPPORT_TYPE_64
        tYUX_I64 curPos = _ftelli64(fp);
        _fseeki64(fp, 0, SEEK_END);
        tYUX_I64 endPos = _ftelli64(fp);
        if ((endPos - curPos) != (tYUX_I64)(nArrSz * (tYUX_I64)nEleSz))
        {
            fclose(fp); return FALSE;
        }
#else
        long curPos = ftell(fp);
        fseek(fp, 0, SEEK_END);
        long endPos = ftell(fp);
        if ((endPos - curPos) != (long)(nArrSz * nEleSz))
        {
            fclose(fp); return FALSE;
        }
#endif

        // alloc resources - mutex
        HANDLE hMux = ::OpenMutexA(MUTEX_ALL_ACCESS, FALSE, _strMutexName);
        if (!hMux)
            hMux = ::CreateMutexA(NULL, FALSE, _strMutexName);
        _hMutex = hMux;
        // alloc resource - mapping file
        tYUX_I64 nAlloc = (tYUX_I64)(nArrSz * (tYUX_I64)nEleSz);
        HANDLE hMap = ::OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, _strMappingFileName);
        if (!hMap)
#ifdef UBASE_SUPPORT_TYPE_64
        {
            LARGE_INTEGER i64Val;
            i64Val.QuadPart = nAlloc;
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, i64Val.HighPart, i64Val.LowPart, _strMappingFileName);
        }
#else
        hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, nAlloc, _strMappingFileName);
#endif
        if (!hMap)
        {
            printf("[UCDataSyncW32::ReadFile]Call CreateFileMappingA with error code-%d\n", ::GetLastError());
            fclose(fp);
            return FALSE;
        }
        LPVOID pBuff = ::MapViewOfFile(hMap, FILE_MAP_ALL_ACCESS, 0, 0, nAlloc);
        if (!pBuff)
        {
            printf("[UCDataSyncW32::ReadFile]Call MapViewOfFile with error code-%d\n", ::GetLastError());
            ::CloseHandle(hMap);
            fclose(fp);
            return FALSE;
        }
        // move back & read to buffer
#ifdef UBASE_SUPPORT_TYPE_64
        _fseeki64(fp, curPos, SEEK_SET);
#else
        fseek(fp, curPos, SEEK_SET);
#endif
        char *pDat = (char *)pBuff;
        pDat += sizeof(tYUX_I64);
        ::WaitForSingleObject(_hMutex, INFINITE);
        nRd = fread(pDat, 1, nAlloc, fp);
        ::ReleaseMutex(_hMutex);

        // close file
        fclose(fp);

        // config the resources to class data
        tYUX_I64 *pLen = (tYUX_I64 *)pBuff;
        *pLen = nArrSz;
        _hMapFile = hMap;
        _pBegMapFile = pBuff;
        _pData = (DATATYPE *)pDat;
        _nData = nArrSz;
        _bReadyAccess = TRUE;
        return TRUE;
    }
    */

    virtual tYUX_BOOL WriteFile(char *pPath)
    {
        UCBinFileIO wr;
        // write header type
        TUCBinFileIOItem item;
        UCBinFileIO::ConfigItem(&item, DATASYNC_FILE_PREFIX, 0, strlen(DATASYNC_FILE_PREFIX), 0, UCDATASYNCWIN32_FILEPREFIX);
        wr.Add(item);
        // write given name
        if (_pStrGivenName && strlen(_pStrGivenName) > 0)
        {
            UCBinFileIO::ConfigItem(&item, _pStrGivenName, 0, strlen(_pStrGivenName), 0, UCDATASYNCWIN32_GIVENNAME);
            wr.Add(item);
        }
        // write mux
        UCBinFileIO::ConfigItem(&item, _strMutexName, 0, strlen(_strMutexName), 0, UCDATASYNCWIN32_MUX);
        wr.Add(item);
        // write mapping file name
        UCBinFileIO::ConfigItem(&item, _strMappingFileName, 0, strlen(_strMappingFileName), 0, UCDATASYNCWIN32_MAPPINGFILE);
        wr.Add(item);
        // write sizeof data
        UCBinFileIO::ConfigItem(&item, &_nSizeofDataType, 0, sizeof(tYUX_I32), 0, UCDATASYNCWIN32_SIZEOFTYPE);
        wr.Add(item);
        // write shared
        UCBinFileIO::ConfigItem(&item, _pData, 0, _nData * _nSizeofDataType, 0, UCDATASYNCWIN32_SHAREDMEM);
        wr.Add(item);
        // write named items
        map<string, TUCDataItemInfo>::iterator it;
        for (it = _mapNamedItems.begin(); it != _mapNamedItems.end(); it++)
        {
            TUCDataItemInfo *pInfo = (TUCDataItemInfo *)malloc(sizeof(TUCDataItemInfo));
            if (!pInfo)
                continue;
            pInfo->_nOffset = it->second._nOffset;
            pInfo->_nSize = it->second._nSize;
            UCBinFileIO::ConfigItem(&item,  (tYUX_PVOID)pInfo, 0, sizeof(TUCDataItemInfo), free, it->first.c_str());
            wr.Add(item);
        }

        // start writing to file
        return (tYUX_BOOL)wr.Write(pPath);
    }

    /*
    virtual tYUX_BOOL WriteFile(char *pPath)
    {
        if (!_bReadyAccess)
            return FALSE;

        FILE *fp = NULL;
        errno_t err = fopen_s(&fp, pPath, "wb");
        if (err) return FALSE;

        // write header
        char *pStr = DATASYNC_FILE_PREFIX;
        fwrite((void*)pStr, sizeof(char), strlen(DATASYNC_FILE_PREFIX), fp);
        fwrite((void *)&_strMutexName[0], sizeof(char), UCDATA_SYNC_MAX_NAME_LEN, fp); // mux name
        fwrite((void *)&_strMappingFileName[0], sizeof(char), UCDATA_SYNC_MAX_NAME_LEN, fp); // map file name
        tYUX_I64 nArrsize = (tYUX_I64)_nData;
        UINT32 nElesize = (UINT32)_nSizeofDataType;
        fwrite((void *)&nArrsize, sizeof(tYUX_I64), 1, fp);
        fwrite((void *)&nElesize, sizeof(UINT32), 1, fp);

        ::WaitForSingleObject(_hMutex, INFINITE);
        // write data
        size_t nW = nArrsize * (size_t)nElesize;
        fwrite((void*)_pData, (size_t)nElesize, (size_t)nArrsize, fp);
        ::ReleaseMutex(_hMutex);

        fclose(fp);
        return TRUE;
    }
    */
    virtual DATATYPE* Get(tYUX_I64 &nSize)
    {
        Check();

        nSize = 0;
        if (!_bReadyAccess) return NULL;
        nSize = _nData;
        return _pData;
    }
    virtual tYUX_BOOL Get(tYUX_I64 index, DATATYPE &val)
    {
        if (!_bReadyAccess)
            return FALSE;

        Check();

        if (index < 0 || index >= _nData) return FALSE;
        val = _pData[index];
        return TRUE;
    }
    virtual tYUX_BOOL Set(tYUX_I64 index, DATATYPE val, tYUX_U32 nTimeout = INFINITE)
    {
        Check();

        if (index < 0 || index >= _nData || !_bReadyAccess)
            return FALSE;

        if (::WaitForSingleObject(_hMutex, nTimeout) == WAIT_OBJECT_0)
        {
            if (!_bReadyAccess)
            {
                ::ReleaseMutex(_hMutex);
                return FALSE;
            }

            _pData[index] = val;

            ::ReleaseMutex(_hMutex);
            return TRUE;
        }

        return FALSE;
    }
    virtual tYUX_BOOL Set(tYUX_I64 *pIndexes, DATATYPE *pValues, tYUX_I32 nSize, tYUX_U32 nTimeout = INFINITE)
    {
        Check();

        if (!pIndexes || !pValues || nSize <= 0 || !_bReadyAccess)
            return FALSE;

        if (::WaitForSingleObject(_hMutex, nTimeout) == WAIT_OBJECT_0)
        {
            if (!_bReadyAccess)
            {
                ::ReleaseMutex(_hMutex);
                return FALSE;
            }

            for (int i = 0; i < nSize; i++)
            {
                if (pIndexes[i] < 0 || pIndexes[i] >= _nData)
                    continue;
                _pData[pIndexes[i]] = pValues[i];
            }

            ::ReleaseMutex(_hMutex);
            return TRUE;
        }
        return FALSE;
    }

    BOOL Available() { return _bReadyAccess; }
    HANDLE GetSyncMux() { return _hMutex; }

    virtual DATATYPE* GetFromNamedMapArray(const char *pName, tYUX_I64 &nSize, tYUX_PVOID &muxObj)
    {
        if (!_pData)
            return false;

        TUCDataItemInfo info;
        if (!QueryFromNamedMap(pName, info))
            return false;

        muxObj = (tYUX_PVOID)_hMutex;
        nSize = info._nSize;
        return (DATATYPE*)&_pData[info._nOffset];
    }
    virtual bool GetFromNamedMap(const char *pName, DATATYPE *pDat2Fill, tYUX_I64 nOffset, tYUX_I64 nMaxsize, tYUX_BOOL sync)
    {
        if (!_pData || !pDat2Fill || nOffset < 0)
            return false;

        TUCDataItemInfo info;
        if (!QueryFromNamedMap(pName, info))
            return false;

        bool synced = false;
        bool ret = true;
        if (sync)
        {
            if (::WaitForSingleObject(_hMutex, INFINITE) != WAIT_OBJECT_0)
                return false;
            synced = true;
        }

        if (nMaxsize < info._nSize)
            ret = false;
        else
            memcpy(&pDat2Fill[nOffset], &_pData[info._nOffset], info._nSize * sizeof(DATATYPE));

        if (synced)
            ::ReleaseMutex(_hMutex);
        return ret;
    }
    virtual bool SetFromNamedMap(const char *pName, DATATYPE *pDat2Put, tYUX_I64 nOffset, tYUX_I64 nCount, tYUX_BOOL sync)
    {
        if (!_pData || !pDat2Put || nOffset < 0 || nCount <= 0)
            return false;

        TUCDataItemInfo info;
        if (!QueryFromNamedMap(pName, info))
            return false;

        bool synced = false;
        bool ret = true;
        if (sync)
        {
            if (::WaitForSingleObject(_hMutex, INFINITE) != WAIT_OBJECT_0)
                return false;
            synced = true;
        }

        if (nCount > info._nSize)
            ret = false;
        else
            memcpy(&_pData[info._nOffset], &pDat2Put[nOffset], nCount * sizeof(DATATYPE));

        if (synced)
            ::ReleaseMutex(_hMutex);
        return ret;
    }
    void WriteToIni(char *pPath, fpUCDataSync32IniWriteCallback fp)
    {
        if (!_bReadyAccess || !fp)
            return;

        // write type name
        string line;
        fstream file;
        file.open(pPath, ios_base::out | ios_base::trunc);

        file << "[DataType]" << endl;
        file << "  Type = " << typeid(*this).name() << endl;
        file << endl;

        file << "[Resources]" << endl;
        file << "  MappingFileName = " << _pStrGivenName << endl;
        file << "  MutexName = " << _strMutexName << endl;
        file << "  AllocItemCount = " << _nData << endl;
        file << endl;

        file << "[Items]" << endl;
        map<string, TUCDataItemInfo>::iterator it;
        for (it = _mapNamedItems.begin(); it != _mapNamedItems.end(); it++)
        {
            file << "  " << it->first << " = " << it->second._nOffset << ", " << it->second._nSize << ", ";
            file << fp(_pData, it->second._nOffset, it->second._nSize, _nSizeofDataType, ",");
            file << endl;
        }
        for (tYUX_I64 i = 0; i < _nData; i++)
        {
            if (CheckIndexInNamedMap(i))
                continue;
            file << "  UCDataSyncW32_" << i << " = 0, 1, ";
            file << fp(_pData,  i, 1, _nSizeofDataType, ",");
            file << endl;
        }

        file.close();

    }
    void ReadFromIni(char *pPath, fpUCDataSync32IniReadCallback fp)
    {
        // [DataType]
        // Type = name of this class
        // [Resources]
        // MappingFileName = string
        // MutexName = string
        // AllocItemCount = int64
        // [Items]
        // NameOfItem = indexBeg, size, value1, value2...

        if (!fp)
            return;

        char tmpStore[UCDATA_SYNC_MAX_NAME_LEN];
        char *strRdLine = NULL;
        DWORD nMaxSize = (DWORD)UUtility_FileSize(pPath);
        if (nMaxSize <= 0)
            return;

        strRdLine = new char[nMaxSize + 1];
        if (!strRdLine)
            return;

        DWORD nRd = ::GetPrivateProfileStringA("DataType", "Type", NULL, strRdLine, nMaxSize, pPath);
        if (nRd <= 0) {
            delete[] strRdLine; return; // no type available
        }
        if (strcmp(strRdLine, typeid(*this).name())) {
            delete[] strRdLine; return;
        }

        // read mutex
        nRd = ::GetPrivateProfileStringA("Resources", "MutexName", NULL, strRdLine, nMaxSize, pPath);
        if (nRd <= 0) {
            delete[] strRdLine; return;
        }
        UCBinFileIO::CopyString(_strMutexName, UCDATA_SYNC_MAX_NAME_LEN, strRdLine, strlen(strRdLine));

        // read mapping file
        nRd = ::GetPrivateProfileStringA("Resources", "MappingFileName", NULL, strRdLine, nMaxSize, pPath);
        if (nRd <= 0) {
            delete[] strRdLine; return;
        }
        if (_pStrGivenName) delete[] _pStrGivenName;
        _pStrGivenName = new char[strlen(strRdLine) + 1];
        if (_pStrGivenName)
        {
            memcpy(_pStrGivenName, strRdLine, strlen(strRdLine));
            _pStrGivenName[strlen(strRdLine)] = 0;
        }
        sprintf_s(_strMappingFileName, sizeof(_strMappingFileName) - 1, "%s%s", UCDATASYNCWIN32_SHMEM_PREFIX, _pStrGivenName);

        // read count
        tYUX_I64 count = 0;
        nRd = ::GetPrivateProfileStringA("Resources", "AllocItemCount", NULL, strRdLine, nMaxSize, pPath);
        UCBinFileIO::CopyString(tmpStore, UCDATA_SYNC_MAX_NAME_LEN, strRdLine, strlen(strRdLine));
        if (!UUtility_isInt(tmpStore)) {
            delete[] strRdLine; return;
        }
        count = _atoi64(tmpStore);

        // free resources
        FreeResources();

        // create 
        if (!OpenResources(count))
        {
            printf("[UCDataSyncW32::ReadFromIni] open resources fail!\n");
            delete[] strRdLine;
            return;
        }

        // read items
        vector<string> items;
        nRd = ::GetPrivateProfileStringA("Items", NULL, NULL, strRdLine, nMaxSize, pPath);
        UUtility_SplitStringFrom0(strRdLine, nRd, &items);
        for (size_t i = 0; i < items.size(); i++)
        {
            bool bDefItem = strstr(items[i].c_str(), "UCDataSyncW32") == NULL ? false : true;
            nRd = ::GetPrivateProfileStringA("Items", items[i].c_str(), NULL, strRdLine, nMaxSize, pPath);
            string val = strRdLine;
            string::iterator it;
            for (it = val.begin(); it != val.end(); it++)
            {
                if (*it == ' ')
                    val.erase(it);
            }
            int nVals = 0;
            char** ppVals = UUtility_SplitStr(val.c_str(), ",", nVals);
            if (!bDefItem)
            {
                if (nVals >= 2)
                {
                    tYUX_I64 index = _atoi64(ppVals[0]);
                    tYUX_I64 offset = _atoi64(ppVals[1]);
                    // add to named map
                    if (SetNamedMap(items[i].c_str(), index, offset))
                    {
                        tYUX_I64 addrOffset = index * (tYUX_I64)_nSizeofDataType;
                        tYUX_U8 *pAddr = (tYUX_U8 *)_pData;
                        pAddr += addrOffset;

                        // set value
                        int cntVal = 0;
                        for (cntVal = 2; cntVal < nVals; cntVal++, pAddr += (tYUX_I64)_nSizeofDataType)
                            fp(ppVals[cntVal], (tYUX_PVOID)pAddr);
                    }
                }
            }
            else if(nVals == 3)
            {
                int nItemNameSplit = 0;
                char** ppItemNameSplit = UUtility_SplitStr(items[i].c_str(), "_", nItemNameSplit);

                if (nItemNameSplit == 2 && UUtility_isInt(ppItemNameSplit[1]))
                {
#ifdef UBASE_SUPPORT_TYPE_64
                    tYUX_I64 defIndex = _atoi64(ppItemNameSplit[1]);
#else
                    tYUX_I32 defIndex = atoi(ppItemSplit[1]);
#endif
                    tYUX_I64 addrOffset = defIndex * (tYUX_I64)_nSizeofDataType;
                    tYUX_U8 *pAddr = (tYUX_U8 *)_pData;
                    pAddr += addrOffset;

                    fp(ppVals[2], (tYUX_PVOID)pAddr);
                }

                UUtility_HandleSplitedStrings(ppItemNameSplit, nItemNameSplit);
            }
            UUtility_HandleSplitedStrings(ppVals, nVals);
        }

        delete[] strRdLine;
        return;
    }

private:
    void FreeResources()
    {
        _bReadyAccess = FALSE;
        if (_pBegMapFile)
        {
            ::UnmapViewOfFile((LPCVOID)_pBegMapFile);
            _pData = NULL; _pBegMapFile = NULL;
        }
        if (_hMapFile) ::CloseHandle(_hMapFile); _hMapFile = NULL;
        if (_hMutex) ::CloseHandle(_hMutex); _hMutex = NULL;
    }
    bool OpenResources(tYUX_I64 count)
    {
        if (count <= 0)
            return false;

        // alloc resources - mutex
        HANDLE hMux = ::OpenMutexA(MUTEX_ALL_ACCESS, FALSE, _strMutexName);
        if (!hMux)
            hMux = ::CreateMutexA(NULL, FALSE, _strMutexName);
        _hMutex = hMux;
        // alloc resource - mapping file
        HANDLE hMap = ::OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, _strMappingFileName);
        tYUX_I64 nAllocSz = count + sizeof(tYUX_I64);
        if (!hMap)
#ifdef UBASE_SUPPORT_TYPE_64
        {
            LARGE_INTEGER i64Val;
            i64Val.QuadPart = nAllocSz;
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, i64Val.HighPart, i64Val.LowPart, _strMappingFileName);
        }
#else
            hMap = ::CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, nAllocSz, _strMappingFileName);
#endif
        if (!hMap)
        {
            printf("[UCDataSyncW32::OpenResources]Call CreateFileMappingA with error code-%d\n", ::GetLastError());
            return FALSE;
        }
        LPVOID pBuff = ::MapViewOfFile(hMap, FILE_MAP_ALL_ACCESS, 0, 0, nAllocSz);
        if (!pBuff)
        {
            printf("[UCDataSyncW32::OpenResources]Call MapViewOfFile with error code-%d\n", ::GetLastError());
            ::CloseHandle(hMap);
            return FALSE;
        }
        // config pointer
        char *pDat = (char *)pBuff;
        pDat += sizeof(tYUX_I64);

        // config the resources to class data
        tYUX_I64 *pLen = (tYUX_I64 *)pBuff;
        *pLen = count;
        _hMapFile = hMap;
        _pBegMapFile = pBuff;
        _pData = (DATATYPE *)pDat;
        _nData = count;
        _bReadyAccess = TRUE;

        return TRUE;
    }

};

#pragma region inline_function_for_ini
inline string GetItemFromTypeU8(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_U8))
        return "";

    tYUX_U8 *pU8 = (tYUX_U8 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", (tYUX_I32)pU8[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit,(tYUX_I32)pU8[i]);
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeU8(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_U8 *p = (tYUX_U8 *)pCurItem;
    *p = (tYUX_U8)atoi(pRdDat);
}

inline string GetItemFromTypeI8(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_I8))
        return "";

    tYUX_I8 *pI8 = (tYUX_I8 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", pI8[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, pI8[i]);
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeI8(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_I8 *p = (tYUX_I8 *)pCurItem;
    *p = (tYUX_I8)atoi(pRdDat);
}

inline string GetItemFromTypeU16(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_U16))
        return "";

    tYUX_U16 *p = (tYUX_U16 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, p[i]);
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeU16(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_U16 *p = (tYUX_U16 *)pCurItem;
    *p = (tYUX_U16)atoi(pRdDat);
}

inline string GetItemFromTypeI16(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_I16))
        return "";

    tYUX_I16 *p = (tYUX_I16 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, p[i]);
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeI16(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_I16 *p = (tYUX_I16 *)pCurItem;
    *p = (tYUX_I16)atoi(pRdDat);
}

inline string GetItemFromTypeU32(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_U32))
        return "";

    tYUX_U32 *p = (tYUX_U32 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, p[i]);
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeU32(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_U32 *p = (tYUX_U32 *)pCurItem;
    *p = (tYUX_U32)_atoi64(pRdDat);
}

inline string GetItemFromTypeI32(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_I32))
        return "";

    tYUX_I32 *p = (tYUX_I32 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, p[i]);
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeI32(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_I32 *p = (tYUX_I32 *)pCurItem;
    *p = (tYUX_I32)atoi(pRdDat);
}

inline string GetItemFromTypeI64(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_I64))
        return "";

    tYUX_I64 *p = (tYUX_I64 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
#ifdef UBASE_SUPPORT_TYPE_64
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%I64d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %I64d", pSplit, p[i]);
#else
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, p[i]);
#endif
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeI64(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_I64 *p = (tYUX_I64 *)pCurItem;
#ifdef UBASE_SUPPORT_TYPE_64
    *p = (tYUX_I64)_atoi64(pRdDat);
#else
    *p = (tYUX_I64)atoi(pRdDat);
#endif
}

inline string GetItemFromTypeU64(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(tYUX_U64))
        return "";

    tYUX_U64 *p = (tYUX_U64 *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
#ifdef UBASE_SUPPORT_TYPE_64
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%lld", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %lld", pSplit, p[i]);
#else
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%d", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %d", pSplit, p[i]);
#endif
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeU64(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isInt(pRdDat))
        return;

    tYUX_U64 *p = (tYUX_U64 *)pCurItem;
#ifdef UBASE_SUPPORT_TYPE_64
    *p = (tYUX_U64)_atoi64(pRdDat);
#else
    *p = (tYUX_U64)atoi(pRdDat);
#endif
}


inline string GetItemFromTypeDouble(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(double))
        return "";

    double *p = (double *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset, cnt = 0; cnt < nCount; i++, cnt++)
    {
#ifdef UBASE_SUPPORT_TYPE_64
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%f", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %f", pSplit, p[i]);
#else
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%f", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %f", pSplit, p[i]);
#endif
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeDouble(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isFloat(pRdDat))
        return;

    double *p = (double *)pCurItem;
    *p = (double)atof(pRdDat);
}

inline string GetItemFromTypeFloat(tYUX_PVOID pDat, tYUX_I64 nItemOffset, tYUX_I64 nCount, tYUX_I32 nItemSizeof, char *pSplit = ",")
{
    if (!pDat || nItemOffset < 0 || nItemSizeof != sizeof(float))
        return "";

    float *p = (float *)pDat;
    string ret;
    char tmpStr[128];

    for (tYUX_I64 i = nItemOffset; i < nCount; i++)
    {
#ifdef UBASE_SUPPORT_TYPE_64
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%f", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %f", pSplit, p[i]);
#else
        if (i == nItemOffset)
            sprintf_s(tmpStr, 128, "%f", p[i]);
        else
            sprintf_s(tmpStr, 128, "%s %f", pSplit, p[i]);
#endif
        ret += tmpStr;
    }
    return ret;
}
inline void SetItemFromTypeFloat(const char *pRdDat, tYUX_PVOID pCurItem)
{
    if (!pCurItem)
        return;
    if (!UUtility_isFloat(pRdDat))
        return;

    float *p = (float *)pCurItem;
    *p = (float)atof(pRdDat);
}
#pragma endregion


#endif
