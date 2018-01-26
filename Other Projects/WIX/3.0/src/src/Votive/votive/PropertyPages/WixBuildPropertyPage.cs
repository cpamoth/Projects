//-------------------------------------------------------------------------------------------------
// <copyright file="WixBuildPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixBuildPropertyPage class.
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
    /// Property page for the general WiX project configuration-independent build settings.
    /// </summary>
    [ComVisible(true)]
    [Guid("3C50BD5E-0E85-4306-A0A8-5B39CCB07DA0")]
    public class WixBuildPropertyPage : WixPropertyPage
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildPropertyPage"/> class.
        /// </summary>
        public WixBuildPropertyPage()
        {
            this.PageName = WixStrings.WixBuildPropertyPageName;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the main control.
        /// </summary>
        private WixBuildPropertyPagePanel BuildPropertyPagePanel
        {
            get { return (WixBuildPropertyPagePanel)this.PropertyPagePanel; }
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
            this.SetProperty(WixProjectFileConstants.OutputName, this.BuildPropertyPagePanel.OutputName);
            this.SetProperty(WixProjectFileConstants.OutputType, this.BuildPropertyPagePanel.OutputType.ToString());

            return true;
        }

        /// <summary>
        /// Binds the properties from the MSBuild project file to the controls on the property page.
        /// </summary>
        protected override void BindProperties()
        {
            this.BuildPropertyPagePanel.OutputName = this.GetProperty(WixProjectFileConstants.OutputName);
            try
            {
                this.BuildPropertyPagePanel.OutputType = (WixOutputType)Enum.Parse(typeof(WixOutputType), this.GetProperty(WixProjectFileConstants.OutputType), true);
            }
            catch (ArgumentException)
            {
                this.BuildPropertyPagePanel.OutputType = WixOutputType.Package;
            }
        }

        /// <summary>
        /// Creates the controls that constitute the property page. This should be safe to re-entrancy.
        /// </summary>
        /// <returns>The newly created main control that hosts the property page.</returns>
        protected override WixPropertyPagePanel CreatePropertyPagePanel()
        {
            return new WixBuildPropertyPagePanel(this);
        }
    }
}