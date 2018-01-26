//-------------------------------------------------------------------------------------------------
// <copyright file="CompilerCore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base compiler extension.  Any of these methods can be overridden to change
// the behavior of the compiler.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Yes/no type (kinda like a boolean).
    /// </summary>
    public enum YesNoType
    {
        /// <summary>Value not set; equivalent to null for reference types.</summary>
        NotSet,

        /// <summary>The no value.</summary>
        No,

        /// <summary>The yes value.</summary>
        Yes,

        /// <summary>Not a valid yes or no value.</summary>
        IllegalValue,
    }

    /// <summary>
    /// Yes, No, Default xml simple type.
    /// </summary>
    public enum YesNoDefaultType
    {
        /// <summary>Value not set; equivalent to null for reference types.</summary>
        NotSet,

        /// <summary>The default value.</summary>
        Default,

        /// <summary>The no value.</summary>
        No,

        /// <summary>The yes value.</summary>
        Yes,

        /// <summary>Not a valid yes, no or default value.</summary>
        IllegalValue,
    }

    /// <summary>
    /// Core class for the compiler.
    /// </summary>
    public sealed class CompilerCore : IMessageHandler
    {
        public const int IntegerNotSet = int.MinValue;
        public const int IllegalInteger = int.MinValue + 1;
        public const long LongNotSet = long.MinValue;
        public const long IllegalLong = long.MinValue + 1;
        public const string IllegalEmptyAttributeValue = "";
        public const string IllegalGuid = "IllegalGuid";
        public const string IllegalIdentifier = "";
        public static readonly Version IllegalVersion = new Version(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);

        private static readonly Regex AmbiguousFilename = new Regex(@"^.{6}\~\d", RegexOptions.Compiled);
        private static readonly Regex LegalIdentifierCharacters = new Regex(@"^[_A-Za-z][0-9A-Za-z_\.]*$", RegexOptions.Compiled);

        private const string LegalShortFilenameCharacters = @"[^\\\?|><:/\*""\+,;=\[\]\. ]"; // illegal: \ ? | > < : / * " + , ; = [ ] . (space)
        private static readonly Regex LegalShortFilename = new Regex(String.Concat("^", LegalShortFilenameCharacters, @"{1,8}(\.", LegalShortFilenameCharacters, "{0,3})?$"), RegexOptions.Compiled);

        private const string LegalLongFilenameCharacters = @"[^\\\?|><:/\*""]"; // illegal: \ ? | > < : / * "
        private static readonly Regex LegalLongFilename = new Regex(String.Concat("^", LegalLongFilenameCharacters, @"{1,259}$"), RegexOptions.Compiled);

        private const string LegalWildcardShortFilenameCharacters = @"[^\\|><:/""\+,;=\[\]\. ]"; // illegal: \ | > < : / " + , ; = [ ] . (space)
        private static readonly Regex LegalWildcardShortFilename = new Regex(String.Concat("^", LegalWildcardShortFilenameCharacters, @"{1,16}(\.", LegalWildcardShortFilenameCharacters, "{0,6})?$"));

        private const string LegalWildcardLongFilenameCharacters = @"[^\\|><:/""]"; // illegal: \ | > < : / "
        private static readonly Regex LegalWildcardLongFilename = new Regex(String.Concat("^", LegalWildcardLongFilenameCharacters, @"{1,259}$"));

        private TableDefinitionCollection tableDefinitions;
        private Hashtable extensions;
        private Intermediate intermediate;
        private bool showPedanticMessages;

        private Section activeSection;
        private bool encounteredError;

        /// <summary>
        /// Constructor for all compiler core.
        /// </summary>
        /// <param name="intermediate">The Intermediate object representing compiled source document.</param>
        /// <param name="tableDefinitions">The loaded table definition collection.</param>
        /// <param name="extensions">The WiX extensions collection.</param>
        /// <param name="messageHandler">The message handler.</param>
        internal CompilerCore(Intermediate intermediate, TableDefinitionCollection tableDefinitions, Hashtable extensions, MessageEventHandler messageHandler)
        {
            this.tableDefinitions = tableDefinitions;
            this.extensions = extensions;
            this.intermediate = intermediate;
            this.MessageHandler = messageHandler;
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        private event MessageEventHandler MessageHandler;

        /// <summary>
        /// Gets whether the compiler core encountered an error while processing.
        /// </summary>
        /// <value>Flag if core encountered and error during processing.</value>
        public bool EncounteredError
        {
            get { return this.encounteredError; }
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
        /// Gets the table definitions used by the compiler core.
        /// </summary>
        /// <value>Table definition collection.</value>
        public TableDefinitionCollection TableDefinitions
        {
            get { return this.tableDefinitions; }
        }

        /// <summary>
        /// Convert a bit array into an int value.
        /// </summary>
        /// <param name="bits">The bit array to convert.</param>
        /// <returns>The converted int value.</returns>
        public static int ConvertBitArrayToInt32(BitArray bits)
        {
            Debug.Assert(32 == bits.Length);

            int[] intArray = new int[1];

            bits.CopyTo(intArray, 0);

            return intArray[0];
        }

        /// <summary>
        /// Sets a bit in a bit array based on the index at which an attribute name was found in a string array.
        /// </summary>
        /// <param name="attributeNames">Array of attributes that map to bits.</param>
        /// <param name="attributeName">Name of attribute to check.</param>
        /// <param name="attributeValue">Value of attribute to check.</param>
        /// <param name="bits">The bit array in which the bit will be set if found.</param>
        /// <param name="offset">The offset into the bit array.</param>
        /// <returns>true if the bit was set; false otherwise.</returns>
        public static bool NameToBit(string[] attributeNames, string attributeName, YesNoType attributeValue, BitArray bits, int offset)
        {
            for (int i = 0; i < attributeNames.Length; i++)
            {
                if (attributeNames[i] == attributeName)
                {
                    if (YesNoType.Yes == attributeValue)
                    {
                        bits.Set(i + offset, true);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifies that a filename is ambiguous.
        /// </summary>
        /// <param name="filename">Filename to verify.</param>
        /// <returns>true if the filename is ambiguous; false otherwise.</returns>
        public static bool IsAmbiguousFilename(string filename)
        {
            if (null == filename || 0 == filename.Length)
            {
                return false;
            }

            return CompilerCore.AmbiguousFilename.IsMatch(filename);
        }

        /// <summary>
        /// Verifies that a value is a legal identifier.
        /// </summary>
        /// <param name="value">The value to verify.</param>
        /// <returns>true if the value is an identifier; false otherwise.</returns>
        public static bool IsIdentifier(string value)
        {
            if (null == value || 0 == value.Length)
            {
                return false;
            }

            return CompilerCore.LegalIdentifierCharacters.IsMatch(value);
        }

        /// <summary>
        /// Verifies if an identifier is a valid loc identifier.
        /// </summary>
        /// <param name="identifier">Identifier to verify.</param>
        /// <returns>True if the identifier is a valid loc identifier.</returns>
        public static bool IsValidLocIdentifier(string identifier)
        {
            if (null == identifier || 0 == identifier.Length)
            {
                return false;
            }

            Match match = Common.WixVariableRegex.Match(identifier);

            return (match.Success && "loc" == match.Groups["namespace"].Value && 0 == match.Index && identifier.Length == match.Length);
        }

        /// <summary>
        /// Verifies if a filename is a valid short filename.
        /// </summary>
        /// <param name="filename">Filename to verify.</param>
        /// <param name="allowWildcards">true if wildcards are allowed in the filename.</param>
        /// <returns>True if the filename is a valid short filename</returns>
        public static bool IsValidShortFilename(string filename, bool allowWildcards)
        {
            if (null == filename || 0 == filename.Length)
            {
                return false;
            }

            if (allowWildcards)
            {
                if (CompilerCore.LegalWildcardShortFilename.IsMatch(filename))
                {
                    bool foundPeriod = false;
                    int beforePeriod = 0;
                    int afterPeriod = 0;

                    // count the number of characters before and after the period
                    // '*' is not counted because it may represent zero characters
                    foreach (char character in filename)
                    {
                        if ('.' == character)
                        {
                            foundPeriod = true;
                        }
                        else if ('*' != character)
                        {
                            if (foundPeriod)
                            {
                                afterPeriod++;
                            }
                            else
                            {
                                beforePeriod++;
                            }
                        }
                    }

                    if (8 >= beforePeriod && 3 >= afterPeriod)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return CompilerCore.LegalShortFilename.IsMatch(filename);
            }
        }

        /// <summary>
        /// Verifies if a filename is a valid long filename.
        /// </summary>
        /// <param name="filename">Filename to verify.</param>
        /// <param name="allowWildcards">true if wildcards are allowed in the filename.</param>
        /// <returns>True if the filename is a valid long filename</returns>
        public static bool IsValidLongFilename(string filename, bool allowWildcards)
        {
            if (null == filename || 0 == filename.Length)
            {
                return false;
            }

            // check for a non-period character (all periods is not legal)
            bool nonPeriodFound = false;
            foreach (char character in filename)
            {
                if ('.' != character)
                {
                    nonPeriodFound = true;
                    break;
                }
            }

            if (allowWildcards)
            {
                return (nonPeriodFound && CompilerCore.LegalWildcardLongFilename.IsMatch(filename));
            }
            else
            {
                return (nonPeriodFound && CompilerCore.LegalLongFilename.IsMatch(filename));
            }
        }

        /// <summary>
        /// Generates a short file/directory name using an identifier and long file/directory name as input.
        /// </summary>
        /// <param name="longName">The long file/directory name.</param>
        /// <param name="keepExtension">The option to keep the extension on generated short names.</param>
        /// <param name="allowWildcards">true if wildcards are allowed in the filename.</param>
        /// <param name="args">Any additional information to include in the hash for the generated short name.</param>
        /// <returns>The generated 8.3-compliant short file/directory name.</returns>
        public static string GenerateShortName(string longName, bool keepExtension, bool allowWildcards, params string[] args)
        {
            Debug.Assert(null != longName);

            // canonicalize the long name if its not a localization identifier (they are case-sensitive)
            if (!CompilerCore.IsValidLocIdentifier(longName))
            {
                longName = longName.ToLower(CultureInfo.InvariantCulture);
            }

            // collect all the data
            ArrayList strings = new ArrayList();
            strings.Add(longName);
            strings.AddRange(args);

            // prepare for hashing
            string stringData = String.Join("|", (string[])strings.ToArray(typeof(string)));
            byte[] data = Encoding.Unicode.GetBytes(stringData);

            // hash the data
            byte[] hash;
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                hash = md5.ComputeHash(data);
            }

            // generate the short file/directory name without an extension
            StringBuilder shortName = new StringBuilder(Convert.ToBase64String(hash));
            shortName.Remove(8, shortName.Length - 8);
            shortName.Replace('/', '_');
            shortName.Replace('+', '-');

            if (keepExtension)
            {
                string extension = Path.GetExtension(longName);

                if (4 < extension.Length)
                {
                    extension = extension.Substring(0, 4);
                }

                shortName.Append(extension);

                // check the generated short name to ensure its still legal (the extension may not be legal)
                if (!CompilerCore.IsValidShortFilename(shortName.ToString(), allowWildcards))
                {
                    // remove the extension (by truncating the generated file name back to the generated characters)
                    shortName.Length -= extension.Length;
                }
            }

            return shortName.ToString().ToLower(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Verifies the given string is a valid product version.
        /// </summary>
        /// <param name="version">The product version to verify.</param>
        /// <returns>True if version is a valid product version</returns>
        public static bool IsValidProductVersion(Version version)
        {
            if (255 < version.Major || 255 < version.Minor || 65535 < version.Build)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verifies the given string is a valid module version.
        /// </summary>
        /// <param name="version">The module version to verify.</param>
        /// <returns>True if version is a valid module version</returns>
        public static bool IsValidModuleVersion(Version version)
        {
            if (65535 < version.Major || 65535 < version.Minor || 65535 < version.Build || 65535 < version.Revision)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get an node's inner text and trims any extra whitespace.
        /// </summary>
        /// <param name="node">The node with inner text to be trimmed.</param>
        /// <returns>The node's inner text trimmed.</returns>
        public static string GetTrimmedInnerText(XmlNode node)
        {
            string value = node.InnerText;
            if (0 < value.Length)
            {
                value = value.Trim();
            }

            return value;
        }

        /// <summary>
        /// Gets node's inner text and ensure's it is safe for use in a condition by trimming any extra whitespace.
        /// </summary>
        /// <param name="node">The node to ensure inner text is a condition.</param>
        /// <returns>The value converted into a safe condition.</returns>
        public static string GetConditionInnerText(XmlNode node)
        {
            string value = node.InnerText;
            if (0 < value.Length)
            {
                value = value.Trim();
                value = value.Replace('\t', ' ');
                value = value.Replace('\r', ' ');
                value = value.Replace('\n', ' ');
            }
            else // return null for a non-existant condition
            {
                value = null;
            }

            return value;
        }

        /// <summary>
        /// Creates a row in the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Source and line number of current row.</param>
        /// <param name="tableName">Name of table to create row in.</param>
        /// <returns>New row.</returns>
        public Row CreateRow(SourceLineNumberCollection sourceLineNumbers, string tableName)
        {
            TableDefinition tableDefinition = this.tableDefinitions[tableName];
            Table table = this.activeSection.Tables.EnsureTable(this.activeSection, tableDefinition);

            return table.CreateRow(sourceLineNumbers);
        }

        /// <summary>
        /// Adds a patch resource reference to the list of resoures to be filtered when producing a patch. This method should only be used when processing children of a patch family.
        /// </summary>
        /// <param name="sourceLineNumbers">Source and line number of current row.</param>
        /// <param name="tableName">Name of table to create row in.</param>
        /// <param name="primaryKey">Array of keys that make up the primary key of the table.</param>
        /// <returns>New row.</returns>
        public void AddPatchFamilyChildReference(SourceLineNumberCollection sourceLineNumbers, string tableName, string[] primaryKey)
        {
            string joinedPrimaryKey = String.Join("/", primaryKey);
            Row patchReferenceRow = this.CreateRow(sourceLineNumbers, "WixPatchRef");
            patchReferenceRow[0] = tableName;
            patchReferenceRow[1] = joinedPrimaryKey;
        }

        /// <summary>
        /// Creates a Registry row in the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Source and line number of the current row.</param>
        /// <param name="root">The registry entry root.</param>
        /// <param name="key">The registry entry key.</param>
        /// <param name="name">The registry entry name.</param>
        /// <param name="value">The registry entry value.</param>
        /// <param name="componentId">The component which will control installation/uninstallation of the registry entry.</param>
        /// <param name="escapeLeadingHash">If true, "escape" leading '#' characters so the value is written as a REG_SZ.</param>
        public void CreateRegistryRow(SourceLineNumberCollection sourceLineNumbers, int root, string key, string name, string value, string componentId, bool escapeLeadingHash)
        {
            if (!this.EncounteredError)
            {
                if (-1 > root || 3 < root || null == key || null == componentId)
                {
                    throw new ArgumentException("Illegal arguments passed.");
                }

                // escape the leading '#' character for string registry values
                if (escapeLeadingHash && null != value && value.StartsWith("#"))
                {
                    value = String.Concat("#", value);
                }

                Row row = this.CreateRow(sourceLineNumbers, "Registry");
                row[0] = GenerateIdentifier("Registry", componentId, root.ToString(CultureInfo.InvariantCulture.NumberFormat), key.ToLower(CultureInfo.InvariantCulture), (null != name ? name.ToLower(CultureInfo.InvariantCulture) : name));
                row[1] = root;
                row[2] = key;
                row[3] = name;
                row[4] = value;
                row[5] = componentId;
            }
        }

        /// <summary>
        /// Creates a Registry row in the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Source and line number of the current row.</param>
        /// <param name="root">The registry entry root.</param>
        /// <param name="key">The registry entry key.</param>
        /// <param name="name">The registry entry name.</param>
        /// <param name="value">The registry entry value.</param>
        /// <param name="componentId">The component which will control installation/uninstallation of the registry entry.</param>
        public void CreateRegistryRow(SourceLineNumberCollection sourceLineNumbers, int root, string key, string name, string value, string componentId)
        {
            this.CreateRegistryRow(sourceLineNumbers, root, key, name, value, componentId, true);
        }

        /// <summary>
        /// Create a WixSimpleReference row in the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information for the row.</param>
        /// <param name="tableName">The table name of the simple reference.</param>
        /// <param name="primaryKeys">The primary keys of the simple reference.</param>
        public void CreateWixSimpleReferenceRow(SourceLineNumberCollection sourceLineNumbers, string tableName, params string[] primaryKeys)
        {
            if (!this.encounteredError)
            {
                WixSimpleReferenceRow wixSimpleReferenceRow = (WixSimpleReferenceRow)this.CreateRow(sourceLineNumbers, "WixSimpleReference");
                wixSimpleReferenceRow.TableName = tableName;
                wixSimpleReferenceRow.PrimaryKeys = String.Join("/", primaryKeys);
            }
        }

        /// <summary>
        /// Add the appropriate rows to make sure that the given table shows up
        /// in the resulting output.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line numbers.</param>
        /// <param name="tableName">Name of the table to ensure existance of.</param>
        public void EnsureTable(SourceLineNumberCollection sourceLineNumbers, string tableName)
        {
            if (!this.encounteredError)
            {
                Row row = this.CreateRow(sourceLineNumbers, "WixEnsureTable");
                row[0] = tableName;

                // We don't add custom table definitions to the tableDefinitions collection,
                // so if it's not in there, it better be a custom table. If the Id is just wrong,
                // instead of a custom table, we get an unresolved reference at link time.
                if (!this.tableDefinitions.Contains(tableName))
                {
                    this.CreateWixSimpleReferenceRow(sourceLineNumbers, "WixCustomTable", tableName);
                }
            }
        }

        /// <summary>
        /// Get an attribute value and displays an error if the value is empty.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>The attribute's value.</returns>
        public string GetAttributeValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            return this.GetAttributeValue(sourceLineNumbers, attribute, false);
        }

        /// <summary>
        /// Get an attribute value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="canBeEmpty">If true, no error is raised on empty value. If false, an error is raised.</param>
        /// <returns>The attribute's value.</returns>
        public string GetAttributeValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, bool canBeEmpty)
        {
            if (!canBeEmpty && String.Empty == attribute.Value)
            {
                this.OnMessage(WixErrors.IllegalEmptyAttributeValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name));

                return IllegalEmptyAttributeValue;
            }

            return attribute.Value;
        }

        /// <summary>
        /// Get an integer attribute value and displays an error for an illegal integer value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="minimum">The minimum legal value.</param>
        /// <param name="maximum">The maximum legal value.</param>
        /// <returns>The attribute's integer value or a special value if an error occurred during conversion.</returns>
        public int GetAttributeIntegerValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, int minimum, int maximum)
        {
            Debug.Assert(minimum > IntegerNotSet && minimum > IllegalInteger, "The legal values for this attribute collide with at least one sentinel used during parsing.");

            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                try
                {
                    int integer = Convert.ToInt32(value, CultureInfo.InvariantCulture.NumberFormat);

                    if (IntegerNotSet == integer || IllegalInteger == integer)
                    {
                        this.OnMessage(WixErrors.IntegralValueSentinelCollision(sourceLineNumbers, integer));
                    }
                    else if (minimum > integer || maximum < integer)
                    {
                        this.OnMessage(WixErrors.IntegralValueOutOfRange(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, integer, minimum, maximum));
                        integer = IllegalInteger;
                    }

                    return integer;
                }
                catch (FormatException)
                {
                    this.OnMessage(WixErrors.IllegalIntegerValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                catch (OverflowException)
                {
                    this.OnMessage(WixErrors.IllegalIntegerValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return IllegalInteger;
        }

        /// <summary>
        /// Get a long integral attribute value and displays an error for an illegal long value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="minimum">The minimum legal value.</param>
        /// <param name="maximum">The maximum legal value.</param>
        /// <returns>The attribute's long value or a special value if an error occurred during conversion.</returns>
        public long GetAttributeLongValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, long minimum, long maximum)
        {
            Debug.Assert(minimum > LongNotSet && minimum > IllegalLong, "The legal values for this attribute collide with at least one sentinel used during parsing.");

            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                try
                {
                    long longValue = Convert.ToInt64(value, CultureInfo.InvariantCulture.NumberFormat);

                    if (LongNotSet == longValue || IllegalLong == longValue)
                    {
                        this.OnMessage(WixErrors.IntegralValueSentinelCollision(sourceLineNumbers, longValue));
                    }
                    else if (minimum > longValue || maximum < longValue)
                    {
                        this.OnMessage(WixErrors.IntegralValueOutOfRange(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, longValue, minimum, maximum));
                        longValue = IllegalLong;
                    }

                    return longValue;
                }
                catch (FormatException)
                {
                    this.OnMessage(WixErrors.IllegalLongValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                catch (OverflowException)
                {
                    this.OnMessage(WixErrors.IllegalLongValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return IllegalLong;
        }

        /// <summary>
        /// Get a date time attribute value and display errors for illegal values.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>Int representation of the date time.</returns>
        public int GetAttributeDateTimeValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                try
                {
                    DateTime date = DateTime.Parse(value, CultureInfo.InvariantCulture.DateTimeFormat);

                    return ((((date.Year - 1980) * 512) + (date.Month * 32 + date.Day)) * 65536) +
                        (date.Hour * 2048) + (date.Minute * 32) + (date.Second / 2);
                }
                catch (ArgumentOutOfRangeException)
                {
                    this.OnMessage(WixErrors.InvalidDateTimeFormat(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                catch (FormatException)
                {
                    this.OnMessage(WixErrors.InvalidDateTimeFormat(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                catch (OverflowException)
                {
                    this.OnMessage(WixErrors.InvalidDateTimeFormat(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return IllegalInteger;
        }

        /// <summary>
        /// Get an integer attribute value or localize variable and displays an error for 
        /// an illegal value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="minimum">The minimum legal value.</param>
        /// <param name="maximum">The maximum legal value.</param>
        /// <returns>The attribute's integer value or localize variable as a string or a special value if an error occurred during conversion.</returns>
        public string GetAttributeLocalizableIntegerValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, int minimum, int maximum)
        {
            Debug.Assert(minimum > IntegerNotSet && minimum > IllegalInteger, "The legal values for this attribute collide with at least one sentinel used during parsing.");

            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                if (IsValidLocIdentifier(value))
                {
                    return value;
                }
                else
                {
                    try
                    {
                        int integer = Convert.ToInt32(value, CultureInfo.InvariantCulture.NumberFormat);

                        if (IntegerNotSet == integer || IllegalInteger == integer)
                        {
                            this.OnMessage(WixErrors.IntegralValueSentinelCollision(sourceLineNumbers, integer));
                        }
                        else if (minimum > integer || maximum < integer)
                        {
                            this.OnMessage(WixErrors.IntegralValueOutOfRange(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, integer, minimum, maximum));
                            integer = IllegalInteger;
                        }

                        return value;
                    }
                    catch (FormatException)
                    {
                        this.OnMessage(WixErrors.IllegalIntegerValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                    }
                    catch (OverflowException)
                    {
                        this.OnMessage(WixErrors.IllegalIntegerValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a guid attribute value and displays an error for an illegal guid value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="generatable">Determines whether the guid can be automatically generated.</param>
        /// <returns>The attribute's guid value or a special value if an error occurred.</returns>
        public string GetAttributeGuidValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, bool generatable)
        {
            return this.GetAttributeGuidValue(sourceLineNumbers, attribute, generatable, false);
        }

        /// <summary>
        /// Get a guid attribute value and displays an error for an illegal guid value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="generatable">Determines whether the guid can be automatically generated.</param>
        /// <param name="canBeEmpty">If true, no error is raised on empty value. If false, an error is raised.</param>
        /// <returns>The attribute's guid value or a special value if an error occurred.</returns>
        public string GetAttributeGuidValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, bool generatable, bool canBeEmpty)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute, canBeEmpty);

            if (String.Empty == value && canBeEmpty)
            {
                return value;
            }
            else if (IllegalEmptyAttributeValue != value)
            {
                // If the value starts and ends with braces or parenthesis, accept that and strip them off.
                if ((value.StartsWith("{") && value.EndsWith("}")) || (value.StartsWith("(") && value.EndsWith(")")))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                try
                {
                    Guid guid;

                    // question mark syntax is for backwards-compatibility until its removed
                    if (generatable && ("????????-????-????-????-????????????" == value || "*" == value))
                    {
                        return value;
                    }
                    else if ("PUT-GUID-HERE" == value)
                    {
                        this.OnMessage(WixErrors.ExampleGuid(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                        return IllegalGuid;
                    }
                    else
                    {
                        guid = new Guid(value);
                    }

                    string uppercaseGuid = guid.ToString().ToUpper(CultureInfo.InvariantCulture);

                    if (this.showPedanticMessages)
                    {
                        if (uppercaseGuid != value)
                        {
                            this.OnMessage(WixErrors.GuidContainsLowercaseLetters(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                        }
                    }

                    return String.Concat("{", uppercaseGuid, "}");
                }
                catch (FormatException)
                {
                    this.OnMessage(WixErrors.IllegalGuidValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return IllegalGuid;
        }

        /// <summary>
        /// Get an identifier attribute value and displays an error for an illegal identifier value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>The attribute's identifier value or a special value if an error occurred.</returns>
        public string GetAttributeIdentifierValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                if (LegalIdentifierCharacters.IsMatch(value))
                {
                    if (72 < value.Length)
                    {
                        this.OnMessage(WixWarnings.IdentifierTooLong(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                    }

                    return value;
                }
                else
                {
                    this.OnMessage(WixErrors.IllegalIdentifier(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return IllegalIdentifier;
        }

        /// <summary>
        /// Gets a yes/no value and displays an error for an illegal yes/no value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>The attribute's YesNoType value.</returns>
        public YesNoType GetAttributeYesNoValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                switch (value)
                {
                    case "no":
                        return YesNoType.No;
                    case "yes":
                        return YesNoType.Yes;
                    default:
                        this.OnMessage(WixErrors.IllegalYesNoValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                        break;
                }
            }

            return YesNoType.IllegalValue;
        }

        /// <summary>
        /// Gets a yes/no/default value and displays an error for an illegal yes/no value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>The attribute's YesNoDefaultType value.</returns>
        public YesNoDefaultType GetAttributeYesNoDefaultValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                switch (value)
                {
                    case "default":
                        return YesNoDefaultType.Default;
                    case "no":
                        return YesNoDefaultType.No;
                    case "yes":
                        return YesNoDefaultType.Yes;
                    default:
                        this.OnMessage(WixErrors.IllegalYesNoDefaultValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                        break;
                }
            }

            return YesNoDefaultType.IllegalValue;
        }

        /// <summary>
        /// Gets a short filename value and displays an error for an illegal short filename value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="allowWildcards">true if wildcards are allowed in the filename.</param>
        /// <returns>The attribute's short filename value.</returns>
        public string GetAttributeShortFilename(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, bool allowWildcards)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                if (!CompilerCore.IsValidShortFilename(value, allowWildcards) && !CompilerCore.IsValidLocIdentifier(value))
                {
                    this.OnMessage(WixErrors.IllegalShortFilename(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                else if (CompilerCore.IsAmbiguousFilename(value))
                {
                    this.OnMessage(WixWarnings.AmbiguousFileOrDirectoryName(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return value;
        }

        /// <summary>
        /// Gets a long filename value and displays an error for an illegal long filename value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <param name="allowWildcards">true if wildcards are allowed in the filename.</param>
        /// <returns>The attribute's long filename value.</returns>
        public string GetAttributeLongFilename(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute, bool allowWildcards)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                if (!CompilerCore.IsValidLongFilename(value, allowWildcards) && !CompilerCore.IsValidLocIdentifier(value))
                {
                    this.OnMessage(WixErrors.IllegalLongFilename(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                else if (CompilerCore.IsAmbiguousFilename(value))
                {
                    this.OnMessage(WixWarnings.AmbiguousFileOrDirectoryName(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return value;
        }

        /// <summary>
        /// Gets a version value and displays an error for an illegal version value.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The attribute containing the value to get.</param>
        /// <returns>The attribute's version value.</returns>
        public Version GetAttributeVersionValue(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            string value = this.GetAttributeValue(sourceLineNumbers, attribute);

            if (IllegalEmptyAttributeValue != value)
            {
                try
                {
                    return new Version(value);
                }
                catch (FormatException) // illegal integer in version
                {
                    this.OnMessage(WixErrors.IllegalVersionValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
                catch (ArgumentException)
                {
                    this.OnMessage(WixErrors.IllegalVersionValue(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name, value));
                }
            }

            return IllegalVersion;
        }

        /// <summary>
        /// Attempts to use an extension to parse the attribute.
        /// </summary>
        /// <param name="sourceLineNumbers">Current line number for element.</param>
        /// <param name="element">Element containing attribute to be parsed.</param>
        /// <param name="attribute">Attribute to be parsed.</param>
        public void ParseExtensionAttribute(SourceLineNumberCollection sourceLineNumbers, XmlElement element, XmlAttribute attribute)
        {
            // ignore elements defined by the W3C because we'll assume they are always right
            if (!attribute.NamespaceURI.StartsWith("http://www.w3.org/"))
            {
                CompilerExtension extension = this.FindExtension(attribute.NamespaceURI);

                if (null != extension)
                {
                    extension.ParseAttribute(sourceLineNumbers, element, attribute);
                }
                else
                {
                    this.OnMessage(WixErrors.UnhandledExtensionAttribute(sourceLineNumbers, element.Name, attribute.Name, attribute.NamespaceURI));
                }
            }
        }

        /// <summary>
        /// Attempts to use an extension to parse the element.
        /// </summary>
        /// <param name="sourceLineNumbers">Current line number for element.</param>
        /// <param name="parentElement">Element containing element to be parsed.</param>
        /// <param name="element">Element to be parsed.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public void ParseExtensionElement(SourceLineNumberCollection sourceLineNumbers, XmlElement parentElement, XmlElement element, params string[] contextValues)
        {
            CompilerExtension extension = this.FindExtension(element.NamespaceURI);

            if (null != extension)
            {
                extension.ParseElement(sourceLineNumbers, parentElement, element, contextValues);
            }
            else
            {
                SourceLineNumberCollection childSourceLineNumbers = Preprocessor.GetSourceLineNumbers(element);

                this.OnMessage(WixErrors.UnhandledExtensionElement(childSourceLineNumbers, parentElement.Name, element.Name, element.NamespaceURI));
            }
        }

        /// <summary>
        /// Displays an unexpected attribute error if the attribute is not
        /// the namespace attribute.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="attribute">The unexpected attribute.</param>
        public void UnexpectedAttribute(SourceLineNumberCollection sourceLineNumbers, XmlAttribute attribute)
        {
            this.OnMessage(WixErrors.UnexpectedAttribute(sourceLineNumbers, attribute.OwnerElement.Name, attribute.Name));
        }

        /// <summary>
        /// Display an unexepected element error.
        /// </summary>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="childElement">The unexpected child element.</param>
        public void UnexpectedElement(XmlNode parentElement, XmlNode childElement)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(childElement);

            this.OnMessage(WixErrors.UnexpectedElement(sourceLineNumbers, parentElement.Name, childElement.Name));
        }

        /// <summary>
        /// Display an unsupported extension attribute error.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information about the owner element.</param>
        /// <param name="extensionAttribute">The extension attribute.</param>
        public void UnsupportedExtensionAttribute(SourceLineNumberCollection sourceLineNumbers, XmlAttribute extensionAttribute)
        {
            // ignore elements defined by the W3C because we'll assume they are always right
            if (!extensionAttribute.NamespaceURI.StartsWith("http://www.w3.org/"))
            {
                this.OnMessage(WixErrors.UnsupportedExtensionAttribute(sourceLineNumbers, extensionAttribute.OwnerElement.Name, extensionAttribute.Name));
            }
        }

        /// <summary>
        /// Display an unsupported extension element error.
        /// </summary>
        /// <param name="parentElement">The parent element.</param>
        /// <param name="extensionElement">The extension element.</param>
        public void UnsupportedExtensionElement(XmlNode parentElement, XmlNode extensionElement)
        {
            SourceLineNumberCollection sourceLineNumbers = Preprocessor.GetSourceLineNumbers(extensionElement);

            this.OnMessage(WixErrors.UnsupportedExtensionElement(sourceLineNumbers, parentElement.Name, extensionElement.Name));
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != errorEventArgs)
            {
                this.encounteredError = true;
            }

            if (null != this.MessageHandler)
            {
                this.MessageHandler(this, mea);
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }

        /// <summary>
        /// Generate an identifier by hashing data from the row.
        /// </summary>
        /// <param name="tableName">Name of the table for which the identifier is being generated.</param>
        /// <param name="args">Information to hash.</param>
        /// <returns>The generated identifier.</returns>
        internal static string GenerateIdentifier(string tableName, params string[] args)
        {
            string stringData = String.Join("|", args);
            byte[] data = Encoding.Unicode.GetBytes(stringData);

            // hash the data
            byte[] hash;
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                hash = md5.ComputeHash(data);
            }

            // select a prefix based on the element localname
            string prefix = null;
            switch (tableName)
            {
                case "Registry":
                case "RemoveRegistry":
                    prefix = "reg";
                    break;
                default:
                    throw new InvalidOperationException("Invalid table name passed into GenerateIdentifier.");
            }
            Debug.Assert(3 >= prefix.Length, "Prefix for generated identifiers must be 3 characters long or less.");

            // build up the identifier
            StringBuilder identifier = new StringBuilder(35, 35);
            identifier.Append(prefix);
            for (int i = 0; i < hash.Length; i++)
            {
                identifier.Append(hash[i].ToString("X2", CultureInfo.InvariantCulture.NumberFormat));
            }

            return identifier.ToString();
        }

        /// <summary>
        /// Creates a new section and makes it the active section in the core.
        /// </summary>
        /// <param name="id">Unique identifier for the section.</param>
        /// <param name="type">Type of section to create.</param>
        /// <param name="codepage">Codepage for the resulting database for this ection.</param>
        /// <returns>New section.</returns>
        internal Section CreateActiveSection(string id, SectionType type, int codepage)
        {
            Section newSection = new Section(id, type, codepage);

            this.intermediate.Sections.Add(newSection);
            this.activeSection = newSection;

            return newSection;
        }

        /// <summary>
        /// Creates a WiX complex reference in the active section.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information.</param>
        /// <param name="parentType">The parent type.</param>
        /// <param name="parentId">The parent id.</param>
        /// <param name="parentLanguage">The parent language.</param>
        /// <param name="childType">The child type.</param>
        /// <param name="childId">The child id.</param>
        /// <param name="isPrimary">Whether the child is primary.</param>
        internal void CreateWixComplexReferenceRow(SourceLineNumberCollection sourceLineNumbers, ComplexReferenceParentType parentType, string parentId, string parentLanguage, ComplexReferenceChildType childType, string childId, bool isPrimary)
        {
            if (!this.encounteredError)
            {
                WixComplexReferenceRow wixComplexReferenceRow = (WixComplexReferenceRow)this.CreateRow(sourceLineNumbers, "WixComplexReference");
                wixComplexReferenceRow.ParentId = parentId;
                wixComplexReferenceRow.ParentType = parentType;
                wixComplexReferenceRow.ParentLanguage = parentLanguage;
                wixComplexReferenceRow.ChildId = childId;
                wixComplexReferenceRow.ChildType = childType;
                wixComplexReferenceRow.IsPrimary = isPrimary;
            }
        }

        /// <summary>
        /// Finds a compiler extension by namespace URI.
        /// </summary>
        /// <param name="namespaceUri">URI for namespace the extension supports.</param>
        /// <returns>Found compiler extension or null if nothing matches namespace URI.</returns>
        private CompilerExtension FindExtension(string namespaceUri)
        {
            return (CompilerExtension)this.extensions[namespaceUri];
        }
    }
}
