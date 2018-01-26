//-------------------------------------------------------------------------------------------------
// <copyright file="WixNotIntermediateException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception thrown when trying to create an intermediate from a source that is not an intermediate.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception thrown when trying to create an intermediate from a source that is not an intermediate.
    /// </summary>
    [Serializable]
    public sealed class WixNotIntermediateException : WixException
    {
        /// <summary>
        /// Instantiate a new WixNotIntermediateException.
        /// </summary>
        /// <param name="error">Localized error information.</param>
        public WixNotIntermediateException(WixErrorEventArgs error)
            : base(error)
        {
        }
    }
}
