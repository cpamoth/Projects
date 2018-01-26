//-------------------------------------------------------------------------------------------------
// <copyright file="Differ.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Creates a transform by diffing two outputs.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Globalization;

    using Microsoft.Tools.WindowsInstallerXml.Msi;

    /// <summary>
    /// Creates a transform by diffing two outputs.
    /// </summary>
    public sealed class Differ : IMessageHandler
    {
        private bool suppressKeepingSpecialRows;
        private bool preserveUnchangedRows;
        private const char sectionDelimiter = '/';

        /// <summary>
        /// Instantiates a new Differ class.
        /// </summary>
        public Differ()
        {
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets or sets the option to suppress keeping special rows.
        /// </summary>
        /// <value>The option to suppress keeping special rows.</value>
        public bool SuppressKeepingSpecialRows
        {
            get { return this.suppressKeepingSpecialRows; }
            set { this.suppressKeepingSpecialRows = value; }
        }

        /// <summary>
        /// Gets or sets the flag to determine if all rows, even unchanged ones will be persisted in the output.
        /// </summary>
        /// <value>The option to keep all rows including unchanged rows.</value>
        public bool PreserveUnchangedRows
        {
            get { return this.preserveUnchangedRows; }
            set { this.preserveUnchangedRows = value; }
        }

        /// <summary>
        /// Creates a transform by diffing two outputs.
        /// </summary>
        /// <param name="targetOutput">The target output.</param>
        /// <param name="updatedOutput">The updated output.</param>
        /// <returns>The transform.</returns>
        public Output Diff(Output targetOutput, Output updatedOutput)
        {
            Output transform = new Output(null);
            transform.Type = OutputType.Transform;
            transform.Codepage = updatedOutput.Codepage;

            string targetProductCode = null;
            string targetProductVersion = null;
            string targetUpgradeCode = null;
            string updatedProductCode = null;
            string updatedProductVersion = null;

            // compare the codepages
            if (targetOutput.Codepage != updatedOutput.Codepage)
            {
                this.OnMessage(WixErrors.OutputCodepageMismatch(targetOutput.SourceLineNumbers, targetOutput.Codepage, updatedOutput.Codepage));
                if (null != updatedOutput.SourceLineNumbers)
                {
                    this.OnMessage(WixErrors.OutputCodepageMismatch2(updatedOutput.SourceLineNumbers));
                }
            }

            // compare the output types
            if (targetOutput.Type != updatedOutput.Type)
            {
                throw new WixException(WixErrors.OutputTypeMismatch(targetOutput.SourceLineNumbers, targetOutput.Type.ToString(), updatedOutput.Type.ToString()));
            }

            // compare the contents of the tables
            foreach (Table targetTable in targetOutput.Tables)
            {
                Table updatedTable = updatedOutput.Tables[targetTable.Name];

                // dropped tables
                if (null == updatedTable)
                {
                    Table droppedTable = transform.Tables.EnsureTable(null, targetTable.Definition);
                    droppedTable.Operation = TableOperation.Drop;
                }
                else // possibly modified tables
                {
                    SortedList updatedPrimaryKeys = new SortedList();
                    SortedList targetPrimaryKeys = new SortedList();

                    // TODO compare the table definitions - they must be identical for the type, size, primary keys, etc...
                    if (targetTable.Definition.Columns.Count != updatedTable.Definition.Columns.Count)
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Different numbers of columns for {0}.", targetTable.Name));
                    }

                    // index the target rows
                    foreach (Row row in targetTable.Rows)
                    {
                        string primaryKey = row.GetPrimaryKey('/');

                        if (null != primaryKey)
                        {
                            targetPrimaryKeys.Add(primaryKey, row);
                        }
                        else // use the string representation of the row as its primary key (it may not be unique)
                        {
                            // this is provided for compatibility with unreal tables with no primary key
                            // all real tables must specify at least one column as the primary key
                            targetPrimaryKeys[row.ToString()] = row;
                        }

                        if ("Property" == targetTable.Name)
                        {
                            if ("ProductCode" == (string)row[0])
                            {
                                targetProductCode = (string)row[1];
                            }
                            else if ("ProductVersion" == (string)row[0])
                            {
                                targetProductVersion = (string)row[1];
                            }
                            else if ("UpgradeCode" == (string)row[0])
                            {
                                targetUpgradeCode = (string)row[1];
                            }
                        }
                    }

                    // index the updated rows
                    foreach (Row row in updatedTable.Rows)
                    {
                        string primaryKey = row.GetPrimaryKey('/');

                        if (null != primaryKey)
                        {
                            updatedPrimaryKeys.Add(primaryKey, row);
                        }
                        else // use the string representation of the row as its primary key (it may not be unique)
                        {
                            // this is provided for compatibility with unreal tables with no primary key
                            // all real tables must specify at least one column as the primary key
                            updatedPrimaryKeys[row.ToString()] = row;
                        }

                        if ("Property" == targetTable.Name)
                        {
                            if ("ProductCode" == (string)row[0])
                            {
                                updatedProductCode = (string)row[1];
                            }
                            else if ("ProductVersion" == (string)row[0])
                            {
                                updatedProductVersion = (string)row[1];
                            }
                        }
                    }

                    // diff the target and updated rows
                    foreach (DictionaryEntry targetPrimaryKeyEntry in targetPrimaryKeys)
                    {
                        string targetPrimaryKey = (string)targetPrimaryKeyEntry.Key;
                        Row targetRow = (Row)targetPrimaryKeyEntry.Value;
                        Row updatedRow = (Row)updatedPrimaryKeys[targetPrimaryKey];

                        if (null == updatedRow) // deleted row
                        {
                            Table modifiedTable = transform.EnsureTable(targetTable.Definition);
                            targetRow.Operation = RowOperation.Delete;
                            targetRow.SectionId = targetRow.SectionId + sectionDelimiter;
                            modifiedTable.Rows.Add(targetRow);
                        }
                        else // possibly modified
                        {
                            updatedRow.Operation = RowOperation.None;
                            if (!this.suppressKeepingSpecialRows && "_SummaryInformation" == targetTable.Name)
                            {
                                Table table = transform.EnsureTable(updatedTable.Definition);
                                updatedRow.SectionId = targetRow.SectionId + sectionDelimiter + updatedRow.SectionId;
                                table.Rows.Add(updatedRow);
                            }
                            else
                            {
                                bool keepRow = false;

                                if (this.preserveUnchangedRows)
                                {
                                    keepRow = true;
                                }

                                for (int i = 0; i < updatedRow.Fields.Length; i++)
                                {
                                    ColumnDefinition columnDefinition = updatedRow.Fields[i].Column;

                                    if (!columnDefinition.IsPrimaryKey)
                                    {
                                        bool modified = false;

                                        if (ColumnType.Number == columnDefinition.Type && !columnDefinition.IsLocalizable)
                                        {
                                            if (null == targetRow[i] ^ null == updatedRow[i])
                                            {
                                                modified = true;
                                            }
                                            else if (null != targetRow[i] && null != updatedRow[i])
                                            {
                                                modified = ((int)targetRow[i] != (int)updatedRow[i]);
                                            }
                                        }
                                        else if (ColumnType.Object == columnDefinition.Type)
                                        {
                                            ObjectField targetObjectField = (ObjectField)targetRow.Fields[i];
                                            ObjectField updatedObjectField = (ObjectField)updatedRow.Fields[i];

                                            if (null != targetObjectField.CabinetFileId)
                                            {
                                                // TODO: handle this
                                            }

                                            if ((string)targetObjectField.Data != (string)updatedObjectField.Data)
                                            {
                                                updatedObjectField.PreviousData = (string)targetObjectField.Data;
                                            }

                                            // keep rows containing object fields so the files can be compared in the binder
                                            keepRow = !this.suppressKeepingSpecialRows;
                                        }
                                        else
                                        {
                                            modified = ((string)targetRow[i] != (string)updatedRow[i]);
                                        }

                                        if (modified)
                                        {
                                            updatedRow.Fields[i].Modified = true;
                                            updatedRow.Operation = RowOperation.Modify;
                                            keepRow = true;
                                        }
                                    }
                                }

                                if (keepRow)
                                {
                                    Table modifiedTable = transform.EnsureTable(updatedTable.Definition);
                                    updatedRow.SectionId = targetRow.SectionId + sectionDelimiter + updatedRow.SectionId;
                                    modifiedTable.Rows.Add(updatedRow);
                                }
                            }
                        }
                    }

                    // find the inserted rows
                    foreach (DictionaryEntry updatedPrimaryKeyEntry in updatedPrimaryKeys)
                    {
                        string updatedPrimaryKey = (string)updatedPrimaryKeyEntry.Key;

                        if (!targetPrimaryKeys.Contains(updatedPrimaryKey))
                        {
                            Row updatedRow = (Row)updatedPrimaryKeyEntry.Value;

                            Table modifiedTable = transform.EnsureTable(updatedTable.Definition);
                            updatedRow.Operation = RowOperation.Add;
                            updatedRow.SectionId = sectionDelimiter + updatedRow.SectionId;
                            modifiedTable.Rows.Add(updatedRow);
                        }
                    }
                }
            }

            // added tables
            foreach (Table updatedTable in updatedOutput.Tables)
            {
                if (null == targetOutput.Tables[updatedTable.Name])
                {
                    Table addedTable = transform.Tables.EnsureTable(null, updatedTable.Definition);
                    addedTable.Operation = TableOperation.Add;

                    foreach (Row updatedRow in updatedTable.Rows)
                    {
                        updatedRow.Operation = RowOperation.Add;
                        updatedRow.SectionId = sectionDelimiter + updatedRow.SectionId;
                        addedTable.Rows.Add(updatedRow);
                    }
                }
            }

            // create the PID_REVNUMBER summary information property
            if (!this.suppressKeepingSpecialRows)
            {
                foreach (Row row in transform.Tables["_SummaryInformation"].Rows)
                {
                    if (9 == (int)row[0])
                    {
                        row[1] = String.Concat(targetProductCode, targetProductVersion, ';', updatedProductCode, updatedProductVersion, ';', targetUpgradeCode);
                    }
                }
            }

            return transform;
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
    }
}
