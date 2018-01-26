//-------------------------------------------------------------------------------------------------
// <copyright file="WixlibReferenceFileNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A wixlib file node within a Solution Explorer hierarchy.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure;

    /// <summary>
    /// A WiX library (wixlib) file node within the Solution Explorer hierarchy.
    /// </summary>
    internal sealed class WixlibReferenceFileNode : ReferenceFileNode
    {
        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        public WixlibReferenceFileNode(Hierarchy hierarchy, string absolutePath) : base(hierarchy, absolutePath)
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
