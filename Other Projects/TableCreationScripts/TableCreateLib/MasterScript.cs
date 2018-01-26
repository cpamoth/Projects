using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TableCreateLib
{
    public class MasterScript
    {
        public string RootDirectory { get; set; }
        public MasterScript(string directory)
        {
            RootDirectory = directory;
        }

        public void CreateMaster()
        {
            StringBuilder master = new StringBuilder();

            string[] subdirs = new [] {"DestructRelationships", "Tables", "Views", "Triggers", "StoredProcedures", "Stored Procedures", "Procedures", "Indexes", "Scripts", "ConstructRelationships" };
            foreach (string subdir in subdirs)
            {
                if (Directory.Exists(Path.Combine(RootDirectory, subdir)))
                {
                    master.AppendLine("--" + subdir);
                    string[] files = Directory.GetFiles(Path.Combine(RootDirectory, subdir), "*.sql");

                    foreach (string file in files.Where(file => Regex.IsMatch(file, @"relationships?\.rollback\.sql", RegexOptions.IgnoreCase)))
                    {
                        master.AppendFormat("    Print '{0}\\{1}'\r\n    :r \"{0}\\{1}\"\r\n", subdir, Path.GetFileName(file));
                    }

                    foreach (string file in files.Where(file => !Regex.IsMatch(file, "rollback", RegexOptions.IgnoreCase) && !Regex.IsMatch(file, @"relationships?\.")))
                    {
                        master.AppendFormat("    Print '{0}\\{1}'\r\n    :r \"{0}\\{1}\"\r\n", subdir, Path.GetFileName(file));
                    }
                    
                    foreach (string file in files.Where(file => Regex.IsMatch(file, @"relationships?\.sql", RegexOptions.IgnoreCase)))
                    {
                        master.AppendFormat("    Print '{0}\\{1}'\r\n    :r \"{0}\\{1}\"\r\n", subdir, Path.GetFileName(file));
                    }

                    master.Append("\r\n\r\n");
                }
            }

            string masterPath = Directory.GetFiles(RootDirectory).FirstOrDefault(file => Regex.IsMatch(Path.GetFileName(file), "master", RegexOptions.IgnoreCase) && !Regex.IsMatch(Path.GetFileName(file), "rollback", RegexOptions.IgnoreCase));
            if(string.IsNullOrEmpty(masterPath))
            {
                masterPath = Path.Combine(RootDirectory, "Master.sql");
            }

            using (FileStream fs = new FileStream(masterPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(master);
                    sw.Close();
                }
                fs.Close();
            }

            //rollbacks
            master = new StringBuilder();
            subdirs = subdirs.Reverse().ToArray();
            foreach (string subdir in subdirs)
            {
                if (Directory.Exists(Path.Combine(RootDirectory, subdir)))
                {
                    master.AppendLine("--" + subdir);
                    string[] files = Directory.GetFiles(Path.Combine(RootDirectory, subdir), "*.sql");
                    
                    foreach (string file in files.Where(file => Regex.IsMatch(file, @"relationships?\.rollback\.sql", RegexOptions.IgnoreCase)))
                    {
                        master.AppendFormat("    Print '{0}\\{1}'\r\n    :r \"{0}\\{1}\"\r\n", subdir, Path.GetFileName(file));
                    }
                    
                    foreach (string file in files.Where(file => !Regex.IsMatch(file,@"relationships?\.rollback\.sql", RegexOptions.IgnoreCase) && Regex.IsMatch(file,"rollback",RegexOptions.IgnoreCase)))
                    {
                        master.AppendFormat("    Print '{0}\\{1}'\r\n    :r \"{0}\\{1}\"\r\n", subdir, Path.GetFileName(file));
                    }

                    master.Append("\r\n\r\n");
                }
            }

            string masterRbPath = Directory.GetFiles(RootDirectory).FirstOrDefault(file => Regex.IsMatch(Path.GetFileName(file), "master", RegexOptions.IgnoreCase) && Regex.IsMatch(Path.GetFileName(file), "rollback", RegexOptions.IgnoreCase));
            if (string.IsNullOrEmpty(masterRbPath))
            {
                masterRbPath = Path.Combine(RootDirectory, "MasterRollback.sql");
            }

            using (FileStream fs = new FileStream(masterRbPath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(master);
                    sw.Close();
                }
                fs.Close();
            }


        }

    }
}
