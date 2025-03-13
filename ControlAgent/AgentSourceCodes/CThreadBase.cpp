#include "stdafx.h"
#include "CThreadBase.h"
#include "tchar.h"

///////////////////////////////////////////////////////////////////////////////
unsigned int WINAPI CThreadBase::ThreadProcFunc( void * lpParameter )
{
    // Don't know how to exec.
    if ( ! lpParameter )
        return eEXEC_CODE_CONTEXT_PARAM_ERR;

    CThreadBase *pObj = static_cast< CThreadBase * > ( lpParameter );

    if ( pObj->m_bTerminate )
    {
//        ::SetEvent( pObj->m_hFuncEndEvt );
        pObj->m_bDead = TRUE;
        return eEXEC_CODE_IMMEDIATE_TERMINATE;
    }

    try
    {
        pObj->Execute();
    }
    catch( ... )
    {
//        ::SetEvent( pObj->m_hFuncEndEvt );
        pObj->m_bDead = TRUE;
        _endthreadex( eEXEC_CODE_CATCH_ERR );
        //::ExitThread( -3 );
    }

//    ::SetEvent( pObj->m_hFuncEndEvt );
    pObj->m_bDead = TRUE;
    return 0;
}

//*****************************************************************************
CThreadBase::CThreadBase()
{
    m_hHandle = NULL;
    m_dwID    = 0;

    m_bTerminate = FALSE;
    m_bSuspend   = FALSE;
    m_bDead      = FALSE;

    m_dwWaitTerminatedTm = 1000 * 10;
//    m_hFuncEndEvt = ::CreateEvent( NULL, TRUE, FALSE, 0 );
}

//*****************************************************************************
CThreadBase::~CThreadBase()
{
    if ( ! m_hHandle )
    {
//        if ( m_hFuncEndEvt ) ::CloseHandle( m_hFuncEndEvt );
        return;
    }

    m_bTerminate = TRUE;

    // Ever into suspend mode.
    if ( m_bSuspend )
        ::ResumeThread( m_hHandle );

    // Wait to end
    //::WaitForSingleObject( m_hFuncEndEvt, INFINITE );
    DWORD status = ::WaitForSingleObject( m_hHandle, m_dwWaitTerminatedTm );
    if ( status == WAIT_TIMEOUT )
    {
        ::MessageBox( NULL, _T( "Wait Thread End Timeout!" ), _T( "Info" ), MB_OK );
    }

    // Close Handle
//    ::CloseHandle( m_hFuncEndEvt );
    ::CloseHandle( m_hHandle );
    m_bDead = TRUE;
}


//*****************************************************************************
BOOL CThreadBase::Create( BOOL createSuspend )
{
    if ( m_hHandle ) return TRUE;

    m_bSuspend = createSuspend;
//    m_hHandle = ::CreateThread( NULL,   // Default security
//                                0,      // Default stack, 1MB
//                                ThreadProcFunc,
//                                ( LPVOID ) this,
//                                createSuspend ? CREATE_SUSPENDED : 0,
//                                & m_dwID );

    m_hHandle = ( HANDLE ) _beginthreadex( NULL,
                                           0,
                                           ThreadProcFunc,
                                           ( void * ) this,
                                           createSuspend ? CREATE_SUSPENDED : 0,
                                           & m_dwID );

    return ( m_hHandle ? TRUE : FALSE );
}

//*****************************************************************************
BOOL CThreadBase::Suspend( void )
{
    if ( ! m_hHandle ) return FALSE;
    if ( m_bTerminate || m_bDead ) return FALSE;
    if ( m_bSuspend ) return TRUE;

    m_bSuspend = TRUE;
    ::SuspendThread( m_hHandle );
    return TRUE;
}

//*****************************************************************************
BOOL CThreadBase::Resume( void )
{
    if ( ! m_hHandle ) return FALSE;
    if ( ! m_bSuspend ) return TRUE;

    ::ResumeThread( m_hHandle );
    m_bSuspend = FALSE;
    return TRUE;
}

//*****************************************************************************
void CThreadBase::Terminate( void )
{
    if ( ! m_hHandle || m_bDead ) return;
    if ( m_bTerminate ) return;

    m_bTerminate = TRUE;
}

//*****************************************************************************
void CThreadBase::Destroy()
{
    if ( ! m_hHandle || m_bDead || m_bTerminate ) return;

    m_bTerminate = TRUE;

    // Ever into suspend mode.
    if ( m_bSuspend )
        ::ResumeThread( m_hHandle );

    // Wait to end
    DWORD status = ::WaitForSingleObject( m_hHandle, m_dwWaitTerminatedTm );
    if ( status == WAIT_TIMEOUT )
    {
        ::MessageBox( NULL, _T( "Wait Thread End Timeout!" ), _T( "Info" ), MB_OK );
    }

    // Close Handle
    ::CloseHandle( m_hHandle );
    m_hHandle = NULL;

    m_bDead = TRUE;
}

//*****************************************************************************
void CThreadBase::Execute( void )
{
    return; // Do nothing.
}

//*****************************************************************************
void CThreadBase::SetPriority( int priority )
{
    if ( ! m_hHandle ) return;

    switch( priority )
    {
//        case THREAD_MODE_BACKGROUND_BEGIN:
//        case THREAD_MODE_BACKGROUND_END:
        case THREAD_PRIORITY_ABOVE_NORMAL:
        case THREAD_PRIORITY_BELOW_NORMAL:
        case THREAD_PRIORITY_HIGHEST:
        case THREAD_PRIORITY_IDLE:
        case THREAD_PRIORITY_LOWEST:
        case THREAD_PRIORITY_NORMAL:
        case THREAD_PRIORITY_TIME_CRITICAL:
            break;
        default:
            return;
    }

    ::SetThreadPriority( m_hHandle, priority );
}

//*****************************************************************************
void CThreadBase::SetWaitTerminatedTm( DWORD ms )
{
    m_dwWaitTerminatedTm = ms;
}
