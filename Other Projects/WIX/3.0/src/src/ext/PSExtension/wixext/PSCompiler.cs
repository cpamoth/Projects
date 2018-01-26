//-------------------------------------------------------------------------------------------------
// <copyright file="PSCompiler.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The compiler for the Windows Installer XML Toolset PowerShell Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The compiler for the Windows Installer XML Toolset Internet Information Services Extension.
    /// </summary>
    public sealed class PSCompiler : CompilerExtension
    {
        private const string KeyFormat = @"SOFTWARE\Microsoft\PowerShell\{0}\PowerShellSnapIns\{1}";
        private const string VarPrefix = "PSVersionMajor";

        private XmlSchema schema;
        private Regex versionRegex = null;

        /// <summary>
        /// Instantiate a new PSCompiler.
        /// </summary>
        public PSCompiler()
        {
            this.schema = LoadXmlSchemaHelper(Assembly.GetExecutingAssembly(), "Microsoft.Tools.WindowsInstallerXml.Extensions.Xsd.ps.xsd");
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
        /// Gets a <see cref="Regex"/> object to parse the version number of the assembly.
        /// </summary>
        /// <value>A <see cref="Regex"/> object to parse the version number of the assembly.</value>
        private Regex VersionRegex
        {
            get
            {
                if (null == this.versionRegex)
                {
                    this.versionRegex = new Regex(@"version=(?<Version>\d{1,5}(\.\d{1,5}){0,3})", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                }

                return this.versionRegex;
            }
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
            case "File":
                string fileId = contextValues[0];
                string componentId = contextValues[1];

                switch (element.LocalName)
                {
                case "FormatsFile":
                    this.ParseExtensionsFile(element, "Formats", fileId, componentId);
                    break;

                case "SnapIn":
                    this.ParseSnapInElement(element, fileId, componentId);
                    break;

                case "TypesFile":
                    this.ParseExtensionsFile(element, "Types", fileId, componentId);
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
        /// Parses a SnapIn element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="fileId">Identifier for parent file.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        private void ParseSnapInElement(XmlNode node, string fileId, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string id = null;
            string assemblyName = null;
            string description = null;
            string descriptionIndirect = null;
            Version requiredPowerShellVersion = CompilerCore.IllegalVersion;
            string vendor = null;
            string vendorIndirect = null;
            Version version = CompilerCore.IllegalVersion;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                        case "Id":
                            id = this.Core.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;

                        case "AssemblyName":
                            assemblyName = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "Description":
                            description = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "DescriptionIndirect":
                            descriptionIndirect = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "RequiredPowerShellVersion":
                            requiredPowerShellVersion = this.Core.GetAttributeVersionValue(sourceLineNumbers, attrib);
                            break;

                        case "Vendor":
                            vendor = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "VendorIndirect":
                            vendorIndirect = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                            break;

                        case "Version":
                            version = this.Core.GetAttributeVersionValue(sourceLineNumbers, attrib);
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

            // Default to require PowerShell 1.0.
            if (CompilerCore.IllegalVersion == requiredPowerShellVersion)
            {
                requiredPowerShellVersion = new Version(1, 0);
            }

            // Get the snapin version.
            if (CompilerCore.IllegalVersion == version && null != assemblyName)
            {
                Match m = this.VersionRegex.Match(assemblyName);
                if (m.Success)
                {
                    version = new Version(m.Groups["Version"].Value);
                }
            }

            if (CompilerCore.IllegalVersion == version)
            {
                this.Core.OnMessage(PSErrors.IllegalSnapInVersion(sourceLineNumbers, id));
            }

            foreach (XmlNode child in node.ChildNodes)
            {
                if (XmlNodeType.Element == child.NodeType)
                {
                    if (child.NamespaceURI == this.schema.TargetNamespace)
                    {
                        switch (child.LocalName)
                        {
                        case "FormatsFile":
                            this.ParseExtensionsFile(child, "Formats", id, componentId);
                            break;
                        case "TypesFile":
                            this.ParseExtensionsFile(child, "Types", id, componentId);
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

            // Get the major part of the required PowerShell version which is
            // needed for the registry key, and put that into a WiX variable
            // for use in Formats and Types files.
            string major = requiredPowerShellVersion.Major.ToString();
            WixVariableRow wixVariableRow = (WixVariableRow)this.Core.CreateRow(sourceLineNumbers, "WixVariable");
            wixVariableRow.Id = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", VarPrefix, id);
            wixVariableRow.Value = major;
            wixVariableRow.Overridable = false;

            int registryRoot = 2; // HKLM
            string registryKey = string.Format(CultureInfo.InvariantCulture, KeyFormat, major, id);

            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "ApplicationBase", string.Format(CultureInfo.InvariantCulture, "[${0}]", componentId), componentId, false);

            if (null != assemblyName)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "AssemblyName", assemblyName, componentId, false);
            }

            if (null != description)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "Description", description, componentId, false);
            }

            if (null != descriptionIndirect)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "DescriptionIndirect", descriptionIndirect, componentId, false);
            }

            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "ModuleName", string.Format(CultureInfo.InvariantCulture, "[#{0}]", fileId), componentId, false);

            // Create PowerShellVersion row and import appropriate AppSearch.
            string requiredVersion = requiredPowerShellVersion.ToString(2);
            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "PowerShellVersion", requiredVersion, componentId, false);
            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "Property", string.Concat("POWERSHELL", requiredVersion));

            if (null != vendor)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "Vendor", vendor, componentId, false);
            }

            if (null != vendorIndirect)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "VendorIndirect", vendorIndirect, componentId, false);
            }

            if (null != version)
            {
                this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, "Version", version.ToString(), componentId, false);
            }
        }

        /// <summary>
        /// Parses a FormatsFile and TypesFile element.
        /// </summary>
        /// <param name="node">Element to parse.</param>
        /// <param name="valueName">Registry value name.</param>
        /// <param name="id">Idendifier for parent file or snap-in.</param>
        /// <param name="componentId">Identifier for parent component.</param>
        private void ParseExtensionsFile(XmlNode node, string valueName, string id, string componentId)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(node);
            string fileId = null;
            string snapIn = null;

            foreach (XmlAttribute attrib in node.Attributes)
            {
                if (0 == attrib.NamespaceURI.Length || attrib.NamespaceURI == this.schema.TargetNamespace)
                {
                    switch (attrib.LocalName)
                    {
                    case "FileId":
                        fileId = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
                        snapIn = id;
                        break;
                        
                    case "SnapIn":
                        fileId = id;
                        snapIn = this.Core.GetAttributeValue(sourceLineNumbers, attrib);
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

            if (null == fileId && null == snapIn)
            {
                this.Core.OnMessage(PSErrors.NeitherIdSpecified(sourceLineNumbers, valueName));
            }

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
            string registryKey = string.Format(CultureInfo.InvariantCulture, KeyFormat, string.Format(CultureInfo.InvariantCulture, "!(wix.{0}_{1})", VarPrefix, snapIn), snapIn);

            this.Core.CreateWixSimpleReferenceRow(sourceLineNumbers, "File", fileId);
            this.Core.CreateRegistryRow(sourceLineNumbers, registryRoot, registryKey, valueName, string.Format(CultureInfo.InvariantCulture, "[~][#{0}]", fileId), componentId, false);
        }
    }
}
