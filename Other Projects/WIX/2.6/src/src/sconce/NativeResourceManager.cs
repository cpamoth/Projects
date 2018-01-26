//-------------------------------------------------------------------------------------------------
// <copyright file="NativeResourceManager.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the NativeResourceManager class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Contains utility functions for retrieving resources from the main Visual Studio
    /// package's satellite resource DLL.
    /// </summary>
    public class NativeResourceManager
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================
        
        private static readonly Type classType = typeof(NativeResourceManager);
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeResourceManager"/> class.
        /// </summary>
        public NativeResourceManager(int lcid)
        {
            try
            {
                // Set the thread's cultures to that of the VS shell's locale so that our
                // resource strings will be the right language.
                CultureInfo culture = new CultureInfo(lcid, false);
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
            catch (Exception e)
            {
                Tracer.Fail("Cannot set the current thread's culture to {0}: {1}", lcid, e.ToString());
            }
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        /// <summary>
        /// Gets the package type GUID used for retrieving strings from the package's resource DLL.
        /// </summary>
        protected virtual Guid PackageTypeGuid
        {
            get { return Package.Instance.PackageTypeGuid; }
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        /// <summary>
        /// Gets a localized string from the satellite assembly.
        /// </summary>
        /// <param name="resourceId">The string to retrieve from the satellite assembly.</param>
        /// <returns>A localized string from the satellite assembly.</returns>
        public string GetString(ResourceId resourceId)
        {
            Guid packageGuid = this.PackageTypeGuid;
            string stringResource;
            IVsShell vsShell = Package.Instance.Context.ServiceProvider.GetVsShell(classType, "GetString");
            int hr = vsShell.LoadPackageString(ref packageGuid, (uint)resourceId, out stringResource);
            NativeMethods.ThrowOnFailure(hr);

            return stringResource;
        }

        /// <summary>
        /// Gets a formatted, localized string from the satellite assembly.
        /// </summary>
        /// <param name="resourceId">The string to retrieve from the satellite assembly.</param>
        /// <param name="args">Array of arguments to use in formatting the localized string.</param>
        /// <returns>A formatted, localized string from the satellite assembly.</returns>
        public string GetString(ResourceId resourceId, params object[] args)
        {
            string rawString = this.GetString(resourceId);
            return String.Format(CultureInfo.CurrentUICulture, rawString, args);
        }
        #endregion
    }
}
