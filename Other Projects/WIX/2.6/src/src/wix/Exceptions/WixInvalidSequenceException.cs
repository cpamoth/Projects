//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidSequenceException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX invalid sequence exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX invalid sequence exception.
    /// </summary>
    public class WixInvalidSequenceException : WixException
    {
        private string actionId;

        /// <summary>
        /// Instantiate new WixInvalidSequenceException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="actionId">Id of the custom action with an invalid sequence.</param>
        public WixInvalidSequenceException(SourceLineNumberCollection sourceLineNumbers, string actionId) :
            base(sourceLineNumbers, WixExceptionType.InvalidSequence)
        {
            this.actionId = actionId;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                return String.Concat("Invalid sequence number for Action: ", this.actionId);
            }
        }
    }
}
