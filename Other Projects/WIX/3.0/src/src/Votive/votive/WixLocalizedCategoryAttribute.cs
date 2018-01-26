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
    using System.ComponentModel;

    /// <summary>
    /// Subclasses <see cref="CategoryAttribute"/> to allow for localized strings retrieved
    /// from the resource assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class WixLocalizedCategoryAttribute : CategoryAttribute
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLocalizedCategoryAttribute"/> class.
        /// </summary>
        /// <param name="id">The string identifier to get.</param>
        public WixLocalizedCategoryAttribute(string id)
            : base(id)
        {
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Looks up the localized name of the specified category.
        /// </summary>
        /// <param name="value">The identifer for the category to look up.</param>
        /// <returns>The localized name of the category, or null if a localized name does not exist.</returns>
        protected override string GetLocalizedString(string value)
        {
            return WixStrings.ResourceManager.GetString(value);
        }
    }
}