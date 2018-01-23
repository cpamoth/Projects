//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

using System;

using System.Windows.Forms;

public class OpenFiles:Form
{

    // A window which allows the user to select both Assembly and XML

    // Documentation files to open.

#region " Windows Form Designer generated code "

    public () {

        

        //This call is required by the Windows Form Designer.

        InitializeComponent();

        //Add any initialization after the InitializeComponent() call

    }

    //Form overrides dispose to clean up the component list.

    protected override void Dispose(bool disposing) {

        base.Dispose(disposing);

        if (components != null) {

            components.Dispose();

        }

    }

    private OpenFileDialog System.Windows.Forms.OpenFileDialog;

    private AsmPathInputBox System.Windows.Forms.TextBox;

    private XMLPathInputBox System.Windows.Forms.TextBox;

    private BrowseAsmButton System.Windows.Forms.Button;

    private BrowseXMLButton System.Windows.Forms.Button;

    private ButtonOK System.Windows.Forms.Button;

    private ButtonCancel System.Windows.Forms.Button;

    private LabelInstruction System.Windows.Forms.Label;

    //Required by the Windows Form Designer

    private components System.ComponentModel.Container;

    //NOTE: The following procedure is required by the Windows Form Designer

    //It can be modified using the Windows Form Designer.  

    //Do ! modify it using the code editor.

    public Label1 System.Windows.Forms.Label;

    public Label2 System.Windows.Forms.Label;

    private void InitializeComponent() {

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(OpenFiles));

        this.LabelInstruction = new System.Windows.Forms.Label();

        this.AsmPathInputBox = new System.Windows.Forms.TextBox();

        this.XMLPathInputBox = new System.Windows.Forms.TextBox();

        this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();

        this.BrowseAsmButton = new System.Windows.Forms.Button();

        this.BrowseXMLButton = new System.Windows.Forms.Button();

        this.ButtonOK = new System.Windows.Forms.Button();

        this.ButtonCancel = new System.Windows.Forms.Button();

        this.Label1 = new System.Windows.Forms.Label();

        this.Label2 = new System.Windows.Forms.Label();

        this.SuspendLayout();

        //

        //LabelInstruction

        //

        this.LabelInstruction.AccessibleDescription = CType(resources.GetObject("LabelInstruction.AccessibleDescription"), string);

        this.LabelInstruction.AccessibleName = CType(resources.GetObject("LabelInstruction.AccessibleName"), string);

        this.LabelInstruction.Anchor = CType(resources.GetObject("LabelInstruction.Anchor"), System.Windows.Forms.AnchorStyles);

        this.LabelInstruction.AutoSize = CType(resources.GetObject("LabelInstruction.AutoSize"), bool);

        this.LabelInstruction.Dock = CType(resources.GetObject("LabelInstruction.Dock"), System.Windows.Forms.DockStyle);

        this.LabelInstruction.Enabled = CType(resources.GetObject("LabelInstruction.Enabled"), bool);

        this.LabelInstruction.Font = CType(resources.GetObject("LabelInstruction.Font"), System.Drawing.Font);

        this.LabelInstruction.Image = CType(resources.GetObject("LabelInstruction.Image"), System.Drawing.Image);

        this.LabelInstruction.ImageAlign = CType(resources.GetObject("LabelInstruction.ImageAlign"), System.Drawing.ContentAlignment);

        this.LabelInstruction.ImageIndex = CType(resources.GetObject("LabelInstruction.ImageIndex"), int);

        this.LabelInstruction.ImeMode = CType(resources.GetObject("LabelInstruction.ImeMode"), System.Windows.Forms.ImeMode);

        this.LabelInstruction.Location = CType(resources.GetObject("LabelInstruction.Location"), System.Drawing.Point);

        this.LabelInstruction.Name = "LabelInstruction";

        this.LabelInstruction.RightToLeft = CType(resources.GetObject("LabelInstruction.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.LabelInstruction.Size = CType(resources.GetObject("LabelInstruction.Size"), System.Drawing.Size);

        this.LabelInstruction.TabIndex = CType(resources.GetObject("LabelInstruction.TabIndex"), int);

        this.LabelInstruction.Text = resources.GetString("LabelInstruction.Text");

        this.LabelInstruction.TextAlign = CType(resources.GetObject("LabelInstruction.TextAlign"), System.Drawing.ContentAlignment);

        this.LabelInstruction.Visible = CType(resources.GetObject("LabelInstruction.Visible"), bool);

        //

        //AsmPathInputBox

        //

        this.AsmPathInputBox.AccessibleDescription = CType(resources.GetObject("AsmPathInputBox.AccessibleDescription"), string);

        this.AsmPathInputBox.AccessibleName = CType(resources.GetObject("AsmPathInputBox.AccessibleName"), string);

        this.AsmPathInputBox.Anchor = CType(resources.GetObject("AsmPathInputBox.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AsmPathInputBox.AutoSize = CType(resources.GetObject("AsmPathInputBox.AutoSize"), bool);

        this.AsmPathInputBox.BackgroundImage = CType(resources.GetObject("AsmPathInputBox.BackgroundImage"), System.Drawing.Image);

        this.AsmPathInputBox.Dock = CType(resources.GetObject("AsmPathInputBox.Dock"), System.Windows.Forms.DockStyle);

        this.AsmPathInputBox.Enabled = CType(resources.GetObject("AsmPathInputBox.Enabled"), bool);

        this.AsmPathInputBox.Font = CType(resources.GetObject("AsmPathInputBox.Font"), System.Drawing.Font);

        this.AsmPathInputBox.ImeMode = CType(resources.GetObject("AsmPathInputBox.ImeMode"), System.Windows.Forms.ImeMode);

        this.AsmPathInputBox.Location = CType(resources.GetObject("AsmPathInputBox.Location"), System.Drawing.Point);

        this.AsmPathInputBox.MaxLength = CType(resources.GetObject("AsmPathInputBox.MaxLength"), int);

        this.AsmPathInputBox.Multiline = CType(resources.GetObject("AsmPathInputBox.Multiline"), bool);

        this.AsmPathInputBox.Name = "AsmPathInputBox";

        this.AsmPathInputBox.PasswordChar = CType(resources.GetObject("AsmPathInputBox.PasswordChar"), char);

        this.AsmPathInputBox.RightToLeft = CType(resources.GetObject("AsmPathInputBox.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.AsmPathInputBox.ScrollBars = CType(resources.GetObject("AsmPathInputBox.ScrollBars"), System.Windows.Forms.ScrollBars);

        this.AsmPathInputBox.Size = CType(resources.GetObject("AsmPathInputBox.Size"), System.Drawing.Size);

        this.AsmPathInputBox.TabIndex = CType(resources.GetObject("AsmPathInputBox.TabIndex"), int);

        this.AsmPathInputBox.Text = resources.GetString("AsmPathInputBox.Text");

        this.AsmPathInputBox.TextAlign = CType(resources.GetObject("AsmPathInputBox.TextAlign"), System.Windows.Forms.HorizontalAlignment);

        this.AsmPathInputBox.Visible = CType(resources.GetObject("AsmPathInputBox.Visible"), bool);

        this.AsmPathInputBox.WordWrap = CType(resources.GetObject("AsmPathInputBox.WordWrap"), bool);

        //

        //XMLPathInputBox

        //

        this.XMLPathInputBox.AccessibleDescription = CType(resources.GetObject("XMLPathInputBox.AccessibleDescription"), string);

        this.XMLPathInputBox.AccessibleName = CType(resources.GetObject("XMLPathInputBox.AccessibleName"), string);

        this.XMLPathInputBox.Anchor = CType(resources.GetObject("XMLPathInputBox.Anchor"), System.Windows.Forms.AnchorStyles);

        this.XMLPathInputBox.AutoSize = CType(resources.GetObject("XMLPathInputBox.AutoSize"), bool);

        this.XMLPathInputBox.BackgroundImage = CType(resources.GetObject("XMLPathInputBox.BackgroundImage"), System.Drawing.Image);

        this.XMLPathInputBox.Dock = CType(resources.GetObject("XMLPathInputBox.Dock"), System.Windows.Forms.DockStyle);

        this.XMLPathInputBox.Enabled = CType(resources.GetObject("XMLPathInputBox.Enabled"), bool);

        this.XMLPathInputBox.Font = CType(resources.GetObject("XMLPathInputBox.Font"), System.Drawing.Font);

        this.XMLPathInputBox.ImeMode = CType(resources.GetObject("XMLPathInputBox.ImeMode"), System.Windows.Forms.ImeMode);

        this.XMLPathInputBox.Location = CType(resources.GetObject("XMLPathInputBox.Location"), System.Drawing.Point);

        this.XMLPathInputBox.MaxLength = CType(resources.GetObject("XMLPathInputBox.MaxLength"), int);

        this.XMLPathInputBox.Multiline = CType(resources.GetObject("XMLPathInputBox.Multiline"), bool);

        this.XMLPathInputBox.Name = "XMLPathInputBox";

        this.XMLPathInputBox.PasswordChar = CType(resources.GetObject("XMLPathInputBox.PasswordChar"), char);

        this.XMLPathInputBox.RightToLeft = CType(resources.GetObject("XMLPathInputBox.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.XMLPathInputBox.ScrollBars = CType(resources.GetObject("XMLPathInputBox.ScrollBars"), System.Windows.Forms.ScrollBars);

        this.XMLPathInputBox.Size = CType(resources.GetObject("XMLPathInputBox.Size"), System.Drawing.Size);

        this.XMLPathInputBox.TabIndex = CType(resources.GetObject("XMLPathInputBox.TabIndex"), int);

        this.XMLPathInputBox.Text = resources.GetString("XMLPathInputBox.Text");

        this.XMLPathInputBox.TextAlign = CType(resources.GetObject("XMLPathInputBox.TextAlign"), System.Windows.Forms.HorizontalAlignment);

        this.XMLPathInputBox.Visible = CType(resources.GetObject("XMLPathInputBox.Visible"), bool);

        this.XMLPathInputBox.WordWrap = CType(resources.GetObject("XMLPathInputBox.WordWrap"), bool);

        //

        //OpenFileDialog

        //

        this.OpenFileDialog.Filter = resources.GetString("OpenFileDialog.Filter");

        this.OpenFileDialog.Title = resources.GetString("OpenFileDialog.Title");

        //

        //BrowseAsmButton

        //

        this.BrowseAsmButton.AccessibleDescription = CType(resources.GetObject("BrowseAsmButton.AccessibleDescription"), string);

        this.BrowseAsmButton.AccessibleName = CType(resources.GetObject("BrowseAsmButton.AccessibleName"), string);

        this.BrowseAsmButton.Anchor = CType(resources.GetObject("BrowseAsmButton.Anchor"), System.Windows.Forms.AnchorStyles);

        this.BrowseAsmButton.BackgroundImage = CType(resources.GetObject("BrowseAsmButton.BackgroundImage"), System.Drawing.Image);

        this.BrowseAsmButton.Dock = CType(resources.GetObject("BrowseAsmButton.Dock"), System.Windows.Forms.DockStyle);

        this.BrowseAsmButton.Enabled = CType(resources.GetObject("BrowseAsmButton.Enabled"), bool);

        this.BrowseAsmButton.FlatStyle = CType(resources.GetObject("BrowseAsmButton.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.BrowseAsmButton.Font = CType(resources.GetObject("BrowseAsmButton.Font"), System.Drawing.Font);

        this.BrowseAsmButton.Image = CType(resources.GetObject("BrowseAsmButton.Image"), System.Drawing.Image);

        this.BrowseAsmButton.ImageAlign = CType(resources.GetObject("BrowseAsmButton.ImageAlign"), System.Drawing.ContentAlignment);

        this.BrowseAsmButton.ImageIndex = CType(resources.GetObject("BrowseAsmButton.ImageIndex"), int);

        this.BrowseAsmButton.ImeMode = CType(resources.GetObject("BrowseAsmButton.ImeMode"), System.Windows.Forms.ImeMode);

        this.BrowseAsmButton.Location = CType(resources.GetObject("BrowseAsmButton.Location"), System.Drawing.Point);

        this.BrowseAsmButton.Name = "BrowseAsmButton";

        this.BrowseAsmButton.RightToLeft = CType(resources.GetObject("BrowseAsmButton.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.BrowseAsmButton.Size = CType(resources.GetObject("BrowseAsmButton.Size"), System.Drawing.Size);

        this.BrowseAsmButton.TabIndex = CType(resources.GetObject("BrowseAsmButton.TabIndex"), int);

        this.BrowseAsmButton.Text = resources.GetString("BrowseAsmButton.Text");

        this.BrowseAsmButton.TextAlign = CType(resources.GetObject("BrowseAsmButton.TextAlign"), System.Drawing.ContentAlignment);

        this.BrowseAsmButton.Visible = CType(resources.GetObject("BrowseAsmButton.Visible"), bool);

        //

        //BrowseXMLButton

        //

        this.BrowseXMLButton.AccessibleDescription = CType(resources.GetObject("BrowseXMLButton.AccessibleDescription"), string);

        this.BrowseXMLButton.AccessibleName = CType(resources.GetObject("BrowseXMLButton.AccessibleName"), string);

        this.BrowseXMLButton.Anchor = CType(resources.GetObject("BrowseXMLButton.Anchor"), System.Windows.Forms.AnchorStyles);

        this.BrowseXMLButton.BackgroundImage = CType(resources.GetObject("BrowseXMLButton.BackgroundImage"), System.Drawing.Image);

        this.BrowseXMLButton.Dock = CType(resources.GetObject("BrowseXMLButton.Dock"), System.Windows.Forms.DockStyle);

        this.BrowseXMLButton.Enabled = CType(resources.GetObject("BrowseXMLButton.Enabled"), bool);

        this.BrowseXMLButton.FlatStyle = CType(resources.GetObject("BrowseXMLButton.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.BrowseXMLButton.Font = CType(resources.GetObject("BrowseXMLButton.Font"), System.Drawing.Font);

        this.BrowseXMLButton.Image = CType(resources.GetObject("BrowseXMLButton.Image"), System.Drawing.Image);

        this.BrowseXMLButton.ImageAlign = CType(resources.GetObject("BrowseXMLButton.ImageAlign"), System.Drawing.ContentAlignment);

        this.BrowseXMLButton.ImageIndex = CType(resources.GetObject("BrowseXMLButton.ImageIndex"), int);

        this.BrowseXMLButton.ImeMode = CType(resources.GetObject("BrowseXMLButton.ImeMode"), System.Windows.Forms.ImeMode);

        this.BrowseXMLButton.Location = CType(resources.GetObject("BrowseXMLButton.Location"), System.Drawing.Point);

        this.BrowseXMLButton.Name = "BrowseXMLButton";

        this.BrowseXMLButton.RightToLeft = CType(resources.GetObject("BrowseXMLButton.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.BrowseXMLButton.Size = CType(resources.GetObject("BrowseXMLButton.Size"), System.Drawing.Size);

        this.BrowseXMLButton.TabIndex = CType(resources.GetObject("BrowseXMLButton.TabIndex"), int);

        this.BrowseXMLButton.Text = resources.GetString("BrowseXMLButton.Text");

        this.BrowseXMLButton.TextAlign = CType(resources.GetObject("BrowseXMLButton.TextAlign"), System.Drawing.ContentAlignment);

        this.BrowseXMLButton.Visible = CType(resources.GetObject("BrowseXMLButton.Visible"), bool);

        //

        //ButtonOK

        //

        this.ButtonOK.AccessibleDescription = CType(resources.GetObject("ButtonOK.AccessibleDescription"), string);

        this.ButtonOK.AccessibleName = CType(resources.GetObject("ButtonOK.AccessibleName"), string);

        this.ButtonOK.Anchor = CType(resources.GetObject("ButtonOK.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ButtonOK.BackgroundImage = CType(resources.GetObject("ButtonOK.BackgroundImage"), System.Drawing.Image);

        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;

        this.ButtonOK.Dock = CType(resources.GetObject("ButtonOK.Dock"), System.Windows.Forms.DockStyle);

        this.ButtonOK.Enabled = CType(resources.GetObject("ButtonOK.Enabled"), bool);

        this.ButtonOK.FlatStyle = CType(resources.GetObject("ButtonOK.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.ButtonOK.Font = CType(resources.GetObject("ButtonOK.Font"), System.Drawing.Font);

        this.ButtonOK.Image = CType(resources.GetObject("ButtonOK.Image"), System.Drawing.Image);

        this.ButtonOK.ImageAlign = CType(resources.GetObject("ButtonOK.ImageAlign"), System.Drawing.ContentAlignment);

        this.ButtonOK.ImageIndex = CType(resources.GetObject("ButtonOK.ImageIndex"), int);

        this.ButtonOK.ImeMode = CType(resources.GetObject("ButtonOK.ImeMode"), System.Windows.Forms.ImeMode);

        this.ButtonOK.Location = CType(resources.GetObject("ButtonOK.Location"), System.Drawing.Point);

        this.ButtonOK.Name = "ButtonOK";

        this.ButtonOK.RightToLeft = CType(resources.GetObject("ButtonOK.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ButtonOK.Size = CType(resources.GetObject("ButtonOK.Size"), System.Drawing.Size);

        this.ButtonOK.TabIndex = CType(resources.GetObject("ButtonOK.TabIndex"), int);

        this.ButtonOK.Text = resources.GetString("ButtonOK.Text");

        this.ButtonOK.TextAlign = CType(resources.GetObject("ButtonOK.TextAlign"), System.Drawing.ContentAlignment);

        this.ButtonOK.Visible = CType(resources.GetObject("ButtonOK.Visible"), bool);

        //

        //ButtonCancel

        //

        this.ButtonCancel.AccessibleDescription = CType(resources.GetObject("ButtonCancel.AccessibleDescription"), string);

        this.ButtonCancel.AccessibleName = CType(resources.GetObject("ButtonCancel.AccessibleName"), string);

        this.ButtonCancel.Anchor = CType(resources.GetObject("ButtonCancel.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ButtonCancel.BackgroundImage = CType(resources.GetObject("ButtonCancel.BackgroundImage"), System.Drawing.Image);

        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

        this.ButtonCancel.Dock = CType(resources.GetObject("ButtonCancel.Dock"), System.Windows.Forms.DockStyle);

        this.ButtonCancel.Enabled = CType(resources.GetObject("ButtonCancel.Enabled"), bool);

        this.ButtonCancel.FlatStyle = CType(resources.GetObject("ButtonCancel.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.ButtonCancel.Font = CType(resources.GetObject("ButtonCancel.Font"), System.Drawing.Font);

        this.ButtonCancel.Image = CType(resources.GetObject("ButtonCancel.Image"), System.Drawing.Image);

        this.ButtonCancel.ImageAlign = CType(resources.GetObject("ButtonCancel.ImageAlign"), System.Drawing.ContentAlignment);

        this.ButtonCancel.ImageIndex = CType(resources.GetObject("ButtonCancel.ImageIndex"), int);

        this.ButtonCancel.ImeMode = CType(resources.GetObject("ButtonCancel.ImeMode"), System.Windows.Forms.ImeMode);

        this.ButtonCancel.Location = CType(resources.GetObject("ButtonCancel.Location"), System.Drawing.Point);

        this.ButtonCancel.Name = "ButtonCancel";

        this.ButtonCancel.RightToLeft = CType(resources.GetObject("ButtonCancel.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ButtonCancel.Size = CType(resources.GetObject("ButtonCancel.Size"), System.Drawing.Size);

        this.ButtonCancel.TabIndex = CType(resources.GetObject("ButtonCancel.TabIndex"), int);

        this.ButtonCancel.Text = resources.GetString("ButtonCancel.Text");

        this.ButtonCancel.TextAlign = CType(resources.GetObject("ButtonCancel.TextAlign"), System.Drawing.ContentAlignment);

        this.ButtonCancel.Visible = CType(resources.GetObject("ButtonCancel.Visible"), bool);

        //

        //Label1

        //

        this.Label1.AccessibleDescription = CType(resources.GetObject("Label1.AccessibleDescription"), string);

        this.Label1.AccessibleName = CType(resources.GetObject("Label1.AccessibleName"), string);

        this.Label1.Anchor = CType(resources.GetObject("Label1.Anchor"), System.Windows.Forms.AnchorStyles);

        this.Label1.AutoSize = CType(resources.GetObject("Label1.AutoSize"), bool);

        this.Label1.Dock = CType(resources.GetObject("Label1.Dock"), System.Windows.Forms.DockStyle);

        this.Label1.Enabled = CType(resources.GetObject("Label1.Enabled"), bool);

        this.Label1.Font = CType(resources.GetObject("Label1.Font"), System.Drawing.Font);

        this.Label1.Image = CType(resources.GetObject("Label1.Image"), System.Drawing.Image);

        this.Label1.ImageAlign = CType(resources.GetObject("Label1.ImageAlign"), System.Drawing.ContentAlignment);

        this.Label1.ImageIndex = CType(resources.GetObject("Label1.ImageIndex"), int);

        this.Label1.ImeMode = CType(resources.GetObject("Label1.ImeMode"), System.Windows.Forms.ImeMode);

        this.Label1.Location = CType(resources.GetObject("Label1.Location"), System.Drawing.Point);

        this.Label1.Name = "Label1";

        this.Label1.RightToLeft = CType(resources.GetObject("Label1.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.Label1.Size = CType(resources.GetObject("Label1.Size"), System.Drawing.Size);

        this.Label1.TabIndex = CType(resources.GetObject("Label1.TabIndex"), int);

        this.Label1.Text = resources.GetString("Label1.Text");

        this.Label1.TextAlign = CType(resources.GetObject("Label1.TextAlign"), System.Drawing.ContentAlignment);

        this.Label1.Visible = CType(resources.GetObject("Label1.Visible"), bool);

        //

        //Label2

        //

        this.Label2.AccessibleDescription = CType(resources.GetObject("Label2.AccessibleDescription"), string);

        this.Label2.AccessibleName = CType(resources.GetObject("Label2.AccessibleName"), string);

        this.Label2.Anchor = CType(resources.GetObject("Label2.Anchor"), System.Windows.Forms.AnchorStyles);

        this.Label2.AutoSize = CType(resources.GetObject("Label2.AutoSize"), bool);

        this.Label2.Dock = CType(resources.GetObject("Label2.Dock"), System.Windows.Forms.DockStyle);

        this.Label2.Enabled = CType(resources.GetObject("Label2.Enabled"), bool);

        this.Label2.Font = CType(resources.GetObject("Label2.Font"), System.Drawing.Font);

        this.Label2.Image = CType(resources.GetObject("Label2.Image"), System.Drawing.Image);

        this.Label2.ImageAlign = CType(resources.GetObject("Label2.ImageAlign"), System.Drawing.ContentAlignment);

        this.Label2.ImageIndex = CType(resources.GetObject("Label2.ImageIndex"), int);

        this.Label2.ImeMode = CType(resources.GetObject("Label2.ImeMode"), System.Windows.Forms.ImeMode);

        this.Label2.Location = CType(resources.GetObject("Label2.Location"), System.Drawing.Point);

        this.Label2.Name = "Label2";

        this.Label2.RightToLeft = CType(resources.GetObject("Label2.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.Label2.Size = CType(resources.GetObject("Label2.Size"), System.Drawing.Size);

        this.Label2.TabIndex = CType(resources.GetObject("Label2.TabIndex"), int);

        this.Label2.Text = resources.GetString("Label2.Text");

        this.Label2.TextAlign = CType(resources.GetObject("Label2.TextAlign"), System.Drawing.ContentAlignment);

        this.Label2.Visible = CType(resources.GetObject("Label2.Visible"), bool);

        //

        //OpenFiles

        //

        this.AcceptButton = this.ButtonOK;

        this.AccessibleDescription = CType(resources.GetObject("$this.AccessibleDescription"), string);

        this.AccessibleName = CType(resources.GetObject("$this.AccessibleName"), string);

        this.Anchor = CType(resources.GetObject("$this.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AutoScaleBaseSize = CType(resources.GetObject("$this.AutoScaleBaseSize"), System.Drawing.Size);

        this.AutoScroll = CType(resources.GetObject("$this.AutoScroll"), bool);

        this.AutoScrollMargin = CType(resources.GetObject("$this.AutoScrollMargin"), System.Drawing.Size);

        this.AutoScrollMinSize = CType(resources.GetObject("$this.AutoScrollMinSize"), System.Drawing.Size);

        this.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image);

        this.CancelButton = this.ButtonCancel;

        this.ClientSize = CType(resources.GetObject("$this.ClientSize"), System.Drawing.Size);

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.Label2, this.Label1, this.ButtonCancel, this.ButtonOK, this.BrowseXMLButton, this.BrowseAsmButton, this.XMLPathInputBox, this.AsmPathInputBox, this.LabelInstruction});

        this.Dock = CType(resources.GetObject("$this.Dock"), System.Windows.Forms.DockStyle);

        this.Enabled = CType(resources.GetObject("$this.Enabled"), bool);

        this.Font = CType(resources.GetObject("$this.Font"), System.Drawing.Font);

        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;

        this.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon);

        this.ImeMode = CType(resources.GetObject("$this.ImeMode"), System.Windows.Forms.ImeMode);

        this.Location = CType(resources.GetObject("$this.Location"), System.Drawing.Point);

        this.MaximizeBox = false;

        this.MaximumSize = CType(resources.GetObject("$this.MaximumSize"), System.Drawing.Size);

        this.MinimizeBox = false;

        this.MinimumSize = CType(resources.GetObject("$this.MinimumSize"), System.Drawing.Size);

        this.Name = "OpenFiles";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ShowInTaskbar = false;

        this.StartPosition = CType(resources.GetObject("$this.StartPosition"), System.Windows.Forms.FormStartPosition);

        this.Text = resources.GetString("$this.Text");

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        this.ResumeLayout(false);

    }

#endregion

    private void BrowseAsm_Click(object sender, System.EventArgs e) 

        // Open a dialog to select the Assembly file.

        OpenFileDialog.Filter = ASSEMBLY_FILE_FILTER;

        if OpenFileDialog.ShowDialog = DialogResult.OK 
			{

            AsmPathInputBox.Text = OpenFileDialog.FileName;

            //Provide the XML Documentation file path if a corresponding file exists and the user hasn't chosen one yet.

            string xmlp = IO.Path.GetDirectoryName(AsmPathInputBox.Text) + "\" + IO.Path.GetFileNameWithoutExtension(AsmPathInputBox.Text) + ".xml";

            if XMLPathInputBox.Text = "" AndAlso IO.File.Exists(xmlp) 
				{

                XMLPathInputBox.Text = xmlp;

                this.ButtonOK.Focus();

            else 
				{

                this.BrowseXMLButton.Focus();

				}

        }

    }

    private void BrowseXML_Click(object sender, System.EventArgs e) 

        // Open a dialog to select the XML Documentation file.

        OpenFileDialog.Filter = XML_FILE_FILTER;

        if OpenFileDialog.ShowDialog = DialogResult.OK {

            XMLPathInputBox.Text = OpenFileDialog.FileName;

        }

    }

    private void Path_TextChanged(object sender, System.EventArgs e) 
	{

        //Enable OK button when an assembly file is specified.

        ButtonOK.Enabled = (AsmPathInputBox.Text != "");

    }

    public readonly DATA_TYPE_HERE XMLDocumentationPath() string;

        get {

            return XMLPathInputBox.Text;

        }

    }

    public readonly DATA_TYPE_HERE AssemblyPath() string;

        get {

            return AsmPathInputBox.Text;

        }

    }

}

