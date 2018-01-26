using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDiffReport
{
    public class CompareScenario
    {
        public string TemplateName { get; set; }
        public string ReportPath { get; set; }
        public string TargetDB { get; set; }
        public string TargetSvr { get; set; }
        public string SourceDB { get; set; }
        public string SourceSvr { get; set; }
    }
}
