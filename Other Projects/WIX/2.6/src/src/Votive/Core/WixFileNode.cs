//-------------------------------------------------------------------------------------------------
// <copyright file="WixFileNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A WiX file node within a Solution Explorer hierarchy.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Globalization;
    using System.IO;
    using Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// A WiX file node within a Solution Explorer hierarchy.
    /// </summary>
    internal class WixFileNode : FileNode
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private static readonly Type classType = typeof(WixFileNode);
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        public WixFileNode(Hierarchy hierarchy, string absolutePath) : base(hierarchy, absolutePath, BuildAction.Compile)
        {
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        /// <summary>
        /// Queries the state of a command from the standard Visual Studio 97 command set on this node.
        /// </summary>
        /// <param name="command">The command to query.</param>
        /// <returns>One of the <see cref="CommandStatus"/> values if the node handles the command;
        /// otherwise <see cref="CommandStatus.Unhandled"/>.</returns>
        public override CommandStatus QueryStandard97CommandStatus(VsCommand command)
        {
            CommandStatus status = CommandStatus.Unhandled;

            switch (command)
            {
                case VsCommand.ViewCode:
                    status = CommandStatus.SupportedAndEnabled;
                    break;

                default:
                    status = base.QueryStandard97CommandStatus(command);
                    break;
            }

            return status;
        }
        #endregion
    }
}
