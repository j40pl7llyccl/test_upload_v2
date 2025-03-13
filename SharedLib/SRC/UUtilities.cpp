#include "stdafx.h"
#include <Windows.h>
#include "UUtilities.h"
#include <string.h>
#include <list>

char** UUtility_SplitStr(const char *pStr, char *pSplit, int &nRet)
{
    nRet = 0;
    if (!pStr || strlen(pStr) <= 0)
        return 0;

    if (!pSplit || strlen(pSplit) <= 0)
        return 0;

    std::list<char *> splited;

    size_t ttLen = strlen(pStr);
    char *pCopy8 = new char[ttLen + 1];
    if (!pCopy8)
        return 0;

    memcpy(pCopy8, pStr, ttLen);
    pCopy8[ttLen] = 0;

    char *pTmp = pCopy8;
    char *pPos;
    char *pBound = pTmp + ttLen;
    while (pTmp && strlen(pTmp) > 0)
    {
        pPos = strstr(pTmp, pSplit);
        if (!pPos)
        {
            int len = (int)strlen(pTmp);
            char *pCur = len > 0 ? new char[len + 1] : 0;
            if (pCur)
            {
                memcpy(pCur, pTmp, len);
                pCur[len] = 0;
                splited.push_back(pCur);
            }
            pTmp = 0;
        }
        else
        {
            int len = (int)( pPos - pTmp);
            char *pCur = len > 0 ? new char[len + 1] : 0;
            if (pCur)
            {
                memcpy(pCur, pTmp, len);
                pCur[len] = 0;
                splited.push_back(pCur);
            }
            pTmp = pPos + strlen(pSplit);
            if (pTmp >= pBound)
                break;
        }
    }

    char **ppRet = splited.size() <= 0 ? 0 : new char*[splited.size()];
    std::list<char*>::iterator it;
    int i = 0;
    for (i = 0, it = splited.begin(); i < (int)splited.size() && it != splited.end(); i++, it++)
    {
        ppRet[i] = (char *)(*it);
    }
    nRet = (int)splited.size();
    splited.clear();

    delete[] pCopy8;
    return ppRet;
}
void UUtility_HandleSplitedStrings(char **pp, int count)
{
    if (!pp)
        return;

    for (int i = 0; i < count; i++)
    {
        delete[] pp[i];
    }
    delete[] pp;
}

bool UUtility_isInt(const char * pChar)
{
    if (!pChar || strlen(pChar) <= 0) return false;
    size_t sz = strlen(pChar);
    for (size_t i = 0; i < sz; i++)
    {
        if ((pChar[i] >= '0' && pChar[i] <= '9') || pChar[i] == '-' || pChar[i] == '+')
            continue;
        return false;
    }
    return true;
}

bool UUtility_isFloat(const char * pChar)
{
    if (!pChar || strlen(pChar) <= 0) return false;
    size_t sz = strlen(pChar);
    for (size_t i = 0; i < sz; i++)
    {
        if ((pChar[i] >= '0' && pChar[i] <= '9') || 
            pChar[i] == '-' || pChar[i] == '+' || 
            pChar[i] == '.' || pChar[i] == 'e' || pChar[i] == 'E' ||
            pChar[i] == 'd' || pChar[i] == 'D')
            continue;
        return false;
    }
    return true;

}

__int64 UUtility_FileSize(const char *pPath)
{
    WIN32_FILE_ATTRIBUTE_DATA fd;
    if (!GetFileAttributesExA(pPath, GetFileExInfoStandard, &fd))
        return -1;

    LARGE_INTEGER size;
    size.HighPart = fd.nFileSizeHigh;
    size.LowPart = fd.nFileSizeLow;
    return size.QuadPart;
}

#ifdef UBASE_SUPPORT_TYPE_64
void UUtility_SplitStringFrom0(const char *pBuff, __int64 nBuff, std::vector<std::string> *pV)
#else
void UUtility_SplitStringFrom0(const char *pBuff, unsigned int nBuff, std::vector<std::string> *pV)
#endif
{
    if (!pV) return;
    pV->clear();

    if (!pBuff || nBuff <= 0)
        return;

    char *pAlloc = new char[nBuff + 1];
    if (!pAlloc)
        return;

    memcpy(pAlloc, pBuff, nBuff);
    pAlloc[nBuff] = 0;

#ifdef UBASE_SUPPORT_TYPE_64
    __int64 count = 0, len;
#else
    unsigned int count = 0, len;
#endif
    char *pBeg = pAlloc;
    while (count < nBuff)
    {
#ifdef UBASE_SUPPORT_TYPE_64
        len = (__int64)strlen(pBeg);
#else
        len = (unsigned int)strlen(pBeg);
#endif
        if (len > 0)
        {
            if ((len + count) > nBuff)
                break;
            pV->push_back(pBeg);
        }

        count += len;
        char *pNext = pBeg + len + 1;
        if ((unsigned int)(pNext - pBeg) >= nBuff)
            break;
        pBeg = pNext;
    }

    delete[] pAlloc;
    return;
}
