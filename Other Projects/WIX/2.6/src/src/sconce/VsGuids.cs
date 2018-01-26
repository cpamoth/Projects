//-------------------------------------------------------------------------------------------------
// <copyright file="VsGuids.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains standard Visual Studio GUIDs.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;

    /// <summary>
    ///     Visual Studio standard editor, window frame, and other GUIDs
    /// </summary>
    public sealed class VsGuids
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        public static readonly Guid XmlEditor = new Guid("{C76D83F8-A489-11D0-8195-00A0C91BBEE3}");
        public static readonly Guid SolutionExplorer = new Guid("3AE79031-E1BC-11D0-8F78-00A0C9110057");
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================
    
        /// <summary>
        ///     Prevent direct instantiation of this static class.
        /// </summary>
        private VsGuids()
        {
        }
        #endregion
    }
}