//-------------------------------------------------------------------------------------------------
// <copyright file="WixUnresolvedReferenceException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX unresolved reference exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX unresolved reference exception.
    /// </summary>
    public class WixUnresolvedReferenceException : WixException
    {
        private Section section;
        private Reference reference;

        /// <summary>
        /// Instantiate a new WixUnresolvedReferenceException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="section">Section with a missing reference.</param>
        /// <param name="reference">The missing reference.</param>
        internal WixUnresolvedReferenceException(SourceLineNumberCollection sourceLineNumbers, Section section, Reference reference) :
            this(sourceLineNumbers, section, reference, null)
        {
        }

        /// <summary>
        /// Instantiate a new WixUnresolvedReferenceException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="section">Section with a missing reference.</param>
        /// <param name="reference">The missing reference.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        internal WixUnresolvedReferenceException(SourceLineNumberCollection sourceLineNumbers, Section section, Reference reference, Exception innerException) :
            base(sourceLineNumbers, WixExceptionType.UnresolvedReference, innerException)
        {
            this.section = section;
            this.reference = reference;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Format("Unresolved reference: '{0}' in section: '{1}'", this.reference.SymbolicName, this.section.Id); }
        }
    }
}
