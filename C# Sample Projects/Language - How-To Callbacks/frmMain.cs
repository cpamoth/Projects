//Copyright (C) 2002 Microsoft Corporation
//All rights reserved.
//THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
//EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
//MERCHANTIBILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//Requires the Trial or Release version of Visual Studio .NET Professional (or greater).

using System;
using System.Windows.Forms;

public class frmMain: System.Windows.Forms.Form, ICallback
{
	//Will be used in the Interface-based callback
    
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
	protected override void Dispose(bool disposing) 
	{
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
	private System.Windows.Forms.Button cmdBuiltInCallback;

	private System.Windows.Forms.Button cmdAsyncDelegateCallback;

    private System.Windows.Forms.Button cmdDelegateCallback;

    private System.Windows.Forms.Button cmdInterfaceCallback;

	private System.Windows.Forms.GroupBox grpOptions;

	private void InitializeComponent() 
	{
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));

		this.mnuMain = new System.Windows.Forms.MainMenu();

		this.mnuFile = new System.Windows.Forms.MenuItem();

		this.mnuExit = new System.Windows.Forms.MenuItem();

		this.mnuHelp = new System.Windows.Forms.MenuItem();

		this.mnuAbout = new System.Windows.Forms.MenuItem();

		this.grpOptions = new System.Windows.Forms.GroupBox();

		this.cmdBuiltInCallback = new System.Windows.Forms.Button();

		this.cmdAsyncDelegateCallback = new System.Windows.Forms.Button();

		this.cmdDelegateCallback = new System.Windows.Forms.Button();

		this.cmdInterfaceCallback = new System.Windows.Forms.Button();

		this.grpOptions.SuspendLayout();

		this.SuspendLayout();

		//

		//mnuMain

		//

		this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuFile, this.mnuHelp});

		//

		//mnuFile

		//

		this.mnuFile.Index = 0;

		this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuExit});

		this.mnuFile.Text = "&File";

		//

		//mnuExit

		//

		this.mnuExit.Index = 0;

		this.mnuExit.Text = "E&xit";
		this.mnuExit.Click += new EventHandler(mnuExit_Click);

		//

		//mnuHelp

		//

		this.mnuHelp.Index = 1;

		this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.mnuAbout});

		this.mnuHelp.Text = "&Help";

		//

		//mnuAbout

		//

		this.mnuAbout.Index = 0;

		this.mnuAbout.Text = "Text Comes from AssemblyInfo";
		this.mnuAbout.Click += new EventHandler(mnuAbout_Click);
		//

		//grpOptions

		//

		this.grpOptions.Controls.AddRange(new System.Windows.Forms.Control[] {this.cmdBuiltInCallback, this.cmdAsyncDelegateCallback, this.cmdDelegateCallback, this.cmdInterfaceCallback});

		this.grpOptions.Location = new System.Drawing.Point(16, 16);

		this.grpOptions.Name = "grpOptions";

		this.grpOptions.Size = new System.Drawing.Size(232, 152);

		this.grpOptions.TabIndex = 8;

		this.grpOptions.TabStop = false;

		//

		//cmdBuiltInCallback

		//

		this.cmdBuiltInCallback.ImeMode = System.Windows.Forms.ImeMode.NoControl;

		this.cmdBuiltInCallback.Location = new System.Drawing.Point(16, 113);

		this.cmdBuiltInCallback.Name = "cmdBuiltInCallback";

		this.cmdBuiltInCallback.Size = new System.Drawing.Size(200, 23);

		this.cmdBuiltInCallback.TabIndex = 3;

		this.cmdBuiltInCallback.Text = "&Built-in async callback Delegate";
		this.cmdBuiltInCallback.Click += new EventHandler(cmdBuiltInCallback_Click);
		//

		//cmdAsyncDelegateCallback

		//

		this.cmdAsyncDelegateCallback.ImeMode = System.Windows.Forms.ImeMode.NoControl;

		this.cmdAsyncDelegateCallback.Location = new System.Drawing.Point(16, 81);

		this.cmdAsyncDelegateCallback.Name = "cmdAsyncDelegateCallback";

		this.cmdAsyncDelegateCallback.Size = new System.Drawing.Size(200, 23);

		this.cmdAsyncDelegateCallback.TabIndex = 2;

		this.cmdAsyncDelegateCallback.Text = "&Async callback using a Delegate";
		this.cmdAsyncDelegateCallback.Click += new EventHandler(cmdAsyncDelegateCallback_Click);
		//

		//cmdDelegateCallback

		//

		this.cmdDelegateCallback.ImeMode = System.Windows.Forms.ImeMode.NoControl;

		this.cmdDelegateCallback.Location = new System.Drawing.Point(16, 49);

		this.cmdDelegateCallback.Name = "cmdDelegateCallback";

		this.cmdDelegateCallback.Size = new System.Drawing.Size(200, 23);

		this.cmdDelegateCallback.TabIndex = 1;

		this.cmdDelegateCallback.Text = "Callback using a &Delegate";
		this.cmdDelegateCallback.Click += new EventHandler(cmdDelegateCallback_Click);
		//

		//cmdInterfaceCallback

		//

		this.cmdInterfaceCallback.ImeMode = System.Windows.Forms.ImeMode.NoControl;

		this.cmdInterfaceCallback.Location = new System.Drawing.Point(16, 17);

		this.cmdInterfaceCallback.Name = "cmdInterfaceCallback";

		this.cmdInterfaceCallback.Size = new System.Drawing.Size(200, 23);

		this.cmdInterfaceCallback.TabIndex = 0;

		this.cmdInterfaceCallback.Text = "Callback using an &Interface";
		this.cmdInterfaceCallback.Click += new EventHandler(cmdInterfaceCallback_Click);
		//

		//frmMain

		//

		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);

		this.ClientSize = new System.Drawing.Size(266, 179);

		this.Controls.AddRange(new System.Windows.Forms.Control[] {this.grpOptions});

		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

		this.Icon = (System.Drawing.Icon) resources.GetObject("$this.Icon");

		this.MaximizeBox = false;

		this.Menu = this.mnuMain;

		this.Name = "frmMain";

		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

		this.Text = "Title Comes from Assembly Info";

		this.grpOptions.ResumeLayout(false);

		this.ResumeLayout(false);
	}
	#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new frmMain());
	}

#region " Standard Menu Code "

	

	

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

    public void CallbackMethod()
	{
        //This method is called from Class1 after the form is registered
        //with the instance. 

		MessageBox.Show("In the callback method", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void cmdInterfaceCallback_Click(object sender, System.EventArgs e) 
	{
        //This method calls into a method of a Class1 instance, which in turn 
        //calls back into the client via ICallback.
        Class1 refClass1 = new Class1();

        //Register the client, passing a reference to itself
        refClass1.RegisterInterFaceForCallback(this);

        //Call the method which will in turn call back into the client
        refClass1.UseInterfaceCallback();

        //Unregister the client
        refClass1.UnRegisterInterfaceForCallback();
    }

    private void cmdDelegateCallback_Click(object sender, System.EventArgs e)
	{
        //This method calls into a method of a Class1 instance, which in turn 
        //calls back into the client via a delegate.
        Class1 refClass1 = new Class1();

        //Create an instance of a delegate to represent the callback method
        Delegate1 d = new Delegate1(this.CallbackMethod);

        //Register the client, passing a reference to the delegate instance
        refClass1.RegisterDelegateForCallback(d);

        //Call the method which will in turn call back into the client
        refClass1.UseDelegateCallback();

        //Unregister the client
        refClass1.UnRegisterDelegateForCallback();
    }

    private void cmdAsyncDelegateCallback_Click(object sender, System.EventArgs e)
	{
        //This method calls into a method of a Class1 instance, which in turn 
        //calls back asynchronously into the client via a delegate. Note that 
        //the registration calls are identical, whether the callback will be
        //asynchronous or not.

        Class1 refClass1 = new Class1();

        //Create an instance of a delegate to represent the callback method
        Delegate1 d = new Delegate1(this.CallbackMethod);

        //Register the client, passing a reference to the delegate instance
        refClass1.RegisterDelegateForCallback(d);

        //Call the method which will in turn call back asynchronously into the client
        refClass1.UseAsyncDelegateCallback();

        //Unregister the client
        refClass1.UnRegisterDelegateForCallback();
    }

    private void cmdBuiltInCallback_Click(object sender, System.EventArgs e)
	{
		//Delegates have a built-in mechanism to call back into the client. The 
        //method to be called back into must have a specific signature, however.
        //This method will use the built-in callback of an asynchronous invocation
        //on a delegate, and will call into the BuiltInCallback method below this one.
        //Note that no user registration is needed.
        Class1 refClass1 = new Class1();

        //Create an instance of a delegate to represent the callback method
        Delegate1 d = new Delegate1(refClass1.UseBuiltInDelegateCallback);

		//Now create a second delegate to pass to the callee. The callee will use
        //This second delegate to call back into the client. AsyncCallback is 
        //defined in the CLR specifically for calling back after an asynchronous
        //invocation of a delegate.
        AsyncCallback ac = new AsyncCallback(this.BuiltInCallback);

		//Invoke the class one method. Note that this invocation is asynchronous,
        //and will be carried out on a worker thread from the CLR thread pool. The
        //resulting callback will also be performed by the worker thread. The
        //callback delegate is passed in as an argument, so no explicit registration
        //is needed.
        d.BeginInvoke(ac, null);
    }

    private void BuiltInCallback(IAsyncResult ia)
	{
        //This method is called using the AsyncCallback delegate that is passed into
        //the BeginInvoke method in the BuiltInCallback routine above. Note that the 
        //signature includes an IAsyncResult, which associates the callback with a 
        //specific asynchronous invocation.
		MessageBox.Show("In the BuiltInCallback routine", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}