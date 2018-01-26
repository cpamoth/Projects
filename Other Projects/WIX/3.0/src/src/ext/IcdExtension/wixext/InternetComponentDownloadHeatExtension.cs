//-------------------------------------------------------------------------------------------------
// <copyright file="InternetComponentDownloadHeatExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// An Internet Component Download harvesting extension for the Windows Installer XML Toolset Harvester application.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections.Specialized;
    using Microsoft.Tools.WindowsInstallerXml.Tools;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// An ICD harvesting extension for the Windows Installer XML Toolset Harvester application.
    /// </summary>
    public sealed class InternetComponentDownloadHeatExtension : HeatExtension
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
                    new HeatCommandLineOption("icd", "harvest an Internet Component Download package"),
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

            // select the harvester
            switch (type)
            {
                case "icd":
                    harvesterExtension = new InternetComponentDownloadHarvester();
                    active = true;
                    break;
            }

            // parse the options
            foreach (string arg in args)
            {
                if (null == arg || 0 == arg.Length) // skip blank arguments
                {
                    continue;
                }
            }

            // set the appropriate harvester extension
            if (active)
            {
                this.Core.Harvester.Extension = harvesterExtension;

                InternetComponentDownloadHarvesterMutator mutator2 = new InternetComponentDownloadHarvesterMutator(); 
                this.Core.Mutator.AddExtension(mutator2);
                this.Core.Mutator.AddExtension(new UtilFinalizeHarvesterMutator());

                InternetComponentDownloadFinalizerMutator mutator = new InternetComponentDownloadFinalizerMutator();
                mutator.HarvesterMutator = mutator2;
                this.Core.Mutator.AddExtension(mutator);
            }
        }
    }
}
