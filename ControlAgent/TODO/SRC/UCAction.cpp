#include "stdafx.h"
#include "UCAction.h"

UCAction::UCAction() : UCObject()
{

}
UCAction::UCAction(const char *pName)
{
    _strNameOfAction = !pName || strlen(pName) <= 0 ? "" : pName;
}
UCAction::~UCAction()
{
    vector<TActionStage*>::iterator it;
    while (_Action.size() > 0)
    {
        it = _Action.begin();
        TActionStage *p = (TActionStage*)(*it);
        if (p)
            delete p;
        _Action.erase(it);
    }
}

TActionStage* UCAction::NewStage()
{
    TActionStage *p = new TActionStage();
    if (!p)
        return (TActionStage*)(0);

    _Action.push_back(p);

    return p;
}
TActionStage* UCAction::NewStage(TActionBlockDesc *pBlockDesc, tYUX_BOOL bHandleObj)
{
    TActionStage *p = new TActionStage();
    if (!p)
        return (TActionStage*)(0);

    TActionBlockActivate *pBlock = new TActionBlockActivate(pBlockDesc, bHandleObj);
    if (!pBlock)
    {
        delete p;
        return (TActionStage*)(0);
    }

    //pBlock->_pCurrentLevel = p;
    //pBlock->_pPreviousLevel = _Action.size() <= 0 ? NULL : _Action[_Action.size() - 1];

    _Action.push_back(p);
    return p;

}
size_t UCAction::NumOfStages()
{
    return _Action.size();
}
TActionStage* UCAction::InsertStage(size_t index)
{
    size_t maxAccPos = _Action.end() - _Action.begin();
    if (index > maxAccPos)
        return (TActionStage*)(0);

    TActionStage *p = new TActionStage();
    if (!p)
        return (TActionStage*)(0);
    _Action.insert(_Action.begin() + index, p);
    return p;
}
tYUX_BOOL UCAction::RemoveStage(TActionStage *pStage)
{
    tYUX_BOOL ret = (tYUX_BOOL)false;
    vector<TActionStage*>::iterator it;
    for (it = _Action.begin(); it != _Action.end(); it++)
    {
        TActionStage *pConv = (TActionStage*)(*it);
        if (pConv == pStage) {
            delete pConv; // handle the mem
            _Action.erase(it); // erase the item from vector
            ret = (tYUX_BOOL)true;
            break;
        }
    }

    return ret;
}
tYUX_BOOL UCAction::RemoveStage(size_t index)
{
    tYUX_BOOL ret = (tYUX_BOOL)false;
    size_t sz = _Action.size();
    if (index >= sz)
        return ret;

    vector<TActionStage*>::iterator it = _Action.begin() + index;
    TActionStage *pConv = (TActionStage*)(*it);

    delete pConv;
    _Action.erase(it);

    return (tYUX_BOOL)true;
}
void UCAction::ClearStages()
{
    vector<TActionStage*>::iterator it;
    while (_Action.size() > 0) {
        it = _Action.begin();
        TActionStage *p = (TActionStage*)(*it);
        if (p)
            delete p;
        _Action.erase(it);
    }
}

tYUX_BOOL UCAction::AddBlock2Stage(TActionStage *pStage, TActionBlockDesc *pBlockDesc, tYUX_BOOL bHandleObj)
{

    return (tYUX_BOOL)false;
}
tYUX_BOOL UCAction::AddBlock2NextStage(TActionStage *pCurrStage, TActionBlockDesc *pBlockDesc, tYUX_BOOL bHandleObj)
{

    return (tYUX_BOOL)false;
}
tYUX_BOOL UCAction::ChangeBlockStage(TActionStage *pNewStage, TActionBlockActivate *pBlock)
{

    return (tYUX_BOOL)false;
}

tYUX_BOOL UCAction::InvokeData()
{
    return (tYUX_BOOL)false;
}
tYUX_BOOL UCAction::ArrangeStages()
{

    return (tYUX_BOOL)false;
}
