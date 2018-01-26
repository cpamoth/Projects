//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidAssemblyException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception for an invalid assembly file.  This exception should only be thrown
// for assembly files that exist but are unusable for some particular reason.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Xml.Schema;

    /// <summary>
    /// Wrapper for XmlSchemaExceptions that parses the error text of the message
    /// to throw a more meaningful WixException.
    /// </summary>
    public class WixInvalidAssemblyException : WixException
    {
        private FileInfo assemblyFile;

        /// <summary>
        /// Instantiate a new WixInvalidAssemblyException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about where the exception occurred.</param>
        /// <param name="assemblyFile">The invalid assembly file.</param>
        /// <param name="exception">The original exception thrown for this error.</param>
        public WixInvalidAssemblyException(SourceLineNumberCollection sourceLineNumbers, FileInfo assemblyFile, Exception exception) :
            base(sourceLineNumbers, WixExceptionType.InvalidAssembly, exception)
        {
            this.assemblyFile = assemblyFile;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                return String.Format("Invalid assembly file '{0}'.  Please ensure this is a valid assembly file and that the user has the appropriate access rights to this file.", this.assemblyFile.FullName);
            }
        }
    }
}
