//-------------------------------------------------------------------------------------------------
// <copyright file="WixExtensionReferenceNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixExtensionReferenceNode class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using Microsoft.Tools.WindowsInstallerXml;
    using Microsoft.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Represents a Wix extension reference node.
    /// </summary>
    public class WixExtensionReferenceNode : WixReferenceNode
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private Version version = new Version();

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixExtensionReferenceNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        /// <param name="element">The element that contains MSBuild properties.</param>
        public WixExtensionReferenceNode(WixProjectNode root, ProjectElement element)
            : base(root, element)
        {
            this.ExtractPropertiesFromFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WixExtensionReferenceNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="WixProjectNode"/> that contains this node.</param>
        /// <param name="referencePath">The path to the wixlib reference file.</param>
        public WixExtensionReferenceNode(WixProjectNode root, string referencePath)
            : base(root, referencePath, WixProjectFileConstants.WixExtension)
        {
            this.ExtractPropertiesFromFile();
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the version of the wix extension file.
        /// </summary>
        public Version Version
        {
            get { return this.version; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Creates an object derived from <see cref="NodeProperties"/> that will be used to expose
        /// properties specific for this object to the property browser.
        /// </summary>
        /// <returns>A new <see cref="WixExtensionReferenceNodeProperties"/> object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            return new WixExtensionReferenceNodeProperties(this);
        }

        /// <summary>
        /// Opens the wixlib file and read properties from the file.
        /// </summary>
        private void ExtractPropertiesFromFile()
        {
            if (String.IsNullOrEmpty(this.Url) || !File.Exists(this.Url))
            {
                return;
            }

            try
            {
                byte[] rawAssembly = File.ReadAllBytes(this.Url);
                Assembly extensionAssembly = Assembly.ReflectionOnlyLoad(rawAssembly);
                this.version = extensionAssembly.GetName().Version;
            }
            catch (UnauthorizedAccessException e)
            {
                CCITracing.Trace(e);
            }
            catch (FileLoadException e)
            {
                CCITracing.Trace(e);
            }
            catch (BadImageFormatException e)
            {
                CCITracing.Trace(e);
            }
            catch (IOException e)
            {
                CCITracing.Trace(e);
            }
            catch (SecurityException e)
            {
                CCITracing.Trace(e);
            }
        }
    }
}
