//-------------------------------------------------------------------------------------------------
// <copyright file="PreProcExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// A simple preprocessor extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Collections;
    using System.IO;

    /// <summary>
    /// The example preprocessor extension.
    /// </summary>
    public sealed class PreProcExtension : PreprocessorExtension
    {
        /// <summary>
        /// Instantiate a new PreProcExtension.
        /// </summary>
        public PreProcExtension()
        {
        }

        /// <summary>
        /// Gets the variable prefixes for this extension.
        /// </summary>
        /// <value>The variable prefixes for this extension.</value>
        public override string[] Prefixes
        {
            get
            {
                string[] prefixes = new string[1];

                prefixes[0] = "abc";

                return prefixes;
            }
        }

        /// <summary>
        /// Preprocesses a parameter.
        /// </summary>
        /// <param name="name">Name of parameter that matches extension.</param>
        /// <returns>The value of the parameter after processing.</returns>
        /// <remarks>By default this method will cause an error if its called.</remarks>
        public override string PreprocessParameter(string name)
        {
            if ("LcidList" == name)
            {
                return "1033;1041;1055";
            }

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
        public override bool IsForeachVariableAllowed(string variableName)
        {
            // only alow looping on language variables
            if ("LCID" == variableName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
