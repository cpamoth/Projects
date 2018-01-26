//-------------------------------------------------------------------------------------------------
// <copyright file="Common.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Common Wix utility methods and types.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Common Wix utility methods and types.
    /// </summary>
    internal sealed class Common
    {
        internal static readonly string[] FolderPermissions = { "Read", "CreateFile", "CreateChild", "ReadExtendedAttributes", "WriteExtendedAttributes", "Traverse", "DeleteChild", "ReadAttributes", "WriteAttributes" };
        internal static readonly string[] StandardPermissions = { "Delete", "ReadPermission", "ChangePermission", "TakeOwnership", "Synchronize" };
        internal static readonly string[] RegistryPermissions = { "Read", "Write", "CreateSubkeys", "EnumerateSubkeys", "Notify", "CreateLink" };
        internal static readonly string[] FilePermissions = { "Read", "Write", "Append", "ReadExtendedAttributes", "WriteExtendedAttributes", "Execute", null, "ReadAttributes", "WriteAttributes" };
        internal static readonly string[] GenericPermissions = { "GenericAll", "GenericExecute", "GenericWrite", "GenericRead" };

        internal static readonly Regex WixVariableRegex = new Regex(@"(\!|\$)\((?<namespace>loc|wix)\.(?<name>[_A-Za-z][0-9A-Za-z_]+)(\=(?<value>.+?))?\)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Protect the constructor.
        /// </summary>
        private Common()
        {
        }

        /// <summary>
        /// Cleans up the temp files.
        /// </summary>
        /// <param name="path">The temporary directory to delete.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <returns>True if all files were deleted, false otherwise.</returns>
        internal static bool DeleteTempFiles(string path, IMessageHandler messageHandler)
        {
            // try three times and give up with a warning if the temp files aren't gone by then
            int retryLimit = 3;

            for (int i = 0; i < retryLimit; i++)
            {
                try
                {
                    Directory.Delete(path, true);   // toast the whole temp directory
                    break; // no exception means we got success the first time
                }
                catch (UnauthorizedAccessException)
                {
                    if (0 == i) // should only need to unmark readonly once - there's no point in doing it again and again
                    {
                        RecursiveFileAttributes(path, FileAttributes.ReadOnly, false); // toasting will fail if any files are read-only. Try changing them to not be.
                    }
                    else
                    {
                        messageHandler.OnMessage(WixWarnings.AccessDeniedForDeletion(null, path));
                        return false;
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // if the path doesn't exist, then there is nothing for us to worry about
                    break;
                }
                catch (IOException) // directory in use
                {
                    if (i == (retryLimit - 1)) // last try failed still, give up
                    {
                        messageHandler.OnMessage(WixWarnings.DirectoryInUse(null, path));
                        return false;
                    }
                    System.Threading.Thread.Sleep(300);  // sleep a bit before trying again
                }
            }

            return true;
        }

        /// <summary>
        /// Get the value of an attribute with type YesNoType.
        /// </summary>
        /// <param name="sourceLineNumbers">Source information for the value.</param>
        /// <param name="elementName">Name of the element for this attribute, used for a possible exception.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="value">Value to process.</param>
        /// <returns>Returns true for a value of 'yes' and false for a value of 'no'.</returns>
        /// <exception cref="WixException">Thrown when the attribute's value is not 'yes' or 'no'.</exception>
        internal static bool IsYes(SourceLineNumberCollection sourceLineNumbers, string elementName, string attributeName, string value)
        {
            switch (value)
            {
                case "no":
                    return false;
                case "yes":
                    return true;
                default:
                    throw new WixException(WixErrors.IllegalAttributeValue(sourceLineNumbers, elementName, attributeName, value, "no", "yes"));
            }
        }

        /// <summary>
        /// Recursively loops through a directory, changing an attribute on all of the underlying files.
        /// An example is to add/remove the ReadOnly flag from each file.
        /// </summary>
        /// <param name="path">The directory path to start deleting from.</param>
        /// <param name="fileAttribute">The FileAttribute to change on each file.</param>
        /// <param name="markAttribute">If true, add the attribute to each file. If false, remove it.</param>
        private static void RecursiveFileAttributes(string path, FileAttributes fileAttribute, bool markAttribute)
        {
            foreach (string subDirectory in Directory.GetDirectories(path))
            {
                RecursiveFileAttributes(subDirectory, fileAttribute, markAttribute);
            }

            foreach (string filePath in Directory.GetFiles(path))
            {
                FileAttributes attributes = File.GetAttributes(filePath);
                if (markAttribute)
                {
                    attributes = attributes | fileAttribute; // add to list of attributes
                }
                else if (fileAttribute == (attributes & fileAttribute)) // if attribute set
                {
                    attributes = attributes ^ fileAttribute; // remove from list of attributes
                }
                File.SetAttributes(filePath, attributes);
            }
        }
    }
}
