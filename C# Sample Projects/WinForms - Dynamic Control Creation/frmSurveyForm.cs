using System;
using System.Windows.Forms;

// This class represents a Survey form.  However, all of the 
//   controls are added dynamically in code.

public class frmSurveyForm: System.Windows.Forms.Form {

    // Create necessary private variables to hold information.

    private string m_Title = "Survey";
    private string m_SurveyResponse = "Survey ! Completed";

#region " Windows Form Designer generated code "

    public frmSurveyForm () {

        //This call is required by the Windows Form Designer.
        InitializeComponent();
        //Add any initialization after the InitializeComponent() call
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

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;

    private void InitializeComponent() 
	{
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmSurveyForm));
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        //btnOK
        //
        this.btnOK.AccessibleDescription = resources.GetString("btnOK.AccessibleDescription");
        this.btnOK.AccessibleName = resources.GetString("btnOK.AccessibleName");
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
        //btnCancel
        //
        this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
        this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
        this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor"));
        this.btnCancel.BackgroundImage = (System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage"));
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Dock = (System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock"));
        this.btnCancel.Enabled = (bool)(resources.GetObject("btnCancel.Enabled"));
        this.btnCancel.FlatStyle = (System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle"));
        this.btnCancel.Font = (System.Drawing.Font)(resources.GetObject("btnCancel.Font"));
        this.btnCancel.Image = (System.Drawing.Image)(resources.GetObject("btnCancel.Image"));
        this.btnCancel.ImageAlign = (System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign"));
        this.btnCancel.ImageIndex = (int)(resources.GetObject("btnCancel.ImageIndex"));
        this.btnCancel.ImeMode = (System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode"));
        this.btnCancel.Location = (System.Drawing.Point)(resources.GetObject("btnCancel.Location"));
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.RightToLeft = (System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft"));
        this.btnCancel.Size = (System.Drawing.Size)(resources.GetObject("btnCancel.Size"));
        this.btnCancel.TabIndex = (int)(resources.GetObject("btnCancel.TabIndex"));
        this.btnCancel.Text = resources.GetString("btnCancel.Text");
        this.btnCancel.TextAlign = (System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign"));
        this.btnCancel.Visible = (bool)(resources.GetObject("btnCancel.Visible"));
        //
        //frmSurveyForm
        //
        this.AccessibleDescription = (string) resources.GetObject("$this.AccessibleDescription");
        this.AccessibleName = (string) resources.GetObject("$this.AccessibleName");
        this.Anchor = (System.Windows.Forms.AnchorStyles) resources.GetObject("$this.Anchor");
        this.AutoScaleBaseSize = (System.Drawing.Size) resources.GetObject("$this.AutoScaleBaseSize");
        this.AutoScroll = (bool) resources.GetObject("$this.AutoScroll");
        this.AutoScrollMargin = (System.Drawing.Size) resources.GetObject("$this.AutoScrollMargin");
        this.AutoScrollMinSize = (System.Drawing.Size) resources.GetObject("$this.AutoScrollMinSize");
        this.BackgroundImage = (System.Drawing.Image) resources.GetObject("$this.BackgroundImage");
        this.ClientSize = (System.Drawing.Size) resources.GetObject("$this.ClientSize");
		this.Controls.AddRange(new System.Windows.Forms.Control[] {this.btnCancel, this.btnOK});
        this.Dock = (System.Windows.Forms.DockStyle) resources.GetObject("$this.Dock");
        this.Enabled = (bool) resources.GetObject("$this.Enabled");
        this.Font = (System.Drawing.Font) resources.GetObject("$this.Font");
        this.Icon = (System.Drawing.Icon) resources.GetObject("$this.Icon");
        this.ImeMode = (System.Windows.Forms.ImeMode) resources.GetObject("$this.ImeMode");
        this.Location = (System.Drawing.Point) resources.GetObject("$this.Location");
        this.MaximumSize = (System.Drawing.Size) resources.GetObject("$this.MaximumSize");
        this.MinimumSize = (System.Drawing.Size) resources.GetObject("$this.MinimumSize");
        this.Name = "frmSurveyForm";
        this.RightToLeft = (System.Windows.Forms.RightToLeft) resources.GetObject("$this.RightToLeft");
        this.StartPosition = (System.Windows.Forms.FormStartPosition) resources.GetObject("$this.StartPosition");
        this.Text = resources.GetString("$this.Text");
        this.Visible = (bool) resources.GetObject("$this.Visible");
        this.ResumeLayout(false);
    }
#endregion

    // Create a property so the controls can be easily retrieved.

    public Control.ControlCollection SurveyFormControls
	{
        get {
            return this.Controls;
        }
    }

    // Create a height property so height can be changed easily.

    public int SurveyHeight 
		{
        get {
            return this.Height;
        }
        set
		{
            this.Height = value;
        }
    }

    // Create a property so the response can be easily retrieved.

    public string SurveyResponse
	{
        get {
            return m_SurveyResponse;
        }
    }

    // Create a property so the title of the form can be easily

    //   retrieved and set.

    public string SurveyTitle
	{
        get {
            return m_Title;
        }

        set
		{
            m_Title = value;
            this.Text = m_Title;
        }

    }

    // Create a width property so width can be changed easily.

    public int SurveyWidth
	{
        get {
            return this.Width;
        }

        set
		{
            this.Width = value;
        }
    }

    // This simply resets the SurveyResponse string and closes the form.

    private void btnCancel_Click(object sender, System.EventArgs e)
		{

        // Reset the SurveyResponse string.
        m_SurveyResponse = "Survey ! Completed";
        // Close the form.
        this.Close();
    }

    // This button first fills out the SurveyResponse string then closes the form.

    private void btnOK_Click(object sender, System.EventArgs e)
	{
        // Create the controls we'll be using in all of the later loops.
        
        // Reset the Response string
        this.m_SurveyResponse = "";
        // Loop through each control and output the user response into the
        //   the SurveyResponse string. (The string could easily be replaced
        //   with a collection of some sort.)
        foreach(Control myControl in this.Controls)
		{
            // Differentiate output based on the type of the control
            switch (myControl.GetType().ToString())
			{
                case "ComboBox":
                    // Simple get the text out of the group box and add it to the
                    //   response string, along with the question name
                    m_SurveyResponse += myControl.Name + " - ";
                    m_SurveyResponse += myControl.Text;
                    m_SurveyResponse += Environment.NewLine;
					break;
                case "TextBox":
                    // Simple get the text out of the group box and add it to the
                    //   response string, along with the question name
                    m_SurveyResponse += myControl.Name + " - ";
                    m_SurveyResponse += myControl.Text;
                    m_SurveyResponse += Environment.NewLine;
					break;
                case "GroupBox":
                    // Need to go inside of the GroupBox to yank out the 
                    //   RadioButtons
                    foreach(Control myGroupControl in ((GroupBox)(myControl)).Controls)
					{
						RadioButton rb = myGroupControl as RadioButton;
                        if (rb != null) 
						{
                            if (rb.Checked) 
							{
                                // Simple get the question and response of the 
                                //   user being surveyed.
                                m_SurveyResponse += myControl.Name + " - ";
                                m_SurveyResponse += myGroupControl.Text;
                                m_SurveyResponse += Environment.NewLine;
                            }
                        }
                    }
					break;
                case "ListBox":
                    // For this one we must get each of the selected lines, and 
                    //   return them.
                    m_SurveyResponse += myControl.Name + " - ";
                    foreach(Object myObject in ((ListBox)(myControl)).SelectedItems)
					{
						string thisString = myObject as string;
                        if (thisString != null) 
						{
                            // Simple get the question and response of the 
                            //   user being surveyed.
                            m_SurveyResponse += "\t" + Convert.ToString(myObject);
                            m_SurveyResponse += Environment.NewLine;
                        }
                    }
					break;
            }

        }

        // Close the form.

        this.Close();

    }

}

