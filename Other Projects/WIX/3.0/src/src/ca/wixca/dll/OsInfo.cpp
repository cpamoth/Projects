//-------------------------------------------------------------------------------------------------
// <copyright file="OsInfo.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Sets properties for OS product/suite information and standard directories
// </summary>
//-------------------------------------------------------------------------------------------------

#include "precomp.h"

// constants we'll pick up from later SDKs
#define SM_TABLETPC    86
#define SM_MEDIACENTER 87
#define SM_STARTER     88
#define SM_SERVERR2    89
#define VER_SUITE_WH_SERVER 0x00008000

/********************************************************************
WixQueryOsInfo - entry point for WixQueryOsInfo custom action

 Called as Type 1 custom action (DLL from the Binary table) from 
 Windows Installer to set properties that identify OS information 
 and predefined directories
********************************************************************/
extern "C" UINT __stdcall WixQueryOsInfo(
    __in MSIHANDLE hInstall
    )
{
    HRESULT hr = S_OK;
    DWORD er = ERROR_SUCCESS;
    OSVERSIONINFOEX ovix = {0};

    hr = WcaInitialize(hInstall, "WixQueryOsInfo");
    ExitOnFailure(hr, "WixQueryOsInfo failed to initialize");

    // identify product suites
    ovix.dwOSVersionInfoSize = sizeof OSVERSIONINFOEX;
    ::GetVersionEx(LPOSVERSIONINFO(&ovix));

    if (VER_SUITE_SMALLBUSINESS == (ovix.wSuiteMask & VER_SUITE_SMALLBUSINESS))
    {
        WcaSetIntProperty(L"WIX_SUITE_SMALLBUSINESS", 1);
    }

    if (VER_SUITE_ENTERPRISE == (ovix.wSuiteMask & VER_SUITE_ENTERPRISE))
    {
        WcaSetIntProperty(L"WIX_SUITE_ENTERPRISE", 1);
    }

    if (VER_SUITE_BACKOFFICE == (ovix.wSuiteMask & VER_SUITE_BACKOFFICE))
    {
        WcaSetIntProperty(L"WIX_SUITE_BACKOFFICE", 1);
    }

    if (VER_SUITE_COMMUNICATIONS == (ovix.wSuiteMask & VER_SUITE_COMMUNICATIONS))
    {
        WcaSetIntProperty(L"WIX_SUITE_COMMUNICATIONS", 1);
    }

    if (VER_SUITE_TERMINAL == (ovix.wSuiteMask & VER_SUITE_TERMINAL))
    {
        WcaSetIntProperty(L"WIX_SUITE_TERMINAL", 1);
    }

    if (VER_SUITE_SMALLBUSINESS_RESTRICTED == (ovix.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED))
    {
        WcaSetIntProperty(L"WIX_SUITE_SMALLBUSINESS_RESTRICTED", 1);
    }

    if (VER_SUITE_EMBEDDEDNT == (ovix.wSuiteMask & VER_SUITE_EMBEDDEDNT))
    {
        WcaSetIntProperty(L"WIX_SUITE_EMBEDDEDNT", 1);
    }

    if (VER_SUITE_DATACENTER == (ovix.wSuiteMask & VER_SUITE_DATACENTER))
    {
        WcaSetIntProperty(L"WIX_SUITE_DATACENTER", 1);
    }

    if (VER_SUITE_SINGLEUSERTS == (ovix.wSuiteMask & VER_SUITE_SINGLEUSERTS))
    {
        WcaSetIntProperty(L"WIX_SUITE_SINGLEUSERTS", 1);
    }

    if (VER_SUITE_PERSONAL == (ovix.wSuiteMask & VER_SUITE_PERSONAL))
    {
        WcaSetIntProperty(L"WIX_SUITE_PERSONAL", 1);
    }

    if (VER_SUITE_BLADE == (ovix.wSuiteMask & VER_SUITE_BLADE))
    {
        WcaSetIntProperty(L"WIX_SUITE_BLADE", 1);
    }

    if (VER_SUITE_EMBEDDED_RESTRICTED == (ovix.wSuiteMask & VER_SUITE_EMBEDDED_RESTRICTED))
    {
        WcaSetIntProperty(L"WIX_SUITE_EMBEDDED_RESTRICTED", 1);
    }

    if (VER_SUITE_SECURITY_APPLIANCE == (ovix.wSuiteMask & VER_SUITE_SECURITY_APPLIANCE))
    {
        WcaSetIntProperty(L"WIX_SUITE_SECURITY_APPLIANCE", 1);
    }

    if (VER_SUITE_STORAGE_SERVER == (ovix.wSuiteMask & VER_SUITE_STORAGE_SERVER))
    {
        WcaSetIntProperty(L"WIX_SUITE_STORAGE_SERVER", 1);
    }

    if (VER_SUITE_COMPUTE_SERVER == (ovix.wSuiteMask & VER_SUITE_COMPUTE_SERVER))
    {
        WcaSetIntProperty(L"WIX_SUITE_COMPUTE_SERVER", 1);
    }

    if (VER_SUITE_WH_SERVER == (ovix.wSuiteMask & VER_SUITE_WH_SERVER))
    {
        WcaSetIntProperty(L"WIX_SUITE_WH_SERVER", 1);
    }

    // only for XP and later
    if (5 < ovix.dwMajorVersion || (5 == ovix.dwMajorVersion && 0 < ovix.dwMinorVersion))
    {
        if (::GetSystemMetrics(SM_SERVERR2))
        {
            WcaSetIntProperty(L"WIX_SUITE_SERVERR2", 1);
        }

        if (::GetSystemMetrics(SM_MEDIACENTER))
        {
            WcaSetIntProperty(L"WIX_SUITE_MEDIACENTER", 1);
        }

        if (::GetSystemMetrics(SM_STARTER))
        {
            WcaSetIntProperty(L"WIX_SUITE_STARTER", 1);
        }

        if (::GetSystemMetrics(SM_TABLETPC))
        {
            WcaSetIntProperty(L"WIX_SUITE_TABLETPC", 1);
        }
    }

LExit:
    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}

/********************************************************************
WixQueryOsDirs - entry point for WixQueryOsDirs custom action

 Called as Type 1 custom action (DLL from the Binary table) from 
 Windows Installer to set properties that identify predefined directories
********************************************************************/
extern "C" UINT __stdcall WixQueryOsDirs(
    __in MSIHANDLE hInstall
    )
{
    HRESULT hr = S_OK;
    DWORD er = ERROR_SUCCESS;

    hr = WcaInitialize(hInstall, "WixQueryOsDirs");
    ExitOnFailure(hr, "WixQueryOsDirs failed to initialize");

    // get the paths of the CSIDLs that represent real paths and for which MSI
    // doesn't yet have standard folder properties
    WCHAR path[MAX_PATH];
    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_ADMINTOOLS, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_ADMINTOOLS", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_ALTSTARTUP, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_ALTSTARTUP", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_CDBURN_AREA, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_CDBURN_AREA", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_ADMINTOOLS, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_ADMINTOOLS", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_ALTSTARTUP, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_ALTSTARTUP", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_DOCUMENTS, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_DOCUMENTS", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_FAVORITES, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_FAVORITES", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_MUSIC, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_MUSIC", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_PICTURES, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_PICTURES", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COMMON_VIDEO, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COMMON_VIDEO", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_COOKIES, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_COOKIES", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_DESKTOP, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_DESKTOP", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_HISTORY, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_HISTORY", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_INTERNET_CACHE, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_INTERNET_CACHE", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_MYMUSIC, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_MYMUSIC", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_MYPICTURES, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_MYPICTURES", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_MYVIDEO, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_MYVIDEO", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_NETHOOD, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_NETHOOD", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_PERSONAL, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_PERSONAL", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_PRINTHOOD, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_PRINTHOOD", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_PROFILE, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_PROFILE", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_RECENT, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_RECENT", path);
    }

    if (ERROR_SUCCESS == (hr = ::SHGetFolderPathW(NULL, CSIDL_RESOURCES, NULL, SHGFP_TYPE_CURRENT, path)))
    {
        WcaSetProperty(L"WIX_DIR_RESOURCES", path);
    }

LExit:
    if (FAILED(hr))
        er = ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}
