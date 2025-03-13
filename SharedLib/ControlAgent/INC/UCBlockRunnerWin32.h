#ifndef UCBlockRunnerWin32_h__
#define UCBlockRunnerWin32_h__

#include "UCBlockManager.h"
#include "UCThreadWin32.h"

typedef void(*fpBlockRunWorkDoneCallback)(tYUX_U32 sn, tYUX_PVOID pContext, tYUX_I32 status);
enum BLOCKRUNNING_STATE
{
    BLOCKRUNNING_STATE_NA = -1,
    BLOCKRUNNING_STATE_WAIT = 0,
    BLOCKRUNNING_STATE_GETWORK,
    BLOCKRUNNING_STATE_RUNNING,
    BLOCKRUNNING_STATE_PAUSING,
    BLOCKRUNNING_STATE_WAIT_PROCEEDING,
    BLOCKRUNNING_STATE_PROC_PROCEEDING,
    BLOCKRUNNING_STATE_PROC_STOP,
    BLOCKRUNNING_STATE_PROC_STOP_DONE,
    BLOCKRUNNING_STATE_RECYCLE_WORK,
    BLOCKRUNNING_STATE_RUNBLOCK_ERROR,
};

enum BLOCKWORK_RETURNCODE
{
    BLOCKWORK_RETURNCODE_NO_ERROR = 0,       // no error
    BLOCKWORK_RETURNCODE_PARAMERR = -1,      // parameter error
    BLOCKWORK_RETURNCODE_NOMEM = -2,         // no memory to alloc
    BLOCKWORK_RETURNCODE_REMOVESN = -3,      // remove sn
    BLOCKWORK_RETURNCODE_BLOCKNOTFOUNF = -4, // Block not found
    BLOCKWORK_RETURNCODE_BLOCKEXECERR = -5,  // Block exec error
    BLOCKWORK_RETURNCODE_STOP = -6,          // cuase by stop
    BLOCKWORK_RETURNCODE_STATEERR = -7,      // cause by one error
};


struct WorkOfBlock;

//
// Description
//  - Run block on-by-one
//  - sequence
//  - Predefined parameters
//  - DataIO is not available
//
class UCBlockRunnerWin32 : public UCThreadWin32
{
protected:
    char                _strGivenName[nUCB_ID_MAX];
    UCBlockManager     *_pBlockManager;

    HANDLE              _hEvent;           // notify thread to work
    HANDLE              _hSyncSem;         // sync methods of this class
    HANDLE              _hSyncRunListSem;  // sync running work list
    tYUX_U32            _nWorkSN;          // ID SN of work
    list<WorkOfBlock*>  _WorkList;         // list to store the work

    tYUX_BOOL           _bIsLog;
    fpUCBlockLog        _fpLog;

    // run state machine data
    tYUX_I32            _nAWorkStateOfBlock;
    UCBlockBase        *_pCurrRunningBlock;
    HANDLE              _hPauseEvent;
    HANDLE              _hProceedEvent;
    HANDLE              _hStopEvent;

public:
    UCBlockRunnerWin32(UCCommunicationManager *pCommMgr, UCSharedMemFormating *pShMemFormating, 
                       UCObject *pEnvShMem32, UCObject *pEnvShMem64, UCObject *pEnvShMemDf, int threadPriority);
    virtual ~UCBlockRunnerWin32();

    tYUX_BOOL LogCtrlOfTheClass() { return _bIsLog; }
    void LogCtrlOfTheClass(tYUX_BOOL enable) { _bIsLog = enable ? 1 : 0; }

    void SetLogCallback(fpUCBlockLog fp);

    tYUX_BOOL AddBlock(UCBlockBase *pBlock, tYUX_BOOL bHandledByThisClass = (tYUX_BOOL)true);
    tYUX_BOOL RmvBlock(char *pBlockId);
    tYUX_BOOL CallBlockSetMethod(char *pBlockId, char *pNameForSet, tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    tYUX_BOOL CallBlockGetMethod(char *pBlockId, char *pNameForGet, tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler);

    virtual tYUX_U32 AddWork(char *pBlockId, tYUX_PVOID pRunParam, tYUX_I32 nParamUnit, tYUX_I32 nParamSize, fpUCBlockHandleMem fpParamHandler,
        fpBlockRunWorkDoneCallback fpNotifyBeg, tYUX_PVOID pContextBeg,
        fpBlockRunWorkDoneCallback fpNotifyEnd, tYUX_PVOID pContextEnd,
        tYUX_BOOL bHandleOnFail = (tYUX_BOOL)true);
    virtual tYUX_BOOL RmvWork(tYUX_U32 sn);

    virtual void Pause();
    virtual tYUX_BOOL Proceed();
    virtual void Stop();

public:
    virtual void Execute(void); // call by this class and not avialable for any one to access
protected:
    void RunWorkOfBlock(UCBlockBase *pBlock, WorkOfBlock *pWork, tYUX_I32 &nextState, tYUX_I32 &retOfBlock);
    void ClearWorkList(tYUX_I32 returnCode);
};

struct WorkOfBlock
{
    tYUX_U32                    _nSN;
    TUCBlockMutableParam       *_pParam;
    char                        _strBlockId[nUCB_ID_MAX];
    fpBlockRunWorkDoneCallback  _fpNotifyBeg;
    tYUX_PVOID                  _pNotifyBegContext;
    fpBlockRunWorkDoneCallback  _fpNotifyEnd;
    tYUX_PVOID                  _pNotifyEndContext;

    WorkOfBlock()
    {
        _nSN = 0; _pParam = NULL; _strBlockId[0] = 0;
        _fpNotifyBeg = NULL; _pNotifyBegContext = NULL;
        _fpNotifyEnd = NULL; _pNotifyEndContext = NULL;
    }
    ~WorkOfBlock()
    {
        if (_pParam) delete _pParam; _strBlockId[0] = 0;
        _fpNotifyBeg = NULL;
        _pNotifyBegContext = NULL;
        _fpNotifyEnd = NULL;
        _pNotifyEndContext = NULL;
    }
};


#endif
