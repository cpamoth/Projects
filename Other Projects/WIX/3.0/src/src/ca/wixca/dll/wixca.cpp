//-------------------------------------------------------------------------------------------------
// <copyright file="wcautil.cpp" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Windows Installer XML CustomAction DllMain function.
// </summary>
//-------------------------------------------------------------------------------------------------
#include "precomp.h"

/********************************************************************
DllMain - standard entry point for all WiX CustomActions

********************************************************************/
extern "C" BOOL WINAPI DllMain(
    IN HINSTANCE hInst,
    IN ULONG ulReason,
    IN LPVOID)
{
    switch(ulReason)
    {
    case DLL_PROCESS_ATTACH:
        WcaGlobalInitialize(hInst);
        break;

    case DLL_PROCESS_DETACH:
        WcaGlobalFinalize();
        break;
    }

    return TRUE;
}

