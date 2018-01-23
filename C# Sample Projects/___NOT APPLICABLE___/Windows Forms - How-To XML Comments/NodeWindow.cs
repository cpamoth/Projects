//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

using System.Windows.Forms;

using System.Collections;

using System.Diagnostics;

using System.Drawing;

public class NodeWindow : : Form;

    // A form UI for adding and modifying XML content for an assembly

    // member.  This form acts the visual and editable representation of a

    // member node's content descriptors.

    private const INDENT_WIDTH int = 30      'The offset for controls from the corresponding DescriptoLabel.;

    private const VERT_MARGIN int = 4        'Vertical distance between controls.;

    private const LEFT_MARGIN int = 5        'The amount of space between the controls and the form's left edge.;

    private const RIGHT_MARGIN int = 20      'The amount of space between the controls and the form's right edge.;

    private const TOP_MARGIN int = 5         'The amount of space between the controls and the form's top edge.;

    private const BOTTOM_MARGIN int = 40     'The amount of space between the controls and the form's bottom edge.;

    private m_Doc MainDoc                        'The core engine instance.;

    private m_MemberNode MemberNode              'The form's corresponding member node.;

    //The form raises these events so the logic in MainWindow can handle them (rather than duplicate the logic here).

    public Event MenuCutClicked(object sender, System.EventArgs e);

    public Event MenuCopyClicked(object sender, System.EventArgs e);

    public Event MenuPasteClicked(object sender, System.EventArgs e);

#region " Windows Form Designer generated code "

    private Holder UserControl;

    private TextBoxContextMenu System.Windows.Forms.ContextMenu;

    private ContextMenuCut System.Windows.Forms.MenuItem;

    private ContextMenuCopy System.Windows.Forms.MenuItem;

    private ContextMenuPaste System.Windows.Forms.MenuItem;

    //Form overrides dispose to clean up the component list.

    protected override void Dispose(bool disposing) {

        base.Dispose(disposing);

        if (components != null) {

            components.Dispose();

        }

    }

    //Required by the Windows Form Designer

    private components System.ComponentModel.Container;

    //NOTE: The following procedure is required by the Windows Form Designer

    //It can be modified using the Windows Form Designer.  

    //Do ! modify it using the code editor.

    private List System.Windows.Forms.UserControl;

    private void InitializeComponent() {

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(NodeWindow));

        this.List = new System.Windows.Forms.UserControl();

        this.TextBoxContextMenu = new System.Windows.Forms.ContextMenu();

        this.ContextMenuCut = new System.Windows.Forms.MenuItem();

        this.ContextMenuCopy = new System.Windows.Forms.MenuItem();

        this.ContextMenuPaste = new System.Windows.Forms.MenuItem();

        this.SuspendLayout();

        //

        //List

        //

        this.List.AccessibleDescription = CType(resources.GetObject("List.AccessibleDescription"), string);

        this.List.AccessibleName = CType(resources.GetObject("List.AccessibleName"), string);

        this.List.Anchor = CType(resources.GetObject("List.Anchor"), System.Windows.Forms.AnchorStyles);

        this.List.AutoScroll = CType(resources.GetObject("List.AutoScroll"), bool);

        this.List.AutoScrollMargin = CType(resources.GetObject("List.AutoScrollMargin"), System.Drawing.Size);

        this.List.AutoScrollMinSize = CType(resources.GetObject("List.AutoScrollMinSize"), System.Drawing.Size);

        this.List.BackColor = System.Drawing.SystemColors.Control;

        this.List.BackgroundImage = CType(resources.GetObject("List.BackgroundImage"), System.Drawing.Image);

        this.List.Dock = CType(resources.GetObject("List.Dock"), System.Windows.Forms.DockStyle);

        this.List.Enabled = CType(resources.GetObject("List.Enabled"), bool);

        this.List.Font = CType(resources.GetObject("List.Font"), System.Drawing.Font);

        this.List.ImeMode = CType(resources.GetObject("List.ImeMode"), System.Windows.Forms.ImeMode);

        this.List.Location = CType(resources.GetObject("List.Location"), System.Drawing.Point);

        this.List.Name = "List";

        this.List.RightToLeft = CType(resources.GetObject("List.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.List.Size = CType(resources.GetObject("List.Size"), System.Drawing.Size);

        this.List.TabIndex = CType(resources.GetObject("List.TabIndex"), int);

        this.List.Visible = CType(resources.GetObject("List.Visible"), bool);

        //

        //TextBoxContextMenu

        //

        this.TextBoxContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.ContextMenuCut, this.ContextMenuCopy, this.ContextMenuPaste});

        this.TextBoxContextMenu.RightToLeft = CType(resources.GetObject("TextBoxContextMenu.RightToLeft"), System.Windows.Forms.RightToLeft);

        //

        //ContextMenuCut

        //

        this.ContextMenuCut.Enabled = CType(resources.GetObject("ContextMenuCut.Enabled"), bool);

        this.ContextMenuCut.Index = 0;

        this.ContextMenuCut.Shortcut = CType(resources.GetObject("ContextMenuCut.Shortcut"), System.Windows.Forms.Shortcut);

        this.ContextMenuCut.ShowShortcut = CType(resources.GetObject("ContextMenuCut.ShowShortcut"), bool);

        this.ContextMenuCut.Text = resources.GetString("ContextMenuCut.Text");

        this.ContextMenuCut.Visible = CType(resources.GetObject("ContextMenuCut.Visible"), bool);

        //

        //ContextMenuCopy

        //

        this.ContextMenuCopy.Enabled = CType(resources.GetObject("ContextMenuCopy.Enabled"), bool);

        this.ContextMenuCopy.Index = 1;

        this.ContextMenuCopy.Shortcut = CType(resources.GetObject("ContextMenuCopy.Shortcut"), System.Windows.Forms.Shortcut);

        this.ContextMenuCopy.ShowShortcut = CType(resources.GetObject("ContextMenuCopy.ShowShortcut"), bool);

        this.ContextMenuCopy.Text = resources.GetString("ContextMenuCopy.Text");

        this.ContextMenuCopy.Visible = CType(resources.GetObject("ContextMenuCopy.Visible"), bool);

        //

        //ContextMenuPaste

        //

        this.ContextMenuPaste.Enabled = CType(resources.GetObject("ContextMenuPaste.Enabled"), bool);

        this.ContextMenuPaste.Index = 2;

        this.ContextMenuPaste.Shortcut = CType(resources.GetObject("ContextMenuPaste.Shortcut"), System.Windows.Forms.Shortcut);

        this.ContextMenuPaste.ShowShortcut = CType(resources.GetObject("ContextMenuPaste.ShowShortcut"), bool);

        this.ContextMenuPaste.Text = resources.GetString("ContextMenuPaste.Text");

        this.ContextMenuPaste.Visible = CType(resources.GetObject("ContextMenuPaste.Visible"), bool);

        //

        //NodeWindow

        //

        this.AccessibleDescription = CType(resources.GetObject("$this.AccessibleDescription"), string);

        this.AccessibleName = CType(resources.GetObject("$this.AccessibleName"), string);

        this.Anchor = CType(resources.GetObject("$this.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AutoScaleBaseSize = CType(resources.GetObject("$this.AutoScaleBaseSize"), System.Drawing.Size);

        this.AutoScroll = CType(resources.GetObject("$this.AutoScroll"), bool);

        this.AutoScrollMargin = CType(resources.GetObject("$this.AutoScrollMargin"), System.Drawing.Size);

        this.AutoScrollMinSize = CType(resources.GetObject("$this.AutoScrollMinSize"), System.Drawing.Size);

        this.BackColor = System.Drawing.SystemColors.Window;

        this.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image);

        this.ClientSize = CType(resources.GetObject("$this.ClientSize"), System.Drawing.Size);

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.List});

        this.Dock = CType(resources.GetObject("$this.Dock"), System.Windows.Forms.DockStyle);

        this.Enabled = CType(resources.GetObject("$this.Enabled"), bool);

        this.Font = CType(resources.GetObject("$this.Font"), System.Drawing.Font);

        this.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon);

        this.ImeMode = CType(resources.GetObject("$this.ImeMode"), System.Windows.Forms.ImeMode);

        this.Location = CType(resources.GetObject("$this.Location"), System.Drawing.Point);

        this.MaximumSize = CType(resources.GetObject("$this.MaximumSize"), System.Drawing.Size);

        this.MinimumSize = CType(resources.GetObject("$this.MinimumSize"), System.Drawing.Size);

        this.Name = "NodeWindow";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.StartPosition = CType(resources.GetObject("$this.StartPosition"), System.Windows.Forms.FormStartPosition);

        this.Text = resources.GetString("$this.Text");

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        this.ResumeLayout(false);

    }

#endregion

    public void new(node MemberNode, Doc MainDoc, ParentForm MainWindow);

        // Builds a Node Window from a member node.

        

        //This call is required by the Windows Form Designer.

        InitializeComponent();

        m_Doc = Doc;

        m_MemberNode = node;

        //The title of the window is the full friendly signature.

        this.Text = m_MemberNode.FriendlySignatureWithPathAndModifiers;

        //The new form takes on the state of the currently active form.

        //if there is none, the default state is Maximized.

        if ParentForm.ActiveMdiChild == null {

            this.WindowState = FormWindowState.Maximized;

        else {

            this.WindowState = ParentForm.ActiveMdiChild.WindowState;

        }

        BuildFormMembers();

    }

    private void PositionControls(newlabel DescriptoLabel, newbox DescriptoBox, indentlevel int, ref runningypos int);

        // Given a box and label control, and the current indentation level and

        // running Y coordinate, this function calculates the appropriate

        // location for the controls.  The running Y coordinate is ByRef

        // because the next controls' positions are dependent on the previous

        // controls' positions.

        //The X coordinate is the indentation plus the left margin.

        newlabel.Location = new Point(indentlevel * INDENT_WIDTH + LEFT_MARGIN, runningypos);

        //Calls to this function can be made without supplying a DescriptoBox instance, in which case the

        //running y coordiate must be based off the location of the label.

        if ! newbox == null {

            //DescriptoBoxes are indented one level beyond their associated labels.

            newbox.Location = new Point((indentlevel + 1) * INDENT_WIDTH + LEFT_MARGIN, newlabel.Bottom + VERT_MARGIN);

            //The box's width stretches to the right side of the form (minus the right margin).

            newbox.Width = this.Width - newbox.Location.X - RIGHT_MARGIN;

            runningypos = newbox.Bottom + VERT_MARGIN;

        else {

            runningypos = newlabel.Bottom + VERT_MARGIN;

        }

    }

    private void BuildFields(Descriptors ArrayList, LabelText string, indentlevel int, ref runningypos int);

        // Add new DescriptoLabel/DescriptoBox pairs onto the form based on a

        // list of content descriptors.

        CurrentDescriptor ContentDescriptor;

        foreach(CurrentDescriptor In Descriptors;

            newlabel DescriptoLabel = new DescriptoLabel(LabelText, CurrentDescriptor, AddressOf RemoveDescriptor);

            newbox DescriptoBox = new DescriptoBox(CurrentDescriptor, this.TextBoxContextMenu, AddressOf UpdateDescriptorState);

            PositionControls(newlabel, newbox, indentlevel, runningypos);

            this.List.Controls.AddRange(new Control() {newlabel, newbox});

        }

    }

    private void BuildParamFields(Descriptors ArrayList, indentlevel int, ref runningypos int);

        // Add new DescriptoLabel/DescriptoBox pairs onto the form based on a

        // list of parameter descriptors.

        CurrentParameter ParameterDescriptor;

        foreach(CurrentParameter In Descriptors;

            newlabel DescriptoLabel = new DescriptoLabel(CurrentParameter.FriendlySignature, CurrentParameter, AddressOf RemoveDescriptor);

            newbox DescriptoBox = new DescriptoBox(CurrentParameter, this.TextBoxContextMenu, AddressOf UpdateDescriptorState);

            PositionControls(newlabel, newbox, indentlevel, runningypos);

            this.List.Controls.AddRange(new Control() {newlabel, newbox});

        }

    }

    internal void BuildFormMembers();

        // Builds all the controls in a node window based on the content

        // descriptors of the member node.

        //To save memory, member nodes are ! automatically allocated their default content 

        //descriptors.  But without the content descriptors, the controls can! be built on

        //the form.  We need to ensure those default descriptors exist to ensure the controls

        //will be built.

        m_MemberNode.EnsureDefaultDescriptors();

        //Keep track of the Y coordinate we build the controls so we can know where to

        //position the next control.

        runningypos int = TOP_MARGIN;

        this.SuspendLayout();

        BuildFields(m_MemberNode.m_Summary, "Summary", 0, runningypos);

        //Parameters are placed under a label called "Parameters".

        if m_MemberNode.m_Params.Count > 0 {

            //Create the label "Parameters" and position it on the form.

            paramlabel new DescriptoLabel("Parameters", null, null);

            PositionControls(paramlabel, null, 0, runningypos);

            this.List.Controls.Add(paramlabel);

            //Params are indented from the other fields.

            BuildParamFields(m_MemberNode.m_Params, 1, runningypos);

        }

        returntype string = m_MemberNode.FriendlyReturnType;

        BuildFields(m_MemberNode.m_PropertyValue, "Value" + returntype, 0, runningypos);

        BuildFields(m_MemberNode.m_ReturnValue, "Returns" + returntype, 0, runningypos);

        BuildFields(m_MemberNode.m_Remarks, "Remarks", 0, runningypos);

        this.Height = (runningypos - VERT_MARGIN) + BOTTOM_MARGIN;

        this.ResumeLayout(true);

    }

    private void MenuCut_Click(object sender, System.EventArgs e) ContextMenuCut.Click;

        RaiseEvent MenuCutClicked(sender, e);

    }

    private void MenuCopy_Click(object sender, System.EventArgs e) ContextMenuCopy.Click;

        RaiseEvent MenuCopyClicked(sender, e);

    }

    private void MenuPaste_Click(object sender, System.EventArgs e) ContextMenuPaste.Click;

        RaiseEvent MenuPasteClicked(sender, e);

    }

    private void ContextMenu_Popup(object sender, System.EventArgs e) TextBoxContextMenu.Popup;

        //Disable/enable the context menu items.

        if ! this.List.ActiveControl == null {

            box DescriptoBox = CType(this.List.ActiveControl, DescriptoBox);

            ContextMenuCut.Enabled = ! box.ReadOnly AndAlso box.SelectionLength > 0;

            ContextMenuCopy.Enabled = box.SelectionLength > 0;

            ContextMenuPaste.Enabled = ! box.ReadOnly AndAlso typeof(System.Windows.Forms.Clipboard.GetDataObject.GetData(DataFormats.Text) == string;

        }

    }

    private void UpdateDescriptorState(object sender, System.EventArgs e);

        // The user has made an edit to the DescriptoBox.  Copy the text into

        // the content descriptor.  This tight coupling ensures the content

        // descriptor always has the correct state.

        Field DescriptoBox = CType(sender, DescriptoBox);

        if Field.Text.Length > 32000 {

            Field.Text = Microsoft.VisualBasic.Left(Field.Text, 32000);

            Field.SelectionStart = Field.TextLength;

            Microsoft.VisualBasic.Beep();

        }

        Field.Descriptor.Content = Field.Text;

        m_MemberNode.UpdateEditState();

    }

    public void RemoveDescriptor(Descriptor ContentDescriptor);

        // The user has chosen to delete a field.  Remove the corresponding

        // descriptor from the member node, remove all of its errors, and re-

        // build the node window.

        Debug.Assert(Descriptor.HasErrors, "Labels with no errors can! be removed.");

        e ErrorRecord;

        foreach(e In Descriptor.Errors;

            m_Doc.DeleteError(m_MemberNode, Descriptor, e);

        }

        //For simplicity, remove the descriptor from all lists. if the descriptor is not

        //in the list, Remove will do nothing.

        m_MemberNode.m_Summary.Remove(Descriptor);

        m_MemberNode.m_PropertyValue.Remove(Descriptor);

        m_MemberNode.m_Remarks.Remove(Descriptor);

        m_MemberNode.m_ReturnValue.Remove(Descriptor);

        m_MemberNode.m_Params.Remove(Descriptor);

        this.List.Controls.Clear();

        BuildFormMembers();

    }

}

