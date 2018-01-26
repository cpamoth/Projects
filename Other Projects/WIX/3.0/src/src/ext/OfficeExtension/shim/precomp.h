// <copyright file="precomp.h" company="Microsoft">
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//  Connection proxy header.
// </summary>
//
#pragma once

#include <windows.h>
#include <strsafe.h>

#include <mscoree.h>
#import <mscorlib.tlb> auto_rename raw_interfaces_only high_property_prefixes("_get","_put","_putref") rename_namespace("ClrNamespace") // for _AppDomain.  Used to communicate with the default app domain from unmanaged code
#import "libid:AC0714F2-3D04-11D1-AE7D-00A0C90F26F4" raw_interfaces_only named_guids rename_namespace("AddinNamespace") //The following #import imports the MSADDNDR.dl typelib which we need for IDTExtensibility2.

using namespace ClrNamespace;
using namespace AddinNamespace;

#include "dutil.h"
#include "memutil.h"
#include "resrutil.h"
#include "strutil.h"
#include "UnknownImpl.h"

#include "appsynup.h"

#include "ClrLoader.h"
#include "ConnectProxy.h"
#include "ClassFactory.h"
#include "UpdateThread.h"

#include "resource.h"