#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scaweblog.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Custom Actions for handling log settings for a particular IIS Website
// </summary>
//-------------------------------------------------------------------------------------------------

struct SCA_WEB_LOG
{
	// iis configuation information
	WCHAR wzLog[MAX_DARWIN_KEY + 1];

	// for specifying the log format
	WCHAR wzFormat[MAX_DARWIN_KEY + 1];
	WCHAR wzFormatGUID[MAX_DARWIN_KEY + 1];
};


// prototypes
HRESULT ScaGetWebLog(
	IMSAdminBase* piMetabase,
	LPCWSTR wzLog,
	SCA_WEB_LOG* pswl
	);
HRESULT ScaWriteWebLog(
	IMSAdminBase* piMetabase,
	LPCWSTR wzRootOfWeb,
	SCA_WEB_LOG *pswl
	);
