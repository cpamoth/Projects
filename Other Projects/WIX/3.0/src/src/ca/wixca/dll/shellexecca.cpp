//-------------------------------------------------------------------------------------------------
// <copyright file="shellexecca.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Executes default shell verb on a specified object (file, URL, and so forth).
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"

HRESULT ShellExec(
    __in LPCWSTR wzTarget
    )
{
    HRESULT hr = S_OK;

    HINSTANCE hinst = ::ShellExecuteW(NULL, NULL, wzTarget, NULL, NULL, SW_SHOWDEFAULT);
    if (hinst <= HINSTANCE(32))
    {
        switch (int(hinst))
        {
        case ERROR_FILE_NOT_FOUND:
            hr = HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
            break;
        case ERROR_PATH_NOT_FOUND:
            hr = HRESULT_FROM_WIN32(ERROR_PATH_NOT_FOUND);
            break;
        case ERROR_BAD_FORMAT:
            hr = HRESULT_FROM_WIN32(ERROR_BAD_FORMAT);
            break;
        case SE_ERR_ASSOCINCOMPLETE:
        case SE_ERR_NOASSOC:
            hr = HRESULT_FROM_WIN32(ERROR_NO_ASSOCIATION);
            break;
        case SE_ERR_DDEBUSY:
        case SE_ERR_DDEFAIL:
        case SE_ERR_DDETIMEOUT:
            hr = HRESULT_FROM_WIN32(ERROR_DDE_FAIL);
            break;
        case SE_ERR_DLLNOTFOUND:
            hr = HRESULT_FROM_WIN32(ERROR_DLL_NOT_FOUND);
            break;
        case SE_ERR_OOM:
            hr = E_OUTOFMEMORY;
            break;
        case SE_ERR_ACCESSDENIED:
            hr = E_ACCESSDENIED;
            break;
        default:
            hr = E_FAIL;
        }

        ExitOnFailure1(hr, "ShellExec failed with return code %d", int(hinst));
    }

LExit:
    return hr;
}

extern "C" UINT __stdcall WixShellExec(
    __in MSIHANDLE hInstall
    )
{
    Assert(hInstall);
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;
    LPWSTR pwzTarget = NULL;

    hr = WcaInitialize(hInstall, "WixShellExec");
    ExitOnFailure(hr, "failed to initialize");

    hr = WcaGetFormattedProperty(L"WixShellExecTarget", &pwzTarget);
    ExitOnFailure(hr, "failed to get WixShellExecTarget");

    WcaLog(LOGMSG_VERBOSE, "WixShellExecTarget is %S", pwzTarget);

    if (!pwzTarget || !*pwzTarget)
    {
        hr = E_INVALIDARG;
        ExitOnFailure(hr, "failed to get WixShellExecTarget");
    }

    hr = ShellExec(pwzTarget);
    ExitOnFailure(hr, "failed to launch target");

LExit:
    ReleaseStr(pwzTarget);

    if (FAILED(hr)) 
    {
        er = ERROR_INSTALL_FAILURE;
    }
    return WcaFinalize(er); 
}
