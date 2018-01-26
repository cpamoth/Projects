#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scauser.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    User functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

// structs
struct SCA_GROUP
{
    WCHAR wzKey[MAX_DARWIN_KEY + 1];
    WCHAR wzComponent[MAX_DARWIN_KEY + 1];

    WCHAR wzDomain[MAX_DARWIN_COLUMN + 1];
    WCHAR wzName[MAX_DARWIN_COLUMN + 1];

    SCA_GROUP *psgNext;
};

struct SCA_USER
{
    WCHAR wzKey[MAX_DARWIN_KEY + 1];
    WCHAR wzComponent[MAX_DARWIN_KEY + 1];
    INSTALLSTATE isInstalled;
    INSTALLSTATE isAction;

    WCHAR wzDomain[MAX_DARWIN_COLUMN + 1];
    WCHAR wzName[MAX_DARWIN_COLUMN + 1];
    WCHAR wzPassword[MAX_DARWIN_COLUMN + 1];
    INT iAttributes;

    SCA_GROUP *psgGroups;

    SCA_USER *psuNext;
};


// prototypes
HRESULT __stdcall ScaGetUser(
    __in LPCWSTR wzUser, 
    __out SCA_USER* pscau
    );
HRESULT __stdcall ScaGetGroup(
    __in LPCWSTR wzGroup, 
    __out SCA_GROUP* pscag
    );
HRESULT ScaBuildDomainUserName(
    __out_ecount(cchDest) WCHAR* wzDest, 
    __in int cchDest, 
    __in SCA_USER* pscau
    );
void ScaUserFreeList(
    __in SCA_USER* psuList
    );
void ScaGroupFreeList(
    __in SCA_GROUP* psgList
    );
HRESULT ScaUserRead(
    __inout SCA_USER** ppsuList
    );
HRESULT ScaUserExecute(
    __in SCA_USER *psuList
    );
