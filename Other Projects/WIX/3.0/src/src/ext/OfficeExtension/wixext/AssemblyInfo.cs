﻿//-------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The assembly information for the Windows Installer XML Toolset Office Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Extensions;

[assembly: AssemblyTitle("WiX Toolset Office Extension")]
[assembly: AssemblyDescription("Windows Installer XML Toolset Office Extension")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Windows Installer XML Toolset")]
[assembly: AssemblyCopyright("Copyright © Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
[assembly: AssemblyDefaultClickThroughConsoleAttribute(typeof(OfficeAddinClickThroughConsole))]
[assembly: AssemblyDefaultClickThroughUIAttribute(typeof(OfficeAddinClickThroughUI))]
