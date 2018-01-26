//-------------------------------------------------------------------------------------------------
// <copyright file="ISchemaElement.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Interface for generated schema elements.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Serialize
{
    using System;
    using System.Xml;

    /// <summary>
    /// Interface for generated schema elements.
    /// </summary>
    public interface ISchemaElement
    {
        /// <summary>
        /// Outputs xml representing this element, including the associated attributes
        /// and any nested elements.
        /// </summary>
        /// <param name="writer">XmlTextWriter to be used when outputting the element.</param>
        void OutputXml(XmlTextWriter writer);
    }
}
