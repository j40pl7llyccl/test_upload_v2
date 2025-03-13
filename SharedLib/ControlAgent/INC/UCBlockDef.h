#ifndef UCBlockDef_h__
#define UCBlockDef_h__

#include "UBaseTypeDecl.h"

typedef void(*fpUCBlockLog)(char *pStr, int level);
typedef void(*fpUCBlockHandleMem)(tYUX_PVOID pMem);

enum UCBlockBaseRet
{
    RET_STATUS_NG = -1,
    RET_STATUS_OK = 0,
    RET_STATUS_RUNNING,
    RET_STATUS_PAUSED,
    RET_STATUS_PAUSE_RELEASED,
};

// States reserved
#define UCBLOCK_STATE_ERROR        -2 // MUST HAVE: error happened
#define UCBLOCK_STATE_NA           -1
#define UCBLOCK_STATE_PREPARING     0 // MUST HAVE: prepare to exec
#define UCBLOCK_STATE_FINISH        1 // MUST HAVE: normal done
#define UCBLOCK_STATE_PAUSING       2 // MUST HAVE: pausing
#define UCBLOCK_STATE_PAUSE_RELEASE 3 // ABANDONED: pause released
#define UCBLOCK_STATE_STOP          4 // MUST HAVE: stop
#define UCBLOCK_STATE_DUMMY_RUN     5 // Alternative

#endif
