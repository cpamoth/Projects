//--------------------------------------------------------------------------------------------------
// <copyright file="AddFileDialogType.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Enumerates the different types of Add File dialogs that can be shown.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;

    /// <summary>
    /// Enumerates the different types of Add File dialogs that can be shown.
    /// </summary>
    public enum AddFileDialogType
    {
        /// <summary>
        /// Represents the standard "Add New File" Visual Studio dialog.
        /// </summary>
        AddNew,

        /// <summary>
        /// Represents the standard "Add Existing File" Visual Studio dialog.
        /// </summary>
        AddExisting,
    }
}