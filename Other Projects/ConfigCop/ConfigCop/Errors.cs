using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    class Errors
    {
        //(ConsoleColor) Enum.Parse(typeof(ConsoleColor), str);
        public static void ReportDiscrepancy(int lineNumber, string details, string discrepancyType, int statusCode)
        {
            if (Program.CurrentFile.Discrepancies == null)
            {
                Program.CurrentFile.Discrepancies = new List<Discrepancy>();
            }

            Program.CurrentFile.Discrepancies.Add(new Discrepancy
            {
                DiscrepancyDetails = details,
                DiscrepancyType = discrepancyType,
                LineNumber = lineNumber,
                StatusCode = statusCode
            });
        }

        public static int GetStatusCode(string cKey)
        {
            string statusCode = HelperMethods.CheckConfiguration(cKey);
            int sc;
            int.TryParse(statusCode, out sc);
            return sc;
        }

        public static ConsoleColor GetColorByStatus(int status)
        {
            string statusCode = HelperMethods.CheckConfiguration(status.ToString());
            string[] parts = statusCode.Split('|');
            try
            {
                return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), parts[1]);
            }
            catch
            {
                return ConsoleColor.White;
            }
        }

        public static string GetStatusNameByCode(int status)
        {
            string statusCode = HelperMethods.CheckConfiguration(status.ToString());
            string[] parts = statusCode.Split('|');
            return parts[0];
        }

        public static void ConnectivityError(int lineNumber, string endpoint, string reason, int statusCode, int conType)
        {
            if (statusCode != 0)
            {
                Console.ForegroundColor = GetColorByStatus(statusCode);
                Console.WriteLine(reason + " (line " + lineNumber.ToString() + ")");
                Console.ResetColor();
                ReportDiscrepancy(lineNumber, endpoint, reason, statusCode);
                Program.LineError = true;
            }
        }

        public static void ServerDiscrepancy(int lineNumber, string server, string line)
        {
            int sc = GetStatusCode("ServerDiscrepancy");
            if (sc != 0)
            {
                Console.ForegroundColor = GetColorByStatus(sc);
                Console.WriteLine("Possible database server discrepancy found: " + server + " (line " + lineNumber.ToString() + ")");
                Console.ResetColor();
                ReportDiscrepancy(lineNumber, "Line contains " + server + ".  " + line, "Database Server Discrepancy", sc);
                Program.LineError = true;
            }
        }

        public static void RegionDiscrepancy(int lineNumber, string region, string line)
        {
            int sc = GetStatusCode("RegionDiscrepancy");
            if (sc != 0)
            {
                Console.ForegroundColor = GetColorByStatus(sc);
                Console.WriteLine("Possible region discrepancy found: " + region + " (line " + lineNumber.ToString() + ")");
                Console.ResetColor();
                ReportDiscrepancy(lineNumber, "Line contains " + region + ".  " + line, "Region Discrepancy", sc);
                Program.LineError = true;
            }
        }

        public static void MissingContent()
        {
            int sc = GetStatusCode("MissingContent");
            if (sc != 0)
            {
                ReportDiscrepancy(0, "File does not contain specified content!", "Missing Content", sc);
            }
        }

        public static void DuplicateKeyWarning(IGrouping<string, AppSettingInfo> keyGroup)
        {
            if (Program.Report.DuplicateKeys == null)
            {
                Program.Report.DuplicateKeys = new List<AppSettingInfo>();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Duplicate key found: " + keyGroup.Key + ".  See report for values.");
            foreach (var f in keyGroup)
            {
                Program.Report.DuplicateKeys.Add(f);
                FileInfo fi = new FileInfo(f.FilePath);
                if (fi.Name.ToUpper() == "MACHINE.CONFIG")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.WriteLine("     " + f.FilePath);
            }
            Console.ResetColor();
        }
    }
}
