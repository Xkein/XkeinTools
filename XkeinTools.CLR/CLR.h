#pragma once
#include <comdef.h>

class CLR
{
public:
	static bool Init();
	static IUnknownPtr LoadAssembly(const wchar_t* assemblyString);
};

