#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scawebaddr.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    IIS Web Address functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

// global sql queries provided for optimization
extern LPCWSTR vcsAddressQuery;

// structs
struct SCA_WEB_ADDRESS
{
	WCHAR wzKey [MAX_DARWIN_KEY + 1];

	WCHAR wzIP[MAX_DARWIN_COLUMN + 1];
	int iPort;
	WCHAR wzHeader[MAX_DARWIN_COLUMN + 1];
	BOOL fSecure;
};


// prototypes
HRESULT ScaGetWebAddress(MSIHANDLE hViewAddresses, LPCWSTR wzAddress, 
                         SCA_WEB_ADDRESS* pswa);

