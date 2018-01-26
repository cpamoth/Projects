using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildWell
{
    class Program
    {
        //private static TextWriter tw;
        //private static StringBuilder log;
        //private static Build currentBuild;

        private static int? buildId;

        //private static BuildInfo.BuildInfo bi;

        private static string svn;
        private static string baseUrl;
        private static string workspace;
        private static string output;

        private static ConsoleColor oColor;
        private static ConsoleColor infoColor;
        private static ConsoleColor lowLevelInfoColor;
        private static ConsoleColor successColor;

        private static Dictionary<string, string> gatheredRefs;
        private static Dictionary<string, string> referenceList;

        static void Main(string[] args)
        {
            buildId = (int?)null;

            //log = new StringBuilder();
            //tw = new StringWriter(log);

            lowLevelInfoColor = ConsoleColor.DarkGray;
            infoColor = ConsoleColor.Cyan;
            successColor = ConsoleColor.Green;
            oColor = Console.ForegroundColor;

            //Testing
            //http://sourcecontrol/svn/Gis/Development/Interfaces/Watson/Project%20Code/watson.build
            //http://sourcecontrol/svn/Gis/Development/Interfaces/Gis.eLink.Wcf%20v3.0/Project%20Code/Gis.eLink.Wcf.build
            //args = "-svn,http://sourcecontrol/svn/Gis/Development/Interfaces/Gis.eLink.Wcf%20v3.0/Project%20Code/SemiAuto.build,14647".Split(',');
            //Testing

            DoIt(args);
        }

        private static void DoIt(string[] args)
        {
            InitializeBuild(args[2]);

            string rev = "";

            switch (args[0].ToLower())
            {
                case "-svn":
                    InitializeWorkspace(args[1]);
                    //GetBuildTools();
                    FileInfo fi = new FileInfo(PathConverter.ConvertUrlToPath(args[1], baseUrl, workspace));
                    Environment.CurrentDirectory = fi.DirectoryName;
                    BuildWorkspace();
                    Compile(fi.Name);
                    rev = Commit(args[2]);
                    break;
                case "-b":

                    break;
                default:
                    break;
            }

            Success(rev);
        }

        private static void InitializeBuild(string cr)
        {
            Console.WriteLine("Creating the build object...");
            //currentBuild = new Build();
            //currentBuild.BuildDate = DateTime.Now;
            //currentBuild.ChangeControl = cr;
            //currentBuild.Status = "In Progress";
            //currentBuild.BuildLog = "";

            using (BuildWellWCF.BuildWellWCFClient client = new BuildWellWCF.BuildWellWCFClient())
            {
                buildId = client.SaveBuild(null, null, cr, "In Progress");
            }

            //bi = new BuildInfo.BuildInfo();
            //bi.Brand = "SemiAuto Build";
            //bi.Version = currentBuild.ChangeControl;

            try
            {
                //BT_SemiAutoEntities de = new BT_SemiAutoEntities();
                //de = new BT_SemiAutoEntities();
                //using (de = new BT_SemiAutoEntities())
                //{
                //    de.Builds.Add(currentBuild);
                //    de.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Aaaaaaaaaahhhhhhhhh!");
                Console.Write(ex.Message);
                //Console.WriteLine("Trying again...");
                //InitializeBuild(cr);
            }
        }

        private static void InitializeWorkspace(string script)
        {
            Console.ForegroundColor = infoColor;
            Echo("Initializing the build...");
            Echo();
            Console.ForegroundColor = oColor;

            ValidateScript(script);
            CheckoutWorkspace();
            UpdateToReference(script);

            Console.ForegroundColor = successColor;
            Echo();
            Echo("Build initialization complete");
            Echo();
            Console.ForegroundColor = oColor;
        }

        private static void BuildWorkspace()
        {
            gatheredRefs = new Dictionary<string, string>();
            LoadReferencesFromFile();
            GatherSourceCode();
            UpdateRefsToSpecifiedVersion();
            AddReferencesToDatabase();
        }

        private static void Compile(string script)
        {
            try
            {
                string nant = ConfigurationManager.AppSettings["nant"];

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = nant;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    // /clp:ErrorsOnly  /nologo
                    proc.StartInfo.Arguments = "-buildfile:\"" + script + "\"";
                    proc.Start();
                    //proc.WaitForExit();

                    using (StreamReader sr = proc.StandardOutput)
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            Echo(line, true);
                            line = sr.ReadLine();
                        }
                    }

                    if (proc.ExitCode != 0)
                    {
                        Fail("Build failed");
                    }
                }


                //string msbuild = ConfigurationManager.AppSettings["msbuild"];

                //using (Process proc = new Process())
                //{
                //    proc.StartInfo.FileName = msbuild;
                //    proc.StartInfo.CreateNoWindow = true;
                //    proc.StartInfo.UseShellExecute = false;
                //    proc.StartInfo.RedirectStandardOutput = true;
                //    // /clp:ErrorsOnly  /nologo
                //    proc.StartInfo.Arguments = "\"" + script + "\" /v:diag";
                //    proc.Start();
                //    //proc.WaitForExit();

                //    using (StreamReader sr = proc.StandardOutput)
                //    {
                //        string line = sr.ReadLine();
                //        while (line != null)
                //        {
                //            Echo(line, true);
                //            line = sr.ReadLine();
                //        }
                //    }

                //    if (proc.ExitCode != 0)
                //    {
                //        Fail("Build failed");
                //    }
                //}

                Echo();
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private static void AddReferencesToDatabase()
        {
            //bool inDb = false;
            //try
            //{
            //    using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            //    {
            //        de.Builds.Add(currentBuild);
            //        de.SaveChanges();
            //        inDb = true;
            //    }
            //}
            //catch
            //{
            //    inDb = false;
            //}

            //if (inDb == true)
            //{
                Console.ForegroundColor = infoColor;
                Echo("Adding references for this build to the database...");
                Echo();
            //}

            foreach (string r in gatheredRefs.Keys)
            {
                string rev = GetRevision(r);
                string url = PathConverter.ConvertPathToUrl(r, baseUrl, workspace);
                //BuildSource bs = new BuildSource();
                //bs.BuildId = currentBuild.Id;
                //bs.RepositoryType = "svn";
                //bs.Revision = rev;
                //bs.Url = url;
                //bs.ReferencedBy = gatheredRefs[r];

                using (BuildWellWCF.BuildWellWCFClient client = new BuildWellWCF.BuildWellWCFClient())
                {
                    client.AddBuildReference((int)buildId, gatheredRefs[r], "svn", rev, url);
                }

                //bi.SourceCodeRevision += bs.Revision + " " + bs.Url + " Reference By: " + bs.ReferencedBy + "\n\r";

                //if (inDb == true)
                //{
                //    using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
                //    {
                //        de.BuildSources.Add(bs);
                //        de.SaveChanges();
                //    }
                //}
            }

            Echo();
            Console.ForegroundColor = oColor;
        }

        private static void GetBuildTools()
        {
            Console.ForegroundColor = infoColor;
            Echo("Getting required build tools...");

            string bt = ConfigurationManager.AppSettings["buildTools"];

            UpdateToReference(bt);
        }

        private static void LoadReferencesFromFile()
        {
            referenceList = new Dictionary<string, string>();
            string refFile = ConfigurationManager.AppSettings["refFile"];
            Console.ForegroundColor = infoColor;
            Echo();
            Echo("Loading references defined in " + refFile + "...");
            if (File.Exists(refFile))
            {
                using (StreamReader sr = new StreamReader(refFile))
                {
                    string r = sr.ReadLine();
                    while (r != null)
                    {
                        if (!r.StartsWith("//"))
                        {
                            string[] parts = r.Split('|');
                            string rPath = parts[0].Trim();
                            DirectoryInfo di = new DirectoryInfo(rPath);
                            rPath = di.FullName;
                            string rev = "";
                            if (parts.Count() > 1)
                            {
                                rev = parts[1].Trim();
                            }

                            Console.ForegroundColor = lowLevelInfoColor;
                            string output = rPath;
                            if (!string.IsNullOrEmpty(rev))
                            {
                                output += " - " + rev;
                            }
                            Echo("     " + output);
                            Console.ForegroundColor = oColor;
                            referenceList.Add(rPath, rev);
                        }

                        r = sr.ReadLine();
                    }
                }
            }
            else
            {
                Console.ForegroundColor = lowLevelInfoColor;
                Echo("     no reference file was found");
                Console.ForegroundColor = oColor;
            }

            Echo();
        }

        private static string Commit(string ccNumber)
        {
            string buildsFolder = SetupBuildsFolder(ccNumber);
            CheckoutBuildsFolder(buildsFolder);
            CopyBinaries();
            StageArtifacts();
            CommitIt();
            return GetRevision(buildsFolder);
        }

        private static void CopyBinaries()
        {
            Console.ForegroundColor = infoColor;
            Echo("Copying artifacts to output directory...");
            Console.ForegroundColor = oColor;
            Console.ForegroundColor = lowLevelInfoColor;
            DirectoryInfo di = new DirectoryInfo("..\\Install");

            foreach (string f in Directory.GetFiles(di.FullName, "*", SearchOption.AllDirectories))
            {
                string segment = f.Replace(di.FullName, "");
                if (segment.StartsWith("\\"))
                {
                    segment = segment.Remove(0, 1);
                }
                string newPath = Path.Combine(output, segment);
                FileInfo fi = new FileInfo(newPath);
                if (!Directory.Exists(fi.DirectoryName))
                {
                    Echo("Creating directory - " + fi.DirectoryName + "...");
                    Directory.CreateDirectory(fi.DirectoryName);
                }

                Echo("     " + f);
                File.Copy(f, fi.FullName, true);
            }

            Console.ForegroundColor = oColor;
            Echo();
        }

        private static void StageArtifacts()
        {
            //Console.WriteLine("Serializing build info to " + output + "...");
            //BuildInfo.XmlOps.Serialization.SerializeBuildInfo(bi, output);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = svn;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = "status \"" + output + "\" --no-ignore";
                proc.Start();
                proc.WaitForExit();

                using (StreamReader sr = proc.StandardOutput)
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (line.StartsWith("?"))
                        {
                            string x = line.Replace("?", "").Trim();
                            SVNOps("add \"" + x);
                        }
                        else if (line.StartsWith("!"))
                        {
                            string x = line.Replace("!", "").Trim();
                            SVNOps("delete \"" + x);
                        }

                        line = sr.ReadLine();
                    }
                }
            }
        }

        private static void SVNOps(string arg)
        {
            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.Arguments = arg;
                    proc.Start();
                    proc.WaitForExit();

                    using (StreamReader sr = proc.StandardOutput)
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            Echo(line);
                            line = sr.ReadLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private static void CommitIt()
        {
            string svnLockDir = ConfigurationManager.AppSettings["SVNLockDir"];
            if (!Directory.Exists(svnLockDir)) Directory.CreateDirectory(svnLockDir);
            string lockFile = Path.Combine(svnLockDir, "svncommitting.txt");

            if (File.Exists(lockFile))
            {
                Console.WriteLine("Waiting for svn to become available...");
            }

            Process proc = new Process();

            try
            {
                proc.StartInfo.WorkingDirectory = output;
                proc.StartInfo.FileName = svn;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = "commit -m \"Committed by BuildWell";
                proc.Start();
                proc.WaitForExit();

                using (StreamReader sr = proc.StandardOutput)
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        Echo(line, true);
                        line = sr.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
            finally
            {
                proc.Dispose();
                File.Delete(lockFile);
            }
        }

        private static string SetupBuildsFolder(string ccNumber)
        {
            string buildsFolder = ConfigurationManager.AppSettings["buildsRepo"];
            Console.ForegroundColor = infoColor;
            Echo();
            Echo("Seeing if an output folder for this release already exists...");
            Console.ForegroundColor = oColor;

            if (!buildsFolder.EndsWith("/"))
            {
                buildsFolder += "/";
            }

            buildsFolder += ccNumber;
            bool exists = ExistsInRepo(buildsFolder);

            if (exists == false)
            {
                Console.ForegroundColor = lowLevelInfoColor;
                Echo("Release folder not found");
                Echo("Creating " + buildsFolder + "...");
                Console.ForegroundColor = oColor;
                Echo();

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.Arguments = string.Format("mkdir \"" + buildsFolder + "\" -m \"Initial setup for " + ccNumber + "\"");
                    proc.Start();
                    proc.WaitForExit();
                }
            }
            else
            {
                Console.ForegroundColor = lowLevelInfoColor;
                Echo("Release folder exists");
                Console.ForegroundColor = oColor;
                Echo();
            }

            return buildsFolder;
        }

        private static bool ExistsInRepo(string i)
        {
            bool exists = true;
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = svn;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = string.Format("info \"" + i);
                proc.Start();
                proc.WaitForExit();

                using (StreamReader sr = proc.StandardOutput)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                    {
                        exists = false;
                    }
                    else
                    {
                        while (line != null)
                        {
                            if (line.StartsWith("svn:"))
                            {
                                //Folder doesn't exist
                                exists = false;
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return exists;
        }

        private static string GetRevision(string i)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = svn;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = string.Format("info \"" + i);
                proc.Start();
                proc.WaitForExit();

                using (StreamReader sr = proc.StandardOutput)
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (line.StartsWith("Revision:"))
                        {
                            return line.Replace("Revision:", "").Trim();
                        }

                        line = sr.ReadLine();
                    }
                }
            }

            return "?";
        }

        private static void GatherSourceCode()
        {
            Console.ForegroundColor = infoColor;
            Echo("Gathering all source code...");
            Echo();
            Console.ForegroundColor = oColor;

            string projFile = "";

            foreach (string f in Directory.GetFiles(".", "*.*proj", SearchOption.AllDirectories))
            {
                projFile = f;
                break;
            }

            //Testing
            //projFile = "";
            //Testing

            if (string.IsNullOrEmpty(projFile))
            {
                Console.ForegroundColor = lowLevelInfoColor;
                Echo("No project file found.  Using reference list provided.");
                Console.ForegroundColor = oColor;
                Echo();

                foreach (string p in referenceList.Keys)
                {
                    UpdateSource("Reference File", p);
                }
            }
            else
            {
                Console.ForegroundColor = lowLevelInfoColor;
                Echo("Project file found - " + projFile);
                Console.ForegroundColor = oColor;
                Echo();

                if (File.Exists(projFile))
                {
                    gatheredRefs.Add(new FileInfo(projFile).FullName, "");

                    FileInfo fi = new FileInfo(projFile);
                    List<string> refList = ProjFileOps.GetReferences(fi, fi.DirectoryName);
                    foreach (string f in refList)
                    {
                        UpdateSource(fi.FullName, f);
                    }
                }
            }

            Console.ForegroundColor = successColor;
            Echo();
            Echo("All source code has been gathered");
            Echo();
            Console.ForegroundColor = oColor;
        }

        private static void UpdateSource(string parent, string source)
        {
            string url = PathConverter.ConvertPathToUrl(source, baseUrl, workspace);
            string nodeType = GetNodeType(url);
            if (nodeType == "file")
            {
                UpdateToReference(url);
                string r = PathConverter.ConvertUrlToPath(url, baseUrl, workspace);
                AddToGatheredRefsList(parent, r);
                FileInfo fi = new FileInfo(r);
                if (fi.Extension.EndsWith("proj"))
                {
                    List<string> refList = ProjFileOps.GetReferences(fi, fi.DirectoryName);
                    foreach (string f in refList)
                    {
                        UpdateSource(fi.FullName, f);
                    }
                }
            }
            else
            {
                string r = PathConverter.ConvertUrlToPath(url, baseUrl, workspace);
                gatheredRefs.Add(r, parent);
                UpdateToReference(url);
            }
        }

        private static void AddToGatheredRefsList(string parent, string r)
        {
            if (!gatheredRefs.Keys.Contains(r))
            {
                gatheredRefs.Add(r, parent);
            }
            else
            {
                gatheredRefs[r] += "," + parent;
            }
        }

        private static void UpdateRefsToSpecifiedVersion()
        {
            if (referenceList != null && referenceList.Count() > 0)
            {
                Console.ForegroundColor = infoColor;
                Echo("Updating specified references to specific revisions...");
                foreach (string r in referenceList.Keys)
                {
                    if (!string.IsNullOrEmpty(referenceList[r]))
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Echo(r + " -r " + referenceList[r]);
                        Console.ForegroundColor = oColor;
                        RevUpdate(r, referenceList[r]);
                    }
                }

                Console.ForegroundColor = successColor;
                Echo();
                Echo("References are at specified revisions");
                Echo();
                Console.ForegroundColor = oColor;
            }
        }

        private static string GetNodeType(string url)
        {
            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.Arguments = string.Format("info \"" + url);
                    proc.Start();
                    proc.WaitForExit();

                    using (StreamReader sr = proc.StandardOutput)
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            if (line.StartsWith("svn: warning:"))
                            {
                                Fail(sr.ReadToEnd());
                            }
                            else if (line.StartsWith("Node Kind:"))
                            {
                                return line.Replace("Node Kind:", "").Trim().ToLower();
                            }

                            line = sr.ReadLine();
                        }
                    }
                }

                Echo("Failed to gather svn info for " + url + ".  Please make sure the url is valid.  Note that svn urls are case sensitive.");
                return "";
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
                return "";
            }
        }

        private static void ValidateScript(string script)
        {
            svn = ConfigurationManager.AppSettings["svn"];

            Console.ForegroundColor = infoColor;
            Echo("Validating script path " + script + "...");

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = svn;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.Arguments = "info \"" + script;
                try
                {
                    proc.Start();

                    using (StreamReader sr = proc.StandardError)
                    {
                        string output = sr.ReadToEnd();
                        if (!string.IsNullOrEmpty(output))
                        {
                            Fail(output);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Fail(ex.Message);
                }
            }

            Console.ForegroundColor = lowLevelInfoColor;
            Echo("     script is valid");
            Echo();
            Console.ForegroundColor = oColor;
        }

        private static void CheckoutBuildsFolder(string buildsFolder)
        {
            try
            {
                output = ConfigurationManager.AppSettings["output"];

                Console.ForegroundColor = infoColor;
                Echo("Setting up builds folder...");

                Console.ForegroundColor = lowLevelInfoColor;
                if (Directory.Exists(output))
                {
                    Echo("     Deleting " + output + " for a clean setup");
                    Directory.Delete(output, true);
                }

                Echo("     Creating " + output + " as a clean output location");
                Directory.CreateDirectory(output);

                Console.ForegroundColor = oColor;
                Echo();

                Console.ForegroundColor = infoColor;
                Echo();
                Echo("Checking out " + buildsFolder + "...");
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.Arguments = "checkout \"" + buildsFolder + "\" \"" + output + "\" --depth=infinity";
                    proc.Start();
                    proc.WaitForExit();

                    string error = "";
                    using (StreamReader sr = proc.StandardError)
                    {
                        error = sr.ReadToEnd();
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        using (StreamReader sr = proc.StandardOutput)
                        {
                            Console.ForegroundColor = lowLevelInfoColor;
                            string line = sr.ReadLine();
                            while (line != null)
                            {
                                EchoOrLogSVNOutput(line);
                                line = sr.ReadLine();
                            }
                        }
                    }
                    else
                    {
                        Fail(error);
                    }
                }

                Echo();
                Console.ForegroundColor = oColor;
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private static void CheckoutWorkspace()
        {
            try
            {
                baseUrl = ConfigurationManager.AppSettings["sourceRoot"];
                workspace = ConfigurationManager.AppSettings["workspace"];

                SetupWorkspace(workspace);

                Console.ForegroundColor = infoColor;
                Echo("Checking out " + baseUrl + " to workspace...");
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.Arguments = "checkout \"" + baseUrl + "\" \"" + workspace + "\" --depth=immediates";
                    proc.Start();
                    proc.WaitForExit();

                    string error = "";
                    using (StreamReader sr = proc.StandardError)
                    {
                        error = sr.ReadToEnd();
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        using (StreamReader sr = proc.StandardOutput)
                        {
                            Console.ForegroundColor = lowLevelInfoColor;
                            string line = sr.ReadLine();
                            while (line != null)
                            {
                                EchoOrLogSVNOutput(line);
                                line = sr.ReadLine();
                            }
                        }
                    }
                    else
                    {
                        Fail(error);
                    }
                }

                Echo();
                Console.ForegroundColor = oColor;
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private static void SetupWorkspace(string workspace)
        {
            Console.ForegroundColor = infoColor;
            Echo("Setting up workspace...");

            Console.ForegroundColor = lowLevelInfoColor;
            if (Directory.Exists(workspace))
            {
                Echo("     Deleting " + workspace + " for a clean set up");
                Directory.Delete(workspace, true);
            }

            Echo("     Creating " + workspace + " as a clean workspace");
            Directory.CreateDirectory(workspace);

            Console.ForegroundColor = oColor;
            Echo();
        }

        private static void UpdateToReference(string path, string rev = "")
        {
            Console.ForegroundColor = infoColor;
            Echo("Gathering source at " + path + "...");

            string nodeType = GetNodeType(path);

            if (path.Contains("http://"))
            {
                path = PathConverter.ConvertUrlToPath(path, baseUrl, workspace);
            }

            string dir = "";
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                if (!string.IsNullOrEmpty(nodeType))
                {
                    if (nodeType == "file")
                    {
                        FileInfo fi = new FileInfo(path);
                        dir = fi.DirectoryName;
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(path);
                        dir = di.FullName;
                    }

                    while (!Directory.Exists(dir))
                    {
                        RecursiveUpdate(dir);
                    }

                    if (Directory.Exists(dir))
                    {
                        FullUpdate(dir);
                    }
                }
            }
        }

        private static void RecursiveUpdate(string dir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                if (!Directory.Exists(dir) && Directory.Exists(di.Parent.FullName))
                {
                    using (Process proc = new Process())
                    {
                        proc.StartInfo.FileName = svn;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.Arguments = "update \"" + dir + "\" --set-depth=immediates";
                        proc.Start();

                        using (StreamReader sr = proc.StandardOutput)
                        {
                            Console.ForegroundColor = lowLevelInfoColor;
                            string line = sr.ReadLine();
                            while (line != null)
                            {
                                EchoOrLogSVNOutput(line);
                                line = sr.ReadLine();
                            }
                        }
                    }
                }
                else
                {
                    RecursiveUpdate(di.Parent.FullName);
                }
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private static void EchoOrLogSVNOutput(string line)
        {
            if (line.Trim().ToUpper().StartsWith("CHECKING OUT") || line.Trim().ToUpper().StartsWith("UPDATING") || line.Trim().ToUpper().StartsWith("CHECKED OUT REVISION") || line.Trim().ToUpper().StartsWith("UPDATED TO REVISION"))
            {
                Echo(line);
            }
            else
            {
                Console.WriteLine(line);
            }
        }

        private static void RevUpdate(string path, string rev)
        {
            try
            {
                string arg = "update \"" + path + "\" -r " + rev;

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.Arguments = arg;
                    proc.Start();

                    using (StreamReader sr = proc.StandardOutput)
                    {
                        Console.ForegroundColor = lowLevelInfoColor;
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            EchoOrLogSVNOutput(line);
                            line = sr.ReadLine();
                        }
                    }
                }

                Console.ForegroundColor = oColor;
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private static void FullUpdate(string path)
        {
            try
            {
                string arg = "update --set-depth=infinity";

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = svn;
                    proc.StartInfo.WorkingDirectory = path;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.Arguments = arg;
                    proc.Start();

                    using (StreamReader sr = proc.StandardOutput)
                    {
                        Console.ForegroundColor = lowLevelInfoColor;
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            EchoOrLogSVNOutput(line);
                            line = sr.ReadLine();
                        }
                    }
                }

                Console.ForegroundColor = oColor;
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        #region Finalize

        private static void Success(string rev)
        {
            try
            {
                //using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
                //{
                //    Build cb = de.Builds.Single(b => b.Id == currentBuild.Id);
                //    cb.Status = "Success";
                //    cb.BinaryRevision = rev;
                //    de.SaveChanges();
                //}

                Echo("Marking build as Success.  Rev - " + rev, false);

                using (BuildWellWCF.BuildWellWCFClient client = new BuildWellWCF.BuildWellWCFClient())
                {
                    client.UpdateBuildStatus((int)buildId, "Success");
                    client.UpdateBuildBinaryRevision((int)buildId, rev);
                }
            }
            catch
            {

            }
        }

        public static void Fail(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Echo();
            Echo(true, error, true);
            Echo();
            Console.ForegroundColor = oColor;

            if (buildId != (int?)null)
            {
                try
                {
                    //using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
                    //{
                    //    Build cb = de.Builds.Single(b => b.Id == currentBuild.Id);
                    //    cb.Status = "Failed";
                    //    de.SaveChanges();
                    //}

                    Echo("Marking build as Failed.", false);

                    using (BuildWellWCF.BuildWellWCFClient client = new BuildWellWCF.BuildWellWCFClient())
                    {
                        client.UpdateBuildStatus((int)buildId, "Failed");
                    }
                }
                catch
                {

                }
            }

            Environment.Exit(9);
        }

        #endregion

        #region Echo

        private static void Echo(bool asIs, string msg = "", bool saveToLog = false)
        {
            //Echo to console
            ConsoleWrite(msg, asIs);

            if (saveToLog == true)
            {
                //save to log
                if (buildId != (int?)null)
                {
                    //using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
                    //{
                    //    Build cb = de.Builds.Single(b => b.Id == currentBuild.Id);
                    //    de.AppendToBuildLog(buildId, "\n" + msg);
                    //    de.SaveChanges();
                    //}

                    using (BuildWellWCF.BuildWellWCFClient client = new BuildWellWCF.BuildWellWCFClient())
                    {
                        client.AppendToBuildLog((int)buildId, "\n" + msg);
                    }
                }
            }
        }

        private static void Echo(string msg = "", bool saveToLog = false)
        {
            Echo(false, msg, saveToLog);
        }

        private static void ConsoleWrite(string msg, bool asIs)
        {
            if (asIs == true)
            {
                Console.Write(msg);
            }
            else
            {
                Console.WriteLine(msg);
            }
        }

        #endregion
    }
}
