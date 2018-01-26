//-------------------------------------------------------------------------------------------------
// <copyright file="TextNode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A node within a Solution Explorer hierarchy that only displays text.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using System.Drawing;
    using System.IO;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// A node within the Solution Explorer hierarchy that only displays text.
    /// </summary>
    public sealed class TextNode : Node
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private static readonly Type classType = typeof(TextNode);

        private string caption;
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        public TextNode(Hierarchy hierarchy, string absoluteDirectory, string caption) :
            base(hierarchy, Path.Combine(absoluteDirectory, caption))
        {
            this.caption = caption;
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        public override string Caption
        {
            get { return this.caption; }
        }

        public override Image Image
        {
            get { return HierarchyImages.Blank; }
        }

        public override bool IsFile
        {
            get { return false; }
        }

        public override bool IsFolder
        {
            get { return false; }
        }

        public override bool IsVirtual
        {
            get { return true; }
        }

        public override Guid VisualStudioTypeGuid
        {
            get { return NativeMethods.GUID_ItemType_VirtualFolder; }
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        public override void DoDefaultAction()
        {
            // Do nothing.
        }

        public override int SetCaption(string value)
        {
            if (!String.Equals(this.Caption, value, StringComparison.CurrentCulture))
            {
                this.caption = value;
                this.OnPropertyChanged(__VSHPROPID.VSHPROPID_Caption);
            }

            return NativeMethods.S_OK;
        }
        #endregion
    }
}
