using ManualDeploymentCheck.BuildAtlasWCF;
using ManualDeploymentCheck.CherwellWCF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDeploymentCheck
{
    class Program
    {
        //Used for threading
        public static int threadCnt;
        public static ManualResetEvent signal;

        private static int me;
        private static string ssh;
        private static string scp;
        private static List<LightServer> serversToTest;
        private static string comp;
        private static LightChangeDeployment currentComp;

        static void Main(string[] args)
        {
            GetPromInfo();
            GetComponents();
            Console.ReadLine();
        }

        private static void GetComponents()
        {
            using (CherwellWCFClient client = new CherwellWCFClient())
            {
                foreach (LightChangeDeployment lcd in client.GetLatestDeployments())
                {
                    Console.WriteLine();
                    string fullPath = lcd.Component;

                    if (!string.IsNullOrEmpty(lcd.Version))
                    {
                        if (!lcd.DestinationPath.Contains(lcd.Component.ToUpper()))
                        {
                            currentComp = lcd;
                            fullPath = Path.Combine(lcd.DestinationPath, lcd.Component);
                        }

                        Console.WriteLine(lcd.Region + ": " + fullPath + " - " + lcd.DeployDate);

                        GetServersToTest(lcd.Region, fullPath);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(lcd.Region + ": " + fullPath + " - " + lcd.DeployDate + " revision is null");
                        Console.ResetColor();
                    }
                }
            }
        }

        private static void GetPromInfo()
        {
            using (BuildAtlasWCFClient client = new BuildAtlasWCFClient())
            {
                try
                {
                    string name = System.Environment.MachineName;
                    me = client.GetPrometheusIdByServerName(name);
                    int id = client.GetServerByName(name).Id;
                    LightTool lt = client.GetToolByName(id, "ssh");
                    if (lt != null)
                    {
                        ssh = lt.Path;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("ssh has not been defined as a tool for " + name + ".");
                        Console.WriteLine();
                    }

                    lt = client.GetToolByName(id, "scp");
                    if (lt != null)
                    {
                        scp = lt.Path;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("scp has not been defined as a tool for " + name + ".");
                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                }
            }
        }

        private static string cygify(string path)
        {
            string begin = path.Substring(0, 1);
            string fix = path.Substring(2);

            return "/cygdrive/" + begin + fix.Replace("\\", "/");
        }

        private static string DefineConnection(string username, string server)
        {
            string connect = GetRSAString() + " " + username + "@" + server;
            return connect;
        }

        private static string GetRSAString()
        {
            return "-i " + cygify(ConfigurationManager.AppSettings["rsaFile"]);
        }

        private static void GetServersToTest(string region, string fullPath)
        {
            comp = "";
            serversToTest = new List<LightServer>();
            Console.WriteLine("Testing:");
            using (BuildAtlasWCFClient client = new BuildAtlasWCFClient())
            {
                foreach (LightServer ls in client.GetServersByDefaultRegion(region))
                {
                    if (ls.PrometheusId != (int?)null && ls.CanConnect == 1)
                    {
                        //Console.WriteLine(ls.Name);
                        serversToTest.Add(ls);
                    }
                }
            }

            if (serversToTest != null && serversToTest.Count() > 0 && !string.IsNullOrEmpty(fullPath))
            {
                comp = fullPath;
                threadCnt = serversToTest.Count();
                signal = new ManualResetEvent(false);
                for (int s = 0; s < threadCnt; s++)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(TestConnection), (object)s);
                }

                signal.WaitOne();
            }
        }

        private static string ExecuteTest(LightServer ls, string connect, string comp)
        {
            string cmd = connect + " ls '" + cygify(comp) + "'";
            string err = "";
            string std = "";

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = ssh;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = cmd;
                proc.Start();

                err = proc.StandardError.ReadToEnd();
                std = proc.StandardOutput.ReadToEnd();

                proc.WaitForExit(3000);
            }

            if (!string.IsNullOrEmpty(err))
            {
                return err;
            }
            else
            {
                return null;
            }
        }

        private static void TestConnection(object c)
        {
            int i = (int)c;
            LightServer ls = serversToTest[i];

            try
            {
                string server = string.IsNullOrEmpty(ls.IP) ? ls.Name : ls.IP;
                string connect = DefineConnection(ls.Username, server);
                string err = ExecuteTest(ls, connect, comp);

                if (!string.IsNullOrEmpty(err))
                {
                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    //Console.WriteLine("not on " + ls.Name);
                    //Console.WriteLine(err);
                    //Console.ResetColor();
                }
                else
                {
                    TestHash(ls, server);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            finally
            {
                if (Interlocked.Decrement(ref threadCnt) == 0)
                {
                    signal.Set();
                }
            }
        }

        private static void TestHash(LightServer ls, string server)
        {
            string transferTo = Path.Combine(Directory.GetCurrentDirectory(), ls.Name);
            if (Directory.Exists(transferTo))
            {
                Directory.Delete(transferTo, true);
            }

            string fromRemote = Path.Combine(transferTo, currentComp.Component);

            Directory.CreateDirectory(transferTo);

            string connect = DefineConnection(ls.Username, server);
            string cmd = connect + ":'" + cygify(comp) + "' '" + cygify(fromRemote) + "'";
            
            string err = ExecuteSCP(cmd);

            if (!string.IsNullOrEmpty(err))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ls.Name + " - " + err);
            }

            if (File.Exists(fromRemote))
            {
                string remoteHash = BTHash.HashMethods.GetFileHash(fromRemote);
                string sourceHash = "";
                File.Delete(fromRemote);

                string srcUrl = currentComp.SVNPath;
                if (!srcUrl.ToUpper().EndsWith(currentComp.Component.ToUpper()))
                {
                    if (!srcUrl.EndsWith("/"))
                    {
                        srcUrl += "/";
                    }

                    srcUrl += currentComp.Component;
                }

                string svnCmd = "export \"" + srcUrl + "\" -r " + currentComp.Version;
                //Console.WriteLine("Running " + svnCmd);

                using (Process proc = new Process())
                {
                    try
                    {
                        proc.StartInfo.FileName = "svn.exe";
                        proc.StartInfo.WorkingDirectory = transferTo;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.Arguments = svnCmd;
                        proc.Start();

                        string svnErr = proc.StandardError.ReadToEnd();
                        string svnStd = proc.StandardOutput.ReadToEnd(); 

                        proc.WaitForExit();

                        if (!string.IsNullOrEmpty(svnErr))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(svnErr);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message);
                    }

                    if (File.Exists(fromRemote))
                    {
                        sourceHash = BTHash.HashMethods.GetFileHash(fromRemote);
                        File.Delete(fromRemote);
                    }
                }

                if (sourceHash == remoteHash)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("match on " + ls.Name);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("mismatch on " + ls.Name);
                }

                Console.ResetColor();
            }
        }

        private static string ExecuteSCP(string cmd)
        {
            //Console.WriteLine(scp + " " + cmd);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = scp;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.Arguments = cmd;
                proc.Start();

                string err = proc.StandardError.ReadToEnd();
                string std = proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                return err;
            }
        }
    }
}
