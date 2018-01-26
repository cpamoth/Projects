using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using SHDocVw;
using TableCreateLib;

namespace TableCreation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string _rootFolder = null;

        private void AddColumn(object sender, RoutedEventArgs e)
        {
            int start = columnList.SelectionStart;
            
            string insertval = String.Format("{0}{1} {2} {3}", 
                String.IsNullOrWhiteSpace(columnList.Text)?String.Empty:"\r\n", 
                cName.Text, 
                dType.Text, 
                canbenull.IsChecked.GetValueOrDefault() ? "NULL" : "NOT NULL");

            columnList.Text = columnList.Text.Insert(start, insertval);
            columnList.SelectionStart += columnList.Text.Length;
            
            cName.Text = String.Empty;
            dType.Text = String.Empty;
            canbenull.IsChecked = false;
            cName.Focus();
        }

        private void SaveFiles(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(dbName.Text) || String.IsNullOrWhiteSpace(tbName.Text) || String.IsNullOrWhiteSpace(columnList.Text) || String.IsNullOrWhiteSpace(pk.Text))
            {
                MessageBox.Show("You must fill out each element.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Creator tablecreater = new Creator
                {
                    Database = dbName.Text, 
                    TableName = tbName.Text,
                    UseDirectories = UseDirectories.IsChecked.GetValueOrDefault(),
                    Server = DbServer.Text
                };

            string[] cols = columnList.Text.Split('\r', '\n');
            foreach(string column in cols)
            {
                if(String.IsNullOrWhiteSpace(column))
                {
                    continue;
                }
                tablecreater.Columns.Add(column);
            }
            tablecreater.PrimaryKey = pk.Text;
            tablecreater.Create(saveLocation.Text, CreateArchive.IsChecked.GetValueOrDefault());
            MessageBox.Show("Save complete");

            OpenFolder(saveLocation.Text);
        }

        /// <summary>
        /// Opens the folder if not already open.
        /// </summary>
        /// <param name="folderLocation"></param>
        private static void OpenFolder(string folderLocation)
        {
            //don't open the folder if it's already open
            ShellWindows windows = new ShellWindows();

            List<string> openFolders = new List<string>();
            foreach (InternetExplorer ie in windows)
            {
                if ( ie!=null && ie.FullName != null && Path.GetFileNameWithoutExtension(ie.FullName).ToLower().Equals("explorer"))
                {
                    openFolders.Add(ie.LocationURL.ToLower().Trim(' ','\t','/'));
                }
            }

            string checkAgainst = "file:///" + folderLocation.ToLower().Trim(' ', '\t', '\\').Replace("\\", "/");
            if(folderLocation.StartsWith("\\\\"))
            {
                checkAgainst = "file://" + folderLocation.ToLower().Trim(' ', '\t', '\\').Replace("\\", "/");
            }

            if (!openFolders.Contains(checkAgainst))
            {
                Process.Start("explorer.exe", folderLocation);
            }

        }

        private void CreateMasterClick(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists(saveLocation.Text))
            {
                MessageBox.Show("You must provide a viable root folder");
                return;
            }
            MasterScript ms = new MasterScript(saveLocation.Text);
            ms.CreateMaster();
            MessageBox.Show("Complete");
            OpenFolder(saveLocation.Text);
        }

        private void SelectFolder(object sender, MouseEventArgs me)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();


            _rootFolder = _rootFolder ?? Environment.GetEnvironmentVariable("DevelopmentDir", EnvironmentVariableTarget.User) ?? @"c:\Svn\Gis\Development";
            
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = _rootFolder + @"\DatabaseProjects";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                saveLocation.Text = dialog.SelectedPath;
            }
        }

        private void StartBorder(object sender, MouseEventArgs e)
        {
            border1.Height = 1;
        }
        private void StopBorder(object sender, MouseEventArgs e)
        {
            border1.Height = 0;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            tbName.Text = String.Empty;
            columnList.Text = String.Empty;
            pk.Text = String.Empty;
        }
        private void GetBase(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog {Multiselect = false};
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                DetermineColumnsFromFile(dialog.FileName);
            }
        }

        private void DetermineColumnsFromFile(string filename)
        {
            List<string> pathParts = filename.Split('\\').ToList();
            pathParts.RemoveAt(pathParts.Count - 1);
            pathParts.RemoveAt(pathParts.Count - 1);

            saveLocation.Text = String.Join("\\", pathParts);

            using(FileStream tableStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using(StreamReader sr = new StreamReader(tableStream))
                {
                    StringBuilder columns = new StringBuilder();
                    bool startPrim = false;
                    bool startCols = false;
                    string line;
                    while((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("USE ["))
                        {
                            dbName.Text = line.Replace("USE [", String.Empty).Replace("]", String.Empty);
                        }
                        if(line.Contains("CREATE TABLE [dbo].["))
                        {
                            tbName.Text = line.Replace("CREATE TABLE [dbo].[", String.Empty).Replace("](", String.Empty).Replace("_base", String.Empty);
                            startCols = true;
                            continue;
                        }
                        if (line.Contains("PRIMARY KEY CLUSTERED"))
                        {
                            startCols = false;
                            startPrim = true;
                            continue;
                        }
                        if(startCols)
                        {
                            columns.AppendLine(line.Trim().Replace("]", String.Empty).Replace("[", String.Empty).Replace(",", String.Empty));
                        }
                        if(startPrim)
                        {
                            if(Regex.IsMatch(line,@"\s*\w"))
                            {
                                pk.Text = line.Trim();
                                startPrim = false;
                            }
                        }
                    }
                    columnList.Text = columns.ToString();
                    sr.Close();
                }
                tableStream.Close();
            }
        }
    }
}
