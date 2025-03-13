#ifndef UCBlockPipeCommunication_h__
#define UCBlockPipeCommunication_h__

#include "UCCommunicationBase.h"
#include "PipeClient.h"

typedef struct tagUCBlocPipeCommInitDatSet
{
    char       *_pPipeName;
    tYUX_I32    _nMaxRx;
    fpCommOpenStatusCallback       _fpOpenedCallback;
    fpCommunicationDebugCallback   _fpDebugCallback;
}UCBlocPipeCommInitDatSet;

class UCPipeCommunication : public UCCommunicationBase
{
protected:
    PipeClient     *_pPipeClient;

public:
    UCPipeCommunication();
    virtual ~UCPipeCommunication();

    virtual tYUX_BOOL Open(tYUX_PVOID pDat, tYUX_I32 nSizeofDat, tYUX_I32 nNumofDat);
    virtual tYUX_BOOL Close();
    virtual tYUX_BOOL Add(tYUX_U8 *pTxDat, tYUX_I32 nOffset, tYUX_I32 nLen, fpCommunicationHandleMemCallback fpHandTxDat, tYUX_PVOID pContextForRx, fpCommunicationRxCallback fpHandleRxDat);
};

#endif
