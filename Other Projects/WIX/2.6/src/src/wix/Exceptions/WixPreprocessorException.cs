//-------------------------------------------------------------------------------------------------
// <copyright file="WixPreprocessorException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exceptions for a preprocessor.
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
    public class WixPreprocessorException : WixException
    {
        private string message;

        /// <summary>
        /// Instantiate a new WixPreprocessorException.
        /// </summary>
        /// <param name="message">Message to display to the user.</param>
        public WixPreprocessorException(string message) :
            this(null, message)
        {
        }

        /// <summary>
        /// Instantiate a new WixPreprocessorException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number trace to where the exception occured.</param>
        /// <param name="message">Message to display to the user.</param>
        public WixPreprocessorException(SourceLineNumberCollection sourceLineNumbers, string message) :
            base(sourceLineNumbers, WixExceptionType.Preprocessor)
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

