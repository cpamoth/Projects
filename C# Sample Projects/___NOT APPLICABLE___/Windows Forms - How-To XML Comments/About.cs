//Copyright (C) 1999-2002 Microsoft Corporation

//All rights reserved.

//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 

//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 

//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System.Windows.Forms;

public class frmAbout

    : System.Windows.Forms.Form {

#region " Windows Form Designer generated code "

    public () {

        

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

    //Do ! modify it using the code editor.

    private System.Windows.Forms.Label lblCodebase;

    private System.Windows.Forms.Label lblCopyright;

    private System.Windows.Forms.Button cmdOK;

    private System.Windows.Forms.Label lblDescription;

    private System.Windows.Forms.Label lblVersion;

    private System.Windows.Forms.Label lblTitle;

    private System.Windows.Forms.PictureBox pbIcon;

    private void InitializeComponent() {

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(frmAbout));

        this.lblCodebase = new System.Windows.Forms.Label();

        this.lblCopyright = new System.Windows.Forms.Label();

        this.cmdOK = new System.Windows.Forms.Button();

        this.lblDescription = new System.Windows.Forms.Label();

        this.lblVersion = new System.Windows.Forms.Label();

        this.lblTitle = new System.Windows.Forms.Label();

        this.pbIcon = new System.Windows.Forms.PictureBox();

        this.SuspendLayout();

        //

        //lblCodebase

        //

        this.lblCodebase.AccessibleDescription = CType(resources.GetObject("lblCodebase.AccessibleDescription"), string);

        this.lblCodebase.AccessibleName = CType(resources.GetObject("lblCodebase.AccessibleName"), string);

        this.lblCodebase.Anchor = CType(resources.GetObject("lblCodebase.Anchor"), System.Windows.Forms.AnchorStyles);

        this.lblCodebase.AutoSize = CType(resources.GetObject("lblCodebase.AutoSize"), bool);

        this.lblCodebase.Dock = CType(resources.GetObject("lblCodebase.Dock"), System.Windows.Forms.DockStyle);

        this.lblCodebase.Enabled = CType(resources.GetObject("lblCodebase.Enabled"), bool);

        this.lblCodebase.Font = CType(resources.GetObject("lblCodebase.Font"), System.Drawing.Font);

        this.lblCodebase.Image = CType(resources.GetObject("lblCodebase.Image"), System.Drawing.Image);

        this.lblCodebase.ImageAlign = CType(resources.GetObject("lblCodebase.ImageAlign"), System.Drawing.ContentAlignment);

        this.lblCodebase.ImageIndex = CType(resources.GetObject("lblCodebase.ImageIndex"), int);

        this.lblCodebase.ImeMode = CType(resources.GetObject("lblCodebase.ImeMode"), System.Windows.Forms.ImeMode);

        this.lblCodebase.Location = CType(resources.GetObject("lblCodebase.Location"), System.Drawing.Point);

        this.lblCodebase.Name = "lblCodebase";

        this.lblCodebase.RightToLeft = CType(resources.GetObject("lblCodebase.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.lblCodebase.Size = CType(resources.GetObject("lblCodebase.Size"), System.Drawing.Size);

        this.lblCodebase.TabIndex = CType(resources.GetObject("lblCodebase.TabIndex"), int);

        this.lblCodebase.Text = resources.GetString("lblCodebase.Text");

        this.lblCodebase.TextAlign = CType(resources.GetObject("lblCodebase.TextAlign"), System.Drawing.ContentAlignment);

        this.lblCodebase.Visible = CType(resources.GetObject("lblCodebase.Visible"), bool);

        //

        //lblCopyright

        //

        this.lblCopyright.AccessibleDescription = CType(resources.GetObject("lblCopyright.AccessibleDescription"), string);

        this.lblCopyright.AccessibleName = CType(resources.GetObject("lblCopyright.AccessibleName"), string);

        this.lblCopyright.Anchor = CType(resources.GetObject("lblCopyright.Anchor"), System.Windows.Forms.AnchorStyles);

        this.lblCopyright.AutoSize = CType(resources.GetObject("lblCopyright.AutoSize"), bool);

        this.lblCopyright.Dock = CType(resources.GetObject("lblCopyright.Dock"), System.Windows.Forms.DockStyle);

        this.lblCopyright.Enabled = CType(resources.GetObject("lblCopyright.Enabled"), bool);

        this.lblCopyright.Font = CType(resources.GetObject("lblCopyright.Font"), System.Drawing.Font);

        this.lblCopyright.Image = CType(resources.GetObject("lblCopyright.Image"), System.Drawing.Image);

        this.lblCopyright.ImageAlign = CType(resources.GetObject("lblCopyright.ImageAlign"), System.Drawing.ContentAlignment);

        this.lblCopyright.ImageIndex = CType(resources.GetObject("lblCopyright.ImageIndex"), int);

        this.lblCopyright.ImeMode = CType(resources.GetObject("lblCopyright.ImeMode"), System.Windows.Forms.ImeMode);

        this.lblCopyright.Location = CType(resources.GetObject("lblCopyright.Location"), System.Drawing.Point);

        this.lblCopyright.Name = "lblCopyright";

        this.lblCopyright.RightToLeft = CType(resources.GetObject("lblCopyright.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.lblCopyright.Size = CType(resources.GetObject("lblCopyright.Size"), System.Drawing.Size);

        this.lblCopyright.TabIndex = CType(resources.GetObject("lblCopyright.TabIndex"), int);

        this.lblCopyright.Text = resources.GetString("lblCopyright.Text");

        this.lblCopyright.TextAlign = CType(resources.GetObject("lblCopyright.TextAlign"), System.Drawing.ContentAlignment);

        this.lblCopyright.Visible = CType(resources.GetObject("lblCopyright.Visible"), bool);

        //

        //cmdOK

        //

        this.cmdOK.AccessibleDescription = CType(resources.GetObject("cmdOK.AccessibleDescription"), string);

        this.cmdOK.AccessibleName = CType(resources.GetObject("cmdOK.AccessibleName"), string);

        this.cmdOK.Anchor = CType(resources.GetObject("cmdOK.Anchor"), System.Windows.Forms.AnchorStyles);

        this.cmdOK.BackgroundImage = CType(resources.GetObject("cmdOK.BackgroundImage"), System.Drawing.Image);

        this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;

        this.cmdOK.Dock = CType(resources.GetObject("cmdOK.Dock"), System.Windows.Forms.DockStyle);

        this.cmdOK.Enabled = CType(resources.GetObject("cmdOK.Enabled"), bool);

        this.cmdOK.FlatStyle = CType(resources.GetObject("cmdOK.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.cmdOK.Font = CType(resources.GetObject("cmdOK.Font"), System.Drawing.Font);

        this.cmdOK.Image = CType(resources.GetObject("cmdOK.Image"), System.Drawing.Image);

        this.cmdOK.ImageAlign = CType(resources.GetObject("cmdOK.ImageAlign"), System.Drawing.ContentAlignment);

        this.cmdOK.ImageIndex = CType(resources.GetObject("cmdOK.ImageIndex"), int);

        this.cmdOK.ImeMode = CType(resources.GetObject("cmdOK.ImeMode"), System.Windows.Forms.ImeMode);

        this.cmdOK.Location = CType(resources.GetObject("cmdOK.Location"), System.Drawing.Point);

        this.cmdOK.Name = "cmdOK";

        this.cmdOK.RightToLeft = CType(resources.GetObject("cmdOK.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.cmdOK.Size = CType(resources.GetObject("cmdOK.Size"), System.Drawing.Size);

        this.cmdOK.TabIndex = CType(resources.GetObject("cmdOK.TabIndex"), int);

        this.cmdOK.Text = resources.GetString("cmdOK.Text");

        this.cmdOK.TextAlign = CType(resources.GetObject("cmdOK.TextAlign"), System.Drawing.ContentAlignment);

        this.cmdOK.Visible = CType(resources.GetObject("cmdOK.Visible"), bool);

        //

        //lblDescription

        //

        this.lblDescription.AccessibleDescription = CType(resources.GetObject("lblDescription.AccessibleDescription"), string);

        this.lblDescription.AccessibleName = CType(resources.GetObject("lblDescription.AccessibleName"), string);

        this.lblDescription.Anchor = CType(resources.GetObject("lblDescription.Anchor"), System.Windows.Forms.AnchorStyles);

        this.lblDescription.AutoSize = CType(resources.GetObject("lblDescription.AutoSize"), bool);

        this.lblDescription.Dock = CType(resources.GetObject("lblDescription.Dock"), System.Windows.Forms.DockStyle);

        this.lblDescription.Enabled = CType(resources.GetObject("lblDescription.Enabled"), bool);

        this.lblDescription.Font = CType(resources.GetObject("lblDescription.Font"), System.Drawing.Font);

        this.lblDescription.Image = CType(resources.GetObject("lblDescription.Image"), System.Drawing.Image);

        this.lblDescription.ImageAlign = CType(resources.GetObject("lblDescription.ImageAlign"), System.Drawing.ContentAlignment);

        this.lblDescription.ImageIndex = CType(resources.GetObject("lblDescription.ImageIndex"), int);

        this.lblDescription.ImeMode = CType(resources.GetObject("lblDescription.ImeMode"), System.Windows.Forms.ImeMode);

        this.lblDescription.Location = CType(resources.GetObject("lblDescription.Location"), System.Drawing.Point);

        this.lblDescription.Name = "lblDescription";

        this.lblDescription.RightToLeft = CType(resources.GetObject("lblDescription.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.lblDescription.Size = CType(resources.GetObject("lblDescription.Size"), System.Drawing.Size);

        this.lblDescription.TabIndex = CType(resources.GetObject("lblDescription.TabIndex"), int);

        this.lblDescription.Text = resources.GetString("lblDescription.Text");

        this.lblDescription.TextAlign = CType(resources.GetObject("lblDescription.TextAlign"), System.Drawing.ContentAlignment);

        this.lblDescription.Visible = CType(resources.GetObject("lblDescription.Visible"), bool);

        //

        //lblVersion

        //

        this.lblVersion.AccessibleDescription = CType(resources.GetObject("lblVersion.AccessibleDescription"), string);

        this.lblVersion.AccessibleName = CType(resources.GetObject("lblVersion.AccessibleName"), string);

        this.lblVersion.Anchor = CType(resources.GetObject("lblVersion.Anchor"), System.Windows.Forms.AnchorStyles);

        this.lblVersion.AutoSize = CType(resources.GetObject("lblVersion.AutoSize"), bool);

        this.lblVersion.Dock = CType(resources.GetObject("lblVersion.Dock"), System.Windows.Forms.DockStyle);

        this.lblVersion.Enabled = CType(resources.GetObject("lblVersion.Enabled"), bool);

        this.lblVersion.Font = CType(resources.GetObject("lblVersion.Font"), System.Drawing.Font);

        this.lblVersion.Image = CType(resources.GetObject("lblVersion.Image"), System.Drawing.Image);

        this.lblVersion.ImageAlign = CType(resources.GetObject("lblVersion.ImageAlign"), System.Drawing.ContentAlignment);

        this.lblVersion.ImageIndex = CType(resources.GetObject("lblVersion.ImageIndex"), int);

        this.lblVersion.ImeMode = CType(resources.GetObject("lblVersion.ImeMode"), System.Windows.Forms.ImeMode);

        this.lblVersion.Location = CType(resources.GetObject("lblVersion.Location"), System.Drawing.Point);

        this.lblVersion.Name = "lblVersion";

        this.lblVersion.RightToLeft = CType(resources.GetObject("lblVersion.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.lblVersion.Size = CType(resources.GetObject("lblVersion.Size"), System.Drawing.Size);

        this.lblVersion.TabIndex = CType(resources.GetObject("lblVersion.TabIndex"), int);

        this.lblVersion.Text = resources.GetString("lblVersion.Text");

        this.lblVersion.TextAlign = CType(resources.GetObject("lblVersion.TextAlign"), System.Drawing.ContentAlignment);

        this.lblVersion.Visible = CType(resources.GetObject("lblVersion.Visible"), bool);

        //

        //lblTitle

        //

        this.lblTitle.AccessibleDescription = CType(resources.GetObject("lblTitle.AccessibleDescription"), string);

        this.lblTitle.AccessibleName = CType(resources.GetObject("lblTitle.AccessibleName"), string);

        this.lblTitle.Anchor = CType(resources.GetObject("lblTitle.Anchor"), System.Windows.Forms.AnchorStyles);

        this.lblTitle.AutoSize = CType(resources.GetObject("lblTitle.AutoSize"), bool);

        this.lblTitle.Dock = CType(resources.GetObject("lblTitle.Dock"), System.Windows.Forms.DockStyle);

        this.lblTitle.Enabled = CType(resources.GetObject("lblTitle.Enabled"), bool);

        this.lblTitle.Font = CType(resources.GetObject("lblTitle.Font"), System.Drawing.Font);

        this.lblTitle.Image = CType(resources.GetObject("lblTitle.Image"), System.Drawing.Image);

        this.lblTitle.ImageAlign = CType(resources.GetObject("lblTitle.ImageAlign"), System.Drawing.ContentAlignment);

        this.lblTitle.ImageIndex = CType(resources.GetObject("lblTitle.ImageIndex"), int);

        this.lblTitle.ImeMode = CType(resources.GetObject("lblTitle.ImeMode"), System.Windows.Forms.ImeMode);

        this.lblTitle.Location = CType(resources.GetObject("lblTitle.Location"), System.Drawing.Point);

        this.lblTitle.Name = "lblTitle";

        this.lblTitle.RightToLeft = CType(resources.GetObject("lblTitle.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.lblTitle.Size = CType(resources.GetObject("lblTitle.Size"), System.Drawing.Size);

        this.lblTitle.TabIndex = CType(resources.GetObject("lblTitle.TabIndex"), int);

        this.lblTitle.Text = resources.GetString("lblTitle.Text");

        this.lblTitle.TextAlign = CType(resources.GetObject("lblTitle.TextAlign"), System.Drawing.ContentAlignment);

        this.lblTitle.Visible = CType(resources.GetObject("lblTitle.Visible"), bool);

        //

        //pbIcon

        //

        this.pbIcon.AccessibleDescription = CType(resources.GetObject("pbIcon.AccessibleDescription"), string);

        this.pbIcon.AccessibleName = CType(resources.GetObject("pbIcon.AccessibleName"), string);

        this.pbIcon.Anchor = CType(resources.GetObject("pbIcon.Anchor"), System.Windows.Forms.AnchorStyles);

        this.pbIcon.BackgroundImage = CType(resources.GetObject("pbIcon.BackgroundImage"), System.Drawing.Image);

        this.pbIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

        this.pbIcon.Dock = CType(resources.GetObject("pbIcon.Dock"), System.Windows.Forms.DockStyle);

        this.pbIcon.Enabled = CType(resources.GetObject("pbIcon.Enabled"), bool);

        this.pbIcon.Font = CType(resources.GetObject("pbIcon.Font"), System.Drawing.Font);

        this.pbIcon.Image = CType(resources.GetObject("pbIcon.Image"), System.Drawing.Image);

        this.pbIcon.ImeMode = CType(resources.GetObject("pbIcon.ImeMode"), System.Windows.Forms.ImeMode);

        this.pbIcon.Location = CType(resources.GetObject("pbIcon.Location"), System.Drawing.Point);

        this.pbIcon.Name = "pbIcon";

        this.pbIcon.RightToLeft = CType(resources.GetObject("pbIcon.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.pbIcon.Size = CType(resources.GetObject("pbIcon.Size"), System.Drawing.Size);

        this.pbIcon.SizeMode = CType(resources.GetObject("pbIcon.SizeMode"), System.Windows.Forms.PictureBoxSizeMode);

        this.pbIcon.TabIndex = CType(resources.GetObject("pbIcon.TabIndex"), int);

        this.pbIcon.TabStop = false;

        this.pbIcon.Text = resources.GetString("pbIcon.Text");

        this.pbIcon.Visible = CType(resources.GetObject("pbIcon.Visible"), bool);

        //

        //About

        //

        this.AccessibleDescription = CType(resources.GetObject("$this.AccessibleDescription"), string);

        this.AccessibleName = CType(resources.GetObject("$this.AccessibleName"), string);

        this.Anchor = CType(resources.GetObject("$this.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AutoScaleBaseSize = CType(resources.GetObject("$this.AutoScaleBaseSize"), System.Drawing.Size);

        this.AutoScroll = CType(resources.GetObject("$this.AutoScroll"), bool);

        this.AutoScrollMargin = CType(resources.GetObject("$this.AutoScrollMargin"), System.Drawing.Size);

        this.AutoScrollMinSize = CType(resources.GetObject("$this.AutoScrollMinSize"), System.Drawing.Size);

        this.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image);

        this.ClientSize = CType(resources.GetObject("$this.ClientSize"), System.Drawing.Size);

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.lblCodebase, this.lblCopyright, this.cmdOK, this.lblDescription, this.lblVersion, this.lblTitle, this.pbIcon});

        this.Dock = CType(resources.GetObject("$this.Dock"), System.Windows.Forms.DockStyle);

        this.Enabled = CType(resources.GetObject("$this.Enabled"), bool);

        this.Font = CType(resources.GetObject("$this.Font"), System.Drawing.Font);

        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;

        this.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon);

        this.ImeMode = CType(resources.GetObject("$this.ImeMode"), System.Windows.Forms.ImeMode);

        this.Location = CType(resources.GetObject("$this.Location"), System.Drawing.Point);

        this.MaximumSize = CType(resources.GetObject("$this.MaximumSize"), System.Drawing.Size);

        this.MinimumSize = CType(resources.GetObject("$this.MinimumSize"), System.Drawing.Size);

        this.Name = "About";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.StartPosition = CType(resources.GetObject("$this.StartPosition"), System.Windows.Forms.FormStartPosition);

        this.Text = resources.GetString("$this.Text");

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        this.ResumeLayout(false);

    }

#endregion

    // Note: Because this form is opened by MainWindow using the ShowDialog command, we simply set the

    // DialogResult property of cmdOK to OK which causes the form to close when clicked.

    private void frmAbout_Load(object sender, System.EventArgs e) {

        try {

            // Set this Form's Text + Icon properties by using values from the parent form

            this.Text = "About " + this.Owner.Text;

            this.Icon = this.Owner.Icon;

            // Set this Form's Picture Box's image using the parent's icon 

            // However, we need to convert it to a Bitmap since the Picture Box Control

            // will ! accept a raw Icon.

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

        } catch(System.Exception exp) {

            // This catch will trap any unexpected error.

            MessageBox.Show(exp.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

        }

    }

}

