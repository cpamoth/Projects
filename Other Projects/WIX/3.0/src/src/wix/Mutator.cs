//-------------------------------------------------------------------------------------------------
// <copyright file="Mutator.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The Windows Installer XML Toolset mutator.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The Windows Installer XML Toolset mutator.
    /// </summary>
    public sealed class Mutator
    {
        private MutatorCore core;
        private SortedList extensions;

        /// <summary>
        /// Instantiate a new mutator.
        /// </summary>
        public Mutator()
        {
            this.extensions = new SortedList();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Adds a mutator extension.
        /// </summary>
        /// <param name="mutatorExtension">The mutator extension to add.</param>
        public void AddExtension(MutatorExtension mutatorExtension)
        {
            this.extensions.Add(mutatorExtension.Sequence, mutatorExtension);
        }

        /// <summary>
        /// Mutate a WiX document.
        /// </summary>
        /// <param name="wix">The Wix document element.</param>
        /// <returns>true if mutation was successful</returns>
        public bool Mutate(Wix.Wix wix)
        {
            bool encounteredError = false;

            try
            {
                // create a new core
                this.core = new MutatorCore(this.Message);

                foreach (MutatorExtension mutatorExtension in this.extensions.Values)
                {
                    mutatorExtension.Mutate(wix);
                }
            }
            finally
            {
                encounteredError = this.core.EncounteredError;

                this.core = null;
                foreach (MutatorExtension mutatorExtension in this.extensions.Values)
                {
                    mutatorExtension.Core = null;
                }
            }

            // return the Wix document element only if mutation completed successfully
            return !encounteredError;
        }
    }
}
