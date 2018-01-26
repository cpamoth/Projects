// <copyright file="AssemblyDefaultFabricatorExtensionAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Represents a custom attribute for declaring the type to use
// as the default fabricator extension in an assembly.
// </summary>

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Represents a custom attribute for declaring the type to use
    /// as the default builder extension in an assembly.
    /// </summary>
    public class AssemblyDefaultFabricatorExtensionAttribute : Attribute
    {
        private readonly Type extensionType;

        /// <summary>
        /// Instantiate a new AssemblyDefaultBuilderExtensionAttribute.
        /// </summary>
        /// <param name="extensionType">The type of the default builder extension in an assembly.</param>
        public AssemblyDefaultFabricatorExtensionAttribute(Type extensionType)
        {
            this.extensionType = extensionType;
        }

        /// <summary>
        /// Gets the type of the default builder extension in an assembly.
        /// </summary>
        /// <value>The type of the default builder extension in an assembly.</value>
        public Type ExtensionType
        {
            get { return this.extensionType; }
        }
    }
}
