//-------------------------------------------------------------------------------------------------
// <copyright file="WixProjectFactory.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixProjectFactory class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;

    using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    /// <summary>
    /// Implements the IVsProjectFactory and IVsOwnedProjectFactory interfaces, which handle
    /// the creation of our custom WiX projects.
    /// </summary>
    [Guid("930C7802-8A8C-48f9-8165-68863BCCD9DD")]
    public class WixProjectFactory : ProjectFactory
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixProjectFactory"/> class.
        /// </summary>
        /// <param name="package">The <see cref="WixPackage"/> to which this project factory belongs.</param>
        public WixProjectFactory(WixPackage package)
            : base(package)
        {
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Creates a new <see cref="WixProjectNode"/>.
        /// </summary>
        /// <returns>A new <see cref="WixProjectNode"/> object.</returns>
        protected override ProjectNode CreateProject()
        {
            WixProjectNode project = new WixProjectNode(this.Package as WixPackage);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}
