//-------------------------------------------------------------------------------------------------
// <copyright file="WixLibraryReferenceContainerNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLibraryReferenceContainerNode class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;

    using MSBuild = Microsoft.Build.BuildEngine;

    /// <summary>
    /// Represents the project's "References" node.
    /// </summary>
    public class WixLibraryReferenceContainerNode : ReferenceContainerNode
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private static readonly string[] supportedReferenceTypes = new string[]
            {
                WixProjectFileConstants.WixExtension,
                WixProjectFileConstants.WixLibrary,
                ProjectFileConstants.ProjectReference,
            };

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLibraryReferenceContainerNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        public WixLibraryReferenceContainerNode(WixProjectNode root)
            : base(root)
        {
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the caption to show in the Solution Explorer.
        /// </summary>
        public override string Caption
        {
            get { return WixStrings.WixLibraryReferencesFolderName; }
        }

        /// <summary>
        /// Gets the list of reference types (element names in the .wixproj file) that the Wix project supports.
        /// </summary>
        protected override string[] SupportedReferenceTypes
        {
            get { return supportedReferenceTypes; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Creates a new <see cref="WixLibraryReferenceNode"/> object corresponding to the file selected via the Add Reference dialog.
        /// </summary>
        /// <param name="selectorData">The data coming from the Add Reference dialog.</param>
        /// <returns>A new <see cref="WixLibraryReferenceNode"/>.</returns>
        protected override ReferenceNode CreateFileComponent(VSCOMPONENTSELECTORDATA selectorData)
        {
            if (!File.Exists(selectorData.bstrFile))
            {
                return null;
            }

            switch (Path.GetExtension(selectorData.bstrFile))
            {
                case ".wixlib":
                    return new WixLibraryReferenceNode(this.ProjectMgr as WixProjectNode, selectorData.bstrFile);

                case ".dll":
                    return new WixExtensionReferenceNode(this.ProjectMgr as WixProjectNode, selectorData.bstrFile);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates a new <see cref="WixLibraryReferenceNode"/> if the element is a "WixLibraryReference"
        /// or a new <see cref="WixExtensionReferenceNode"/> if the element is a "WixExtensionReference".
        /// </summary>
        /// <param name="reference">The MSBuild element pertaining to the reference.</param>
        protected override ReferenceNode CreateOtherReferenceNode(ProjectElement reference)
        {
            switch (reference.ItemName)
            {
                case WixProjectFileConstants.WixLibrary:
                    return new WixLibraryReferenceNode(this.ProjectMgr as WixProjectNode, reference);

                case WixProjectFileConstants.WixExtension:
                    return new WixExtensionReferenceNode(this.ProjectMgr as WixProjectNode, reference);
            }

            return base.CreateOtherReferenceNode(reference);
        }
    }
}
