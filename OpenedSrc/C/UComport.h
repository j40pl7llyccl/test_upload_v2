#ifndef __UComport_h__
#define __UComport_h__

#include <Windows.h>

enum ComFlowControl
{
    FC_No,
    FC_CtsRts,
    FC_CtsDtr,
    FC_DsrRts,
    FC_DsrDtr,
    FC_XonXoff
};

enum ComParity
{
    PT_No = 0,
    PT_Odd = 1,
    PT_Even = 2,
    PT_Mark = 3,
    PT_Space = 4
};

enum ComStopBits
{
    SB_One,
    SB_OnePointFive,
    SB_Two
};


class UComport {
public:
    UComport();
    virtual ~UComport();

    virtual BOOL Open(INT32 whichPort, INT32 baudRate, ComParity parity = PT_No, 
        BYTE dataBits = 8, ComStopBits stopBits = SB_One, ComFlowControl flowCtrl = FC_No,
        BOOL overlapped = FALSE);
    virtual VOID Close();

    INT32 PortID() { return m_nComPort; }
    BOOL Read(VOID* pBuff, DWORD nNumToRead, DWORD &nRead);
    BOOL ReadEx(VOID* pBuff, DWORD nNumToRead, DWORD &nRead, LPOVERLAPPED lpOverlapped, LPOVERLAPPED_COMPLETION_ROUTINE lpCompletion);
    BOOL Write(VOID* pBuff, DWORD nNumToWrite, DWORD &nWritten);
    BOOL WriteEx(VOID* pBuff, DWORD nNumToWrite, LPOVERLAPPED lpOverlapped, LPOVERLAPPED_COMPLETION_ROUTINE lpCompletion);

    BOOL TxChar(char c);
    BOOL GetOverlapped(OVERLAPPED &overlapped, DWORD &nTransferred, BOOL wait);
    //BOOL GetOverlappedEx(OVERLAPPED &overlapped, DWORD &nTransferred, DWORD millisec, BOOL bAlertable);
    BOOL CancelIO();
    BOOL CancelIOEx(LPOVERLAPPED lpOverlapped = NULL);
    BOOL BytesWaiting(DWORD &n);
    BOOL ClearBreak();
    BOOL SetBreak();
    BOOL ClearErr(DWORD &dwErr);
    BOOL GetStatus(COMSTAT &stat, DWORD &dwErr);
    BOOL CommState(DCB dcb);
    DCB CommState(BOOL &isOk);

    BOOL ClrDTR() { return Escape(CLRDTR); }
    BOOL ClrRTS() { return Escape(CLRRTS); }
    BOOL SetDTR() { return Escape(SETDTR); }
    BOOL SetRTS() { return Escape(SETRTS); }
    BOOL SetXOFF() { return Escape(SETXOFF); }
    BOOL SetXON() { return Escape(SETXON); }
    BOOL GetProperty(COMMPROP &prop) { if (!m_bReady) return FALSE; return ::GetCommProperties(m_hComm, &prop); }
    BOOL GetModemStat(DWORD &dwStat) { if (!m_bReady) return FALSE; return ::GetCommModemStatus(m_hComm, &dwStat); }
    BOOL SetRWTimeout(DWORD r, DWORD w);
    // Event
    BOOL Mask(DWORD dwM) { if (!m_bReady) return FALSE; return ::SetCommMask(m_hComm, dwM); }
    DWORD Mask() { if (!m_bReady) return FALSE; DWORD m; ::GetCommMask(m_hComm, &m ); return m; }
    BOOL WaitEvt(DWORD &dwMask, LPOVERLAPPED pOverlap = NULL );
    // Queue
    BOOL Flush();
    BOOL TerminateOutstandingWrite();
    BOOL TerminateOutstandingRead();
    BOOL ClrWriteBuff();
    BOOL ClrReadBuff();
    BOOL SetupTxRxQueSize(DWORD nInQ, DWORD nOutQ); 

    void CtrlSemaphore(BOOL enable){ m_bEnableSem = enable; }

private:
    HANDLE m_hComm;
    HANDLE m_hSyncSem;
    BOOL m_bEnableSem;

protected:
    BOOL m_bReady;
    INT32 m_nComPort;

    BOOL OpenPort(LPCSTR pPort, BOOL overlaped = FALSE, BOOL bSync = TRUE);
    VOID ClosePort(BOOL bSync = TRUE);
    BOOL SemSync(DWORD wait = INFINITE );
    VOID SemRele();
    BOOL Escape(DWORD dwFn){ if (!m_bReady) return FALSE; return ::EscapeCommFunction(m_hComm, dwFn); }
    BOOL SetTimeouts(COMMTIMEOUTS &timeouts) { if (!m_bReady) return FALSE; return ::SetCommTimeouts(m_hComm, &timeouts); }
    BOOL GetTimeouts(COMMTIMEOUTS &timeouts) { if (!m_bReady) return FALSE; return ::GetCommTimeouts(m_hComm, &timeouts); }
    BOOL Purge(DWORD flag) { if (!m_bReady) return FALSE; return ::PurgeComm(m_hComm, flag); }
};

#endif
