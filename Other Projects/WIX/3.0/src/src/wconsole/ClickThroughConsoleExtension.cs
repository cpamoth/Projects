//-------------------------------------------------------------------------------------------------
// <copyright file="ClickThroughConsoleExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
// ClickThrough console extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Console ClickThrough Extension.
    /// </summary>
    public abstract class ClickThroughConsoleExtension
    {
        /// <summary>
        /// Gets the fabricator for the extension.
        /// </summary>
        /// <value>The fabricator for the extension.</value>
        public abstract Fabricator Fabricator
        {
            get;
        }

        /// <summary>
        /// Gets the supported command line types for this extension.
        /// </summary>
        /// <value>The supported command line types for this extension.</value>
        public virtual CommandLineOption[] CommandLineTypes
        {
            get { return null; }
        }

        /// <summary>
        /// Loads a ClickThroughConsoleExtension from a type description string.
        /// </summary>
        /// <param name="extension">The extension type description string.</param>
        /// <returns>The loaded ClickThroughConsoleExtension.</returns>
        public static ClickThroughConsoleExtension Load(string extension)
        {
            Type extensionType;

            if (2 == extension.Split(',').Length)
            {
                extensionType = System.Type.GetType(extension);

                if (null == extensionType)
                {
                    throw new WixException(WixErrors.InvalidExtension(extension));
                }
            }
            else
            {
                try
                {
                    Assembly extensionAssembly = Assembly.Load(extension);

                    AssemblyDefaultClickThroughConsoleAttribute extensionAttribute = (AssemblyDefaultClickThroughConsoleAttribute)Attribute.GetCustomAttribute(extensionAssembly, typeof(AssemblyDefaultClickThroughConsoleAttribute));

                    extensionType = extensionAttribute.ExtensionType;
                }
                catch
                {
                    throw new WixException(WixErrors.InvalidExtension(extension));
                }
            }

            if (extensionType.IsSubclassOf(typeof(ClickThroughConsoleExtension)))
            {
                return Activator.CreateInstance(extensionType) as ClickThroughConsoleExtension;
            }
            else
            {
                throw new WixException(WixErrors.InvalidExtension(extension, extensionType.ToString(), typeof(ClickThroughConsoleExtension).ToString()));
            }
        }

        /// <summary>
        /// Parse the command line options for this extension.
        /// </summary>
        /// <param name="args">The option arguments.</param>
        public virtual void ParseOptions(string[] args)
        {
        }
    }
}
