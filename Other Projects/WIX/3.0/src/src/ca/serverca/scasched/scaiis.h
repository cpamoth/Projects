#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="scaiis.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    IIS functions for CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

HRESULT ScaMetabaseTransaction(LPCWSTR wzBackup);

HRESULT ScaCreateWeb(IMSAdminBase* piMetabase, LPCWSTR wzWeb, LPCWSTR wzWebBase);

HRESULT ScaCreateApp(IMSAdminBase* piMetabase, LPCWSTR wzWebRoot, 
                     DWORD dwIsolation);

HRESULT ScaCreateMetabaseKey(IMSAdminBase* piMetabase, LPCWSTR wzRootKey, 
                             LPCWSTR wzSubKey);

HRESULT ScaDeleteMetabaseKey(IMSAdminBase* piMetabase, LPCWSTR wzRootKey, 
                             LPCWSTR wzSubKey);

HRESULT ScaWriteMetabaseValue(IMSAdminBase* piMetabase, LPCWSTR wzRootKey, 
                              LPCWSTR wzSubKey, DWORD dwIdentifier, 
                              DWORD dwAttributes, DWORD dwUserType, 
                              DWORD dwDataType, LPVOID pvData);

HRESULT ScaScheduleMetabaseConfiguration();


HRESULT ScaLoadMetabase(IMSAdminBase** piMetabase);