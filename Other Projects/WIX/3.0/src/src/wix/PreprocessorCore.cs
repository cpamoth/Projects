//-------------------------------------------------------------------------------------------------
// <copyright file="PreprocessorCore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The preprocessor core - handles functionality shared between the preprocessor and its extensions.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    /// <summary>
    /// The preprocessor core.
    /// </summary>
    public sealed class PreprocessorCore : IMessageHandler
    {
        private static readonly char[] variableSplitter = new char[] { '.' };
        private static readonly char[] argumentSplitter = new char[] { ',' };

        private bool encounteredError;
        private Hashtable extensionsByPrefix;
        private string sourceFile;
        private Hashtable variables;

        /// <summary>
        /// Instantiate a new PreprocessorCore.
        /// </summary>
        /// <param name="extensionsByPrefix">The extensions indexed by their prefixes.</param>
        /// <param name="messageHandler">The message handler.</param>
        /// <param name="sourceFile">The source file being preprocessed.</param>
        /// <param name="variables">The variables defined prior to preprocessing.</param>
        internal PreprocessorCore(Hashtable extensionsByPrefix, MessageEventHandler messageHandler, string sourceFile, Hashtable variables)
        {
            this.extensionsByPrefix = extensionsByPrefix;
            this.MessageHandler = messageHandler;
            this.sourceFile = Path.GetFullPath(sourceFile);

            this.variables = new Hashtable();
            foreach (DictionaryEntry entry in variables)
            {
                this.AddVariable(null, (string)entry.Key, (string)entry.Value);
            }
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        private event MessageEventHandler MessageHandler;

        /// <summary>
        /// Gets whether the core encoutered an error while processing.
        /// </summary>
        /// <value>Flag if core encountered and error during processing.</value>
        public bool EncounteredError
        {
            get { return this.encounteredError; }
        }

        /// <summary>
        /// Replaces parameters in the source text.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information for the function.</param>
        /// <param name="value">Text that may contain parameters to replace.</param>
        /// <returns>Text after parameters have been replaced.</returns>
        public string PreprocessString(SourceLineNumberCollection sourceLineNumbers, string value)
        {
            StringBuilder sb = new StringBuilder();
            int currentPosition = 0;
            int end = 0;

            while (-1 != (currentPosition = value.IndexOf('$', end)))
            {
                if (end < currentPosition)
                {
                    sb.Append(value, end, currentPosition - end);
                }

                end = currentPosition + 1;
                string remainder = value.Substring(end);
                if (remainder.StartsWith("$"))
                {
                    sb.Append("$");
                    end++;
                }
                else if (remainder.StartsWith("(loc."))
                {
                    currentPosition = remainder.IndexOf(')');
                    if (-1 == currentPosition)
                    {
                        throw new WixException(WixErrors.InvalidPreprocessorVariable(sourceLineNumbers, remainder));
                    }

                    sb.Append("$");   // just put the resource reference back as was
                    sb.Append(remainder, 0, currentPosition + 1);

                    end += currentPosition + 1;
                }
                else if (remainder.StartsWith("("))
                {
                    int openParenCount = 1;
                    int closingParenCount = 0;
                    bool isFunction = false;
                    bool foundClosingParen = false;

                    // find the closing paren
                    int closingParenPosition;
                    for (closingParenPosition = 1; closingParenPosition < remainder.Length; closingParenPosition++)
                    {
                        switch (remainder[closingParenPosition])
                        {
                            case '(':
                                openParenCount++;
                                isFunction = true;
                                break;
                            case ')':
                                closingParenCount++;
                                break;
                        }
                        if (openParenCount == closingParenCount)
                        {
                            foundClosingParen = true;
                            break;
                        }
                    }

                    // move the currentPosition to the closing paren
                    currentPosition += closingParenPosition;

                    if (!foundClosingParen)
                    {
                        if (isFunction)
                        {
                            throw new WixException(WixErrors.InvalidPreprocessorFunction(sourceLineNumbers, remainder));
                        }
                        else
                        {
                            throw new WixException(WixErrors.InvalidPreprocessorVariable(sourceLineNumbers, remainder));
                        }
                    }

                    string subString = remainder.Substring(1, closingParenPosition - 1);
                    string result = null;
                    if (isFunction)
                    {
                        result = this.EvaluateFunction(sourceLineNumbers, subString);
                    }
                    else
                    {
                        result = this.GetVariableValue(sourceLineNumbers, subString, false);
                    }

                    if (null == result)
                    {
                        if (isFunction)
                        {
                            throw new WixException(WixErrors.UndefinedPreprocessorFunction(sourceLineNumbers, subString));
                        }
                        else
                        {
                            throw new WixException(WixErrors.UndefinedPreprocessorVariable(sourceLineNumbers, subString));
                        }
                    }
                    sb.Append(result);
                    end += closingParenPosition + 1;
                }
                else   // just a floating "$" so put it in the final string (i.e. leave it alone) and keep processing
                {
                    sb.Append('$');
                }
            }

            if (end < value.Length)
            {
                sb.Append(value.Substring(end));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Evaluate a function.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information for the function.</param>
        /// <param name="function">The function expression including the prefix and name.</param>
        /// <returns>The function value.</returns>
        public string EvaluateFunction(SourceLineNumberCollection sourceLineNumbers, string function)
        {
            string[] prefixParts = function.Split(variableSplitter, 2);
            // Check to make sure there are 2 parts and neither is an empty string.
            if (2 != prefixParts.Length || 0 >= prefixParts[0].Length || 0 >= prefixParts[1].Length)
            {
                throw new WixException(WixErrors.InvalidPreprocessorFunction(sourceLineNumbers, function));
            }
            string prefix = prefixParts[0];

            string[] functionParts = prefixParts[1].Split(new char[] { '(' }, 2);
            // Check to make sure there are 2 parts, neither is an empty string, and the second part ends with a closing paren.
            if (2 != functionParts.Length || 0 >= functionParts[0].Length || 0 >= functionParts[1].Length || !functionParts[1].EndsWith(")"))
            {
                throw new WixException(WixErrors.InvalidPreprocessorFunction(sourceLineNumbers, function));
            }
            string functionName = functionParts[0];

            // Remove the trailing closing paren.
            string allArgs = functionParts[1].Substring(0, functionParts[1].Length - 1);

            // Parse the arguments and preprocess them.
            string[] args = allArgs.Split(argumentSplitter);
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = this.PreprocessString(sourceLineNumbers, args[i].Trim());
            }

            return this.EvaluateFunction(sourceLineNumbers, prefix, functionName, args);
        }

        /// <summary>
        /// Evaluate a function.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information for the function.</param>
        /// <param name="prefix">The function prefix.</param>
        /// <param name="function">The function name.</param>
        /// <param name="args">The arguments for the function.</param>
        /// <returns>The function value or null if the function is not defined.</returns>
        public string EvaluateFunction(SourceLineNumberCollection sourceLineNumbers, string prefix, string function, string[] args)
        {
            if (null == prefix)
            {
                throw new ArgumentNullException("prefix");
            }

            if (null == function)
            {
                throw new ArgumentNullException("name");
            }

            if (0 == function.Length)
            {
                throw new ArgumentException("Empty function call.", "function");
            }

            switch (prefix)
            {
                case "fun":
                    switch (function)
                    {
                        // Add any core defined functions here
                        default:
                            return null;
                    }
                default:
                    PreprocessorExtension extension = (PreprocessorExtension)this.extensionsByPrefix[prefix];
                    if (null != extension)
                    {
                        try
                        {
                            return extension.EvaluateFunction(prefix, function, args);
                        }
                        catch (Exception e)
                        {
                            throw new WixException(WixErrors.PreprocessorExtensionEvaluateFunctionFailed(sourceLineNumbers, prefix, function, String.Join(",", args), e.Message));
                        }
                    }
                    else
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Get the value of a variable expression like var.name.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information for the variable.</param>
        /// <param name="variable">The variable expression including the optional prefix and name.</param>
        /// <param name="allowMissingPrefix">true to allow the variable prefix to be missing.</param>
        /// <returns>The variable value.</returns>
        public string GetVariableValue(SourceLineNumberCollection sourceLineNumbers, string variable, bool allowMissingPrefix)
        {
            string[] parts = variable.Split(variableSplitter, 2);

            if (1 == parts.Length) // missing prefix
            {
                if (allowMissingPrefix)
                {
                    return this.GetVariableValue(sourceLineNumbers, "var", parts[0]);
                }
                else
                {
                    throw new WixException(WixErrors.InvalidPreprocessorVariable(sourceLineNumbers, variable));
                }
            }
            else
            {
                // check for empty variable name
                if (0 < parts[1].Length)
                {
                    return this.GetVariableValue(sourceLineNumbers, parts[0], parts[1]);
                }
                else
                {
                    throw new WixException(WixErrors.InvalidPreprocessorVariable(sourceLineNumbers, variable));
                }
            }
        }

        /// <summary>
        /// Get the value of a variable.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information for the function.</param>
        /// <param name="prefix">The variable prefix.</param>
        /// <param name="name">The variable name.</param>
        /// <returns>The variable value or null if the variable is not set.</returns>
        public string GetVariableValue(SourceLineNumberCollection sourceLineNumbers, string prefix, string name)
        {
            if (null == prefix)
            {
                throw new ArgumentNullException("prefix");
            }

            if (null == name)
            {
                throw new ArgumentNullException("name");
            }

            if (0 == name.Length)
            {
                throw new ArgumentException("Empty variable name.", "name");
            }

            switch (prefix)
            {
                case "env":
                    return Environment.GetEnvironmentVariable(name);
                case "sys":
                    switch (name)
                    {
                        case "CURRENTDIR":
                            return String.Concat(Directory.GetCurrentDirectory(), Path.DirectorySeparatorChar);
                        case "SOURCEFILEDIR":
                            return String.Concat(Path.GetDirectoryName(this.sourceFile), Path.DirectorySeparatorChar);
                        case "SOURCEFILEPATH":
                            return this.sourceFile;
                        default:
                            return null;
                    }
                case "var":
                    return (string)this.variables[name];
                default:
                    PreprocessorExtension extension = (PreprocessorExtension)this.extensionsByPrefix[prefix];
                    if (null != extension)
                    {
                        try
                        {
                            return extension.GetVariableValue(prefix, name);
                        }
                        catch (Exception e)
                        {
                            throw new WixException(WixErrors.PreprocessorExtensionGetVariableValueFailed(sourceLineNumbers, prefix, name, e.Message));
                        }
                    }
                    else
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != errorEventArgs)
            {
                this.encounteredError = true;
            }

            if (null != this.MessageHandler)
            {
                this.MessageHandler(this, mea);
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }

        /// <summary>
        /// Add a variable.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information of the variable.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable value.</param>
        internal void AddVariable(SourceLineNumberCollection sourceLineNumbers, string name, string value)
        {
            string currentValue = this.GetVariableValue(sourceLineNumbers, "var", name);

            if (null != currentValue)
            {
                this.RemoveVariable(sourceLineNumbers, name);
                this.variables.Add(name, value);
            }
            else if (null == currentValue)
            {
                this.variables.Add(name, value);
            }
            else
            {
                throw new WixException(WixErrors.VariableDeclarationCollision(sourceLineNumbers, name, value, currentValue));
            }
        }

        /// <summary>
        /// Remove a variable.
        /// </summary>
        /// <param name="sourceLineNumbers">The source line information of the variable.</param>
        /// <param name="name">The variable name.</param>
        internal void RemoveVariable(SourceLineNumberCollection sourceLineNumbers, string name)
        {
            if (this.variables.Contains(name))
            {
                this.variables.Remove(name);
            }
            else
            {
                throw new WixException(WixErrors.CannotReundefineVariable(sourceLineNumbers, name));
            }
        }
    }
}
