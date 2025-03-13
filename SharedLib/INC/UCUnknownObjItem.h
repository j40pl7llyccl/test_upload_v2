#ifndef __UCUnknownObjItem_h__
#define __UCUnknownObjItem_h__

#include <string>

//*****************************************************************************
// Creator Base Class
//*****************************************************************************
class UCUnKnownBase
{
protected:
    std::string m_strName;

public:
    UCUnKnownBase() {}
    virtual ~UCUnKnownBase(){}

    const char* GetName() { return m_strName.c_str(); }
    void SetName( char *pStr )
    {
        if (pStr && strlen(pStr) > 0) m_strName = pStr;
        else m_strName = "";
    }

    virtual bool Allocate( void **ppObj ) = 0;
    virtual bool Free( void **ppObj ) = 0;

};

//*****************************************************************************
// Creator Template Class
//*****************************************************************************
template< class T >
class UCUnknownObjItem : public UCUnKnownBase
{
public:
    UCUnknownObjItem() {}
    UCUnknownObjItem( char *pName ) { SetName( pName ); }
    ~UCUnknownObjItem() {}

    bool Allocate( void **ppObj )
    {
        if ( ! ppObj )
            return false;
        T *p = new T(); // Must have default constructor
        if ( ! p )
            return false;
        *ppObj = ( void * ) p;
        return true;
    }
    bool Free( void **ppObj )
    {
        if ( ! ppObj || ! *ppObj )
            return false;
        T *p = ( T * ) *ppObj; // Convert to class type "T" and call delete
        delete p;
        *ppObj = NULL;
        return true;
    }
};



#endif
