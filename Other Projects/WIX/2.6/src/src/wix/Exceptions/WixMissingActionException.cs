//-------------------------------------------------------------------------------------------------
// <copyright file="WixMissingActionException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX missing action exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX missing action exception.
    /// </summary>
    public class WixMissingActionException : WixException
    {
        private string actionName;
        private string actionParentName;

        /// <summary>
        /// Instantiate a new WixMissingActionException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="actionParentName">Name of the parent action which could not be found.</param>
        /// <param name="actionName">Name of the action with a missing parent.</param>
        public WixMissingActionException(SourceLineNumberCollection sourceLineNumbers, string actionParentName, string actionName) :
            base(sourceLineNumbers, WixExceptionType.MissingAction)
        {
            this.actionName = actionName;
            this.actionParentName = actionParentName;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                return String.Format("Parent action: {0} could not be found for Action: {1}", this.actionParentName, this.actionName);
            }
        }
    }
}
