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
    using System.Diagnostics;
    using Microsoft.Tools.WindowsInstallerXml;
    using Microsoft.Tools.WindowsInstallerXml.Cab;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The finalize mutator for the Windows Installer XML Toolset Internet Component Download Extension.
    /// 
    /// This extension is designed to populate the product information for the application based on the 
    /// attribute information from the dll in the ActiveX control.
    /// </summary>
    public sealed class InternetComponentDownloadFinalizerMutator : MutatorExtension
    {
        private InternetComponentDownloadHarvesterMutator harvesterMutator;

        internal InternetComponentDownloadHarvesterMutator HarvesterMutator
        {
            get { return this.harvesterMutator; }
            set { this.harvesterMutator = value; }

        }

        public override int Sequence
        {
            get { return 1025; }
        }


        /// <summary>
        /// Harvest a WiX document.
        /// </summary>
        /// <param name="argument">The argument for harvesting.</param>
        /// <returns>The harvested Fragment.</returns>
        public override void Mutate(Wix.Wix wix)
        {
            //Add in the product information from the main dll
            //Get the Product Component - update the following
            String MainBinFileName = harvesterMutator.MainBinFilePath;
            String MainBinGuid = harvesterMutator.MainComponentGuid;

            if (MainBinFileName != null)
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(MainBinFileName);

                foreach (Wix.ISchemaElement ise in wix.Children)
                {
                    if (ise is Wix.Product)
                    {
                        foreach (Wix.ISchemaElement fts in ((Wix.IParentElement)ise).Children)
                        {
                            if (fts is Wix.Feature)
                            {
                                ((Wix.Feature)fts).Title = "ActiveXControl";
                            }
                        }

                        Wix.Product productelement = (Wix.Product)ise;
                        productelement.Id = MainBinGuid;
                        productelement.Manufacturer = fileVersionInfo.CompanyName;
                        productelement.Name = fileVersionInfo.ProductName;
                        productelement.UpgradeCode = Guid.NewGuid().ToString("B").ToUpper();
                        productelement.Version = fileVersionInfo.FileVersion.Trim( " " );
                        break;
                    }
                }
            }    

        }

    }
}
