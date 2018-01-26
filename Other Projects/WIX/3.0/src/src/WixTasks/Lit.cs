//-------------------------------------------------------------------------------------------------
// <copyright file="Lit.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Build task to execute the lib tool of the Windows Installer Xml toolset.
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
    /// An MSBuild task to run the WiX lib tool.
    /// </summary>
    public sealed class Lit : ToolTask
    {
        private const string LitToolName = "lit.exe";

        private bool bindFiles;
        private ITaskItem[] extensions;
        private ITaskItem[] localizationFiles;
        private bool noLogo;
        private ITaskItem[] objectFiles;
        private ITaskItem outputFile;
        private bool suppressIntermediateFileVersionMatching;
        private bool suppressSchemaValidation;
        private string[] suppressSpecificWarnings;
        private bool treatWarningsAsErrors;
        private bool verboseOutput;

        public bool BindFiles
        {
            get { return this.bindFiles; }
            set { this.bindFiles = value; }
        }

        public ITaskItem[] Extensions
        {
            get { return this.extensions; }
            set { this.extensions = value; }
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

        [Required]
        [Output]
        public ITaskItem OutputFile
        {
            get { return this.outputFile; }
            set { this.outputFile = value; }
        }

        public bool SuppressIntermediateFileVersionMatching
        {
            get { return this.suppressIntermediateFileVersionMatching; }
            set { this.suppressIntermediateFileVersionMatching = true; }
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

        public bool TreatWarningsAsErrors
        {
            get { return this.treatWarningsAsErrors; }
            set { this.treatWarningsAsErrors = value; }
        }

        public bool VerboseOutput
        {
            get { return this.verboseOutput; }
            set { this.verboseOutput = value; }
        }

        /// <summary>
        /// Get the name of the executable.
        /// </summary>
        /// <remarks>The ToolName is used with the ToolPath to get the location of lit.exe</remarks>
        /// <value>The name of the executable.</value>
        protected override string ToolName
        {
            get { return LitToolName; }
        }

        /// <summary>
        /// Get the path to the executable. 
        /// </summary>
        /// <remarks>GetFullPathToTool is only called when the ToolPath property is not set (see the ToolName remarks above).</remarks>
        /// <returns>The full path to the executable or simply lit.exe if it's expected to be in the system path.</returns>
        protected override string GenerateFullPathToTool()
        {
            // If there's not a ToolPath specified, it has to be in the system path.
            if (String.IsNullOrEmpty(this.ToolPath))
            {
                return LitToolName;
            }

            return Path.Combine(Path.GetFullPath(this.ToolPath), LitToolName);
        }

        /// <summary>
        /// Generate the command line arguments to write to the response file from the properties.
        /// </summary>
        /// <returns>Command line string.</returns>
        protected override string GenerateResponseFileCommands()
        {
            WixCommandLineBuilder commandLineBuilder = new WixCommandLineBuilder();

            commandLineBuilder.AppendIfTrue("-nologo", this.NoLogo);
            commandLineBuilder.AppendSwitchIfNotNull("-out ", this.OutputFile);
            commandLineBuilder.AppendIfTrue("-bf", this.BindFiles);
            commandLineBuilder.AppendExtensions(this.extensions, this.Log);
            commandLineBuilder.AppendArrayIfNotNull("-loc ", this.LocalizationFiles);
            commandLineBuilder.AppendIfTrue("-ss", this.SuppressSchemaValidation);
            commandLineBuilder.AppendIfTrue("-sv", this.SuppressIntermediateFileVersionMatching);
            commandLineBuilder.AppendArrayIfNotNull("-sw", this.SuppressSpecificWarnings);
            commandLineBuilder.AppendIfTrue("-wx", this.TreatWarningsAsErrors);
            commandLineBuilder.AppendIfTrue("-v", this.VerboseOutput);
            commandLineBuilder.AppendFileNamesIfNotNull(this.ObjectFiles, " ");

            return commandLineBuilder.ToString();
        }
    }
}
