#ifndef CStateMachineManager_h__
#define CStateMachineManager_h__

#include "CStateMachineBase.h"
#include <vector>

class CStateMachineManager
{
protected:
    std::vector<CStateMachineBase *>    _StateMachines;

    std::vector<CDoubleSharedMem*>      _DoubleSharedMems;
    std::vector<CInt32SharedMem*>       _Int32SharedMems;

    CDoubleSharedMem                  **_ppDoubleSharedMemArray;
    INT32                               _nDoubleSharedMemArray;

    CInt32SharedMem                   **_ppInt32SharedMemArray;
    INT32                               _nInt32SharedMemArray;

public:
    CStateMachineManager();
    virtual ~CStateMachineManager();

    BOOL SetStateMachine(char *pSMID, char *pParamId, PVOID pData, INT32 nDataUnit, INT32 nDataSize);
    BOOL GetStateMachine(char *pSMID, char *pParamId, PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, fpStateMachineHandMem &fpRetMemHandler);
    virtual BOOL AddStateMachine(CStateMachineBase *pSM, BOOL bHandleOnFail);
    virtual BOOL DelStateMachine(char *pSMID);
    CStateMachineBase* GetInstance(char *pSMID);

    virtual BOOL NewDoubleSharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems);
    virtual BOOL DelDoubleSharedMem(char *pSharedMemName);
    virtual void MergeDoubleSharedMem2Array();
    virtual CDoubleSharedMem* GetDoubleSharedMem(char *pSharedMemName);
    CDoubleSharedMem** GetDoubleSharedMems(INT32 &nSize);

    virtual BOOL NewInt32SharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems);
    virtual BOOL DelInt32SharedMem(char *pSharedMemName);
    virtual void MergeInt32SharedMem2Array();
    virtual CInt32SharedMem* GetInt32SharedMem(char *pSharedMemName);
    CInt32SharedMem** GetInt32SharedMems(INT32 &nSize);
};

CDoubleSharedMem* GetDoubleStorageInStateMachineMgr(PVOID pContext, char *pSharedMemName);
CInt32SharedMem* GetInt32StorageInStateMachineMgr(PVOID pContext, char *pSharedMemName);

#endif
