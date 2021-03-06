/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

using MSBuild = Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Package
{
	[CLSCompliant(false), ComVisible(true)]
	public class OutputGroup : IVsOutputGroup2
	{
		#region fields
		private ProjectConfig projectCfg;
		private ProjectNode project;

		private List<Output> outputs = new List<Output>();
		private Output keyOutput = null;
		private string name;
		private string targetName;
		#endregion

		#region properties
		protected ProjectConfig ProjectCfg
		{
			get { return projectCfg; }
		}
		protected ProjectNode Project
		{
			get { return project; }
		}
		protected string TargetName
		{
			get { return targetName; }
		}
		#endregion

		#region ctors

		/// <summary>
		/// Constructor for IVSOutputGroup2 implementation
		/// </summary>
		/// <param name="outputName">Name of the output group. See VS_OUTPUTGROUP_CNAME_Build in vsshell.idl for the list of standard values</param>
		/// <param name="msBuildTargetName">MSBuild target name</param>
		/// <param name="projectManager">Project that produce this output</param>
		/// <param name="configuration">Configuration that produce this output</param>
		public OutputGroup(string outputName, string msBuildTargetName, ProjectNode projectManager, ProjectConfig configuration)
		{
			if (outputName == null)
				throw new ArgumentNullException("outputName");
			if (msBuildTargetName == null)
				throw new ArgumentNullException("outputName");
			if (projectManager == null)
				throw new ArgumentNullException("projectManager");
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			name = outputName;
			targetName = msBuildTargetName;
			project = projectManager;
			projectCfg = configuration;
		}
		#endregion

		#region virtual methods
		protected virtual void Refresh()
		{
			// Let MSBuild know which configuration we are working with
			project.SetConfiguration(projectCfg.ConfigName);
			// Generate dependencies if such a task exist
			const string generateDependencyList = "AllProjectOutputGroups";
			if (project.BuildProject.Targets.Exists(generateDependencyList))
			{
				bool succeeded = false;
				project.BuildTarget(generateDependencyList, out succeeded);
				Debug.Assert(succeeded, "Failed to build target: " + generateDependencyList);
			}
			
			// Rebuild the content of our list of output
			string outputType = this.targetName + "Output";
			this.outputs.Clear();
			foreach (MSBuild.BuildItem assembly in project.BuildProject.GetEvaluatedItemsByName(outputType))
			{
				Output output = new Output(project, projectCfg, project.GetProjectElement(assembly));
				this.outputs.Add(output);
				// See if it is our key output
				if (String.Compare(assembly.GetEvaluatedMetadata("IsKeyOutput"), true.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
					keyOutput = output;
			}

			project.SetCurrentConfiguration();

		}
		#endregion

		#region IVsOutputGroup2 Members

		public virtual int get_CanonicalName(out string pbstrCanonicalName)
		{
			pbstrCanonicalName = this.name;
			return VSConstants.S_OK;
		}

		public virtual int get_DeployDependencies(uint celt, IVsDeployDependency[] rgpdpd, uint[] pcActual)
		{
			return VSConstants.E_NOTIMPL;
		}

		public virtual int get_Description(out string pbstrDescription)
		{
			pbstrDescription = null;

			string description;
			int hr = this.get_CanonicalName(out description);
			if (ErrorHandler.Succeeded(hr))
				pbstrDescription = this.Project.GetOutputGroupDescription(description);
			return hr;
		}

		public virtual int get_DisplayName(out string pbstrDisplayName)
		{
			pbstrDisplayName = null;

			string displayName;
			int hr = this.get_CanonicalName(out displayName);
			if (ErrorHandler.Succeeded(hr))
				pbstrDisplayName = this.Project.GetOutputGroupDisplayName(displayName);
			return hr;
		}

		public virtual int get_KeyOutput(out string pbstrCanonicalName)
		{
			pbstrCanonicalName = null;
			if (keyOutput == null)
				Refresh();
			if (keyOutput == null)
			{
				pbstrCanonicalName = String.Empty;
				return VSConstants.S_FALSE;
			}
			return keyOutput.get_CanonicalName(out pbstrCanonicalName);
		}

		public virtual int get_KeyOutputObject(out IVsOutput2 ppKeyOutput)
		{
			if (keyOutput == null)
				Refresh();
			ppKeyOutput = keyOutput;
			if (ppKeyOutput == null)
				return VSConstants.S_FALSE;
			return VSConstants.S_OK;
		}

		public virtual int get_Outputs(uint celt, IVsOutput2[] rgpcfg, uint[] pcActual)
		{
			// Ensure that we are refreshed.  This is somewhat of a hack that enables project to
			// project reference scenarios to work.  Normally, output groups are populated as part
			// of build.  However, in the project to project reference case, what ends up happening
			// is that the referencing projects requests the referenced project's output group
			// before a build is done on the referenced project.
			//
			// Furthermore, the project auto toolbox manager requires output groups to be populated
			// on project reopen as well...
			//
			// In the end, this is probably the right thing to do, though -- as it keeps the output
			// groups always up to date.
			Refresh();

			// See if only the caller only wants to know the count
			if (celt == 0 || rgpcfg == null)
			{
				if (pcActual != null && pcActual.Length > 0)
					pcActual[0] = (uint)outputs.Count;
				return VSConstants.S_OK;
			}

			// Fill the array with our outputs
			uint count = 0;
			foreach (Output output in outputs)
			{
				if (rgpcfg.Length > count && celt > count && output != null)
				{
					rgpcfg[count] = output;
					++count;
				}
			}

			if (pcActual != null && pcActual.Length > 0)
				pcActual[0] = count;

			// If the number asked for does not match the number returned, return S_FALSE
			return (count == celt) ? VSConstants.S_OK : VSConstants.S_FALSE;
		}

		public virtual int get_ProjectCfg(out IVsProjectCfg2 ppIVsProjectCfg2)
		{
			ppIVsProjectCfg2 = (IVsProjectCfg2)this.projectCfg;
			return VSConstants.S_OK;
		}

		public virtual int get_Property(string pszProperty, out object pvar)
		{
			pvar = project.GetProjectProperty(pszProperty);
			return VSConstants.S_OK;
		}

		#endregion
}
}
