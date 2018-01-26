using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace InstallMSI
{
    class Program
    {
        public static ConsoleColor OriginalForeground;
        public static ConsoleColor Good;
        public static ConsoleColor Bad;
        public static ConsoleColor Warning;
        public static ConsoleColor Info;

        static void Main(string[] args)
        {
            try
            {
                OriginalForeground = Console.ForegroundColor;
                Good = ConsoleColor.Green;
                Bad = ConsoleColor.Red;
                Warning = ConsoleColor.Yellow;
                Info = ConsoleColor.DarkCyan;

                // Arg validation
                string msi;
                string env;
                string machineName = System.Environment.MachineName;
                validateArgs(args, machineName, out msi, out env);

                // IO validation and setup
                string timestamp = DateTime.Now.ToString("dd_MMM_yyyy__hh_mm_ss");
                string installRoot;
                string prodName;
                validateInstallerDetails(msi, out installRoot, out prodName);
                string installPath = Path.Combine(installRoot, prodName);
                string backupRoot = Path.Combine(installRoot, "BACKUPS");
                string backupPath = Path.Combine(backupRoot, prodName + "_" + timestamp);
                string logDir = Path.Combine(backupRoot, "LOGS");
                string deployLog = Path.Combine(logDir, "DeploymentLog_" + prodName + ".log");
                validateDirectory(backupRoot, false, true);
                validateDirectory(logDir, false, true);

                // Backup and Install
                backUpAndCleanExisting(installPath, prodName, backupRoot, backupPath);
                installMsi(msi, installPath, machineName, env, deployLog);
                validateInstallation(installPath, msi, deployLog);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = Bad;
                Console.Write(ex.ToString());
            }
            finally
            {
                Console.ForegroundColor = OriginalForeground;
            }
        }

        #region Validation

        private static void displayInstructions()
        {
            Console.ForegroundColor = Info;
            Console.Write(Templates.Instructions);
            Console.ForegroundColor = OriginalForeground;

            Environment.Exit(0);
        }

        private static void validateArgs(string[] args, string machineName, out string msi, out string env)
        {
            msi = "";
            env = "";

            if (args.Count() == 0)
            {
                displayInstructions();
            }

            if (args.Count() > 0)
            {
                if (args[0].Contains("?"))
                {
                    displayInstructions();
                }
                else
                {
                    msi = args[0];
                    validateMSI(msi);
                }
            }

            env = determineEnvironment(args, machineName);
            validateEnvironment(env);
        }

        private static void validateMSI(string msi)
        {
            Console.WriteLine();
            Console.WriteLine("Validating msi path...");

            Console.WriteLine();
            if (File.Exists(msi))
            {
                Console.ForegroundColor = Good;
                Console.WriteLine("Found msi file : \"" + msi + "\"");
                Console.ForegroundColor = OriginalForeground;

                displayMsiInfo(msi);
            }
            else
            {
                Console.ForegroundColor = Bad;
                Console.WriteLine("ERROR : Could not find msi file \"" + msi + "\" . Exiting...");
                Console.WriteLine();
                Console.ForegroundColor = Info;
                Console.WriteLine("The following files are valid msi files found in this directory:");
                foreach (string f in Directory.GetFiles(".", "*.msi", SearchOption.TopDirectoryOnly))
                {
                    FileInfo fi = new FileInfo(f);
                    Console.WriteLine("     - " + fi.Name + " (" + fi.CreationTime + ")");
                }

                Console.ForegroundColor = OriginalForeground;

                Environment.Exit(0);
            }

            Console.WriteLine();
        }

        private static void validateEnvironment(string env)
        {
            Console.WriteLine();
            Console.WriteLine("Validating the server environment...");

            bool validEnv = false;

            switch (env)
            {
                case "DEV":
                    validEnv = true;
                    break;
                case "DEV2":
                    validEnv = true;
                    break;
                case "QA":
                    validEnv = true;
                    break;
                case "QA1":
                    validEnv = true;
                    break;
                case "QA2":
                    validEnv = true;
                    break;
                case "MODEL":
                    validEnv = true;
                    break;
                case "MODEL1":
                    validEnv = true;
                    break;
                case "MODEL2":
                    validEnv = true;
                    break;
                case "PROD":
                    validEnv = true;
                    break;
                default:
                    validEnv = false;
                    break;
            }

            Console.WriteLine();
            if (validEnv)
            {
                Console.ForegroundColor = Good;
                Console.WriteLine("The environment for this server is: " + env);
                Console.ForegroundColor = OriginalForeground;
            }
            else
            {
                Console.ForegroundColor = Bad;
                Console.WriteLine("ERROR : Can not determine Evn values from ComputerName.. Please check and add to the logic.. Exiting..");
                Console.ForegroundColor = OriginalForeground;
                Environment.Exit(0);
            }
            Console.WriteLine();
        }

        private static void validateInstallerDetails(string msi, out string installRoot, out string prodName)
        {
            Type type = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            WindowsInstaller.Installer installer = (WindowsInstaller.Installer)Activator.CreateInstance(type);
            WindowsInstaller.Database db = installer.OpenDatabase(msi, 0);

            WindowsInstaller.View dv_pp = db.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='MYAPPPATH'");
            WindowsInstaller.View dv_pn = db.OpenView("SELECT `Value` FROM `Property` WHERE `Property`='ProductName'");

            WindowsInstaller.Record record_pp = null;
            WindowsInstaller.Record record_pn = null;
            dv_pp.Execute(record_pp);
            record_pp = dv_pp.Fetch(); //product path  Eg : E:\Web
            dv_pn.Execute(record_pn);
            record_pn = dv_pn.Fetch(); //product name

            installRoot = record_pp.get_StringData(1).ToString();
            prodName = record_pn.get_StringData(1).ToString();

            Console.WriteLine();
            if (string.IsNullOrEmpty(installRoot))
            {
                Console.ForegroundColor = Bad;
                Console.WriteLine("ERROR : The property 'MYAPPPATH' has not been set.  The install path could not be resolved.");
                Console.ForegroundColor = OriginalForeground;
                Environment.Exit(0);
            }

            if (string.IsNullOrEmpty(prodName))
            {
                Console.ForegroundColor = Bad;
                Console.WriteLine("ERROR : The property 'ProductName' has not been set.  The install path could not be resolved.");
                Console.ForegroundColor = OriginalForeground;
                Environment.Exit(0);
            }

            Console.ForegroundColor = Info;
            System.Console.WriteLine("MSI Install path: " + Path.Combine(installRoot, prodName));
            Console.WriteLine();
            Console.ForegroundColor = OriginalForeground;

            validateDirectory(installRoot, true, false);
        }

        private static bool validateDirectory(string dir, bool crapOut, bool create)
        {
            Console.WriteLine();
            Console.WriteLine("Validating \"" + dir + "\"...");
            Console.WriteLine();
            if (Directory.Exists(dir))
            {
                Console.ForegroundColor = Good;
                Console.WriteLine("Directory \"" + dir + "\" has been validated.");
                Console.ForegroundColor = OriginalForeground;
                return true;
            }
            else
            {
                if (crapOut)
                {
                    Console.ForegroundColor = Bad;
                    Console.WriteLine("ERROR : Could not find part of the path \"" + dir + "\"");
                    Console.ForegroundColor = OriginalForeground;
                    Environment.Exit(0);
                }
                else
                {
                    Console.ForegroundColor = Warning;
                    Console.WriteLine("Could not find part of the path \"" + dir + "\"");
                    Console.ForegroundColor = OriginalForeground;

                    if (create)
                    {
                        try
                        {
                            Console.WriteLine("Creating directory \"" + dir + "\"...");
                            Directory.CreateDirectory(dir);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = Bad;
                            Console.Write(ex.Message);
                            Console.ForegroundColor = OriginalForeground;
                            throw;
                        }
                    }
                }

                return false;
            }
        }

        private static bool validateToolPath(string toolPath)
        {
            if (File.Exists(toolPath)) return true;
            else return false;
        }

        private static void validateInstallation(string installPath, string msi, string deployLog)
        {
            Console.WriteLine();
            Console.WriteLine("Validating the installation...");
            Console.WriteLine();

            if (validateDirectory(installPath, false, false))
            {
                DateTime today = DateTime.Today;

                DirectoryInfo di = new DirectoryInfo(installPath);
                if (di.CreationTime.Date == today)
                {
                    Console.ForegroundColor = Good;
                    Console.WriteLine(msi + " has been successfully installed.");
                    displayInstallOutput(installPath, deployLog);
                    Console.ForegroundColor = OriginalForeground;
                }
            }
            else
            {
                Console.ForegroundColor = Bad;
                Console.WriteLine("ERROR: " + msi + " did not install correctly.");
                displayInstallOutput(installPath, deployLog);
                Console.ForegroundColor = OriginalForeground;
            }
        }

        #endregion // Validation

        #region Environment Methods

        private static string determineEnvironment(string[] args, string machineName)
        {
            string machine = machineName.ToUpper();

            string env = "?";
            if (args.Count() > 1)
            {
                // Means that you're passing an environment
                env = args[1].ToUpper();
            }
            else
            {
                string spcProdPattern = "^JOBQ[0-9]$";
                string spcProdDRPattern = "^JOBQ[0-9]D$";

                if (machine.Contains("PROD") || System.Text.RegularExpressions.Regex.IsMatch(machine, spcProdPattern, RegexOptions.IgnoreCase) || System.Text.RegularExpressions.Regex.IsMatch(machine, spcProdDRPattern, RegexOptions.IgnoreCase))
                {
                    env = "PROD";
                }
                if (machine.Contains("AUTO1"))
                {
                    env = "AUTO1";
                }
                if (machine.Contains("DEV2"))
                {
                    env = "DEV2";
                }
                if (machine.Contains("QA2"))
                {
                    env = "QA2";
                }
                if (machine.Contains("QA1"))
                {
                    env = "QA1";
                }
                if (machine.Contains("MODEL1"))
                {
                    env = "MODEL1";
                }
                if (machine.Contains("MODEL2"))
                {
                    env = "MODEL2";
                }
            }

            return env;
        }

        #endregion //Environment Methods

        #region MSI Methods

        private static void displayMsiInfo(string msi)
        {
            FileInfo fi = new FileInfo(msi);

            Console.ForegroundColor = Info;
            Console.WriteLine();
            Console.WriteLine(fi.Name + ":");
            Console.WriteLine("Created: " + fi.CreationTime);
            Console.WriteLine("Full Path: " + fi.FullName);
            Console.WriteLine();
            Console.ForegroundColor = OriginalForeground;
        }

        private static void installMsi(string msi, string installPath, string machineName, string env, string deployLog)
        {
            string exe = @"C:\WINDOWS\system32\msiexec.exe";
            if (validateToolPath(exe))
            {
                Console.WriteLine();
                Console.WriteLine("Installing msi \"" + msi + "\" to folder: \"" + installPath + "\" on machineName: " + machineName + " for Environment: " + env);

                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = exe;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.Arguments = "/i \"" + msi + "\"/qn /l*v " + deployLog + " ENVIRONMENT=" + env;
                    p.Start();
                    p.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = Bad;
                    Console.WriteLine();
                    Console.WriteLine("ERROR: An error occurred while installing " + msi);
                    Console.WriteLine();
                    Console.Write(ex.Message);
                    Console.ForegroundColor = OriginalForeground;
                    throw;
                }
            }
        }

        private static void displayInstallOutput(string installPath, string deployLog)
        {
            if (Directory.Exists(installPath))
            {
                Console.WriteLine();
                Console.WriteLine(installPath + " exists.  Checking output...");
                Console.WriteLine();
                DirectoryInfo di = new DirectoryInfo(installPath);
                Console.WriteLine("Creation Date: " + di.CreationTime);
                Console.WriteLine("Directory Contents:");
                foreach (string f in Directory.GetFiles(installPath, "*", SearchOption.AllDirectories))
                {
                    Console.WriteLine(f.Replace(installPath, ""));
                }
                Console.WriteLine();
                Console.WriteLine("Reference " + deployLog + " for more information.");
                Console.WriteLine();
            }
        }

        #endregion

        #region Backup and Clean

        private static void backUpAndCleanExisting(string installPath, string prodName, string backupRoot, string backupPath)
        {
            cleanBackups(backupRoot, prodName);

            if (validateDirectory(installPath, false, false))
            {
                Console.WriteLine();
                Console.WriteLine("Copying " + installPath + " to " + backupPath + "...");
                Console.WriteLine();
                string copyTool = @"C:\WINDOWS\system32\xcopy.exe";
                if (validateToolPath(copyTool))
                {
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.FileName = copyTool;
                        proc.StartInfo.Arguments = "\"" + installPath + "\" \"" + backupPath + "\" /E /I";
                        proc.Start();
                        proc.WaitForExit();

                        Console.ForegroundColor = Good;
                        Console.WriteLine("Backup completed");
                        Console.ForegroundColor = OriginalForeground;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = Warning;
                        Console.WriteLine("Failed to backup " + prodName);
                        Console.WriteLine();
                        Console.Write(ex.Message);
                        Console.WriteLine();
                        Console.ForegroundColor = OriginalForeground;
                        Console.WriteLine("You can manually back up the directory now.  Press the SpaceBar to continue when finished or press 'Esc' to cancel installation.");
                        ConsoleKeyInfo info = Console.ReadKey();
                        if (info.Key == ConsoleKey.Spacebar)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Continuing...");
                            Console.WriteLine();
                        }
                        else if (info.Key == ConsoleKey.Escape)
                        {
                            Environment.Exit(0);
                        }
                    }
                }

                try
                {
                    Directory.Delete(installPath, true);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = Warning;
                    Console.WriteLine("Error occurred while deleting " + installPath);
                    Console.WriteLine();
                    Console.Write(ex.Message);
                    Console.WriteLine();
                    Console.ForegroundColor = OriginalForeground;

                    manuallyDeletePreviousInstall(installPath);
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(installPath + " does not exist on this server.  Skipping backup process.");
                Console.WriteLine();
            }
        }

        private static void manuallyDeletePreviousInstall(string installPath)
        {
            if (Directory.Exists(installPath))
            {
                Console.WriteLine(installPath + " still exists and must be deleted or renamed before continuing.");
                Console.WriteLine();
                Console.WriteLine("You can manually delete the directory now.  Press the SpaceBar to continue when finished or press 'Esc' to cancel installation.");
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.Key == ConsoleKey.Spacebar)
                {
                    if (Directory.Exists(installPath))
                    {
                        manuallyDeletePreviousInstall(installPath);
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Continuing...");
                        Console.WriteLine();
                    }
                }
                else if (info.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.ForegroundColor = Good;
                Console.WriteLine("Not sure what happened but " + installPath + " does not exist.  Safe to continue...");
                Console.ForegroundColor = OriginalForeground;
            }
        }

        private static void cleanBackups(string backupRoot, string prodName)
        {
            Console.WriteLine();
            Console.WriteLine("Deleting all old backups of " + prodName + " from " + backupRoot + "...");
            foreach (string bu in Directory.GetDirectories(backupRoot))
            {
                if (bu.ToUpper().Contains(prodName.ToUpper()) && !bu.ToUpper().Contains("KEEP"))
                {
                    try
                    {
                        Directory.Delete(bu, true);
                    }
                    catch
                    {
                        Console.ForegroundColor = Warning;
                        Console.WriteLine("Failed to delete old backup - " + bu);
                        Console.ForegroundColor = OriginalForeground;
                    }
                }
            }
        }

        #endregion // Backup and Clean
    }
}
