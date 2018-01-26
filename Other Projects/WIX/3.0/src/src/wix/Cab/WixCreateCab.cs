//-------------------------------------------------------------------------------------------------
// <copyright file="WixCreateCab.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Wrapper class around interop with wixcab.dll to compress files into a cabinet.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Cab
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Tools.WindowsInstallerXml.Cab.Interop;

    /// <summary>
    /// Compression level to use when creating cabinet.
    /// </summary>
    public enum CompressionLevel
    {
        /// <summary>Use no compression.</summary>
        None,

        /// <summary>Use low compression.</summary>
        Low,

        /// <summary>Use medium compression.</summary>
        Medium,

        /// <summary>Use high compression.</summary>
        High,

        /// <summary>Use ms-zip compression.</summary>
        Mszip
    }

    /// <summary>
    /// Wrapper class around interop with wixcab.dll to compress files into a cabinet.
    /// </summary>
    public sealed class WixCreateCab : IDisposable
    {
        private IntPtr handle = IntPtr.Zero;
        private bool disposed;

        /// <summary>
        /// Creates a cabinet.
        /// </summary>
        /// <param name="cabName">Name of cabinet to create.</param>
        /// <param name="cabDir">Directory to create cabinet in.</param>
        /// <param name="maxSize">Maximum size of cabinet.</param>
        /// <param name="maxThresh">Maximum threshold for each cabinet.</param>
        /// <param name="compressionLevel">Level of compression to apply.</param>
        public WixCreateCab(string cabName, string cabDir, int maxSize, int maxThresh, CompressionLevel compressionLevel)
        {
            CabInterop.CreateCabBegin(cabName, cabDir, (uint)maxSize, (uint)maxThresh, (uint)compressionLevel, out this.handle);
        }

        /// <summary>
        /// Destructor for cabinet creation.
        /// </summary>
        ~WixCreateCab()
        {
            this.Dispose();
        }

        /// <summary>
        /// Adds a file to the cabinet.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="token">The token for the file.</param>
        public void AddFile(string file, string token)
        {
            try
            {
                CabInterop.CreateCabAddFile(file, token, this.handle);
            }
            catch (DirectoryNotFoundException)
            {
                throw new FileNotFoundException("The system cannot find the file specified.", file);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("The system cannot find the file specified.", file);
            }
        }

        /// <summary>
        /// Disposes the managed and unmanaged objects in this object.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                CabInterop.CreateCabFinish(this.handle);
                this.handle = IntPtr.Zero;

                GC.SuppressFinalize(this);
                this.disposed = true;
            }
        }
    }
}
