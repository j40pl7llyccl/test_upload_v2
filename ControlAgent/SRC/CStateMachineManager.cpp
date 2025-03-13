#include "stdafx.h"
#include "CStateMachineManager.h"

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CStateMachineManager::CStateMachineManager()
{
    _ppDoubleSharedMemArray = NULL;
    _nDoubleSharedMemArray = 0;

    _ppInt32SharedMemArray = NULL;
    _nInt32SharedMemArray = 0;

}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CStateMachineManager::~CStateMachineManager()
{
    std::vector<CStateMachineBase*>::iterator ite;
    while (_StateMachines.size() > 0)
    {
        CStateMachineBase *pBase;

        ite = _StateMachines.begin();
        pBase = (CStateMachineBase*)(*ite);
        if (pBase) delete pBase;

        _StateMachines.erase(ite);
    }

    std::vector<CDoubleSharedMem*>::iterator itd;
    while (_DoubleSharedMems.size() > 0)
    {
        CDoubleSharedMem *pSD;

        itd = _DoubleSharedMems.begin();
        pSD = (CDoubleSharedMem*)(*itd);
        if (pSD) delete pSD;

        _DoubleSharedMems.erase(itd);
    }

    std::vector<CInt32SharedMem*>::iterator iti;
    while (_Int32SharedMems.size() > 0)
    {
        CInt32SharedMem *pSI;

        iti = _Int32SharedMems.begin();
        pSI = (CInt32SharedMem*)(*iti);
        if (pSI) delete pSI;

        _Int32SharedMems.erase(iti);
    }

    if (_ppDoubleSharedMemArray) delete[] _ppDoubleSharedMemArray;
    _ppDoubleSharedMemArray = NULL;
    _nDoubleSharedMemArray = 0;

    if (_ppInt32SharedMemArray) delete[] _ppInt32SharedMemArray;
    _ppInt32SharedMemArray = NULL;
    _nInt32SharedMemArray = 0;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineManager::SetStateMachine(char *pSMID, char *pParamId, PVOID pData, INT32 nDataUnit, INT32 nDataSize)
{
    CStateMachineBase *pCurr;
    for (int i = 0; i < _StateMachines.size(); i++)
    {
        pCurr = _StateMachines[i];
        if (!pCurr->ID())
            continue;
        if (!strcmp(pCurr->ID(), pSMID))
        {
            return pCurr->Set(pParamId, pData, nDataUnit, nDataSize);
        }
    }

    return FALSE;
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineManager::GetStateMachine(char *pSMID, char *pParamId, 
                                           PVOID &ppRetData, INT32 &nRetDataUnit, INT32 &nRetDataSize, 
                                           fpStateMachineHandMem &fpRetMemHandler)
{
    CStateMachineBase *pCurr;
    for (int i = 0; i < _StateMachines.size(); i++)
    {
        pCurr = _StateMachines[i];
        if (!pCurr->ID())
            continue;
        if (!strcmp(pCurr->ID(), pSMID))
        {
            return pCurr->Get(pParamId, ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
        }
    }

    return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineManager::AddStateMachine(CStateMachineBase *pSM, BOOL bHandleOnFail)
{
    CStateMachineBase *pCurr;
    BOOL bSame = FALSE;
    if (!pSM )
        return FALSE;
    if (!pSM->ID() || strlen(pSM->ID()) <= 0)
    {
        if (bHandleOnFail) delete pSM;
        return FALSE;
    }

    for (int i = 0; i < _StateMachines.size(); i++)
    {
        pCurr = _StateMachines[i];
        if (!pCurr->ID())
            continue;
        if (!strcmp(pCurr->ID(), pSM->ID()))
        {
            bSame = TRUE; break;
        }
    }

    if (bSame)
    {
        if (bHandleOnFail) delete pSM;
        return FALSE;
    }

    pSM->SetOwner(this);
    _StateMachines.push_back(pSM);
    return TRUE;
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineManager::DelStateMachine(char *pSMID)
{
    if (!pSMID || strlen(pSMID) <= 0)
        return FALSE;

    std::vector<CStateMachineBase*>::iterator it;
    for (it = _StateMachines.begin(); it != _StateMachines.end(); it++)
    {
        CStateMachineBase *pCurr = (CStateMachineBase*)(*it);
        if (!strcmp(pCurr->ID(), pSMID))
        {
            delete pCurr;
            _StateMachines.erase(it);
            return TRUE;
        }
    }

    return FALSE;
}
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CStateMachineBase* CStateMachineManager::GetInstance(char *pSMID)
{
    if (!pSMID || strlen(pSMID) <= 0)
        return NULL;

    CStateMachineBase *pCurr;
    for (int i = 0; i < _StateMachines.size(); i++)
    {
        pCurr = _StateMachines[i];
        if (!pCurr->ID())
            continue;
        if (!strcmp(pCurr->ID(), pSMID))
        {
            return pCurr;
        }
    }

    return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineManager::NewDoubleSharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems)
{
    if (!pSharedMemName || strlen(pSharedMemName) <= 0 || !pMuxName || strlen(pMuxName) <= 0 || nItems <= 0)
        return FALSE;

    // check same one
    for (int i = 0; i < _DoubleSharedMems.size(); i++)
    {
        CDoubleSharedMem *pD = _DoubleSharedMems[i];
        if (pD && (strstr(pD->SharedMemName(), pSharedMemName) || !strcmp(pD->MuxName(), pMuxName)))
        {
            return FALSE;
        }
    }

    // new
    _DoubleSharedMems.push_back(new CDoubleSharedMem(pSharedMemName, pMuxName, nItems));
    return TRUE;
}
BOOL CStateMachineManager::DelDoubleSharedMem(char *pSharedMemName)
{
    if (!pSharedMemName || strlen(pSharedMemName) <= 0)
        return FALSE;

    // check same one
    std::vector<CDoubleSharedMem*>::iterator it;
    for (it = _DoubleSharedMems.begin(); it != _DoubleSharedMems.end(); it++)
    {
        CDoubleSharedMem *pD = (CDoubleSharedMem*)(*it);
        if (pD && strstr(pD->SharedMemName(), pSharedMemName))
        {
            _DoubleSharedMems.erase(it);
            delete pD;
            return TRUE;
        }
    }

    return FALSE;
}
void CStateMachineManager::MergeDoubleSharedMem2Array()
{
    if (_ppDoubleSharedMemArray) delete[] _ppDoubleSharedMemArray;
    _ppDoubleSharedMemArray = NULL;
    _nDoubleSharedMemArray = 0;
    if (_DoubleSharedMems.size() <= 0)
        return;

    _ppDoubleSharedMemArray = new CDoubleSharedMem*[_DoubleSharedMems.size()];
    for (int i = 0; i < _DoubleSharedMems.size(); i++)
    {
        _ppDoubleSharedMemArray[i] = _DoubleSharedMems[i];
    }
    _nDoubleSharedMemArray = (INT32) _DoubleSharedMems.size();
    return;
}
CDoubleSharedMem* CStateMachineManager::GetDoubleSharedMem(char *pSharedMemName)
{
    if (!pSharedMemName || strlen(pSharedMemName) <= 0)
        return NULL;

    for (int i = 0; i < _DoubleSharedMems.size(); i++)
    {
        CDoubleSharedMem *pD = _DoubleSharedMems[i];
        if (pD && strstr(pD->SharedMemName(), pSharedMemName))
        {
            return pD;
        }
    }
    return NULL;
}
CDoubleSharedMem** CStateMachineManager::GetDoubleSharedMems(INT32 &nSize)
{
    nSize = _nDoubleSharedMemArray;
    return _ppDoubleSharedMemArray;
}


///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL CStateMachineManager::NewInt32SharedMem(char *pSharedMemName, char *pMuxName, INT32 nItems)
{
    if (!pSharedMemName || strlen(pSharedMemName) <= 0 || !pMuxName || strlen(pMuxName) <= 0 || nItems <= 0)
        return FALSE;

    // check same one
    for (int i = 0; i < _Int32SharedMems.size(); i++)
    {
        CInt32SharedMem *pD = _Int32SharedMems[i];
        if (pD && (strstr(pD->SharedMemName(), pSharedMemName) || !strcmp(pD->MuxName(), pMuxName)))
        {
            return FALSE;
        }
    }

    // new
    _Int32SharedMems.push_back(new CInt32SharedMem(pSharedMemName, pMuxName, nItems));
    return TRUE;
}
BOOL CStateMachineManager::DelInt32SharedMem(char *pSharedMemName)
{
    if (!pSharedMemName || strlen(pSharedMemName) <= 0)
        return FALSE;

    // check same one
    std::vector<CInt32SharedMem*>::iterator it;
    for (it = _Int32SharedMems.begin(); it != _Int32SharedMems.end(); it++)
    {
        CInt32SharedMem *pD = (CInt32SharedMem*)(*it);
        if (pD && strstr(pD->SharedMemName(), pSharedMemName))
        {
            _Int32SharedMems.erase(it);
            delete pD;
            return TRUE;
        }
    }

    return FALSE;
}
void CStateMachineManager::MergeInt32SharedMem2Array()
{
    if (_ppInt32SharedMemArray) delete[] _ppInt32SharedMemArray;
    _ppInt32SharedMemArray = NULL;
    _nInt32SharedMemArray = 0;
    if (_Int32SharedMems.size() <= 0)
        return;

    _ppInt32SharedMemArray = new CInt32SharedMem*[_Int32SharedMems.size()];
    for (int i = 0; i < _Int32SharedMems.size(); i++)
    {
        _ppInt32SharedMemArray[i] = _Int32SharedMems[i];
    }
    _nInt32SharedMemArray = (INT32)_Int32SharedMems.size();
    return;
}
CInt32SharedMem* CStateMachineManager::GetInt32SharedMem(char *pSharedMemName)
{
    if (!pSharedMemName || strlen(pSharedMemName) <= 0)
        return NULL;

    for (int i = 0; i < _Int32SharedMems.size(); i++)
    {
        CInt32SharedMem *pD = _Int32SharedMems[i];
        if (pD && strstr(pD->SharedMemName(), pSharedMemName))
        {
            return pD;
        }
    }

    return NULL;
}
CInt32SharedMem** CStateMachineManager::GetInt32SharedMems(INT32 &nSize)
{
    nSize = _nInt32SharedMemArray;
    return _ppInt32SharedMemArray;
}


///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
CDoubleSharedMem* GetDoubleStorageInStateMachineMgr(PVOID pContext, char *pSharedMemName)
{
    CStateMachineManager *pMgr = static_cast<CStateMachineManager*>(pContext);
    if (!pMgr)
        return NULL;

    return pMgr->GetDoubleSharedMem(pSharedMemName);
}
CInt32SharedMem* GetInt32StorageInStateMachineMgr(PVOID pContext, char *pSharedMemName)
{
    CStateMachineManager *pMgr = static_cast<CStateMachineManager*>(pContext);
    if (!pMgr)
        return NULL;

    return pMgr->GetInt32SharedMem(pSharedMemName);
}

