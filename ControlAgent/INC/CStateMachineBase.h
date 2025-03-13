#ifndef CStateMachineBase_h__
#define CStateMachineBase_h__

#include "windows.h"
#include <list>
#include "TypeOfDataSyncDecl.h"
#include "TStateMachineTimeoutObj.h"

typedef void (*fpStateMachineLog)(char *pStr, int level);
typedef void(*fpStateMachineHandMem)(PVOID pMem);
typedef CDoubleSharedMem* (*fpStateMachineGetDoubleSMem)(PVOID pContext, char *pSMemName);
typedef CInt32SharedMem* (*fpStateMachineGetInt32SMem)(PVOID pContext, char *pSMemName);
enum StateMachineBaseRet
{
    RET_STATUS_NG = -1,
    RET_STATUS_OK = 0,
    RET_STATUS_RUNNING,
    RET_STATUS_PAUSED,
    RET_STATUS_PAUSE_RELEASED,
};

// States reserved
#define STATEMACHINE_STATE_ERROR           -2 // MUST HAVE: error happened
#define STATEMACHINE_STATE_NA              -1
#define STATEMACHINE_STATE_PREPARING        0 // MUST HAVE: prepare to exec
#define STATEMACHINE_STATE_NORMAL_END        1 // MUST HAVE: normal done
#define STATEMACHINE_STATE_PAUSING          2 // MUST HAVE: pausing
#define STATEMACHINE_STATE_PAUSE_RELEASE    3 // ABANDONED: pause released
#define STATEMACHINE_STATE_STOP             4 // MUST HAVE: stop
#define STATEMACHINE_STATE_DUMMY_RUN        5 // Alternative

typedef struct tStateMachineParam
{
    INT32   _nUnit;
    INT32   _nSize;
    PVOID   _pData;
    fpStateMachineHandMem   _fpMemHandler;

    tStateMachineParam()
    {
        _nUnit = 0;
        _nSize = 0;
        _pData = NULL;
        _fpMemHandler = NULL;
    }
    ~tStateMachineParam()
    {
        if (_fpMemHandler && _pData) _fpMemHandler(_pData);
        _pData = NULL;
        _fpMemHandler = NULL;
    }
}TStateMachineParam;

#define nSMB_NUM_SETTINGS           4
#define strSMB_DOUBLE_STORAGE       "doubleStorage" // not suggest to config in runtime and control by one
#define strSMB_INT32_STORAGE        "int32Storage" // not suggest to config in runtime and control by one
#define strSMB_ID                   "id"
#define strSMB_LOG                  "log" // not suggest to config in runtime
#define nSMB_ID_MAX                 64

class CStateMachineBase
{
private:
    INT32 _nDummyRun;
    INT32 _nDummyRunCount;

protected:
    VOID   *_pOwner;
    HANDLE                  _hBinSem;

    fpStateMachineGetDoubleSMem _fpGetDoubleStorage;
    PVOID                       _pContextForDoubleStorage;

    fpStateMachineGetInt32SMem  _fpGetInt32Storage;
    PVOID                       _pContextForInt32Storage;

    char                _strID[nSMB_ID_MAX];
    fpStateMachineLog   _fpLog;

    char               *_arraySetList[nSMB_NUM_SETTINGS];
    char               *_arrayGetList[nSMB_NUM_SETTINGS];

    LARGE_INTEGER   _nFreq;

    INT32   _nPrevState;
    INT32   _nState;
    INT32   _nProceedingState;
    BOOL    _bPause;
    BOOL    _bProceed;
    BOOL    _bStop;

public:
    CStateMachineBase();
    CStateMachineBase(char *pGivenName);
    virtual ~CStateMachineBase();

    void SetOwner(VOID *owner) { _pOwner = owner; }
    INT32 SizeofID() { return (INT32) sizeof(_strID); }
    char* ID() { return _strID; }
    void ID(char *pId)
    {
        if (!pId || strlen(pId) <= 0) return;
        sprintf_s(_strID, sizeof(_strID), "%s", pId);
        return;
    }

    virtual BOOL Set(char *pID, PVOID pData, INT32 nDataUnit, INT32 nDataSize);
    virtual BOOL Get(char *pID, PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler);

    // pause in certain condition
    // keep data of current state
    virtual VOID Pause() { _bPause = TRUE; }
    // proceed from pause state
    virtual VOID Proceed() { _bProceed = TRUE; }
    // reset condition to the end, also can release the pause status
    // goto end state and UCT to begin
    virtual VOID Stop() { _bStop = TRUE; }
    // this function only be executed by single thread, regarding multi-threading
    // thinking and cannot be suspended by sync object otherwise polling design broken.
    virtual char* OutDescriptionOfRunningParameters() const { return NULL; }
    virtual INT32 RunState(TStateMachineParam *pInput, INT32 &errorCode, char **ppRetMsg, BOOL bHandleRetMsg);

private:
    void InitResources();
    BOOL SetDoubleStorage(PVOID pData, INT32 nDataUnit, INT32 nDataSize);
    //BOOL GetDoubleStorage(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler);

    BOOL SetInt32Storage(PVOID pData, INT32 nDataUnit, INT32 nDataSize);
    //BOOL GetInt32Storage(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler);

    BOOL SetId(PVOID pData, INT32 nDataUnit, INT32 nDataSize);
    BOOL GetId(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler);

    BOOL SetLog(PVOID pData, INT32 nDataUnit, INT32 nDataSize);
    BOOL GetLog(PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler);

protected:
    // All memory should be from this function including char[] to store string
    template<typename TP>
    static TP* AllocBaseDataTypeMem(UINT32 size)
    {
        UINT32 sz = size * sizeof(TP);
        return sz == 0 ? NULL : (TP*)malloc(sz);
    }
    // first change in state
    //BOOL Is1stChanged() { return _nPrevState != _nState; }

public:
    static void FreeBaseDataTypeMem(PVOID mem)
    {
        if(mem) free(mem);
    }
    static double DiffMs(LARGE_INTEGER beg, LARGE_INTEGER end, LARGE_INTEGER freq)
    {
        return ((double)(end.QuadPart - beg.QuadPart) / (double)freq.QuadPart) * 1000.;
    }
    static BOOL IsTimeout(TStateMachineTimeoutObj &obj)
    {
        return obj.TimeoutMs();
    }
};

typedef struct tSMBDataStorage
{
    PVOID   _fpCallback;
    PVOID   _pContext;
}SMBDataStorage;

#endif
