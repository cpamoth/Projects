//-------------------------------------------------------------------------------------------------
// <copyright file="WixLinkerPropertyPagePanel.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLinkerPropertyPagePanel class.
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
    /// Property page contents for the Light Settings page.
    /// </summary>
    public partial class WixLinkerPropertyPagePanel : WixPropertyPagePanel
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private bool allowIdenticalRows;
        private string cabinetCachePath;
        private int cabinetCreationThreadCount;
        private bool leaveTemporaryFiles;
        private bool reuseCabinetCache;
        private bool setMsiAssemblyNameFileVersion;
        private bool suppressAclReset;
        private bool suppressAssemblies;
        private bool suppressDefaultAdminSequenceActions;
        private bool suppressDefaultAdvSequenceActions;
        private bool suppressDefaultUISequenceActions;
        private bool suppressDroppingUnrealTables;
        private bool suppressFileHashAndInfo;
        private bool suppressFiles;
        private bool suppressIntermediateFileVersionMatching;
        private string suppressIces;
        private bool suppressLayout;
        private bool suppressMsiAssemblyTableProcessing;
        private bool suppressSchemaValidation;
        private bool suppressValidation;
        private bool verboseOutput;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLinkerPropertyPagePanel"/> class.
        /// </summary>
        public WixLinkerPropertyPagePanel()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLinkerPropertyPagePanel"/> class.
        /// </summary>
        /// <param name="parentPropertyPage">The parent property page to which this is bound.</param>
        public WixLinkerPropertyPagePanel(WixPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            this.InitializeComponent();

            // hook up the generic events
            EventHandler setDirtyHandler = delegate { this.ParentPropertyPage.IsDirty = true; };
            this.wixVariablesTextBox.TextChanged += setDirtyHandler;
            this.outputPathTextBox.TextChanged += setDirtyHandler;
            this.culturesTextBox.TextChanged += setDirtyHandler;
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
        /// Gets or sets a flag indicating whether the -ai flag is used in the linker.
        /// </summary>
        public bool AllowIdenticalRows
        {
            get { return this.allowIdenticalRows; }
            set { this.allowIdenticalRows = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -cc flag is used in the linker.
        /// </summary>
        public string CabinetCachePath
        {
            get { return this.cabinetCachePath; }
            set { this.cabinetCachePath = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ct flag is used in the linker.
        /// </summary>
        public int CabinetCreationThreadCount
        {
            get { return this.cabinetCreationThreadCount; }
            set { this.cabinetCreationThreadCount = value; }
        }

        /// <summary>
        /// Gets or sets a semicolon-delimited list of cultures to bind into the linker.
        /// </summary>
        public string Cultures
        {
            get { return this.culturesTextBox.Text; }
            set { this.culturesTextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -notidy flag is used in the linker.
        /// </summary>
        public bool LeaveTemporaryFiles
        {
            get { return this.leaveTemporaryFiles; }
            set { this.leaveTemporaryFiles = value; }
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
        /// Gets or sets a flag indicating whether the -reusecab flag is used in the linker.
        /// </summary>
        public bool ReuseCabinetCache
        {
            get { return this.reuseCabinetCache; }
            set { this.reuseCabinetCache = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -fv flag is used in the linker.
        /// </summary>
        public bool SetMsiAssemblyNameFileVersion
        {
            get { return this.setMsiAssemblyNameFileVersion; }
            set { this.setMsiAssemblyNameFileVersion = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sacl flag is used in the linker.
        /// </summary>
        public bool SuppressAclReset
        {
            get { return this.suppressAclReset; }
            set { this.suppressAclReset = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sa flag is used in the linker.
        /// </summary>
        public bool SuppressAssemblies
        {
            get { return this.suppressAssemblies; }
            set { this.suppressAssemblies = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sadmin flag is used in the linker.
        /// </summary>
        public bool SuppressDefaultAdminSequenceActions
        {
            get { return this.suppressDefaultAdminSequenceActions; }
            set { this.suppressDefaultAdminSequenceActions = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sadv flag is used in the linker.
        /// </summary>
        public bool SuppressDefaultAdvSequenceActions
        {
            get { return this.suppressDefaultAdvSequenceActions; }
            set { this.suppressDefaultAdvSequenceActions = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sui flag is used in the linker.
        /// </summary>
        public bool SuppressDefaultUISequenceActions
        {
            get { return this.suppressDefaultUISequenceActions; }
            set { this.suppressDefaultUISequenceActions = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sdut flag is used in the linker.
        /// </summary>
        public bool SuppressDroppingUnrealTables
        {
            get { return this.suppressDroppingUnrealTables; }
            set { this.suppressDroppingUnrealTables = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sh flag is used in the linker.
        /// </summary>
        public bool SuppressFileHashAndInfo
        {
            get { return this.suppressFileHashAndInfo; }
            set { this.suppressFileHashAndInfo = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sf flag is used in the linker.
        /// </summary>
        public bool SuppressFiles
        {
            get { return this.suppressFiles; }
            set { this.suppressFiles = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sv flag is used in the linker.
        /// </summary>
        public bool SuppressIntermediateFileVersionMatching
        {
            get { return this.suppressIntermediateFileVersionMatching; }
            set { this.suppressIntermediateFileVersionMatching = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sice flag is used in the linker.
        /// </summary>
        public string SuppressIces
        {
            get { return this.suppressIces; }
            set { this.suppressIces = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sl flag is used in the linker.
        /// </summary>
        public bool SuppressLayout
        {
            get { return this.suppressLayout; }
            set { this.suppressLayout = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sma flag is used in the linker.
        /// </summary>
        public bool SuppressMsiAssemblyTableProcessing
        {
            get { return this.suppressMsiAssemblyTableProcessing; }
            set { this.suppressMsiAssemblyTableProcessing = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ss flag is used in the linker.
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
        /// Gets or sets a flag indicating whether the -X flag is used in the linker.
        /// </summary>
        public bool SuppressValidation
        {
            get { return this.suppressValidation; }
            set { this.suppressValidation = value; }
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

        /// <summary>
        /// Gets or sets the semicolon-delimited list of wix variables.
        /// </summary>
        public string WixVariables
        {
            get { return this.wixVariablesTextBox.Text; }
            set { this.wixVariablesTextBox.Text = value; }
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
            WixLinkerAdvancedSettingsForm form = new WixLinkerAdvancedSettingsForm();
            form.AllowIdenticalRows = this.AllowIdenticalRows;
            form.CabinetCachePath = this.CabinetCachePath;
            form.CabinetCreationThreadCount = this.CabinetCreationThreadCount;
            form.LeaveTemporaryFiles = this.LeaveTemporaryFiles;
            form.ReuseCabinetCache = this.ReuseCabinetCache;
            form.SetMsiAssemblyNameFileVersion = this.SetMsiAssemblyNameFileVersion;
            form.SuppressAclReset = this.SuppressAclReset;
            form.SuppressAssemblies = this.SuppressAssemblies;
            form.SuppressDefaultAdminSequenceActions = this.SuppressDefaultAdminSequenceActions;
            form.SuppressDefaultAdvSequenceActions = this.SuppressDefaultAdvSequenceActions;
            form.SuppressDefaultUISequenceActions = this.SuppressDefaultUISequenceActions;
            form.SuppressDroppingUnrealTables = this.SuppressDroppingUnrealTables;
            form.SuppressFileHashAndInfo = this.SuppressFileHashAndInfo;
            form.SuppressFiles = this.SuppressFiles;
            form.SuppressIces = this.SuppressIces;
            form.SuppressIntermediateFileVersionMatching = this.SuppressIntermediateFileVersionMatching;
            form.SuppressLayout = this.SuppressLayout;
            form.SuppressMsiAssemblyTableProcessing = this.SuppressMsiAssemblyTableProcessing;
            form.SuppressSchemaValidation = this.SuppressSchemaValidation;
            form.SuppressValidation = this.SuppressValidation;
            form.VerboseOutput = this.VerboseOutput;

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // save the form's settings
                this.AllowIdenticalRows = form.AllowIdenticalRows;
                this.CabinetCachePath = form.CabinetCachePath;
                this.CabinetCreationThreadCount = form.CabinetCreationThreadCount;
                this.LeaveTemporaryFiles = form.LeaveTemporaryFiles;
                this.ReuseCabinetCache = form.ReuseCabinetCache;
                this.SetMsiAssemblyNameFileVersion = form.SetMsiAssemblyNameFileVersion;
                this.SuppressAclReset = form.SuppressAclReset;
                this.SuppressAssemblies = form.SuppressAssemblies;
                this.SuppressDefaultAdminSequenceActions = form.SuppressDefaultAdminSequenceActions;
                this.SuppressDefaultAdvSequenceActions = form.SuppressDefaultAdvSequenceActions;
                this.SuppressDefaultUISequenceActions = form.SuppressDefaultUISequenceActions;
                this.SuppressDroppingUnrealTables = form.SuppressDroppingUnrealTables;
                this.SuppressFileHashAndInfo = form.SuppressFileHashAndInfo;
                this.SuppressFiles = form.SuppressFiles;
                this.SuppressIces = form.SuppressIces;
                this.SuppressIntermediateFileVersionMatching = form.SuppressIntermediateFileVersionMatching;
                this.SuppressLayout = form.SuppressLayout;
                this.SuppressMsiAssemblyTableProcessing = form.SuppressMsiAssemblyTableProcessing;
                this.SuppressSchemaValidation = form.SuppressSchemaValidation;
                this.SuppressValidation = form.SuppressValidation;
                this.VerboseOutput = form.VerboseOutput;

                this.ParentPropertyPage.IsDirty = true;
            }
        }
    }
}
