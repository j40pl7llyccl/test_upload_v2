#ifndef TypeOfDataSyncDecl_h__
#define TypeOfDataSyncDecl_h__

#include "DataSync.h"

typedef CDataSync<float> CFloatSharedMem;
typedef CDataSync<double> CDoubleSharedMem;
typedef CDataSync<INT32> CInt32SharedMem;
typedef CDataSync<UINT32> CUInt32SharedMem;

#endif
