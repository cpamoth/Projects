//-------------------------------------------------------------------------------------------------
// <copyright file="Localizer.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Parses localization files and localizes database values.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Parses localization files and localizes database values.
    /// </summary>
    public sealed class Localizer
    {
        private int codepage;
        private Hashtable variables;

        /// <summary>
        /// Instantiate a new Localizer.
        /// </summary>
        public Localizer()
        {
            this.codepage = -1;
            this.variables = new Hashtable();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets the codepage.
        /// </summary>
        /// <value>The codepage.</value>
        public int Codepage
        {
            get { return this.codepage; }
        }

        /// <summary>
        /// Add a localization file.
        /// </summary>
        /// <param name="localization">The localization file to add.</param>
        public void AddLocalization(Localization localization)
        {
            if (-1 == this.codepage)
            {
                this.codepage = localization.Codepage;
            }
            else if (-1 != this.codepage && -1 != localization.Codepage && this.codepage != localization.Codepage)
            {
                this.OnMessage(WixErrors.DuplicateLocalizedCodepage(null, this.codepage));
            }

            foreach (WixVariableRow wixVariableRow in localization.Variables)
            {
                WixVariableRow existingWixVariableRow = (WixVariableRow)this.variables[wixVariableRow.Id];

                if (null == existingWixVariableRow || (existingWixVariableRow.Overridable && !wixVariableRow.Overridable))
                {
                    this.variables.Add(wixVariableRow.Id, wixVariableRow);
                }
                else if (!wixVariableRow.Overridable)
                {
                    this.OnMessage(WixErrors.DuplicateLocalizationIdentifier(wixVariableRow.SourceLineNumbers, wixVariableRow.Id));
                }
            }
        }

        /// <summary>
        /// Get a localized data value.
        /// </summary>
        /// <param name="id">The name of the localization variable.</param>
        /// <returns>The localized data value or null if it wasn't found.</returns>
        public string GetLocalizedValue(string id)
        {
            WixVariableRow wixVariableRow = (WixVariableRow)this.variables[id];

            if (null != wixVariableRow)
            {
                return wixVariableRow.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        private void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != this.Message)
            {
                this.Message(this, mea);
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }
    }
}