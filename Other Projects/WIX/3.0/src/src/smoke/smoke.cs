//-------------------------------------------------------------------------------------------------
// <copyright file="smoke.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The smoke validator application.
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// The main entry point for Smoke.
    /// </summary>
    public sealed class Smoke
    {
        private bool addDefault;
        private StringCollection extensionList;
        private StringCollection inputFiles;
        private ConsoleMessageHandler messageHandler;
        private bool showHelp;
        private bool showLogo;
        private StringCollection suppressICEs;
        private bool tidy;
        private Validator validator;

        /// <summary>
        /// Instantiate a new Smoke class.
        /// </summary>
        private Smoke()
        {
            this.extensionList = new StringCollection();
            this.inputFiles = new StringCollection();
            this.messageHandler = new ConsoleMessageHandler("SMOK", "smoke.exe");
            this.addDefault = true;
            this.showLogo = true;
            this.suppressICEs = new StringCollection();
            this.tidy = true;
            this.validator = new Validator();
        }

        /// <summary>
        /// The main entry point for smoke.
        /// </summary>
        /// <param name="args">Commandline arguments for the application.</param>
        /// <returns>Returns the application error code.</returns>
        [MTAThread]
        public static int Main(string[] args)
        {
            Smoke smoke = new Smoke();
            return smoke.Run(args);
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

                if (0 == this.inputFiles.Count)
                {
                    this.showHelp = true;
                }

                if (this.showLogo)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Validator version {0}", assembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage:  smoke.exe [-?]  databaseFile [databaseFile ...]");
                    Console.WriteLine();
                    Console.WriteLine("   -cub         additional .cub file containing ICEs to run");
                    Console.WriteLine("   -ext         extension assembly or \"class, assembly\"");
                    Console.WriteLine("   -nodefault   do not add the default .cub files for .msi and .msm files");
                    Console.WriteLine("   -nologo      skip printing smoke logo information");
                    Console.WriteLine("   -notidy      do not delete temporary files (useful for debugging)");
                    Console.WriteLine("   -sice:<ICE>  suppress an internal consistency evaluator (ICE)");
                    Console.WriteLine("   -sw<N>       suppress warning with specific message ID");
                    Console.WriteLine("   -v           verbose output");
                    Console.WriteLine("   -wx          treat warnings as errors");
                    Console.WriteLine("   -?           this help information");
                    Console.WriteLine();
                    Console.WriteLine("Environment variables:");
                    Console.WriteLine("   WIX_TEMP   overrides the temporary directory used for validation");
                    Console.WriteLine();
                    Console.WriteLine("For more information see: http://wix.sourceforge.net");

                    return this.messageHandler.LastErrorNumber;
                }

                validator.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");

                // load any extensions
                bool validatorExtensionLoaded = false;
                foreach (string extension in this.extensionList)
                {
                    WixExtension wixExtension = WixExtension.Load(extension);

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
                validator.Extension.Message += new MessageEventHandler(this.messageHandler.Display);

                // disable ICE33 by default
                this.suppressICEs.Add("ICE33");

                // set the suppressed ICEs
                string[] suppressICEArray = new string[this.suppressICEs.Count];
                this.suppressICEs.CopyTo(suppressICEArray, 0);
                validator.SuppressedICEs = suppressICEArray;

                foreach (string inputFile in this.inputFiles)
                {
                    // set the default cube file
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string appDirectory = Path.GetDirectoryName(assembly.Location);

                    if (this.addDefault)
                    {
                           switch (Path.GetExtension(inputFile).ToLower(CultureInfo.InvariantCulture))
                        {
                            case ".msm":
                                validator.AddCubeFile(Path.Combine(appDirectory, "mergemod.cub"));
                                break;
                            case ".msi":
                                validator.AddCubeFile(Path.Combine(appDirectory, "darice.cub"));
                                break;
                            default:
                                throw new Exception("Unknown input file format - expected a .msi or .msm file.");
                        }
                    }

                    // print friendly message saying what file is being validated
                    Console.WriteLine(Path.GetFileName(inputFile));

                    try
                    {
                        validator.Validate(Path.GetFullPath(inputFile));
                    }
                    finally
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
                            Console.WriteLine("Temporary directory located at '{0}'.", validator.TempFilesLocation);
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

                // skip blank arguments
                if (null == arg || 0 == arg.Length)
                {
                    continue;
                }

                if ('-' == arg[0] || '/' == arg[0])
                {
                    string parameter = arg.Substring(1);

                    if ("cub" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired("-cub"));
                            return;
                        }

                        this.validator.AddCubeFile(args[i]);
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
                    else if ("nodefault" == parameter)
                    {
                        this.addDefault = false;
                    }
                    else if ("nologo" == parameter)
                    {
                        this.showLogo = false;
                    }
                    else if ("notidy" == parameter)
                    {
                        this.tidy = false;
                    }
                    else if (parameter.StartsWith("sice:"))
                    {
                        this.suppressICEs.Add(parameter.Substring(5));
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
