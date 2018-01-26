// <copyright file="ClrLoader.cpp" company="Microsoft">
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//  CLR Loader header.
// </summary>
//
#pragma once

extern "C" void WINAPI ClrLoaderInitialize();

extern "C" void WINAPI ClrLoaderUninitialize();

extern "C" HRESULT WINAPI ClrLoaderCreateInstance(
    __in_opt LPCWSTR wzClrVersion,
    __in LPCWSTR wzAssemblyName,
    __in LPCWSTR wzClassName,
    __in const IID &riid,
    __in void ** ppvObject
    );

extern "C" void WINAPI ClrLoaderDestroyInstance();
