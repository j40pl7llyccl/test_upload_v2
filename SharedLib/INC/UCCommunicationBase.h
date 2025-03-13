#ifndef UCCommunicationBase_h__
#define UCCommunicationBase_h__

#include "UCObject.h"
#include "UCCommunicationDecl.h"

#define MAX_BLOCK_COMM_NAME     256

class UCCommunicationBase : public UCObject
{
protected:
    tYUX_I8 _strID[MAX_BLOCK_COMM_NAME];

public:
    UCCommunicationBase() : UCObject()
    {
        _strID[0] = 0;
        _strClassRttiName = typeid(*this).name();
    }
    UCCommunicationBase(tYUX_I8 *pName) : UCObject()
    {
        ID(pName);
        _strClassRttiName = typeid(*this).name();
    }
    virtual ~UCCommunicationBase() {}

    virtual tYUX_BOOL Open(tYUX_PVOID pDat, tYUX_I32 nSizeofDat, tYUX_I32 nNumofDat) = 0;
    virtual tYUX_BOOL Close() = 0;
    virtual tYUX_BOOL Add(tYUX_U8 *pTxDat, tYUX_I32 nOffset, tYUX_I32 nLen, fpCommunicationHandleMemCallback fpHandTxDat, tYUX_PVOID pContextForRx, fpCommunicationRxCallback fpHandleRxDat) = 0;

    void ID(tYUX_I8 *pName)
    {
        _strID[0] = 0;
        if (!pName || strlen(pName))
            return;

        int srcLen = (int)strlen(pName);
        int cpLen = srcLen > (MAX_BLOCK_COMM_NAME - 1) ? MAX_BLOCK_COMM_NAME - 1 : srcLen;
        memcpy(_strID, pName, cpLen);
        _strID[cpLen] = 0;
    }
    tYUX_I8* ID() { return _strID; }

};


#endif
