//-------------------------------------------------------------------------------------------------
// <copyright file="Binder.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Binder core of the Windows Installer Xml toolset.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using Microsoft.Tools.WindowsInstallerXml.Cab;
    using Microsoft.Tools.WindowsInstallerXml.CLR.Interop;
    using Microsoft.Tools.WindowsInstallerXml.MergeMod;
    using Microsoft.Tools.WindowsInstallerXml.Msi;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Binder core of the Windows Installer Xml toolset.
    /// </summary>
    public sealed class Binder : IMessageHandler
    {
        private const int VisitedActionSentinel = -10;
        private static readonly char[] tabCharacter = "\t".ToCharArray();
        private static readonly char[] colonCharacter = ":".ToCharArray();

        private TableDefinitionCollection tableDefinitions;

        private BinderExtension extension;
        private bool encounteredError;
        private Localizer localizer;
        private bool setMsiAssemblyNameFileVersion;
        private bool suppressAclReset;
        private bool suppressAddingValidationRows;
        private bool suppressAssemblies;
        private bool suppressFileHashAndInfo;
        private bool suppressLayout;
        private TempFileCollection tempFiles;
        private string emptyFile;
        private Validator validator;
        private WixVariableResolver wixVariableResolver;
        private int cabbingThreadCount;

        /// <summary>
        /// Creates a binder.
        /// </summary>
        public Binder()
        {
            this.tableDefinitions = Installer.GetTableDefinitions();

            this.extension = new BinderExtension();
            this.cabbingThreadCount = 1;
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets or sets the binder extension class.
        /// </summary>
        /// <value>The binder extension class.</value>
        public BinderExtension Extension
        {
            get { return this.extension; }
            set { this.extension = value; }
        }

        /// <summary>
        /// Gets or sets the localizer.
        /// </summary>
        /// <value>The localizer.</value>
        public Localizer Localizer
        {
            get { return this.localizer; }
            set { this.localizer = value; }
        }

        /// <summary>
        /// Gets and sets the option to set the file version in the MsiAssemblyName table.
        /// </summary>
        /// <value>The option to set the file version in the MsiAssemblyName table.</value>
        public bool SetMsiAssemblyNameFileVersion
        {
            get { return this.setMsiAssemblyNameFileVersion; }
            set { this.setMsiAssemblyNameFileVersion = value; }
        }

        /// <summary>
        /// Gets and sets the option to suppress resetting ACLs by the binder.
        /// </summary>
        /// <value>The option to suppress resetting ACLs by the binder.</value>
        public bool SuppressAclReset
        {
            get { return this.suppressAclReset; }
            set { this.suppressAclReset = value; }
        }

        /// <summary>
        /// Gets and sets the option to suppress adding _Validation rows.
        /// </summary>
        /// <value>The option to suppress adding _Validation rows.</value>
        public bool SuppressAddingValidationRows
        {
            get { return this.suppressAddingValidationRows; }
            set { this.suppressAddingValidationRows = value; }
        }

        /// <summary>
        /// Gets and sets the option to suppress grabbing assembly name information from assemblies.
        /// </summary>
        /// <value>The option to suppress grabbing assembly name information from assemblies.</value>
        public bool SuppressAssemblies
        {
            get { return this.suppressAssemblies; }
            set { this.suppressAssemblies = value; }
        }

        /// <summary>
        /// Gets and sets the option to suppress grabbing the file hash, version and language at link time.
        /// </summary>
        /// <value>The option to suppress grabbing the file hash, version and language.</value>
        public bool SuppressFileHashAndInfo
        {
            get { return this.suppressFileHashAndInfo; }
            set { this.suppressFileHashAndInfo = value; }
        }

        /// <summary>
        /// Gets and sets the option to suppress creating an image for MSI/MSM.
        /// </summary>
        /// <value>The option to suppress creating an image for MSI/MSM.</value>
        public bool SuppressLayout
        {
            get { return this.suppressLayout; }
            set { this.suppressLayout = value; }
        }

        /// <summary>
        /// Gets the table definitions used by the Binder.
        /// </summary>
        /// <value>Table definitions used by the binder.</value>
        public TableDefinitionCollection TableDefinitions
        {
            get { return this.tableDefinitions; }
        }

        /// <summary>
        /// Gets or sets the temporary path for the Binder.  If left null, the binder
        /// will use %TEMP% environment variable.
        /// </summary>
        /// <value>Path to temp files.</value>
        public string TempFilesLocation
        {
            get
            {
                // if we don't have the temporary files object yet, get one
                if (null == this.tempFiles)
                {
                    this.TempFilesLocation = null;
                }

                return this.tempFiles.BasePath;
            }

            set
            {
                if (null == value)
                {
                    this.tempFiles = new TempFileCollection();
                }
                else
                {
                    this.tempFiles = new TempFileCollection(value);
                }

                // ensure the base path exists
                Directory.CreateDirectory(this.tempFiles.BasePath);
            }
        }

        /// <summary>
        /// Gets or sets the MSI validator.
        /// </summary>
        /// <value>The MSI validator.</value>
        public Validator Validator
        {
            get { return this.validator; }
            set { this.validator = value; }
        }

        /// <summary>
        /// Gets or sets the Wix variable resolver.
        /// </summary>
        /// <value>The Wix variable resolver.</value>
        public WixVariableResolver WixVariableResolver
        {
            get { return this.wixVariableResolver; }
            set { this.wixVariableResolver = value; }
        }

        /// <summary>
        /// Gets or sets the number of threads to use for cabinet creation.
        /// </summary>
        /// <value>The number of threads to use for cabinet creation.</value>
        public int CabbingThreadCount
        {
            get { return this.cabbingThreadCount; }
            set { this.cabbingThreadCount = value; }
        }

        /// <summary>
        /// Cleans up the temp files used by the Binder.
        /// </summary>
        /// <returns>True if all files were deleted, false otherwise.</returns>
        public bool DeleteTempFiles()
        {
            if (null == this.tempFiles)
            {
                return true; // no work to do
            }
            else
            {
                bool deleted = Common.DeleteTempFiles(this.TempFilesLocation, this);

                if (deleted)
                {
                    this.tempFiles = null; // temp files have been deleted, no need to remember this now
                    this.emptyFile = null;
                }

                return deleted;
            }
        }

        /// <summary>
        /// Binds an output.
        /// </summary>
        /// <param name="output">The output to bind.</param>
        /// <param name="file">The Windows Installer file to create.</param>
        /// <remarks>The Binder.DeleteTempFiles method should be called after calling this method.</remarks>
        /// <returns>true if binding completed successfully; false otherwise</returns>
        public bool Bind(Output output, string file)
        {
            this.encounteredError = false;

            if (OutputType.Transform == output.Type)
            {
                return this.BindTransform(output, file);
            }
            else
            {
                return this.BindDatabase(output, file);
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != errorEventArgs)
            {
                this.encounteredError = true;
            }

            if (null != this.Message)
            {
                this.Message(this, mea);
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }

        /// <summary>
        /// Creates the MSI/MSM/PCP database.
        /// </summary>
        /// <param name="output">Output to create database for.</param>
        /// <param name="databaseFile">The database file to create.</param>
        internal void GenerateDatabase(Output output, string databaseFile)
        {
            // add the _Validation rows
            if (!this.suppressAddingValidationRows)
            {
                Table validationTable = output.EnsureTable(this.tableDefinitions["_Validation"]);

                foreach (Table table in output.Tables)
                {
                    if (!table.Definition.IsUnreal)
                    {
                        // add the validation rows for this table
                        table.Definition.AddValidationRows(validationTable);
                    }
                }
            }

            try
            {
                OpenDatabase type = OpenDatabase.CreateDirect;

                // set special flag for patch files
                if (OutputType.Patch == output.Type)
                {
                    type |= OpenDatabase.OpenPatchFile;
                }

                // try to create the database
                using (Database db = new Database(databaseFile, type))
                {
                    // localize the codepage if a value was specified by the localizer
                    if (null != this.localizer && -1 != this.localizer.Codepage)
                    {
                        output.Codepage = this.localizer.Codepage;
                    }

                    // if we're not using the default codepage, import a new one into our
                    // database before we add any tables (or the tables would be added
                    // with the wrong codepage)
                    if (0 != output.Codepage)
                    {
                        this.SetDatabaseCodepage(db, output);
                    }

                    foreach (Table table in output.Tables)
                    {
                        Table importStreamTable = null;

                        // skip all unreal tables other than _Streams
                        if (table.Definition.IsUnreal && "_Streams" != table.Name)
                        {
                            continue;
                        }

                        // Do not put the _Validation table in patches, it is not needed
                        if (OutputType.Patch == output.Type && "_Validation" == table.Name)
                        {
                            continue;
                        }

                        // for tables containing streams, move the rows out of the table so that
                        // the table can be created empty, then populated by SQL queries
                        foreach (ColumnDefinition columnDefinition in table.Definition.Columns)
                        {
                            if (ColumnType.Object == columnDefinition.Type)
                            {
                                importStreamTable = new Table(null, table.Definition);
                                importStreamTable.Rows.AddRange(table.Rows);
                                table.Rows.Clear();
                                break;
                            }
                        }

                        // create the table via IDT import
                        if ("_Streams" != table.Name)
                        {
                            try
                            {
                                this.ImportTable(db, output, table);
                            }
                            catch (WixInvalidIdtException)
                            {
                                // If ValidateRows finds anything it doesn't like, it throws
                                table.ValidateRows();

                                // Otherwise we rethrow the InvalidIdt
                                throw;
                            }
                        }

                        // insert the rows via SQL query if this table contains object fields
                        if (null != importStreamTable)
                        {
                            StringBuilder query = new StringBuilder("SELECT ");

                            // build the query for the view
                            bool firstColumn = true;
                            foreach (ColumnDefinition columnDefinition in importStreamTable.Definition.Columns)
                            {
                                if (!firstColumn)
                                {
                                    query.Append(",");
                                }
                                query.AppendFormat(" `{0}`", columnDefinition.Name);
                                firstColumn = false;
                            }
                            query.AppendFormat(" FROM `{0}`", importStreamTable.Name);

                            using (View tableView = db.OpenExecuteView(query.ToString()))
                            {
                                // import each row containing a stream
                                foreach (Row row in importStreamTable.Rows)
                                {
                                    using (Record record = new Record(importStreamTable.Definition.Columns.Count))
                                    {
                                        StringBuilder streamName = new StringBuilder();

                                        // the _Streams table doesn't prepend the table name (or a period)
                                        if ("_Streams" != importStreamTable.Name)
                                        {
                                            streamName.Append(importStreamTable.Name);
                                        }

                                        for (int i = 0; i < importStreamTable.Definition.Columns.Count; i++)
                                        {
                                            ColumnDefinition columnDefinition = importStreamTable.Definition.Columns[i];

                                            switch (columnDefinition.Type)
                                            {
                                                case ColumnType.Localized:
                                                case ColumnType.String:
                                                    if (columnDefinition.IsPrimaryKey)
                                                    {
                                                        if (0 < streamName.Length)
                                                        {
                                                            streamName.Append(".");
                                                        }
                                                        streamName.Append((string)row[i]);
                                                    }

                                                    record.SetString(i + 1, (string)row[i]);
                                                    break;
                                                case ColumnType.Number:
                                                    record.SetInteger(i + 1, Convert.ToInt32(row[i], CultureInfo.InvariantCulture));
                                                    break;
                                                case ColumnType.Object:
                                                    if (null != row[i])
                                                    {
                                                        try
                                                        {
                                                            record.SetStream(i + 1, (string)row[i]);
                                                        }
                                                        catch (Win32Exception e)
                                                        {
                                                            if (0xA1 == e.NativeErrorCode) // ERROR_BAD_PATHNAME
                                                            {
                                                                throw new WixException(WixErrors.FileNotFound(row.SourceLineNumbers, (string)row[i]));
                                                            }
                                                            else
                                                            {
                                                                throw new WixException(WixErrors.Win32Exception(e.NativeErrorCode, e.Message));
                                                            }
                                                        }
                                                    }
                                                    break;
                                            }
                                        }

                                        // stream names are created by concatenating the name of the table with the values
                                        // of the primary key (delimited by periods)
                                        // check for a stream name that is more than 62 characters long (the maximum allowed length)
                                        if (62 < streamName.Length)
                                        {
                                            this.OnMessage(WixErrors.StreamNameTooLong(row.SourceLineNumbers, importStreamTable.Name, streamName.ToString(), streamName.Length));
                                        }
                                        else // add the row to the database
                                        {
                                            tableView.Modify(ModifyView.Assign, record);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // insert substorages (like transforms inside a patch)
                    if (0 < output.SubStorages.Count)
                    {
                        using (View storagesView = new View(db, "SELECT `Name`, `Data` FROM `_Storages`"))
                        {
                            foreach (SubStorage subStorage in output.SubStorages)
                            {
                                string transformFile = Path.Combine(this.TempFilesLocation, String.Concat(subStorage.Name, ".mst"));

                                // bind the transform
                                this.Bind(subStorage.Data, transformFile);

                                // add the storage
                                using (Record record = new Record(2))
                                {
                                    record.SetString(1, subStorage.Name);
                                    record.SetStream(2, transformFile);
                                    storagesView.Modify(ModifyView.Assign, record);
                                }
                            }
                        }
                    }

                    // we're good, commit the changes to the new MSI
                    db.Commit();
                }
            }
            catch (IOException)
            {
                // TODO: this error message doesn't seem specific enough
                throw new WixFileNotFoundException(SourceLineNumberCollection.FromFileName(databaseFile), databaseFile);
            }
        }

        /// <summary>
        /// Generate a new Windows Installer-friendly guid.
        /// </summary>
        /// <returns>A new guid.</returns>
        private static string GenerateGuid()
        {
            return String.Concat("{", Guid.NewGuid().ToString().ToUpper(CultureInfo.InvariantCulture), "}");
        }

        /// <summary>
        /// Get the source name for a file or directory.
        /// </summary>
        /// <param name="names">Source names for the file or directory.</param>
        /// <param name="longNamesInImage">Flag if to resolve to long names.</param>
        /// <returns>Source name for a file or directory.</returns>
        private static string GetSourceName(string names, bool longNamesInImage)
        {
            string source;

            // get source
            int sourceTargetSeparator = names.IndexOf(":");
            if (0 <= sourceTargetSeparator)
            {
                source = names.Substring(sourceTargetSeparator + 1);
            }
            else
            {
                source = names;
            }

            // get the short and long source names
            int sourceSeparator = source.IndexOf("|");
            if (0 <= sourceSeparator)
            {
                if (longNamesInImage)
                {
                    return source.Substring(sourceSeparator + 1);
                }
                else
                {
                    return source.Substring(0, sourceSeparator);
                }
            }
            else
            {
                return source;
            }
        }

        /// <summary>
        /// Get the source path of a directory.
        /// </summary>
        /// <param name="directories">All cached directories.</param>
        /// <param name="directory">Directory identifier.</param>
        /// <param name="canonicalize">Canonicalize the path for standard directories.</param>
        /// <returns>Source path of a directory.</returns>
        private static string GetDirectoryPath(Hashtable directories, string directory, bool canonicalize)
        {
            if (!directories.Contains(directory))
            {
                throw new WixException(WixErrors.ExpectedDirectory(directory));
            }
            ResolvedDirectory resolvedDirectory = (ResolvedDirectory)directories[directory];

            if (null == resolvedDirectory.Path)
            {
                if (canonicalize && Installer.IsStandardDirectory(directory))
                {
                    // when canonicalization is on, standard directories are treated equally
                    resolvedDirectory.Path = directory;
                }
                else
                {
                    string name = resolvedDirectory.Name;

                    if (canonicalize && null != name)
                    {
                        name = name.ToLower(CultureInfo.InvariantCulture);
                    }

                    if (null == resolvedDirectory.DirectoryParent)
                    {
                        resolvedDirectory.Path = name;
                    }
                    else
                    {
                        string parentPath = GetDirectoryPath(directories, resolvedDirectory.DirectoryParent, canonicalize);

                        if (null != resolvedDirectory.Name)
                        {
                            resolvedDirectory.Path = Path.Combine(parentPath, name);
                        }
                        else
                        {
                            resolvedDirectory.Path = parentPath;
                        }
                    }
                }
            }

            return resolvedDirectory.Path;
        }

        /// <summary>
        /// Set an MsiAssemblyName row.  If it was directly authored, override the value, otherwise
        /// create a new row.
        /// </summary>
        /// <param name="assemblyNameTable">MsiAssemblyName table.</param>
        /// <param name="fileRow">FileRow containing the assembly read for the MsiAssemblyName row.</param>
        /// <param name="name">MsiAssemblyName name.</param>
        /// <param name="value">MsiAssemblyName value.</param>
        private static void SetMsiAssemblyName(Table assemblyNameTable, FileRow fileRow, string name, string value)
        {
            // check for null value (this can occur when grabbing the file version from an assembly without one)
            if (null == value || 0 == value.Length)
            {
                throw new WixException(WixErrors.NullMsiAssemblyNameValue(fileRow.SourceLineNumbers, fileRow.Component, name));
            }

            // override directly authored value
            foreach (Row row in assemblyNameTable.Rows)
            {
                if ((string)row[0] == fileRow.Component && (string)row[1] == name)
                {
                    row[2] = value;
                    return;
                }
            }

            Row assemblyNameRow = assemblyNameTable.CreateRow(fileRow.SourceLineNumbers);
            assemblyNameRow[0] = fileRow.Component;
            assemblyNameRow[1] = name;
            assemblyNameRow[2] = value;

            if (null == fileRow.AssemblyNameRows)
            {
                fileRow.AssemblyNameRows = new RowCollection();
            }
            fileRow.AssemblyNameRows.Add(assemblyNameRow);
        }

        /// <summary>
        /// Merge data from the unreal tables into the real tables.
        /// </summary>
        /// <param name="tables">Collection of all tables.</param>
        private static void MergeUnrealTables(TableCollection tables)
        {
            // merge data from the WixBBControl rows into the BBControl rows
            Table wixBBControlTable = tables["WixBBControl"];
            if (null != wixBBControlTable)
            {
                Hashtable indexedBBControlRows = new Hashtable();

                // index all the BBControl rows by their identifier
                Table bbControlTable = tables["BBControl"];
                if (null != bbControlTable)
                {
                    foreach (BBControlRow bbControlRow in bbControlTable.Rows)
                    {
                        indexedBBControlRows.Add(bbControlRow.GetPrimaryKey('/'), bbControlRow);
                    }
                }

                foreach (Row row in wixBBControlTable.Rows)
                {
                    BBControlRow bbControlRow = (BBControlRow)indexedBBControlRows[row.GetPrimaryKey('/')];

                    bbControlRow.SourceFile = (string)row[2];
                }
            }

            // merge data from the WixControl rows into the Control rows
            Table wixControlTable = tables["WixControl"];
            if (null != wixControlTable)
            {
                Hashtable indexedControlRows = new Hashtable();

                // index all the Control rows by their identifier
                Table controlTable = tables["Control"];
                if (null != controlTable)
                {
                    foreach (ControlRow controlRow in controlTable.Rows)
                    {
                        indexedControlRows.Add(controlRow.GetPrimaryKey('/'), controlRow);
                    }
                }

                foreach (Row row in wixControlTable.Rows)
                {
                    ControlRow controlRow = (ControlRow)indexedControlRows[row.GetPrimaryKey('/')];

                    controlRow.SourceFile = (string)row[2];
                }
            }

            // merge data from the WixFile rows into the File rows
            Table wixFileTable = tables["WixFile"];
            if (null != wixFileTable)
            {
                Hashtable indexedFileRows = new Hashtable();

                // index all the File rows by their identifier
                Table fileTable = tables["File"];
                if (null != fileTable)
                {
                    foreach (FileRow fileRow in fileTable.Rows)
                    {
                        indexedFileRows.Add(fileRow.File, fileRow);
                    }
                }

                foreach (Row row in wixFileTable.Rows)
                {
                    FileRow fileRow = (FileRow)indexedFileRows[row[0]];

                    if (null != row[1])
                    {
                        fileRow.AssemblyType = (FileAssemblyType)Enum.ToObject(typeof(FileAssemblyType), (int)row[1]);
                    }
                    else
                    {
                        fileRow.AssemblyType = FileAssemblyType.NotAnAssembly;
                    }
                    fileRow.AssemblyManifest = (string)row[2];
                    fileRow.Directory = (string)row[4];
                    fileRow.DiskId = (int)row[5];
                    fileRow.Source = (string)row[6];
                    fileRow.ProcessorArchitecture = (string)row[7];
                    fileRow.PatchGroup = (int)row[8];
                }
            }

            // copy data from the WixMedia rows into the Media rows
            Table wixMediaTable = tables["WixMedia"];
            if (null != wixMediaTable)
            {
                Hashtable indexedMediaRows = new Hashtable();

                // index all the Media rows by their identifier
                Table mediaTable = tables["Media"];
                if (null != mediaTable)
                {
                    foreach (MediaRow mediaRow in mediaTable.Rows)
                    {
                        indexedMediaRows.Add(mediaRow.DiskId, mediaRow);
                    }
                }

                foreach (Row row in wixMediaTable.Rows)
                {
                    MediaRow mediaRow = (MediaRow)indexedMediaRows[row[0]];

                    // compression level
                    if (null != row[1])
                    {
                        switch ((string)row[1])
                        {
                            case "low":
                                mediaRow.CompressionLevel = Cab.CompressionLevel.Low;
                                break;
                            case "medium":
                                mediaRow.CompressionLevel = Cab.CompressionLevel.Medium;
                                break;
                            case "high":
                                mediaRow.CompressionLevel = Cab.CompressionLevel.High;
                                break;
                            case "none":
                                mediaRow.CompressionLevel = Cab.CompressionLevel.None;
                                break;
                            case "mszip":
                                mediaRow.CompressionLevel = Cab.CompressionLevel.Mszip;
                                break;
                        }
                    }

                    // layout
                    if (null != row[2])
                    {
                        mediaRow.Layout = (string)row[2];
                    }
                }
            }
        }

        /// <summary>
        /// Binds a databse.
        /// </summary>
        /// <param name="output">The output to bind.</param>
        /// <param name="databaseFile">The database file to create.</param>
        /// <returns>true if binding completed successfully; false otherwise</returns>
        private bool BindDatabase(Output output, string databaseFile)
        {
            Hashtable cabinets = new Hashtable();
            bool compressed = false;
            FileRowCollection fileRows = new FileRowCollection(OutputType.Patch == output.Type);
            ArrayList fileTransfers = new ArrayList();
            bool longNames = false;
            MediaRowCollection mediaRows = new MediaRowCollection();
            string modularizationGuid = null;
            Hashtable suppressModularizationIdentifiers = null;
            StringCollection suppressedTableNames = new StringCollection();

            // process the summary information table before the other tables
            Table summaryInformationTable = output.Tables["_SummaryInformation"];
            if (null != summaryInformationTable)
            {
                bool foundCreateDataTime = false;
                bool foundLastSaveDataTime = false;
                bool foundCreatingApplication = false;
                string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

                foreach (Row summaryInformationRow in summaryInformationTable.Rows)
                {
                    switch ((int)summaryInformationRow[0])
                    {
                        case 9: // PID_REVNUMBER
                            string packageCode = (string)summaryInformationRow[1];

                            if (OutputType.Module == output.Type)
                            {
                                modularizationGuid = packageCode.Substring(1, 36).Replace('-', '_');
                            }
                            else if ("*" == packageCode)
                            {
                                // set the revision number (package/patch code) if it should be automatically generated
                                summaryInformationRow[1] = GenerateGuid();
                            }
                            break;
                        case 12: // PID_CREATE_DTM
                            foundCreateDataTime = true;
                            break;
                        case 13: // PID_LASTSAVE_DTM
                            foundLastSaveDataTime = true;
                            break;
                        case 15: // PID_WORDCOUNT
                            if (OutputType.Patch == output.Type)
                            {
                                longNames = true;
                                compressed = true;
                            }
                            else
                            {
                                longNames = (0 == (Convert.ToInt32(summaryInformationRow[1], CultureInfo.InvariantCulture) & 1));
                                compressed = (2 == (Convert.ToInt32(summaryInformationRow[1], CultureInfo.InvariantCulture) & 2));
                            }
                            break;
                        case 18: // PID_APPNAME
                            foundCreatingApplication = true;
                            break;
                    }
                }

                // add a summary information row for the create time/date property if its not already set
                if (!foundCreateDataTime)
                {
                    Row createTimeDateRow = summaryInformationTable.CreateRow(null);
                    createTimeDateRow[0] = 12;
                    createTimeDateRow[1] = now;
                }

                // add a summary information row for the last save time/date property if its not already set
                if (!foundLastSaveDataTime)
                {
                    Row lastSaveTimeDateRow = summaryInformationTable.CreateRow(null);
                    lastSaveTimeDateRow[0] = 13;
                    lastSaveTimeDateRow[1] = now;
                }

                // add a summary information row for the creating application property if its not already set
                if (!foundCreatingApplication)
                {
                    Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    Row creatingApplicationRow = summaryInformationTable.CreateRow(null);
                    creatingApplicationRow[0] = 18;
                    creatingApplicationRow[1] = String.Format(CultureInfo.InvariantCulture, "Windows Installer XML v{0}", currentVersion.ToString());
                }
            }

            // gather all the wix variables
            Table wixVariableTable = output.Tables["WixVariable"];
            if (null != wixVariableTable)
            {
                foreach (WixVariableRow wixVariableRow in wixVariableTable.Rows)
                {
                    this.wixVariableResolver.AddVariable(wixVariableRow);
                }
            }

            // gather all the suppress modularization identifiers
            Table wixSuppressModularizationTable = output.Tables["WixSuppressModularization"];
            if (null != wixSuppressModularizationTable)
            {
                suppressModularizationIdentifiers = new Hashtable();

                foreach (Row row in wixSuppressModularizationTable.Rows)
                {
                    suppressModularizationIdentifiers[row[0]] = null;
                }
            }

            // localize fields, resolve wix variables, and resolve file paths
            foreach (Table table in output.Tables)
            {
                foreach (Row row in table.Rows)
                {
                    foreach (Field field in row.Fields)
                    {
                        bool isDefault = true;

                        // resolve localization and wix variables
                        if (field.Data is string)
                        {
                            field.Data = this.wixVariableResolver.ResolveVariables(this.localizer, row.SourceLineNumbers, (string)field.Data, false, ref isDefault);
                        }

                        // resolve file paths
                        if (!this.wixVariableResolver.EncounteredError && ColumnType.Object == field.Column.Type)
                        {
                            ObjectField objectField = (ObjectField)field;

                            // file is compressed in a cabinet (and not modified above)
                            if (null != objectField.CabinetFileId && isDefault)
                            {
                                // index cabinets that have not been previously encountered
                                if (!cabinets.ContainsKey(objectField.BaseUri))
                                {
                                    Uri baseUri = new Uri(objectField.BaseUri);
                                    string localFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseUri.LocalPath);
                                    string extractedDirectoryName = String.Format(CultureInfo.InvariantCulture, "cab_{0}_{1}", cabinets.Count, localFileNameWithoutExtension);

                                    // index the cabinet file's base URI (source location) and extracted directory
                                    cabinets.Add(objectField.BaseUri, Path.Combine(this.TempFilesLocation, extractedDirectoryName));
                                }

                                // set the path to the file once its extracted from the cabinet
                                objectField.Data = Path.Combine((string)cabinets[objectField.BaseUri], objectField.CabinetFileId);
                            }
                            else if (null != objectField.Data) // non-compressed file (or localized value)
                            {
                                // when SuppressFileHasAndInfo is true do not resolve file paths
                                if (this.SuppressFileHashAndInfo && table.Name == "WixFile")
                                {
                                    continue;
                                }

                                try
                                {
                                    // resolve the path to the file
                                    objectField.Data = this.extension.ResolveFile((string)objectField.Data);
                                }
                                catch (WixFileNotFoundException)
                                {
                                    // display the error with source line information
                                    this.OnMessage(WixErrors.FileNotFound(row.SourceLineNumbers, (string)objectField.Data));
                                }
                            }
                        }
                    }
                }
            }

            // remember if the variable resolver found an error
            if (this.wixVariableResolver.EncounteredError)
            {
                this.encounteredError = true;
            }

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return false;
            }

            // set generated component guids
            // this must occur before modularization and after all variables have been resolved
            this.SetComponentGuids(output);

            // modularize identifiers and add tables with real streams to the import tables
            foreach (Table table in output.Tables)
            {
                // modularize the output if this is a merge module
                if (OutputType.Module == output.Type)
                {
                    table.Modularize(modularizationGuid, suppressModularizationIdentifiers);
                }
            }

            // merge unreal table data into the real tables
            // this must occur after all variables and source paths have been resolved
            MergeUnrealTables(output.Tables);
            if (OutputType.Patch == output.Type)
            {
                foreach (SubStorage substorage in output.SubStorages)
                {
                    Output transform = (Output)substorage.Data;
                    MergeUnrealTables(transform.Tables);
                }
            }

            // index the File table for quicker access later
            // this must occur after the unreal data has been merged in
            Table fileTable = output.Tables["File"];
            if (null != fileTable)
            {
                fileRows.AddRange(fileTable.Rows);
            }

            // index the media table
            Table mediaTable = output.Tables["Media"];
            if (null != mediaTable)
            {
                mediaRows.AddRange(mediaTable.Rows);
            }

            // set the ProductCode if its generated
            Table propertyTable = output.Tables["Property"];
            if (null != propertyTable)
            {
                foreach (Row propertyRow in propertyTable.Rows)
                {
                    if ("ProductCode" == propertyRow[0].ToString() && "*" == propertyRow[1].ToString())
                    {
                        propertyRow[1] = GenerateGuid();
                    }
                }
            }

            // extract files that come from cabinet files (this does not extract files from merge modules)
            foreach (DictionaryEntry cabinet in cabinets)
            {
                Uri baseUri = new Uri((string)cabinet.Key);
                string localPath;

                if ("embeddedresource" == baseUri.Scheme)
                {
                    int bytesRead;
                    byte[] buffer = new byte[512];

                    string originalLocalPath = Path.GetFullPath(baseUri.LocalPath.Substring(1));
                    string resourceName = baseUri.Fragment.Substring(1);
                    Assembly assembly = Assembly.LoadFile(originalLocalPath);

                    localPath = String.Concat(cabinet.Value, ".cab");

                    using (FileStream fs = File.OpenWrite(localPath))
                    {
                        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                        {
                            while (0 < (bytesRead = resourceStream.Read(buffer, 0, buffer.Length)))
                            {
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                else // normal file
                {
                    localPath = baseUri.LocalPath;
                }

                // extract the cabinet's files into a temporary directory
                Directory.CreateDirectory((string)cabinet.Value);

                using (WixExtractCab extractCab = new WixExtractCab())
                {
                    extractCab.Extract(localPath, (string)cabinet.Value);
                }
            }

            // retrieve files and their information from merge modules
            if (OutputType.Product == output.Type)
            {
                this.ProcessMergeModules(output, fileRows);
            }
            else if (OutputType.Patch == output.Type)
            {
                // merge transform data into the output object
                this.CopyTransformData(output, fileRows);
            }

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return false;
            }

            // update file version, hash, assembly, etc.. information
            this.OnMessage(WixVerboses.UpdatingFileInformation());
            this.UpdateFileInformation(output, fileRows, mediaRows);
            this.UpdateControlText(output);

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return false;
            }

            // create cabinet files and process uncompressed files
            string layoutDirectory = Path.GetDirectoryName(databaseFile);
            FileRowCollection uncompressedFileRows = null;
            if (!this.suppressLayout || OutputType.Module == output.Type)
            {
                this.OnMessage(WixVerboses.CreatingCabinetFiles());
                uncompressedFileRows = this.CreateCabinetFiles(output, fileRows, fileTransfers, mediaRows, layoutDirectory, compressed);
            }

            if (OutputType.Patch == output.Type)
            {
                // copy output data back into the transforms
                this.CopyTransformData(output, null);
            }

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return false;
            }

            // add back suppressed tables which must be present prior to merging in modules
            if (OutputType.Product == output.Type)
            {
                Table wixMergeTable = output.Tables["WixMerge"];

                if (null != wixMergeTable && 0 < wixMergeTable.Rows.Count)
                {
                    foreach (SequenceTable sequence in Enum.GetValues(typeof(SequenceTable)))
                    {
                        string sequenceTableName = sequence.ToString();
                        Table sequenceTable = output.Tables[sequenceTableName];

                        if (null == sequenceTable)
                        {
                            sequenceTable = output.EnsureTable(this.tableDefinitions[sequenceTableName]);
                        }

                        if (0 == sequenceTable.Rows.Count)
                        {
                            suppressedTableNames.Add(sequenceTableName);
                        }
                    }
                }
            }

            // generate database file
            this.OnMessage(WixVerboses.GeneratingDatabase());
            string tempDatabaseFile = Path.Combine(this.TempFilesLocation, Path.GetFileName(databaseFile));
            this.GenerateDatabase(output, tempDatabaseFile);
            fileTransfers.Add(new FileTransfer(tempDatabaseFile, databaseFile, true)); // note where this database needs to move in the future

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return false;
            }

            // merge modules
            if (OutputType.Product == output.Type)
            {
                this.OnMessage(WixVerboses.MergingModules());
                this.MergeModules(tempDatabaseFile, output, fileRows, suppressedTableNames);

                // stop processing if an error previously occurred
                if (this.encounteredError)
                {
                    return false;
                }
            }

            // validate the output if there is an MSI validator
            if (null != this.validator)
            {
                // set the output file for source line information
                this.validator.Output = output;

                this.OnMessage(WixVerboses.ValidatingDatabase());
                this.encounteredError = !this.validator.Validate(tempDatabaseFile);

                // stop processing if an error previously occurred
                if (this.encounteredError)
                {
                    return false;
                }
            }

            // process uncompressed files
            if (!this.suppressLayout)
            {
                this.ProcessUncompressedFiles(tempDatabaseFile, uncompressedFileRows, fileTransfers, mediaRows, layoutDirectory, compressed, longNames);
            }

            // layout media
            this.OnMessage(WixVerboses.LayingOutMedia());
            this.LayoutMedia(fileTransfers);

            return !this.encounteredError;
        }

        /// <summary>
        /// Copy file data between transform substorages and the patch output object
        /// </summary>
        /// <param name="output">The output to bind.</param>
        /// <param name="allFileRows">True if copying from transform to patch, false the other way.</param>
        /// <returns>true if binding completed successfully; false otherwise</returns>
        private bool CopyTransformData(Output output, FileRowCollection allFileRows)
        {
            bool copyToPatch = (allFileRows != null);
            bool copyFromPatch = !copyToPatch;
            if (OutputType.Patch != output.Type)
            {
                return true;
            }

            Hashtable patchMediaRows = new Hashtable();
            Hashtable patchMediaFileRows = new Hashtable();
            Table patchFileTable = output.EnsureTable(this.tableDefinitions["File"]);
            if (copyFromPatch)
            {
                // index patch files by diskId+fileId
                foreach (FileRow patchFileRow in patchFileTable.Rows)
                {
                    int diskId = patchFileRow.DiskId;
                    if (!patchMediaFileRows.Contains(diskId))
                    {
                        patchMediaFileRows[diskId] = new FileRowCollection();
                    }
                    FileRowCollection mediaFileRows = (FileRowCollection)patchMediaFileRows[diskId];
                    mediaFileRows.Add(patchFileRow);
                }

                Table patchMediaTable = output.EnsureTable(this.tableDefinitions["Media"]);
                foreach (MediaRow patchMediaRow in patchMediaTable.Rows)
                {
                    patchMediaRows[patchMediaRow.DiskId] = patchMediaRow;
                }
            }

            // index paired transforms
            Hashtable pairedTransforms = new Hashtable();
            foreach (SubStorage substorage in output.SubStorages)
            {
                if ("#" == substorage.Name.Substring(0, 1))
                {
                    pairedTransforms[substorage.Name.Substring(1)] = substorage.Data;
                }
            }

            try
            {
                // copy File bind data into substorages
                foreach (SubStorage substorage in output.SubStorages)
                {
                    if ("#" == substorage.Name.Substring(0, 1))
                    {
                        // no changes necessary for paired transforms
                        continue;
                    }

                    this.extension.ActiveSubStorage = substorage;
                    Hashtable wixFiles = new Hashtable();
                    Output mainTransform = (Output)substorage.Data;
                    Table mainFileTable = mainTransform.Tables["File"];
                    Output pairedTransform = (Output)pairedTransforms[substorage.Name];

                    Table mainWixFileTable = mainTransform.Tables["WixFile"];
                    if (null != mainWixFileTable)
                    {
                        // Index the WixFile table for later use.
                        foreach (WixFileRow row in mainWixFileTable.Rows)
                        {
                            wixFiles.Add(row.Fields[0].Data.ToString(), row);
                        }
                    }

                    // copy Media.LastSequence
                    if (copyFromPatch)
                    {
                        Table pairedMediaTable = pairedTransform.Tables["Media"];
                        foreach (MediaRow pairedMediaRow in pairedMediaTable.Rows)
                        {
                            MediaRow patchMediaRow = (MediaRow)patchMediaRows[pairedMediaRow.DiskId];
                            pairedMediaRow.Fields[1] = patchMediaRow.Fields[1];
                        }
                    }

                    // index File table of pairedTransform
                    FileRowCollection pairedFileRows = new FileRowCollection();
                    Table pairedFileTable = pairedTransform.Tables["File"];
                    if (null != pairedFileTable)
                    {
                        pairedFileRows.AddRange(pairedFileTable.Rows);
                    }

                    if (null != mainFileTable)
                    {
                        foreach (FileRow mainFileRow in mainFileTable.Rows)
                        {
                            if (mainFileRow.Operation == RowOperation.Delete)
                            {
                                continue;
                            }

                            if (!copyToPatch && mainFileRow.Operation == RowOperation.None)
                            {
                                continue;
                            }
                            // When copying to the patch, we need compare the underlying files and include all file changes.
                            else if (copyToPatch)
                            {
                                // If the file is new, we always need to add it to the patch.
                                if (mainFileRow.Operation != RowOperation.Add)
                                {
                                    WixFileRow wixFileRow = (WixFileRow)wixFiles[mainFileRow.Fields[0].Data.ToString()];
                                    ObjectField objectField = (ObjectField)wixFileRow.Fields[6];
                                    FileRow pairedFileRow = pairedFileRows[mainFileRow.Fields[0].Data.ToString()];

                                    // If PreviousData doesn't exist, target and upgrade layout point to the same location. No need to compare.
                                    if (null == objectField.PreviousData)
                                    {
                                        if (mainFileRow.Operation == RowOperation.None)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (!this.extension.CompareFiles(objectField.PreviousData.ToString(), objectField.Data.ToString()))
                                        {
                                            // If the file is different, we need to mark the mainFileRow and pairedFileRow as modified.
                                            mainFileRow.Operation = RowOperation.Modify;
                                            if (null != pairedFileRow)
                                            {
                                                pairedFileRow.Attributes |= MsiInterop.MsidbFileAttributesPatchAdded;
                                                pairedFileRow.Attributes &= ~MsiInterop.MsidbFileAttributesNoncompressed;
                                                pairedFileRow.Attributes |= MsiInterop.MsidbFileAttributesCompressed;
                                                pairedFileRow.Fields[6].Modified = true;
                                                pairedFileRow.Operation = RowOperation.Modify;
                                            }
                                        }
                                        else
                                        {
                                            // The File is same. We need mark all the attributes as unchanged.
                                            mainFileRow.Operation = RowOperation.None;
                                            foreach (Field field in mainFileRow.Fields)
                                            {
                                                field.Modified = false;
                                            }

                                            if (null != pairedFileRow)
                                            {
                                                pairedFileRow.Attributes &= ~MsiInterop.MsidbFileAttributesPatchAdded;
                                                pairedFileRow.Fields[6].Modified = false;
                                                pairedFileRow.Operation = RowOperation.None;
                                            }
                                            continue;
                                        }
                                    }
                                }
                            }

                            // index patch files by diskId+fileId
                            int diskId = mainFileRow.DiskId;
                            if (!patchMediaFileRows.Contains(diskId))
                            {
                                patchMediaFileRows[diskId] = new FileRowCollection();
                            }
                            FileRowCollection mediaFileRows = (FileRowCollection)patchMediaFileRows[diskId];

                            string fileId = mainFileRow.File;
                            FileRow patchFileRow = mediaFileRows[fileId];
                            if (copyToPatch)
                            {
                                if (patchFileRow == null)
                                {
                                    patchFileRow = (FileRow)patchFileTable.CreateRow(null);
                                    patchFileRow.CopyFrom(mainFileRow);
                                    mediaFileRows.Add(patchFileRow);
                                    allFileRows.Add(patchFileRow);
                                }
                                else
                                {
                                    // TODO: confirm the data is identical?
                                }
                            }
                            else
                            {
                                // copy data from the patch back to the transform
                                if (null != patchFileRow)
                                {
                                    FileRow pairedFileRow = (FileRow)pairedFileRows[fileId];
                                    for (int i = 0; i < patchFileRow.Fields.Length; i++)
                                    {
                                        string patchValue = patchFileRow[i] == null ? "" : patchFileRow[i].ToString();
                                        string mainValue = mainFileRow[i] == null ? "" : mainFileRow[i].ToString();
                                        if (7 == i)
                                        {
                                            // File.Sequence is updated in pairedTransform, not mainTransform
                                            pairedFileRow[i] = patchFileRow[i];
                                            pairedFileRow.Fields[i].Modified = true;
                                            mainFileRow.Fields[i].Modified = false;
                                        }
                                        else if (patchValue != mainValue)
                                        {
                                            mainFileRow[i] = patchFileRow[i];
                                            mainFileRow.Fields[i].Modified = true;
                                            if (mainFileRow.Operation == RowOperation.None)
                                            {
                                                mainFileRow.Operation = RowOperation.Modify;
                                            }
                                        }
                                    }

                                    // copy MsiFileHash row for this File
                                    Row patchHashRow = patchFileRow.HashRow;
                                    if (null != patchHashRow)
                                    {
                                        Table mainHashTable = mainTransform.EnsureTable(this.TableDefinitions["MsiFileHash"]);
                                        Row mainHashRow = mainHashTable.CreateRow(mainFileRow.SourceLineNumbers);
                                        for (int i = 0; i < patchHashRow.Fields.Length; i++)
                                        {
                                            mainHashRow[i] = patchHashRow[i];
                                            if (i > 1)
                                            {
                                                // assume all hash fields have been modified
                                                mainHashRow.Fields[i].Modified = true;
                                            }
                                        }

                                        // assume the MsiFileHash operation follows the File one
                                        mainHashRow.Operation = mainFileRow.Operation;
                                    }

                                    // copy MsiAssemblyName rows for this File
                                    RowCollection patchAssemblyNameRows = patchFileRow.AssemblyNameRows;
                                    if (null != patchAssemblyNameRows)
                                    {
                                        Table mainAssemblyNameTable = mainTransform.EnsureTable(this.TableDefinitions["MsiAssemblyName"]);
                                        foreach (Row patchAssemblyNameRow in patchAssemblyNameRows)
                                        {
                                            Row mainAssemblyNameRow = mainAssemblyNameTable.CreateRow(mainFileRow.SourceLineNumbers);
                                            for (int i = 0; i < patchAssemblyNameRow.Fields.Length; i++)
                                            {
                                                mainAssemblyNameRow[i] = patchAssemblyNameRow[i];
                                            }

                                            // assume value field has been modified
                                            mainAssemblyNameRow.Fields[2].Modified = true;
                                            mainAssemblyNameRow.Operation = mainFileRow.Operation;
                                        }
                                    }
                                }
                                else
                                {
                                    // TODO: throw because all transform rows should have made it into the patch
                                }
                            }
                        }
                    }

                    if (copyFromPatch)
                    {
                        output.Tables.Remove("Media");
                        output.Tables.Remove("File");
                        output.Tables.Remove("MsiFileHash");
                        output.Tables.Remove("MsiAssemblyName");
                    }
                }
            }
            finally
            {
                this.extension.ActiveSubStorage = null;
            }

            return true;
        }

        /// <summary>
        /// Binds a transform.
        /// </summary>
        /// <param name="transform">The transform to bind.</param>
        /// <param name="transformFile">The transform to create.</param>
        /// <returns>true if binding completed successfully; false otherwise</returns>
        private bool BindTransform(Output transform, string transformFile)
        {
            int transformFlags = 0;

            Output targetOutput = new Output(null);
            Output updatedOutput = new Output(null);

            // TODO: handle added columns

            // remove certain Property rows which will be populated from summary information values
            string updatedUpgradeCode = null;
            Table propertyTable = transform.Tables["Property"];
            if (null != propertyTable)
            {
                for (int i = propertyTable.Rows.Count - 1; i >= 0; i--)
                {
                    Row row = propertyTable.Rows[i];

                    if ("ProductCode" == (string)row[0] || "ProductVersion" == (string)row[0] || "UpgradeCode" == (string)row[0])
                    {
                        propertyTable.Rows.RemoveAt(i);

                        if ("UpgradeCode" == (string)row[0])
                        {
                            updatedUpgradeCode = (string)row[1];
                        }
                    }
                }
            }

            // process special summary information values
            foreach (Row row in transform.Tables["_SummaryInformation"].Rows)
            {
                if (9 == (int)row[0]) // PID_REVNUMBER
                {
                    string[] propertyData = ((string)row[1]).Split(';');

                    Table targetPropertyTable = targetOutput.EnsureTable(this.tableDefinitions["Property"]);
                    Table updatedPropertyTable = updatedOutput.EnsureTable(this.tableDefinitions["Property"]);

                    Row targetProductCodeRow = targetPropertyTable.CreateRow(null);
                    targetProductCodeRow[0] = "ProductCode";
                    targetProductCodeRow[1] = propertyData[0].Substring(0, 38);

                    Row targetProductVersionRow = targetPropertyTable.CreateRow(null);
                    targetProductVersionRow[0] = "ProductVersion";
                    targetProductVersionRow[1] = propertyData[0].Substring(38);

                    Row updatedProductCodeRow = updatedPropertyTable.CreateRow(null);
                    updatedProductCodeRow[0] = "ProductCode";
                    updatedProductCodeRow[1] = propertyData[1].Substring(0, 38);

                    Row updatedProductVersionRow = updatedPropertyTable.CreateRow(null);
                    updatedProductVersionRow[0] = "ProductVersion";
                    updatedProductVersionRow[1] = propertyData[1].Substring(38);

                    Row targetUpgradeCodeRow = targetPropertyTable.CreateRow(null);
                    targetUpgradeCodeRow[0] = "UpgradeCode";
                    targetUpgradeCodeRow[1] = propertyData[2];

                    Row updatedTargetUpgradeCodeRow = updatedPropertyTable.CreateRow(null);
                    updatedTargetUpgradeCodeRow[0] = "UpgradeCode";
                    if (null != updatedUpgradeCode)
                    {
                        updatedTargetUpgradeCodeRow[1] = updatedUpgradeCode;
                    }
                    else
                    {
                        updatedTargetUpgradeCodeRow[1] = propertyData[2];
                    }
                }
                else if (16 == (int)row[0]) // PID_CHARCOUNT
                {
                    transformFlags = Convert.ToInt32(row[1], CultureInfo.InvariantCulture);
                }
            }

            foreach (Table table in transform.Tables)
            {
                // Ignore unreal tables when building transforms except the _Stream table.
                // These tables are ignored when generating the database so there is no reason
                // to process them here.
                if (table.Definition.IsUnreal && "_Streams" != table.Name)
                {
                    continue;
                }

                // process table operations
                switch (table.Operation)
                {
                    case TableOperation.Add:
                        updatedOutput.EnsureTable(table.Definition);
                        break;
                    case TableOperation.Drop:
                        targetOutput.EnsureTable(table.Definition);
                        continue;
                    default:
                        targetOutput.EnsureTable(table.Definition);
                        updatedOutput.EnsureTable(table.Definition);
                        break;
                }

                // process row operations
                foreach (Row row in table.Rows)
                {
                    switch (row.Operation)
                    {
                        case RowOperation.Add:
                            Table updatedTable = updatedOutput.EnsureTable(table.Definition);
                            updatedTable.Rows.Add(row);
                            continue;
                        case RowOperation.Delete:
                            Table targetTable = targetOutput.EnsureTable(table.Definition);
                            targetTable.Rows.Add(row);

                            // fill-in non-primary key values
                            foreach (Field field in row.Fields)
                            {
                                if (!field.Column.IsPrimaryKey)
                                {
                                    if (ColumnType.Number == field.Column.Type && !field.Column.IsLocalizable)
                                    {
                                        field.Data = field.Column.MinValue;
                                    }
                                    else if (ColumnType.Object == field.Column.Type)
                                    {
                                        if (null == this.emptyFile)
                                        {
                                            this.emptyFile = this.tempFiles.AddExtension("empty");
                                            using (FileStream fileStream = File.Create(this.emptyFile))
                                            {
                                            }
                                        }

                                        field.Data = emptyFile;
                                    }
                                    else
                                    {
                                        field.Data = "0";
                                    }
                                }
                            }
                            continue;
                    }

                    // process modified and unmodified rows
                    bool modifiedRow = false;
                    Row targetRow = new Row(null, table.Definition);
                    Row updatedRow = row;
                    for (int i = 0; i < row.Fields.Length; i++)
                    {
                        Field updatedField = row.Fields[i];

                        if (updatedField.Modified)
                        {
                            // set a different value in the target row to ensure this value will be modified during transform generation
                            if (ColumnType.Number == updatedField.Column.Type && !updatedField.Column.IsLocalizable)
                            {
                                if (null == updatedField.Data || 1 != (int)updatedField.Data)
                                {
                                    targetRow[i] = 1;
                                }
                                else
                                {
                                    targetRow[i] = 2;
                                }
                            }
                            else if (ColumnType.Object == updatedField.Column.Type)
                            {
                                if (null == emptyFile)
                                {
                                    emptyFile = this.tempFiles.AddExtension("empty");
                                    using (FileStream fileStream = File.Create(emptyFile))
                                    {
                                    }
                                }

                                targetRow[i] = emptyFile;
                            }
                            else
                            {
                                if ("0" != (string)updatedField.Data)
                                {
                                    targetRow[i] = "0";
                                }
                                else
                                {
                                    targetRow[i] = "1";
                                }
                            }

                            modifiedRow = true;
                        }
                        else if (ColumnType.Object == updatedField.Column.Type)
                        {
                            ObjectField objectField = (ObjectField)updatedField;

                            // create an empty file for comparing against
                            if (null == objectField.PreviousData)
                            {
                                if (null == emptyFile)
                                {
                                    emptyFile = this.tempFiles.AddExtension("empty");
                                    using (FileStream fileStream = File.Create(emptyFile))
                                    {
                                    }
                                }

                                targetRow[i] = emptyFile;
                                modifiedRow = true;
                            }
                            else if (!this.extension.CompareFiles(objectField.PreviousData, (string)objectField.Data))
                            {
                                targetRow[i] = objectField.PreviousData;
                                modifiedRow = true;
                            }
                        }
                        else // unmodified
                        {
                            if (null != updatedField.Data)
                            {
                                targetRow[i] = updatedField.Data;
                            }
                        }
                    }

                    // modified rows and certain special rows go in the target and updated msi databases
                    if (modifiedRow ||
                        ("_SummaryInformation" == table.Name ||
                        ("Property" == table.Name &&
                        ("ProductCode" == (string)row[0] ||
                        "ProductVersion" == (string)row[0] ||
                        "UpgradeCode" == (string)row[0]))))
                    {
                        Table targetTable = targetOutput.EnsureTable(table.Definition);
                        targetTable.Rows.Add(targetRow);

                        Table updatedTable = updatedOutput.EnsureTable(table.Definition);
                        updatedTable.Rows.Add(updatedRow);
                    }
                }
            }

            string transformFileName = Path.GetFileNameWithoutExtension(transformFile);
            string targetDatabaseFile = Path.Combine(this.TempFilesLocation, String.Concat(transformFileName, "_target.msi"));
            string updatedDatabaseFile = Path.Combine(this.TempFilesLocation, String.Concat(transformFileName, "_updated.msi"));

            this.suppressAddingValidationRows = true;
            this.GenerateDatabase(targetOutput, targetDatabaseFile);
            this.GenerateDatabase(updatedOutput, updatedDatabaseFile);

            // create the transform file
            using (Database targetDatabase = new Database(targetDatabaseFile, OpenDatabase.ReadOnly))
            {
                using (Database updatedDatabase = new Database(updatedDatabaseFile, OpenDatabase.ReadOnly))
                {
                    if (!updatedDatabase.GenerateTransform(targetDatabase, transformFile))
                    {
                        throw new WixException(WixErrors.NoDifferencesInTransform(transform.SourceLineNumbers));
                    }

                    updatedDatabase.CreateTransformSummaryInfo(targetDatabase, transformFile, (TransformErrorConditions)(transformFlags & 0xFFFF), (TransformValidations)((transformFlags >> 16) & 0xFFFF));
                }
            }

            return !this.encounteredError;
        }

        /// <summary>
        /// Retrieve files and their information from merge modules.
        /// </summary>
        /// <param name="output">Internal representation of the msi database to operate upon.</param>
        /// <param name="fileRows">The indexed file rows.</param>
        private void ProcessMergeModules(Output output, FileRowCollection fileRows)
        {
            Table wixMergeTable = output.Tables["WixMerge"];
            if (null != wixMergeTable)
            {
                IMsmMerge2 merge = null;
                MsmMerge msmMerge = new MsmMerge();
                merge = (IMsmMerge2)msmMerge;

                foreach (Row row in wixMergeTable.Rows)
                {
                    bool containsFiles = false;
                    WixMergeRow wixMergeRow = (WixMergeRow)row;

                    try
                    {
                        // read the module's File table to get its FileMediaInformation entries
                        using (Database db = new Database(wixMergeRow.SourceFile, OpenDatabase.ReadOnly))
                        {
                            if (db.TableExists("File") && db.TableExists("Component"))
                            {
                                Hashtable uniqueModuleFileIdentifiers = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

                                using (View view = db.OpenExecuteView("SELECT `File`, `Directory_` FROM `File`, `Component` WHERE `Component_`=`Component`"))
                                {
                                    Record record;

                                    // add each file row from the merge module into the file row collection (check for errors along the way)
                                    while (null != (record = view.Fetch()))
                                    {
                                        // NOTE: this is very tricky - the merge module file rows are not added to the
                                        // file table because they should not be created via idt import.  Instead, these
                                        // rows are created by merging in the actual modules
                                        FileRow fileRow = new FileRow(null, this.tableDefinitions["File"]);
                                        fileRow.File = record[1];
                                        fileRow.Compressed = wixMergeRow.FileCompression;
                                        fileRow.Directory = record[2];
                                        fileRow.DiskId = wixMergeRow.DiskId;
                                        fileRow.FromModule = true;
                                        fileRow.PatchGroup = -1;
                                        fileRow.Source = String.Concat(this.TempFilesLocation, Path.DirectorySeparatorChar, "MergeId.", wixMergeRow.Number.ToString(CultureInfo.InvariantCulture.NumberFormat), Path.DirectorySeparatorChar, record[1]);

                                        FileRow collidingFileRow = fileRows[fileRow.File];
                                        FileRow collidingModuleFileRow = (FileRow)uniqueModuleFileIdentifiers[fileRow.File];

                                        if (null == collidingFileRow && null == collidingModuleFileRow)
                                        {
                                            fileRows.Add(fileRow);

                                            // keep track of file identifiers in this merge module
                                            uniqueModuleFileIdentifiers.Add(fileRow.File, fileRow);
                                        }
                                        else // collision(s) detected
                                        {
                                            // case-sensitive collision with another merge module or a user-authored file identifier
                                            if (null != collidingFileRow)
                                            {
                                                this.OnMessage(WixErrors.DuplicateModuleFileIdentifier(wixMergeRow.SourceLineNumbers, wixMergeRow.Id, collidingFileRow.File));
                                            }

                                            // case-insensitive collision with another file identifier in the same merge module
                                            if (null != collidingModuleFileRow)
                                            {
                                                this.OnMessage(WixErrors.DuplicateModuleCaseInsensitiveFileIdentifier(wixMergeRow.SourceLineNumbers, wixMergeRow.Id, fileRow.File, collidingModuleFileRow.File));
                                            }
                                        }

                                        containsFiles = true;
                                    }
                                }
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        throw new WixException(WixErrors.FileNotFound(wixMergeRow.SourceLineNumbers, wixMergeRow.SourceFile));
                    }
                    catch (Win32Exception)
                    {
                        throw new WixException(WixErrors.CannotOpenMergeModule(wixMergeRow.SourceLineNumbers, wixMergeRow.Id, wixMergeRow.SourceFile));
                    }

                    // if the module has files and creating layout
                    if (containsFiles && !this.suppressLayout)
                    {
                        bool moduleOpen = false;
                        short mergeLanguage;

                        try
                        {
                            mergeLanguage = Convert.ToInt16(wixMergeRow.Language, CultureInfo.InvariantCulture);
                        }
                        catch (System.FormatException)
                        {
                            this.OnMessage(WixErrors.InvalidMergeLanguage(wixMergeRow.SourceLineNumbers, wixMergeRow.Id, wixMergeRow.Language));
                            continue;
                        }

                        try
                        {
                            merge.OpenModule(wixMergeRow.SourceFile, mergeLanguage);
                            moduleOpen = true;

                            string safeMergeId = wixMergeRow.Number.ToString(CultureInfo.InvariantCulture.NumberFormat);

                            // extract the module cabinet, then explode all of the files to a temp directory
                            string moduleCabPath = String.Concat(this.TempFilesLocation, Path.DirectorySeparatorChar, safeMergeId, ".module.cab");
                            merge.ExtractCAB(moduleCabPath);

                            string mergeIdPath = String.Concat(this.TempFilesLocation, Path.DirectorySeparatorChar, "MergeId.", safeMergeId);
                            Directory.CreateDirectory(mergeIdPath);

                            using (WixExtractCab extractCab = new WixExtractCab())
                            {
                                try
                                {
                                    extractCab.Extract(moduleCabPath, mergeIdPath);
                                }
                                catch (FileNotFoundException)
                                {
                                    throw new WixException(WixErrors.CabFileDoesNotExist(moduleCabPath, wixMergeRow.SourceFile, mergeIdPath));
                                }
                                catch
                                {
                                    throw new WixException(WixErrors.CabExtractionFailed(moduleCabPath, wixMergeRow.SourceFile, mergeIdPath));
                                }
                            }
                        }
                        finally
                        {
                            if (moduleOpen)
                            {
                                merge.CloseModule();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the guids for components with generatable guids.
        /// </summary>
        /// <param name="output">Internal representation of the database to operate on.</param>
        private void SetComponentGuids(Output output)
        {
            Table componentTable = output.Tables["Component"];
            Table directoryTable = output.Tables["Directory"];
            Table fileTable = output.Tables["File"];

            if (null != componentTable && null != directoryTable && null != fileTable)
            {
                Hashtable components = new Hashtable();

                // find components with generatable guids
                foreach (Row row in componentTable.Rows)
                {
                    // component guid will be generated
                    if ("*" == (string)row[1])
                    {
                        components.Add((string)row[0], row);
                    }
                }

                // some components have generatable guids
                if (0 < components.Count)
                {
                    Hashtable directories = new Hashtable();

                    // as outlined in RFC 4122, this is our namespace for generating name-based (version 3) UUIDs
                    Guid wixComponentGuidNamespace = new Guid("{3064E5C6-FB63-4FE9-AC49-E446A792EFA5}");

                    // get the target paths for all directories
                    foreach (Row row in directoryTable.Rows)
                    {
                        string targetName = Installer.GetName((string)row[2], false, true);

                        directories.Add(row[0], new ResolvedDirectory((string)row[1], targetName));
                    }

                    // find files belonging to components with generated guids
                    foreach (FileRow fileRow in fileTable.Rows)
                    {
                        if (components.Contains(fileRow.Component))
                        {
                            Row componentRow = (Row)components[fileRow.Component];

                            string directoryPath = GetDirectoryPath(directories, (string)componentRow[2], true);
                            string fileName = Installer.GetName(fileRow.FileName, false, true).ToLower(CultureInfo.InvariantCulture);

                            string path = Path.Combine(directoryPath, fileName);

                            // find paths that are not canonicalized
                            if (path.StartsWith(@"PersonalFolder\my pictures") ||
                                path.StartsWith(@"ProgramFilesFolder\common files") ||
                                path.StartsWith(@"ProgramMenuFolder\startup") ||
                                path.StartsWith("TARGETDIR") ||
                                path.StartsWith(@"StartMenuFolder\programs") ||
                                path.StartsWith(@"WindowsFolder\fonts"))
                            {
                                this.OnMessage(WixErrors.IllegalPathForGeneratedComponentGuid(componentRow.SourceLineNumbers, fileRow.Component, path));
                            }
                            else // generate a guid
                            {
                                componentRow[1] = Uuid.NewUuid(wixComponentGuidNamespace, path).ToString("B").ToUpper(CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update several msi tables with data contained in files references in the File table.
        /// </summary>
        /// <remarks>
        /// For versioned files, update the file version and language in the File table.  For
        /// unversioned files, add a row to the MsiFileHash table for the file.  For assembly
        /// files, add a row to the MsiAssembly table and add AssemblyName information by adding
        /// MsiAssemblyName rows.
        /// </remarks>
        /// <param name="output">Internal representation of the msi database to operate upon.</param>
        /// <param name="fileRows">The indexed file rows.</param>
        /// <param name="mediaRows">The indexed media rows.</param>
        private void UpdateFileInformation(Output output, FileRowCollection fileRows, MediaRowCollection mediaRows)
        {
            Table mediaTable = output.Tables["Media"];

            // calculate sequence numbers and media disk id layout for all file media information objects
            if (OutputType.Module == output.Type)
            {
                int lastSequence = 0;
                foreach (FileRow fileRow in fileRows)
                {
                    fileRow.Sequence = ++lastSequence;
                }
            }
            else if (null != mediaTable)
            {
                int lastSequence = 0;
                MediaRow mediaRow = null;
                SortedList patchGroups = new SortedList();

                // sequence the non-patch-added files
                foreach (FileRow fileRow in fileRows)
                {
                    if (null == mediaRow)
                    {
                        mediaRow = mediaRows[fileRow.DiskId];
                        if (OutputType.Patch == output.Type)
                        {
                            // patch Media cannot start at zero
                            lastSequence = mediaRow.LastSequence;
                        }
                    }
                    else if (mediaRow.DiskId != fileRow.DiskId)
                    {
                        mediaRow.LastSequence = lastSequence;
                        mediaRow = mediaRows[fileRow.DiskId];
                    }

                    if (0 < fileRow.PatchGroup)
                    {
                        ArrayList patchGroup = (ArrayList)patchGroups[fileRow.PatchGroup];

                        if (null == patchGroup)
                        {
                            patchGroup = new ArrayList();
                            patchGroups.Add(fileRow.PatchGroup, patchGroup);
                        }

                        patchGroup.Add(fileRow);
                    }
                    else
                    {
                        fileRow.Sequence = ++lastSequence;
                    }
                }

                if (null != mediaRow)
                {
                    mediaRow.LastSequence = lastSequence;
                    mediaRow = null;
                }

                // sequence the patch-added files
                foreach (ArrayList patchGroup in patchGroups.Values)
                {
                    foreach (FileRow fileRow in patchGroup)
                    {
                        if (null == mediaRow)
                        {
                            mediaRow = mediaRows[fileRow.DiskId];
                        }
                        else if (mediaRow.DiskId != fileRow.DiskId)
                        {
                            mediaRow.LastSequence = lastSequence;
                            mediaRow = mediaRows[fileRow.DiskId];
                        }

                        fileRow.Sequence = ++lastSequence;
                    }
                }

                if (null != mediaRow)
                {
                    mediaRow.LastSequence = lastSequence;
                }
            }

            // no more work to do here if there are no file rows in the file table
            // note that this does not mean there are no files - the files from
            // merge modules are never put in the output's file table
            Table fileTable = output.Tables["File"];
            if (null == fileTable)
            {
                return;
            }

            // gather information about files that did not come from merge modules
            foreach (FileRow fileRow in fileTable.Rows)
            {
                FileInfo fileInfo = null;

                if (!this.suppressFileHashAndInfo || (!this.suppressAssemblies && FileAssemblyType.NotAnAssembly != fileRow.AssemblyType))
                {
                    try
                    {
                        fileInfo = new FileInfo(fileRow.Source);
                    }
                    catch (ArgumentException)
                    {
                        this.OnMessage(WixErrors.InvalidFileName(fileRow.SourceLineNumbers, fileRow.Source));
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        this.OnMessage(WixErrors.InvalidFileName(fileRow.SourceLineNumbers, fileRow.Source));
                        continue;
                    }
                    catch (NotSupportedException)
                    {
                        this.OnMessage(WixErrors.InvalidFileName(fileRow.SourceLineNumbers, fileRow.Source));
                        continue;
                    }
                }

                if (!this.suppressFileHashAndInfo)
                {
                    if (fileInfo.Exists)
                    {
                        string version;
                        string language;

                        fileRow.FileSize = Convert.ToInt32(fileInfo.Length, CultureInfo.InvariantCulture);
                        try
                        {
                            Installer.GetFileVersion(fileInfo.FullName, out version, out language);
                        }
                        catch (Win32Exception e)
                        {
                            if (0x2 == e.NativeErrorCode) // ERROR_FILE_NOT_FOUND
                            {
                                throw new WixException(WixErrors.FileNotFound(fileRow.SourceLineNumbers, fileInfo.FullName));
                            }
                            else
                            {
                                throw new WixException(WixErrors.Win32Exception(e.NativeErrorCode, e.Message));
                            }
                        }

                        if (0 == version.Length && 0 == language.Length)   // unversioned files have their hashes added to the MsiFileHash table
                        {
                            int[] hash;
                            try
                            {
                                Installer.GetFileHash(fileInfo.FullName, 0, out hash);
                            }
                            catch (Win32Exception e)
                            {
                                if (0x2 == e.NativeErrorCode) // ERROR_FILE_NOT_FOUND
                                {
                                    throw new WixException(WixErrors.FileNotFound(fileRow.SourceLineNumbers, fileInfo.FullName));
                                }
                                else
                                {
                                    throw new WixException(WixErrors.Win32Exception(e.NativeErrorCode, e.Message));
                                }
                            }

                            Table msiFileHashTable = output.EnsureTable(this.tableDefinitions["MsiFileHash"]);
                            Row msiFileHashRow = msiFileHashTable.CreateRow(fileRow.SourceLineNumbers);
                            msiFileHashRow[0] = fileRow.File;
                            msiFileHashRow[1] = 0;
                            msiFileHashRow[2] = hash[0];
                            msiFileHashRow[3] = hash[1];
                            msiFileHashRow[4] = hash[2];
                            msiFileHashRow[5] = hash[3];
                            fileRow.HashRow = msiFileHashRow;
                        }
                        else // update the file row with the version and language information
                        {
                            fileRow.Version = version;
                            fileRow.Language = language;
                        }
                    }
                    else
                    {
                        this.OnMessage(WixErrors.CannotFindFile(fileRow.SourceLineNumbers, fileRow.File, fileRow.FileName, fileRow.Source));
                    }
                }

                // if we're not suppressing automagically grabbing assembly information and this is a
                // CLR assembly, load the assembly and get the assembly name information
                if (!this.suppressAssemblies) 
                {
                    if (FileAssemblyType.DotNetAssembly == fileRow.AssemblyType)
                    {
                        StringDictionary assemblyNameValues = new StringDictionary();

                        // under CLR 2.0, use a more robust method of gathering AssemblyName information
                        if (2 <= Environment.Version.Major)
                        {
                            CLRInterop.IReferenceIdentity referenceIdentity = null;
                            Guid referenceIdentityGuid = CLRInterop.ReferenceIdentityGuid;

                            if (0 == CLRInterop.GetAssemblyIdentityFromFile(fileInfo.FullName, ref referenceIdentityGuid, out referenceIdentity))
                            {
                                if (null != referenceIdentity)
                                {
                                    string culture = referenceIdentity.GetAttribute(null, "Culture");
                                    if (null != culture)
                                    {
                                        assemblyNameValues.Add("Culture", culture);
                                    }

                                    string name = referenceIdentity.GetAttribute(null, "Name");
                                    if (null != name)
                                    {
                                        assemblyNameValues.Add("Name", name);
                                    }

                                    string processorArchitecture = referenceIdentity.GetAttribute(null, "ProcessorArchitecture");
                                    if (null != processorArchitecture)
                                    {
                                        assemblyNameValues.Add("ProcessorArchitecture", processorArchitecture);
                                    }

                                    string publicKeyToken = referenceIdentity.GetAttribute(null, "PublicKeyToken");
                                    if (null != publicKeyToken)
                                    {
                                        assemblyNameValues.Add("PublicKeyToken", publicKeyToken.ToUpper(CultureInfo.InvariantCulture));
                                    }

                                    string version = referenceIdentity.GetAttribute(null, "Version");
                                    if (null != version)
                                    {
                                        assemblyNameValues.Add("Version", version);
                                    }
                                }
                            }
                        }
                        else
                        {
                            AssemblyName assemblyName = null;
                            try
                            {
                                assemblyName = AssemblyName.GetAssemblyName(fileInfo.FullName);

                                if (null != assemblyName.CultureInfo)
                                {
                                    assemblyNameValues.Add("Culture", assemblyName.CultureInfo.ToString());
                                }

                                if (null != assemblyName.Name)
                                {
                                    assemblyNameValues.Add("Name", assemblyName.Name);
                                }

                                byte[] publicKey = assemblyName.GetPublicKeyToken();
                                if (null != publicKey && 0 < publicKey.Length)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    for (int i = 0; i < publicKey.GetLength(0); ++i)
                                    {
                                        sb.AppendFormat("{0:X2}", publicKey[i]);
                                    }
                                    assemblyNameValues.Add("PublicKeyToken", sb.ToString());
                                }

                                if (null != assemblyName.Version)
                                {
                                    assemblyNameValues.Add("Version", assemblyName.Version.ToString());
                                }
                            }
                            catch (FileNotFoundException fnfe)
                            {
                                throw new WixFileNotFoundException(fileRow.SourceLineNumbers, fnfe.FileName);
                            }
                            catch (Exception e)
                            {
                                if (e is NullReferenceException || e is SEHException)
                                {
                                    throw;
                                }
                                else
                                {
                                    this.OnMessage(WixErrors.InvalidAssemblyFile(fileRow.SourceLineNumbers, fileInfo.FullName, e.Message));
                                    continue;
                                }
                            }
                        }

                        Table assemblyNameTable = output.EnsureTable(this.tableDefinitions["MsiAssemblyName"]);
                        if (assemblyNameValues.ContainsKey("name"))
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "name", assemblyNameValues["name"]);
                        }

                        string fileVersion = null;
                        if (this.setMsiAssemblyNameFileVersion)
                        {
                            string language;

                            Installer.GetFileVersion(fileInfo.FullName, out fileVersion, out language);
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "fileVersion", fileVersion);
                        }

                        if (assemblyNameValues.ContainsKey("version"))
                        {
                            string assemblyVersion = assemblyNameValues["version"];

                            // there is a bug in fusion that requires the assembly's "version" attribute
                            // to be equal to or longer than the "fileVersion" in length when its present;
                            // the workaround is to prepend zeroes to the last version number in the assembly version
                            if (this.setMsiAssemblyNameFileVersion && null != fileVersion && fileVersion.Length > assemblyVersion.Length)
                            {
                                string padding = new string('0', fileVersion.Length - assemblyVersion.Length);
                                string[] assemblyVersionNumbers = assemblyVersion.Split('.');

                                if (assemblyVersionNumbers.Length > 0)
                                {
                                    assemblyVersionNumbers[assemblyVersionNumbers.Length - 1] = String.Concat(padding, assemblyVersionNumbers[assemblyVersionNumbers.Length - 1]);
                                    assemblyVersion = String.Join(".", assemblyVersionNumbers);
                                }
                            }

                            SetMsiAssemblyName(assemblyNameTable, fileRow, "version", assemblyVersion);
                        }

                        if (assemblyNameValues.ContainsKey("culture"))
                        {
                            string culture = assemblyNameValues["culture"];
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "culture", (0 == culture.Length ? "neutral" : culture));
                        }

                        if (assemblyNameValues.ContainsKey("publicKeyToken"))
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "publicKeyToken", assemblyNameValues["publicKeyToken"]);
                        }

                        if (null != fileRow.ProcessorArchitecture && 0 < fileRow.ProcessorArchitecture.Length)
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "processorArchitecture", fileRow.ProcessorArchitecture);
                        }

                        if (assemblyNameValues.ContainsKey("processorArchitecture"))
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "processorArchitecture", assemblyNameValues["processorArchitecture"]);
                        }
                    }
                    else if (FileAssemblyType.Win32Assembly == fileRow.AssemblyType)
                    {
                        FileRow fileManifestRow = fileRow;

                        // would rather look this up through a data structure rather than
                        // do an order n search through the list of files for every
                        // instance of a win32 assembly.  From what I can find, there
                        // are no indexed data structures available at this point
                        // in the code we're left with this expensive search.
                        // REVIEW(DerekC): Won't this just take the last encountered file row
                        // if the match isn't found, thus making the initialization above to
                        // fileRow useless?
                        foreach (FileRow manifestRow in fileTable.Rows)
                        {
                            fileManifestRow = manifestRow as FileRow;
                            if (fileManifestRow.File == fileRow.AssemblyManifest)
                            {
                                break;
                            }
                        }

                        string type = null;
                        string name = null;
                        string version = null;
                        string processorArchitecture = null;
                        string publicKeyToken = null;

                        // loading the dom is expensive we want more performant APIs than the DOM
                        // Navigator is cheaper than dom.  Perhaps there is a cheaper API still.
                        try
                        {
                            XPathDocument doc = new XPathDocument(fileManifestRow.Source);
                            XPathNavigator nav = doc.CreateNavigator();
                            nav.MoveToRoot();

                            // this assumes a particular schema for a win32 manifest and does not
                            // provide error checking if the file does not conform to schema.
                            // The fallback case here is that nothing is added to the MsiAssemblyName
                            // table for a out of tollerence Win32 manifest.  Perhaps warnings needed.
                            if (nav.MoveToFirstChild())
                            {
                                while (nav.NodeType != XPathNodeType.Element || nav.Name != "assembly")
                                {
                                    nav.MoveToNext();
                                }

                                if (nav.MoveToFirstChild())
                                {
                                    bool hasNextSibling = true;
                                    while (nav.NodeType != XPathNodeType.Element || nav.Name != "assemblyIdentity" && hasNextSibling)
                                    {
                                        hasNextSibling = nav.MoveToNext();
                                    }
                                    if (!hasNextSibling)
                                    {
                                        this.OnMessage(WixErrors.InvalidManifestContent(fileRow.SourceLineNumbers, fileManifestRow.Source));
                                        continue;
                                    }

                                    if (nav.MoveToAttribute("type", String.Empty))
                                    {
                                        type = nav.Value;
                                        nav.MoveToParent();
                                    }

                                    if (nav.MoveToAttribute("name", String.Empty))
                                    {
                                        name = nav.Value;
                                        nav.MoveToParent();
                                    }

                                    if (nav.MoveToAttribute("version", String.Empty))
                                    {
                                        version = nav.Value;
                                        nav.MoveToParent();
                                    }

                                    if (nav.MoveToAttribute("processorArchitecture", String.Empty))
                                    {
                                        processorArchitecture = nav.Value;
                                        nav.MoveToParent();
                                    }

                                    if (nav.MoveToAttribute("publicKeyToken", String.Empty))
                                    {
                                        publicKeyToken = nav.Value;
                                        nav.MoveToParent();
                                    }
                                }
                            }
                        }
                        catch (XmlException xe)
                        {
                            this.OnMessage(WixErrors.InvalidXml(SourceLineNumberCollection.FromFileName(fileManifestRow.Source), "manifest", xe.Message));
                        }

                        Table assemblyNameTable = output.EnsureTable(this.tableDefinitions["MsiAssemblyName"]);
                        if (null != name && 0 < name.Length)
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "name", name);
                        }

                        if (null != version && 0 < version.Length)
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "version", version);
                        }

                        if (null != type && 0 < type.Length)
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "type", type);
                        }

                        if (null != processorArchitecture && 0 < processorArchitecture.Length)
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "processorArchitecture", processorArchitecture);
                        }

                        if (null != publicKeyToken && 0 < publicKeyToken.Length)
                        {
                            SetMsiAssemblyName(assemblyNameTable, fileRow, "publicKeyToken", publicKeyToken);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update Control and BBControl text by reading from files when necessary.
        /// </summary>
        /// <param name="output">Internal representation of the msi database to operate upon.</param>
        private void UpdateControlText(Output output)
        {
            // Control table
            Table controlTable = output.Tables["Control"];
            if (null != controlTable)
            {
                foreach (ControlRow controlRow in controlTable.Rows)
                {
                    if (null != controlRow.SourceFile)
                    {
                        controlRow.Text = this.ReadTextFile(controlRow.SourceLineNumbers, controlRow.SourceFile);
                    }
                }
            }

            // BBControl table
            Table bbcontrolTable = output.Tables["BBControl"];
            if (null != bbcontrolTable)
            {
                foreach (BBControlRow bbcontrolRow in bbcontrolTable.Rows)
                {
                    if (null != bbcontrolRow.SourceFile)
                    {
                        bbcontrolRow.Text = this.ReadTextFile(bbcontrolRow.SourceLineNumbers, bbcontrolRow.SourceFile);
                    }
                }
            }
        }

        /// <summary>
        /// Reads a text file and returns the contents.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers for row from source.</param>
        /// <param name="source">Source path to file to read.</param>
        /// <returns>Text string read from file.</returns>
        private string ReadTextFile(SourceLineNumberCollection sourceLineNumbers, string source)
        {
            string text = null;

            try
            {
                using (StreamReader reader = new StreamReader(source))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (DirectoryNotFoundException e)
            {
                this.OnMessage(WixErrors.BinderExtensionMissingFile(sourceLineNumbers, e.Message));
            }
            catch (FileNotFoundException e)
            {
                this.OnMessage(WixErrors.BinderExtensionMissingFile(sourceLineNumbers, e.Message));
            }
            catch (IOException e)
            {
                this.OnMessage(WixErrors.BinderExtensionMissingFile(sourceLineNumbers, e.Message));
            }
            catch (NotSupportedException)
            {
                this.OnMessage(WixErrors.FileNotFound(sourceLineNumbers, source));
            }

            return text;
        }

        /// <summary>
        /// Merges in any modules to the output database.
        /// </summary>
        /// <param name="tempDatabaseFile">The temporary database file.</param>
        /// <param name="output">Output that specifies database and modules to merge.</param>
        /// <param name="fileRows">The indexed file rows.</param>
        /// <param name="suppressedTableNames">The names of tables that are suppressed.</param>
        /// <remarks>Expects that output's database has already been generated.</remarks>
        private void MergeModules(string tempDatabaseFile, Output output, FileRowCollection fileRows, StringCollection suppressedTableNames)
        {
            Debug.Assert(OutputType.Product == output.Type);

            Table wixMergeTable = output.Tables["WixMerge"];
            Table wixFeatureModulesTable = output.Tables["WixFeatureModules"];

            // check for merge rows to see if there is any work to do
            if (null == wixMergeTable || 0 == wixMergeTable.Rows.Count)
            {
                return;
            }

            IMsmMerge2 merge = null;
            bool commit = true;
            bool logOpen = false;
            bool databaseOpen = false;
            string logPath = null;
            try
            {
                MsmMerge msmMerge = new MsmMerge();
                merge = (IMsmMerge2)msmMerge;

                logPath = Path.Combine(this.TempFilesLocation, "merge.log");
                merge.OpenLog(logPath);
                logOpen = true;

                merge.OpenDatabase(tempDatabaseFile);
                databaseOpen = true;

                // process all the merge rows
                foreach (WixMergeRow wixMergeRow in wixMergeTable.Rows)
                {
                    bool moduleOpen = false;

                    try
                    {
                        short mergeLanguage;

                        try
                        {
                            mergeLanguage = Convert.ToInt16(wixMergeRow.Language, CultureInfo.InvariantCulture);
                        }
                        catch (System.FormatException)
                        {
                            this.OnMessage(WixErrors.InvalidMergeLanguage(wixMergeRow.SourceLineNumbers, wixMergeRow.Id, wixMergeRow.Language));
                            continue;
                        }

                        this.OnMessage(WixVerboses.OpeningMergeModule(wixMergeRow.SourceFile, mergeLanguage));
                        merge.OpenModule(wixMergeRow.SourceFile, mergeLanguage);
                        moduleOpen = true;

                        // if this is a configurable merge module, there should be some configuration data for a callback
                        ConfigurationCallback callback = null;
                        if (null != wixMergeRow.ConfigurationData)
                        {
                            callback = new ConfigurationCallback(wixMergeRow.ConfigurationData);
                        }

                        // merge the module into the database that's being built
                        this.OnMessage(WixVerboses.MergingMergeModule(wixMergeRow.SourceFile));
                        merge.MergeEx(wixMergeRow.Feature, wixMergeRow.Directory, callback);

                        // connect any non-primary features
                        if (null != wixFeatureModulesTable)
                        {
                            foreach (Row row in wixFeatureModulesTable.Rows)
                            {
                                if (wixMergeRow.Id == (string)row[1])
                                {
                                    this.OnMessage(WixVerboses.ConnectingMergeModule(wixMergeRow.SourceFile, (string)row[0]));
                                    merge.Connect((string)row[0]);
                                }
                            }
                        }
                    }
                    catch (COMException)
                    {
                        commit = false;
                    }
                    finally
                    {
                        IMsmErrors mergeErrors = merge.Errors;

                        // display all the errors encountered during the merge operations for this module
                        for (int i = 1; i <= mergeErrors.Count; i++)
                        {
                            IMsmError mergeError = mergeErrors[i];
                            StringBuilder databaseKeys = new StringBuilder();
                            StringBuilder moduleKeys = new StringBuilder();

                            // build a string of the database keys
                            for (int j = 1; j <= mergeError.DatabaseKeys.Count; j++)
                            {
                                if (1 != j)
                                {
                                    databaseKeys.Append(';');
                                }
                                databaseKeys.Append(mergeError.DatabaseKeys[j]);
                            }

                            // build a string of the module keys
                            for (int j = 1; j <= mergeError.ModuleKeys.Count; j++)
                            {
                                if (1 != j)
                                {
                                    moduleKeys.Append(';');
                                }
                                moduleKeys.Append(mergeError.ModuleKeys[j]);
                            }

                            // display the merge error based on the msm error type
                            switch (mergeError.Type)
                            {
                                case MsmErrorType.msmErrorExclusion:
                                    this.OnMessage(WixErrors.MergeExcludedModule(wixMergeRow.SourceLineNumbers, wixMergeRow.Id, moduleKeys.ToString()));
                                    break;
                                case MsmErrorType.msmErrorFeatureRequired:
                                    this.OnMessage(WixErrors.MergeFeatureRequired(wixMergeRow.SourceLineNumbers, mergeError.ModuleTable, moduleKeys.ToString(), wixMergeRow.SourceFile, wixMergeRow.Id));
                                    break;
                                case MsmErrorType.msmErrorLanguageFailed:
                                    this.OnMessage(WixErrors.MergeLanguageFailed(wixMergeRow.SourceLineNumbers, mergeError.Language, wixMergeRow.SourceFile));
                                    break;
                                case MsmErrorType.msmErrorLanguageUnsupported:
                                    this.OnMessage(WixErrors.MergeLanguageUnsupported(wixMergeRow.SourceLineNumbers, mergeError.Language, wixMergeRow.SourceFile));
                                    break;
                                case MsmErrorType.msmErrorResequenceMerge:
                                    this.OnMessage(WixWarnings.MergeRescheduledAction(wixMergeRow.SourceLineNumbers, mergeError.DatabaseTable, databaseKeys.ToString(), wixMergeRow.SourceFile));
                                    break;
                                case MsmErrorType.msmErrorTableMerge:
                                    if ("_Validation" != mergeError.DatabaseTable) // ignore merge errors in the _Validation table
                                    {
                                        this.OnMessage(WixWarnings.MergeTableFailed(wixMergeRow.SourceLineNumbers, mergeError.DatabaseTable, databaseKeys.ToString(), wixMergeRow.SourceFile));
                                    }
                                    break;
                                default:
                                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Encountered an unexpected merge error of type '{0}' for which there is currently no error message to display.  Please notify the WiX team with the problematic authoring and merge module.  More information about the merge and the failure can be found in the merge log: '{1}'", Enum.GetName(typeof(MsmErrorType), mergeError.Type), logPath));
                            }
                        }

                        if (0 >= mergeErrors.Count && !commit)
                        {
                            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Encountered an unexpected error while merging '{0}'. More information about the merge and the failure can be found in the merge log: '{1}'", wixMergeRow.SourceFile, logPath));
                        }

                        if (moduleOpen)
                        {
                            merge.CloseModule();
                        }
                    }
                }
            }
            finally
            {
                if (databaseOpen)
                {
                    merge.CloseDatabase(commit);
                }

                if (logOpen)
                {
                    merge.CloseLog();
                }
            }

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return;
            }

            using (Database db = new Database(tempDatabaseFile, OpenDatabase.Direct))
            {
                Table suppressActionTable = output.Tables["WixSuppressAction"];

                // suppress individual actions
                if (null != suppressActionTable)
                {
                    foreach (Row row in suppressActionTable.Rows)
                    {
                        if (db.TableExists((string)row[0]))
                        {
                            string query = String.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE `Action` = '{1}'", row[0].ToString(), (string)row[1]);

                            using (View view = db.OpenExecuteView(query))
                            {
                                Record record;

                                if (null != (record = view.Fetch()))
                                {
                                    this.OnMessage(WixWarnings.SuppressMergedAction((string)row[1], row[0].ToString()));
                                    view.Modify(ModifyView.Delete, record);
                                    record.Close();
                                }
                            }
                        }
                    }
                }

                // query for merge module actions in suppressed sequences and drop them
                foreach (string tableName in suppressedTableNames)
                {
                    if (!db.TableExists(tableName))
                    {
                        continue;
                    }

                    using (View view = db.OpenExecuteView(String.Concat("SELECT `Action` FROM ", tableName)))
                    {
                        Record resultRecord;
                        while (null != (resultRecord = view.Fetch()))
                        {
                            this.OnMessage(WixWarnings.SuppressMergedAction(resultRecord.GetString(1), tableName));
                            resultRecord.Close();
                        }
                    }

                    // drop suppressed sequences
                    using (View view = db.OpenExecuteView(String.Concat("DROP TABLE ", tableName)))
                    {
                    }

                    // delete the validation rows
                    using (View view = db.OpenView(String.Concat("DELETE FROM _Validation WHERE `Table` = ?")))
                    {
                        using (Record record = new Record(1))
                        {
                            record.SetString(1, tableName);
                            view.Execute(record);
                        }
                    }
                }

                // now update the Attributes column for the files from the Merge Modules
                this.OnMessage(WixVerboses.ResequencingMergeModuleFiles());
                using (View view = db.OpenView("SELECT `Sequence`, `Attributes` FROM `File` WHERE `File`=?"))
                {
                    foreach (FileRow fileRow in fileRows)
                    {
                        if (!fileRow.FromModule)
                        {
                            continue;
                        }

                        using (Record record = new Record(1))
                        {
                            record.SetString(1, fileRow.File);
                            view.Execute(record);
                        }

                        using (Record recordUpdate = view.Fetch())
                        {
                            if (null == recordUpdate)
                            {
                                throw new InvalidOperationException("Failed to fetch a File row from the database that was merged in from a module.");
                            }

                            recordUpdate.SetInteger(1, fileRow.Sequence);

                            // update the file attributes to match the compression specified
                            // on the Merge element or on the Package element
                            int attributes = 0;

                            // get the current value if its not null
                            if (!recordUpdate.IsNull(2))
                            {
                                attributes = recordUpdate.GetInteger(2);
                            }

                            if (YesNoType.Yes == fileRow.Compressed)
                            {
                                // these are mutually exclusive
                                attributes |= MsiInterop.MsidbFileAttributesCompressed;
                                attributes &= ~MsiInterop.MsidbFileAttributesNoncompressed;
                            }
                            else if (YesNoType.No == fileRow.Compressed)
                            {
                                // these are mutually exclusive
                                attributes |= MsiInterop.MsidbFileAttributesNoncompressed;
                                attributes &= ~MsiInterop.MsidbFileAttributesCompressed;
                            }
                            else // not specified
                            {
                                Debug.Assert(YesNoType.NotSet == fileRow.Compressed);

                                // clear any compression bits
                                attributes &= ~MsiInterop.MsidbFileAttributesCompressed;
                                attributes &= ~MsiInterop.MsidbFileAttributesNoncompressed;
                            }
                            recordUpdate.SetInteger(2, attributes);

                            view.Modify(ModifyView.Update, recordUpdate);
                        }
                    }
                }

                db.Commit();
            }
        }

        /// <summary>
        /// Creates cabinet files.
        /// </summary>
        /// <param name="output">Output to generate image for.</param>
        /// <param name="fileRows">The indexed file rows.</param>
        /// <param name="fileTransfers">Array of files to be transfered.</param>
        /// <param name="mediaRows">The indexed media rows.</param>
        /// <param name="layoutDirectory">The directory in which the image should be layed out.</param>
        /// <param name="compressed">Flag if source image should be compressed.</param>
        /// <returns>The uncompressed file rows.</returns>
        private FileRowCollection CreateCabinetFiles(Output output, FileRowCollection fileRows, ArrayList fileTransfers, MediaRowCollection mediaRows, string layoutDirectory, bool compressed)
        {
            Hashtable cabinets = new Hashtable();
            MediaRow mergeModuleMediaRow = null;
            FileRowCollection uncompressedFileRows = new FileRowCollection();

            if (OutputType.Module == output.Type)
            {
                Table mediaTable = new Table(null, this.tableDefinitions["Media"]);
                mergeModuleMediaRow = (MediaRow)mediaTable.CreateRow(null);
                mergeModuleMediaRow.Cabinet = "#MergeModule.CABinet";

                cabinets.Add(mergeModuleMediaRow, new FileRowCollection());
            }
            else
            {
                foreach (MediaRow mediaRow in mediaRows)
                {
                    if (null != mediaRow.Cabinet)
                    {
                        cabinets.Add(mediaRow, new FileRowCollection());
                    }
                }
            }

            foreach (FileRow fileRow in fileRows)
            {
                if (OutputType.Module == output.Type)
                {
                    ((FileRowCollection)cabinets[mergeModuleMediaRow]).Add(fileRow);
                }
                else
                {
                    MediaRow mediaRow = mediaRows[fileRow.DiskId];

                    if (OutputType.Product == output.Type &&
                    (YesNoType.No == fileRow.Compressed ||
                    (YesNoType.NotSet == fileRow.Compressed && !compressed)))
                    {
                        uncompressedFileRows.Add(fileRow);
                    }
                    else // file in a Module or marked compressed
                    {
                        FileRowCollection cabinetFileRow = (FileRowCollection)cabinets[mediaRow];

                        if (null != cabinetFileRow)
                        {
                            cabinetFileRow.Add(fileRow);
                        }
                        else
                        {
                            throw new WixException(WixErrors.ExpectedMediaCabinet(fileRow.SourceLineNumbers, fileRow.File, fileRow.DiskId));
                        }
                    }
                }
            }

            CabinetBuilder cabinetBuilder = new CabinetBuilder(this.cabbingThreadCount);
            if (null != this.Message)
            {
                cabinetBuilder.Message += new MessageEventHandler(this.Message);
            }

            foreach (DictionaryEntry entry in cabinets)
            {
                MediaRow mediaRow = (MediaRow)entry.Key;
                FileRowCollection files = (FileRowCollection)entry.Value;

                string cabinetDir = this.extension.ResolveMedia(mediaRow, layoutDirectory);

                CabinetWorkItem cabinetWorkItem = this.CreateCabinetWorkItem(output, cabinetDir, mediaRow, files, fileTransfers);
                if (null != cabinetWorkItem)
                {
                    cabinetBuilder.Enqueue(cabinetWorkItem);
                }
            }

            // stop processing if an error previously occurred
            if (this.encounteredError)
            {
                return null;
            }

            // create queued cabinets with multiple threads
            cabinetBuilder.CreateQueuedCabinets();

            return uncompressedFileRows;
        }

        /// <summary>
        /// Final step in binding that transfers (moves/copies) all files generated into the appropriate
        /// location in the source image
        /// </summary>
        /// <param name="fileTransfers">Array of files to transfer.</param>
        private void LayoutMedia(ArrayList fileTransfers)
        {
            ArrayList destinationFiles = new ArrayList();

            for (int i = 0; i < fileTransfers.Count; ++i)
            {
                FileTransfer fileTransfer = (FileTransfer)fileTransfers[i];

                bool retry = false;
                do
                {
                    try
                    {
                        if (fileTransfer.Move)
                        {
                            this.OnMessage(WixVerboses.MoveFile(fileTransfer.Source, fileTransfer.Destination));
                            this.extension.MoveFile(fileTransfer.Source, fileTransfer.Destination);
                            retry = false;
                        }
                        else
                        {
                            this.OnMessage(WixVerboses.CopyFile(fileTransfer.Source, fileTransfer.Destination));
                            this.extension.CopyFile(fileTransfer.Source, fileTransfer.Destination, true);
                            retry = false;
                        }

                        destinationFiles.Add(fileTransfer.Destination);
                    }
                    catch (FileNotFoundException e)
                    {
                        throw new WixFileNotFoundException(e.FileName);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // if we already retried, give up
                        if (retry)
                        {
                            throw;
                        }

                        string directory = Path.GetDirectoryName(fileTransfer.Destination);
                        this.OnMessage(WixVerboses.CreateDirectory(directory));
                        Directory.CreateDirectory(directory);
                        retry = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // if we already retried, give up
                        if (retry)
                        {
                            throw;
                        }

                        if (File.Exists(fileTransfer.Destination))
                        {
                            this.OnMessage(WixVerboses.RemoveDestinationFile(fileTransfer.Destination));

                            // try to ensure the file is not read-only
                            FileAttributes attributes = File.GetAttributes(fileTransfer.Destination);
                            try
                            {
                                File.SetAttributes(fileTransfer.Destination, attributes & ~FileAttributes.ReadOnly);
                            }
                            catch (ArgumentException) // thrown for unauthorized access errors
                            {
                                throw new WixException(WixErrors.UnauthorizedAccess(fileTransfer.Destination));
                            }

                            // try to delete the file
                            try
                            {
                                File.Delete(fileTransfer.Destination);
                            }
                            catch (IOException)
                            {
                                throw new WixException(WixErrors.FileInUse(null, fileTransfer.Destination));
                            }
                            retry = true;
                        }
                        else // no idea what just happened, bail
                        {
                            throw;
                        }
                    }
                    catch (IOException)
                    {
                        // if we already retried, give up
                        if (retry)
                        {
                            throw;
                        }

                        if (File.Exists(fileTransfer.Destination))
                        {
                            this.OnMessage(WixVerboses.RemoveDestinationFile(fileTransfer.Destination));

                            // ensure the file is not read-only, then delete it
                            FileAttributes attributes = File.GetAttributes(fileTransfer.Destination);
                            File.SetAttributes(fileTransfer.Destination, attributes & ~FileAttributes.ReadOnly);
                            try
                            {
                                File.Delete(fileTransfer.Destination);
                            }
                            catch (IOException)
                            {
                                throw new WixException(WixErrors.FileInUse(null, fileTransfer.Destination));
                            }
                            retry = true;
                        }
                        else // no idea what just happened, bail
                        {
                            throw;
                        }
                    }
                }
                while (retry);
            }

            // finally, if there were any files remove the ACL that may have been added to
            // during the file transfer process
            if (0 < destinationFiles.Count && !this.suppressAclReset)
            {
                try
                {
                    Microsoft.Tools.WindowsInstallerXml.Cab.Interop.CabInterop.ResetAcls((string[])destinationFiles.ToArray(typeof(string)), (uint)destinationFiles.Count);
                }
                catch
                {
                    this.OnMessage(WixWarnings.UnableToResetAcls());
                }
            }
        }

        /// <summary>
        /// Sets the codepage of a database.
        /// </summary>
        /// <param name="db">Database to set codepage into.</param>
        /// <param name="output">Output with the codepage for the database.</param>
        private void SetDatabaseCodepage(Database db, Output output)
        {
            // write out the _ForceCodepage IDT file
            string idtPath = Path.Combine(this.TempFilesLocation, "codepage.idt");
            using (StreamWriter idtFile = new StreamWriter(idtPath, true, Encoding.ASCII))
            {
                idtFile.WriteLine(); // dummy column name record
                idtFile.WriteLine(); // dummy column definition record
                idtFile.Write(output.Codepage);
                idtFile.WriteLine("\t_ForceCodepage");
            }

            // try to import the table into the MSI
            try
            {
                db.Import(Path.GetDirectoryName(idtPath), Path.GetFileName(idtPath));
            }
            catch (System.Configuration.ConfigurationException)
            {
                throw new WixException(WixErrors.IllegalCodepage(output.Codepage));
            }
        }

        /// <summary>
        /// Imports a table into the database.
        /// </summary>
        /// <param name="db">Database to import table to.</param>
        /// <param name="output">Output for current database.</param>
        /// <param name="table">Table to import into database.</param>
        private void ImportTable(Database db, Output output, Table table)
        {
            // write out the table to an IDT file
            string idtPath = Path.Combine(this.TempFilesLocation, String.Concat(table.Name, ".idt"));
            StreamWriter idtWriter = null;

            try
            {
                Encoding encoding = (0 == output.Codepage ? Encoding.ASCII : Encoding.GetEncoding(output.Codepage));

                // this is a workaround to prevent the UTF-8 byte order marking (BOM)
                // from being added to the beginning of the idt file - according to
                // MSDN, the default encoding for StreamWriter is a special UTF-8
                // encoding that returns an empty byte[] from GetPreamble
                if (Encoding.UTF8 == encoding)
                {
                    idtWriter = new StreamWriter(idtPath, false);
                }
                else
                {
                    idtWriter = new StreamWriter(idtPath, false, encoding);
                }

                idtWriter.Write(table.ToIdtDefinition());
            }
            finally
            {
                if (null != idtWriter)
                {
                    idtWriter.Close();
                }
            }

            // try to import the table into the MSI
            try
            {
                db.Import(Path.GetDirectoryName(idtPath), Path.GetFileName(idtPath));
            }
            catch (Win32Exception)
            {
                table.ValidateRows();

                // if ValidateRows finds anything it doesn't like, it throws
                // otherwise throw WixInvalidIdtException (which is caught in light and turns tidy mode off)
                throw new WixInvalidIdtException(idtPath, table.Name);
            }
        }

        /// <summary>
        /// Creates a work item to create a cabinet.
        /// </summary>
        /// <param name="output">Output for the current database.</param>
        /// <param name="cabinetDir">Directory to create cabinet in.</param>
        /// <param name="mediaRow">MediaRow containing information about the cabinet.</param>
        /// <param name="fileRows">Collection of files in this cabinet.</param>
        /// <param name="fileTransfers">Array of files to be transfered.</param>
        /// <returns>created CabinetWorkItem object</returns>
        private CabinetWorkItem CreateCabinetWorkItem(Output output, string cabinetDir, MediaRow mediaRow, FileRowCollection fileRows, ArrayList fileTransfers)
        {
            CabinetWorkItem cabinetWorkItem = null;
            string tempCabinetFile = Path.Combine(this.TempFilesLocation, mediaRow.Cabinet);

            // check for an empty cabinet
            if (0 == fileRows.Count)
            {
                string cabinetName = mediaRow.Cabinet;

                // remove the leading '#' from the embedded cabinet name to make the warning easier to understand
                if (cabinetName.StartsWith("#"))
                {
                    cabinetName = cabinetName.Substring(1);
                }

                this.OnMessage(WixWarnings.EmptyCabinet(mediaRow.SourceLineNumbers, cabinetName));
            }

            CabinetBuildOption cabinetBuildOption = this.extension.ResolveCabinet(fileRows, ref tempCabinetFile);

            // create a cabinet work item if it's not being skipped
            if (CabinetBuildOption.BuildAndCopy == cabinetBuildOption || CabinetBuildOption.BuildAndMove == cabinetBuildOption)
            {
                cabinetWorkItem = new CabinetWorkItem(fileRows, tempCabinetFile, mediaRow.CompressionLevel);
            }

            if (mediaRow.Cabinet.StartsWith("#"))
            {
                Table streamsTable = output.EnsureTable(this.tableDefinitions["_Streams"]);

                Row streamRow = streamsTable.CreateRow(null);
                streamRow[0] = mediaRow.Cabinet.Substring(1);
                streamRow[1] = tempCabinetFile;
            }
            else
            {
                string destinationPath = Path.Combine(cabinetDir, mediaRow.Cabinet);
                fileTransfers.Add(new FileTransfer(tempCabinetFile, destinationPath, CabinetBuildOption.BuildAndMove == cabinetBuildOption));
            }

            return cabinetWorkItem;
        }

        /// <summary>
        /// Process uncompressed files.
        /// </summary>
        /// <param name="tempDatabaseFile">The temporary database file.</param>
        /// <param name="fileRows">The collection of files to copy into the image.</param>
        /// <param name="fileTransfers">Array of files to be transfered.</param>
        /// <param name="mediaRows">The indexed media rows.</param>
        /// <param name="layoutDirectory">The directory in which the image should be layed out.</param>
        /// <param name="compressed">Flag if source image should be compressed.</param>
        /// <param name="longNamesInImage">Flag if long names should be used.</param>
        private void ProcessUncompressedFiles(string tempDatabaseFile, FileRowCollection fileRows, ArrayList fileTransfers, MediaRowCollection mediaRows, string layoutDirectory, bool compressed, bool longNamesInImage)
        {
            if (0 == fileRows.Count || this.encounteredError)
            {
                return;
            }

            Hashtable directories = new Hashtable();
            using (Database db = new Database(tempDatabaseFile, OpenDatabase.ReadOnly))
            {
                using (View directoryView = db.OpenExecuteView("SELECT `Directory`, `Directory_Parent`, `DefaultDir` FROM `Directory`"))
                {
                    Record directoryRecord;

                    while (null != (directoryRecord = directoryView.Fetch()))
                    {
                        using (directoryRecord)
                        {
                            string sourceName = Installer.GetName(directoryRecord.GetString(3), true, longNamesInImage);

                            directories.Add(directoryRecord.GetString(1), new ResolvedDirectory(directoryRecord.GetString(2), sourceName));
                        }
                    }
                }

                using (View fileView = db.OpenView("SELECT `Directory_`, `FileName` FROM `Component`, `File` WHERE `Component`.`Component`=`File`.`Component_` AND `File`.`File`=?"))
                {
                    using (Record fileQueryRecord = new Record(1))
                    {
                        // for each file in the array of uncompressed files
                        foreach (FileRow fileRow in fileRows)
                        {
                            string relativeFileLayoutPath = null;

                            string mediaLayoutDirectory = this.extension.ResolveMedia(mediaRows[fileRow.DiskId], layoutDirectory);

                            // setup up the query record and find the appropriate file in the
                            // previously executed file view
                            fileQueryRecord[1] = fileRow.File;
                            fileView.Execute(fileQueryRecord);

                            using (Record fileRecord = fileView.Fetch())
                            {
                                if (null == fileRecord)
                                {
                                    throw new WixException(WixErrors.FileIdentifierNotFound(fileRow.SourceLineNumbers, fileRow.File));
                                }

                                string fileName = Installer.GetName(fileRecord[2], true, longNamesInImage);

                                if (compressed)
                                {
                                    // use just the file name of the file since all uncompressed files must appear
                                    // in the root of the image in a compressed package
                                    relativeFileLayoutPath = fileName;
                                }
                                else
                                {
                                    // get the relative path of where we want the file to be layed out as specified
                                    // in the Directory table
                                    string directoryPath = GetDirectoryPath(directories, fileRecord[1], false);
                                    relativeFileLayoutPath = Path.Combine(directoryPath, fileName);
                                }
                            }

                            // strip off "SourceDir" if it's still on there
                            if (relativeFileLayoutPath.StartsWith("SourceDir\\"))
                            {
                                relativeFileLayoutPath = relativeFileLayoutPath.Substring(10);
                            }

                            // finally put together the base media layout path and the relative file layout path
                            string fileLayoutPath = Path.Combine(mediaLayoutDirectory, relativeFileLayoutPath);

                            // if the current source path (where we know that the file already exists) and the resolved
                            // path as dictated by the Directory table are not the same, then propagate the file.  The
                            // image that we create may have already been done by some other process other than the linker, so 
                            // there is no reason to copy the files to the resolved source if they are already there.
                            if (0 != String.Compare(Path.GetFullPath(fileRow.Source), Path.GetFullPath(fileLayoutPath), true, CultureInfo.InvariantCulture))
                            {
                                // just put the file in the transfers array, how anti-climatic
                                fileTransfers.Add(new FileTransfer(fileRow.Source, fileLayoutPath, false));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Structure used for all file transfer information.
        /// </summary>
        private struct FileTransfer
        {
            /// <summary>Source path to file.</summary>
            public string Source;

            /// <summary>Destination path for file.</summary>
            public string Destination;

            /// <summary>Flag if file should be moved (optimal).</summary>
            public bool Move;

            /// <summary>
            /// Basic constructor for struct
            /// </summary>
            /// <param name="source">Source path to file.</param>
            /// <param name="destination">Destination path for file.</param>
            /// <param name="move">File if file should be moved (optimal).</param>
            public FileTransfer(string source, string destination, bool move)
            {
                this.Source = source;
                this.Destination = destination;
                this.Move = move;
            }
        }

        /// <summary>
        /// Structure used for resolved directory information.
        /// </summary>
        private struct ResolvedDirectory
        {
            /// <summary>The directory parent.</summary>
            public string DirectoryParent;

            /// <summary>The name of this directory.</summary>
            public string Name;

            /// <summary>The path of this directory.</summary>
            public string Path;

            /// <summary>
            /// Constructor for ResolvedDirectory.
            /// </summary>
            /// <param name="directoryParent">Parent directory.</param>
            /// <param name="name">The directory name.</param>
            public ResolvedDirectory(string directoryParent, string name)
            {
                this.DirectoryParent = directoryParent;
                this.Name = name;
                this.Path = null;
            }
        }

        /// <summary>
        /// Callback object for configurable merge modules.
        /// </summary>
        private sealed class ConfigurationCallback : IMsmConfigureModule
        {
            private const int SOk = 0x0;
            private const int SFalse = 0x1;
            private Hashtable configurationData;

            /// <summary>
            /// Creates a ConfigurationCallback object.
            /// </summary>
            /// <param name="configData">String to break up into name/value pairs.</param>
            public ConfigurationCallback(string configData)
            {
                if (null == configData)
                {
                    throw new ArgumentNullException("configData");
                }

                this.configurationData = new Hashtable();
                string[] pairs = configData.Split(',');
                for (int i = 0; i < pairs.Length; ++i)
                {
                    string[] nameVal = pairs[i].Split('=');
                    string name = nameVal[0];
                    string value = nameVal[1];

                    name = name.Replace("%2C", ",");
                    name = name.Replace("%3D", "=");
                    name = name.Replace("%25", "%");

                    value = value.Replace("%2C", ",");
                    value = value.Replace("%3D", "=");
                    value = value.Replace("%25", "%");

                    this.configurationData[name] = value;
                }
            }

            /// <summary>
            /// Returns text data based on name.
            /// </summary>
            /// <param name="name">Name of value to return.</param>
            /// <param name="configData">Out param to put configuration data into.</param>
            /// <returns>S_OK if value provided, S_FALSE if not.</returns>
            public int ProvideTextData(string name, out string configData)
            {
                if (this.configurationData.Contains(name))
                {
                    configData = (string)this.configurationData[name];
                    return SOk;
                }
                else
                {
                    configData = null;
                    return SFalse;
                }
            }

            /// <summary>
            /// Returns integer data based on name.
            /// </summary>
            /// <param name="name">Name of value to return.</param>
            /// <param name="configData">Out param to put configuration data into.</param>
            /// <returns>S_OK if value provided, S_FALSE if not.</returns>
            public int ProvideIntegerData(string name, out int configData)
            {
                if (this.configurationData.Contains(name))
                {
                    string val = (string)this.configurationData[name];
                    configData = Convert.ToInt32(val, CultureInfo.InvariantCulture);
                    return SOk;
                }
                else
                {
                    configData = 0;
                    return SFalse;
                }
            }
        }
    }
}
