//-------------------------------------------------------------------------------------------------
// <copyright file="VerboseEventArgs.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Event arguments for verbose messages.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Event arguments for verbose messages.
    /// </summary>
    public class VerboseEventArgs : EventArgs 
    {
        private VerboseLevel level;
        private string message;

        /// <summary>
        /// VerboseEventArgs Constructor.
        /// </summary>
        /// <param name="level">Level of the verbose message.</param>
        /// <param name="message">Verbose message content.</param>
        public VerboseEventArgs(VerboseLevel level, string message)
        {
            this.level = level;
            this.message = message;
        }

        /// <summary>
        /// Getter for the verbose level.
        /// </summary>
        /// <value>The level of this message.</value>
        public VerboseLevel Level
        {
            get { return this.level; }
        }

        /// <summary>
        /// Getter for the message content.
        /// </summary>
        /// <value>The message content.</value>
        public string Message
        {
            get { return this.message; }
        }
    }
}