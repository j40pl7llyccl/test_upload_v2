#ifndef __UCUnknownObjListWin32_h__
#define __UCUnknownObjListWin32_h__

#include <windows.h>
#include "UCUnknownObjList.h"

class UCUnknownObjListWin32 : public UCUnknownObjList
{
protected:
    HANDLE   m_hSem;

    BOOL ObtainSem(DWORD timeout = INFINITE) {
        if (!m_hSem) return TRUE;
        return (::WaitForSingleObject(m_hSem, timeout) == WAIT_OBJECT_0);
    }
    VOID ReleaseSem() {
        if (!m_hSem) return;
        ::ReleaseSemaphore(m_hSem, 1, NULL);
    }

public:
    UCUnknownObjListWin32()
    {
        m_hSem = ::CreateSemaphoreA(NULL, 1, 1, NULL);
    }
    virtual ~UCUnknownObjListWin32()
    {
        ObtainSem();
        FreeAll();
        ReleaseSem();

        if (m_hSem) ::CloseHandle(m_hSem);
        m_hSem = NULL;
    }

    virtual void Close()
    {
        ObtainSem();
        FreeAll();
        ReleaseSem();
    }

    // bHandleIt: true -- pUnknown will controlled by this class
    virtual void* New( UCUnKnownBase *pUnknown, bool bHandleIt )
    {
        if ( ! pUnknown )
            return NULL;
        if ( ! m_bValid ) {
            delete pUnknown;
            return NULL;
        }

        void *pRetID = NULL;

        if (!ObtainSem()) {
            return NULL;
        }

        pRetID = UCUnknownObjList::New( pUnknown, bHandleIt );

        ReleaseSem();
        return pRetID;
    }

    virtual void Delete( void *pID )
    {
        if (!ObtainSem()) return;

        UCUnknownObjList::Delete(pID);

        ReleaseSem();
    }

    virtual bool IsExist( void *pID )
    {
        bool found;
        if (!ObtainSem()) return FALSE;

        found = UCUnknownObjList::IsExist(pID);

        ReleaseSem();
        return found;
    }

    virtual TUnknownObjItem* Get( void *pID )
    {
        TUnknownObjItem *pRet = NULL;

        if (!ObtainSem()) return NULL;

        pRet = UCUnknownObjList::Get(pID);

        ReleaseSem();
        return pRet;
    }

};



#endif
