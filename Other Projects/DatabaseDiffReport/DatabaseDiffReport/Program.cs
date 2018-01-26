using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseDiffReport
{
    class Program
    {
        private static List<CompareScenario> CompareList;
        private static string dbghost;
        private static string reportsDir;

        private static void Main(string[] args)
        {
            GetDBGhostPath();
            GetReportsDir();
            UseArgs(args);

            ExecuteCompareList();
            CleanupReports();
            //OrganizeReports();
        }

        private static void GetDBGhostPath()
        {
            try
            {
                dbghost = ConfigurationManager.AppSettings["DBGhost"];
            }
            catch (ConfigurationException ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.Message);
                Environment.Exit(2);
            }

            if (!File.Exists(dbghost))
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine("Could not find part of the path " + dbghost);
                Console.WriteLine("Please check to make sure it is installed and that the path is defined corrctly in the configuration.");
                Environment.Exit(1);
            }
        }

        private static void GetReportsDir()
        {
            try
            {
                reportsDir = ConfigurationManager.AppSettings["ReportsDirectory"];
            }
            catch (ConfigurationException ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.Message);
                Environment.Exit(2);
            }
        }

        private static void UseArgs(string[] args)
        {
            //Create reports directory
            if (!Directory.Exists(reportsDir))
            {
                Directory.CreateDirectory(reportsDir);
            }

            CompareList = new List<CompareScenario>();

            string naming = Guid.NewGuid().ToString();
            CompareList.Add(new CompareScenario
            {
                ReportPath = Path.Combine(reportsDir, naming + ".txt"),
                SourceDB = args[0].Trim(),
                SourceSvr = args[1].Trim(),
                TargetDB = args[2].Trim(),
                TargetSvr = args[3].Trim(),
                TemplateName = naming + ".dbgcm"
            });
        }

        private static void ExecuteCompareList()
        {
            foreach (CompareScenario cs in CompareList)
            {
                Compare(cs);
            }

            Console.WriteLine();
            Console.WriteLine("Compare complete.");
            Console.WriteLine();
        }

        private static void CleanupReports()
        {
            Console.WriteLine("Generating Reports...");

            foreach (CompareScenario cs in CompareList)
            {
                FinalizeReport(cs);
            }

            Console.WriteLine();
            Console.WriteLine("Reports complete.");
            Console.WriteLine();
        }

        private static void FinalizeReport(CompareScenario cs)
        {
            try
            {
                if (File.Exists(cs.ReportPath))
                {
                    StringBuilder sb = new StringBuilder();
                    using (StreamReader sr = new StreamReader(cs.ReportPath))
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            if (!line.Contains("...Checking for"))
                            {
                                int start = line.IndexOf("...");
                                sb.AppendLine(line.Remove(0, start + 3).Replace("source:", cs.SourceDB + "|" + cs.SourceSvr + ":").Replace("target:", cs.TargetDB + "|" + cs.TargetSvr + ":").Replace(" the target ", " " + cs.TargetDB + "|" + cs.TargetSvr + " ").Replace(" the source ", " " + cs.SourceDB + "|" + cs.SourceSvr + " "));
                            }

                            line = sr.ReadLine();
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("RESULTS:");
                    Console.WriteLine();
                    Console.Write(sb.ToString());
                    Console.WriteLine();

                    CleanUp(cs.ReportPath);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Could not find generated report file.  " + cs.ReportPath);
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.Write(ex.Message);
                Console.WriteLine();
            }
        }

        private static void Compare(CompareScenario cs)
        {
            try
            {
                GenerateTemplate(cs);

                Console.WriteLine();
                Console.WriteLine("Comparing " + cs.TargetDB + " on " + cs.TargetSvr + " to " + cs.SourceDB + " on " + cs.SourceSvr + "...");
                ExecuteDBGhost(cs.TemplateName);
                CleanUp(cs.TemplateName);
            }
            catch(Exception ex)
            {
                Console.WriteLine();
                Console.Write(ex.Message);
                Console.WriteLine();
            }
        }

        private static void GenerateTemplate(CompareScenario cs)
        {
            using (StreamWriter sw = new StreamWriter(cs.TemplateName))
            {
                sw.Write(Templates.CompareReportSettings.Replace("***TARGETDB***", cs.TargetDB).Replace("***TARGETSVR***", cs.TargetSvr).Replace("***SOURCEDB***", cs.SourceDB).Replace("***SOURCESVR***", cs.SourceSvr).Replace("***REPORTFILE***", cs.ReportPath));
            }
        }

        private static void ExecuteDBGhost(string template)
        {
            Console.WriteLine();
            Console.WriteLine("Executing DB Ghost (" + template + ")...");
            Console.WriteLine();

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = dbghost;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Arguments = template;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.Start();
                proc.WaitForExit();

                //string std = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(err))
                {
                    Console.WriteLine();
                    Console.WriteLine("DB Ghost error:");
                    Console.WriteLine();
                    Console.Write(err.Replace("sqldeploy", "*****").Replace("pyTQpeg2", "*****"));
                    Environment.Exit(1);
                }
            }
        }

        private static void CleanUp(string toDelete)
        {
            Console.WriteLine("Deleting " + toDelete + "...");
            if (File.Exists(toDelete))
            {
                File.Delete(toDelete);
            }
        }
    }
}
