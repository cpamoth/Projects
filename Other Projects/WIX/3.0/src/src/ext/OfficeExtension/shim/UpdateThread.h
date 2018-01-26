// <copyright file="UpdateThread.cpp" company="Microsoft">
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//  Background thread for doing update checks header.
// </summary>
//
#pragma once

extern "C" void WINAPI UpdateThreadInitialize();

extern "C" void WINAPI UpdateThreadUninitialize();

extern "C" HRESULT WINAPI UpdateThreadCheck(
    __in LPCWSTR wzAppId,
    __in BOOL fTryExecuteUpdate
    );
