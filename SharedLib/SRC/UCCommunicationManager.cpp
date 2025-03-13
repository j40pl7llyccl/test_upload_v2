#include "stdafx.h"
#include "UCCommunicationManager.h"

UCCommunicationManager::UCCommunicationManager()
    : UCObject()
{
    _strClassRttiName = typeid(*this).name();
}
UCCommunicationManager::~UCCommunicationManager()
{
    Clear();
}

void UCCommunicationManager::Clear()
{
    std::vector<TUCCommunicationInfo*>::iterator it;
    for (it = _Instances.begin(); it != _Instances.end(); it++)
    {
        TUCCommunicationInfo *pInfo = (TUCCommunicationInfo *)(*it);
        delete pInfo;
    }
    _Instances.clear();
}

UCCommunicationBase* UCCommunicationManager::Get(tYUX_I8 *pName)
{
    if (_Instances.size() <= 0 || !pName || strlen(pName) <= 0)
        return 0;

    for (int i = 0; i < (int)_Instances.size(); i++)
    {
        TUCCommunicationInfo *pInfo = _Instances[i];
        if (!pInfo->_pCommunication)
            continue;

        if (!strcmp(pInfo->_pCommunication->ID(), pName))
            return pInfo->_pCommunication;
    }

    return 0;
}
UCCommunicationBase* UCCommunicationManager::Create(UCCommunicationBase *pInst, tYUX_BOOL bHandledByThisClass)
{
    if (!pInst)
        return 0;

    if (pInst && (!pInst->ID() || strlen(pInst->ID()) <= 0))
    {
        if (bHandledByThisClass)
            delete pInst;
        return 0;
    }

    std::vector<TUCCommunicationInfo *>::iterator it;
    for (it = _Instances.begin(); it != _Instances.end(); it++)
    {
        TUCCommunicationInfo *pInfo = (TUCCommunicationInfo *)(*it);
        if (!pInfo->_pCommunication)
            continue;

        if (!strcmp(pInfo->_pCommunication->ID(), pInst->ID()))
        {
            delete pInfo;

            pInfo = new TUCCommunicationInfo(pInst, bHandledByThisClass);
            *it = pInfo;
            return pInst;
        }
    }

    TUCCommunicationInfo *p = new TUCCommunicationInfo(pInst, bHandledByThisClass);
    _Instances.push_back(p);
    return pInst;
}


