//-------------------------------------------------------------------------------------------------
// <copyright file="setupbld.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
// Setup executable builder header for ClickThrough.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once


typedef struct
{
    LPCWSTR wzSourcePath;

    BOOL fPrivileged;
    BOOL fCache;
    BOOL fShowUI;
    BOOL fIgnoreFailures;
    BOOL fMinorUpgrade;
    BOOL fLink;
    BOOL fUseTransform;
} CREATE_SETUP_PACKAGE;

typedef struct
{
    LPCWSTR wzSourcePath;
    DWORD dwLocaleId;
} CREATE_SETUP_TRANSFORMS;
