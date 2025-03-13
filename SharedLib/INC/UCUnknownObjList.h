#ifndef __UCUnknownObjList_h__
#define __UCUnknownObjList_h__

#include "UCUnknownObjItem.h"
#include <list>

//*****************************************************************************
// Item information
//*****************************************************************************
typedef struct tagTUnknownObjItem
{
    UCUnKnownBase  *_pObject;
    bool            _bHandleIt;
    void           *_pObjID;
}TUnknownObjItem;

//*****************************************************************************
// List
//*****************************************************************************
class UCUnknownObjList
{
protected:
    bool                            m_bValid;
    std::list< TUnknownObjItem* >  *m_plistObj;

protected:
    void FreeAll()
    {
        if ( ! m_bValid )
            return;
        m_bValid = false;

        std::list< TUnknownObjItem * >::iterator iter;
        TUnknownObjItem *pItem;

        while( m_plistObj->size() > 0 )
        {
            iter = m_plistObj->begin();
            if ( *iter )
            {
                pItem = *iter;
                FreeOne( pItem );
                delete pItem;
            }
            m_plistObj->erase( iter );
        }

        m_plistObj->clear();
        delete m_plistObj;
        m_plistObj = 0;
    }

    void FreeOne( TUnknownObjItem *pItem )
    {
        if ( ! pItem )
            return;
        if ( pItem->_pObject && pItem->_pObjID )
            pItem->_pObject->Free( & pItem->_pObjID );

        pItem->_pObjID = ((void*)(0));

        if ( pItem->_bHandleIt && pItem->_pObject )
        {
            delete pItem->_pObject;
            pItem->_pObject = 0;
        }
    }

public:
    UCUnknownObjList()
    {
        m_bValid = true;
        m_plistObj = new std::list< TUnknownObjItem* >();
    }
    virtual ~UCUnknownObjList()
    {
        FreeAll();
    }

    virtual void Close()
    {
        FreeAll();
    }

    // bHandleIt: true -- pUnknown will controlled by this class
    virtual void* New( UCUnKnownBase *pUnknown, bool bHandleIt )
    {
        if ( ! pUnknown || ! m_bValid )
            return ((void*)(0));

        TUnknownObjItem *pItem = new TUnknownObjItem;
        if ( pItem == ((void*)(0)) )
            return ((void*)(0));

        pItem->_bHandleIt = bHandleIt;
        pItem->_pObject   = pUnknown;
        pItem->_pObjID    = ((void*)(0));

        bool retStat = pItem->_pObject->Allocate( & pItem->_pObjID );
        if ( ! retStat || ! pItem->_pObjID )
        {
            // Free object instance
            pItem->_pObject->Free( & pItem->_pObjID );
            // Free creator if need.
            if ( bHandleIt )
                delete pUnknown;
            // Free item memory
            delete pItem;
            // error return
            return ((void*)(0));
        }

        // Add
        if ( m_plistObj ) m_plistObj->push_back( pItem );

        return pItem->_pObjID;
    }

    virtual void Delete( void *pID )
    {
        if ( ! pID || ! m_bValid || ! m_plistObj )
            return;

        std::list< TUnknownObjItem * >::iterator iter;
        TUnknownObjItem *pItem;
        for( iter = m_plistObj->begin(); iter != m_plistObj->end(); iter++ )
        {
            pItem = *iter;
            if ( ! pItem ) continue;
            if ( pItem->_pObjID == pID )
            {

                // Process
                FreeOne( pItem );
                m_plistObj->erase( iter );
                delete pItem;

                break;
            }
        }
    }

    virtual bool IsExist( void *pID )
    {
        bool isOK = false;

        if ( ! pID || ! m_plistObj )
            return false;

        std::list< TUnknownObjItem * >::iterator iter;
        TUnknownObjItem *pItem;
        for( iter = m_plistObj->begin(); iter != m_plistObj->end(); iter++ )
        {
            pItem = *iter;
            if ( ! pItem ) continue;
            if ( pItem->_pObjID == pID )
            {
                isOK = true; break;
            }
        }
        return isOK;
    }

    virtual TUnknownObjItem* Get( void *pID )
    {
        if ( ! pID || ! m_plistObj )
            return ((TUnknownObjItem*)(0));

        std::list< TUnknownObjItem * >::iterator iter;
        TUnknownObjItem *pItem;
        for( iter = m_plistObj->begin(); iter != m_plistObj->end(); iter++ )
        {
            pItem = *iter;
            if ( ! pItem ) continue;
            if ( pItem->_pObjID == pID )
            {
                return pItem;
            }
        }

        return ((TUnknownObjItem*)(0));
    }

    std::list< TUnknownObjItem* >  *List() const { return m_plistObj; }
};



#endif
