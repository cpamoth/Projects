//-------------------------------------------------------------------------------------------------
// <copyright file="WixNotOutputException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception thrown when trying to create an output from a file that is not an output file.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception thrown when trying to create an output from a file that is not an output file.
    /// </summary>
    [Serializable]
    public sealed class WixNotOutputException : WixException
    {
        /// <summary>
        /// Instantiate a new WixNotOutputException.
        /// </summary>
        /// <param name="error">Localized error information.</param>
        public WixNotOutputException(WixErrorEventArgs error)
            : base(error)
        {
        }
    }
}
