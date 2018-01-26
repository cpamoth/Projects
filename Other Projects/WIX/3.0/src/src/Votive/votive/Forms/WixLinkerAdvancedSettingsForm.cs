//-------------------------------------------------------------------------------------------------
// <copyright file="WixLinkerAdvancedSettingsForm.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLinkerAdvancedSettingsForm class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>
    /// Detail form for the candle settings property page for adding search include paths.
    /// </summary>
    public partial class WixLinkerAdvancedSettingsForm : Form
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLinkerAdvancedSettingsForm"/> class.
        /// </summary>
        public WixLinkerAdvancedSettingsForm()
        {
            this.InitializeComponent();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets a flag indicating whether the -ai flag is used in the linker.
        /// </summary>
        public bool AllowIdenticalRows
        {
            get { return this.allowIdenticalRowsCheckBox.Checked; }
            set { this.allowIdenticalRowsCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -cc flag is used in the linker.
        /// </summary>
        public string CabinetCachePath
        {
            get { return this.cabinetCacheTextBox.Text; }
            set { this.cabinetCacheTextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ct flag is used in the linker.
        /// </summary>
        public int CabinetCreationThreadCount
        {
            get
            {
                if (this.cabinetThreadsCheckBox.Checked)
                {
                    return (int)this.cabinetThreadsUpDown.Value;
                }
                return WixProjectFileConstants.UnspecifiedValue;
            }

            set
            {
                if (value == WixProjectFileConstants.UnspecifiedValue)
                {
                    this.cabinetThreadsCheckBox.Checked = false;
                    this.cabinetThreadsUpDown.Value = this.cabinetThreadsUpDown.Minimum;
                }
                else
                {
                    this.cabinetThreadsCheckBox.Checked = true;
                    this.cabinetThreadsUpDown.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -notidy flag is used in the linker.
        /// </summary>
        public bool LeaveTemporaryFiles
        {
            get { return this.leaveTemporaryFilesCheckBox.Checked; }
            set { this.leaveTemporaryFilesCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -reusecab flag is used in the linker.
        /// </summary>
        public bool ReuseCabinetCache
        {
            get { return this.reuseCabCheckBox.Checked; }
            set { this.reuseCabCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -fv flag is used in the linker.
        /// </summary>
        public bool SetMsiAssemblyNameFileVersion
        {
            get { return this.setMsiAssemblyNameFileVersionCheckBox.Checked; }
            set { this.setMsiAssemblyNameFileVersionCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sacl flag is used in the linker.
        /// </summary>
        public bool SuppressAclReset
        {
            get { return this.suppressAclResetCheckBox.Checked; }
            set { this.suppressAclResetCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sa flag is used in the linker.
        /// </summary>
        public bool SuppressAssemblies
        {
            get { return this.suppressAssembliesCheckBox.Checked; }
            set { this.suppressAssembliesCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sadmin flag is used in the linker.
        /// </summary>
        public bool SuppressDefaultAdminSequenceActions
        {
            get { return this.suppressDefaultAdminSequenceCheckBox.Checked; }
            set { this.suppressDefaultAdminSequenceCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sadv flag is used in the linker.
        /// </summary>
        public bool SuppressDefaultAdvSequenceActions
        {
            get { return this.suppressDefaultAdvSequenceCheckBox.Checked; }
            set { this.suppressDefaultAdvSequenceCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sui flag is used in the linker.
        /// </summary>
        public bool SuppressDefaultUISequenceActions
        {
            get { return this.suppressDefaultUISequenceCheckBox.Checked; }
            set { this.suppressDefaultUISequenceCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sdut flag is used in the linker.
        /// </summary>
        public bool SuppressDroppingUnrealTables
        {
            get { return this.suppressDroppingUnrealTablesCheckBox.Checked; }
            set { this.suppressDroppingUnrealTablesCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sh flag is used in the linker.
        /// </summary>
        public bool SuppressFileHashAndInfo
        {
            get { return this.suppressFileAndHashInfoCheckBox.Checked; }
            set { this.suppressFileAndHashInfoCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sf flag is used in the linker.
        /// </summary>
        public bool SuppressFiles
        {
            get { return this.suppressFilesCheckBox.Checked; }
            set { this.suppressFilesCheckBox.Checked = value; }
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
        /// Gets or sets a flag indicating whether the -sice flag is used in the linker.
        /// </summary>
        public string SuppressIces
        {
            get { return this.suppressIcesTextBox.Text; }
            set
            {
                this.suppressIcesTextBox.Text = value;
                this.suppressIcesCheckBox.Checked = !String.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sl flag is used in the linker.
        /// </summary>
        public bool SuppressLayout
        {
            get { return this.suppressLayoutCheckBox.Checked; }
            set { this.suppressLayoutCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sma flag is used in the linker.
        /// </summary>
        public bool SuppressMsiAssemblyTableProcessing
        {
            get { return this.suppressMsiAssemblyTableProcessingCheckBox.Checked; }
            set { this.suppressMsiAssemblyTableProcessingCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ss flag is used in the linker.
        /// </summary>
        public bool SuppressSchemaValidation
        {
            get { return this.suppressSchemaValidationCheckBox.Checked; }
            set { this.suppressSchemaValidationCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -sval flag is used in the linker.
        /// </summary>
        public bool SuppressValidation
        {
            get { return this.suppressValidationCheckBox.Checked; }
            set { this.suppressValidationCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -v flag is used in the compiler.
        /// </summary>
        public bool VerboseOutput
        {
            get { return this.verboseOutputCheckBox.Checked; }
            set { this.verboseOutputCheckBox.Checked = value; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        private void suppressIcesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.suppressIcesTextBox.Enabled = this.iceExampleLabel.Enabled = this.suppressIcesCheckBox.Checked;

            // clear the text box if the check box is not checked
            if (!this.suppressIcesCheckBox.Checked)
            {
                this.suppressIcesTextBox.Text = String.Empty;
            }
        }

        private void cabinetThreadsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.cabinetThreadsUpDown.Enabled = this.cabinetThreadsCheckBox.Checked;
        }
    }
}