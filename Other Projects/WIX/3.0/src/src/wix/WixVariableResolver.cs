//-------------------------------------------------------------------------------------------------
// <copyright file="WixVariableResolver.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX variable resolver.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// WiX variable resolver.
    /// </summary>
    public sealed class WixVariableResolver
    {
        private bool encounteredError;
        private Hashtable wixVariables;

        /// <summary>
        /// Instantiate a new WixVariableResolver.
        /// </summary>
        public WixVariableResolver()
        {
            this.wixVariables = new Hashtable();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Gets whether an error was encountered while resolving variables.
        /// </summary>
        /// <value>Whether an error was encountered while resolving variables.</value>
        public bool EncounteredError
        {
            get { return this.encounteredError; }
        }

        /// <summary>
        /// Add a variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public void AddVariable(string name, string value)
        {
            if (!this.wixVariables.Contains(name))
            {
                this.wixVariables.Add(name, value);
            }
            else
            {
                this.OnMessage(WixErrors.WixVariableCollision(null, name));
            }
        }

        /// <summary>
        /// Add a variable.
        /// </summary>
        /// <param name="wixVariableRow">The WixVariableRow to add.</param>
        public void AddVariable(WixVariableRow wixVariableRow)
        {
            if (!this.wixVariables.Contains(wixVariableRow.Id))
            {
                this.wixVariables.Add(wixVariableRow.Id, wixVariableRow.Value);
            }
            else if (!wixVariableRow.Overridable) // collision
            {
                this.OnMessage(WixErrors.WixVariableCollision(wixVariableRow.SourceLineNumbers, wixVariableRow.Id));
            }
        }

        /// <summary>
        /// Resolve the wix variables in a value.
        /// </summary>
        /// <param name="localizer">The localizer.</param>
        /// <param name="sourceLineNumbers">The source line information for the value.</param>
        /// <param name="value">The value to resolve.</param>
        /// <param name="localizationOnly">true to only resolve localization variables; false otherwise.</param>
        /// <returns>The resolved value.</returns>
        public string ResolveVariables(Localizer localizer, SourceLineNumberCollection sourceLineNumbers, string value, bool localizationOnly)
        {
            bool isDefault = false;

            return this.ResolveVariables(localizer, sourceLineNumbers, value, localizationOnly, ref isDefault);
        }

        /// <summary>
        /// Resolve the wix variables in a value.
        /// </summary>
        /// <param name="localizer">The localizer.</param>
        /// <param name="sourceLineNumbers">The source line information for the value.</param>
        /// <param name="value">The value to resolve.</param>
        /// <param name="localizationOnly">true to only resolve localization variables; false otherwise.</param>
        /// <param name="isDefault">true if the resolved value was the default.</param>
        /// <returns>The resolved value.</returns>
        public string ResolveVariables(Localizer localizer, SourceLineNumberCollection sourceLineNumbers, string value, bool localizationOnly, ref bool isDefault)
        {
            MatchCollection matches = Common.WixVariableRegex.Matches(value);

            // the value is the default unless its substituted further down
            isDefault = true;

            if (0 < matches.Count)
            {
                StringBuilder sb = new StringBuilder(value);

                // notice how this code walks backward through the list
                // because it modifies the string as we through it
                for (int i = matches.Count - 1; 0 <= i; i--)
                {
                    string variableNamespace = matches[i].Groups["namespace"].Value;
                    string variableId = matches[i].Groups["name"].Value;
                    string variableDefaultValue = null;

                    // get the default value if one was specified
                    if (matches[i].Groups["value"].Success)
                    {
                        variableDefaultValue = matches[i].Groups["value"].Value;

                        // localization variables to not support inline default values
                        if ("loc" == variableNamespace)
                        {
                            this.OnMessage(WixErrors.IllegalInlineLocVariable(sourceLineNumbers, variableId, variableDefaultValue));
                        }
                    }

                    // check for an escape sequence of !! indicating the match is not a variable expression
                    if (0 < matches[i].Index && '!' == sb[matches[i].Index - 1])
                    {
                        if (!localizationOnly)
                        {
                            sb.Remove(matches[i].Index - 1, 1);
                        }
                    }
                    else
                    {
                        string resolvedValue = null;

                        if ("loc" == variableNamespace)
                        {
                            // warn about deprecated syntax of $(loc.var)
                            if ('$' == sb[matches[i].Index])
                            {
                                this.OnMessage(WixWarnings.DeprecatedLocalizationVariablePrefix(sourceLineNumbers, variableId));
                            }

                            if (null != localizer)
                            {
                                resolvedValue = localizer.GetLocalizedValue(variableId);
                            }
                        }
                        else if (!localizationOnly && "wix" == variableNamespace)
                        {
                            // illegal syntax of $(wix.var)
                            if ('$' == sb[matches[i].Index])
                            {
                                this.OnMessage(WixErrors.IllegalWixVariablePrefix(sourceLineNumbers, variableId));
                            }
                            else
                            {
                                // default the resolved value to the inline value if one was specified
                                if (null != variableDefaultValue)
                                {
                                    resolvedValue = variableDefaultValue;
                                }

                                if (this.wixVariables.Contains(variableId))
                                {
                                    resolvedValue = (string)this.wixVariables[variableId];
                                    isDefault = false;
                                }
                            }
                        }

                        // insert the resolved value if it was found or display an error
                        if (null != resolvedValue)
                        {
                            sb.Remove(matches[i].Index, matches[i].Length);
                            sb.Insert(matches[i].Index, resolvedValue);
                        }
                        else if ("loc" == variableNamespace) // unresolved loc variable
                        {
                            this.OnMessage(WixErrors.LocalizationVariableUnknown(sourceLineNumbers, variableId));
                        }
                        else if (!localizationOnly && "wix" == variableNamespace) // unresolved wix variable
                        {
                            this.OnMessage(WixErrors.WixVariableUnknown(sourceLineNumbers, variableId));
                        }
                    }
                }

                value = sb.ToString();
            }

            return value;
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        private void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (mea is WixErrorEventArgs)
            {
                this.encounteredError = true;
            }

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
