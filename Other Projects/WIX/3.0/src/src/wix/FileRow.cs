//-------------------------------------------------------------------------------------------------
// <copyright file="FileRow.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Specialization of a row for the file table.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Every file row has an assembly type.
    /// </summary>
    public enum FileAssemblyType
    {
        /// <summary>File is not an assembly.</summary>
        NotAnAssembly = -1,

        /// <summary>File is a Common Language Runtime Assembly.</summary>
        DotNetAssembly = 0,

        /// <summary>File is Win32 SxS assembly.</summary>
        Win32Assembly = 1,
    }

    /// <summary>
    /// Specialization of a row for the file table.
    /// </summary>
    public sealed class FileRow : Row, IComparable
    {
        private string assemblyManifest;
        private FileAssemblyType assemblyType;
        private string directory;
        private int diskId;
        private bool fromModule;
        private bool isGeneratedShortFileName;
        private int patchGroup;
        private string processorArchitecture;
        private string source;
        private Row hashRow;
        private RowCollection assemblyNameRows;

        /// <summary>
        /// Creates a File row that belongs to a table.
        /// </summary>
        /// <param name="sourceLineNumbers">Original source lines for this row.</param>
        /// <param name="table">Table this File row belongs to and should get its column definitions from.</param>
        public FileRow(SourceLineNumberCollection sourceLineNumbers, Table table)
            : base(sourceLineNumbers, table)
        {
            this.assemblyType = FileAssemblyType.NotAnAssembly;
        }

        /// <summary>
        /// Creates a File row that does not belong to a table.
        /// </summary>
        /// <param name="sourceLineNumbers">Original source lines for this row.</param>
        /// <param name="tableDefinition">TableDefinition this Media row belongs to and should get its column definitions from.</param>
        public FileRow(SourceLineNumberCollection sourceLineNumbers, TableDefinition tableDefinition)
            : base(sourceLineNumbers, tableDefinition)
        {
            this.assemblyType = FileAssemblyType.NotAnAssembly;
        }

        /// <summary>
        /// Gets or sets the primary key of the file row.
        /// </summary>
        /// <value>Primary key of the file row.</value>
        public string File
        {
            get { return (string)this.Fields[0].Data; }
            set { this.Fields[0].Data = value; }
        }

        /// <summary>
        /// Gets or sets the component this file row belongs to.
        /// </summary>
        /// <value>Component this file row belongs to.</value>
        public string Component
        {
            get { return (string)this.Fields[1].Data; }
            set { this.Fields[1].Data = value; }
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>Name of the file.</value>
        public string FileName
        {
            get { return (string)this.Fields[2].Data; }
            set { this.Fields[2].Data = value; }
        }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>Size of the file.</value>
        public int FileSize
        {
            get { return (int)this.Fields[3].Data; }
            set { this.Fields[3].Data = value; }
        }

        /// <summary>
        /// Gets or sets the version of the file.
        /// </summary>
        /// <value>Version of the file.</value>
        public string Version
        {
            get { return (string)this.Fields[4].Data; }
            set { this.Fields[4].Data = value; }
        }

        /// <summary>
        /// Gets or sets the LCID of the file.
        /// </summary>
        /// <value>LCID of the file.</value>
        public string Language
        {
            get { return (string)this.Fields[5].Data; }
            set { this.Fields[5].Data = value; }
        }

        /// <summary>
        /// Gets or sets the attributes on a file.
        /// </summary>
        /// <value>Attributes on a file.</value>
        public int Attributes
        {
            get { return Convert.ToInt32(this.Fields[6].Data, CultureInfo.InvariantCulture); }
            set { this.Fields[6].Data = value; }
        }

        /// <summary>
        /// Gets or sets whether this file should be compressed.
        /// </summary>
        /// <value>Whether this file should be compressed.</value>
        public YesNoType Compressed
        {
            get
            {
                bool compressedFlag = (0 < (this.Attributes & MsiInterop.MsidbFileAttributesCompressed));
                bool noncompressedFlag = (0 < (this.Attributes & MsiInterop.MsidbFileAttributesNoncompressed));

                if (compressedFlag && noncompressedFlag)
                {
                    throw new WixException(WixErrors.IllegalFileCompressionAttributes(this.SourceLineNumbers));
                }
                else if (compressedFlag)
                {
                    return YesNoType.Yes;
                }
                else if (noncompressedFlag)
                {
                    return YesNoType.No;
                }
                else
                {
                    return YesNoType.NotSet;
                }
            }

            set
            {
                if (YesNoType.Yes == value)
                {
                    // these are mutually exclusive
                    this.Attributes |= MsiInterop.MsidbFileAttributesCompressed;
                    this.Attributes &= ~MsiInterop.MsidbFileAttributesNoncompressed;
                }
                else if (YesNoType.No == value)
                {
                    // these are mutually exclusive
                    this.Attributes |= MsiInterop.MsidbFileAttributesNoncompressed;
                    this.Attributes &= ~MsiInterop.MsidbFileAttributesCompressed;
                }
                else // not specified
                {
                    Debug.Assert(YesNoType.NotSet == value);

                    // clear any compression bits
                    this.Attributes &= ~MsiInterop.MsidbFileAttributesCompressed;
                    this.Attributes &= ~MsiInterop.MsidbFileAttributesNoncompressed;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sequence of the file row.
        /// </summary>
        /// <value>Sequence of the file row.</value>
        public int Sequence
        {
            get { return (int)this.Fields[7].Data; }
            set { this.Fields[7].Data = value; }
        }

        /// <summary>
        /// Gets or sets the type of assembly of file row.
        /// </summary>
        /// <value>Assembly type for file row.</value>
        public FileAssemblyType AssemblyType
        {
            get { return this.assemblyType; }
            set { this.assemblyType = value; }
        }

        /// <summary>
        /// Gets or sets the identifier for the assembly manifest.
        /// </summary>
        /// <value>Identifier for the assembly manifest.</value>
        public string AssemblyManifest
        {
            get { return this.assemblyManifest; }
            set { this.assemblyManifest = value; }
        }

        /// <summary>
        /// Gets or sets the directory of the file.
        /// </summary>
        /// <value>Directory of the file.</value>
        public string Directory
        {
            get { return this.directory; }
            set { this.directory = value; }
        }

        /// <summary>
        /// Gets or sets the disk id for this file.
        /// </summary>
        /// <value>Disk id for the file.</value>
        public int DiskId
        {
            get { return this.diskId; }
            set { this.diskId = value; }
        }

        /// <summary>
        /// Gets or sets the source location to the file.
        /// </summary>
        /// <value>Source location to the file.</value>
        public string Source
        {
            get { return this.source; }
            set { this.source = value; }
        }

        /// <summary>
        /// Gets or sets the architecture the file executes on.
        /// </summary>
        /// <value>Architecture the file executes on.</value>
        public string ProcessorArchitecture
        {
            get { return this.processorArchitecture; }
            set { this.processorArchitecture = value; }
        }

        /// <summary>
        /// Gets of sets the patch group of a patch-added file.
        /// </summary>
        /// <value>The patch group of a patch-added file.</value>
        public int PatchGroup
        {
            get { return this.patchGroup; }
            set { this.patchGroup = value; }
        }

        /// <summary>
        /// Gets or sets the generated short file name attribute.
        /// </summary>
        /// <value>The generated short file name attribute.</value>
        public bool IsGeneratedShortFileName
        {
            get { return this.isGeneratedShortFileName; }

            set { this.isGeneratedShortFileName = value; }
        }

        /// <summary>
        /// Gets or sets whether this row came from a merge module.
        /// </summary>
        /// <value>Whether this row came from a merge module.</value>
        public bool FromModule
        {
            get { return this.fromModule; }
            set { this.fromModule = value; }
        }

        /// <summary>
        /// Gets or sets the MsiFileHash row created for this FileRow.
        /// </summary>
        /// <value>Row for MsiFileHash table.</value>
        public Row HashRow
        {
            get { return this.hashRow; }
            set { this.hashRow = value; }
        }

        /// <summary>
        /// Gets or sets the set of MsiAssemblyName rows created for this FileRow.
        /// </summary>
        /// <value>RowCollection of MsiAssemblyName table.</value>
        public RowCollection AssemblyNameRows
        {
            get { return this.assemblyNameRows; }
            set { this.assemblyNameRows = value; }
        }

        /// <summary>
        /// Compares the current FileRow with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>An integer that indicates the relative order of the comparands.</returns>
        public int CompareTo(object obj)
        {
            if (this == obj)
            {
                return 0;
            }

            FileRow fileRow = obj as FileRow;
            if (null == fileRow)
            {
                throw new ArgumentException("The other object is not a FileRow.");
            }

            int compared = this.DiskId - fileRow.DiskId;
            if (0 == compared)
            {
                compared = this.patchGroup - fileRow.patchGroup;

                if (0 == compared)
                {
                    compared = String.Compare(this.File, fileRow.File, false, CultureInfo.InvariantCulture);
                }
            }

            return compared;
        }

        /// <summary>
        /// Copies data from another FileRow object.
        /// </summary>
        /// <param name="src">An row to get data from.</param>
        public void CopyFrom(FileRow src)
        {
            for (int i = 0; i < src.Fields.Length; i++)
            {
                this[i] = src[i];
            }
            this.assemblyManifest = src.assemblyManifest;
            this.assemblyType = src.assemblyType;
            this.directory = src.directory;
            this.diskId = src.diskId;
            this.fromModule = src.fromModule;
            this.isGeneratedShortFileName = src.isGeneratedShortFileName;
            this.patchGroup = src.patchGroup;
            this.processorArchitecture = src.processorArchitecture;
            this.source = src.source;
        }
    }
}
