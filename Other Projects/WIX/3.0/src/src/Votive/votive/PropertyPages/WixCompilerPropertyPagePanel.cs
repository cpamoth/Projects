//-------------------------------------------------------------------------------------------------
// <copyright file="WixCompilerPropertyPagePanel.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixCompilerPropertyPagePanel class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Windows.Forms;

    using Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms;

    /// <summary>
    /// Property page contents for the Candle Settings page.
    /// </summary>
    public partial class WixCompilerPropertyPagePanel : WixPropertyPagePanel
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private const string DebugDefine = "Debug";

        private string includePaths = String.Empty;
        private bool showSourceTrace;
        private bool suppressSchemaValidation;
        private bool verboseOutput;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixCompilerPropertyPagePanel"/> class.
        /// </summary>
        public WixCompilerPropertyPagePanel()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixCompilerPropertyPagePanel"/> class.
        /// </summary>
        /// <param name="parentPropertyPage">The parent property page to which this is bound.</param>
        public WixCompilerPropertyPagePanel(WixPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            this.InitializeComponent();

            // hook up the generic events
            EventHandler setDirtyHandler = delegate { this.ParentPropertyPage.IsDirty = true; };
            this.defineDebugCheckBox.CheckedChanged += setDirtyHandler;
            this.defineConstantsTextBox.TextChanged += setDirtyHandler;
            this.outputPathTextBox.TextChanged += setDirtyHandler;
            this.warningsAsErrorsCheckBox.CheckedChanged += setDirtyHandler;
            this.pedanticCheckBox.CheckedChanged += setDirtyHandler;
            this.specificWarningsTextBox.TextChanged += setDirtyHandler;

            EventHandler radioCheckedHandler = new EventHandler(this.SuppressWarningsCheckedChanged);
            this.suppressWarningsSpecificRadioButton.CheckedChanged += radioCheckedHandler;
            this.suppressWarningsNoneRadioButton.CheckedChanged += radioCheckedHandler;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the semicolon-delimited list of defined constants.
        /// </summary>
        public string DefineConstants
        {
            get
            {
                StringBuilder constants = new StringBuilder(this.defineConstantsTextBox.Text);

                if (this.defineDebugCheckBox.Checked)
                {
                    constants.Insert(0, DebugDefine + ";");
                }

                // get rid of the last semicolon if there is one
                if (constants.Length > 0 && constants[constants.Length - 1] == ';')
                {
                    constants.Remove(constants.Length - 1, 1);
                }

                return constants.ToString();
            }

            set
            {
                string valueToShow = value;

                // see if the "Debug" variable is already defined in the list
                int debugIndex = value.IndexOf(DebugDefine, StringComparison.InvariantCulture);

                if (debugIndex >= 0)
                {
                    // "Debug" is in the list already, so we want to remove it before showing the contents in the text box
                    StringBuilder constants = new StringBuilder(value);
                    int lengthToRemove = DebugDefine.Length;
                    int semicolonIndex = debugIndex + lengthToRemove;

                    // see if we also need to remove the semicolon
                    if (constants.Length > semicolonIndex && constants[semicolonIndex] == ';')
                    {
                        lengthToRemove++;
                    }

                    constants.Remove(debugIndex, lengthToRemove);

                    valueToShow = constants.ToString();
                }

                // set the Debug check box and the constants text box
                this.defineDebugCheckBox.Checked = (debugIndex >= 0);
                this.defineConstantsTextBox.Text = valueToShow;
            }
        }

        /// <summary>
        /// Gets or sets a semicolon-delimited list of paths to include in the search for include files.
        /// </summary>
        public string IncludePaths
        {
            get { return this.includePaths; }
            set { this.includePaths = (value == null ? String.Empty : value); }
        }

        /// <summary>
        /// Gets or sets the output path for the compiler.
        /// </summary>
        public string OutputPath
        {
            get { return this.outputPathTextBox.Text; }
            set { this.outputPathTextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -pedantic flag is used in the compiler.
        /// </summary>
        public bool Pedantic
        {
            get { return this.pedanticCheckBox.Checked; }
            set { this.pedanticCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -trace flag is used in the compiler.
        /// </summary>
        public bool ShowSourceTrace
        {
            get { return this.showSourceTrace; }
            set { this.showSourceTrace = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ss flag is used in the compiler.
        /// </summary>
        public bool SuppressSchemaValidation
        {
            get { return this.suppressSchemaValidation; }
            set { this.suppressSchemaValidation = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sw&lt;N&gt; flag is used in the compiler.
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
        /// Gets or sets a flag indicating whether the -wx flag is used in the compiler.
        /// </summary>
        public bool TreatWarningsAsErrors
        {
            get { return this.warningsAsErrorsCheckBox.Checked; }
            set { this.warningsAsErrorsCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -v flag is used in the compiler.
        /// </summary>
        public bool VerboseOutput
        {
            get { return this.verboseOutput; }
            set { this.verboseOutput = value; }
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

        private void advancedButton_Click(object sender, EventArgs e)
        {
            // initialize the form's settings
            WixCompilerAdvancedSettingsForm form = new WixCompilerAdvancedSettingsForm();
            form.IncludePaths = this.IncludePaths.Split(';');
            form.ShowSourceTrace = this.ShowSourceTrace;
            form.SuppressSchemaValidation = this.SuppressSchemaValidation;
            form.VerboseOutput = this.VerboseOutput;

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // save the form's settings
                this.IncludePaths = String.Join(";", form.IncludePaths);
                this.ShowSourceTrace = form.ShowSourceTrace;
                this.SuppressSchemaValidation = form.SuppressSchemaValidation;
                this.VerboseOutput = form.VerboseOutput;

                this.ParentPropertyPage.IsDirty = true;
            }
        }
    }
}
