//-------------------------------------------------------------------------------------------------
// <copyright file="TableCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Hash table collection for tables.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Hash table collection for tables.
    /// </summary>
    public class TableCollection : HashCollectionBase
    {
        /// <summary>
        /// Gets a table by name.
        /// </summary>
        /// <param name="tableName">Name of table to locate.</param>
        public Table this[string tableName]
        {
            get { return (Table)this.collection[tableName]; }
        }

        /// <summary>
        /// Adds a table to the collection.
        /// </summary>
        /// <param name="table">Table to add to the collection.</param>
        /// <remarks>Indexes the table by name.</remarks>
        public void Add(Table table)
        {
            if (null == table)
            {
                throw new ArgumentNullException("table");
            }

            this.collection.Add(table.Name, table);
        }
    }
}
