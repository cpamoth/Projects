//-------------------------------------------------------------------------------------------------
// <copyright file="WixMissingDirectoryException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception for a missing directory.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Create a new exception for a missing directory.
    /// </summary>
    public class WixMissingDirectoryException : WixException
    {
        private string directory;

        /// <summary>
        /// Instantiate a new WixMissingDirectoryException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about where the exception occurred.</param>
        /// <param name="directory">Directory identifier.</param>
        public WixMissingDirectoryException(SourceLineNumberCollection sourceLineNumbers, string directory) :
            base(sourceLineNumbers, WixExceptionType.MissingDirectory)
        {
            this.directory = directory;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                return String.Format("Missing directory '{0}'.", this.directory);
            }
        }
    }
}