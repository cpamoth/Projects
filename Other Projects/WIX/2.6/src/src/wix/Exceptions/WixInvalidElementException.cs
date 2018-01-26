//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidElementException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX invalid element exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX invalid element exception.
    /// </summary>
    public class WixInvalidElementException : WixException
    {
        private string element;
        private string id;
        private string detail;

        /// <summary>
        /// Instantiate new WixInvalidElementException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="elementName">Name of the invalid element.</param>
        /// <param name="detail">Detail about the exception.</param>
        public WixInvalidElementException(SourceLineNumberCollection sourceLineNumbers, string elementName, string detail) :
            this(sourceLineNumbers, elementName, detail, null)
        {
        }

        /// <summary>
        /// Instantiate new WixInvalidElementException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line information of the exception.</param>
        /// <param name="elementName">Name of the invalid element.</param>
        /// <param name="detail">Detail about the exception.</param>
        /// <param name="id">Id of the invalid element.</param>
        public WixInvalidElementException(SourceLineNumberCollection sourceLineNumbers, string elementName, string detail, string id) :
            base(sourceLineNumbers, WixExceptionType.InvalidElement)
        {
            this.element = elementName;
            this.id = id;
            this.detail = detail;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get
            {
                if (null == this.id)
                {
                    return String.Format("The element: {0} is invalid, detail: {1}", this.element, this.detail);
                }
                else
                {
                    return String.Format("The element: {0}[{1}] is invalid, detail: {2}", this.element, this.id, this.detail);
                }
            }
        }
    }
}
