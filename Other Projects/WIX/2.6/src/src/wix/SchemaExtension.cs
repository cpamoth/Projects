//-------------------------------------------------------------------------------------------------
// <copyright file="SchemaExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base schema extension.  Any of these methods can be overridden to extend
// the wix schema processing.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Xml.Schema;

    /// <summary>
    /// Base class for creating a schema extension.
    /// </summary>
    public abstract class SchemaExtension
    {
        protected XmlSchema xmlSchema;
        protected TableDefinitionCollection tableDefinitionCollection;
        private ExtensionMessages messages;

        /// <summary>
        /// Gets and sets the object for message handling.
        /// </summary>
        /// <value>Wrapper object for sending messages.</value>
        public ExtensionMessages Messages
        {
            get { return this.messages; }
            set { this.messages = value; }
        }

        /// <summary>
        /// Gets the schema for this schema extension.
        /// </summary>
        /// <value>Schema for this schema extension.</value>
        public XmlSchema Schema
        {
            get { return this.xmlSchema; }
        }

        /// <summary>
        /// Gets the table definitions for this schema extension.
        /// </summary>
        /// <value>Table definitions for this schema extension.</value>
        public TableDefinitionCollection TableDefinitions
        {
            get { return this.tableDefinitionCollection; }
        }
    }
}