//-------------------------------------------------------------------------------------------------
// <copyright file="WixNotLibraryException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception thrown when trying to create an library from a file that is not an library file.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception thrown when trying to create an library from a file that is not an library file.
    /// </summary>
    [Serializable]
    public sealed class WixNotLibraryException : WixException
    {
        /// <summary>
        /// Instantiate a new WixNotLibraryException.
        /// </summary>
        /// <param name="error">Localized error information.</param>
        public WixNotLibraryException(WixErrorEventArgs error)
            : base(error)
        {
        }
    }
}
