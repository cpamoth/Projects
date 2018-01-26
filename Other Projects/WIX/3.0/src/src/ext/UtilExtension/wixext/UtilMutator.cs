//-------------------------------------------------------------------------------------------------
// <copyright file="UtilMutator.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The mutator for the Windows Installer XML Toolset Utility Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The template type.
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        /// A fragment template.
        /// </summary>
        Fragment,

        /// <summary>
        /// A module template.
        /// </summary>
        Module,

        /// <summary>
        /// A product template.
        /// </summary>
        Product
    }

    /// <summary>
    /// The mutator for the Windows Installer XML Toolset Internet Information Services Extension.
    /// </summary>
    public sealed class UtilMutator : MutatorExtension
    {
        private ArrayList components;
        private bool createFragments;
        private ArrayList directories;
        private ArrayList directoryRefs;
        private ArrayList files;
        private ArrayList features;
        private SortedList fragments;
        private bool generateGuids;
        private Wix.IParentElement rootElement;
        private bool setUniqueIdentifiers;
        private TemplateType templateType;

        /// <summary>
        /// Instantiate a new UtilMutator.
        /// </summary>
        public UtilMutator()
        {
            this.components = new ArrayList();
            this.directories = new ArrayList();
            this.directoryRefs = new ArrayList();
            this.features = new ArrayList();
            this.files = new ArrayList();
            this.fragments = new SortedList();
        }

        /// <summary>
        /// Gets or sets the option to create fragments.
        /// </summary>
        /// <value>The option to create fragments.</value>
        public bool CreateFragments
        {
            get { return this.createFragments; }
            set { this.createFragments = value; }
        }

        /// <summary>
        /// Gets or sets the option to generate missing guids.
        /// </summary>
        /// <value>The option to generate missing guids.</value>
        public bool GenerateGuids
        {
            get { return this.generateGuids; }
            set { this.generateGuids = value; }
        }

        /// <summary>
        /// Gets the sequence of the extension.
        /// </summary>
        /// <value>The sequence of the extension.</value>
        public override int Sequence
        {
            get { return 1000; }
        }

        /// <summary>
        /// Gets of sets the option to set unique identifiers.
        /// </summary>
        /// <value>The option to set unique identifiers.</value>
        public bool SetUniqueIdentifiers
        {
            get { return this.setUniqueIdentifiers; }
            set { this.setUniqueIdentifiers = value; }
        }

        /// <summary>
        /// Gets or sets the template type.
        /// </summary>
        /// <value>The template type.</value>
        public TemplateType TemplateType
        {
            get { return this.templateType; }
            set { this.templateType = value; }
        }

        /// <summary>
        /// Mutate a WiX document.
        /// </summary>
        /// <param name="wix">The Wix document element.</param>
        public override void Mutate(Wix.Wix wix)
        {
            this.components.Clear();
            this.directories.Clear();
            this.directoryRefs.Clear();
            this.features.Clear();
            this.files.Clear();
            this.fragments.Clear();
            this.rootElement = null;

            // index elements in this wix document
            this.IndexElement(wix);

            this.MutateWix(wix);

            this.MutateFiles();

            this.MutateDirectories();

            this.MutateComponents();

            // add the components to the product feature after all the identifiers have been set
            if (TemplateType.Product == this.templateType)
            {
                Wix.Feature feature = (Wix.Feature)this.features[0];

                foreach (Wix.Component component in this.components)
                {
                    if (null != component.Id)
                    {
                        Wix.ComponentRef componentRef = new Wix.ComponentRef();
                        componentRef.Id = component.Id;
                        feature.AddChild(componentRef);
                    }
                }
            }
            // create a ComponentGroup with all the components
            else if (TemplateType.Fragment == this.templateType && !this.createFragments)
            {
                Wix.ComponentGroup componentGroup = new Wix.ComponentGroup();
                componentGroup.Id = "ComponentGroup1";

                foreach (Wix.Component component in this.components)
                {
                    if (null != component.Id)
                    {
                        Wix.ComponentRef componentRef = new Wix.ComponentRef();
                        componentRef.Id = component.Id;
                        componentGroup.AddChild(componentRef);
                    }
                }

                Wix.Fragment fragment = new Wix.Fragment();
                this.fragments.Add("ComponentGroup:1", fragment);
                fragment.AddChild(componentGroup);
            }

            foreach (Wix.Fragment fragment in this.fragments.Values)
            {
                wix.AddChild(fragment);
            }
        }

        /// <summary>
        /// Index an element.
        /// </summary>
        /// <param name="element">The element to index.</param>
        private void IndexElement(Wix.ISchemaElement element)
        {
            if (element is Wix.Component)
            {
                this.components.Add(element);
            }
            else if (element is Wix.Directory)
            {
                this.directories.Add(element);
            }
            else if (element is Wix.DirectoryRef)
            {
                this.directoryRefs.Add(element);
            }
            else if (element is Wix.Feature)
            {
                this.features.Add(element);
            }
            else if (element is Wix.File)
            {
                this.files.Add(element);
            }
            else if (element is Wix.Module || element is Wix.PatchCreation || element is Wix.Product)
            {
                Debug.Assert(null == this.rootElement);
                this.rootElement = (Wix.IParentElement)element;
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
            IdentifierGenerator identifierGenerator = null;

            if (this.setUniqueIdentifiers)
            {
                identifierGenerator = new IdentifierGenerator("Component");

                // index all the existing identifiers and names
                foreach (Wix.Component component in this.components)
                {
                    if (null != component.Id)
                    {
                        identifierGenerator.IndexExistingIdentifier(component.Id);
                    }
                    else
                    {
                        string firstFileId = string.Empty;

                        // attempt to create a possible identifier from the first file identifier in the component
                        foreach (Wix.File file in component[typeof(Wix.File)])
                        {
                            firstFileId = file.Id;
                            break;
                        }

                        identifierGenerator.IndexName(firstFileId);
                    }
                }
            }

            foreach (Wix.Component component in this.components)
            {
                if (this.setUniqueIdentifiers && null == component.Id)
                {
                    string firstFileId = string.Empty;

                    // attempt to create a possible identifier from the first file identifier in the component
                    foreach (Wix.File file in component[typeof(Wix.File)])
                    {
                        firstFileId = file.Id;
                        break;
                    }

                    component.Id = identifierGenerator.GetIdentifier(firstFileId);
                }

                if (null == component.Guid)
                {
                    component.Guid = this.GetGuid();
                }

                if (this.createFragments && component.ParentElement is Wix.Directory)
                {
                    Wix.Directory directory = (Wix.Directory)component.ParentElement;

                    // parent directory must have an identifier to create a reference to it
                    if (null == directory.Id)
                    {
                        break;
                    }

                    if (this.rootElement is Wix.Module)
                    {
                        // add a ComponentRef for the Component
                        Wix.ComponentRef componentRef = new Wix.ComponentRef();
                        componentRef.Id = component.Id;
                        this.rootElement.AddChild(componentRef);
                    }

                    // create a new Fragment
                    Wix.Fragment fragment = new Wix.Fragment();
                    this.fragments.Add(String.Concat("Component:", (null != component.Id ? component.Id : this.fragments.Count.ToString())), fragment);

                    // create a new DirectoryRef
                    Wix.DirectoryRef directoryRef = new Wix.DirectoryRef();
                    directoryRef.Id = directory.Id;
                    fragment.AddChild(directoryRef);

                    // move the Component from the the Directory to the DirectoryRef
                    directory.RemoveChild(component);
                    directoryRef.AddChild(component);
                }
            }
        }

        /// <summary>
        /// Mutate the directories.
        /// </summary>
        private void MutateDirectories()
        {
            // assign all identifiers before fragmenting (because fragmenting requires them all to be present)
            if (this.setUniqueIdentifiers)
            {
                IdentifierGenerator identifierGenerator = new IdentifierGenerator("Directory");

                // index all the existing identifiers and names
                foreach (Wix.Directory directory in this.directories)
                {
                    if (null != directory.Id)
                    {
                        identifierGenerator.IndexExistingIdentifier(directory.Id);
                    }
                    else
                    {
                        identifierGenerator.IndexName(directory.Name);
                    }
                }

                foreach (Wix.Directory directory in this.directories)
                {
                    if (null == directory.Id)
                    {
                        directory.Id = identifierGenerator.GetIdentifier(directory.Name);
                    }
                }
            }

            foreach (Wix.Directory directory in this.directories)
            {
                if (this.createFragments)
                {
                    if (directory.ParentElement is Wix.Directory)
                    {
                        Wix.Directory parentDirectory = (Wix.Directory)directory.ParentElement;

                        // parent directory must have an identifier to create a reference to it
                        if (null == parentDirectory.Id)
                        {
                            return;
                        }

                        // create a new Fragment
                        Wix.Fragment fragment = new Wix.Fragment();
                        this.fragments.Add(String.Concat("Directory:", ("TARGETDIR" == directory.Id ? null : (null != directory.Id ? directory.Id : this.fragments.Count.ToString()))), fragment);

                        // create a new DirectoryRef
                        Wix.DirectoryRef directoryRef = new Wix.DirectoryRef();
                        directoryRef.Id = parentDirectory.Id;
                        fragment.AddChild(directoryRef);

                        // move the Directory from the parent Directory to DirectoryRef
                        parentDirectory.RemoveChild(directory);
                        directoryRef.AddChild(directory);
                    }
                    else if (directory.ParentElement == this.rootElement)
                    {
                        // create a new Fragment
                        Wix.Fragment fragment = new Wix.Fragment();
                        this.fragments.Add(String.Concat("Directory:", ("TARGETDIR" == directory.Id ? null : (null != directory.Id ? directory.Id : this.fragments.Count.ToString()))), fragment);

                        // move the Directory from the root element to the Fragment
                        this.rootElement.RemoveChild(directory);
                        fragment.AddChild(directory);
                    }
                }
            }
        }

        /// <summary>
        /// Mutate the files.
        /// </summary>
        private void MutateFiles()
        {
            if (this.setUniqueIdentifiers)
            {
                IdentifierGenerator identifierGenerator = new IdentifierGenerator("File");

                // index all the existing identifiers and names
                foreach (Wix.File file in this.files)
                {
                    if (null != file.Id)
                    {
                        identifierGenerator.IndexExistingIdentifier(file.Id);
                    }
                    else
                    {
                        identifierGenerator.IndexName(file.Name);
                    }
                }

                foreach (Wix.File file in this.files)
                {
                    if (null == file.Id)
                    {
                        file.Id = identifierGenerator.GetIdentifier(file.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Mutate a Wix element.
        /// </summary>
        /// <param name="wix">The Wix element to mutate.</param>
        private void MutateWix(Wix.Wix wix)
        {
            if (TemplateType.Fragment != this.templateType)
            {
                if (null != this.rootElement || 0 != this.features.Count)
                {
                    throw new Exception("The template option cannot be used with Feature, Product, or Module elements present.");
                }

                // create a package element although it won't always be used
                Wix.Package package = new Wix.Package();
                if (TemplateType.Module == this.templateType)
                {
                    package.Id = this.GetGuid();
                }
                package.Compressed = Wix.YesNoType.yes;
                package.InstallerVersion = 200;

                // create the root directory
                Wix.Directory targetDir = new Wix.Directory();
                targetDir.Id = "TARGETDIR";
                targetDir.Name = "SourceDir";

                // add all previous root directories to the root directory
                foreach (Wix.Directory directory in this.directories)
                {
                    if (!(directory.ParentElement is Wix.Directory || directory.ParentElement is Wix.DirectoryRef))
                    {
                        ((Wix.IParentElement)directory.ParentElement).RemoveChild(directory);
                        targetDir.AddChild(directory);
                    }
                }

                // add children of DirectoryRef/@Id="TARGETROOT" elements to the root directory
                foreach (Wix.DirectoryRef directoryRef in this.directoryRefs)
                {
                    if ("TARGETDIR" == directoryRef.Id)
                    {
                        foreach (Wix.ISchemaElement element in directoryRef.Children)
                        {
                            targetDir.AddChild(element);
                        }
                        ((Wix.IParentElement)directoryRef.ParentElement).RemoveChild(directoryRef);
                    }
                }

                this.directories.Add(targetDir);

                if (TemplateType.Module == this.templateType)
                {
                    Wix.Module module = new Wix.Module();
                    module.Id = "PUT-MODULE-NAME-HERE";
                    module.Language = "1033";
                    module.Version = "1.0.0.0";

                    package.Manufacturer = "PUT-COMPANY-NAME-HERE";
                    module.AddChild(package);

                    // add the authoring from the fragments directly into the module
                    foreach (Wix.Fragment fragment in wix.Children)
                    {
                        foreach (Wix.ISchemaElement element in fragment.Children)
                        {
                            module.AddChild(element);
                        }
                    }

                    foreach (Wix.Fragment fragment in wix.Children)
                    {
                        wix.RemoveChild(fragment);
                    }

                    module.AddChild(targetDir);

                    wix.AddChild(module);
                    this.rootElement = module;
                }
                else // product
                {
                    Wix.Product product = new Wix.Product();
                    product.Id = this.GetGuid();
                    product.Language = "1033";
                    product.Manufacturer = "PUT-COMPANY-NAME-HERE";
                    product.Name = "PUT-PRODUCT-NAME-HERE";
                    product.UpgradeCode = this.GetGuid();
                    product.Version = "1.0.0.0";

                    product.AddChild(package);

                    Wix.Media media = new Wix.Media();
                    media.Id = 1;
                    media.Cabinet = "product.cab";
                    media.EmbedCab = Wix.YesNoType.yes;
                    product.AddChild(media);

                    Wix.Feature feature = new Wix.Feature();
                    feature.Id = "ProductFeature";
                    feature.Title = "PUT-FEATURE-TITLE-HERE";
                    feature.Level = 1;
                    product.AddChild(feature);
                    this.features.Add(feature);

                    // add the authoring from the fragments directly into the product
                    foreach (Wix.Fragment fragment in wix.Children)
                    {
                        foreach (Wix.ISchemaElement element in fragment.Children)
                        {
                            product.AddChild(element);
                        }
                    }

                    foreach (Wix.Fragment fragment in wix.Children)
                    {
                        wix.RemoveChild(fragment);
                    }

                    product.AddChild(targetDir);

                    wix.AddChild(product);
                    this.rootElement = product;
                }
            }
        }

        /// <summary>
        /// Get a generated guid or a placeholder for a guid.
        /// </summary>
        /// <returns>A generated guid or placeholder.</returns>
        private string GetGuid()
        {
            if (this.generateGuids)
            {
                return Guid.NewGuid().ToString("B", CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture);
            }
            else
            {
                return "PUT-GUID-HERE";
            }
        }
    }
}
