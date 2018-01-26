//-------------------------------------------------------------------------------------------------
// <copyright file="ColumnDefinitionCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A collection of definitions.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;

    /// <summary>
    /// A collection of definitions.
    /// </summary>
    public sealed class ColumnDefinitionCollection : ICollection
    {
        private ArrayList collection;
        private Hashtable indexHashtable;
        private Hashtable nameHashtable;

        /// <summary>
        /// Instantiate a new ColumnDefinitionCollection class.
        /// </summary>
        public ColumnDefinitionCollection()
        {
            this.collection = new ArrayList();
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        /// <value>Number of items in collection.</value>
        public int Count
        {
            get { return this.collection.Count; }
        }

        /// <summary>
        /// Gets if the collection has been synchronized.
        /// </summary>
        /// <value>True if the collection has been synchronized.</value>
        public bool IsSynchronized
        {
            get { return this.collection.IsSynchronized; }
        }

        /// <summary>
        /// Gets the object used to synchronize the collection.
        /// </summary>
        /// <value>Oject used the synchronize the collection.</value>
        public object SyncRoot
        {
            get { return this.collection.SyncRoot; }
        }

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
        /// Gets a column definition by name.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>Column definition for the named column.</returns>
        public ColumnDefinition this[string columnName]
        {
            get
            {
                if (null == this.nameHashtable)
                {
                    this.nameHashtable = new Hashtable();

                    foreach (ColumnDefinition columnDefinition in this.collection)
                    {
                        this.nameHashtable.Add(columnDefinition.Name, columnDefinition);
                    }
                }

                return (ColumnDefinition)this.nameHashtable[columnName];
            }
        }

        /// <summary>
        /// Adds a column definition to the collection.
        /// </summary>
        /// <param name="columnDefinition">Column definition to add to array.</param>
        public void Add(ColumnDefinition columnDefinition)
        {
            this.collection.Add(columnDefinition);
        }

        /// <summary>
        /// Copies the collection into an array.
        /// </summary>
        /// <param name="array">Array to copy the collection into.</param>
        /// <param name="index">Index to start copying from.</param>
        public void CopyTo(System.Array array, int index)
        {
            this.collection.CopyTo(array, index);
        }

        /// <summary>
        /// Gets enumerator for the collection.
        /// </summary>
        /// <returns>Enumerator for the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        /// <summary>
        /// Returns the zero-based index of the named column.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The zero-based index of the named column.</returns>
        public int IndexOf(string columnName)
        {
            if (null == this.indexHashtable)
            {
                this.indexHashtable = new Hashtable();

                for (int i = 0; i < this.collection.Count; i++)
                {
                    this.indexHashtable.Add(((ColumnDefinition)this.collection[i]).Name, i);
                }
            }

            return (int)this.indexHashtable[columnName];
        }
    }
}
