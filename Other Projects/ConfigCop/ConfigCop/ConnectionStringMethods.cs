using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConfigCop
{
    class ConnectionStringMethods
    {
        public static void CheckForDatabaseServerDiscrepancies(string line, int lineNumber)
        {
            List<DBServerInfo> dbServers = GetAllDatabaseServers();

            foreach (DBServerInfo dbi in dbServers)
            {
                if (IgnoreMethods.Ignore(dbi.Name) == false || IgnoreMethods.Ignore(dbi.Port) == false || IgnoreMethods.Ignore(dbi.Region) == false)
                {
                    if (line.ToUpper().Contains(dbi.Name.ToUpper()))
                    {
                        TestServerRegion(lineNumber, dbi, line);
                        TestDBServerConnection(line, lineNumber, dbi);
                    }
                }
            }
        }

        private static void TestServerRegion(int lineNumber, DBServerInfo dbi, string line)
        {
            if (Program.Region.ToUpper() != dbi.Region.ToUpper())
            {
                Errors.ServerDiscrepancy(lineNumber, dbi.Name, line);
            }
        }

        private static void TestDBServerConnection(string line, int lineNumber, DBServerInfo dbi)
        {
            TestedConnection tc = HelperMethods.ConnectionTested(dbi.Name);
            if (tc != null)
            {
                Errors.ConnectivityError(lineNumber, dbi.Name, tc.Reason, tc.StatusCode, 2);
            }
            else
            {
                IPAddress[] IPs = Dns.GetHostAddresses(dbi.Name);
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                int port;
                int.TryParse(dbi.Port, out port);

                try
                {
                    s.Connect(IPs[0], port);
                    HelperMethods.AddConnection(dbi.Name, 0, "", 2, dbi.Port);
                }
                catch
                {
                    int sc = Errors.GetStatusCode("ServerConnectionError");
                    Errors.ConnectivityError(lineNumber, dbi.Name, "Could not connect to remote server.", sc, 2);
                    HelperMethods.AddConnection(dbi.Name, sc, "Could not connect to remote server.", 2, dbi.Port);
                }
                finally
                {
                    s.Close();
                    s.Dispose();
                }
            }
        }

        private static List<DBServerInfo> GetAllDatabaseServers()
        {
            List<DBServerInfo> dbServers = new List<DBServerInfo>();
            string[] regions = HelperMethods.CheckConfiguration("regions").Split(',');
            foreach (string r in regions)
            {
                string[] servers = HelperMethods.CheckConfiguration(r).Split(',');
                foreach (string svr in servers)
                {
                    string name;
                    string port;
                    ExtractPort(r, svr, out name, out port);

                    dbServers.Add(new DBServerInfo { Name = name, Region = r, Port = port });
                }
            }

            return dbServers;
        }

        private static void ExtractPort(string r, string svr, out string name, out string port)
        {
            string[] parts = svr.Split('|');
            name = "";
            port = "";

            if (parts.Count() > 0)
            {
                name = parts[0];
            }
            if (parts.Count() == 2)
            {
                port = parts[1];
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No port defined for server " + name + " - " + r + " region.");
                Console.ResetColor();
                Environment.Exit(110);
            }
        }
    }

    class DBServerInfo
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Port { get; set; }
    }
}
