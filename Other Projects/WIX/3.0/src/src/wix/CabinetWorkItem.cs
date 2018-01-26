//-------------------------------------------------------------------------------------------------
// <copyright file="CabinetWorkItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A cabinet builder work item.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// A cabinet builder work item.
    /// </summary>
    internal sealed class CabinetWorkItem
    {
        private string cabinetFile;
        private Cab.CompressionLevel compressionLevel;
        private FileRowCollection fileRows;

        /// <summary>
        /// Instantiate a new CabinetWorkItem.
        /// </summary>
        /// <param name="fileRows">The collection of files in this cabinet.</param>
        /// <param name="cabinetFile">The cabinet file.</param>
        /// <param name="compressionLevel">The compression level of the cabinet.</param>
        public CabinetWorkItem(FileRowCollection fileRows, string cabinetFile, Cab.CompressionLevel compressionLevel)
        {
            this.cabinetFile = cabinetFile;
            this.compressionLevel = compressionLevel;
            this.fileRows = fileRows;
        }

        /// <summary>
        /// Gets the cabinet file.
        /// </summary>
        /// <value>The cabinet file.</value>
        public string CabinetFile
        {
            get { return this.cabinetFile; }
        }

        /// <summary>
        /// Gets the compression level of the cabinet.
        /// </summary>
        /// <value>The compression level of the cabinet.</value>
        public Cab.CompressionLevel CompressionLevel
        {
            get { return this.compressionLevel; }
        }

        /// <summary>
        /// Gets the collection of files in this cabinet.
        /// </summary>
        /// <value>The collection of files in this cabinet.</value>
        public FileRowCollection FileRows
        {
            get { return this.fileRows; }
        }
    }
}
