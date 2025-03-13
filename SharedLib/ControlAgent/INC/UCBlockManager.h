#ifndef UCBlockManager_h__
#define UCBlockManager_h__

#include "UCObject.h"
#include "UCCommunicationManager.h"
#include "UCSharedMemFormating.h"
#include "UCBlockBase.h"
#include <map>
#include <string>

using namespace std;

class UCBlockManager : public UCObject
{
protected:
    map<string, UCBlockBase*>       _Blocks;
    map<tYUX_PVOID, tYUX_BOOL>      _BlocksHandling;
    UCCommunicationManager         *_pCommunicationMgr; // reference to an object
    UCSharedMemFormating           *_pFormatSharedMems; // reference to an object
    UCObject                       *_pEnvSharedMemInt32; // reference to an object
    UCObject                       *_pEnvSharedMemInt64; // reference to an object
    UCObject                       *_pEnvSharedMemDouble; // reference to an object

public:
    UCBlockManager();
    UCBlockManager(UCCommunicationManager *pCommMgr, UCSharedMemFormating *pShMemFormating, UCObject *pEnvShMem32, UCObject *pEnvShMem64, UCObject *pEnvShMemDf);
    virtual ~UCBlockManager();

    // set reference objects
    void SetReferenceObjects(UCCommunicationManager *pCommMgr, UCSharedMemFormating *pShMemFormating, UCObject *pEnvShMem32, UCObject *pEnvShMem64, UCObject *pEnvShMemDf);

    // block management
    virtual tYUX_BOOL AddBlock(UCBlockBase *pBlock, tYUX_BOOL bHandledByThisClass = (tYUX_BOOL) true);
    virtual tYUX_BOOL RmvBlock(char *pBlockId);
    UCBlockBase* GetBlock(char *pBlockId);
    size_t GetNumOfBlocks() { return _Blocks.size(); }
    UCBlockBase* GetBlockByIndex(size_t index_0);

    // block get, set
    virtual tYUX_BOOL CallBlockSetMethod(char *pBlockId, char *pNameForSet, tYUX_PVOID pData, tYUX_I32 nDataUnit, tYUX_I32 nDataSize);
    virtual tYUX_BOOL CallBlockGetMethod(char *pBlockId, char *pNameForGet, tYUX_PVOID &ppRetData, tYUX_I32 &nRetDataUnit, tYUX_I32 &nRetDataSize, fpUCBlockHandleMem &fpRetMemHandler);
};

#endif
