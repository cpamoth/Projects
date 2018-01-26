//-------------------------------------------------------------------------------------------------
// <copyright file="WixPackageSettings.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixPackageSettings class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Globalization;
    using System.Security.AccessControl;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.Win32;

    /// <summary>
    /// Helper class for setting and retrieving registry settings for the package. All machine
    /// settings are cached on first use, so only one registry read is performed.
    /// </summary>
    public class WixPackageSettings
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private static readonly Version DefaultVersion = new Version(8, 0, 50727, 42);

        private string devEnvPath;
        private string machineRootPath;
        private string visualStudioRegistryRoot;
        private Version visualStudioVersion = null;
        private MachineSettingString toolsDirectory;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixPackageSettings"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        public WixPackageSettings(IServiceProvider serviceProvider)
        {
            WixHelperMethods.VerifyNonNullArgument(serviceProvider, "serviceProvider");

            // get the Visual Studio registry root
            ILocalRegistry3 localRegistry = WixHelperMethods.GetService<ILocalRegistry3, SLocalRegistry>(serviceProvider);
            ErrorHandler.ThrowOnFailure(localRegistry.GetLocalRegistryRoot(out this.visualStudioRegistryRoot));

            this.machineRootPath = WixHelperMethods.RegistryPathCombine(this.visualStudioRegistryRoot, @"InstalledProducts\WiX");

            // initialize all of the machine settings
            this.toolsDirectory = new MachineSettingString(this.machineRootPath, KeyNames.ToolsDirectory, String.Empty);
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the path to the directory where the WiX tools reside.
        /// </summary>
        public string ToolsDirectory
        {
            get { return this.toolsDirectory.Value; }
        }

        /// <summary>
        /// Gets the absolute path to the devenv.exe that we're currently running in.
        /// </summary>
        public string DevEnvPath
        {
            get
            {
                if (this.devEnvPath == null)
                {
                    string regPath = WixHelperMethods.RegistryPathCombine(this.visualStudioRegistryRoot, @"Setup\VS");
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey))
                    {
                        this.devEnvPath = regKey.GetValue("EnvironmentPath", String.Empty) as string;
                    }
                }

                return this.devEnvPath;
            }
        }

        /// <summary>
        /// Gets the version of the currently running instance of Visual Studio.
        /// </summary>
        public Version VisualStudioVersion
        {
            get
            {
                if (this.visualStudioVersion == null)
                {
                    string regPath = WixHelperMethods.RegistryPathCombine(this.visualStudioRegistryRoot, @"Setup\VS\BuildNumber");
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey))
                    {
                        string lcid = CultureInfo.CurrentUICulture.LCID.ToString();
                        string versionString = regKey.GetValue(lcid) as string;
                        if (versionString == null)
                        {
                            WixHelperMethods.TraceFail("Cannot find the Visual Studio environment version in the registry path '{0}'.", WixHelperMethods.RegistryPathCombine(regPath, lcid));
                            this.visualStudioVersion = DefaultVersion;
                        }
                        else
                        {
                            try
                            {
                                this.visualStudioVersion = new Version(versionString);
                            }
                            catch (Exception e)
                            {
                                WixHelperMethods.TraceFail("Cannot parse the Visual Studio environment version string {0}: {1}", versionString, e);
                                this.visualStudioVersion = DefaultVersion;
                            }
                        }
                    }
                }

                return this.visualStudioVersion;
            }
        }

        // =========================================================================================
        // Classes
        // =========================================================================================

        /// <summary>
        /// Names of the various registry keys that store our settings.
        /// </summary>
        private sealed class KeyNames
        {
            public const string ToolsDirectory = "ToolsDirectory";
        }

        /// <summary>
        /// Abstract base class for a strongly-typed machine-level setting.
        /// </summary>
        private abstract class MachineSetting<T>
        {
            private T defaultValue;
            private bool initialized;
            private string name;
            private string rootPath;
            private T value;

            public MachineSetting(string rootPath, string name, T defaultValue)
            {
                this.rootPath = rootPath;
                this.name = name;
                this.defaultValue = defaultValue;
            }

            public string Name
            {
                get { return this.name; }
            }

            public T Value
            {
                get
                {
                    if (!this.initialized)
                    {
                        this.Refresh();
                    }
                    return this.value;
                }

                protected set
                {
                    this.value = value;
                }
            }

            protected T DefaultValue
            {
                get { return this.defaultValue; }
            }

            public void Refresh()
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(this.rootPath, false))
                {
                    object value = regKey.GetValue(this.name, this.defaultValue, RegistryValueOptions.None);
                    this.initialized = true;
                    this.value = (T)value;
                }
            }

            protected abstract void CastAndStoreValue(object value);
        }

        /// <summary>
        /// Represents a strongly-typed integer machine setting.
        /// </summary>
        private sealed class MachineSettingInt32 : MachineSetting<int>
        {
            public MachineSettingInt32(string rootPath, string name, int defaultValue)
                : base(rootPath, name, defaultValue)
            {
            }

            protected override void CastAndStoreValue(object value)
            {
                try
                {
                    this.Value = (int)value;
                }
                catch (InvalidCastException)
                {
                    this.Value = this.DefaultValue;
                    WixHelperMethods.TraceFail("Cannot convert '{0}' to an Int32.", value);
                }
            }
        }

        /// <summary>
        /// Represents a strongly-typed string machine setting.
        /// </summary>
        private sealed class MachineSettingString : MachineSetting<string>
        {
            public MachineSettingString(string rootPath, string name, string defaultValue)
                : base(rootPath, name, defaultValue)
            {
            }

            protected override void CastAndStoreValue(object value)
            {
                try
                {
                    this.Value = (string)value;
                }
                catch (InvalidCastException)
                {
                    this.Value = this.DefaultValue;
                    WixHelperMethods.TraceFail("Cannot convert '{0}' to a string.", value);
                }
            }
        }

        /// <summary>
        /// Represents a strongly-typed enum machine setting.
        /// </summary>
        private class MachineSettingEnum<T> : MachineSetting<T> where T : struct
        {
            public MachineSettingEnum(string rootPath, string name, T defaultValue)
                : base(rootPath, name, defaultValue)
            {
            }

            protected override void CastAndStoreValue(object value)
            {
                try
                {
                    this.Value = (T)Enum.Parse(typeof(T), value.ToString(), true);
                }
                catch (Exception e)
                {
                    if (e is FormatException || e is InvalidCastException)
                    {
                        this.Value = this.DefaultValue;
                        WixHelperMethods.TraceFail("Cannot convert '{0}' to an enum of type '{1}'.", value, typeof(T).Name);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
