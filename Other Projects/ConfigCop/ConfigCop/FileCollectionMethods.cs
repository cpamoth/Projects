using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    class FileCollectionMethods
    {
        public static List<string> GatherFilesToAnalyze()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("Searching for files in the following directories...");
            Console.ResetColor();

            List<string> searchFiles = new List<string>();

            List<DirectorySearchInfo> searchDirList = GatherDirectoriesToSearch();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(searchDirList.Count() + " directories found");
            Console.ResetColor();

            //Get a list of files to ignore from the config file
            List<string> toIgnore = IgnoreMethods.GetIgnoreFiles();

            foreach (DirectorySearchInfo dir in searchDirList)
            {
                foreach (string fType in dir.FileTypes)
                {
                    try
                    {
                        foreach (string f in Directory.GetFiles(dir.DirPath, "*" + fType, SearchOption.TopDirectoryOnly))
                        {
                            FileInfo fi = new FileInfo(f);
                            if (IgnoreMethods.IgnorePath(toIgnore, fi.Name) == false)
                            {
                                searchFiles.Add(f);
                            }
                        }
                    }
                    catch (System.UnauthorizedAccessException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(ex.Message);
                        Console.ResetColor();

                        Program.Report.UnauthorizedDirectories.Add(ex.Message);
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(searchFiles.Count() + " files found.");
            Console.WriteLine();
            Console.ResetColor();

            return searchFiles;
        }

        private static List<DirectorySearchInfo> GatherDirectoriesToSearch()
        {
            List<DirectorySearchInfo> searchDirectories = new List<DirectorySearchInfo>();

            //Get a list of directories to ignore from the config file
            List<string> toIgnore = IgnoreMethods.GetIgnoreDirectories();
            toIgnore.Add("FileContentChecks");
            toIgnore.Add("ConfigCopPages");

            //Get the file types to search for as configured for the directory
            List<string> fTypes = HelperMethods.CheckConfiguration("fileTypes").Split(',').ToList<string>();

            //Add passed directory or working directory if none was passed
            if (IgnoreMethods.IgnorePath(toIgnore, Program.SearchDir) == false)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Program.SearchDir);
                Console.ResetColor();
                AddSearchDirectory(searchDirectories, Program.SearchDir, fTypes, toIgnore);
            }

            //Get all the subdirectories
            List<string> subDirs = new List<string>();
            GetSubDirs(Program.SearchDir, toIgnore, subDirs);
            foreach (string subD in subDirs)
            {
                AddSearchDirectory(searchDirectories, subD, fTypes, toIgnore);
            }

            //Get all the additional directories to search from the config file
            string fullSearchDirs = HelperMethods.CheckConfiguration("searchDirsFull");
            if (!string.IsNullOrEmpty(fullSearchDirs))
            {
                string[] searchDirListFull = fullSearchDirs.Split(';');
                GatherConfiguredDirectories(searchDirectories, toIgnore, searchDirListFull, true);
            }
            string topSearchDirs = HelperMethods.CheckConfiguration("searchDirsTopOnly");
            if (!string.IsNullOrEmpty(topSearchDirs))
            {
                string[] searchDirListTop = topSearchDirs.Split(';');
                GatherConfiguredDirectories(searchDirectories, toIgnore, searchDirListTop, false);
            }

            return searchDirectories;
        }

        private static void GatherConfiguredDirectories(List<DirectorySearchInfo> searchDirectories, List<string> toIgnore, string[] searchDirList, bool full)
        {
            if (searchDirList.Count() > 0)
            {
                foreach (string dir in searchDirList)
                {
                    string[] parts = dir.Split('|');
                    string d = parts[0];

                    //Get the file types to search for as configured for the directory
                    List<string> fTypes = ExtractFileTypesFromSearchDirs(parts);

                    //Add configured directory
                    if (IgnoreMethods.IgnorePath(toIgnore, d) == false)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(d);
                        Console.ResetColor();
                        AddSearchDirectory(searchDirectories, d, fTypes, toIgnore);
                    }

                    if (full == true)
                    {
                        //Get all the subdirectories
                        List<string> subDirs = new List<string>();
                        GetSubDirs(d, toIgnore, subDirs);
                        foreach (string subD in subDirs)
                        {
                            AddSearchDirectory(searchDirectories, subD, fTypes, toIgnore);
                        }
                    }
                }
            }
        }

        private static List<string> GetSubDirs(string parent, List<string> toIgnore, List<string> subDirs)
        {
            if (IgnoreMethods.IgnorePath(toIgnore, parent) == false)
            {
                try
                {
                    //Get all the subdirectories
                    var dirList = Directory.EnumerateDirectories(parent, "*", SearchOption.AllDirectories);
                    foreach (string d in dirList)
                    {
                        if (IgnoreMethods.IgnorePath(toIgnore, d) == false)
                        {
                            try
                            {
                                var test = Directory.GetDirectories(d, "*", SearchOption.TopDirectoryOnly);
                                subDirs.Add(d);
                                GetSubDirs(d, toIgnore, subDirs);
                            }
                            catch (System.UnauthorizedAccessException ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(ex.Message);
                                Console.ResetColor();

                                Program.Report.UnauthorizedDirectories.Add(ex.Message);
                            }
                        }
                    }
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();

                    Program.Report.UnauthorizedDirectories.Add(ex.Message);
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();

                    Program.Report.UnauthorizedDirectories.Add(ex.Message);
                }
            }

            return subDirs;
        }

        private static void AddSearchDirectory(List<DirectorySearchInfo> searchDirectories, string dir, List<string> fTypes, List<string> toIgnore)
        {
            if (IgnoreMethods.IgnorePath(toIgnore, dir) == false)
            {
                searchDirectories.Add(new DirectorySearchInfo
                {
                    DirPath = dir,
                    FileTypes = fTypes
                });
            }
        }

        private static List<string> ExtractFileTypesFromSearchDirs(string[] parts)
        {
            List<string> fTypes = new List<string>();
            string setting = "";
            if (parts.Count() == 2)
            {
                setting = parts[1];
            }

            if (string.IsNullOrEmpty(setting) || setting == "*")
            {
                //Use default file types
                fTypes = HelperMethods.CheckConfiguration("fileTypes").Split(',').ToList<string>();
            }
            else
            {
                fTypes = setting.Split(';').ToList<string>();
            }

            return fTypes;
        }
    }
}
