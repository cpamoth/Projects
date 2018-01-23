using System;
using System.Reflection;
using System.Runtime.InteropServices;
// This using lets you use Attributes like "ApplicationNameAttribute"
// and "ApplicationActivation" (see below). Remember that you must set a
// Reference to this namespace in order to Import it.
using System.EnterpriseServices;


// The ApplicationNameAttribute specifies the Name for the COM+

// application this assembly will be a part of. When the assembly is
// first invoked, the COM+ application will be created if it doesn't yet
// exist, provided the calling application has Administrator privileges.
[assembly: ApplicationNameAttribute("C# How-To Create Enterprise Services Components")]
// The ApplicationIDAttribute provides COM+ with a GUID the
// application identifier.
[assembly: ApplicationIDAttribute("F3F4E0DA-6712-4AA9-9F48-871A81FD2844")]

// The ApplicationActivation attribute specifies where assembly
// components are loaded when they're activated.
//
// Library : components run in the creator's process

// Server :
//components run in a system process, dllhost.exe.
[assembly: ApplicationActivation(ActivationOption.Library)]
// The DescriptionAttribute shows up in the application's properties in
// :the COM+ Explorer.
[assembly: DescriptionAttribute("C# How-To Data Access Components")]
// The AssemblyKeyFile attribute identifies the file containing the
// Public/private key pair we're using to sign the assembly. It's
// located in the Application folder, so we must indicate that it's two
// levels above the assembly itself.
//
// The .snk file was generated at the command line with sn.exe, like
// this: sn -k KeyFile.snk
[assembly: AssemblyKeyFile("..\\..\\KeyFile.snk")]
[assembly: AssemblyTitle("C# How-To: Create Enterprise Services Components")]
[assembly: AssemblyDescription("Microsoft C# How-To: Create Enterprise Services Components")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Microsoft C# How To: 2002")]
[assembly: AssemblyCopyright("Copyright  2002 Microsoft Corporation. All rights reserved.")]
[assembly: CLSCompliant(true)]
[assembly: AssemblyVersion("1.0.0.0")]

