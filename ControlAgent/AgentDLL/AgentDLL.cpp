// AgentDLL.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "AgentDLL.h"


// This is an example of an exported variable
AGENTDLL_API int nAgentDLL=0;

// This is an example of an exported function.
AGENTDLL_API int fnAgentDLL(void)
{
    return 42;
}

// This is the constructor of a class that has been exported.
// see AgentDLL.h for the class definition
CAgentDLL::CAgentDLL()
{
    return;
}
