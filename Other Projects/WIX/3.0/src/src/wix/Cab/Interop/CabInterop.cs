//-------------------------------------------------------------------------------------------------
// <copyright file="CabInterop.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Interop class for the winterop.dll.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Cab.Interop
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Interop class for the winterop.dll.
    /// </summary>
    internal sealed class CabInterop
    {
        /// <summary>
        /// Private constructor since you can't create this object.
        /// </summary>
        private CabInterop()
        {
        }

        /// <summary>
        /// Delegate type that's called by cabinet api for every file in cabinet.
        /// </summary>
        /// <param name="fdint">NOTIFICATIONTYPE</param>
        /// <param name="pfdin">NOTIFICATION</param>
        /// <returns>0 for success, -1 otherwise</returns>
        public delegate Int32 PFNNOTIFY(NOTIFICATIONTYPE fdint, NOTIFICATION pfdin);

        /// <summary>
        /// Wraps FDINOTIFICATIONTYPE.
        /// </summary>
        public enum NOTIFICATIONTYPE : int
        {
            /// <summary>Info about the cabinet.</summary>
            CABINET_INFO,
            /// <summary>One or more files are continued.</summary>
            PARTIAL_FILE,
            /// <summary>Called for each file in cabinet.</summary>
            COPY_FILE,
            /// <summary>Called after all of the data has been written to a target file.</summary>
            CLOSE_FILE_INFO,
            /// <summary>A file is continued to the next cabinet.</summary>
            NEXT_CABINET,
            /// <summary>Called once after a call to FDICopy() starts scanning a CAB's CFFILE entries, and again when there are no more CFFILE entries.</summary>
            ENUMERATE,
        }

        /// <summary>
        /// Converts DateTime to MS-DOS date and time which cabinet uses.
        /// </summary>
        /// <param name="dateTime">DateTime</param>
        /// <param name="cabDate">MS-DOS date</param>
        /// <param name="cabTime">MS-DOS time</param>
        public static void DateTimeToCabDateAndTime(DateTime dateTime, out ushort cabDate, out ushort cabTime)
        {
            // dateTime.ToLocalTime() does not match FileTimeToLocalFileTime() for some reason.
            // so we need to call FileTimeToLocalFileTime() from kernel32.dll.
            long filetime = dateTime.ToFileTime();
            long localTime = 0;
            FileTimeToLocalFileTime(ref filetime, ref localTime);
            FileTimeToDosDateTime(ref localTime, out cabDate, out cabTime);
        }

        /// <summary>
        /// Starts creating a cabinet.
        /// </summary>
        /// <param name="cabinetName">Name of cabinet to create.</param>
        /// <param name="cabinetDirectory">Directory to create cabinet in.</param>
        /// <param name="maxSize">Maximum size of the cabinet.</param>
        /// <param name="maxThreshold">Maximum threshold in the cabinet.</param>
        /// <param name="compressionType">Type of compression to use in the cabinet.</param>
        /// <param name="contextHandle">Handle to opened cabinet.</param>
        [DllImport("winterop.dll", EntryPoint = "CreateCabBegin", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern void CreateCabBegin(string cabinetName, string cabinetDirectory, uint maxSize, uint maxThreshold, uint compressionType, out IntPtr contextHandle);

        /// <summary>
        /// Adds a file to an open cabinet.
        /// </summary>
        /// <param name="file">Full path to file to add to cabinet.</param>
        /// <param name="token">Name of file in cabinet.</param>
        /// <param name="contextHandle">Handle to open cabinet.</param>
        [DllImport("winterop.dll", EntryPoint = "CreateCabAddFile", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern void CreateCabAddFile(string file, string token, IntPtr contextHandle);

        /// <summary>
        /// Closes a cabinet.
        /// </summary>
        /// <param name="contextHandle">Handle to open cabinet to close.</param>
        [DllImport("winterop.dll", EntryPoint = "CreateCabFinish", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern void CreateCabFinish(IntPtr contextHandle);

        /// <summary>
        /// Initializes cabinet extraction.
        /// </summary>
        [DllImport("winterop.dll", EntryPoint = "ExtractCabBegin", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern void ExtractCabBegin();

        /// <summary>
        /// Extracts files from cabinet.
        /// </summary>
        /// <param name="cabinet">Path to cabinet to extract files from.</param>
        /// <param name="extractDirectory">Directory to extract files to.</param>
        [DllImport("winterop.dll", EntryPoint = "ExtractCab", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, PreserveSig = false)]
        internal static extern void ExtractCab(string cabinet, string extractDirectory);

        /// <summary>
        /// Cleans up after cabinet extraction.
        /// </summary>
        [DllImport("winterop.dll", EntryPoint = "ExtractCabFinish", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern void ExtractCabFinish();

        /// <summary>
        /// Initializes cabinet enumeration.
        /// </summary>
        [DllImport("winterop.dll", EntryPoint = "EnumerateCabBegin", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern void EnumerateCabBegin();

        /// <summary>
        /// Enumerates files from cabinet.
        /// </summary>
        /// <param name="cabinet">Path to cabinet to enumerate files from.</param>
        /// <param name="notify">callback that gets each file.</param>
        [DllImport("winterop.dll", EntryPoint = "EnumerateCab", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, PreserveSig = false)]
        internal static extern void EnumerateCab(string cabinet, PFNNOTIFY notify);

        /// <summary>
        /// Cleans up after cabinet enumeration.
        /// </summary>
        [DllImport("winterop.dll", EntryPoint = "EnumerateCabFinish", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern void EnumerateCabFinish();

        /// <summary>
        /// Resets the DACL on an array of files to "empty".
        /// </summary>
        /// <param name="files">Array of file reset ACL to "empty".</param>
        /// <param name="fileCount">Number of file paths in array.</param>
        [DllImport("winterop.dll", EntryPoint = "ResetAcls", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        internal static extern void ResetAcls(string[] files, uint fileCount);

        /// <summary>
        /// Converts file time to a local file time.
        /// </summary>
        /// <param name="fileTime">file time</param>
        /// <param name="localTime">local file time</param>
        /// <returns>true if successful, false otherwise</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FileTimeToLocalFileTime(ref long fileTime, ref long localTime);

        /// <summary>
        /// Converts file time to a MS-DOS time.
        /// </summary>
        /// <param name="fileTime">file time</param>
        /// <param name="wFatDate">MS-DOS date</param>
        /// <param name="wFatTime">MS-DOS time</param>
        /// <returns>true if successful, false otherwise</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FileTimeToDosDateTime(ref long fileTime, out ushort wFatDate, out ushort wFatTime);

        /// <summary>
        /// Wraps FDINOTIFICATION.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class NOTIFICATION
        {
            private int cb = 0;
            [MarshalAs(UnmanagedType.LPStr)]
            private string psz1 = null;
            [MarshalAs(UnmanagedType.LPStr)]
            private string psz2 = null;
            [MarshalAs(UnmanagedType.LPStr)]
            private string psz3 = null;
            private IntPtr pv = (IntPtr)0;

            private IntPtr hf = IntPtr.Zero;

            private ushort date = 0;
            private ushort time = 0;
            private ushort attribs = 0;
            private ushort setID = 0;
            private ushort cabinet = 0;
            private ushort folder = 0;
            private int fdie = 0;

            /// <summary>
            /// Uncompressed size of file.
            /// </summary>
            public int Cb
            {
                get { return this.cb; }
            }

            /// <summary>
            /// File name in cabinet.
            /// </summary>
            public String Psz1 
            { 
                get { return this.psz1; } 
            }

            /// <summary>
            /// Name of next disk.
            /// </summary>
            public string Psz2 
            { 
                get { return this.psz2; } 
            }

            /// <summary>
            /// Points to a 256 character buffer.
            /// </summary>
            public string Psz3 
            {
                get { return this.psz3; } 
            }

            /// <summary>
            /// Value for client.
            /// </summary>
            public IntPtr Pv 
            { 
                get { return this.pv; } 
            }

            /// <summary>
            /// Not used.
            /// </summary>
            public Int32 Hf 
            { 
                get { return (Int32)this.hf; }
            }

            /// <summary>
            /// Last modified MS-DOS date.
            /// </summary>
            public ushort Date 
            { 
                get { return this.date; } 
            }

            /// <summary>
            /// Last modified MS-DOS time.
            /// </summary>
            public ushort Time 
            { 
                get { return this.time; } 
            }

            /// <summary>
            /// File attributes.
            /// </summary>
            public ushort Attribs 
            { 
                get { return this.attribs; } 
            }

            /// <summary>
            /// Cabinet set ID (a random 16-bit number).
            /// </summary>
            public ushort SetID 
            {
                get { return this.setID; } 
            }

            /// <summary>
            /// Cabinet number within cabinet set (0-based).
            /// </summary>
            public ushort Cabinet 
            { 
                get { return this.cabinet; } 
            }

            /// <summary>
            /// File's folder index.
            /// </summary>
            public ushort Folder 
            { 
                get { return this.folder; } 
            }

            /// <summary>
            /// Error code.
            /// </summary>
            public int Fdie 
            { 
                get { return this.fdie; } 
            }
        }
    }
}
