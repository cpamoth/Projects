using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace InfIntrepretor
{
    /// <summary>
    /// This class represents the section unit for an inf.
    /// It contains the lines and section name.
    /// </summary>

    class InfSection
    {
        private String SectionName;
        private List<String> SectionLines;

        public InfSection( String sname )
        {
            SectionName = sname;
            SectionLines = new List<string>();
        }

        public InfSection( String sname, List<String> slines )
        {
            SectionName = sname;
            SectionLines = new List<string>();
            SectionLines = slines;
        }

        public String GetInfSectionName(){ return SectionName; }
        public List<String> GetInfSectionLines() { return SectionLines; }

        public void AddLine( String line )
        {
            SectionLines.Add(line);
        }

        public void ClearCommentedLines()
        {
            foreach (String s in SectionLines)
            {
                if (s[0] = ";")
                {
                    SectionLines.Remove(s);
                }
            }
        }

    }

    /// <summary>
    /// InfFileInstructions takes a section that pertains to files and parses the
    /// particular files attributes.
    /// </summary>
    class InfFileInstructions
    {
        public InfSection curInfSection;
        public String FileName;
        public String FileLocation;
        public String FileVersion;
        public String CLSID;
        public String DestDir;
        public String RegisterServer;


        public InfFileInstructions(String FileName, InfSection infs)
        {
            this.FileName = FileName;
            curInfSection = infs;

            ProcessFileSection();
        }

        /// <summary>
        /// This method will crawl through the section.
        /// 
        /// It will parse the various attributes for a file and hydrate a file operation.
        /// </summary>
        private void ProcessFileSection()
        {
            foreach (String line in curInfSection.GetInfSectionLines())
            {
                String[] vars = line.Split('=');

                if (vars.Length != 2)
                {
                    Console.WriteLine("Exception: bad line in the file section. {0}", line);
                }

                switch (vars[0].ToLower())
                {
                    //Here are the keywords that can be in a file section:
                    //File-%opersys%-%cpu%=[url | ignore | thiscab] 
                    //%opersys% can be one of [win32 | mac]. 
                    //%cpu% can be one of [x86 | ppc | mips | alpha]. 
                    //A URL can indicate the correct file for the target operating system and CPU, or the special value "ignore", 
                    //which indicates the file is not required for the specified platform. Microsoft Internet Explorer always looks for this 
                    //key before the platform-independent description key File=, as described below.
                    case "file-win32-x86":

                    //File=[url | thiscab] 
                    //It can be a URL or the special value "thiscab", 
                    case "file":
                        FileLocation = vars[1];
                        break;

                    //FileVersion=a,b,c,d 
                    //The FileVersion key specifies the minimum required version of the file specified by the File key. If no value is specified, any version is acceptable.
                    case "fileversion":
                        FileVersion = vars[1];
                        break;

                    //Clsid={nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn} 
                    //The value of the Clsid key is the string representation of the component CLSID, enclosed in braces {}.
                    case "clsid":
                        CLSID = vars[1];
                        break;

                    //DestDir determines where we'll put the file...currently we'll adjust this to whatever we see fit.
                    //DestDir=[10 | 11] 
                    //DestDir can be set to 10 to place the file into the \Windows directory or to 11 to place the file into the \Windows\System directory. 
                    //If no value is specified, the file is placed into the \Cache directory.
                    case "destdir":
                        DestDir = vars[1];
                        break;

                    //remove all of the registry keys for the file if this line exists.
                    //RegisterServer=[yes | no]
                    case "registerserver":
                        RegisterServer = vars[1];
                        break;


                    default:
                        Console.WriteLine("Exception: skipping processing of this line in inf {0}", line);
                        break;
                }

            }
        }


    }

    /// <summary>
    /// The main class for parsing an Inf
    /// </summary>
    class InfParser
    {
        String InfPath;
        List<InfSection> InfSections;
        bool HooksEnabled = false;

        public List<InfFileInstructions> InfFiles = null;
        public String MainBinFileName;

        
        public InfParser(String Path)
        {
            InfPath = Path;
            MainBinFileName = null;
            InfSections = new List<InfSection>();
            InfFiles = new List<InfFileInstructions>();

        }
    
        /// <summary>
        /// This function will take the inf and parse it into InfSection's.
        /// 
        /// The sections can then be ordered to create an execution order.
        /// </summary>
        public void ParseIntoSections( )
        {
            String InfLine;
            StreamReader InfReader = new StreamReader(InfPath);

            InfSection CurrentSection = null;

            while( (InfLine = InfReader.ReadLine() ) != null )
            {
                InfLine = InfLine.Trim();

                if (InfLine.Length == 0)
                {
                    //Add the current section to the list of sections.
                    if (CurrentSection != null)
                    {
                        InfSections.Add(CurrentSection);
                        CurrentSection = null;
                    }

                    continue;
                }

                if (InfLine[0] == '[')
                {
                    //if there can be sections without whitespace between them
                    //the code above to add the current section to InfSections must be added
                    CurrentSection = new InfSection(InfLine.Substring(1, InfLine.IndexOf(']')-1));
                }else if (InfLine[0] == ';') 
                {
                    continue;
                }
                else
                {
                    CurrentSection.AddLine(InfLine);
                }

            }


            if (CurrentSection != null)
            {
                //there was no whitespace after the last section
                //add the section to the list
                InfSections.Add(CurrentSection);
            }

            InfReader.Close();

        }

        public InfSection ReturnNamedSection(String sname)
        {
            foreach (InfSection infs in InfSections)
            {
                if (infs.GetInfSectionName() == sname)
                {
                    return infs;
                }
            }

            return null;
        }


        /// <summary>
        /// This function will probably need to be rewritten.
        /// 
        /// It just verifies the version section of the inf.
        /// </summary>
        public void CheckHookEnablement()
        {
            //Get the version section and check to see if this is an Internet Component Download inf
            //If it isn't bomb out the install with an invalid error issue.
            InfSection versect = ReturnNamedSection("version");
            if (versect == null)
            {
                Console.WriteLine("Exception: This is an invalid inf file with no version section.");
                HooksEnabled = false;
            }

            
            //TODO: this is fragile code.  "AdvancedINF=2.0" should be replaced by a set of language keywords.
            bool AdvInf = false;
            bool Chicago = false;
            foreach (String sline in versect.GetInfSectionLines())
            {
                if (sline == "AdvancedINF=2.0" )
                {
                    AdvInf = true;
                }

                if( sline == "Signature=\"$CHICAGO$\"" )
                {
                    Chicago = true;
                }
            }

            if (AdvInf && Chicago)
            {
                HooksEnabled = true;
            }
            else
            {
                HooksEnabled = false;
            }

            return;

        }


        public void InterpretInfToOpTree()
        {
            //this is implemented because the ICD spec includes a check in the 
            //CheckHookEnablement();
            
            //if (!HooksEnabled)
            //{
            //    Console.WriteLine("Exception: Hooks aren't enabled for this install - this is unusual.");
            //}
             

            //Get the [Setup Hooks] and [Add.Code] or [DefaultInstall] sections to check to see where to start processing
            //If none of these exist then bomb out the install with an invalid format error.
            InfSection SetupHooks = ReturnNamedSection( "Setup Hooks" );
            InfSection AddCode = ReturnNamedSection( "Add.Code" );
            InfSection DefaultInstall = ReturnNamedSection( "DefaultInstall" );

            //Currently we only support installs with only Add.Code sections.
            //if either section exists then we'll throw an exception.
            if (AddCode != null)
            {
                //This datastructure is currently replaced - 
                //if there were multiple sections with files you'd want to add them InfFiles
                InfFiles = CreateFileListFromSection(AddCode);

            }


            //Because this is an ActiveX Inf file, we should be building the op tree starting with the 
            if (SetupHooks != null || DefaultInstall != null)
            {
                //BUGBUG remove the exception...
                throw new System.IO.InvalidDataException();
            }
            
        }

        
        public List<InfFileInstructions> CreateFileListFromSection(InfSection InfFileSection)
        {
            List<InfFileInstructions> ifi = new List<InfFileInstructions>();

            //The section should already be specified for the InfSectionOpNode if it isn't there was an error
            foreach (String infline in InfFileSection.GetInfSectionLines())
            {
                //if this line is a hook then you should add it to the section list
                //this url specifies the line syntax: 
                //http://msdn.microsoft.com/library/default.asp?url=/workshop/delivery/download/overview/overview.asp

                //check if there is an equals sign
                string[] s = infline.Split('=');
                
                if (s.Length == 2)
                {
                    if (MainBinFileName == null) { MainBinFileName = s[0]; }
                    
                    ifi.Add( new InfFileInstructions(s[0], ReturnNamedSection(s[1]) ) );
                }
                else
                {
                    Console.WriteLine("Exception: this line is busted: {0}", infline);
                    throw new System.IO.InvalidDataException();
                }
            }

            return ifi;
        }

    }


}
