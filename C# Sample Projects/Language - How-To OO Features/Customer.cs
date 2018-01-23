//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

public class Customer
{
	private string custAccount;

	private string custFirstName;

	private string custLastName;

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

