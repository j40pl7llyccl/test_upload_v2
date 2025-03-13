#ifndef UCCommunicationDecl_h__
#define UCCommunicationDecl_h__

#include "UBaseTypeDecl.h"

typedef void(*fpCommOpenStatusCallback)(char *pName, tYUX_BOOL bStatus);
typedef void(*fpCommunicationDebugCallback)(char *pMsg, tYUX_I32 lvl);
typedef void(*fpCommunicationRxCallback)(tYUX_PVOID pContext, tYUX_I32 status, tYUX_U8 *buff, tYUX_U32 nRx, tYUX_BOOL bOk, char *pResaon); // tYUX_U32 statusCode);
typedef void(*fpCommunicationHandleMemCallback)(tYUX_PVOID pMem);

#endif
