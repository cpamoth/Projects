//--------------------------------------------------------------------------------------------------
// <copyright file="LightTask.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// NAnt task for the light linker.
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
    /// Represents the NAnt task for the &lt;light&gt; element in a NAnt script.
    /// </summary>
    [TaskName("light")]
    public class LightTask : SingleOutputWixTask
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private string cultures;
        private FileSet localizations;
        private string suppressICEs;
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="LightTask"/> class.
        /// </summary>
        public LightTask() : base("light.exe")
        {
            this.localizations = new FileSet();
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        /// <summary>
        /// Gets or sets the cultures to use.
        /// </summary>
        [TaskAttribute("cultures")]
        public string Cultures
        {
            get { return this.cultures; }
            set { this.cultures = value; }
        }

        /// <summary>
        /// Gets or sets the localization files to use.
        /// </summary>
        [BuildElement("localizations")]
        public FileSet Localizations
        {
            get { return this.localizations; }
            set { this.localizations = value; }
        }

        /// <summary>
        /// Gets or sets the option to suppress particular ICEs.
        /// </summary>
        [TaskAttribute("suppressices")]
        public string SuppressICEs
        {
            get { return this.suppressICEs; }
            set { this.suppressICEs = value; }
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

            if (null != this.cultures)
            {
                writer.WriteLine("-cultures:{0}", this.cultures);
            }

            foreach (string fileName in this.localizations.FileNames)
            {
                writer.WriteLine("-loc \"{0}\"", fileName);
            }

            if (this.suppressICEs != null)
            {
                foreach (string suppressICE in this.suppressICEs.Split(';'))
                {
                    writer.WriteLine("-sice:{0}", suppressICE);
                }
            }
        }
        #endregion
    }
}