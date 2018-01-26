//-------------------------------------------------------------------------------------------------
// <copyright file="MutatorExtension.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// The base mutator extension.  Any of these methods can be overridden to change
// the behavior of the mutator.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Text;

    using Wix = Microsoft.Tools.WindowsInstallerXml.Serialize;

    /// <summary>
    /// The base mutator extension.  Any of these methods can be overridden to change
    /// the behavior of the mutator.
    /// </summary>
    public abstract class MutatorExtension
    {
        private MutatorCore core;

        /// <summary>
        /// Gets or sets the mutator core for the extension.
        /// </summary>
        /// <value>The mutator core for the extension.</value>
        public MutatorCore Core
        {
            get { return this.core; }
            set { this.core = value; }
        }

        /// <summary>
        /// Gets the sequence of the extension.
        /// </summary>
        /// <value>The sequence of the extension.</value>
        public abstract int Sequence
        {
            get;
        }

        /// <summary>
        /// Mutate a WiX document.
        /// </summary>
        /// <param name="wix">The Wix document element.</param>
        public abstract void Mutate(Wix.Wix wix);

        /// <summary>
        /// Generate unique MSI identifiers.
        /// </summary>
        protected class IdentifierGenerator
        {
            private string baseName;
            private Hashtable existingIdentifiers;
            private Hashtable possibleIdentifiers;

            /// <summary>
            /// Instantiate a new IdentifierGenerator.
            /// </summary>
            /// <param name="baseName">The base resource name to use if a resource name contains no usable characters.</param>
            public IdentifierGenerator(string baseName)
            {
                this.baseName = baseName;
                this.existingIdentifiers = new Hashtable();
                this.possibleIdentifiers = new Hashtable();
            }

            /// <summary>
            /// Index an existing identifier for collision detection.
            /// </summary>
            /// <param name="identifier">The identifier.</param>
            public void IndexExistingIdentifier(string identifier)
            {
                if (null == identifier)
                {
                    throw new ArgumentNullException("identifier");
                }

                this.existingIdentifiers[identifier] = null;
            }

            /// <summary>
            /// Index a resource name for collision detection.
            /// </summary>
            /// <param name="name">The resource name.</param>
            public void IndexName(string name)
            {
                if (null == name)
                {
                    throw new ArgumentNullException("name");
                }

                string identifier = this.CreateIdentifier(name, 0);

                if (this.possibleIdentifiers.Contains(identifier))
                {
                    this.possibleIdentifiers[identifier] = String.Empty;
                }
                else
                {
                    this.possibleIdentifiers.Add(identifier, null);
                }
            }

            /// <summary>
            /// Get the identifier for the given resource name.
            /// </summary>
            /// <param name="name">The resource name.</param>
            /// <returns>A legal MSI identifier.</returns>
            public string GetIdentifier(string name)
            {
                for (int i = 0; i <= int.MaxValue; i++)
                {
                    string identifier = this.CreateIdentifier(name, i);

                    if (this.existingIdentifiers.Contains(identifier) || // already used
                        (0 == i && null != this.possibleIdentifiers[identifier]) || // needs an index because its duplicated
                        (0 != i && this.possibleIdentifiers.Contains(identifier))) // collides with another possible identifier
                    {
                        continue;
                    }
                    else // use this identifier
                    {
                        this.existingIdentifiers.Add(identifier, null);

                        return identifier;
                    }
                }

                throw new InvalidOperationException("Could not find a unique identifier for the given resource name.");
            }

            /// <summary>
            /// Create a legal MSI identifier from a resource name and an index.
            /// </summary>
            /// <param name="name">The name of the resource for which an identifier should be created.</param>
            /// <param name="index">An index to append to the end of the identifier to make it unique.</param>
            /// <returns>A legal MSI identifier.</returns>
            private string CreateIdentifier(string name, int index)
            {
                bool foundFirstChar = false;
                StringBuilder identifier = new StringBuilder();

                foreach (char character in name)
                {
                    if ('_' == character || Char.IsLetter(character))
                    {
                        identifier.Append(character);
                        foundFirstChar = true;
                    }
                    else if ('.' == character || Char.IsNumber(character))
                    {
                        // could be an entirely numeric name, so add the base name to avoid losing real resource characters
                        if (!foundFirstChar)
                        {
                            identifier.Append(this.baseName);
                            foundFirstChar = true;
                        }

                        identifier.Append(character);
                    }
                }

                // no legal identifier characters were found, use the base id instead
                if (0 == identifier.Length)
                {
                    identifier.Append(this.baseName);
                }

                // truncate the identifier to 69 characters (reserve 3 characters for up to 99 collisions)
                if (69 < identifier.Length)
                {
                    identifier.Length = 69;
                }

                // if the index is not zero, then append it to the identifier name
                if (0 != index)
                {
                    identifier.AppendFormat("_{0}", index);
                }

                return identifier.ToString();
            }
        }
    }
}
