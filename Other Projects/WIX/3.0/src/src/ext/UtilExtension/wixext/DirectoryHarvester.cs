//-------------------------------------------------------------------------------------------------
// <copyright file="DirectoryHarvester.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Harvest WiX authoring for a directory from the file system.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.IO;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// Harvest WiX authoring for a directory from the file system.
    /// </summary>
    public sealed class DirectoryHarvester : HarvesterExtension
    {
        private FileHarvester fileHarvester;
        private bool keepEmptyDirectories;

        /// <summary>
        /// Instantiate a new FileSystemHarvester.
        /// </summary>
        public DirectoryHarvester()
        {
            this.fileHarvester = new FileHarvester();
            this.keepEmptyDirectories = false;
        }

        /// <summary>
        /// Gets or sets the option to keep empty directories.
        /// </summary>
        /// <value>The option to keep empty directories.</value>
        public bool KeepEmptyDirectories
        {
            get { return this.keepEmptyDirectories; }
            set { this.keepEmptyDirectories = value; }
        }

        /// <summary>
        /// Harvest a directory.
        /// </summary>
        /// <param name="argument">The path of the directory.</param>
        /// <returns>The harvested directory.</returns>
        public override Wix.Fragment Harvest(string argument)
        {
            if (null == argument)
            {
                throw new ArgumentNullException("argument");
            }

            Wix.Directory directory = this.HarvestDirectory(argument, true);

            Wix.Fragment fragment = new Wix.Fragment();
            fragment.AddChild(directory);

            return fragment;
        }

        /// <summary>
        /// Harvest a directory.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="harvestChildren">The option to harvest child directories and files.</param>
        /// <returns>The harvested directory.</returns>
        public Wix.Directory HarvestDirectory(string path, bool harvestChildren)
        {
            if (null == path)
            {
                throw new ArgumentNullException("path");
            }

            Wix.Directory directory = new Wix.Directory();

            // use absolute paths
            path = Path.GetFullPath(path);

            directory.FileSource = path;
            directory.Name = Path.GetFileName(path);

            if (harvestChildren)
            {
                try
                {
                    int fileCount = this.HarvestDirectory(path, directory);

                    // its an error to not harvest anything with the option to keep empty directories off
                    if (0 == fileCount && !this.keepEmptyDirectories)
                    {
                        throw new WixException(UtilErrors.EmptyDirectory(path));
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    throw new WixException(UtilErrors.DirectoryNotFound(path));
                }
            }

            return directory;
        }

        /// <summary>
        /// Harvest a directory.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="directory">The directory for this path.</param>
        /// <returns>The number of files harvested.</returns>
        private int HarvestDirectory(string path, Wix.Directory directory)
        {
            int fileCount = 0;

            // harvest the child directories
            foreach (string childDirectoryPath in Directory.GetDirectories(path))
            {
                Wix.Directory childDirectory = new Wix.Directory();

                childDirectory.FileSource = childDirectoryPath;
                childDirectory.Name = Path.GetFileName(childDirectoryPath);

                int childFileCount = this.HarvestDirectory(childDirectoryPath, childDirectory);

                // keep the directory if it contained any files (or empty directories are being kept)
                if (0 < childFileCount || this.keepEmptyDirectories)
                {
                    directory.AddChild(childDirectory);
                }

                fileCount += childFileCount;
            }

            // harvest the files
            string[] files = Directory.GetFiles(path);
            if (0 < files.Length)
            {
                foreach (string filePath in Directory.GetFiles(path))
                {
                    Wix.Component component = new Wix.Component();

                    Wix.File file = this.fileHarvester.HarvestFile(filePath);
                    component.AddChild(file);

                    directory.AddChild(component);
                }
            }
            else if (0 == fileCount && this.keepEmptyDirectories)
            {
                Wix.Component component = new Wix.Component();
                component.KeyPath = Wix.YesNoType.yes;

                Wix.CreateFolder createFolder = new Wix.CreateFolder();
                component.AddChild(createFolder);

                directory.AddChild(component);
            }

            return fileCount + files.Length;
        }
    }
}
