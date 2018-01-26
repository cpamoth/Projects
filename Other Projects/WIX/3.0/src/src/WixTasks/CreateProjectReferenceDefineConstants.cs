//-------------------------------------------------------------------------------------------------
// <copyright file="CreateProjectReferenceDefineConstants.cs" company="Microsoft">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;
    using System.Text;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// An MSBuild task to create a list of preprocessor defines to be passed to candle from the
    /// list of referenced projects.
    /// </summary>
    public sealed class CreateProjectReferenceDefineConstants : Task
    {
        private ITaskItem[] defineConstants;
        private ITaskItem[] projectReferencePaths;

        [Output]
        public ITaskItem[] DefineConstants
        {
            get { return this.defineConstants; }
        }

        [Required]
        public ITaskItem[] ProjectReferencePaths
        {
            get { return this.projectReferencePaths; }
            set { this.projectReferencePaths = value; }
        }

        public override bool Execute()
        {
            List<ITaskItem> outputItems = new List<ITaskItem>();

            foreach (ITaskItem item in this.ProjectReferencePaths)
            {
                string configuration = item.GetMetadata("Configuration");
                string fullConfiguration = item.GetMetadata("FullConfiguration");
                string platform = item.GetMetadata("Platform");

                string projectPath = item.GetMetadata("MSBuildSourceProjectFileFullPath");
                string projectDir = Path.GetDirectoryName(projectPath) + Path.DirectorySeparatorChar;
                string projectExt = Path.GetExtension(projectPath);
                string projectFileName = Path.GetFileName(projectPath);
                string projectName = Path.GetFileNameWithoutExtension(projectPath);

                string targetPath = item.GetMetadata("FullPath");
                string targetDir = Path.GetDirectoryName(targetPath) + Path.DirectorySeparatorChar;
                string targetExt = Path.GetExtension(targetPath);
                string targetFileName = Path.GetFileName(targetPath);
                string targetName = Path.GetFileNameWithoutExtension(targetPath);

                // write out the platform/configuration defines
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.Configuration={1}", projectName, configuration)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.FullConfiguration={1}", projectName, fullConfiguration)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.Platform={1}", projectName, platform)));

                // write out the ProjectX defines
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.ProjectDir={1}", projectName, projectDir)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.ProjectExt={1}", projectName, projectExt)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.ProjectFileName={1}", projectName, projectFileName)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.ProjectName={1}", projectName, projectName)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.ProjectPath={1}", projectName, projectPath)));

                // write out the TargetX defines
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.TargetDir={1}", projectName, targetDir)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.TargetExt={1}", projectName, targetExt)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.TargetFileName={1}", projectName, targetFileName)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.TargetName={1}", projectName, targetName)));
                outputItems.Add(new TaskItem(String.Format(CultureInfo.InvariantCulture, "{0}.TargetPath={1}", projectName, targetPath)));
            }

            this.defineConstants = outputItems.ToArray();

            return true;
        }
    }
}
