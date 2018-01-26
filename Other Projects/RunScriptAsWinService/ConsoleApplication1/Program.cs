<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace ConsoleApplication1
{
   public class MyService : ServiceBase
{
    public static void Main(string[] args)
    {
        ServiceBase.Run(new MyService());
    }

    protected override void OnStart(string[] args)
    {
        base.OnStart(args);
        this.RunScript(@"Implementations.bat");
        Thread.Sleep(1000);
    }

    protected override void OnStop()
    {
        base.OnStop();
        this.RunScript(@"Implementations.bat");
        Thread.Sleep(1000);
    }

    private void RunScript(string processFileName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = Path.Combine(@"C:\GIT", processFileName),
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden

            /*
              FileName = "cmd.exe",
            Arguments = "/C " + Path.Combine(@"C:\server", processFileName),
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
             */
        };

             // startInfo.EnvironmentVariables.Add("GIT_HOME", @"c:\GIT");

        var process = new Process();
        process.StartInfo = startInfo;
        process.Start();
    }
}
    }

=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace ConsoleApplication1
{
   public class MyService : ServiceBase
{
    public static void Main(string[] args)
    {
        ServiceBase.Run(new MyService());
    }

    protected override void OnStart(string[] args)
    {
        base.OnStart(args);
        this.RunScript(@"Implementations.bat");
        Thread.Sleep(1000);
    }

    protected override void OnStop()
    {
        base.OnStop();
        this.RunScript(@"Implementations.bat");
        Thread.Sleep(1000);
    }

    private void RunScript(string processFileName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = Path.Combine(@"C:\GIT", processFileName),
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden

            /*
              FileName = "cmd.exe",
            Arguments = "/C " + Path.Combine(@"C:\server", processFileName),
            CreateNoWindow = true,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
             */
        };

             // startInfo.EnvironmentVariables.Add("GIT_HOME", @"c:\GIT");

        var process = new Process();
        process.StartInfo = startInfo;
        process.Start();
    }
}
    }

>>>>>>> 9b26097d726bc3fbdcb36873849fecdf752e22e3
