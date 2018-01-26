//-------------------------------------------------------------------------------------------------
// <copyright file="UtilFinalizeHarvesterMutator.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The finalize harvester mutator for the Windows Installer XML Toolset Utility Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The finalize harvester mutator for the Windows Installer XML Toolset Utility Extension.
    /// </summary>
    public sealed class UtilFinalizeHarvesterMutator : MutatorExtension
    {
        private ArrayList components;
        private ArrayList directories;
        private SortedList directoryPaths;
        private Hashtable filePaths;
        private ArrayList files;
        private ArrayList registryValues;
        private bool suppressCOMElements;

        /// <summary>
        /// Instantiate a new UtilFinalizeHarvesterMutator.
        /// </summary>
        public UtilFinalizeHarvesterMutator()
        {
            this.components = new ArrayList();
            this.directories = new ArrayList();
            this.directoryPaths = new SortedList();
            this.filePaths = new Hashtable();
            this.files = new ArrayList();
            this.registryValues = new ArrayList();
        }

        /// <summary>
        /// Gets the sequence of the extension.
        /// </summary>
        /// <value>The sequence of the extension.</value>
        public override int Sequence
        {
            get { return 2000; }
        }

        /// <summary>
        /// Gets or sets the option to suppress COM elements.
        /// </summary>
        /// <value>The option to suppress COM elements.</value>
        public bool SuppressCOMElements
        {
            get { return this.suppressCOMElements; }
            set { this.suppressCOMElements = value; }
        }

        /// <summary>
        /// Mutate a WiX document.
        /// </summary>
        /// <param name="wix">The Wix document element.</param>
        public override void Mutate(Wix.Wix wix)
        {
            this.components.Clear();
            this.directories.Clear();
            this.directoryPaths.Clear();
            this.filePaths.Clear();
            this.files.Clear();
            this.registryValues.Clear();

            // index elements in this wix document
            this.IndexElement(wix);

            this.MutateDirectories();

            this.MutateFiles();

            this.MutateRegistryValues();

            // must occur after all the registry values have been formatted
            this.MutateComponents();
        }

        /// <summary>
        /// Index an element.
        /// </summary>
        /// <param name="element">The element to index.</param>
        private void IndexElement(Wix.ISchemaElement element)
        {
            if (element is Wix.Component)
            {
                // Component elements only need to be indexed if COM registry values will be strongly typed
                if (!this.suppressCOMElements)
                {
                    this.components.Add(element);
                }
            }
            else if (element is Wix.Directory)
            {
                this.directories.Add(element);
            }
            else if (element is Wix.File)
            {
                this.files.Add(element);
            }
            else if (element is Wix.RegistryValue)
            {
                this.registryValues.Add(element);
            }

            // index the child elements
            if (element is Wix.IParentElement)
            {
                foreach (Wix.ISchemaElement childElement in ((Wix.IParentElement)element).Children)
                {
                    this.IndexElement(childElement);
                }
            }
        }

        /// <summary>
        /// Mutate the components.
        /// </summary>
        private void MutateComponents()
        {
            foreach (Wix.Component component in this.components)
            {
                SortedList indexedElements = CollectionsUtil.CreateCaseInsensitiveSortedList();
                SortedList indexedRegistryValues = CollectionsUtil.CreateCaseInsensitiveSortedList();

                // index all the File elements
                foreach (Wix.File file in component[typeof(Wix.File)])
                {
                    indexedElements.Add(String.Concat("file/", file.Id), file);
                }

                // group all the registry values by the COM element they would correspond to and
                // create a COM element for each group
                foreach (Wix.RegistryValue registryValue in component[typeof(Wix.RegistryValue)])
                {
                    if (Wix.RegistryValue.ActionType.write == registryValue.Action && Wix.RegistryRootType.HKCR == registryValue.Root && Wix.RegistryValue.TypeType.@string == registryValue.Type)
                    {
                        string index = null;
                        string[] parts = registryValue.Key.Split('\\');

                        // create a COM element for COM registration and index it
                        if (1 <= parts.Length)
                        {
                            if (0 == String.Compare(parts[0], "AppID", true))
                            {
                                // only work with GUID AppIds here
                                if (2 <= parts.Length && parts[1].StartsWith("{") && parts[1].EndsWith("}"))
                                {
                                    index = String.Concat(parts[0], '/', parts[1]);

                                    if (!indexedElements.Contains(index))
                                    {
                                        Wix.AppId appId = new Wix.AppId();
                                        appId.Id = parts[1].ToUpper(CultureInfo.InvariantCulture);
                                        indexedElements.Add(index, appId);
                                    }
                                }
                            }
                            else if (0 == String.Compare(parts[0], "CLSID", true))
                            {
                                if (2 <= parts.Length)
                                {
                                    index = String.Concat(parts[0], '/', parts[1]);

                                    if (!indexedElements.Contains(index))
                                    {
                                        Wix.Class wixClass = new Wix.Class();
                                        wixClass.Id = parts[1].ToUpper(CultureInfo.InvariantCulture);
                                        indexedElements.Add(index, wixClass);
                                    }
                                }
                            }
                            else if (0 == String.Compare(parts[0], "Component Categories", true))
                            {
                                // TODO: add support for this to the compiler
                            }
                            else if (0 == String.Compare(parts[0], "Interface", true))
                            {
                                if (2 <= parts.Length)
                                {
                                    index = String.Concat(parts[0], '/', parts[1]);

                                    if (!indexedElements.Contains(index))
                                    {
                                        Wix.Interface wixInterface = new Wix.Interface();
                                        wixInterface.Id = parts[1].ToUpper(CultureInfo.InvariantCulture);
                                        indexedElements.Add(index, wixInterface);
                                    }
                                }
                            }
                            else if (0 == String.Compare(parts[0], "TypeLib"))
                            {
                                if (3 <= parts.Length)
                                {
                                    // use a special index to ensure progIds are processed before classes
                                    index = String.Concat(".typelib/", parts[1], '/', parts[2]);

                                    if (!indexedElements.Contains(index))
                                    {
                                        try
                                        {
                                            // TODO: properly handle hexadecimal in version
                                            Version version = new Version(parts[2]);

                                            Wix.TypeLib typeLib = new Wix.TypeLib();
                                            typeLib.Id = parts[1].ToUpper(CultureInfo.InvariantCulture);
                                            typeLib.MajorVersion = version.Major;
                                            typeLib.MinorVersion = version.Minor;
                                            indexedElements.Add(index, typeLib);
                                        }
                                        catch // not a valid type library registry value
                                        {
                                            index = null;
                                        }
                                    }
                                }
                            }
                            else if (parts[0].StartsWith("."))
                            {
                                // extension
                            }
                            else // ProgId (hopefully)
                            {
                                // use a special index to ensure progIds are processed before classes
                                index = String.Concat(".progid/", parts[0]);

                                if (!indexedElements.Contains(index))
                                {
                                    Wix.ProgId progId = new Wix.ProgId();
                                    progId.Id = parts[0];
                                    indexedElements.Add(index, progId);
                                }
                            }
                        }

                        // index the RegistryValue element according to the COM element it corresponds to
                        if (null != index)
                        {
                            SortedList registryValues = (SortedList)indexedRegistryValues[index];

                            if (null == registryValues)
                            {
                                registryValues = CollectionsUtil.CreateCaseInsensitiveSortedList();
                                indexedRegistryValues.Add(index, registryValues);
                            }

                            registryValues.Add(String.Concat(registryValue.Key, '/', registryValue.Name), registryValue);
                        }
                    }
                }

                // set various values on the COM elements from their corresponding registry values
                Hashtable indexedProcessedRegistryValues = new Hashtable();
                foreach (DictionaryEntry entry in indexedRegistryValues)
                {
                    Wix.ISchemaElement element = (Wix.ISchemaElement)indexedElements[entry.Key];
                    string parentIndex = null;
                    SortedList registryValues = (SortedList)entry.Value;

                    // element-specific variables (for really tough situations)
                    string classAppId = null;
                    bool threadingModelSet = false;

                    foreach (Wix.RegistryValue registryValue in registryValues.Values)
                    {
                        string[] parts = registryValue.Key.ToLower(CultureInfo.InvariantCulture).Split('\\');
                        bool processed = false;

                        if (element is Wix.AppId)
                        {
                            Wix.AppId appId = (Wix.AppId)element;

                            if (2 == parts.Length)
                            {
                                if (null == registryValue.Name)
                                {
                                    appId.Description = registryValue.Value;
                                    processed = true;
                                }
                            }
                        }
                        else if (element is Wix.Class)
                        {
                            Wix.Class wixClass = (Wix.Class)element;

                            if (2 == parts.Length)
                            {
                                if (null == registryValue.Name)
                                {
                                    wixClass.Description = registryValue.Value;
                                    processed = true;
                                }
                                else if (0 == String.Compare(registryValue.Name, "AppID", true))
                                {
                                    classAppId = registryValue.Value;
                                    processed = true;
                                }
                            }
                            else if (3 == parts.Length)
                            {
                                Wix.Class.ContextType contextType = Wix.Class.ContextType.None;

                                switch (parts[2])
                                {
                                    case "inprochandler":
                                        if (null == registryValue.Name)
                                        {
                                            if (null == wixClass.Handler)
                                            {
                                                wixClass.Handler = "1";
                                                processed = true;
                                            }
                                            else if ("2" == wixClass.Handler)
                                            {
                                                wixClass.Handler = "3";
                                                processed = true;
                                            }
                                        }
                                        break;
                                    case "inprochandler32":
                                        if (null == registryValue.Name)
                                        {
                                            if (null == wixClass.Handler)
                                            {
                                                wixClass.Handler = "2";
                                                processed = true;
                                            }
                                            else if ("1" == wixClass.Handler)
                                            {
                                                wixClass.Handler = "3";
                                                processed = true;
                                            }
                                        }
                                        break;
                                    case "inprocserver":
                                        contextType = Wix.Class.ContextType.InprocServer;
                                        break;
                                    case "inprocserver32":
                                        contextType = Wix.Class.ContextType.InprocServer32;
                                        break;
                                    case "localserver":
                                        contextType = Wix.Class.ContextType.LocalServer;
                                        break;
                                    case "localserver32":
                                        contextType = Wix.Class.ContextType.LocalServer32;
                                        break;
                                    case "progid":
                                        if (null == registryValue.Name)
                                        {
                                            Wix.ProgId progId = (Wix.ProgId)indexedElements[String.Concat(".progid/", registryValue.Value)];

                                            // verify that the versioned ProgId appears under this Class element
                                            // if not, toss the entire element
                                            if (null == progId || wixClass != progId.ParentElement)
                                            {
                                                element = null;
                                            }
                                            else
                                            {
                                                processed = true;
                                            }
                                        }
                                        break;
                                    case "typelib":
                                        if (null == registryValue.Name)
                                        {
                                            foreach (DictionaryEntry indexedEntry in indexedElements)
                                            {
                                                string key = (string)indexedEntry.Key;
                                                Wix.ISchemaElement possibleTypeLib = (Wix.ISchemaElement)indexedEntry.Value;

                                                if (key.StartsWith(".typelib/") &&
                                                    0 == String.Compare(key, 9, registryValue.Value, 0, registryValue.Value.Length, true))
                                                {
                                                    // ensure the TypeLib is nested under the same thing we want the Class under
                                                    if (null == parentIndex || indexedElements[parentIndex] == possibleTypeLib.ParentElement)
                                                    {
                                                        parentIndex = key;
                                                        processed = true;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case "version":
                                        if (null == registryValue.Name)
                                        {
                                            wixClass.Version = registryValue.Value;
                                            processed = true;
                                        }
                                        break;
                                    case "versionindependentprogid":
                                        if (null == registryValue.Name)
                                        {
                                            Wix.ProgId progId = (Wix.ProgId)indexedElements[String.Concat(".progid/", registryValue.Value)];

                                            // verify that the version independent ProgId appears somewhere
                                            // under this Class element - if not, toss the entire element
                                            if (null == progId || wixClass != progId.ParentElement)
                                            {
                                                // check the parent of the parent
                                                if (null == progId || null == progId.ParentElement || wixClass != progId.ParentElement.ParentElement)
                                                {
                                                    element = null;
                                                }
                                            }

                                            processed = true;
                                        }
                                        break;
                                }

                                if (Wix.Class.ContextType.None != contextType)
                                {
                                    wixClass.Context |= contextType;

                                    if (null == registryValue.Name)
                                    {
                                        if ((registryValue.Value.StartsWith("[!") || registryValue.Value.StartsWith("[#")) && registryValue.Value.EndsWith("]"))
                                        {
                                            parentIndex = String.Concat("file/", registryValue.Value.Substring(2, registryValue.Value.Length - 3));
                                            processed = true;
                                        }
                                    }
                                    else if (0 == String.Compare(registryValue.Name, "ThreadingModel", true))
                                    {
                                        Wix.Class.ThreadingModelType threadingModel;

                                        switch (registryValue.Value.ToLower(CultureInfo.InvariantCulture))
                                        {
                                            case "apartment":
                                                threadingModel = Wix.Class.ThreadingModelType.apartment;
                                                processed = true;
                                                break;
                                            case "both":
                                                threadingModel = Wix.Class.ThreadingModelType.both;
                                                processed = true;
                                                break;
                                            case "free":
                                                threadingModel = Wix.Class.ThreadingModelType.free;
                                                processed = true;
                                                break;
                                            case "neutral":
                                                threadingModel = Wix.Class.ThreadingModelType.neutral;
                                                processed = true;
                                                break;
                                            case "rental":
                                                threadingModel = Wix.Class.ThreadingModelType.rental;
                                                processed = true;
                                                break;
                                            case "single":
                                                threadingModel = Wix.Class.ThreadingModelType.single;
                                                processed = true;
                                                break;
                                            default:
                                                continue;
                                        }

                                        if (!threadingModelSet || wixClass.ThreadingModel == threadingModel)
                                        {
                                            wixClass.ThreadingModel = threadingModel;
                                            threadingModelSet = true;
                                        }
                                        else
                                        {
                                            element = null;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (element is Wix.Interface)
                        {
                            Wix.Interface wixInterface = (Wix.Interface)element;

                            if (2 == parts.Length && null == registryValue.Name)
                            {
                                wixInterface.Name = registryValue.Value;
                                processed = true;
                            }
                            else if (3 == parts.Length)
                            {
                                switch (parts[2])
                                {
                                    case "proxystubclsid":
                                        if (null == registryValue.Name)
                                        {
                                            wixInterface.ProxyStubClassId = registryValue.Value.ToUpper(CultureInfo.InvariantCulture);
                                            processed = true;
                                        }
                                        break;
                                    case "proxystubclsid32":
                                        if (null == registryValue.Name)
                                        {
                                            wixInterface.ProxyStubClassId32 = registryValue.Value.ToUpper(CultureInfo.InvariantCulture);
                                            processed = true;
                                        }
                                        break;
                                    case "nummethods":
                                        if (null == registryValue.Name)
                                        {
                                            wixInterface.NumMethods = Convert.ToInt32(registryValue.Value, CultureInfo.InvariantCulture);
                                            processed = true;
                                        }
                                        break;
                                    case "typelib":
                                        if (0 == String.Compare("Version", registryValue.Name, true))
                                        {
                                            parentIndex = String.Concat(parentIndex, registryValue.Value);
                                            processed = true;
                                        }
                                        else if (null == registryValue.Name) // TypeLib guid
                                        {
                                            parentIndex = String.Concat(".typelib/", registryValue.Value, '/', parentIndex);
                                            processed = true;
                                        }
                                        break;
                                }
                            }
                        }
                        else if (element is Wix.ProgId)
                        {
                            Wix.ProgId progId = (Wix.ProgId)element;

                            if (null == registryValue.Name)
                            {
                                if (1 == parts.Length)
                                {
                                    progId.Description = registryValue.Value;
                                    processed = true;
                                }
                                else if (2 == parts.Length)
                                {
                                    if (0 == String.Compare(parts[1], "CLSID", true))
                                    {
                                        parentIndex = String.Concat("CLSID/", registryValue.Value);
                                        processed = true;
                                    }
                                    else if (0 == String.Compare(parts[1], "CurVer", true))
                                    {
                                        // this registry value should usually be processed second so the
                                        // version independent ProgId should be under the versioned one
                                        parentIndex = String.Concat(".progid/", registryValue.Value);
                                        processed = true;
                                    }
                                }
                            }
                        }
                        else if (element is Wix.TypeLib)
                        {
                            Wix.TypeLib typeLib = (Wix.TypeLib)element;

                            if (null == registryValue.Name)
                            {
                                if (3 == parts.Length)
                                {
                                    typeLib.Description = registryValue.Value;
                                    processed = true;
                                }
                                else if (4 == parts.Length)
                                {
                                    switch (parts[3].ToLower(CultureInfo.InvariantCulture))
                                    {
                                        case "flags":
                                            int flags = Convert.ToInt32(registryValue.Value, CultureInfo.InvariantCulture);

                                            if (0x1 == (flags & 0x1))
                                            {
                                                typeLib.Restricted = Wix.YesNoType.yes;
                                            }

                                            if (0x2 == (flags & 0x2))
                                            {
                                                typeLib.Control = Wix.YesNoType.yes;
                                            }

                                            if (0x4 == (flags & 0x4))
                                            {
                                                typeLib.Hidden = Wix.YesNoType.yes;
                                            }

                                            if (0x8 == (flags & 0x8))
                                            {
                                                typeLib.HasDiskImage = Wix.YesNoType.yes;
                                            }

                                            processed = true;
                                            break;
                                        case "helpdir":
                                            if (registryValue.Value.StartsWith("[") && (registryValue.Value.EndsWith("]") || registryValue.Value.EndsWith("]\\")))
                                            {
                                                typeLib.HelpDirectory = registryValue.Value.Substring(1, registryValue.Value.LastIndexOf(']') - 1);
                                                processed = true;
                                            }
                                            break;
                                    }
                                }
                                else if (5 == parts.Length && 0 == String.Compare("win32", parts[4], true))
                                {
                                    typeLib.Language = Convert.ToInt32(parts[3], CultureInfo.InvariantCulture);

                                    if ((registryValue.Value.StartsWith("[!") || registryValue.Value.StartsWith("[#")) && registryValue.Value.EndsWith("]"))
                                    {
                                        parentIndex = String.Concat("file/", registryValue.Value.Substring(2, registryValue.Value.Length - 3));
                                    }

                                    processed = true;
                                }
                            }
                        }

                        // index the processed registry values by their corresponding COM element
                        if (processed)
                        {
                            indexedProcessedRegistryValues.Add(registryValue, element);
                        }
                    }

                    // parent the COM element
                    if (null != element)
                    {
                        if (null != parentIndex)
                        {
                            Wix.IParentElement parentElement = (Wix.IParentElement)indexedElements[parentIndex];

                            if (null != parentElement)
                            {
                                parentElement.AddChild(element);
                            }
                        }
                        else
                        {
                            component.AddChild(element);
                        }

                        // special handling for AppID since it doesn't fit the general model
                        if (null != classAppId)
                        {
                            Wix.AppId appId = (Wix.AppId)indexedElements[String.Concat("AppID/", classAppId)];

                            // move the Class element under the AppId (and put the AppId under its old parent)
                            if (null != appId)
                            {
                                // move the AppId element
                                ((Wix.IParentElement)appId.ParentElement).RemoveChild(appId);
                                ((Wix.IParentElement)element.ParentElement).AddChild(appId);

                                // move the Class element
                                ((Wix.IParentElement)element.ParentElement).RemoveChild(element);
                                appId.AddChild(element);
                            }
                        }
                    }
                }

                // remove the RegistryValue elements which were converted into COM elements
                // that were successfully nested under the Component element
                foreach (DictionaryEntry entry in indexedProcessedRegistryValues)
                {
                    Wix.ISchemaElement element = (Wix.ISchemaElement)entry.Value;
                    Wix.RegistryValue registryValue = (Wix.RegistryValue)entry.Key;

                    while (null != element)
                    {
                        if (element == component)
                        {
                            ((Wix.IParentElement)registryValue.ParentElement).RemoveChild(registryValue);
                            break;
                        }

                        element = element.ParentElement;
                    }
                }
            }
        }

        /// <summary>
        /// Mutate the directories.
        /// </summary>
        private void MutateDirectories()
        {
            foreach (Wix.Directory directory in this.directories)
            {
                string path = directory.FileSource;

                // create a new directory element without the FileSource attribute
                if (null != path)
                {
                    Wix.Directory newDirectory = new Wix.Directory();

                    newDirectory.Id = directory.Id;
                    newDirectory.Name = directory.Name;

                    foreach (Wix.ISchemaElement element in directory.Children)
                    {
                        newDirectory.AddChild(element);
                    }

                    ((Wix.IParentElement)directory.ParentElement).AddChild(newDirectory);
                    ((Wix.IParentElement)directory.ParentElement).RemoveChild(directory);

                    if (null != newDirectory.Id)
                    {
                        this.directoryPaths[path.ToLower(CultureInfo.InvariantCulture)] = String.Concat("[", newDirectory.Id, "]");
                    }
                }
            }
        }

        /// <summary>
        /// Mutate the files.
        /// </summary>
        private void MutateFiles()
        {
            foreach (Wix.File file in this.files)
            {
                if (null != file.Id && null != file.Source)
                {
                    // index the long path
                    this.filePaths[file.Source.ToLower(CultureInfo.InvariantCulture)] = String.Concat("[#", file.Id, "]");

                    // index the long path as a URL for assembly harvesting
                    Uri fileUri = new Uri(file.Source);
                    this.filePaths[fileUri.ToString().ToLower(CultureInfo.InvariantCulture)] = String.Concat("[#", file.Id, "]");

                    // index the short path
                    string shortPath = NativeMethods.GetShortPathName(file.Source);
                    this.filePaths[shortPath.ToLower(CultureInfo.InvariantCulture)] = String.Concat("[!", file.Id, "]");
                }
            }
        }

        /// <summary>
        /// Mutate the registry values.
        /// </summary>
        private void MutateRegistryValues()
        {
            ArrayList reversedDirectoryPaths = new ArrayList();

            // reverse the indexed directory paths to ensure the longest paths are found first
            foreach (DictionaryEntry entry in this.directoryPaths)
            {
                reversedDirectoryPaths.Insert(0, entry);
            }

            foreach (Wix.RegistryValue registryValue in this.registryValues)
            {
                string lowercaseValue = registryValue.Value.ToLower(CultureInfo.InvariantCulture);

                // first replace file paths with their MSI tokens
                foreach (DictionaryEntry entry in this.filePaths)
                {
                    int index;

                    while (0 <= (index = lowercaseValue.IndexOf((string)entry.Key)))
                    {
                        registryValue.Value = registryValue.Value.Remove(index, ((string)entry.Key).Length);
                        registryValue.Value = registryValue.Value.Insert(index, (string)entry.Value);
                        lowercaseValue = registryValue.Value.ToLower(CultureInfo.InvariantCulture);
                    }
                }

                // next replace directory paths with their MSI tokens
                foreach (DictionaryEntry entry in reversedDirectoryPaths)
                {
                    int index;

                    while (0 <= (index = lowercaseValue.IndexOf((string)entry.Key)))
                    {
                        registryValue.Value = registryValue.Value.Remove(index, ((string)entry.Key).Length);
                        registryValue.Value = registryValue.Value.Insert(index, (string)entry.Value);
                        lowercaseValue = registryValue.Value.ToLower(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        /// <summary>
        /// The native methods for grabbing machine-specific short file paths.
        /// </summary>
        private class NativeMethods
        {
            private const int MaxPath = 255;

            /// <summary>
            /// Gets the short name for a file.
            /// </summary>
            /// <param name="fullPath">Fullpath to file on disk.</param>
            /// <returns>Short name for file.</returns>
            internal static string GetShortPathName(string fullPath)
            {
                StringBuilder shortPath = new StringBuilder(MaxPath, MaxPath);

                uint result = GetShortPathName(fullPath, shortPath, MaxPath);

                if (0 == result)
                {
                    int err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    throw new System.Runtime.InteropServices.COMException("Failed to get short path name", err);
                }

                return shortPath.ToString();
            }

            /// <summary>
            /// Gets the short name for a file.
            /// </summary>
            /// <param name="longPath">Long path to convert to short path.</param>
            /// <param name="shortPath">Short path from long path.</param>
            /// <param name="buffer">Size of short path.</param>
            /// <returns>zero if success.</returns>
            [DllImport("kernel32.dll", EntryPoint = "GetShortPathNameW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
            internal static extern uint GetShortPathName(string longPath, StringBuilder shortPath, [MarshalAs(UnmanagedType.U4)]int buffer);
        }
    }
}
