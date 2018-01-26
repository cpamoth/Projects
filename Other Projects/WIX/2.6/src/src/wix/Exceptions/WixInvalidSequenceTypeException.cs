//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidSequenceTypeException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX invalid sequence type exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX invalid sequence type exception.
    /// </summary>
    public class WixInvalidSequenceTypeException : WixException
    {
        private string sequenceTypeName;

        /// <summary>
        /// Instantiate a new WixInvalidSequenceTypeException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="sequenceTypeName">Name of the invalid sequence type.</param>
        public WixInvalidSequenceTypeException(SourceLineNumberCollection sourceLineNumbers, string sequenceTypeName) :
            base(sourceLineNumbers, WixExceptionType.InvalidSequenceType)
        {
            this.sequenceTypeName = sequenceTypeName;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Concat("Unknown sequence type: ", this.sequenceTypeName); }
        }
    }
}
