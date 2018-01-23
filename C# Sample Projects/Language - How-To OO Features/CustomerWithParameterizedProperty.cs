//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

public class CustomerWithParameterizedProperty
{
	private string custAccount;
	private string custFirstName;
	private string custLastName;
	private int custDefaultQuantity = 12;

	// To create a parameterized property, simply add a parameter to the
	// property statement, in this case it is "Multiplier":
	//TODO: Could not find a solution to this problem of Property with Parameter.
	/*public int DefaultQuantity(int Multiplier)
	{
		get {

			// Normally a DATA_TYPE_HERE get {procedure doesn't use incoming
			// parameters, but in the case of a parameterized property
			// it does. The parameter is treated just like a normal
			// passed parameter:

			return custDefaultQuantity * Multiplier;
		}
		set{
			custDefaultQuantity = value;
		}
	}*/

#region "Customer Properties."

	public string AccountNumber
	{
		get {
			return custAccount;
		}
		set {
			custAccount = value;
		}
	}

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

