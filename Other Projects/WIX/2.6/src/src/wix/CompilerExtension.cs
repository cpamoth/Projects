//-------------------------------------------------------------------------------------------------
// <copyright file="CompilerExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base compiler extension.  Any of these methods can be overridden to change
// the behavior of the compiler.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Base class for creating a compiler extension.
    /// </summary>
    public abstract class CompilerExtension : SchemaExtension
    {
        private CompilerCore compilerCore;

        /// <summary>
        /// Gets or sets the compiler core for the extension.
        /// </summary>
        /// <value>Compiler core for the extension.</value>
        public CompilerCore Core
        {
            get { return this.compilerCore; }
            set { this.compilerCore = value; }
        }

        /// <summary>
        /// Called at the beginning of the compilation of a source file.
        /// </summary>
        public virtual void InitializeCompile()
        {
        }

        /// <summary>
        /// Called at the end of the compilation of a source file.
        /// </summary>
        public virtual void FinalizeCompile()
        {
        }

        /// <summary>
        /// Processes an attribute for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of attribute.</param>
        /// <param name="attribute">Attribute to process.</param>
        public abstract void ParseAttribute(SourceLineNumberCollection sourceLineNumbers, XmlElement parentElement, XmlAttribute attribute);

        /// <summary>
        /// Processes an element for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="element">Element to process.</param>
        public abstract void ParseElement(SourceLineNumberCollection sourceLineNumbers, XmlElement parentElement, XmlElement element);
    }
}
