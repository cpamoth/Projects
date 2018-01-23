//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).
using System;
using System.IO;
using System.Windows.Forms;
using System.Security;

public class frmMain:System.Windows.Forms.Form 
{
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
	private System.Windows.Forms.GroupBox grpInputs;
	private System.Windows.Forms.GroupBox grpCommands;
	private System.Windows.Forms.Button cmdNoTryCatch;
	private System.Windows.Forms.TextBox txtFileName;
	private System.Windows.Forms.Label lblFileName;
	private System.Windows.Forms.Button cmdBasicTryCatch;
	private System.Windows.Forms.Button cmdDetailedTryCatch;
	private System.Windows.Forms.Button cmdTryCatchFinally;
	private System.Windows.Forms.Button cmdCustomMessage;
	private System.Windows.Forms.TextBox txtMessage;

	private void InitializeComponent() {
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
		this.mnuMain = new System.Windows.Forms.MainMenu();
		this.mnuFile = new System.Windows.Forms.MenuItem();
		this.mnuExit = new System.Windows.Forms.MenuItem();
		this.mnuHelp = new System.Windows.Forms.MenuItem();
		this.mnuAbout = new System.Windows.Forms.MenuItem();
		this.grpInputs = new System.Windows.Forms.GroupBox();
		this.txtMessage = new System.Windows.Forms.TextBox();
		this.lblFileName = new System.Windows.Forms.Label();
		this.txtFileName = new System.Windows.Forms.TextBox();
		this.grpCommands = new System.Windows.Forms.GroupBox();
		this.cmdCustomMessage = new System.Windows.Forms.Button();
		this.cmdTryCatchFinally = new System.Windows.Forms.Button();
		this.cmdDetailedTryCatch = new System.Windows.Forms.Button();
		this.cmdBasicTryCatch = new System.Windows.Forms.Button();
		this.cmdNoTryCatch = new System.Windows.Forms.Button();
		this.grpInputs.SuspendLayout();
		this.grpCommands.SuspendLayout();
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
		// grpInputs
		// 
		this.grpInputs.AccessibleDescription = resources.GetString("grpInputs.AccessibleDescription");
		this.grpInputs.AccessibleName = resources.GetString("grpInputs.AccessibleName");
		this.grpInputs.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("grpInputs.Anchor")));
		this.grpInputs.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("grpInputs.BackgroundImage")));
		this.grpInputs.Controls.Add(this.txtMessage);
		this.grpInputs.Controls.Add(this.lblFileName);
		this.grpInputs.Controls.Add(this.txtFileName);
		this.grpInputs.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("grpInputs.Dock")));
		this.grpInputs.Enabled = ((bool)(resources.GetObject("grpInputs.Enabled")));
		this.grpInputs.Font = ((System.Drawing.Font)(resources.GetObject("grpInputs.Font")));
		this.grpInputs.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("grpInputs.ImeMode")));
		this.grpInputs.Location = ((System.Drawing.Point)(resources.GetObject("grpInputs.Location")));
		this.grpInputs.Name = "grpInputs";
		this.grpInputs.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("grpInputs.RightToLeft")));
		this.grpInputs.Size = ((System.Drawing.Size)(resources.GetObject("grpInputs.Size")));
		this.grpInputs.TabIndex = ((int)(resources.GetObject("grpInputs.TabIndex")));
		this.grpInputs.TabStop = false;
		this.grpInputs.Text = resources.GetString("grpInputs.Text");
		this.grpInputs.Visible = ((bool)(resources.GetObject("grpInputs.Visible")));
		// 
		// txtMessage
		// 
		this.txtMessage.AccessibleDescription = resources.GetString("txtMessage.AccessibleDescription");
		this.txtMessage.AccessibleName = resources.GetString("txtMessage.AccessibleName");
		this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtMessage.Anchor")));
		this.txtMessage.AutoSize = ((bool)(resources.GetObject("txtMessage.AutoSize")));
		this.txtMessage.BackColor = System.Drawing.SystemColors.Control;
		this.txtMessage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtMessage.BackgroundImage")));
		this.txtMessage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtMessage.Dock")));
		this.txtMessage.Enabled = ((bool)(resources.GetObject("txtMessage.Enabled")));
		this.txtMessage.Font = ((System.Drawing.Font)(resources.GetObject("txtMessage.Font")));
		this.txtMessage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtMessage.ImeMode")));
		this.txtMessage.Location = ((System.Drawing.Point)(resources.GetObject("txtMessage.Location")));
		this.txtMessage.MaxLength = ((int)(resources.GetObject("txtMessage.MaxLength")));
		this.txtMessage.Multiline = ((bool)(resources.GetObject("txtMessage.Multiline")));
		this.txtMessage.Name = "txtMessage";
		this.txtMessage.PasswordChar = ((char)(resources.GetObject("txtMessage.PasswordChar")));
		this.txtMessage.ReadOnly = true;
		this.txtMessage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtMessage.RightToLeft")));
		this.txtMessage.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtMessage.ScrollBars")));
		this.txtMessage.Size = ((System.Drawing.Size)(resources.GetObject("txtMessage.Size")));
		this.txtMessage.TabIndex = ((int)(resources.GetObject("txtMessage.TabIndex")));
		this.txtMessage.Text = resources.GetString("txtMessage.Text");
		this.txtMessage.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtMessage.TextAlign")));
		this.txtMessage.Visible = ((bool)(resources.GetObject("txtMessage.Visible")));
		this.txtMessage.WordWrap = ((bool)(resources.GetObject("txtMessage.WordWrap")));
		// 
		// lblFileName
		// 
		this.lblFileName.AccessibleDescription = resources.GetString("lblFileName.AccessibleDescription");
		this.lblFileName.AccessibleName = resources.GetString("lblFileName.AccessibleName");
		this.lblFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblFileName.Anchor")));
		this.lblFileName.AutoSize = ((bool)(resources.GetObject("lblFileName.AutoSize")));
		this.lblFileName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblFileName.Dock")));
		this.lblFileName.Enabled = ((bool)(resources.GetObject("lblFileName.Enabled")));
		this.lblFileName.Font = ((System.Drawing.Font)(resources.GetObject("lblFileName.Font")));
		this.lblFileName.Image = ((System.Drawing.Image)(resources.GetObject("lblFileName.Image")));
		this.lblFileName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFileName.ImageAlign")));
		this.lblFileName.ImageIndex = ((int)(resources.GetObject("lblFileName.ImageIndex")));
		this.lblFileName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblFileName.ImeMode")));
		this.lblFileName.Location = ((System.Drawing.Point)(resources.GetObject("lblFileName.Location")));
		this.lblFileName.Name = "lblFileName";
		this.lblFileName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblFileName.RightToLeft")));
		this.lblFileName.Size = ((System.Drawing.Size)(resources.GetObject("lblFileName.Size")));
		this.lblFileName.TabIndex = ((int)(resources.GetObject("lblFileName.TabIndex")));
		this.lblFileName.Text = resources.GetString("lblFileName.Text");
		this.lblFileName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblFileName.TextAlign")));
		this.lblFileName.Visible = ((bool)(resources.GetObject("lblFileName.Visible")));
		// 
		// txtFileName
		// 
		this.txtFileName.AccessibleDescription = resources.GetString("txtFileName.AccessibleDescription");
		this.txtFileName.AccessibleName = resources.GetString("txtFileName.AccessibleName");
		this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtFileName.Anchor")));
		this.txtFileName.AutoSize = ((bool)(resources.GetObject("txtFileName.AutoSize")));
		this.txtFileName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtFileName.BackgroundImage")));
		this.txtFileName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtFileName.Dock")));
		this.txtFileName.Enabled = ((bool)(resources.GetObject("txtFileName.Enabled")));
		this.txtFileName.Font = ((System.Drawing.Font)(resources.GetObject("txtFileName.Font")));
		this.txtFileName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtFileName.ImeMode")));
		this.txtFileName.Location = ((System.Drawing.Point)(resources.GetObject("txtFileName.Location")));
		this.txtFileName.MaxLength = ((int)(resources.GetObject("txtFileName.MaxLength")));
		this.txtFileName.Multiline = ((bool)(resources.GetObject("txtFileName.Multiline")));
		this.txtFileName.Name = "txtFileName";
		this.txtFileName.PasswordChar = ((char)(resources.GetObject("txtFileName.PasswordChar")));
		this.txtFileName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtFileName.RightToLeft")));
		this.txtFileName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtFileName.ScrollBars")));
		this.txtFileName.Size = ((System.Drawing.Size)(resources.GetObject("txtFileName.Size")));
		this.txtFileName.TabIndex = ((int)(resources.GetObject("txtFileName.TabIndex")));
		this.txtFileName.Text = resources.GetString("txtFileName.Text");
		this.txtFileName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtFileName.TextAlign")));
		this.txtFileName.Visible = ((bool)(resources.GetObject("txtFileName.Visible")));
		this.txtFileName.WordWrap = ((bool)(resources.GetObject("txtFileName.WordWrap")));
		// 
		// grpCommands
		// 
		this.grpCommands.AccessibleDescription = resources.GetString("grpCommands.AccessibleDescription");
		this.grpCommands.AccessibleName = resources.GetString("grpCommands.AccessibleName");
		this.grpCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("grpCommands.Anchor")));
		this.grpCommands.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("grpCommands.BackgroundImage")));
		this.grpCommands.Controls.Add(this.cmdCustomMessage);
		this.grpCommands.Controls.Add(this.cmdTryCatchFinally);
		this.grpCommands.Controls.Add(this.cmdDetailedTryCatch);
		this.grpCommands.Controls.Add(this.cmdBasicTryCatch);
		this.grpCommands.Controls.Add(this.cmdNoTryCatch);
		this.grpCommands.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("grpCommands.Dock")));
		this.grpCommands.Enabled = ((bool)(resources.GetObject("grpCommands.Enabled")));
		this.grpCommands.Font = ((System.Drawing.Font)(resources.GetObject("grpCommands.Font")));
		this.grpCommands.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("grpCommands.ImeMode")));
		this.grpCommands.Location = ((System.Drawing.Point)(resources.GetObject("grpCommands.Location")));
		this.grpCommands.Name = "grpCommands";
		this.grpCommands.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("grpCommands.RightToLeft")));
		this.grpCommands.Size = ((System.Drawing.Size)(resources.GetObject("grpCommands.Size")));
		this.grpCommands.TabIndex = ((int)(resources.GetObject("grpCommands.TabIndex")));
		this.grpCommands.TabStop = false;
		this.grpCommands.Text = resources.GetString("grpCommands.Text");
		this.grpCommands.Visible = ((bool)(resources.GetObject("grpCommands.Visible")));
		this.grpCommands.Enter += new System.EventHandler(this.grpCommands_Enter);
		// 
		// cmdCustomMessage
		// 
		this.cmdCustomMessage.AccessibleDescription = resources.GetString("cmdCustomMessage.AccessibleDescription");
		this.cmdCustomMessage.AccessibleName = resources.GetString("cmdCustomMessage.AccessibleName");
		this.cmdCustomMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdCustomMessage.Anchor")));
		this.cmdCustomMessage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdCustomMessage.BackgroundImage")));
		this.cmdCustomMessage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdCustomMessage.Dock")));
		this.cmdCustomMessage.Enabled = ((bool)(resources.GetObject("cmdCustomMessage.Enabled")));
		this.cmdCustomMessage.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdCustomMessage.FlatStyle")));
		this.cmdCustomMessage.Font = ((System.Drawing.Font)(resources.GetObject("cmdCustomMessage.Font")));
		this.cmdCustomMessage.Image = ((System.Drawing.Image)(resources.GetObject("cmdCustomMessage.Image")));
		this.cmdCustomMessage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdCustomMessage.ImageAlign")));
		this.cmdCustomMessage.ImageIndex = ((int)(resources.GetObject("cmdCustomMessage.ImageIndex")));
		this.cmdCustomMessage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdCustomMessage.ImeMode")));
		this.cmdCustomMessage.Location = ((System.Drawing.Point)(resources.GetObject("cmdCustomMessage.Location")));
		this.cmdCustomMessage.Name = "cmdCustomMessage";
		this.cmdCustomMessage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdCustomMessage.RightToLeft")));
		this.cmdCustomMessage.Size = ((System.Drawing.Size)(resources.GetObject("cmdCustomMessage.Size")));
		this.cmdCustomMessage.TabIndex = ((int)(resources.GetObject("cmdCustomMessage.TabIndex")));
		this.cmdCustomMessage.Text = resources.GetString("cmdCustomMessage.Text");
		this.cmdCustomMessage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdCustomMessage.TextAlign")));
		this.cmdCustomMessage.Visible = ((bool)(resources.GetObject("cmdCustomMessage.Visible")));
		this.cmdCustomMessage.Click +=new EventHandler(cmdCustomMessage_Click);

		// 
		// cmdTryCatchFinally
		// 
		this.cmdTryCatchFinally.AccessibleDescription = resources.GetString("cmdTryCatchFinally.AccessibleDescription");
		this.cmdTryCatchFinally.AccessibleName = resources.GetString("cmdTryCatchFinally.AccessibleName");
		this.cmdTryCatchFinally.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdTryCatchFinally.Anchor")));
		this.cmdTryCatchFinally.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdTryCatchFinally.BackgroundImage")));
		this.cmdTryCatchFinally.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdTryCatchFinally.Dock")));
		this.cmdTryCatchFinally.Enabled = ((bool)(resources.GetObject("cmdTryCatchFinally.Enabled")));
		this.cmdTryCatchFinally.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdTryCatchFinally.FlatStyle")));
		this.cmdTryCatchFinally.Font = ((System.Drawing.Font)(resources.GetObject("cmdTryCatchFinally.Font")));
		this.cmdTryCatchFinally.Image = ((System.Drawing.Image)(resources.GetObject("cmdTryCatchFinally.Image")));
		this.cmdTryCatchFinally.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdTryCatchFinally.ImageAlign")));
		this.cmdTryCatchFinally.ImageIndex = ((int)(resources.GetObject("cmdTryCatchFinally.ImageIndex")));
		this.cmdTryCatchFinally.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdTryCatchFinally.ImeMode")));
		this.cmdTryCatchFinally.Location = ((System.Drawing.Point)(resources.GetObject("cmdTryCatchFinally.Location")));
		this.cmdTryCatchFinally.Name = "cmdTryCatchFinally";
		this.cmdTryCatchFinally.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdTryCatchFinally.RightToLeft")));
		this.cmdTryCatchFinally.Size = ((System.Drawing.Size)(resources.GetObject("cmdTryCatchFinally.Size")));
		this.cmdTryCatchFinally.TabIndex = ((int)(resources.GetObject("cmdTryCatchFinally.TabIndex")));
		this.cmdTryCatchFinally.Text = resources.GetString("cmdTryCatchFinally.Text");
		this.cmdTryCatchFinally.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdTryCatchFinally.TextAlign")));
		this.cmdTryCatchFinally.Visible = ((bool)(resources.GetObject("cmdTryCatchFinally.Visible")));
		this.cmdTryCatchFinally.Click += new System.EventHandler(this.cmdTryCatchFinally_Click);
		// 
		// cmdDetailedTryCatch
		// 
		this.cmdDetailedTryCatch.AccessibleDescription = resources.GetString("cmdDetailedTryCatch.AccessibleDescription");
		this.cmdDetailedTryCatch.AccessibleName = resources.GetString("cmdDetailedTryCatch.AccessibleName");
		this.cmdDetailedTryCatch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdDetailedTryCatch.Anchor")));
		this.cmdDetailedTryCatch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdDetailedTryCatch.BackgroundImage")));
		this.cmdDetailedTryCatch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdDetailedTryCatch.Dock")));
		this.cmdDetailedTryCatch.Enabled = ((bool)(resources.GetObject("cmdDetailedTryCatch.Enabled")));
		this.cmdDetailedTryCatch.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdDetailedTryCatch.FlatStyle")));
		this.cmdDetailedTryCatch.Font = ((System.Drawing.Font)(resources.GetObject("cmdDetailedTryCatch.Font")));
		this.cmdDetailedTryCatch.Image = ((System.Drawing.Image)(resources.GetObject("cmdDetailedTryCatch.Image")));
		this.cmdDetailedTryCatch.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdDetailedTryCatch.ImageAlign")));
		this.cmdDetailedTryCatch.ImageIndex = ((int)(resources.GetObject("cmdDetailedTryCatch.ImageIndex")));
		this.cmdDetailedTryCatch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdDetailedTryCatch.ImeMode")));
		this.cmdDetailedTryCatch.Location = ((System.Drawing.Point)(resources.GetObject("cmdDetailedTryCatch.Location")));
		this.cmdDetailedTryCatch.Name = "cmdDetailedTryCatch";
		this.cmdDetailedTryCatch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdDetailedTryCatch.RightToLeft")));
		this.cmdDetailedTryCatch.Size = ((System.Drawing.Size)(resources.GetObject("cmdDetailedTryCatch.Size")));
		this.cmdDetailedTryCatch.TabIndex = ((int)(resources.GetObject("cmdDetailedTryCatch.TabIndex")));
		this.cmdDetailedTryCatch.Text = resources.GetString("cmdDetailedTryCatch.Text");
		this.cmdDetailedTryCatch.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdDetailedTryCatch.TextAlign")));
		this.cmdDetailedTryCatch.Visible = ((bool)(resources.GetObject("cmdDetailedTryCatch.Visible")));
		this.cmdDetailedTryCatch.Click += new System.EventHandler(this.cmdDetailedTryCatch_Click);
		// 
		// cmdBasicTryCatch
		// 
		this.cmdBasicTryCatch.AccessibleDescription = resources.GetString("cmdBasicTryCatch.AccessibleDescription");
		this.cmdBasicTryCatch.AccessibleName = resources.GetString("cmdBasicTryCatch.AccessibleName");
		this.cmdBasicTryCatch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdBasicTryCatch.Anchor")));
		this.cmdBasicTryCatch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdBasicTryCatch.BackgroundImage")));
		this.cmdBasicTryCatch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdBasicTryCatch.Dock")));
		this.cmdBasicTryCatch.Enabled = ((bool)(resources.GetObject("cmdBasicTryCatch.Enabled")));
		this.cmdBasicTryCatch.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdBasicTryCatch.FlatStyle")));
		this.cmdBasicTryCatch.Font = ((System.Drawing.Font)(resources.GetObject("cmdBasicTryCatch.Font")));
		this.cmdBasicTryCatch.Image = ((System.Drawing.Image)(resources.GetObject("cmdBasicTryCatch.Image")));
		this.cmdBasicTryCatch.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdBasicTryCatch.ImageAlign")));
		this.cmdBasicTryCatch.ImageIndex = ((int)(resources.GetObject("cmdBasicTryCatch.ImageIndex")));
		this.cmdBasicTryCatch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdBasicTryCatch.ImeMode")));
		this.cmdBasicTryCatch.Location = ((System.Drawing.Point)(resources.GetObject("cmdBasicTryCatch.Location")));
		this.cmdBasicTryCatch.Name = "cmdBasicTryCatch";
		this.cmdBasicTryCatch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdBasicTryCatch.RightToLeft")));
		this.cmdBasicTryCatch.Size = ((System.Drawing.Size)(resources.GetObject("cmdBasicTryCatch.Size")));
		this.cmdBasicTryCatch.TabIndex = ((int)(resources.GetObject("cmdBasicTryCatch.TabIndex")));
		this.cmdBasicTryCatch.Text = resources.GetString("cmdBasicTryCatch.Text");
		this.cmdBasicTryCatch.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdBasicTryCatch.TextAlign")));
		this.cmdBasicTryCatch.Visible = ((bool)(resources.GetObject("cmdBasicTryCatch.Visible")));
		this.cmdBasicTryCatch.Click += new System.EventHandler(this.cmdBasicTryCatch_Click);
		// 
		// cmdNoTryCatch
		// 
		this.cmdNoTryCatch.AccessibleDescription = resources.GetString("cmdNoTryCatch.AccessibleDescription");
		this.cmdNoTryCatch.AccessibleName = resources.GetString("cmdNoTryCatch.AccessibleName");
		this.cmdNoTryCatch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cmdNoTryCatch.Anchor")));
		this.cmdNoTryCatch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdNoTryCatch.BackgroundImage")));
		this.cmdNoTryCatch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cmdNoTryCatch.Dock")));
		this.cmdNoTryCatch.Enabled = ((bool)(resources.GetObject("cmdNoTryCatch.Enabled")));
		this.cmdNoTryCatch.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cmdNoTryCatch.FlatStyle")));
		this.cmdNoTryCatch.Font = ((System.Drawing.Font)(resources.GetObject("cmdNoTryCatch.Font")));
		this.cmdNoTryCatch.Image = ((System.Drawing.Image)(resources.GetObject("cmdNoTryCatch.Image")));
		this.cmdNoTryCatch.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdNoTryCatch.ImageAlign")));
		this.cmdNoTryCatch.ImageIndex = ((int)(resources.GetObject("cmdNoTryCatch.ImageIndex")));
		this.cmdNoTryCatch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cmdNoTryCatch.ImeMode")));
		this.cmdNoTryCatch.Location = ((System.Drawing.Point)(resources.GetObject("cmdNoTryCatch.Location")));
		this.cmdNoTryCatch.Name = "cmdNoTryCatch";
		this.cmdNoTryCatch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cmdNoTryCatch.RightToLeft")));
		this.cmdNoTryCatch.Size = ((System.Drawing.Size)(resources.GetObject("cmdNoTryCatch.Size")));
		this.cmdNoTryCatch.TabIndex = ((int)(resources.GetObject("cmdNoTryCatch.TabIndex")));
		this.cmdNoTryCatch.Text = resources.GetString("cmdNoTryCatch.Text");
		this.cmdNoTryCatch.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cmdNoTryCatch.TextAlign")));
		this.cmdNoTryCatch.Visible = ((bool)(resources.GetObject("cmdNoTryCatch.Visible")));
		this.cmdNoTryCatch.Click += new System.EventHandler(this.cmdNoTryCatch_Click);
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
		this.Controls.Add(this.grpCommands);
		this.Controls.Add(this.grpInputs);
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
		this.grpInputs.ResumeLayout(false);
		this.grpCommands.ResumeLayout(false);
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

#region " Form Load "

	// Load the message for the text box that is below the file name text box.

	private void frmMain_Load(object sender, System.EventArgs e) {

		// The code below uses the new stringWriter to build a string in memory.

		// The WriteLine commands append code to the string buffer with carriage return and line feed.
			
			StringWriter txt  = new StringWriter();
			
			txt.WriteLine("Enter a file name + path to test error handling. ");

			txt.WriteLine("try { different combinations for example:");

			txt.WriteLine("");

			txt.WriteLine("  C:\nofile.txt");

			txt.WriteLine("  c:\nodir\nofile.txt");

			txt.WriteLine("  \\noserver\noshare\nofile.txt");

		

		// Ask the stringWriter to covert its buffer to a string

		this.txtMessage.Text = txt.ToString();

	}

#endregion

	private void cmdNoTryCatch_Click(object sender, System.EventArgs e) 
	{
		// Ask to make sure the user is willing to possibly blow up the program.

		string strMsg  = "The following code has no error handling and will cause an unhandled exception if a file is not found. Do you want to continue?";
		
		if (MessageBox.Show(strMsg, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question,MessageBoxDefaultButton.Button2) == DialogResult.Yes)

		{
			// Use the FileStream class from the System.IO Namespace (see using at top of file).
			FileStream fs ;
			// This command will fail if the file does not exist.
			fs = File.Open(this.txtFileName.Text, FileMode.Open);
			MessageBox.Show("The size of the file is: " + fs.Length, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
			fs.Close();
		}

	}

	private void cmdBasicTryCatch_Click(object sender, System.EventArgs e) 
	{
		// This procedure will perform a basic try {, Catch.

		// Use the FileStream class from the System.IO Namespace (see using at top of file).

		FileStream fs;

		try 
		{

			// This command will fail if the file does not exist.

			fs = File.Open(this.txtFileName.Text, FileMode.Open);

			MessageBox.Show("The size of the file is: " + fs.Length, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

			fs.Close();
		}

		catch (Exception exp) 
		{

			// Will catch any error that we're not explicitly trapping.

			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

		}

	}

	private void cmdDetailedTryCatch_Click(object sender, System.EventArgs e) 
	{

		// This procedure will perform a specific try {,} catch( looking for any IO related errors.

		// Use the FileStream class from the System.IO Namespace (see using at top of file).

		FileStream fs ;

		try 
		{

			// This command will fail if the file does not exist.

			fs = File.Open(this.txtFileName.Text, FileMode.Open);

			MessageBox.Show("The size of the file is: " + fs.Length, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

			fs.Close();
		}
		catch  (FileNotFoundException exp)
		{
			// Will catch an error when the file requested does not exist.
			MessageBox.Show("The file you requested does not exist.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
		catch (DirectoryNotFoundException exp)
		{

			// Will catch an error when the directory requested does not exist.
			MessageBox.Show("The directory you requested does not exist.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
		catch (IOException exp)
		{
			// Will catch any generic IO exception.
			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
		catch (Exception exp) 
		{

			// Will catch any error that we're not explicitly trapping.

			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

		}

	}

	private void cmdCustomMessage_Click(object sender, System.EventArgs e)
	{
		// This procedure will perform a specific try {,} catch( looking for any IO related errors
		// Use the FileStream class from the System.IO Namespace (see using at top of file)

		FileStream fs ;

		try 
		{

			// This command will fail if the file does not exist

			fs = File.Open(this.txtFileName.Text, FileMode.Open);

			MessageBox.Show("The size of the file is: " + fs.Length, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

			fs.Close();
		}
		catch (IOException exp)
		{
			// Will catch any generic IO exception
			// You could use the stringWriter to build a multi-line string in memory.
			// However, it's overkill for this simple message.
			// FYI stringWriter comes from the System.IO Namespace.

			string strMsg;

			strMsg = "I was unable to open the file you requested, " + this.txtFileName.Text + Environment.NewLine + Environment.NewLine + "Detailed Error Information below:" + Environment.NewLine + Environment.NewLine + "  Message: " + exp.Message + Environment.NewLine + "  Source: " + exp.Source + Environment.NewLine + Environment.NewLine + "  Stack Trace:" + Environment.NewLine;

			string strStackTrace;

			// Accessing an exception objects StackTrace could cause an exception
			// thus we need to wrap the call in its own try {,} catch( block.
			
			try 
			{
				
				strStackTrace = exp.StackTrace;
			}
		
			catch(SecurityException stExp)
			{
				
				strStackTrace = "Unable to access stack trace due to security restrictions.";
			}
			catch(Exception stExp)
			{
				strStackTrace = "Unable to access stack trace.";
			}
				strMsg = strMsg + strStackTrace;

				MessageBox.Show(strMsg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		
			
		}
		catch (Exception exp)
			{
			//This catch will trap any error unexpected error.
			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
}
	

	private void cmdTryCatchFinally_Click(object sender, System.EventArgs e) 
	{

		// This procedure will perform a basic try {, Catch, and then a Finally.

		// Use the FileStream class from the System.IO Namespace (see using at top of file).

		FileStream fs = null;

		try 
		{

			// This command will fail if the file does not exist.

			fs = File.Open(this.txtFileName.Text, FileMode.Open);

			MessageBox.Show("The size of the file is: " + fs.Length, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		catch (Exception exp) 
		{

			// Will catch any error that we're not explicitly trapping.
			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
		finally
		{
			// if we didn't open the file successfully, then our reference will be null.
			//if (! fs == null) 
			if(fs != null)
			{
				fs.Close();
				MessageBox.Show("File closed successfully in Finally block", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

		}

	}

	private void grpCommands_Enter(object sender, System.EventArgs e)
	{
	
	}

}

