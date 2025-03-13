#ifndef TStateMachineTimeoutObj_h__
#define TStateMachineTimeoutObj_h__

#include "windows.h"

typedef struct tStateMachineTimeoutObj
{
    LARGE_INTEGER   _nFreq;
    LARGE_INTEGER   _nBeg;
    double          _dfTimeout;

    tStateMachineTimeoutObj()
    {
        ::QueryPerformanceFrequency(&_nFreq);
        ::QueryPerformanceCounter(&_nBeg);
        _dfTimeout = 0.;
    }

    tStateMachineTimeoutObj(double dfTimeoutMs)
    {
        ::QueryPerformanceFrequency(&_nFreq);
        ::QueryPerformanceCounter(&_nBeg);
        _dfTimeout = dfTimeoutMs;
    }

    BOOL TimeoutMs()
    {
        LARGE_INTEGER curr;
        ::QueryPerformanceCounter(&curr);

        double diff = ((double)(curr.QuadPart - _nBeg.QuadPart) / (double)_nFreq.QuadPart) * 1000.;
        return (diff > _dfTimeout);
    }
    void ResetBeg()
    {
        ::QueryPerformanceCounter(&_nBeg);
    }
}TStateMachineTimeoutObj;


#endif
