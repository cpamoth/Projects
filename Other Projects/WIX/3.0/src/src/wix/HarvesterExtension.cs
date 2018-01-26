//-------------------------------------------------------------------------------------------------
// <copyright file="HarvesterExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base harvester extension.  Any of these methods can be overridden to change
// the behavior of the harvester.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The base harvester extension.  Any of these methods can be overridden to change
    /// the behavior of the harvester.
    /// </summary>
    public abstract class HarvesterExtension
    {
        private HarvesterCore core;

        /// <summary>
        /// Gets or sets the harvester core for the extension.
        /// </summary>
        /// <value>The harvester core for the extension.</value>
        public HarvesterCore Core
        {
            get { return this.core; }
            set { this.core = value; }
        }

        /// <summary>
        /// Harvest a WiX document.
        /// </summary>
        /// <param name="argument">The argument for harvesting.</param>
        /// <returns>The harvested Fragment.</returns>
        public abstract Wix.Fragment Harvest(string argument);
    }
}
