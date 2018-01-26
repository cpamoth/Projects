//-------------------------------------------------------------------------------------------------
// <copyright file="lit.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Main entry point for library tool.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Main entry point for library tool.
    /// </summary>
    public sealed class Lit
    {
        private StringCollection basePaths;
        private BinderExtension binderExtension;
        private bool bindFiles;
        private StringCollection inputFiles;
        private string outputFile;
        private bool showLogo;
        private bool showHelp;
        private bool suppressSchema;
        private bool suppressVersionCheck;
        private ConsoleMessageHandler messageHandler;
        private StringCollection extensionList;
        private StringCollection localizationFiles;
        private StringCollection sourcePaths;

        /// <summary>
        /// Instantiate a new Lit class.
        /// </summary>
        private Lit()
        {
            this.basePaths = new StringCollection();
            this.inputFiles = new StringCollection();
            this.extensionList = new StringCollection();
            this.showLogo = true;
            this.messageHandler = new ConsoleMessageHandler("LIT", "lit.exe");
            this.extensionList = new StringCollection();
            this.localizationFiles = new StringCollection();
            this.sourcePaths = new StringCollection();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Commandline arguments for lit.</param>
        /// <returns>Returns non-zero error code in the case of an error.</returns>
        [MTAThread]
        public static int Main(string[] args)
        {
            Lit lit = new Lit();
            return lit.Run(args);
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
                Librarian librarian = null;
                SectionCollection sections = new SectionCollection();

                // parse the command line
                this.ParseCommandLine(args);

                // exit if there was an error parsing the command line (otherwise the logo appears after error messages)
                if (this.messageHandler.EncounteredError)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                if (0 == this.inputFiles.Count)
                {
                    this.showHelp = true;
                }
                else if (null == this.outputFile)
                {
                    if (1 < this.inputFiles.Count)
                    {
                        throw new ArgumentException("must specify output file when using more than one input file", "-out");
                    }

                    // we'll let the linker change the extension later
                    this.outputFile = Path.ChangeExtension(this.inputFiles[0], ".wix");
                }

                if (this.showLogo)
                {
                    Assembly litAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Library Tool version {0}", litAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage:  lit.exe [-?] [-nologo] [-out libraryFile] objectFile [objectFile ...]");
                    Console.WriteLine();
                    Console.WriteLine("   -nologo    skip printing lit logo information");
                    Console.WriteLine("   -out       specify output file (default: write to current directory)");
                    Console.WriteLine();
                    Console.WriteLine("   -b         base path to locate all files (default: current directory)");
                    Console.WriteLine("   -bf        bind files into the library file");
                    Console.WriteLine("   -ext       extension assembly or \"class, assembly\"");
                    Console.WriteLine("   -loc <loc.wxl>  bind localization strings from a wxl into the library file");
                    Console.WriteLine("   -ss        suppress schema validation of documents (performance boost)");
                    Console.WriteLine("   -sv        suppress intermediate file version mismatch checking");
                    Console.WriteLine("   -sw<N>     suppress warning with specific message ID");
                    Console.WriteLine("   -wx        treat warnings as errors");
                    Console.WriteLine("   -v         verbose output");
                    Console.WriteLine("   -?         this help information");
                    Console.WriteLine();
                    Console.WriteLine("Common extensions:");
                    Console.WriteLine("   .wxs    - Windows installer Xml Source file");
                    Console.WriteLine("   .wxi    - Windows installer Xml Include file");
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

                // create the librarian
                librarian = new Librarian();
                librarian.Message += new MessageEventHandler(this.messageHandler.Display);

                if (null != this.basePaths)
                {
                    foreach (string basePath in this.basePaths)
                    {
                        this.sourcePaths.Add(basePath);
                    }
                }

                // load any extensions
                foreach (string extension in this.extensionList)
                {
                    WixExtension wixExtension = WixExtension.Load(extension);

                    librarian.AddExtension(wixExtension);

                    // load the binder extension regardless of whether it will be used in case there is a collision
                    if (null != wixExtension.BinderExtension)
                    {
                        if (null != this.binderExtension)
                        {
                            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "cannot load binder extension: {0}.  lit can only load one binder extension and has already loaded binder extension: {1}.", wixExtension.BinderExtension.GetType().ToString(), this.binderExtension.GetType().ToString()), "ext");
                        }

                        this.binderExtension = wixExtension.BinderExtension;
                    }
                }

                // add the sections to the librarian
                foreach (string inputFile in this.inputFiles)
                {
                    string inputFileFullPath = Path.GetFullPath(inputFile);
                    string dirName = Path.GetDirectoryName(inputFileFullPath);

                    if (!this.sourcePaths.Contains(dirName))
                    {
                        this.sourcePaths.Add(dirName);
                    }

                    // try loading as an object file
                    try
                    {
                        Intermediate intermediate = Intermediate.Load(inputFileFullPath, librarian.TableDefinitions, this.suppressVersionCheck, this.suppressSchema);
                        sections.AddRange(intermediate.Sections);
                        continue; // next file
                    }
                    catch (WixNotIntermediateException)
                    {
                        // try another format
                    }

                    // try loading as a library file
                    Library loadedLibrary = Library.Load(inputFileFullPath, librarian.TableDefinitions, this.suppressVersionCheck, this.suppressSchema);
                    sections.AddRange(loadedLibrary.Sections);
                }

                // and now for the fun part
                Library library = librarian.Combine(sections);

                // save the library output if an error did not occur
                if (null != library)
                {
                    if (this.bindFiles)
                    {
                        // if the binder extension has not been loaded yet use the built-in binder extension
                        if (null == this.binderExtension)
                        {
                            this.binderExtension = new BinderExtension();
                        }

                        // set the binder extension information
                        foreach (string basePath in this.basePaths)
                        {
                            this.binderExtension.BasePaths.Add(basePath);
                        }

                        foreach (string sourcePath in this.sourcePaths)
                        {
                            this.binderExtension.SourcePaths.Add(sourcePath);
                        }
                    }
                    else
                    {
                        this.binderExtension = null;
                    }

                    foreach (string localizationFile in this.localizationFiles)
                    {
                        Localization localization = Localization.Load(localizationFile, librarian.TableDefinitions, this.suppressSchema);

                        library.AddLocalization(localization);
                    }

                    WixVariableResolver wixVariableResolver = new WixVariableResolver();

                    wixVariableResolver.Message += new MessageEventHandler(this.messageHandler.Display);

                    library.Save(this.outputFile, this.binderExtension, wixVariableResolver);
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

                    if ("b" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.DirectoryPathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.basePaths.Add(Path.GetFullPath(args[i]));
                    }
                    else if ("bf" == parameter)
                    {
                        this.bindFiles = true;
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
                    else if ("loc" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.localizationFiles.Add(Path.GetFullPath(args[i]));
                    }
                    else if ("nologo" == parameter)
                    {
                        this.showLogo = false;
                    }
                    else if ("o" == parameter || "out" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.outputFile = Path.GetFullPath(args[i]);
                    }
                    else if ("ss" == parameter)
                    {
                        this.suppressSchema = true;
                    }
                    else if ("sv" == parameter)
                    {
                        this.suppressVersionCheck = true;
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
                    this.inputFiles.AddRange(AppCommon.GetFiles(arg, "Source"));
                }
            }
        }
    }
}
