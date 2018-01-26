using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EndpointDiagnostics
{
    class MainEntry
    {
        private static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();

            if (args.Count() != 4)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("Invalid Arguments");
                Console.WriteLine();
                Console.WriteLine("Please provide <search directory> <search filter i.e. *, *.config> <username> <password>");
                Console.WriteLine();

                sb.AppendLine("Invalid Arguments");
                sb.AppendLine();
                sb.AppendLine("Please provide <search directory> <search filter i.e. *, *.config> <username> <password>");
                sb.AppendLine();
            }
            else
            {
                if (!Directory.Exists(args[0]))
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine();
                    Console.WriteLine("The specified search directory doesn't exist!");
                    Console.WriteLine();

                    sb.AppendLine();
                    sb.AppendLine("The specified search directory doesn't exist!");
                    sb.AppendLine();
                }
                else
                {
                    foreach (string f in Directory.GetFiles(args[0], args[1], SearchOption.AllDirectories))
                    {
                        try
                        {
                            using (StreamReader sr = new StreamReader(f))
                            {
                                int lineNum = 1;
                                string line = sr.ReadLine();
                                while (line != null)
                                {
                                    MatchCollection mc = Regex.Matches(line, @"(http[^ \s]+)([^<>][\s]|$)", RegexOptions.IgnoreCase);

                                    if (mc != null)
                                    {
                                        foreach (Match m in mc)
                                        {
                                            try
                                            {
                                                Uri validUri = new Uri(m.Value);
                                                string adress = m.Value.Replace("/>", "");
                                                adress = Regex.Replace(adress, "</[a-zA-Z0-9]+>+", "", RegexOptions.IgnoreCase).Replace("\"", "");
                                                TestSite(f, lineNum, adress, args[2], args[3], sb);
                                            }
                                            catch
                                            {

                                            }
                                        }
                                    }

                                    line = sr.ReadLine();
                                    lineNum++;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            createLogFile(sb);
        }

        private static void createLogFile(StringBuilder sb)
        {
            if (!Directory.Exists("edl")) Directory.CreateDirectory("edl");

            using (StreamWriter sw = new StreamWriter("edl\\EndpointDiagnostics_" + DateTime.Now.ToString("MMddyyy_HHmmss") + ".txt"))
            {
                sw.Write(sb.ToString());
            }
        }

        private static void TestSite(string f, int lineNum, string Url, string user, string password, StringBuilder sb)
        {
            ConsoleColor cc = Console.ForegroundColor;

            Console.WriteLine();
            Console.WriteLine(f + " (line " + lineNum.ToString() + ")");
            Console.WriteLine("Testing " + Url);

            sb.AppendLine();
            sb.AppendLine(f + " (line " + lineNum.ToString() + ")");
            sb.AppendLine("Testing " + Url);

            string Message = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
                request.Credentials = new NetworkCredential(user, password);
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                }
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("No Connection");
                Console.Write(ex.Message);
                Console.WriteLine();

                sb.AppendLine("No Connection");
                sb.Append(ex.Message);
                sb.AppendLine();

                Console.ForegroundColor = cc;

                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Success");

            sb.AppendLine("Success");

            Console.ForegroundColor = cc;
        }
    }
}
