//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidExtensionException.cs" company="Microsoft">
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
    public class WixInvalidExtensionException : WixException
    {
        private const WixExceptionType ExceptionType = WixExceptionType.InvalidExtension;
        private string typeName;
        private Type actualType;
        private Type expectedType;
        private Type expectedType2;

        /// <summary>
        /// Instantiate a new WixInvalidExtensionException.
        /// </summary>
        /// <param name="typeName">User-specified name of the type containing an extension.</param>
        public WixInvalidExtensionException(string typeName) :
            base(null, ExceptionType)
        {
            this.typeName = typeName;
        }

        /// <summary>
        /// Instantiate a new WixInvalidExtensionException.
        /// </summary>
        /// <param name="typeName">User-specified name of the type containing an extension.</param>
        /// <param name="actualType">Actual type of the loaded extension.</param>
        /// <param name="expectedType">Expected type of the extension.</param>
        public WixInvalidExtensionException(string typeName, Type actualType, Type expectedType) :
            this(typeName, actualType, expectedType, null)
        {
        }

        /// <summary>
        /// Instantiate a new WixInvalidExtensionException.
        /// </summary>
        /// <param name="typeName">User-specified name of the type containing an extension.</param>
        /// <param name="actualType">Actual type of the loaded extension.</param>
        /// <param name="expectedType">Expected type of the extension.</param>
        /// <param name="expectedType2">Another expected type of the extension.</param>
        public WixInvalidExtensionException(string typeName, Type actualType, Type expectedType, Type expectedType2) :
            base(null, ExceptionType)
        {
            this.typeName = typeName;
            this.actualType = actualType;
            this.expectedType = expectedType;
            this.expectedType2 = expectedType2;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                if (null != this.actualType && null != this.expectedType)
                {
                    if (null != this.expectedType2)
                    {
                        return String.Format("The specified extension '{0}' is the wrong type: '{1}'.  The expected type was '{2}' or '{3}'.", this.typeName, this.actualType.ToString(), this.expectedType.ToString(), this.expectedType2.ToString());
                    }
                    else
                    {
                        return String.Format("The specified extension '{0}' is the wrong type: '{1}'.  The expected type was '{2}'.", this.typeName, this.actualType.ToString(), this.expectedType.ToString());
                    }
                }
                else
                {
                    return String.Format("Could not find extension '{0}'.", this.typeName);
                }
            }
        }
    }
}
