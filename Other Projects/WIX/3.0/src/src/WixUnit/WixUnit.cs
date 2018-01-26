//-------------------------------------------------------------------------------------------------
// <copyright file="WixUnit.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML unit test runner.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;

    using Microsoft.Tools.WindowsInstallerXml;

    /// <summary>
    /// The Windows Installer XML unit test runner.
    /// </summary>
    public sealed class WixUnit
    {
        private const string XmlNamespace = "http://schemas.microsoft.com/wix/2006/WixUnit";

        private object lockObject = new object();
        private AutoResetEvent unitTestsComplete = new AutoResetEvent(false);

        private int completedUnitTests;
        private int failedUnitTests;
        private int totalUnitTests;

        private ConsoleMessageHandler messageHandler = new ConsoleMessageHandler("WUNT", "WixUnit");
        private List<KeyValuePair<string, string>> environmentVariables = new List<KeyValuePair<string, string>>();
        private bool noTidy;
        private bool showHelp;
        private TempFileCollection tempFileCollection;
        private Queue unitTestElements = new Queue();
        private ArrayList unitTests = new ArrayList();
        private string unitTestsFile;
        private bool updateTests;
        private bool validate;
        private bool verbose;

        /// <summary>
        /// Constructor to prevent instantiating a static class.
        /// </summary>
        private WixUnit()
        {
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The error code for the application.</returns>
        public static int Main(string[] args)
        {
            WixUnit wixUnit = new WixUnit();
            return wixUnit.Run(args);
        }

        /// <summary>
        /// Recursively loops through a directory, changing an attribute on all of the underlying files.
        /// An example is to add/remove the ReadOnly flag from each file.
        /// </summary>
        /// <param name="path">The directory path to start deleting from.</param>
        /// <param name="fileAttribute">The FileAttribute to change on each file.</param>
        /// <param name="markAttribute">If true, add the attribute to each file. If false, remove it.</param>
        private static void RecursiveFileAttributes(string path, FileAttributes fileAttribute, bool markAttribute)
        {
            foreach (string subDirectory in Directory.GetDirectories(path))
            {
                RecursiveFileAttributes(subDirectory, fileAttribute, markAttribute);
            }

            foreach (string filePath in Directory.GetFiles(path))
            {
                FileAttributes attributes = File.GetAttributes(filePath);
                if (markAttribute)
                {
                    attributes = attributes | fileAttribute; // add to list of attributes
                }
                else if (fileAttribute == (attributes & fileAttribute)) // if attribute set
                {
                    attributes = attributes ^ fileAttribute; // remove from list of attributes
                }
                File.SetAttributes(filePath, attributes);
            }
        }


        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            int beginTickCount = Environment.TickCount;

            try
            {
                this.tempFileCollection = new TempFileCollection();

                this.ParseCommandline(args);

                // get the assemblies
                Assembly thisAssembly = Assembly.GetExecutingAssembly();

                if (this.showHelp)
                {
                    Console.WriteLine("Microsoft (R) WixUnit version {0}", thisAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation. All rights reserved.");
                    Console.WriteLine();
                    Console.WriteLine(" usage: WixUnit [-?] tests.xml");
                    Console.WriteLine();
                    Console.WriteLine("   -env:<var>=<value>  Sets an environment variable to the value for the current process");
                    Console.WriteLine("   -notidy             Do not delete temporary files (for checking results)");
                    Console.WriteLine("   -test:<Test_name>   Run only the specified test (may use wildcards)");
                    Console.WriteLine("   -update             Prompt user to auto-update a test if expected and actual output files do not match");
                    Console.WriteLine("   -v                  Verbose output");
                    Console.WriteLine("   -val                Run MSI validation for light unit tests");

                    return 0;
                }

                // set the environment variables for the process only
                foreach (KeyValuePair<string, string> environmentVariable in this.environmentVariables)
                {
                    Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value, EnvironmentVariableTarget.Process);
                }

                // load the schema
                XmlReader schemaReader = null;
                XmlSchemaCollection schemas = null;
                try
                {
                    schemas = new XmlSchemaCollection();

                    schemaReader = new XmlTextReader(thisAssembly.GetManifestResourceStream("Microsoft.Tools.WindowsInstallerXml.Unit.unitTests.xsd"));
                    XmlSchema schema = XmlSchema.Read(schemaReader, null);
                    schemas.Add(schema);
                }
                finally
                {
                    if (schemaReader != null)
                    {
                        schemaReader.Close();
                    }
                }

                // load the unit tests
                XmlTextReader reader = null;
                XmlDocument doc = new XmlDocument();
                try
                {
                    reader = new XmlTextReader(this.unitTestsFile);
                    XmlValidatingReader validatingReader = new XmlValidatingReader(reader);
                    validatingReader.Schemas.Add(schemas);

                    // load the xml into a DOM
                    doc.Load(validatingReader);
                }
                catch (XmlException e)
                {
                    SourceLineNumber sourceLineNumber = new SourceLineNumber(this.unitTestsFile, e.LineNumber);
                    SourceLineNumberCollection sourceLineNumbers = new SourceLineNumberCollection(new SourceLineNumber[] { sourceLineNumber });

                    throw new WixException(WixErrors.InvalidXml(sourceLineNumbers, "unitTests", e.Message));
                }
                catch (XmlSchemaException e)
                {
                    SourceLineNumber sourceLineNumber = new SourceLineNumber(this.unitTestsFile, e.LineNumber);
                    SourceLineNumberCollection sourceLineNumbers = new SourceLineNumberCollection(new SourceLineNumber[] { sourceLineNumber });

                    throw new WixException(WixErrors.SchemaValidationFailed(sourceLineNumbers, e.Message));
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }

                // check the document element
                if ("UnitTests" != doc.DocumentElement.LocalName || XmlNamespace != doc.DocumentElement.NamespaceURI)
                {
                    throw new InvalidOperationException("Unrecognized document element.");
                }

                // create a regular expression of the selected tests
                Regex selectedUnitTests = new Regex(String.Concat("^", String.Join("$|^", (string[])this.unitTests.ToArray(typeof(string))), "$"), RegexOptions.IgnoreCase | RegexOptions.Singleline);

                // find the unit tests
                foreach (XmlNode node in doc.DocumentElement)
                {
                    if (XmlNodeType.Element == node.NodeType)
                    {
                        switch (node.LocalName)
                        {
                            case "UnitTest":
                                XmlElement unitTestElement = (XmlElement)node;
                                string unitTestName = unitTestElement.GetAttribute("Name");

                                if (selectedUnitTests.IsMatch(unitTestName))
                                {
                                    unitTestElement.SetAttribute("TempDirectory", this.tempFileCollection.BasePath);
                                    this.unitTestElements.Enqueue(node);
                                }
                                break;
                        }
                    }
                }

                if (this.unitTests.Count > 0)
                {
                    this.totalUnitTests = this.unitTestElements.Count;
                    int numThreads;

                    if (this.updateTests)
                    {
                        // Run on one thread so execution is paused if the user is prompted to update a test
                        numThreads = 1;
                    }
                    else
                    {
                        // create a thread for each processor
                        numThreads = Convert.ToInt32(Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"), CultureInfo.InvariantCulture);
                    }

                    Thread[] threads = new Thread[numThreads];

                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i] = new Thread(new ThreadStart(this.RunUnitTests));
                        threads[i].Start();
                    }

                    // wait for all threads to finish
                    foreach (Thread thread in threads)
                    {
                        thread.Join();
                    }

                    // report the results
                    Console.WriteLine();
                    int elapsedTime = (Environment.TickCount - beginTickCount) / 1000;
                    if (this.failedUnitTests > 0)
                    {
                        Console.WriteLine("Failed {0} out of {1} unit test{2} ({3} seconds).", this.failedUnitTests, this.totalUnitTests, (1 != this.completedUnitTests ? "s" : ""), elapsedTime);
                    }
                    else
                    {
                        Console.WriteLine("Successful unit tests: {0} ({1} seconds).", this.completedUnitTests, elapsedTime);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("No unit tests were selected.");
                }
            }
            catch (WixException we)
            {
                this.messageHandler.Display(this, we.Error);
            }
            catch (Exception e)
            {
                this.messageHandler.Display(this, WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));
                if (e is NullReferenceException)
                {
                    throw;
                }
            }
            finally
            {
                if (this.noTidy)
                {
                    Console.WriteLine();
                    Console.WriteLine("The notidy option was specified, temporary files can be found at:");
                    Console.WriteLine(this.tempFileCollection.BasePath);
                }
                else
                {
                    // try three times and give up with a warning if the temp files aren't gone by then
                    const int RetryLimit = 3;

                    for (int i = 0; i < RetryLimit; i++)
                    {
                        try
                        {
                            Directory.Delete(this.tempFileCollection.BasePath, true);   // toast the whole temp directory
                            break; // no exception means we got success the first time
                        }
                        catch (UnauthorizedAccessException)
                        {
                            if (0 == i) // should only need to unmark readonly once - there's no point in doing it again and again
                            {
                                RecursiveFileAttributes(this.tempFileCollection.BasePath, FileAttributes.ReadOnly, false); // toasting will fail if any files are read-only. Try changing them to not be.
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (DirectoryNotFoundException)
                        {
                            // if the path doesn't exist, then there is nothing for us to worry about
                            break;
                        }
                        catch (IOException) // directory in use
                        {
                            if (i == (RetryLimit - 1)) // last try failed still, give up
                            {
                                break;
                            }
                            Thread.Sleep(300);  // sleep a bit before trying again
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Run the unit tests.
        /// </summary>
        private void RunUnitTests()
        {
            try
            {
                while (true)
                {
                    XmlElement unitTestElement;

                    lock (this.unitTestElements)
                    {
                        // check if there are any more cabinets to create
                        if (0 == this.unitTestElements.Count)
                        {
                            break;
                        }

                        unitTestElement = (XmlElement)this.unitTestElements.Dequeue();
                    }

                    // create a cabinet
                    this.RunUnitTest(unitTestElement);
                }
            }
            catch (WixException we)
            {
                this.messageHandler.Display(this, we.Error);
            }
            catch (Exception e)
            {
                this.messageHandler.Display(this, WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));
            }
        }

        /// <summary>
        /// Run a unit test.
        /// </summary>
        /// <param name="unitTestElement">The unit test to run.</param>
        private void RunUnitTest(XmlElement unitTestElement)
        {
            string name = unitTestElement.GetAttribute("Name");
            string tempDirectory = Path.Combine(unitTestElement.GetAttribute("TempDirectory"), name);
            UnitResults unitResults = new UnitResults();

            try
            {
                // ensure the temp directory exists
                Directory.CreateDirectory(tempDirectory);

                foreach (XmlNode node in unitTestElement.ChildNodes)
                {
                    if (XmlNodeType.Element == node.NodeType)
                    {
                        XmlElement unitElement = (XmlElement)node;

                        // add inherited attributes from the parent element
                        foreach (XmlAttribute attribute in unitTestElement.ParentNode.Attributes)
                        {
                            if (attribute.NamespaceURI.Length == 0)
                            {
                                unitElement.SetAttribute(attribute.Name, Environment.ExpandEnvironmentVariables(attribute.Value));
                            }
                        }
                        unitElement.SetAttribute("TempDirectory", tempDirectory);

                        switch (node.LocalName)
                        {
                            case "Candle":
                                CandleUnit.RunUnitTest(unitElement, unitResults);
                                break;
                            case "Compare":
                                CompareUnit.RunUnitTest(unitElement, unitResults, this.updateTests);
                                break;
                            case "Dark":
                                DarkUnit.RunUnitTest(unitElement, unitResults);
                                break;
                            case "Heat":
                                HeatUnit.RunUnitTest(unitElement, unitResults);
                                break;
                            case "Light":
                                // If WixUnit was not run with -val then suppress MSI validation
                                if (!this.validate && ("true" != unitElement.GetAttribute("ForceValidation")))
                                {
                                    string arguments = unitElement.GetAttribute("Arguments");
                                    if (!arguments.Contains("-sval"))
                                    {
                                        unitElement.SetAttribute("Arguments", String.Concat(arguments, " -sval"));
                                    }
                                }

                                LightUnit.RunUnitTest(unitElement, unitResults, this.updateTests);
                                break;
                            case "Lit":
                                LitUnit.RunUnitTest(unitElement, unitResults);
                                break;
                            case "Process":
                                ProcessUnit.RunUnitTest(unitElement, unitResults);
                                break;
                            case "Pyro":
                                PyroUnit.RunUnitTest(unitElement, unitResults, this.updateTests);
                                break;
                            case "Torch":
                                TorchUnit.RunUnitTest(unitElement, unitResults, this.updateTests);
                                break;
                            case "WixProj":
                                bool skipValidation = (!this.validate);
                                WixProjUnit.RunUnitTest(unitElement, unitResults, this.verbose, skipValidation, this.updateTests);
                                break;
                        }

                        // check for errors
                        if (unitResults.Errors.Count > 0)
                        {
                            break;
                        }
                    }
                }
            }
            catch (WixException we)
            {
                string message = this.messageHandler.GetMessageString(this, we.Error);

                if (null != message)
                {
                    unitResults.Errors.Add(message);
                    unitResults.Output.Add(message);
                }
            }
            catch (Exception e)
            {
                string message = this.messageHandler.GetMessageString(this, WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));

                if (null != message)
                {
                    unitResults.Errors.Add(message);
                    unitResults.Output.Add(message);
                }
            }

            lock (this.lockObject)
            {
                Console.Write("{0} of {1} - {2}: ", ++this.completedUnitTests, this.totalUnitTests, name.PadRight(30, '.'));

                if (unitResults.Errors.Count > 0)
                {
                    failedUnitTests++;
                    Console.WriteLine("Failed");

                    if (this.verbose)
                    {
                        foreach (string line in unitResults.Output)
                        {
                            Console.WriteLine(line);
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        foreach (string line in unitResults.Errors)
                        {
                            Console.WriteLine(line);
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("Success");

                    if (this.verbose)
                    {
                        foreach (string line in unitResults.Output)
                        {
                            Console.WriteLine(line);
                        }
                        Console.WriteLine();
                    }
                }

                if (this.totalUnitTests == this.completedUnitTests)
                {
                    this.unitTestsComplete.Set();
                }
            }
        }

        /// <summary>
        /// Parse the command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void ParseCommandline(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    string parameter = arg.Substring(1);

                    if ("?" == parameter)
                    {
                        this.showHelp = true;
                    }
                    else if (parameter.StartsWith("env:"))
                    {
                        parameter = parameter.Substring("env:".Length);
                        int equalPos = parameter.IndexOf('=');
                        if (0 > equalPos)
                        {
                            throw new ArgumentException("env parameters require a name=value pair.");
                        }
                        string name = parameter.Substring(0, equalPos);
                        string value = parameter.Substring(equalPos + 1);
                        this.environmentVariables.Add(new KeyValuePair<string, string>(name, value));
                    }
                    else if ("notidy" == parameter)
                    {
                        this.noTidy = true;
                    }
                    else if (parameter.StartsWith("test:"))
                    {
                        this.unitTests.Add(parameter.Substring(5));
                    }
                    else if ("update" == parameter)
                    {
                        this.updateTests = true;
                    }
                    else if ("v" == parameter)
                    {
                        this.verbose = true;
                    }
                    else if ("val" == parameter)
                    {
                        this.validate = true;
                    }
                    else
                    {
                        throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unrecognized commandline parameter '{0}'.", arg));
                    }
                }
                else if (this.unitTestsFile == null)
                {
                    this.unitTestsFile = arg;
                }
                else
                {
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Unrecognized argument '{0}'.", arg));
                }
            }

            // no individual unit tests were selected, so match all unit tests
            if (this.unitTests.Count == 0)
            {
                this.unitTests.Add(".*");
            }
        }
    }
}
