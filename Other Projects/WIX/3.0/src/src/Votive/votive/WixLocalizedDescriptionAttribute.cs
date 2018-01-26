//--------------------------------------------------------------------------------------------------
// <copyright file="WixLocalizedDescriptionAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLocalizedDescritpionAttribute class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Subclasses <see cref="DescriptionAttribute"/> to allow for localized strings retrieved
    /// from the resource assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class WixLocalizedDescriptionAttribute : DescriptionAttribute
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private bool initialized;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLocalizedDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="id">The string identifier to get.</param>
        public WixLocalizedDescriptionAttribute(string id)
            : base(id)
        {
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the description stored in this attribute.
        /// </summary>
        public override string Description
        {
            get
            {
                if (!this.initialized)
                {
                    string localizedDescription = WixStrings.ResourceManager.GetString(this.DescriptionValue);
                    if (localizedDescription != null)
                    {
                        this.DescriptionValue = localizedDescription;
                    }
                    this.initialized = true;
                }
                return this.DescriptionValue;
            }
        }
    }
}