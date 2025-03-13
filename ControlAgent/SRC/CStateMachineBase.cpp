#include "stdafx.h"
#include "CStateMachineBase.h"

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CStateMachineBase::CStateMachineBase()
{
    InitResources();
}

CStateMachineBase::CStateMachineBase(char *pGivenName)
{
    InitResources();

    if(pGivenName && strlen(pGivenName) > 0)
        sprintf_s(_strID, sizeof(_strID) - 1, "%s", pGivenName);
}

void CStateMachineBase::InitResources()
{
    _pOwner = NULL;
    _hBinSem = ::CreateSemaphore(NULL, 1, 1, NULL);
    _fpGetDoubleStorage = NULL;
    _pContextForDoubleStorage = NULL;

    _fpGetInt32Storage = NULL;
    _pContextForInt32Storage = NULL;

    _strID[0] = 0;
    _fpLog = NULL;

    ::QueryPerformanceFrequency(&_nFreq);

    for (int i = 0; i < nSMB_NUM_SETTINGS; i++)
    {
        _arraySetList[i] = NULL;
        _arrayGetList[i] = NULL;
    }

    _arraySetList[0] = strSMB_DOUBLE_STORAGE;
    _arraySetList[1] = strSMB_INT32_STORAGE;
    _arraySetList[2] = strSMB_ID;
    _arraySetList[3] = strSMB_LOG;

    _arrayGetList[0] = strSMB_DOUBLE_STORAGE;
    _arrayGetList[1] = strSMB_INT32_STORAGE;
    _arrayGetList[2] = strSMB_ID;
    _arrayGetList[3] = strSMB_LOG;

    _nPrevState = STATEMACHINE_STATE_NA;
    _nState = STATEMACHINE_STATE_PREPARING;
    _nProceedingState = STATEMACHINE_STATE_NA;
    _bPause = FALSE;
    _bProceed = FALSE;
    _bStop = FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CStateMachineBase::~CStateMachineBase()
{
    ::WaitForSingleObject(_hBinSem, INFINITE);
    _fpGetDoubleStorage = NULL;
    _pContextForDoubleStorage = NULL;
    _fpGetInt32Storage = NULL;
    _pContextForInt32Storage = NULL;
    ::ReleaseSemaphore(_hBinSem, 1, NULL);

    ::CloseHandle(_hBinSem);
    _hBinSem = NULL;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineBase::Set(char *pID, PVOID pData, INT32 nDataUnit, INT32 nDataSize)
{
    for (int i = 0; i < nSMB_NUM_SETTINGS; i++)
    {
        if (!strcmp(pID, _arraySetList[i]))
        {
            switch (i)
            {
            case 0:
                return SetDoubleStorage(pData, nDataUnit, nDataSize);
            case 1:
                return SetInt32Storage(pData, nDataUnit, nDataSize);
            case 2:
                return SetId(pData, nDataUnit, nDataSize);
            case 3:
                return SetLog(pData, nDataUnit, nDataSize);
            }
        }
    }

    return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineBase::Get(char *pID, PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize,
                            fpStateMachineHandMem &fpRetMemHandler)
{
    ppRetData = NULL;
    nRetDataUnit = 0;
    nRetDataSize = 0;
    fpRetMemHandler = NULL;

    for (int i = 0; i < nSMB_NUM_SETTINGS; i++)
    {
        if (!strcmp(pID, _arrayGetList[i]))
        {
            switch (i)
            {
            //case 0:
            //    return GetDoubleStorage(ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
            //case 1:
            //    return GetInt32Storage(ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
            case 2:
                return GetId(ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
            case 3:
                return GetLog(ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
            }
        }
    }
    return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
INT32 CStateMachineBase::RunState(TStateMachineParam *pInput, INT32 &errorCode, char **ppRetMsg, BOOL bHandleRetMsg)
{
    INT32 locCurrentState = _nState; // saving current state

    if (_bPause)
    {
        _bPause = FALSE;
        _nProceedingState = locCurrentState; // before setting, record current state.
        _nState = locCurrentState = STATEMACHINE_STATE_PAUSING;
    }

    if (_bStop)
    {
        _bStop = FALSE;
        _nState = locCurrentState = STATEMACHINE_STATE_STOP;
    }

    BOOL locIs1stChanging = locCurrentState != _nPrevState; // check 1st changing state
    _nPrevState = locCurrentState; // config previous to current
    switch (locCurrentState)
    {
    case STATEMACHINE_STATE_PREPARING:
        _bPause = FALSE;
        _bProceed = FALSE;
        _bStop = FALSE;

        // 1. 1st part, first change state
        if (locIs1stChanging)
        {
            _nDummyRun = 1000;
            _nDummyRunCount = 0;
        }
        // 2. 1st, 2nd, ... across
        _nState = STATEMACHINE_STATE_DUMMY_RUN;

        break;

    case STATEMACHINE_STATE_DUMMY_RUN:
        if (++_nDummyRunCount >= _nDummyRun)
            _nState = STATEMACHINE_STATE_NORMAL_END;
        ::Sleep(100);
        break;

    case STATEMACHINE_STATE_NORMAL_END:
        _nState = STATEMACHINE_STATE_PREPARING; // back to 1st step of state machine
        return RET_STATUS_OK;

    case STATEMACHINE_STATE_STOP:
        // MUST choose one state to end. STATEMACHINE_STATE_ERROR or STATEMACHINE_STATE_NORMAL_END
        _nState = STATEMACHINE_STATE_NORMAL_END;
        break;

    case STATEMACHINE_STATE_PAUSING:
        if (_bProceed)
        {
            _bProceed = FALSE;
            _nState = _nProceedingState;
            return RET_STATUS_PAUSE_RELEASED;
        }
        return RET_STATUS_PAUSED;

    case STATEMACHINE_STATE_ERROR:
        _nState = STATEMACHINE_STATE_PREPARING; // back to 1st step of state machine
        return RET_STATUS_NG;
    }

    return RET_STATUS_RUNNING;
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
BOOL CStateMachineBase::SetDoubleStorage(PVOID pData, INT32 nDataUnit, INT32 nDataSize)
{
    BOOL ret = FALSE;

    ::WaitForSingleObject(_hBinSem, INFINITE);
    if (nDataUnit == sizeof(SMBDataStorage) && nDataSize == 1)
    {
        SMBDataStorage *pConv = (SMBDataStorage *)pData;
        _fpGetDoubleStorage = (fpStateMachineGetDoubleSMem) pConv->_fpCallback;
        _pContextForDoubleStorage = pConv->_pContext;
        ret = TRUE;
    }
    ::ReleaseSemaphore(_hBinSem, 1, NULL);

    return ret;
}
//BOOL CStateMachineBase::GetDoubleStorage(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler)
//{
//    SMBDataStorage* pMem = AllocBaseDataTypeMem<SMBDataStorage>(1);
//    if (!pMem) return FALSE;
//
//    ::WaitForSingleObject(_hBinSem, INFINITE);
//    pMem->_ppStorages = _ppDoubleStorages;
//    pMem->_nStorage = _nDoubleStorages;
//    ::ReleaseSemaphore(_hBinSem, 1, NULL);
//
//    ppRetData = (PVOID)pMem;
//    nRetDataUnit = sizeof(SMBDataStorage);
//    nRetDataSize = 1;
//    fpRetMemHandler = FreeBaseDataTypeMem;
//    return TRUE;
//}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
BOOL CStateMachineBase::SetInt32Storage(PVOID pData, INT32 nDataUnit, INT32 nDataSize)
{
    BOOL ret = FALSE;

    ::WaitForSingleObject(_hBinSem, INFINITE);
    if (nDataUnit == sizeof(SMBDataStorage) && nDataSize == 1)
    {
        SMBDataStorage *pConv = (SMBDataStorage *)pData;
        _fpGetInt32Storage = (fpStateMachineGetInt32SMem)pConv->_fpCallback;
        _pContextForInt32Storage = pConv->_pContext;
        ret = TRUE;
    }
    ::ReleaseSemaphore(_hBinSem, 1, NULL);

    return ret;
}
//BOOL CStateMachineBase::GetInt32Storage(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler)
//{
//    SMBDataStorage* pMem = AllocBaseDataTypeMem<SMBDataStorage>(1);
//    if (!pMem) return FALSE;
//
//    ::WaitForSingleObject(_hBinSem, INFINITE);
//    pMem->_ppStorages = _ppDoubleStorages;
//    pMem->_nStorage = _nDoubleStorages;
//    ::ReleaseSemaphore(_hBinSem, 1, NULL);
//
//    ppRetData = (PVOID)pMem;
//    nRetDataUnit = sizeof(SMBDataStorage);
//    nRetDataSize = 1;
//    fpRetMemHandler = FreeBaseDataTypeMem;
//    return TRUE;
//}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
BOOL CStateMachineBase::SetId(PVOID pData, INT32 nDataUnit, INT32 nDataSize)
{
    if (!pData || nDataUnit != sizeof(char) || nDataSize <= 0 || nDataSize >= sizeof(_strID))
        return FALSE;

    char *pConv = (char*)pData;
    memcpy(_strID, pData, nDataSize * nDataUnit);
    _strID[nDataSize] = 0;
    return TRUE;
}
BOOL CStateMachineBase::GetId(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler)
{
    if (strlen(_strID) <= 0)
    {
        return TRUE;
    }

    size_t sz = strlen(_strID);
    char *pMem = AllocBaseDataTypeMem<char>((UINT32)sz + 1);
    if (!pMem)
        return FALSE;

    memcpy(pMem, _strID, sz);
    pMem[sz] = 0;

    ppRetData = (PVOID)pMem;
    nRetDataUnit = sizeof(char);
    nRetDataSize = (INT32)sz;
    fpRetMemHandler = FreeBaseDataTypeMem;
    return TRUE;
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
BOOL CStateMachineBase::SetLog(PVOID pData, INT32 nDataUnit, INT32 nDataSize)
{
    if (!pData || nDataUnit != sizeof(fpStateMachineLog) || nDataSize != 1)
        return FALSE;

    _fpLog = (fpStateMachineLog)pData;
    return TRUE;
}
BOOL CStateMachineBase::GetLog(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler)
{
    ppRetData = (PVOID)_fpLog;
    nRetDataUnit = sizeof(fpStateMachineLog);
    nRetDataSize = 1;
    return TRUE;
}
