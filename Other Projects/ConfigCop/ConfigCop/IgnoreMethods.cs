using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    class IgnoreMethods
    {
        public static List<string> GetIgnoreDirectories()
        {
            List<string> toIgnore = new List<string>();
            foreach (string id in HelperMethods.CheckConfiguration("ignoreDirs").Split(','))
            {
                toIgnore.Add(id.ToUpper());
            }

            return toIgnore;
        }

        public static List<string> GetIgnoreFiles()
        {
            List<string> toIgnore = new List<string>();
            foreach (string id in HelperMethods.CheckConfiguration("ignoreFiles").Split(','))
            {
                toIgnore.Add(id.ToUpper());
            }

            return toIgnore;
        }

        public static bool IgnorePath(List<string> toIgnore, string dirOrFile)
        {
            bool ignore = false;
            foreach (string ignoreDir in toIgnore)
            {
                dirOrFile = dirOrFile.ToUpper();
                if (dirOrFile.Contains(ignoreDir.ToUpper()))
                {
                    ignore = true;
                }
            }
            return ignore;
        }

        public static void GetIgnores()
        {
            Program.IgnoreList = new List<IgnoreObj>();
            string ignoreFile = HelperMethods.CheckConfiguration("ignoreFile");
            if (File.Exists(ignoreFile))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Configured to use " + ignoreFile + " to filter tests.");
                Console.ResetColor();

                using (StreamReader sr = new StreamReader(ignoreFile))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        AddIgnoresForFile(line);
                        line = sr.ReadLine();
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("No existing ignoreFile has been specified in the configuration.");
                Console.ResetColor();
            }
        }

        private static void AddIgnoresForFile(string line)
        {
            if (!line.StartsWith("-"))
            {
                string[] parts = line.Split('|');
                string fPath = parts[0].Trim().ToUpper();
                List<string> ignores = new List<string>();
                if (parts.Count() > 1)
                {
                    foreach (string i in parts[1].Split(','))
                    {
                        ignores.Add(i.Trim().ToUpper());
                    }
                }

                if (!string.IsNullOrEmpty(fPath) && ignores.Count() > 0)
                {
                    Program.IgnoreList.Add(new IgnoreObj
                    {
                        FilePath = fPath,
                        IgnoreList = ignores
                    });
                }
            }
        }

        public static void GetIgnoresForFile(string filePath)
        {
            Program.Ignores = new List<string>();
            var existing = from f in Program.IgnoreList where f.FilePath == filePath.ToUpper() || f.FilePath == "*" select f.IgnoreList;
            if (existing != null && existing.Count() > 0)
            {
                foreach(var grp in existing)
                {
                    foreach (string i in grp)
                    {
                        var have = from added in Program.Ignores where added == i select added;
                        if (have == null || have.Count() == 0)
                        {
                            Program.Ignores.Add(i);
                        }
                    }
                }
            }
        }

        public static bool Ignore(string test)
        {
            bool ignore = false;
            if (Program.Ignores.Count() > 0)
            {
                foreach (string i in Program.Ignores)
                {
                    if (test.ToUpper().Contains(i))
                    {
                        ignore = true;
                    }
                }
            }

            return ignore;
        }
    }
}
