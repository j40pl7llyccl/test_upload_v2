#ifndef UUtilities_h__
#define UUtilities_h__

#include <vector>
#include <string>

char** UUtility_SplitStr(const char *pStr, char *pSplit, int &nRet);
void UUtility_HandleSplitedStrings(char **pp, int count);

bool UUtility_isInt(const char * pChar);
bool UUtility_isFloat(const char * pChar);

__int64 UUtility_FileSize(const char *pPath);

#ifdef UBASE_SUPPORT_TYPE_64
void UUtility_SplitStringFrom0(const char *pBuff, __int64 nBuff, std::vector<std::string> *pV);
#else
void UUtility_SplitStringFrom0(const char *pBuff, unsigned int nBuff, std::vector<std::string> *pV);
#endif
#endif
