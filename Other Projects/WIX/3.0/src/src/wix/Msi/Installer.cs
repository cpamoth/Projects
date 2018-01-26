//-------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Represents the Windows Installer, provides wrappers to
// create the top-level objects and access their methods.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Msi
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Windows Installer message types.
    /// </summary>
    [Flags]
    internal enum InstallMessage
    {
        /// <summary>
        /// Premature termination, possibly fatal out of memory.
        /// </summary>
        FatalExit = 0x00000000,

        /// <summary>
        /// Formatted error message, [1] is message number in Error table.
        /// </summary>
        Error = 0x01000000,

        /// <summary>
        /// Formatted warning message, [1] is message number in Error table.
        /// </summary>
        Warning = 0x02000000,

        /// <summary>
        /// User request message, [1] is message number in Error table.
        /// </summary>
        User = 0x03000000,

        /// <summary>
        /// Informative message for log, not to be displayed.
        /// </summary>
        Info = 0x04000000,

        /// <summary>
        /// List of files in use that need to be replaced.
        /// </summary>
        FilesInUse = 0x05000000,

        /// <summary>
        /// Request to determine a valid source location.
        /// </summary>
        ResolveSource = 0x06000000,

        /// <summary>
        /// Insufficient disk space message.
        /// </summary>
        OutOfDiskSpace = 0x07000000,

        /// <summary>
        /// Progress: start of action, [1] action name, [2] description, [3] template for ACTIONDATA messages.
        /// </summary>
        ActionStart = 0x08000000,

        /// <summary>
        /// Action data. Record fields correspond to the template of ACTIONSTART message.
        /// </summary>
        ActionData = 0x09000000,

        /// <summary>
        /// Progress bar information. See the description of record fields below.
        /// </summary>
        Progress = 0x0A000000,

        /// <summary>
        /// To enable the Cancel button set [1] to 2 and [2] to 1. To disable the Cancel button set [1] to 2 and [2] to 0.
        /// </summary>
        CommonData = 0x0B000000,

        /// <summary>
        /// Sent prior to UI initialization, no string data.
        /// </summary>
        Initilize = 0x0C000000,

        /// <summary>
        /// Sent after UI termination, no string data.
        /// </summary>
        Terminate = 0x0D000000,

        /// <summary>
        /// Sent prior to display or authored dialog or wizard.
        /// </summary>
        ShowDialog = 0x0E000000
    }

    /// <summary>
    /// Windows Installer log modes.
    /// </summary>
    [Flags]
    internal enum InstallLogModes
    {
        /// <summary>
        /// Premature termination of installation.
        /// </summary>
        FatalExit = (1 << ((int)InstallMessage.FatalExit >> 24)),

        /// <summary>
        /// The error messages are logged.
        /// </summary>
        Error = (1 << ((int)InstallMessage.Error >> 24)),

        /// <summary>
        /// The warning messages are logged.
        /// </summary>
        Warning = (1 << ((int)InstallMessage.Warning >> 24)),

        /// <summary>
        /// The user requests are logged.
        /// </summary>
        User = (1 << ((int)InstallMessage.User >> 24)),

        /// <summary>
        /// The status messages that are not displayed are logged.
        /// </summary>
        Info = (1 << ((int)InstallMessage.Info >> 24)),

        /// <summary>
        /// Request to determine a valid source location.
        /// </summary>
        ResolveSource = (1 << ((int)InstallMessage.ResolveSource >> 24)),

        /// <summary>
        /// The was insufficient disk space.
        /// </summary>
        OutOfDiskSpace = (1 << ((int)InstallMessage.OutOfDiskSpace >> 24)),

        /// <summary>
        /// The start of new installation actions are logged.
        /// </summary>
        ActionStart = (1 << ((int)InstallMessage.ActionStart >> 24)),

        /// <summary>
        /// The data record with the installation action is logged.
        /// </summary>
        ActionData = (1 << ((int)InstallMessage.ActionData >> 24)),

        /// <summary>
        /// The parameters for user-interface initialization are logged.
        /// </summary>
        CommonData = (1 << ((int)InstallMessage.CommonData >> 24)),

        /// <summary>
        /// Logs the property values at termination.
        /// </summary>
        PropertyDump = (1 << ((int)InstallMessage.Progress >> 24)),

        /// <summary>
        /// Sends large amounts of information to a log file not generally useful to users.
        /// May be used for technical support.
        /// </summary>
        Verbose = (1 << ((int)InstallMessage.Initilize >> 24)),

        /// <summary>
        /// Sends extra debugging information, such as handle creation information, to the log file.
        /// </summary>
        ExtraDebug = (1 << ((int)InstallMessage.Terminate >> 24)),

        /// <summary>
        /// Progress bar information. This message includes information on units so far and total number of units.
        /// See MsiProcessMessage for an explanation of the message format.
        /// This message is only sent to an external user interface and is not logged.
        /// </summary>
        Progress = (1 << ((int)InstallMessage.Progress >> 24)),

        /// <summary>
        /// If this is not a quiet installation, then the basic UI has been initialized.
        /// If this is a full UI installation, the full UI is not yet initialized.
        /// This message is only sent to an external user interface and is not logged.
        /// </summary>
        Initialize = (1 << ((int)InstallMessage.Initilize >> 24)),

        /// <summary>
        /// If a full UI is being used, the full UI has ended.
        /// If this is not a quiet installation, the basic UI has not yet ended.
        /// This message is only sent to an external user interface and is not logged.
        /// </summary>
        Terminate = (1 << ((int)InstallMessage.Terminate >> 24)),

        /// <summary>
        /// Sent prior to display of the full UI dialog.
        /// This message is only sent to an external user interface and is not logged.
        /// </summary>
        ShowDialog = (1 << ((int)InstallMessage.ShowDialog >> 24)),

        /// <summary>
        /// Files in use information. When this message is received, a FilesInUse Dialog should be displayed.
        /// </summary>
        FilesInUse = (1 << ((int)InstallMessage.FilesInUse >> 24))
    }

    /// <summary>
    /// Windows Installer UI levels.
    /// </summary>
    [Flags]
    internal enum InstallUILevels
    {
        /// <summary>
        /// No change in the UI level. However, if phWnd is not Null, the parent window can change.
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// The installer chooses an appropriate user interface level.
        /// </summary>
        Default = 1,

        /// <summary>
        /// Completely silent installation.
        /// </summary>
        None = 2,

        /// <summary>
        /// Simple progress and error handling.
        /// </summary>
        Basic = 3,

        /// <summary>
        /// Authored user interface with wizard dialog boxes suppressed.
        /// </summary>
        Reduced = 4,

        /// <summary>
        /// Authored user interface with wizards, progress, and errors.
        /// </summary>
        Full = 5,

        /// <summary>
        /// If combined with the Basic value, the installer shows simple progress dialog boxes but
        /// does not display a Cancel button on the dialog. This prevents users from canceling the install.
        /// Available with Windows Installer version 2.0.
        /// </summary>
        HideCancel = 0x20,

        /// <summary>
        /// If combined with the Basic value, the installer shows simple progress
        /// dialog boxes but does not display any modal dialog boxes or error dialog boxes.
        /// </summary>
        ProgressOnly = 0x40,

        /// <summary>
        /// If combined with any above value, the installer displays a modal dialog
        /// box at the end of a successful installation or if there has been an error.
        /// No dialog box is displayed if the user cancels.
        /// </summary>
        EndDialog = 0x80,

        /// <summary>
        /// If this value is combined with the None value, the installer displays only the dialog
        /// boxes used for source resolution. No other dialog boxes are shown. This value has no
        /// effect if the UI level is not INSTALLUILEVEL_NONE. It is used with an external user
        /// interface designed to handle all of the UI except for source resolution. In this case,
        /// the installer handles source resolution. This value is only available with Windows Installer 2.0 and later.
        /// </summary>
        SourceResOnly = 0x100
    }

    /// <summary>
    /// Represents the Windows Installer, provides wrappers to
    /// create the top-level objects and access their methods.
    /// </summary>
    internal sealed class Installer
    {
        private static object lockObject = new object();

        private static TableDefinitionCollection tableDefinitions;
        private static WixActionRowCollection standardActions;
        private static Hashtable standardActionsHash;
        private static Hashtable standardDirectories;
        private static Hashtable standardProperties;

        internal static Hashtable StandardDirectories
        {
            get
            {
                if (null == standardDirectories)
                {
                    LoadStandardDirectories();
                }
                return standardDirectories;
            }
        }

        /// <summary>
        /// Protect the constructor.
        /// </summary>
        private Installer()
        {
        }

        /// <summary>
        /// Takes the path to a file and returns a 128-bit hash of that file.
        /// </summary>
        /// <param name="filePath">Path to file that is to be hashed.</param>
        /// <param name="options">The value in this column must be 0. This parameter is reserved for future use.</param>
        /// <param name="hash">Int array that receives the returned file hash information.</param>
        internal static void GetFileHash(string filePath, int options, out int[] hash)
        {
            MsiInterop.MSIFILEHASHINFO hashInterop = new MsiInterop.MSIFILEHASHINFO();
            hashInterop.FileHashInfoSize = 20;

            int error = MsiInterop.MsiGetFileHash(filePath, Convert.ToUInt32(options), ref hashInterop);
            if (0 != error)
            {
                throw new Win32Exception(error);
            }

            Debug.Assert(20 == hashInterop.FileHashInfoSize);

            hash = new int[4];
            hash[0] = hashInterop.Data0;
            hash[1] = hashInterop.Data1;
            hash[2] = hashInterop.Data2;
            hash[3] = hashInterop.Data3;
        }

        /// <summary>
        /// Returns the version string and language string in the format that the installer 
        /// expects to find them in the database.  If you just want version information, set 
        /// lpLangBuf and pcchLangBuf to zero. If you just want language information, set 
        /// lpVersionBuf and pcchVersionBuf to zero.
        /// </summary>
        /// <param name="filePath">Specifies the path to the file.</param>
        /// <param name="version">Returns the file version. Set to 0 for language information only.</param>
        /// <param name="language">Returns the file language. Set to 0 for version information only.</param>
        internal static void GetFileVersion(string filePath, out string version, out string language)
        {
            int versionLength = 20;
            int languageLength = 20;
            StringBuilder versionBuffer = new StringBuilder(versionLength);
            StringBuilder languageBuffer = new StringBuilder(languageLength);

            int error = MsiInterop.MsiGetFileVersion(filePath, versionBuffer, ref versionLength, languageBuffer, ref languageLength);
            if (234 == error)
            {
                versionBuffer.EnsureCapacity(++versionLength);
                languageBuffer.EnsureCapacity(++languageLength);
                error = MsiInterop.MsiGetFileVersion(filePath, versionBuffer, ref versionLength, languageBuffer, ref languageLength);
            }
            else if (1006 == error)
            {
                // file has no version or language, so no error
                error = 0;
            }

            if (0 != error)
            {
                throw new Win32Exception(error);
            }

            version = versionBuffer.ToString();
            language = languageBuffer.ToString();
        }

        /// <summary>
        /// Gets the table definitions stored in this assembly.
        /// </summary>
        /// <returns>Table definition collection for tables stored in this assembly.</returns>
        internal static TableDefinitionCollection GetTableDefinitions()
        {
            lock (lockObject)
            {
                if (null == tableDefinitions)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    XmlReader tableDefinitionsReader = null;

                    try
                    {
                        tableDefinitionsReader = new XmlTextReader(assembly.GetManifestResourceStream("Microsoft.Tools.WindowsInstallerXml.Data.tables.xml"));

#if DEBUG
                        tableDefinitions = TableDefinitionCollection.Load(tableDefinitionsReader, false);
#else
                        tableDefinitions = TableDefinitionCollection.Load(tableDefinitionsReader, true);
#endif
                    }
                    finally
                    {
                        if (null != tableDefinitionsReader)
                        {
                            tableDefinitionsReader.Close();
                        }
                    }
                }
            }

            return tableDefinitions.Clone();
        }

        /// <summary>
        /// Gets the standard actions stored in this assembly.
        /// </summary>
        /// <returns>Collection of standard actions in this assembly.</returns>
        internal static WixActionRowCollection GetStandardActions()
        {
            lock (lockObject)
            {
                if (null == standardActions)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    XmlReader actionDefinitionsReader = null;

                    try
                    {
                        actionDefinitionsReader = new XmlTextReader(assembly.GetManifestResourceStream("Microsoft.Tools.WindowsInstallerXml.Data.actions.xml"));
#if DEBUG
                        standardActions = WixActionRowCollection.Load(actionDefinitionsReader, false);
#else
                        standardActions = WixActionRowCollection.Load(actionDefinitionsReader, true);
#endif
                    }
                    finally
                    {
                        if (null != actionDefinitionsReader)
                        {
                            actionDefinitionsReader.Close();
                        }
                    }
                }
            }

            return standardActions;
        }

        /// <summary>
        /// Find out if an action is a standard action.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        /// <returns>true if the action is standard, false otherwise.</returns>
        internal static bool IsStandardAction(string actionName)
        {
            lock (lockObject)
            {
                if (null == standardActionsHash)
                {
                    standardActionsHash = new Hashtable();
                    standardActionsHash.Add("InstallInitialize", null);
                    standardActionsHash.Add("InstallFinalize", null);
                    standardActionsHash.Add("InstallFiles", null);
                    standardActionsHash.Add("InstallAdminPackage", null);
                    standardActionsHash.Add("FileCost", null);
                    standardActionsHash.Add("CostInitialize", null);
                    standardActionsHash.Add("CostFinalize", null);
                    standardActionsHash.Add("InstallValidate", null);
                    standardActionsHash.Add("ExecuteAction", null);
                    standardActionsHash.Add("CreateShortcuts", null);
                    standardActionsHash.Add("MsiPublishAssemblies", null);
                    standardActionsHash.Add("PublishComponents", null);
                    standardActionsHash.Add("PublishFeatures", null);
                    standardActionsHash.Add("PublishProduct", null);
                    standardActionsHash.Add("RegisterClassInfo", null);
                    standardActionsHash.Add("RegisterExtensionInfo", null);
                    standardActionsHash.Add("RegisterMIMEInfo", null);
                    standardActionsHash.Add("RegisterProgIdInfo", null);
                    standardActionsHash.Add("AllocateRegistrySpace", null);
                    standardActionsHash.Add("AppSearch", null);
                    standardActionsHash.Add("BindImage", null);
                    standardActionsHash.Add("CCPSearch", null);
                    standardActionsHash.Add("CreateFolders", null);
                    standardActionsHash.Add("DeleteServices", null);
                    standardActionsHash.Add("DuplicateFiles", null);
                    standardActionsHash.Add("FindRelatedProducts", null);
                    standardActionsHash.Add("InstallODBC", null);
                    standardActionsHash.Add("InstallServices", null);
                    standardActionsHash.Add("IsolateComponents", null);
                    standardActionsHash.Add("LaunchConditions", null);
                    standardActionsHash.Add("MigrateFeatureStates", null);
                    standardActionsHash.Add("MoveFiles", null);
                    standardActionsHash.Add("PatchFiles", null);
                    standardActionsHash.Add("ProcessComponents", null);
                    standardActionsHash.Add("RegisterComPlus", null);
                    standardActionsHash.Add("RegisterFonts", null);
                    standardActionsHash.Add("RegisterProduct", null);
                    standardActionsHash.Add("RegisterTypeLibraries", null);
                    standardActionsHash.Add("RegisterUser", null);
                    standardActionsHash.Add("RemoveDuplicateFiles", null);
                    standardActionsHash.Add("RemoveEnvironmentStrings", null);
                    standardActionsHash.Add("RemoveFiles", null);
                    standardActionsHash.Add("RemoveFolders", null);
                    standardActionsHash.Add("RemoveIniValues", null);
                    standardActionsHash.Add("RemoveODBC", null);
                    standardActionsHash.Add("RemoveRegistryValues", null);
                    standardActionsHash.Add("RemoveShortcuts", null);
                    standardActionsHash.Add("RMCCPSearch", null);
                    standardActionsHash.Add("SelfRegModules", null);
                    standardActionsHash.Add("SelfUnregModules", null);
                    standardActionsHash.Add("SetODBCFolders", null);
                    standardActionsHash.Add("StartServices", null);
                    standardActionsHash.Add("StopServices", null);
                    standardActionsHash.Add("MsiUnpublishAssemblies", null);
                    standardActionsHash.Add("UnpublishComponents", null);
                    standardActionsHash.Add("UnpublishFeatures", null);
                    standardActionsHash.Add("UnregisterClassInfo", null);
                    standardActionsHash.Add("UnregisterComPlus", null);
                    standardActionsHash.Add("UnregisterExtensionInfo", null);
                    standardActionsHash.Add("UnregisterFonts", null);
                    standardActionsHash.Add("UnregisterMIMEInfo", null);
                    standardActionsHash.Add("UnregisterProgIdInfo", null);
                    standardActionsHash.Add("UnregisterTypeLibraries", null);
                    standardActionsHash.Add("ValidateProductID", null);
                    standardActionsHash.Add("WriteEnvironmentStrings", null);
                    standardActionsHash.Add("WriteIniValues", null);
                    standardActionsHash.Add("WriteRegistryValues", null);
                    standardActionsHash.Add("InstallExecute", null);
                    standardActionsHash.Add("InstallExecuteAgain", null);
                    standardActionsHash.Add("RemoveExistingProducts", null);
                    standardActionsHash.Add("DisableRollback", null);
                    standardActionsHash.Add("ScheduleReboot", null);
                    standardActionsHash.Add("ForceReboot", null);
                    standardActionsHash.Add("ResolveSource", null);
                }
            }

            return standardActionsHash.ContainsKey(actionName);
        }

        /// <summary>
        /// Find out if a directory is a standard directory.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns>true if the directory is standard, false otherwise.</returns>
        internal static bool IsStandardDirectory(string directoryName)
        {
            if (null == standardDirectories)
            {
                LoadStandardDirectories();
            }

            return standardDirectories.ContainsKey(directoryName);
        }

        /// <summary>
        /// Find out if a property is a standard property.
        /// References: 
        /// Title:   Property Reference [Windows Installer]: 
        /// URL:     http://msdn.microsoft.com/library/en-us/msi/setup/property_reference.asp
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>true if a property is standard, false otherwise.</returns>
        internal static bool IsStandardProperty(string propertyName)
        {
            lock (lockObject)
            {
                if (null == standardProperties)
                {
                    standardProperties = new Hashtable();
                    standardProperties.Add("~", null); // REG_MULTI_SZ/NULL marker
                    standardProperties.Add("ACTION", null);
                    standardProperties.Add("ADDDEFAULT", null);
                    standardProperties.Add("ADDLOCAL", null);
                    standardProperties.Add("ADDDSOURCE", null);
                    standardProperties.Add("AdminProperties", null);
                    standardProperties.Add("AdminUser", null);
                    standardProperties.Add("ADVERTISE", null);
                    standardProperties.Add("AFTERREBOOT", null);
                    standardProperties.Add("AllowProductCodeMismatches", null);
                    standardProperties.Add("AllowProductVersionMajorMismatches", null);
                    standardProperties.Add("ALLUSERS", null);
                    standardProperties.Add("Alpha", null);
                    standardProperties.Add("ApiPatchingSymbolFlags", null);
                    standardProperties.Add("ARPAUTHORIZEDCDFPREFIX", null);
                    standardProperties.Add("ARPCOMMENTS", null);
                    standardProperties.Add("ARPCONTACT", null);
                    standardProperties.Add("ARPHELPLINK", null);
                    standardProperties.Add("ARPHELPTELEPHONE", null);
                    standardProperties.Add("ARPINSTALLLOCATION", null);
                    standardProperties.Add("ARPNOMODIFY", null);
                    standardProperties.Add("ARPNOREMOVE", null);
                    standardProperties.Add("ARPNOREPAIR", null);
                    standardProperties.Add("ARPPRODUCTIONICON", null);
                    standardProperties.Add("ARPREADME", null);
                    standardProperties.Add("ARPSIZE", null);
                    standardProperties.Add("ARPSYSTEMCOMPONENT", null);
                    standardProperties.Add("ARPULRINFOABOUT", null);
                    standardProperties.Add("ARPURLUPDATEINFO", null);
                    standardProperties.Add("AVAILABLEFREEREG", null);
                    standardProperties.Add("BorderSize", null);
                    standardProperties.Add("BorderTop", null);
                    standardProperties.Add("CaptionHeight", null);
                    standardProperties.Add("CCP_DRIVE", null);
                    standardProperties.Add("ColorBits", null);
                    standardProperties.Add("COMPADDLOCAL", null);
                    standardProperties.Add("COMPADDSOURCE", null);
                    standardProperties.Add("COMPANYNAME", null);
                    standardProperties.Add("ComputerName", null);
                    standardProperties.Add("CostingComplete", null);
                    standardProperties.Add("Date", null);
                    standardProperties.Add("DefaultUIFont", null);
                    standardProperties.Add("DISABLEADVTSHORTCUTS", null);
                    standardProperties.Add("DISABLEMEDIA", null);
                    standardProperties.Add("DISABLEROLLBACK", null);
                    standardProperties.Add("DiskPrompt", null);
                    standardProperties.Add("DontRemoveTempFolderWhenFinished", null);
                    standardProperties.Add("EnableUserControl", null);
                    standardProperties.Add("EXECUTEACTION", null);
                    standardProperties.Add("EXECUTEMODE", null);
                    standardProperties.Add("FASTOEM", null);
                    standardProperties.Add("FILEADDDEFAULT", null);
                    standardProperties.Add("FILEADDLOCAL", null);
                    standardProperties.Add("FILEADDSOURCE", null);
                    standardProperties.Add("IncludeWholeFilesOnly", null);
                    standardProperties.Add("Installed", null);
                    standardProperties.Add("INSTALLLEVEL", null);
                    standardProperties.Add("Intel", null);
                    standardProperties.Add("Intel64", null);
                    standardProperties.Add("IsAdminPackage", null);
                    standardProperties.Add("LeftUnit", null);
                    standardProperties.Add("LIMITUI", null);
                    standardProperties.Add("ListOfPatchGUIDsToReplace", null);
                    standardProperties.Add("ListOfTargetProductCode", null);
                    standardProperties.Add("LOGACTION", null);
                    standardProperties.Add("LogonUser", null);
                    standardProperties.Add("Manufacturer", null);
                    standardProperties.Add("MEDIAPACKAGEPATH", null);
                    standardProperties.Add("MediaSourceDir", null);
                    standardProperties.Add("MinimumRequiredMsiVersion", null);
                    standardProperties.Add("MsiAMD64", null);
                    standardProperties.Add("MSIAPRSETTINGSIDENTIFIER", null);
                    standardProperties.Add("MSICHECKCRCS", null);
                    standardProperties.Add("MSIDISABLERMRESTART", null);
                    standardProperties.Add("MSIENFORCEUPGRADECOMPONENTRULES", null);
                    standardProperties.Add("MsiFileToUseToCreatePatchTables", null);
                    standardProperties.Add("MsiHiddenProperties", null);
                    standardProperties.Add("MSIINSTANCEGUID", null);
                    standardProperties.Add("MsiLogFileLocation", null);
                    standardProperties.Add("MsiLogging", null);
                    standardProperties.Add("MsiNetAssemblySupport", null);
                    standardProperties.Add("MSINEWINSTANCE", null);
                    standardProperties.Add("MSINODISABLEMEDIA", null);
                    standardProperties.Add("MsiNTProductType", null);
                    standardProperties.Add("MsiNTSuiteBackOffice", null);
                    standardProperties.Add("MsiNTSuiteDataCenter", null);
                    standardProperties.Add("MsiNTSuiteEnterprise", null);
                    standardProperties.Add("MsiNTSuiteSmallBusiness", null);
                    standardProperties.Add("MsiNTSuiteSmallBusinessRestricted", null);
                    standardProperties.Add("MsiNTSuiteWebServer", null);
                    standardProperties.Add("MsiNTSuitePersonal", null);
                    standardProperties.Add("MsiPatchRemovalList", null);
                    standardProperties.Add("MSIPATCHREMOVE", null);
                    standardProperties.Add("MSIRESTARTMANAGERCONTROL", null);
                    standardProperties.Add("MsiRestartManagerSessionKey", null);
                    standardProperties.Add("MSIRMSHUTDOWN", null);
                    standardProperties.Add("MsiRunningElevated", null);
                    standardProperties.Add("MsiUIHideCancel", null);
                    standardProperties.Add("MsiUIProgressOnly", null);
                    standardProperties.Add("MsiUISourceResOnly", null);
                    standardProperties.Add("MsiSystemRebootPending", null);
                    standardProperties.Add("MsiWin32AssemblySupport", null);
                    standardProperties.Add("NOCOMPANYNAME", null);
                    standardProperties.Add("NOUSERNAME", null);
                    standardProperties.Add("OLEAdvtSupport", null);
                    standardProperties.Add("OptimizePatchSizeForLargeFiles", null);
                    standardProperties.Add("OriginalDatabase", null);
                    standardProperties.Add("OutOfDiskSpace", null);
                    standardProperties.Add("OutOfNoRbDiskSpace", null);
                    standardProperties.Add("ParentOriginalDatabase", null);
                    standardProperties.Add("ParentProductCode", null);
                    standardProperties.Add("PATCH", null);
                    standardProperties.Add("PATCH_CACHE_DIR", null);
                    standardProperties.Add("PATCH_CACHE_ENABLED", null);
                    standardProperties.Add("PatchGUID", null);
                    standardProperties.Add("PATCHNEWPACKAGECODE", null);
                    standardProperties.Add("PATCHNEWSUMMARYCOMMENTS", null);
                    standardProperties.Add("PATCHNEWSUMMARYSUBJECT", null);
                    standardProperties.Add("PatchOutputPath", null);
                    standardProperties.Add("PatchSourceList", null);
                    standardProperties.Add("PhysicalMemory", null);
                    standardProperties.Add("PIDKEY", null);
                    standardProperties.Add("PIDTemplate", null);
                    standardProperties.Add("Preselected", null);
                    standardProperties.Add("PRIMARYFOLDER", null);
                    standardProperties.Add("PrimaryVolumePath", null);
                    standardProperties.Add("PrimaryVolumeSpaceAvailable", null);
                    standardProperties.Add("PrimaryVolumeSpaceRemaining", null);
                    standardProperties.Add("PrimaryVolumeSpaceRequired", null);
                    standardProperties.Add("Privileged", null);
                    standardProperties.Add("ProductCode", null);
                    standardProperties.Add("ProductID", null);
                    standardProperties.Add("ProductLanguage", null);
                    standardProperties.Add("ProductName", null);
                    standardProperties.Add("ProductState", null);
                    standardProperties.Add("ProductVersion", null);
                    standardProperties.Add("PROMPTROLLBACKCOST", null);
                    standardProperties.Add("REBOOT", null);
                    standardProperties.Add("REBOOTPROMPT", null);
                    standardProperties.Add("RedirectedDllSupport", null);
                    standardProperties.Add("REINSTALL", null);
                    standardProperties.Add("REINSTALLMODE", null);
                    standardProperties.Add("RemoveAdminTS", null);
                    standardProperties.Add("REMOVE", null);
                    standardProperties.Add("ReplacedInUseFiles", null);
                    standardProperties.Add("RestrictedUserControl", null);
                    standardProperties.Add("RESUME", null);
                    standardProperties.Add("RollbackDisabled", null);
                    standardProperties.Add("ROOTDRIVE", null);
                    standardProperties.Add("ScreenX", null);
                    standardProperties.Add("ScreenY", null);
                    standardProperties.Add("SecureCustomProperties", null);
                    standardProperties.Add("ServicePackLevel", null);
                    standardProperties.Add("ServicePackLevelMinor", null);
                    standardProperties.Add("SEQUENCE", null);
                    standardProperties.Add("SharedWindows", null);
                    standardProperties.Add("ShellAdvtSupport", null);
                    standardProperties.Add("SHORTFILENAMES", null);
                    standardProperties.Add("SourceDir", null);
                    standardProperties.Add("SOURCELIST", null);
                    standardProperties.Add("SystemLanguageID", null);
                    standardProperties.Add("TARGETDIR", null);
                    standardProperties.Add("TerminalServer", null);
                    standardProperties.Add("TextHeight", null);
                    standardProperties.Add("Time", null);
                    standardProperties.Add("TRANSFORMS", null);
                    standardProperties.Add("TRANSFORMSATSOURCE", null);
                    standardProperties.Add("TRANSFORMSSECURE", null);
                    standardProperties.Add("TTCSupport", null);
                    standardProperties.Add("UILevel", null);
                    standardProperties.Add("UpdateStarted", null);
                    standardProperties.Add("UpgradeCode", null);
                    standardProperties.Add("UPGRADINGPRODUCTCODE", null);
                    standardProperties.Add("UserLanguageID", null);
                    standardProperties.Add("USERNAME", null);
                    standardProperties.Add("UserSID", null);
                    standardProperties.Add("Version9X", null);
                    standardProperties.Add("VersionDatabase", null);
                    standardProperties.Add("VersionMsi", null);
                    standardProperties.Add("VersionNT", null);
                    standardProperties.Add("VersionNT64", null);
                    standardProperties.Add("VirtualMemory", null);
                    standardProperties.Add("WindowsBuild", null);
                    standardProperties.Add("WindowsVolume", null);
                }
            }

            return standardProperties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Enables an external user-interface handler.
        /// </summary>
        /// <param name="installUIHandler">Specifies a callback function.</param>
        /// <param name="messageFilter">Specifies which messages to handle using the external message handler.</param>
        /// <param name="context">Pointer to an application context that is passed to the callback function.</param>
        /// <returns>The return value is the previously set external handler, or null if there was no previously set handler.</returns>
        internal static InstallUIHandler SetExternalUI(InstallUIHandler installUIHandler, int messageFilter, IntPtr context)
        {
            return MsiInterop.MsiSetExternalUI(installUIHandler, messageFilter, context);
        }

        /// <summary>
        /// Enables the installer's internal user interface.
        /// </summary>
        /// <param name="uiLevel">Specifies the level of complexity of the user interface.</param>
        /// <param name="hwnd">Pointer to a window. This window becomes the owner of any user interface created.</param>
        /// <returns>The previous user interface level is returned. If an invalid dwUILevel is passed, then INSTALLUILEVEL_NOCHANGE is returned.</returns>
        internal static int SetInternalUI(int uiLevel, ref IntPtr hwnd)
        {
            return MsiInterop.MsiSetInternalUI(uiLevel, ref hwnd);
        }

        /// <summary>
        /// Get the source/target and short/long file names from an MSI Filename column.
        /// </summary>
        /// <param name="value">The Filename value.</param>
        /// <returns>An array of strings of length 4.  The contents are: short target, long target, short source, and long source.</returns>
        /// <remarks>
        /// If any particular file name part is not parsed, its set to null in the appropriate location of the returned array of strings.
        /// However, the returned array will always be of length 4.
        /// </remarks>
        internal static string[] GetNames(string value)
        {
            string[] names = new string[4];
            int targetSeparator = value.IndexOf(":");

            // split source and target
            string sourceName = null;
            string targetName = value;
            if (0 <= targetSeparator)
            {
                sourceName = value.Substring(targetSeparator + 1);
                targetName = value.Substring(0, targetSeparator);
            }

            // split the source short and long names
            string sourceLongName = null;
            if (null != sourceName)
            {
                int sourceLongNameSeparator = sourceName.IndexOf("|");
                if (0 <= sourceLongNameSeparator)
                {
                    sourceLongName = sourceName.Substring(sourceLongNameSeparator + 1);
                    sourceName = sourceName.Substring(0, sourceLongNameSeparator);
                }
            }

            // split the target short and long names
            int targetLongNameSeparator = targetName.IndexOf("|");
            string targetLongName = null;
            if (0 <= targetLongNameSeparator)
            {
                targetLongName = targetName.Substring(targetLongNameSeparator + 1);
                targetName = targetName.Substring(0, targetLongNameSeparator);
            }

            // remove the long source name when its identical to the long source name
            if (null != sourceName && sourceName == sourceLongName)
            {
                sourceLongName = null;
            }

            // remove the long target name when its identical to the long target name
            if (null != targetName && targetName == targetLongName)
            {
                targetLongName = null;
            }

            // remove the source names when they are identical to the target names
            if (sourceName == targetName && sourceLongName == targetLongName)
            {
                sourceName = null;
                sourceLongName = null;
            }

            // target name(s)
            if ("." != targetName)
            {
                names[0] = targetName;
            }

            if (null != targetLongName && "." != targetLongName)
            {
                names[1] = targetLongName;
            }

            // source name(s)
            if (null != sourceName)
            {
                names[2] = sourceName;
            }

            if (null != sourceLongName && "." != sourceLongName)
            {
                names[3] = sourceLongName;
            }

            return names;
        }

        /// <summary>
        /// Get a source/target and short/long file name from an MSI Filename column.
        /// </summary>
        /// <param name="value">The Filename value.</param>
        /// <param name="source">true to get a source name; false to get a target name</param>
        /// <param name="longName">true to get a long name; false to get a short name</param>
        /// <returns>The name.</returns>
        internal static string GetName(string value, bool source, bool longName)
        {
            string[] names = GetNames(value);

            if (source)
            {
                if (longName && null != names[3])
                {
                    return names[3];
                }
                else if (null != names[2])
                {
                    return names[2];
                }
            }

            if (longName && null != names[1])
            {
                return names[1];
            }
            else
            {
                return names[0];
            }
        }

        private static void LoadStandardDirectories()
        {
            lock (lockObject)
            {
                if (null == standardDirectories)
                {
                    standardDirectories = new Hashtable();
                    standardDirectories.Add("TARGETDIR", null);
                    standardDirectories.Add("AdminToolsFolder", null);
                    standardDirectories.Add("AppDataFolder", null);
                    standardDirectories.Add("CommonAppDataFolder", null);
                    standardDirectories.Add("CommonFilesFolder", null);
                    standardDirectories.Add("DesktopFolder", null);
                    standardDirectories.Add("FavoritesFolder", null);
                    standardDirectories.Add("FontsFolder", null);
                    standardDirectories.Add("LocalAppDataFolder", null);
                    standardDirectories.Add("MyPicturesFolder", null);
                    standardDirectories.Add("PersonalFolder", null);
                    standardDirectories.Add("ProgramFilesFolder", null);
                    standardDirectories.Add("ProgramMenuFolder", null);
                    standardDirectories.Add("SendToFolder", null);
                    standardDirectories.Add("StartMenuFolder", null);
                    standardDirectories.Add("StartupFolder", null);
                    standardDirectories.Add("System16Folder", null);
                    standardDirectories.Add("SystemFolder", null);
                    standardDirectories.Add("TempFolder", null);
                    standardDirectories.Add("TemplateFolder", null);
                    standardDirectories.Add("WindowsFolder", null);
                    standardDirectories.Add("CommonFiles64Folder", null);
                    standardDirectories.Add("ProgramFiles64Folder", null);
                    standardDirectories.Add("System64Folder", null);
                }
            }
        }
    }
}
