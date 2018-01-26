//-------------------------------------------------------------------------------------------------
// <copyright file="WixlibReferenceFolderNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A wixlib reference folder node within a Solution Explorer hierarchy.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Drawing;
    using System.IO;
    using Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure;

    /// <summary>
    /// Represents the root of the wixlib references in the Solution Explorer hierarchy.
    /// </summary>
    internal sealed class WixlibReferenceFolderNode : ReferenceFolderNode
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private static readonly Type classType = typeof(WixlibReferenceFolderNode);
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixlibReferenceFolderNode"/> class.
        /// </summary>
        /// <param name="hierarchy">The parent <see cref="Hierarchy"/> object.</param>
        /// <param name="rootDirectory">The absolute path to the folder.</param>
        public WixlibReferenceFolderNode(Hierarchy hierarchy, string rootDirectory)
            : base(hierarchy, rootDirectory, WixStrings.WixlibReferenceFolderCaption)
        {
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        #endregion
    }
}
