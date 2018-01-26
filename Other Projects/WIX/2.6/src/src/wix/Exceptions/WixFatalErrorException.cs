//-------------------------------------------------------------------------------------------------
// <copyright file="WixFatalErrorException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception for when fatal errors occur.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX fatal error exception.
    /// </summary>
    public class WixFatalErrorException : WixException
    {
        private MessageEventArgs messageEventArgs;

        /// <summary>
        /// Instantiate a new WixFatalErrorException.
        /// </summary>
        /// <param name="messageEventArgs">Message event args.</param>
        public WixFatalErrorException(MessageEventArgs messageEventArgs) :
            base(null, WixExceptionType.FatalError, null)
        {
            this.messageEventArgs = messageEventArgs;
        }
    }
}
