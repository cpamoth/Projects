using System;
using NDesk.Options;

namespace XapFileInjector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inputXapFile = string.Empty;
            var outputXapFile = string.Empty;
            var injectedFile = string.Empty;
            var injectedFileName = string.Empty;
            var helpRequested = false;

            var optionSet = new OptionSet {
                                 {
                                     "f=|file=", 
                                     "File to be placed into the XAP.",
                                     v => injectedFile = v
                                 },
                                 {
                                     "h|?|help",
                                     "Show this help information.",
                                     v => helpRequested = true
                                 },
                                 {
                                     "n=|name=",
                                     "Optional: Name to use for the file when placed into the XAP (file name only, no path - default behaviour is to use the name of the injected file).",
                                     v => injectedFileName = v
                                 },
                                 {
                                     "o=|output=",
                                     "Optional: Write the changed XAP file to this location rather than modifying the existing one (default behaviour is to overwrite existing file).",
                                     v => outputXapFile = v
                                 },
                                 {
                                     "x=|xap=", 
                                     "Source XAP file that file is to be injected into.",
                                     v => inputXapFile = v
                                 },
                                 
                             };
            optionSet.Parse(args);

            if (helpRequested)
            {
                ShowProgrammeInformation();
                ShowParameterInformation(optionSet);
                Environment.Exit(0);
            }

            InjectionInformation injectionInformation = null;

            try
            {
                
                injectionInformation = new InjectionInformation(inputXapFile, outputXapFile, injectedFile,
                                                                  injectedFileName);
            }
            catch(ArgumentException argumentException)
            {
                ShowError(argumentException.Message);
                ShowParameterInformation(optionSet);
                Environment.Exit(1);
            }

            FileInjector.Inject(injectionInformation);
        }

        private static void ShowError(string error)
        {
            Console.WriteLine("Error: " + error);
            Console.WriteLine("");
        }

        private static void ShowParameterInformation(OptionSet optionSet)
        {
            Console.WriteLine("PARAMETERS:");
            Console.WriteLine("=================");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static void ShowProgrammeInformation()
        {
            Console.WriteLine(
                "Takes a Silverlight .xap file, and places or replaces a specified file in it. It is commonly used to inject environment-specific configuration files into a .xap file after build.");
            Console.WriteLine("");
            Console.WriteLine("EXAMPLE");
            Console.WriteLine("=================");
            Console.WriteLine(
                @"XapFileInjector.exe -xap c:\something.xap -out c:\something_else.xap -file c:\some\location\my.xml -name something.xml");
            Console.WriteLine("");
            Console.WriteLine(
                @"Given a xap file c:\something.xap, and a file c:\some\location\my.xml, this program will create a new .xap file at c:\something_else.xap, which is a copy of the input .xap with the injected file added to it with the name something.xml (overwriting any existing file inside).");
            Console.WriteLine("");
        }
    }
}
