//-------------------------------------------------------------------------------------------------
// <copyright file="PreprocessorExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base preprocessor extension.  Any of these methods can be overridden to change
// the behavior of the preprocessor.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Base class for creating a preprocessor extension.
    /// </summary>
    public abstract class PreprocessorExtension
    {
        private PreprocessorCore core;

        /// <summary>
        /// Gets or sets the preprocessor core for the extension.
        /// </summary>
        /// <value>Preprocessor core for the extension.</value>
        public PreprocessorCore Core
        {
            get { return this.core; }
            set { this.core = value; }
        }

        /// <summary>
        /// Gets or sets the variable prefixes for the extension.
        /// </summary>
        /// <value>The variable prefixes for the extension.</value>
        public virtual string[] Prefixes
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the value of a variable whose prefix matches the extension.
        /// </summary>
        /// <param name="prefix">The prefix of the variable to be processed by the extension.</param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The value of the variable or null if the variable is undefined.</returns>
        public virtual string GetVariableValue(string prefix, string name)
        {
            return null;
        }

        /// <summary>
        /// Evaluates a function defined in the extension.
        /// </summary>
        /// <param name="prefix">The prefix of the function to be processed by the extension.</param>
        /// <param name="function">The name of the function.</param>
        /// <param name="args">The list of arguments.</param>
        /// <returns>The value of the function or null if the function is not defined.</returns>
        public virtual string EvaluateFunction(string prefix, string function, string[] args)
        {
            return null;
        }

        /// <summary>
        /// Called at the beginning of the preprocessing of a source file.
        /// </summary>
        public virtual void InitializePreprocess()
        {
        }

        /// <summary>
        /// Preprocess a document after normal preprocessing has completed.
        /// </summary>
        /// <param name="document">The document to preprocess.</param>
        public virtual void PreprocessDocument(XmlDocument document)
        {
        }

        /// <summary>
        /// Preprocesses a parameter.
        /// </summary>
        /// <param name="name">Name of parameter that matches extension.</param>
        /// <returns>The value of the parameter after processing.</returns>
        /// <remarks>By default this method will cause an error if its called.</remarks>
        public virtual string PreprocessParameter(string name)
        {
            return null;
        }

        /// <summary>
        /// Determines if the variable used in a foreach is legal.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <returns>true if the variable is allowed; false otherwise.</returns>
        /// <remarks>
        /// We use this to lockdown the foreach loops to certain variable
        /// values since they can easily be abused to obfuscate code.
        /// </remarks>
        public virtual bool IsForeachVariableAllowed(string variableName)
        {
            return false;
        }
    }
}
