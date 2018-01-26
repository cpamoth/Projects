//-------------------------------------------------------------------------------------------------
// <copyright file="PreProcExampleExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset PreProcesses Example Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The Windows Installer XML Toolset PreProcessor Example Extension.
    /// </summary>
    public sealed class PreProcExampleExtension : WixExtension
    {
        private PreProcExtension extension;

        /// <summary>
        /// Gets the optional preprocessor extension.
        /// </summary>
        /// <value>The optional preprocessor extension.</value>
        public override PreprocessorExtension PreprocessorExtension
        {
            get
            {
                if (null == this.extension)
                {
                    this.extension = new PreProcExtension();
                }

                return this.extension;
            }
        }
    }
}
