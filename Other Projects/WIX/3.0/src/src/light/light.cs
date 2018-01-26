//-------------------------------------------------------------------------------------------------
// <copyright file="light.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The light linker application.
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
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// The main entry point for light.
    /// </summary>
    public sealed class Light
    {
        private string[] cultures;
        private StringCollection inputFiles;
        private bool allowIdenticalRows;
        private bool allowUnresolvedReferences;
        private bool bindFiles;
        private bool outputXml;
        private bool reuseCabinets;
        private bool sectionIdOnRows;
        private bool generateSectionIds;
        private bool setMsiAssemblyNameFileVersion;
        private bool showHelp;
        private bool showLogo;
        private bool suppressAclReset;
        private bool suppressAdminSequence;
        private bool suppressAdvertiseSequence;
        private bool suppressAssemblies;
        private bool suppressDroppingUnrealTables;
        private bool suppressFileHashAndInfo;
        private bool suppressFiles;
        private StringCollection suppressICEs;
        private bool suppressLayout;
        private bool suppressMsiAssemblyTable;
        private bool suppressSchema;
        private bool suppressUISequence;
        private bool suppressValidation;
        private bool suppressVersionCheck;
        private bool tidy;
        private string outputFile;
        private ConsoleMessageHandler messageHandler;
        private string unreferencedSymbolsFile;
        private bool showPedanticMessages;
        private string cabCachePath;
        private StringCollection basePaths;
        private StringCollection extensionList;
        private StringCollection localizationFiles;
        private StringCollection sourcePaths;
        private WixVariableResolver wixVariableResolver;
        private int cabbingThreadCount;
        private Validator validator;

        /// <summary>
        /// Instantiate a new Light class.
        /// </summary>
        private Light()
        {
            this.basePaths = new StringCollection();
            this.extensionList = new StringCollection();
            this.localizationFiles = new StringCollection();
            this.messageHandler = new ConsoleMessageHandler("LGHT", "light.exe");
            this.inputFiles = new StringCollection();
            this.sourcePaths = new StringCollection();
            this.showLogo = true;
            this.suppressICEs = new StringCollection();
            this.tidy = true;

            this.wixVariableResolver = new WixVariableResolver();
            this.wixVariableResolver.Message += new MessageEventHandler(this.messageHandler.Display);

            this.validator = new Validator();
        }

        /// <summary>
        /// The main entry point for light.
        /// </summary>
        /// <param name="args">Commandline arguments for the application.</param>
        /// <returns>Returns the application error code.</returns>
        [MTAThread]
        public static int Main(string[] args)
        {
            Light light = new Light();
            return light.Run(args);
        }

        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            Microsoft.Tools.WindowsInstallerXml.Binder binder = null;
            Linker linker = null;
            Localizer localizer = null;
            SectionCollection sections = new SectionCollection();
            ArrayList transforms = new ArrayList();

            try
            {
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

                    this.outputFile = Path.ChangeExtension(Path.GetFileName(this.inputFiles[0]), ".wix"); // we'll let the linker change the extension later
                }

                if (this.showLogo)
                {
                    Assembly lightAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Linker version {0}", lightAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage:  light.exe [-?] [-b basePath] [-nologo] [-out outputFile] objectFile [objectFile ...]");
                    Console.WriteLine();
                    Console.WriteLine("   -ai        allow identical rows, identical rows will be treated as a warning");
                    Console.WriteLine("   -au        (experimental) allow unresolved references, will not create a valid output");
                    Console.WriteLine("   -b         base path to locate all files (default: current directory)");
                    Console.WriteLine("   -bf        bind files into a wixout (only valid with -xo option)");
                    Console.WriteLine("   -cc        path to cache built cabinets (will not be deleted after linking)");
                    Console.WriteLine("   -ct <N>    number of threads to use when creating cabinets (default: %NUMBER_OF_PROCESSORS%)");
                    Console.WriteLine("   -cultures:<cultures>  semicolon-delimited list of localized string cultures to load from libraries");
                    Console.WriteLine("   -cub       additional .cub file containing ICEs to run");
                    Console.WriteLine("   -d<name>=<value>  define a wix variable");
                    Console.WriteLine("   -ext       extension assembly or \"class, assembly\"");
                    Console.WriteLine("   -fv        add a 'fileVersion' entry to the MsiAssemblyName table (rarely needed)");
                    Console.WriteLine("   -loc <loc.wxl>  read localization strings from .wxl file");
                    Console.WriteLine("   -nologo    skip printing light logo information");
                    Console.WriteLine("   -notidy    do not delete temporary files (useful for debugging)");
                    Console.WriteLine("   -out       specify output file (default: write to current directory)");
                    Console.WriteLine("   -pedantic  show pedantic messages");
                    Console.WriteLine("   -reusecab  reuse cabinets from cabinet cache");
                    Console.WriteLine("   -sa        suppress assemblies: do not get assembly name information for assemblies");
                    Console.WriteLine("   -sacl      suppress resetting ACLs (useful when laying out image to a network share)");
                    Console.WriteLine("   -sadmin    suppress default admin sequence actions");
                    Console.WriteLine("   -sadv      suppress default adv sequence actions");
                    Console.WriteLine("   -sdut      suppress dropping unreal tables to the output image (default with -xo)");
                    Console.WriteLine("   -sice:<ICE>  suppress an internal consistency evaluator (ICE)");
                    Console.WriteLine("   -sma       suppress processing the data in MsiAssembly table");
                    Console.WriteLine("   -sf        suppress files: do not get any file information (equivalent to -sa and -sh)");
                    Console.WriteLine("   -sh        suppress file info: do not get hash, version, language, etc");
                    Console.WriteLine("   -sl        suppress layout");
                    Console.WriteLine("   -ss        suppress schema validation of documents (performance boost)");
                    Console.WriteLine("   -sui       suppress default UI sequence actions");
                    Console.WriteLine("   -sv        suppress intermediate file version mismatch checking");
                    Console.WriteLine("   -sval      suppress MSI/MSM validation");
                    Console.WriteLine("   -sw<N>     suppress warning with specific message ID");
                    Console.WriteLine("   -ts        tag sectionId attribute on rows (default with -xo)");
                    Console.WriteLine("   -tsa       tag sectionId attribute on rows, generating when null (default with -xo)");
                    Console.WriteLine("   -usf <output.xml>  unreferenced symbols file");
                    Console.WriteLine("   -v         verbose output");
                    Console.WriteLine("   -wx        treat warnings as errors");
                    Console.WriteLine("   -xo        output xml instead of MSI format");
                    Console.WriteLine("   -?         this help information");
                    Console.WriteLine();
                    Console.WriteLine("Environment variables:");
                    Console.WriteLine("   WIX_TEMP   overrides the temporary directory used for cab creation, msm exploding, ...");
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

                // create the linker, binder, and validator
                linker = new Linker();
                binder = new Microsoft.Tools.WindowsInstallerXml.Binder();

                linker.AllowIdenticalRows = this.allowIdenticalRows;
                linker.AllowUnresolvedReferences = this.allowUnresolvedReferences;
                linker.Cultures = this.cultures;
                linker.UnreferencedSymbolsFile = this.unreferencedSymbolsFile;
                linker.ShowPedanticMessages = this.showPedanticMessages;
                linker.SuppressDroppingUnrealTables = this.suppressDroppingUnrealTables;
                linker.SuppressMsiAssemblyTable = this.suppressMsiAssemblyTable;
                linker.WixVariableResolver = this.wixVariableResolver;

                // set the sequence suppression options
                linker.SuppressAdminSequence = this.suppressAdminSequence;
                linker.SuppressAdvertiseSequence = this.suppressAdvertiseSequence;
                linker.SuppressUISequence = this.suppressUISequence;

                linker.SectionIdOnRows = this.sectionIdOnRows;
                linker.GenerateSectionIds = this.generateSectionIds;

                // default the number of cabbing threads to the number of processors if it wasn't specified
                if (0 == this.cabbingThreadCount)
                {
                    string numberOfProcessors = System.Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS");

                    try
                    {
                        if (null != numberOfProcessors)
                        {
                            this.cabbingThreadCount = Convert.ToInt32(numberOfProcessors, CultureInfo.InvariantCulture.NumberFormat);

                            if (0 >= this.cabbingThreadCount)
                            {
                                throw new WixException(WixErrors.IllegalEnvironmentVariable("NUMBER_OF_PROCESSORS", numberOfProcessors));
                            }
                        }
                        else // default to 1 if the environment variable is not set
                        {
                            this.cabbingThreadCount = 1;
                        }
                    }
                    catch (ArgumentException)
                    {
                        throw new WixException(WixErrors.IllegalEnvironmentVariable("NUMBER_OF_PROCESSORS", numberOfProcessors));
                    }
                    catch (FormatException)
                    {
                        throw new WixException(WixErrors.IllegalEnvironmentVariable("NUMBER_OF_PROCESSORS", numberOfProcessors));
                    }
                }
                binder.CabbingThreadCount = this.cabbingThreadCount;

                binder.SuppressAclReset = this.suppressAclReset;
                binder.SetMsiAssemblyNameFileVersion = this.setMsiAssemblyNameFileVersion;
                binder.SuppressAssemblies = this.suppressAssemblies;
                binder.SuppressFileHashAndInfo = this.suppressFileHashAndInfo;

                if (this.suppressFiles)
                {
                    binder.SuppressAssemblies = true;
                    binder.SuppressFileHashAndInfo = true;
                }

                binder.SuppressLayout = this.suppressLayout;
                binder.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");
                binder.WixVariableResolver = this.wixVariableResolver;

                validator.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");

                if (null != this.cabCachePath || this.reuseCabinets)
                {
                    // ensure the cabinet cache path exists if we are going to use it
                    if (null != this.cabCachePath && !Directory.Exists(this.cabCachePath))
                    {
                        Directory.CreateDirectory(this.cabCachePath);
                    }
                }

                if (null != this.basePaths)
                {
                    foreach (string basePath in this.basePaths)
                    {
                        this.sourcePaths.Add(basePath);
                    }
                }

                // load any extensions
                bool binderExtensionLoaded = false;
                bool validatorExtensionLoaded = false;
                foreach (string extension in this.extensionList)
                {
                    WixExtension wixExtension = WixExtension.Load(extension);

                    linker.AddExtension(wixExtension);

                    if (null != wixExtension.BinderExtension)
                    {
                        if (binderExtensionLoaded)
                        {
                            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "cannot load binder extension: {0}.  light can only load one binder extension and has already loaded binder extension: {1}.", wixExtension.BinderExtension.GetType().ToString(), binder.Extension.ToString()), "ext");
                        }

                        binder.Extension = wixExtension.BinderExtension;
                        binderExtensionLoaded = true;
                    }

                    ValidatorExtension validatorExtension = wixExtension.ValidatorExtension;
                    if (null != validatorExtension)
                    {
                        if (validatorExtensionLoaded)
                        {
                            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "cannot load linker extension: {0}.  light can only load one link extension and has already loaded link extension: {1}.", validatorExtension.GetType().ToString(), validator.Extension.ToString()), "ext");
                        }

                        validator.Extension = validatorExtension;
                        validatorExtensionLoaded = true;
                    }
                }

                // set the message handlers
                linker.Message += new MessageEventHandler(this.messageHandler.Display);
                binder.Message += new MessageEventHandler(this.messageHandler.Display);
                validator.Extension.Message += new MessageEventHandler(this.messageHandler.Display);

                Output output = null;

                // instantiate the localizer and load any localization files
                if (0 < this.localizationFiles.Count || null != this.cultures || !this.outputXml)
                {
                    localizer = new Localizer();

                    localizer.Message += new MessageEventHandler(this.messageHandler.Display);

                    // load each localization file
                    foreach (string localizationFile in this.localizationFiles)
                    {
                        Localization localization = Localization.Load(localizationFile, linker.TableDefinitions, this.suppressSchema);

                        localizer.AddLocalization(localization);
                    }

                    // immediately stop processing if any errors were found
                    if (this.messageHandler.EncounteredError)
                    {
                        return this.messageHandler.LastErrorNumber;
                    }
                }

                // loop through all the believed object files
                foreach (string inputFile in this.inputFiles)
                {
                    string dirName = Path.GetDirectoryName(inputFile);
                    string inputFileFullPath = Path.GetFullPath(inputFile);

                    if (!this.sourcePaths.Contains(dirName))
                    {
                        this.sourcePaths.Add(dirName);
                    }

                    // try loading as an object file
                    try
                    {
                        Intermediate intermediate = Intermediate.Load(inputFileFullPath, linker.TableDefinitions, this.suppressVersionCheck, this.suppressSchema);
                        sections.AddRange(intermediate.Sections);
                        continue; // next file
                    }
                    catch (WixNotIntermediateException)
                    {
                        // try another format
                    }

                    // try loading as a library file
                    try
                    {
                        Library library = Library.Load(inputFileFullPath, linker.TableDefinitions, this.suppressVersionCheck, this.suppressSchema);
                        sections.AddRange(library.Sections);

                        // load the localization files for the selected cultures
                        if (null != this.cultures)
                        {
                            Localization localization = library.GetLocalization(this.cultures);

                            if (null != localization)
                            {
                                localizer.AddLocalization(localization);
                            }
                        }

                        continue; // next file
                    }
                    catch (WixNotLibraryException)
                    {
                        // try another format
                    }

                    // try loading as an output file
                    output = Output.Load(inputFileFullPath, this.suppressVersionCheck, this.suppressSchema);
                }

                // immediately stop processing if any errors were found
                if (this.messageHandler.EncounteredError)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                // set the binder extension information
                foreach (string basePath in this.basePaths)
                {
                    binder.Extension.BasePaths.Add(basePath);
                }
                binder.Extension.CabCachePath = this.cabCachePath;
                binder.Extension.ReuseCabinets = this.reuseCabinets;
                foreach (string sourcePath in this.sourcePaths)
                {
                    binder.Extension.SourcePaths.Add(sourcePath);
                }

                // and now for the fun part
                if (null == output)
                {
                    // tell the linker about the localizer
                    linker.Localizer = localizer;
                    localizer = null;

                    output = linker.Link(sections, transforms);

                    // if an error occurred during linking, stop processing
                    if (null == output)
                    {
                        return this.messageHandler.LastErrorNumber;
                    }
                }
                else if (0 != sections.Count)
                {
                    throw new InvalidOperationException("Cannot link object files (.wixobj) files with an output file (.wixout)");
                }

                // Now that the output object is either linked or loaded, tell the binder extension about it.
                binder.Extension.Output = output;

                // only output the xml if its a patch build or user specfied to only output wixout
                if (this.outputXml || OutputType.Patch == output.Type)
                {
                    string outputExtension = Path.GetExtension(this.outputFile);
                    if (null == outputExtension || 0 == outputExtension.Length || ".wix" == outputExtension)
                    {
                        if (OutputType.Patch == output.Type)
                        {
                            this.outputFile = Path.ChangeExtension(this.outputFile, ".wixmsp");
                        }
                        else
                        {
                            this.outputFile = Path.ChangeExtension(this.outputFile, ".wixout");
                        }
                    }
                    output.Save(this.outputFile, (this.bindFiles ? binder.Extension : null), this.wixVariableResolver, binder.TempFilesLocation);
                }
                else // finish creating the MSI/MSM
                {
                    string outputExtension = Path.GetExtension(this.outputFile);
                    if (null == outputExtension || 0 == outputExtension.Length || ".wix" == outputExtension)
                    {
                        if (OutputType.Module == output.Type)
                        {
                            this.outputFile = Path.ChangeExtension(this.outputFile, ".msm");
                        }
                        else if (OutputType.PatchCreation == output.Type)
                        {
                            this.outputFile = Path.ChangeExtension(this.outputFile, ".pcp");
                        }
                        else
                        {
                            this.outputFile = Path.ChangeExtension(this.outputFile, ".msi");
                        }
                    }

                    // tell the binder about the localizer
                    binder.Localizer = localizer;

                    // tell the binder about the validator if validation isn't suppressed
                    if (!this.suppressValidation && (OutputType.Module == output.Type || OutputType.Product == output.Type))
                    {
                        // set the default cube file
                        Assembly lightAssembly = Assembly.GetExecutingAssembly();
                        string lightDirectory = Path.GetDirectoryName(lightAssembly.Location);
                        if (OutputType.Module == output.Type)
                        {
                            validator.AddCubeFile(Path.Combine(lightDirectory, "mergemod.cub"));
                        }
                        else // product
                        {
                            validator.AddCubeFile(Path.Combine(lightDirectory, "darice.cub"));
                        }

                        // disable ICE33 by default
                        this.suppressICEs.Add("ICE33");

                        // set the suppressed ICEs
                        string[] suppressICEArray = new string[this.suppressICEs.Count];
                        this.suppressICEs.CopyTo(suppressICEArray, 0);
                        validator.SuppressedICEs = suppressICEArray;

                        binder.Validator = validator;
                    }

                    binder.Bind(output, this.outputFile);
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

                if (null != validator)
                {
                    if (this.tidy)
                    {
                        if (!validator.DeleteTempFiles())
                        {
                            Console.WriteLine("Warning, failed to delete temporary directory: {0}", validator.TempFilesLocation);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Validator temporary directory located at '{0}'.", validator.TempFilesLocation);
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
                    if ("o" == parameter || "out" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.outputFile = Path.GetFullPath(args[i]);
                    }
                    else if ("ai" == parameter)
                    {
                        this.allowIdenticalRows = true;
                    }
                    else if ("au" == parameter)
                    {
                        this.allowUnresolvedReferences = true;
                    }
                    else if ("b" == parameter)
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
                    else if ("cc" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.DirectoryPathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.cabCachePath = args[i];
                    }
                    else if ("ct" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.IllegalCabbingThreadCount(String.Empty));
                            return;
                        }
                        try
                        {
                            this.cabbingThreadCount = Convert.ToInt32(args[i], CultureInfo.InvariantCulture.NumberFormat);

                            if (0 >= this.cabbingThreadCount)
                            {
                                this.messageHandler.Display(this, WixErrors.IllegalCabbingThreadCount(args[i]));
                            }
                        }
                        catch (FormatException)
                        {
                            this.messageHandler.Display(this, WixErrors.IllegalCabbingThreadCount(args[i]));
                        }
                        catch (OverflowException)
                        {
                            this.messageHandler.Display(this, WixErrors.IllegalCabbingThreadCount(args[i]));
                        }
                    }
                    else if (parameter.StartsWith("cultures:"))
                    {
                        this.cultures = arg.Substring(10).ToLower(CultureInfo.InvariantCulture).Split(';');
                    }
                    else if (parameter.StartsWith("cub"))
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired("-cub"));
                            return;
                        }

                        this.validator.AddCubeFile(args[i]);
                    }
                    else if (parameter.StartsWith("d"))
                    {
                        parameter = arg.Substring(2);
                        string[] value = parameter.Split("=".ToCharArray(), 2);

                        if (1 == value.Length)
                        {
                            this.messageHandler.Display(this, WixErrors.ExpectedWixVariableValue(value[0]));
                        }
                        else
                        {
                            this.wixVariableResolver.AddVariable(value[0], value[1]);
                        }
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
                    else if ("fv" == parameter)
                    {
                        this.setMsiAssemblyNameFileVersion = true;
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
                    else if ("notidy" == parameter)
                    {
                        this.tidy = false;
                    }
                    else if ("usf" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.unreferencedSymbolsFile = Path.GetFullPath(args[i]);
                    }
                    else if ("pedantic" == parameter)
                    {
                        this.showPedanticMessages = true;
                    }
                    else if ("reusecab" == parameter)
                    {
                        this.reuseCabinets = true;
                    }
                    else if ("sa" == parameter)
                    {
                        this.suppressAssemblies = true;
                    }
                    else if ("sacl" == parameter)
                    {
                        this.suppressAclReset = true;
                    }
                    else if ("sadmin" == parameter)
                    {
                        this.suppressAdminSequence = true;
                    }
                    else if ("sadv" == parameter)
                    {
                        this.suppressAdvertiseSequence = true;
                    }
                    else if ("sdut" == parameter)
                    {
                        this.suppressDroppingUnrealTables = true;
                    }
                    else if ("sma" == parameter)
                    {
                        this.suppressMsiAssemblyTable = true;
                    }
                    else if ("sf" == parameter)
                    {
                        this.suppressFiles = true;
                    }
                    else if ("sh" == parameter)
                    {
                        this.suppressFileHashAndInfo = true;
                    }
                    else if (parameter.StartsWith("sice:"))
                    {
                        this.suppressICEs.Add(parameter.Substring(5));
                    }
                    else if ("sl" == parameter)
                    {
                        this.suppressLayout = true;
                    }
                    else if ("ss" == parameter)
                    {
                        this.suppressSchema = true;
                    }
                    else if ("sui" == parameter)
                    {
                        this.suppressUISequence = true;
                    }
                    else if ("sv" == parameter)
                    {
                        this.suppressVersionCheck = true;
                    }
                    else if ("sval" == parameter)
                    {
                        this.suppressValidation = true;
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
                    else if ("ts" == parameter)
                    {
                        this.sectionIdOnRows = true;
                    }
                    else if ("tsa" == parameter)
                    {
                        this.sectionIdOnRows = true;
                        this.generateSectionIds = true;
                    }
                    else if ("v" == parameter)
                    {
                        this.messageHandler.ShowVerboseMessages = true;
                    }
                    else if ("wx" == parameter)
                    {
                        this.messageHandler.WarningAsError = true;
                    }
                    else if ("xo" == parameter)
                    {
                        this.outputXml = true;
                        this.sectionIdOnRows = true;
                        this.generateSectionIds = true;
                        this.suppressDroppingUnrealTables = true;
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

            if (this.bindFiles && !this.outputXml)
            {
                throw new ArgumentException("The -bf (bind files) option is only applicable with the -xo (xml output) option.");
            }
        }
    }
}
