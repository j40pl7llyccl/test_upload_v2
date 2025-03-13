#include "stdafx.h"
#include "UCBlockRunnerWin32.h"

static char* StateName(tYUX_I32 code);

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
UCBlockRunnerWin32::UCBlockRunnerWin32(UCCommunicationManager *pCommMgr, 
    UCSharedMemFormating *pShMemFormating,
    UCObject *pEnvShMem32, UCObject *pEnvShMem64, UCObject *pEnvShMemDf,
    int threadPriority)
    : UCThreadWin32()
{
    _strClassRttiName = typeid(*this).name();
    _strGivenName[0] = 0;
    _pBlockManager = new UCBlockManager(pCommMgr, pShMemFormating, pEnvShMem32, pEnvShMem64, pEnvShMemDf);
    _hEvent = ::CreateEvent(NULL, TRUE, FALSE, NULL);
    _hSyncSem = ::CreateSemaphore(NULL, 1, 1, NULL);

    _hSyncRunListSem = ::CreateSemaphore(NULL, 1, 1, NULL);
    _nWorkSN = 1;

    _nAWorkStateOfBlock = BLOCKRUNNING_STATE_NA;
    _pCurrRunningBlock  = NULL;

    _bIsLog = FALSE;
    _fpLog = NULL;

    _hPauseEvent   = ::CreateEvent(NULL, FALSE, FALSE, NULL); // auto-reset
    _hProceedEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL); // auto-reset
    _hStopEvent    = ::CreateEvent(NULL, FALSE, FALSE, NULL); // auto-reset

    m_iPriority = threadPriority;
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
UCBlockRunnerWin32::~UCBlockRunnerWin32()
{
    // stop Exec
    Terminate();
    ::SetEvent(_hEvent);
    Destroy();

    // free work
    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);
    std::list<WorkOfBlock*>::iterator it;
    while (_WorkList.size() > 0)
    {
        it = _WorkList.begin();
        WorkOfBlock *pWork = (WorkOfBlock *)(*it);
        delete pWork;

        _WorkList.erase(it);
    }
    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

    // free block manager
    ::WaitForSingleObject(_hSyncSem, INFINITE);
    if (_pBlockManager) delete _pBlockManager;
    _pBlockManager = NULL;
    ::ReleaseSemaphore(_hSyncSem, 1, NULL);

    // close windows resources
    ::CloseHandle(_hSyncRunListSem); _hSyncRunListSem = NULL;
    ::CloseHandle(_hSyncSem); _hSyncSem = NULL;
    ::CloseHandle(_hEvent); _hEvent = NULL;
    ::CloseHandle(_hPauseEvent); _hPauseEvent = NULL;
    ::CloseHandle(_hProceedEvent); _hProceedEvent = NULL;
    ::CloseHandle(_hStopEvent); _hStopEvent = NULL;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
void UCBlockRunnerWin32::SetLogCallback(fpUCBlockLog fp)
{
    _fpLog = fp;

    if (_pBlockManager)
    {
        size_t n = _pBlockManager->GetNumOfBlocks();
        for (size_t i = 0; i < n; i++)
        {
            UCBlockBase* pBlock = _pBlockManager->GetBlockByIndex(i);
            if (!pBlock)
                continue;

            pBlock->Set(strUCB_LOG, fp, sizeof(fpUCBlockLog), 1);
        }
    }
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_BOOL UCBlockRunnerWin32::AddBlock(UCBlockBase *pBlock, tYUX_BOOL bHandledByThisClass)
{
    if (!_pBlockManager) return FALSE;
    if (_nAWorkStateOfBlock > BLOCKRUNNING_STATE_WAIT)
    {
        printf("[UCBlockRunnerWin32::AddBlock]-Error- runner is working!\n");
        if (bHandledByThisClass)
            delete pBlock;
        return FALSE;
    }

    if (::WaitForSingleObject(_hSyncSem, INFINITE) != WAIT_OBJECT_0)
    {
        if (bHandledByThisClass)
            delete pBlock;
        return FALSE;
    }

    tYUX_BOOL ret = _pBlockManager->AddBlock(pBlock, bHandledByThisClass);
    if (ret && bHandledByThisClass)
        pBlock->SetOwner(this);

    ::ReleaseSemaphore(_hSyncSem, 1, NULL);
    return ret;
}
tYUX_BOOL UCBlockRunnerWin32::RmvBlock(char *pBlockId)
{
    if (!_pBlockManager) return FALSE;
    if (_nAWorkStateOfBlock > BLOCKRUNNING_STATE_WAIT)
    {
        printf("[UCBlockRunnerWin32::RmvBlock]-Error- runner is working!\n");
        return FALSE;
    }
    if (::WaitForSingleObject(_hSyncSem, INFINITE) != WAIT_OBJECT_0)
    {
        return FALSE;
    }

    tYUX_BOOL ret = _pBlockManager->RmvBlock(pBlockId);

    ::ReleaseSemaphore(_hSyncSem, 1, NULL);
    return ret;
}
tYUX_BOOL UCBlockRunnerWin32::CallBlockSetMethod(char *pBlockId, char *pNameForSet, 
    tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (!_pBlockManager) return FALSE;
    if (_nAWorkStateOfBlock > BLOCKRUNNING_STATE_WAIT)
    {
        printf("[UCBlockRunnerWin32::CallBlockSetMethod]-Error- runner is working!\n");
        return FALSE;
    }
    if (::WaitForSingleObject(_hSyncSem, INFINITE) != WAIT_OBJECT_0)
    {
        return FALSE;
    }

    tYUX_BOOL ret = _pBlockManager->CallBlockSetMethod(pBlockId, pNameForSet, pData, nDataUnit, nDataSize);

    ::ReleaseSemaphore(_hSyncSem, 1, NULL);
    return ret;
}
tYUX_BOOL UCBlockRunnerWin32::CallBlockGetMethod(char *pBlockId, char *pNameForGet, 
    tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler)
{
    if (!_pBlockManager) return FALSE;
    if (_nAWorkStateOfBlock > BLOCKRUNNING_STATE_WAIT)
    {
        printf("[UCBlockRunnerWin32::CallBlockGetMethod]-Error- runner is working!\n");
        return FALSE;
    }
    if (::WaitForSingleObject(_hSyncSem, INFINITE) != WAIT_OBJECT_0)
    {
        return FALSE;
    }

    tYUX_BOOL ret = _pBlockManager->CallBlockGetMethod(pBlockId, pNameForGet, ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);

    ::ReleaseSemaphore(_hSyncSem, 1, NULL);
    return ret;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
tYUX_U32 UCBlockRunnerWin32::AddWork(char *pBlockId, 
    tYUX_PVOID pRunParam, tYUX_I32 nParamUnit, tYUX_I32 nParamSize, fpUCBlockHandleMem fpParamHandler,
    fpBlockRunWorkDoneCallback fpNotifyBeg, tYUX_PVOID pContextBeg,
    fpBlockRunWorkDoneCallback fpNotifyEnd, tYUX_PVOID pContextEnd,
    tYUX_BOOL bHandleOnFail)
{
    if (m_bTerminate || !pBlockId || strlen(pBlockId) <= 0)
    {
        if (bHandleOnFail)
        {
            if (pRunParam && fpParamHandler)
                fpParamHandler(pRunParam);
        }
        if (fpNotifyBeg)
            fpNotifyBeg(0, pContextBeg, BLOCKWORK_RETURNCODE_PARAMERR);
        return 0;
    }

    tYUX_U32 nCurrSN = 0;
    tYUX_BOOL bTrg = FALSE;
    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);

    // alloc mem
    WorkOfBlock *pWork = new WorkOfBlock();
    if (pWork) pWork->_pParam = new TUCBlockMutableParam();

    if (pWork && pWork->_pParam)
    {
        // config sn
        nCurrSN = _nWorkSN == 0xffffffff ? 1 : _nWorkSN++;
        pWork->_nSN = nCurrSN;
        // config parameter for a block
        pWork->_pParam->_pData = pRunParam;
        pWork->_pParam->_nUnit = nParamUnit;
        pWork->_pParam->_nSize = nParamSize;
        pWork->_pParam->_fpMemHandler = fpParamHandler;
        // belong to which block to do
        sprintf_s(pWork->_strBlockId, sizeof(pWork->_strBlockId) - 1, "%s", pBlockId);
        // config the callback information
        pWork->_pNotifyBegContext = pContextBeg;
        pWork->_fpNotifyBeg = fpNotifyBeg;
        pWork->_pNotifyEndContext = pContextEnd;
        pWork->_fpNotifyEnd = fpNotifyEnd;
        // add to list
        _WorkList.push_back(pWork);
        bTrg = TRUE;
    }
    else
    {
        if (pWork) delete pWork;
    }

    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

    // do error handling
    if (!bTrg)
    {
        if (bHandleOnFail)
        {
            if (pRunParam && fpParamHandler)
                fpParamHandler(pRunParam);
        }
        if (fpNotifyBeg)
            fpNotifyBeg(0, pContextBeg, BLOCKWORK_RETURNCODE_NOMEM);
        return 0;
    }

    // success to trigger thread working
    ::SetEvent(_hEvent);
    return nCurrSN;
}
tYUX_BOOL UCBlockRunnerWin32::RmvWork(tYUX_U32 sn)
{
    if (m_bTerminate) return FALSE;

    WorkOfBlock *pWork = NULL;
    list<WorkOfBlock*>::iterator it;

    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);

    for (it = _WorkList.begin(); it != _WorkList.end(); it++)
    {
        pWork = (WorkOfBlock*)(*it);
        if (pWork->_nSN == sn)
        {
            _WorkList.erase(it);
            break;
        }
    }

    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

    if (pWork)
    {
        if (pWork->_fpNotifyBeg)
            pWork->_fpNotifyBeg(pWork->_nSN, pWork->_pNotifyBegContext, BLOCKWORK_RETURNCODE_REMOVESN);
        delete pWork;
        return TRUE;
    }
    return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
void UCBlockRunnerWin32::Pause()
{
    if (m_bTerminate)
        return;
    if (_nAWorkStateOfBlock == BLOCKRUNNING_STATE_PAUSING) // already doing pause
        return;
    if (_hPauseEvent)
        ::SetEvent(_hPauseEvent);
}
tYUX_BOOL UCBlockRunnerWin32::Proceed()
{
    if (m_bTerminate)
        return FALSE;

    if (_nAWorkStateOfBlock != BLOCKRUNNING_STATE_WAIT_PROCEEDING) // not in wait proceeding
        return FALSE;

    if (_hProceedEvent)
        ::SetEvent(_hProceedEvent);
    return TRUE;
}
void UCBlockRunnerWin32::Stop()
{
    if (m_bTerminate)
        return;
    if (_nAWorkStateOfBlock == BLOCKRUNNING_STATE_PROC_STOP) // already in stop
        return;
    if (_hStopEvent)
        ::SetEvent(_hStopEvent);
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
void UCBlockRunnerWin32::Execute(void)
{
    tYUX_I32 nRetCode;
    list<WorkOfBlock*>::iterator it;
    WorkOfBlock *pCurrWork = NULL;
    char logMsg[512];
    tYUX_I32 nRunBlockErrorState = 0;

    while (!m_bTerminate)
    {
        // process receive stop event
        if (::WaitForSingleObject(_hStopEvent, 0) == WAIT_OBJECT_0)
        {
            if (_pCurrRunningBlock)
            {
                _pCurrRunningBlock->Stop();
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_PROC_STOP;
            }
        }

        switch (_nAWorkStateOfBlock)
        {
        case BLOCKRUNNING_STATE_WAIT:
            if (::WaitForSingleObject(_hEvent, 1000) == WAIT_OBJECT_0)
                ::ResetEvent(_hEvent);
            if (_WorkList.size() > 0)
            {
                if (_fpLog && pCurrWork)
                {
                    _fpLog("[UCBlockRunnerWin32::Exec()] pCurrWork not null in state BLOCKRUNNING_STATE_WAIT!", 1);
                }
                // work come in
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_GETWORK;
                _pCurrRunningBlock = NULL;
                pCurrWork = NULL;
            }
            break;

        case BLOCKRUNNING_STATE_GETWORK:
            if (::WaitForSingleObject(_hSyncRunListSem, 0) == WAIT_OBJECT_0)
            {
                if (_fpLog && pCurrWork)
                {
                    _fpLog("[UCBlockRunnerWin32::Exec()] pCurrWork not null in state BLOCKRUNNING_STATE_GETWORK!", 1);
                }

                if (_WorkList.size() > 0)
                {
                    it = _WorkList.begin();
                    pCurrWork = (WorkOfBlock *)(*it);
                    _WorkList.erase(it);
                }
                else
                    _nAWorkStateOfBlock = BLOCKRUNNING_STATE_WAIT;
                ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

                if (pCurrWork)
                {
                    _pCurrRunningBlock = _pBlockManager->GetBlock(pCurrWork->_strBlockId);
                    if (!_pCurrRunningBlock)
                    {
                        if (pCurrWork->_fpNotifyBeg)
                            pCurrWork->_fpNotifyBeg(pCurrWork->_nSN, pCurrWork->_pNotifyBegContext, BLOCKWORK_RETURNCODE_BLOCKNOTFOUNF);

                        // not found the block, goto BLOCKRUNNING_STATE_RECYCLE_WORK
                        _nAWorkStateOfBlock = BLOCKRUNNING_STATE_RECYCLE_WORK;
                        break;
                    }

                    if (pCurrWork->_fpNotifyBeg)
                        pCurrWork->_fpNotifyBeg(pCurrWork->_nSN, pCurrWork->_pNotifyBegContext, BLOCKWORK_RETURNCODE_NO_ERROR);

                    // work ready to run
                    _nAWorkStateOfBlock = BLOCKRUNNING_STATE_RUNNING;
                    // reset all event
                    ::ResetEvent(_hPauseEvent);
                    ::ResetEvent(_hProceedEvent);
                }
            }
            break;

        case BLOCKRUNNING_STATE_RUNNING:
            // Check pause event coming
            if (::WaitForSingleObject(_hPauseEvent, 0) == WAIT_OBJECT_0)
            {
                if (_pCurrRunningBlock->State() != UCBLOCK_STATE_PAUSING)
                {
                    _pCurrRunningBlock->Pause();
                    _nAWorkStateOfBlock = BLOCKRUNNING_STATE_PAUSING;
                    break;
                }
            }

            RunWorkOfBlock(_pCurrRunningBlock, pCurrWork, _nAWorkStateOfBlock, nRetCode);
            if (nRetCode == RET_STATUS_NG)
                nRunBlockErrorState = _nAWorkStateOfBlock;
            break;

#pragma region PAUSING_CONDITION
        case BLOCKRUNNING_STATE_PAUSING:
            RunWorkOfBlock(_pCurrRunningBlock, pCurrWork, _nAWorkStateOfBlock, nRetCode);
            if (nRetCode == RET_STATUS_NG)
                nRunBlockErrorState = _nAWorkStateOfBlock; // record current state
            else if (nRetCode == RET_STATUS_PAUSED)
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_WAIT_PROCEEDING;

            break;

        case BLOCKRUNNING_STATE_WAIT_PROCEEDING:
            // check proceed event coming
            if (::WaitForSingleObject(_hProceedEvent, 0) == WAIT_OBJECT_0)
            {
                _pCurrRunningBlock->Proceed();
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_PROC_PROCEEDING;
                break;
            }

            // running SM
            RunWorkOfBlock(_pCurrRunningBlock, pCurrWork, _nAWorkStateOfBlock, nRetCode);
            if (nRetCode == RET_STATUS_NG)
                nRunBlockErrorState = _nAWorkStateOfBlock;
            else if (nRetCode == RET_STATUS_PAUSED)
                ::Sleep(10);
            break;

        case BLOCKRUNNING_STATE_PROC_PROCEEDING:
            RunWorkOfBlock(_pCurrRunningBlock, pCurrWork, _nAWorkStateOfBlock, nRetCode);
            if (nRetCode == RET_STATUS_NG)
                nRunBlockErrorState = _nAWorkStateOfBlock;
            else if (nRetCode == RET_STATUS_PAUSE_RELEASED)
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_RUNNING;
            break;
#pragma endregion PAUSING_CONDITION

#pragma region STOP_CONDITION
        case BLOCKRUNNING_STATE_PROC_STOP:
            // if have SM running, wait to end
            RunWorkOfBlock(_pCurrRunningBlock, pCurrWork, _nAWorkStateOfBlock, nRetCode);
            if (nRetCode == RET_STATUS_NG)
                nRunBlockErrorState = _nAWorkStateOfBlock;
            else if(nRetCode == RET_STATUS_OK)
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_PROC_STOP_DONE;
            break;

        case BLOCKRUNNING_STATE_PROC_STOP_DONE:
            // clear work list
            ClearWorkList(BLOCKWORK_RETURNCODE_STOP);
            // change recycle work
            _nAWorkStateOfBlock = BLOCKRUNNING_STATE_RECYCLE_WORK;
            break;
#pragma endregion STOP_CONDITION

        case BLOCKRUNNING_STATE_RECYCLE_WORK:
            if (pCurrWork) delete pCurrWork;
            pCurrWork = NULL;
            _pCurrRunningBlock = NULL;
            _nAWorkStateOfBlock = BLOCKRUNNING_STATE_WAIT;
            break;

        case BLOCKRUNNING_STATE_RUNBLOCK_ERROR:
            // clear all works in list
            ClearWorkList(BLOCKWORK_RETURNCODE_STATEERR);
            // log message
            if (_fpLog)
            {
                sprintf_s(logMsg, sizeof(logMsg) - 1, "[UCBlockRunnerWin32::Exec()]-Error- run block(%s) with error in state %s and following works will be removed!\n", pCurrWork->_strBlockId, StateName(nRunBlockErrorState));
                _fpLog(logMsg, 0);
            }
            // recycle work memory
            if (pCurrWork) delete pCurrWork;
            pCurrWork = NULL;
            _pCurrRunningBlock = NULL;
            // switch to wait state
            _nAWorkStateOfBlock = BLOCKRUNNING_STATE_WAIT;
            break;

        default:
            _nAWorkStateOfBlock = BLOCKRUNNING_STATE_WAIT;
            break;
        } // end-switch

        // process receive stop event
        if (::WaitForSingleObject(_hStopEvent, 0) == WAIT_OBJECT_0)
        {
            if (_pCurrRunningBlock)
            {
                _pCurrRunningBlock->Stop();
                _nAWorkStateOfBlock = BLOCKRUNNING_STATE_PROC_STOP;
            }
        }

        // prevent program halt
        //::Sleep(10);
    }

    // recycle the work memory
    if (pCurrWork)
        delete pCurrWork;
    pCurrWork = NULL;
}

///////////////////////////////////////////////////////////////////////////////
// Description
//  - Running the block
//  - Switch next state of _nAWorkStateOfBlock in conditions
//    * RET_STATUS_NG: done in error -> callback to notify error
//    * RET_STATUS_OK: done in OK -> callback to notify ok
///////////////////////////////////////////////////////////////////////////////
void UCBlockRunnerWin32::RunWorkOfBlock(UCBlockBase *pBlock, WorkOfBlock *pWork, 
    tYUX_I32 &nextState, tYUX_I32 &retOfBlock)
{
    tYUX_I32 errCode;
    tYUX_BOOL bHandleMsg = FALSE;
    char *pMsg = NULL;

    bHandleMsg = FALSE;
    pMsg = NULL;
    // Exec the block
    retOfBlock = pBlock->Run(pWork->_pParam, errCode, &pMsg, bHandleMsg);
    // proc the message
    if (pMsg && _bIsLog && _fpLog)
        _fpLog(pMsg, 0);
    // handle the message
    if (bHandleMsg && pMsg)
        UCBlockBase::FreeBaseDataTypeMem(pMsg);

    switch (retOfBlock)
    {
    case RET_STATUS_NG:
        if (pWork->_fpNotifyEnd)
            pWork->_fpNotifyEnd(pWork->_nSN, pWork->_pNotifyEndContext, BLOCKWORK_RETURNCODE_BLOCKEXECERR);
        //nextState = BLOCKRUNNING_STATE_GETWORK;
        nextState = BLOCKRUNNING_STATE_RUNBLOCK_ERROR;
        break;
    case RET_STATUS_OK:
        if (pWork->_fpNotifyEnd)
            pWork->_fpNotifyEnd(pWork->_nSN, pWork->_pNotifyEndContext, BLOCKWORK_RETURNCODE_NO_ERROR);
        //nextState = BLOCKRUNNING_STATE_GETWORK;
        nextState = BLOCKRUNNING_STATE_RECYCLE_WORK;
        break;
    }
}

void UCBlockRunnerWin32::ClearWorkList(tYUX_I32 returnCode)
{
    list<WorkOfBlock*>::iterator it;
    WorkOfBlock *pWork;

    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);
    while (_WorkList.size() > 0)
    {
        it = _WorkList.begin();
        pWork = (WorkOfBlock *)(*it);
        _WorkList.erase(it);
        if (pWork->_fpNotifyBeg) pWork->_fpNotifyBeg(pWork->_nSN, pWork->_pNotifyBegContext, returnCode);
        if (pWork->_fpNotifyEnd) pWork->_fpNotifyEnd(pWork->_nSN, pWork->_pNotifyEndContext, returnCode);
        delete pWork; pWork = NULL;
    }
    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
char* StateName(tYUX_I32 code)
{
    switch (code)
    {
    case BLOCKRUNNING_STATE_NA: return "[NA]";
    case BLOCKRUNNING_STATE_WAIT: return "[Wait]";
    case BLOCKRUNNING_STATE_GETWORK: return "[Get work]";
    case BLOCKRUNNING_STATE_RUNNING: return "[Run Block]";
    case BLOCKRUNNING_STATE_PAUSING: return "[Pausing]";
    case BLOCKRUNNING_STATE_WAIT_PROCEEDING: return "[Wait proceeding]";
    case BLOCKRUNNING_STATE_PROC_PROCEEDING: return "[Process proceeding]";
    case BLOCKRUNNING_STATE_PROC_STOP: return "[Process stop]";
    case BLOCKRUNNING_STATE_PROC_STOP_DONE: return "[Process stop done]";
    case BLOCKRUNNING_STATE_RECYCLE_WORK: return "[Recycle work]";
    case BLOCKRUNNING_STATE_RUNBLOCK_ERROR: return "[Run block error]";
    }

    return "[Unkonwn]";
}
