//-------------------------------------------------------------------------------------------------
// <copyright file="BuildOperation.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the BuildOperation enum.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;

    /// <summary>
    /// Enumerates the different types of build operations that can be performed on a project.
    /// </summary>
    public enum BuildOperation
    {
        /// <summary>
        /// Performs a clean build, which removes any intermediate and project output.
        /// </summary>
        Clean,

        /// <summary>
        /// Performs an incremental build.
        /// </summary>
        Build,

        /// <summary>
        /// Performs a full rebuild, which is a clean followed by a build.
        /// </summary>
        Rebuild,
    }
}
