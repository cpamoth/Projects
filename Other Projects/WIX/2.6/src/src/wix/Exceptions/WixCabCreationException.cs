//-------------------------------------------------------------------------------------------------
// <copyright file="WixCabCreationException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX cab creation exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// WiX cab creation exception.
    /// </summary>
    public class WixCabCreationException : WixException
    {
        private const WixExceptionType ExceptionType = WixExceptionType.CabCreation;
        private string fileName;
        private int errorCode;

        /// <summary>
        /// Instantiate a new WixCabCreationException.
        /// </summary>
        /// <param name="errorCode">Error code encountered during cab creation.</param>
        public WixCabCreationException(int errorCode) :
            base(null, ExceptionType)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Instantiate a new WixCabCreationException.
        /// </summary>
        /// <param name="fileName">Name of the file that could not be added to the cabinet file.</param>
        /// <param name="errorCode">Error code encountered during cab creation.</param>
        public WixCabCreationException(string fileName, int errorCode) :
            base(null, ExceptionType)
        {
            this.fileName = fileName;
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Gets the file name that was being cabed when the failure was encountered.
        /// </summary>
        /// <value>The file name that was being cabed when the failure was encountered.</value>
        public string FileName
        {
            get { return this.fileName; }
        }

        /// <summary>
        /// Gets the error code from the cab creation failure.
        /// </summary>
        /// <value>The error code encounterd during cab creation.</value>
        public int ErrorCode
        {
            get { return this.errorCode; }
        }
    }
}

