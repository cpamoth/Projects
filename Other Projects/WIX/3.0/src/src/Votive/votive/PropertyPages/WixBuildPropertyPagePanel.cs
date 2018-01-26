//-------------------------------------------------------------------------------------------------
// <copyright file="WixBuildPropertyPagePanel.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixBuildPropertyPagePanel class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    /// Property page contents for the Wix Project Build Settings page.
    /// </summary>
    public partial class WixBuildPropertyPagePanel : WixPropertyPagePanel
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildPropertyPagePanel"/> class.
        /// </summary>
        public WixBuildPropertyPagePanel()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildPropertyPagePanel"/> class.
        /// </summary>
        /// <param name="parentPropertyPage">The parent property page to which this is bound.</param>
        public WixBuildPropertyPagePanel(WixPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            this.InitializeComponent();
            this.InitializeOutputTypes();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the output name.
        /// </summary>
        public string OutputName
        {
            get { return this.outputNameTextBox.Text; }
            set { this.outputNameTextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets the output type, which is defined in the MSBuild wix.targets file.
        /// </summary>
        public WixOutputType OutputType
        {
            get
            {
                if (this.outputTypeModuleRadioButton.Checked)
                {
                    return WixOutputType.Module;
                }
                else if (this.outputTypeLibraryRadioButton.Checked)
                {
                    return WixOutputType.Library;
                }
                else
                {
                    Debug.Assert(this.outputTypePackageRadioButton.Checked, "None of the radio buttons are checked. Not possible.");
                    return WixOutputType.Package;
                }
            }

            set
            {
                this.outputTypePackageRadioButton.Checked = (value == WixOutputType.Package);
                this.outputTypeModuleRadioButton.Checked = (value == WixOutputType.Module);
                this.outputTypeLibraryRadioButton.Checked = (value == WixOutputType.Library);
            }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Initializes the output types with the localized display names of the <see cref="WixOutputType"/> enum.
        /// </summary>
        private void InitializeOutputTypes()
        {
            // set the images from the resource file
            this.outputTypePackagePictureBox.Image = WixStrings.WixProjectIcon.ToBitmap();
            this.outputTypeModulePictureBox.Image = WixStrings.WixMergeModuleProjectIcon.ToBitmap();
            this.outputTypeLibraryPictureBox.Image = WixStrings.WixLibraryProjectIcon.ToBitmap();

            EventHandler checkHandler = new EventHandler(outputTypeRadioButtonCheckedChanged);
            this.outputTypePackageRadioButton.CheckedChanged += checkHandler;
            this.outputTypeModuleRadioButton.CheckedChanged += checkHandler;
            this.outputTypeLibraryRadioButton.CheckedChanged += checkHandler;
        }

        private void outputTypeRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            this.IsDirty = true;
        }

        private void outputNameTextBox_TextChanged(object sender, EventArgs e)
        {
            this.IsDirty = true;
        }
    }
}
