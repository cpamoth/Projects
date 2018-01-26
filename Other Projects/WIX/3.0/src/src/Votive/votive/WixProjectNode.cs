//-------------------------------------------------------------------------------------------------
// <copyright file="WixProjectNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixProjectNode class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using Microsoft.Tools.WindowsInstallerXml.VisualStudio.PropertyPages;

    /// <summary>
    /// Represents the root node of a WiX project within a Solution Explorer hierarchy.
    /// </summary>
    [Guid("D79D1001-AD43-4a1d-AFD6-B6CBBE6B816B")]
    public class WixProjectNode : ProjectNode
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        internal const string ProjectTypeName = "WiX";

        private WixPackage package;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixProjectNode"/> class.
        /// </summary>
        /// <param name="package">The <see cref="WixPackage"/> to which this project belongs.</param>
        public WixProjectNode(WixPackage package)
        {
            WixHelperMethods.VerifyNonNullArgument(package, "package");

            this.package = package;

            // We allow destructive deletes on the project
            this.CanProjectDeleteItems = true;

            // use the VS 2005 style property pages (project designer) instead of the old VS 2003 dialog
            this.SupportsProjectDesigner = true;

            this.InitializeCATIDs();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the index of the node's image in the image list.
        /// </summary>
        public override int ImageIndex
        {
            get { return NoImage; }
        }

        /// <summary>
        /// Gets or sets the output type of the project.
        /// </summary>
        public WixOutputType OutputType
        {
            get
            {
                string outputTypeString = this.GetProjectProperty(WixProjectFileConstants.OutputType);
                WixOutputType outputType = WixOutputType.Package;

                try
                {
                    outputType = (WixOutputType)Enum.Parse(typeof(WixOutputType), outputTypeString, true);
                }
                catch (ArgumentException)
                {
                }

                return outputType;
            }

            set
            {
                this.SetProjectProperty(WixProjectFileConstants.OutputType, value.ToString());
            }
        }

        /// <summary>
        /// Gets the project type GUID, which is registered with Visual Studio.
        /// </summary>
        public override Guid ProjectGuid
        {
            get { return typeof(WixProjectFactory).GUID; }
        }

        /// <summary>
        /// Returns a caption for VSHPROPID_TypeName.
        /// </summary>
        public override string ProjectType
        {
            get { return ProjectTypeName; }
        }

        /// <summary>
        /// Gets the filter used in the Add Reference dialog box.
        /// </summary>
        private string AddReferenceDialogFilter
        {
            get { return WixStrings.AddReferenceDialogFilter.Replace("\\0", "\0"); }
        }

        /// <summary>
        /// Gets the initial directory for the Add Reference dialog box.
        /// </summary>
        private string AddReferenceDialogInitialDirectory
        {
            get
            {
                // get the tools directory from the registry, which has the wixlibs that we ship with
                string toolsDirectory = this.package.Settings.ToolsDirectory;
                if (String.IsNullOrEmpty(toolsDirectory) || !Directory.Exists(toolsDirectory))
                {
                    return Directory.GetCurrentDirectory();
                }

                return toolsDirectory;
            }
        }

        /// <summary>
        /// Gets the title used in the Add Reference dialog box.
        /// </summary>
        private string AddReferenceDialogTitle
        {
            get { return WixStrings.AddReferenceDialogTitle; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Hides all of the tabs in the Add Reference dialog except for the browse tab, which will search for wixlibs.
        /// </summary>
        /// <returns></returns>
        public override int AddProjectReference()
        {
            CCITracing.TraceCall();

            Guid emptyGuid = Guid.Empty;
            Guid showOnlyThisTabGuid = Guid.Empty;
            Guid startOnThisTabGuid = VSConstants.GUID_BrowseFilePage;
            string helpTopic = "VS.AddReference";
            string machineName = String.Empty;
            string browseFilters = this.AddReferenceDialogFilter;
            string browseLocation = this.AddReferenceDialogInitialDirectory;

            // initialize the structure that we have to pass into the dialog call
            VSCOMPONENTSELECTORTABINIT[] tabInitializers = new VSCOMPONENTSELECTORTABINIT[2];

            // tab 1 is the Project References tab: passing VSHPROPID_ShowProjInSolutionPage will tell the Add Reference
            // dialog to call into our GetProperty to determine if we should show ourself in the dialog
            tabInitializers[0].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            tabInitializers[0].guidTab = VSConstants.GUID_SolutionPage;
            tabInitializers[0].varTabInitInfo = (int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage;

            // tab 2 is the Browse tab
            tabInitializers[1].dwSize = (uint)Marshal.SizeOf(typeof(VSCOMPONENTSELECTORTABINIT));
            tabInitializers[1].guidTab = VSConstants.GUID_BrowseFilePage;
            tabInitializers[1].varTabInitInfo = 0;

            // initialize the flags to control the dialog
            __VSCOMPSELFLAGS flags = __VSCOMPSELFLAGS.VSCOMSEL_HideCOMClassicTab |
                __VSCOMPSELFLAGS.VSCOMSEL_HideCOMPlusTab |
                __VSCOMPSELFLAGS.VSCOMSEL_IgnoreMachineName |
                __VSCOMPSELFLAGS.VSCOMSEL_MultiSelectMode;

            // get the dialog service from the environment
            IVsComponentSelectorDlg dialog = WixHelperMethods.GetService<IVsComponentSelectorDlg, SVsComponentSelectorDlg>(this.Site);

            try
            {
                // tell ourself not to show our project in the Add Reference dialog
                this.ShowProjectInSolutionPage = false;

                // show the dialog
                ErrorHandler.ThrowOnFailure(dialog.ComponentSelectorDlg(
                    (uint)flags,
                    (IVsComponentUser)this,
                    this.AddReferenceDialogTitle,
                    helpTopic,
                    ref showOnlyThisTabGuid,
                    ref startOnThisTabGuid,
                    machineName,
                    (uint)tabInitializers.Length,
                    tabInitializers,
                    browseFilters,
                    ref browseLocation));
            }
            catch (COMException e)
            {
                CCITracing.Trace(e);
                return e.ErrorCode;
            }
            finally
            {
                // we can show ourself in the Add Reference dialog if somebody else invokes it
                this.ShowProjectInSolutionPage = true;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Create a file node based on an MSBuild item.
        /// </summary>
        /// <param name="item">MSBuild item</param>
        /// <returns>The added <see cref="FileNode"/>.</returns>
        public override FileNode CreateFileNode(ProjectElement item)
        {
            return new WixFileNode(this, item);
        }

        /// <summary>
        /// Returns a value indicating whether the specified file is a code file, i.e. compileable.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <returns><see langword="true"/> if the file is compileable; otherwise, <see langword="false"/>.</returns>
        public override bool IsCodeFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return String.Equals(".wxs", extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a value indicating whether the specified file is an embedded resource.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <returns><see langword="true"/> if the file is an embedded resource; otherwise, <see langword="false"/>.</returns>
        public override bool IsEmbeddedResource(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return String.Equals(extension, ".wxl", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a file to the MSBuild project.
        /// </summary>
        /// <param name="file">The file to be added.</param>
        /// <returns>A <see cref="ProjectElement"/> describing the newly added file.</returns>
        protected override ProjectElement AddFileToMsBuild(string file)
        {
            ProjectElement newItem;

            string itemPath = PackageUtilities.MakeRelativeIfRooted(file, this.BaseURI);
            Debug.Assert(!Path.IsPathRooted(itemPath), "Cannot add item with full path.");

            if (this.IsCodeFile(itemPath))
            {
                newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.Compile);
            }
            else if (this.IsEmbeddedResource(itemPath))
            {
                newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.EmbeddedResource);
            }
            else
            {
                newItem = this.CreateMsBuildFileItem(itemPath, ProjectFileConstants.Content);
            }

            return newItem;
        }

        /// <summary>
        /// Creates an object derived from <see cref="NodeProperties"/> that will be used to expose
        /// properties specific for this object to the property browser.
        /// </summary>
        /// <returns>A new <see cref="WixProjectNodeProperties"/> object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            return new WixProjectNodeProperties(this);
        }

        /// <summary>
        /// Creates a type-specific <see cref="WixLibraryReferenceContainerNode"/> for the project.
        /// </summary>
        /// <returns>A new <see cref="WixLibraryReferenceContainerNode"/> instance.</returns>
        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return new WixLibraryReferenceContainerNode(this);
        }

        /// <summary>
        /// Gets an array of property page GUIDs that are configuration dependent.
        /// </summary>
        /// <returns>An array of property page GUIDs that are configuration dependent.</returns>
        protected override Guid[] GetConfigurationDependentPropertyPages()
        {
            // depending on the project type, either the linker (light) or librarian (lit) property pages are shown
            Guid linkerOrLibrarianGuid = (this.OutputType == WixOutputType.Library ? typeof(WixLibrarianPropertyPage).GUID : typeof(WixLinkerPropertyPage).GUID);

            Guid[] result = new Guid[]
            {
                typeof(WixCompilerPropertyPage).GUID,
                linkerOrLibrarianGuid,
            };

            return result;
        }

        /// <summary>
        /// Gets an array of property page GUIDs that are common, or not dependent upon the configuration.
        /// </summary>
        /// <returns>An array of property page GUIDs that are independent upon the configuration.</returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[]
            {
                typeof(WixBuildPropertyPage).GUID,
                typeof(WixBuildEventsPropertyPage).GUID,
            };

            return result;
        }

        /// <summary>
        /// Initialize common project properties with default value if they are empty.
        /// </summary>
        /// <remarks>
        /// The following common project properties are defaulted to projectName (if empty): ToolPath.
        /// If the project filename is not set then no properties are set.
        /// </remarks>
        protected override void InitializeProjectProperties()
        {
            // make sure the project file name has been set
            if (String.IsNullOrEmpty(this.FileName) || String.IsNullOrEmpty(Path.GetFileNameWithoutExtension(this.FileName)))
            {
                return;
            }

            // initialize the WixToolPath property from the value in the registry
            string toolsDirectory = this.package.Settings.ToolsDirectory;
            string programFilesDirectory = WixHelperMethods.EnsureTrailingDirectoryChar(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

            // if the path in the registry starts with the same path as program files, then just put in $(ProgramFiles) to make it more portable
            if (toolsDirectory.StartsWith(programFilesDirectory, StringComparison.OrdinalIgnoreCase))
            {
                toolsDirectory = "$(ProgramFiles)\\" + toolsDirectory.Substring(programFilesDirectory.Length);
            }

            this.SetProjectProperty(WixProjectFileConstants.WixToolPath, toolsDirectory);
        }

        /// <summary>
        /// Sets the value of an MSBuild project property.
        /// </summary>
        /// <param name="propertyName">The name of the property to change.</param>
        /// <param name="propertyValue">The value to assign the property.</param>
        public override void SetProjectProperty(string propertyName, string propertyValue)
        {
            this.SetProjectProperty(propertyName, propertyValue, null, PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
        }

        /// <summary>
        /// Sets the value of an MSBuild project property.
        /// </summary>
        /// <param name="propertyName">The name of the property to change.</param>
        /// <param name="propertyValue">The value to assign the property.</param>
        /// <param name="condition">The condition to use on the property. Corresponds to the Condition attribute of the Property element.</param>
        /// <param name="position">A <see cref="PropertyPosition"/> value indicating the location to insert the property.</param>
        /// <param name="treatPropertyValueAsLiteral">true to treat the <paramref name="propertyValue"/> parameter as a literal value; otherwise, false.</param>
        public void SetProjectProperty(string propertyName, string propertyValue, string condition, PropertyPosition position, bool treatPropertyValueAsLiteral)
        {
            WixHelperMethods.VerifyStringArgument(propertyName, "propertyName");

            if (propertyValue == null)
            {
                propertyValue = String.Empty;
            }

            // see if the value is the same as what's already in the project so we
            // know whether to actually mark the project file dirty or not
            string oldValue = this.GetProjectProperty(propertyName, true);

            if (!String.Equals(oldValue, propertyValue, StringComparison.Ordinal))
            {
                // check out the project file
                if (!this.ProjectMgr.QueryEditProjectFile(false))
                {
                    throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                }

                this.BuildProject.SetProperty(propertyName, propertyValue, condition, position, treatPropertyValueAsLiteral);

                // refresh the cached values
                this.SetCurrentConfiguration();
                this.SetProjectFileDirty(true);
            }
        }

        /// <summary>
        /// Executes an MSBuild target.
        /// </summary>
        /// <param name="target">Name of the MSBuild target to execute.</param>
        /// <returns>Result from executing the target (success/failure).</returns>
        protected override MSBuildResult InvokeMsBuild(string target)
        {
            this.DefineSolutionProperties();
            return base.InvokeMsBuild(target);
        }

        /// <summary>
        /// When building with only a wixproj in the solution, the SolutionX variables are not
        /// defined, so we have to define them here.
        /// </summary>
        internal void DefineSolutionProperties()
        {
            IVsSolution solution = WixHelperMethods.GetService<IVsSolution, SVsSolution>(this.Site);
            object solutionPathObj;
            ErrorHandler.ThrowOnFailure(solution.GetProperty((int)__VSPROPID.VSPROPID_SolutionFileName, out solutionPathObj));
            string solutionPath = (string)solutionPathObj;
            string devEnvDir = WixHelperMethods.EnsureTrailingDirectoryChar(Path.GetDirectoryName(this.package.Settings.DevEnvPath));

            string[][] properties = new string[][]
                {
                    new string [] { WixProjectFileConstants.DevEnvDir, devEnvDir },
                    new string [] { WixProjectFileConstants.SolutionPath, solutionPath },
                    new string [] { WixProjectFileConstants.SolutionDir, WixHelperMethods.EnsureTrailingDirectoryChar(Path.GetDirectoryName(solutionPath)) },
                    new string [] { WixProjectFileConstants.SolutionExt, Path.GetExtension(solutionPath) },
                    new string [] { WixProjectFileConstants.SolutionFileName, Path.GetFileName(solutionPath) },
                    new string [] { WixProjectFileConstants.SolutionName,Path.GetFileNameWithoutExtension(solutionPath) },
                };

            foreach (string[] property in properties)
            {
                string propertyName = property[0];
                string propertyValue = property[1];

                this.BuildEngine.GlobalProperties.SetProperty(propertyName, propertyValue);
            }
        }

        /// <summary>
        /// Provide mapping from our browse objects and automation objects to our CATIDs.
        /// </summary>
        private void InitializeCATIDs()
        {
            // The following properties classes are specific to wix so we can use their GUIDs directly
            this.AddCATIDMapping(typeof(WixFileNodeProperties), typeof(WixFileNodeProperties).GUID);
            this.AddCATIDMapping(typeof(WixProjectNodeProperties), typeof(WixProjectNodeProperties).GUID);
            this.AddCATIDMapping(typeof(WixExtensionReferenceNodeProperties), typeof(WixExtensionReferenceNodeProperties).GUID);
            this.AddCATIDMapping(typeof(WixLibraryReferenceNodeProperties), typeof(WixLibraryReferenceNodeProperties).GUID);

            // The following are not specific to wix and as such we need a separate GUID (we simply used guidgen.exe to create new guids)
            this.AddCATIDMapping(typeof(FolderNodeProperties), new Guid("86AD5EAF-5629-4fe3-8F8E-E1D9DA0A81C3"));
            this.AddCATIDMapping(typeof(FileNodeProperties), new Guid("BC59A9F0-9C85-44f9-B133-4A019BDC9739"));
        }
    }
}
