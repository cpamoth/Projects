//-------------------------------------------------------------------------------------------------
// <copyright file="CabinetFileInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains properties for a file inside a cabinet
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Properties of a file in a cabinet.
    /// </summary>
    internal sealed class CabinetFileInfo
    {
        private string fileId;
        private int fileSize;
        private ushort date;
        private ushort time;

        /// <summary>
        /// Constructs CabinetFileInfo
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <param name="fileSize">Uncompressed file size</param>
        /// <param name="date">Last modified date (MS-DOS time)</param>
        /// <param name="time">Last modified time (MS-DOS time)</param>
        public CabinetFileInfo(string fileId, int fileSize, ushort date, ushort time)
        {
            this.fileId = fileId;
            this.fileSize = fileSize;
            this.date = date;
            this.time = time;
        }

        /// <summary>
        /// Gets the file Id of the file.
        /// </summary>
        /// <value>file Id</value>
        public string FileId
        {
            get { return this.fileId; }
        }

        /// <summary>
        /// Gets the uncompressed size of the file.
        /// </summary>
        /// <value>uncompressed size of the file.</value>
        public int FileSize
        {
            get { return this.fileSize; }
        }

        /// <summary>
        /// Gets modified date (DOS format).
        /// </summary>
        public ushort Date
        {
            get { return this.date; }
        }

        /// <summary>
        /// Gets modified time (DOS format).
        /// </summary>
        public ushort Time
        {
            get { return this.time; }
        }
    }
}
