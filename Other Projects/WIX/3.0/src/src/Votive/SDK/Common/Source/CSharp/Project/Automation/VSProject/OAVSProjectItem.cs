/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using IServiceProvider = System.IServiceProvider;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using EnvDTE;
using VSLangProj;

namespace Microsoft.VisualStudio.Package.Automation
{
    [ComVisible(true), CLSCompliant(false)]
    public class OAVSProjectItem : VSProjectItem
    {
        #region fields
        private FileNode fileNode;
        #endregion

        #region ctors
        public OAVSProjectItem(FileNode fileNode)
        {
            this.FileNode = fileNode;
        }
        #endregion

        #region VSProjectItem Members

        public virtual Project ContainingProject
        {
            get { return fileNode.ProjectMgr.GetAutomationObject() as Project; }
        }

        public virtual ProjectItem ProjectItem
        {
            get { return fileNode.GetAutomationObject() as ProjectItem; }
        }

        public virtual DTE DTE
        {
            get { return (DTE)this.fileNode.ProjectMgr.Site.GetService(typeof(DTE)); }
        }

        public virtual void RunCustomTool()
        {
            FileNodeProperties props = FileNode.NodeProperties as FileNodeProperties;
            FileNode.InvokeGenerator(props.CustomTool, props.CustomToolNamespace);
        }

        #endregion

        #region public properties
        /// <summary>
        /// File Node property
        /// </summary>
        public FileNode FileNode
        {
            get
            {
                return fileNode;
            }
            set
            {
                fileNode = value;
            }
        }
        #endregion

    }
}
