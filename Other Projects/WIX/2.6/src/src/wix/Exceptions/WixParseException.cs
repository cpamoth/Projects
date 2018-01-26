//-------------------------------------------------------------------------------------------------
// <copyright file="WixParseException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception thrown when a parsing problem occurs.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception throw when a parsing problem occurs.
    /// </summary>
    public class WixParseException : WixException
    {
        private string detail;

        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="detail">More information about error.</param>
        public WixParseException(string detail) :
            base(null, WixExceptionType.Parse)
        {
            this.detail = detail;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Concat("Error while parsing: ", this.detail); }
        }
    }
}
