#ifndef UCCommunicationManager_h__
#define UCCommunicationManager_h__

#include "UCObject.h"
#include "UCCommunicationBase.h"
#include <vector>

typedef struct tagUCCommunicationInfo
{
    UCCommunicationBase    *_pCommunication;
    tYUX_BOOL               _bHandleMem;

    tagUCCommunicationInfo()
    {
        _pCommunication = 0;
        _bHandleMem = (tYUX_BOOL)false;
    }
    tagUCCommunicationInfo(UCCommunicationBase *pInst, tYUX_BOOL handleMem)
    {
        _pCommunication = pInst;
        _bHandleMem = handleMem;
    }

    ~tagUCCommunicationInfo()
    {
        if (_pCommunication && _bHandleMem)
            delete _pCommunication;
        _pCommunication = 0;
    }
}TUCCommunicationInfo;

class UCCommunicationManager : public UCObject
{
private:
    std::vector<TUCCommunicationInfo*> _Instances;

public:
    UCCommunicationManager();
    virtual ~UCCommunicationManager();

    void Clear();
    UCCommunicationBase* Get(tYUX_I8 *pName);
    // if the pInst's name was existing, it will be replaced by new one.
    UCCommunicationBase* Create(UCCommunicationBase *pInst, tYUX_BOOL bHandledByThisClass = (tYUX_BOOL)true);

};

#endif
