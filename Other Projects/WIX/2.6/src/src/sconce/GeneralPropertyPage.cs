//-------------------------------------------------------------------------------------------------
// <copyright file="GeneralPropertyPage.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the GeneralPropertyPage class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudioInfrastructure
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents the default "General" property page for a project.
    /// </summary>
    [Guid("85550E99-05E7-4778-BD7D-576FC334D522")]
    public class GeneralPropertyPage : PropertyPage
    {
        #region Member Variables
        //==========================================================================================
        // Member Variables
        //==========================================================================================

        private BuildSettings clonedBoundObject;
        #endregion

        #region Constructors
        //==========================================================================================
        // Constructors
        //==========================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralPropertyPage"/> class.
        /// </summary>
        public GeneralPropertyPage()
            : base(SconceStrings.PropertyPageGeneral)
        {
        }
        #endregion

        #region Properties
        //==========================================================================================
        // Properties
        //==========================================================================================

        /// <summary>
        /// Gets the object that is bound to this property page.
        /// </summary>
        protected override PropertyPageSettings BoundObject
        {
            get
            {
                if (this.clonedBoundObject == null && this.Project != null)
                {
                    this.clonedBoundObject = this.Project.BuildSettings.Clone() as BuildSettings;
                    this.clonedBoundObject.ClearDirty();
                }

                return this.clonedBoundObject;
            }
        }
        #endregion

        #region Methods
        //==========================================================================================
        // Methods
        //==========================================================================================

        /// <summary>
        /// Applies the changes made on the property page to the bound objects.
        /// </summary>
        /// <returns>
        /// true if the changes were successfully applied and the property page is current with the bound objects;
        /// false if the changes were applied, but the property page cannot determine if its state is current with the objects.
        /// </returns>
        protected override bool ApplyChanges()
        {
            if (this.clonedBoundObject == null)
            {
                return false;
            }

            // Unbind from the cloned object
            this.UnbindObject();

            // Apply the changes on the cloned object back to the real object
            this.Project.BuildSettings = this.clonedBoundObject;

            // Reclone the object by setting it to null (it will be recloned in get_BoundObject)
            this.clonedBoundObject = null;
            this.BindObject();

            return true;
        }
        #endregion
    }
}
