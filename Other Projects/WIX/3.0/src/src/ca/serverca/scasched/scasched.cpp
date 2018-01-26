//-------------------------------------------------------------------------------------------------
// <copyright file="scasched.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Windows Installer XML Server Scheduling CustomAction.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"


/********************************************************************
DllMain - standard entry point for all WiX CustomActions

********************************************************************/
extern "C" BOOL WINAPI DllMain(
    __in HINSTANCE hInst,
    __in ULONG ulReason,
    __in LPVOID
    )
{
    switch(ulReason)
    {
    case DLL_PROCESS_ATTACH:
        WcaGlobalInitialize(hInst);
        break;

    case DLL_PROCESS_DETACH:
        WcaGlobalFinalize();
        break;
    }

    return TRUE;
}


/********************************************************************
ConfigureIIs - CUSTOM ACTION ENTRY POINT for installing IIs settings

********************************************************************/
extern "C" UINT __stdcall ConfigureIIs(
    __in MSIHANDLE hInstall
    )
{
    //AssertSz(FALSE, "debug ConfigureIIs here");
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    BOOL fInitializedCom = FALSE;
    IMSAdminBase* piMetabase = NULL;

    SCA_WEB* pswList = NULL;
    SCA_WEBDIR* pswdList = NULL;
    SCA_VDIR* psvdList = NULL;
    SCA_FILTER* psfList = NULL;
    SCA_APPPOOL *psapList = NULL;
    SCA_MIMEMAP* psmmList = NULL;
    SCA_HTTP_HEADER* pshhList = NULL;
    SCA_PROPERTY *pspList = NULL;
    SCA_WEBSVCEXT* psWseList = NULL;
    SCA_WEB_ERROR* psweList = NULL;

    // initialize
    hr = WcaInitialize(hInstall, "ConfigureIIs");
    ExitOnFailure(hr, "Failed to initialize");

    // check for the prerequsite tables
    if (S_OK != WcaTableExists(L"IIsWebSite") && S_OK != WcaTableExists(L"IIsFilter") && S_OK != WcaTableExists(L"IIsWebServiceExtension"))
    {
        WcaLog(LOGMSG_VERBOSE, "skipping IIs CustomAction, no IIsWebSite table, no IIsFilter table, and no IIsWebServiceExtension table");
        ExitFunction1(hr = S_FALSE);
    }

    hr = ::CoInitialize(NULL);
    ExitOnFailure(hr, "failed to initialize COM");
    fInitializedCom = TRUE;

    // if IIS was uninstalled (thus no IID_IMSAdminBase) allow the
    // user to still uninstall this package by clicking "Ignore"
    do
    {
        hr = ::CoCreateInstance(CLSID_MSAdminBase, NULL, CLSCTX_ALL, IID_IMSAdminBase, (void**)&piMetabase); 
        if (FAILED(hr))
        {
            WcaLog(LOGMSG_STANDARD, "failed to get IID_IMSAdminBase Object");
            er = WcaErrorMessage(msierrIISCannotConnect, hr, INSTALLMESSAGE_ERROR | MB_ABORTRETRYIGNORE, 0);
            switch (er)
            {
            case IDABORT:
                ExitFunction();   // bail with the error result from the CoCreate to kick off a rollback
            case IDRETRY:
                hr = S_FALSE;   // hit me, baby, one more time
                break;
            case IDIGNORE:
                ExitFunction1(hr = S_OK);  // pretend everything is okay and bail
            }
        }
    } while (S_FALSE == hr);

    // make sure the operations below are wrapped in a "transaction"
    hr = ScaMetabaseTransaction(L"ScaConfigureIIs");
    MessageExitOnFailure(hr, msierrIISFailedSchedTransaction, "failed to start transaction");

    // read the msi tables
    hr = ScaWebSvcExtRead(&psWseList);
    MessageExitOnFailure(hr, msierrIISFailedReadWebSvcExt, "failed to read IIsWebServiceExtension table");

    hr = ScaAppPoolRead(&psapList);
    MessageExitOnFailure(hr, msierrIISFailedReadAppPool, "failed to read IIsAppPool table");

    // MimeMap, Error and HttpHeader need to be read before the virtual directory and web read
    hr = ScaMimeMapRead(&psmmList);
    MessageExitOnFailure(hr, msierrIISFailedReadMimeMap, "failed to read IIsMimeMap table");

    hr = ScaHttpHeaderRead(&pshhList);
    MessageExitOnFailure(hr, msierrIISFailedReadHttpHeader, "failed to read IIsHttpHeader table");

    hr = ScaWebErrorRead(&psweList);
    MessageExitOnFailure(hr, msierrIISFailedReadWebError, "failed to read IIsWebError table");

    hr = ScaWebsRead(piMetabase, &pswList, &pshhList, &psweList);
    MessageExitOnFailure(hr, msierrIISFailedReadWebSite, "failed to read IIsWebSite table");

    hr = ScaWebDirsRead(piMetabase, pswList, &pswdList);
    MessageExitOnFailure(hr, msierrIISFailedReadWebDirs, "failed to read IIsWebDir table");

    hr = ScaVirtualDirsRead(piMetabase, pswList, &psvdList, &psmmList, &pshhList, &psweList);
    MessageExitOnFailure(hr, msierrIISFailedReadVDirs, "failed to read IIsWebVirtualDir table");

    hr = ScaFiltersRead(piMetabase, pswList, &psfList);
    MessageExitOnFailure(hr, msierrIISFailedReadFilters, "failed to read IIsFilter table");

    hr = ScaPropertyRead(&pspList);
    MessageExitOnFailure(hr, msierrIISFailedReadProp, "failed to read IIsProperty table");

    // do uninstall actions (order is important!)
    hr = ScaPropertyUninstall(piMetabase, pspList);
    MessageExitOnFailure(hr, msierrIISFailedSchedUninstallProp, "failed to uninstall IIS properties");

    hr = ScaFiltersUninstall(piMetabase, psfList);
    MessageExitOnFailure(hr, msierrIISFailedSchedUninstallFilters, "failed to schedule uninstall of filters");

    hr = ScaVirtualDirsUninstall(piMetabase, psvdList);
    MessageExitOnFailure(hr, msierrIISFailedSchedUninstallVDirs, "failed to schedule uninstall of virtual directories");

    hr = ScaWebDirsUninstall(piMetabase, pswdList);
    MessageExitOnFailure(hr, msierrIISFailedSchedUninstallWebDirs, "failed to schedule uninstall of web directories");

    hr = ScaWebsUninstall(piMetabase, pswList);
    MessageExitOnFailure(hr, msierrIISFailedSchedUninstallWebs, "failed to schedule uninstall of webs");

    hr = ScaAppPoolUninstall(piMetabase, psapList);
    MessageExitOnFailure(hr, msierrIISFailedSchedUninstallAppPool, "failed to schedule uninstall of AppPools");


    // do install actions (order is important!)
    // ScaWebSvcExtCommit contains both uninstall and install actions.
    hr = ScaWebSvcExtCommit(piMetabase, psWseList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallWebSvcExt, "failed to schedule install/uninstall of WebSvcExt");

    hr = ScaAppPoolInstall(piMetabase, psapList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallAppPool, "failed to schedule install of AppPools");

    hr = ScaWebsInstall(piMetabase, pswList, psapList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallWebs, "failed to schedule install of webs");

    hr = ScaWebDirsInstall(piMetabase, pswdList, psapList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallWebDirs, "failed to schedule install of web directories");

    hr = ScaVirtualDirsInstall(piMetabase, psvdList, psapList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallVDirs, "failed to schedule install of virtual directories");

    hr = ScaFiltersInstall(piMetabase, psfList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallFilters, "failed to schedule install of filters");

    hr = ScaPropertyInstall(piMetabase, pspList);
    MessageExitOnFailure(hr, msierrIISFailedSchedInstallProp, "failed to schedule install of properties");

    hr = ScaScheduleMetabaseConfiguration();
    ExitOnFailure(hr, "failed to schedule metabase configuration");

LExit:
    if (psWseList)
    {
        ScaWebSvcExtFreeList(psWseList);
    }

    if (psfList)
    {
        ScaFiltersFreeList(psfList);
    }

    if (psvdList)
    {
        ScaVirtualDirsFreeList(psvdList);
    }

    if (pswdList)
    {
        ScaWebDirsFreeList(pswdList);
    }

    if (pswList)
    {
        ScaWebsFreeList(pswList);
    }

    if (psmmList)
    {
        ScaMimeMapCheckList(psmmList);
        ScaMimeMapFreeList(psmmList);
    }

    if (pshhList)
    {
        ScaHttpHeaderCheckList(pshhList);
        ScaHttpHeaderFreeList(pshhList);
    }

    if (psweList)
    {
        ScaWebErrorCheckList(psweList);
        ScaWebErrorFreeList(psweList);
    }

    if (piMetabase)
    {
        piMetabase->Release();
    }

    if (fInitializedCom)
    {
        ::CoUninitialize();
    }

    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}


/********************************************************************
ConfigureSmb - CUSTOM ACTION ENTRY POINT for installing fileshare settings

********************************************************************/
extern "C" UINT __stdcall ConfigureSmb(
    __in MSIHANDLE hInstall
    )
{
    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    SCA_SMB* pssList = NULL;

    // initialize
    hr = WcaInitialize(hInstall, "ConfigureSmb");
    ExitOnFailure(hr, "Failed to initialize");

    // check to see if necessary tables are specified
    if (WcaTableExists(L"FileShare") != S_OK)
    {
        WcaLog(LOGMSG_VERBOSE, "Skipping SMB CustomAction, no FileShare table");
        ExitFunction1(hr = S_FALSE);
    }

    hr = ScaSmbRead(&pssList);
    ExitOnFailure(hr, "failed to read FileShare table");

    // do uninstall actions.  Order is important! Should be reverse of install.
    hr = ScaSmbUninstall(pssList);
    ExitOnFailure(hr, "failed to uninstall FileShares");

    // do install actions (order is important!)
    hr = ScaSmbInstall(pssList);
    ExitOnFailure(hr, "failed to install FileShares");

LExit:
    if (pssList)
        ScaSmbFreeList(pssList);

    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}


/********************************************************************
ConfigureUsers - CUSTOM ACTION ENTRY POINT for installing users

********************************************************************/
extern "C" UINT __stdcall ConfigureUsers(
    __in MSIHANDLE hInstall
    )
{
    //AssertSz(0, "Debug ConfigureUsers");

    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;

    SCA_USER* psuList = NULL;

    // initialize
    hr = WcaInitialize(hInstall, "ConfigureUsers");
    ExitOnFailure(hr, "Failed to initialize");

    hr = ScaUserRead(&psuList);
    ExitOnFailure(hr, "failed to read User table");

    hr = ScaUserExecute(psuList);
    ExitOnFailure(hr, "failed to add/remove User actions");

LExit:
    if (psuList)
    {
        ScaUserFreeList(psuList);
    }

    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}
