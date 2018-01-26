//-------------------------------------------------------------------------------------------------
// <copyright file="Compiler.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Compiler core of the Windows Installer Xml toolset.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;

    using Microsoft.Tools.WindowsInstallerXml.Msi;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Compiler core of the Windows Installer Xml toolset.
    /// </summary>
    public sealed class Compiler
    {
        private TableDefinitionCollection tableDefinitions;
        private Hashtable extensions;
        private CompilerCore core;
        private XmlSchema schema;
        private XmlSchemaCollection schemas;
        private bool showPedanticMessages;
        private bool suppressValidation;

        // if these are true you know you are building a module or product
        // but if they are false you cannot not be sure they will not end
        // up a product or module.  Use these flags carefully.
        private bool compilingModule;
        private bool compilingProduct;

        private bool useShortFileNames;
        private string activeName;
        private string activeLanguage;

        /// <summary>
        /// Creates a new compiler object with a default set of table definitions.
        /// </summary>
        public Compiler()
        {
            this.tableDefinitions = Installer.GetTableDefinitions();
            this.extensions = new Hashtable();

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Tools.WindowsInstallerXml.Xsd.wix.xsd"))
            {
                this.schema = XmlSchema.Read(resourceStream, null);
            }
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Type of RadioButton element in a group.
        /// </summary>
        private enum RadioButtonType
        {
            /// <summary>Not set, yet.</summary>
            NotSet,

            /// <summary>Text</summary>
            Text,

            /// <summary>Bitmap</summary>
            Bitmap,

            /// <summary>Icon</summary>
            Icon,
        }

        /// <summary>
        /// Gets and sets if the compiler uses short names when processing MSI file names.
        /// </summary>
        /// <value>true if using short names for files in MSI format.</value>
        public bool ShortNames
        {
            get { return this.useShortFileNames; }
            set { this.useShortFileNames = value; }
        }

        /// <summary>
        /// Gets or sets the option to show pedantic messages.
        /// </summary>
        /// <value>The option to show pedantic messages.</value>
        public bool ShowPedanticMessages
        {
            get { return this.showPedanticMessages; }
            set { this.showPedanticMessages = value; }
        }

        /// <summary>
        /// Gets and sets if the source document should not be validated.
        /// </summary>
        /// <value>true if validation should be suppressed.</value>
        public bool SuppressValidate
        {
            get { return this.suppressValidation; }
            set { this.suppressValidation = value; }
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void AddExtension(WixExtension extension)
        {
            if (null != extension.CompilerExtension)
            {
                CompilerExtension collidingExtension = (CompilerExtension)this.extensions[extension.CompilerExtension.Schema.TargetNamespace];

                // check if this extension is addding a schema namespace that already exists
                if (null == collidingExtension)
                {
                    this.extensions.Add(extension.CompilerExtension.Schema.TargetNamespace, extension.CompilerExtension);
                }
                else
                {
                    throw new WixException(WixErrors.DuplicateExtensionXmlSchemaNamespace(extension.CompilerExtension.GetType().ToString(), extension.CompilerExtension.Schema.TargetNamespace, collidingExtension.GetType().ToString()));
                }

                if (null != extension.TableDefinitions)
                {
                    foreach (TableDefinition tableDefinition in extension.TableDefinitions)
                    {
                        if (!this.tableDefinitions.Contains(tableDefinition.Name))
                        {
                            this.tableDefinitions.Add(tableDefinition);
                        }
                        else
                        {
                            throw new WixException(WixErrors.DuplicateExtensionTable(extension.GetType().ToString(), tableDefinition.Name));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Compiles the provided Xml document into an intermediate object
        /// </summary>
        /// <param name="source">Source xml document to compile.  The BaseURI property
        /// should be properly set to get messages containing source line information.</param>
        /// <returns>Intermediate object representing compiled source document.</returns>
        /// <remarks>This method is not thread-safe.</remarks>
        public Intermediate Compile(XmlDocument source)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            bool encounteredError = true; // assume we'll hit an error

            // create the intermediate
            Intermediate target = new Intermediate();

            // try to compile it
            try
            {
                this.core = new CompilerCore(target, this.tableDefinitions, this.extensions, this.Message);
                this.core.ShowPedanticMessages = this.showPedanticMessages;

                foreach (CompilerExtension extension in this.extensions.Values)
                {
                    extension.Core = this.core;
                    extension.InitializeCompile();
                }

                // parse the document
                SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(source.DocumentElement);
                if ("Wix" == source.DocumentElement.LocalName)
                {
                    if (this.schema.TargetNamespace == source.DocumentElement.NamespaceURI)
                    {
                        this.ParseWixElement(source.DocumentElement);
                    }
                    else // invalid or missing namespace
                    {
                        if (0 == source.DocumentElement.NamespaceURI.Length)
                        {
                            this.core.OnMessage(WixErrors.InvalidWixXmlNamespace(sourceLineNumbers, this.schema.TargetNamespace));
                        }
                        else
                        {
                            this.core.OnMessage(WixErrors.InvalidWixXmlNamespace(sourceLineNumbers, source.DocumentElement.NamespaceURI, this.schema.TargetNamespace));
                        }
                    }
                }
                else
                {
                    this.core.OnMessage(WixErrors.InvalidDocumentElement(sourceLineNumbers, source.DocumentElement.Name, "source", "Wix"));
                }

                // perform schema validation if there were no errors and validation isn't suppressed
                if (!this.core.EncounteredError && !this.suppressValidation)
                {
                    this.ValidateDocument(source);
                }
            }
            finally
            {
                encounteredError = this.core.EncounteredError;

                foreach (CompilerExtension extension in this.extensions.Values)
                {
                    extension.FinalizeCompile();
                    extension.Core = null;
                }
                this.core = null;
            }

            // return the compiled intermediate only if it completed successfully
            return (encounteredError ? null : target);
        }

        /// <summary>
        /// Uppercases the first character of a string.
        /// </summary>
        /// <param name="s">String to uppercase first character of.</param>
        /// <returns>String with first character uppercased.</returns>
        private static string UppercaseFirstChar(string s)
        {
            if (0 == s.Length)
            {
                return s;
            }

            return String.Concat(s.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture), s.Substring(1));
        }

        /// <summary>
        /// Given a possible short and long file name, creat an msi filename value.
        /// </summary>
        /// <param name="shortName">The short file name.</param>
        /// <param name="longName">Possibly the long file name.</param>
        /// <returns>The value in the msi filename data type.</returns>
        private static string GetMsiFilenameValue(string shortName, string longName)
        {
            if (null != longName)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}|{1}", shortName, longName);
            }
            else
            {
                return shortName;
            }
        }

        /// <summary>
        /// Adds a search property to the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Current source/line number of processing.</param>
        /// <param name="property">Property to add to search.</param>
        /// <param name="signature">Signature for search.</param>
        private void AddAppSearch(SourceLineNumberCollection sourceLineNumbers, string property, string signature)
        {
            if (!this.core.EncounteredError)
            {
                if (property.ToUpper(CultureInfo.InvariantCulture) != property)
                {
                    this.core.OnMessage(WixErrors.SearchPropertyNotUppercase(sourceLineNumbers, "Property", "Id", property));
                }

                Row row = this.core.CreateRow(sourceLineNumbers, "AppSearch");
                row[0] = property;
                row[1] = signature;
            }
        }

        /// <summary>
        /// Adds a property to the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Current source/line number of processing.</param>
        /// <param name="property">Name of property to add.</param>
        /// <param name="value">Value of property.</param>
        /// <param name="admin">Flag if property is an admin property.</param>
        /// <param name="secure">Flag if property is a secure property.</param>
        /// <param name="hidden">Flag if property is to be hidden.</param>
        private void AddProperty(SourceLineNumberCollection sourceLineNumbers, string property, string value, bool admin, bool secure, bool hidden)
        {
            // properties without a valid identifier should not be processed any further
            if (null == property || 0 == property.Length)
            {
                return;
            }

            if (secure && property.ToUpper(CultureInfo.InvariantCulture) != property)
            {
                this.core.OnMessage(WixErrors.SecurePropertyNotUppercase(sourceLineNumbers, "Property", "Id", property));
            }

            if (null != value && 0 != value.Length)
            {
                Regex regex = new Regex(@"\[(?<identifier>[a-zA-Z_][a-zA-Z0-9_\.]*)]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
                MatchCollection matches = regex.Matches(value);

                for (int i = 0; i < matches.Count; i++)
                {
                    Group group = matches[i].Groups["identifier"];

                    if (group.Success)
                    {
                        this.core.OnMessage(WixWarnings.PropertyValueContainsPropertyReference(sourceLineNumbers, property, group.Value));
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Property");
                row[0] = property;

                // allow row to exist with no value so that PropertyRefs can be made for *Search elements
                // the linker will remove these rows before the final output is created
                if (null != value)
                {
                    row[1] = value;
                }

                if (admin || hidden || secure)
                {
                    WixPropertyRow wixPropertyRow = (WixPropertyRow)this.core.CreateRow(sourceLineNumbers, "WixProperty");
                    wixPropertyRow.Id = property;
                    wixPropertyRow.Admin = admin;
                    wixPropertyRow.Hidden = hidden;
                    wixPropertyRow.Secure = secure;
                }
            }
        }

        /// <summary>
        /// Adds a "implemented category" registry key to active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Current source/line number of processing.</param>
        /// <param name="categoryId">GUID for category.</param>
        /// <param name="classId">ClassId for to mark "implemented".</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void RegisterImplementedCategories(SourceLineNumberCollection sourceLineNumbers, string categoryId, string classId, string componentId)
        {
            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\Implemented Categories\\", categoryId), "+", null, componentId);
        }

        /// <summary>
        /// Parses an application identifer element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="advertise">The required advertise state (set depending upon the parent).</param>
        /// <param name="fileServer">Optional file identifier for CLSID when not advertised.</param>
        /// <param name="typeLibId">Optional TypeLib GUID for CLSID.</param>
        /// <param name="typeLibVersion">Optional TypeLib Version for CLSID Interfaces (if any).</param>
        private void ParseAppIdElement(XmlNode node, string componentId, YesNoType advertise, string fileServer, string typeLibId, string typeLibVersion)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string appId = null;
            string remoteServerName = null;
            string localService = null;
            string serviceParameters = null;
            string dllSurrogate = null;
            YesNoType activateAtStorage = YesNoType.NotSet;
            YesNoType runAsInteractiveUser = YesNoType.NotSet;
            string description = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            appId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "ActivateAtStorage":
                            activateAtStorage = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Advertise":
                            YesNoType appIdAdvertise = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if ((YesNoType.No == advertise && YesNoType.Yes == appIdAdvertise) || (YesNoType.Yes == advertise && YesNoType.No == appIdAdvertise))
                            {
                                this.core.OnMessage(WixErrors.AppIdIncompatibleAdvertiseState(sourceLineNumbers, node.Name, attrib.Name, appIdAdvertise.ToString(CultureInfo.InvariantCulture.NumberFormat), advertise.ToString(CultureInfo.InvariantCulture.NumberFormat)));
                            }
                            advertise = appIdAdvertise;
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DllSurrogate":
                            dllSurrogate = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "LocalService":
                            localService = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "RemoteServerName":
                            remoteServerName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "RunAsInteractiveUser":
                            runAsInteractiveUser = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "ServiceParameters":
                            serviceParameters = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == appId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // if the advertise state has not been set, default to non-advertised
            if (YesNoType.NotSet == advertise)
            {
                advertise = YesNoType.No;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Class":
                                this.ParseClassElement(child, componentId, advertise, fileServer, typeLibId, typeLibVersion, appId);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            Debug.Assert((YesNoType.Yes == advertise) || (YesNoType.No == advertise) || (YesNoType.IllegalValue == advertise), "Unexpected YesNoType value encountered.");
            if (YesNoType.Yes == advertise)
            {
                if (null != description)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWhenAdvertised(sourceLineNumbers, node.Name, "Description"));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "AppId");
                    row[0] = appId;
                    row[1] = remoteServerName;
                    row[2] = localService;
                    row[3] = serviceParameters;
                    row[4] = dllSurrogate;
                    if (YesNoType.Yes == activateAtStorage)
                    {
                        row[5] = 1;
                    }

                    if (YesNoType.Yes == runAsInteractiveUser)
                    {
                        row[6] = 1;
                    }
                }
            }
            else if (YesNoType.No == advertise)
            {
                if (null != description)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), null, description, componentId);
                }
                else
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "+", null, componentId);
                }

                if (null != remoteServerName)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "RemoteServerName", remoteServerName, componentId);
                }

                if (null != localService)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "LocalService", localService, componentId);
                }

                if (null != serviceParameters)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "ServiceParameters", serviceParameters, componentId);
                }

                if (null != dllSurrogate)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "DllSurrogate", dllSurrogate, componentId);
                }

                if (YesNoType.Yes == activateAtStorage)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "ActivateAtStorage", "Y", componentId);
                }

                if (YesNoType.Yes == runAsInteractiveUser)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("AppID\\", appId), "RunAs", "Interactive User", componentId);
                }
            }
        }

        /// <summary>
        /// Parses an AssemblyName element.
        /// </summary>
        /// <param name="node">File element to parse.</param>
        /// <param name="componentId">Parent's component id.</param>
        private void ParseAssemblyName(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "MsiAssemblyName");
                row[0] = componentId;
                row[1] = id;
                row[2] = value;
            }
        }

        /// <summary>
        /// Parses a binary element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Identifier for the new row.</returns>
        private string ParseBinaryElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string sourceFile = null;
            YesNoType suppressModularization = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceFile":
                        case "src":
                            if (null != sourceFile)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceFile", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourceFile"));
                            }
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SuppressModularization":
                            suppressModularization = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                id = CompilerCore.IllegalIdentifier;
            }
            else if (CompilerCore.IllegalIdentifier != id) // only check legal values
            {
                if (55 < id.Length)
                {
                    this.core.OnMessage(WixErrors.StreamNameTooLong(sourceLineNumbers, node.Name, "Id", id, id.Length, 55));
                }
                else if (!this.compilingProduct) // if we're not doing a product then we can't be sure that a binary identifier will fit when modularized
                {
                    if (18 < id.Length)
                    {
                        this.core.OnMessage(WixWarnings.IdentifierCannotBeModularized(sourceLineNumbers, node.Name, "Id", id, id.Length, 18));
                    }
                }
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Binary");
                row[0] = id;
                row[1] = sourceFile;
              
                if (YesNoType.Yes == suppressModularization)
                {
                    Row wixSuppressModularizationRow = this.core.CreateRow(sourceLineNumbers, "WixSuppressModularization");
                    wixSuppressModularizationRow[0] = id;
                }
            }

            return id;
        }

        /// <summary>
        /// Parses an icon element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Identifier for the new row.</returns>
        private string ParseIconElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string sourceFile = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceFile":
                        case "src":
                            if (null != sourceFile)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceFile", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourceFile"));
                            }
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                id = CompilerCore.IllegalIdentifier;
            }
            else if (CompilerCore.IllegalIdentifier != id) // only check legal values
            {
                if (57 < id.Length)
                {
                    this.core.OnMessage(WixErrors.StreamNameTooLong(sourceLineNumbers, node.Name, "Id", id, id.Length, 57));
                }
                else if (!this.compilingProduct) // if we're not doing a product then we can't be sure that a binary identifier will fit when modularized
                {
                    if (20 < id.Length)
                    {
                        this.core.OnMessage(WixWarnings.IdentifierCannotBeModularized(sourceLineNumbers, node.Name, "Id", id, id.Length, 20));
                    }
                }
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Icon");
                row[0] = id;
                row[1] = sourceFile;
            }

            return id;
        }

        /// <summary>
        /// Parses a category element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseCategoryElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string appData = null;
            string feature = null;
            string qualifier = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "AppData":
                            appData = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Feature":
                            feature = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Feature", feature);
                            break;
                        case "Qualifier":
                            qualifier = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == qualifier)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Qualifier"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "PublishComponent");
                row[0] = id;
                row[1] = qualifier;
                row[2] = componentId;
                row[3] = appData;
                if (null == feature)
                {
                    row[4] = Guid.Empty.ToString("B");
                }
                else
                {
                    row[4] = feature;
                }
            }
        }

        /// <summary>
        /// Parses a class element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="advertise">Optional Advertise State for the parent AppId element (if any).</param>
        /// <param name="fileServer">Optional file identifier for CLSID when not advertised.</param>
        /// <param name="typeLibId">Optional TypeLib GUID for CLSID.</param>
        /// <param name="typeLibVersion">Optional TypeLib Version for CLSID Interfaces (if any).</param>
        /// <param name="parentAppId">Optional parent AppId.</param>
        private void ParseClassElement(XmlNode node, string componentId, YesNoType advertise, string fileServer, string typeLibId, string typeLibVersion, string parentAppId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);

            string appId = null;
            string argument = null;
            bool class16bit = false;
            bool class32bit = false;
            string classId = null;
            StringCollection context = new StringCollection();
            bool control = false;
            string defaultInprocHandler = null;
            string defaultProgId = null;
            string description = null;
            string fileTypeMask = null;
            string icon = null;
            int iconIndex = CompilerCore.IntegerNotSet;
            string insertable = null;
            bool programmable = false;
            YesNoType relativePath = YesNoType.NotSet;
            bool safeForInit = false;
            bool safeForScripting = false;
            string threadingModel = null;
            string version = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            classId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Advertise":
                            YesNoType classAdvertise = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if ((YesNoType.No == advertise && YesNoType.Yes == classAdvertise) || (YesNoType.Yes == advertise && YesNoType.No == classAdvertise))
                            {
                                this.core.OnMessage(WixErrors.AdvertiseStateMustMatch(sourceLineNumbers, classAdvertise.ToString(CultureInfo.InvariantCulture.NumberFormat), advertise.ToString(CultureInfo.InvariantCulture.NumberFormat)));
                            }
                            advertise = classAdvertise;
                            break;
                        case "AppId":
                            if (null != parentAppId)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            appId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Argument":
                            argument = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Context":
                            string[] value = this.core.GetAttributeValue(sourceLineNumbers, attrib).Split("\r\n\t ".ToCharArray());
                            for (int i = 0; i < value.Length; ++i)
                            {
                                if (0 == value[i].Length)
                                {
                                    continue;
                                }

                                // check for duplicates in the list
                                for (int j = 0; j < context.Count; ++j)
                                {
                                    if (context[j] == value[i])
                                    {
                                        this.core.OnMessage(WixErrors.DuplicateContextValue(sourceLineNumbers, value[i]));
                                    }
                                }

                                // check if this context is 32 bit or not
                                if (value[i].EndsWith("32"))
                                {
                                    class32bit = true;
                                }
                                else
                                {
                                    class16bit = true;
                                }

                                context.Add(value[i]);
                            }
                            break;
                        case "Control":
                            control = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Handler":
                            defaultInprocHandler = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Icon":
                            icon = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "IconIndex":
                            iconIndex = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, short.MinValue + 1, short.MaxValue);
                            break;
                        case "RelativePath":
                            relativePath = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;

                        // The following attributes result in rows always added to the Registry table rather than the Class table
                        case "Insertable":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                insertable = "Insertable";
                            }
                            else
                            {
                                insertable = "NotInsertable";
                            }
                            break;
                        case "Programmable":
                            programmable = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "SafeForInitializing":
                            safeForInit = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "SafeForScripting":
                            safeForScripting = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Server":
                            fileServer = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", fileServer);
                            break;
                        case "ThreadingModel":
                            threadingModel = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Version":
                            version = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // If the advertise state has not been set, default to non-advertised.
            if (YesNoType.NotSet == advertise)
            {
                advertise = YesNoType.No;
            }

            if (null == classId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (0 == context.Count)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Context", classId));
            }

            // Local variables used strictly for child node processing.
            int fileTypeMaskIndex = 0;
            YesNoType firstProgIdForClass = YesNoType.Yes;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                        switch (child.LocalName)
                        {
                            case "FileTypeMask":
                                if (YesNoType.Yes == advertise)
                                {
                                    fileTypeMask = String.Concat(fileTypeMask, null == fileTypeMask ? String.Empty : ";", this.ParseFileTypeMaskElement(child));
                                }
                                else if (YesNoType.No == advertise)
                                {
                                    this.core.CreateRegistryRow(childSourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("FileType\\", classId, "\\", fileTypeMaskIndex.ToString(CultureInfo.InvariantCulture.NumberFormat)), String.Empty, this.ParseFileTypeMaskElement(child), componentId);
                                    fileTypeMaskIndex++;
                                }
                                break;
                            case "Interface":
                                this.ParseInterfaceElement(child, componentId, class16bit ? classId : null, class32bit ? classId : null, typeLibId, typeLibVersion);
                                break;
                            case "ProgId":
                                bool foundExtension = false;
                                string progId = this.ParseProgIdElement(child, componentId, advertise, classId, description, null, ref foundExtension, firstProgIdForClass);
                                if (null == defaultProgId)
                                {
                                    defaultProgId = progId;
                                }
                                firstProgIdForClass = YesNoType.No;
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // If this Class is being advertised.
            if (YesNoType.Yes == advertise)
            {
                if (null != fileServer)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Server", "Advertise", "yes"));
                }

                if (null == appId && null != parentAppId)
                {
                    appId = parentAppId;
                }

                // add a Class row for each context
                if (!this.core.EncounteredError)
                {
                    for (int i = 0; i < context.Count; ++i)
                    {
                        Row row = this.core.CreateRow(sourceLineNumbers, "Class");
                        row[0] = classId;
                        row[1] = context[i];
                        row[2] = componentId;
                        row[3] = defaultProgId;
                        row[4] = description;
                        if (null != appId)
                        {
                            row[5] = appId;
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "AppId", appId);
                        }
                        row[6] = fileTypeMask;
                        if (null != icon)
                        {
                            row[7] = icon;
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Icon", icon);
                        }
                        if (CompilerCore.IntegerNotSet != iconIndex)
                        {
                            row[8] = iconIndex;
                        }
                        row[9] = defaultInprocHandler;
                        row[10] = argument;
                        row[11] = Guid.Empty.ToString("B");
                        if (YesNoType.Yes == relativePath)
                        {
                            row[12] = MsiInterop.MsidbClassAttributesRelativePath;
                        }
                    }
                }
            }
            else if (YesNoType.No == advertise)
            {
                if (null == fileServer)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Server"));
                }

                if (null != appId) // need to use nesting (not a reference) for the unadvertised Class elements
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "AppId", "Advertise", "no"));
                }

                // add the core registry keys for each context in the class
                for (int i = 0; i < context.Count; ++i)
                {
                    if (null == argument)
                    {
                        this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\", context[i]), String.Empty, String.Concat("\"[#", fileServer, "]\""), componentId); // ClassId context
                    }
                    else
                    {
                        this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\", context[i]), String.Empty, String.Concat("\"[#", fileServer, "]\" ", argument), componentId); // ClassId context
                    }

                    if (null != icon) // ClassId default icon
                    {
                        this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", icon);

                        icon = String.Format(CultureInfo.InvariantCulture, "\"[#{0}]\"", icon);

                        if (CompilerCore.IntegerNotSet != iconIndex)
                        {
                            icon = String.Concat(icon, ",", iconIndex);
                        }
                        this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\", context[i], "\\DefaultIcon"), String.Empty, icon, componentId);
                    }
                }

                if (null != parentAppId) // ClassId AppId (must be specified via nesting, not with the AppId attribute)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId), "AppID", parentAppId, componentId);
                }

                if (null != description) // ClassId description
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId), String.Empty, description, componentId);
                }

                if (null != defaultInprocHandler)
                {
                    switch (defaultInprocHandler) // ClassId Default Inproc Handler
                    {
                        case "1":
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\InprocHandler"), String.Empty, "ole.dll", componentId);
                            break;
                        case "2":
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\InprocHandler32"), String.Empty, "ole32.dll", componentId);
                            break;
                        case "3":
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\InprocHandler"), String.Empty, "ole.dll", componentId);
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\InprocHandler32"), String.Empty, "ole32.dll", componentId);
                            break;
                        default:
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\InprocHandler32"), String.Empty, defaultInprocHandler, componentId);
                            break;
                    }
                }

                if (YesNoType.NotSet != relativePath) // ClassId's RelativePath
                {
                    this.core.OnMessage(WixErrors.RelativePathForRegistryElement(sourceLineNumbers));
                }
            }

            if (null != threadingModel)
            {
                threadingModel = Compiler.UppercaseFirstChar(threadingModel);

                // add a threading model for each context in the class
                for (int i = 0; i < context.Count; ++i)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\", context[i]), "ThreadingModel", threadingModel, componentId);
                }
            }

            if (null != typeLibId)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\TypeLib"), null, typeLibId, componentId);
            }

            if (null != version)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\Version"), null, version, componentId);
            }

            if (null != insertable)
            {
                // Add "*" for name so that any subkeys (shouldn't be any) are removed on uninstall.
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\", insertable), "*", null, componentId);
            }

            if (control)
            {
                // Add "*" for name so that any subkeys (shouldn't be any) are removed on uninstall.
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\Control"), "*", null, componentId);
            }

            if (programmable)
            {
                // Add "*" for name so that any subkeys (shouldn't be any) are removed on uninstall.
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\Programmable"), "*", null, componentId);
            }

            if (safeForInit)
            {
                this.RegisterImplementedCategories(sourceLineNumbers, "{7DD95802-9882-11CF-9FA9-00AA006C42C4}", classId, componentId);
            }

            if (safeForScripting)
            {
                this.RegisterImplementedCategories(sourceLineNumbers, "{7DD95801-9882-11CF-9FA9-00AA006C42C4}", classId, componentId);
            }
        }

        /// <summary>
        /// Parses an Interface element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="proxyId">16-bit proxy for interface.</param>
        /// <param name="proxyId32">32-bit proxy for interface.</param>
        /// <param name="typeLibId">Optional TypeLib GUID for CLSID.</param>
        /// <param name="typelibVersion">Version of the TypeLib to which this interface belongs.  Required if typeLibId is specified</param>
        private void ParseInterfaceElement(XmlNode node, string componentId, string proxyId, string proxyId32, string typeLibId, string typelibVersion)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string baseInterface = null;
            string interfaceId = null;
            string name = null;
            int numMethods = CompilerCore.IntegerNotSet;
            bool versioned = true;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            interfaceId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "BaseInterface":
                            baseInterface = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "NumMethods":
                            numMethods = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "ProxyStubClassId":
                            proxyId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "ProxyStubClassId32":
                            proxyId32 = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Versioned":
                            versioned = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == interfaceId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId), null, name, componentId);
            if (null != typeLibId)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId, "\\TypeLib"), null, typeLibId, componentId);
                if (versioned)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId, "\\TypeLib"), "Version", typelibVersion, componentId);
                }
            }

            if (null != baseInterface)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId, "\\BaseInterface"), null, baseInterface, componentId);
            }

            if (CompilerCore.IntegerNotSet != numMethods)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId, "\\NumMethods"), null, numMethods.ToString(CultureInfo.InvariantCulture.NumberFormat), componentId);
            }

            if (null != proxyId)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId, "\\ProxyStubClsid"), null, proxyId, componentId);
            }

            if (null != proxyId32)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("Interface\\", interfaceId, "\\ProxyStubClsid32"), null, proxyId32, componentId);
            }
        }

        /// <summary>
        /// Parses a CLSID's file type mask element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>String representing the file type mask elements.</returns>
        private string ParseFileTypeMaskElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int cb = 0;
            int offset = CompilerCore.IntegerNotSet;
            string mask = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Mask":
                            mask = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Offset":
                            offset = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == mask)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Mask"));
            }

            if (CompilerCore.IntegerNotSet == offset)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Offset"));
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                if (mask.Length != value.Length)
                {
                    this.core.OnMessage(WixErrors.ValueAndMaskMustBeSameLength(sourceLineNumbers));
                }
                cb = mask.Length / 2;
            }

            return String.Concat(offset.ToString(CultureInfo.InvariantCulture.NumberFormat), ",", cb.ToString(CultureInfo.InvariantCulture.NumberFormat), ",", mask, ",", value);
        }

        /// <summary>
        /// Parses a registry search element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Signature for search element.</returns>
        private string ParseRegistrySearchElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string key = null;
            string name = null;
            string signature = null;
            int root = CompilerCore.IntegerNotSet;
            int type = CompilerCore.IntegerNotSet;
            bool search64bit = false;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Root":
                            string rootValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (rootValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "HKCR":
                                    root = MsiInterop.MsidbRegistryRootClassesRoot;
                                    break;
                                case "HKCU":
                                    root = MsiInterop.MsidbRegistryRootCurrentUser;
                                    break;
                                case "HKLM":
                                    root = MsiInterop.MsidbRegistryRootLocalMachine;
                                    break;
                                case "HKU":
                                    root = MsiInterop.MsidbRegistryRootUsers;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Root", rootValue, "HKCR", "HKCU", "HKLM", "HKU"));
                                    break;
                            }
                            break;
                        case "Type":
                            string typeValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (typeValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "directory":
                                    type = 0;
                                    break;
                                case "file":
                                    type = 1;
                                    break;
                                case "raw":
                                    type = 2;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Type", typeValue, "directory", "file", "raw"));
                                    break;
                            }
                            break;
                        case "Win64":
                            search64bit = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
            }

            if (CompilerCore.IntegerNotSet == root)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Root"));
            }

            if (CompilerCore.IntegerNotSet == type)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Type"));
            }

            signature = id;
            bool oneChild = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "DirectorySearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;

                                // directorysearch parentage should work like directory element, not the rest of the signature type because of the DrLocator.Parent column
                                signature = this.ParseDirectorySearchElement(child, id);
                                break;
                            case "DirectorySearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchRefElement(child, id);
                                break;
                            case "FileSearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseFileSearchElement(child, id);
                                id = signature; // FileSearch signatures override parent signatures
                                break;
                            case "FileSearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                id = this.ParseSimpleRefElement(child, "Signature"); // FileSearch signatures override parent signatures
                                signature = null;
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "RegLocator");
                row[0] = id;
                row[1] = root;
                row[2] = key;
                row[3] = name;
                row[4] = search64bit ? (type | 16) : type;
            }

            return signature;
        }

        /// <summary>
        /// Parses a registry search reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Signature of referenced search element.</returns>
        private string ParseRegistrySearchRefElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "RegLocator", id);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return id; // the id of the RegistrySearchRef element is its signature
        }

        /// <summary>
        /// Parses child elements for search signatures.
        /// </summary>
        /// <param name="node">Node whose children we are parsing.</param>
        /// <returns>Returns ArrayList of string signatures.</returns>
        private ArrayList ParseSearchSignatures(XmlNode node)
        {
            ArrayList signatures = new ArrayList();
            foreach (XmlNode child in node.ChildNodes)
            {
                string signature = null;
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ComplianceDrive":
                                signature = this.ParseComplianceDriveElement(child);
                                break;
                            case "ComponentSearch":
                                signature = this.ParseComponentSearchElement(child);
                                break;
                            case "DirectorySearch":
                                signature = this.ParseDirectorySearchElement(child, String.Empty);
                                break;
                            case "DirectorySearchRef":
                                signature = this.ParseDirectorySearchRefElement(child, String.Empty);
                                break;
                            case "FileSearch":
                                signature = this.ParseFileSearchElement(child, String.Empty);
                                break;
                            case "IniFileSearch":
                                signature = this.ParseIniFileSearchElement(child);
                                break;
                            case "RegistrySearch":
                                signature = this.ParseRegistrySearchElement(child);
                                break;
                            case "RegistrySearchRef":
                                signature = this.ParseRegistrySearchRefElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }

                if (null != signature)
                {
                    signatures.Add(signature);
                }
            }

            return signatures;
        }

        /// <summary>
        /// Parses a compliance drive element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Signature of nested search elements.</returns>
        private string ParseComplianceDriveElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string signature = null;

            bool oneChild = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
                        switch (child.LocalName)
                        {
                            case "DirectorySearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchElement(child, "CCP_DRIVE");
                                break;
                            case "DirectorySearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchRefElement(child, "CCP_DRIVE");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null == signature)
            {
                this.core.OnMessage(WixErrors.SearchElementRequired(sourceLineNumbers, node.Name));
            }

            return signature;
        }

        /// <summary>
        /// Parses a compilance check element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseComplianceCheckElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string signature = null;

            // see if this property is used for appSearch
            ArrayList signatures = this.ParseSearchSignatures(node);
            foreach (string sig in signatures)
            {
                // if we haven't picked a signature for this ComplianceCheck pick
                // this one
                if (null == signature)
                {
                    signature = sig;
                }
                else if (signature != sig)
                {
                    // all signatures under a ComplianceCheck must be the same
                    this.core.OnMessage(WixErrors.MultipleIdentifiersFound(sourceLineNumbers, node.Name, sig, signature));
                }
            }

            if (null == signature)
            {
                this.core.OnMessage(WixErrors.SearchElementRequired(sourceLineNumbers, node.Name));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "CCPSearch");
                row[0] = signature;
            }
        }

        /// <summary>
        /// Parses a component element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="diskId">Disk id inherited from parent directory.</param>
        /// <param name="directoryId">Identifier for component's directory.</param>
        /// <param name="srcPath">Source path for files up to this point.</param>
        private void ParseComponentElement(XmlNode node, int diskId, string directoryId, string srcPath)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);

            string id = null;
            int bits = 0;
            int comPlusBits = CompilerCore.IntegerNotSet;
            string guid = null;
            string condition = null;
            int keyBits = 0;
            bool keyFound = false;
            string keyPath = null;
            bool win64 = false;

            int files = 0;
            bool encounteredODBCDataSource = false;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ComPlusFlags":
                            comPlusBits = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "DisableRegistryReflection":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbComponentAttributesDisableRegistryReflection;
                            }
                            break;
                        case "DiskId":
                            diskId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "Guid":
                            guid = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, true, true);
                            if ("????????-????-????-????-????????????" == guid)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedQuestionMarksGuid(sourceLineNumbers, node.Name, attrib.Name));
                            }
                            break;
                        case "KeyPath":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                keyFound = true;
                                keyPath = null;
                                keyBits = 0;
                            }
                            break;
                        case "Location":
                            string location = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (location)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "either":
                                    bits |= MsiInterop.MsidbComponentAttributesOptional;
                                    break;
                                case "local": // this is the default
                                    break;
                                case "source":
                                    bits |= MsiInterop.MsidbComponentAttributesSourceOnly;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, "either", "local", "source"));
                                    break;
                            }
                            break;
                        case "NeverOverwrite":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbComponentAttributesNeverOverwrite;
                            }
                            break;
                        case "Permanent":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbComponentAttributesPermanent;
                            }
                            break;
                        case "SharedDllRefCount":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbComponentAttributesSharedDllRefCount;
                            }
                            break;
                        case "Transitive":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbComponentAttributesTransitive;
                            }
                            break;
                        case "Win64":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbComponentAttributes64bit;
                                win64 = true;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == guid)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Guid"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        bool keyPathSet = false;
                        string keyPossible = null;
                        int keyBit = 0;
                        switch (child.LocalName)
                        {
                            case "AppId":
                                this.ParseAppIdElement(child, id, YesNoType.NotSet, null, null, null);
                                break;
                            case "Category":
                                this.ParseCategoryElement(child, id);
                                break;
                            case "Class":
                                this.ParseClassElement(child, id, YesNoType.NotSet, null, null, null, null);
                                break;
                            case "Condition":
                                if (null != condition)
                                {
                                    SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
                                    this.core.OnMessage(WixErrors.TooManyChildren(childSourceLineNumbers, node.Name, child.Name));
                                }
                                condition = this.ParseConditionElement(child, node.LocalName, null, null);
                                break;
                            case "CopyFile":
                                this.ParseCopyFileElement(child, id, null);
                                break;
                            case "CreateFolder":
                                this.ParseCreateFolderElement(child, id, directoryId);
                                break;
                            case "Environment":
                                this.ParseEnvironmentElement(child, id);
                                break;
                            case "Extension":
                                this.ParseExtensionElement(child, id, YesNoType.NotSet, null);
                                break;
                            case "File":
                                keyPathSet = this.ParseFileElement(child, id, directoryId, diskId, srcPath, out keyPossible, win64);
                                keyBit = 0;
                                files++;
                                break;
                            case "IniFile":
                                this.ParseIniFileElement(child, id);
                                break;
                            case "Interface":
                                this.ParseInterfaceElement(child, id, null, null, null, null);
                                break;
                            case "IsolateComponent":
                                this.ParseIsolateComponentElement(child, id);
                                break;
                            case "ODBCDataSource":
                                keyPathSet = this.ParseODBCDataSource(child, id, null, out keyPossible);
                                keyBit = MsiInterop.MsidbComponentAttributesODBCDataSource;
                                encounteredODBCDataSource = true;
                                break;
                            case "ODBCDriver":
                                this.ParseODBCDriverOrTranslator(child, id, null, this.tableDefinitions["ODBCDriver"]);
                                break;
                            case "ODBCTranslator":
                                this.ParseODBCDriverOrTranslator(child, id, null, this.tableDefinitions["ODBCTranslator"]);
                                break;
                            case "ProgId":
                                bool foundExtension = false;
                                this.ParseProgIdElement(child, id, YesNoType.NotSet, null, null, null, ref foundExtension, YesNoType.NotSet);
                                break;
                            case "Registry":
                                keyPathSet = this.ParseRegistryElement(child, id, CompilerCore.IntegerNotSet, null, out keyPossible);
                                keyBit = MsiInterop.MsidbComponentAttributesRegistryKeyPath;
                                break;
                            case "RegistryKey":
                                keyPathSet = this.ParseRegistryKeyElement(child, id, CompilerCore.IntegerNotSet, null, out keyPossible);
                                keyBit = MsiInterop.MsidbComponentAttributesRegistryKeyPath;
                                break;
                            case "RegistryValue":
                                keyPathSet = this.ParseRegistryValueElement(child, id, CompilerCore.IntegerNotSet, null, out keyPossible);
                                keyBit = MsiInterop.MsidbComponentAttributesRegistryKeyPath;
                                break;
                            case "RemoveFile":
                                this.ParseRemoveFileElement(child, id, directoryId);
                                break;
                            case "RemoveFolder":
                                this.ParseRemoveFolderElement(child, id, directoryId);
                                break;
                            case "RemoveRegistryKey":
                                this.ParseRemoveRegistryKeyElement(child, id);
                                break;
                            case "RemoveRegistryValue":
                                this.ParseRemoveRegistryValueElement(child, id);
                                break;
                            case "ReserveCost":
                                this.ParseReserveCostElement(child, id, directoryId);
                                break;
                            case "ServiceControl":
                                this.ParseServiceControlElement(child, id);
                                break;
                            case "ServiceInstall":
                                this.ParseServiceInstallElement(child, id);
                                break;
                            case "Shortcut":
                                this.ParseShortcutElement(child, id, node.LocalName, directoryId);
                                break;
                            case "TypeLib":
                                this.ParseTypeLibElement(child, id, null, win64);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                        Debug.Assert(!keyPathSet || (keyPathSet && null != keyPossible));

                        if (keyFound && keyPathSet)
                        {
                            this.core.OnMessage(WixErrors.ComponentMultipleKeyPaths(sourceLineNumbers, node.Name, "KeyPath", "yes", "File", "RegistryValue", "ODBCDataSource"));
                        }

                        // if a possible KeyPath has been found and that value was explicitly set as
                        // the KeyPath of the component, set it now.  Alternatively, if a possible
                        // KeyPath has been found and no KeyPath has been previously set, use this
                        // value as the default KeyPath of the component
                        if (null != keyPossible && (keyPathSet || (null == keyPath && !keyFound)))
                        {
                            keyFound = keyPathSet;
                            keyPath = keyPossible;
                            keyBits = keyBit;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, directoryId);
                    }
                }
            }

            // check for implicit KeyPath which can easily be accidentally changed
            if (this.showPedanticMessages && !keyFound)
            {
                this.core.OnMessage(WixErrors.ImplicitComponentKeyPath(sourceLineNumbers, id));
            }

            if ("*" == guid)
            {
                // check for conditions that may exclude this component from using generated guids
                if (1 != files || encounteredODBCDataSource || 0 != keyBits)
                {
                    this.core.OnMessage(WixErrors.IllegalComponentWithGeneratableGuid(sourceLineNumbers));
                }
                else // TODO: remove this warning about using this experimental functionality
                {
                    this.core.OnMessage(WixWarnings.GeneratableComponentGuidsAreDangerous(sourceLineNumbers));
                }
            }

            // finally add the Component table row
            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Component");
                row[0] = id;
                row[1] = guid;
                row[2] = directoryId;
                row[3] = bits | keyBits;
                row[4] = condition;
                row[5] = keyPath;
            }

            // if this is a module, automatically add this component to the references to ensure it gets in the ModuleComponents table
            if (this.compilingModule)
            {
                this.core.CreateWixComplexReferenceRow(sourceLineNumbers, ComplexReferenceParentType.Module, this.activeName, this.activeLanguage, ComplexReferenceChildType.Component, id, false);
            }

            if (!this.core.EncounteredError)
            {
                // Complus
                if (CompilerCore.IntegerNotSet != comPlusBits)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "Complus");
                    row[0] = id;
                    row[1] = comPlusBits;
                }
            }
        }

        /// <summary>
        /// Parses a component group element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseComponentGroupElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ComponentGroupRef":
                                this.ParseComponentGroupRefElement(child, ComplexReferenceParentType.Group, id, null);
                                break;
                            case "ComponentRef":
                                this.ParseComponentRefElement(child, ComplexReferenceParentType.Group, id, null);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixComponentGroup");
                row[0] = id;
            }
        }

        /// <summary>
        /// Parses a component group reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentType">ComplexReferenceParentType of parent element.</param>
        /// <param name="parentId">Identifier of parent element (usually a Feature or Module).</param>
        /// <param name="parentLanguage">Optional language of parent (only useful for Modules).</param>
        private void ParseComponentGroupRefElement(XmlNode node, ComplexReferenceParentType parentType, string parentId, string parentLanguage)
        {
            Debug.Assert(ComplexReferenceParentType.Group == parentType || ComplexReferenceParentType.Feature == parentType || ComplexReferenceParentType.Module == parentType);

            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            YesNoType primary = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixComponentGroup", id);
                            break;
                        case "Primary":
                            primary = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.CreateWixComplexReferenceRow(sourceLineNumbers, parentType, parentId, parentLanguage, ComplexReferenceChildType.Group, id, (YesNoType.Yes == primary));
        }

        /// <summary>
        /// Parses a component reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentType">ComplexReferenceParentType of parent element.</param>
        /// <param name="parentId">Identifier of parent element (usually a Feature or Module).</param>
        /// <param name="parentLanguage">Optional language of parent (only useful for Modules).</param>
        private void ParseComponentRefElement(XmlNode node, ComplexReferenceParentType parentType, string parentId, string parentLanguage)
        {
            Debug.Assert(ComplexReferenceParentType.Group == parentType || ComplexReferenceParentType.Feature == parentType || ComplexReferenceParentType.Module == parentType);

            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            YesNoType primary = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Component", id);
                            break;
                        case "Primary":
                            primary = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.CreateWixComplexReferenceRow(sourceLineNumbers, parentType, parentId, parentLanguage, ComplexReferenceChildType.Component, id, (YesNoType.Yes == primary));
        }

        /// <summary>
        /// Parses a component search element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Signature for search element.</returns>
        private string ParseComponentSearchElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string signature = null;
            string id = null;
            string componentId = null;
            int type = MsiInterop.MsidbLocatorTypeFileName;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Guid":
                            componentId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Type":
                            string typeValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (typeValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "directory":
                                    type = MsiInterop.MsidbLocatorTypeDirectory;
                                    break;
                                case "file":
                                    type = MsiInterop.MsidbLocatorTypeFileName;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, typeValue, "directory", "file"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            signature = id;
            bool oneChild = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "DirectorySearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;

                                // directorysearch parentage should work like directory element, not the rest of the signature type because of the DrLocator.Parent column
                                signature = this.ParseDirectorySearchElement(child, id);
                                break;
                            case "DirectorySearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchRefElement(child, id);
                                break;
                            case "FileSearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseFileSearchElement(child, id);
                                id = signature; // FileSearch signatures override parent signatures
                                break;
                            case "FileSearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                id = this.ParseSimpleRefElement(child, "Signature"); // FileSearch signatures override parent signatures
                                signature = null;
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "CompLocator");
                row[0] = id;
                row[1] = componentId;
                row[2] = type;
            }

            return signature;
        }

        /// <summary>
        /// Parses a create folder element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        /// <param name="directoryId">Default identifier for directory to create.</param>
        private void ParseCreateFolderElement(XmlNode node, string componentId, string directoryId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Directory":
                            directoryId = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", directoryId);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Shortcut":
                                this.ParseShortcutElement(child, componentId, node.LocalName, directoryId);
                                break;
                            case "Permission":
                                this.ParsePermissionElement(child, directoryId, "CreateFolder");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, directoryId, componentId);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "CreateFolder");
                row[0] = directoryId;
                row[1] = componentId;
            }
        }

        /// <summary>
        /// Parses a copy file element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="fileId">Identifier of file to copy (null if moving the file).</param>
        private void ParseCopyFileElement(XmlNode node, string componentId, string fileId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            bool delete = false;
            string destinationDirectory = null;
            string destinationLongName = null;
            string destinationName = null;
            string destinationProperty = null;
            string sourceDirectory = null;
            string sourceFolder = null;
            string sourceName = null;
            string sourceProperty = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Delete":
                            delete = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "DestinationDirectory":
                            destinationDirectory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", destinationDirectory);
                            break;
                        case "DestinationLongName":
                            destinationLongName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "DestinationName":
                            destinationName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "DestinationProperty":
                            destinationProperty = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "FileId":
                            if (null != fileId)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            fileId = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceDirectory":
                            sourceDirectory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", sourceDirectory);
                            break;
                        case "SourceName":
                            sourceName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceProperty":
                            sourceProperty = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null != sourceFolder && null != sourceDirectory) // SourceFolder and SourceDirectory cannot coexist
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceFolder", "SourceDirectory"));
            }

            if (null != sourceFolder && null != sourceProperty) // SourceFolder and SourceProperty cannot coexist
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceFolder", "SourceProperty"));
            }

            if (null != sourceDirectory && null != sourceProperty) // SourceDirectory and SourceProperty cannot coexist
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceProperty", "SourceDirectory"));
            }

            if (null != destinationDirectory && null != destinationProperty) // DestinationDirectory and DestinationProperty cannot coexist
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "DestinationProperty", "DestinationDirectory"));
            }

            if (null != destinationLongName)
            {
                if (null == destinationName)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DestinationName", "DestinationLongName"));
                }

                destinationName = String.Concat(destinationName, "|", destinationLongName);
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null == fileId)
            {
                // DestinationDirectory or DestinationProperty must be specified
                if (null == destinationDirectory && null == destinationProperty)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttributesWithoutOtherAttribute(sourceLineNumbers, node.Name, "DestinationDirectory", "DestinationProperty", "FileId"));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MoveFile");
                    row[0] = id;
                    row[1] = componentId;
                    row[2] = sourceName;
                    row[3] = destinationName;
                    if (null != sourceDirectory)
                    {
                        row[4] = sourceDirectory;
                    }
                    else if (null != sourceProperty)
                    {
                        row[4] = sourceProperty;
                    }
                    else
                    {
                        row[4] = sourceFolder;
                    }

                    if (null != destinationDirectory)
                    {
                        row[5] = destinationDirectory;
                    }
                    else
                    {
                        row[5] = destinationProperty;
                    }
                    row[6] = delete ? 1 : 0;
                }
            }
            else // copy the file
            {
                if (null != sourceDirectory)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceDirectory", "FileId"));
                }

                if (null != sourceFolder)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceFolder", "FileId"));
                }

                if (null != sourceName)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceName", "FileId"));
                }

                if (null != sourceProperty)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceProperty", "FileId"));
                }

                if (delete)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Delete", "FileId"));
                }

                if (null == destinationName && null == destinationDirectory && null == destinationProperty)
                {
                    this.core.OnMessage(WixWarnings.CopyFileFileIdUseless(sourceLineNumbers));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "DuplicateFile");
                    row[0] = id;
                    row[1] = componentId;
                    row[2] = fileId;
                    row[3] = destinationName;
                    if (null != destinationDirectory)
                    {
                        row[4] = destinationDirectory;
                    }
                    else
                    {
                        row[4] = destinationProperty;
                    }
                }
            }
        }

        /// <summary>
        /// Parses a CustomAction element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseCustomActionElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int bits = 0;
            bool inlineScript = false;
            string innerText = null;
            string source = null;
            int sourceBits = 0;
            YesNoType suppressModularization = YesNoType.NotSet;
            string target = null;
            int targetBits = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "BinaryKey":
                            if (null != source)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleSources(sourceLineNumbers, node.Name, attrib.Name, "BinaryKey", "Directory", "FileKey", "Property", "Script"));
                            }
                            source = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            sourceBits = MsiInterop.MsidbCustomActionTypeBinaryData;
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Binary", source); // add a reference to the appropriate Binary
                            break;
                        case "Directory":
                            if (null != source)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleSources(sourceLineNumbers, node.Name, attrib.Name, "BinaryKey", "Directory", "FileKey", "Property", "Script"));
                            }
                            source = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            sourceBits = MsiInterop.MsidbCustomActionTypeDirectory;
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", source); // add a reference to the appropriate Directory
                            break;
                        case "DllEntry":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            targetBits = MsiInterop.MsidbCustomActionTypeDll;
                            break;
                        case "Error":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            targetBits = MsiInterop.MsidbCustomActionTypeTextData | MsiInterop.MsidbCustomActionTypeSourceFile;

                            bool errorReference = true;

                            try
                            {
                                // The target can be either a formatted error string or a literal 
                                // error number. Try to convert to error number to determine whether
                                // to add a reference. No need to look at the value.
                                Convert.ToInt32(target, CultureInfo.InvariantCulture.NumberFormat);
                            }
                            catch (FormatException)
                            {
                                errorReference = false;
                            }
                            catch (OverflowException)
                            {
                                errorReference = false;
                            }

                            if (errorReference)
                            {
                                this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Error", target);
                            }
                            break;
                        case "ExeCommand":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib, true); // one of the few cases where an empty string value is valid
                            targetBits = MsiInterop.MsidbCustomActionTypeExe;
                            break;
                        case "Execute":
                            string execute = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (execute)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "commit":
                                    bits |= MsiInterop.MsidbCustomActionTypeInScript | MsiInterop.MsidbCustomActionTypeCommit;
                                    break;
                                case "deferred":
                                    bits |= MsiInterop.MsidbCustomActionTypeInScript;
                                    break;
                                case "firstSequence":
                                    bits |= MsiInterop.MsidbCustomActionTypeFirstSequence;
                                    break;
                                case "immediate":
                                    break;
                                case "oncePerProcess":
                                    bits |= MsiInterop.MsidbCustomActionTypeOncePerProcess;
                                    break;
                                case "rollback":
                                    bits |= MsiInterop.MsidbCustomActionTypeInScript | MsiInterop.MsidbCustomActionTypeRollback;
                                    break;
                                case "secondSequence":
                                    bits |= MsiInterop.MsidbCustomActionTypeClientRepeat;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, execute, "commit", "deferred", "firstSequence", "immediate", "oncePerProcess", "rollback", "secondSequence"));
                                    break;
                            }
                            break;
                        case "FileKey":
                            if (null != source)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleSources(sourceLineNumbers, node.Name, attrib.Name, "BinaryKey", "Directory", "FileKey", "Property", "Script"));
                            }
                            source = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            sourceBits = MsiInterop.MsidbCustomActionTypeSourceFile;
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", source); // add a reference to the appropriate File
                            break;
                        case "HideTarget":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbCustomActionTypeHideTarget;
                            }
                            break;
                        case "Impersonate":
                            if (YesNoType.No == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbCustomActionTypeNoImpersonate;
                            }
                            break;
                        case "JScriptCall":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib, true); // one of the few cases where an empty string value is valid
                            targetBits = MsiInterop.MsidbCustomActionTypeJScript;
                            break;
                        case "Property":
                            if (null != source)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleSources(sourceLineNumbers, node.Name, attrib.Name, "BinaryKey", "Directory", "FileKey", "Property", "Script"));
                            }
                            source = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            sourceBits = MsiInterop.MsidbCustomActionTypeProperty;
                            break;
                        case "Return":
                            string returnValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (returnValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "asyncNoWait":
                                    bits |= MsiInterop.MsidbCustomActionTypeAsync | MsiInterop.MsidbCustomActionTypeContinue;
                                    break;
                                case "asyncWait":
                                    bits |= MsiInterop.MsidbCustomActionTypeAsync;
                                    break;
                                case "check":
                                    break;
                                case "ignore":
                                    bits |= MsiInterop.MsidbCustomActionTypeContinue;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, returnValue, "asyncNoWait", "asyncWait", "check", "ignore"));
                                    break;
                            }
                            break;
                        case "Script":
                            if (null != source)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleSources(sourceLineNumbers, node.Name, attrib.Name, "BinaryKey", "Directory", "FileKey", "Property", "Script"));
                            }

                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }

                            // set the source and target to empty string for error messages when the user sets multiple sources or targets
                            source = string.Empty;
                            target = string.Empty;

                            inlineScript = true;

                            string script = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (script)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "jscript":
                                    sourceBits = MsiInterop.MsidbCustomActionTypeDirectory;
                                    targetBits = MsiInterop.MsidbCustomActionTypeJScript;
                                    break;
                                case "vbscript":
                                    sourceBits = MsiInterop.MsidbCustomActionTypeDirectory;
                                    targetBits = MsiInterop.MsidbCustomActionTypeVBScript;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, script, "jscript", "vbscript"));
                                    break;
                            }
                            break;
                        case "SuppressModularization":
                            suppressModularization = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "TerminalServerAware":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbCustomActionTypeTSAware;
                            }
                            break;
                        case "Value":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib, true); // one of the few cases where an empty string value is valid
                            targetBits = MsiInterop.MsidbCustomActionTypeTextData;
                            break;
                        case "VBScriptCall":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.CustomActionMultipleTargets(sourceLineNumbers, node.Name, attrib.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib, true); // one of the few cases where an empty string value is valid
                            targetBits = MsiInterop.MsidbCustomActionTypeVBScript;
                            break;
                        case "Win64":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbCustomActionType64BitScript;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            // get the inner text if any exists
            innerText = CompilerCore.GetTrimmedInnerText(node);

            // if we have an in-lined Script CustomAction ensure no source or target attributes were provided
            if (inlineScript)
            {
                target = innerText;
            }
            else if (MsiInterop.MsidbCustomActionTypeVBScript == targetBits) // non-inline vbscript
            {
                if (null == source)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "VBScriptCall", "BinaryKey", "FileKey", "Property"));
                }
                else if (MsiInterop.MsidbCustomActionTypeDirectory == sourceBits)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "VBScriptCall", "Directory"));
                }
            }
            else if (MsiInterop.MsidbCustomActionTypeJScript == targetBits) // non-inline jscript
            {
                if (null == source)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "JScriptCall", "BinaryKey", "FileKey", "Property"));
                }
                else if (MsiInterop.MsidbCustomActionTypeDirectory == sourceBits)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "JScriptCall", "Directory"));
                }
            }
            else if (MsiInterop.MsidbCustomActionTypeTextData == (bits | sourceBits | targetBits))
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "Value", "Directory", "Property"));
            }
            else if (0 != innerText.Length) // inner text cannot be specified with non-script CAs
            {
                this.core.OnMessage(WixErrors.CustomActionIllegalInnerText(sourceLineNumbers, node.Name, innerText, "Script"));
            }

            if (MsiInterop.MsidbCustomActionType64BitScript == (bits & MsiInterop.MsidbCustomActionType64BitScript) && MsiInterop.MsidbCustomActionTypeVBScript != targetBits && MsiInterop.MsidbCustomActionTypeJScript != targetBits)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "Win64", "Script", "VBScriptCall", "JScriptCall"));
            }

            if ((MsiInterop.MsidbCustomActionTypeAsync | MsiInterop.MsidbCustomActionTypeContinue) == (bits & (MsiInterop.MsidbCustomActionTypeAsync | MsiInterop.MsidbCustomActionTypeContinue)) && MsiInterop.MsidbCustomActionTypeExe != targetBits)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeValueWithoutOtherAttribute(sourceLineNumbers, node.Name, "Return", "asyncNoWait", "ExeCommand"));
            }

            if (MsiInterop.MsidbCustomActionTypeTSAware == (bits & MsiInterop.MsidbCustomActionTypeTSAware))
            {
                // TS-aware CAs are valid only when deferred so require the in-script Type bit...
                if (0 == (bits & MsiInterop.MsidbCustomActionTypeInScript))
                {
                    this.core.OnMessage(WixErrors.IllegalTerminalServerCustomActionAttributes(sourceLineNumbers));
                }
            }

            if (0 == targetBits)
            {
                this.core.OnMessage(WixErrors.ExpectedAttributes(sourceLineNumbers, node.Name, "DllEntry", "Error", "ExeCommand", "JScriptCall", "Script", "Value", "VBScriptCall"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "CustomAction");
                row[0] = id;
                row[1] = bits | sourceBits | targetBits;
                row[2] = source;
                row[3] = target;

                if (YesNoType.Yes == suppressModularization)
                {
                    Row wixSuppressModularizationRow = this.core.CreateRow(sourceLineNumbers, "WixSuppressModularization");
                    wixSuppressModularizationRow[0] = id;
                }
            }
        }

        /// <summary>
        /// Parses a simple reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="table">Table which contains the target of the simple reference.</param>
        /// <returns>Id of the referenced element.</returns>
        private string ParseSimpleRefElement(XmlNode node, string table)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, table, id);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Parses a PatchFamilyRef element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Id of the referenced element.</returns>
        private void ParsePatchFamilyRefElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string[] primaryKeys = new string[2];

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            primaryKeys[0] = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ProductCode":
                            primaryKeys[1] = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == primaryKeys[0])
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "MsiPatchSequence", primaryKeys);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }
        }

        /// <summary>
        /// Parses an ensure table element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseEnsureTableElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            else if (31 < id.Length)
            {
                this.core.OnMessage(WixErrors.TableNameTooLong(sourceLineNumbers, node.Name, "Id", id));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.EnsureTable(sourceLineNumbers, id);
        }

        /// <summary>
        /// Parses a custom table element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <remarks>not cleaned</remarks>
        private void ParseCustomTableElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string tableId = null;

            string categories = null;
            int columnCount = 0;
            string columnNames = null;
            string columnTypes = null;
            string descriptions = null;
            string keyColumns = null;
            string keyTables = null;
            string maxValues = null;
            string minValues = null;
            string modularizations = null;
            string primaryKeys = null;
            string sets = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            tableId = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == tableId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            else if (31 < tableId.Length)
            {
                this.core.OnMessage(WixErrors.CustomTableNameTooLong(sourceLineNumbers, node.Name, "Id", tableId));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                        switch (child.LocalName)
                        {
                            case "Column":
                                ++columnCount;

                                string category = String.Empty;
                                string columnName = null;
                                string columnType = null;
                                string description = String.Empty;
                                int keyColumn = CompilerCore.IntegerNotSet;
                                string keyTable = String.Empty;
                                bool localizable = false;
                                long maxValue = CompilerCore.LongNotSet;
                                long minValue = CompilerCore.LongNotSet;
                                string modularization = "None";
                                bool nullable = false;
                                bool primaryKey = false;
                                string setValues = String.Empty;
                                string typeName = null;
                                int width = 0;

                                foreach (XmlAttribute childAttrib in child.Attributes)
                                {
                                    switch (childAttrib.LocalName)
                                    {
                                        case "Id":
                                            columnName = this.core.GetAttributeIdentifierValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "Category":
                                            category = this.core.GetAttributeValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "Description":
                                            description = this.core.GetAttributeValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "KeyColumn":
                                            keyColumn = this.core.GetAttributeIntegerValue(childSourceLineNumbers, childAttrib, 1, 32);
                                            break;
                                        case "KeyTable":
                                            keyTable = this.core.GetAttributeValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "Localizable":
                                            localizable = YesNoType.Yes == this.core.GetAttributeYesNoValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "MaxValue":
                                            maxValue = this.core.GetAttributeLongValue(childSourceLineNumbers, childAttrib, int.MinValue + 1, int.MaxValue);
                                            break;
                                        case "MinValue":
                                            minValue = this.core.GetAttributeLongValue(childSourceLineNumbers, childAttrib, int.MinValue + 1, int.MaxValue);
                                            break;
                                        case "Modularize":
                                            modularization = this.core.GetAttributeValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "Nullable":
                                            nullable = YesNoType.Yes == this.core.GetAttributeYesNoValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "PrimaryKey":
                                            primaryKey = YesNoType.Yes == this.core.GetAttributeYesNoValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "Set":
                                            setValues = this.core.GetAttributeValue(childSourceLineNumbers, childAttrib);
                                            break;
                                        case "Type":
                                            string typeValue = this.core.GetAttributeValue(childSourceLineNumbers, childAttrib);
                                            switch (typeValue)
                                            {
                                                case CompilerCore.IllegalEmptyAttributeValue:
                                                    break;
                                                case "binary":
                                                    typeName = "OBJECT";
                                                    break;
                                                case "int":
                                                    typeName = "SHORT";
                                                    break;
                                                case "string":
                                                    typeName = "CHAR";
                                                    break;
                                                default:
                                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(childSourceLineNumbers, child.Name, "Type", typeValue, "binary", "int", "string"));
                                                    break;
                                            }
                                            break;
                                        case "Width":
                                            width = this.core.GetAttributeIntegerValue(childSourceLineNumbers, childAttrib, 0, int.MaxValue);
                                            break;
                                        default:
                                            this.core.UnexpectedAttribute(childSourceLineNumbers, childAttrib);
                                            break;
                                    }
                                }

                                if (null == columnName)
                                {
                                    this.core.OnMessage(WixErrors.ExpectedAttribute(childSourceLineNumbers, child.Name, "Id"));
                                }

                                if (null == typeName)
                                {
                                    this.core.OnMessage(WixErrors.ExpectedAttribute(childSourceLineNumbers, child.Name, "Type"));
                                }
                                else if ("SHORT" == typeName)
                                {
                                    if (2 != width && 4 != width)
                                    {
                                        this.core.OnMessage(WixErrors.CustomTableIllegalColumnWidth(childSourceLineNumbers, child.Name, "Width", width));
                                    }
                                    columnType = String.Concat(nullable ? "I" : "i", width);
                                }
                                else if ("CHAR" == typeName)
                                {
                                    string typeChar = localizable ? "l" : "s";
                                    columnType = String.Concat(nullable ? typeChar.ToUpper(CultureInfo.InvariantCulture) : typeChar.ToLower(CultureInfo.InvariantCulture), width);
                                }
                                else if ("OBJECT" == typeName)
                                {
                                    if ("Binary" != category)
                                    {
                                        this.core.OnMessage(WixErrors.ExpectedBinaryCategory(childSourceLineNumbers));
                                    }
                                    columnType = String.Concat(nullable ? "V" : "v", width);
                                }

                                foreach (XmlNode grandChild in child.ChildNodes)
                                {
                                    if (XmlNodeType.Element == grandChild.NodeType)
                                    {
                                        if (grandChild.NamespaceURI == this.schema.TargetNamespace)
                                        {
                                            this.core.UnexpectedElement(child, grandChild);
                                        }
                                        else
                                        {
                                            this.core.UnsupportedExtensionElement(child, grandChild);
                                        }
                                    }
                                }

                                columnNames = String.Concat(columnNames, null == columnNames ? String.Empty : "\t", columnName);
                                columnTypes = String.Concat(columnTypes, null == columnTypes ? String.Empty : "\t", columnType);
                                if (primaryKey)
                                {
                                    primaryKeys = String.Concat(primaryKeys, null == primaryKeys ? String.Empty : "\t", columnName);
                                }

                                minValues = String.Concat(minValues, null == minValues ? String.Empty : "\t", CompilerCore.LongNotSet != minValue ? minValue.ToString(CultureInfo.InvariantCulture) : String.Empty);
                                maxValues = String.Concat(maxValues, null == maxValues ? String.Empty : "\t", CompilerCore.LongNotSet != maxValue ? maxValue.ToString(CultureInfo.InvariantCulture) : String.Empty);
                                keyTables = String.Concat(keyTables, null == keyTables ? String.Empty : "\t", keyTable);
                                keyColumns = String.Concat(keyColumns, null == keyColumns ? String.Empty : "\t", CompilerCore.IntegerNotSet != keyColumn ? keyColumn.ToString(CultureInfo.InvariantCulture) : String.Empty);
                                categories = String.Concat(categories, null == categories ? String.Empty : "\t", category);
                                sets = String.Concat(sets, null == sets ? String.Empty : "\t", setValues);
                                descriptions = String.Concat(descriptions, null == descriptions ? String.Empty : "\t", description);
                                modularizations = String.Concat(modularizations, null == modularizations ? String.Empty : "\t", modularization);

                                break;
                            case "Row":
                                string dataValue = null;

                                foreach (XmlAttribute childAttrib in child.Attributes)
                                {
                                    this.core.UnexpectedAttribute(childSourceLineNumbers, childAttrib);
                                }

                                foreach (XmlNode data in child.ChildNodes)
                                {
                                    SourceLineNumberCollection dataSourceLineNumbers = Preprocessor.GetSourceLineNumbers(data);
                                    switch (data.LocalName)
                                    {
                                        case "Data":
                                            columnName = null;
                                            foreach (XmlAttribute dataAttrib in data.Attributes)
                                            {
                                                switch (dataAttrib.LocalName)
                                                {
                                                    case "Column":
                                                        columnName = this.core.GetAttributeValue(dataSourceLineNumbers, dataAttrib);
                                                        break;
                                                    default:
                                                        this.core.UnexpectedAttribute(dataSourceLineNumbers, dataAttrib);
                                                        break;
                                                }
                                            }

                                            if (null == columnName)
                                            {
                                                this.core.OnMessage(WixErrors.ExpectedAttribute(dataSourceLineNumbers, data.Name, "Column"));
                                            }

                                            dataValue = String.Concat(dataValue, null == dataValue ? String.Empty : "\t", columnName, ":", data.InnerText);
                                            break;
                                    }
                                }

                                this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixCustomTable", tableId);

                                if (!this.core.EncounteredError)
                                {
                                    Row rowRow = this.core.CreateRow(childSourceLineNumbers, "WixCustomRow");
                                    rowRow[0] = tableId;
                                    rowRow[1] = dataValue;
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (0 < columnCount)
            {
                if (null == primaryKeys || 0 == primaryKeys.Length)
                {
                    this.core.OnMessage(WixErrors.CustomTableMissingPrimaryKey(sourceLineNumbers));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "WixCustomTable");
                    row[0] = tableId;
                    row[1] = columnCount;
                    row[2] = columnNames;
                    row[3] = columnTypes;
                    row[4] = primaryKeys;
                    row[5] = minValues;
                    row[6] = maxValues;
                    row[7] = keyTables;
                    row[8] = keyColumns;
                    row[9] = categories;
                    row[10] = sets;
                    row[11] = descriptions;
                    row[12] = modularizations;
                }
            }
        }

        /// <summary>
        /// Parses a directory element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentId">Optional identifier of parent directory.</param>
        /// <param name="diskId">Disk id inherited from parent directory.</param>
        /// <param name="fileSource">Path to source file as of yet.</param>
        private void ParseDirectoryElement(XmlNode node, string parentId, int diskId, string fileSource)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            bool fileSourceAttribSet = false;
            string longName = null;
            string longSourceName = null;
            string name = null;
            string shortName = null;
            string shortSourceName = null;
            string sourceName = null;
            StringBuilder defaultDir = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "DiskId":
                            diskId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "FileSource":
                        case "src":
                            if (fileSourceAttribSet)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "FileSource", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "FileSource"));
                            }
                            fileSource = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            fileSourceAttribSet = true;
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "LongSource":
                            longSourceName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourceName", "ShortSourceName"));
                            break;
                        case "Name":
                            if ("." != attrib.Value) // treat "." as a null value and add it back as "." later
                            {
                                name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                                if ("PUT-APPLICATION-DIRECTORY-HERE" == name)
                                {
                                    this.core.OnMessage(WixWarnings.PlaceholderValue(sourceLineNumbers, node.Name, attrib.Name, name));
                                }
                            }
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "ShortSourceName":
                            shortSourceName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "SourceName":
                            if ("." == attrib.Value)
                            {
                                sourceName = attrib.Value;
                            }
                            else
                            {
                                sourceName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            else if (CompilerCore.IllegalIdentifier != id)
            {
                if (null == parentId && "TARGETDIR" != id)
                {
                    this.core.OnMessage(WixErrors.IllegalRootDirectory(sourceLineNumbers, id));
                }
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }

            if (null != shortSourceName && null != longSourceName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortSourceName", "LongSource"));
            }

            if (CompilerCore.IllegalEmptyAttributeValue != name && CompilerCore.IllegalEmptyAttributeValue != longName)
            {
                if (null == name && null != longName)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name", "LongName"));
                }
                else if (null != name)
                {
                    if (name == longName) // Name and LongName are identical
                    {
                        this.core.OnMessage(WixWarnings.DirectoryRedundantNames(sourceLineNumbers, node.Name, "Name", "LongName", name));
                    }
                    else if ("SourceDir" == name || CompilerCore.IsValidShortFilename(name, false))
                    {
                        if (null == shortName)
                        {
                            shortName = name;
                        }
                        else
                        {
                            this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                        }
                    }
                    else
                    {
                        if (null == longName)
                        {
                            longName = name;

                            // generate a short directory name
                            if (null == shortName)
                            {
                                shortName = CompilerCore.GenerateShortName(name, false, false, node.LocalName, parentId);
                            }
                        }
                        else
                        {
                            this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                        }
                    }
                }

                if (null == shortName && null == parentId)
                {
                    this.core.OnMessage(WixErrors.DirectoryRootWithoutName(sourceLineNumbers, node.Name, "Name"));
                }
            }

            if (CompilerCore.IllegalEmptyAttributeValue != sourceName && CompilerCore.IllegalEmptyAttributeValue != longSourceName)
            {
                if (null == sourceName && null != longSourceName)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceName", "LongSource"));
                }
                else if ("." == sourceName && null != longSourceName) // long source name cannot be specified with source name of "."
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "LongSourceName", longSourceName, "SourceName", sourceName));
                }
                else if (null != sourceName)
                {
                    if (sourceName == longSourceName) // SourceName and LongSource are identical
                    {
                        this.core.OnMessage(WixWarnings.DirectoryRedundantNames(sourceLineNumbers, node.Name, "SourceName", "LongSource", sourceName));
                    }
                    else if (CompilerCore.IsValidShortFilename(sourceName, false) || "." == sourceName)
                    {
                        if (null == shortSourceName)
                        {
                            shortSourceName = sourceName;
                        }
                        else
                        {
                            this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "SourceName", sourceName, "ShortSourceName"));
                        }
                    }
                    else
                    {
                        if (null == longSourceName)
                        {
                            longSourceName = sourceName;

                            // generate a short source directory name
                            if (null == shortSourceName)
                            {
                                shortSourceName = CompilerCore.GenerateShortName(sourceName, false, false, node.LocalName, parentId);
                            }
                        }
                        else
                        {
                            this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "SourceName", sourceName, "LongSourceName"));
                        }
                    }
                }
            }

            if (CompilerCore.IllegalEmptyAttributeValue != shortName && CompilerCore.IllegalEmptyAttributeValue != longName && CompilerCore.IllegalEmptyAttributeValue != shortSourceName && CompilerCore.IllegalEmptyAttributeValue != longSourceName &&
                shortName == shortSourceName && longName == longSourceName && (null != shortName || null != longName)) // source and target are identical
            {
                this.core.OnMessage(WixWarnings.DirectoryRedundantNames(sourceLineNumbers, node.Name, "SourceName", "LongSource"));
            }

            if (fileSourceAttribSet && !fileSource.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fileSource = String.Concat(fileSource, Path.DirectorySeparatorChar);
            }

            // track the src path
            if (!fileSourceAttribSet)
            {
                if (this.useShortFileNames)
                {
                    if (null != sourceName)
                    {
                        fileSource = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", fileSource, shortSourceName, Path.DirectorySeparatorChar);
                    }
                    else if (null != name)
                    {
                        fileSource = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", fileSource, shortName, Path.DirectorySeparatorChar);
                    }
                }
                else if (null != longSourceName)
                {
                    fileSource = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", fileSource, longSourceName, Path.DirectorySeparatorChar);
                }
                else
                {
                    if (null != sourceName)
                    {
                        fileSource = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", fileSource, shortSourceName, Path.DirectorySeparatorChar);
                    }
                    else if (null != longName)
                    {
                        fileSource = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", fileSource, longName, Path.DirectorySeparatorChar);
                    }
                    else if (null != name)
                    {
                        fileSource = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", fileSource, shortName, Path.DirectorySeparatorChar);
                    }
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Component":
                                this.ParseComponentElement(child, diskId, id, fileSource);
                                break;
                            case "Directory":
                                this.ParseDirectoryElement(child, id, diskId, fileSource);
                                break;
                            case "Merge":
                                this.ParseMergeElement(child, id);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, diskId.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    }
                }
            }

            // put back the "." if short name wasn't specified
            if (null == shortName)
            {
                // the Name and SourceName cannot both be "."
                if ("." == sourceName)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "SourceName", sourceName, "Name", "."));
                }

                shortName = ".";
            }

            // build the DefaultDir column
            defaultDir = new StringBuilder(GetMsiFilenameValue(shortName, longName));
            if (null != shortSourceName)
            {
                defaultDir.AppendFormat(":{0}", GetMsiFilenameValue(shortSourceName, longSourceName));
            }

            if ("TARGETDIR" == id && ("SourceDir" != defaultDir.ToString() && "SOURCEDIR" != defaultDir.ToString()))
            {
                this.core.OnMessage(WixErrors.IllegalTargetDirDefaultDir(sourceLineNumbers, defaultDir.ToString()));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Directory");
                row[0] = id;
                row[1] = parentId;
                row[2] = defaultDir.ToString();
            }
        }

        /// <summary>
        /// Parses a directory reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseDirectoryRefElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int diskId = CompilerCore.IntegerNotSet;
            string fileSource = String.Empty;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", id);
                            break;
                        case "DiskId":
                            diskId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "FileSource":
                        case "src":
                            if (0 != fileSource.Length)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "FileSource", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "FileSource"));
                            }
                            fileSource = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (!fileSource.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fileSource = String.Concat(fileSource, Path.DirectorySeparatorChar);
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Component":
                                this.ParseComponentElement(child, diskId, id, fileSource);
                                break;
                            case "Directory":
                                this.ParseDirectoryElement(child, id, diskId, fileSource);
                                break;
                            case "Merge":
                                this.ParseMergeElement(child, id);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, diskId.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    }
                }
            }
        }

        /// <summary>
        /// Parses a directory search element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentSignature">Signature of parent search element.</param>
        /// <returns>Signature of search element.</returns>
        private string ParseDirectorySearchElement(XmlNode node, string parentSignature)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int depth = CompilerCore.IntegerNotSet;
            string path = null;
            string signature = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Depth":
                            depth = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Path":
                            path = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            signature = id;

            bool oneChild = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                        switch (child.LocalName)
                        {
                            case "DirectorySearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchElement(child, id);
                                break;
                            case "DirectorySearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchRefElement(child, id);
                                break;
                            case "FileSearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseFileSearchElement(child, id);
                                id = signature; // FileSearch signatures override parent signatures
                                break;
                            case "FileSearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                id = this.ParseSimpleRefElement(child, "Signature"); // FileSearch signatures override parent signatures
                                signature = null;
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "DrLocator");
                row[0] = id;
                row[1] = parentSignature;
                row[2] = path;
                if (CompilerCore.IntegerNotSet != depth)
                {
                    row[3] = depth;
                }
            }

            return signature;
        }

        /// <summary>
        /// Parses a directory search reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentSignature">Signature of parent search element.</param>
        /// <returns>Signature of search element.</returns>
        private string ParseDirectorySearchRefElement(XmlNode node, string parentSignature)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string parent = null;
            string path = null;
            string signature = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Parent":
                            parent = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Path":
                            path = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            signature = id;

            if (null != parent && 0 < parent.Length)
            {
                if (null != parentSignature && 0 < parentSignature.Length)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, id, parent, parentSignature));
                }
                else
                {
                    parentSignature = parent;
                }
            }

            bool oneChild = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                        switch (child.LocalName)
                        {
                            case "DirectorySearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchElement(child, id);
                                break;
                            case "DirectorySearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchRefElement(child, id);
                                break;
                            case "FileSearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseFileSearchElement(child, id);
                                id = signature; // FileSearch signatures override parent signatures
                                break;
                            case "FileSearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                id = this.ParseSimpleRefElement(child, "Signature"); // FileSearch signatures override parent signatures
                                signature = null;
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "DrLocator", id, parentSignature, path);
            return signature;
        }

        /// <summary>
        /// Parses a feature element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentType">The type of parent.</param>
        /// <param name="parentId">Optional identifer for parent feature.</param>
        /// <param name="lastDisplay">Display value for last feature used to get the features to display in the same order as specified 
        /// in the source code.</param>
        private void ParseFeatureElement(XmlNode node, ComplexReferenceParentType parentType, string parentId, ref int lastDisplay)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string allowAdvertise = null;
            int bits = 0;
            string configurableDirectory = null;
            string description = null;
            string display = "collapse";
            YesNoType followParent = YesNoType.NotSet;
            string installDefault = null;
            int level = CompilerCore.IntegerNotSet;
            string title = null;
            string typicalDefault = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Absent":
                            string absent = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (absent)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "allow": // this is the default
                                    break;
                                case "disallow":
                                    bits = bits | MsiInterop.MsidbFeatureAttributesUIDisallowAbsent;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, absent, "allow", "disallow"));
                                    break;
                            }
                            break;
                        case "AllowAdvertise":
                            allowAdvertise = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (allowAdvertise)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "no":
                                    bits |= MsiInterop.MsidbFeatureAttributesDisallowAdvertise;
                                    break;
                                case "system":
                                    bits |= MsiInterop.MsidbFeatureAttributesNoUnsupportedAdvertise;
                                    break;
                                case "yes": // this is the default
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, allowAdvertise, "no", "system", "yes"));
                                    break;
                            }
                            break;
                        case "ConfigurableDirectory":
                            configurableDirectory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", configurableDirectory);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Display":
                            display = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "InstallDefault":
                            installDefault = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (installDefault)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "followParent":
                                    if (ComplexReferenceParentType.Product == parentType)
                                    {
                                        this.core.OnMessage(WixErrors.RootFeatureCannotFollowParent(sourceLineNumbers));
                                    }
                                    bits = bits | MsiInterop.MsidbFeatureAttributesFollowParent;
                                    break;
                                case "local": // this is the default
                                    break;
                                case "source":
                                    bits = bits | MsiInterop.MsidbFeatureAttributesFavorSource;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, installDefault, "followParent", "local", "source"));
                                    break;
                            }
                            break;
                        case "Level":
                            level = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Title":
                            title = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if ("PUT-FEATURE-TITLE-HERE" == title)
                            {
                                this.core.OnMessage(WixWarnings.PlaceholderValue(sourceLineNumbers, node.Name, attrib.Name, title));
                            }
                            break;
                        case "TypicalDefault":
                            typicalDefault = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (typicalDefault)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "advertise":
                                    bits = bits | MsiInterop.MsidbFeatureAttributesFavorAdvertise;
                                    break;
                                case "install": // this is the default
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, typicalDefault, "advertise", "install"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            else if (38 < id.Length)
            {
                this.core.OnMessage(WixErrors.FeatureNameTooLong(sourceLineNumbers, node.Name, "Id", id));
            }

            if (null != configurableDirectory && configurableDirectory.ToUpper(CultureInfo.InvariantCulture) != configurableDirectory)
            {
                this.core.OnMessage(WixErrors.FeatureConfigurableDirectoryNotUppercase(sourceLineNumbers, node.Name, "ConfigurableDirectory", configurableDirectory));
            }

            if (CompilerCore.IntegerNotSet == level)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Level"));
            }

            if ("advertise" == typicalDefault && "no" == allowAdvertise)
            {
                this.core.OnMessage(WixErrors.FeatureCannotFavorAndDisallowAdvertise(sourceLineNumbers, node.Name, "TypicalDefault", typicalDefault, "AllowAdvertise", allowAdvertise));
            }

            if (YesNoType.Yes == followParent && ("local" == installDefault || "source" == installDefault))
            {
                this.core.OnMessage(WixErrors.FeatureCannotFollowParentAndFavorLocalOrSource(sourceLineNumbers, node.Name, "InstallDefault", "FollowParent", "yes"));
            }

            int childDisplay = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ComponentGroupRef":
                                this.ParseComponentGroupRefElement(child, ComplexReferenceParentType.Feature, id, null);
                                break;
                            case "ComponentRef":
                                this.ParseComponentRefElement(child, ComplexReferenceParentType.Feature, id, null);
                                break;
                            case "Condition":
                                this.ParseConditionElement(child, node.LocalName, id, null);
                                break;
                            case "Feature":
                                this.ParseFeatureElement(child, ComplexReferenceParentType.Feature, id, ref childDisplay);
                                break;
                            case "FeatureGroupRef":
                                this.ParseFeatureGroupRefElement(child, ComplexReferenceParentType.Feature, id);
                                break;
                            case "FeatureRef":
                                this.ParseFeatureRefElement(child, ComplexReferenceParentType.Feature, id);
                                break;
                            case "MergeRef":
                                this.ParseMergeRefElement(child, id);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Feature");
                row[0] = id;
                row[1] = null; // this column is set in the linker
                row[2] = title;
                row[3] = description;
                switch (display)
                {
                    case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                        break;
                    case "collapse":
                        lastDisplay = (lastDisplay | 1) + 1;
                        row[4] = lastDisplay;
                        break;
                    case "expand":
                        lastDisplay = (lastDisplay + 1) | 1;
                        row[4] = lastDisplay;
                        break;
                    case "hidden":
                        row[4] = 0;
                        break;
                    default:
                        try
                        {
                            row[4] = Convert.ToInt32(display, CultureInfo.InvariantCulture);

                            // save the display value of this row (if its not hidden) for subsequent rows
                            if (0 != (int)row[4])
                            {
                                lastDisplay = (int)row[4];
                            }
                        }
                        catch (Exception)
                        {
                            this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Display", display, "collapse", "expand", "hidden"));
                        }
                        break;
                }
                row[5] = level;
                row[6] = configurableDirectory;
                row[7] = bits;

                if (ComplexReferenceParentType.Unknown != parentType)
                {
                    this.core.CreateWixComplexReferenceRow(sourceLineNumbers, parentType, parentId, null, ComplexReferenceChildType.Feature, id, false);
                }
            }
        }

        /// <summary>
        /// Parses a feature reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentType">The type of parent.</param>
        /// <param name="parentId">Optional identifier for parent feature.</param>
        private void ParseFeatureRefElement(XmlNode node, ComplexReferenceParentType parentType, string parentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Feature", id);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            int lastDisplay = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ComponentGroupRef":
                                this.ParseComponentGroupRefElement(child, ComplexReferenceParentType.Feature, id, null);
                                break;
                            case "ComponentRef":
                                this.ParseComponentRefElement(child, ComplexReferenceParentType.Feature, id, null);
                                break;
                            case "Feature":
                                this.ParseFeatureElement(child, ComplexReferenceParentType.Feature, id, ref lastDisplay);
                                break;
                            case "FeatureRef":
                                this.ParseFeatureRefElement(child, ComplexReferenceParentType.Feature, id);
                                break;
                            case "MergeRef":
                                this.ParseMergeRefElement(child, id);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                if (ComplexReferenceParentType.Unknown != parentType)
                {
                    this.core.CreateWixComplexReferenceRow(sourceLineNumbers, parentType, parentId, null, ComplexReferenceChildType.Feature, id, false);
                }
            }
        }

        /// <summary>
        /// Parses a feature group element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseFeatureGroupElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            int lastDisplay = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ComponentGroupRef":
                                this.ParseComponentGroupRefElement(child, ComplexReferenceParentType.Group, id, null);
                                break;
                            case "ComponentRef":
                                this.ParseComponentRefElement(child, ComplexReferenceParentType.Group, id, null);
                                break;
                            case "Feature":
                                this.ParseFeatureElement(child, ComplexReferenceParentType.Group, id, ref lastDisplay);
                                break;
                            case "FeatureGroupRef":
                                this.ParseFeatureGroupRefElement(child, ComplexReferenceParentType.Group, id);
                                break;
                            case "FeatureRef":
                                this.ParseFeatureRefElement(child, ComplexReferenceParentType.Group, id);
                                break;
                            case "MergeRef":
                                this.ParseMergeRefElement(child, id);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixFeatureGroup");
                row[0] = id;
            }
        }

        /// <summary>
        /// Parses a feature group reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentType">The type of parent.</param>
        /// <param name="parentId">Identifier of parent element.</param>
        private void ParseFeatureGroupRefElement(XmlNode node, ComplexReferenceParentType parentType, string parentId)
        {
            Debug.Assert(ComplexReferenceParentType.Feature == parentType || ComplexReferenceParentType.Group == parentType || ComplexReferenceParentType.Product == parentType);

            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            YesNoType primary = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixFeatureGroup", id);
                            break;
                        case "Primary":
                            primary = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.CreateWixComplexReferenceRow(sourceLineNumbers, parentType, parentId, null, ComplexReferenceChildType.Group, id, (YesNoType.Yes == primary));
        }

        /// <summary>
        /// Parses an environment element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseEnvironmentElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string action = null;
            string name = null;
            string part = null;
            bool permanent = false;
            string separator = ";"; // default to ';'
            bool system = false;
            string text = null;
            string uninstall = "-"; // default to remove at uninstall

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            string value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (value)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "create":
                                    action = "+";
                                    break;
                                case "set":
                                    action = "=";
                                    break;
                                case "remove":
                                    action = "!";
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, value, "create", "set", "remove"));
                                    break;
                            }
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Part":
                            part = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Permanent":
                            permanent = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Separator":
                            separator = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "System":
                            system = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            text = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (null != part)
            {
                if ("+" == action)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Part", "Action", "create"));
                }

                switch (part)
                {
                    case CompilerCore.IllegalEmptyAttributeValue:
                        break;
                    case "all":
                        break;
                    case "first":
                        text = String.Concat(text, separator, "[~]");
                        break;
                    case "last":
                        text = String.Concat("[~]", separator, text);
                        break;
                    default:
                        this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Part", part, "all", "first", "last"));
                        break;
                }
            }

            if (permanent)
            {
                uninstall = null;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Environment");
                row[0] = id;
                row[1] = String.Concat(action, uninstall, system ? "*" : String.Empty, name);
                row[2] = text;
                row[3] = componentId;
            }
        }

        /// <summary>
        /// Parses an error element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseErrorElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int id = CompilerCore.IntegerNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (CompilerCore.IntegerNotSet == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                id = CompilerCore.IllegalInteger;
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Error");
                row[0] = id;
                row[1] = node.InnerText;
            }
        }

        /// <summary>
        /// Parses an extension element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="advertise">Flag if this extension is advertised.</param>
        /// <param name="progId">ProgId for extension.</param>
        private void ParseExtensionElement(XmlNode node, string componentId, YesNoType advertise, string progId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string extension = null;
            string mime = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            extension = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Advertise":
                            YesNoType extensionAdvertise = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if ((YesNoType.No == advertise && YesNoType.Yes == extensionAdvertise) || (YesNoType.Yes == advertise && YesNoType.No == extensionAdvertise))
                            {
                                this.core.OnMessage(WixErrors.AdvertiseStateMustMatch(sourceLineNumbers, extensionAdvertise.ToString(CultureInfo.InvariantCulture.NumberFormat), advertise.ToString(CultureInfo.InvariantCulture.NumberFormat)));
                            }
                            advertise = extensionAdvertise;
                            break;
                        case "ContentType":
                            mime = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (YesNoType.NotSet == advertise)
            {
                advertise = YesNoType.No;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Verb":
                                this.ParseVerbElement(child, extension, progId, componentId, advertise);
                                break;
                            case "MIME":
                                string newMime = this.ParseMIMEElement(child, extension, componentId, advertise);
                                if (null != newMime && null == mime)
                                {
                                    mime = newMime;
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (YesNoType.Yes == advertise)
            {
                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "Extension");
                    row[0] = extension;
                    row[1] = componentId;
                    row[2] = progId;
                    row[3] = mime;
                    row[4] = Guid.Empty.ToString("B");
                }
            }
            else if (YesNoType.No == advertise)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(".", extension), String.Empty, progId, componentId); // Extension
                if (null != mime)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(".", extension), "Content Type", mime, componentId); // Extension's MIME ContentType
                }
            }
        }

        /// <summary>
        /// Parses a file element.
        /// </summary>
        /// <param name="node">File element to parse.</param>
        /// <param name="componentId">Parent's component id.</param>
        /// <param name="directoryId">Ancestor's directory id.</param>
        /// <param name="diskId">Disk id inherited from parent component.</param>
        /// <param name="sourcePath">Default source path of parent directory.</param>
        /// <param name="possibleKeyPath">This will be set with the possible keyPath for the parent component.</param>
        /// <param name="win64Component">true if the component is 64-bit.</param>
        /// <returns>Returns true if this file is the key path.</returns>
        private bool ParseFileElement(XmlNode node, string componentId, string directoryId, int diskId, string sourcePath, out string possibleKeyPath, bool win64Component)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int assemblyAttributes = CompilerCore.IntegerNotSet;
            string assemblyApplication = null;
            string assemblyManifest = null;
            string bindPath = null;
            int bits = 0;
            string companionFile = null;
            string defaultLanguage = null;
            int defaultSize = 0;
            string defaultVersion = null;
            string fontTitle = null;
            bool generatedShortFileName = false;
            bool keyPath = false;
            string longName = null;
            string name = null;
            int patchGroup = CompilerCore.IntegerNotSet;
            string procArch = null;
            int selfRegCost = CompilerCore.IntegerNotSet;
            string shortName = null;
            string source = sourcePath;   // assume we'll use the parents as the source for this file
            bool sourceSet = false;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Assembly":
                            string assemblyValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (assemblyValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case ".net":
                                    assemblyAttributes = 0;
                                    break;
                                case "no":
                                    break;
                                case "win32":
                                    assemblyAttributes = 1;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, "File", "Assembly", assemblyValue, "no", "win32", ".net"));
                                    break;
                            }
                            break;
                        case "AssemblyApplication":
                            assemblyApplication = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", assemblyApplication);
                            break;
                        case "AssemblyManifest":
                            assemblyManifest = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", assemblyManifest);
                            break;
                        case "BindPath":
                            bindPath = this.core.GetAttributeValue(sourceLineNumbers, attrib, true);
                            break;
                        case "Checksum":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbFileAttributesChecksum;
                            }
                            break;
                        case "CompanionFile":
                            companionFile = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", companionFile);
                            break;
                        case "Compressed":
                            YesNoDefaultType compressed = this.core.GetAttributeYesNoDefaultValue(sourceLineNumbers, attrib);
                            if (YesNoDefaultType.Yes == compressed)
                            {
                                bits |= MsiInterop.MsidbFileAttributesCompressed;
                            }
                            else if (YesNoDefaultType.No == compressed)
                            {
                                bits |= MsiInterop.MsidbFileAttributesNoncompressed;
                            }
                            break;
                        case "DefaultLanguage":
                            defaultLanguage = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DefaultSize":
                            defaultSize = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "DefaultVersion":
                            defaultVersion = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DiskId":
                            diskId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "FontTitle":
                            fontTitle = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Hidden":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbFileAttributesHidden;
                            }
                            break;
                        case "KeyPath":
                            keyPath = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "Name":
                            name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "PatchGroup":
                            patchGroup = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, int.MaxValue);
                            break;
                        case "ProcessorArchitecture":
                            string procArchValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (procArchValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "msil":
                                    procArch = "MSIL";
                                    break;
                                case "x86":
                                    procArch = "x86";
                                    break;
                                case "x64":
                                    procArch = "amd64";
                                    break;
                                case "ia64":
                                    procArch = "ia64";
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, "File", "ProcessorArchitecture", procArchValue, "msil", "x86", "x64", "ia64"));
                                    break;
                            }
                            break;
                        case "ReadOnly":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbFileAttributesReadOnly;
                            }
                            break;
                        case "SelfRegCost":
                            selfRegCost = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "Source":
                        case "src":
                            if (sourceSet)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Source", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "Source"));
                            }
                            source = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            sourceSet = true;
                            break;
                        case "System":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbFileAttributesSystem;
                            }
                            break;
                        case "TrueType":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                fontTitle = String.Empty;
                            }
                            break;
                        case "Vital":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbFileAttributesVital;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null != companionFile)
            {
                // the companion file cannot be the key path of a component
                if (keyPath)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "CompanionFile", "KeyPath", "yes"));
                }
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }

            if (null == name)
            {
                name = id;
            }

            if (CompilerCore.IsValidShortFilename(name, false))
            {
                if (null == shortName)
                {
                    shortName = name;
                }
                else
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                }
            }
            else
            {
                if (null == longName)
                {
                    longName = name;

                    // generate a short file name
                    if (null == shortName)
                    {
                        shortName = CompilerCore.GenerateShortName(name, true, false, node.LocalName, componentId);
                        generatedShortFileName = true;
                    }
                }
                else
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                }
            }

            if (this.compilingModule && CompilerCore.IntegerNotSet != diskId)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeInMergeModule(sourceLineNumbers, node.Name, "DiskId"));
            }
            else if (!this.compilingModule && CompilerCore.IntegerNotSet == diskId)
            {
                diskId = 1; // default to first Media
            }

            if (null != defaultVersion && null != companionFile)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "DefaultVersion", "CompanionFile", companionFile));
            }

            if (CompilerCore.IntegerNotSet == assemblyAttributes)
            {
                if (null != assemblyManifest)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Assembly", "AssemblyManifest"));
                }

                if (null != assemblyApplication)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Assembly", "AssemblyApplication"));
                }
            }
            else
            {
                if (!keyPath)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeValueWithoutOtherAttribute(sourceLineNumbers, node.Name, "Assembly", (0 == assemblyAttributes ? ".net" : "win32"), "KeyPath", "yes"));
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "AppId":
                                this.ParseAppIdElement(child, componentId, YesNoType.NotSet, id, null, null);
                                break;
                            case "AssemblyName":
                                this.ParseAssemblyName(child, componentId);
                                break;
                            case "Class":
                                this.ParseClassElement(child, componentId, YesNoType.NotSet, id, null, null, null);
                                break;
                            case "CopyFile":
                                this.ParseCopyFileElement(child, componentId, id);
                                break;
                            case "ODBCDriver":
                                this.ParseODBCDriverOrTranslator(child, componentId, id, this.tableDefinitions["ODBCDriver"]);
                                break;
                            case "ODBCTranslator":
                                this.ParseODBCDriverOrTranslator(child, componentId, id, this.tableDefinitions["ODBCTranslator"]);
                                break;
                            case "Permission":
                                this.ParsePermissionElement(child, id, "File");
                                break;
                            case "Shortcut":
                                this.ParseShortcutElement(child, componentId, node.LocalName, id);
                                break;
                            case "TypeLib":
                                this.ParseTypeLibElement(child, componentId, id, win64Component);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, componentId);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                // if source relies on parent directories, append the file name
                if (source.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    if (!this.useShortFileNames && null != longName)
                    {
                        source = Path.Combine(source, longName);
                    }
                    else
                    {
                        source = Path.Combine(source, shortName);
                    }
                }

                FileRow fileRow = (FileRow)this.core.CreateRow(sourceLineNumbers, "File");
                fileRow[0] = id;
                fileRow[1] = componentId;
                fileRow[2] = GetMsiFilenameValue(shortName, longName);
                fileRow[3] = defaultSize;
                if (null != companionFile)
                {
                    fileRow[4] = companionFile;
                }
                else if (null != defaultVersion)
                {
                    fileRow[4] = defaultVersion;
                }
                fileRow[5] = defaultLanguage;
                fileRow[6] = bits;

                // the Sequence row is set in the binder

                Row wixFileRow = this.core.CreateRow(sourceLineNumbers, "WixFile");
                wixFileRow[0] = id;
                if (CompilerCore.IntegerNotSet != assemblyAttributes)
                {
                    wixFileRow[1] = assemblyAttributes;
                }
                wixFileRow[2] = assemblyManifest;
                wixFileRow[3] = assemblyApplication;
                wixFileRow[4] = directoryId;
                if (CompilerCore.IntegerNotSet != diskId)
                {
                    wixFileRow[5] = diskId;
                }
                wixFileRow[6] = source;
                wixFileRow[7] = procArch;
                wixFileRow[8] = (CompilerCore.IntegerNotSet != patchGroup ? patchGroup : -1);
                wixFileRow[9] = (generatedShortFileName ? 0x1 : 0x0);

                if (CompilerCore.IntegerNotSet != assemblyAttributes)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiAssembly");
                    row[0] = componentId;
                    row[1] = Guid.Empty.ToString("B");
                    row[2] = assemblyManifest;
                    row[3] = assemblyApplication;
                    row[4] = assemblyAttributes;
                }

                if (null != bindPath)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "BindImage");
                    row[0] = id;
                    row[1] = bindPath;

                    // TODO: technically speaking each of the properties in the "bindPath" should be added as references, but how much do we really care about BindImage?
                }

                if (CompilerCore.IntegerNotSet != selfRegCost)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "SelfReg");
                    row[0] = id;
                    row[1] = selfRegCost;
                }

                if (null != fontTitle)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "Font");
                    row[0] = id;
                    row[1] = fontTitle;
                }
            }

            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Media", diskId.ToString(CultureInfo.InvariantCulture.NumberFormat));

            possibleKeyPath = id;
            return keyPath;
        }

        /// <summary>
        /// Parses a file search element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentSignature">Signature of parent search element.</param>
        /// <returns>Signature of search element.</returns>
        private string ParseFileSearchElement(XmlNode node, string parentSignature)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string languages = null;
            string longName = null;
            int minDate = CompilerCore.IntegerNotSet;
            int maxDate = CompilerCore.IntegerNotSet;
            string maxSize = null;
            string minSize = null;
            string maxVersion = null;
            string minVersion = null;
            string name = null;
            string shortName = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "Name":
                            name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "MinVersion":
                            minVersion = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MaxVersion":
                            maxVersion = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MinSize":
                            minSize = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MaxSize":
                            maxSize = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MinDate":
                            minDate = this.core.GetAttributeDateTimeValue(sourceLineNumbers, attrib);
                            break;
                        case "MaxDate":
                            maxDate = this.core.GetAttributeDateTimeValue(sourceLineNumbers, attrib);
                            break;
                        case "Languages":
                            languages = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                if (null == parentSignature)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                }

                id = parentSignature;
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            // also, using both ShortName and LongName will not always work due to a Windows Installer bug
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }
            else if (null != shortName && null != name)
            {
                this.core.OnMessage(WixWarnings.FileSearchFileNameIssue(sourceLineNumbers, node.Name, "ShortName", "Name"));
            }
            else if (null != name && null != longName)
            {
                this.core.OnMessage(WixWarnings.FileSearchFileNameIssue(sourceLineNumbers, node.Name, "Name", "LongName"));
            }

            // at least one name must be specified
            if (null == shortName && null == name && null == longName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (null != name && CompilerCore.IllegalEmptyAttributeValue != name)
            {
                if (CompilerCore.IsValidShortFilename(name, false))
                {
                    if (null == shortName)
                    {
                        shortName = name;
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                    }
                }
                else if (!CompilerCore.IsValidLocIdentifier(name))
                {
                    if (null == longName)
                    {
                        longName = name;
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                    }
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Signature");
                row[0] = id;
                if (null != shortName && null != longName)
                {
                    row[1] = GetMsiFilenameValue(shortName, longName);
                }
                else
                {
                    row[1] = (null != longName ? longName : shortName);
                }
                row[2] = minVersion;
                row[3] = maxVersion;
                row[4] = minSize;
                row[5] = maxSize;
                if (CompilerCore.IntegerNotSet != minDate)
                {
                    row[6] = minDate;
                }

                if (CompilerCore.IntegerNotSet != maxDate)
                {
                    row[7] = maxDate;
                }
                row[8] = languages;
            }

            return id; // the id of the FileSearch element is its signature
        }

        /// <summary>
        /// Parses a fragment element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseFragmentElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            this.activeName = null;
            this.activeLanguage = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // NOTE: Id is not required for Fragments, this is a departure from the normal run of the mill processing.

            this.core.CreateActiveSection(id, SectionType.Fragment, 0);

            int featureDisplay = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "_locDefinition":
                                break;
                            case "AdminExecuteSequence":
                            case "AdminUISequence":
                            case "AdvertiseExecuteSequence":
                            case "InstallExecuteSequence":
                            case "InstallUISequence":
                                this.ParseSequenceElement(child, child.LocalName);
                                break;
                            case "AppId":
                                this.ParseAppIdElement(child, null, YesNoType.Yes, null, null, null);
                                break;
                            case "Binary":
                                this.ParseBinaryElement(child);
                                break;
                            case "ComplianceCheck":
                                this.ParseComplianceCheckElement(child);
                                break;
                            case "ComponentGroup":
                                this.ParseComponentGroupElement(child);
                                break;
                            case "Condition":
                                this.ParseConditionElement(child, node.LocalName, null, null);
                                break;
                            case "CustomAction":
                                this.ParseCustomActionElement(child);
                                break;
                            case "CustomActionRef":
                                this.ParseSimpleRefElement(child, "CustomAction");
                                break;
                            case "CustomTable":
                                this.ParseCustomTableElement(child);
                                break;
                            case "Directory":
                                this.ParseDirectoryElement(child, null, CompilerCore.IntegerNotSet, String.Empty);
                                break;
                            case "DirectoryRef":
                                this.ParseDirectoryRefElement(child);
                                break;
                            case "EnsureTable":
                                this.ParseEnsureTableElement(child);
                                break;
                            case "Feature":
                                this.ParseFeatureElement(child, ComplexReferenceParentType.Unknown, null, ref featureDisplay);
                                break;
                            case "FeatureGroup":
                                this.ParseFeatureGroupElement(child);
                                break;
                            case "FeatureRef":
                                this.ParseFeatureRefElement(child, ComplexReferenceParentType.Unknown, null);
                                break;
                            case "Icon":
                                this.ParseIconElement(child);
                                break;
                            case "IgnoreModularization":
                                this.ParseIgnoreModularizationElement(child);
                                break;
                            case "Media":
                                this.ParseMediaElement(child, null);
                                break;
                            case "PatchCertificates":
                                this.ParsePatchCertificatesElement(child);
                                break;
                            case "PatchFamily":
                                this.ParsePatchFamilyElement(child);
                                break;
                            case "Property":
                                this.ParsePropertyElement(child);
                                break;
                            case "PropertyRef":
                                this.ParseSimpleRefElement(child, "Property");
                                break;
                            case "SFPCatalog":
                                string parentName = null;
                                this.ParseSFPCatalogElement(child, ref parentName);
                                break;
                            case "UI":
                                this.ParseUIElement(child);
                                break;
                            case "UIRef":
                                this.ParseSimpleRefElement(child, "WixUI");
                                break;
                            case "Upgrade":
                                this.ParseUpgradeElement(child);
                                break;
                            case "WixVariable":
                                this.ParseWixVariableElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError && null != id)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixFragment");
                row[0] = id;
            }
        }

        /// <summary>
        /// Parses a condition element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentElementLocalName">LocalName of the parent element.</param>
        /// <param name="id">Id of the parent element.</param>
        /// <param name="dialog">Dialog of the parent element if its a Control.</param>
        /// <returns>The condition if one was found.</returns>
        private string ParseConditionElement(XmlNode node, string parentElementLocalName, string id, string dialog)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string action = null;
            string condition = null;
            int level = CompilerCore.IntegerNotSet;
            string message = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Action":
                            if ("Control" == parentElementLocalName)
                            {
                                action = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                                switch (action)
                                {
                                    case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                        break;
                                    case "default":
                                    case "disable":
                                    case "enable":
                                    case "hide":
                                    case "show":
                                        action = Compiler.UppercaseFirstChar(action);
                                        break;
                                    default:
                                        this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, action, "default", "disable", "enable", "hide", "show"));
                                        break;
                                }
                            }
                            else
                            {
                                this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            }
                            break;
                        case "Level":
                            if ("Feature" == parentElementLocalName)
                            {
                                level = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            }
                            else
                            {
                                this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            }
                            break;
                        case "Message":
                            if ("Fragment" == parentElementLocalName || "Product" == parentElementLocalName)
                            {
                                message = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            }
                            else
                            {
                                this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // get the condition from the inner text of the element
            condition = CompilerCore.GetConditionInnerText(node);

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // the condition should not be empty
            if (null == condition || 0 == condition.Length)
            {
                condition = null;
                this.core.OnMessage(WixErrors.ConditionExpected(sourceLineNumbers, node.Name));
            }

            switch (parentElementLocalName)
            {
                case "Control":
                    if (null == action)
                    {
                        this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Action"));
                    }

                    if (!this.core.EncounteredError)
                    {
                        Row row = this.core.CreateRow(sourceLineNumbers, "ControlCondition");
                        row[0] = dialog;
                        row[1] = id;
                        row[2] = action;
                        row[3] = condition;
                    }
                    break;
                case "Feature":
                    if (CompilerCore.IntegerNotSet == level)
                    {
                        this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Level"));
                        level = CompilerCore.IllegalInteger;
                    }

                    if (!this.core.EncounteredError)
                    {
                        Row row = this.core.CreateRow(sourceLineNumbers, "Condition");
                        row[0] = id;
                        row[1] = level;
                        row[2] = condition;
                    }
                    break;
                case "Fragment":
                case "Product":
                    if (null == message)
                    {
                        this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Message"));
                    }

                    if (!this.core.EncounteredError)
                    {
                        Row row = this.core.CreateRow(sourceLineNumbers, "LaunchCondition");
                        row[0] = condition;
                        row[1] = message;
                    }
                    break;
            }

            return condition;
        }

        /// <summary>
        /// Parses a IniFile element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of the parent component.</param>
        private void ParseIniFileElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int action = CompilerCore.IntegerNotSet;
            string directory = null;
            string key = null;
            string longName = null;
            string name = null;
            string section = null;
            string shortName = null;
            string tableName = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            string actionValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (actionValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "addLine":
                                    action = MsiInterop.MsidbIniFileActionAddLine;
                                    break;
                                case "addTag":
                                    action = MsiInterop.MsidbIniFileActionAddTag;
                                    break;
                                case "createLine":
                                    action = MsiInterop.MsidbIniFileActionCreateLine;
                                    break;
                                case "removeLine":
                                    action = MsiInterop.MsidbIniFileActionRemoveLine;
                                    break;
                                case "removeTag":
                                    action = MsiInterop.MsidbIniFileActionRemoveTag;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Action", actionValue, "addLine", "addTag", "createLine", "removeLine", "removeTag"));
                                    break;
                            }
                            break;
                        case "Directory":
                            directory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "Name":
                            name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "Section":
                            section = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (CompilerCore.IntegerNotSet == action)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Action"));
                action = CompilerCore.IllegalInteger;
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }
            else if (CompilerCore.IllegalEmptyAttributeValue != name)
            {
                if (CompilerCore.IsValidShortFilename(name, false))
                {
                    if (null == shortName)
                    {
                        shortName = name;
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                    }
                }
                else
                {
                    if (null == longName)
                    {
                        longName = name;

                        // generate a short file name
                        if (null == shortName)
                        {
                            shortName = CompilerCore.GenerateShortName(name, true, false, node.LocalName, componentId);
                        }
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                    }
                }
            }

            if (null == section)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Section"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (MsiInterop.MsidbIniFileActionRemoveLine == action || MsiInterop.MsidbIniFileActionRemoveTag == action)
            {
                tableName = "RemoveIniFile";
            }
            else
            {
                if (null == value)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
                }

                tableName = "IniFile";
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, tableName);
                row[0] = id;
                row[1] = GetMsiFilenameValue(shortName, longName);
                row[2] = directory;
                row[3] = section;
                row[4] = key;
                row[5] = value;
                row[6] = action;
                row[7] = componentId;
            }
        }

        /// <summary>
        /// Parses an IniFile search element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Signature for search element.</returns>
        private string ParseIniFileSearchElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int field = CompilerCore.IntegerNotSet;
            string key = null;
            string longName = null;
            string name = null;
            string section = null;
            string shortName = null;
            string signature = null;
            int type = 1; // default is file

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Field":
                            field = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "Name":
                            name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "Section":
                            section = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "Type":
                            string typeValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (typeValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "directory":
                                    type = 0;
                                    break;
                                case "file":
                                    type = 1;
                                    break;
                                case "raw":
                                    type = 2;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Type", typeValue, "directory", "file", "registry"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }
            else if (CompilerCore.IllegalEmptyAttributeValue != name)
            {
                if (CompilerCore.IsValidShortFilename(name, false))
                {
                    if (null == shortName)
                    {
                        shortName = name;
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                    }
                }
                else
                {
                    if (null == longName)
                    {
                        longName = name;

                        // generate a short file name
                        if (null == shortName)
                        {
                            shortName = CompilerCore.GenerateShortName(name, true, false, node.LocalName, id);
                        }
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                    }
                }
            }

            if (null == section)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Section"));
            }

            signature = id;

            bool oneChild = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                        switch (child.LocalName)
                        {
                            case "DirectorySearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;

                                // directorysearch parentage should work like directory element, not the rest of the signature type because of the DrLocator.Parent column
                                signature = this.ParseDirectorySearchElement(child, id);
                                break;
                            case "DirectorySearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(childSourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseDirectorySearchRefElement(child, id);
                                break;
                            case "FileSearch":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                signature = this.ParseFileSearchElement(child, id);
                                id = signature; // FileSearch signatures override parent signatures
                                break;
                            case "FileSearchRef":
                                if (oneChild)
                                {
                                    this.core.OnMessage(WixErrors.TooManySearchElements(sourceLineNumbers, node.Name));
                                }
                                oneChild = true;
                                id = this.ParseSimpleRefElement(child, "Signature"); // FileSearch signatures override parent signatures
                                signature = null;
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "IniLocator");
                row[0] = id;
                row[1] = GetMsiFilenameValue(shortName, longName);
                row[2] = section;
                row[3] = key;
                if (CompilerCore.IntegerNotSet != field)
                {
                    row[4] = field;
                }
                row[5] = type;
            }

            return signature;
        }

        /// <summary>
        /// Parses an isolated component element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseIsolateComponentElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string shared = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Shared":
                            shared = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Component", shared);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == shared)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Shared"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "IsolatedComponent");
                row[0] = shared;
                row[1] = componentId;
            }
        }

        /// <summary>
        /// Parses a PatchCertificates element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        private void ParsePatchCertificatesElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);

            // no attributes are supported for this element
            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "DigitalCertificate":
                                string name = this.ParseDigitalCertificateElement(child);

                                if (!this.core.EncounteredError)
                                {
                                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchCertificate");
                                    row[0] = name;
                                    row[1] = name;
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }
        }

        /// <summary>
        /// Parses an digital certificate element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>The identifier of the certificate.</returns>
        private string ParseDigitalCertificateElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string sourceFile = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceFile":
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            else if (CompilerCore.IllegalEmptyAttributeValue != id)
            {
                if (40 < id.Length)
                {
                    this.core.OnMessage(WixErrors.StreamNameTooLong(sourceLineNumbers, node.Name, "Id", id, id.Length, 40));
                }

                // no need to check for modularization problems since DigitalSignature and thus DigitalCertificate
                // currently have no usage in merge modules
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "MsiDigitalCertificate");
                row[0] = id;
                row[1] = sourceFile;
            }

            return id;
        }

        /// <summary>
        /// Parses an digital signature element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="diskId">Disk id inherited from parent media.</param>
        private void ParseDigitalSignatureElement(XmlNode node, string diskId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string certificateId = null;
            string sourceFile = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "SourceFile":
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // sanity check for debug to ensure the stream name will not be a problem
            if (null != sourceFile)
            {
                Debug.Assert(62 >= "MsiDigitalSignature.Media.".Length + diskId.Length);
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "DigitalCertificate":
                                certificateId = this.ParseDigitalCertificateElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null == certificateId)
            {
                this.core.OnMessage(WixErrors.ExpectedElement(sourceLineNumbers, node.Name, "DigitalCertificate"));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "MsiDigitalSignature");
                row[0] = "Media";
                row[1] = diskId;
                row[2] = certificateId;
                row[3] = sourceFile;
            }
        }

        /// <summary>
        /// Parses a media element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="patchId">Set to the PatchId if parsing Patch/Media element otherwise null.</param>
        private void ParseMediaElement(XmlNode node, string patchId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int id = CompilerCore.IntegerNotSet;
            string cabinet = null;
            string compressionLevel = null; // this defaults to mszip in MediaRow
            string diskPrompt = null;
            string layout = null;
            bool patch = null != patchId;
            string volumeLabel = null;
            string source = null;

            YesNoType embedCab = patch ? YesNoType.Yes : YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "Cabinet":
                            cabinet = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "CompressionLevel":
                            compressionLevel = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (compressionLevel)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "high":
                                case "low":
                                case "medium":
                                case "mszip":
                                case "none":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, compressionLevel, "high", "low", "medium", "mszip", "none"));
                                    break;
                            }
                            break;
                        case "DiskPrompt":
                            diskPrompt = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Property", "DiskPrompt"); // ensure the output has a DiskPrompt Property defined
                            break;
                        case "EmbedCab":
                            embedCab = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Layout":
                        case "src":
                            if (null != layout)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Layout", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "Layout"));
                            }
                            layout = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "VolumeLabel":
                            volumeLabel = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Source":
                            source = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (CompilerCore.IntegerNotSet == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                id = CompilerCore.IllegalInteger;
            }

            if (YesNoType.IllegalValue != embedCab)
            {
                if (YesNoType.Yes == embedCab)
                {
                    if (null == cabinet)
                    {
                        this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Cabinet", "EmbedCab", "yes"));
                    }
                    else
                    {
                        if (62 < cabinet.Length)
                        {
                            this.core.OnMessage(WixErrors.MediaEmbeddedCabinetNameTooLong(sourceLineNumbers, node.Name, "Cabinet", cabinet, cabinet.Length));
                        }
                        cabinet = String.Concat("#", cabinet);
                    }
                }
                else // external cabinet file
                {
                    // external cabinet files must use 8.3 filenames
                    if (null != cabinet && CompilerCore.IllegalEmptyAttributeValue != cabinet && !CompilerCore.IsValidShortFilename(cabinet, false))
                    {
                        this.core.OnMessage(WixWarnings.MediaExternalCabinetFilenameIllegal(sourceLineNumbers, node.Name, "Cabinet", cabinet));
                    }
                }
            }

            if (null != compressionLevel && null == cabinet)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Cabinet", "CompressionLevel"));
            }

            if (patch)
            {
                // Default Source to a form of the Patch Id if none is specified.
                if (null == source)
                {
                    source = String.Concat("_", new Guid(patchId).ToString("N", CultureInfo.InvariantCulture).ToUpper());
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);

                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "DigitalSignature":
                                if (YesNoType.Yes == embedCab)
                                {
                                    this.core.OnMessage(WixErrors.SignedEmbeddedCabinet(childSourceLineNumbers));
                                }
                                else if (null == cabinet)
                                {
                                    this.core.OnMessage(WixErrors.ExpectedSignedCabinetName(childSourceLineNumbers));
                                }
                                else
                                {
                                    this.ParseDigitalSignatureElement(child, id.ToString(CultureInfo.InvariantCulture.NumberFormat));
                                }
                                break;
                            case "PatchBaseline":
                                if (patch)
                                {
                                    this.ParsePatchBaselineElement(child, id);
                                }
                                else
                                {
                                    this.core.UnexpectedElement(node, child);
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // add the row to the section
            if (!this.core.EncounteredError)
            {
                MediaRow mediaRow = (MediaRow)this.core.CreateRow(sourceLineNumbers, "Media");
                mediaRow.DiskId = id;
                mediaRow.LastSequence = 0; // this is set in the binder
                mediaRow.DiskPrompt = diskPrompt;
                mediaRow.Cabinet = cabinet;
                mediaRow.VolumeLabel = volumeLabel;
                mediaRow.Source = source;

                // the Source column is only set when creating a patch

                if (null != compressionLevel || null != layout)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "WixMedia");
                    row[0] = id;
                    row[1] = compressionLevel;
                    row[2] = layout;
                }
            }
        }

        /// <summary>
        /// Parses a merge element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="directoryId">Identifier for parent directory.</param>
        private void ParseMergeElement(XmlNode node, string directoryId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string configData = String.Empty;
            int diskId = CompilerCore.IntegerNotSet;
            YesNoType fileCompression = YesNoType.NotSet;
            string language = null;
            string sourceFile = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DiskId":
                            diskId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Media", diskId.ToString(CultureInfo.InvariantCulture.NumberFormat));
                            break;
                        case "FileCompression":
                            fileCompression = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Language":
                            language = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "SourceFile":
                        case "src":
                            if (null != sourceFile)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "SourceFile", "src"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourceFile"));
                            }
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == language)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Language"));
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            if (CompilerCore.IntegerNotSet == diskId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DiskId"));
                diskId = CompilerCore.IllegalInteger;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ConfigurationData":
                                if (0 == configData.Length)
                                {
                                    configData = this.ParseConfigurationDataElement(child);
                                }
                                else
                                {
                                    configData = String.Concat(configData, ",", this.ParseConfigurationDataElement(child));
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixMerge");
                row[0] = id;
                row[1] = language;
                row[2] = directoryId;
                row[3] = sourceFile;
                row[4] = diskId;
                if (YesNoType.Yes == fileCompression)
                {
                    row[5] = 1;
                }
                else if (YesNoType.No == fileCompression)
                {
                    row[5] = 0;
                }
                else // YesNoType.NotSet == fileCompression
                {
                    // and we leave the column null
                }
                row[6] = configData;
                row[7] = Guid.Empty.ToString("B");
            }
        }

        /// <summary>
        /// Parses a configuration data element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>String in format "name=value" with '%', ',' and '=' hex encoded.</returns>
        private string ParseConfigurationDataElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string name = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }
            else // need to hex encode these characters
            {
                name = name.Replace("%", "%25");
                name = name.Replace("=", "%3D");
                name = name.Replace(",", "%2C");
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }
            else // need to hex encode these characters
            {
                value = value.Replace("%", "%25");
                value = value.Replace("=", "%3D");
                value = value.Replace(",", "%2C");
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return String.Concat(name, "=", value);
        }

        /// <summary>
        /// Parses a merge reference element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="featureId">Identifier for parent feature.</param>
        private void ParseMergeRefElement(XmlNode node, string featureId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            YesNoType primary = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixMerge", id);
                            break;
                        case "Primary":
                            primary = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.core.CreateWixComplexReferenceRow(sourceLineNumbers, ComplexReferenceParentType.Feature, featureId, null, ComplexReferenceChildType.Module, id, (YesNoType.Yes == primary));
        }

        /// <summary>
        /// Parses a mime element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="extension">Identifier for parent extension.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        /// <param name="parentAdvertised">Flag if the parent element is advertised.</param>
        /// <returns>Content type if this is the default for the MIME type.</returns>
        private string ParseMIMEElement(XmlNode node, string extension, string componentId, YesNoType parentAdvertised)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string classId = null;
            string contentType = null;
            YesNoType advertise = parentAdvertised;
            YesNoType returnContentType = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Advertise":
                            advertise = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Class":
                            classId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "ContentType":
                            contentType = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Default":
                            returnContentType = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == contentType)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "ContentType"));
            }

            // if the advertise state has not been set, default to non-advertised
            if (YesNoType.NotSet == advertise)
            {
                advertise = YesNoType.No;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (YesNoType.Yes == advertise)
            {
                if (YesNoType.Yes != parentAdvertised)
                {
                    this.core.OnMessage(WixErrors.AdvertiseStateMustMatch(sourceLineNumbers, advertise.ToString(CultureInfo.InvariantCulture.NumberFormat), parentAdvertised.ToString(CultureInfo.InvariantCulture.NumberFormat)));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MIME");
                    row[0] = contentType;
                    row[1] = extension;
                    row[2] = classId;
                }
            }
            else if (YesNoType.No == advertise)
            {
                if (YesNoType.Yes == returnContentType && YesNoType.Yes == parentAdvertised)
                {
                    this.core.OnMessage(WixErrors.CannotDefaultMismatchedAdvertiseStates(sourceLineNumbers));
                }

                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("MIME\\Database\\Content Type\\", contentType), "Extension", String.Concat(".", extension), componentId);
                if (null != classId)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("MIME\\Database\\Content Type\\", contentType), "CLSID", classId, componentId);
                }
            }

            return YesNoType.Yes == returnContentType ? contentType : null;
        }

        /// <summary>
        /// Parses a module element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseModuleElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int codepage = 0;
            string moduleId = null;
            Version version = null;

            this.activeName = null;
            this.activeLanguage = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            this.activeName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if ("PUT-MODULE-NAME-HERE" == this.activeName)
                            {
                                this.core.OnMessage(WixWarnings.PlaceholderValue(sourceLineNumbers, node.Name, attrib.Name, this.activeName));
                            }
                            else
                            {
                                this.activeName = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            }
                            break;
                        case "Codepage":
                            codepage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "Guid":
                            moduleId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedModuleGuidAttribute(sourceLineNumbers));
                            break;
                        case "Language":
                            this.activeLanguage = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Version":
                            version = this.core.GetAttributeVersionValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == this.activeName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == this.activeLanguage)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Language"));
            }

            if (null == version)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Version"));
            }

            if (!CompilerCore.IsValidModuleVersion(version))
            {
                this.core.OnMessage(WixWarnings.InvalidModuleVersion(sourceLineNumbers, version.ToString()));
            }

            try
            {
                this.compilingModule = true; // notice that we are actually building a Merge Module here
                this.core.CreateActiveSection(this.activeName, SectionType.Module, codepage);

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (XmlNodeType.Element == child.NodeType)
                    {
                        if (child.NamespaceURI == this.schema.TargetNamespace)
                        {
                            switch (child.LocalName)
                            {
                                case "AdminExecuteSequence":
                                case "AdminUISequence":
                                case "AdvertiseExecuteSequence":
                                case "InstallExecuteSequence":
                                case "InstallUISequence":
                                    this.ParseSequenceElement(child, child.LocalName);
                                    break;
                                case "AppId":
                                    this.ParseAppIdElement(child, null, YesNoType.Yes, null, null, null);
                                    break;
                                case "Binary":
                                    this.ParseBinaryElement(child);
                                    break;
                                case "ComponentGroupRef":
                                    this.ParseComponentGroupRefElement(child, ComplexReferenceParentType.Module, this.activeName, this.activeLanguage);
                                    break;
                                case "ComponentRef":
                                    this.ParseComponentRefElement(child, ComplexReferenceParentType.Module, this.activeName, this.activeLanguage);
                                    break;
                                case "Configuration":
                                    this.ParseConfigurationElement(child);
                                    break;
                                case "CustomAction":
                                    this.ParseCustomActionElement(child);
                                    break;
                                case "CustomActionRef":
                                    this.ParseSimpleRefElement(child, "CustomAction");
                                    break;
                                case "CustomTable":
                                    this.ParseCustomTableElement(child);
                                    break;
                                case "Dependency":
                                    this.ParseDependencyElement(child);
                                    break;
                                case "Directory":
                                    this.ParseDirectoryElement(child, null, CompilerCore.IntegerNotSet, String.Empty);
                                    break;
                                case "DirectoryRef":
                                    this.ParseDirectoryRefElement(child);
                                    break;
                                case "EnsureTable":
                                    this.ParseEnsureTableElement(child);
                                    break;
                                case "Exclusion":
                                    this.ParseExclusionElement(child);
                                    break;
                                case "Icon":
                                    this.ParseIconElement(child);
                                    break;
                                case "IgnoreModularization":
                                    this.ParseIgnoreModularizationElement(child);
                                    break;
                                case "IgnoreTable":
                                    this.ParseIgnoreTableElement(child);
                                    break;
                                case "Package":
                                    this.ParsePackageElement(child, null, moduleId);
                                    break;
                                case "Property":
                                    this.ParsePropertyElement(child);
                                    break;
                                case "PropertyRef":
                                    this.ParseSimpleRefElement(child, "Property");
                                    break;
                                case "SFPCatalog":
                                    string parentName = null;
                                    this.ParseSFPCatalogElement(child, ref parentName);
                                    break;
                                case "Substitution":
                                    this.ParseSubstitutionElement(child);
                                    break;
                                case "UI":
                                    this.ParseUIElement(child);
                                    break;
                                case "UIRef":
                                    this.ParseSimpleRefElement(child, "WixUI");
                                    break;
                                case "WixVariable":
                                    this.ParseWixVariableElement(child);
                                    break;
                                default:
                                    this.core.UnexpectedElement(node, child);
                                    break;
                            }
                        }
                        else
                        {
                            this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                        }
                    }
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "ModuleSignature");
                    row[0] = this.activeName;
                    row[1] = this.activeLanguage;
                    row[2] = version.ToString();
                }
            }
            finally
            {
                this.compilingModule = false; // notice that we are no longer building a Merge Module here
            }
        }

        /// <summary>
        /// Parses a patch creation element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        private void ParsePatchCreationElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            bool clean = true; // Default is to clean
            int codepage = 0;
            string outputPath = null;
            bool productMismatches = false;
            string replaceGuids = String.Empty;
            string sourceList = null;
            string symbolFlags = null;
            string targetProducts = String.Empty;
            bool versionMismatches = false;
            bool wholeFiles = false;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            this.activeName = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "AllowMajorVersionMismatches":
                            versionMismatches = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "AllowProductCodeMismatches":
                            productMismatches = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "CleanWorkingFolder":
                            clean = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Codepage":
                            codepage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "OutputPath":
                            outputPath = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceList":
                            sourceList = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SymbolFlags":
                            symbolFlags = String.Format(CultureInfo.InvariantCulture, "0x{0:x8}", this.core.GetAttributeLongValue(sourceLineNumbers, attrib, 0, uint.MaxValue));
                            break;
                        case "WholeFilesOnly":
                            wholeFiles = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == this.activeName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            this.core.CreateActiveSection(this.activeName, SectionType.PatchCreation, codepage);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Family":
                                this.ParseFamilyElement(child);
                                break;
                            case "PatchInformation":
                                this.ParsePatchInformationElement(child);
                                break;
                            case "PatchMetadata":
                                this.ParsePatchMetadataElement(child);
                                break;
                            case "PatchProperty":
                                this.ParsePatchPropertyElement(child, false);
                                break;
                            case "PatchSequence":
                                this.ParsePatchSequenceElement(child);
                                break;
                            case "ReplacePatch":
                                replaceGuids = String.Concat(replaceGuids, this.ParseReplacePatchElement(child));
                                break;
                            case "TargetProductCode":
                                string targetProduct = this.ParseTargetProductCodeElement(child);
                                if (0 < targetProducts.Length)
                                {
                                    targetProducts = String.Concat(targetProducts, ";");
                                }
                                targetProducts = String.Concat(targetProducts, targetProduct);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            this.ProcessProperties(sourceLineNumbers, "PatchGUID", this.activeName);
            this.ProcessProperties(sourceLineNumbers, "AllowProductCodeMismatches", productMismatches ? "1" : "0");
            this.ProcessProperties(sourceLineNumbers, "AllowProductVersionMajorMismatches", versionMismatches ? "1" : "0");
            this.ProcessProperties(sourceLineNumbers, "DontRemoveTempFolderWhenFinished", clean ? "0" : "1");
            this.ProcessProperties(sourceLineNumbers, "IncludeWholeFilesOnly", wholeFiles ? "1" : "0");

            if (null != symbolFlags)
            {
                this.ProcessProperties(sourceLineNumbers, "ApiPatchingSymbolFlags", symbolFlags);
            }

            if (0 < replaceGuids.Length)
            {
                this.ProcessProperties(sourceLineNumbers, "ListOfPatchGUIDsToReplace", replaceGuids);
            }

            if (0 < targetProducts.Length)
            {
                this.ProcessProperties(sourceLineNumbers, "ListOfTargetProductCodes", targetProducts);
            }

            if (null != outputPath)
            {
                this.ProcessProperties(sourceLineNumbers, "PatchOutputPath", outputPath);
            }

            if (null != sourceList)
            {
                this.ProcessProperties(sourceLineNumbers, "PatchSourceList", sourceList);
            }
        }

        /// <summary>
        /// Parses a family element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        private void ParseFamilyElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int diskId = CompilerCore.IntegerNotSet;
            string diskPrompt = null;
            string mediaSrcProp = null;
            string name = null;
            int sequenceStart = CompilerCore.IntegerNotSet;
            string volumeLabel = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "DiskId":
                            diskId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "DiskPrompt":
                            diskPrompt = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MediaSrcProp":
                            mediaSrcProp = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SequenceStart":
                            sequenceStart = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, int.MaxValue);
                            break;
                        case "VolumeLabel":
                            volumeLabel = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }
            else if (CompilerCore.IllegalEmptyAttributeValue != name)
            {
                if (8 < name.Length) // check the length
                {
                    this.core.OnMessage(WixErrors.FamilyNameTooLong(sourceLineNumbers, node.Name, "Name", name, name.Length));
                }
                else // check for illegal characters
                {
                    foreach (char character in name)
                    {
                        if (!Char.IsLetterOrDigit(character) && '_' != character)
                        {
                            this.core.OnMessage(WixErrors.IllegalFamilyName(sourceLineNumbers, node.Name, "Name", name));
                        }
                    }
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "UpgradeImage":
                                this.ParseUpgradeImageElement(child, name);
                                break;
                            case "ExternalFile":
                                this.ParseExternalFileElement(child, name);
                                break;
                            case "ProtectFile":
                                this.ParseProtectFileElement(child, name);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ImageFamilies");
                row[0] = name;
                row[1] = mediaSrcProp;
                if (CompilerCore.IntegerNotSet != diskId)
                {
                    row[2] = diskId;
                }

                if (CompilerCore.IntegerNotSet != sequenceStart)
                {
                    row[3] = sequenceStart;
                }
                row[4] = diskPrompt;
                row[5] = volumeLabel;
            }
        }

        /// <summary>
        /// Parses an upgrade image element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="family">The family for this element.</param>
        private void ParseUpgradeImageElement(XmlNode node, string family)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string sourceFile = null;
            string sourcePatch = null;
            StringBuilder symbols = new StringBuilder();
            string upgrade = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            upgrade = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SourceFile":
                        case "src":
                            if (null != sourceFile)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "src", "SourceFile"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourceFile"));
                            }
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SourcePatch":
                        case "srcPatch":
                            if (null != sourcePatch)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "srcPatch", "SourcePatch"));
                            }

                            if ("srcPatch" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourcePatch"));
                            }
                            sourcePatch = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == upgrade)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "SymbolPath":
                                if (0 < symbols.Length)
                                {
                                    symbols.AppendFormat(";{0}", this.ParseSymbolPathElement(child));
                                }
                                else
                                {
                                    symbols.Append(this.ParseSymbolPathElement(child));
                                }
                                break;
                            case "TargetImage":
                                this.ParseTargetImageElement(child, upgrade, family);
                                break;
                            case "UpgradeFile":
                                this.ParseUpgradeFileElement(child, upgrade);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "UpgradedImages");
                row[0] = upgrade;
                row[1] = sourceFile;
                row[2] = sourcePatch;
                row[3] = symbols.ToString();
                row[4] = family;
            }
        }

        /// <summary>
        /// Parses an upgrade file element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="upgrade">The upgrade key for this element.</param>
        private void ParseUpgradeFileElement(XmlNode node, string upgrade)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            bool allowIgnoreOnError = false;
            string file = null;
            bool ignore = false;
            StringBuilder symbols = new StringBuilder();
            bool wholeFile = false;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "AllowIgnoreOnError":
                            allowIgnoreOnError = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "File":
                            file = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Ignore":
                            ignore = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "WholeFile":
                            wholeFile = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == file)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "File"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "SymbolPath":
                                if (0 < symbols.Length)
                                {
                                    symbols.AppendFormat(";{0}", this.ParseSymbolPathElement(child));
                                }
                                else
                                {
                                    symbols.Append(this.ParseSymbolPathElement(child));
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                if (ignore)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "UpgradedFilesToIgnore");
                    row[0] = upgrade;
                    row[1] = file;
                }
                else
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "UpgradedFiles_OptionalData");
                    row[0] = upgrade;
                    row[1] = file;
                    row[2] = symbols.ToString();
                    row[3] = allowIgnoreOnError ? 1 : 0;
                    row[4] = wholeFile ? 1 : 0;
                }
            }
        }

        /// <summary>
        /// Parses a target image element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="upgrade">The upgrade key for this element.</param>
        /// <param name="family">The family key for this element.</param>
        private void ParseTargetImageElement(XmlNode node, string upgrade, string family)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            bool ignore = false;
            int order = CompilerCore.IntegerNotSet;
            string sourceFile = null;
            string symbols = null;
            string target = null;
            string validation = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "IgnoreMissingFiles":
                            ignore = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Order":
                            order = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, int.MinValue + 2, int.MaxValue);
                            break;
                        case "SourceFile":
                        case "src":
                            if (null != sourceFile)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "src", "SourceFile"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "SourceFile"));
                            }
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Validation":
                            validation = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == target)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            if (CompilerCore.IntegerNotSet == order)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Order"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "SymbolPath":
                                if (null != symbols)
                                {
                                    symbols = String.Concat(symbols, ";", this.ParseSymbolPathElement(child));
                                }
                                else
                                {
                                    symbols = this.ParseSymbolPathElement(child);
                                }
                                break;
                            case "TargetFile":
                                this.ParseTargetFileElement(child, target, family);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "TargetImages");
                row[0] = target;
                row[1] = sourceFile;
                row[2] = symbols;
                row[3] = upgrade;
                row[4] = order;
                row[5] = validation;
                row[6] = ignore ? 1 : 0;
            }
        }

        /// <summary>
        /// Parses an upgrade file element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="target">The upgrade key for this element.</param>
        /// <param name="family">The family key for this element.</param>
        private void ParseTargetFileElement(XmlNode node, string target, string family)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string file = null;
            string ignoreLengths = null;
            string ignoreOffsets = null;
            string protectLengths = null;
            string protectOffsets = null;
            string symbols = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            file = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == file)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "IgnoreRange":
                                this.ParseRangeElement(child, ref ignoreOffsets, ref ignoreLengths);
                                break;
                            case "ProtectRange":
                                this.ParseRangeElement(child, ref protectOffsets, ref protectLengths);
                                break;
                            case "SymbolPath":
                                symbols = this.ParseSymbolPathElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "TargetFiles_OptionalData");
                row[0] = target;
                row[1] = file;
                row[2] = symbols;
                row[3] = ignoreOffsets;
                row[4] = ignoreLengths;

                if (null != protectOffsets)
                {
                    row[5] = protectOffsets;

                    Row row2 = this.core.CreateRow(sourceLineNumbers, "FamilyFileRanges");
                    row2[0] = family;
                    row2[1] = file;
                    row2[2] = protectOffsets;
                    row2[3] = protectLengths;
                }
            }
        }

        /// <summary>
        /// Parses an external file element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="family">The family for this element.</param>
        private void ParseExternalFileElement(XmlNode node, string family)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string file = null;
            string ignoreLengths = null;
            string ignoreOffsets = null;
            int order = CompilerCore.IntegerNotSet;
            string protectLengths = null;
            string protectOffsets = null;
            string source = null;
            string symbols = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "File":
                            file = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Order":
                            order = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, int.MinValue + 2, int.MaxValue);
                            break;
                        case "Source":
                        case "src":
                            if (null != source)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "src", "Source"));
                            }

                            if ("src" == attrib.LocalName)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "Source"));
                            }
                            source = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == file)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "File"));
            }

            if (null == source)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Source"));
            }

            if (CompilerCore.IntegerNotSet == order)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Order"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "IgnoreRange":
                                this.ParseRangeElement(child, ref ignoreOffsets, ref ignoreLengths);
                                break;
                            case "ProtectRange":
                                this.ParseRangeElement(child, ref protectOffsets, ref protectLengths);
                                break;
                            case "SymbolPath":
                                symbols = this.ParseSymbolPathElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ExternalFiles");
                row[0] = family;
                row[1] = file;
                row[2] = source;
                row[3] = symbols;
                row[4] = ignoreOffsets;
                row[5] = ignoreLengths;
                if (null != protectOffsets)
                {
                    row[6] = protectOffsets;
                }

                if (CompilerCore.IntegerNotSet != order)
                {
                    row[7] = order;
                }

                if (null != protectOffsets)
                {
                    Row row2 = this.core.CreateRow(sourceLineNumbers, "FamilyFileRanges");
                    row2[0] = family;
                    row2[1] = file;
                    row2[2] = protectOffsets;
                    row2[3] = protectLengths;
                }
            }
        }

        /// <summary>
        /// Parses a protect file element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="family">The family for this element.</param>
        private void ParseProtectFileElement(XmlNode node, string family)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string file = null;
            string protectLengths = null;
            string protectOffsets = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "File":
                            file = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == file)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "File"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ProtectRange":
                                this.ParseRangeElement(child, ref protectOffsets, ref protectLengths);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null == protectOffsets || null == protectLengths)
            {
                this.core.OnMessage(WixErrors.ExpectedElement(sourceLineNumbers, node.Name, "ProtectRange"));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "FamilyFileRanges");
                row[0] = family;
                row[1] = file;
                row[2] = protectOffsets;
                row[3] = protectLengths;
            }
        }

        /// <summary>
        /// Parses a range element (ProtectRange, IgnoreRange, etc).
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="offsets">Reference to the offsets string.</param>
        /// <param name="lengths">Reference to the lengths string.</param>
        private void ParseRangeElement(XmlNode node, ref string offsets, ref string lengths)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string length = null;
            string offset = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Length":
                            length = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Offset":
                            offset = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == length)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Length"));
            }

            if (null == offset)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Offset"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null != lengths)
            {
                lengths = String.Concat(lengths, ",", length);
            }
            else
            {
                lengths = length;
            }

            if (null != offsets)
            {
                offsets = String.Concat(offsets, ",", offset);
            }
            else
            {
                offsets = offset;
            }
        }

        /// <summary>
        /// Parses a patch property element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="patch">True if parsing an patch element.</param>
        private void ParsePatchPropertyElement(XmlNode node, bool patch)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string name = null;
            string company = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Company":
                            company = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (patch)
            {
                // /Patch/PatchProperty goes directly into MsiPatchMetadata table
                Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                row[0] = company;
                row[1] = name;
                row[2] = value;
            }
            else
            {
                if (null != company)
                {
                    this.core.OnMessage(WixErrors.UnexpectedAttribute(sourceLineNumbers, node.Name, "Company"));
                }
                this.ProcessProperties(sourceLineNumbers, name, value);
            }
        }

        /// <summary>
        /// Parses a patch sequence element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        private void ParsePatchSequenceElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string family = null;
            string target = null;
            Version sequence = null;
            int attributes = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "PatchFamily":
                            family = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ProductCode":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttributes(sourceLineNumbers, node.Name, attrib.Name, "Target", "TargetImage"));
                            }
                            target = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Target":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttributes(sourceLineNumbers, node.Name, attrib.Name, "TargetImage", "ProductCode"));
                            }
                            this.core.OnMessage(WixWarnings.DeprecatedPatchSequenceTargetAttribute(sourceLineNumbers, node.Name, attrib.Name));
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "TargetImage":
                            if (null != target)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttributes(sourceLineNumbers, node.Name, attrib.Name, "Target", "ProductCode"));
                            }
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "TargetImages", target);
                            break;
                        case "Sequence":
                            sequence = this.core.GetAttributeVersionValue(sourceLineNumbers, attrib);
                            break;
                        case "Supersede":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= 0x1;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == family)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "PatchFamily"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "PatchSequence");
                row[0] = family;
                row[1] = target;
                if (null != sequence)
                {
                    row[2] = sequence.ToString();
                }
                row[3] = attributes;
            }
        }

        /// <summary>
        /// Parses a TargetProductCode element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <returns>The id from the node.</returns>
        private string ParseTargetProductCodeElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if (CompilerCore.IllegalEmptyAttributeValue != id && "*" != id)
                            {
                                id = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Parses a ReplacePatch element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <returns>The id from the node.</returns>
        private string ParseReplacePatchElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return id;
        }

        /// <summary>
        /// Parses a symbol path element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <returns>The path from the node.</returns>
        private string ParseSymbolPathElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string path = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Path":
                            path = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == path)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Path"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Parses an patch element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        private void ParsePatchElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string patchId = null;
            int codepage = 0;
            bool versionMismatches = false;
            bool productMismatches = false;
            bool allowRemoval = false;
            string classification = null;
            string description = null;
            string displayName = null;
            string manufacturer = null;
            string moreInfoUrl = null;
            // string replaceGuids = String.Empty;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            patchId = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, true);
                            break;
                        case "Codepage":
                            codepage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "AllowMajorVersionMismatches":
                            versionMismatches = (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib));
                            break;
                        case "AllowProductCodeMismatches":
                            productMismatches = (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib));
                            break;
                        case "AllowRemoval":
                            allowRemoval = (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib));
                            break;
                        case "Classification":
                            classification = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (classification)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "Critical Update":
                                case "Hotfix":
                                case "Security Rollup":
                                case "Service Pack":
                                case "Update":
                                case "Update Rollup":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, classification, "Critical Update", "Hotfix", "Security Rollup", "Service Pack", "Update", "Update Rollup"));
                                    break;
                            }
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DisplayName":
                            displayName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Manufacturer":
                            manufacturer = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MoreInfoURL":
                            moreInfoUrl = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (patchId == null || patchId == "*")
            {
                // auto-generate at compile time, since this value gets dispersed to several locations
                patchId = String.Concat("{", Guid.NewGuid().ToString().ToUpper(CultureInfo.InvariantCulture), "}");
            }
            this.activeName = patchId;

            if (null == this.activeName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            if (null == classification)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Classification"));
            }
            if (null == description)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Description"));
            }
            if (null == displayName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DisplayName"));
            }
            if (null == manufacturer)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Manufacturer"));
            }

            this.core.CreateActiveSection(this.activeName, SectionType.Patch, codepage);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Media":
                                this.ParseMediaElement(child, patchId);
                                break;
                            case "PatchFamily":
                                this.ParsePatchFamilyElement(child);
                                break;
                            case "PatchFamilyRef":
                                this.ParsePatchFamilyRefElement(child);
                                break;
                            case "PatchProperty":
                                this.ParsePatchPropertyElement(child, true);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row patchIdRow = this.core.CreateRow(sourceLineNumbers, "WixPatchId");
                patchIdRow[0] = patchId;

                if (allowRemoval)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                    row[0] = null;
                    row[1] = "AllowRemoval";
                    row[2] = allowRemoval ? "1" : "0";
                }

                if (null != classification)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                    row[0] = null;
                    row[1] = "Classification";
                    row[2] = classification;
                }

                if (null != description)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                    row[0] = null;
                    row[1] = "Description";
                    row[2] = description;
                }

                if (null != displayName)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                    row[0] = null;
                    row[1] = "DisplayName";
                    row[2] = displayName;
                }

                if (null != manufacturer)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                    row[0] = null;
                    row[1] = "ManufacturerName";
                    row[2] = manufacturer;
                }

                if (null != moreInfoUrl)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchMetadata");
                    row[0] = null;
                    row[1] = "MoreInfoURL";
                    row[2] = moreInfoUrl;
                }
            }
            // TODO: do something with versionMismatches and productMismatches
        }

        /// <summary>
        /// Parses a PatchFamily element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        private void ParsePatchFamilyElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string productCode = null;
            Version version = null;
            int attributes = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ProductCode":
                            productCode = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Version":
                            version = this.core.GetAttributeVersionValue(sourceLineNumbers, attrib);
                            break;
                        case "Supersede":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= 0x1;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == version)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Version"));
            }
            else if (!CompilerCore.IsValidProductVersion(version))
            {
                this.core.OnMessage(WixErrors.InvalidProductVersion(sourceLineNumbers, version.ToString()));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "BinaryRef":
                                this.ParsePatchChildRefElement(child, "Binary");
                                break;
                            case "ComponentRef":
                                this.ParsePatchChildRefElement(child, "Component");
                                break;
                            case "CustomActionRef":
                                this.ParsePatchChildRefElement(child, "CustomAction");
                                break;
                            case "DirectoryRef":
                                this.ParsePatchChildRefElement(child, "Directory");
                                break;
                            case "FeatureRef":
                                this.ParsePatchChildRefElement(child, "Feature");
                                break;
                            case "PropertyRef":
                                this.ParsePatchChildRefElement(child, "Property");
                                break;
                            case "UIRef":
                                this.ParsePatchChildRefElement(child, "WixUI");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "MsiPatchSequence");
                row[0] = id;
                row[1] = productCode;
                row[2] = version.ToString();
                row[3] = attributes;
            }
        }

        /// <summary>
        /// Parses all reference elements under a PatchFamily.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="tableName">Table that reference was made to.</param>
        private void ParsePatchChildRefElement(XmlNode node, string tableName)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string[] id = new string[1];

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id[0] = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id[0])
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                this.core.AddPatchFamilyChildReference(sourceLineNumbers, tableName, id);
            }
        }

        /// <summary>
        /// Parses a PatchBaseline element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="diskId">Media index from parent element.</param>
        private void ParsePatchBaselineElement(XmlNode node, int diskId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                id = CompilerCore.IllegalIdentifier;
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixPatchBaseline");
                row[0] = id;
                row[1] = diskId;
            }
        }

        /// <summary>
        /// Adds a row to the properties table.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers.</param>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">Value of the property.</param>
        private void ProcessProperties(SourceLineNumberCollection sourceLineNumbers, string name, string value)
        {
            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Properties");
                row[0] = name;
                row[1] = value;
            }
        }

        /// <summary>
        /// Parses a dependency element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseDependencyElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string requiredId = null;
            int requiredLanguage = CompilerCore.IntegerNotSet;
            string requiredVersion = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "RequiredId":
                            requiredId = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "RequiredLanguage":
                            requiredLanguage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "RequiredVersion":
                            requiredVersion = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == requiredId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "RequiredId"));
                requiredId = CompilerCore.IllegalIdentifier;
            }

            if (CompilerCore.IntegerNotSet == requiredLanguage)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "RequiredLanguage"));
                requiredLanguage = CompilerCore.IllegalInteger;
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ModuleDependency");
                row[0] = this.activeName;
                row[1] = this.activeLanguage;
                row[2] = requiredId;
                row[3] = requiredLanguage.ToString(CultureInfo.InvariantCulture);
                row[4] = requiredVersion;
            }
        }

        /// <summary>
        /// Parses an exclusion element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseExclusionElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string excludedId = null;
            int excludeExceptLanguage = CompilerCore.IntegerNotSet;
            int excludeLanguage = CompilerCore.IntegerNotSet;
            string excludedLanguageField = "0";
            string excludedMaxVersion = null;
            string excludedMinVersion = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "ExcludedId":
                            excludedId = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ExcludeExceptLanguage":
                            excludeExceptLanguage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "ExcludeLanguage":
                            excludeLanguage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "ExcludedMaxVersion":
                            excludedMaxVersion = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ExcludedMinVersion":
                            excludedMinVersion = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == excludedId)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "ExcludedId"));
                excludedId = CompilerCore.IllegalIdentifier;
            }

            if (CompilerCore.IntegerNotSet != excludeExceptLanguage && CompilerCore.IntegerNotSet != excludeLanguage)
            {
                this.core.OnMessage(WixErrors.IllegalModuleExclusionLanguageAttributes(sourceLineNumbers));
            }
            else if (CompilerCore.IntegerNotSet != excludeExceptLanguage)
            {
                excludedLanguageField = Convert.ToString(-excludeExceptLanguage, CultureInfo.InvariantCulture);
            }
            else if (CompilerCore.IntegerNotSet != excludeLanguage)
            {
                excludedLanguageField = Convert.ToString(excludeLanguage, CultureInfo.InvariantCulture);
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ModuleExclusion");
                row[0] = this.activeName;
                row[1] = this.activeLanguage;
                row[2] = excludedId;
                row[3] = excludedLanguageField;
                row[4] = excludedMinVersion;
                row[5] = excludedMaxVersion;
            }
        }

        /// <summary>
        /// Parses a configuration element for a configurable merge module.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseConfigurationElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int attributes = 0;
            string contextData = null;
            string defaultValue = null;
            string description = null;
            string displayName = null;
            int format = CompilerCore.IntegerNotSet;
            string helpKeyword = null;
            string helpLocation = null;
            string name = null;
            string type = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Name":
                            name = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ContextData":
                            contextData = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DefaultValue":
                            defaultValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DisplayName":
                            displayName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Format":
                            string formatStr = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (formatStr)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "Text":
                                    format = 0;
                                    break;
                                case "Key":
                                    format = 1;
                                    break;
                                case "Integer":
                                    format = 2;
                                    break;
                                case "Bitfield":
                                    format = 3;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Format", formatStr, "Text", "Key", "Integer", "Bitfield"));
                                    break;
                            }
                            break;
                        case "HelpKeyword":
                            helpKeyword = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "HelpLocation":
                            helpLocation = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "KeyNoOrphan":
                            attributes |= MsiInterop.MsidbMsmConfigurableOptionKeyNoOrphan;
                            break;
                        case "NonNullable":
                            attributes |= MsiInterop.MsidbMsmConfigurableOptionNonNullable;
                            break;
                        case "Type":
                            type = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
                name = CompilerCore.IllegalIdentifier;
            }

            if (CompilerCore.IntegerNotSet == format)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Format"));
                format = CompilerCore.IllegalInteger;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ModuleConfiguration");
                row[0] = name;
                row[1] = format;
                row[2] = type;
                row[3] = contextData;
                row[4] = defaultValue;
                row[5] = attributes;
                row[6] = displayName;
                row[7] = description;
                row[8] = helpLocation;
                row[9] = helpKeyword;
            }
        }

        /// <summary>
        /// Parses a substitution element for a configurable merge module.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseSubstitutionElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string column = null;
            string rowKeys = null;
            string table = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Column":
                            column = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Row":
                            rowKeys = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Table":
                            table = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == column)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Column"));
                column = CompilerCore.IllegalIdentifier;
            }

            if (null == table)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Table"));
                table = CompilerCore.IllegalIdentifier;
            }

            if (null == rowKeys)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Row"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ModuleSubstitution");
                row[0] = table;
                row[1] = rowKeys;
                row[2] = column;
                row[3] = value;
            }
        }

        /// <summary>
        /// Parses an IgnoreTable element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseIgnoreTableElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ModuleIgnoreTable");
                row[0] = id;
            }
        }

        /// <summary>
        /// Parses an odbc driver or translator element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="fileId">Default identifer for driver/translator file.</param>
        /// <param name="table">Table we're processing for.</param>
        private void ParseODBCDriverOrTranslator(XmlNode node, string componentId, string fileId, TableDefinition table)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string driver = fileId;
            string name = null;
            string setup = fileId;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "File":
                            driver = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", driver);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SetupFile":
                            setup = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", setup);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            // drivers have a few possible children
            if ("ODBCDriver" == table.Name)
            {
                // process any data sources for the driver
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (XmlNodeType.Element == child.NodeType)
                    {
                        if (child.NamespaceURI == this.schema.TargetNamespace)
                        {
                            switch (child.LocalName)
                            {
                                case "ODBCDataSource":
                                    string ignoredKeyPath = null;
                                    this.ParseODBCDataSource(child, componentId, name, out ignoredKeyPath);
                                    break;
                                case "Property":
                                    this.ParseODBCProperty(child, id, "ODBCAttribute");
                                    break;
                                default:
                                    this.core.UnexpectedElement(node, child);
                                    break;
                            }
                        }
                        else
                        {
                            this.core.UnsupportedExtensionElement(node, child);
                        }
                    }
                }
            }
            else
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (XmlNodeType.Element == child.NodeType)
                    {
                        if (child.NamespaceURI == this.schema.TargetNamespace)
                        {
                            this.core.UnexpectedElement(node, child);
                        }
                        else
                        {
                            this.core.UnsupportedExtensionElement(node, child);
                        }
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, table.Name);
                row[0] = id;
                row[1] = componentId;
                row[2] = name;
                row[3] = driver;
                row[4] = setup;
            }
        }

        /// <summary>
        /// Parses a Property element underneath an ODBC driver or translator.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentId">Identifier of parent driver or translator.</param>
        /// <param name="tableName">Name of the table to create property in.</param>
        private void ParseODBCProperty(XmlNode node, string parentId, string tableName)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string propertyValue = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            propertyValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, tableName);
                row[0] = parentId;
                row[1] = id;
                row[2] = propertyValue;
            }
        }

        /// <summary>
        /// Parse an odbc data source element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="driverName">Default name of driver.</param>
        /// <param name="possibleKeyPath">Identifier of this element in case it is a keypath.</param>
        /// <returns>True if this element was marked as the parent component's key path.</returns>
        private bool ParseODBCDataSource(XmlNode node, string componentId, string driverName, out string possibleKeyPath)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            bool keyPath = false;
            string name = null;
            int registration = CompilerCore.IntegerNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "DriverName":
                            driverName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "KeyPath":
                            keyPath = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Registration":
                            string registrationValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (registrationValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "machine":
                                    registration = 0;
                                    break;
                                case "user":
                                    registration = 1;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Registration", registrationValue, "machine", "user"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (CompilerCore.IntegerNotSet == registration)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Registration"));
                registration = CompilerCore.IllegalInteger;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Property":
                                this.ParseODBCProperty(child, id, "ODBCSourceAttribute");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ODBCDataSource");
                row[0] = id;
                row[1] = componentId;
                row[2] = name;
                row[3] = driverName;
                row[4] = registration;
            }

            possibleKeyPath = id;
            return keyPath;
        }

        /// <summary>
        /// Parses a package element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="productAuthor">Default package author.</param>
        /// <param name="moduleId">The module guid - this is necessary until Module/@Guid is removed.</param>
        private void ParsePackageElement(XmlNode node, string productAuthor, string moduleId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string codepage = "1252";
            string comments = String.Format(CultureInfo.InvariantCulture, "This installer database contains the logic and data required to install {0}.", this.activeName);
            string keywords = "Installer";
            int msiVersion = 100; // lowest released version, really should be specified
            string packageAuthor = productAuthor;
            string packageCode = null;
            string packageLanguages = this.activeLanguage;
            string packageName = this.activeName;
            string platforms = null;
            YesNoDefaultType security = YesNoDefaultType.Default;
            int sourceBits = (this.compilingModule ? 2 : 0);
            Row row;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            packageCode = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, this.compilingProduct);
                            if ("????????-????-????-????-????????????" == packageCode)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedPackageQuestionMarksGuid(sourceLineNumbers, node.Name, attrib.Name));
                                packageCode = "*";
                            }
                            break;
                        case "AdminImage":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                sourceBits = sourceBits | 4;
                            }
                            break;
                        case "Comments":
                            comments = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Compressed":
                            // merge modules must always be compressed, so this attribute is invalid
                            if (this.compilingModule)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedPackageCompressedAttribute(sourceLineNumbers));
                                // this.core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, "Compressed", "Module"));
                            }
                            else if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                sourceBits = sourceBits | 2;
                            }
                            break;
                        case "Description":
                            packageName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "InstallPrivileges":
                            string installPrivileges = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (installPrivileges)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "elevated":
                                    // this is the default setting
                                    break;
                                case "limited":
                                    sourceBits = sourceBits | 8;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, installPrivileges, "elevated", "limited"));
                                    break;
                            }
                            break;
                        case "InstallerVersion":
                            msiVersion = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "Keywords":
                            keywords = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Languages":
                            packageLanguages = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Manufacturer":
                            packageAuthor = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if ("PUT-COMPANY-NAME-HERE" == packageAuthor)
                            {
                                this.core.OnMessage(WixWarnings.PlaceholderValue(sourceLineNumbers, node.Name, attrib.Name, packageAuthor));
                            }
                            break;
                        case "Platforms":
                            platforms = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ReadOnly":
                            security = this.core.GetAttributeYesNoDefaultValue(sourceLineNumbers, attrib);
                            break;
                        case "ShortNames":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                sourceBits = sourceBits | 1;
                                this.useShortFileNames = true;
                            }
                            break;
                        case "SummaryCodepage":
                            codepage = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == packageAuthor)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Manufacturer"));
            }

            if (this.compilingModule)
            {
                if (null == packageCode)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
                }

                // merge modules use the modularization guid as the package code
                if (null != moduleId)
                {
                    packageCode = moduleId;
                }

                // merge modules are always compressed
                sourceBits = 2;
            }
            else // product
            {
                if (null == packageCode)
                {
                    packageCode = "*";
                }

                if ("*" != packageCode)
                {
                    this.core.OnMessage(WixWarnings.PackageCodeSet(sourceLineNumbers));
                }
            }

            // check if codepage is an integer, and if so, make sure it is a valid codepage (greater than zero)
            try
            {
                int integerCodepage = Convert.ToInt32(codepage, CultureInfo.InvariantCulture.NumberFormat);
                if (0 >= integerCodepage)
                {
                    this.core.OnMessage(WixErrors.InvalidSummaryCodepage(sourceLineNumbers, node.Name, integerCodepage));
                }
            }
            catch (FormatException)
            {
                // ignore the exception if the codepage was a $(loc.variable)
            }
            catch (OverflowException)
            {
                // ignore the exception if the codepage was a $(loc.variable)
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 1;
                row[1] = codepage;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 2;
                row[1] = "Installation Database";

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 3;
                row[1] = packageName;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 4;
                row[1] = packageAuthor;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 5;
                row[1] = keywords;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 6;
                row[1] = comments;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 7;
                row[1] = String.Format(CultureInfo.InvariantCulture, "{0};{1}", platforms, packageLanguages);

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 9;
                row[1] = packageCode;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 14;
                row[1] = msiVersion.ToString(CultureInfo.InvariantCulture);

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 15;
                row[1] = sourceBits.ToString(CultureInfo.InvariantCulture);

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 19;
                switch (security)
                {
                    case YesNoDefaultType.No: // no restriction
                        row[1] = "0";
                        break;
                    case YesNoDefaultType.Default: // read-only recommended
                        row[1] = "2";
                        break;
                    case YesNoDefaultType.Yes: // read-only enforced
                        row[1] = "4";
                        break;
                }
            }
        }

        /// <summary>
        /// Parses a patch metadata element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParsePatchMetadataElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            YesNoType allowRemoval = YesNoType.NotSet;
            string classification = null;
            string creationTimeUtc = null;
            string description = null;
            string displayName = null;
            string manufacturerName = null;
            string minorUpdateTargetRTM = null;
            string moreInfoUrl = null;
            YesNoType optimizedInstallMode = YesNoType.NotSet;
            string targetProductName = null;
            
            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "AllowRemoval":
                            allowRemoval = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Classification":
                            classification = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (classification)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "Critical Update":
                                case "Hotfix":
                                case "Security Rollup":
                                case "Service Pack":
                                case "Update":
                                case "Update Rollup":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, classification, "Critical Update", "Hotfix", "Security Rollup", "Service Pack", "Update", "Update Rollup"));
                                    break;
                            }
                            break;
                        case "CreationTimeUTC":
                            creationTimeUtc = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DisplayName":
                            displayName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ManufacturerName":
                            manufacturerName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MinorUpdateTargetRTM":
                            minorUpdateTargetRTM = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MoreInfoURL":
                            moreInfoUrl = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "OptimizedInstallMode":
                            optimizedInstallMode = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "TargetProductName":
                            targetProductName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (YesNoType.NotSet == allowRemoval)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "AllowRemoval"));
            }

            if (null == classification)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Classification"));
            }

            if (null == description)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Description"));
            }

            if (null == displayName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DisplayName"));
            }

            if (null == manufacturerName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "ManufacturerName"));
            }

            if (null == targetProductName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "TargetProductName"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "CustomProperty":
                                this.ParseCustomPropertyElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                if (YesNoType.NotSet != allowRemoval)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "AllowRemoval";
                    row[2] = YesNoType.Yes == allowRemoval ? "1" : "0";
                }

                if (null != classification)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "Classification";
                    row[2] = classification;
                }

                if (null != creationTimeUtc)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "CreationTimeUTC";
                    row[2] = creationTimeUtc;
                }

                if (null != description)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "Description";
                    row[2] = description;
                }

                if (null != displayName)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "DisplayName";
                    row[2] = displayName;
                }

                if (null != manufacturerName)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "ManufacturerName";
                    row[2] = manufacturerName;
                }

                if (null != minorUpdateTargetRTM)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "MinorUpdateTargetRTM";
                    row[2] = minorUpdateTargetRTM;
                }

                if (null != moreInfoUrl)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "MoreInfoURL";
                    row[2] = moreInfoUrl;
                }

                if (YesNoType.NotSet != optimizedInstallMode)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "OptimizedInstallMode";
                    row[2] = YesNoType.Yes == optimizedInstallMode ? "1" : "0";
                }

                if (null != targetProductName)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                    row[0] = null;
                    row[1] = "TargetProductName";
                    row[2] = targetProductName;
                }
            }
        }

        /// <summary>
        /// Parses a custom property element for the PatchMetadata table.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseCustomPropertyElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string company = null;
            string property = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Company":
                            company = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Property":
                            property = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == company)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Company"));
            }

            if (null == property)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Property"));
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "PatchMetadata");
                row[0] = company;
                row[1] = property;
                row[2] = value;
            }
        }

        /// <summary>
        /// Parses a patch information element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParsePatchInformationElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string codepage = "1252";
            string comments = null;
            string keywords = "Installer,Patching,PCP,Database";
            int msiVersion = 1; // Should always be 1 for patches
            string packageAuthor = null;
            string packageCode = null;
            string packageLanguages = this.activeLanguage;
            string packageName = this.activeName;
            string platform = null;
            YesNoDefaultType security = YesNoDefaultType.Default;
            int sourceBits = 0;
            Row row;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "AdminImage":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                sourceBits = sourceBits | 4;
                            }
                            else
                            {
                                sourceBits = sourceBits & ~4;
                            }
                            break;
                        case "Comments":
                            comments = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Compressed":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                sourceBits = sourceBits | 2;
                            }
                            else
                            {
                                sourceBits = sourceBits & ~2;
                            }
                            break;
                        case "Description":
                            packageName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Keywords":
                            keywords = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Languages":
                            packageLanguages = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Manufacturer":
                            packageAuthor = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Platforms":
                            platform = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ReadOnly":
                            security = this.core.GetAttributeYesNoDefaultValue(sourceLineNumbers, attrib);
                            break;
                        case "ShortNames":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                sourceBits = sourceBits | 1;
                                this.useShortFileNames = true;
                            }
                            else
                            {
                                sourceBits = sourceBits & ~1;
                            }
                            break;
                        case "SummaryCodepage":
                            codepage = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // check if codepage is an integer, and if so, make sure it is a valid codepage (greater than zero)
            try
            {
                int integerCodepage = Convert.ToInt32(codepage, CultureInfo.InvariantCulture.NumberFormat);
                if (0 >= integerCodepage)
                {
                    this.core.OnMessage(WixErrors.InvalidSummaryCodepage(sourceLineNumbers, node.Name, integerCodepage));
                }
            }
            catch (FormatException)
            {
                // ignore the exception if the codepage was a $(loc.variable)
            }
            catch (OverflowException)
            {
                // ignore the exception if the codepage was a $(loc.variable)
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 1;
                row[1] = codepage;

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 2;
                row[1] = "Installation Database";

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 3;
                row[1] = packageName;

                if (null != packageAuthor)
                {
                    row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                    row[0] = 4;
                    row[1] = packageAuthor;
                }

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 5;
                row[1] = keywords;

                if (null != comments)
                {
                    row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                    row[0] = 6;
                    row[1] = comments;
                }

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 7;
                row[1] = String.Format(CultureInfo.InvariantCulture, "{0};{1}", platform, packageLanguages);

                if (null != packageCode)
                {
                    row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                    row[0] = 9;
                    row[1] = packageCode;
                }

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 14;
                row[1] = msiVersion.ToString(CultureInfo.InvariantCulture);

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 15;
                row[1] = sourceBits.ToString(CultureInfo.InvariantCulture);

                row = this.core.CreateRow(sourceLineNumbers, "_SummaryInformation");
                row[0] = 19;
                switch (security)
                {
                    case YesNoDefaultType.No: // no restriction
                        row[1] = "0";
                        break;
                    case YesNoDefaultType.Default: // read-only recommended
                        row[1] = "2";
                        break;
                    case YesNoDefaultType.Yes: // read-only enforced
                        row[1] = "4";
                        break;
                }
            }
        }

        /// <summary>
        /// Parses an ignore modularization element.
        /// </summary>
        /// <param name="node">XmlNode on an IgnoreModulatization element.</param>
        private void ParseIgnoreModularizationElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string name = null;

            this.core.OnMessage(WixWarnings.DeprecatedIgnoreModularizationElement(sourceLineNumbers));

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Name":
                            name = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Type":
                            // this is actually not used
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixSuppressModularization");
                row[0] = name;
            }
        }

        /// <summary>
        /// Parses a permission element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="objectId">Identifier of object to be secured.</param>
        /// <param name="tableName">Name of table that contains objectId.</param>
        private void ParsePermissionElement(XmlNode node, string objectId, string tableName)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            BitArray bits = new BitArray(32);
            string domain = null;
            int permission = 0;
            string[] specialPermissions = null;
            string user = null;

            switch (tableName)
            {
                case "CreateFolder":
                    specialPermissions = Common.FolderPermissions;
                    break;
                case "File":
                    specialPermissions = Common.FilePermissions;
                    break;
                case "Registry":
                    specialPermissions = Common.RegistryPermissions;
                    break;
                default:
                    this.core.UnexpectedElement(node.ParentNode, node);
                    return; // stop processing this element since no valid permissions are available
            }

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Domain":
                            domain = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "User":
                            user = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            YesNoType attribValue = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if (!CompilerCore.NameToBit(Common.StandardPermissions, attrib.Name, attribValue, bits, 16))
                            {
                                if (!CompilerCore.NameToBit(Common.GenericPermissions, attrib.Name, attribValue, bits, 28))
                                {
                                    if (!CompilerCore.NameToBit(specialPermissions, attrib.Name, attribValue, bits, 0))
                                    {
                                        this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            permission = CompilerCore.ConvertBitArrayToInt32(bits);

            if (null == user)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "User"));
            }

            if (int.MinValue == permission) // just GENERIC_READ, which is MSI_NULL
            {
                this.core.OnMessage(WixErrors.GenericReadNotAllowed(sourceLineNumbers));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "LockPermissions");
                row[0] = objectId;
                row[1] = tableName;
                row[2] = domain;
                row[3] = user;
                row[4] = permission;
            }
        }

        /// <summary>
        /// Parses a product element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseProductElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int codepage = 0;
            string productCode = null;
            string upgradeCode = null;
            string manufacturer = null;
            Version version = null;

            this.activeName = null;
            this.activeLanguage = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            productCode = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, true);
                            if ("????????-????-????-????-????????????" == productCode)
                            {
                                this.core.OnMessage(WixWarnings.DeprecatedQuestionMarksGuid(sourceLineNumbers, node.Name, attrib.Name));
                                productCode = "*";
                            }
                            break;
                        case "Codepage":
                            codepage = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "Language":
                            this.activeLanguage = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Manufacturer":
                            manufacturer = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if ("PUT-COMPANY-NAME-HERE" == manufacturer)
                            {
                                this.core.OnMessage(WixWarnings.PlaceholderValue(sourceLineNumbers, node.Name, attrib.Name, manufacturer));
                            }
                            break;
                        case "Name":
                            this.activeName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if ("PUT-PRODUCT-NAME-HERE" == this.activeName)
                            {
                                this.core.OnMessage(WixWarnings.PlaceholderValue(sourceLineNumbers, node.Name, attrib.Name, this.activeName));
                            }
                            break;
                        case "UpgradeCode":
                            upgradeCode = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Version":
                            version = this.core.GetAttributeVersionValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == productCode)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == this.activeLanguage)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Language"));
            }

            if (null == manufacturer)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Manufacturer"));
            }

            if (null == this.activeName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (null == upgradeCode)
            {
                this.core.OnMessage(WixWarnings.MissingUpgradeCode(sourceLineNumbers));
            }

            if (null == version)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Version"));
            }

            if (!CompilerCore.IsValidProductVersion(version))
            {
                this.core.OnMessage(WixErrors.InvalidProductVersion(sourceLineNumbers, version.ToString()));
            }

            try
            {
                this.compilingProduct = true;
                this.core.CreateActiveSection(productCode, SectionType.Product, codepage);

                this.AddProperty(sourceLineNumbers, "Manufacturer", manufacturer, false, false, false);
                this.AddProperty(sourceLineNumbers, "ProductCode", productCode, false, false, false);
                this.AddProperty(sourceLineNumbers, "ProductLanguage", this.activeLanguage, false, false, false);
                this.AddProperty(sourceLineNumbers, "ProductName", this.activeName, false, false, false);
                this.AddProperty(sourceLineNumbers, "ProductVersion", version.ToString(), false, false, false);
                if (null != upgradeCode)
                {
                    this.AddProperty(sourceLineNumbers, "UpgradeCode", upgradeCode, false, false, false);
                }

                int featureDisplay = 0;
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (XmlNodeType.Element == child.NodeType)
                    {
                        if (child.NamespaceURI == this.schema.TargetNamespace)
                        {
                            switch (child.LocalName)
                            {
                                case "_locDefinition":
                                    break;
                                case "AdminExecuteSequence":
                                case "AdminUISequence":
                                case "AdvertiseExecuteSequence":
                                case "InstallExecuteSequence":
                                case "InstallUISequence":
                                    this.ParseSequenceElement(child, child.LocalName);
                                    break;
                                case "AppId":
                                    this.ParseAppIdElement(child, null, YesNoType.Yes, null, null, null);
                                    break;
                                case "Binary":
                                    this.ParseBinaryElement(child);
                                    break;
                                case "ComplianceCheck":
                                    this.ParseComplianceCheckElement(child);
                                    break;
                                case "Condition":
                                    this.ParseConditionElement(child, node.LocalName, null, null);
                                    break;
                                case "CustomAction":
                                    this.ParseCustomActionElement(child);
                                    break;
                                case "CustomActionRef":
                                    this.ParseSimpleRefElement(child, "CustomAction");
                                    break;
                                case "CustomTable":
                                    this.ParseCustomTableElement(child);
                                    break;
                                case "Directory":
                                    this.ParseDirectoryElement(child, null, CompilerCore.IntegerNotSet, String.Empty);
                                    break;
                                case "DirectoryRef":
                                    this.ParseDirectoryRefElement(child);
                                    break;
                                case "EnsureTable":
                                    this.ParseEnsureTableElement(child);
                                    break;
                                case "Feature":
                                    this.ParseFeatureElement(child, ComplexReferenceParentType.Product, productCode, ref featureDisplay);
                                    break;
                                case "FeatureRef":
                                    this.ParseFeatureRefElement(child, ComplexReferenceParentType.Product, productCode);
                                    break;
                                case "FeatureGroupRef":
                                    this.ParseFeatureGroupRefElement(child, ComplexReferenceParentType.Product, productCode);
                                    break;
                                case "Icon":
                                    this.ParseIconElement(child);
                                    break;
                                case "Media":
                                    this.ParseMediaElement(child, null);
                                    break;
                                case "Package":
                                    this.ParsePackageElement(child, manufacturer, null);
                                    break;
                                case "PatchCertificates":
                                    this.ParsePatchCertificatesElement(child);
                                    break;
                                case "Property":
                                    this.ParsePropertyElement(child);
                                    break;
                                case "PropertyRef":
                                    this.ParseSimpleRefElement(child, "Property");
                                    break;
                                case "SFPCatalog":
                                    string parentName = null;
                                    this.ParseSFPCatalogElement(child, ref parentName);
                                    break;
                                case "UI":
                                    this.ParseUIElement(child);
                                    break;
                                case "UIRef":
                                    this.ParseSimpleRefElement(child, "WixUI");
                                    break;
                                case "Upgrade":
                                    this.ParseUpgradeElement(child);
                                    break;
                                case "WixVariable":
                                    this.ParseWixVariableElement(child);
                                    break;
                                default:
                                    this.core.UnexpectedElement(node, child);
                                    break;
                            }
                        }
                        else
                        {
                            this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child);
                        }
                    }
                }
            }
            finally
            {
                this.compilingProduct = false;
            }
        }

        /// <summary>
        /// Parses a progid element
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="advertise">Flag if progid is advertised.</param>
        /// <param name="classId">CLSID related to ProgId.</param>
        /// <param name="description">Default description of ProgId</param>
        /// <param name="parent">Optional parent ProgId</param>
        /// <param name="foundExtension">Set to true if an extension is found; used for error-checking.</param>
        /// <param name="firstProgIdForClass">Whether or not this ProgId is the first one found in the parent class.</param>
        /// <returns>This element's Id.</returns>
        private string ParseProgIdElement(XmlNode node, string componentId, YesNoType advertise, string classId, string description, string parent, ref bool foundExtension, YesNoType firstProgIdForClass)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string icon = null;
            int iconIndex = CompilerCore.IntegerNotSet;
            string noOpen = null;
            string progId = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            progId = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Advertise":
                            YesNoType progIdAdvertise = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if ((YesNoType.No == advertise && YesNoType.Yes == progIdAdvertise) || (YesNoType.Yes == advertise && YesNoType.No == progIdAdvertise))
                            {
                                this.core.OnMessage(WixErrors.AdvertiseStateMustMatch(sourceLineNumbers, advertise.ToString(CultureInfo.InvariantCulture.NumberFormat), progIdAdvertise.ToString(CultureInfo.InvariantCulture.NumberFormat)));
                            }
                            advertise = progIdAdvertise;
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib, true);
                            break;
                        case "Icon":
                            icon = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "IconIndex":
                            iconIndex = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, short.MinValue + 1, short.MaxValue);
                            break;
                        case "NoOpen":
                            noOpen = this.core.GetAttributeValue(sourceLineNumbers, attrib, true);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (YesNoType.NotSet == advertise)
            {
                advertise = YesNoType.No;
            }

            if (null != parent && (null != icon || CompilerCore.IntegerNotSet != iconIndex))
            {
                this.core.OnMessage(WixErrors.VersionIndependentProgIdsCannotHaveIcons(sourceLineNumbers));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Extension":
                                this.ParseExtensionElement(child, componentId, advertise, progId);
                                foundExtension = true;
                                break;
                            case "ProgId":
                                // Only allow one nested ProgId.  If we have a child, we should not have a parent.
                                if (null == parent)
                                {
                                    if (YesNoType.Yes == advertise)
                                    {
                                        this.ParseProgIdElement(child, componentId, advertise, null, description, progId, ref foundExtension, firstProgIdForClass);
                                    }
                                    else if (YesNoType.No == advertise)
                                    {
                                        this.ParseProgIdElement(child, componentId, advertise, classId, description, progId, ref foundExtension, firstProgIdForClass);
                                    }
                                }
                                else
                                {
                                    SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                                    this.core.OnMessage(WixErrors.ProgIdNestedTooDeep(childSourceLineNumbers));
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (YesNoType.Yes == advertise)
            {
                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "ProgId");
                    row[0] = progId;
                    row[1] = parent;
                    row[2] = classId;
                    row[3] = description;
                    if (null != icon)
                    {
                        row[4] = icon;
                        this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Icon", icon);
                    }

                    if (CompilerCore.IntegerNotSet != iconIndex)
                    {
                        row[5] = iconIndex;
                    }
                }
            }
            else if (YesNoType.No == advertise)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, progId, String.Empty, description, componentId);
                if (null != classId)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(progId, "\\CLSID"), String.Empty, classId, componentId);
                    if (null != parent)   // if this is a version independent ProgId
                    {
                        if (YesNoType.Yes == firstProgIdForClass)
                        {
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\VersionIndependentProgID"), String.Empty, progId, componentId);
                        }

                        this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(progId, "\\CurVer"), String.Empty, parent, componentId);
                    }
                    else
                    {
                        if (YesNoType.Yes == firstProgIdForClass)
                        {
                            this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat("CLSID\\", classId, "\\ProgID"), String.Empty, progId, componentId);
                        }
                    }
                }

                if (null != icon)   // ProgId's Default Icon
                {
                    this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", icon);

                    icon = String.Format(CultureInfo.InvariantCulture, "\"[#{0}]\"", icon);

                    if (CompilerCore.IntegerNotSet != iconIndex)
                    {
                        icon = String.Concat(icon, ",", iconIndex);
                    }

                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(progId, "\\DefaultIcon"), String.Empty, icon, componentId);
                }
            }

            if (null != noOpen)
            {
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, progId, "NoOpen", noOpen, componentId); // ProgId NoOpen name
            }

            // raise an error for an orphaned ProgId
            if (YesNoType.Yes == advertise && !foundExtension && null == parent && null == classId)
            {
                this.core.OnMessage(WixWarnings.OrphanedProgId(sourceLineNumbers, progId));
            }

            return progId;
        }

        /// <summary>
        /// Parses a property element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParsePropertyElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            bool admin = false;
            bool complianceCheck = false;
            bool hidden = false;
            bool secure = false;
            YesNoType suppressModularization = YesNoType.NotSet;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Admin":
                            admin = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "ComplianceCheck":
                            complianceCheck = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Hidden":
                            hidden = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Secure":
                            secure = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "SuppressModularization":
                            suppressModularization = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }
            else if ("ProductID" == id)
            {
                this.core.OnMessage(WixWarnings.ProductIDAuthored(sourceLineNumbers));
            }

            if ("SecureCustomProperties" == id || "AdminProperties" == id || "MsiHiddenProperties" == id)
            {
                this.core.OnMessage(WixErrors.CannotAuthorSpecialProperties(sourceLineNumbers, id));
            }

            string innerText = CompilerCore.GetTrimmedInnerText(node);
            if (null != value)
            {
                // cannot specify both the value attribute and inner text
                if (0 != innerText.Length)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithInnerText(sourceLineNumbers, node.Name, "Value"));
                }
            }
            else // value attribute not specified
            {
                if (0 < innerText.Length)
                {
                    value = innerText;
                }
            }

            if ("ErrorDialog" == id)
            {
                this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Dialog", value);
            }

            // see if this property is used for appSearch
            ArrayList signatures = this.ParseSearchSignatures(node);

            // if we're doing CCP
            if (complianceCheck)
            {
                // there must be a signature
                if (0 == signatures.Count)
                {
                    this.core.OnMessage(WixErrors.SearchElementRequiredWithAttribute(sourceLineNumbers, node.Name, "ComplianceCheck", "yes"));
                }
            }

            foreach (string sig in signatures)
            {
                if (complianceCheck && !this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "CCPSearch");
                    row[0] = sig;
                }

                this.AddAppSearch(sourceLineNumbers, id, sig);
            }

            // if we're doing AppSearch get that setup
            if (0 < signatures.Count)
            {
                this.AddProperty(sourceLineNumbers, id, value, admin, secure, hidden);
            }
            else // just a normal old property
            {
                // if the property value is empty and none of the flags are set, print out a warning that we're ignoring
                // the element
                if ((null == value || 0 == value.Length) && !admin && !secure && !hidden)
                {
                    this.core.OnMessage(WixWarnings.PropertyUseless(sourceLineNumbers, id));
                }
                else // there is a value and/or a flag set, do that
                {
                    this.AddProperty(sourceLineNumbers, id, value, admin, secure, hidden);
                }
            }

            if (!this.core.EncounteredError && YesNoType.Yes == suppressModularization)
            {
                this.core.OnMessage(WixWarnings.PropertyModularizationSuppressed(sourceLineNumbers));

                Row row = this.core.CreateRow(sourceLineNumbers, "WixSuppressModularization");
                row[0] = id;
            }
        }

        /// <summary>
        /// Parses a RegistryKey element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        /// <param name="root">Root specified when element is nested under another Registry element, otherwise CompilerCore.IntegerNotSet.</param>
        /// <param name="parentKey">Parent key for this Registry element when nested.</param>
        /// <param name="possibleKeyPath">Identifier of this registry key since it could be the component's keypath.</param>
        /// <returns>True if registry element is marked as component's keypath</returns>
        private bool ParseRegistryKeyElement(XmlNode node, string componentId, int root, string parentKey, out string possibleKeyPath)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string key = parentKey; // default to parent key path
            string name = null;
            string action = null;
            bool keyPath = false;
            bool couldBeKeyPath = true; // assume that this is a regular registry key that could become the key path

            possibleKeyPath = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            action = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (action)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "create":
                                    name = "+";
                                    break;
                                case "createAndRemoveOnUninstall":
                                    name = "*";
                                    break;
                                case "none":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, action, "create", "createAndRemoveOnUninstall", "none"));
                                    break;
                            }
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if (null != parentKey)
                            {
                                if (parentKey.EndsWith("\\"))
                                {
                                    key = String.Concat(parentKey, key);
                                }
                                else
                                {
                                    key = String.Concat(parentKey, "\\", key);
                                }
                            }
                            break;
                        case "Root":
                            if (CompilerCore.IntegerNotSet != root)
                            {
                                this.core.OnMessage(WixErrors.RegistryRootInvalid(sourceLineNumbers));
                            }

                            string rootString = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (rootString)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "HKMU":
                                    root = -1;
                                    break;
                                case "HKCR":
                                    root = MsiInterop.MsidbRegistryRootClassesRoot;
                                    break;
                                case "HKCU":
                                    root = MsiInterop.MsidbRegistryRootCurrentUser;
                                    break;
                                case "HKLM":
                                    root = MsiInterop.MsidbRegistryRootLocalMachine;
                                    break;
                                case "HKU":
                                    root = MsiInterop.MsidbRegistryRootUsers;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, rootString, "HKMU", "HKCR", "HKCU", "HKLM", "HKU"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null != name) // generates a Registry row, so an Id must be present
            {
                // generate the identifier if it wasn't provided
                if (null == id)
                {
                    id = CompilerCore.GenerateIdentifier("Registry", componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), (null != key ? key.ToLower(CultureInfo.InvariantCulture) : key), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
                }
            }
            else // does not generate a Registry row, so no Id should be present
            {
                if (null != id)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Id", "Action", "none"));
                }
            }

            if (CompilerCore.IntegerNotSet == root)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Root"));
                root = CompilerCore.IllegalInteger;
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
                key = String.Empty; // set the key to something to prevent null reference exceptions
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        string possibleChildKeyPath = null;

                        switch (child.LocalName)
                        {
                            case "RegistryKey":
                                if (this.ParseRegistryKeyElement(child, componentId, root, key, out possibleChildKeyPath))
                                {
                                    if (keyPath)
                                    {
                                        this.core.OnMessage(WixErrors.ComponentMultipleKeyPaths(sourceLineNumbers, child.Name, "KeyPath", "yes", "File", "RegistryValue", "ODBCDataSource"));
                                    }

                                    possibleKeyPath = possibleChildKeyPath; // the child is the key path
                                    keyPath = true;
                                }
                                break;
                            case "RegistryValue":
                                if (this.ParseRegistryValueElement(child, componentId, root, key, out possibleChildKeyPath))
                                {
                                    if (keyPath)
                                    {
                                        this.core.OnMessage(WixErrors.ComponentMultipleKeyPaths(sourceLineNumbers, child.Name, "KeyPath", "yes", "File", "RegistryValue", "ODBCDataSource"));
                                    }

                                    possibleKeyPath = possibleChildKeyPath; // the child is the key path
                                    keyPath = true;
                                }
                                break;
                            case "Permission":
                                if (action != "createAndRemoveOnUninstall" && action != "create")
                                {
                                    this.core.OnMessage(WixErrors.UnexpectedElementWithAttributeValue(sourceLineNumbers, node.Name, child.Name, "Action", "create", "createAndRemoveOnUninstall"));
                                }
                                this.ParsePermissionElement(child, id, "Registry");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, componentId);
                    }
                }
            }

            if (!this.core.EncounteredError && null != name)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Registry");
                row[0] = id;
                row[1] = root;
                row[2] = key;
                row[3] = name;
                row[4] = null;
                row[5] = componentId;
            }

            // If this was just a regular registry key (that could be the key path)
            // and no child registry key set the possible key path, let's make this
            // Registry/@Id a possible key path.
            if (couldBeKeyPath && null == possibleKeyPath)
            {
                possibleKeyPath = id;
            }

            return keyPath;
        }

        /// <summary>
        /// Parses a RegistryValue element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        /// <param name="root">Root specified when element is nested under a RegistryKey element, otherwise CompilerCore.IntegerNotSet.</param>
        /// <param name="parentKey">Root specified when element is nested under a RegistryKey element, otherwise CompilerCore.IntegerNotSet.</param>
        /// <param name="possibleKeyPath">Identifier of this registry key since it could be the component's keypath.</param>
        /// <returns>True if registry element is marked as component's keypath</returns>
        private bool ParseRegistryValueElement(XmlNode node, string componentId, int root, string parentKey, out string possibleKeyPath)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string key = parentKey; // default to parent key path
            string name = null;
            string value = null;
            string type = null;
            string action = null;
            bool keyPath = false;
            bool couldBeKeyPath = true; // assume that this is a regular registry key that could become the key path

            possibleKeyPath = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            action = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (action)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "append":
                                case "prepend":
                                case "write":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, action, "append", "prepend", "write"));
                                    break;
                            }
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if (null != parentKey)
                            {
                                if (parentKey.EndsWith("\\"))
                                {
                                    key = String.Concat(parentKey, key);
                                }
                                else
                                {
                                    key = String.Concat(parentKey, "\\", key);
                                }
                            }
                            break;
                        case "KeyPath":
                            keyPath = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Root":
                            if (CompilerCore.IntegerNotSet != root)
                            {
                                this.core.OnMessage(WixErrors.RegistryRootInvalid(sourceLineNumbers));
                            }

                            string rootString = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (rootString)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "HKMU":
                                    root = -1;
                                    break;
                                case "HKCR":
                                    root = MsiInterop.MsidbRegistryRootClassesRoot;
                                    break;
                                case "HKCU":
                                    root = MsiInterop.MsidbRegistryRootCurrentUser;
                                    break;
                                case "HKLM":
                                    root = MsiInterop.MsidbRegistryRootLocalMachine;
                                    break;
                                case "HKU":
                                    root = MsiInterop.MsidbRegistryRootUsers;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, rootString, "HKMU", "HKCR", "HKCU", "HKLM", "HKU"));
                                    break;
                            }
                            break;
                        case "Type":
                            type = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (type)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "binary":
                                case "expandable":
                                case "integer":
                                case "multiString":
                                case "string":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, type, "binary", "expandable", "integer", "multiString", "string"));
                                    break;
                            }
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib, true);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // generate the identifier if it wasn't provided
            if (null == id)
            {
                id = CompilerCore.GenerateIdentifier("Registry", componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), (null != key ? key.ToLower(CultureInfo.InvariantCulture) : key), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
            }

            if (("append" == action || "prepend" == action) && "multiString" != type)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeValueWithoutOtherAttribute(sourceLineNumbers, node.Name, "Action", action, "Type", "multiString"));
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
            }

            if (CompilerCore.IntegerNotSet == root)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Root"));
            }

            if (null == type)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Type"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "MultiStringValue":
                                if ("multiString" != type && null != value)
                                {
                                    this.core.OnMessage(WixErrors.RegistryMultipleValuesWithoutMultiString(sourceLineNumbers, node.Name, "Value", child.Name, "Type"));
                                }
                                else if (null == value)
                                {
                                    value = child.InnerText;
                                }
                                else
                                {
                                    value = String.Concat(value, "[~]", child.InnerText);
                                }
                                break;
                            case "Permission":
                                this.ParsePermissionElement(child, id, "Registry");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, componentId);
                    }
                }
            }

            switch (type)
            {
                case "binary":
                    value = String.Concat("#x", value);
                    break;
                case "expandable":
                    value = String.Concat("#%", value);
                    break;
                case "integer":
                    value = String.Concat("#", value);
                    break;
                case "multiString":
                    switch (action)
                    {
                        case "append":
                            value = String.Concat("[~]", value);
                            break;
                        case "prepend":
                            value = String.Concat(value, "[~]");
                            break;
                        case "write":
                        default:
                            if (null != value && -1 == value.IndexOf("[~]"))
                            {
                                value = String.Format(CultureInfo.InvariantCulture, "[~]{0}[~]", value);
                            }
                            break;
                    }
                    break;
                case "string":
                    // escape the leading '#' character for string registry keys
                    if (null != value && value.StartsWith("#"))
                    {
                        value = String.Concat("#", value);
                    }
                    break;
            }

            // value may be set by child MultiStringValue elements, so it must be checked here
            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }
            else if (0 == value.Length && ("+" == name || "-" == name || "*" == name)) // prevent accidental authoring of special name values
            {
                this.core.OnMessage(WixErrors.RegistryNameValueIncorrect(sourceLineNumbers, node.Name, "Name", name));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Registry");
                row[0] = id;
                row[1] = root;
                row[2] = key;
                row[3] = name;
                row[4] = value;
                row[5] = componentId;
            }

            // If this was just a regular registry key (that could be the key path)
            // and no child registry key set the possible key path, let's make this
            // Registry/@Id a possible key path.
            if (couldBeKeyPath && null == possibleKeyPath)
            {
                possibleKeyPath = id;
            }

            return keyPath;
        }

        /// <summary>
        /// Parses a RemoveRegistryKey element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="componentId">The component identifier of the parent element.</param>
        private void ParseRemoveRegistryKeyElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string action = null;
            string key = null;
            string name = "-";
            int root = CompilerCore.IntegerNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            action = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (action)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "removeOnInstall":
                                case "removeOnUninstall":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, action, "removeOnInstall", "removeOnUninstall"));
                                    break;
                            }
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Root":
                            string rootString = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (rootString)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "HKMU":
                                    root = -1;
                                    break;
                                case "HKCR":
                                    root = MsiInterop.MsidbRegistryRootClassesRoot;
                                    break;
                                case "HKCU":
                                    root = MsiInterop.MsidbRegistryRootCurrentUser;
                                    break;
                                case "HKLM":
                                    root = MsiInterop.MsidbRegistryRootLocalMachine;
                                    break;
                                case "HKU":
                                    root = MsiInterop.MsidbRegistryRootUsers;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, rootString, "HKMU", "HKCR", "HKCU", "HKLM", "HKU"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // generate the identifier if it wasn't provided
            if (null == id)
            {
                id = CompilerCore.GenerateIdentifier(("removeOnUninstall" == action ? "Registry" : "RemoveRegistry"), componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), (null != key ? key.ToLower(CultureInfo.InvariantCulture) : key), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
            }

            if (null == action)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Action"));
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
            }

            if (CompilerCore.IntegerNotSet == root)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Root"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, ("removeOnUninstall" == action ? "Registry" : "RemoveRegistry"));
                row[0] = id;
                row[1] = root;
                row[2] = key;
                row[3] = name;
                if ("removeOnUninstall" == action) // Registry table
                {
                    row[4] = null;
                    row[5] = componentId;
                }
                else // RemoveRegistry table
                {
                    row[4] = componentId;
                }
            }
        }

        /// <summary>
        /// Parses a RemoveRegistryValue element.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="componentId">The component identifier of the parent element.</param>
        private void ParseRemoveRegistryValueElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string key = null;
            string name = null;
            int root = CompilerCore.IntegerNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Root":
                            string rootString = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (rootString)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "HKMU":
                                    root = -1;
                                    break;
                                case "HKCR":
                                    root = MsiInterop.MsidbRegistryRootClassesRoot;
                                    break;
                                case "HKCU":
                                    root = MsiInterop.MsidbRegistryRootCurrentUser;
                                    break;
                                case "HKLM":
                                    root = MsiInterop.MsidbRegistryRootLocalMachine;
                                    break;
                                case "HKU":
                                    root = MsiInterop.MsidbRegistryRootUsers;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, rootString, "HKMU", "HKCR", "HKCU", "HKLM", "HKU"));
                                    break;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // generate the identifier if it wasn't provided
            if (null == id)
            {
                id = CompilerCore.GenerateIdentifier("RemoveRegistry", componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), (null != key ? key.ToLower(CultureInfo.InvariantCulture) : key), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
            }

            if (CompilerCore.IntegerNotSet == root)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Root"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "RemoveRegistry");
                row[0] = id;
                row[1] = root;
                row[2] = key;
                row[3] = name;
                row[4] = componentId;
            }
        }

        /// <summary>
        /// Parses a registry element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        /// <param name="root">Root specified when element is nested under another Registry element, otherwise CompilerCore.IntegerNotSet.</param>
        /// <param name="parentKey">Parent key for this Registry element when nested.</param>
        /// <param name="possibleKeyPath">Identifier of this registry key since it could be the component's keypath.</param>
        /// <returns>True if registry element is marked as component's keypath</returns>
        private bool ParseRegistryElement(XmlNode node, string componentId, int root, string parentKey, out string possibleKeyPath)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string key = parentKey; // default to parent key path
            string name = null;
            string value = null;
            string type = null;
            string action = null;
            bool keyPath = false;
            bool couldBeKeyPath = true; // assume that this is a regular registry key that could become the key path

            possibleKeyPath = null;

            this.core.OnMessage(WixWarnings.DeprecatedRegistryElement(sourceLineNumbers));

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            action = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (action)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "append":
                                case "createKey":
                                case "createKeyAndRemoveKeyOnUninstall":
                                case "prepend":
                                case "remove":
                                case "removeKeyOnInstall":
                                case "removeKeyOnUninstall":
                                case "write":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, action, "append", "createKey", "createKeyAndRemoveKeyOnUninstall", "prepend", "remove", "removeKeyOnInstall", "removeKeyOnUninstall", "write"));
                                    break;
                            }
                            break;
                        case "Key":
                            key = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if (null != parentKey)
                            {
                                if (parentKey.EndsWith("\\"))
                                {
                                    key = String.Concat(parentKey, key);
                                }
                                else
                                {
                                    key = String.Concat(parentKey, "\\", key);
                                }
                            }
                            break;
                        case "KeyPath":
                            keyPath = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Root":
                            if (CompilerCore.IntegerNotSet != root)
                            {
                                this.core.OnMessage(WixErrors.RegistryRootInvalid(sourceLineNumbers));
                            }

                            string rootString = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (rootString)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "HKMU":
                                    root = -1;
                                    break;
                                case "HKCR":
                                    root = MsiInterop.MsidbRegistryRootClassesRoot;
                                    break;
                                case "HKCU":
                                    root = MsiInterop.MsidbRegistryRootCurrentUser;
                                    break;
                                case "HKLM":
                                    root = MsiInterop.MsidbRegistryRootLocalMachine;
                                    break;
                                case "HKU":
                                    root = MsiInterop.MsidbRegistryRootUsers;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, rootString, "HKMU", "HKCR", "HKCU", "HKLM", "HKU"));
                                    break;
                            }
                            break;
                        case "Type":
                            type = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (type)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue: // this error is already handled
                                    break;
                                case "binary":
                                case "expandable":
                                case "integer":
                                case "multiString":
                                case "string":
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, type, "binary", "expandable", "integer", "multiString", "string"));
                                    break;
                            }
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (CompilerCore.IntegerNotSet == root)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Root"));
                root = CompilerCore.IllegalInteger;
            }

            if (null == key)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Key"));
                key = String.Empty; // set the key to something to prevent null reference exceptions
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Permission":
                                // We need to handle this below the generation of the id, because the id is an
                                // input into the Permission element.
                                break;
                            case "Registry":
                                if ("remove" == action || "removeKeyOnInstall" == action)
                                {
                                    this.core.OnMessage(WixErrors.RegistrySubElementCannotBeRemoved(sourceLineNumbers, node.Name, child.Name, "Action", "remove", "removeKeyOnInstall"));
                                }

                                string possibleChildKeyPath = null;
                                bool childIsKeyPath = this.ParseRegistryElement(child, componentId, root, key, out possibleChildKeyPath);
                                if (childIsKeyPath)
                                {
                                    if (keyPath)
                                    {
                                        this.core.OnMessage(WixErrors.ComponentMultipleKeyPaths(sourceLineNumbers, child.Name, "KeyPath", "yes", "File", "RegistryValue", "ODBCDataSource"));
                                    }

                                    possibleKeyPath = possibleChildKeyPath; // the child is the key path
                                    keyPath = true;
                                }

                                break;
                            case "RegistryValue":
                                if ("remove" == action || "removeKeyOnInstall" == action)
                                {
                                    this.core.OnMessage(WixErrors.RegistrySubElementCannotBeRemoved(sourceLineNumbers, node.Name, child.Name, "Action", "remove", "removeKeyOnInstall"));
                                }

                                if ("multiString" != type && null != value)
                                {
                                    this.core.OnMessage(WixErrors.RegistryMultipleValuesWithoutMultiString(sourceLineNumbers, node.Name, "Value", child.Name, "Type"));
                                }
                                else if (null == value)
                                {
                                    value = child.InnerText;
                                }
                                else
                                {
                                    value = String.Concat(value, "[~]", child.InnerText);
                                }
                                break;

                            // We need to other elements this below the generation of the id, because the id is an
                            // input into extension elements.  This also means that we need to re-parse these elements below.
                        }
                    }
                }
            }

            if ("remove" == action || "removeKeyOnInstall" == action) // RemoveRegistry table
            {
                if (keyPath)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "KeyPath", "Action", action));
                }

                if (null != value)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Value", "Action", action));
                }

                if (null != type)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Type", "Action", action));
                }

                if ("removeKeyOnInstall" == action)
                {
                    if (null != name)
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Name", "Action", action));
                    }

                    name = "-";
                }

                // this cannot be a KeyPath
                couldBeKeyPath = false;

                // generate the identifier if it wasn't provided
                if (null == id)
                {
                    id = CompilerCore.GenerateIdentifier(node.LocalName, componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), key.ToLower(CultureInfo.InvariantCulture), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "RemoveRegistry");
                    row[0] = id;
                    row[1] = root;
                    row[2] = key;
                    row[3] = name;
                    row[4] = componentId;
                }

                this.core.EnsureTable(sourceLineNumbers, "Registry"); // RemoveRegistry table requires the Registry table
            }
            else // Registry table
            {
                if (("append" == action || "prepend" == action) && "multiString" != type)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeValueWithoutOtherAttribute(sourceLineNumbers, node.Name, "Action", action, "Type", "multiString"));
                }

                if ("createKey" == action || "createKeyAndRemoveKeyOnUninstall" == action || "removeKeyOnUninstall" == action)
                {
                    if (keyPath)
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "KeyPath", "Action", action));
                    }

                    if (null != name)
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Name", "Action", action));
                    }

                    if (null != value)
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Value", "Action", action));
                    }

                    // this cannot be a KeyPath
                    couldBeKeyPath = false;
                }

                if (null != value)
                {
                    if ("removeKeyOnUninstall" == action || "writeKeyAndRemoveKeyOnUninstall" == action)
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Value", "Action", action));
                    }

                    if (null == type)
                    {
                        this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Type", "Value"));
                    }

                    switch (type)
                    {
                        case "binary":
                            value = String.Concat("#x", value);
                            break;
                        case "expandable":
                            value = String.Concat("#%", value);
                            break;
                        case "integer":
                            value = String.Concat("#", value);
                            break;
                        case "multiString":
                            switch (action)
                            {
                                case "append":
                                    value = String.Concat("[~]", value);
                                    break;
                                case "prepend":
                                    value = String.Concat(value, "[~]");
                                    break;
                                case "write":
                                default:
                                    if (-1 == value.IndexOf("[~]"))
                                    {
                                        value = String.Format(CultureInfo.InvariantCulture, "[~]{0}[~]", value);
                                    }
                                    break;
                            }
                            break;
                        case "string":
                            // escape the leading '#' character for string registry keys
                            if (value.StartsWith("#"))
                            {
                                value = String.Concat("#", value);
                            }
                            break;
                    }
                }
                else // no value
                {
                    if (null == name) // no name or value
                    {
                        if (null != type)
                        {
                            this.core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "Type", "Name", "Value"));
                        }

                        switch (action)
                        {
                            case "createKey":
                                name = "+";
                                break;
                            case "createKeyAndRemoveKeyOnUninstall":
                                name = "*";
                                break;
                            case "removeKeyOnUninstall":
                                name = "-";
                                break;
                        }
                    }
                    else // name specified, no value
                    {
                        if ("+" == name || "-" == name || "*" == name)
                        {
                            this.core.OnMessage(WixErrors.RegistryNameValueIncorrect(sourceLineNumbers, node.Name, "Name", name));

                            if (keyPath)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "KeyPath", "Name", name));
                            }
                        }
                    }

                    if ("multiString" == type)
                    {
                        value = "[~][~]";
                    }
                }

                // generate the identifier if it wasn't provided
                if (null == id)
                {
                    id = CompilerCore.GenerateIdentifier(node.LocalName, componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), key.ToLower(CultureInfo.InvariantCulture), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "Registry");
                    row[0] = id;
                    row[1] = root;
                    row[2] = key;
                    row[3] = name;
                    row[4] = value;
                    row[5] = componentId;
                }
            }

            // This looks different from all the others, because the id must be generated before
            // we get here.
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Permission":
                                if ("remove" == action || "removeKeyOnInstall" == action)
                                {
                                    this.core.OnMessage(WixErrors.RegistrySubElementCannotBeRemoved(sourceLineNumbers, node.Name, child.Name, "Action", "remove", "removeKeyOnInstall"));
                                }
                                this.ParsePermissionElement(child, id, node.LocalName);
                                break;
                            case "Registry":
                            case "RegistryValue":
                                // these were parsed above; they are parsed again here to keep them from falling into the default case
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, componentId);
                    }
                }
            }

            // If this was just a regular registry key (that could be the key path)
            // and no child registry key set the possible key path, let's make this
            // Registry/@Id a possible key path.
            if (couldBeKeyPath && null == possibleKeyPath)
            {
                possibleKeyPath = id;
            }

            return keyPath;
        }

        /// <summary>
        /// Parses a remove file element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="parentDirectory">Identifier of the parent component's directory.</param>
        private void ParseRemoveFileElement(XmlNode node, string componentId, string parentDirectory)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string directory = null;
            string longName = null;
            string name = null;
            int on = CompilerCore.IntegerNotSet;
            string property = null;
            string shortName = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Directory":
                            directory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", directory);
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, true);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "Name":
                            name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, true);
                            break;
                        case "On":
                            string onValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (onValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    on = CompilerCore.IllegalInteger;
                                    break;
                                case "install":
                                    on = 1;
                                    break;
                                case "uninstall":
                                    on = 2;
                                    break;
                                case "both":
                                    on = 3;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "On", onValue, "install", "uninstall", "both"));
                                    on = CompilerCore.IllegalInteger;
                                    break;
                            }
                            break;
                        case "Property":
                            property = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, true);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }
            else if (CompilerCore.IllegalEmptyAttributeValue != name)
            {
                if (CompilerCore.IsValidShortFilename(name, true))
                {
                    if (null == shortName)
                    {
                        shortName = name;
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                    }
                }
                else
                {
                    if (null == longName)
                    {
                        longName = name;

                        // generate a short file name
                        if (null == shortName)
                        {
                            shortName = CompilerCore.GenerateShortName(name, true, true, node.LocalName, componentId);
                        }
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                    }
                }
            }

            if (CompilerCore.IntegerNotSet == on)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "On"));
                on = CompilerCore.IllegalInteger;
            }

            if (null != directory && null != property)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Property", "Directory", directory));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "RemoveFile");
                row[0] = id;
                row[1] = componentId;
                row[2] = GetMsiFilenameValue(shortName, longName);
                if (null != directory)
                {
                    row[3] = directory;
                }
                else if (null != property)
                {
                    row[3] = property;
                }
                else
                {
                    row[3] = parentDirectory;
                }
                row[4] = on;
            }
        }

        /// <summary>
        /// Parses a RemoveFolder element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="parentDirectory">Identifier of parent component's directory.</param>
        private void ParseRemoveFolderElement(XmlNode node, string componentId, string parentDirectory)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string directory = null;
            int on = CompilerCore.IntegerNotSet;
            string property = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Directory":
                            directory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", directory);
                            break;
                        case "On":
                            string onValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (onValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    on = CompilerCore.IllegalInteger;
                                    break;
                                case "install":
                                    on = 1;
                                    break;
                                case "uninstall":
                                    on = 2;
                                    break;
                                case "both":
                                    on = 3;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "On", onValue, "install", "uninstall", "both"));
                                    on = CompilerCore.IllegalInteger;
                                    break;
                            }
                            break;
                        case "Property":
                            property = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (CompilerCore.IntegerNotSet == on)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "On"));
                on = CompilerCore.IllegalInteger;
            }

            if (null != directory && null != property)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Property", "Directory", directory));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "RemoveFile");
                row[0] = id;
                row[1] = componentId;
                row[2] = null;
                if (null != directory)
                {
                    row[3] = directory;
                }
                else if (null != property)
                {
                    row[3] = property;
                }
                else
                {
                    row[3] = parentDirectory;
                }
                row[4] = on;
            }
        }

        /// <summary>
        /// Parses a reserve cost element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="directoryId">Optional and default identifier of referenced directory.</param>
        private void ParseReserveCostElement(XmlNode node, string componentId, string directoryId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int runFromSource = CompilerCore.IntegerNotSet;
            int runLocal = CompilerCore.IntegerNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Directory":
                            directoryId = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", directoryId);
                            break;
                        case "RunFromSource":
                            runFromSource = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "RunLocal":
                            runLocal = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (CompilerCore.IntegerNotSet == runFromSource)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "RunFromSource"));
            }

            if (CompilerCore.IntegerNotSet == runLocal)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "RunLocal"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ReserveCost");
                row[0] = id;
                row[1] = componentId;
                row[2] = directoryId;
                row[3] = runLocal;
                row[4] = runFromSource;
            }
        }

        /// <summary>
        /// Parses a sequence element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="sequenceTable">Name of sequence table.</param>
        private void ParseSequenceElement(XmlNode node, string sequenceTable)
        {
            // use the proper table name internally
            if ("AdvertiseExecuteSequence" == sequenceTable)
            {
                sequenceTable = "AdvtExecuteSequence";
            }

            // Parse each action in the sequence.
            foreach (XmlNode child in node.ChildNodes)
            {
                if (!(child is XmlElement))   // only process elements here
                {
                    continue;
                }

                SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                string actionName = child.LocalName;
                string afterAction = null;
                string beforeAction = null;
                string condition = null;
                bool customAction = "Custom" == actionName;
                bool overridable = false;
                int sequence = CompilerCore.IntegerNotSet;
                bool showDialog = "Show" == actionName;
                bool specialAction = "InstallExecute" == actionName || "InstallExecuteAgain" == actionName || "RemoveExistingProducts" == actionName || "DisableRollback" == actionName || "ScheduleReboot" == actionName || "ForceReboot" == actionName || "ResolveSource" == actionName;
                bool specialStandardAction = "AppSearch" == actionName || "CCPSearch" == actionName || "RMCCPSearch" == actionName || "LaunchConditions" == actionName || "FindRelatedProducts" == actionName;
                bool suppress = false;

                foreach (XmlAttribute attrib in child.Attributes)
                {
                    switch (attrib.LocalName)
                    {
                        case "Action":
                            if (!customAction)
                            {
                                this.core.UnexpectedAttribute(childSourceLineNumbers, attrib);
                            }
                            actionName = this.core.GetAttributeIdentifierValue(childSourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(childSourceLineNumbers, "CustomAction", actionName);
                            break;
                        case "After":
                            if (!customAction && !showDialog && !specialAction && !specialStandardAction)
                            {
                                this.core.UnexpectedAttribute(childSourceLineNumbers, attrib); // only valid for Custom actions and Show dialogs
                            }
                            afterAction = this.core.GetAttributeIdentifierValue(childSourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(childSourceLineNumbers, "WixAction", sequenceTable, afterAction);
                            break;
                        case "Before":
                            if (!customAction && !showDialog && !specialAction && !specialStandardAction)
                            {
                                this.core.UnexpectedAttribute(childSourceLineNumbers, attrib); // only valid for Custom actions and Show dialogs
                            }
                            beforeAction = this.core.GetAttributeIdentifierValue(childSourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(childSourceLineNumbers, "WixAction", sequenceTable, beforeAction);
                            break;
                        case "Dialog":
                            if (!showDialog)
                            {
                                this.core.UnexpectedAttribute(childSourceLineNumbers, attrib);
                            }
                            actionName = this.core.GetAttributeIdentifierValue(childSourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(childSourceLineNumbers, "Dialog", actionName);
                            break;
                        case "OnExit":
                            if (!customAction && !showDialog && !specialAction)
                            {
                                this.core.UnexpectedAttribute(childSourceLineNumbers, attrib);
                            }
                            else if (CompilerCore.IntegerNotSet != sequence)
                            {
                                this.core.OnMessage(WixErrors.CannotSpecifySequenceAndOnExit(childSourceLineNumbers, child.Name));
                            }

                            string onExitValue = this.core.GetAttributeValue(childSourceLineNumbers, attrib);
                            switch (onExitValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "success":
                                    sequence = -1;
                                    break;
                                case "cancel":
                                    sequence = -2;
                                    break;
                                case "error":
                                    sequence = -3;
                                    break;
                                case "suspend":
                                    sequence = -4;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(childSourceLineNumbers, child.Name, "OnExit", onExitValue, "success", "cancel", "error", "suspend"));
                                    break;
                            }
                            break;
                        case "Overridable":
                            overridable = (YesNoType.Yes == this.core.GetAttributeYesNoValue(childSourceLineNumbers, attrib));
                            break;
                        case "Sequence":
                            if (CompilerCore.IntegerNotSet != sequence)
                            {
                                this.core.OnMessage(WixErrors.CannotSpecifySequenceAndOnExit(childSourceLineNumbers, child.Name));
                            }

                            sequence = this.core.GetAttributeIntegerValue(childSourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "Suppress":
                            suppress = YesNoType.Yes == this.core.GetAttributeYesNoValue(childSourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(childSourceLineNumbers, attrib);
                            break;
                    }
                }

                // Get the condition from the inner text of the element.
                condition = CompilerCore.GetConditionInnerText(child);

                if (customAction && "Custom" == actionName)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(childSourceLineNumbers, child.Name, "Action"));
                }
                else if (showDialog && "Show" == actionName)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(childSourceLineNumbers, child.Name, "Dialog"));
                }

                if (CompilerCore.IntegerNotSet != sequence && (null != beforeAction || null != afterAction))
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(childSourceLineNumbers, child.Name, "Sequence", "Before", "After"));
                }

                if (null != beforeAction && null != afterAction)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(childSourceLineNumbers, child.Name, "After", "Before"));
                }
                else if ((customAction || showDialog || specialAction) && !suppress && CompilerCore.IntegerNotSet == sequence && null == beforeAction && null == afterAction)
                {
                    this.core.OnMessage(WixErrors.NeedSequenceBeforeOrAfter(childSourceLineNumbers, child.Name));
                }

                // action that is scheduled to occur before/after itself
                if (beforeAction == actionName)
                {
                    this.core.OnMessage(WixErrors.ActionScheduledRelativeToItself(childSourceLineNumbers, child.Name, "Before", beforeAction));
                }
                else if (afterAction == actionName)
                {
                    this.core.OnMessage(WixErrors.ActionScheduledRelativeToItself(childSourceLineNumbers, child.Name, "After", afterAction));
                }

                // normal standard actions cannot be set overridable by the user (since they are overridable by default)
                if (overridable && Installer.IsStandardAction(actionName) && !specialAction)
                {
                    this.core.OnMessage(WixErrors.UnexpectedAttribute(childSourceLineNumbers, child.Name, "Overridable"));
                }

                // suppress cannot be specified at the same time as Before, After, or Sequence
                if (suppress && (null != afterAction || null != beforeAction || CompilerCore.IntegerNotSet != sequence || overridable))
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttributes(childSourceLineNumbers, child.Name, "Suppress", "Before", "After", "Sequence", "Overridable"));
                }

                foreach (XmlNode grandChild in child.ChildNodes)
                {
                    if (XmlNodeType.Element == grandChild.NodeType)
                    {
                        if (grandChild.NamespaceURI == this.schema.TargetNamespace)
                        {
                            this.core.UnexpectedElement(child, grandChild);
                        }
                        else
                        {
                            this.core.UnsupportedExtensionElement(child, grandChild);
                        }
                    }
                }

                // add the row and any references needed
                if (!this.core.EncounteredError)
                {
                    if (suppress)
                    {
                        Row row = this.core.CreateRow(childSourceLineNumbers, "WixSuppressAction");
                        row[0] = sequenceTable;
                        row[1] = actionName;
                    }
                    else
                    {
                        Row row = this.core.CreateRow(childSourceLineNumbers, "WixAction");
                        row[0] = sequenceTable;
                        row[1] = actionName;
                        row[2] = condition;
                        if (CompilerCore.IntegerNotSet != sequence)
                        {
                            row[3] = sequence;
                        }
                        row[4] = beforeAction;
                        row[5] = afterAction;
                        row[6] = overridable ? 1 : 0;
                    }
                }
            }
        }

        /// <summary>
        /// Parses a service control element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseServiceControlElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string arguments = null;
            int events = 0; // default is to do nothing
            string id = null;
            string name = null;
            YesNoType wait = YesNoType.NotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Remove":
                            string removeValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (this.core.GetAttributeValue(sourceLineNumbers, attrib))
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "install":
                                    events |= MsiInterop.MsidbServiceControlEventDelete;
                                    break;
                                case "uninstall":
                                    events |= MsiInterop.MsidbServiceControlEventUninstallDelete;
                                    break;
                                case "both":
                                    events |= MsiInterop.MsidbServiceControlEventDelete | MsiInterop.MsidbServiceControlEventUninstallDelete;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Remove", removeValue, "install", "uninstall", "both"));
                                    break;
                            }
                            break;
                        case "Start":
                            string startValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (startValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "install":
                                    events |= MsiInterop.MsidbServiceControlEventStart;
                                    break;
                                case "uninstall":
                                    events |= MsiInterop.MsidbServiceControlEventUninstallStart;
                                    break;
                                case "both":
                                    events |= MsiInterop.MsidbServiceControlEventStart | MsiInterop.MsidbServiceControlEventUninstallStart;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Start", startValue, "install", "uninstall", "both"));
                                    break;
                            }
                            break;
                        case "Stop":
                            string stopValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (stopValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "install":
                                    events |= MsiInterop.MsidbServiceControlEventStop;
                                    break;
                                case "uninstall":
                                    events |= MsiInterop.MsidbServiceControlEventUninstallStop;
                                    break;
                                case "both":
                                    events |= MsiInterop.MsidbServiceControlEventStop | MsiInterop.MsidbServiceControlEventUninstallStop;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Stop", stopValue, "install", "uninstall", "both"));
                                    break;
                            }
                            break;
                        case "Wait":
                            wait = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            // get the ServiceControl arguments
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ServiceArgument":
                                if (null != arguments)
                                {
                                    arguments = String.Concat(arguments, "[~]");
                                }
                                arguments = String.Concat(arguments, CompilerCore.GetTrimmedInnerText(child));
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ServiceControl");
                row[0] = id;
                row[1] = name;
                row[2] = events;
                row[3] = arguments;
                if (YesNoType.NotSet != wait)
                {
                    row[4] = YesNoType.Yes == wait ? 1 : 0;
                }
                row[5] = componentId;
            }
        }

        /// <summary>
        /// Parses a service dependency element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <returns>Parsed sevice dependency name.</returns>
        private string ParseServiceDependencyElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string dependency = null;
            bool group = false;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            dependency = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Group":
                            group = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == dependency)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return group ? String.Concat("+", dependency) : dependency;
        }

        /// <summary>
        /// Parses a service install element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseServiceInstallElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string account = null;
            string arguments = null;
            string dependencies = null;
            string description = null;
            string displayName = null;
            bool eraseDescription = false;
            int errorbits = 0;
            string loadOrderGroup = null;
            string name = null;
            string password = null;
            int startType = 0;
            int typebits = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Account":
                            account = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Arguments":
                            arguments = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DisplayName":
                            displayName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "EraseDescription":
                            eraseDescription = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "ErrorControl":
                            string errorControlValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (errorControlValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "ignore":
                                    errorbits |= MsiInterop.MsidbServiceInstallErrorIgnore;
                                    break;
                                case "normal":
                                    errorbits |= MsiInterop.MsidbServiceInstallErrorNormal;
                                    break;
                                case "critical":
                                    errorbits |= MsiInterop.MsidbServiceInstallErrorCritical;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, errorControlValue, "ignore", "normal", "critical"));
                                    break;
                            }
                            break;
                        case "Interactive":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                typebits |= MsiInterop.MsidbServiceInstallInteractive;
                            }
                            break;
                        case "LoadOrderGroup":
                            loadOrderGroup = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Password":
                            password = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Start":
                            string startValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (startValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "auto":
                                    startType = MsiInterop.MsidbServiceInstallAutoStart;
                                    break;
                                case "demand":
                                    startType = MsiInterop.MsidbServiceInstallDemandStart;
                                    break;
                                case "disabled":
                                    startType = MsiInterop.MsidbServiceInstallDisabled;
                                    break;
                                case "boot":
                                case "system":
                                    this.core.OnMessage(WixErrors.ValueNotSupported(sourceLineNumbers, node.Name, attrib.Name, startValue));
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, startValue, "auto", "demand", "disabled"));
                                    break;
                            }
                            break;
                        case "Type":
                            string typeValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (typeValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    break;
                                case "ownProcess":
                                    typebits |= MsiInterop.MsidbServiceInstallOwnProcess;
                                    break;
                                case "shareProcess":
                                    typebits |= MsiInterop.MsidbServiceInstallShareProcess;
                                    break;
                                case "kernelDriver":
                                case "systemDriver":
                                    this.core.OnMessage(WixErrors.ValueNotSupported(sourceLineNumbers, node.Name, attrib.Name, typeValue));
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, node.Name, typeValue, "ownProcess", "shareProcess"));
                                    break;
                            }
                            break;
                        case "Vital":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                errorbits |= MsiInterop.MsidbServiceInstallErrorControlVital;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (eraseDescription)
            {
                description = "[~]";
            }

            // get the ServiceInstall dependencies and config
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "ServiceDependency":
                                dependencies = String.Concat(dependencies, this.ParseServiceDependencyElement(child), "[~]");
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.ParseExtensionElement(sourceLineNumbers, (XmlElement)node, (XmlElement)child, id, name, componentId);
                    }
                }
            }

            if (null != dependencies)
            {
                dependencies = String.Concat(dependencies, "[~]");
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ServiceInstall");
                row[0] = id;
                row[1] = name;
                row[2] = displayName;
                row[3] = typebits;
                row[4] = startType;
                row[5] = errorbits;
                row[6] = loadOrderGroup;
                row[7] = dependencies;
                row[8] = account;
                row[9] = password;
                row[10] = arguments;
                row[11] = componentId;
                row[12] = description;
            }
        }

        /// <summary>
        /// Parses a SFP catalog element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentSFPCatalog">Parent SFPCatalog.</param>
        private void ParseSFPFileElement(XmlNode node, string parentSFPCatalog)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "FileSFPCatalog");
                row[0] = id;
                row[1] = parentSFPCatalog;
            }
        }

        /// <summary>
        /// Parses a SFP catalog element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="parentSFPCatalog">Parent SFPCatalog.</param>
        private void ParseSFPCatalogElement(XmlNode node, ref string parentSFPCatalog)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string parentName = null;
            string dependency = null;
            string name = null;
            string sourceFile = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Dependency":
                            dependency = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            parentSFPCatalog = name;
                            break;
                        case "SourceFile":
                            sourceFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (null == sourceFile)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "SourceFile"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "SFPCatalog":
                                this.ParseSFPCatalogElement(child, ref parentName);
                                if (null != dependency && parentName == dependency)
                                {
                                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Dependency"));
                                }
                                dependency = parentName;
                                break;
                            case "SFPFile":
                                this.ParseSFPFileElement(child, name);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null == dependency)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Dependency"));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "SFPCatalog");
                row[0] = name;
                row[1] = sourceFile;
                row[2] = dependency;
            }
        }

        /// <summary>
        /// Parses a shortcut element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifer for parent component.</param>
        /// <param name="parentElementLocalName">Local name of parent element.</param>
        /// <param name="defaultTarget">Default identifier of target.</param>
        private void ParseShortcutElement(XmlNode node, string componentId, string parentElementLocalName, string defaultTarget)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            bool advertise = false;
            string arguments = null;
            string description = null;
            string descriptionResourceDll = null;
            int descriptionResourceId = CompilerCore.IntegerNotSet;
            string directory = null;
            string displayResourceDll = null;
            int displayResourceId = CompilerCore.IntegerNotSet;
            int hotkey = CompilerCore.IntegerNotSet;
            string icon = null;
            int iconIndex = CompilerCore.IntegerNotSet;
            string longName = null;
            string name = null;
            string shortName = null;
            int show = CompilerCore.IntegerNotSet;
            string target = null;
            string workingDirectory = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Advertise":
                            advertise = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Arguments":
                            arguments = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DescriptionResourceDll":
                            descriptionResourceDll = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DescriptionResourceId":
                            descriptionResourceId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Directory":
                            directory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", directory);
                            break;
                        case "DisplayResourceDll":
                            displayResourceDll = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DisplayResourceId":
                            displayResourceId = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Hotkey":
                            hotkey = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Icon":
                            icon = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Icon", icon);
                            break;
                        case "IconIndex":
                            iconIndex = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, short.MinValue + 1, short.MaxValue);
                            break;
                        case "LongName":
                            longName = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            this.core.OnMessage(WixWarnings.DeprecatedLongNameAttribute(sourceLineNumbers, node.Name, attrib.Name, "Name", "ShortName"));
                            break;
                        case "Name":
                            name = this.core.GetAttributeLongFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "ShortName":
                            shortName = this.core.GetAttributeShortFilename(sourceLineNumbers, attrib, false);
                            break;
                        case "Show":
                            string showValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (showValue)
                            {
                                case CompilerCore.IllegalEmptyAttributeValue:
                                    show = CompilerCore.IllegalInteger;
                                    break;
                                case "normal":
                                    show = 1;
                                    break;
                                case "maximized":
                                    show = 3;
                                    break;
                                case "minimized":
                                    show = 7;
                                    break;
                                default:
                                    this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Show", showValue, "normal", "maximized", "minimized"));
                                    show = CompilerCore.IllegalInteger;
                                    break;
                            }
                            break;
                        case "Target":
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "WorkingDirectory":
                            workingDirectory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (advertise && null != target)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Target", "Advertise", "yes"));
            }

            if (null != descriptionResourceDll)
            {
                if (CompilerCore.IntegerNotSet == descriptionResourceId)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DescriptionResourceDll", "DescriptionResourceId"));
                }
            }
            else
            {
                if (CompilerCore.IntegerNotSet != descriptionResourceId)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DescriptionResourceId", "DescriptionResourceDll"));
                }
            }

            if (null != displayResourceDll)
            {
                if (CompilerCore.IntegerNotSet == displayResourceId)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DisplayResourceDll", "DisplayResourceId"));
                }
            }
            else
            {
                if (CompilerCore.IntegerNotSet != displayResourceId)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "DisplayResourceId", "DisplayResourceDll"));
                }
            }

            // the ShortName and LongName attributes should not both be specified because LongName is only for
            // the old deprecated method of specifying a file name whereas ShortName is specifically for the new method
            if (null != shortName && null != longName)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "ShortName", "LongName"));
            }

            if (null == name)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }
            else if (CompilerCore.IllegalEmptyAttributeValue != name)
            {
                if (CompilerCore.IsValidShortFilename(name, false))
                {
                    if (null == shortName)
                    {
                        shortName = name;
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "ShortName"));
                    }
                }
                else
                {
                    if (null == longName)
                    {
                        longName = name;

                        // generate a short file name
                        if (null == shortName)
                        {
                            shortName = CompilerCore.GenerateShortName(name, true, false, node.LocalName, componentId, directory);
                        }
                    }
                    else
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, "Name", name, "LongName"));
                    }
                }
            }

            if ("Component" != parentElementLocalName && null != target)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, "Target", parentElementLocalName));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Icon":
                                this.ParseIconElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Shortcut");
                row[0] = id;
                row[1] = directory;
                row[2] = GetMsiFilenameValue(shortName, longName);
                row[3] = componentId;
                if (advertise)
                {
                    row[4] = Guid.Empty.ToString("B");
                }
                else if (null != target)
                {
                    row[4] = target;
                }
                else if ("Component" == parentElementLocalName || "CreateFolder" == parentElementLocalName)
                {
                    row[4] = String.Format(CultureInfo.InvariantCulture, "[{0}]", defaultTarget);
                }
                else if ("File" == parentElementLocalName)
                {
                    row[4] = String.Format(CultureInfo.InvariantCulture, "[#{0}]", defaultTarget);
                }
                row[5] = arguments;
                row[6] = description;
                if (CompilerCore.IntegerNotSet != hotkey)
                {
                    row[7] = hotkey;
                }
                row[8] = icon;
                if (CompilerCore.IntegerNotSet != iconIndex)
                {
                    row[9] = iconIndex;
                }

                if (CompilerCore.IntegerNotSet != show)
                {
                    row[10] = show;
                }
                row[11] = workingDirectory;
                row[12] = displayResourceDll;
                if (CompilerCore.IntegerNotSet != displayResourceId)
                {
                    row[13] = displayResourceId;
                }
                row[14] = descriptionResourceDll;
                if (CompilerCore.IntegerNotSet != descriptionResourceId)
                {
                    row[15] = descriptionResourceId;
                }
            }
        }

        /// <summary>
        /// Parses a typelib element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="fileServer">Identifier of file that acts as typelib server.</param>
        /// <param name="win64Component">true if the component is 64-bit.</param>
        private void ParseTypeLibElement(XmlNode node, string componentId, string fileServer, bool win64Component)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            YesNoType advertise = YesNoType.NotSet;
            int cost = CompilerCore.IntegerNotSet;
            string description = null;
            int flags = 0;
            string helpDirectory = null;
            int language = CompilerCore.IntegerNotSet;
            int majorVersion = CompilerCore.IntegerNotSet;
            int minorVersion = CompilerCore.IntegerNotSet;
            long resourceId = CompilerCore.LongNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "Advertise":
                            advertise = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Control":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                flags |= 2;
                            }
                            break;
                        case "Cost":
                            cost = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "Description":
                            description = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "HasDiskImage":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                flags |= 8;
                            }
                            break;
                        case "HelpDirectory":
                            helpDirectory = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Directory", helpDirectory);
                            break;
                        case "Hidden":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                flags |= 4;
                            }
                            break;
                        case "Language":
                            language = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "MajorVersion":
                            majorVersion = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, ushort.MaxValue);
                            break;
                        case "MinorVersion":
                            minorVersion = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, byte.MaxValue);
                            break;
                        case "ResourceId":
                            resourceId = this.core.GetAttributeLongValue(sourceLineNumbers, attrib, int.MinValue, int.MaxValue);
                            break;
                        case "Restricted":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                flags |= 1;
                            }
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (CompilerCore.IntegerNotSet == language)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Language"));
                language = CompilerCore.IllegalInteger;
            }

            // build up the typelib version string for the registry if the major or minor version was specified
            string registryVersion = null;
            if (CompilerCore.IntegerNotSet != majorVersion || CompilerCore.IntegerNotSet != minorVersion)
            {
                if (CompilerCore.IntegerNotSet != majorVersion)
                {
                    registryVersion = majorVersion.ToString("x", CultureInfo.InvariantCulture.NumberFormat);
                }
                else
                {
                    registryVersion = "0";
                }

                if (CompilerCore.IntegerNotSet != minorVersion)
                {
                    registryVersion = String.Concat(registryVersion, ".", minorVersion.ToString("x", CultureInfo.InvariantCulture.NumberFormat));
                }
                else
                {
                    registryVersion = String.Concat(registryVersion, ".0");
                }
            }

            // if the advertise state has not been set, default to non-advertised
            if (YesNoType.NotSet == advertise)
            {
                advertise = YesNoType.No;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "AppId":
                                this.ParseAppIdElement(child, componentId, YesNoType.NotSet, fileServer, id, registryVersion);
                                break;
                            case "Class":
                                this.ParseClassElement(child, componentId, YesNoType.NotSet, fileServer, id, registryVersion, null);
                                break;
                            case "Interface":
                                this.ParseInterfaceElement(child, componentId, null, null, id, registryVersion);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (YesNoType.Yes == advertise)
            {
                if (CompilerCore.LongNotSet != resourceId)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWhenAdvertised(sourceLineNumbers, node.Name, "ResourceId"));
                }

                if (0 != flags)
                {
                    if (0x1 == (flags & 0x1))
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Restricted", "Advertise", "yes"));
                    }

                    if (0x2 == (flags & 0x2))
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Control", "Advertise", "yes"));
                    }

                    if (0x4 == (flags & 0x4))
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Hidden", "Advertise", "yes"));
                    }

                    if (0x8 == (flags & 0x8))
                    {
                        this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "HasDiskImage", "Advertise", "yes"));
                    }
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "TypeLib");
                    row[0] = id;
                    row[1] = language;
                    row[2] = componentId;
                    if (CompilerCore.IntegerNotSet != majorVersion || CompilerCore.IntegerNotSet != minorVersion)
                    {
                        row[3] = (CompilerCore.IntegerNotSet != majorVersion ? majorVersion * 256 : 0) + (CompilerCore.IntegerNotSet != minorVersion ? minorVersion : 0);
                    }
                    row[4] = description;
                    row[5] = helpDirectory;
                    row[6] = Guid.Empty.ToString("B");
                    if (CompilerCore.IntegerNotSet != cost)
                    {
                        row[7] = cost;
                    }
                }
            }
            else if (YesNoType.No == advertise)
            {
                if (CompilerCore.IntegerNotSet != cost && CompilerCore.IllegalInteger != cost)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Cost", "Advertise", "no"));
                }

                if (null == fileServer)
                {
                    this.core.OnMessage(WixErrors.MissingTypeLibFile(sourceLineNumbers, node.Name, "File"));
                }

                if (null == registryVersion)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttributesWithOtherAttribute(sourceLineNumbers, node.Name, "MajorVersion", "MinorVersion", "Advertise", "no"));
                }

                // HKCR\TypeLib\[ID]\[MajorVersion].[MinorVersion], (Default) = [Description]
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Format(CultureInfo.InvariantCulture, @"TypeLib\{0}\{1}", id, registryVersion), null, description, componentId);

                // HKCR\TypeLib\[ID]\[MajorVersion].[MinorVersion]\[Language]\[win16|win32|win64], (Default) = [TypeLibPath]\[ResourceId]
                string path = String.Concat("[#", fileServer, "]");
                if (CompilerCore.LongNotSet != resourceId)
                {
                    path = String.Concat(path, Path.DirectorySeparatorChar, resourceId.ToString(CultureInfo.InvariantCulture.NumberFormat));
                }
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Format(CultureInfo.InvariantCulture, @"TypeLib\{0}\{1}\{2}\{3}", id, registryVersion, language, (win64Component ? "win64" : "win32")), null, path, componentId);

                // HKCR\TypeLib\[ID]\[MajorVersion].[MinorVersion]\FLAGS, (Default) = [TypeLibFlags]
                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Format(CultureInfo.InvariantCulture, @"TypeLib\{0}\{1}\FLAGS", id, registryVersion), null, flags.ToString(CultureInfo.InvariantCulture.NumberFormat), componentId);

                if (null != helpDirectory)
                {
                    // HKCR\TypeLib\[ID]\[MajorVersion].[MinorVersion]\HELPDIR, (Default) = [HelpDirectory]
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Format(CultureInfo.InvariantCulture, @"TypeLib\{0}\{1}\HELPDIR", id, registryVersion), null, String.Concat("[", helpDirectory, "]"), componentId);
                }
            }
        }

        /// <summary>
        /// Parses UI elements.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseUIElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "BillboardAction":
                                this.ParseBillboardActionElement(child);
                                break;
                            case "ComboBox":
                                this.ParseControlGroupElement(child, this.tableDefinitions["ComboBox"], "ListItem");
                                break;
                            case "Dialog":
                                this.ParseDialogElement(child);
                                break;
                            case "DialogRef":
                                this.ParseSimpleRefElement(child, "Dialog");
                                break;
                            case "Error":
                                this.ParseErrorElement(child);
                                break;
                            case "ListBox":
                                this.ParseControlGroupElement(child, this.tableDefinitions["ListBox"], "ListItem");
                                break;
                            case "ListView":
                                this.ParseControlGroupElement(child, this.tableDefinitions["ListView"], "ListItem");
                                break;
                            case "ProgressText":
                                this.ParseActionTextElement(child);
                                break;
                            case "Publish":
                                int order = 0;
                                this.ParsePublishElement(child, null, null, ref order);
                                break;
                            case "RadioButtonGroup":
                                RadioButtonType radioButtonType = this.ParseRadioButtonGroupElement(child, null, RadioButtonType.NotSet);
                                if (RadioButtonType.Bitmap == radioButtonType || RadioButtonType.Icon == radioButtonType)
                                {
                                    SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                                    this.core.OnMessage(WixErrors.RadioButtonBitmapAndIconDisallowed(childSourceLineNumbers));
                                }
                                break;
                            case "TextStyle":
                                this.ParseTextStyleElement(child);
                                break;
                            case "UIText":
                                this.ParseUITextElement(child);
                                break;

                            // the following are available indentically under the UI and Product elements for document organization use only
                            case "AdminUISequence":
                            case "InstallUISequence":
                                this.ParseSequenceElement(child, child.LocalName);
                                break;
                            case "Binary":
                                this.ParseBinaryElement(child);
                                break;
                            case "Property":
                                this.ParsePropertyElement(child);
                                break;
                            case "UIRef":
                                this.ParseSimpleRefElement(child, "WixUI");
                                break;

                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null != id && !this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "WixUI");
                row[0] = id;
            }
        }

        /// <summary>
        /// Parses a list item element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="table">Table to add row to.</param>
        /// <param name="property">Identifier of property referred to by list item.</param>
        /// <param name="order">Relative order of list items.</param>
        private void ParseListItemElement(XmlNode node, TableDefinition table, string property, ref int order)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string icon = null;
            string text = null;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Icon":
                            if ("ListView" == table.Name)
                            {
                                icon = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                                this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Binary", icon);
                            }
                            else
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeExceptOnElement(sourceLineNumbers, node.Name, attrib.Name, "ListView"));
                            }
                            break;
                        case "Text":
                            text = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, table.Name);
                row[0] = property;
                row[1] = ++order;
                row[2] = value;
                row[3] = text;
                if (null != icon)
                {
                    row[4] = icon;
                }
            }
        }

        /// <summary>
        /// Parses a radio button element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="property">Identifier of property referred to by radio button.</param>
        /// <param name="order">Relative order of radio buttons.</param>
        /// <returns>Type of this radio button.</returns>
        private RadioButtonType ParseRadioButtonElement(XmlNode node, string property, ref int order)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            RadioButtonType type = RadioButtonType.NotSet;
            string value = null;
            string x = null;
            string y = null;
            string width = null;
            string height = null;
            string text = null;
            string tooltip = null;
            string help = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Bitmap":
                            if (RadioButtonType.NotSet != type)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttributes(sourceLineNumbers, node.Name, attrib.Name, "Icon", "Text"));
                            }
                            text = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Binary", text);
                            type = RadioButtonType.Bitmap;
                            break;
                        case "Height":
                            height = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Help":
                            help = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Icon":
                            if (RadioButtonType.NotSet != type)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttributes(sourceLineNumbers, node.Name, attrib.Name, "Bitmap", "Text"));
                            }
                            text = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Binary", text);
                            type = RadioButtonType.Icon;
                            break;
                        case "Text":
                            if (RadioButtonType.NotSet != type)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, attrib.Name, "Bitmap", "Icon"));
                            }
                            text = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            type = RadioButtonType.Text;
                            break;
                        case "ToolTip":
                            tooltip = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Width":
                            width = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "X":
                            x = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Y":
                            y = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }

            if (null == x)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "X"));
            }

            if (null == y)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Y"));
            }

            if (null == width)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Width"));
            }

            if (null == height)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Height"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "RadioButton");
                row[0] = property;
                row[1] = ++order;
                row[2] = value;
                row[3] = x;
                row[4] = y;
                row[5] = width;
                row[6] = height;
                row[7] = text;
                if (null != tooltip || null != help)
                {
                    row[8] = String.Concat(tooltip, "|", help);
                }
            }

            return type;
        }

        /// <summary>
        /// Parses a billboard element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseBillboardActionElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string action = null;
            int order = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            action = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixAction", "InstallExecuteSequence", action);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == action)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Billboard":
                                order = order + 1;
                                this.ParseBillboardElement(child, action, order);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a billboard element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="action">Action for the billboard.</param>
        /// <param name="order">Order of the billboard.</param>
        private void ParseBillboardElement(XmlNode node, string action, int order)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string feature = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Feature":
                            feature = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Feature", feature);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Control":
                                // These are all thrown away.
                                Row lastTabRow = null;
                                string firstControl = null;
                                string defaultControl = null;
                                string cancelControl = null;

                                this.ParseControlElement(child, id, this.tableDefinitions["BBControl"], ref lastTabRow, ref firstControl, ref defaultControl, ref cancelControl, false);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Billboard");
                row[0] = id;
                row[1] = feature;
                row[2] = action;
                row[3] = order;
            }
        }

        /// <summary>
        /// Parses a control group element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="table">Table referred to by control group.</param>
        /// <param name="childTag">Expected child elements.</param>
        private void ParseControlGroupElement(XmlNode node, TableDefinition table, string childTag)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int order = 0;
            string property = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Property":
                            property = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == property)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Property"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        if (childTag != child.LocalName)
                        {
                            this.core.UnexpectedElement(node, child);
                        }

                        switch (child.LocalName)
                        {
                            case "ListItem":
                                this.ParseListItemElement(child, table, property, ref order);
                                break;
                            case "Property":
                                this.ParsePropertyElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a radio button control group element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="property">Property associated with this radio button group.</param>
        /// <param name="groupType">Specifies the current type of radio buttons in the group.</param>
        /// <returns>The current type of radio buttons in the group.</returns>
        private RadioButtonType ParseRadioButtonGroupElement(XmlNode node, string property, RadioButtonType groupType)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            int order = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Property":
                            property = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Property", property);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == property)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Property"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "RadioButton":
                                RadioButtonType type = this.ParseRadioButtonElement(child, property, ref order);
                                if (RadioButtonType.NotSet == groupType)
                                {
                                    groupType = type;
                                }
                                else if (groupType != type)
                                {
                                    SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                                    this.core.OnMessage(WixErrors.RadioButtonTypeInconsistent(childSourceLineNumbers));
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            return groupType;
        }

        /// <summary>
        /// Parses an action text element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseActionTextElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string action = null;
            string template = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Action":
                            action = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Template":
                            template = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == action)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Action"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ActionText");
                row[0] = action;
                row[1] = node.InnerText;
                row[2] = template;
            }
        }

        /// <summary>
        /// Parses an ui text element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseUITextElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "UIText");
                row[0] = id;
                row[1] = node.InnerText;
            }
        }

        /// <summary>
        /// Parses a text style element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseTextStyleElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int bits = 0;
            int color = CompilerCore.IntegerNotSet;
            string faceName = null;
            int size = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;

                        // RGB Values
                        case "Red":
                            int redColor = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, byte.MaxValue);
                            if (CompilerCore.IllegalInteger != redColor)
                            {
                                if (CompilerCore.IntegerNotSet == color)
                                {
                                    color = redColor;
                                }
                                else
                                {
                                    color += redColor;
                                }
                            }
                            break;
                        case "Green":
                            int greenColor = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, byte.MaxValue);
                            if (CompilerCore.IllegalInteger != greenColor)
                            {
                                if (CompilerCore.IntegerNotSet == color)
                                {
                                    color = greenColor * 256;
                                }
                                else
                                {
                                    color += greenColor * 256;
                                }
                            }
                            break;
                        case "Blue":
                            int blueColor = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, byte.MaxValue);
                            if (CompilerCore.IllegalInteger != blueColor)
                            {
                                if (CompilerCore.IntegerNotSet == color)
                                {
                                    color = blueColor * 65536;
                                }
                                else
                                {
                                    color += blueColor * 65536;
                                }
                            }
                            break;

                        // Style values
                        case "Bold":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbTextStyleStyleBitsBold;
                            }
                            break;
                        case "Italic":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbTextStyleStyleBitsItalic;
                            }
                            break;
                        case "Strike":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbTextStyleStyleBitsStrike;
                            }
                            break;
                        case "Underline":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits |= MsiInterop.MsidbTextStyleStyleBitsUnderline;
                            }
                            break;

                        // Font values
                        case "FaceName":
                            faceName = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Size":
                            size = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;

                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == faceName)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "FaceName"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "TextStyle");
                row[0] = id;
                row[1] = faceName;
                row[2] = size;
                if (0 <= color)
                {
                    row[3] = color;
                }

                if (0 < bits)
                {
                    row[4] = bits;
                }
            }
        }

        /// <summary>
        /// Parses a dialog element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseDialogElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int bits = MsiInterop.MsidbDialogAttributesVisible | MsiInterop.MsidbDialogAttributesModal | MsiInterop.MsidbDialogAttributesMinimize;
            int height = 0;
            string title = null;
            bool trackDiskSpace = false;
            int width = 0;
            int x = 50;
            int y = 50;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Height":
                            height = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Title":
                            title = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Width":
                            width = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "X":
                            x = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, 100);
                            break;
                        case "Y":
                            y = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, 100);
                            break;

                        case "CustomPalette":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesUseCustomPalette;
                            }
                            break;
                        case "ErrorDialog":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesError;
                            }
                            break;
                        case "Hidden":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesVisible;
                            }
                            break;
                        case "KeepModeless":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesKeepModeless;
                            }
                            break;
                        case "LeftScroll":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesLeftScroll;
                            }
                            break;
                        case "Modeless":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesModal;
                            }
                            break;
                        case "NoMinimize":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesMinimize;
                            }
                            break;
                        case "RightAligned":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesRightAligned;
                            }
                            break;
                        case "RightToLeft":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesRTLRO;
                            }
                            break;
                        case "SystemModal":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesSysModal;
                            }
                            break;
                        case "TrackDiskSpace":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                bits ^= MsiInterop.MsidbDialogAttributesTrackDiskSpace;
                                trackDiskSpace = true;
                            }
                            break;

                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            Row lastTabRow = null;
            string cancelControl = null;
            string defaultControl = null;
            string firstControl = null;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Control":
                                this.ParseControlElement(child, id, this.tableDefinitions["Control"], ref lastTabRow, ref firstControl, ref defaultControl, ref cancelControl, trackDiskSpace);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null != lastTabRow && null != lastTabRow[1])
            {
                if (firstControl != lastTabRow[1].ToString())
                {
                    lastTabRow[10] = firstControl;
                }
            }

            if (null == firstControl)
            {
                this.core.OnMessage(WixErrors.NoFirstControlSpecified(sourceLineNumbers, id));
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Dialog");
                row[0] = id;
                row[1] = x;
                row[2] = y;
                row[3] = width;
                row[4] = height;
                row[5] = bits;
                row[6] = title;
                row[7] = firstControl;
                row[8] = defaultControl;
                row[9] = cancelControl;
            }
        }

        /// <summary>
        /// Parses a control element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="dialog">Identifier for parent dialog.</param>
        /// <param name="table">Table control belongs in.</param>
        /// <param name="lastTabRow">Last row in the tab order.</param>
        /// <param name="firstControl">Name of the first control in the tab order.</param>
        /// <param name="defaultControl">Name of the default control.</param>
        /// <param name="cancelControl">Name of the candle control.</param>
        /// <param name="trackDiskSpace">True if the containing dialog tracks disk space.</param>
        private void ParseControlElement(XmlNode node, string dialog, TableDefinition table, ref Row lastTabRow, ref string firstControl, ref string defaultControl, ref string cancelControl, bool trackDiskSpace)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            BitArray bits = new BitArray(32);
            int attributes = 0;
            string checkboxValue = null;
            string controlType = null;
            bool disabled = false;
            string height = null;
            string help = null;
            bool isCancel = false;
            bool isDefault = false;
            bool notTabbable = false;
            string property = null;
            int publishOrder = 0;
            string[] specialAttributes = null;
            string sourceFile = null;
            string text = null;
            string tooltip = null;
            RadioButtonType radioButtonsType = RadioButtonType.NotSet;
            string width = null;
            string x = null;
            string y = null;

            // The rest of the method relies on the control's Type, so we have to get that first.
            XmlAttribute typeAttribute = node.Attributes["Type"];
            if (null == typeAttribute)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Type"));
            }
            else
            {
                controlType = this.core.GetAttributeValue(sourceLineNumbers, typeAttribute);
            }

            switch (controlType)
            {
                case "Billboard":
                    specialAttributes = null;
                    notTabbable = true;
                    disabled = true;

                    this.core.EnsureTable(sourceLineNumbers, "Billboard");
                    break;
                case "Bitmap":
                    specialAttributes = MsiInterop.BitmapControlAttributes;
                    notTabbable = true;
                    disabled = true;
                    break;
                case "CheckBox":
                    specialAttributes = MsiInterop.CheckboxControlAttributes;
                    break;
                case "ComboBox":
                    specialAttributes = MsiInterop.ComboboxControlAttributes;
                    break;
                case "DirectoryCombo":
                    specialAttributes = MsiInterop.VolumeControlAttributes;
                    break;
                case "DirectoryList":
                    specialAttributes = null;
                    break;
                case "Edit":
                    specialAttributes = MsiInterop.EditControlAttributes;
                    break;
                case "GroupBox":
                    specialAttributes = null;
                    notTabbable = true;
                    break;
                case "Icon":
                    specialAttributes = MsiInterop.IconControlAttributes;
                    notTabbable = true;
                    disabled = true;
                    break;
                case "Line":
                    specialAttributes = null;
                    notTabbable = true;
                    disabled = true;
                    break;
                case "ListBox":
                    specialAttributes = MsiInterop.ListboxControlAttributes;
                    break;
                case "ListView":
                    specialAttributes = MsiInterop.ListviewControlAttributes;
                    break;
                case "MaskedEdit":
                    specialAttributes = MsiInterop.EditControlAttributes;
                    break;
                case "PathEdit":
                    specialAttributes = MsiInterop.EditControlAttributes;
                    break;
                case "ProgressBar":
                    specialAttributes = MsiInterop.ProgressControlAttributes;
                    notTabbable = true;
                    disabled = true;
                    break;
                case "PushButton":
                    specialAttributes = MsiInterop.ButtonControlAttributes;
                    break;
                case "RadioButtonGroup":
                    specialAttributes = MsiInterop.RadioControlAttributes;
                    break;
                case "ScrollableText":
                    specialAttributes = null;
                    break;
                case "SelectionTree":
                    specialAttributes = null;
                    break;
                case "Text":
                    specialAttributes = MsiInterop.TextControlAttributes;
                    notTabbable = true;
                    break;
                case "VolumeCostList":
                    specialAttributes = MsiInterop.VolumeControlAttributes;
                    notTabbable = true;
                    break;
                case "VolumeSelectCombo":
                    specialAttributes = MsiInterop.VolumeControlAttributes;
                    break;
                default:
                    specialAttributes = null;
                    notTabbable = true;
                    break;
            }

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Type": // already processed
                            break;
                        case "Cancel":
                            isCancel = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "CheckBoxValue":
                            checkboxValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Default":
                            isDefault = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Height":
                            height = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Help":
                            help = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "IconSize":
                            string iconSizeValue = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            if (null != specialAttributes)
                            {
                                switch (iconSizeValue)
                                {
                                    case CompilerCore.IllegalEmptyAttributeValue:
                                        break;
                                    case "16":
                                        CompilerCore.NameToBit(specialAttributes, "Icon16", YesNoType.Yes, bits, 16);
                                        break;
                                    case "32":
                                        CompilerCore.NameToBit(specialAttributes, "Icon32", YesNoType.Yes, bits, 16);
                                        break;
                                    case "48":
                                        CompilerCore.NameToBit(specialAttributes, "Icon16", YesNoType.Yes, bits, 16);
                                        CompilerCore.NameToBit(specialAttributes, "Icon32", YesNoType.Yes, bits, 16);
                                        break;
                                    default:
                                        this.core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, iconSizeValue, "16", "32", "48"));
                                        break;
                                }
                            }
                            else
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeValueWithOtherAttribute(sourceLineNumbers, node.Name, attrib.Name, iconSizeValue, "Type"));
                            }
                            break;
                        case "Property":
                            property = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "TabSkip":
                            notTabbable = YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Text":
                            text = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ToolTip":
                            tooltip = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Width":
                            width = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "X":
                            x = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        case "Y":
                            y = this.core.GetAttributeLocalizableIntegerValue(sourceLineNumbers, attrib, 0, short.MaxValue);
                            break;
                        default:
                            YesNoType attribValue = this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if (!CompilerCore.NameToBit(MsiInterop.CommonControlAttributes, attrib.Name, attribValue, bits, 0))
                            {
                                if (null == specialAttributes || !CompilerCore.NameToBit(specialAttributes, attrib.Name, attribValue, bits, 16))
                                {
                                    this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                                }
                            }
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            attributes = CompilerCore.ConvertBitArrayToInt32(bits);

            if (disabled)
            {
                attributes |= MsiInterop.MsidbControlAttributesEnabled; // bit will be inverted when stored
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == height)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Height"));
            }

            if (null == width)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Width"));
            }

            if (null == x)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "X"));
            }

            if (null == y)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Y"));
            }

            if (isCancel)
            {
                cancelControl = id;
            }

            if (isDefault)
            {
                defaultControl = id;
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);
                        switch (child.LocalName)
                        {
                            case "Binary":
                                this.ParseBinaryElement(child);
                                break;
                            case "ComboBox":
                                this.ParseControlGroupElement(child, this.tableDefinitions["ComboBox"], "ListItem");
                                break;
                            case "Condition":
                                this.ParseConditionElement(child, node.LocalName, id, dialog);
                                break;
                            case "ListBox":
                                this.ParseControlGroupElement(child, this.tableDefinitions["ListBox"], "ListItem");
                                break;
                            case "ListView":
                                this.ParseControlGroupElement(child, this.tableDefinitions["ListView"], "ListItem");
                                break;
                            case "Property":
                                this.ParsePropertyElement(child);
                                break;
                            case "Publish":
                                // ensure that the dialog and control identifiers are not null
                                if (null == dialog)
                                {
                                    dialog = String.Empty;
                                }

                                if (null == id)
                                {
                                    id = String.Empty;
                                }

                                this.ParsePublishElement(child, dialog, id, ref publishOrder);
                                break;
                            case "RadioButtonGroup":
                                radioButtonsType = this.ParseRadioButtonGroupElement(child, property, radioButtonsType);
                                break;
                            case "Subscribe":
                                this.ParseSubscribeElement(child, dialog, id);
                                break;
                            case "Text":
                                foreach (XmlAttribute attrib in child.Attributes)
                                {
                                    switch (attrib.LocalName)
                                    {
                                        case "SourceFile":
                                        case "src":
                                            if (null != sourceFile)
                                            {
                                                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(childSourceLineNumbers, node.Name, "src", "SourceFile"));
                                            }

                                            if ("src" == attrib.LocalName)
                                            {
                                                this.core.OnMessage(WixWarnings.DeprecatedAttribute(childSourceLineNumbers, node.Name, attrib.Name, "SourceFile"));
                                            }
                                            sourceFile = this.core.GetAttributeValue(childSourceLineNumbers, attrib);
                                            break;
                                        default:
                                            this.core.UnexpectedAttribute(childSourceLineNumbers, attrib);
                                            break;
                                    }
                                }

                                if (0 < child.InnerText.Length)
                                {
                                    if (null != sourceFile)
                                    {
                                        this.core.OnMessage(WixErrors.IllegalAttributeWithInnerText(childSourceLineNumbers, child.Name, "SourceFile"));
                                    }
                                    else
                                    {
                                        text = child.InnerText;
                                    }
                                }
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // If the radio buttons have icons, then we need to add the icon attribute.
            switch (radioButtonsType)
            {
                case RadioButtonType.Bitmap:
                    attributes |= MsiInterop.MsidbControlAttributesBitmap;
                    break;
                case RadioButtonType.Icon:
                    attributes |= MsiInterop.MsidbControlAttributesIcon;
                    break;
                case RadioButtonType.Text:
                    // Text is the default so nothing needs to be added bits
                    break;
            }

            // If we're tracking disk space, and this is a non-FormatSize Text control, and the text attribute starts with 
            // '[' and ends with ']', add a space. It is not necessary for the whole string to be a property, just 
            // those two characters matter.
            if (trackDiskSpace && "Text" == controlType &&
                MsiInterop.MsidbControlAttributesFormatSize != (attributes & MsiInterop.MsidbControlAttributesFormatSize) &&
                null != text && text.StartsWith("[") && text.EndsWith("]"))
            {
                text = String.Concat(text, " ");
            }

            // the logic for creating control rows is a little tricky because of the way tabable controls are set
            Row row = null;
            if (!this.core.EncounteredError)
            {
                if ("CheckBox" == controlType && null != property)
                {
                    row = this.core.CreateRow(sourceLineNumbers, "CheckBox");
                    row[0] = property;
                    row[1] = checkboxValue;
                }

                row = this.core.CreateRow(sourceLineNumbers, table.Name);
                row[0] = dialog;
                row[1] = id;
                row[2] = controlType;
                row[3] = x;
                row[4] = y;
                row[5] = width;
                row[6] = height;
                row[7] = attributes ^ (MsiInterop.MsidbControlAttributesVisible | MsiInterop.MsidbControlAttributesEnabled);
                if ("BBControl" == table.Name)
                {
                    row[8] = text; // BBControl.Text

                    if (null != sourceFile)
                    {
                        Row wixBBControlRow = this.core.CreateRow(sourceLineNumbers, "WixBBControl");
                        wixBBControlRow[0] = dialog;
                        wixBBControlRow[1] = id;
                        wixBBControlRow[2] = sourceFile;
                    }
                }
                else
                {
                    row[8] = property;
                    row[9] = text;
                    if (null != tooltip || null != help)
                    {
                        row[11] = String.Concat(tooltip, "|", help); // Separator is required, even if only one is non-null.
                    }

                    if (null != sourceFile)
                    {
                        Row wixControlRow = this.core.CreateRow(sourceLineNumbers, "WixControl");
                        wixControlRow[0] = dialog;
                        wixControlRow[1] = id;
                        wixControlRow[2] = sourceFile;
                    }
                }
            }

            if (!notTabbable)
            {
                if ("BBControl" == table.Name)
                {
                    this.core.OnMessage(WixErrors.TabbableControlNotAllowedInBillboard(sourceLineNumbers, node.Name, controlType));
                }

                if (null == firstControl)
                {
                    firstControl = id;
                }

                if (null != lastTabRow)
                {
                    lastTabRow[10] = id;
                }
                lastTabRow = row;
            }

            // bitmap and icon controls contain a foreign key into the binary table in the text column;
            // add a reference if the identifier of the binary entry is known during compilation
            if (("Bitmap" == controlType || "Icon" == controlType) && CompilerCore.IsIdentifier(text))
            {
                this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Binary", text);
            }
        }

        /// <summary>
        /// Parses a publish control event element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="dialog">Identifier of parent dialog.</param>
        /// <param name="control">Identifier of parent control.</param>
        /// <param name="order">Relative order of controls.</param>
        private void ParsePublishElement(XmlNode node, string dialog, string control, ref int order)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string argument = null;
            string condition = null;
            string controlEvent = null;
            string property = null;

            // give this control event a unique ordering
            order++;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Control":
                            if (null != control)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            control = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Dialog":
                            if (null != dialog)
                            {
                                this.core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            dialog = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Dialog", dialog);
                            break;
                        case "Event":
                            controlEvent = Compiler.UppercaseFirstChar(this.core.GetAttributeValue(sourceLineNumbers, attrib));
                            break;
                        case "Order":
                            order = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, 2147483647);
                            break;
                        case "Property":
                            property = String.Concat("[", this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib), "]");
                            break;
                        case "Value":
                            argument = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }
            condition = CompilerCore.GetConditionInnerText(node);

            if (null == control)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Control"));
            }

            if (null == dialog)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Dialog"));
            }

            if (null == controlEvent && null == property) // need to specify at least one
            {
                this.core.OnMessage(WixErrors.ExpectedAttributes(sourceLineNumbers, node.Name, "Event", "Property"));
            }
            else if (null != controlEvent && null != property) // cannot specify both
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Event", "Property"));
            }

            if (null == argument)
            {
                if (null != controlEvent)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value", "Event"));
                }
                else if (null != property)
                {
                    // if this is setting a property to null, put a special value in the argument column
                    argument = "{}";
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "ControlEvent");
                row[0] = dialog;
                row[1] = control;
                row[2] = (null != controlEvent ? controlEvent : property);
                row[3] = argument;
                row[4] = condition;
                row[5] = order;
            }

            if ("DoAction" == controlEvent && null != argument)
            {
                // if we're not looking at a standard action then create a reference 
                // to the custom action.
                if (!Installer.IsStandardAction(argument))
                {
                    this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", argument);
                }
            }

            // if we're referring to a dialog but not through a property, add it to the references
            if (("NewDialog" == controlEvent || "SpawnDialog" == controlEvent || "SpawnWaitDialog" == controlEvent || "SelectionBrowse" == controlEvent) && CompilerCore.IsIdentifier(argument))
            {
                this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Dialog", argument);
            }
        }

        /// <summary>
        /// Parses a control subscription element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="dialog">Identifier of dialog.</param>
        /// <param name="control">Identifier of control.</param>
        private void ParseSubscribeElement(XmlNode node, string dialog, string control)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string controlAttribute = null;
            string eventMapping = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Attribute":
                            controlAttribute = Compiler.UppercaseFirstChar(this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib));
                            break;
                        case "Event":
                            eventMapping = Compiler.UppercaseFirstChar(this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib));
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "EventMapping");
                row[0] = dialog;
                row[1] = control;
                row[2] = eventMapping;
                row[3] = controlAttribute;
            }
        }

        /// <summary>
        /// Parses an upgrade element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseUpgradeElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // process the UpgradeVersion children here
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);

                        switch (child.LocalName)
                        {
                            case "Property":
                                this.ParsePropertyElement(child);
                                this.core.OnMessage(WixWarnings.DeprecatedUpgradeProperty(childSourceLineNumbers));
                                break;
                            case "UpgradeVersion":
                                this.ParseUpgradeVersionElement(child, id);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // No rows created here. All row creation is done in ParseUpgradeVersionElement.
        }

        /// <summary>
        /// Parse upgrade version element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="upgradeId">Upgrade code.</param>
        private void ParseUpgradeVersionElement(XmlNode node, string upgradeId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);

            string actionProperty = null;
            string language = null;
            string maximum = null;
            string minimum = null;
            int options = 256;
            string removeFeatures = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "ExcludeLanguages":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                options |= 1024;
                            }
                            break;
                        case "IgnoreRemoveFailure":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                options |= 4;
                            }
                            break;
                        case "IncludeMaximum":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                options |= 512;
                            }
                            break;
                        case "IncludeMinimum": // this is "yes" by default
                            if (YesNoType.No == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                options &= ~256;
                            }
                            break;
                        case "Language":
                            language = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Minimum":
                            minimum = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Maximum":
                            maximum = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MigrateFeatures":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                options |= 1;
                            }
                            break;
                        case "OnlyDetect":
                            if (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                options |= 2;
                            }
                            break;
                        case "Property":
                            actionProperty = this.core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "RemoveFeatures":
                            removeFeatures = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.ParseExtensionAttribute(sourceLineNumbers, (XmlElement)node, attrib);
                }
            }

            if (null == actionProperty)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Property"));
            }
            else if (actionProperty.ToUpper(CultureInfo.InvariantCulture) != actionProperty)
            {
                this.core.OnMessage(WixErrors.SecurePropertyNotUppercase(sourceLineNumbers, node.Name, "Property", actionProperty));
            }

            if (null == minimum && null == maximum)
            {
                this.core.OnMessage(WixErrors.ExpectedAttributes(sourceLineNumbers, node.Name, "Minimum", "Maximum"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                Row row = this.core.CreateRow(sourceLineNumbers, "Upgrade");
                row[0] = upgradeId;
                row[1] = minimum;
                row[2] = maximum;
                row[3] = language;
                row[4] = options;
                row[5] = removeFeatures;
                row[6] = actionProperty;

                if (1 == row.Table.Rows.Count)
                {
                    // Ensure that RemoveExistingProducts is authored in InstallExecuteSequence
                    // if at least one row in Upgrade table
                    this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixAction", "InstallExecuteSequence", "RemoveExistingProducts");
                }
            }
        }

        /// <summary>
        /// Parses a verb element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="extension">Extension verb is releated to.</param>
        /// <param name="progId">Optional progId for extension.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        /// <param name="advertise">Flag if verb is advertised.</param>
        private void ParseVerbElement(XmlNode node, string extension, string progId, string componentId, YesNoType advertise)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string argument = null;
            string command = null;
            int sequence = CompilerCore.IntegerNotSet;
            string target = null;
            string targetFile = null;
            string targetProperty = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Argument":
                            argument = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Command":
                            command = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Sequence":
                            sequence = this.core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "Target":
                            target = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.OnMessage(WixWarnings.DeprecatedAttribute(sourceLineNumbers, node.Name, attrib.Name, "TargetFile", "TargetProperty"));
                            break;
                        case "TargetFile":
                            targetFile = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            this.core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", targetFile);
                            break;
                        case "TargetProperty":
                            targetProperty = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null != target && null != targetFile)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Target", "TargetFile"));
            }

            if (null != target && null != targetProperty)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Target", "TargetProperty"));
            }

            if (null != targetFile && null != targetProperty)
            {
                this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "TargetFile", "TargetProperty"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (YesNoType.Yes == advertise)
            {
                if (null != target)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWhenAdvertised(sourceLineNumbers, node.Name, "Target"));
                }

                if (null != targetFile)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWhenAdvertised(sourceLineNumbers, node.Name, "TargetFile"));
                }

                if (null != targetProperty)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWhenAdvertised(sourceLineNumbers, node.Name, "TargetProperty"));
                }

                if (!this.core.EncounteredError)
                {
                    Row row = this.core.CreateRow(sourceLineNumbers, "Verb");
                    row[0] = extension;
                    row[1] = id;
                    if (CompilerCore.IntegerNotSet != sequence)
                    {
                        row[2] = sequence;
                    }
                    row[3] = command;
                    row[4] = argument;
                }
            }
            else if (YesNoType.No == advertise)
            {
                if (CompilerCore.IntegerNotSet != sequence)
                {
                    this.core.OnMessage(WixErrors.IllegalAttributeWithOtherAttribute(sourceLineNumbers, node.Name, "Sequence", "Advertise", "no"));
                }

                if (null == target && null == targetFile && null == targetProperty)
                {
                    this.core.OnMessage(WixErrors.ExpectedAttributesWithOtherAttribute(sourceLineNumbers, node.Name, "TargetFile", "TargetProperty", "Advertise", "no"));
                }

                if (null == target)
                {
                    if (null != targetFile)
                    {
                        target = String.Concat("\"[#", targetFile, "]\"");
                    }

                    if (null != targetProperty)
                    {
                        target = String.Concat("\"[", targetProperty, "]\"");
                    }
                }

                if (null != argument)
                {
                    target = String.Concat(target, " ", argument);
                }

                string prefix = (null != progId ? progId : String.Concat(".", extension));

                if (null != command)
                {
                    this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(prefix, "\\shell\\", id), String.Empty, command, componentId);
                }

                this.core.CreateRegistryRow(sourceLineNumbers, MsiInterop.MsidbRegistryRootClassesRoot, String.Concat(prefix, "\\shell\\", id, "\\command"), String.Empty, target, componentId);
            }
        }

        /// <summary>
        /// Parses a Wix element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseWixElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            Version requiredVersion = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "RequiredVersion":
                            requiredVersion = this.core.GetAttributeVersionValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null != requiredVersion && CompilerCore.IllegalVersion != requiredVersion)
            {
                Version currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (0 < requiredVersion.CompareTo(currentVersion))
                {
                    this.core.OnMessage(WixErrors.InsufficientVersion(sourceLineNumbers, currentVersion, requiredVersion));
                }
            }

            // process all the sections
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "Fragment":
                                this.ParseFragmentElement(child);
                                break;
                            case "Module":
                                this.ParseModuleElement(child);
                                break;
                            case "PatchCreation":
                                this.ParsePatchCreationElement(child);
                                break;
                            case "Product":
                                this.ParseProductElement(child);
                                break;
                            case "Patch":
                                this.ParsePatchElement(child);
                                break;
                            default:
                                this.core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a WixVariable element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseWixVariableElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            bool overridable = false;
            string value = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Overridable":
                            overridable = (YesNoType.Yes == this.core.GetAttributeYesNoValue(sourceLineNumbers, attrib));
                            break;
                        case "Value":
                            value = this.core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == value)
            {
                this.core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Value"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.core.EncounteredError)
            {
                WixVariableRow wixVariableRow = (WixVariableRow)this.core.CreateRow(sourceLineNumbers, "WixVariable");
                wixVariableRow.Id = id;
                wixVariableRow.Value = value;
                wixVariableRow.Overridable = overridable;
            }
        }

        /// <summary>
        /// Validate the document against the standard WiX schema and any extensions.
        /// </summary>
        /// <param name="document">The xml document to validate.</param>
        private void ValidateDocument(XmlDocument document)
        {
            // if we haven't loaded the schemas yet, do that now
            if (null == this.schemas)
            {
                this.schemas = new XmlSchemaCollection();

                // always add the WiX schema first
                this.schemas.Add(this.schema);

                // add all the extension schemas
                foreach (CompilerExtension compilerExtension in this.extensions.Values)
                {
                    this.schemas.Add(compilerExtension.Schema);
                }
            }

            // write the document to a string for validation
            StringWriter xml = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(xml);
                document.WriteTo(writer);
            }
            catch (ArgumentException)
            {
                this.core.OnMessage(WixErrors.SP1ProbablyNotInstalled());
            }
            finally
            {
                if (null != writer)
                {
                    writer.Close();
                }
            }

            // validate the xml string (and thus the document)
            SourceLineNumberCollection sourceLineNumbers = null;
            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.None);
            XmlValidatingReader validatingReader = null;
            try
            {
                validatingReader = new XmlValidatingReader(xml.ToString(), XmlNodeType.Document, context);
                validatingReader.Schemas.Add(this.schemas);

                while (validatingReader.Read())
                {
                    if (XmlNodeType.ProcessingInstruction == validatingReader.NodeType && Preprocessor.LineNumberElementName == validatingReader.Name)
                    {
                        sourceLineNumbers = new SourceLineNumberCollection(validatingReader.Value);
                    }
                }
            }
            catch (XmlSchemaException e)
            {
                string message = e.Message.Replace(String.Concat(this.schema.TargetNamespace, ":"), String.Empty);

                // find the index of the erroneous line information and chop it off.
                int length = message.IndexOf(" An error occurred at");
                if (-1 == length)
                {
                    length = message.Length; // couldn't find the erroneous info, so just show the whole message.
                }

                this.core.OnMessage(WixErrors.SchemaValidationFailed(sourceLineNumbers, message.Substring(0, length)));
            }
            finally
            {
                if (null != validatingReader)
                {
                    validatingReader.Close();
                }
            }
        }
    }
}
