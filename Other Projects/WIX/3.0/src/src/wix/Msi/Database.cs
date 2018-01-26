//-------------------------------------------------------------------------------------------------
// <copyright file="Database.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Wrapper for MSI API database functions.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Msi
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Enum of predefined persist modes used when opening a database.
    /// </summary>
    public enum OpenDatabase
    {
        /// <summary>
        /// Open a database read-only, no persistent changes.
        /// </summary>
        ReadOnly = MsiInterop.MSIDBOPENREADONLY,

        /// <summary>
        /// Open a database read/write in transaction mode.
        /// </summary>
        Transact = MsiInterop.MSIDBOPENTRANSACT,

        /// <summary>
        /// Open a database direct read/write without transaction.
        /// </summary>
        Direct = MsiInterop.MSIDBOPENDIRECT,

        /// <summary>
        /// Create a new database, transact mode read/write.
        /// </summary>
        Create = MsiInterop.MSIDBOPENCREATE,

        /// <summary>
        /// Create a new database, direct mode read/write.
        /// </summary>
        CreateDirect = MsiInterop.MSIDBOPENCREATEDIRECT,

        /// <summary>
        /// Indicates a patch file is being opened.
        /// </summary>
        OpenPatchFile = MsiInterop.MSIDBOPENPATCHFILE
    }

    /// <summary>
    /// The errors to suppress when applying a transform.
    /// </summary>
    [Flags]
    public enum TransformErrorConditions
    {
        /// <summary>
        /// None of the following conditions.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Suppress error when adding a row that exists.
        /// </summary>
        AddExistingRow = 0x1,

        /// <summary>
        /// Suppress error when deleting a row that does not exist.
        /// </summary>
        DeleteMissingRow = 0x2,

        /// <summary>
        /// Suppress error when adding a table that exists.
        /// </summary>
        AddExistingTable = 0x4,

        /// <summary>
        /// Suppress error when deleting a table that does not exist.
        /// </summary>
        DeleteMissingTable = 0x8,

        /// <summary>
        /// Suppress error when updating a row that does not exist.
        /// </summary>
        UpdateMissingRow = 0x10,

        /// <summary>
        /// Suppress error when transform and database code pages do not match, and their code pages are neutral.
        /// </summary>
        ChangeCodepage = 0x20,

        /// <summary>
        /// Create the temporary _TransformView table when applying a transform.
        /// </summary>
        ViewTransform = 0x100,

        /// <summary>
        /// Suppress all errors but the option to create the temporary _TransformView table.
        /// </summary>
        All = 0x3F
    }

    /// <summary>
    /// The validation to run while applying a transform.
    /// </summary>
    [Flags]
    public enum TransformValidations
    {
        /// <summary>
        /// Do not validate properties.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Default language must match base database.
        /// </summary>
        Language = 0x1,

        /// <summary>
        /// Product must match base database.
        /// </summary>
        Product = 0x2,

        /// <summary>
        /// Check major version only.
        /// </summary>
        MajorVersion = 0x8,

        /// <summary>
        /// Check major and minor versions only.
        /// </summary>
        MinorVersion = 0x10,

        /// <summary>
        /// Check major, minor, and update versions.
        /// </summary>
        UpdateVersion = 0x20,

        /// <summary>
        /// Installed version &lt; base version.
        /// </summary>
        NewLessBaseVersion = 0x40,

        /// <summary>
        /// Installed version &lt;= base version.
        /// </summary>
        NewLessEqualBaseVersion = 0x80,

        /// <summary>
        /// Installed version = base version.
        /// </summary>
        NewEqualBaseVersion = 0x100,

        /// <summary>
        /// Installed version &gt;= base version.
        /// </summary>
        NewGreaterEqualBaseVersion = 0x200,

        /// <summary>
        /// Installed version &gt; base version.
        /// </summary>
        NewGreaterBaseVersion = 0x400,

        /// <summary>
        /// UpgradeCode must match base database.
        /// </summary>
        UpgradeCode = 0x800
    }

    /// <summary>
    /// Wrapper class for managing MSI API database handles.
    /// </summary>
    public sealed class Database : MsiHandle
    {
        /// <summary>
        /// Constructor that opens an MSI database.
        /// </summary>
        /// <param name="path">Path to the database to be opened.</param>
        /// <param name="type">Persist mode to use when opening the database.</param>
        public Database(string path, OpenDatabase type)
        {
            uint handle = 0;
            int error = MsiInterop.MsiOpenDatabase(path, new IntPtr((int)type), out handle);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
            this.Handle = handle;
        }

        /// <summary>
        /// Applies a transform to this database.
        /// </summary>
        /// <param name="transformFile">Path to the transform file being applied.</param>
        /// <param name="errorConditions">Specifies the error conditions that are to be suppressed.</param>
        public void ApplyTransform(string transformFile, TransformErrorConditions errorConditions)
        {
            int error = MsiInterop.MsiDatabaseApplyTransform(this.Handle, transformFile, errorConditions);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Commits changes made to the database.
        /// </summary>
        public void Commit()
        {
            int error = MsiInterop.MsiDatabaseCommit(this.Handle);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Creates and populates the summary information stream of an existing transform file.
        /// </summary>
        /// <param name="referenceDatabase">Required database that does not include the changes.</param>
        /// <param name="transformFile">The name of the generated transform file.</param>
        /// <param name="errorConditions">Required error conditions that should be suppressed when the transform is applied.</param>
        /// <param name="validations">Required when the transform is applied to a database;
        /// shows which properties should be validated to verify that this transform can be applied to the database.</param>
        public void CreateTransformSummaryInfo(Database referenceDatabase, string transformFile, TransformErrorConditions errorConditions, TransformValidations validations)
        {
            int error = MsiInterop.MsiCreateTransformSummaryInfo(this.Handle, referenceDatabase.Handle, transformFile, errorConditions, validations);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Imports an installer text archive table (idt file) into an open database.
        /// </summary>
        /// <param name="folderPath">Specifies the path to the folder containing archive files.</param>
        /// <param name="fileName">Specifies the name of the file to import.</param>
        public void Import(string folderPath, string fileName)
        {
            int error = MsiInterop.MsiDatabaseImport(this.Handle, folderPath, fileName);
            if (1627 == error)
            {
                throw new Win32Exception(error, String.Format(CultureInfo.InvariantCulture, "Invalid IDT file: '{0}\\{1}'", folderPath, fileName));
            }
            else if (0 != error)
            {
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Exports an installer table from an open database to a text archive file (idt file).
        /// </summary>
        /// <param name="tableName">Specifies the name of the table to export.</param>
        /// <param name="folderPath">Specifies the name of the folder that contains archive files. If null or empty string, uses current directory.</param>
        /// <param name="fileName">Specifies the name of the exported table archive file.</param>
        public void Export(string tableName, string folderPath, string fileName)
        {
            if (null == folderPath || 0 == folderPath.Length)
            {
                folderPath = System.Environment.CurrentDirectory;
            }

            int error = MsiInterop.MsiDatabaseExport(this.Handle, tableName, folderPath, fileName);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Creates a transform that, when applied to the reference database, results in this database.
        /// </summary>
        /// <param name="referenceDatabase">Required database that does not include the changes.</param>
        /// <param name="transformFile">The name of the generated transform file. This is optional.</param>
        /// <returns>true if a transform is generated; false if a transform is not generated because
        /// there are no differences between the two databases.</returns>
        public bool GenerateTransform(Database referenceDatabase, string transformFile)
        {
            int error = MsiInterop.MsiDatabaseGenerateTransform(this.Handle, referenceDatabase.Handle, transformFile, 0, 0);
            if (0 != error && 0xE8 != error) // ERROR_NO_DATA(0xE8) means no differences were found
            {
                throw new Win32Exception(error);
            }

            return (0xE8 != error);
        }

        /// <summary>
        /// Merges two databases together.
        /// </summary>
        /// <param name="mergeDatabase">The database to merge into the base database.</param>
        /// <param name="tableName">The name of the table to receive merge conflict information.</param>
        public void Merge(Database mergeDatabase, string tableName)
        {
            int error = MsiInterop.MsiDatabaseMerge(this.Handle, mergeDatabase.Handle, tableName);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
        }

        /// <summary>
        /// Prepares a database query and creates a <see cref="View">View</see> object.
        /// </summary>
        /// <param name="query">Specifies a SQL query string for querying the database.</param>
        /// <returns>A view object is returned if the query was successful.</returns>
        public View OpenView(string query)
        {
            return new View(this, query);
        }

        /// <summary>
        /// Prepares and executes a database query and creates a <see cref="View">View</see> object.
        /// </summary>
        /// <param name="query">Specifies a SQL query string for querying the database.</param>
        /// <returns>A view object is returned if the query was successful.</returns>
        public View OpenExecuteView(string query)
        {
            View view = new View(this, query);

            view.Execute();
            return view;
        }

        /// <summary>
        /// Verifies the existence or absence of a table.
        /// </summary>
        /// <param name="tableName">Table name to to verify the existence of.</param>
        /// <returns>Returns true if the table exists, false if it does not.</returns>
        public bool TableExists(string tableName)
        {
            int result = MsiInterop.MsiDatabaseIsTablePersistent(this.Handle, tableName);
            return MsiInterop.MSICONDITIONTRUE == result;
        }

        /// <summary>
        /// Returns a <see cref="Record">Record</see> containing the names of all the primary 
        /// key columns for a specified table.
        /// </summary>
        /// <param name="tableName">Specifies the name of the table from which to obtain 
        /// primary key names.</param>
        /// <returns>Returns a <see cref="Record">Record</see> containing the names of all the 
        /// primary key columns for a specified table.</returns>
        public Record PrimaryKeys(string tableName)
        {
            uint recordHandle;
            int error = MsiInterop.MsiDatabaseGetPrimaryKeys(this.Handle, tableName, out recordHandle);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }

            return new Record(recordHandle);
        }
    }
}
