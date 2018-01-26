This directory is a copy of parts of the September 2006 Visual Studio SDK.

From: C:\Program Files\Visual Studio 2005 SDK\2006.09\VisualStudioIntegration\Common\Source\CSharp\Project
  To: .\SDK\Common\Source\CSharp\Project

From: C:\Program Files\Visual Studio 2005 SDK\2006.09\VisualStudioIntegration\Tools\Build
  To: .\SDK\Tools\Build

There is currently a bug in the SDK that does not allow adding Wixlibs as a reference. The fix is
simple and involves changing just a few lines of code in the supplied SDK sources.

SDK\Common\Source\CSharp\Project\ReferenceContainerNode.cs
----------------------------------------------------------
* Add the following method to the class:

		/// <summary>
		/// Creates a project-specific reference node that isn't an assembly reference, COM reference,
		/// or project reference.
		/// </summary>
		protected virtual ReferenceNode CreateOtherReferenceNode(ProjectElement reference)
		{
			return null;
		}

* LoadReferencesFromBuildProject - You need to call the method added above inside of the last
  else block:

					else if (isProjectReference)
					{
						node = this.CreateProjectReferenceNode(element);
					}
					else
					{
-->					// JRock: Added support for other references
-->					node = this.CreateOtherReferenceNode(element);
					}

SDK\Tools\Build\Microsoft.VsSDK.targets
---------------------------------------
* For the 4 Zip tasks, remove the $(MSBuildProjectDirectory) from the TargetPath attributes so that
  they all read like this:

  TargetPath=$(IntermediateOutputPath)
