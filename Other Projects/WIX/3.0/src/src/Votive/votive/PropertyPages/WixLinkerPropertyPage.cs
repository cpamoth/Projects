//-------------------------------------------------------------------------------------------------
// <copyright file="WixLinkerPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLinkerPropertyPage class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Package;

    /// <summary>
    /// Property page for the linker (light) settings.
    /// </summary>
    [ComVisible(true)]
    [Guid("1D7B7FA7-4D01-4112-8972-F287E9DC206A")]
    public class WixLinkerPropertyPage : WixPropertyPage
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLinkerPropertyPage"/> class.
        /// </summary>
        public WixLinkerPropertyPage()
        {
            this.PageName = WixStrings.WixLinkerPropertyPageName;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the main control.
        /// </summary>
        private WixLinkerPropertyPagePanel LinkerPropertyPagePanel
        {
            get { return (WixLinkerPropertyPagePanel)this.PropertyPagePanel; }
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
            this.SetConfigProperty(WixProjectFileConstants.AllowIdenticalRows, this.LinkerPropertyPagePanel.AllowIdenticalRows);
            this.SetConfigProperty(WixProjectFileConstants.CabinetCachePath, this.LinkerPropertyPagePanel.CabinetCachePath);
            this.SetConfigProperty(WixProjectFileConstants.CabinetCreationThreadCount, this.LinkerPropertyPagePanel.CabinetCreationThreadCount);
            this.SetConfigProperty(WixProjectFileConstants.Cultures, this.LinkerPropertyPagePanel.Cultures);
            this.SetConfigProperty(WixProjectFileConstants.LeaveTemporaryFiles, this.LinkerPropertyPagePanel.LeaveTemporaryFiles);
            this.SetConfigProperty(WixProjectFileConstants.OutputPath, this.LinkerPropertyPagePanel.OutputPath);
            this.SetConfigProperty(WixProjectFileConstants.LinkerPedantic, this.LinkerPropertyPagePanel.Pedantic);
            this.SetConfigProperty(WixProjectFileConstants.ReuseCabinetCache, this.LinkerPropertyPagePanel.ReuseCabinetCache);
            this.SetConfigProperty(WixProjectFileConstants.SetMsiAssemblyNameFileVersion, this.LinkerPropertyPagePanel.SetMsiAssemblyNameFileVersion);
            this.SetConfigProperty(WixProjectFileConstants.SuppressAclReset, this.LinkerPropertyPagePanel.SuppressAclReset);
            this.SetConfigProperty(WixProjectFileConstants.SuppressAssemblies, this.LinkerPropertyPagePanel.SuppressAssemblies);
            this.SetConfigProperty(WixProjectFileConstants.SuppressDefaultAdminSequenceActions, this.LinkerPropertyPagePanel.SuppressDefaultAdminSequenceActions);
            this.SetConfigProperty(WixProjectFileConstants.SuppressDefaultAdvSequenceActions, this.LinkerPropertyPagePanel.SuppressDefaultAdvSequenceActions);
            this.SetConfigProperty(WixProjectFileConstants.SuppressDefaultUISequenceActions, this.LinkerPropertyPagePanel.SuppressDefaultUISequenceActions);
            this.SetConfigProperty(WixProjectFileConstants.SuppressDroppingUnrealTables, this.LinkerPropertyPagePanel.SuppressDroppingUnrealTables);
            this.SetConfigProperty(WixProjectFileConstants.SuppressFileHashAndInfo, this.LinkerPropertyPagePanel.SuppressFileHashAndInfo);
            this.SetConfigProperty(WixProjectFileConstants.SuppressFiles, this.LinkerPropertyPagePanel.SuppressFiles);
            this.SetConfigProperty(WixProjectFileConstants.SuppressIces, this.LinkerPropertyPagePanel.SuppressIces);
            this.SetConfigProperty(WixProjectFileConstants.LinkerSuppressIntermediateFileVersionMatching, this.LinkerPropertyPagePanel.SuppressIntermediateFileVersionMatching);
            this.SetConfigProperty(WixProjectFileConstants.SuppressLayout, this.LinkerPropertyPagePanel.SuppressLayout);
            this.SetConfigProperty(WixProjectFileConstants.SuppressMsiAssemblyTableProcessing, this.LinkerPropertyPagePanel.SuppressMsiAssemblyTableProcessing);
            this.SetConfigProperty(WixProjectFileConstants.LinkerSuppressSchemaValidation, this.LinkerPropertyPagePanel.SuppressSchemaValidation);
            this.SetConfigProperty(WixProjectFileConstants.LinkerSuppressSpecificWarnings, this.LinkerPropertyPagePanel.SuppressSpecificWarnings);
            this.SetConfigProperty(WixProjectFileConstants.SuppressValidation, this.LinkerPropertyPagePanel.SuppressValidation);
            this.SetConfigProperty(WixProjectFileConstants.LinkerTreatWarningsAsErrors, this.LinkerPropertyPagePanel.TreatWarningsAsErrors);
            this.SetConfigProperty(WixProjectFileConstants.LinkerVerboseOutput, this.LinkerPropertyPagePanel.VerboseOutput);
            this.SetConfigProperty(WixProjectFileConstants.WixVariables, this.LinkerPropertyPagePanel.WixVariables);

            return true;
        }

        /// <summary>
        /// Binds the properties from the MSBuild project file to the controls on the property page.
        /// </summary>
        protected override void BindProperties()
        {
            this.LinkerPropertyPagePanel.AllowIdenticalRows = this.GetConfigPropertyBoolean(WixProjectFileConstants.AllowIdenticalRows);
            this.LinkerPropertyPagePanel.CabinetCachePath = this.GetConfigProperty(WixProjectFileConstants.CabinetCachePath);
            this.LinkerPropertyPagePanel.CabinetCreationThreadCount = this.GetConfigPropertyInt32(WixProjectFileConstants.CabinetCreationThreadCount);
            this.LinkerPropertyPagePanel.Cultures = this.GetConfigProperty(WixProjectFileConstants.Cultures);
            this.LinkerPropertyPagePanel.LeaveTemporaryFiles = this.GetConfigPropertyBoolean(WixProjectFileConstants.LeaveTemporaryFiles);
            this.LinkerPropertyPagePanel.OutputPath = this.GetConfigProperty(WixProjectFileConstants.OutputPath);
            this.LinkerPropertyPagePanel.Pedantic = this.GetConfigPropertyBoolean(WixProjectFileConstants.LinkerPedantic);
            this.LinkerPropertyPagePanel.ReuseCabinetCache = this.GetConfigPropertyBoolean(WixProjectFileConstants.ReuseCabinetCache);
            this.LinkerPropertyPagePanel.SetMsiAssemblyNameFileVersion = this.GetConfigPropertyBoolean(WixProjectFileConstants.SetMsiAssemblyNameFileVersion);
            this.LinkerPropertyPagePanel.SuppressAclReset = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressAclReset);
            this.LinkerPropertyPagePanel.SuppressAssemblies = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressAssemblies);
            this.LinkerPropertyPagePanel.SuppressDefaultAdminSequenceActions = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressDefaultAdminSequenceActions);
            this.LinkerPropertyPagePanel.SuppressDefaultAdvSequenceActions = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressDefaultAdvSequenceActions);
            this.LinkerPropertyPagePanel.SuppressDefaultUISequenceActions = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressDefaultUISequenceActions);
            this.LinkerPropertyPagePanel.SuppressDroppingUnrealTables = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressDroppingUnrealTables);
            this.LinkerPropertyPagePanel.SuppressFileHashAndInfo = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressFileHashAndInfo);
            this.LinkerPropertyPagePanel.SuppressFiles = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressFiles);
            this.LinkerPropertyPagePanel.SuppressIces = this.GetConfigProperty(WixProjectFileConstants.SuppressIces);
            this.LinkerPropertyPagePanel.SuppressIntermediateFileVersionMatching = this.GetConfigPropertyBoolean(WixProjectFileConstants.LinkerSuppressIntermediateFileVersionMatching);
            this.LinkerPropertyPagePanel.SuppressLayout = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressLayout);
            this.LinkerPropertyPagePanel.SuppressMsiAssemblyTableProcessing = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressMsiAssemblyTableProcessing);
            this.LinkerPropertyPagePanel.SuppressSchemaValidation = this.GetConfigPropertyBoolean(WixProjectFileConstants.LinkerSuppressSchemaValidation);
            this.LinkerPropertyPagePanel.SuppressSpecificWarnings = this.GetConfigProperty(WixProjectFileConstants.LinkerSuppressSpecificWarnings);
            this.LinkerPropertyPagePanel.SuppressValidation = this.GetConfigPropertyBoolean(WixProjectFileConstants.SuppressValidation);
            this.LinkerPropertyPagePanel.TreatWarningsAsErrors = this.GetConfigPropertyBoolean(WixProjectFileConstants.LinkerTreatWarningsAsErrors);
            this.LinkerPropertyPagePanel.VerboseOutput = this.GetConfigPropertyBoolean(WixProjectFileConstants.LinkerVerboseOutput);
            this.LinkerPropertyPagePanel.WixVariables = this.GetConfigProperty(WixProjectFileConstants.WixVariables);
        }

        /// <summary>
        /// Creates the controls that constitute the property page. This should be safe to re-entrancy.
        /// </summary>
        /// <returns>The newly created main control that hosts the property page.</returns>
        protected override WixPropertyPagePanel CreatePropertyPagePanel()
        {
            return new WixLinkerPropertyPagePanel(this);
        }
    }
}