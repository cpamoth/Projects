//-------------------------------------------------------------------------------------------------
// <copyright file="ConnectToFeatureCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Hash collection of connect to feature objects.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Hash collection of connect to feature objects.
    /// </summary>
    public class ConnectToFeatureCollection : HashCollectionBase
    {
        /// <summary>
        /// Gets a feature connection by child id.
        /// </summary>
        /// <param name="childId">Identifier of child to locate.</param>
        public ConnectToFeature this[string childId]
        {
            get { return (ConnectToFeature)this.collection[childId]; }
        }

        /// <summary>
        /// Adds a feature connection to the collection.
        /// </summary>
        /// <param name="connection">Feature connection to add.</param>
        public void Add(ConnectToFeature connection)
        {
            if (null == connection)
            {
                throw new ArgumentNullException("connection");
            }

            this.collection.Add(connection.ChildId, connection);
        }
    }
}
