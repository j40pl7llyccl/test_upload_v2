#include "stdafx.h"
#include "PipeClient.h"
#include <process.h>

struct PipeClientTxReq
{
    PVOID	_pTxBuff;
    INT32	_nOffsetIdx;
    INT32	_nTxLen;
    BOOL	_bHandleTxBuff;
    fpCommunicationHandleMemCallback	_fpHandleMem;
    PVOID                                   _pContextOfRxCallback;
    fpCommunicationRxCallback		    _fpRxDataCallback;

    PipeClientTxReq()
    {
        _pTxBuff = NULL;
        _nOffsetIdx = 0;
        _nTxLen = 0;
        _bHandleTxBuff = FALSE;
        _fpHandleMem = NULL;
        _pContextOfRxCallback = NULL;
        _fpRxDataCallback = NULL;
    }
    ~PipeClientTxReq()
    {
        if (_bHandleTxBuff && _fpHandleMem && _pTxBuff) _fpHandleMem(_pTxBuff);
        _pTxBuff = NULL;
        _fpHandleMem = NULL;
        _pContextOfRxCallback = NULL;
        _fpRxDataCallback = NULL;
    }
};

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
static void FreeTxListResources(std::list<void*> *pList)
{
    if (!pList) return;

    std::list< void * >::iterator iter;
    PipeClientTxReq *pReq;

    while (pList->size() > 0)
    {
        iter = pList->begin();
        pReq = (PipeClientTxReq *)(*iter);
        if (pReq) // NULL reference check
        {
            delete pReq;
        }
        pList->erase(iter);
    }

    pList->clear();
    delete pList;
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
static void FreeRxListBuffer(std::list<UCHAR*> *pList)
{
    if (!pList)return;

    std::list<UCHAR*>::iterator iter;
    UCHAR *pBuff;

    while (pList->size() > 0)
    {
        iter = pList->begin();
        pBuff = (UCHAR *)(*iter);
        if (pBuff) delete[] pBuff;
        pList->erase(iter);
    }

    pList->clear();
}

//----------------------------------------------------------------------------
//
//----------------------------------------------------------------------------
static UCHAR* MergeRxListBuffer(std::list<UCHAR*> *pList, UINT32 nUnit, UINT32 nTotal)
{
    if (!pList || nUnit == 0 || nTotal == 0) return NULL;

    UCHAR *pRet = new UCHAR[nTotal];
    if (!pRet) return NULL;

    std::list<UCHAR*>::iterator iter;
    UCHAR *pBuff, *pCurr = pRet;
    BOOL bAgain = TRUE;

    while (pList->size() > 0 && bAgain)
    {
        iter = pList->begin();
        pBuff = (UCHAR *)(*iter);

        memcpy(pCurr, pBuff, nTotal < nUnit ? nTotal : nUnit);
        bAgain = nTotal > nUnit;

        pCurr += nUnit;
        nTotal -= nUnit;

        pList->erase(iter);

    }
    pList->clear();

    return pRet;
}


///////////////////////////////////////////////////////////////////////////////
// 
///////////////////////////////////////////////////////////////////////////////
PipeClient::PipeClient(char *pPipeName, INT32 nMaxRx, fpCommOpenStatusCallback fpCallbackOpenStatus, fpCommunicationDebugCallback fpCallbackDbg)
    : UCObject()
{
    _strClassRttiName = typeid(*this).name();
    _bTerminate = FALSE;
    _bReady = FALSE;
    _fpOpenStatus = fpCallbackOpenStatus;
    _fpDebugMsg = fpCallbackDbg;

    _nMaxBuffSize = nMaxRx <= 0 ? PIPECLIENT_MAX_BUFF_SIZE : nMaxRx;

    // config the pipe name
    if (!pPipeName || strlen(pPipeName) <= 0)
        _pPipeName = NULL;
    else
    {
        _pPipeName = new char[strlen(pPipeName) + 1];
        if (_pPipeName)
        {
            memcpy(_pPipeName, pPipeName, strlen(pPipeName));
            _pPipeName[strlen(pPipeName)] = 0;
        }
    }

    _hProcThread = NULL; // thread handle
    _hEvtNotify = ::CreateEvent( NULL, TRUE, FALSE, NULL) ; // event
    _hSyncSem = ::CreateSemaphore( NULL, 1, 1, NULL ) ; // semaphore
    _hPipe = NULL; // pipe handle
    _listReq = new std::list<void*>(); // request

    // create thread
    _hProcThread = (HANDLE)_beginthreadex(NULL,
                                          0,
                                          ThreadProcFunc,
                                          (void *) this,
                                          0,
                                          &_dwID);
}

///////////////////////////////////////////////////////////////////////////////
// 
///////////////////////////////////////////////////////////////////////////////
PipeClient::~PipeClient()
{
    _bTerminate = TRUE;
    // free the name string
    if (_pPipeName) delete[] _pPipeName;
    _pPipeName = NULL;

    //
    // terminate thread
    //
    // trigger thread alive
    ::SetEvent(_hEvtNotify);
    // wait thread stop normally
    if(_hProcThread) ::WaitForSingleObject(_hProcThread, INFINITE);

    //
    // close handle resources
    //
    if(_hProcThread)::CloseHandle(_hProcThread);
    ::CloseHandle(_hEvtNotify);
    _hProcThread = NULL;
    _hEvtNotify = NULL;

    // close pipe
    if (_hPipe)
    {
        ::CloseHandle(_hPipe);
        _hPipe = NULL;
    }

    //
    // Free req data
    //
    ::WaitForSingleObject(_hSyncSem, INFINITE);
    FreeTxListResources(_listReq);
    ::ReleaseSemaphore(_hSyncSem, 1, NULL);

    //
    // close sem obj
    //
    ::CloseHandle(_hSyncSem);
    _hSyncSem = NULL;
}

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
unsigned int WINAPI PipeClient::ThreadProcFunc(void * lpParameter)
{
    // Don't know how to exec.
    if (!lpParameter) {
        return -1;
    }

    PipeClient *pObj = static_cast<PipeClient *> (lpParameter);

    if (pObj->_bTerminate)
    {
        return -2;
    }

    try
    {
        pObj->Process();
    }
    catch (...)
    {
        _endthreadex(-3);
    }

    return 0;
}

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
BOOL PipeClient::Initialize()
{
    if (!_pPipeName || strlen(_pPipeName) <= 0)
        return FALSE;

    // connect to pipe first
    CHAR pipName[1024];
    char dbgMsg[1024];
    sprintf_s(pipName, sizeof(pipName), "\\\\.\\pipe\\%s", _pPipeName);
    DWORD errCode;
    HANDLE hPipe = INVALID_HANDLE_VALUE;

    while (!_bTerminate)
    {
        hPipe = ::CreateFile(
            pipName,        // pipe name 
                             GENERIC_READ |  // read and write access 
                             GENERIC_WRITE,
                             0,              // no sharing 
                             NULL,           // default security attributes
                             OPEN_EXISTING,  // opens existing pipe 
                             0,              // default attributes 
                             NULL);          // no template file 
        if (hPipe != INVALID_HANDLE_VALUE)
            break;
        errCode = ::GetLastError();
        if (errCode != ERROR_PIPE_BUSY)
        {
            if (_fpDebugMsg)
            {
                sprintf_s(dbgMsg, sizeof(dbgMsg),  "[PipeClient::Initialize] could not open pipe with code %d.", errCode);
                _fpDebugMsg(dbgMsg, 0);
            }
            return FALSE;
        }
        if (!::WaitNamedPipe(pipName, 5000))
        {
            if (_fpDebugMsg) _fpDebugMsg("[PipeClient::Initialize] call WaitNamedPipe() for 5-sec timeout.", 0);
            return FALSE;
        }
    }

    if (_bTerminate)
    {
        if (hPipe != INVALID_HANDLE_VALUE)
            ::CloseHandle(hPipe);
        return FALSE;
    }

    // The pipe connected; change to message-read mode. 
    DWORD dwMode = PIPE_READMODE_MESSAGE;
    if (!::SetNamedPipeHandleState(hPipe, // pipe handle 
                                   &dwMode, // new pipe mode 
                                   NULL,    // don't set maximum bytes 
                                   NULL))   // don't set maximum time 
    {
        if (_fpDebugMsg)
        {
            sprintf_s(dbgMsg, sizeof(dbgMsg), "[PipeClient::Initialize] call SetNamedPipeHandleState() with error code %d.", ::GetLastError());
            _fpDebugMsg(dbgMsg, 0);
        }
        return FALSE;
    }

    _hPipe = hPipe;
    return TRUE;
}

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------
VOID PipeClient::Process()
{
    _bReady = Initialize();
    if (_bTerminate)
    {
        if (_fpOpenStatus)_fpOpenStatus(_pPipeName, FALSE);
        return;
    }
    if (!_bReady)
    {
        if (_fpDebugMsg) _fpDebugMsg("[PipeClient::Process()] call Initialize() fail.", 0);
        if (_fpOpenStatus)_fpOpenStatus(_pPipeName, FALSE);
        return;
    } else {
        if (_fpOpenStatus) _fpOpenStatus(_pPipeName, TRUE);
    }

    INT32 state = 0;
    PipeClientTxReq *pCurr = NULL;
    BOOL bSucc = false;
    DWORD nTx = 0, nRx = 0;
    std::list<UCHAR*> *listRx = new std::list<UCHAR*>();
    BOOL bExit = false;
    DWORD code = 0;
    std::list<void*>::iterator iter;
    void * pvoid;
    UCHAR *pChar;
    char dbgMsg[1024];
    UINT32 nRxTotal = 0;

    while (!_bTerminate && !bExit)
    {
        switch (state)
        {
        // wait request
        case 0:
            if (_listReq->size() > 0)
            {
                ::WaitForSingleObject(_hSyncSem, INFINITE);
                if (_listReq->size() > 0)
                {
                    iter = _listReq->begin();
                    pCurr = (PipeClientTxReq*)(*iter);
                    _listReq->erase(iter);
                }
                else
                    pCurr = NULL;
                ::ReleaseSemaphore(_hSyncSem, 1, NULL);
                state = 1;
            }
            else
            {
                ::WaitForSingleObject(_hEvtNotify, INFINITE);
                ::ResetEvent(_hEvtNotify);
                if (_bTerminate) // triggered terminate
                    break;
            }
            break;

        // tx request
        case 1:
            if (!pCurr || !pCurr->_pTxBuff || pCurr->_nTxLen <= 0)
            {
                if (pCurr) delete pCurr;
                pCurr = NULL;
                state = 0; break;
            }
            pChar = (UCHAR *)pCurr->_pTxBuff;
            pvoid = (void*)(&pChar[pCurr->_nOffsetIdx]);
            bSucc = ::WriteFile(_hPipe, pvoid, (DWORD)pCurr->_nTxLen, &nTx, NULL); // write to pipe
            if (!bSucc)
            {
                code = ::GetLastError();
                bExit = true; // trigger thread die
                _bReady = false;

                if (_fpDebugMsg)
                {
                    sprintf_s(dbgMsg, sizeof(dbgMsg), "[PipeClient::Process] tx data error with code(%d) & connection going to failure.", code);
                    _fpDebugMsg(dbgMsg, 0);
                }
                // notify back to caller
                if (pCurr->_fpRxDataCallback)
                {
                    sprintf_s(dbgMsg, sizeof(dbgMsg), "Tx data error with code(%d) & connection going to failure.", code);
                    pCurr->_fpRxDataCallback(pCurr->_pContextOfRxCallback, PS_TXERROR, NULL, 0, FALSE, dbgMsg);
                }
                // free pCurr
                delete pCurr; pCurr = NULL;
                continue;
            }
            // change to rx state
            state = 2;
            // clear rx data
            nRxTotal = 0;
            FreeRxListBuffer(listRx);

            break;

        // rx response
        case 2:
        {
            // alloc for current rx
            UCHAR *pRxBuff = new UCHAR[_nMaxBuffSize];
            if (!pRxBuff)
            {
                state = 0;
                // notify back to caller
                if (pCurr->_fpRxDataCallback)
                    pCurr->_fpRxDataCallback(pCurr->_pContextOfRxCallback, PS_RXERROR, NULL, 0, FALSE, "Rx error!");
                delete pCurr; pCurr = NULL;
                break;
            }

            // read data from pipe & keep in list
            bSucc = ::ReadFile(_hPipe, (LPVOID)pRxBuff, _nMaxBuffSize, &nRx, NULL);
            code = ::GetLastError();
            listRx->push_back(pRxBuff);

            // error handling
            if (!bSucc && code != ERROR_MORE_DATA)
            {
                if (pCurr->_fpRxDataCallback)
                {
                    sprintf_s(dbgMsg, sizeof(dbgMsg), "Rx data error with code(%d).", code);
                    pCurr->_fpRxDataCallback(pCurr->_pContextOfRxCallback, PS_RXERROR, NULL, 0, FALSE, dbgMsg);
                }
                if (_fpDebugMsg)
                {
                    sprintf_s(dbgMsg, sizeof(dbgMsg), "[PipeClient::Process] rx error with code(%d).", code);
                    _fpDebugMsg(dbgMsg, 0);
                }
                // manage mem
                delete pCurr; pCurr = NULL;
                FreeRxListBuffer(listRx);
                // change state to wait
                state = 0;
                break;
            }

            nRxTotal += nRx;
            if (bSucc) // meaning all data rx
            {
                if (listRx->size() == 1)
                {
                    std::list<UCHAR*>::iterator iit = listRx->begin();
                    UCHAR *p1st = (UCHAR*)(*iit);
                    if (pCurr->_fpRxDataCallback)
                        pCurr->_fpRxDataCallback(pCurr->_pContextOfRxCallback, PS_DATAREADY, p1st, nRx, TRUE, NULL);
                }
                else if (listRx->size() > 1)
                {
                    UCHAR *pPack = MergeRxListBuffer(listRx, _nMaxBuffSize, nRxTotal);
                    if (pCurr->_fpRxDataCallback)
                        pCurr->_fpRxDataCallback(pCurr->_pContextOfRxCallback, PS_DATAREADY, pPack, nRxTotal, TRUE, NULL);
                    delete[] pPack;
                }
                // manage mem
                FreeRxListBuffer(listRx);
                delete pCurr; pCurr = NULL;
                // chnage state to wait
                state = 0;
            }

            break;
        }
        } // end-switch
    } // end-while

    FreeRxListBuffer(listRx);
    delete listRx;

    if (pCurr) delete pCurr;
}

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
BOOL PipeClient::Add(UCHAR *txBuff, INT32 offset, INT32 len, BOOL bHandleBuff,
                     fpCommunicationHandleMemCallback fpHandleMem,
                     PVOID pRxContext, fpCommunicationRxCallback fpRxDat)
{
    if (!Available())
        return FALSE;

    if (txBuff == NULL || len <= 0) return FALSE;

    BOOL ret = FALSE;

    ::WaitForSingleObject(_hSyncSem, INFINITE);
    if (Available())
    {
        PipeClientTxReq *req = new PipeClientTxReq();
        req->_pTxBuff = (PVOID)txBuff;
        req->_nOffsetIdx = offset;
        req->_nTxLen = len;
        req->_bHandleTxBuff = bHandleBuff;
        req->_fpHandleMem = fpHandleMem;
        req->_pContextOfRxCallback = pRxContext;
        req->_fpRxDataCallback = fpRxDat;

        _listReq->push_back((PVOID)req);
        ret = TRUE;
    }
    ::ReleaseSemaphore(_hSyncSem, 1, NULL);
    if (!ret)
        return FALSE;

    ::SetEvent(_hEvtNotify);
    return TRUE;
}
