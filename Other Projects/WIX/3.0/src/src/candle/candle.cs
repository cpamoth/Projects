//-------------------------------------------------------------------------------------------------
// <copyright file="candle.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The candle compiler application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Xml;

    /// <summary>
    /// The main entry point for candle.
    /// </summary>
    public sealed class Candle
    {
        private StringCollection sourceFiles;
        private Hashtable parameters;
        private StringCollection includeSearchPaths;
        private StringCollection extensionList;
        private string outputFile;
        private string outputDirectory;
        private bool showLogo;
        private bool showHelp;
        private bool suppressSchema;
        private bool schemaOnly;
        private bool showPedanticMessages;
        private bool preprocessToStdout;
        private String preprocessFile;
        private ConsoleMessageHandler messageHandler;

        /// <summary>
        /// Instantiate a new Candle class.
        /// </summary>
        private Candle()
        {
            this.sourceFiles = new StringCollection();
            this.parameters = new Hashtable();
            this.includeSearchPaths = new StringCollection();
            this.extensionList = new StringCollection();
            this.showLogo = true;
            this.messageHandler = new ConsoleMessageHandler("CNDL", "candle.exe");
        }

        /// <summary>
        /// The main entry point for candle.
        /// </summary>
        /// <param name="args">Commandline arguments for the application.</param>
        /// <returns>Returns the application error code.</returns>
        [MTAThread]
        public static int Main(string[] args)
        {
            Candle candle = new Candle();
            return candle.Run(args);
        }

        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            try
            {
                // parse the command line
                this.ParseCommandLine(args);

                // exit if there was an error parsing the command line (otherwise the logo appears after error messages)
                if (this.messageHandler.EncounteredError)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                if (0 == this.sourceFiles.Count)
                {
                    this.showHelp = true;
                }
                else if (1 < this.sourceFiles.Count && null != this.outputFile)
                {
                    throw new ArgumentException("cannot specify more than one source file with single output file.  Either specify an output directory for the -out argument by ending the argument with a '\\' or remove the -out argument to have the source files compiled to the current directory.", "-out");
                }

                if (this.showLogo)
                {
                    Assembly candleAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Compiler version {0}", candleAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage:  candle.exe [-?] [-nologo] [-out outputFile] sourceFile [sourceFile ...]");
                    Console.WriteLine();
                    Console.WriteLine("   -d<name>=<value>  define a parameter for the preprocessor");
                    Console.WriteLine("   -p<file>   preprocess to a file (or stdout if no file supplied)");
                    Console.WriteLine("   -I<dir>    add to include search path");
                    Console.WriteLine("   -nologo    skip printing candle logo information");
                    Console.WriteLine("   -out       specify output file (default: write to current directory)");
                    Console.WriteLine("   -pedantic  show pedantic messages");
                    Console.WriteLine("   -ss        suppress schema validation of documents (performance boost)");
                    Console.WriteLine("   -trace     show source trace for errors, warnings, and verbose messages");
                    Console.WriteLine("   -ext       extension assembly or \"class, assembly\"");
                    Console.WriteLine("   -zs        only do validation of documents (no output)");
                    Console.WriteLine("   -wx        treat warnings as errors");
                    Console.WriteLine("   -sw<N>     suppress warning with specific message ID");
                    Console.WriteLine("   -v         verbose output");
                    Console.WriteLine("   -?         this help information");
                    Console.WriteLine();
                    Console.WriteLine("Common extensions:");
                    Console.WriteLine("   .wxs    - Windows installer Xml Source file");
                    Console.WriteLine("   .wxi    - Windows installer Xml Include file");
                    Console.WriteLine("   .wxl    - Windows installer Xml Localization file");
                    Console.WriteLine("   .wixobj - Windows installer Xml Object file (in XML format)");
                    Console.WriteLine("   .wixlib - Windows installer Xml Library file (in XML format)");
                    Console.WriteLine("   .wixout - Windows installer Xml Output file (in XML format)");
                    Console.WriteLine();
                    Console.WriteLine("   .msm - Windows installer Merge Module");
                    Console.WriteLine("   .msi - Windows installer Product Database");
                    Console.WriteLine("   .msp - Windows installer Patch");
                    Console.WriteLine("   .mst - Windows installer Transform");
                    Console.WriteLine("   .pcp - Windows installer Patch Creation Package");
                    Console.WriteLine();
                    Console.WriteLine("For more information see: http://wix.sourceforge.net");

                    return this.messageHandler.LastErrorNumber;
                }

                // create the preprocessor and compiler
                Preprocessor preprocessor = new Preprocessor();
                preprocessor.Message += new MessageEventHandler(this.messageHandler.Display);
                for (int i = 0; i < this.includeSearchPaths.Count; ++i)
                {
                    preprocessor.IncludeSearchPaths.Add(this.includeSearchPaths[i]);
                }

                Compiler compiler = new Compiler();
                compiler.Message += new MessageEventHandler(this.messageHandler.Display);
                compiler.ShowPedanticMessages = this.showPedanticMessages;
                compiler.SuppressValidate = this.suppressSchema;

                // load any extensions
                foreach (string extension in this.extensionList)
                {
                    WixExtension wixExtension = WixExtension.Load(extension);

                    preprocessor.AddExtension(wixExtension);
                    compiler.AddExtension(wixExtension);
                }

                // preprocess then compile each source file
                foreach (string sourceFile in this.sourceFiles)
                {
                    string sourceFileName = Path.GetFileName(sourceFile);
                    string targetFile;

                    if (null != this.outputFile)
                    {
                        targetFile = this.outputFile;
                    }
                    else if (null != this.outputDirectory)
                    {
                        targetFile = Path.Combine(this.outputDirectory, Path.ChangeExtension(sourceFileName, ".wixobj"));
                    }
                    else
                    {
                        targetFile = Path.ChangeExtension(sourceFileName, ".wixobj");
                    }

                    // print friendly message saying what file is being compiled
                    Console.WriteLine(sourceFileName);

                    // preprocess the source
                    XmlDocument sourceDocument;
                    try
                    {
                        if (this.preprocessToStdout)
                        {
                            preprocessor.PreprocessOut = Console.Out;
                        }
                        else if (null != this.preprocessFile)
                        {
                            preprocessor.PreprocessOut = new StreamWriter(this.preprocessFile);
                        }

                        sourceDocument = preprocessor.Process(Path.GetFullPath(sourceFile), this.parameters);
                    }
                    finally
                    {
                        if (null != preprocessor.PreprocessOut && Console.Out != preprocessor.PreprocessOut)
                        {
                            preprocessor.PreprocessOut.Close();
                        }
                    }

                    // if we're not actually going to compile anything, move on to the next file
                    if (this.schemaOnly || null == sourceDocument || this.preprocessToStdout || null != this.preprocessFile)
                    {
                        continue;
                    }

                    // and now we do what we came here to do...
                    Intermediate intermediate = compiler.Compile(sourceDocument);

                    // save the intermediate to disk if no errors were found for this source file
                    if (null != intermediate)
                    {
                        intermediate.Save(targetFile);
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
                    if ('d' == parameter[0])
                    {
                        parameter = arg.Substring(2);
                        string[] value = parameter.Split("=".ToCharArray(), 2);

                        if (1 == value.Length)
                        {
                            this.parameters.Add(value[0], "");
                        }
                        else
                        {
                            this.parameters.Add(value[0], value[1]);
                        }
                    }
                    else if ('I' == parameter[0])
                    {
                        this.includeSearchPaths.Add(parameter.Substring(1));
                    }
                    else if ("ext" == parameter)
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
                    else if ("o" == parameter || "out" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FileOrDirectoryPathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        if (0 <= args[i].IndexOf('\"'))
                        {
                            this.messageHandler.Display(this, WixErrors.PathCannotContainQuote(args[i]));
                            return;
                        }
                        else if (args[i].EndsWith("\\") || args[i].EndsWith("/"))
                        {
                            this.outputDirectory = Path.GetFullPath(args[i]);
                        }
                        else
                        {
                            this.outputFile = Path.GetFullPath(args[i]);
                        }
                    }
                    else if ("pedantic" == parameter)
                    {
                        this.showPedanticMessages = true;
                    }
                    else if ('p' == parameter[0])
                    {
                        String file = arg.Substring(2);
                        this.preprocessFile = file;
                        this.preprocessToStdout = (0 == file.Length);
                    }
                    else if ("ss" == parameter)
                    {
                        this.suppressSchema = true;
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
                    else if ("trace" == parameter)
                    {
                        this.messageHandler.SourceTrace = true;
                    }
                    else if ("v" == parameter)
                    {
                        this.messageHandler.ShowVerboseMessages = true;
                    }
                    else if ("wx" == parameter)
                    {
                        this.messageHandler.WarningAsError = true;
                    }
                    else if ("zs" == parameter)
                    {
                        this.schemaOnly = true;
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
                    this.sourceFiles.AddRange(AppCommon.GetFiles(arg, "Source"));
                }
            }

            return;
        }
    }
}
