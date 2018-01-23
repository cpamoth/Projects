//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;
using System.Windows.Forms;

public class frmMain: System.Windows.Forms.Form 
{
	// Used to specify which listbox the DisplayArrayData
	// method should use when loading data.
	private enum WhichListBox
	{
		BoxOne,
		BoxTwo
	}

	#region " Windows Form Designer generated code "
	public frmMain() 
	{
		//This call is required by the Windows Form Designer.
		InitializeComponent();

		//Add any initialization after the InitializeComponent() call
		// So that we only need to set the title of the application once,
		// we use the AssemblyInfo class (defined in the AssemblyInfo.cs file)
		// to read the AssemblyTitle attribute.
		AssemblyInfo ainfo = new AssemblyInfo();

		this.Text = ainfo.Title;
		this.mnuAbout.Text = string.Format("&About {0} ...", ainfo.Title);
	}

	//Form overrides dispose to clean up the component list.
	protected override void Dispose(bool disposing) 
	{
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
	private System.Windows.Forms.MainMenu mnuMain;
	private System.Windows.Forms.MenuItem mnuFile;
	private System.Windows.Forms.MenuItem mnuExit;
	private System.Windows.Forms.MenuItem mnuHelp;
	private System.Windows.Forms.MenuItem mnuAbout;
	private System.Windows.Forms.Button cmdCreateStatic;

	private System.Windows.Forms.Button cmdCreateDynamic;

	private System.Windows.Forms.Button cmdSort;

	private System.Windows.Forms.Button cmdReverse;

	private System.Windows.Forms.Button cmdBinarySearch;

	private System.Windows.Forms.GroupBox grpArrayType;

	private System.Windows.Forms.RadioButton optstrings;

	private System.Windows.Forms.RadioButton optObjects;

	private System.Windows.Forms.ListBox lstArrayData;

	private System.Windows.Forms.ListBox lstAfter;

	private System.Windows.Forms.GroupBox grpCompareField;

	private System.Windows.Forms.RadioButton optId;

	private System.Windows.Forms.RadioButton optName;

	private System.Windows.Forms.Label lblDataAfter;

	private System.Windows.Forms.TextBox txtLength;

	private System.Windows.Forms.TextBox txtBSearchFor;

	private System.Windows.Forms.Label lblDataAsLoaded;

	private System.Windows.Forms.Label lblSearchFor;

	private System.Windows.Forms.Label lblNumElements;

	private System.Windows.Forms.Button cmdCreateMatrix;

	private void InitializeComponent() 
	{
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));

		this.mnuMain = new System.Windows.Forms.MainMenu();

		this.mnuFile = new System.Windows.Forms.MenuItem();

		this.mnuExit = new System.Windows.Forms.MenuItem();

		this.mnuHelp = new System.Windows.Forms.MenuItem();

		this.mnuAbout = new System.Windows.Forms.MenuItem();

		this.cmdCreateStatic = new System.Windows.Forms.Button();

		this.cmdCreateDynamic = new System.Windows.Forms.Button();

		this.cmdSort = new System.Windows.Forms.Button();

		this.cmdReverse = new System.Windows.Forms.Button();

		this.cmdBinarySearch = new System.Windows.Forms.Button();

		this.cmdCreateMatrix = new System.Windows.Forms.Button();

		this.grpArrayType = new System.Windows.Forms.GroupBox();

		this.optObjects = new System.Windows.Forms.RadioButton();

		this.optstrings = new System.Windows.Forms.RadioButton();

		this.lstArrayData = new System.Windows.Forms.ListBox();

		this.lstAfter = new System.Windows.Forms.ListBox();

		this.grpCompareField = new System.Windows.Forms.GroupBox();

		this.optName = new System.Windows.Forms.RadioButton();

		this.optId = new System.Windows.Forms.RadioButton();

		this.lblDataAsLoaded = new System.Windows.Forms.Label();

		this.lblDataAfter = new System.Windows.Forms.Label();

		this.txtLength = new System.Windows.Forms.TextBox();

		this.txtBSearchFor = new System.Windows.Forms.TextBox();

		this.lblSearchFor = new System.Windows.Forms.Label();

		this.lblNumElements = new System.Windows.Forms.Label();

		this.grpArrayType.SuspendLayout();

		this.grpCompareField.SuspendLayout();

		this.SuspendLayout();

		//

		//mnuMain

		//

		this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuFile, this.mnuHelp});

		//

		//mnuFile

		//

		this.mnuFile.Index = 0;

		this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuExit});

		this.mnuFile.Text = "&File";

		//

		//mnuExit

		//

		this.mnuExit.Index = 0;

		this.mnuExit.Text = "E&xit";
		this.mnuExit.Click += new EventHandler(mnuExit_Click);

		//

		//mnuHelp

		//

		this.mnuHelp.Index = 1;

		this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuAbout});

		this.mnuHelp.Text = "&Help";

		//

		//mnuAbout

		//

		this.mnuAbout.Index = 0;

		this.mnuAbout.Text = "Text Comes from AssemblyInfo";
		this.mnuAbout.Click += new EventHandler(mnuAbout_Click);

		//

		//cmdCreateStatic

		//

		this.cmdCreateStatic.Location = new System.Drawing.Point(8, 8);

		this.cmdCreateStatic.Name = "cmdCreateStatic";

		this.cmdCreateStatic.Size = new System.Drawing.Size(144, 23);

		this.cmdCreateStatic.TabIndex = 0;

		this.cmdCreateStatic.Text = "&Create Static Array";
		this.cmdCreateStatic.Click += new EventHandler(cmdCreateStatic_Click);

		//

		//cmdCreateDynamic

		//

		this.cmdCreateDynamic.Location = new System.Drawing.Point(8, 192);

		this.cmdCreateDynamic.Name = "cmdCreateDynamic";

		this.cmdCreateDynamic.Size = new System.Drawing.Size(144, 23);

		this.cmdCreateDynamic.TabIndex = 6;

		this.cmdCreateDynamic.Text = "Create &Dynamic Array";
		this.cmdCreateDynamic.Click += new EventHandler(cmdCreateDynamic_Click);
		//

		//cmdSort

		//

		this.cmdSort.Location = new System.Drawing.Point(8, 40);

		this.cmdSort.Name = "cmdSort";

		this.cmdSort.Size = new System.Drawing.Size(144, 23);

		this.cmdSort.TabIndex = 1;

		this.cmdSort.Text = "&Sort";
		this.cmdSort.Click += new EventHandler(cmdSort_Click);

		//

		//cmdReverse

		//

		this.cmdReverse.Location = new System.Drawing.Point(8, 72);

		this.cmdReverse.Name = "cmdReverse";

		this.cmdReverse.Size = new System.Drawing.Size(144, 23);

		this.cmdReverse.TabIndex = 2;

		this.cmdReverse.Text = "&Reverse";
		this.cmdReverse.Click += new EventHandler(cmdReverse_Click);
		//

		//cmdBinarySearch

		//

		this.cmdBinarySearch.Location = new System.Drawing.Point(8, 104);

		this.cmdBinarySearch.Name = "cmdBinarySearch";

		this.cmdBinarySearch.Size = new System.Drawing.Size(144, 23);

		this.cmdBinarySearch.TabIndex = 3;

		this.cmdBinarySearch.Text = "&Binary Search";
		this.cmdBinarySearch.Click += new EventHandler(cmdBinarySearch_Click);
		//

		//cmdCreateMatrix

		//

		this.cmdCreateMatrix.Location = new System.Drawing.Point(8, 256);

		this.cmdCreateMatrix.Name = "cmdCreateMatrix";

		this.cmdCreateMatrix.Size = new System.Drawing.Size(144, 23);

		this.cmdCreateMatrix.TabIndex = 9;

		this.cmdCreateMatrix.Text = "Create a &Matrix Array";
		this.cmdCreateMatrix.Click += new EventHandler(cmdCreateMatrix_Click);
		//

		//grpArrayType

		//

		this.grpArrayType.Controls.AddRange(new System.Windows.Forms.Control[] {this.optObjects, this.optstrings});

		this.grpArrayType.Location = new System.Drawing.Point(160, 8);

		this.grpArrayType.Name = "grpArrayType";

		this.grpArrayType.Size = new System.Drawing.Size(120, 80);

		this.grpArrayType.TabIndex = 10;

		this.grpArrayType.TabStop = false;

		this.grpArrayType.Text = "&Array of . . .";

		//

		//optObjects

		//

		this.optObjects.Location = new System.Drawing.Point(8, 48);

		this.optObjects.Name = "optObjects";

		this.optObjects.Size = new System.Drawing.Size(75, 24);

		this.optObjects.TabIndex = 1;

		this.optObjects.Text = "Objects";
		this.optObjects.CheckedChanged += new EventHandler(DataTypeCheckedChanged);

		//

		//optstrings

		//

		this.optstrings.Location = new System.Drawing.Point(8, 24);

		this.optstrings.Name = "optstrings";

		this.optstrings.Size = new System.Drawing.Size(75, 24);

		this.optstrings.TabIndex = 0;

		this.optstrings.Text = "strings";
		this.optstrings.CheckedChanged += new EventHandler(DataTypeCheckedChanged);
		//

		//lstArrayData

		//

		this.lstArrayData.Location = new System.Drawing.Point(160, 120);

		this.lstArrayData.Name = "lstArrayData";

		this.lstArrayData.Size = new System.Drawing.Size(264, 95);

		this.lstArrayData.TabIndex = 13;

		//

		//lstAfter

		//

		this.lstAfter.Location = new System.Drawing.Point(160, 248);

		this.lstAfter.Name = "lstAfter";

		this.lstAfter.Size = new System.Drawing.Size(264, 108);

		this.lstAfter.TabIndex = 15;

		//

		//grpCompareField

		//

		this.grpCompareField.Controls.AddRange(new System.Windows.Forms.Control[] {this.optName, this.optId});

		this.grpCompareField.Location = new System.Drawing.Point(288, 8);

		this.grpCompareField.Name = "grpCompareField";

		this.grpCompareField.Size = new System.Drawing.Size(136, 80);

		this.grpCompareField.TabIndex = 11;

		this.grpCompareField.TabStop = false;

		this.grpCompareField.Text = "&Field to use for Sorts";

		//

		//optName

		//

		this.optName.Location = new System.Drawing.Point(8, 48);

		this.optName.Name = "optName";

		this.optName.Size = new System.Drawing.Size(120, 24);

		this.optName.TabIndex = 1;

		this.optName.Text = "Customer Name";
		this.optName.CheckedChanged += new EventHandler(CompareKeyCheckedChanged);

		//

		//optId

		//

		this.optId.Location = new System.Drawing.Point(8, 24);

		this.optId.Name = "optId";

		this.optId.Size = new System.Drawing.Size(110, 24);

		this.optId.TabIndex = 0;

		this.optId.Text = "Customer Id";
		this.optId.CheckedChanged += new EventHandler(CompareKeyCheckedChanged);
		//

		//lblDataAsLoaded

		//

		this.lblDataAsLoaded.Location = new System.Drawing.Point(160, 96);

		this.lblDataAsLoaded.Name = "lblDataAsLoaded";

		this.lblDataAsLoaded.Size = new System.Drawing.Size(264, 23);

		this.lblDataAsLoaded.TabIndex = 12;

		this.lblDataAsLoaded.Text = "Data Loaded";

		//

		//lblDataAfter

		//

		this.lblDataAfter.Location = new System.Drawing.Point(160, 224);

		this.lblDataAfter.Name = "lblDataAfter";

		this.lblDataAfter.Size = new System.Drawing.Size(264, 23);

		this.lblDataAfter.TabIndex = 14;

		this.lblDataAfter.Text = "No Data Displayed";

		//

		//txtLength

		//

		this.txtLength.Location = new System.Drawing.Point(112, 224);

		this.txtLength.Name = "txtLength";

		this.txtLength.Size = new System.Drawing.Size(40, 20);

		this.txtLength.TabIndex = 8;

		this.txtLength.Text = "3";

		this.txtLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.txtLength.Validating += new System.ComponentModel.CancelEventHandler(txtLength_Validating);

		//

		//txtBSearchFor

		//

		this.txtBSearchFor.Location = new System.Drawing.Point(8, 160);

		this.txtBSearchFor.Name = "txtBSearchFor";

		this.txtBSearchFor.Size = new System.Drawing.Size(144, 20);

		this.txtBSearchFor.TabIndex = 5;

		this.txtBSearchFor.Text = "Joe Max";

		//

		//lblSearchFor

		//

		this.lblSearchFor.Location = new System.Drawing.Point(8, 136);

		this.lblSearchFor.Name = "lblSearchFor";

		this.lblSearchFor.Size = new System.Drawing.Size(144, 23);

		this.lblSearchFor.TabIndex = 4;

		this.lblSearchFor.Text = "Search F&or:";

		//

		//lblNumElements

		//

		this.lblNumElements.Location = new System.Drawing.Point(8, 224);

		this.lblNumElements.Name = "lblNumElements";

		this.lblNumElements.Size = new System.Drawing.Size(96, 32);

		this.lblNumElements.TabIndex = 7;

		this.lblNumElements.Text = "# of &Elements for Dynamic Array";

		//

		//frmMain

		//

		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);

		this.ClientSize = new System.Drawing.Size(434, 371);

		this.Controls.AddRange(new System.Windows.Forms.Control[] {this.lblNumElements, this.lblSearchFor, this.txtBSearchFor, this.txtLength, this.lblDataAfter, this.lblDataAsLoaded, this.grpCompareField, this.lstAfter, this.lstArrayData, this.grpArrayType, this.cmdCreateMatrix, this.cmdBinarySearch, this.cmdReverse, this.cmdSort, this.cmdCreateDynamic, this.cmdCreateStatic});

		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

		this.Icon = (System.Drawing.Icon) resources.GetObject("$this.Icon");

		this.MaximizeBox = false;

		this.Menu = this.mnuMain;

		this.Name = "frmMain";

		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

		this.Text = "Title Comes from Assembly Info";

		this.grpArrayType.ResumeLayout(false);

		this.grpCompareField.ResumeLayout(false);

		this.ResumeLayout(false);
		this.Load += new EventHandler(frmMain_Load);
	}

#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new frmMain());
	}

#region " Standard Menu Code "
	// This code simply shows the About form.
	private void mnuAbout_Click(object sender, System.EventArgs e) 
	{
		// Open the About form in Dialog Mode
		frmAbout frm = new frmAbout();
		frm.ShowDialog(this);
		frm.Dispose();
	}

	// This code will close the form.
	private void mnuExit_Click(object sender, System.EventArgs e) 
	{
		// Close the current form
		this.Close();
	}
#endregion

	private void frmMain_Load(object sender, System.EventArgs e) 
	{
		this.optstrings.Checked = true;
		this.optName.Checked = true;
	}

	private void cmdBinarySearch_Click(object sender, System.EventArgs e)
	{
		string strMsg;
		int intPos;

		// Make sure there is a value to use a search criteria.
		if (this.txtBSearchFor.Text.Length == 0) 
		{
			MessageBox.Show("Please enter a value to search for", "Binary Search", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			this.txtBSearchFor.Select();
			return;
		}

		string strDataToFind = this.txtBSearchFor.Text;
		this.lstAfter.Items.Clear();

		if (this.optstrings.Checked)
		{
			string[] strData = {"Joe Hasterman", "Ted Mattison", "Joe Rummel", "Brian Gurnure", "Doug Landal"};

			// Sort the array otherwise the BinarySearch method won't work.
			Array.Sort(strData);

			// Show the data sorted.
			DisplayArrayData(strData);

			// Perform search
			intPos = Array.BinarySearch(strData, strDataToFind);

			if (intPos > 0) 
			{
				strMsg = string.Format("The value {0} was found in the array at position {1}.", strDataToFind, intPos.ToString());
			}
			else 
			{
				// if the item is not found, a negative number is returned.
				// This is the bitwise complement for the location where the
				// item would have been *if* it existed.
				// We use the ! operator to flip it back to a postive number.
				int intBWC = (~intPos);

				if (intBWC == 0) 
				{
					strMsg = string.Format("The value {0} was NOT found in the array. if it did exist it would be at position {1} before {2}", strDataToFind, intBWC.ToString(), strData[1]);
				}
				else 
				{
					string[] strItems = {strDataToFind, intBWC.ToString(), strData[intBWC - 1], strData[intBWC]};
					strMsg = string.Format("The value {0} was NOT found in the array. if it did exist it would be at position {1} between {2} and {3}.", strItems);
				}
			}

			MessageBox.Show(strMsg, "Binary Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		else 
		{
			Customer[] objData = {new Customer(3423, "Joe Hasterman"), new Customer(9348, "Ted Mattison"), new Customer(3581, "Joe Rummel"), new Customer(7642, "Brian Gurnure"), new Customer(2985, "Doug Landal")};
			Array.Sort(objData);
			DisplayArrayData(objData);

			// When searching an array of objects, we need to use
			// a compatible object type. In this case we're using
			// the value provided by the txtBSearchFor box as the 
			// customer's name and providing a bogus Id value.
			// The key to binary search working with objects is that
			// the object must implement IComparable.
			Customer c = new Customer(1, this.txtBSearchFor.Text);
			intPos = Array.BinarySearch(objData, c, null);

			if (intPos > 0)
			{
				strMsg = string.Format("The value {0} was found in the array at position {1}.", strDataToFind, intPos.ToString());
			}
			else 
			{
				int intBWC = (~intPos);

				if (intBWC == 0)
				{
					strMsg = string.Format("The value {0} was NOT found in the array. if it did exist it would be at position {1} before {2}", strDataToFind, intBWC.ToString(), objData[1].Name);
				}
				else
				{
					string[] strItems = {strDataToFind, intBWC.ToString(), objData[intBWC - 1].Name, objData[intBWC].Name};
					strMsg = string.Format("The value {0} was NOT found in the array. if it did exist it would be at position {1} between {2} and {3}.", strItems);
				}
			}
			MessageBox.Show(strMsg, "Binary Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	private void cmdCreateDynamic_Click(object sender, System.EventArgs e) 
	{
		// This routine builds a dynamic array
		// using the value specified in txtLength
		// to set the number of buckets.

		if (this.optstrings.Checked)
		{
			string[] strDynamicData = new string[System.Convert.ToInt32(this.txtLength.Text)];

			for(int i = 0; i < strDynamicData.Length; i++)
			{				
				strDynamicData[i] = frmInputBox.InputBox("Enter a string", i.ToString(), "None " + i);
			}
			DisplayArrayData(strDynamicData);
		}
		else 
		{
			Customer[] strDynamicData = new Customer[System.Convert.ToInt32(this.txtLength.Text)];

			for(int i = 0; i < strDynamicData.Length; i++)
			{
				strDynamicData[i] = new Customer();
				strDynamicData[i].Id = ((i + 1) * 10);
				strDynamicData[i].Name = frmInputBox.InputBox("Enter a string", ("Item " + (i + 1)), ("None " + i + 1));
			}

			DisplayArrayData(strDynamicData);
		}
	}

	private void cmdCreateMatrix_Click(object sender, System.EventArgs e)
	{
		// This procedure uses the new initialization syntax to create a simple matrix array.
		string[,] strMatrix = {{"Bob", "Carol"}, {"Ted", "Alice"}, {"Joe", "Lisa"}};

		this.DisplayArrayData(strMatrix);
	}

	private void cmdCreateStatic_Click(object sender, System.EventArgs e)
	{
		// This procedure uses the new initialization to build either a static
		// array of strings or an array of Customer objects.
		// Note that the arrary could of course be resized using the Restaetment.

		if (this.optstrings.Checked)
		{
			string[] strData = {"Joe Hasterman", "Ted Mattison", "Joe Rummel", "Brian Gurnure", "Doug Landal"};
			DisplayArrayData(strData);
		}		
		else 
		{
			// This command here takes advantage of the fact that objects in .NET
			// can have parameterized constructors. This allows us to specify an
			// array of objects in one line of code.
			Customer[] objData = {new Customer(3423, "Joe Hasterman"), new Customer(9348, "Ted Mattison"), new Customer(3581, "Joe Rummel"), new Customer(7642, "Brian Gurnure"), new Customer(2985, "Doug Landal")};
			DisplayArrayData(objData);
		}
	}

	private void cmdReverse_Click(object sender, System.EventArgs e)
	{
		// Reverse reorders the elements in an array in decending order as they were added.
		this.lblDataAfter.Text = "Data After Reverse";

		if (this.optstrings.Checked)
		{
			string[] strData = {"Joe Hasterman", "Ted Mattison", "Joe Rummel", "Brian Gurnure", "Doug Landal"};
			Array.Reverse(strData);
			DisplayArrayData(strData, WhichListBox.BoxTwo);
		}
		else 
		{
			Customer[] objData = {new Customer(3423, "Joe Hasterman"), new Customer(9348, "Ted Mattison"), new Customer(3581, "Joe Rummel"), new Customer(7642, "Brian Gurnure"), new Customer(2985, "Doug Landal")};
			Array.Reverse(objData);
			DisplayArrayData(objData, WhichListBox.BoxTwo);
		}
	}

	private void cmdSort_Click(object sender, System.EventArgs e)
	{
		// In order for this procedure to work, the items contained in the array
		// must support comparsion. This is accomplished by objects by implementing
		// the IComparable interface.
		// See the Customer object definition below for more information.
		this.lblDataAfter.Text = "Data After Sort";

		if (this.optstrings.Checked)
		{
			string[] strData = {"Joe Hasterman", "Ted Mattison", "Joe Rummel", "Brian Gurnure", "Doug Landal"};
			Array.Sort(strData);
			DisplayArrayData(strData, WhichListBox.BoxTwo);
		}
		else 
		{
			Customer[] objData = {new Customer(3423, "Joe Hasterman"), new Customer(9348, "Ted Mattison"), new Customer(3581, "Joe Rummel"), new Customer(7642, "Brian Gurnure"), new Customer(2985, "Doug Landal")};
			Array.Sort(objData);
			DisplayArrayData(objData, WhichListBox.BoxTwo);
		}
	}

	private void txtLength_Validating(object sender, System.ComponentModel.CancelEventArgs e)
	{
		TextBox txt = (TextBox) sender;

		if (!IsNumeric(txt.Text))
		{
			// Make sure only numbers are entered in this text box.
			MessageBox.Show(txt.Text + " is not a valid number.", "Non-Numeric Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txt.Text = 1.ToString();
		}
		else 
		{
			try 
			{
				// Make sure the value fits within an int data types value range.
				int i = int.Parse(txt.Text);
			}
			catch (OverflowException)
			{
				string strMsg;

				strMsg = string.Format("The value you entered, {0} is too large. Valid values are between -{1} and {2}", txt.Text, int.MaxValue, int.MaxValue);
				MessageBox.Show(strMsg, "Value Overflow", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				// 
				txt.Text = 1.ToString();
			}
		}
	}

	private void CompareKeyCheckedChanged(object sender, System.EventArgs e) 
	{
		RadioButton opt = (RadioButton) sender;

		// Change the field used by the Customer object when sorts or searches 
		// are applied.
		if (opt.Name == "optId") 
		{
			// SetCompareKey is a shared member that will affect all instances
			// of the Customer type in this AppDomain.
			Customer.SetCompareKey(Customer.CompareField.Id);
		}
		else 
		{
			Customer.SetCompareKey(Customer.CompareField.Name);
		}

		this.lstAfter.Items.Clear();
		this.lblDataAfter.Text = "Data Cleared. Sort Again";
		this.cmdSort.Select();
	}

	private void DataTypeCheckedChanged(object sender, System.EventArgs e)
	{
		// Enabled the grpCompareField only when objects are being loaded
		// into arrays.
		RadioButton opt = (RadioButton) sender;

		this.grpCompareField.Enabled = (opt.Name != "optstrings");
		this.lstArrayData.Items.Clear();
		this.lstAfter.Items.Clear();
	}

	private void DisplayArrayData(Array arr)
	{
		// Delegate to the next more complex version.
		this.DisplayArrayData(arr, WhichListBox.BoxOne);
	}

	private void DisplayArrayData(Array arr, WhichListBox ListBox)
	{
		int u = (arr.Length);

		ListBox lst;

		if (ListBox == WhichListBox.BoxOne)
		{
			lst = this.lstArrayData;
		}
		else 
		{
			lst = this.lstAfter;
		}

		lst.Items.Clear();
		
		// Figure out how many dimensions (expressed as Rank)
		// the passed in array has.
		switch(arr.Rank)
		{
			case 1:
			{
				for(int i = 0; i < u; i++)
				{	
					lst.Items.Add(string.Format("{0} = {1}", i, arr.GetValue(i).ToString()));
				}
				break;
			}
			case 2:
			{
				for(int i = 0; i < arr.GetLength(0); i++)
				{
					for(int j = 0; j < arr.GetLength(1); j++)
					{
						lst.Items.Add(string.Format("({0}, {1}) = {2}", i, j, arr.GetValue(i, j).ToString()));
					}
				}
				break;
			}
			default:
			{
				// Sorry, we don't go beyond two dimensions
				lst.Items.Add(string.Format("The array received has too many dimensions ({0})", arr.Rank));
				break;
			}
		}
	}

	// Simple version of VB's IsNumeric()
	public bool IsNumeric(string val)
	{
		try
		{
			double result = 0;
			return Double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.CurrentInfo, out result);
		}
		catch
		{
			return false;
		}
	}
}

public class Customer: IComparable	// ' This allows us to support sorting and binary searches;
{
	// Used to control which field is used for comparisons.
	private static CompareField m_CompareField;

	public int Id;
	public string Name;

	public enum CompareField
	{
		Name,
		Id
	}

	static Customer()
	{
		// Set the default compare field
		m_CompareField = CompareField.Name;
	}

	public static void SetCompareKey(CompareField CompareKey)
	{
		// Change the comparison field
		m_CompareField = CompareKey;
	}
	
	// Set default values by delegating to the next most complex constructor
	public Customer():this(-1, "No Name")
	{
	}

	public Customer(int newId, string newName)
	{
		this.Id = newId;
		this.Name = newName;
	}

	public int CompareTo(object obj)
	{
		// First check to make sure we're only being compared to another customer.
		if(obj.GetType().ToString() == "Customer")
		{
			// Create a strongly typed reference
			Customer c = (Customer) obj;

			// Determine if we should compare by Name or Id
			if (Customer.m_CompareField == CompareField.Name)
			{
				return string.Compare(c.Name, this.Name, false);
			}
			else 
			{
				if (c.Id == this.Id)
				{
					return 0;
				} 
				else if (c.Id < this.Id)
				{
					return 1;
				}
				else 
				{
					return -1;
				}
			}
		}
		else 
		{
			throw new ArgumentException("Customers can only be compared against other customers. The object being passed in was a " + obj.GetType().ToString());
		}
	}

	public override string ToString()
	{
		// Normally ToString returns the fully qualified typename.
		// We are overriding it so that we can return a simple
		// display string when we're added to a list box.
		return string.Format("Id={0}, Name={1}", this.Id, this.Name);
	}
}
