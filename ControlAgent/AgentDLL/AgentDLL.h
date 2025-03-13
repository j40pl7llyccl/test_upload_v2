// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the AGENTDLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// AGENTDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef AGENTDLL_EXPORTS
#define AGENTDLL_API __declspec(dllexport)
#else
#define AGENTDLL_API __declspec(dllimport)
#endif

// This class is exported from the AgentDLL.dll
class AGENTDLL_API CAgentDLL {
public:
	CAgentDLL(void);
	// TODO: add your methods here.
};

extern AGENTDLL_API int nAgentDLL;

AGENTDLL_API int fnAgentDLL(void);
