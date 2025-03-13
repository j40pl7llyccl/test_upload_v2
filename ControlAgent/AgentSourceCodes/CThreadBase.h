#ifndef CThreadBase_h__
#define CThreadBase_h__

#include <windows.h>
#include <process.h>

///////////////////////////////////////////////////////////////////////////////
enum THREAD_EXEC_CODE
{
    eEXEC_CODE_CONTEXT_PARAM_ERR = 1,
    eEXEC_CODE_IMMEDIATE_TERMINATE = 2,
    eEXEC_CODE_CATCH_ERR = 3,
};

///////////////////////////////////////////////////////////////////////////////
// End the thread
//  ( I )
//  1. Call Terminate() : notify to terminate
//      - base class: set m_bTerminate flag.
//      - child class: prepare to terminate the thread ( send event... ), and 
//                     set m_bTerminate flag.
//  2. Call destructor
//  ( II )
//  - Call from Destroy() to wait thread end.
///////////////////////////////////////////////////////////////////////////////
class CThreadBase
{
private:
    static unsigned int WINAPI ThreadProcFunc( void * lpParameter );

protected:
//    HANDLE          m_hFuncEndEvt;
    HANDLE          m_hHandle;
    unsigned        m_dwID;
    int             m_iPriority;

    BOOL            m_bSuspend;
    BOOL            m_bTerminate;
    BOOL            m_bDead;

    DWORD           m_dwWaitTerminatedTm;

public:
    CThreadBase();
    virtual ~CThreadBase();

    BOOL IsTerminated() { return  m_bTerminate; }
    BOOL IsSuspended()  { return  m_bSuspend; }
    BOOL IsDead()       { return  m_bDead; }

    void SetWaitTerminatedTm( DWORD ms );

    virtual BOOL Create( BOOL createSuspend );
    virtual BOOL Suspend( void );
    virtual BOOL Resume( void );
    virtual void Terminate( void );
    virtual void Destroy();

    virtual void Execute( void ); // A thread will exec this function.
    // THREAD_MODE_BACKGROUND_BEGIN
    // THREAD_MODE_BACKGROUND_END
    // THREAD_PRIORITY_ABOVE_NORMAL
    // THREAD_PRIORITY_BELOW_NORMAL
    // THREAD_PRIORITY_HIGHEST
    // THREAD_PRIORITY_IDLE
    // THREAD_PRIORITY_LOWEST
    // THREAD_PRIORITY_NORMAL
    // THREAD_PRIORITY_TIME_CRITICAL
    void SetPriority( int priority );

};

#endif
