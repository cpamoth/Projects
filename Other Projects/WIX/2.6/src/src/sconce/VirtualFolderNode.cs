//-------------------------------------------------------------------------------------------------
// <copyright file="VirtualFolderNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A virtual folder node within a Solution Explorer hierarchy.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Represents a virtual folder within a Solution Explorer hierarchy, virtual in the sense that
    /// there is not an underlying OS folder.
    /// </summary>
    public class VirtualFolderNode : FolderNode
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private static readonly Type classType = typeof(VirtualFolderNode);
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        public VirtualFolderNode(Hierarchy hierarchy, string absolutePath) : base(hierarchy, absolutePath)
        {
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        public override bool IsVirtual
        {
            get { return true; }
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        #endregion
    }
}
