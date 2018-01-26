//-------------------------------------------------------------------------------------------------
// <copyright file="WixPropertyPagePanel.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixPropertyPagePanel class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Property page contents for the Wix Project Build Settings page.
    /// </summary>
    public partial class WixPropertyPagePanel : UserControl
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private WixPropertyPage parentPropertyPage;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixPropertyPagePanel"/> class.
        /// </summary>
        public WixPropertyPagePanel()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixPropertyPagePanel"/> class.
        /// </summary>
        /// <param name="parentPropertyPage">The parent property page to which this is bound.</param>
        public WixPropertyPagePanel(WixPropertyPage parentPropertyPage)
        {
            this.parentPropertyPage = parentPropertyPage;
            this.InitializeComponent();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the dirty state on the bound parent property page.
        /// </summary>
        protected bool IsDirty
        {
            get
            {
                Debug.Assert(this.ParentPropertyPage != null, "ParentPropertyPage has not been set.");
                if (this.ParentPropertyPage != null)
                {
                    return this.ParentPropertyPage.IsDirty;
                }
                return true;
            }

            set
            {
                Debug.Assert(this.ParentPropertyPage != null, "ParentPropertyPage has not been set.");
                if (this.ParentPropertyPage != null)
                {
                    this.ParentPropertyPage.IsDirty = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the parent property page to which this is bound.
        /// </summary>
        internal WixPropertyPage ParentPropertyPage
        {
            get { return this.parentPropertyPage; }
            set { this.parentPropertyPage = value; }
        }
    }
}
