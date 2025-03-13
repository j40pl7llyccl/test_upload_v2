// SharedMemTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "UCDataSyncW32.h"
#include <vector>
#include <conio.h>
#include <ctype.h>
#include "UCWin32SharedMemFormating.h"

static void Test1();

static BOOL GetFromConsole( int ch, char *pBuff, INT32 nBuff, INT32 &nCurCnt)
{
    if (ch >= ' ' && ch <= '~' && nCurCnt < ( nBuff - 1))
    {
        if ((ch >= 'a' && ch <= 'z') ||
            (ch >= '0' && ch <= '9') || ch == ',' ||
            ch == '-' || ch == '.')
            pBuff[nCurCnt++] = ch;
    }
    else if (ch == 0xa)
    {
        if (nCurCnt >= (nBuff - 1)) pBuff[nBuff - 1] = 0;
        else pBuff[nCurCnt] = 0;
        return TRUE;
    }

    return FALSE;
}

static void ClearVectorStrings(std::vector<char *> &v)
{
    std::vector<char *>::iterator it;
    while (v.size() > 0)
    {
        it = v.begin();

        char *pStr = (char *)(*it);
        delete[] pStr;

        v.erase(it);
    }
}

static void SplitString(std::vector<char *> &strings, char splitC, char *pString)
{
    ClearVectorStrings(strings);

    if (!pString || strlen(pString) <= 0)
        return;

    int szStr = (int)strlen(pString);
    char *pTmp = new char[szStr + 1];
    if (!pTmp)
        return;

    int currCnt = 0;
    for (int i = 0; i < szStr; i++)
    {
        if (pString[i] != splitC)
        {
            pTmp[currCnt++] = pString[i];
        }
        else
        {
            pTmp[currCnt] = 0;
            currCnt = 0;

            int szTT = (int)strlen(pTmp);
            if (szTT > 0)
            {
                char *pTT = new char[szTT + 1];
                if (!pTT)
                {
                    delete[] pTmp;
                    ClearVectorStrings(strings);
                    return;
                }
                memcpy(pTT, pTmp, szTT);
                pTT[szTT] = 0;
                strings.push_back(pTT);
            }
        }
    }

    if (currCnt > 0)
    {
        pTmp[currCnt] = 0;
        currCnt = 0;

        int szTT = (int)strlen(pTmp);
        if (szTT > 0)
        {
            char *pTT = new char[szTT + 1];
            if (!pTT)
            {
                delete[] pTmp;
                ClearVectorStrings(strings);
                return;
            }
            memcpy(pTT, pTmp, szTT);
            pTT[szTT] = 0;
            strings.push_back(pTT);
        }
    }
}

static void ActionOnInput(std::vector<char *> &strings, UCDataSyncW32<double> *pShMem)
{
    if (strings.size() <= 0 || !pShMem)
        return;

    tYUX_I64 count = 0;
    double *pMem = pShMem->Get(count);

    if (!strcmp("init", strings[0]))
    {
        if (strings.size() < 2)
        {
            printf("init: no count!\n");
            return;
        }
        int cnt = atoi(strings[1]);
        tYUX_BOOL ok = pShMem->Initialize("TestDoubleSharedMem$$", "TestDoubleSharedMemMux", cnt);
        printf("init: status=%s, count = %d\n", ok ? "true" : "false", cnt);
    }
    else if (!strcmp("all", strings[0]))
    {
        for (int i = 0; i < count; i++)
        {
            printf("[%d]: %f\n", i, pMem[i]);
        }
    }
    else if (!strcmp("list", strings[0]))
    {

        if (strings.size() > 2)
        {
            int from = atoi(strings[1]);
            int to = atoi(strings[2]);
            int tmp;

            if (from > to)
            {
                tmp = from;
                from = to;
                to = tmp;
            }

            if (from < 0 || to < 0 || from >= count || to >= count)
            {
                printf("list: index out-of-range!\n");
                return;
            }

            for (int i = from; i <= to; i++)
                printf("[%d]: %f\n", i, pMem[i]);
        }
        else if (strings.size() > 1)
        {
            int index = atoi(strings[1]);
            if (index < 0 || index >= count)
            {
                printf("list: index out-of-range!\n");
                return;
            }

            printf("[%d]: %f\n", index, pMem[index]);
        }
        else
        {
            printf("list: no index to get data!\n");
        }
    }
    else if (!strcmp("set", strings[0]))
    {
        if (strings.size() < 3)
        {
            printf("set: parameter error!\n");
            return;
        }

        int index = atoi(strings[1]);
        double val = atof(strings[2]);

        if (index < 0 || index >= count)
        {
            printf("set: index out-of-range!\n");
            return;
        }

        int stat = pShMem->Set(index, val);
        printf("set: status=%s, index-%d with val %f\n", stat ? "true" : "false", index, val);
    }
    else if (!strcmp("count", strings[0]))
    {
#ifdef UBASE_SUPPORT_TYPE_64
        printf("Count: %I64d\n", count);
#else
        printf("Count: %d\n", count);
#endif
    }
    else if (!strcmp("save", strings[0]))
    {
        if (strings.size() < 2)
        {
            printf("save: without file name!\n");
            return;
        }
        char path[512];
        sprintf_s(path, sizeof(path) - 1, ".\\%s", strings[1]);
        pShMem->WriteFile(path);
    }
    else if (!strcmp("saveini", strings[0]))
    {
        if (strings.size() < 2)
        {
            printf("save: without file name!\n");
            return;
        }
        char path[512];
        sprintf_s(path, sizeof(path) - 1, ".\\%s", strings[1]);
        pShMem->WriteToIni(path, GetItemFromTypeDouble);
    }
    else if (!strcmp("read", strings[0]))
    {
        if (strings.size() < 2)
        {
            printf("read: parameter error!\n");
            return;
        }

        char tmp[512];
        sprintf_s(tmp, sizeof(tmp) - 1, ".\\%s", strings[1]);
        int stat = pShMem->ReadFile(tmp);
        printf("read: file %s with status %s\n", tmp, stat ? "true" : "false");
    }
    else if (!strcmp("readini", strings[0]))
    {
        if (strings.size() < 2)
        {
            printf("read: parameter error!\n");
            return;
        }

        char tmp[512];
        sprintf_s(tmp, sizeof(tmp) - 1, ".\\%s", strings[1]);
        pShMem->ReadFromIni(tmp, SetItemFromTypeDouble);
    }
    else if (!strcmp("test1", strings[0]))
    {
        Test1();
    }
    else if (!strcmp("createnamedmap", strings[0]))
    {
        if (strings.size() < 4)
        {
            printf("create_named_map: parameter error!\n");
            return;
        }

        if (strlen(strings[1]) < 0)
        {
            printf("create_named_map: invalid name!\n");
            return;
        }

        int offset = atoi(strings[2]);
        int count = atoi(strings[3]);

        if (!pShMem->SetNamedMap(strings[1], offset, count))
        {
            printf("create_named_map: create fail!\n");
            return;
        }
        else
            printf("create_named_map: name=%s, offset = %ld, count = %ld\n", strings[1], offset, count);
    }
    else if (!strcmp("setnamedmap", strings[0]))
    {
        TUCDataItemInfo itemInfo;
        if (!pShMem->QueryFromNamedMap(strings[1], itemInfo))
        {
            printf("set_named_map: cannot get name %s\n", strings[1]);
            return;
        }

        tYUX_I64 count;
        double *pAddr = pShMem->Get(count);
        if (itemInfo._nOffset < 0 || itemInfo._nOffset >= count || (itemInfo._nOffset + itemInfo._nSize) > count)
        {
            printf("set_named_map: invalid item offset=%I64d, size=%I64d\n", itemInfo._nOffset, itemInfo._nSize);
            return;
        }

        for (size_t i = 2, cnt = 0; i < strings.size() && (tYUX_I64)cnt < itemInfo._nSize; i++, cnt++)
        {
            pAddr[itemInfo._nOffset + cnt] = atof(strings[i]);
        }
    }
    else if (!strcmp("getnamedmap", strings[0]))
    {
        if (strings.size() < 2)
        {
            printf("get_named_map: parameter error!\n");
            return;
        }

        TUCDataItemInfo itemInfo;
        if (!pShMem->QueryFromNamedMap(strings[1], itemInfo))
        {
            printf("get_named_map: cannot get name %s\n", strings[1]);
            return;
        }

        tYUX_I64 count;
        double *pAddr = pShMem->Get(count);
        if (itemInfo._nOffset < 0 || itemInfo._nOffset >= count || (itemInfo._nOffset + itemInfo._nSize) > count)
        {
            printf("get_named_map: invalid item offset=%I64d, size=%I64d\n", itemInfo._nOffset, itemInfo._nSize);
            return;
        }

        printf("[%s] offset = %I64d, size = %I64d\n",strings[1], itemInfo._nOffset, itemInfo._nSize);
        for (tYUX_I64 i = 0; i < itemInfo._nSize; i++)
        {
            printf("%f ", pAddr[itemInfo._nOffset+i]);
        }
        printf("\n");
    }
    else if (!strcmp("listnamedmap", strings[0]))
    {
        size_t count = pShMem->NumofNamedMap();
        TUCDataItemInfo itemInfo;
        for (size_t i = 0; i < count; i++)
        {
            const char *pKey = pShMem->GetNamedMapItem(i, itemInfo);
            printf("index %I64d, name = %s, offset = %I64d, count=%I64d\n", i, pKey, itemInfo._nOffset, itemInfo._nSize);
        }
    }
}

static char *g_strCmd = 
"exit: exit the program\n\
init: init,[count: int]\n\
all: all the values in this shared memory\n\
count: number of items\n\
save: save, [file name: string]\n\
save ini: saveini, [file name: string]\n\
read: read, [file name: string]\n\
read ini: readini, [file name: string]\n\
list: list, [From Index: INT32], [To Index: INT32]\n\
set: set, [Index: INT32], [Value: double]\n\
create named map: create_named_map, [name: string], [offset: int], [count: int]\n\
set named map: set_named_map, [name: string], val1, val2...\n\
get named map: get_named_map, [name: string]\n\
list named map: list_named_map\n\
formating Test: test1\n\
";

int main()
{
    UCDataSyncW32<double> *pDoubleSharedMem = new UCDataSyncW32<double>();
    //pDoubleSharedMem->Initialize("TestDoubleSharedMem", "TestDoubleSharedMemMux", 100);

    char tmp[256];
    INT32 nCurCount = 0;
    BOOL bGetLineReady = FALSE;
    std::vector<char *> splitedStrings;

    printf("%s", g_strCmd);

    while (true)
    {
        int ch = getchar();
        bGetLineReady = GetFromConsole(ch, tmp, sizeof(tmp), nCurCount);
        if (bGetLineReady)
        {
            SplitString(splitedStrings, ',', tmp);
            nCurCount = 0;

            if (splitedStrings.size() > 0 && !strcmp("exit", splitedStrings[0]))
            {
                system("cls");
                printf("Exit!!\n");
                break;
            }
            else
            {
                ActionOnInput(splitedStrings, pDoubleSharedMem);
            }

            printf("Press any key...\n");
            _getch();

            system("cls");
            printf("%s", g_strCmd);

        }
    }


    ClearVectorStrings(splitedStrings);

    delete pDoubleSharedMem;

    return 0;
}

#include "UCSharedMemFormating.h"

void Test1()
{
    UCSharedMemFormating *pFormating = new UCWin32SharedMemFormating();

    if (!pFormating->Initialize("TestFormating", 64, 50))
    {
        printf("[Test1] call Initialize() fail!\n");
        delete pFormating;
        return;
    }

    tYUX_PVOID hI32Subsection1;
    tYUX_PVOID hI32Subsection2;
    tYUX_BOOL status = pFormating->CreateI32ShMem("I32_Subsection1", 5, hI32Subsection1);
    if (!status)
        printf("[Test1] create I32_Subsection1 fail!\n");
    status = pFormating->CreateI32ShMem("I32_Subsection2", 2, hI32Subsection2);
    if (!status)
        printf("[Test1] create I32_Subsection2 fail!\n");

    tYUX_PVOID hDfSubsection1;
    tYUX_PVOID hDfSubsection2;
    tYUX_PVOID hDfSubsection3;
    status = pFormating->CreateDoubleShMem("Df_Subsection1", 4, hDfSubsection1);
    if (!status)
        printf("[Test1] create hDfSubsection1 fail!\n");

    status = pFormating->CreateDoubleShMem("Df_Subsection2", 3, hDfSubsection2);
    if (!status)
        printf("[Test1] create hDfSubsection2 fail!\n");

    status = pFormating->CreateDoubleShMem("Df_Subsection3", 2, hDfSubsection3);
    if (!status)
        printf("[Test1] create hDfSubsection3 fail!\n");

    tYUX_PVOID hU8Subsection1;
    status = pFormating->CreateU8ShMem("U8_Subsection1", 512 * 512, hU8Subsection1);
    if (!status)
        printf("[Test1] create hU8Subsection1 fail!\n");

    tYUX_PVOID hI32_1_0;
    tYUX_PVOID hI32_1_1;
    pFormating->AddI32(hI32Subsection1, 2, hI32_1_0);
    pFormating->AddI32(hI32Subsection1, 3, hI32_1_1);

    tYUX_PVOID hI32_2_0;
    pFormating->AddI32(hI32Subsection2, 10, hI32_2_0);

    tYUX_PVOID hDF_1_0;
    tYUX_PVOID hDF_1_1;
    pFormating->AddDouble(hDfSubsection1, 2, hDF_1_0);
    pFormating->AddDouble(hDfSubsection1, 2, hDF_1_1);

    tYUX_PVOID hDF_2_0;
    pFormating->AddDouble(hDfSubsection2, 10, hDF_2_0);

    tYUX_PVOID hDF_3_0;
    pFormating->AddDouble(hDfSubsection3, 2, hDF_3_0);

    tYUX_PVOID hU8_1_0;
    pFormating->AddU8(hU8Subsection1, 256 * 256, hU8_1_0);

    tYUX_PVOID dummy;
    int nI32_1_0;
    tYUX_I32 *pI32_1_0 = pFormating->GetI32(hI32_1_0, nI32_1_0, dummy);

    int nDF_3_0;
    double *pDf_3_0 = pFormating->GetDouble(hDF_3_0, nDF_3_0, dummy);

    int nDF_1_1;
    tYUX_I32 *pDf_1_1 = pFormating->GetI32(hDF_1_1, nDF_1_1, dummy);

    tYUX_I32 I32Dat_1_1[3];
    int tt = UBASE_ARR_COUNT(I32Dat_1_1);
    pFormating->AccessI32(hI32_1_1, TRUE, I32Dat_1_1, tt);

    pFormating->DeleteShMem(UCSHAREDMEM_DOUBLE);

    pDf_3_0 = pFormating->GetDouble(hDF_3_0, nDF_3_0, dummy);

	status = pFormating->CreateDoubleShMem("Df_Subsection3", 2, hDfSubsection3);
	if (!status)
		printf("[Test1] create hDfSubsection3 fail!\n");
	pFormating->AddDouble(hDfSubsection3, 2, hDF_3_0);

    pDf_3_0 = pFormating->GetDouble(hDF_3_0, nDF_3_0, dummy);

    delete pFormating;

}
