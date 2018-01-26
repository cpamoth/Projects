//-------------------------------------------------------------------------------------------------
// <copyright file="setup.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
// Setup executable builder header for ClickThrough.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once

#define SETUP_MAX_FILE_LENGTH                        64

#define SETUP_RESOURCE_IDS_PRODUCTNAME               51001
#define SETUP_RESOURCE_IDS_LICENSETEXT               51002
#define SETUP_RESOURCE_IDB_BACKGROUND                51003

#define SETUP_INSTALL_CHAIN_ALLUSERS                0x01 // package installs to ALLUSERS
#define SETUP_INSTALL_CHAIN_PRIVILEGED              0x02 // package requires elevated privileges to install
#define SETUP_INSTALL_CHAIN_CACHE                   0x04 // cache the package before installing
#define SETUP_INSTALL_CHAIN_SHOW_UI                 0x08 // show the UI of the embedded MSI
#define SETUP_INSTALL_CHAIN_IGNORE_FAILURES         0x10 // ignore any install failures
#define SETUP_INSTALL_CHAIN_MINOR_UPGRADE_ALLOWED   0x20 // the embedded MSI can do a minor upgrade
#define SETUP_INSTALL_CHAIN_LINK                    0x30 // embedded resource is only a link to package
#define SETUP_INSTALL_CHAIN_USE_TRANSFORM           0x40 // Uses the transforms included in setup

typedef struct
{
    CHAR szSource[12];
    WCHAR wzFilename[SETUP_MAX_FILE_LENGTH];
    GUID guidProductCode;
    DWORD dwVersionMajor;
    DWORD dwVersionMinor;
    DWORD dwAttributes; // any combination of the #define SETUP_INSTALL_CHAIN_
    // DWORD rgHash[8];
} SETUP_INSTALL_PACKAGE;

typedef struct
{
    CHAR szTransform[12];
    DWORD dwLocaleId;
} SETUP_INSTALL_TRANSFORM;


typedef struct
{
    BYTE dwRevision;
    BYTE cPackages;
    // Total number of transforms in package
    BYTE cTransforms;
    SETUP_INSTALL_PACKAGE rgPackages[1];
    SETUP_INSTALL_TRANSFORM rgTransforms[1];
} SETUP_INSTALL_CHAIN;
