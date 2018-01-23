//*****************************************************************************
// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.
//*****************************************************************************
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

public enum ErrorID
{

    // The ID enum is a table of all errors the tool can generate.  The entries
    // in this enum are used to index into the table of error messages to fetch
    // their corresponding messages.  The numeric postfix indicates the number
    // of substitutions in the error message.

    NoError,
    NodeInXMLOnly,
    DuplicateField1,
    UnexpectedField2,
    ParamNameItemNotValid1,
    ParamNotFound2,
    UnknownField2,
    MemberNotValid1,
    MemberNameItemNotValid,
    BadSignatureInMemberNode1,
    //The errors below will ! show up in the error window but will instead show up in error dialogs.
    FieldCountNotMatch1,
    ErrorDuringLoad3,
    ErrorDuringSave3
}

public class ErrorTable
{

    // The table of error messages.  Each error message may have one or more
    // substitution sites denoted by a "|" followed by a number.

	private string[] Errorstrings = {
			"No error.",
			"This node is found only in the XML Documentation file.",
			"Duplicate '|0' field found.",
			"Unexpected '|0' field found. Only |1 can have '|0' fields.",
			"'name' item for parameter on node '|0' is ! valid.",
			"Parameter '|0' on node '|1' is found only in the XML Documentation file.",
			"Field '|0' is unrecognized; content is ignored. Content: '|1'",
			"Node name '|0' is ! valid. This node will be removed upon save.",
			"'name' item for member node is ! valid. This node will be removed upon save.",
			"Bad signature found in member node: '|0'",
			"The number of '|0' fields is greater in the source than in the destination. Can! copy.", 
			"An error occurred while loading |0 '|1'. Details: " + Environment.NewLine + Environment.NewLine + "|2",
			"An error occurred while saving |0 '|1'. Details:" + Environment.NewLine + Environment.NewLine + "|2"
			};

    public string GetErrorMessage(ErrorID ErrID, string[] voidstitutions)
{

        // This function maps Error IDs to error messages.  In doing so, it
        // sequentially makes the substitutions, specified with the
        // substitution param array, into the message using the substitution
        // sites embedded in the string.

        string Message = Errorstrings(ErrID);

        if (voidstitutions != null) {

			for (int i = 0;i < voidstitutions.Length;i++)
			{

                //Replace instances of "|i" with the corresponding item in the substitution param array.

                Message = Message.Replace("|" + i.ToString, voidstitutions[i]);

            }

        }

        return Message;

    }

}

public class ErrorRecord: ListViewItem
{

    // A container which represents an error.  Each record has an ID, a
    // reference to the associated member node, and a visual style which
    // includes the error message and path.  This class inherits from
    // ListViewItem so it can be directly inserted into the error list.

    private ErrorID m_ErrorID;    //This record's error ID;
    private MemberNode m_Member;  //The member this record is associated with.;

	private ErrorRecord(string Path, 
		string Message):base(new System.Windows.Forms.ListViewItem.ListViewSubItem[]
	{new System.Windows.Forms.ListViewItem.ListViewSubItem(
			null,Path,System.Drawing.SystemColors.WindowText,
			System.Drawing.SystemColors.Window, 
		new System.Drawing.Font("Microsoft Sans Serif",(float)8.25, 
		System.Drawing.FontStyle.Regular,System.Drawing.GraphicsUnit.Point,0))
		,new System.Windows.Forms.ListViewItem.ListViewSubItem( 
		Nothing, Message, System.Drawing.SystemColors.WindowText, 
		System.Drawing.SystemColors.Window, 
		new System.Drawing.Font("Microsoft Sans Serif", (float)8.25, 
		System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0))
		}, -1)
	{

        // Builds a instance of ListViewItem with the visual style of an error
        // record.  The text which is shown to the user is passed-in the
        // path and message.

    }

    public ErrorRecord(node MemberNode, ErrorID ErrorID, string[] voidstitutions):this(NormalizeNodePath(node.FullPath), GetErrorMessage(ErrorID, voidstitutions))
	{
        //Build the visual style, which includes fetching the error message and stripping 
        //out the slashes from the node's path so it looks good to the user.

        m_ErrorID = _ErrorID;
        m_Member = node;

    }

    public ErrorID _ErrorID
	{
        get {
            return m_ErrorID;

        }

    }

    public string Path
	{
        get {

            //The path is the text in the first column.
            return this.SubItems(0).Text;
        }
    }

    public string Message
	{
        get {
            //The message is the text in the second column.
            return this.SubItems(1).Text;
        }
    }

    public MemberNode Member
	{

        get {

            return m_Member;

        }

    }

}

public class ErrorWindow:UserControl
{

	// Displays all errors to the user.  Errors are represented ErrorRecords
	// held in a ListView control.  The report and delete methods on this
	// control are the only way to add and remove ErrorRecords from the list.
	//
	// Errors are created by passing-in an Error ID and a optional set of
	// substitutions.  During construction of an error record, the message
	// string is looked up in the error table using the ID and the
	// substitutions are made into the string.  The error record is then
	// inserted into the list view and also into any associated member nodes
	// and content descriptors (so these objects can keep track of it well).
	//
	// Errors are deleted by passing-in the error record to be deleted.  The
	// record gets removed from the list well from any member nodes and
	// content descriptors that are keeping track of it.

	private class ErrorRecordComparer: IComparer
	{

		// This class compares Error Records for sorting when the user clicks
		// on the error window's column headers.

		public enum CompareKind
		{

			Path,        //The 0th column in the error window.;
			Message     //The 1st column in the error window.;

		}

		private CompareKind m_Kind;

		public ErrorRecordComparer(CompareKind kind)
		{

			//The compare kind determines which field to compare Error Records with.
			m_Kind = kind;

		}

		public int Compare(Object x, Object y)
		{

		// Compare two Error Records.  The comparison kind determines which
		// field of the Error Record to perform the comparison on.

		switch(m_Kind)
			{
			case (CompareKind.Path):
				{	
				return (((ErrorRecord) x).Path.CompareTo(((ErrorRecord)(y)).Path);
				break;
				}
			case (CompareKind.Message):
				{
				return (((ErrorRecord) x).Message.CompareTo(((ErrorRecord) y).Message);
				break;
				}
			default: 
				{
                    Debug.Fail("Unexpected compare kind: " + m_Kind.ToString();
					break;
				}
        }
    }

		public delegate void ErrorListDoubleClick(MemberNode member);   //Fires when an error record is double-clicked.;
		public delegate void OnCloseButtonClicked();                    //Fires when the button to close the error window is clicked.;

#region " Windows Form Designer generated code "

    public () {

        

        //This call is required by the Windows Form Designer.

        InitializeComponent();

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

    private ErrorList System.Windows.Forms.ListView;

    private CloseButton System.Windows.Forms.Button;

    private ErrorInformationTitle System.Windows.Forms.Label;

    private ColumnPath System.Windows.Forms.ColumnHeader;

    private ColumnDescription System.Windows.Forms.ColumnHeader;

    //Required by the Windows Form Designer

    private System.ComponentModel.IContainer components = null;

    //NOTE: The following procedure is required by the Windows Form Designer

    //It can be modified using the Windows Form Designer.  

    //Do ! modify it using the code editor.

    private void InitializeComponent() {

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(ErrorWindow));

        this.ErrorInformationTitle = new System.Windows.Forms.Label();

        this.ErrorList = new System.Windows.Forms.ListView();

        this.ColumnPath = new System.Windows.Forms.ColumnHeader();

        this.ColumnDescription = new System.Windows.Forms.ColumnHeader();

        this.CloseButton = new System.Windows.Forms.Button();

        this.SuspendLayout();

        //

        //ErrorInformationTitle

        //

        this.ErrorInformationTitle.AccessibleDescription = CType(resources.GetObject("ErrorInformationTitle.AccessibleDescription"), string);

        this.ErrorInformationTitle.AccessibleName = CType(resources.GetObject("ErrorInformationTitle.AccessibleName"), string);

        this.ErrorInformationTitle.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;

        this.ErrorInformationTitle.Anchor = CType(resources.GetObject("ErrorInformationTitle.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ErrorInformationTitle.AutoSize = CType(resources.GetObject("ErrorInformationTitle.AutoSize"), bool);

        this.ErrorInformationTitle.BackColor = System.Drawing.SystemColors.Control;

        this.ErrorInformationTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

        this.ErrorInformationTitle.Dock = CType(resources.GetObject("ErrorInformationTitle.Dock"), System.Windows.Forms.DockStyle);

        this.ErrorInformationTitle.Enabled = CType(resources.GetObject("ErrorInformationTitle.Enabled"), bool);

        this.ErrorInformationTitle.Font = CType(resources.GetObject("ErrorInformationTitle.Font"), System.Drawing.Font);

        this.ErrorInformationTitle.Image = CType(resources.GetObject("ErrorInformationTitle.Image"), System.Drawing.Image);

        this.ErrorInformationTitle.ImageAlign = CType(resources.GetObject("ErrorInformationTitle.ImageAlign"), System.Drawing.ContentAlignment);

        this.ErrorInformationTitle.ImageIndex = CType(resources.GetObject("ErrorInformationTitle.ImageIndex"), int);

        this.ErrorInformationTitle.ImeMode = CType(resources.GetObject("ErrorInformationTitle.ImeMode"), System.Windows.Forms.ImeMode);

        this.ErrorInformationTitle.Location = CType(resources.GetObject("ErrorInformationTitle.Location"), System.Drawing.Point);

        this.ErrorInformationTitle.Name = "ErrorInformationTitle";

        this.ErrorInformationTitle.RightToLeft = CType(resources.GetObject("ErrorInformationTitle.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ErrorInformationTitle.Size = CType(resources.GetObject("ErrorInformationTitle.Size"), System.Drawing.Size);

        this.ErrorInformationTitle.TabIndex = CType(resources.GetObject("ErrorInformationTitle.TabIndex"), int);

        this.ErrorInformationTitle.Text = resources.GetString("ErrorInformationTitle.Text");

        this.ErrorInformationTitle.TextAlign = CType(resources.GetObject("ErrorInformationTitle.TextAlign"), System.Drawing.ContentAlignment);

        this.ErrorInformationTitle.Visible = CType(resources.GetObject("ErrorInformationTitle.Visible"), bool);

        //

        //ErrorList

        //

        this.ErrorList.AccessibleDescription = CType(resources.GetObject("ErrorList.AccessibleDescription"), string);

        this.ErrorList.AccessibleName = CType(resources.GetObject("ErrorList.AccessibleName"), string);

        this.ErrorList.Alignment = CType(resources.GetObject("ErrorList.Alignment"), System.Windows.Forms.ListViewAlignment);

        this.ErrorList.Anchor = CType(resources.GetObject("ErrorList.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ErrorList.BackColor = System.Drawing.SystemColors.Window;

        this.ErrorList.BackgroundImage = CType(resources.GetObject("ErrorList.BackgroundImage"), System.Drawing.Image);

        this.ErrorList.Columns.AddRange(new System.Windows.Forms.ColumnHeader() {this.ColumnPath, this.ColumnDescription});

        this.ErrorList.Dock = CType(resources.GetObject("ErrorList.Dock"), System.Windows.Forms.DockStyle);

        this.ErrorList.Enabled = CType(resources.GetObject("ErrorList.Enabled"), bool);

        this.ErrorList.Font = CType(resources.GetObject("ErrorList.Font"), System.Drawing.Font);

        this.ErrorList.FullRowSelect = true;

        this.ErrorList.HideSelection = false;

        this.ErrorList.ImeMode = CType(resources.GetObject("ErrorList.ImeMode"), System.Windows.Forms.ImeMode);

        this.ErrorList.LabelWrap = CType(resources.GetObject("ErrorList.LabelWrap"), bool);

        this.ErrorList.Location = CType(resources.GetObject("ErrorList.Location"), System.Drawing.Point);

        this.ErrorList.MultiSelect = false;

        this.ErrorList.Name = "ErrorList";

        this.ErrorList.RightToLeft = CType(resources.GetObject("ErrorList.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ErrorList.Size = CType(resources.GetObject("ErrorList.Size"), System.Drawing.Size);

        this.ErrorList.Sorting = System.Windows.Forms.SortOrder.Ascending;

        this.ErrorList.TabIndex = CType(resources.GetObject("ErrorList.TabIndex"), int);

        this.ErrorList.Text = resources.GetString("ErrorList.Text");

        this.ErrorList.View = System.Windows.Forms.View.Details;

        this.ErrorList.Visible = CType(resources.GetObject("ErrorList.Visible"), bool);

        //

        //ColumnPath

        //

        this.ColumnPath.Text = resources.GetString("ColumnPath.Text");

        this.ColumnPath.TextAlign = CType(resources.GetObject("ColumnPath.TextAlign"), System.Windows.Forms.HorizontalAlignment);

        this.ColumnPath.Width = CType(resources.GetObject("ColumnPath.Width"), int);

        //

        //ColumnDescription

        //

        this.ColumnDescription.Text = resources.GetString("ColumnDescription.Text");

        this.ColumnDescription.TextAlign = CType(resources.GetObject("ColumnDescription.TextAlign"), System.Windows.Forms.HorizontalAlignment);

        this.ColumnDescription.Width = CType(resources.GetObject("ColumnDescription.Width"), int);

        //

        //CloseButton

        //

        this.CloseButton.AccessibleDescription = CType(resources.GetObject("CloseButton.AccessibleDescription"), string);

        this.CloseButton.AccessibleName = CType(resources.GetObject("CloseButton.AccessibleName"), string);

        this.CloseButton.Anchor = CType(resources.GetObject("CloseButton.Anchor"), System.Windows.Forms.AnchorStyles);

        this.CloseButton.BackColor = System.Drawing.SystemColors.Control;

        this.CloseButton.BackgroundImage = CType(resources.GetObject("CloseButton.BackgroundImage"), System.Drawing.Image);

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

        //ErrorWindow

        //

        this.AccessibleDescription = CType(resources.GetObject("$this.AccessibleDescription"), string);

        this.AccessibleName = CType(resources.GetObject("$this.AccessibleName"), string);

        this.Anchor = CType(resources.GetObject("$this.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AutoScroll = CType(resources.GetObject("$this.AutoScroll"), bool);

        this.AutoScrollMargin = CType(resources.GetObject("$this.AutoScrollMargin"), System.Drawing.Size);

        this.AutoScrollMinSize = CType(resources.GetObject("$this.AutoScrollMinSize"), System.Drawing.Size);

        this.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image);

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.CloseButton, this.ErrorList, this.ErrorInformationTitle});

        this.Dock = CType(resources.GetObject("$this.Dock"), System.Windows.Forms.DockStyle);

        this.Enabled = CType(resources.GetObject("$this.Enabled"), bool);

        this.Font = CType(resources.GetObject("$this.Font"), System.Drawing.Font);

        this.ImeMode = CType(resources.GetObject("$this.ImeMode"), System.Windows.Forms.ImeMode);

        this.Location = CType(resources.GetObject("$this.Location"), System.Drawing.Point);

        this.Name = "ErrorWindow";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.Size = CType(resources.GetObject("$this.Size"), System.Drawing.Size);

        this.TabIndex = CType(resources.GetObject("$this.TabIndex"), int);

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        this.ResumeLayout(false);

    }

#endregion

    public void ReportError(MemberNode node, ErrorID ErrID, string[] voidstitutions)
		{

        Debug.Assert(ErrID != ErrorID.NoError, "Why report 'No Error'?");

        //Create a new error record and post it to the list.

        PostError(node, new ErrorRecord(node, ErrID, voidstitutions));

    }

    public void ReportError(MemberNode node, ContentDescriptor descriptor, ErrorID ErrID, string[] voidstitutions)
		{

        Debug.Assert(ErrID != ErrorID.NoError, "Why report 'No Error'?");

        //Create a new error record and post it to the list.
        PostError(node, descriptor, new ErrorRecord(node, ErrID, voidstitutions));

    }

    private void PostError(MemberNode node, ErrorRecord newError)
		{

        //Tell the node about this error.
        node.AddError(newError);
        //Actually put the error in the list.
        this.ErrorList.Items.Add(newError);

    }

    private void PostError(MemberNode node, ContentDescriptor descriptor, ErrorRecord newError)
		{

        PostError(node, newError);

        //Tell the descriptor about this error.

        descriptor.AddError(newError);

    }

    public void DeleteNodeFromErrorList(MemberNode node)
		{

        // Deletes all errors from a node and removes those errors records from
        // the error list.

        Debug.Assert(node.Errors != null, "Why try to delete a node which has no errors?");

        //Remove each Error the node is responsible for.

        foreach(ErrorRecord e in node.Errors)
			{
            this.ErrorList.Items.Remove(e);
        }

        //Remove all Errors from the node because they have all gone away.

        node.RemoveAllErrors();

    }

    public void DeleteError(MemberNode node, ErrorRecord OldError)
		{

        Debug.Assert(node.Errors != null, "Why try to delete a node which has no errors?");

        //Remove the error record from the errors list.

        this.ErrorList.Items.Remove(OldError);

        //And then tell the node to remove it well.

        node.RemoveError(OldError);

        FileDirty = true;

    }

    public void DeleteError(node MemberNode, descriptor ContentDescriptor, OldError ErrorRecord);

        DeleteError(node, OldError);

        //Tell the descriptor to remove the error record from its list of errors.

        descriptor.RemoveError(OldError);

    }

    public void ClearAllItems()
		{
        this.ErrorList.Items.Clear();
		}

    private void OnErrorListDoubleClick(object sender, System.EventArgs e) //ErrorList.DoubleClick;
		{
        // Signals that the user has double-clicked on an error record.  This
        // event is handled by the main window to display the associated member
        // node's window.
        RaiseEvent(ErrorListDoubleClick(((ErrorRecord)this.ErrorList.SelectedItems(0)).Member));
    }

    private void OnColumnHeader_Click(object sender, System.Windows.Forms.ColumnClickEventArgs e) //ErrorList.ColumnClick;

        //Determine the kind of the comparison.  The column number determines the kind of the comparison.
        ErrorRecordComparer.CompareKind kind = (ErrorRecordComparer.CompareKind) e.Column;

        //Setting the list view item sorter will automatically call the sort function.
        this.ErrorList.Columns(e.Column).ListView.ListViewItemSorter = new ErrorRecordComparer(kind);

    }

    private void ButtonClose_Click(object sender, System.EventArgs e) //CloseButton.Click;

        // Signals that this control requests to be closed.  When the close
        // button is clicked we will just hide the window.

        RaiseEvent(OnCloseButtonClicked());

    }

    public void AdjustColumns()
		{

        // Once the errors have been inserted into the list, the columns' sizes
        // can be adjusted to fit the width of the largest item.
        //Adjust the "path" column.

        this.ColumnDescription.Width = -1;

        //Adjust the "description" column.
        //this.ColumnPath.Width = -1  'This can be too wide so don't adjust it.

    }

    public int ErrorCount
		{
        get {
            return this.ErrorList.Items.Count;
        }
    }

    public void BeginUpdate()
		{
        this.ErrorList.BeginUpdate();
    }

    public void EndUpdate()
		{
        this.ErrorList.EndUpdate();
    }

}

