using System;
using System.Windows.Forms;

public class frmMain: System.Windows.Forms.Form 
{

    private Customers myCustomers = new Customers();
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new frmMain());
	}

#region " Windows Form Designer generated code "

    public frmMain() 
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

	private System.Windows.Forms.Button cmdRemove;

	private System.Windows.Forms.Button cmdItem;

    private System.Windows.Forms.GroupBox GroupBox1;

    private System.Windows.Forms.Button cmdAdd;

    private System.Windows.Forms.TextBox txtAccount;

    private System.Windows.Forms.TextBox txtLastName;

    private System.Windows.Forms.TextBox txtFirstName;

    private System.Windows.Forms.Label Label3;

    private System.Windows.Forms.Label Label2;

    private System.Windows.Forms.Label Label1;

    private System.Windows.Forms.ListBox lstItems;

    private System.Windows.Forms.Button cmdReload;

	private System.Windows.Forms.MainMenu mnuMain;

	private System.Windows.Forms.MenuItem mnuFile;

	private System.Windows.Forms.MenuItem mnuExit;

	private System.Windows.Forms.MenuItem mnuHelp;

	private System.Windows.Forms.MenuItem mnuAbout;

	private void InitializeComponent() {

		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));

		this.cmdRemove = new System.Windows.Forms.Button();

		this.cmdItem = new System.Windows.Forms.Button();

		this.GroupBox1 = new System.Windows.Forms.GroupBox();

		this.cmdAdd = new System.Windows.Forms.Button();

		this.txtAccount = new System.Windows.Forms.TextBox();

		this.txtLastName = new System.Windows.Forms.TextBox();

		this.txtFirstName = new System.Windows.Forms.TextBox();

		this.Label3 = new System.Windows.Forms.Label();

		this.Label2 = new System.Windows.Forms.Label();

		this.Label1 = new System.Windows.Forms.Label();

		this.lstItems = new System.Windows.Forms.ListBox();

		this.cmdReload = new System.Windows.Forms.Button();

		this.mnuMain = new System.Windows.Forms.MainMenu();

		this.mnuFile = new System.Windows.Forms.MenuItem();

		this.mnuExit = new System.Windows.Forms.MenuItem();

		this.mnuHelp = new System.Windows.Forms.MenuItem();

		this.mnuAbout = new System.Windows.Forms.MenuItem();

		this.GroupBox1.SuspendLayout();

		this.SuspendLayout();

		//

		//cmdRemove

		//

		this.cmdRemove.Location = new System.Drawing.Point(272, 200);

		this.cmdRemove.Name = "cmdRemove";

		this.cmdRemove.Size = new System.Drawing.Size(160, 32);

		this.cmdRemove.TabIndex = 3;

		this.cmdRemove.Text = "&Remove";

		this.cmdRemove.Click += new EventHandler(cmdRemove_Click);

		//

		//cmdItem

		//

		this.cmdItem.Location = new System.Drawing.Point(272, 160);

		this.cmdItem.Name = "cmdItem";

		this.cmdItem.Size = new System.Drawing.Size(160, 32);

		this.cmdItem.TabIndex = 2;

		this.cmdItem.Text = "Show &Item Details";

		this.cmdItem.Click += new EventHandler(cmdItem_Click);

		//

		//GroupBox1

		//

		this.GroupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {this.cmdAdd, this.txtAccount, this.txtLastName, this.txtFirstName, this.Label3, this.Label2, this.Label1});

		this.GroupBox1.Location = new System.Drawing.Point(8, 8);

		this.GroupBox1.Name = "GroupBox1";

		this.GroupBox1.Size = new System.Drawing.Size(424, 144);

		this.GroupBox1.TabIndex = 0;

		this.GroupBox1.TabStop = false;

		this.GroupBox1.Text = "Add to Collection";

		//

		//cmdAdd

		//

		this.cmdAdd.Location = new System.Drawing.Point(304, 16);

		this.cmdAdd.Name = "cmdAdd";

		this.cmdAdd.Size = new System.Drawing.Size(112, 32);

		this.cmdAdd.TabIndex = 6;

		this.cmdAdd.Text = "A&dd";
		this.cmdAdd.Click += new EventHandler(cmdAdd_Click);

		//

		//txtAccount

		//

		this.txtAccount.Location = new System.Drawing.Point(180, 105);

		this.txtAccount.Name = "txtAccount";

		this.txtAccount.TabIndex = 5;

		this.txtAccount.Text = string.Empty;

		//

		//txtLastName

		//

		this.txtLastName.Location = new System.Drawing.Point(180, 73);

		this.txtLastName.Name = "txtLastName";

		this.txtLastName.TabIndex = 3;

		this.txtLastName.Text = string.Empty;

		//

		//txtFirstName

		//

		this.txtFirstName.Location = new System.Drawing.Point(180, 41);

		this.txtFirstName.Name = "txtFirstName";

		this.txtFirstName.TabIndex = 1;

		this.txtFirstName.Text = string.Empty;

		//

		//Label3

		//

		this.Label3.Location = new System.Drawing.Point(20, 105);

		this.Label3.Name = "Label3";

		this.Label3.Size = new System.Drawing.Size(152, 23);

		this.Label3.TabIndex = 4;

		this.Label3.Text = "&Account Number:";

		this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

		//

		//Label2

		//

		this.Label2.Location = new System.Drawing.Point(20, 73);

		this.Label2.Name = "Label2";

		this.Label2.Size = new System.Drawing.Size(152, 23);

		this.Label2.TabIndex = 2;

		this.Label2.Text = "&Last Name:";

		this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

		//

		//Label1

		//

		this.Label1.Location = new System.Drawing.Point(20, 41);

		this.Label1.Name = "Label1";

		this.Label1.Size = new System.Drawing.Size(152, 23);

		this.Label1.TabIndex = 0;

		this.Label1.Text = "First &Name:";

		this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

		//

		//lstItems

		//

		this.lstItems.Location = new System.Drawing.Point(8, 160);

		this.lstItems.Name = "lstItems";

		this.lstItems.Size = new System.Drawing.Size(256, 121);

		this.lstItems.TabIndex = 1;

		//

		//cmdReload

		//

		this.cmdReload.Location = new System.Drawing.Point(272, 240);

		this.cmdReload.Name = "cmdReload";

		this.cmdReload.Size = new System.Drawing.Size(160, 32);

		this.cmdReload.TabIndex = 4;

		this.cmdReload.Text = "Re&load list from collection";

		this.cmdReload.Click += new EventHandler(cmdReload_Click);

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

		//frmMain

		//

		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);

		this.ClientSize = new System.Drawing.Size(440, 305);

		this.Controls.AddRange(new System.Windows.Forms.Control[] {this.cmdReload, this.lstItems, this.GroupBox1, this.cmdItem, this.cmdRemove});

		this.Icon = (System.Drawing.Icon) resources.GetObject("$this.Icon");

		this.Name = "frmMain";

		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

		this.Text = "Title Comes from Assembly Info";

		this.GroupBox1.ResumeLayout(false);

		this.ResumeLayout(false);
		this.Load+=new EventHandler(frmMain_Load);

	

	}

#endregion

#region " Standard Menu Code "

    

    

    // This code simply shows the About form.

	[System.Diagnostics.DebuggerStepThroughAttribute()] private void mnuAbout_Click(object sender, System.EventArgs e)
	{
		// Open the About form in Dialog Mode

		frmAbout frm = new frmAbout();

		frm.ShowDialog(this);

		frm.Dispose();

	}

	// This code will close the form.

	[System.Diagnostics.DebuggerStepThroughAttribute()] private void mnuFileClose_Click(object sender, System.EventArgs e)
	{
		// Close the current form

		this.Close();

	}

#endregion

    private void cmdAdd_Click(object sender, System.EventArgs e)
	{

		Customer cust;

		int listNumber;

        cust = myCustomers.Add(txtFirstName.Text, txtLastName.Text, txtAccount.Text);

		listNumber = lstItems.Items.Add(cust);

        lstItems.Refresh();

		txtFirstName.Text = string.Empty;

		txtLastName.Text = string.Empty;

		txtAccount.Text = string.Empty;

	}

	private void cmdItem_Click(object sender, System.EventArgs e)
	{
		Customer listCustomer;

		Customer cust;

		if (lstItems.SelectedIndex > -1) 
			{

			listCustomer = (Customer) lstItems.SelectedItem;

			// From here, we COULD simply display the information

			// by using the listCustomer object instead of looking

			// up the primary copy in the Customers collection.

			//

			// However, we will be looking up the item in the 

			// Customers collection since we went through the trouble

			// of creating the collection to begin with. Also to show

			// how it is done.

			try 
			{

				cust = myCustomers.Item(listCustomer);

				MessageBox.Show(cust.AccountNumber + " " + cust.FirstName + " " + cust.LastName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception exp) 
			{

				MessageBox.Show(exp.Message, exp.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);

			}

		}

	}

	private void cmdReload_Click(object sender, System.EventArgs e)
	{
		//Customer cust;

		lstItems.Items.Clear();

		//lstItems.DisplayMember = "AccountNumber"

		foreach (Customer cust in myCustomers)
		{
			lstItems.Items.Add(cust);
		}

	}

	private void cmdRemove_Click(object sender, System.EventArgs e)
	{

		Customer listCustomer;

		Customer cust;

		if (lstItems.SelectedIndex > -1) {

			listCustomer = (Customer) lstItems.SelectedItem;

			// From here, we COULD simply remove the collection

			// item by using the listCustomer object instead of

			// looking up the primary copy in the Customers 

			// collection.

			//

			// However, we will be looking up the item in the 

			// Customers collection since we went through the trouble

			// of creating the collection to begin with. Also to show

			// how it is done.

			try 
			{

				cust = myCustomers.Item(listCustomer);

				myCustomers.Remove(cust);

				lstItems.Items.Remove(listCustomer);
			}

			catch (Exception exp) 
			{

				MessageBox.Show(exp.Message, exp.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);

			}

		}

	}

	private void frmMain_Load(object sender, System.EventArgs e) 
	{

		Customer cust;

		int listNumber;

		// When adding an object to a listbox, you must

		// set the listbox "DisplayMember" property BEFORE

		// adding the object. This will allow the listbox

		// to display whatever property of the added object

		// you want.

		lstItems.DisplayMember = "DisplayData";

		// Remove the comment below to change the binded property

		//lstItems.DisplayMember = "AccountNumber"

		// Load Some Samples Customers

		cust = myCustomers.Add("Tom", "Slick", "1234567890");

		listNumber = lstItems.Items.Add(cust);

		cust = myCustomers.Add("Bob", "Dabs", "9812462424");

		listNumber = lstItems.Items.Add(cust);

		cust = myCustomers.Add("Mike", "Chevel", "5783574123");

		listNumber = lstItems.Items.Add(cust);

	}

}

