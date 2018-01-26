//-------------------------------------------------------------------------------------------------
// <copyright file="MsiHandle.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Wrapper for MSI API handles.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Msi
{
    using System;
    using System.ComponentModel;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Wrapper class for MSI handle.
    /// </summary>
    public class MsiHandle : IDisposable
    {
        private bool disposed;
        private uint handle;

        /// <summary>
        /// MSI handle destructor.
        /// </summary>
        ~MsiHandle()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the MSI handle.
        /// </summary>
        /// <value>The MSI handle.</value>
        internal uint Handle
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("MsiHandle");
                }

                return this.handle;
            }

            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("MsiHandle");
                }

                this.handle = value;
            }
        }

        /// <summary>
        /// Close the MSI handle.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes the managed and unmanaged objects in this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the managed and unmanaged objects in this object.
        /// </summary>
        /// <param name="disposing">true to dispose the managed objects.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                int error = MsiInterop.MsiCloseHandle(this.handle);
                if (0 != error)
                {
                    throw new Win32Exception(error);
                }
                this.handle = 0;

                this.disposed = true;
            }
        }
    }
}
