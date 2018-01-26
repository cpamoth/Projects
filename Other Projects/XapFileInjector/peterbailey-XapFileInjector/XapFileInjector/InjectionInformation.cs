using System;
using System.IO;

namespace XapFileInjector
{
    internal class InjectionInformation
    {
        internal string InputXapFile { get; set; }
        internal string OutputXapFile { get; set; }
        internal string InjectedFile { get; set; }
        internal string InjectedFileName { get; set; }

        public InjectionInformation(string inputXapFile, string outputXapFile, string injectedFile, string injectedFileName)
        {
            InputXapFile = inputXapFile;
            OutputXapFile = outputXapFile;
            InjectedFile = injectedFile;
            InjectedFileName = injectedFileName;

            if (string.IsNullOrEmpty(InputXapFile))
                throw new ArgumentException("Input xap file must be specified.");

            if (string.IsNullOrEmpty(InjectedFile))
                throw new ArgumentException("Injected file must be specified.");

            if (!File.Exists(InputXapFile))
                throw new ArgumentException("Input xap file does not exist.");

            if (!File.Exists(InjectedFile))
                throw new ArgumentException("Injected file does not exist.");

            if (string.IsNullOrEmpty(OutputXapFile))
                OutputXapFile = InputXapFile;

            if (string.IsNullOrEmpty(InjectedFileName))
                InjectedFileName = new FileInfo(InjectedFile).Name;
        }
    }
}