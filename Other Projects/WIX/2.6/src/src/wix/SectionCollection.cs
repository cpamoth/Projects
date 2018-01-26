//-------------------------------------------------------------------------------------------------
// <copyright file="SectionCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Array collection of sections.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Array collection of sections.
    /// </summary>
    public class SectionCollection : ArrayCollectionBase
    {
        /// <summary>
        /// Adds a section to the collection.
        /// </summary>
        /// <param name="section">Section to add to collection.</param>
        public void Add(Section section)
        {
            this.collection.Add(section);
        }
    }
}
