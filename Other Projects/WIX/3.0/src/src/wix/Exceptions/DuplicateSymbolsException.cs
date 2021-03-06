//-------------------------------------------------------------------------------------------------
// <copyright file="DuplicateSymbolsException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Duplicate symbols exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;

    /// <summary>
    /// Duplicate symbols exception.
    /// </summary>
    [Serializable]
    public sealed class DuplicateSymbolsException : Exception
    {
        [NonSerialized]
        private Symbol[] duplicateSymbols;

        /// <summary>
        /// Instantiate a new DuplicateSymbolException.
        /// </summary>
        /// <param name="symbols">The duplicated symbols.</param>
        public DuplicateSymbolsException(ArrayList symbols)
        {
            this.duplicateSymbols = (Symbol[])symbols.ToArray(typeof(Symbol));
        }

        /// <summary>
        /// Gets the duplicate symbols.
        /// </summary>
        /// <returns>List of duplicate symbols.</returns>
        public Symbol[] GetDuplicateSymbols()
        {
            return this.duplicateSymbols;
        }
    }
}

