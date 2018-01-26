//-------------------------------------------------------------------------------------------------
// <copyright file="WixRecursiveActionException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception that occurs when user has loop in actions.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception that occurs when user has loop in actions.
    /// </summary>
    public class WixRecursiveActionException : WixException
    {
        private string actionName;
        private string actionTable;

        /// <summary>
        /// Creates recursive action exception.
        /// </summary>
        /// <param name="sourceLineNumbers">Optional source file and line number error occured at.</param>
        /// <param name="actionName">Name of action in loop.</param>
        /// <param name="actionTable">Table for action.</param>
        public WixRecursiveActionException(SourceLineNumberCollection sourceLineNumbers, string actionName, string actionTable) :
            base(sourceLineNumbers, WixExceptionType.RecursiveAction)
        {
            this.actionName = actionName;
            this.actionTable = actionTable;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Format("Action: {0} is recursively placed in the {1} table", this.actionName, this.actionTable); }
        }
    }
}
