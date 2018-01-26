//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidOutputException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception throw when output file is corrupt.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception throw when output file is corrupt.
    /// </summary>
    public class WixInvalidOutputException : WixException
    {
        private string detail;

        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="sourceLineNumbers">File and line number where error happened.</param>
        /// <param name="detail">More information about error.</param>
        public WixInvalidOutputException(SourceLineNumberCollection sourceLineNumbers, string detail) :
            base(sourceLineNumbers, WixExceptionType.InvalidOutput)
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
                    return "Invalid output (.wixout) file.";
                }
                else
                {
                    return String.Concat("Invalid output file, detail: ", this.detail);
                }
            }
        }
    }
}
