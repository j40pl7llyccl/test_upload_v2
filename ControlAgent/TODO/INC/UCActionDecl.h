#ifndef UCActionDecl_h__
#define UCActionDecl_h__

#include "UCSharedMemFormatingDecl.h"
#include "UCBlockBase.h"

struct TActionBlockDesc;
struct TActionStage;
struct TActionBlockActivate;

// return TActionBlockDesc* must use the new TActionBlockChainedPreviousOutput[nNextCount]
typedef TActionBlockDesc*   (*fpActionBlockGenNextLevelBlocks)(vector<TActionStage> *pAction, int currLevel, UCBlockBase *pCurrBlock, int &nNextBlocks);

struct TActionBlockChainedPreviousOutput
{
    tYUX_BOOL       _bEnbaled;
    int             _nIndexOfAction;        // to identify which block in a container such as vector, list...
    string          _strNameOfId;           // name of block ID
    UCBlockBase    *_pWhichBlock;           // check the pointer is availabel before using

    string          _strNameOfPrevBlockOutput;  // name of output defined in previous block
    const char     *_pNameOfCurrBlockInput;   // name of input defined in current block
    tYUX_BOOL       _bBlockInvoked;

    TUCBlockOChaining  *_pPrevOutput;
    TUCBlockIChaining  *_pCurrInput;

    TActionBlockChainedPreviousOutput();
};
struct TActionBlockDesc
{
    UCBlockBase            *_pBlockInstance;
    tYUX_BOOL               _bHandleBlock;
    tYUX_BOOL               _bIsDynamicBlock;

    TUCBlockMutableParam                        _ExecParam;
    vector<TActionBlockChainedPreviousOutput>   _ChainedPrevOutputs;
    fpActionBlockGenNextLevelBlocks             _fpGenNext;

    TActionBlockDesc();
    TActionBlockDesc(UCBlockBase *pObj, tYUX_BOOL bHandleObj = (tYUX_BOOL)false, tYUX_BOOL bDynamicBlock = (tYUX_BOOL)false);
    ~TActionBlockDesc();

    void ClearExecParam();
    void SetExecParam(tYUX_PVOID pDat, tYUX_I32 nSizeof, tYUX_I64 nNumof, fpUCBlockHandleMem fpHandleMem);

    /* link block: 當前的 block 執行時會與先前的哪些 block  */
    tYUX_BOOL InvokePreviousBlock(vector<TActionBlockActivate *> *pProcess, size_t nCurrIndex);
    // link data of blocks
    tYUX_BOOL InvokePreviousBlockOutputs();
};
struct TActionBlockActivate
{
    //TActionStage       *_pPreviousLevel;
    //TActionStage       *_pCurrentLevel;
    TActionBlockDesc   *_pBlockDat;
    tYUX_BOOL           _bHandleBlockDesc;

    TActionBlockActivate()
    {
        //_pPreviousLevel = NULL;
        //_pCurrentLevel = NULL;
        _pBlockDat = new TActionBlockDesc();
        _bHandleBlockDesc = (tYUX_BOOL)true;
    }
    TActionBlockActivate(TActionBlockDesc *pDesc, tYUX_BOOL bHandleIt)
    {
        //_pPreviousLevel = NULL;
        //_pCurrentLevel = NULL;
        _pBlockDat = pDesc;
        _bHandleBlockDesc = bHandleIt;
    }
    ~TActionBlockActivate()
    {
        if (_pBlockDat && _bHandleBlockDesc)
            delete _pBlockDat;
        _pBlockDat = NULL;
    }

    static TActionBlockActivate* New() { return new TActionBlockActivate(); }
    static TActionBlockActivate* New(TActionBlockDesc *pDesc, tYUX_BOOL bHandleIt = (tYUX_BOOL)true) { return new TActionBlockActivate(pDesc, bHandleIt); }
};
struct TActionStage
{
    vector<TActionBlockActivate*>   _ActivatedBlocks;

    TActionStage() {}
    ~TActionStage()
    {
        vector<TActionBlockActivate*>::iterator it;
        while (_ActivatedBlocks.size() > 0)
        {
            it = _ActivatedBlocks.begin();
            TActionBlockActivate* p = (TActionBlockActivate*)(*it);
            if (p)
                delete p;
            _ActivatedBlocks.erase(it);
        }
    }
};

#endif
