//-------------------------------------------------------------------------------------------------
// <copyright file="FileSystemBase.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Wrapper around interop for Win32 file system calls.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.FileSystem
{
    using System;
    using System.Text;
    using Microsoft.Tools.WindowsInstallerXml.FileSystem.Interop;

    /// <summary>
    /// Wrapper around interop for Win32 file system calls.
    /// </summary>
    internal sealed class FileSystemBase
    {
        /// <summary>
        /// Cannot instantiate this class.
        /// </summary>
        private FileSystemBase()
        {
        }

        /// <summary>
        /// Gets the short name for a file.
        /// </summary>
        /// <param name="fullPath">Fullpath to file on disk.</param>
        /// <returns>Short name for file.</returns>
        public static string GetShortPathName(string fullPath)
        {
            StringBuilder shortPath = new StringBuilder(FileSystemInterop.MaxPath, FileSystemInterop.MaxPath);

            uint result = FileSystemInterop.GetShortPathName(fullPath, shortPath, FileSystemInterop.MaxPath);
            
            if (0 == result)
            {
                int err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new System.Runtime.InteropServices.COMException("Failed to get short path name", err);
            }

            return shortPath.ToString();
        }

        /// <summary>
        /// Gets the long name for a file.
        /// </summary>
        /// <param name="fullPath">Fullpath to file on disk.</param>
        /// <returns>Short name for file.</returns>
        public static string GetLongPathName(string fullPath)
        {
            StringBuilder longPath = new StringBuilder(FileSystemInterop.MaxPath, FileSystemInterop.MaxPath);

            uint result = FileSystemInterop.GetLongPathName(fullPath, longPath, FileSystemInterop.MaxPath);
            
            if (0 == result)
            {
                int err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new System.Runtime.InteropServices.COMException("Failed to get long path name", err);
            }

            return longPath.ToString();
        }
    }
}
