//--------------------------------------------------------------------------------------------------
// <copyright file="Transform.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the Transform class.
// </summary>
//--------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.NAntTasks
{
    using System;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    /// <summary>
    /// Represents the transform element for the <see cref="PyroTask"/> task.
    /// </summary>
    [ElementName("transform")]
    public class Transform : Element
    {
        // =========================================================================================
        // Member Variables
        // =========================================================================================

        private string filePath;
        private string baseline;

        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// </summary>
        public Transform()
        {
        }

        // =========================================================================================
        // Properties
        // =========================================================================================

        /// <summary>
        /// Gets or sets the path to the baseline file.
        /// </summary>
        [TaskAttribute("baseline", Required = true)]
        public string Baseline
        {
            get { return this.baseline; }
            set { this.baseline = value; }
        }

        /// <summary>
        /// Gets or sets the path to the transform file.
        /// </summary>
        [TaskAttribute("filepath", Required = true)]
        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
    }
}