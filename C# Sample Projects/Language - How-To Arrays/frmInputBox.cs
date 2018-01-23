using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

/// <summary>
/// Summary description for frmInputBox.
/// </summary>
public class frmInputBox : System.Windows.Forms.Form
{
	private string _title = "Title";
	private string _prompt = "Please enter in a value:";
	private string _inputValue = string.Empty;
	
	private System.Windows.Forms.Label lblPrompt;
	private System.Windows.Forms.TextBox txtInput;
	private System.Windows.Forms.Button cmdOK;
	private System.Windows.Forms.Button cmdCancel;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public frmInputBox()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		SetValues();
	}

	public frmInputBox(string prompt, string title, string inputValue)
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	
		this._title = title;
		this._prompt = prompt;
		this._inputValue = inputValue;

		SetValues();
	}

	private void SetValues()
	{
		this.Text = this._title;
		this.lblPrompt.Text = this._prompt;
		this.txtInput.Text = this._inputValue;
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if(components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}

	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.lblPrompt = new System.Windows.Forms.Label();
		this.txtInput = new System.Windows.Forms.TextBox();
		this.cmdOK = new System.Windows.Forms.Button();
		this.cmdCancel = new System.Windows.Forms.Button();
		this.SuspendLayout();
		// 
		// lblPrompt
		// 
		this.lblPrompt.Location = new System.Drawing.Point(8, 8);
		this.lblPrompt.Name = "lblPrompt";
		this.lblPrompt.Size = new System.Drawing.Size(344, 56);
		this.lblPrompt.TabIndex = 0;
		this.lblPrompt.Text = "Please enter in a value:";
		// 
		// txtInput
		// 
		this.txtInput.Location = new System.Drawing.Point(8, 80);
		this.txtInput.Name = "txtInput";
		this.txtInput.Size = new System.Drawing.Size(416, 20);
		this.txtInput.TabIndex = 1;
		this.txtInput.Text = "";
		// 
		// cmdOK
		// 
		this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.cmdOK.Location = new System.Drawing.Point(360, 8);
		this.cmdOK.Name = "cmdOK";
		this.cmdOK.TabIndex = 2;
		this.cmdOK.Text = "OK";
		this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
		// 
		// cmdCancel
		// 
		this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cmdCancel.Location = new System.Drawing.Point(360, 40);
		this.cmdCancel.Name = "cmdCancel";
		this.cmdCancel.TabIndex = 3;
		this.cmdCancel.Text = "Cancel";
		this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
		// 
		// frmInputBox
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(440, 109);
		this.Controls.Add(this.cmdCancel);
		this.Controls.Add(this.cmdOK);
		this.Controls.Add(this.txtInput);
		this.Controls.Add(this.lblPrompt);
		this.KeyPreview = true;
		this.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(448, 136);
		this.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(448, 136);
		this.Name = "frmInputBox";
		this.ShowInTaskbar = false;
		this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Title";
		this.Activated += new System.EventHandler(this.frmInputBox_Activated);
		this.ResumeLayout(false);
		this.KeyDown += new KeyEventHandler(frmInputBox_KeyDown);

	}
	#endregion

	private void frmInputBox_Activated(object sender, EventArgs e)
	{
		this.txtInput.Focus();
		this.txtInput.SelectAll();
	}

	private void cmdOK_Click(object sender, System.EventArgs e)
	{
		this._inputValue = txtInput.Text;
		this.Close();
	}

	private void cmdCancel_Click(object sender, System.EventArgs e)
	{
		this.Close();
	}
	
	public string InputValue
	{
		get { return this._inputValue; }
		set { this._inputValue = value; }
	}
	
	public string Title
	{
		get { return this._title; }
		set { this._title = value; }
	}

	public string Prompt
	{
		get { return this._prompt; }
		set { this._prompt = value; }
	}

	public static string InputBox(string prompt, string title, string inputValue)
	{
		string retVal = string.Empty;

		frmInputBox frm = new frmInputBox(prompt, title, inputValue);
		if(frm.ShowDialog() == DialogResult.OK)
			retVal = frm.InputValue;
		
		frm.Dispose();

		return retVal;
	}

	private void frmInputBox_KeyDown(object sender, KeyEventArgs e)
	{
		if(e.KeyData == Keys.Enter)
			cmdOK_Click(sender, EventArgs.Empty);
		else if(e.KeyData == Keys.Escape)
			cmdCancel_Click(sender, EventArgs.Empty);
	}
}
