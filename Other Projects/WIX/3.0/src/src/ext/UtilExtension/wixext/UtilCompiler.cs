//-------------------------------------------------------------------------------------------------
// <copyright file="UtilCompiler.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The compiler for the Windows Installer XML Toolset Utility Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// The compiler for the Windows Installer XML Toolset Utility Extension.
    /// </summary>
    public sealed class UtilCompiler : CompilerExtension
    {
        // user creation attributes definitions (from sca.h)
        internal const int UserDontExpirePasswrd = 0x00000001;
        internal const int UserPasswdCantChange = 0x00000002;
        internal const int UserPasswdChangeReqdOnLogin = 0x00000004;
        internal const int UserDisableAccount = 0x00000008;
        internal const int UserFailIfExists = 0x00000010;
        internal const int UserUpdateIfExists = 0x00000020;

        internal const int UserDontRemoveOnUninstall = 0x00000100;
        internal const int UserDontCreateUser = 0x00000200;

        private XmlSchema schema;

        /// <summary>
        /// Instantiate a new UtilCompiler.
        /// </summary>
        public UtilCompiler()
        {
            this.schema = LoadXmlSchemaHelper(Assembly.GetExecutingAssembly(), "Microsoft.Tools.WindowsInstallerXml.Extensions.Xsd.util.xsd");
        }

        /// <summary>
        /// Types of permission setting methods.
        /// </summary>
        private enum PermissionType
        {
            /// <summary>LockPermissions (normal) type permission setting.</summary>
            LockPermissions,

            /// <summary>FileSharePermissions type permission setting.</summary>
            FileSharePermissions,

            /// <summary>SecureObjects type permission setting.</summary>
            SecureObjects,
        }

        /// <summary>
        /// Gets the schema for this extension.
        /// </summary>
        /// <value>Schema for this extension.</value>
        public override XmlSchema Schema
        {
            get { return this.schema; }
        }

        /// <summary>
        /// Processes an element for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="element">Element to process.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public override void ParseElement(SourceLineNumberCollection sourceLineNumbers, XmlElement parentElement, XmlElement element, params string[] contextValues)
        {
            switch (parentElement.LocalName)
            {
                case "CreateFolder":
                    string createFolderId = contextValues[0];
                    string createFolderComponentId = contextValues[1];

                    switch (element.LocalName)
                    {
                        case "PermissionEx":
                            this.ParsePermissionExElement(element, createFolderId, "CreateFolder");
                            break;
                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                case "Component":
                    string componentId = contextValues[0];
                    string directoryId = contextValues[1];

                    switch (element.LocalName)
                    {
                        case "EventSource":
                            this.ParseEventSourceElement(element, componentId);
                            break;
                        case "FileShare":
                            this.ParseFileShareElement(element, componentId, directoryId);
                            break;
                        case "PerformanceCategory":
                            this.ParsePerformanceCategoryElement(element, componentId);
                            break;
                        case "ServiceConfig":
                            this.ParseServiceConfigElement(element, componentId, "Component", null);
                            break;
                        case "User":
                            this.ParseUserElement(element, componentId);
                            break;
                        case "XmlFile":
                            this.ParseXmlFileElement(element, componentId);
                            break;
                        case "XmlConfig":
                            this.ParseXmlConfigElement(element, componentId, false);
                            break;
                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                case "File":
                    string fileId = contextValues[0];
                    string fileComponentId = contextValues[1];

                    switch (element.LocalName)
                    {
                        case "PerfCounter":
                            this.ParsePerfCounterElement(element, fileComponentId, fileId);
                            break;
                        case "PermissionEx":
                            this.ParsePermissionExElement(element, fileId, "File");
                            break;
                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                case "Fragment":
                case "Module":
                case "Product":
                    switch (element.LocalName)
                    {
                        case "CloseApplication":
                            this.ParseCloseApplicationElement(element);
                            break;
                        case "Group":
                            this.ParseGroupElement(element, null);
                            break;
                        case "User":
                            this.ParseUserElement(element, null);
                            break;
                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                case "Registry":
                case "RegistryKey":
                case "RegistryValue":
                    string registryId = contextValues[0];
                    string registryComponentId = contextValues[1];

                    switch (element.LocalName)
                    {
                        case "PermissionEx":
                            this.ParsePermissionExElement(element, registryId, "Registry");
                            break;
                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                case "ServiceInstall":
                    string serviceInstallId = contextValues[0];
                    string serviceInstallName = contextValues[1];
                    string serviceInstallComponentId = contextValues[2];

                    switch (element.LocalName)
                    {
                        case "PermissionEx":
                            this.ParsePermissionExElement(element, serviceInstallId, "ServiceInstall");
                            break;
                        case "ServiceConfig":
                            this.ParseServiceConfigElement(element, serviceInstallComponentId, "ServiceInstall", serviceInstallName);
                            break;
                        default:
                            this.Core.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                default:
                    this.Core.UnexpectedElement(parentElement, element);
                    break;
            }
        }

        /// <summary>
        /// Parses an event source element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseEventSourceElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string sourceName = null;
            string logName = null;
            string categoryMessageFile = null;
            int categoryCount = CompilerCore.IntegerNotSet;
            string eventMessageFile = null;
            string parameterMessageFile = null;
            int typesSupported = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "CategoryCount":
                            categoryCount = this.Core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "CategoryMessageFile":
                            categoryMessageFile = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "EventMessageFile":
                            eventMessageFile = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Log":
                            logName = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            if ("Security" == logName)
                            {
                                this.Core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, logName, "Application", "System", "<customEventLog>"));
                            }
                            break;
                        case "Name":
                            sourceName = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ParameterMessageFile":
                            parameterMessageFile = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "SupportsErrors":
                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                typesSupported |= 0x01; // EVENTLOG_ERROR_TYPE
                            }
                            break;
                        case "SupportsFailureAudits":
                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                typesSupported |= 0x10; // EVENTLOG_AUDIT_FAILURE
                            }
                            break;
                        case "SupportsInformationals":
                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                typesSupported |= 0x04; // EVENTLOG_INFORMATION_TYPE
                            }
                            break;
                        case "SupportsSuccessAudits":
                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                typesSupported |= 0x08; // EVENTLOG_AUDIT_SUCCESS
                            }
                            break;
                        case "SupportsWarnings":
                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                typesSupported |= 0x02; // EVENTLOG_WARNING_TYPE
                            }
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == sourceName)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (null == logName)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "EventLog"));
            }

            if (null == categoryMessageFile && 0 < categoryCount)
            {
                this.Core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "CategoryCount", "CategoryMessageFile"));
            }

            if (null != categoryMessageFile && CompilerCore.IntegerNotSet == categoryCount)
            {
                this.Core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "CategoryMessageFile", "CategoryCount"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            int registryRoot = 2; // HKLM
            string eventSourceKey = String.Format(@"SYSTEM\CurrentControlSet\Services\EventLog\{0}\{1}", logName, sourceName);

            if (null != categoryMessageFile)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, eventSourceKey, "CategoryMessageFile", String.Concat("#%", categoryMessageFile), componentId, false);
            }

            if (CompilerCore.IntegerNotSet != categoryCount)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, eventSourceKey, "CategoryCount", String.Concat("#", categoryCount), componentId, false);
            }

            if (null != eventMessageFile)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, eventSourceKey, "EventMessageFile", String.Concat("#%", eventMessageFile), componentId, false);
            }

            if (null != parameterMessageFile)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, eventSourceKey, "ParameterMessageFile", String.Concat("#%", parameterMessageFile), componentId, false);
            }

            if (0 != typesSupported)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, eventSourceKey, "TypesSupported", String.Concat("#", typesSupported), componentId, false);
            }
        }

        /// <summary>
        /// Parses a close application element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        private void ParseCloseApplicationElement(XmlNode node)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string condition = null;
            string description = null;
            string target = null;
            string id = null;
            int attributes = 0;
            int sequence = CompilerCore.IntegerNotSet;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Sequence":
                            sequence = this.Core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "Target":
                            target = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == target)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Target"));
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
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // Reference CustomAction since nothing will happen without it
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "WixCloseApplications");

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "WixCloseApplication");
                row[0] = id;
                row[1] = target;
                row[2] = description;
                row[3] = condition;
                row[4] = attributes;
                if (CompilerCore.IntegerNotSet != sequence)
                {
                    row[5] = sequence;
                }
            }
        }

        /// <summary>
        /// Parses a file share element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="directoryId">Identifier of referred to directory.</param>
        private void ParseFileShareElement(XmlNode node, string componentId, string directoryId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string description = null;
            string name = null;
            string id = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Description":
                            description = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            if (1 > node.ChildNodes.Count)
            {
                this.Core.OnMessage(WixErrors.ExpectedElement(sourceLineNumbers, node.Name, "FileSharePermission"));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                            case "FileSharePermission":
                                this.ParseFileSharePermissionElement(child, id);
                                break;
                            default:
                                this.Core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // Reference ConfigureSmb since nothing will happen without it
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "ConfigureSmb");

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "FileShare");
                row[0] = id;
                row[1] = name;
                row[2] = componentId;
                row[3] = description;
                row[4] = directoryId;
            }
        }

        /// <summary>
        /// Parses a FileSharePermission element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="fileShareId">The identifier of the parent FileShare element.</param>
        private void ParseFileSharePermissionElement(XmlNode node, string fileShareId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            BitArray bits = new BitArray(32);
            int permission = 0;
            string user = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "User":
                            user = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "User", user);
                            break;
                        default:
                            YesNoType attribValue = this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if (!CompilerCore.NameToBit(UtilExtension.StandardPermissions, attrib.Name, attribValue, bits, 16))
                            {
                                if (!CompilerCore.NameToBit(UtilExtension.GenericPermissions, attrib.Name, attribValue, bits, 28))
                                {
                                    if (!CompilerCore.NameToBit(UtilExtension.FolderPermissions, attrib.Name, attribValue, bits, 0))
                                    {
                                        this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            permission = CompilerCore.ConvertBitArrayToInt32(bits);

            if (null == user)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "User"));
            }

            if (int.MinValue == permission) // just GENERIC_READ, which is MSI_NULL
            {
                this.Core.OnMessage(WixErrors.GenericReadNotAllowed(sourceLineNumbers));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "FileSharePermissions");
                row[0] = fileShareId;
                row[1] = user;
                row[2] = permission;
            }
        }

        /// <summary>
        /// Parses a group element.
        /// </summary>
        /// <param name="node">Node to be parsed.</param>
        /// <param name="componentId">Component Id of the parent component of this element.</param>
        private void ParseGroupElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string domain = null;
            string name = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Domain":
                            domain = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "Group");
                row[0] = id;
                row[1] = componentId;
                row[2] = name;
                row[3] = domain;
            }
        }

        /// <summary>
        /// Parses a GroupRef element
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="userId">Required user id to be joined to the group.</param>
        private void ParseGroupRefElement(XmlNode node, String userId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string groupId = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            groupId = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Group", groupId);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "UserGroup");
                row[0] = userId;
                row[1] = groupId;
            }
        }

        /// <summary>
        /// Parses a performance category element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParsePerformanceCategoryElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string name = null;
            string help = null;
            YesNoType multiInstance = YesNoType.No;
            int defaultLanguage = CompilerCore.IntegerNotSet;

            ArrayList parsedPerformanceCounters = new ArrayList();

            // default to managed performance counter
            string library = "netfxperf.dll";
            string openEntryPoint = "OpenPerformanceData";
            string collectEntryPoint = "CollectPerformanceData";
            string closeEntryPoint = "ClosePerformanceData";

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Close":
                            closeEntryPoint = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Collect":
                            collectEntryPoint = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "DefaultLanguage":
                            defaultLanguage = this.GetPerformanceCounterLanguage(sourceLineNumbers, attrib);
                            break;
                        case "Help":
                            help = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Id":
                            id = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Library":
                            library = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "MultiInstance":
                            multiInstance = this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Open":
                            openEntryPoint = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                name = id;
            }

            // Process the child counter elements.
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(child);

                        switch (child.LocalName)
                        {
                            case "PerformanceCounter":
                                ParsedPerformanceCounter counter = this.ParsePerformanceCounterElement(child, defaultLanguage);
                                if (null != counter)
                                {
                                    parsedPerformanceCounters.Add(counter);
                                }
                                break;
                            default:
                                this.Core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                // Calculate the ini and h file content.
                string objectName = String.Concat(name.Replace(" ", "_").ToUpper(CultureInfo.InvariantCulture), "_OBJECT");
                string objectLanguage = defaultLanguage.ToString("D3", CultureInfo.InvariantCulture);

                StringBuilder sbIniData = new StringBuilder();
                sbIniData.AppendFormat("[info]\r\ndrivername={0}\r\nsymbolfile={0}.h\r\n\r\n[objects]\r\n{1}_{2}_NAME=\r\n\r\n[languages]\r\n{2}=LANG{2}\r\n\r\n", name, objectName, objectLanguage);
                sbIniData.AppendFormat("[text]\r\n{0}_{1}_NAME={2}\r\n", objectName, objectLanguage, name);
                if (null != help)
                {
                    sbIniData.AppendFormat("{0}_{1}_HELP={2}\r\n", objectName, objectLanguage, help);
                }

                int symbolConstantsCounter = 0;
                StringBuilder sbSymbolicConstants = new StringBuilder();
                sbSymbolicConstants.AppendFormat("#define {0}    {1}\r\n", objectName, symbolConstantsCounter);

                StringBuilder sbCounterNames = new StringBuilder("[~]");
                StringBuilder sbCounterTypes = new StringBuilder("[~]");
                for (int i = 0; i < parsedPerformanceCounters.Count; ++i)
                {
                    ParsedPerformanceCounter counter = (ParsedPerformanceCounter)parsedPerformanceCounters[i];
                    string counterName = counter.Name.Replace(" ", "_").ToUpper(CultureInfo.InvariantCulture);

                    sbIniData.AppendFormat("{0}_{1}_NAME={2}\r\n", counterName, counter.Language, counter.Name);
                    if (null != help)
                    {
                        sbIniData.AppendFormat("{0}_{1}_HELP={2}\r\n", counterName, counter.Language, counter.Help);
                    }

                    symbolConstantsCounter += 2;
                    sbSymbolicConstants.AppendFormat("#define {0}    {1}\r\n", counterName, symbolConstantsCounter);

                    sbCounterNames.Append(counter.Name);
                    sbCounterNames.Append("[~]");
                    sbCounterTypes.Append(counter.Type);
                    sbCounterTypes.Append("[~]");
                }

                sbSymbolicConstants.AppendFormat("#define LAST_{0}_COUNTER_OFFSET    {1}\r\n", objectName, symbolConstantsCounter);

                // Add the calculated INI and H strings to the PerformanceCategory table.
                Row row = this.Core.CreateRow(sourceLineNumbers, "PerformanceCategory");
                row[0] = id;
                row[1] = componentId;
                row[2] = name;
                row[3] = sbIniData.ToString();
                row[4] = sbSymbolicConstants.ToString();

                // Set up the application's performance key.
                int registryRoot = 2; // HKLM
                string linkageKey = String.Format(@"SYSTEM\CurrentControlSet\Services\{0}\Linkage", name);
                string performanceKey = String.Format(@"SYSTEM\CurrentControlSet\Services\{0}\Performance", name);

                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, linkageKey, "Export", name, componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "-", null, componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "Library", library, componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "Open", openEntryPoint, componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "Collect", collectEntryPoint, componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "Close", closeEntryPoint, componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "IsMultiInstance", YesNoType.Yes == multiInstance ? "#1" : "#0", componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "Counter Names", sbCounterNames.ToString(), componentId, false);
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, performanceKey, "Counter Types", sbCounterTypes.ToString(), componentId, false);
            }

            // Reference InstallPerfCounterData and UninstallPerfCounterData since nothing will happen without them
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "InstallPerfCounterData");
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "UninstallPerfCounterData");
        }

        /// <summary>
        /// Gets the performance counter language as a decimal number.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>Numeric representation of the language as per WinNT.h.</returns>
        private int GetPerformanceCounterLanguage(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            int language = 0;
            if (String.Empty == attribute.Value)
            {
                this.Core.OnMessage(WixErrors.IllegalEmptyAttributeValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name));
            }
            else
            {
                switch (attribute.Value)
                {
                    case "afrikaans":
                        language = 0x36;
                        break;
                    case "albanian":
                        language = 0x1c;
                        break;
                    case "arabic":
                        language = 0x01;
                        break;
                    case "armenian":
                        language = 0x2b;
                        break;
                    case "assamese":
                        language = 0x4d;
                        break;
                    case "azeri":
                        language = 0x2c;
                        break;
                    case "basque":
                        language = 0x2d;
                        break;
                    case "belarusian":
                        language = 0x23;
                        break;
                    case "bengali":
                        language = 0x45;
                        break;
                    case "bulgarian":
                        language = 0x02;
                        break;
                    case "catalan":
                        language = 0x03;
                        break;
                    case "chinese":
                        language = 0x04;
                        break;
                    case "croatian":
                        language = 0x1a;
                        break;
                    case "czech":
                        language = 0x05;
                        break;
                    case "danish":
                        language = 0x06;
                        break;
                    case "divehi":
                        language = 0x65;
                        break;
                    case "dutch":
                        language = 0x13;
                        break;
                    case "piglatin":
                    case "english":
                        language = 0x09;
                        break;
                    case "estonian":
                        language = 0x25;
                        break;
                    case "faeroese":
                        language = 0x38;
                        break;
                    case "farsi":
                        language = 0x29;
                        break;
                    case "finnish":
                        language = 0x0b;
                        break;
                    case "french":
                        language = 0x0c;
                        break;
                    case "galician":
                        language = 0x56;
                        break;
                    case "georgian":
                        language = 0x37;
                        break;
                    case "german":
                        language = 0x07;
                        break;
                    case "greek":
                        language = 0x08;
                        break;
                    case "gujarati":
                        language = 0x47;
                        break;
                    case "hebrew":
                        language = 0x0d;
                        break;
                    case "hindi":
                        language = 0x39;
                        break;
                    case "hungarian":
                        language = 0x0e;
                        break;
                    case "icelandic":
                        language = 0x0f;
                        break;
                    case "indonesian":
                        language = 0x21;
                        break;
                    case "italian":
                        language = 0x10;
                        break;
                    case "japanese":
                        language = 0x11;
                        break;
                    case "kannada":
                        language = 0x4b;
                        break;
                    case "kashmiri":
                        language = 0x60;
                        break;
                    case "kazak":
                        language = 0x3f;
                        break;
                    case "konkani":
                        language = 0x57;
                        break;
                    case "korean":
                        language = 0x12;
                        break;
                    case "kyrgyz":
                        language = 0x40;
                        break;
                    case "latvian":
                        language = 0x26;
                        break;
                    case "lithuanian":
                        language = 0x27;
                        break;
                    case "macedonian":
                        language = 0x2f;
                        break;
                    case "malay":
                        language = 0x3e;
                        break;
                    case "malayalam":
                        language = 0x4c;
                        break;
                    case "manipuri":
                        language = 0x58;
                        break;
                    case "marathi":
                        language = 0x4e;
                        break;
                    case "mongolian":
                        language = 0x50;
                        break;
                    case "nepali":
                        language = 0x61;
                        break;
                    case "norwegian":
                        language = 0x14;
                        break;
                    case "oriya":
                        language = 0x48;
                        break;
                    case "polish":
                        language = 0x15;
                        break;
                    case "portuguese":
                        language = 0x16;
                        break;
                    case "punjabi":
                        language = 0x46;
                        break;
                    case "romanian":
                        language = 0x18;
                        break;
                    case "russian":
                        language = 0x19;
                        break;
                    case "sanskrit":
                        language = 0x4f;
                        break;
                    case "serbian":
                        language = 0x1a;
                        break;
                    case "sindhi":
                        language = 0x59;
                        break;
                    case "slovak":
                        language = 0x1b;
                        break;
                    case "slovenian":
                        language = 0x24;
                        break;
                    case "spanish":
                        language = 0x0a;
                        break;
                    case "swahili":
                        language = 0x41;
                        break;
                    case "swedish":
                        language = 0x1d;
                        break;
                    case "syriac":
                        language = 0x5a;
                        break;
                    case "tamil":
                        language = 0x49;
                        break;
                    case "tatar":
                        language = 0x44;
                        break;
                    case "telugu":
                        language = 0x4a;
                        break;
                    case "thai":
                        language = 0x1e;
                        break;
                    case "turkish":
                        language = 0x1f;
                        break;
                    case "ukrainian":
                        language = 0x22;
                        break;
                    case "urdu":
                        language = 0x20;
                        break;
                    case "uzbek":
                        language = 0x43;
                        break;
                    case "vietnamese":
                        language = 0x2a;
                        break;
                    default:
                        this.Core.OnMessage(WixErrors.IllegalEmptyAttributeValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name));
                        break;
                }
            }

            return language;
        }

        /// <summary>
        /// Parses a performance counter element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="defaultLanguage">Default language for the performance counter.</param>
        private ParsedPerformanceCounter ParsePerformanceCounterElement(XmlNode node, int defaultLanguage)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            ParsedPerformanceCounter parsedPerformanceCounter = null;
            string name = null;
            string help = null;
            System.Diagnostics.PerformanceCounterType type = System.Diagnostics.PerformanceCounterType.NumberOfItems32;
            int language = defaultLanguage;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Help":
                            help = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Type":
                            type = this.GetPerformanceCounterType(sourceLineNumbers, attrib);
                            break;
                        case "Language":
                            language = this.GetPerformanceCounterLanguage(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                parsedPerformanceCounter = new ParsedPerformanceCounter(name, help, type, language);
            }

            return parsedPerformanceCounter;
        }

        /// <summary>
        /// Gets the performance counter type.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>Numeric representation of the language as per WinNT.h.</returns>
        private System.Diagnostics.PerformanceCounterType GetPerformanceCounterType(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            System.Diagnostics.PerformanceCounterType type = System.Diagnostics.PerformanceCounterType.NumberOfItems32;
            if (String.Empty == attribute.Value)
            {
                this.Core.OnMessage(WixErrors.IllegalEmptyAttributeValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name));
            }
            else
            {
                switch (attribute.Value)
                {
                    case "averageBase":
                        type = System.Diagnostics.PerformanceCounterType.AverageBase;
                        break;
                    case "averageCount64":
                        type = System.Diagnostics.PerformanceCounterType.AverageCount64;
                        break;
                    case "averageTimer32":
                        type = System.Diagnostics.PerformanceCounterType.AverageTimer32;
                        break;
                    case "counterDelta32":
                        type = System.Diagnostics.PerformanceCounterType.CounterDelta32;
                        break;
                    case "counterTimerInverse":
                        type = System.Diagnostics.PerformanceCounterType.CounterTimerInverse;
                        break;
                    case "sampleFraction":
                        type = System.Diagnostics.PerformanceCounterType.SampleFraction;
                        break;
                    case "timer100Ns":
                        type = System.Diagnostics.PerformanceCounterType.Timer100Ns;
                        break;
                    case "counterTimer":
                        type = System.Diagnostics.PerformanceCounterType.CounterTimer;
                        break;
                    case "rawFraction":
                        type = System.Diagnostics.PerformanceCounterType.RawFraction;
                        break;
                    case "timer100NsInverse":
                        type = System.Diagnostics.PerformanceCounterType.Timer100NsInverse;
                        break;
                    case "counterMultiTimer":
                        type = System.Diagnostics.PerformanceCounterType.CounterMultiTimer;
                        break;
                    case "counterMultiTimer100Ns":
                        type = System.Diagnostics.PerformanceCounterType.CounterMultiTimer100Ns;
                        break;
                    case "counterMultiTimerInverse":
                        type = System.Diagnostics.PerformanceCounterType.CounterMultiTimerInverse;
                        break;
                    case "counterMultiTimer100NsInverse":
                        type = System.Diagnostics.PerformanceCounterType.CounterMultiTimer100NsInverse;
                        break;
                    case "elapsedTime":
                        type = System.Diagnostics.PerformanceCounterType.ElapsedTime;
                        break;
                    case "sampleBase":
                        type = System.Diagnostics.PerformanceCounterType.SampleBase;
                        break;
                    case "rawBase":
                        type = System.Diagnostics.PerformanceCounterType.RawBase;
                        break;
                    case "counterMultiBase":
                        type = System.Diagnostics.PerformanceCounterType.CounterMultiBase;
                        break;
                    case "rateOfCountsPerSecond64":
                        type = System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond64;
                        break;
                    case "rateOfCountsPerSecond32":
                        type = System.Diagnostics.PerformanceCounterType.RateOfCountsPerSecond32;
                        break;
                    case "countPerTimeInterval64":
                        type = System.Diagnostics.PerformanceCounterType.CountPerTimeInterval64;
                        break;
                    case "countPerTimeInterval32":
                        type = System.Diagnostics.PerformanceCounterType.CountPerTimeInterval32;
                        break;
                    case "sampleCounter":
                        type = System.Diagnostics.PerformanceCounterType.SampleCounter;
                        break;
                    case "counterDelta64":
                        type = System.Diagnostics.PerformanceCounterType.CounterDelta64;
                        break;
                    case "numberOfItems64":
                        type = System.Diagnostics.PerformanceCounterType.NumberOfItems64;
                        break;
                    case "numberOfItems32":
                        type = System.Diagnostics.PerformanceCounterType.NumberOfItems32;
                        break;
                    case "numberOfItemsHEX64":
                        type = System.Diagnostics.PerformanceCounterType.NumberOfItemsHEX64;
                        break;
                    case "numberOfItemsHEX32":
                        type = System.Diagnostics.PerformanceCounterType.NumberOfItemsHEX32;
                        break;
                    default:
                        this.Core.OnMessage(WixErrors.IllegalEmptyAttributeValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name));
                        break;
                }
            }

            return type;
        }

        /// <summary>
        /// Parses a perf counter element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="fileId">Identifier of referenced file.</param>
        private void ParsePerfCounterElement(XmlNode node, string componentId, string fileId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string name = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == name)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "Perfmon");
                row[0] = componentId;
                row[1] = String.Concat("[#", fileId, "]");
                row[2] = name;
            }

            // Reference ConfigurePerfmonInstall and ConfigurePerfmonUninstall since nothing will happen without them
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "ConfigurePerfmonInstall");
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "ConfigurePerfmonUninstall");
        }

        /// <summary>
        /// Parses a PermissionEx element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="objectId">Identifier of object to be secured.</param>
        /// <param name="tableName">Name of table that contains objectId.</param>
        private void ParsePermissionExElement(XmlNode node, string objectId, string tableName)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            BitArray bits = new BitArray(32);
            string domain = null;
            int permission = 0;
            string[] specialPermissions = null;
            string user = null;

            PermissionType permissionType = PermissionType.SecureObjects;

            switch (tableName)
            {
                case "CreateFolder":
                    specialPermissions = UtilExtension.FolderPermissions;
                    break;
                case "File":
                    specialPermissions = UtilExtension.FilePermissions;
                    break;
                case "Registry":
                    specialPermissions = UtilExtension.RegistryPermissions;
                    break;
                case "ServiceInstall":
                    specialPermissions = UtilExtension.ServicePermissions;
                    permissionType = PermissionType.SecureObjects;
                    break;
                default:
                    this.Core.UnexpectedElement(node.ParentNode, node);
                    break;
            }

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Domain":
                            if (PermissionType.FileSharePermissions == permissionType)
                            {
                                this.Core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            domain = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "User":
                            user = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            YesNoType attribValue = this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib);
                            if (!CompilerCore.NameToBit(UtilExtension.StandardPermissions, attrib.Name, attribValue, bits, 16))
                            {
                                if (!CompilerCore.NameToBit(UtilExtension.GenericPermissions, attrib.Name, attribValue, bits, 28))
                                {
                                    if (!CompilerCore.NameToBit(specialPermissions, attrib.Name, attribValue, bits, 0))
                                    {
                                        this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            permission = CompilerCore.ConvertBitArrayToInt32(bits);

            if (null == user)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "User"));
            }

            if (int.MinValue == permission) // just GENERIC_READ, which is MSI_NULL
            {
                this.Core.OnMessage(WixErrors.GenericReadNotAllowed(sourceLineNumbers));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "SecureObjects");
                row[0] = objectId;
                row[1] = tableName;
                row[2] = domain;
                row[3] = user;
                row[4] = permission;

                // Reference SchedSecureObjects since nothing will happen without it
                this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "SchedSecureObjects");
            }
        }

        /// <summary>
        /// Parses a service configuration element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="parentTableName">Name of parent element.</param>
        /// <param name="parentTableServiceName">Optional name of service </param>
        private void ParseServiceConfigElement(XmlNode node, string componentId, string parentTableName, string parentTableServiceName)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string firstFailureActionType = null;
            bool newService = false;
            string programCommandLine = null;
            string rebootMessage = null;
            int resetPeriod = CompilerCore.IntegerNotSet;
            int restartServiceDelay = CompilerCore.IntegerNotSet;
            string secondFailureActionType = null;
            string serviceName = null;
            string thirdFailureActionType = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "FirstFailureActionType":
                            firstFailureActionType = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ProgramCommandLine":
                            programCommandLine = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "RebootMessage":
                            rebootMessage = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ResetPeriodInDays":
                            resetPeriod = this.Core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "RestartServiceDelayInSeconds":
                            restartServiceDelay = this.Core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 0, int.MaxValue);
                            break;
                        case "SecondFailureActionType":
                            secondFailureActionType = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ServiceName":
                            serviceName = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ThirdFailureActionType":
                            thirdFailureActionType = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            // if this element is a child of ServiceInstall then ignore the service name provided.
            if ("ServiceInstall" == parentTableName)
            {
                // TODO: the ServiceName attribute should not be allowed in this case (the overwriting behavior may confuse users)
                serviceName = parentTableServiceName;
                newService = true;
            }
            else
            {
                // not a child of ServiceInstall, so ServiceName must have been provided
                if (null == serviceName)
                {
                    this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "ServiceName"));
                }
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            // Reference SchedServiceConfig since nothing will happen without it
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "SchedServiceConfig");

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "ServiceConfig");
                row[0] = serviceName;
                row[1] = componentId;
                row[2] = (newService ? 1 : 0);
                row[3] = firstFailureActionType;
                row[4] = secondFailureActionType;
                row[5] = thirdFailureActionType;
                if (CompilerCore.IntegerNotSet != resetPeriod)
                {
                    row[6] = resetPeriod;
                }

                if (CompilerCore.IntegerNotSet != restartServiceDelay)
                {
                    row[7] = restartServiceDelay;
                }
                row[8] = programCommandLine;
                row[9] = rebootMessage;
            }
        }

        /// <summary>
        /// Parses an user element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Optional identifier of parent component.</param>
        private void ParseUserElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            int attributes = 0;
            string domain = null;
            string name = null;
            string password = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "CanNotChangePassword":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserPasswdCantChange;
                            }
                            break;
                        case "CreateUser":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.No == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserDontCreateUser;
                            }
                            break;
                        case "Disabled":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserDisableAccount;
                            }
                            break;
                        case "Domain":
                            domain = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "FailIfExists":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserFailIfExists;
                            }
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Password":
                            password = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "PasswordExpired":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserPasswdChangeReqdOnLogin;
                            }
                            break;
                        case "PasswordNeverExpires":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserDontExpirePasswrd;
                            }
                            break;
                        case "RemoveOnUninstall":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.No == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserDontRemoveOnUninstall;
                            }
                            break;
                        case "UpdateIfExists":
                            if (null == componentId)
                            {
                                this.Core.OnMessage(UtilErrors.IllegalAttributeWithoutComponent(sourceLineNumbers, node.Name, attrib.Name));
                            }

                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                attributes |= UserUpdateIfExists;
                            }
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == name)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Name"));
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
                            case "GroupRef":
                                if (null == componentId)
                                {
                                    this.Core.OnMessage(UtilErrors.IllegalElementWithoutComponent(childSourceLineNumbers, child.Name));
                                }

                                this.ParseGroupRefElement(child, id);
                                break;
                            default:
                                this.Core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (null != componentId)
            {
                // Reference ConfigureIIs since nothing will happen without it
                this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "ConfigureUsers");
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "User");
                row[0] = id;
                row[1] = componentId;
                row[2] = name;
                row[3] = domain;
                row[4] = password;
                row[5] = attributes;
            }
        }

        /// <summary>
        /// Parses a XmlFile element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        private void ParseXmlFileElement(XmlNode node, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string file = null;
            string elementPath = null;
            string name = null;
            string value = null;
            int sequence = -1;
            int flags = 0;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Action":
                            string actionValue = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            switch (actionValue)
                            {
                                case "createElement":
                                    flags |= 0x00000001; // XMLFILE_CREATE_ELEMENT
                                    break;
                                case "deleteValue":
                                    flags |= 0x00000002; // XMLFILE_DELETE_VALUE
                                    break;
                                case "setValue":
                                    // no flag for set value since it's the default
                                    break;
                                default:
                                    this.Core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, "Action", actionValue, "createElement", "deleteValue", "setValue"));
                                    break;
                            }
                            break;
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "File":
                            file = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "ElementPath":
                            elementPath = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Permanent":
                            if (YesNoType.Yes == this.Core.GetAttributeYesNoValue(sourceLineNumbers, attrib))
                            {
                                flags |= 0x00010000; // XMLFILE_DONT_UNINSTALL
                            }
                            break;
                        case "Sequence":
                            sequence = this.Core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "Value":
                            value = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == file)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "File"));
            }

            if (null == elementPath)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "ElementPath"));
            }

            if ((0x00000001 /*XMLFILE_CREATE_ELEMENT*/ & flags) != 0 && null == name)
            {
                this.Core.OnMessage(WixErrors.IllegalAttributeWithoutOtherAttributes(sourceLineNumbers, node.Name, "Action", "Name"));
            }

            // find unexpected child elements
            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        this.Core.UnexpectedElement(node, child);
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "XmlFile");
                row[0] = id;
                row[1] = file;
                row[2] = elementPath;
                row[3] = name;
                row[4] = value;
                row[5] = flags;
                row[6] = componentId;
                if (-1 != sequence)
                {
                    row[7] = sequence;
                }
            }

            // Reference SchedXmlFile since nothing will happen without it
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "SchedXmlFile");
        }

                /// <summary>
        /// Parses a XmlConfig element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="componentId">Identifier of parent component.</param>
        /// <param name="nested">Whether or not the element is nested.</param>
        private void ParseXmlConfigElement(XmlNode node, string componentId, bool nested)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string elementPath = null;
            int flags = 0;
            string file = null;
            string name = null;
            int sequence = CompilerCore.IntegerNotSet;
            string value = null;
            string verifyPath = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "Action":
                            if (nested)
                            {
                                this.Core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            else
                            {
                                string actionValue = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                                switch (actionValue)
                                {
                                    case "create":
                                        flags |= 0x4; // XMLCONFIG_CREATE
                                        break;
                                    case "delete":
                                        flags |= 0x8; // XMLCONFIG_DELETE
                                        break;
                                    default:
                                        this.Core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, actionValue, "create", "delete"));
                                        break;
                                }
                            }
                            break;
                        case "ElementPath":
                            elementPath = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "File":
                            file = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Name":
                            name = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Node":
                            if (nested)
                            {
                                this.Core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            else
                            {
                                string nodeValue = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                                switch (nodeValue)
                                {
                                    case "element":
                                        flags |= 0x1; // XMLCONFIG_ELEMENT
                                        break;
                                    case "value":
                                        flags |= 0x2; // XMLCONFIG_VALUE
                                        break;
                                    default:
                                        this.Core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, nodeValue, "element", "value"));
                                        break;
                                }
                            }
                            break;
                        case "On":
                            if (nested)
                            {
                                this.Core.OnMessage(WixErrors.IllegalAttributeWhenNested(sourceLineNumbers, node.Name, attrib.Name, node.ParentNode.Name));
                            }
                            else
                            {
                                string onValue = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                                switch (onValue)
                                {
                                    case "install":
                                        flags |= 0x10; // XMLCONFIG_INSTALL
                                        break;
                                    case "uninstall":
                                        flags |= 0x20; // XMLCONFIG_UNINSTALL
                                        break;
                                    default:
                                        this.Core.OnMessage(WixErrors.IllegalAttributeValue(sourceLineNumbers, node.Name, attrib.Name, onValue, "install", "uninstall"));
                                        break;
                                }
                            }
                            break;
                        case "Sequence":
                            sequence = this.Core.GetAttributeIntegerValue(sourceLineNumbers, attrib, 1, short.MaxValue);
                            break;
                        case "Value":
                            value = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "VerifyPath":
                            verifyPath = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.Core.UnexpectedAttribute(sourceLineNumbers, attrib);
                            break;
                    }
                }
                else
                {
                    this.Core.UnsupportedExtensionAttribute(sourceLineNumbers, attrib);
                }
            }

            if (null == id)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "Id"));
            }

            if (null == file)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "File"));
            }

            if (null == elementPath)
            {
                this.Core.OnMessage(WixErrors.ExpectedAttribute(sourceLineNumbers, node.Name, "ElementPath"));
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
                            case "XmlConfig":
                                if (nested)
                                {
                                    this.Core.OnMessage(WixErrors.UnexpectedElement(sourceLineNumbers, node.Name, child.Name));
                                }
                                else
                                {
                                    this.ParseXmlConfigElement(child, componentId, true);
                                }
                                break;
                            default:
                                this.Core.UnexpectedElement(node, child);
                                break;
                        }
                    }
                    else
                    {
                        this.Core.UnsupportedExtensionElement(node, child);
                    }
                }
            }

            if (!this.Core.EncounteredError)
            {
                Row row = this.Core.CreateRow(sourceLineNumbers, "XmlConfig");
                row[0] = id;
                row[1] = file;
                row[2] = elementPath;
                row[3] = verifyPath;
                row[4] = name;
                row[5] = value;
                row[6] = flags;
                row[7] = componentId;
                if (CompilerCore.IntegerNotSet != sequence)
                {
                    row[8] = sequence;
                }
            }

            // Reference SchedXmlConfig since nothing will happen without it
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "CustomAction", "SchedXmlConfig");
        }

        /// <summary>
        /// Private class that stores the data from a parsed PerformanceCounter element.
        /// </summary>
        private class ParsedPerformanceCounter
        {
            string name;
            string help;
            int type;
            string language;

            internal ParsedPerformanceCounter(string name, string help, System.Diagnostics.PerformanceCounterType type, int language)
            {
                this.name = name;
                this.help = help;
                this.type = (int)type;
                this.language = language.ToString("D3", CultureInfo.InvariantCulture);
            }

            internal string Name
            {
                get { return this.name; }
            }

            internal string Help
            {
                get { return this.help; }
            }

            internal int Type
            {
                get { return this.type; }
            }

            internal string Language
            {
                get { return this.language; }
            }
        }
    }
}