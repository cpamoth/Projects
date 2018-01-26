//-------------------------------------------------------------------------------------------------
// <copyright file="wixconvert.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Convert wix 1 to wix 2.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;

    /// <summary>
    /// Converter class.
    /// </summary>
    public class WixConvertExe
    {
        /// <summary>
        /// Main entry point for the converter program.
        /// </summary>
        public static void Main() 
        {
            Converter wixconv = new Converter();
            bool showLogo = true;

            String[] sarr = System.Environment.GetCommandLineArgs();

            for (int i = 1; i < sarr.Length; i++)
            {
                String s = sarr[i];

                // let people use either -nologo or /nologo syntax.  
                // convert -nologo to /nologo.
                if (s[0] == '-')
                {
                    s = "/" + s.Substring(1, s.Length - 1);
                }

                switch (s.ToLower())
                {
                    case "/nologo":
                        showLogo = false;
                        break;
                    case "/isinclude":
                        wixconv.IsInclude = true;
                        break;
                    case "/wxftowxi":
                        wixconv.ReplaceWxfWithWxi = true;
                        break;
                    case "/wixxsd":
                        if (i < sarr.Length - 1)
                        {
                            i++;
                            wixconv.WixXsdFile = sarr[i];
                        }
                        break;
                    case "/out":
                        if (i < sarr.Length - 1)
                        {
                            i++;
                            wixconv.OutputFile = sarr[i];
                        }
                        break;
                    default:
                        wixconv.InputFile = sarr[i];
                        break;
                }
            }

            if (showLogo)
            {
                Console.WriteLine("Microsoft (R) Wix 1.0 to Wix 2.0 converter. Version 0.1"); 
                Console.WriteLine("Copyright (C) Microsoft Corporation 2003. All rights reserved.\n"); 
            }

            if (sarr.Length <= 1)
            {
                DisplayUsage();
                return;
            }

            wixconv.Convert();
        }

        /// <summary>
        /// Display program usage information.
        /// </summary>
        private static void DisplayUsage()
        {
            Console.WriteLine("");
            Console.WriteLine("    usage: 	wixconvert <input .wxp/.wxf file> -wixxsd <location of WixXSD>");
            Console.WriteLine("              	                 -out <output .wxs file>");
            Console.WriteLine("              	                 -isinclude");
            Console.WriteLine("              	                 -wxftowxi");
            Console.WriteLine("    example:	wixconvert foo.wxp -wixxsd ..\\wix2\\wix.xsd -out foo.wxs");
            Console.WriteLine("");
            Console.WriteLine("    Use IsInclude parameter for .wxi files. A different start tag will be used.");
            Console.WriteLine("    Use WxfToWxi parameter to convert .wxf to .wxi.");
            Console.WriteLine("    The location of the Wix XSD is necessary to resequence your WiX tags.");
            Console.WriteLine("    If you want to resequence your tags manually, simply omit the wixxsd ");
            Console.WriteLine("    parameter.");
            Console.WriteLine("");
            Console.WriteLine("    CONVERTING FROM WIX 1 to WIX 2");
            Console.WriteLine("");
            Console.WriteLine("    In general, Wix2 does a much better job of validating your Xml source code.");
            Console.WriteLine("    Therefore, you will may encounter logical errors -- invalid attributes, for");
            Console.WriteLine("    example -- that were ignored by Wix1 but will need to be fixed for Wix2. ");
            Console.WriteLine("");
            Console.WriteLine("    KNOWN ITEMS YOU WILL NEED TO CONVERT BY HAND:");
            Console.WriteLine("    (this list is NOT exhaustive.)");
            Console.WriteLine("");
            Console.WriteLine("    .  Component information that was under Feature now is under the");
            Console.WriteLine("       Directory/Component and has the “Advertise”=’yes’ attribute.");
            Console.WriteLine("    .  TypeLibs can no longer be children of Class elements, but Class elements");
            Console.WriteLine("       can now be children of TypeLibs.");
            Console.WriteLine("    .  MsiAssembly goo is gone and now you set \"Assembly\"='yes' on the");
            Console.WriteLine("       appropriate File.");
            Console.WriteLine("    .  You can't inline binary data for <Binary> tags.");
            Console.WriteLine("       Replace them with src=\"foo.bmp\".");
            Console.WriteLine("    .  For <include> operations, you may need to repath them so that they");
            Console.WriteLine("       are based on the location of your command prompt, not the location");
            Console.WriteLine("       of your .wxs file.");
            Console.WriteLine("    .  <Permission> tags now need an Id.");
            Console.WriteLine("    .  The Text attribute of <UIText> tags is no longer supported; move them");
            Console.WriteLine("       into the element text area.");
        }
    }
}
