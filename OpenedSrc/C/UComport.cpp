#include "stdafx.h"
#include "UComport.h"
#include <stdio.h>

UComport::UComport()
{
    m_hComm = NULL;
    m_hSyncSem = ::CreateSemaphoreA(NULL, 1, 1, NULL);
    m_bReady = FALSE;
    m_nComPort = 0;
    m_bEnableSem = TRUE;
}

UComport::~UComport()
{
    ClosePort();
    ::CloseHandle(m_hSyncSem);
    m_hSyncSem = NULL;
}

BOOL UComport::Open(INT32 whichPort, INT32 baudrate, ComParity parity, 
        BYTE dataBits, ComStopBits stopBits, ComFlowControl flowCtrl,
        BOOL overlapped)
{
    SemSync(); // critical enter
    ClosePort(FALSE);
    CHAR portDesc[128];
    sprintf_s(portDesc, sizeof(portDesc) - 1, "\\\\.\\COM%d", whichPort);
    BOOL bOk;
    // open file description
    if (!(bOk = OpenPort(portDesc, overlapped, FALSE))) {
        SemRele(); // critical leave
        return FALSE;
    }
    // config the description
    DCB dcb;
    dcb.DCBlength = sizeof(DCB);
    if (!::GetCommState(m_hComm, &dcb)) {
        SemRele(); // critical leave
        return FALSE;
    }
    // conf: baudrate
    dcb.BaudRate = baudrate;
    // conf: parity
    switch(parity) {
    case PT_Odd: dcb.Parity = ODDPARITY; break;
    case PT_Even: dcb.Parity = EVENPARITY; break;
    case PT_Mark: dcb.Parity = MARKPARITY; break;
    case PT_Space: dcb.Parity = SPACEPARITY; break;
    default: dcb.Parity = NOPARITY; break;
    }
    // conf: data bits
    dcb.ByteSize = dataBits;
    // conf: stop bits
    switch(stopBits) {
    case SB_One: dcb.StopBits = ONESTOPBIT; break;
    case SB_OnePointFive: dcb.StopBits = ONE5STOPBITS; break;
    case SB_Two: dcb.StopBits = TWOSTOPBITS; break;
    default: dcb.StopBits = ONESTOPBIT; break;
    }
    // conf: flow ctrl
    dcb.fDsrSensitivity = FALSE;
    switch(flowCtrl) {
    case FC_No:
    default:
        dcb.fOutxCtsFlow = FALSE;
        dcb.fOutxDsrFlow = FALSE;
        dcb.fOutX = FALSE;
        dcb.fInX = FALSE;
        break;
    case FC_CtsRts:
        dcb.fOutxCtsFlow = TRUE;
        dcb.fOutxDsrFlow = FALSE;
        dcb.fRtsControl = RTS_CONTROL_HANDSHAKE;
        dcb.fOutX = FALSE;
        dcb.fInX = FALSE;
        break;
    case FC_CtsDtr:
        dcb.fOutxCtsFlow = TRUE;
        dcb.fOutxDsrFlow = FALSE;
        dcb.fDtrControl = DTR_CONTROL_HANDSHAKE;
        dcb.fOutX = FALSE;
        dcb.fInX = FALSE;
        break;
    case FC_DsrRts:
        dcb.fOutxCtsFlow = FALSE;
        dcb.fOutxDsrFlow = TRUE;
        dcb.fRtsControl = RTS_CONTROL_HANDSHAKE;
        dcb.fOutX = FALSE;
        dcb.fInX = FALSE;
        break;
    case FC_DsrDtr:
        dcb.fOutxCtsFlow = FALSE;
        dcb.fOutxDsrFlow = TRUE;
        dcb.fDtrControl = DTR_CONTROL_HANDSHAKE;
        dcb.fOutX = FALSE;
        dcb.fInX = FALSE;
        break;
    case FC_XonXoff:
        dcb.fOutxCtsFlow = FALSE;
        dcb.fOutxDsrFlow = FALSE;
        dcb.fOutX = TRUE;
        dcb.fInX = TRUE;
        dcb.XonChar = 0x11;
        dcb.XoffChar = 0x13;
        dcb.XoffLim = 100;
        dcb.XonLim = 100;
        break;
    }
    m_bReady = ::SetCommState(m_hComm, &dcb);
    m_nComPort = whichPort;
    SemRele(); // critical leave
#ifdef _DEBUG
    printf("[COM%d] open status = %s\n", m_nComPort, m_bReady ? "OK" : "Fail");
#endif
    return m_bReady;
}

VOID UComport::Close()
{
    ClosePort();
}

BOOL UComport::Read(VOID* pBuff, DWORD nNumToRead, DWORD &nRead)
{
    if (!m_bReady) return FALSE;
    BOOL bRet;
    SemSync(); // critical enter

    bRet = ::ReadFile(m_hComm, pBuff, nNumToRead, &nRead, NULL);

    SemRele(); // critical leave
    return bRet;
}
BOOL UComport::ReadEx(VOID* pBuff, DWORD nNumToRead, DWORD &nRead, 
    LPOVERLAPPED lpOverlapped, LPOVERLAPPED_COMPLETION_ROUTINE lpCompletion)
{
    if (!m_bReady) return FALSE;
    BOOL bRet;
    //SemSync(); // critical enter

    bRet = ::ReadFileEx(m_hComm, pBuff, nNumToRead, lpOverlapped, lpCompletion);

    //SemRele(); // cirtical leave
    return bRet;
}
BOOL UComport::Write(VOID* pBuff, DWORD nNumToWrite, DWORD &nWritten)
{
    if (!m_bReady) return FALSE;
    BOOL bRet;
    SemSync(); // critical enter

    bRet = ::WriteFile(m_hComm, pBuff, nNumToWrite, &nWritten, NULL);

    SemRele(); // critical leave
    return bRet;
}
BOOL UComport::WriteEx(VOID* pBuff, DWORD nNumToWrite, LPOVERLAPPED lpOverlapped, 
    LPOVERLAPPED_COMPLETION_ROUTINE lpCompletion)
{
    if (!m_bReady) return FALSE;
    BOOL bRet;

    bRet = ::WriteFileEx(m_hComm, pBuff, nNumToWrite, lpOverlapped, lpCompletion);

    return bRet;
}

BOOL UComport::TxChar(char c)
{
    if (!m_bReady) return FALSE;
    return ::TransmitCommChar(m_hComm, c);
}
BOOL UComport::GetOverlapped(OVERLAPPED &overlapped, DWORD &nTransferred, BOOL wait)
{
    if (!m_bReady) return FALSE;
    return ::GetOverlappedResult(m_hComm, &overlapped, &nTransferred, wait);
}
//BOOL UComport::GetOverlappedEx(OVERLAPPED &overlapped, DWORD &nTransferred, DWORD millisec, BOOL bAlertable)
//{
//    if (!m_bReady) return FALSE;
//    return ::GetOverlappedResultEx(m_hComm, &overlapped, &nTransferred, millisec, bAlertable);
//}
BOOL UComport::CancelIO()
{
    if (!m_bReady) return FALSE;
    return ::CancelIo(m_hComm);
}
BOOL UComport::CancelIOEx(LPOVERLAPPED lpOverlapped)
{
    if (!m_bReady) return FALSE;
    return ::CancelIoEx(m_hComm, lpOverlapped);
}
BOOL UComport::BytesWaiting(DWORD &n)
{
    n = 0;
    if (!m_bReady) return FALSE;
    COMSTAT stat;
    DWORD dwErr = 0;
    if (!::ClearCommError(m_hComm, &dwErr, &stat))
        return FALSE;
    n = stat.cbOutQue;
    return TRUE;
}
BOOL UComport::ClearBreak()
{
    if (!m_bReady) return FALSE;
    return ::ClearCommBreak(m_hComm);
}
BOOL UComport::SetBreak()
{
    if (!m_bReady) return FALSE;
    return ::SetCommBreak(m_hComm);
}
BOOL UComport::ClearErr(DWORD &dwErr)
{
    if (!m_bReady) return FALSE;
    return ::ClearCommError(m_hComm, &dwErr, NULL);
}
BOOL UComport::GetStatus(COMSTAT &stat, DWORD &dwErr)
{
    dwErr = 0;
    if (!m_bReady) return FALSE;
    return ::ClearCommError(m_hComm, &dwErr, &stat);
}
BOOL UComport::CommState(DCB dcb)
{
    if (!m_bReady) return FALSE;
    return ::SetCommState(m_hComm, &dcb);
}
DCB UComport::CommState(BOOL &isOk)
{
    DCB dcb;
    ::memset(&dcb, 0x0, sizeof(DCB));
    dcb.DCBlength = sizeof(DCB);
    if (!m_bReady) {
        isOk = FALSE;
        return dcb;
    }

    isOk = ::GetCommState(m_hComm, &dcb);
    return dcb;
}

BOOL UComport::SetRWTimeout(DWORD r, DWORD w)
{
    COMMTIMEOUTS timeouts;
    ::memset(&timeouts, 0, sizeof(timeouts));
    timeouts.ReadTotalTimeoutConstant = r;
    timeouts.WriteTotalTimeoutConstant = w;
#ifdef _DEBUG
    printf("[COM%d] connfig timeout ReadTotalTimeoutConstant = %d, WriteTotalTimeoutConstant = %d\n", m_nComPort, r, w);
#endif
    return ::SetCommTimeouts(m_hComm, &timeouts);
}
BOOL UComport::WaitEvt(DWORD &dwMask, LPOVERLAPPED pOverlap)
{
    if (!m_bReady) return FALSE;
    if (!pOverlap) {
        return ::WaitCommEvent(m_hComm, &dwMask, NULL);
    }
    if (pOverlap->hEvent == NULL) return FALSE;
    return ::WaitCommEvent(m_hComm, &dwMask, pOverlap);
}
BOOL UComport::Flush()
{
    if (!m_bReady) return FALSE;
    return ::FlushFileBuffers(m_hComm);
}
BOOL UComport::TerminateOutstandingWrite() { return Purge(PURGE_TXABORT); }
BOOL UComport::TerminateOutstandingRead() { return Purge(PURGE_RXABORT); }
BOOL UComport::ClrWriteBuff() { return Purge(PURGE_TXCLEAR); }
BOOL UComport::ClrReadBuff() { return Purge(PURGE_RXCLEAR); }
BOOL UComport::SetupTxRxQueSize(DWORD nInQ, DWORD nOutQ)
{
    if (!m_bReady) return FALSE;
#ifdef _DEBUG
    printf("[COM%d] connfig Q, tx = %d, rx = %d\n", m_nComPort, nInQ, nOutQ);
#endif
    return ::SetupComm(m_hComm, nInQ, nOutQ);
}


BOOL UComport::OpenPort(LPCSTR pPort, BOOL overlaped, BOOL bSync)
{
    if (bSync) SemSync();
    m_hComm = ::CreateFileA( pPort, GENERIC_READ | GENERIC_WRITE, 0, nullptr, OPEN_EXISTING, overlaped ? FILE_FLAG_OVERLAPPED : FILE_ATTRIBUTE_NORMAL, nullptr);
    if (bSync) SemRele();
    return (m_hComm != INVALID_HANDLE_VALUE);
}

VOID UComport::ClosePort(BOOL bSync)
{
    if (bSync) SemSync();
    if ( m_hComm != INVALID_HANDLE_VALUE && m_hComm != NULL ) {
        ::CloseHandle(m_hComm);
    }
    m_hComm = NULL;
    m_bReady = FALSE;
    if (bSync) SemRele();
}

BOOL UComport::SemSync(DWORD wait)
{
    if (!m_bEnableSem) return TRUE;
    return (::WaitForSingleObject(m_hSyncSem, wait) == WAIT_OBJECT_0);
}
VOID UComport::SemRele()
{
    if (!m_bEnableSem) return;
    ReleaseSemaphore(m_hSyncSem, 1, NULL);
}
