//-------------------------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Global suppression of inappropriate code analysis tools.
// </summary>
//-------------------------------------------------------------------------------------------------

// MSI-defined types
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Scope = "type", Target = "Microsoft.Tools.WindowsInstallerXml.Msi.InstallMessage")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Scope = "type", Target = "Microsoft.Tools.WindowsInstallerXml.Msi.InstallUILevels")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Scope = "type", Target = "Microsoft.Tools.WindowsInstallerXml.Msi.OpenDatabase")]

// .NET 2.0 requirement
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Scope = "member", Target = "Microsoft.Tools.WindowsInstallerXml.Intermediate.Load(System.String,Microsoft.Tools.WindowsInstallerXml.TableDefinitionCollection,System.Boolean,System.Boolean):Microsoft.Tools.WindowsInstallerXml.Intermediate")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Scope = "member", Target = "Microsoft.Tools.WindowsInstallerXml.Library.Load(System.IO.Stream,System.Uri,Microsoft.Tools.WindowsInstallerXml.TableDefinitionCollection,System.Boolean,System.Boolean):Microsoft.Tools.WindowsInstallerXml.Library")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Scope = "member", Target = "Microsoft.Tools.WindowsInstallerXml.Localization.Load(System.IO.Stream,System.Uri,Microsoft.Tools.WindowsInstallerXml.TableDefinitionCollection,System.Boolean):Microsoft.Tools.WindowsInstallerXml.Localization")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Scope = "member", Target = "Microsoft.Tools.WindowsInstallerXml.Output.Load(System.IO.Stream,System.Uri,System.Boolean,System.Boolean):Microsoft.Tools.WindowsInstallerXml.Output")]

// .NET 2.0 requirement
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Scope = "member", Target = "Microsoft.Tools.WindowsInstallerXml.Cab.WixCreateCab.handle")]
