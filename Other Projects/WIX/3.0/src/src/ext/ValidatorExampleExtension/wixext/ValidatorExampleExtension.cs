//-------------------------------------------------------------------------------------------------
// <copyright file="ValidatorExampleExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset Validator Example Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The Windows Installer XML Toolset Validator Example Extension.
    /// </summary>
    public sealed class ValidatorExampleExtension : WixExtension
    {
        private ValidatorExtension extension;

        /// <summary>
        /// Gets the optional validator extension.
        /// </summary>
        /// <value>The optional validator extension.</value>
        public override ValidatorExtension ValidatorExtension
        {
            get
            {
                if (null == this.extension)
                {
                    this.extension = new ValidatorXmlExtension();
                }

                return this.extension;
            }
        }
    }
}
