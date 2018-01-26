using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    class FileContentCompareMethods
    {
        public static void CompareFileContents(FileInfo fi)
        {
            string contentFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileContentChecks");
            contentFile = Path.Combine(contentFile, fi.Name);
            if (File.Exists(contentFile))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine("Comparing contents of " + fi.FullName + " to " + contentFile + ".");
                Console.ResetColor();

                StringBuilder found = new StringBuilder();
                using (StreamReader sr = new StreamReader(fi.FullName))
                {
                    found.AppendLine(sr.ReadToEnd());
                }

                bool equal = false;
                bool contain = false;
                bool ignoreCase = false;
                StringBuilder contents = new StringBuilder();
                List<string> contentList = new List<string>();
                using (StreamReader sr = new StreamReader(contentFile))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        switch (line.Trim().ToUpper())
                        {
                            case "EQUAL":
                                equal = true;
                                break;
                            case "CONTAIN":
                                contain = true;
                                break;
                            case "IGNORECASE":
                                ignoreCase = true;
                                break;
                            default:
                                contents.AppendLine(line);
                                contentList.Add(line);
                                break;
                        }

                        line = sr.ReadLine();
                    }
                }

                if (equal == true)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("File contents should match...");
                    Console.ResetColor();

                    if (ignoreCase == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Ignoring case...");
                        Console.ResetColor();

                        bool result = string.Equals(found.ToString(), contents.ToString(), StringComparison.CurrentCultureIgnoreCase);

                        ContentsMatch(result);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Case sensitive...");
                        Console.ResetColor();

                        bool result = string.Equals(found.ToString(), contents.ToString(), StringComparison.CurrentCulture);

                        ContentsMatch(result);
                    }
                }
                else if (contain == true)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Checking that the file contains the specified content...");
                    Console.ResetColor();

                    if (ignoreCase == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Ignoring case...");
                        Console.ResetColor();

                        CheckForLine(found, contentList, ignoreCase);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Case sensitive...");
                        Console.ResetColor();

                        CheckForLine(found, contentList, ignoreCase);
                    }
                }
            }
        }

        private static void CheckForLine(StringBuilder found, List<string> contentList, bool ignoreCase)
        {
            string foundUse = found.ToString();
            if (ignoreCase == true)
            {
                foundUse = foundUse.ToUpper();
            }

            foreach(string line in contentList)
            {
                string lineUse = line;

                if (ignoreCase == true)
                {
                    lineUse = lineUse.ToUpper();
                }

                if (!foundUse.Contains(lineUse.Trim()))
                {
                    int sc = Errors.GetStatusCode("MissingContent");
                    if (sc != 0)
                    {
                        Console.ForegroundColor = Errors.GetColorByStatus(sc);
                        Console.WriteLine("File does not contain specified content!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(line);
                        Console.ResetColor();

                        Errors.MissingContent();
                    }
                }
            }
        }

        private static void ContentsMatch(bool result)
        {
            if (result == false)
            {
                int sc = Errors.GetStatusCode("MissingContent");
                if (sc != 0)
                {
                    Console.ForegroundColor = Errors.GetColorByStatus(sc);
                    Console.WriteLine("File contents do not match!");
                    Console.ResetColor();

                    Errors.MissingContent();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Contents match.");
                Console.ResetColor();
            }
        }
    }
}
