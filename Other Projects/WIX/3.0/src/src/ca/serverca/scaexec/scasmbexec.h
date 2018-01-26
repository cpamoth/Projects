#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scasmbexec.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    File share functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

//Structure used to hold the permission User Name pairs
struct SCA_SMBP_USER_PERMS
{
	DWORD nPermissions;
	WCHAR* wzUser;
	//Not adding Password because I can't find anywhere that it is used
};

struct SCA_SMBP  // hungarian ssp
{
	WCHAR* wzKey;
	WCHAR* wzDescription;
	WCHAR* wzComponent;
	WCHAR* wzDirectory;  // full path of the dir to share to

	DWORD dwUserPermissionCount;  //Count of SCA_SMBP_EX_USER_PERMS structures
	SCA_SMBP_USER_PERMS* pUserPerms;
	BOOL fUseIntegratedAuth;
};


HRESULT ScaEnsureSmbExists(SCA_SMBP* pssp);
HRESULT ScaDropSmb(SCA_SMBP* pssp);
