//-------------------------------------------------------------------------------------------------
// <copyright file="IntermediateCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Container class for a set of intermediates.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Text;

    /// <summary>
    /// Collection of intermediates.
    /// </summary>
    public class IntermediateCollection : ArrayCollectionBase
    {
        /// <summary>
        /// Add a new intermediate to the collection.
        /// </summary>
        /// <param name="intermediate">Intermediate to add to the collection.</param>
        public void Add(Intermediate intermediate)
        {
            collection.Add(intermediate);
        }
    }
}

