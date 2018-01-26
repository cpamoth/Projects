//-------------------------------------------------------------------------------------------------
// <copyright file="cacleanup.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Code to clean up after CAScript actions.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"


/******************************************************************
CommitCAScriptCleanup - entry point for CommitCAScriptCleanup
                        CustomAction

Note: This is a commit CustomAction.
********************************************************************/
extern "C" UINT __stdcall CommitCAScriptCleanup(
    __in MSIHANDLE hInstall
    )
{
    // AssertSz(FALSE, "debug CommitCAScriptCleanup");
    HRESULT hr = S_OK;
    DWORD er = ERROR;

    LPWSTR pwzCustomActionData = NULL;
    LPWSTR pwz = NULL;
    LPWSTR pwzProductCode = NULL;

    // initialize
    hr = WcaInitialize(hInstall, "CommitCAScriptCleanup");
    ExitOnFailure(hr, "Failed to initialize.");

    hr = WcaGetProperty( L"CustomActionData", &pwzCustomActionData);
    ExitOnFailure(hr, "failed to get CustomActionData");

    pwz = pwzCustomActionData;

    hr = WcaReadStringFromCaData(&pwz, &pwzProductCode);
    ExitOnFailure(hr, "failed to process CustomActionData");

    WcaCaScriptCleanup(pwzProductCode, FALSE);

LExit:
    ReleaseStr(pwzProductCode);
    ReleaseStr(pwzCustomActionData);

    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}
