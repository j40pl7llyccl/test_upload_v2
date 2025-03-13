#include "stdafx.h"
#include "UCBlockManager.h"

static void ClearMap(map<string, UCBlockBase*> *p, map<tYUX_PVOID, tYUX_BOOL> *p2);

UCBlockManager::UCBlockManager()
{
    _strClassRttiName = typeid(*this).name();
    _pCommunicationMgr = NULL;
    _pFormatSharedMems = NULL;
    _pEnvSharedMemInt32 = NULL;
    _pEnvSharedMemInt64 = NULL;
    _pEnvSharedMemDouble = NULL;
}
UCBlockManager::UCBlockManager(UCCommunicationManager *pCommMgr, UCSharedMemFormating *pShMemFormating,
    UCObject *pEnvShMem32, UCObject *pEnvShMem64, UCObject *pEnvShMemDf)
{
    _strClassRttiName = typeid(*this).name();
    _pCommunicationMgr = pCommMgr;
    _pFormatSharedMems = pShMemFormating;
    _pEnvSharedMemInt32 = pEnvShMem32;
    _pEnvSharedMemInt64 = pEnvShMem64;
    _pEnvSharedMemDouble = pEnvShMemDf;
}
UCBlockManager::~UCBlockManager()
{
    ClearMap(&_Blocks, &_BlocksHandling);
}

// set reference objects
void UCBlockManager::SetReferenceObjects(UCCommunicationManager *pCommMgr, UCSharedMemFormating *pShMemFormating, UCObject *pEnvShMem32, UCObject *pEnvShMem64, UCObject *pEnvShMemDf)
{
    _pCommunicationMgr = pCommMgr;
    _pFormatSharedMems = pShMemFormating;
    _pEnvSharedMemInt32 = pEnvShMem32;
    _pEnvSharedMemInt64 = pEnvShMem64;
    _pEnvSharedMemDouble = pEnvShMemDf;

    map<string, UCBlockBase*>::iterator it;
    for (it = _Blocks.begin(); it != _Blocks.end(); it++)
    {
        it->second->SetFormatableSharedMem(pShMemFormating);
        it->second->SetEnvironmentSharedMems(pEnvShMem32, pEnvShMem64, pEnvShMemDf);
        it->second->Set(strUCB_COMMUNICATION_MGR, (tYUX_PVOID)pCommMgr, sizeof(tYUX_PVOID), 1);
    }
}

// block management
tYUX_BOOL UCBlockManager::AddBlock(UCBlockBase *pBlock, tYUX_BOOL bHandledByThisClass)
{
    if (!pBlock || strlen(pBlock->ID()) <= 0)
    {
        if (bHandledByThisClass && pBlock)
            delete pBlock;

        return (tYUX_BOOL)false;
    }

    map<string, UCBlockBase*>::iterator it = _Blocks.find(pBlock->ID());
    if (it != _Blocks.end())
    {
        if (bHandledByThisClass)
            delete pBlock;

        return (tYUX_BOOL)false;
    }

    _Blocks.insert(pair<string, UCBlockBase*>(pBlock->ID(), pBlock));
    _BlocksHandling.insert(pair<tYUX_PVOID, tYUX_BOOL>((tYUX_PVOID)pBlock, bHandledByThisClass));
    return (tYUX_BOOL)true;
}
tYUX_BOOL UCBlockManager::RmvBlock(char *pBlockId)
{
    if (!pBlockId || strlen(pBlockId) <= 0)
        return (tYUX_BOOL)false;

    map<string, UCBlockBase*>::iterator it = _Blocks.find(pBlockId);
    if (it != _Blocks.end())
    {
        map<tYUX_PVOID, tYUX_BOOL>::iterator it2 = _BlocksHandling.find((tYUX_PVOID)it->second);
        if (it2 == _BlocksHandling.end())
            throw new exception("[UCBlockManager::RmvBlock] not found if handling the object!");
        if (it2->second)
        {
            UCBlockBase *pBlock = it->second;
            delete pBlock;
        }

        _Blocks.erase(it);
        _BlocksHandling.erase(it2);
    }

    return (tYUX_BOOL)true;
}
UCBlockBase* UCBlockManager::GetBlock(char *pBlockId)
{
    if (!pBlockId || strlen(pBlockId) <= 0)
        return NULL;

    map<string, UCBlockBase*>::iterator it = _Blocks.find(pBlockId);
    if (it != _Blocks.end())
    {
        UCBlockBase *pBlock = it->second;
        return pBlock;
    }

    return NULL;
}
UCBlockBase* UCBlockManager::GetBlockByIndex(size_t index_0)
{
    map<string, UCBlockBase*>::iterator it;
    size_t index = 0;
    for (it = _Blocks.begin(); it != _Blocks.end(); it++, index++)
    {
        if (index == index_0)
            return it->second;
    }

    return NULL;
}


// block get, set
tYUX_BOOL UCBlockManager::CallBlockSetMethod(char *pBlockId, char *pNameForSet, 
    tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize)
{
    if (!pBlockId || strlen(pBlockId) <= 0)
        return (tYUX_BOOL)false;

    map<string, UCBlockBase*>::iterator it = _Blocks.find(pBlockId);
    if (it != _Blocks.end())
    {
        UCBlockBase *pBlock = it->second;
        return pBlock->Set(pNameForSet, pData, nDataUnit, nDataSize);
    }

    return (tYUX_BOOL)false;
}
tYUX_BOOL UCBlockManager::CallBlockGetMethod(char *pBlockId, char *pNameForGet, 
    tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler)
{
    ppRetData = NULL;
    nRetDataUnit = 0;
    nRetDataSize = 0;
    fpRetMemHandler = NULL;

    if (!pBlockId || strlen(pBlockId) <= 0)
        return (tYUX_BOOL)false;

    map<string, UCBlockBase*>::iterator it = _Blocks.find(pBlockId);
    if (it != _Blocks.end())
    {
        UCBlockBase *pBlock = it->second;
        return pBlock->Get(pNameForGet, ppRetData, nRetDataUnit, nRetDataSize, fpRetMemHandler);
    }

    return (tYUX_BOOL)false;
}

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
void ClearMap(map<string, UCBlockBase*> *p, map<tYUX_PVOID, tYUX_BOOL> *p2)
{
    if (!p || !p2) return;

    map<string, UCBlockBase*>::iterator it;
    while (true)
    {
        it = p->begin();

        map<tYUX_PVOID, tYUX_BOOL>::iterator it2 = p2->find((tYUX_PVOID)it->second);
        if (it2 == p2->end())
            throw new exception("[UCBlockManager:ClearMap] not find if handling the object!");

        if (it2->second)
        {
            UCBlockBase* pBlock = it->second;
            if (pBlock) delete pBlock;
        }

        p->erase(it);
    }
    p->clear();
    p2->clear();
}
