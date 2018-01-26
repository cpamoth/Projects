//-------------------------------------------------------------------------------------------------
// <copyright file="WixNotIntermediateException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception thrown when trying to create an intermediate from a file that is not an object file.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception thrown when trying to create an intermediate from a file that is not an object file.
    /// </summary>
    public class WixNotIntermediateException : WixException
    {
        private string detail;

        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="sourceLineNumbers">Path to file that failed.</param>
        /// <param name="detail">Extra information about error.</param>
        public WixNotIntermediateException(SourceLineNumberCollection sourceLineNumbers, string detail) :
            base(sourceLineNumbers, WixExceptionType.NotIntermediate)
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
                    return "Not an object file.";
                }
                else
                {
                    return String.Format("Not an object file, detail: {0}.", this.detail);
                }
            }
        }
    }
}
