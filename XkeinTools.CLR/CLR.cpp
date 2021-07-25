#include "CLR.h"

#include <string>

#include <mscoree.h>
#include <metahost.h>

#import "mscorlib.tlb" raw_interfaces_only \
    high_property_prefixes("_get","_put","_putref")		\
    rename("ReportEvent", "InteropServices_ReportEvent")  rename("or", "or_")
using namespace mscorlib;

void ActiveByCLR()
{
    //using System::Reflection::Assembly;
    //using System::Activator;
    //using System::Console;

    //Console::WriteLine("AllocConsole() succeed");
    ////Assembly^ assembly = Assembly::Load("DynamicPatcher");
    ////Activator::CreateInstance(assembly->GetType("Program"));
    ////auto program = gcnew DynamicPatcher::Program();
    // use clr will trigger many int3 breakpoint, which make syringe work bad
    //DynamicPatcher::Program::Activate();
    //Console::WriteLine("load succeed");
}

// see https://www.codeproject.com/Articles/1236146/Protecting-NET-plus-Application-By-Cplusplus-Unman

ICLRMetaHost* pMetaHost = NULL; /// Metahost installed.
ICLRMetaHostPolicy* pMetaHostPolicy = NULL;  /// Metahost Policy installed.
ICLRDebugging* pCLRDebugging = NULL;  /// Metahost Debugging installed.
ICLRRuntimeInfo* pRuntimeInfo = NULL;
ICLRRuntimeHost* pClrRuntimeHost = NULL;
ICorRuntimeHost* pCorRuntimeHost = NULL;
IUnknownPtr pAppDomainThunk = NULL;
_AppDomainPtr pDefaultAppDomain = NULL;

#define CHECK(expr) \
    if(auto hr = expr; FAILED(hr)) { \
        MessageBoxW(NULL, (TEXT(#expr##"\nHRESULT: ") + std::to_wstring(hr)).c_str(), TEXT("HRESULT error!"), MB_OK); \
        if (pMetaHost) { \
            pMetaHost->Release(); \
            pMetaHost = NULL; \
        } \
        if (pRuntimeInfo) { \
            pRuntimeInfo->Release(); \
            pRuntimeInfo = NULL; \
        } \
    return false; \
    }

bool CLR::Init()
{
    CHECK(CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost)));
    CHECK(CLRCreateInstance(CLSID_CLRMetaHostPolicy, IID_PPV_ARGS(&pMetaHostPolicy)));
    CHECK(CLRCreateInstance(CLSID_CLRDebugging, IID_PPV_ARGS(&pCLRDebugging)));

    CHECK(pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo)));

    BOOL loadable;
    CHECK(pRuntimeInfo->IsLoadable(&loadable));

    //CHECK(pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost)));
    //CHECK(pClrRuntimeHost->Start());
    CHECK(pRuntimeInfo->GetInterface(CLSID_CorRuntimeHost, IID_PPV_ARGS(&pCorRuntimeHost)));
    CHECK(pCorRuntimeHost->Start());

    pAppDomainThunk = NULL;
    CHECK(pCorRuntimeHost->GetDefaultDomain(&pAppDomainThunk));

    pDefaultAppDomain = NULL;
    CHECK(pAppDomainThunk->QueryInterface(IID_PPV_ARGS(&pDefaultAppDomain)));

    return true;
}

IUnknownPtr CLR::LoadAssembly(const wchar_t* assemblyString)
{
    _AssemblyPtr pAssembly = NULL;
    CHECK(pDefaultAppDomain->Load_2(_bstr_t(assemblyString), &pAssembly));
    return pAssembly;
}

#undef CHECK

enum class CLRState
{
    UnInitialized, Initialized
};

#include <atomic>
std::atomic<CLRState> clr_state = CLRState::UnInitialized;

extern "C" __declspec(dllexport) bool __cdecl CLR_Init()
{
    if (clr_state.load() != CLRState::UnInitialized) {
        return false;
    }

    bool ret = CLR::Init();
    clr_state.store(CLRState::Initialized);
    return ret;
}

extern "C" __declspec(dllexport) IUnknownPtr __cdecl CLR_LoadAssembly(const wchar_t* assemblyString)
{
    return CLR::LoadAssembly(assemblyString);
}


