//-------------------------------------------------------------------------------------------------
// <copyright file="WixBuildEventsPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixBuildEventsPropertyPage class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Package;

    /// <summary>
    /// Property page for the build events.
    /// </summary>
    [ComVisible(true)]
    [Guid("A71983CF-33B9-4241-9B5A-80091BCE57DA")]
    public class WixBuildEventsPropertyPage : WixPropertyPage
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildEventsPropertyPage"/> class.
        /// </summary>
        public WixBuildEventsPropertyPage()
        {
            this.PageName = WixStrings.WixBuildEventsPropertyPageName;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the main control.
        /// </summary>
        private WixBuildEventsPropertyPagePanel BuildEventsPropertyPagePanel
        {
            get { return (WixBuildEventsPropertyPagePanel)this.PropertyPagePanel; }
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
            this.SetProperty(WixProjectFileConstants.RunPostBuildEvent, this.BuildEventsPropertyPagePanel.RunPostBuildEvent.ToString());

            // the pre- and post-build events should be inserted in a property group after the last import
            this.SetProperty(WixProjectFileConstants.PreBuildEvent, this.BuildEventsPropertyPagePanel.PreBuildEvent, null, PropertyPosition.UseExistingOrCreateAfterLastImport, false);
            this.SetProperty(WixProjectFileConstants.PostBuildEvent, this.BuildEventsPropertyPagePanel.PostBuildEvent, null, PropertyPosition.UseExistingOrCreateAfterLastImport, false);

            return true;
        }

        /// <summary>
        /// Binds the properties from the MSBuild project file to the controls on the property page.
        /// </summary>
        protected override void BindProperties()
        {
            // we don't write out the properties as literals (meaning that we don't encode the $(Property) values),
            // but we don't want to get the evaluated property - we want the original value
            this.BuildEventsPropertyPagePanel.PreBuildEvent = this.GetLiteralProperty(WixProjectFileConstants.PreBuildEvent);
            this.BuildEventsPropertyPagePanel.PostBuildEvent = this.GetLiteralProperty(WixProjectFileConstants.PostBuildEvent);

            try
            {
                this.BuildEventsPropertyPagePanel.RunPostBuildEvent = (RunPostBuildEvent)Enum.Parse(typeof(RunPostBuildEvent), this.GetProperty(WixProjectFileConstants.RunPostBuildEvent), true);
            }
            catch (ArgumentException)
            {
                this.BuildEventsPropertyPagePanel.RunPostBuildEvent = RunPostBuildEvent.OnBuildSuccess;
            }
        }

        /// <summary>
        /// Creates the controls that constitute the property page. This should be safe to re-entrancy.
        /// </summary>
        /// <returns>The newly created main control that hosts the property page.</returns>
        protected override WixPropertyPagePanel CreatePropertyPagePanel()
        {
            return new WixBuildEventsPropertyPagePanel(this);
        }
    }
}