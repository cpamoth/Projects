using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Security.Permissions;
using System.Security;
using System.Text.RegularExpressions;

namespace Mail_Auditor
{
    public partial class MainForm : Form
    {
        List<ConfigInfo> Configs;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            mailCheck("LCMailOut.geninfo.com");
            mailCheck("mail.geninfo.com");

            refreshFilesList();

            foreach (ConfigInfo ci in Configs)
            {
                if (!cboTargets.Items.Contains(ci.MailTarget)) cboTargets.Items.Add(ci.MailTarget);
            }
        }

        private void mailCheck(string check)
        {
            TcpClient TcpScan = new TcpClient();

            Label lbl = null;

            if (check == "LCMailOut.geninfo.com") lbl = lblLCMailOutResult;
            else lbl = lblMailResult;

            try
            {
                // Try to connect 
                TcpScan.Connect(check, 25);
                lbl.Text = check + ":25 Connection Successful";
                lbl.ForeColor = Color.DarkGreen;
            }
            catch
            {
                // An exception occured, meaning the port is probably closed 
                lbl.Text = check + ":25 Connection Failed";
                lbl.ForeColor = Color.Red;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshFilesList();
        }

        private void refreshFilesList()
        {
            Configs = new List<ConfigInfo>();
            List<string> searchDirs = new List<string>();

            foreach (string d in Directory.GetDirectories("E:\\", "*", SearchOption.TopDirectoryOnly))
            {
                if (!d.Contains("System Volume Information"))
                {
                    searchDirs.Add(d);
                }
            }

            foreach (string dir in searchDirs)
            {
                foreach (string f in Directory.GetFiles(dir, "*.config", SearchOption.AllDirectories))
                {
                    if (File.ReadAllText(f).ToUpper().Contains("MAIL.GENINFO.COM"))
                    {
                        Configs.Add(new ConfigInfo
                        {
                            ConfigFileName = f,
                            MailTarget = "mail.geninfo.com"
                        });
                    }
                    else if (File.ReadAllText(f).ToUpper().Contains("LCMAILOUT.GENINFO.COM"))
                    {
                        Configs.Add(new ConfigInfo
                        {
                            ConfigFileName = f,
                            MailTarget = "LCMailOut.geninfo.com"
                        });
                    }
                }
            }

            dgvFiles.DataSource = Configs;
        }

        private void replaceInFile(string filePath, string searchText, string replaceText)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            content = Regex.Replace(content, searchText, replaceText, RegexOptions.IgnoreCase);

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(content);
            writer.Close();
        }

        private bool hasAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder.         
                // This will raise an exception if the path is read only or do not have access to view the permissions.         
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private void btnFixTargets_Click(object sender, EventArgs e)
        {
            if (cboTargets.Text != "")
            {
                var badTargets = from t in Configs where t.MailTarget != cboTargets.Text select t;
                foreach (ConfigInfo ci in badTargets)
                {
                    replaceInFile(ci.ConfigFileName, ci.MailTarget, cboTargets.Text.Trim());
                }

                refreshFilesList();
            }
            else
            {
                MessageBox.Show("Please select a new mail target");
            }
        }
    }

    class ConfigInfo
    {
        private string fn;
        public string ConfigFileName
        {
            get { return fn; }
            set { fn = value; }
        }

        private string mt;
        public string MailTarget
        {
            get { return mt; }
            set { mt = value; }
        }
    }
}
