//-------------------------------------------------------------------------------------------------
// <copyright file="BinderExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base binder extension.  Any of these methods can be overridden to change
// the behavior of the binder.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;

    /// <summary>
    /// Options for building the cabinet.
    /// </summary>
    public enum CabinetBuildOption
    {
        /// <summary>
        /// Build the cabinet and move it to the target location.
        /// </summary>
        BuildAndMove,

        /// <summary>
        /// Build the cabinet and copy it to the target location.
        /// </summary>
        BuildAndCopy,

        /// <summary>
        /// Just copy the cabinet to the target location.
        /// </summary>
        Copy
    }

    /// <summary>
    /// Base class for creating a binder extension.
    /// </summary>
    public class BinderExtension
    {
        private StringCollection basePaths;
        private string cabCachePath;
        private Output output;
        private bool reuseCabinets;
        private StringCollection sourcePaths;
        private SubStorage activeSubstorage = null;

        /// <summary>
        /// Instantiate a new BinderExtension.
        /// </summary>
        public BinderExtension()
        {
            this.basePaths = new StringCollection();
            this.sourcePaths = new StringCollection();
        }

        /// <summary>
        /// Gets the base paths to locate files.
        /// </summary>
        /// <value>The base paths to locate files.</value>
        public StringCollection BasePaths
        {
            get { return this.basePaths; }
        }

        /// <summary>
        /// Gets or sets the path to cabinet cache.
        /// </summary>
        /// <value>The path to cabinet cache.</value>
        public string CabCachePath
        {
            get { return this.cabCachePath; }
            set { this.cabCachePath = value; }
        }

        /// <summary>
        /// Gets or sets the output object used for binding.
        /// </summary>
        /// <value>The output object.</value>
        public Output Output
        {
            get { return this.output; }
            set { this.output = value; }
        }

        /// <summary>
        /// Gets or sets the option to reuse cabinets in the cache.
        /// </summary>
        /// <value>The option to reuse cabinets in the cache.</value>
        public bool ReuseCabinets
        {
            get { return this.reuseCabinets; }
            set { this.reuseCabinets = value; }
        }

        /// <summary>
        /// Gets the collection of all source paths to intermediate files.
        /// </summary>
        /// <value>The collection of all source paths to intermediate files.</value>
        public StringCollection SourcePaths
        {
            get { return this.sourcePaths; }
        }

        /// <summary>
        /// Gets or sets the active subStorage used for binding.
        /// </summary>
        /// <value>The subStorage object.</value>
        public SubStorage ActiveSubStorage
        {
            get { return this.activeSubstorage; }
            set { this.activeSubstorage = value; }
        }

        /// <summary>
        /// Compares two files to determine if they are equivalent.
        /// </summary>
        /// <param name="targetFile">The target file.</param>
        /// <param name="updatedFile">The updated file.</param>
        /// <returns>true if the files are equal; false otherwise.</returns>
        public virtual bool CompareFiles(string targetFile, string updatedFile)
        {
            using (FileStream targetStream = File.OpenRead(targetFile))
            {
                using (FileStream updatedStream = File.OpenRead(updatedFile))
                {
                    if (targetStream.Length != updatedStream.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < targetStream.Length; i++)
                    {
                        if (targetStream.ReadByte() != updatedStream.ReadByte())
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Resolves the source path of a file.
        /// </summary>
        /// <param name="source">Original source value.</param>
        /// <returns>Should return a valid path for the stream to be imported.</returns>
        public virtual string ResolveFile(string source)
        {
            string filePath = null;

            if (source.StartsWith("SourceDir\\") || source.StartsWith("SourceDir/"))
            {
                foreach (string basePath in this.basePaths)
                {
                    filePath = Path.Combine(basePath, source.Substring(10));
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }
            }

            if (Path.IsPathRooted(source) || File.Exists(source))
            {
                return source;
            }

            foreach (string path in this.sourcePaths)
            {
                filePath = Path.Combine(path, source);
                if (File.Exists(filePath))
                {
                    return filePath;
                }

                if (source.StartsWith("SourceDir\\") || source.StartsWith("SourceDir/"))
                {
                    filePath = Path.Combine(path, source.Substring(10));
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }
            }

            throw new WixFileNotFoundException(source);
        }

        /// <summary>
        /// Resolves the source path of a cabinet file.
        /// </summary>
        /// <param name="fileRows">Collection of files in this cabinet.</param>
        /// <param name="cabinetPath">Path to cabinet to generate.  Path may be modified by delegate.</param>
        /// <returns>The CabinetBuildOption.  By default the cabinet is built and moved to its target location.</returns>
        public virtual CabinetBuildOption ResolveCabinet(FileRowCollection fileRows, ref string cabinetPath)
        {
            // no special behavior specified, use the default
            if (null == this.cabCachePath && !this.reuseCabinets)
            {
                return CabinetBuildOption.BuildAndMove;
            }

            // if a cabinet cache path was provided, change the location for the cabinet
            // to be built to
            if (null != this.cabCachePath)
            {
                string cabinetName = Path.GetFileName(cabinetPath);
                cabinetPath = Path.Combine(this.cabCachePath, cabinetName);
            }

            // if we still think we're going to reuse the cabinet check to see if the cabinet exists first
            if (this.reuseCabinets)
            {
                bool cabinetExists = false;

                if (File.Exists(cabinetPath))
                {
                    // check to see if
                    // 1. any files are added or removed
                    // 2. order of files changed or names changed
                    // 3. modified time changed
                    cabinetExists = true;

                    Cab.WixEnumerateCab wixEnumerateCab = new Cab.WixEnumerateCab();
                    ArrayList fileList = wixEnumerateCab.Enumerate(cabinetPath);

                    if (fileRows.Count != fileList.Count)
                    {
                        cabinetExists = false;
                    }
                    else
                    {
                        int i = 0;
                        foreach (FileRow fileRow in fileRows)
                        {
                            CabinetFileInfo fileInfo = fileList[i] as CabinetFileInfo;
                            DateTime fileTime = File.GetLastWriteTime(fileRow.Source);

                            ushort cabDate;
                            ushort cabTime;
                            Cab.Interop.CabInterop.DateTimeToCabDateAndTime(fileTime, out cabDate, out cabTime);
                            if (fileRow.File != fileInfo.FileId || fileInfo.Date != cabDate || fileInfo.Time != cabTime)
                            {
                                cabinetExists = false;
                                break;
                            }
                            i++;
                        }
                    }
                }

                return (cabinetExists ? CabinetBuildOption.Copy : CabinetBuildOption.BuildAndCopy);
            }
            else // by default move the built cabinet
            {
                return CabinetBuildOption.BuildAndMove;
            }
        }

        /// <summary>
        /// Resolve the layout path of a media.
        /// </summary>
        /// <param name="mediaRow">The media's row.</param>
        /// <param name="layoutDirectory">The layout directory for the setup image.</param>
        /// <returns>The layout path for the media.</returns>
        public virtual string ResolveMedia(MediaRow mediaRow, string layoutDirectory)
        {
            string mediaLayoutDirectory = mediaRow.Layout;

            if (null == mediaLayoutDirectory)
            {
                mediaLayoutDirectory = layoutDirectory;
            }
            else if (!Path.IsPathRooted(mediaLayoutDirectory))
            {
                mediaLayoutDirectory = Path.Combine(layoutDirectory, mediaLayoutDirectory);
            }

            return mediaLayoutDirectory;
        }

        /// <summary>
        /// Copies a file.
        /// </summary>
        /// <param name="source">The file to copy.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        public virtual void CopyFile(string source, string destination, bool overwrite)
        {
            File.Copy(source, destination, overwrite);
        }

        /// <summary>
        /// Moves a file.
        /// </summary>
        /// <param name="source">The file to move.</param>
        /// <param name="destination">The destination file.</param>
        public virtual void MoveFile(string source, string destination)
        {
            File.Move(source, destination);
        }
    }
}