//-------------------------------------------------------------------------------------------------
// <copyright file="Localization.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Object that represents a localization file.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Object that represents a localization file.
    /// </summary>
    public sealed class Localization
    {
        public const string XmlNamespaceUri = "http://schemas.microsoft.com/wix/2006/localization";
        private static XmlSchemaCollection schemas;

        private int codepage;
        private string culture;
        private Hashtable variables;

        /// <summary>
        /// Instantiate a new Localization.
        /// </summary>
        /// <param name="codepage">The codepage of the localization.</param>
        /// <param name="culture">The culture of the localization.</param>
        private Localization(int codepage, string culture)
        {
            this.codepage = codepage;
            this.culture = culture.ToLower(CultureInfo.InvariantCulture);
            this.variables = new Hashtable();
        }

        /// <summary>
        /// Gets the codepage.
        /// </summary>
        /// <value>The codepage.</value>
        public int Codepage
        {
            get { return this.codepage; }
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        /// <value>The culture.</value>
        public string Culture
        {
            get { return this.culture; }
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public ICollection Variables
        {
            get { return this.variables.Values; }
        }

        /// <summary>
        /// Loads a localization file from a path on disk.
        /// </summary>
        /// <param name="path">Path to library file saved on disk.</param>
        /// <param name="tableDefinitions">Collection containing TableDefinitions to use when loading the localization file.</param>
        /// <param name="suppressSchema">Suppress xml schema validation while loading.</param>
        /// <returns>Returns the loaded localization file.</returns>
        public static Localization Load(string path, TableDefinitionCollection tableDefinitions, bool suppressSchema)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                return Load(stream, new Uri(Path.GetFullPath(path)), tableDefinitions, suppressSchema);
            }
        }

        /// <summary>
        /// Persists a localization file into an XML format.
        /// </summary>
        /// <param name="writer">XmlWriter where the localization file should persist itself as XML.</param>
        public void Persist(XmlWriter writer)
        {
            writer.WriteStartElement("WixLocalization", XmlNamespaceUri);

            if (-1 != this.codepage)
            {
                writer.WriteAttributeString("Codepage", this.codepage.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteAttributeString("Culture", this.culture);

            foreach (WixVariableRow wixVariableRow in this.variables.Values)
            {
                writer.WriteStartElement("String", XmlNamespaceUri);

                writer.WriteAttributeString("Id", wixVariableRow.Id);

                if (wixVariableRow.Overridable)
                {
                    writer.WriteAttributeString("Overridable", "yes");
                }

                writer.WriteCData(wixVariableRow.Value);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads a localization file from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the localization file.</param>
        /// <param name="uri">Uri for finding this stream.</param>
        /// <param name="tableDefinitions">Collection containing TableDefinitions to use when loading the localization file.</param>
        /// <param name="suppressSchema">Suppress xml schema validation while loading.</param>
        /// <returns>Returns the loaded localization file.</returns>
        internal static Localization Load(Stream stream, Uri uri, TableDefinitionCollection tableDefinitions, bool suppressSchema)
        {
            XmlReader reader = null;

            try
            {
                reader = new XmlTextReader(uri.AbsoluteUri, stream);

                if (!suppressSchema)
                {
                    reader = new XmlValidatingReader(reader);
                    ((XmlValidatingReader)reader).Schemas.Add(GetSchemas());
                }

                reader.MoveToContent();

                if ("WixLocalization" != reader.LocalName)
                {
                    throw new WixNotIntermediateException(WixErrors.InvalidDocumentElement(SourceLineNumberCollection.FromUri(reader.BaseURI), reader.Name, "localization", "WixLocalization"));
                }

                return Parse(reader, tableDefinitions);
            }
            catch (XmlException xe)
            {
                throw new WixException(WixErrors.InvalidXml(SourceLineNumberCollection.FromUri(reader.BaseURI), "localization", xe.Message));
            }
            catch (XmlSchemaException xse)
            {
                throw new WixException(WixErrors.SchemaValidationFailed(SourceLineNumberCollection.FromUri(reader.BaseURI), xse.Message));
            }
            finally
            {
                if (null != reader)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Get the schemas required to validate a library.
        /// </summary>
        /// <returns>The schemas required to validate a library.</returns>
        internal static XmlSchemaCollection GetSchemas()
        {
            if (null == schemas)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                using (Stream localizationSchemaStream = assembly.GetManifestResourceStream("Microsoft.Tools.WindowsInstallerXml.Xsd.wixloc.xsd"))
                {
                    schemas = new XmlSchemaCollection();
                    XmlSchema localizationSchema = XmlSchema.Read(localizationSchemaStream, null);
                    schemas.Add(localizationSchema);
                }
            }

            return schemas;
        }

        /// <summary>
        /// Parse a localization file from an XML format.
        /// </summary>
        /// <param name="reader">XmlReader where the localization file is persisted.</param>
        /// <param name="tableDefinitions">Collection containing TableDefinitions to use when parsing the localization file.</param>
        /// <returns>The parsed localization.</returns>
        internal static Localization Parse(XmlReader reader, TableDefinitionCollection tableDefinitions)
        {
            Debug.Assert("WixLocalization" == reader.LocalName);

            bool empty = reader.IsEmptyElement;
            int codepage = -1;
            string culture = null;

            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "Codepage":
                        try
                        {
                            codepage = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture.NumberFormat);
                        }
                        catch (FormatException)
                        {
                            throw new WixException(WixErrors.IllegalIntegerValue(SourceLineNumberCollection.FromUri(reader.BaseURI), "WixLocalization", reader.Name, reader.Value));
                        }
                        catch (OverflowException)
                        {
                            throw new WixException(WixErrors.IllegalIntegerValue(SourceLineNumberCollection.FromUri(reader.BaseURI), "WixLocalization", reader.Name, reader.Value));
                        }
                        break;
                    case "Culture":
                        culture = reader.Value;
                        break;
                    default:
                        if (!reader.NamespaceURI.StartsWith("http://www.w3.org/"))
                        {
                            throw new WixException(WixErrors.UnexpectedAttribute(SourceLineNumberCollection.FromUri(reader.BaseURI), "WixLocalization", reader.Name));
                        }
                        break;
                }
            }

            if (null == culture)
            {
                throw new WixException(WixErrors.ExpectedAttribute(SourceLineNumberCollection.FromUri(reader.BaseURI), "WixLocalization", "Culture"));
            }

            Localization localization = new Localization(codepage, culture);

            if (!empty)
            {
                bool done = false;

                while (!done && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.LocalName)
                            {
                                case "String":
                                    ParseString(reader, tableDefinitions, localization);
                                    break;
                                default:
                                    throw new WixException(WixErrors.UnexpectedElement(SourceLineNumberCollection.FromUri(reader.BaseURI), "WixLocalization", reader.Name));
                            }
                            break;
                        case XmlNodeType.EndElement:
                            done = true;
                            break;
                    }
                }

                if (!done)
                {
                    throw new WixException(WixErrors.ExpectedEndElement(SourceLineNumberCollection.FromUri(reader.BaseURI), "WixLocalization"));
                }
            }

            return localization;
        }

        /// <summary>
        /// Parse a localization string.
        /// </summary>
        /// <param name="reader">XmlReader where the localization file is persisted.</param>
        /// <param name="tableDefinitions">Collection containing TableDefinitions to use when loading the localization file.</param>
        /// <param name="localization">The localization being parsed.</param>
        private static void ParseString(XmlReader reader, TableDefinitionCollection tableDefinitions, Localization localization)
        {
            Debug.Assert("String" == reader.LocalName);

            bool empty = reader.IsEmptyElement;
            SourceLineNumberCollection sourceLineNumbers = SourceLineNumberCollection.FromUri(reader.BaseURI);

            string id = null;
            bool overridable = false;
            string value = String.Empty; // default this value to the empty string

            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "Id":
                        id = reader.Value;
                        break;
                    case "Overridable":
                        overridable = Common.IsYes(sourceLineNumbers, "String", reader.Name, reader.Value);
                        break;
                    default:
                        if (!reader.NamespaceURI.StartsWith("http://www.w3.org/"))
                        {
                            throw new WixException(WixErrors.UnexpectedAttribute(sourceLineNumbers, "String", reader.Name));
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
                            throw new WixException(WixErrors.UnexpectedElement(sourceLineNumbers, "String", reader.Name));
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                            if (0 < reader.Value.Length)
                            {
                                value = reader.Value;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            done = true;
                            break;
                    }
                }

                if (!done)
                {
                    throw new WixException(WixErrors.ExpectedEndElement(sourceLineNumbers, "String"));
                }
            }

            if (null == id)
            {
                throw new WixException(WixErrors.ExpectedAttribute(sourceLineNumbers, "String", "Id"));
            }

            WixVariableRow wixVariableRow = new WixVariableRow(sourceLineNumbers, tableDefinitions["WixVariable"]);
            wixVariableRow.Id = id;
            wixVariableRow.Overridable = overridable;
            wixVariableRow.Value = value;

            WixVariableRow existingWixVariableRow = (WixVariableRow)localization.variables[id];
            if (null == existingWixVariableRow || (existingWixVariableRow.Overridable && !overridable))
            {
                localization.variables.Add(id, wixVariableRow);
            }
            else if (!overridable)
            {
                throw new WixException(WixErrors.DuplicateLocalizationIdentifier(sourceLineNumbers, id));
            }
        }
    }
}