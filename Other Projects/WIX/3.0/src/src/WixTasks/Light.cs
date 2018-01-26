//-------------------------------------------------------------------------------------------------
// <copyright file="Light.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Build task to execute the linker of the Windows Installer Xml toolset.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Build.Tasks
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// An MSBuild task to run the WiX linker.
    /// </summary>
    public sealed class Light : ToolTask
    {
        private const string LightToolName = "Light.exe";

        private bool allowIdenticalRows;
        private bool allowUnresolvedReferences;
        private string[] baseInputPaths;
        private bool bindFiles;
        private string cabinetCachePath;
        private int cabinetCreationThreadCount = WixCommandLineBuilder.Unspecified;
        private string cultures;
        private ITaskItem[] extensions;
        private bool leaveTemporaryFiles;
        private ITaskItem[] localizationFiles;
        private bool noLogo;
        private ITaskItem[] objectFiles;
        private bool outputAsXml;
        private ITaskItem outputFile;
        private bool pedantic;
        private bool reuseCabinetCache;
        private bool setMsiAssemblyNameFileVersion;
        private bool suppressAclReset;
        private bool suppressAssemblies;
        private bool suppressDefaultAdminSequenceActions;
        private bool suppressDefaultAdvSequenceActions;
        private bool suppressDefaultUISequenceActions;
        private bool suppressDroppingUnrealTables;
        private bool suppressFileHashAndInfo;
        private bool suppressFiles;
        private bool suppressIntermediateFileVersionMatching;
        private string[] suppressIces;
        private bool suppressLayout;
        private bool suppressMsiAssemblyTableProcessing;
        private bool suppressSchemaValidation;
        private bool suppressValidation;
        private string[] suppressSpecificWarnings;
        private bool tagSectionIdAttributeOnTuples;
        private bool treatWarningsAsErrors;
        private ITaskItem unreferencedSymbolsFile;
        private bool verboseOutput;
        private string[] wixVariables;

        public bool AllowIdenticalRows
        {
            get { return this.allowIdenticalRows; }
            set { this.allowIdenticalRows = value; }
        }

        public bool AllowUnresolvedReferences
        {
            get { return this.allowUnresolvedReferences; }
            set { this.allowUnresolvedReferences = value; }
        }

        public string[] BaseInputPaths
        {
            get { return this.baseInputPaths; }
            set { this.baseInputPaths = value; }
        }

        public bool BindFiles
        {
            get { return this.bindFiles; }
            set { this.bindFiles = value; }
        }

        public string CabinetCachePath
        {
            get { return this.cabinetCachePath; }
            set { this.cabinetCachePath = value; }
        }

        public int CabinetCreationThreadCount
        {
            get { return this.cabinetCreationThreadCount; }
            set { this.cabinetCreationThreadCount = value; }
        }

        public string Cultures
        {
            get { return this.cultures; }
            set { this.cultures = value; }
        }

        public ITaskItem[] Extensions
        {
            get { return this.extensions; }
            set { this.extensions = value; }
        }

        public bool LeaveTemporaryFiles
        {
            get { return this.leaveTemporaryFiles; }
            set { this.leaveTemporaryFiles = value; }
        }

        public ITaskItem[] LocalizationFiles
        {
            get { return this.localizationFiles; }
            set { this.localizationFiles = value; }
        }

        public bool NoLogo
        {
            get { return this.noLogo; }
            set { this.noLogo = value; }
        }

        [Required]
        public ITaskItem[] ObjectFiles
        {
            get { return this.objectFiles; }
            set { this.objectFiles = value; }
        }

        public bool OutputAsXml
        {
            get { return this.outputAsXml; }
            set { this.outputAsXml = value; }
        }

        [Required]
        [Output]
        public ITaskItem OutputFile
        {
            get { return this.outputFile; }
            set { this.outputFile = value; }
        }

        public bool Pedantic
        {
            get { return this.pedantic; }
            set { this.pedantic = value; }
        }

        public bool ReuseCabinetCache
        {
            get { return this.reuseCabinetCache; }
            set { this.reuseCabinetCache = value; }
        }

        public bool SetMsiAssemblyNameFileVersion
        {
            get { return this.setMsiAssemblyNameFileVersion; }
            set { this.setMsiAssemblyNameFileVersion = value; }
        }

        public bool SuppressAclReset
        {
            get { return this.suppressAclReset; }
            set { this.suppressAclReset = value; }
        }

        public bool SuppressAssemblies
        {
            get { return this.suppressAssemblies; }
            set { this.suppressAssemblies = value; }
        }

        public bool SuppressDefaultAdminSequenceActions
        {
            get { return this.suppressDefaultAdminSequenceActions; }
            set { this.suppressDefaultAdminSequenceActions = value; }
        }

        public bool SuppressDefaultAdvSequenceActions
        {
            get { return this.suppressDefaultAdvSequenceActions; }
            set { this.suppressDefaultAdvSequenceActions = value; }
        }

        public bool SuppressDefaultUISequenceActions
        {
            get { return this.suppressDefaultUISequenceActions; }
            set { this.suppressDefaultUISequenceActions = value; }
        }

        public bool SuppressDroppingUnrealTables
        {
            get { return this.suppressDroppingUnrealTables; }
            set { this.suppressDroppingUnrealTables = value; }
        }

        public bool SuppressFileHashAndInfo
        {
            get { return this.suppressFileHashAndInfo; }
            set { this.suppressFileHashAndInfo = value; }
        }

        public bool SuppressFiles
        {
            get { return this.suppressFiles; }
            set { this.suppressFiles = value; }
        }

        public bool SuppressIntermediateFileVersionMatching
        {
            get { return this.suppressIntermediateFileVersionMatching; }
            set { this.suppressIntermediateFileVersionMatching = value; }
        }

        public string[] SuppressIces
        {
            get { return this.suppressIces; }
            set { this.suppressIces = value; }
        }

        public bool SuppressLayout
        {
            get { return this.suppressLayout; }
            set { this.suppressLayout = value; }
        }

        public bool SuppressMsiAssemblyTableProcessing
        {
            get { return this.suppressMsiAssemblyTableProcessing; }
            set { this.suppressMsiAssemblyTableProcessing = value; }
        }

        public bool SuppressSchemaValidation
        {
            get { return this.suppressSchemaValidation; }
            set { this.suppressSchemaValidation = value; }
        }

        public string[] SuppressSpecificWarnings
        {
            get { return this.suppressSpecificWarnings; }
            set { this.suppressSpecificWarnings = value; }
        }

        public bool SuppressValidation
        {
            get { return this.suppressValidation; }
            set { this.suppressValidation = value; }
        }

        public bool TagSectionIdAttributeOnTuples
        {
            get { return this.tagSectionIdAttributeOnTuples; }
            set { this.tagSectionIdAttributeOnTuples = value; }
        }

        public bool TreatWarningsAsErrors
        {
            get { return this.treatWarningsAsErrors; }
            set { this.treatWarningsAsErrors = value; }
        }

        public ITaskItem UnreferencedSymbolsFile
        {
            get { return this.unreferencedSymbolsFile; }
            set { this.unreferencedSymbolsFile = value; }
        }

        public bool VerboseOutput
        {
            get { return this.verboseOutput; }
            set { this.verboseOutput = value; }
        }

        public string[] WixVariables
        {
            get { return this.wixVariables; }
            set { this.wixVariables = value; }
        }

        /// <summary>
        /// Get the name of the executable.
        /// </summary>
        /// <remarks>The ToolName is used with the ToolPath to get the location of light.exe.</remarks>
        /// <value>The name of the executable.</value>
        protected override string ToolName
        {
            get { return LightToolName; }
        }

        /// <summary>
        /// Get the path to the executable. 
        /// </summary>
        /// <remarks>GetFullPathToTool is only called when the ToolPath property is not set (see the ToolName remarks above).</remarks>
        /// <returns>The full path to the executable or simply light.exe if it's expected to be in the system path.</returns>
        protected override string GenerateFullPathToTool()
        {
            // If there's not a ToolPath specified, it has to be in the system path.
            if (String.IsNullOrEmpty(this.ToolPath))
            {
                return LightToolName;
            }

            return Path.Combine(Path.GetFullPath(this.ToolPath), LightToolName);
        }

        /// <summary>
        /// Generate the command line arguments to write to the response file from the properties.
        /// </summary>
        /// <returns>Command line string.</returns>
        protected override string GenerateResponseFileCommands()
        {
            WixCommandLineBuilder commandLineBuilder = new WixCommandLineBuilder();

            commandLineBuilder.AppendIfTrue("-ai", this.AllowIdenticalRows);
            commandLineBuilder.AppendIfTrue("-au", this.AllowUnresolvedReferences);
            commandLineBuilder.AppendArrayIfNotNull("-b ", this.BaseInputPaths);
            commandLineBuilder.AppendIfTrue("-bf", this.BindFiles);
            commandLineBuilder.AppendSwitchIfNotNull("-cc ", this.CabinetCachePath);
            commandLineBuilder.AppendIfSpecified("-ct ", this.CabinetCreationThreadCount);
            commandLineBuilder.AppendSwitchIfNotNull("-cultures:", this.Cultures);
            commandLineBuilder.AppendArrayIfNotNull("-d", this.WixVariables);
            commandLineBuilder.AppendExtensions(this.Extensions, this.Log);
            commandLineBuilder.AppendIfTrue("-fv", this.SetMsiAssemblyNameFileVersion);
            commandLineBuilder.AppendArrayIfNotNull("-loc ", this.LocalizationFiles);
            commandLineBuilder.AppendIfTrue("-nologo", this.NoLogo);
            commandLineBuilder.AppendIfTrue("-notidy", this.LeaveTemporaryFiles);
            commandLineBuilder.AppendSwitchIfNotNull("-out ", this.OutputFile);
            commandLineBuilder.AppendIfTrue("-pedantic", this.Pedantic);
            commandLineBuilder.AppendIfTrue("-reusecab", this.ReuseCabinetCache);
            commandLineBuilder.AppendIfTrue("-sa", this.SuppressAssemblies);
            commandLineBuilder.AppendIfTrue("-sacl", this.SuppressAclReset);
            commandLineBuilder.AppendIfTrue("-sadmin", this.SuppressDefaultAdminSequenceActions);
            commandLineBuilder.AppendIfTrue("-sadv", this.SuppressDefaultAdvSequenceActions);
            commandLineBuilder.AppendIfTrue("-sdut", this.SuppressDroppingUnrealTables);
            commandLineBuilder.AppendArrayIfNotNull("-sice:", this.SuppressIces);
            commandLineBuilder.AppendIfTrue("-sma", this.SuppressMsiAssemblyTableProcessing);
            commandLineBuilder.AppendIfTrue("-sf", this.SuppressFiles);
            commandLineBuilder.AppendIfTrue("-sh", this.SuppressFileHashAndInfo);
            commandLineBuilder.AppendIfTrue("-sl", this.SuppressLayout);
            commandLineBuilder.AppendIfTrue("-ss", this.SuppressSchemaValidation);
            commandLineBuilder.AppendIfTrue("-sui", this.SuppressDefaultUISequenceActions);
            commandLineBuilder.AppendIfTrue("-sv", this.SuppressIntermediateFileVersionMatching);
            commandLineBuilder.AppendIfTrue("-sval", this.SuppressValidation);
            commandLineBuilder.AppendArrayIfNotNull("-sw", this.SuppressSpecificWarnings);
            commandLineBuilder.AppendIfTrue("-ts", this.TagSectionIdAttributeOnTuples);
            commandLineBuilder.AppendSwitchIfNotNull("-usf", this.UnreferencedSymbolsFile);
            commandLineBuilder.AppendIfTrue("-v", this.VerboseOutput);
            commandLineBuilder.AppendIfTrue("-wx", this.TreatWarningsAsErrors);
            commandLineBuilder.AppendIfTrue("-xo", this.OutputAsXml);
            commandLineBuilder.AppendFileNamesIfNotNull(this.ObjectFiles, " ");

            return commandLineBuilder.ToString();
        }
    }
}
