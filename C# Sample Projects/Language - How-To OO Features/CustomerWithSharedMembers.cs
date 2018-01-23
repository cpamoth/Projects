//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;


public class CustomerWithstaticMembers
{
	private string custAccount;
	private string custFirstName;
	private string custLastName;

	// The following variable (CompanyName) is declared as
	// static. All instances of this class will use the same
	// instance of this variable. if it is changed in any
	// instance of this class, it will be changed in all
	// instances.

	public static string CompanyName;

	public static DateTime LastOrderDate()
	{
		// This is a shared function. Refer to the calling
		// code in frmMain for an explanation of how to 
		// invoke a procedure declared static.

		DateTime orderDate = DateTime.Now;

		// Normally you would look up in a database or
		// collection the date of the last order here.
		// For brevity we'll return a dummy date.

		return orderDate;
	}

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

