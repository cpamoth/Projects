namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class Converter
    {
        // this is a list of tags in wixv1 where the Id is specified in InnerText and it
        // should be converted to Id=" "
        private String[] m_sarrIdTags =  {   "Billboard",
                                             "BillboardAction",
                                             "Category",
                                             // "Column", COLUMN still seems to use the old format.
                                             "Control",
                                             "CopyFile",
                                             "Component",
                                             "ComponentRef",
                                             "ComponentSearch",
                                             "CustomAction",
                                             //"CustomTable", CUSTOMTABLE still seems to use the old format.
                                             "Dialog", 
                                             "Directory",
                                             "DirectoryRef",
                                             "DirectorySearch",
                                             "Environment",
                                             "Extension",
                                             "Feature",
                                             "File",
                                             "FileSearch",
                                             "IniFileSearch",
                                             "ListItem",
                                             "Merge",
                                             "MergeRef",
                                             "Module",
                                             "Property",
                                             "PropertyRef",
                                             "ODBCDataSource",
                                             "ODBCDriver",
                                             "ODBCTranslator",
                                             "ProgId",
                                             //"RadioButton", RadioButton still seems to use the old format.
                                             "RemoveFile",
                                             "Registry",
                                             "RegistrySearch",
                                             "ReserveCost",
                                             // "Row",	ROW still seems to use the old format.
                                             "ServiceControl",
                                             "ServiceDependency",
                                             "ServiceInstall",
                                             "Shortcut",
                                             "SqlDatabase",
                                             "SqlScript",
                                             "SqlString",
                                             "TextStyle",
                                             "TypeLib",
                                             "UIText",
                                             "Upgrade",
                                             "Verb",
                                             "WebAddress",
                                             "WebApplication",
                                             "WebAplpicationExtension",
                                             "WebAddress",
                                             "WebFilter",
                                             "WebDir",
                                             "WebDirProperties",
                                             "WebLockdown",
                                             "WebSite",
                                             "WebVirtualDir"
                                         };

        // this is a list of tags in wixv1 that have an Id='<guid>' attribute that need to be
        // coverted to Guid='<guid>'.
        private String[] m_sarrGuidTags =  {
                                               "Component",
                                               "Module"
                                               //"Package"
                                               //"Product"
                                           };

        // this is a list of tags in wixv1 that might have xmlns= attributes which should be killed.
        private String[] m_sarrOldXmlnsTags =  {
                                                   "Module",
                                                   "Product",
        };

        private String m_suncInputFile = null;
        public String InputFile
        {
            set { m_suncInputFile = value; }
            get { return m_suncInputFile; }
        }

        private String m_suncOutputFile = null;
        public String OutputFile
        {
            set { m_suncOutputFile = value; }
            get { return m_suncOutputFile; }
        }

        private String m_suncWixXsdFile = null;
        public String WixXsdFile
        {
            set { m_suncWixXsdFile = value; }
            get { return m_suncWixXsdFile; }
        }

        private bool m_fIsInclude = false;
        public bool IsInclude
        {
            set { m_fIsInclude = value; }
            get { return m_fIsInclude; }
        }

        private bool m_fReplaceWxfWithWxi = false;
        public bool ReplaceWxfWithWxi
        {
            set { m_fReplaceWxfWithWxi = value; }
            get { return m_fReplaceWxfWithWxi; }
        }

        private Hashtable m_hshtTagSequences = null;

        public void Convert()
        {
            if (m_suncInputFile == null)
            {
                Console.WriteLine("No input file was specified; no conversion occurred.");
                return; 
            }

            if (m_suncOutputFile == null)
            {
                Console.WriteLine("No output file was specified; no conversion occurred.");
                return; 
            }

            if (!File.Exists(m_suncInputFile))
            {
                Console.WriteLine("Could not find file '" + m_suncInputFile + "'; no conversion occurred.");
                return;
            }

            LoadTagOrdering();

            String sSource = ReadTextFrom(m_suncInputFile);

            // --- THIS IS THE LIST OF OPERATIONS WE DO TO CONVERT WIXV1 TO WIXV2

            StripTagsOfType(ref sSource, "?xml");

            if (ReplaceWxfWithWxi)
            {
                sSource = sSource.Replace(".wxf", ".wxi");
            }

            ConvertTagNameUnderneathTag(ref sSource, "Feature", "Component", "ComponentRef");
            ConvertTagNameUnderneathTag(ref sSource, "Feature", "Module", "MergeRef");
            ConvertTagNameUnderneathTag(ref sSource, "Directory", "Module", "Merge");
            StripAttributeFromTags(ref sSource, m_sarrOldXmlnsTags, "xmlns");
            ChangeAttributeNameForTags(ref sSource, m_sarrGuidTags, "Id", "Guid");
            ChangeAttributeNameForTag(ref sSource, "Binary", "Name", "Id");
            ChangeAttributeNameForTag(ref sSource, "Icon", "Name", "Id");
            ChangeAttributeNameForTag(ref sSource, "Media", "DiskId", "Id");
            ReplaceIdsForTags(ref sSource, m_sarrIdTags);
            ResequenceTags(ref sSource);
            AddWrapperTag(ref sSource);

            // --- END LIST OF OPERATIONS
            EnsureDirectory(m_suncOutputFile);
            WriteTextTo(m_suncOutputFile, sSource);
        }

        private void LoadTagOrdering()
        {
            if (m_suncWixXsdFile == null)
            {
                Console.WriteLine("No WiX file was specified.  Resequencing of your WiX file will not occur.");
                return;
            }

            if (!File.Exists(m_suncWixXsdFile))
            {
                Console.WriteLine("Could not find WiX XSD file at '" + m_suncWixXsdFile + "'.  Resequencing of your WiX source will not occur.");
                return;
            }

            XmlDocument xmldoc = new XmlDocument();

            xmldoc.Load(m_suncWixXsdFile);

            XmlNamespaceManager xmlnsmgr = new XmlNamespaceManager(xmldoc.NameTable);
            xmlnsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            XmlNodeList xnlElements = xmldoc.DocumentElement.SelectNodes("xs:element", xmlnsmgr);
            m_hshtTagSequences = new Hashtable();

            foreach (XmlNode xmlnode in xnlElements)
            {
                XmlNode xmlnodeSeq = xmlnode.SelectSingleNode("xs:complexType/xs:sequence", xmlnsmgr);

                if (xmlnodeSeq != null)
                {
                    XmlNode xmlnodeName = xmlnode.Attributes.GetNamedItem("name");

                    if (xmlnodeName != null)
                    {
                        ArrayList arrliTagNames = new ArrayList();
                        XmlNodeList xnlSeqElements = xmlnodeSeq.SelectNodes("xs:element", xmlnsmgr);

                        foreach (XmlNode xmlnodeSeqChild in xnlSeqElements)
                        {
                            XmlNode xmlnodeAttrRef = xmlnodeSeqChild.Attributes.GetNamedItem("ref");

                            if (xmlnodeAttrRef != null)
                                arrliTagNames.Add(xmlnodeAttrRef.InnerXml);
                        }

                        m_hshtTagSequences.Add(xmlnodeName.InnerXml, arrliTagNames);
                    }
                }
            }
        }

        private void ResequenceTags(ref String source)
        {
            if (m_hshtTagSequences == null)
                return;

            foreach (String sKey in m_hshtTagSequences.Keys)
            {
                ArrayList arrliTags = (ArrayList)m_hshtTagSequences[sKey];

                // look at the list of tags backwards so that when we make tags first in the list
                // the sequence comes out right.
                for (int i=arrliTags.Count - 1; i>=0; i--)
                {
                    MakeTagNameUnderneathTagFirst	(ref source, sKey, (String)arrliTags[i]);
                }
            }
        }

        private void AddWrapperTag(ref String source)
        {
            if (!m_fIsInclude)
                source = "<?xml version='1.0'?><Wix xmlns='http://schemas.microsoft.com/wix/2003/01/wi'>" + source + "</Wix>";
            else
            {
                ConvertTagName(ref source, "Fragment", "Include");
                source = "<?xml version='1.0'?>" + source;
            }
        }

        private void ReplaceIdsForTags(ref String source, String[] idTags)
        {
            foreach (String s in idTags)
                ReplaceIdsForTag(ref source, s);
        }

        private void ReplaceIdsForTag(ref String source, String tag)
        {
            Regex regexTagStart = new Regex("<" + tag + "(\\s|>)");
            Regex regexTagEnd = new Regex("</" + tag + ">");

            int iTagBase = source.IndexOf("<" + tag);

            Match matchStart = regexTagStart.Match(source);
            while (matchStart.Success)
            {
                int iEndOfTag = GetEndOfTag(source, tag, matchStart.Index);
                int iEndOfStartTagInitial = matchStart.Index + matchStart.Length - 1;
                int iEndOfStartTag = source.IndexOf(">", iEndOfStartTagInitial);
                iEndOfStartTag++;

                // we've found the end tag for this start tag.
                if (iEndOfTag >= 0)
                {
                    //iEndOfTag will not be on the end of the tag, but will be at the beginning of the 
                    //inner text.  GetEndOfTag() actually positions iEndOfTag 1 character after the end
                    //of the tag.  For example:
                    //<File Name="Abou0.htm" ...>Abou0.htm</File><File Name="Abou1.htm" ...">Abou1.htm</File>
                    //
                    //In this case GetEndOfTag() will position iEndOfTag at the ent of '</File><' (col 66 above, the beginning of the next tag).
                    //If there was a space after the ending tag (</File> <), then the code below (source.LastIndexOf)
                    //would find the inner text of the tag just fine because iEndOfTag was positioned at the space
                    //...but if there was not a space (</File><), then the inner text of the tag would include </File>.
                    //I tried to change source.LastIndexOf (below) to source.IndexOf, but alot of other stuff
                    //just plain broke.  Also, I looked into changing GetEndOfTag(), but alot of other code relies
                    //on it and I did not want to destabilize that code.  So, I put in a hack to subtract 1 from
                    //the 'end of the tag'.  deanj
                    int iBeginningOfEndTag = source.LastIndexOf("<", (iEndOfTag -1));

                    if (iBeginningOfEndTag > iEndOfStartTag)
                    {
                        String sInnerText = source.Substring(iEndOfStartTag, iBeginningOfEndTag - iEndOfStartTag);

                        // strip out the tags in innertext and trim it up, and there is our ID.					
                        String sId = sInnerText;

                        StripTagsAndTheirContents(ref sId);

                        sId = sId.Trim();

                        // are there no sub tags in this XML element? then convert it to
                        // a single tag (e.g,  <Foo></Foo> becomes <Foo/>)
                        if (sInnerText.IndexOf("<") < 0)
                        {
                            String sTextAfterTag = source.Substring(iEndOfTag, source.Length - iEndOfTag);

                            source = source.Substring(0, iEndOfStartTagInitial) + " Id='" + sId + "'" + source.Substring(iEndOfStartTagInitial, (iEndOfStartTag -1) - iEndOfStartTagInitial) + "/>" + sTextAfterTag;
                        }
                        else
                        {
                            String sTextAfterTag = source.Substring(iEndOfStartTag, source.Length - iEndOfStartTag);

                            // remove the Id tag from the innertext.
                            // this is just a simple check to see if the person has put the tag somewhere before 
                            // the start of the next tag... if the person put the Id somewhere weird, like in between tags,
                            // it'll stay there.
                            int iNextTag = sTextAfterTag.IndexOf("<");
                            int iId = sTextAfterTag.IndexOf(sId);

                            if (iNextTag > 0 && iId >= 0 && iId < iNextTag)
                            {
                                sTextAfterTag = sTextAfterTag.Substring(0, iId) + sTextAfterTag.Substring(iId + sId.Length, sTextAfterTag.Length - (iId + sId.Length));
                            }

                            source = source.Substring(0, iEndOfStartTagInitial) + " Id='" + sId + "'" + source.Substring(iEndOfStartTagInitial, iEndOfStartTag - iEndOfStartTagInitial) + sTextAfterTag;
                        }
                    }
                }

                matchStart = regexTagStart.Match(source, iEndOfStartTag);
            }
        }


        /* ============ XML UTILITY FUNCTIONS ===========================*/

        public void MakeTagNameUnderneathTagFirst(ref String source, String underneathTagName, String tagName)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            Regex regexSpecificTagStart = new Regex("<" + underneathTagName + "(\\s|>)");
            Match matchSpecificTagStart = regexSpecificTagStart.Match(source);

            while (matchSpecificTagStart.Success)
            {
                int i = matchSpecificTagStart.Index;
                int iNextTagStart= source.IndexOf("<", i + 1);
                int iNextStartTagEnd= source.IndexOf(">", i + 1);
                int iNextSingletonTagEnd = source.IndexOf("/>", i + 1);
                int iNextComplexTagEnd = GetEndOfTag(source, underneathTagName, i);

                if (iNextSingletonTagEnd < 0 ||   // make sure this tag is not a singleton
                    (iNextTagStart > i && iNextSingletonTagEnd > iNextTagStart) )
                {
                    iNextStartTagEnd++;
                    int iLast = source.LastIndexOf("</", iNextComplexTagEnd);

                    String sTag = source.Substring(iNextStartTagEnd, iLast - iNextStartTagEnd );

                    String sDiscard= StripTagsOfTypeAndTheirContents(ref sTag, tagName, true);

                    MakeTagNameUnderneathTagFirst(ref sTag, underneathTagName, tagName);
                    sTag = sDiscard + sTag;

                    source = source.Substring(0, iNextStartTagEnd) + sTag + source.Substring(iLast, source.Length - iLast);
                    matchSpecificTagStart = regexSpecificTagStart.Match(source, iNextComplexTagEnd);
                }
                else
                    matchSpecificTagStart = regexSpecificTagStart.Match(source, iNextSingletonTagEnd);
            }
        }

        private int GetEndOfTag(String source, String tagName, int startIndex)
        {
            Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|>)");
            Regex regexSingleTagEnd = new Regex("/>");
            Regex regexComplexTagEnd = new Regex("</" + tagName + ">");
            Regex regexTagStart = new Regex("<\\w");


            // really find the start of the tag (should match startIndex)
            Match matchStart = regexSpecificTagStart.Match(source, startIndex);

            if (!matchStart.Success)
            {
                Console.WriteLine("Warning: could not refind start tag '" + tagName + "'.");
                return -1;
            }


            // find the end of the start tag
            int iEndOfStartTagInitial = matchStart.Index + matchStart.Length;

            // find next </Foo>
            Match matchNextComplexCloser = regexComplexTagEnd.Match(source, iEndOfStartTagInitial);

            // find next <Foo
            Match matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, iEndOfStartTagInitial);

            // find next />
            Match matchNextSingleCloser = regexSingleTagEnd.Match(source, iEndOfStartTagInitial);

            // find next <
            Match matchNextSingleStart = regexTagStart.Match(source, iEndOfStartTagInitial);

            // are we dealing with a singleton (<Foo/>) tag?  if so, just kill it.
            if (matchNextSingleCloser.Success &&
                (!matchNextSingleStart.Success || matchNextSingleCloser.Index < matchNextSingleStart.Index) &&
                (!matchNextComplexCloser.Success || matchNextSingleCloser.Index < matchNextComplexCloser.Index)
                )
            {
                return matchNextSingleCloser.Index + 2;
            }


            // ok, it's a complex tag.  We need to maintain a count of the number of start tags
            // we have seen, and subtract out the number of closer tags we have seen
            // when starttags == 0, we know we are done.
            int iStartTags = 1;

            while (iStartTags > 0 && matchNextComplexCloser.Success)
            {
                if (matchNextComplexCloser.Index < matchNextSpecificSingleStart.Index || !matchNextSpecificSingleStart.Success)
                {
                    iStartTags--;
                    if (iStartTags > 0)
                        matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 1);
                }
                    // is the next thing a tag start?
                else  if (matchNextSpecificSingleStart.Success &&
                    matchNextSpecificSingleStart.Index < matchNextComplexCloser.Index)
                {
                    iStartTags++;

                    // see if this is a singleton tag... if so, subtract the count back out.
                    matchNextSingleCloser = regexSingleTagEnd.Match(source, matchNextSpecificSingleStart.Index + 1);
                    matchNextSingleStart = regexTagStart.Match(source, matchNextSpecificSingleStart.Index + 1);

                    if (matchNextSingleCloser.Success &&                                // we've found a />
                        matchNextSingleCloser.Index < matchNextComplexCloser.Index &&   // it comes before the next </Foo>	
                        (matchNextSingleCloser.Index < matchNextSingleStart.Index ||    // it comes before the next <Bar> or <Foo> or whatever.
                        !matchNextSingleStart.Success))
                    {
                        iStartTags--;
                    }

                    matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, matchNextSpecificSingleStart.Index + 2);
                }
                else
                {
                    Console.WriteLine("Warning: malformed tags detected in source '" + source + "' (mismatched tree)");
                    return -1;
                }
            }

            if (matchNextComplexCloser.Success)
            {
                int iComplexEnd = source.IndexOf(">", matchNextComplexCloser.Index);

                // for any complex tag ender </, there should be a > that follows it. if not, bail.
                if (iComplexEnd >= 0)
                {
                    iComplexEnd++;
                    return iComplexEnd;
                }
            }

            Console.WriteLine("Warning: malformed tags detected in source '" + source + "' (no end tag)");
            return -1;
        }

        private int GetTagDepth(String source, int tagStartIndex)
        {
            int iTagDepth = 0;
            String sPrior = source.Substring(0, tagStartIndex);

            Regex regexSingleTagEnd = new Regex("/>");
            Regex regexComplexTagEnd = new Regex("</");
            Regex regexTagStart = new Regex("<\\w");

            Match matchNextTagStart = regexTagStart.Match(sPrior);
            Match matchNextTagEnd = regexComplexTagEnd.Match(sPrior);

            while (matchNextTagStart.Success || matchNextTagEnd.Success)
            {

                if ((matchNextTagEnd.Index < matchNextTagStart.Index && matchNextTagEnd.Success)  ||
                    !matchNextTagStart.Success)
                {
                    iTagDepth --;
                    matchNextTagEnd = regexComplexTagEnd.Match(sPrior, matchNextTagEnd.Index + 1);
                }
                    // is the next thing a tag start?
                else  if (	(matchNextTagStart.Index < matchNextTagEnd.Index && matchNextTagStart.Success) || 
                    !matchNextTagEnd.Success)
                {
                    iTagDepth++;

                    Match matchNextSingleTagEnd = regexSingleTagEnd.Match(sPrior, matchNextTagStart.Index + 2);
                    matchNextTagStart = regexTagStart.Match(sPrior, matchNextTagStart.Index + 1);

                    // see if this is a singleton tag... if so, subtract the count back out.
                    if (matchNextSingleTagEnd.Success)
                    {

                        if ((!matchNextTagStart.Success || 
                            matchNextSingleTagEnd.Index < matchNextTagStart.Index) && 
                            (!matchNextTagEnd.Success || 
                            matchNextSingleTagEnd.Index < matchNextTagEnd.Index))
                        {
                            iTagDepth--;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Warning: malformed tags detected in sPrior '" + sPrior + "' (mismatched tree)");
                    return -1;
                }
            }

            return iTagDepth;
        }

        public void ConvertTagNameUnderneathTag(ref String source, String underneathTagName, String oldTagName, String newTagName)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            Regex regexSpecificTagStart = new Regex("<" +underneathTagName + "(\\s|>)");

            Match matchStart = regexSpecificTagStart.Match(source);

            while (matchStart.Success)
            {
                int iEndOfTag= GetEndOfTag(source, underneathTagName, matchStart.Index);

                if (iEndOfTag >= 0 )
                {
                    String sTag = source.Substring(matchStart.Index, iEndOfTag - matchStart.Index);

                    ConvertTagName(ref sTag, oldTagName, newTagName);

                    source = source.Substring(0, matchStart.Index) + sTag + source.Substring(iEndOfTag, source.Length - iEndOfTag);
                }

                matchStart = regexSpecificTagStart.Match(source, matchStart.Index + 1);
            }
        }

        public void ConvertTagName(ref String source, String oldTagName, String newTagName)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            // stupid but effective...
            source = source.Replace("<" + oldTagName + "\t", "<" + newTagName + "\t");
            source = source.Replace("<" + oldTagName + " ", "<" + newTagName + " ");
            source = source.Replace("<" + oldTagName + ">", "<" + newTagName + ">");
            source = source.Replace("<" + oldTagName + "/>", "<" + newTagName + "/>");
            source = source.Replace("</" + oldTagName + ">", "</" + newTagName + ">");
        }

        public void StripAttributeFromTags(ref String source, String[] tagNames, String attributeName)
        {
            foreach (String tagName in tagNames)
                StripAttributeFromTag(ref source, tagName, attributeName);
        }

        private void StripAttributeFromTag(ref String source, String tagName, String attributeName)
        {

            // find tag starts
            int i = source.IndexOf("<" + tagName);

            while (i >= 0)
            {
                // find the end of the tag
                int iEnd = source.IndexOf(">", i);

                // if there is no closer for this tag, then things are bad.  bail.
                if (iEnd < i)
                {
                    Console.WriteLine("Incomplete tag detected in source '" + source + "'.  Parts of this file could not be processed.");
                    return;
                }

                // now find the attribute within the tag.
                int iAttributeStart = source.IndexOf(attributeName + "=", i);

                if (iAttributeStart > i && iAttributeStart < iEnd)
                {
                    // get the quote char in use... is it ' or " ?
                    char chQuote = source[iAttributeStart + attributeName.Length + 1];

                    // find the closing quote char
                    int iAttributeEnd = source.IndexOf(chQuote, iAttributeStart + attributeName.Length + 2);

                    if (iAttributeEnd > iAttributeStart && iAttributeEnd < iEnd)
                    {
                        source = source.Substring(0, iAttributeStart) + source.Substring(iAttributeEnd + 1, source.Length - (iAttributeEnd + 1));
                    }
                }

                i = source.IndexOf("<" + tagName, iEnd);
            }

        }

        private void ChangeAttributeNameForTags(ref String source, String[] tagNames, String oldAttributeName, String newAttributeName)
        {
            foreach (String tagName in tagNames)
                ChangeAttributeNameForTag(ref source, tagName, oldAttributeName, newAttributeName);
        }

        private void ChangeAttributeNameForTag(ref String source, String tagName, String oldAttributeName, String newAttributeName)
        {
            int i = source.IndexOf("<" + tagName);

            while (i >= 0)
            {
                int iEnd = source.IndexOf(">", i);

                if (iEnd < i)
                    return;

                int iOldAttribute = source.IndexOf(oldAttributeName, i);

                if (iOldAttribute > i && iOldAttribute < iEnd)
                {
                    source = source.Substring(0, iOldAttribute) + newAttributeName + source.Substring(iOldAttribute + oldAttributeName.Length, source.Length - (iOldAttribute + oldAttributeName.Length));
                }

                i = source.IndexOf("<" + tagName, iEnd);
            }

        }

        private void StripTagsOfType(ref String source, String tagName)
        {
            int iLeft = source.IndexOf("<" + tagName);

            while (iLeft >= 0)
            {
                int iRight = source.IndexOf(">", iLeft);
                if (iRight < iLeft)
                {
                    Console.WriteLine("Warning: source string '" + source + "' looks malformed.");
                    return;
                }

                source = source.Substring(0, iLeft) + source.Substring(iRight + 1, source.Length - (iRight + 1));

                iLeft = source.IndexOf("<" + tagName);
            }

            source = source.Replace("</" + tagName + ">", "");
        }

        public void StripTags(ref String source)
        {
            StripChunks(ref source, "<", ">");
        }

        private void StripChunks(ref String source, String chunkStart, String chunkEnd)
        {
            int iChunkStart = source.IndexOf(chunkStart);
            int iChunkEnd;

            while (iChunkStart >= 0)
            {
                iChunkEnd = source.IndexOf(chunkEnd);

                if (iChunkEnd >= iChunkStart)
                    source = source.Substring(0, iChunkStart) + source.Substring(iChunkEnd + chunkEnd.Length, source.Length - (iChunkEnd + chunkEnd.Length));
                else
                {
                    Console.WriteLine("Warning: malformed chunk found.  There is no chunk end '" + chunkEnd + "' for the start.");
                    return;
                }

                iChunkStart = source.IndexOf(chunkStart);
            }
        }

        private void StripTagsAndTheirContents(ref String source)
        {
            // deal with meta tags
            StripChunks(ref source, "<?", "?>");

            // deal with comments
            StripChunks(ref source, "<!--", "-->");

            // now deal with normal tags.
            Regex regexTagStart = new Regex("<\\w");
            Regex regexSingleTagEnd = new Regex("/>");
            Regex regexComplexTagEnd = new Regex("</");

            Match matchStart = regexTagStart.Match(source);

            while (matchStart.Success)
            {
                int iEndOfStartTagInitial = matchStart.Index + matchStart.Length;

                Match matchNextSingleCloser = regexSingleTagEnd.Match(source, iEndOfStartTagInitial);
                Match matchNextComplexCloser = regexComplexTagEnd.Match(source, iEndOfStartTagInitial);
                Match matchNextSingleStart = regexTagStart.Match(source, iEndOfStartTagInitial);

                // are we dealing with a singleton (<Foo/>) tag?  if so, just kill it.
                if (matchNextSingleCloser.Success &&
                    (!matchNextSingleStart.Success || matchNextSingleCloser.Index < matchNextSingleStart.Index) &&
                    (!matchNextComplexCloser.Success || matchNextSingleCloser.Index < matchNextComplexCloser.Index)
                    )
                {
                    source = source.Substring(0, matchStart.Index) + source.Substring(matchNextSingleCloser.Index + 2, source.Length - (matchNextSingleCloser.Index + 2));
                }
                else
                {
                    int iStartTags = 1;

                    while (iStartTags > 0 && matchNextComplexCloser.Success)
                    {
                        // is the next thing a />?
                        if (matchNextSingleCloser.Success &&                              // we've found a single closer
                            (matchNextSingleCloser.Index < matchNextSingleStart.Index ||  // it comes before the next <foo.. tag (or there is no more <foo.. tags)
                            !matchNextSingleStart.Success) &&
                            matchNextSingleCloser.Index < matchNextComplexCloser.Index)   // it comes before the next </foo
                        {
                            iStartTags--;
                            matchNextSingleCloser = regexSingleTagEnd.Match(source, matchNextSingleCloser.Index + 2);
                        }
                            // is the next thing a </ ?)
                            // note we always assume there must be a future complex closer...
                        else if ((matchNextComplexCloser.Index < matchNextSingleStart.Index || !matchNextSingleStart.Success) &&
                                 (matchNextComplexCloser.Index < matchNextSingleCloser.Index || !matchNextSingleCloser.Success))
                        {
                            iStartTags--;
                            if (iStartTags > 0)
                                matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 2);
                        }
                            // is the next thing a tag start?
                        else if (matchNextSingleStart.Success && matchNextSingleStart.Index < matchNextComplexCloser.Index &&
                                (matchNextSingleStart.Index < matchNextSingleCloser.Index || !matchNextSingleCloser.Success))
                        {
                            iStartTags++;
                            matchNextSingleStart = regexTagStart.Match(source, matchNextSingleStart.Index + 2);
                        }
                    }

                    if (matchNextComplexCloser.Success)
                    {
                        int iComplexEnd = source.IndexOf(">", matchNextComplexCloser.Index);

                        // for any complex tag ender </, there should be a > that follows it. if not, bail.
                        if (iComplexEnd < 0)
                        {
                            Console.WriteLine("Warning: malformed tags detected in source '" + source + "'");
                            return;
                        }

                        iComplexEnd++;

                        source = source.Substring(0, matchStart.Index) + source.Substring(iComplexEnd, source.Length - iComplexEnd);
                    }
                    else
                    {
                        Console.WriteLine("Warning: could not find a proper end tag in source '" + source + "'");
                        return;
                    }
                }

                matchStart = regexTagStart.Match(source);
            }
        }

        private String StripTagsOfTypeAndTheirContents(ref String source, String tagName, bool topLevelTagsOnly)
        {
            String sDiscardedContent = "";

            Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|>)");
            Match matchStart = regexSpecificTagStart.Match(source);

            while (matchStart.Success)
            {
                if (!topLevelTagsOnly || GetTagDepth(source, matchStart.Index) < 1)
                {
                    int iEndOfTag = GetEndOfTag(source, tagName, matchStart.Index);
                    int iTagBeginning = matchStart.Index - 1;

                    // back the count up and get the whitespace before the tag start.
                    while (iTagBeginning >= 0 && (source[iTagBeginning] == ' ' || source[iTagBeginning] == '\r' || source[iTagBeginning] == '\n' || source[iTagBeginning] == '\t'))
                        iTagBeginning--;

                    // we iterated until we found the first non whitespace char, so add one back in
                    // to take us to a whitespace char.
                    iTagBeginning++;
                    if (iEndOfTag < 0)
                        return null;

                    sDiscardedContent += source.Substring(iTagBeginning, iEndOfTag - iTagBeginning);
                    source = source.Substring(0, iTagBeginning) + source.Substring(iEndOfTag, source.Length - iEndOfTag);
                    matchStart = regexSpecificTagStart.Match(source, iTagBeginning);
                }
                else
                {
                    matchStart = regexSpecificTagStart.Match(source, matchStart.Index + 1);
                }
            }
            return sDiscardedContent;
        }

        /* ============ DISK UTILITY FUNCTIONS ===========================*/
        private void WriteTextTo(String file, String text)
        {
            try 
            {
                StreamWriter sw;

                sw = new StreamWriter(file);

                sw.Write(text);
                sw.Close();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("An unauthorized access exception '" + e.Message + " occurred when trying to write file '" + file + "'.", 1);
            }
            catch (IOException e)
            {
                Console.WriteLine("An IO exception '" + e.Message + " occurred when trying to write file '" + file + "'.", 1);
            }
        }

        private String ReadTextFrom(String file)
        {
            StreamReader sr = File.OpenText(file);

            String sText = sr.ReadToEnd();
            sr.Close();
            return sText;
        }

        private void EnsureDirectory(String targetFile)
        {
            String suncDirectory;
            int i;

            i = targetFile.LastIndexOf("\\");

            if (i >= 0)
            {
                suncDirectory = targetFile.Substring(0, i);
                if (!System.IO.Directory.Exists(suncDirectory))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(suncDirectory);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine("Warning, could create dir '" + suncDirectory  + ".'  Reason: " + e.Message);
                    }
                }
            }
        }
    }
}
