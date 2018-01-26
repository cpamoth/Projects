//-------------------------------------------------------------------------------------------------
// <copyright file="WixInvalidIdtException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// WiX invalid idt exception.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;

    /// <summary>
    /// WiX invalid idt exception.
    /// </summary>
    [Serializable]
    public sealed class WixInvalidIdtException : WixException
    {
        /// <summary>
        /// Instantiate a new WixInvalidIdtException.
        /// </summary>
        /// <param name="idtFile">The invalid idt file.</param>
        /// <param name="tableName">The table name of the invalid idt file.</param>
        public WixInvalidIdtException(string idtFile, string tableName) :
            base(WixErrors.InvalidIdt(SourceLineNumberCollection.FromFileName(idtFile), tableName))
        {
        }
    }
}
