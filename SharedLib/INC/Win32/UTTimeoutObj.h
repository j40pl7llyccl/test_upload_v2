#ifndef UTTimeoutObj_h__
#define UTTimeoutObj_h__

#include <windows.h>

typedef struct tUTimeoutObj
{
    LARGE_INTEGER   _nFreq;
    LARGE_INTEGER   _nBeg;
    double          _dfTimeout;

    tUTimeoutObj()
    {
        ::QueryPerformanceFrequency(&_nFreq);
        ::QueryPerformanceCounter(&_nBeg);
        _dfTimeout = 0.;
    }

    tUTimeoutObj(double dfTimeoutMs)
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
    
    double DiffTime()
    {
        LARGE_INTEGER curr;
        ::QueryPerformanceCounter(&curr);

        double diff = ((double)(curr.QuadPart - _nBeg.QuadPart) / (double)_nFreq.QuadPart) * 1000.;
        return diff;
    }
}UTTimeoutObj;


#endif
