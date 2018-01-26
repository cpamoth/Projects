//-------------------------------------------------------------------------------------------------
// <copyright file="RowCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Array collection of rows.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Array collection of rows.
    /// </summary>
    public class RowCollection : ArrayCollectionBase
    {
        /// <summary>
        /// Adds a row to the collection.
        /// </summary>
        /// <param name="row">Row to add to collection.</param>
        public void Add(Row row)
        {
            this.collection.Add(row);
        }
    }
}
