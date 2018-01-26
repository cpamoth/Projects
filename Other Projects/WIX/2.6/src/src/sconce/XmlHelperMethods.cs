//-------------------------------------------------------------------------------------------------
// <copyright file="XmlHelperMethods.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Helper methods for the working with XML.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using System.Xml;

    /// <summary>
    /// Contains helper methods for working with XML.
    /// </summary>
    public sealed class XmlHelperMethods
    {
        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Prevent direct instantiation of this class.
        /// </summary>
        private XmlHelperMethods()
        {
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        /// <summary>
        /// Gets an attribute value, returning the default value if it's not present.
        /// </summary>
        /// <param name="node">The <see cref="XmlNode"/> from which to retrieve the attribute.</param>
        /// <param name="attributeName">The name of the attribute to retrieve.</param>
        /// <param name="defaultValue">The value that will be returned if the attribute is not found or cannot be converted to the target type.</param>
        /// <returns>The attribute value converted to the target type.</returns>
        public static Guid GetAttributeGuid(XmlNode node, string attributeName, Guid defaultValue)
        {
            Guid returnValue = defaultValue;
            XmlAttribute attribute = node.Attributes[attributeName];
            if (attribute != null && attribute.Value.Length > 0)
            {
                try
                {
                    returnValue = XmlConvert.ToGuid(attribute.Value);
                }
                catch (FormatException)
                {
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Gets an attribute value, returning the default value if it's not present.
        /// </summary>
        /// <param name="node">The <see cref="XmlNode"/> from which to retrieve the attribute.</param>
        /// <param name="attributeName">The name of the attribute to retrieve.</param>
        /// <param name="defaultValue">The value that will be returned if the attribute is not found or cannot be converted to the target type.</param>
        /// <returns>The attribute value converted to the target type.</returns>
        public static string GetAttributeString(XmlNode node, string attributeName, string defaultValue)
        {
            string returnValue = defaultValue;
            XmlAttribute attribute = node.Attributes[attributeName];
            if (attribute != null)
            {
                returnValue = attribute.Value;
            }
            return returnValue;
        }
        #endregion
	}
}
