//-------------------------------------------------------------------------------------------------
// <copyright file="FileHarvester.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Harvest WiX authoring for a file from the file system.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.IO;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// Harvest WiX authoring for a file from the file system.
    /// </summary>
    public sealed class FileHarvester : HarvesterExtension
    {
        /// <summary>
        /// Harvest a file.
        /// </summary>
        /// <param name="argument">The path of the file.</param>
        /// <returns>A harvested file.</returns>
        public override Wix.Fragment Harvest(string argument)
        {
            if (null == argument)
            {
                throw new ArgumentNullException("argument");
            }

            Wix.File file = this.HarvestFile(argument);

            Wix.Component component = new Wix.Component();
            component.AddChild(file);

            string directoryPath = Path.GetDirectoryName(Path.GetFullPath(argument));
            Wix.Directory directory = new Wix.Directory();
            directory.FileSource = directoryPath;
            directory.Name = Path.GetFileName(directoryPath);
            directory.AddChild(component);

            Wix.DirectoryRef directoryRef = new Wix.DirectoryRef();
            directoryRef.Id = "TARGETDIR";
            directoryRef.AddChild(directory);

            Wix.Fragment fragment = new Wix.Fragment();
            fragment.AddChild(directoryRef);

            return fragment;
        }

        /// <summary>
        /// Harvest a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>A harvested file.</returns>
        public Wix.File HarvestFile(string path)
        {
            if (null == path)
            {
                throw new ArgumentNullException("path");
            }

            Wix.File file = new Wix.File();

            // use absolute paths
            path = Path.GetFullPath(path);

            file.KeyPath = Wix.YesNoType.yes;
            file.Name = Path.GetFileName(path);
            file.Source = path;

            return file;
        }
    }
}
