//-------------------------------------------------------------------------------------------------
// <copyright file="WixMissingTableDefinitionException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX missing table defintion exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX missing table defintion exception.
    /// </summary>
    public class WixMissingTableDefinitionException : WixException
    {
        private string tableName;

        /// <summary>
        /// Instantiate new WixMissingTableDefinitionException.
        /// </summary>
        /// <param name="tableName">The name of the table for which a defintion could not be found.</param>
        public WixMissingTableDefinitionException(string tableName) :
            base(null, WixExceptionType.MissingTableDefintion)
        {
            this.tableName = tableName;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Format("Cannot find the table definitions for the '{0}' table.  This is likely due to a missing schema extension.  Please ensure all the necessary extensions are supplied on the command line with the -ext parameter.", this.tableName); }
        }
    }
}
