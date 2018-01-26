//-------------------------------------------------------------------------------------------------
// <copyright file="WixExtensionNamespaceConflictException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WixException thrown when an extension is invalid.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WixException thrown when an extension is invalid.
    /// </summary>
    public class WixExtensionNamespaceConflictException : WixException
    {
        private const WixExceptionType ExceptionType = WixExceptionType.ExtensionNamespaceConflict;
        private SchemaExtension extension;
        private SchemaExtension existingExtension;

        /// <summary>
        /// Instantiate a new WixExtensionNamespaceConflictException.
        /// </summary>
        /// <param name="extension">Extension with the conflict.</param>
        /// <param name="existingExtension">Existing extension with the same namespace</param>
        public WixExtensionNamespaceConflictException(SchemaExtension extension, SchemaExtension existingExtension) :
            base(null, ExceptionType)
        {
            this.extension = extension;
            this.existingExtension = existingExtension;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                return String.Format("Extension '{0}' uses the same namespace '{1}' as already loaded extension '{2}'.  Either remove one of the extensions or rename the namespace to avoid the conflict.", this.extension.ToString(), this.extension.Schema.Namespaces, this.existingExtension.ToString());
            }
        }
    }
}
