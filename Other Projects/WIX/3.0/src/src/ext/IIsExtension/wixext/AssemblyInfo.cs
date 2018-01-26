//-------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The assembly information for the Windows Installer XML Toolset Internet Information Services Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Tools.WindowsInstallerXml;
using Microsoft.Tools.WindowsInstallerXml.Extensions;
using Microsoft.Tools.WindowsInstallerXml.Tools;

[assembly: AssemblyTitle("WiX Toolset IIS Extension")]
[assembly: AssemblyDescription("Windows Installer XML Toolset Internet Information Services Extension")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Windows Installer XML Toolset")]
[assembly: AssemblyCopyright("Copyright © Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: AssemblyDefaultHeatExtension(typeof(IIsHeatExtension))]
[assembly: AssemblyDefaultWixExtension(typeof(IIsExtension))]
