//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

Option Compare Text;

using Microsoft.VisualBasic;

using System;

using System.Reflection;

using System.Diagnostics;

using System.Drawing;

using System.Collections;

using System.Windows.Forms;

public class MemberNode : : TreeNode;

    // This class is a complete representation of a member and a container for

    // its associated XML content.  This class inherits from TreeNode so

    // instances of it can be directly inserted into the Assembly tree.

    // MemberNode instances are constructed from either reflection objects

    // (which come from reflecting against the assembly) or from XML

    // Documentation signatures (read from the XML Documentation file).  Each

    // member node keeps track of its content descriptors through the use of

    // five array lists which store the content descriptors.  Each member also

    // keeps track of its kind, name, path, and associated errors.

    // 

    // Before an error can be reported on a member node, the node must first be

    // added into the Assembly tree.

    //

    // Every member has a Raw signature and a Friendly signature.  The raw

    // signature (described below) is written to the XML Documentation file

    // whereas the Friendly signature (similar to the declaration it would

    // appear in VB source) is the representation displayed to the user in the

    // UI.

    //

    // The raw signature (used in the "name" item of a "member" XML node) has

    // the following structure:

    //     kind full-name ( params ) specialending

    // where:

    //     'kind' is

    //         "N:" for namespace

    //         "T:" for class, structure, enum

    //         "F:" for field

    //         "P:" for property

    //         "M:" for method

    //         "E:" for event

    //         "!:" for error

    //     'full-name' is a dot-delimited, fully-qualified name.  Constructors

    //                 are represented "#ctor" and "#cctor" (shared).

    //     'params'    is a comma-delimited list of type signatures (see class

    //                 ParameterDescriptor for details).  if there are no 

    //                 parameters, there are no parentheses.

    //     'specialending' is the return type prepended with a "~" for certain

    //                 special cases like op_Implicit and op_Explicit members.

    // Examples:

    //     "N:Namespace1"

    //     "T:Namespace1.Class1"

    //     "M:Namespace1.Class1.Foo1(System.Int32,System.Int32@)"

    //     "M:Namespace1.Class1.Foo2"

    //

    // Note: Whitespace is ! allowed in the raw signature.

    private enum PropertyKind;

        [ReadWrite];

        [ReadOnly];

        [WriteOnly];

    }

    private const CTOR_SIG string = "#ctor"              'Raw representation for a constructor.;

    private const SHARED_CTOR_SIG string = "#cctor"      'Raw representation for a shared constructor.;

    public static readonly NormalColor System.Drawing.Color = Color.Black    'Nodes with no errors are black.;

    public static readonly ErrorColor System.Drawing.Color = Color.Red       'Nodes with errors are red.;

    public static readonly NormalFont Font = new Font("Microsoft Sans Serif", 8.25!, FontStyle.Regular) 'Nodes without content are normal.;

    public static readonly EditedFont Font = new Font("Microsoft Sans Serif", 8.25!, FontStyle.Bold)    'Nodes with content are bold.;

    private m_Doc MainDoc                'The core engine instance.;

    private m_Kind NodeKind              'Type of the node.;

    private m_Name string                'Name of the member node.;

    private m_Path string                'The path in the tree hierarchy.;

    private m_ReturnType string          'string representation of the return type.;

    private m_SpecialEnding string       'The special ending if there is one.;

    private m_Errors ArrayList           'List of errors for this treenode.;

    private m_Source NodeSource          'Where this node was created from (Assmembly or XML Documentation file).;

    private m_Isstatic bool           'true if the member is declared with the static keyword.;

    private m_Isprotected bool        'true if the member is declared with protected accessibility.;

    private m_PropertyKind PropertyKind  'Whether the property is ReadOnly, WriteOnly, or neither (if the member node represents a property).;

    public m_Summary ArrayList           '"summary" content descriptors.;

    public m_Remarks ArrayList           '"remarks" content descriptors.;

    public m_Params ArrayList            '"param" parameter descriptors.;

    public m_PropertyValue ArrayList     '"value" content descriptors.;

    public m_ReturnValue ArrayList       '"returns" content descriptors.;

    private void new(kind NodeKind, source NodeSource, doc MainDoc);

        // Standard constructor for all member nodes.  All other constructors

        // must call this one for standard initialization.  This constructor

        // isn't sufficient on its own, however, thus it has private

        // accessibility.

        

        m_Doc = doc;

        m_Kind = kind;

        m_Source = source;

        this.NodeFont = NormalFont;

        this.ForeColor = NormalColor;

        m_Summary = new ArrayList(1)  'every node has (at least) one summary field;

        m_Remarks = new ArrayList(1)  'every node has (at least) one remarks field;

        m_Params = new ArrayList();

        m_PropertyValue = new ArrayList();

        m_ReturnValue = new ArrayList();

        m_Errors = null;

        m_ReturnType = null;

    }

    public void new(kind NodeKind, source NodeSource, name string, path string, doc MainDoc);

        // Builds a node from only the name and path.  Used for namespaces and

        // error nodes.

        MyClass.new(kind, source, doc);

        if kind = NodeKind.Error { this.ForeColor = ErrorColor;

        m_Name = name;

        m_Path = path;

        m_Isstatic = false;

        m_Isprotected = false;

        InitializeVisualElements();

    }

    public void new(t Type, doc MainDoc);

        // Builds a member node from a Type reflection object.

        MyClass.new(ClassifyNodeKind(t), NodeSource.Assembly, doc);

        if t.MemberType = MemberTypes.NestedType {

            //Nested types are delimited by "+", so we need to strip these out and append the parent classes

            //to the namespace for the full path.

            i int = t.FullName.LastIndexOf("+"c);

            m_Path = NormalizeTypeDelimiters(t.FullName.Substring(0, i));

        else {

            m_Path = t.Namespace;

        }

        m_Name = t.Name;

        m_Isprotected = IsProtected(t);

        InitializeVisualElements();

    }

    public void new(ctor ConstructorInfo, path string, doc MainDoc);

        // Builds a member node from a ConstructorInfo reflection object.

        MyClass.new(NodeKind.Constructor, NodeSource.Assembly, doc);

        //Instance constructors have a name of "#ctor" whereas

        //shared constructors have a name of "#cctor".

        if ctor.IsStatic {

            m_Name = SHARED_CTOR_SIG;

            m_Isstatic = true;

        else {

            m_Name = CTOR_SIG;

        }

        m_Path = path;

        AddParams(ctor.GetParameters);

        m_Isprotected = IsProtected(ctor);

        InitializeVisualElements();

    }

    public void new(field FieldInfo, path string, doc MainDoc);

        // Builds a member node from a Field reflection object.

        MyClass.new(ClassifyNodeKind(field), NodeSource.Assembly, doc);

        m_Name = field.Name;

        m_Path = path;

        m_Isstatic = field.IsStatic;

        m_Isprotected = IsProtected(field);

        InitializeVisualElements();

    }

    public void new(method MethodInfo, path string, doc MainDoc);

        // Builds a member node from a Method reflection object.

        MyClass.new(NodeKind.Method, NodeSource.Assembly, doc);

        m_Name = method.Name;

        m_Path = path;

        m_Isstatic = method.IsStatic;

        AddParams(method.GetParameters);

        m_Isprotected = IsProtected(method);

        //Determine the return type.  System.Void is ignored, so leave the return type null.

        if method.ReturnType.FullName != "System.Void" {

            m_ReturnType = NormalizeTypeDelimiters(method.ReturnType.FullName);

        }

        //Handle the special cases for method names.

        switch( method.Name;

            case "op_Implicit";

                m_SpecialEnding = "~" + m_ReturnType;

            case "op_Explicit";

                m_SpecialEnding = "~" + m_ReturnType;

        }

        InitializeVisualElements();

    }

    public void new(prop PropertyInfo, path string, doc MainDoc);

        // Builds a member node from a DATA_TYPE_HERE reflection object.

        MyClass.new(NodeKind.Property, NodeSource.Assembly, doc);

        m_Name = prop.Name;

        m_Path = path;

        //This property is shared if its accessors are shared. 

        if prop.CanRead {

            m_Isstatic = prop.GetGetMethod(true).IsStatic;

        else {

            m_Isstatic = prop.GetSetMethod(true).IsStatic;

        }

        AddParams(prop.GetIndexParameters);

        m_Isprotected = IsProtected(prop);

        m_ReturnType = NormalizeTypeDelimiters(prop.PropertyType.FullName);

        //Determine the kind of property this is.

        if ! prop.CanRead {

            m_PropertyKind = PropertyKind.WriteOnly;

        } else if ( ! prop.CanWrite {

            m_PropertyKind = PropertyKind.ReadOnly;

        else {

            m_PropertyKind = PropertyKind.ReadWrite;

        }

        InitializeVisualElements();

    }

    public void new(ei EventInfo, path string, doc MainDoc);

        // Builds a member node from an Event reflection object.

        MyClass.new(NodeKind.Event, NodeSource.Assembly, doc);

        m_Name = ei.Name;

        m_Path = path;

        m_Isprotected = IsProtected(ei);

        InitializeVisualElements();

    }

    public void new(RawSignature string, doc MainDoc);

        // Builds a member node from a raw signature from an XML Documentation

        // file. This constructor is used if the matching member was ! found

        // within the Assembly file, therefore member nodes constructed in this

        // manner automatically have a XML-only error.

        MyClass.new(ClassifyNodeKind(Left(RawSignature, 2)), NodeSource.XML, doc);

        //A bad raw signature results in bad member.

        if RawSignature = BAD_SIG {

            MakeBad();

            //Before reporting an error, we need to add this new MemberNode to the tree.

            m_Doc.AddToTree(this);

            m_Doc.ReportError(this, ErrorID.MemberNameItemNotValid);

            Return;

        }

        try {

            if m_Kind = NodeKind.Error {  'The node kind has already been set by the standard constructor.;

                throw new Exception();

            }

            //Parse the signature into its constituent parts.

            ParsedSignature Signature = ParseRawSignature(RawSignature);

            FullName string = ParsedSignature.FullName;

            //Build the parameter descriptors if the signature contains parameters.

            //The actual content will be filled in later we continue to parse the XML Documentation file,

            //however the "shell" of the member must be created beforehand.

            if ! ParsedSignature.Params == null {

                paramsig string;

                foreach(paramsig In ParsedSignature.Params;

                    newParameter ParameterDescriptor = new ParameterDescriptor(paramsig);

                    m_Params.Add(newParameter);

                }

            }

            //Handle nested types correctly if they've snuck into the XML Documentation file.

            FullName = NormalizeTypeDelimiters(FullName);

            //The name is everything after the last dot, the path is everything before it.

            LastDot int = FullName.LastIndexOf("."c);

            if LastDot = -1 {

                m_Path = "";

                m_Name = FullName;

            else {

                m_Path = FullName.Substring(0, LastDot);

                m_Name = FullName.Substring(LastDot + 1);

            }

            //if the name is one of the special constructor names, then change this

            //member's kind to Constructor.

            if m_Name = CTOR_SIG Orelse { m_Name = SHARED_CTOR_SIG {

                m_Kind = NodeKind.Constructor;

                if m_Name = SHARED_CTOR_SIG {

                    m_Isstatic = true;

                }

            }

            m_SpecialEnding = ParsedSignature.SpecialEnding;

            //This kind of information is ! stored in the raw signature, so just set them to false.

            m_Isstatic = false;

            m_Isprotected = false;

            //The member node is now constructed enough that we can setup its visual elements.

            InitializeVisualElements();

            //This is an XML-only node, so report it such, but before reporting

            //an error, we need to add this new MemberNode to the tree.

            m_Doc.AddToTree(this);

            m_Doc.AddToHashtable(this);

            m_Doc.ReportError(this, ErrorID.NodeInXMLOnly);

       } catch( e Exception;

            //Any exception thrown during construction indicates a bad member.

            MakeBad();

            InitializeVisualElements();

            //Before reporting an error, we need to add this new MemberNode to the tree

            m_Doc.AddToTree(this);

            m_Doc.ReportError(this, ErrorID.BadSignatureInMemberNode1, RawSignature);

        }

    }

    public () {

        // Builds an empty node.  This is necessary for cloning.

        

    }

    private static BadMemberCounter int = 1  'Used to create a unique name for bad members.;

    private void MakeBad();

        m_Kind = NodeKind.Error;

        m_Path = "";

        m_Name = BAD_MEMBER_NAME + " " + CStr(BadMemberCounter)  'The counter makes sure the name is unique.;

        BadMemberCounter += 1;

    }

    private void InitializeVisualElements();

        // Initialize the fields which determine how the member node is

        // rendered on the screen.  The friendly signature is the text which

        // appears in the Assembly tree.  Additionally, the correct icon

        // needs to be selected based on the kind of member node and whether it

        // has protected accessibility.

        this.Text = this.FriendlySignature;

        //Determine the appropriate icon for the tree node.

        index int = MapToImageIndex(m_Kind, m_IsProtected);

        this.ImageIndex = index;

        this.SelectedImageIndex = index;

    }

    public readonly DATA_TYPE_HERE Source() NodeSource;

        get {

            return m_Source;

        }

    }

    public readonly DATA_TYPE_HERE Path() string;

        get {

            return m_Path;

        }

    }

    public readonly DATA_TYPE_HERE Name() string;

        get {

            return m_Name;

        }

    }

    public DATA_TYPE_HERE HasContent() bool;

        // Returns true if this member node has any non-empty content

        // descriptors, false otherwise.

        CurrentDescriptor ContentDescriptor;

        foreach(CurrentDescriptor In m_Summary;

            if CurrentDescriptor.Content != "" {

                return true;

            }

        }

        foreach(CurrentDescriptor In m_Params;

            if CurrentDescriptor.Content != "" {

                return true;

            }

        }

        foreach(CurrentDescriptor In m_PropertyValue;

            if CurrentDescriptor.Content != "" {

                return true;

            }

        }

        foreach(CurrentDescriptor In m_ReturnValue;

            if CurrentDescriptor.Content != "" {

                return true;

            }

        }

        foreach(CurrentDescriptor In m_Remarks;

            if CurrentDescriptor.Content != "" {

                return true;

            }

        }

        return false;

    }

    public void UpdateEditState();

        // Member nodes with content are bold, otherwise they have a normal

        // font style.  This function is called when a modification has been

        // made to the content of a member node and its font weight needs to be

        // adjusted.

        if this.HasContent() {

            this.NodeFont = EditedFont;

            FileDirty = true;

        else {

            this.NodeFont = NormalFont;

        }

    }

    public readonly DATA_TYPE_HERE Errors() ArrayList;

        get {

            if this.HasErrors {

                //Make a copy of the error list to avoid others from directly modifying the list.

                //Modifications should only be performed by members of this class.

                return CType(m_Errors.Clone, ArrayList);

            else {

                return null;

            }

        }

    }

    public void AddError(newError ErrorRecord);

        Debug.Assert(Not newError == null, "Must have a valid ErrorRecord to add.");

        //Most member nodes have no errors, so the error list gets created only when

        //an error gets added.

        if m_Errors == null {

            m_Errors = new ArrayList(1);

        }

        m_Errors.Add(newError);

        this.ForeColor = ErrorColor;

    }

    public void RemoveError(OldError ErrorRecord);

        Debug.Assert(Not OldError == null, "Must have a valid ErrorRecord to remove.");

        Debug.Assert(Not m_Errors == null AndAlso m_Errors.Contains(OldError), _;

                     "Attempting to remove an error which is ! in the list.");

        m_Errors.Remove(OldError);

        //Destroy the error list if there are no more errors.

        if m_Errors.Count = 0 {

            m_Errors = null;

            this.ForeColor = NormalColor;

        }

    }

    public void RemoveAllErrors();

        Debug.Assert(Not m_Errors == null, "Attempting to remove errors where no errors exist.");

        m_Errors.Clear();

        m_Errors = null;

        this.ForeColor = NormalColor;

    }

    public DATA_TYPE_HERE HasErrors() bool;

        Debug.Assert(m_Errors == null Orelse { m_Errors.Count > 0, _;

                     "List must have more than 0 items if it is allocated.");

        return ! m_Errors == null;

    }

    private DATA_TYPE_HERE AppendModifiers(sig string) string;

        // Decorates the input with the appropriate VB keywords.

        if m_Isstatic {

            sig = "static " + sig;

        }

        if m_PropertyKind = PropertyKind.ReadOnly {

            sig = "ReadOnly " + sig;

        } else if ( m_PropertyKind = PropertyKind.WriteOnly {

            sig = "WriteOnly " + sig;

        }

        return sig;

    }

    public readonly DATA_TYPE_HERE FriendlyKind() string;

        get {

            return m_Kind.ToString();

        }

    }

    public readonly DATA_TYPE_HERE FriendlySignature() string;

        // Builds the friendly signature for this member node.  The friendly

        // signature is what the VB declaration would look like.  This is used

        // for the node's text displayed in the Assembly tree.

        get {

            sig string = "";

            if m_Kind = NodeKind.Constructor {

                sig += "new";

            else {

                sig += m_Name;

            }

            if CanHaveParameters(m_Kind) {

                UseParens bool = m_Kind != NodeKind.DATA_TYPE_HERE Orelse { m_Params.Count > 0;

                if UseParens {

                    sig += "(";

                }

                first bool = true;

                CurrentParameter ParameterDescriptor;

                foreach(CurrentParameter In m_Params;

                    if ! first {

                        sig += ", ";

                    else {

                        first = false;

                    }

                    sig += CurrentParameter.FriendlyType;

                }

                if UseParens {

                    sig += ")";

                }

                sig += this.FriendlyReturnType;

            }

            return sig;

        }

    }

    public readonly DATA_TYPE_HERE FriendlySignatureWithPath() string;

        // Builds a friendly signature and prepends the path.

        get {

            sig string = this.FriendlySignature;

            if m_Path != "" {

                sig = m_Path + "." + sig;

            }

            return sig;

        }

    }

    public readonly DATA_TYPE_HERE FriendlySignatureWithPathAndModifiers() string;

        // Full friendly signature with modifiers.  Used for the titles of node

        // windows.

        get {

            return AppendModifiers(this.FriendlySignatureWithPath);

        }

    }

    public readonly DATA_TYPE_HERE FriendlyReturnType() string;

        // Builds the friendly representation of the return type (using

        // VB-style array notation and type names).

        get {

            ret string = "";

            if m_ReturnType != "" {

                ret += " " + UseFriendlyArrayNotation(IntrinsicToVBType(m_ReturnType));

            }

            return ret;

        }

    }

    public readonly DATA_TYPE_HERE RawSignature() string;

        // Builds the raw signature for the member node.  This signature is

        // written to the XML Documentation file.

        get {

            sig System.Text.stringBuilder = new System.Text.stringBuilder();

            sig.Append(NodeKindRepresentation(m_Kind));

            if m_Path != "" {

                sig.Append(m_Path + ".");

            }

            sig.Append(m_Name);

            //Parameter parentheses will have to be omitted if there are no parameters.

            if CanHaveParameters(m_Kind) AndAlso m_Params.Count > 0 {

                first bool = true;

                CurrentParameter ParameterDescriptor;

                foreach(CurrentParameter In m_Params;

                    if first {

                        sig.Append("("c);

                        first = false;

                    else {

                        sig.Append(","c);

                    }

                    sig.Append(CurrentParameter.RawSignature);

                }

                sig.Append(")"c);

            }

            if m_SpecialEnding != "" {

                sig.Append(m_SpecialEnding);

            }

            return sig.ToString();

        }

    }

    public DATA_TYPE_HERE GenerateXML() string;

        // Builds the complete XML representation for this member node, using

        // the raw signature and the content descriptors, in this format:

        //

        //                 <member name="the raw signature">

        // optional nodes:

        //                     <summary>content</summary>

        //                     <param name="the parameter name">content</param>

        //                     <value>content</value>

        //                     <returns>content</returns>

        //                     <remarks>content</remarks>

        //                 </member>

        //if the node has no content to write, or it's a bad member, then ignore it.

        if ! this.HasContent() {

            return null;

        }

        n System.Text.stringBuilder = new System.Text.stringBuilder();

        n.Append(INDENT2 + "<member name=" + ControlChars.Quote + this.RawSignature + ControlChars.Quote + ">" + Environment.NewLine);

        CurrentDescriptor ContentDescriptor;

        foreach(CurrentDescriptor In m_Summary;

            if CurrentDescriptor.Content != "" {

                n.Append(INDENT3 + "<summary>" + CurrentDescriptor.Content + "</summary>" + Environment.NewLine);

            }

        }

        CurrentParameter ParameterDescriptor;

        foreach(CurrentParameter In m_Params;

            if CurrentParameter.Content != "" {

                n.Append(INDENT3 + "<param name=""" + CurrentParameter.Name + """>" + CurrentParameter.Content + "</param>" + Environment.NewLine);

            }

        }

        foreach(CurrentDescriptor In m_PropertyValue;

            if CurrentDescriptor.Content != "" {

                n.Append(INDENT3 + "<value>" + CurrentDescriptor.Content + "</value>" + Environment.NewLine);

            }

        }

        foreach(CurrentDescriptor In m_ReturnValue;

            if CurrentDescriptor.Content != "" {

                n.Append(INDENT3 + "<returns>" + CurrentDescriptor.Content + "</returns>" + Environment.NewLine);

            }

        }

        foreach(CurrentDescriptor In m_Remarks;

            if CurrentDescriptor.Content != "" {

                n.Append(INDENT3 + "<remarks>" + CurrentDescriptor.Content + "</remarks>" + Environment.NewLine);

            }

        }

        n.Append(INDENT2 + "</member>" + Environment.NewLine);

        return n.ToString();

    }

    public void PopulateFromXML(node Xml.XmlNode);

        // Given an XML node from the XML Documentation file, fill-in the

        // member node with the content inside the XML node.  Depending on the

        // member node's kind, the XML node may have errors (for example,

        // multiple summary fields or param fields for a namespace).  This

        // function detects these errors and reports them.

        el Xml.XmlNode;

        //Build a list of params on the member node so that once we match content to a parameter,

        //we can remove that parameter from further consideration.

        ParamsToSearch ArrayList;

        ParamsToSearch = CType(m_Params.Clone(), ArrayList);

        foreach(el In node.ChildNodes;

            content string = el.InnerXml.Trim();

            //We only care if the XML field contains any content.

            if content != "" {

                //Depending on the XML field kind, we will build the appropriate content descriptor,

                //and detect any error conditions.

                switch( el.Name;

                    case "summary";

                        newSummary ContentDescriptor = new ContentDescriptor(content);

                        m_Summary.Add(newSummary);

                        if m_Summary.Count > 1 {

                            m_Doc.ReportError(this, newSummary, ErrorID.DuplicateField1, "summary");

                        }

                    case "remarks";

                        newRemarks ContentDescriptor = new ContentDescriptor(content);

                        m_Remarks.Add(newRemarks);

                        if m_Remarks.Count > 1 {

                            m_Doc.ReportError(this, newRemarks, ErrorID.DuplicateField1, "remarks");

                        }

                    case "value";

                        newValue ContentDescriptor = new ContentDescriptor(content);

                        m_PropertyValue.Add(newValue);

                        //Only Properties can have "value" fields.

                        if m_Kind != NodeKind.DATA_TYPE_HERE {

                            m_Doc.ReportError(this, newValue, ErrorID.UnexpectedField2, "value", "properties");

                        }

                        if m_PropertyValue.Count > 1 {

                            m_Doc.ReportError(this, newValue, ErrorID.DuplicateField1, "value");

                        }

                    case "returns";

                        newReturnValue ContentDescriptor = new ContentDescriptor(content);

                        m_ReturnValue.Add(newReturnValue);

                        //Only Methods can have "returns" fields.

                        if m_Kind != NodeKind.Method {

                            m_Doc.ReportError(this, newReturnValue, ErrorID.UnexpectedField2, "returns", "methods");

                        }

                        if m_ReturnValue.Count > 1 {

                            m_Doc.ReportError(this, newReturnValue, ErrorID.DuplicateField1, "returns");

                        }

                    case "param";

                        paramName string = "";

                        try {

                            paramName = el.Attributes.GetNamedItem("name").Value().Trim;

                        Catch;

                            //No "name" item means this is a bad parameter.

                            BadParam ParameterDescriptor = new ParameterDescriptor(UNKNOWN_PARAM_NAME, content);

                            m_Params.Add(BadParam);

                            m_Doc.ReportError(this, BadParam, ErrorID.ParamNameItemNotValid1, m_Name);

                            GoTo continue   'VB wish:  add a Continue keyword.;

                        }

                        CurrentParam ParameterDescriptor = null;

                        //Search the member node's paramters looking for a match.

                        foreach(CurrentParam In ParamsToSearch;

                            if CurrentParam.Name = UNKNOWN_PARAM_NAME {

                                //We found a blank name, so match it with this param node and fill in the name and content.

                                //This is useful for XML-only nodes because the param name is ! encoded in the 

                                //raw signature.

                                CurrentParam.Name = paramName;

                                CurrentParam.Content = content;

                                ParamsToSearch.Remove(CurrentParam);

                                break;

                            }

                            if CurrentParam.Name = paramName {

                                //The names match, so fill in the content.

                                CurrentParam.Content = content;

                                ParamsToSearch.Remove(CurrentParam);

                                break;

                            }

                        }

                        if CurrentParam == null Orelse { paramName = UNKNOWN_PARAM_NAME {

                            CurrentParam = new ParameterDescriptor(paramName, content);

                            m_Params.Add(CurrentParam);

                            m_Doc.ReportError(this, CurrentParam, ErrorID.ParamNotFound2, paramName, m_Name);

                        }

                        if ! CanHaveParameters(m_Kind) {

                            m_Doc.ReportError(this, CurrentParam, ErrorID.UnexpectedField2, "param", "methods and properties");

                        }

                    default: {

                        m_Doc.ReportError(this, ErrorID.UnknownField2, el.Name, el.InnerText);

                }

            }

continue:;

        }

        //The member node's content may have changed, so update its visual state.

        UpdateEditState();

    }

    public void EnsureDefaultDescriptors();

        // Member nodes do ! have content descriptors allocated by default to

        // save memory.  Only when the user adds content do the descriptors get

        // created.  However it is very useful, during drag-and-drop for

        // example, to allocated the default set of descriptors.  This makes

        // comparisons between nodes easier.

        //All nodes have one summary field.

        if m_Summary.Count = 0 {

            m_Summary.Add(new ContentDescriptor());

        }

        //All nodes have one remarks field.

        if m_Remarks.Count = 0 {

            m_Remarks.Add(new ContentDescriptor());

        }

        //if this is a property, then it has a property value field.

        if m_Kind = NodeKind.DATA_TYPE_HERE AndAlso m_PropertyValue.Count = 0 {

            m_PropertyValue.Add(new ContentDescriptor());

        }

        //if this is a method with a return type, then it has a return value field.

        if m_Kind = NodeKind.Method AndAlso m_ReturnType != "" AndAlso m_ReturnValue.Count = 0 {

            m_ReturnValue.Add(new ContentDescriptor());

        }

        //Param fields are built only during node construction and populating from the XML file,

        //so there is nothing to do here.

    }

    public static DATA_TYPE_HERE ContentCount(list ArrayList) int;

        // When comparing nodes for drag-and-drop, the count of content

        // descriptor objects is ! useful.  The last index of the content

        // descriptor with actual content in it is useful because this

        // number represents how many content descriptors will actually need to

        // be copied.

        maxindex int = -1;

        i int;

        For i = 0 To list.Count - 1;

            if CType(list(i), ContentDescriptor).Content != "" {

                maxindex = i;

            }

        }

        return maxindex + 1;

    }

    public static DATA_TYPE_HERE ClassifyDifference(source MemberNode, dest MemberNode) Difference;

        // Determine the difference between the source and destination member

        // nodes.  This tells us if a copy can succeed.  Before doing any work,

        // however, it is much easier if we first ensure that all the default

        // descriptors are allocated.

        source.EnsureDefaultDescriptors();

        dest.EnsureDefaultDescriptors();

        //Determine if there are enough slots in the destination to hold all of the content in the source.

        //First, count how much content is in the source.  if this number is less than or equal to the number

        //of descriptors sitting in the destination, then the copy can succeed.

        //We count the content from the source because empty content ("") will ! get copied and should not

        //disqualify a copy from occuring.

        if ContentCount(source.m_Summary) > dest.m_Summary.Count {

            return Difference.SummaryNum;

        } else if ( ContentCount(source.m_Remarks) > dest.m_Remarks.Count {

            return Difference.RemarksNum;

        } else if ( ContentCount(source.m_ReturnValue) > dest.m_ReturnValue.Count {

            return Difference.ReturnNum;

        } else if ( ContentCount(source.m_PropertyValue) > dest.m_PropertyValue.Count {

            return Difference.PropertyValueNum;

        } else if ( ContentCount(source.m_Params) > dest.m_Params.Count {

            return Difference.ParamNum;

        }

        return Difference.None;

    }

    public void Copy(source MemberNode);

        // Copy the source's content descriptors to this node.

        dest MemberNode = Me  'For readability.;

        CopyDescriptors(source.m_Summary, dest.m_Summary);

        CopyDescriptors(source.m_Params, dest.m_Params);

        CopyDescriptors(source.m_ReturnValue, dest.m_ReturnValue);

        CopyDescriptors(source.m_Remarks, dest.m_Remarks);

        CopyDescriptors(source.m_PropertyValue, dest.m_PropertyValue);

        dest.UpdateEditState();

    }

    private static void CopyDescriptors(sourceList ArrayList, destList ArrayList);

        // Copy the content fields of each content descriptor in the source

        // list to the destination list.

        Debug.Assert(ContentCount(sourceList) <= destList.Count, _;

                     "lists should be of similar size, guaranteed by ClassifyDifference");

        i int;

        For i = 0 To ContentCount(sourceList) - 1;

            source ContentDescriptor = CType(sourceList(i), ContentDescriptor);

            dest ContentDescriptor = CType(destList(i), ContentDescriptor);

            //Don't overwrite existing content with non-existant content.

            if source.Content != "" {

                dest.Content = source.Content;

            }

        }

    }

    private void AddParams(params() ParameterInfo);

        // Create a parameter descriptor out of each parameter reflection

        // object.

        param ParameterInfo;

        foreach(param In params;

            m_Params.Add(new ParameterDescriptor(param));

        }

    }

    private static DATA_TYPE_HERE MapToImageIndex(kind NodeKind, isprotected bool) int;

        // Looks up the treenode icon based on the node kind and accessibility.

        // Returns an icon index into the imagestrip resource.

        indexKind int;

        switch( kind;

            case NodeKind.Class;

                indexKind = VB_IMG_CLASS;

            case NodeKind.Delegate;

                indexKind = VB_IMG_DELEGATE;

            case NodeKind.Module;

                indexKind = VB_IMG_MODULE;

            case NodeKind.Enum;

                indexKind = VB_IMG_ENUM;

            case NodeKind.EnumMember;

                indexKind = VB_IMG_ENUMMEMBER;

            case NodeKind.Event;

                indexKind = VB_IMG_EVENT;

            case NodeKind.Field;

                indexKind = VB_IMG_FIELD;

            case NodeKind.Constant;

                indexKind = VB_IMG_CONSTANT;

            case NodeKind.Interface;

                indexKind = VB_IMG_INTERFACE;

            case NodeKind.Method, NodeKind.Constructor;

                indexKind = VB_IMG_METHOD;

            case NodeKind.Namespace;

                indexKind = VB_IMG_NAMESPACE;

            case NodeKind.Property;

                indexKind = VB_IMG_PROPERTY;

            case NodeKind.Structure;

                indexKind = VB_IMG_STRUCT;

            default: {

                Debug.Assert(kind = NodeKind.Error, "Unexpected node kind: ", kind.ToString();

                indexKind = VB_IMG_ERROR;

        }

        //All nodes are either public or protected accessibility.

        indexAccess int;

        if isprotected {

            indexAccess = VB_IMG_ACC_PROTECTED;

        else {

            indexAccess = VB_IMG_ACC_PUBLIC;

        }

        return indexKind + indexAccess;

    }

    public override DATA_TYPE_HERE Clone() Object;

        // Clone this node.  This will be used when copy/pasting nodes.

        //Clone base

        copy new MemberNode();

        copy = CType(base.Clone(), MemberNode);

        //Clone data

        copy.m_Kind = m_Kind;

        copy.m_Name = m_Name;

        copy.m_Path = m_Path;

        copy.m_ReturnType = m_ReturnType;

        copy.m_SpecialEnding = m_SpecialEnding;

        copy.m_Errors = this.Errors;

        copy.m_Source = m_Source;

        copy.m_Isstatic = m_Isstatic;

        copy.m_Isprotected = m_IsProtected;

        copy.m_PropertyKind = m_PropertyKind;

        //Clone Summary field

        n int;

        content ContentDescriptor;

        copy.m_Summary = new ArrayList(m_Summary.Count);

        For n = 0 To m_Summary.Count - 1;

            content = new ContentDescriptor(CType(m_Summary(n), ContentDescriptor).Content);

            copy.m_Summary.Add(content);

        }

        //Clone Remarks field

        copy.m_Remarks = new ArrayList(m_Remarks.Count);

        For n = 0 To m_Remarks.Count - 1;

            content = new ContentDescriptor(CType(m_Remarks(n), ContentDescriptor).Content);

            copy.m_Remarks.Add(content);

        }

        //Clone Params fields

        param ParameterDescriptor;

        source ParameterDescriptor;

        copy.m_Params = new ArrayList(m_Params.Count);

        For n = 0 To m_Params.Count - 1;

            source = CType(m_Params(n), ParameterDescriptor);

            copy.m_Params.Add(source.Clone);

        }

        //Clone PropertyValue field

        copy.m_PropertyValue = new ArrayList(m_PropertyValue.Count);

        For n = 0 To m_PropertyValue.Count - 1;

            content = new ContentDescriptor(CType(m_PropertyValue(n), ContentDescriptor).Content);

            copy.m_PropertyValue.Add(content);

        }

        //Clone ReturnValue field

        copy.m_ReturnValue = new ArrayList(m_ReturnValue.Count);

        For n = 0 To m_ReturnValue.Count - 1;

            content = new ContentDescriptor(CType(m_ReturnValue(n), ContentDescriptor).Content);

            copy.m_ReturnValue.Add(content);

        }

        return copy;

    }

}

public enum Difference;

    //Types of differences between two member nodes.

    None;

    ParamNum;

    SummaryNum;

    RemarksNum;

    ReturnNum;

    PropertyValueNum;

}

public enum NodeSource;

    //The source from where the member node was created.

    [XML];

    [Assembly];

}

public enum NodeKind;

    //The member node's kind.

    [Namespace];

    [Class];

    [Delegate];

    [Interface];

    [Module];

    [Structure];

    [Enum];

    [Field];

    [EnumMember];

    [Constant];

    [Method];

    [Constructor];

    [Property];

    [Event];

    [Error];

}

