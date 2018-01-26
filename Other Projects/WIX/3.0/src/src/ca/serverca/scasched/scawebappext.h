#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scawebappext.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Functions for dealing with Web Application Extensions in Server CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

// structs
struct SCA_WEB_APPLICATION_EXTENSION
{
    WCHAR wzExtension[MAX_DARWIN_COLUMN + 1];

    WCHAR wzVerbs[MAX_DARWIN_COLUMN + 1];
    WCHAR wzExecutable[MAX_DARWIN_COLUMN + 1];
    int iAttributes;

    SCA_WEB_APPLICATION_EXTENSION* pswappextNext;
};


// prototypes
HRESULT ScaWebAppExtensionsRead(
    __in LPCWSTR wzApplication,
    __inout SCA_WEB_APPLICATION_EXTENSION** ppswappextList
    );

HRESULT ScaWebAppExtensionsWrite(
    __in IMSAdminBase* piMetabase,
    __in LPCWSTR wzRootOfWeb,
    __in SCA_WEB_APPLICATION_EXTENSION* pswappextList
    );

void ScaWebAppExtensionsFreeList(
    __in SCA_WEB_APPLICATION_EXTENSION* pswappextList
    );
