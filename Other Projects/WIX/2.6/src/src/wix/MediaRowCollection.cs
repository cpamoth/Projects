//-------------------------------------------------------------------------------------------------
// <copyright file="MediaRowCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Hash table collection of specialized media rows.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Hash table collection of specialized media rows.
    /// </summary>
    public class MediaRowCollection : HashCollectionBase
    {
        /// <summary>
        /// Creates a new collection.
        /// </summary>
        public MediaRowCollection()
        {
        }

        /// <summary>
        /// Gets a media row by disk id.
        /// </summary>
        /// <param name="diskId">Disk identifier of media row to locate.</param>
        public MediaRow this[int diskId]
        {
            get { return (MediaRow)this.collection[diskId]; }
        }

        /// <summary>
        /// Adds a media row to the collection.
        /// </summary>
        /// <param name="row">Row to add to the colleciton.</param>
        /// <remarks>Indexes the row by disk id.</remarks>
        public void Add(MediaRow row)
        {
            if (null == row)
            {
                throw new ArgumentNullException("row");
            }

            this.collection.Add(row.DiskId, row);
        }
    }
}
