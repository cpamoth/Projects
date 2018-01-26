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
    using System.Collections.Generic;
    using System.IO;
    using System.Diagnostics;
    using InfIntrepretor;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The Windows Installer XML Toolset harvester mutator.
    /// </summary>
    public sealed class InternetComponentDownloadHarvesterMutator  : MutatorExtension
    {

        private String mainBinFilePath;
        private String mainComponentGuid;

        internal String MainBinFilePath
        {
            get { return this.mainBinFilePath; }
        }

        internal String MainComponentGuid
        {
            get { return this.mainComponentGuid; }
        }

        /// <summary>
        /// Gets the sequence of this mutator extension.
        /// </summary>
        /// <value>The sequence of this mutator extension.</value>
        public override int Sequence
        {
            get { return 108; }
        }

        /// <summary>
        /// Mutate a WiX document.
        /// </summary>
        /// <param name="wix">The Wix document element.</param>
        public override void Mutate(Wix.Wix wix)
        {
            //Remove the inf file from the package
            Wix.Component InfComponent = this.GetComponentWithFileFromWix(null, wix, ".inf", true);
            
            String InfPath = GetFilePathFromComponent(InfComponent, ".inf", true);

            if (InfPath != null)
            {
                ((Wix.IParentElement)((Wix.ISchemaElement)InfComponent).ParentElement).RemoveChild(InfComponent);
            }


            //Process the inf for the package
            if (InfPath != null)
            {
                InfParser infp = new InfIntrepretor.InfParser( InfPath );
                infp.ParseIntoSections();

                infp.InterpretInfToOpTree();
                
                ProcessInfFile( infp.InfFiles, wix );

                Wix.Component BinComponent = this.GetComponentWithFileFromWix(null, wix, infp.MainBinFileName, false);

                this.mainBinFilePath = GetFilePathFromComponent(BinComponent, infp.MainBinFileName, false);

            }
            else
            {
                //go through and register every file
                //BUGBUGAddRegistrationInformation(wix);
            }


        }

        private void ProcessInfFile(List<InfFileInstructions> InfFiles, Wix.Wix wix)
        {
            foreach (InfFileInstructions ifi in InfFiles)
            {

                Wix.Component FileComponent = this.GetComponentWithFileFromWix(null, wix, ifi.FileName, false);

                if (null == FileComponent)
                {
                    Console.WriteLine("Exception: this file wasn't found in the current component...");
             
                }

                if (mainComponentGuid == null)
                {
                    mainComponentGuid = Guid.NewGuid().ToString("B").ToUpper();
                    FileComponent.Guid = mainComponentGuid;
                }
                else
                {
                    FileComponent.Guid = Guid.NewGuid().ToString("B").ToUpper();
                }
            

                String FilePath = GetFilePathFromComponent(FileComponent, ifi.FileName, false);

                if (ifi.RegisterServer == "yes")
                {
                    DllHarvester dh = new DllHarvester();

                    Wix.RegistryValue[] axregvals = dh.HarvestRegistryValues(FilePath);

                    foreach (Wix.RegistryValue r in axregvals)
                    {
                        FileComponent.AddChild(r);
                    }

                }

            }
        }
 



        /// <summary>
        /// Mutate an element.
        /// </summary>
        /// <param name="parentElement">The parent of the element to mutate.</param>
        /// <param name="element">The element to mutate.</param>
        private Wix.Component GetComponentWithFileFromWix(Wix.IParentElement parentElement, Wix.ISchemaElement element, String FileName, bool isExtension)
        {
            String InfFilePath = null;

            if (element is Wix.Component)
            {
                if ((InfFilePath = this.GetFilePathFromComponent((Wix.Component)element, FileName, isExtension )) != null)
                { 
                    //we return once we see an inf file
                    return (Wix.Component)element;
                }
            }


            if (element is Wix.IParentElement)
            {
                ArrayList childElements = new ArrayList();

                foreach (Wix.ISchemaElement childElement in ((Wix.IParentElement)element).Children)
                {
                    childElements.Add(childElement);
                }

                foreach (Wix.ISchemaElement childElement in childElements)
                {
                    Wix.Component comp = GetComponentWithFileFromWix((Wix.IParentElement)element, childElement, FileName, isExtension );

                    if (comp != null)
                    {
                        return comp;
                    }
                }
            }

            return null;

        }

        
        /// <summary>
        /// Mutate a file.
        /// </summary>
        /// <param name="parentElement">The parent of the element to mutate.</param>
        /// <param name="file">The file to mutate.</param>
        private string GetFilePathFromComponent( Wix.Component element, String filename, bool isFileExtension )
        {
            ArrayList childElements = new ArrayList();

            // copy the child elements to a temporary array (to allow them to be deleted/moved)
            foreach (Wix.ISchemaElement childElement in ((Wix.IParentElement)element).Children)
            {
                childElements.Add(childElement);
            }

            foreach (Wix.ISchemaElement childElement in childElements)
            {
                if (childElement is Wix.File)
                {

                    if (isFileExtension)
                    {                    
                        if (0 == String.Compare(filename, Path.GetExtension(((Wix.File)childElement).Source), true))
                        {
                            return ((Wix.File)childElement).Source;
                        }
                    }
                    else
                    {
                        if (0 == String.Compare(filename, Path.GetFileName(((Wix.File)childElement).Source), true))
                        {
                            return ((Wix.File)childElement).Source;
                        }
                    }
       
                }
            }

            return null;
        }


   }
}