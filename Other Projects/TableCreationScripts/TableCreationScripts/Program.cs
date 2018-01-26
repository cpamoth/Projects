using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TableCreateLib;

namespace TableCreationScripts
{
    class Program
    {
        static void Main()
        {
            Console.Write("Database:");
            string database = Console.ReadLine();

            Console.Write("Table Name:");
            string consoleReply = Console.ReadLine();
            Creator tablecreater = new Creator();
            tablecreater.Database = database;
            tablecreater.TableName = consoleReply;

            Trace.Listeners.Add(new ConsoleListener());

            while(!String.IsNullOrWhiteSpace(consoleReply))
            {
                consoleReply = GetColumnDefinition();
                if(!String.IsNullOrWhiteSpace(consoleReply))
                    tablecreater.Columns.Add(consoleReply);
            }
            Console.Write("Primary Key:");
            string primaryKey = Console.ReadLine();
            tablecreater.PrimaryKey = primaryKey;

            Console.Write("Where to save these:");
            string path = Console.ReadLine();
            tablecreater.Create(path);

            Console.Write("Press any key to quit");
            Console.ReadKey();
        }

        private static string GetPathInput()
        {
            StringBuilder pathout = new StringBuilder();
            string quits = "\r\n";
            char c = Console.ReadKey().KeyChar;
            while(!quits.Contains(c))
            {
                if (c == '\t')
                {
                    Console.Write("\b \b");
                    char sc = LookupNextPathItem(pathout);
                    if(sc == '\b')
                    {
                        pathout.Remove(pathout.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    else
                    {
                        pathout.Append(sc);
                    }
                }
                else
                {
                    if (c == '\b')
                    {
                        pathout.Remove(pathout.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    else
                    {
                        pathout.Append(c);
                    }
                }
                c = Console.ReadKey().KeyChar;
            }
            return pathout.ToString().Trim();
        }

        private static char LookupNextPathItem(StringBuilder pathout)
        {
            int lastdir = pathout.ToString().LastIndexOf("\\");
            string dir = pathout.ToString().Substring(0, lastdir);
            char output = '\b';
            if(Directory.Exists(dir))
            {
                string filefirst = pathout.ToString().Substring(lastdir + 1).ToLower();

                List<string> ff = new List<string>();
                ff.AddRange(Directory.GetFiles(dir));
                ff.AddRange(Directory.GetDirectories(dir+"\\"));
                string[] files = (from f in ff
                                  where Path.GetFileName(f).ToLower().StartsWith(filefirst)
                                  orderby Path.GetFileName(f).ToLower()
                                  select Path.GetFileName(f)).ToArray();
                if(files.Length < 1)
                {
                    return '\b';
                }
                int i = 0;
                string quits = "\r\n \\";
                int currentnamelen = filefirst.Length;
                for (int ci = 0; ci <= currentnamelen; ci++)
                {
                    Console.Write("\b");
                }


                Console.Write(files[0]);
                currentnamelen = files[0].Length;


                ConsoleKeyInfo c = Console.ReadKey();
                while(!quits.Contains(c.KeyChar))
                {
                    if(c.Key == ConsoleKey.Tab || c.Key == ConsoleKey.DownArrow)
                    {
                        i++;
                    }
                    if (c.Key == ConsoleKey.Backspace || c.Key == ConsoleKey.UpArrow)
                    {
                        i--;
                    }
                    if (i >= files.Length)
                    {
                        i = 0;
                    }
                    if (i < 0)
                    {
                        i = files.Length - 1;
                    }

                    for (int ci = 0; ci <= currentnamelen; ci++)
                    {
                        Console.Write("\b");
                    }

                    Console.Write(files[i]);
                    currentnamelen = files[i].Length;

                    c = Console.ReadKey();
                }
                output = c.KeyChar;
                pathout = new StringBuilder(dir);
                pathout.Append('\\');
                pathout.Append(files[i]);
            }
            return output;

        }

        private static string GetColumnDefinition()
        {
            Console.Write("Column Name and/or Description:");
            string colName = Console.ReadLine();
            if(Creator.IsFullDescription(colName))
            {
                return colName;
            }
            if (String.IsNullOrWhiteSpace(colName))
            {
                return String.Empty;
            }
            Console.Write("Data Type:");
            string dtype = Console.ReadLine();
            Console.Write("Nullable?(Y/N):");
            bool nullable = GetBoolFromString(Console.ReadLine());

            return String.Format("{0} {1} {2}"
                    , colName
                    , dtype
                    , nullable?"NULL":"NOT NULL"
                );
        }

        private static bool GetBoolFromString(string tfString)
        {
            if(tfString == null)
            {
                return false;
            }
            if(String.IsNullOrWhiteSpace(tfString))
            {
                return false;
            }
            if(tfString.ToLower().StartsWith("n"))
            {
                return false;
            }
            if(tfString.ToLower().StartsWith("f"))
            {
                return false;
            }
            if(tfString == "0")
            {
                return false;
            }
            return true;
        }
    }

    internal class ConsoleListener : TraceListener
    {

        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
