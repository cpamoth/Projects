//-------------------------------------------------------------------------------------------------
// <copyright file="IExtensionMessageHandler.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Interface for handling messages (error/warning/verbose) from an extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Interface for handling messages (error/warning/verbose) from an extension.
    /// </summary>
    internal interface IExtensionMessageHandler
    {
        /// <summary>
        /// Sends an error to the message delegate if there is one.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers.</param>
        /// <param name="errorLevel">Level of the error message.</param>
        /// <param name="errorMessage">Error message string.</param>
        void OnExtensionError(SourceLineNumberCollection sourceLineNumbers, ErrorLevel errorLevel, string errorMessage);

        /// <summary>
        /// Sends a warning to the message delegate if there is one.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers.</param>
        /// <param name="warningLevel">Level of the warning message.</param>
        /// <param name="warningMessage">Warning message string.</param>
        void OnExtensionWarning(SourceLineNumberCollection sourceLineNumbers, WarningLevel warningLevel, string warningMessage);

        /// <summary>
        /// Sends an error to the message delegate if there is one.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers.</param>
        /// <param name="verboseLevel">Level of the verbose message.</param>
        /// <param name="verboseMessage">Verbose message string.</param>
        void OnExtensionVerbose(SourceLineNumberCollection sourceLineNumbers, VerboseLevel verboseLevel, string verboseMessage);
    }
}