//-------------------------------------------------------------------------------------------------
// <copyright file="WixBuildEventsPropertyPagePanel.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixBuildEventsPropertyPagePanel class.
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
    public partial class WixBuildEventsPropertyPagePanel : WixPropertyPagePanel
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private WixBuildEventEditorForm editorForm = new WixBuildEventEditorForm();

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildEventsPropertyPagePanel"/> class.
        /// </summary>
        public WixBuildEventsPropertyPagePanel()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildEventsPropertyPagePanel"/> class.
        /// </summary>
        /// <param name="parentPropertyPage">The parent property page to which this is bound.</param>
        public WixBuildEventsPropertyPagePanel(WixPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            this.InitializeComponent();

            // hook up the form to both editors
            this.preBuildEditor.Initialize(parentPropertyPage.ProjectMgr, this.editorForm);
            this.postBuildEditor.Initialize(parentPropertyPage.ProjectMgr, this.editorForm);

            // hook up our events
            EventHandler dirtyHandler = delegate(object sender, EventArgs e) { this.ParentPropertyPage.IsDirty = true; };
            this.preBuildEditor.TextBox.TextChanged += dirtyHandler;
            this.postBuildEditor.TextBox.TextChanged += dirtyHandler;
            this.runPostBuildComboBox.SelectedIndexChanged += dirtyHandler;
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the pre-build event command line.
        /// </summary>
        public string PreBuildEvent
        {
            get { return this.preBuildEditor.TextBox.Text; }
            set { this.preBuildEditor.TextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets the post-build event command line.
        /// </summary>
        public string PostBuildEvent
        {
            get { return this.postBuildEditor.TextBox.Text; }
            set { this.postBuildEditor.TextBox.Text = value; }
        }

        /// <summary>
        /// Gets or sets the condition on when the post-build event command line is run.
        /// </summary>
        public RunPostBuildEvent RunPostBuildEvent
        {
            get { return (RunPostBuildEvent)this.runPostBuildComboBox.SelectedIndex; }
            set { this.runPostBuildComboBox.SelectedIndex = (int)value; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================
    }
}
