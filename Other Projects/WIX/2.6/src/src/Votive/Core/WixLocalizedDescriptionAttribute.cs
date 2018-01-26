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
    using Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure;

    /// <summary>
    /// Subclasses <see cref="DescriptionAttribute"/> to allow for localized strings retrieved
    /// from the resource assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class WixLocalizedDescriptionAttribute : LocalizedDescriptionAttribute
    {
        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLocalizedDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="id">The sconce string identifier to get.</param>
        public WixLocalizedDescriptionAttribute(WixStrings.StringId id)
            : base(id.ToString())
        {
        }
        #endregion
    }
}