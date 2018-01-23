//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).
// Since the survey form uses an XML document to provide information
//   the System.Xml namespace is imported for simplicity.

using System;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;

public class frmMain: System.Windows.Forms.Form 
{

	// Create constants to use in the form.

	private const int CONTROL_WIDTH= 300;
	private const int CHARS_PER_LINE = 30;
	private const int HEIGHT_PER_LINE= 19;

	// Create class variables to use during the form.

	private int m_ControlCount = 0;
	private Point m_Location = new Point(10, 10);

	

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
		// So that we only need to set the title of the application once,
		// we use the AssemblyInfo class (defined in the AssemblyInfo.cs file)
		// to read the AssemblyTitle attribute.
		GetAssemblyAttributes();
	}

	private void GetAssemblyAttributes()
	{
		AssemblyInfo ainfo = new AssemblyInfo();
		this.Text = ainfo.Title;
		this.mnuAbout.Text = string.Format("&About {0} ...", ainfo.Title);
	}

	//Form overrides dispose to clean up the component list.
	protected override void Dispose(bool disposing)
	{
			if (disposing) 
			 {
				 if (components != null) 
				 {
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
	private System.Windows.Forms.Button btnClearControls;
	private System.Windows.Forms.Button btnTightlyBoundControls;
	private System.Windows.Forms.Button btnAddButton;
	private System.Windows.Forms.Button btnCreateSurvey;
	private System.Windows.Forms.Label Label1;
	private System.Windows.Forms.Label Label2;
	private System.Windows.Forms.Label Label3;
	private System.Windows.Forms.Label Label4;


	private void InitializeComponent() 
	{
		System.Resources.ResourceManager resources  = new System.Resources.ResourceManager(typeof(frmMain));
		this.mnuMain = new System.Windows.Forms.MainMenu();
		this.mnuFile = new System.Windows.Forms.MenuItem();
		this.mnuExit = new System.Windows.Forms.MenuItem();
		this.mnuHelp = new System.Windows.Forms.MenuItem();
		this.mnuAbout = new System.Windows.Forms.MenuItem();
		this.btnAddButton = new System.Windows.Forms.Button();
		this.btnClearControls = new System.Windows.Forms.Button();
		this.btnTightlyBoundControls = new System.Windows.Forms.Button();
		this.btnCreateSurvey = new System.Windows.Forms.Button();
		this.Label1 = new System.Windows.Forms.Label();
		this.Label2 = new System.Windows.Forms.Label();
		this.Label3 = new System.Windows.Forms.Label();
		this.Label4 = new System.Windows.Forms.Label();
		this.SuspendLayout();
		//
		//mnuMain
		//
		this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuFile, this.mnuHelp});
		this.mnuMain.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("mnuMain.RightToLeft");
		//
		//mnuFile
		//
		this.mnuFile.Enabled = (bool) resources.GetObject("mnuFile.Enabled");
		this.mnuFile.Index = 0;
		this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuExit});
		this.mnuFile.Shortcut = (System.Windows.Forms.Shortcut) resources.GetObject("mnuFile.Shortcut");
		this.mnuFile.ShowShortcut = (bool) resources.GetObject("mnuFile.ShowShortcut");
		this.mnuFile.Text = resources.GetString("mnuFile.Text");
		this.mnuFile.Visible = (bool) resources.GetObject("mnuFile.Visible");
		//
		//mnuExit
		//
		this.mnuExit.Enabled = (bool) resources.GetObject("mnuExit.Enabled");
		this.mnuExit.Index = 0;
		this.mnuExit.Shortcut = (System.Windows.Forms.Shortcut) resources.GetObject("mnuExit.Shortcut");
		this.mnuExit.ShowShortcut = (bool) resources.GetObject("mnuExit.ShowShortcut");
		this.mnuExit.Text = resources.GetString("mnuExit.Text");
		this.mnuExit.Visible = (bool) resources.GetObject("mnuExit.Visible");
		//
		//mnuHelp
		//
		this.mnuHelp.Enabled = (bool) resources.GetObject("mnuHelp.Enabled");
		this.mnuHelp.Index = 1;
		this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuAbout});
		this.mnuHelp.Shortcut = (System.Windows.Forms.Shortcut) resources.GetObject("mnuHelp.Shortcut");
		this.mnuHelp.ShowShortcut = (bool) resources.GetObject("mnuHelp.ShowShortcut");
		this.mnuHelp.Text = resources.GetString("mnuHelp.Text");
		this.mnuHelp.Visible = (bool) resources.GetObject("mnuHelp.Visible");
		//
		//mnuAbout
		//
		this.mnuAbout.Enabled = (bool) resources.GetObject("mnuAbout.Enabled");
		this.mnuAbout.Index = 0;
		this.mnuAbout.Shortcut = (System.Windows.Forms.Shortcut) resources.GetObject("mnuAbout.Shortcut");
		this.mnuAbout.ShowShortcut = (bool) resources.GetObject("mnuAbout.ShowShortcut");
		this.mnuAbout.Text = resources.GetString("mnuAbout.Text");
		this.mnuAbout.Visible = (bool) resources.GetObject("mnuAbout.Visible");
		//
		//btnAddButton
		//
		this.btnAddButton.AccessibleDescription = resources.GetString("btnAddButton.AccessibleDescription");
		this.btnAddButton.AccessibleName = resources.GetString("btnAddButton.AccessibleName");
		this.btnAddButton.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("btnAddButton.Anchor");
		this.btnAddButton.BackgroundImage = (System.Drawing.Image) resources.GetObject("btnAddButton.BackgroundImage");
		this.btnAddButton.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("btnAddButton.Dock");
		this.btnAddButton.Enabled = (bool) resources.GetObject("btnAddButton.Enabled");
		this.btnAddButton.FlatStyle = (System.Windows.Forms.FlatStyle) resources.GetObject("btnAddButton.FlatStyle");
		this.btnAddButton.Font = (System.Drawing.Font) resources.GetObject("btnAddButton.Font");
		this.btnAddButton.Image = (System.Drawing.Image) resources.GetObject("btnAddButton.Image");
		this.btnAddButton.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnAddButton.ImageAlign");
		this.btnAddButton.ImageIndex = (int) resources.GetObject("btnAddButton.ImageIndex");
		this.btnAddButton.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("btnAddButton.ImeMode");
		this.btnAddButton.Location = (System.Drawing.Point) resources.GetObject("btnAddButton.Location");
		this.btnAddButton.Name = "btnAddButton";
		this.btnAddButton.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("btnAddButton.RightToLeft");
		this.btnAddButton.Size = (System.Drawing.Size) resources.GetObject("btnAddButton.Size");
		this.btnAddButton.TabIndex = (int) resources.GetObject("btnAddButton.TabIndex");
		this.btnAddButton.Text = resources.GetString("btnAddButton.Text");
		this.btnAddButton.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnAddButton.TextAlign");
		this.btnAddButton.Visible = (bool) resources.GetObject("btnAddButton.Visible");
		//
		//btnClearControls
		//
		this.btnClearControls.AccessibleDescription = resources.GetString("btnClearControls.AccessibleDescription");
		this.btnClearControls.AccessibleName = resources.GetString("btnClearControls.AccessibleName");
		this.btnClearControls.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("btnClearControls.Anchor");
		this.btnClearControls.BackgroundImage = (System.Drawing.Image) resources.GetObject("btnClearControls.BackgroundImage");
		this.btnClearControls.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("btnClearControls.Dock");
		this.btnClearControls.Enabled = (bool) resources.GetObject("btnClearControls.Enabled");
		this.btnClearControls.FlatStyle = (System.Windows.Forms.FlatStyle) resources.GetObject("btnClearControls.FlatStyle");
		this.btnClearControls.Font = (System.Drawing.Font) resources.GetObject("btnClearControls.Font");
		this.btnClearControls.Image = (System.Drawing.Image) resources.GetObject("btnClearControls.Image");
		this.btnClearControls.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnClearControls.ImageAlign");
		this.btnClearControls.ImageIndex = (int) resources.GetObject("btnClearControls.ImageIndex");
		this.btnClearControls.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("btnClearControls.ImeMode");
		this.btnClearControls.Location = (System.Drawing.Point) resources.GetObject("btnClearControls.Location");
		this.btnClearControls.Name = "btnClearControls";
		this.btnClearControls.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("btnClearControls.RightToLeft");
		this.btnClearControls.Size = (System.Drawing.Size) resources.GetObject("btnClearControls.Size");
		this.btnClearControls.TabIndex = (int) resources.GetObject("btnClearControls.TabIndex");
		this.btnClearControls.Text = resources.GetString("btnClearControls.Text");
		this.btnClearControls.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnClearControls.TextAlign");
		this.btnClearControls.Visible = (bool) resources.GetObject("btnClearControls.Visible");
		//
		//btnTightlyBoundControls
		//
		this.btnTightlyBoundControls.AccessibleDescription = resources.GetString("btnTightlyBoundControls.AccessibleDescription");
		this.btnTightlyBoundControls.AccessibleName = resources.GetString("btnTightlyBoundControls.AccessibleName");
		this.btnTightlyBoundControls.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("btnTightlyBoundControls.Anchor");
		this.btnTightlyBoundControls.BackgroundImage = (System.Drawing.Image) resources.GetObject("btnTightlyBoundControls.BackgroundImage");
		this.btnTightlyBoundControls.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("btnTightlyBoundControls.Dock");
		this.btnTightlyBoundControls.Enabled = (bool) resources.GetObject("btnTightlyBoundControls.Enabled");
		this.btnTightlyBoundControls.FlatStyle = (System.Windows.Forms.FlatStyle) resources.GetObject("btnTightlyBoundControls.FlatStyle");
		this.btnTightlyBoundControls.Font = (System.Drawing.Font) resources.GetObject("btnTightlyBoundControls.Font");
		this.btnTightlyBoundControls.Image = (System.Drawing.Image) resources.GetObject("btnTightlyBoundControls.Image");
		this.btnTightlyBoundControls.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnTightlyBoundControls.ImageAlign");
		this.btnTightlyBoundControls.ImageIndex = (int) resources.GetObject("btnTightlyBoundControls.ImageIndex");
		this.btnTightlyBoundControls.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("btnTightlyBoundControls.ImeMode");
		this.btnTightlyBoundControls.Location = (System.Drawing.Point) resources.GetObject("btnTightlyBoundControls.Location");
		this.btnTightlyBoundControls.Name = "btnTightlyBoundControls";
		this.btnTightlyBoundControls.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("btnTightlyBoundControls.RightToLeft");
		this.btnTightlyBoundControls.Size = (System.Drawing.Size) resources.GetObject("btnTightlyBoundControls.Size");
		this.btnTightlyBoundControls.TabIndex = (int) resources.GetObject("btnTightlyBoundControls.TabIndex");
		this.btnTightlyBoundControls.Text = resources.GetString("btnTightlyBoundControls.Text");
		this.btnTightlyBoundControls.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnTightlyBoundControls.TextAlign");
		this.btnTightlyBoundControls.Visible = (bool) resources.GetObject("btnTightlyBoundControls.Visible");
		//
		//btnCreateSurvey
		//
		this.btnCreateSurvey.AccessibleDescription = resources.GetString("btnCreateSurvey.AccessibleDescription");
		this.btnCreateSurvey.AccessibleName = resources.GetString("btnCreateSurvey.AccessibleName");
		this.btnCreateSurvey.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("btnCreateSurvey.Anchor");
		this.btnCreateSurvey.BackgroundImage = (System.Drawing.Image) resources.GetObject("btnCreateSurvey.BackgroundImage");
		this.btnCreateSurvey.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("btnCreateSurvey.Dock");
		this.btnCreateSurvey.Enabled = (bool) resources.GetObject("btnCreateSurvey.Enabled");
		this.btnCreateSurvey.FlatStyle = (System.Windows.Forms.FlatStyle) resources.GetObject("btnCreateSurvey.FlatStyle");
		this.btnCreateSurvey.Font = (System.Drawing.Font) resources.GetObject("btnCreateSurvey.Font");
		this.btnCreateSurvey.Image = (System.Drawing.Image) resources.GetObject("btnCreateSurvey.Image");
		this.btnCreateSurvey.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnCreateSurvey.ImageAlign");
		this.btnCreateSurvey.ImageIndex = (int) resources.GetObject("btnCreateSurvey.ImageIndex");
		this.btnCreateSurvey.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("btnCreateSurvey.ImeMode");
		this.btnCreateSurvey.Location = (System.Drawing.Point) resources.GetObject("btnCreateSurvey.Location");
		this.btnCreateSurvey.Name = "btnCreateSurvey";
		this.btnCreateSurvey.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("btnCreateSurvey.RightToLeft");
		this.btnCreateSurvey.Size = (System.Drawing.Size) resources.GetObject("btnCreateSurvey.Size");
		this.btnCreateSurvey.TabIndex = (int) resources.GetObject("btnCreateSurvey.TabIndex");
		this.btnCreateSurvey.Text = resources.GetString("btnCreateSurvey.Text");
		this.btnCreateSurvey.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnCreateSurvey.TextAlign");
		this.btnCreateSurvey.Visible = (bool) resources.GetObject("btnCreateSurvey.Visible");
		//
		//Label1
		//
		this.Label1.AccessibleDescription = resources.GetString("Label1.AccessibleDescription");
		this.Label1.AccessibleName = resources.GetString("Label1.AccessibleName");
		this.Label1.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("Label1.Anchor");
		this.Label1.AutoSize = (bool) resources.GetObject("Label1.AutoSize");
		this.Label1.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("Label1.Dock");
		this.Label1.Enabled = (bool) resources.GetObject("Label1.Enabled");
		this.Label1.Font = (System.Drawing.Font) resources.GetObject("Label1.Font");
		this.Label1.Image = (System.Drawing.Image) resources.GetObject("Label1.Image");
		this.Label1.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label1.ImageAlign");
		this.Label1.ImageIndex = (int) resources.GetObject("Label1.ImageIndex");
		this.Label1.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("Label1.ImeMode");
		this.Label1.Location = (System.Drawing.Point) resources.GetObject("Label1.Location");
		this.Label1.Name = "Label1";
		this.Label1.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("Label1.RightToLeft");
		this.Label1.Size = (System.Drawing.Size) resources.GetObject("Label1.Size");
		this.Label1.TabIndex = (int) resources.GetObject("Label1.TabIndex");
		this.Label1.Text = resources.GetString("Label1.Text");
		this.Label1.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label1.TextAlign");
		this.Label1.Visible = (bool) resources.GetObject("Label1.Visible");
		//
		//Label2
		//
		this.Label2.AccessibleDescription = resources.GetString("Label2.AccessibleDescription");
		this.Label2.AccessibleName = resources.GetString("Label2.AccessibleName");
		this.Label2.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("Label2.Anchor");
		this.Label2.AutoSize = (bool) resources.GetObject("Label2.AutoSize");
		this.Label2.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("Label2.Dock");
		this.Label2.Enabled = (bool) resources.GetObject("Label2.Enabled");
		this.Label2.Font = (System.Drawing.Font) resources.GetObject("Label2.Font");
		this.Label2.Image = (System.Drawing.Image) resources.GetObject("Label2.Image");
		this.Label2.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label2.ImageAlign");
		this.Label2.ImageIndex = (int) resources.GetObject("Label2.ImageIndex");
		this.Label2.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("Label2.ImeMode");
		this.Label2.Location = (System.Drawing.Point) resources.GetObject("Label2.Location");
		this.Label2.Name = "Label2";
		this.Label2.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("Label2.RightToLeft");
		this.Label2.Size = (System.Drawing.Size) resources.GetObject("Label2.Size");
		this.Label2.TabIndex = (int) resources.GetObject("Label2.TabIndex");
		this.Label2.Text = resources.GetString("Label2.Text");
		this.Label2.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label2.TextAlign");
		this.Label2.Visible = (bool) resources.GetObject("Label2.Visible");
		//
		//Label3
		//
		this.Label3.AccessibleDescription = resources.GetString("Label3.AccessibleDescription");
		this.Label3.AccessibleName = resources.GetString("Label3.AccessibleName");
		this.Label3.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("Label3.Anchor");
		this.Label3.AutoSize = (bool) resources.GetObject("Label3.AutoSize");
		this.Label3.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("Label3.Dock");
		this.Label3.Enabled = (bool) resources.GetObject("Label3.Enabled");
		this.Label3.Font = (System.Drawing.Font) resources.GetObject("Label3.Font");
		this.Label3.Image = (System.Drawing.Image) resources.GetObject("Label3.Image");
		this.Label3.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label3.ImageAlign");
		this.Label3.ImageIndex = (int) resources.GetObject("Label3.ImageIndex");
		this.Label3.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("Label3.ImeMode");
		this.Label3.Location = (System.Drawing.Point) resources.GetObject("Label3.Location");
		this.Label3.Name = "Label3";
		this.Label3.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("Label3.RightToLeft");
		this.Label3.Size = (System.Drawing.Size) resources.GetObject("Label3.Size");
		this.Label3.TabIndex = (int) resources.GetObject("Label3.TabIndex");
		this.Label3.Text = resources.GetString("Label3.Text");
		this.Label3.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label3.TextAlign");
		this.Label3.Visible = (bool) resources.GetObject("Label3.Visible");
		//
		//Label4
		//
		this.Label4.AccessibleDescription = resources.GetString("Label4.AccessibleDescription");
		this.Label4.AccessibleName = resources.GetString("Label4.AccessibleName");
		this.Label4.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("Label4.Anchor");
		this.Label4.AutoSize = (bool) resources.GetObject("Label4.AutoSize");
		this.Label4.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("Label4.Dock");
		this.Label4.Enabled = (bool) resources.GetObject("Label4.Enabled");
		this.Label4.Font = (System.Drawing.Font) resources.GetObject("Label4.Font");
		this.Label4.Image = (System.Drawing.Image) resources.GetObject("Label4.Image");
		this.Label4.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label4.ImageAlign");
		this.Label4.ImageIndex = (int) resources.GetObject("Label4.ImageIndex");
		this.Label4.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("Label4.ImeMode");
		this.Label4.Location = (System.Drawing.Point) resources.GetObject("Label4.Location");
		this.Label4.Name = "Label4";
		this.Label4.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("Label4.RightToLeft");
		this.Label4.Size = (System.Drawing.Size) resources.GetObject("Label4.Size");
		this.Label4.TabIndex = (int) resources.GetObject("Label4.TabIndex");
		this.Label4.Text = resources.GetString("Label4.Text");
		this.Label4.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("Label4.TextAlign");
		this.Label4.Visible = (bool) resources.GetObject("Label4.Visible");
		//
		//frmMain
		//
		this.AccessibleDescription = (string) resources.GetObject("$this.AccessibleDescription");
		this.AccessibleName = (string) resources.GetObject("$this.AccessibleName");
		this.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("$this.Anchor");
		this.AutoScaleBaseSize = (System.Drawing.Size) resources.GetObject("$this.AutoScaleBaseSize");
		this.AutoScroll = (bool) resources.GetObject("$this.AutoScroll");
		this.AutoScrollMargin = (System.Drawing.Size) resources.GetObject("$this.AutoScrollMargin");
		this.AutoScrollMinSize = (System.Drawing.Size) resources.GetObject("$this.AutoScrollMinSize");
		this.BackgroundImage = (System.Drawing.Image) resources.GetObject("$this.BackgroundImage");
		this.ClientSize = (System.Drawing.Size) resources.GetObject("$this.ClientSize");
		this.Controls.AddRange(new System.Windows.Forms.Control[] {this.Label4, this.Label3, this.Label2, this.Label1, this.btnCreateSurvey, this.btnTightlyBoundControls, this.btnClearControls, this.btnAddButton});
		this.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("$this.Dock");
		this.Enabled = (bool) resources.GetObject("$this.Enabled");
		this.Font = (System.Drawing.Font) resources.GetObject("$this.Font");
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.Icon = (System.Drawing.Icon) resources.GetObject("$this.Icon");
		this.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("$this.ImeMode");
		this.Location = (System.Drawing.Point) resources.GetObject("$this.Location");
		this.MaximizeBox = false;
		this.MaximumSize = (System.Drawing.Size) resources.GetObject("$this.MaximumSize");
		this.Menu = this.mnuMain;
		this.MinimumSize = (System.Drawing.Size) resources.GetObject("$this.MinimumSize");
		this.Name = "frmMain";
		this.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("$this.RightToLeft");
		this.StartPosition = (System.Windows.Forms.FormStartPosition) resources.GetObject("$this.StartPosition");
		this.Text = resources.GetString("$this.Text");
		this.Visible = (bool) resources.GetObject("$this.Visible");
		this.ResumeLayout(false);

		// Add event handlers
		this.btnAddButton.Click += new System.EventHandler(this.btnAddButton_Click);
		this.btnClearControls.Click += new System.EventHandler(this.btnClearControls_Click);
		this.btnCreateSurvey.Click += new System.EventHandler(this.btnCreateSurvey_Click);
		this.btnTightlyBoundControls.Click += new System.EventHandler(this.btnTightlyBoundControls_Click);

	}

	#endregion

	#region " Standard Menu Code "

	// has been added to some procedures since they are

	// not the focus of the demo. Remove them if you wish to debug the procedures.

	// This code simply shows the About form.

	private void mnuAbout_Click(object sender, System.EventArgs e) 
	{

		// Open the About form in Dialog Mode

		frmAbout frm =  new frmAbout();

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

	// This subroutine adds a new button to the form, and sets up event handlers
	//   for it that will be fired on the Click and MouseHover events.

	private void btnAddButton_Click(object sender, System.EventArgs e)
	{
		// Increment the control count
		m_ControlCount += 1;
		// Only allow 5 buttons, just to simplify drawing of the user interface
		if (m_ControlCount <= 5) 
		{
			// Create a new Button
			Button x = new Button();
			// Add properties to the form
			x.Name = "btn" + m_ControlCount.ToString();
			x.Text = "btn" + m_ControlCount.ToString();
			x.Location = new Point(this.m_Location.X + 250, this.m_Location.Y);
			m_Location.Y += x.Height + 5;
			// Add the two event handlers  
			x.Click += new System.EventHandler(myButtonHandler_Click);
			x.MouseHover += new System.EventHandler(myButtonHandler_MouseHover);
			// Add the control to the collection of controls
			Controls.Add(x);
		}
		else 
		{
			// Just allow 5 controls to simplify UI.

			MessageBox.Show("You've reached 5 controls. Clear controls to start again.",this.Text, MessageBoxButtons.OK);

		}

	}

	// This subroutine clears all of the dynamically generated controls 

	//   on the page.  It does this by removing all of the controls, then calling

	//   the InitializeComponent() subroutine that was automatically generated

	//   by Visual Studio .NET.  This is a very easy way of resetting a form to

	//   its original state.

	private void btnClearControls_Click(object sender, System.EventArgs e)
		{
		// Clear all the controls.
		Controls.Clear();
		// Re-Add all of the original controls
		InitializeComponent();
		// Re-set properties such as the app title from Assembly attributes
		GetAssemblyAttributes();
		// Reset the m_Location to its original location.
		m_Location = new Point(10, 10);
		// Reset the number of controls.
		m_ControlCount = 0;
		// Show the form again.
		Show();
	}

    // This subroutine handles the btnCreateSurvey.Click event and creates
    //   a new frmSurveyForm. The controls that are generated are added to the
    //   created survey form. There are no event handlers associated with the 
    //   created controls.
    // The created form is fairly general, and creates a survey with questions
    //   that are based off information provided by the Questions.xml document.
    //   By changing, adding, or removing nodes in the XML document, you can 
    //   change the structure and form of the survey.

	private void btnCreateSurvey_Click(object sender, System.EventArgs e)
	{
		// Create a new Survey Form to display to the User.
		frmSurveyForm survey = new frmSurveyForm();
		// get {the controls collection of the Survey form.
		Control.ControlCollection surveyControls = survey.SurveyFormControls;
		// Reset the m_Location in case the user creates multiple forms
		m_Location = new Point(10, 10);
		// Create an XML document to read in the survey questions
		XmlDocument xr = new XmlDocument();
		xr.Load(@"..\..\Questions.xml");
		// get {the Tag used for each of the Controls we'll create. This may
		//   be useful later, if the example was extended to break apart
		//   different types of questions/responses for analysis.
		string myTag  = xr.SelectSingleNode("//survey").Attributes["name"].Value;
		// set {the Title on the survey form to the Display Name of the Survey
		survey.SurveyTitle = xr.SelectSingleNode("//survey").Attributes["displayName"].Value;
		// Create an XmlNodeList to contain each of the questions. Fill it.
		XmlNodeList nodeList;
		nodeList = xr.GetElementsByTagName("question");
		// Create a temporary XML Node to use when retrieving information
		//   about the nodes in the nodeList just created.
		foreach(XmlNode myNode in nodeList)
		{
			if (myNode.Attributes != null) 
			{
				// Determine what type of control should be created. Pass
				//   in the required information, including the Controls collection
				//   from the frmSurveyForm form.
				switch (myNode.Attributes["type"].Value)
				{
					case "dropdown":
						m_Location = Survey_AddComboBox(myNode, surveyControls, m_Location, myTag);
						break;
					case "multilist":
						m_Location = Survey_AddListBox(myNode, surveyControls, m_Location, myTag, true);
						break;
					case "text":
						m_Location = Survey_AddTextBox(myNode, surveyControls, m_Location, myTag);
						break;
					case "radio":
						m_Location = Survey_AddRadioButtons(myNode, surveyControls, m_Location, myTag);
						break;
				}
			}
		}

		// set {the size of the form, based off of how many controls
		//   have been placed on the form, and their dimensions.
		survey.Width = m_Location.X + CONTROL_WIDTH + 30;
		// Add a bit extra to leave room for the OK and Cancel buttons.
		survey.Height = m_Location.Y + 75;
		// Show the form.  You can also use the Show() method if you like.
		survey.ShowDialog();
		// Show the response to the user.
		MessageBox.Show(survey.SurveyResponse, this.Text, MessageBoxButtons.OK);

	}

    // This subroutine handles the btnTightlyBoundControls.Click event and creates
    //   two tightly bound controls. It uses the event handlers that have been 
    //   previously defined to handle the events. These event handlers are 
    //   have to be defined beforehand, unless Reflection.Emit is used.
    // The two controls are a Button and a TextBox. When the Button is pressed, the
    //   text in the TextBox is displayed in a MsgBox.  In order to ensure that
    //   we know which TextBox is being used, it is added to the Tag property of
    //   the Button. (We don't know the name of the TextBox while creating the 
    //   event handler, since the TextBox will be created dynamically.

    private void btnTightlyBoundControls_Click(object sender, System.EventArgs e)
		{
        // Increment the number of controls (only by one, even though two 
        //   will be added.
        m_ControlCount += 1;
        // Only allow 5 buttons, just to simplify drawing of the user interface
		if (m_ControlCount <= 5) 
		{
			// Create the TextBox that will contain the text to speak
			TextBox txtSpeakText =  new TextBox();
			// set {up some properties for the TextBox
			txtSpeakText.Text = "Hello, World";
			txtSpeakText.Name = "txtSpeakText";
			txtSpeakText.Location = new Point(m_Location.X + 250, m_Location.Y);
			txtSpeakText.Size = new Size(200, txtSpeakText.Height);
			// Add the TextBox to the controls collection.
			Controls.Add(txtSpeakText);
			// Increment the m_LocationY so the next control won't overwrite it
			m_Location.Y += txtSpeakText.Height + 5;
			// Create a button that will be used to react to clicks
			// Since this button is tightly coupled to the TextBox which will
			//   provide it with the text to display, we'll add the TextBox created
			//   above as the Tag of this Button. 
			Button btnSpeakText = new Button();
			// set {up some properties for the TextBox
			btnSpeakText.Text = "Speak Text";
			btnSpeakText.Name = "btnSpeakText";
			btnSpeakText.Location = new Point(m_Location.X + 250, m_Location.Y);
			btnSpeakText.Size = new Size(100, btnSpeakText.Height);
			// Add the previously created TextBox to this button
			btnSpeakText.Tag = txtSpeakText;
			// Add the TextBox to the controls collection.
			Controls.Add(btnSpeakText);
			// Increment the m_LocationY so the next control won't overwrite it
			m_Location.Y += btnSpeakText.Height + 5;
			// Add the event handler that will handle the event when the
			//   button is pressed.
			btnSpeakText.Click += new System.EventHandler(SpeakTextClickHandler);
		}
		else 
		{
			// Just allow 5 controls to simplify UI.
			MessageBox.Show("You've reached 5 controls. Clear controls to start again.", this.Text, MessageBoxButtons.OK);
		}
    }

    // This subroutine handles the Click events of all the dynamically generated
    //   buttons.  It is attached to all the buttons using the AddHandler function
    //   at the time of button creation.

    private void myButtonHandler_Click(object sender ,EventArgs e)
	{
        // Verify that the type of control triggering this event is indeed
        //   a Button. This is necessary since this handler can be attached
        //   to any event.
		Button btn = sender as Button;
		if (btn != null) 
		{
            // Let the user know what Button was pressed.
            MessageBox.Show(btn.Text + " was pressed!", this.Text,MessageBoxButtons.OK);
        }
    }

    // This subroutine handles the MouseHover events of all the dynamically generated
    //   buttons.  It is attached to all the buttons using the AddHandler function
    //   at the time of button creation.

    private void myButtonHandler_MouseHover(Object sender ,EventArgs e)
	{

        // Verify that the type of control triggering this event is indeed
        //   a Button. This is necessary since this handler can be attached
        //   to any event.

		Button btn = sender as Button;
        if (btn != null) {
            // Let the user know what Button was hovered over.
            MessageBox.Show( btn.Text + " was hovered over!", this.Text, MessageBoxButtons.OK);
        }
    }

    // This subroutine handles the Click event of the Button created in the
    //   tightly bound controls example. It displays in a MsgBox the text that
    //   is inside of the Tag of the Button (which is provided in the 'sender' 
    //   parameter.  Although those event handlers are sophisticated, 
    //   they do have to be defined beforehand, unless Reflection.Emit is used.

    private void SpeakTextClickHandler(object sender, System.EventArgs e)
	{
        // Check to see if the sender is a button, and that it has
        //   a valid, tightly-coupled TextBox object attached as its
        //   Tag property.
		Button btn = sender as Button;
		if (btn != null) 
		{
            // Create a button object to use in its place
            // Check to see if the Button has a TextBox in its Tag property
			TextBox tb = btn.Tag as TextBox;
            if (tb != null) 
				{
                // Display the text to the user
                MessageBox.Show(((TextBox)(btn.Tag)).Text, this.Text,MessageBoxButtons.OK);
            }
        }
    }

    // This function adds a ComboBox to the passed control collection, along

    //   with an associated Label control to display the survey question.

    private Point Survey_AddComboBox(XmlNode inNode ,Control.ControlCollection inControls,
			Point location ,string tag)
	{

        // Create a new control.
        ComboBox myCombo = new ComboBox();
        // set {up some properties for the control
        myCombo.Text = "";
        myCombo.Name = inNode.Attributes["name"].Value;
        myCombo.Tag = tag;
        myCombo.Width = CONTROL_WIDTH;
        // Create a temporary XML Node to use when retrieving information
        //   about the response nodes in the passed node.
        // get {the response nodes.
        foreach(XmlNode myNode in inNode.SelectNodes("responses/response"))
								  {
            // Add the InnerText of the response nodes as the values for
            //   the drop down options.
            myCombo.Items.Add(myNode.InnerText);
            // if a default has been specified, use it as the current text.
            if (myNode.Attributes["default"] != null) {
                if (myNode.Attributes["default"].Value == "true") 
					{
                    myCombo.Text = myNode.InnerText;
                }
            }
        }

        // Create a Label and add it to the collection

        Label myLabel = new Label();
        // set {up some properties for the control
        myLabel.Name = myCombo.Name + "Label";
        myLabel.Text = inNode.SelectSingleNode("text").InnerText;
        myLabel.Width = CONTROL_WIDTH;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myLabel.Location = location;
        inControls.Add(myLabel);
        location.Y += myLabel.Height;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myCombo.Location = location;
        inControls.Add(myCombo);
        location.Y += myCombo.Height + 10;
        // Send back the location for the next control to be added.
        return location;
    }


    // This function adds a ListBox to the passed control collection, along
    //   with an associated Label control to display the survey question.

    private Point Survey_AddListBox(XmlNode inNode , Control.ControlCollection inControls ,
        Point location ,string tag, bool isMultiSelect)
	{
        // Create a new control.
        ListBox myList =  new ListBox();
        // set {up some properties for the control
        myList.Text = "";
        myList.Name = inNode.Attributes["name"].Value;
        myList.Tag = tag;
        myList.Width = CONTROL_WIDTH;
        // Since this function can be used with either multi or single select
        //   list boxes, set the proper SelectionMode based on the passed
        //   isMultiSelect bool variable.
		if (isMultiSelect) 
		{
			myList.SelectionMode = SelectionMode.MultiSimple;
		}
		else 
		{
			myList.SelectionMode = SelectionMode.One;
		}
        // Create a temporary XML Node to use when retrieving information
        //   about the response nodes in the passed node.
        // Add the InnerText of the response nodes as the values for
        //   the list box options.
        foreach(XmlNode myNode in inNode.SelectNodes("responses/response"))
			{
            myList.Items.Add(myNode.InnerText);
            // if a default has been specified, use it as the current text.
            if (myNode.Attributes["default"] != null) 
				{
                if (myNode.Attributes["default"].Value == "true") {
                    myList.Text = myNode.InnerText;
                }
            }
        }
        // Create a Label and add it to the collection
        Label myLabel = new Label();
        // set {up some properties for the control
        myLabel.Name = myList.Name + "Label";
        myLabel.Text = inNode.SelectSingleNode("text").InnerText;
        myLabel.Width = CONTROL_WIDTH;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myLabel.Location = location;
        inControls.Add(myLabel);
        location.Y += myLabel.Height;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myList.Location = location;
        inControls.Add(myList);
        location.Y += myList.Height + 10;
        // Send back the location for the next control
        return location;
    }
    // This function adds a GroupBox to the passed control collection, along
    //   with all the appropriate radio buttons, one for each available response.
    //   It also adds an associated Label control to display the survey question.

    private Point Survey_AddRadioButtons(XmlNode inNode, Control.ControlCollection inControls ,
        Point location ,string tag)
		{
        // Must create a GroupBox to contain the radio buttons
        //   otherwise they are not logically distinct from the other
        //   radio buttons on the form.
        GroupBox myGroupBox = new GroupBox();
        // set {up some properties for the control.
        myGroupBox.Text = "";
        myGroupBox.Name = inNode.Attributes["name"].Value;
        myGroupBox.Tag = tag;
        myGroupBox.Width = CONTROL_WIDTH + 20;
        // Create some useful variables to use in the following block of code.
        Point myRadioPoint = new Point(5, 10);    
        // Loop through each response, and add it as a new radio button.
        foreach(XmlNode myNode in inNode.SelectNodes("responses/response"))
		{
            // Create the radio button.
            RadioButton myRadio = new RadioButton();
            // Add the appropriate properties.
            myRadio.Text = myNode.InnerText;
            myRadio.Location = myRadioPoint;
            myRadioPoint.Y += myRadio.Height;
            // set {the default value as the selected radio button, but
            //   only if the default attribute exists and is set to true.
            if (myNode.Attributes["default"] != null) 
				{
                if (myNode.Attributes["default"].Value == "true") 
				{
                    myRadio.Checked = true;
                }
            }
            // Add the control to the group box.
            myGroupBox.Controls.Add(myRadio);
        }
        // Reset the height for the textbox, based on the 
        //   contained Radio Buttons.
        myGroupBox.Height = myRadioPoint.Y + 5;
        // Create a Label and add it to the collection.
        Label myLabel = new Label();
        // Fix the label properties.
        myLabel.Name = myGroupBox.Name + "Label";
        myLabel.Text = inNode.SelectSingleNode("text").InnerText;
        myLabel.Width = CONTROL_WIDTH;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myLabel.Location = location;
        inControls.Add(myLabel);
        location.Y += myLabel.Height - 5;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myGroupBox.Location = location;
        inControls.Add(myGroupBox);
        location.Y += myGroupBox.Height + 10;
        // Send back the location for the next control
        return location;
    }
    // This function adds a TextBox to the passed control collection, along
    //   with an associated Label control to display the survey question.

    private Point Survey_AddTextBox(XmlNode inNode , Control.ControlCollection inControls,
        Point location ,string tag) 
{
        // Create a new control.
        TextBox myText = new TextBox();
        // Fill in some of the appropriate properties
        if (inNode.SelectSingleNode("defaultResponse") != null) 
			{
            myText.Text = inNode.SelectSingleNode("defaultResponse").InnerText;
        }
        if (inNode.Attributes["name"] != null) {
            myText.Name = inNode.Attributes["name"].Value;
        }
        myText.Tag = tag;
        myText.Width = CONTROL_WIDTH;
        // set {the MaxLength property based off of the XML node information.
        if (inNode.SelectSingleNode("maxCharacters") != null) {
            myText.MaxLength = int.Parse(inNode.SelectSingleNode("maxCharacters").InnerText);
        }
        // Calculate the number of lines that should be allowed for
        if (myText.MaxLength > 0) 
			{
            int numLines  = (myText.MaxLength / CHARS_PER_LINE ) + 1;
            // Calculate how large the textbox should be, and whether 
            //   scrollbars are necessary.
			if (numLines == 1) 
			{
				myText.Multiline = false;
			}
			else 
			{
				if (numLines >= 4) 
				{
					myText.Multiline = true;
					myText.Height = 4 * HEIGHT_PER_LINE;
					myText.ScrollBars = ScrollBars.Vertical;
				}
				else 
				{
					myText.Multiline = true;
					myText.Height = numLines * HEIGHT_PER_LINE;
					myText.ScrollBars = ScrollBars.None;
				}
			}
        }

        // Create a Label and add it to the collection
        Label myLabel = new Label();
        myLabel.Name = myText.Name + "Label";
        if (inNode.SelectSingleNode("text") != null) {
            myLabel.Text = inNode.SelectSingleNode("text").InnerText;
        }
        myLabel.Width = CONTROL_WIDTH;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myLabel.Location = location;
        inControls.Add(myLabel);
        location.Y += myLabel.Height;
        // Add the control to the Controls collection, and reset
        //   the location to the location for the next control.
        myText.Location = location;
        inControls.Add(myText);
        location.Y += myText.Height + 10;
        // Send back the location for the next control.
        return location;

    }

}

