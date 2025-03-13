#ifndef _PipeClient_h__
#define _PipeClient_h__

#include "UCObject.h"
#include "windows.h"
#include <list>
#include "UCCommunicationDecl.h"

typedef enum
{
	PS_NA = 0,
	PS_CONNECTING,
	PS_READING,
	PS_WRITING,
	PS_DATAREADY,
	PS_TXERROR,
	PS_RXERROR,
	PS_CONNECTIONFAILURE,
}PipeState;

//typedef VOID (*fpNamedPipeOpenStatus)(char *pName, BOOL bStatus);
//typedef VOID (*fpPipeDebug)(char *pMsg, INT32 lvl);
//typedef VOID (*fpCltProcPipeRxDat)(PVOID pContext, INT32 status, UCHAR *buff, UINT32 nRx, DWORD statusCode);
//typedef VOID(*fpPipeClientHandleMem)(PVOID pMem);

#define PIPECLIENT_MAX_BUFF_SIZE    4096

//
// Rx callback:
//  - In callback function, the buffer is handled by this class. If need to keep, copy it again.
//
class PipeClient : public UCObject
{
private:
    static unsigned int WINAPI ThreadProcFunc(void * lpParameter);

private:
    BOOL                    _bTerminate;
	BOOL					_bReady;
    fpCommOpenStatusCallback	    _fpOpenStatus;
    fpCommunicationDebugCallback	_fpDebugMsg;

	HANDLE					_hProcThread; // thread handle
    UINT32                  _dwID;
	HANDLE					_hEvtNotify; // event
	HANDLE					_hSyncSem; // semaphore
	HANDLE					_hPipe; // pipe handle
	std::list<void*>        *_listReq; // request

	char					*_pPipeName;
    INT32                   _nMaxBuffSize;


	BOOL Initialize();
	VOID Process();

public:
	PipeClient(char *pPipeName, INT32 nMaxRx, fpCommOpenStatusCallback fpCallbackOpenStatus, fpCommunicationDebugCallback fpCallbackDbg);
	virtual ~PipeClient();

    BOOL Available() { return _bReady && !_bTerminate; }
	BOOL Add(UCHAR *txBuff, INT32 offset, INT32 len, BOOL bHandleBuff, fpCommunicationHandleMemCallback fpHandleMem, PVOID pRxContext, fpCommunicationRxCallback fpRxDat);
};


#endif
