//--------------------------------------------------------------------------------------------------
// <copyright file="WixBuildEventEditorForm.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixBuildEventEditorForm class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Forms
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Advanced editor form for build events.
    /// </summary>
    public partial class WixBuildEventEditorForm : Form
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildEventEditorForm"/> class.
        /// </summary>
        public WixBuildEventEditorForm()
        {
            this.InitializeComponent();

            this.imageList.Images.Add(WixStrings.MacroIcon);
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the editor's text.
        /// </summary>
        public string EditorText
        {
            get { return this.contentTextBox.Text; }
            set { this.contentTextBox.Text = value; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Sets up the macros list view.
        /// </summary>
        public void InitializeMacroList(WixBuildMacroCollection buildMacros)
        {
            this.macrosListView.Items.Clear();

            foreach (WixBuildMacroCollection.MacroNameValuePair pair in buildMacros)
            {
                ListViewItem item = new ListViewItem(new string[] { "$(" + pair.MacroName + ")", pair.Value }, 0);
                this.macrosListView.Items.Add(item);
            }

            this.macrosListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void InsertMacro(ListViewItem item)
        {
            string macroName = item.Text;

            this.contentTextBox.SelectedText = macroName;
            this.contentTextBox.Focus();
        }

        private void ToggleMacrosPane()
        {
            this.SuspendLayout();
            this.contentTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            if (this.macrosListView.Visible)
            {
                this.macrosListView.Visible = false;
                this.Height -= this.macrosListView.Height;
                this.macrosButton.Text = "Show &Macros";
                this.insertButton.Visible = false;
            }
            else
            {
                this.macrosListView.Visible = true;
                this.Height += this.macrosListView.Height;
                this.macrosButton.Text = "Hide &Macros";
                this.insertButton.Visible = true;
            }

            this.contentTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.ResumeLayout(true);
        }

        // =========================================================================================
        // Event Handlers
        // =========================================================================================

        private void insertButton_Click(object sender, EventArgs e)
        {
            if (this.macrosListView.SelectedItems.Count > 0)
            {
                this.InsertMacro(this.macrosListView.SelectedItems[0]);
            }
        }

        private void macrosButton_Click(object sender, EventArgs e)
        {
            this.ToggleMacrosPane();
        }

        private void macrosListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hitTestInfo = this.macrosListView.HitTest(e.Location);

            if (hitTestInfo.Item != null)
            {
                this.InsertMacro(hitTestInfo.Item);
            }
        }

        private void macrosListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.insertButton.Enabled = this.macrosListView.SelectedItems.Count > 0;
        }
    }
}