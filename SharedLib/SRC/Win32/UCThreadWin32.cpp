#include "stdafx.h"
#include "UCThreadWin32.h"
#include "tchar.h"

///////////////////////////////////////////////////////////////////////////////
unsigned int WINAPI UCThreadWin32::ThreadProcFunc( void * lpParameter )
{
    // Don't know how to exec.
    if ( ! lpParameter )
        return eUEXEC_CODE_CONTEXT_PARAM_ERR;

    UCThreadWin32 *pObj = static_cast< UCThreadWin32 * > ( lpParameter );

    if ( pObj->m_bTerminate )
    {
//        ::SetEvent( pObj->m_hFuncEndEvt );
        pObj->m_bDead = TRUE;
        return eUEXEC_CODE_IMMEDIATE_TERMINATE;
    }

    try
    {
        pObj->Execute();
    }
    catch( ... )
    {
//        ::SetEvent( pObj->m_hFuncEndEvt );
        pObj->m_bDead = TRUE;
        _endthreadex( eUEXEC_CODE_CATCH_ERR );
        //::ExitThread( -3 );
    }

//    ::SetEvent( pObj->m_hFuncEndEvt );
    pObj->m_bDead = TRUE;
    return 0;
}

//*****************************************************************************
UCThreadWin32::UCThreadWin32()
    : UCThreadBase()
{
    _strClassRttiName = typeid(*this).name();
    m_hHandle = NULL;
    m_dwID    = 0;

    m_dwWaitTerminatedTm = 1000 * 10;
//    m_hFuncEndEvt = ::CreateEvent( NULL, TRUE, FALSE, 0 );
}

//*****************************************************************************
UCThreadWin32::~UCThreadWin32()
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
tYUX_BOOL UCThreadWin32::Create( tYUX_BOOL createSuspend )
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
tYUX_BOOL UCThreadWin32::Suspend( void )
{
    if ( ! m_hHandle ) return FALSE;
    if ( m_bTerminate || m_bDead ) return FALSE;
    if ( m_bSuspend ) return TRUE;

    m_bSuspend = TRUE;
    ::SuspendThread( m_hHandle );
    return TRUE;
}

//*****************************************************************************
tYUX_BOOL UCThreadWin32::Resume( void )
{
    if ( ! m_hHandle ) return FALSE;
    if ( ! m_bSuspend ) return TRUE;

    ::ResumeThread( m_hHandle );
    m_bSuspend = FALSE;
    return TRUE;
}

//*****************************************************************************
void UCThreadWin32::Terminate( void )
{
    if ( ! m_hHandle || m_bDead ) return;
    if ( m_bTerminate ) return;

    m_bTerminate = TRUE;
}

//*****************************************************************************
void UCThreadWin32::Destroy()
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
void UCThreadWin32::Execute( void )
{
    return; // Do nothing.
}

//*****************************************************************************
void UCThreadWin32::SetPriority( int priority )
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
