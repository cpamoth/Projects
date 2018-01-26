//-------------------------------------------------------------------------------------------------
// <copyright file="WixLibrarianPropertyPagePanel.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLibrarianPropertyPagePanel class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Property page contents for the Candle Settings page.
    /// </summary>
    public partial class WixLibrarianPropertyPagePanel : WixPropertyPagePanel
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLibrarianPropertyPagePanel"/> class.
        /// </summary>
        public WixLibrarianPropertyPagePanel()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLibrarianPropertyPagePanel"/> class.
        /// </summary>
        /// <param name="parentPropertyPage">The parent property page to which this is bound.</param>
        public WixLibrarianPropertyPagePanel(WixPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            this.InitializeComponent();

            // hook up the generic events
            EventHandler setDirtyHandler = delegate { this.ParentPropertyPage.IsDirty = true; };
            this.outputPathTextBox.TextChanged += setDirtyHandler;
            this.bindFilesCheckBox.CheckedChanged += setDirtyHandler;
            this.warningsAsErrorsCheckBox.CheckedChanged += setDirtyHandler;
            this.suppressSchemaValidationCheckBox.CheckedChanged += setDirtyHandler;
            this.suppressIntermediateFileVersionMismatchCheckBox.CheckedChanged += setDirtyHandler;
            this.verboseOutputCheckBox.CheckedChanged += setDirtyHandler;
            this.specificWarningsTextBox.TextChanged += setDirtyHandler;

            EventHandler radioCheckedHandler = new EventHandler(this.SuppressWarningsCheckedChanged);
            this.suppressWarningsSpecificRadioButton.CheckedChanged += radioCheckedHandler;
            this.suppressWarningsNoneRadioButton.CheckedChanged += radioCheckedHandler;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets a flag indicating whether the -bf flag is used in the librarian.
        /// </summary>
        public bool BindFiles
        {
            get { return this.bindFilesCheckBox.Checked; }
            set { this.bindFilesCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets the output path for the librarian.
        /// </summary>
        public string OutputPath
        {
            get { return this.outputPathTextBox.Text; }
            set { this.outputPathTextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sv flag is used in the linker.
        /// </summary>
        public bool SuppressIntermediateFileVersionMatching
        {
            get { return this.suppressIntermediateFileVersionMismatchCheckBox.Checked; }
            set { this.suppressIntermediateFileVersionMismatchCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ss flag is used in the librarian.
        /// </summary>
        public bool SuppressSchemaValidation
        {
            get { return this.suppressSchemaValidationCheckBox.Checked; }
            set { this.suppressSchemaValidationCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sw&lt;N&gt; flag is used in the librarian.
        /// </summary>
        public string SuppressSpecificWarnings
        {
            get
            {
                if (this.suppressWarningsSpecificRadioButton.Checked)
                {
                    return this.specificWarningsTextBox.Text;
                }

                return String.Empty;
            }

            set
            {
                this.suppressWarningsSpecificRadioButton.Checked = !String.IsNullOrEmpty(value);
                this.specificWarningsTextBox.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -wx flag is used in the librarian.
        /// </summary>
        public bool TreatWarningsAsErrors
        {
            get { return this.warningsAsErrorsCheckBox.Checked; }
            set { this.warningsAsErrorsCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -v flag is used in the librarian.
        /// </summary>
        public bool VerboseOutput
        {
            get { return this.verboseOutputCheckBox.Checked; }
            set { this.verboseOutputCheckBox.Checked = value; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Changes the enabled state of the suppress specific warnings text box to be enabled
        /// only when the corresponding radio button is checked.
        /// </summary>
        /// <param name="sender">The object that caused the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> object that contains the event data.</param>
        private void SuppressWarningsCheckedChanged(object sender, EventArgs e)
        {
            this.specificWarningsTextBox.Enabled = this.suppressWarningsSpecificRadioButton.Checked;
            this.ParentPropertyPage.IsDirty = true;
        }
    }
}
