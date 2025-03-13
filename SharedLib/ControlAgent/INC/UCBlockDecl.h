#ifndef UCBlockDecl_h__
#define UCBlockDecl_h__

#include "UCBlockDef.h"

typedef struct tUCBlockMutableParam
{
    tYUX_I32            _nUnit;
    tYUX_I64            _nSize;
    tYUX_PVOID          _pData;
    fpUCBlockHandleMem  _fpMemHandler;

    tUCBlockMutableParam()
    {
        _nUnit = 0;
        _nSize = 0;
        _pData = (tYUX_PVOID)0;
        _fpMemHandler = (fpUCBlockHandleMem)0;
    }
    ~tUCBlockMutableParam()
    {
        if (_fpMemHandler && _pData) _fpMemHandler(_pData);
        _pData = (tYUX_PVOID)0;
        _fpMemHandler = (fpUCBlockHandleMem)0;
    }
}TUCBlockMutableParam;

#endif
