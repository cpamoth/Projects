//-------------------------------------------------------------------------------------------------
// <copyright file="WixLibrarianPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLibrarianPropertyPage class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Package;

    /// <summary>
    /// Property page for the librarian (lit) settings.
    /// </summary>
    [ComVisible(true)]
    [Guid("6CE92892-70C4-4385-87F4-627E1E04CA66")]
    public class WixLibrarianPropertyPage : WixPropertyPage
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLibrarianPropertyPage"/> class.
        /// </summary>
        public WixLibrarianPropertyPage()
        {
            this.PageName = WixStrings.WixLibrarianPropertyPageName;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the main control.
        /// </summary>
        private WixLibrarianPropertyPagePanel LibrarianPropertyPagePanel
        {
            get { return (WixLibrarianPropertyPagePanel)this.PropertyPagePanel; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Applies the changes made on the property page to the bound objects.
        /// </summary>
        /// <returns>
        /// true if the changes were successfully applied and the property page is current with the bound objects;
        /// false if the changes were applied, but the property page cannot determine if its state is current with the objects.
        /// </returns>
        protected override bool ApplyChanges()
        {
            this.SetConfigProperty(WixProjectFileConstants.LibBindFiles, this.LibrarianPropertyPagePanel.BindFiles);
            this.SetConfigProperty(WixProjectFileConstants.LibSuppressIntermediateFileVersionMatching, this.LibrarianPropertyPagePanel.SuppressIntermediateFileVersionMatching);
            this.SetConfigProperty(WixProjectFileConstants.LibSuppressSchemaValidation, this.LibrarianPropertyPagePanel.SuppressSchemaValidation);
            this.SetConfigProperty(WixProjectFileConstants.LibSuppressSpecificWarnings, this.LibrarianPropertyPagePanel.SuppressSpecificWarnings);
            this.SetConfigProperty(WixProjectFileConstants.LibTreatWarningsAsErrors, this.LibrarianPropertyPagePanel.TreatWarningsAsErrors);
            this.SetConfigProperty(WixProjectFileConstants.LibVerboseOutput, this.LibrarianPropertyPagePanel.VerboseOutput);
            this.SetConfigProperty(WixProjectFileConstants.OutputPath, this.LibrarianPropertyPagePanel.OutputPath);

            return true;
        }

        /// <summary>
        /// Binds the properties from the MSBuild project file to the controls on the property page.
        /// </summary>
        protected override void BindProperties()
        {
            this.LibrarianPropertyPagePanel.BindFiles = this.GetConfigPropertyBoolean(WixProjectFileConstants.LibBindFiles);
            this.LibrarianPropertyPagePanel.SuppressIntermediateFileVersionMatching = this.GetConfigPropertyBoolean(WixProjectFileConstants.LibSuppressIntermediateFileVersionMatching);
            this.LibrarianPropertyPagePanel.SuppressSchemaValidation = this.GetConfigPropertyBoolean(WixProjectFileConstants.LibSuppressSchemaValidation);
            this.LibrarianPropertyPagePanel.SuppressSpecificWarnings = this.GetConfigProperty(WixProjectFileConstants.LibSuppressSpecificWarnings);
            this.LibrarianPropertyPagePanel.TreatWarningsAsErrors = this.GetConfigPropertyBoolean(WixProjectFileConstants.LibTreatWarningsAsErrors);
            this.LibrarianPropertyPagePanel.VerboseOutput = this.GetConfigPropertyBoolean(WixProjectFileConstants.LibVerboseOutput);
            this.LibrarianPropertyPagePanel.OutputPath = this.GetConfigProperty(WixProjectFileConstants.OutputPath);
        }

        /// <summary>
        /// Creates the controls that constitute the property page. This should be safe to re-entrancy.
        /// </summary>
        /// <returns>The newly created main control that hosts the property page.</returns>
        protected override WixPropertyPagePanel CreatePropertyPagePanel()
        {
            return new WixLibrarianPropertyPagePanel(this);
        }
    }
}