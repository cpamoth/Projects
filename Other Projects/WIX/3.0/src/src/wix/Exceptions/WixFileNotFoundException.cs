//-------------------------------------------------------------------------------------------------
// <copyright file="WixFileNotFoundException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WixException thrown when a file cannot be found.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WixException thrown when a file cannot be found.
    /// </summary>
    [Serializable]
    public sealed class WixFileNotFoundException : WixException
    {
        /// <summary>
        /// Instantiate a new WixFileNotFoundException.
        /// </summary>
        /// <param name="file">The file that could not be found.</param>
        public WixFileNotFoundException(string file) : this(null, file, null)
        {
        }

        /// <summary>
        /// Instantiate a new WixFileNotFoundException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information pertaining to the file that cannot be found.</param>
        /// <param name="file">The file that could not be found.</param>
        public WixFileNotFoundException(SourceLineNumberCollection sourceLineNumbers, string file) : this(sourceLineNumbers, file, null)
        {
        }

        /// <summary>
        /// Instantiate a new WixFileNotFoundException.
        /// </summary>
        /// <param name="file">The file that could not be found.</param>
        /// <param name="fileType">The type of file that cannot be found.</param>
        public WixFileNotFoundException(string file, string fileType) : this(null, file, fileType)
        {
        }

        /// <summary>
        /// Instantiate a new WixFileNotFoundException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information pertaining to the file that cannot be found.</param>
        /// <param name="file">The file that could not be found.</param>
        /// <param name="fileType">The type of file that cannot be found.</param>
        public WixFileNotFoundException(SourceLineNumberCollection sourceLineNumbers, string file, string fileType) :
            base(WixErrors.FileNotFound(sourceLineNumbers, file, fileType))
        {
        }
    }
}
