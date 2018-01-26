//-------------------------------------------------------------------------------------------------
// <copyright file="SummaryInformation.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Summary information for the MSI files.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Msi
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Runtime.InteropServices;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Summary information for the MSI files.
    /// </summary>
    internal sealed class SummaryInformation : MsiHandle
    {
        /// <summary>
        /// Summary information properties for patches.
        /// </summary>
        public enum Transform
        {
            /// <summary>PID_REVNUMBER = {productcode}version;{newproductcode}newversion;upgradecode</summary>
            ProductCodes = 9,

            /// <summary>PID_PAGECOUNT = minimum installer version</summary>
            InstallerRequirement = 14,

            /// <summary>PID_CHARCOUNT = validation and error flags</summary>
            ValidationFlags = 16,
        }

        /// <summary>
        /// Summary information properties for patches.
        /// </summary>
        public enum Patch
        {
            /// <summary>PID_TEMPLATE = semicolon delimited list of ProductCodes</summary>
            ProductCodes = 7,

            /// <summary>PID_LASTAUTHOR = semicolon delimited list of transform names</summary>
            TransformNames = 8,

            /// <summary>PID_REVNUMBER = GUID patch code</summary>
            PatchCode = 9,

            /// <summary>PID_WORDCOUNT = minimum installer version</summary>
            InstallerRequirement = 15,
        }

        /// <summary>
        /// Summary information values for the InstallerRequirement property.
        /// </summary>
        public enum InstallerRequirement
        {
            /// <summary>Any version of the installer will do</summary>
            Version10 = 1,

            /// <summary>At least 1.2</summary>
            Version12 = 2,

            /// <summary>At least 2.0</summary>
            Version20 = 3,

            /// <summary>At least 3.0</summary>
            Version30 = 4,

            /// <summary>At least 3.1</summary>
            Version31 = 5,
        }

        /// <summary>
        /// Summary information values for the CharCount property in transforms.
        /// </summary>
        public enum TransformFlags
        {
            /// <summary>Ignore error when adding a row that exists.</summary>
            ErrorAddExistingRow = 0x1,

            /// <summary>Ignore error when deleting a row that does not exist.</summary>
            ErrorDeleteMissingRow = 0x2,

            /// <summary>Ignore error when adding a table that exists. </summary>
            ErrorAddExistingTable = 0x4,

            /// <summary>Ignore error when deleting a table that does not exist. </summary>
            ErrorDeleteMissingTable = 0x8,

            /// <summary>Ignore error when updating a row that does not exist. </summary>
            ErrorUpdateMissingRow = 0x10,

            /// <summary>Ignore error when transform and database code pages do not match, and their code pages are neutral.</summary>
            ErrorChangeCodePage = 0x20,

            /// <summary>Default language must match base database. </summary>
            ValidateLanguage = 0x10000,

            /// <summary>Product must match base database.</summary>
            ValidateProduct = 0x20000,

            /// <summary>Check major version only. </summary>
            ValidateMajorVersion = 0x80000,

            /// <summary>Check major and minor versions only. </summary>
            ValidateMinorVersion = 0x100000,

            /// <summary>Check major, minor, and update versions.</summary>
            ValidateUpdateVersion = 0x200000,

            /// <summary>Installed version lt base version. </summary>
            ValidateNewLessBaseVersion = 0x400000,

            /// <summary>Installed version lte base version. </summary>
            ValidateNewLessEqualBaseVersion = 0x800000,

            /// <summary>Installed version eq base version. </summary>
            ValidateNewEqualBaseVersion = 0x1000000,

            /// <summary>Installed version gte base version.</summary>
            ValidateNewGreaterEqualBaseVersion = 0x2000000,

            /// <summary>Installed version gt base version.</summary>
            ValidateNewGreaterBaseVersion = 0x4000000,
        }

        /// <summary>
        /// Instantiate a new SummaryInformation class from an open database.
        /// </summary>
        /// <param name="db">Database to retrieve summary information from.</param>
        public SummaryInformation(Database db)
        {
            if (null == db)
            {
                throw new ArgumentNullException("db");
            }

            uint handle = 0;
            int error = MsiInterop.MsiGetSummaryInformation(db.Handle, null, 0, ref handle);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
            this.Handle = handle;
        }

        /// <summary>
        /// Instantiate a new SummaryInformation class from a database file.
        /// </summary>
        /// <param name="databaseFile">The database file.</param>
        public SummaryInformation(string databaseFile)
        {
            if (null == databaseFile)
            {
                throw new ArgumentNullException("databaseFile");
            }

            uint handle = 0;
            int error = MsiInterop.MsiGetSummaryInformation(0, databaseFile, 0, ref handle);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }
            this.Handle = handle;
        }

        /// <summary>
        /// Variant types in the summary information table.
        /// </summary>
        private enum VT : uint
        {
            /// <summary>Variant has not been assigned.</summary>
            EMPTY    = 0,

            /// <summary>Null variant type.</summary>
            NULL     = 1,

            /// <summary>16-bit integer variant type.</summary>
            I2       = 2,

            /// <summary>32-bit integer variant type.</summary>
            I4       = 3,

            /// <summary>String variant type.</summary>
            LPSTR    = 30,

            /// <summary>Date time (FILETIME, converted to Variant time) variant type.</summary>
            FILETIME = 64,
        }

        /// <summary>
        /// Gets a summary information property.
        /// </summary>
        /// <param name="index">Index of the summary information property.</param>
        /// <returns>The summary information property.</returns>
        public string GetProperty(int index)
        {
            uint dataType;
            StringBuilder stringValue = new StringBuilder("");
            int bufSize = 0;
            int intValue;
            FILETIME timeValue;
            timeValue.dwHighDateTime = 0;
            timeValue.dwLowDateTime = 0;

            int error = MsiInterop.MsiSummaryInfoGetProperty(this.Handle, index, out dataType, out intValue, ref timeValue, stringValue, ref bufSize);
            if (234 == error)
            {
                stringValue.EnsureCapacity(++bufSize);
                error = MsiInterop.MsiSummaryInfoGetProperty(this.Handle, index, out dataType, out intValue, ref timeValue, stringValue, ref bufSize);
            }

            if (0 != error)
            {
                throw new Win32Exception(error);
            }

            switch ((VT)dataType)
            {
                case VT.EMPTY:
                    return String.Empty;
                case VT.LPSTR:
                    return stringValue.ToString();
                case VT.I2:
                case VT.I4:
                    return Convert.ToString(intValue, CultureInfo.InvariantCulture);
                case VT.FILETIME:
                    long longFileTime = (((long)timeValue.dwHighDateTime) << 32) | unchecked((uint)timeValue.dwLowDateTime);
                    DateTime dateTime = DateTime.FromFileTime(longFileTime);
                    return dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
