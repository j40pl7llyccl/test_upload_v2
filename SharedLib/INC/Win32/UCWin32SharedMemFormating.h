#ifndef UCWin32SharedMemFormating_h__
#define UCWin32SharedMemFormating_h__

#include "UCSharedMemFormating.h"
#include "UUtilities.h"

class UCWin32SharedMemFormating : public UCSharedMemFormating
{
public:
    UCWin32SharedMemFormating();
    virtual ~UCWin32SharedMemFormating();

    tYUX_BOOL Initialize(char *pMainSectionName, int nNameMax, int nItemCountMax);

    tYUX_BOOL CreateI32ShMem(char *pName, int nCount, tYUX_PVOID &ptrHandle);
    tYUX_BOOL CreateDoubleShMem(char *pName, int nCount, tYUX_PVOID &ptrHandle);
    tYUX_BOOL CreateU8ShMem(char *pName, tYUX_I64 nCount, tYUX_PVOID &ptrHandle);
    tYUX_BOOL DeleteShMem(char *pName);

    tYUX_BOOL AddI32(tYUX_PVOID hOfCreated, int nNeededItems, tYUX_PVOID &itemHandle);
    tYUX_I32* GetI32(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux);
    tYUX_BOOL AccessI32(tYUX_PVOID itemHandle, tYUX_BOOL bRead, tYUX_I32 *pDat, tYUX_I32 nDat, tYUX_BOOL bSync = (tYUX_BOOL)true);

    tYUX_BOOL AddDouble(tYUX_PVOID hOfCreated, int nNeededItems, tYUX_PVOID &itemHandle);
    double* GetDouble(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux);
    tYUX_BOOL AccessDouble(tYUX_PVOID itemHandle, tYUX_BOOL bRead, double *pDat, tYUX_I32 nDat, tYUX_BOOL bSync = (tYUX_BOOL)true);

    tYUX_BOOL AddU8(tYUX_PVOID hOfCreated, tYUX_I64 nNeededItems, tYUX_PVOID &itemHandle);
    tYUX_U8* GetU8(tYUX_PVOID itemHandle, int &nCount, tYUX_PVOID &mux);
    tYUX_BOOL AccessU8(tYUX_PVOID itemHandle, tYUX_BOOL bRead, tYUX_U8 *pDat, tYUX_I64 nDat, tYUX_BOOL bSync = (tYUX_BOOL)true);

private:
    TShMemUsedInfo* GetShMemInfo(char *pName);
    tYUX_BOOL ShMemHandleValid(tYUX_PVOID h);
    tYUX_BOOL ItemHandleValid(tYUX_PVOID h);
};


#endif
