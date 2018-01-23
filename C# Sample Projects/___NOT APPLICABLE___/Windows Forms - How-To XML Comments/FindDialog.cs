//*****************************************************************************
// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.
//*****************************************************************************
//Since we have case sensitive and insensitive search.

using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Windows.Forms;

public class FindDialog: Form
{
    // A dialog for performing text searches through the Assembly tree.
    // The dialog maintains state for performing the Find Next command.  It
    // keeps track of the search string well the node to start searching
    // from.

    private MainWindow m_ParentForm;      //For access to data members such treeview and also for displaying status messages.;
    private string m_LastSearchedstring;  //The last string we have searched.;
    private TreeNode m_NextNode;          //The next node we will be checking in the Assembly tree.;

#region " Windows Form Designer generated code "

    //Form overrides dispose to clean up the component list.

    protected override void Dispose(bool disposing) {

        if (disposing) {

            if (components != null) {

                components.Dispose();

            }

        }

        base.Dispose(disposing);

    }

    private findDialogLabel System.Windows.Forms.Label;

    //Required by the Windows Form Designer

    private components System.ComponentModel.Container;

    //NOTE: The following procedure is required by the Windows Form Designer

    //It can be modified using the Windows Form Designer.  

    //Do ! modify it using the code editor.

    private FindTextBox System.Windows.Forms.TextBox;

    private FindButton System.Windows.Forms.Button;

    private CloseButton System.Windows.Forms.Button;

    private MatchCaseCheckBox System.Windows.Forms.CheckBox;

    private MatchExactstringCheckBox System.Windows.Forms.CheckBox;

    private void InitializeComponent() {

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(FindDialog));

        this.findDialogLabel = new System.Windows.Forms.Label();

        this.FindTextBox = new System.Windows.Forms.TextBox();

        this.FindButton = new System.Windows.Forms.Button();

        this.CloseButton = new System.Windows.Forms.Button();

        this.MatchCaseCheckBox = new System.Windows.Forms.CheckBox();

        this.MatchExactstringCheckBox = new System.Windows.Forms.CheckBox();

        this.SuspendLayout();

        //

        //findDialogLabel

        //

        this.findDialogLabel.AccessibleDescription = CType(resources.GetObject("findDialogLabel.AccessibleDescription"), string);

        this.findDialogLabel.AccessibleName = CType(resources.GetObject("findDialogLabel.AccessibleName"), string);

        this.findDialogLabel.Anchor = CType(resources.GetObject("findDialogLabel.Anchor"), System.Windows.Forms.AnchorStyles);

        this.findDialogLabel.AutoSize = CType(resources.GetObject("findDialogLabel.AutoSize"), bool);

        this.findDialogLabel.Dock = CType(resources.GetObject("findDialogLabel.Dock"), System.Windows.Forms.DockStyle);

        this.findDialogLabel.Enabled = CType(resources.GetObject("findDialogLabel.Enabled"), bool);

        this.findDialogLabel.Font = CType(resources.GetObject("findDialogLabel.Font"), System.Drawing.Font);

        this.findDialogLabel.Image = CType(resources.GetObject("findDialogLabel.Image"), System.Drawing.Image);

        this.findDialogLabel.ImageAlign = CType(resources.GetObject("findDialogLabel.ImageAlign"), System.Drawing.ContentAlignment);

        this.findDialogLabel.ImageIndex = CType(resources.GetObject("findDialogLabel.ImageIndex"), int);

        this.findDialogLabel.ImeMode = CType(resources.GetObject("findDialogLabel.ImeMode"), System.Windows.Forms.ImeMode);

        this.findDialogLabel.Location = CType(resources.GetObject("findDialogLabel.Location"), System.Drawing.Point);

        this.findDialogLabel.Name = "findDialogLabel";

        this.findDialogLabel.RightToLeft = CType(resources.GetObject("findDialogLabel.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.findDialogLabel.Size = CType(resources.GetObject("findDialogLabel.Size"), System.Drawing.Size);

        this.findDialogLabel.TabIndex = CType(resources.GetObject("findDialogLabel.TabIndex"), int);

        this.findDialogLabel.Text = resources.GetString("findDialogLabel.Text");

        this.findDialogLabel.TextAlign = CType(resources.GetObject("findDialogLabel.TextAlign"), System.Drawing.ContentAlignment);

        this.findDialogLabel.Visible = CType(resources.GetObject("findDialogLabel.Visible"), bool);

        //

        //FindTextBox

        //

        this.FindTextBox.AccessibleDescription = CType(resources.GetObject("FindTextBox.AccessibleDescription"), string);

        this.FindTextBox.AccessibleName = CType(resources.GetObject("FindTextBox.AccessibleName"), string);

        this.FindTextBox.Anchor = CType(resources.GetObject("FindTextBox.Anchor"), System.Windows.Forms.AnchorStyles);

        this.FindTextBox.AutoSize = CType(resources.GetObject("FindTextBox.AutoSize"), bool);

        this.FindTextBox.BackgroundImage = CType(resources.GetObject("FindTextBox.BackgroundImage"), System.Drawing.Image);

        this.FindTextBox.Dock = CType(resources.GetObject("FindTextBox.Dock"), System.Windows.Forms.DockStyle);

        this.FindTextBox.Enabled = CType(resources.GetObject("FindTextBox.Enabled"), bool);

        this.FindTextBox.Font = CType(resources.GetObject("FindTextBox.Font"), System.Drawing.Font);

        this.FindTextBox.ImeMode = CType(resources.GetObject("FindTextBox.ImeMode"), System.Windows.Forms.ImeMode);

        this.FindTextBox.Location = CType(resources.GetObject("FindTextBox.Location"), System.Drawing.Point);

        this.FindTextBox.MaxLength = CType(resources.GetObject("FindTextBox.MaxLength"), int);

        this.FindTextBox.Multiline = CType(resources.GetObject("FindTextBox.Multiline"), bool);

        this.FindTextBox.Name = "FindTextBox";

        this.FindTextBox.PasswordChar = CType(resources.GetObject("FindTextBox.PasswordChar"), char);

        this.FindTextBox.RightToLeft = CType(resources.GetObject("FindTextBox.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.FindTextBox.ScrollBars = CType(resources.GetObject("FindTextBox.ScrollBars"), System.Windows.Forms.ScrollBars);

        this.FindTextBox.Size = CType(resources.GetObject("FindTextBox.Size"), System.Drawing.Size);

        this.FindTextBox.TabIndex = CType(resources.GetObject("FindTextBox.TabIndex"), int);

        this.FindTextBox.Text = resources.GetString("FindTextBox.Text");

        this.FindTextBox.TextAlign = CType(resources.GetObject("FindTextBox.TextAlign"), System.Windows.Forms.HorizontalAlignment);

        this.FindTextBox.Visible = CType(resources.GetObject("FindTextBox.Visible"), bool);

        this.FindTextBox.WordWrap = CType(resources.GetObject("FindTextBox.WordWrap"), bool);

        //

        //FindButton

        //

        this.FindButton.AccessibleDescription = CType(resources.GetObject("FindButton.AccessibleDescription"), string);

        this.FindButton.AccessibleName = CType(resources.GetObject("FindButton.AccessibleName"), string);

        this.FindButton.Anchor = CType(resources.GetObject("FindButton.Anchor"), System.Windows.Forms.AnchorStyles);

        this.FindButton.BackgroundImage = CType(resources.GetObject("FindButton.BackgroundImage"), System.Drawing.Image);

        this.FindButton.Dock = CType(resources.GetObject("FindButton.Dock"), System.Windows.Forms.DockStyle);

        this.FindButton.Enabled = CType(resources.GetObject("FindButton.Enabled"), bool);

        this.FindButton.FlatStyle = CType(resources.GetObject("FindButton.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.FindButton.Font = CType(resources.GetObject("FindButton.Font"), System.Drawing.Font);

        this.FindButton.Image = CType(resources.GetObject("FindButton.Image"), System.Drawing.Image);

        this.FindButton.ImageAlign = CType(resources.GetObject("FindButton.ImageAlign"), System.Drawing.ContentAlignment);

        this.FindButton.ImageIndex = CType(resources.GetObject("FindButton.ImageIndex"), int);

        this.FindButton.ImeMode = CType(resources.GetObject("FindButton.ImeMode"), System.Windows.Forms.ImeMode);

        this.FindButton.Location = CType(resources.GetObject("FindButton.Location"), System.Drawing.Point);

        this.FindButton.Name = "FindButton";

        this.FindButton.RightToLeft = CType(resources.GetObject("FindButton.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.FindButton.Size = CType(resources.GetObject("FindButton.Size"), System.Drawing.Size);

        this.FindButton.TabIndex = CType(resources.GetObject("FindButton.TabIndex"), int);

        this.FindButton.Text = resources.GetString("FindButton.Text");

        this.FindButton.TextAlign = CType(resources.GetObject("FindButton.TextAlign"), System.Drawing.ContentAlignment);

        this.FindButton.Visible = CType(resources.GetObject("FindButton.Visible"), bool);

        //

        //CloseButton

        //

        this.CloseButton.AccessibleDescription = CType(resources.GetObject("CloseButton.AccessibleDescription"), string);

        this.CloseButton.AccessibleName = CType(resources.GetObject("CloseButton.AccessibleName"), string);

        this.CloseButton.Anchor = CType(resources.GetObject("CloseButton.Anchor"), System.Windows.Forms.AnchorStyles);

        this.CloseButton.BackgroundImage = CType(resources.GetObject("CloseButton.BackgroundImage"), System.Drawing.Image);

        this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;

        this.CloseButton.Dock = CType(resources.GetObject("CloseButton.Dock"), System.Windows.Forms.DockStyle);

        this.CloseButton.Enabled = CType(resources.GetObject("CloseButton.Enabled"), bool);

        this.CloseButton.FlatStyle = CType(resources.GetObject("CloseButton.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.CloseButton.Font = CType(resources.GetObject("CloseButton.Font"), System.Drawing.Font);

        this.CloseButton.Image = CType(resources.GetObject("CloseButton.Image"), System.Drawing.Image);

        this.CloseButton.ImageAlign = CType(resources.GetObject("CloseButton.ImageAlign"), System.Drawing.ContentAlignment);

        this.CloseButton.ImageIndex = CType(resources.GetObject("CloseButton.ImageIndex"), int);

        this.CloseButton.ImeMode = CType(resources.GetObject("CloseButton.ImeMode"), System.Windows.Forms.ImeMode);

        this.CloseButton.Location = CType(resources.GetObject("CloseButton.Location"), System.Drawing.Point);

        this.CloseButton.Name = "CloseButton";

        this.CloseButton.RightToLeft = CType(resources.GetObject("CloseButton.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.CloseButton.Size = CType(resources.GetObject("CloseButton.Size"), System.Drawing.Size);

        this.CloseButton.TabIndex = CType(resources.GetObject("CloseButton.TabIndex"), int);

        this.CloseButton.Text = resources.GetString("CloseButton.Text");

        this.CloseButton.TextAlign = CType(resources.GetObject("CloseButton.TextAlign"), System.Drawing.ContentAlignment);

        this.CloseButton.Visible = CType(resources.GetObject("CloseButton.Visible"), bool);

        //

        //MatchCaseCheckBox

        //

        this.MatchCaseCheckBox.AccessibleDescription = CType(resources.GetObject("MatchCaseCheckBox.AccessibleDescription"), string);

        this.MatchCaseCheckBox.AccessibleName = CType(resources.GetObject("MatchCaseCheckBox.AccessibleName"), string);

        this.MatchCaseCheckBox.Anchor = CType(resources.GetObject("MatchCaseCheckBox.Anchor"), System.Windows.Forms.AnchorStyles);

        this.MatchCaseCheckBox.Appearance = CType(resources.GetObject("MatchCaseCheckBox.Appearance"), System.Windows.Forms.Appearance);

        this.MatchCaseCheckBox.BackgroundImage = CType(resources.GetObject("MatchCaseCheckBox.BackgroundImage"), System.Drawing.Image);

        this.MatchCaseCheckBox.CheckAlign = CType(resources.GetObject("MatchCaseCheckBox.CheckAlign"), System.Drawing.ContentAlignment);

        this.MatchCaseCheckBox.Dock = CType(resources.GetObject("MatchCaseCheckBox.Dock"), System.Windows.Forms.DockStyle);

        this.MatchCaseCheckBox.Enabled = CType(resources.GetObject("MatchCaseCheckBox.Enabled"), bool);

        this.MatchCaseCheckBox.FlatStyle = CType(resources.GetObject("MatchCaseCheckBox.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.MatchCaseCheckBox.Font = CType(resources.GetObject("MatchCaseCheckBox.Font"), System.Drawing.Font);

        this.MatchCaseCheckBox.Image = CType(resources.GetObject("MatchCaseCheckBox.Image"), System.Drawing.Image);

        this.MatchCaseCheckBox.ImageAlign = CType(resources.GetObject("MatchCaseCheckBox.ImageAlign"), System.Drawing.ContentAlignment);

        this.MatchCaseCheckBox.ImageIndex = CType(resources.GetObject("MatchCaseCheckBox.ImageIndex"), int);

        this.MatchCaseCheckBox.ImeMode = CType(resources.GetObject("MatchCaseCheckBox.ImeMode"), System.Windows.Forms.ImeMode);

        this.MatchCaseCheckBox.Location = CType(resources.GetObject("MatchCaseCheckBox.Location"), System.Drawing.Point);

        this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";

        this.MatchCaseCheckBox.RightToLeft = CType(resources.GetObject("MatchCaseCheckBox.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.MatchCaseCheckBox.Size = CType(resources.GetObject("MatchCaseCheckBox.Size"), System.Drawing.Size);

        this.MatchCaseCheckBox.TabIndex = CType(resources.GetObject("MatchCaseCheckBox.TabIndex"), int);

        this.MatchCaseCheckBox.Text = resources.GetString("MatchCaseCheckBox.Text");

        this.MatchCaseCheckBox.TextAlign = CType(resources.GetObject("MatchCaseCheckBox.TextAlign"), System.Drawing.ContentAlignment);

        this.MatchCaseCheckBox.Visible = CType(resources.GetObject("MatchCaseCheckBox.Visible"), bool);

        //

        //MatchExactstringCheckBox

        //

        this.MatchExactstringCheckBox.AccessibleDescription = CType(resources.GetObject("MatchExactstringCheckBox.AccessibleDescription"), string);

        this.MatchExactstringCheckBox.AccessibleName = CType(resources.GetObject("MatchExactstringCheckBox.AccessibleName"), string);

        this.MatchExactstringCheckBox.Anchor = CType(resources.GetObject("MatchExactstringCheckBox.Anchor"), System.Windows.Forms.AnchorStyles);

        this.MatchExactstringCheckBox.Appearance = CType(resources.GetObject("MatchExactstringCheckBox.Appearance"), System.Windows.Forms.Appearance);

        this.MatchExactstringCheckBox.BackgroundImage = CType(resources.GetObject("MatchExactstringCheckBox.BackgroundImage"), System.Drawing.Image);

        this.MatchExactstringCheckBox.CheckAlign = CType(resources.GetObject("MatchExactstringCheckBox.CheckAlign"), System.Drawing.ContentAlignment);

        this.MatchExactstringCheckBox.Dock = CType(resources.GetObject("MatchExactstringCheckBox.Dock"), System.Windows.Forms.DockStyle);

        this.MatchExactstringCheckBox.Enabled = CType(resources.GetObject("MatchExactstringCheckBox.Enabled"), bool);

        this.MatchExactstringCheckBox.FlatStyle = CType(resources.GetObject("MatchExactstringCheckBox.FlatStyle"), System.Windows.Forms.FlatStyle);

        this.MatchExactstringCheckBox.Font = CType(resources.GetObject("MatchExactstringCheckBox.Font"), System.Drawing.Font);

        this.MatchExactstringCheckBox.Image = CType(resources.GetObject("MatchExactstringCheckBox.Image"), System.Drawing.Image);

        this.MatchExactstringCheckBox.ImageAlign = CType(resources.GetObject("MatchExactstringCheckBox.ImageAlign"), System.Drawing.ContentAlignment);

        this.MatchExactstringCheckBox.ImageIndex = CType(resources.GetObject("MatchExactstringCheckBox.ImageIndex"), int);

        this.MatchExactstringCheckBox.ImeMode = CType(resources.GetObject("MatchExactstringCheckBox.ImeMode"), System.Windows.Forms.ImeMode);

        this.MatchExactstringCheckBox.Location = CType(resources.GetObject("MatchExactstringCheckBox.Location"), System.Drawing.Point);

        this.MatchExactstringCheckBox.Name = "MatchExactstringCheckBox";

        this.MatchExactstringCheckBox.RightToLeft = CType(resources.GetObject("MatchExactstringCheckBox.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.MatchExactstringCheckBox.Size = CType(resources.GetObject("MatchExactstringCheckBox.Size"), System.Drawing.Size);

        this.MatchExactstringCheckBox.TabIndex = CType(resources.GetObject("MatchExactstringCheckBox.TabIndex"), int);

        this.MatchExactstringCheckBox.Text = resources.GetString("MatchExactstringCheckBox.Text");

        this.MatchExactstringCheckBox.TextAlign = CType(resources.GetObject("MatchExactstringCheckBox.TextAlign"), System.Drawing.ContentAlignment);

        this.MatchExactstringCheckBox.Visible = CType(resources.GetObject("MatchExactstringCheckBox.Visible"), bool);

        //

        //FindDialog

        //

        this.AcceptButton = this.FindButton;

        this.AccessibleDescription = CType(resources.GetObject("$this.AccessibleDescription"), string);

        this.AccessibleName = CType(resources.GetObject("$this.AccessibleName"), string);

        this.Anchor = CType(resources.GetObject("$this.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AutoScaleBaseSize = CType(resources.GetObject("$this.AutoScaleBaseSize"), System.Drawing.Size);

        this.AutoScroll = CType(resources.GetObject("$this.AutoScroll"), bool);

        this.AutoScrollMargin = CType(resources.GetObject("$this.AutoScrollMargin"), System.Drawing.Size);

        this.AutoScrollMinSize = CType(resources.GetObject("$this.AutoScrollMinSize"), System.Drawing.Size);

        this.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image);

        this.CancelButton = this.CloseButton;

        this.ClientSize = CType(resources.GetObject("$this.ClientSize"), System.Drawing.Size);

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.FindButton, this.findDialogLabel, this.MatchExactstringCheckBox, this.MatchCaseCheckBox, this.CloseButton, this.FindTextBox});

        this.Dock = CType(resources.GetObject("$this.Dock"), System.Windows.Forms.DockStyle);

        this.Enabled = CType(resources.GetObject("$this.Enabled"), bool);

        this.Font = CType(resources.GetObject("$this.Font"), System.Drawing.Font);

        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

        this.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon);

        this.ImeMode = CType(resources.GetObject("$this.ImeMode"), System.Windows.Forms.ImeMode);

        this.Location = CType(resources.GetObject("$this.Location"), System.Drawing.Point);

        this.MaximizeBox = false;

        this.MaximumSize = CType(resources.GetObject("$this.MaximumSize"), System.Drawing.Size);

        this.MinimizeBox = false;

        this.MinimumSize = CType(resources.GetObject("$this.MinimumSize"), System.Drawing.Size);

        this.Name = "FindDialog";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ShowInTaskbar = false;

        this.StartPosition = CType(resources.GetObject("$this.StartPosition"), System.Windows.Forms.FormStartPosition);

        this.Text = resources.GetString("$this.Text");

        this.TopMost = true;

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        this.ResumeLayout(false);

    }

#endregion

    public FindDialog(MainWindow form)
	{

        m_ParentForm = form;

        //This call is required by the Windows Form Designer.

        InitializeComponent();

        //Reset function is equivalent to an initialize() here

        Reset();

    }

    public void Reset()
	{

        // Resets the state of the Find so a new search can begin.

        m_LastSearchedstring = null;

        //The next node is now the first node in the Assembly tree, unless it has no nodes.

		if (m_ParentForm.TreeView.GetNodeCount(false) > 0) 
		{
			m_NextNode = m_ParentForm.TreeView.Nodes(0);
		}
		else 
		{
			m_NextNode = null;
		}

    }

    private void CloseButton_Click(object sender, System.EventArgs e) //CloseButton.Click;
	{

        //Hide the form instead of closing it because we want to maintain the Find state.
        this.Hide();

    }

    private void FindTextBox_TextChanged(object sender, System.EventArgs e) //FindTextBox.TextChanged;
	{

        FindButton.Enabled = FindTextBox.Text != "";

    }

    private void FindButton_Click(object sender, System.EventArgs e) //FindButton.Click;
	{
        if (m_ParentForm.TreeView.Nodes.Count = 0) 
			{
            MessageBox.Show("The Assembly tree is empty.");
            return;
        }

        string searchtext = this.FindTextBox.Text;

        //Reset the Find state if this is a new search string.

        if (searchtext != m_LastSearchedstring) 
			{
            Reset();
            m_LastSearchedstring = searchtext;
        }

        StartSearch(searchtext);

    }

    private void StartSearch(string searchtext)
	{

        // Perform the actual search.

        m_ParentForm.StatusMessage("Searching ...");
        TreeNode currentNode;
        bool found = false;
        while ((!found) && (m_NextNode != null))
		{
            currentNode = m_NextNode;
            if (IsMatch(searchtext, currentNode.Text)) 
				{
                m_ParentForm.TreeView.SelectedNode = currentNode;
                m_ParentForm.StatusMessageReady();
                found = true;
            }
            FindNext(currentNode);
        }

        //if we have reached the end of the search, ask to search again.

        if ((m_NextNode == null) && (!found)) 
			{
            m_ParentForm.StatusMessageReady();
            if  (DialogResult.OK = MessageBox.Show("The end of the tree has been reached. Start search from the beginning?", MsgBoxStyle.OKCancel))
				{
                m_NextNode = m_ParentForm.TreeView.Nodes(0);
                //For simplicity, just recurse.
                StartSearch(searchtext);
            }
        }
        m_ParentForm.StatusMessageReady();
    }

    private void FindNext(StartingNode TreeNode)
	{

        //set {the next node to be searched.
        //
        //We need to consider three cases:
        //   1. if the node has children, select the first child.
        //   2. if the node has a sibling next to it it, select the sibling.
        //   3. else {, go to the parent and do case #2 over again.
        //

        //if there are no nodes left to search, the loop will bottom out at the
        //root of the tree and m_NextNode will be null.

		if (StartingNode.Nodes.Count > 0) 
		{
			//The next node is the first child.
			m_NextNode = StartingNode.Nodes(0);
		} 
		else if (StartingNode.NextNode != null) 
		{
			//The next node is the next sibling.
			m_NextNode = StartingNode.NextNode;
		}
		else 
		{
			//The next node is the parent's next sibling.
			//Walk up the tree, looking for a parent that has a next sibling.
			m_NextNode = StartingNode.Parent;
			while (m_NextNode != null)
			{
				if (m_NextNode.NextNode != null)
				{
				//We have found a sibling.
				m_NextNode = m_NextNode.NextNode;
				break;
			}

			m_NextNode = m_NextNode.Parent;

		}

        }

    }

    public void SkipDeletedNode(TreeNode DeletedNode)
	{
        Debug.Assert(DeletedNode != null, "DeletedNode doesn't exist.");
        if (m_NextNode == DeletedNode) 
			{
            FindNext(DeletedNode);
        }

    }

    private bool IsMatch(string searchtext, string nodetext)
	{

        Debug.Assert(searchtext != "", "Can! search with an empty string; the find button should be disabled.");

        if (!this.MatchCaseCheckBox.Checked) 
			{
            searchtext = searchtext.ToLower;
            nodetext = nodetext.ToLower;
        }

        //The text must match exactly if the user is doing an exact string search, otherwise
        //substring matches are okay.

		if (this.MatchExactstringCheckBox.Checked) 
		{
			return searchtext = nodetext;
		}
		else 
		{
			return nodetext.IndexOf(searchtext) != -1;
		}

    }

    private void CheckBox_CheckedChanged(object sender, System.EventArgs e) //MatchCaseCheckBox.CheckedChanged, MatchExactstringCheckBox.CheckedChanged;
	{

        Reset();

    }

}

