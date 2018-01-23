//Copyright (C) 2002 Microsoft Corporation

//All rights reserved.

//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 

//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 

//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;
using System.Windows.Forms;

public class frmMain:System.Windows.Forms.Form 
{

#region " Windows Form Designer generated code "

	public frmMain() 
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

	private System.Windows.Forms.MainMenu mnuMain;

	private System.Windows.Forms.MenuItem mnuFile;

	private System.Windows.Forms.MenuItem mnuExit;

	private System.Windows.Forms.MenuItem mnuHelp;

	private System.Windows.Forms.MenuItem mnuAbout;

	private System.Windows.Forms.TabControl TabControl1;

	private System.Windows.Forms.TabPage TabPage1;

	private System.Windows.Forms.Label lblMaxValue;

	private System.Windows.Forms.Label lblMinValue;

	private System.Windows.Forms.Label lblUtcNow;

	private System.Windows.Forms.Label lblToday;

	private System.Windows.Forms.Label lblNow;

	private System.Windows.Forms.Label lblFromOADate;

	private System.Windows.Forms.Label lblIsLeapYear;

	private System.Windows.Forms.Label lblDaysInMonth;

	private System.Windows.Forms.TextBox txtMonth;

	private System.Windows.Forms.TextBox txtFromOADate;

	private System.Windows.Forms.TextBox txtIsLeapYear;

	private System.Windows.Forms.TextBox txtYear;

	private System.Windows.Forms.Label Label8;

	private System.Windows.Forms.Label Label7;

	private System.Windows.Forms.Label Label6;

	private System.Windows.Forms.Label Label5;

	private System.Windows.Forms.Label Label4;

	private System.Windows.Forms.Label Label3;

	private System.Windows.Forms.Label Label2;

	private System.Windows.Forms.Label Label1;

	private System.Windows.Forms.Button btnRefreshstatic;

	private System.Windows.Forms.TabPage TabPage4;

	private System.Windows.Forms.TextBox txtYears;

	private System.Windows.Forms.TextBox txtTicks;

	private System.Windows.Forms.TextBox txtSeconds;

	private System.Windows.Forms.TextBox txtMonths;

	private System.Windows.Forms.TextBox txtMinutes;

	private System.Windows.Forms.TextBox txtMilliseconds;

	private System.Windows.Forms.TextBox txtHours;

	private System.Windows.Forms.TextBox txtDays;

	private System.Windows.Forms.Button btnRefreshCalculation;

	private System.Windows.Forms.Label lblAddSeconds;

	private System.Windows.Forms.Label lblAddMonths;

	private System.Windows.Forms.Label lblAddMinutes;

	private System.Windows.Forms.Label lblAddMilliseconds;

	private System.Windows.Forms.Label lblAddHours;

	private System.Windows.Forms.Label lblAddDays;

	private System.Windows.Forms.Label lblAddTicks;

	private System.Windows.Forms.Label lblAddYears;

	private System.Windows.Forms.Label lblNow3;

	private System.Windows.Forms.Label Label49;

	private System.Windows.Forms.Label Label50;

	private System.Windows.Forms.Label Label51;

	private System.Windows.Forms.Label Label52;

	private System.Windows.Forms.Label Label53;

	private System.Windows.Forms.Label Label54;

	private System.Windows.Forms.Label Label55;

	private System.Windows.Forms.Label Label56;

	private System.Windows.Forms.Label Label57;

	private System.Windows.Forms.TabPage TabPage2;

	private System.Windows.Forms.Label lblMillisecond;

	private System.Windows.Forms.Label lblHour;

	private System.Windows.Forms.Label lblDayOfYear;

	private System.Windows.Forms.Label lblDayOfWeek;

	private System.Windows.Forms.Label lblDate;

	private System.Windows.Forms.Label lblDay;

	private System.Windows.Forms.Label lblMinute;

	private System.Windows.Forms.Label lblMonth;

	private System.Windows.Forms.Label lblSecond;

	private System.Windows.Forms.Label lblTicks;

	private System.Windows.Forms.Label lblYear;

	private System.Windows.Forms.Label lblTimeOfDay;

	private System.Windows.Forms.Label lblNow1;

	private System.Windows.Forms.Label Label21;

	private System.Windows.Forms.Label Label20;

	private System.Windows.Forms.Label Label19;

	private System.Windows.Forms.Label Label18;

	private System.Windows.Forms.Label Label17;

	private System.Windows.Forms.Label Label16;

	private System.Windows.Forms.Label Label15;

	private System.Windows.Forms.Label Label14;

	private System.Windows.Forms.Label Label13;

	private System.Windows.Forms.Label Label12;

	private System.Windows.Forms.Label Label11;

	private System.Windows.Forms.Label Label10;

	private System.Windows.Forms.Button btnRefreshProperties;

	private System.Windows.Forms.Label Label9;

	private System.Windows.Forms.TabPage TabPage3;

	private System.Windows.Forms.Label lblToShortDatestring;

	private System.Windows.Forms.Label lblToOADate;

	private System.Windows.Forms.Label lblToLongTimestring;

	private System.Windows.Forms.Label lblToLongDatestring;

	private System.Windows.Forms.Label lblToLocalTime;

	private System.Windows.Forms.Label lblToFileTime;

	private System.Windows.Forms.Label lblToShortTimestring;

	private System.Windows.Forms.Label lblTostring;

	private System.Windows.Forms.Label lblToUniversalTime;

	private System.Windows.Forms.Label Label38;

	private System.Windows.Forms.Label Label39;

	private System.Windows.Forms.Label Label40;

	private System.Windows.Forms.Label Label41;

	private System.Windows.Forms.Label Label42;

	private System.Windows.Forms.Label Label43;

	private System.Windows.Forms.Label Label44;

	private System.Windows.Forms.Label Label45;

	private System.Windows.Forms.Label Label46;

	private System.Windows.Forms.Label lblNow2;

	private System.Windows.Forms.Button btnRefreshConversion;

	private System.Windows.Forms.Label Label23;

	private System.Windows.Forms.TabPage TabPage6;

	private System.Windows.Forms.Label lblTotalSeconds;

	private System.Windows.Forms.Label Label61;

	private System.Windows.Forms.Label lblTotalMinutes;

	private System.Windows.Forms.Label Label62;

	private System.Windows.Forms.Label lblTotalMilliseconds;

	private System.Windows.Forms.Label Label63;

	private System.Windows.Forms.Label lblTotalHours;

	private System.Windows.Forms.Label Label64;

	private System.Windows.Forms.Label lblTotalDays;

	private System.Windows.Forms.Label Label65;

	private System.Windows.Forms.Label lblTimeSpanTicks;

	private System.Windows.Forms.Label Label67;

	private System.Windows.Forms.Label lblSeconds;

	private System.Windows.Forms.Label Label68;

	private System.Windows.Forms.Label lblMinutes;

	private System.Windows.Forms.Label Label69;

	private System.Windows.Forms.Label lblMilliseconds;

	private System.Windows.Forms.Label Label70;

	private System.Windows.Forms.Label lblHours;

	private System.Windows.Forms.Label Label71;

	private System.Windows.Forms.Label lblDays;

	private System.Windows.Forms.Label Label72;

	private System.Windows.Forms.TabControl tabTimeSpan;

	private System.Windows.Forms.TabPage TabPage9;

	private System.Windows.Forms.Button btnRefreshTSProperties;

	private System.Windows.Forms.Label Label28;

	private System.Windows.Forms.Label Label31;

	private System.Windows.Forms.TextBox txtEnd;

	private System.Windows.Forms.TextBox txtStart;

	private System.Windows.Forms.TabPage TabPage10;

	private System.Windows.Forms.Button btnCalcParse;

	private System.Windows.Forms.TextBox txtParse;

	private System.Windows.Forms.Label Label33;

	private System.Windows.Forms.TabPage TabPage7;

	private System.Windows.Forms.Button btnRefreshTSMethods;

	private System.Windows.Forms.TextBox txtFromHours;

	private System.Windows.Forms.Label lblFromHours;

	private System.Windows.Forms.Label Label34;

	private System.Windows.Forms.TextBox txtFromSeconds;

	private System.Windows.Forms.Label lblFromSeconds;

	private System.Windows.Forms.Label Label35;

	private System.Windows.Forms.TextBox txtFromTicks;

	private System.Windows.Forms.Label lblFromTicks;

	private System.Windows.Forms.Label Label36;

	private System.Windows.Forms.TextBox txtFromMilliseconds;

	private System.Windows.Forms.Label lblFromMilliseconds;

	private System.Windows.Forms.Label Label37;

	private System.Windows.Forms.TextBox txtFromMinutes;

	private System.Windows.Forms.Label lblFromMinutes;

	private System.Windows.Forms.Label Label47;

	private System.Windows.Forms.TextBox txtFromDays;

	private System.Windows.Forms.Label lblFromDays;

	private System.Windows.Forms.Label Label48;

	private System.Windows.Forms.TabPage TabPage5;

	private System.Windows.Forms.Label lblZero;

	private System.Windows.Forms.Label Label22;

	private System.Windows.Forms.Label lblTicksPerSecond;

	private System.Windows.Forms.Label Label25;

	private System.Windows.Forms.Label lblTicksPerMinute;

	private System.Windows.Forms.Label Label27;

	private System.Windows.Forms.Label lblTicksPerMillisecond;

	private System.Windows.Forms.Label Label29;

	private System.Windows.Forms.Label lblTicksPerHour;

	private System.Windows.Forms.Label Label24;

	private System.Windows.Forms.Label lblTicksPerDay;

	private System.Windows.Forms.Label Label26;

	private System.Windows.Forms.Label lblMinValueTS;

	private System.Windows.Forms.Label Label30;

	private System.Windows.Forms.Label lblMaxValueTS;

	private System.Windows.Forms.Label Label32;

	private void InitializeComponent() {
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
		this.mnuMain = new System.Windows.Forms.MainMenu();
		this.mnuFile = new System.Windows.Forms.MenuItem();
		this.mnuExit = new System.Windows.Forms.MenuItem();
		this.mnuHelp = new System.Windows.Forms.MenuItem();
		this.mnuAbout = new System.Windows.Forms.MenuItem();
		this.TabControl1 = new System.Windows.Forms.TabControl();
		this.TabPage1 = new System.Windows.Forms.TabPage();
		this.lblMaxValue = new System.Windows.Forms.Label();
		this.lblMinValue = new System.Windows.Forms.Label();
		this.lblUtcNow = new System.Windows.Forms.Label();
		this.lblToday = new System.Windows.Forms.Label();
		this.lblNow = new System.Windows.Forms.Label();
		this.lblFromOADate = new System.Windows.Forms.Label();
		this.lblIsLeapYear = new System.Windows.Forms.Label();
		this.lblDaysInMonth = new System.Windows.Forms.Label();
		this.txtMonth = new System.Windows.Forms.TextBox();
		this.txtFromOADate = new System.Windows.Forms.TextBox();
		this.txtIsLeapYear = new System.Windows.Forms.TextBox();
		this.txtYear = new System.Windows.Forms.TextBox();
		this.Label8 = new System.Windows.Forms.Label();
		this.Label7 = new System.Windows.Forms.Label();
		this.Label6 = new System.Windows.Forms.Label();
		this.Label5 = new System.Windows.Forms.Label();
		this.Label4 = new System.Windows.Forms.Label();
		this.Label3 = new System.Windows.Forms.Label();
		this.Label2 = new System.Windows.Forms.Label();
		this.Label1 = new System.Windows.Forms.Label();
		this.btnRefreshstatic = new System.Windows.Forms.Button();
		this.TabPage4 = new System.Windows.Forms.TabPage();
		this.txtYears = new System.Windows.Forms.TextBox();
		this.txtTicks = new System.Windows.Forms.TextBox();
		this.txtSeconds = new System.Windows.Forms.TextBox();
		this.txtMonths = new System.Windows.Forms.TextBox();
		this.txtMinutes = new System.Windows.Forms.TextBox();
		this.txtMilliseconds = new System.Windows.Forms.TextBox();
		this.txtHours = new System.Windows.Forms.TextBox();
		this.txtDays = new System.Windows.Forms.TextBox();
		this.btnRefreshCalculation = new System.Windows.Forms.Button();
		this.lblAddSeconds = new System.Windows.Forms.Label();
		this.lblAddMonths = new System.Windows.Forms.Label();
		this.lblAddMinutes = new System.Windows.Forms.Label();
		this.lblAddMilliseconds = new System.Windows.Forms.Label();
		this.lblAddHours = new System.Windows.Forms.Label();
		this.lblAddDays = new System.Windows.Forms.Label();
		this.lblAddTicks = new System.Windows.Forms.Label();
		this.lblAddYears = new System.Windows.Forms.Label();
		this.lblNow3 = new System.Windows.Forms.Label();
		this.Label49 = new System.Windows.Forms.Label();
		this.Label50 = new System.Windows.Forms.Label();
		this.Label51 = new System.Windows.Forms.Label();
		this.Label52 = new System.Windows.Forms.Label();
		this.Label53 = new System.Windows.Forms.Label();
		this.Label54 = new System.Windows.Forms.Label();
		this.Label55 = new System.Windows.Forms.Label();
		this.Label56 = new System.Windows.Forms.Label();
		this.Label57 = new System.Windows.Forms.Label();
		this.TabPage2 = new System.Windows.Forms.TabPage();
		this.lblMillisecond = new System.Windows.Forms.Label();
		this.lblHour = new System.Windows.Forms.Label();
		this.lblDayOfYear = new System.Windows.Forms.Label();
		this.lblDayOfWeek = new System.Windows.Forms.Label();
		this.lblDate = new System.Windows.Forms.Label();
		this.lblDay = new System.Windows.Forms.Label();
		this.lblMinute = new System.Windows.Forms.Label();
		this.lblMonth = new System.Windows.Forms.Label();
		this.lblSecond = new System.Windows.Forms.Label();
		this.lblTicks = new System.Windows.Forms.Label();
		this.lblYear = new System.Windows.Forms.Label();
		this.lblTimeOfDay = new System.Windows.Forms.Label();
		this.lblNow1 = new System.Windows.Forms.Label();
		this.Label21 = new System.Windows.Forms.Label();
		this.Label20 = new System.Windows.Forms.Label();
		this.Label19 = new System.Windows.Forms.Label();
		this.Label18 = new System.Windows.Forms.Label();
		this.Label17 = new System.Windows.Forms.Label();
		this.Label16 = new System.Windows.Forms.Label();
		this.Label15 = new System.Windows.Forms.Label();
		this.Label14 = new System.Windows.Forms.Label();
		this.Label13 = new System.Windows.Forms.Label();
		this.Label12 = new System.Windows.Forms.Label();
		this.Label11 = new System.Windows.Forms.Label();
		this.Label10 = new System.Windows.Forms.Label();
		this.btnRefreshProperties = new System.Windows.Forms.Button();
		this.Label9 = new System.Windows.Forms.Label();
		this.TabPage3 = new System.Windows.Forms.TabPage();
		this.lblToShortDatestring = new System.Windows.Forms.Label();
		this.lblToOADate = new System.Windows.Forms.Label();
		this.lblToLongTimestring = new System.Windows.Forms.Label();
		this.lblToLongDatestring = new System.Windows.Forms.Label();
		this.lblToLocalTime = new System.Windows.Forms.Label();
		this.lblToFileTime = new System.Windows.Forms.Label();
		this.lblToShortTimestring = new System.Windows.Forms.Label();
		this.lblTostring = new System.Windows.Forms.Label();
		this.lblToUniversalTime = new System.Windows.Forms.Label();
		this.Label38 = new System.Windows.Forms.Label();
		this.Label39 = new System.Windows.Forms.Label();
		this.Label40 = new System.Windows.Forms.Label();
		this.Label41 = new System.Windows.Forms.Label();
		this.Label42 = new System.Windows.Forms.Label();
		this.Label43 = new System.Windows.Forms.Label();
		this.Label44 = new System.Windows.Forms.Label();
		this.Label45 = new System.Windows.Forms.Label();
		this.Label46 = new System.Windows.Forms.Label();
		this.lblNow2 = new System.Windows.Forms.Label();
		this.btnRefreshConversion = new System.Windows.Forms.Button();
		this.Label23 = new System.Windows.Forms.Label();
		this.TabPage6 = new System.Windows.Forms.TabPage();
		this.lblTotalSeconds = new System.Windows.Forms.Label();
		this.Label61 = new System.Windows.Forms.Label();
		this.lblTotalMinutes = new System.Windows.Forms.Label();
		this.Label62 = new System.Windows.Forms.Label();
		this.lblTotalMilliseconds = new System.Windows.Forms.Label();
		this.Label63 = new System.Windows.Forms.Label();
		this.lblTotalHours = new System.Windows.Forms.Label();
		this.Label64 = new System.Windows.Forms.Label();
		this.lblTotalDays = new System.Windows.Forms.Label();
		this.Label65 = new System.Windows.Forms.Label();
		this.lblTimeSpanTicks = new System.Windows.Forms.Label();
		this.Label67 = new System.Windows.Forms.Label();
		this.lblSeconds = new System.Windows.Forms.Label();
		this.Label68 = new System.Windows.Forms.Label();
		this.lblMinutes = new System.Windows.Forms.Label();
		this.Label69 = new System.Windows.Forms.Label();
		this.lblMilliseconds = new System.Windows.Forms.Label();
		this.Label70 = new System.Windows.Forms.Label();
		this.lblHours = new System.Windows.Forms.Label();
		this.Label71 = new System.Windows.Forms.Label();
		this.lblDays = new System.Windows.Forms.Label();
		this.Label72 = new System.Windows.Forms.Label();
		this.tabTimeSpan = new System.Windows.Forms.TabControl();
		this.TabPage9 = new System.Windows.Forms.TabPage();
		this.btnRefreshTSProperties = new System.Windows.Forms.Button();
		this.Label28 = new System.Windows.Forms.Label();
		this.Label31 = new System.Windows.Forms.Label();
		this.txtEnd = new System.Windows.Forms.TextBox();
		this.txtStart = new System.Windows.Forms.TextBox();
		this.TabPage10 = new System.Windows.Forms.TabPage();
		this.btnCalcParse = new System.Windows.Forms.Button();
		this.txtParse = new System.Windows.Forms.TextBox();
		this.Label33 = new System.Windows.Forms.Label();
		this.TabPage7 = new System.Windows.Forms.TabPage();
		this.btnRefreshTSMethods = new System.Windows.Forms.Button();
		this.txtFromHours = new System.Windows.Forms.TextBox();
		this.lblFromHours = new System.Windows.Forms.Label();
		this.Label34 = new System.Windows.Forms.Label();
		this.txtFromSeconds = new System.Windows.Forms.TextBox();
		this.lblFromSeconds = new System.Windows.Forms.Label();
		this.Label35 = new System.Windows.Forms.Label();
		this.txtFromTicks = new System.Windows.Forms.TextBox();
		this.lblFromTicks = new System.Windows.Forms.Label();
		this.Label36 = new System.Windows.Forms.Label();
		this.txtFromMilliseconds = new System.Windows.Forms.TextBox();
		this.lblFromMilliseconds = new System.Windows.Forms.Label();
		this.Label37 = new System.Windows.Forms.Label();
		this.txtFromMinutes = new System.Windows.Forms.TextBox();
		this.lblFromMinutes = new System.Windows.Forms.Label();
		this.Label47 = new System.Windows.Forms.Label();
		this.txtFromDays = new System.Windows.Forms.TextBox();
		this.lblFromDays = new System.Windows.Forms.Label();
		this.Label48 = new System.Windows.Forms.Label();
		this.TabPage5 = new System.Windows.Forms.TabPage();
		this.lblZero = new System.Windows.Forms.Label();
		this.Label22 = new System.Windows.Forms.Label();
		this.lblTicksPerSecond = new System.Windows.Forms.Label();
		this.Label25 = new System.Windows.Forms.Label();
		this.lblTicksPerMinute = new System.Windows.Forms.Label();
		this.Label27 = new System.Windows.Forms.Label();
		this.lblTicksPerMillisecond = new System.Windows.Forms.Label();
		this.Label29 = new System.Windows.Forms.Label();
		this.lblTicksPerHour = new System.Windows.Forms.Label();
		this.Label24 = new System.Windows.Forms.Label();
		this.lblTicksPerDay = new System.Windows.Forms.Label();
		this.Label26 = new System.Windows.Forms.Label();
		this.lblMinValueTS = new System.Windows.Forms.Label();
		this.Label30 = new System.Windows.Forms.Label();
		this.lblMaxValueTS = new System.Windows.Forms.Label();
		this.Label32 = new System.Windows.Forms.Label();
		this.TabControl1.SuspendLayout();
		this.TabPage1.SuspendLayout();
		this.TabPage4.SuspendLayout();
		this.TabPage2.SuspendLayout();
		this.TabPage3.SuspendLayout();
		this.TabPage6.SuspendLayout();
		this.tabTimeSpan.SuspendLayout();
		this.TabPage9.SuspendLayout();
		this.TabPage10.SuspendLayout();
		this.TabPage7.SuspendLayout();
		this.TabPage5.SuspendLayout();
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
		// TabControl1
		// 
		this.TabControl1.Controls.Add(this.TabPage1);
		this.TabControl1.Controls.Add(this.TabPage4);
		this.TabControl1.Controls.Add(this.TabPage2);
		this.TabControl1.Controls.Add(this.TabPage3);
		this.TabControl1.Controls.Add(this.TabPage6);
		this.TabControl1.Controls.Add(this.TabPage7);
		this.TabControl1.Controls.Add(this.TabPage5);
		this.TabControl1.ItemSize = new System.Drawing.Size(92, 18);
		this.TabControl1.Location = new System.Drawing.Point(8, 8);
		this.TabControl1.Multiline = true;
		this.TabControl1.Name = "TabControl1";
		this.TabControl1.SelectedIndex = 0;
		this.TabControl1.Size = new System.Drawing.Size(600, 304);
		this.TabControl1.TabIndex = 1;
		// 
		// TabPage1
		// 
		this.TabPage1.Controls.Add(this.lblMaxValue);
		this.TabPage1.Controls.Add(this.lblMinValue);
		this.TabPage1.Controls.Add(this.lblUtcNow);
		this.TabPage1.Controls.Add(this.lblToday);
		this.TabPage1.Controls.Add(this.lblNow);
		this.TabPage1.Controls.Add(this.lblFromOADate);
		this.TabPage1.Controls.Add(this.lblIsLeapYear);
		this.TabPage1.Controls.Add(this.lblDaysInMonth);
		this.TabPage1.Controls.Add(this.txtMonth);
		this.TabPage1.Controls.Add(this.txtFromOADate);
		this.TabPage1.Controls.Add(this.txtIsLeapYear);
		this.TabPage1.Controls.Add(this.txtYear);
		this.TabPage1.Controls.Add(this.Label8);
		this.TabPage1.Controls.Add(this.Label7);
		this.TabPage1.Controls.Add(this.Label6);
		this.TabPage1.Controls.Add(this.Label5);
		this.TabPage1.Controls.Add(this.Label4);
		this.TabPage1.Controls.Add(this.Label3);
		this.TabPage1.Controls.Add(this.Label2);
		this.TabPage1.Controls.Add(this.Label1);
		this.TabPage1.Controls.Add(this.btnRefreshstatic);
		this.TabPage1.Location = new System.Drawing.Point(4, 40);
		this.TabPage1.Name = "TabPage1";
		this.TabPage1.Size = new System.Drawing.Size(592, 260);
		this.TabPage1.TabIndex = 0;
		this.TabPage1.Text = "DateTime static Members";
		// 
		// lblMaxValue
		// 
		this.lblMaxValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMaxValue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMaxValue.Location = new System.Drawing.Point(136, 128);
		this.lblMaxValue.Name = "lblMaxValue";
		this.lblMaxValue.Size = new System.Drawing.Size(200, 23);
		this.lblMaxValue.TabIndex = 20;
		this.lblMaxValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblMinValue
		// 
		this.lblMinValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMinValue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMinValue.Location = new System.Drawing.Point(136, 104);
		this.lblMinValue.Name = "lblMinValue";
		this.lblMinValue.Size = new System.Drawing.Size(200, 23);
		this.lblMinValue.TabIndex = 19;
		this.lblMinValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblUtcNow
		// 
		this.lblUtcNow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblUtcNow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblUtcNow.Location = new System.Drawing.Point(136, 80);
		this.lblUtcNow.Name = "lblUtcNow";
		this.lblUtcNow.Size = new System.Drawing.Size(200, 23);
		this.lblUtcNow.TabIndex = 18;
		this.lblUtcNow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToday
		// 
		this.lblToday.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToday.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToday.Location = new System.Drawing.Point(136, 56);
		this.lblToday.Name = "lblToday";
		this.lblToday.Size = new System.Drawing.Size(200, 23);
		this.lblToday.TabIndex = 17;
		this.lblToday.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblNow
		// 
		this.lblNow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblNow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblNow.Location = new System.Drawing.Point(136, 32);
		this.lblNow.Name = "lblNow";
		this.lblNow.Size = new System.Drawing.Size(200, 23);
		this.lblNow.TabIndex = 16;
		this.lblNow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblFromOADate
		// 
		this.lblFromOADate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromOADate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromOADate.Location = new System.Drawing.Point(264, 224);
		this.lblFromOADate.Name = "lblFromOADate";
		this.lblFromOADate.Size = new System.Drawing.Size(200, 23);
		this.lblFromOADate.TabIndex = 15;
		this.lblFromOADate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblIsLeapYear
		// 
		this.lblIsLeapYear.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblIsLeapYear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblIsLeapYear.Location = new System.Drawing.Point(264, 200);
		this.lblIsLeapYear.Name = "lblIsLeapYear";
		this.lblIsLeapYear.Size = new System.Drawing.Size(200, 23);
		this.lblIsLeapYear.TabIndex = 14;
		this.lblIsLeapYear.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblDaysInMonth
		// 
		this.lblDaysInMonth.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblDaysInMonth.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblDaysInMonth.Location = new System.Drawing.Point(264, 176);
		this.lblDaysInMonth.Name = "lblDaysInMonth";
		this.lblDaysInMonth.Size = new System.Drawing.Size(200, 23);
		this.lblDaysInMonth.TabIndex = 13;
		this.lblDaysInMonth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// txtMonth
		// 
		this.txtMonth.Location = new System.Drawing.Point(232, 176);
		this.txtMonth.Name = "txtMonth";
		this.txtMonth.Size = new System.Drawing.Size(24, 20);
		this.txtMonth.TabIndex = 12;
		this.txtMonth.Text = "2";
		// 
		// txtFromOADate
		// 
		this.txtFromOADate.Location = new System.Drawing.Point(136, 224);
		this.txtFromOADate.Name = "txtFromOADate";
		this.txtFromOADate.Size = new System.Drawing.Size(88, 20);
		this.txtFromOADate.TabIndex = 11;
		this.txtFromOADate.Text = "36578.325";
		// 
		// txtIsLeapYear
		// 
		this.txtIsLeapYear.Location = new System.Drawing.Point(136, 200);
		this.txtIsLeapYear.Name = "txtIsLeapYear";
		this.txtIsLeapYear.Size = new System.Drawing.Size(88, 20);
		this.txtIsLeapYear.TabIndex = 10;
		this.txtIsLeapYear.Text = "2002";
		// 
		// txtYear
		// 
		this.txtYear.Location = new System.Drawing.Point(136, 176);
		this.txtYear.Name = "txtYear";
		this.txtYear.Size = new System.Drawing.Size(88, 20);
		this.txtYear.TabIndex = 9;
		this.txtYear.Text = "2002";
		// 
		// Label8
		// 
		this.Label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label8.Location = new System.Drawing.Point(16, 224);
		this.Label8.Name = "Label8";
		this.Label8.Size = new System.Drawing.Size(112, 20);
		this.Label8.TabIndex = 8;
		this.Label8.Text = "FromOADate";
		this.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label7
		// 
		this.Label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label7.Location = new System.Drawing.Point(16, 200);
		this.Label7.Name = "Label7";
		this.Label7.Size = new System.Drawing.Size(112, 20);
		this.Label7.TabIndex = 7;
		this.Label7.Text = "IsLeapYear";
		this.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label6
		// 
		this.Label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label6.Location = new System.Drawing.Point(16, 176);
		this.Label6.Name = "Label6";
		this.Label6.Size = new System.Drawing.Size(112, 20);
		this.Label6.TabIndex = 6;
		this.Label6.Text = "DaysInMonth";
		this.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label5
		// 
		this.Label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label5.Location = new System.Drawing.Point(16, 128);
		this.Label5.Name = "Label5";
		this.Label5.Size = new System.Drawing.Size(112, 20);
		this.Label5.TabIndex = 5;
		this.Label5.Text = "MaxValue";
		this.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label4
		// 
		this.Label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label4.Location = new System.Drawing.Point(16, 104);
		this.Label4.Name = "Label4";
		this.Label4.Size = new System.Drawing.Size(112, 20);
		this.Label4.TabIndex = 4;
		this.Label4.Text = "MinValue";
		this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label3
		// 
		this.Label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label3.Location = new System.Drawing.Point(16, 80);
		this.Label3.Name = "Label3";
		this.Label3.Size = new System.Drawing.Size(112, 20);
		this.Label3.TabIndex = 3;
		this.Label3.Text = "UtcNow";
		this.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label2
		// 
		this.Label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label2.Location = new System.Drawing.Point(16, 56);
		this.Label2.Name = "Label2";
		this.Label2.Size = new System.Drawing.Size(112, 20);
		this.Label2.TabIndex = 2;
		this.Label2.Text = "Today";
		this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label1
		// 
		this.Label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label1.Location = new System.Drawing.Point(16, 32);
		this.Label1.Name = "Label1";
		this.Label1.Size = new System.Drawing.Size(112, 20);
		this.Label1.TabIndex = 1;
		this.Label1.Text = "Now";
		this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// btnRefreshstatic
		// 
		this.btnRefreshstatic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnRefreshstatic.Location = new System.Drawing.Point(16, 8);
		this.btnRefreshstatic.Name = "btnRefreshstatic";
		this.btnRefreshstatic.Size = new System.Drawing.Size(75, 24);
		this.btnRefreshstatic.TabIndex = 0;
		this.btnRefreshstatic.Text = "&Refresh";
		this.btnRefreshstatic.Click += new System.EventHandler(this.btnRefreshstatic_Click);
		// 
		// TabPage4
		// 
		this.TabPage4.Controls.Add(this.txtYears);
		this.TabPage4.Controls.Add(this.txtTicks);
		this.TabPage4.Controls.Add(this.txtSeconds);
		this.TabPage4.Controls.Add(this.txtMonths);
		this.TabPage4.Controls.Add(this.txtMinutes);
		this.TabPage4.Controls.Add(this.txtMilliseconds);
		this.TabPage4.Controls.Add(this.txtHours);
		this.TabPage4.Controls.Add(this.txtDays);
		this.TabPage4.Controls.Add(this.btnRefreshCalculation);
		this.TabPage4.Controls.Add(this.lblAddSeconds);
		this.TabPage4.Controls.Add(this.lblAddMonths);
		this.TabPage4.Controls.Add(this.lblAddMinutes);
		this.TabPage4.Controls.Add(this.lblAddMilliseconds);
		this.TabPage4.Controls.Add(this.lblAddHours);
		this.TabPage4.Controls.Add(this.lblAddDays);
		this.TabPage4.Controls.Add(this.lblAddTicks);
		this.TabPage4.Controls.Add(this.lblAddYears);
		this.TabPage4.Controls.Add(this.lblNow3);
		this.TabPage4.Controls.Add(this.Label49);
		this.TabPage4.Controls.Add(this.Label50);
		this.TabPage4.Controls.Add(this.Label51);
		this.TabPage4.Controls.Add(this.Label52);
		this.TabPage4.Controls.Add(this.Label53);
		this.TabPage4.Controls.Add(this.Label54);
		this.TabPage4.Controls.Add(this.Label55);
		this.TabPage4.Controls.Add(this.Label56);
		this.TabPage4.Controls.Add(this.Label57);
		this.TabPage4.Location = new System.Drawing.Point(4, 40);
		this.TabPage4.Name = "TabPage4";
		this.TabPage4.Size = new System.Drawing.Size(592, 260);
		this.TabPage4.TabIndex = 3;
		this.TabPage4.Text = "DateTime Calculation Methods";
		this.TabPage4.Visible = false;
		// 
		// txtYears
		// 
		this.txtYears.Location = new System.Drawing.Point(136, 224);
		this.txtYears.Name = "txtYears";
		this.txtYears.Size = new System.Drawing.Size(48, 20);
		this.txtYears.TabIndex = 64;
		this.txtYears.Text = "3";
		// 
		// txtTicks
		// 
		this.txtTicks.Location = new System.Drawing.Point(136, 200);
		this.txtTicks.Name = "txtTicks";
		this.txtTicks.Size = new System.Drawing.Size(48, 20);
		this.txtTicks.TabIndex = 63;
		this.txtTicks.Text = "3";
		// 
		// txtSeconds
		// 
		this.txtSeconds.Location = new System.Drawing.Point(136, 176);
		this.txtSeconds.Name = "txtSeconds";
		this.txtSeconds.Size = new System.Drawing.Size(48, 20);
		this.txtSeconds.TabIndex = 62;
		this.txtSeconds.Text = "3";
		// 
		// txtMonths
		// 
		this.txtMonths.Location = new System.Drawing.Point(136, 152);
		this.txtMonths.Name = "txtMonths";
		this.txtMonths.Size = new System.Drawing.Size(48, 20);
		this.txtMonths.TabIndex = 61;
		this.txtMonths.Text = "3";
		// 
		// txtMinutes
		// 
		this.txtMinutes.Location = new System.Drawing.Point(136, 128);
		this.txtMinutes.Name = "txtMinutes";
		this.txtMinutes.Size = new System.Drawing.Size(48, 20);
		this.txtMinutes.TabIndex = 60;
		this.txtMinutes.Text = "3";
		// 
		// txtMilliseconds
		// 
		this.txtMilliseconds.Location = new System.Drawing.Point(136, 104);
		this.txtMilliseconds.Name = "txtMilliseconds";
		this.txtMilliseconds.Size = new System.Drawing.Size(48, 20);
		this.txtMilliseconds.TabIndex = 59;
		this.txtMilliseconds.Text = "3";
		// 
		// txtHours
		// 
		this.txtHours.Location = new System.Drawing.Point(136, 80);
		this.txtHours.Name = "txtHours";
		this.txtHours.Size = new System.Drawing.Size(48, 20);
		this.txtHours.TabIndex = 58;
		this.txtHours.Text = "3";
		// 
		// txtDays
		// 
		this.txtDays.Location = new System.Drawing.Point(136, 56);
		this.txtDays.Name = "txtDays";
		this.txtDays.Size = new System.Drawing.Size(48, 20);
		this.txtDays.TabIndex = 57;
		this.txtDays.Text = "3";
		// 
		// btnRefreshCalculation
		// 
		this.btnRefreshCalculation.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnRefreshCalculation.Location = new System.Drawing.Point(16, 8);
		this.btnRefreshCalculation.Name = "btnRefreshCalculation";
		this.btnRefreshCalculation.Size = new System.Drawing.Size(75, 24);
		this.btnRefreshCalculation.TabIndex = 56;
		this.btnRefreshCalculation.Text = "Refresh";
		this.btnRefreshCalculation.Click += new System.EventHandler(this.btnRefreshCalculation_Click);
		// 
		// lblAddSeconds
		// 
		this.lblAddSeconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddSeconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddSeconds.Location = new System.Drawing.Point(184, 176);
		this.lblAddSeconds.Name = "lblAddSeconds";
		this.lblAddSeconds.Size = new System.Drawing.Size(200, 23);
		this.lblAddSeconds.TabIndex = 55;
		this.lblAddSeconds.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddMonths
		// 
		this.lblAddMonths.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddMonths.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddMonths.Location = new System.Drawing.Point(184, 152);
		this.lblAddMonths.Name = "lblAddMonths";
		this.lblAddMonths.Size = new System.Drawing.Size(200, 23);
		this.lblAddMonths.TabIndex = 54;
		this.lblAddMonths.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddMinutes
		// 
		this.lblAddMinutes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddMinutes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddMinutes.Location = new System.Drawing.Point(184, 128);
		this.lblAddMinutes.Name = "lblAddMinutes";
		this.lblAddMinutes.Size = new System.Drawing.Size(200, 23);
		this.lblAddMinutes.TabIndex = 53;
		this.lblAddMinutes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddMilliseconds
		// 
		this.lblAddMilliseconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddMilliseconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddMilliseconds.Location = new System.Drawing.Point(184, 104);
		this.lblAddMilliseconds.Name = "lblAddMilliseconds";
		this.lblAddMilliseconds.Size = new System.Drawing.Size(200, 23);
		this.lblAddMilliseconds.TabIndex = 52;
		this.lblAddMilliseconds.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddHours
		// 
		this.lblAddHours.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddHours.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddHours.Location = new System.Drawing.Point(184, 80);
		this.lblAddHours.Name = "lblAddHours";
		this.lblAddHours.Size = new System.Drawing.Size(200, 23);
		this.lblAddHours.TabIndex = 51;
		this.lblAddHours.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddDays
		// 
		this.lblAddDays.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddDays.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddDays.Location = new System.Drawing.Point(184, 56);
		this.lblAddDays.Name = "lblAddDays";
		this.lblAddDays.Size = new System.Drawing.Size(200, 23);
		this.lblAddDays.TabIndex = 50;
		this.lblAddDays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddTicks
		// 
		this.lblAddTicks.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddTicks.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddTicks.Location = new System.Drawing.Point(184, 200);
		this.lblAddTicks.Name = "lblAddTicks";
		this.lblAddTicks.Size = new System.Drawing.Size(200, 23);
		this.lblAddTicks.TabIndex = 49;
		this.lblAddTicks.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblAddYears
		// 
		this.lblAddYears.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblAddYears.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblAddYears.Location = new System.Drawing.Point(184, 224);
		this.lblAddYears.Name = "lblAddYears";
		this.lblAddYears.Size = new System.Drawing.Size(200, 23);
		this.lblAddYears.TabIndex = 48;
		this.lblAddYears.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblNow3
		// 
		this.lblNow3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblNow3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblNow3.Location = new System.Drawing.Point(184, 32);
		this.lblNow3.Name = "lblNow3";
		this.lblNow3.Size = new System.Drawing.Size(200, 23);
		this.lblNow3.TabIndex = 43;
		this.lblNow3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// Label49
		// 
		this.Label49.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label49.Location = new System.Drawing.Point(16, 224);
		this.Label49.Name = "Label49";
		this.Label49.Size = new System.Drawing.Size(112, 20);
		this.Label49.TabIndex = 38;
		this.Label49.Text = "AddYears";
		this.Label49.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label50
		// 
		this.Label50.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label50.Location = new System.Drawing.Point(16, 200);
		this.Label50.Name = "Label50";
		this.Label50.Size = new System.Drawing.Size(112, 20);
		this.Label50.TabIndex = 37;
		this.Label50.Text = "AddTicks";
		this.Label50.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label51
		// 
		this.Label51.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label51.Location = new System.Drawing.Point(16, 176);
		this.Label51.Name = "Label51";
		this.Label51.Size = new System.Drawing.Size(112, 20);
		this.Label51.TabIndex = 36;
		this.Label51.Text = "AddSeconds";
		this.Label51.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label52
		// 
		this.Label52.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label52.Location = new System.Drawing.Point(16, 152);
		this.Label52.Name = "Label52";
		this.Label52.Size = new System.Drawing.Size(112, 20);
		this.Label52.TabIndex = 35;
		this.Label52.Text = "AddMonths";
		this.Label52.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label53
		// 
		this.Label53.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label53.Location = new System.Drawing.Point(16, 128);
		this.Label53.Name = "Label53";
		this.Label53.Size = new System.Drawing.Size(112, 20);
		this.Label53.TabIndex = 34;
		this.Label53.Text = "AddMinutes";
		this.Label53.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label54
		// 
		this.Label54.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label54.Location = new System.Drawing.Point(16, 104);
		this.Label54.Name = "Label54";
		this.Label54.Size = new System.Drawing.Size(112, 20);
		this.Label54.TabIndex = 33;
		this.Label54.Text = "AddMilliseconds";
		this.Label54.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label55
		// 
		this.Label55.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label55.Location = new System.Drawing.Point(16, 80);
		this.Label55.Name = "Label55";
		this.Label55.Size = new System.Drawing.Size(112, 20);
		this.Label55.TabIndex = 32;
		this.Label55.Text = "AddHours";
		this.Label55.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label56
		// 
		this.Label56.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label56.Location = new System.Drawing.Point(16, 56);
		this.Label56.Name = "Label56";
		this.Label56.Size = new System.Drawing.Size(112, 20);
		this.Label56.TabIndex = 31;
		this.Label56.Text = "AddDays";
		this.Label56.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label57
		// 
		this.Label57.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label57.Location = new System.Drawing.Point(16, 32);
		this.Label57.Name = "Label57";
		this.Label57.Size = new System.Drawing.Size(112, 20);
		this.Label57.TabIndex = 30;
		this.Label57.Text = "Now";
		this.Label57.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// TabPage2
		// 
		this.TabPage2.Controls.Add(this.lblMillisecond);
		this.TabPage2.Controls.Add(this.lblHour);
		this.TabPage2.Controls.Add(this.lblDayOfYear);
		this.TabPage2.Controls.Add(this.lblDayOfWeek);
		this.TabPage2.Controls.Add(this.lblDate);
		this.TabPage2.Controls.Add(this.lblDay);
		this.TabPage2.Controls.Add(this.lblMinute);
		this.TabPage2.Controls.Add(this.lblMonth);
		this.TabPage2.Controls.Add(this.lblSecond);
		this.TabPage2.Controls.Add(this.lblTicks);
		this.TabPage2.Controls.Add(this.lblYear);
		this.TabPage2.Controls.Add(this.lblTimeOfDay);
		this.TabPage2.Controls.Add(this.lblNow1);
		this.TabPage2.Controls.Add(this.Label21);
		this.TabPage2.Controls.Add(this.Label20);
		this.TabPage2.Controls.Add(this.Label19);
		this.TabPage2.Controls.Add(this.Label18);
		this.TabPage2.Controls.Add(this.Label17);
		this.TabPage2.Controls.Add(this.Label16);
		this.TabPage2.Controls.Add(this.Label15);
		this.TabPage2.Controls.Add(this.Label14);
		this.TabPage2.Controls.Add(this.Label13);
		this.TabPage2.Controls.Add(this.Label12);
		this.TabPage2.Controls.Add(this.Label11);
		this.TabPage2.Controls.Add(this.Label10);
		this.TabPage2.Controls.Add(this.btnRefreshProperties);
		this.TabPage2.Controls.Add(this.Label9);
		this.TabPage2.Location = new System.Drawing.Point(4, 40);
		this.TabPage2.Name = "TabPage2";
		this.TabPage2.Size = new System.Drawing.Size(592, 260);
		this.TabPage2.TabIndex = 1;
		this.TabPage2.Text = "DateTime Properties";
		this.TabPage2.Visible = false;
		// 
		// lblMillisecond
		// 
		this.lblMillisecond.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMillisecond.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMillisecond.Location = new System.Drawing.Point(392, 32);
		this.lblMillisecond.Name = "lblMillisecond";
		this.lblMillisecond.Size = new System.Drawing.Size(136, 23);
		this.lblMillisecond.TabIndex = 29;
		this.lblMillisecond.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblHour
		// 
		this.lblHour.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblHour.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblHour.Location = new System.Drawing.Point(136, 152);
		this.lblHour.Name = "lblHour";
		this.lblHour.Size = new System.Drawing.Size(136, 23);
		this.lblHour.TabIndex = 28;
		this.lblHour.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblDayOfYear
		// 
		this.lblDayOfYear.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblDayOfYear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblDayOfYear.Location = new System.Drawing.Point(136, 128);
		this.lblDayOfYear.Name = "lblDayOfYear";
		this.lblDayOfYear.Size = new System.Drawing.Size(136, 23);
		this.lblDayOfYear.TabIndex = 27;
		this.lblDayOfYear.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblDayOfWeek
		// 
		this.lblDayOfWeek.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblDayOfWeek.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblDayOfWeek.Location = new System.Drawing.Point(136, 104);
		this.lblDayOfWeek.Name = "lblDayOfWeek";
		this.lblDayOfWeek.Size = new System.Drawing.Size(136, 23);
		this.lblDayOfWeek.TabIndex = 26;
		this.lblDayOfWeek.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblDate
		// 
		this.lblDate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblDate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblDate.Location = new System.Drawing.Point(136, 80);
		this.lblDate.Name = "lblDate";
		this.lblDate.Size = new System.Drawing.Size(136, 23);
		this.lblDate.TabIndex = 25;
		this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblDay
		// 
		this.lblDay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblDay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblDay.Location = new System.Drawing.Point(136, 56);
		this.lblDay.Name = "lblDay";
		this.lblDay.Size = new System.Drawing.Size(136, 23);
		this.lblDay.TabIndex = 24;
		this.lblDay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblMinute
		// 
		this.lblMinute.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMinute.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMinute.Location = new System.Drawing.Point(392, 56);
		this.lblMinute.Name = "lblMinute";
		this.lblMinute.Size = new System.Drawing.Size(136, 23);
		this.lblMinute.TabIndex = 23;
		this.lblMinute.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblMonth
		// 
		this.lblMonth.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMonth.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMonth.Location = new System.Drawing.Point(392, 80);
		this.lblMonth.Name = "lblMonth";
		this.lblMonth.Size = new System.Drawing.Size(136, 23);
		this.lblMonth.TabIndex = 22;
		this.lblMonth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblSecond
		// 
		this.lblSecond.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblSecond.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblSecond.Location = new System.Drawing.Point(392, 104);
		this.lblSecond.Name = "lblSecond";
		this.lblSecond.Size = new System.Drawing.Size(136, 23);
		this.lblSecond.TabIndex = 21;
		this.lblSecond.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblTicks
		// 
		this.lblTicks.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTicks.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTicks.Location = new System.Drawing.Point(392, 128);
		this.lblTicks.Name = "lblTicks";
		this.lblTicks.Size = new System.Drawing.Size(136, 23);
		this.lblTicks.TabIndex = 20;
		this.lblTicks.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblYear
		// 
		this.lblYear.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblYear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblYear.Location = new System.Drawing.Point(392, 176);
		this.lblYear.Name = "lblYear";
		this.lblYear.Size = new System.Drawing.Size(136, 23);
		this.lblYear.TabIndex = 19;
		this.lblYear.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblTimeOfDay
		// 
		this.lblTimeOfDay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTimeOfDay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTimeOfDay.Location = new System.Drawing.Point(392, 152);
		this.lblTimeOfDay.Name = "lblTimeOfDay";
		this.lblTimeOfDay.Size = new System.Drawing.Size(136, 23);
		this.lblTimeOfDay.TabIndex = 18;
		this.lblTimeOfDay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblNow1
		// 
		this.lblNow1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblNow1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblNow1.Location = new System.Drawing.Point(136, 32);
		this.lblNow1.Name = "lblNow1";
		this.lblNow1.Size = new System.Drawing.Size(136, 23);
		this.lblNow1.TabIndex = 17;
		this.lblNow1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// Label21
		// 
		this.Label21.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label21.Location = new System.Drawing.Point(296, 176);
		this.Label21.Name = "Label21";
		this.Label21.Size = new System.Drawing.Size(88, 20);
		this.Label21.TabIndex = 15;
		this.Label21.Text = "Year";
		this.Label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label20
		// 
		this.Label20.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label20.Location = new System.Drawing.Point(296, 152);
		this.Label20.Name = "Label20";
		this.Label20.Size = new System.Drawing.Size(88, 20);
		this.Label20.TabIndex = 14;
		this.Label20.Text = "TimeOfDay";
		this.Label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label19
		// 
		this.Label19.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label19.Location = new System.Drawing.Point(296, 128);
		this.Label19.Name = "Label19";
		this.Label19.Size = new System.Drawing.Size(88, 20);
		this.Label19.TabIndex = 13;
		this.Label19.Text = "Ticks";
		this.Label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label18
		// 
		this.Label18.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label18.Location = new System.Drawing.Point(296, 104);
		this.Label18.Name = "Label18";
		this.Label18.Size = new System.Drawing.Size(88, 20);
		this.Label18.TabIndex = 12;
		this.Label18.Text = "Second";
		this.Label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label17
		// 
		this.Label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label17.Location = new System.Drawing.Point(296, 80);
		this.Label17.Name = "Label17";
		this.Label17.Size = new System.Drawing.Size(88, 20);
		this.Label17.TabIndex = 11;
		this.Label17.Text = "Month";
		this.Label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label16
		// 
		this.Label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label16.Location = new System.Drawing.Point(296, 56);
		this.Label16.Name = "Label16";
		this.Label16.Size = new System.Drawing.Size(88, 20);
		this.Label16.TabIndex = 10;
		this.Label16.Text = "Minute";
		this.Label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label15
		// 
		this.Label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label15.Location = new System.Drawing.Point(296, 32);
		this.Label15.Name = "Label15";
		this.Label15.Size = new System.Drawing.Size(88, 20);
		this.Label15.TabIndex = 9;
		this.Label15.Text = "Millisecond";
		this.Label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label14
		// 
		this.Label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label14.Location = new System.Drawing.Point(40, 152);
		this.Label14.Name = "Label14";
		this.Label14.Size = new System.Drawing.Size(88, 20);
		this.Label14.TabIndex = 8;
		this.Label14.Text = "Hour";
		this.Label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label13
		// 
		this.Label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label13.Location = new System.Drawing.Point(40, 128);
		this.Label13.Name = "Label13";
		this.Label13.Size = new System.Drawing.Size(88, 20);
		this.Label13.TabIndex = 7;
		this.Label13.Text = "DayOfYear";
		this.Label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label12
		// 
		this.Label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label12.Location = new System.Drawing.Point(40, 104);
		this.Label12.Name = "Label12";
		this.Label12.Size = new System.Drawing.Size(88, 20);
		this.Label12.TabIndex = 6;
		this.Label12.Text = "DayOfWeek";
		this.Label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label11
		// 
		this.Label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label11.Location = new System.Drawing.Point(40, 80);
		this.Label11.Name = "Label11";
		this.Label11.Size = new System.Drawing.Size(88, 20);
		this.Label11.TabIndex = 5;
		this.Label11.Text = "Day";
		this.Label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label10
		// 
		this.Label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label10.Location = new System.Drawing.Point(40, 56);
		this.Label10.Name = "Label10";
		this.Label10.Size = new System.Drawing.Size(88, 20);
		this.Label10.TabIndex = 4;
		this.Label10.Text = "Date";
		this.Label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// btnRefreshProperties
		// 
		this.btnRefreshProperties.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnRefreshProperties.Location = new System.Drawing.Point(16, 8);
		this.btnRefreshProperties.Name = "btnRefreshProperties";
		this.btnRefreshProperties.Size = new System.Drawing.Size(75, 24);
		this.btnRefreshProperties.TabIndex = 3;
		this.btnRefreshProperties.Text = "Refresh";
		this.btnRefreshProperties.Click += new System.EventHandler(this.btnRefreshProperties_Click);
		// 
		// Label9
		// 
		this.Label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label9.Location = new System.Drawing.Point(40, 32);
		this.Label9.Name = "Label9";
		this.Label9.Size = new System.Drawing.Size(88, 20);
		this.Label9.TabIndex = 2;
		this.Label9.Text = "Now";
		this.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// TabPage3
		// 
		this.TabPage3.Controls.Add(this.lblToShortDatestring);
		this.TabPage3.Controls.Add(this.lblToOADate);
		this.TabPage3.Controls.Add(this.lblToLongTimestring);
		this.TabPage3.Controls.Add(this.lblToLongDatestring);
		this.TabPage3.Controls.Add(this.lblToLocalTime);
		this.TabPage3.Controls.Add(this.lblToFileTime);
		this.TabPage3.Controls.Add(this.lblToShortTimestring);
		this.TabPage3.Controls.Add(this.lblTostring);
		this.TabPage3.Controls.Add(this.lblToUniversalTime);
		this.TabPage3.Controls.Add(this.Label38);
		this.TabPage3.Controls.Add(this.Label39);
		this.TabPage3.Controls.Add(this.Label40);
		this.TabPage3.Controls.Add(this.Label41);
		this.TabPage3.Controls.Add(this.Label42);
		this.TabPage3.Controls.Add(this.Label43);
		this.TabPage3.Controls.Add(this.Label44);
		this.TabPage3.Controls.Add(this.Label45);
		this.TabPage3.Controls.Add(this.Label46);
		this.TabPage3.Controls.Add(this.lblNow2);
		this.TabPage3.Controls.Add(this.btnRefreshConversion);
		this.TabPage3.Controls.Add(this.Label23);
		this.TabPage3.Location = new System.Drawing.Point(4, 40);
		this.TabPage3.Name = "TabPage3";
		this.TabPage3.Size = new System.Drawing.Size(592, 260);
		this.TabPage3.TabIndex = 2;
		this.TabPage3.Text = "DateTime Conversion Methods";
		this.TabPage3.Visible = false;
		// 
		// lblToShortDatestring
		// 
		this.lblToShortDatestring.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToShortDatestring.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToShortDatestring.Location = new System.Drawing.Point(416, 56);
		this.lblToShortDatestring.Name = "lblToShortDatestring";
		this.lblToShortDatestring.Size = new System.Drawing.Size(160, 23);
		this.lblToShortDatestring.TabIndex = 53;
		this.lblToShortDatestring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToOADate
		// 
		this.lblToOADate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToOADate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToOADate.Location = new System.Drawing.Point(416, 32);
		this.lblToOADate.Name = "lblToOADate";
		this.lblToOADate.Size = new System.Drawing.Size(160, 23);
		this.lblToOADate.TabIndex = 52;
		this.lblToOADate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToLongTimestring
		// 
		this.lblToLongTimestring.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToLongTimestring.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToLongTimestring.Location = new System.Drawing.Point(136, 128);
		this.lblToLongTimestring.Name = "lblToLongTimestring";
		this.lblToLongTimestring.Size = new System.Drawing.Size(144, 23);
		this.lblToLongTimestring.TabIndex = 51;
		this.lblToLongTimestring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToLongDatestring
		// 
		this.lblToLongDatestring.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToLongDatestring.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToLongDatestring.Location = new System.Drawing.Point(136, 104);
		this.lblToLongDatestring.Name = "lblToLongDatestring";
		this.lblToLongDatestring.Size = new System.Drawing.Size(144, 23);
		this.lblToLongDatestring.TabIndex = 50;
		this.lblToLongDatestring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToLocalTime
		// 
		this.lblToLocalTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToLocalTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToLocalTime.Location = new System.Drawing.Point(136, 80);
		this.lblToLocalTime.Name = "lblToLocalTime";
		this.lblToLocalTime.Size = new System.Drawing.Size(144, 23);
		this.lblToLocalTime.TabIndex = 49;
		this.lblToLocalTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToFileTime
		// 
		this.lblToFileTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToFileTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToFileTime.Location = new System.Drawing.Point(136, 56);
		this.lblToFileTime.Name = "lblToFileTime";
		this.lblToFileTime.Size = new System.Drawing.Size(144, 23);
		this.lblToFileTime.TabIndex = 48;
		this.lblToFileTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToShortTimestring
		// 
		this.lblToShortTimestring.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToShortTimestring.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToShortTimestring.Location = new System.Drawing.Point(416, 80);
		this.lblToShortTimestring.Name = "lblToShortTimestring";
		this.lblToShortTimestring.Size = new System.Drawing.Size(160, 23);
		this.lblToShortTimestring.TabIndex = 47;
		this.lblToShortTimestring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblTostring
		// 
		this.lblTostring.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTostring.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTostring.Location = new System.Drawing.Point(416, 104);
		this.lblTostring.Name = "lblTostring";
		this.lblTostring.Size = new System.Drawing.Size(160, 23);
		this.lblTostring.TabIndex = 46;
		this.lblTostring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// lblToUniversalTime
		// 
		this.lblToUniversalTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblToUniversalTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblToUniversalTime.Location = new System.Drawing.Point(416, 128);
		this.lblToUniversalTime.Name = "lblToUniversalTime";
		this.lblToUniversalTime.Size = new System.Drawing.Size(160, 23);
		this.lblToUniversalTime.TabIndex = 45;
		this.lblToUniversalTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// Label38
		// 
		this.Label38.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label38.Location = new System.Drawing.Point(296, 128);
		this.Label38.Name = "Label38";
		this.Label38.Size = new System.Drawing.Size(112, 20);
		this.Label38.TabIndex = 38;
		this.Label38.Text = "ToUniversalTime";
		this.Label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label39
		// 
		this.Label39.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label39.Location = new System.Drawing.Point(296, 104);
		this.Label39.Name = "Label39";
		this.Label39.Size = new System.Drawing.Size(112, 20);
		this.Label39.TabIndex = 37;
		this.Label39.Text = "Tostring";
		this.Label39.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label40
		// 
		this.Label40.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label40.Location = new System.Drawing.Point(296, 80);
		this.Label40.Name = "Label40";
		this.Label40.Size = new System.Drawing.Size(112, 20);
		this.Label40.TabIndex = 36;
		this.Label40.Text = "ToShortTimestring";
		this.Label40.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label41
		// 
		this.Label41.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label41.Location = new System.Drawing.Point(296, 56);
		this.Label41.Name = "Label41";
		this.Label41.Size = new System.Drawing.Size(112, 20);
		this.Label41.TabIndex = 35;
		this.Label41.Text = "ToShortDatestring";
		this.Label41.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label42
		// 
		this.Label42.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label42.Location = new System.Drawing.Point(296, 32);
		this.Label42.Name = "Label42";
		this.Label42.Size = new System.Drawing.Size(112, 20);
		this.Label42.TabIndex = 34;
		this.Label42.Text = "ToOADate";
		this.Label42.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label43
		// 
		this.Label43.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label43.Location = new System.Drawing.Point(16, 128);
		this.Label43.Name = "Label43";
		this.Label43.Size = new System.Drawing.Size(112, 20);
		this.Label43.TabIndex = 33;
		this.Label43.Text = "ToLongTimestring";
		this.Label43.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label44
		// 
		this.Label44.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label44.Location = new System.Drawing.Point(16, 104);
		this.Label44.Name = "Label44";
		this.Label44.Size = new System.Drawing.Size(112, 20);
		this.Label44.TabIndex = 32;
		this.Label44.Text = "ToLongDatestring";
		this.Label44.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label45
		// 
		this.Label45.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label45.Location = new System.Drawing.Point(16, 80);
		this.Label45.Name = "Label45";
		this.Label45.Size = new System.Drawing.Size(112, 20);
		this.Label45.TabIndex = 31;
		this.Label45.Text = "ToLocalTime";
		this.Label45.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label46
		// 
		this.Label46.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label46.Location = new System.Drawing.Point(16, 56);
		this.Label46.Name = "Label46";
		this.Label46.Size = new System.Drawing.Size(112, 20);
		this.Label46.TabIndex = 30;
		this.Label46.Text = "ToFileTime";
		this.Label46.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblNow2
		// 
		this.lblNow2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblNow2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblNow2.Location = new System.Drawing.Point(136, 32);
		this.lblNow2.Name = "lblNow2";
		this.lblNow2.Size = new System.Drawing.Size(144, 23);
		this.lblNow2.TabIndex = 20;
		this.lblNow2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// 
		// btnRefreshConversion
		// 
		this.btnRefreshConversion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnRefreshConversion.Location = new System.Drawing.Point(16, 8);
		this.btnRefreshConversion.Name = "btnRefreshConversion";
		this.btnRefreshConversion.Size = new System.Drawing.Size(75, 24);
		this.btnRefreshConversion.TabIndex = 19;
		this.btnRefreshConversion.Text = "Refresh";
		this.btnRefreshConversion.Click += new System.EventHandler(this.btnRefreshConversion_Click);
		// 
		// Label23
		// 
		this.Label23.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label23.Location = new System.Drawing.Point(16, 32);
		this.Label23.Name = "Label23";
		this.Label23.Size = new System.Drawing.Size(112, 20);
		this.Label23.TabIndex = 18;
		this.Label23.Text = "Now";
		this.Label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// TabPage6
		// 
		this.TabPage6.Controls.Add(this.lblTotalSeconds);
		this.TabPage6.Controls.Add(this.Label61);
		this.TabPage6.Controls.Add(this.lblTotalMinutes);
		this.TabPage6.Controls.Add(this.Label62);
		this.TabPage6.Controls.Add(this.lblTotalMilliseconds);
		this.TabPage6.Controls.Add(this.Label63);
		this.TabPage6.Controls.Add(this.lblTotalHours);
		this.TabPage6.Controls.Add(this.Label64);
		this.TabPage6.Controls.Add(this.lblTotalDays);
		this.TabPage6.Controls.Add(this.Label65);
		this.TabPage6.Controls.Add(this.lblTimeSpanTicks);
		this.TabPage6.Controls.Add(this.Label67);
		this.TabPage6.Controls.Add(this.lblSeconds);
		this.TabPage6.Controls.Add(this.Label68);
		this.TabPage6.Controls.Add(this.lblMinutes);
		this.TabPage6.Controls.Add(this.Label69);
		this.TabPage6.Controls.Add(this.lblMilliseconds);
		this.TabPage6.Controls.Add(this.Label70);
		this.TabPage6.Controls.Add(this.lblHours);
		this.TabPage6.Controls.Add(this.Label71);
		this.TabPage6.Controls.Add(this.lblDays);
		this.TabPage6.Controls.Add(this.Label72);
		this.TabPage6.Controls.Add(this.tabTimeSpan);
		this.TabPage6.Location = new System.Drawing.Point(4, 40);
		this.TabPage6.Name = "TabPage6";
		this.TabPage6.Size = new System.Drawing.Size(592, 260);
		this.TabPage6.TabIndex = 5;
		this.TabPage6.Text = "TimeSpan Properties";
		this.TabPage6.Visible = false;
		// 
		// lblTotalSeconds
		// 
		this.lblTotalSeconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTotalSeconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTotalSeconds.Location = new System.Drawing.Point(376, 232);
		this.lblTotalSeconds.Name = "lblTotalSeconds";
		this.lblTotalSeconds.Size = new System.Drawing.Size(136, 23);
		this.lblTotalSeconds.TabIndex = 50;
		this.lblTotalSeconds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label61
		// 
		this.Label61.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label61.Location = new System.Drawing.Point(272, 232);
		this.Label61.Name = "Label61";
		this.Label61.Size = new System.Drawing.Size(96, 20);
		this.Label61.TabIndex = 49;
		this.Label61.Text = "TotalSeconds";
		this.Label61.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTotalMinutes
		// 
		this.lblTotalMinutes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTotalMinutes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTotalMinutes.Location = new System.Drawing.Point(376, 208);
		this.lblTotalMinutes.Name = "lblTotalMinutes";
		this.lblTotalMinutes.Size = new System.Drawing.Size(136, 23);
		this.lblTotalMinutes.TabIndex = 48;
		this.lblTotalMinutes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label62
		// 
		this.Label62.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label62.Location = new System.Drawing.Point(272, 208);
		this.Label62.Name = "Label62";
		this.Label62.Size = new System.Drawing.Size(96, 20);
		this.Label62.TabIndex = 47;
		this.Label62.Text = "TotalMinutes";
		this.Label62.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTotalMilliseconds
		// 
		this.lblTotalMilliseconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTotalMilliseconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTotalMilliseconds.Location = new System.Drawing.Point(376, 184);
		this.lblTotalMilliseconds.Name = "lblTotalMilliseconds";
		this.lblTotalMilliseconds.Size = new System.Drawing.Size(136, 23);
		this.lblTotalMilliseconds.TabIndex = 46;
		this.lblTotalMilliseconds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label63
		// 
		this.Label63.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label63.Location = new System.Drawing.Point(272, 184);
		this.Label63.Name = "Label63";
		this.Label63.Size = new System.Drawing.Size(96, 20);
		this.Label63.TabIndex = 45;
		this.Label63.Text = "TotalMilliseconds";
		this.Label63.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTotalHours
		// 
		this.lblTotalHours.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTotalHours.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTotalHours.Location = new System.Drawing.Point(376, 160);
		this.lblTotalHours.Name = "lblTotalHours";
		this.lblTotalHours.Size = new System.Drawing.Size(136, 23);
		this.lblTotalHours.TabIndex = 44;
		this.lblTotalHours.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label64
		// 
		this.Label64.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label64.Location = new System.Drawing.Point(272, 160);
		this.Label64.Name = "Label64";
		this.Label64.Size = new System.Drawing.Size(96, 20);
		this.Label64.TabIndex = 43;
		this.Label64.Text = "TotalHours";
		this.Label64.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTotalDays
		// 
		this.lblTotalDays.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTotalDays.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTotalDays.Location = new System.Drawing.Point(376, 136);
		this.lblTotalDays.Name = "lblTotalDays";
		this.lblTotalDays.Size = new System.Drawing.Size(136, 23);
		this.lblTotalDays.TabIndex = 42;
		this.lblTotalDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label65
		// 
		this.Label65.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label65.Location = new System.Drawing.Point(272, 136);
		this.Label65.Name = "Label65";
		this.Label65.Size = new System.Drawing.Size(96, 20);
		this.Label65.TabIndex = 41;
		this.Label65.Text = "TotalDays";
		this.Label65.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTimeSpanTicks
		// 
		this.lblTimeSpanTicks.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTimeSpanTicks.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTimeSpanTicks.Location = new System.Drawing.Point(376, 112);
		this.lblTimeSpanTicks.Name = "lblTimeSpanTicks";
		this.lblTimeSpanTicks.Size = new System.Drawing.Size(136, 23);
		this.lblTimeSpanTicks.TabIndex = 40;
		this.lblTimeSpanTicks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label67
		// 
		this.Label67.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label67.Location = new System.Drawing.Point(272, 112);
		this.Label67.Name = "Label67";
		this.Label67.Size = new System.Drawing.Size(96, 20);
		this.Label67.TabIndex = 39;
		this.Label67.Text = "Ticks";
		this.Label67.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblSeconds
		// 
		this.lblSeconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblSeconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblSeconds.Location = new System.Drawing.Point(120, 208);
		this.lblSeconds.Name = "lblSeconds";
		this.lblSeconds.Size = new System.Drawing.Size(136, 23);
		this.lblSeconds.TabIndex = 38;
		this.lblSeconds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label68
		// 
		this.Label68.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label68.Location = new System.Drawing.Point(16, 208);
		this.Label68.Name = "Label68";
		this.Label68.Size = new System.Drawing.Size(96, 20);
		this.Label68.TabIndex = 37;
		this.Label68.Text = "Seconds";
		this.Label68.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblMinutes
		// 
		this.lblMinutes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMinutes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMinutes.Location = new System.Drawing.Point(120, 184);
		this.lblMinutes.Name = "lblMinutes";
		this.lblMinutes.Size = new System.Drawing.Size(136, 23);
		this.lblMinutes.TabIndex = 36;
		this.lblMinutes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label69
		// 
		this.Label69.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label69.Location = new System.Drawing.Point(16, 184);
		this.Label69.Name = "Label69";
		this.Label69.Size = new System.Drawing.Size(96, 20);
		this.Label69.TabIndex = 35;
		this.Label69.Text = "Minutes";
		this.Label69.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblMilliseconds
		// 
		this.lblMilliseconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMilliseconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMilliseconds.Location = new System.Drawing.Point(120, 160);
		this.lblMilliseconds.Name = "lblMilliseconds";
		this.lblMilliseconds.Size = new System.Drawing.Size(136, 23);
		this.lblMilliseconds.TabIndex = 34;
		this.lblMilliseconds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label70
		// 
		this.Label70.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label70.Location = new System.Drawing.Point(16, 160);
		this.Label70.Name = "Label70";
		this.Label70.Size = new System.Drawing.Size(96, 20);
		this.Label70.TabIndex = 33;
		this.Label70.Text = "Milliseconds";
		this.Label70.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblHours
		// 
		this.lblHours.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblHours.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblHours.Location = new System.Drawing.Point(120, 136);
		this.lblHours.Name = "lblHours";
		this.lblHours.Size = new System.Drawing.Size(136, 23);
		this.lblHours.TabIndex = 32;
		this.lblHours.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label71
		// 
		this.Label71.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label71.Location = new System.Drawing.Point(16, 136);
		this.Label71.Name = "Label71";
		this.Label71.Size = new System.Drawing.Size(96, 20);
		this.Label71.TabIndex = 31;
		this.Label71.Text = "Hours";
		this.Label71.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblDays
		// 
		this.lblDays.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblDays.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblDays.Location = new System.Drawing.Point(120, 112);
		this.lblDays.Name = "lblDays";
		this.lblDays.Size = new System.Drawing.Size(136, 23);
		this.lblDays.TabIndex = 30;
		this.lblDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label72
		// 
		this.Label72.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label72.Location = new System.Drawing.Point(16, 112);
		this.Label72.Name = "Label72";
		this.Label72.Size = new System.Drawing.Size(96, 20);
		this.Label72.TabIndex = 29;
		this.Label72.Text = "Days";
		this.Label72.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// tabTimeSpan
		// 
		this.tabTimeSpan.Controls.Add(this.TabPage9);
		this.tabTimeSpan.Controls.Add(this.TabPage10);
		this.tabTimeSpan.ItemSize = new System.Drawing.Size(136, 18);
		this.tabTimeSpan.Location = new System.Drawing.Point(8, 8);
		this.tabTimeSpan.Name = "tabTimeSpan";
		this.tabTimeSpan.SelectedIndex = 0;
		this.tabTimeSpan.Size = new System.Drawing.Size(440, 96);
		this.tabTimeSpan.TabIndex = 10;
		this.tabTimeSpan.SelectedIndexChanged += new System.EventHandler(this.tabTimeSpan_SelectedIndexChanged);
		// 
		// TabPage9
		// 
		this.TabPage9.Controls.Add(this.btnRefreshTSProperties);
		this.TabPage9.Controls.Add(this.Label28);
		this.TabPage9.Controls.Add(this.Label31);
		this.TabPage9.Controls.Add(this.txtEnd);
		this.TabPage9.Controls.Add(this.txtStart);
		this.TabPage9.Location = new System.Drawing.Point(4, 22);
		this.TabPage9.Name = "TabPage9";
		this.TabPage9.Size = new System.Drawing.Size(432, 70);
		this.TabPage9.TabIndex = 0;
		this.TabPage9.Text = "Enter Start and End Times";
		// 
		// btnRefreshTSProperties
		// 
		this.btnRefreshTSProperties.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnRefreshTSProperties.Location = new System.Drawing.Point(304, 8);
		this.btnRefreshTSProperties.Name = "btnRefreshTSProperties";
		this.btnRefreshTSProperties.Size = new System.Drawing.Size(120, 23);
		this.btnRefreshTSProperties.TabIndex = 9;
		this.btnRefreshTSProperties.Text = "Refresh";
		this.btnRefreshTSProperties.Click += new System.EventHandler(this.btnRefreshTSProperties_Click);
		// 
		// Label28
		// 
		this.Label28.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label28.Location = new System.Drawing.Point(8, 40);
		this.Label28.Name = "Label28";
		this.Label28.Size = new System.Drawing.Size(104, 20);
		this.Label28.TabIndex = 8;
		this.Label28.Text = "Ending Time";
		this.Label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label31
		// 
		this.Label31.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label31.Location = new System.Drawing.Point(8, 8);
		this.Label31.Name = "Label31";
		this.Label31.Size = new System.Drawing.Size(104, 20);
		this.Label31.TabIndex = 7;
		this.Label31.Text = "Starting Time";
		this.Label31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtEnd
		// 
		this.txtEnd.Location = new System.Drawing.Point(120, 40);
		this.txtEnd.Name = "txtEnd";
		this.txtEnd.Size = new System.Drawing.Size(176, 20);
		this.txtEnd.TabIndex = 6;
		this.txtEnd.Text = "5:25:17 PM";
		// 
		// txtStart
		// 
		this.txtStart.Location = new System.Drawing.Point(120, 8);
		this.txtStart.Name = "txtStart";
		this.txtStart.Size = new System.Drawing.Size(176, 20);
		this.txtStart.TabIndex = 5;
		this.txtStart.Text = "8:14:12 AM";
		// 
		// TabPage10
		// 
		this.TabPage10.Controls.Add(this.btnCalcParse);
		this.TabPage10.Controls.Add(this.txtParse);
		this.TabPage10.Controls.Add(this.Label33);
		this.TabPage10.Location = new System.Drawing.Point(4, 22);
		this.TabPage10.Name = "TabPage10";
		this.TabPage10.Size = new System.Drawing.Size(432, 70);
		this.TabPage10.TabIndex = 1;
		this.TabPage10.Text = "Parse TimeSpan";
		this.TabPage10.Visible = false;
		// 
		// btnCalcParse
		// 
		this.btnCalcParse.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnCalcParse.Location = new System.Drawing.Point(304, 8);
		this.btnCalcParse.Name = "btnCalcParse";
		this.btnCalcParse.Size = new System.Drawing.Size(120, 23);
		this.btnCalcParse.TabIndex = 8;
		this.btnCalcParse.Text = "Refresh";
		this.btnCalcParse.Click += new System.EventHandler(this.btnCalcParse_Click);
		// 
		// txtParse
		// 
		this.txtParse.Location = new System.Drawing.Point(120, 8);
		this.txtParse.Name = "txtParse";
		this.txtParse.Size = new System.Drawing.Size(176, 20);
		this.txtParse.TabIndex = 6;
		this.txtParse.Text = "3.14:55:26.27";
		// 
		// Label33
		// 
		this.Label33.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label33.Location = new System.Drawing.Point(16, 8);
		this.Label33.Name = "Label33";
		this.Label33.Size = new System.Drawing.Size(96, 20);
		this.Label33.TabIndex = 7;
		this.Label33.Text = "Parse";
		this.Label33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// TabPage7
		// 
		this.TabPage7.Controls.Add(this.btnRefreshTSMethods);
		this.TabPage7.Controls.Add(this.txtFromHours);
		this.TabPage7.Controls.Add(this.lblFromHours);
		this.TabPage7.Controls.Add(this.Label34);
		this.TabPage7.Controls.Add(this.txtFromSeconds);
		this.TabPage7.Controls.Add(this.lblFromSeconds);
		this.TabPage7.Controls.Add(this.Label35);
		this.TabPage7.Controls.Add(this.txtFromTicks);
		this.TabPage7.Controls.Add(this.lblFromTicks);
		this.TabPage7.Controls.Add(this.Label36);
		this.TabPage7.Controls.Add(this.txtFromMilliseconds);
		this.TabPage7.Controls.Add(this.lblFromMilliseconds);
		this.TabPage7.Controls.Add(this.Label37);
		this.TabPage7.Controls.Add(this.txtFromMinutes);
		this.TabPage7.Controls.Add(this.lblFromMinutes);
		this.TabPage7.Controls.Add(this.Label47);
		this.TabPage7.Controls.Add(this.txtFromDays);
		this.TabPage7.Controls.Add(this.lblFromDays);
		this.TabPage7.Controls.Add(this.Label48);
		this.TabPage7.Location = new System.Drawing.Point(4, 40);
		this.TabPage7.Name = "TabPage7";
		this.TabPage7.Size = new System.Drawing.Size(592, 260);
		this.TabPage7.TabIndex = 6;
		this.TabPage7.Text = "TimeSpan Methods";
		this.TabPage7.Visible = false;
		// 
		// btnRefreshTSMethods
		// 
		this.btnRefreshTSMethods.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.btnRefreshTSMethods.Location = new System.Drawing.Point(16, 8);
		this.btnRefreshTSMethods.Name = "btnRefreshTSMethods";
		this.btnRefreshTSMethods.Size = new System.Drawing.Size(75, 24);
		this.btnRefreshTSMethods.TabIndex = 27;
		this.btnRefreshTSMethods.Text = "Refresh";
		this.btnRefreshTSMethods.Click += new System.EventHandler(this.btnRefreshTSMethods_Click);
		// 
		// txtFromHours
		// 
		this.txtFromHours.Location = new System.Drawing.Point(136, 56);
		this.txtFromHours.Name = "txtFromHours";
		this.txtFromHours.Size = new System.Drawing.Size(104, 20);
		this.txtFromHours.TabIndex = 35;
		this.txtFromHours.Text = "47.6";
		this.txtFromHours.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// lblFromHours
		// 
		this.lblFromHours.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromHours.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromHours.Location = new System.Drawing.Point(248, 56);
		this.lblFromHours.Name = "lblFromHours";
		this.lblFromHours.Size = new System.Drawing.Size(152, 23);
		this.lblFromHours.TabIndex = 41;
		this.lblFromHours.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label34
		// 
		this.Label34.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label34.Location = new System.Drawing.Point(24, 56);
		this.Label34.Name = "Label34";
		this.Label34.Size = new System.Drawing.Size(104, 20);
		this.Label34.TabIndex = 29;
		this.Label34.Text = "FromHours";
		this.Label34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtFromSeconds
		// 
		this.txtFromSeconds.Location = new System.Drawing.Point(136, 128);
		this.txtFromSeconds.Name = "txtFromSeconds";
		this.txtFromSeconds.Size = new System.Drawing.Size(104, 20);
		this.txtFromSeconds.TabIndex = 38;
		this.txtFromSeconds.Text = "289";
		this.txtFromSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// lblFromSeconds
		// 
		this.lblFromSeconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromSeconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromSeconds.Location = new System.Drawing.Point(248, 128);
		this.lblFromSeconds.Name = "lblFromSeconds";
		this.lblFromSeconds.Size = new System.Drawing.Size(152, 23);
		this.lblFromSeconds.TabIndex = 44;
		this.lblFromSeconds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label35
		// 
		this.Label35.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label35.Location = new System.Drawing.Point(24, 128);
		this.Label35.Name = "Label35";
		this.Label35.Size = new System.Drawing.Size(104, 20);
		this.Label35.TabIndex = 32;
		this.Label35.Text = "FromSeconds";
		this.Label35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtFromTicks
		// 
		this.txtFromTicks.Location = new System.Drawing.Point(136, 152);
		this.txtFromTicks.Name = "txtFromTicks";
		this.txtFromTicks.Size = new System.Drawing.Size(104, 20);
		this.txtFromTicks.TabIndex = 39;
		this.txtFromTicks.Text = "123456789";
		this.txtFromTicks.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// lblFromTicks
		// 
		this.lblFromTicks.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromTicks.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromTicks.Location = new System.Drawing.Point(248, 152);
		this.lblFromTicks.Name = "lblFromTicks";
		this.lblFromTicks.Size = new System.Drawing.Size(152, 23);
		this.lblFromTicks.TabIndex = 45;
		this.lblFromTicks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label36
		// 
		this.Label36.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label36.Location = new System.Drawing.Point(24, 152);
		this.Label36.Name = "Label36";
		this.Label36.Size = new System.Drawing.Size(104, 20);
		this.Label36.TabIndex = 33;
		this.Label36.Text = "FromTicks";
		this.Label36.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtFromMilliseconds
		// 
		this.txtFromMilliseconds.Location = new System.Drawing.Point(136, 80);
		this.txtFromMilliseconds.Name = "txtFromMilliseconds";
		this.txtFromMilliseconds.Size = new System.Drawing.Size(104, 20);
		this.txtFromMilliseconds.TabIndex = 36;
		this.txtFromMilliseconds.Text = "20098";
		this.txtFromMilliseconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// lblFromMilliseconds
		// 
		this.lblFromMilliseconds.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromMilliseconds.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromMilliseconds.Location = new System.Drawing.Point(248, 80);
		this.lblFromMilliseconds.Name = "lblFromMilliseconds";
		this.lblFromMilliseconds.Size = new System.Drawing.Size(152, 23);
		this.lblFromMilliseconds.TabIndex = 42;
		this.lblFromMilliseconds.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label37
		// 
		this.Label37.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label37.Location = new System.Drawing.Point(24, 80);
		this.Label37.Name = "Label37";
		this.Label37.Size = new System.Drawing.Size(104, 20);
		this.Label37.TabIndex = 30;
		this.Label37.Text = "FromMilliseconds";
		this.Label37.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtFromMinutes
		// 
		this.txtFromMinutes.Location = new System.Drawing.Point(136, 104);
		this.txtFromMinutes.Name = "txtFromMinutes";
		this.txtFromMinutes.Size = new System.Drawing.Size(104, 20);
		this.txtFromMinutes.TabIndex = 37;
		this.txtFromMinutes.Text = "128";
		this.txtFromMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// lblFromMinutes
		// 
		this.lblFromMinutes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromMinutes.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromMinutes.Location = new System.Drawing.Point(248, 104);
		this.lblFromMinutes.Name = "lblFromMinutes";
		this.lblFromMinutes.Size = new System.Drawing.Size(152, 23);
		this.lblFromMinutes.TabIndex = 43;
		this.lblFromMinutes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label47
		// 
		this.Label47.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label47.Location = new System.Drawing.Point(24, 104);
		this.Label47.Name = "Label47";
		this.Label47.Size = new System.Drawing.Size(104, 20);
		this.Label47.TabIndex = 31;
		this.Label47.Text = "FromMinutes";
		this.Label47.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// txtFromDays
		// 
		this.txtFromDays.Location = new System.Drawing.Point(136, 32);
		this.txtFromDays.Name = "txtFromDays";
		this.txtFromDays.Size = new System.Drawing.Size(104, 20);
		this.txtFromDays.TabIndex = 34;
		this.txtFromDays.Text = "13.456";
		this.txtFromDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// lblFromDays
		// 
		this.lblFromDays.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblFromDays.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblFromDays.Location = new System.Drawing.Point(248, 32);
		this.lblFromDays.Name = "lblFromDays";
		this.lblFromDays.Size = new System.Drawing.Size(152, 23);
		this.lblFromDays.TabIndex = 40;
		this.lblFromDays.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label48
		// 
		this.Label48.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label48.Location = new System.Drawing.Point(24, 32);
		this.Label48.Name = "Label48";
		this.Label48.Size = new System.Drawing.Size(104, 20);
		this.Label48.TabIndex = 28;
		this.Label48.Text = "FromDays";
		this.Label48.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// TabPage5
		// 
		this.TabPage5.Controls.Add(this.lblZero);
		this.TabPage5.Controls.Add(this.Label22);
		this.TabPage5.Controls.Add(this.lblTicksPerSecond);
		this.TabPage5.Controls.Add(this.Label25);
		this.TabPage5.Controls.Add(this.lblTicksPerMinute);
		this.TabPage5.Controls.Add(this.Label27);
		this.TabPage5.Controls.Add(this.lblTicksPerMillisecond);
		this.TabPage5.Controls.Add(this.Label29);
		this.TabPage5.Controls.Add(this.lblTicksPerHour);
		this.TabPage5.Controls.Add(this.Label24);
		this.TabPage5.Controls.Add(this.lblTicksPerDay);
		this.TabPage5.Controls.Add(this.Label26);
		this.TabPage5.Controls.Add(this.lblMinValueTS);
		this.TabPage5.Controls.Add(this.Label30);
		this.TabPage5.Controls.Add(this.lblMaxValueTS);
		this.TabPage5.Controls.Add(this.Label32);
		this.TabPage5.Location = new System.Drawing.Point(4, 40);
		this.TabPage5.Name = "TabPage5";
		this.TabPage5.Size = new System.Drawing.Size(592, 260);
		this.TabPage5.TabIndex = 4;
		this.TabPage5.Text = "TimeSpan Fields";
		this.TabPage5.Visible = false;
		this.TabPage5.Click += new System.EventHandler(this.TabPage5_Click);
		// 
		// lblZero
		// 
		this.lblZero.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblZero.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblZero.Location = new System.Drawing.Point(136, 200);
		this.lblZero.Name = "lblZero";
		this.lblZero.Size = new System.Drawing.Size(200, 23);
		this.lblZero.TabIndex = 40;
		this.lblZero.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label22
		// 
		this.Label22.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label22.Location = new System.Drawing.Point(16, 200);
		this.Label22.Name = "Label22";
		this.Label22.Size = new System.Drawing.Size(112, 20);
		this.Label22.TabIndex = 39;
		this.Label22.Text = "Zero";
		this.Label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTicksPerSecond
		// 
		this.lblTicksPerSecond.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTicksPerSecond.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTicksPerSecond.Location = new System.Drawing.Point(136, 176);
		this.lblTicksPerSecond.Name = "lblTicksPerSecond";
		this.lblTicksPerSecond.Size = new System.Drawing.Size(200, 23);
		this.lblTicksPerSecond.TabIndex = 38;
		this.lblTicksPerSecond.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label25
		// 
		this.Label25.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label25.Location = new System.Drawing.Point(16, 176);
		this.Label25.Name = "Label25";
		this.Label25.Size = new System.Drawing.Size(112, 20);
		this.Label25.TabIndex = 37;
		this.Label25.Text = "TicksPerSecond";
		this.Label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTicksPerMinute
		// 
		this.lblTicksPerMinute.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTicksPerMinute.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTicksPerMinute.Location = new System.Drawing.Point(136, 152);
		this.lblTicksPerMinute.Name = "lblTicksPerMinute";
		this.lblTicksPerMinute.Size = new System.Drawing.Size(200, 23);
		this.lblTicksPerMinute.TabIndex = 36;
		this.lblTicksPerMinute.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label27
		// 
		this.Label27.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label27.Location = new System.Drawing.Point(16, 152);
		this.Label27.Name = "Label27";
		this.Label27.Size = new System.Drawing.Size(112, 20);
		this.Label27.TabIndex = 35;
		this.Label27.Text = "TicksPerMinute";
		this.Label27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTicksPerMillisecond
		// 
		this.lblTicksPerMillisecond.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTicksPerMillisecond.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTicksPerMillisecond.Location = new System.Drawing.Point(136, 128);
		this.lblTicksPerMillisecond.Name = "lblTicksPerMillisecond";
		this.lblTicksPerMillisecond.Size = new System.Drawing.Size(200, 23);
		this.lblTicksPerMillisecond.TabIndex = 34;
		this.lblTicksPerMillisecond.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label29
		// 
		this.Label29.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label29.Location = new System.Drawing.Point(16, 128);
		this.Label29.Name = "Label29";
		this.Label29.Size = new System.Drawing.Size(112, 20);
		this.Label29.TabIndex = 33;
		this.Label29.Text = "TicksPerMillisecond";
		this.Label29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTicksPerHour
		// 
		this.lblTicksPerHour.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTicksPerHour.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTicksPerHour.Location = new System.Drawing.Point(136, 104);
		this.lblTicksPerHour.Name = "lblTicksPerHour";
		this.lblTicksPerHour.Size = new System.Drawing.Size(200, 23);
		this.lblTicksPerHour.TabIndex = 32;
		this.lblTicksPerHour.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label24
		// 
		this.Label24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label24.Location = new System.Drawing.Point(16, 104);
		this.Label24.Name = "Label24";
		this.Label24.Size = new System.Drawing.Size(112, 20);
		this.Label24.TabIndex = 31;
		this.Label24.Text = "TicksPerHour";
		this.Label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblTicksPerDay
		// 
		this.lblTicksPerDay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblTicksPerDay.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblTicksPerDay.Location = new System.Drawing.Point(136, 80);
		this.lblTicksPerDay.Name = "lblTicksPerDay";
		this.lblTicksPerDay.Size = new System.Drawing.Size(200, 23);
		this.lblTicksPerDay.TabIndex = 30;
		this.lblTicksPerDay.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label26
		// 
		this.Label26.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label26.Location = new System.Drawing.Point(16, 80);
		this.Label26.Name = "Label26";
		this.Label26.Size = new System.Drawing.Size(112, 20);
		this.Label26.TabIndex = 29;
		this.Label26.Text = "TicksPerDay";
		this.Label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblMinValueTS
		// 
		this.lblMinValueTS.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMinValueTS.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMinValueTS.Location = new System.Drawing.Point(136, 56);
		this.lblMinValueTS.Name = "lblMinValueTS";
		this.lblMinValueTS.Size = new System.Drawing.Size(200, 23);
		this.lblMinValueTS.TabIndex = 28;
		this.lblMinValueTS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label30
		// 
		this.Label30.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label30.Location = new System.Drawing.Point(16, 56);
		this.Label30.Name = "Label30";
		this.Label30.Size = new System.Drawing.Size(112, 20);
		this.Label30.TabIndex = 27;
		this.Label30.Text = "MinValue";
		this.Label30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// lblMaxValueTS
		// 
		this.lblMaxValueTS.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblMaxValueTS.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.lblMaxValueTS.Location = new System.Drawing.Point(136, 32);
		this.lblMaxValueTS.Name = "lblMaxValueTS";
		this.lblMaxValueTS.Size = new System.Drawing.Size(200, 23);
		this.lblMaxValueTS.TabIndex = 26;
		this.lblMaxValueTS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// Label32
		// 
		this.Label32.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.Label32.Location = new System.Drawing.Point(16, 32);
		this.Label32.Name = "Label32";
		this.Label32.Size = new System.Drawing.Size(112, 20);
		this.Label32.TabIndex = 25;
		this.Label32.Text = "MaxValue";
		this.Label32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		// 
		// frmMain
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(618, 324);
		this.Controls.Add(this.TabControl1);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.MaximizeBox = false;
		this.Menu = this.mnuMain;
		this.Name = "frmMain";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Title Comes from Assembly Info";
		this.TabControl1.ResumeLayout(false);
		this.TabPage1.ResumeLayout(false);
		this.TabPage4.ResumeLayout(false);
		this.TabPage2.ResumeLayout(false);
		this.TabPage3.ResumeLayout(false);
		this.TabPage6.ResumeLayout(false);
		this.tabTimeSpan.ResumeLayout(false);
		this.TabPage9.ResumeLayout(false);
		this.TabPage10.ResumeLayout(false);
		this.TabPage7.ResumeLayout(false);
		this.TabPage5.ResumeLayout(false);
		this.ResumeLayout(false);
		this.Load+=new EventHandler(frmMain_Load);

	}

#endregion

#region " Standard Menu Code "

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new frmMain());
	}
	

	

	// This code simply shows the About form.

	private void mnuAbout_Click(object sender, System.EventArgs e) 
	{

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

	

	

	private void frmMain_Load(object sender, System.EventArgs e) {

		// So that we only need to set the title of the application once,

		// we use the AssemblyInfo class (defined in the AssemblyInfo.cs file)

		// to read the AssemblyTitle attribute.

		AssemblyInfo ainfo = new AssemblyInfo();

		this.Text = ainfo.Title;

		this.mnuAbout.Text = string.Format("&About {0} ...", ainfo.Title);

		// Call procedures that load values onto the form.

		LoadstaticMembers();

		LoadProperties();

		LoadConversionMethods();

		LoadCalculationMethods();

		LoadTimeSpanFields();

		LoadTSMethods();

	}

#endregion

	private void btnCalcParse_Click(object sender, System.EventArgs e) 
	{
		// Display TimeSpan properties given a string containing

		// a TimeSpan value to parse.

		try 
		{

			// No need to use an explicit constructor

			// unless you need to specify a value

			// at the time the instance gets created.

			TimeSpan ts;

			ts = TimeSpan.Parse(txtParse.Text);

			// Display the properties of the TimeSpan

			// instance you've created.

			DisplayTSProperties(ts);
		}
		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void btnRefreshCalculation_Click(object sender, System.EventArgs e)
	{

		LoadCalculationMethods();

	}

	private void btnRefreshConversion_Click(object sender, System.EventArgs e)
	{

		LoadConversionMethods();

	}

	private void btnRefreshProperties_Click(object sender, System.EventArgs e)
	{

		LoadProperties();

	}

	private void btnRefreshstatic_Click(object sender, System.EventArgs e)
	{

		LoadstaticMembers();

	}

	private void btnRefreshTSMethods_Click(object sender, System.EventArgs e)
	{

		LoadTSMethods();

	}

	private void btnRefreshTSProperties_Click(object sender, System.EventArgs e) 
	{

		try 
		{

			// Create a TimeSpan instance based on 

			// DateTime values provided on the form.

			TimeSpan ts;

			DateTime dtStart;

			DateTime dtEnd;

			// Parse the text from the text boxes.

			dtStart = DateTime.Parse(txtStart.Text);

			dtEnd = DateTime.Parse(txtEnd.Text);

			ts = dtEnd.Subtract(dtStart).Duration();

			// Display the properties of the TimeSpan

			// instance you've created.

			DisplayTSProperties(ts);
		}

		catch (Exception Exp)
		{

			MessageBox.Show(Exp.Message, this.Text);

		}

	}

	private void tabTimeSpan_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		// Clear out all the items on the page.

		// This is the only page that appears

		// empty until you refresh the data. 

		lblDays.Text = string.Empty;

		lblHours.Text = string.Empty;

		lblMilliseconds.Text = string.Empty;

		lblMinutes.Text = string.Empty;

		lblSeconds.Text = string.Empty;

		lblTimeSpanTicks.Text = string.Empty;

		lblTotalDays.Text = string.Empty;

		lblTotalHours.Text = string.Empty;

		lblTotalMilliseconds.Text = string.Empty;

		lblTotalMinutes.Text = string.Empty;

		lblTotalSeconds.Text = string.Empty;

	}

	private void DisplayTSProperties(TimeSpan ts)
	{

		// Use instance properties of the TimeSpan type.

		// Demonstrates:

		//  TimeSpan.Days

		//  TimeSpan.Hours

		//  TimeSpan.Milliseconds

		//  TimeSpan.Minutes

		//  TimeSpan.Seconds

		//  TimeSpan.Ticks

		//  TimeSpan.TotalDays

		//  TimeSpan.TotalHours

		//  TimeSpan.TotalMilliseconds

		//  TimeSpan.TotalMinutes

		//  TimeSpan.TotalSeconds

		try 
		{

			lblDays.Text = ts.Days.ToString();

			lblHours.Text = ts.Hours.ToString();

			lblMilliseconds.Text = ts.Milliseconds.ToString();

			lblMinutes.Text = ts.Minutes.ToString();

			lblSeconds.Text = ts.Seconds.ToString();

			lblTimeSpanTicks.Text = ts.Ticks.ToString();

			lblTotalDays.Text = ts.TotalDays.ToString();

			lblTotalHours.Text = ts.TotalHours.ToString();

			lblTotalMilliseconds.Text = ts.TotalMilliseconds.ToString();

			lblTotalMinutes.Text = ts.TotalMinutes.ToString();

			lblTotalSeconds.Text = ts.TotalSeconds.ToString();
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void LoadCalculationMethods()
	{

		// Use instance methods of the DateTime type.

		// Demonstrates:

		//  DateTime.AddDays

		//  DateTime.AddHours

		//  DateTime.AddMilliseconds

		//  DateTime.AddMinutes

		//  DateTime.AddMonths

		//  DateTime.AddSeconds

		//  DateTime.AddTicks

		//  DateTime.AddYears

		try 
		{

			DateTime dt = DateTime.Now;

			lblNow3.Text = dt.ToString();

			lblAddDays.Text = dt.AddDays(Convert.ToDouble(txtDays.Text)).ToString();

			lblAddHours.Text = dt.AddHours(Convert.ToDouble(txtHours.Text)).ToString();

			lblAddMilliseconds.Text = dt.AddMilliseconds(Convert.ToDouble(txtMilliseconds.Text)).ToString();

			lblAddMinutes.Text = dt.AddMinutes(Convert.ToDouble(txtMinutes.Text)).ToString();

			lblAddMonths.Text = dt.AddMonths(Convert.ToInt32(txtMonths.Text)).ToString();

			lblAddSeconds.Text = dt.AddSeconds(Convert.ToDouble(txtSeconds.Text)).ToString();

			lblAddTicks.Text = dt.AddTicks(Convert.ToInt64(txtTicks.Text)).ToString();

			lblAddYears.Text = dt.AddYears(Convert.ToInt32(txtYears.Text)).ToString();
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void LoadConversionMethods()
	{

		// Use instance methods of the DateTime type.

		// Demonstrates:

		//  DateTime.ToFileTime

		//  DateTime.ToLocalTime

		//  DateTime.ToLongDatestring

		//  DateTime.ToLongTimestring

		//  DateTime.ToOADate

		//  DateTime.ToShortDatestring

		//  DateTime.ToShortTimestring

		//  DateTime.Tostring

		//  DateTime.ToUniversalTime

		try 
		{

			DateTime dt = DateTime.Now;

			lblNow2.Text = dt.ToString();

			//lblToFileTime.Text = dt.ToFileTime.ToString();
			lblToFileTime.Text = Convert.ToString(dt.ToFileTime());

			//lblToLocalTime.Text = dt.ToLocalTime.ToString();
			lblToLocalTime.Text = Convert.ToString(dt.ToLocalTime());
			
			//lblToLongDatestring.Text = dt.ToLongDatestring;
			lblToLongDatestring.Text = Convert.ToString(dt.ToLongDateString());
			
			//lblToLongTimestring.Text = dt.ToLongTimestring;
			lblToLongTimestring.Text = Convert.ToString(dt.ToLongTimeString());

			//lblToOADate.Text = dt.ToOADate;
			lblToOADate.Text = Convert.ToString(dt.ToOADate());

			//lblToShortDatestring.Text = dt.ToShortDatestring;
			lblToShortDatestring.Text = Convert.ToString(dt.ToShortDateString());

			//lblToShortTimestring.Text = dt.ToShortTimestring;
			lblToShortTimestring.Text = Convert.ToString(dt.ToShortTimeString());

			lblTostring.Text = dt.ToString();

			//lblToUniversalTime.Text = dt.ToUniversalTime.ToString();
			lblToUniversalTime.Text = Convert.ToString(dt.ToUniversalTime());
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void LoadProperties()
	{

		// Use instance properties of the 

		// the DateTime type.

		// Demonstrates:

		//  DateTime.Now

		//  DateTime.Date

		//  DateTime.Day

		//  DateTime.DayOfYear

		//  DateTime.Hour

		//  DateTime.Millisecond

		//  DateTime.DayOfWeek

		//  DateTime.Minute

		//  DateTime.Month

		//  DateTime.Second

		//  DateTime.Ticks

		//  DateTime.TimeOfDay

		//  DateTime.Year

		try 
		{

			// No need to use an explicit constructor

			// unless you need to specify a value

			// at the time the instance gets created.

			DateTime dt = DateTime.Now;

			lblNow1.Text = dt.ToString();

			lblDate.Text = dt.Date.ToString();

			lblDay.Text = dt.Day.ToString();

			lblDayOfYear.Text = dt.DayOfYear.ToString();

			lblHour.Text = dt.Hour.ToString();

			lblMillisecond.Text = dt.Millisecond.ToString();

			lblDayOfWeek.Text = dt.DayOfWeek.ToString();

			lblMinute.Text = dt.Minute.ToString();

			lblMonth.Text = dt.Month.ToString();

			lblSecond.Text = dt.Second.ToString();

			lblTicks.Text = dt.Ticks.ToString();

			lblTimeOfDay.Text = dt.TimeOfDay.ToString();

			lblYear.Text = dt.Year.ToString();
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void LoadstaticMembers()
	{

		// Use shared members of the DateTime type.

		// Demonstrates these properties:

		//  DateTime.Now

		//  DateTime.UtcNow

		//  DateTime.MinValue

		//  DateTime.MaxValue

		// Demonstrates these methods:

		//  DateTime.FromOADate

		//  DateTime.IsLeapYear

		//  DateTime.DaysInMonth

		try 
		{

			lblNow.Text = DateTime.Now.ToString();

			lblToday.Text = DateTime.Today.ToString();

			lblUtcNow.Text = DateTime.UtcNow.ToString();

			lblMinValue.Text = DateTime.MinValue.ToString();

			lblMaxValue.Text = DateTime.MaxValue.ToString();

			lblFromOADate.Text = DateTime.FromOADate(Convert.ToDouble(txtFromOADate.Text)).ToString();

			lblIsLeapYear.Text = DateTime.IsLeapYear(Convert.ToInt32(txtIsLeapYear.Text)).ToString();

			lblDaysInMonth.Text = DateTime.DaysInMonth(Convert.ToInt32(txtYear.Text), Convert.ToInt32(txtMonth.Text)).ToString();
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void LoadTimeSpanFields()
	{

		// Use shared fields provided by TimeSpan type.

		// Demonstrates:

		//  TimeSpan.MaxValue

		//  TimeSpan.Minvalue

		//  TimeSpan.TicksPerDay

		//  TimeSpan.TicksPerHour

		//  TimeSpan.TicksPerMillisecond

		//  TimeSpan.TicksPerMinute

		//  TimeSpan.TicksPerSecond

		//  TimeSpan.Zero

		try 
		{

			lblMaxValueTS.Text = TimeSpan.MaxValue.ToString();

			lblMinValueTS.Text = TimeSpan.MinValue.ToString();

			lblTicksPerDay.Text = TimeSpan.TicksPerDay.ToString();

			lblTicksPerHour.Text = TimeSpan.TicksPerHour.ToString();

			lblTicksPerMillisecond.Text = TimeSpan.TicksPerMillisecond.ToString();

			lblTicksPerMinute.Text = TimeSpan.TicksPerMinute.ToString();

			lblTicksPerSecond.Text = TimeSpan.TicksPerSecond.ToString();

			lblZero.Text = TimeSpan.Zero.ToString();
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void LoadTSMethods()
	{

		// Use shared methods of the TimeSpan type.

		// Demonstrates:

		//  TimeSpan.FromDays

		//  TimeSpan.FromHours

		//  TimeSpan.FromMilliseconds

		//  TimeSpan.FromMinutes

		//  TimeSpan.FromSeconds

		//  TimeSpan.FromTicks

		try 
		{

			lblFromDays.Text = TimeSpan.FromDays(Convert.ToDouble(txtFromDays.Text)).ToString();

			lblFromHours.Text = TimeSpan.FromHours(Convert.ToDouble(txtFromHours.Text)).ToString();

			lblFromMilliseconds.Text = TimeSpan.FromMilliseconds(Convert.ToDouble(txtFromMilliseconds.Text)).ToString();

			lblFromMinutes.Text = TimeSpan.FromMinutes(Convert.ToDouble(txtFromMinutes.Text)).ToString();

			lblFromSeconds.Text = TimeSpan.FromSeconds(Convert.ToDouble(txtFromSeconds.Text)).ToString();

			lblFromTicks.Text = TimeSpan.FromTicks(Convert.ToInt64(txtFromTicks.Text)).ToString();
		}

		catch (Exception exp) 
		{

			MessageBox.Show(exp.Message, this.Text);

		}

	}

	private void TabPage5_Click(object sender, System.EventArgs e)
	{
	
	}

}

