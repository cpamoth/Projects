//-------------------------------------------------------------------------------------------------
// <copyright file="UtilHeatExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A utility heat extension for the Windows Installer XML Toolset Harvester application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections.Specialized;
    using Microsoft.Tools.WindowsInstallerXml.Tools;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// A utility heat extension for the Windows Installer XML Toolset Harvester application.
    /// </summary>
    public sealed class UtilHeatExtension : HeatExtension
    {
        /// <summary>
        /// Gets the supported command line types for this extension.
        /// </summary>
        /// <value>The supported command line types for this extension.</value>
        public override HeatCommandLineOption[] CommandLineTypes
        {
            get
            {
                return new HeatCommandLineOption[]
                {
                    new HeatCommandLineOption("dir", "harvest a directory"),
                    new HeatCommandLineOption("file", "harvest a file")
                };
            }
        }

        /// <summary>
        /// Parse the command line options for this extension.
        /// </summary>
        /// <param name="type">The active harvester type.</param>
        /// <param name="args">The option arguments.</param>
        public override void ParseOptions(string type, string[] args)
        {
            bool active = false;
            HarvesterExtension harvesterExtension = null;
            bool suppressHarvestingRegistryValues = false;
            UtilFinalizeHarvesterMutator utilFinalizeHarvesterMutator = new UtilFinalizeHarvesterMutator();
            UtilMutator utilMutator = new UtilMutator();

            // select the harvester
            switch (type)
            {
                case "dir":
                    harvesterExtension = new DirectoryHarvester();
                    active = true;
                    break;
                case "file":
                    harvesterExtension = new FileHarvester();
                    active = true;
                    break;
            }

            // set default settings
            utilMutator.CreateFragments = true;
            utilMutator.SetUniqueIdentifiers = true;

            // parse the options
            foreach (string arg in args)
            {
                if (null == arg || 0 == arg.Length) // skip blank arguments
                {
                    continue;
                }

                if ('-' == arg[0] || '/' == arg[0])
                {
                    string parameter = arg.Substring(1);

                    if ("gg" == parameter)
                    {
                        utilMutator.GenerateGuids = true;
                    }
                    else if ("ke" == parameter)
                    {
                        if (harvesterExtension is DirectoryHarvester)
                        {
                            ((DirectoryHarvester)harvesterExtension).KeepEmptyDirectories = true;
                        }
                        else if (active)
                        {
                            // TODO: error message - not applicable to file harvester
                        }
                    }
                    else if ("scom" == parameter)
                    {
                        if (active)
                        {
                            utilFinalizeHarvesterMutator.SuppressCOMElements = true;
                        }
                        else
                        {
                            // TODO: error message - not applicable
                        }
                    }
                    else if ("sfrag" == parameter)
                    {
                        utilMutator.CreateFragments = false;
                    }
                    else if ("sreg" == parameter)
                    {
                        suppressHarvestingRegistryValues = true;
                    }
                    else if ("suid" == parameter)
                    {
                        utilMutator.SetUniqueIdentifiers = false;
                    }
                    else if (parameter.StartsWith("template:"))
                    {
                        switch (parameter.Substring(9))
                        {
                            case "fragment":
                                utilMutator.TemplateType = TemplateType.Fragment;
                                break;
                            case "module":
                                utilMutator.TemplateType = TemplateType.Module;
                                break;
                            case "product":
                                utilMutator.TemplateType = TemplateType.Product;
                                break;
                            default:
                                // TODO: error
                                break;
                        }
                    }
                }
            }

            // set the appropriate harvester extension
            if (active)
            {
                this.Core.Harvester.Extension = harvesterExtension;

                if (!suppressHarvestingRegistryValues)
                {
                    this.Core.Mutator.AddExtension(new UtilHarvesterMutator());
                }

                this.Core.Mutator.AddExtension(utilFinalizeHarvesterMutator);
            }

            // set the mutator
            this.Core.Mutator.AddExtension(utilMutator);
        }
    }
}
