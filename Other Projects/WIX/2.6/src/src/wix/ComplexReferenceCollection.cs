//-------------------------------------------------------------------------------------------------
// <copyright file="ComplexReferenceCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Array collection of complex references.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Array collection of complex references.
    /// </summary>
    internal class ComplexReferenceCollection : ArrayCollectionBase
    {
        /// <summary>
        /// Adds a complex reference to the collection.
        /// </summary>
        /// <param name="complexRef">Complex reference to add to the collection.</param>
        public void Add(ComplexReference complexRef)
        {
            this.collection.Add(complexRef);
        }
    }
}
