//-------------------------------------------------------------------------------------------------
// <copyright file="StatusChangingEventArgs.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// New status data.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.ClickThrough
{
    using System;

    /// <summary>
    /// Delegate called when a status is changing.
    /// </summary>
    /// <param name="sender">Control that is changing status.</param>
    /// <param name="e">Data about the changing status.</param>
    public delegate void StatusChangingHandler(object sender, StatusChangingEventArgs e);

    /// <summary>
    /// New status data.
    /// </summary>
    public class StatusChangingEventArgs : EventArgs
    {
        private string message;

        /// <summary>
        /// Instantiate a new StatusChangeEventArgs class.
        /// </summary>
        public StatusChangingEventArgs()
        {
            this.message = null;
        }

        /// <summary>
        /// Instantiate a new StatusChangingEventArgs class.
        /// </summary>
        /// <param name="message">The message for the new status.</param>
        public StatusChangingEventArgs(string message)
        {
            this.message = message;
        }

        /// <summary>
        /// Gets the message for the new status.
        /// </summary>
        /// <value>The message for the new status.</value>
        public string Message
        {
            get { return this.message; }
        }
    }
}
