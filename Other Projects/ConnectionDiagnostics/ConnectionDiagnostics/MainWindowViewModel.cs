using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConnectionDiagnostics
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _searchDir;
        public string SearchDir
        {
            get { return _searchDir; }
            set
            {
                _searchDir = value;
                RaisePropertyChanged("SearchDir");
            }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                RaisePropertyChanged("Filter");
            }
        }

        private ObservableCollection<ConnectionInfo> _connections;
        public ObservableCollection<ConnectionInfo> Connections
        {
            get { return _connections; }
            set
            {
                _connections = value;
                RaisePropertyChanged("Connections");
            }
        }

        private ConnectionInfo _currentConn;
        public ConnectionInfo CurrentConn
        {
            get { return _currentConn; }
            set
            {
                _currentConn = value;
                RaisePropertyChanged("CurrentConn");
            }
        }

        public MainWindowViewModel()
        {

        }

        public void GetConnections()
        {
            string filter = Filter;
            if (string.IsNullOrEmpty(Filter))
            {
                filter = "*";
            }

            foreach (string f in Directory.GetFiles(SearchDir, filter, SearchOption.AllDirectories))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(f))
                    {
                        int lineNum = 1;
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            MatchCollection mc = Regex.Matches(line, @"(http[^ \s]+)([^<>][\s]|$)", RegexOptions.IgnoreCase);

                            if (mc != null)
                            {
                                foreach (Match m in mc)
                                {
                                    try
                                    {
                                        Uri validUri = new Uri(m.Value);
                                        string adress = Regex.Replace(m.Value, "</[a-zA-Z0-9]+>", "", RegexOptions.IgnoreCase).Replace("\"", "");
                                        TestSite(f, lineNum, adress, args[2], args[3]);
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                            line = sr.ReadLine();
                            lineNum++;
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private static void TestSite(string f, int lineNum, string Url, string user, string password)
        {
            ConsoleColor cc = Console.ForegroundColor;

            Console.WriteLine();
            Console.WriteLine(f + " (line " + lineNum.ToString() + ")");
            Console.WriteLine("Testing " + Url);

            string Message = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
                request.Credentials = new NetworkCredential(user, password);
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                }
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("No Connection");
                Console.Write(ex.Message);
                Console.WriteLine();

                Console.ForegroundColor = cc;

                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Success");

            Console.ForegroundColor = cc;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void VerifyPropertyName(string propertyName)
        {
            var myType = this.GetType();
            if (myType.GetProperty(propertyName) == null)
            {
                throw new ArgumentException("Property not found", propertyName);
            }
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
