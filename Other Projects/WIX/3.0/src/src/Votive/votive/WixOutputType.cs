//-------------------------------------------------------------------------------------------------
// <copyright file="WixOutputType.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixOutputType enum.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Enumeration for the various output types for a Wix project.
    /// </summary>
    public enum WixOutputType
    {
        /// <summary>
        /// Wix project that builds an MSI file.
        /// </summary>
        Package,

        /// <summary>
        /// Wix project that builds an MSM file.
        /// </summary>
        Module,

        /// <summary>
        /// Wix project that builds a wixlib file.
        /// </summary>
        Library,
    }
}
