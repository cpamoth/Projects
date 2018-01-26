//-------------------------------------------------------------------------------------------------
// <copyright file="OutputTableCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//     Hash table collection of output tables.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Hash table collection of output tables.
    /// </summary>
    public class OutputTableCollection : HashCollectionBase
    {
        /// <summary>
        /// Gets an output table by name.
        /// </summary>
        /// <param name="tableName">Table name to find.</param>
        public OutputTable this[string tableName]
        {
            get { return (OutputTable)this.collection[tableName]; }
        }

        /// <summary>
        /// Adds an output table to the collection
        /// </summary>
        /// <param name="table">Table to add.</param>
        public void Add(OutputTable table)
        {
            if (null == table)
            {
                throw new ArgumentNullException("table");
            }

            this.collection.Add(table.Name, table);
        }
    }
}
