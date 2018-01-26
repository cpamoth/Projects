//-------------------------------------------------------------------------------------------------
// <copyright file="HeatExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// An extension for the Windows Installer XML Toolset Harvester application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.Reflection;
    using Microsoft.Tools.WindowsInstallerXml;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// A command line option.
    /// </summary>
    public struct HeatCommandLineOption
    {
        public string Option;
        public string Description;

        /// <summary>
        /// Instantiates a new CommandLineOption.
        /// </summary>
        /// <param name="option">The option name.</param>
        /// <param name="description">The description of the option.</param>
        public HeatCommandLineOption(string option, string description)
        {
            this.Option = option;
            this.Description = description;
        }
    }

    /// <summary>
    /// An extension for the Windows Installer XML Toolset Harvester application.
    /// </summary>
    public abstract class HeatExtension
    {
        private HeatCore core;

        /// <summary>
        /// Gets or sets the heat core for the extension.
        /// </summary>
        /// <value>The heat core for the extension.</value>
        public HeatCore Core
        {
            get { return this.core; }
            set { this.core = value; }
        }

        /// <summary>
        /// Gets the supported command line types for this extension.
        /// </summary>
        /// <value>The supported command line types for this extension.</value>
        public virtual HeatCommandLineOption[] CommandLineTypes
        {
            get { return null; }
        }

        /// <summary>
        /// Loads a HeatExtension from a type description string.
        /// </summary>
        /// <param name="extension">The extension type description string.</param>
        /// <returns>The loaded HeatExtension.</returns>
        public static HeatExtension Load(string extension)
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

                    AssemblyDefaultHeatExtensionAttribute extensionAttribute = (AssemblyDefaultHeatExtensionAttribute)Attribute.GetCustomAttribute(extensionAssembly, typeof(AssemblyDefaultHeatExtensionAttribute));

                    extensionType = extensionAttribute.ExtensionType;
                }
                catch
                {
                    throw new WixException(WixErrors.InvalidExtension(extension));
                }
            }

            if (extensionType.IsSubclassOf(typeof(HeatExtension)))
            {
                return Activator.CreateInstance(extensionType) as HeatExtension;
            }
            else
            {
                throw new WixException(WixErrors.InvalidExtension(extension, extensionType.ToString(), typeof(HeatExtension).ToString()));
            }
        }

        /// <summary>
        /// Parse the command line options for this extension.
        /// </summary>
        /// <param name="type">The active harvester type.</param>
        /// <param name="args">The option arguments.</param>
        public virtual void ParseOptions(string type, string[] args)
        {
        }
    }
}
