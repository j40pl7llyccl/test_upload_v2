#ifndef UCBlockDataIO_h__
#define UCBlockDataIO_h__

#include "UCBlockDef.h"

struct TUCBlockOChaining;
union BlockInputChainingData;
typedef union BlockInputChainingData(*fpUBlockOutputNextInputDat)(TUCBlockOChaining *pCurr, tYUX_I32 reqType/* UCBLOCKICHAIN_DATATYPE */);

enum UCBLOCKOCHAIN_DATATYPE
{
    UCBLOCKOCHAIN_SHAREDMEMITEM = 0,
    UCBLOCKOCHAIN_USERDEFMEM,
    UCBLOCKOCHAIN_STRING,
};
struct TUCBlockOChaining
{
    tYUX_I32    _nDataType; // UCBLOCKOCHAIN_DATATYPE
    const char *_pTypeName; // basic type name, from typeid(T).name()
    const char *_pDescription; // description the data
    union ChainingData {
        struct TFromFormattingSharedMem {
            tYUX_I32    _nDataType;   // tUCSHAREDMEM_TYPE
            tYUX_I64    _nDataCount;  // data count
            tYUX_PVOID  _pDataAddr;   // export shared memory
            tYUX_PVOID  _pHandleOfSharedMemItem; // an item from shared
        } _FormattingSharedMemItem;
        struct TUserDefMem {
            tYUX_I32    _nSizeofData; // sizeof data type
            tYUX_I64    _nNumOfData;  // number of data count
            tYUX_PVOID  _pDataAddr;   // memory to store data, just refer to an address
            tYUX_PVOID  _pBlockInstance;
        } _UserDefMem;
        struct TUserDefString {
            char       *_pString;
        }_String;
    } _Data;
    void ResetDat()
    {
        size_t sz = sizeof(ChainingData);
        tYUX_U8 *pU8 = (tYUX_U8 *)&this->_Data;
        for (size_t i = 0; i < sz; i++)pU8[i] = 0;
    }

    TUCBlockOChaining()
    {
        _nDataType = -1;
        _pTypeName = 0;
        _pDescription = 0;
        ResetDat();
    }
};

enum UCBLOCKICHAIN_DATATYPE
{
    UCBLOCKICHAIN_KNOWNTYPE = 0,
    UCBLOCKICHAIN_STRINGTYPE,
};
union BlockInputChainingData {
    // known data type
    struct TFlexibleData {
        tYUX_PVOID          _pMem;
        fpUCBlockHandleMem  _fpHandleMem;
        tYUX_I32            _nSizeof;
        tYUX_I64            _nNumof;
    } _FlexMem;
    // if data type cannot be defined in known type, using string to carry.
    //  - json
    //  - xml
    //  - ...
    struct TByteString {
        char               *_pString;
        fpUCBlockHandleMem  _fpHandleStr;
    } _StrMem;
};
struct TUCBlockIChaining
{
    tYUX_I32    _nDataType;     // type of data: using which struct, UCBLOCKICHAIN_DATATYPE
    const char *_pWhichBlockId; // from which block
    const char *_pOutputIdName; // use which data of a block
    const char *_pRequestType;  // name of type, typeid(T).name()
    union BlockInputChainingData _Data;
    fpUBlockOutputNextInputDat  _fpOutDat; // keep flexible, this maybe from dll, runtime bindinb from specified block's output
    void ResetData()
    {
        size_t sz = sizeof(BlockInputChainingData);
        tYUX_U8 *pU8 = (tYUX_U8 *)&this->_Data;
        for (size_t i = 0; i < sz; i++)pU8[i] = 0;
    }

    TUCBlockIChaining()
    {
        _nDataType = UCBLOCKICHAIN_KNOWNTYPE;
        _pWhichBlockId = 0;
        _pOutputIdName = 0;
        _pRequestType = 0;

        ResetData();
    }

    ~TUCBlockIChaining()
    {
        if (_nDataType == UCBLOCKICHAIN_KNOWNTYPE)
        {
            if (_Data._FlexMem._fpHandleMem && _Data._FlexMem._pMem)
                _Data._FlexMem._fpHandleMem(_Data._FlexMem._pMem);
        }
        else if (_nDataType == UCBLOCKICHAIN_STRINGTYPE)
        {
            if (_Data._StrMem._pString && _Data._StrMem._fpHandleStr)
                _Data._StrMem._fpHandleStr(_Data._StrMem._pString);
        }
        else
        {
            printf("[TUCBlockIChaining::~TUCBlockIChaining()] don't know how to process the type of data!\n");
            //throw new std::exception("[TUCBlockIChaining::~TUCBlockIChaining()] invalid data type!");
        }
    }
};

#endif
