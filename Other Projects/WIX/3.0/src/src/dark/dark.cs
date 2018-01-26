//-------------------------------------------------------------------------------------------------
// <copyright file="dark.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The dark decompiler application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// Entry point for decompiler
    /// </summary>
    public sealed class Dark
    {
        private string exportBasePath;
        private StringCollection extensionList;
        private string inputFile;
        private ConsoleMessageHandler messageHandler;
        private string outputFile;
        private OutputType outputType;
        private bool outputXml;
        private bool showHelp;
        private bool showLogo;
        private bool suppressDroppingEmptyTables;
        private bool suppressRelativeActionSequencing;
        private bool suppressUI;
        private bool tidy;

        /// <summary>
        /// Instantiate a new Dark class.
        /// </summary>
        private Dark()
        {
            this.extensionList = new StringCollection();
            this.messageHandler = new ConsoleMessageHandler("DARK", "dark.exe");
            this.showLogo = true;
            this.tidy = true;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments to decompiler.</param>
        /// <returns>0 if sucessful, otherwise 1.</returns>
        public static int Main(string[] args)
        {
            Dark dark = new Dark();
            return dark.Run(args);
        }

        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            Decompiler decompiler = null;
            Mutator mutator = null;
            Unbinder unbinder = null;

            try
            {
                // parse the command line
                this.ParseCommandLine(args);

                // exit if there was an error parsing the command line (otherwise the logo appears after error messages)
                if (this.messageHandler.EncounteredError)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                if (null == this.inputFile || null == this.outputFile)
                {
                    this.showHelp = true;
                }

                if (this.showLogo)
                {
                    Assembly darkAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Decompiler Version {0}", darkAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.\n");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage: dark.exe [-?] [-nologo] database.msi source.wxs");
                    Console.WriteLine();
                    Console.WriteLine("   -ext       extension assembly or \"class, assembly\"");
                    Console.WriteLine("   -nologo    skip printing dark logo information");
                    Console.WriteLine("   -notidy    do not delete temporary files (useful for debugging)");
                    Console.WriteLine("   -sdet      suppress dropping empty tables (adds EnsureTable as appropriate)");
                    Console.WriteLine("   -sras      suppress relative action sequencing (use explicit sequence numbers)");
                    Console.WriteLine("   -sui       suppress decompiling UI-related tables");
                    Console.WriteLine("   -sw<N>     suppress warning with specific message ID");
                    Console.WriteLine("   -v         verbose output");
                    Console.WriteLine("   -wx        treat warnings as errors");
                    Console.WriteLine("   -x <path>  export binaries from cabinets and embedded binaries to the provided path");
                    Console.WriteLine("   -xo        output xml instead of WiX source code (mandatory for transforms and patches)");
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

                // create the decompiler and mutator
                decompiler = new Decompiler();
                mutator = new Mutator();
                unbinder = new Unbinder();

                // read the configuration file (dark.exe.config)
                AppCommon.ReadConfiguration(this.extensionList);

                // load any extensions
                foreach (string extension in this.extensionList)
                {
                    WixExtension wixExtension = WixExtension.Load(extension);

                    decompiler.AddExtension(wixExtension);
                    unbinder.AddExtension(wixExtension);
                }

                // set options
                decompiler.SuppressDroppingEmptyTables = this.suppressDroppingEmptyTables;
                decompiler.SuppressRelativeActionSequencing = this.suppressRelativeActionSequencing;
                decompiler.SuppressUI = this.suppressUI;
                decompiler.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");

                unbinder.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");

                decompiler.Message += new MessageEventHandler(this.messageHandler.Display);
                mutator.Message += new MessageEventHandler(this.messageHandler.Display);
                unbinder.Message += new MessageEventHandler(this.messageHandler.Display);

                // print friendly message saying what file is being decompiled
                Console.WriteLine(Path.GetFileName(this.inputFile));

                // unbind
                Output output = unbinder.Unbind(this.inputFile, this.outputType, this.exportBasePath);

                if (null != output)
                {
                    if (OutputType.Patch == this.outputType || OutputType.Transform == this.outputType || this.outputXml)
                    {
                        output.Save(this.outputFile, null, new WixVariableResolver(), null);
                    }
                    else // decompile
                    {
                        Wix.Wix wix = decompiler.Decompile(output);

                        // output
                        if (null != wix)
                        {
                            XmlTextWriter writer = null;

                            // mutate the Wix document
                            if (!mutator.Mutate(wix))
                            {
                                return this.messageHandler.LastErrorNumber;
                            }

                            try
                            {
                                writer = new XmlTextWriter(this.outputFile, System.Text.Encoding.UTF8);

                                writer.Indentation = 4;
                                writer.IndentChar = ' ';
                                writer.QuoteChar = '"';
                                writer.Formatting = Formatting.Indented;

                                writer.WriteStartDocument();
                                wix.OutputXml(writer);
                                writer.WriteEndDocument();
                            }
                            finally
                            {
                                if (null != writer)
                                {
                                    writer.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (WixException we)
            {
                this.messageHandler.Display(this, we.Error);
            }
            catch (Exception e)
            {
                this.messageHandler.Display(this, WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));
                if (e is NullReferenceException || e is SEHException)
                {
                    throw;
                }
            }
            finally
            {
                if (null != decompiler)
                {
                    if (this.tidy)
                    {
                        if (!decompiler.DeleteTempFiles())
                        {
                            Console.WriteLine("Warning, failed to delete temporary directory: {0}", decompiler.TempFilesLocation);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Temporary directory located at '{0}'.", decompiler.TempFilesLocation);
                    }
                }

                if (null != unbinder)
                {
                    if (this.tidy)
                    {
                        if (!unbinder.DeleteTempFiles())
                        {
                            Console.WriteLine("Warning, failed to delete temporary directory: {0}", unbinder.TempFilesLocation);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Temporary directory located at '{0}'.", unbinder.TempFilesLocation);
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

                    if ("ext" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.TypeSpecificationForExtensionRequired("-ext"));
                            return;
                        }

                        this.extensionList.Add(args[i]);
                    }
                    else if ("nologo" == parameter)
                    {
                        this.showLogo = false;
                    }
                    else if ("notidy" == parameter)
                    {
                        this.tidy = false;
                    }
                    else if ("sdet" == parameter)
                    {
                        this.suppressDroppingEmptyTables = true;
                    }
                    else if ("sras" == parameter)
                    {
                        this.suppressRelativeActionSequencing = true;
                    }
                    else if ("sui" == parameter)
                    {
                        this.suppressUI = true;
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
                    else if ("x" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.DirectoryPathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.exportBasePath = Path.GetFullPath(args[i]);
                    }
                    else if ("xo" == parameter)
                    {
                        this.outputXml = true;
                    }
                    else if ("?" == parameter || "help" == parameter)
                    {
                        this.showHelp = true;
                    }
                }
                else
                {
                    if (null == this.inputFile)
                    {
                        this.inputFile = Path.GetFullPath(arg);

                        // guess the output type based on the extension of the input file
                        if (OutputType.Unknown == this.outputType)
                        {
                            switch (Path.GetExtension(this.inputFile).ToLower(CultureInfo.InvariantCulture))
                            {
                                case ".msi":
                                    this.outputType = OutputType.Product;
                                    break;
                                case ".msm":
                                    this.outputType = OutputType.Module;
                                    break;
                                case ".msp":
                                    this.outputType = OutputType.Patch;
                                    break;
                                case ".mst":
                                    this.outputType = OutputType.Transform;
                                    break;
                                case ".pcp":
                                    this.outputType = OutputType.PatchCreation;
                                    break;
                                default:
                                    throw new ArgumentException(String.Format("Cannot determine the type of msi database file based on file extension '{0}'.", Path.GetExtension(this.inputFile)));
                            }
                        }
                    }
                    else if (null == this.outputFile)
                    {
                        this.outputFile = Path.GetFullPath(arg);
                    }
                    else
                    {
                        this.messageHandler.Display(this, WixErrors.AdditionalArgumentUnexpected(arg));
                    }
                }
            }
        }
    }
}
