//--------------------------------------------------------------------------------------------------
// <copyright file="LocalizedCategoryAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Subclasses CategoryAttribute to allow for localized strings retrieved from the resource assembly.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Subclasses <see cref="CategoryAttribute"/> to allow for localized strings retrieved
    /// from the resource assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedCategoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the string resource to get.</param>
        public LocalizedCategoryAttribute(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedCategoryAttribute"/> class.
        /// </summary>
        /// <param name="id">The sconce string identifier to get.</param>
        public LocalizedCategoryAttribute(SconceStrings.StringId id)
            : base(id.ToString())
        {
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        /// <summary>
        /// Looks up the localized name of the specified category.
        /// </summary>
        /// <param name="value">The identifer for the category to look up.</param>
        /// <returns>The localized name of the category, or null if a localized name does not exist.</returns>
        protected override string GetLocalizedString(string value)
        {
            return Package.Instance.Context.ManagedResources.GetString(value);
        }
        #endregion
    }
}