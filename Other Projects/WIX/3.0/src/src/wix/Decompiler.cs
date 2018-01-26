//-------------------------------------------------------------------------------------------------
// <copyright file="Decompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Decompiles an msi database into WiX source.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using Microsoft.Tools.WindowsInstallerXml.Msi;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;
    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// Decompiles an msi database into WiX source.
    /// </summary>
    public class Decompiler
    {
        private static readonly Regex NullSplitter = new Regex(@"\[~]");

        private int codepage;
        private bool compressed;
        private bool shortNames;
        private DecompilerCore core;
        private ArrayList extensions;
        private Hashtable extensionsByTableName;
        private string modularizationGuid;
        private OutputType outputType;
        private Hashtable patchTargetFiles;
        private Hashtable sequenceElements;
        private bool showPedanticMessages;
        private WixActionRowCollection standardActions;
        private bool suppressDroppingEmptyTables;
        private bool suppressRelativeActionSequencing;
        private bool suppressUI;
        private TableDefinitionCollection tableDefinitions;
        private TempFileCollection tempFiles;

        /// <summary>
        /// Creates a new decompiler object with a default set of table definitions.
        /// </summary>
        public Decompiler()
        {
            this.standardActions = Installer.GetStandardActions();

            this.extensions = new ArrayList();
            this.extensionsByTableName = new Hashtable();
            this.patchTargetFiles = new Hashtable();
            this.sequenceElements = new Hashtable();
            this.tableDefinitions = new TableDefinitionCollection();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

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
        /// Gets or sets the option to suppress dropping empty tables.
        /// </summary>
        /// <value>The option to suppress dropping empty tables.</value>
        public bool SuppressDroppingEmptyTables
        {
            get { return this.suppressDroppingEmptyTables; }
            set { this.suppressDroppingEmptyTables = value; }
        }

        /// <summary>
        /// Gets or sets the option to suppress decompiling with relative action sequencing (uses sequence numbers).
        /// </summary>
        /// <value>The option to suppress decompiling with relative action sequencing (uses sequence numbers).</value>
        public bool SuppressRelativeActionSequencing
        {
            get { return this.suppressRelativeActionSequencing; }
            set { this.suppressRelativeActionSequencing = value; }
        }

        /// <summary>
        /// Gets or sets the option to suppress decompiling UI-related tables.
        /// </summary>
        /// <value>The option to suppress decompiling UI-related tables.</value>
        public bool SuppressUI
        {
            get { return this.suppressUI; }
            set { this.suppressUI = value; }
        }

        /// <summary>
        /// Gets or sets the temporary path for the Decompiler.  If left null, the decompiler
        /// will use %TEMP% environment variable.
        /// </summary>
        /// <value>Path to temp files.</value>
        public string TempFilesLocation
        {
            get
            {
                return null == this.tempFiles ? String.Empty : this.tempFiles.BasePath;
            }

            set
            {
                if (null == value)
                {
                    this.tempFiles = new TempFileCollection();
                }
                else
                {
                    this.tempFiles = new TempFileCollection(value);
                }
            }
        }

        /// <summary>
        /// Decompile the database file.
        /// </summary>
        /// <param name="output">The ouput to decompile.</param>
        /// <returns>The serialized WiX source code.</returns>
        public Wix.Wix Decompile(Output output)
        {
            if (null == output)
            {
                throw new ArgumentNullException("output");
            }

            this.codepage = output.Codepage;
            this.outputType = output.Type;

            // collect the table definitions from the output
            this.tableDefinitions.Clear();
            foreach (Table table in output.Tables)
            {
                this.tableDefinitions.Add(table.Definition);
            }

            // add any missing standard and wix-specific table definitions
            foreach (TableDefinition tableDefinition in Installer.GetTableDefinitions())
            {
                if (!this.tableDefinitions.Contains(tableDefinition.Name))
                {
                    this.tableDefinitions.Add(tableDefinition);
                }
            }

            // add any missing extension table definitions
            foreach (WixExtension extension in this.extensions)
            {
                if (null != extension.TableDefinitions)
                {
                    foreach (TableDefinition tableDefinition in extension.TableDefinitions)
                    {
                        if (!this.tableDefinitions.Contains(tableDefinition.Name))
                        {
                            this.tableDefinitions.Add(tableDefinition);
                        }
                    }
                }
            }

            // if we don't have the temporary files object yet, get one
            if (null == this.tempFiles)
            {
                this.TempFilesLocation = null;
            }
            Directory.CreateDirectory(this.tempFiles.BasePath); // ensure the base path is there

            bool encounteredError = false;
            Wix.IParentElement rootElement;
            Wix.Wix wixElement = new Wix.Wix();

            switch (this.outputType)
            {
                case OutputType.Module:
                    rootElement = new Wix.Module();
                    break;
                case OutputType.PatchCreation:
                    rootElement = new Wix.PatchCreation();
                    break;
                case OutputType.Product:
                    rootElement = new Wix.Product();
                    break;
                default:
                    throw new InvalidOperationException("Unknown output type.");
            }
            wixElement.AddChild((Wix.ISchemaElement)rootElement);

            // try to decompile the database file
            try
            {
                this.core = new DecompilerCore(rootElement, this.Message);
                this.core.ShowPedanticMessages = this.showPedanticMessages;

                // stop processing if an error previously occurred
                if (this.core.EncounteredError)
                {
                    return null;
                }

                // initialize the decompiler and its extensions
                foreach (WixExtension extension in this.extensions)
                {
                    if (null != extension.DecompilerExtension)
                    {
                        extension.DecompilerExtension.Core = this.core;
                        extension.DecompilerExtension.InitializeDecompile(output.Tables);
                    }
                }
                this.InitializeDecompile(output.Tables);

                // stop processing if an error previously occurred
                if (this.core.EncounteredError)
                {
                    return null;
                }

                // decompile the tables
                this.DecompileTables(output);

                // finalize the decompiler and its extensions
                this.FinalizeDecompile(output.Tables);
                foreach (WixExtension extension in this.extensions)
                {
                    if (null != extension.DecompilerExtension)
                    {
                        extension.DecompilerExtension.FinalizeDecompile(output.Tables);
                    }
                }
            }
            finally
            {
                encounteredError = this.core.EncounteredError;

                this.core = null;
                foreach (WixExtension extension in this.extensions)
                {
                    if (null != extension.DecompilerExtension)
                    {
                        extension.DecompilerExtension.Core = null;
                    }
                }
            }

            // return the root element only if decompilation completed successfully
            return (encounteredError ? null : wixElement);
        }

        /// <summary>
        /// Adds an extension.
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        public void AddExtension(WixExtension extension)
        {
            this.extensions.Add(extension);

            if (null != extension.DecompilerExtension)
            {
                if (null != extension.TableDefinitions)
                {
                    foreach (TableDefinition tableDefinition in extension.TableDefinitions)
                    {
                        if (!this.extensionsByTableName.Contains(tableDefinition.Name))
                        {
                            this.extensionsByTableName.Add(tableDefinition.Name, extension.DecompilerExtension);
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
        /// Cleans up the temp files used by the Decompiler.
        /// </summary>
        /// <returns>True if all files were deleted, false otherwise.</returns>
        /// <remarks>
        /// This should be called after every call to Decompile to ensure there
        /// are no conflicts between each decompiled database.
        /// </remarks>
        public bool DeleteTempFiles()
        {
            if (null == this.tempFiles)
            {
                return true; // no work to do
            }
            else
            {
                bool deleted = Common.DeleteTempFiles(this.tempFiles.BasePath, this.core);

                if (deleted)
                {
                    this.tempFiles = null; // temp files have been deleted, no need to remember this now
                }

                return deleted;
            }
        }

        /// <summary>
        /// Set the common control attributes in a control element.
        /// </summary>
        /// <param name="attributes">The control attributes.</param>
        /// <param name="control">The control element.</param>
        private static void SetControlAttributes(int attributes, Wix.Control control)
        {
            if (0 == (attributes & MsiInterop.MsidbControlAttributesEnabled))
            {
                control.Disabled = Wix.YesNoType.yes;
            }

            if (MsiInterop.MsidbControlAttributesIndirect == (attributes & MsiInterop.MsidbControlAttributesIndirect))
            {
                control.Indirect = Wix.YesNoType.yes;
            }

            if (MsiInterop.MsidbControlAttributesInteger == (attributes & MsiInterop.MsidbControlAttributesInteger))
            {
                control.Integer = Wix.YesNoType.yes;
            }

            if (MsiInterop.MsidbControlAttributesLeftScroll == (attributes & MsiInterop.MsidbControlAttributesLeftScroll))
            {
                control.LeftScroll = Wix.YesNoType.yes;
            }

            if (MsiInterop.MsidbControlAttributesRightAligned == (attributes & MsiInterop.MsidbControlAttributesRightAligned))
            {
                control.RightAligned = Wix.YesNoType.yes;
            }

            if (MsiInterop.MsidbControlAttributesRTLRO == (attributes & MsiInterop.MsidbControlAttributesRTLRO))
            {
                control.RightToLeft = Wix.YesNoType.yes;
            }

            if (MsiInterop.MsidbControlAttributesSunken == (attributes & MsiInterop.MsidbControlAttributesSunken))
            {
                control.Sunken = Wix.YesNoType.yes;
            }

            if (0 == (attributes & MsiInterop.MsidbControlAttributesVisible))
            {
                control.Hidden = Wix.YesNoType.yes;
            }
        }

        /// <summary>
        /// Creates an action element.
        /// </summary>
        /// <param name="actionRow">The action row from which the element should be created.</param>
        private void CreateActionElement(WixActionRow actionRow)
        {
            Wix.ISchemaElement actionElement = null;

            if (null != this.core.GetIndexedElement("CustomAction", actionRow.Action)) // custom action
            {
                Wix.Custom custom = new Wix.Custom();

                custom.Action = actionRow.Action;

                if (null != actionRow.Condition)
                {
                    custom.Content = actionRow.Condition;
                }

                switch (actionRow.Sequence)
                {
                    case (-4):
                        custom.OnExit = Wix.Custom.OnExitType.suspend;
                        break;
                    case (-3):
                        custom.OnExit = Wix.Custom.OnExitType.error;
                        break;
                    case (-2):
                        custom.OnExit = Wix.Custom.OnExitType.cancel;
                        break;
                    case (-1):
                        custom.OnExit = Wix.Custom.OnExitType.success;
                        break;
                    default:
                        if (null != actionRow.Before)
                        {
                            custom.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            custom.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            custom.Sequence = actionRow.Sequence;
                        }
                        break;
                }

                actionElement = custom;
            }
            else if (null != this.core.GetIndexedElement("Dialog", actionRow.Action)) // dialog
            {
                Wix.Show show = new Wix.Show();

                show.Dialog = actionRow.Action;

                if (null != actionRow.Condition)
                {
                    show.Content = actionRow.Condition;
                }

                switch (actionRow.Sequence)
                {
                    case (-4):
                        show.OnExit = Wix.Show.OnExitType.suspend;
                        break;
                    case (-3):
                        show.OnExit = Wix.Show.OnExitType.error;
                        break;
                    case (-2):
                        show.OnExit = Wix.Show.OnExitType.cancel;
                        break;
                    case (-1):
                        show.OnExit = Wix.Show.OnExitType.success;
                        break;
                    default:
                        if (null != actionRow.Before)
                        {
                            show.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            show.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            show.Sequence = actionRow.Sequence;
                        }
                        break;
                }

                actionElement = show;
            }
            else // possibly a standard action without suggested sequence information
            {
                switch (actionRow.Action)
                {
                    case "AllocateRegistrySpace":
                        Wix.AllocateRegistrySpace allocateRegistrySpace = new Wix.AllocateRegistrySpace();

                        if (null != actionRow.Condition)
                        {
                            allocateRegistrySpace.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            allocateRegistrySpace.Sequence = actionRow.Sequence;
                        }

                        actionElement = allocateRegistrySpace;
                        break;
                    case "AppSearch":
                        WixActionRow appSearchActionRow = this.standardActions[actionRow.SequenceTable, actionRow.Action];

                        if (null != actionRow.Before || null != actionRow.After || (null != appSearchActionRow && actionRow.Sequence != appSearchActionRow.Sequence))
                        {
                            Wix.AppSearch appSearch = new Wix.AppSearch();

                            if (null != actionRow.Condition)
                            {
                                appSearch.Content = actionRow.Condition;
                            }

                            if (null != actionRow.Before)
                            {
                                appSearch.Before = actionRow.Before;
                            }
                            else if (null != actionRow.After)
                            {
                                appSearch.After = actionRow.After;
                            }
                            else if (0 < actionRow.Sequence)
                            {
                                appSearch.Sequence = actionRow.Sequence;
                            }

                            actionElement = appSearch;
                        }
                        break;
                    case "BindImage":
                        Wix.BindImage bindImage = new Wix.BindImage();

                        if (null != actionRow.Condition)
                        {
                            bindImage.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            bindImage.Sequence = actionRow.Sequence;
                        }

                        actionElement = bindImage;
                        break;
                    case "CCPSearch":
                        Wix.CCPSearch ccpSearch = new Wix.CCPSearch();

                        if (null != actionRow.Condition)
                        {
                            ccpSearch.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            ccpSearch.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            ccpSearch.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            ccpSearch.Sequence = actionRow.Sequence;
                        }

                        actionElement = ccpSearch;
                        break;
                    case "CostFinalize":
                        Wix.CostFinalize costFinalize = new Wix.CostFinalize();

                        if (null != actionRow.Condition)
                        {
                            costFinalize.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            costFinalize.Sequence = actionRow.Sequence;
                        }

                        actionElement = costFinalize;
                        break;
                    case "CostInitialize":
                        Wix.CostInitialize costInitialize = new Wix.CostInitialize();

                        if (null != actionRow.Condition)
                        {
                            costInitialize.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            costInitialize.Sequence = actionRow.Sequence;
                        }

                        actionElement = costInitialize;
                        break;
                    case "CreateFolders":
                        Wix.CreateFolders createFolders = new Wix.CreateFolders();

                        if (null != actionRow.Condition)
                        {
                            createFolders.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            createFolders.Sequence = actionRow.Sequence;
                        }

                        actionElement = createFolders;
                        break;
                    case "CreateShortcuts":
                        Wix.CreateShortcuts createShortcuts = new Wix.CreateShortcuts();

                        if (null != actionRow.Condition)
                        {
                            createShortcuts.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            createShortcuts.Sequence = actionRow.Sequence;
                        }

                        actionElement = createShortcuts;
                        break;
                    case "DeleteServices":
                        Wix.DeleteServices deleteServices = new Wix.DeleteServices();

                        if (null != actionRow.Condition)
                        {
                            deleteServices.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            deleteServices.Sequence = actionRow.Sequence;
                        }

                        actionElement = deleteServices;
                        break;
                    case "DisableRollback":
                        Wix.DisableRollback disableRollback = new Wix.DisableRollback();

                        if (null != actionRow.Condition)
                        {
                            disableRollback.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            disableRollback.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            disableRollback.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            disableRollback.Sequence = actionRow.Sequence;
                        }

                        actionElement = disableRollback;
                        break;
                    case "DuplicateFiles":
                        Wix.DuplicateFiles duplicateFiles = new Wix.DuplicateFiles();

                        if (null != actionRow.Condition)
                        {
                            duplicateFiles.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            duplicateFiles.Sequence = actionRow.Sequence;
                        }

                        actionElement = duplicateFiles;
                        break;
                    case "ExecuteAction":
                        Wix.ExecuteAction executeAction = new Wix.ExecuteAction();

                        if (null != actionRow.Condition)
                        {
                            executeAction.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            executeAction.Sequence = actionRow.Sequence;
                        }

                        actionElement = executeAction;
                        break;
                    case "FileCost":
                        Wix.FileCost fileCost = new Wix.FileCost();

                        if (null != actionRow.Condition)
                        {
                            fileCost.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            fileCost.Sequence = actionRow.Sequence;
                        }

                        actionElement = fileCost;
                        break;
                    case "FindRelatedProducts":
                        Wix.FindRelatedProducts findRelatedProducts = new Wix.FindRelatedProducts();

                        if (null != actionRow.Condition)
                        {
                            findRelatedProducts.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            findRelatedProducts.Sequence = actionRow.Sequence;
                        }

                        actionElement = findRelatedProducts;
                        break;
                    case "ForceReboot":
                        Wix.ForceReboot forceReboot = new Wix.ForceReboot();

                        if (null != actionRow.Condition)
                        {
                            forceReboot.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            forceReboot.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            forceReboot.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            forceReboot.Sequence = actionRow.Sequence;
                        }

                        actionElement = forceReboot;
                        break;
                    case "InstallAdminPackage":
                        Wix.InstallAdminPackage installAdminPackage = new Wix.InstallAdminPackage();

                        if (null != actionRow.Condition)
                        {
                            installAdminPackage.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installAdminPackage.Sequence = actionRow.Sequence;
                        }

                        actionElement = installAdminPackage;
                        break;
                    case "InstallExecute":
                        Wix.InstallExecute installExecute = new Wix.InstallExecute();

                        if (null != actionRow.Condition)
                        {
                            installExecute.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            installExecute.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            installExecute.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installExecute.Sequence = actionRow.Sequence;
                        }

                        actionElement = installExecute;
                        break;
                    case "InstallExecuteAgain":
                        Wix.InstallExecuteAgain installExecuteAgain = new Wix.InstallExecuteAgain();

                        if (null != actionRow.Condition)
                        {
                            installExecuteAgain.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            installExecuteAgain.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            installExecuteAgain.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installExecuteAgain.Sequence = actionRow.Sequence;
                        }

                        actionElement = installExecuteAgain;
                        break;
                    case "InstallFiles":
                        Wix.InstallFiles installFiles = new Wix.InstallFiles();

                        if (null != actionRow.Condition)
                        {
                            installFiles.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installFiles.Sequence = actionRow.Sequence;
                        }

                        actionElement = installFiles;
                        break;
                    case "InstallFinalize":
                        Wix.InstallFinalize installFinalize = new Wix.InstallFinalize();

                        if (null != actionRow.Condition)
                        {
                            installFinalize.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installFinalize.Sequence = actionRow.Sequence;
                        }

                        actionElement = installFinalize;
                        break;
                    case "InstallInitialize":
                        Wix.InstallInitialize installInitialize = new Wix.InstallInitialize();

                        if (null != actionRow.Condition)
                        {
                            installInitialize.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installInitialize.Sequence = actionRow.Sequence;
                        }

                        actionElement = installInitialize;
                        break;
                    case "InstallODBC":
                        Wix.InstallODBC installODBC = new Wix.InstallODBC();

                        if (null != actionRow.Condition)
                        {
                            installODBC.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installODBC.Sequence = actionRow.Sequence;
                        }

                        actionElement = installODBC;
                        break;
                    case "InstallServices":
                        Wix.InstallServices installServices = new Wix.InstallServices();

                        if (null != actionRow.Condition)
                        {
                            installServices.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installServices.Sequence = actionRow.Sequence;
                        }

                        actionElement = installServices;
                        break;
                    case "InstallValidate":
                        Wix.InstallValidate installValidate = new Wix.InstallValidate();

                        if (null != actionRow.Condition)
                        {
                            installValidate.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            installValidate.Sequence = actionRow.Sequence;
                        }

                        actionElement = installValidate;
                        break;
                    case "IsolateComponents":
                        Wix.IsolateComponents isolateComponents = new Wix.IsolateComponents();

                        if (null != actionRow.Condition)
                        {
                            isolateComponents.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            isolateComponents.Sequence = actionRow.Sequence;
                        }

                        actionElement = isolateComponents;
                        break;
                    case "LaunchConditions":
                        Wix.LaunchConditions launchConditions = new Wix.LaunchConditions();

                        if (null != actionRow.Condition)
                        {
                            launchConditions.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            launchConditions.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            launchConditions.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            launchConditions.Sequence = actionRow.Sequence;
                        }

                        actionElement = launchConditions;
                        break;
                    case "MigrateFeatureStates":
                        Wix.MigrateFeatureStates migrateFeatureStates = new Wix.MigrateFeatureStates();

                        if (null != actionRow.Condition)
                        {
                            migrateFeatureStates.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            migrateFeatureStates.Sequence = actionRow.Sequence;
                        }

                        actionElement = migrateFeatureStates;
                        break;
                    case "MoveFiles":
                        Wix.MoveFiles moveFiles = new Wix.MoveFiles();

                        if (null != actionRow.Condition)
                        {
                            moveFiles.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            moveFiles.Sequence = actionRow.Sequence;
                        }

                        actionElement = moveFiles;
                        break;
                    case "MsiPublishAssemblies":
                        Wix.MsiPublishAssemblies msiPublishAssemblies = new Wix.MsiPublishAssemblies();

                        if (null != actionRow.Condition)
                        {
                            msiPublishAssemblies.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            msiPublishAssemblies.Sequence = actionRow.Sequence;
                        }

                        actionElement = msiPublishAssemblies;
                        break;
                    case "MsiUnpublishAssemblies":
                        Wix.MsiUnpublishAssemblies msiUnpublishAssemblies = new Wix.MsiUnpublishAssemblies();

                        if (null != actionRow.Condition)
                        {
                            msiUnpublishAssemblies.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            msiUnpublishAssemblies.Sequence = actionRow.Sequence;
                        }

                        actionElement = msiUnpublishAssemblies;
                        break;
                    case "PatchFiles":
                        Wix.PatchFiles patchFiles = new Wix.PatchFiles();

                        if (null != actionRow.Condition)
                        {
                            patchFiles.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            patchFiles.Sequence = actionRow.Sequence;
                        }

                        actionElement = patchFiles;
                        break;
                    case "ProcessComponents":
                        Wix.ProcessComponents processComponents = new Wix.ProcessComponents();

                        if (null != actionRow.Condition)
                        {
                            processComponents.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            processComponents.Sequence = actionRow.Sequence;
                        }

                        actionElement = processComponents;
                        break;
                    case "PublishComponents":
                        Wix.PublishComponents publishComponents = new Wix.PublishComponents();

                        if (null != actionRow.Condition)
                        {
                            publishComponents.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            publishComponents.Sequence = actionRow.Sequence;
                        }

                        actionElement = publishComponents;
                        break;
                    case "PublishFeatures":
                        Wix.PublishFeatures publishFeatures = new Wix.PublishFeatures();

                        if (null != actionRow.Condition)
                        {
                            publishFeatures.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            publishFeatures.Sequence = actionRow.Sequence;
                        }

                        actionElement = publishFeatures;
                        break;
                    case "PublishProduct":
                        Wix.PublishProduct publishProduct = new Wix.PublishProduct();

                        if (null != actionRow.Condition)
                        {
                            publishProduct.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            publishProduct.Sequence = actionRow.Sequence;
                        }

                        actionElement = publishProduct;
                        break;
                    case "RegisterClassInfo":
                        Wix.RegisterClassInfo registerClassInfo = new Wix.RegisterClassInfo();

                        if (null != actionRow.Condition)
                        {
                            registerClassInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerClassInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerClassInfo;
                        break;
                    case "RegisterComPlus":
                        Wix.RegisterComPlus registerComPlus = new Wix.RegisterComPlus();

                        if (null != actionRow.Condition)
                        {
                            registerComPlus.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerComPlus.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerComPlus;
                        break;
                    case "RegisterExtensionInfo":
                        Wix.RegisterExtensionInfo registerExtensionInfo = new Wix.RegisterExtensionInfo();

                        if (null != actionRow.Condition)
                        {
                            registerExtensionInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerExtensionInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerExtensionInfo;
                        break;
                    case "RegisterFonts":
                        Wix.RegisterFonts registerFonts = new Wix.RegisterFonts();

                        if (null != actionRow.Condition)
                        {
                            registerFonts.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerFonts.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerFonts;
                        break;
                    case "RegisterMIMEInfo":
                        Wix.RegisterMIMEInfo registerMIMEInfo = new Wix.RegisterMIMEInfo();

                        if (null != actionRow.Condition)
                        {
                            registerMIMEInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerMIMEInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerMIMEInfo;
                        break;
                    case "RegisterProduct":
                        Wix.RegisterProduct registerProduct = new Wix.RegisterProduct();

                        if (null != actionRow.Condition)
                        {
                            registerProduct.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerProduct.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerProduct;
                        break;
                    case "RegisterProgIdInfo":
                        Wix.RegisterProgIdInfo registerProgIdInfo = new Wix.RegisterProgIdInfo();

                        if (null != actionRow.Condition)
                        {
                            registerProgIdInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerProgIdInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerProgIdInfo;
                        break;
                    case "RegisterTypeLibraries":
                        Wix.RegisterTypeLibraries registerTypeLibraries = new Wix.RegisterTypeLibraries();

                        if (null != actionRow.Condition)
                        {
                            registerTypeLibraries.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerTypeLibraries.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerTypeLibraries;
                        break;
                    case "RegisterUser":
                        Wix.RegisterUser registerUser = new Wix.RegisterUser();

                        if (null != actionRow.Condition)
                        {
                            registerUser.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            registerUser.Sequence = actionRow.Sequence;
                        }

                        actionElement = registerUser;
                        break;
                    case "RemoveDuplicateFiles":
                        Wix.RemoveDuplicateFiles removeDuplicateFiles = new Wix.RemoveDuplicateFiles();

                        if (null != actionRow.Condition)
                        {
                            removeDuplicateFiles.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeDuplicateFiles.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeDuplicateFiles;
                        break;
                    case "RemoveEnvironmentStrings":
                        Wix.RemoveEnvironmentStrings removeEnvironmentStrings = new Wix.RemoveEnvironmentStrings();

                        if (null != actionRow.Condition)
                        {
                            removeEnvironmentStrings.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeEnvironmentStrings.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeEnvironmentStrings;
                        break;
                    case "RemoveExistingProducts":
                        Wix.RemoveExistingProducts removeExistingProducts = new Wix.RemoveExistingProducts();

                        if (null != actionRow.Condition)
                        {
                            removeExistingProducts.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            removeExistingProducts.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            removeExistingProducts.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeExistingProducts.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeExistingProducts;
                        break;
                    case "RemoveFiles":
                        Wix.RemoveFiles removeFiles = new Wix.RemoveFiles();

                        if (null != actionRow.Condition)
                        {
                            removeFiles.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeFiles.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeFiles;
                        break;
                    case "RemoveFolders":
                        Wix.RemoveFolders removeFolders = new Wix.RemoveFolders();

                        if (null != actionRow.Condition)
                        {
                            removeFolders.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeFolders.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeFolders;
                        break;
                    case "RemoveIniValues":
                        Wix.RemoveIniValues removeIniValues = new Wix.RemoveIniValues();

                        if (null != actionRow.Condition)
                        {
                            removeIniValues.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeIniValues.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeIniValues;
                        break;
                    case "RemoveODBC":
                        Wix.RemoveODBC removeODBC = new Wix.RemoveODBC();

                        if (null != actionRow.Condition)
                        {
                            removeODBC.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeODBC.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeODBC;
                        break;
                    case "RemoveRegistryValues":
                        Wix.RemoveRegistryValues removeRegistryValues = new Wix.RemoveRegistryValues();

                        if (null != actionRow.Condition)
                        {
                            removeRegistryValues.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeRegistryValues.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeRegistryValues;
                        break;
                    case "RemoveShortcuts":
                        Wix.RemoveShortcuts removeShortcuts = new Wix.RemoveShortcuts();

                        if (null != actionRow.Condition)
                        {
                            removeShortcuts.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            removeShortcuts.Sequence = actionRow.Sequence;
                        }

                        actionElement = removeShortcuts;
                        break;
                    case "ResolveSource":
                        Wix.ResolveSource resolveSource = new Wix.ResolveSource();

                        if (null != actionRow.Condition)
                        {
                            resolveSource.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            resolveSource.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            resolveSource.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            resolveSource.Sequence = actionRow.Sequence;
                        }

                        actionElement = resolveSource;
                        break;
                    case "RMCCPSearch":
                        Wix.RMCCPSearch rmccpSearch = new Wix.RMCCPSearch();

                        if (null != actionRow.Condition)
                        {
                            rmccpSearch.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            rmccpSearch.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            rmccpSearch.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            rmccpSearch.Sequence = actionRow.Sequence;
                        }

                        actionElement = rmccpSearch;
                        break;
                    case "ScheduleReboot":
                        Wix.ScheduleReboot scheduleReboot = new Wix.ScheduleReboot();

                        if (null != actionRow.Condition)
                        {
                            scheduleReboot.Content = actionRow.Condition;
                        }

                        if (null != actionRow.Before)
                        {
                            scheduleReboot.Before = actionRow.Before;
                        }
                        else if (null != actionRow.After)
                        {
                            scheduleReboot.After = actionRow.After;
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            scheduleReboot.Sequence = actionRow.Sequence;
                        }

                        actionElement = scheduleReboot;
                        break;
                    case "SelfRegModules":
                        Wix.SelfRegModules selfRegModules = new Wix.SelfRegModules();

                        if (null != actionRow.Condition)
                        {
                            selfRegModules.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            selfRegModules.Sequence = actionRow.Sequence;
                        }

                        actionElement = selfRegModules;
                        break;
                    case "SelfUnregModules":
                        Wix.SelfUnregModules selfUnregModules = new Wix.SelfUnregModules();

                        if (null != actionRow.Condition)
                        {
                            selfUnregModules.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            selfUnregModules.Sequence = actionRow.Sequence;
                        }

                        actionElement = selfUnregModules;
                        break;
                    case "SetODBCFolders":
                        Wix.SetODBCFolders setODBCFolders = new Wix.SetODBCFolders();

                        if (null != actionRow.Condition)
                        {
                            setODBCFolders.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            setODBCFolders.Sequence = actionRow.Sequence;
                        }

                        actionElement = setODBCFolders;
                        break;
                    case "StartServices":
                        Wix.StartServices startServices = new Wix.StartServices();

                        if (null != actionRow.Condition)
                        {
                            startServices.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            startServices.Sequence = actionRow.Sequence;
                        }

                        actionElement = startServices;
                        break;
                    case "StopServices":
                        Wix.StopServices stopServices = new Wix.StopServices();

                        if (null != actionRow.Condition)
                        {
                            stopServices.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            stopServices.Sequence = actionRow.Sequence;
                        }

                        actionElement = stopServices;
                        break;
                    case "UnpublishComponents":
                        Wix.UnpublishComponents unpublishComponents = new Wix.UnpublishComponents();

                        if (null != actionRow.Condition)
                        {
                            unpublishComponents.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unpublishComponents.Sequence = actionRow.Sequence;
                        }

                        actionElement = unpublishComponents;
                        break;
                    case "UnpublishFeatures":
                        Wix.UnpublishFeatures unpublishFeatures = new Wix.UnpublishFeatures();

                        if (null != actionRow.Condition)
                        {
                            unpublishFeatures.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unpublishFeatures.Sequence = actionRow.Sequence;
                        }

                        actionElement = unpublishFeatures;
                        break;
                    case "UnregisterClassInfo":
                        Wix.UnregisterClassInfo unregisterClassInfo = new Wix.UnregisterClassInfo();

                        if (null != actionRow.Condition)
                        {
                            unregisterClassInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterClassInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterClassInfo;
                        break;
                    case "UnregisterComPlus":
                        Wix.UnregisterComPlus unregisterComPlus = new Wix.UnregisterComPlus();

                        if (null != actionRow.Condition)
                        {
                            unregisterComPlus.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterComPlus.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterComPlus;
                        break;
                    case "UnregisterExtensionInfo":
                        Wix.UnregisterExtensionInfo unregisterExtensionInfo = new Wix.UnregisterExtensionInfo();

                        if (null != actionRow.Condition)
                        {
                            unregisterExtensionInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterExtensionInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterExtensionInfo;
                        break;
                    case "UnregisterFonts":
                        Wix.UnregisterFonts unregisterFonts = new Wix.UnregisterFonts();

                        if (null != actionRow.Condition)
                        {
                            unregisterFonts.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterFonts.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterFonts;
                        break;
                    case "UnregisterMIMEInfo":
                        Wix.UnregisterMIMEInfo unregisterMIMEInfo = new Wix.UnregisterMIMEInfo();

                        if (null != actionRow.Condition)
                        {
                            unregisterMIMEInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterMIMEInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterMIMEInfo;
                        break;
                    case "UnregisterProgIdInfo":
                        Wix.UnregisterProgIdInfo unregisterProgIdInfo = new Wix.UnregisterProgIdInfo();

                        if (null != actionRow.Condition)
                        {
                            unregisterProgIdInfo.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterProgIdInfo.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterProgIdInfo;
                        break;
                    case "UnregisterTypeLibraries":
                        Wix.UnregisterTypeLibraries unregisterTypeLibraries = new Wix.UnregisterTypeLibraries();

                        if (null != actionRow.Condition)
                        {
                            unregisterTypeLibraries.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            unregisterTypeLibraries.Sequence = actionRow.Sequence;
                        }

                        actionElement = unregisterTypeLibraries;
                        break;
                    case "ValidateProductID":
                        Wix.ValidateProductID validateProductID = new Wix.ValidateProductID();

                        if (null != actionRow.Condition)
                        {
                            validateProductID.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            validateProductID.Sequence = actionRow.Sequence;
                        }

                        actionElement = validateProductID;
                        break;
                    case "WriteEnvironmentStrings":
                        Wix.WriteEnvironmentStrings writeEnvironmentStrings = new Wix.WriteEnvironmentStrings();

                        if (null != actionRow.Condition)
                        {
                            writeEnvironmentStrings.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            writeEnvironmentStrings.Sequence = actionRow.Sequence;
                        }

                        actionElement = writeEnvironmentStrings;
                        break;
                    case "WriteIniValues":
                        Wix.WriteIniValues writeIniValues = new Wix.WriteIniValues();

                        if (null != actionRow.Condition)
                        {
                            writeIniValues.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            writeIniValues.Sequence = actionRow.Sequence;
                        }

                        actionElement = writeIniValues;
                        break;
                    case "WriteRegistryValues":
                        Wix.WriteRegistryValues writeRegistryValues = new Wix.WriteRegistryValues();

                        if (null != actionRow.Condition)
                        {
                            writeRegistryValues.Content = actionRow.Condition;
                        }

                        if ((null != actionRow.Before || null != actionRow.After) && 0 == actionRow.Sequence)
                        {
                            this.core.OnMessage(WixWarnings.DecompiledStandardActionRelativelyScheduledInModule(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        }
                        else if (0 < actionRow.Sequence)
                        {
                            writeRegistryValues.Sequence = actionRow.Sequence;
                        }

                        actionElement = writeRegistryValues;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.UnknownAction(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                        break;
                }
            }

            // add the action element to the appropriate sequence element
            if (null != actionElement)
            {
                string sequenceTable = actionRow.SequenceTable.ToString();
                Wix.IParentElement sequenceElement = (Wix.IParentElement)this.sequenceElements[sequenceTable];

                if (null == sequenceElement)
                {
                    switch (actionRow.SequenceTable)
                    {
                        case SequenceTable.AdminExecuteSequence:
                            sequenceElement = new Wix.AdminExecuteSequence();
                            break;
                        case SequenceTable.AdminUISequence:
                            sequenceElement = new Wix.AdminUISequence();
                            break;
                        case SequenceTable.AdvtExecuteSequence:
                            sequenceElement = new Wix.AdvertiseExecuteSequence();
                            break;
                        case SequenceTable.InstallExecuteSequence:
                            sequenceElement = new Wix.InstallExecuteSequence();
                            break;
                        case SequenceTable.InstallUISequence:
                            sequenceElement = new Wix.InstallUISequence();
                            break;
                        default:
                            throw new InvalidOperationException("Unknown sequence table.");
                    }

                    this.core.RootElement.AddChild((Wix.ISchemaElement)sequenceElement);
                    this.sequenceElements.Add(sequenceTable, sequenceElement);
                }

                try
                {
                    sequenceElement.AddChild(actionElement);
                }
                catch (System.ArgumentException) // action/dialog is not valid for this sequence
                {
                    this.core.OnMessage(WixWarnings.IllegalActionInSequence(actionRow.SourceLineNumbers, actionRow.SequenceTable.ToString(), actionRow.Action));
                }
            }
        }

        /// <summary>
        /// Ensure that a particular property exists in the decompiled output.
        /// </summary>
        /// <param name="id">The identifier of the property.</param>
        /// <returns>The property element.</returns>
        private Wix.Property EnsureProperty(string id)
        {
            Wix.Property property = (Wix.Property)this.core.GetIndexedElement("Property", id);

            if (null == property)
            {
                property = new Wix.Property();
                property.Id = id;

                // create a dummy row for indexing
                Row row = new Row(null, this.tableDefinitions["Property"]);
                row[0] = id;

                this.core.RootElement.AddChild(property);
                this.core.IndexElement(row, property);
            }

            return property;
        }

        /// <summary>
        /// Finalize decompilation.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        private void FinalizeDecompile(TableCollection tables)
        {
            if (OutputType.PatchCreation == this.outputType)
            {
                this.FinalizeFamilyFileRangesTable(tables);
            }
            else
            {
                this.FinalizeCheckBoxTable(tables);
                this.FinalizeComponentTable(tables);
                this.FinalizeDialogTable(tables);
                this.FinalizeDuplicateMoveFileTables(tables);
                this.FinalizeFeatureComponentsTable(tables);
                this.FinalizeFileTable(tables);
                this.FinalizeMIMETable(tables);
                this.FinalizeLockPermissionsTable(tables);
                this.FinalizeProgIdTable(tables);
                this.FinalizeRemoveFileTable(tables);
                this.FinalizeSearchTables(tables);
                this.FinalizeSequenceTables(tables);
                this.FinalizeVerbTable(tables);
            }
        }

        /// <summary>
        /// Finalize the CheckBox table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Enumerates through all the Control rows, looking for controls of type "CheckBox" with
        /// a value in the Property column.  This is then possibly matched up with a CheckBox row
        /// to retrieve a CheckBoxValue.  There is no foreign key from the Control to CheckBox table.
        /// </remarks>
        private void FinalizeCheckBoxTable(TableCollection tables)
        {
            Table checkBoxTable = tables["CheckBox"];
            Table controlTable = tables["Control"];

            Hashtable checkBoxes = new Hashtable();

            // index the CheckBox table
            if (null != checkBoxTable)
            {
                foreach (Row row in checkBoxTable.Rows)
                {
                    checkBoxes.Add(row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), row);
                }
            }

            // enumerate through the Control table, adding CheckBox values where appropriate
            if (null != controlTable)
            {
                foreach (Row row in controlTable.Rows)
                {
                    Wix.Control control = (Wix.Control)this.core.GetIndexedElement(row);

                    if ("CheckBox" == (string)row[2] && null != row[8])
                    {
                        Row checkBoxRow = (Row)checkBoxes[row[8]];

                        if (null == checkBoxRow)
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Control", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Property", (string)row[8], "CheckBox"));
                        }
                        else if (null != checkBoxRow[1])
                        {
                            control.CheckBoxValue = (string)checkBoxRow[1];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the Component table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Set the keypaths for each component.
        /// </remarks>
        private void FinalizeComponentTable(TableCollection tables)
        {
            Table componentTable = tables["Component"];
            Table fileTable = tables["File"];
            Table odbcDataSourceTable = tables["ODBCDataSource"];
            Table registryTable = tables["Registry"];

            // set the component keypaths
            if (null != componentTable)
            {
                foreach (Row row in componentTable.Rows)
                {
                    int attributes = (int)row[3];

                    if (null == row[5])
                    {
                        Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[0]);

                        component.KeyPath = Wix.YesNoType.yes;
                    }
                    else if (MsiInterop.MsidbComponentAttributesRegistryKeyPath == (attributes & MsiInterop.MsidbComponentAttributesRegistryKeyPath))
                    {
                        object registryObject = this.core.GetIndexedElement("Registry", (string)row[5]);

                        if (null != registryObject)
                        {
                            Wix.RegistryValue registryValue = registryObject as Wix.RegistryValue;

                            if (null != registryValue)
                            {
                                registryValue.KeyPath = Wix.YesNoType.yes;
                            }
                            else
                            {
                                this.core.OnMessage(WixWarnings.IllegalRegistryKeyPath(row.SourceLineNumbers, "Component", (string)row[5]));
                            }
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Component", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "KeyPath", (string)row[5], "Registry"));
                        }
                    }
                    else if (MsiInterop.MsidbComponentAttributesODBCDataSource == (attributes & MsiInterop.MsidbComponentAttributesODBCDataSource))
                    {
                        Wix.ODBCDataSource odbcDataSource = (Wix.ODBCDataSource)this.core.GetIndexedElement("ODBCDataSource", (string)row[5]);

                        if (null != odbcDataSource)
                        {
                            odbcDataSource.KeyPath = Wix.YesNoType.yes;
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Component", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "KeyPath", (string)row[5], "ODBCDataSource"));
                        }
                    }
                    else
                    {
                        Wix.File file = (Wix.File)this.core.GetIndexedElement("File", (string)row[5]);

                        if (null != file)
                        {
                            file.KeyPath = Wix.YesNoType.yes;
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Component", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "KeyPath", (string)row[5], "File"));
                        }
                    }
                }
            }

            // add the File children elements
            if (null != fileTable)
            {
                foreach (FileRow fileRow in fileTable.Rows)
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", fileRow.Component);
                    Wix.File file = (Wix.File)this.core.GetIndexedElement(fileRow);

                    if (null != component)
                    {
                        component.AddChild(file);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(fileRow.SourceLineNumbers, "File", fileRow.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", fileRow.Component, "Component"));
                    }
                }
            }

            // add the ODBCDataSource children elements
            if (null != odbcDataSourceTable)
            {
                foreach (Row row in odbcDataSourceTable.Rows)
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                    Wix.ODBCDataSource odbcDataSource = (Wix.ODBCDataSource)this.core.GetIndexedElement(row);

                    if (null != component)
                    {
                        component.AddChild(odbcDataSource);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "ODBCDataSource", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                    }
                }
            }

            // add the Registry children elements
            if (null != registryTable)
            {
                foreach (Row row in registryTable.Rows)
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[5]);
                    Wix.ISchemaElement registryElement = (Wix.ISchemaElement)this.core.GetIndexedElement(row);

                    if (null != component)
                    {
                        component.AddChild(registryElement);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Registry", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[5], "Component"));
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the Dialog table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Sets the first, default, and cancel control for each dialog and adds all child control
        /// elements to the dialog.
        /// </remarks>
        private void FinalizeDialogTable(TableCollection tables)
        {
            Table controlTable = tables["Control"];
            Table dialogTable = tables["Dialog"];

            Hashtable addedControls = new Hashtable();
            Hashtable controlRows = new Hashtable();

            // index the rows in the control rows (because we need the Control_Next value)
            if (null != controlTable)
            {
                foreach (Row row in controlTable.Rows)
                {
                    controlRows.Add(row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), row);
                }
            }

            if (null != dialogTable)
            {
                foreach (Row row in dialogTable.Rows)
                {
                    Wix.Dialog dialog = (Wix.Dialog)this.core.GetIndexedElement(row);
                    string dialogId = (string)row[0];

                    Wix.Control control = (Wix.Control)this.core.GetIndexedElement("Control", dialogId, (string)row[7]);
                    if (null == control)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Dialog", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog", dialogId, "Control_First", (string)row[7], "Control"));
                    }

                    // add tabbable controls
                    while (null != control)
                    {
                        Row controlRow = (Row)controlRows[String.Concat(dialogId, DecompilerCore.PrimaryKeyDelimiter, control.Id)];

                        control.TabSkip = Wix.YesNoType.no;
                        dialog.AddChild(control);
                        addedControls.Add(control, null);

                        if (null != controlRow[10])
                        {
                            control = (Wix.Control)this.core.GetIndexedElement("Control", dialogId, (string)controlRow[10]);
                            if (null != control)
                            {
                                // looped back to the first control in the dialog
                                if (addedControls.Contains(control))
                                {
                                    control = null;
                                }
                            }
                            else
                            {
                                this.core.OnMessage(WixWarnings.ExpectedForeignRow(controlRow.SourceLineNumbers, "Control", controlRow.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog_", dialogId, "Control_Next", (string)controlRow[10], "Control"));
                            }
                        }
                        else
                        {
                            control = null;
                        }
                    }

                    // set default control
                    if (null != row[8])
                    {
                        Wix.Control defaultControl = (Wix.Control)this.core.GetIndexedElement("Control", dialogId, (string)row[8]);

                        if (null != defaultControl)
                        {
                            defaultControl.Default = Wix.YesNoType.yes;
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Dialog", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog", dialogId, "Control_Default", (string)row[8], "Control"));
                        }
                    }

                    // set cancel control
                    if (null != row[9])
                    {
                        Wix.Control cancelControl = (Wix.Control)this.core.GetIndexedElement("Control", dialogId, (string)row[9]);

                        if (null != cancelControl)
                        {
                            cancelControl.Cancel = Wix.YesNoType.yes;
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Dialog", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog", dialogId, "Control_Cancel", (string)row[9], "Control"));
                        }
                    }
                }
            }

            // add the non-tabbable controls to the dialog
            if (null != controlTable)
            {
                foreach (Row row in controlTable.Rows)
                {
                    Wix.Control control = (Wix.Control)this.core.GetIndexedElement(row);
                    Wix.Dialog dialog = (Wix.Dialog)this.core.GetIndexedElement("Dialog", (string)row[0]);

                    if (null == dialog)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Control", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog_", (string)row[0], "Dialog"));
                        continue;
                    }

                    if (!addedControls.Contains(control))
                    {
                        control.TabSkip = Wix.YesNoType.yes;
                        dialog.AddChild(control);
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the DuplicateFile and MoveFile tables.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Sets the source/destination property/directory for each DuplicateFile or
        /// MoveFile row.
        /// </remarks>
        private void FinalizeDuplicateMoveFileTables(TableCollection tables)
        {
            Table duplicateFileTable = tables["DuplicateFile"];
            Table moveFileTable = tables["MoveFile"];

            if (null != duplicateFileTable)
            {
                foreach (Row row in duplicateFileTable.Rows)
                {
                    Wix.CopyFile copyFile = (Wix.CopyFile)this.core.GetIndexedElement(row);

                    if (null != row[4])
                    {
                        if (null != this.core.GetIndexedElement("Directory", (string)row[4]))
                        {
                            copyFile.DestinationDirectory = (string)row[4];
                        }
                        else
                        {
                            copyFile.DestinationProperty = (string)row[4];
                        }
                    }
                }
            }

            if (null != moveFileTable)
            {
                foreach (Row row in moveFileTable.Rows)
                {
                    Wix.CopyFile copyFile = (Wix.CopyFile)this.core.GetIndexedElement(row);

                    if (null != row[4])
                    {
                        if (null != this.core.GetIndexedElement("Directory", (string)row[4]))
                        {
                            copyFile.SourceDirectory = (string)row[4];
                        }
                        else
                        {
                            copyFile.SourceProperty = (string)row[4];
                        }
                    }

                    if (null != this.core.GetIndexedElement("Directory", (string)row[5]))
                    {
                        copyFile.DestinationDirectory = (string)row[5];
                    }
                    else
                    {
                        copyFile.DestinationProperty = (string)row[5];
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the FamilyFileRanges table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        private void FinalizeFamilyFileRangesTable(TableCollection tables)
        {
            Table externalFilesTable = tables["ExternalFiles"];
            Table familyFileRangesTable = tables["FamilyFileRanges"];
            Table targetFiles_OptionalDataTable = tables["TargetFiles_OptionalData"];

            Hashtable usedProtectRanges = new Hashtable();

            if (null != familyFileRangesTable)
            {
                foreach (Row row in familyFileRangesTable.Rows)
                {
                    Wix.ProtectRange protectRange = new Wix.ProtectRange();

                    if (null != row[2] && null != row[3])
                    {
                        string[] retainOffsets = ((string)row[2]).Split(',');
                        string[] retainLengths = ((string)row[3]).Split(',');

                        if (retainOffsets.Length == retainLengths.Length)
                        {
                            for (int i = 0; i < retainOffsets.Length; i++)
                            {
                                if (retainOffsets[i].StartsWith("0x"))
                                {
                                    protectRange.Offset = Convert.ToInt32(retainOffsets[i].Substring(2), 16);
                                }
                                else
                                {
                                    protectRange.Offset = Convert.ToInt32(retainOffsets[i], CultureInfo.InvariantCulture);
                                }

                                if (retainLengths[i].StartsWith("0x"))
                                {
                                    protectRange.Length = Convert.ToInt32(retainLengths[i].Substring(2), 16);
                                }
                                else
                                {
                                    protectRange.Length = Convert.ToInt32(retainLengths[i], CultureInfo.InvariantCulture);
                                }
                            }
                        }
                        else
                        {
                            // TODO: warn
                        }
                    }
                    else if (null != row[2] || null != row[3])
                    {
                        // TODO: warn about mismatch between columns
                    }

                    this.core.IndexElement(row, protectRange);
                }
            }

            if (null != externalFilesTable)
            {
                foreach (Row row in externalFilesTable.Rows)
                {
                    Wix.ExternalFile externalFile = (Wix.ExternalFile)this.core.GetIndexedElement(row);

                    Wix.ProtectRange protectRange = (Wix.ProtectRange)this.core.GetIndexedElement("FamilyFileRanges", (string)row[0], (string)row[1]);
                    if (null != protectRange)
                    {
                        externalFile.AddChild(protectRange);
                        usedProtectRanges[protectRange] = null;
                    }
                }
            }

            if (null != targetFiles_OptionalDataTable)
            {
                Table targetImagesTable = tables["TargetImages"];
                Table upgradedImagesTable = tables["UpgradedImages"];

                Hashtable targetImageRows = new Hashtable();
                Hashtable upgradedImagesRows = new Hashtable();

                // index the TargetImages table
                if (null != targetImagesTable)
                {
                    foreach (Row row in targetImagesTable.Rows)
                    {
                        targetImageRows.Add(row[0], row);
                    }
                }

                // index the UpgradedImages table
                if (null != upgradedImagesTable)
                {
                    foreach (Row row in upgradedImagesTable.Rows)
                    {
                        upgradedImagesRows.Add(row[0], row);
                    }
                }

                foreach (Row row in targetFiles_OptionalDataTable.Rows)
                {
                    Wix.TargetFile targetFile = (Wix.TargetFile)this.patchTargetFiles[row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter)];

                    Row targetImageRow = (Row)targetImageRows[row[0]];
                    if (null == targetImageRow)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, targetFiles_OptionalDataTable.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Target", (string)row[0], "TargetImages"));
                        continue;
                    }

                    Row upgradedImagesRow = (Row)upgradedImagesRows[targetImageRow[3]];
                    if (null == upgradedImagesRow)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(targetImageRow.SourceLineNumbers, targetImageRow.Table.Name, targetImageRow.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Upgraded", (string)row[3], "UpgradedImages"));
                        continue;
                    }

                    Wix.ProtectRange protectRange = (Wix.ProtectRange)this.core.GetIndexedElement("FamilyFileRanges", (string)upgradedImagesRow[4], (string)row[1]);
                    if (null != protectRange)
                    {
                        targetFile.AddChild(protectRange);
                        usedProtectRanges[protectRange] = null;
                    }
                }
            }

            if (null != familyFileRangesTable)
            {
                foreach (Row row in familyFileRangesTable.Rows)
                {
                    Wix.ProtectRange protectRange = (Wix.ProtectRange)this.core.GetIndexedElement(row);

                    if (!usedProtectRanges.Contains(protectRange))
                    {
                        Wix.ProtectFile protectFile = new Wix.ProtectFile();

                        protectFile.File = (string)row[1];

                        protectFile.AddChild(protectRange);

                        Wix.Family family = (Wix.Family)this.core.GetIndexedElement("ImageFamilies", (string)row[0]);
                        if (null != family)
                        {
                            family.AddChild(protectFile);
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, familyFileRangesTable.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Family", (string)row[0], "ImageFamilies"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the FeatureComponents table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Since tables specifying references to the FeatureComponents table have references to
        /// the Feature and Component table separately, but not the FeatureComponents table specifically,
        /// the FeatureComponents table and primary features must be decompiled during finalization.
        /// </remarks>
        private void FinalizeFeatureComponentsTable(TableCollection tables)
        {
            Table classTable = tables["Class"];
            Table extensionTable = tables["Extension"];
            Table msiAssemblyTable = tables["MsiAssembly"];
            Table publishComponentTable = tables["PublishComponent"];
            Table shortcutTable = tables["Shortcut"];
            Table typeLibTable = tables["TypeLib"];

            if (null != classTable)
            {
                foreach (Row row in classTable.Rows)
                {
                    this.SetPrimaryFeature(row, 11, 2);
                }
            }

            if (null != extensionTable)
            {
                foreach (Row row in extensionTable.Rows)
                {
                    this.SetPrimaryFeature(row, 4, 1);
                }
            }

            if (null != msiAssemblyTable)
            {
                foreach (Row row in msiAssemblyTable.Rows)
                {
                    this.SetPrimaryFeature(row, 1, 0);
                }
            }

            if (null != publishComponentTable)
            {
                foreach (Row row in publishComponentTable.Rows)
                {
                    this.SetPrimaryFeature(row, 4, 2);
                }
            }

            if (null != shortcutTable)
            {
                foreach (Row row in shortcutTable.Rows)
                {
                    string target = (string)row[4];

                    if (!target.StartsWith("[") && !target.EndsWith("]"))
                    {
                        this.SetPrimaryFeature(row, 4, 3);
                    }
                }
            }

            if (null != typeLibTable)
            {
                foreach (Row row in typeLibTable.Rows)
                {
                    this.SetPrimaryFeature(row, 6, 2);
                }
            }
        }

        /// <summary>
        /// Finalize the File table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Sets the source, diskId, and assembly information for each file.
        /// </remarks>
        private void FinalizeFileTable(TableCollection tables)
        {
            Table fileTable = tables["File"];
            Table mediaTable = tables["Media"];
            Table msiAssemblyTable = tables["MsiAssembly"];
            Table typeLibTable = tables["TypeLib"];

            MediaRowCollection mediaRows;

            // index the media table by media id
            if (null != mediaTable)
            {
                mediaRows = new MediaRowCollection();
                mediaRows.AddRange(mediaTable.Rows);
            }

            // set the disk identifiers and sources for files
            if (null != fileTable)
            {
                foreach (FileRow fileRow in fileTable.Rows)
                {
                    Wix.File file = (Wix.File)this.core.GetIndexedElement("File", fileRow.File);

                    // set the diskid
                    if (null != mediaTable)
                    {
                        foreach (MediaRow mediaRow in mediaTable.Rows)
                        {
                            if (fileRow.Sequence <= mediaRow.LastSequence)
                            {
                                file.DiskId = mediaRow.DiskId;
                                break;
                            }
                        }
                    }

                    // set the source (done here because it requires information from the Directory table)
                    if (OutputType.Module == this.outputType)
                    {
                        file.Source = String.Concat("SourceDir", Path.DirectorySeparatorChar, "File", Path.DirectorySeparatorChar, file.Id, '.', this.modularizationGuid.Substring(1, 36).Replace('-', '_'));
                    }
                    else if (Wix.YesNoDefaultType.yes == file.Compressed || (Wix.YesNoDefaultType.@default == file.Compressed && this.compressed))
                    {
                        file.Source = String.Concat("SourceDir", Path.DirectorySeparatorChar, "File", Path.DirectorySeparatorChar, file.Id);
                    }
                    else // uncompressed
                    {
                        string fileName = (null != file.ShortName ? file.ShortName : file.Name);

                        if (!this.shortNames && null != file.Name)
                        {
                            fileName = file.Name;
                        }

                        if (this.compressed) // uncompressed at the root of the source image
                        {
                            file.Source = String.Concat("SourceDir", Path.DirectorySeparatorChar, fileName);
                        }
                        else
                        {
                            string sourcePath = this.GetSourcePath(file);

                            file.Source = Path.Combine(sourcePath, fileName);
                        }
                    }
                }
            }

            // set the file assemblies and manifests
            if (null != msiAssemblyTable)
            {
                foreach (Row row in msiAssemblyTable.Rows)
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[0]);

                    if (null == component)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "MsiAssembly", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[0], "Component"));
                    }
                    else
                    {
                        foreach (Wix.ISchemaElement element in component.Children)
                        {
                            Wix.File file = element as Wix.File;

                            if (null != file && Wix.YesNoType.yes == file.KeyPath)
                            {
                                if (null != row[2])
                                {
                                    file.AssemblyManifest = (string)row[2];
                                }

                                if (null != row[3])
                                {
                                    file.AssemblyApplication = (string)row[3];
                                }

                                if (null == row[4] || 0 == (int)row[4])
                                {
                                    file.Assembly = Wix.File.AssemblyType.net;
                                }
                                else
                                {
                                    file.Assembly = Wix.File.AssemblyType.win32;
                                }
                            }
                        }
                    }
                }
            }

            // nest the TypeLib elements
            if (null != typeLibTable)
            {
                foreach (Row row in typeLibTable.Rows)
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[2]);
                    Wix.TypeLib typeLib = (Wix.TypeLib)this.core.GetIndexedElement(row);

                    foreach (Wix.ISchemaElement element in component.Children)
                    {
                        Wix.File file = element as Wix.File;

                        if (null != file && Wix.YesNoType.yes == file.KeyPath)
                        {
                            file.AddChild(typeLib);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the MIME table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// There is a foreign key shared between the MIME and Extension
        /// tables so either one would be valid to be decompiled first, so
        /// the only safe way to nest the MIME elements is to do it during finalize.
        /// </remarks>
        private void FinalizeMIMETable(TableCollection tables)
        {
            Table extensionTable = tables["Extension"];
            Table mimeTable = tables["MIME"];

            Hashtable comExtensions = new Hashtable();

            if (null != extensionTable)
            {
                foreach (Row row in extensionTable.Rows)
                {
                    Wix.Extension extension = (Wix.Extension)this.core.GetIndexedElement(row);

                    // index the extension
                    if (!comExtensions.Contains(row[0]))
                    {
                        comExtensions.Add(row[0], new ArrayList());
                    }
                    ((ArrayList)comExtensions[row[0]]).Add(extension);

                    // set the default MIME element for this extension
                    if (null != row[3])
                    {
                        Wix.MIME mime = (Wix.MIME)this.core.GetIndexedElement("MIME", (string)row[3]);

                        if (null != mime)
                        {
                            mime.Default = Wix.YesNoType.yes;
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Extension", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "MIME_", (string)row[3], "MIME"));
                        }
                    }
                }
            }

            if (null != mimeTable)
            {
                foreach (Row row in mimeTable.Rows)
                {
                    Wix.MIME mime = (Wix.MIME)this.core.GetIndexedElement(row);

                    if (comExtensions.Contains(row[1]))
                    {
                        ArrayList extensionElements = (ArrayList)comExtensions[row[1]];

                        foreach (Wix.Extension extension in extensionElements)
                        {
                            extension.AddChild(mime);
                        }
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "MIME", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Extension_", (string)row[1], "Extension"));
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the ProgId table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Enumerates through all the Class rows, looking for child ProgIds (these are the
        /// default ProgIds for a given Class).  Then go through the ProgId table and add any
        /// remaining ProgIds for each Class.  This happens during finalize because there is
        /// a circular dependency between the Class and ProgId tables.
        /// </remarks>
        private void FinalizeProgIdTable(TableCollection tables)
        {
            Table classTable = tables["Class"];
            Table progIdTable = tables["ProgId"];

            Hashtable addedProgIds = new Hashtable();
            Hashtable classes = new Hashtable();

            // add the default ProgIds for each class (and index the class table)
            if (null != classTable)
            {
                foreach (Row row in classTable.Rows)
                {
                    Wix.Class wixClass = (Wix.Class)this.core.GetIndexedElement(row);

                    if (null != row[3])
                    {
                        Wix.ProgId progId = (Wix.ProgId)this.core.GetIndexedElement("ProgId", (string)row[3]);

                        if (null != progId)
                        {
                            wixClass.AddChild(progId);
                            addedProgIds.Add(progId, null);
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "Class", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "ProgId_Default", (string)row[3], "ProgId"));
                        }
                    }

                    // index the Class elements for nesting of ProgId elements (which don't use the full Class primary key)
                    if (!classes.Contains(wixClass.Id))
                    {
                        classes.Add(wixClass.Id, new ArrayList());
                    }
                    ((ArrayList)classes[wixClass.Id]).Add(wixClass);
                }
            }

            // add the remaining non-default ProgId entries for each class
            if (null != progIdTable)
            {
                foreach (Row row in progIdTable.Rows)
                {
                    Wix.ProgId progId = (Wix.ProgId)this.core.GetIndexedElement(row);

                    if (!addedProgIds.Contains(progId) && null != row[2])
                    {
                        ArrayList classElements = (ArrayList)classes[row[2]];

                        if (null != classElements)
                        {
                            foreach (Wix.Class wixClass in classElements)
                            {
                                wixClass.AddChild(progId);
                            }
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "ProgId", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Class_", (string)row[2], "Class"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the RemoveFile table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Sets the directory/property for each RemoveFile row.
        /// </remarks>
        private void FinalizeRemoveFileTable(TableCollection tables)
        {
            Table removeFileTable = tables["RemoveFile"];

            if (null != removeFileTable)
            {
                foreach (Row row in removeFileTable.Rows)
                {
                    bool isDirectory = false;
                    string property = (string)row[3];

                    // determine if the property is actually authored as a directory
                    if (null != this.core.GetIndexedElement("Directory", property))
                    {
                        isDirectory = true;
                    }

                    Wix.ISchemaElement element = this.core.GetIndexedElement(row);

                    if (element is Wix.RemoveFile)
                    {
                        Wix.RemoveFile removeFile = (Wix.RemoveFile)element;

                        if (isDirectory)
                        {
                            removeFile.Directory = property;
                        }
                        else
                        {
                            removeFile.Property = property;
                        }
                    }
                    else
                    {
                        Wix.RemoveFolder removeFolder = (Wix.RemoveFolder)element;

                        if (isDirectory)
                        {
                            removeFolder.Directory = property;
                        }
                        else
                        {
                            removeFolder.Property = property;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the LockPermissions table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Nests the Permission elements below their parent elements.  There are no declared foreign
        /// keys for the parents of the LockPermissions table.
        /// </remarks>
        private void FinalizeLockPermissionsTable(TableCollection tables)
        {
            Table createFolderTable = tables["CreateFolder"];
            Table lockPermissionsTable = tables["LockPermissions"];

            Hashtable createFolders = new Hashtable();

            // index the CreateFolder table because the foreign key to this table from the
            // LockPermissions table is only part of the primary key of this table
            if (null != createFolderTable)
            {
                foreach (Row row in createFolderTable.Rows)
                {
                    Wix.CreateFolder createFolder = (Wix.CreateFolder)this.core.GetIndexedElement(row);
                    string directoryId = (string)row[0];

                    if (!createFolders.Contains(directoryId))
                    {
                        createFolders.Add(directoryId, new ArrayList());
                    }
                    ((ArrayList)createFolders[directoryId]).Add(createFolder);
                }
            }

            if (null != lockPermissionsTable)
            {
                foreach (Row row in lockPermissionsTable.Rows)
                {
                    string id = (string)row[0];
                    string table = (string)row[1];

                    Wix.Permission permission = (Wix.Permission)this.core.GetIndexedElement(row);

                    if ("CreateFolder" == table)
                    {
                        ArrayList createFolderElements = (ArrayList)createFolders[id];

                        if (null != createFolderElements)
                        {
                            foreach (Wix.CreateFolder createFolder in createFolderElements)
                            {
                                createFolder.AddChild(permission);
                            }
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "LockPermissions", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "LockObject", id, table));
                        }
                    }
                    else
                    {
                        Wix.IParentElement parentElement = (Wix.IParentElement)this.core.GetIndexedElement(table, id);

                        if (null != parentElement)
                        {
                            parentElement.AddChild(permission);
                        }
                        else
                        {
                            this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, "LockPermissions", row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "LockObject", id, table));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the search tables.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>Does all the complex linking required for the search tables.</remarks>
        private void FinalizeSearchTables(TableCollection tables)
        {
            Table appSearchTable = tables["AppSearch"];
            Table ccpSearchTable = tables["CCPSearch"];
            Table drLocatorTable = tables["DrLocator"];

            Hashtable appSearches = new Hashtable();
            Hashtable ccpSearches = new Hashtable();
            Hashtable drLocators = new Hashtable();
            Hashtable locators = new Hashtable();
            Hashtable usedSearchElements = new Hashtable();
            ArrayList unusedSearchElements = new ArrayList();

            Wix.ComplianceCheck complianceCheck = null;

            // index the AppSearch table by signatures
            if (null != appSearchTable)
            {
                foreach (Row row in appSearchTable.Rows)
                {
                    string property = (string)row[0];
                    string signature = (string)row[1];

                    if (!appSearches.Contains(signature))
                    {
                        appSearches.Add(signature, new StringCollection());
                    }

                    ((StringCollection)appSearches[signature]).Add(property);
                }
            }

            // index the CCPSearch table by signatures
            if (null != ccpSearchTable)
            {
                foreach (Row row in ccpSearchTable.Rows)
                {
                    string signature = (string)row[0];

                    if (!ccpSearches.Contains(signature))
                    {
                        ccpSearches.Add(signature, new StringCollection());
                    }

                    ((StringCollection)ccpSearches[signature]).Add(null);

                    if (null == complianceCheck && !appSearches.Contains(signature))
                    {
                        complianceCheck = new Wix.ComplianceCheck();
                        this.core.RootElement.AddChild(complianceCheck);
                    }
                }
            }

            // index the directory searches by their search elements (to get back the original row)
            if (null != drLocatorTable)
            {
                foreach (Row row in drLocatorTable.Rows)
                {
                    drLocators.Add(this.core.GetIndexedElement(row), row);
                }
            }

            // index the locator tables by their signatures
            string[] locatorTableNames = new string[] { "CompLocator", "RegLocator", "IniLocator", "DrLocator", "Signature" };
            foreach (string locatorTableName in locatorTableNames)
            {
                Table locatorTable = tables[locatorTableName];

                if (null != locatorTable)
                {
                    foreach (Row row in locatorTable.Rows)
                    {
                        string signature = (string)row[0];

                        if (!locators.Contains(signature))
                        {
                            locators.Add(signature, new ArrayList());
                        }

                        ((ArrayList)locators[signature]).Add(row);
                    }
                }
            }

            // move the DrLocator rows with a parent of CCP_DRIVE first to ensure they get FileSearch children (not FileSearchRef)
            foreach (ArrayList locatorRows in locators.Values)
            {
                int firstDrLocator = -1;

                for (int i = 0; i < locatorRows.Count; i++)
                {
                    Row locatorRow = (Row)locatorRows[i];

                    if ("DrLocator" == locatorRow.TableDefinition.Name)
                    {
                        if (-1 == firstDrLocator)
                        {
                            firstDrLocator = i;
                        }

                        if ("CCP_DRIVE" == (string)locatorRow[1])
                        {
                            locatorRows.RemoveAt(i);
                            locatorRows.Insert(firstDrLocator, locatorRow);
                            break;
                        }
                    }
                }
            }

            foreach (string signature in locators.Keys)
            {
                ArrayList locatorRows = (ArrayList)locators[signature];
                ArrayList signatureSearchElements = new ArrayList();

                foreach (Row locatorRow in locatorRows)
                {
                    bool used = true;
                    Wix.ISchemaElement searchElement = this.core.GetIndexedElement(locatorRow);

                    if ("Signature" == locatorRow.TableDefinition.Name && 0 < signatureSearchElements.Count)
                    {
                        foreach (Wix.IParentElement searchParentElement in signatureSearchElements)
                        {
                            if (!usedSearchElements.Contains(searchElement))
                            {
                                searchParentElement.AddChild(searchElement);
                                usedSearchElements[searchElement] = null;
                            }
                            else
                            {
                                Wix.FileSearchRef fileSearchRef = new Wix.FileSearchRef();

                                fileSearchRef.Id = signature;

                                searchParentElement.AddChild(fileSearchRef);
                            }
                        }
                    }
                    else if ("DrLocator" == locatorRow.TableDefinition.Name && null != locatorRow[1])
                    {
                        string parentSignature = (string)locatorRow[1];

                        if ("CCP_DRIVE" == parentSignature)
                        {
                            if (appSearches.Contains(signature))
                            {
                                StringCollection appSearchPropertyIds = (StringCollection)appSearches[signature];

                                foreach (string propertyId in appSearchPropertyIds)
                                {
                                    Wix.Property property = this.EnsureProperty(propertyId);
                                    Wix.ComplianceDrive complianceDrive = null;

                                    if (ccpSearches.Contains(signature))
                                    {
                                        property.ComplianceCheck = Wix.YesNoType.yes;
                                    }

                                    foreach (Wix.ISchemaElement element in property.Children)
                                    {
                                        if (element is Wix.ComplianceDrive)
                                        {
                                            complianceDrive = (Wix.ComplianceDrive)element;
                                            break;
                                        }
                                    }

                                    if (null == complianceDrive)
                                    {
                                        complianceDrive = new Wix.ComplianceDrive();
                                        property.AddChild(complianceDrive);
                                    }

                                    if (!usedSearchElements.Contains(searchElement))
                                    {
                                        complianceDrive.AddChild(searchElement);
                                        usedSearchElements[searchElement] = null;
                                    }
                                    else
                                    {
                                        Wix.DirectorySearchRef directorySearchRef = new Wix.DirectorySearchRef();

                                        directorySearchRef.Id = signature;

                                        if (null != locatorRow[1])
                                        {
                                            directorySearchRef.Parent = (string)locatorRow[1];
                                        }

                                        if (null != locatorRow[2])
                                        {
                                            directorySearchRef.Path = (string)locatorRow[2];
                                        }

                                        complianceDrive.AddChild(directorySearchRef);
                                        signatureSearchElements.Add(directorySearchRef);
                                    }
                                }
                            }
                            else if (ccpSearches.Contains(signature))
                            {
                                Wix.ComplianceDrive complianceDrive = null;

                                foreach (Wix.ISchemaElement element in complianceCheck.Children)
                                {
                                    if (element is Wix.ComplianceDrive)
                                    {
                                        complianceDrive = (Wix.ComplianceDrive)element;
                                        break;
                                    }
                                }

                                if (null == complianceDrive)
                                {
                                    complianceDrive = new Wix.ComplianceDrive();
                                    complianceCheck.AddChild(complianceDrive);
                                }

                                if (!usedSearchElements.Contains(searchElement))
                                {
                                    complianceDrive.AddChild(searchElement);
                                    usedSearchElements[searchElement] = null;
                                }
                                else
                                {
                                    Wix.DirectorySearchRef directorySearchRef = new Wix.DirectorySearchRef();

                                    directorySearchRef.Id = signature;

                                    if (null != locatorRow[1])
                                    {
                                        directorySearchRef.Parent = (string)locatorRow[1];
                                    }

                                    if (null != locatorRow[2])
                                    {
                                        directorySearchRef.Path = (string)locatorRow[2];
                                    }

                                    complianceDrive.AddChild(directorySearchRef);
                                    signatureSearchElements.Add(directorySearchRef);
                                }
                            }
                        }
                        else
                        {
                            bool usedDrLocator = false;
                            ArrayList parentLocatorRows = (ArrayList)locators[parentSignature];

                            if (null != parentLocatorRows)
                            {
                                foreach (Row parentLocatorRow in parentLocatorRows)
                                {
                                    if ("DrLocator" == parentLocatorRow.TableDefinition.Name)
                                    {
                                        Wix.IParentElement parentSearchElement = (Wix.IParentElement)this.core.GetIndexedElement(parentLocatorRow);

                                        if (parentSearchElement.Children.GetEnumerator().MoveNext())
                                        {
                                            Row parentDrLocatorRow = (Row)drLocators[parentSearchElement];
                                            Wix.DirectorySearchRef directorySeachRef = new Wix.DirectorySearchRef();

                                            directorySeachRef.Id = parentSignature;

                                            if (null != parentDrLocatorRow[1])
                                            {
                                                directorySeachRef.Parent = (string)parentDrLocatorRow[1];
                                            }

                                            if (null != parentDrLocatorRow[2])
                                            {
                                                directorySeachRef.Path = (string)parentDrLocatorRow[2];
                                            }

                                            parentSearchElement = directorySeachRef;
                                            unusedSearchElements.Add(directorySeachRef);
                                        }

                                        if (!usedSearchElements.Contains(searchElement))
                                        {
                                            parentSearchElement.AddChild(searchElement);
                                            usedSearchElements[searchElement] = null;
                                            usedDrLocator = true;
                                        }
                                        else
                                        {
                                            Wix.DirectorySearchRef directorySearchRef = new Wix.DirectorySearchRef();

                                            directorySearchRef.Id = signature;

                                            directorySearchRef.Parent = parentSignature;

                                            if (null != locatorRow[2])
                                            {
                                                directorySearchRef.Path = (string)locatorRow[2];
                                            }

                                            parentSearchElement.AddChild(searchElement);
                                            usedDrLocator = true;
                                        }
                                    }
                                }

                                // keep track of unused DrLocator rows
                                if (!usedDrLocator)
                                {
                                    unusedSearchElements.Add(searchElement);
                                }
                            }
                            else
                            {
                                // TODO: warn
                            }
                        }
                    }
                    else if (appSearches.Contains(signature))
                    {
                        StringCollection appSearchPropertyIds = (StringCollection)appSearches[signature];

                        foreach (string propertyId in appSearchPropertyIds)
                        {
                            Wix.Property property = this.EnsureProperty(propertyId);

                            if (ccpSearches.Contains(signature))
                            {
                                property.ComplianceCheck = Wix.YesNoType.yes;
                            }

                            if (!usedSearchElements.Contains(searchElement))
                            {
                                property.AddChild(searchElement);
                                usedSearchElements[searchElement] = null;
                            }
                            else if ("RegLocator" == locatorRow.TableDefinition.Name)
                            {
                                Wix.RegistrySearchRef registrySearchRef = new Wix.RegistrySearchRef();

                                registrySearchRef.Id = signature;

                                property.AddChild(registrySearchRef);
                                signatureSearchElements.Add(registrySearchRef);
                            }
                            else
                            {
                                // TODO: warn about unavailable Ref element
                            }
                        }
                    }
                    else if (ccpSearches.Contains(signature))
                    {
                        if (!usedSearchElements.Contains(searchElement))
                        {
                            complianceCheck.AddChild(searchElement);
                            usedSearchElements[searchElement] = null;
                        }
                        else if ("RegLocator" == locatorRow.TableDefinition.Name)
                        {
                            Wix.RegistrySearchRef registrySearchRef = new Wix.RegistrySearchRef();

                            registrySearchRef.Id = signature;

                            complianceCheck.AddChild(registrySearchRef);
                            signatureSearchElements.Add(registrySearchRef);
                        }
                        else
                        {
                            // TODO: warn about unavailable Ref element
                        }
                    }
                    else
                    {
                        if ("DrLocator" == locatorRow.TableDefinition.Name)
                        {
                            unusedSearchElements.Add(searchElement);
                        }
                        else
                        {
                            // TODO: warn
                            used = false;
                        }
                    }

                    // keep track of the search elements for this signature so that nested searches go in the proper parents
                    if (used)
                    {
                        signatureSearchElements.Add(searchElement);
                    }
                }
            }

            foreach (Wix.IParentElement unusedSearchElement in unusedSearchElements)
            {
                bool used = false;

                foreach (Wix.ISchemaElement schemaElement in unusedSearchElement.Children)
                {
                    if (schemaElement is Wix.DirectorySearch)
                    {
                        Wix.DirectorySearch directorySearch = (Wix.DirectorySearch)schemaElement;
                        StringCollection appSearchProperties = (StringCollection)appSearches[directorySearch.Id];

                        if (null != appSearchProperties)
                        {
                            Wix.Property property = (Wix.Property)this.core.GetIndexedElement("Property", appSearchProperties[0]);

                            property.AddChild((Wix.ISchemaElement)unusedSearchElement);
                            used = true;
                            break;
                        }
                        else if (ccpSearches.Contains(directorySearch.Id))
                        {
                            complianceCheck.AddChild((Wix.ISchemaElement)unusedSearchElement);
                            used = true;
                            break;
                        }
                        else
                        {
                            // TODO: warn
                        }
                    }
                }

                if (!used)
                {
                    // TODO: warn
                }
            }
        }

        /// <summary>
        /// Finalize the sequence tables.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// Creates the sequence elements.  Occurs during finalization because its
        /// not known if sequences refer to custom actions or dialogs during decompilation.
        /// </remarks>
        private void FinalizeSequenceTables(TableCollection tables)
        {
            // finalize the normal sequence tables
            if (OutputType.Product == this.outputType)
            {
                foreach (SequenceTable sequenceTable in Enum.GetValues(typeof(SequenceTable)))
                {
                    Table actionsTable = new Table(null, this.tableDefinitions["WixAction"]);
                    Table table = tables[sequenceTable.ToString()];

                    if (null != table)
                    {
                        ArrayList actionRows = new ArrayList();
                        bool needAbsoluteScheduling = this.suppressRelativeActionSequencing;
                        WixActionRowCollection nonSequencedActionRows = new WixActionRowCollection();
                        WixActionRowCollection suppressedRelativeActionRows = new WixActionRowCollection();

                        // create a sorted array of actions in this table
                        foreach (Row row in table.Rows)
                        {
                            WixActionRow actionRow = (WixActionRow)actionsTable.CreateRow(null);

                            actionRow.Action = (string)row[0];

                            if (null != row[1])
                            {
                                actionRow.Condition = (string)row[1];
                            }

                            actionRow.Sequence = (int)row[2];

                            actionRow.SequenceTable = sequenceTable;

                            actionRows.Add(actionRow);
                        }
                        actionRows.Sort();

                        for (int i = 0; i < actionRows.Count && !needAbsoluteScheduling; i++)
                        {
                            WixActionRow actionRow = (WixActionRow)actionRows[i];
                            WixActionRow standardActionRow = this.standardActions[actionRow.SequenceTable, actionRow.Action];

                            // create actions for custom actions, dialogs, AppSearch when its moved, and standard actions with non-standard conditions
                            if ("AppSearch" == actionRow.Action || null == standardActionRow || actionRow.Condition != standardActionRow.Condition)
                            {
                                WixActionRow previousActionRow = null;
                                WixActionRow nextActionRow = null;

                                // find the previous action row if there is one
                                if (0 <= i - 1)
                                {
                                    previousActionRow = (WixActionRow)actionRows[i - 1];
                                }

                                // find the next action row if there is one
                                if (actionRows.Count > i + 1)
                                {
                                    nextActionRow = (WixActionRow)actionRows[i + 1];
                                }

                                // the logic for setting the before or after attribute for an action:
                                // 1. If more than one action shares the same sequence number, everything must be absolutely sequenced.
                                // 2. If the next action is a standard action and is 1 sequence number higher, this action occurs before it.
                                // 3. If the previous action is a standard action and is 1 sequence number lower, this action occurs after it.
                                // 4. If this action is not standard and the previous action is 1 sequence number lower and does not occur before this action, this action occurs after it.
                                // 5. If this action is not standard and the previous action does not have the same sequence number and the next action is 1 sequence number higher, this action occurs before it.
                                // 6. If this action is AppSearch and has all standard information, ignore it.
                                // 7. If this action is standard and has a non-standard condition, create the action without any scheduling information.
                                // 8. Everything must be absolutely sequenced.
                                if ((null != previousActionRow && actionRow.Sequence == previousActionRow.Sequence) || (null != nextActionRow && actionRow.Sequence == nextActionRow.Sequence))
                                {
                                    needAbsoluteScheduling = true;
                                }
                                else if (null != nextActionRow && null != this.standardActions[sequenceTable, nextActionRow.Action] && actionRow.Sequence + 1 == nextActionRow.Sequence)
                                {
                                    actionRow.Before = nextActionRow.Action;
                                }
                                else if (null != previousActionRow && null != this.standardActions[sequenceTable, previousActionRow.Action] && actionRow.Sequence - 1 == previousActionRow.Sequence)
                                {
                                    actionRow.After = previousActionRow.Action;
                                }
                                else if (null == standardActionRow && null != previousActionRow && actionRow.Sequence - 1 == previousActionRow.Sequence && previousActionRow.Before != actionRow.Action)
                                {
                                    actionRow.After = previousActionRow.Action;
                                }
                                else if (null == standardActionRow && null != previousActionRow && actionRow.Sequence != previousActionRow.Sequence && null != nextActionRow && actionRow.Sequence + 1 == nextActionRow.Sequence)
                                {
                                    actionRow.Before = nextActionRow.Action;
                                }
                                else if ("AppSearch" == actionRow.Action && null != standardActionRow && actionRow.Sequence == standardActionRow.Sequence && actionRow.Condition == standardActionRow.Condition)
                                {
                                    // ignore an AppSearch row which has the WiX standard sequence and a standard condition
                                }
                                else if (null != standardActionRow && actionRow.Condition != standardActionRow.Condition) // standard actions get their standard sequence numbers
                                {
                                    nonSequencedActionRows.Add(actionRow);
                                }
                                else if (0 < actionRow.Sequence)
                                {
                                    needAbsoluteScheduling = true;
                                }
                            }
                            else
                            {
                                suppressedRelativeActionRows.Add(actionRow);
                            }
                        }

                        // create the actions now that we know if they must be absolutely or relatively scheduled
                        foreach (WixActionRow actionRow in actionRows)
                        {
                            if (needAbsoluteScheduling)
                            {
                                // remove any before/after information to ensure this is absolutely sequenced
                                actionRow.Before = null;
                                actionRow.After = null;
                            }
                            else if (nonSequencedActionRows.Contains(actionRow.SequenceTable, actionRow.Action))
                            {
                                // clear the sequence attribute to ensure this action is scheduled without a sequence number (or before/after)
                                actionRow.Sequence = 0;
                            }
                            else if (suppressedRelativeActionRows.Contains(actionRow.SequenceTable, actionRow.Action))
                            {
                                // skip the suppressed relatively scheduled action rows
                                continue;
                            }

                            // create the action element
                            this.CreateActionElement(actionRow);
                        }
                    }
                }
            }
            else if (OutputType.Module == this.outputType) // finalize the Module sequence tables
            {
                foreach (SequenceTable sequenceTable in Enum.GetValues(typeof(SequenceTable)))
                {
                    Table actionsTable = new Table(null, this.tableDefinitions["WixAction"]);
                    Table table = tables[String.Concat("Module", sequenceTable.ToString())];

                    if (null != table)
                    {
                        foreach (Row row in table.Rows)
                        {
                            WixActionRow actionRow = (WixActionRow)actionsTable.CreateRow(null);

                            actionRow.Action = (string)row[0];

                            if (null != row[1])
                            {
                                actionRow.Sequence = (int)row[1];
                            }

                            if (null != row[2] && null != row[3])
                            {
                                switch ((int)row[3])
                                {
                                    case 0:
                                        actionRow.Before = (string)row[2];
                                        break;
                                    case 1:
                                        actionRow.After = (string)row[2];
                                        break;
                                    default:
                                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[3].Column.Name, row[3]));
                                        break;
                                }
                            }

                            if (null != row[4])
                            {
                                actionRow.Condition = (string)row[4];
                            }

                            actionRow.SequenceTable = sequenceTable;

                            // create action elements for non-standard actions
                            if (null == this.standardActions[actionRow.SequenceTable, actionRow.Action] || null != actionRow.After || null != actionRow.Before)
                            {
                                this.CreateActionElement(actionRow);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finalize the Verb table.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        /// <remarks>
        /// The Extension table is a foreign table for the Verb table, but the
        /// foreign key is only part of the primary key of the Extension table,
        /// so it needs special logic to be nested properly.
        /// </remarks>
        private void FinalizeVerbTable(TableCollection tables)
        {
            Table extensionTable = tables["Extension"];
            Table verbTable = tables["Verb"];

            Hashtable extensionElements = new Hashtable();

            if (null != extensionTable)
            {
                foreach (Row row in extensionTable.Rows)
                {
                    Wix.Extension extension = (Wix.Extension)this.core.GetIndexedElement(row);

                    if (!extensionElements.Contains(row[0]))
                    {
                        extensionElements.Add(row[0], new ArrayList());
                    }

                    ((ArrayList)extensionElements[row[0]]).Add(extension);
                }
            }

            if (null != verbTable)
            {
                foreach (Row row in verbTable.Rows)
                {
                    Wix.Verb verb = (Wix.Verb)this.core.GetIndexedElement(row);

                    ArrayList extensionsArray = (ArrayList)extensionElements[row[0]];
                    if (null != extensionsArray)
                    {
                        foreach (Wix.Extension extension in extensionsArray)
                        {
                            extension.AddChild(verb);
                        }
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, verbTable.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Extension_", (string)row[0], "Extension"));
                    }
                }
            }
        }

        /// <summary>
        /// Get the path to a file in the source image.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The path to the file in the source image.</returns>
        private string GetSourcePath(Wix.File file)
        {
            StringBuilder sourcePath = new StringBuilder();

            Wix.Component component = (Wix.Component)file.ParentElement;

            for (Wix.Directory directory = (Wix.Directory)component.ParentElement; null != directory; directory = directory.ParentElement as Wix.Directory)
            {
                string name;

                if (!this.shortNames && null != directory.SourceName)
                {
                    name = directory.SourceName;
                }
                else if (null != directory.ShortSourceName)
                {
                    name = directory.ShortSourceName;
                }
                else if (!this.shortNames || null == directory.ShortName)
                {
                    name = directory.Name;
                }
                else
                {
                    name = directory.ShortName;
                }

                if (0 == sourcePath.Length)
                {
                    sourcePath.Append(name);
                }
                else
                {
                    sourcePath.Insert(0, Path.DirectorySeparatorChar);
                    sourcePath.Insert(0, name);
                }
            }

            return sourcePath.ToString();
        }

        /// <summary>
        /// Resolve the dependencies for a table (this is a helper method for GetSortedTableNames).
        /// </summary>
        /// <param name="tableName">The name of the table to resolve.</param>
        /// <param name="unsortedTableNames">The unsorted table names.</param>
        /// <param name="sortedTableNames">The sorted table names.</param>
        private void ResolveTableDependencies(string tableName, SortedList unsortedTableNames, StringCollection sortedTableNames)
        {
            unsortedTableNames.Remove(tableName);

            foreach (ColumnDefinition columnDefinition in this.tableDefinitions[tableName].Columns)
            {
                // no dependency to resolve because this column doesn't reference another table
                if (null == columnDefinition.KeyTable)
                {
                    continue;
                }

                foreach (string keyTable in columnDefinition.KeyTable.Split(';'))
                {
                    if (tableName == keyTable)
                    {
                        continue; // self-referencing dependency
                    }
                    else if (sortedTableNames.Contains(keyTable))
                    {
                        continue; // dependent table has already been sorted
                    }
                    else if (!this.tableDefinitions.Contains(keyTable))
                    {
                        this.core.OnMessage(WixErrors.MissingTableDefinition(keyTable));
                    }
                    else if (unsortedTableNames.Contains(keyTable))
                    {
                        this.ResolveTableDependencies(keyTable, unsortedTableNames, sortedTableNames);
                    }
                    else
                    {
                        // found a circular dependency, so ignore it (this assumes that the tables will
                        // use a finalize method to nest their elements since the ordering will not be
                        // deterministic
                    }
                }
            }

            sortedTableNames.Add(tableName);
        }

        /// <summary>
        /// Get the names of the tables to process in the order they should be processed, according to their dependencies.
        /// </summary>
        /// <returns>A StringCollection containing the ordered table names.</returns>
        private StringCollection GetSortedTableNames()
        {
            StringCollection sortedTableNames = new StringCollection();
            SortedList unsortedTableNames = new SortedList();

            // index the table names
            foreach (TableDefinition tableDefinition in this.tableDefinitions)
            {
                unsortedTableNames.Add(tableDefinition.Name, tableDefinition.Name);
            }

            // resolve the dependencies for each table
            while (0 < unsortedTableNames.Count)
            {
                this.ResolveTableDependencies((string)unsortedTableNames.GetByIndex(0), unsortedTableNames, sortedTableNames);
            }

            return sortedTableNames;
        }

        /// <summary>
        /// Initialize decompilation.
        /// </summary>
        /// <param name="tables">The collection of all tables.</param>
        private void InitializeDecompile(TableCollection tables)
        {
            // reset all the state information
            this.compressed = false;
            this.patchTargetFiles.Clear();
            this.sequenceElements.Clear();
            this.shortNames = false;

            // set the codepage if its not neutral (0)
            if (0 != this.codepage)
            {
                switch (this.outputType)
                {
                    case OutputType.Module:
                        ((Wix.Module)this.core.RootElement).Codepage = this.codepage;
                        break;
                    case OutputType.PatchCreation:
                        ((Wix.PatchCreation)this.core.RootElement).Codepage = this.codepage;
                        break;
                    case OutputType.Product:
                        ((Wix.Product)this.core.RootElement).Codepage = this.codepage;
                        break;
                }
            }

            // index the rows from the extension libraries
            Hashtable indexedExtensionTables = new Hashtable();
            foreach (WixExtension extension in this.extensions)
            {
                // determine if the extension would like its library rows to be removed
                // if there is no decompiler extension, assume rows should not be removed
                if (null == extension.DecompilerExtension || !extension.DecompilerExtension.RemoveLibraryRows)
                {
                    continue;
                }

                Library library = extension.GetLibrary(this.tableDefinitions);

                if (null != library)
                {
                    foreach (Section section in library.Sections)
                    {
                        foreach (Table table in section.Tables)
                        {
                            foreach (Row row in table.Rows)
                            {
                                string primaryKey;
                                string tableName;

                                // the Actions table needs to be handled specially
                                if ("WixAction" == table.Name)
                                {
                                    primaryKey = (string)row[1];

                                    if (OutputType.Module == this.outputType)
                                    {
                                        tableName = String.Concat("Module", (string)row[0]);
                                    }
                                    else
                                    {
                                        tableName = (string)row[0];
                                    }
                                }
                                else
                                {
                                    primaryKey = row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter);
                                    tableName = table.Name;
                                }

                                if (null != primaryKey)
                                {
                                    if (!indexedExtensionTables.Contains(tableName))
                                    {
                                        indexedExtensionTables.Add(tableName, new Hashtable());
                                    }
                                    Hashtable indexedExtensionRows = (Hashtable)indexedExtensionTables[tableName];

                                    indexedExtensionRows[primaryKey] = null;
                                }
                            }
                        }
                    }
                }
            }

            // remove the rows from the extension libraries (to allow full round-tripping)
            foreach (string tableName in indexedExtensionTables.Keys)
            {
                Table table = tables[tableName];
                Hashtable indexedExtensionRows = (Hashtable)indexedExtensionTables[tableName];

                if (null != table)
                {
                    RowCollection originalRows = table.Rows.Clone();

                    // remove the original rows so that they can be added back if they should remain
                    table.Rows.Clear();

                    foreach (Row row in originalRows)
                    {
                        if (!indexedExtensionRows.Contains(row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter)))
                        {
                            table.Rows.Add(row);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decompile the tables.
        /// </summary>
        /// <param name="output">The ouput being decompiled.</param>
        private void DecompileTables(Output output)
        {
            StringCollection sortedTableNames = this.GetSortedTableNames();

            foreach (string tableName in sortedTableNames)
            {
                Table table = output.Tables[tableName];

                // table does not exist in this database or should not be decompiled
                if (null == table || !this.DecompilableTable(output, tableName))
                {
                    continue;
                }

                this.core.OnMessage(WixVerboses.DecompilingTable(table.Name));

                // empty tables may be kept with EnsureTable if the user set the proper option
                if (0 == table.Rows.Count && this.suppressDroppingEmptyTables)
                {
                    Wix.EnsureTable ensureTable = new Wix.EnsureTable();
                    ensureTable.Id = table.Name;
                    this.core.RootElement.AddChild(ensureTable);
                }

                switch (table.Name)
                {
                    case "_SummaryInformation":
                        this.Decompile_SummaryInformationTable(table);
                        break;
                    case "AdminExecuteSequence":
                    case "AdminUISequence":
                    case "AdvtExecuteSequence":
                    case "InstallExecuteSequence":
                    case "InstallUISequence":
                    case "ModuleAdminExecuteSequence":
                    case "ModuleAdminUISequence":
                    case "ModuleAdvtExecuteSequence":
                    case "ModuleInstallExecuteSequence":
                    case "ModuleInstallUISequence":
                        // handled in FinalizeSequenceTables
                        break;
                    case "ActionText":
                        this.DecompileActionTextTable(table);
                        break;
                    case "AdvtUISequence":
                        this.core.OnMessage(WixWarnings.DeprecatedTable(table.Name));
                        break;
                    case "AppId":
                        this.DecompileAppIdTable(table);
                        break;
                    case "AppSearch":
                        // handled in FinalizeSearchTables
                        break;
                    case "BBControl":
                        this.DecompileBBControlTable(table);
                        break;
                    case "Billboard":
                        this.DecompileBillboardTable(table);
                        break;
                    case "Binary":
                        this.DecompileBinaryTable(table);
                        break;
                    case "BindImage":
                        this.DecompileBindImageTable(table);
                        break;
                    case "CCPSearch":
                        // handled in FinalizeSearchTables
                        break;
                    case "CheckBox":
                        // handled in FinalizeCheckBoxTable
                        break;
                    case "Class":
                        this.DecompileClassTable(table);
                        break;
                    case "ComboBox":
                        this.DecompileComboBoxTable(table);
                        break;
                    case "Control":
                        this.DecompileControlTable(table);
                        break;
                    case "ControlCondition":
                        this.DecompileControlConditionTable(table);
                        break;
                    case "ControlEvent":
                        this.DecompileControlEventTable(table);
                        break;
                    case "CreateFolder":
                        this.DecompileCreateFolderTable(table);
                        break;
                    case "CustomAction":
                        this.DecompileCustomActionTable(table);
                        break;
                    case "CompLocator":
                        this.DecompileCompLocatorTable(table);
                        break;
                    case "Complus":
                        this.DecompileComplusTable(table);
                        break;
                    case "Component":
                        this.DecompileComponentTable(table);
                        break;
                    case "Condition":
                        this.DecompileConditionTable(table);
                        break;
                    case "Dialog":
                        this.DecompileDialogTable(table);
                        break;
                    case "Directory":
                        this.DecompileDirectoryTable(table);
                        break;
                    case "DrLocator":
                        this.DecompileDrLocatorTable(table);
                        break;
                    case "DuplicateFile":
                        this.DecompileDuplicateFileTable(table);
                        break;
                    case "Environment":
                        this.DecompileEnvironmentTable(table);
                        break;
                    case "Error":
                        this.DecompileErrorTable(table);
                        break;
                    case "EventMapping":
                        this.DecompileEventMappingTable(table);
                        break;
                    case "Extension":
                        this.DecompileExtensionTable(table);
                        break;
                    case "ExternalFiles":
                        this.DecompileExternalFilesTable(table);
                        break;
                    case "FamilyFileRanges":
                        // handled in FinalizeFamilyFileRangesTable
                        break;
                    case "Feature":
                        this.DecompileFeatureTable(table);
                        break;
                    case "FeatureComponents":
                        this.DecompileFeatureComponentsTable(table);
                        break;
                    case "File":
                        this.DecompileFileTable(table);
                        break;
                    case "FileSFPCatalog":
                        this.DecompileFileSFPCatalogTable(table);
                        break;
                    case "Font":
                        this.DecompileFontTable(table);
                        break;
                    case "Icon":
                        this.DecompileIconTable(table);
                        break;
                    case "ImageFamilies":
                        this.DecompileImageFamiliesTable(table);
                        break;
                    case "IniFile":
                        this.DecompileIniFileTable(table);
                        break;
                    case "IniLocator":
                        this.DecompileIniLocatorTable(table);
                        break;
                    case "IsolatedComponent":
                        this.DecompileIsolatedComponentTable(table);
                        break;
                    case "LaunchCondition":
                        this.DecompileLaunchConditionTable(table);
                        break;
                    case "ListBox":
                        this.DecompileListBoxTable(table);
                        break;
                    case "ListView":
                        this.DecompileListViewTable(table);
                        break;
                    case "LockPermissions":
                        this.DecompileLockPermissionsTable(table);
                        break;
                    case "Media":
                        this.DecompileMediaTable(table);
                        break;
                    case "MIME":
                        this.DecompileMIMETable(table);
                        break;
                    case "ModuleAdvtUISequence":
                        this.core.OnMessage(WixWarnings.DeprecatedTable(table.Name));
                        break;
                    case "ModuleComponents":
                        // handled by DecompileComponentTable (since the ModuleComponents table
                        // rows are created by nesting components under the Module element)
                        break;
                    case "ModuleConfiguration":
                        this.DecompileModuleConfigurationTable(table);
                        break;
                    case "ModuleDependency":
                        this.DecompileModuleDependencyTable(table);
                        break;
                    case "ModuleExclusion":
                        this.DecompileModuleExclusionTable(table);
                        break;
                    case "ModuleIgnoreTable":
                        this.DecompileModuleIgnoreTableTable(table);
                        break;
                    case "ModuleSignature":
                        this.DecompileModuleSignatureTable(table);
                        break;
                    case "ModuleSubstitution":
                        this.DecompileModuleSubstitutionTable(table);
                        break;
                    case "MoveFile":
                        this.DecompileMoveFileTable(table);
                        break;
                    case "MsiAssembly":
                        // handled in FinalizeFileTable
                        break;
                    case "MsiDigitalCertificate":
                        this.DecompileMsiDigitalCertificateTable(table);
                        break;
                    case "MsiDigitalSignature":
                        this.DecompileMsiDigitalSignatureTable(table);
                        break;
                    case "MsiPatchCertificate":
                        this.DecompileMsiPatchCertificateTable(table);
                        break;
                    case "ODBCAttribute":
                        this.DecompileODBCAttributeTable(table);
                        break;
                    case "ODBCDataSource":
                        this.DecompileODBCDataSourceTable(table);
                        break;
                    case "ODBCDriver":
                        this.DecompileODBCDriverTable(table);
                        break;
                    case "ODBCSourceAttribute":
                        this.DecompileODBCSourceAttributeTable(table);
                        break;
                    case "ODBCTranslator":
                        this.DecompileODBCTranslatorTable(table);
                        break;
                    case "PatchMetadata":
                        this.DecompilePatchMetadataTable(table);
                        break;
                    case "PatchSequence":
                        this.DecompilePatchSequenceTable(table);
                        break;
                    case "ProgId":
                        this.DecompileProgIdTable(table);
                        break;
                    case "Properties":
                        this.DecompilePropertiesTable(table);
                        break;
                    case "Property":
                        this.DecompilePropertyTable(table);
                        break;
                    case "PublishComponent":
                        this.DecompilePublishComponentTable(table);
                        break;
                    case "RadioButton":
                        this.DecompileRadioButtonTable(table);
                        break;
                    case "Registry":
                        this.DecompileRegistryTable(table);
                        break;
                    case "RegLocator":
                        this.DecompileRegLocatorTable(table);
                        break;
                    case "RemoveFile":
                        this.DecompileRemoveFileTable(table);
                        break;
                    case "RemoveIniFile":
                        this.DecompileRemoveIniFileTable(table);
                        break;
                    case "RemoveRegistry":
                        this.DecompileRemoveRegistryTable(table);
                        break;
                    case "ReserveCost":
                        this.DecompileReserveCostTable(table);
                        break;
                    case "SelfReg":
                        this.DecompileSelfRegTable(table);
                        break;
                    case "ServiceControl":
                        this.DecompileServiceControlTable(table);
                        break;
                    case "ServiceInstall":
                        this.DecompileServiceInstallTable(table);
                        break;
                    case "SFPCatalog":
                        this.DecompileSFPCatalogTable(table);
                        break;
                    case "Shortcut":
                        this.DecompileShortcutTable(table);
                        break;
                    case "Signature":
                        this.DecompileSignatureTable(table);
                        break;
                    case "TargetFiles_OptionalData":
                        this.DecompileTargetFiles_OptionalDataTable(table);
                        break;
                    case "TargetImages":
                        this.DecompileTargetImagesTable(table);
                        break;
                    case "TextStyle":
                        this.DecompileTextStyleTable(table);
                        break;
                    case "TypeLib":
                        this.DecompileTypeLibTable(table);
                        break;
                    case "Upgrade":
                        this.DecompileUpgradeTable(table);
                        break;
                    case "UpgradedFiles_OptionalData":
                        this.DecompileUpgradedFiles_OptionalDataTable(table);
                        break;
                    case "UpgradedFilesToIgnore":
                        this.DecompileUpgradedFilesToIgnoreTable(table);
                        break;
                    case "UpgradedImages":
                        this.DecompileUpgradedImagesTable(table);
                        break;
                    case "UIText":
                        this.DecompileUITextTable(table);
                        break;
                    case "Verb":
                        this.DecompileVerbTable(table);
                        break;
                    default:
                        DecompilerExtension extension = (DecompilerExtension)this.extensionsByTableName[table.Name];

                        if (null != extension)
                        {
                            extension.DecompileTable(table);
                        }
                        else
                        {
                            this.DecompileCustomTable(table);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Determine if a particular table should be decompiled with the current settings.
        /// </summary>
        /// <param name="output">The output being decompiled.</param>
        /// <param name="tableName">The name of a table.</param>
        /// <returns>true if the table should be decompiled; false otherwise.</returns>
        private bool DecompilableTable(Output output, string tableName)
        {
            switch (tableName)
            {
                case "ActionText":
                case "BBControl":
                case "Billboard":
                case "Control":
                case "ControlCondition":
                case "ControlEvent":
                case "Dialog":
                case "Error":
                case "EventMapping":
                case "RadioButton":
                case "TextStyle":
                case "UIText":
                    return !this.suppressUI;
                case "ModuleAdminExecuteSequence":
                case "ModuleAdminUISequence":
                case "ModuleAdvtExecuteSequence":
                case "ModuleAdvtUISequence":
                case "ModuleComponents":
                case "ModuleConfiguration":
                case "ModuleDependency":
                case "ModuleIgnoreTable":
                case "ModuleInstallExecuteSequence":
                case "ModuleInstallUISequence":
                case "ModuleExclusion":
                case "ModuleSignature":
                case "ModuleSubstitution":
                    if (OutputType.Module != output.Type)
                    {
                        this.core.OnMessage(WixWarnings.SkippingMergeModuleTable(output.SourceLineNumbers, tableName));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case "ExternalFiles":
                case "FamilyFileRanges":
                case "ImageFamilies":
                case "PatchMetadata":
                case "PatchSequence":
                case "Properties":
                case "TargetFiles_OptionalData":
                case "TargetImages":
                case "UpgradedFiles_OptionalData":
                case "UpgradedFilesToIgnore":
                case "UpgradedImages":
                    if (OutputType.PatchCreation != output.Type)
                    {
                        this.core.OnMessage(WixWarnings.SkippingPatchCreationTable(output.SourceLineNumbers, tableName));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case "MsiPatchHeaders":
                case "MsiPatchMetadata":
                case "MsiPatchOldAssemblyName":
                case "MsiPatchOldAssemblyFile":
                case "MsiPatchSequence":
                case "Patch":
                case "PatchPackage":
                    this.core.OnMessage(WixWarnings.PatchTable(output.SourceLineNumbers, tableName));
                    return false;
                case "_SummaryInformation":
                    return true;
                case "_Validation":
                case "MsiAssemblyName":
                case "MsiFileHash":
                    return false;
                default: // all other tables are allowed in any output except for a patch creation package
                    if (OutputType.PatchCreation == output.Type)
                    {
                        this.core.OnMessage(WixWarnings.IllegalPatchCreationTable(output.SourceLineNumbers, tableName));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }
        }

        /// <summary>
        /// Decompile the _SummaryInformation table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void Decompile_SummaryInformationTable(Table table)
        {
            if (OutputType.Module == this.outputType || OutputType.Product == this.outputType)
            {
                Wix.Package package = new Wix.Package();

                foreach (Row row in table.Rows)
                {
                    string value = (string)row[1];

                    if (null != value && 0 < value.Length)
                    {
                        switch ((int)row[0])
                        {
                            case 1:
                                if ("1252" != value)
                                {
                                    package.SummaryCodepage = value;
                                }
                                break;
                            case 3:
                                package.Description = value;
                                break;
                            case 4:
                                package.Manufacturer = value;
                                break;
                            case 5:
                                if ("Installer" != value)
                                {
                                    package.Keywords = value;
                                }
                                break;
                            case 6:
                                package.Comments = value;
                                break;
                            case 7:
                                string[] template = value.Split(';');
                                if (0 < template.Length && 0 < template[template.Length - 1].Length)
                                {
                                    package.Languages = template[template.Length - 1];
                                }

                                if (1 < template.Length && null != template[0] && 0 < template[0].Length)
                                {
                                    package.Platforms = template[0];
                                }
                                break;
                            case 9:
                                if (OutputType.Module == this.outputType)
                                {
                                    this.modularizationGuid = value;
                                    package.Id = value;
                                }
                                break;
                            case 14:
                                package.InstallerVersion = Convert.ToInt32(row[1], CultureInfo.InvariantCulture);
                                break;
                            case 15:
                                int wordCount = Convert.ToInt32(row[1], CultureInfo.InvariantCulture);
                                if (0x1 == (wordCount & 0x1))
                                {
                                    this.shortNames = true;
                                    package.ShortNames = Wix.YesNoType.yes;
                                }

                                if (0x2 == (wordCount & 0x2))
                                {
                                    this.compressed = true;

                                    if (OutputType.Product == this.outputType)
                                    {
                                        package.Compressed = Wix.YesNoType.yes;
                                    }
                                }

                                if (0x4 == (wordCount & 0x4))
                                {
                                    package.AdminImage = Wix.YesNoType.yes;
                                }

                                if (0x8 == (wordCount & 0x8))
                                {
                                    package.InstallPrivileges = Wix.Package.InstallPrivilegesType.limited;
                                }

                                break;
                            case 19:
                                int security = Convert.ToInt32(row[1], CultureInfo.InvariantCulture);
                                switch (security)
                                {
                                    case 0:
                                        package.ReadOnly = Wix.YesNoDefaultType.no;
                                        break;
                                    case 4:
                                        package.ReadOnly = Wix.YesNoDefaultType.yes;
                                        break;
                                }
                                break;
                        }
                    }
                }

                this.core.RootElement.AddChild(package);
            }
            else
            {
                Wix.PatchInformation patchInformation = new Wix.PatchInformation();

                foreach (Row row in table.Rows)
                {
                    int propertyId = (int)row[0];
                    string value = (string)row[1];

                    if (null != row[1] && 0 < value.Length)
                    {
                        switch (propertyId)
                        {
                            case 1:
                                if ("1252" != value)
                                {
                                    patchInformation.SummaryCodepage = value;
                                }
                                break;
                            case 3:
                                patchInformation.Description = value;
                                break;
                            case 4:
                                patchInformation.Manufacturer = value;
                                break;
                            case 5:
                                if ("Installer,Patching,PCP,Database" != value)
                                {
                                    patchInformation.Keywords = value;
                                }
                                break;
                            case 6:
                                patchInformation.Comments = value;
                                break;
                            case 7:
                                string[] template = value.Split(';');
                                if (0 < template.Length && 0 < template[template.Length - 1].Length)
                                {
                                    patchInformation.Languages = template[template.Length - 1];
                                }

                                if (1 < template.Length && null != template[0] && 0 < template[0].Length)
                                {
                                    patchInformation.Platforms = template[0];
                                }
                                break;
                            case 15:
                                int wordCount = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                                if (0x1 == (wordCount & 0x1))
                                {
                                    patchInformation.ShortNames = Wix.YesNoType.yes;
                                }

                                if (0x2 == (wordCount & 0x2))
                                {
                                    patchInformation.Compressed = Wix.YesNoType.yes;
                                }

                                if (0x4 == (wordCount & 0x4))
                                {
                                    patchInformation.AdminImage = Wix.YesNoType.yes;
                                }
                                break;
                            case 19:
                                int security = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                                switch (security)
                                {
                                    case 0:
                                        patchInformation.ReadOnly = Wix.YesNoDefaultType.no;
                                        break;
                                    case 4:
                                        patchInformation.ReadOnly = Wix.YesNoDefaultType.yes;
                                        break;
                                }
                                break;
                        }
                    }
                }

                this.core.RootElement.AddChild(patchInformation);
            }
        }

        /// <summary>
        /// Decompile the ActionText table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileActionTextTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ProgressText progressText = new Wix.ProgressText();

                progressText.Action = (string)row[0];

                if (null != row[1])
                {
                    progressText.Content = (string)row[1];
                }

                if (null != row[2])
                {
                    progressText.Template = (string)row[2];
                }

                this.core.UIElement.AddChild(progressText);
            }
        }

        /// <summary>
        /// Decompile the AppId table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileAppIdTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.AppId appId = new Wix.AppId();

                appId.Advertise = Wix.YesNoType.yes;

                appId.Id = (string)row[0];

                if (null != row[1])
                {
                    appId.RemoteServerName = (string)row[1];
                }

                if (null != row[2])
                {
                    appId.LocalService = (string)row[2];
                }

                if (null != row[3])
                {
                    appId.ServiceParameters = (string)row[3];
                }

                if (null != row[4])
                {
                    appId.DllSurrogate = (string)row[4];
                }

                if (null != row[5])
                {
                    appId.ActivateAtStorage = Wix.YesNoType.yes;
                }

                if (null != row[6])
                {
                    appId.RunAsInteractiveUser = Wix.YesNoType.yes;
                }

                this.core.RootElement.AddChild(appId);
                this.core.IndexElement(row, appId);
            }
        }

        /// <summary>
        /// Decompile the BBControl table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileBBControlTable(Table table)
        {
            foreach (BBControlRow bbControlRow in table.Rows)
            {
                Wix.Control control = new Wix.Control();

                control.Id = bbControlRow.BBControl;

                control.Type = bbControlRow.Type;

                control.X = bbControlRow.X;

                control.Y = bbControlRow.Y;

                control.Width = bbControlRow.Width;

                control.Height = bbControlRow.Height;

                if (null != bbControlRow[7])
                {
                    SetControlAttributes(bbControlRow.Attributes, control);
                }

                if (null != bbControlRow.Text)
                {
                    control.Text = bbControlRow.Text;
                }

                Wix.Billboard billboard = (Wix.Billboard)this.core.GetIndexedElement("Billboard", bbControlRow.Billboard);
                if (null != billboard)
                {
                    billboard.AddChild(control);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(bbControlRow.SourceLineNumbers, table.Name, bbControlRow.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Billboard_", bbControlRow.Billboard, "Billboard"));
                }
            }
        }

        /// <summary>
        /// Decompile the Billboard table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileBillboardTable(Table table)
        {
            Hashtable billboardActions = new Hashtable();
            SortedList billboards = new SortedList();

            foreach (Row row in table.Rows)
            {
                Wix.Billboard billboard = new Wix.Billboard();

                billboard.Id = (string)row[0];

                billboard.Feature = (string)row[1];

                this.core.IndexElement(row, billboard);
                billboards.Add(String.Format(CultureInfo.InvariantCulture, "{0}|{1:0000000000}", row[0], row[3]), row);
            }

            foreach (Row row in billboards.Values)
            {
                Wix.Billboard billboard = (Wix.Billboard)this.core.GetIndexedElement(row);
                Wix.BillboardAction billboardAction = (Wix.BillboardAction)billboardActions[row[2]];

                if (null == billboardAction)
                {
                    billboardAction = new Wix.BillboardAction();

                    billboardAction.Id = (string)row[2];

                    this.core.UIElement.AddChild(billboardAction);
                    billboardActions.Add(row[2], billboardAction);
                }

                billboardAction.AddChild(billboard);
            }
        }

        /// <summary>
        /// Decompile the Binary table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileBinaryTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Binary binary = new Wix.Binary();

                binary.Id = (string)row[0];

                binary.SourceFile = (string)row[1];

                this.core.RootElement.AddChild(binary);
            }
        }

        /// <summary>
        /// Decompile the BindImage table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileBindImageTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.File file = (Wix.File)this.core.GetIndexedElement("File", (string)row[0]);

                if (null != file)
                {
                    file.BindPath = (string)row[1];
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "File_", (string)row[0], "File"));
                }
            }
        }

        /// <summary>
        /// Decompile the Class table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileClassTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Class wixClass = new Wix.Class();

                wixClass.Advertise = Wix.YesNoType.yes;

                wixClass.Id = (string)row[0];

                switch ((string)row[1])
                {
                    case "LocalServer":
                        wixClass.Context = Wix.Class.ContextType.LocalServer;
                        break;
                    case "LocalServer32":
                        wixClass.Context = Wix.Class.ContextType.LocalServer32;
                        break;
                    case "InprocServer":
                        wixClass.Context = Wix.Class.ContextType.InprocServer;
                        break;
                    case "InprocServer32":
                        wixClass.Context = Wix.Class.ContextType.InprocServer32;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                // ProgId children are handled in FinalizeProgIdTable

                if (null != row[4])
                {
                    wixClass.Description = (string)row[4];
                }

                if (null != row[5])
                {
                    wixClass.AppId = (string)row[5];
                }

                if (null != row[6])
                {
                    string[] fileTypeMaskStrings = ((string)row[6]).Split(';');

                    try
                    {
                        foreach (string fileTypeMaskString in fileTypeMaskStrings)
                        {
                            string[] fileTypeMaskParts = fileTypeMaskString.Split(',');

                            if (4 == fileTypeMaskParts.Length)
                            {
                                Wix.FileTypeMask fileTypeMask = new Wix.FileTypeMask();

                                fileTypeMask.Offset = Convert.ToInt32(fileTypeMaskParts[0], CultureInfo.InvariantCulture);

                                fileTypeMask.Mask = fileTypeMaskParts[2];

                                fileTypeMask.Value = fileTypeMaskParts[3];

                                wixClass.AddChild(fileTypeMask);
                            }
                            else
                            {
                                // TODO: warn
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[6].Column.Name, row[6]));
                    }
                    catch (OverflowException)
                    {
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[6].Column.Name, row[6]));
                    }
                }

                if (null != row[7])
                {
                    wixClass.Icon = (string)row[7];
                }

                if (null != row[8])
                {
                    wixClass.IconIndex = (int)row[8];
                }

                if (null != row[9])
                {
                    wixClass.Handler = (string)row[9];
                }

                if (null != row[10])
                {
                    wixClass.Argument = (string)row[10];
                }

                if (null != row[12])
                {
                    if (1 == (int)row[12])
                    {
                        wixClass.RelativePath = Wix.YesNoType.yes;
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[12].Column.Name, row[12]));
                    }
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[2]);
                if (null != component)
                {
                    component.AddChild(wixClass);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[2], "Component"));
                }

                this.core.IndexElement(row, wixClass);
            }
        }

        /// <summary>
        /// Decompile the ComboBox table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileComboBoxTable(Table table)
        {
            Wix.ComboBox comboBox = null;
            SortedList comboBoxRows = new SortedList();

            // sort the combo boxes by their property and order
            foreach (Row row in table.Rows)
            {
                comboBoxRows.Add(String.Concat("{0}|{1:0000000000}", row[0], row[1]), row);
            }

            foreach (Row row in comboBoxRows.Values)
            {
                if (null == comboBox || (string)row[0] != comboBox.Property)
                {
                    comboBox = new Wix.ComboBox();

                    comboBox.Property = (string)row[0];

                    this.core.UIElement.AddChild(comboBox);
                }

                Wix.ListItem listItem = new Wix.ListItem();

                listItem.Value = (string)row[2];

                if (null != row[3])
                {
                    listItem.Text = (string)row[3];
                }

                comboBox.AddChild(listItem);
            }
        }

        /// <summary>
        /// Decompile the Control table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileControlTable(Table table)
        {
            foreach (ControlRow controlRow in table.Rows)
            {
                Wix.Control control = new Wix.Control();

                control.Id = controlRow.Control;

                control.Type = controlRow.Type;

                control.X = controlRow.X;

                control.Y = controlRow.Y;

                control.Width = controlRow.Width;

                control.Height = controlRow.Height;

                if (null != controlRow[7])
                {
                    string[] specialAttributes;

                    // sets various common attributes like Disabled, Indirect, Integer, ...
                    SetControlAttributes(controlRow.Attributes, control);

                    switch (control.Type)
                    {
                        case "Bitmap":
                            specialAttributes = MsiInterop.BitmapControlAttributes;
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
                        case "Edit":
                            specialAttributes = MsiInterop.EditControlAttributes;
                            break;
                        case "Icon":
                            specialAttributes = MsiInterop.IconControlAttributes;
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
                            break;
                        case "PushButton":
                            specialAttributes = MsiInterop.ButtonControlAttributes;
                            break;
                        case "RadioButtonGroup":
                            specialAttributes = MsiInterop.RadioControlAttributes;
                            break;
                        case "Text":
                            specialAttributes = MsiInterop.TextControlAttributes;
                            break;
                        case "VolumeCostList":
                            specialAttributes = MsiInterop.VolumeControlAttributes;
                            break;
                        case "VolumeSelectCombo":
                            specialAttributes = MsiInterop.VolumeControlAttributes;
                            break;
                        default:
                            specialAttributes = null;
                            break;
                    }

                    if (null != specialAttributes)
                    {
                        bool iconSizeSet = false;

                        for (int i = 16; 32 > i; i++)
                        {
                            if (1 == ((controlRow.Attributes >> i) & 1))
                            {
                                string attribute = null;

                                if (specialAttributes.Length > (i - 16))
                                {
                                    attribute = specialAttributes[i - 16];
                                }

                                // unknown attribute
                                if (null == attribute)
                                {
                                    this.core.OnMessage(WixWarnings.IllegalColumnValue(controlRow.SourceLineNumbers, table.Name, controlRow.Fields[7].Column.Name, controlRow.Attributes));
                                    continue;
                                }

                                switch (attribute)
                                {
                                    case "Bitmap":
                                        control.Bitmap = Wix.YesNoType.yes;
                                        break;
                                    case "CDROM":
                                        control.CDROM = Wix.YesNoType.yes;
                                        break;
                                    case "ComboList":
                                        control.ComboList = Wix.YesNoType.yes;
                                        break;
                                    case "ElevationShield":
                                        control.ElevationShield = Wix.YesNoType.yes;
                                        break;
                                    case "Fixed":
                                        control.Fixed = Wix.YesNoType.yes;
                                        break;
                                    case "FixedSize":
                                        control.FixedSize = Wix.YesNoType.yes;
                                        break;
                                    case "Floppy":
                                        control.Floppy = Wix.YesNoType.yes;
                                        break;
                                    case "FormatSize":
                                        control.FormatSize = Wix.YesNoType.yes;
                                        break;
                                    case "HasBorder":
                                        control.HasBorder = Wix.YesNoType.yes;
                                        break;
                                    case "Icon":
                                        control.Icon = Wix.YesNoType.yes;
                                        break;
                                    case "Icon16":
                                        if (iconSizeSet)
                                        {
                                            control.IconSize = Wix.Control.IconSizeType.Item48;
                                        }
                                        else
                                        {
                                            iconSizeSet = true;
                                            control.IconSize = Wix.Control.IconSizeType.Item16;
                                        }
                                        break;
                                    case "Icon32":
                                        if (iconSizeSet)
                                        {
                                            control.IconSize = Wix.Control.IconSizeType.Item48;
                                        }
                                        else
                                        {
                                            iconSizeSet = true;
                                            control.IconSize = Wix.Control.IconSizeType.Item32;
                                        }
                                        break;
                                    case "Image":
                                        control.Image = Wix.YesNoType.yes;
                                        break;
                                    case "Multiline":
                                        control.Multiline = Wix.YesNoType.yes;
                                        break;
                                    case "NoPrefix":
                                        control.NoPrefix = Wix.YesNoType.yes;
                                        break;
                                    case "NoWrap":
                                        control.NoWrap = Wix.YesNoType.yes;
                                        break;
                                    case "Password":
                                        control.Password = Wix.YesNoType.yes;
                                        break;
                                    case "ProgressBlocks":
                                        control.ProgressBlocks = Wix.YesNoType.yes;
                                        break;
                                    case "PushLike":
                                        control.PushLike = Wix.YesNoType.yes;
                                        break;
                                    case "RAMDisk":
                                        control.RAMDisk = Wix.YesNoType.yes;
                                        break;
                                    case "Remote":
                                        control.Remote = Wix.YesNoType.yes;
                                        break;
                                    case "Removable":
                                        control.Removable = Wix.YesNoType.yes;
                                        break;
                                    case "ShowRollbackCost":
                                        control.ShowRollbackCost = Wix.YesNoType.yes;
                                        break;
                                    case "Sorted":
                                        control.Sorted = Wix.YesNoType.yes;
                                        break;
                                    case "Transparent":
                                        control.Transparent = Wix.YesNoType.yes;
                                        break;
                                    case "UserLanguage":
                                        control.UserLanguage = Wix.YesNoType.yes;
                                        break;
                                    default:
                                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unknown control attribute: '{0}'.", attribute));
                                }
                            }
                        }
                    }
                    else if (0 < (controlRow.Attributes & 0xFFFF0000))
                    {
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(controlRow.SourceLineNumbers, table.Name, controlRow.Fields[7].Column.Name, controlRow.Attributes));
                    }
                }

                if (null != controlRow.Property)
                {
                    control.Property = controlRow.Property;
                }

                if (null != controlRow.Text)
                {
                    control.Text = controlRow.Text;
                }

                if (null != controlRow.Help)
                {
                    string[] help = controlRow.Help.Split('|');

                    if (2 == help.Length)
                    {
                        if (0 < help[0].Length)
                        {
                            control.ToolTip = help[0];
                        }

                        if (0 < help[1].Length)
                        {
                            control.Help = help[1];
                        }
                    }
                }

                this.core.IndexElement(controlRow, control);
            }
        }

        /// <summary>
        /// Decompile the ControlCondition table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileControlConditionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Condition condition = new Wix.Condition();

                switch ((string)row[2])
                {
                    case "Default":
                        condition.Action = Wix.Condition.ActionType.@default;
                        break;
                    case "Disable":
                        condition.Action = Wix.Condition.ActionType.disable;
                        break;
                    case "Enable":
                        condition.Action = Wix.Condition.ActionType.enable;
                        break;
                    case "Hide":
                        condition.Action = Wix.Condition.ActionType.hide;
                        break;
                    case "Show":
                        condition.Action = Wix.Condition.ActionType.show;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[2].Column.Name, row[2]));
                        break;
                }

                condition.Content = (string)row[3];

                Wix.Control control = (Wix.Control)this.core.GetIndexedElement("Control", (string)row[0], (string)row[1]);
                if (null != control)
                {
                    control.AddChild(condition);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog_", (string)row[0], "Control_", (string)row[1], "Control"));
                }
            }
        }

        /// <summary>
        /// Decompile the ControlEvent table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileControlEventTable(Table table)
        {
            SortedList controlEvents = new SortedList();

            foreach (Row row in table.Rows)
            {
                Wix.Publish publish = new Wix.Publish();

                string publishEvent = (string)row[2];
                if (publishEvent.StartsWith("[") && publishEvent.EndsWith("]"))
                {
                    publish.Property = publishEvent.Substring(1, publishEvent.Length - 2);

                    if ("{}" != (string)row[3])
                    {
                        publish.Value = (string)row[3];
                    }
                }
                else
                {
                    publish.Event = publishEvent;
                    publish.Value = (string)row[3];
                }

                if (null != row[4])
                {
                    publish.Content = (string)row[4];
                }

                controlEvents.Add(String.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2:0000000000}|{3}|{4}|{5}", row[0], row[1], (null == row[5] ? 0 : (int)row[5]), row[2], row[3], row[4]), row);

                this.core.IndexElement(row, publish);
            }

            foreach (Row row in controlEvents.Values)
            {
                Wix.Control control = (Wix.Control)this.core.GetIndexedElement("Control", (string)row[0], (string)row[1]);
                Wix.Publish publish = (Wix.Publish)this.core.GetIndexedElement(row);

                if (null != control)
                {
                    control.AddChild(publish);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog_", (string)row[0], "Control_", (string)row[1], "Control"));
                }
            }
        }

        /// <summary>
        /// Decompile a custom table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileCustomTable(Table table)
        {
            if (0 < table.Rows.Count || this.suppressDroppingEmptyTables)
            {
                Wix.CustomTable customTable = new Wix.CustomTable();

                this.core.OnMessage(WixWarnings.DecompilingAsCustomTable(table.Rows[0].SourceLineNumbers, table.Name));

                customTable.Id = table.Name;

                foreach (ColumnDefinition columnDefinition in table.Definition.Columns)
                {
                    Wix.Column column = new Wix.Column();

                    column.Id = columnDefinition.Name;

                    if (ColumnCategory.Unknown != columnDefinition.Category)
                    {
                        switch (columnDefinition.Category)
                        {
                            case ColumnCategory.Text:
                                column.Category = Wix.Column.CategoryType.Text;
                                break;
                            case ColumnCategory.UpperCase:
                                column.Category = Wix.Column.CategoryType.UpperCase;
                                break;
                            case ColumnCategory.LowerCase:
                                column.Category = Wix.Column.CategoryType.LowerCase;
                                break;
                            case ColumnCategory.Integer:
                                column.Category = Wix.Column.CategoryType.Integer;
                                break;
                            case ColumnCategory.DoubleInteger:
                                column.Category = Wix.Column.CategoryType.DoubleInteger;
                                break;
                            case ColumnCategory.TimeDate:
                                column.Category = Wix.Column.CategoryType.TimeDate;
                                break;
                            case ColumnCategory.Identifier:
                                column.Category = Wix.Column.CategoryType.Identifier;
                                break;
                            case ColumnCategory.Property:
                                column.Category = Wix.Column.CategoryType.Property;
                                break;
                            case ColumnCategory.Filename:
                                column.Category = Wix.Column.CategoryType.Filename;
                                break;
                            case ColumnCategory.WildCardFilename:
                                column.Category = Wix.Column.CategoryType.WildCardFilename;
                                break;
                            case ColumnCategory.Path:
                                column.Category = Wix.Column.CategoryType.Path;
                                break;
                            case ColumnCategory.Paths:
                                column.Category = Wix.Column.CategoryType.Paths;
                                break;
                            case ColumnCategory.AnyPath:
                                column.Category = Wix.Column.CategoryType.AnyPath;
                                break;
                            case ColumnCategory.DefaultDir:
                                column.Category = Wix.Column.CategoryType.DefaultDir;
                                break;
                            case ColumnCategory.RegPath:
                                column.Category = Wix.Column.CategoryType.RegPath;
                                break;
                            case ColumnCategory.Formatted:
                                column.Category = Wix.Column.CategoryType.Formatted;
                                break;
                            case ColumnCategory.Template:
                                column.Category = Wix.Column.CategoryType.Template;
                                break;
                            case ColumnCategory.Condition:
                                column.Category = Wix.Column.CategoryType.Condition;
                                break;
                            case ColumnCategory.Guid:
                                column.Category = Wix.Column.CategoryType.Guid;
                                break;
                            case ColumnCategory.Version:
                                column.Category = Wix.Column.CategoryType.Version;
                                break;
                            case ColumnCategory.Language:
                                column.Category = Wix.Column.CategoryType.Language;
                                break;
                            case ColumnCategory.Binary:
                                column.Category = Wix.Column.CategoryType.Binary;
                                break;
                            case ColumnCategory.CustomSource:
                                column.Category = Wix.Column.CategoryType.CustomSource;
                                break;
                            case ColumnCategory.Cabinet:
                                column.Category = Wix.Column.CategoryType.Cabinet;
                                break;
                            case ColumnCategory.Shortcut:
                                column.Category = Wix.Column.CategoryType.Shortcut;
                                break;
                            default:
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unknown custom column category '{0}'.", columnDefinition.Category.ToString()));
                        }
                    }

                    if (null != columnDefinition.Description)
                    {
                        column.Description = columnDefinition.Description;
                    }

                    if (columnDefinition.IsKeyColumnSet)
                    {
                        column.KeyColumn = columnDefinition.KeyColumn;
                    }

                    if (null != columnDefinition.KeyTable)
                    {
                        column.KeyTable = columnDefinition.KeyTable;
                    }

                    if (columnDefinition.IsLocalizable)
                    {
                        column.Localizable = Wix.YesNoType.yes;
                    }

                    if (columnDefinition.IsMaxValueSet)
                    {
                        column.MaxValue = columnDefinition.MaxValue;
                    }

                    if (columnDefinition.IsMinValueSet)
                    {
                        column.MinValue = columnDefinition.MinValue;
                    }

                    if (ColumnModularizeType.None != columnDefinition.ModularizeType)
                    {
                        switch (columnDefinition.ModularizeType)
                        {
                            case ColumnModularizeType.Column:
                                column.Modularize = Wix.Column.ModularizeType.Column;
                                break;
                            case ColumnModularizeType.Condition:
                                column.Modularize = Wix.Column.ModularizeType.Condition;
                                break;
                            case ColumnModularizeType.Icon:
                                column.Modularize = Wix.Column.ModularizeType.Icon;
                                break;
                            case ColumnModularizeType.Property:
                                column.Modularize = Wix.Column.ModularizeType.Property;
                                break;
                            case ColumnModularizeType.SemicolonDelimited:
                                column.Modularize = Wix.Column.ModularizeType.SemicolonDelimited;
                                break;
                            default:
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unknown custom column modularization type '{0}'.", columnDefinition.ModularizeType.ToString()));
                        }
                    }

                    if (columnDefinition.IsNullable)
                    {
                        column.Nullable = Wix.YesNoType.yes;
                    }

                    if (columnDefinition.IsPrimaryKey)
                    {
                        column.PrimaryKey = Wix.YesNoType.yes;
                    }

                    if (null != columnDefinition.Possibilities)
                    {
                        column.Set = columnDefinition.Possibilities;
                    }

                    if (ColumnType.Unknown != columnDefinition.Type)
                    {
                        switch (columnDefinition.Type)
                        {
                            case ColumnType.Localized:
                                column.Localizable = Wix.YesNoType.yes;
                                column.Type = Wix.Column.TypeType.@string;
                                break;
                            case ColumnType.Number:
                                column.Type = Wix.Column.TypeType.@int;
                                break;
                            case ColumnType.Object:
                                column.Type = Wix.Column.TypeType.binary;
                                break;
                            case ColumnType.String:
                                column.Type = Wix.Column.TypeType.@string;
                                break;
                            default:
                                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unknown custom column type '{0}'.", columnDefinition.Type.ToString()));
                        }
                    }

                    column.Width = columnDefinition.Length;

                    customTable.AddChild(column);
                }

                foreach (Row row in table.Rows)
                {
                    Wix.Row wixRow = new Wix.Row();

                    foreach (Field field in row.Fields)
                    {
                        Wix.Data data = new Wix.Data();

                        data.Column = field.Column.Name;

                        data.Content = Convert.ToString(field.Data, CultureInfo.InvariantCulture);

                        wixRow.AddChild(data);
                    }

                    customTable.AddChild(wixRow);
                }

                this.core.RootElement.AddChild(customTable);
            }
        }

        /// <summary>
        /// Decompile the CreateFolder table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileCreateFolderTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.CreateFolder createFolder = new Wix.CreateFolder();

                createFolder.Directory = (string)row[0];

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(createFolder);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
                this.core.IndexElement(row, createFolder);
            }
        }

        /// <summary>
        /// Decompile the CustomAction table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileCustomActionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.CustomAction customAction = new Wix.CustomAction();

                customAction.Id = (string)row[0];

                int type = (int)row[1];

                if (MsiInterop.MsidbCustomActionTypeHideTarget == (type & MsiInterop.MsidbCustomActionTypeHideTarget))
                {
                    customAction.HideTarget = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbCustomActionTypeNoImpersonate == (type & MsiInterop.MsidbCustomActionTypeNoImpersonate))
                {
                    customAction.Impersonate = Wix.YesNoType.no;
                }

                if (MsiInterop.MsidbCustomActionTypeTSAware == (type & MsiInterop.MsidbCustomActionTypeTSAware))
                {
                    customAction.TerminalServerAware = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbCustomActionType64BitScript == (type & MsiInterop.MsidbCustomActionType64BitScript))
                {
                    customAction.Win64 = Wix.YesNoType.yes;
                }

                switch (type & MsiInterop.MsidbCustomActionTypeExecuteBits)
                {
                    case 0:
                        // this is the default value
                        break;
                    case MsiInterop.MsidbCustomActionTypeFirstSequence:
                        customAction.Execute = Wix.CustomAction.ExecuteType.firstSequence;
                        break;
                    case MsiInterop.MsidbCustomActionTypeOncePerProcess:
                        customAction.Execute = Wix.CustomAction.ExecuteType.oncePerProcess;
                        break;
                    case MsiInterop.MsidbCustomActionTypeClientRepeat:
                        customAction.Execute = Wix.CustomAction.ExecuteType.secondSequence;
                        break;
                    case MsiInterop.MsidbCustomActionTypeInScript:
                        customAction.Execute = Wix.CustomAction.ExecuteType.deferred;
                        break;
                    case MsiInterop.MsidbCustomActionTypeInScript + MsiInterop.MsidbCustomActionTypeRollback:
                        customAction.Execute = Wix.CustomAction.ExecuteType.rollback;
                        break;
                    case MsiInterop.MsidbCustomActionTypeInScript + MsiInterop.MsidbCustomActionTypeCommit:
                        customAction.Execute = Wix.CustomAction.ExecuteType.commit;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                switch (type & MsiInterop.MsidbCustomActionTypeReturnBits)
                {
                    case 0:
                        // this is the default value
                        break;
                    case MsiInterop.MsidbCustomActionTypeContinue:
                        customAction.Return = Wix.CustomAction.ReturnType.ignore;
                        break;
                    case MsiInterop.MsidbCustomActionTypeAsync:
                        customAction.Return = Wix.CustomAction.ReturnType.asyncWait;
                        break;
                    case MsiInterop.MsidbCustomActionTypeAsync + MsiInterop.MsidbCustomActionTypeContinue:
                        customAction.Return = Wix.CustomAction.ReturnType.asyncNoWait;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                int source = type & MsiInterop.MsidbCustomActionTypeSourceBits;
                switch (source)
                {
                    case MsiInterop.MsidbCustomActionTypeBinaryData:
                        customAction.BinaryKey = (string)row[2];
                        break;
                    case MsiInterop.MsidbCustomActionTypeSourceFile:
                        if (null != row[2])
                        {
                            customAction.FileKey = (string)row[2];
                        }
                        break;
                    case MsiInterop.MsidbCustomActionTypeDirectory:
                        if (null != row[2])
                        {
                            customAction.Directory = (string)row[2];
                        }
                        break;
                    case MsiInterop.MsidbCustomActionTypeProperty:
                        customAction.Property = (string)row[2];
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                switch (type & MsiInterop.MsidbCustomActionTypeTargetBits)
                {
                    case MsiInterop.MsidbCustomActionTypeDll:
                        customAction.DllEntry = (string)row[3];
                        break;
                    case MsiInterop.MsidbCustomActionTypeExe:
                        customAction.ExeCommand = (string)row[3];
                        break;
                    case MsiInterop.MsidbCustomActionTypeTextData:
                        if (MsiInterop.MsidbCustomActionTypeSourceFile == source)
                        {
                            customAction.Error = (string)row[3];
                        }
                        else
                        {
                            customAction.Value = (string)row[3];
                        }
                        break;
                    case MsiInterop.MsidbCustomActionTypeJScript:
                        if (MsiInterop.MsidbCustomActionTypeDirectory == source)
                        {
                            customAction.Script = Wix.CustomAction.ScriptType.jscript;
                            customAction.Content = (string)row[3];
                        }
                        else
                        {
                            customAction.JScriptCall = (string)row[3];
                        }
                        break;
                    case MsiInterop.MsidbCustomActionTypeVBScript:
                        if (MsiInterop.MsidbCustomActionTypeDirectory == source)
                        {
                            customAction.Script = Wix.CustomAction.ScriptType.vbscript;
                            customAction.Content = (string)row[3];
                        }
                        else
                        {
                            customAction.VBScriptCall = (string)row[3];
                        }
                        break;
                    case MsiInterop.MsidbCustomActionTypeInstall:
                        this.core.OnMessage(WixWarnings.NestedInstall(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        continue;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                this.core.RootElement.AddChild(customAction);
                this.core.IndexElement(row, customAction);
            }
        }

        /// <summary>
        /// Decompile the CompLocator table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileCompLocatorTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ComponentSearch componentSearch = new Wix.ComponentSearch();

                componentSearch.Id = (string)row[0];

                componentSearch.Guid = (string)row[1];

                if (null != row[2])
                {
                    switch ((int)row[2])
                    {
                        case MsiInterop.MsidbLocatorTypeDirectory:
                            componentSearch.Type = Wix.ComponentSearch.TypeType.directory;
                            break;
                        case MsiInterop.MsidbLocatorTypeFileName:
                            // this is the default value
                            break;
                        default:
                            this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[2].Column.Name, row[2]));
                            break;
                    }
                }

                this.core.IndexElement(row, componentSearch);
            }
        }

        /// <summary>
        /// Decompile the Complus table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileComplusTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                if (null != row[1])
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[0]);

                    if (null != component)
                    {
                        component.ComPlusFlags = (int)row[1];
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[0], "Component"));
                    }
                }
            }
        }

        /// <summary>
        /// Decompile the Component table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileComponentTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Component component = new Wix.Component();

                component.Id = (string)row[0];

                component.Guid = (string)row[1];

                int attributes = (int)row[3];

                if (MsiInterop.MsidbComponentAttributesSourceOnly == (attributes & MsiInterop.MsidbComponentAttributesSourceOnly))
                {
                    component.Location = Wix.Component.LocationType.source;
                }
                else if (MsiInterop.MsidbComponentAttributesOptional == (attributes & MsiInterop.MsidbComponentAttributesOptional))
                {
                    component.Location = Wix.Component.LocationType.either;
                }

                if (MsiInterop.MsidbComponentAttributesSharedDllRefCount == (attributes & MsiInterop.MsidbComponentAttributesSharedDllRefCount))
                {
                    component.SharedDllRefCount = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbComponentAttributesPermanent == (attributes & MsiInterop.MsidbComponentAttributesPermanent))
                {
                    component.Permanent = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbComponentAttributesTransitive == (attributes & MsiInterop.MsidbComponentAttributesTransitive))
                {
                    component.Transitive = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbComponentAttributesNeverOverwrite == (attributes & MsiInterop.MsidbComponentAttributesNeverOverwrite))
                {
                    component.NeverOverwrite = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbComponentAttributes64bit == (attributes & MsiInterop.MsidbComponentAttributes64bit))
                {
                    component.Win64 = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbComponentAttributesDisableRegistryReflection == (attributes & MsiInterop.MsidbComponentAttributesDisableRegistryReflection))
                {
                    component.DisableRegistryReflection = Wix.YesNoType.yes;
                }

                if (null != row[4])
                {
                    Wix.Condition condition = new Wix.Condition();

                    condition.Content = (string)row[4];

                    component.AddChild(condition);
                }

                Wix.Directory directory = (Wix.Directory)this.core.GetIndexedElement("Directory", (string)row[2]);
                if (null != directory)
                {
                    directory.AddChild(component);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Directory_", (string)row[2], "Directory"));
                }
                this.core.IndexElement(row, component);
            }
        }

        /// <summary>
        /// Decompile the Condition table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileConditionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Condition condition = new Wix.Condition();

                condition.Level = (int)row[1];

                if (null != row[2])
                {
                    condition.Content = (string)row[2];
                }

                Wix.Feature feature = (Wix.Feature)this.core.GetIndexedElement("Feature", (string)row[0]);
                if (null != feature)
                {
                    feature.AddChild(condition);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Feature_", (string)row[0], "Feature"));
                }
            }
        }

        /// <summary>
        /// Decompile the Dialog table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileDialogTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Dialog dialog = new Wix.Dialog();

                dialog.Id = (string)row[0];

                dialog.X = (int)row[1];

                dialog.Y = (int)row[2];

                dialog.Width = (int)row[3];

                dialog.Height = (int)row[4];

                if (null != row[5])
                {
                    int attributes = (int)row[5];

                    if (0 == (attributes & MsiInterop.MsidbDialogAttributesVisible))
                    {
                        dialog.Hidden = Wix.YesNoType.yes;
                    }

                    if (0 == (attributes & MsiInterop.MsidbDialogAttributesModal))
                    {
                        dialog.Modeless = Wix.YesNoType.yes;
                    }

                    if (0 == (attributes & MsiInterop.MsidbDialogAttributesMinimize))
                    {
                        dialog.NoMinimize = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesSysModal == (attributes & MsiInterop.MsidbDialogAttributesSysModal))
                    {
                        dialog.SystemModal = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesKeepModeless == (attributes & MsiInterop.MsidbDialogAttributesKeepModeless))
                    {
                        dialog.KeepModeless = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesTrackDiskSpace == (attributes & MsiInterop.MsidbDialogAttributesTrackDiskSpace))
                    {
                        dialog.TrackDiskSpace = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesUseCustomPalette == (attributes & MsiInterop.MsidbDialogAttributesUseCustomPalette))
                    {
                        dialog.CustomPalette = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesRTLRO == (attributes & MsiInterop.MsidbDialogAttributesRTLRO))
                    {
                        dialog.RightToLeft = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesRightAligned == (attributes & MsiInterop.MsidbDialogAttributesRightAligned))
                    {
                        dialog.RightAligned = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesLeftScroll == (attributes & MsiInterop.MsidbDialogAttributesLeftScroll))
                    {
                        dialog.LeftScroll = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbDialogAttributesError == (attributes & MsiInterop.MsidbDialogAttributesError))
                    {
                        dialog.ErrorDialog = Wix.YesNoType.yes;
                    }
                }

                if (null != row[6])
                {
                    dialog.Title = (string)row[6];
                }

                this.core.UIElement.AddChild(dialog);
                this.core.IndexElement(row, dialog);
            }
        }

        /// <summary>
        /// Decompile the Directory table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileDirectoryTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Directory directory = new Wix.Directory();

                directory.Id = (string)row[0];

                string[] names = Installer.GetNames((string)row[2]);
                if (null != names[0] && "." != names[0])
                {
                    if (null != names[1])
                    {
                        directory.ShortName = names[0];
                    }
                    else
                    {
                        directory.Name = names[0];
                    }
                }

                if (null != names[1])
                {
                    directory.Name = names[1];
                }

                if (null != names[2])
                {
                    if (null != names[3])
                    {
                        directory.ShortSourceName = names[2];
                    }
                    else
                    {
                        directory.SourceName = names[2];
                    }
                }

                if (null != names[3])
                {
                    directory.SourceName = names[3];
                }

                this.core.IndexElement(row, directory);
            }

            // nest the directories
            foreach (Row row in table.Rows)
            {
                Wix.Directory directory = (Wix.Directory)this.core.GetIndexedElement(row);

                if (null == row[1])
                {
                    this.core.RootElement.AddChild(directory);
                }
                else
                {
                    Wix.Directory parentDirectory = (Wix.Directory)this.core.GetIndexedElement("Directory", (string)row[1]);

                    if (null == parentDirectory)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Directory_Parent", (string)row[1], "Directory"));
                    }
                    else if (parentDirectory == directory) // another way to specify a root directory
                    {
                        this.core.RootElement.AddChild(directory);
                    }
                    else
                    {
                        parentDirectory.AddChild(directory);
                    }
                }
            }
        }

        /// <summary>
        /// Decompile the DrLocator table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileDrLocatorTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.DirectorySearch directorySearch = new Wix.DirectorySearch();

                directorySearch.Id = (string)row[0];

                if (null != row[2])
                {
                    directorySearch.Path = (string)row[2];
                }

                if (null != row[3])
                {
                    directorySearch.Depth = (int)row[3];
                }

                this.core.IndexElement(row, directorySearch);
            }
        }

        /// <summary>
        /// Decompile the DuplicateFile table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileDuplicateFileTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.CopyFile copyFile = new Wix.CopyFile();

                copyFile.Id = (string)row[0];

                copyFile.FileId = (string)row[2];

                if (null != row[3])
                {
                    string[] names = Installer.GetNames((string)row[3]);

                    if (null != names[0])
                    {
                        copyFile.DestinationName = names[0];
                    }

                    if (null != names[1])
                    {
                        copyFile.DestinationLongName = names[1];
                    }
                }

                // destination directory/property is set in FinalizeDuplicateMoveFileTables

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(copyFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
                this.core.IndexElement(row, copyFile);
            }
        }

        /// <summary>
        /// Decompile the Environment table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileEnvironmentTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Environment environment = new Wix.Environment();

                environment.Id = (string)row[0];

                bool done = false;
                bool permanent = true;
                string name = (string)row[1];
                for (int i = 0; i < name.Length && !done; i++)
                {
                    switch (name[i])
                    {
                        case '=':
                            environment.Action = Wix.Environment.ActionType.set;
                            break;
                        case '+':
                            environment.Action = Wix.Environment.ActionType.create;
                            break;
                        case '-':
                            permanent = false;
                            break;
                        case '!':
                            environment.Action = Wix.Environment.ActionType.remove;
                            break;
                        case '*':
                            environment.System = Wix.YesNoType.yes;
                            break;
                        default:
                            environment.Name = name.Substring(i);
                            done = true;
                            break;
                    }
                }

                if (permanent)
                {
                    environment.Permanent = Wix.YesNoType.yes;
                }

                if (null != row[2])
                {
                    string value = (string)row[2];

                    if (value.StartsWith("[~]"))
                    {
                        environment.Part = Wix.Environment.PartType.last;

                        if (3 < value.Length)
                        {
                            environment.Separator = value.Substring(3, 1);
                            environment.Value = value.Substring(4);
                        }
                    }
                    else if (value.EndsWith("[~]"))
                    {
                        environment.Part = Wix.Environment.PartType.first;

                        if (3 < value.Length)
                        {
                            environment.Separator = value.Substring(value.Length - 4, 1);
                            environment.Value = value.Substring(0, value.Length - 4);
                        }
                    }
                    else
                    {
                        environment.Value = value;
                    }
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[3]);
                if (null != component)
                {
                    component.AddChild(environment);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[3], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the Error table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileErrorTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Error error = new Wix.Error();

                error.Id = (int)row[0];

                error.Content = (string)row[1];

                this.core.UIElement.AddChild(error);
            }
        }

        /// <summary>
        /// Decompile the EventMapping table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileEventMappingTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Subscribe subscribe = new Wix.Subscribe();

                subscribe.Event = (string)row[2];

                subscribe.Attribute = (string)row[3];

                Wix.Control control = (Wix.Control)this.core.GetIndexedElement("Control", (string)row[0], (string)row[1]);
                if (null != control)
                {
                    control.AddChild(subscribe);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Dialog_", (string)row[0], "Control_", (string)row[1], "Control"));
                }
            }
        }

        /// <summary>
        /// Decompile the Extension table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileExtensionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Extension extension = new Wix.Extension();

                extension.Advertise = Wix.YesNoType.yes;

                extension.Id = (string)row[0];

                if (null != row[3])
                {
                    Wix.MIME mime = (Wix.MIME)this.core.GetIndexedElement("MIME", (string)row[3]);

                    if (null != mime)
                    {
                        mime.Default = Wix.YesNoType.yes;
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "MIME_", (string)row[3], "MIME"));
                    }
                }

                if (null != row[2])
                {
                    Wix.ProgId progId = (Wix.ProgId)this.core.GetIndexedElement("ProgId", (string)row[2]);

                    if (null != progId)
                    {
                        progId.AddChild(extension);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "ProgId_", (string)row[2], "ProgId"));
                    }
                }
                else
                {
                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);

                    if (null != component)
                    {
                        component.AddChild(extension);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                    }
                }

                this.core.IndexElement(row, extension);
            }
        }

        /// <summary>
        /// Decompile the ExternalFiles table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileExternalFilesTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ExternalFile externalFile = new Wix.ExternalFile();

                externalFile.File = (string)row[1];

                externalFile.Source = (string)row[2];

                if (null != row[3])
                {
                    string[] symbolPaths = ((string)row[3]).Split(';');

                    foreach (string symbolPathString in symbolPaths)
                    {
                        Wix.SymbolPath symbolPath = new Wix.SymbolPath();

                        symbolPath.Path = symbolPathString;

                        externalFile.AddChild(symbolPath);
                    }
                }

                if (null != row[4] && null != row[5])
                {
                    string[] ignoreOffsets = ((string)row[4]).Split(',');
                    string[] ignoreLengths = ((string)row[5]).Split(',');

                    if (ignoreOffsets.Length == ignoreLengths.Length)
                    {
                        for (int i = 0; i < ignoreOffsets.Length; i++)
                        {
                            Wix.IgnoreRange ignoreRange = new Wix.IgnoreRange();

                            if (ignoreOffsets[i].StartsWith("0x"))
                            {
                                ignoreRange.Offset = Convert.ToInt32(ignoreOffsets[i].Substring(2), 16);
                            }
                            else
                            {
                                ignoreRange.Offset = Convert.ToInt32(ignoreOffsets[i], CultureInfo.InvariantCulture);
                            }

                            if (ignoreLengths[i].StartsWith("0x"))
                            {
                                ignoreRange.Length = Convert.ToInt32(ignoreLengths[i].Substring(2), 16);
                            }
                            else
                            {
                                ignoreRange.Length = Convert.ToInt32(ignoreLengths[i], CultureInfo.InvariantCulture);
                            }

                            externalFile.AddChild(ignoreRange);
                        }
                    }
                    else
                    {
                        // TODO: warn
                    }
                }
                else if (null != row[4] || null != row[5])
                {
                    // TODO: warn about mismatch between columns
                }

                // the RetainOffsets column is handled in FinalizeFamilyFileRangesTable

                if (null != row[7])
                {
                    externalFile.Order = (int)row[7];
                }

                Wix.Family family = (Wix.Family)this.core.GetIndexedElement("ImageFamilies", (string)row[0]);
                if (null != family)
                {
                    family.AddChild(externalFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Family", (string)row[0], "ImageFamilies"));
                }
                this.core.IndexElement(row, externalFile);
            }
        }

        /// <summary>
        /// Decompile the Feature table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileFeatureTable(Table table)
        {
            SortedList sortedFeatures = new SortedList();

            foreach (Row row in table.Rows)
            {
                Wix.Feature feature = new Wix.Feature();

                feature.Id = (string)row[0];

                if (null != row[2])
                {
                    feature.Title = (string)row[2];
                }

                if (null != row[3])
                {
                    feature.Description = (string)row[3];
                }

                if (null == row[4])
                {
                    feature.Display = "hidden";
                }
                else
                {
                    int display = (int)row[4];

                    if (0 == display)
                    {
                        feature.Display = "hidden";
                    }
                    else if (1 == display % 2)
                    {
                        feature.Display = "expand";
                    }
                }

                feature.Level = (int)row[5];

                if (null != row[6])
                {
                    feature.ConfigurableDirectory = (string)row[6];
                }

                int attributes = (int)row[7];

                if (MsiInterop.MsidbFeatureAttributesFavorSource == (attributes & MsiInterop.MsidbFeatureAttributesFavorSource) && MsiInterop.MsidbFeatureAttributesFollowParent == (attributes & MsiInterop.MsidbFeatureAttributesFollowParent))
                {
                    // TODO: display a warning for setting favor local and follow parent together
                }
                else if (MsiInterop.MsidbFeatureAttributesFavorSource == (attributes & MsiInterop.MsidbFeatureAttributesFavorSource))
                {
                    feature.InstallDefault = Wix.Feature.InstallDefaultType.source;
                }
                else if (MsiInterop.MsidbFeatureAttributesFollowParent == (attributes & MsiInterop.MsidbFeatureAttributesFollowParent))
                {
                    feature.InstallDefault = Wix.Feature.InstallDefaultType.followParent;
                }

                if (MsiInterop.MsidbFeatureAttributesFavorAdvertise == (attributes & MsiInterop.MsidbFeatureAttributesFavorAdvertise))
                {
                    feature.TypicalDefault = Wix.Feature.TypicalDefaultType.advertise;
                }

                if (MsiInterop.MsidbFeatureAttributesDisallowAdvertise == (attributes & MsiInterop.MsidbFeatureAttributesDisallowAdvertise) &&
                    MsiInterop.MsidbFeatureAttributesNoUnsupportedAdvertise == (attributes & MsiInterop.MsidbFeatureAttributesNoUnsupportedAdvertise))
                {
                    // TODO: display a warning about setting disallow advertise and no unsupported advertise together
                }
                else if (MsiInterop.MsidbFeatureAttributesDisallowAdvertise == (attributes & MsiInterop.MsidbFeatureAttributesDisallowAdvertise))
                {
                    feature.AllowAdvertise = Wix.Feature.AllowAdvertiseType.no;
                }
                else if (MsiInterop.MsidbFeatureAttributesNoUnsupportedAdvertise == (attributes & MsiInterop.MsidbFeatureAttributesNoUnsupportedAdvertise))
                {
                    feature.AllowAdvertise = Wix.Feature.AllowAdvertiseType.system;
                }

                if (MsiInterop.MsidbFeatureAttributesUIDisallowAbsent == (attributes & MsiInterop.MsidbFeatureAttributesUIDisallowAbsent))
                {
                    feature.Absent = Wix.Feature.AbsentType.disallow;
                }

                this.core.IndexElement(row, feature);

                // sort the features by their display column (and append the identifier to ensure unique keys)
                sortedFeatures.Add(String.Format(CultureInfo.InvariantCulture, "{0:00000}|{1}", Convert.ToInt32(row[4], CultureInfo.InvariantCulture), row[0]), row);
            }

            // nest the features
            foreach (Row row in sortedFeatures.Values)
            {
                Wix.Feature feature = (Wix.Feature)this.core.GetIndexedElement("Feature", (string)row[0]);

                if (null == row[1])
                {
                    this.core.RootElement.AddChild(feature);
                }
                else
                {
                    Wix.Feature parentFeature = (Wix.Feature)this.core.GetIndexedElement("Feature", (string)row[1]);

                    if (null == parentFeature)
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Feature_Parent", (string)row[1], "Feature"));
                    }
                    else if (parentFeature == feature)
                    {
                        // TODO: display a warning about self-nesting
                    }
                    else
                    {
                        parentFeature.AddChild(feature);
                    }
                }
            }
        }

        /// <summary>
        /// Decompile the FeatureComponents table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileFeatureComponentsTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ComponentRef componentRef = new Wix.ComponentRef();

                componentRef.Id = (string)row[1];

                Wix.Feature parentFeature = (Wix.Feature)this.core.GetIndexedElement("Feature", (string)row[0]);
                if (null != parentFeature)
                {
                    parentFeature.AddChild(componentRef);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Feature_", (string)row[0], "Feature"));
                }
                this.core.IndexElement(row, componentRef);
            }
        }

        /// <summary>
        /// Decompile the File table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileFileTable(Table table)
        {
            foreach (FileRow fileRow in table.Rows)
            {
                Wix.File file = new Wix.File();

                file.Id = fileRow.File;

                string[] names = Installer.GetNames(fileRow.FileName);
                if (null != names[0] && null != names[1])
                {
                    file.ShortName = names[0];
                    file.Name = names[1];
                }
                else if (null != names[0])
                {
                    file.Name = names[0];
                }

                if (null != fileRow.Version && 0 < fileRow.Version.Length)
                {
                    if (!Char.IsDigit(fileRow.Version[0]))
                    {
                        file.CompanionFile = fileRow.Version;
                    }
                }

                if (MsiInterop.MsidbFileAttributesReadOnly == (fileRow.Attributes & MsiInterop.MsidbFileAttributesReadOnly))
                {
                    file.ReadOnly = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbFileAttributesHidden == (fileRow.Attributes & MsiInterop.MsidbFileAttributesHidden))
                {
                    file.Hidden = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbFileAttributesSystem == (fileRow.Attributes & MsiInterop.MsidbFileAttributesSystem))
                {
                    file.System = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbFileAttributesVital == (fileRow.Attributes & MsiInterop.MsidbFileAttributesVital))
                {
                    file.Vital = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbFileAttributesChecksum == (fileRow.Attributes & MsiInterop.MsidbFileAttributesChecksum))
                {
                    file.Checksum = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbFileAttributesNoncompressed == (fileRow.Attributes & MsiInterop.MsidbFileAttributesNoncompressed) &&
                    MsiInterop.MsidbFileAttributesCompressed == (fileRow.Attributes & MsiInterop.MsidbFileAttributesCompressed))
                {
                    // TODO: error
                }
                else if (MsiInterop.MsidbFileAttributesNoncompressed == (fileRow.Attributes & MsiInterop.MsidbFileAttributesNoncompressed))
                {
                    file.Compressed = Wix.YesNoDefaultType.no;
                }
                else if (MsiInterop.MsidbFileAttributesCompressed == (fileRow.Attributes & MsiInterop.MsidbFileAttributesCompressed))
                {
                    file.Compressed = Wix.YesNoDefaultType.yes;
                }

                this.core.IndexElement(fileRow, file);
            }
        }

        /// <summary>
        /// Decompile the FileSFPCatalog table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileFileSFPCatalogTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.SFPFile sfpFile = new Wix.SFPFile();

                sfpFile.Id = (string)row[0];

                Wix.SFPCatalog sfpCatalog = (Wix.SFPCatalog)this.core.GetIndexedElement("SFPCatalog", (string)row[1]);
                if (null != sfpCatalog)
                {
                    sfpCatalog.AddChild(sfpFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "SFPCatalog_", (string)row[1], "SFPCatalog"));
                }
            }
        }

        /// <summary>
        /// Decompile the Font table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileFontTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.File file = (Wix.File)this.core.GetIndexedElement("File", (string)row[0]);

                if (null != file)
                {
                    if (null != row[1])
                    {
                        file.FontTitle = (string)row[1];
                    }
                    else
                    {
                        file.TrueType = Wix.YesNoType.yes;
                    }
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "File_", (string)row[0], "File"));
                }
            }
        }

        /// <summary>
        /// Decompile the Icon table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileIconTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Icon icon = new Wix.Icon();

                icon.Id = (string)row[0];

                icon.SourceFile = (string)row[1];

                this.core.RootElement.AddChild(icon);
            }
        }

        /// <summary>
        /// Decompile the ImageFamilies table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileImageFamiliesTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Family family = new Wix.Family();

                family.Name = (string)row[0];

                if (null != row[1])
                {
                    family.MediaSrcProp = (string)row[1];
                }

                if (null != row[2])
                {
                    family.DiskId = (int)row[2];
                }

                if (null != row[3])
                {
                    family.SequenceStart = (int)row[3];
                }

                if (null != row[4])
                {
                    family.DiskPrompt = (string)row[4];
                }

                if (null != row[5])
                {
                    family.VolumeLabel = (string)row[5];
                }

                this.core.RootElement.AddChild(family);
                this.core.IndexElement(row, family);
            }
        }

        /// <summary>
        /// Decompile the IniFile table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileIniFileTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.IniFile iniFile = new Wix.IniFile();

                iniFile.Id = (string)row[0];

                string[] names = Installer.GetNames((string)row[1]);

                if (null != names[0])
                {
                    iniFile.Name = names[0];
                }

                if (null != names[1])
                {
                    iniFile.LongName = names[1];
                }

                if (null != row[2])
                {
                    iniFile.Directory = (string)row[2];
                }

                iniFile.Section = (string)row[3];

                iniFile.Key = (string)row[4];

                iniFile.Value = (string)row[5];

                switch ((int)row[6])
                {
                    case MsiInterop.MsidbIniFileActionAddLine:
                        iniFile.Action = Wix.IniFile.ActionType.addLine;
                        break;
                    case MsiInterop.MsidbIniFileActionCreateLine:
                        iniFile.Action = Wix.IniFile.ActionType.createLine;
                        break;
                    case MsiInterop.MsidbIniFileActionAddTag:
                        iniFile.Action = Wix.IniFile.ActionType.addTag;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[6].Column.Name, row[6]));
                        break;
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[7]);
                if (null != component)
                {
                    component.AddChild(iniFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[7], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the IniLocator table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileIniLocatorTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.IniFileSearch iniFileSearch = new Wix.IniFileSearch();

                iniFileSearch.Id = (string)row[0];

                string[] names = Installer.GetNames((string)row[1]);
                if (null != names[0] && null != names[1])
                {
                    iniFileSearch.ShortName = names[0];
                    iniFileSearch.Name = names[1];
                }
                else if (null != names[0])
                {
                    iniFileSearch.Name = names[0];
                }

                iniFileSearch.Section = (string)row[2];

                iniFileSearch.Key = (string)row[3];

                if (null != row[4])
                {
                    int field = (int)row[4];

                    if (0 != field)
                    {
                        iniFileSearch.Field = field;
                    }
                }

                if (null != row[5])
                {
                    switch ((int)row[5])
                    {
                        case MsiInterop.MsidbLocatorTypeDirectory:
                            iniFileSearch.Type = Wix.IniFileSearch.TypeType.directory;
                            break;
                        case MsiInterop.MsidbLocatorTypeFileName:
                            // this is the default value
                            break;
                        case MsiInterop.MsidbLocatorTypeRawValue:
                            iniFileSearch.Type = Wix.IniFileSearch.TypeType.raw;
                            break;
                        default:
                            this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[5].Column.Name, row[5]));
                            break;
                    }
                }

                this.core.IndexElement(row, iniFileSearch);
            }
        }

        /// <summary>
        /// Decompile the IsolatedComponent table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileIsolatedComponentTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.IsolateComponent isolateComponent = new Wix.IsolateComponent();

                isolateComponent.Shared = (string)row[0];

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(isolateComponent);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the LaunchCondition table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileLaunchConditionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Condition condition = new Wix.Condition();

                condition.Content = (string)row[0];

                condition.Message = (string)row[1];

                this.core.RootElement.AddChild(condition);
            }
        }

        /// <summary>
        /// Decompile the ListBox table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileListBoxTable(Table table)
        {
            Wix.ListBox listBox = null;
            SortedList listBoxRows = new SortedList();

            // sort the list boxes by their property and order
            foreach (Row row in table.Rows)
            {
                listBoxRows.Add(String.Concat("{0}|{1:0000000000}", row[0], row[1]), row);
            }

            foreach (Row row in listBoxRows.Values)
            {
                if (null == listBox || (string)row[0] != listBox.Property)
                {
                    listBox = new Wix.ListBox();

                    listBox.Property = (string)row[0];

                    this.core.UIElement.AddChild(listBox);
                }

                Wix.ListItem listItem = new Wix.ListItem();

                listItem.Value = (string)row[2];

                if (null != row[3])
                {
                    listItem.Text = (string)row[3];
                }

                listBox.AddChild(listItem);
            }
        }

        /// <summary>
        /// Decompile the ListView table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileListViewTable(Table table)
        {
            Wix.ListView listView = null;
            SortedList listViewRows = new SortedList();

            // sort the list views by their property and order
            foreach (Row row in table.Rows)
            {
                listViewRows.Add(String.Concat("{0}|{1:0000000000}", row[0], row[1]), row);
            }

            foreach (Row row in listViewRows.Values)
            {
                if (null == listView || (string)row[0] != listView.Property)
                {
                    listView = new Wix.ListView();

                    listView.Property = (string)row[0];

                    this.core.UIElement.AddChild(listView);
                }

                Wix.ListItem listItem = new Wix.ListItem();

                listItem.Value = (string)row[2];

                if (null != row[3])
                {
                    listItem.Text = (string)row[3];
                }

                if (null != row[4])
                {
                    listItem.Icon = (string)row[4];
                }

                listView.AddChild(listItem);
            }
        }

        /// <summary>
        /// Decompile the LockPermissions table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileLockPermissionsTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Permission permission = new Wix.Permission();
                string[] specialPermissions;

                switch ((string)row[1])
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
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, row.Table.Name, row.Fields[1].Column.Name, row[1]));
                        return;
                }

                int permissionBits = (int)row[4];
                for (int i = 0; i < 32; i++)
                {
                    if (0 != ((permissionBits >> i) & 1))
                    {
                        string name = null;

                        if (16 > i && specialPermissions.Length > i)
                        {
                            name = specialPermissions[i];
                        }
                        else if (28 > i && Common.StandardPermissions.Length > (i - 16))
                        {
                            name = Common.StandardPermissions[i - 16];
                        }
                        else if (0 <= (i - 28) && Common.GenericPermissions.Length > (i - 28))
                        {
                            name = Common.GenericPermissions[i - 28];
                        }

                        if (null == name)
                        {
                            this.core.OnMessage(WixWarnings.UnknownPermission(row.SourceLineNumbers, row.Table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), i));
                        }
                        else
                        {
                            switch (name)
                            {
                                case "Append":
                                    permission.Append = Wix.YesNoType.yes;
                                    break;
                                case "ChangePermission":
                                    permission.ChangePermission = Wix.YesNoType.yes;
                                    break;
                                case "CreateChild":
                                    permission.CreateChild = Wix.YesNoType.yes;
                                    break;
                                case "CreateFile":
                                    permission.CreateFile = Wix.YesNoType.yes;
                                    break;
                                case "CreateLink":
                                    permission.CreateLink = Wix.YesNoType.yes;
                                    break;
                                case "CreateSubkeys":
                                    permission.CreateSubkeys = Wix.YesNoType.yes;
                                    break;
                                case "Delete":
                                    permission.Delete = Wix.YesNoType.yes;
                                    break;
                                case "DeleteChild":
                                    permission.DeleteChild = Wix.YesNoType.yes;
                                    break;
                                case "EnumerateSubkeys":
                                    permission.EnumerateSubkeys = Wix.YesNoType.yes;
                                    break;
                                case "Execute":
                                    permission.Execute = Wix.YesNoType.yes;
                                    break;
                                case "GenericAll":
                                    permission.GenericAll = Wix.YesNoType.yes;
                                    break;
                                case "GenericExecute":
                                    permission.GenericExecute = Wix.YesNoType.yes;
                                    break;
                                case "GenericRead":
                                    permission.GenericRead = Wix.YesNoType.yes;
                                    break;
                                case "GenericWrite":
                                    permission.GenericWrite = Wix.YesNoType.yes;
                                    break;
                                case "Notify":
                                    permission.Notify = Wix.YesNoType.yes;
                                    break;
                                case "Read":
                                    permission.Read = Wix.YesNoType.yes;
                                    break;
                                case "ReadAttributes":
                                    permission.ReadAttributes = Wix.YesNoType.yes;
                                    break;
                                case "ReadExtendedAttributes":
                                    permission.ReadExtendedAttributes = Wix.YesNoType.yes;
                                    break;
                                case "ReadPermission":
                                    permission.ReadPermission = Wix.YesNoType.yes;
                                    break;
                                case "Synchronize":
                                    permission.Synchronize = Wix.YesNoType.yes;
                                    break;
                                case "TakeOwnership":
                                    permission.TakeOwnership = Wix.YesNoType.yes;
                                    break;
                                case "Traverse":
                                    permission.Traverse = Wix.YesNoType.yes;
                                    break;
                                case "Write":
                                    permission.Write = Wix.YesNoType.yes;
                                    break;
                                case "WriteAttributes":
                                    permission.WriteAttributes = Wix.YesNoType.yes;
                                    break;
                                case "WriteExtendedAttributes":
                                    permission.WriteExtendedAttributes = Wix.YesNoType.yes;
                                    break;
                                default:
                                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unknown permission attribute '{0}'.", name));
                            }
                        }
                    }
                }

                if (null != row[2])
                {
                    permission.Domain = (string)row[2];
                }

                permission.User = (string)row[3];

                this.core.IndexElement(row, permission);
            }
        }

        /// <summary>
        /// Decompile the Media table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileMediaTable(Table table)
        {
            foreach (MediaRow mediaRow in table.Rows)
            {
                Wix.Media media = new Wix.Media();

                media.Id = mediaRow.DiskId;

                if (null != mediaRow.DiskPrompt)
                {
                    media.DiskPrompt = mediaRow.DiskPrompt;
                }

                if (null != mediaRow.Cabinet)
                {
                    string cabinet = mediaRow.Cabinet;

                    if (cabinet.StartsWith("#"))
                    {
                        media.EmbedCab = Wix.YesNoType.yes;
                        cabinet = cabinet.Substring(1);
                    }

                    media.Cabinet = cabinet;
                }

                if (null != mediaRow.VolumeLabel)
                {
                    media.VolumeLabel = mediaRow.VolumeLabel;
                }

                this.core.RootElement.AddChild(media);
                this.core.IndexElement(mediaRow, media);
            }
        }

        /// <summary>
        /// Decompile the MIME table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileMIMETable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.MIME mime = new Wix.MIME();

                mime.ContentType = (string)row[0];

                if (null != row[2])
                {
                    mime.Class = (string)row[2];
                }

                this.core.IndexElement(row, mime);
            }
        }

        /// <summary>
        /// Decompile the ModuleConfiguration table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileModuleConfigurationTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Configuration configuration = new Wix.Configuration();

                configuration.Name = (string)row[0];

                switch ((int)row[1])
                {
                    case 0:
                        configuration.Format = Wix.Configuration.FormatType.Text;
                        break;
                    case 1:
                        configuration.Format = Wix.Configuration.FormatType.Key;
                        break;
                    case 2:
                        configuration.Format = Wix.Configuration.FormatType.Integer;
                        break;
                    case 3:
                        configuration.Format = Wix.Configuration.FormatType.Bitfield;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                if (null != row[2])
                {
                    configuration.Type = (string)row[2];
                }

                if (null != row[3])
                {
                    configuration.ContextData = (string)row[3];
                }

                if (null != row[4])
                {
                    configuration.DefaultValue = (string)row[4];
                }

                if (null != row[5])
                {
                    int attributes = (int)row[5];

                    if (MsiInterop.MsidbMsmConfigurableOptionKeyNoOrphan == (attributes & MsiInterop.MsidbMsmConfigurableOptionKeyNoOrphan))
                    {
                        configuration.KeyNoOrphan = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbMsmConfigurableOptionNonNullable == (attributes & MsiInterop.MsidbMsmConfigurableOptionNonNullable))
                    {
                        configuration.NonNullable = Wix.YesNoType.yes;
                    }

                    if (3 < attributes)
                    {
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[5].Column.Name, row[5]));
                    }
                }

                if (null != row[6])
                {
                    configuration.DisplayName = (string)row[6];
                }

                if (null != row[7])
                {
                    configuration.Description = (string)row[7];
                }

                if (null != row[8])
                {
                    configuration.HelpLocation = (string)row[8];
                }

                if (null != row[9])
                {
                    configuration.HelpKeyword = (string)row[9];
                }

                this.core.RootElement.AddChild(configuration);
            }
        }

        /// <summary>
        /// Decompile the ModuleDependency table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileModuleDependencyTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Dependency dependency = new Wix.Dependency();

                dependency.RequiredId = (string)row[2];

                dependency.RequiredLanguage = Convert.ToInt32(row[3], CultureInfo.InvariantCulture);

                if (null != row[4])
                {
                    dependency.RequiredVersion = (string)row[4];
                }

                this.core.RootElement.AddChild(dependency);
            }
        }

        /// <summary>
        /// Decompile the ModuleExclusion table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileModuleExclusionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Exclusion exclusion = new Wix.Exclusion();

                exclusion.ExcludedId = (string)row[2];

                int excludedLanguage = Convert.ToInt32((string)row[3], CultureInfo.InvariantCulture);
                if (0 < excludedLanguage)
                {
                    exclusion.ExcludeLanguage = excludedLanguage;
                }
                else if (0 > excludedLanguage)
                {
                    exclusion.ExcludeExceptLanguage = -excludedLanguage;
                }

                if (null != row[4])
                {
                    exclusion.ExcludedMinVersion = (string)row[4];
                }

                if (null != row[5])
                {
                    exclusion.ExcludedMinVersion = (string)row[5];
                }

                this.core.RootElement.AddChild(exclusion);
            }
        }

        /// <summary>
        /// Decompile the ModuleIgnoreTable table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileModuleIgnoreTableTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                string tableName = (string)row[0];

                // the linker automatically adds a ModuleIgnoreTable row for some tables
                if ("ModuleConfiguration" != tableName && "ModuleSubstitution" != tableName)
                {
                    Wix.IgnoreTable ignoreTable = new Wix.IgnoreTable();

                    ignoreTable.Id = tableName;

                    this.core.RootElement.AddChild(ignoreTable);
                }
            }
        }

        /// <summary>
        /// Decompile the ModuleSignature table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileModuleSignatureTable(Table table)
        {
            if (1 == table.Rows.Count)
            {
                Row row = table.Rows[0];

                Wix.Module module = (Wix.Module)this.core.RootElement;

                module.Id = (string)row[0];

                module.Language = (string)row[1];

                module.Version = (string)row[2];
            }
            else
            {
                // TODO: warn
            }
        }

        /// <summary>
        /// Decompile the ModuleSubstitution table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileModuleSubstitutionTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Substitution substitution = new Wix.Substitution();

                substitution.Table = (string)row[0];

                substitution.Row = (string)row[1];

                substitution.Column = (string)row[2];

                if (null != row[3])
                {
                    substitution.Value = (string)row[3];
                }

                this.core.RootElement.AddChild(substitution);
            }
        }

        /// <summary>
        /// Decompile the MoveFile table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileMoveFileTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.CopyFile copyFile = new Wix.CopyFile();

                copyFile.Id = (string)row[0];

                if (null != row[2])
                {
                    copyFile.SourceName = (string)row[2];
                }

                if (null != row[3])
                {
                    string[] names = Installer.GetNames((string)row[3]);

                    if (null != names[0])
                    {
                        copyFile.DestinationName = names[0];
                    }

                    if (null != names[1])
                    {
                        copyFile.DestinationLongName = names[1];
                    }
                }

                // source/destination directory/property is set in FinalizeDuplicateMoveFileTables

                switch ((int)row[6])
                {
                    case 0:
                        break;
                    case MsiInterop.MsidbMoveFileOptionsMove:
                        copyFile.Delete = Wix.YesNoType.yes;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[6].Column.Name, row[6]));
                        break;
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(copyFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
                this.core.IndexElement(row, copyFile);
            }
        }

        /// <summary>
        /// Decompile the MsiDigitalCertificate table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileMsiDigitalCertificateTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.DigitalCertificate digitalCertificate = new Wix.DigitalCertificate();

                digitalCertificate.Id = (string)row[0];

                digitalCertificate.SourceFile = (string)row[1];

                this.core.IndexElement(row, digitalCertificate);
            }
        }

        /// <summary>
        /// Decompile the MsiDigitalSignature table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileMsiDigitalSignatureTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.DigitalSignature digitalSignature = new Wix.DigitalSignature();

                if (null != row[3])
                {
                    digitalSignature.SourceFile = (string)row[3];
                }

                Wix.DigitalCertificate digitalCertificate = (Wix.DigitalCertificate)this.core.GetIndexedElement("MsiDigitalCertificate", (string)row[2]);
                if (null != digitalCertificate)
                {
                    digitalSignature.AddChild(digitalCertificate);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "DigitalCertificate_", (string)row[2], "MsiDigitalCertificate"));
                }

                Wix.IParentElement parentElement = (Wix.IParentElement)this.core.GetIndexedElement((string)row[0], (string)row[1]);
                if (null != parentElement)
                {
                    parentElement.AddChild(digitalSignature);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "SignObject", (string)row[1], (string)row[0]));
                }
            }
        }

        /// <summary>
        /// Decompile the MsiPatchCertificate table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileMsiPatchCertificateTable(Table table)
        {
            Wix.PatchCertificates patchCertificates = null;

            if (0 < table.Rows.Count)
            {
                patchCertificates = new Wix.PatchCertificates();

                this.core.RootElement.AddChild(patchCertificates);
            }

            foreach (Row row in table.Rows)
            {
                Wix.DigitalCertificate digitalCertificate = (Wix.DigitalCertificate)this.core.GetIndexedElement("MsiDigitalCertificate", (string)row[1]);

                if (null != digitalCertificate)
                {
                    patchCertificates.AddChild(digitalCertificate);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "DigitalCertificate_", (string)row[1], "MsiDigitalCertificate"));
                }
            }
        }

        /// <summary>
        /// Decompile the ODBCAttribute table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileODBCAttributeTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Property property = new Wix.Property();

                property.Id = (string)row[1];

                if (null != row[2])
                {
                    property.Value = (string)row[2];
                }

                Wix.ODBCDriver odbcDriver = (Wix.ODBCDriver)this.core.GetIndexedElement("ODBCDriver", (string)row[0]);
                if (null != odbcDriver)
                {
                    odbcDriver.AddChild(property);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Driver_", (string)row[0], "ODBCDriver"));
                }
            }
        }

        /// <summary>
        /// Decompile the ODBCDataSource table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileODBCDataSourceTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ODBCDataSource odbcDataSource = new Wix.ODBCDataSource();

                odbcDataSource.Id = (string)row[0];

                odbcDataSource.Name = (string)row[2];

                odbcDataSource.DriverName = (string)row[3];

                switch ((int)row[4])
                {
                    case MsiInterop.MsidbODBCDataSourceRegistrationPerMachine:
                        odbcDataSource.Registration = Wix.ODBCDataSource.RegistrationType.machine;
                        break;
                    case MsiInterop.MsidbODBCDataSourceRegistrationPerUser:
                        odbcDataSource.Registration = Wix.ODBCDataSource.RegistrationType.user;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[4].Column.Name, row[4]));
                        break;
                }

                this.core.IndexElement(row, odbcDataSource);
            }
        }

        /// <summary>
        /// Decompile the ODBCDriver table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileODBCDriverTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ODBCDriver odbcDriver = new Wix.ODBCDriver();

                odbcDriver.Id = (string)row[0];

                odbcDriver.Name = (string)row[2];

                odbcDriver.File = (string)row[3];

                if (null != row[4])
                {
                    odbcDriver.SetupFile = (string)row[4];
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(odbcDriver);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
                this.core.IndexElement(row, odbcDriver);
            }
        }

        /// <summary>
        /// Decompile the ODBCSourceAttribute table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileODBCSourceAttributeTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Property property = new Wix.Property();

                property.Id = (string)row[1];

                if (null != row[2])
                {
                    property.Value = (string)row[2];
                }

                Wix.ODBCDataSource odbcDataSource = (Wix.ODBCDataSource)this.core.GetIndexedElement("ODBCDataSource", (string)row[0]);
                if (null != odbcDataSource)
                {
                    odbcDataSource.AddChild(property);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "DataSource_", (string)row[0], "ODBCDataSource"));
                }
            }
        }

        /// <summary>
        /// Decompile the ODBCTranslator table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileODBCTranslatorTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ODBCTranslator odbcTranslator = new Wix.ODBCTranslator();

                odbcTranslator.Id = (string)row[0];

                odbcTranslator.Name = (string)row[2];

                odbcTranslator.File = (string)row[3];

                if (null != row[4])
                {
                    odbcTranslator.SetupFile = (string)row[4];
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(odbcTranslator);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the PatchMetadata table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompilePatchMetadataTable(Table table)
        {
            if (0 < table.Rows.Count)
            {
                Wix.PatchMetadata patchMetadata = new Wix.PatchMetadata();

                foreach (Row row in table.Rows)
                {
                    string value = (string)row[2];

                    switch ((string)row[1])
                    {
                        case "AllowRemoval":
                            if ("1" == value)
                            {
                                patchMetadata.AllowRemoval = Wix.YesNoType.yes;
                            }
                            break;
                        case "Classification":
                            switch (value)
                            {
                                case "Critical Update":
                                    patchMetadata.Classification = Wix.PatchMetadata.ClassificationType.CriticalUpdate;
                                    break;
                                case "Hotfix":
                                    patchMetadata.Classification = Wix.PatchMetadata.ClassificationType.Hotfix;
                                    break;
                                case "Security Rollup":
                                    patchMetadata.Classification = Wix.PatchMetadata.ClassificationType.SecurityRollup;
                                    break;
                                case "Service Pack":
                                    patchMetadata.Classification = Wix.PatchMetadata.ClassificationType.ServicePack;
                                    break;
                                case "Update":
                                    patchMetadata.Classification = Wix.PatchMetadata.ClassificationType.Update;
                                    break;
                                case "Update Rollup":
                                    patchMetadata.Classification = Wix.PatchMetadata.ClassificationType.UpdateRollup;
                                    break;
                                default:
                                    this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[2].Column.Name, row[2]));
                                    break;
                            }
                            break;
                        case "CreationTimeUTC":
                            if (null != value)
                            {
                                patchMetadata.CreationTimeUTC = value;
                            }
                            break;
                        case "Description":
                            if (null != value)
                            {
                                patchMetadata.Description = value;
                            }
                            break;
                        case "DisplayName":
                            if (null != value)
                            {
                                patchMetadata.DisplayName = value;
                            }
                            break;
                        case "ManufacturerName":
                            if (null != value)
                            {
                                patchMetadata.ManufacturerName = value;
                            }
                            break;
                        case "MinorUpdateTargetRTM":
                            if (null != value)
                            {
                                patchMetadata.MinorUpdateTargetRTM = value;
                            }
                            break;
                        case "MoreInfoURL":
                            if (null != value)
                            {
                                patchMetadata.MoreInfoURL = value;
                            }
                            break;
                        case "OptimizedInstallMode":
                            if ("1" == value)
                            {
                                patchMetadata.OptimizedInstallMode = Wix.YesNoType.yes;
                            }
                            break;
                        case "TargetProductName":
                            if (null != value)
                            {
                                patchMetadata.TargetProductName = value;
                            }
                            break;
                        default:
                            Wix.CustomProperty customProperty = new Wix.CustomProperty();

                            if (null != row[0])
                            {
                                customProperty.Company = (string)row[0];
                            }

                            customProperty.Property = (string)row[1];

                            if (null != row[2])
                            {
                                customProperty.Value = (string)row[2];
                            }

                            patchMetadata.AddChild(customProperty);
                            break;
                    }
                }

                this.core.RootElement.AddChild(patchMetadata);
            }
        }

        /// <summary>
        /// Decompile the PatchSequence table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompilePatchSequenceTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.PatchSequence patchSequence = new Wix.PatchSequence();

                patchSequence.PatchFamily = (string)row[0];

                if (null != row[1])
                {
                    try
                    {
                        Guid guid = new Guid((string)row[1]);

                        patchSequence.ProductCode = (string)row[1];
                    }
                    catch // non-guid value
                    {
                        patchSequence.TargetImage = (string)row[1];
                    }
                }

                if (null != row[2])
                {
                    patchSequence.Sequence = (string)row[2];
                }

                if (null != row[3] && 0x1 == (int)row[3])
                {
                    patchSequence.Supersede = Wix.YesNoType.yes;
                }

                this.core.RootElement.AddChild(patchSequence);
            }
        }

        /// <summary>
        /// Decompile the ProgId table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileProgIdTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ProgId progId = new Wix.ProgId();

                progId.Advertise = Wix.YesNoType.yes;

                progId.Id = (string)row[0];

                if (null != row[3])
                {
                    progId.Description = (string)row[3];
                }

                if (null != row[4])
                {
                    progId.Icon = (string)row[4];
                }

                if (null != row[5])
                {
                    progId.IconIndex = (int)row[5];
                }

                this.core.IndexElement(row, progId);
            }

            // nest the ProgIds
            foreach (Row row in table.Rows)
            {
                Wix.ProgId progId = (Wix.ProgId)this.core.GetIndexedElement(row);

                if (null != row[1])
                {
                    Wix.ProgId parentProgId = (Wix.ProgId)this.core.GetIndexedElement("ProgId", (string)row[1]);

                    if (null != parentProgId)
                    {
                        parentProgId.AddChild(progId);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "ProgId_Parent", (string)row[1], "ProgId"));
                    }
                }
                else if (null != row[2])
                {
                    // nesting is handled in FinalizeProgIdTable
                }
                else
                {
                    // TODO: warn for orphaned ProgId
                }
            }
        }

        /// <summary>
        /// Decompile the Properties table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompilePropertiesTable(Table table)
        {
            Wix.PatchCreation patchCreation = (Wix.PatchCreation)this.core.RootElement;

            foreach (Row row in table.Rows)
            {
                string name = (string)row[0];
                string value = (string)row[1];

                switch (name)
                {
                    case "AllowProductCodeMismatches":
                        if ("1" == value)
                        {
                            patchCreation.AllowProductCodeMismatches = Wix.YesNoType.yes;
                        }
                        break;
                    case "AllowProductVersionMajorMismatches":
                        if ("1" == value)
                        {
                            patchCreation.AllowMajorVersionMismatches = Wix.YesNoType.yes;
                        }
                        break;
                    case "ApiPatchingSymbolFlags":
                        if (null != value)
                        {
                            try
                            {
                                // remove the leading "0x" if its present
                                if (value.StartsWith("0x"))
                                {
                                    value = value.Substring(2);
                                }

                                patchCreation.SymbolFlags = Convert.ToInt32(value, 16);
                            }
                            catch
                            {
                                this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                            }
                        }
                        break;
                    case "DontRemoveTempFolderWhenFinished":
                        if ("1" == value)
                        {
                            patchCreation.CleanWorkingFolder = Wix.YesNoType.no;
                        }
                        break;
                    case "IncludeWholeFilesOnly":
                        if ("1" == value)
                        {
                            patchCreation.WholeFilesOnly = Wix.YesNoType.yes;
                        }
                        break;
                    case "ListOfPatchGUIDsToReplace":
                        if (null != value)
                        {
                            Regex guidRegex = new Regex(@"\{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\}");
                            MatchCollection guidMatches = guidRegex.Matches(value);

                            foreach (Match guidMatch in guidMatches)
                            {
                                Wix.ReplacePatch replacePatch = new Wix.ReplacePatch();

                                replacePatch.Id = guidMatch.Value;

                                this.core.RootElement.AddChild(replacePatch);
                            }
                        }
                        break;
                    case "ListOfTargetProductCodes":
                        if (null != value)
                        {
                            string[] targetProductCodes = value.Split(';');

                            foreach (string targetProductCodeString in targetProductCodes)
                            {
                                Wix.TargetProductCode targetProductCode = new Wix.TargetProductCode();

                                targetProductCode.Id = targetProductCodeString;

                                this.core.RootElement.AddChild(targetProductCode);
                            }
                        }
                        break;
                    case "PatchGUID":
                        patchCreation.Id = value;
                        break;
                    case "PatchSourceList":
                        patchCreation.SourceList = value;
                        break;
                    case "PatchOutputPath":
                        patchCreation.OutputPath = value;
                        break;
                    default:
                        Wix.PatchProperty patchProperty = new Wix.PatchProperty();

                        patchProperty.Name = name;

                        patchProperty.Value = value;

                        this.core.RootElement.AddChild(patchProperty);
                        break;
                }
            }
        }

        /// <summary>
        /// Decompile the Property table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompilePropertyTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                string id = (string)row[0];
                string value = (string)row[1];

                if ("AdminProperties" == id || "MsiHiddenProperties" == id || "SecureCustomProperties" == id)
                {
                    if (0 < value.Length)
                    {
                        foreach (string propertyId in value.Split(';'))
                        {
                            Wix.Property specialProperty = this.EnsureProperty(propertyId);

                            switch (id)
                            {
                                case "AdminProperties":
                                    specialProperty.Admin = Wix.YesNoType.yes;
                                    break;
                                case "MsiHiddenProperties":
                                    specialProperty.Hidden = Wix.YesNoType.yes;
                                    break;
                                case "SecureCustomProperties":
                                    specialProperty.Secure = Wix.YesNoType.yes;
                                    break;
                            }
                        }
                    }

                    continue;
                }
                else if (OutputType.Product == this.outputType)
                {
                    Wix.Product product = (Wix.Product)this.core.RootElement;

                    switch (id)
                    {
                        case "Manufacturer":
                            product.Manufacturer = value;
                            continue;
                        case "ProductCode":
                            product.Id = value.ToUpper(CultureInfo.InvariantCulture);
                            continue;
                        case "ProductLanguage":
                            product.Language = value;
                            continue;
                        case "ProductName":
                            product.Name = value;
                            continue;
                        case "ProductVersion":
                            product.Version = value;
                            continue;
                        case "UpgradeCode":
                            product.UpgradeCode = value;
                            continue;
                    }
                }

                Wix.Property property = this.EnsureProperty(id);

                property.Value = value;
            }
        }

        /// <summary>
        /// Decompile the PublishComponent table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompilePublishComponentTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Category category = new Wix.Category();

                category.Id = (string)row[0];

                category.Qualifier = (string)row[1];

                if (null != row[3])
                {
                    category.AppData = (string)row[3];
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[2]);
                if (null != component)
                {
                    component.AddChild(category);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[2], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the RadioButton table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileRadioButtonTable(Table table)
        {
            SortedList radioButtons = new SortedList();
            Hashtable radioButtonGroups = new Hashtable();

            foreach (Row row in table.Rows)
            {
                Wix.RadioButton radioButton = new Wix.RadioButton();

                radioButton.Value = (string)row[2];

                radioButton.X = (string)row[3];

                radioButton.Y = (string)row[4];

                radioButton.Width = (string)row[5];

                radioButton.Height = (string)row[6];

                if (null != row[7])
                {
                    radioButton.Text = (string)row[7];
                }

                if (null != row[8])
                {
                    string[] help = ((string)row[8]).Split('|');

                    if (2 == help.Length)
                    {
                        if (0 < help[0].Length)
                        {
                            radioButton.ToolTip = help[0];
                        }

                        if (0 < help[1].Length)
                        {
                            radioButton.Help = help[1];
                        }
                    }
                }

                radioButtons.Add(String.Format(CultureInfo.InvariantCulture, "{0}|{1:0000000000}", row[0], row[1]), row);
                this.core.IndexElement(row, radioButton);
            }

            // nest the radio buttons
            foreach (Row row in radioButtons.Values)
            {
                Wix.RadioButton radioButton = (Wix.RadioButton)this.core.GetIndexedElement(row);
                Wix.RadioButtonGroup radioButtonGroup = (Wix.RadioButtonGroup)radioButtonGroups[(string)row[0]];

                if (null == radioButtonGroup)
                {
                    radioButtonGroup = new Wix.RadioButtonGroup();

                    radioButtonGroup.Property = (string)row[0];

                    this.core.UIElement.AddChild(radioButtonGroup);
                    radioButtonGroups.Add((string)row[0], radioButtonGroup);
                }

                radioButtonGroup.AddChild(radioButton);
            }
        }

        /// <summary>
        /// Decompile the Registry table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileRegistryTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                if ("-" == (string)row[3] && null == row[4])
                {
                    Wix.RemoveRegistryKey removeRegistryKey = new Wix.RemoveRegistryKey();

                    removeRegistryKey.Id = (string)row[0];

                    Wix.RegistryRootType registryRootType;
                    if (this.GetRegistryRootType(row.SourceLineNumbers, table.Name, row.Fields[1], out registryRootType))
                    {
                        removeRegistryKey.Root = registryRootType;
                    }

                    removeRegistryKey.Key = (string)row[2];

                    removeRegistryKey.Action = Wix.RemoveRegistryKey.ActionType.removeOnUninstall;

                    this.core.IndexElement(row, removeRegistryKey);
                }
                else if (("+" == (string)row[3] || "*" == (string)row[3]) && null == row[4])
                {
                    Wix.RegistryKey registryKey = new Wix.RegistryKey();

                    registryKey.Id = (string)row[0];

                    Wix.RegistryRootType registryRootType;
                    if (this.GetRegistryRootType(row.SourceLineNumbers, table.Name, row.Fields[1], out registryRootType))
                    {
                        registryKey.Root = registryRootType;
                    }

                    registryKey.Key = (string)row[2];

                    if ("+" == (string)row[3])
                    {
                        registryKey.Action = Wix.RegistryKey.ActionType.create;
                    }
                    else
                    {
                        registryKey.Action = Wix.RegistryKey.ActionType.createAndRemoveOnUninstall;
                    }

                    this.core.IndexElement(row, registryKey);
                }
                else
                {
                    Wix.RegistryValue registryValue = new Wix.RegistryValue();

                    registryValue.Id = (string)row[0];

                    Wix.RegistryRootType registryRootType;
                    if (this.GetRegistryRootType(row.SourceLineNumbers, table.Name, row.Fields[1], out registryRootType))
                    {
                        registryValue.Root = registryRootType;
                    }

                    registryValue.Key = (string)row[2];

                    if (null != row[3])
                    {
                        registryValue.Name = (string)row[3];
                    }

                    if (null != row[4])
                    {
                        string value = (string)row[4];

                        if (value.StartsWith("#x"))
                        {
                            registryValue.Type = Wix.RegistryValue.TypeType.binary;
                            registryValue.Value = value.Substring(2);
                        }
                        else if (value.StartsWith("#%"))
                        {
                            registryValue.Type = Wix.RegistryValue.TypeType.expandable;
                            registryValue.Value = value.Substring(2);
                        }
                        else if (value.StartsWith("#") && !value.StartsWith("##"))
                        {
                            registryValue.Type = Wix.RegistryValue.TypeType.integer;
                            registryValue.Value = value.Substring(1);
                        }
                        else
                        {
                            if (value.StartsWith("##"))
                            {
                                value = value.Substring(1);
                            }

                            if (0 <= value.IndexOf("[~]"))
                            {
                                registryValue.Type = Wix.RegistryValue.TypeType.multiString;

                                if ("[~]" == value)
                                {
                                    value = string.Empty;
                                }
                                else if (value.StartsWith("[~]") && value.EndsWith("[~]"))
                                {
                                    value = value.Substring(3, value.Length - 6);
                                }
                                else if (value.StartsWith("[~]"))
                                {
                                    registryValue.Action = Wix.RegistryValue.ActionType.append;
                                    value = value.Substring(3);
                                }
                                else if (value.EndsWith("[~]"))
                                {
                                    registryValue.Action = Wix.RegistryValue.ActionType.prepend;
                                    value = value.Substring(0, value.Length - 3);
                                }

                                string[] multiValues = NullSplitter.Split(value);
                                foreach (string multiValue in multiValues)
                                {
                                    Wix.MultiStringValue multiStringValue = new Wix.MultiStringValue();

                                    multiStringValue.Content = multiValue;

                                    registryValue.AddChild(multiStringValue);
                                }
                            }
                            else
                            {
                                registryValue.Type = Wix.RegistryValue.TypeType.@string;
                                registryValue.Value = value;
                            }
                        }
                    }
                    else
                    {
                        registryValue.Type = Wix.RegistryValue.TypeType.@string;
                        registryValue.Value = String.Empty;
                    }

                    this.core.IndexElement(row, registryValue);
                }
            }
        }

        /// <summary>
        /// Decompile the RegLocator table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileRegLocatorTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.RegistrySearch registrySearch = new Wix.RegistrySearch();

                registrySearch.Id = (string)row[0];

                switch ((int)row[1])
                {
                    case MsiInterop.MsidbRegistryRootClassesRoot:
                        registrySearch.Root = Wix.RegistrySearch.RootType.HKCR;
                        break;
                    case MsiInterop.MsidbRegistryRootCurrentUser:
                        registrySearch.Root = Wix.RegistrySearch.RootType.HKCU;
                        break;
                    case MsiInterop.MsidbRegistryRootLocalMachine:
                        registrySearch.Root = Wix.RegistrySearch.RootType.HKLM;
                        break;
                    case MsiInterop.MsidbRegistryRootUsers:
                        registrySearch.Root = Wix.RegistrySearch.RootType.HKU;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[1].Column.Name, row[1]));
                        break;
                }

                registrySearch.Key = (string)row[2];

                if (null != row[3])
                {
                    registrySearch.Name = (string)row[3];
                }

                if (null == row[4])
                {
                    registrySearch.Type = Wix.RegistrySearch.TypeType.file;
                }
                else
                {
                    int type = (int)row[4];

                    if (MsiInterop.MsidbLocatorType64bit == (type & MsiInterop.MsidbLocatorType64bit))
                    {
                        registrySearch.Win64 = Wix.YesNoType.yes;
                        type &= ~MsiInterop.MsidbLocatorType64bit;
                    }

                    switch (type)
                    {
                        case MsiInterop.MsidbLocatorTypeDirectory:
                            registrySearch.Type = Wix.RegistrySearch.TypeType.directory;
                            break;
                        case MsiInterop.MsidbLocatorTypeFileName:
                            registrySearch.Type = Wix.RegistrySearch.TypeType.file;
                            break;
                        case MsiInterop.MsidbLocatorTypeRawValue:
                            registrySearch.Type = Wix.RegistrySearch.TypeType.raw;
                            break;
                        default:
                            this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[4].Column.Name, row[4]));
                            break;
                    }
                }

                this.core.IndexElement(row, registrySearch);
            }
        }

        /// <summary>
        /// Decompile the RemoveFile table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileRemoveFileTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                if (null == row[2])
                {
                    Wix.RemoveFolder removeFolder = new Wix.RemoveFolder();

                    removeFolder.Id = (string)row[0];

                    // directory/property is set in FinalizeDecompile

                    switch ((int)row[4])
                    {
                        case MsiInterop.MsidbRemoveFileInstallModeOnInstall:
                            removeFolder.On = Wix.RemoveFolder.OnType.install;
                            break;
                        case MsiInterop.MsidbRemoveFileInstallModeOnRemove:
                            removeFolder.On = Wix.RemoveFolder.OnType.uninstall;
                            break;
                        case MsiInterop.MsidbRemoveFileInstallModeOnBoth:
                            removeFolder.On = Wix.RemoveFolder.OnType.both;
                            break;
                        default:
                            this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[4].Column.Name, row[4]));
                            break;
                    }

                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                    if (null != component)
                    {
                        component.AddChild(removeFolder);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                    }
                    this.core.IndexElement(row, removeFolder);
                }
                else
                {
                    Wix.RemoveFile removeFile = new Wix.RemoveFile();

                    removeFile.Id = (string)row[0];

                    string[] names = Installer.GetNames((string)row[2]);
                    if (null != names[0] && null != names[1])
                    {
                        removeFile.ShortName = names[0];
                        removeFile.Name = names[1];
                    }
                    else if (null != names[0])
                    {
                        removeFile.Name = names[0];
                    }

                    // directory/property is set in FinalizeDecompile

                    switch ((int)row[4])
                    {
                        case MsiInterop.MsidbRemoveFileInstallModeOnInstall:
                            removeFile.On = Wix.RemoveFile.OnType.install;
                            break;
                        case MsiInterop.MsidbRemoveFileInstallModeOnRemove:
                            removeFile.On = Wix.RemoveFile.OnType.uninstall;
                            break;
                        case MsiInterop.MsidbRemoveFileInstallModeOnBoth:
                            removeFile.On = Wix.RemoveFile.OnType.both;
                            break;
                        default:
                            this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[4].Column.Name, row[4]));
                            break;
                    }

                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                    if (null != component)
                    {
                        component.AddChild(removeFile);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                    }
                    this.core.IndexElement(row, removeFile);
                }
            }
        }

        /// <summary>
        /// Decompile the RemoveIniFile table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileRemoveIniFileTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.IniFile iniFile = new Wix.IniFile();

                iniFile.Id = (string)row[0];

                string[] names = Installer.GetNames((string)row[1]);
                if (null != names[0] && null != names[1])
                {
                    iniFile.ShortName = names[0];
                    iniFile.Name = names[1];
                }
                else if (null != names[0])
                {
                    iniFile.Name = names[0];
                }

                if (null != row[2])
                {
                    iniFile.Directory = (string)row[2];
                }

                iniFile.Section = (string)row[3];

                iniFile.Key = (string)row[4];

                if (null != row[5])
                {
                    iniFile.Value = (string)row[5];
                }

                switch ((int)row[6])
                {
                    case MsiInterop.MsidbIniFileActionRemoveLine:
                        iniFile.Action = Wix.IniFile.ActionType.removeLine;
                        break;
                    case MsiInterop.MsidbIniFileActionRemoveTag:
                        iniFile.Action = Wix.IniFile.ActionType.removeTag;
                        break;
                    default:
                        this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[6].Column.Name, row[6]));
                        break;
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[7]);
                if (null != component)
                {
                    component.AddChild(iniFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[7], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the RemoveRegistry table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileRemoveRegistryTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                if ("-" == (string)row[3])
                {
                    Wix.RemoveRegistryKey removeRegistryKey = new Wix.RemoveRegistryKey();

                    removeRegistryKey.Id = (string)row[0];

                    Wix.RegistryRootType registryRootType;
                    if (this.GetRegistryRootType(row.SourceLineNumbers, table.Name, row.Fields[1], out registryRootType))
                    {
                        removeRegistryKey.Root = registryRootType;
                    }

                    removeRegistryKey.Key = (string)row[2];

                    removeRegistryKey.Action = Wix.RemoveRegistryKey.ActionType.removeOnInstall;

                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[4]);
                    if (null != component)
                    {
                        component.AddChild(removeRegistryKey);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[4], "Component"));
                    }
                }
                else
                {
                    Wix.RemoveRegistryValue removeRegistryValue = new Wix.RemoveRegistryValue();

                    removeRegistryValue.Id = (string)row[0];

                    Wix.RegistryRootType registryRootType;
                    if (this.GetRegistryRootType(row.SourceLineNumbers, table.Name, row.Fields[1], out registryRootType))
                    {
                        removeRegistryValue.Root = registryRootType;
                    }

                    removeRegistryValue.Key = (string)row[2];

                    if (null != row[3])
                    {
                        removeRegistryValue.Name = (string)row[3];
                    }

                    Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[4]);
                    if (null != component)
                    {
                        component.AddChild(removeRegistryValue);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[4], "Component"));
                    }
                }
            }
        }

        /// <summary>
        /// Decompile the ReserveCost table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileReserveCostTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ReserveCost reserveCost = new Wix.ReserveCost();

                reserveCost.Id = (string)row[0];

                if (null != row[2])
                {
                    reserveCost.Directory = (string)row[2];
                }

                reserveCost.RunLocal = (int)row[3];

                reserveCost.RunFromSource = (int)row[4];

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[1]);
                if (null != component)
                {
                    component.AddChild(reserveCost);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[1], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the SelfReg table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileSelfRegTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.File file = (Wix.File)this.core.GetIndexedElement("File", (string)row[0]);

                if (null != file)
                {
                    if (null != row[1])
                    {
                        file.SelfRegCost = (int)row[1];
                    }
                    else
                    {
                        file.SelfRegCost = 0;
                    }
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "File_", (string)row[0], "File"));
                }
            }
        }

        /// <summary>
        /// Decompile the ServiceControl table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileServiceControlTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ServiceControl serviceControl = new Wix.ServiceControl();

                serviceControl.Id = (string)row[0];

                serviceControl.Name = (string)row[1];

                int eventValue = (int)row[2];
                if (MsiInterop.MsidbServiceControlEventStart == (eventValue & MsiInterop.MsidbServiceControlEventStart) &&
                    MsiInterop.MsidbServiceControlEventUninstallStart == (eventValue & MsiInterop.MsidbServiceControlEventUninstallStart))
                {
                    serviceControl.Start = Wix.ServiceControl.StartType.both;
                }
                else if (MsiInterop.MsidbServiceControlEventStart == (eventValue & MsiInterop.MsidbServiceControlEventStart))
                {
                    serviceControl.Start = Wix.ServiceControl.StartType.install;
                }
                else if (MsiInterop.MsidbServiceControlEventUninstallStart == (eventValue & MsiInterop.MsidbServiceControlEventUninstallStart))
                {
                    serviceControl.Start = Wix.ServiceControl.StartType.uninstall;
                }

                if (MsiInterop.MsidbServiceControlEventStop == (eventValue & MsiInterop.MsidbServiceControlEventStop) &&
                    MsiInterop.MsidbServiceControlEventUninstallStop == (eventValue & MsiInterop.MsidbServiceControlEventUninstallStop))
                {
                    serviceControl.Stop = Wix.ServiceControl.StopType.both;
                }
                else if (MsiInterop.MsidbServiceControlEventStop == (eventValue & MsiInterop.MsidbServiceControlEventStop))
                {
                    serviceControl.Stop = Wix.ServiceControl.StopType.install;
                }
                else if (MsiInterop.MsidbServiceControlEventUninstallStop == (eventValue & MsiInterop.MsidbServiceControlEventUninstallStop))
                {
                    serviceControl.Stop = Wix.ServiceControl.StopType.uninstall;
                }

                if (MsiInterop.MsidbServiceControlEventDelete == (eventValue & MsiInterop.MsidbServiceControlEventDelete) &&
                    MsiInterop.MsidbServiceControlEventUninstallDelete == (eventValue & MsiInterop.MsidbServiceControlEventUninstallDelete))
                {
                    serviceControl.Remove = Wix.ServiceControl.RemoveType.both;
                }
                else if (MsiInterop.MsidbServiceControlEventDelete == (eventValue & MsiInterop.MsidbServiceControlEventDelete))
                {
                    serviceControl.Remove = Wix.ServiceControl.RemoveType.install;
                }
                else if (MsiInterop.MsidbServiceControlEventUninstallDelete == (eventValue & MsiInterop.MsidbServiceControlEventUninstallDelete))
                {
                    serviceControl.Remove = Wix.ServiceControl.RemoveType.uninstall;
                }

                if (null != row[3])
                {
                    string[] arguments = NullSplitter.Split((string)row[3]);

                    foreach (string argument in arguments)
                    {
                        Wix.ServiceArgument serviceArgument = new Wix.ServiceArgument();

                        serviceArgument.Content = argument;

                        serviceControl.AddChild(serviceArgument);
                    }
                }

                if (null != row[4] && 1 == (int)row[4])
                {
                    serviceControl.Wait = Wix.YesNoType.yes;
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[5]);
                if (null != component)
                {
                    component.AddChild(serviceControl);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[5], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the ServiceInstall table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileServiceInstallTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.ServiceInstall serviceInstall = new Wix.ServiceInstall();

                serviceInstall.Id = (string)row[0];

                serviceInstall.Name = (string)row[1];

                if (null != row[2])
                {
                    serviceInstall.DisplayName = (string)row[2];
                }

                int serviceType = (int)row[3];
                if (MsiInterop.MsidbServiceInstallInteractive == (serviceType & MsiInterop.MsidbServiceInstallInteractive))
                {
                    serviceInstall.Interactive = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbServiceInstallOwnProcess == (serviceType & MsiInterop.MsidbServiceInstallOwnProcess) &&
                    MsiInterop.MsidbServiceInstallShareProcess == (serviceType & MsiInterop.MsidbServiceInstallShareProcess))
                {
                    // TODO: warn
                }
                else if (MsiInterop.MsidbServiceInstallOwnProcess == (serviceType & MsiInterop.MsidbServiceInstallOwnProcess))
                {
                    serviceInstall.Type = Wix.ServiceInstall.TypeType.ownProcess;
                }
                else if (MsiInterop.MsidbServiceInstallShareProcess == (serviceType & MsiInterop.MsidbServiceInstallShareProcess))
                {
                    serviceInstall.Type = Wix.ServiceInstall.TypeType.shareProcess;
                }

                int startType = (int)row[4];
                if (MsiInterop.MsidbServiceInstallDisabled == startType)
                {
                    serviceInstall.Start = Wix.ServiceInstall.StartType.disabled;
                }
                else if (MsiInterop.MsidbServiceInstallDemandStart == startType)
                {
                    serviceInstall.Start = Wix.ServiceInstall.StartType.demand;
                }
                else if (MsiInterop.MsidbServiceInstallAutoStart == startType)
                {
                    serviceInstall.Start = Wix.ServiceInstall.StartType.auto;
                }
                else
                {
                    this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[4].Column.Name, row[4]));
                }

                int errorControl = (int)row[5];
                if (MsiInterop.MsidbServiceInstallErrorCritical == (errorControl & MsiInterop.MsidbServiceInstallErrorCritical))
                {
                    serviceInstall.ErrorControl = Wix.ServiceInstall.ErrorControlType.critical;
                }
                else if (MsiInterop.MsidbServiceInstallErrorNormal == (errorControl & MsiInterop.MsidbServiceInstallErrorNormal))
                {
                    serviceInstall.ErrorControl = Wix.ServiceInstall.ErrorControlType.normal;
                }
                else
                {
                    serviceInstall.ErrorControl = Wix.ServiceInstall.ErrorControlType.ignore;
                }

                if (MsiInterop.MsidbServiceInstallErrorControlVital == (errorControl & MsiInterop.MsidbServiceInstallErrorControlVital))
                {
                    serviceInstall.Vital = Wix.YesNoType.yes;
                }

                if (null != row[6])
                {
                    serviceInstall.LoadOrderGroup = (string)row[6];
                }

                if (null != row[7])
                {
                    string[] dependencies = NullSplitter.Split((string)row[7]);

                    foreach (string dependency in dependencies)
                    {
                        if (0 < dependency.Length)
                        {
                            Wix.ServiceDependency serviceDependency = new Wix.ServiceDependency();

                            if (dependency.StartsWith("+"))
                            {
                                serviceDependency.Group = Wix.YesNoType.yes;
                                serviceDependency.Id = dependency.Substring(1);
                            }
                            else
                            {
                                serviceDependency.Id = dependency;
                            }

                            serviceInstall.AddChild(serviceDependency);
                        }
                    }
                }

                if (null != row[8])
                {
                    serviceInstall.Account = (string)row[8];
                }

                if (null != row[9])
                {
                    serviceInstall.Password = (string)row[9];
                }

                if (null != row[10])
                {
                    serviceInstall.Arguments = (string)row[10];
                }

                if (null != row[12])
                {
                    serviceInstall.Description = (string)row[12];
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[11]);
                if (null != component)
                {
                    component.AddChild(serviceInstall);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[11], "Component"));
                }
                this.core.IndexElement(row, serviceInstall);
            }
        }

        /// <summary>
        /// Decompile the SFPCatalog table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileSFPCatalogTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.SFPCatalog sfpCatalog = new Wix.SFPCatalog();

                sfpCatalog.Name = (string)row[0];

                sfpCatalog.SourceFile = (string)row[1];

                this.core.IndexElement(row, sfpCatalog);
            }

            // nest the SFPCatalog elements
            foreach (Row row in table.Rows)
            {
                Wix.SFPCatalog sfpCatalog = (Wix.SFPCatalog)this.core.GetIndexedElement(row);

                if (null != row[2])
                {
                    Wix.SFPCatalog parentSFPCatalog = (Wix.SFPCatalog)this.core.GetIndexedElement("SFPCatalog", (string)row[2]);

                    if (null != parentSFPCatalog)
                    {
                        parentSFPCatalog.AddChild(sfpCatalog);
                    }
                    else
                    {
                        sfpCatalog.Dependency = (string)row[2];

                        this.core.RootElement.AddChild(sfpCatalog);
                    }
                }
                else
                {
                    this.core.RootElement.AddChild(sfpCatalog);
                }
            }
        }

        /// <summary>
        /// Decompile the Shortcut table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileShortcutTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Shortcut shortcut = new Wix.Shortcut();

                shortcut.Id = (string)row[0];

                shortcut.Directory = (string)row[1];

                string[] names = Installer.GetNames((string)row[2]);
                if (null != names[0] && null != names[1])
                {
                    shortcut.ShortName = names[0];
                    shortcut.Name = names[1];
                }
                else if (null != names[0])
                {
                    shortcut.Name = names[0];
                }

                string target = (string)row[4];
                if (target.StartsWith("[") && target.EndsWith("]"))
                {
                    // TODO: use this value to do a "more-correct" nesting under the indicated File or CreateDirectory element
                    shortcut.Target = target;
                }
                else
                {
                    shortcut.Advertise = Wix.YesNoType.yes;

                    // primary feature is set in FinalizeFeatureComponentsTable
                }

                if (null != row[5])
                {
                    shortcut.Arguments = (string)row[5];
                }

                if (null != row[6])
                {
                    shortcut.Description = (string)row[6];
                }

                if (null != row[7])
                {
                    shortcut.Hotkey = (int)row[7];
                }

                if (null != row[8])
                {
                    shortcut.Icon = (string)row[8];
                }

                if (null != row[9])
                {
                    shortcut.IconIndex = (int)row[9];
                }

                if (null != row[10])
                {
                    switch ((int)row[10])
                    {
                        case 1:
                            shortcut.Show = Wix.Shortcut.ShowType.normal;
                            break;
                        case 3:
                            shortcut.Show = Wix.Shortcut.ShowType.maximized;
                            break;
                        case 7:
                            shortcut.Show = Wix.Shortcut.ShowType.minimized;
                            break;
                        default:
                            this.core.OnMessage(WixWarnings.IllegalColumnValue(row.SourceLineNumbers, table.Name, row.Fields[10].Column.Name, row[10]));
                            break;
                    }
                }

                if (null != row[11])
                {
                    shortcut.WorkingDirectory = (string)row[11];
                }

                if (null != row[12])
                {
                    shortcut.DisplayResourceDll = (string)row[12];
                }

                if (null != row[13])
                {
                    shortcut.DisplayResourceId = (int)row[13];
                }

                if (null != row[14])
                {
                    shortcut.DescriptionResourceDll = (string)row[14];
                }

                if (null != row[15])
                {
                    shortcut.DescriptionResourceId = (int)row[15];
                }

                Wix.Component component = (Wix.Component)this.core.GetIndexedElement("Component", (string)row[3]);
                if (null != component)
                {
                    component.AddChild(shortcut);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Component_", (string)row[3], "Component"));
                }
            }
        }

        /// <summary>
        /// Decompile the Signature table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileSignatureTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.FileSearch fileSearch = new Wix.FileSearch();

                fileSearch.Id = (string)row[0];

                string[] names = Installer.GetNames((string)row[1]);
                if (null != names[0])
                {
                    // it is permissable to just have a long name
                    if (!CompilerCore.IsValidShortFilename(names[0], false) && null == names[1])
                    {
                        fileSearch.Name = names[0];
                    }
                    else
                    {
                        fileSearch.ShortName = names[0];
                    }
                }

                if (null != names[1])
                {
                    fileSearch.LongName = names[1];
                }

                if (null != row[2])
                {
                    fileSearch.MinVersion = (string)row[2];
                }

                if (null != row[3])
                {
                    fileSearch.MaxVersion = (string)row[3];
                }

                if (null != row[4])
                {
                    fileSearch.MinSize = (int)row[4];
                }

                if (null != row[5])
                {
                    fileSearch.MaxSize = (int)row[5];
                }

                if (null != row[6])
                {
                    fileSearch.MinDate = DecompilerCore.ConvertIntegerToDateTime((int)row[6]);
                }

                if (null != row[7])
                {
                    fileSearch.MaxDate = DecompilerCore.ConvertIntegerToDateTime((int)row[7]);
                }

                if (null != row[8])
                {
                    fileSearch.Languages = (string)row[8];
                }

                this.core.IndexElement(row, fileSearch);
            }
        }

        /// <summary>
        /// Decompile the TargetFiles_OptionalData table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileTargetFiles_OptionalDataTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.TargetFile targetFile = (Wix.TargetFile)this.patchTargetFiles[row[0]];
                if (null == targetFile)
                {
                    targetFile = new Wix.TargetFile();

                    targetFile.Id = (string)row[1];

                    Wix.TargetImage targetImage = (Wix.TargetImage)this.core.GetIndexedElement("TargetImages", (string)row[0]);
                    if (null != targetImage)
                    {
                        targetImage.AddChild(targetFile);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Target", (string)row[0], "TargetImages"));
                    }
                    this.patchTargetFiles.Add(row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), targetFile);
                }

                if (null != row[2])
                {
                    string[] symbolPaths = ((string)row[2]).Split(';');

                    foreach (string symbolPathString in symbolPaths)
                    {
                        Wix.SymbolPath symbolPath = new Wix.SymbolPath();

                        symbolPath.Path = symbolPathString;

                        targetFile.AddChild(symbolPath);
                    }
                }

                if (null != row[3] && null != row[4])
                {
                    string[] ignoreOffsets = ((string)row[3]).Split(',');
                    string[] ignoreLengths = ((string)row[4]).Split(',');

                    if (ignoreOffsets.Length == ignoreLengths.Length)
                    {
                        for (int i = 0; i < ignoreOffsets.Length; i++)
                        {
                            Wix.IgnoreRange ignoreRange = new Wix.IgnoreRange();

                            if (ignoreOffsets[i].StartsWith("0x"))
                            {
                                ignoreRange.Offset = Convert.ToInt32(ignoreOffsets[i].Substring(2), 16);
                            }
                            else
                            {
                                ignoreRange.Offset = Convert.ToInt32(ignoreOffsets[i], CultureInfo.InvariantCulture);
                            }

                            if (ignoreLengths[i].StartsWith("0x"))
                            {
                                ignoreRange.Length = Convert.ToInt32(ignoreLengths[i].Substring(2), 16);
                            }
                            else
                            {
                                ignoreRange.Length = Convert.ToInt32(ignoreLengths[i], CultureInfo.InvariantCulture);
                            }

                            targetFile.AddChild(ignoreRange);
                        }
                    }
                    else
                    {
                        // TODO: warn
                    }
                }
                else if (null != row[3] || null != row[4])
                {
                    // TODO: warn about mismatch between columns
                }

                // the RetainOffsets column is handled in FinalizeFamilyFileRangesTable
            }
        }

        /// <summary>
        /// Decompile the TargetImages table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileTargetImagesTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.TargetImage targetImage = new Wix.TargetImage();

                targetImage.Id = (string)row[0];

                targetImage.SourceFile = (string)row[1];

                if (null != row[2])
                {
                    string[] symbolPaths = ((string)row[3]).Split(';');

                    foreach (string symbolPathString in symbolPaths)
                    {
                        Wix.SymbolPath symbolPath = new Wix.SymbolPath();

                        symbolPath.Path = symbolPathString;

                        targetImage.AddChild(symbolPath);
                    }
                }

                targetImage.Order = (int)row[4];

                if (null != row[5])
                {
                    targetImage.Validation = (string)row[5];
                }

                if (0 != (int)row[6])
                {
                    targetImage.IgnoreMissingFiles = Wix.YesNoType.yes;
                }

                Wix.UpgradeImage upgradeImage = (Wix.UpgradeImage)this.core.GetIndexedElement("UpgradedImages", (string)row[3]);
                if (null != upgradeImage)
                {
                    upgradeImage.AddChild(targetImage);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Upgraded", (string)row[3], "UpgradedImages"));
                }
                this.core.IndexElement(row, targetImage);
            }
        }

        /// <summary>
        /// Decompile the TextStyle table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileTextStyleTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.TextStyle textStyle = new Wix.TextStyle();

                textStyle.Id = (string)row[0];

                textStyle.FaceName = (string)row[1];

                textStyle.Size = (int)row[2];

                if (null != row[3])
                {
                    int color = (int)row[3];

                    textStyle.Red = color & 0xFF;

                    textStyle.Green = (color & 0xFF00) >> 8;

                    textStyle.Blue = (color & 0xFF0000) >> 16;
                }

                if (null != row[4])
                {
                    int styleBits = (int)row[4];

                    if (MsiInterop.MsidbTextStyleStyleBitsBold == (styleBits & MsiInterop.MsidbTextStyleStyleBitsBold))
                    {
                        textStyle.Bold = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbTextStyleStyleBitsItalic == (styleBits & MsiInterop.MsidbTextStyleStyleBitsItalic))
                    {
                        textStyle.Italic = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbTextStyleStyleBitsUnderline == (styleBits & MsiInterop.MsidbTextStyleStyleBitsUnderline))
                    {
                        textStyle.Underline = Wix.YesNoType.yes;
                    }

                    if (MsiInterop.MsidbTextStyleStyleBitsStrike == (styleBits & MsiInterop.MsidbTextStyleStyleBitsStrike))
                    {
                        textStyle.Strike = Wix.YesNoType.yes;
                    }
                }

                this.core.UIElement.AddChild(textStyle);
            }
        }

        /// <summary>
        /// Decompile the TypeLib table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileTypeLibTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.TypeLib typeLib = new Wix.TypeLib();

                typeLib.Id = (string)row[0];

                typeLib.Advertise = Wix.YesNoType.yes;

                typeLib.Language = (int)row[1];

                if (null != row[3])
                {
                    int version = (int)row[3];

                    if (65536 == version)
                    {
                        this.core.OnMessage(WixWarnings.PossiblyIncorrectTypelibVersion(row.SourceLineNumbers, typeLib.Id));
                    }

                    typeLib.MajorVersion = ((version & 0xFFFF00) >> 8);
                    typeLib.MinorVersion = (version & 0xFF);
                }

                if (null != row[4])
                {
                    typeLib.Description = (string)row[4];
                }

                if (null != row[5])
                {
                    typeLib.HelpDirectory = (string)row[5];
                }

                if (null != row[7])
                {
                    typeLib.Cost = (int)row[7];
                }

                // nested under the appropriate File element in FinalizeFileTable
                this.core.IndexElement(row, typeLib);
            }
        }

        /// <summary>
        /// Decompile the Upgrade table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileUpgradeTable(Table table)
        {
            Hashtable upgradeElements = new Hashtable();

            foreach (UpgradeRow upgradeRow in table.Rows)
            {
                Wix.Upgrade upgrade = (Wix.Upgrade)upgradeElements[upgradeRow.UpgradeCode];

                // create the parent Upgrade element if it doesn't already exist
                if (null == upgrade)
                {
                    upgrade = new Wix.Upgrade();

                    upgrade.Id = upgradeRow.UpgradeCode;

                    this.core.RootElement.AddChild(upgrade);
                    upgradeElements.Add(upgrade.Id, upgrade);
                }

                Wix.UpgradeVersion upgradeVersion = new Wix.UpgradeVersion();

                if (null != upgradeRow.VersionMin)
                {
                    upgradeVersion.Minimum = upgradeRow.VersionMin;
                }

                if (null != upgradeRow.VersionMax)
                {
                    upgradeVersion.Maximum = upgradeRow.VersionMax;
                }

                if (null != upgradeRow.Language)
                {
                    upgradeVersion.Language = upgradeRow.Language;
                }

                if (MsiInterop.MsidbUpgradeAttributesMigrateFeatures == (upgradeRow.Attributes & MsiInterop.MsidbUpgradeAttributesMigrateFeatures))
                {
                    upgradeVersion.MigrateFeatures = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbUpgradeAttributesOnlyDetect == (upgradeRow.Attributes & MsiInterop.MsidbUpgradeAttributesOnlyDetect))
                {
                    upgradeVersion.OnlyDetect = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbUpgradeAttributesIgnoreRemoveFailure == (upgradeRow.Attributes & MsiInterop.MsidbUpgradeAttributesIgnoreRemoveFailure))
                {
                    upgradeVersion.IgnoreRemoveFailure = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbUpgradeAttributesVersionMinInclusive == (upgradeRow.Attributes & MsiInterop.MsidbUpgradeAttributesVersionMinInclusive))
                {
                    upgradeVersion.IncludeMinimum = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbUpgradeAttributesVersionMaxInclusive == (upgradeRow.Attributes & MsiInterop.MsidbUpgradeAttributesVersionMaxInclusive))
                {
                    upgradeVersion.IncludeMaximum = Wix.YesNoType.yes;
                }

                if (MsiInterop.MsidbUpgradeAttributesLanguagesExclusive == (upgradeRow.Attributes & MsiInterop.MsidbUpgradeAttributesLanguagesExclusive))
                {
                    upgradeVersion.ExcludeLanguages = Wix.YesNoType.yes;
                }

                if (null != upgradeRow.Remove)
                {
                    upgradeVersion.RemoveFeatures = upgradeRow.Remove;
                }

                upgradeVersion.Property = upgradeRow.ActionProperty;

                upgrade.AddChild(upgradeVersion);
            }
        }

        /// <summary>
        /// Decompile the UpgradedFiles_OptionalData table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileUpgradedFiles_OptionalDataTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.UpgradeFile upgradeFile = new Wix.UpgradeFile();

                upgradeFile.File = (string)row[1];

                if (null != row[2])
                {
                    string[] symbolPaths = ((string)row[2]).Split(';');

                    foreach (string symbolPathString in symbolPaths)
                    {
                        Wix.SymbolPath symbolPath = new Wix.SymbolPath();

                        symbolPath.Path = symbolPathString;

                        upgradeFile.AddChild(symbolPath);
                    }
                }

                if (null != row[3] && 1 == (int)row[3])
                {
                    upgradeFile.AllowIgnoreOnError = Wix.YesNoType.yes;
                }

                if (null != row[4] && 0 != (int)row[4])
                {
                    upgradeFile.WholeFile = Wix.YesNoType.yes;
                }

                upgradeFile.Ignore = Wix.YesNoType.no;

                Wix.UpgradeImage upgradeImage = (Wix.UpgradeImage)this.core.GetIndexedElement("UpgradedImages", (string)row[0]);
                if (null != upgradeImage)
                {
                    upgradeImage.AddChild(upgradeFile);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Upgraded", (string)row[0], "UpgradedImages"));
                }
            }
        }

        /// <summary>
        /// Decompile the UpgradedFilesToIgnore table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileUpgradedFilesToIgnoreTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                if ("*" != (string)row[0])
                {
                    Wix.UpgradeFile upgradeFile = new Wix.UpgradeFile();

                    upgradeFile.File = (string)row[1];

                    upgradeFile.Ignore = Wix.YesNoType.yes;

                    Wix.UpgradeImage upgradeImage = (Wix.UpgradeImage)this.core.GetIndexedElement("UpgradedImages", (string)row[0]);
                    if (null != upgradeImage)
                    {
                        upgradeImage.AddChild(upgradeFile);
                    }
                    else
                    {
                        this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Upgraded", (string)row[0], "UpgradedImages"));
                    }
                }
                else
                {
                    this.core.OnMessage(WixWarnings.UnrepresentableColumnValue(row.SourceLineNumbers, table.Name, row.Fields[0].Column.Name, row[0]));
                }
            }
        }

        /// <summary>
        /// Decompile the UpgradedImages table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileUpgradedImagesTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.UpgradeImage upgradeImage = new Wix.UpgradeImage();

                upgradeImage.Id = (string)row[0];

                upgradeImage.SourceFile = (string)row[1];

                if (null != row[2])
                {
                    upgradeImage.SourcePatch = (string)row[2];
                }

                if (null != row[3])
                {
                    string[] symbolPaths = ((string)row[3]).Split(';');

                    foreach (string symbolPathString in symbolPaths)
                    {
                        Wix.SymbolPath symbolPath = new Wix.SymbolPath();

                        symbolPath.Path = symbolPathString;

                        upgradeImage.AddChild(symbolPath);
                    }
                }

                Wix.Family family = (Wix.Family)this.core.GetIndexedElement("ImageFamilies", (string)row[4]);
                if (null != family)
                {
                    family.AddChild(upgradeImage);
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, table.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), "Family", (string)row[4], "ImageFamilies"));
                }
                this.core.IndexElement(row, upgradeImage);
            }
        }

        /// <summary>
        /// Decompile the UIText table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileUITextTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.UIText uiText = new Wix.UIText();

                uiText.Id = (string)row[0];

                uiText.Content = (string)row[1];

                this.core.UIElement.AddChild(uiText);
            }
        }

        /// <summary>
        /// Decompile the Verb table.
        /// </summary>
        /// <param name="table">The table to decompile.</param>
        private void DecompileVerbTable(Table table)
        {
            foreach (Row row in table.Rows)
            {
                Wix.Verb verb = new Wix.Verb();

                verb.Id = (string)row[1];

                if (null != row[2])
                {
                    verb.Sequence = (int)row[2];
                }

                if (null != row[3])
                {
                    verb.Command = (string)row[3];
                }

                if (null != row[4])
                {
                    verb.Argument = (string)row[4];
                }

                this.core.IndexElement(row, verb);
            }
        }

        /// <summary>
        /// Gets the RegistryRootType from an integer representation of the root.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information for the root.</param>
        /// <param name="tableName">The name of the table containing the field.</param>
        /// <param name="field">The field containing the root value.</param>
        /// <param name="registryRootType">The strongly-typed representation of the root.</param>
        /// <returns>true if the value could be converted; false otherwise.</returns>
        private bool GetRegistryRootType(SourceLineNumberCollection sourceLineNumbers, string tableName, Field field, out Wix.RegistryRootType registryRootType)
        {
            switch ((int)field.Data)
            {
                case (-1):
                    registryRootType = Wix.RegistryRootType.HKMU;
                    return true;
                case MsiInterop.MsidbRegistryRootClassesRoot:
                    registryRootType = Wix.RegistryRootType.HKCR;
                    return true;
                case MsiInterop.MsidbRegistryRootCurrentUser:
                    registryRootType = Wix.RegistryRootType.HKCU;
                    return true;
                case MsiInterop.MsidbRegistryRootLocalMachine:
                    registryRootType = Wix.RegistryRootType.HKLM;
                    return true;
                case MsiInterop.MsidbRegistryRootUsers:
                    registryRootType = Wix.RegistryRootType.HKU;
                    return true;
                default:
                    this.core.OnMessage(WixWarnings.IllegalColumnValue(sourceLineNumbers, tableName, field.Column.Name, field.Data));
                    registryRootType = Wix.RegistryRootType.HKCR; // assign anything to satisfy the out parameter
                    return false;
            }
        }

        /// <summary>
        /// Set the primary feature for a component.
        /// </summary>
        /// <param name="row">The row which specifies a primary feature.</param>
        /// <param name="featureColumnIndex">The index of the column contaning the feature identifier.</param>
        /// <param name="componentColumnIndex">The index of the column containing the component identifier.</param>
        private void SetPrimaryFeature(Row row, int featureColumnIndex, int componentColumnIndex)
        {
            // only products contain primary features
            if (OutputType.Product == this.outputType)
            {
                Field featureField = row.Fields[featureColumnIndex];
                Field componentField = row.Fields[componentColumnIndex];

                Wix.ComponentRef componentRef = (Wix.ComponentRef)this.core.GetIndexedElement("FeatureComponents", (string)featureField.Data, (string)componentField.Data);

                if (null != componentRef)
                {
                    componentRef.Primary = Wix.YesNoType.yes;
                }
                else
                {
                    this.core.OnMessage(WixWarnings.ExpectedForeignRow(row.SourceLineNumbers, row.TableDefinition.Name, row.GetPrimaryKey(DecompilerCore.PrimaryKeyDelimiter), featureField.Column.Name, (string)featureField.Data, componentField.Column.Name, (string)componentField.Data, "FeatureComponents"));
                }
            }
        }
    }
}
