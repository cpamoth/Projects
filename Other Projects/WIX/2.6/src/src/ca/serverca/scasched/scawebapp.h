#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scawebapp.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    IIS Web Application functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

#include "scaapppool.h"
#include "scawebappext.h"

// global sql queries provided for optimization
extern LPCWSTR vcsWebApplicationQuery;
const int MAX_APP_NAME = 32;

// structs
struct SCA_WEB_APPLICATION
{
	WCHAR wzName[MAX_APP_NAME + 1];

	int iIsolation;
	BOOL fAllowSessionState;
	int iSessionTimeout;
	BOOL fBuffer;
	BOOL fParentPaths;

	WCHAR wzDefaultScript[MAX_DARWIN_COLUMN + 1];
	int iScriptTimeout;
	BOOL fServerDebugging;
	BOOL fClientDebugging;
	WCHAR wzAppPool[MAX_DARWIN_COLUMN + 1];

	SCA_WEB_APPLICATION_EXTENSION* pswappextList;
};


// prototypes
HRESULT ScaGetWebApplication(MSIHANDLE hViewApplications, 
                             LPCWSTR pwzApplication, SCA_WEB_APPLICATION* pswapp);

HRESULT ScaWriteWebApplication(IMSAdminBase* piMetabase, LPCWSTR wzRootOfWeb, 
                               SCA_WEB_APPLICATION* pswapp, SCA_APPPOOL * psapList);

