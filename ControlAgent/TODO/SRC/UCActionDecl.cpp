#include "stdafx.h"
#include "UCActionDecl.h"

///////////////////////////////////////////////////////////////////////////////
// TActionBlockChainedPreviousOutput
///////////////////////////////////////////////////////////////////////////////
TActionBlockChainedPreviousOutput::TActionBlockChainedPreviousOutput()
{
    _bEnbaled = (tYUX_BOOL)true;
    _nIndexOfAction = -1;
    _pWhichBlock = NULL;
    _bBlockInvoked = (tYUX_BOOL)false;
    _pNameOfCurrBlockInput = NULL;

    _pPrevOutput = NULL;
    _pCurrInput = NULL;
}

///////////////////////////////////////////////////////////////////////////////
// TActionBlockDesc
///////////////////////////////////////////////////////////////////////////////
TActionBlockDesc::TActionBlockDesc()
{
    _pBlockInstance = NULL;
    _bHandleBlock = false;
    _bIsDynamicBlock = false;

    _fpGenNext = NULL;
}
TActionBlockDesc::TActionBlockDesc(UCBlockBase *pObj, tYUX_BOOL bHandleObj, tYUX_BOOL bDynamicBlock)
{
    _pBlockInstance = pObj;
    _bHandleBlock = bHandleObj;
    _bIsDynamicBlock = bDynamicBlock;

    _fpGenNext = NULL;
}
TActionBlockDesc::~TActionBlockDesc()
{
    if (_pBlockInstance && _bHandleBlock)
        delete _pBlockInstance;
    _pBlockInstance = NULL;
    _bHandleBlock = false;

    ClearExecParam();
}

void TActionBlockDesc::ClearExecParam()
{
    if (_ExecParam._pData && _ExecParam._fpMemHandler)
        _ExecParam._fpMemHandler(_ExecParam._pData);
    _ExecParam._pData = NULL;
    _ExecParam._fpMemHandler = NULL;
    _ExecParam._nUnit = 0;
    _ExecParam._nSize = 0;
}
void TActionBlockDesc::SetExecParam(tYUX_PVOID pDat, tYUX_I32 nSizeof, tYUX_I64 nNumof, fpUCBlockHandleMem fpHandleMem)
{
    ClearExecParam();

    _ExecParam._pData = pDat;
    _ExecParam._nUnit = nSizeof;
    _ExecParam._nSize = nNumof;
    _ExecParam._fpMemHandler = fpHandleMem;
}
tYUX_BOOL TActionBlockDesc::InvokePreviousBlock(vector<TActionBlockActivate *> *pProcess, size_t nCurrIndex)
{
    if (!pProcess)
        return (tYUX_BOOL)false;

    tYUX_BOOL ret = (tYUX_BOOL)true;
    // invocation
    for (size_t i = 0; i < _ChainedPrevOutputs.size(); i++)
    {
        TActionBlockChainedPreviousOutput& chain = _ChainedPrevOutputs[i];
        chain._bBlockInvoked = (tYUX_BOOL)false;
        chain._pWhichBlock = NULL;

        // searching prev block
        for (size_t j = 0; j < nCurrIndex && j < pProcess->size(); j++)
        {
            TActionBlockActivate *prev = (*pProcess)[j];
            if (prev->_pBlockDat && prev->_pBlockDat->_pBlockInstance
                && !strcmp(prev->_pBlockDat->_pBlockInstance->ID(), chain._strNameOfId.c_str()) // ID name matching ?
                && j == chain._nIndexOfAction // index matching ?
                )
            {
                chain._bBlockInvoked = (tYUX_BOOL)true;
                chain._pWhichBlock = prev->_pBlockDat->_pBlockInstance;
                break;
            }
        } // end for-j

        if (!chain._bBlockInvoked)
            ret = (tYUX_BOOL)false;
    } // end for-i

    return ret;
}
tYUX_BOOL TActionBlockDesc::InvokePreviousBlockOutputs()
{
    if (!_pBlockInstance)
        return (tYUX_BOOL)false;
    tYUX_BOOL ret = (tYUX_BOOL)true;
    // invocation
    for (size_t i = 0; i < _ChainedPrevOutputs.size(); i++)
    {
        TActionBlockChainedPreviousOutput& chain = _ChainedPrevOutputs[i];
        if (chain._pWhichBlock)
        {
            chain._pPrevOutput = chain._pWhichBlock->GetOutputDat(chain._strNameOfPrevBlockOutput.c_str());
            chain._pCurrInput = _pBlockInstance->GetInputDat(chain._pNameOfCurrBlockInput);
            if (!chain._bEnbaled && (!chain._pPrevOutput || !chain._pCurrInput))
                ret = (tYUX_BOOL)false;
        }
        else
            ret = (tYUX_BOOL)false;
    } // end for-i

    return ret;
}


