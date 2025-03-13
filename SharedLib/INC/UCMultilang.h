#ifndef UCMultilang_h__
#define UCMultilang_h__

#include <stdio.h>
#include <stdarg.h>
#include <string>
#include <map>
#include "UCObject.h"

#define UCMULTILANG_STRING_SYMBOL   '"'
#define UCMULTILANG_SPLIT_SYMBOL    '='

using namespace std;
// using multi-language, the format must be considerated.
//  - Console: SetConsoleOutputCP(65001) --> UTF-8
//  - Default: ascii
// default loading formating
//  - "Original string" = "Applying language string"
//  - cannot not including control symbol
class UCMultilang : public UCObject
{
protected:
    string              _strLanguageID;
    map<string, string> _mapToTranslate;

public:
    UCMultilang();
    UCMultilang(char *pLangCode);
    virtual ~UCMultilang();

    void LangCode(char *p);
    const char* LangCode();

    bool Add(const char *pOriFormat, const char *pTranslatedFormat);
    bool Remove(const char *pOriFormat);
    const char* Get(const char *pOriFormat);
    void Read(char *pPath);

    static void Translate(char *pBuff, size_t nBuff, const char *Format, ...);
    static bool GetKeyValue(char *pString, char* &ppKey, int &nKeyOffset, int &nKeySize, char* &ppVal, int &nValOffset, int &nValSize);
    static bool GetKeyValue(string str, int &nKeyOffset, int &nKeySize, int &nValOffset, int &nValSize);
    static void DebugPrint(char *pBeg, int size);
};

#endif
