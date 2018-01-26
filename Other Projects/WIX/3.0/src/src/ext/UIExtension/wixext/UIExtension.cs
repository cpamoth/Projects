//-------------------------------------------------------------------------------------------------
// <copyright file="UIExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset UI Extension.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Extensions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The Windows Installer XML Toolset UI Extension.
    /// </summary>
    public sealed class UIExtension : WixExtension
    {
        private UIDecompiler decompilerExtension;
        private Library library;

        /// <summary>
        /// Gets the optional decompiler extension.
        /// </summary>
        /// <value>The optional decompiler extension.</value>
        public override DecompilerExtension DecompilerExtension
        {
            get
            {
                if (null == this.decompilerExtension)
                {
                    this.decompilerExtension = new UIDecompiler();
                }

                return this.decompilerExtension;
            }
        }

        /// <summary>
        /// Gets the library associated with this extension.
        /// </summary>
        /// <param name="tableDefinitions">The table definitions to use while loading the library.</param>
        /// <returns>The library for this extension.</returns>
        public override Library GetLibrary(TableDefinitionCollection tableDefinitions)
        {
            if (null == this.library)
            {
                this.library = LoadLibraryHelper(Assembly.GetExecutingAssembly(), "Microsoft.Tools.WindowsInstallerXml.Extensions.Data.ui.wixlib", tableDefinitions);
            }

            return this.library;
        }
    }
}
