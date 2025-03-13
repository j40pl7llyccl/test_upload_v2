#ifndef UCThreadBase_h__
#define UCThreadBase_h__

#include "UCObject.h"
///////////////////////////////////////////////////////////////////////////////
enum UTHREAD_EXEC_CODE
{
    eUEXEC_CODE_CONTEXT_PARAM_ERR = 1,
    eUEXEC_CODE_IMMEDIATE_TERMINATE = 2,
    eUEXEC_CODE_CATCH_ERR = 3,
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
class UCThreadBase : public UCObject
{

protected:
    tYUX_BOOL   m_bSuspend;
    tYUX_BOOL   m_bTerminate;
    tYUX_BOOL   m_bDead;
    tYUX_U32    m_dwWaitTerminatedTm;

public:
    UCThreadBase();
    virtual ~UCThreadBase();

    tYUX_BOOL IsTerminated() { return  m_bTerminate; }
    tYUX_BOOL IsSuspended()  { return  m_bSuspend; }
    tYUX_BOOL IsDead()       { return  m_bDead; }
    void SetWaitTerminatedTm( tYUX_U32 ms ) { m_dwWaitTerminatedTm = ms; }

    virtual tYUX_BOOL Create( tYUX_BOOL createSuspend ) = 0;
    virtual tYUX_BOOL Suspend( void ) = 0;
    virtual tYUX_BOOL Resume( void ) = 0;
    virtual void Terminate( void ) = 0;
    virtual void Destroy() = 0;
    virtual void Execute( void ) = 0;
    virtual void SetPriority( int priority ) = 0;

};

#endif
