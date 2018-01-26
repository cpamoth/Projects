using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildWell
{
    public class ProjFileOps
    {
        public static List<string> GetReferences(FileInfo projectFile, string initialDir)
        {
            List<string> refsList = new List<string>();
            Console.WriteLine("Getting References for {0}", projectFile.FullName);

            XElement root = XElement.Load(projectFile.FullName);
            XNamespace p = root.Name.Namespace;

            var staticRefs = from refs in root.Descendants(p + "HintPath") select refs;

            foreach (XElement el in staticRefs)
            {
                Regex r = new Regex(@"(?<dir>.*\\[^\\]*)");
                string dir = r.Match(el.Value).Result("${dir}");

                dir = Path.Combine(initialDir, dir);
                DirectoryInfo di = new DirectoryInfo(dir);
                if (!refsList.Contains(di.FullName))
                {
                    refsList.Add(di.FullName);
                }
            }

            var dotNet11Refs = from refs in root.Descendants(p + "Reference").Attributes("HintPath") select refs;

            foreach (XAttribute el in dotNet11Refs)
            {
                Regex r = new Regex(@"(?<dir>.*\\[^\\]*)");
                string dir = r.Match(el.Value).Result("${dir}");

                dir = Path.Combine(initialDir, dir);
                DirectoryInfo di = new DirectoryInfo(dir);
                if (!refsList.Contains(di.FullName))
                {
                    refsList.Add(di.FullName);
                }
            }

            var projectRefs = from refs in root.Descendants(p + "ProjectReference").Attributes("Include") select refs;

            foreach (XAttribute el in projectRefs)
            {
                Regex r = new Regex(@"(?<dir>.*\\[^\\]*)");
                string dir = r.Match(el.Value).Result("${dir}");

                dir = Path.Combine(initialDir, dir);
                DirectoryInfo di = new DirectoryInfo(dir);
                if (!refsList.Contains(di.FullName))
                {
                    refsList.Add(di.FullName);
                }
            }

            return refsList;
        }
    }
}
