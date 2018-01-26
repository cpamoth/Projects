//-------------------------------------------------------------------------------------------------
// <copyright file="Harvester.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset harvester.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The Windows Installer XML Toolset harvester.
    /// </summary>
    public sealed class Harvester
    {
        private HarvesterExtension harvesterExtension;

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>The extension.</value>
        public HarvesterExtension Extension
        {
            get
            {
                return this.harvesterExtension;
            }

            set
            {
                if (null != this.harvesterExtension)
                {
                    throw new InvalidOperationException("Multiple harvester extensions specified.");
                }

                this.harvesterExtension = value;
            }
        }

        /// <summary>
        /// Harvest wix authoring.
        /// </summary>
        /// <param name="argument">The argument for harvesting.</param>
        /// <returns>The harvested wix authoring.</returns>
        public Wix.Wix Harvest(string argument)
        {
            if (null == argument)
            {
                throw new ArgumentNullException("argument");
            }

            if (null == this.harvesterExtension)
            {
                throw new InvalidOperationException("No harvester extension was specified.");
            }

            this.harvesterExtension.Core = new HarvesterCore(this.Message);

            Wix.Fragment fragment = this.harvesterExtension.Harvest(argument);
            if (null == fragment)
            {
                return null;
            }

            Wix.Wix wix = new Wix.Wix();
            wix.AddChild(fragment);

            return wix;
        }
    }
}
