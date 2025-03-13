#ifndef UCThreadWin32_h__
#define UCThreadWin32_h__

#include <windows.h>
#include <process.h>
#include "UCThreadBase.h"

class UCThreadWin32 : public UCThreadBase
{
private:
    static unsigned int WINAPI ThreadProcFunc( void * lpParameter );

protected:
    HANDLE          m_hHandle;
    unsigned        m_dwID;
    int             m_iPriority;

public:
    UCThreadWin32();
    virtual ~UCThreadWin32();

    virtual tYUX_BOOL Create( tYUX_BOOL createSuspend );
    virtual tYUX_BOOL Suspend( void );
    virtual tYUX_BOOL Resume( void );
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
