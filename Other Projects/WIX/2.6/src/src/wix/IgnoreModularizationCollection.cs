//-------------------------------------------------------------------------------------------------
// <copyright file="IgnoreModularizationCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//     Hashtable collection of identifiers to ignore when modularizing.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Hashtable collection of identifiers to ignore when modularizing.
    /// </summary>
    public class IgnoreModularizationCollection : HashCollectionBase
    {
        /// <summary>
        /// Adds an identifier to collection to ignore.
        /// </summary>
        /// <param name="ignoreModularization">Item to ignore during modularization.</param>
        public void Add(IgnoreModularization ignoreModularization)
        {
            if (null == ignoreModularization)
            {
                throw new ArgumentNullException("ignoreModularization");
            }

            collection.Add(ignoreModularization.Name, ignoreModularization);
        }

        /// <summary>
        /// Adds a collection of items to ignore during modularization to this collection.
        /// </summary>
        /// <param name="ignoreModularizations">Collection to add to this collection.</param>
        public void Add(IgnoreModularizationCollection ignoreModularizations)
        {
            foreach (IgnoreModularization ignoreModularization in ignoreModularizations)
            {
                this.Add(ignoreModularization);
            }
        }

        /// <summary>
        /// Helper function that specifies when to ignore modularization.
        /// </summary>
        /// <param name="identifier">Identifier to check if it should be ignored.</param>
        /// <returns>true if identifier should not be modularized.</returns>
        internal bool ShouldIgnoreModularization(string identifier)
        {
            return collection.ContainsKey(identifier);
        }
    }
}
