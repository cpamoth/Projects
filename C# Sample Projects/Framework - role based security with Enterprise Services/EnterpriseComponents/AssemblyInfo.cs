using System;
using System.Reflection;
using System.Runtime.InteropServices;
// This using lets you use Attributes like "ApplicationNameAttribute"
// and "ApplicationActivation" (see below). Remember that you must set a
// Reference to this namespace in order to Import it.
using System.EnterpriseServices;

// The SecurityRoleAttribute lets you add role-based security to this
// assembly. It accepts parameters including the role name, a
// description, and a boolean indicating whether the Everyone group
// should be added to this role. Choosing true is convenient for testing
// purposes, since it gives the Everyone group access to the component,
// but be sure to make it false before you deploy.
//
// See the Supplier class for details on how to control access via

// these roles. Note that user accounts must be added to these roles
// manually in the COM+ Explorer once the application has been added to
// the COM+ Catalog.
[assembly: SecurityRoleAttribute("Managers", Description="Managers have complete access", SetEveryoneAccess=false)]
[assembly: SecurityRoleAttribute("Clerks", Description="Clerks have limited access", SetEveryoneAccess=false)]
// The ApplicationAccessControlAttribute lets you control where access
// checks are performed. The options are:
//:
//Application: Enable access checks only at the process level. No
//     access checks are made at the component, interface, or method
//     level.
// ApplicationComponent: Enable access checks at every level on
//     calls into the application.

[assembly: ApplicationAccessControlAttribute(AccessChecksLevel=AccessChecksLevelOption.ApplicationComponent)]
// The ApplicationNameAttribute specifies the Name for the COM+
// application this assembly will be a part of. When the assembly is
// first invoked, the COM+ application will be created if it doesn't yet
// exist, provided the calling application has Administrator privileges.
[assembly: ApplicationNameAttribute("C# How-To Implement Role-Based Security in Enterprise Services")]
// The ApplicationIDAttribute provides COM+ with a GUID the
// application identifier.
[assembly: ApplicationIDAttribute("52E273A0-0154-4E28-B176-650A6FBF17C5")]
// The ApplicationActivation attribute specifies where assembly
// components are loaded when they're activated.
//
// Library : components run in the creator's process
// Server : components run in a system process, dllhost.exe.
[assembly: ApplicationActivation(ActivationOption.Library)]
// The DescriptionAttribute shows up in the application's properties in
// the COM+ Explorer.
[assembly: DescriptionAttribute("C# How-To: Implement Enterprise Services Role-Based Security")]
// The AssemblyKeyFile attribute identifies the file containing the
// Public/private key pair we're using to sign the assembly. It's
// located in the Application folder, so we must indicate that it's two
// levels above the assembly itself.
//
// The .snk file was generated at the command line with sn.exe, like
// this: sn -k KeyFile.snk
[assembly: AssemblyKeyFile("..\\..\\KeyFile.snk")]
[assembly: AssemblyTitle("C# How-To: Implement Role-Based Security in Enterprise Services")]
[assembly: AssemblyDescription("Microsoft C# How-To: Implement Role-Based Security in Enterprise Services")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Microsoft C# How To: 2002")]
[assembly: AssemblyCopyright("Copyright  2002 Microsoft Corporation.  All rights reserved.")]
[assembly: CLSCompliant(true)]
[assembly: AssemblyVersion("1.0.0.0")]
