//-------------------------------------------------------------------------------------------------
// <copyright file="UtilHarvesterMutator.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset harvester mutator.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.IO;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The Windows Installer XML Toolset harvester mutator.
    /// </summary>
    public sealed class UtilHarvesterMutator : MutatorExtension
    {
        /// <summary>
        /// Gets the sequence of this mutator extension.
        /// </summary>
        /// <value>The sequence of this mutator extension.</value>
        public override int Sequence
        {
            get { return 100; }
        }

        /// <summary>
        /// Mutate a WiX document.
        /// </summary>
        /// <param name="wix">The Wix document element.</param>
        public override void Mutate(Wix.Wix wix)
        {
            this.MutateElement(null, wix);
        }

        /// <summary>
        /// Mutate an element.
        /// </summary>
        /// <param name="parentElement">The parent of the element to mutate.</param>
        /// <param name="element">The element to mutate.</param>
        private void MutateElement(Wix.IParentElement parentElement, Wix.ISchemaElement element)
        {
            if (element is Wix.File)
            {
                this.MutateFile(parentElement, (Wix.File)element);
            }

            // mutate the child elements
            if (element is Wix.IParentElement)
            {
                ArrayList childElements = new ArrayList();

                // copy the child elements to a temporary array (to allow them to be deleted/moved)
                foreach (Wix.ISchemaElement childElement in ((Wix.IParentElement)element).Children)
                {
                    childElements.Add(childElement);
                }

                foreach (Wix.ISchemaElement childElement in childElements)
                {
                    this.MutateElement((Wix.IParentElement)element, childElement);
                }
            }
        }

        /// <summary>
        /// Mutate a file.
        /// </summary>
        /// <param name="parentElement">The parent of the element to mutate.</param>
        /// <param name="file">The file to mutate.</param>
        private void MutateFile(Wix.IParentElement parentElement, Wix.File file)
        {
            if (null != file.Source)
            {
                string fileExtension = Path.GetExtension(file.Source);

                if (0 == String.Compare(".ax", fileExtension, true) || // DirectShow filter
                    0 == String.Compare(".dll", fileExtension, true) ||
                    0 == String.Compare(".exe", fileExtension, true) ||
                    0 == String.Compare(".ocx", fileExtension, true) || // ActiveX
                    0 == String.Compare(".olb", fileExtension, true) || // type library
                    0 == String.Compare(".tlb", fileExtension, true)) // type library
                {
                    // try the assembly harvester
                    try
                    {
                        AssemblyHarvester assemblyHarvester = new AssemblyHarvester();

                        Wix.RegistryValue[] registryValues = assemblyHarvester.HarvestRegistryValues(file.Source);

                        foreach (Wix.RegistryValue registryValue in registryValues)
                        {
                            parentElement.AddChild(registryValue);
                        }
                    }
                    catch
                    {
                        // try the self-reg harvester
                        try
                        {
                            DllHarvester dllHarvester = new DllHarvester();

                            Wix.RegistryValue[] registryValues = dllHarvester.HarvestRegistryValues(file.Source);

                            foreach (Wix.RegistryValue registryValue in registryValues)
                            {
                                parentElement.AddChild(registryValue);
                            }
                        }
                        catch
                        {
                            // try the type library harvester
                            try
                            {
                                TypeLibraryHarvester typeLibHarvester = new TypeLibraryHarvester();

                                Wix.RegistryValue[] registryValues = typeLibHarvester.HarvestRegistryValues(file.Source);

                                foreach (Wix.RegistryValue registryValue in registryValues)
                                {
                                    parentElement.AddChild(registryValue);
                                }
                            }
                            catch
                            {
                                // ignore all exceptions
                            }
                        }
                    }
                }
            }
        }
    }
}