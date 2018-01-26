//--------------------------------------------------------------------------------------------------
// <copyright file="WixBuildMacroCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixBuildMacroCollection class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using EnvDTE;
    using Microsoft.Build.BuildEngine;
    using Microsoft.VisualStudio.Package;

    /// <summary>
    /// Collection class for a series of solution and project variables with their associated
    /// values. For example: SolutionDir=C:\MySolution, ProjectName=MyProject, etc.
    /// </summary>
    public class WixBuildMacroCollection : ICollection, IEnumerable<WixBuildMacroCollection.MacroNameValuePair>
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private static readonly string[] globalMacroNames =
            {
                WixProjectFileConstants.DevEnvDir,
                WixProjectFileConstants.SolutionDir,
                WixProjectFileConstants.SolutionExt,
                WixProjectFileConstants.SolutionName,
                WixProjectFileConstants.SolutionFileName,
                WixProjectFileConstants.SolutionPath,
            };

        private static readonly string[] macroNames =
            {
                "ConfigurationName",
                "OutDir",
                "PlatformName",
                "ProjectDir",
                "ProjectExt",
                "ProjectFileName",
                "ProjectName",
                "ProjectPath",
                "TargetDir",
                "TargetExt",
                "TargetFileName",
                "TargetName",
                "TargetPath",
            };

        private SortedList<string, string> list = new SortedList<string, string>(macroNames.Length, StringComparer.OrdinalIgnoreCase);

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixBuildMacroCollection"/> class.
        /// </summary>
        /// <param name="project">The project from which to read the properties.</param>
        public WixBuildMacroCollection(WixProjectNode project)
        {
            WixHelperMethods.VerifyNonNullArgument(project, "project");

            // get the global SolutionX properties
            project.DefineSolutionProperties();
            foreach (string globalMacroName in globalMacroNames)
            {
                BuildProperty property = project.BuildEngine.GlobalProperties[globalMacroName];
                if (property == null)
                {
                    this.list.Add(globalMacroName, "*Undefined*");
                }
                else
                {
                    string value = property.FinalValue;
                    this.list.Add(globalMacroName, value);
                }
            }

            // we need to call GetTargetPath first so that TargetDir and TargetPath are resolved correctly
            string config = Microsoft.VisualStudio.Package.Utilities.GetActiveConfigurationName(project.GetAutomationObject() as EnvDTE.Project);
            bool builtSuccessfully = (project.Build(config, WixProjectFileConstants.MsBuildTarget.GetTargetPath) == MSBuildResult.Sucessful);

            // get the ProjectX and TargetX variables
            foreach (string macroName in macroNames)
            {
                string value = project.GetProjectProperty(macroName);
                this.list.Add(macroName, value);
            }
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)this.list).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)this.list).SyncRoot; }
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)this.list).CopyTo(array, index);
        }

        IEnumerator<MacroNameValuePair> IEnumerable<MacroNameValuePair>.GetEnumerator()
        {
            foreach (KeyValuePair<string, string> pair in this.list)
            {
                yield return (MacroNameValuePair)pair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<MacroNameValuePair>)this).GetEnumerator();
        }

        // =========================================================================================
        // Classes
        // =========================================================================================

        /// <summary>
        /// Defines a macro name/value pair that can be set or retrieved.
        /// </summary>
        public struct MacroNameValuePair
        {
            private KeyValuePair<string, string> pair;

            /// <summary>
            /// Initializes a new instance of the <see cref="MacroNameValuePair"/> class.
            /// </summary>
            /// <param name="macroName">The macro name.</param>
            /// <param name="value">The macro's value.</param>
            public MacroNameValuePair(string macroName, string value)
            {
                this.pair = new KeyValuePair<string,string>(macroName, value);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MacroNameValuePair"/> class.
            /// </summary>
            /// <param name="pair">The KeyValuePair&lt;string, string&gt; to store.</param>
            private MacroNameValuePair(KeyValuePair<string, string> pair)
            {
                this.pair = pair;
            }

            /// <summary>
            /// Gets the macro name in the macro name/value pair.
            /// </summary>
            public string MacroName
            {
                get { return this.pair.Key; }
            }

            /// <summary>
            /// Gets the value in the macro name/value pair.
            /// </summary>
            public string Value
            {
                get { return this.pair.Value; }
            }

            /// <summary>
            /// Converts a <see cref="KeyValuePair&lt;T, T&gt;">KeyValuePair&lt;string, string&gt;</see>
            /// to a <see cref="MacroNameValuePair"/>.
            /// </summary>
            /// <param name="source">The KeyValuePair&lt;string, string&gt; to convert.</param>
            /// <returns>The converted <see cref="MacroNameValuePair"/>.</returns>
            public static implicit operator MacroNameValuePair(KeyValuePair<string, string> source)
            {
                return new MacroNameValuePair(source);
            }
        }
    }
}
