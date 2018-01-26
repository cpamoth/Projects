//-------------------------------------------------------------------------------------------------
// <copyright file="TypeLibraryHarvester.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Harvest WiX authoring from a type library file.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// Harvest WiX authoring from a type library file.
    /// </summary>
    public sealed class TypeLibraryHarvester
    {
        /// <summary>
        /// Harvest the registry values written by RegisterTypeLib.
        /// </summary>
        /// <param name="path">The file to harvest registry values from.</param>
        /// <returns>The harvested registry values.</returns>
        public Wix.RegistryValue[] HarvestRegistryValues(string path)
        {
            using (RegistryHarvester registryHarvester = new RegistryHarvester(true))
            {
                NativeMethods.RegisterTypeLibrary(path);

                return registryHarvester.HarvestRegistry();
            }
        }

        /// <summary>
        /// Native methods for registering type libraries.
        /// </summary>
        private sealed class NativeMethods
        {
            /// <summary>
            /// Registers a type library.
            /// </summary>
            /// <param name="typeLibraryFile">The type library file to register.</param>
            internal static void RegisterTypeLibrary(string typeLibraryFile)
            {
                IntPtr ptlib;

                LoadTypeLib(typeLibraryFile, out ptlib);

                RegisterTypeLib(ptlib, typeLibraryFile, null);
            }

            /// <summary>
            /// Loads and registers a type library.
            /// </summary>
            /// <param name="szFile">Contains the name of the file from which LoadTypeLib should attempt to load a type library.</param>
            /// <param name="pptlib">On return, contains a pointer to a pointer to the loaded type library.</param>
            /// <remarks>LoadTypeLib will not register the type library if the path of the type library is specified.</remarks>
            [DllImport("oleaut32.dll", PreserveSig = false)]
            private static extern void LoadTypeLib([MarshalAs(UnmanagedType.BStr)] string szFile, out IntPtr pptlib);

            /// <summary>
            /// Adds information about a type library to the system registry.
            /// </summary>
            /// <param name="ptlib">Pointer to the type library being registered.</param>
            /// <param name="szFullPath">Fully qualified path specification for the type library being registered.</param>
            /// <param name="szHelpDir">Directory in which the Help file for the library being registered can be found. Can be Null.</param>
            [DllImport("oleaut32.dll", PreserveSig = false)]
            private static extern void RegisterTypeLib(IntPtr ptlib, [MarshalAs(UnmanagedType.BStr)] string szFullPath, [MarshalAs(UnmanagedType.BStr)] string szHelpDir);
        }
    }
}
