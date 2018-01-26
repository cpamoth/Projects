#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="precomp.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Precompiled header for Server scheduling CustomActions
// </summary>
//-------------------------------------------------------------------------------------------------

#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0500
#endif

#include <windows.h>
#include <msiquery.h>
#include <strsafe.h>

#include <lm.h>        // NetApi32.lib
#include <xenroll.h> // ICEnroll2
#include <certsrv.h> // ICertRequest
#include <cguid.h>
#include <oledberr.h>
#include <sqloledb.h>
#include <accctrl.h>
#include <aclapi.h>
#include <Dsgetdc.h>

#include <winperf.h>    // PerfMon counter header file.
#include <loadperf.h>   // PerfMon counter header file.

#include "wcautil.h"
#include "fileutil.h"
#include "memutil.h"
#include "metautil.h"
#include "perfutil.h"
#include "strutil.h"

#include "CustomMsiErrors.h"

#include "..\inc\sca.h"
#include "..\inc\scacost.h"

#include "scaapppool.h"
#include "scacert.h"
#include "scadb.h"
#include "scafilter.h"
#include "scaiis.h"
#include "scamimemap.h"
#include "scahttpheader.h"
#include "scaproperty.h"
#include "scassl.h"
#include "scasmb.h"
#include "scasqlstr.h"
#include "scaweb.h"
#include "scawebdir.h"
#include "scaweblog.h"
#include "scawebsvcext.h"
#include "scavdir.h"
