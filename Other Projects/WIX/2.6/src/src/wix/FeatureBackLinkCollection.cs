//-------------------------------------------------------------------------------------------------
// <copyright file="FeatureBacklinkCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Array based collection of feature backlinks.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Array based collection of feature backlinks.
    /// </summary>
    public sealed class FeatureBacklinkCollection : ArrayCollectionBase
    {
        /// <summary>
        /// Adds a backlink to the collection.
        /// </summary>
        /// <param name="backlink">Backlink to add to the collection.</param>
        public void Add(FeatureBacklink backlink)
        {
            this.collection.Add(backlink);
        }
    }
}
