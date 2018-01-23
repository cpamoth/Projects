//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;
using System.Windows.Forms;

public class frmMain : System.Windows.Forms.Form 
{

#region " Windows Form Designer generated code "

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new frmMain());
	}

	public frmMain () 
	{
		//This call is required by the Windows Form Designer.

		InitializeComponent();

		//Add any initialization after the InitializeComponent() call
		// This code sets up the Help|About menuitem.

		AssemblyInfo ainfo = new AssemblyInfo();
		this.Text = ainfo.Title;
		this.mnuAbout.Text = string.Format("&About {0} ...", ainfo.Title);
	}

	//Form overrides dispose to clean up the component list.

	protected override void Dispose(bool disposing) {
		if (disposing) {
			if (components != null) {
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	//Required by the Windows Form Designer

	private System.ComponentModel.IContainer components = null;

	//NOTE: The following procedure is required by the Windows Form Designer
	//It can be modified using the Windows Form Designer.  
	//Do not modify it using the code editor.

	private System.Windows.Forms.Button cmdOverloads;

	private System.Windows.Forms.Button cmdConstructors;

	private System.Windows.Forms.Button cmdPropertySyntax;

	private System.Windows.Forms.Button cmdstaticMembers;

	private System.Windows.Forms.MainMenu MainMenu1;

	private System.Windows.Forms.MenuItem mnuFile;

	private System.Windows.Forms.MenuItem mnuExit;

	private System.Windows.Forms.MenuItem mnuHelp;

	private System.Windows.Forms.MenuItem mnuAbout;

	private void InitializeComponent() {
		this.cmdOverloads = new System.Windows.Forms.Button();
		this.cmdConstructors = new System.Windows.Forms.Button();
		this.cmdPropertySyntax = new System.Windows.Forms.Button();
		this.cmdstaticMembers = new System.Windows.Forms.Button();
		this.MainMenu1 = new System.Windows.Forms.MainMenu();
		this.mnuFile = new System.Windows.Forms.MenuItem();
		this.mnuExit = new System.Windows.Forms.MenuItem();
		this.mnuHelp = new System.Windows.Forms.MenuItem();
		this.mnuAbout = new System.Windows.Forms.MenuItem();
		this.SuspendLayout();
		// 
		// cmdOverloads
		// 
		this.cmdOverloads.Location = new System.Drawing.Point(8, 8);
		this.cmdOverloads.Name = "cmdOverloads";
		this.cmdOverloads.Size = new System.Drawing.Size(336, 25);
		this.cmdOverloads.TabIndex = 0;
		this.cmdOverloads.Text = "&Overloads";
		this.cmdOverloads.Click += new System.EventHandler(this.cmdOverloads_Click);
		// 
		// cmdConstructors
		// 
		this.cmdConstructors.Location = new System.Drawing.Point(8, 48);
		this.cmdConstructors.Name = "cmdConstructors";
		this.cmdConstructors.Size = new System.Drawing.Size(336, 25);
		this.cmdConstructors.TabIndex = 1;
		this.cmdConstructors.Text = "&Constructors";
		this.cmdConstructors.Click += new System.EventHandler(this.cmdConstructors_Click);
		// 
		// cmdPropertySyntax
		// 
		this.cmdPropertySyntax.Location = new System.Drawing.Point(8, 88);
		this.cmdPropertySyntax.Name = "cmdPropertySyntax";
		this.cmdPropertySyntax.Size = new System.Drawing.Size(336, 25);
		this.cmdPropertySyntax.TabIndex = 2;
		this.cmdPropertySyntax.Text = "Poperty &Syntax";
		this.cmdPropertySyntax.Click += new System.EventHandler(this.cmdPropertySyntax_Click);
		// 
		// cmdstaticMembers
		// 
		this.cmdstaticMembers.Location = new System.Drawing.Point(8, 128);
		this.cmdstaticMembers.Name = "cmdstaticMembers";
		this.cmdstaticMembers.Size = new System.Drawing.Size(336, 25);
		this.cmdstaticMembers.TabIndex = 4;
		this.cmdstaticMembers.Text = "static &Members";
		this.cmdstaticMembers.Click += new System.EventHandler(this.cmdstaticMembers_Click);
		// 
		// MainMenu1
		// 
		this.MainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				  this.mnuFile,
																				  this.mnuHelp});
		// 
		// mnuFile
		// 
		this.mnuFile.Index = 0;
		this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				this.mnuExit});
		this.mnuFile.Text = "&File";
		// 
		// mnuExit
		// 
		this.mnuExit.Index = 0;
		this.mnuExit.Text = "E&xit";
		this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
		// 
		// mnuHelp
		// 
		this.mnuHelp.Index = 1;
		this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				this.mnuAbout});
		this.mnuHelp.Text = "&Help";
		// 
		// mnuAbout
		// 
		this.mnuAbout.Index = 0;
		this.mnuAbout.Text = "Text Comes from AssemblyInfo";
		this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
		// 
		// frmMain
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(352, 177);
		this.Controls.Add(this.cmdstaticMembers);
		this.Controls.Add(this.cmdPropertySyntax);
		this.Controls.Add(this.cmdConstructors);
		this.Controls.Add(this.cmdOverloads);
		this.Menu = this.MainMenu1;
		this.Name = "frmMain";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Title Comes from Assembly Info";
		this.ResumeLayout(false);

	}

#endregion

#region " Standard Menu Code "

	

	

	// This code simply shows the About form.

	private void mnuAbout_Click(object sender, System.EventArgs e) {

		// Open the About form in Dialog Mode

		frmAbout frm = new frmAbout();

		frm.ShowDialog(this);

		frm.Dispose();

	}

	// This code will close the form.

	private void mnuExit_Click(object sender, System.EventArgs e) {

		// Close the current form

		this.Close();

	}

#endregion

	private void cmdConstructors_Click(object sender, System.EventArgs e)
	{
		// A Constructor in C# is simply code that executes when
		// an class is instanced. The constructor may or may not require
		// parameters to be passed into the constructor.
		// Here is the syntax for instantiating a class with a
		// constructor in a single line. This is the recommended way to
		// instantiate a class with a constructor:

		CustomerWithConstructor cust = new CustomerWithConstructor("1101", "Carmen", "Smith");

		// Alternatively you can instantiate a class after creating the
		// variable:

		CustomerWithConstructor cust2 = new CustomerWithConstructor("1101", "Carmen", "Smith");

		// You can overload the void new procedure in a class if you want.
		// By doing this, you can create different versions of the
		// constructor that can be called from different client code.
		// For example, you may want a constructor that can only be called
		// by external code, in which case you would use public void
		// new(). { you might want to add a separate constructor that can
		// only be called from code within the class (for example, if you
		// were implementing a Clone method). In that case you would use
		// private void new().
		//
		// For more information on overloading constructors, see 
		// ms-help://MS.VSCC/MS.MSDNVS/vbcon/html/vbconHowDoIControlCreationOfComponents.htm
		// in the Visual Studio .NET documentation.
		//
		// Another thing you can do with constructors in C# is perform 
		// access control for your class. For example, suppose you wanted to
		// develop a class that is exposed to external code, but not creatable
		// directly. In classic Visual Basic you would set the Instancing
		// property of the class to "PublicNotCreatable". InC#, you would
		// create a public class and add a constructor to the class that is
		// declared Friend. For more information on using constructors to
		// control access to your class, see
		// ms-help://MS.VSCC/MS.MSDNVS/vbcon/html/vbconhowvisualbasic60instancingmapstovisualbasic70.htm
		// in the Visual Studio .NET documentation.
	}

	private void cmdOverloads_Click(object sender, System.EventArgs e)
	{
		Customer myCust1;

		Customer myCust2;

		// The purpose of the "Overloads" keyword in C# is to allow
		// you to create multiple procedures that have the same name but
		// execute separate code.
		//
		// Suppose that you want to create a way of looking up a customer
		// from either a database or a collection. You also want to be able
		// to look up the customer by either last name or the customer's ID
		// number. In classic Visual Basic, you would normally create two
		// separate functions such "GetCustomerByID" and 
		// "GetCustomerByLastName".
		//
		// In C# you can create to functions with the same name. 
		// Each version of the function can have its
		// own set of parameters that are different from the others.
		//
		// When calling an overloaded procedure, you will see the different
		// versions that are available throughC#'s Intellisense. At the
		// beginning of the Intellisense message you will see up and down
		// arrows with an indicator to show how many overloaded versions of
		// the procedure exist, and which one you are currently viewing.
		//
		// You can see an example of the Intellisense for an overloaded
		// procedure by changing the parameters below. Put your cursor after
		// the number 1101 in the second GetCustomer call and hit the space
		// bar.

		myCust1 = GetCustomer("Smith");
		myCust2 = GetCustomer(1101);

		// Scroll down to the GetCustomer procedures below to see how to
		// implement  in C#.
	}

	

	private void cmdPropertySyntax_Click(object sender, System.EventArgs e)
	{
		// The code below creates an instance of the 
		// CustomerPropertySyntax class and retrieves the value
		// from a read-only property, AccountNumber. Step into the
		// code to see more details on property syntax.

		string myAccount;
		CustomerPropertySyntax cust = new CustomerPropertySyntax("1101");
		myAccount = cust.AccountNumber;

	}

	private void cmdstaticMembers_Click(object sender, System.EventArgs e)
	{
		// There are two main types of shared members in a class.
		// The first type are shared variables, and the second
		// type are shared procedures.
		//
		// Below we create two instances of the customer class
		// and change the shared variable "CompanyName". When
		// you run this code you should see a message that the
		// company name is Tailspin Toys. This shows that
		// changing the variable on any instance of the class
		// actually changes its value for all instances.

		CustomerWithstaticMembers cust1 = new CustomerWithstaticMembers();
		CustomerWithstaticMembers cust2 = new CustomerWithstaticMembers();


		// The next type of shared member is the shared procedure.
		// When you declare a procedure shared inside a class,
		// you can invoke the procedure the normal way through
		// an instance of the class (ie cust1.LastOrderDate).
		// You can also invoke the procedure without using an
		// instance of the class by using the syntax:
		// Classname.ProcedureName.
		//
		// Invoke the shared procedure by using the classname
		// syntax (recommended):

		MessageBox.Show("The last order was placed on " + CustomerWithstaticMembers.LastOrderDate(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private Customer GetCustomer(int CustID)
	{
		// This is the function that is called if you pass in an integer.

		Customer cust = new Customer();

		// Normally, you would use the CustID integer to search a
		// database or collection for the customer's ID number.
		// Here, we're just going to populate a Customer object with
		// dummy data.

		cust.AccountNumber = "1101";
		cust.FirstName = "Carmen";
		cust.LastName = "Smith";
		return cust;
	}

	private Customer GetCustomer(string CustLastName)
	{
		// This is the function that is called if you pass in a string.
		Customer cust = new Customer();

		// Normally, you would use the CustLastName string to search a
		// database or collection for the customer's last name.
		// Here, we're just going to populate a Customer object with
		// dummy data.
		cust.AccountNumber = "1101";
		cust.FirstName = "Carmen";
		cust.LastName = "Smith";
		return cust;
	}

	private void frmMain_Load(object sender, System.EventArgs e) {

	}

}

