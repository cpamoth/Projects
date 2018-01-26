//-------------------------------------------------------------------------------------------------
// <copyright file="MessageEventArgs.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
// Event args for message events.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Event args for message events.
    /// </summary>
    public abstract class MessageEventArgs : EventArgs
    {
        private SourceLineNumberCollection sourceLineNumbers;
        private int id;
        private string resourceName;
        private object[] messageArgs;

        /// <summary>
        /// Creates a new MessageEventArgs.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers for the message.</param>
        /// <param name="id">Id for the message.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="messageArgs">Arguments for the format string.</param>
        protected MessageEventArgs(SourceLineNumberCollection sourceLineNumbers, int id, string resourceName, params object[] messageArgs)
        {
            this.sourceLineNumbers = sourceLineNumbers;
            this.id = id;
            this.resourceName = resourceName;
            this.messageArgs = messageArgs;
        }

        /// <summary>
        /// Gets the resource manager for this event args.
        /// </summary>
        /// <value>The resource manager for this event args.</value>
        public abstract ResourceManager ResourceManager
        {
            get;
        }

        /// <summary>
        /// Gets the source line numbers.
        /// </summary>
        /// <value>The source line numbers.</value>
        public SourceLineNumberCollection SourceLineNumbers
        {
            get { return this.sourceLineNumbers; }
        }

        /// <summary>
        /// Gets the Id for the message.
        /// </summary>
        /// <value>The Id for the message.</value>
        public int Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        /// <value>The name of the resource.</value>
        public string ResourceName
        {
            get { return this.resourceName; }
        }

        /// <summary>
        /// Gets the arguments for the format string.
        /// </summary>
        /// <value>The arguments for the format string.</value>
        public object[] MessageArgs
        {
            get { return this.messageArgs; }
        }
    }
}