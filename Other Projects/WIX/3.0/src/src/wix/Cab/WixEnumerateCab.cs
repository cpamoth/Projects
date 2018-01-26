//-------------------------------------------------------------------------------------------------
// <copyright file="WixEnumerateCab.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Wrapper class around interop with wixcab.dll to enumerate files from a cabinet.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Cab
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using Microsoft.Tools.WindowsInstallerXml.Cab.Interop;
    using Handle = System.Int32;

    /// <summary>
    /// Wrapper class around interop with wixcab.dll to enumerate files from a cabinet.
    /// </summary>
    public sealed class WixEnumerateCab : IDisposable
    {
        private bool disposed;
        private ArrayList fileInfoList;

        /// <summary>
        /// Creates a cabinet enumerator.
        /// </summary>
        public WixEnumerateCab()
        {
            this.fileInfoList = new ArrayList();
            CabInterop.EnumerateCabBegin();
        }

        /// <summary>
        /// Destructor for cabinet enumeration.
        /// </summary>
        ~WixEnumerateCab()
        {
            this.Dispose();
        }

        /// <summary>
        /// Enumerates all files in a cabinet.
        /// </summary>
        /// <param name="cabinetFile">path to cabinet</param>
        /// <returns>list of CabinetFileInfo</returns>
        public ArrayList Enumerate(string cabinetFile)
        {
            this.fileInfoList.Clear(); // we need to clear the list before starting new one

            // the callback (this.Notify) will populate the list for each file in cabinet
            CabInterop.EnumerateCab(cabinetFile, new CabInterop.PFNNOTIFY(this.Notify));

            return this.fileInfoList;
        }

        /// <summary>
        /// Disposes the managed and unmanaged objects in this object.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                CabInterop.EnumerateCabFinish();

                GC.SuppressFinalize(this);
                this.disposed = true;
            }
        }

        /// <summary>
        /// Delegate that's called for every file in cabinet.
        /// </summary>
        /// <param name="fdint">NOTIFICATIONTYPE</param>
        /// <param name="pfdin">NOTIFICATION</param>
        /// <returns>System.Int32</returns>
        internal Handle Notify(CabInterop.NOTIFICATIONTYPE fdint, CabInterop.NOTIFICATION pfdin)
        {
            if (fdint == CabInterop.NOTIFICATIONTYPE.COPY_FILE)
            {
                CabinetFileInfo fileInfo = new CabinetFileInfo(pfdin.Psz1, pfdin.Cb, pfdin.Date, pfdin.Time);
                this.fileInfoList.Add(fileInfo);
            }
            return 0; // tell cabinet api to skip this file
        }
    }
}
