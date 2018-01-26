//-------------------------------------------------------------------------------------------------
// <copyright file="WixPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixPropertyPage class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Abstract base class for a WiX project property page.
    /// </summary>
    [ComVisible(true)]
    [Guid("9F78788F-0BE9-4962-A4D5-2BE8A4C78DF7")]
    public abstract class WixPropertyPage : IPropertyPage
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private const int SW_HIDE = 0;

        private bool active;
        private bool isDirty;
        private WixPropertyPagePanel propertyPagePanel;
        private string pageName;
        private IPropertyPageSite pageSite;
        private WixProjectNode project;
        private ProjectConfig[] projectConfigs;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixPropertyPage"/> class.
        /// </summary>
        public WixPropertyPage()
        {
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets a value indicating whether this property page has changed its state since
        /// the last call to <see cref="ApplyChanges"/>. The property sheet uses this information
        /// to enable or disable the Apply button in the dialog box.
        /// </summary>
        [Browsable(false)]
        [AutomationBrowsable(false)]
        public bool IsDirty
        {
            get { return this.isDirty; }

            set
            {
                if (this.isDirty != value)
                {
                    this.isDirty = value;
                    if (this.pageSite != null)
                    {
                        this.pageSite.OnStatusChange((uint)(this.isDirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the property page that will be displayed in the left hand
        /// navigation bar on the VS property page dialog.
        /// </summary>
        [Browsable(false)]
        [AutomationBrowsable(false)]
        public string PageName
        {
            get { return this.pageName; }
            set { this.pageName = value; }
        }

        /// <summary>
        /// Gets the project associated with this property page.
        /// </summary>
        [Browsable(false)]
        [AutomationBrowsable(false)]
        public WixProjectNode ProjectMgr
        {
            get { return this.project; }
        }

        /// <summary>
        /// Gets the main control that hosts the property page.
        /// </summary>
        protected WixPropertyPagePanel PropertyPagePanel
        {
            get { return this.propertyPagePanel; }
            private set { this.propertyPagePanel = value; }
        }

        /// <summary>
        /// Gets the output type of the project.
        /// </summary>
        internal WixOutputType ProjectOutputType
        {
            get { return ((WixProjectNode)this.ProjectMgr).OutputType; }
        }

        // =========================================================================================
        // IPropertyPage Members
        // =========================================================================================

        /// <summary>
        /// Called when the environment wants us to create our property page.
        /// </summary>
        /// <param name="hWndParent">The HWND of the parent window.</param>
        /// <param name="pRect">The bounds of the area that we should fill.</param>
        /// <param name="bModal">Indicates whether the dialog box is shown modally or not.</param>
        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            // create the panel control
            this.PropertyPagePanel = this.CreatePropertyPagePanel();

            // we need to create the control so the handle is valid
            this.PropertyPagePanel.CreateControl();

            // set our parent
            NativeMethods.SetParent(this.PropertyPagePanel.Handle, hWndParent);

            // set our initial size
            this.ResizeContents(pRect[0]);

            this.active = true;
            this.BindProperties();
            this.IsDirty = false;
        }

        /// <summary>
        /// Applies the changes made on the property page to the bound objects.
        /// </summary>
        /// <returns>
        /// <b>S_OK</b> if the changes were successfully applied and the property page is current with the bound objects;
        /// <b>S_FALSE</b> if the changes were applied, but the property page cannot determine if its state is current with the objects.
        /// </returns>
        int IPropertyPage.Apply()
        {
            if (this.IsDirty)
            {
                bool applied = this.ApplyChanges();
                this.IsDirty = !applied;
                return (applied ? VSConstants.S_OK : VSConstants.S_FALSE);
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// The environment calls this to notify us that we should clean up our resources.
        /// </summary>
        void IPropertyPage.Deactivate()
        {
            if (this.PropertyPagePanel != null)
            {
                this.PropertyPagePanel.Dispose();
                this.PropertyPagePanel = null;
            }
            this.active = false;
        }

        /// <summary>
        /// The environment calls this to get the parameters to describe the property page.
        /// </summary>
        /// <param name="pPageInfo">The parameters are returned in this one-sized array.</param>
        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            WixHelperMethods.VerifyNonNullArgument(pPageInfo, "pPageInfo");

            if (this.PropertyPagePanel == null)
            {
                this.PropertyPagePanel = this.CreatePropertyPagePanel();
            }

            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = null;
            info.pszHelpFile = null;
            info.pszTitle = this.PageName;
            info.SIZE.cx = this.PropertyPagePanel.Width;
            info.SIZE.cy = this.PropertyPagePanel.Height;
            pPageInfo[0] = info;
        }

        /// <summary>
        /// Invokes the property page help in response to an end-user request.
        /// </summary>
        /// <param name="pszHelpDir">
        /// String under the HelpDir key in the property page's CLSID information in the registry.
        /// If HelpDir does not exist, this will be the path found in the InProcServer32 entry
        /// minus the server file name. (Note that LocalServer32 is not checked in the current
        /// implementation, since local property pages are not currently supported). 
        /// </param>
        void IPropertyPage.Help(string pszHelpDir)
        {
        }

        /// <summary>
        /// Indicates whether this property page has changed its state since the last call to
        /// <see cref="IPropertyPage.Apply"/>. The property sheet uses this information to enable
        /// or disable the Apply button in the dialog box.
        /// </summary>
        /// <returns>
        /// <b>S_OK</b> if the value state of the property page is dirty, that is, it has changed
        /// and is different from the state of the bound objects;
        /// <b>S_FALSE</b> if the value state of the page has not changed and is current with that
        /// of the bound objects.
        /// </returns>
        int IPropertyPage.IsPageDirty()
        {
            return (this.IsDirty ? VSConstants.S_OK : VSConstants.S_FALSE);
        }

        /// <summary>
        /// Repositions and resizes the property page dialog box according to the contents of
        /// <paramref name="pRect"/>. The rectangle specified by <paramref name="pRect"/> is
        /// treated identically to that passed to <see cref="IPropertyPage.Activate"/>.
        /// </summary>
        /// <param name="pRect">The bounds of the area that we should fill.</param>
        void IPropertyPage.Move(RECT[] pRect)
        {
            this.ResizeContents(pRect[0]);
        }

        /// <summary>
        /// The environment calls this to set the currently selected objects that the property page should show.
        /// </summary>
        /// <param name="count">The count of elements in <paramref name="punk"/>.</param>
        /// <param name="punk">An array of <b>IUnknown</b> objects to show in the property page.</param>
        /// <remarks>
        /// We are supposed to cache these objects until we get another call with <paramref name="count"/> = 0.
        /// Also, the environment is supposed to call this before calling <see cref="IPropertyPage.Activate"/>,
        /// but like all things when interacting with Visual Studio, don't trust that and code defensively.
        /// </remarks>
        void IPropertyPage.SetObjects(uint count, object[] punk)
        {
            if (count == 0)
            {
                this.project = null;
                return;
            }

            if (punk[0] is ProjectConfig)
            {
                List<ProjectConfig> configs = new List<ProjectConfig>();

                for (int i = 0; i < count; i++)
                {
                    ProjectConfig config = (ProjectConfig)punk[i];

                    if (this.project == null)
                    {
                        this.project = config.ProjectMgr as WixProjectNode;
                    }

                    configs.Add(config);
                }

                this.projectConfigs = configs.ToArray();
            }
            else if (punk[0] is NodeProperties)
            {
                if (this.project == null)
                {
                    this.project = (punk[0] as NodeProperties).Node.ProjectMgr as WixProjectNode;
                }

                Dictionary<string, ProjectConfig> configsMap = new Dictionary<string, ProjectConfig>();

                for (int i = 0; i < count; i++)
                {
                    NodeProperties property = (NodeProperties)punk[i];
                    IVsCfgProvider provider;
                    ErrorHandler.ThrowOnFailure(property.Node.ProjectMgr.GetCfgProvider(out provider));
                    uint[] expected = new uint[1];
                    ErrorHandler.ThrowOnFailure(provider.GetCfgs(0, null, expected, null));
                    if (expected[0] > 0)
                    {
                        ProjectConfig[] configs = new ProjectConfig[expected[0]];
                        uint[] actual = new uint[1];
                        provider.GetCfgs(expected[0], configs, actual, null);

                        foreach (ProjectConfig config in configs)
                        {
                            if (!configsMap.ContainsKey(config.ConfigName))
                            {
                                configsMap.Add(config.ConfigName, config);
                            }
                        }
                    }
                }

                if (configsMap.Count > 0)
                {
                    if (this.projectConfigs == null)
                    {
                        this.projectConfigs = new ProjectConfig[configsMap.Keys.Count];
                    }
                    configsMap.Values.CopyTo(this.projectConfigs, 0);
                }
            }

            if (this.active && this.project != null)
            {
                this.BindProperties();
                this.IsDirty = false;
            }
        }

        /// <summary>
        /// Initializes a property page and provides the property page object with the
        /// <see cref="IPropertyPageSite"/> interface through which the property page communicates
        /// with the property frame.
        /// </summary>
        /// <param name="pPageSite">
        /// The <see cref="IPropertyPageSite"/> that manages and provides services to this property
        /// page within the entire property sheet.
        /// </param>
        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite)
        {
            // pPageSite can be null (on deactivation)
            this.pageSite = pPageSite;
        }

        /// <summary>
        /// Makes the property page dialog box visible or invisible according to the <paramref name="nCmdShow"/>
        /// parameter. If the page is made visible, the page should set the focus to itself, specifically to the
        /// first property on the page.
        /// </summary>
        /// <param name="nCmdShow">
        /// Command describing whether to become visible (SW_SHOW or SW_SHOWNORMAL) or hidden (SW_HIDE). No other values are valid for this parameter.
        /// </param>
        void IPropertyPage.Show(uint nCmdShow)
        {
            if (this.PropertyPagePanel != null)
            {
                if (nCmdShow == SW_HIDE)
                {
                    this.PropertyPagePanel.Hide();
                }
                else
                {
                    this.PropertyPagePanel.Show();
                }
            }
        }

        /// <summary>
        /// Instructs the property page to process the keystroke described in <paramref name="pMsg"/>.
        /// </summary>
        /// <param name="pMsg">Describes the keystroke to process.</param>
        /// <returns>
        /// <list type="table">
        /// <item><term>S_OK</term><description>The property page handles the accelerator.</description></item>
        /// <item><term>S_FALSE</term><description>The property page handles accelerators, but this one was not useful to it.</description></item>
        /// <item><term>E_NOTIMPL</term><description>The proeprty page does not handle accelerators.</description></item>
        /// </list>
        /// </returns>
        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            WixHelperMethods.VerifyNonNullArgument(pMsg, "pMsg");

            // Always return S_FALSE otherwise we hijack all of the accelerators even for the menus
            return VSConstants.S_FALSE;
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Gets a configuration-specific property from the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>
        /// The configuration-specific property from the MSBuild project file if only one configuration is selected;
        /// otherwise, the union of all of the selected configurations.
        /// </returns>
        public string GetConfigProperty(string propertyName)
        {
            string unifiedResult = String.Empty;

            if (this.ProjectMgr != null)
            {
                bool cacheNeedReset = true;

                for (int i = 0; i < this.projectConfigs.Length; i++)
                {
                    ProjectConfig config = projectConfigs[i];
                    string property = config.GetConfigurationProperty(propertyName, cacheNeedReset);
                    cacheNeedReset = false;

                    if (property != null)
                    {
                        string text = property.Trim();

                        if (i == 0)
                        {
                            unifiedResult = text;
                        }
                        else if (unifiedResult != text)
                        {
                            unifiedResult = String.Empty; // tristate value is blank then
                            break;
                        }
                    }
                }
            }

            return unifiedResult;
        }

        /// <summary>
        /// Gets a boolean configuration-specific property from the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>
        /// The configuration-specific property from the MSBuild project file if only one configuration is selected;
        /// otherwise, the union of all of the selected configurations.
        /// </returns>
        public bool GetConfigPropertyBoolean(string propertyName)
        {
            string value = this.GetConfigProperty(propertyName);
            bool convertedValue;

            if (String.IsNullOrEmpty(value) || !Boolean.TryParse(value, out convertedValue))
            {
                return false;
            }

            return convertedValue;
        }

        /// <summary>
        /// Gets an integer configuration-specific property from the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>
        /// The configuration-specific property from the MSBuild project file if only one configuration is selected;
        /// otherwise, the union of all of the selected configurations.
        /// </returns>
        public int GetConfigPropertyInt32(string propertyName)
        {
            string value = this.GetConfigProperty(propertyName);
            int convertedValue;

            if (String.IsNullOrEmpty(value) || !Int32.TryParse(value, out convertedValue))
            {
                return WixProjectFileConstants.UnspecifiedValue;
            }

            return convertedValue;
        }

        /// <summary>
        /// Gets the property from the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value of the property from the project file, or <see cref="String.Empty"/> if the property doesn't exist.</returns>
        public string GetProperty(string propertyName)
        {
            if (this.ProjectMgr != null)
            {
                string property = this.ProjectMgr.GetProjectProperty(propertyName, true);

                if (property != null)
                {
                    return property;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets the literal property value from the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value of the property from the project file, or <see cref="String.Empty"/> if the property doesn't exist.</returns>
        public string GetLiteralProperty(string propertyName)
        {
            if (this.ProjectMgr != null)
            {
                BuildProperty property = this.ProjectMgr.GetMsBuildProperty(propertyName, true);

                if (property != null)
                {
                    return property.Value;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Sets a configuration-specific property in the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value of the property to set.</param>
        public void SetConfigProperty(string propertyName, bool propertyValue)
        {
            this.SetConfigProperty(propertyName, propertyValue.ToString());
        }

        /// <summary>
        /// Sets a configuration-specific property in the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value of the property to set.</param>
        public void SetConfigProperty(string propertyName, int propertyValue)
        {
            this.SetConfigProperty(propertyName, propertyValue.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets a configuration-specific property in the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value of the property to set.</param>
        public void SetConfigProperty(string propertyName, string propertyValue)
        {
            if (propertyValue == null)
            {
                propertyValue = String.Empty;
            }

            if (this.ProjectMgr != null)
            {
                for (int i = 0, n = this.projectConfigs.Length; i < n; i++)
                {
                    ProjectConfig config = projectConfigs[i];

                    config.SetConfigurationProperty(propertyName, propertyValue);
                }

                this.ProjectMgr.SetProjectFileDirty(true);
            }
        }

        /// <summary>
        /// Sets a property in the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value of the property to set.</param>
        public void SetProperty(string propertyName, string propertyValue)
        {
            if (propertyValue == null)
            {
                propertyValue = String.Empty;
            }

            if (this.ProjectMgr != null)
            {
                this.ProjectMgr.SetProjectProperty(propertyName, propertyValue);
            }
        }

        /// <summary>
        /// Sets a property in the MSBuild project file.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="propertyValue">The value of the property to set.</param>
        /// <param name="condition">The condition to use on the property. Corresponds to the Condition attribute of the Property element.</param>
        /// <param name="position">A <see cref="PropertyPosition"/> value indicating the location to insert the property.</param>
        /// <param name="treatPropertyValueAsLiteral">true to treat the <paramref name="propertyValue"/> parameter as a literal value; otherwise, false.</param>
        public void SetProperty(string propertyName, string propertyValue, string condition, PropertyPosition position, bool treatPropertyValueAsLiteral)
        {
            if (propertyValue == null)
            {
                propertyValue = String.Empty;
            }

            if (this.ProjectMgr != null)
            {
                this.ProjectMgr.SetProjectProperty(propertyName, propertyValue, condition, position, treatPropertyValueAsLiteral);
            }
        }

        /// <summary>
        /// Applies the changes made on the property page to the bound objects.
        /// </summary>
        /// <returns>
        /// true if the changes were successfully applied and the property page is current with the bound objects;
        /// false if the changes were applied, but the property page cannot determine if its state is current with the objects.
        /// </returns>
        protected abstract bool ApplyChanges();

        /// <summary>
        /// Binds the properties from the MSBuild project file to the controls on the property page.
        /// </summary>
        protected abstract void BindProperties();

        /// <summary>
        /// Creates the controls that constitute the property page. This should be safe to re-entrancy.
        /// </summary>
        /// <returns>The newly created main control that hosts the property page.</returns>
        protected abstract WixPropertyPagePanel CreatePropertyPagePanel();

        /// <summary>
        /// Resizes the property grid to the specified bounds.
        /// </summary>
        /// <param name="newBounds">The total area of the property page.</param>
        private void ResizeContents(RECT newBounds)
        {
            if (this.PropertyPagePanel != null && this.PropertyPagePanel.IsHandleCreated)
            {
                this.PropertyPagePanel.Bounds = new Rectangle(newBounds.left, newBounds.top, newBounds.right - newBounds.left, newBounds.bottom - newBounds.top);
            }
        }
    }
}