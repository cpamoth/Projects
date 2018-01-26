//-------------------------------------------------------------------------------------------------
// <copyright file="WixGroupBox.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixGroupBox class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    /// <summary>
    /// Customized group box used for property page groups.
    /// </summary>
    [DefaultProperty("Text")]
    public partial class WixGroupBox : Panel
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        WixGroupLabel groupLabel;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixGroupBox"/> class.
        /// </summary>
        public WixGroupBox()
        {
            this.InitializeComponent();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the group label text.
        /// </summary>
        [Browsable(true)]
        public override string Text
        {
            get { return this.groupLabel.Text; }
            set { this.groupLabel.Text = value; }
        }

        /// <summary>
        /// Gets the space, in pixels, that is specified by default between controls.
        /// </summary>
        protected override Padding DefaultMargin
        {
            get { return new Padding(3, 12, 3, 3); }
        }

        /// <summary>
        /// Gets the internal spacing, in pixels, of the contents of a control.
        /// </summary>
        protected override Padding DefaultPadding
        {
            get { return new Padding(24, 24, 0, 0); }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Occurs when the control has resized.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.groupLabel != null)
            {
                this.groupLabel.Width = this.ClientSize.Width + 3;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // set our properties
            this.Size = new Size(300, 200);

            // groupLabel
            this.groupLabel = new WixGroupLabel();
            this.groupLabel.Location = new Point(-3, 0);
            this.groupLabel.Size = new Size(this.ClientSize.Width + 3, 100);
            this.groupLabel.Text = "Group Label";

            // add the controls
            this.Controls.Add(this.groupLabel);

            this.ResumeLayout(false);
        }
    }
}
