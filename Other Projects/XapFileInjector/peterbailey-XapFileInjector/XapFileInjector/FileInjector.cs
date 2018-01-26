using System.IO;
using Ionic.Zip;

namespace XapFileInjector
{
    internal static class FileInjector
    {
        internal static void Inject(InjectionInformation injectionInformation)
        {
            using (var zip = ZipFile.Read(injectionInformation.InputXapFile))
            {
                var existingFiles = zip.EntryFileNames;
                if (existingFiles.Contains(injectionInformation.InjectedFileName))
                    zip.RemoveEntry(injectionInformation.InjectedFileName);

                zip.AddEntry(injectionInformation.InjectedFileName, "", File.OpenText(injectionInformation.InjectedFile).ReadToEnd());

                zip.Save(injectionInformation.OutputXapFile);
            }
        }
    }
}
