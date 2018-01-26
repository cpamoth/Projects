//-------------------------------------------------------------------------------------------------
// <copyright file="ImportStreamCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Hash table collection of import streams.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Hash table collection of import streams.
    /// </summary>
    public class ImportStreamCollection : HashCollectionBase
    {
        /// <summary>
        /// Creates a new collection.
        /// </summary>
        public ImportStreamCollection()
        {
        }

        /// <summary>
        /// Gets an import stream by name.
        /// </summary>
        /// <param name="importStreamName">Name of stream to get.</param>
        public ImportStream this[string importStreamName]
        {
            get { return (ImportStream)this.collection[importStreamName]; }
        }

        /// <summary>
        /// Adds an import stream to the collection.
        /// </summary>
        /// <param name="importStream">Import stream to add to collection.</param>
        /// <remarks>Indexes collection by import stream name.</remarks>
        public void Add(ImportStream importStream)
        {
            if (null == importStream)
            {
                throw new ArgumentNullException("importStream");
            }

            this.collection.Add(importStream.Name, importStream);
        }
    }
}
