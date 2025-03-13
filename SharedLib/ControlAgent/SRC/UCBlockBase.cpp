#include "stdafx.h"
#include "UCBlockBase.h"

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
UCBlockBase::UCBlockBase()
    : UCObject()
{
    _strClassRttiName = typeid(*this).name();
    InitResources();
}

UCBlockBase::UCBlockBase(char *pGivenName)
{
    InitResources();

    if(pGivenName && strlen(pGivenName) > 0)
    {
        if(strlen(pGivenName) < (sizeof(_strID) - 1) )
            sprintf_s(_strID, sizeof(_strID) - 1, "%s", pGivenName);
        else
        {
            int cpLen = sizeof(_strID) - 1;
            memcpy(_strID, pGivenName, cpLen);
            _strID[cpLen] = 0;
        }
    }
}

void UCBlockBase::InitResources()
{
    _pCommunicationMgr = 0;
    _pSharedMems = 0;

    _strID[0] = 0;
    _fpLog = 0;
    _fpStateChangedCall = 0;
    _fpInformOut = 0;
    for (int i = 0; i < nUCB_NUM_SETTINGS; i++)
    {
        _arraySetList[i] = 0;
        _arrayGetList[i] = 0;
    }

    _arraySetList[0] = strUCB_ID;
    _arraySetList[1] = strUCB_LOG;
    _arraySetList[2] = strUCB_STATE_CHANGE_CALLBACK;
    _arraySetList[3] = strUCB_ASSISTANT_STATE_CHANGE_CALLBACK;
    _arraySetList[4] = strUCB_INFORM_OUT;
    _arraySetList[5] = strUCB_COMMUNICATION_MGR;

    _arrayGetList[0] = strUCB_ID;
    _arrayGetList[1] = strUCB_LOG;
    _arrayGetList[2] = strUCB_STATE_CHANGE_CALLBACK;
    _arrayGetList[3] = strUCB_ASSISTANT_STATE_CHANGE_CALLBACK;
    _arrayGetList[4] = strUCB_INFORM_OUT;
    _arrayGetList[5] = strUCB_COMMUNICATION_MGR;

    _nPrevState       = UCBLOCK_STATE_NA;
    _nState           = UCBLOCK_STATE_PREPARING;
    _nProceedingState = UCBLOCK_STATE_NA;
    _bPause           = false;
    _bProceed         = false;
    _bStop            = false;

    // History of block states
    _nMaxStoredBlockState = 0;

    // Pattern runner
    _nProceedingBackState = UCBLOCK_STATE_NA;
    _p1stStateHandlerContext = (tYUX_PVOID)0;
    _fp1stStateHandler = (fpUCBlockStateCallback)0;
    _pDesignedStateHandlerContext = (tYUX_PVOID)0;
    _fpDesignedStateHandler = (fpUCBlockStateCallback)0;

    // Assistants' control block
    _pAssistants = 0;
    _nAssistants = 0;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
UCBlockBase::~UCBlockBase()
{
    ClearInput();
    ClearOutput();
    if (_pAssistants) delete[] _pAssistants;
    _pAssistants = 0;
    _nAssistants = 0;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
void UCBlockBase::ID(char *pId)
{
    if (!pId || strlen(pId) <= 0) return;

    if(strlen(pId) < (sizeof(_strID) - 1) )
        sprintf_s(_strID, sizeof(_strID) - 1, "%s", pId);
    else
    {
        int cpLen = sizeof(_strID) - 1;
        memcpy(_strID, pId, cpLen);
        _strID[cpLen] = 0;
    }
    
    return;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_BOOL UCBlockBase::Set(char *pID, tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    for (int i = 0; i < nUCB_NUM_SETTINGS; i++)
    {
        if (!strcmp(pID, _arraySetList[i]))
        {
            switch (i)
            {
            case 0:
                return SetId(pData, nDataUnit, nDataSize);
            case 1:
                return SetLog(pData, nDataUnit, nDataSize);
            case 2:
                return SetStateChangeCallback(pData, nDataUnit, nDataSize);
            case 3:
                return SetAssistantStateChangeCallback(pData, nDataUnit, nDataSize);
            case 4:
                return SetInformOut(pData, nDataUnit, nDataSize);
            case 5:
                return SetCommunicationInstanceMgr(pData, nDataUnit, nDataSize);
            }
        }
    }

    return false;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_BOOL UCBlockBase::Get(char *pID, 
                           tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, 
                           fpUCBlockHandleMem &fpRetMemHandler)
{
    ppRetData = 0;
    nRetDataUnit = 0;
    nRetDataSize = 0;
    fpRetMemHandler = 0;

    for (int i = 0; i < nUCB_NUM_SETTINGS; i++)
    {
        if (!strcmp(pID, _arrayGetList[i]))
        {
            switch (i)
            {
            case 0:
                return GetId(ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
            case 1:
                return GetLog(ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
            }
        }
    }
    return true;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_BOOL UCBlockBase::SetFormatableSharedMem(UCSharedMemFormating *pInst)
{
    _pSharedMems = pInst;
    return (tYUX_BOOL)true;
}
tYUX_BOOL UCBlockBase::SetEnvironmentSharedMems(UCObject *pEnvI32, UCObject *pEnvI64, UCObject *pEnvDouble)
{
    _pEnvSharedMemInt32 = dynamic_cast<UCDataSync<tYUX_I32>*>(pEnvI32);
    _pEnvSharedMemInt64 = dynamic_cast<UCDataSync<tYUX_I64>*>(pEnvI64);
    _pEnvSharedMemDouble = dynamic_cast<UCDataSync<double>*>(pEnvDouble);
    return (tYUX_BOOL)true;
}


///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_I32 UCBlockBase::Run(TUCBlockMutableParam *pInput, tYUX_I32 &errorCode, char **ppRetMsg, 
                          tYUX_BOOL &bHandleRetMsg)
{
    return PatternRunner_1( pInput->_pData, pInput->_nUnit, (tYUX_I32)pInput->_nSize, errorCode, ppRetMsg, bHandleRetMsg);
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_I32 UCBlockBase::PatternRunner_1(tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_I32 &errorCode, char **ppRetMsg, tYUX_BOOL &bHandleRetMsg)
{
    tYUX_BOOL bError = (tYUX_BOOL)false;
    //tYUX_BOOL locStopDone = (tYUX_BOOL)false;
    tYUX_BOOL locStopDoneErr = (tYUX_BOOL)false;
    tYUX_BOOL locPauseErr = (tYUX_BOOL)false;
    //tYUX_BOOL locProceedDone = (tYUX_BOOL)false;
    tYUX_BOOL locProceedDoneErr = (tYUX_BOOL)false;
    tYUX_I32 locPrevState = _nPrevState;
    tYUX_I32 locCurrState = _nState;

    if (_bPause)
    {
        _bPause = (tYUX_BOOL)false;
        if (locCurrState != UCBLOCK_STATE_ERROR &&
            locCurrState != UCBLOCK_STATE_FINISH &&
            locCurrState != UCBLOCK_STATE_PAUSING &&
            locCurrState != UCBLOCK_STATE_STOP)
        {
            _nProceedingState = locCurrState;
            _nState = locCurrState = UCBLOCK_STATE_PAUSING;
        }
    }

    if (_bStop)
    {
        _bStop = (tYUX_BOOL)false;
        if (locCurrState != UCBLOCK_STATE_ERROR &&
            locCurrState != UCBLOCK_STATE_FINISH &&
            locCurrState != UCBLOCK_STATE_STOP)
        {
            _nState = locCurrState = UCBLOCK_STATE_STOP;
        }
    }

    tYUX_BOOL locFirstChanged = locCurrState != _nPrevState; // check 1st changing state
    _nPrevState = locCurrState; // config previous to current

    AddBlockState(locCurrState);

    switch (locCurrState)
    {
    case UCBLOCK_STATE_PREPARING:
        _bPause = (tYUX_BOOL)false;
        _bProceed = (tYUX_BOOL)false;
        _bStop = (tYUX_BOOL)false;

        ClearBlockKeeper();
        AddBlockState(locCurrState);

        if (!_fp1stStateHandler)
            _nState = UCBLOCK_STATE_ERROR;
        else
            _fp1stStateHandler(_p1stStateHandlerContext, pParams, nSizeofParam, nNumOfParams, locFirstChanged, locCurrState, locPrevState, &_BlockStateStorage, _nState);

        break;

    case UCBLOCK_STATE_FINISH:
        _nState = UCBLOCK_STATE_PREPARING; // back to 1st step of state machine
        return RET_STATUS_OK;

    case UCBLOCK_STATE_STOP:
        if (locFirstChanged)
            AssistantsFirstGotoStopState();

        if (AssistantsHandlingStopState(locStopDoneErr))
            _nState = locStopDoneErr ? UCBLOCK_STATE_ERROR : UCBLOCK_STATE_FINISH;
        break;

    case UCBLOCK_STATE_PAUSING:
        if (locFirstChanged)
            AssistantsFirstGotoPauseState();

        if (!AssistantsHandlingPauseState(locPauseErr))
            break;

        if (locPauseErr)
        {
            _nState = UCBLOCK_STATE_ERROR;
            break;
        }

        if (_bProceed)
        {
            if (!AssistantsHandlingProceed(locProceedDoneErr))
                break;
            if (locProceedDoneErr)
            {
                _nState = UCBLOCK_STATE_ERROR;
                break;
            }

            _bProceed = (tYUX_BOOL)false;
            _nState = _nProceedingBackState;
            return RET_STATUS_PAUSE_RELEASED;
        }
        return RET_STATUS_PAUSED;

    case UCBLOCK_STATE_ERROR:
        _nState = UCBLOCK_STATE_PREPARING;
        return RET_STATUS_NG;

    default:
        if (!_fpDesignedStateHandler)
            _nState = UCBLOCK_STATE_ERROR;
        else
            _fpDesignedStateHandler(_pDesignedStateHandlerContext, pParams, nSizeofParam, nNumOfParams, locFirstChanged, locCurrState, locPrevState, &_BlockStateStorage, _nState);

        break;
    }

    return RET_STATUS_RUNNING;
}

void UCBlockBase::FirstStateInfo(tYUX_PVOID pContext, fpUCBlockStateCallback fpHandler)
{
    _p1stStateHandlerContext = pContext;
    _fp1stStateHandler = fpHandler;
}
void UCBlockBase::DesignedStateInfo(tYUX_PVOID pContext, fpUCBlockStateCallback fpHandler)
{
    _pDesignedStateHandlerContext = pContext;
    _fpDesignedStateHandler = fpHandler;
}
tYUX_I32 UCBlockBase::Pattern1HandleFirstState(tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState)
{
    nNextState = UCBLOCK_STATE_FINISH;
    return RET_STATUS_OK;
}
tYUX_I32 UCBlockBase::Pattern1HandleDesignedState(tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState)
{
    nNextState = UCBLOCK_STATE_FINISH;
    return RET_STATUS_OK;
}

static tYUX_I32 Pattern1StaticCallFirstState(tYUX_PVOID pContext, tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState)
{
    nNextState = UCBLOCK_STATE_ERROR;
    if (!pContext)
        return RET_STATUS_NG;

    UCBlockBase *pBase = (UCBlockBase*)pContext;
    return pBase->Pattern1HandleFirstState(pParams, nSizeofParam, nNumOfParams, bFistEnter, nCurrState, nCurrPrevState, pStateHistory, nNextState);
}
static tYUX_I32 Pattern1StaticCallDesignedState(tYUX_PVOID pContext, tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState)
{
    nNextState = UCBLOCK_STATE_ERROR;
    if (!pContext)
        return RET_STATUS_NG;

    UCBlockBase *pBase = (UCBlockBase*)pContext;
    return pBase->Pattern1HandleDesignedState(pParams, nSizeofParam, nNumOfParams, bFistEnter, nCurrState, nCurrPrevState, pStateHistory, nNextState);
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
tYUX_BOOL UCBlockBase::SetId(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (!pData || nDataUnit != sizeof(char) || nDataSize <= 0 || nDataSize >= sizeof(_strID))
        return false;

    char *pConv = (char*)pData;
    memcpy(_strID, pData, nDataSize * nDataUnit);
    _strID[nDataSize] = 0;
    return true;
}

tYUX_BOOL UCBlockBase::GetId(tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, 
                             fpUCBlockHandleMem &fpRetMemHandler)
{
    if (strlen(_strID) <= 0)
    {
        return true;
    }

    size_t sz = strlen(_strID);
    char *pMem = AllocBaseDataTypeMem<char>((tYUX_U32)sz + 1);
    if (!pMem)
        return false;

    memcpy(pMem, _strID, sz);
    pMem[sz] = 0;

    ppRetData = (tYUX_PVOID)pMem;
    nRetDataUnit = sizeof(char);
    nRetDataSize = (tYUX_I32)sz;
    fpRetMemHandler = FreeBaseDataTypeMem;
    return true;
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
tYUX_BOOL UCBlockBase::SetLog(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (nDataUnit != sizeof(fpUCBlockLog) || nDataSize != 1)
        return false;

    _fpLog = (fpUCBlockLog)pData;
    return true;
}

tYUX_BOOL UCBlockBase::GetLog(tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler)
{
    ppRetData = (tYUX_PVOID)_fpLog;
    nRetDataUnit = sizeof(fpUCBlockLog);
    nRetDataSize = 1;
    return true;
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
tYUX_BOOL UCBlockBase::SetStateChangeCallback(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (nDataUnit != sizeof(fpUCBlockStateChangedCallback) || nDataSize != 1)
        return false;

    _fpStateChangedCall = (fpUCBlockStateChangedCallback)pData;
    return true;
}
tYUX_BOOL UCBlockBase::SetAssistantStateChangeCallback(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    return AssistantsSet(strUCB_STATE_CHANGE_CALLBACK, pData, nDataUnit, nDataSize);
}
tYUX_BOOL UCBlockBase::SetInformOut(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (nDataUnit != sizeof(fpUCBlockInformOut) || nDataSize != 1)
        return false;

    _fpInformOut = (fpUCBlockInformOut)pData;
    return true;
}
tYUX_BOOL UCBlockBase::SetCommunicationInstanceMgr(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (nDataUnit != sizeof(tYUX_PVOID) || nDataSize != 1)
        return false;

    if (pData)
        _pCommunicationMgr = static_cast<UCCommunicationManager*>(pData);
    else
        _pCommunicationMgr = 0;
    return true;
}

//-----------------------------------------------------------------------------
// Formatting shared memory access
//-----------------------------------------------------------------------------
size_t UCBlockBase::GetNumOfFormattingSharedMemory()
{
    return _mapAllocFormattedSharedMem.size();
}
TShMemAccItem* UCBlockBase::GetFormattingSharedMemoryFromIndex(size_t index_0)
{
    if (!_pSharedMems || index_0 < 0 || index_0 >= _mapAllocFormattedSharedMem.size()) return 0;

    map<string, tYUX_PVOID>::iterator it;
    size_t i = 0;
    for (it = _mapAllocFormattedSharedMem.begin(); it != _mapAllocFormattedSharedMem.end(); it++, i++)
    {
        if (index_0 == i)
            return _pSharedMems->ConvertItemHandle(it->second);
    }
    return 0;
}
TShMemAccItem* UCBlockBase::GetFormattingSharedMemoryFromHandle(tYUX_PVOID h)
{
    if (!_pSharedMems) return 0;
    return _pSharedMems->ConvertItemHandle(h);
}
TShMemAccItem* UCBlockBase::GetFormattingSharedMemoryFromIdName(char *pIdName)
{
    map<string, tYUX_PVOID>::iterator it = _mapAllocFormattedSharedMem.find(pIdName);
    if (it == _mapAllocFormattedSharedMem.end()) return 0;

    return _pSharedMems->ConvertItemHandle(it->second);
}

TShMemAccItem* UCBlockBase::CreateSharedMemoryStorageI32(char *pIdName, int nCount)
{
    if (!_pSharedMems || !pIdName || strlen(pIdName) <= 0) return 0;

    map<string, tYUX_PVOID>::iterator it = _mapAllocFormattedSharedMem.find(pIdName);
    if (it != _mapAllocFormattedSharedMem.end())
    {
        return _pSharedMems->ConvertItemHandle(it->second);
    }

    tYUX_PVOID hOfShMem = _pSharedMems->GetAvailableShMemI32(nCount);
    tYUX_PVOID hOfItem;
    if (!_pSharedMems->AddI32(hOfShMem, nCount, hOfItem))
        return 0;

    TShMemAccItem *pItem = static_cast<TShMemAccItem*>(hOfItem);
    pItem->_pUniqueName = _strID;
    pItem->_strId = pIdName;

    _mapAllocFormattedSharedMem.insert(pair<string, tYUX_PVOID>(pIdName, (tYUX_PVOID)hOfItem));
    return pItem;
}
TShMemAccItem* UCBlockBase::CreateSharedMemoryStorageDouble(char *pIdName, int nCount)
{
    if (!_pSharedMems || !pIdName || strlen(pIdName) <= 0) return 0;

    map<string, tYUX_PVOID>::iterator it = _mapAllocFormattedSharedMem.find(pIdName);
    if (it != _mapAllocFormattedSharedMem.end())
    {
        return _pSharedMems->ConvertItemHandle(it->second);
    }

    tYUX_PVOID hOfShMem = _pSharedMems->GetAvailableShMemDouble(nCount);
    tYUX_PVOID hOfItem;
    if (!_pSharedMems->AddDouble(hOfShMem, nCount, hOfItem))
        return 0;

    TShMemAccItem *pItem = static_cast<TShMemAccItem*>(hOfItem);
    pItem->_pUniqueName = _strID;
    pItem->_strId = pIdName;

    _mapAllocFormattedSharedMem.insert(pair<string, tYUX_PVOID>(pIdName, (tYUX_PVOID)hOfItem));
    return pItem;
}
TShMemAccItem* UCBlockBase::CreateSharedMemoryStorageU8(char *pIdName, tYUX_I64 nCount)
{
    if (!_pSharedMems || !pIdName || strlen(pIdName) <= 0) return 0;

    map<string, tYUX_PVOID>::iterator it = _mapAllocFormattedSharedMem.find(pIdName);
    if (it != _mapAllocFormattedSharedMem.end())
    {
        return _pSharedMems->ConvertItemHandle(it->second);
    }

    tYUX_PVOID hOfShMem = _pSharedMems->GetAvailableShMemU8(nCount);
    tYUX_PVOID hOfItem;
    if (!_pSharedMems->AddU8(hOfShMem, nCount, hOfItem))
        return 0;

    TShMemAccItem *pItem = static_cast<TShMemAccItem*>(hOfItem);
    pItem->_pUniqueName = _strID;
    pItem->_strId = pIdName;

    _mapAllocFormattedSharedMem.insert(pair<string, tYUX_PVOID>(pIdName, (tYUX_PVOID)hOfItem));
    return pItem;
}

//-----------------------------------------------------------------------------
// Data operating
//-----------------------------------------------------------------------------
map<string, TUCBlockIChaining *>* UCBlockBase::InputData()
{
    return &_mapInputData;
}
map<string, TUCBlockOChaining *>* UCBlockBase::OutputData()
{
    return &_mapOutputData;
}
void UCBlockBase::ClearInputData()
{
    map<string, TUCBlockIChaining*>::iterator it;
    for (it = _mapInputData.begin(); it != _mapInputData.end(); it++)
    {
        if (!it->second)
            continue;

        if (it->second->_nDataType == UCBLOCKICHAIN_KNOWNTYPE)
        {
            if (it->second->_Data._FlexMem._fpHandleMem && it->second->_Data._FlexMem._pMem)
                it->second->_Data._FlexMem._fpHandleMem(it->second->_Data._FlexMem._pMem);
            it->second->ResetData();
        }
        else if (it->second->_nDataType == UCBLOCKICHAIN_STRINGTYPE)
        {
            if (it->second->_Data._StrMem._pString && it->second->_Data._StrMem._fpHandleStr)
                it->second->_Data._StrMem._fpHandleStr(it->second->_Data._StrMem._pString);
            it->second->ResetData();
        }
        else
            throw new std::exception("[UCBlockBase::ClearInputData()] invalid data type!");

        //delete it->second;
        //it->second = NULL;
    }
}
void UCBlockBase::ClearInput()
{
    map<string, TUCBlockIChaining*>::iterator it;
    while (_mapInputData.size() > 0)
    {
        it = _mapInputData.begin();
        delete it->second;
        it->second = NULL;
        _mapInputData.erase(it);
    }
}
TUCBlockIChaining* UCBlockBase::GetInputDat(const char *pName)
{
    if (!pName || strlen(pName) <= 0)
        return (tYUX_BOOL)false;

    map<string, TUCBlockIChaining *>::iterator it = _mapInputData.find(pName);
    return it == _mapInputData.end() ? NULL : it->second;
}
TUCBlockIChaining* UCBlockBase::NewInputDat(const char *pName)
{
    TUCBlockIChaining* pRet = GetInputDat(pName);
    if (pRet)
        return pRet;

    if (!pName || strlen(pName) <= 0)
        return NULL;

    pRet = new TUCBlockIChaining();
    if (!pRet)
        return NULL;
    _mapInputData.insert(pair<string, TUCBlockIChaining*>(pName, pRet));
    return pRet;
}
void UCBlockBase::DelInputDat(const char *pName)
{
    map<string, TUCBlockIChaining *>::iterator it = _mapInputData.find(pName);
    if (it != _mapInputData.end())
    {
        TUCBlockIChaining *pDat = it->second;
        // remove from map
        _mapInputData.erase(it);
        // delete memory
        if (pDat)
            delete pDat;
    }
}
void UCBlockBase::ClearOutput()
{
    map<string, TUCBlockOChaining *>::iterator it;
    while (_mapOutputData.size() > 0)
    {
        it = _mapOutputData.begin();
        if (it->second)
        {
            delete it->second;
            it->second = NULL;
        }
        _mapOutputData.erase(it);
    }
}
TUCBlockOChaining* UCBlockBase::GetOutputDat(const char *pName)
{
    if (!pName || strlen(pName) <= 0)
        return NULL;
    map<string, TUCBlockOChaining *>::iterator it = _mapOutputData.find(pName);
    return it == _mapOutputData.end() ? NULL : it->second;
}
TUCBlockOChaining* UCBlockBase::NewOutputDat(const char *pName)
{
    TUCBlockOChaining* pRet = GetOutputDat(pName);
    if (pRet)
        return pRet;
    if (!pName || strlen(pName) <= 0)
        return NULL;
    pRet = new TUCBlockOChaining();
    if (!pRet)
        return NULL;
    _mapOutputData.insert(pair<string, TUCBlockOChaining*>(pName, pRet));
    return pRet;
}
void UCBlockBase::DelOutputDat(const char *pName)
{
    map<string, TUCBlockOChaining *>::iterator it = _mapOutputData.find(pName);
    if (it != _mapOutputData.end())
    {
        delete it->second;
        it->second = NULL;
        _mapOutputData.erase(it);
    }
}


//----------------------------------------------------------------------------
// Run block
//----------------------------------------------------------------------------
tYUX_I32 UCBlockBase::RunBlock(UCBlockBase *pBlock, TUCBlockMutableParam *pBlockParamInfo)
{
    if (!pBlock)
        return RET_STATUS_NG;

    tYUX_I32 retErrCode;
    char *pMsg = 0;
    tYUX_BOOL bHandMsg = (tYUX_BOOL)false;
    tYUX_I32 retCode = pBlock->Run(pBlockParamInfo, retErrCode, &pMsg, bHandMsg);
    if (pBlock->LogMethod())
    {
        char dbgMsg[512];
        sprintf_s(dbgMsg, sizeof(dbgMsg) - 1, "[Block-%s-] call with Msg(%s) and code(Normal ret code=%d, Error code=%d)\n", pBlock->ID(), pMsg == 0 ? "" : pMsg, retCode, retErrCode);
        pBlock->LogMethod()(dbgMsg, 0);
    }
    if (pMsg && bHandMsg)
    {
        UCBlockBase::FreeBaseDataTypeMem(pMsg);
    }

    return retCode;
}

tYUX_I32 UCBlockBase::RunBlock(UCBlockBase *pBlock, tYUX_PVOID pParameters, 
    tYUX_I32 nSizeofParams, tYUX_I32 nNumOfParams, fpUCBlockHandleMem fpHandleParameters)
{
    if (!pBlock)
        return RET_STATUS_NG;

    TUCBlockMutableParam locCall;
    locCall._fpMemHandler = fpHandleParameters;
    locCall._pData = pParameters;
    locCall._nUnit = nSizeofParams;
    locCall._nSize = nNumOfParams;

    tYUX_I32 retCode = RunBlock(pBlock, &locCall);

    return retCode;
}

//----------------------------------------------------------------------------
// State History Keeper
//----------------------------------------------------------------------------
void UCBlockBase::ClearBlockKeeper()
{
    _BlockStateStorage.clear();
}
void UCBlockBase::AddBlockState(tYUX_I32 state)
{
    if (_BlockStateStorage.size() <= 0)
    {
        _BlockStateStorage.push_front(state);
        return;
    }

    if (_nMaxStoredBlockState > 0)
    {
        if ((int)_BlockStateStorage.size() >= _nMaxStoredBlockState)
        {
            while ((int)_BlockStateStorage.size() >= _nMaxStoredBlockState)
            {
                _BlockStateStorage.pop_back();
            }
        }
    }

    std::list<tYUX_I32>::iterator it;
    it = _BlockStateStorage.begin();

    if (state == *it) return; // same one, not add

    _BlockStateStorage.push_front(state);
}
bool UCBlockBase::FindBlockPreviousState(tYUX_I32 &ret, tYUX_I32 count)
{
    ret = UCBLOCK_STATE_NA;
    if (_BlockStateStorage.size() <= 0)
        return false;

    std::list<tYUX_I32>::iterator it;
    if (count <= 0)
    {
        it = _BlockStateStorage.begin();
        ret = *it;
        return true;
    }

    int index = 0;
    for (it = _BlockStateStorage.begin(); it != _BlockStateStorage.end(); it++, index++)
    {
        if (index == count)
        {
            ret = *it;
            return true;
        }
    }

    return false;
}
bool UCBlockBase::FindBlockPreviousState(tYUX_I32 firstHitState, tYUX_I32 &ret, tYUX_I32 count)
{
    ret = UCBLOCK_STATE_NA;
    if (_BlockStateStorage.size() <= 0)
        return false;

    std::list<tYUX_I32>::iterator it;
    bool firstHit = false;
    int index = 0;

    for (it = _BlockStateStorage.begin(); it != _BlockStateStorage.end(); it++)
    {
        if (!firstHit && *it == firstHitState)
        {
            firstHit = true;

            if (count <= 0)
            {
                ret = firstHitState;
                return true;
            }

            continue;
        }

        if (firstHit)
        {
            if (++index == count)
            {
                ret = *it;
                return true;
            }
        }
    }

    return false;
}

//----------------------------------------------------------------------------
// Assistants' block
//----------------------------------------------------------------------------
void UCBlockBase::AllocAssistants(tYUX_I32 count)
{
    if (_pAssistants) delete[] _pAssistants;
    _pAssistants = 0;
    _nAssistants = 0;

    if (count <= 0)
        return;

    _pAssistants = new TBlockAssistant[count];
    if (!_pAssistants) return;

    memset((void *)_pAssistants, 0x0, sizeof(TBlockAssistant) * count);
    _nAssistants = count;
}
void UCBlockBase::RestAssistantParams(tYUX_I32 index_0)
{
    if (!_pAssistants || _nAssistants <= 0)
        return;

    TBlockAssistant *p = index_0 < 0 || index_0 >= _nAssistants ? 0 : &_pAssistants[index_0];
    if (!p || !p->_pParams || p->_nSizeofParam <= 0 || p->_nNumOfParams <= 0)
        return;

    memset(p->_pParams, 0x0, p->_nNumOfParams * p->_nSizeofParam);
}
void UCBlockBase::RestAssistantsFlags()
{
    if (!_pAssistants || _nAssistants <= 0)
        return;

    for (int i = 0; i < _nAssistants; i++)
    {
        _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
        _pAssistants[i]._bRunningDone = (tYUX_BOOL)false;
    }
}
TBlockAssistant* UCBlockBase::Assistant(tYUX_I32 index_0)
{
    if (!_pAssistants || _nAssistants <= 0)
        return 0;

    return (TBlockAssistant*)(index_0 < 0 || index_0 >= _nAssistants ? 0 : &_pAssistants[index_0]);
}
tYUX_BOOL UCBlockBase::AssistantsSet(char *pID, tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)false;

    tYUX_BOOL ret = (tYUX_BOOL)true;
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;

        if (!_pAssistants[i]._pAssistant->Set(pID, pData, nDataUnit, nDataSize))
            ret = (tYUX_BOOL)false;
    }

    return ret;
}
//template<typename T>
//T* UCBlockBase::AssistantParameters(tYUX_I32 index_0, tYUX_I32 &count)
//{
//    count = 0;
//    if (!_pAssistants || _nAssistants <= 0)
//        return 0;
//
//    TBlockAssistant *p = index_0 < 0 || index_0 >= _nAssistants ? 0 : &_pAssistants[index_0];
//    if (p->_nSizeofParam != sizeof(T))
//        return 0;
//
//    count = p->_nNumOfParams;
//    return (T*)p->_pParams;
//}

// return:
//  - RET_STATUS_OK: Done and success
//  - RET_STATUS_NG: Done with error
tYUX_I32 UCBlockBase::WorkWithAssistant(tYUX_I32 assistantIndex, tYUX_I32 nBlockStateWhenFinish, tYUX_BOOL bChangeBlock2ErrWhenAssistantFail)
{
    if (!_pAssistants)
    {
        _nState = nBlockStateWhenFinish;
        return RET_STATUS_OK;
    }
    if (assistantIndex < 0 || assistantIndex >= _nAssistants)
    {
        if (_fpLog)
        {
            sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s]-ERROR- WorkWithAssistant() assistantIndex %d out of range!\n", _strID, assistantIndex);
            _fpLog(_strDbgMsg, 0);
        }
        return RET_STATUS_NG;
    }

    TBlockAssistant *pWant = &_pAssistants[assistantIndex];
    if (!pWant->_pAssistant)
    {
        if (_fpLog)
        {
            sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s]-ERROR- WorkWithAssistant() null assistant!\n", _strID);
            _fpLog(_strDbgMsg, 0);
        }
        return RET_STATUS_NG;

    }
    tYUX_I32 locRetCode = RunBlock(pWant->_pAssistant, pWant->_pParams, pWant->_nSizeofParam, pWant->_nNumOfParams);
    if (locRetCode == RET_STATUS_OK)
        _nState = nBlockStateWhenFinish;
    else if (locRetCode == RET_STATUS_NG)
    {
        if (bChangeBlock2ErrWhenAssistantFail)
            _nState = UCBLOCK_STATE_ERROR;
    }

    return locRetCode;
}
// Predefined state: pause
void UCBlockBase::AssistantsFirstGotoPauseState()
{
    if (!_pAssistants)
        return;

    _bAssistantsRunningPausing = (tYUX_BOOL)true;
    // reset all assistants flags
    RestAssistantsFlags();
    // all assistants' state change to pause
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;
        if (_pAssistants[i]._pAssistant->IsRunning())
            _pAssistants[i]._pAssistant->Pause();
    }
}
tYUX_BOOL UCBlockBase::AssistantsRunToPauseState()
{
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)true;

    tYUX_BOOL locDone = (tYUX_BOOL)true;
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;
        if (_pAssistants[i]._bRunningDone)
            continue;
        if (!_pAssistants[i]._pAssistant->IsRunning())
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
            continue;
        }
        // run assistant block
        tYUX_I32 locRet = RunBlock(_pAssistants[i]._pAssistant, _pAssistants[i]._pParams, _pAssistants[i]._nSizeofParam, _pAssistants[i]._nNumOfParams);
        if (locRet == RET_STATUS_PAUSED) // expect state -> done
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
        }
        else if (locRet == RET_STATUS_NG) // done with error
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)true;
        }
        else // unexpect state -> not done yet
            locDone = (tYUX_BOOL)false;
    }

    return locDone;
}
tYUX_BOOL UCBlockBase::AssistantsHandlingPauseState(tYUX_BOOL &bErr)
{
    bErr = (tYUX_BOOL)false;
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)true;

    // need to run assistants pausing
    if (_bAssistantsRunningPausing)
    {
        if (AssistantsRunToPauseState())
        {
            // all done: reset flag
            _bAssistantsRunningPausing = (tYUX_BOOL)false;
            tYUX_BOOL gotErr = (tYUX_BOOL)false;

            if (_fpLog)
            {
                sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s] exec pause done, proceed state = %d.\n", _strID, _nProceedingState);
                _fpLog(_strDbgMsg, 0);
            }
            for (int i = 0; i < _nAssistants; i++)
            {
                if (!_pAssistants[i]._pAssistant)
                    continue;

                if (_pAssistants[i]._bDoneWithError) // check any error
                {
                    gotErr = (tYUX_BOOL)true;
                    if (_fpLog)
                    {
                        sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s] exec %s pause got error.\n", _strID, _pAssistants[i]._pAssistant->ID());
                        _fpLog(_strDbgMsg, 0);
                    }
                }
            }
            if (gotErr) // auto change to error state
            {
                _nState = UCBLOCK_STATE_ERROR;
                bErr = gotErr;
            }
            return (tYUX_BOOL)true;
        }
    }
    else
        return (tYUX_BOOL)true;
    return (tYUX_BOOL)false;
}
// Predefined action in pause state: proceed
void UCBlockBase::AssistantsFirstProceed()
{
    if (!_pAssistants || _nAssistants <= 0)
        return;

    // reset all assistants flags, prepare to check
    RestAssistantsFlags();
    // call to pause
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;
        if (_pAssistants[i]._pAssistant->IsRunning())
            _pAssistants[i]._pAssistant->Proceed();
    }
}
tYUX_BOOL UCBlockBase::AssistantsGoProceeding()
{
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)true;

    tYUX_BOOL locDone = (tYUX_BOOL)true;
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;
        if (_pAssistants[i]._bRunningDone)
            continue;
        if (!_pAssistants[i]._pAssistant->IsRunning())
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
            continue;
        }
        // run assistant block
        tYUX_I32 locRet = RunBlock(_pAssistants[i]._pAssistant, _pAssistants[i]._pParams, _pAssistants[i]._nSizeofParam, _pAssistants[i]._nNumOfParams);
        if (locRet == RET_STATUS_PAUSE_RELEASED) // expect state -> done
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
        }
        else if (locRet == RET_STATUS_NG) // done with error
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)true;
        }
        else // still running
            locDone = (tYUX_BOOL)false;
    }

    return locDone;
}
tYUX_BOOL UCBlockBase::AssistantsHandlingProceed(tYUX_BOOL &bErr)
{
    bErr = (tYUX_BOOL)false;
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)true;

    tYUX_BOOL bDone = AssistantsGoProceeding(); // run all assistants' blocks
    if (bDone)
    {
        tYUX_BOOL gotErr = (tYUX_BOOL)false;
        for (int i = 0; i < _nAssistants; i++)
        {
            // discard null assistant
            if (!_pAssistants[i]._pAssistant)
                continue;

            if (_pAssistants[i]._bDoneWithError) // check any error when run assistant block
            {
                gotErr = (tYUX_BOOL)true;
                if (_fpLog)
                {
                    sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s] exec %s proceeding got error.\n", _strID, _pAssistants[i]._pAssistant->ID());
                    _fpLog(_strDbgMsg, 0);
                }
            }
        }
        if (gotErr)
        {
            _nState = UCBLOCK_STATE_ERROR;
            bErr = gotErr;
        }
    }

    return bDone;
}
// Predefined state: stop
void UCBlockBase::AssistantsFirstGotoStopState()
{
    if (!_pAssistants || _nAssistants <= 0)
        return;

    // reset all assistants flags, prepare to check
    RestAssistantsFlags();
    // call to stop
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;
        if (_pAssistants[i]._pAssistant->IsRunning())
            _pAssistants[i]._pAssistant->Stop();
    }
}
tYUX_BOOL UCBlockBase::AssistantsRunToStopState()
{
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)true;

    tYUX_BOOL isAllDone = (tYUX_BOOL)false;
    tYUX_BOOL locDone = (tYUX_BOOL)true;
    for (int i = 0; i < _nAssistants; i++)
    {
        if (!_pAssistants[i]._pAssistant)
            continue;
        if (_pAssistants[i]._bRunningDone)
            continue;
        if (!_pAssistants[i]._pAssistant->IsRunning())
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
            continue;
        }
        // run assistant block
        tYUX_I32 locRet = RunBlock(_pAssistants[i]._pAssistant, _pAssistants[i]._pParams, _pAssistants[i]._nSizeofParam, _pAssistants[i]._nNumOfParams);
        if (locRet == RET_STATUS_OK) // expect state -> done
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)false;
        }
        else if (locRet == RET_STATUS_NG) // done with error
        {
            _pAssistants[i]._bRunningDone = (tYUX_BOOL)true;
            _pAssistants[i]._bDoneWithError = (tYUX_BOOL)true;
        }
        else // still running
            locDone = (tYUX_BOOL)false;
    }

    isAllDone = locDone;
    return isAllDone;
}
tYUX_BOOL UCBlockBase::AssistantsHandlingStopState(tYUX_BOOL &bErr)
{
    bErr = (tYUX_BOOL)false;
    if (!_pAssistants || _nAssistants <= 0)
        return (tYUX_BOOL)true;

    tYUX_BOOL bDone = AssistantsRunToStopState();
    if (bDone)
    {
        for (int i = 0; i < _nAssistants; i++)
        {
            if (!_pAssistants[i]._pAssistant)
                continue;
            if (_pAssistants[i]._bDoneWithError)
            {
                bErr = (tYUX_BOOL)true;
                if (_fpLog)
                {
                    sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s] exec %s stopping got error.\n", _strID, _pAssistants[i]._pAssistant->ID());
                    _fpLog(_strDbgMsg, 0);
                }
            }
        }
        if (bErr)
        {
            if (_fpLog)
            {
                sprintf_s(_strDbgMsg, sizeof(_strDbgMsg) - 1, "[%s] stopping got error change state to STATE_ERROR.\n", _strID);
                _fpLog(_strDbgMsg, 0);
            }
            _nState = UCBLOCK_STATE_ERROR;
        }
    }

    return bDone;
}

