//-------------------------------------------------------------------------------------------------
// <copyright file="VsMessageBoxResult.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains Visual Studio message box result values.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;

    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Visual Studio message box result values.
    /// </summary>
    public enum VsMessageBoxResult 
    {
        OK      = 1,
        Cancel  = 2,
        Abort   = 3,
        Retry   = 4,
        Ignore  = 5,
        Yes     = 6,
        No      = 7,
    }
}
