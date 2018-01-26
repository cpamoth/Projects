//-------------------------------------------------------------------------------------------------
// <copyright file="CloseApps.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Code to close applications via custom actions when the installer cannot.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"


// WixCloseApplication     Target      Description     Condition       Attributes      Sequence

// structs
LPCWSTR wzQUERY_CLOSEAPPS = L"SELECT `WixCloseApplication`, `Target`, `Description`, `Condition`, `Attributes` FROM `WixCloseApplication` ORDER BY `Sequence`";
enum eQUERY_CLOSEAPPS { QCA_ID = 1, QCA_TARGET, QCA_DESCRIPTION, QCA_CONDITION, QCA_ATTRIBUTES };


/******************************************************************
 WixCloseApplications - entry point for WixCloseApplications Custom Action

 called as Type 1 CustomAction (binary DLL) from Windows Installer 
 in InstallExecuteSequence before InstallFiles
******************************************************************/
extern "C" UINT __stdcall WixCloseApplications(
    __in MSIHANDLE hInstall
    )
{
//    AssertSz(FALSE, "debug WixCloseApplications");
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    LPWSTR pwzData = NULL;
    LPWSTR pwzId = NULL;
    LPWSTR pwzTarget = NULL;
    LPWSTR pwzDescription = NULL;
    LPWSTR pwzCondition = NULL;
    DWORD dwAttributes = 0;
    MSICONDITION condition = MSICONDITION_NONE;

    DWORD cCloseApps = 0;

    PMSIHANDLE hView = NULL;
    PMSIHANDLE hRec = NULL;
    MSIHANDLE hListboxTable = NULL;
    MSIHANDLE hListboxColumns = NULL;

    LPWSTR pwzCustomActionData = NULL;
    DWORD cchCustomActionData = 0;

    //
    // initialize
    //
    hr = WcaInitialize(hInstall, "WixCloseApplications");
    ExitOnFailure(hr, "failed to initialize");

    //
    // loop through all the objects to be secured
    //
    hr = WcaOpenExecuteView(wzQUERY_CLOSEAPPS, &hView);
    ExitOnFailure(hr, "failed to open view on WixCloseApplication table");
    while (S_OK == (hr = WcaFetchRecord(hView, &hRec)))
    {
        hr = WcaGetRecordString(hRec, QCA_ID, &pwzId);
        ExitOnFailure(hr, "failed to get id from WixCloseApplication table");

        hr = WcaGetRecordString(hRec, QCA_CONDITION, &pwzCondition);
        ExitOnFailure(hr, "failed to get condition from WixCloseApplication table");

        if (pwzCondition && *pwzCondition)
        {
            condition = ::MsiEvaluateConditionW(hInstall, pwzCondition);
            if (MSICONDITION_ERROR == condition)
            {
                hr = E_INVALIDARG;
                ExitOnFailure1(hr, "failed to process condition for WixCloseApplication '%S'", pwzId);
            }
            else if (MSICONDITION_FALSE == condition)
            {
                continue; // skip processing this target
            }
        }

        hr = WcaGetRecordFormattedString(hRec, QCA_TARGET, &pwzTarget);
        ExitOnFailure(hr, "failed to get target from WixCloseApplication table");

        hr = WcaGetRecordFormattedString(hRec, QCA_DESCRIPTION, &pwzDescription);
        ExitOnFailure(hr, "failed to get description from WixCloseApplication table");

        hr = WcaGetRecordInteger(hRec, QCA_ATTRIBUTES, reinterpret_cast<int*>(&dwAttributes));
        ExitOnFailure(hr, "failed to get attributes from WixCloseApplication table");

        //
        // Pass all of the targets to the deferred action in case the app comes back
        // even if we close it now.
        //
        hr = WcaWriteStringToCaData(pwzTarget, &pwzCustomActionData);
        ExitOnFailure(hr, "failed to add target data to CustomActionData");

        hr = WcaWriteIntegerToCaData(dwAttributes, &pwzCustomActionData);
        ExitOnFailure(hr, "failed to add attribute data to CustomActionData");

        cCloseApps++;
    }

    // if we looped through all records all is well
    if (E_NOMOREITEMS == hr)
        hr = S_OK;
    ExitOnFailure(hr, "failed while looping through all apps to close");

    //
    // Do the UI dance now.
    //
    /*

    TODO: Do this eventually

    if (cCloseApps)
    {
        while (TRUE)
        {
            for (DWORD i = 0; i < cCloseApps; ++i)
            {
                hr = WcaAddTempRecord(&hListboxTable, &hListboxColumns, L"Listbox", 0, 4, L"FileInUseProcess", i, target, description);
                if (FAILED(hr))
                {
                }
            }
        }
    }
    */

    //
    // schedule the custom action and add to progress bar
    //
    if (pwzCustomActionData && *pwzCustomActionData)
    {
        Assert(0 < cCloseApps);

        hr = WcaDoDeferredAction(L"WixCloseApplicationsDeferred", pwzCustomActionData, cCloseApps * COST_CLOSEAPP);
        ExitOnFailure(hr, "failed to schedule WixCloseApplicationsDeferred action");
    }

LExit:
    if (hListboxColumns)
    {
        ::MsiCloseHandle(hListboxColumns);
    }
    if (hListboxTable)
    {
        ::MsiCloseHandle(hListboxTable);
    }

    ReleaseStr(pwzCustomActionData);
    ReleaseStr(pwzData);
    ReleaseStr(pwzCondition);
    ReleaseStr(pwzDescription);
    ReleaseStr(pwzTarget);
    ReleaseStr(pwzId);

    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}


/******************************************************************
 WixCloseApplicationsDeferred - entry point for 
                                WixCloseApplicationsDeferred Custom Action
                                called as Type 1025 CustomAction 
                                (deferred binary DLL)

 NOTE: deferred CustomAction since it modifies the machine
 NOTE: CustomActionData == wzTarget\tdwAttributes\t...
******************************************************************/
extern "C" UINT __stdcall WixCloseApplicationsDeferred(
    __in MSIHANDLE hInstall
    )
{
//    AssertSz(FALSE, "debug WixCloseApplicationsDeferred");
    HRESULT hr = S_OK;
    DWORD er = ERROR_SUCCESS;

    LPWSTR pwz = NULL;
    LPWSTR pwzData = NULL;
    LPWSTR pwzTarget = NULL;
    DWORD dwAttributes = 0;

    DWORD *prgProcessIds = NULL;
    DWORD cProcessIds = 0;

    //
    // initialize
    //
    hr = WcaInitialize(hInstall, "WixCloseApplicationsDeferred");
    ExitOnFailure(hr, "failed to initialize");

    hr = WcaGetProperty(L"CustomActionData", &pwzData);
    ExitOnFailure(hr, "failed to get CustomActionData");

    WcaLog(LOGMSG_TRACEONLY, "CustomActionData: %S", pwzData);

    pwz = pwzData;

    //
    // loop through all the passed in data
    //
    while (pwz && *pwz)
    {
        hr = WcaReadStringFromCaData(&pwz, &pwzTarget);
        ExitOnFailure(hr, "failed to process CustomActionData");
        hr = WcaReadIntegerFromCaData(&pwz, reinterpret_cast<int*>(&dwAttributes));
        ExitOnFailure(hr, "failed to processCustomActionData");

        WcaLog(LOGMSG_VERBOSE, "Checking for App: %S Attributes: %d", pwzTarget, dwAttributes);

        //
        // If we find that an app that we need closed is still runing, require a
        // reboot and bail since we can do no more than require *anohter* 
        // reboot.
        //
        ProcFindAllIdsFromExeName(pwzTarget, &prgProcessIds, &cProcessIds);
        if (0 < cProcessIds)
        {
            WcaLog(LOGMSG_VERBOSE, "App: %S found running, requiring a reboot.", pwzTarget);

            WcaDeferredActionRequiresReboot();
            break;
        }

        hr = WcaProgressMessage(COST_CLOSEAPP, FALSE);
        ExitOnFailure(hr, "failed to send progress message");
    }

LExit:
    if (prgProcessIds)
    {
        MemFree(prgProcessIds);
    }

    ReleaseStr(pwzTarget);
    ReleaseStr(pwzData);

    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}
