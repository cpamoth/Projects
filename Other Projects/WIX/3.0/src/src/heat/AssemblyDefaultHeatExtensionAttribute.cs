//-------------------------------------------------------------------------------------------------
// <copyright file="AssemblyDefaultHeatExtensionAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Represents a custom attribute for declaring the type to use
// as the default heat extension in an assembly.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;

    /// <summary>
    /// Represents a custom attribute for declaring the type to use
    /// as the default heat extension in an assembly.
    /// </summary>
    public class AssemblyDefaultHeatExtensionAttribute : Attribute
    {
        private readonly Type extensionType;

        /// <summary>
        /// Instantiate a new AssemblyDefaultHeatExtensionAttribute.
        /// </summary>
        /// <param name="extensionType">The type of the default heat extension in an assembly.</param>
        public AssemblyDefaultHeatExtensionAttribute(Type extensionType)
        {
            this.extensionType = extensionType;
        }

        /// <summary>
        /// Gets the type of the default heat extension in an assembly.
        /// </summary>
        /// <value>The type of the default heat extension in an assembly.</value>
        public Type ExtensionType
        {
            get { return this.extensionType; }
        }
    }
}
