//-------------------------------------------------------------------------------------------------
// <copyright file="WixDuplicatePrimaryKeyException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX duplicate primary key exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;

    /// <summary>
    /// WiX duplicate primary key exception.
    /// </summary>
    public class WixDuplicatePrimaryKeyException : WixException
    {
        private string tableName;
        private ArrayList keyValues;
        private ArrayList columnNames;
        private string detail;

        /// <summary>
        /// Instantiate a new WixDuplicatePrimaryKeyException.
        /// </summary>
        /// <param name="tableName">Name of the table with duplicate primary keys.</param>
        /// <param name="keyValues">List of key values that are duplicated.</param>
        /// <param name="columnNames">List of columns with duplicated values.</param>
        public WixDuplicatePrimaryKeyException(string tableName, ArrayList keyValues, ArrayList columnNames) :
            this(null, tableName, keyValues, columnNames, null, null)
        {
        }

        /// <summary>
        /// Instantiate a new WixDuplicatePrimaryKeyException.
        /// </summary>
        /// <param name="tableName">Name of the table with duplicate primary keys.</param>
        /// <param name="keyValues">List of key values that are duplicated.</param>
        /// <param name="columnNames">List of columns with duplicated values.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WixDuplicatePrimaryKeyException(string tableName, ArrayList keyValues, ArrayList columnNames, Exception innerException) :
            this(null, tableName, keyValues, columnNames, null, innerException)
        {
        }

        /// <summary>
        /// Instantiate a new WixDuplicatePrimaryKeyException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="tableName">Name of the table with duplicate primary keys.</param>
        /// <param name="keyValues">List of key values that are duplicated.</param>
        /// <param name="columnNames">List of columns with duplicated values.</param>
        /// <param name="detail">Detail information about the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public WixDuplicatePrimaryKeyException(SourceLineNumberCollection sourceLineNumbers, string tableName, ArrayList keyValues, ArrayList columnNames, string detail, Exception innerException) :
            base(sourceLineNumbers, WixExceptionType.DuplicateSymbol, innerException)
        {
            this.tableName = tableName;
            this.keyValues = keyValues;
            this.columnNames = columnNames;
            this.detail = detail;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                string primaryKeys = String.Empty;
                for (int i = 0; i < this.columnNames.Count; ++i)
                {
                    if (0 == primaryKeys.Length)
                    {
                        primaryKeys = String.Concat("primary key '", (string)this.keyValues[i], "' in column '", (string)this.columnNames[i], "'");
                    }
                    else
                    {
                        primaryKeys = String.Concat(primaryKeys, " and primary key '", (string)this.keyValues[i], "' in column '", (string)this.columnNames[i], "'");
                    }
                }
                if (null == this.detail)
                {
                    return String.Format("{0} are duplicated in table '{1}'", primaryKeys, this.tableName);
                }
                else
                {
                    return String.Format("{0} are duplicated in table '{1}', detail: {2}", primaryKeys, this.tableName, this.detail);
                }
            }
        }
    }
}
