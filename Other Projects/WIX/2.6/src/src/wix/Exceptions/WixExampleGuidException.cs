//-------------------------------------------------------------------------------------------------
// <copyright file="WixExampleGuidException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX example guid exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX example guid exception.
    /// </summary>
    public class WixExampleGuidException : WixException
    {
        /// <summary>
        /// Instantiate a new WixExampleGuidException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        public WixExampleGuidException(SourceLineNumberCollection sourceLineNumbers) :
            base(sourceLineNumbers, WixExceptionType.ExampleGuid)
        {
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return "A Guid needs to be generated and put in place of PUT-GUID-HERE in the source file."; }
        }
    }
}

