//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

using System.Reflection;

using System.Windows.Forms;

public class VersionInfo : Form
{
    // A form for comparing the version info between the Assembly and
    // XML Documentation files.  A property grid is used to display an instance
    // of the VersionProperties class.

    private VersionProperties m_VersionProperties;    
	//The instance to display in the property grid

#region " Windows Form Designer generated code "

    public void new(versionProperties VersionProperties);

        

        //This call is required by the Windows Form Designer.

        InitializeComponent();

        m_VersionProperties = versionProperties;

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

    public Button1 System.Windows.Forms.Button;

    public VersionInfoPropertyGrid System.Windows.Forms.PropertyGrid;

    private void InitializeComponent() {

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(VersionInfo));

        this.VersionInfoPropertyGrid = new System.Windows.Forms.PropertyGrid();

        this.Button1 = new System.Windows.Forms.Button();

        this.SuspendLayout();

        //

        //VersionInfoPropertyGrid

        //

        this.VersionInfoPropertyGrid.AccessibleDescription = CType(resources.GetObject("VersionInfoPropertyGrid.AccessibleDescription"), string);

        this.VersionInfoPropertyGrid.AccessibleName = CType(resources.GetObject("VersionInfoPropertyGrid.AccessibleName"), string);

        this.VersionInfoPropertyGrid.Anchor = CType(resources.GetObject("VersionInfoPropertyGrid.Anchor"), System.Windows.Forms.AnchorStyles);

        this.VersionInfoPropertyGrid.BackgroundImage = CType(resources.GetObject("VersionInfoPropertyGrid.BackgroundImage"), System.Drawing.Image);

        this.VersionInfoPropertyGrid.CommandsVisibleifAvailable = true;

        this.VersionInfoPropertyGrid.Dock = CType(resources.GetObject("VersionInfoPropertyGrid.Dock"), System.Windows.Forms.DockStyle);

        this.VersionInfoPropertyGrid.Enabled = CType(resources.GetObject("VersionInfoPropertyGrid.Enabled"), bool);

        this.VersionInfoPropertyGrid.Font = CType(resources.GetObject("VersionInfoPropertyGrid.Font"), System.Drawing.Font);

        this.VersionInfoPropertyGrid.HelpVisible = CType(resources.GetObject("VersionInfoPropertyGrid.HelpVisible"), bool);

        this.VersionInfoPropertyGrid.ImeMode = CType(resources.GetObject("VersionInfoPropertyGrid.ImeMode"), System.Windows.Forms.ImeMode);

        this.VersionInfoPropertyGrid.LargeButtons = false;

        this.VersionInfoPropertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;

        this.VersionInfoPropertyGrid.Location = CType(resources.GetObject("VersionInfoPropertyGrid.Location"), System.Drawing.Point);

        this.VersionInfoPropertyGrid.Name = "VersionInfoPropertyGrid";

        this.VersionInfoPropertyGrid.RightToLeft = CType(resources.GetObject("VersionInfoPropertyGrid.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.VersionInfoPropertyGrid.Size = CType(resources.GetObject("VersionInfoPropertyGrid.Size"), System.Drawing.Size);

        this.VersionInfoPropertyGrid.TabIndex = CType(resources.GetObject("VersionInfoPropertyGrid.TabIndex"), int);

        this.VersionInfoPropertyGrid.Text = resources.GetString("VersionInfoPropertyGrid.Text");

        this.VersionInfoPropertyGrid.ToolbarVisible = false;

        this.VersionInfoPropertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;

        this.VersionInfoPropertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;

        this.VersionInfoPropertyGrid.Visible = CType(resources.GetObject("VersionInfoPropertyGrid.Visible"), bool);

        //

        //Button1

        //

        this.Button1.AccessibleDescription = CType(resources.GetObject("Button1.AccessibleDescription"), string);

        this.Button1.AccessibleName = CType(resources.GetObject("Button1.AccessibleName"), string);

        this.Button1.Anchor = CType(resources.GetObject("Button1.Anchor"), System.Windows.Forms.AnchorStyles);

        this.Button1.BackgroundImage = CType(resources.GetObject("Button1.BackgroundImage"), System.Drawing.Image);

        this.Button1.DialogResult = System.Windows.Forms.DialogResult.OK;

        this.Button1.Dock = CType(resources.GetObject("Button1.Dock"), System.Windows.Forms.DockStyle);

        this.Button1.Enabled = CType(resources.GetObject("Button1.Enabled"), bool);

        this.Button1.FlatStyle = CType(resources.GetObject("Button1.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.Button1.Font = CType(resources.GetObject("Button1.Font"), System.Drawing.Font);

        this.Button1.Image = CType(resources.GetObject("Button1.Image"), System.Drawing.Image);

        this.Button1.ImageAlign = CType(resources.GetObject("Button1.ImageAlign"), System.Drawing.ContentAlignment);

        this.Button1.ImageIndex = CType(resources.GetObject("Button1.ImageIndex"), int);

        this.Button1.ImeMode = CType(resources.GetObject("Button1.ImeMode"), System.Windows.Forms.ImeMode);

        this.Button1.Location = CType(resources.GetObject("Button1.Location"), System.Drawing.Point);

        this.Button1.Name = "Button1";

        this.Button1.RightToLeft = CType(resources.GetObject("Button1.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.Button1.Size = CType(resources.GetObject("Button1.Size"), System.Drawing.Size);

        this.Button1.TabIndex = CType(resources.GetObject("Button1.TabIndex"), int);

        this.Button1.Text = resources.GetString("Button1.Text");

        this.Button1.TextAlign = CType(resources.GetObject("Button1.TextAlign"), System.Drawing.ContentAlignment);

        this.Button1.Visible = CType(resources.GetObject("Button1.Visible"), bool);

        //

        //VersionInfo

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

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.Button1, this.VersionInfoPropertyGrid});

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

        this.Name = "VersionInfo";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.StartPosition = CType(resources.GetObject("$this.StartPosition"), System.Windows.Forms.FormStartPosition);

        this.Text = resources.GetString("$this.Text");

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        this.ResumeLayout(false);

    }

#endregion

    private void VersionInfo_Load(object sender, System.EventArgs e)
{

        //When the form is loaded, put the object in the property grid.

        VersionInfoPropertyGrid.SelectedObject = m_VersionProperties;

    }

}

public class VersionData
{

    // This class holds the version data for a file.

    public Name string;

    public Version string;

    public FullName string;

    void new(Name string, Version string, FullName string)
	{

        //Build version info from strings.

        this.Name = Name;

        this.Version = Version;

        this.FullName = FullName;

    }

    void new(AsmInfo AssemblyName)
	{

        //Build version info from a reflection object.

        this.Name = AsmInfo.Name;

        this.Version = AsmInfo.Version.ToString();

        this.FullName = AsmInfo.FullName;

    }

}

public class VersionProperties
{

    // A class specifically designed for use in a property grid.  It exposes
    // the fields of a VersionData object unique properties.

    private m_AsmData VersionData;

    private m_XMLData VersionData;

    void new(AsmData VersionData)
	{

        //Use just the assembly version info.

        m_AsmData = AsmData;

        m_XMLData = new VersionData("Not Loaded", "Not Loaded", "Not Loaded");

    }

    void new(AsmData VersionData, XMLData VersionData)
	{

        //Use version info from both files.

        m_AsmData = AsmData;

        m_XMLData = XMLData;

    }

    [System.ComponentModel.Category("Assembly")]
    public readonly string AssemblyName()
		{
			get 
			{

            return m_AsmData.Name;
			}

		}

    [System.ComponentModel.Category("Assembly")]
    public readonly string AssemblyVersion()
	{

        get {

            return m_AsmData.Version;

        }

    }

    [System.ComponentModel.Category("Assembly")]
    public readonly string AssemblyFullName()
		{

        get {

            return m_AsmData.FullName;

        }

    }

    [System.ComponentModel.Category("XML Documentation File")]
    public readonly string XMLName()
	{

        get {

            return m_XMLData.Name;

        }

    }

    [System.ComponentModel.Category("XML Documentation File")]
    public readonly string XMLVersion()
	{

        get {

            return m_XMLData.Version;

        }

    }

    [System.ComponentModel.Category("XML Documentation File")]
    public readonly string XMLFullName()
	{

        get {

            return m_XMLData.FullName;

        }

    }

}

