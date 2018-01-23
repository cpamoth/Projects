//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

using Microsoft.VisualBasic;

using System;

using System.Diagnostics;

using System.Reflection;

using System.Collections;

//public Module Constants
public class Constants
{

    // A collection of global constants.

    public string const ASSEMBLY_FILE_FILTER  = "Assembly Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

    public const XML_FILE_FILTER string = "XML Documentation Files (*.xml)|*.xml|All files (*.*)|*.*";

    //Indentation for XML nodes.

    public const INDENT1 string = "    ";

    public const INDENT2 string = INDENT1 + INDENT1;

    public const INDENT3 string = INDENT2 + INDENT1;

    public const BAD_MEMBER_NAME string = "{BAD_MEMBER}";

    public const UNKNOWN_PARAM_NAME string = "{UNKNOWN_NAME}";

}

public Module GlobalFlags;

    // A collection of global flags.

    public FileDirty bool = false;

}

public Module SignatureHelpers;

    // A collection of functions for manipulating and parsing raw and friendly

    // signatures.

    public const BAD_SIG string = "{BAD_SIG}"    'Represents a bad signature.;

    public DATA_TYPE_HERE IntrinsicToVBType(Type string) string;

        // Maps the fully-qualified .Net runtime name to the VB .Net friendly

        // name.

        if Type.StartsWith("System.") {  'A simple optimization;

            if Type.StartsWith("System.bool") {

                return Type.Replace("System.bool", "bool");

            }

            if Type.StartsWith("System.Byte") {

                return Type.Replace("System.Byte", "Byte");

            }

            if Type.StartsWith("System.Int16") {

                return Type.Replace("System.Int16", "Short");

            }

            if Type.StartsWith("System.Int32") {

                return Type.Replace("System.Int32", "int");

            }

            if Type.StartsWith("System.Int64") {

                return Type.Replace("System.Int64", "Long");

            }

            if Type.StartsWith("System.Single") {

                return Type.Replace("System.Single", "Single");

            }

            if Type.StartsWith("System.Double") {

                return Type.Replace("System.Double", "Double");

            }

            if Type.StartsWith("System.Decimal") {

                return Type.Replace("System.Decimal", "Decimal");

            }

            if Type.StartsWith("System.Char") {

                return Type.Replace("System.Char", "Char");

            }

            if Type.StartsWith("System.string") {

                return Type.Replace("System.string", "string");

            }

            if Type.StartsWith("System.Object") {

                return Type.Replace("System.Object", "Object");

            }

            if Type.StartsWith("System.DateTime") {

                return Type.Replace("System.DateTime", "Date");

            }

        }

        return Type;

    }

    public DATA_TYPE_HERE UseFriendlyArrayNotation(s string) string;

        //XML Documentation uses square brackets whereas VB uses parentheses for Array notation.

        return s.Replace("["c, "("c).Replace("]"c, ")"c);

    }

    public DATA_TYPE_HERE NormalizeNodePath(s string) string;

        //Paths of treenodes are delimited with "\".  These need to be changed to ".".

        return s.Replace("\"c, "."c);

    }

    public DATA_TYPE_HERE NormalizeTypeDelimiters(s string) string;

        //Nested types are delimited with "+".  These need to be changed to ".".

        return s.Replace("+"c, "."c);

    }

    public DATA_TYPE_HERE NormalizeArrayNotation(s string) string;

        //Array notation from a raw signature will have bound instances.  These

        //need to be removed.

        if s.IndexOf(","c) != -1 AndAlso s.IndexOf("0:") != -1 {

            s = s.Replace("0:", null);

        }

        return s;

    }

    public DATA_TYPE_HERE NormalizeType(s string) string;

        return NormalizeArrayNotation(NormalizeTypeDelimiters(s));

    }

    public DATA_TYPE_HERE SplitParams(s string) string();

        //This function splits on all commas contained in "s" except for commas embedded in square brackets.

        if s == null {

            return null;

        }

        //Use a stack to determine if a comma is embedded in square brackets.

        BracketStack Stack = new Stack();

        FragmentList ArrayList = new ArrayList();

        start int = 0;

        index int;

        For index = 0 To s.Length - 1;

            switch( s.Chars(index);

                case "["c;

                    BracketStack.Push(new Object());

                case "]"c;

                    BracketStack.Pop();

                case ","c;

                    if BracketStack.Count = 0 {

                        FragmentList.Add(s.Substring(start, index - start));

                        start = index + 1;

                    }

            }

        }

        //Don't fail if the last character is a comma.

        if start != index {

            //Special case: attach the last fragment after the last comma encountered.

            FragmentList.Add(s.Substring(start, index - start));

        }

        return CType(FragmentList.ToArray(typeof(string)), string());

    }

    public Structure Signature;

        // This structure represents a raw signature parsed into its

        // constituent parts.

        public Kind string               'The member's kind.;

        public FullName string           'The fully-qualified name.;

        public Params string()           'An array of parameter type signatures.;

        public SpecialEnding string      'The return type prepended with a "~" for certain special cases.;

        public override DATA_TYPE_HERE Tostring() string;

            // Re-assembles the raw signature into one string.

            result string = "";

            if Kind != "" { result += Kind;

            if FullName != "" { result += FullName;

            if ! Params == null AndAlso Params.Length > 0 {

                result += "("c;

                i int;

                For i = 0 To Params.Length - 1;

                    if i != 0 {

                        result += ","c;

                    }

                    result += Params(i);

                }

                result += ")"c;

            }

            if SpecialEnding != "" { result += SpecialEnding;

            return result;

        }

    End Structure;

    public DATA_TYPE_HERE ParseRawSignature(RawSig string) Signature;

        // Given a raw signature, break it up into its constituent parts.

        // This function will either throw exceptions or have undefined

        // behavior if the signature is ! in the correct form;  the caller

        // must handle these cases appropriately.

        Result Signature;

        Result.Kind = Left(RawSig, 2);

        //Strip off the Kind now that we are done with it.

        RawSig = RawSig.Substring(2);

        FirstParen int = RawSig.IndexOf("("c);

        LastParen int = RawSig.LastIndexOf(")"c);

        if FirstParen = -1 {

            //No params found.

            Result.FullName = RawSig;

        else {

            Result.Params = SplitParams(RawSig.Substring(FirstParen + 1, LastParen - FirstParen - 1));

            i int;

            For i = 0 To Result.Params.Length - 1;

                Result.Params(i) = UseRawArrayNotation(Result.Params(i));

            }

            Result.FullName = Left(RawSig, FirstParen);

        }

        //if there are characters after the last paren, they are considered the special ending used

        //for op_Implicit and op_Explicit.

        if LastParen != -1 AndAlso LastParen < RawSig.Length - 1 {

            Result.SpecialEnding = RawSig.Substring(LastParen + 1);

        }

        return Result;

    }

    public DATA_TYPE_HERE UseRawArrayNotation(paramsig string) string;

        // Multi-arrays need to be represented "[0:,0:]" rather than

        // "[,]" for the C# Object Browser to work.  Patch up the raw signature

        // to include the "0:" notation.

        if paramsig.IndexOf(","c) != -1 {

            //First clean the string of all special notation.  This normalizes for the algorithm below.

            paramsig = NormalizeType(paramsig);

            fragments string() = paramsig.Split(","c);

            paramsig = fragments(0);

            i int;

            For i = 1 To fragments.Length - 1;

                //Special case if there are two or more commas next to each other "[,,]"

                //Do a run of "0:" for every directly adjacent comma (signified by empty string from the split).

                if fragments(i) = "" {

                    paramsig += "0:,";

                else {

                    paramsig += "0:,0:" + fragments(i);

                }

            }

        }

        return paramsig;

    }

}

public Module NodeKindHelpers;

    // A collection of helpers for determining and classifying node kinds.

    public DATA_TYPE_HERE IsType(k NodeKind) bool;

        return k = NodeKind.class Orelse { _;

               k = NodeKind.delegate Orelse { _;

               k = NodeKind.enum Orelse { _;

               k = NodeKind.interface Orelse { _;

               k = NodeKind.Module Orelse { _;

               k = NodeKind.Structure;

    }

    public DATA_TYPE_HERE IsField(k NodeKind) bool;

        return k = NodeKind.Field Orelse { _;

               k = NodeKind.EnumMember Orelse { _;

               k = NodeKind.Constant;

    }

    public DATA_TYPE_HERE CanHaveParameters(k NodeKind) bool;

        return k = NodeKind.Method Orelse { _;

               k = NodeKind.Constructor Orelse { _;

               k = NodeKind.Property;

    }

    public DATA_TYPE_HERE ClassifyNodeKind(t System.Type) NodeKind;

        // Given a reflection type object, this function returns the

        // corresponding node kind.

        if t.Isclass {

            if t.IsSubclassOf(typeof(System.Delegate)) {

                return NodeKind.Delegate;

            } else if ( t.IsDefined(typeof(Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute), false) {

                //This type is a VB Module if it has the standard module attribute attached to it.

                return NodeKind.Module;

            else {

                return NodeKind.Class;

            }

        } else if ( t.Isinterface {

            return NodeKind.Interface;

        } else if ( t.Isenum {

            return NodeKind.Enum;

        } else if ( t.IsValueType {

            return NodeKind.Structure;

        }

        //"System.Enum" actually falls through to here, which is a bug.  return NodeKind.class the default.

        Debug.Assert(t == typeof(System.Enum), "Unexpected type: " + t.FullName);

        return NodeKind.Class;

    }

    public DATA_TYPE_HERE ClassifyNodeKind(f FieldInfo) NodeKind;

        // Given a reflection field object, this function returns the

        // corresponding node kind.

        if f.IsLiteral {

            if f.DeclaringType.Isenum {

                return NodeKind.EnumMember;

            else {

                return NodeKind.Constant;

            }

        }

        return NodeKind.Field;

    }

    public DATA_TYPE_HERE ClassifyNodeKind(s string) NodeKind;

        // Given the "kind" component of a raw signature, this function returns

        // the corresponding node kind.

        switch( UCase(s);

            case "N:" : return NodeKind.Namespace;

            case "T:" : return NodeKind.Class;

            case "F:" : return NodeKind.Field;

            case "P:" : return NodeKind.Property;

            case "M:" : return NodeKind.Method;

            case "E:" : return NodeKind.Event;

            case "!:" : return NodeKind.Error;

        }

        return NodeKind.Error;

    }

    public DATA_TYPE_HERE NodeKindRepresentation(k NodeKind) string;

        // Give a node kind, this function returns the "kind" component

        // representation for the raw signature.

        if IsType(k) {

            return "T:";

        }

        if IsField(k) {

            return "F:";

        }

        switch( k;

            case NodeKind.Method, NodeKind.Constructor;

                return "M:";

            case NodeKind.Property;

                return "P:";

            case NodeKind.Namespace;

                return "N:";

            case NodeKind.Event;

                return "E:";

        }

        Debug.Assert(k = NodeKind.Error, "The value of m_Kind should be 'Error': ", k.ToString();

        return "!:";

    }

}

public Module ReflectionHelpers;

    // A collection of helpers used for Reflection.  We care about asking two

    // questions:

    //   1) == a Member visible outside of the assembly?  (protected and Public

    //       members are, private and internal members are not)

    //   2) Does a Member have protected accessiblity?

    // These questions are answered with the helpers below, overloaded for each

    // type of reflection object.

    //Reflect on all members public or private on a type, except those which are inherited.

    public const MemberBindingFlags BindingFlags = _;

        BindingFlags.Instance || _;

        BindingFlags.Static || _;

        BindingFlags.public || _;

        BindingFlags.Nonpublic || _;

        BindingFlags.DeclaredOnly;

    public DATA_TYPE_HERE IsProtected(t Type) bool;

        return t.IsNestedFamily Orelse { t.IsNestedFamORAssem;

    }

    public DATA_TYPE_HERE IsProtected(method MethodBase) bool;

        return method.IsFamily Orelse { method.IsFamilyOrAssembly;

    }

    public DATA_TYPE_HERE IsProtected(field FieldInfo) bool;

        return field.IsFamily Orelse { field.IsFamilyOrAssembly;

    }

    public DATA_TYPE_HERE IsProtected(evt EventInfo) bool;

        //Accessibilty is determined by the accessibily of the hookups.

        return IsProtected(evt.GetAddMethod(true));

    }

    public DATA_TYPE_HERE IsProtected(prop PropertyInfo) bool;

        //Accessibilty is determined by the accessibily of the accessors.

        if prop.CanRead {

            return IsProtected(prop.GetGetMethod(true));

        else {

            return IsProtected(prop.GetSetMethod(true));

        }

    }

    public DATA_TYPE_HERE IsExportable(t Type) bool;

        return t.Ispublic Orelse { t.IsNestedpublic Orelse { IsProtected(t);

    }

    public DATA_TYPE_HERE IsExportable(method MethodBase) bool;

        return method.Ispublic Orelse { IsProtected(method);

    }

    public DATA_TYPE_HERE IsExportable(field FieldInfo) bool;

        return field.Ispublic Orelse { IsProtected(field);

    }

    public DATA_TYPE_HERE IsExportable(evt EventInfo) bool;

        //Accessibilty is determined by the accessibily of the hookups.

        return IsExportable(evt.GetAddMethod(true));

    }

    public DATA_TYPE_HERE IsExportable(prop PropertyInfo) bool;

        //Accessibilty is determined by the accessibily of the accessors.

        if prop.CanRead {

            return IsExportable(prop.GetGetMethod(true));

        else {

            return IsExportable(prop.GetSetMethod(true));

        }

    }

    public DATA_TYPE_HERE IsExportable(m MemberInfo) bool;

        switch( m.MemberType;

            case MemberTypes.Method, MemberTypes.Constructor;

                return IsExportable(CType(m, MethodBase));

            case MemberTypes.Event;

                return IsExportable(CType(m, EventInfo));

            case MemberTypes.Field;

                return IsExportable(CType(m, FieldInfo));

            case MemberTypes.Property;

                return IsExportable(CType(m, PropertyInfo));

            case MemberTypes.NestedType;

                return IsExportable(CType(m, Type));

        }

        Debug.Fail("Unexpected member type: ", m.GetType.ToString();

        return false;

    }

}

