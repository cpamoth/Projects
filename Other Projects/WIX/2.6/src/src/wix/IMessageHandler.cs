//-------------------------------------------------------------------------------------------------
// <copyright file="IMessageHandler.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Interface for handling messages (error/warning/verbose).
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Interface for handling messages (error/warning/verbose).
    /// </summary>
    internal interface IMessageHandler
    {
        /// <summary>
        /// Sends a message with the given arguments.
        /// </summary>
        /// <param name="mea">Message arguments.</param>
        void OnMessage(MessageEventArgs mea);
    }
}