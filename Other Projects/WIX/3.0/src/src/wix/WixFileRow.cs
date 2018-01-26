//-------------------------------------------------------------------------------------------------
// <copyright file="WixFileRow.cs" company="Microsoft">
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
    using System.Globalization;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Specialization of a row for the WixFile table.
    /// </summary>
    public sealed class WixFileRow : Row
    {
        /// <summary>
        /// Creates a WixFile row that does not belong to a table.
        /// </summary>
        /// <param name="sourceLineNumbers">Original source lines for this row.</param>
        /// <param name="tableDef">TableDefinition this Media row belongs to and should get its column definitions from.</param>
        public WixFileRow(SourceLineNumberCollection sourceLineNumbers, TableDefinition tableDef) :
            base(sourceLineNumbers, tableDef)
        {
        }

        /// <summary>
        /// Creates a WixFile row that belongs to a table.
        /// </summary>
        /// <param name="sourceLineNumbers">Original source lines for this row.</param>
        /// <param name="table">Table this File row belongs to and should get its column definitions from.</param>
        public WixFileRow(SourceLineNumberCollection sourceLineNumbers, Table table) :
            base(sourceLineNumbers, table)
        {
        }

        /// <summary>
        /// Gets or sets the application for the assembly.
        /// </summary>
        /// <value>Application for the assembly.</value>
        public string AssemblyApplication
        {
            get { return (string)this.Fields[3].Data; }
            set { this.Fields[3].Data = value; }
        }

        /// <summary>
        /// Gets or sets the assembly attributes of the file row.
        /// </summary>
        /// <value>Assembly attributes of the file row.</value>
        public int AssemblyAttributes
        {
            get { return Convert.ToInt32(this.Fields[1].Data, CultureInfo.InvariantCulture); }
            set { this.Fields[1].Data = value; }
        }

        /// <summary>
        /// Gets or sets the identifier for the assembly manifest.
        /// </summary>
        /// <value>Identifier for the assembly manifest.</value>
        public string AssemblyManifest
        {
            get { return (string)this.Fields[2].Data; }
            set { this.Fields[2].Data = value; }
        }

        /// <summary>
        /// Gets or sets the attributes on a file.
        /// </summary>
        /// <value>Attributes on a file.</value>
        public int Attributes
        {
            get { return Convert.ToInt32(this.Fields[9].Data, CultureInfo.InvariantCulture); }
            set { this.Fields[9].Data = value; }
        }

        /// <summary>
        /// Gets or sets the directory of the file.
        /// </summary>
        /// <value>Directory of the file.</value>
        public string Directory
        {
            get { return (string)this.Fields[4].Data; }
            set { this.Fields[4].Data = value; }
        }

        /// <summary>
        /// Gets or sets the disk id for this file.
        /// </summary>
        /// <value>Disk id for the file.</value>
        public int DiskId
        {
            get { return Convert.ToInt32(this.Fields[5].Data, CultureInfo.InvariantCulture); }
            set { this.Fields[5].Data = value; }
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
        /// Gets of sets the patch group of a patch-added file.
        /// </summary>
        /// <value>The patch group of a patch-added file.</value>
        public int PatchGroup
        {
            get { return Convert.ToInt32(this.Fields[8].Data, CultureInfo.InvariantCulture); }
            set { this.Fields[8].Data = value; }
        }

        /// <summary>
        /// Gets or sets the architecture the file executes on.
        /// </summary>
        /// <value>Architecture the file executes on.</value>
        public string ProcessorArchitecture
        {
            get { return (string)this.Fields[7].Data; }
            set { this.Fields[7].Data = value; }
        }

        /// <summary>
        /// Gets or sets the source location to the file.
        /// </summary>
        /// <value>Source location to the file.</value>
        public string Source
        {
            get { return (string)this.Fields[6].Data; }
            set { this.Fields[6].Data = value; }
        }
    }
}
