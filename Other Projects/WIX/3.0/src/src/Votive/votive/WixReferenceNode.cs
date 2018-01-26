//-------------------------------------------------------------------------------------------------
// <copyright file="WixReferenceNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixReferenceNode class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Abstract base class for a Wix reference node.
    /// </summary>
    public abstract class WixReferenceNode : ReferenceNode
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixReferenceNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        /// <param name="element">The element that contains MSBuild properties.</param>
        protected WixReferenceNode(WixProjectNode root, ProjectElement element)
            : base(root, element)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixReferenceNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        /// <param name="referencePath">The path to the wixlib reference file.</param>
        /// <param name="msBuildElementName">The element name of the reference in an MSBuild file.</param>
        protected WixReferenceNode(WixProjectNode root, string referencePath, string msBuildElementName)
            : this(root, new ProjectElement(root, referencePath, msBuildElementName))
        {
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the caption shown in the Solution Explorer.
        /// </summary>
        public override string Caption
        {
            get
            {
                string caption = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
                caption = Path.GetFileNameWithoutExtension(caption);
                return caption;
            }
        }

        /// <summary>
        /// Gets the absolute path to the reference file.
        /// </summary>
        public override string Url
        {
            get
            {
                string path = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
                if (String.IsNullOrEmpty(path))
                {
                    return String.Empty;
                }

                Url url;
                if (Path.IsPathRooted(path))
                {
                    // use absolute path
                    url = new Microsoft.VisualStudio.Shell.Url(path);
                }
                else
                {
                    // path is relative, so make it relative to project path
                    url = new Url(this.ProjectMgr.BaseURI, path);
                }

                return url.AbsoluteUrl;
            }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Links a reference node to the project and hierarchy.
        /// </summary>
        protected override void BindReferenceData()
        {
            WixHelperMethods.ShipAssert(this.ItemNode != null, "The MSBuild ItemNode should have been set by now.");

            // resolve the references, which will copy the files locally if the Private flag is set
            this.ProjectMgr.Build(WixProjectFileConstants.MsBuildTarget.ResolveWixLibraryReferences);
        }

        /// <summary>
        /// Determines if this is node a valid node for painting the default reference icon.
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowDefaultIcon()
        {
            return (!String.IsNullOrEmpty(this.Url) && File.Exists(this.Url));
        }
    }
}
