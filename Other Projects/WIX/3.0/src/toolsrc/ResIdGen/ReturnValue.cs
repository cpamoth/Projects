//-------------------------------------------------------------------------------------------------
// <copyright file="ReturnValue.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Enumerates all of the possible return values from the ResIdGen application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Tools
{
    using System;

    /// <summary>
    /// Enumerates all of the possible return values from the ResIdGen application.
    /// </summary>
    public enum ReturnValue
    {
        Success = 0,
        UnknownError,
        InvalidParameters,
        ResourceFileNotFound,
        SourceFileNotFound,
        FileOpenError,
        FileReadWriteError,
        NoStartAutoGenerateTagFound,
        NoEndAutoGenarateTagFound,
    }
}
