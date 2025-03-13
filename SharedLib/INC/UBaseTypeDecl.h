#ifndef UBaseTypeDecl_h__
#define UBaseTypeDecl_h__

#define UBASE_SUPPORT_TYPE_64
#ifdef UBASE_NOT_SUPPORT_TYPE_64
#undef UBASE_SUPPORT_TYPE_64
#endif

typedef char                tYUX_I8;
typedef unsigned char       tYUX_U8;
typedef short               tYUX_I16;
typedef unsigned short      tYUX_U16;
typedef int                 tYUX_I32;
typedef unsigned int        tYUX_U32;
typedef int                 tYUX_BOOL;
typedef void*               tYUX_PVOID;
#ifdef UBASE_SUPPORT_TYPE_64
typedef long long           tYUX_I64;
typedef unsigned long long  tYUX_U64;
#else
typedef long           tYUX_I64;
typedef unsigned long  tYUX_U64;
#endif

#define UBASE_ARR_COUNT(arr)    (sizeof(arr) == 0 ? 0 : sizeof(arr) / sizeof(arr[0]))

#endif
