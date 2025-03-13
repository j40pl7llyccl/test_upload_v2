#ifndef UCObject_h__
#define UCObject_h__

#include "UBaseTypeDecl.h"
#include <typeinfo>
#include <string>

using namespace std;

class UCObject
{
protected:
    string _strClassRttiName;

public:
    UCObject() {}
    virtual ~UCObject() {}

    const char* TypeName() { return _strClassRttiName.c_str(); }
};

#endif
