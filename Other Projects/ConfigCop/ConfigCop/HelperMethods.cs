using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConfigCop
{
    class HelperMethods
    {
        public static TestedConnection ConnectionTested(string connection)
        {
            if (Program.Report.Connections == null)
            {
                Program.Report.Connections = new List<TestedConnection>();
            }

            var alreadyTested = from t in Program.Report.Connections where t.Connection.ToUpper() == connection.ToUpper() select t;
            if (alreadyTested != null && alreadyTested.Count() > 0)
            {
                return alreadyTested.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public static void AddConnection(string name, int statusCode, string reason, int conType, string port)
        {
            Program.Report.Connections.Add(new TestedConnection { Connection = name, StatusCode = statusCode, Reason = reason, ConType = conType, Port = port });
        }

        public static string CheckConfiguration(string cKey)
        {
            try
            {
                return ConfigurationManager.AppSettings[cKey];
            }
            catch (ConfigurationException ce)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine(ce.Message);
                Console.WriteLine();
                Console.ResetColor();

                Environment.Exit(100);

                return null;
            }
        }

        public static bool ValidateXML(FileInfo fi)
        {
            if (Program.CurrentFile == null)
            {
                Program.CurrentFile = new FileAnalysis();
            }

            try
            {
                XDocument xd1 = new XDocument();
                xd1 = XDocument.Load(fi.FullName);
                Program.CurrentFile.IsValidXML = true;
            }
            catch
            {
                Program.CurrentFile.IsValidXML = false;
                return false;
            }

            return true;
        }

        public static void CollectAppSetting(FileInfo fi, string line)
        {
            if (line.StartsWith("<add key="))
            {
                int keyStart = line.LastIndexOf("<add key=") + "<add key=".Length + 1;
                int valueStart = line.LastIndexOf("value=") + "value=".Length + 1;
                int keyLength = valueStart - keyStart - "value=".Length - 1;
                string key = line.Substring(keyStart, keyLength).Replace("\"", "").Trim().ToUpper();
                string value = line.Substring(valueStart, line.Length - valueStart).Replace("\"", "").Replace("/>", "").Trim().ToUpper();
                Program.AppSettingsCollection.Add(new AppSettingInfo { KeyName = key, KeyValue = value, FilePath = fi.FullName });
            }
        }
    }
}
