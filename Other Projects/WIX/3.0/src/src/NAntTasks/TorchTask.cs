//--------------------------------------------------------------------------------------------------
// <copyright file="TorchTask.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// NAnt task for the Torch linker.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.NAntTasks
{
    using System;
    using System.IO;

    using NAnt.Core;
    using NAnt.Core.Attributes;
    using NAnt.Core.Types;

    /// <summary>
    /// Represents the NAnt task for the &lt;Torch&gt; element in a NAnt script.
    /// </summary>
    [TaskName("torch")]
    public class TorchTask : SingleOutputWixTask
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private bool bindFiles;
        private FileSet localizations;
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="TorchTask"/> class.
        /// </summary>
        public TorchTask() : base("Torch.exe")
        {
            this.localizations = new FileSet();
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        /// <summary>
        /// Gets or sets the option to bind files into the wixlib.
        /// </summary>
        [TaskAttribute("bindfiles")]
        [BooleanValidator]
        public bool BindFiles
        {
            get { return this.bindFiles; }
            set { this.bindFiles = value; }
        }

        /// <summary>
        /// Gets or sets the localization files to bind.
        /// </summary>
        [BuildElement("localizations")]
        public FileSet Localizations
        {
            get { return this.localizations; }
            set { this.localizations = value; }
        }
        #endregion
 
        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        /// <summary>
        /// Writes all of the command-line parameters for the tool to a response file, one parameter per line.
        /// </summary>
        /// <param name="writer">The output writer.</param>
        protected override void WriteOptions(TextWriter writer)
        {
            base.WriteOptions(writer);

            if (this.BindFiles)
            {
                writer.WriteLine("-bf");
            }

            foreach (string fileName in this.localizations.FileNames)
            {
                writer.WriteLine("-loc \"{0}\"", fileName);
            }
        }
        #endregion
    }
}