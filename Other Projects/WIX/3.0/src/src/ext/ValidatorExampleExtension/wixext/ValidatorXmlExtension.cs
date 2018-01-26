//-------------------------------------------------------------------------------------------------
// <copyright file="ValidatorXmlExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A simple validator extension to output XML.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// The example validator extension.
    /// </summary>
    /// <remarks>
    /// <para>This example extension writes an XML using an
    /// <see cref="XmlWriter"/> to avoid the overhead of reflection and
    /// XML serialization.</para>
    /// </remarks>
    public sealed class ValidatorXmlExtension : ValidatorExtension
    {
        private readonly string[] elements;
        private XmlWriter writer;

        /// <summary>
        /// Creates an instance of the <see cref="ValidatorXmlExtension"/> class.
        /// </summary>
        public ValidatorXmlExtension()
        {
            elements = new string[] {
                "ICE",
                "Type",
                "Description",
                "URL",
                "Table",
                "Column",
                "Key"
            };
        }

        /// <summary>
        /// Initialize the extension.
        /// </summary>
        public override void InitializeValidator()
        {
            base.InitializeValidator();

            // Computer the XML output file name based on the database file
            // and open the file for writing XML.
            string filename = Path.ChangeExtension(this.DatabaseFile, ".xml");
            this.writer = new XmlTextWriter(filename, Encoding.UTF8);

            // Write the declaration and root element.
            this.writer.WriteStartDocument(true);
            this.writer.WriteStartElement("Validation");
        }

        /// <summary>
        /// Finalizes the extension.
        /// </summary>
        public override void FinalizeValidator()
        {
            if (null != this.writer)
            {
                // Write any open elements and close the file.
                this.writer.WriteEndDocument();
                this.writer.Close();

                this.writer = null;
            }

            base.FinalizeValidator();
        }

        /// <summary>
        /// Logs the messages for ICE errors, warnings, and information.
        /// </summary>
        /// <param name="message">The entire string sent from the validator.</param>
        public override void Log(string message)
        {
            if (null == message) return;

            // Open the message element.
            this.writer.WriteStartElement("Message");

            // Write elements for each message part.
            string[] messageParts = message.Split('\t');
            for (int i = 0; i < messageParts.Length; i++)
            {
                this.writer.WriteElementString(
                    elements[Math.Min(i, elements.Length - 1)],
                    messageParts[i]);
            }

            // Close the element.
            this.writer.WriteEndElement();
        }
    }
}
