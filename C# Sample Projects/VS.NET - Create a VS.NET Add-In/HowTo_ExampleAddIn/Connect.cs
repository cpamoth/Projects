
	using System;
	using Microsoft.Office.Core;
	using Extensibility;
	using System.Runtime.InteropServices;
	using EnvDTE;
	using System.Windows.Forms;

	#region Read me for Add-in installation and setup information.
	// When run, the Add-in wizard prepared the registry for the Add-in.
	// At a later time, if the Add-in becomes unavailable for reasons such as:
	//   1) You moved this project to a computer other than which is was originally created on.
	//   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
	//   3) Registry corruption.
	// you will need to re-register the Add-in by building the MyAddin21Setup project 
	// by right clicking the project in the Solution Explorer, then choosing install.
	#endregion
	
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("2AEFC7ED-4724-44BE-8CAC-A1C24508AAF3"), ProgId("HowTo_ExampleAddIn.Connect")]
	public class Connect : Object, Extensibility.IDTExtensibility2
	{
		private EnvDTE.Events m_Events;
		private EnvDTE.SolutionEvents m_SolutionEvents;
		private EnvDTE.BuildEvents m_BuildEvents;

		public Connect()
		{

			
		
		}
		
		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom)
		{
			applicationObject = (_DTE)application;
			addInInstance = (AddIn)addInInst;
			
			// Add code for How-To.
			// Initialize all the variables that contain the events we're looking for.
			// Get the events object that contains information about all of the 
			//   extensibility events.
			m_Events = ((DTE)(application)).Events;
			// Initialize a variable to track SolutionEvents
			m_SolutionEvents = m_Events.SolutionEvents;
			// Initialize a variable to track BuildEvents
			m_BuildEvents = m_Events.BuildEvents;

			m_BuildEvents.OnBuildBegin +=new _dispBuildEvents_OnBuildBeginEventHandler(m_BuildEvents_OnBuildBegin);
			m_SolutionEvents.AfterClosing +=new _dispSolutionEvents_AfterClosingEventHandler(m_SolutionEvents_AfterClosing);
			m_SolutionEvents.Opened +=new _dispSolutionEvents_OpenedEventHandler(m_SolutionEvents_Opened);
			m_SolutionEvents.ProjectAdded +=new _dispSolutionEvents_ProjectAddedEventHandler(m_SolutionEvents_ProjectAdded);
			m_SolutionEvents.ProjectRemoved +=new _dispSolutionEvents_ProjectRemovedEventHandler(m_SolutionEvents_ProjectRemoved);
			m_SolutionEvents.ProjectRenamed +=new _dispSolutionEvents_ProjectRenamedEventHandler(m_SolutionEvents_ProjectRenamed);

			
		}

		/// <summary>
		///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///     Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///      Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///      Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///      Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///      Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom)
		{
		}
			
		// This subroutine is fired when a project or solution is built or rebuilt.

		private void m_BuildEvents_OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action) 
		{
			WriteEvent("Build Beginning. Action: " + Action.ToString());

		}

		// This subroutine is fired when a solution is closing. It simply makes a
		//   log entry of the event.

		private void m_SolutionEvents_AfterClosing() 
		{
			WriteEvent("Solution Closed: " + applicationObject.Solution.FullName);

		}

		// This subroutine is fired when a solution is opened. It simply makes a
		//   log entry of the event.

		private void m_SolutionEvents_Opened() 
		{
			WriteEvent("Solution Opened: " + applicationObject.Solution.FullName);

		}

		// This subroutine is fired when a project is added to a solution. 
		//   It simply makes a log entry of the event.

		private void m_SolutionEvents_ProjectAdded(EnvDTE.Project Project) 
		{
			WriteEvent("Project Added: " + Project.FullName);
		}

		// This subroutine is fired when a project is removed from a solution. 
		//   It simply makes a log entry of the event.

		private void m_SolutionEvents_ProjectRemoved(EnvDTE.Project Project) 
		{
			WriteEvent("Project Removed: " + Project.FullName);

		}

		// This subroutine is fired when a project in a solution is renamed. 
		//   It simply makes a log entry of the event.

		private void m_SolutionEvents_ProjectRenamed(EnvDTE.Project Project, string OldName) 
		{
			WriteEvent("Project Renamed: " + Project.FullName + " from " + OldName);

		}

		private _DTE applicationObject;
		private AddIn addInInstance;

		// Code for How-To
		// This subroutine writes events to a File in the System directory.  
		//   It connects and disconnects to the file rapidly possible, since       
		//   the file may be shared by many running instances of VS.NET

		private void WriteEvent(string message)
		{
			try 
			{
				// Create a StreamWriter.
				System.IO.StreamWriter mySW = new System.IO.StreamWriter(Environment.SystemDirectory +
					@"\IdeLog.txt", true);
				// Write the time to the output file.
				mySW.WriteLine(System.DateTime.Now.ToShortDateString() + " - " +
					System.DateTime.Now.ToShortTimeString());
				// Write the message and a carriage return to the file.
				mySW.WriteLine(message);
				mySW.WriteLine(Environment.NewLine);
				// Flush the file and close it.
				mySW.Flush();
				mySW.Close();
			} 
			catch
			{
				// Report the error to the user.
				MessageBox.Show("The log could not be written to.", 
					"How-To Create a VS.NET Add-in",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

		}
		
		
	}