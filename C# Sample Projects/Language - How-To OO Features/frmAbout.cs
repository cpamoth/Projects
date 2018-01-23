//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).
using System;
using System.Reflection;
using System.Windows.Forms;

public class frmAbout : System.Windows.Forms.Form 
{

#region " Windows Form Designer generated code "

	public frmAbout() 
	{
		//This call is required by the Windows Form Designer.

		InitializeComponent();

		//Add any initialization after the InitializeComponent() call
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

	private System.Windows.Forms.PictureBox pbIcon;

	private System.Windows.Forms.Label lblTitle;

	private System.Windows.Forms.Label lblVersion;

	private System.Windows.Forms.Label lblDescription;

	private System.Windows.Forms.Label lblCopyright;

	private System.Windows.Forms.Label lblCodebase;

	private System.Windows.Forms.Button btnOK;

	private void InitializeComponent() 
	{
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmAbout));

		this.pbIcon = new System.Windows.Forms.PictureBox();

		this.lblTitle = new System.Windows.Forms.Label();

		this.lblVersion = new System.Windows.Forms.Label();

		this.lblDescription = new System.Windows.Forms.Label();

		this.btnOK = new System.Windows.Forms.Button();

		this.lblCopyright = new System.Windows.Forms.Label();

		this.lblCodebase = new System.Windows.Forms.Label();

		this.SuspendLayout();

		//

		//pbIcon

		//

		this.pbIcon.AccessibleDescription = (string) resources.GetObject("pbIcon.AccessibleDescription");

		this.pbIcon.AccessibleName = (string) resources.GetObject("pbIcon.AccessibleName");

		this.pbIcon.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("pbIcon.Anchor");

		this.pbIcon.BackgroundImage = (System.Drawing.Image) resources.GetObject("pbIcon.BackgroundImage");

		this.pbIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

		this.pbIcon.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("pbIcon.Dock");

		this.pbIcon.Enabled = (bool) resources.GetObject("pbIcon.Enabled");

		this.pbIcon.Font = (System.Drawing.Font) resources.GetObject("pbIcon.Font");

		this.pbIcon.Image = (System.Drawing.Image) resources.GetObject("pbIcon.Image");

		this.pbIcon.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("pbIcon.ImeMode");

		this.pbIcon.Location = (System.Drawing.Point) resources.GetObject("pbIcon.Location");

		this.pbIcon.Name = "pbIcon";

		this.pbIcon.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("pbIcon.RightToLeft");

		this.pbIcon.Size = (System.Drawing.Size) resources.GetObject("pbIcon.Size");

		this.pbIcon.SizeMode = (System.Windows.Forms.PictureBoxSizeMode) resources.GetObject("pbIcon.SizeMode");

		this.pbIcon.TabIndex = (int) resources.GetObject("pbIcon.TabIndex");

		this.pbIcon.TabStop = false;

		this.pbIcon.Text = resources.GetString("pbIcon.Text");

		this.pbIcon.Visible = (bool) resources.GetObject("pbIcon.Visible");

		//

		//lblTitle

		//

		this.lblTitle.AccessibleDescription = (string) resources.GetObject("lblTitle.AccessibleDescription");

		this.lblTitle.AccessibleName = (string) resources.GetObject("lblTitle.AccessibleName");

		this.lblTitle.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("lblTitle.Anchor");

		this.lblTitle.AutoSize = (bool) resources.GetObject("lblTitle.AutoSize");

		this.lblTitle.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("lblTitle.Dock");

		this.lblTitle.Enabled = (bool) resources.GetObject("lblTitle.Enabled");

		this.lblTitle.Font = (System.Drawing.Font) resources.GetObject("lblTitle.Font");

		this.lblTitle.Image = (System.Drawing.Image) resources.GetObject("lblTitle.Image");

		this.lblTitle.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblTitle.ImageAlign");

		this.lblTitle.ImageIndex = (int) resources.GetObject("lblTitle.ImageIndex");

		this.lblTitle.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("lblTitle.ImeMode");

		this.lblTitle.Location = (System.Drawing.Point) resources.GetObject("lblTitle.Location");

		this.lblTitle.Name = "lblTitle";

		this.lblTitle.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("lblTitle.RightToLeft");

		this.lblTitle.Size = (System.Drawing.Size) resources.GetObject("lblTitle.Size");

		this.lblTitle.TabIndex = (int) resources.GetObject("lblTitle.TabIndex");

		this.lblTitle.Text = resources.GetString("lblTitle.Text");

		this.lblTitle.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblTitle.TextAlign");

		this.lblTitle.Visible = (bool) resources.GetObject("lblTitle.Visible");

		//

		//lblVersion

		//

		this.lblVersion.AccessibleDescription = (string) resources.GetObject("lblVersion.AccessibleDescription");

		this.lblVersion.AccessibleName = (string) resources.GetObject("lblVersion.AccessibleName");

		this.lblVersion.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("lblVersion.Anchor");

		this.lblVersion.AutoSize = (bool) resources.GetObject("lblVersion.AutoSize");

		this.lblVersion.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("lblVersion.Dock");

		this.lblVersion.Enabled = (bool) resources.GetObject("lblVersion.Enabled");

		this.lblVersion.Font = (System.Drawing.Font) resources.GetObject("lblVersion.Font");

		this.lblVersion.Image = (System.Drawing.Image) resources.GetObject("lblVersion.Image");

		this.lblVersion.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblVersion.ImageAlign");

		this.lblVersion.ImageIndex = (int) resources.GetObject("lblVersion.ImageIndex");

		this.lblVersion.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("lblVersion.ImeMode");

		this.lblVersion.Location = (System.Drawing.Point) resources.GetObject("lblVersion.Location");

		this.lblVersion.Name = "lblVersion";

		this.lblVersion.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("lblVersion.RightToLeft");

		this.lblVersion.Size = (System.Drawing.Size) resources.GetObject("lblVersion.Size");

		this.lblVersion.TabIndex = (int) resources.GetObject("lblVersion.TabIndex");

		this.lblVersion.Text = resources.GetString("lblVersion.Text");

		this.lblVersion.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblVersion.TextAlign");

		this.lblVersion.Visible = (bool) resources.GetObject("lblVersion.Visible");

		//

		//lblDescription

		//

		this.lblDescription.AccessibleDescription = (string) resources.GetObject("lblDescription.AccessibleDescription");

		this.lblDescription.AccessibleName = (string) resources.GetObject("lblDescription.AccessibleName");

		this.lblDescription.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("lblDescription.Anchor");

		this.lblDescription.AutoSize = (bool) resources.GetObject("lblDescription.AutoSize");

		this.lblDescription.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("lblDescription.Dock");

		this.lblDescription.Enabled = (bool) resources.GetObject("lblDescription.Enabled");

		this.lblDescription.Font = (System.Drawing.Font) resources.GetObject("lblDescription.Font");

		this.lblDescription.Image = (System.Drawing.Image) resources.GetObject("lblDescription.Image");

		this.lblDescription.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblDescription.ImageAlign");

		this.lblDescription.ImageIndex = (int) resources.GetObject("lblDescription.ImageIndex");

		this.lblDescription.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("lblDescription.ImeMode");

		this.lblDescription.Location = (System.Drawing.Point) resources.GetObject("lblDescription.Location");

		this.lblDescription.Name = "lblDescription";

		this.lblDescription.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("lblDescription.RightToLeft");

		this.lblDescription.Size = (System.Drawing.Size) resources.GetObject("lblDescription.Size");

		this.lblDescription.TabIndex = (int) resources.GetObject("lblDescription.TabIndex");

		this.lblDescription.Text = resources.GetString("lblDescription.Text");

		this.lblDescription.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblDescription.TextAlign");

		this.lblDescription.Visible = (bool) resources.GetObject("lblDescription.Visible");

		//

		//btnOK

		//

		this.btnOK.AccessibleDescription = (string) resources.GetObject("btnOK.AccessibleDescription");

		this.btnOK.AccessibleName = (string) resources.GetObject("btnOK.AccessibleName");

		this.btnOK.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("btnOK.Anchor");

		this.btnOK.BackgroundImage = (System.Drawing.Image) resources.GetObject("btnOK.BackgroundImage");

		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;

		this.btnOK.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("btnOK.Dock");

		this.btnOK.Enabled = (bool) resources.GetObject("btnOK.Enabled");

		this.btnOK.FlatStyle = (System.Windows.Forms.FlatStyle) resources.GetObject("btnOK.FlatStyle");

		this.btnOK.Font = (System.Drawing.Font) resources.GetObject("btnOK.Font");

		this.btnOK.Image = (System.Drawing.Image) resources.GetObject("btnOK.Image");

		this.btnOK.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnOK.ImageAlign");

		this.btnOK.ImageIndex = (int) resources.GetObject("btnOK.ImageIndex");

		this.btnOK.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("btnOK.ImeMode");

		this.btnOK.Location = (System.Drawing.Point) resources.GetObject("btnOK.Location");

		this.btnOK.Name = "btnOK";

		this.btnOK.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("btnOK.RightToLeft");

		this.btnOK.Size = (System.Drawing.Size) resources.GetObject("btnOK.Size");

		this.btnOK.TabIndex = (int) resources.GetObject("btnOK.TabIndex");

		this.btnOK.Text = resources.GetString("btnOK.Text");

		this.btnOK.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("btnOK.TextAlign");

		this.btnOK.Visible = (bool) resources.GetObject("btnOK.Visible");

		//

		//lblCopyright

		//

		this.lblCopyright.AccessibleDescription = (string) resources.GetObject("lblCopyright.AccessibleDescription");

		this.lblCopyright.AccessibleName = (string) resources.GetObject("lblCopyright.AccessibleName");

		this.lblCopyright.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("lblCopyright.Anchor");

		this.lblCopyright.AutoSize = (bool) resources.GetObject("lblCopyright.AutoSize");

		this.lblCopyright.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("lblCopyright.Dock");

		this.lblCopyright.Enabled = (bool) resources.GetObject("lblCopyright.Enabled");

		this.lblCopyright.Font = (System.Drawing.Font) resources.GetObject("lblCopyright.Font");

		this.lblCopyright.Image = (System.Drawing.Image) resources.GetObject("lblCopyright.Image");

		this.lblCopyright.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblCopyright.ImageAlign");

		this.lblCopyright.ImageIndex = (int) resources.GetObject("lblCopyright.ImageIndex");

		this.lblCopyright.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("lblCopyright.ImeMode");

		this.lblCopyright.Location = (System.Drawing.Point) resources.GetObject("lblCopyright.Location");

		this.lblCopyright.Name = "lblCopyright";

		this.lblCopyright.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("lblCopyright.RightToLeft");

		this.lblCopyright.Size = (System.Drawing.Size) resources.GetObject("lblCopyright.Size");

		this.lblCopyright.TabIndex = (int) resources.GetObject("lblCopyright.TabIndex");

		this.lblCopyright.Text = resources.GetString("lblCopyright.Text");

		this.lblCopyright.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblCopyright.TextAlign");

		this.lblCopyright.Visible = (bool) resources.GetObject("lblCopyright.Visible");

		//

		//lblCodebase

		//

		this.lblCodebase.AccessibleDescription = (string) resources.GetObject("lblCodebase.AccessibleDescription");

		this.lblCodebase.AccessibleName = (string) resources.GetObject("lblCodebase.AccessibleName");

		this.lblCodebase.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("lblCodebase.Anchor");

		this.lblCodebase.AutoSize = (bool) resources.GetObject("lblCodebase.AutoSize");

		this.lblCodebase.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("lblCodebase.Dock");

		this.lblCodebase.Enabled = (bool) resources.GetObject("lblCodebase.Enabled");

		this.lblCodebase.Font = (System.Drawing.Font) resources.GetObject("lblCodebase.Font");

		this.lblCodebase.Image = (System.Drawing.Image) resources.GetObject("lblCodebase.Image");

		this.lblCodebase.ImageAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblCodebase.ImageAlign");

		this.lblCodebase.ImageIndex = (int) resources.GetObject("lblCodebase.ImageIndex");

		this.lblCodebase.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("lblCodebase.ImeMode");

		this.lblCodebase.Location = (System.Drawing.Point) resources.GetObject("lblCodebase.Location");

		this.lblCodebase.Name = "lblCodebase";

		this.lblCodebase.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("lblCodebase.RightToLeft");

		this.lblCodebase.Size = (System.Drawing.Size) resources.GetObject("lblCodebase.Size");

		this.lblCodebase.TabIndex = (int) resources.GetObject("lblCodebase.TabIndex");

		this.lblCodebase.Text = resources.GetString("lblCodebase.Text");

		this.lblCodebase.TextAlign = (System.Drawing.ContentAlignment) resources.GetObject("lblCodebase.TextAlign");

		this.lblCodebase.Visible = (bool) resources.GetObject("lblCodebase.Visible");

		//

		//frmAbout

		//

		this.AcceptButton = this.btnOK;

		this.AccessibleDescription = (string) resources.GetObject("$this.AccessibleDescription");

		this.AccessibleName = (string) resources.GetObject("$this.AccessibleName");

		this.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("$this.Anchor");

		this.AutoScaleBaseSize = (System.Drawing.Size) resources.GetObject("$this.AutoScaleBaseSize");

		this.AutoScroll = (bool) resources.GetObject("$this.AutoScroll");

		this.AutoScrollMargin = (System.Drawing.Size) resources.GetObject("$this.AutoScrollMargin");

		this.AutoScrollMinSize = (System.Drawing.Size) resources.GetObject("$this.AutoScrollMinSize");

		this.BackgroundImage = (System.Drawing.Image) resources.GetObject("$this.BackgroundImage");

		this.CancelButton = this.btnOK;

		this.ClientSize = (System.Drawing.Size) resources.GetObject("$this.ClientSize");

		this.Controls.AddRange(new System.Windows.Forms.Control[] {this.lblCodebase, this.lblCopyright, this.btnOK, this.lblDescription, this.lblVersion, this.lblTitle, this.pbIcon});

		this.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("$this.Dock");

		this.Enabled = (bool) resources.GetObject("$this.Enabled");

		this.Font = (System.Drawing.Font) resources.GetObject("$this.Font");

		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;

		this.Icon = (System.Drawing.Icon) resources.GetObject("$this.Icon");

		this.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("$this.ImeMode");

		this.Location = (System.Drawing.Point) resources.GetObject("$this.Location");

		this.MaximizeBox = false;

		this.MaximumSize = (System.Drawing.Size) resources.GetObject("$this.MaximumSize");

		this.MinimizeBox = false;

		this.MinimumSize = (System.Drawing.Size) resources.GetObject("$this.MinimumSize");

		this.Name = "frmAbout";

		this.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("$this.RightToLeft");

		this.ShowInTaskbar = false;

		this.StartPosition = (System.Windows.Forms.FormStartPosition) resources.GetObject("$this.StartPosition");

		this.Text = resources.GetString("$this.Text");

		this.Visible = (bool) resources.GetObject("$this.Visible");

		this.ResumeLayout(false);

	}

#endregion

	// Note: Because this form is opened by frmMain using the ShowDialog command, we simply set the
	// DialogResult property of cmdOK to OK which cause the form to close when clicked.

	private void frmAbout_Load(object sender, System.EventArgs e) {

		try {

			// Set this Form's Text + Icon properties by using values from the parent form

			this.Text = "About " + this.Owner.Text + " ...";
			this.Icon = this.Owner.Icon;

			// Set this Form's Picture Box's image using the parent's icon 
			// However, we need to convert it to a Bitmap since the Picture Box Control
			// will not accept a raw Icon.

			this.pbIcon.Image = this.Owner.Icon.ToBitmap();

			// Set the labels identitying the Title, Version, and Description by
			// reading Assembly meta-data originally entered in the AssemblyInfo.cs file
			// using the AssemblyInfo class defined in the same file

			AssemblyInfo ainfo = new AssemblyInfo();

			this.lblTitle.Text = ainfo.Title;

			this.lblVersion.Text = string.Format("Version {0}", ainfo.Version);

			this.lblCopyright.Text = ainfo.Copyright;

			this.lblDescription.Text = ainfo.Description;

			this.lblCodebase.Text = ainfo.CodeBase;

		} 
		catch(System.Exception exp) {

			// This catch will trap any unexpected error.

			MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
	}
}

