#ifndef UTypeOfDataSyncDecl_h__
#define UTypeOfDataSyncDecl_h__

#include "UCDataSyncW32.h"

typedef UCDataSyncW32<char>     CUByteSharedMem;
typedef UCDataSyncW32<float>    CUFloatSharedMem;
typedef UCDataSyncW32<double>   CUDoubleSharedMem;
typedef UCDataSyncW32<INT32>    CUInt32SharedMem;
typedef UCDataSyncW32<UINT32>   CUUInt32SharedMem;
typedef UCDataSyncW32<tYUX_I64> CUInt64SharedMem;

#endif
