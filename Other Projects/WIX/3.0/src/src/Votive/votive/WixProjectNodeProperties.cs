//-------------------------------------------------------------------------------------------------
// <copyright file="WixProjectNodeProperties.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixProjectNodeProperties class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Package;

    /// <summary>
    /// Represents the root node of a WiX project within a Solution Explorer hierarchy.
    /// </summary>
    [ComVisible(true)]
    [Guid("CC565F35-2526-4426-BF53-35620AAB1DCD")]
    public class WixProjectNodeProperties : ProjectNodeProperties
    {
        // =========================================================================================
        // Constructors
        // =========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="WixProjectNodeProperties"/> class.
        /// </summary>
        /// <param name="node">The <see cref="WixProjectNode"/> from which the properties are read.</param>
        public WixProjectNodeProperties(WixProjectNode node)
            : base(node)
        {
        }

        // =========================================================================================
        // Methods
        // =========================================================================================

        /// <summary>
        /// Creates a custom property descriptor for the node properties, which affects the behavior
        /// of the property grid.
        /// </summary>
        /// <param name="p">The <see cref="PropertyDescriptor"/> to wrap.</param>
        /// <returns>A custom <see cref="PropertyDescriptor"/> object.</returns>
        public override DesignPropertyDescriptor CreateDesignPropertyDescriptor(PropertyDescriptor p)
        {
            return new WixDesignPropertyDescriptor(p);
        }
    }
}
