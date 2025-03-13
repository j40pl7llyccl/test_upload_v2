#include "stdafx.h"
#include "UCMultilang.h"
#include <vector>
#include <sstream>
#include <fstream>

UCMultilang::UCMultilang()
    : UCObject()
{
    _strClassRttiName = typeid(*this).name();
}
UCMultilang::UCMultilang(char *pLangCode)
    : UCObject()
{
    LangCode(pLangCode);
    _strClassRttiName = typeid(*this).name();
}
UCMultilang::~UCMultilang() {}

void UCMultilang::LangCode(char *p) { _strLanguageID = !p || strlen(p) <= 0 ? "" : p; }
const char* UCMultilang::LangCode() { return _strLanguageID.c_str(); }

bool UCMultilang::Add(const char *pOriFormat, const char *pTranslatedFormat)
{
    if (!pOriFormat || strlen(pOriFormat) <= 0 || !pTranslatedFormat || strlen(pTranslatedFormat) <= 0)
        return false;

    map<string, string>::iterator it = _mapToTranslate.find(pOriFormat);
    if (it != _mapToTranslate.end())
        _mapToTranslate[pOriFormat] = pTranslatedFormat;
    else
    {
        _mapToTranslate.insert(pair<string, string>(pOriFormat, pTranslatedFormat));
    }
    return true;
}
bool UCMultilang::Remove(const char *pOriFormat)
{
    if (!pOriFormat || strlen(pOriFormat) <= 0)
        return false;

    map<string, string>::iterator it = _mapToTranslate.find(pOriFormat);
    if (it != _mapToTranslate.end())
    {
        _mapToTranslate.erase(it);
        return true;
    }

    return false;
}
const char* UCMultilang::Get(const char *pOriFormat)
{
    if (!pOriFormat || strlen(pOriFormat) <= 0)
        return false;

    map<string, string>::iterator it = _mapToTranslate.find(pOriFormat);
    return it != _mapToTranslate.end() ? it->second.c_str() : pOriFormat;
}
void UCMultilang::Read(char *pPath)
{
    _mapToTranslate.clear();

    string line;
    ifstream infile(pPath);
    while (getline(infile, line))
    {
        int keyOffset, keySize;
        int valOffset, valSize;
        string strKey, strVal;

        if (!GetKeyValue(line, keyOffset, keySize, valOffset, valSize))
            continue;
        if (keySize <= 0 || valSize <= 0)
            continue;

        strKey.assign(line, keyOffset, keySize);
        strVal.assign(line, valOffset, valSize);

        Add(strKey.c_str(), strVal.c_str());
    }
}
void UCMultilang::Translate(char *pBuff, size_t nBuff, const char *pFormat, ...)
{
    va_list vl;
    va_start(vl, pFormat);
    try
    {
        vsprintf_s(pBuff, nBuff, pFormat, vl);
    }
    catch (...)
    {
        if (pBuff  && nBuff > 0)
            pBuff[0] = 0;
    }
    va_end(vl);
}

static bool locGetKeyValue(const char *pString,
    int &nKeyOffset, int &nKeySize,
    int &nValOffset, int &nValSize)
{
    nKeyOffset = 0;
    nKeySize = 0;

    nValOffset = 0;
    nValSize = 0;

    if (!pString || strlen(pString) <= 0)
        return false;

    std::vector<int> indexes;
    int nLen = (int)strlen(pString);
    int prevSymb = 0;
    int posSplit = -1;

    for (int i = 0; i < nLen; i++)
    {
        if (pString[i] == UCMULTILANG_STRING_SYMBOL && prevSymb != '\\')
            indexes.push_back(i);
        prevSymb = pString[i];
    }

    if (indexes.size() < 4)
        return false;

    int index1 = indexes[1];
    int index2 = indexes[2];
    for (int i = index1 + 1; i < index2; i++)
    {
        if (i > index1 && i < index2 && pString[i] == UCMULTILANG_SPLIT_SYMBOL)
        {
            posSplit = i;
            break;
        }
    }
    if (posSplit < 0)
        return false;

    index1 = indexes[0]; index1++;
    index2 = indexes[1]; index2--;
    nKeyOffset = index1;
    nKeySize = index2 - index1 + 1;

    index1 = indexes[2]; index1++;
    index2 = indexes[3]; index2--;
    nValOffset = index1;
    nValSize = index2 - index1 + 1;

    return true;
}

bool UCMultilang::GetKeyValue(char *pString,
    char* &ppKey, int &nKeyOffset, int &nKeySize,
    char* &ppVal, int &nValOffset, int &nValSize)
{
    ppKey = 0;
    ppVal = 0;
    if (!locGetKeyValue(pString, nKeyOffset, nKeySize, nValOffset, nValSize))
        return false;    

    ppKey = nKeySize <= 0 ? "" : &pString[nKeyOffset];
    ppVal = nValSize <= 0 ? "" : &pString[nValOffset];

    return true;
}

bool UCMultilang::GetKeyValue(string str, 
    int &nKeyOffset, int &nKeySize, 
    int &nValOffset, int &nValSize)
{
    return locGetKeyValue(str.c_str(), nKeyOffset, nKeySize, nValOffset, nValSize);
}

void UCMultilang::DebugPrint(char *pBeg, int size)
{
    if (!pBeg || size <= 0)
    {
        printf("[UCMultilang::DebugPrint] invalid info!\n");
        return;
    }

    printf("[UCMultilang::DebugPrint] ");
    for (int i = 0; i < size; i++)
    {
        if (pBeg[i] >= ' ' && pBeg[i] <= '~')
            printf("%c", pBeg[i]);
        else
            printf("%.2x", (unsigned char)pBeg[i]);
    }
    printf("\n");
}
