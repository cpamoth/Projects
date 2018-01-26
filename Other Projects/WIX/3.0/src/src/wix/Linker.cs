//-------------------------------------------------------------------------------------------------
// <copyright file="Linker.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Linker core of the Windows Installer Xml toolset.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using Microsoft.Tools.WindowsInstallerXml.Msi;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Linker core of the Windows Installer Xml toolset.
    /// </summary>
    public sealed class Linker : IMessageHandler
    {
        private static readonly char[] colonCharacter = ":".ToCharArray();
        private static readonly string emptyGuid = Guid.Empty.ToString("B");

        private string[] cultures;
        private WixActionRowCollection standardActions;
        private bool encounteredError;
        private bool allowIdenticalRows;
        private bool allowUnresolvedReferences;
        private ArrayList extensions;
        private string unreferencedSymbolsFile;
        private bool sectionIdOnRows;
        private bool generateSectionIds;
        private bool showPedanticMessages;
        private bool suppressAdminSequence;
        private bool suppressAdvertiseSequence;
        private bool suppressDroppingUnrealTables;
        private bool suppressMsiAssemblyTable;
        private bool suppressUISequence;
        private Localizer localizer;
        private Output activeOutput;
        private string imagebaseOutputPath;
        private TableDefinitionCollection tableDefinitions;
        private WixVariableResolver wixVariableResolver;

        /// <summary>
        /// Creates a linker.
        /// </summary>
        public Linker()
        {
            this.standardActions = Installer.GetStandardActions();
            this.tableDefinitions = Installer.GetTableDefinitions();

            this.extensions = new ArrayList();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets or sets the cultures to load from libraries in the extensions.
        /// </summary>
        /// <value>The cultures to load from libraries in the extensions.</value>
        public string[] Cultures
        {
            get { return this.cultures; }
            set { this.cultures = value; }
        }

        /// <summary>
        /// Gets or sets the base path for output image.
        /// </summary>
        /// <value>Base path for output image.</value>
        public string ImageBaseOutputPath
        {
            get { return this.imagebaseOutputPath; }
            set { this.imagebaseOutputPath = value; }
        }

        /// <summary>
        /// Gets or sets the flag specifying if identical rows are allowed during linking.
        /// </summary>
        /// <value>True if identical rows are allowed.</value>
        public bool AllowIdenticalRows
        {
            get { return this.allowIdenticalRows; }
            set { this.allowIdenticalRows = value; }
        }

        /// <summary>
        /// Gets or sets the flag specifying if unresolved references are allowed during linking.
        /// </summary>
        /// <value>True if unresolved references are allowed.</value>
        public bool AllowUnresolvedReferences
        {
            get { return this.allowUnresolvedReferences; }
            set { this.allowUnresolvedReferences = value; }
        }

        /// <summary>
        /// Gets or sets the localizer.
        /// </summary>
        /// <value>The localizer.</value>
        public Localizer Localizer
        {
            get { return this.localizer; }
            set { this.localizer = value; }
        }
        /// <summary>
        /// Gets or sets the path to output unreferenced symbols to. If null or empty, there is no output.
        /// </summary>
        /// <value>The path to output the xml file.</value>
        public string UnreferencedSymbolsFile
        {
            get { return this.unreferencedSymbolsFile; }
            set { this.unreferencedSymbolsFile = value; }
        }

        /// <summary>
        /// Turns on or off generating section ids where none were authored.
        /// </summary>
        /// <value>True if section ids should be generated.</value>
        public bool GenerateSectionIds
        {
            get { return this.generateSectionIds; }
            set { this.generateSectionIds = value; }
        }

        /// <summary>
        /// Turns on or off tagging the rows with the sectionId attribute in the output xml.
        /// </summary>
        /// <value>True if rows should be tagged.</value>
        public bool SectionIdOnRows
        {
            get { return this.sectionIdOnRows; }
            set { this.sectionIdOnRows = value; }
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
        /// Sets the option to suppress admin sequence actions.
        /// </summary>
        /// <value>The option to suppress admin sequence actions.</value>
        public bool SuppressAdminSequence
        {
            get { return this.suppressAdminSequence; }
            set { this.suppressAdminSequence = value; }
        }

        /// <summary>
        /// Sets the option to suppress advertise sequence actions.
        /// </summary>
        /// <value>The option to suppress advertise sequence actions.</value>
        public bool SuppressAdvertiseSequence
        {
            get { return this.suppressAdvertiseSequence; }
            set { this.suppressAdvertiseSequence = value; }
        }

        /// <summary>
        /// Writes unreal tables to the output image.
        /// </summary>
        /// <value>The option to supress dropping unreal tables from the output image.</value>
        /// <remarks>This will not affect the handling of "special" tables; those
        /// tables are specifically handled.</remarks>
        public bool SuppressDroppingUnrealTables
        {
            get { return this.suppressDroppingUnrealTables; }
            set { this.suppressDroppingUnrealTables = value; }
        }

        /// <summary>
        /// Sets the option to suppress the MsiAssembly table.
        /// </summary>
        /// <value>The option to supress processing the MsiAssembly table.</value>
        public bool SuppressMsiAssemblyTable
        {
            get { return this.suppressMsiAssemblyTable; }
            set { this.suppressMsiAssemblyTable = value; }
        }

        /// <summary>
        /// Sets the option to suppress UI sequence actions.
        /// </summary>
        /// <value>The option to suppress UI sequence actions.</value>
        public bool SuppressUISequence
        {
            get { return this.suppressUISequence; }
            set { this.suppressUISequence = value; }
        }

        /// <summary>
        /// Gets the table definitions used by the linker.
        /// </summary>
        /// <value>Table definitions used by the linker.</value>
        public TableDefinitionCollection TableDefinitions
        {
            get { return this.tableDefinitions; }
        }

        /// <summary>
        /// Gets or sets the Wix variable resolver.
        /// </summary>
        /// <value>The Wix variable resolver.</value>
        public WixVariableResolver WixVariableResolver
        {
            get { return this.wixVariableResolver; }
            set { this.wixVariableResolver = value; }
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void AddExtension(WixExtension extension)
        {
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

            // keep track of extensions so the libraries can be loaded later once all the table definitions
            // are loaded; this will allow extensions to have cross table definition dependencies
            this.extensions.Add(extension);
        }

        /// <summary>
        /// Links a collection of sections into an output.
        /// </summary>
        /// <param name="sections">The collection of sections to link together.</param>
        /// <returns>Output object from the linking.</returns>
        public Output Link(SectionCollection sections)
        {
            return this.Link(sections, null);
        }

        /// <summary>
        /// Links a collection of sections into an output.
        /// </summary>
        /// <param name="sections">The collection of sections to link together.</param>
        /// <param name="transforms">The collection of transforms to link as substorages.</param>
        /// <returns>Output object from the linking.</returns>
        public Output Link(SectionCollection sections, ArrayList transforms)
        {
            Output output = null;

            try
            {
                SymbolCollection allSymbols;
                Section entrySection;
                bool containsModuleSubstitution = false;
                bool containsModuleConfiguration = false;

                StringCollection referencedSymbols = new StringCollection();
                ArrayList unresolvedReferences = new ArrayList();

                ConnectToFeatureCollection componentsToFeatures = new ConnectToFeatureCollection();
                ConnectToFeatureCollection featuresToFeatures = new ConnectToFeatureCollection();
                ConnectToFeatureCollection modulesToFeatures = new ConnectToFeatureCollection();

                this.activeOutput = null;
                this.encounteredError = false;

                SortedList adminProperties = new SortedList();
                SortedList secureProperties = new SortedList();
                SortedList hiddenProperties = new SortedList();

                RowCollection actionRows = new RowCollection();
                RowCollection suppressActionRows = new RowCollection();

                TableDefinitionCollection customTableDefinitions = new TableDefinitionCollection();
                RowCollection customRows = new RowCollection();

                StringCollection generatedShortFileNameIdentifiers = new StringCollection();
                Hashtable generatedShortFileNames = new Hashtable();

                Hashtable multipleFeatureComponents = new Hashtable();

                Hashtable wixVariables = new Hashtable();

                // verify that modularization types match for foreign key relationships
                foreach (TableDefinition tableDefinition in this.tableDefinitions)
                {
                    foreach (ColumnDefinition columnDefinition in tableDefinition.Columns)
                    {
                        if (null != columnDefinition.KeyTable && 0 > columnDefinition.KeyTable.IndexOf(';') && columnDefinition.IsKeyColumnSet)
                        {
                            try
                            {
                                TableDefinition keyTableDefinition = this.tableDefinitions[columnDefinition.KeyTable];

                                if (0 >= columnDefinition.KeyColumn || keyTableDefinition.Columns.Count < columnDefinition.KeyColumn)
                                {
                                    this.OnMessage(WixErrors.InvalidKeyColumn(tableDefinition.Name, columnDefinition.Name, columnDefinition.KeyTable, columnDefinition.KeyColumn));
                                }
                                else if (keyTableDefinition.Columns[columnDefinition.KeyColumn - 1].ModularizeType != columnDefinition.ModularizeType && ColumnModularizeType.CompanionFile != columnDefinition.ModularizeType)
                                {
                                    this.OnMessage(WixErrors.CollidingModularizationTypes(tableDefinition.Name, columnDefinition.Name, columnDefinition.KeyTable, columnDefinition.KeyColumn, columnDefinition.ModularizeType.ToString(), keyTableDefinition.Columns[columnDefinition.KeyColumn - 1].ModularizeType.ToString()));
                                }
                            }
                            catch (WixMissingTableDefinitionException)
                            {
                                // ignore missing table definitions - this error is caught in other places
                            }
                        }
                    }
                }

                // add in the extension sections
                foreach (WixExtension extension in this.extensions)
                {
                    Library library = extension.GetLibrary(this.tableDefinitions);

                    if (null != library)
                    {
                        sections.AddRange(library.Sections);

                        // load the localization files for the selected cultures
                        if (null != this.cultures && null != this.localizer)
                        {
                            Localization localization = library.GetLocalization(this.cultures);

                            if (null != localization)
                            {
                                this.localizer.AddLocalization(localization);
                            }
                        }
                    }
                }

                // first find the entry section and create the symbols hash for all the sections
                sections.FindEntrySectionAndLoadSymbols(this.allowIdenticalRows, this, out entrySection, out allSymbols);

                // should have found an entry section by now
                if (null == entrySection)
                {
                    throw new WixException(WixErrors.MissingEntrySection());
                }

                // add the missing standard action symbols
                this.LoadStandardActionSymbols(allSymbols);

                // now that we know where we're starting from, create the output object
                output = new Output(null);
                output.EntrySection = entrySection; // Note: this entry section will get added to the Output.Sections collection later
                if (null != this.localizer && -1 != this.localizer.Codepage)
                {
                    output.Codepage = this.localizer.Codepage;
                }
                this.activeOutput = output;

                // Resolve the symbol references to find the set of sections we
                // care about for linking.  Of course, we start with the entry 
                // section (that's how it got its name after all).
                output.Sections.AddRange(output.EntrySection.ResolveReferences(output.Type, allSymbols, referencedSymbols, unresolvedReferences, this));

                // Flattening the complex references that participate in groups.
                this.FlattenSectionsComplexReferences(output.Sections);

                if (this.encounteredError)
                {
                    return null;
                }

                // The hard part in linking is processing the complex references.
                this.ProcessComplexReferences(output, output.Sections, referencedSymbols, componentsToFeatures, featuresToFeatures, modulesToFeatures);
                for (int i = 0; i < unresolvedReferences.Count; ++i)
                {
                    Section.SimpleReferenceSection referenceSection = (Section.SimpleReferenceSection)unresolvedReferences[i];
                    if (this.allowUnresolvedReferences)
                    {
                        this.OnMessage(WixWarnings.UnresolvedReferenceWarning(referenceSection.WixSimpleReferenceRow.SourceLineNumbers, referenceSection.Section.Type.ToString(), referenceSection.Section.Id, referenceSection.WixSimpleReferenceRow.SymbolicName));
                    }
                    else
                    {
                        this.OnMessage(WixErrors.UnresolvedReference(referenceSection.WixSimpleReferenceRow.SourceLineNumbers, referenceSection.Section.Type.ToString(), referenceSection.Section.Id, referenceSection.WixSimpleReferenceRow.SymbolicName));
                    }
                }

                if (this.encounteredError)
                {
                    return null;
                }

                SymbolCollection unreferencedSymbols = output.Sections.GetOrphanedSymbols(referencedSymbols, this);
                if (null != this.unreferencedSymbolsFile)
                {
                    unreferencedSymbols.OutputSymbols(this.unreferencedSymbolsFile);
                }

                // Display a warning message for Components that were never referenced by a Feature.
                foreach (Symbol symbol in unreferencedSymbols)
                {
                    if ("Component" == symbol.Row.Table.Name)
                    {
                        this.OnMessage(WixWarnings.OrphanedComponent(symbol.Row.SourceLineNumbers, (string)symbol.Row[0]));
                    }
                }

                // resolve the feature to feature connects
                this.ResolveFeatureToFeatureConnects(featuresToFeatures, allSymbols);

                // start generating OutputTables and OutputRows for all the sections in the output
                RowCollection ensureTableRows = new RowCollection();
                int sectionCount = 0;
                foreach (Section section in output.Sections)
                {
                    sectionCount++;
                    string sectionId = section.Id;
                    if (null == sectionId && this.generateSectionIds)
                    {
                        sectionId = "wix.section." + sectionCount.ToString();
                    }

                    foreach (Table table in section.Tables)
                    {
                        // by default, copy rows if the table is not unreal
                        // or if user requests suppression of droppng unreal tables from the output
                        bool copyRows = this.suppressDroppingUnrealTables ? true : !table.Definition.IsUnreal;
                        
                        // handle special tables
                        switch (table.Name)
                        {
                            case "AppSearch":
                                this.activeOutput.EnsureTable(this.tableDefinitions["Signature"]);
                                break;

                            case "Class":
                                if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 2, 11, componentsToFeatures, multipleFeatureComponents);
                                }
                                break;

                            case "CustomAction":
                                if (OutputType.Module == this.activeOutput.Type)
                                {
                                    this.activeOutput.EnsureTable(this.tableDefinitions["AdminExecuteSequence"]);
                                    this.activeOutput.EnsureTable(this.tableDefinitions["AdminUISequence"]);
                                    this.activeOutput.EnsureTable(this.tableDefinitions["AdvtExecuteSequence"]);
                                    this.activeOutput.EnsureTable(this.tableDefinitions["InstallExecuteSequence"]);
                                    this.activeOutput.EnsureTable(this.tableDefinitions["InstallUISequence"]);
                                }
                                break;

                            case "Dialog":
                                this.activeOutput.EnsureTable(this.tableDefinitions["ListBox"]);
                                break;

                            case "Directory":
                                foreach (Row row in table.Rows)
                                {
                                    if (OutputType.Module == this.activeOutput.Type && Installer.IsStandardDirectory(row[0].ToString()))
                                    {
                                        // if the directory table contains references to standard windows folders
                                        // mergemod.dll will add customactions to set the MSM directory to 
                                        // the same directory as the standard windows folder and will add references to 
                                        // custom action to all the standard sequence tables.  A problem will occur
                                        // if the MSI does not have these tables as mergemod.dll does not add these
                                        // tables to the MSI if absent.  This code adds the tables in case mergemod.dll
                                        // needs them.
                                        this.activeOutput.EnsureTable(this.tableDefinitions["CustomAction"]);
                                        this.activeOutput.EnsureTable(this.tableDefinitions["AdminExecuteSequence"]);
                                        this.activeOutput.EnsureTable(this.tableDefinitions["AdminUISequence"]);
                                        this.activeOutput.EnsureTable(this.tableDefinitions["AdvtExecuteSequence"]);
                                        this.activeOutput.EnsureTable(this.tableDefinitions["InstallExecuteSequence"]);
                                        this.activeOutput.EnsureTable(this.tableDefinitions["InstallUISequence"]);
                                        break; // no need to look here any more, we already found all that we needed to
                                    }
                                }
                                break;

                            case "Extension":
                                if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 1, 4, componentsToFeatures, multipleFeatureComponents);
                                }
                                break;

                            case "ModuleSubstitution":
                                containsModuleSubstitution = true;
                                break;

                            case "ModuleConfiguration":
                                containsModuleConfiguration = true;
                                break;

                            case "MsiAssembly":
                                if (this.suppressMsiAssemblyTable)
                                {
                                    copyRows = false;
                                }
                                else if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 0, 1, componentsToFeatures, multipleFeatureComponents);
                                }
                                break;

                            case "ProgId":
                                // the Extension table is required with a ProgId table
                                this.activeOutput.EnsureTable(this.tableDefinitions["Extension"]);
                                break;

                            case "Property":
                                for (int i = 0; i < table.Rows.Count; i++)
                                {
                                    if (null == table.Rows[i][1])
                                    {
                                        table.Rows.RemoveAt(i);
                                        i--;
                                    }
                                }
                                break;

                            case "PublishComponent":
                                if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 2, 4, componentsToFeatures, multipleFeatureComponents);
                                }
                                break;

                            case "Shortcut":
                                if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 3, 4, componentsToFeatures, multipleFeatureComponents);
                                }
                                break;

                            case "TypeLib":
                                if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 2, 6, componentsToFeatures, multipleFeatureComponents);
                                }
                                break;

                            case "Upgrade":
                                foreach (UpgradeRow row in table.Rows)
                                {
                                    secureProperties[row.ActionProperty] = null;
                                }
                                break;

                            case "WixAction":
                                if (this.sectionIdOnRows)
                                {
                                    foreach (Row row in table.Rows)
                                    {
                                        row.SectionId = sectionId;
                                    }
                                }
                                actionRows.AddRange(table.Rows);
                                break;

                            case "WixBBControl":
                            case "WixControl":
                                copyRows = true;
                                break;

                            case "WixCustomTable":
                                foreach (Row row in table.Rows)
                                {
                                    TableDefinition customTable = new TableDefinition((string)row[0], false, false);

                                    if (null == row[4])
                                    {
                                        this.OnMessage(WixErrors.ExpectedAttribute(row.SourceLineNumbers, "CustomTable/Column", "PrimaryKey"));
                                    }

                                    string[] columnNames = row[2].ToString().Split('\t');
                                    string[] columnTypes = row[3].ToString().Split('\t');
                                    string[] primaryKeys = row[4].ToString().Split('\t');
                                    string[] minValues = row[5] == null ? null : row[5].ToString().Split('\t');
                                    string[] maxValues = row[6] == null ? null : row[6].ToString().Split('\t');
                                    string[] keyTables = row[7] == null ? null : row[7].ToString().Split('\t');
                                    string[] keyColumns = row[8] == null ? null : row[8].ToString().Split('\t');
                                    string[] categories = row[9] == null ? null : row[9].ToString().Split('\t');
                                    string[] sets = row[10] == null ? null : row[10].ToString().Split('\t');
                                    string[] descriptions = row[11] == null ? null : row[11].ToString().Split('\t');
                                    string[] modularizations = row[12] == null ? null : row[12].ToString().Split('\t');

                                    int currentPrimaryKey = 0;

                                    for (int i = 0; i < columnNames.Length; ++i)
                                    {
                                        string name = columnNames[i];
                                        ColumnType type = ColumnType.Unknown;
                                        switch (columnTypes[i].Substring(0, 1).ToLower(CultureInfo.InvariantCulture))
                                        {
                                            case "s":
                                                type = ColumnType.String;
                                                break;
                                            case "l":
                                                type = ColumnType.Localized;
                                                break;
                                            case "i":
                                                type = ColumnType.Number;
                                                break;
                                            case "v":
                                                type = ColumnType.Object;
                                                break;
                                            default:
                                                throw new WixException(WixErrors.UnknownCustomTableColumnType(row.SourceLineNumbers, columnTypes[i]));
                                        }
                                        bool nullable = columnTypes[i].Substring(0, 1) == columnTypes[i].Substring(0, 1).ToUpper(CultureInfo.InvariantCulture);
                                        int length = Convert.ToInt32(columnTypes[i].Substring(1), CultureInfo.InvariantCulture);

                                        bool primaryKey = false;
                                        if (currentPrimaryKey < primaryKeys.Length && primaryKeys[currentPrimaryKey] == columnNames[i])
                                        {
                                            primaryKey = true;
                                            currentPrimaryKey++;
                                        }

                                        bool minValSet = null != minValues && null != minValues[i] && 0 < minValues[i].Length;
                                        int minValue = 0;
                                        if (minValSet)
                                        {
                                            minValue = Convert.ToInt32(minValues[i], CultureInfo.InvariantCulture);
                                        }

                                        bool maxValSet = null != maxValues && null != maxValues[i] && 0 < maxValues[i].Length;
                                        int maxValue = 0;
                                        if (maxValSet)
                                        {
                                            maxValue = Convert.ToInt32(maxValues[i], CultureInfo.InvariantCulture);
                                        }

                                        bool keyColumnSet = null != keyColumns && null != keyColumns[i] && 0 < keyColumns[i].Length;
                                        int keyColumn = 0;
                                        if (keyColumnSet)
                                        {
                                            keyColumn = Convert.ToInt32(keyColumns[i], CultureInfo.InvariantCulture);
                                        }

                                        ColumnCategory category = ColumnCategory.Unknown;
                                        if (null != categories && null != categories[i] && 0 < categories[i].Length)
                                        {
                                            switch (categories[i])
                                            {
                                                case "Text":
                                                    category = ColumnCategory.Text;
                                                    break;
                                                case "UpperCase":
                                                    category = ColumnCategory.UpperCase;
                                                    break;
                                                case "LowerCase":
                                                    category = ColumnCategory.LowerCase;
                                                    break;
                                                case "Integer":
                                                    category = ColumnCategory.Integer;
                                                    break;
                                                case "DoubleInteger":
                                                    category = ColumnCategory.DoubleInteger;
                                                    break;
                                                case "TimeDate":
                                                    category = ColumnCategory.TimeDate;
                                                    break;
                                                case "Identifier":
                                                    category = ColumnCategory.Identifier;
                                                    break;
                                                case "Property":
                                                    category = ColumnCategory.Property;
                                                    break;
                                                case "Filename":
                                                    category = ColumnCategory.Filename;
                                                    break;
                                                case "WildCardFilename":
                                                    category = ColumnCategory.WildCardFilename;
                                                    break;
                                                case "Path":
                                                    category = ColumnCategory.Path;
                                                    break;
                                                case "Paths":
                                                    category = ColumnCategory.Paths;
                                                    break;
                                                case "AnyPath":
                                                    category = ColumnCategory.AnyPath;
                                                    break;
                                                case "DefaultDir":
                                                    category = ColumnCategory.DefaultDir;
                                                    break;
                                                case "RegPath":
                                                    category = ColumnCategory.RegPath;
                                                    break;
                                                case "Formatted":
                                                    category = ColumnCategory.Formatted;
                                                    break;
                                                case "Template":
                                                    category = ColumnCategory.Template;
                                                    break;
                                                case "Condition":
                                                    category = ColumnCategory.Condition;
                                                    break;
                                                case "Guid":
                                                    category = ColumnCategory.Guid;
                                                    break;
                                                case "Version":
                                                    category = ColumnCategory.Version;
                                                    break;
                                                case "Language":
                                                    category = ColumnCategory.Language;
                                                    break;
                                                case "Binary":
                                                    category = ColumnCategory.Binary;
                                                    break;
                                                case "CustomSource":
                                                    category = ColumnCategory.CustomSource;
                                                    break;
                                                case "Cabinet":
                                                    category = ColumnCategory.Cabinet;
                                                    break;
                                                case "Shortcut":
                                                    category = ColumnCategory.Shortcut;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }

                                        string keyTable = keyTables != null ? keyTables[i] : null;
                                        string setValue = sets != null ? sets[i] : null;
                                        string description = descriptions != null ? descriptions[i] : null;
                                        string modString = modularizations != null ? modularizations[i] : null;
                                        ColumnModularizeType modularization = ColumnModularizeType.None;
                                        if (modString != null)
                                        {
                                            switch (modString)
                                            {
                                                case "None":
                                                    modularization = ColumnModularizeType.None;
                                                    break;
                                                case "Column":
                                                    modularization = ColumnModularizeType.Column;
                                                    break;
                                                case "Property":
                                                    modularization = ColumnModularizeType.Property;
                                                    break;
                                                case "Condition":
                                                    modularization = ColumnModularizeType.Condition;
                                                    break;
                                                case "CompanionFile":
                                                    modularization = ColumnModularizeType.CompanionFile;
                                                    break;
                                                case "SemicolonDelimited":
                                                    modularization = ColumnModularizeType.SemicolonDelimited;
                                                    break;
                                            }
                                        }

                                        ColumnDefinition columnDefinition = new ColumnDefinition(name, type, length, primaryKey, nullable, modularization, ColumnType.Localized == type, minValSet, minValue, maxValSet, maxValue, keyTable, keyColumnSet, keyColumn, category, setValue, description, true, true);
                                        customTable.Columns.Add(columnDefinition);
                                    }

                                    customTableDefinitions.Add(customTable);
                                }

                                copyRows = false; // we've created table definitions from these rows, no need to process them any longer
                                break;

                            case "WixCustomRow":
                                customRows.AddRange(table.Rows);
                                copyRows = false;
                                break;

                            case "WixEnsureTable":
                                ensureTableRows.AddRange(table.Rows);
                                break;

                            case "WixFile":
                                foreach (Row row in table.Rows)
                                {
                                    // DiskId is not valid when creating a module, so set it to
                                    // 0 for all files to ensure proper sorting in the binder
                                    if (OutputType.Module == this.activeOutput.Type)
                                    {
                                        row[5] = 0;
                                    }

                                    // if the short file name was generated, check for collisions
                                    if (0x1 == (int)row[9])
                                    {
                                        generatedShortFileNameIdentifiers.Add((string)row[0]);
                                    }
                                }
                                copyRows = true;
                                break;

                            case "WixFragment":
                                copyRows = true;
                                break;

                            case "WixMedia":
                                copyRows = true;
                                break;

                            case "WixMerge":
                                if (OutputType.Product == output.Type)
                                {
                                    this.ResolveFeatures(table.Rows, 0, 7, modulesToFeatures, null);
                                }
                                copyRows = true;
                                break;

                            case "WixProperty":
                                foreach (WixPropertyRow wixPropertyRow in table.Rows)
                                {
                                    if (wixPropertyRow.Admin)
                                    {
                                        adminProperties[wixPropertyRow.Id] = null;
                                    }

                                    if (wixPropertyRow.Hidden)
                                    {
                                        hiddenProperties[wixPropertyRow.Id] = null;
                                    }

                                    if (wixPropertyRow.Secure)
                                    {
                                        secureProperties[wixPropertyRow.Id] = null;
                                    }
                                }
                                break;

                            case "WixSuppressAction":
                                suppressActionRows.AddRange(table.Rows);
                                break;

                            case "WixSuppressModularization":
                                // just copy the rows to the output
                                copyRows = true;
                                break;

                            case "WixVariable":
                                // check for colliding values and collect the wix variable rows
                                foreach (WixVariableRow row in table.Rows)
                                {
                                    WixVariableRow collidingRow = (WixVariableRow)wixVariables[row.Id];

                                    if (null == collidingRow || (collidingRow.Overridable && !row.Overridable))
                                    {
                                        wixVariables[row.Id] = row;
                                    }
                                    else if (!row.Overridable || (collidingRow.Overridable && row.Overridable))
                                    {
                                        this.OnMessage(WixErrors.WixVariableCollision(row.SourceLineNumbers, row.Id));
                                    }
                                }
                                break;
                            case "WixPatchRef":
                            case "WixPatchBaseline":
                            case "WixPatchId":
                                copyRows = true;
                                break;
                        }

                        if (copyRows)
                        {
                            Table outputTable = this.activeOutput.EnsureTable(this.tableDefinitions[table.Name]);
                            this.CopyTableRowsToOutputTable(table, outputTable, sectionId);
                        }
                    }
                }

                // Verify that there were no duplicate fragment Id's.
                Table wixFragmentTable = this.activeOutput.Tables["WixFragment"];
                Hashtable fragmentIdIndex = new Hashtable();
                if (null != wixFragmentTable)
                {
                    foreach (Row row in wixFragmentTable.Rows)
                    {
                        string fragmentId = row.Fields[0].Data.ToString();
                        if (!fragmentIdIndex.ContainsKey(fragmentId))
                        {
                            fragmentIdIndex.Add(fragmentId, row.SourceLineNumbers);
                        }
                        else
                        {
                            this.OnMessage(WixErrors.DuplicateSymbol(row.SourceLineNumbers, fragmentId));
                            if (null != fragmentIdIndex[fragmentId])
                            {
                                this.OnMessage(WixErrors.DuplicateSymbol2((SourceLineNumberCollection)fragmentIdIndex[fragmentId]));
                            }
                        }
                    }
                }

                // copy the module to feature connections into the output
                if (0 < modulesToFeatures.Count)
                {
                    Table wixFeatureModulesTable = this.activeOutput.EnsureTable(this.tableDefinitions["WixFeatureModules"]);

                    foreach (ConnectToFeature connectToFeature in modulesToFeatures)
                    {
                        foreach (string feature in connectToFeature.ConnectFeatures)
                        {
                            Row row = wixFeatureModulesTable.CreateRow(null);
                            row[0] = feature;
                            row[1] = connectToFeature.ChildId;
                        }
                    }
                }

                // ensure the creation of tables that need to exist
                if (0 < ensureTableRows.Count)
                {
                    foreach (Row row in ensureTableRows)
                    {
                        string tableId = (string)row[0];
                        TableDefinition tableDef = null;

                        try
                        {
                            tableDef = this.tableDefinitions[tableId];
                        }
                        catch (WixMissingTableDefinitionException)
                        {
                            tableDef = customTableDefinitions[tableId];
                        }

                        this.activeOutput.EnsureTable(tableDef);
                    }
                }

                // copy all the suppress action rows to the output to suppress actions from merge modules
                if (0 < suppressActionRows.Count)
                {
                    Table suppressActionTable = this.activeOutput.EnsureTable(this.tableDefinitions["WixSuppressAction"]);
                    suppressActionTable.Rows.AddRange(suppressActionRows);
                }
                
                // sequence all the actions
                this.SequenceActions(actionRows, suppressActionRows);

                // check for missing table and add them or display an error as appropriate
                switch (this.activeOutput.Type)
                {
                    case OutputType.Module:
                        this.activeOutput.EnsureTable(this.tableDefinitions["Component"]);
                        this.activeOutput.EnsureTable(this.tableDefinitions["Directory"]);
                        this.activeOutput.EnsureTable(this.tableDefinitions["FeatureComponents"]);
                        this.activeOutput.EnsureTable(this.tableDefinitions["File"]);
                        this.activeOutput.EnsureTable(this.tableDefinitions["ModuleComponents"]);
                        this.activeOutput.EnsureTable(this.tableDefinitions["ModuleSignature"]);
                        break;
                    case OutputType.PatchCreation:
                        Table imageFamiliesTable = this.activeOutput.Tables["ImageFamilies"];
                        Table targetImagesTable = this.activeOutput.Tables["TargetImages"];
                        Table upgradedImagesTable = this.activeOutput.Tables["UpgradedImages"];

                        if (null == imageFamiliesTable || 1 > imageFamiliesTable.Rows.Count)
                        {
                            this.OnMessage(WixErrors.ExpectedRowInPatchCreationPackage("ImageFamilies"));
                        }

                        if (null == targetImagesTable || 1 > targetImagesTable.Rows.Count)
                        {
                            this.OnMessage(WixErrors.ExpectedRowInPatchCreationPackage("TargetImages"));
                        }

                        if (null == upgradedImagesTable || 1 > upgradedImagesTable.Rows.Count)
                        {
                            this.OnMessage(WixErrors.ExpectedRowInPatchCreationPackage("UpgradedImages"));
                        }

                        this.activeOutput.EnsureTable(this.tableDefinitions["Properties"]);
                        break;
                    case OutputType.Product:
                        this.activeOutput.EnsureTable(this.tableDefinitions["File"]);
                        this.activeOutput.EnsureTable(this.tableDefinitions["Media"]);
                        break;
                }

                // check for illegal tables
                foreach (Table table in this.activeOutput.Tables)
                {
                    switch (this.activeOutput.Type)
                    {
                        case OutputType.Module:
                            if ("BBControl" == table.Name ||
                                "Billboard" == table.Name ||
                                "CCPSearch" == table.Name ||
                                "Feature" == table.Name ||
                                "LaunchCondition" == table.Name ||
                                "Media" == table.Name ||
                                "Patch" == table.Name ||
                                "Upgrade" == table.Name ||
                                "WixMerge" == table.Name)
                            {
                                foreach (Row row in table.Rows)
                                {
                                    this.OnMessage(WixErrors.UnexpectedTableInMergeModule(row.SourceLineNumbers, table.Name));
                                }
                            }
                            else if ("Error" == table.Name)
                            {
                                foreach (Row row in table.Rows)
                                {
                                    this.OnMessage(WixWarnings.DangerousTableInMergeModule(row.SourceLineNumbers, table.Name));
                                }
                            }
                            break;
                        case OutputType.PatchCreation:
                            if ("_SummaryInformation" != table.Name &&
                                "ExternalFiles" != table.Name &&
                                "FamilyFileRanges" != table.Name &&
                                "ImageFamilies" != table.Name &&
                                "PatchMetadata" != table.Name &&
                                "PatchSequence" != table.Name &&
                                "Properties" != table.Name &&
                                "TargetFiles_OptionalData" != table.Name &&
                                "TargetImages" != table.Name &&
                                "UpgradedFiles_OptionalData" != table.Name &&
                                "UpgradedFilesToIgnore" != table.Name &&
                                "UpgradedImages" != table.Name)
                            {
                                foreach (Row row in table.Rows)
                                {
                                    this.OnMessage(WixErrors.UnexpectedTableInPatchCreationPackage(row.SourceLineNumbers, table.Name));
                                }
                            }
                            break;
                        case OutputType.Patch:
                            if (!table.Definition.IsUnreal && 
                                "_SummaryInformation" != table.Name &&
                                "Media" != table.Name &&
                                "MsiPatchMetadata" != table.Name &&
                                "MsiPatchSequence" != table.Name)
                            {
                                foreach (Row row in table.Rows)
                                {
                                    this.OnMessage(WixErrors.UnexpectedTableInPatch(row.SourceLineNumbers, table.Name));
                                }
                            }
                            break;
                        case OutputType.Product:
                            if ("ModuleAdminExecuteSequence" == table.Name ||
                                "ModuleAdminUISequence" == table.Name ||
                                "ModuleAdvtExecuteSequence" == table.Name ||
                                "ModuleAdvtUISequence" == table.Name ||
                                "ModuleComponents" == table.Name ||
                                "ModuleConfiguration" == table.Name ||
                                "ModuleDependency" == table.Name ||
                                "ModuleExclusion" == table.Name ||
                                "ModuleIgnoreTable" == table.Name ||
                                "ModuleInstallExecuteSequence" == table.Name ||
                                "ModuleInstallUISequence" == table.Name ||
                                "ModuleSignature" == table.Name ||
                                "ModuleSubstitution" == table.Name)
                            {
                                foreach (Row row in table.Rows)
                                {
                                    this.OnMessage(WixWarnings.UnexpectedTableInProduct(row.SourceLineNumbers, table.Name));
                                }
                            }
                            break;
                    }
                }

                // add the custom row data
                foreach (Row row in customRows)
                {
                    TableDefinition customTableDefinition = (TableDefinition)customTableDefinitions[row[0].ToString()];
                    Table customTable = this.activeOutput.EnsureTable(customTableDefinition);
                    Row customRow = customTable.CreateRow(row.SourceLineNumbers);

                    string[] data = row[1].ToString().Split('\t');

                    for (int i = 0; i < data.Length; ++i)
                    {
                        bool foundColumn = false;
                        string[] item = data[i].Split(colonCharacter, 2);

                        for (int j = 0; j < customRow.Fields.Length; ++j)
                        {
                            if (customRow.Fields[j].Column.Name == item[0])
                            {
                                if (0 < item[1].Length)
                                {
                                    if (ColumnType.Number == customRow.Fields[j].Column.Type)
                                    {
                                        customRow.Fields[j].Data = Convert.ToInt32(item[1], CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        customRow.Fields[j].Data = item[1];
                                    }
                                }
                                foundColumn = true;
                                break;
                            }
                        }

                        if (!foundColumn)
                        {
                            this.OnMessage(WixErrors.UnexpectedCustomTableColumn(row.SourceLineNumbers, item[0]));
                        }
                    }

                    for (int i = 0; i < customTableDefinition.Columns.Count; ++i)
                    {
                        if (!customTableDefinition.Columns[i].IsNullable && (null == customRow.Fields[i] || 0 == customRow.Fields[i].Data.ToString().Length))
                        {
                            this.OnMessage(WixErrors.NoDataForColumn(row.SourceLineNumbers, customTableDefinition.Columns[i].Name, customTableDefinition.Name));
                        }
                    }
                }

                //correct the section Id in FeatureComponents table
                if (this.sectionIdOnRows)
                {
                    Hashtable componentSectionIds = new Hashtable();
                    Table componentTable = output.Tables["Component"];

                    if (null != componentTable)
                    {
                        foreach (Row componentRow in componentTable.Rows)
                        {
                            componentSectionIds.Add(componentRow.Fields[0].Data.ToString(), componentRow.SectionId);
                        }
                    }

                    Table featureComponentsTable = output.Tables["FeatureComponents"];

                    if (null != featureComponentsTable)
                    {
                        foreach (Row featureComponentsRow in featureComponentsTable.Rows)
                        {
                            if (componentSectionIds.Contains(featureComponentsRow.Fields[1].Data.ToString()))
                            {
                                featureComponentsRow.SectionId = (string)componentSectionIds[featureComponentsRow.Fields[1].Data.ToString()];
                            }
                        }
                    }
                }

                // update the special properties
                if (0 < adminProperties.Count)
                {
                    Table propertyTable = this.activeOutput.EnsureTable(this.tableDefinitions["Property"]);

                    Row row = propertyTable.CreateRow(null);
                    row[0] = "AdminProperties";
                    row[1] = GetPropertyListString(adminProperties);
                }

                if (0 < secureProperties.Count)
                {
                    Table propertyTable = this.activeOutput.EnsureTable(this.tableDefinitions["Property"]);

                    Row row = propertyTable.CreateRow(null);
                    row[0] = "SecureCustomProperties";
                    row[1] = GetPropertyListString(secureProperties);
                }

                if (0 < hiddenProperties.Count)
                {
                    Table propertyTable = this.activeOutput.EnsureTable(this.tableDefinitions["Property"]);

                    Row row = propertyTable.CreateRow(null);
                    row[0] = "MsiHiddenProperties";
                    row[1] = GetPropertyListString(hiddenProperties);
                }

                // add the ModuleSubstitution table to the ModuleIgnoreTable
                if (containsModuleSubstitution)
                {
                    Table moduleIgnoreTableTable = this.activeOutput.EnsureTable(this.tableDefinitions["ModuleIgnoreTable"]);

                    Row moduleIgnoreTableRow = moduleIgnoreTableTable.CreateRow(null);
                    moduleIgnoreTableRow[0] = "ModuleSubstitution";
                }

                // add the ModuleConfiguration table to the ModuleIgnoreTable
                if (containsModuleConfiguration)
                {
                    Table moduleIgnoreTableTable = this.activeOutput.EnsureTable(this.tableDefinitions["ModuleIgnoreTable"]);

                    Row moduleIgnoreTableRow = moduleIgnoreTableTable.CreateRow(null);
                    moduleIgnoreTableRow[0] = "ModuleConfiguration";
                }

                // index all the file rows
                FileRowCollection indexedFileRows = new FileRowCollection();
                Table fileTable = this.activeOutput.Tables["File"];
                if (null != fileTable)
                {
                    indexedFileRows.AddRange(fileTable.Rows);
                }

                // flag all the generated short file name collisions
                foreach (string fileId in generatedShortFileNameIdentifiers)
                {
                    FileRow fileRow = indexedFileRows[fileId];

                    string[] names = fileRow.FileName.Split('|');
                    string shortFileName = names[0];

                    // create lists of conflicting generated short file names
                    if (!generatedShortFileNames.Contains(shortFileName))
                    {
                        generatedShortFileNames.Add(shortFileName, new ArrayList());
                    }
                    ((ArrayList)generatedShortFileNames[shortFileName]).Add(fileRow);
                }

                // check for generated short file name collisions
                foreach (DictionaryEntry entry in generatedShortFileNames)
                {
                    string shortFileName = (string)entry.Key;
                    ArrayList fileRows = (ArrayList)entry.Value;

                    if (1 < fileRows.Count)
                    {
                        // sort the rows by DiskId
                        fileRows.Sort();

                        this.OnMessage(WixWarnings.GeneratedShortFileNameConflict(((FileRow)fileRows[0]).SourceLineNumbers, shortFileName));

                        for (int i = 1; i < fileRows.Count; i++)
                        {
                            FileRow fileRow = (FileRow)fileRows[i];

                            if (null != fileRow.SourceLineNumbers)
                            {
                                this.OnMessage(WixWarnings.GeneratedShortFileNameConflict2(fileRow.SourceLineNumbers));
                            }
                        }
                    }
                }

                // copy the wix variable rows to the output
                if (0 < wixVariables.Count)
                {
                    Table wixVariableTable = output.EnsureTable(this.tableDefinitions["WixVariable"]);

                    foreach (WixVariableRow row in wixVariables.Values)
                    {
                        wixVariableTable.Rows.Add(row);
                    }
                }
            }
            finally
            {
                this.activeOutput = null;
            }

            return (this.encounteredError ? null : output);
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

            if (null != this.Message)
            {
                this.Message(this, mea);
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }

        /// <summary>
        /// Get a sorted property list as a semicolon-delimited string.
        /// </summary>
        /// <param name="properties">SortedList of the properties.</param>
        /// <returns>Semicolon-delimited string representing the property list.</returns>
        private static string GetPropertyListString(SortedList properties)
        {
            bool first = true;
            StringBuilder propertiesString = new StringBuilder();

            foreach (string propertyName in properties.Keys)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    propertiesString.Append(';');
                }
                propertiesString.Append(propertyName);
            }

            return propertiesString.ToString();
        }

        /// <summary>
        /// Load the standard action symbols.
        /// </summary>
        /// <param name="allSymbols">Collection of symbols.</param>
        private void LoadStandardActionSymbols(SymbolCollection allSymbols)
        {
            foreach (WixActionRow actionRow in this.standardActions)
            {
                // if the action's symbol has not already been defined (i.e. overriden by the user), add it now
                if (!allSymbols.Contains(actionRow.Symbol.Name))
                {
                    allSymbols.Add(actionRow.Symbol);
                }
            }
        }

        /// <summary>
        /// Process the complex references.
        /// </summary>
        /// <param name="output">Active output to add sections to.</param>
        /// <param name="sections">Sections that are referenced during the link process.</param>
        /// <param name="referencedSymbols">Collection of all symbols referenced during linking.</param>
        /// <param name="componentsToFeatures">Component to feature complex references.</param>
        /// <param name="featuresToFeatures">Feature to feature complex references.</param>
        /// <param name="modulesToFeatures">Module to feature complex references.</param>
        private void ProcessComplexReferences(
            Output output,
            SectionCollection sections,
            StringCollection referencedSymbols,
            ConnectToFeatureCollection componentsToFeatures,
            ConnectToFeatureCollection featuresToFeatures,
            ConnectToFeatureCollection modulesToFeatures)
        {
            Hashtable componentsToModules = new Hashtable();

            foreach (Section section in sections)
            {
                Table wixComplexReferenceTable = section.Tables["WixComplexReference"];

                if (null != wixComplexReferenceTable)
                {
                    foreach (WixComplexReferenceRow wixComplexReferenceRow in wixComplexReferenceTable.Rows)
                    {
                        ConnectToFeature connection;
                        switch (wixComplexReferenceRow.ParentType)
                        {
                            case ComplexReferenceParentType.Feature:
                                switch (wixComplexReferenceRow.ChildType)
                                {
                                    case ComplexReferenceChildType.Component:
                                        connection = componentsToFeatures[wixComplexReferenceRow.ChildId];
                                        if (null == connection)
                                        {
                                            componentsToFeatures.Add(new ConnectToFeature(section, wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentId, wixComplexReferenceRow.IsPrimary));
                                        }
                                        else if (wixComplexReferenceRow.IsPrimary)
                                        {
                                            if (connection.IsExplicitPrimaryFeature)
                                            {
                                                this.OnMessage(WixErrors.MultiplePrimaryReferences(section.SourceLineNumbers, wixComplexReferenceRow.ChildType.ToString(), wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentType.ToString(), wixComplexReferenceRow.ParentId, (null != connection.PrimaryFeature ? "Feature" : "Product"), (null != connection.PrimaryFeature ? connection.PrimaryFeature : this.activeOutput.EntrySection.Id)));
                                                continue;
                                            }
                                            else
                                            {
                                                connection.ConnectFeatures.Add(connection.PrimaryFeature); // move the guessed primary feature to the list of connects
                                                connection.PrimaryFeature = wixComplexReferenceRow.ParentId; // set the new primary feature
                                                connection.IsExplicitPrimaryFeature = true; // and make sure we remember that we set it so we can fail if we try to set it again
                                            }
                                        }
                                        else
                                        {
                                            connection.ConnectFeatures.Add(wixComplexReferenceRow.ParentId);
                                        }

                                        // add a row to the FeatureComponents table
                                        Table featureComponentsTable = output.EnsureTable(this.tableDefinitions["FeatureComponents"]);
                                        Row row = featureComponentsTable.CreateRow(null);
                                        if (this.sectionIdOnRows)
                                        {
                                            row.SectionId = section.Id;
                                        }
                                        row[0] = wixComplexReferenceRow.ParentId;
                                        row[1] = wixComplexReferenceRow.ChildId;

                                        // index the component for finding orphaned records
                                        string symbolName = String.Concat("Component:", wixComplexReferenceRow.ChildId);
                                        if (!referencedSymbols.Contains(symbolName))
                                        {
                                            referencedSymbols.Add(symbolName);
                                        }

                                        break;

                                    case ComplexReferenceChildType.Feature:
                                        connection = featuresToFeatures[wixComplexReferenceRow.ChildId];
                                        if (null != connection)
                                        {
                                            this.OnMessage(WixErrors.MultiplePrimaryReferences(section.SourceLineNumbers, wixComplexReferenceRow.ChildType.ToString(), wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentType.ToString(), wixComplexReferenceRow.ParentId, (null != connection.PrimaryFeature ? "Feature" : "Product"), (null != connection.PrimaryFeature ? connection.PrimaryFeature : this.activeOutput.EntrySection.Id)));
                                            continue;
                                        }

                                        featuresToFeatures.Add(new ConnectToFeature(section, wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentId, wixComplexReferenceRow.IsPrimary));
                                        break;

                                    case ComplexReferenceChildType.Module:
                                        connection = modulesToFeatures[wixComplexReferenceRow.ChildId];
                                        if (null == connection)
                                        {
                                            modulesToFeatures.Add(new ConnectToFeature(section, wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentId, wixComplexReferenceRow.IsPrimary));
                                        }
                                        else if (wixComplexReferenceRow.IsPrimary)
                                        {
                                            if (connection.IsExplicitPrimaryFeature)
                                            {
                                                this.OnMessage(WixErrors.MultiplePrimaryReferences(section.SourceLineNumbers, wixComplexReferenceRow.ChildType.ToString(), wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentType.ToString(), wixComplexReferenceRow.ParentId, (null != connection.PrimaryFeature ? "Feature" : "Product"), (null != connection.PrimaryFeature ? connection.PrimaryFeature : this.activeOutput.EntrySection.Id)));
                                                continue;
                                            }
                                            else
                                            {
                                                connection.ConnectFeatures.Add(connection.PrimaryFeature); // move the guessed primary feature to the list of connects
                                                connection.PrimaryFeature = wixComplexReferenceRow.ParentId; // set the new primary feature
                                                connection.IsExplicitPrimaryFeature = true; // and make sure we remember that we set it so we can fail if we try to set it again
                                            }
                                        }
                                        else
                                        {
                                            connection.ConnectFeatures.Add(wixComplexReferenceRow.ParentId);
                                        }
                                        break;

                                    default:
                                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unexpected complex reference child type: {0}", Enum.GetName(typeof(ComplexReferenceChildType), wixComplexReferenceRow.ChildType)));
                                }
                                break;

                            case ComplexReferenceParentType.Module:
                                switch (wixComplexReferenceRow.ChildType)
                                {
                                    case ComplexReferenceChildType.Component:
                                        if (componentsToModules.ContainsKey(wixComplexReferenceRow.ChildId))
                                        {
                                            this.OnMessage(WixErrors.ComponentReferencedTwice(section.SourceLineNumbers, wixComplexReferenceRow.ChildId));
                                            continue;
                                        }
                                        else
                                        {
                                            componentsToModules.Add(wixComplexReferenceRow.ChildId, wixComplexReferenceRow); // should always be new

                                            // add a row to the ModuleComponents table
                                            Table moduleComponentsTable = output.EnsureTable(this.tableDefinitions["ModuleComponents"]);
                                            Row row = moduleComponentsTable.CreateRow(null);
                                            if (this.sectionIdOnRows)
                                            {
                                                row.SectionId = section.Id;
                                            }
                                            row[0] = wixComplexReferenceRow.ChildId;
                                            row[1] = wixComplexReferenceRow.ParentId;
                                            row[2] = wixComplexReferenceRow.ParentLanguage;
                                        }

                                        // index the component for finding orphaned records
                                        string componentSymbolName = String.Concat("Component:", wixComplexReferenceRow.ChildId);
                                        if (!referencedSymbols.Contains(componentSymbolName))
                                        {
                                            referencedSymbols.Add(componentSymbolName);
                                        }

                                        break;

                                    default:
                                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unexpected complex reference child type: {0}", Enum.GetName(typeof(ComplexReferenceChildType), wixComplexReferenceRow.ChildType)));
                                }
                                break;

                            case ComplexReferenceParentType.Product:
                                switch (wixComplexReferenceRow.ChildType)
                                {
                                    case ComplexReferenceChildType.Feature:
                                        connection = featuresToFeatures[wixComplexReferenceRow.ChildId];
                                        if (null != connection)
                                        {
                                            this.OnMessage(WixErrors.MultiplePrimaryReferences(section.SourceLineNumbers, wixComplexReferenceRow.ChildType.ToString(), wixComplexReferenceRow.ChildId, wixComplexReferenceRow.ParentType.ToString(), wixComplexReferenceRow.ParentId, (null != connection.PrimaryFeature ? "Feature" : "Product"), (null != connection.PrimaryFeature ? connection.PrimaryFeature : this.activeOutput.EntrySection.Id)));
                                            continue;
                                        }

                                        featuresToFeatures.Add(new ConnectToFeature(section, wixComplexReferenceRow.ChildId, null, wixComplexReferenceRow.IsPrimary));
                                        break;

                                    default:
                                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unexpected complex reference child type: {0}", Enum.GetName(typeof(ComplexReferenceChildType), wixComplexReferenceRow.ChildType)));
                                }
                                break;

                            default:
                                // Note: Groups have been processed before getting here so they are not handled by any case above.
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unexpected complex reference parent type: {0}", Enum.GetName(typeof(ComplexReferenceParentType), wixComplexReferenceRow.ParentType)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flattens all complex references in all sections in the collection.
        /// </summary>
        /// <param name="sections">Sections that are referenced during the link process.</param>
        private void FlattenSectionsComplexReferences(SectionCollection sections)
        {
            Hashtable parentGroups = new Hashtable();
            Hashtable parentGroupsSections = new Hashtable();
            Hashtable parentGroupsNeedingProcessing = new Hashtable();

            // DisplaySectionComplexReferences("--- section's complex references before flattening ---", sections);

            // Step 1:  Gather all of the complex references that are going participate
            // in the flatting process.  This means complex references that have "grouping
            //  parents" of Features, Modules, and, of course, Groups.  These references
            // that participate in a "grouping parent" will be removed from their section
            // now and after processing added back in Step 3 below.
            foreach (Section section in sections)
            {
                Table wixComplexReferenceTable = section.Tables["WixComplexReference"];

                if (null != wixComplexReferenceTable)
                {
                    // Count down because we'll sometimes remove items from the list.
                    for (int i = wixComplexReferenceTable.Rows.Count - 1; i >= 0; --i)
                    {
                        WixComplexReferenceRow wixComplexReferenceRow = (WixComplexReferenceRow)wixComplexReferenceTable.Rows[i];

                        // Only process the "grouping parents" such as Group, Feature,
                        // and Module.  Non-grouping complex references are simple and
                        // resolved during normal complex reference resolutions.
                        if (ComplexReferenceParentType.Group == wixComplexReferenceRow.ParentType ||
                            ComplexReferenceParentType.Feature == wixComplexReferenceRow.ParentType ||
                            ComplexReferenceParentType.Module == wixComplexReferenceRow.ParentType ||
                            ComplexReferenceParentType.Product == wixComplexReferenceRow.ParentType)
                        {
                            // Group all complex references with a common parent
                            // together so we can find them quickly while processing in
                            // Step 2.
                            ArrayList childrenComplexRefs = parentGroups[wixComplexReferenceRow.ParentId] as ArrayList;
                            if (null == childrenComplexRefs)
                            {
                                childrenComplexRefs = new ArrayList();
                                parentGroups.Add(wixComplexReferenceRow.ParentId, childrenComplexRefs);
                            }

                            childrenComplexRefs.Add(wixComplexReferenceRow);
                            wixComplexReferenceTable.Rows.RemoveAt(i);

                            // Remember the mapping from set of complex references with a common
                            // parent to their section.  We'll need this to add them back to the
                            // correct section in Step 3.
                            Section parentSection = parentGroupsSections[wixComplexReferenceRow.ParentId] as Section;
                            if (null == parentSection)
                            {
                                parentGroupsSections.Add(wixComplexReferenceRow.ParentId, section);
                            }
                            // Debug.Assert(section == (Section)parentGroupsSections[wixComplexReferenceRow.ParentId]);

                            // If the child of the complex reference is another group, then in Step 2
                            // we're going to have to process this complex reference again to copy 
                            // the child group's references into the parent group.
                            if (ComplexReferenceChildType.Group == wixComplexReferenceRow.ChildType)
                            {
                                if (!parentGroupsNeedingProcessing.ContainsKey(wixComplexReferenceRow.ParentId))
                                {
                                    parentGroupsNeedingProcessing.Add(wixComplexReferenceRow.ParentId, section);
                                }
                                // Debug.Assert(section == (Section)parentGroupsNeedingProcessing[wixComplexReferenceRow.ParentId]);
                            }
                        }
                    }
                }
            }
            Debug.Assert(parentGroups.Count == parentGroupsSections.Count);
            Debug.Assert(parentGroupsNeedingProcessing.Count <= parentGroups.Count);

            // DisplaySectionComplexReferences("\r\n\r\n--- section's complex references middle of flattening ---", sections);

            // Step 2:  Loop through the parent groups that have nested groups removing
            // them from the hash table as they are processed.  At the end of this the
            // complex references should all be flattened.
            string[] keys = new string[parentGroupsNeedingProcessing.Keys.Count];
            parentGroupsNeedingProcessing.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                if (parentGroupsNeedingProcessing.Contains(key))
                {
                    Stack loopDetector = new Stack();
                    this.FlattenGroup(key, loopDetector, parentGroups, parentGroupsNeedingProcessing);
                }
                else
                {
                    // the group must have allready been procesed and removed from the hash table
                }
            }
            Debug.Assert(0 == parentGroupsNeedingProcessing.Count);

            // Step 3:  Finally, ensure that all of the groups that were removed
            // in Step 1 and flattened in Step 2 are added to their appropriate
            // section.  This is where we will toss out the final no-longer-needed
            // groups.
            foreach (string parentGroup in parentGroups.Keys)
            {
                Section section = (Section)parentGroupsSections[parentGroup];
                Table wixComplexReferenceTable = section.Tables["WixComplexReference"];

                foreach (WixComplexReferenceRow wixComplexReferenceRow in (ArrayList)parentGroups[parentGroup])
                {
                    if (ComplexReferenceParentType.Group != wixComplexReferenceRow.ParentType)
                    {
                        wixComplexReferenceTable.Rows.Add(wixComplexReferenceRow);
                    }
                }
            }

            // DisplaySectionComplexReferences("\r\n\r\n--- section's complex references after flattening ---", sections);
        }

        /// <summary>
        /// Recursively processes the group.
        /// </summary>
        /// <param name="parentId">String name of group to process next.</param>
        /// <param name="loopDetector">Stack of groups processed thus far.  Used to detect loops.</param>
        /// <param name="parentGroups">Hash table of complex references grouped by parent id.</param>
        /// <param name="parentGroupsNeedingProcessing">Hash table of parent groups that still have nested groups that need to be flattened.</param>
        private void FlattenGroup(string parentId, Stack loopDetector, Hashtable parentGroups, Hashtable parentGroupsNeedingProcessing)
        {
            Debug.Assert(parentGroupsNeedingProcessing.Contains(parentId));
            loopDetector.Push(parentId); // push this complex reference child identfier into the stack for loop verifying

            ArrayList allNewChildComplexReferences = new ArrayList();
            ArrayList referencesToParent = (ArrayList)parentGroups[parentId];
            foreach (WixComplexReferenceRow wixComplexReferenceRow in referencesToParent)
            {
                Debug.Assert(ComplexReferenceParentType.Group == wixComplexReferenceRow.ParentType || ComplexReferenceParentType.Feature == wixComplexReferenceRow.ParentType || ComplexReferenceParentType.Module == wixComplexReferenceRow.ParentType || ComplexReferenceParentType.Product == wixComplexReferenceRow.ParentType);
                Debug.Assert(parentId == wixComplexReferenceRow.ParentId);

                // We are only interested processing when the child is a group.
                if (ComplexReferenceChildType.Group == wixComplexReferenceRow.ChildType)
                {
                    if (loopDetector.Contains(wixComplexReferenceRow.ChildId))
                    {
                        // Create a comma delimited list of the references that participate in the 
                        // loop for the error message.  Start with the child id that created the
                        // problem.
                        StringBuilder loop = new StringBuilder(wixComplexReferenceRow.ChildId);
                        foreach (string childId in loopDetector)
                        {
                            loop.Append(", ");
                            loop.Append(childId);
                        }

                        this.OnMessage(WixErrors.ReferenceLoopDetected(wixComplexReferenceRow.Table.Section == null ? null : wixComplexReferenceRow.Table.Section.SourceLineNumbers, wixComplexReferenceRow.ChildId, loop.ToString()));
                        return; // bail
                    }

                    // Check to see if the child group still needs to be processed.  If so,
                    // go do that so that we'll get all of that children's (and children's 
                    // children) complex references correctly merged into our parent group.
                    if (parentGroupsNeedingProcessing.ContainsKey(wixComplexReferenceRow.ChildId))
                    {
                        this.FlattenGroup(wixComplexReferenceRow.ChildId, loopDetector, parentGroups, parentGroupsNeedingProcessing);
                    }

                    // If the child is a parent to anything (i.e. the parent has grandchildren)
                    // clone each of the children's complex references, repoint them to the parent
                    // complex reference (because we're moving references up the tree), and finally
                    // add the cloned child's complex reference to the list of complex references 
                    // that we'll eventually add to the parent group.
                    ArrayList referencesToChild = (ArrayList)parentGroups[wixComplexReferenceRow.ChildId];
                    if (null != referencesToChild)
                    {
                        foreach (WixComplexReferenceRow crefChild in referencesToChild)
                        {
                            // Only merge up the non-group items since groups are purged
                            // after this part of the processing anyway (cloning them would
                            // be a complete waste of time).
                            if (ComplexReferenceChildType.Group != crefChild.ChildType)
                            {
                                WixComplexReferenceRow crefChildClone = crefChild.Clone();
                                Debug.Assert(crefChildClone.ParentId == wixComplexReferenceRow.ChildId);

                                crefChildClone.Reparent(wixComplexReferenceRow);
                                allNewChildComplexReferences.Add(crefChildClone);
                            }
                        }
                    }
                }
            }

            // Add the children group's complex references to the parent
            // group.  Clean out any left over groups and quietly remove any
            // duplicate complex references that occurred during the merge.
            referencesToParent.AddRange(allNewChildComplexReferences);
            referencesToParent.Sort();
            for (int i = referencesToParent.Count - 1; i >= 0; --i)
            {
                WixComplexReferenceRow wixComplexReferenceRow = (WixComplexReferenceRow)referencesToParent[i];
                if (ComplexReferenceChildType.Group == wixComplexReferenceRow.ChildType)
                {
                    referencesToParent.RemoveAt(i);
                }
                else if (i > 0)
                {
                    // Since the list is already sorted, we can find duplicates by simply 
                    // looking at the next sibling in the list and tossing out one if they
                    // match.
                    WixComplexReferenceRow crefCompare = (WixComplexReferenceRow)referencesToParent[i - 1];
                    if (0 == wixComplexReferenceRow.CompareToWithoutConsideringPrimary(crefCompare))
                    {
                        referencesToParent.RemoveAt(i);
                    }
                }
            }

            loopDetector.Pop(); // pop this complex reference off the stack since we're done verify the loop here
            parentGroupsNeedingProcessing.Remove(parentId); // remove the newly processed complex reference
        }

/*
        /// <summary>
        /// Debugging method for displaying the section complex references.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="sections">The sections to display.</param>
        private void DisplaySectionComplexReferences(string header, SectionCollection sections)
        {
            Console.WriteLine(header);
            foreach (Section section in sections)
            {
                Table wixComplexReferenceTable = section.Tables["WixComplexReference"];

                foreach (WixComplexReferenceRow cref in wixComplexReferenceTable.Rows)
                {
                    Console.WriteLine("Section: {0} Parent: {1} Type: {2} Child: {3} Primary: {4}", section.Id, cref.ParentId, cref.ParentType, cref.ChildId, cref.IsPrimary);
                }
            }
        }
*/

        /// <summary>
        /// Resolves the features connected to other features in the active output.
        /// </summary>
        /// <param name="featuresToFeatures">Feature to feature complex references.</param>
        /// <param name="allSymbols">All symbols loaded from the sections.</param>
        private void ResolveFeatureToFeatureConnects(
            ConnectToFeatureCollection featuresToFeatures, 
            SymbolCollection allSymbols)
        {
            foreach (ConnectToFeature connection in featuresToFeatures)
            {
                WixSimpleReferenceRow wixSimpleReferenceRow = new WixSimpleReferenceRow(null, this.tableDefinitions["WixSimpleReference"]);
                wixSimpleReferenceRow.TableName = "Feature";
                wixSimpleReferenceRow.PrimaryKeys = connection.ChildId;
                Symbol symbol = allSymbols.GetSymbolForSimpleReference(wixSimpleReferenceRow, this);
                if (null == symbol)
                {
                    continue;
                }

                Row row = symbol.Row;
                row[1] = connection.PrimaryFeature;
            }
        }

        /// <summary>
        /// Copies a table's rows to an output table.
        /// </summary>
        /// <param name="table">Source table to copy rows from.</param>
        /// <param name="outputTable">Destination table in output to copy rows into.</param>
        /// <param name="sectionId">Id of the section that the table lives in.</param>
        private void CopyTableRowsToOutputTable(Table table, Table outputTable, string sectionId)
        {
            int[] localizedColumns = new int[table.Definition.Columns.Count];
            int localizedColumnCount = 0;

            // if there are localization strings, figure out which columns can be localized in this table
            if (null != this.localizer)
            {
                for (int i = 0; i < table.Definition.Columns.Count; i++)
                {
                    if (table.Definition.Columns[i].IsLocalizable)
                    {
                        localizedColumns[localizedColumnCount++] = i;
                    }
                }
            }

            // process each row in the table doing the string resource substitutions
            // then add the row to the output
            foreach (Row row in table.Rows)
            {
                for (int j = 0; j < localizedColumnCount; j++)
                {
                    Field field = row.Fields[localizedColumns[j]];

                    if (null != field.Data)
                    {
                        field.Data = this.wixVariableResolver.ResolveVariables(this.localizer, row.SourceLineNumbers, (string)field.Data, true);
                    }
                }

                row.SectionId = (this.sectionIdOnRows ? sectionId : null);
                outputTable.Rows.Add(row);
            }

            // remember if errors were found
            if (this.wixVariableResolver.EncounteredError)
            {
                this.encounteredError = true;
            }
        }

        /// <summary>
        /// Set sequence numbers for all the actions and create rows in the output object.
        /// </summary>
        /// <param name="actionRows">Collection of actions to schedule.</param>
        /// <param name="suppressActionRows">Collection of actions to suppress.</param>
        private void SequenceActions(RowCollection actionRows, RowCollection suppressActionRows)
        {
            WixActionRowCollection overridableActionRows = new WixActionRowCollection();
            WixActionRowCollection requiredActionRows = new WixActionRowCollection();
            ArrayList scheduledActionRows = new ArrayList();

            // gather the required actions for the output type
            if (OutputType.Product == this.activeOutput.Type)
            {
                // AdminExecuteSequence table
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "CostFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "CostInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "FileCost"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "InstallAdminPackage"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "InstallFiles"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "InstallFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "InstallInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminExecuteSequence, "InstallValidate"]);

                // AdminUISequence table
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminUISequence, "CostFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminUISequence, "CostInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminUISequence, "ExecuteAction"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdminUISequence, "FileCost"]);

                // AdvtExecuteSequence table
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "CostFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "CostInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "InstallFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "InstallInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "InstallValidate"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "PublishFeatures"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "PublishProduct"]);

                // InstallExecuteSequence table
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "CostFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "CostInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "FileCost"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "InstallFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "InstallInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "InstallValidate"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "ProcessComponents"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "PublishFeatures"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "PublishProduct"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterProduct"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterUser"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnpublishFeatures"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "ValidateProductID"]);

                // InstallUISequence table
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "CostFinalize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "CostInitialize"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "ExecuteAction"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "FileCost"]);
                overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "ValidateProductID"]);
            }

            // gather the required actions for each table
            foreach (Table table in this.activeOutput.Tables)
            {
                switch (table.Name)
                {
                    case "AppSearch":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "AppSearch"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "AppSearch"], true);
                        break;
                    case "BindImage":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "BindImage"], true);
                        break;
                    case "CCPSearch":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "AppSearch"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "CCPSearch"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RMCCPSearch"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "AppSearch"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "CCPSearch"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "RMCCPSearch"], true);
                        break;
                    case "Class":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "RegisterClassInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterClassInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterClassInfo"], true);
                        break;
                    case "Complus":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterComPlus"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterComPlus"], true);
                        break;
                    case "CreateFolder":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "CreateFolders"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveFolders"], true);
                        break;
                    case "DuplicateFile":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "DuplicateFiles"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveDuplicateFiles"], true);
                        break;
                    case "Environment":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "WriteEnvironmentStrings"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveEnvironmentStrings"], true);
                        break;
                    case "Extension":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "RegisterExtensionInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterExtensionInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterExtensionInfo"], true);
                        break;
                    case "File":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "InstallFiles"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveFiles"], true);
                        break;
                    case "Font":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterFonts"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterFonts"], true);
                        break;
                    case "IniFile":
                    case "RemoveIniFile":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "WriteIniValues"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveIniValues"], true);
                        break;
                    case "IsolatedComponent":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "IsolateComponents"], true);
                        break;
                    case "LaunchCondition":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "LaunchConditions"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "LaunchConditions"], true);
                        break;
                    case "MIME":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "RegisterMIMEInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterMIMEInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterMIMEInfo"], true);
                        break;
                    case "MoveFile":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "MoveFiles"], true);
                        break;
                    case "MsiAssembly":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "MsiPublishAssemblies"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "MsiPublishAssemblies"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "MsiUnpublishAssemblies"], true);
                        break;
                    case "ODBCDataSource":
                    case "ODBCTranslator":
                    case "ODBCDriver":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "SetODBCFolders"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "InstallODBC"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveODBC"], true);
                        break;
                    case "ProgId":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "RegisterProgIdInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterProgIdInfo"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterProgIdInfo"], true);
                        break;
                    case "PublishComponent":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "PublishComponents"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "PublishComponents"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnpublishComponents"], true);
                        break;
                    case "Registry":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "WriteRegistryValues"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveRegistryValues"], true);
                        break;
                    case "RemoveFile":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveFiles"], true);
                        break;
                    case "SelfReg":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "SelfRegModules"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "SelfUnregModules"], true);
                        break;
                    case "ServiceControl":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "StartServices"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "StopServices"], true);
                        break;
                    case "ServiceInstall":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "InstallServices"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "DeleteServices"], true);
                        break;
                    case "Shortcut":
                        overridableActionRows.Add(this.standardActions[SequenceTable.AdvtExecuteSequence, "CreateShortcuts"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "CreateShortcuts"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RemoveShortcuts"], true);
                        break;
                    case "TypeLib":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "RegisterTypeLibraries"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "UnregisterTypeLibraries"], true);
                        break;
                    case "Upgrade":
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "FindRelatedProducts"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallExecuteSequence, "MigrateFeatureStates"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "FindRelatedProducts"], true);
                        overridableActionRows.Add(this.standardActions[SequenceTable.InstallUISequence, "MigrateFeatureStates"], true);
                        break;
                }
            }

            // index all the action rows (look for collisions)
            foreach (WixActionRow actionRow in actionRows)
            {
                if (actionRow.Overridable) // overridable action
                {
                    WixActionRow collidingActionRow = overridableActionRows[actionRow.SequenceTable, actionRow.Action];

                    if (null != collidingActionRow)
                    {
                        this.OnMessage(WixErrors.OverridableActionCollision(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        if (null != collidingActionRow.SourceLineNumbers)
                        {
                            this.OnMessage(WixErrors.OverridableActionCollision2(collidingActionRow.SourceLineNumbers));
                        }
                    }
                    else
                    {
                        overridableActionRows.Add(actionRow);
                    }
                }
                else // unscheduled/scheduled action
                {
                    // unscheduled action (allowed for certain standard actions)
                    if (null == actionRow.Before && null == actionRow.After && 0 == actionRow.Sequence)
                    {
                        WixActionRow standardAction = this.standardActions[actionRow.SequenceTable, actionRow.Action];

                        if (null != standardAction)
                        {
                            // populate the sequence from the standard action
                            actionRow.Sequence = standardAction.Sequence;
                        }
                        else // not a supported unscheduled action
                        {
                            throw new InvalidOperationException("Found an ActionRow with no Sequence, Before, or After column set.");
                        }
                    }

                    WixActionRow collidingActionRow = requiredActionRows[actionRow.SequenceTable, actionRow.Action];

                    if (null != collidingActionRow)
                    {
                        this.OnMessage(WixErrors.ActionCollision(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        if (null != collidingActionRow.SourceLineNumbers)
                        {
                            this.OnMessage(WixErrors.ActionCollision2(collidingActionRow.SourceLineNumbers));
                        }
                    }
                    else
                    {
                        requiredActionRows.Add(actionRow);
                    }
                }
            }

            // add the overridable action rows that are not overrided to the required action rows
            foreach (WixActionRow actionRow in overridableActionRows)
            {
                if (null == requiredActionRows[actionRow.SequenceTable, actionRow.Action])
                {
                    requiredActionRows.Add(actionRow);
                }
            }

            // suppress the required actions that are overridable
            foreach (Row suppressActionRow in suppressActionRows)
            {
                SequenceTable sequenceTable = (SequenceTable)Enum.Parse(typeof(SequenceTable), (string)suppressActionRow[0]);
                string action = (string)suppressActionRow[1];

                // get the action being suppressed (if it exists)
                WixActionRow requiredActionRow = requiredActionRows[sequenceTable, action];

                // if there is an overridable row to suppress; suppress it
                // there is no warning if there is no action to suppress because the action may be suppressed from a merge module in the binder
                if (null != requiredActionRow)
                {
                    if (requiredActionRow.Overridable)
                    {
                        this.OnMessage(WixWarnings.SuppressAction(suppressActionRow.SourceLineNumbers, action, sequenceTable.ToString()));
                        if (null != requiredActionRow.SourceLineNumbers)
                        {
                            this.OnMessage(WixWarnings.SuppressAction2(requiredActionRow.SourceLineNumbers));
                        }
                        requiredActionRows.Remove(sequenceTable, action);
                    }
                    else // suppressing a non-overridable action row
                    {
                        this.OnMessage(WixErrors.SuppressNonoverridableAction(suppressActionRow.SourceLineNumbers, sequenceTable.ToString(), action));
                        if (null != requiredActionRow.SourceLineNumbers)
                        {
                            this.OnMessage(WixErrors.SuppressNonoverridableAction2(requiredActionRow.SourceLineNumbers));
                        }
                    }
                }
            }

            // create a copy of the required action rows so that new rows can be added while enumerating
            WixActionRow[] copyOfRequiredActionRows = new WixActionRow[requiredActionRows.Count];
            requiredActionRows.CopyTo(copyOfRequiredActionRows, 0);

            // build up dependency trees of the relatively scheduled actions
            foreach (WixActionRow actionRow in copyOfRequiredActionRows)
            {
                if (0 == actionRow.Sequence)
                {
                    // check for standard actions that don't have a sequence number in a merge module
                    if (OutputType.Module == this.activeOutput.Type && Installer.IsStandardAction(actionRow.Action))
                    {
                        this.OnMessage(WixErrors.StandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                    }

                    if (null != actionRow.Before)
                    {
                        WixActionRow parentActionRow = requiredActionRows[actionRow.SequenceTable, actionRow.Before];

                        if (null == parentActionRow)
                        {
                            parentActionRow = this.standardActions[actionRow.SequenceTable, actionRow.Before];

                            // if the missing parent action is a standard action (with a suggested sequence number), add it
                            if (null != parentActionRow)
                            {
                                requiredActionRows.Add(parentActionRow);
                            }
                            else
                            {
                                throw new InvalidOperationException("Found an ActionRow with a non-existant Before action.");
                            }
                        }
                        else if (actionRow == parentActionRow || actionRow.ContainsChildActionRow(parentActionRow)) // cycle detected
                        {
                            throw new WixException(WixErrors.ActionCircularDependency(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action, parentActionRow.Action));
                        }

                        // if the parent action row was found, add this action to the appropriate list of dependent action rows
                        if (null != parentActionRow)
                        {
                            parentActionRow.PreviousActionRows.Add(actionRow);
                        }
                    }
                    else if (null != actionRow.After)
                    {
                        WixActionRow parentActionRow = requiredActionRows[actionRow.SequenceTable, actionRow.After];

                        if (null == parentActionRow)
                        {
                            parentActionRow = this.standardActions[actionRow.SequenceTable, actionRow.After];

                            // if the missing parent action is a standard action (with a suggested sequence number), add it
                            if (null != parentActionRow)
                            {
                                requiredActionRows.Add(parentActionRow);
                            }
                            else
                            {
                                throw new InvalidOperationException("Found an ActionRow with a non-existant After action.");
                            }
                        }
                        else if (actionRow == parentActionRow || actionRow.ContainsChildActionRow(parentActionRow)) // cycle detected
                        {
                            throw new WixException(WixErrors.ActionCircularDependency(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action, parentActionRow.Action));
                        }

                        // if the parent action row was found, add this action to the appropriate list of dependent action rows
                        if (null != parentActionRow)
                        {
                            parentActionRow.NextActionRows.Add(actionRow);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Found an ActionRow with no Sequence, Before, or After column set.");
                    }
                }
                else if (OutputType.Module == this.activeOutput.Type && 0 < actionRow.Sequence && !Installer.IsStandardAction(actionRow.Action)) // check for custom actions and dialogs that have a sequence number
                {
                    this.OnMessage(WixErrors.CustomActionSequencedInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                }
            }

            // schedule actions
            if (OutputType.Module == this.activeOutput.Type)
            {
                // add the action row to the list of scheduled action rows
                scheduledActionRows.AddRange(requiredActionRows);
            }
            else
            {
                // process each sequence table individually
                foreach (SequenceTable sequenceTable in Enum.GetValues(typeof(SequenceTable)))
                {
                    // create a collection of just the action rows in this sequence
                    WixActionRowCollection sequenceActionRows = new WixActionRowCollection();
                    foreach (WixActionRow actionRow in requiredActionRows)
                    {
                        if (sequenceTable == actionRow.SequenceTable)
                        {
                            sequenceActionRows.Add(actionRow);
                        }
                    }

                    // schedule the absolutely scheduled actions (by sorting them by their sequence numbers)
                    ArrayList absoluteActionRows = new ArrayList();
                    foreach (WixActionRow actionRow in sequenceActionRows)
                    {
                        if (0 != actionRow.Sequence)
                        {
                            // look for sequence number collisions
                            foreach (WixActionRow sequenceScheduledActionRow in absoluteActionRows)
                            {
                                if (sequenceScheduledActionRow.Sequence == actionRow.Sequence)
                                {
                                    this.OnMessage(WixWarnings.ActionSequenceCollision(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action, sequenceScheduledActionRow.Action, actionRow.Sequence));
                                    if (null != sequenceScheduledActionRow.SourceLineNumbers)
                                    {
                                        this.OnMessage(WixWarnings.ActionSequenceCollision2(sequenceScheduledActionRow.SourceLineNumbers));
                                    }
                                }
                            }

                            absoluteActionRows.Add(actionRow);
                        }
                    }
                    absoluteActionRows.Sort();

                    // schedule the relatively scheduled actions (by resolving the dependency trees)
                    int previousUsedSequence = 0;
                    ArrayList relativeActionRows = new ArrayList();
                    for (int j = 0; j < absoluteActionRows.Count; j++)
                    {
                        WixActionRow absoluteActionRow = (WixActionRow)absoluteActionRows[j];
                        int unusedSequence;

                        // get all the relatively scheduled action rows occuring before this absolutely scheduled action row
                        RowCollection allPreviousActionRows = new RowCollection();
                        absoluteActionRow.GetAllPreviousActionRows(sequenceTable, allPreviousActionRows);

                        // get all the relatively scheduled action rows occuring after this absolutely scheduled action row
                        RowCollection allNextActionRows = new RowCollection();
                        absoluteActionRow.GetAllNextActionRows(sequenceTable, allNextActionRows);

                        // check for relatively scheduled actions occuring before/after a special action (these have a negative sequence number)
                        if (0 > absoluteActionRow.Sequence && (0 < allPreviousActionRows.Count || 0 < allNextActionRows.Count))
                        {
                            // create errors for all the before actions
                            foreach (WixActionRow actionRow in allPreviousActionRows)
                            {
                                this.OnMessage(WixErrors.ActionScheduledRelativeToTerminationAction(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action, absoluteActionRow.Action));
                            }

                            // create errors for all the after actions
                            foreach (WixActionRow actionRow in allNextActionRows)
                            {
                                this.OnMessage(WixErrors.ActionScheduledRelativeToTerminationAction(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action, absoluteActionRow.Action));
                            }

                            // if there is source line information for the absolutely scheduled action display it
                            if (null != absoluteActionRow.SourceLineNumbers)
                            {
                                this.OnMessage(WixErrors.ActionScheduledRelativeToTerminationAction2(absoluteActionRow.SourceLineNumbers));
                            }

                            continue;
                        }

                        // schedule the action rows before this one
                        unusedSequence = absoluteActionRow.Sequence - 1;
                        for (int i = allPreviousActionRows.Count - 1; i >= 0; i--)
                        {
                            WixActionRow relativeActionRow = (WixActionRow)allPreviousActionRows[i];

                            // look for collisions
                            if (unusedSequence == previousUsedSequence)
                            {
                                this.OnMessage(WixErrors.NoUniqueActionSequenceNumber(relativeActionRow.SourceLineNumbers, relativeActionRow.SequenceTable.ToString(), relativeActionRow.Action, absoluteActionRow.Action));
                                if (null != absoluteActionRow.SourceLineNumbers)
                                {
                                    this.OnMessage(WixErrors.NoUniqueActionSequenceNumber2(absoluteActionRow.SourceLineNumbers));
                                }

                                unusedSequence++;
                            }

                            relativeActionRow.Sequence = unusedSequence;
                            relativeActionRows.Add(relativeActionRow);

                            unusedSequence--;
                        }

                        // determine the next used action sequence number
                        int nextUsedSequence;
                        if (absoluteActionRows.Count > j + 1)
                        {
                            nextUsedSequence = ((WixActionRow)absoluteActionRows[j + 1]).Sequence;
                        }
                        else
                        {
                            nextUsedSequence = short.MaxValue + 1;
                        }

                        // schedule the action rows after this one
                        unusedSequence = absoluteActionRow.Sequence + 1;
                        for (int i = 0; i < allNextActionRows.Count; i++)
                        {
                            WixActionRow relativeActionRow = (WixActionRow)allNextActionRows[i];

                            if (unusedSequence == nextUsedSequence)
                            {
                                this.OnMessage(WixErrors.NoUniqueActionSequenceNumber(relativeActionRow.SourceLineNumbers, relativeActionRow.SequenceTable.ToString(), relativeActionRow.Action, absoluteActionRow.Action));
                                if (null != absoluteActionRow.SourceLineNumbers)
                                {
                                    this.OnMessage(WixErrors.NoUniqueActionSequenceNumber2(absoluteActionRow.SourceLineNumbers));
                                }

                                unusedSequence--;
                            }

                            relativeActionRow.Sequence = unusedSequence;
                            relativeActionRows.Add(relativeActionRow);

                            unusedSequence++;
                        }

                        // keep track of this sequence number as the previous used sequence number for the next iteration
                        previousUsedSequence = absoluteActionRow.Sequence;
                    }

                    // add the absolutely and relatively scheduled actions to the list of scheduled actions
                    scheduledActionRows.AddRange(absoluteActionRows);
                    scheduledActionRows.AddRange(relativeActionRows);
                }
            }

            // create the action rows for sequences that are not suppressed
            foreach (WixActionRow actionRow in scheduledActionRows)
            {
                // skip actions in suppressed sequences
                if ((this.suppressAdminSequence && (SequenceTable.AdminExecuteSequence == actionRow.SequenceTable || SequenceTable.AdminUISequence == actionRow.SequenceTable)) ||
                    (this.suppressAdvertiseSequence && SequenceTable.AdvtExecuteSequence == actionRow.SequenceTable) ||
                    (this.suppressUISequence && (SequenceTable.AdminUISequence == actionRow.SequenceTable || SequenceTable.InstallUISequence == actionRow.SequenceTable)))
                {
                    continue;
                }

                // get the table definition for the action (and ensure the proper table exists for a module)
                TableDefinition sequenceTableDefinition = null;
                switch (actionRow.SequenceTable)
                {
                    case SequenceTable.AdminExecuteSequence:
                        if (OutputType.Module == this.activeOutput.Type)
                        {
                            this.activeOutput.EnsureTable(this.tableDefinitions["AdminExecuteSequence"]);
                            sequenceTableDefinition = this.tableDefinitions["ModuleAdminExecuteSequence"];
                        }
                        else
                        {
                            sequenceTableDefinition = this.tableDefinitions["AdminExecuteSequence"];
                        }
                        break;
                    case SequenceTable.AdminUISequence:
                        if (OutputType.Module == this.activeOutput.Type)
                        {
                            this.activeOutput.EnsureTable(this.tableDefinitions["AdminUISequence"]);
                            sequenceTableDefinition = this.tableDefinitions["ModuleAdminUISequence"];
                        }
                        else
                        {
                            sequenceTableDefinition = this.tableDefinitions["AdminUISequence"];
                        }
                        break;
                    case SequenceTable.AdvtExecuteSequence:
                        if (OutputType.Module == this.activeOutput.Type)
                        {
                            this.activeOutput.EnsureTable(this.tableDefinitions["AdvtExecuteSequence"]);
                            sequenceTableDefinition = this.tableDefinitions["ModuleAdvtExecuteSequence"];
                        }
                        else
                        {
                            sequenceTableDefinition = this.tableDefinitions["AdvtExecuteSequence"];
                        }
                        break;
                    case SequenceTable.InstallExecuteSequence:
                        if (OutputType.Module == this.activeOutput.Type)
                        {
                            this.activeOutput.EnsureTable(this.tableDefinitions["InstallExecuteSequence"]);
                            sequenceTableDefinition = this.tableDefinitions["ModuleInstallExecuteSequence"];
                        }
                        else
                        {
                            sequenceTableDefinition = this.tableDefinitions["InstallExecuteSequence"];
                        }
                        break;
                    case SequenceTable.InstallUISequence:
                        if (OutputType.Module == this.activeOutput.Type)
                        {
                            this.activeOutput.EnsureTable(this.tableDefinitions["InstallUISequence"]);
                            sequenceTableDefinition = this.tableDefinitions["ModuleInstallUISequence"];
                        }
                        else
                        {
                            sequenceTableDefinition = this.tableDefinitions["InstallUISequence"];
                        }
                        break;
                }

                // create the action sequence row in the output
                Table sequenceTable = this.activeOutput.EnsureTable(sequenceTableDefinition);
                Row row = sequenceTable.CreateRow(actionRow.SourceLineNumbers);
                if (this.sectionIdOnRows)
                {
                    row.SectionId = actionRow.SectionId;
                }

                if (OutputType.Module == this.activeOutput.Type)
                {
                    row[0] = actionRow.Action;
                    if (0 != actionRow.Sequence)
                    {
                        row[1] = actionRow.Sequence;
                    }
                    else
                    {
                        bool after = (null == actionRow.Before);
                        row[2] = after ? actionRow.After : actionRow.Before;
                        row[3] = after ? 1 : 0;
                    }
                    row[4] = actionRow.Condition;
                }
                else
                {
                    row[0] = actionRow.Action;
                    row[1] = actionRow.Condition;
                    row[2] = actionRow.Sequence;
                }
            }
        }

        /// <summary>
        /// Resolve features for columns that have null guid placeholders.
        /// </summary>
        /// <param name="rows">Rows to resolve.</param>
        /// <param name="connectionColumn">Number of the column containing the connection identifier.</param>
        /// <param name="featureColumn">Number of the column containing the feature.</param>
        /// <param name="connectToFeatures">Connect to feature complex references.</param>
        /// <param name="multipleFeatureComponents">Hashtable of known components under multiple features.</param>
        private void ResolveFeatures(RowCollection rows, int connectionColumn, int featureColumn, ConnectToFeatureCollection connectToFeatures, Hashtable multipleFeatureComponents)
        {
            foreach (Row row in rows)
            {
                string connectionId = (string)row[connectionColumn];
                string featureId = (string)row[featureColumn];

                if (emptyGuid == featureId)
                {
                    ConnectToFeature connection = connectToFeatures[connectionId];

                    if (null == connection)
                    {
                        // display an error for the component or merge module as approrpriate
                        if (null != multipleFeatureComponents)
                        {
                            this.OnMessage(WixErrors.ComponentExpectedFeature(row.SourceLineNumbers, connectionId, row.Table.Name, row.GetPrimaryKey('/')));
                        }
                        else
                        {
                            this.OnMessage(WixErrors.MergeModuleExpectedFeature(row.SourceLineNumbers, connectionId));
                        }
                    }
                    else
                    {
                        // check for unique, implicit, primary feature parents with multiple possible parent features
                        if (this.showPedanticMessages &&
                            !connection.IsExplicitPrimaryFeature &&
                            0 < connection.ConnectFeatures.Count)
                        {
                            // display a warning for the component or merge module as approrpriate
                            if (null != multipleFeatureComponents)
                            {
                                if (!multipleFeatureComponents.Contains(connectionId))
                                {
                                    this.OnMessage(WixWarnings.ImplicitComponentPrimaryFeature(connectionId));

                                    // remember this component so only one warning is generated for it
                                    multipleFeatureComponents[connectionId] = null;
                                }
                            }
                            else
                            {
                                this.OnMessage(WixWarnings.ImplicitMergeModulePrimaryFeature(connectionId));
                            }
                        }

                        // set the feature
                        row[featureColumn] = connection.PrimaryFeature;
                    }
                }
            }
        }
    }
}
