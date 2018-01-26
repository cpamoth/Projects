//-------------------------------------------------------------------------------------------------
// <copyright file="AppCommon.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Common utilities for Wix applications.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Common utilities for Wix applications.
    /// </summary>
    public sealed class AppCommon
    {
        /// <summary>
        /// Protect the constructor.
        /// </summary>
        private AppCommon()
        {
        }

        /// <summary>
        /// Get a set of files that possibly have a search pattern in the path (such as '*').
        /// </summary>
        /// <param name="searchPath">Search path to find files in.</param>
        /// <param name="fileType">Type of file; typically "Source".</param>
        /// <returns>An array of files matching the search path.</returns>
        /// <remarks>
        /// This method is written in this verbose way because it needs to support ".." in the path.
        /// It needs the directory path isolated from the file name in order to use Directory.GetFiles
        /// or DirectoryInfo.GetFiles.  The only way to get this directory path is manually since
        /// Path.GetDirectoryName does not support ".." in the path.
        /// </remarks>
        /// <exception cref="WixFileNotFoundException">Throws WixFileNotFoundException if no file matching the pattern can be found.</exception>
        public static string[] GetFiles(string searchPath, string fileType)
        {
            if (null == searchPath)
            {
                throw new ArgumentNullException("searchPath");
            }

            // convert alternate directory separators to the standard one
            string filePath = searchPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            int lastSeparator = filePath.LastIndexOf(Path.DirectorySeparatorChar);
            string[] files = null;

            try
            {
                if (0 > lastSeparator)
                {
                    files = Directory.GetFiles(".", filePath);
                }
                else // found directory separator
                {
                    files = Directory.GetFiles(filePath.Substring(0, lastSeparator + 1), filePath.Substring(lastSeparator + 1));
                }
            }
            catch (DirectoryNotFoundException)
            {
                // don't let this function throw the DirectoryNotFoundException. (this exception
                // occurs for non-existant directories and invalid characters in the searchPattern)
            }
            catch (IOException)
            {
                throw new WixFileNotFoundException(searchPath, fileType);
            }

            // file could not be found or path is invalid in some way
            if (null == files || 0 == files.Length)
            {
                throw new WixFileNotFoundException(searchPath, fileType);
            }

            return files;
        }

        /// <summary>
        /// Read the configuration file (*.exe.config).
        /// </summary>
        /// <param name="extensions">Extensions to load.</param>
        public static void ReadConfiguration(StringCollection extensions)
        {
            // find extensions
            try
            {
                AppSettingsReader appSettingsReader = new AppSettingsReader();
                string extensionTypes = (string)appSettingsReader.GetValue("extensions", typeof(string));
                foreach (string extensionType in extensionTypes.Split(";".ToCharArray()))
                {
                    extensions.Add(extensionType);
                }
            }
            catch (InvalidOperationException)
            {
                // This exception is thrown if there is no extensions key in the appSettings configuration section.
            }
        }

        /// <summary>
        /// Parse a response file.
        /// </summary>
        /// <param name="responseFile">The file to parse.</param>
        /// <returns>The array of arguments.</returns>
        public static string[] ParseResponseFile(string responseFile)
        {
            ArrayList newArgs = new ArrayList();

            using (StreamReader reader = new StreamReader(responseFile))
            {
                string line;

                while (null != (line = reader.ReadLine()))
                {
                    StringBuilder newArg = new StringBuilder();
                    bool betweenQuotes = false;
                    for (int j = 0; j < line.Length; ++j)
                    {
                        // skip whitespace
                        if (!betweenQuotes && (' ' == line[j] || '\t' == line[j]))
                        {
                            if (0 != newArg.Length)
                            {
                                newArgs.Add(newArg.ToString());
                                newArg = new StringBuilder();
                            }

                            continue;
                        }

                        // if we're escaping a quote
                        if ('\\' == line[j] && '"' == line[j])
                        {
                            ++j;
                        }
                        else if ('"' == line[j])   // if we've hit a new quote
                        {
                            betweenQuotes = !betweenQuotes;
                            continue;
                        }

                        newArg.Append(line[j]);
                    }

                    if (0 != newArg.Length)
                    {
                        newArgs.Add(newArg.ToString());
                    }
                }
            }

            return (string[])newArgs.ToArray(typeof(string));
        }
    }
}
