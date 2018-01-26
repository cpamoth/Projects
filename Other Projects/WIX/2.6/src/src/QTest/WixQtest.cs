//-------------------------------------------------------------------------------------------------
// <copyright file="WiXQTest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// QTest runner for WiX sources.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WixQTest
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;
    using Microsoft.Tools.WindowsInstallerXml;
    using Microsoft.Tools.WindowsInstallerXml.Msi;

    /// <summary>
    /// Run QTests against WiX files.
    /// </summary>
    public class WixQTest
    {
        private const string WixQTestNamespace = "http://schemas.microsoft.com/wix/2005/WixQTest";
        private const int MSICOLINFONAMES = 0;
        private const int MSICOLINFOTYPES = 1;

        // matches lines output by wix tools that indicate errors
        private static Regex wixErrorMessage = new Regex(@":.*error.*:", RegexOptions.Compiled);

        private string compiler;
        private string decompiler;
        private string libber;
        private string linker;
        private bool noTidy;
        private bool showHelp;
        private string singleTest;
        private string testsFile;
        private bool verbose;
        private string wixCop;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The error code for the application.</returns>
        [STAThread]
        public static int Main(string[] args)
        {
            WixQTest wixQTest = new WixQTest();
            return wixQTest.Run(args);
        }

        /// <summary>
        /// Recursively loops through a directory, changing an attribute on all of the underlying files.
        /// An example is to add/remove the ReadOnly flag from each file.
        /// </summary>
        /// <param name="path">The directory path to start deleting from.</param>
        /// <param name="fileAttribute">The FileAttribute to change on each file.</param>
        /// <param name="markAttribute">If true, add the attribute to each file. If false, remove it.</param>
        private static void RecursiveFileAttributes(string path, FileAttributes fileAttribute, bool markAttribute)
        {
            foreach (string subDirectory in Directory.GetDirectories(path))
            {
                RecursiveFileAttributes(subDirectory, fileAttribute, markAttribute);
            }

            foreach (string filePath in Directory.GetFiles(path))
            {
                FileAttributes attributes = File.GetAttributes(filePath);
                if (markAttribute)
                {
                    attributes = attributes | fileAttribute; // add to list of attributes
                }
                else if (fileAttribute == (attributes & fileAttribute)) // if attribute set
                {
                    attributes = attributes ^ fileAttribute; // remove from list of attributes
                }
                File.SetAttributes(filePath, attributes);
            }
        }

        /// <summary>
        /// Compare two msi database files.
        /// </summary>
        /// <param name="beforeDatabaseFile">The before database.</param>
        /// <param name="afterDatabaseFile">The after database.</param>
        /// <returns>The errors found while diffing.</returns>
        private static string DatabaseDiff(string beforeDatabaseFile, string afterDatabaseFile)
        {
            StringBuilder errors = new StringBuilder();

            try
            {
                using (Database beforeDatabase = new Database(beforeDatabaseFile, OpenDatabase.ReadOnly))
                {
                    using (Database afterDatabase = new Database(afterDatabaseFile, OpenDatabase.ReadOnly))
                    {
                        SummaryInformation beforeSummaryInformation = new SummaryInformation(beforeDatabase);
                        SummaryInformation afterSummaryInformation = new SummaryInformation(afterDatabase);

                        // diff the SummaryInformation properties
                        if (beforeSummaryInformation.GetPropertyCount() != afterSummaryInformation.GetPropertyCount())
                        {
                            errors.AppendFormat("The number of SummaryInformation properties has changed from {0} to {1}.{2}", beforeSummaryInformation.GetPropertyCount(), afterSummaryInformation.GetPropertyCount(), Environment.NewLine);
                        }
                        else
                        {
                            int summaryInformationPropertyCount = beforeSummaryInformation.GetPropertyCount();

                            for (int i = 1; i <= 19; i++)
                            {
                                // skip property 9 (PackageCode), 10 (non-existant), 17 (non-existant), and 18 (application)
                                if (i != 9 && i != 10 && i != 17 && i != 18 && beforeSummaryInformation.GetProperty(i).ToString() != afterSummaryInformation.GetProperty(i).ToString())
                                {
                                    errors.AppendFormat("The SummaryInformation property {0} value has changed from '{1}' to '{2}'.{3}", i, beforeSummaryInformation.GetProperty(i), afterSummaryInformation.GetProperty(i), Environment.NewLine);
                                }
                            }
                        }

                        SortedList tableNames = new SortedList();

                        // index all the before tables
                        using (View tablesView = beforeDatabase.OpenExecuteView("SELECT * FROM _Tables"))
                        {
                            Record tableRecord;

                            while (tablesView.Fetch(out tableRecord))
                            {
                                tableNames.Add(tableRecord.GetString(1), null);
                                tableRecord.Close();
                            }
                        }

                        // index all the after tables (and create errors for added tables)
                        using (View tablesView = afterDatabase.OpenExecuteView("SELECT * FROM _Tables"))
                        {
                            Record tableRecord;
                        
                            while (tablesView.Fetch(out tableRecord))
                            {
                                string tableName = tableRecord.GetString(1);

                                if (tableNames.ContainsKey(tableName))
                                {
                                    tableNames[tableName] = String.Empty;
                                }
                                else
                                {
                                    errors.AppendFormat("The '{0}' table has been added.{1}", tableName, Environment.NewLine);
                                }

                                tableRecord.Close();
                            }
                        }

                        // look for removed tables
                        foreach (DictionaryEntry tableEntry in tableNames)
                        {
                            if (tableEntry.Value == null)
                            {
                                errors.AppendFormat("The '{0}' table has been removed.{1}", tableEntry.Key, Environment.NewLine);
                            }
                        }

                        // diff the contents of each table
                        foreach (string tableName in tableNames.Keys)
                        {
                            string afterTableColumnTypes = null;
                            int beforeColumnCount = 0;
                            string beforeTableColumnTypes = null;
                            bool foundError = false;
                            Record record;
                            Hashtable rows = new Hashtable();
                            int sequenceColumn = 0;
                            ArrayList sortedBeforeRows = new ArrayList();
                            ArrayList sortedAfterRows = new ArrayList();

                            // skip tables that were removed
                            if (tableNames[tableName] == null)
                            {
                                continue;
                            }

                            // index all the before rows
                            using (View beforeTableView = beforeDatabase.OpenExecuteView(String.Format("SELECT * FROM {0}", tableName)))
                            {
                                // index the before column definitions and field count
                                beforeTableView.GetColumnInfo(MSICOLINFOTYPES, out record);
                                beforeTableColumnTypes = HashRow(null, record, 0);
                                beforeColumnCount = record.GetFieldCount();

                                // figure out if there is a sequence column
                                beforeTableView.GetColumnInfo(MSICOLINFONAMES, out record);
                                for (int i = 1; i <= record.GetFieldCount(); i++)
                                {
                                    if ("Sequence" == record.GetString(i))
                                    {
                                        sequenceColumn = i;
                                        break;
                                    }
                                }

                                // index the before table rows
                                while (beforeTableView.Fetch(out record))
                                {
                                    string rowHash = HashRow(tableName, record, sequenceColumn);

                                    if (rows.Contains(rowHash))
                                    {
                                        errors.AppendFormat("The '{0}' table row '{1}' is duplicated.{2}", tableName, rowHash, Environment.NewLine);
                                        foundError = true;
                                    }
                                    else
                                    {
                                        rows.Add(rowHash, null);

                                        if (sequenceColumn > 0 && !record.IsNull(sequenceColumn))
                                        {
                                            SequencedRow sequencedRow = new SequencedRow(record.GetInteger(sequenceColumn), rowHash);
                                            sortedBeforeRows.Add(sequencedRow);
                                        }
                                    }

                                    record.Close();
                                }
                            }

                            // index all the after rows (and create errors for added rows)
                            using (View afterTableView = afterDatabase.OpenExecuteView(String.Format("SELECT * FROM {0}", tableName)))
                            {
                                // index the before column definitions
                                afterTableView.GetColumnInfo(MSICOLINFOTYPES, out record);
                                afterTableColumnTypes = HashRow(null, record, 0);

                                // check that the table definition contains the same number of columns
                                if (beforeColumnCount != record.GetFieldCount())
                                {
                                    errors.AppendFormat("The '{0}' table contains a differing number of columns {1} and {2}.{3}", tableName, beforeColumnCount, record.GetFieldCount(), Environment.NewLine);
                                    continue;
                                }

                                while (afterTableView.Fetch(out record))
                                {
                                    string rowHash = HashRow(tableName, record, sequenceColumn);

                                    if (rows.Contains(rowHash))
                                    {
                                        rows[rowHash] = String.Empty;

                                        if (sequenceColumn > 0 && !record.IsNull(sequenceColumn))
                                        {
                                            SequencedRow sequencedRow = new SequencedRow(record.GetInteger(sequenceColumn), rowHash);
                                            sortedAfterRows.Add(sequencedRow);
                                        }
                                    }
                                    else
                                    {
                                        errors.AppendFormat("The '{0}' table row '{1}' was added.{2}", tableName, rowHash, Environment.NewLine);
                                        foundError = true;
                                    }

                                    record.Close();
                                }
                            }

                            // look for removed rows
                            foreach (DictionaryEntry rowEntry in rows)
                            {
                                if (rowEntry.Value == null)
                                {
                                    errors.AppendFormat("The '{0}' table row '{1}' was removed.{2}", tableName, rowEntry.Key, Environment.NewLine);
                                    foundError = true;
                                }
                            }

                            // compare the table definitions
                            if (beforeTableColumnTypes != afterTableColumnTypes)
                            {
                                errors.AppendFormat("The '{0}' table definitions '{1}' and '{2}' do not match.{3}", tableName, beforeTableColumnTypes, afterTableColumnTypes, Environment.NewLine);
                            }

                            // check the ordering of sequenced columns
                            if (sequenceColumn > 0 && !foundError)
                            {
                                if (sortedBeforeRows.Count != sortedAfterRows.Count)
                                {
                                    errors.AppendFormat("The '{0}' table contains a differing number of rows with sequence numbers specified.{1}", tableName, Environment.NewLine);
                                }

                                sortedBeforeRows.Sort();
                                sortedAfterRows.Sort();

                                for (int i = 0; i < sortedBeforeRows.Count; i++)
                                {
                                    if (((SequencedRow)sortedBeforeRows[i]).RowHash != ((SequencedRow)sortedAfterRows[i]).RowHash)
                                    {
                                        errors.AppendFormat("The '{0}' table row '{1}' with sequence {2} was moved around in sequencing.{3}", tableName, ((SequencedRow)sortedBeforeRows[i]).RowHash, ((SequencedRow)sortedBeforeRows[i]).Sequence, Environment.NewLine);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException ioe)
            {
                errors.AppendFormat("{0}{1}", ioe.Message, Environment.NewLine);
            }

            return errors.ToString();
        }

        /// <summary>
        /// Hash a row, ignoring some columns as appropriate.
        /// </summary>
        /// <param name="tableName">Name of the table corresponding to the row being hashed.</param>
        /// <param name="record">Record containing information about the row being hashed.</param>
        /// <param name="sequenceColumn">The column containing the sequence number for this row (if there is one).</param>
        /// <returns>A string hash of the row.</returns>
        private static string HashRow(string tableName, Record record, int sequenceColumn)
        {
            int fieldCount = record.GetFieldCount();
            StringBuilder rowHash = new StringBuilder("|");

            for (int i = 1; i <= fieldCount; i++)
            {
                if (record.IsNull(i))
                {
                    rowHash.Append("null|");
                }
                else
                {
                    // skip the value of ProductCode since its expected to change
                    if (tableName == "Property" && i == 2 && "|ProductCode|" == rowHash.ToString())
                    {
                        continue;
                    }
                    else if (sequenceColumn == i) // skip all sequence columns (they are handled separately)
                    {
                        continue;
                    }

                    try
                    {
                        rowHash.Append(record.GetString(i));
                        rowHash.Append('|');
                    }
                    catch // assume fields that cannot be queried as a string are binary data (which isn't diffed anyways)
                    {
                        rowHash.Append("binary|");
                    }
                }
            }

            return rowHash.ToString();
        }

        /// <summary>
        /// Compare two wixout files.
        /// </summary>
        /// <param name="beforeOutput">The before output.</param>
        /// <param name="afterOutput">The after output.</param>
        /// <returns>The errors found while diffing.</returns>
        /// TODO: This should compare column definitions
        private static string WixoutDiff(Output beforeOutput, Output afterOutput)
        {
            StringBuilder errors = new StringBuilder();

            // check all the top-level data
            if (beforeOutput.Codepage != afterOutput.Codepage)
            {
                errors.AppendFormat("The output codepage differs: {0} changed to {1}.{2}", beforeOutput.Codepage, afterOutput.Codepage, Environment.NewLine);
            }

            if (beforeOutput.Compressed != afterOutput.Compressed)
            {
                errors.AppendFormat("The output compressed flag differs: {0} changed to {1}.{2}", beforeOutput.Compressed, afterOutput.Compressed, Environment.NewLine);
            }

            if (beforeOutput.LongFileNames != afterOutput.LongFileNames)
            {
                errors.AppendFormat("The output long file name flag differs: {0} changed to {1}.{2}", beforeOutput.LongFileNames, afterOutput.LongFileNames, Environment.NewLine);
            }

            if (beforeOutput.ModularizationGuid != afterOutput.ModularizationGuid)
            {
                errors.AppendFormat("The output modularization guid differs: {0} changed to {1}.{2}", beforeOutput.ModularizationGuid, afterOutput.ModularizationGuid, Environment.NewLine);
            }

            if (beforeOutput.SuppressAdminSequence != afterOutput.SuppressAdminSequence)
            {
                errors.AppendFormat("The output suppress AdminSequence flag differs: {0} changed to {1}.{2}", beforeOutput.SuppressAdminSequence, afterOutput.SuppressAdminSequence, Environment.NewLine);
            }

            if (beforeOutput.SuppressAdvertiseSequence != afterOutput.SuppressAdvertiseSequence)
            {
                errors.AppendFormat("The output suppress AdvertiseSequence flag differs: {0} changed to {1}.{2}", beforeOutput.SuppressAdvertiseSequence, afterOutput.SuppressAdvertiseSequence, Environment.NewLine);
            }

            if (beforeOutput.SuppressUISequence != afterOutput.SuppressUISequence)
            {
                errors.AppendFormat("The output suppress UISequence flag differs: {0} changed to {1}.{2}", beforeOutput.SuppressUISequence, afterOutput.SuppressUISequence, Environment.NewLine);
            }

            if (beforeOutput.Type != afterOutput.Type)
            {
                errors.AppendFormat("The output type differs: {0} changed to {1}.{2}", beforeOutput.Type, afterOutput.Type, Environment.NewLine);
            }

            // check FileMediaInformation rows
            if (beforeOutput.FileMediaInformationCollection.Count != afterOutput.FileMediaInformationCollection.Count)
            {
                errors.AppendFormat("The output count of FileMediaInformation rows differs: {0} changed to {1}.{2}", beforeOutput.FileMediaInformationCollection.Count, afterOutput.FileMediaInformationCollection.Count, Environment.NewLine);
            }

            foreach (FileMediaInformation beforeFmi in beforeOutput.FileMediaInformationCollection)
            {
                FileMediaInformation afterFmi = afterOutput.FileMediaInformationCollection[beforeFmi.File];

                if (null == afterFmi ||
                    beforeFmi.FileCompression != afterFmi.FileCompression ||
                    beforeFmi.FileId != afterFmi.FileId ||
                    beforeFmi.IsInModule != afterFmi.IsInModule ||
                    beforeFmi.Media != afterFmi.Media)
                {
                    errors.AppendFormat("A FileMediaInformation row differs.{0}", Environment.NewLine);
                }
            }

            // check IgnoreModularizations
            if (beforeOutput.IgnoreModularizations.Count != afterOutput.IgnoreModularizations.Count)
            {
                errors.AppendFormat("The output count of IgnoreModularization rows differs: {0} changed to {1}.{2}", beforeOutput.IgnoreModularizations.Count, afterOutput.IgnoreModularizations.Count, Environment.NewLine);
            }

            foreach (IgnoreModularization beforeIm in beforeOutput.IgnoreModularizations)
            {
                bool found = false;

                foreach (IgnoreModularization afterIm in afterOutput.IgnoreModularizations)
                {
                    if (beforeIm.Name == afterIm.Name)
                    {
                        found = true;
                        
                        if (beforeIm.Type != afterIm.Type)
                        {
                            errors.AppendFormat("An IgnoreModularization row differs.{0}", Environment.NewLine);
                        }

                        break;
                    }
                }

                if (!found)
                {
                    errors.AppendFormat("An IgnoreModularization row has been deleted.{0}", Environment.NewLine);
                }
            }

            // check ImportStreams
            if (beforeOutput.ImportStreams.Count != afterOutput.ImportStreams.Count)
            {
                errors.AppendFormat("The output count of ImportStream rows differs: {0} changed to {1}.{2}", beforeOutput.ImportStreams.Count, afterOutput.ImportStreams.Count, Environment.NewLine);
            }

            foreach (ImportStream beforeIs in beforeOutput.ImportStreams)
            {
                bool found = false;

                foreach (ImportStream afterIs in afterOutput.ImportStreams)
                {
                    if (beforeIs.Name == afterIs.Name)
                    {
                        found = true;
                        
                        if (0 != String.Compare(beforeIs.Path, afterIs.Path, true) ||
                            beforeIs.StreamName != afterIs.StreamName ||
                            beforeIs.Type != afterIs.Type)
                        {
                            errors.AppendFormat("An ImportStream row differs.{0}", Environment.NewLine);
                        }

                        break;
                    }
                }

                if (!found)
                {
                    errors.AppendFormat("An ImportStream row has been deleted.{0}", Environment.NewLine);
                }
            }

            // check MediaRows
            if (beforeOutput.MediaRows.Count != afterOutput.MediaRows.Count)
            {
                errors.AppendFormat("The output count of Media rows differs: {0} changed to {1}.{2}", beforeOutput.MediaRows.Count, afterOutput.MediaRows.Count, Environment.NewLine);
            }

            foreach (MediaRow beforeMr in beforeOutput.MediaRows)
            {
                bool found = false;

                foreach (MediaRow afterMr in afterOutput.MediaRows)
                {
                    if (beforeMr.DiskId == afterMr.DiskId)
                    {
                        found = true;

                        if (beforeMr.Cabinet != afterMr.Cabinet ||
                            beforeMr.CompressionLevel != afterMr.CompressionLevel ||
                            beforeMr.DiskPrompt != afterMr.DiskPrompt ||
                            beforeMr.LastSequence != afterMr.LastSequence ||
                            beforeMr.Source != afterMr.Source ||
                            beforeMr.VolumeLabel != afterMr.VolumeLabel)
                        {
                            errors.AppendFormat("A Media row differs.{0}", Environment.NewLine);
                        }

                        break;
                    }
                }

                if (!found)
                {
                    errors.AppendFormat("A Media row has been deleted.{0}", Environment.NewLine);
                }
            }

            // diff the output tables (yep this is less than optimal)
            foreach (OutputTable beforeTable in beforeOutput.OutputTables)
            {
                if (null == afterOutput.OutputTables[beforeTable.Name])
                {
                    // table not found
                    errors.AppendFormat("The '{0}' table in the output has been deleted.{1}", beforeTable.Name, Environment.NewLine);
                }
            }

            foreach (OutputTable afterTable in afterOutput.OutputTables)
            {
                if (null == beforeOutput.OutputTables[afterTable.Name])
                {
                    // table not found
                    errors.AppendFormat("The '{0}' table in the output has been added.{1}", afterTable.Name, Environment.NewLine);
                }
            }

            // diff the contents of each table
            if (errors.Length == 0)
            {
                foreach (OutputTable beforeOutputTable in beforeOutput.OutputTables)
                {
                    bool foundError = false;
                    Hashtable rows = new Hashtable();
                    int sequenceColumn = 0;
                    ArrayList sortedBeforeRows = new ArrayList();
                    ArrayList sortedAfterRows = new ArrayList();

                    // check that the table definition contains the same number of columns
                    if (beforeOutputTable.TableDefinition.Columns.Count != afterOutput.OutputTables[beforeOutputTable.Name].TableDefinition.Columns.Count)
                    {
                        errors.AppendFormat("The '{0}' table contains a differing number of columns {1} and {2}.{3}", beforeOutputTable.Name, beforeOutputTable.TableDefinition.Columns.Count, afterOutput.OutputTables[beforeOutputTable.Name].TableDefinition.Columns.Count, Environment.NewLine);
                        continue;
                    }

                    // find the Sequence column if there is one
                    if (beforeOutputTable.OutputRows.Count > 0)
                    {
                        for (int i = 0; i < beforeOutputTable.TableDefinition.Columns.Count; i++)
                        {
                            if (beforeOutputTable.TableDefinition.Columns[i].Name == "Sequence")
                            {
                                sequenceColumn = i;
                            }
                        }
                    }
                    else if (afterOutput.OutputTables[beforeOutputTable.Name].OutputRows.Count > 0)
                    {
                        for (int i = 0; i < beforeOutputTable.TableDefinition.Columns.Count; i++)
                        {
                            if (beforeOutputTable.TableDefinition.Columns[i].Name == "Sequence")
                            {
                                sequenceColumn = i;
                            }
                        }
                    }

                    foreach (OutputRow beforeOutputRow in beforeOutputTable.OutputRows)
                    {
                        string rowHash = HashRow(beforeOutputRow.Row);

                        if (rows.Contains(rowHash))
                        {
                            // duplicate row
                            errors.AppendFormat("Duplicate row found in the '{0}' table of the output.{1}", beforeOutputTable.Name, Environment.NewLine);
                            foundError = true;
                        }
                        else
                        {
                            rows.Add(rowHash, null);

                            if (sequenceColumn > 0 && beforeOutputRow.Row[sequenceColumn] != null && beforeOutputRow.Row[sequenceColumn].ToString().Length > 0)
                            {
                                SequencedRow sequencedRow = new SequencedRow(Convert.ToInt32(beforeOutputRow.Row[sequenceColumn]), rowHash);
                                sortedBeforeRows.Add(sequencedRow);
                            }
                        }
                    }

                    foreach (OutputRow afterOutputRow in afterOutput.OutputTables[beforeOutputTable.Name].OutputRows)
                    {
                        string rowHash = HashRow(afterOutputRow.Row);

                        if (rows.ContainsKey(rowHash.ToString()))
                        {
                            rows[rowHash.ToString()] = string.Empty;

                            if (sequenceColumn > 0 && afterOutputRow.Row[sequenceColumn] != null && afterOutputRow.Row[sequenceColumn].ToString().Length > 0)
                            {
                                SequencedRow sequencedRow = new SequencedRow(Convert.ToInt32(afterOutputRow.Row[sequenceColumn]), rowHash);
                                sortedAfterRows.Add(sequencedRow);
                            }
                        }
                        else
                        {
                            // found a mismatched row
                            errors.AppendFormat("Changed row found in the '{0}' table of the output.{1}", beforeOutputTable.Name, Environment.NewLine);
                            foundError = true;
                        }
                    }

                    foreach (string found in rows.Values)
                    {
                        if (null == found)
                        {
                            // null means a matching right row was not found
                            errors.AppendFormat("A row has been deleted from the '{0}' table of the output.{1}", beforeOutputTable.Name, Environment.NewLine);
                            foundError = true;
                        }
                    }

                    // check the ordering of sequenced columns
                    if (sequenceColumn > 0 && !foundError)
                    {
                        if (sortedBeforeRows.Count != sortedAfterRows.Count)
                        {
                            errors.AppendFormat("The '{0}' table contains a differing number of rows with sequence numbers specified.{1}", beforeOutputTable.Name, Environment.NewLine);
                        }

                        sortedBeforeRows.Sort();
                        sortedAfterRows.Sort();

                        for (int i = 0; i < sortedBeforeRows.Count; i++)
                        {
                            if (((SequencedRow)sortedBeforeRows[i]).RowHash != ((SequencedRow)sortedAfterRows[i]).RowHash)
                            {
                                errors.AppendFormat("The '{0}' table row '{1}' with sequence {2} was moved around in sequencing.{3}", beforeOutputTable.Name, ((SequencedRow)sortedBeforeRows[i]).RowHash, ((SequencedRow)sortedBeforeRows[i]).Sequence, Environment.NewLine);
                            }
                        }
                    }
                }
            }

            return errors.ToString();
        }

        /// <summary>
        /// Hash a row, ignoring some columns as appropriate.
        /// </summary>
        /// <param name="row">Row to hash.</param>
        /// <returns>A string hash of the row.</returns>
        private static string HashRow(Row row)
        {
            StringBuilder rowHash = new StringBuilder();

            for (int i = 0; i < row.Fields.Length; i++)
            {
                Field field = row.Fields[i];

                // skip some values that can change each time the wixout is built
                if (("_SummaryInformation" == row.Table.Name && "9" == (string)row[0]))
                {
                    continue;
                }
                else if (field.Name == "Sequence") // skip all sequence columns (they are handled separately)
                {
                    continue;
                }

                if (("File" == row.Table.Name && 13 == i))
                {
                    rowHash.Append("src");
                }
                else if (null == field.Data)
                {
                    rowHash.Append("null");
                }
                else
                {
                    rowHash.Append(field.Data.ToString());
                }
            }

            return rowHash.ToString();
        }

        /// <summary>
        /// Run the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The error code for the application.</returns>
        private int Run(string[] args)
        {
            Process compilerProcess = null;
            Process decompilerProcess = null;
            Process libberProcess = null;
            Process linkerProcess = null;
            Process wixcopProcess = null;
            
            TempFileCollection tempFileCollection = new TempFileCollection();
            tempFileCollection.KeepFiles = !this.noTidy;

            try
            {
                this.ParseCommandline(args);

                // get the assemblies
                Assembly thisAssembly = Assembly.GetExecutingAssembly();

                if (this.showHelp)
                {
                    Console.WriteLine("Microsoft (R) WiX QTest version {0}", thisAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation. All rights reserved.");
                    Console.WriteLine();
                    Console.WriteLine(" usage: wixQTest [-?] -c<compiler> -d<decompiler> -l<linker> -w<wixcop> -y<libber> tests.xml");
                    Console.WriteLine();
                    Console.WriteLine("   -notidy           Do not delete temporary files (for checking results)");
                    Console.WriteLine("   -t<Test_name>     Run only the specified test (this is case-sensitive)");

                    return 0;
                }

                // create a process for each command
                compilerProcess = this.CreateProcess(this.compiler);
                decompilerProcess = this.CreateProcess(this.decompiler);
                libberProcess = this.CreateProcess(this.libber);
                linkerProcess = this.CreateProcess(this.linker);
                wixcopProcess = this.CreateProcess(this.wixCop);

                // load the schema
                XmlReader schemaReader = null;
                XmlSchemaCollection schemas = null;
                try
                {
                    schemaReader = new XmlTextReader(thisAssembly.GetManifestResourceStream("Microsoft.Tools.WixQTest.Xsd.tests.xsd"));
                    schemas = new XmlSchemaCollection();
                    schemas.Add(WixQTestNamespace, schemaReader);
                }
                finally
                {
                    schemaReader.Close();
                }

                // load the tests xml file
                XmlTextReader reader = null;
                XmlDocument doc = new XmlDocument();
                try
                {
                    reader = new XmlTextReader(this.testsFile);
                    XmlValidatingReader validatingReader = new XmlValidatingReader(reader);
                    validatingReader.Schemas.Add(schemas);

                    // load the xml into a DOM
                    doc.Load(validatingReader);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }

                // create a namespace manager
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("wqt", WixQTestNamespace);
                namespaceManager.PushScope();

                // query for the tests
                XmlNodeList testNodes = doc.SelectNodes("/wqt:Tests/wqt:Test", namespaceManager);

                // run the tests
                int failedTests = 0;
                int totalTests = 0;
                StringCollection lastResults = new StringCollection();
                foreach (XmlNode testNode in testNodes)
                {
                    bool testFailed = false;
                    string errors = String.Empty;

                    // get the test name
                    string name = testNode.Attributes["Name"].Value;

                    // skip tests that we are not performing
                    if (this.singleTest != null && 0 != String.Compare(this.singleTest, name, true, CultureInfo.InvariantCulture))
                    {
                        continue;
                    }
                    totalTests++;
                    Console.Write("{0} of {1} - {2}{3}: ", totalTests, (this.singleTest != null ? 1 : testNodes.Count), name, new String('.', 30 - name.Length));

                    // get the test's extensions
                    string[] extensions = null;
                    XmlAttribute extensionsAttribute = testNode.Attributes["Extensions"];
                    if (extensionsAttribute != null)
                    {
                        extensions = extensionsAttribute.Value.Split(new char[] { ';' });
                    }

                    // get the test's expected result
                    string expectedResult = null;
                    XmlAttribute expectedResultAttribute = testNode.Attributes["ExpectedResult"];
                    if (expectedResultAttribute != null)
                    {
                        expectedResult = Path.GetFullPath(expectedResultAttribute.Value);
                    }

                    // create a directory for this test's files
                    string testDirectory = Path.Combine(tempFileCollection.BasePath, name);
                    Directory.CreateDirectory(testDirectory);

                    // process the compile node
                    XmlNode compileNode = testNode["Compile"];
                    if (compileNode != null)
                    {
                        StringBuilder commandline = this.CreateCommandline(compileNode, extensions);
                        StringBuilder wixcopCommandline = new StringBuilder();

                        XmlNodeList sourceFileNodes = compileNode.SelectNodes("wqt:SourceFile", namespaceManager);
                        foreach (XmlNode sourceFileNode in sourceFileNodes)
                        {
                            string sourceFile = Path.GetFullPath(sourceFileNode.InnerText);

                            // add the source file to the compiler and wixcop commandlines
                            commandline.AppendFormat(" \"{0}\"", sourceFile);
                            wixcopCommandline.AppendFormat(" \"{0}\"", sourceFile);

                            // create the path to the corresponding object file
                            string objectFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFile);
                            string objectFileName = String.Concat(objectFileNameWithoutExtension, ".wixobj");
                            string objectFile = Path.Combine(testDirectory, objectFileName);

                            // add the object file to the list of last results
                            lastResults.Add(objectFile);
                        }

                        // add the output directory to the commandline (note the additional trailing slash)
                        commandline.AppendFormat(" -out \"{0}{1}\\\"", testDirectory, Path.DirectorySeparatorChar);

                        // run the wixcop command
                        errors = this.RunCommand(wixcopProcess, wixcopCommandline.ToString());

                        // run the compiler command (if no errors were found by wixcop)
                        if (errors.Length == 0)
                        {
                            errors = this.RunCommand(compilerProcess, commandline.ToString());
                        }
                    }

                    // process the lib node
                    XmlNode libNode = testNode["Lib"];
                    if (!testFailed && libNode != null)
                    {
                        if (errors.Length != 0)
                        {
                            testFailed = true;
                        }
                        else
                        {
                            StringBuilder commandline = this.CreateCommandline(libNode, extensions);

                            // add the last results
                            foreach (string lastResult in lastResults)
                            {
                                commandline.AppendFormat(" \"{0}\"", lastResult);
                            }
                            lastResults.Clear();

                            // generate a library file name
                            string libraryFile = Path.Combine(testDirectory, "library.wixlib");

                            // add the library file to the libber commandline and the list of last results
                            commandline.AppendFormat(" -out \"{0}\"", libraryFile);
                            lastResults.Add(libraryFile);

                            // run the command
                            errors = this.RunCommand(libberProcess, commandline.ToString());
                        }
                    }

                    // process the link node
                    XmlNode linkNode = testNode["Link"];
                    if (!testFailed && linkNode != null)
                    {
                        if (errors.Length != 0)
                        {
                            testFailed = true;
                        }
                        else
                        {
                            StringBuilder commandline = this.CreateCommandline(linkNode, extensions);

                            // add the last results
                            foreach (string lastResult in lastResults)
                            {
                                commandline.AppendFormat(" \"{0}\"", lastResult);
                            }
                            lastResults.Clear();

                            // add the library files
                            XmlNodeList libraryFileNodes = linkNode.SelectNodes("wqt:LibraryFile", namespaceManager);
                            foreach (XmlNode libraryFileNode in libraryFileNodes)
                            {
                                commandline.AppendFormat(" \"{0}\"", Environment.ExpandEnvironmentVariables(libraryFileNode.InnerText));
                            }

                            // add the localization files
                            XmlNodeList localizationFileNodes = linkNode.SelectNodes("wqt:LocalizationFile", namespaceManager);
                            foreach (XmlNode localizationFileNode in localizationFileNodes)
                            {
                                commandline.AppendFormat(" -loc \"{0}\"", Environment.ExpandEnvironmentVariables(localizationFileNode.InnerText));
                            }

                            // use the expected result name if there is one, otherwise use a hard-coded name
                            string outputFile = "link_result.msi";
                            if (expectedResult != null)
                            {
                                outputFile = Path.GetFileName(expectedResult);
                            }
                            outputFile = Path.Combine(testDirectory, outputFile);

                            // add the output file to the linker commandline and the list of last results
                            commandline.AppendFormat(" -out \"{0}\"", outputFile);
                            lastResults.Add(outputFile);

                            // run the command
                            errors = this.RunCommand(linkerProcess, commandline.ToString());
                        }
                    }

                    // process the decompileAndDiff node
                    XmlNode decompileNode = testNode["DecompileAndDiff"];
                    if (!testFailed && decompileNode != null)
                    {
                        if (errors.Length != 0)
                        {
                            testFailed = true;
                        }
                        else
                        {
                            string binariesDirectory = Path.Combine(testDirectory, "binaries");
                            StringBuilder commandline = this.CreateCommandline(decompileNode, extensions);

                            // check the number of previous results is correct
                            if (lastResults.Count != 1)
                            {
                                throw new ApplicationException("Misconfiguration in tests.xml: command before DecompileAndDiff does not produce the correct number of results.");
                            }

                            // create decompiled source file, wixobj file, and new result file
                            string previousResultFile = lastResults[0];
                            string decompiledSourceFile = Path.Combine(testDirectory, "decompiledSource.wxs");
                            string decompiledObjectFile = Path.Combine(testDirectory, "decompiledSource.wixobj");
                            string decompiledOutputFile = Path.Combine(testDirectory, String.Concat("decompiled_", Path.GetFileName(lastResults[0])));

                            // add the argument to extract binaries
                            commandline.AppendFormat(" -x \"{0}\"", binariesDirectory);

                            // add the last result
                            commandline.AppendFormat(" \"{0}\"", previousResultFile);
                            lastResults.Clear();

                            // add the decompiled source file to the commandline
                            commandline.AppendFormat(" \"{0}\"", decompiledSourceFile);

                            // run the decompiler command
                            errors = this.RunCommand(decompilerProcess, commandline.ToString());

                            // run the compiler command
                            if (errors.Length == 0)
                            {
                                StringBuilder compilerCommandline = this.CreateCommandline(testNode["Compile"], extensions);
                                compilerCommandline.AppendFormat(" \"{0}\" -out \"{1}\"", decompiledSourceFile, decompiledObjectFile);
                                errors = this.RunCommand(compilerProcess, compilerCommandline.ToString());
                            }

                            // run the linker command
                            if (errors.Length == 0)
                            {
                                StringBuilder linkerCommandline = this.CreateCommandline(testNode["Link"], extensions);
                                linkerCommandline.AppendFormat(" -b \"{0}\" \"{1}\" -out \"{2}\"", binariesDirectory, decompiledObjectFile, decompiledOutputFile);
                                errors = this.RunCommand(linkerProcess, linkerCommandline.ToString());
                            }

                            // diff the results
                            if (errors.Length == 0)
                            {
                                errors = DatabaseDiff(decompiledOutputFile, previousResultFile);
                                if (errors.Length != 0)
                                {
                                    errors = String.Format(
                                        "Error(s) found while comparing:{0}{1}{2}{3}{4}{5}",
                                        Environment.NewLine,
                                        previousResultFile,
                                        Environment.NewLine,
                                        decompiledOutputFile,
                                        Environment.NewLine,
                                        errors);
                                }
                            }

                            // add extra information to decompiler errors
                            if (errors.Length != 0)
                            {
                                errors = String.Concat("Errors encountered during decompilation and rebuilding of decompiled sources:", Environment.NewLine, errors);
                            }

                            // save the decompiled output file as the result
                            lastResults.Add(decompiledOutputFile);
                        }
                    }

                    // check the actual result against the expected result
                    if (expectedResult != null)
                    {
                        if (errors.Length != 0)
                        {
                            testFailed = true;
                        }
                        else
                        {
                            // check the number of previous results is correct
                            if (lastResults.Count != 1)
                            {
                                throw new ApplicationException("Misconfiguration in tests.xml: last command does not produce the correct number of results.");
                            }

                            // check for the wixout extension to determine if we use the wixout diff
                            string fileExtension = Path.GetExtension(expectedResult);
                            if (String.Compare(fileExtension, ".wixout", true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (decompileNode != null)
                                {
                                    errors = "Decompilation is not currently supported if the expected result is a wixout.";
                                }
                                else
                                {
                                    Output expectedOutput = null;
                                    Output lastOutput = null;

                                    try
                                    {
                                        lastOutput = Output.Load(lastResults[0], false);
                                    }
                                    catch
                                    {
                                        errors = String.Format("Failed to open '{0}'.", lastResults[0]);
                                    }

                                    try
                                    {
                                        expectedOutput = Output.Load(expectedResult, false);
                                    }
                                    catch
                                    {
                                        errors = String.Format("Failed to open '{0}'.", expectedResult);
                                    }

                                    // run the wixout differ command
                                    if (expectedOutput != null && lastOutput != null && errors.Length == 0)
                                    {
                                        errors = WixoutDiff(lastOutput, expectedOutput);
                                    }
                                }
                            }
                            else
                            {
                                // diff the msi database results
                                errors = DatabaseDiff(expectedResult, lastResults[0]);
                            }

                            // add extra info if there was an error detected
                            if (errors.Length != 0)
                            {
                                errors = String.Format(
                                    "Error(s) found while comparing:{0}{1}{2}{3}{4}{5}",
                                    Environment.NewLine,
                                    lastResults[0],
                                    Environment.NewLine,
                                    expectedResult,
                                    Environment.NewLine,
                                    errors);
                                testFailed = true;
                            }
                        }
                    }
                    else // expected failure
                    {
                        if (errors.Length == 0)
                        {
                            errors = "Expected failure, actual result was success.";
                            testFailed = true;
                        }
                    }

                    // output the results of this test
                    if (testFailed)
                    {
                        Console.WriteLine("Failure");
                        Console.WriteLine();
                        Console.WriteLine(errors);
                        Console.WriteLine();
                        failedTests++;
                    }
                    else
                    {
                        Console.WriteLine("Success");
                    }

                    // clear the last results files
                    lastResults.Clear();
                }

                // output the number of failures
                Console.WriteLine();
                Console.WriteLine();
                if (totalTests == 0)
                {
                    if (this.singleTest != null)
                    {
                        Console.WriteLine("Could not find the '{0}' test.", this.singleTest);
                    }
                    else
                    {
                        Console.WriteLine("Could not find any tests.");
                    }
                }
                else
                {
                    if (failedTests > 0)
                    {
                        Console.WriteLine("Failed {0} out of {1} test{2}.", failedTests, totalTests, (totalTests != 1 ? "s" : ""));
                    }
                    else if (totalTests > 0)
                    {
                        Console.WriteLine("Successful tests: {0}", totalTests);
                    }

                    if (this.noTidy)
                    {
                        Console.WriteLine();
                        Console.WriteLine("The notidy option was specified, temporary files can be found at:");
                        Console.WriteLine(tempFileCollection.BasePath);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("WixQTest.exe : fatal error WXQT0001: {0}", e.Message);
                Console.WriteLine();
                Console.WriteLine("Stack Trace:");
                Console.WriteLine(e.StackTrace);

                if (e is NullReferenceException)
                {
                    throw;
                }

                return 1;
            }
            finally
            {
                if (compilerProcess != null)
                {
                    compilerProcess.Close();
                }

                if (decompilerProcess != null)
                {
                    decompilerProcess.Close();
                }

                if (libberProcess != null)
                {
                    libberProcess.Close();
                }

                if (linkerProcess != null)
                {
                    linkerProcess.Close();
                }

                if (!this.noTidy)
                {
                    // try three times and give up with a warning if the temp files aren't gone by then
                    const int retryLimit = 3;
                    for (int i = 0; i < retryLimit; i++)
                    {
                        try
                        {
                            Directory.Delete(tempFileCollection.BasePath, true);   // toast the whole temp directory
                            break; // no exception means we got success the first time
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (0 == i) // should only need to unmark readonly once - there's no point in doing it again and again
                            {
                                RecursiveFileAttributes(tempFileCollection.BasePath, FileAttributes.ReadOnly, false); // toasting will fail if any files are read-only. Try changing them to not be.
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (DirectoryNotFoundException)
                        {
                            // if the path doesn't exist, then there is nothing for us to worry about
                            break;
                        }
                        catch (IOException) // directory in use
                        {
                            if (i == (retryLimit - 1)) // last try failed still, give up
                            {
                                break;
                            }
                            System.Threading.Thread.Sleep(300);  // sleep a bit before trying again
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Create a new command line with some default arguments and the given extensions.
        /// </summary>
        /// <param name="commandNode">Optional command node.</param>
        /// <param name="extensions">The extensions to pass into the WiX tool.</param>
        /// <returns>A new commandline.</returns>
        private StringBuilder CreateCommandline(XmlNode commandNode, string[] extensions)
        {
            StringBuilder commandline = new StringBuilder();

            // turn on all warnings and verbosity levels
            commandline.Append(" -w0 -wx -pedantic:legendary");

            // add optional additional arguments
            if (commandNode != null)
            {
                XmlAttribute argumentsAttribute = commandNode.Attributes["Arguments"];
                if (argumentsAttribute != null)
                {
                    commandline.AppendFormat(" {0}", argumentsAttribute.Value);
                }
            }

            // add the extensions
            if (extensions != null)
            {
                foreach (string extension in extensions)
                {
                    commandline.AppendFormat(" -ext \"{0}\"", extension);
                }
            }

            return commandline;
        }

        /// <summary>
        /// Create a new process for running a particular command.
        /// </summary>
        /// <param name="fileName">Path to the file containing the command to run.</param>
        /// <returns>The new process.</returns>
        private Process CreateProcess(string fileName)
        {
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            return process;
        }

        /// <summary>
        /// Runs a command with the given commandline and parses the results for errors.
        /// </summary>
        /// <param name="process">Process representing the program to run.</param>
        /// <param name="commandline">The commandline to run.</param>
        /// <returns>
        /// A string containing any errors that were encountered while running the command;
        /// empty string if none were encountered.</returns>
        private string RunCommand(Process process, string commandline)
        {
            if (this.verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Running: {0} {1}", process.StartInfo.FileName, commandline);
            }
            process.StartInfo.Arguments = commandline;
            process.Start();

            string line;
            StringBuilder errors = new StringBuilder();

            // selectively grab stdout messages
            while (null != (line = process.StandardOutput.ReadLine()))
            {
                if (this.verbose)
                {
                    Console.WriteLine(line);
                }

                if (errors.Length > 0 || wixErrorMessage.IsMatch(line))
                {
                    errors.AppendFormat("{0}{1}", line, Environment.NewLine);
                }
            }

            // grab all stderr messages
            while (null != (line = process.StandardError.ReadLine()))
            {
                if (this.verbose)
                {
                    Console.WriteLine(line);
                }
                errors.AppendFormat("{0}{1}", line, Environment.NewLine);
            }

            process.WaitForExit();

            if (this.verbose)
            {
                Console.WriteLine("Exit code: {0}", process.ExitCode);
            }

            // check for errors running the process
            if (process.ExitCode != 0 && errors.Length == 0)
            {
                errors.AppendFormat("Unknown problem running test; exit code: {0}.", process.ExitCode);
            }

            return errors.ToString();
        }

        /// <summary>
        /// Parse the command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void ParseCommandline(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    if (arg.Length > 1)
                    {
                        switch (arg[1])
                        {
                            case '?':
                                this.showHelp = true;
                                break;
                            case 'c':
                                this.compiler = arg.Substring(2);
                                break;
                            case 'd':
                                this.decompiler = arg.Substring(2);
                                break;
                            case 'l':
                                this.linker = arg.Substring(2);
                                break;
                            case 'n':
                                this.noTidy = true;
                                break;
                            case 't':
                                this.singleTest = arg.Substring(2);
                                break;
                            case 'v':
                                this.verbose = true;
                                break;
                            case 'w':
                                this.wixCop = arg.Substring(2);
                                break;
                            case 'y':
                                this.libber = arg.Substring(2);
                                break;
                            default:
                                throw new ArgumentException(String.Format("Unrecognized commandline parameter '{0}'.", arg));
                        }
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Unrecognized commandline parameter '{0}'.", arg));
                    }
                }
                else if (this.testsFile == null)
                {
                    this.testsFile = arg;
                }
                else
                {
                    throw new ArgumentException(String.Format("Unrecognized argument '{0}'.", arg));
                }
            }

            // check for missing mandatory arguments
            if (!this.showHelp && (this.compiler == null || this.decompiler == null || this.linker == null || this.libber == null || this.testsFile == null))
            {
                throw new ArgumentException("Missing mandatory argument.");
            }
        }

        /// <summary>
        /// Wraps up a sequenced row for sorting during comparison.
        /// </summary>
        private struct SequencedRow : IComparable
        {
            private int sequence;
            private string rowHash;

            /// <summary>
            /// Instantiate a new SequencedRow.
            /// </summary>
            /// <param name="sequence">The sequence number of the row.</param>
            /// <param name="rowHash">A hash of the data in the row (excluding the Sequence column).</param>
            public SequencedRow(int sequence, string rowHash)
            {
                this.sequence = sequence;
                this.rowHash = rowHash;
            }

            /// <summary>
            /// Gets a hash of the data in the row (excluding the Sequence column).
            /// </summary>
            /// <value>A hash of the data in the row (excluding the Sequence column).</value>
            public string RowHash
            {
                get { return this.rowHash; }
            }

            /// <summary>
            /// Gets the sequence number of the row.
            /// </summary>
            /// <value>The sequence number of the row.</value>
            public int Sequence
            {
                get { return this.sequence; }
            }

            /// <summary>
            /// Compares this instance with a specified SequencedRow object.
            /// </summary>
            /// <param name="obj">The other SequencedRow to compare against.</param>
            /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
            public int CompareTo(object obj)
            {
                int compare = this.sequence.CompareTo(((SequencedRow)obj).sequence);

                if (0 == compare)
                {
                    compare = this.rowHash.CompareTo(((SequencedRow)obj).rowHash);
                }

                return compare;
            }
        }
    }
}
