//-------------------------------------------------------------------------------------------------
// <copyright file="CandleTask.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Build task to execute the compiler of the Windows Installer Xml toolset.
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
    /// An MSBuild task to run the WiX compiler.
    /// </summary>
    public sealed class Candle : ToolTask
    {
        private const string CandleToolName = "candle.exe";

        private string[] defineConstants;
        private ITaskItem[] extensions;
        private string[] includeSearchPaths;
        private bool noLogo;
        private ITaskItem outputFile;
        private bool pedantic;
        private string preprocessToFile;
        private bool preprocessToStdOut;
        private bool showSourceTrace;
        private ITaskItem[] sourceFiles;
        private bool suppressSchemaValidation;
        private string[] suppressSpecificWarnings;
        private bool treatWarningsAsErrors;
        private bool verboseOutput;

        public string[] DefineConstants
        {
            get { return this.defineConstants; }
            set { this.defineConstants = value; }
        }

        public ITaskItem[] Extensions
        {
            get { return this.extensions; }
            set { this.extensions = value; }
        }

        public string[] IncludeSearchPaths
        {
            get { return this.includeSearchPaths; }
            set { this.includeSearchPaths = value; }
        }

        public bool NoLogo
        {
            get { return this.noLogo; }
            set { this.noLogo = value; }
        }

        [Output]
        [Required]
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

        public string PreprocessToFile
        {
            get { return this.preprocessToFile; }
            set { this.preprocessToFile = value; }
        }

        public bool PreprocessToStdOut
        {
            get { return this.preprocessToStdOut; }
            set { this.preprocessToStdOut = value; }
        }

        public bool ShowSourceTrace
        {
            get { return this.showSourceTrace; }
            set { this.showSourceTrace = value; }
        }

        [Required]
        public ITaskItem[] SourceFiles
        {
            get { return this.sourceFiles; }
            set { this.sourceFiles = value; }
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
        /// <remarks>The ToolName is used with the ToolPath to get the location of candle.exe.</remarks>
        /// <value>The name of the executable.</value>
        protected override string ToolName
        {
            get { return CandleToolName; }
        }

        /// <summary>
        /// Get the path to the executable. 
        /// </summary>
        /// <remarks>GetFullPathToTool is only called when the ToolPath property is not set (see the ToolName remarks above).</remarks>
        /// <returns>The full path to the executable or simply candle.exe if it's expected to be in the system path.</returns>
        protected override string GenerateFullPathToTool()
        {
            // If there's not a ToolPath specified, it has to be in the system path.
            if (String.IsNullOrEmpty(this.ToolPath))
            {
                return CandleToolName;
            }

            return Path.Combine(Path.GetFullPath(this.ToolPath), CandleToolName);
        }

        /// <summary>
        /// Generate the command line arguments to write to the response file from the properties.
        /// </summary>
        /// <returns>Command line string.</returns>
        protected override string GenerateResponseFileCommands()
        {
            WixCommandLineBuilder commandLineBuilder = new WixCommandLineBuilder();

            commandLineBuilder.AppendArrayIfNotNull("-d", this.DefineConstants);
            commandLineBuilder.AppendIfTrue("-p", this.PreprocessToStdOut);
            commandLineBuilder.AppendSwitchIfNotNull("-p", this.PreprocessToFile);
            commandLineBuilder.AppendArrayIfNotNull("-I", this.IncludeSearchPaths);
            commandLineBuilder.AppendIfTrue("-nologo", this.NoLogo);
            commandLineBuilder.AppendSwitchIfNotNull("-out ", this.OutputFile);
            commandLineBuilder.AppendIfTrue("-pedantic", this.Pedantic);
            commandLineBuilder.AppendIfTrue("-ss", this.SuppressSchemaValidation);
            commandLineBuilder.AppendIfTrue("-trace", this.ShowSourceTrace);
            commandLineBuilder.AppendExtensions(this.Extensions, this.Log);
            commandLineBuilder.AppendArrayIfNotNull("-sw", this.SuppressSpecificWarnings);
            commandLineBuilder.AppendIfTrue("-wx", this.TreatWarningsAsErrors);
            commandLineBuilder.AppendIfTrue("-v", this.VerboseOutput);
            commandLineBuilder.AppendFileNamesIfNotNull(this.SourceFiles, " ");
            
            return commandLineBuilder.ToString();
        }
    }
}
