//-------------------------------------------------------------------------------------------------
// <copyright file="WixExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The main class for a WiX extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    /// The main class for a WiX extension.
    /// </summary>
    public class WixExtension
    {
        /// <summary>
        /// Gets the optional binder extension.
        /// </summary>
        /// <value>The optional binder extension.</value>
        public virtual BinderExtension BinderExtension
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the optional compiler extension.
        /// </summary>
        /// <value>The optional compiler extension.</value>
        public virtual CompilerExtension CompilerExtension
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the optional decompiler extension.
        /// </summary>
        /// <value>The optional decompiler extension.</value>
        public virtual DecompilerExtension DecompilerExtension
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the optional preprocessor extension.
        /// </summary>
        /// <value>The optional preprocessor extension.</value>
        public virtual PreprocessorExtension PreprocessorExtension
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the optional table definitions for this extension.
        /// </summary>
        /// <value>The optional table definitions for this extension.</value>
        public virtual TableDefinitionCollection TableDefinitions
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the optional validator extension.
        /// </summary>
        /// <value>The optional validator extension.</value>
        public virtual ValidatorExtension ValidatorExtension
        {
            get { return null; }
        }

        /// <summary>
        /// Loads a WixExtension from a type description string.
        /// </summary>
        /// <param name="extension">The extension type description string.</param>
        /// <returns>The loaded WixExtension.</returns>
        /// <remarks>
        /// <paramref name="extension"/> can be in several different forms:
        /// <list type="number">
        /// <item><term>AssemblyQualifiedName (TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089)</term></item>
        /// <item><term>AssemblyName (MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089)</term></item>
        /// <item><term>Absolute path to an assembly (C:\MyExtensions\ExtensionAssembly.dll)</term></item>
        /// <item><term>Relative path to an assembly (..\..\MyExtensions\ExtensionAssembly.dll)</term></item>
        /// </list>
        /// To specify a particular class to use, prefix the fully qualified class name to the assembly and separate them with a comma.
        /// For example: "TopNamespace.SubNameSpace.ContainingClass+NestedClass, C:\MyExtensions\ExtensionAssembly.dll"
        /// </remarks>
        public static WixExtension Load(string extension)
        {
            Type extensionType = null;
            int commaIndex = extension.IndexOf(',');
            string className = String.Empty;
            string assemblyName = extension;

            if (0 <= commaIndex)
            {
                className = extension.Substring(0, commaIndex);
                assemblyName = (extension.Length <= commaIndex + 1 ? String.Empty : extension.Substring(commaIndex + 1));
            }

            className = className.Trim();
            assemblyName = assemblyName.Trim();

            if (null == extensionType && 0 < assemblyName.Length)
            {
                try
                {
                    Assembly extensionAssembly;

                    // case 3: Absolute path to an assembly
                    if (Path.IsPathRooted(assemblyName))
                    {
                        extensionAssembly = Assembly.LoadFrom(assemblyName);
                    }
                    else
                    {
                        try
                        {
                            // case 2: AssemblyName
                            extensionAssembly = Assembly.Load(assemblyName);
                        }
                        catch (IOException e)
                        {
                            if (e is FileLoadException || e is FileNotFoundException)
                            {
                                // case 4: Relative path to an assembly

                                // we want to use Assembly.Load when we can because it has some benefits over Assembly.LoadFrom
                                // (see the documentation for Assembly.LoadFrom). However, it may fail when the path is a relative
                                // path, so we should try Assembly.LoadFrom one last time. We could have detected a directory
                                // separator character and used Assembly.LoadFrom directly, but dealing with path canonicalization
                                // issues is something we don't want to deal with if we don't have to.
                                extensionAssembly = Assembly.LoadFrom(assemblyName);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    if (0 < className.Length)
                    {
                        // case 1: AssemblyQualifiedName
                        extensionType = extensionAssembly.GetType(className, false /* throwOnError */, true /* ignoreCase */);
                    }
                    else
                    {
                        // if no class name was specified, then let's hope the assembly defined a default WixExtension
                        AssemblyDefaultWixExtensionAttribute extensionAttribute = (AssemblyDefaultWixExtensionAttribute)Attribute.GetCustomAttribute(extensionAssembly, typeof(AssemblyDefaultWixExtensionAttribute));

                        if (null != extensionAttribute)
                        {
                            extensionType = extensionAttribute.ExtensionType;
                        }
                    }
                }
                catch
                {
                    throw new WixException(WixErrors.InvalidExtension(extension));
                }
            }

            if (null == extensionType)
            {
                throw new WixException(WixErrors.InvalidExtension(extension));
            }

            if (extensionType.IsSubclassOf(typeof(WixExtension)))
            {
                return Activator.CreateInstance(extensionType) as WixExtension;
            }
            else
            {
                throw new WixException(WixErrors.InvalidExtension(extension, extensionType.ToString(), typeof(WixExtension).ToString()));
            }
        }

        /// <summary>
        /// Gets the library associated with this extension.
        /// </summary>
        /// <param name="tableDefinitions">The table definitions to use while loading the library.</param>
        /// <returns>The library for this extension.</returns>
        public virtual Library GetLibrary(TableDefinitionCollection tableDefinitions)
        {
            return null;
        }

        /// <summary>
        /// Help for loading a library from an embedded resource.
        /// </summary>
        /// <param name="assembly">The assembly containing the embedded resource.</param>
        /// <param name="resourceName">The name of the embedded resource being requested.</param>
        /// <param name="tableDefinitions">The table definitions to use while loading the library.</param>
        /// <returns>The loaded library.</returns>
        protected static Library LoadLibraryHelper(Assembly assembly, string resourceName, TableDefinitionCollection tableDefinitions)
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "embeddedresource";
                uriBuilder.Path = assembly.Location;
                uriBuilder.Fragment = resourceName;

                return Library.Load(resourceStream, uriBuilder.Uri, tableDefinitions, false, true);
            }
        }

        /// <summary>
        /// Helper for loading table definitions from an embedded resource.
        /// </summary>
        /// <param name="assembly">The assembly containing the embedded resource.</param>
        /// <param name="resourceName">The name of the embedded resource being requested.</param>
        /// <returns>The loaded table definitions.</returns>
        protected static TableDefinitionCollection LoadTableDefinitionHelper(Assembly assembly, string resourceName)
        {
            XmlReader reader = null;

            try
            {
                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    reader = new XmlTextReader(resourceStream);

                    return TableDefinitionCollection.Load(reader, false);
                }
            }
            finally
            {
                if (null != reader)
                {
                    reader.Close();
                }
            }
        }
    }
}