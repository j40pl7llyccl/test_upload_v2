#ifndef CWin32StateMachineWorker_h__
#define CWin32StateMachineWorker_h__

#include "CStateMachineManager.h"
#include "CThreadBase.h"
#include <list>

enum W32SMWORKER_SM
{
    W32SMWORKER_SM_NA = -1,
    W32SMWORKER_SM_WAIT = 0,
    W32SMWORKER_SM_GETJOB,
    W32SMWORKER_SM_RUNNINGSM,
    W32SMWORKER_SM_RUN_SPECIFIC_SM,
    //W32SMWORKER_SM_WAIT_RUN_POLLING,
    //W32SMWORKER_SM_JOBDONE,
    W32SMWORKER_SM_PAUSING,
    W32SMWORKER_SM_WAIT_PROCEEDING,
    W32SMWORKER_SM_PROC_PROCEEDING,
    W32SMWORKER_SM_PROC_STOP,
    W32SMWORKER_SM_PROC_STOP_DONE,
};

enum W32SMWORKER_NOTIFYCODE
{
    W32SMWORKER_NOTIFYCODE_PARAMERR = -1, // parameter error
    W32SMWORKER_NOTIFYCODE_NOMEM = -2, // no memory to alloc
    W32SMWORKER_NOTIFYCODE_REMOVESN = -3, // remove sn
    W32SMWORKER_NOTIFYCODE_SMNOTFOUNF = -4, // SM not found
    W32SMWORKER_NOTIFYCODE_SMEXECERR = -5, // SM exec error
    W32SMWORKER_NOTIFYCODE_STOP = -6, // cuase by stop
};

typedef void(*fpWin32SMWorkerExecJobNotify)(UINT32 sn, PVOID pContext, INT32 status);

struct Job2Do
{
    UINT32              _nSN;
    TStateMachineParam *_pParam;
    char                _strSM[nSMB_ID_MAX];
    fpWin32SMWorkerExecJobNotify    _fpNotifyBeg;
    PVOID                           _pNotifyBegContext;
    fpWin32SMWorkerExecJobNotify    _fpNotifyEnd;
    PVOID                           _pNotifyEndContext;

    Job2Do()
    {
        _nSN = 0; _pParam = NULL; _strSM[0] = 0;
        _fpNotifyBeg = NULL; _pNotifyBegContext = NULL;
        _fpNotifyEnd = NULL; _pNotifyEndContext = NULL;
    }
    ~Job2Do()
    {
        if(_pParam) delete _pParam; _strSM[0] = 0;
        _fpNotifyBeg = NULL;
        _pNotifyBegContext = NULL;
        _fpNotifyEnd = NULL;
        _pNotifyEndContext = NULL;
    }
};

class CWin32StateMachineWorker : public CStateMachineManager, public CThreadBase
{
protected:
    HANDLE  _hEvent;
    HANDLE  _hSyncSem;
    HANDLE  _hSyncSharedMem;

    HANDLE              _hSyncRunListSem;
    UINT32              _nJobSerialSN;
    std::list<Job2Do*>  _JobList;

    BOOL                _bIsLog;
    fpStateMachineLog   _fpLog;

    // run state machine data
    INT32               _nSMState;
    CStateMachineBase  *_pCurrRunningSM;
    HANDLE              _hPauseEvent;
    HANDLE              _hProceedEvent;
    HANDLE              _hStopEvent;

public:
    CWin32StateMachineWorker();
    ~CWin32StateMachineWorker();

    BOOL IsLog() { return _bIsLog; }
    void IsLog(BOOL bEnable) { _bIsLog = bEnable; }
    void SetLog(fpStateMachineLog fp)
    {
        _fpLog = fp;
        for (int i = 0; i < _StateMachines.size(); i++)
        {
            CStateMachineBase *pSM = _StateMachines[i];
            pSM->Set(strSMB_LOG, fp, sizeof(fpStateMachineLog), 1);
        }
    }

    // for state machine manager
    virtual BOOL AddStateMachine(CStateMachineBase *pSM, BOOL bHandleOnFail);
    virtual BOOL DelStateMachine(char *pSMID);

    // for shared memory manager
    virtual BOOL NewDoubleSharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems);
    virtual BOOL DelDoubleSharedMem(char *pSharedMemName);
    virtual void MergeDoubleSharedMem2Array();
    virtual CDoubleSharedMem* GetDoubleSharedMem(char *pSharedMemName);
    virtual BOOL NewInt32SharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems);
    virtual BOOL DelInt32SharedMem(char *pSharedMemName);
    virtual void MergeInt32SharedMem2Array();
    virtual CInt32SharedMem* GetInt32SharedMem(char *pSharedMemName);


    virtual UINT32 AddJob( char *pSMID, PVOID pRunParam, INT32 nParamUnit, INT32 nParamSize, fpStateMachineHandMem fpParamHandler, 
                           fpWin32SMWorkerExecJobNotify fpNotifyBeg, PVOID pContextBeg,
                           fpWin32SMWorkerExecJobNotify fpNotifyEnd, PVOID pContextEnd,
                           BOOL bHandleOnFail=TRUE);
    virtual BOOL DelJob(UINT32 sn);

    virtual void Pause();
    virtual BOOL Proceed();
    virtual void Stop();

public:
    virtual void Execute(void); // only for CThreadBase to call
private:
    void RunSpecifiedSM(CStateMachineBase *pSM, Job2Do *ppJob, INT32 &nextState, INT32 &retOfSM);
};


#endif
