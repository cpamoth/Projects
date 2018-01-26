//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidIdtException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX invalid idt exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;

    /// <summary>
    /// WiX invalid idt exception.
    /// </summary>
    public class WixInvalidIdtException : WixException
    {
        private string tableName;
        private FileInfo idtPath;

        /// <summary>
        /// Instantiate a new WixInvalidIdtException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="tableName">Name of the table that could not be imported with an idt file.</param>
        /// <param name="fileName">Name of the idt file that was invalid.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WixInvalidIdtException(SourceLineNumberCollection sourceLineNumbers, string tableName, string fileName, Exception innerException) :
            base(sourceLineNumbers, WixExceptionType.InvalidIdt, innerException)
        {
            this.tableName = tableName;
            this.idtPath = new FileInfo(fileName);
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Format("There was an error importing table: {0} with file: {1}", this.tableName, this.idtPath.FullName); }
        }
    }
}
