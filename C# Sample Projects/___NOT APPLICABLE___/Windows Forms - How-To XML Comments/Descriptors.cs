//*****************************************************************************
// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.
//*****************************************************************************

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

public class ContentDescriptor
{

    // A container for content and its associated error(s).
    // See the MmberNode class for more details.

    private string m_Content;     //The content which is contained by this class.;
    private ArrayList m_Errors;   //List of errors associated with the content.;

    public ContentDescriptor(string Content)
		{
        this.Content = Content;

		}

    public ContentDescriptor ():this("") {

    }

    public string Content
	{

        get {

            return m_Content;

        }

        set {

            m_Content = value;

        }

    }

    public ArrayList Errors
	{
        get {
			if (this.HasErrors) 
			{

				//Make a copy of the error list to avoid others from directly modifying the list.
				//Modifications should only be performed by members of this class.

				return (ArrayList)(m_Errors.Clone);
			}
			else 
			{
				return null;
			}
        }
    }

    public void AddError(ErrorRecord newError)
	{

        Debug.Assert(newError != null, "Must have a valid ErrorRecord to add.");

        //Most descriptors have no errors, so the error list gets created only when
        //an error gets added.

        if (m_Errors == null) 
		{
            m_Errors = new ArrayList(1);
        }

        m_Errors.Add(newError);

    }

    public void RemoveError(ErrorRecord OldError)
	{

        Debug.Assert(OldError != null, "Must have a valid ErrorRecord to remove.");

        Debug.Assert((m_Errors != null) && (m_Errors.Contains(OldError)), 
                     "Attempting to remove an error which is ! in the list.");

        m_Errors.Remove(OldError);

        //Destroy the error list if there are no more errors.

        if (m_Errors.Count = 0) 
			{
            m_Errors = null;
        }
    }

    public bool HasErrors() 
	{

        Debug.Assert((m_Errors == null) || (m_Errors.Count > 0),
                     "List must have more than 0 items if it is allocated.");

        return m_Errors != null;

    }

}

public class ParameterDescriptor: ContentDescriptor
{

    // A container for a parameter's content, associated error(s), and
    // modifiers like ref and Optional.  It also renders the signature for 
    // the parameter using these modifiers.
    // See the MemberNode class for more details.

    private string m_Name;				//The name of the parameter.;
    private string m_NormalizedType;	//A standard representation of the parameter's type.;
    private bool m_IsByRef;				//Has a value of true if the parameter is ByRef.;
    private bool m_IsOptional;			//Has a value of true if the parameter is Optional.;
    private bool m_IsParamArray;		//Has a value of true if the parameter is a ParamArray.;

    public ParameterDescriptor(ParameterInfo Info):base("")
	{

        // Takes a parameter reflection object and builds a descriptor from it.
        // The content is ! known at this point.

        this.Name = Info.Name;

        m_IsOptional = Info.IsOptional;
        Type TypeInfo = Info.ParameterType;

        if (TypeInfo.IsByRef) 
			{
            m_IsByRef = true;
            //Dig through the ref identifier.
            TypeInfo = TypeInfo.GetElementType;
        }

        //This parameter is a ParamArray if it has the ParamArray custom attribute attached to it.
        if (Info.IsDefined(typeof(System.ParamArrayAttribute), false)) {

            m_IsParamArray = true;

        }

        //The normalized type is a standard representation of the string from which we build all 
        //required type-signature forms (such the form displayed to the user (friendly) and 
        //the form written to the XML Documentation file (raw)).  Reflection delimits nested types with
        //"+".  These must be changed to "." to normalize the type.

        m_NormalizedType = NormalizeType(TypeInfo.FullName);

    }

    public ParameterDescriptor(string TypeSig):base("")
	{

        // Takes an XML Documentation type signature and builds a descriptor from it.
        // The content is ! known at this point.
        //
        // XML Documentation type signature format:
        //     typename [arrayrank] [@]
        // where
        //     'typename' is the name of the type
        //     'arrayrank' is an optional description of the array dimensions
        //     '@' is an optional postfix signifying ByRef
        //
        // 'arrayrank' is comprised of zero or more (but never one) instances
        // of the string "0:" separated by commas with the whole encased in
        // square brackets "[ ]".
        //     Examples:  [0:,0:]
        //                []
        //                [][0:,0:,0:][][0:,0:]
        //
        // See MSDN for more information.

        //We don't know the name yet because we are building this param from the XML Documentation file.
		//set {it to a special string so we know to set the names sequentially when we populate the
        //rest of the data from the XML Documentation file.

        this.Name = UNKNOWN_PARAM_NAME;

        if (TypeSig.EndsWith("@")) 
			{
            //The @-sign denotes a ref parameter.
            m_IsByRef = true;
            //Remove the @-sign at the end.
            TypeSig = TypeSig.Remove(TypeSig.Length - 1, 1);
        }

        //The normalized type is a standard representation of the string from which we build all 
        //required type-signature forms (such the form displayed to the user (Friendly) and 
        //the form written to the XML Documentation file (Raw)).  Array notation from the
        //XML Documentation file if considered Raw and in the form 'arrayrank' (see above).
        //Instances of "0:" must be removed to normalize the type.

        m_NormalizedType = NormalizeType(TypeSig);

    }

    public ParameterDescriptor(string Name, string Content):base(Content)
	{

        // Build a descriptor using just the name and content.  This is done
        // when all we can know about a parameter are these values, such as
        // when a bad parameter is detected or a param node is found in the XML
        // without a corresponding entry in the member signature.

        this.Name = Name;

    }

    public string Name
	{
        get {
            return m_Name;
        }
        set {
            m_Name = value;
        }
    }

    public string RawSignature
	{

        // Builds a signature it would appear in the XML Documentation file.

        get {
            string sig = "";
            if (m_NormalizedType != "") 
				{
                sig += m_NormalizedType;
                if (m_IsByRef) 
					{
                    //The @-sign denotes a ref parameter.
                    sig += "@";
                }

                //This adds the "0:" notation if needed.

                sig = UseRawArrayNotation(sig);

            }

            return sig;

        }

    }

    public string FriendlySignature
	{

        // Builds a signature it would appear to the user.

        get {

            string sig = "";

			if (m_IsOptional) { sig += "Optional ";}

			if (m_IsByRef) 
			{
				sig += "ByRef ";
			}
			else 
			{
				sig += "";
			}

			if (m_IsParamArray) 
			{
				sig += "ParamArray ";
				sig += this.Name;
			}
			
			if (m_NormalizedType != "") 
					{

						sig += " " + this.FriendlyType;

					}

					return sig;

				}

			}

    public string FriendlyType
	{

        // Builds a type-signature it would appear to the user.

        get {

            string sig = "";

            if (m_NormalizedType != "") 
			{
                sig += UseFriendlyArrayNotation(IntrinsicToVBType(m_NormalizedType));

            }

            return sig;

        }

    }

    public ParameterDescriptor Clone()
	{

        // Clone this descriptor.  This will be used when copy/pasting nodes.

        ParameterDescriptor copy = new ParameterDescriptor(this.Name, this.Content);
        copy.m_IsByRef = this.m_IsByRef;
        copy.m_IsOptional = this.m_IsOptional;
        copy.m_IsParamArray = this.m_IsParamArray;
        copy.m_NormalizedType = this.m_NormalizedType;

        return copy;

    }

}

