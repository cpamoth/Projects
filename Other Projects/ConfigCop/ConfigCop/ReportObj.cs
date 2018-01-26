using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigCop
{
    public class ReportObj
    {
        public DateTime ReportDate { get; set; }
        public List<FileAnalysis> AlalyzedFiles {get;set;}
        public List<AppSettingInfo> DuplicateKeys { get; set; }
        public List<string> UnauthorizedDirectories { get; set; }
        public List<TestedConnection> Connections { get; set; }
    }

    public class FileAnalysis
    {
        public string AlalyzedFile { get; set; }
        public string FullPath { get; set; }
        public bool IsValidXML { get; set; }
        public DateTime LastModified { get; set; }
        public List<Discrepancy> Discrepancies { get; set; }
    }

    public class Discrepancy
    {
        public string DiscrepancyType { get; set; }
        public int LineNumber { get; set; }
        public string DiscrepancyDetails { get; set; }
        public int StatusCode { get; set; } //Configured
    }

    public class TestedConnection
    {
        public string Connection { get; set; }
        public int StatusCode { get; set; } //Configured
        public string Reason { get; set; }
        public int ConType { get; set; } //1 Endpoint, 2 Server
        public string Port { get; set; }
    }
}
