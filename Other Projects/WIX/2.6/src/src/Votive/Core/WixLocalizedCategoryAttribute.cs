//--------------------------------------------------------------------------------------------------
// <copyright file="WixLocalizedCategoryAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLocalizedCategoryAttribute class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure;

    /// <summary>
    /// Subclasses <see cref="CategoryAttribute"/> to allow for localized strings retrieved
    /// from the resource assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class WixLocalizedCategoryAttribute : LocalizedCategoryAttribute
    {
        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLocalizedCategoryAttribute"/> class.
        /// </summary>
        /// <param name="id">The string identifier to get.</param>
        public WixLocalizedCategoryAttribute(WixStrings.StringId id)
            : base(id.ToString())
        {
        }
        #endregion
    }
}