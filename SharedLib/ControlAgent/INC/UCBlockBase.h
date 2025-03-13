#ifndef UCBlockBase_h__
#define UCBlockBase_h__

#include "UCObject.h"
#include "UCBlockDecl.h"
#include "UCBlockDataIO.h"
#include "UCRTTIClassInst.h"
#include "UCCommunicationManager.h"
#include "UCSharedMemFormating.h"
#include <stdlib.h>
#include <malloc.h>
#include <list>
#include <map>

//
// TODO:
//  - 1. add communication interface: will attach to win32 IPC, named pipe
//

class UCBlockBase;

// return:
//   - RET_STATUS_RUNNING: not want to change to next state
//   - RET_STATUS_NG: automatically change to ERROR state
//   - RET_STATUS_OK: automatically change to END state
typedef tYUX_I32 (*fpUCBlockStateCallback)(tYUX_PVOID pContext, tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState);
typedef void (*fpUCBlockStateChangedCallback)(UCBlockBase *pInstance, tYUX_I32 nState);
typedef void(*fpUCBlockInformOut)(UCBlockBase *pInstance, tYUX_PVOID pDat, tYUX_I32 nSizeofDat, tYUX_I32 nNumofDat);

typedef struct tagBlockAssistant
{
    UCBlockBase    *_pAssistant;
    tYUX_BOOL       _bRunningDone;
    tYUX_BOOL       _bDoneWithError;
    tYUX_PVOID      _pParams;
    tYUX_I32        _nSizeofParam;
    tYUX_I32        _nNumOfParams;
}TBlockAssistant;

#define nUCB_NUM_SETTINGS   6
#define strUCB_ID                               "id"
#define strUCB_LOG                              "log" // not suggest to config in runtime
#define strUCB_STATE_CHANGE_CALLBACK            "stateChangeCallback"
#define strUCB_ASSISTANT_STATE_CHANGE_CALLBACK  "assistantStateChangeCallback"
#define strUCB_INFORM_OUT                       "informOut"
#define strUCB_COMMUNICATION_MGR                "communicationInstanceMgr"
#define nUCB_ID_MAX         256 // Note: sprintf_s exceeding the max len of size will cause a exception
#define nUCB_DBG_STR_MAX    256

class UCBlockBase : public UCObject
{
private:
   int _nDummyRun;
   int _nDummyRunCount;

protected:
    UCRTTIClassInst                 _Owner;
    UCCommunicationManager         *_pCommunicationMgr;
    UCSharedMemFormating           *_pSharedMems;
    map<string, tYUX_PVOID>         _mapAllocFormattedSharedMem;
    UCDataSync<tYUX_I32>           *_pEnvSharedMemInt32;
    UCDataSync<tYUX_I64>           *_pEnvSharedMemInt64;
    UCDataSync<double>             *_pEnvSharedMemDouble;

    map<string, TUCBlockIChaining*>  _mapInputData;  // <id name:string, needed descript:TUCBlockIChaining>
    map<string, TUCBlockOChaining*>  _mapOutputData; // <id name:string, alloc res:TUCBlockOChaining>

    char            _strDbgMsg[nUCB_DBG_STR_MAX];
    char            _strID[nUCB_ID_MAX];
    fpUCBlockLog    _fpLog;

    char           *_arraySetList[nUCB_NUM_SETTINGS];
    char           *_arrayGetList[nUCB_NUM_SETTINGS];

    tYUX_I32        _nPrevState;
    tYUX_I32        _nState;
    tYUX_I32        _nProceedingState;
    tYUX_BOOL       _bPause;
    tYUX_BOOL       _bProceed;
    tYUX_BOOL       _bStop;

    fpUCBlockStateChangedCallback   _fpStateChangedCall;
    fpUCBlockInformOut              _fpInformOut;

    //
    // For normal child class to implement
    //
public:
    UCBlockBase();
    UCBlockBase(char *pGivenName);
    virtual ~UCBlockBase();

    template<class T>
    void SetOwner(T *pOwner) { _Owner.Set<T>(pOwner); }
    template<class T>
    T* GetOwner() { return _Owner.Convert<T>(); }

    tYUX_I32 SizeofID() { return (tYUX_I32) sizeof(_strID); }
    char* ID() { return _strID; }
    void ID(char *pId);
    fpUCBlockLog LogMethod() { return _fpLog; }

    virtual tYUX_BOOL Set(char *pID, tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    virtual tYUX_BOOL Get(char *pID, tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler);

    // if need memory shared outside world, using this object
    virtual tYUX_BOOL SetFormatableSharedMem(UCSharedMemFormating *pInst);
    virtual tYUX_BOOL SetEnvironmentSharedMems(UCObject *pEnvI32, UCObject *pEnvI64, UCObject *pEnvDouble);

    // pause in certain condition
    // keep data of current state
    virtual void Pause() { _bPause = true; }
    // proceed from pause state
    virtual void Proceed() { AssistantsFirstProceed(); _bProceed = true; }
    // reset condition to the end, also can release the pause status
    // goto end state and UCT to begin
    virtual void Stop() { _bStop = true; }
    // this function only be executed by single thread, regarding multi-threading
    // thinking and cannot be suspended by sync object otherwise polling design broken.
    virtual char* OutDescriptionOfMutableParameters() { return NULL; }
    // Remark:
    // - ppRetMsg: must be alloc from AllocBaseDataTypeMem()
    // - currently used PatternRunner_1()
    virtual tYUX_I32 Run(TUCBlockMutableParam *pInput, tYUX_I32 &errorCode, char **ppRetMsg, tYUX_BOOL &bHandleRetMsg);

//
// Pattern 1
//
// Config
//  - _nProceedingBackState: switch to which state
//  - FirstStateInfo()
//  - DesignedStateInfo()
protected:
    tYUX_I32                _nProceedingBackState;
    tYUX_PVOID              _p1stStateHandlerContext; // child instance of this class
    fpUCBlockStateCallback  _fp1stStateHandler;
    tYUX_PVOID              _pDesignedStateHandlerContext; // child instance of this class
    fpUCBlockStateCallback  _fpDesignedStateHandler;
    tYUX_I32 PatternRunner_1(tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_I32 &errorCode, char **ppRetMsg, tYUX_BOOL &bHandleRetMsg);
public:
    void FirstStateInfo(tYUX_PVOID pContext, fpUCBlockStateCallback fpHandler);
    void DesignedStateInfo(tYUX_PVOID pContext, fpUCBlockStateCallback fpHandler);
    // nNextState: must fill next state to make it work
    virtual tYUX_I32 Pattern1HandleFirstState(tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState);
    virtual tYUX_I32 Pattern1HandleDesignedState(tYUX_PVOID pParams, tYUX_I32 nSizeofParam, tYUX_I32 nNumOfParams, tYUX_BOOL bFistEnter, tYUX_I32 nCurrState, tYUX_I32 nCurrPrevState, std::list<tYUX_I32> *pStateHistory, tYUX_I32 &nNextState);

//
// Utility methods
//
public:
    //
    // get run state
    //
    tYUX_I32 State() { return _nState; }
    //
    // Call class to run
    //
    static tYUX_I32 RunBlock(UCBlockBase *pBlock, TUCBlockMutableParam *pBlockParamInfo);
    static tYUX_I32 RunBlock(UCBlockBase *pBlock, tYUX_PVOID pParameters, tYUX_I32 nSizeofParams, tYUX_I32 nNumOfParams, fpUCBlockHandleMem fpHandleParameters = (fpUCBlockHandleMem)0);

    //
    // Check running?
    //
    tYUX_BOOL IsRunning()
    {
        return ( ( _nPrevState == UCBLOCK_STATE_FINISH || _nPrevState == UCBLOCK_STATE_NA || _nPrevState == UCBLOCK_STATE_ERROR ) &&
                 ( _nState == UCBLOCK_STATE_PREPARING ) ) ? false : true;
    }
    tYUX_BOOL CheckInitState()
    {
        return ( _nPrevState == UCBLOCK_STATE_NA && _nState == UCBLOCK_STATE_PREPARING );
    }
    tYUX_BOOL CheckDoneStateOK()
    {
        return (_nPrevState == UCBLOCK_STATE_FINISH && _nState == UCBLOCK_STATE_PREPARING);
    }
    tYUX_BOOL CheckDoneStateError()
    {
        return (_nPrevState == UCBLOCK_STATE_ERROR && _nState == UCBLOCK_STATE_PREPARING);
    }
    tYUX_BOOL IsRunningDoneAndOK(tYUX_BOOL &isRunning)
    {
        isRunning = IsRunning();
        if(isRunning) return false;
        return CheckDoneStateOK();
    }
    tYUX_BOOL IsRunningDoneWithError(tYUX_BOOL &isRunning)
    {
        isRunning = IsRunning();
        if(isRunning) return false;
        return CheckDoneStateError();
    }
  
    //
    // Formatting shared memory access
    //
    size_t GetNumOfFormattingSharedMemory();
    TShMemAccItem* GetFormattingSharedMemoryFromIndex(size_t index_0);
    TShMemAccItem* GetFormattingSharedMemoryFromHandle(tYUX_PVOID h);
    TShMemAccItem* GetFormattingSharedMemoryFromIdName(char *pIdName);

protected:
    TShMemAccItem* CreateSharedMemoryStorageI32(char *pIdName, int nCount);
    TShMemAccItem* CreateSharedMemoryStorageDouble(char *pIdName, int nCount);
    TShMemAccItem* CreateSharedMemoryStorageU8(char *pIdName, tYUX_I64 nCount);

    //
    // Data operating
    //
public:
    map<string, TUCBlockIChaining *>* InputData();
    map<string, TUCBlockOChaining *>* OutputData();
    void ClearInputData();
    void ClearInput();
    TUCBlockIChaining* GetInputDat(const char *pName);
    TUCBlockIChaining* NewInputDat(const char *pName);
    void DelInputDat(const char *pName);
    void ClearOutput();
    TUCBlockOChaining* GetOutputDat(const char *pName);
    TUCBlockOChaining* NewOutputDat(const char *pName);
    void DelOutputDat(const char *pName);

private:
    void InitResources();

    tYUX_BOOL SetId(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    tYUX_BOOL GetId(tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler);

    tYUX_BOOL SetLog(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    tYUX_BOOL GetLog(tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler);

    tYUX_BOOL SetStateChangeCallback(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    tYUX_BOOL SetAssistantStateChangeCallback(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    tYUX_BOOL SetInformOut(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    tYUX_BOOL SetCommunicationInstanceMgr(tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);

protected:
    // All memory should be from this function including char[] to store string
    template<typename TP>
    static TP* AllocBaseDataTypeMem(tYUX_U32 size)
    {
        tYUX_U32 sz = size * sizeof(TP);
        return sz == 0 ? NULL : (TP*)malloc(sz);
    }

protected:
    //
    // History of block state
    //
    // variables
    tYUX_I32            _nMaxStoredBlockState;
    std::list<tYUX_I32> _BlockStateStorage;
    // methods
    void ClearBlockKeeper();
    void AddBlockState(tYUX_I32 state);
    bool FindBlockPreviousState(tYUX_I32 &ret, tYUX_I32 count);
    bool FindBlockPreviousState(tYUX_I32 firstHitState, tYUX_I32 &ret, tYUX_I32 count);

    //
    // Assistant blocks
    //
protected:
    TBlockAssistant    *_pAssistants;
    tYUX_I32            _nAssistants;
    tYUX_I32            _bAssistantsRunningPausing;
    void AllocAssistants(tYUX_I32 count); // alloc array to store assistant blocks
    void RestAssistantParams(tYUX_I32 index_0); // reset assistant block mutable parameters
    void RestAssistantsFlags();
    TBlockAssistant* Assistant(tYUX_I32 index_0); // get assistant block from index
    tYUX_BOOL AssistantsSet(char *pID, tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    template<typename T>
    T* AssistantParameters(tYUX_I32 index_0, tYUX_I32 &count) // get assistant block parameters and convert to T*
    {
        count = 0;
        if (!_pAssistants || _nAssistants <= 0)
            return NULL;

        TBlockAssistant *p = index_0 < 0 || index_0 >= _nAssistants ? NULL : &_pAssistants[index_0];
        if (p->_nSizeofParam != sizeof(T))
            return NULL;

        count = p->_nNumOfParams;
        return (T*)p->_pParams;
    }
    //template<class T>
    //T* CreateAssistant()
    //{
    //    T* pNew = new T();
    //    pNew->SetOwner(this);

    //    return pNew;
    //}
    // return:
    //  - RET_STATUS_OK: Done and success
    //  - RET_STATUS_NG: Done with error
    tYUX_I32 WorkWithAssistant(tYUX_I32 assistantIndex, tYUX_I32 nBlockStateWhenFinish, tYUX_BOOL bChangeBlock2ErrWhenAssistantFail = (tYUX_BOOL)true);
    // Predefined state: pause
    //  - Assistant must return RET_STATTE_PAUSED to make it done
    void AssistantsFirstGotoPauseState();
    tYUX_BOOL AssistantsRunToPauseState();
    tYUX_BOOL AssistantsHandlingPauseState(tYUX_BOOL &bErr); // auto change to error state when any assistant reported fail
    // Predefined action in pause state: proceed
    //  - Assistant must report RET_STATUS_PAUSE_RELEASED to make it done
    void AssistantsFirstProceed();
    tYUX_BOOL AssistantsGoProceeding();
    tYUX_BOOL AssistantsHandlingProceed(tYUX_BOOL &bErr);
    // Predefined state: stop
    //  - Assistant must report RET_STATUS_OK to make it done
    void AssistantsFirstGotoStopState();
    tYUX_BOOL AssistantsRunToStopState();
    tYUX_BOOL AssistantsHandlingStopState(tYUX_BOOL &bErr);

public:
    static void FreeBaseDataTypeMem(void *mem)
    {
        if(mem) free(mem);
    }
};

#endif
