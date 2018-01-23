//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

Option Compare Text     'For case insensitive string comparisons.;

using Microsoft.VisualBasic;

using System;

using System.Drawing;

using System.Diagnostics;

using System.Reflection;

using System.Collections;

using System.Windows.Forms;

using System.Xml;

public class MainDoc;

    // A class which represents the core engine of the XML Documentation Tool.

    // There is one and only one instance of a MainDoc per loaded assembly.  It

    // performs file loads, reflection, error management, and populates the

    // Assembly tree with member nodes.  A hash table is used to quickly

    // determine which entries in the XML Documentation file match members

    // contained within the assembly.  The key used for the hashtable lookup

    // is the raw string signature of the member it would be written to the

    // XML Documentation file.  These signatures are guaranteed to be unique because

    // they contain the fully qualified name of the member.

    private m_Members Hashtable      'A table for matching members in the assembly and XML Documentation files.;

    private m_ParentForm MainWindow  'The user interface for the tool.;

    public m_Asm [Assembly]          'The assembly the user has opened.;

    public m_XMLData VersionData     'Version information for the XML Documentation file.;

    public void new(form MainWindow, asm [Assembly]);

        m_ParentForm = form;

        m_Asm = asm;

        //We will do case insensitive comparison between hashtable keys.

        m_Members = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());

    }

    public void OpenXML(filename string);

        // Open the XML Documentation file, find the root node and start the

        // parse.

        if filename = "" { throw new Exception("The file name is ! valid.");

        xmlDoc XmlDocument = new XmlDocument();

        xmlDoc.Load(filename);

        root XmlNode = xmlDoc.FirstChild;

        if root == null {

            throw new Exception("Root node can! be found in the XML Documentation file '" + filename + "'.");

        }

        //Skip the XML document header.

        if root.Name = "xml" {

            //The root doc node should be next.

            root = root.NextSibling;

        }

        if LCase(root.Name) = "doc" {

            //Begin the import.

            ParseXML(root);

            //Make sure all the opened windows get updated if we've imported data for some of them.

            if m_ParentForm.MdiClient.MdiChildren.Length > 0 {

                nodeWindow NodeWindow;

                foreach(nodeWindow In m_ParentForm.MdiClient.MdiChildren;

                    nodeWindow.Update();

                }

            }

        else {

            throw new Exception("'doc' node can! be found.");

        }

    }

    private void ParseXML(root Xml.XmlNode);

        // Walk the XML Documentation file reading-in all the data.  The format

        // of the XML Documentation file is roughly:

        // 

        //               <doc>

        // Version info:

        //                   <assembly>

        //                       <name>

        //                       <version>

        //                       <fullname>

        //                   <members>

        //                       <member name="the raw signature">

        // Content nodes:

        //                           <summary>

        //                           <remarks>

        //                           <value>

        //                           <returns>

        //                           <param name="the parameter name">

        //

        // The signature determines which content nodes are allowed (for

        // example, fields are ! allowed to have param content).  The

        // signature format is discussed in MemberNode.

        //

        // This function walks each <member> node, comparing its signature to

        // the contents of the hashtable.  if it finds a match, then the member

        // node matches a member imported from the assembly.  if there is no

        // match, the member is found only in the XML Documentation file.  if

        // the signature can! be parsed for some reason, the member is Bad

        // and an error is reported.

        //

        // See MSDN for more information.

        child Xml.XmlNode;

        node Xml.XmlNode;

        foreach(child In root.ChildNodes;

            switch( LCase(child.Name);

                case "assembly";

                    //Build a version data structure from the version info contained within the XML Documentation file.

                    Name string = "";

                    Version string = "";

                    FullName string = "";

                    foreach(node In child.ChildNodes;

                        switch( LCase(node.Name);

                            case "name";

                                Name = node.InnerXml.Trim;

                            case "version";

                                Version = node.InnerXml.Trim;

                            case "fullname";

                                FullName = node.InnerXml.Trim;

                        }

                    }

                    m_XMLData = new VersionData(Name, Version, FullName);

                case "members";

                    foreach(node In child.ChildNodes;

                        if LCase(node.Name) = "member" {

                            RawSignature string      'The signature it appears in the XML Documentation file.;

                            CleanSignature string    'The signature cleaned of inconsistencies, used a key into the hashtable.;

                            try {

                                RawSignature = node.Attributes.GetNamedItem("name").Value.Trim;

                                //Parse the raw signature into its parts and reassemble it to "clean" it.

                                CleanSignature = ParseRawSignature(RawSignature).ToString();

                           } catch( e Exception;

                                //if something goes wrong with the parse, the signature is bad.

                                CleanSignature = BAD_SIG;

                            }

                            member MemberNode;

                            if m_Members.ContainsKey(CleanSignature) {

                                //The member has been imported from the Assembly.  Select that member.

                                member = CType(m_Members.Item(CleanSignature), MemberNode);

                            else {

                                //The member only appears in the XML Documentation file.  Construct a new one based off the signature.

                                member = new MemberNode(RawSignature, Me);

                            }

                            //Populate the content descriptors from the content nodes in the XML Documentation file.

                            member.PopulateFromXML(node);

                        else {

                            //We found a node other than "member" so construct a Bad member and put it in the tree.

                            BadMember MemberNode = new MemberNode("Bad Member", Me);

                            AddToTree(BadMember);

                            ReportError(BadMember, ErrorID.MemberNotValid1, node.Name);

                        }

                    }

                default: {

                    message string = "Unexpected node '" + child.Name + "' has been found and will be ignored. Abort load of XML file?";

                    if vbYes = MessageBox.Show(message, MsgBoxStyle.YesNo) {

                        break;

                    }

            }

        }

    }

    public void SaveXML(filename string);

        // Recursively walk the tree writing out each member to the XML

        // Documentation file.

        if filename = "" { throw new Exception("The file name is ! valid.");

        xmlStr IO.StreamWriter;

        xmlStr = new IO.StreamWriter(filename);

        //Write the XML header.

        xmlStr.WriteLine("<?xml version=""1.0""?>");

        xmlStr.WriteLine("<doc>");

        //Write the assembly version info.

        xmlStr.WriteLine(INDENT1 + "<assembly>");

        xmlStr.WriteLine(INDENT2 + "<name>" + m_Asm.GetName.Name + "</name>");

        xmlStr.WriteLine(INDENT2 + "<version>" + m_Asm.GetName.Version.Tostring + "</version>");

        xmlStr.WriteLine(INDENT2 + "<fullname>" + m_Asm.GetName.FullName + "</fullname>");

        xmlStr.WriteLine(INDENT1 + "</assembly>");

        xmlStr.WriteLine(INDENT1 + "<members>");

        //Recursively write the members (if there are any).

        if m_ParentForm.TreeView.GetNodeCount(false) > 0 {

            node MemberNode;

            foreach(node In m_ParentForm.TreeView.Nodes;

                RecursivelySave(xmlStr, node);

            }

        }

        //Write the ending.

        xmlStr.WriteLine(INDENT1 + "</members>");

        xmlStr.WriteLine("</doc>");

        xmlStr.Close();

    }

    private void RecursivelySave(xmlStr System.IO.StreamWriter, node MemberNode);

        // Each node is asked to generate its XML form.  This is then written

        // to the XML Documentation file.

        xmlstring string = node.GenerateXML();

        if ! xmlstring == null {

            xmlStr.Write(xmlstring);

        }

        if node.GetNodeCount(false) > 0 {

            //Recurse on the children of this node.

            subnode MemberNode;

            foreach(subnode In node.Nodes;

                RecursivelySave(xmlStr, subnode);

            }

        }

    }

    public void AddToTree(member MemberNode);

        // Add the member node to the appropriate location in the Assembly

        // tree.  We use the path of the member to walk down the tree.  Because

        // namespaces are ! imported through refelection and may not

        // necessarily have entries in the XML Documentation file, we need to

        // create the namespace nodes on the fly if they can! be found when

        // walking down the tree.  We find matches based on case-insensitive

        // name comparison.  Names are case-insensitive unique when built from 

        // CLS compliant compilers.

        nodes TreeNodeCollection     'The parent where we will insert the new member.;

        nodes = m_ParentForm.TreeView.Nodes;

        //First we need to find the insertion point in the tree.

        //if the path is empty, this is a root node.

        if member.Path != "" {

            //Split the path into its components.  This will be our search order.

            leaves() string;

            leaves = member.Path.Split("."c);

            //In case we need to build a new node, we need the path to build it with.

            //Keep track of the path we walk down the tree.

            currentpath string = "";

            leaf string;

            foreach(leaf In leaves;

                found bool = false;

                node MemberNode;

                //Walk the tree up to the last node

                foreach(node In nodes;

                    if node.Text = leaf {    'This is a case-insensitive compare, because of Option Compare Text.;

                        found = true;

                        break;

                    }

                }

                //if one of the components of the path is ! found then we need to build

                //a node for it.  This is how Namespace nodes get built they are ! exposed

                //as exported types through reflection.

                if ! found {

                    //We need to create intermediate node, but what kind depends upon the source

                    //of the member we are trying to add.  if the member node comes from the assembly,

                    //then we know we need to build a Namespace for it.  if it comes from the XML Documentation

                    //file, then it is unknown what kind of node this is (could be a namespace, a class,

                    //a structure, etc.), so make it an error node.

                    if member.Source = NodeSource.Assembly {

                        node = new MemberNode(NodeKind.Namespace, NodeSource.Assembly, leaf, currentpath, Me);

                    else {

                        node = new MemberNode(NodeKind.Error, NodeSource.XML, leaf, currentpath, Me);

                    }

                    //Add the new node to the tree.

                    nodes.Add(node);

                    AddToHashtable(node);

                }

                if currentpath != "" {

                    currentpath += ".";

                }

                currentpath += leaf;

                nodes = node.Nodes;

            }

        }

        //Finally, add the member to the tree in the correct place.

        nodes.Add(member);

    }

    public void ImportAssembly();

        //Iterate through the assembly.

        t Type;

        foreach(t In m_Asm.GetTypes();

            if IsExportable(t) {

                ImportType(t);

            }

        }

    }

    private void ImportType(t Type);

        // Import a type object and then iterate over each of its members.

        // Because some special members are ! supposed to be displayed in the

        // tree (such property acecssors), we need to defer adding them to

        // the tree.  Once all members are imported from a type, we can

        // determine which of these special members to add.  This process works

        // by way of the Deferred Methods list where each special member is

        // added to this list.  we import Properties and Delegates, we add

        // their associated methods to the list of Methods to Ignore.  Once

        // the member import is complete, we add those special members which

        // appear only in the Deferred Methods list.  This has the effect of

        // filtering-out property accessors and delegate handlers so they don't

        // appear in the Assembly tree (since they are ! valid items to 

        // add comments for).

        //Take care of the type first.

        node MemberNode = new MemberNode(t, Me);

        AddToTree(node);

        AddToHashtable(node);

        member MemberInfo;

        DeferredMethods ArrayList = new ArrayList(4);

        MethodsToIgnore ArrayList = new ArrayList(4);

        //Now import each of its members.

        foreach(member In t.GetMembers(MemberBindingFlags);

            if IsExportable(member) {

                ImportMember(member, node.FriendlySignatureWithPath, DeferredMethods, MethodsToIgnore);

            }

        }

        //Process the deferred methods for this type.  if a deferred method also exists in the list of 

        //methods to ignore, then do nothing (to ignore it).  Otherwise, build a member node and add it to

        //the tree.

        method MethodInfo;

        foreach(method In DeferredMethods;

            if ! MethodsToIgnore.Contains(method) {

                membernode MemberNode = new MemberNode(method, node.FriendlySignatureWithPath, Me);

                AddToTree(membernode);

                AddToHashtable(membernode);

            }

        }

    }

    private void ImportMember(member MemberInfo, parentName string, DeferredMethods ArrayList, MethodsToIgnore ArrayList);

        // Import a member.  Construct a new member node for each member and

        // then add it to the tree (unless the member is special in which case

        // we either ignore it or add it to the Deferred Methods list,

        // depending on its kind).

        m MemberNode = null;

        switch( member.MemberType;

            case MemberTypes.Constructor;

                m = new MemberNode(CType(member, ConstructorInfo), parentName, Me);

            case MemberTypes.Field;

                //Skip the special "value__" fields for enums.

                if ! (member.DeclaringType.Isenum AndAlso ! CType(member, FieldInfo).IsLiteral AndAlso member.Name = "value__") {

                    m = new MemberNode(CType(member, FieldInfo), parentName, Me);

                }

            case MemberTypes.Method;

                method MethodInfo = CType(member, MethodInfo);

                //Some special-named members (like property accessors and event hookups) will ! 

                //be added to the tree, so we need to defer adding them to the tree until the entire 

                //type has been imported and we can determine which members ! to add.

                if method.IsSpecialName {

                    DeferredMethods.Add(method);

                else {

                    m = new MemberNode(method, parentName, Me);

                }

            case MemberTypes.Property;

                prop PropertyInfo = CType(member, PropertyInfo);

                //DATA_TYPE_HERE accessor methods will ! be added to the tree, so add these two accessors 

                //to the list of methods to remove.

                MethodsToIgnore.Add(prop.GetGetMethod(true));

                MethodsToIgnore.Add(prop.GetSetMethod(true));

                m = new MemberNode(prop, parentName, Me);

            case MemberTypes.Event;

                e EventInfo = CType(member, EventInfo);

                //Event hookup methods will ! be added to the tree, so add these two hookups 

                //to the list of methods to remove.

                MethodsToIgnore.Add(e.GetAddMethod(true));

                MethodsToIgnore.Add(e.GetRemoveMethod(true));

                m = new MemberNode(e, parentName, Me);

            case MemberTypes.NestedType;

                //null needs to be done here because this nested type will be imported separately (as a type, ! a member).

            default: {

                Debug.Fail("Unexpected member type: ", member.GetType.ToString();

        }

        //if we built a member node, add it to the Assembly tree and the hashtable.

        if ! m == null {

            AddToTree(m);

            AddToHashtable(m);

        }

    }

    public void ReportError(node MemberNode, ErrorID ErrorID, ParamArray voidstitutions string());

        m_ParentForm.ErrorWindow.ReportError(node, ErrorID, voidstitutions);

        m_ParentForm.UpdateErrorStatus();

    }

    public void ReportError(node MemberNode, descriptor ContentDescriptor, ErrorID ErrorID, ParamArray voidstitutions string());

        m_ParentForm.ErrorWindow.ReportError(node, descriptor, ErrorID, voidstitutions);

        m_ParentForm.UpdateErrorStatus();

    }

    public void DeleteNodeFromErrorList(node MemberNode);

        //No need to worry about errors in the descriptors because those errors are also sitting in 

        //the node's error list and will be removed.

        Debug.Assert(Not node.Errors == null, "Why try to delete a node which has no errors?");

        m_ParentForm.ErrorWindow.DeleteNodeFromErrorList(node);

        m_ParentForm.UpdateErrorStatus();

    }

    public void DeleteError(node MemberNode, descriptor ContentDescriptor, OldError ErrorRecord);

        Debug.Assert(Not node.Errors == null, "Why try to delete a node which has no errors?");

        m_ParentForm.ErrorWindow.DeleteError(node, descriptor, OldError);

        m_ParentForm.UpdateErrorStatus();

    }

    public void AddToHashtable(member MemberNode);

        //Add this member to the hashtable using its raw signature the key.

        m_Members.Add(member.RawSignature, member);

    }

}

