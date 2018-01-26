//-------------------------------------------------------------------------------------------------
// <copyright file="WixWarningAsErrorException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX warning as error exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX warning as error exception.
    /// </summary>
    public class WixWarningAsErrorException : WixException
    {
        private string message;

        /// <summary>
        /// Instantiate a new WixWarningAsErrorException.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception, or an empty string("").</param>
        public WixWarningAsErrorException(string message) :
            base(null, WixExceptionType.WarningAsError, null)
        {
            this.message = message;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return this.message; }
        }
    }
}

