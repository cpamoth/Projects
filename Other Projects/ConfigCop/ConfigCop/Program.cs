using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ConfigCop
{
    class Program
    {
        public static ReportObj Report;
        public static FileAnalysis CurrentFile;
        public static string Region;
        public static string SearchDir;
        public static bool LineError;
        public static List<AppSettingInfo> AppSettingsCollection;
        public static List<IgnoreObj> IgnoreList;
        public static List<BrowseObj> BrowseList;
        public static List<string> Ignores;

        static void Main(string[] args)
        {
            IgnoreMethods.GetIgnores();
            EndpointMethods.GetBrowseObjects();
            AppSettingsCollection = new List<AppSettingInfo>();
            InstantiateReport();

            SearchDir = "";
            Region = "";
            //Testing
            //args = ".,dev2".Split(',');
            //SearchDir = ".";
            //Region = "dev2";
            SetArgs(args);
            StartAnalysis();
            WriteReport();
            //Console.ReadLine();
        }

        private static void InstantiateReport()
        {
            Report = new ReportObj();
            Report.ReportDate = DateTime.Now;
            Report.AlalyzedFiles = new List<FileAnalysis>();
            Report.UnauthorizedDirectories = new List<string>();
            Report.DuplicateKeys = new List<AppSettingInfo>();
            Report.UnauthorizedDirectories = new List<string>();
            Report.Connections = new List<TestedConnection>();
        }

        private static void InstantiateCurrentFile(FileInfo fi)
        {
            CurrentFile = new FileAnalysis();
            CurrentFile.AlalyzedFile = fi.Name;
            CurrentFile.FullPath = fi.FullName;
            CurrentFile.LastModified = fi.LastWriteTime;
            CurrentFile.Discrepancies = new List<Discrepancy>();
        }

        private static void SetArgs(string[] args)
        {
            string[] regions = HelperMethods.CheckConfiguration("regions").Split(',');
            if (args.Count() > 0)
            {
                foreach (string a in args)
                {
                    if (Directory.Exists(a))
                    {
                        SearchDir = a;
                    }
                    else
                    {
                        if (regions.Contains(a, StringComparer.OrdinalIgnoreCase))
                        {
                            Region = a;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(SearchDir))
            {
                SearchDir = GetSearchDirectory(SearchDir);
            }

            if (string.IsNullOrEmpty(Region))
            {
                Region = AutoDetermineRegion(regions);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("Analyzing  as " + Region + " region...");
            Console.WriteLine();
            Console.ResetColor();
        }

        private static string AutoDetermineRegion(string[] regions)
        {
            string machine = Environment.MachineName.ToUpper();
            foreach (string r in regions)
            {
                if (machine.StartsWith(r.ToUpper()))
                {
                    Region = r;
                }
            }

            if (string.IsNullOrEmpty(Region))
            {
                foreach (string r in regions)
                {
                    if (machine.Contains(r.ToUpper()))
                    {
                        Region = r;
                    }
                }
            }

            return Region;
        }

        private static void StartAnalysis()
        {
            List<string> searchFiles = FileCollectionMethods.GatherFilesToAnalyze();
            if (searchFiles != null && searchFiles.Count() > 0)
            {
                foreach (string f in searchFiles)
                {
                    FileInfo fi = new FileInfo(f);
                    if (!fi.Name.ToUpper().StartsWith("CONFIGCOP"))
                    {
                        InstantiateCurrentFile(fi);

                        FileContentCompareMethods.CompareFileContents(fi);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine();
                        Console.WriteLine("Analyzing " + f);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Last Modified: " + fi.LastWriteTime);
                        Console.WriteLine();
                        Console.ResetColor();

                        IgnoreMethods.GetIgnoresForFile(fi.FullName);

                        if (HelperMethods.ValidateXML(fi) == true)
                        {
                            ExtractLinesXML(fi);
                        }
                        else
                        {
                            ExtractLinesStd(fi);
                        }

                        Report.AlalyzedFiles.Add(CurrentFile);
                    }
                }
            }

            GeneralConfigCheckMethods.CheckForDuplicateKeys();
        }

        private static void ExtractLinesStd(FileInfo fi)
        {
            try
            {
                using (StreamReader sr = new StreamReader(fi.FullName))
                {
                    int lineNum = 1;
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string commentChar = "";
                        try
                        {
                            commentChar = ConfigurationManager.AppSettings["comment" + fi.Extension.ToLower()];
                        }
                        catch
                        {
                            //No comment character defined
                        }


                        if (!string.IsNullOrEmpty(commentChar))
                        {
                            if (!line.Trim().StartsWith(commentChar))
                            {
                                TestLine(lineNum, line);
                            }
                        }
                        else
                        {
                            TestLine(lineNum, line);
                        }

                        line = sr.ReadLine();
                        lineNum++;
                    }
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                int sc = Errors.GetStatusCode("UnauthorizedAccessException");
                Console.ForegroundColor = Errors.GetColorByStatus(sc);
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Errors.ReportDiscrepancy(0, "Unable to Analyze file.", "Access Denied", sc);

                return;
            }
            catch (Exception ex)
            {
                int sc = Errors.GetStatusCode("Default");
                Console.ForegroundColor = Errors.GetColorByStatus(sc);
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Errors.ReportDiscrepancy(0, "Unable to Analyze file.", "Unknown Error.", sc);


                return;
            }
        }

        private static void ExtractLinesXML(FileInfo fi)
        {
            try
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreComments = true;
                readerSettings.DtdProcessing = DtdProcessing.Parse;
                string content = "";
                using (XmlReader reader = XmlTextReader.Create(fi.FullName, readerSettings))
                {
                    while (reader.Read())
                    {
                        content = reader.ReadOuterXml();
                    }
                }

                using (StringReader sr = new StringReader(content))
                {
                    int lineNum = 2;
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        line = line.Trim();
                        TestLine(lineNum, line);

                        if (fi.Extension.ToUpper() == ".CONFIG")
                        {
                            HelperMethods.CollectAppSetting(fi, line);
                        }

                        line = sr.ReadLine();
                        lineNum++;
                    }
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                int sc = Errors.GetStatusCode("UnauthorizedAccessException");
                Console.ForegroundColor = Errors.GetColorByStatus(sc);
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Errors.ReportDiscrepancy(0, "Unable to Analyze file.", "Access Denied", sc);

                return;
            }
            catch (Exception ex)
            {
                int sc = Errors.GetStatusCode("Default");
                Console.ForegroundColor = Errors.GetColorByStatus(sc);
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Errors.ReportDiscrepancy(0, "Unable to Analyze file.", "Unknown Error.", sc);

                return;
            }
        }

        private static void TestLine(int lineNum, string line)
        {
            LineError = false;
            RegionCheckMethods.CheckForRegionDiscrepancies(line, lineNum);
            ConnectionStringMethods.CheckForDatabaseServerDiscrepancies(line, lineNum);
            EndpointMethods.CheckEndpoint(line, lineNum);

            if (LineError == true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(line.Trim());
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        private static string GetSearchDirectory(string searchDir)
        {
            if (string.IsNullOrEmpty(searchDir))
            {
                searchDir = Environment.CurrentDirectory;
            }
            return searchDir;
        }

        private static void WriteReport()
        {
            EchoTotals();

            XmlSerializer serializer = new XmlSerializer(typeof(ReportObj));

            ReportObj ro = new ReportObj();
            ro.ReportDate = Report.ReportDate;
            ro.AlalyzedFiles = (from f in Report.AlalyzedFiles where f.Discrepancies.Count() > 0 select f).ToList<FileAnalysis>();
            ro.DuplicateKeys = Report.DuplicateKeys;
            ro.UnauthorizedDirectories = Report.UnauthorizedDirectories;

            // Create the xml file
            string report = HelperMethods.CheckConfiguration("logPath");
            if (File.Exists(report))
            {
                try
                {
                    TextWriter textWriter = new StreamWriter(report, false);
                    serializer.Serialize(textWriter, ro);
                    textWriter.Close();
                }
                catch (UnauthorizedAccessException)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine(System.Environment.UserName + " Does not have access to the path " + report + ".  No log will be created.");
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("Could not find part of the path " + report + ".  No log will be created.");
                Console.WriteLine();
                Console.ResetColor();
            }

            string epPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigCopPages");
            epPage = Path.Combine(epPage, "Endpoints.html");
            string html = GenEnpointPageHtml();
            if (File.Exists(epPage))
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(epPage, false))
                    {
                        sw.Write(html);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine(System.Environment.UserName + " Does not have access to the path " + epPage + ".  Endpoints.html will not be created.");
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine();
                Console.WriteLine("Could not find part of the path " + epPage + ".  Endpoints.html will not be created.");
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        private static string GenEnpointPageHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\" />");
            sb.AppendLine("<title>Endpoint Report</title>");
            sb.AppendLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"Styles/Styles.css\">");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<header>");
            sb.AppendLine("<div class=\"banner\">");
            sb.AppendLine("<h1>ConfigCop Failed Connection Report</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine("</header>");
            sb.AppendLine("<section>");
            sb.AppendLine("<div class=\"headerStrip\">");
            sb.AppendLine("<h3>Servers</h3>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"mainDiv\">");
            foreach (var tc in from c in Report.Connections where c.StatusCode != 0 && c.ConType == 2 select c)
            {
                sb.AppendLine("<p>telnet " + tc.Connection + "  " + tc.Port + "</p>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"headerStrip\">");
            sb.AppendLine("<h3>Endpoints</h3>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class=\"mainDiv\">");
            foreach (var tc in from c in Report.Connections where c.StatusCode != 0 && c.ConType == 1 select c)
            {
                sb.AppendLine("<p><a href=\"" + tc.Connection + "\" target=\"_blank\">" + tc.Connection + "</a></p>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</section>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        private static void EchoTotals()
        {
            List<Discrepancy> discrepancies = new List<Discrepancy>();
            foreach (var file in Report.AlalyzedFiles)
            {
                foreach (Discrepancy d in file.Discrepancies)
                {
                    discrepancies.Add(d);
                }
            }

            Console.WriteLine();

            var sorted = from d in discrepancies group d by d.StatusCode into grpd select grpd;
            foreach (var grp in sorted)
            {
                Console.ForegroundColor = Errors.GetColorByStatus(grp.Key);
                Console.WriteLine(grp.Count() + " " + Errors.GetStatusNameByCode(grp.Key));
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }
}
