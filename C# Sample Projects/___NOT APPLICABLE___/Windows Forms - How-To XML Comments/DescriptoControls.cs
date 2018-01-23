//*****************************************************************************
// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.
//*****************************************************************************

using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Reflection.Assembly;

public class DescriptoBox:RichTextBox
{

    // A textbox control specifically tailored for the XML Documentation Tool.
    // There is a one-to-one mapping between a member node's content
    // descriptors and DescriptoBoxes.  It is a DescriptoBox which offers the
    // interface through which a content descriptor is edited.

    private const int BOX_HEIGHT = 64;    //Height of a DescriptoBox.;
    private ContentDescriptor m_Descriptor;   //The descriptor which this textbox edits.;

    public DescriptoBox(descriptor ContentDescriptor, ContextMenu ContextMenu, TextChangedHandler EventHandler)
	{

        // Builds a DescriptoBox with a specific visual style.  It also hooks-
        // up a TextChanged event handler so the member node and content
        // descriptor's states can be updated.

        m_Descriptor = descriptor;
        this.Multiline = true;
        this.ContextMenu = ContextMenu;
        this.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

        //Width will be adjusted automatically, but the height will remain static.

        this.Height = BOX_HEIGHT;

        //The inital contents of the DescriptoBox is the content of its corresponding content descriptor.

        this.Text = descriptor.Content;

        //A descriptor with errors can! be edited, so make the DescriptoBox readonly, otherwise
        //hook-up the textchanged event handler.

		if (descriptor.HasErrors) 
		{
			this.ReadOnly = true;
			this.ForeColor = Color.Red;
		}
		else 
		{
			this.TextChanged +=new System.EventHandler(TextChangedHandler);

		}

    }

    public ContentDescriptor Descriptor
	{

        get {

            return m_Descriptor;

        }

    }

}

public class DescriptoLabel: Label
{

    // A label control specifically tailored for the XML Documentation Tool.
    // For simplicity, every DescriptoLabel references the content descriptor
    // for which it was built.  This facilitates deleting the content
    // descriptor when the user selects "Delete" from the DescriptoLabel's
    // context menu.  A label need ! represent a content descriptor, however
    // the context menu and delete ability goes away.

    private ContentDescriptor m_Descriptor;       //The descriptor which this label represents.;
    private m_LabelContextMenu ContextMenu;       //The context menu for the DescriptoLabel (used for sinking events).;
    private m_MenuDelete MenuItem;                //Delete item on the context menu (used for sinking events).;

    //Signals the user has chosen to delete the content descriptor.

    delegate void DeleteDescriptor(ContentDescriptor descriptor);

    public DescriptoLabel(string text, ContentDescriptor descriptor, DeleteDescriptorEventHandler DeleteHandler)
	{

        // Builds a DescriptoLabel using the text for the label and the
        // descriptor for which this label represents.  The visual style is
        // also built, which includes a context menu and text alignment.
        
        m_Descriptor = descriptor;

        this.Text = text;
        this.TextAlign = ContentAlignment.MiddleLeft;
        this.Size = new Size(this.PreferredWidth, this.PreferredHeight);
        if (m_Descriptor != null) 
			{
            if (m_Descriptor.HasErrors) 
				{
                this.ForeColor = Color.Red;
				DeleteDescriptor += new EventHandler(DeleteHandler);
            }

            //Construct the context menu and menu items.

            m_MenuDelete = new MenuItem("Delete");
            m_LabelContextMenu = new ContextMenu();
            m_LabelContextMenu.MenuItems.Add(m_MenuDelete);
            this.ContextMenu = m_LabelContextMenu;

        }

    }

    private void ContextMenu_Popup(object sender, System.EventArgs e) //m_LabelContextMenu.Popup;
	{
        //The Delete command on the context menu is enabled only if this label's descriptor has errors.

        m_MenuDelete.Enabled = ((m_Descriptor != null) && (m_Descriptor.HasErrors));

    }

    private void MenuDelete_Click(object sender, System.EventArgs e) //m_MenuDelete.Click;
	{
        //Signal that the user wants to delete this label.

        RaiseEvent(DeleteDescriptor(m_Descriptor));

    }

}

