//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidCodepageException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX merge failure exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX invalid codepage exception.
    /// </summary>
    public class WixInvalidCodepageException : WixException
    {
        private int codepage;

        /// <summary>
        /// Instantiate a new WixInvalidCodepageException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="codepage">Invalid codepage.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WixInvalidCodepageException(SourceLineNumberCollection sourceLineNumbers, int codepage, Exception innerException) :
            base(sourceLineNumbers, WixExceptionType.InvalidCodepage, innerException)
        {
            this.codepage = codepage;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Format("Codepage \"{0}\" is not a valid codepage. Check PatchCreation/@Codepage, Product/@Codepage, or Module/@Codepage in your source file, or WixLocalization/@Codepage in your localization file.", this.codepage); }
        }
    }
}