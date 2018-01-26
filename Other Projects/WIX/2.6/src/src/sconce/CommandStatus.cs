//-------------------------------------------------------------------------------------------------
// <copyright file="CommandStatus.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Enumeration for the different states that a menu command can be in.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using Microsoft.VisualStudio.OLE.Interop;

    /// <summary>
    /// Enumeration for the different states that a menu command can be in.
    /// </summary>
    public enum CommandStatus
    {
        Unhandled = -1,
        NotSupportedOrEnabled = 0,
        Supported = OLECMDF.OLECMDF_SUPPORTED,
        Enabled = OLECMDF.OLECMDF_ENABLED,
        SupportedAndEnabled = OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED,
        Hidden = OLECMDF.OLECMDF_INVISIBLE,
    }
}