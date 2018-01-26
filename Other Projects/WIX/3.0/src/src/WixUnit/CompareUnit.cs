//-------------------------------------------------------------------------------------------------
// <copyright file="CompareUnit.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Compares to results files as part of a Windows Installer XML WixUnit test.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Unit
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    using Microsoft.Tools.WindowsInstallerXml.Unit;

    /// <summary>
    /// Compares to results files as part of a Windows Installer XML WixUnit test.
    /// </summary>
    internal class CompareUnit
    {
        /// <summary>
        /// Private constructor to prevent instantiation of static class.
        /// </summary>
        private CompareUnit()
        {
        }

        /// <summary>
        /// Run a Compare unit test.
        /// </summary>
        /// <param name="element">The unit test element.</param>
        /// <param name="previousUnitResults">The previous unit test results.</param>
        /// <param name="update">Indicates whether to give the user the option to fix a failing test.</param>
        public static void RunUnitTest(XmlElement element, UnitResults previousUnitResults, bool update)
        {
            string file1 = Environment.ExpandEnvironmentVariables(element.GetAttribute("File1"));
            string file2 = Environment.ExpandEnvironmentVariables(element.GetAttribute("File2"));
            string testName = element.ParentNode.Attributes["Name"].Value;

            // Check the results
            ArrayList differences = CompareUnit.CompareResults(file1, file2, testName, update);
            previousUnitResults.Errors.AddRange(differences);
            previousUnitResults.Output.AddRange(differences);
        }

        /// <summary>
        /// Compare two result files and update the expected result if specified
        /// </summary>
        /// <param name="expectedResult">The expected result file.</param>
        /// <param name="actualResult">The actual result file.</param>
        /// <param name="testName">The name of the test.</param>
        /// <param name="update">If true, update the expected result with the actual result.</param>
        /// <returns>Any differences found</returns>
        public static ArrayList CompareResults(string expectedResult, string actualResult, string testName, bool update)
        {
            ArrayList differences = CompareUnit.CompareResults(expectedResult, actualResult);

            // Update the test
            if (0 < differences.Count && update)
            {
                bool isUpdated = CompareUnit.UpdateTest(expectedResult, actualResult, testName, differences);

                if (isUpdated)
                {
                    // CompareResults again to verify that there are now no differences
                    differences = CompareResults(expectedResult, actualResult);
                }
            }

            return differences;
        }

        /// <summary>
        /// Compare two result files.
        /// </summary>
        /// <param name="expectedResult">The expected result file.</param>
        /// <param name="actualResult">The actual result file.</param>
        /// <returns>Any differences found.</returns>
        public static ArrayList CompareResults(string expectedResult, string actualResult)
        {
            ArrayList differences = new ArrayList();
            Output targetOutput;
            Output updatedOutput;

            OutputType outputType;
            string extension = Path.GetExtension(expectedResult);
            if (String.Compare(extension, ".msi", true, CultureInfo.InvariantCulture) == 0)
            {
                outputType = OutputType.Product;
            }
            else if (String.Compare(extension, ".msm", true, CultureInfo.InvariantCulture) == 0)
            {
                outputType = OutputType.Module;
            }
            else if (String.Compare(extension, ".msp", true, CultureInfo.InvariantCulture) == 0)
            {
                outputType = OutputType.Patch;
            }
            else if (String.Compare(extension, ".mst", true, CultureInfo.InvariantCulture) == 0)
            {
                outputType = OutputType.Transform;
            }
            else if (String.Compare(extension, ".pcp", true, CultureInfo.InvariantCulture) == 0)
            {
                outputType = OutputType.PatchCreation;
            }
            else if (String.Compare(extension, ".wixout", true, CultureInfo.InvariantCulture) == 0)
            {
                outputType = OutputType.Unknown;
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Cannot determine the type of msi database file based on file extension '{0}'.", extension));
            }

            if (outputType != OutputType.Unknown)
            {
                Unbinder unbinder = new Unbinder();
                unbinder.SuppressDemodularization = true;

                targetOutput = unbinder.Unbind(expectedResult, outputType, null);
                updatedOutput = unbinder.Unbind(actualResult, outputType, null);
            }
            else
            {
                targetOutput = Output.Load(expectedResult, false, false);
                updatedOutput = Output.Load(actualResult, false, false);
            }

            Differ differ = new Differ();
            differ.SuppressKeepingSpecialRows = true;
            Output transform = differ.Diff(targetOutput, updatedOutput);

            foreach (Table table in transform.Tables)
            {
                switch (table.Operation)
                {
                    case TableOperation.Add:
                        differences.Add(String.Format(CultureInfo.InvariantCulture, "The {0} table has been added.", table.Name));
                        break;
                    case TableOperation.Drop:
                        differences.Add(String.Format(CultureInfo.InvariantCulture, "The {0} table has been dropped.", table.Name));
                        continue;
                }

                // index the target rows for better error messages
                Hashtable targetRows = new Hashtable();
                Table targetTable = targetOutput.Tables[table.Name];
                if (null != targetTable)
                {
                    foreach (Row row in targetTable.Rows)
                    {
                        string primaryKey = row.GetPrimaryKey('/');

                        // only index rows with primary keys since these are the ones that can be modified
                        if (null != primaryKey)
                        {
                            targetRows.Add(primaryKey, row);
                        }
                    }
                }

                foreach (Row row in table.Rows)
                {
                    switch (row.Operation)
                    {
                        case RowOperation.Add:
                            differences.Add(String.Format(CultureInfo.InvariantCulture, "The {0} table, row '{1}' has been added.", table.Name, row.ToString()));
                            break;
                        case RowOperation.Delete:
                            differences.Add(String.Format(CultureInfo.InvariantCulture, "The {0} table, row '{1}' has been deleted.", table.Name, row.ToString()));
                            break;
                        case RowOperation.Modify:
                            if (("_SummaryInformation" != table.Name || (9 != (int)row[0] && 12 != (int)row[0] && 13 != (int)row[0] && 18 != (int)row[0])) &&
                                ("Property" != table.Name || "ProductCode" != (string)row[0]))
                            {
                                string primaryKey = row.GetPrimaryKey('/');
                                Row targetRow = (Row)targetRows[primaryKey];

                                differences.Add(String.Format(CultureInfo.InvariantCulture, "The {0} table, row '{1}' has changed to '{2}'.", table.Name, targetRow.ToString(), row.ToString()));
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Unknown diff row.");
                    }
                }
            }

            // add a description of the files being compared
            if (0 < differences.Count)
            {
                differences.Insert(0, "Differences found while comparing:");
                differences.Insert(1, expectedResult);
                differences.Insert(2, actualResult);
            }

            return differences;
        }

        /// <summary>
        /// Fix a failed test by replacing the expected file with the actual file
        /// </summary>
        /// <param name="expectedFile">The expected file</param>
        /// <param name="actualFile">The actual file</param>
        /// <param name="testName">The test name</param>
        /// <param name="differences">The list of differences between to files</param>
        /// <returns>True if the user chose to update the test, false otherwise</returns>
        private static bool UpdateTest(string expectedFile, string actualFile, string testName, ArrayList differences)
        {
            Console.WriteLine();
            Console.WriteLine(String.Concat("Test Name: ", testName));

            // Print the differences that were found
            foreach (string line in differences)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
            Console.Write("Do you wish to update the test to use the actual file '{0}' as the expected '{1}' (y/n)? ", actualFile, expectedFile);
            string answer = Console.ReadLine();

            if (answer.Equals("y", StringComparison.InvariantCultureIgnoreCase) || answer.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                // sd edit the expected file
                ArrayList output = ToolUtility.RunTool("sd.exe", String.Concat("edit ", expectedFile));

                // Print the sd edit output
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }

                File.Copy(actualFile, expectedFile, true);
                return true;
            }
            else
            {
                Console.WriteLine("The test was not updated");
                return false;
            }
        }
    }
}
