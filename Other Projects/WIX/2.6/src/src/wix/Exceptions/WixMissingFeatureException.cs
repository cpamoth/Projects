//-------------------------------------------------------------------------------------------------
// <copyright file="WixMissingFeatureException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Exception for components that are not in features (but need to be).
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// Exception for components that are not in features (but need to be).
    /// </summary>
    public class WixMissingFeatureException : WixException
    {
        private FeatureBacklink blink;

        /// <summary>
        /// Instantiate a new WixPreprocessorException.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number trace to where the exception occured.</param>
        /// <param name="blink">FeatureBacklink that is missing a feature to link with.</param>
        public WixMissingFeatureException(SourceLineNumberCollection sourceLineNumbers, FeatureBacklink blink) :
            base(sourceLineNumbers, WixExceptionType.MissingFeature)
        {
            this.blink = blink;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The error message that explains the reason for the exception, or an empty string("").</value>
        public override string Message
        {
            get { return String.Format("Component '{0}' is not assigned to a feature.  The component's {1} '{2}' requires it to be assigned to a feature.", this.blink.Component, this.blink.Type.ToString(), this.blink.Target); }
        }
    }
}

