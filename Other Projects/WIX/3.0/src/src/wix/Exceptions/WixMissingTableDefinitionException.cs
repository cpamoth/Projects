//-------------------------------------------------------------------------------------------------
// <copyright file="WixMissingTableDefinitionException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception thrown when a table definition is missing.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception thrown when a table definition is missing.
    /// </summary>
    [Serializable]
    public class WixMissingTableDefinitionException : WixException
    {
        /// <summary>
        /// Instantiate new WixMissingTableDefinitionException.
        /// </summary>
        /// <param name="error">Localized error information.</param>
        public WixMissingTableDefinitionException(WixErrorEventArgs error)
            : base(error)
        {
        }
    }
}
