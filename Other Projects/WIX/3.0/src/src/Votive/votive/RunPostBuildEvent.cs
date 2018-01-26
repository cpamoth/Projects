//--------------------------------------------------------------------------------------------------
// <copyright file="RunPostBuildEvent.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the RunPostBuildEvent class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;

    /// <summary>
    /// Enumerates the values of the RunPostBuildEvent MSBuild property.
    /// </summary>
    public enum RunPostBuildEvent
    {
        /// <summary>
        /// The post-build event is always run.
        /// </summary>
        Always,

        /// <summary>
        /// The post-build event is only run when the build succeeds.
        /// </summary>
        OnBuildSuccess,

        /// <summary>
        /// The post-build event is only run if the project's output is updated.
        /// </summary>
        OnOutputUpdated,
    }
}
