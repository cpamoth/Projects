#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scamimemap.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    IIS Mime Map functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

enum eMimeMapParentType	{ mmptVDir = 1 };

struct SCA_MIMEMAP
{
	// iis configuation information
	WCHAR wzMimeMap[MAX_DARWIN_KEY + 1];
	int iParentType;
	WCHAR wzParentValue[MAX_DARWIN_KEY + 1];
	WCHAR wzMimeType[MAX_DARWIN_KEY + 1];
	WCHAR wzExtension[MAX_DARWIN_KEY + 1];


	SCA_MIMEMAP* psmmNext;
};


// prototypes

HRESULT __stdcall ScaMimeMapRead(SCA_MIMEMAP** ppsmmList);

HRESULT ScaGetMimeMap(int iParentType, LPCWSTR wzParentValue, SCA_MIMEMAP **psmmList, SCA_MIMEMAP **ppsmmOut);

HRESULT ScaMimeMapCheckList(SCA_MIMEMAP* psmmList);

void ScaMimeMapFreeList(SCA_MIMEMAP* psmmList);

HRESULT ScaWriteMimeMap(IMSAdminBase* piMetabase, LPCWSTR wzRootOfWeb, 
                               SCA_MIMEMAP* psmmList);

