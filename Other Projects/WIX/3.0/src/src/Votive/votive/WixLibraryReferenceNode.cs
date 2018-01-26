//-------------------------------------------------------------------------------------------------
// <copyright file="WixLibraryReferenceNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixLibraryReferenceNode class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Represents a Wixlib reference node.
    /// </summary>
    public class WixLibraryReferenceNode : WixReferenceNode
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLibraryReferenceNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        /// <param name="element">The element that contains MSBuild properties.</param>
        public WixLibraryReferenceNode(WixProjectNode root, ProjectElement element)
            : base(root, element)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixLibraryReferenceNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        /// <param name="referencePath">The path to the wixlib reference file.</param>
        public WixLibraryReferenceNode(WixProjectNode root, string referencePath)
            : base(root, referencePath, WixProjectFileConstants.WixLibrary)
        {
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Gets the icon handle for the node.
        /// </summary>
        /// <param name="open">Flag indicating whether the open or closed image should be returned.</param>
        /// <returns>A handle to the icon or null if the default icon should be used from the OS.</returns>
        public override object GetIconHandle(bool open)
        {
            return null;
        }

        /// <summary>
        /// Creates an object derived from <see cref="NodeProperties"/> that will be used to expose
        /// properties specific for this object to the property browser.
        /// </summary>
        /// <returns>A new <see cref="WixLibraryReferenceNodeProperties"/> object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            return new WixLibraryReferenceNodeProperties(this);
        }
    }
}
