#include "stdafx.h"
#include "UCPipeCommunication.h"

UCPipeCommunication::UCPipeCommunication()
    :UCCommunicationBase()
{
    _strClassRttiName = typeid(*this).name();
    _pPipeClient = NULL;
}
UCPipeCommunication::~UCPipeCommunication()
{
    if (_pPipeClient)
    {
        delete _pPipeClient;
        _pPipeClient = NULL;
    }
}

tYUX_BOOL UCPipeCommunication::Open(tYUX_PVOID pDat, tYUX_I32 nSizeofDat, tYUX_I32 nNumofDat)
{
    if (_pPipeClient)
    {
        return _pPipeClient->Available();
    }
    if (!pDat || nSizeofDat != sizeof(UCBlocPipeCommInitDatSet) || nNumofDat <= 0)
        return (tYUX_BOOL)false;

    UCBlocPipeCommInitDatSet *pConv = static_cast<UCBlocPipeCommInitDatSet*>(pDat);
    _pPipeClient = new PipeClient(pConv->_pPipeName, pConv->_nMaxRx, pConv->_fpOpenedCallback, pConv->_fpDebugCallback);
    ID(pConv->_pPipeName);

    return (tYUX_BOOL)true;
}
tYUX_BOOL UCPipeCommunication::Close()
{
    if (_pPipeClient)
    {
        delete _pPipeClient;
        _pPipeClient = NULL;
    }
    return TRUE;
}
tYUX_BOOL UCPipeCommunication::Add(tYUX_U8 *pTxDat, tYUX_I32 nOffset, tYUX_I32 nLen, fpCommunicationHandleMemCallback fpHandTxDat,
    tYUX_PVOID pContextForRx, fpCommunicationRxCallback fpHandleRxDat)
{
    if (!_pPipeClient || !_pPipeClient->Available())
    {
        if (fpHandTxDat)
            fpHandTxDat((tYUX_PVOID)pTxDat);
        if (fpHandleRxDat)
            fpHandleRxDat(pContextForRx, PS_NA, NULL, 0, FALSE, "Pipe not ready!");
        return FALSE;
    }

    return _pPipeClient->Add(pTxDat, nOffset, nLen, fpHandTxDat ? TRUE : FALSE, fpHandTxDat, pContextForRx, fpHandleRxDat);
}

