//-------------------------------------------------------------------------------------------------
// <copyright file="heat.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset Harvester application.
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
    using System.Xml;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The Windows Installer XML Toolset Harvester application.
    /// </summary>
    public sealed class Heat
    {
        private string extensionArgument;
        private StringCollection extensionOptions;
        private string extensionType;
        private ArrayList extensions;
        private SortedList extensionsByType;
        private string outputFile;
        private bool showLogo;
        private bool showHelp;
        private ConsoleMessageHandler messageHandler;

        /// <summary>
        /// Instantiate a new Heat class.
        /// </summary>
        private Heat()
        {
            this.extensionOptions = new StringCollection();
            this.extensions = new ArrayList();
            this.extensionsByType = new SortedList();
            this.messageHandler = new ConsoleMessageHandler("HEAT", "heat.exe");
            this.showLogo = true;
        }

        /// <summary>
        /// The main entry point for heat.
        /// </summary>
        /// <param name="args">Commandline arguments for the application.</param>
        /// <returns>Returns the application error code.</returns>
        [MTAThread]
        public static int Main(string[] args)
        {
            Heat heat = new Heat();
            return heat.Run(args);
        }

        /// <summary>
        /// Main running method for the application.
        /// </summary>
        /// <param name="args">Commandline arguments to the application.</param>
        /// <returns>Returns the application error code.</returns>
        private int Run(string[] args)
        {
            StringCollection extensionList = new StringCollection();
            HeatCore heatCore = new HeatCore(new MessageEventHandler(this.messageHandler.Display));

            try
            {
                // read the configuration file (heat.exe.config)
                AppCommon.ReadConfiguration(extensionList);

                // load any extensions
                foreach (string extensionType in extensionList)
                {
                    HeatExtension heatExtension = HeatExtension.Load(extensionType);

                    this.extensions.Add(heatExtension);

                    foreach (HeatCommandLineOption commandLineOption in heatExtension.CommandLineTypes)
                    {
                        if (this.extensionsByType.Contains(commandLineOption.Option))
                        {
                            throw new Exception();
                        }

                        this.extensionsByType.Add(commandLineOption.Option, heatExtension);
                    }

                    heatExtension.Core = heatCore;
                }

                // parse the command line
                this.ParseCommandLine(args);

                // parse the extension's command line arguments
                string[] extensionOptionsArray = new string[this.extensionOptions.Count];
                this.extensionOptions.CopyTo(extensionOptionsArray, 0);
                foreach (HeatExtension heatExtension in this.extensions)
                {
                    heatExtension.ParseOptions(this.extensionType, extensionOptionsArray);
                    if (heatCore.EncounteredError)
                    {
                        this.showHelp = true;
                    }
                }

                // exit if there was an error parsing the command line (otherwise the logo appears after error messages)
                if (this.messageHandler.EncounteredError)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                if (this.showLogo)
                {
                    Assembly heatAssembly = Assembly.GetExecutingAssembly();

                    Console.WriteLine("Microsoft (R) Windows Installer XML Toolset Harvester version {0}", heatAssembly.GetName().Version.ToString());
                    Console.WriteLine("Copyright (C) Microsoft Corporation 2006. All rights reserved.");
                    Console.WriteLine();
                }

                if (this.showHelp)
                {
                    Console.WriteLine(" usage:  heat.exe harvestType <harvester arguments> -out sourceFile.wxs");
                    Console.WriteLine();
                    Console.WriteLine("Supported harvesting types (use \"heat.exe <type> -?\" for more info):");

                    // output the harvest types alphabetically
                    SortedList harvestTypes = new SortedList();
                    foreach (HeatExtension heatExtension in this.extensions)
                    {
                        foreach (HeatCommandLineOption commandLineOption in heatExtension.CommandLineTypes)
                        {
                            harvestTypes.Add(commandLineOption.Option, commandLineOption);
                        }
                    }

                    foreach (HeatCommandLineOption commandLineOption in harvestTypes.Values)
                    {
                        Console.WriteLine("   {0,-7}  {1}", commandLineOption.Option, commandLineOption.Description);
                    }

                    Console.WriteLine();
                    Console.WriteLine("   -nologo  skip printing heat logo information");
                    Console.WriteLine("   -out     specify output file (default: write to current directory)");
                    Console.WriteLine("   -sw<N>   suppress warning with specific message ID");
                    Console.WriteLine("   -v       verbose output");
                    Console.WriteLine("   -?       this help information");
                    Console.WriteLine("");
                    Console.WriteLine("For more information see: http://wix.sourceforge.net");

                    return this.messageHandler.LastErrorNumber;
                }

                // harvest the output
                Wix.Wix wix = heatCore.Harvester.Harvest(this.extensionArgument);
                if (null == wix)
                {
                    return this.messageHandler.LastErrorNumber;
                }

                // mutate the output
                if (!heatCore.Mutator.Mutate(wix))
                {
                    return this.messageHandler.LastErrorNumber;
                }

                XmlTextWriter writer = null;

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
                    if ("nologo" == parameter)
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
                        else
                        {
                            this.outputFile = Path.GetFullPath(args[i]);
                        }
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
                    else
                    {
                        this.extensionOptions.Add(arg);
                    }
                }
                else if ('@' == arg[0])
                {
                    this.ParseCommandLine(AppCommon.ParseResponseFile(arg.Substring(1)));
                }
                else if (null == this.extensionType)
                {
                    this.extensionType = arg;
                }
                else if (null == this.extensionArgument)
                {
                    this.extensionArgument = arg;
                }
                else
                {
                    this.showHelp = true;
                }
            }

            if (null == this.outputFile)
            {
                this.showHelp = true;
            }

            return;
        }
    }
}
