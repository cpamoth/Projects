//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

public class CustomerPropertySyntax
{
	private string custAccount;
	private string custFirstName;
	private string custLastName;

	public CustomerPropertySyntax(string AccountNumber)
{

		// This is the Constructor for this class.
		// Normally, you would use the AccountNumber string to search a
		// database or collection for the customer's account number.
		// Here, we're just going to populate a Customer object with
		// dummy data.
		//
		// Because the account number is a readonly property,
		// we need to set the private custAccount variable
		// directly rather than calling the property procedure.
		custAccount = "1101";
		this.FirstName = "Carmen";
		this.LastName = "Smith";
	}

	// The following line is the property declaration
	// for the AccountNumber property. This line is
	// referred to the "property statement".

	public string AccountNumber 
		{
		// Most properties are read/write, which means they
		// can be evaluated or changed by the calling code.
		// When a property is read/write you have both a
		// Get portion and a set {portion of code within
		// the property code. The get {and set {portions of
		// the property are referred to the "property
		// procedures".
		// Because this property is flagged ReadOnly, there
		// is only a get {property procedure with no matching 
		// "Set" property procedure.
		// Alternatively you can have a property that is
		// write only by declaring the property "WriteOnly"
		// instead of ReadOnly. In that case you would have
		// a set {with no Get.
		// Note: A property's data type and access are defined 
		// in the DATA_TYPE_HERE statement, not in the DATA_TYPE_HERE 
		// procedures. A property can have only one data type 
		// and one accessibility. For example, you can! define 
		// a property to set a Decimal value, but get a double 
		// value. Similarly, you can! define a private set {and 
		// a public get {on the same property. To achieve this 
		// functionality, you can define a public readonly 
		// property and a private setting method separate from 
		// the property.

		get {
			return custAccount;
		}
	}

#region "Standard Customer Properties";

	public string FirstName 
	{
		get {
			return custFirstName;
		}
		set {
			custFirstName = value;
		}
	}

	public string LastName
	{
		get {
			return custLastName;
		}
		set {
			custLastName = value;
		}
	}

#endregion

}

