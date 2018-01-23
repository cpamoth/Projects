//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

public class AssemblyTreeView: TreeView
{

    // A tree view control with drag and drop ability.  This control is used to
    // display the Assembly tree.
    //Signals that a node has been copied to another node.  The main window uses this event to update the MDI forms.

    public Event NodeCopied(destinationNode MemberNode);

    public AssemblyTreeView() {

        //Turn on drag and drop.
        this.AllowDrop = true;
    }

    protected override void OnDragDrop(drgevent DragEventArgs)
	{

        // Perform the actual Drag and Drop operation.  The copy target will
        // have been selected by the OnDragOver method.  The node kinds are
        // compared and the user is notified if the node shapes prohibit a
        // copy.  if the copy occurs, an OnNodeCopied event is raised so the
        // main window can update the MDI forms.
        //
        // Tip:
        //     It will be difficult to determine the type of data in the drag
        //     event object.  try { the code below first to figure out the
        //     correct string parameter for DragEventArgs.Data.GetData:
        //
        //         x string
        //         foreach(x In drgevent.Data.GetFormats(true)
        //            obj Object = _
        //                   drgevent.Data.GetData(x, false)
        //            if ! obj == null {
        //               Debug.WriteLine(x)
        //            }
        //         Next

        base.OnDragDrop(drgevent);
        //get {the object being dragged.
        MemberNode source = (MemberNode) (drgevent.Data.GetData("XMLDocumentationTool.MemberNode",False));
        //There is no target, so do nothing.
        if (this.SelectedNode == null)
		{
            return;
        }
        MemberNode target = (MemberNode) (this.SelectedNode);
        //Copying a node onto itself does nothing.
        if (target == source) {
            return;
        }
        //Copy
        CopyNode(source, target);
    }

    protected override void OnDragEnter(drgevent DragEventArgs)
	{

        // The user is allowed to drag and drop member nodes only.  if the user
        // attempts to drag something else, display a Not-Allowed mouse cursor.

        Object obj = drgevent.Data.GetData("XMLDocumentationTool.MemberNode", false);

		if (obj == null) 
		{
			drgevent.Effect = DragDropEffects.None;
		}
		else 
		{
			drgevent.Effect = DragDropEffects.Copy;
		}

        base.OnDragEnter(drgevent);

    }

    protected override void OnDragOver(drgevent DragEventArgs)
	{

        //Select the target nodes something is dragged over them.

        base.OnDragOver(drgevent);

        //The PointToClient function is necessary to orient the Point coordinates correctly.

        this.SelectedNode = this.GetNodeAt(PointToClient(new Point(drgevent.X, drgevent.Y)));

    }

    protected override void OnItemDrag(e ItemDragEventArgs)
	{

        base.OnItemDrag(e);

        //Begin the drag-drop operation.

        DoDragDrop(e.Item, DragDropEffects.Copy);

    }

    public void CopyNode(source MemberNode, target MemberNode)
	{

        //Copy one node to another, either by copy/paste or drag/drop.

        if (!source.HasContent()) {

            MessageBox.Show("Source has no content. null to copy.");

            return;

        }

        //Determine the differences between the source and target.  if the differences are too great,
        //a copy can! be performed.

        switch(MemberNode.ClassifyDifference(source, target))
		{

			case Difference.None:
			{
				//There are no differences between the source and target which prohibit a copy.
				//Ask the user to verify the copy operation.

				string message = "Copy:" + Environment.NewLine + "    " + source.FriendlySignatureWithPath 
					+ Environment.NewLine + "To:" + Environment.NewLine + "    " 
					+ target.FriendlySignatureWithPath + Environment.NewLine 
					+ Environment.NewLine + "Destination content will be overwritten. Continue with copy?";

				if (MsgBoxResult.Yes = MessageBox.Show(message, MsgBoxStyle.YesNo)) 
				{

					//Copy the content.
					target.Copy(source);
					//Signal that a copy has occurred.
					RaiseEvent(NodeCopied(target));
				}
				break;
			}
			case Difference.ParamNum:
			{

				MessageBox.Show(GetErrorMessage(ErrorID.FieldCountNotMatch1, "parameter"));
				break;
			}
			case Difference.RemarksNum:
			{
				MessageBox.Show(GetErrorMessage(ErrorID.FieldCountNotMatch1, "remarks"));
				break;
			}
			case Difference.ReturnNum:
			{
				MessageBox.Show(GetErrorMessage(ErrorID.FieldCountNotMatch1, "returns"));
				break;
			}
			case Difference.SummaryNum:
			{
				MessageBox.Show(GetErrorMessage(ErrorID.FieldCountNotMatch1, "summary"));
				break;
			}
			case Difference.PropertyValueNum:
			{
				MessageBox.Show(GetErrorMessage(ErrorID.FieldCountNotMatch1, "value"));
				break;
			}
			default: 
			{

				Debug.Fail("unexpected difference value");
				break;
			}
        }

    }

}

