//-------------------------------------------------------------------------------------------------
// <copyright file="Unbinder.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Unbinder core of the Windows Installer Xml toolset.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    using Microsoft.Tools.WindowsInstallerXml.Cab;
    using Microsoft.Tools.WindowsInstallerXml.Msi;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;
    using Microsoft.Tools.WindowsInstallerXml.Ole32;

    /// <summary>
    /// Unbinder core of the Windows Installer Xml toolset.
    /// </summary>
    public sealed class Unbinder : IMessageHandler
    {
        private string emptyFile;
        private bool suppressDemodularization;
        private bool suppressExtractCabinets;
        private TableDefinitionCollection tableDefinitions;
        private TempFileCollection tempFiles;

        /// <summary>
        /// Creates a new unbinder object with a default set of table definitions.
        /// </summary>
        public Unbinder()
        {
            this.tableDefinitions = Installer.GetTableDefinitions();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets or sets the option to suppress demodularizing values.
        /// </summary>
        /// <value>The option to suppress demodularizing values.</value>
        public bool SuppressDemodularization
        {
            get { return this.suppressDemodularization; }
            set { this.suppressDemodularization = value; }
        }

        /// <summary>
        /// Gets or sets the option to suppress extracting cabinets.
        /// </summary>
        /// <value>The option to suppress extracting cabinets.</value>
        public bool SuppressExtractCabinets
        {
            get { return this.suppressExtractCabinets; }
            set { this.suppressExtractCabinets = value; }
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
                return null == this.tempFiles ? String.Empty : this.tempFiles.BasePath;
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
            }
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void AddExtension(WixExtension extension)
        {
            if (null != extension.TableDefinitions)
            {
                foreach (TableDefinition tableDefinition in extension.TableDefinitions)
                {
                    if (!this.tableDefinitions.Contains(tableDefinition.Name))
                    {
                        this.tableDefinitions.Add(tableDefinition);
                    }
                    else
                    {
                        throw new WixException(WixErrors.DuplicateExtensionTable(extension.GetType().ToString(), tableDefinition.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Unbind a Windows Installer file.
        /// </summary>
        /// <param name="file">The Windows Installer file.</param>
        /// <param name="outputType">The type of output to create.</param>
        /// <param name="exportBasePath">The path where files should be exported.</param>
        /// <returns>The output representing the database.</returns>
        public Output Unbind(string file, OutputType outputType, string exportBasePath)
        {
            // if we don't have the temporary files object yet, get one
            if (null == this.tempFiles)
            {
                this.TempFilesLocation = null;
            }
            Directory.CreateDirectory(this.tempFiles.BasePath); // ensure the base path is there

            if (OutputType.Patch == outputType)
            {
                return this.UnbindPatch(file, exportBasePath);
            }
            else if (OutputType.Transform == outputType)
            {
                return this.UnbindTransform(file, exportBasePath);
            }
            else // other database types
            {
                return this.UnbindDatabase(file, outputType, exportBasePath);
            }
        }

        /// <summary>
        /// Cleans up the temp files used by the Decompiler.
        /// </summary>
        /// <returns>True if all files were deleted, false otherwise.</returns>
        /// <remarks>
        /// This should be called after every call to Decompile to ensure there
        /// are no conflicts between each decompiled database.
        /// </remarks>
        public bool DeleteTempFiles()
        {
            if (null == this.tempFiles)
            {
                return true; // no work to do
            }
            else
            {
                bool deleted = Common.DeleteTempFiles(this.tempFiles.BasePath, this);

                if (deleted)
                {
                    this.tempFiles = null; // temp files have been deleted, no need to remember this now
                }

                return deleted;
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

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
        /// Unbind an MSI database file.
        /// </summary>
        /// <param name="databaseFile">The database file.</param>
        /// <param name="outputType">The output type.</param>
        /// <param name="exportBasePath">The path where files should be exported.</param>
        /// <returns>The unbound database.</returns>
        private Output UnbindDatabase(string databaseFile, OutputType outputType, string exportBasePath)
        {
            Output output;

            try
            {
                using (Database database = new Database(databaseFile, OpenDatabase.ReadOnly))
                {
                    output = this.UnbindDatabase(databaseFile, database, outputType, exportBasePath);

                    // extract the files from the cabinets
                    if (null != exportBasePath && !this.suppressExtractCabinets)
                    {
                        this.ExtractCabinets(output, database, databaseFile, exportBasePath);
                    }
                }
            }
            catch (Win32Exception e)
            {
                if (0x6E == e.NativeErrorCode) // ERROR_OPEN_FAILED
                {
                    throw new WixException(WixErrors.OpenDatabaseFailed(databaseFile));
                }

                throw;
            }

            return output;
        }

        /// <summary>
        /// Unbind an MSI database file.
        /// </summary>
        /// <param name="databaseFile">The database file.</param>
        /// <param name="database">The opened database.</param>
        /// <param name="outputType">The type of output to create.</param>
        /// <param name="exportBasePath">The path where files should be exported.</param>
        /// <returns>The output representing the database.</returns>
        private Output UnbindDatabase(string databaseFile, Database database, OutputType outputType, string exportBasePath)
        {
            string modularizationGuid = null;
            Output output = new Output(SourceLineNumberCollection.FromFileName(databaseFile));
            View validationView = null;

            // set the output type
            output.Type = outputType;

            // get the codepage
            database.Export("_ForceCodepage", this.TempFilesLocation, "_ForceCodepage.idt");
            using (StreamReader sr = File.OpenText(Path.Combine(this.TempFilesLocation, "_ForceCodepage.idt")))
            {
                string line;

                while (null != (line = sr.ReadLine()))
                {
                    string[] data = line.Split('\t');

                    if (2 == data.Length)
                    {
                        output.Codepage = Convert.ToInt32(data[0], CultureInfo.InvariantCulture);
                    }
                }
            }

            // get the summary information table
            using (SummaryInformation summaryInformation = new SummaryInformation(database))
            {
                Table table = new Table(null, this.tableDefinitions["_SummaryInformation"]);

                for (int i = 1; 19 >= i; i++)
                {
                    string value = summaryInformation.GetProperty(i);

                    if (0 < value.Length)
                    {
                        Row row = table.CreateRow(output.SourceLineNumbers);
                        row[0] = i;
                        row[1] = value;
                    }
                }

                output.Tables.Add(table);
            }

            try
            {
                // open a view on the validation table if it exists
                if (database.TableExists("_Validation"))
                {
                    validationView = database.OpenView("SELECT * FROM `_Validation` WHERE `Table` = ? AND `Column` = ?");
                }

                // get the normal tables
                using (View tablesView = database.OpenExecuteView("SELECT * FROM _Tables"))
                {
                    Record tableRecord;

                    while (null != (tableRecord = tablesView.Fetch()))
                    {
                        string tableName = tableRecord.GetString(1);

                        using (View tableView = database.OpenExecuteView(String.Format(CultureInfo.InvariantCulture, "SELECT * FROM `{0}`", tableName)))
                        {
                            Record rowRecord;
                            Table table;
                            TableDefinition tableDefinition;

                            if (this.tableDefinitions.Contains(tableName))
                            {
                                tableDefinition = this.tableDefinitions[tableName];

                                // TODO: verify table definitions
                                // - error for different column name or data type
                                // - warn for additional columns
                                // - need extra info for new columns in standard tables (MSI 4.0 changes)
                            }
                            else // custom table
                            {
                                TableDefinition customTableDefinition = new TableDefinition(tableName, false, false);
                                Hashtable customTablePrimaryKeys = new Hashtable();

                                Record customColumnNameRecord = tableView.GetColumnInfo(MsiInterop.MSICOLINFONAMES);
                                Record customColumnTypeRecord = tableView.GetColumnInfo(MsiInterop.MSICOLINFOTYPES);
                                int customColumnCount = customColumnNameRecord.GetFieldCount();

                                // index the primary keys
                                using (Record primaryKeysRecord = database.PrimaryKeys(tableName))
                                {
                                    int primaryKeysFieldCount = primaryKeysRecord.GetFieldCount();

                                    for (int i = 1; i <= primaryKeysFieldCount; i++)
                                    {
                                        customTablePrimaryKeys[primaryKeysRecord.GetString(i)] = null;
                                    }
                                }

                                for (int i = 1; i <= customColumnCount; i++)
                                {
                                    string columnName = customColumnNameRecord.GetString(i);
                                    string idtType = customColumnTypeRecord.GetString(i);

                                    ColumnType columnType;
                                    int length;
                                    bool nullable;

                                    ColumnCategory columnCategory = ColumnCategory.Unknown;
                                    ColumnModularizeType columnModularizeType = ColumnModularizeType.None;
                                    bool primary = customTablePrimaryKeys.Contains(columnName);
                                    bool minValueSet = false;
                                    int minValue = -1;
                                    bool maxValueSet = false;
                                    int maxValue = -1;
                                    string keyTable = null;
                                    bool keyColumnSet = false;
                                    int keyColumn = -1;
                                    string category = null;
                                    string set = null;
                                    string description = null;

                                    // get the column type, length, and whether its nullable
                                    switch (Char.ToLower(idtType[0], CultureInfo.InvariantCulture))
                                    {
                                        case 'i':
                                            columnType = ColumnType.Number;
                                            break;
                                        case 'l':
                                            columnType = ColumnType.Localized;
                                            break;
                                        case 's':
                                            columnType = ColumnType.String;
                                            break;
                                        case 'v':
                                            columnType = ColumnType.Object;
                                            break;
                                        default:
                                            // TODO: error
                                            columnType = ColumnType.Unknown;
                                            break;
                                    }
                                    length = Convert.ToInt32(idtType.Substring(1), CultureInfo.InvariantCulture);
                                    nullable = Char.IsUpper(idtType[0]);

                                    // try to get validation information
                                    if (null != validationView)
                                    {
                                        Record validationRecord = new Record(2);
                                        validationRecord.SetString(1, tableName);
                                        validationRecord.SetString(2, columnName);

                                        validationView.Execute(validationRecord);
                                        validationRecord.Close();

                                        if (null != (validationRecord = validationView.Fetch()))
                                        {
                                            string validationNullable = validationRecord.GetString(3);
                                            minValueSet = !validationRecord.IsNull(4);
                                            minValue = (minValueSet ? validationRecord.GetInteger(4) : -1);
                                            maxValueSet = !validationRecord.IsNull(5);
                                            maxValue = (maxValueSet ? validationRecord.GetInteger(5) : -1);
                                            keyTable = (!validationRecord.IsNull(6) ? validationRecord.GetString(6) : null);
                                            keyColumnSet = !validationRecord.IsNull(7);
                                            keyColumn = (keyColumnSet ? validationRecord.GetInteger(7) : -1);
                                            category = (!validationRecord.IsNull(8) ? validationRecord.GetString(8) : null);
                                            set = (!validationRecord.IsNull(9) ? validationRecord.GetString(9) : null);
                                            description = (!validationRecord.IsNull(10) ? validationRecord.GetString(10) : null);

                                            // check the validation nullable value against the column definition
                                            if (null == validationNullable)
                                            {
                                                // TODO: warn for illegal validation nullable column
                                            }
                                            else if ((nullable && "Y" != validationNullable) || (!nullable && "N" != validationNullable))
                                            {
                                                // TODO: warn for mismatch between column definition and validation nullable
                                            }

                                            // convert category to ColumnCategory
                                            if (null != category)
                                            {
                                                try
                                                {
                                                    columnCategory = (ColumnCategory)Enum.Parse(typeof(ColumnCategory), category, true);
                                                }
                                                catch (ArgumentException)
                                                {
                                                    columnCategory = ColumnCategory.Unknown;
                                                }
                                            }

                                            validationRecord.Close();
                                        }
                                        else
                                        {
                                            // TODO: warn about no validation information
                                        }
                                    }

                                    // guess the modularization type
                                    if ("Icon" == keyTable && 1 == keyColumn)
                                    {
                                        columnModularizeType = ColumnModularizeType.Icon;
                                    }
                                    else if ("Condition" == columnName)
                                    {
                                        columnModularizeType = ColumnModularizeType.Condition;
                                    }
                                    else if (ColumnCategory.Formatted == columnCategory)
                                    {
                                        columnModularizeType = ColumnModularizeType.Property;
                                    }
                                    else if (ColumnCategory.Identifier == columnCategory)
                                    {
                                        columnModularizeType = ColumnModularizeType.Column;
                                    }

                                    customTableDefinition.Columns.Add(new ColumnDefinition(columnName, columnType, length, primary, nullable, columnModularizeType, (ColumnType.Localized == columnType), minValueSet, minValue, maxValueSet, maxValue, keyTable, keyColumnSet, keyColumn, columnCategory, set, description, true, true));
                                }

                                tableDefinition = customTableDefinition;

                                customColumnNameRecord.Close();
                                customColumnTypeRecord.Close();
                            }

                            table = new Table(null, tableDefinition);

                            while (null != (rowRecord = tableView.Fetch()))
                            {
                                int recordCount = rowRecord.GetFieldCount();
                                Row row = table.CreateRow(output.SourceLineNumbers);

                                for (int i = 0; recordCount > i && row.Fields.Length > i; i++)
                                {
                                    if (rowRecord.IsNull(i + 1))
                                    {
                                        if (!row.Fields[i].Column.IsNullable)
                                        {
                                            // TODO: display an error for a null value in a non-nullable field OR
                                            // display a warning and put an empty string in the value to let the compiler handle it
                                            // (the second option is risky because the later code may make certain assumptions about
                                            // the contents of a row value)
                                        }
                                    }
                                    else
                                    {
                                        switch (row.Fields[i].Column.Type)
                                        {
                                            case ColumnType.Number:
                                                if (row.Fields[i].Column.IsLocalizable)
                                                {
                                                    row[i] = Convert.ToString(rowRecord.GetInteger(i + 1), CultureInfo.InvariantCulture);
                                                }
                                                else
                                                {
                                                    row[i] = rowRecord.GetInteger(i + 1);
                                                }
                                                break;
                                            case ColumnType.Object:
                                                string sourceFile = "FILE NOT EXPORTED, USE THE dark.exe -x OPTION TO EXPORT BINARIES";

                                                if (null != exportBasePath)
                                                {
                                                    string relativeSourceFile = Path.Combine(tableName, row.GetPrimaryKey('.'));
                                                    sourceFile = Path.Combine(exportBasePath, relativeSourceFile);

                                                    // ensure the parent directory exists
                                                    System.IO.Directory.CreateDirectory(Path.Combine(exportBasePath, tableName));

                                                    using (FileStream fs = System.IO.File.Create(sourceFile))
                                                    {
                                                        int bytesRead;
                                                        byte[] buffer = new byte[512];

                                                        while (0 != (bytesRead = rowRecord.GetStream(i + 1, buffer, buffer.Length)))
                                                        {
                                                            fs.Write(buffer, 0, bytesRead);
                                                        }
                                                    }
                                                }

                                                row[i] = sourceFile;
                                                break;
                                            default:
                                                string value = rowRecord.GetString(i + 1);

                                                switch (row.Fields[i].Column.Category)
                                                {
                                                    case ColumnCategory.Guid:
                                                        value = value.ToUpper(CultureInfo.InvariantCulture);
                                                        break;
                                                }

                                                // de-modularize
                                                if (!this.suppressDemodularization && OutputType.Module == output.Type && ColumnModularizeType.None != row.Fields[i].Column.ModularizeType)
                                                {
                                                    Regex modularization = new Regex(@"\.[0-9A-Fa-f]{8}_[0-9A-Fa-f]{4}_[0-9A-Fa-f]{4}_[0-9A-Fa-f]{4}_[0-9A-Fa-f]{12}");

                                                    if (null == modularizationGuid)
                                                    {
                                                        Match match = modularization.Match(value);
                                                        if (match.Success)
                                                        {
                                                            modularizationGuid = String.Concat('{', match.Value.Substring(1).Replace('_', '-'), '}');
                                                        }
                                                    }

                                                    value = modularization.Replace(value, String.Empty);
                                                }

                                                // escape "$(" for the preprocessor
                                                value = value.Replace("$(", "$$(");

                                                // escape things that look like wix variables
                                                MatchCollection matches = Common.WixVariableRegex.Matches(value);
                                                for (int j = matches.Count - 1; 0 <= j; j--)
                                                {
                                                    value = value.Insert(matches[j].Index, "!");
                                                }

                                                row[i] = value;
                                                break;
                                        }
                                    }
                                }

                                rowRecord.Close();
                            }

                            output.Tables.Add(table);
                        }

                        tableRecord.Close();
                    }
                }
            }
            finally
            {
                if (null != validationView)
                {
                    validationView.Close();
                }
            }

            // set the modularization guid as the PackageCode
            if (null != modularizationGuid)
            {
                Table table = output.Tables["_SummaryInformation"];

                foreach (Row row in table.Rows)
                {
                    if (9 == (int)row[0]) // PID_REVNUMBER
                    {
                        row[1] = modularizationGuid;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Unbind an MSP patch file.
        /// </summary>
        /// <param name="patchFile">The patch file.</param>
        /// <param name="exportBasePath">The path where files should be exported.</param>
        /// <returns>The unbound patch.</returns>
        private Output UnbindPatch(string patchFile, string exportBasePath)
        {
            Output patch;

            // patch files are essentially database files (use a special flag to let the API know its a patch file)
            try
            {
                using (Database database = new Database(patchFile, OpenDatabase.ReadOnly | OpenDatabase.OpenPatchFile))
                {
                    patch = this.UnbindDatabase(patchFile, database, OutputType.Patch, exportBasePath);
                }
            }
            catch (Win32Exception e)
            {
                if (0x6E == e.NativeErrorCode) // ERROR_OPEN_FAILED
                {
                    throw new WixException(WixErrors.OpenDatabaseFailed(patchFile));
                }

                throw;
            }

            // retrieve the transforms (they are in substorages)
            using (Storage storage = Storage.Open(patchFile, StorageMode.Read | StorageMode.ShareDenyWrite))
            {
                Table summaryInformationTable = patch.Tables["_SummaryInformation"];
                foreach (Row row in summaryInformationTable.Rows)
                {
                    if (8 == (int)row[0]) // PID_LASTAUTHOR
                    {
                        string value = (string)row[1];

                        foreach (string decoratedSubStorageName in value.Split(';'))
                        {
                            string subStorageName = decoratedSubStorageName.Substring(1);
                            string transformFile = Path.Combine(this.TempFilesLocation, String.Concat("Transform", Path.DirectorySeparatorChar, subStorageName, ".mst"));

                            // ensure the parent directory exists
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(transformFile));

                            // copy the substorage to a new storage for the transform file
                            using (Storage subStorage = storage.OpenStorage(subStorageName))
                            {
                                using (Storage transformStorage = Storage.CreateDocFile(transformFile, StorageMode.ReadWrite | StorageMode.ShareExclusive | StorageMode.Create))
                                {
                                    subStorage.CopyTo(transformStorage);
                                }
                            }

                            // unbind the transform
                            Output transform = this.UnbindTransform(transformFile, (null == exportBasePath ? null : Path.Combine(exportBasePath, subStorageName)));
                            patch.SubStorages.Add(new SubStorage(subStorageName, transform));
                        }

                        break;
                    }
                }
            }

            // extract the files from the cabinets
            // TODO: use per-transform export paths for support of multi-product patches
            if (null != exportBasePath && !this.suppressExtractCabinets)
            {
                using (Database database = new Database(patchFile, OpenDatabase.ReadOnly | OpenDatabase.OpenPatchFile))
                {
                    foreach (SubStorage subStorage in patch.SubStorages)
                    {
                        // only patch transforms should carry files
                        if (subStorage.Name.StartsWith("#"))
                        {
                            this.ExtractCabinets(subStorage.Data, database, patchFile, exportBasePath);
                        }
                    }
                }
            }

            return patch;
        }

        /// <summary>
        /// Unbind an MSI transform file.
        /// </summary>
        /// <param name="transformFile">The transform file.</param>
        /// <param name="exportBasePath">The path where files should be exported.</param>
        /// <returns>The unbound transform.</returns>
        private Output UnbindTransform(string transformFile, string exportBasePath)
        {
            Output transform = new Output(SourceLineNumberCollection.FromFileName(transformFile));
            transform.Type = OutputType.Transform;

            // get the summary information table
            using (SummaryInformation summaryInformation = new SummaryInformation(transformFile))
            {
                Table table = transform.Tables.EnsureTable(null, this.tableDefinitions["_SummaryInformation"]);

                for (int i = 1; 19 >= i; i++)
                {
                    string value = summaryInformation.GetProperty(i);

                    if (0 < value.Length)
                    {
                        Row row = table.CreateRow(transform.SourceLineNumbers);
                        row[0] = i;
                        row[1] = value;
                    }
                }
            }

            // create a schema msi which hopefully matches the table schemas in the transform
            Output schemaOutput = new Output(null);
            string msiDatabaseFile = Path.Combine(this.tempFiles.BasePath, "schema.msi");
            foreach (TableDefinition tableDefinition in this.tableDefinitions)
            {
                // skip unreal tables and the Patch table
                if (!tableDefinition.IsUnreal && "Patch" != tableDefinition.Name)
                {
                    schemaOutput.EnsureTable(tableDefinition);
                }
            }

            // bind the schema msi
            Binder binder = new Binder();
            binder.SuppressAddingValidationRows = true;
            binder.WixVariableResolver = new WixVariableResolver();
            binder.GenerateDatabase(schemaOutput, msiDatabaseFile);

            // apply the transform to the database and retrieve the modifications
            Hashtable addedRows = new Hashtable();
            Table transformViewTable;
            using (Database msiDatabase = new Database(msiDatabaseFile, OpenDatabase.Transact))
            {
                // apply the transform with the ViewTransform option to collect all the modifications
                msiDatabase.ApplyTransform(transformFile, TransformErrorConditions.All | TransformErrorConditions.ViewTransform);

                // unbind the database
                Output transformViewOutput = this.UnbindDatabase(msiDatabaseFile, msiDatabase, OutputType.Product, exportBasePath);

                // index the added and possibly modified rows (added rows may also appears as modified rows)
                transformViewTable = transformViewOutput.Tables["_TransformView"];
                Hashtable modifiedRows = new Hashtable();
                foreach (Row row in transformViewTable.Rows)
                {
                    string tableName = (string)row[0];
                    string columnName = (string)row[1];
                    string primaryKeys = (string)row[2];

                    if ("INSERT" == columnName)
                    {
                        string index = String.Concat(tableName, ':', primaryKeys);

                        addedRows.Add(index, null);
                    }
                    else if ("CREATE" != columnName && "DELETE" != columnName && "DROP" != columnName && null != primaryKeys) // modified row
                    {
                        string index = String.Concat(tableName, ':', primaryKeys);

                        modifiedRows[index] = row;
                    }
                }

                // create placeholder rows for modified rows to make the transform insert the updated values when its applied
                foreach (Row row in modifiedRows.Values)
                {
                    string tableName = (string)row[0];
                    string columnName = (string)row[1];
                    string primaryKeys = (string)row[2];

                    string index = String.Concat(tableName, ':', primaryKeys);

                    // ignore information for added rows
                    if (!addedRows.Contains(index))
                    {
                        Table table = schemaOutput.Tables[tableName];
                        this.CreateRow(table, primaryKeys, true);
                    }
                }
            }

            // re-bind the schema output with the placeholder rows
            binder.GenerateDatabase(schemaOutput, msiDatabaseFile);

            // apply the transform to the database and retrieve the modifications
            using (Database msiDatabase = new Database(msiDatabaseFile, OpenDatabase.Transact))
            {
                // apply the transform
                msiDatabase.ApplyTransform(transformFile, TransformErrorConditions.All);

                // commit the database to guard against weird errors with streams
                msiDatabase.Commit();

                // unbind the database
                Output output = this.UnbindDatabase(msiDatabaseFile, msiDatabase, OutputType.Product, exportBasePath);

                // index all the rows to easily find modified rows
                Hashtable rows = new Hashtable();
                foreach (Table table in output.Tables)
                {
                    foreach (Row row in table.Rows)
                    {
                        rows.Add(String.Concat(table.Name, ':', row.GetPrimaryKey('\t')), row);
                    }
                }

                // process the _TransformView rows into transform rows
                foreach (Row row in transformViewTable.Rows)
                {
                    string tableName = (string)row[0];
                    string columnName = (string)row[1];
                    string primaryKeys = (string)row[2];

                    Table table = transform.Tables.EnsureTable(null, this.tableDefinitions[tableName]);

                    if ("CREATE" == columnName) // added table
                    {
                        table.Operation = TableOperation.Add;
                    }
                    else if ("DELETE" == columnName) // deleted row
                    {
                        Row deletedRow = this.CreateRow(table, primaryKeys, false);
                        deletedRow.Operation = RowOperation.Delete;
                    }
                    else if ("DROP" == columnName) // dropped table
                    {
                        table.Operation = TableOperation.Drop;
                    }
                    else if ("INSERT" == columnName) // added row
                    {
                        string index = String.Concat(tableName, ':', primaryKeys);
                        Row addedRow = (Row)rows[index];
                        addedRow.Operation = RowOperation.Add;
                        table.Rows.Add(addedRow);
                    }
                    else if (null != primaryKeys) // modified row
                    {
                        string index = String.Concat(tableName, ':', primaryKeys);

                        // the _TransformView table includes information for added rows
                        // that looks like modified rows so it sometimes needs to be ignored
                        if (!addedRows.Contains(index))
                        {
                            Row modifiedRow = (Row)rows[index];

                            // mark the field as modified
                            int indexOfModifiedValue = modifiedRow.TableDefinition.Columns.IndexOf(columnName);
                            modifiedRow.Fields[indexOfModifiedValue].Modified = true;

                            // move the modified row into the transform the first time its encountered
                            if (RowOperation.None == modifiedRow.Operation)
                            {
                                modifiedRow.Operation = RowOperation.Modify;
                                table.Rows.Add(modifiedRow);
                            }
                        }
                    }
                    else // added column
                    {
                        table.Definition.Columns[columnName].Added = true;
                    }
                }
            }

            return transform;
        }

        /// <summary>
        /// Create a deleted or modified row.
        /// </summary>
        /// <param name="table">The table containing the row.</param>
        /// <param name="primaryKeys">The primary keys of the row.</param>
        /// <param name="setRequiredFields">Option to set all required fields with placeholder values.</param>
        /// <returns>The new row.</returns>
        private Row CreateRow(Table table, string primaryKeys, bool setRequiredFields)
        {
            Row row = table.CreateRow(null);

            string[] primaryKeyParts = primaryKeys.Split('\t');
            int primaryKeyPartIndex = 0;

            for (int i = 0; i < table.Definition.Columns.Count; i++)
            {
                ColumnDefinition columnDefinition = table.Definition.Columns[i];

                if (columnDefinition.IsPrimaryKey)
                {
                    if (ColumnType.Number == columnDefinition.Type && !columnDefinition.IsLocalizable)
                    {
                        row[i] = Convert.ToInt32(primaryKeyParts[primaryKeyPartIndex++], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row[i] = primaryKeyParts[primaryKeyPartIndex++];
                    }
                }
                else if (setRequiredFields)
                {
                    if (ColumnType.Number == columnDefinition.Type && !columnDefinition.IsLocalizable)
                    {
                        row[i] = 1;
                    }
                    else if (ColumnType.Object == columnDefinition.Type)
                    {
                        if (null == this.emptyFile)
                        {
                            this.emptyFile = this.tempFiles.AddExtension("empty");
                            using (FileStream fileStream = File.Create(this.emptyFile))
                            {
                            }
                        }

                        row[i] = this.emptyFile;
                    }
                    else
                    {
                        row[i] = "1";
                    }
                }
            }

            return row;
        }

        /// <summary>
        /// Extract the cabinets from a database.
        /// </summary>
        /// <param name="output">The output to use when finding cabinets.</param>
        /// <param name="database">The database containing the cabinets.</param>
        /// <param name="databaseFile">The location of the database file.</param>
        /// <param name="exportBasePath">The path where the files should be exported.</param>
        private void ExtractCabinets(Output output, Database database, string databaseFile, string exportBasePath)
        {
            string databaseBasePath = Path.GetDirectoryName(databaseFile);
            StringCollection cabinetFiles = new StringCollection();
            Hashtable embeddedCabinets = new Hashtable();

            // index all of the cabinet files
            if (OutputType.Module == output.Type)
            {
                embeddedCabinets.Add(0, "MergeModule.CABinet");
            }
            else if (null != output.Tables["Media"])
            {
                foreach (MediaRow mediaRow in output.Tables["Media"].Rows)
                {
                    if (null != mediaRow.Cabinet)
                    {
                        if (OutputType.Product == output.Type ||
                            (OutputType.Transform == output.Type && RowOperation.Add == mediaRow.Operation))
                        {
                            if (mediaRow.Cabinet.StartsWith("#"))
                            {
                                embeddedCabinets.Add(mediaRow.DiskId, mediaRow.Cabinet.Substring(1));
                            }
                            else
                            {
                                cabinetFiles.Add(Path.Combine(databaseBasePath, mediaRow.Cabinet));
                            }
                        }
                    }
                }
            }

            // extract the embedded cabinet files from the database
            if (0 < embeddedCabinets.Count)
            {
                using (View streamsView = database.OpenView("SELECT `Data` FROM `_Streams` WHERE `Name` = ?"))
                {
                    foreach (int diskId in embeddedCabinets.Keys)
                    {
                        Record record = new Record(1);

                        record.SetString(1, (string)embeddedCabinets[diskId]);
                        streamsView.Execute(record);
                        record.Close();

                        if (null != (record = streamsView.Fetch()))
                        {
                            // since the cabinets are stored in case-sensitive streams inside the msi, but the file system is not case-sensitive,
                            // embedded cabinets must be extracted to a canonical file name (like their diskid) to ensure extraction will always work
                            string cabinetFile = Path.Combine(this.TempFilesLocation, String.Concat("Media", Path.DirectorySeparatorChar, diskId.ToString(CultureInfo.InvariantCulture), ".cab"));

                            // ensure the parent directory exists
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(cabinetFile));

                            using (FileStream fs = System.IO.File.Create(cabinetFile))
                            {
                                int bytesRead;
                                byte[] buffer = new byte[512];

                                while (0 != (bytesRead = record.GetStream(1, buffer, buffer.Length)))
                                {
                                    fs.Write(buffer, 0, bytesRead);
                                }
                            }

                            record.Close();

                            cabinetFiles.Add(cabinetFile);
                        }
                        else
                        {
                            // TODO: warning about missing embedded cabinet
                        }
                    }
                }
            }

            // extract the cabinet files
            if (0 < cabinetFiles.Count)
            {
                string fileDirectory = Path.Combine(exportBasePath, "File");

                // delete the directory and its files to prevent cab extraction due to an existing file
                if (Directory.Exists(fileDirectory))
                {
                    Directory.Delete(fileDirectory, true);
                }

                // ensure the directory exists of extraction will fail
                Directory.CreateDirectory(fileDirectory);

                foreach (string cabinetFile in cabinetFiles)
                {
                    using (WixExtractCab extractCab = new WixExtractCab())
                    {
                        try
                        {
                            extractCab.Extract(cabinetFile, fileDirectory);
                        }
                        catch (FileNotFoundException)
                        {
                            throw new WixException(WixErrors.FileNotFound(SourceLineNumberCollection.FromFileName(databaseFile), cabinetFile));
                        }
                    }
                }
            }
        }
    }
}
