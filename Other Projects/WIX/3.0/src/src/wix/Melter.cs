//-------------------------------------------------------------------------------------------------
// <copyright file="Melter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Converts a wixout representation of an MSM database into a ComponentGroup the form of WiX source.
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
    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;
    using Installer = Microsoft.Tools.WindowsInstallerXml.Msi;

    /// <summary>
    /// Converts a wixout representation of an MSM database into a ComponentGroup the form of WiX source.
    /// </summary>
    public sealed class Melter
    {
        private MelterCore core;
        private Decompiler decompiler;

        private Wix.ComponentGroup componentGroup;
        private Wix.DirectoryRef primaryDirectoryRef;
        private Wix.Fragment fragment;

        private string id;
        private const string nullGuid = "{00000000-0000-0000-0000-000000000000}";

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public Decompiler Decompiler
        {
            get { return this.decompiler; }
            set { this.decompiler = value; }
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Creates a new melter object.
        /// </summary>
        /// <param name="decompiler">The decompiler to use during the melting process.</param>
        /// <param name="id">The Id to use for the ComponentGroup, DirectoryRef, and WixVariables. If null, defaults to the Module's Id</param>
        public Melter(Decompiler decompiler, string id)
        {
            this.core = new MelterCore(this.Message);

            this.componentGroup = new Wix.ComponentGroup();
            this.fragment = new Wix.Fragment();
            this.primaryDirectoryRef = new Wix.DirectoryRef();

            this.decompiler = decompiler;
            this.id = id;

            if (null == this.decompiler)
            {
                this.core.OnMessage(WixErrors.ExpectedDecompiler("The melting process"));
            }
        }

        /// <summary>
        /// Converts a Module wixout into a ComponentGroup.
        /// </summary>
        /// <param name="wixout">The output object representing the unbound merge module to melt.</param>
        /// <returns>The converted Module as a ComponentGroup.</returns>
        public Wix.Wix Melt(Output wixout)
        {
            string moduleId = GetModuleId(wixout);

            // Assign the default componentGroupId if none was specified
            if (null == this.id)
            {
                this.id = moduleId;
            }

            this.componentGroup.Id = this.id;
            this.primaryDirectoryRef.Id = this.id;

            PreDecompile(wixout);
            Wix.Wix wix = decompiler.Decompile(wixout);

            if (null == wix)
            {
                return wix;
            }

            ConvertModule(wix);

            return wix;
        }

        /// <summary>
        /// Converts a Module to a ComponentGroup and adds all of its relevant elements to the main fragment.
        /// </summary>
        /// <param name="wix">The output object representing an unbound merge module.</param>
        private void ConvertModule(Wix.Wix wix)
        {
            Wix.Module module = GetModule(wix);

            foreach (Wix.ISchemaElement child in module.Children)
            {
                if (child is Wix.Directory)
                {
                    bool isTargetDir = WalkDirectory((Wix.Directory)child);
                    if (isTargetDir)
                    {
                        continue;
                    }
                }
                else if (child is Wix.Dependency)
                {
                    AddPropertyRef(((Wix.Dependency)child).RequiredId);
                    continue;
                }
                else if (child is Wix.Package)
                {
                    continue;
                }

                this.fragment.AddChild(child);
            }

            AddProperty(module.Id, this.id);

            wix.RemoveChild(module);
            wix.AddChild(this.fragment);

            this.fragment.AddChild(this.componentGroup);
            this.fragment.AddChild(this.primaryDirectoryRef);
        }

        /// <summary>
        /// Gets the module from the Wix object.
        /// </summary>
        /// <param name="wix">The Wix object.</param>
        /// <returns>The Module in the Wix object, null if no Module was found</returns>
        private Wix.Module GetModule(Wix.Wix wix)
        {
            foreach (Wix.ISchemaElement element in wix.Children)
            {
                if (element is Wix.Module)
                {
                    return (Wix.Module)element;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a PropertyRef to the main Fragment.
        /// </summary>
        /// <param name="id">Id of the PropertyRef.</param>
        private void AddPropertyRef(string id)
        {
            Wix.PropertyRef propertyRef = new Wix.PropertyRef();
            propertyRef.Id = id;
            this.fragment.AddChild(propertyRef);
        }

        /// <summary>
        /// Adds a Property to the main Fragment.
        /// </summary>
        /// <param name="id">Id of the Property.</param>
        /// <param name="value">Value of the Property.</param>
        private void AddProperty(string id, string value)
        {
            Wix.Property property = new Wix.Property();
            property.Id = id;
            property.Value = value;
            this.fragment.AddChild(property);
        }

        /// <summary>
        /// Walks a directory structure obtaining Component Id's and Standard Directory Id's.
        /// </summary>
        /// <param name="directory">The Directory to walk.</param>
        /// <returns>true if the directory is TARGETDIR.</returns>
        private bool WalkDirectory(Wix.Directory directory)
        {
            bool isTargetDir = false;
            if ("TARGETDIR" == directory.Id)
            {
                isTargetDir = true;
            }

            string standardDirectoryId = null;
            if (StartsWithStandardDirectoryId(directory.Id, out standardDirectoryId) && !isTargetDir)
            {
                AddSetPropertyCustomAction(directory.Id, String.Format("[{0}]", standardDirectoryId));
            }

            foreach (Wix.ISchemaElement child in directory.Children)
            {
                if (child is Wix.Directory)
                {
                    if (isTargetDir)
                    {
                        this.primaryDirectoryRef.AddChild(child);
                    }
                    WalkDirectory((Wix.Directory)child);
                }
                else if (child is Wix.Component)
                {
                    if (isTargetDir)
                    {
                        this.primaryDirectoryRef.AddChild(child);
                    }
                    AddComponentRef((Wix.Component)child);
                }
            }

            return isTargetDir;
        }

        /// <summary>
        /// Gets the module Id out of the Output object.
        /// </summary>
        /// <param name="wixout">The output object.</param>
        /// <returns>The module Id from the Output object.</returns>
        private string GetModuleId(Output wixout)
        {
            // get the moduleId from the wixout
            Table moduleSignatureTable = wixout.Tables["ModuleSignature"];
            if (null == moduleSignatureTable || 0 >= moduleSignatureTable.Rows.Count)
            {
                this.core.OnMessage(WixErrors.ExpectedTableInMergeModule("ModuleSignature"));
            }
            return moduleSignatureTable.Rows[0].Fields[0].Data.ToString();
        }

        /// <summary>
        /// Determines if the directory Id starts with a standard directory id.
        /// </summary>
        /// <param name="directoryId">The directory id.</param>
        /// <param name="standardDirectoryId">The standard directory id.</param>
        /// <returns>true if the directory starts with a standard directory id.</returns>
        private bool StartsWithStandardDirectoryId(string directoryId, out string standardDirectoryId)
        {
            standardDirectoryId = null;
            foreach (string id in Installer.Installer.StandardDirectories.Keys)
            {
                if (directoryId.StartsWith(id))
                {
                    standardDirectoryId = id;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a ComponentRef to the main ComponentGroup.
        /// </summary>
        /// <param name="component">The component to add.</param>
        private void AddComponentRef(Wix.Component component)
        {
            Wix.ComponentRef componentRef = new Wix.ComponentRef();
            componentRef.Id = component.Id;
            this.componentGroup.AddChild(componentRef);
        }

        /// <summary>
        /// Adds a SetProperty CA for a Directory.
        /// </summary>
        /// <param name="id">The Id of the Property to set.</param>
        /// <param name="value">The value to set the Property to.</param>
        private void AddSetPropertyCustomAction(String id, string value)
        {
            // Add the action
            Wix.CustomAction customAction = new Wix.CustomAction();
            customAction.Id = id;
            customAction.Property = id;
            customAction.Value = value;
            this.fragment.AddChild(customAction);

            // Schedule the action
            Wix.InstallExecuteSequence installExecuteSequence = new Wix.InstallExecuteSequence();
            Wix.Custom custom = new Wix.Custom();
            custom.Action = customAction.Id;
            custom.Before = "CostInitialize";
            installExecuteSequence.AddChild(custom);
            this.fragment.AddChild(installExecuteSequence);
        }

        /// <summary>
        /// Does any operations to the wixout that would need to be done before decompiling.
        /// </summary>
        /// <param name="wixout">The output object representing the unbound merge module.</param>
        private void PreDecompile(Output wixout)
        {
            string wixVariable = String.Format("!(wix.{0}", this.id);

            foreach (Table table in wixout.Tables)
            {
                // Determine if the table has a feature foreign key
                bool hasFeatureForeignKey = false;
                foreach (ColumnDefinition columnDef in table.Definition.Columns)
                {
                    if (null != columnDef.KeyTable)
                    {
                        string[] keyTables = columnDef.KeyTable.Split(';');
                        foreach (string keyTable in keyTables)
                        {
                            if ("Feature" == keyTable)
                            {
                                hasFeatureForeignKey = true;
                                break;
                            }
                        }
                    }
                }

                // If this table has no foreign keys to the feature table, skip it.
                if (!hasFeatureForeignKey)
                {
                    continue;
                }

                // Go through all the rows and replace the null guid with the wix variable
                // for columns that are foreign keys into the feature table.
                foreach (Row row in table.Rows)
                {
                    foreach (Field field in row.Fields)
                    {
                        if (null != field.Column.KeyTable)
                        {
                            string[] keyTables = field.Column.KeyTable.Split(';');
                            foreach (string keyTable in keyTables)
                            {
                                if ("Feature" == keyTable)
                                {
                                    field.Data = field.Data.ToString().Replace(nullGuid, wixVariable);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
