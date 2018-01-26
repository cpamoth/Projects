#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scasqlstr.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    SQL String functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

#include "scauser.h"
#include "scadb.h"

struct SCA_SQLSTR
{
    // darwin information
    WCHAR wzKey[MAX_DARWIN_KEY + 1];
    WCHAR wzComponent[MAX_DARWIN_KEY + 1];
    INSTALLSTATE isInstalled, isAction;

    WCHAR wzSqlDb[MAX_DARWIN_COLUMN + 1];

    BOOL fHasUser;
    SCA_USER scau;

    LPWSTR pwzSql;
    int iAttributes;
    int iSequence; //used to sequence SqlString and SqlScript tables together

    SCA_SQLSTR* psssNext;
};


// prototypes
HRESULT ScaSqlStrsRead(
    __inout SCA_SQLSTR** ppsssList,
    __in SCA_ACTION saAction
    );

HRESULT ScaSqlStrsReadScripts(
    __inout SCA_SQLSTR** ppsssList,
    __in SCA_ACTION saAction
    );

HRESULT ScaSqlStrsInstall(
    __in SCA_DB* psdList,
    __in SCA_SQLSTR* psssList
    );

HRESULT ScaSqlStrsUninstall(
    __in SCA_DB* psdList,
    __in SCA_SQLSTR* psssList
    );

void ScaSqlStrsFreeList(
    __in SCA_SQLSTR* psssList
    );

