//-------------------------------------------------------------------------------------------------
// <copyright file="ColumnDefinitionCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Array collection of definitions.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Array collection of definitions.
    /// </summary>
    public class ColumnDefinitionCollection : ArrayCollectionBase
    {
        /// <summary>
        /// Gets a column definition by index.
        /// </summary>
        /// <param name="index">Index into array.</param>
        /// <value>Column definition at index location.</value>
        public ColumnDefinition this[int index]
        {
            get { return (ColumnDefinition)this.collection[index]; }
        }

        /// <summary>
        /// Adds a column definition to the collection.
        /// </summary>
        /// <param name="columnDefinition">Column definition to add to array.</param>
        public void Add(ColumnDefinition columnDefinition)
        {
            this.collection.Add(columnDefinition);
        }
    }
}
