//-------------------------------------------------------------------------------------------------
// <copyright file="ProcessUnit.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Runs a process as part of a Windows Installer XML WixUnit test.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Unit
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Xml;

    using Microsoft.Tools.WindowsInstallerXml.Unit;

    /// <summary>
    /// Runs a process as a Windows Installer XML WixUnit test.
    /// </summary>
    internal class ProcessUnit
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class.
        /// </summary>
        private ProcessUnit()
        {
        }

        /// <summary>
        /// Run a Process unit test.
        /// </summary>
        /// <param name="element">The unit test element.</param>
        /// <param name="previousUnitResults">The previous unit test results.</param>
        public static void RunUnitTest(XmlElement element, UnitResults previousUnitResults)
        {
            string arguments = element.GetAttribute("Arguments");
            string executable = Environment.ExpandEnvironmentVariables(element.GetAttribute("Executable"));
            string expectedReturnCode = element.GetAttribute("ExpectedReturnCode");
            string workingDirectory = element.GetAttribute("WorkingDirectory");

            bool compareReturnCodes = false;

            // Check if an ExpectedReturnCode was set
            if (null != expectedReturnCode && String.Empty != expectedReturnCode)
            {
                compareReturnCodes = true;
            }

            // Set the current working directory if one was specified
            string currentDirectory = Environment.CurrentDirectory;

            if (null != workingDirectory && String.Empty != workingDirectory)
            {
                Environment.CurrentDirectory = workingDirectory;
            }

            // Run the process
            int actualReturnCode;
            ArrayList output = ToolUtility.RunTool(executable, arguments, out actualReturnCode);
            Environment.CurrentDirectory = currentDirectory;

            previousUnitResults.Output.AddRange(output);

            // Check the results
            if (compareReturnCodes)
            {
                if (actualReturnCode == Convert.ToInt32(expectedReturnCode))
                {
                    previousUnitResults.Output.Add(String.Format("Actual return code {0} matched expected return code {1}", actualReturnCode, expectedReturnCode));
                }
                else
                {
                    previousUnitResults.Errors.Add(String.Format("Actual return code {0} did not match expected return code {1}", actualReturnCode, expectedReturnCode));
                    previousUnitResults.Output.Add(String.Format("Actual return code {0} did not match expected return code {1}", actualReturnCode, expectedReturnCode));
                }
            }
        }
    }
}
