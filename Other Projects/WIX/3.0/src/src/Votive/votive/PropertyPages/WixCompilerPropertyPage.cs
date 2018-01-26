//-------------------------------------------------------------------------------------------------
// <copyright file="WixCompilerPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixCompilerPropertyPage class.
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
    /// Property page for the compiler (candle) settings.
    /// </summary>
    [ComVisible(true)]
    [Guid("6D7F1842-14C0-4697-9AE6-0B777D1F5C65")]
    public class WixCompilerPropertyPage : WixPropertyPage
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixCompilerPropertyPage"/> class.
        /// </summary>
        public WixCompilerPropertyPage()
        {
            this.PageName = WixStrings.WixCompilerPropertyPageName;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the main control.
        /// </summary>
        private WixCompilerPropertyPagePanel CompilerPropertyPagePanel
        {
            get { return (WixCompilerPropertyPagePanel)this.PropertyPagePanel; }
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
            this.SetConfigProperty(WixProjectFileConstants.DefineConstants, this.CompilerPropertyPagePanel.DefineConstants);
            this.SetConfigProperty(WixProjectFileConstants.IncludeSearchPaths, this.CompilerPropertyPagePanel.IncludePaths);
            this.SetConfigProperty(WixProjectFileConstants.IntermediateOutputPath, this.CompilerPropertyPagePanel.OutputPath);
            this.SetConfigProperty(WixProjectFileConstants.Pedantic, this.CompilerPropertyPagePanel.Pedantic);
            this.SetConfigProperty(WixProjectFileConstants.ShowSourceTrace, this.CompilerPropertyPagePanel.ShowSourceTrace);
            this.SetConfigProperty(WixProjectFileConstants.SuppressSchemaValidation, this.CompilerPropertyPagePanel.SuppressSchemaValidation);
            this.SetConfigProperty(WixProjectFileConstants.SuppressSpecificWarnings, this.CompilerPropertyPagePanel.SuppressSpecificWarnings);
            this.SetConfigProperty(WixProjectFileConstants.TreatWarningsAsErrors, this.CompilerPropertyPagePanel.TreatWarningsAsErrors);
            this.SetConfigProperty(WixProjectFileConstants.VerboseOutput, this.CompilerPropertyPagePanel.VerboseOutput);

            return true;
        }

        /// <summary>
        /// Binds the properties from the MSBuild project file to the controls on the property page.
        /// </summary>
        protected override void BindProperties()
        {
            this.CompilerPropertyPagePanel.DefineConstants = this.GetConfigProperty(WixProjectFileConstants.DefineConstants);
            this.CompilerPropertyPagePanel.IncludePaths = this.GetConfigProperty(WixProjectFileConstants.IncludeSearchPaths);
            this.CompilerPropertyPagePanel.OutputPath = this.GetConfigProperty(WixProjectFileConstants.IntermediateOutputPath);
            this.CompilerPropertyPagePanel.Pedantic = this.GetConfigPropertyBoolean(WixProjectFileConstants.Pedantic);
            this.CompilerPropertyPagePanel.ShowSourceTrace = this.GetConfigPropertyBoolean(WixProjectFileConstants.ShowSourceTrace);
            this.CompilerPropertyPagePanel.SuppressSchemaValidation = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressSchemaValidation);
            this.CompilerPropertyPagePanel.SuppressSpecificWarnings = this.GetConfigProperty(WixProjectFileConstants.SuppressSpecificWarnings);
            this.CompilerPropertyPagePanel.TreatWarningsAsErrors = this.GetConfigPropertyBoolean(WixProjectFileConstants.TreatWarningsAsErrors);
            this.CompilerPropertyPagePanel.VerboseOutput = this.GetConfigPropertyBoolean(WixProjectFileConstants.VerboseOutput);
        }

        /// <summary>
        /// Creates the controls that constitute the property page. This should be safe to re-entrancy.
        /// </summary>
        /// <returns>The newly created main control that hosts the property page.</returns>
        protected override WixPropertyPagePanel CreatePropertyPagePanel()
        {
            return new WixCompilerPropertyPagePanel(this);
        }
    }
}