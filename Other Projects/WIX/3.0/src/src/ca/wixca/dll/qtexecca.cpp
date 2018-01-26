//-------------------------------------------------------------------------------------------------
// <copyright file="qtexecca.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Executes command line instructions without popping up a shell.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"

#define OUTPUT_BUFFER 1024

HRESULT BuildCommandLine(
    __out LPWSTR *ppwzCommand
    )
{
    Assert(ppwzCommand);

    HRESULT hr = S_OK;
    BOOL fScheduled = ::MsiGetMode(WcaGetInstallHandle(), MSIRUNMODE_SCHEDULED);
    BOOL fRollback = ::MsiGetMode(WcaGetInstallHandle(), MSIRUNMODE_ROLLBACK);
    BOOL fCommit = ::MsiGetMode(WcaGetInstallHandle(), MSIRUNMODE_COMMIT);

    if (fScheduled || fRollback || fCommit)
    {
        if (WcaIsPropertySet("CustomActionData"))
        {
            hr = WcaGetProperty( L"CustomActionData", ppwzCommand);
            ExitOnFailure(hr, "failed to get CustomActionData");
        }
    }
    else if (WcaIsPropertySet("QtExecCmdLine"))
    {
        hr = WcaGetFormattedProperty( L"QtExecCmdLine", ppwzCommand);
        ExitOnFailure(hr, "failed to get QtExecCmdLine");
        hr = WcaSetProperty(L"QtExecCmdLine", L""); // clear out the property now that we've read it
        ExitOnFailure(hr, "failed to set QtExecCmdLine");
    }
    if (!*ppwzCommand)
        ExitOnFailure(hr = E_INVALIDARG, "failed to get command line data");

    if (L'"' != **ppwzCommand)
    {
        WcaLog(LOGMSG_STANDARD, "Command string must begin with quoted application name.");
        ExitOnFailure(hr = E_INVALIDARG, "invalid command line property value");
    }

LExit:
    return hr;
}

#define ONEMINUTE 60000

DWORD GetTimeout()
{
    DWORD dwTimeout = ONEMINUTE;
    HRESULT hr = S_OK;

    LPWSTR pwzData = NULL;

    if (WcaIsPropertySet("QtExecCmdTimeout"))
    {
        hr = WcaGetProperty( L"QtExecCmdTimeout", &pwzData);
        ExitOnFailure(hr, "failed to get QtExecCmdTimeout");

        if ((dwTimeout = (DWORD)_wtoi(pwzData)) == 0)
            dwTimeout = ONEMINUTE;
    }

LExit:
    ReleaseStr(pwzData);

    return dwTimeout;

}

extern "C" UINT __stdcall CAQuietExec(
    __in MSIHANDLE hInstall
    )
{
    Assert(hInstall);
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;
    LPWSTR pwzCommand = NULL;
    DWORD dwTimeout = 0;

    hr = WcaInitialize(hInstall,"CAQuietExec");
    ExitOnFailure(hr, "failed to initialize");

    hr = BuildCommandLine(&pwzCommand);
    ExitOnFailure(hr, "failed to get Command Line");

    dwTimeout = GetTimeout();

    hr = QuietExec(pwzCommand, dwTimeout);
    ExitOnFailure(hr, "CAQuietExec Failed");

LExit:
    ReleaseStr(pwzCommand);

    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er); 
}
