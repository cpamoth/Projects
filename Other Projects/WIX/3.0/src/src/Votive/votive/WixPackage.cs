//-------------------------------------------------------------------------------------------------
// <copyright file="WixPackage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixPackage class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages;

    /// <summary>
    /// Implements and/or provides all of the required interfaces and services to allow the
    /// Microsoft Windows Installer XML (WiX) project to be integrated into the Visual Studio
    /// environment.
    /// </summary>
    [DefaultRegistryRoot(@"Software\\Microsoft\\VisualStudio\\8.0Exp")]
    [InstalledProductRegistration(true, "WiX", null, null)]
    [Guid("E0EE8E7D-F498-459e-9E90-2B3D73124AD5")]
    [PackageRegistration(RegisterUsing = RegistrationMethod.CodeBase, UseManagedResourcesOnly = true)]
    [ProvideLoadKey("Standard", "3.0", "Votive", "Microsoft", WixPackage.PackageLoadKeyResourceId)]
    [ProvideObject(typeof(WixBuildPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(WixCompilerPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(WixLinkerPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(WixBuildEventsPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideProjectFactory(typeof(WixProjectFactory), WixProjectNode.ProjectTypeName, "#100", "wixproj", "wixproj", "", LanguageVsTemplate = "WiX")]
    public class WixPackage : ProjectPackage, IVsInstalledProduct
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private const short PackageLoadKeyResourceId = 150;
        private const uint SplashBitmapResourceId = 300;
        private const uint AboutBoxIconResourceId = 400;

        private WixPackageSettings settings;

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the settings stored in the registry for this package.
        /// </summary>
        public WixPackageSettings Settings
        {
            get { return this.settings; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Initializes the package by registering all of the services that we support.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.settings = new WixPackageSettings(this);
            this.RegisterProjectFactory(new WixProjectFactory(this));
        }

        #region IVsInstalledProduct Members
        int IVsInstalledProduct.IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = SplashBitmapResourceId;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductDetails(out string pbstrProductDetails)
        {
            // get the version number from the assembly
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            pbstrProductDetails = String.Format(CultureInfo.InvariantCulture, WixStrings.ProductDetails, assemblyVersion);
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = AboutBoxIconResourceId;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.ProductID(out string pbstrPID)
        {
            pbstrPID = WixStrings.ProductId;
            return VSConstants.S_OK;
        }

        int IVsInstalledProduct.OfficialName(out string pbstrName)
        {
            pbstrName = WixStrings.OfficialName;
            return VSConstants.S_OK;
        }
        #endregion
    }
}
