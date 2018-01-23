//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.EnterpriseServices;

// These attributes set the component up for its interaction with COM+.
//
// Transaction(TransactionOption.Required)
//     This component requires a Transaction.
//
// JustInTimeActivation(true)
//     COM+ will activate and deactivate the object needed,
//     conserving resources when the object is not actively in use.
//
// ObjectPooling(Enabled:=true, MinPoolSize:=2, MaxPoolSize:=10)
//     The component uses Object Pooling, which lets you create a pool
//     of objects that won't be destroyed when they're released. Object
//     pooling increases performance for objects that are expensive to
//     create. This component will hold at least 2 objects and a maximum
//     of 10 in its pool at any given time.
//
// ConstructionEnabledAttribute(Default:="Server=localhost;
// DataBase=Northwind;Integrated Security=SSPI"
//     The component's Object Construction string. See the Construct
//     method below for more.

[Transaction(TransactionOption.Required),
JustInTimeActivation(true), 
ObjectPooling(Enabled=true, MinPoolSize=2, MaxPoolSize=10), 
ConstructionEnabledAttribute(Default="Server=localhost;DataBase=Northwind;Integrated Security=SSPI")]
public class Supplier : ServicedComponent
{
    private string m_strConnectionstring;

#region " Constants for Database connections ";

    // Database connection constants used for example purposes only below.
    protected const string SQL_CONNECTION_STRING = "Server=localhost;" + 
													"DataBase=Northwind;" + 
													"Integrated Security=SSPI";

    protected const string MSDE_CONNECTION_STRING = "Server=(local)\\NetSDK;" + 
													"DataBase=Northwind;" + 
													"Integrated Security=SSPI";

#endregion

    // Default constructor.
    public Supplier()
	{
        // Set the default connection string. if we're using the component in
        // COM+, this value will get overriden later in the Construct method.
        m_strConnectionstring = SQL_CONNECTION_STRING;
        DoTracing("Supplier instantiated at " + DateTime.Now.ToLongTimeString());
    }

    // Construct method applies only when the component is part of a
    // COM+ application.
    protected override void Construct(string constructstring)
	{
        // This method gets called right after the constructor. The
        // constructstring parameter contains the class's default
        // property, which was set above in the
        // ConstructionEnabledAttribute. The default string appears in
        // the COM+ Explorer, in the Object construction section of the
        // Activation Tab of your component's properties. So a System
        // Administrator can adjust its value without your having to
        // recompile your class.
        // In this example, we've set it to a connection string, but it
        // could contain any string value you choose.

        if (constructstring.Length > 0 )
		{
            m_strConnectionstring = constructstring;
        }
    } //Construct

    // Simulates a call to a stored procedure to add a supplier to the
    // Suppliers table.
    //
    // AutoComplete attribute means if the procedure exits with no
    // exception, then SetComplete is automatically called, otherwise
    // SetAbort is called.
    [AutoCompleteAttribute(true)]
    public void AddSupplier(string CompanyName, string Phone)
	{
        //...
        try 
		{
            //cmd = new SqlCommand("AddSupplier", cnn)
            //...
            //cmd.ExecuteNonQuery()
            DoTracing("Supplier.AddSupplier called at " + DateTime.Now.ToLongTimeString());
		}
		catch( Exception exp)
		{
            throw new Exception(exp.Message, exp.InnerException);
        }
    }

    // Simulates a call to a stored procedure to delete a supplier.
    [AutoCompleteAttribute(true)]
    public void DeleteSupplier(int SupplierID)
	{
        //...
        try 
		{
            //cmd = new SqlCommand("DeleteSupplier", cnn)
            //...
            //cmd.ExecuteNonQuery()
            DoTracing("Supplier.DeleteSupplier called at " + DateTime.Now.ToLongTimeString());
       } 
		catch( Exception exp)
		{
            throw new Exception(exp.Message, exp.InnerException);
        }
    }

    // Simulates a call to a stored procedure to get a list of all
    // suppliers in Suppliers table.
    [AutoCompleteAttribute(true)]
    public void GetSuppliers() 
	{
        //...
        try 
		{
            //..
            //da.Fill(dt)
            DoTracing("Supplier.GetSuppliers called at " + DateTime.Now.ToLongTimeString());
            //return dt
		}
		catch( Exception exp )
		{
            throw new Exception(exp.Message, exp.InnerException);
        }
    }

    // Simulates a call to a stored procedure to update a supplier.
    [AutoCompleteAttribute(true)]
    public void UpdateSupplier(int SupplierID, string CompanyName, string Phone)
	{
        //...
        try 
		{
            //cmd = new SqlCommand("UpdateSupplier", cnn)
            //...
            //cmd.ExecuteNonQuery()
            DoTracing("Supplier.UpdateSupplier called at " + DateTime.Now.ToLongTimeString());
		} 
		catch( Exception exp )
		{
            throw new Exception(exp.Message, exp.InnerException);
        }
    }

    // Initialize the object for the current user

    protected override void Activate()
	{
        // Put code here such opening a connection, creating
        // temp files, etc.
        DoTracing("Supplier activated at " + DateTime.Now.ToLongTimeString());
    }

    // Indicate to COM+ whether the object can be pooled.
    // Put code here to indicate whether or not you want the object to
    // be pooled at this time.
    protected override bool CanBePooled() 
	{
        DoTracing("Supplier.CanBePooled called at " + DateTime.Now.ToLongTimeString());
        return true;
    }

    // Reset the object for the next user
    protected override void Deactivate()
	{
        DoTracing("Supplier deactivated at " + DateTime.Now.ToLongTimeString());
    }

    // Writes to Application Log.
    private void WriteToLog(string strMsg)
	{
        EventLog oEventLog = new EventLog("Application");
        oEventLog.Source = "HowTo.CreateEnterpriseServicesComponents";
        oEventLog.WriteEntry(strMsg);
    }

    // Reports on current object status in various ways.
    private void DoTracing(string strMsg)
	{
        // Create a trace listener for the event log.
        //logTraceListener new EventLogTraceListener("Supplier--C# How-To: EnterpriseComponents")
        // Add the trace listener to the collection.
        //Trace.Listeners.Add(logTraceListener)
        // Create a trace listener that will send send trace output to the console
        // It could have written to a file or stream instead.
        TextWriterTraceListener consoleTraceListener = new TextWriterTraceListener(System.Console.Out);
        // Add the trace listener to the collection.
        Trace.Listeners.Add(consoleTraceListener);
        // Write output.
        Trace.WriteLine(strMsg);
    }
}

