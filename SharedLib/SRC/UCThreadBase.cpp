#include "stdafx.h"
#include "UCThreadBase.h"

UCThreadBase::UCThreadBase()
    : UCObject()
{
    _strClassRttiName = typeid(*this).name();
    m_bSuspend = 0;
    m_bTerminate = 0;
    m_bDead = 0;
    m_dwWaitTerminatedTm = 0;
}
UCThreadBase::~UCThreadBase()
{
}


