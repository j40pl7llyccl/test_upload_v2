#ifndef UCAction_h__
#define UCAction_h__

#include "UCObject.h"
#include "UCActionDecl.h"
#include <vector>

class UCAction : public UCObject
{
protected:
    string                  _strNameOfAction;
    vector<TActionStage*>   _Action;

public:
    UCAction();
    UCAction(const char *pName);
    virtual ~UCAction();

    const char* NamedOf() { return _strNameOfAction.c_str(); }
    void NameOf(const char *pName) { _strNameOfAction = !pName || strlen(pName) <= 0 ? "" : pName; }

    virtual TActionStage* NewStage();
    virtual TActionStage* NewStage(TActionBlockDesc *pBlockDesc, tYUX_BOOL bHandleObj = (tYUX_BOOL)true);
    virtual size_t NumOfStages();
    virtual TActionStage* InsertStage(size_t index);
    virtual tYUX_BOOL RemoveStage(TActionStage *pStage);
    virtual tYUX_BOOL RemoveStage(size_t index);
    virtual tYUX_BOOL ContainStage(TActionStage* pStage);
    virtual void ClearStages();

    virtual tYUX_BOOL AddBlock2Stage(TActionStage *pStage, TActionBlockDesc *pBlockDesc, tYUX_BOOL bHandleObj = (tYUX_BOOL)true);
    virtual tYUX_BOOL AddBlock2NextStage(TActionStage *pCurrStage, TActionBlockDesc *pBlockDesc, tYUX_BOOL bHandleObj = (tYUX_BOOL)true);
    virtual tYUX_BOOL ChangeBlockStage(TActionStage *pNewStage, TActionBlockActivate *pBlock);

    virtual tYUX_BOOL InvokeData();
    virtual tYUX_BOOL ArrangeStages();
};
#endif
