//-------------------------------------------------------------------------------------------------
// <copyright file="InternetComponentDownloadHarvester.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The harvester for the Windows Installer XML Toolset Internet Component Download Extension.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using Microsoft.Tools.WindowsInstallerXml;
    using Microsoft.Tools.WindowsInstallerXml.Cab;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The harvester for the Windows Installer XML Toolset Internet Component Download Extension.
    /// </summary>
    public sealed class InternetComponentDownloadHarvester : HarvesterExtension
    {
        /// <summary>
        /// Harvest a WiX document.
        /// </summary>
        /// <param name="argument">The argument for harvesting.</param>
        /// <returns>The harvested Fragment.</returns>
        public override Wix.Fragment Harvest(string argument)
        {
            Wix.Component component = new Wix.Component();
            component.AddChild(new Wix.CreateFolder());
            
            Wix.Fragment fragment = new Wix.Fragment();

            // Add in any CAB\DLL\OCX pre-processing here.

            // get the Internet Component Download type
            if (argument.StartsWith("ocx:") || argument.StartsWith("dll:"))
            {
                fragment = this.HarvestDll(fragment, argument.Substring(4));
            }
            else if (argument.StartsWith("cab:")) 
            {
                fragment = this.HarvestCab(fragment, argument.Substring(4));
            }

            return fragment;
        }

        /// <summary>
        /// Harvest the cab file.
        /// </summary>
        /// <param name="fragment">The property name of the bindings property.</param>
        /// <param name="argument">The path to the cab file.</param>
        /// <returns>The harvested cab binaries.</returns>
        private Wix.Fragment HarvestCab(Wix.Fragment fragment, String argument)
        {
            // crack the cab file into a temp directory.
            String cabDir = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();

            // should this be in a try...catch?
            System.IO.Directory.CreateDirectory(cabDir);

            using (WixExtractCab extractCab = new WixExtractCab())
            {
                try
                {
                    extractCab.Extract(argument, cabDir);
                }
                catch (FileNotFoundException)
                {
                    throw new WixException(WixErrors.CabExtractionFailed(argument, cabDir));
                }
            }

            // Process the folder
            //
            DirectoryHarvester ds = new DirectoryHarvester();

            fragment = ds.Harvest(cabDir);
         
            return fragment;
        }



        /// <summary>
        /// Harvest the dll or ocx file.
        /// </summary>
        /// <param name="fragment">The property name of the bindings property.</param>
        /// <param name="argument">The dll path.</param>
        /// <returns>The harvested dll or ocx product xml.</returns>
        /// 
        private Wix.Fragment HarvestDll(Wix.Fragment fragment, string argument)
        {
            // argument contains the path to the ICD dll file.

            // create the msi file
            //BUGBUG:: the dll file needs to be dealt with...
            Console.WriteLine("Exception: this currently isn't implemented.");

            return fragment;
        }

    
    }
}
