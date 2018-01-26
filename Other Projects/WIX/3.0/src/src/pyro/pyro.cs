//-------------------------------------------------------------------------------------------------
// <copyright file="pyro.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The pyro patch builder application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml;

    /// <summary>
    /// The pyro patch builder application.
    /// </summary>
    public sealed class Pyro
    {
        private string cabCachePath;
        private string extension;
        private string inputFile;
        private Hashtable inputTransforms;
        private ConsoleMessageHandler messageHandler;
        private string outputFile;
        private bool reuseCabinets;
        private bool showHelp;
        private bool showLogo;
        private bool suppressAssemblies;
        private bool suppressFileHashAndInfo;
        private bool suppressFiles;
        private bool tidy;
        private WixVariableResolver wixVariableResolver;

        /// <summary>
        /// Instantiate a new Pyro class.
        /// </summary>
        private Pyro()
        {
            this.messageHandler = new ConsoleMessageHandler("PYRO", "pyro.exe");
            this.showLogo = true;
            this.tidy = true;
            this.inputTransforms = new Hashtable();

            // set the message handler
            this.Message += new MessageEventHandler(this.messageHandler.Display);

            this.wixVariableResolver = new WixVariableResolver();
            this.wixVariableResolver.Message += new MessageEventHandler(this.messageHandler.Display);
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        private event MessageEventHandler Message;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments to pyro.</param>
        /// <returns>0 if sucessful, otherwise 1.</returns>
        public static int Main(string[] args)
        {
            Pyro pyro = new Pyro();
            return pyro.Run(args);
        }

        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            Microsoft.Tools.WindowsInstallerXml.Binder binder = new Microsoft.Tools.WindowsInstallerXml.Binder();

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
                    Assembly pyroAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer Xml Patch Builder Version {0}", pyroAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.\n");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage: pyro.exe [-?] [-nologo] inputFile -out outputFile [-t baseline wixTransform]");
                    Console.WriteLine();
                    Console.WriteLine("   -cc        path to cache built cabinets");
                    Console.WriteLine("   -ext       extension assembly or \"class, assembly\"");
                    Console.WriteLine("   -nologo    skip printing logo information");
                    Console.WriteLine("   -notidy    do not delete temporary files (useful for debugging)");
                    Console.WriteLine("   -out       specify output file");
                    Console.WriteLine("   -reusecab  reuse cabinets from cabinet cache");
                    Console.WriteLine("   -sa        suppress assemblies: do not get assembly name information for assemblies");
                    Console.WriteLine("   -sf        suppress files: do not get any file information (equivalent to -sa and -sh)");
                    Console.WriteLine("   -sh        suppress file info: do not get hash, version, language, etc");
                    Console.WriteLine("   -sw<N>     suppress warning with specific message ID");
                    Console.WriteLine("   -t baseline transform  one or more wix transforms and its baseline");
                    Console.WriteLine("   -v         verbose output");
                    Console.WriteLine("   -wx        treat warnings as errors");
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

                // Load the extension if one was passed on the command line.
                WixExtension wixExtension = null;
                if (null != this.extension)
                {
                    wixExtension = WixExtension.Load(extension);
                }

                // Load in transforms
                ArrayList transforms = new ArrayList();
                foreach (DictionaryEntry inputTransform in inputTransforms)
                {
                    Output transformOutput = Output.Load(inputTransform.Key.ToString(), false, false);
                    PatchTransform patchTransform = new PatchTransform(transformOutput, (string) inputTransform.Value);
                    transforms.Add(patchTransform);
                }

                // Load the patch
                Patch patch = new Patch();
                patch.Load(this.inputFile);

                // Copy transforms into output
                if (0 < transforms.Count)
                {
                    patch.AttachTransforms(transforms);
                }

                // Create and configure the binder
                binder = new Microsoft.Tools.WindowsInstallerXml.Binder();
                binder.TempFilesLocation = Environment.GetEnvironmentVariable("WIX_TEMP");
                binder.WixVariableResolver = this.wixVariableResolver;
                binder.Message += new MessageEventHandler(this.messageHandler.Display);
                binder.SuppressAssemblies = this.suppressAssemblies;
                binder.SuppressFileHashAndInfo = this.suppressFileHashAndInfo;

                if (this.suppressFiles)
                {
                    binder.SuppressAssemblies = true;
                    binder.SuppressFileHashAndInfo = true;
                }

                // Load the extension if provided and set all its properties
                if (null != wixExtension)
                {
                    binder.Extension = wixExtension.BinderExtension;
                }

                if (null != this.cabCachePath || this.reuseCabinets)
                {
                    // ensure the cabinet cache path exists if we are going to use it
                    if (null != this.cabCachePath && !Directory.Exists(this.cabCachePath))
                    {
                        Directory.CreateDirectory(this.cabCachePath);
                    }
                }

                binder.Extension.ReuseCabinets = this.reuseCabinets;
                binder.Extension.CabCachePath = this.cabCachePath;
                binder.Extension.Output = patch.PatchOutput;

                // Bind the patch to an msp.
                binder.Bind(patch.PatchOutput, this.outputFile);
            }
            catch (WixException we)
            {
                this.OnMessage(we.Error);
            }
            catch (Exception e)
            {
                this.OnMessage(WixErrors.UnexpectedException(e.Message, e.GetType().ToString(), e.StackTrace));
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
                        Console.WriteLine("Temporary directory located at '{0}'.", binder.TempFilesLocation);
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
                    
                    if ("cc" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.DirectoryPathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.cabCachePath = args[i];
                    }
                    else if ("ext" == parameter)
                    {
                        if (null != this.extension)
                        {
                            this.messageHandler.Display(this, WixErrors.SingleExtensionSupported());
                            return;
                        }

                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.TypeSpecificationForExtensionRequired("-ext"));
                            return;
                        }

                        this.extension = args[i];
                    }
                    else if ("nologo" == parameter)
                    {
                        this.showLogo = false;
                    }
                    else if ("notidy" == parameter)
                    {
                        this.tidy = false;
                    }
                    else if (parameter.StartsWith("sw"))
                    {
                        try
                        {
                            int suppressWarning = Convert.ToInt32(parameter.Substring(2), CultureInfo.InvariantCulture.NumberFormat);

                            if (0 >= suppressWarning)
                            {
                                this.OnMessage(WixErrors.IllegalSuppressWarningId(parameter.Substring(2)));
                            }

                            this.messageHandler.SuppressWarningMessage(suppressWarning);
                        }
                        catch (FormatException)
                        {
                            this.OnMessage(WixErrors.IllegalSuppressWarningId(parameter.Substring(2)));
                        }
                        catch (OverflowException)
                        {
                            this.OnMessage(WixErrors.IllegalSuppressWarningId(parameter.Substring(2)));
                        }
                    }
                    if ("o" == parameter || "out" == parameter)
                    {
                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        this.outputFile = Path.GetFullPath(args[i]);
                    }
                    else if ("reusecab" == parameter)
                    {
                        this.reuseCabinets = true;
                    }
                    else if ("sa" == parameter)
                    {
                        this.suppressAssemblies = true;
                    }
                    else if ("sf" == parameter)
                    {
                        this.suppressFiles = true;
                    }
                    else if ("sh" == parameter)
                    {
                        this.suppressFileHashAndInfo = true;
                    }
                    else if ("t" == parameter)
                    {
                        string transform = null;
                        string baseline = null;

                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.BaselineRequired());
                            return;
                        }

                        baseline = args[i];

                        if (args.Length < ++i || '/' == args[i][0] || '-' == args[i][0])
                        {
                            this.messageHandler.Display(this, WixErrors.FilePathRequired(String.Concat("-", parameter)));
                            return;
                        }

                        transform = Path.GetFullPath(args[i]).ToLower();

                        // Verify the transform hasnt already been added.
                        if (this.inputTransforms.ContainsKey(transform))
                        {
                            this.messageHandler.Display(this, WixErrors.DuplicateTransform(transform));
                            return;
                        }

                        this.inputTransforms.Add(transform, baseline);
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
                    if (null == this.inputFile)
                    {
                        this.inputFile = Path.GetFullPath(args[i]);
                    }
                    else
                    {
                        this.OnMessage(WixErrors.AdditionalArgumentUnexpected(arg));
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        private void OnMessage(MessageEventArgs mea)
        {
            if (null != this.Message)
            {
                this.Message(this, mea);
            }
        }
    }
}
