//-------------------------------------------------------------------------------------------------
// <copyright file="WixCompilerAdvancedSettingsForm.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixCompilerAdvancedSettingsForm class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Detail form for the candle settings property page for adding search include paths.
    /// </summary>
    public partial class WixCompilerAdvancedSettingsForm : Form
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixCompilerAdvancedSettingsForm"/> class.
        /// </summary>
        public WixCompilerAdvancedSettingsForm()
        {
            this.InitializeComponent();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets an array of paths to include in the search for include files.
        /// </summary>
        public string[] IncludePaths
        {
            get
            {
                string[] items = new string[this.foldersListBox.Items.Count];
                this.foldersListBox.Items.CopyTo(items, 0);

                return items;
            }

            set
            {
                this.foldersListBox.Items.Clear();

                if (value != null && value.Length > 0)
                {
                    this.foldersListBox.Items.AddRange(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -trace flag is used in the compiler.
        /// </summary>
        public bool ShowSourceTrace
        {
            get { return this.showSourceTraceCheckBox.Checked; }
            set { this.showSourceTraceCheckBox.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the -ss flag is used in the compiler.
        /// </summary>
        public bool SuppressSchemaValidation
        {
            get { return this.suppressSchemaValidationCheckBox.Checked; }
            set { this.suppressSchemaValidationCheckBox.Checked = value; }
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

        /// <summary>
        /// Changes the enabled state of all of the buttons after an operation which could change
        /// the state has occurred.
        /// </summary>
        private void ChangeButtonEnableState()
        {
            this.addFolderButton.Enabled = !String.IsNullOrEmpty(this.folderBrowserTextBox.Text);
            this.updateButton.Enabled = this.foldersListBox.SelectedIndex >= 0;
            this.moveUpButton.Enabled = (this.foldersListBox.SelectedIndex > 0);
            this.moveDownButton.Enabled = (this.foldersListBox.SelectedIndex >= 0 && this.foldersListBox.SelectedIndex < this.foldersListBox.Items.Count - 1);
            this.deleteButton.Enabled = (this.foldersListBox.SelectedIndex >= 0);
        }

        /// <summary>
        /// Reinserts the selected folder item in the list box by deleting it and re-adding it in
        /// the position specified by <paramref name="indexDelta"/>.
        /// </summary>
        /// <param name="indexDelta"></param>
        private void ReInsertFolderItem(int indexDelta)
        {
            string item = (string)this.foldersListBox.SelectedItem;
            int index = this.foldersListBox.SelectedIndex;
            this.foldersListBox.Items.RemoveAt(index);
            index += indexDelta;
            this.foldersListBox.Items.Insert(index, item);
            this.foldersListBox.SelectedIndex = index;
        }

        private void folderBrowserTextBox_TextChanged(object sender, EventArgs e)
        {
            this.ChangeButtonEnableState();
        }

        private void addFolderButton_Click(object sender, EventArgs e)
        {
            this.foldersListBox.Items.Add(this.folderBrowserTextBox.Text);
            this.ChangeButtonEnableState();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            this.foldersListBox.Items[this.foldersListBox.SelectedIndex] = this.folderBrowserTextBox.Text;
            this.ChangeButtonEnableState();
        }

        private void foldersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.folderBrowserTextBox.Text = (string)this.foldersListBox.SelectedItem;
            this.ChangeButtonEnableState();
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            this.ReInsertFolderItem(-1);
            this.ChangeButtonEnableState();
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            this.ReInsertFolderItem(1);
            this.ChangeButtonEnableState();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            this.foldersListBox.Items.RemoveAt(this.foldersListBox.SelectedIndex);
            this.ChangeButtonEnableState();
        }
    }
}