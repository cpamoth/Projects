//--------------------------------------------------------------------------------------------------
// <copyright file="WixLocalizedDisplayNameAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLocalizedDisplayNameAttribute class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Subclasses <see cref="DisplayNameAttribute"/> to allow for localized strings retrieved
    /// from the resource assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    public class WixLocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private bool initialized;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLocalizedDisplayNameAttribute"/> class.
        /// </summary>
        /// <param name="id">The string identifier to get.</param>
        public WixLocalizedDisplayNameAttribute(string id)
            : base(id)
        {
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the display name for a property, event, or public void method that takes no
        /// arguments stored in this attribute.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (!this.initialized)
                {
                    string localizedString = WixStrings.ResourceManager.GetString(this.DisplayNameValue);
                    if (localizedString != null)
                    {
                        this.DisplayNameValue = localizedString;
                    }
                    this.initialized = true;
                }
                return this.DisplayNameValue;
            }
        }
    }
}