#ifndef UCActionManager_h__
#define UCActionManager_h__

#include "UCObject.h"
#include "UCActionDecl.h"

class UCActionManager : public UCObject
{
protected:

    vector<TActionStage *> _BlocksOfLevel;
    // TODO: 
    //  - add block dynamically
    //  - remove dynamical block

public:
    UCActionManager() : UCObject() {}
    virtual ~UCActionManager() {}
};

#endif
