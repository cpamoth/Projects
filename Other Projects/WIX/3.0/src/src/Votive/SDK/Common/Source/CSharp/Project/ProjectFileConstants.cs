/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion


namespace Microsoft.VisualStudio.Package
{

	public static class MsBuildTarget
	{
		public const string ResolveProjectReferences = "ResolveProjectReferences";
		public const string ResolveAssemblyReferences = "ResolveAssemblyReferences";
		public const string ResolveComReferences = "ResolveComReferences";
		public const string Build = "Build";
		public const string Rebuild = "ReBuild";
		public const string Clean = "Clean";
	}

	public static class MsBuildGeneratedItemType
	{
		public const string ReferenceCopyLocalPaths = "ReferenceCopyLocalPaths";
		public const string ComReferenceWrappers = "ComReferenceWrappers";
	}

	public static class ProjectFileConstants
	{
		public const string Include = "Include";
		public const string Name = "Name";
		public const string HintPath = "HintPath";
		public const string AssemblyName = "AssemblyName";
		public const string Project = "Project";
		public const string LinkedIntoProjectAt = "LinkedIntoProjectAt";
		public const string TypeGuid = "TypeGuid";
		public const string InstanceGuid = "InstanceGuid";
		public const string Private = "Private";
		public const string ProjectReference = "ProjectReference";
		public const string Reference = "Reference";
		public const string WebReference = "WebReference";
		public const string WebReferenceFolder = "WebReferenceFolder";
		public const string Folder = "Folder";
		public const string Content = "Content";
		public const string EmbeddedResource = "EmbeddedResource";
		public const string RootNamespace = "RootNamespace";
		public const string OutputType = "OutputType";
		public const string SubType = "SubType";
		public const string DependentUpon = "DependentUpon";
		public const string Compile = "Compile";
		public const string ReferencePath = "ReferencePath";
		public const string ResolvedProjectReferencePaths = "ResolvedProjectReferencePaths";
		public const string Configuration = "Configuration";
		public const string Platform = "Platform";
		public const string AvailablePlatforms = "AvailablePlatforms";
		public const string BuildVerbosity = "BuildVerbosity";
		public const string Template = "Template";
		public const string SubProject = "SubProject";
		public const string BuildAction = "BuildAction";
		public const string COMReference = "COMReference";
		public const string Guid = "Guid";
		public const string VersionMajor = "VersionMajor";
		public const string VersionMinor = "VersionMinor";
		public const string Lcid = "Lcid";
		public const string Isolated = "Isolated";
		public const string WrapperTool = "WrapperTool";
		public const string BuildingInsideVisualStudio = "BuildingInsideVisualStudio";
		public const string SccProjectName = "SccProjectName";
		public const string SccLocalPath = "SccLocalPath";
		public const string SccAuxPath = "SccAuxPath";
		public const string SccProvider = "SccProvider";
		public const string ProjectGuid = "ProjectGuid";
		public const string ProjectTypeGuids = "ProjectTypeGuids";
		public const string Generator = "Generator";
		public const string CustomToolNamespace = "CustomToolNamespace";
		public const string FlavorProperties = "FlavorProperties";
		public const string VisualStudio = "VisualStudio";
		public const string User = "User";
	}

	public static class ProjectFileAttributeValue
	{
		public const string Code = "Code";
		public const string Form = "Form";
		public const string Component = "Component";
		public const string Designer = "Designer";
		public const string UserControl = "UserControl";
	}

	internal static class ProjectFileValues
	{
		internal const string AnyCPU = "AnyCPU";
	}

	public enum WrapperToolAttributeValue
	{
		Primary,
		TlbImp
	}

	public static class DefaultSortOrderNode
	{
		public const int HierarchyNode = 1000;
		public const int FolderNode = 500;
		public const int NestedProjectNode = 200;
		public const int ReferenceContainerNode = 300;
	}

}
