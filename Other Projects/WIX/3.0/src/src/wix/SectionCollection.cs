//-------------------------------------------------------------------------------------------------
// <copyright file="SectionCollection.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Array collection of sections.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    /// <summary>
    /// Array collection of sections.
    /// </summary>
    public sealed class SectionCollection : ICollection
    {
        private ArrayList collection;

        /// <summary>
        /// Instantiate a new SectionCollection class.
        /// </summary>
        public SectionCollection()
        {
            this.collection = new ArrayList();
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        /// <value>Number of items in collection.</value>
        public int Count
        {
            get { return this.collection.Count; }
        }

        /// <summary>
        /// Gets if the collection has been synchronized.
        /// </summary>
        /// <value>True if the collection has been synchronized.</value>
        public bool IsSynchronized
        {
            get { return this.collection.IsSynchronized; }
        }

        /// <summary>
        /// Gets the object used to synchronize the collection.
        /// </summary>
        /// <value>Oject used the synchronize the collection.</value>
        public object SyncRoot
        {
            get { return this.collection.SyncRoot; }
        }

        /// <summary>
        /// Adds a section to the collection.
        /// </summary>
        /// <param name="section">Section to add to collection.</param>
        public void Add(Section section)
        {
            this.collection.Add(section);
        }

        /// <summary>
        /// Adds a SectionCollection to the collection.
        /// </summary>
        /// <param name="sections">SectionCollection to add to collection.</param>
        public void AddRange(SectionCollection sections)
        {
            this.collection.AddRange(sections.collection);
        }

        /// <summary>
        /// Checks if the collection contains a Section.
        /// </summary>
        /// <param name="section">The Section to check in the collection.</param>
        /// <returns>True if collection contains the Section.</returns>
        public bool Contains(Section section)
        {
            return this.collection.Contains(section);
        }

        /// <summary>
        /// Copies the collection into an array.
        /// </summary>
        /// <param name="array">Array to copy the collection into.</param>
        /// <param name="index">Index to start copying from.</param>
        public void CopyTo(System.Array array, int index)
        {
            this.collection.CopyTo(array, index);
        }

        /// <summary>
        /// Gets enumerator for the collection.
        /// </summary>
        /// <returns>Enumerator for the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        /// <summary>
        /// Gets the symbols that were not referenced during processing (usually linking).
        /// </summary>
        /// <param name="referencedSymbols">Collection of symbol names in string form.</param>
        /// <param name="messageHandler">Message handler to display errors while acquiring symbols.</param>
        /// <returns>Collection of unreferenced symbols.</returns>
        internal SymbolCollection GetOrphanedSymbols(StringCollection referencedSymbols, IMessageHandler messageHandler)
        {
            SymbolCollection unreferencedSymbols = new SymbolCollection();

            // Loop through all of the symbols in all of the sections in this collection
            // looking for names that are not in the provided string collection.
            foreach (Section section in this.collection)
            {
                foreach (Symbol symbol in section.GetSymbols(messageHandler))
                {
                    if (!referencedSymbols.Contains(symbol.Name))
                    {
                        // If the symbol was created by the user, then it will have
                        // a row associated with it.  We don't care about generated
                        // (those with out Rows) unreferenced symbols so skip them.
                        if (null != symbol.Row && !unreferencedSymbols.Contains(symbol.Name))
                        {
                            unreferencedSymbols.Add(symbol);
                        }
                    }
                }
            }

            return unreferencedSymbols;
        }

        /// <summary>
        /// Finds the entry section and loads the symbols from an array of intermediates.
        /// </summary>
        /// <param name="allowIdenticalRows">Flag specifying whether identical rows are allowed or not.</param>
        /// <param name="messageHandler">Message handler object to route all errors through.</param>
        /// <param name="entrySection">Located entry section.</param>
        /// <param name="allSymbols">Collection of symbols loaded.</param>
        internal void FindEntrySectionAndLoadSymbols(
            bool allowIdenticalRows,
            IMessageHandler messageHandler,
            out Section entrySection,
            out SymbolCollection allSymbols)
        {
            entrySection = null;
            allSymbols = new SymbolCollection();

            foreach (Section section in this.collection)
            {
                if (SectionType.Product == section.Type || SectionType.Module == section.Type || SectionType.PatchCreation == section.Type || SectionType.Patch == section.Type)
                {
                    if (null == entrySection)
                    {
                        entrySection = section;
                    }
                    else
                    {
                        messageHandler.OnMessage(WixErrors.MultipleEntrySections(entrySection.SourceLineNumbers, entrySection.Id, section.Id));
                        messageHandler.OnMessage(WixErrors.MultipleEntrySections2(section.SourceLineNumbers));
                    }
                }

                foreach (Symbol symbol in section.GetSymbols(messageHandler))
                {
                    try
                    {
                        Symbol existingSymbol = allSymbols[symbol.Name];
                        if (null == existingSymbol)
                        {
                            allSymbols.Add(symbol);
                        }
                        else if (allowIdenticalRows && existingSymbol.Row.IsIdentical(symbol.Row))
                        {
                            messageHandler.OnMessage(WixWarnings.IdenticalRowWarning(symbol.Row.SourceLineNumbers, existingSymbol.Name));
                            messageHandler.OnMessage(WixWarnings.IdenticalRowWarning2(existingSymbol.Row.SourceLineNumbers));
                        }
                        else
                        {
                            allSymbols.AddDuplicate(symbol);
                        }
                    }
                    catch (DuplicateSymbolsException)
                    {
                        // if there is already a duplicate symbol, just
                        // another to the list, don't bother trying to
                        // see if there are any identical symbols
                        allSymbols.AddDuplicate(symbol);
                    }
                }
            }
        }
    }
}
