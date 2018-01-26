//-------------------------------------------------------------------------------------------------
// <copyright file="main.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
// Setup executable for ClickThrough.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"

#define INVALID_LCID -1


enum CONFIGURE_ACTION
{
    CONFIGURE_ACTION_NONE,
    CONFIGURE_ACTION_INSTALL,
    CONFIGURE_ACTION_RECACHE,
    CONFIGURE_ACTION_MINOR_UPGRADE,
    CONFIGURE_ACTION_MAJOR_UPGRADE,
    CONFIGURE_ACTION_PATCH,
    CONFIGURE_ACTION_UNINSTALL,
};


typedef struct
{
    WCHAR wzProductCode[39];

    WCHAR wzTempPath[MAX_PATH];
    WCHAR wzCachedPath[MAX_PATH];

    BOOL bCached;
    BOOL bInstalled;
} SETUP_ROLLBACK_PACKAGE;


HINSTANCE vhInstance = NULL;
LPCSTR vwzLicenseText = NULL;

#if 0
HRESULT LaunchAndWaitForExecutable(
    __in LPCWSTR wzExecutablePath,
    __in LPCWSTR wzExecutableArgs,
    __in BOOL bInstall
    )
{
    HRESULT hr = S_OK;

    WCHAR wzCommandLine[MAX_PATH];
    STARTUPINFOW startupInfo = { 0 };
    PROCESS_INFORMATION procInfo = { 0 };

    hr = ::StringCchPrintfW(wzCommandLine, countof(wzCommandLine), L"%s %s", wzExecutablePath, wzExecutableArgs ? wzExecutableArgs : L"");
    ExitOnFailure(hr, "Failed to generate commandline for msiexec.exe.");

    startupInfo.cb = sizeof(startupInfo);
    if (!::CreateProcessW(NULL, wzCommandLine, NULL, NULL, FALSE, 0, NULL, NULL, &startupInfo, &procInfo))
    {
        ExitOnLastError1(hr, "Failed to CreateProcess on path: %S", wzExecutablePath);
    }

    if (WAIT_OBJECT_0 != ::WaitForSingleObject(procInfo.hProcess, INFINITE))
    {
        ExitOnFailure1(hr = E_UNEXPECTED, "Unexpected terminated process: %S", wzExecutablePath);
    }

LExit:
    if (procInfo.hProcess)
    {
        ::CloseHandle(procInfo.hProcess);
    }

    if (procInfo.hThread)
    {
        ::CloseHandle(procInfo.hThread);
    }

    return hr;
}
#endif


HRESULT ConfigureMsi(
    __in LPCWSTR wzProductCode,
    __in LPCWSTR wzMsiPath,
    __in LPCWSTR wzMsiInstallArgs,
    __in CONFIGURE_ACTION action,
    __in BOOL fShowUI,
    __in BOOL fIgnoreFailures
    )
{
    Assert(wzProductCode || wzMsiPath);

    HRESULT hr = S_OK;
    DWORD er = ERROR_SUCCESS;

    DWORD dwUiLevel = INSTALLUILEVEL_BASIC; // use at least basic UI so the user knows something is going on.
    INSTALLUI_HANDLERW pfnExternalUI = NULL;
    LPWSTR pwzLogFile = NULL;
    LPWSTR pwzProperties = NULL;
    LPVOID pvErrorMessage = NULL;

    //
    // setup the external UI handler and logging
    //
    if (fShowUI)
    {
        dwUiLevel = INSTALLUILEVEL_FULL; // use Full UI
    }
    else if (fIgnoreFailures)
    {
        dwUiLevel |= INSTALLUILEVEL_PROGRESSONLY; // don't throw any scary error dialogs
    }

    ::MsiSetInternalUI(static_cast<INSTALLUILEVEL>(dwUiLevel), NULL);

    //pfnExternalUI = ::MsiSetExternalUIW(pfnInstallEngineCallback, INSTALLLOGMODE_PROGRESS|INSTALLLOGMODE_FATALEXIT|INSTALLLOGMODE_ERROR
    //    |INSTALLLOGMODE_WARNING|INSTALLLOGMODE_USER|INSTALLLOGMODE_INFO
    //    |INSTALLLOGMODE_RESOLVESOURCE|INSTALLLOGMODE_OUTOFDISKSPACE
    //    |INSTALLLOGMODE_ACTIONSTART|INSTALLLOGMODE_ACTIONDATA
    //    |INSTALLLOGMODE_COMMONDATA|INSTALLLOGMODE_PROGRESS|INSTALLLOGMODE_INITIALIZE
    //    |INSTALLLOGMODE_TERMINATE|INSTALLLOGMODE_SHOWDIALOG, pvCallbackContext);

    //hr = FileCreateTemp(L"setup.wix", L"log", &pwzLogFile, NULL);
    //if (SUCCEEDED(hr))
    //{
    //    er = ::MsiEnableLogW(INSTALLLOGMODE_FATALEXIT|INSTALLLOGMODE_ERROR|INSTALLLOGMODE_WARNING
    //        |INSTALLLOGMODE_USER|INSTALLLOGMODE_INFO|INSTALLLOGMODE_RESOLVESOURCE
    //        |INSTALLLOGMODE_OUTOFDISKSPACE|INSTALLLOGMODE_ACTIONSTART|INSTALLLOGMODE_ACTIONDATA
    //        |INSTALLLOGMODE_COMMONDATA|INSTALLLOGMODE_PROPERTYDUMP|INSTALLLOGMODE_VERBOSE,
    //        pwzLogFile, 0);
    //    hr = HRESULT_FROM_WIN32(er);
    //    TraceOnFailure1(hr, "failed to set logging: %S", pwzLogFile);
    //}

    // Ignore all failures setting up the UI handler and
    // logging, they aren't important enough to abort the
    // install attempt.
    hr = S_OK;

    // set up our properties
    hr = StrAllocString(&pwzProperties, wzMsiInstallArgs ? wzMsiInstallArgs : L"", 0);
    ExitOnFailure1(hr, "Failed to allocate string for MSI install arguments: %S", wzMsiInstallArgs);

    //
    // Do the actual action.
    //
    switch (action)
    {
    case CONFIGURE_ACTION_MAJOR_UPGRADE: __fallthrough;
    case CONFIGURE_ACTION_INSTALL:
        er = ::MsiInstallProductW(wzMsiPath, pwzProperties);
        hr = HRESULT_FROM_WIN32(er);
        break;
    case CONFIGURE_ACTION_RECACHE:
        hr = StrAllocConcat(&pwzProperties, L" REINSTALLMODE=\"vdmus\"", 0);
        ExitOnFailure(hr, "failed to format property: REINSTALLMODE");

        //hr = ::MsiSourceListClearAllW(wzProductCode, NULL, 0);
        //hr = HRESULT_FROM_WIN32(hr);
        //ExitOnFailure(hr, "failed to clear MSI source list");

        //hr = StrAllocString(&wzSourceDirectory, pwi->wzMsiPath, 0);
        //ExitOnFailure(hr, "failed to allocate source buffer");
        //if (wzLastSlash = wcsrchr(wzSourceDirectory, L'\\'))
        //{
        //    *wzLastSlash = 0;
        //}
        //hr = ::MsiSourceListAddSourceW(wzProductCode, NULL, 0, wzSourceDirectory);
        //hr = HRESULT_FROM_WIN32(hr);
        //ExitOnFailure(hr, "failed to add to MSI source list");

        //hr = ::MsiSourceListForceResolutionW(wzProductCode, NULL, 0);
        //hr = HRESULT_FROM_WIN32(hr);
        //ExitOnFailure(hr, "failed to force MSI source resolution");

        er = ::MsiConfigureProductExW(wzProductCode, 0, INSTALLSTATE_DEFAULT, pwzProperties);
        hr = HRESULT_FROM_WIN32(er);
        break;
    case CONFIGURE_ACTION_MINOR_UPGRADE:
        hr = StrAllocConcat(&pwzProperties, L" REINSTALL=ALL REINSTALLMODE=\"vomus\"", 0);
        ExitOnFailure(hr, "failed to format properties: REINSTALL and REINSTALLMODE");

        er = ::MsiInstallProductW(wzMsiPath, pwzProperties);
        hr = HRESULT_FROM_WIN32(er);
        break;
    case CONFIGURE_ACTION_UNINSTALL:
        er = ::MsiConfigureProductW(wzProductCode, 0, INSTALLSTATE_ABSENT);
        hr = HRESULT_FROM_WIN32(er);
        break;
    }

    if (FAILED(hr))
    {
        if (fIgnoreFailures)
        {
            hr = S_FALSE; // success, but did not install
        }
        else
        {
            DWORD cchErrorMessage = 0;

            switch(er)
            {
            case ERROR_INSTALL_PACKAGE_REJECTED: __fallthrough;
            case ERROR_CREATE_FAILED:
                cchErrorMessage = ::FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, hr, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPWSTR)&pvErrorMessage, 0, NULL);
                if (0 < cchErrorMessage)
                {
                    ::MessageBoxExW(NULL, (LPWSTR)pvErrorMessage, L"Install Error", MB_ICONERROR | MB_OK, 0);
                }
                break;
            }
        }
    }

LExit:
    if (pfnExternalUI)  // unset the UI handler
    {
        ::MsiSetExternalUIW(pfnExternalUI, 0, NULL);
    }

    if (pvErrorMessage)
    {
        ::LocalFree(pvErrorMessage);
    }

    ReleaseStr(pwzProperties);
    ReleaseStr(pwzLogFile);

    return hr;
}


HRESULT CacheMsi(
    __in SETUP_INSTALL_PACKAGE* pPackage,
    __in LPCWSTR wzTempMsiPath,
    __in BOOL bAllUsers,
    __out LPWSTR* ppwzCachedMsiPath
    )
{
    Assert(ppwzCachedMsiPath);

    HRESULT hr = S_OK;
    DWORD dwAppDataFlags = bAllUsers ? CSIDL_COMMON_APPDATA : CSIDL_LOCAL_APPDATA;
    WCHAR wzLocalAppPath[MAX_PATH];
    WCHAR wzProductCode[39];
    WCHAR wzVersion[36];

    // actually cache the MSI now
    hr = ::SHGetFolderPathW(NULL, dwAppDataFlags | CSIDL_FLAG_CREATE, NULL, SHGFP_TYPE_CURRENT, wzLocalAppPath);
    ExitOnFailure(hr, "Failed to get local app data folder.");

    hr = ::StringFromGUID2(pPackage->guidProductCode, wzProductCode, countof(wzProductCode));
    ExitOnFailure(hr, "Failed to convert ProductCode GUID to string.");

    hr = ::StringCchPrintfW(wzVersion, countof(wzVersion), L"%d.%d.%d.%d", pPackage->dwVersionMajor >> 16, pPackage->dwVersionMajor & 0xFFFF,
                                                                           pPackage->dwVersionMinor >> 16, pPackage->dwVersionMinor & 0xFFFF);
    ExitOnFailure(hr, "Failed to format version string.");

    hr = StrAllocFormatted(ppwzCachedMsiPath, L"%ls\\Applications\\Cache\\%lsv%ls.msi", wzLocalAppPath, wzProductCode, wzVersion);
    ExitOnFailure(hr, "Failed to allocate formatted.");

    hr = FileEnsureMove(wzTempMsiPath, *ppwzCachedMsiPath, TRUE, TRUE);
    if (FAILED(hr) && TRUE == bAllUsers)
    {
       // Attempted to place file in All Users Cache, but failed
       hr = CacheMsi(pPackage, wzTempMsiPath, !bAllUsers, ppwzCachedMsiPath);
       ExitOnFailure(hr, "Failed to move MSI to cache, second chance.");
    }
    else
    {
        ExitOnFailure(hr, "Failed to move MSI to cache.");
    }

LExit:
    return hr;
}


HRESULT DetectMsi(
    __in LPCWSTR wzProductCode,
    __in DWORD dwMajorVersion,
    __in DWORD dwMinorVersion,
    __out CONFIGURE_ACTION* pAction
    )
{
    Assert(wzProductCode);
    Assert(pAction);

    HRESULT hr = S_OK;
    UINT er = ERROR_SUCCESS;
    WCHAR wzInstalledVersion[36];
    DWORD cchInstalledVersion = countof(wzInstalledVersion);
    DWORD dwInstalledMajorVersion = 0;
    DWORD dwInstalledMinorVersion = 0;
    CONFIGURE_ACTION action = CONFIGURE_ACTION_NONE;

    er = ::MsiGetProductInfoW(wzProductCode, L"VersionString", wzInstalledVersion, &cchInstalledVersion);
    if (ERROR_SUCCESS == er)
    {
        hr = FileVersionFromString(wzInstalledVersion, &dwInstalledMajorVersion, &dwInstalledMinorVersion);
        ExitOnFailure2(hr, "Failed to convert version: %S to DWORDs for ProductCode: %S", wzInstalledVersion, wzProductCode);

        //
        // Take a look at the version and determine if this is a potential
        // minor upgrade, a major upgrade (just install) otherwise, there
        // is a newer version so no work necessary.
        //
        // minor upgrade "10.1.X.X" > "10.0.X.X" or "10.3.2.0" > "10.3.1.0"
        if (((dwMajorVersion >> 16) == (dwInstalledMajorVersion >> 16) && dwMajorVersion > dwInstalledMajorVersion) ||
            (dwMajorVersion == dwInstalledMajorVersion && dwMinorVersion > dwInstalledMinorVersion))
        {
            action = CONFIGURE_ACTION_MINOR_UPGRADE;
        }
        else if (dwMajorVersion > dwInstalledMajorVersion) // major upgrade "11.X.X.X" > "10.X.X.X"
        {
            Assert((dwMajorVersion >> 16) > (dwInstalledMajorVersion >> 16));
            action = CONFIGURE_ACTION_MAJOR_UPGRADE;
        }
        else // newer version installed "14.X.X.X" < "15.X.X.X", skip
        {
            action = CONFIGURE_ACTION_NONE;
        }

        hr = S_OK;
    }
    else if (ERROR_UNKNOWN_PRODUCT == er || ERROR_UNKNOWN_PROPERTY == er)
    {
        action = CONFIGURE_ACTION_INSTALL;
        hr = S_OK;
    }
    else
    {
        hr = HRESULT_FROM_WIN32(er);
        ExitOnFailure1(hr, "Failed to get product information for ProductCode: %S", wzProductCode);
    }

    *pAction = action;

LExit:
    return hr;
}


HRESULT ExtractMsi(
    __in SETUP_INSTALL_PACKAGE* pPackage,
    __out LPWSTR* ppwzTempMsiPath
    )
{
    HRESULT hr = S_OK;
    DWORD cchTempMsiPath = MAX_PATH;

    // Drop the package into the temp directory.
    hr = StrAlloc(ppwzTempMsiPath, cchTempMsiPath);
    ExitOnFailure(hr, "Failed to allocate string for temp path.");

    hr = DirCreateTempPath(L"CT", *ppwzTempMsiPath, cchTempMsiPath);
    ExitOnFailure(hr, "Failed to generate a temporary filename.");

    hr = ResExportDataToFile(pPackage->szSource, *ppwzTempMsiPath, CREATE_ALWAYS);
    ExitOnFailure1(hr, "Failed to write MSI package to %S.", *ppwzTempMsiPath);

LExit:
    return hr;
}


HRESULT ExtractMsiToDir(
    __in LPCWSTR pwzMsiOutputPath
    )
{
    HRESULT hr = S_OK;
    SETUP_INSTALL_CHAIN* pSetupChain = NULL;
    DWORD cbSetupChain = 0;
    LPWSTR pwzFullFilePath = NULL;

    hr = DirEnsureExists(pwzMsiOutputPath, NULL);
    ExitOnFailure(hr, "Failed creating/verifying directory.");

    hr = ResReadData("MANIFEST", (LPVOID*)&pSetupChain, &cbSetupChain);
    ExitOnFailure(hr, "Failed to get manifest for setup chain.");

    for (BYTE i = 0; i < pSetupChain->cPackages; ++i)
    {
        SETUP_INSTALL_PACKAGE* pPackage = pSetupChain->rgPackages + i;

        hr = StrAllocString(&pwzFullFilePath, pwzMsiOutputPath, 0);
        ExitOnFailure(hr, "Failed to allocate string and copy path for full file path.");

        hr = StrAllocConcat(&pwzFullFilePath, L"\\", 0);
        ExitOnFailure(hr, "Failed to concatenate filename into full file path.");

        hr = StrAllocConcat(&pwzFullFilePath, pPackage->wzFilename, 0);
        ExitOnFailure(hr, "Failed to concatenate filename into full file path.");

        hr = ResExportDataToFile(pPackage->szSource, pwzFullFilePath, CREATE_ALWAYS);
        ExitOnFailure1(hr, "Failed to write msi package to %S.", pwzFullFilePath);
    }

LExit:
    ReleaseStr(pwzFullFilePath);
    return hr;
}


HRESULT ExtractMst(
    __in SETUP_INSTALL_TRANSFORM* pTransform,
    __out LPWSTR* ppwzTempMstPath
    )
{
    HRESULT hr = S_OK;
    DWORD cchTempMsiPath = MAX_PATH;

    // Drop the package into the temp directory.
    hr = StrAlloc(ppwzTempMstPath, cchTempMsiPath);
    ExitOnFailure(hr, "Failed to allocate string for temp path.");

    hr = DirCreateTempPath(L"MST", *ppwzTempMstPath, cchTempMsiPath);
    ExitOnFailure(hr, "Failed to generate a temporary filename.");

    hr = ResExportDataToFile(pTransform->szTransform, *ppwzTempMstPath, CREATE_ALWAYS);
    ExitOnFailure1(hr, "Failed to write MSI transform to %S.", *ppwzTempMstPath);

LExit:
    return hr;
}


void UISetupComplete(
    __in HRESULT hrStatus
    )
{
    HRESULT hr = S_OK;
    LPWSTR pwzProductName = NULL;
    LPWSTR pwzFormat = NULL;
    LPWSTR pwzDisplay = NULL;
    UINT uiType = MB_OK | MB_SETFOREGROUND;

    if (HRESULT_FROM_WIN32(ERROR_INSTALL_USEREXIT) == hrStatus)
    {
        ExitFunction();
    }

    hr = ResReadString(vhInstance, SETUP_RESOURCE_IDS_PRODUCTNAME, &pwzProductName);
    ExitOnFailure(hr, "Failed to load product name.");

    if (SUCCEEDED(hrStatus))
    {
        hr = ResReadString(vhInstance, IDS_SUCCESSFORMAT, &pwzFormat);
        ExitOnFailure(hr, "Failed to load success format.");

        uiType |= MB_ICONINFORMATION;
    }
    else if (HRESULT_FROM_WIN32(ERROR_SUCCESS_REBOOT_REQUIRED) == hrStatus ||
              HRESULT_FROM_WIN32(ERROR_SUCCESS_RESTART_REQUIRED) == hrStatus ||
              HRESULT_FROM_WIN32(ERROR_SUCCESS_REBOOT_INITIATED) == hrStatus)
    {
        hr = ResReadString(vhInstance, IDS_REBOOTFORMAT, &pwzFormat);
        ExitOnFailure(hr, "Failed to load reboot format.");

        uiType |= MB_ICONWARNING;
    }
    else
    {
        hr = ResReadString(vhInstance, IDS_FAILEDFORMAT, &pwzFormat);
        ExitOnFailure(hr, "Failed to load failed format.");

        uiType |= MB_ICONERROR;
    }

    hr = StrAllocFormatted(&pwzDisplay, pwzFormat, pwzProductName);
    ExitOnFailure1(hr, "Failed to set display with product name: %S", pwzProductName);

    ::MessageBoxW(NULL, pwzDisplay, pwzProductName, uiType);

LExit:
    ReleaseStr(pwzDisplay);
    ReleaseStr(pwzFormat);
    ReleaseStr(pwzProductName);

    return;
}

HRESULT UIUpdateInstallButtonState(
    __in HWND hwndDlg
    )
{
    HRESULT hr = S_OK;
    HWND hwndInstall = NULL;
    BOOL bAccepted = ::IsDlgButtonChecked(hwndDlg, IDC_CHECKACCEPT);

    hwndInstall = ::GetDlgItem(hwndDlg, IDC_INSTALL);
    ExitOnNullWithLastError(hwndInstall, hr, "Failed to get install button.");

    ::EnableWindow(hwndInstall, bAccepted);

LExit:
    return hr;
}


HRESULT UIInitDialog(
    __in HWND hwndDlg
    )
{
    HRESULT hr = S_OK;

    HWND hwndLicense = NULL;
    LPSTR pwzLicenseText = NULL;
    LPWSTR pwzProductName = NULL;
    LPWSTR pwzIntroductionFormat = NULL;
    LPWSTR pwzIntroduction = NULL;
    LRESULT lResult = 0;

    // First, enable/disable the Install button based on the state of
    // the license accept check box.
    UIUpdateInstallButtonState(hwndDlg);

    // Second, populate the license text box with the license.
    if (!::SetDlgItemText(hwndDlg, IDC_RTFLICENSE, vwzLicenseText))
    {
        hr = E_UNEXPECTED;
    }

    // Third, get the product name and update the dialog title with it.
    hr = ResReadString(vhInstance, SETUP_RESOURCE_IDS_PRODUCTNAME, &pwzProductName);
    ExitOnFailure(hr, "Failed to load product name.");

    if (!::SetWindowTextW(hwndDlg, pwzProductName))
    {
        ExitWithLastError1(hr, "Failed to set dialog title to product name: %S", pwzProductName);
    }

    // Finally, write the license agreement introduction.
    hr = ResReadString(vhInstance, IDS_INTRODUCTIONFORMAT, &pwzIntroductionFormat);
    ExitOnFailure(hr, "Failed to load introduction format.");

    hr = StrAllocFormatted(&pwzIntroduction, pwzIntroductionFormat, pwzProductName);
    ExitOnFailure1(hr, "Failed to set introduction with product name: %S", pwzProductName);

    if (!::SetDlgItemTextW(hwndDlg, IDC_INTRODUCTION, pwzIntroduction))
    {
        ExitWithLastError1(hr, "Failed to set introduction to product name: %S", pwzProductName);
    }

LExit:
    ReleaseStr(pwzIntroduction);
    ReleaseStr(pwzIntroductionFormat);
    ReleaseStr(pwzProductName);

    return hr;
}


INT_PTR CALLBACK UIDialogProc(
    __in HWND hwndDlg,
    __in UINT uMsg,
    __in WPARAM wParam,
    __in LPARAM lParam
    )
{
    HRESULT hr = S_OK;
    BOOL bProcessed = FALSE;

    switch (uMsg)
    {
        case WM_INITDIALOG:
            hr = UIInitDialog(hwndDlg);
            ExitOnFailure(hr, "Failed to initialize UI.");

            bProcessed = TRUE;
            break;

        case WM_COMMAND:
            switch (LOWORD(wParam))
            {
            case IDC_CHECKACCEPT:
                hr = UIUpdateInstallButtonState(hwndDlg);
                ExitOnFailure(hr, "Failed to update install button state.");

                bProcessed = TRUE;
                break;

            case IDC_INSTALL:
                ::EndDialog(hwndDlg, 0);

                bProcessed = TRUE;
                break;

            case IDCANCEL:
                ::EndDialog(hwndDlg, ERROR_INSTALL_USEREXIT);

                bProcessed = TRUE;
                break;
            }
    }

LExit:
    return bProcessed;
}


HRESULT DisplayLicenseText(
    __in LPCSTR wzLicenseText
    )
{
    HRESULT hr = S_OK;
    HWND hwnd = NULL;
    INT_PTR iResult = 0;

    // Initialize common controls.
    ::InitCommonControls();

    // Load rich edit before trying to create the controls.
    HMODULE richEd = ::LoadLibraryW(L"Riched20.dll");
    ExitOnNullWithLastError(richEd, hr, "Failed to load Rich Edit Control.");

    vwzLicenseText = wzLicenseText;

    // Create the dialog and show it.
    iResult = ::DialogBoxW(vhInstance, MAKEINTRESOURCEW(IDD_LICENSEDIALOG), NULL, UIDialogProc);
    if (-1 == iResult)
    {
        ExitWithLastError(hr, "Failed to create dialog box.");
    }

    hr = HRESULT_FROM_WIN32(iResult);

LExit:
    vwzLicenseText = NULL;
    return hr;
}


BOOL MatchLanguage(
    __in DWORD dwLang,
    __in DWORD dwTransformLang
    )
{
    BOOL fLanguageMatch = FALSE;
    if (INVALID_LCID == dwLang)
    {
        LANGID userLangID = ::GetUserDefaultUILanguage();
        dwLang = MAKELCID(userLangID, SORT_DEFAULT);
    }

    // Convert Languages as appropriate
    if (0x04 == PRIMARYLANGID(dwLang))
    {
        switch(dwLang)
        {
        // Match Simplified Chinese
        case 0x0804: __fallthrough;
        case 0x1004:
            dwLang = 0x0804;
            break;

        // Match Traditional Chinese
        case 0x0404: __fallthrough;
        case 0x0c04: __fallthrough;
        case 0x1404:
            dwLang = 0x0404;
            break;
        }

        fLanguageMatch = (dwLang == dwTransformLang);
    }
    else
    {
        fLanguageMatch = (dwLang == PRIMARYLANGID(dwTransformLang));
    }

    return fLanguageMatch;
}


HRESULT ApplyTransform(
    __in LPCWSTR wzMsiPath,
    __in LPCWSTR wzMstPath
    )
{
    HRESULT hr = S_OK;
    DWORD er = ERROR_SUCCESS;
    PMSIHANDLE hMsi;

    er = ::MsiOpenDatabaseW(wzMsiPath, reinterpret_cast<LPCWSTR>(MSIDBOPEN_TRANSACT), &hMsi);
    hr = HRESULT_FROM_WIN32(er);
    ExitOnFailure(hr, "Failed to open MSI database");

    er = ::MsiDatabaseApplyTransformW(hMsi, wzMstPath, MSITRANSFORM_ERROR_CHANGECODEPAGE);
    hr = HRESULT_FROM_WIN32(er);
    ExitOnFailure(hr, "Failed to apply transform");

    er = ::MsiDatabaseCommit(hMsi);
    hr = HRESULT_FROM_WIN32(er);
    ExitOnFailure(hr, "Failed to apply transform");

LExit:
    return hr;
}


HRESULT ProcessSetupChain(
    __in_opt LPCSTR wzLicenseText,
    __in_opt LPCWSTR wzMsiArgs,
    __in_opt DWORD dwLocaleId
    )
{
    HRESULT hr = S_OK;

    SETUP_INSTALL_CHAIN* pSetupChain = NULL;
    DWORD cbSetupChain = 0;

    SETUP_ROLLBACK_PACKAGE* prgRollbackPackages = NULL;
    BYTE cRollbackPackages = 0;

    LPWSTR pwzTempMsiPath = NULL;
    LPWSTR pwzTempMstPath = NULL;
    LPWSTR pwzCachedMsiPath = NULL;
    LPCWSTR wzMsiPath;
    LPVOID pvErrorMessage = NULL;

    // If we have a license to display, let's go do that.
    if (wzLicenseText)
    {
        hr = DisplayLicenseText(wzLicenseText);
        ExitOnFailure(hr, "Failed while displaying license text.");
    }

    hr = ResReadData("MANIFEST", (LPVOID*)&pSetupChain, &cbSetupChain);
    ExitOnFailure(hr, "Failed to get manifest for setup chain.");

    // Create enough space to track rollback information for all possible packages in the chain.
    prgRollbackPackages = static_cast<SETUP_ROLLBACK_PACKAGE*>(MemAlloc(sizeof(SETUP_ROLLBACK_PACKAGE) * pSetupChain->cPackages, TRUE));
    ExitOnNull(prgRollbackPackages, hr, E_OUTOFMEMORY, "Failed to allocate rollback chain.");

    // If we have transforms, extract them to temp path
    for(BYTE j = 0; j < pSetupChain->cTransforms; ++j)
    {
        SETUP_INSTALL_TRANSFORM* pTransform = pSetupChain->rgTransforms + j;

        // Compare Locales
        if(MatchLanguage(dwLocaleId , pTransform->dwLocaleId))
        {
            // Get the MST out of the setup.exe.
            hr = ExtractMst(pTransform, &pwzTempMstPath);
            ExitOnFailure(hr, "Failed to extract transform to temp path.");
        }
    }

    for (BYTE i = 0; i < pSetupChain->cPackages; ++i)
    {
        CONFIGURE_ACTION action = CONFIGURE_ACTION_INSTALL; // assume this will be a simple install
        BOOL bDeleteTempPath = FALSE;

        SETUP_INSTALL_PACKAGE* pPackage = pSetupChain->rgPackages + i;
        SETUP_ROLLBACK_PACKAGE* pRollbackPackage = prgRollbackPackages + cRollbackPackages;

        hr = ::StringFromGUID2(pPackage->guidProductCode, pRollbackPackage->wzProductCode, countof(pRollbackPackage->wzProductCode));
        ExitOnFailure(hr, "Failed to convert ProductCode GUID to string.");

        //
        // Try to detect the MSI to determine what action is necessary.
        //
        hr = DetectMsi(pRollbackPackage->wzProductCode, pPackage->dwVersionMajor, pPackage->dwVersionMinor, &action);
        ExitOnFailure(hr, "Failed during MSI detection.");

        //
        // If there is no action, move on to the next package.  If we need to do a minor upgrade
        // but the package isn't configured to allow minor upgrades, bail
        //
        if (CONFIGURE_ACTION_NONE == action)
        {
            continue;
        }
        else if (CONFIGURE_ACTION_MINOR_UPGRADE == action && !(pPackage->dwAttributes & SETUP_INSTALL_CHAIN_MINOR_UPGRADE_ALLOWED))
        {
            hr = HRESULT_FROM_WIN32(ERROR_PRODUCT_VERSION);
            DWORD cchErrorMessage = ::FormatMessageW(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, hr, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPWSTR)&pvErrorMessage, 0, NULL);
            if (0 < cchErrorMessage)
            {
                ::MessageBoxExW(NULL, (LPWSTR)pvErrorMessage, L"Install Error", MB_ICONERROR | MB_OK, 0);
            }
            ExitOnFailure(hr, "Cannot upgrade previously installed package.");
        }

        ++cRollbackPackages;

        // Get the MSI out of the setup.exe.
        hr = ExtractMsi(pPackage, &pwzTempMsiPath);
        ExitOnFailure(hr, "Failed to extract MSI to temp path.");

        bDeleteTempPath = TRUE; // extracted temp MSI path, so we'll need to clean it up at the end of this loop

        hr = ::StringCchCopyW(pRollbackPackage->wzTempPath, countof(pRollbackPackage->wzTempPath), pwzTempMsiPath);
        ExitOnFailure(hr, "Failed to copy temp path into rollback chain.");

        // If use transform is set for this package, and transform exists use transform
        if(pwzTempMstPath && (pPackage->dwAttributes & SETUP_INSTALL_CHAIN_USE_TRANSFORM))
        {
            // Apply transform to database
            hr = ApplyTransform(pwzTempMsiPath, pwzTempMstPath);
        }

        if (pPackage->dwAttributes & SETUP_INSTALL_CHAIN_CACHE)
        {
            // Cache the package to our application cache.
            hr = CacheMsi(pPackage, pwzTempMsiPath, pPackage->dwAttributes & SETUP_INSTALL_CHAIN_ALLUSERS, &pwzCachedMsiPath);
            ExitOnFailure(hr, "Failed to cache MSI file.");

            bDeleteTempPath = FALSE; // temp MSI has been cached (moved), so we shouldn't delete the temp file any longer.

            wzMsiPath = pwzCachedMsiPath;

            hr = ::StringCchCopyW(pRollbackPackage->wzCachedPath, countof(pRollbackPackage->wzCachedPath), pwzCachedMsiPath);
            ExitOnFailure(hr, "Failed to copy cached path into rollback chain.");

            pRollbackPackage->bCached = TRUE;
        }
        else
        {
            wzMsiPath = pwzTempMsiPath;
        }

        // Install the cached package.
        hr = ConfigureMsi(pRollbackPackage->wzProductCode, wzMsiPath, wzMsiArgs, action, pPackage->dwAttributes & SETUP_INSTALL_CHAIN_SHOW_UI, pPackage->dwAttributes & SETUP_INSTALL_CHAIN_IGNORE_FAILURES);
        ExitOnFailure1(hr, "Failed to launch and wait for cached MSI: %ls", wzMsiPath);

        if (S_FALSE == hr)
        {
            if (pRollbackPackage->bCached)
            {
                ::DeleteFileW(pRollbackPackage->wzCachedPath);

                *pRollbackPackage->wzCachedPath = L'\0';
                pRollbackPackage->bCached = FALSE;
            }
        }
        else
        {
            pRollbackPackage->bInstalled = TRUE;
        }

        if (bDeleteTempPath)
        {
            ::DeleteFileW(pwzTempMsiPath);
        }
    }

LExit:
    if (FAILED(hr) &&
        HRESULT_FROM_WIN32(ERROR_SUCCESS_REBOOT_REQUIRED) != hr &&
        HRESULT_FROM_WIN32(ERROR_SUCCESS_RESTART_REQUIRED) != hr &&
        HRESULT_FROM_WIN32(ERROR_SUCCESS_REBOOT_INITIATED) != hr)
    {
        for (BYTE i = 0; i < cRollbackPackages; ++i)
        {
            SETUP_ROLLBACK_PACKAGE* pRollbackPackage = prgRollbackPackages + i;

            if (pRollbackPackage->bInstalled)
            {
                ConfigureMsi(pRollbackPackage->wzProductCode, NULL, NULL, CONFIGURE_ACTION_UNINSTALL, FALSE, TRUE);
            }

            if (pRollbackPackage->wzCachedPath && *pRollbackPackage->wzCachedPath)
            {
                ::DeleteFileW(pRollbackPackage->wzCachedPath);
            }

            if (pRollbackPackage->wzTempPath && *pRollbackPackage->wzTempPath)
            {
                ::DeleteFileW(pRollbackPackage->wzTempPath);
            }
        }
    }

    if (prgRollbackPackages)
    {
        MemFree(prgRollbackPackages);
    }

    if (pvErrorMessage)
    {
        ::LocalFree(pvErrorMessage);
    }
    ReleaseStr(pwzTempMsiPath);
    ReleaseStr(pwzCachedMsiPath);

    //UISetupComplete(hr);

    return hr;
}


int WINAPI WinMain(
    __in HINSTANCE hInstance,
    __in HINSTANCE hPrevInstance,
    __in LPSTR lpCmdLine,
    __in int nCmdShow
    )
{
    HRESULT hr = S_OK;
    int argc = 0;
    LPWSTR *argv = NULL;

    BOOL bDropMsi = FALSE;
    LPCWSTR wzDropDir = NULL;
    LPCWSTR wzMsiArgs = NULL;
    LPSTR wzLicenseText = NULL;
    DWORD cbLicenseText = 0;
    DWORD dwLocaleId = INVALID_LCID;

    vhInstance = hInstance;

    argv = ::CommandLineToArgvW(::GetCommandLineW(), &argc);
    ExitOnNullWithLastError(argv, hr, "Failed to get command line.");

    //
    // Process command-line arguments.
    //
    for (int i = 1; i < argc; i++)
    {
        if (argv[i][0] == L'-' || argv[i][0] == L'/')
        {
            if (CSTR_EQUAL == ::CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, &argv[i][1], -1, L"out", -1))
            {
                if (i + 1 >= argc)
                {
                    ExitOnFailure(hr = E_INVALIDARG, "Must specify a filename with the /out switch.");
                }

                bDropMsi = TRUE;
                wzDropDir = argv[++i];
            }
            else if (CSTR_EQUAL == ::CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, &argv[i][1], -1, L"msicl", -1))
            {
                wzMsiArgs = argv[++i];
                break;
            }
            else if (CSTR_EQUAL == ::CompareStringW(LOCALE_INVARIANT, NORM_IGNORECASE, &argv[i][1], -1, L"lang", -1))
            {
                if (i + 1 >= argc)
                {
                    ExitOnFailure(hr = E_INVALIDARG, "Must specify a Locale ID with the /lang switch.");
                }
                dwLocaleId = _wtoi(argv[++i]);
            }
            else
            {
                ExitOnFailure1(hr = E_INVALIDARG, "Bad commandline argument: %S", argv[i]);
            }
        }
        else
        {
            ExitOnFailure1(hr = E_INVALIDARG, "Bad commandline argument: %S", argv[i]);
        }
    }

    //
    // Do what we came here to do.
    //
    if (bDropMsi)
    {
        hr = ExtractMsiToDir(wzDropDir);
        ExitOnFailure(hr, "Failure in extracting msi.");
    }
    else // Default is to install the package.
    {
        hr = ResReadData(MAKEINTRESOURCE(SETUP_RESOURCE_IDS_LICENSETEXT), (LPVOID*)&wzLicenseText, &cbLicenseText);
        if (HRESULT_FROM_WIN32(ERROR_RESOURCE_NAME_NOT_FOUND) == hr)
        {
            // No license is okay.
            AssertSz(!wzLicenseText, "License text should be null if the resource was not found.");
            hr = S_OK;
        }
        ExitOnFailure(hr, "Failed to load license text.");

        hr = ProcessSetupChain(wzLicenseText, wzMsiArgs, dwLocaleId);
        ExitOnFailure(hr, "Failed to process setup chain.");
    }

LExit:
    // UISetupComplete(hr);

    return SCODE_CODE(hr);
}
