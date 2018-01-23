//Copyright (C) 2002 Microsoft Corporation

//All rights reserved.

//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 

//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 

//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

public class Customers: System.Collections.CollectionBase
{

	
	// The first thing to do when building a CollectionBase class is

	// to inherit from System.Collections.CollectionBase

	public Customer Add(Customer Value)
	{

		// After you inherit the CollectionBase class, 

		// you can access an intrinsic object

		// called InnerList that represents your 

		// collection. InnerList is of type ArrayList

		this.InnerList.Add(Value);

		return Value;

	}

	public Customer Add(string FirstName, string LastName, string AccountNum) 
	{
		Customer cust = new Customer();

		cust.FirstName = FirstName;

		cust.LastName = LastName;

		cust.AccountNumber = AccountNum;

		// When you use the InnerList.Add method, you must

		// pass it an object to add to the collection. In this

		// case we've created a new Customer object based on

		// passed parameters, and now we're adding it to the

		// InnerList.

		this.InnerList.Add(cust);

		return cust;

	}

	public Customer Item(int Index) 
		{
		// To retrieve an item from the InnerList, pass

		// the index of that item to the .Item property.

		

		return (Customer) this.InnerList[Index];
		
	}

	public Customer Item(Customer cust) 
	{
		// To retrieve an item from the InnerList, pass

		// the index of that item to the .Item property.

		int myIndex;
		
		myIndex = this.InnerList.IndexOf(cust);
		
		return (Customer) this.InnerList[myIndex];
		
	}

	public void Remove(Customer cust)
	{
		// To remove an item from the collection, you must

		// pass in a reference to that item (in this case, the

		// Customer object we want to remove).

		// However, it is often more convenient to create a

		// Remove method that allows the calling code to pass in 

		// only the index of the item instead of an object reference.

		// So we've overloaded the Remove method to use either one.

		this.InnerList.Remove(cust);

	}

	public void Remove(int Index)
{
		// This is the second Remove method. Instead of passing

		// in an object reference, this Remove expects an index.

		// The calling code can decide which one to call.

		// if the calling code passes an index, you can simply

		// look up that item by using the InnerList.Item method,

		// then remove the item.

		Customer cust;
		cust = (Customer) this.Item(Index);
		//cust = (Customer) this.InnerList.Item(Index);

		if (cust != null) {

			this.InnerList.Remove(cust);

		}

	}

}

