#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scawebprop.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    IIS Web Directory Property functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

#include "scauser.h"
 
// global sql queries provided for optimization
extern LPCWSTR vcsWebDirPropertiesQuery;


// structs
struct SCA_WEB_PROPERTIES
{
	WCHAR wzKey[MAX_DARWIN_KEY + 1];

	int iAccess;

	int iAuthorization;
	BOOL fHasUser;
	SCA_USER scau;
	BOOL fIIsControlledPassword;

	BOOL fLogVisits;
	BOOL fIndex;

	BOOL fHasDefaultDoc;
	WCHAR wzDefaultDoc[MAX_DARWIN_COLUMN + 1];

	BOOL fHasHttpExp;
	WCHAR wzHttpExp[MAX_DARWIN_COLUMN + 1];

	BOOL fAspDetailedError;

	int iCacheControlMaxAge;

	BOOL fHasCacheControlCustom;
	WCHAR wzCacheControlCustom[MAX_DARWIN_COLUMN + 1];

	BOOL fNoCustomError;

	int iAccessSSLFlags;

	WCHAR wzAuthenticationProviders[MAX_DARWIN_COLUMN + 1];
};
 

// prototypes
HRESULT ScaGetWebDirProperties(LPCWSTR pwzProperties, SCA_WEB_PROPERTIES* pswp);

HRESULT ScaWriteWebDirProperties(IMSAdminBase* piMetabase, LPCWSTR wzRootOfWeb, 
                                 SCA_WEB_PROPERTIES* pswp);

