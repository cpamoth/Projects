//-------------------------------------------------------------------------------------------------
// <copyright file="BuildAction.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Enumerates the different types of build actions a node can have.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;

    /// <summary>
    /// Enumerates the different types of build actions a node can have.
    /// </summary>
    public enum BuildAction
    {
        /// <summary>The node is part of the project, but will not be part of the build.</summary>
        None,

        /// <summary>The node will be part of the build.</summary>
        Compile,
    }
}