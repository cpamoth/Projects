// <copyright file="AssemblyDefaultClickThroughConsoleAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Represents a custom attribute for declaring the type to use
// as the default ClickThrough Console extension in an assembly.
// </summary>

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Represents a custom attribute for declaring the type to use
    /// as the default bConsolelder extension in an assembly.
    /// </summary>
    public class AssemblyDefaultClickThroughConsoleAttribute : Attribute
    {
        private readonly Type extensionType;

        /// <summary>
        /// Instantiate a new AssemblyDefaultBConsolelderExtensionAttribute.
        /// </summary>
        /// <param name="extensionType">The type of the default bConsolelder extension in an assembly.</param>
        public AssemblyDefaultClickThroughConsoleAttribute(Type extensionType)
        {
            this.extensionType = extensionType;
        }

        /// <summary>
        /// Gets the type of the default bConsolelder extension in an assembly.
        /// </summary>
        /// <value>The type of the default bConsolelder extension in an assembly.</value>
        public Type ExtensionType
        {
            get { return this.extensionType; }
        }
    }
}
