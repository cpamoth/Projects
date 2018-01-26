using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ConnectionDiagnostics
{
    public class ConnectionInfo
    {
        public string Connection { get; set; }
        public string Result { get; set; }
        public string Exception { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string ChangeTo { get; set; }
        public Brush ConnColor { get; set; }
    }
}
