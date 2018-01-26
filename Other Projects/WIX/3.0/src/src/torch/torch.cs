//-------------------------------------------------------------------------------------------------
// <copyright file="torch.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The torch transform builder application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Specialized;
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml;

    /// <summary>
    /// The torch transform builder application.
    /// </summary>
    public sealed class Torch
    {
        private StringCollection inputFiles;
        private ConsoleMessageHandler messageHandler;
        private string outputFile;
        private bool preserveUnchangedRows;
        private bool showHelp;
        private bool showLogo;
        private bool tidy;
        private bool xmlInputs;
        private bool xmlOutput;

        /// <summary>
        /// Instantiate a new Torch class.
        /// </summary>
        private Torch()
        {
            this.inputFiles = new StringCollection();
            this.messageHandler = new ConsoleMessageHandler("TRCH", "torch.exe");
            this.preserveUnchangedRows = false;
            this.showLogo = true;
            this.tidy = true;
            this.xmlInputs = false;
            this.xmlOutput = false;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments to torch.</param>
        /// <returns>0 if sucessful, otherwise 1.</returns>
        public static int Main(string[] args)
        {
            Torch torch = new Torch();
            return torch.Run(args);
        }

        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            Microsoft.Tools.WindowsInstallerXml.Binder binder = null;
            Differ differ = null;
            Unbinder unbinder = null;

            TempFileCollection tempFileCollection = null;

            try
            {
                // parse the command line
                this.ParseCommandLine(args);

                // exit if there was an error parsing the command line (otherwise the logo appears after error messages)
                if (this.messageHandler.EncounteredError)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                // validate the inputs
                if (1 == this.inputFiles.Count)
                {
                    // Validate that if its a single input, it is a wixout to be converted to an mst.
                    if (0 != String.Compare(Path.GetExtension(this.inputFiles[0]), ".wixout", true, CultureInfo.InvariantCulture))
                    {
                        this.showHelp = true;
                    }
                }
                else if (2 == this.inputFiles.Count)
                {
                    string expectedExtension = ".msi";
                    if (this.xmlInputs)
                    {
                        expectedExtension = ".wixout";
                    }

                    // Validate that all inputs have the correct extension
                    foreach (string inputFile in inputFiles)
                    {
                        if (0 != String.Compare(Path.GetExtension(inputFile), expectedExtension, true, CultureInfo.InvariantCulture))
                        {
                            this.messageHandler.Display(this, WixErrors.UnexpectedFileExtension(this.inputFiles[0], expectedExtension));
                            this.showHelp = true;
                        }
                    }
                }
                else
                {
                    this.showHelp = true;
                }

                if (null == this.outputFile)
                {
                    this.showHelp = true;
                }

                if (this.showLogo)
                {
                    Assembly torchAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Transform Builder Version {0}", torchAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.\n");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage: torch.exe [-?] [options] targetInput updatedInput -out outputFile");
                    Console.WriteLine();
                    Console.WriteLine("   -nologo    skip printing logo information");
                    Console.WriteLine("   -notidy    do not delete temporary files (useful for debugging)");
                    Console.WriteLine("   -p         preserve unmodified content in the output");
                    Console.WriteLine("   -sw<N>     suppress warning with specific message ID");
                    Console.WriteLine("   -v         verbose output");
                    Console.WriteLine("   -wx        treat warnings as errors");
                    Console.WriteLine("   -xi        input xml instead of MSI format");
                    Console.WriteLine("   -xo        output xml instead of MST format (set by default if -xi is present");
                    Console.WriteLine("   -?         this help information");
                    Console.WriteLine();
                    Console.WriteLine("Environment variables:");
                    Console.WriteLine("   WIX_TEMP   overrides the temporary directory used for cab extraction, binary extraction, ...");
                    Console.WriteLine();
                    Console.WriteLine("Common extensions:");
                    Console.WriteLine("   .wxi    - Windows installer Xml Include file");
                    Console.WriteLine("   .wxl    - Windows installer Xml Localization file");
                    Console.WriteLine("   .wxs    - Windows installer Xml Source file");
                    Console.WriteLine("   .wixlib - Windows installer Xml Library file (in XML format)");
                    Console.WriteLine("   .wixobj - Windows installer Xml Object file (in XML format)");
                    Console.WriteLine("   .wixout - Windows installer Xml Output file (in XML format)");
                    Console.WriteLine();
                    Console.WriteLine("   .msi - Windows installer Product Database");
                    Console.WriteLine("   .msm - Windows installer Merge Module");
                    Console.WriteLine("   .msp - Windows installer Patch");
                    Console.WriteLine("   .mst - Windows installer Transform");
                    Console.WriteLine("   .pcp - Windows installer Patch Creation Package");
                    Console.WriteLine();
                    Console.WriteLine("For more information see: http://wix.sourceforge.net");

                    return this.messageHandler.LastErrorNumber;
                }

                binder = new Microsoft.Tools.WindowsInstallerXml.Binder();
                differ = new Differ();
                unbinder = new Unbinder();

                binder.Message += new MessageEventHandler(this.messageHandler.Display);
                differ.Message += new MessageEventHandler(this.messageHandler.Display);
                unbinder.Message += new MessageEventHandler(this.messageHandler.Display);

                binder.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");
                unbinder.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");
                tempFileCollection = new TempFileCollection(Environment.GetEnvironmentVariable("WIX_TEMP"));

                binder.WixVariableResolver = new WixVariableResolver();
                differ.PreserveUnchangedRows = this.preserveUnchangedRows;
                unbinder.SuppressExtractCabinets = true;

                // load and process the inputs
                Output transform;
                if (1 == this.inputFiles.Count)
                {
                    transform = Output.Load(this.inputFiles[0], false, false);
                    if (OutputType.Transform != transform.Type)
                    {
                        this.messageHandler.Display(this, WixErrors.InvalidWixTransform(this.inputFiles[0]));
                        return this.messageHandler.LastErrorNumber;
                    }
                }
                else // 2 inputs
                {
                    Output targetOutput;
                    Output updatedOutput;

                    if (this.xmlInputs)
                    {
                        // load the target database
                        targetOutput = Output.Load(this.inputFiles[0], false, false);

                        // load the updated database
                        updatedOutput = Output.Load(this.inputFiles[1], false, false);
                    }
                    else
                    {
                        // load the target database
                        targetOutput = unbinder.Unbind(this.inputFiles[0], OutputType.Product, Path.Combine(tempFileCollection.BasePath, "targetBinaries"));

                        // load the updated database
                        updatedOutput = unbinder.Unbind(this.inputFiles[1], OutputType.Product, Path.Combine(tempFileCollection.BasePath, "updatedBinaries"));
                    }

                    // diff the target and updated databases
                    transform = differ.Diff(targetOutput, updatedOutput);

                    if (null == transform.Tables || 0 >= transform.Tables.Count)
                    {
                        throw new WixException(WixErrors.NoDifferencesInTransform(transform.SourceLineNumbers));
                    }
                }

                // output the transform
                if (null != transform)
                {
                    // If either the user selected xml output or gave xml input, save as xml output.
                    // With xml inputs, many funtions of the binder have not been performed on the inputs (ie. file sequencing). This results in bad IDT files which cannot be put in a transform.
                    if (this.xmlOutput || this.xmlInputs)
                    {
                        transform.Save(this.outputFile, null, null, null);
                    }
                    else
                    {
                        binder.Bind(transform, this.outputFile);
                    }
                }
            }
            catch (WixException we)
            {
                if (we is WixInvalidIdtException)
                {
                    // make sure the IDT files stay around
                    this.tidy = false;
                }

                this.messageHandler.Display(this, we.Error);
            }
            catch (Exception e)
            {
                // make sure the files stay around for debugging
                this.tidy = false;

                this.messageHandler.Display(this, WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));
                if (e is NullReferenceException || e is SEHException)
                {
                    throw;
                }
            }
            finally
            {
                if (null != binder)
                {
                    if (this.tidy)
                    {
                        if (!binder.DeleteTempFiles())
                        {
                            Console.WriteLine("Warning, failed to delete temporary directory: {0}", binder.TempFilesLocation);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Binder temporary directory located at '{0}'.", binder.TempFilesLocation);
                    }
                }

                if (null != unbinder)
                {
                    if (this.tidy)
                    {
                        if (!unbinder.DeleteTempFiles())
                        {
                            Console.WriteLine("Warning, failed to delete temporary directory: {0}", binder.TempFilesLocation);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unbinder temporary directory located at '{0}'.", binder.TempFilesLocation);
                    }
                }

                if (null != tempFileCollection)
                {
                    if (this.tidy)
                    {
                        try
                        {
                            Directory.Delete(tempFileCollection.BasePath, true);
                        }
                        catch
                        {
                            Console.WriteLine("Warning, failed to delete temporary directory: {0}", tempFileCollection.BasePath);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Torch temporary directory located at '{0}'.", tempFileCollection.BasePath);
                    }
                }
            }

            return this.messageHandler.LastErrorNumber;
        }

        /// <summary>
        /// Parse the commandline arguments.
        /// </summary>
        /// <param name="args">Commandline arguments.</param>
        private void ParseCommandLine(string[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i];
                if (null == arg || 0 == arg.Length) // skip blank arguments
                {
                    continue;
                }

                if ('-' == arg[0] || '/' == arg[0])
                {
                    string parameter = arg.Substring(1);

                    if ("nologo" == parameter)
                    {
                        this.showLogo = false;
                    }
                    else if ("notidy" == parameter)
                    {
                        this.tidy = false;
                    }
                    else if ("out" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.outputFile = Path.GetFullPath(args[i]);
                    }
                    else if ("p" == parameter)
                    {
                        this.preserveUnchangedRows = true;
                    }
                    else if (parameter.StartsWith("sw"))
                    {
                        try
                        {
                            int suppressWarning = Convert.ToInt32(parameter.Substring(2), CultureInfo.InvariantCulture.NumberFormat);

                            if (0 >= suppressWarning)
                            {
                                this.messageHandler.Display(this, WixErrors.IllegalSuppressWarningId(parameter.Substring(2)));
                            }

                            this.messageHandler.SuppressWarningMessage(suppressWarning);
                        }
                        catch (FormatException)
                        {
                            this.messageHandler.Display(this, WixErrors.IllegalSuppressWarningId(parameter.Substring(2)));
                        }
                        catch (OverflowException)
                        {
                            this.messageHandler.Display(this, WixErrors.IllegalSuppressWarningId(parameter.Substring(2)));
                        }
                    }
                    else if ("v" == parameter)
                    {
                        this.messageHandler.ShowVerboseMessages = true;
                    }
                    else if ("wx" == parameter)
                    {
                        this.messageHandler.WarningAsError = true;
                    }
                    else if ("xi" == parameter)
                    {
                        this.xmlInputs = true;
                    }
                    else if ("xo" == parameter)
                    {
                        this.xmlOutput = true;
                    }
                    else if ("?" == parameter || "help" == parameter)
                    {
                        this.showHelp = true;
                    }
                }
                else if ('@' == arg[0])
                {
                    this.ParseCommandLine(AppCommon.ParseResponseFile(arg.Substring(1)));
                }
                else
                {
                    this.inputFiles.Add(Path.GetFullPath(arg));
                }
            }
        }
    }
}
