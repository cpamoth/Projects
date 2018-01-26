//-------------------------------------------------------------------------------------------------
// <copyright file="VotivePPReturnValue.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Enumerates all of the possible return values from the VotivePP application.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio.Tools
{
    using System;

    /// <summary>
    /// Enumerates all of the possible return values from the VotivePP application.
    /// </summary>
    public enum VotivePPReturnValue
    {
        Success = 0,
        UnknownError,
        InvalidParameters,
        InvalidPlaceholderParam,
        SourceFileNotFound,
        FileReadError,
    }
}
