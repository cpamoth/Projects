//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidIntermediateException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX invalid intermediate exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX invalid intermediate exception.
    /// </summary>
    public class WixInvalidIntermediateException : WixException
    {
        private string detail;

        /// <summary>
        /// Instantiate a new WixInvalidIntermediateException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="detail">Detail of the exception.</param>
        public WixInvalidIntermediateException(SourceLineNumberCollection sourceLineNumbers, string detail) :
            base(sourceLineNumbers, WixExceptionType.InvalidIntermediate)
        {
            this.detail = detail;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                if (null == this.detail)
                {
                    return "Invalid object file.";
                }
                else
                {
                    return String.Format("Invalid object file, detail: {0}.", this.detail);
                }
            }
        }
    }
}
