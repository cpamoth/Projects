//--------------------------------------------------------------------------------------------------
// <copyright file="WixLocalizedControlTextAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLocalizedControlTextAttribute class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Attribute to denote the localized text that should be displayed on the control for the
    /// property page settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WixLocalizedControlTextAttribute : Attribute
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private string id;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLocalizedControlTextAttribute"/> class.
        /// </summary>
        /// <param name="id">The string identifier to get.</param>
        public WixLocalizedControlTextAttribute(string id)
        {
            this.id = id;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the text to display for the associated control or control's label.
        /// </summary>
        public string ControlText
        {
            get { return this.GetLocalizedString(this.id); }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Looks up the localized name of the specified category.
        /// </summary>
        /// <param name="id">The identifer for the category to look up.</param>
        /// <returns>The localized name of the category, or null if a localized name does not exist.</returns>
        protected virtual string GetLocalizedString(string id)
        {
            return WixStrings.ResourceManager.GetString(id);
        }
    }
}