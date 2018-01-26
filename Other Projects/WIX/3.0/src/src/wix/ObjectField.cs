//-------------------------------------------------------------------------------------------------
// <copyright file="ObjectField.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Field containing data for an object column in a row.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Field containing data for an object column in a row.
    /// </summary>
    public sealed class ObjectField : Field
    {
        private string baseUri;
        private string cabinetFileId;
        private string previousData;

        /// <summary>
        /// Instantiates a new Field.
        /// </summary>
        /// <param name="columnDefinition">Column definition for this field.</param>
        internal ObjectField(ColumnDefinition columnDefinition) :
            base(columnDefinition)
        {
        }

        /// <summary>
        /// Gets or sets the identifier of the file in the cabinet.
        /// </summary>
        /// <value>The identifier of the file in the cabinet.</value>
        public string CabinetFileId
        {
            get { return this.cabinetFileId; }
            set { this.cabinetFileId = value; }
        }

        /// <summary>
        /// Gets or sets the previous data.
        /// </summary>
        /// <value>The previous data.</value>
        public string PreviousData
        {
            get { return this.previousData; }
            set { this.previousData = value; }
        }

        /// <summary>
        /// Gets the base URI of the object field.
        /// </summary>
        /// <value>The base URI of the object field.</value>
        internal string BaseUri
        {
            get { return this.baseUri; }
        }

        /// <summary>
        /// Parse a field from the xml.
        /// </summary>
        /// <param name="reader">XmlReader where the intermediate is persisted.</param>
        internal override void Parse(XmlReader reader)
        {
            Debug.Assert("field" == reader.LocalName);

            bool empty = reader.IsEmptyElement;

            this.baseUri = reader.BaseURI;

            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "cabinetFileId":
                        this.cabinetFileId = reader.Value;
                        break;
                    case "modified":
                        this.Modified = Common.IsYes(SourceLineNumberCollection.FromUri(reader.BaseURI), "field", reader.Name, reader.Value);
                        break;
                    case "previousData":
                        this.previousData = reader.Value;
                        break;
                    default:
                        if (!reader.NamespaceURI.StartsWith("http://www.w3.org/"))
                        {
                            throw new WixException(WixErrors.UnexpectedAttribute(SourceLineNumberCollection.FromUri(reader.BaseURI), "field", reader.Name));
                        }
                        break;
                }
            }

            if (!empty)
            {
                bool done = false;

                while (!done && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            throw new WixException(WixErrors.UnexpectedElement(SourceLineNumberCollection.FromUri(reader.BaseURI), "field", reader.Name));
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            if (0 < reader.Value.Length)
                            {
                                this.Data = reader.Value;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            done = true;
                            break;
                    }
                }

                if (!done)
                {
                    throw new WixException(WixErrors.ExpectedEndElement(SourceLineNumberCollection.FromUri(reader.BaseURI), "field"));
                }
            }
        }

        /// <summary>
        /// Persists a field in an XML format.
        /// </summary>
        /// <param name="writer">XmlWriter where the Field should persist itself as XML.</param>
        internal override void Persist(XmlWriter writer)
        {
            string text;

            // convert the data to a string that will persist nicely
            if (null == this.Data)
            {
                text = String.Empty;
            }
            else
            {
                text = (string)this.Data;
            }

            writer.WriteStartElement("field", Intermediate.XmlNamespaceUri);

            if (null != this.cabinetFileId)
            {
                writer.WriteAttributeString("cabinetFileId", this.cabinetFileId);
            }

            if (this.Modified)
            {
                writer.WriteAttributeString("modified", "yes");
            }

            if (null != this.previousData)
            {
                writer.WriteAttributeString("previousData", this.previousData);
            }

            if (this.Column.UseCData)
            {
                writer.WriteCData(text);
            }
            else
            {
                writer.WriteString(text);
            }

            writer.WriteEndElement();
        }
    }
}
