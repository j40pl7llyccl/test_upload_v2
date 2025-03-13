#include "stdafx.h"
#include "CWin32StateMachineWorker.h"

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CWin32StateMachineWorker::CWin32StateMachineWorker()
    :CStateMachineManager(), CThreadBase()
{
    _hEvent = ::CreateEvent(NULL, TRUE, FALSE, NULL);
    _hSyncSem = ::CreateSemaphore(NULL, 1, 1, NULL);
    _hSyncSharedMem = ::CreateSemaphore(NULL, 1, 1, NULL);

    _hSyncRunListSem = ::CreateSemaphore(NULL, 1, 1, NULL);
    _nJobSerialSN = 1;

    _nSMState = W32SMWORKER_SM_NA;
    _pCurrRunningSM = NULL;

    _bIsLog = FALSE;
    _fpLog = NULL;

    _hPauseEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL); // auto-reset
    _hProceedEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL); // auto-reset
    _hStopEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL); // auto-reset

    //Create(FALSE);
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CWin32StateMachineWorker::~CWin32StateMachineWorker()
{
    // stop Exec
    Terminate();
    ::SetEvent(_hEvent);
    Destroy();

    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);
    std::list<Job2Do*>::iterator it;
    while (_JobList.size() > 0)
    {
        Job2Do *pJob = (Job2Do *)(*it);
        delete pJob;

        _JobList.erase(it);
    }
    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

    ::CloseHandle(_hSyncRunListSem); _hSyncRunListSem = NULL;
    ::CloseHandle(_hSyncSharedMem); _hSyncSharedMem = NULL;
    ::CloseHandle(_hSyncSem); _hSyncSem = NULL;
    ::CloseHandle(_hEvent); _hEvent = NULL;
    ::CloseHandle(_hPauseEvent); _hPauseEvent = NULL;
    ::CloseHandle(_hProceedEvent); _hProceedEvent = NULL;
    ::CloseHandle(_hStopEvent); _hStopEvent = NULL;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CWin32StateMachineWorker::AddStateMachine(CStateMachineBase *pSM, BOOL bHandleOnFail)
{
    if (!pSM || m_bTerminate) return FALSE;

    BOOL ret = TRUE;

    if (::WaitForSingleObject(_hSyncSem, INFINITE) != WAIT_OBJECT_0)
    {
        if (bHandleOnFail)
            delete pSM;
        return FALSE;
    }

    if (!CStateMachineManager::AddStateMachine(pSM, bHandleOnFail))
    {
        ret = FALSE;
    }
    else
    {
        SMBDataStorage ss;
        ss._fpCallback = GetDoubleStorageInStateMachineMgr;
        ss._pContext = this;
        pSM->Set(strSMB_DOUBLE_STORAGE, (PVOID)(&ss), sizeof(SMBDataStorage), 1);
        ss._fpCallback = GetInt32StorageInStateMachineMgr;
        pSM->Set(strSMB_INT32_STORAGE, (PVOID)(&ss), sizeof(SMBDataStorage), 1);
    }

    ::ReleaseSemaphore(_hSyncSem, 1, NULL);

    return ret;
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CWin32StateMachineWorker::DelStateMachine(char *pSMID)
{
    if (!pSMID || strlen(pSMID) <= 0 || m_bTerminate)
        return FALSE;

    BOOL ret = TRUE;

    if (::WaitForSingleObject(_hSyncSem, INFINITE) != WAIT_OBJECT_0)
    {
        return FALSE;
    }

    if (!CStateMachineManager::DelStateMachine(pSMID))
    {
        ret = FALSE;
    }

    ::ReleaseSemaphore(_hSyncSem, 1, NULL);

    return ret;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CWin32StateMachineWorker::NewDoubleSharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems)
{
    BOOL ret;
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    ret = CStateMachineManager::NewDoubleSharedMem(pSharedMemName, pMuxName, nItems);
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);
    return ret;
}
BOOL CWin32StateMachineWorker::DelDoubleSharedMem(char *pSharedMemName)
{
    BOOL ret;
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    ret = CStateMachineManager::DelDoubleSharedMem(pSharedMemName);
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);
    return ret;
}
void CWin32StateMachineWorker::MergeDoubleSharedMem2Array()
{
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    CStateMachineManager::MergeDoubleSharedMem2Array();
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);

}
CDoubleSharedMem* CWin32StateMachineWorker::GetDoubleSharedMem(char *pSharedMemName)
{
    CDoubleSharedMem* ret;
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    ret = CStateMachineManager::GetDoubleSharedMem(pSharedMemName);
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);
    return ret;
}
BOOL CWin32StateMachineWorker::NewInt32SharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems)
{
    BOOL ret;
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    ret = CStateMachineManager::NewInt32SharedMem(pSharedMemName, pMuxName, nItems);
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);
    return ret;
}
BOOL CWin32StateMachineWorker::DelInt32SharedMem(char *pSharedMemName)
{
    BOOL ret;
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    ret = CStateMachineManager::DelInt32SharedMem(pSharedMemName);
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);
    return ret;

}
void CWin32StateMachineWorker::MergeInt32SharedMem2Array()
{
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    CStateMachineManager::MergeInt32SharedMem2Array();
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);

}
CInt32SharedMem* CWin32StateMachineWorker::GetInt32SharedMem(char *pSharedMemName)
{
    CInt32SharedMem* ret;
    ::WaitForSingleObject(_hSyncSharedMem, INFINITE);
    ret = CStateMachineManager::GetInt32SharedMem(pSharedMemName);
    ::ReleaseSemaphore(_hSyncSharedMem, 1, NULL);
    return ret;
}


///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
void CWin32StateMachineWorker::RunSpecifiedSM(CStateMachineBase *pSM, Job2Do *pCurrJob, INT32 &nextState, INT32 &retOfSM)
{
    INT32 errCode;
    BOOL bHandleMsg = FALSE;
    char *pMsg = NULL;

    bHandleMsg = FALSE;
    pMsg = NULL;
    retOfSM = pSM->RunState(pCurrJob->_pParam, errCode, &pMsg, bHandleMsg);
    // proc the message
    if (pMsg && _bIsLog && _fpLog)
        _fpLog(pMsg, 0);
    // handle the message
    if (bHandleMsg && pMsg)
        CStateMachineBase::FreeBaseDataTypeMem(pMsg);

    switch (retOfSM)
    {
    case RET_STATUS_NG:
        if (pCurrJob->_fpNotifyEnd)
            pCurrJob->_fpNotifyEnd(pCurrJob->_nSN, pCurrJob->_pNotifyEndContext, W32SMWORKER_NOTIFYCODE_SMEXECERR);
        nextState = W32SMWORKER_SM_GETJOB;
        break;
    case RET_STATUS_OK:
        if (pCurrJob->_fpNotifyEnd)
            pCurrJob->_fpNotifyEnd(pCurrJob->_nSN, pCurrJob->_pNotifyEndContext, 0);
        nextState = W32SMWORKER_SM_GETJOB;
        break;
    }

}
void CWin32StateMachineWorker::Execute(void)
{
    std::list<Job2Do*>::iterator it;
    Job2Do *pCurrJob = NULL;
    INT32 nRetCode;

    while (!m_bTerminate)
    {
        if (::WaitForSingleObject(_hStopEvent, 0) == WAIT_OBJECT_0)
        {
            if (_pCurrRunningSM)
            {
                _pCurrRunningSM->Stop();
                _nSMState = W32SMWORKER_SM_PROC_STOP;
            }
        }

        switch (_nSMState)
        {
        case W32SMWORKER_SM_WAIT:
            if (::WaitForSingleObject(_hEvent, 1000) == WAIT_OBJECT_0)
                ::ResetEvent(_hEvent);
            if (_JobList.size() > 0)
            {
                if (pCurrJob)
                    printf("[CWin32StateMachineWorker::Exec()] pCurrJob not null in state W32SMWORKER_SM_WAIT(%d)!!\n", W32SMWORKER_SM_WAIT);
                // job come in
                _nSMState = W32SMWORKER_SM_GETJOB;
                _pCurrRunningSM = NULL;
                pCurrJob = NULL;
            }
            break;

        case W32SMWORKER_SM_GETJOB:
            if (::WaitForSingleObject(_hSyncRunListSem, 0) == WAIT_OBJECT_0)
            {
                if (pCurrJob)
                    printf("[CWin32StateMachineWorker::Exec()] pCurrJob not null in state W32SMWORKER_SM_GETJOB(%d)!!\n", W32SMWORKER_SM_GETJOB);

                if (_JobList.size() > 0)
                {
                    it = _JobList.begin();
                    pCurrJob = (Job2Do *)(*it);
                    _JobList.erase(it);
                }
                else
                    _nSMState = W32SMWORKER_SM_WAIT;
                ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

                if (pCurrJob)
                {
                    _pCurrRunningSM = CStateMachineManager::GetInstance(pCurrJob->_strSM);
                    if (!_pCurrRunningSM)
                    {
                        if (pCurrJob->_fpNotifyBeg)
                            pCurrJob->_fpNotifyBeg(pCurrJob->_nSN, pCurrJob->_pNotifyBegContext, W32SMWORKER_NOTIFYCODE_SMNOTFOUNF);
                        // not found the SM back to wait
                        _nSMState = W32SMWORKER_SM_WAIT;
                        break;
                    }

                    if (pCurrJob->_fpNotifyBeg)
                        pCurrJob->_fpNotifyBeg(pCurrJob->_nSN, pCurrJob->_pNotifyBegContext, 0);

                    // job read to run
                    _nSMState = W32SMWORKER_SM_RUNNINGSM;
                    // reset all event
                    ::ResetEvent(_hPauseEvent);
                    ::ResetEvent(_hProceedEvent);
                }
            }
            break;

        case W32SMWORKER_SM_RUNNINGSM:
            // handle trigger event
            if (::WaitForSingleObject(_hPauseEvent, 0) == WAIT_OBJECT_0)
            {
                _pCurrRunningSM->Pause();
                _nSMState = W32SMWORKER_SM_PAUSING;
                break;
            }

            RunSpecifiedSM(_pCurrRunningSM, pCurrJob, _nSMState, nRetCode);
            if (nRetCode == RET_STATUS_NG || nRetCode == RET_STATUS_OK)
            {
                delete pCurrJob; pCurrJob = NULL;
                _pCurrRunningSM = NULL;
            }
            break;

#pragma region PAUSING_CONDITION
        case W32SMWORKER_SM_PAUSING:
            RunSpecifiedSM(_pCurrRunningSM, pCurrJob, _nSMState, nRetCode);
            if (nRetCode == RET_STATUS_NG || nRetCode == RET_STATUS_OK)
            {
                delete pCurrJob; pCurrJob = NULL;
                _pCurrRunningSM = NULL;
            }
            else if (nRetCode == RET_STATUS_PAUSED)
                _nSMState = W32SMWORKER_SM_WAIT_PROCEEDING;

            break;

        case W32SMWORKER_SM_WAIT_PROCEEDING:
            // check signal proceed
            if (::WaitForSingleObject(_hProceedEvent, 0) == WAIT_OBJECT_0)
            {
                _pCurrRunningSM->Proceed();
                _nSMState = W32SMWORKER_SM_PROC_PROCEEDING;
                break;
            }
            // running SM
            RunSpecifiedSM(_pCurrRunningSM, pCurrJob, _nSMState, nRetCode);
            if (nRetCode == RET_STATUS_NG || nRetCode == RET_STATUS_OK)
            {
                delete pCurrJob; pCurrJob = NULL;
                _pCurrRunningSM = NULL;
            }
            break;

        case W32SMWORKER_SM_PROC_PROCEEDING:
            RunSpecifiedSM(_pCurrRunningSM, pCurrJob, _nSMState, nRetCode);
            if (nRetCode == RET_STATUS_NG || nRetCode == RET_STATUS_OK)
            {
                delete pCurrJob; pCurrJob = NULL;
                _pCurrRunningSM = NULL;
            }
            else if (nRetCode == RET_STATUS_PAUSE_RELEASED)
                _nSMState = W32SMWORKER_SM_RUNNINGSM;
            break;
#pragma endregion PAUSING_CONDITION

#pragma region STOP_CONDITION
        case W32SMWORKER_SM_PROC_STOP:
            // if have SM running, wait to end
            RunSpecifiedSM(_pCurrRunningSM, pCurrJob, _nSMState, nRetCode);
            if (nRetCode == RET_STATUS_NG || nRetCode == RET_STATUS_OK)
            {
                delete pCurrJob; pCurrJob = NULL;
                _pCurrRunningSM = NULL;

                // change the state
                _nSMState = W32SMWORKER_SM_PROC_STOP_DONE;
            }
            break;

        case W32SMWORKER_SM_PROC_STOP_DONE:
            // clear job list
            ::WaitForSingleObject(_hSyncRunListSem, INFINITE);
            while (_JobList.size() > 0)
            {
                it = _JobList.begin();
                pCurrJob = (Job2Do *)(*it);
                _JobList.erase(it);
                if (pCurrJob->_fpNotifyBeg) pCurrJob->_fpNotifyBeg(pCurrJob->_nSN, pCurrJob->_pNotifyBegContext, W32SMWORKER_NOTIFYCODE_STOP);
                if (pCurrJob->_fpNotifyEnd) pCurrJob->_fpNotifyEnd(pCurrJob->_nSN, pCurrJob->_pNotifyEndContext, W32SMWORKER_NOTIFYCODE_STOP);
                delete pCurrJob; pCurrJob = NULL;
            }
            ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);
            // TODO: it can do something. STOP is done...
            // change state to wait
            _nSMState = W32SMWORKER_SM_WAIT;
            break;
#pragma endregion STOP_CONDITION

        default:
            _nSMState = W32SMWORKER_SM_WAIT;
            break;
        } // end-switch

        if (::WaitForSingleObject(_hStopEvent, 0) == WAIT_OBJECT_0)
        {
            if (_pCurrRunningSM)
            {
                _pCurrRunningSM->Stop();
                _nSMState = W32SMWORKER_SM_PROC_STOP;
            }
        }

    }
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
UINT32 CWin32StateMachineWorker::AddJob(char *pSMID, PVOID pRunParam, INT32 nParamUnit, INT32 nParamSize, 
                                        fpStateMachineHandMem fpParamHandler, 
                                        fpWin32SMWorkerExecJobNotify fpNotifyBeg, PVOID pContextBeg,
                                        fpWin32SMWorkerExecJobNotify fpNotifyEnd, PVOID pContextEnd,
                                        BOOL bHandleOnFail)
{
    if (m_bTerminate || !pSMID || strlen(pSMID) <= 0)
    {
        if (bHandleOnFail)
        {
            if (pRunParam && fpParamHandler)
                fpParamHandler(pRunParam);
        }
        if (fpNotifyBeg)
            fpNotifyBeg(0, pContextBeg, W32SMWORKER_NOTIFYCODE_PARAMERR);
        return 0;
    }

    UINT32 nCurrSN = 0;
    BOOL bTrg = FALSE;
    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);

    // alloc mem
    Job2Do *pJ = new Job2Do();
    if (pJ) pJ->_pParam = new TStateMachineParam();

    if (pJ && pJ->_pParam)
    {
        // config sn
        nCurrSN = _nJobSerialSN == 0xffffffff ? 1 : _nJobSerialSN++;
        pJ->_nSN = nCurrSN;
        // config parameter for a SM
        pJ->_pParam = new TStateMachineParam();
        pJ->_pParam->_pData = pRunParam;
        pJ->_pParam->_nUnit = nParamUnit;
        pJ->_pParam->_nSize = nParamSize;
        pJ->_pParam->_fpMemHandler = fpParamHandler;
        sprintf_s(pJ->_strSM, sizeof(pJ->_strSM) - 1, "%s", pSMID);
        pJ->_pNotifyBegContext = pContextBeg;
        pJ->_fpNotifyBeg = fpNotifyBeg;
        pJ->_pNotifyEndContext = pContextEnd;
        pJ->_fpNotifyEnd = fpNotifyEnd;
        // add to list
        _JobList.push_back(pJ);
        bTrg = TRUE;
    }
    else
    {
        if (pJ) delete pJ;
    }

    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

    if (!bTrg)
    {
        if (bHandleOnFail)
        {
            if (pRunParam && fpParamHandler)
                fpParamHandler(pRunParam);
        }
        if (fpNotifyBeg)
            fpNotifyBeg(0, pContextBeg, W32SMWORKER_NOTIFYCODE_NOMEM);
        return 0;
    }

    ::SetEvent(_hEvent);
    return nCurrSN;
}
BOOL CWin32StateMachineWorker::DelJob(UINT32 sn)
{
    if (m_bTerminate) return FALSE;

    Job2Do *pJob = NULL;
    std::list<Job2Do*>::iterator it;

    ::WaitForSingleObject(_hSyncRunListSem, INFINITE);

    for (it = _JobList.begin(); it != _JobList.end(); it++)
    {
        pJob = (Job2Do*)(*it);
        if (pJob->_nSN == sn)
        {
            _JobList.erase(it);
            break;
        }
    }

    ::ReleaseSemaphore(_hSyncRunListSem, 1, NULL);

    if (pJob)
    {
        if (pJob->_fpNotifyBeg)
            pJob->_fpNotifyBeg(pJob->_nSN, pJob->_pNotifyBegContext, W32SMWORKER_NOTIFYCODE_REMOVESN);
        delete pJob;
        return TRUE;
    }
    return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
void CWin32StateMachineWorker::Pause()
{
    if (m_bTerminate)
        return;
    if (_nSMState == W32SMWORKER_SM_PAUSING) // already doing pause
        return;
    if (_hPauseEvent)
        ::SetEvent(_hPauseEvent);
}
BOOL CWin32StateMachineWorker::Proceed()
{
    if (m_bTerminate)
        return FALSE;

    if (_nSMState != W32SMWORKER_SM_WAIT_PROCEEDING) // not in wait proceeding
        return FALSE;

    if (_hProceedEvent)
        ::SetEvent(_hProceedEvent);
    return TRUE;
}
void CWin32StateMachineWorker::Stop()
{
    if (m_bTerminate)
        return;
    if (_nSMState == W32SMWORKER_SM_PROC_STOP) // already in stop
        return;
    if (_hStopEvent)
        ::SetEvent(_hStopEvent);
}

