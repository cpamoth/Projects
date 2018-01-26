//-------------------------------------------------------------------------------------------------
// <copyright file="WixCommandLineBuilder.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Improved CommandLineBuilder with additional calls.
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
    /// Helper class for appending the command line arguments.
    /// </summary>
    internal class WixCommandLineBuilder : CommandLineBuilder
    {
        internal const int Unspecified = -1;
        
        /// <summary>
        /// Append a switch to the command line if the value has been specified.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="value">Value specified by the user.</param>
        public void AppendIfSpecified(string switchName, int value)
        {
            if (value != Unspecified)
            {
                this.AppendSwitchIfNotNull(switchName, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Append a switch to the command line if the condition is true.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="condition">Condition specified by the user.</param>
        public void AppendIfTrue(string switchName, bool condition)
        {
            if (condition)
            {
                this.AppendSwitch(switchName);
            }
        }

        /// <summary>
        /// Append a switch to the command line if any values in the array have been specified.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="values">Values specified by the user.</param>
        public void AppendArrayIfNotNull(string switchName, ITaskItem[] values)
        {
            if (values != null)
            {
                foreach (ITaskItem value in values)
                {
                    this.AppendSwitchIfNotNull(switchName, value);
                }
            }
        }

        /// <summary>
        /// Append a switch to the command line if any values in the array have been specified.
        /// </summary>
        /// <param name="switchName">Switch to append.</param>
        /// <param name="values">Values specified by the user.</param>
        public void AppendArrayIfNotNull(string switchName, string[] values)
        {
            if (values != null)
            {
                foreach (string value in values)
                {
                    this.AppendSwitchIfNotNull(switchName, value);
                }
            }
        }

        /// <summary>
        /// Build the extensions argument.
        /// </summary>
        /// <param name="extensions">The list of extensions to include.</param>
        /// <param name="log">The logger to use.</param>
        public void AppendExtensions(ITaskItem[] extensions, TaskLoggingHelper log)
        {
            if (extensions == null)
            {
                return;
            }

            foreach (ITaskItem extension in extensions)
            {
                string className = extension.GetMetadata("Class");

                if (String.IsNullOrEmpty(className))
                {
                    this.AppendSwitchIfNotNull("-ext ", extension);
                }
                else
                {
                    this.AppendSwitchIfNotNull("-ext ", className + ", " + extension.ItemSpec);
                }
            }
        }
    }
}