using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BuildWell
{
    public class PathConverter
    {
        public static string ConvertUrlToPath(string url, string baseUrl, string workspace)
        {
            if(!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            if(!workspace.EndsWith("\\"))
            {
                workspace += "\\";
            }

            string p = url.Replace(baseUrl, workspace);
            p = p.Replace("/", "\\").Replace("%20", " ");

            return p;
        }

        public static string ConvertPathToUrl(string path, string baseUrl, string workspace)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            if (!workspace.EndsWith("\\"))
            {
                workspace += "\\";
            }

            string url = di.FullName.Replace(workspace, baseUrl);
            url = url.Replace("\\", "/");

            return url;
        }
    }
}
