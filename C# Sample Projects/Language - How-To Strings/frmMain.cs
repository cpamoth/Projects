//Copyright (C) 2002 Microsoft Corporatio
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System.Windows.Forms;
using System;
using System.Text;
using System.IO;

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

    private System.Windows.Forms.RadioButton optInsert;

    private System.Windows.Forms.RadioButton optRemove;

    private System.Windows.Forms.RadioButton optReplace;

    private System.Windows.Forms.RadioButton optSubstring;

    private System.Windows.Forms.RadioButton optPadRight;

    private System.Windows.Forms.RadioButton optPadLeft;

    private System.Windows.Forms.RadioButton optTrim;

    private System.Windows.Forms.RadioButton optToUpper;

    private System.Windows.Forms.RadioButton optToLower;

    private System.Windows.Forms.RadioButton optTrimStart;

    private System.Windows.Forms.RadioButton optTrimEnd;

    private System.Windows.Forms.Label lblPrm1;

    private System.Windows.Forms.Label lblPrm2;

    private System.Windows.Forms.Label lblPrm3;

    private System.Windows.Forms.TextBox txtPrm1;

    private System.Windows.Forms.TextBox txtPrm2;

    private System.Windows.Forms.TextBox txtPrm3;

    private System.Windows.Forms.TextBox txtSample;

    private System.Windows.Forms.Label lblResultsLabel;

    private System.Windows.Forms.Label lblResults;

    private System.Windows.Forms.Button btnRecalc;

    private System.Windows.Forms.Button btnRefresh;

    private System.Windows.Forms.ToolTip ttpstrings;

    private System.Windows.Forms.Panel pnlDemo;

    private System.Windows.Forms.TabControl tabstringDemo;

    private System.Windows.Forms.TabPage pagReturnstrings;

    private System.Windows.Forms.TabPage pagInfo;

    private System.Windows.Forms.RadioButton optEndsWith;

    private System.Windows.Forms.RadioButton optStartsWith;

    private System.Windows.Forms.RadioButton optLastIndexOfAny;

    private System.Windows.Forms.RadioButton optIndexOfAny;

    private System.Windows.Forms.RadioButton optIndexOf;

    private System.Windows.Forms.RadioButton optLastIndexOf;

    private System.Windows.Forms.Label Label1;

    private System.Windows.Forms.Label Label2;

    private System.Windows.Forms.TabPage pagstatic;

    private System.Windows.Forms.RadioButton optFormat;

    private System.Windows.Forms.RadioButton optConcat;

    private System.Windows.Forms.RadioButton optCompareOrdinal;

    private System.Windows.Forms.RadioButton optCompare;

    private System.Windows.Forms.RadioButton optJoin;

    private System.Windows.Forms.GroupBox grpResults;

    private System.Windows.Forms.GroupBox grpParameters;

    private System.Windows.Forms.GroupBox grpSample;

    private System.Windows.Forms.RadioButton optSplit;

    private System.Windows.Forms.Button btnstringWriter;

    private System.Windows.Forms.Button btnstringBuilder;

    private System.Windows.Forms.TabPage pagOther;

    private System.Windows.Forms.TextBox txtResults;

    private System.Windows.Forms.CheckBox chkDebug;

    private System.Windows.Forms.Button btnClear;

    private void InitializeComponent() 
	{
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
		this.mnuMain = new System.Windows.Forms.MainMenu();
		this.mnuFile = new System.Windows.Forms.MenuItem();
		this.mnuExit = new System.Windows.Forms.MenuItem();
		this.mnuHelp = new System.Windows.Forms.MenuItem();
		this.mnuAbout = new System.Windows.Forms.MenuItem();
		this.tabstringDemo = new System.Windows.Forms.TabControl();
		this.pagReturnstrings = new System.Windows.Forms.TabPage();
		this.optTrimStart = new System.Windows.Forms.RadioButton();
		this.optTrimEnd = new System.Windows.Forms.RadioButton();
		this.optTrim = new System.Windows.Forms.RadioButton();
		this.optToUpper = new System.Windows.Forms.RadioButton();
		this.optToLower = new System.Windows.Forms.RadioButton();
		this.optSubstring = new System.Windows.Forms.RadioButton();
		this.optPadRight = new System.Windows.Forms.RadioButton();
		this.optPadLeft = new System.Windows.Forms.RadioButton();
		this.optReplace = new System.Windows.Forms.RadioButton();
		this.optRemove = new System.Windows.Forms.RadioButton();
		this.optInsert = new System.Windows.Forms.RadioButton();
		this.pnlDemo = new System.Windows.Forms.Panel();
		this.grpResults = new System.Windows.Forms.GroupBox();
		this.lblResultsLabel = new System.Windows.Forms.Label();
		this.lblResults = new System.Windows.Forms.Label();
		this.btnRecalc = new System.Windows.Forms.Button();
		this.grpParameters = new System.Windows.Forms.GroupBox();
		this.Label1 = new System.Windows.Forms.Label();
		this.lblPrm2 = new System.Windows.Forms.Label();
		this.lblPrm3 = new System.Windows.Forms.Label();
		this.txtPrm1 = new System.Windows.Forms.TextBox();
		this.txtPrm2 = new System.Windows.Forms.TextBox();
		this.lblPrm1 = new System.Windows.Forms.Label();
		this.txtPrm3 = new System.Windows.Forms.TextBox();
		this.grpSample = new System.Windows.Forms.GroupBox();
		this.Label2 = new System.Windows.Forms.Label();
		this.txtSample = new System.Windows.Forms.TextBox();
		this.btnRefresh = new System.Windows.Forms.Button();
		this.pagInfo = new System.Windows.Forms.TabPage();
		this.optSplit = new System.Windows.Forms.RadioButton();
		this.optEndsWith = new System.Windows.Forms.RadioButton();
		this.optStartsWith = new System.Windows.Forms.RadioButton();
		this.optLastIndexOfAny = new System.Windows.Forms.RadioButton();
		this.optLastIndexOf = new System.Windows.Forms.RadioButton();
		this.optIndexOfAny = new System.Windows.Forms.RadioButton();
		this.optIndexOf = new System.Windows.Forms.RadioButton();
		this.pagstatic = new System.Windows.Forms.TabPage();
		this.optJoin = new System.Windows.Forms.RadioButton();
		this.optFormat = new System.Windows.Forms.RadioButton();
		this.optConcat = new System.Windows.Forms.RadioButton();
		this.optCompareOrdinal = new System.Windows.Forms.RadioButton();
		this.optCompare = new System.Windows.Forms.RadioButton();
		this.pagOther = new System.Windows.Forms.TabPage();
		this.btnClear = new System.Windows.Forms.Button();
		this.chkDebug = new System.Windows.Forms.CheckBox();
		this.txtResults = new System.Windows.Forms.TextBox();
		this.btnstringWriter = new System.Windows.Forms.Button();
		this.btnstringBuilder = new System.Windows.Forms.Button();
		this.ttpstrings = new System.Windows.Forms.ToolTip(this.components);
		this.tabstringDemo.SuspendLayout();
		this.pagReturnstrings.SuspendLayout();
		this.pnlDemo.SuspendLayout();
		this.grpResults.SuspendLayout();
		this.grpParameters.SuspendLayout();
		this.grpSample.SuspendLayout();
		this.pagInfo.SuspendLayout();
		this.pagstatic.SuspendLayout();
		this.pagOther.SuspendLayout();
		this.SuspendLayout();
		// 
		// mnuMain
		// 
		this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
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
		// tabstringDemo
		// 
		this.tabstringDemo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.tabstringDemo.Controls.Add(this.pagReturnstrings);
		this.tabstringDemo.Controls.Add(this.pagInfo);
		this.tabstringDemo.Controls.Add(this.pagstatic);
		this.tabstringDemo.Controls.Add(this.pagOther);
		this.tabstringDemo.Location = new System.Drawing.Point(8, 8);
		this.tabstringDemo.Name = "tabstringDemo";
		this.tabstringDemo.SelectedIndex = 0;
		this.tabstringDemo.Size = new System.Drawing.Size(640, 324);
		this.tabstringDemo.TabIndex = 0;
		this.tabstringDemo.SelectedIndexChanged += new System.EventHandler(this.tabstringDemo_SelectedIndexChanged);
		// 
		// pagReturnstrings
		// 
		this.pagReturnstrings.Controls.Add(this.optTrimStart);
		this.pagReturnstrings.Controls.Add(this.optTrimEnd);
		this.pagReturnstrings.Controls.Add(this.optTrim);
		this.pagReturnstrings.Controls.Add(this.optToUpper);
		this.pagReturnstrings.Controls.Add(this.optToLower);
		this.pagReturnstrings.Controls.Add(this.optSubstring);
		this.pagReturnstrings.Controls.Add(this.optPadRight);
		this.pagReturnstrings.Controls.Add(this.optPadLeft);
		this.pagReturnstrings.Controls.Add(this.optReplace);
		this.pagReturnstrings.Controls.Add(this.optRemove);
		this.pagReturnstrings.Controls.Add(this.optInsert);
		this.pagReturnstrings.Controls.Add(this.pnlDemo);
		this.pagReturnstrings.Location = new System.Drawing.Point(4, 22);
		this.pagReturnstrings.Name = "pagReturnstrings";
		this.pagReturnstrings.Size = new System.Drawing.Size(632, 298);
		this.pagReturnstrings.TabIndex = 0;
		this.pagReturnstrings.Text = "Methods that return strings";
		// 
		// optTrimStart
		// 
		this.optTrimStart.Appearance = System.Windows.Forms.Appearance.Button;
		this.optTrimStart.Location = new System.Drawing.Point(8, 248);
		this.optTrimStart.Name = "optTrimStart";
		this.optTrimStart.Size = new System.Drawing.Size(104, 22);
		this.optTrimStart.TabIndex = 10;
		this.optTrimStart.Text = "TrimStart";
		this.optTrimStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optTrimStart.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optTrimEnd
		// 
		this.optTrimEnd.Appearance = System.Windows.Forms.Appearance.Button;
		this.optTrimEnd.Location = new System.Drawing.Point(8, 224);
		this.optTrimEnd.Name = "optTrimEnd";
		this.optTrimEnd.Size = new System.Drawing.Size(104, 22);
		this.optTrimEnd.TabIndex = 9;
		this.optTrimEnd.Text = "TrimEnd";
		this.optTrimEnd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optTrimEnd.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optTrim
		// 
		this.optTrim.Appearance = System.Windows.Forms.Appearance.Button;
		this.optTrim.Location = new System.Drawing.Point(8, 200);
		this.optTrim.Name = "optTrim";
		this.optTrim.Size = new System.Drawing.Size(104, 22);
		this.optTrim.TabIndex = 8;
		this.optTrim.Text = "Trim";
		this.optTrim.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optTrim.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optToUpper
		// 
		this.optToUpper.Appearance = System.Windows.Forms.Appearance.Button;
		this.optToUpper.Location = new System.Drawing.Point(8, 176);
		this.optToUpper.Name = "optToUpper";
		this.optToUpper.Size = new System.Drawing.Size(104, 22);
		this.optToUpper.TabIndex = 7;
		this.optToUpper.Text = "ToUpper";
		this.optToUpper.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optToUpper.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optToLower
		// 
		this.optToLower.Appearance = System.Windows.Forms.Appearance.Button;
		this.optToLower.Location = new System.Drawing.Point(8, 152);
		this.optToLower.Name = "optToLower";
		this.optToLower.Size = new System.Drawing.Size(104, 22);
		this.optToLower.TabIndex = 6;
		this.optToLower.Text = "ToLower";
		this.optToLower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optToLower.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optSubstring
		// 
		this.optSubstring.Appearance = System.Windows.Forms.Appearance.Button;
		this.optSubstring.Location = new System.Drawing.Point(8, 128);
		this.optSubstring.Name = "optSubstring";
		this.optSubstring.Size = new System.Drawing.Size(104, 22);
		this.optSubstring.TabIndex = 5;
		this.optSubstring.Text = "Substring";
		this.optSubstring.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optSubstring.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optPadRight
		// 
		this.optPadRight.Appearance = System.Windows.Forms.Appearance.Button;
		this.optPadRight.Location = new System.Drawing.Point(8, 104);
		this.optPadRight.Name = "optPadRight";
		this.optPadRight.Size = new System.Drawing.Size(104, 22);
		this.optPadRight.TabIndex = 4;
		this.optPadRight.Text = "PadRight";
		this.optPadRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optPadRight.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optPadLeft
		// 
		this.optPadLeft.Appearance = System.Windows.Forms.Appearance.Button;
		this.optPadLeft.Location = new System.Drawing.Point(8, 80);
		this.optPadLeft.Name = "optPadLeft";
		this.optPadLeft.Size = new System.Drawing.Size(104, 22);
		this.optPadLeft.TabIndex = 3;
		this.optPadLeft.Text = "PadLeft";
		this.optPadLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optPadLeft.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optReplace
		// 
		this.optReplace.Appearance = System.Windows.Forms.Appearance.Button;
		this.optReplace.Location = new System.Drawing.Point(8, 56);
		this.optReplace.Name = "optReplace";
		this.optReplace.Size = new System.Drawing.Size(104, 22);
		this.optReplace.TabIndex = 2;
		this.optReplace.Text = "Replace";
		this.optReplace.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optReplace.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optRemove
		// 
		this.optRemove.Appearance = System.Windows.Forms.Appearance.Button;
		this.optRemove.Location = new System.Drawing.Point(8, 32);
		this.optRemove.Name = "optRemove";
		this.optRemove.Size = new System.Drawing.Size(104, 22);
		this.optRemove.TabIndex = 1;
		this.optRemove.Text = "Remove";
		this.optRemove.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optRemove.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optInsert
		// 
		this.optInsert.Appearance = System.Windows.Forms.Appearance.Button;
		this.optInsert.Checked = true;
		this.optInsert.Location = new System.Drawing.Point(8, 8);
		this.optInsert.Name = "optInsert";
		this.optInsert.Size = new System.Drawing.Size(104, 22);
		this.optInsert.TabIndex = 0;
		this.optInsert.TabStop = true;
		this.optInsert.Text = "Insert";
		this.optInsert.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optInsert.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// pnlDemo
		// 
		this.pnlDemo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.pnlDemo.Controls.Add(this.grpResults);
		this.pnlDemo.Controls.Add(this.grpParameters);
		this.pnlDemo.Controls.Add(this.grpSample);
		this.pnlDemo.Location = new System.Drawing.Point(120, 8);
		this.pnlDemo.Name = "pnlDemo";
		this.pnlDemo.Size = new System.Drawing.Size(504, 284);
		this.pnlDemo.TabIndex = 11;
		// 
		// grpResults
		// 
		this.grpResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.grpResults.Controls.Add(this.lblResultsLabel);
		this.grpResults.Controls.Add(this.lblResults);
		this.grpResults.Controls.Add(this.btnRecalc);
		this.grpResults.Location = new System.Drawing.Point(8, 192);
		this.grpResults.Name = "grpResults";
		this.grpResults.Size = new System.Drawing.Size(488, 76);
		this.grpResults.TabIndex = 2;
		this.grpResults.TabStop = false;
		// 
		// lblResultsLabel
		// 
		this.lblResultsLabel.AutoSize = true;
		this.lblResultsLabel.Location = new System.Drawing.Point(8, 0);
		this.lblResultsLabel.Name = "lblResultsLabel";
		this.lblResultsLabel.Size = new System.Drawing.Size(42, 16);
		this.lblResultsLabel.TabIndex = 0;
		this.lblResultsLabel.Text = "Results";
		this.lblResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblResults
		// 
		this.lblResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.lblResults.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblResults.Location = new System.Drawing.Point(16, 24);
		this.lblResults.Name = "lblResults";
		this.lblResults.Size = new System.Drawing.Size(376, 44);
		this.lblResults.TabIndex = 1;
		// 
		// btnRecalc
		// 
		this.btnRecalc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.btnRecalc.Location = new System.Drawing.Point(400, 24);
		this.btnRecalc.Name = "btnRecalc";
		this.btnRecalc.TabIndex = 2;
		this.btnRecalc.Text = "&Recalc";
		this.ttpstrings.SetToolTip(this.btnRecalc, "Recalculate the results");
		this.btnRecalc.Click += new System.EventHandler(this.btnRecalc_Click);
		// 
		// grpParameters
		// 
		this.grpParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.grpParameters.Controls.Add(this.Label1);
		this.grpParameters.Controls.Add(this.lblPrm2);
		this.grpParameters.Controls.Add(this.lblPrm3);
		this.grpParameters.Controls.Add(this.txtPrm1);
		this.grpParameters.Controls.Add(this.txtPrm2);
		this.grpParameters.Controls.Add(this.lblPrm1);
		this.grpParameters.Controls.Add(this.txtPrm3);
		this.grpParameters.Location = new System.Drawing.Point(8, 80);
		this.grpParameters.Name = "grpParameters";
		this.grpParameters.Size = new System.Drawing.Size(488, 104);
		this.grpParameters.TabIndex = 1;
		this.grpParameters.TabStop = false;
		// 
		// Label1
		// 
		this.Label1.AutoSize = true;
		this.Label1.Location = new System.Drawing.Point(8, 0);
		this.Label1.Name = "Label1";
		this.Label1.Size = new System.Drawing.Size(63, 16);
		this.Label1.TabIndex = 0;
		this.Label1.Text = "Parameters";
		this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblPrm2
		// 
		this.lblPrm2.Location = new System.Drawing.Point(16, 48);
		this.lblPrm2.Name = "lblPrm2";
		this.lblPrm2.Size = new System.Drawing.Size(120, 23);
		this.lblPrm2.TabIndex = 3;
		this.lblPrm2.Text = "Param2";
		this.lblPrm2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblPrm3
		// 
		this.lblPrm3.Location = new System.Drawing.Point(16, 72);
		this.lblPrm3.Name = "lblPrm3";
		this.lblPrm3.Size = new System.Drawing.Size(120, 23);
		this.lblPrm3.TabIndex = 5;
		this.lblPrm3.Text = "Param3";
		this.lblPrm3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtPrm1
		// 
		this.txtPrm1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtPrm1.Location = new System.Drawing.Point(136, 24);
		this.txtPrm1.Name = "txtPrm1";
		this.txtPrm1.Size = new System.Drawing.Size(200, 20);
		this.txtPrm1.TabIndex = 2;
		this.txtPrm1.Text = "";
		// 
		// txtPrm2
		// 
		this.txtPrm2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtPrm2.Location = new System.Drawing.Point(136, 48);
		this.txtPrm2.Name = "txtPrm2";
		this.txtPrm2.Size = new System.Drawing.Size(200, 20);
		this.txtPrm2.TabIndex = 4;
		this.txtPrm2.Text = "";
		// 
		// lblPrm1
		// 
		this.lblPrm1.Location = new System.Drawing.Point(16, 24);
		this.lblPrm1.Name = "lblPrm1";
		this.lblPrm1.Size = new System.Drawing.Size(120, 23);
		this.lblPrm1.TabIndex = 1;
		this.lblPrm1.Text = "Param1";
		this.lblPrm1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtPrm3
		// 
		this.txtPrm3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtPrm3.Location = new System.Drawing.Point(136, 72);
		this.txtPrm3.Name = "txtPrm3";
		this.txtPrm3.Size = new System.Drawing.Size(200, 20);
		this.txtPrm3.TabIndex = 6;
		this.txtPrm3.Text = "";
		// 
		// grpSample
		// 
		this.grpSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.grpSample.Controls.Add(this.Label2);
		this.grpSample.Controls.Add(this.txtSample);
		this.grpSample.Controls.Add(this.btnRefresh);
		this.grpSample.Location = new System.Drawing.Point(8, 8);
		this.grpSample.Name = "grpSample";
		this.grpSample.Size = new System.Drawing.Size(488, 64);
		this.grpSample.TabIndex = 0;
		this.grpSample.TabStop = false;
		// 
		// Label2
		// 
		this.Label2.AutoSize = true;
		this.Label2.Location = new System.Drawing.Point(8, 0);
		this.Label2.Name = "Label2";
		this.Label2.Size = new System.Drawing.Size(43, 16);
		this.Label2.TabIndex = 0;
		this.Label2.Text = "Sample";
		this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// txtSample
		// 
		this.txtSample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtSample.Location = new System.Drawing.Point(16, 24);
		this.txtSample.Name = "txtSample";
		this.txtSample.Size = new System.Drawing.Size(376, 20);
		this.txtSample.TabIndex = 1;
		this.txtSample.Text = "";
		// 
		// btnRefresh
		// 
		this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
		this.btnRefresh.Location = new System.Drawing.Point(400, 24);
		this.btnRefresh.Name = "btnRefresh";
		this.btnRefresh.TabIndex = 2;
		this.btnRefresh.Text = "Re&fresh";
		this.ttpstrings.SetToolTip(this.btnRefresh, "Refresh the sample text");
		this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
		// 
		// pagInfo
		// 
		this.pagInfo.Controls.Add(this.optSplit);
		this.pagInfo.Controls.Add(this.optEndsWith);
		this.pagInfo.Controls.Add(this.optStartsWith);
		this.pagInfo.Controls.Add(this.optLastIndexOfAny);
		this.pagInfo.Controls.Add(this.optLastIndexOf);
		this.pagInfo.Controls.Add(this.optIndexOfAny);
		this.pagInfo.Controls.Add(this.optIndexOf);
		this.pagInfo.Location = new System.Drawing.Point(4, 22);
		this.pagInfo.Name = "pagInfo";
		this.pagInfo.Size = new System.Drawing.Size(632, 298);
		this.pagInfo.TabIndex = 1;
		this.pagInfo.Text = "Methods that return Information";
		// 
		// optSplit
		// 
		this.optSplit.Appearance = System.Windows.Forms.Appearance.Button;
		this.optSplit.Location = new System.Drawing.Point(8, 152);
		this.optSplit.Name = "optSplit";
		this.optSplit.Size = new System.Drawing.Size(104, 22);
		this.optSplit.TabIndex = 6;
		this.optSplit.Text = "Split";
		this.optSplit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optSplit.CheckedChanged += new System.EventHandler(this.optSplit_CheckedChanged);
		// 
		// optEndsWith
		// 
		this.optEndsWith.Appearance = System.Windows.Forms.Appearance.Button;
		this.optEndsWith.Location = new System.Drawing.Point(8, 128);
		this.optEndsWith.Name = "optEndsWith";
		this.optEndsWith.Size = new System.Drawing.Size(104, 22);
		this.optEndsWith.TabIndex = 5;
		this.optEndsWith.Text = "EndsWith";
		this.optEndsWith.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optEndsWith.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optStartsWith
		// 
		this.optStartsWith.Appearance = System.Windows.Forms.Appearance.Button;
		this.optStartsWith.Location = new System.Drawing.Point(8, 104);
		this.optStartsWith.Name = "optStartsWith";
		this.optStartsWith.Size = new System.Drawing.Size(104, 22);
		this.optStartsWith.TabIndex = 4;
		this.optStartsWith.Text = "StartsWith";
		this.optStartsWith.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optStartsWith.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optLastIndexOfAny
		// 
		this.optLastIndexOfAny.Appearance = System.Windows.Forms.Appearance.Button;
		this.optLastIndexOfAny.Location = new System.Drawing.Point(8, 80);
		this.optLastIndexOfAny.Name = "optLastIndexOfAny";
		this.optLastIndexOfAny.Size = new System.Drawing.Size(104, 22);
		this.optLastIndexOfAny.TabIndex = 3;
		this.optLastIndexOfAny.Text = "LastIndexOfAny";
		this.optLastIndexOfAny.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optLastIndexOfAny.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optLastIndexOf
		// 
		this.optLastIndexOf.Appearance = System.Windows.Forms.Appearance.Button;
		this.optLastIndexOf.Location = new System.Drawing.Point(8, 56);
		this.optLastIndexOf.Name = "optLastIndexOf";
		this.optLastIndexOf.Size = new System.Drawing.Size(104, 22);
		this.optLastIndexOf.TabIndex = 2;
		this.optLastIndexOf.Text = "LastIndexOf";
		this.optLastIndexOf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optLastIndexOf.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optIndexOfAny
		// 
		this.optIndexOfAny.Appearance = System.Windows.Forms.Appearance.Button;
		this.optIndexOfAny.Location = new System.Drawing.Point(8, 32);
		this.optIndexOfAny.Name = "optIndexOfAny";
		this.optIndexOfAny.Size = new System.Drawing.Size(104, 22);
		this.optIndexOfAny.TabIndex = 1;
		this.optIndexOfAny.Text = "IndexOfAny";
		this.optIndexOfAny.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optIndexOfAny.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optIndexOf
		// 
		this.optIndexOf.Appearance = System.Windows.Forms.Appearance.Button;
		this.optIndexOf.Checked = true;
		this.optIndexOf.Location = new System.Drawing.Point(8, 8);
		this.optIndexOf.Name = "optIndexOf";
		this.optIndexOf.Size = new System.Drawing.Size(104, 22);
		this.optIndexOf.TabIndex = 0;
		this.optIndexOf.TabStop = true;
		this.optIndexOf.Text = "IndexOf";
		this.optIndexOf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optIndexOf.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// pagstatic
		// 
		this.pagstatic.Controls.Add(this.optJoin);
		this.pagstatic.Controls.Add(this.optFormat);
		this.pagstatic.Controls.Add(this.optConcat);
		this.pagstatic.Controls.Add(this.optCompareOrdinal);
		this.pagstatic.Controls.Add(this.optCompare);
		this.pagstatic.Location = new System.Drawing.Point(4, 22);
		this.pagstatic.Name = "pagstatic";
		this.pagstatic.Size = new System.Drawing.Size(632, 298);
		this.pagstatic.TabIndex = 2;
		this.pagstatic.Text = "static Methods";
		// 
		// optJoin
		// 
		this.optJoin.Appearance = System.Windows.Forms.Appearance.Button;
		this.optJoin.Location = new System.Drawing.Point(8, 104);
		this.optJoin.Name = "optJoin";
		this.optJoin.Size = new System.Drawing.Size(104, 22);
		this.optJoin.TabIndex = 4;
		this.optJoin.Text = "Join";
		this.optJoin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optJoin.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optFormat
		// 
		this.optFormat.Appearance = System.Windows.Forms.Appearance.Button;
		this.optFormat.Location = new System.Drawing.Point(8, 80);
		this.optFormat.Name = "optFormat";
		this.optFormat.Size = new System.Drawing.Size(104, 22);
		this.optFormat.TabIndex = 3;
		this.optFormat.Text = "Format";
		this.optFormat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optFormat.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optConcat
		// 
		this.optConcat.Appearance = System.Windows.Forms.Appearance.Button;
		this.optConcat.Location = new System.Drawing.Point(8, 56);
		this.optConcat.Name = "optConcat";
		this.optConcat.Size = new System.Drawing.Size(104, 22);
		this.optConcat.TabIndex = 2;
		this.optConcat.Text = "Concat";
		this.optConcat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optConcat.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optCompareOrdinal
		// 
		this.optCompareOrdinal.Appearance = System.Windows.Forms.Appearance.Button;
		this.optCompareOrdinal.Location = new System.Drawing.Point(8, 32);
		this.optCompareOrdinal.Name = "optCompareOrdinal";
		this.optCompareOrdinal.Size = new System.Drawing.Size(104, 22);
		this.optCompareOrdinal.TabIndex = 1;
		this.optCompareOrdinal.Text = "CompareOrdinal";
		this.optCompareOrdinal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optCompareOrdinal.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// optCompare
		// 
		this.optCompare.Appearance = System.Windows.Forms.Appearance.Button;
		this.optCompare.Checked = true;
		this.optCompare.Location = new System.Drawing.Point(8, 8);
		this.optCompare.Name = "optCompare";
		this.optCompare.Size = new System.Drawing.Size(104, 22);
		this.optCompare.TabIndex = 0;
		this.optCompare.TabStop = true;
		this.optCompare.Text = "Compare";
		this.optCompare.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.optCompare.CheckedChanged += new System.EventHandler(this.HandleCheckedChanged);
		// 
		// pagOther
		// 
		this.pagOther.Controls.Add(this.btnClear);
		this.pagOther.Controls.Add(this.chkDebug);
		this.pagOther.Controls.Add(this.txtResults);
		this.pagOther.Controls.Add(this.btnstringWriter);
		this.pagOther.Controls.Add(this.btnstringBuilder);
		this.pagOther.Location = new System.Drawing.Point(4, 22);
		this.pagOther.Name = "pagOther";
		this.pagOther.Size = new System.Drawing.Size(632, 298);
		this.pagOther.TabIndex = 3;
		this.pagOther.Text = "Other Classes";
		// 
		// btnClear
		// 
		this.btnClear.Location = new System.Drawing.Point(8, 104);
		this.btnClear.Name = "btnClear";
		this.btnClear.Size = new System.Drawing.Size(104, 22);
		this.btnClear.TabIndex = 4;
		this.btnClear.Text = "Clear Output";
		this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
		// 
		// chkDebug
		// 
		this.chkDebug.Checked = true;
		this.chkDebug.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkDebug.Location = new System.Drawing.Point(8, 80);
		this.chkDebug.Name = "chkDebug";
		this.chkDebug.TabIndex = 3;
		this.chkDebug.Text = "Single Step";
		// 
		// txtResults
		// 
		this.txtResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
		this.txtResults.Location = new System.Drawing.Point(128, 8);
		this.txtResults.Multiline = true;
		this.txtResults.Name = "txtResults";
		this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtResults.Size = new System.Drawing.Size(496, 256);
		this.txtResults.TabIndex = 2;
		this.txtResults.Text = "";
		// 
		// btnstringWriter
		// 
		this.btnstringWriter.Location = new System.Drawing.Point(8, 32);
		this.btnstringWriter.Name = "btnstringWriter";
		this.btnstringWriter.Size = new System.Drawing.Size(104, 22);
		this.btnstringWriter.TabIndex = 1;
		this.btnstringWriter.Text = "stringWriter";
		this.btnstringWriter.Click += new System.EventHandler(this.btnstringWriter_Click);
		// 
		// btnstringBuilder
		// 
		this.btnstringBuilder.Location = new System.Drawing.Point(8, 8);
		this.btnstringBuilder.Name = "btnstringBuilder";
		this.btnstringBuilder.Size = new System.Drawing.Size(104, 22);
		this.btnstringBuilder.TabIndex = 0;
		this.btnstringBuilder.Text = "stringBuilder";
		this.btnstringBuilder.Click += new System.EventHandler(this.btnstringBuilder_Click);
		// 
		// frmMain
		// 
		this.AcceptButton = this.btnRecalc;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(656, 346);
		this.Controls.Add(this.tabstringDemo);
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.MaximizeBox = false;
		this.Menu = this.mnuMain;
		this.MinimumSize = new System.Drawing.Size(664, 380);
		this.Name = "frmMain";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Title Comes from Assembly Info";
		this.Load += new System.EventHandler(this.frmMain_Load);
		this.tabstringDemo.ResumeLayout(false);
		this.pagReturnstrings.ResumeLayout(false);
		this.pnlDemo.ResumeLayout(false);
		this.grpResults.ResumeLayout(false);
		this.grpParameters.ResumeLayout(false);
		this.grpSample.ResumeLayout(false);
		this.pagInfo.ResumeLayout(false);
		this.pagstatic.ResumeLayout(false);
		this.pagOther.ResumeLayout(false);
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

    private const string DEFAULT_TEXT  = "the quick brown fox jumps over the lazy dog";

	private const string QUOTE  ="\"";

    private string mstrSample  = DEFAULT_TEXT;

    private RadioButton rbtnstatic;

    private RadioButton rbtnstringreturn;

    private RadioButton rbtnstringInfo;

    private enum SelectedMethod
	{
        Compare,
        CompareOrdinal,
        Concat,
        EndsWith,
        Format,
        IndexOf,
        IndexOfAny,
        Insert,
        Join,
        LastIndexOf,
        LastIndexOfAny,
        PadLeft,
        PadRight,
        Remove,
        Replace,
        Split,
        StartsWith,
		Substring,
        voidstring,
        ToLower,
        ToUpper,
        Trim,
        TrimEnd,
        TrimStart
    };

    private SelectedMethod sm ;

    private void btnClear_Click(object sender, System.EventArgs e)
	{

        txtResults.Clear();

    }
														
    private void btnRecalc_Click(object sender, System.EventArgs e)
	{
        Recalc();

    }

    private void btnRefresh_Click(object sender, System.EventArgs e) 
	{
        RefreshText();

    }

    private void btnstringBuilder_Click(object sender, System.EventArgs e)
	{

        // Demonstrate the stringBuilder class.

        if (chkDebug.Checked)
		{

            // Trigger a breakpoint.

            System.Diagnostics.Debugger.Break();

        }

        // stringBuilder is provided by the System.Text 
        // namespace. See the using statement
        // at the top of this module.

        StringBuilder sb = new StringBuilder("The quick brown fox jumps over the lazy dog");

        sb.Insert(19, " happily");

        sb.Remove(10, 6);

        sb.Replace("jumps", "leaps");

        sb.AppendFormat(" {0} times in {1} minutes", 17, 2);

        // Suppose you want to insert a comma after the word "dog".
        // You need to locate the word, then insert a comma
        // after the word. To locate the string, you must search
        // using IndexOf, but stringBuilder doesn't supply this method.
        // Therefore, you must use the Tostring method to retrieve
        // the string, and work with that object instead.

        int intPos;
		
        intPos = sb.ToString().IndexOf("dog");
		
        if (intPos > 0)
		{

            // Insert the comma at the position
            // you found + the length of the text "dog".

            sb.Insert(intPos + "dog".Length, ", ");

        }

        // The same code using the string object directly
        // would look like this. Note the number of times 
        // you require the .NET framework to create new strings.
        // stringBuilder is significantly more efficient for
        // this sort of operation.

        string str = "The quick brown fox jumps over the lazy dog";

        str = str.Insert(19, " happily");

        str = str.Remove(10, 6);

        str = str.Replace("jumps", "leaps");

        str += string.Format("{0} times in {1} minutes", 17, 2);

        intPos = str.IndexOf("dog");

        if (intPos > 0)
		{

            str = str.Insert(intPos + "dog".Length, ", ");

        }

        txtResults.AppendText("stringBuilder output: " + sb.ToString() + Environment.NewLine);

        txtResults.AppendText("string output: " + str + Environment.NewLine);

    }

    private void btnstringWriter_Click(object sender, System.EventArgs e)
	{
        // Simple demonstration of the stringWriter class.
        // Write to the stringWriter if you were 
        // writing to a file--you're really writing to 
        // a buffer in memory, instead.
        // stringWriter is provided by the System.IO
        // namespace. See the using statement
        // at the top of this module.

        StringWriter sw = new StringWriter();

        if (chkDebug.Checked)
			{

            // Trigger a breakpoint.
            System.Diagnostics.Debugger.Break();

			}

        // You've been supplied with some data structure,
        // perhaps an array of strings, containing
        // address information. Create a formatted
        // address.

        string[] addressInfo  = {"John Smith", "123 Main Street", "Centerville", "WA", "98111"};

        // Add in the name and address lines:

        sw.WriteLine(addressInfo[0]);

        sw.WriteLine(addressInfo[1]);

        // You might use the string.Format method to create
        // this line of the address, but this demo 
        // shows how to use both the WriteLine and the
        // Write methods. Although this is somewhat contrived, you 
        // can at least see how to use the two methods:

        sw.Write(addressInfo[2]);

        sw.Write(", ");

        sw.Write(addressInfo[3]);

        sw.Write(" ");

        sw.WriteLine(addressInfo[4]);

        // Or:
        // sw.WriteLine(string.Format("{0}, {1} {2}", addressInfo(2), addressInfo(3), addressInfo(4)))
        // Using the string class, the code's uglier:

        string str;

        str = addressInfo[0] + Environment.NewLine;

        str += addressInfo[1] + Environment.NewLine;

        // Add on the city/region/postal code values:

        str += addressInfo[2] + ", ";

        str += addressInfo[3] + " " + addressInfo[4];

        str += Environment.NewLine;

        // Or: 

        // str += string.Format("{0}, {1} {2}{3}", addressInfo(2), addressInfo(3), addressInfo(4), Environment.newLine)
        // Display the output:

        txtResults.AppendText("stringWriter output: " + Environment.NewLine + sw.ToString());

        txtResults.AppendText("string output: " + Environment.NewLine + str);

    }

    private void frmMain_Load(object sender, System.EventArgs e) 
	{

        // Initialize values.
        // Selected page:

        tabstringDemo.SelectedIndex = 0;

        // Selected buttons on each page.
        // We've selected the first button on each page.

        rbtnstringInfo = optIndexOf;

        rbtnstringreturn = optInsert;

        rbtnstatic = optCompare;

        // Display text on the first page.

        DisplayText(optInsert);

    }

    private void tabstringDemo_SelectedIndexChanged(object sender, System.EventArgs e) 
	{

        TabPage pagSelected  = tabstringDemo.SelectedTab;

        // This code runs before the Load event of the form.
        // Skip all the code if the Load event hasn't yet 
        // run.

        if (rbtnstringreturn != null)

		{

            // Move the panel containing the 
            // info to the current page. Don't do 
            // this for the "other" page, however.

            if (pagSelected != pagOther)
			{

				pnlDemo.Parent = pagSelected;

			}

            // Based on the selected page, display output
            // from the item previously selected.

            if (pagSelected == pagReturnstrings)
			{

                grpSample.Visible = true;

                DisplayText(this.rbtnstringreturn);

            } 
			else if (pagSelected == pagInfo) 
			{

                grpSample.Visible = true;

                DisplayText(rbtnstringInfo);

            } 
			else if (pagSelected == pagstatic)
			{

                grpSample.Visible = false;
                DisplayText(rbtnstatic);

            }

        }

    }

    private string AddQuotes(string strValue )
	{

        // Add quotes around the supplied text.
        // Replace single quotes (") with double quotes ("")
        // to appease the C# parser.

        return QUOTE + strValue.Replace(QUOTE, QUOTE + QUOTE) + QUOTE;
	
    }

    private void DisplayText(RadioButton rbtn )
	{

        // Take action depending on which radio button you've selected.

        if (rbtn == optCompare)
		{

            rbtnstatic = rbtn;
            sm = SelectedMethod.Compare;
            HandleParameters("strA", "This is a test", "strB", "this is a test", "ignoreCase", "true");

        } 
		else if (rbtn == optCompareOrdinal)
		{

            rbtnstatic = rbtn;
            sm = SelectedMethod.CompareOrdinal;
            HandleParameters("strA", "This is a test", "strB", "this is a test");

        } 
		else if (rbtn == optConcat) 
		{

            rbtnstatic = rbtn;
            sm = SelectedMethod.Concat;
            HandleParameters("strA", "This is a test", "strB", " of how this works", "strC", " when you concatenate");

        } 
		else if (rbtn == optEndsWith)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.EndsWith;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("value", "dog");

        } 
		else if (rbtn == optFormat)
		{

            rbtnstatic = rbtn;
            sm = SelectedMethod.Format;
            HandleParameters("Format", "Your {0:N0} items total {1:C}.", "Param1", "12", "Param2", "12.35");

        } 
		else if (rbtn == optIndexOf)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.IndexOf;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("value", "o", "startIndex", "", "count", "");

        } 
		else if (rbtn == optIndexOfAny)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.IndexOfAny;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("anyOf()", "abc", "startIndex", "", "count", "");

        } 
		else if (rbtn == optInsert)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.Insert;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("startIndex", "19", "value", " happily");

        } 
		else if (rbtn == optJoin)
		{

            rbtnstatic = rbtn;
            sm = SelectedMethod.Join;
            HandleParameters("separator", "-", "values()", "item1,item2,item3,item4");

        } 
		else if (rbtn == optLastIndexOf)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.LastIndexOf;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("value", "o", "startIndex", "", "count", "");

        } 
		else if (rbtn == optLastIndexOfAny)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.LastIndexOfAny;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("anyOf()", "abc", "startIndex", "", "count", "");

        } 
		else if (rbtn == optPadLeft) 
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.PadLeft;
            mstrSample = "123.45";
            HandleParameters("totalWidth", "10", "paddingChar", "$");

        } 
		else if (rbtn == optPadRight)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.PadRight;
            mstrSample = "Pad";
            HandleParameters("totalWidth", "10", "paddingChar", "_");

        } 
		else if (rbtn == optRemove)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.Remove;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("startIndex", "10", "count", "6");

        } 
		else if (rbtn == optReplace)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.Replace;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("oldValue", "jumps", "newValue", "leaps");

        } 
		else if (rbtn == optSplit)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.Split;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("separator()", "abc", "count", "");

        } 
		else if (rbtn == optStartsWith)
		{

            rbtnstringInfo = rbtn;
            sm = SelectedMethod.StartsWith;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("value", "The qui");

        } 
		else if (rbtn == optSubstring)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.Substring;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("startIndex", "12", "length", "5");

        } else if ( rbtn == optToLower)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.ToLower;
            mstrSample = "This Sample has SOME mixed-CASE Text!";
            HandleParameters();

        } 
		else if (rbtn == optToUpper)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.ToUpper;
            mstrSample = "This Sample has SOME mixed-CASE Text!";
            HandleParameters();

        } 
		else if (rbtn == optTrim)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.Trim;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("trimChars()", "the dog");

        } 
		else if (rbtn == optTrimEnd)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.TrimEnd;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("trimChars()", "the dog");

        } 
		else if (rbtn == optTrimStart)
		{

            rbtnstringreturn = rbtn;
            sm = SelectedMethod.TrimStart;
            mstrSample = DEFAULT_TEXT;
            HandleParameters("trimChars()", "the dog");

        }

        // Display the sample text, and 
        // recalc to display the output.

        RefreshText();

        Recalc();

    }

    private void HandleCheckedChanged(object sender, System.EventArgs e) 
	{

        DisplayText((RadioButton) sender);

    }

    private void HandleParameters()
	{

        // Display 0 parameters.

        HandleParameterDisplay(0);

    }

    private void HandleParameters(string Param1 , string Param1Value)
	{

        // Display one parameter.

        lblPrm1.Text = Param1;

        txtPrm1.Text = Param1Value;

        HandleParameterDisplay(1);

    }

    private void HandleParameters(string Param1 , string Param1Value, string Param2 , string Param2Value )
	{

        // Display two parameters.

        lblPrm1.Text = Param1;

        txtPrm1.Text = Param1Value;

        lblPrm2.Text = Param2;

        txtPrm2.Text = Param2Value;

        HandleParameterDisplay(2);

    }

    private void HandleParameters(string Param1 , string Param1Value, string Param2, string Param2Value, string Param3, string Param3Value)
	{

        // Display three parameters.

        lblPrm1.Text = Param1;

        txtPrm1.Text = Param1Value;

        lblPrm2.Text = Param2;

        txtPrm2.Text = Param2Value;

        lblPrm3.Text = Param3;

        txtPrm3.Text = Param3Value;

        HandleParameterDisplay(3);

    }

    private void HandleParameterDisplay(int intCount )
	{

        // Show or hide the parameter labels and text boxes, 
        // depending on the number of parameters.

        lblPrm1.Visible = (intCount > 0);

        txtPrm1.Visible = (intCount > 0);

        lblPrm2.Visible = (intCount > 1);

        txtPrm2.Visible = (intCount > 1);

        lblPrm3.Visible = (intCount > 2);

        txtPrm3.Visible = (intCount > 2);

    }

    private void HandleResultsLabel(string MethodName )
	{

        SetResultsLabel("Sample", MethodName, null);

    }

    private void HandleResultsLabel(string MethodName , string Param1)
	{

        SetResultsLabel("Sample", MethodName, Param1);

    }

    private void HandleResultsLabel(string MethodName , string Param1, string Param2)
	{
        SetResultsLabel("Sample", MethodName, Param1 + ", " + Param2);

    }

    private void HandleResultsLabel(string MethodName , string Param1,string Param2, string Param3)
	{

        SetResultsLabel("Sample", MethodName, Param1 + ", " + Param2 + ", " + Param3);

    }

    private void HandlestaticResultsLabel(string MethodName )
	{

        SetResultsLabel("string", MethodName, null);

    }

    private void HandlestaticResultsLabel(string MethodName , string Param1)
	{

        SetResultsLabel("string", MethodName, Param1);

    }

    private void HandlestaticResultsLabel(string MethodName ,string Param1, string Param2)
	{
        SetResultsLabel("string", MethodName, Param1 + ", " + Param2);

    }

    private void HandlestaticResultsLabel(string MethodName ,string Param1,string Param2, string Param3)
	{

        SetResultsLabel("string", MethodName, Param1 + ", " + Param2 + ", " + Param3);

    }

    private void Recalc()
	{

        string strSample  = txtSample.Text;

        string strParam1 = txtPrm1.Text.TrimEnd();

        string strParam2 = txtPrm2.Text.TrimEnd();

        string strParam3  = txtPrm3.Text.TrimEnd();

        try 
		{

            switch(sm)
			{

                case SelectedMethod.Compare:

					if (strParam3 != string.Empty) 
					{

						HandlestaticResultsLabel("Compare", AddQuotes(strParam1), AddQuotes(strParam2), strParam3);

						lblResults.Text = string.Compare(strParam1, strParam2, Convert.ToBoolean(strParam3)).ToString();
					}
					else 
					{

						HandlestaticResultsLabel("Compare", AddQuotes(strParam1), AddQuotes(strParam2));

						lblResults.Text = string.Compare(strParam1, strParam2).ToString();

					}
						break;

				case SelectedMethod.CompareOrdinal:

                    HandlestaticResultsLabel("CompareOrdinal", AddQuotes(strParam1));

                    lblResults.Text = string.CompareOrdinal(strParam1, strParam2).ToString();
					break;
                case SelectedMethod.Concat:

                    HandlestaticResultsLabel("Concat", AddQuotes(strParam1), AddQuotes(strParam2), AddQuotes(strParam3));

                    lblResults.Text = string.Concat(strParam1, strParam2, strParam3);
					break;
                case SelectedMethod.EndsWith:

                    HandleResultsLabel("EndsWith", AddQuotes(strParam1));

                    // EndsWith returns a boolean -- convert to a string.

                    lblResults.Text = strSample.EndsWith(strParam1).ToString();
					break;
                case SelectedMethod.Format:

                    HandlestaticResultsLabel("Format", AddQuotes(strParam1), strParam2, strParam3);

                    lblResults.Text = string.Format(strParam1, Convert.ToInt32(strParam2), Convert.ToDouble(strParam3));
					break;
                case SelectedMethod.IndexOf:

                    if (strParam1 != string.Empty)
					{

						if (strParam2 != string.Empty && strParam3 != string.Empty)
						{
							HandleResultsLabel("IndexOf", AddQuotes(strParam1), strParam2, strParam3);
							lblResults.Text = strSample.IndexOf(strParam1, Convert.ToInt32(strParam2), Convert.ToInt32(strParam3)).ToString();

						} 
						else if (strParam2 != string.Empty)
						{

							HandleResultsLabel("IndexOf", AddQuotes(strParam1), strParam2);
							lblResults.Text = strSample.IndexOf(strParam1, Convert.ToInt32(strParam2)).ToString();
						}
						else 
						{

							HandleResultsLabel("IndexOf", AddQuotes(strParam1));
							lblResults.Text = strSample.IndexOf(strParam1).ToString();

						}

                    }
				break;
                case SelectedMethod.IndexOfAny:

                    if (strParam1 != string.Empty)
					{

						if (strParam2 != string.Empty && strParam3 != string.Empty)
						{
							HandleResultsLabel("IndexOfAny", AddQuotes(strParam1), strParam2, strParam3);
							lblResults.Text = strSample.IndexOfAny(strParam1.ToCharArray(), Convert.ToInt32(strParam2), Convert.ToInt32(strParam3)).ToString();

						} 
						else if (strParam2 != string.Empty)
						{

							HandleResultsLabel("IndexOfAny", AddQuotes(strParam1), strParam2);
							lblResults.Text = strSample.IndexOfAny(strParam1.ToCharArray(), Convert.ToInt32(strParam2)).ToString();
						}
						else 
						{
							HandleResultsLabel("IndexOfAny", AddQuotes(strParam1));
							lblResults.Text = strSample.IndexOfAny(strParam1.ToCharArray()).ToString();

						}

                    }
					break;

                case SelectedMethod.Insert:

                    HandleResultsLabel("Insert", strParam1, AddQuotes(strParam2));
                    lblResults.Text = strSample.Insert(Convert.ToInt32(strParam1), strParam2);
					break;
                case SelectedMethod.Join:

                    // This code expects to receive a comma-delimited
                    // list of values. We'll strip out spaces, well.
                    // HandlestaticResultsLabel("Join", AddQuotes(strParam1), )

                    string[] astrValues;
                    astrValues = strParam2.Split(", ".ToCharArray());
                    lblResults.Text = string.Join(strParam1, astrValues);
					break;
                case SelectedMethod.LastIndexOf:

                    if (strParam1 != string.Empty)
					{

						if (strParam2 != string.Empty && strParam3 != string.Empty)
						{
							HandleResultsLabel("LastIndexOf", AddQuotes(strParam1), strParam2, strParam3);
							lblResults.Text = strSample.LastIndexOf(strParam1, Convert.ToInt32(strParam2), Convert.ToInt32(strParam3)).ToString();

						} 
						else if (strParam2 != string.Empty)
						{
							HandleResultsLabel("LastIndexOf", AddQuotes(strParam1), strParam2);
							lblResults.Text = strSample.LastIndexOf(strParam1, Convert.ToInt32(strParam2)).ToString();
						}
						else 
						{
							HandleResultsLabel("LastIndexOf", AddQuotes(strParam1));
							lblResults.Text = strSample.LastIndexOf(strParam1).ToString();
						}

                    }
					break;
                case SelectedMethod.LastIndexOfAny:

                    if (strParam1 != string.Empty)
					{

						if (strParam2 != string.Empty && strParam3 != string.Empty)
						{
							HandleResultsLabel("LastIndexOfAny", AddQuotes(strParam1), strParam2, strParam3);
							lblResults.Text = strSample.LastIndexOfAny(strParam1.ToCharArray(), Convert.ToInt32(strParam2), Convert.ToInt32(strParam3)).ToString();

						} 
						else if (strParam2 != string.Empty)
						{

							HandleResultsLabel("LastIndexOfAny", AddQuotes(strParam1), strParam2);
							lblResults.Text = strSample.LastIndexOfAny(strParam1.ToCharArray(), Convert.ToInt32(strParam2)).ToString();
						}
						else 
						{
							HandleResultsLabel("LastIndexOfAny", AddQuotes(strParam1));
							lblResults.Text = strSample.LastIndexOfAny(strParam1.ToCharArray()).ToString();

						}

                    }
					break;
                case SelectedMethod.PadLeft:

					if (strParam2 == string.Empty)
					{

						// if you leave off the second parameter, string.PadLeft
						// uses a space character.

						HandleResultsLabel("PadLeft", strParam1);
						lblResults.Text = strSample.PadLeft(Convert.ToInt32(strParam1));
					}
					else 
					{
						HandleResultsLabel("PadLeft", strParam1, AddQuotes(strParam2));
						lblResults.Text = strSample.PadLeft(Convert.ToInt32(strParam1), Convert.ToChar(strParam2));
					}
					break;

                case SelectedMethod.PadRight:

                    // if you leave off the second parameter, string.PadRight
                    // uses a space character.

					if (strParam2 == string.Empty)
					{

						HandleResultsLabel("PadRight", strParam1);
						lblResults.Text = strSample.PadRight(Convert.ToInt32(strParam1));
					}
					else 
					{
						HandleResultsLabel("PadRight", strParam1, AddQuotes(strParam2));
						lblResults.Text = strSample.PadRight(Convert.ToInt32(strParam1), Convert.ToChar(strParam2));

					}
					break;

                case SelectedMethod.Remove:

                    HandleResultsLabel("Remove", strParam1, strParam2);
                    lblResults.Text = strSample.Remove(Convert.ToInt32(strParam1), Convert.ToInt32(strParam2));
					break;
		        case SelectedMethod.Replace:

                    HandleResultsLabel("Replace", AddQuotes(strParam1), AddQuotes(strParam2));
                    lblResults.Text = strSample.Replace(strParam1, strParam2);
					break;
                case SelectedMethod.Split:

                    // Display an array of strings, formatted 
                    // like this:
                    // {"item1", "item2", "item3"}
                    // This example uses the Join method, well
                    // the Split method.  

					if (strParam2 == string.Empty)
					{

						HandleResultsLabel("Split", AddQuotes(strParam1));
						lblResults.Text = "{" + QUOTE + string.Join(QUOTE + "," + QUOTE, strSample.Split(strParam1.ToCharArray())) + QUOTE + "}";
					}
					else 
					{
						HandleResultsLabel("Split", AddQuotes(strParam1), strParam2);
						lblResults.Text = "{" + QUOTE + string.Join(QUOTE + "," + QUOTE, strSample.Split(strParam1.ToCharArray(), Convert.ToInt32(strParam2))) + QUOTE + "}";
					}
					break;

                case SelectedMethod.StartsWith:

                    HandleResultsLabel("StartsWith", AddQuotes(strParam1));
                    lblResults.Text = strSample.StartsWith(strParam1).ToString();
					break;

                case SelectedMethod.Substring:

                    // if you leave off the second parameter, 
                    // string.Substring returns all the remaining characters.

					if (strParam2 == string.Empty)
					{
						HandleResultsLabel("Substring", strParam1);
						lblResults.Text = strSample.Substring(Convert.ToInt32(strParam1));
					}
					else 
					{
						HandleResultsLabel("Substring", strParam1, strParam2);
						lblResults.Text = strSample.Substring(Convert.ToInt32(strParam1), Convert.ToInt32(strParam2));
					}
					break;
                case SelectedMethod.ToLower:

                    HandleResultsLabel("ToLower");
                    lblResults.Text = strSample.ToLower();
					break;

                case SelectedMethod.ToUpper:

                    HandleResultsLabel("ToUpper");
                    lblResults.Text = strSample.ToUpper();
					break;

                case SelectedMethod.Trim:

                    // if you don't pass a parameter, Trim
                    // assumes you want to trim spaces only.

					if (strParam1 == string.Empty)
					{
						HandleResultsLabel("Trim");
						lblResults.Text = strSample.Trim();
					}
					else 
					{
						HandleResultsLabel("Trim", AddQuotes(strParam1));
						lblResults.Text = strSample.Trim(strParam1.ToCharArray());
					}
					break;
                case SelectedMethod.TrimEnd:

                    // if you don't pass a parameter, TrimEnd
                    // assumes you want to trim spaces only.

					if (strParam1 == string.Empty)
					{
						HandleResultsLabel("TrimEnd");
						lblResults.Text = strSample.TrimEnd();
					}
					else 
					{
						HandleResultsLabel("TrimEnd", AddQuotes(strParam1));
						lblResults.Text = strSample.TrimEnd(strParam1.ToCharArray());
					}
					break;

                case SelectedMethod.TrimStart:

                    // if you don't pass a parameter, Trim
                    // assumes you want to trim spaces only.

					if (strParam1 == string.Empty)
					{

						HandleResultsLabel("TrimStart");
						lblResults.Text = strSample.TrimStart();
					}
					else 
					{
						HandleResultsLabel("TrimStart", AddQuotes(strParam1));
						lblResults.Text = strSample.TrimStart(strParam1.ToCharArray());
					}
					break;

            }

       } 
		catch(Exception exp) 
		{

            // if any error occurs, display the information.
            lblResultsLabel.Text = "<<ERROR>>";
            lblResults.Text = exp.Message;
        }

    }

    private void RefreshText()
	{

        txtSample.Text = mstrSample;

    }

    private void SetResultsLabel(string Source , string Method , string Parameters)
	{

        lblResultsLabel.Text = string.Format("Results of {0}.{1}({2})", Source, Method, Parameters);

    }

	private void optSplit_CheckedChanged(object sender, System.EventArgs e)
	{
	
	}

}

