//-------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//   Entry point for ClickThrough UI.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools.ClickThrough
{
    using System;
    using System.Collections.Specialized;
    using System.Windows.Forms;

    using Microsoft.Tools.WindowsInstallerXml;

    /// <summary>
    /// ClickThrough UI entry point.
    /// </summary>
    static public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Arguments passed to application.</param>
        [STAThread]
        static private void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClickThroughForm());
        }
    }
}
