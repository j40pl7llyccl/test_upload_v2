#ifndef UCRTTIClassInst_h__
#define UCRTTIClassInst_h__

#include "UCObject.h"
#include "UBaseTypeDecl.h"
#include <typeinfo>
#include <string>

class UCRTTIClassInst : public UCObject
{
protected:
    std::string _strClassTypeName;
    tYUX_PVOID  _pInstance;

public:
    UCRTTIClassInst() : UCObject()
    {
        _pInstance = 0;
        _strClassRttiName = typeid(*this).name();
    }
    UCRTTIClassInst(tYUX_PVOID pInst) : UCObject()
    {
        _pInstance = pInst;
        _strClassRttiName = typeid(*this).name();
    }
    virtual ~UCRTTIClassInst() {}

    template<class T>
    void Set(T* pInst)
    {
        _strClassTypeName = typeid(T).name();
        _pInstance = (tYUX_PVOID)pInst;
    }
    tYUX_PVOID Ptr() { return _pInstance; }

    const char* TypeName() { return _strClassTypeName.c_str(); }
    void TypeName(const char *pStr)
    {
        _strClassTypeName = !pStr || strlen(pStr) <= 0 ? "" : pStr;
    }

    template<class T>
    T* Convert()
    {
        const char *pName = typeid(T).name();
        return pName == _strClassTypeName ? static_cast<T*>(_pInstance) : (T*)(0);
    }

    UCRTTIClassInst& operator=(UCRTTIClassInst &right)
    {
        if (&right == this)
            return *this;

        const char *pRightName = right.TypeName();
        this->TypeName(pRightName);
        this->_pInstance = right.Ptr();

        return *this;
    }

};

#endif
