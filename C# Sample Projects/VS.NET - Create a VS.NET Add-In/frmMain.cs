//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;
using System.Windows.Forms;

// This class provides the main user interface for the How-To.

public class frmMain: System.Windows.Forms.Form {

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
    private System.Windows.Forms.MainMenu mnuMain;
    private System.Windows.Forms.MenuItem mnuFile;
    private System.Windows.Forms.MenuItem mnuExit;
    private System.Windows.Forms.MenuItem mnuHelp;
    private System.Windows.Forms.MenuItem mnuAbout;
    private System.Windows.Forms.Button btnCheckForAddIn;
    private System.Windows.Forms.TextBox txtInstalled;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.Label lblLogDescription;

    private void InitializeComponent() {
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
		this.mnuMain = new System.Windows.Forms.MainMenu();
		this.mnuFile = new System.Windows.Forms.MenuItem();
		this.mnuExit = new System.Windows.Forms.MenuItem();
		this.mnuHelp = new System.Windows.Forms.MenuItem();
		this.mnuAbout = new System.Windows.Forms.MenuItem();
		this.lblDescription = new System.Windows.Forms.Label();
		this.lblLogDescription = new System.Windows.Forms.Label();
		this.btnCheckForAddIn = new System.Windows.Forms.Button();
		this.txtInstalled = new System.Windows.Forms.TextBox();
		this.lblStatus = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// mnuMain
		// 
		this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				this.mnuFile,
																				this.mnuHelp});
		this.mnuMain.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("mnuMain.RightToLeft")));
		// 
		// mnuFile
		// 
		this.mnuFile.Enabled = ((bool)(resources.GetObject("mnuFile.Enabled")));
		this.mnuFile.Index = 0;
		this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				this.mnuExit});
		this.mnuFile.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("mnuFile.Shortcut")));
		this.mnuFile.ShowShortcut = ((bool)(resources.GetObject("mnuFile.ShowShortcut")));
		this.mnuFile.Text = resources.GetString("mnuFile.Text");
		this.mnuFile.Visible = ((bool)(resources.GetObject("mnuFile.Visible")));
		// 
		// mnuExit
		// 
		this.mnuExit.Enabled = ((bool)(resources.GetObject("mnuExit.Enabled")));
		this.mnuExit.Index = 0;
		this.mnuExit.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("mnuExit.Shortcut")));
		this.mnuExit.ShowShortcut = ((bool)(resources.GetObject("mnuExit.ShowShortcut")));
		this.mnuExit.Text = resources.GetString("mnuExit.Text");
		this.mnuExit.Visible = ((bool)(resources.GetObject("mnuExit.Visible")));
		this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
		// 
		// mnuHelp
		// 
		this.mnuHelp.Enabled = ((bool)(resources.GetObject("mnuHelp.Enabled")));
		this.mnuHelp.Index = 1;
		this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				this.mnuAbout});
		this.mnuHelp.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("mnuHelp.Shortcut")));
		this.mnuHelp.ShowShortcut = ((bool)(resources.GetObject("mnuHelp.ShowShortcut")));
		this.mnuHelp.Text = resources.GetString("mnuHelp.Text");
		this.mnuHelp.Visible = ((bool)(resources.GetObject("mnuHelp.Visible")));
		// 
		// mnuAbout
		// 
		this.mnuAbout.Enabled = ((bool)(resources.GetObject("mnuAbout.Enabled")));
		this.mnuAbout.Index = 0;
		this.mnuAbout.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("mnuAbout.Shortcut")));
		this.mnuAbout.ShowShortcut = ((bool)(resources.GetObject("mnuAbout.ShowShortcut")));
		this.mnuAbout.Text = resources.GetString("mnuAbout.Text");
		this.mnuAbout.Visible = ((bool)(resources.GetObject("mnuAbout.Visible")));
		this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
		// 
		// lblDescription
		// 
		this.lblDescription.AccessibleDescription = resources.GetString("lblDescription.AccessibleDescription");
		this.lblDescription.AccessibleName = resources.GetString("lblDescription.AccessibleName");
		this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblDescription.Anchor")));
		this.lblDescription.AutoSize = ((bool)(resources.GetObject("lblDescription.AutoSize")));
		this.lblDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblDescription.Dock")));
		this.lblDescription.Enabled = ((bool)(resources.GetObject("lblDescription.Enabled")));
		this.lblDescription.Font = ((System.Drawing.Font)(resources.GetObject("lblDescription.Font")));
		this.lblDescription.Image = ((System.Drawing.Image)(resources.GetObject("lblDescription.Image")));
		this.lblDescription.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescription.ImageAlign")));
		this.lblDescription.ImageIndex = ((int)(resources.GetObject("lblDescription.ImageIndex")));
		this.lblDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblDescription.ImeMode")));
		this.lblDescription.Location = ((System.Drawing.Point)(resources.GetObject("lblDescription.Location")));
		this.lblDescription.Name = "lblDescription";
		this.lblDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblDescription.RightToLeft")));
		this.lblDescription.Size = ((System.Drawing.Size)(resources.GetObject("lblDescription.Size")));
		this.lblDescription.TabIndex = ((int)(resources.GetObject("lblDescription.TabIndex")));
		this.lblDescription.Text = resources.GetString("lblDescription.Text");
		this.lblDescription.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescription.TextAlign")));
		this.lblDescription.Visible = ((bool)(resources.GetObject("lblDescription.Visible")));
		
		// 
		// lblLogDescription
		// 
		this.lblLogDescription.AccessibleDescription = resources.GetString("lblLogDescription.AccessibleDescription");
		this.lblLogDescription.AccessibleName = resources.GetString("lblLogDescription.AccessibleName");
		this.lblLogDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblLogDescription.Anchor")));
		this.lblLogDescription.AutoSize = ((bool)(resources.GetObject("lblLogDescription.AutoSize")));
		this.lblLogDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblLogDescription.Dock")));
		this.lblLogDescription.Enabled = ((bool)(resources.GetObject("lblLogDescription.Enabled")));
		this.lblLogDescription.Font = ((System.Drawing.Font)(resources.GetObject("lblLogDescription.Font")));
		this.lblLogDescription.Image = ((System.Drawing.Image)(resources.GetObject("lblLogDescription.Image")));
		this.lblLogDescription.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblLogDescription.ImageAlign")));
		this.lblLogDescription.ImageIndex = ((int)(resources.GetObject("lblLogDescription.ImageIndex")));
		this.lblLogDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblLogDescription.ImeMode")));
		this.lblLogDescription.Location = ((System.Drawing.Point)(resources.GetObject("lblLogDescription.Location")));
		this.lblLogDescription.Name = "lblLogDescription";
		this.lblLogDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblLogDescription.RightToLeft")));
		this.lblLogDescription.Size = ((System.Drawing.Size)(resources.GetObject("lblLogDescription.Size")));
		this.lblLogDescription.TabIndex = ((int)(resources.GetObject("lblLogDescription.TabIndex")));
		this.lblLogDescription.Text = resources.GetString("lblLogDescription.Text");
		this.lblLogDescription.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblLogDescription.TextAlign")));
		this.lblLogDescription.Visible = ((bool)(resources.GetObject("lblLogDescription.Visible")));
		// 
		// btnCheckForAddIn
		// 
		this.btnCheckForAddIn.AccessibleDescription = resources.GetString("btnCheckForAddIn.AccessibleDescription");
		this.btnCheckForAddIn.AccessibleName = resources.GetString("btnCheckForAddIn.AccessibleName");
		this.btnCheckForAddIn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCheckForAddIn.Anchor")));
		this.btnCheckForAddIn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCheckForAddIn.BackgroundImage")));
		this.btnCheckForAddIn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCheckForAddIn.Dock")));
		this.btnCheckForAddIn.Enabled = ((bool)(resources.GetObject("btnCheckForAddIn.Enabled")));
		this.btnCheckForAddIn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCheckForAddIn.FlatStyle")));
		this.btnCheckForAddIn.Font = ((System.Drawing.Font)(resources.GetObject("btnCheckForAddIn.Font")));
		this.btnCheckForAddIn.Image = ((System.Drawing.Image)(resources.GetObject("btnCheckForAddIn.Image")));
		this.btnCheckForAddIn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCheckForAddIn.ImageAlign")));
		this.btnCheckForAddIn.ImageIndex = ((int)(resources.GetObject("btnCheckForAddIn.ImageIndex")));
		this.btnCheckForAddIn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCheckForAddIn.ImeMode")));
		this.btnCheckForAddIn.Location = ((System.Drawing.Point)(resources.GetObject("btnCheckForAddIn.Location")));
		this.btnCheckForAddIn.Name = "btnCheckForAddIn";
		this.btnCheckForAddIn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCheckForAddIn.RightToLeft")));
		this.btnCheckForAddIn.Size = ((System.Drawing.Size)(resources.GetObject("btnCheckForAddIn.Size")));
		this.btnCheckForAddIn.TabIndex = ((int)(resources.GetObject("btnCheckForAddIn.TabIndex")));
		this.btnCheckForAddIn.Text = resources.GetString("btnCheckForAddIn.Text");
		this.btnCheckForAddIn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCheckForAddIn.TextAlign")));
		this.btnCheckForAddIn.Visible = ((bool)(resources.GetObject("btnCheckForAddIn.Visible")));
		this.btnCheckForAddIn.Click += new System.EventHandler(this.btnCheckForAddIn_Click);
		// 
		// txtInstalled
		// 
		this.txtInstalled.AccessibleDescription = resources.GetString("txtInstalled.AccessibleDescription");
		this.txtInstalled.AccessibleName = resources.GetString("txtInstalled.AccessibleName");
		this.txtInstalled.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtInstalled.Anchor")));
		this.txtInstalled.AutoSize = ((bool)(resources.GetObject("txtInstalled.AutoSize")));
		this.txtInstalled.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtInstalled.BackgroundImage")));
		this.txtInstalled.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtInstalled.Dock")));
		this.txtInstalled.Enabled = ((bool)(resources.GetObject("txtInstalled.Enabled")));
		this.txtInstalled.Font = ((System.Drawing.Font)(resources.GetObject("txtInstalled.Font")));
		this.txtInstalled.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtInstalled.ImeMode")));
		this.txtInstalled.Location = ((System.Drawing.Point)(resources.GetObject("txtInstalled.Location")));
		this.txtInstalled.MaxLength = ((int)(resources.GetObject("txtInstalled.MaxLength")));
		this.txtInstalled.Multiline = ((bool)(resources.GetObject("txtInstalled.Multiline")));
		this.txtInstalled.Name = "txtInstalled";
		this.txtInstalled.PasswordChar = ((char)(resources.GetObject("txtInstalled.PasswordChar")));
		this.txtInstalled.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtInstalled.RightToLeft")));
		this.txtInstalled.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtInstalled.ScrollBars")));
		this.txtInstalled.Size = ((System.Drawing.Size)(resources.GetObject("txtInstalled.Size")));
		this.txtInstalled.TabIndex = ((int)(resources.GetObject("txtInstalled.TabIndex")));
		this.txtInstalled.TabStop = false;
		this.txtInstalled.Text = resources.GetString("txtInstalled.Text");
		this.txtInstalled.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtInstalled.TextAlign")));
		this.txtInstalled.Visible = ((bool)(resources.GetObject("txtInstalled.Visible")));
		this.txtInstalled.WordWrap = ((bool)(resources.GetObject("txtInstalled.WordWrap")));
		// 
		// lblStatus
		// 
		this.lblStatus.AccessibleDescription = resources.GetString("lblStatus.AccessibleDescription");
		this.lblStatus.AccessibleName = resources.GetString("lblStatus.AccessibleName");
		this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblStatus.Anchor")));
		this.lblStatus.AutoSize = ((bool)(resources.GetObject("lblStatus.AutoSize")));
		this.lblStatus.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblStatus.Dock")));
		this.lblStatus.Enabled = ((bool)(resources.GetObject("lblStatus.Enabled")));
		this.lblStatus.Font = ((System.Drawing.Font)(resources.GetObject("lblStatus.Font")));
		this.lblStatus.Image = ((System.Drawing.Image)(resources.GetObject("lblStatus.Image")));
		this.lblStatus.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblStatus.ImageAlign")));
		this.lblStatus.ImageIndex = ((int)(resources.GetObject("lblStatus.ImageIndex")));
		this.lblStatus.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblStatus.ImeMode")));
		this.lblStatus.Location = ((System.Drawing.Point)(resources.GetObject("lblStatus.Location")));
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblStatus.RightToLeft")));
		this.lblStatus.Size = ((System.Drawing.Size)(resources.GetObject("lblStatus.Size")));
		this.lblStatus.TabIndex = ((int)(resources.GetObject("lblStatus.TabIndex")));
		this.lblStatus.Text = resources.GetString("lblStatus.Text");
		this.lblStatus.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblStatus.TextAlign")));
		this.lblStatus.Visible = ((bool)(resources.GetObject("lblStatus.Visible")));
		// 
		// frmMain
		// 
		this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
		this.AccessibleName = resources.GetString("$this.AccessibleName");
		this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
		this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
		this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
		this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
		this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
		this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
		this.Controls.Add(this.lblStatus);
		this.Controls.Add(this.txtInstalled);
		this.Controls.Add(this.btnCheckForAddIn);
		this.Controls.Add(this.lblLogDescription);
		this.Controls.Add(this.lblDescription);
		this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
		this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
		this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
		this.MaximizeBox = false;
		this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
		this.Menu = this.mnuMain;
		this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
		this.Name = "frmMain";
		this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
		this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
		this.Text = resources.GetString("$this.Text");
		this.Load += new System.EventHandler(this.frmMain_Load);
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

    // This subroutine checks for the existence of the Add-In. The status of the

    //   installation is reported to the user.

    private void btnCheckForAddIn_Click(object sender, System.EventArgs e) 
	{

        bool isInstalled = false;

		System.Type t = System.Type.GetTypeFromProgID("VisualStudio.DTE.7.1");
		object obj = System.Activator.CreateInstance(t, true);


        // Create a DTE object, to get the Add-In informations.

        EnvDTE.DTE myDTE = (EnvDTE.DTE)obj;

        // Create a collection of all the AddIn objects.

        EnvDTE.AddIns myAddIns = myDTE.AddIns;

        // Iterate through the AddIns to determine if the C# How To
        //   Example Add-in is installed.

        foreach(EnvDTE.AddIn myAddIn in myAddIns)
		{
            if (myAddIn.Name == "How-To Example Add-In")

			{

                isInstalled = true;

            }

        }

        // Report installation status to the user.

		if (isInstalled) 
		{

			txtInstalled.Text = "C# How To Example Add-in is installed!";
		}
		else 
		{
			txtInstalled.Text = "C# How To Example Add-in is not installed!";
		}

    }

    // This subroutine executes when the main form is loaded. It simply calls
    //   the btnCheckForAddIn_Click subroutine to determine if the Add-in
    //   is loaded.

    private void frmMain_Load(object sender, System.EventArgs e) {

        // Call the btnCheckForAddIn_Click subroutine.

        btnCheckForAddIn_Click(this, new System.EventArgs());

    }


}

